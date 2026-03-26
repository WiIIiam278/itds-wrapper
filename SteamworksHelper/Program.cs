using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if MACOS
using AppKit;
using Foundation;
#endif
using Steamworks;

namespace SteamworksHelper;

public static class Program
{
    private const string ResetAchievementsEnvironmentVariable = "RESET_ACHIEVEMENTS";
    private const string GamePathEnvironmentVariable = "ITDS_PATH";

    private static bool _loop = true;
    private static NamedPipeClientStream? _keyboardPipe;

    private static bool _stealFocus = true;

    private static void Main(string[] args)
    {
#if MACOS
        NSApplication.Init();

        // Oh boy! macOS sucks!
        // This is how we pass keyboard inputs from the focus-stealing helper to the game
        NSWindow helperWindow = new(new(-10, -10, 1, 1), NSWindowStyle.Borderless, NSBackingStore.Buffered, false);
        helperWindow.BackgroundColor = NSColor.Clear;
        helperWindow.IsOpaque = false;
        helperWindow.MakeKeyAndOrderFront(null);

        NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask.KeyDown | NSEventMask.KeyUp | NSEventMask.FlagsChanged,
            nsEvent =>
            {
                if (nsEvent.KeyCode == 0x0C && (nsEvent.ModifierFlags & NSEventModifierMask.CommandKeyMask) != 0)
                {
                    _loop = false;
                }
                else
                {
                    byte eventType = nsEvent.Type switch
                    {
                        NSEventType.KeyDown => 0,
                        NSEventType.KeyUp => 1,
                        _ => 2,
                    };
                    byte modifiers = (byte)((uint)nsEvent.ModifierFlags >> 16);

                    byte[] packet = [eventType, ..BitConverter.GetBytes(nsEvent.KeyCode), modifiers];

                    _keyboardPipe?.Write(packet);
                }
                return null!; // putting an exclamation mark here is so funny. my programmer: it's not null, compiler! it's not null! me: literally null
                // Anyway, we need to pass null here to prevent keyboard beeps and such; we're breaking the event response chain
            });

        NSWorkspace.Notifications.ObserveDidActivateApplication((_, appArgs) =>
        {
            if (!_stealFocus)
                return;
            
            NSRunningApplication activatedApp = appArgs.Application;

            if (activatedApp.BundleIdentifier == "com.intothedreamspring.ITDSWrapper")
            {
                NSApplication.SharedApplication.InvokeOnMainThread(() =>
                    NSApplication.SharedApplication.ActivateIgnoringOtherApps(true));
            }
        });

        NSApplication.Notifications.ObserveDidBecomeActive((_, _) =>
        {
            if (!_stealFocus)
                return;
            _stealFocus = false;
            
            NSRunningApplication? wrapper = NSWorkspace.SharedWorkspace.RunningApplications
                .FirstOrDefault(a => a.BundleIdentifier == "com.intothedreamspring.ITDSWrapper");
            wrapper?.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);

            Task.Delay(500).ContinueWith(_ =>
            {
                NSApplication.SharedApplication.InvokeOnMainThread(() =>
                    NSApplication.SharedApplication.ActivateIgnoringOtherApps(true));

                Task.Delay(200).ContinueWith(__ => _stealFocus = true);
            });
        });

        // Init on the main thread on macOS so we can pump it for input requests
        SteamClient.Init(4026050);

        Task.Run(async () =>
        {
            try
            {
                await MainAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                NSApplication.SharedApplication.InvokeOnMainThread(() =>
                    NSApplication.SharedApplication.Stop(NSRunLoop.Current));
            }
        });

        NSApplication.SharedApplication.Run();
#else
        MainAsync().GetAwaiter().GetResult();
