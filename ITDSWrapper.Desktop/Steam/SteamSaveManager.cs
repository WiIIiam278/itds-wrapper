using System;
using System.IO;
using System.Reflection;
using DiscUtils;
using DiscUtils.Fat;
using DiscUtils.Streams;
using Libretro.NET;
using Steamworks;

namespace ITDSWrapper.Desktop.Steam;

public static class SteamSaveManager
{
    private const string SaveFileName = "into-the-dream-spring.sav";
    private static readonly string SaveDir = RetroWrapper.GetDirectoryForPlatform("saves");
    
    
    public static void UploadCloudSave()
    {
        byte[] sdCardBytes = File.ReadAllBytes(Path.Combine(SaveDir, "melonDS DS", "dldi_sd_card.bin"));
        using MemoryStream sdCardStream = new(sdCardBytes[0x7E00..]);
        FatFileSystem sdCardFat = new(sdCardStream, Ownership.None);
        if (!sdCardFat.FileExists(SaveFileName))
        {
            return;
        }
        
        using SparseStream savStream = sdCardFat.OpenFile(SaveFileName, FileMode.Open);
        byte[] savFile = new byte[savStream.Length];
        savStream.ReadExactly(savFile);
        SteamRemoteStorage.FileWrite(SaveFileName, savFile);
    }
    
    public static bool DownloadCloudSave()
    {
        string sdCardFile = Path.Combine(RetroWrapper.GetDirectoryForPlatform("saves"), "melonDS DS", "dldi_sd_card.bin");
        if (!SteamRemoteStorage.FileExists(SaveFileName))
        {
            return false;
        }
        if (!File.Exists(sdCardFile))
        {
            return true;
        }
        
        byte[] saveFile = SteamRemoteStorage.FileRead(SaveFileName);
        byte[] sdCardBytes = File.ReadAllBytes(sdCardFile);
        using SparseMemoryStream sdCardStream = new();
        sdCardStream.Write(sdCardBytes[0x7E00..]);
        sdCardStream.Seek(0, SeekOrigin.Begin);
        
        FatFileSystem sdCardFat = new(sdCardStream, Ownership.None);
        if (sdCardFat.FileExists(SaveFileName))
        {
            using SparseStream savStream =
                sdCardFat.OpenFile(SaveFileName, FileMode.Open, FileAccess.Write);
            savStream.Write(saveFile, 0, saveFile.Length);
            savStream.Flush();
        }
        else
        {
            using SparseStream savStream =
                sdCardFat.OpenFile(SaveFileName, FileMode.CreateNew, FileAccess.Write);
            savStream.Write(saveFile, 0, saveFile.Length);
            savStream.Flush();
            byte[] oldSdCardBytes = sdCardBytes;
            sdCardBytes = new byte[0x7E00 + sdCardStream.Length];
            Array.Copy(oldSdCardBytes, sdCardBytes, oldSdCardBytes.Length);
        }

        byte[] sdCardStreamBytes = new byte[sdCardStream.Length];
        sdCardStream.Seek(0, SeekOrigin.Begin);
        sdCardStream.ReadExactly(sdCardStreamBytes);
        Array.Copy(sdCardStreamBytes, 0, sdCardBytes, 0x7E00, sdCardStream.Length);
        File.WriteAllBytes(sdCardFile, sdCardBytes);

        return false;
    }
}