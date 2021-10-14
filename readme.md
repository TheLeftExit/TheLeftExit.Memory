### TheLeftExit.Memory
One of the many libraries focused on reading process memory.

**TheLeftExit.Memory** is designed to be compact, fast and memory-efficient, making extensive use of unsafe context.

Available as a [NuGet package](https://www.nuget.org/packages/TheLeftExit.Memory/).

### Main features
#### `TheLeftExit.Memory.Sources`
##### `MemorySource`
```cs
public abstract class MemorySource {
	public abstract bool TryRead(ulong address, int count, void* buffer);
	public bool TryRead<T>(ulong address, out T result) where T : unmanaged;
	public bool TryRead<T>(ulong address, Span<T> buffer) where T : unmanaged;

	public T? Read<T>(ulong address) where T : unmanaged;
	public T ReadValue<T>(ulong address) where T : unmanaged;
}

public abstract class WriteableMemorySource : MemorySource {
	public abstract bool TryWrite(ulong address, int count, void* buffer);
	public bool TryWrite<T>(ulong address, T value) where T : unmanaged;
	public bool TryWrite<T>(ulong address, ref T value) where T : unmanaged;
	public bool TryWrite<T>(ulong address, Span<T> buffer) where T : unmanaged;
}

public abstract class LocalMemorySource : WriteableMemorySource {
	public abstract Span<T> Slice<T>(ulong address, int count) where T : unmanaged;
	public ref T ReadRef<T>(ulong address) where T : unmanaged;

	// Compatibility with WriteableMemorySource (not recommended to call directly).
	public abstract bool Contains(ulong address, int count);
	public override bool TryRead(ulong address, int count, void* buffer);
	public override bool TryWrite(ulong address, int count, void* buffer);
}
```
`MemorySource`/`WriteableMemorySource` are designed for remote memory sources, while `LocalMemorySource` works with memory allocated by your program.

None of these classes' methods allocate memory on the heap, or invoke any type-specific conversion logic.

##### `ProcessMemory`
```cs
public class ProcessMemory : WriteableMemorySource, IDisposable {
	public readonly IntPtr Handle;

	public readonly uint Id;
	public readonly uint ProcessAccessRights;
	public readonly bool InheritHandle;

	public ProcessMemory(uint processId); // Optional parameters not listed.
	public void Dispose();
}
```
A `WriteableMemorySource` over a process that allows you to read or write data as primitive types, structures, or even tuples:  
```(float X, float Y) Position = source.ReadValue<(float, float)>(address);```

##### `CachedMemory`
```cs
public class CachedMemory : LocalMemorySource {}
	public ulong BaseAddress { get; }
	public int Size { get; }
	public Memory<byte> Memory { get; }

	public CachedMemory(ulong baseAddress, int size);
	public CachedMemory.Handle Pin(); // Disposable wrapper for MemoryHandle.

	public override Span<T> Slice<T>(ulong address, int count);
	public override bool Contains(ulong address, int count);
}
```
A `LocalMemorySource` over `Memory<byte>` that allows you to store and access memory regions for complex operations, as well as cache process memory for use with existing `MemorySource` logic.

#### `TheLeftExit.Memory.RTTI`
##### `RTTIMethods`
```cs
public static class RTTIMethods {
	public static string[] GetRTTIClassNames64(this MemorySource source, ulong address);
	public static string[] GetRTTIClassNames32(this MemorySource source, ulong address);
}
```
Allows you to query a structure for its RTTI-sourced class names.  
The names you can find this way are the same names that tools like Cheat Engine, IDA or ReClass.NET will show you for the address or the pointer to it.  
In fact, those methods have been transcribed from [ReClass.NET's source](https://github.com/ReClassNET/ReClass.NET/blob/0ee8a4cd6a00e2664f2ef3250a81089c32d69392/ReClass.NET/Memory/RemoteProcess.cs#L190).

To learn more about this technology, visit http://www.openrce.org/articles/full_view/23

---
There's more stuff, but it's mostly specific to my own needs.