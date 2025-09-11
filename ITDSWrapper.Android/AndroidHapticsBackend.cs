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
        FeedbackConstants type = press ? FeedbackConstants.VirtualKey : FeedbackConstants.VirtualKeyRelease;
        View?.PerformHapticFeedback(type);
    }
}