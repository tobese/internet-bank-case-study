using InternetBankCalculator.Services;
using InternetBankCalculator.ViewModels;
using Microsoft.UI.Xaml.Input;

namespace InternetBankCalculator;

public sealed partial class MainPage : Page
{
    private readonly MathApiService _api = new(GetApiBaseUrl());

    public MainPage()
    {
        this.InitializeComponent();
        DataContext = new MainPageViewModel(_api);
    }

    private void ExpressionInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            if (DataContext is MainPageViewModel vm)
            {
                vm.Calculate.Execute(null);
            }
        }
    }

    private static string GetApiBaseUrl()
    {
#if __WASM__
        // When served from a container (nginx), /api/* is reverse-proxied to
        // the backend API service, so we use the current page's origin.
        return Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.location.origin");
#else
        // Android emulator: use 10.0.2.2 instead of localhost.
        return "http://localhost:8282";
#endif
    }
}
