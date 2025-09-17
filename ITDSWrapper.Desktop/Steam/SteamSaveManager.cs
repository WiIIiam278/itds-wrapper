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
    private const string SdCardHeaderName = "sd_header.bin";
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

        SteamRemoteStorage.FileWrite(SdCardHeaderName, sdCardBytes[..0x7E00]);
    }
    
    public static void DownloadCloudSave()
    {
        string sdCardFile = Path.Combine(RetroWrapper.GetDirectoryForPlatform("saves"), "melonDS DS", "dldi_sd_card.bin");
        if (!SteamRemoteStorage.FileExists(SaveFileName))
        {
            return;
        }
        
        byte[] saveFile = SteamRemoteStorage.FileRead(SaveFileName);
        if (!File.Exists(sdCardFile))
        {
            File.WriteAllText($"{sdCardFile}.idx", "SIZE 4294967296\n");

            byte[] sdCardHeaderBytes = SteamRemoteStorage.FileRead(SdCardHeaderName);
            using MemoryStream sdCardStream = new();
            FatFileSystem sdCardFat = FatFileSystem.FormatPartition(sdCardStream, "NO NAME    ",
                Geometry.FromCapacity(4294393856), 0, 8388545, 0);

            using SparseStream romStream = sdCardFat.OpenFile("itds.nds", FileMode.Create, FileAccess.Write);
            using Stream ndsStream =
                Assembly.GetExecutingAssembly().GetManifestResourceStream("ITDSWrapper.itds.nds")!;
            ndsStream.CopyTo(romStream);
            romStream.Flush();

            using SparseStream savStream =
                sdCardFat.OpenFile(SaveFileName, FileMode.Create, FileAccess.Write);
            savStream.Write(saveFile, 0, saveFile.Length);
            savStream.Flush();

            byte[] sdCardBytes = new byte[sdCardHeaderBytes.Length + sdCardStream.Length];
            sdCardHeaderBytes.CopyTo(sdCardBytes, 0);
            Array.Copy(sdCardStream.ToArray(), 0, sdCardBytes, 0x7E00, sdCardStream.Length);
            File.WriteAllBytes(sdCardFile, sdCardBytes);
        }
        else
        {
            byte[] sdCardBytes = File.ReadAllBytes(sdCardFile);
            using MemoryStream sdCardStream = new(sdCardBytes[0x7E00..]);
            FatFileSystem sdCardFat = new(sdCardStream, Ownership.None);
            using SparseStream savStream =
                sdCardFat.OpenFile(SaveFileName, FileMode.Create, FileAccess.Write);
            savStream.Write(saveFile, 0, saveFile.Length);
            savStream.Flush();
            Array.Copy(sdCardStream.ToArray(), 0, sdCardBytes, 0x7E00, sdCardStream.Length);
            File.WriteAllBytes(sdCardFile, sdCardBytes);
        }
    }
}