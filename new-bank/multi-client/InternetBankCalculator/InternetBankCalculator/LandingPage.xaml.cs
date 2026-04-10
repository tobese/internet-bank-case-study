namespace InternetBankCalculator;

public sealed partial class LandingPage : Page
{
    public LandingPage()
    {
        this.InitializeComponent();
    }

    private void BtnToCalc_Click(object sender, RoutedEventArgs e)
    {
        if (App.Current is App app && app.MainWindow?.Content is AppShell shell)
        {
            shell.NavigateToCalc();
        }
    }
}
