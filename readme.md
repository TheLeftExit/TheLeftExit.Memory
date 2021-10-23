### TheLeftExit.Memory
One of the many libraries focused on reading process memory.

**TheLeftExit.Memory** is designed to be compact, fast and extremely memory-efficient, making extensive use of unsafe context.

Available as a [NuGet package](https://www.nuget.org/packages/TheLeftExit.Memory/).

### Main features
#### `MemorySource`
 - `TheLeftExit.Memory.Sources.ReadOnlyMemorySource`
 - `TheLeftExit.Memory.Sources.MemorySource`
 - `TheLeftExit.Memory.Sources.LocalMemorySource`

These abstract classes allow wrapping remote memory sources for generic reading and writing:
```cs
bool ReadOnlyMemorySource.TryRead<T>(ulong address, out T result) where T : unmanaged;
bool ReadOnlyMemorySource.TryRead<T>(ulong address, Span<T> buffer) where T : unmanagedl;

bool MemorySource.TryWrite<T>(ulong address, T value) where T : unmanaged;
bool ReadOnlyMemorySource.TryWrite<T>(ulong address, Span<T> buffer) where T : unmanagedl;

ref T LocalMemorySource.ReadRef<T>(ulong address) where T : unmanaged;
```

#### `ProcessMemory`
 - `TheLeftExit.Memory.Sources.ProcessMemory`

A `MemorySource` over a process that allows you to read or write data as primitive types, structures, or even tuples:  
```(float X, float Y) Position = source.ReadValue<(float, float)>(address);```

#### `CachedMemory`
 - `TheLeftExit.Memory.Sources.CachedMemory`

A `LocalMemorySource` over `Memory<byte>` that allows you to store and access memory regions for complex operations, as well as cache process memory for use with existing `MemorySource` logic.

#### `RTTIMethods`
```cs
string RTTIMethods.GetRTTIClassNames64(this ReadOnlyMemorySource source, ulong address);
string RTTIMethods.GetRTTIClassNames32(this ReadOnlyMemorySource source, ulong address);
```
Allows you to query a structure for its RTTI-sourced class name.  
The names you can find this way are the same names that tools like Cheat Engine, IDA or ReClass.NET will show you for the address or the pointer to it.

To learn more about this technology, visit http://www.openrce.org/articles/full_view/23

### `PointerQuery`
 - `TheLeftExit.Memory.Queries.PointerQuery`

A simple iterator over a `ReadOnlyMemorySource` memory region. Allows you to scan remote memory for addresses matching a specific condition.

```cs
public partial class PointerQuery {
    public PointerQueryOptions Settings { get; set; }
    public object Tag { get; set; }
    public uint? Offset { get; set; }
    public EventHandler<ProcessStepEventArgs> ProcessStep { get; set; }

    public ulong? Run(ReadOnlyMemorySource source, ulong baseAddress);

    public static void AOB(object sender, ProcessStepEventArgs e);
    public static void RTTIByRef(object sender, ProcessStepEventArgs e);
    public static void RTTIByVal(object sender, ProcessStepEventArgs e);
}

public class ProcessStepEventArgs : EventArgs {
    public ReadOnlyMemorySource Source { get; }
    public ulong Address { get; }
    public ProcessStepResult? Result { get; set; }
}
```

---

There is more stuff, but it's mostly specific to my needs and/or too bothersome to document.

I often update this project as a I come up with more efficient ways to achieve its functionality, so expect breaking changes with most new versions.