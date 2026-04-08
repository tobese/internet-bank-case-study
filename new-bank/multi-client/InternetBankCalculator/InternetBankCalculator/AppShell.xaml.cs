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
    private void BtnCalc_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(MainPage), BtnCalc);
    private void BtnAbout_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(AboutPage), BtnAbout);
    private void BtnSystemDesign_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(SystemDesignPage), BtnSystemDesign);

    private void BtnReload_Click(object sender, RoutedEventArgs e)
    {
#if __WASM__
        Uno.Foundation.WebAssemblyRuntime.InvokeJS("location.reload()");
#endif
    }

    public Frame ContentFramePublic => ContentFrame;
    public Button BtnAboutPublic => BtnAbout;
    public void UpdateNavHighlight(Button active)
    {
        foreach (var btn in new[] { BtnCalc, BtnAbout, BtnSystemDesign })
            btn.Opacity = 0.6;
        active.Opacity = 1.0;
    }

    private void NavigateTo(Type page, Button active)
    {
        if (ContentFrame.CurrentSourcePageType != page)
            ContentFrame.Navigate(page);
        UpdateNavHighlight(active);
    }

    // ...existing code...
}
