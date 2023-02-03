using System.Threading;
using System.Threading.Tasks;


namespace EnumerableToStream;

class StreamOverAsyncEnumerable : Stream
{
    private int _currentIndex;
    private IAsyncEnumerable<string?>? _enumerable;
    private IAsyncEnumerator<string?>? _enumerator;
    private readonly Encoder _encoder;

    public StreamOverAsyncEnumerable(IAsyncEnumerable<string?> input, Encoder encoder)
    {
        _encoder = encoder;
        _enumerable = input;
        _enumerator = null;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        _enumerator ??= _enumerable?.GetAsyncEnumerator(cancellationToken) 
            ?? throw new ObjectDisposedException($"The {nameof(StreamOverAsyncEnumerable)} has been disposed.");
        
        int totalBytesRead = 0;
        bool spaceInBuffer = true;

        while (spaceInBuffer && totalBytesRead < count && (_currentIndex != 0 || await _enumerator.MoveNextAsync(cancellationToken)))
        {
            _encoder.Convert(_enumerator.Current, buffer, offset, count,
                ref _currentIndex, ref totalBytesRead, ref spaceInBuffer);
        }

        return totalBytesRead;
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
        throw new NotImplementedException("Please use ReadAsync() instead.");
    }
    
    protected override void Dispose(bool disposing)
    {
        throw new NotImplementedException("Please use DisposeAsync() instead.");
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