namespace EnumerableToStream;

abstract class ReadOnlyStream : Stream
{
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
    
    protected Exception NewObjectDisposedException()
    {
        string message = $"The {GetType().Name} has been disposed.";
        return new ObjectDisposedException(message);
    }
}