
#pragma warning disable
namespace InternetBankCalculator.Pages.About;

public sealed partial class CloudBankingPage : Page
{
    public CloudBankingPage()
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
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
            e.Handled = true;
        }
    }
#endif
}
