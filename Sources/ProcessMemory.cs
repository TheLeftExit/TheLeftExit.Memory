using System;
using System.Collections.Generic;
using System.Linq;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;

namespace TheLeftExit.Memory.Sources {
    public class ProcessMemory : WriteableMemorySource, IDisposable {
        public readonly uint Id;
        public readonly IntPtr Handle;
        public readonly uint ProcessAccessRights;
        public readonly bool InheritHandle;

        internal HANDLE handle;

        public ProcessMemory(uint processId, uint rights = (uint)PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, bool inheritHandle = true) {
            Id = processId;
            ProcessAccessRights = (uint)rights;
            InheritHandle = inheritHandle;
            handle = Kernel32.OpenProcess((PROCESS_ACCESS_RIGHTS)rights, inheritHandle, Id);
            if (handle.IsNull)
                throw new ApplicationException($"Unable to open processId {Id} (process not open or access denied).");
            Handle = handle.Value;
        }

        public override unsafe bool TryRead(ulong address, int count, void* buffer) =>
            Kernel32.ReadProcessMemory(handle, (void*)address, buffer, (nuint)count);

        public override unsafe bool TryWrite(ulong address, int count, void* buffer) =>
            Kernel32.WriteProcessMemory(handle, (void*)address, buffer, (nuint)count);

        public void Dispose() {
            Kernel32.CloseHandle(handle);
        }
    }
}
