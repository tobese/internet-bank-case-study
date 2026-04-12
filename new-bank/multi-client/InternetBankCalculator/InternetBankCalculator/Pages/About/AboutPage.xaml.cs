
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
        NavigationService.Instance.NavigateInShell(
            typeof(Pages.About.FromJavaToCSharpPage),
            shell => shell.UpdateNavHighlight(shell.BtnAboutPublic));
    }

    // NavigationService handles shell access

    private void MainframeModernization_Click(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.NavigateInShell(
            typeof(Pages.About.MainframeModernizationPage),
            shell => shell.UpdateNavHighlight(shell.BtnAboutPublic));
    }

    private void CloudBanking_Click(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.NavigateInShell(
            typeof(Pages.About.CloudBankingPage),
            shell => shell.UpdateNavHighlight(shell.BtnAboutPublic));
    }

    private void CobolToModern_Click(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.NavigateInShell(
            typeof(Pages.About.CobolToModernPage),
            shell => shell.UpdateNavHighlight(shell.BtnAboutPublic));
    }

    private void DoggerBank_Click(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.NavigateInShell(
            typeof(Pages.About.DoggerBankPage),
            shell => shell.UpdateNavHighlight(shell.BtnAboutPublic));
    }

    private void DoggerLand_Click(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.NavigateInShell(
            typeof(Pages.About.DoggerLandPage),
            shell => shell.UpdateNavHighlight(shell.BtnAboutPublic));
    }
}
