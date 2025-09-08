using System.Collections.Generic;
using ITDSWrapper.Audio;

namespace ITDSWrapper.Core;

public class EmulationDriver
{
    private readonly Stack<bool> _pauseStack = [];
    
    public IAudioBackend? AudioBackend { get; set; }

    public bool IsPaused() => _pauseStack.Count > 0 && _pauseStack.Peek();

    public void PushPauseState(bool pause)
    {
        if (pause)
        {
            if (!IsPaused())
            {
                AudioBackend?.TogglePause();
            }
            _pauseStack.Push(true);
        }
        else
        {
            if (IsPaused())
            {
                AudioBackend?.TogglePause();
            }
            if (_pauseStack.Count > 0)
            {
                _pauseStack.Pop();
            }
        }
    }
}