#endif
    }

    private static async Task MainAsync()
    {
#if !MACOS
        SteamClient.Init(4026050);
#endif
        if (Environment.GetEnvironmentVariable(ResetAchievementsEnvironmentVariable)
                ?.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            SteamUserStats.ResetAll(includeAchievements: true); // TODO: DELETE THIS
        }

        string? gamePath = Environment.GetEnvironmentVariable(GamePathEnvironmentVariable);
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEBUG_IPC")) && !string.IsNullOrEmpty(gamePath))
        {
            if (OperatingSystem.IsMacOS())
            {
                ProcessStartInfo psi = new("open")
                {
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    ArgumentList = { gamePath },
                };
                Process.Start(psi);
            }
            else
            {
                Process.Start(gamePath);
            }
        }
        else if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DEBUG_IPC")))
        {
            Process.Start(OperatingSystem.IsWindows() ? ".\\ITDSWrapper.Desktop.exe" : "./ITDSWrapper.Desktop");
        }

        NamedPipeServerStream steamworksServer = new("SteamworksHelperPipe");
        NamedPipeClientStream steamworksClient = new("SteamworksReturnPipe");
        await steamworksClient.ConnectAsync();
        ControllerManager controllerManager = new();
        await steamworksServer.WaitForConnectionAsync();
#if MACOS
        _keyboardPipe = new("macOSKeyboardPipe");
        await _keyboardPipe.ConnectAsync();
#endif

        await RunIpcLoop(steamworksServer, steamworksClient, controllerManager);
    }

    private static async Task RunIpcLoop(NamedPipeServerStream steamworksServer, NamedPipeClientStream steamworksClient,
        ControllerManager controllerManager)
    {
        while (_loop)
        {
            try
            {
                if (!steamworksServer.CanRead)
                {
                    continue;
                }

                byte[] utf8Bytes = new byte[512];
                await steamworksServer.ReadExactlyAsync(utf8Bytes);
                string[] cmd = Encoding.UTF8.GetString(utf8Bytes, 0, utf8Bytes.Length).Split(' ')
                    .Select(s => s.Replace('\u0000', ' ').TrimEnd()).ToArray();

                switch (cmd[0])
                {
                    case "ACHIEVEMENT":
                        Console.WriteLine("Unlocking achievement...");
                        Steamworks.UnlockAchievement(cmd[1]);
                        break;

                    case "CLOUD_SAVE_CLEAR":
                        Console.WriteLine("Clearing cloud saves...");
                        Steamworks.ClearSteamCloud();
                        break;

                    case "CLOUD_SAVE_DOWNLOAD":
                        Console.WriteLine("Downloading cloud save...");
                        Steamworks.DownloadCloudSave(cmd[1], cmd[2]);
                        break;

                    case "CLOUD_SAVE_UPLOAD":
                        Console.WriteLine("Uploading cloud save...");
                        Steamworks.UploadCloudSave(cmd[1], cmd[2], cmd[3]);
                        break;

                    case "DIE":
                        Console.WriteLine("Exiting...");
                        _loop = false;
                        break;

                    case "GAME_LANGUAGE":
                        Console.WriteLine("Fetching game language...");
                        steamworksClient.SendResponse(Steamworks.GameLanguage());
                        break;

                    case "INPUT_ACTION_ANALOG":
                        ControllerAnalogResponse analogResponse = controllerManager.GetAnalogState(cmd[1]);
                        steamworksClient.SendResponse([
                            analogResponse.Up, analogResponse.Right, analogResponse.Down, analogResponse.Left
                        ]);
                        break;

                    case "INPUT_ACTION_DIGITAL":
                        steamworksClient.SendResponse([(byte)(controllerManager.GetDigitalState(cmd[1]) ? 1 : 0)]);
                        break;

                    case "INPUT_ACTION_GET_GLYPH":
                        string? glyph = controllerManager.GetGlyph(cmd[1]);
                        steamworksClient.SendResponse(glyph ?? string.Empty);
                        break;

                    case "INPUT_ACTION_SET_SET":
                        controllerManager.SetActionSet(cmd[1]);
                        break;

                    case "INPUT_POLL_CONTROLLERS":
                        ControllerPollResponse pollResponse = controllerManager.PollControllers();
                        steamworksClient.SendResponse([pollResponse.HasController, pollResponse.NewController]);
                        break;

                    case "INPUT_INIT":
                        Console.WriteLine("Initializing input...");
                        Steamworks.InputInit();
                        break;

                    case "INPUT_RUMBLE":
                        Console.WriteLine("Starting controller rumble...");
                        controllerManager.Rumble(ushort.Parse(cmd[1]));
                        break;

                    case "INPUT_SHUTDOWN":
                        Console.WriteLine("Shutting down input...");
                        Steamworks.InputShutdown();
                        break;

                    case "RICH_PRESENCE":
                        Console.WriteLine("Emitting rich presence...");
                        Steamworks.SetRichPresence(cmd[1..]);
                        break;

                    case "TIMELINE_INST":
                        Console.WriteLine("Emitting instantaneous timeline event...");
                        Steamworks.TimelineInstantaneous(cmd[1..]);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}