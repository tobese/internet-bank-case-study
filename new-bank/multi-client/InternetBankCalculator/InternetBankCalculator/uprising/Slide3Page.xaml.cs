using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InternetBankCalculator.Uprising
{
    public sealed partial class Slide3Page : Page
    {
        public Slide3Page()
        {
            this.InitializeComponent();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Slide2Page));
        }
        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Slide4Page));
        }
    }
}
