using Uno.UI.Hosting;

namespace InternetBankCalculator;

public class Program
{
    public static async Task Main(string[] args)
    {
        System.Diagnostics.Debug.WriteLine("Starting InternetBankCalculator on WebAssembly...");
        App.InitializeLogging();

        var host = UnoPlatformHostBuilder.Create()
            .App(() => new App())
            .UseWebAssembly()
            .Build();

        await host.RunAsync();
    }
}
