using System;

namespace ITDSWrapper.Accessibility;

public interface IScreenReader : IDisposable
{
    public bool Initialize();
    public void Speak(string text);
}