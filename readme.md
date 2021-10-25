### TheLeftExit.Memory
One of the many libraries focused on reading process memory.

**TheLeftExit.Memory** is designed to be compact, fast and extremely memory-efficient, making extensive use of unsafe context.

Available as a [NuGet package](https://www.nuget.org/packages/TheLeftExit.Memory/).

### Main features
#### `MemorySource`
An abstract class that allows wrapping remote memory sources for generic reading and writing:
```cs
public class MemorySource {
    public T Read<T>(ulong address) where T : unmanaged;
    public bool TryRead<T>(ulong address, out T result) where T : unmanaged;
    public bool TryRead<T>(ulong address, Span<T> buffer) where T : unmanaged;
    public bool TryRead(ulong address, int count, void* buffer);
    protected abstract bool TryReadCore(ulong address, int count, void* buffer);
    public virtual bool AllowRead { get; }

    public void Write(ulong address, T value) where T : unmanaged;
    public bool TryWrite<T>(ulong address, T value) where T : unmanaged;
    public bool TryWrite<T>(ulong address, Span<T> buffer) where T : unmanaged;
    public bool TryWrite(ulong address, int count, void* buffer);
    protected abstract bool TryWriteCore(ulong address, int count, void* buffer);
    public virtual bool AllowWrite { get; }
}
```

#### `ProcessMemory`
A `MemorySource` over a process that allows you to read its memory.
```cs
public partial class ProcessMemory : MemorySource, IDisposable {
    public readonly uint Id;
    public readonly uint ProcessAccessRights;
    public readonly bool InheritHandle;

    public readonly IntPtr Handle;
    public readonly bool Is32Bit;

    public readonly ulong BaseAddress;
    public readonly uint MainModuleSize;

    public unsafe ProcessMemory(Process process, [uint rights], [bool inheritHandle]);
    public unsafe ProcessMemory(int ProcessId, [uint rights], [bool inheritHandle]);

    public RemoteStructure Root { get; }
    public Dictionary<(string, string, bool), int> Offsets { get; }
```

#### `RemoteStructure`
A node in a structure hierarchy of a remote process. Allows you to easily branch between structures by scanning and caching offsets based on structure names. Works on MSVC RTTI (using methods from the static `RTTI` class).
```cs
    public struct RemoteStructure : IRemoteStructure {
    public readonly ProcessMemory Source;
    public readonly ulong Address;
    public readonly string Name;

    public T Read<T>(int offset) where T : unmanaged;
    public void Write<T>(int offset, T value) where T : unmanaged;

    public RemoteStructure this[int offset, [bool byRef], [string name]] { get; }
    public RemoteStructure this[string className, [bool byRef]] { get; }
}
```

---

There might be more stuff, but it's mostly specific to my needs and/or too bothersome to document.

I often update this project as a I come up with more efficient ways to achieve its functionality, so expect breaking changes with most new versions.
