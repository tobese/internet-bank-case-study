using InternetBankCalculator.Services;
using InternetBankCalculator.ViewModels;
using Microsoft.UI.Xaml.Input;

namespace InternetBankCalculator.Pages;

public sealed partial class CalculatorPage : Page
{
    private readonly MathApiService _api = new(GetApiBaseUrl());

    public CalculatorPage()
    {
        this.InitializeComponent();
        DataContext = new CalculatorPageViewModel(_api);
#if !WINDOWS
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
#endif
    }

#if !WINDOWS
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
    }
    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
    }
    private void OnBackRequested(object? sender, Windows.UI.Core.BackRequestedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
            e.Handled = true;
        }
    }
#endif

    private void ExpressionInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            if (DataContext is CalculatorPageViewModel vm)
            {
                vm.Calculate.Execute(null);
            }
        }
    }

    public static string GetApiBaseUrl()
    {
#if __WASM__
        // Use a fixed API URL for local debugging, otherwise use origin for production/container.
        var origin = Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.location.origin");
        if (origin.Contains("localhost") || origin.Contains("127.0.0.1"))
        {
            // Use the backend API port for local dev
            return "http://localhost:8282";
        }
        return origin;
#else
        // Android emulator: use 10.0.2.2 instead of localhost.
        return "http://localhost:8282";
#endif
    }
}
