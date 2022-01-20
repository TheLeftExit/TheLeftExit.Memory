using System;
using System.Diagnostics;
using TheLeftExit.Memory.NativeInterop;

namespace TheLeftExit.Memory {
    public class ProcessMemorySource : MemorySource {
        public ProcessHandle Handle { get; }

        public ProcessMemorySource(ProcessHandle handle) => Handle = handle;

        protected override unsafe bool TryReadCore(nuint address, nuint count, void* buffer) =>
            NativeMethods.ReadProcessMemory(Handle.NativeHandle, (void*)address, buffer, count);

        protected override unsafe bool TryWriteCore(nuint address, nuint count, void* buffer) =>
            NativeMethods.WriteProcessMemory(Handle.NativeHandle, (void*)address, buffer, count);
    }

    public readonly struct ProcessHandle {
        public readonly IntPtr Value => NativeHandle;
        internal readonly HANDLE NativeHandle;

        public ProcessHandle(IntPtr handleValue) => NativeHandle = handleValue;

        public static ProcessHandle Open(uint processId, uint rights = 0x001FFFFF, bool inheritHandle = true) {
            HANDLE result = NativeMethods.OpenProcess((PROCESS_ACCESS_RIGHTS)rights, inheritHandle, processId);
            if (result == IntPtr.Zero) {
                throw new ApplicationException("Unable to open process.");
            }
            return new(result);
        }
    }

    public static class ProcessHandleExtensions {
        public static bool Close(this ProcessHandle handle) {
            return NativeMethods.CloseHandle(handle.NativeHandle);
        }

        public static unsafe bool Is64BitProcess(this ProcessHandle handle) {
            NativeMethods.IsWow64Process(handle.NativeHandle, out bool result);
            return result;
        }

        public static uint GetProcessId(this ProcessHandle handle) {
            return NativeMethods.GetProcessId(handle.NativeHandle);
        }

        internal static ulong GetBaseAddress(this ProcessHandle handle) {
            int processId = (int)GetProcessId(handle);
            Process process = Process.GetProcessById(processId);
            return (ulong)process.MainModule.BaseAddress;
        }
    }
}
