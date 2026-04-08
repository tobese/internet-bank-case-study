using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using InternetBankCalculator.Models;
using InternetBankCalculator.Services;

namespace InternetBankCalculator.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly MathApiService _api;

    private string _expression = string.Empty;
    private string _result = "Enter an expression above";
    private bool _isError = false;
    private string _primesResult = "Enter a limit and click Primes(n)";
    private MathematicianResponse? _dailyMathematician;
    private bool _isLoading;
    private bool _isBusy;
    private bool _isPrimesRunning;

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainPageViewModel(MathApiService api)
    {
        _api = api;
        Calculate = new AsyncCommand(ExecuteCalculateAsync);
        CalculateFactorial = new AsyncCommand(ExecuteFactorialAsync);
        CalculateFibonacci = new AsyncCommand(ExecuteFibonacciAsync);
        StreamPrimes = new AsyncCommand(ExecuteStreamPrimesAsync);
        _ = LoadDailyMathematicianAsync();
    }

    public string Expression
    {
        get => _expression;
        set { _expression = value; OnPropertyChanged(); }
    }

    public string Result
    {
        get => _result;
        private set { _result = value; OnPropertyChanged(); }
    }

    public bool IsError
    {
        get => _isError;
        private set { _isError = value; OnPropertyChanged(); }
    }

    public string PrimesResult
    {
        get => _primesResult;
        private set { _primesResult = value; OnPropertyChanged(); }
    }

    public MathematicianResponse? DailyMathematician
    {
        get => _dailyMathematician;
        private set { _dailyMathematician = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set { _isLoading = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsInputEnabled)); }
    }

    public bool IsPrimesRunning
    {
        get => _isPrimesRunning;
        private set { _isPrimesRunning = value; OnPropertyChanged(); }
    }

    public bool IsInputEnabled => !_isBusy;

    public ICommand Calculate { get; }
    public ICommand CalculateFactorial { get; }
    public ICommand CalculateFibonacci { get; }
    public ICommand StreamPrimes { get; }

    public ObservableCollection<string> History { get; } = new();

    private async Task LoadDailyMathematicianAsync()
    {
        try { DailyMathematician = await _api.GetDailyMathematicianAsync(); }
        catch { /* footer stays empty on failure */ }
    }

    private async Task ExecuteCalculateAsync()
    {
        IsLoading = true;
        IsBusy = true;
        Result = "Calculating\u2026";
        IsError = false;
        try
        {
            var result = await EvaluateAsync(Expression, CancellationToken.None);
            Result = result;
            IsError = false;
            AddToHistory(Expression, result);
        }
        catch (Exception ex)
        {
            Result = ex.Message;
            IsError = true;
        }
        finally { IsLoading = false; IsBusy = false; }
    }

    private async Task ExecuteFactorialAsync()
    {
        if (!Regex.IsMatch(Expression ?? string.Empty, @"^\d+$"))
        {
            Result = "Enter a number for fac(n)";
            IsError = true;
            return;
        }
        IsLoading = true;
        IsBusy = true;
        Result = "Calculating\u2026";
        IsError = false;
        try
        {
            var result = await _api.GetFactorialAsync(int.Parse(Expression!), CancellationToken.None);
            Result = result;
            IsError = false;
            AddToHistory($"fac({Expression})", result);
        }
        catch (Exception ex)
        {
            Result = ex.Message;
            IsError = true;
        }
        finally { IsLoading = false; IsBusy = false; }
    }

    private async Task ExecuteFibonacciAsync()
    {
        if (!Regex.IsMatch(Expression ?? string.Empty, @"^\d+$"))
        {
            Result = "Enter a number for fib(n)";
            IsError = true;
            return;
        }
        IsLoading = true;
        IsBusy = true;
        Result = "Calculating\u2026";
        IsError = false;
        try
        {
            var result = await _api.GetFibonacciAsync(int.Parse(Expression!), CancellationToken.None);
            Result = result;
            IsError = false;
            AddToHistory($"fib({Expression})", result);
        }
        catch (Exception ex)
        {
            Result = ex.Message;
            IsError = true;
        }
        finally { IsLoading = false; IsBusy = false; }
    }

    private async Task ExecuteStreamPrimesAsync()
    {
        if (!Regex.IsMatch(Expression ?? string.Empty, @"^\d+$"))
        {
            PrimesResult = "Enter a number for primes(n)";
            return;
        }
        var limit = int.Parse(Expression!);
        if (limit < 2 || limit > 10_000)
        {
            PrimesResult = "Limit must be between 2 and 10000";
            return;
        }
        IsBusy = true;
        IsPrimesRunning = true;
        PrimesResult = string.Empty;
        var primes = new List<int>();
        try
        {
            await foreach (var prime in _api.StreamPrimesAsync(limit))
            {
                primes.Add(prime);
                PrimesResult = string.Join(", ", primes);
            }
            AddToHistory($"primes({limit})", $"{primes.Count} primes found");
        }
        catch (Exception ex)
        {
            PrimesResult = $"Error: {ex.Message}";
        }
        finally { IsBusy = false; IsPrimesRunning = false; }
    }

    private void AddToHistory(string expr, string result) =>
        History.Insert(0, $"{expr} = {result}");

    private async Task<string> EvaluateAsync(string input, CancellationToken ct)
    {
        var trimmed = input.Trim().ToLowerInvariant();

        var fnMatch = Regex.Match(trimmed, @"^(fac|fib)\((\d+)\)$");
        if (fnMatch.Success)
        {
            var fn = fnMatch.Groups[1].Value;
            var n = int.Parse(fnMatch.Groups[2].Value);
            return fn == "fac"
                ? await _api.GetFactorialAsync(n, ct)
                : await _api.GetFibonacciAsync(n, ct);
        }

        var result = BasicCalc(trimmed);
        if (result != null) return result;

        throw new Exception("Unknown expression. Try \"3 + 4\", \"fac(5)\", or \"fib(10)\".");
    }

    private static readonly Regex ArithmeticPattern =
        new(@"^\s*(-?\d+(?:\.\d+)?)\s*([+\-*/])\s*(-?\d+(?:\.\d+)?)\s*$");

    private static string? BasicCalc(string expression)
    {
        var m = ArithmeticPattern.Match(expression);
        if (!m.Success) return null;

        var a = double.Parse(m.Groups[1].Value);
        var op = m.Groups[2].Value;
        var b = double.Parse(m.Groups[3].Value);

        return op switch
        {
            "+" => (a + b).ToString(),
            "-" => (a - b).ToString(),
            "*" => (a * b).ToString(),
            "/" => b == 0 ? "Error: division by zero" : (a / b).ToString(),
            _ => null
        };
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

internal sealed class AsyncCommand : ICommand
{
    private readonly Func<Task> _execute;
    private bool _isExecuting;

    public AsyncCommand(Func<Task> execute) => _execute = execute;

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => !_isExecuting;

    public async void Execute(object? parameter)
    {
        if (_isExecuting) return;
        _isExecuting = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try { await _execute(); }
        finally
        {
            _isExecuting = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}