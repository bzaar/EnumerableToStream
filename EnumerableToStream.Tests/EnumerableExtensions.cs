using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnumerableToStream;

static class EnumerableExtensions
{
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> seq)
    {
        foreach (var item in seq)
        {
            yield return item;
        }

        await Task.Delay(0);
    }
}
