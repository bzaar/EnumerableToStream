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
        var enumerator = _enumerator ?? throw NewObjectDisposedException();

        var session = new ReadingSession(buffer, offset, count, this);
        
        while (session.HasSpaceInBuffer && (CurrentHasMore || enumerator.MoveNext()))
        {
            session.Convert(enumerator.Current);
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