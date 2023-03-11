using System.Runtime.CompilerServices;

namespace EnumerableToStream;

static class EnumerableExtensions
{
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(
        this IEnumerable<T> seq, 
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var item in seq)
        {
            ct.ThrowIfCancellationRequested();
            yield return item;
        }

        await Task.Delay(0, ct);
    }
}
