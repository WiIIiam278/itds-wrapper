using System;
using NAudio.Wave;
using Silk.NET.OpenAL;

namespace ITDSWrapper.Audio;

public class NAudioSilkNetOpenALBackend : IAudioBackend
{
    private readonly WaveFormat _waveFormat;
    private readonly BufferFormat _sourceALFormat;        // 8 or 16 bit buffer

    private int _currentBuffer;

    /// <summary>
    /// Gets or sets the Device
    /// Should be set before a call to InitPlayer
    /// </summary>
    public string? DeviceName { get; set; }
    
    /// <summary>
    /// Gets or sets the number of buffers used
    /// Should be set before a call to InitPlayer
    /// </summary>
    public int NumberOfBuffers { get; set; }

    private unsafe Device* _device;
    public ALContext? Alc { get; private set; }
    public AL? Al { get; private set; }
    
    private unsafe Context* _context;
    
    //private int _alSource;
    private uint _alSource;

    //private int[] alBuffers;
    private uint[]? _alBuffers;

    // private byte[]? _sourceBuffer;
    
    public unsafe NAudioSilkNetOpenALBackend(double sampleRate, int numBuffers)
    {
        NumberOfBuffers = numBuffers;
        
        _waveFormat = new((int)sampleRate, 2);
        _sourceALFormat = GetBufferFormat(_waveFormat);
        
        Alc = ALContext.GetApi(soft: true);
        Al = AL.GetApi(soft: true);


        _device = Alc.OpenDevice(DeviceName);

        _context = Alc.CreateContext(_device, null);

        Alc.MakeContextCurrent(_context);

        //Al.GenSource(out _alSource);
        _alSource = Al.GenSource();

        //Al.Source(_alSource, ALSourcef.Gain, 1f);
        Al.SetSourceProperty(_alSource, SourceFloat.Gain, 1f);

        //alBuffers = new int[NumberOfBuffers];
        _alBuffers = new uint[NumberOfBuffers];
        for (var i = 0; i < NumberOfBuffers; i++)
        {
            //AL.GenBuffer(out alBuffers[i]);
            _alBuffers[i] = Al.GenBuffer();
        }
        
        // _sourceBuffer = new byte[_bufferSizeByte];
    }

    public void Initialize(double sampleRate)
    {
    }

    public void TogglePause()
    {
    }

    public void PlaySamples(byte[] samples)
    {
        ReadAndQueueBuffers(_alBuffers!, samples);
        
        Al!.GetSourceProperty(_alSource, GetSourceInteger.BuffersProcessed, out var processed);


        //AL.GetSource(_alSource, ALGetSourcei.SourceState, out state);
        Al!.GetSourceProperty(_alSource, GetSourceInteger.SourceState, out var state);


        if (processed > 0) //there are processed buffers
        {
            //unqueue
            //int[] unqueueBuffers = AL.SourceUnqueueBuffers(_alSource, processed);

            var unqueueBuffers = new uint[processed];
            Al.SourceUnqueueBuffers(_alSource, unqueueBuffers);
        }

        if ((SourceState)state != SourceState.Playing)
        {
            //AL.SourcePlay(_alSource);
            Al.SourcePlay(_alSource);
        }
    }

    private BufferFormat GetBufferFormat(WaveFormat format)
    {
        if (format.Channels == 2)
        {
            return BufferFormat.Stereo16;
        }
        else if (format.Channels == 1)
        {
            return BufferFormat.Mono16;
        }
        throw new FormatException("Cannot translate WaveFormat.");
    }
    
    private void ReadAndQueueBuffers(uint[] alBuffers, byte[] samples)
    {
        Al!.BufferData(alBuffers[_currentBuffer], _sourceALFormat, samples, _waveFormat.SampleRate);

        // AL.SourceQueueBuffer(_alSource, alBuffers[i]);
        uint[] buffersToQueue = [alBuffers[_currentBuffer]];
        Al!.SourceQueueBuffers(_alSource, buffersToQueue);
        
        _currentBuffer = _currentBuffer == alBuffers.Length - 1 ? 0 : _currentBuffer + 1;
    }
}