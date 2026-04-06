namespace InternetBankCalculator;

public sealed partial class SystemDesignPage : Page
{
    public SystemDesignPage()
    {
        this.InitializeComponent();
    }

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
