using Hardware.Info;
using ITDSWrapper.Core;

namespace ITDSWrapper.Desktop;

public class BatteryMonitor : IBatteryMonitor
{
    public uint GetBatteryLevel()
    {
        HardwareInfo info = new();
        info.RefreshBatteryList();
        if (info.BatteryList.Count == 0 || info.BatteryList[0].DesignCapacity == 0)
        {
            return 100;
        }

        return info.BatteryList[0].EstimatedChargeRemaining;
    }
}