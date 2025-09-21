using System;

namespace ITDSWrapper.Accessibility;

public interface IScreenReader : IDisposable
{
    public bool Initialize(string language);
    public void Speak(string text);
}