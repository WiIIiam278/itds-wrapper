using System;
using NAudio.Wave;

namespace ITDSWrapper.Audio;

public class StreamingWaveProvider : IWaveProvider
{
    private byte[] _buffer;
    private int _bufferedPosition;
    private int _samplePosition;

    public WaveFormat WaveFormat { get; }
    
    public int Distance => _samplePosition - _bufferedPosition;
    public int BufferLength => _buffer.Length;

    public StreamingWaveProvider(WaveFormat waveFormat)
    {
        WaveFormat = waveFormat;
        _buffer = new byte[waveFormat.SampleRate * waveFormat.Channels * 30];
    }

    public void AddSamples(byte[] buffer)
    {
        int count = buffer.Length;
        int offset = 0;
        if (_samplePosition + buffer.Length >= _buffer.Length)
        {
            count -= _buffer.Length - _samplePosition;
            Array.Copy(_buffer, _samplePosition, buffer, offset, _buffer.Length - _samplePosition);
            offset += _buffer.Length - _samplePosition;
            _samplePosition = 0;
        }
        Array.Copy(buffer, offset, _buffer, _samplePosition, count);
        _samplePosition += count;
    }
    
    public int Read(byte[] buffer, int offset, int count)
    {
        int toRead = count > _samplePosition - _bufferedPosition && _samplePosition - _bufferedPosition > 0
            ? _samplePosition - _bufferedPosition
            : count;
        int read = toRead;
        if (_bufferedPosition + toRead >= _buffer.Length)
        {
            toRead -= _buffer.Length - _bufferedPosition;
            Array.Copy(_buffer, _bufferedPosition, buffer, offset, _buffer.Length - _bufferedPosition);
            offset += _buffer.Length - _bufferedPosition;
            _bufferedPosition = 0;
        }
        Array.Copy(_buffer, _bufferedPosition, buffer, offset, toRead);
        _bufferedPosition += toRead;
        return read;
    }
}