namespace InternetBankCalculator;

public sealed partial class OperatorsPage : Page
{
    public OperatorsPage()
    {
        this.InitializeComponent();
    }

    private void CloudBanking_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to CloudBankingPage
        if (this.Frame != null)
        {
            this.Frame.Navigate(typeof(CloudBankingPage));
        }
    }

    private void DoggerBank_Click(object sender, RoutedEventArgs e)
    {
        if (this.Frame != null)
        {
            this.Frame.Navigate(typeof(DoggerBankPage));
        }
    }

    private void DoggerLand_Click(object sender, RoutedEventArgs e)
    {
        if (this.Frame != null)
        {
            this.Frame.Navigate(typeof(DoggerLandPage));
        }
    }
}
