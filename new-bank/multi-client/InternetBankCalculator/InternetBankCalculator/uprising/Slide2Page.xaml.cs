using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InternetBankCalculator.Uprising
{
    public sealed partial class Slide2Page : Page
    {
        public Slide2Page()
        {
            this.InitializeComponent();
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Slide1Page));
        }
        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Slide3Page));
        }
    }
}
