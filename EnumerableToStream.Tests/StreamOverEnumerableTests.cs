namespace EnumerableToStream;

[TestFixture]
public class StreamOverEnumerableTests
{
    [Test]
    public void EmptyInput_EmptyStream()
    {
        var stream = Enumerable.Empty<string>().ToStream();

        byte[] buffer = Array.Empty<byte>();

        Assert.AreEqual(0, stream.Read(buffer, 0, 0));
    }

    [Test]
    public void ReadZeroBytes()
    {
        var stream = new[] { "Hello" }.ToStream();

        Assert.AreEqual(0, stream.Read(Array.Empty<byte>(), 0, 0));
    }

    [Test]
    public void NonEmptyInput()
    {
        var input = new [] {"Hello, ", " world", null, "", "!"};

        var stream = input.ToStream();

        using var reader = new StreamReader(stream);
            
        Assert.AreEqual(string.Join("", input), reader.ReadToEnd());
    }

    [Test]
    // Make sure ReadAsync works too.
    public async Task AsyncTest()
    {
        var stream = new [] {"Hello"}.ToStream();
        var actual = await new StreamReader(stream).ReadToEndAsync();
        Assert.AreEqual("Hello", actual);
    }

    [Test]
    // Make sure the ReadAsync overload taking Memory works too.
    public async Task ReadAsyncAsMemory()
    {
        var stream = new [] {"Hello"}.ToStream();
        var buffer = new byte[5];
        int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, 5));
        Assert.AreEqual(5, bytesRead);
    }

    class Disposable : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose()
        {
            Disposed = true;
        }
    }

    [Test]
    public void EnumerableDisposedWhenStreamClosed()
    {
        var disposable = new Disposable();

        IEnumerable<string> Input()
        {
            using (disposable)
            {
                yield return "Hello";
                yield return "world";
            }
        }

        var stream = Input().ToStream();

        var buffer = new byte[3];

        int _ = stream.Read(buffer, 0, buffer.Length);

        stream.Close();

        Assert.AreEqual(true, disposable.Disposed);
    }

    [Test]
    public void SmallBuffer()
    {
        var input = new[] { "Hello,", " world!", null, "" };

        var stream = input.ToStream();

        var buffer = new byte[5];

        Assert.AreEqual(5, stream.Read(buffer, 0, buffer.Length));
        Assert.AreEqual("Hello", Encoding.UTF8.GetString(buffer));

        Assert.AreEqual(5, stream.Read(buffer, 0, buffer.Length));
        Assert.AreEqual(", wor", Encoding.UTF8.GetString(buffer));

        Assert.AreEqual(3, stream.Read(buffer, 0, buffer.Length));
        Assert.AreEqual("ld!", Encoding.UTF8.GetString(buffer, 0, 3));
    }

    const string TwoByteChar = "Ж";

    [Test]
    public void SplitChar()
    {
        var input = new[] { "." + TwoByteChar + "." }; // three chars: 1-byte, 2-byte and 1-byte.

        var stream = input.ToStream();

        var buffer = new byte[4];

        Assert.AreEqual(1, stream.Read(buffer, 0, 2));
        Assert.AreEqual(2, stream.Read(buffer, 1, 2));
        Assert.AreEqual(1, stream.Read(buffer, 3, 1));
        Assert.AreEqual(string.Join("", input), Encoding.UTF8.GetString(buffer));
    }

    [Test]
    public void BufferTooSmall()
    {
        var input = new[] { TwoByteChar };

        var stream = input.ToStream();

        var buffer = new byte[1];

        // Need at least a 2-byte buffer to read the 2-byte char.
        Assert.Throws<ArgumentException>(() =>
        {
            int _ = stream.Read(buffer, 0, 1);
        });
    }

    [Test]
    public void ThrowsObjectDisposedException()
    {
        var input = new[] { "" };

        var stream = input.ToStream();
        stream.Dispose();

        var buffer = new byte[1];

        Assert.Throws<ObjectDisposedException>(() =>
        {
            int _ = stream.Read(buffer, 0, 1);
        });
    }

    [Test]
    public void OkToDisposeMultipleTimes()
    {
        var input = new[] { "" };
        var buffer = new byte[1];
        var stream = input.ToStream();
        int _ = stream.Read(buffer, 0, 1);
        stream.Dispose();
        stream.Dispose();
    }
}