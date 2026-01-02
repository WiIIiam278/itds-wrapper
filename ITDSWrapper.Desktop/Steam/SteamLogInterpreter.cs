using System;
using Avalonia.Threading;
using ITDSWrapper.Core;
using ITDSWrapper.Input;
using Libretro.NET.Bindings;
using Steamworks;

namespace ITDSWrapper.Desktop.Steam;

public class SteamLogInterpreter(SteamInputDriver inputDriver, InputSwitcher inputSwitcher) : LogInterpreter
{
    private const string ActionSetVerb = "ACTION_SET";
    private const string CloudSaveVerb = "CLOUD_SAVE";
    private const string RichPresenceVerb = "RICH_PRESENCE";
    private const string TimelineInstantaneousEventVerb = "TIMELINE_EVENT_I";
    private const string TimelineRangeEventVerb = "TIMELINE_EVENT_R";
    private const string InputChangeStartVerb = "INPUT_CHANGE_START";
    private const string InputChangeRequestVerb = "INPUT_CHANGE_REQUEST";
    private const string InputChangeCompleteVerb = "INPUT_CHANGE_COMPLETE";

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
                Dispatcher.UIThread.InvokeAsync(SteamSaveManager.UploadCloudSave);
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
            
            case InputChangeStartVerb:
                inputSwitcher.SetInputDelegate((_, _, _, id) =>
                {
                    return id switch
                    {
                        RetroBindings.RETRO_DEVICE_ID_JOYPAD_UP or RetroBindings.RETRO_DEVICE_ID_JOYPAD_DOWN => 1,
                        _ => 0,
                    };
                });
                break;
            
            case InputChangeRequestVerb:
                uint[] glyphAction = inputDriver.GetActionGlyphId(log[(endIndex + 2)..^1]);
                int numInputs = 0;
                int numInputsToSend = glyphAction.Length * 2;
                inputSwitcher.SetInputDelegate((_, _, _, id) =>
                {
                    if (numInputs < numInputsToSend && id == glyphAction[numInputs / 2])
                    {
                        numInputs++;
                        return 1;
                    }
                    return 0;
                });
                break;
            
            case InputChangeCompleteVerb:
                inputSwitcher.ResetInputDelegate();
                break;
        }

        return wrapperPrefixLocation;
    }
}