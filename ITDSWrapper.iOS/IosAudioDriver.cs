using AVFoundation;

namespace ITDSWrapper.iOS;

public class IosAudioDriver : AvFoundationBackend.AvFoundationAudioBackend
{

    public override void Initialize(double sampleRate)
    {
        var session = AVAudioSession.SharedInstance();
        session.SetCategory(AVAudioSessionCategory.Playback);
        base.Initialize(sampleRate);
    }

}