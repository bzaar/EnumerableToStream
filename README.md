# EnumerableToStream [Nuget package][nuget]

Converts an ```IEnumerable<string>``` to a ```Stream```:

```csharp
using EnumerableToStream;

IEnumerable<string> enumerable = new [] {"Hello, ", "world!"};

Stream stream = enumerable.ToStream();
```

Points of interest:

* The enumerable is properly disposed of when the stream is closed.
* The stream does zero allocations on .NET Standard 2.1.
* ToStream() supports encodings: ```enumerable.ToStream(Encoding.UTF8);```

[nuget]: https://www.nuget.org/packages/EnumerableToStream/
