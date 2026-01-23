using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using Steamworks;

namespace SteamworksHelper;

class Program
{
    private const string ResetAchievementsEnvironmentVariable = "RESET_ACHIEVEMENTS";
    
    private static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
    }

    private static async Task MainAsync()
    {
        SteamClient.Init(4026050);
        if (Environment.GetEnvironmentVariable(ResetAchievementsEnvironmentVariable)
                ?.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ?? false)
        {
            SteamUserStats.ResetAll(includeAchievements: true); // TODO: DELETE THIS
        }
        
        NamedPipeServerStream steamworksServer = new("SteamworksHelperPipe");
        NamedPipeClientStream steamworksClient = new("SteamworksReturnPipe");
        await steamworksClient.ConnectAsync();
        ControllerManager controllerManager = new();
        bool loop = true;
        await steamworksServer.WaitForConnectionAsync();
        
        while (loop)
        {
            try
            {
                if (!steamworksServer.CanRead)
                {
                    continue;
                }
                byte[] utf8Bytes = new byte[512];
                await steamworksServer.ReadExactlyAsync(utf8Bytes);
                string[] cmd = Encoding.UTF8.GetString(utf8Bytes, 0, utf8Bytes.Length).Replace('\u0000', ' ').TrimEnd().Split(' ');

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
                        loop = false;
                        break;
                    
                    case "GAME_LANGUAGE":
                        Console.WriteLine("Fetching game language...");
                        steamworksClient.SendResponse(Steamworks.GameLanguage());
                        break;
                    
                    case "INPUT_ACTION_ANALOG":
                        Console.WriteLine("Fetching analog input action...");
                        ControllerAnalogResponse analogResponse = controllerManager.GetAnalogState(cmd[1]);
                        steamworksClient.SendResponse([analogResponse.Up, analogResponse.Right, analogResponse.Down, analogResponse.Left]);
                        break;
                    
                    case "INPUT_ACTION_DIGITAL":
                        Console.WriteLine("Fetching digital input action...");
                        steamworksClient.SendResponse([(byte)(controllerManager.GetDigitalState(cmd[1]) ? 1 : 0)]);
                        break;
                    
                    case "INPUT_ACTION_GET_GLYPH":
                        Console.WriteLine("Fetching action glyph...");
                        string? glyph = controllerManager.GetGlyph(cmd[1]);
                        steamworksClient.SendResponse(glyph ?? string.Empty);
                        break;
                    
                    case "INPUT_ACTION_SET_SET":
                        Console.WriteLine("Setting input action set...");
                        controllerManager.SetActionSet(cmd[1]);
                        break;
                    
                    case "INPUT_POLL_CONTROLLERS":
                        Console.WriteLine("Polling controllers...");
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