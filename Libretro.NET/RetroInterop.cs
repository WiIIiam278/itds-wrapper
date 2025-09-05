using System.Runtime.InteropServices;
using Libretro.NET.Bindings;

namespace Libretro.NET;

public class RetroInterop
{
    public void set_environment(retro_environment_t param0)
    {
        GCHandle.Alloc(param0);
        RetroBindings.set_environment(param0);
    }

    public void set_video_refresh(retro_video_refresh_t param0)
    {
        GCHandle.Alloc(param0);
        RetroBindings.set_video_refresh(param0);
    }

    public void set_input_poll(retro_input_poll_t param0)
    {
        GCHandle.Alloc(param0);
        RetroBindings.set_input_poll(param0);
    }

    public void set_input_state(retro_input_state_t param0)
    {
        GCHandle.Alloc(param0);
        RetroBindings.set_input_state(param0);
    }

    public void set_audio_sample(retro_audio_sample_t param0)
    {
        GCHandle.Alloc(param0);
        RetroBindings.set_audio_sample(param0);
    }

    public void set_audio_sample_batch(retro_audio_sample_batch_t param0)
    {
        GCHandle.Alloc(param0);
        RetroBindings.set_audio_sample_batch(param0);
    }

    public void init()
    {
        RetroBindings.init();
    }

    public unsafe void get_system_info(ref retro_system_info param0)
    {
        fixed (retro_system_info* ptr = &param0)
        {
            RetroBindings.get_system_info(ptr);
        }
    }

    public unsafe byte load_game(ref retro_game_info param0)
    {
        fixed (retro_game_info* ptr = &param0)
        {
            return RetroBindings.load_game(ptr);
        }
    }

    public unsafe void get_system_av_info(ref retro_system_av_info param0)
    {
        fixed (retro_system_av_info* ptr = &param0)
        {
            RetroBindings.get_system_av_info(ptr);
        }
    }

    public void run()
    {
        RetroBindings.run();
    }
}