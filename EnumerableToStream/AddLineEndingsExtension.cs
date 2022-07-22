using System;
using System.Collections.Generic;

namespace EnumerableToStream;

public static class AddLineEndingsExtension
{
    /// <summary>
    /// Adds Environment.NewLine after each string in <param name="source" />.
    /// </summary>
    public static IEnumerable<string?> AddLineEndings(this IEnumerable<string?> source) => 
        AddLineEndings(source, Environment.NewLine);

    /// <summary>
    /// Adds <param name="ending" /> after each string in <param name="source" />.
    /// </summary>
    public static IEnumerable<string?> AddLineEndings(this IEnumerable<string?> source, string ending)
    {
        foreach (string? line in source)
        {
            yield return line;
            yield return ending;
        }
    }
}