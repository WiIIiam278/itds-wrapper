using System;
using System.IO;
using System.Runtime.InteropServices;
using Libretro.NET.Bindings;

namespace Libretro.NET
{
    /// <summary>
    /// Wraps all (most? (necessary?)) libretro mechanisms used to run a core and a game.
    /// After creation, <see cref="LoadCore()"/> and then <see cref="LoadGame(byte[])"/> must be called before anything else.
    /// </summary>
    public unsafe class RetroWrapper : IDisposable
    {
        private RetroInterop _interop;
        
        private static retro_log_printf_t _log;
        private static retro_set_rumble_state_t _setRumbleState;
        
        private static GCHandle? _logHandle;
        private static GCHandle? _setRumbleStateHandle;

        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public double FPS { get; private set; }
        public double SampleRate { get; private set; }
        public uint BatteryLevel { get; set; } = 100;
        public static retro_pixel_format PixelFormat { get; private set; }

        public delegate void OnFrameDelegate(byte[] frame, uint width, uint height);

        public OnFrameDelegate OnFrame { get; set; }

        public delegate void OnSampleDelegate(byte[] sample);

        public OnSampleDelegate OnSample { get; set; }

        public delegate short OnCheckInputDelegate(uint port, uint device, uint index, uint id);

        public OnCheckInputDelegate OnCheckInput { get; set; }

        public delegate bool OnRumbleDelegate(uint port, uint effect, ushort strength);
        
        public OnRumbleDelegate OnRumble { get; set; }

        public delegate void OnReceiveLogDelegate(string line);
        
        public OnReceiveLogDelegate OnReceiveLog { get; set; }

        public void LoadCore()
        {
            _interop = new();
            
            _interop.set_environment(Environment);
            _interop.set_video_refresh(VideoRefresh);
            _interop.set_input_poll(InputPoll);
            _interop.set_input_state(InputState);
            _interop.set_audio_sample(AudioSample);
            _interop.set_audio_sample_batch(AudioSampleBatch);
            _interop.init();
        }

        public bool LoadGame(byte[] gameData)
        {
            var system = new retro_system_info();
            _interop.get_system_info(ref system);

            string fakePath = "itds.nds";
            retro_game_info game = new()
            {
                path = (sbyte*)Marshal.StringToHGlobalAnsi(fakePath),
                size = (UIntPtr)gameData.Length,
            };

            game.data = (void*)Marshal.AllocHGlobal((int)game.size);
            Marshal.Copy(gameData, 0, (IntPtr)game.data, (int)game.size);

            byte result = _interop.load_game(ref game);

            var av = new retro_system_av_info();
            _interop.get_system_av_info(ref av);

            Width = av.geometry.base_width;
            Height = av.geometry.base_height;
            FPS = av.timing.fps;
            SampleRate = av.timing.sample_rate;

            return result == 1;
        }

        public void Run()
        {
            _interop.run();
        }

