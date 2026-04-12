namespace InternetBankCalculator;

using Microsoft.UI.Xaml.Controls;
using System;

public class NavigationService
{
    private static NavigationService? _instance;
    public static NavigationService Instance => _instance ??= new NavigationService();

    public Frame? RootFrame { get; private set; }
    public AppShell? Shell { get; private set; }

    public void Initialize(Frame rootFrame, AppShell shell)
    {
        RootFrame = rootFrame;
        Shell = shell;
    }

    public void Navigate(Type pageType)
    {
        RootFrame?.Navigate(pageType);
    }

    public void NavigateInShell(Type pageType, Action<AppShell>? shellAction = null)
    {
        if (Shell != null)
        {
            Shell.ContentFramePublic.Navigate(pageType);
            shellAction?.Invoke(Shell);
        }
    }
}