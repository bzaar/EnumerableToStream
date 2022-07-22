using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EnumerableToStream;

class StreamOverEnumerable : Stream
{
    private int _currentIndex;
    private IEnumerator<string?>? _enumerator;
    private readonly Encoder _encoder;

    public StreamOverEnumerable(IEnumerable<string?> input, Encoder encoder)
    {
        _encoder = encoder;
        _enumerator = input.GetEnumerator();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_enumerator == null)
        {
            throw new ObjectDisposedException($"The {nameof(StreamOverEnumerable)} has been disposed.");
        }

        int totalBytesRead = 0;
        bool spaceInBuffer = true;

        while (spaceInBuffer && totalBytesRead < count && (_currentIndex != 0 || _enumerator.MoveNext()))
        {
            string? current = _enumerator.Current;
            if (current == null || current.Length == 0) continue;
#if SPANS_SUPPORTED
                _encoder.Convert(current.AsSpan(_currentIndex),  
                    buffer.AsSpan(offset + totalBytesRead, count - totalBytesRead),
                    false, out int charsUsed, out int bytesUsed, out spaceInBuffer);
#else
            _encoder.Convert(current.ToCharArray(), _currentIndex, current.Length - _currentIndex, 
                buffer, offset + totalBytesRead, count - totalBytesRead,
                false, out int charsUsed, out int bytesUsed, out spaceInBuffer);
#endif
            totalBytesRead += bytesUsed;
            _currentIndex = (_currentIndex + charsUsed) % current.Length;
        }

        return totalBytesRead;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _enumerator?.Dispose();
            _enumerator = null;
        }
        base.Dispose(disposing);
    }
        
    public override void Flush()
        => throw new NotImplementedException();

    public override long Seek(long offset, SeekOrigin origin)
        => throw new NotImplementedException();

    public override void SetLength(long value)
        => throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count) 
        => throw new NotImplementedException();

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotImplementedException();
    public override long Position
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}