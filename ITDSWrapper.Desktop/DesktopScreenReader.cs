using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia.Threading;
using ITDSWrapper.Accessibility;

namespace ITDSWrapper.Desktop;

public unsafe partial class DesktopScreenReader : IScreenReader
{
    private List<GCHandle> _handles = [];

    public static DesktopScreenReader? Instantiate(string language)
    {
        try
        {
            DesktopScreenReader reader = new();
            if (reader.Initialize(GetPlatformSpecificLanguageCode(language)))
            {
                return reader;
            }

            reader.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize screen reader: {ex.Message}");
        }

        return null;
    }

    public bool Initialize(string language)
    {
#if IS_LINUX
        bool success = Initialize(EspeakAudioOutput.AUDIO_OUTPUT_PLAYBACK, 0, null, 0) != -1;
        success = success && SetLanguage(language) == 0;

        return success;
#elif IS_MACOS
#else
#endif
        return false;
    }

    public void Speak(string text)
    {
#if IS_LINUX
        if (IsPlaying() == 1)
        {
            _ = Cancel();
        }
        _ = Synthesize(text, text.Length, 0, EspeakPositionType.POS_CHARACTER, 0, 1,
            IntPtr.Zero, IntPtr.Zero);
#elif IS_MACOS
#else
#endif
    }

    public void Dispose()
    {
        foreach (GCHandle handle in _handles)
        {
            handle.Free();
        }
#if IS_LINUX
#elif IS_MACOS
#else
#endif
    }

    private static string GetPlatformSpecificLanguageCode(string language)
    {
        return language switch
        {
#if IS_LINUX
            _ => "en",
#elif IS_MACOS
            _ => "en",
#else
            _ => "en",
#endif
        };
    }

#if IS_LINUX

    private struct EspeakErrorContext
    {
        public uint type;
        public IntPtr name;
        public int version;
        public int expected_version;
    }

    private enum EspeakPositionType
    {
        POS_CHARACTER = 1,
        POS_WORD,
        POS_SENTENCE,
    }

    private enum EspeakAudioOutput
    {
        /// <summary>
        /// PLAYBACK mode: plays the audio data, supplies events to the calling program
        /// </summary>
        AUDIO_OUTPUT_PLAYBACK,

        /// <summary>
        /// RETRIEVAL mode: supplies audio data and events to the calling program
        /// </summary>
        AUDIO_OUTPUT_RETRIEVAL,

        /// <summary>
        /// SYNCHRONOUS mode: as RETRIEVAL but doesn't return until synthesis is completed
        /// </summary>
        AUDIO_OUTPUT_SYNCHRONOUS,

        /// <summary>
        /// Synchronous playback
        /// </summary>
        AUDIO_OUTPUT_SYNCH_PLAYBACK,
    }

    [LibraryImport("espeak-ng.so.1", EntryPoint = "espeak_Initialize", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int Initialize(EspeakAudioOutput output, int bufferLength, string? path, int options);

    [LibraryImport("espeak-ng.so.1", EntryPoint = "espeak_SetVoiceByName",  StringMarshalling = StringMarshalling.Utf8)]
    private static partial uint SetLanguage(string name);

    [LibraryImport("espeak-ng.so.1", EntryPoint = "espeak_Synth", StringMarshalling = StringMarshalling.Utf8)]
    private static partial uint Synthesize(string text, nint size, uint position, EspeakPositionType type,
        uint endPosition, uint flags, IntPtr uniqueIdentifier, IntPtr userData);

    [LibraryImport("espeak-ng.so.1", EntryPoint = "espeak_Cancel")]
    private static partial uint Cancel();

    [LibraryImport("espeak-ng.so.1", EntryPoint = "espeak_IsPlaying")]
    private static partial int IsPlaying();

#else
    private static void InitializeInternal(IntPtr context);
#endif
}