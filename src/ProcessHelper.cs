using System;
using TheLeftExit.Memory.NativeInterop;

namespace TheLeftExit.Memory {
    public static class ProcessHelper {
        public static IntPtr OpenHandle(uint processId, uint rights = (uint)PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, bool inheritHandle = true) {
            HANDLE handle = NativeMethods.OpenProcess((PROCESS_ACCESS_RIGHTS)rights, inheritHandle, processId);
            return handle.Value;
        }
    }
}
