using System;
using System.IO;
using Avalonia;
using Avalonia.ReactiveUI;
using ITDSWrapper.Desktop.Steam;
using Steamworks;

namespace ITDSWrapper.Desktop;

sealed class Program
{
    private const string NoSteamEnvironmentVariable = "NOSTEAM";
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace()
            .AfterSetup(b =>
            {
                if (!Environment.GetEnvironmentVariable(NoSteamEnvironmentVariable)
                        ?.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ?? true)
                {
                    try
                    {
                        SteamClient.Init(4026050);
                        SteamInputDriver inputDriver = new();
                        ((App)b.Instance!).InputDriver = inputDriver;
                        ((App)b.Instance!).Updater = new SteamUpdater(inputDriver);
                    }
                    catch (Exception ex)
                    {
                        File.WriteAllText("crash.log", $"{ex.Message}\n{ex.StackTrace}");
                        throw;
                    }
                }
            });
}