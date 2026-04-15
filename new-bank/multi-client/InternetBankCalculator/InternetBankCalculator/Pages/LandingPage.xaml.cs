namespace InternetBankCalculator.Pages;

public sealed partial class LandingPage : Page
{
    public LandingPage()
    {
        InitializeComponent();
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

    private void BtnToCalc_Click(object sender, RoutedEventArgs e)
    {
        NavigationService.Instance.NavigateInShell(
            typeof(Pages.CalculatorPage),
            shell => shell.UpdateNavHighlight(shell.BtnCalc));
    }
}
