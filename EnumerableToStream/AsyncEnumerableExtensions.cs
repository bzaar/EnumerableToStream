namespace EnumerableToStream;

public static class AsyncEnumerableExtensions
{
    public static Stream ToStream(this IAsyncEnumerable<string?> seq, Encoding? encoding = null)
    {
        return new StreamOverAsyncEnumerable(seq, (encoding ?? Encoding.UTF8).GetEncoder());
    }
}
