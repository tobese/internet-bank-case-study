using InternetBankCalculator.Services;
using InternetBankCalculator.ViewModels;
using InternetBankCalculator.Loadables;

namespace InternetBankCalculator;

public sealed partial class AppShell : Page
{
    public SplashLoadable LoadableSource { get; } = new();

    public string AppVersion { get; } =
        (typeof(AppShell).Assembly
            .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
            .FirstOrDefault() as System.Reflection.AssemblyInformationalVersionAttribute)
            ?.InformationalVersion?.Split('+')[0]
        ?? typeof(AppShell).Assembly.GetName().Version?.ToString(2)
        ?? "1.0";
    public AppShell()
    {
        this.InitializeComponent();
        // Update back button visibility on navigation
        ContentFrame.Navigated += (s, e) => UpdateBackButtonVisibility();
        // Bind splash state
        if (this.FindName("Splash") is Uno.Toolkit.UI.ExtendedSplashScreen splash)
        {
            splash.Source = LoadableSource;
        }
        // Set up NavigationService singleton
        NavigationService.Instance.Initialize(ContentFrame, this);
        ContentFrame.Navigate(typeof(Pages.LandingPage));
        UpdateNavHighlight(BtnHome);
        if (Window.Current?.CoreWindow != null)
        {
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
        }
        // Set DataContext for mathematician facts
        var api = new MathApiService(Pages.CalculatorPage.GetApiBaseUrl());
        DataContext = new CalculatorPageViewModel(api);

        // Set initial back button visibility
        UpdateBackButtonVisibility();

        // Removed broken JS interop for browser back navigation (mono_bind_static_method)
    }

    private void UpdateBackButtonVisibility()
    {
#if !WINDOWS
        Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            ContentFrame.CanGoBack
                ? Windows.UI.Core.AppViewBackButtonVisibility.Visible
                : Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
#endif
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

    private void BtnHome_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(Pages.LandingPage), BtnHome);

    public void NavigateToCalc() => NavigateTo(typeof(Pages.CalculatorPage), BtnCalc);
    private void BtnCalc_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(Pages.CalculatorPage), BtnCalc);
    private void BtnAbout_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(Pages.About.AboutPage), BtnAbout);
    private void BtnSystemDesign_Click(object sender, RoutedEventArgs e) => NavigateTo(typeof(Pages.SystemDesignPage), BtnSystemDesign);

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
        var inactiveOpacity = Application.Current.Resources.TryGetValue("MediumOpacity", out var o) ? (double)o : 0.64;
        foreach (var btn in new[] { BtnCalc, BtnAbout, BtnSystemDesign })
            btn.Opacity = inactiveOpacity;
        active.Opacity = 1.0;
    }

    private void NavigateTo(Type page, Button active)
    {
        if (ContentFrame.CurrentSourcePageType != page)
            ContentFrame.Navigate(page);
        UpdateNavHighlight(active);
    }
}
