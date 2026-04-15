namespace InternetBankCalculator.Pages;

public sealed partial class SystemDesignPage : Page
{
    public SystemDesignPage()
    {
        this.InitializeComponent();
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

    private void BtnGrafana_Click(object sender, RoutedEventArgs e)
    {
#if __WASM__
        Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.open('http://localhost:3000', '_blank')");
#endif
    }

    private void BtnPrometheus_Click(object sender, RoutedEventArgs e)
    {
#if __WASM__
        Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.open('http://localhost:9090', '_blank')");
#endif
    }

    private void BtnGitHubActions_Click(object sender, RoutedEventArgs e)
    {
#if __WASM__
        Uno.Foundation.WebAssemblyRuntime.InvokeJS("window.open('https://github.com/tobese/internet-bank-case-study/actions', '_blank')");
#endif
    }
}
