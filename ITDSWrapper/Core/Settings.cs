using System.IO;
using System.Text.Json;

namespace ITDSWrapper.Core;

public class Settings
{
    public bool FirstLaunch { get; set; }
    public bool BordersEnabled { get; set; }
    public bool ScreenReaderEnabled { get; set; }

    public void Save(string path)
    {
        File.WriteAllText(Path.Join(path, "settings.json"), JsonSerializer.Serialize(this));
    }

    public static Settings? Load(string path)
    {
        return JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Join(path, "settings.json")));
    }
}