using System.Collections.Generic;
using Steamworks;
using ITDSWrapper.Core;

namespace ITDSWrapper.Desktop.Steam;

public class SteamUpdater : IUpdater
{
    private readonly List<Controller> _controllers = [];

    public SteamUpdater()
    {
        SteamInput.Init();
    }
    
    public void Update()
    {
        SteamInput.GetControllerNoAlloc(_controllers);
        if (_controllers.Count > 0)
        {
            
        }
    }
}