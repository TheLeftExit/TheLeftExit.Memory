#pragma warning disable CA1416

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
    /// <summary>
    /// <see cref="MemorySource"/> over a process's memory.
    /// </summary>
    public class ProcessMemory : MemorySource, IDisposable {
        protected HANDLE handle;

        /// <summary>
        /// Opens an full access handle to a process.
        /// </summary>
        /// <param name="processId"></param>
        /// <exception cref="ApplicationException"></exception>
        public ProcessMemory(uint processId) {
            handle = OpenProcess(processId);
            if (handle.IsNull)
                throw new ApplicationException($"Unable to open processId {processId}.");
        }

        public override unsafe bool TryRead(ulong address, int count, void* buffer) {
            return Kernel32.ReadProcessMemory(handle, (void*)address, buffer, (nuint)count);
        }

        protected HANDLE OpenProcess(uint id) =>
            Kernel32.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, false, id);

        protected BOOL CloseProcess(HANDLE pHandle) =>
            Kernel32.CloseHandle(pHandle);

        /// <summary>
        /// Closes the process handle.
        /// </summary>
        public void Dispose() {
            CloseProcess(handle);
        }
    }
}
