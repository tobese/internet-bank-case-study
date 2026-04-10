using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InternetBankCalculator.Uprising
{
    public sealed partial class Slide8Page : Page
    {
        public Slide8Page()
        {
            this.InitializeComponent();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Slide7Page));
        }
        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(EndPage));
        }
    }
}
