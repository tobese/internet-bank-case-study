using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InternetBankCalculator.Uprising
{
    public sealed partial class Slide7Page : Page
    {
        public Slide7Page()
        {
            this.InitializeComponent();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Slide6Page));
        }
        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Slide8Page));
        }
    }
}
