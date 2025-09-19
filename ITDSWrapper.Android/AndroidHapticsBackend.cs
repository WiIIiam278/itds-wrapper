using System;
using Android.Views;
using ITDSWrapper.Haptics;

namespace ITDSWrapper.Android;

public class AndroidHapticsBackend : IHapticsBackend
{
    public View? View { get; set; }
    
    public void Initialize()
    {
    }

    public void Fire(bool press)
    {
        FeedbackConstants type = OperatingSystem.IsAndroidVersionAtLeast(27)
            ? press ? FeedbackConstants.VirtualKey : FeedbackConstants.VirtualKeyRelease
            : FeedbackConstants.VirtualKey;
        View?.PerformHapticFeedback(type);
    }
}