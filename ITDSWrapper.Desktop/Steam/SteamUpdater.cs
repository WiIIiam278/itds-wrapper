using System.Collections.Generic;
using Steamworks;
using ITDSWrapper.Core;

namespace ITDSWrapper.Desktop.Steam;

public class SteamUpdater(SteamInputDriver inputDriver) : IUpdater
{
    private readonly List<Controller> _controllers = [];

    public void Update()
    {
        SteamInput.GetControllerNoAlloc(_controllers);
        if (_controllers.Count > 0)
        {
            inputDriver.SetController(_controllers[0]);
            inputDriver.UpdateState();
        }
    }
}