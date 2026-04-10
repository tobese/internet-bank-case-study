using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InternetBankCalculator.Uprising
{
    public sealed partial class Slide1Page : Page
    {
        public Slide1Page()
        {
            this.InitializeComponent();
        }

        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Slide2Page));
        }
    }
}
