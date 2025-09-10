using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ITDSWrapper.Audio;

namespace ITDSWrapper.Core;

public class PauseDriver
{
    private readonly Stack<bool> _pauseStack = [];
    
    public IAudioBackend? AudioBackend { get; set; }

    public bool IsPaused() => _pauseStack.Count > 0 && _pauseStack.Peek();

    public PauseDriver(bool useActivatableLifetime = false)
    {
        if (useActivatableLifetime && Application.Current!.TryGetFeature(typeof(IActivatableLifetime)) is
                { } activatableLifetime)
        {
            IActivatableLifetime activation = (IActivatableLifetime)activatableLifetime;
            activation.Activated += (_, _) =>
            {
                PushPauseState(false);
            };
            activation.Deactivated += (_, _) =>
            {
                PushPauseState(true);
            };
        }
    }
    
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