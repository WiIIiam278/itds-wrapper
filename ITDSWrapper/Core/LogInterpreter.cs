using System;

namespace ITDSWrapper.Core;

public class LogInterpreter
{
    protected const string WrapperLogPrefix = "[WRAPPER] ";
    public bool WatchForSdCreate { get; set; }

    private const string AchievementUnlockedVerb = "ACHIEVEMENT_UNLOCKED";
    
    public IAchievementManager? AchievementManager { get; set; }
    
    public virtual int InterpretLog(string log)
    {
        // The SD card is initialized after the game boots -- if we replace the SD card image here, it will load properly
        int wrapperPrefixLocation = WatchForSdCreate && log.Contains("[melonDS] Game is now booting") ? 0 :
            log.IndexOf(WrapperLogPrefix, StringComparison.Ordinal);
        if (wrapperPrefixLocation < 0)
        {
            return wrapperPrefixLocation;
        }
        
        int startIndex = wrapperPrefixLocation + WrapperLogPrefix.Length;
        int endIndex = log.IndexOf(':', startIndex);
        string verb = log[startIndex..endIndex];

        switch (verb)
        {
            case AchievementUnlockedVerb:
                AchievementManager?.Unlock(log[(endIndex + 2)..^1]);
                break;
        }

        return wrapperPrefixLocation;
    }
}