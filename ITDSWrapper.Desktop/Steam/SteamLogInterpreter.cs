using System;
using ITDSWrapper.Core;
using Steamworks;

namespace ITDSWrapper.Desktop.Steam;

public class SteamLogInterpreter(SteamInputDriver inputDriver) : LogInterpreter
{
    private const string ActionSetVerb = "ACTION_SET";
    private const string CloudSaveVerb = "CLOUD_SAVE";
    private const string AchievementUnlockedVerb = "ACHIEVEMENT_UNLOCKED";
    private const string RichPresenceVerb = "RICH_PRESENCE";
    private const string TimelineInstantaneousEventVerb = "TIMELINE_EVENT_I";
    private const string TimelineRangeEventVerb = "TIMELINE_EVENT_R";
    
    public override int InterpretLog(string log)
    {
        int wrapperPrefixLocation = base.InterpretLog(log);
        if (wrapperPrefixLocation < 0)
        {
            return wrapperPrefixLocation;
        }
        if (wrapperPrefixLocation == 0 && WatchForSdCreate)
        {
            WatchForSdCreate = false;
            SteamSaveManager.DownloadCloudSave();
            return -1;
        }

        int startIndex = wrapperPrefixLocation + WrapperLogPrefix.Length;
        int endIndex = log.IndexOf(':', startIndex);
        string verb = log[startIndex..endIndex];

        switch (verb)
        {
            case ActionSetVerb:
                inputDriver.SetActionSet(log[(endIndex + 2)..^1]);
                break;

            case CloudSaveVerb:
                SteamSaveManager.UploadCloudSave();
                break;
            
            case AchievementUnlockedVerb:
                break;
            
            case RichPresenceVerb:
                try
                {
                    string[] rpSplit = log[(endIndex + 2)..^1].Split('|');
                    SteamFriends.SetRichPresence(rpSplit[0], rpSplit[1]);
                    SteamFriends.SetRichPresence("steam_display", "#Status");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to set rich presence due to exception: {ex.Message}");
                }
                break;
            
            case TimelineInstantaneousEventVerb:
                try
                {
                    string[] timelineSplit = log[(endIndex + 2)..^1].Split('|');
                    SteamTimeline.AddInstantaneousTimelineEvent(timelineSplit[0], timelineSplit[1], timelineSplit[2],
                        uint.Parse(timelineSplit[3]), float.Parse(timelineSplit[4]), TimelineEventClipPriority.Standard);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to add instantaneous timeline event due to exception: {ex.Message}");
                }
                break;
            
            case TimelineRangeEventVerb:
                break;
        }

        return wrapperPrefixLocation;
    }
}