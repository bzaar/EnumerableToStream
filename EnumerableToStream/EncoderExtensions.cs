namespace EnumerableToStream;

static class EncoderExtensions
{
    public static void Convert(this Encoder encoder, string? current, byte[] buffer, int offset, int count,
        ref int currentIndex,
        ref int totalBytesRead,
        ref bool spaceInBuffer)
    {
        if (current == null || current.Length == 0) return;
        
#if SPANS_SUPPORTED
        encoder.Convert(current.AsSpan(currentIndex),  
            buffer.AsSpan(offset + totalBytesRead, count - totalBytesRead),
            false, out int charsUsed, out int bytesUsed, out spaceInBuffer);
#else
        encoder.Convert(current.ToCharArray(), currentIndex, current.Length - currentIndex, 
            buffer, offset + totalBytesRead, count - totalBytesRead,
            false, out int charsUsed, out int bytesUsed, out spaceInBuffer);
#endif
        totalBytesRead += bytesUsed;
        currentIndex = (currentIndex + charsUsed) % current.Length;
    }
}