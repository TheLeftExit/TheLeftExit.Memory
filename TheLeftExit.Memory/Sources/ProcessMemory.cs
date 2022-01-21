using System.Diagnostics;
using TheLeftExit.Memory.Native;

namespace TheLeftExit.Memory.Sources {
    /// <summary>
    /// A <see cref="MemorySource"/> wrapper for a process handle that uses native
    /// <a href="https://docs.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-readprocessmemory">ReadProcessMemory</a>/<a href="https://docs.microsoft.com/en-us/windows/win32/api/memoryapi/nf-memoryapi-writeprocessmemory">WriteProcessMemory</a> API.
    /// </summary>
    public sealed class ProcessMemory : MemorySource {
        public HANDLE Handle { get; }

        public ProcessMemory(HANDLE handle) => Handle = handle;

        public override unsafe bool TryRead(nuint address, nuint count, void* buffer) =>
            Handle.ReadProcessMemory((void*)address, buffer, count, out _);

        public override unsafe bool TryWrite(nuint address, nuint count, void* buffer) =>
            Handle.WriteProcessMemory((void*)address, buffer, count, out _);
    }
}
