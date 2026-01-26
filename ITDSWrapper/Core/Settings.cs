using System.IO;
using System.Text.Json;
using DynamicData;

namespace ITDSWrapper.Core;

public class Settings
{
    public bool FirstLaunch { get; set; } = true;
    public bool BordersEnabled { get; set; } = true;
    public bool ScreenReaderEnabled { get; set; } = false;
    public bool ControlPadHapticsEnabled { get; set; } = true;
    public string LanguageCode { get; set; } = "en";

    private static string[] _langCodeArray = ["en", "ja"];
    public byte LanguageIndex => (byte)_langCodeArray.IndexOf(LanguageCode);
    
    public WindowingMode WindowingMode { get; set; } = WindowingMode.FULL_SCREEN;
    public ScreenLayout CurrentScreenLayout { get; set; } = ScreenLayout.TOP_BOTTOM;

    public void Save(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        File.WriteAllText(Path.Combine(path, "settings.json"), JsonSerializer.Serialize(this));
    }

    public static Settings Load(string? path)
    {
        return !string.IsNullOrEmpty(path) && Path.Exists(Path.Combine(path, "settings.json"))
            ? JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(path, "settings.json"))) ?? new()
            : new();
    }
}

public enum WindowingMode
{
    FULL_SCREEN,
    BORDERLESS,
    WINDOWED,
}

public enum ScreenLayout
{
    TOP_BOTTOM,
    LEFT_RIGHT,
    RIGHT_LEFT,
}