namespace EnumerableToStream;

internal record struct ReadingSession(byte[] Buffer, int Offset, int Count, StreamOverEnumerableBase Context)
{
    public int TotalBytesRead = 0;
    private bool _spaceInBuffer = true;

    public bool HasSpaceInBuffer => _spaceInBuffer && TotalBytesRead < Count;
    
    public void Convert(string? current)
    {
        // ReSharper disable once ReplaceWithStringIsNullOrEmpty
        if (current == null || current.Length == 0) return;
        
#if SPANS_SUPPORTED
        Context.Encoder.Convert(current.AsSpan(Context.CurrentIndex),  
            Buffer.AsSpan(Offset + TotalBytesRead, Count - TotalBytesRead),
            false, out int charsUsed, out int bytesUsed, out _spaceInBuffer);
#else
        Context.Encoder.Convert(current.ToCharArray(), Context.CurrentIndex, current.Length - Context.CurrentIndex, 
            Buffer, Offset + TotalBytesRead, Count - TotalBytesRead,
            false, out int charsUsed, out int bytesUsed, out _spaceInBuffer);
#endif
        TotalBytesRead += bytesUsed;
        Context.CurrentIndex += charsUsed;
        Context.CurrentIndex %= current.Length;
    }
}

