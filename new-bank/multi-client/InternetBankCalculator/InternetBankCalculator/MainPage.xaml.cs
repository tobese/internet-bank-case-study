using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InternetBankCalculator.Services;
using Microsoft.UI.Xaml.Input;

namespace InternetBankCalculator;

public sealed partial class MainPage : Page
{
    // TODO: Update this URL to point at your running API instance.
    // Android emulator: use 10.0.2.2 instead of localhost.
    // WASM: use the host the WASM app is served from, or a CORS-enabled API URL.
    private const string ApiBaseUrl = "http://localhost:8282";

    private readonly MathApiService _api = new(ApiBaseUrl);
    private readonly ObservableCollection<string> _history = new();

    public MainPage()
    {
        this.InitializeComponent();
        HistoryList.ItemsSource = _history;
    }

    // ── Result display helpers ──────────────────────────

    private void ShowResult(string text, bool isError = false)
    {
        ResultText.Text = text;
        ResultBorder.Background = isError
            ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 253, 236, 234))   // #FDECEA
            : new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 240, 244, 248));  // #F0F4F8
        ResultText.Foreground = isError
            ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 198, 40, 40))     // #C62828
            : new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 26, 26, 26));     // #1A1A1A
    }

    private void AddHistory(string expr, string result)
    {
        _history.Insert(0, $"{expr}  =  {result}");
    }

    // ── Local basic arithmetic ─────────────────────────

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

    // ── Evaluate expression ────────────────────────────

    private async Task<string> EvaluateAsync(string input)
    {
        var trimmed = input.Trim().ToLowerInvariant();

        // fac(n) or fib(n) via API
        var fnMatch = Regex.Match(trimmed, @"^(fac|fib)\((\d+)\)$");
        if (fnMatch.Success)
        {
            var fn = fnMatch.Groups[1].Value;
            var n = int.Parse(fnMatch.Groups[2].Value);
            return fn == "fac"
                ? await _api.GetFactorialAsync(n)
                : await _api.GetFibonacciAsync(n);
        }

        // Local arithmetic
        var result = BasicCalc(trimmed);
        if (result != null) return result;

        throw new Exception("Unknown expression. Try \"3 + 4\", \"fac(5)\", or \"fib(10)\".");
    }

    // ── Event handlers ─────────────────────────────────

    private async void BtnCalc_Click(object sender, RoutedEventArgs e)
    {
        await HandleSubmitAsync();
    }

    private async void BtnFac_Click(object sender, RoutedEventArgs e)
    {
        var n = ExpressionInput.Text.Trim();
        if (!Regex.IsMatch(n, @"^\d+$"))
        {
            ShowResult("Enter a number for fac(n)", isError: true);
            return;
        }
        try
        {
            var result = await _api.GetFactorialAsync(int.Parse(n));
            ShowResult(result);
            AddHistory($"fac({n})", result);
        }
        catch (Exception ex) { ShowResult(ex.Message, isError: true); }
    }

    private async void BtnFib_Click(object sender, RoutedEventArgs e)
    {
        var n = ExpressionInput.Text.Trim();
        if (!Regex.IsMatch(n, @"^\d+$"))
        {
            ShowResult("Enter a number for fib(n)", isError: true);
            return;
        }
        try
        {
            var result = await _api.GetFibonacciAsync(int.Parse(n));
            ShowResult(result);
            AddHistory($"fib({n})", result);
        }
        catch (Exception ex) { ShowResult(ex.Message, isError: true); }
    }

    private async void ExpressionInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            await HandleSubmitAsync();
        }
    }

    private async Task HandleSubmitAsync()
    {
        var input = ExpressionInput.Text.Trim();
        if (string.IsNullOrEmpty(input)) return;
        try
        {
            var result = await EvaluateAsync(input);
            ShowResult(result);
            AddHistory(input, result);
        }
        catch (Exception ex)
        {
            ShowResult(ex.Message, isError: true);
        }
    }
}
