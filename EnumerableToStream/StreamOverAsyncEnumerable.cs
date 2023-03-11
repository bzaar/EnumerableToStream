namespace EnumerableToStream;

class StreamOverAsyncEnumerable : StreamOverEnumerableBase
{
    private IAsyncEnumerable<string?>? _enumerable;
    private IAsyncEnumerator<string?>? _enumerator;

    public StreamOverAsyncEnumerable(IAsyncEnumerable<string?> input, Encoder encoder) 
        : base(encoder)
    {
        _enumerable = input;
        _enumerator = null;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        _enumerator ??= _enumerable?.GetAsyncEnumerator(cancellationToken) 
                        ?? throw NewObjectDisposedException();

        var session = new ReadingSession(buffer, offset, count, this);
        
        while (session.HasSpaceInBuffer && (CurrentHasMore || await _enumerator.MoveNextAsync()))
        {
            session.Convert(_enumerator.Current);
        }

        return session.TotalBytesRead;
    }

    public override async ValueTask DisposeAsync()
    {
        _enumerable = null;
        var enumerator = Interlocked.Exchange(ref _enumerator, null);
        if (enumerator != null)
            await enumerator.DisposeAsync();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("Please use ReadAsync() instead.");
    }
    
    protected override void Dispose(bool disposing)
    {
        throw new NotSupportedException("Please use DisposeAsync() instead.");
    }
}