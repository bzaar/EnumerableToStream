using System.Threading;

namespace EnumerableToStream;

class StreamOverEnumerable : Stream
{
    private IEnumerator<string?>? _enumerator;
    private readonly Reader _reader;

    public StreamOverEnumerable(IEnumerable<string?> input, Encoder encoder)
    {
        _reader = new Reader(encoder);
        _enumerator = input.GetEnumerator();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_enumerator == null)
        {
            throw new ObjectDisposedException($"The {nameof(StreamOverEnumerable)} has been disposed.");
        }

        var session = new ReadingSession(buffer, offset, count, _reader);
        
        while (session.HasSpaceInBuffer && (session.CurrentHasMore || _enumerator.MoveNext()))
        {
            session.Convert(_enumerator.Current);
        }

        return session.TotalBytesRead;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Interlocked.Exchange(ref _enumerator, null)?.Dispose();
        }
        base.Dispose(disposing);
    }
        
    public override void Flush()
        => throw WritingNotSupported();

    public override long Seek(long offset, SeekOrigin origin)
        => throw SeekingNotSupported();

    public override void SetLength(long value)
        => throw SeekingNotSupported();

    public override void Write(byte[] buffer, int offset, int count) 
        => throw WritingNotSupported();

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw SeekingNotSupported();
    public override long Position
    {
        get => throw SeekingNotSupported();
        set => throw SeekingNotSupported();
    }
    
    private static Exception WritingNotSupported() =>
        new NotSupportedException("Writing is not supported.");

    private static Exception SeekingNotSupported() =>
        new NotSupportedException("Seeking is not supported.");
}