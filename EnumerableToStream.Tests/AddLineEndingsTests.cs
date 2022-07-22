using System.Linq;
using NUnit.Framework;

namespace EnumerableToStream;

[TestFixture]
public class AddLineEndingsTests
{
    [Test]
    public void EmptyEnumerable()
    {
        var enumerable = Enumerable.Empty<string>();
        string result = string.Join("", enumerable.AddLineEndings());
        Assert.AreEqual("", result);
    }
        
    [Test]
    public void NonEmptyEnumerable()
    {
        var enumerable = new [] {"line 1", "line 2"};
        string result = string.Join("", enumerable.AddLineEndings("\n"));
        Assert.AreEqual("line 1\nline 2\n", result);
    }
}