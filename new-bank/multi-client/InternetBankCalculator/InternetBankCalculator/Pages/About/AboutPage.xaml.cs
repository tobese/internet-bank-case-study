
#pragma warning disable
namespace InternetBankCalculator.Pages.About;

public sealed partial class AboutPage : Page
{
    public AboutPage()
    {
        this.InitializeComponent();
    }

    private void FromJavaToCSharp_Click(object sender, RoutedEventArgs e)
    {
        var shell = GetShell();
        if (shell != null)
        {
            shell.ContentFramePublic.Navigate(typeof(Pages.About.FromJavaToCSharpPage));
            shell.UpdateNavHighlight(shell.BtnAboutPublic);
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
            shell.ContentFramePublic.Navigate(typeof(Pages.About.MainframeModernizationPage));
            shell.UpdateNavHighlight(shell.BtnAboutPublic);
        }
    }

    private void CloudBanking_Click(object sender, RoutedEventArgs e)
    {
        var shell = GetShell();
        if (shell != null)
        {
            shell.ContentFramePublic.Navigate(typeof(Pages.About.CloudBankingPage));
            shell.UpdateNavHighlight(shell.BtnAboutPublic);
        }
    }

    private void CobolToModern_Click(object sender, RoutedEventArgs e)
    {
        var shell = GetShell();
        if (shell != null)
        {
            shell.ContentFramePublic.Navigate(typeof(Pages.About.CobolToModernPage));
            shell.UpdateNavHighlight(shell.BtnAboutPublic);
        }
    }

    private void DoggerBank_Click(object sender, RoutedEventArgs e)
    {
        var shell = GetShell();
        if (shell != null)
        {
            shell.ContentFramePublic.Navigate(typeof(Pages.About.DoggerBankPage));
            shell.UpdateNavHighlight(shell.BtnAboutPublic);
        }
    }

    private void DoggerLand_Click(object sender, RoutedEventArgs e)
    {
        var shell = GetShell();
        if (shell != null)
        {
            shell.ContentFramePublic.Navigate(typeof(Pages.About.DoggerLandPage));
            shell.UpdateNavHighlight(shell.BtnAboutPublic);
        }
    }
}
