
#pragma warning disable
namespace InternetBankCalculator.AboutContent;

public sealed partial class AboutPage : Page
{
    public AboutPage()
    {
        this.InitializeComponent();
    }

    private void FromJavaToCSharp_Click(object sender, RoutedEventArgs e)
    {
        if (this.Frame != null)
        {
            this.Frame.Navigate(typeof(AboutContent.FromJavaToCSharpPage));
        }
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
            shell.ContentFramePublic.Navigate(typeof(AboutContent.MainframeModernizationPage));
            shell.UpdateNavHighlight(shell.BtnAboutPublic);
        }
    }

    private void CloudBanking_Click(object sender, RoutedEventArgs e)
    {
        var shell = GetShell();
        if (shell != null)
        {
            shell.ContentFramePublic.Navigate(typeof(AboutContent.CloudBankingPage));
            shell.UpdateNavHighlight(shell.BtnAboutPublic);
        }
    }

    private void CobolToModern_Click(object sender, RoutedEventArgs e)
    {
        var shell = GetShell();
        if (shell != null)
        {
            shell.ContentFramePublic.Navigate(typeof(AboutContent.CobolToModernPage));
            shell.UpdateNavHighlight(shell.BtnAboutPublic);
        }
    }

    private void DoggerBank_Click(object sender, RoutedEventArgs e)
    {
        if (this.Frame != null)
        {
            this.Frame.Navigate(typeof(AboutContent.DoggerBankPage));
        }
    }

    private void DoggerLand_Click(object sender, RoutedEventArgs e)
    {
        if (this.Frame != null)
        {
            this.Frame.Navigate(typeof(AboutContent.DoggerLandPage));
        }
    }
}