        private byte Environment(uint cmd, void* data)
        {
            switch (cmd)
            {
                case RetroBindings.RETRO_ENVIRONMENT_GET_SYSTEM_DIRECTORY:
                {
                    string sysDir = GetDirectoryForPlatform("sys");
                    if (!Directory.Exists(sysDir))
                    {
                        Directory.CreateDirectory(sysDir!);
                    }
                    char** cb = (char**)data;
                    *cb = (char*)Marshal.StringToHGlobalAnsi(sysDir);
                    return 1;
                }
                case RetroBindings.RETRO_ENVIRONMENT_SET_PIXEL_FORMAT:
                {
                    PixelFormat = (retro_pixel_format)(*(byte*)data);
                    return 1;
                }
                case RetroBindings.RETRO_ENVIRONMENT_GET_VARIABLE:
                {
                    string key = Marshal.PtrToStringUTF8((IntPtr)(*(char **)data));
                    retro_variable* cb = (retro_variable*)data;
                    switch (key)
                    {
                        case "melonds_homebrew_sdcard":
                            *cb = new()
                            {
                                key = (sbyte*)Marshal.StringToHGlobalAnsi(key),
                                value = (sbyte*)Marshal.StringToHGlobalAnsi("enabled"),
                            };
                            break;
                        case "melonds_jit_enable":
                        case "melonds_jit_branch_optimisations":
                        case "melonds_jit_literal_optimisations":
                        case "melonds_show_cursor":
                        case "melonds_homebrew_sync_sdcard_to_host":
                        case "melonds_homebrew_readonly":
                            *cb = new()
                            {
                                key = (sbyte*)Marshal.StringToHGlobalAnsi(key),
                                value = (sbyte*)Marshal.StringToHGlobalAnsi("disabled"),
                            };
                            break;
                        case "melonds_slot2_device":
                            *cb = new()
                            {
                                key = (sbyte*)Marshal.StringToHGlobalAnsi(key),
                                value = (sbyte*)Marshal.StringToHGlobalAnsi("rumble-pak"),
                            };
                            break;
                    }
                    return 1;
                }
                case RetroBindings.RETRO_ENVIRONMENT_GET_CAN_DUPE:
                {
                    return *(byte*)data = 1;
                }
                case RetroBindings.RETRO_ENVIRONMENT_SET_FRAME_TIME_CALLBACK:
                {
                    retro_frame_time_callback* cb = (retro_frame_time_callback*)data;
                    return 1;
                }
                case RetroBindings.RETRO_ENVIRONMENT_GET_RUMBLE_INTERFACE:
                {
                    retro_rumble_interface* cb = (retro_rumble_interface*)data;
                    _setRumbleState = Rumble;
                    _setRumbleStateHandle = GCHandle.Alloc(_setRumbleState);
                    cb->set_rumble_state = Marshal.GetFunctionPointerForDelegate(_setRumbleState);
                    return 1;
                }
                case RetroBindings.RETRO_ENVIRONMENT_GET_LOG_INTERFACE:
                {
                    retro_log_callback* cb = (retro_log_callback*)data;

                    _log = Log;
                    _logHandle = GCHandle.Alloc(_log);
                    cb->log = Marshal.GetFunctionPointerForDelegate(_log);
                    return 1;
                }
                case RetroBindings.RETRO_ENVIRONMENT_GET_SAVE_DIRECTORY:
                {
                    string saveDir = GetDirectoryForPlatform("saves");
                    if (!Directory.Exists(saveDir))
                    {
                        Directory.CreateDirectory(saveDir!);
                    }
                    char** cb = (char**)data;
                    *cb = (char*)Marshal.StringToHGlobalAnsi(saveDir);
                    return 1;
                }
                case RetroBindings.RETRO_ENVIRONMENT_GET_CORE_OPTIONS_VERSION:
                {
                    *(uint*)data = 1;
                    return 1;
                }
                case RetroBindings.RETRO_ENVIRONMENT_SET_CORE_OPTIONS:
                {
                    return 1;
                }
                case RetroBindings.RETRO_ENVIRONMENT_SET_CORE_OPTIONS_DISPLAY:
                {
                    retro_core_option_display s = Marshal.PtrToStructure<retro_core_option_display>((IntPtr)data);
                    string value = Marshal.PtrToStringUTF8((IntPtr)((char *)s.key));
                    return 1;
                }
                case RetroBindings.RETRO_ENVIRONMENT_GET_DEVICE_POWER:
                {
                    if (data == null)
                    {
                        return 1;
                    }

                    *(uint*)data = BatteryLevel;
                    return 1;
                }
                default:
                {
                    return 0;
                }
            }
        }

        private void VideoRefresh(void* data, uint width, uint height, UIntPtr pitch)
        {
            byte[] raw = new byte[(uint)pitch * height];
            Marshal.Copy((IntPtr)data, raw, 0, (int)pitch * (int)height);

            byte[] result = new byte[width * 4 * height];
            var destinationIndex = 0;
            for (var sourceIndex = 0; sourceIndex < (uint)pitch * height; sourceIndex += (int)pitch)
            {
                Array.Copy(raw, sourceIndex, result, destinationIndex, width * 4);
                destinationIndex += (int)width * 4;
            }

            OnFrame?.Invoke(result, width, height);
        }

        private void InputPoll()
        {
            //Am I supposed to do something?
        }

        private short InputState(uint port, uint device, uint index, uint id)
        {
            return OnCheckInput?.Invoke(port, device, index, id) ?? 0;
        }

        private void AudioSample(short left, short right)
        {
            var count = 2;
            var audio = new byte[count * 2];
            var data = Marshal.AllocHGlobal(count * 2);

            Marshal.Copy(new[] { left, right }, 0, data, 0);
            Marshal.Copy(data, audio, 0, count * 2);

            OnSample?.Invoke(audio);
        }

        private UIntPtr AudioSampleBatch(short* data, UIntPtr frames)
        {
            var count = (int)frames * 2;
            var audio = new byte[count * 2];

            Marshal.Copy((IntPtr)data, audio, 0, count * 2);

            OnSample?.Invoke(audio);

            return frames;
        }

        private void Log(retro_log_level level, sbyte* fmt)
        {
            string str = Marshal.PtrToStringUTF8((IntPtr)(char*)fmt);
            Console.Write(str);
            
            OnReceiveLog?.Invoke(str);
        }

        private bool Rumble(uint port, retro_rumble_effect effect, ushort strength)
        {
            if (strength > 0)
            {
                return OnRumble?.Invoke(port, (uint)effect, strength) ?? false;
            }

            return false;
        }

        private void Time(long usec)
        {
            // Nothing relevant to do yet...
        }

        public static string GetDirectoryForPlatform(string dirName)
        {
            return OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() ?
                Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "itds", dirName) :
                OperatingSystem.IsMacOS() ?
                    Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.FullName)!.FullName)!.FullName, dirName) :
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dirName);
        }

        public void Dispose()
        {
            _interop?.Dispose();
            _logHandle?.Free();
            _setRumbleStateHandle?.Free();
        }
    }
}