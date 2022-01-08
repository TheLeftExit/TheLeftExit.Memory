using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;

namespace TheLeftExit.Memory {
    public static class ProcessHelper {
        public static IntPtr OpenHandle(uint processId, uint rights = (uint)PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, bool inheritHandle = true) {
            HANDLE handle = Kernel32.OpenProcess((PROCESS_ACCESS_RIGHTS)rights, inheritHandle, processId);
            return handle.Value;
        }

        public static bool CloseHandle(IntPtr handle) {
            return Kernel32.CloseHandle(new HANDLE(handle));
        }

        public static unsafe bool Is64BitProcess(IntPtr handle) {
            BOOL result;
            Kernel32.IsWow64Process(new HANDLE(handle), &result);
            return result;
        }

        public static uint GetProcessId(IntPtr handle) {
            return Kernel32.GetProcessId(new HANDLE(handle));
        }

        internal static ulong GetBaseAddress(IntPtr handle) {
            int processId = (int)GetProcessId(handle);
            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(processId);
            return (ulong)process.MainModule.BaseAddress;
        }
    }
}
