using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Foundation;
using Libretro.NET.Bindings;
using UIKit;

namespace ITDSWrapper.iOS;

public class Application
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        // Set up resolver path for melondsds_libretro
        NativeLibrary.SetDllImportResolver(Assembly.GetAssembly(typeof(RetroBindings))!, DllImportResolver);

        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }

    private static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        return libraryName.Equals("melondsds_libretro")
            ?
            // iOS needs to look in this frameworks directory
            NativeLibrary.Load(
                System.IO.Path.Combine(NSBundle.MainBundle.BundlePath, "Frameworks", "melondsds_libretro.framework",
                    "melondsds_libretro"), assembly, searchPath)
            : IntPtr.Zero;
    }
}