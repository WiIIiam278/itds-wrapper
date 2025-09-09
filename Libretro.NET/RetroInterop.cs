using System;
using System.Runtime.InteropServices;
using Libretro.NET.Bindings;

namespace Libretro.NET;

public class RetroInterop : IDisposable
{
    private retro_environment_t _environment;
    private retro_video_refresh_t _videoRefresh;
    private retro_input_poll_t _inputPoll;
    private retro_input_state_t _inputState;
    private retro_audio_sample_t _audioSample;
    private retro_audio_sample_batch_t _audioSampleBatch;

    public retro_system_info retroSystemInfo;
    public retro_game_info retroGameInfo;
    public retro_system_av_info retroSystemAvInfo;

    private GCHandle? _environmentHandle;
    private GCHandle? _videoRefreshHandle;
    private GCHandle? _inputPollHandle;
    private GCHandle? _inputStateHandle;
    private GCHandle? _audioSampleHandle;
    private GCHandle? _audioSampleBatchHandle;

    public void set_environment(retro_environment_t param0)
    {
        _environment = param0;
        _environmentHandle = GCHandle.Alloc(_environment);
        RetroBindings.set_environment(_environment);
    }

    public void set_video_refresh(retro_video_refresh_t param0)
    {
        _videoRefresh = param0;
        _videoRefreshHandle = GCHandle.Alloc(_videoRefresh);
        RetroBindings.set_video_refresh(_videoRefresh);
    }

    public void set_input_poll(retro_input_poll_t param0)
    {
        _inputPoll = param0;
        _inputPollHandle = GCHandle.Alloc(_inputPoll);
        RetroBindings.set_input_poll(_inputPoll);
    }

    public void set_input_state(retro_input_state_t param0)
    {
        _inputState = param0;
        _inputStateHandle = GCHandle.Alloc(_inputState);
        RetroBindings.set_input_state(_inputState);
    }

    public void set_audio_sample(retro_audio_sample_t param0)
    {
        _audioSample = param0;
        _audioSampleHandle = GCHandle.Alloc(_audioSample);
        RetroBindings.set_audio_sample(_audioSample);
    }

    public void set_audio_sample_batch(retro_audio_sample_batch_t param0)
    {
        _audioSampleBatch = param0;
        _audioSampleBatchHandle = GCHandle.Alloc(_audioSampleBatch);
        RetroBindings.set_audio_sample_batch(_audioSampleBatch);
    }

    public void init()
    {
        RetroBindings.init();
    }

    public unsafe void get_system_info(ref retro_system_info param0)
    {
        retroSystemInfo = param0;

        fixed (retro_system_info* ptr = &param0)
        {
            RetroBindings.get_system_info(ptr);
        }
    }

    public unsafe byte load_game(ref retro_game_info param0)
    {
        retroGameInfo = param0;

        fixed (retro_game_info* ptr = &param0)
        {
            return RetroBindings.load_game(ptr);
        }
    }

    public unsafe void get_system_av_info(ref retro_system_av_info param0)
    {
        retroSystemAvInfo = param0;

        fixed (retro_system_av_info* ptr = &param0)
        {
            RetroBindings.get_system_av_info(ptr);
        }
    }

    public void run()
    {
        RetroBindings.run();
    }

    public void Dispose()
    {
        _environmentHandle?.Free();
        _videoRefreshHandle?.Free();
        _inputPollHandle?.Free();
        _inputStateHandle?.Free();
        _audioSampleHandle?.Free();
        _audioSampleBatchHandle?.Free();
    }
}