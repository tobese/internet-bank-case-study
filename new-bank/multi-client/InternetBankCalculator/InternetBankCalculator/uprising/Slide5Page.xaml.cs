using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InternetBankCalculator.Uprising
// Suppress nullability warning for OnBackRequested handler
#pragma warning disable CS8622
{
    public sealed partial class Slide5Page : Page
    {
        public Slide5Page()
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
        // Re-enable warnings
#pragma warning restore CS8622

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Slide4Page));
        }
        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Slide6Page));
        }
    }
}
