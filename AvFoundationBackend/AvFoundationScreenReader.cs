using AVFoundation;
using ITDSWrapper.Accessibility;

namespace AvFoundationBackend;

public class AvFoundationScreenReader : IScreenReader
{
    private AVSpeechSynthesizer? _synthesizer;
    private AVSpeechSynthesisVoice? _voice;

    public AvFoundationScreenReader(string language)
    {
        Initialize(language);
    }
    
    public bool Initialize(string language)
    {
        _synthesizer = new();
        _voice = AVSpeechSynthesisVoice.FromLanguage(language);
        return true;
    }

    public void Speak(string text)
    {
        using AVSpeechUtterance utterance = new(text);
        utterance.Voice = _voice;
        _synthesizer?.StopSpeaking(AVSpeechBoundary.Immediate);
        _synthesizer?.SpeakUtterance(utterance);
    }

    public void SetLanguage(string language)
    {
        _voice = AVSpeechSynthesisVoice.FromLanguage(language switch
        {
            "ja" => "ja",
            _ => "en-GB",
        });
    }
    
    public void Dispose()
    {
        _synthesizer?.Dispose();
        _voice?.Dispose();
    }
}