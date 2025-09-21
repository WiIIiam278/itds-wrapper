using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ITDSWrapper.Accessibility;

namespace ITDSWrapper.Desktop;

public unsafe class DesktopScreenReader : IScreenReader
{
    private List<GCHandle> _handles = [];

    public static DesktopScreenReader? Instantiate()
    {
        DesktopScreenReader reader = new();
        if (reader.Initialize())
        {
            return reader;
        }
        reader.Dispose();
        return null;
    }
    
    public bool Initialize()
    {
#if IS_LINUX
        EspeakErrorContext context = new();
        GCHandle handle = GCHandle.Alloc(context, GCHandleType.Pinned);
        InitializeInternal(handle.AddrOfPinnedObject());
        handle.Free();
        return true;
#elif IS_MACOS
#else
#endif
        return false;
    }

    public void Speak(string text)
    {
#if IS_LINUX
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
    
#if IS_LINUX
    internal struct EspeakErrorContext
    {
        public uint type;
        public IntPtr name;
        public int version;
        public int expected_version;
    }
    
    [DllImport("espeak-ng", EntryPoint = "espeak_ng_Initialize")]
    private static extern uint InitializeInternal(IntPtr context);
#else
    private static void InitializeInternal(IntPtr context);
#endif
}