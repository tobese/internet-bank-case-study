using InternetBankCalculator.Services;
using InternetBankCalculator.ViewModels;

namespace InternetBankCalculator;

public sealed partial class AppShell : Page
{
    public MyLoadableSource LoadableSource { get; } = new();
    public AppShell()
    {
        this.InitializeComponent();
        // Bind splash state
        if (this.FindName("Splash") is Uno.Toolkit.UI.ExtendedSplashScreen splash)
        {
            splash.Source = LoadableSource;
        }
        ContentFrame.Navigate(typeof(LandingPage));
        UpdateNavHighlight(BtnHome);
        if (Window.Current?.CoreWindow != null)
        {
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }
        // Set DataContext for mathematician facts
        var api = new MathApiService(CalculatorPage.GetApiBaseUrl());
        DataContext = new CalculatorPageViewModel(api);
    }

    private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
    {
        var coreWindow = Windows.UI.Core.CoreWindow.GetForCurrentThread();
        if (coreWindow != null)
        {
            var ctrl = coreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
            if (ctrl && args.VirtualKey == Windows.System.VirtualKey.P)
            {
                BtnUprisingHidden_Click(this, new Microsoft.UI.Xaml.RoutedEventArgs());
            }
        }
    }

    private void BtnUprisingHidden_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(InternetBankCalculator.Uprising.StartPage));
    }

    private void BtnHome_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(LandingPage), BtnHome);

    public void NavigateToCalc() => NavigateTo(typeof(CalculatorPage), BtnCalc);
    private void BtnCalc_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(CalculatorPage), BtnCalc);
    private void BtnAbout_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(InternetBankCalculator.AboutContent.AboutPage), BtnAbout);
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
}
