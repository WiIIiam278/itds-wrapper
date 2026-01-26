using Android.Views;
using ITDSWrapper.Core;

namespace ITDSWrapper.Android;

public class AndroidUpdater(AndroidControllerInputDriver inputDriver) : IUpdater
{
    public int RetValue = -1;
    
    public int Update()
    {
        int[] deviceIds = InputDevice.GetDeviceIds() ?? [];
        foreach (int deviceId in deviceIds)
        {
            InputDevice? device = InputDevice.GetDevice(deviceId);
            if (device is not null && device.SupportsSource(InputSourceType.Gamepad) && !(inputDriver.Controller?.Equals(device) ?? false))
            {
                inputDriver.SetController(device);
                break;
            }
        }

        if (RetValue != -1)
        {
            int temp = RetValue;
            RetValue = -1;
            return temp;
        }

        return RetValue;
    }

    public void Die()
    {
    }
}