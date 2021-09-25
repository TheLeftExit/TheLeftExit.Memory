### TheLeftExit.Memory
One of the many libraries focused on reading process memory.

**TheLeftExit.Memory** is designed to be compact, fast and memory-efficient, making use of unsafe context, and latest .NET features such as [spans](https://docs.microsoft.com/en-us/dotnet/api/system.span-1) or [Win32 API source generation](https://github.com/microsoft/CsWin32).

Available as a [NuGet package](https://www.nuget.org/packages/TheLeftExit.Memory/).

### Overview of API
#### `TheLeftExit.Memory.Sources`
##### `MemorySource`
```cs
public abstract class MemorySource {
	public abstract bool ReadBytes(ulong address, nuint count, Span<byte> buffer);
	
	public bool TryRead<T>(ulong address, out T result) where T : unmanaged;
	public T? Read<T>(ulong address) where T : unmanaged;
	public T ReadValue<T>(ulong address) where T : unmanaged;
}
```
None of the `MemorySource` methods allocate memory on the heap, or invoke any type-specific conversion logic.

You can use those methods to read directly into primitive types, structures containing primitive types, or even tuples:  
```(float X, float Y) Position = source.ReadValue<(float, float)>(address);```

##### `ProcessMemory`
```cs
public class ProcessMemory : MemorySource, IDisposable {
	public ProcessMemory(uint processId);
	public void Dispose();
}
```
##### `StoredMemory`
```cs
public class StoredMemory : MemorySource {
	public StoredMemory(byte[] source);
}
```
These are just a few possible implementations of `MemorySource` - you can inherit any of the classes listed to manually enable features like remote memory reading or caching.

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

#### `TheLeftExit.Memory.Queries`
##### `PointerQuery`
```cs
public class PointerQuery {
	public Int64? Offset { get; set; }
	public PointerQuery(PointerQueryCondition queryCondition, uint maxOffset, sbyte scanStep);
	public PointerQueryResult? GetResult(MemorySource source, ulong baseAddress);
}

public delegate PointerQueryConditionResult PointerQueryCondition(MemorySource memorySource, ulong address);
public enum PointerQueryConditionResult { Continue, Return, Break }
```
Allows you to scan a `MemorySource` for values matching specific condition. More on conditions below.
##### `QueryConditions`
```cs
public static class QueryConditions {
	public static PointerQueryCondition AOB(params Byte?[] pattern);
	public static PointerQueryCondition RTTIByRef(string name);
	public static PointerQueryCondition RTTIByVal(string name);
}
```
Provides a list of preconfigured delegates to use as `PointerQuery` conditions.