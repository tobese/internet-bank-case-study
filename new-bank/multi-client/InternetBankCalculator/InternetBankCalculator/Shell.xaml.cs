using Microsoft.UI.Xaml.Controls;

namespace InternetBankCalculator;

public sealed partial class Shell : UserControl
{
    public Frame RootFrame => ShellFrame;
    public MyLoadableSource LoadableSource { get; } = new();

    public Shell()
    {
        this.InitializeComponent();
        Splash.Source = LoadableSource;
    }
}