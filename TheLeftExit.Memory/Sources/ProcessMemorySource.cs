using System.Diagnostics;
using TheLeftExit.Memory.Native;

namespace TheLeftExit.Memory.Sources {
    /// <summary>
    /// A <see cref="MemorySource"/> wrapper for a process handle that uses native <c>ReadProcessMemory</c>/<c>WriteProcessMemory</c> API.
    /// </summary>
    public class ProcessMemorySource : MemorySource {
        public HANDLE Handle { get; }

        public ProcessMemorySource(HANDLE handle) => Handle = handle;

        protected override unsafe bool TryReadCore(nuint address, nuint count, void* buffer) =>
            Handle.ReadProcessMemory((void*)address, buffer, count, out _);

        protected override unsafe bool TryWriteCore(nuint address, nuint count, void* buffer) =>
            Handle.WriteProcessMemory((void*)address, buffer, count, out _);
    }
}
