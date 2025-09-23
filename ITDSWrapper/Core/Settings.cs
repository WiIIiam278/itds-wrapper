using System.IO;
using System.Text.Json;
using DynamicData;

namespace ITDSWrapper.Core;

public class Settings
{
    public bool FirstLaunch { get; set; } = true;
    public bool BordersEnabled { get; set; } = true;
    public bool ScreenReaderEnabled { get; set; } = false;
    public string LanguageCode { get; set; } = "en";

    private static string[] LangCodeArray = ["en", "ja"];
    public byte LanguageIndex => (byte)LangCodeArray.IndexOf(LanguageCode);

    public void Save(string path)
    {
        File.WriteAllText(Path.Combine(path, "settings.json"), JsonSerializer.Serialize(this));
    }

    public static Settings Load(string? path)
    {
        return !string.IsNullOrEmpty(path) && Path.Exists(Path.Combine(path, "settings.json"))
            ? JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(path, "settings.json"))) ?? new()
            : new();
    }
}