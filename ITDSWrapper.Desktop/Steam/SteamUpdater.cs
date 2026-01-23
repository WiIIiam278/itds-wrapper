using ITDSWrapper.Core;

namespace ITDSWrapper.Desktop.Steam;

public class SteamUpdater(SteamInputDriver inputDriver, SteamHelperIpc ipc) : IUpdater
{
    public int Update()
    {
        ipc.SendCommand("INPUT_POLL_CONTROLLERS");
        byte[] receipt = ipc.ReceiveResponse();
        if (receipt.Length > 0 && receipt[0] == 1)
        {
            if (receipt[1] == 1)
            {
                inputDriver.RequestInputUpdate = true;
            }
            
            if (inputDriver.UpdateState())
            {
                return 1;
            }
        }

        return -1;
    }
}