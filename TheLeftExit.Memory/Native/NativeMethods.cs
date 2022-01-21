using System;
using System.Runtime.InteropServices;

namespace TheLeftExit.Memory.Native {
    // Method signatures mostly transcribed from CsWin32 outputs.
    internal unsafe class NativeMethods {
        [DllImport("DbgHelp", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern uint UnDecorateSymbolName(void* name, void* outputString, uint maxStringLength, uint flags);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool ReadProcessMemory(HANDLE hProcess, void* lpBaseAddress, void* lpBuffer, nuint nSize, out nuint lpNumberOfBytesRead);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool WriteProcessMemory(HANDLE hProcess, void* lpBaseAddress, void* lpBuffer, nuint nSize, out nuint lpNumberOfBytesWritten);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool IsWow64Process(HANDLE hProcess, out bool Wow64Process);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern HANDLE OpenProcess(PROCESS_ACCESS_RIGHTS dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern bool CloseHandle(HANDLE hObject);

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern uint GetProcessId(HANDLE Process);
    }

    [Flags]
    public enum PROCESS_ACCESS_RIGHTS : uint {
        PROCESS_TERMINATE = 0x00000001,
        PROCESS_CREATE_THREAD = 0x00000002,
        PROCESS_SET_SESSIONID = 0x00000004,
        PROCESS_VM_OPERATION = 0x00000008,
        PROCESS_VM_READ = 0x00000010,
        PROCESS_VM_WRITE = 0x00000020,
        PROCESS_DUP_HANDLE = 0x00000040,
        PROCESS_CREATE_PROCESS = 0x00000080,
        PROCESS_SET_QUOTA = 0x00000100,
        PROCESS_SET_INFORMATION = 0x00000200,
        PROCESS_QUERY_INFORMATION = 0x00000400,
        PROCESS_SUSPEND_RESUME = 0x00000800,
        PROCESS_QUERY_LIMITED_INFORMATION = 0x00001000,
        PROCESS_SET_LIMITED_INFORMATION = 0x00002000,
        PROCESS_ALL_ACCESS = 0x001FFFFF,
        PROCESS_DELETE = 0x00010000,
        PROCESS_READ_CONTROL = 0x00020000,
        PROCESS_WRITE_DAC = 0x00040000,
        PROCESS_WRITE_OWNER = 0x00080000,
        PROCESS_SYNCHRONIZE = 0x00100000,
        PROCESS_STANDARD_RIGHTS_REQUIRED = 0x000F0000,
    }
}
