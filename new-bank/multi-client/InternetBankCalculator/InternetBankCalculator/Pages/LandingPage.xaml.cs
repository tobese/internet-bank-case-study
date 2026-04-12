namespace InternetBankCalculator.Pages;

public sealed partial class LandingPage : Page
{
    public LandingPage()
    {
        InitializeComponent();
    }

    private void BtnToCalc_Click(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.NavigateInShell(
            typeof(Pages.CalculatorPage),
            shell => shell.UpdateNavHighlight(shell.BtnCalc));
    }
}
