namespace EnumerableToStream;

internal class Reader
{
    public Reader(Encoder encoder)
    {
        Encoder = encoder;
    }

    public Encoder Encoder { get; }
    public int CurrentIndex;
}