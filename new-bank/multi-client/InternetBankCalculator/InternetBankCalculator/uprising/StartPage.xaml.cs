using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InternetBankCalculator.Uprising
{
    public sealed partial class StartPage : Page
    {
        public StartPage()
        {
            this.InitializeComponent();
        }

        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Slide1Page));
        }
    }
}
