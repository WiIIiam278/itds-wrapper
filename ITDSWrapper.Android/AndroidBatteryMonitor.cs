using Android.OS;
using ITDSWrapper.Core;

namespace ITDSWrapper.Android;

public class AndroidBatteryMonitor : IBatteryMonitor
{
    public uint GetBatteryLevel()
    {
        using BatteryManager batteryManager = new();
        return (uint)batteryManager.GetIntProperty((int)BatteryProperty.Capacity);
    }
}