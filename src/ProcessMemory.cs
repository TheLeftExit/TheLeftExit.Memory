using System;
using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace TheLeftExit.Memory {
    public partial class ProcessMemory : MemorySource {
        public IntPtr Handle => _nativeHandle.Value;
        internal HANDLE _nativeHandle;

        public ProcessMemory(IntPtr handle) => _nativeHandle = new(handle);

        protected override unsafe bool TryReadCore(ulong address, int count, void* buffer) =>
            Kernel32.ReadProcessMemory(_nativeHandle, (void*)address, buffer, (nuint)count);

        protected override unsafe bool TryWriteCore(ulong address, int count, void* buffer) =>
            Kernel32.WriteProcessMemory(_nativeHandle, (void*)address, buffer, (nuint)count);
    }
}
