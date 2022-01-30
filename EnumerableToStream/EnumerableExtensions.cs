using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EnumerableToStream
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> of <see cref="string"/> to a <see cref="Stream"/>.
        /// The IEnumerable is evaluated lazily as the stream is read.
        /// </summary>
        public static Stream ToStream(this IEnumerable<string?> source, Encoding? encoding = default) => 
            new StreamOverEnumerable(source, (encoding ?? Encoding.UTF8).GetEncoder());
    }
}