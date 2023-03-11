using System.Threading;

namespace EnumerableToStream;

class StreamOverEnumerable : StreamOverEnumerableBase
{
    private IEnumerator<string?>? _enumerator;

    public StreamOverEnumerable(IEnumerable<string?> input, Encoder encoder) : base(encoder)
    {
        _enumerator = input.GetEnumerator();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_enumerator == null)
        {
            throw new ObjectDisposedException($"The {nameof(StreamOverEnumerable)} has been disposed.");
        }

        var session = new ReadingSession(buffer, offset, count, this);
        
        while (session.HasSpaceInBuffer && (CurrentHasMore || _enumerator.MoveNext()))
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
}