using System;
using System.Runtime.CompilerServices;

namespace TheLeftExit.Memory.Native {
    /// <summary>
    /// Exposes process handle API from <c>Kernel32.dll</c>.<br/>
    /// Can be created using <see cref="HANDLE.OpenProcess"/>, or implicitly cast from <see cref="IntPtr"/>.
    /// </summary>
    public readonly unsafe struct HANDLE {
        public readonly IntPtr Value;
        public static implicit operator IntPtr(HANDLE handle) => Unsafe.As<HANDLE, IntPtr>(ref handle);
        public static implicit operator HANDLE(IntPtr value) => Unsafe.As<IntPtr, HANDLE>(ref value);

        public static HANDLE OpenProcess(uint dwProcessId) =>
            OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS, true, dwProcessId);

        public static HANDLE OpenProcess(PROCESS_ACCESS_RIGHTS dwDesiredAccess, bool bInheritHandle, uint dwProcessId) =>
            NativeMethods.OpenProcess(dwDesiredAccess, bInheritHandle, dwProcessId);

        public bool ReadProcessMemory(void* lpBaseAddress, void* lpBuffer, nuint nSize, out nuint lpNumberOfBytesRead) =>
            NativeMethods.ReadProcessMemory(this, lpBaseAddress, lpBuffer, nSize, out lpNumberOfBytesRead);

        public bool WriteProcessMemory(void* lpBaseAddress, void* lpBuffer, nuint nSize, out nuint lpNumberOfBytesWritten) =>
            NativeMethods.WriteProcessMemory(this, lpBaseAddress, lpBuffer, nSize, out lpNumberOfBytesWritten);

        public bool? IsWow64Process() =>
            NativeMethods.IsWow64Process(this, out bool result) ? result : null;

        public bool CloseHandle() =>
            NativeMethods.CloseHandle(this);

        public uint GetProcessId() =>
            NativeMethods.GetProcessId(this);

        public nuint GetBaseAddress() =>
            (nuint)System.Diagnostics.Process.GetProcessById((int)GetProcessId()).MainModule.BaseAddress.ToPointer();
    }
}
