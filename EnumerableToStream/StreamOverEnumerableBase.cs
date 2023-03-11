namespace EnumerableToStream;

abstract class StreamOverEnumerableBase : ReadOnlyStream
{
    public Encoder Encoder { get; }
    public int CurrentIndex;

    protected StreamOverEnumerableBase(Encoder encoder)
    {
        this.Encoder = encoder;
    }
    
    protected bool CurrentHasMore => CurrentIndex > 0;
}