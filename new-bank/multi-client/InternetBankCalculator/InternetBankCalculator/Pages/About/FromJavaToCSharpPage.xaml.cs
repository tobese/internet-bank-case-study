
#pragma warning disable
namespace InternetBankCalculator.Pages.About;

public sealed partial class FromJavaToCSharpPage : Page
{
    public FromJavaToCSharpPage()
    {
        this.InitializeComponent();
#if WINDOWS_UWP || HAS_UNO_WINUI
        var navManager = Windows.UI.Core.SystemNavigationManager.GetForCurrentView();
        navManager.BackRequested += OnBackRequested;
#endif
    }

#if WINDOWS_UWP || HAS_UNO_WINUI
    private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
    {
        // Mark event as handled and navigate as needed
        e.Handled = true;
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
#endif
}
