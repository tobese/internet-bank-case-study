
#pragma warning disable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System;

namespace InternetBankCalculator.Pages.About;

public sealed partial class FromJavaToCSharpPage : Page
{
    public FromJavaToCSharpPage()
    {
        this.InitializeComponent();
#if !WINDOWS
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
#endif
    }

#if !WINDOWS
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;
    }
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested -= OnBackRequested;
    }
    private void OnBackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
            e.Handled = true;
        }
    }
#endif

    private async void OnExternalLinkClick(object sender, object e)
    {
        string? url = null;
        if (sender is HyperlinkButton btn && btn.NavigateUri is Uri btnUri)
        {
            url = btnUri.ToString();
        }
        else if (sender is Hyperlink link && link.NavigateUri is Uri linkUri)
        {
            url = linkUri.ToString();
        }
        if (!string.IsNullOrEmpty(url))
        {
#if __WASM__
            // Open in new tab for WebAssembly
            await Windows.System.Launcher.LaunchUriAsync(new Uri(url), new Windows.System.LauncherOptions { TreatAsUntrusted = false });
#else
            await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
#endif
        }
    }
}
