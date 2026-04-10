using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace InternetBankCalculator.Uprising
{
    public sealed partial class EndPage : Page
    {
        public EndPage()
        {
            this.InitializeComponent();
        }

        private void OnRestartClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StartPage));
        }
    }
}
