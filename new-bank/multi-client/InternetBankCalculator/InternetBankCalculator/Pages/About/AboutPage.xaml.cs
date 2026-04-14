
#pragma warning disable
namespace InternetBankCalculator.Pages.About;

public sealed partial class AboutPage : Page
{
    public AboutPage()
    {
        this.InitializeComponent();
#if !WINDOWS
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
#endif
    }

#if !WINDOWS
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
    }
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
    }
    private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
    {
        // Always navigate back to LandingPage instead of Frame.GoBack
        NavigationService.Instance.NavigateInShell(
            typeof(Pages.LandingPage),
            shell => shell.UpdateNavHighlight(shell.BtnHome));
        e.Handled = true;
    }
#endif

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
