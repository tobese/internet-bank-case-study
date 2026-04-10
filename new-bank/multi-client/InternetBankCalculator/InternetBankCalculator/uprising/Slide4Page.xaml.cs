using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InternetBankCalculator.Uprising
{
    public sealed partial class Slide4Page : Page
    {
        public Slide4Page()
        {
            this.InitializeComponent();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Slide3Page));
        }
        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Slide5Page));
        }
    }
}
