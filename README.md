[![Build status](https://ci.appveyor.com/api/projects/status/q4c8f61tjwgv7cwu?svg=true)](https://ci.appveyor.com/project/morpher/enumerabletostream)

# EnumerableToStream [Nuget package][nuget]

Converts an ```IEnumerable<string>``` to a ```Stream```:

```csharp
using EnumerableToStream;

IEnumerable<string> enumerable = new [] {"Hello, ", "world!"};

Stream stream = enumerable.ToStream();
```

## Points of interest

* The enumerable is evaluated lazily as the stream is read.
* The enumerable is properly disposed of when the stream is closed.
* ToStream() does zero allocations on .NET Standard 2.1 compatible runtimes.
* ToStream() supports encodings: `enumerable.ToStream(Encoding.UTF8);`
* ToStream() accepts both `IEnumerable` and `IAsyncEnumerable`.
  If you use the async version, you will need to call `stream.ReadAsync()` rather than `Read()`.

[nuget]: https://www.nuget.org/packages/EnumerableToStream/

## Using in ASP.NET Core

Streaming query results to the client in a memory efficient manner:

```csharp
public IActionResult Get()
{
    IEnumerable<string> lines = GetLines();
    Stream stream = lines.AddLineEndings().ToStream();
    return File(stream, "text/csv");
}
```
