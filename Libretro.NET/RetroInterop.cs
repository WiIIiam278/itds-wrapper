using System;
using System.Runtime.InteropServices;
using Libretro.NET.Bindings;

namespace Libretro.NET;

public class RetroInterop
{
    public GCHandle EnvironmentHandle { get; set; }
    public retro_video_refresh_t VideoRefresh { get; set; }
    public retro_input_poll_t InputPoll { get; set; }
    public retro_input_state_t InputState { get; set; }
    public retro_audio_sample_t AudioSample { get; set; }
    public retro_audio_sample_batch_t AudioSampleBatch { get; set; }

    public retro_system_info RetroSystemInfo { get; set; }
    public retro_game_info RetroGameInfo { get; set; }
    public retro_system_av_info RetroSystemAvInfo { get; set; }

    public void set_environment(retro_environment_t param0)
    {
        EnvironmentHandle = GCHandle.Alloc(param0);
        RetroBindings.set_environment(param0);
    }

    public void set_video_refresh(retro_video_refresh_t param0)
    {
        VideoRefresh = param0;
        RetroBindings.set_video_refresh(VideoRefresh);
    }

    public void set_input_poll(retro_input_poll_t param0)
    {
        InputPoll = param0;
        RetroBindings.set_input_poll(InputPoll);
    }

    public void set_input_state(retro_input_state_t param0)
    {
        InputState = param0;
        RetroBindings.set_input_state(InputState);
    }

    public void set_audio_sample(retro_audio_sample_t param0)
    {
        AudioSample = param0;
        RetroBindings.set_audio_sample(AudioSample);
    }

    public void set_audio_sample_batch(retro_audio_sample_batch_t param0)
    {
        AudioSampleBatch = param0;
        RetroBindings.set_audio_sample_batch(AudioSampleBatch);
    }

    public void init()
    {
        RetroBindings.init();
    }

    public unsafe void get_system_info(ref retro_system_info param0)
    {
        RetroSystemInfo = param0;

        fixed (retro_system_info* ptr = &param0)
        {
            RetroBindings.get_system_info(ptr);
        }
    }

    public unsafe byte load_game(ref retro_game_info param0)
    {
        RetroGameInfo = param0;

        fixed (retro_game_info* ptr = &param0)
        {
            return RetroBindings.load_game(ptr);
        }
    }

    public unsafe void get_system_av_info(ref retro_system_av_info param0)
    {
        RetroSystemAvInfo = param0;

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