using System;
using System.Text.RegularExpressions;
using ITDSWrapper.Accessibility;

namespace ITDSWrapper.Core;

public partial class LogInterpreter : IDisposable
{
    protected const string WrapperLogPrefix = "[WRAPPER] ";

    private const string AchievementUnlockedVerb = "ACHIEVEMENT_UNLOCKED";
    private const string BorderSetVerb = "BORDER_SET";
    private const string ScreenReaderVerb = "SCREEN_READER";
    private const string StartupReceivedVerb = "STARTUP_RECEIVED";
    private const string LanguageReceivedVerb = "LANG_RECEIVED";

    public bool WatchForSdCreate { get; set; }
    public Action<string>? SetNextBorder { get; set; }
    
    public IAchievementManager? AchievementManager { get; set; }
    public IScreenReader? ScreenReader { get; set; }
    
    public bool StartupReceived { get; private set; }
    public bool LangReceived { get; private set; }

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

        string logParam = log[(endIndex + 2)..^1];
        switch (verb)
        {
            case AchievementUnlockedVerb:
                AchievementManager?.Unlock(logParam);
                break;
            
            case BorderSetVerb:
                SetNextBorder?.Invoke(logParam);
                break;
            
            case ScreenReaderVerb:
                ScreenReader?.Speak(TextColorRegex().Replace(logParam, ""));
                break;
            
            case StartupReceivedVerb:
                StartupReceived = true;
                break;
            
            case LanguageReceivedVerb:
                LangReceived = true;
                break;
        }

        return wrapperPrefixLocation;
    }

    public void Dispose()
    {
        ScreenReader?.Dispose();
    }

    [GeneratedRegex(@"\@\d")]
    private static partial Regex TextColorRegex();
}