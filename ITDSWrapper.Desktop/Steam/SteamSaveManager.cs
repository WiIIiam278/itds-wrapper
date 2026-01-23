using System;
using System.IO;
using System.Threading;
using DiscUtils.Fat;
using DiscUtils.Streams;
using Libretro.NET;

namespace ITDSWrapper.Desktop.Steam;

public static class SteamSaveManager
{
    private const string SaveFileName = "itds-demo.sav";
    private static readonly string SaveDir = Path.GetFullPath(RetroWrapper.GetDirectoryForPlatform("saves"));
    
    public static void UploadCloudSave(SteamHelperIpc ipc)
    {
        string sdCardPath = Path.Combine(SaveDir, "melonDS DS", "dldi_sd_card.bin");
        ipc.SendCommand($"CLOUD_SAVE_UPLOAD {SaveDir.Replace(' ', '\u0000')} {SaveFileName} {sdCardPath.Replace(' ', '\u0000')}");
    }
    
    public static bool DownloadCloudSave(SteamHelperIpc ipc)
    {
        string sdCardPath = Path.Combine(RetroWrapper.GetDirectoryForPlatform("saves"), "melonDS DS", "dldi_sd_card.bin");
        ipc.SendCommand($"CLOUD_SAVE_UPLOAD {SaveFileName} {sdCardPath.Replace(' ', '\u0000')}");

        return false;
    }

    public static void ClearSteamCloud(SteamHelperIpc ipc)
    {
        ipc.SendCommand("CLOUD_SAVE_CLEAR");
    }
}