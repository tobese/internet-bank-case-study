namespace InternetBankCalculator;

public sealed partial class AppShell : Page
{
    public AppShell()
    {
        this.InitializeComponent();
        ContentFrame.Navigate(typeof(LandingPage));
        UpdateNavHighlight(BtnHome);
    }

    private void BtnHome_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(LandingPage), BtnHome);

    public void NavigateToCalc() => NavigateTo(typeof(MainPage), BtnCalc);
    public void NavigateToDoggerBank() => NavigateTo(typeof(DoggerBankPage), BtnDoggerBank);
    public void NavigateToDoggerLand() => NavigateTo(typeof(DoggerLandPage), BtnDoggerLand);
    private void BtnCalc_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(MainPage), BtnCalc);
    private void BtnDoggerBank_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(DoggerBankPage), BtnDoggerBank);
    private void BtnDoggerLand_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(DoggerLandPage), BtnDoggerLand);
    private void BtnOperators_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(OperatorsPage), BtnOperators);
    private void BtnSystemDesign_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(SystemDesignPage), BtnSystemDesign);

    private void BtnReload_Click(object sender, RoutedEventArgs e)
    {
#if __WASM__
        Uno.Foundation.WebAssemblyRuntime.InvokeJS("location.reload()");
#endif
    }

    private void NavigateTo(Type page, Button active)
    {
        if (ContentFrame.CurrentSourcePageType != page)
            ContentFrame.Navigate(page);
        UpdateNavHighlight(active);
    }

    private void UpdateNavHighlight(Button active)
    {
        foreach (var btn in new[] { BtnCalc, BtnDoggerBank, BtnDoggerLand, BtnOperators, BtnSystemDesign })
            btn.Opacity = 0.6;
        active.Opacity = 1.0;
    }
}
