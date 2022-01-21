### TheLeftExit.Memory
One of the many libraries focused on reading process memory.

**TheLeftExit.Memory** is designed to be compact, fast and extremely memory-efficient by means of unsafe context.

Available as a [NuGet package](https://www.nuget.org/packages/TheLeftExit.Memory/) targeting .NET Standard 2.0.

### Main features
#### `MemorySource`
An abstract class that allows wrapping remote memory sources for generic reading and writing:
```cs
public abstract class MemorySource {
    public abstract bool TryRead(nuint address, nuint count, void* buffer);
    public abstract bool TryWrite(nuint address, nuint count, void* buffer);

    public bool TryWrite<T>(nuint address, Span<T> buffer) where T : unmanaged;
    public bool TryRead<T>(nuint address, Span<T> buffer) where T : unmanaged;

    public bool TryRead<T>(nuint address, out T result) where T : unmanaged;
    public T Read<T>(nuint address) where T : unmanaged;

    public bool TryWrite<T>(nuint address, T value) where T : unmanaged;
    public void Write<T>(nuint address, T value) where T : unmanaged;

    public bool TryWrite<T>(nuint address, in T value) where T : unmanaged;
    public void Write<T>(nuint address, in T value) where T : unmanaged;
}
```

#### `ProcessMemory`
A `MemorySource` over a process that allows you to read its memory using [ReadProcessMemory](https://docs.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-readprocessmemory)/[WriteProcessMemory](https://docs.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-writeprocessmemory) API.

The `HANDLE` structure encapsulates a process handle represented by an `IntPtr` value and exposes certain native API methods.
```cs
public class ProcessMemory : MemorySource {
    public HANDLE Handle { get; }
    public ProcessMemory(HANDLE handle);
}

public readonly struct HANDLE {
    public readonly IntPtr Value;

    public HANDLE(IntPtr value);
    public static implicit operator IntPtr(HANDLE handle);
    public static implicit operator HANDLE(IntPtr value);

    public static HANDLE OpenProcess(uint dwProcessId);
    public static HANDLE OpenProcess(PROCESS_ACCESS_RIGHTS dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    public bool ReadProcessMemory(void* lpBaseAddress, void* lpBuffer, nuint nSize, out nuint lpNumberOfBytesRead);
    public bool WriteProcessMemory(void* lpBaseAddress, void* lpBuffer, nuint nSize, out nuint lpNumberOfBytesWritten);

    public bool? IsWow64Process();
    public bool CloseHandle();
    public uint GetProcessId();
    public nuint GetBaseAddress();
}
```

#### `RttiExtensions`
Methods that can be used to retrieve RTTI class names in MSVC application memory.
```cs
public static class RttiExtensions {
    public static string GetClassName64(this MemorySource source, nuint address, PointerDepth depth);
    public static string GetClassName32(this MemorySource source, nuint address, PointerDepth depth);
}

public enum PointerDepth {
    VTable = 0,
    Instance = 1,
    Reference = 2
}
```

---

Implementing more robust or fluent structures based on those primitives is up to end-user, as the implementation and API may differ greatly depending on the usage scenario.