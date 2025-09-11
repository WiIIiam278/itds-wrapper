using System.Collections.Generic;
using Steamworks;
using ITDSWrapper.Core;

namespace ITDSWrapper.Desktop.Steam;

public class SteamUpdater : IUpdater
{
    private readonly List<Controller> _controllers = [];
    
    private SteamInputDriver _inputDriver;

    public SteamUpdater(SteamInputDriver inputDriver)
    {
        _inputDriver = inputDriver;
    }
    
    public void Update()
    {
        SteamInput.GetControllerNoAlloc(_controllers);
        if (_controllers.Count > 0)
        {
            _inputDriver.SetController(_controllers[0]);
        }
    }
}