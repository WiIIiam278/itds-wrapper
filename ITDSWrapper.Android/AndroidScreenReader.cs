using System;
using Android.OS;
using Android.Speech.Tts;
using ITDSWrapper.Accessibility;
using Java.Util;

namespace ITDSWrapper.Android;

public class AndroidScreenReader : Java.Lang.Object, IScreenReader, TextToSpeech.IOnInitListener
{
    private TextToSpeech? _tts;
    private Locale? _language;
    private int _currentId;
    
    public AndroidScreenReader(MainActivity activity, string language)
    {
        Initialize(language);
        _tts = new(activity.ApplicationContext, this);
    }
    
    public bool Initialize(string language)
    {
        _language = language switch
        {
            _ => Locale.Uk,
        };
        return true;
    }

    public void Speak(string text)
    {
        _tts?.Speak(text, QueueMode.Flush, Bundle.Empty, $"tts_{_currentId++}");
    }

    public void SetLanguage(string language)
    {
        _language = language switch
        {
            _ => Locale.Uk,
        };
        LanguageAvailableResult result = _tts?.SetLanguage(_language) ?? LanguageAvailableResult.NotSupported;
        if (result is LanguageAvailableResult.MissingData or LanguageAvailableResult.NotSupported)
        {
            Console.WriteLine("Language is not available");
            Dispose();
        }
    }
    
    public new void Dispose()
    {
        _tts?.Shutdown();
        _tts?.Dispose();
        _tts = null;
        GC.SuppressFinalize(this);
    }
    
    public void OnInit(OperationResult status)
    {
        if (status == OperationResult.Success && _tts is not null)
        {
            LanguageAvailableResult result = _tts.SetLanguage(_language);
            if (result is LanguageAvailableResult.MissingData or LanguageAvailableResult.NotSupported)
            {
                Console.WriteLine("Language is not available");
                Dispose();
            }
        }
        else
        {
            Console.WriteLine("Failed to initialize text to speech!");
            Dispose();
        }
    }
}