using Microsoft.UI.Xaml.Controls;
using InternetBankCalculator.Loadables;

namespace InternetBankCalculator;

public sealed partial class Shell : UserControl
{
    public Frame RootFrame => ShellFrame;
    public SplashLoadable LoadableSource { get; } = new();

    public Shell()
    {
        this.InitializeComponent();
        Splash.Source = LoadableSource;
    }
}