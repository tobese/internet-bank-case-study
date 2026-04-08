namespace InternetBankCalculator;

public sealed partial class AboutPage : Page
{
    public AboutPage()
    {
        this.InitializeComponent();
    }

    private static AppShell? GetShell()
    {
        if (App.Current is App app && app.MainWindow?.Content is Frame f && f.Content is AppShell shell)
            return shell;
        return null;
    }

    private void MainframeModernization_Click(object sender, RoutedEventArgs e)
    {
        var shell = GetShell();
        if (shell != null)
        {
            shell.ContentFramePublic.Navigate(typeof(MainframeModernizationPage));
            shell.UpdateNavHighlight(shell.BtnAboutPublic);
        }
    }

    private void CloudBanking_Click(object sender, RoutedEventArgs e)
    {
        var shell = GetShell();
        if (shell != null)
        {
            shell.ContentFramePublic.Navigate(typeof(CloudBankingPage));
            shell.UpdateNavHighlight(shell.BtnAboutPublic);
        }
    }


    private void DoggerBank_Click(object sender, RoutedEventArgs e)
    {
        if (this.Frame != null)
        {
            this.Frame.Navigate(typeof(DoggerBankPage));
        }
    }

    private void DoggerLand_Click(object sender, RoutedEventArgs e)
    {
        if (this.Frame != null)
        {
            this.Frame.Navigate(typeof(DoggerLandPage));
        }
    }
}
