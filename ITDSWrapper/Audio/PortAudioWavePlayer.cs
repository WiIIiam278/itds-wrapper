// From here: https://github.com/haiyaku365/StimmingSignalGenerator/blob/master/StimmingSignalGenerator/NAudio/PortAudio/PortAudioWavePlayer.cs

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Utils;
using NAudio.Wave;
using Nito.AsyncEx;
using PortAudioSharp;

namespace ITDSWrapper.Audio;

class PortAudioWavePlayer : IWavePlayer
{
    public float Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public PlaybackState PlaybackState { get; private set; }

    public event EventHandler<StoppedEventArgs> PlaybackStopped;

    public int Latency { get; }

    public WaveFormat OutputWaveFormat => _sourceProvider.WaveFormat;
    private int _deviceIndex;
    private DeviceInfo _deviceInfo;

    private IWaveProvider _sourceProvider;
    private int _bufferSizeByte;
    private CircularBuffer _sourceBuffer;
    private byte[] _bufferWrite;
    private byte[] _bufferRead;
    private byte[] _bufferWriteLastestBlock;
    private AsyncAutoResetEvent _sourceBufferDequeuedEvent;

    private Stream _stream;
    public PortAudioWavePlayer(int latency)
    {
        PortAudio.Initialize();
        _deviceIndex = PortAudio.DefaultOutputDevice;
        _deviceInfo = PortAudio.GetDeviceInfo(_deviceIndex);
        Latency = latency;
        _sourceBufferDequeuedEvent = new(false);
    }

    public void Init(IWaveProvider waveProvider)
    {
        _sourceProvider = waveProvider;
        _bufferSizeByte = OutputWaveFormat.ConvertLatencyToByteSize(Latency);
        _sourceBuffer = new(_bufferSizeByte);
        _bufferWrite = new byte[_bufferSizeByte];
        _bufferRead = new byte[_bufferSizeByte];
        _bufferWriteLastestBlock = new byte[OutputWaveFormat.BlockAlign];

        var param = new StreamParameters
        {
            device = _deviceIndex,
            channelCount = OutputWaveFormat.Channels,
            sampleFormat = SampleFormat.Float32,
            suggestedLatency = _deviceInfo.defaultLowInputLatency,
            hostApiSpecificStreamInfo = IntPtr.Zero,
        };

        StreamCallbackResult Callback(
            IntPtr input, IntPtr output,
            UInt32 frameCount,
            ref StreamCallbackTimeInfo timeInfo,
            StreamCallbackFlags statusFlags,
            IntPtr userData
        )
        {
            int cnt = (int)frameCount * OutputWaveFormat.BlockAlign;
            var byteReadCnt = _sourceBuffer.Read(_bufferWrite, 0, cnt);
            _sourceBufferDequeuedEvent.Set();

            #region prevent wave jump when not enough buffer
            if (byteReadCnt >= OutputWaveFormat.BlockAlign)
            {
                // Copy latest data to use when not enough buffer.
                Array.Copy(
                    _bufferWrite, byteReadCnt - OutputWaveFormat.BlockAlign,
                    _bufferWriteLastestBlock, 0, OutputWaveFormat.BlockAlign);
            }
            while (byteReadCnt < cnt && byteReadCnt < _bufferWrite.Length)
            {
                // When running out of buffer data (Latency too low).
                // Fill the rest of buffer with latest data
                // so wave does not jump.
                _bufferWrite[byteReadCnt] = _bufferWriteLastestBlock[byteReadCnt % OutputWaveFormat.BlockAlign];
                byteReadCnt++;
            }
            #endregion

            Marshal.Copy(_bufferWrite, 0, output, byteReadCnt);
            return StreamCallbackResult.Continue;
        }
        _stream = new PortAudioSharp.Stream(
            inParams: null, outParams: param, sampleRate: _deviceInfo.defaultSampleRate,
            framesPerBuffer: 0,
            streamFlags: StreamFlags.NoFlag,
            callback: Callback,
            userData: IntPtr.Zero
        );
    }

    private readonly CancellationTokenSource _fillSourceBufferWorkerCts = new();
    private Task _fillSourceBufferWorker;
    private async Task FillSourceBufferTaskAsync()
    {
        _fillSourceBufferWorkerCts.TryReset();
        while (!_fillSourceBufferWorkerCts.Token.IsCancellationRequested)
        {
            var bufferSpace = _sourceBuffer.MaxLength - _sourceBuffer.Count;
            if (bufferSpace > 0)
            {
                FillSourceBuffer(bufferSpace);
            }
            await _sourceBufferDequeuedEvent.WaitAsync(_fillSourceBufferWorkerCts.Token);
        }
    }

    private void FillSourceBuffer(int bufferSpace)
    {
        _sourceProvider.Read(_bufferRead, 0, bufferSpace);
        _sourceBuffer.Write(_bufferRead, 0, bufferSpace);
    }

    public void Pause()
    {
        throw new NotImplementedException();
    }

    public void Play()
    {
        if (PlaybackState != PlaybackState.Playing)
        {
            FillSourceBuffer(_sourceBuffer.MaxLength);
            _fillSourceBufferWorker = Task.Factory.StartNew(
                function: FillSourceBufferTaskAsync,
                cancellationToken: CancellationToken.None,
                creationOptions:
                TaskCreationOptions.RunContinuationsAsynchronously |
                TaskCreationOptions.LongRunning,
                scheduler: TaskScheduler.Default);
            _stream.Start();
            PlaybackState = PlaybackState.Playing;
        }
    }

    public void Stop()
    {
        if (PlaybackState != PlaybackState.Stopped)
        {
            _stream.Stop();
            _fillSourceBufferWorkerCts.Cancel();
            while (!_stream.IsStopped && _fillSourceBufferWorker.Status != TaskStatus.Running) { Thread.Sleep(30); };
            _fillSourceBufferWorker.Dispose();
            PlaybackState = PlaybackState.Stopped;
            PlaybackStopped.Invoke(this, new StoppedEventArgs(null));
        }
    }

    private bool _disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // dispose managed state (managed objects)
                _stream.Close();
                _stream.Dispose();
            }

            // free unmanaged resources (unmanaged objects) and override finalizer
            // set large fields to null
            Array.Clear(_bufferWrite);
            _bufferWrite = null;
            Array.Clear(_bufferRead);
            _bufferRead = null;
            _disposedValue = true;
        }
    }

    // // override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~PortAudioWavePlayer()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}