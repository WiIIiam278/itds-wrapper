using System;
using System.IO;
using Avalonia;
using Avalonia.ReactiveUI;
#if MACOS
using AvFoundationBackend;
#endif
using ITDSWrapper.Desktop.Steam;
using Steamworks;

namespace ITDSWrapper.Desktop;

sealed class Program
{
    private const string NoSteamEnvironmentVariable = "NOSTEAM";
    private const string ResetAchievementsEnvironmentVariable = "RESET_ACHIEVEMENTS";
    private const string ClearSteamCloudEnvironmentVariable = "CLEAR_CLOUD";
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"), $"CRASH: {ex.Message}\n\n{ex.StackTrace}");
            throw;
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace()
            .With(new Win32PlatformOptions
            {
                RenderingMode = [Win32RenderingMode.Vulkan, Win32RenderingMode.AngleEgl, Win32RenderingMode.Wgl, Win32RenderingMode.Software],
            })
            .AfterSetup(b =>
            {
                if (!Environment.GetEnvironmentVariable(NoSteamEnvironmentVariable)
                        ?.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ?? true)
                {
                    try
                    {
                        SteamClient.Init(4026050);
                        if (Environment.GetEnvironmentVariable(ResetAchievementsEnvironmentVariable)
                                ?.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ?? false)
                        {
                            SteamUserStats.ResetAll(includeAchievements: true); // TODO: DELETE THIS
                        }
                        if (Environment.GetEnvironmentVariable(ClearSteamCloudEnvironmentVariable)
                                ?.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ?? false)
                        {
                            SteamSaveManager.ClearSteamCloud();
                        }
                        SteamInputDriver inputDriver = new();
                        ((App)b.Instance!).InputDrivers = [inputDriver];
                        ((App)b.Instance).Updater = new SteamUpdater(inputDriver);
                        ((App)b.Instance).InputSwitcher = new();
                        SteamLogInterpreter logInterpreter = new(inputDriver, ((App)b.Instance).InputSwitcher!)
                        {
                            AchievementManager = new SteamAchievementManager(),
                            WatchForSdCreate = SteamSaveManager.DownloadCloudSave(),
                        };
                        ((App)b.Instance).LogInterpreter = logInterpreter;
                    }
                    catch (Exception ex)
                    {
                        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"setup_crash.log"), $"{ex.Message}\n{ex.StackTrace}");
                        throw;
                    }
                }
                ((App)b.Instance!).BatteryMonitor = new BatteryMonitor();
#if MACOS
                ((App)b.Instance).AudioBackend = new AvFoundationAudioBackend();
#endif
                
#if MACOS
                ((App)b.Instance).ScreenReader = new AvFoundationScreenReader(DesktopScreenReader.GetPlatformSpecificLanguageCode(SteamApps.GameLanguage));
#else
                ((App)b.Instance).ScreenReader = DesktopScreenReader.Instantiate(SteamApps.GameLanguage);
#endif
            });
}