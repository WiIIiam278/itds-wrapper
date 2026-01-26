using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.ReactiveUI;
#if MACOS
using AvFoundationBackend;
#endif
using ITDSWrapper.Desktop.Steam;

namespace ITDSWrapper.Desktop;

sealed class Program
{
    private const string DebugIpcEnvironmentVariable = "DEBUG_IPC";
    private const string NoSteamEnvironmentVariable = "NOSTEAM";
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
                string? ipcPath = Environment.GetEnvironmentVariable(DebugIpcEnvironmentVariable);
                if (!string.IsNullOrEmpty(ipcPath))
                {
                    Process.Start(ipcPath);
                }
                SteamHelperIpc ipc = new();
                if (!Environment.GetEnvironmentVariable(NoSteamEnvironmentVariable)
                        ?.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ?? true)
                {
                    try
                    {
                        if (Environment.GetEnvironmentVariable(ClearSteamCloudEnvironmentVariable)
                                ?.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ?? false)
                        {
                            SteamSaveManager.ClearSteamCloud(ipc);
                        }
                        SteamInputDriver inputDriver = new(ipc);
                        ((App)b.Instance!).InputDrivers = [inputDriver];
                        ((App)b.Instance).Updater = new SteamUpdater(inputDriver, ipc);
                        ((App)b.Instance).InputSwitcher = new();
                        SteamLogInterpreter logInterpreter = new(inputDriver, ((App)b.Instance).InputSwitcher!, ipc)
                        {
                            AchievementManager = new SteamAchievementManager(ipc),
                            WatchForSdCreate = SteamSaveManager.DownloadCloudSave(ipc),
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
                ipc.SendCommand("GAME_LANGUAGE");
#if MACOS
                ((App)b.Instance).ScreenReader = new AvFoundationScreenReader(DesktopScreenReader.GetPlatformSpecificLanguageCode(Encoding.UTF8.GetString(ipc.ReceiveResponse())));
#else
                ((App)b.Instance).ScreenReader = DesktopScreenReader.Instantiate(Encoding.UTF8.GetString(ipc.ReceiveResponse()));
#endif
            });
}