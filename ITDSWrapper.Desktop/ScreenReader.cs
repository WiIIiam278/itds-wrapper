using System;
using CrossSpeak;

namespace ITDSWrapper.Desktop;

public class ScreenReader : IDisposable
{
    public ScreenReader()
    {
        CrossSpeakManager.Instance.TrySAPI(true);
        CrossSpeakManager.Instance.Initialize();
    }

    public void Speak(string str, bool interrupt)
    {
        CrossSpeakManager.Instance.Speak(str, interrupt);
        if (OperatingSystem.IsWindows())
        {
            CrossSpeakManager.Instance.Braille(str);
        }
    }

    public void Dispose()
    {
        CrossSpeakManager.Instance.Close();
    }
}