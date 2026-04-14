namespace InternetBankCalculator;

public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    public Window? MainWindow { get; private set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new Window();
#if DEBUG
        MainWindow.UseStudio();
#endif

#if !WINDOWS
        // Enable system back button for WASM/Android
        Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
#endif

        if (MainWindow.Content is not AppShell shell)
        {
            shell = new AppShell();
            MainWindow.Content = shell;
        }

        // Show splash, then navigate
        if (shell.LoadableSource.IsExecuting == false)
        {
            shell.LoadableSource.IsExecuting = true;
            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(5000);
                shell.LoadableSource.IsExecuting = false;
            });
        }

        MainWindow.Activate();
    }

    /// <summary>
    /// Invoked when Navigation to a certain page fails
    /// </summary>
    /// <param name="sender">The Frame which failed navigation</param>
    /// <param name="e">Details about the navigation failure</param>
    void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
    }

    /// <summary>
    /// Configures global Uno Platform logging
    /// </summary>
    public static void InitializeLogging()
    {
        // Enable verbose Uno logging for debugging/tracing in all environments
        var factory = LoggerFactory.Create(builder =>
        {
#if __WASM__
            builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
        builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
        builder.AddConsole();
#else
        builder.AddConsole();
#endif

            // Set minimum log level to Debug for all categories
            builder.SetMinimumLevel(LogLevel.Debug);
            // Optionally, set to Trace for even more detail:
            // builder.SetMinimumLevel(LogLevel.Trace);

            // Enable Uno, Windows, and Microsoft logs at Debug level
            builder.AddFilter("Uno", LogLevel.Debug);
            builder.AddFilter("Windows", LogLevel.Debug);
            builder.AddFilter("Microsoft", LogLevel.Debug);

            // Uncomment for even more detail:
            // builder.AddFilter("Microsoft.UI.Xaml", LogLevel.Debug);
            // builder.AddFilter("Microsoft.UI.Xaml.FrameworkElement", LogLevel.Trace);
            // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug);
        });

        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
    }
}
