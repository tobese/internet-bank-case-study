namespace InternetBankCalculator;

public sealed partial class LandingPage : Page
{
    public LandingPage()
    {
        this.InitializeComponent();
    }

    private void BtnToCalc_Click(object sender, RoutedEventArgs e) =>
        GetShell()?.NavigateToCalc();

    private static AppShell? GetShell()
    {
        if (App.Current is App app && app.MainWindow?.Content is Frame f && f.Content is AppShell shell)
            return shell;
        return null;
    }
}
