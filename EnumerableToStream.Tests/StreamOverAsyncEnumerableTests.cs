namespace EnumerableToStream;

[TestFixture]
public class StreamOverAsyncEnumerableTests
{
    [Test]
    public async Task EmptyInput_EmptyStream()
    {
        var stream = Enumerable.Empty<string>().AsAsyncEnumerable().ToStream();

        byte[] buffer = Array.Empty<byte>();

        Assert.AreEqual(0, await stream.ReadAsync(buffer, 0, 0));
    }

    [Test]
    public async Task ReadZeroBytes()
    {
        var stream = new[] { "Hello" }.AsAsyncEnumerable().ToStream();

        Assert.AreEqual(0, await stream.ReadAsync(Array.Empty<byte>(), 0, 0));
    }

    [Test]
    public async Task NonEmptyInput()
    {
        var input = new [] {"Hello, ", " world", null, "", "!"};

        await using var stream = input.AsAsyncEnumerable().ToStream();

        var reader = new StreamReader(stream);
            
        Assert.AreEqual(string.Join("", input), await reader.ReadToEndAsync());
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
    public async Task EnumerableDisposedWhenStreamClosed()
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

        var stream = Input().AsAsyncEnumerable().ToStream();

        var buffer = new byte[3];

        int _ = await stream.ReadAsync(buffer, 0, buffer.Length);

        await stream.DisposeAsync();

        Assert.AreEqual(true, disposable.Disposed);
    }

    [Test]
    public async Task SmallBuffer()
    {
        var input = new[] { "Hello,", " world!", null, "" };

        var stream = input.AsAsyncEnumerable().ToStream();

        var buffer = new byte[5];

        Assert.AreEqual(5, await stream.ReadAsync(buffer, 0, buffer.Length));
        Assert.AreEqual("Hello", Encoding.UTF8.GetString(buffer));

        Assert.AreEqual(5, await stream.ReadAsync(buffer, 0, buffer.Length));
        Assert.AreEqual(", wor", Encoding.UTF8.GetString(buffer));

        Assert.AreEqual(3, await stream.ReadAsync(buffer, 0, buffer.Length));
        Assert.AreEqual("ld!", Encoding.UTF8.GetString(buffer, 0, 3));
    }

    const string TwoByteChar = "Ж";

    [Test]
    public async Task SplitChar()
    {
        var input = new[] { "." + TwoByteChar + "." }; // three chars: 1-byte, 2-byte and 1-byte.

        var stream = input.AsAsyncEnumerable().ToStream();

        var buffer = new byte[4];

        Assert.AreEqual(1, await stream.ReadAsync(buffer, 0, 2));
        Assert.AreEqual(2, await stream.ReadAsync(buffer, 1, 2));
        Assert.AreEqual(1, await stream.ReadAsync(buffer, 3, 1));
        Assert.AreEqual(string.Join("", input), Encoding.UTF8.GetString(buffer));
    }

    [Test]
    public async Task BufferTooSmall()
    {
        var input = new[] { TwoByteChar };

        var stream = input.AsAsyncEnumerable().ToStream();

        var buffer = new byte[1];

        bool thrown = false;
        
        // Need at least a 2-byte buffer to read the 2-byte char.
        try
        {
            int _ = await stream.ReadAsync(buffer, 0, 1);
        }
        catch (ArgumentException)
        {
            thrown = true;
        }
        Assert.IsTrue(thrown);
    }

    [Test]
    public async Task ThrowsObjectDisposedException()
    {
        var input = new[] { "" };

        var stream = input.AsAsyncEnumerable().ToStream();
        await stream.DisposeAsync();

        var buffer = new byte[1];

        bool thrown = false;
        
        try
        {
            int _ = await stream.ReadAsync(buffer, 0, 1);
        }
        catch (ObjectDisposedException)
        {
            thrown = true;
        }
        Assert.IsTrue(thrown);
    }

    [Test]
    public async Task OkToDisposeMultipleTimes()
    {
        var input = new[] { "" };
        var buffer = new byte[1];
        var stream = input.AsAsyncEnumerable().ToStream();
        int _ = await stream.ReadAsync(buffer, 0, 1);
        await stream.DisposeAsync();
        await stream.DisposeAsync();
    }

    [Test]
    public async Task CanBeCancelledUsingExternalToken()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var input = new[] { "A" };
        var buffer = new byte[1];
        var ct = cancellationTokenSource.Token;
        var stream = input.AsAsyncEnumerable(ct).ToStream();
        bool thrown = false;
        
        try
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, 1, default);
            Assert.AreEqual(0, bytesRead);
        }
        catch (OperationCanceledException)
        {
            thrown = true;
        }
        Assert.IsTrue(thrown);
    }

    [Test]
    public async Task CanBeCancelledUsingTokenPassedIn()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var input = new[] { "A" };
        var buffer = new byte[1];
        var ct = cancellationTokenSource.Token;
        var stream = input.ToStream();
        bool thrown = false;
        
        try
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, 1, ct);
            Assert.AreEqual(0, bytesRead);
        }
        catch (OperationCanceledException)
        {
            thrown = true;
        }
        Assert.IsTrue(thrown);
    }
}