using System;
using CrossSpeak;

namespace ITDSWrapper.Desktop;

public class ScreenReader : IDisposable
{
    public static ScreenReader? Initialize()
    {
        if (CrossSpeakManager.Instance.Initialize())
        {
            return new();
        }

        return null;
    }

    public void Speak(string str)
    {
        CrossSpeakManager.Instance.Speak(str);
    }

    public void Dispose()
    {
        CrossSpeakManager.Instance.Close();
    }
}