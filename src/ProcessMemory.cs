using System;
using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;

namespace TheLeftExit.Memory {
    public partial class ProcessMemory : MemorySource, IDisposable {
        public readonly uint Id;
        public readonly uint ProcessAccessRights;
        public readonly bool InheritHandle;


        public readonly IntPtr Handle;
        public readonly bool Is32Bit;

        public readonly ulong BaseAddress;
        public readonly uint MainModuleSize;

        private HANDLE NativeHandle => new HANDLE(Handle);

        public unsafe ProcessMemory(Process process, uint rights = (uint)PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, bool inheritHandle = true) {
            Id = (uint)process.Id;
            ProcessAccessRights = rights;
            InheritHandle = inheritHandle;

            HANDLE handle = Kernel32.OpenProcess((PROCESS_ACCESS_RIGHTS)rights, inheritHandle, Id);
            if (handle.IsNull)
                throw new ApplicationException($"Unable to open processId {Id} (process not open or access denied).");
            Handle = handle.Value;

            BOOL is32Bit;
            Kernel32.IsWow64Process(handle, &is32Bit);
            Is32Bit = is32Bit;

            BaseAddress = (ulong)process.MainModule.BaseAddress;
            MainModuleSize = (uint)process.MainModule.ModuleMemorySize;
        }

        public ProcessMemory(int processId, uint rights = (uint)PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, bool inheritHandle = true) :
            this(Process.GetProcessById(processId), rights, inheritHandle) { }

        protected override unsafe bool TryReadCore(ulong address, int count, void* buffer) =>
            Kernel32.ReadProcessMemory(NativeHandle, (void*)address, buffer, (nuint)count);

        protected override unsafe bool TryWriteCore(ulong address, int count, void* buffer) =>
            Kernel32.WriteProcessMemory(NativeHandle, (void*)address, buffer, (nuint)count);

        public void Dispose() {
            Kernel32.CloseHandle(NativeHandle);
        }
    }
}
