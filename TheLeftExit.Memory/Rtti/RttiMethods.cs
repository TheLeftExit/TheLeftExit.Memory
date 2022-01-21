using System.Text;
using TheLeftExit.Memory.Native;
using TheLeftExit.Memory.Sources;

using static TheLeftExit.Memory.Rtti.PointerDepth;

namespace TheLeftExit.Memory.Rtti {
    /// <summary>
    /// Specifies how an address relates to a structure in memory.
    /// </summary>
    public enum PointerDepth {
        /// <summary>
        /// <i>For internal use.</i> Indicates that the address points to the type's virtual function table.
        /// </summary>
        VTable = 0,
        /// <summary>
        /// Indicates that the address points to (or that the scanned range contains) the structure itself.
        /// </summary>
        Instance = 1,
        /// <summary>
        /// Indicates that the address points to (or that the scanned range contains) a pointer to the structure.
        /// </summary>
        Reference = 2
    }

    public delegate string GetClassName(MemorySource source, nuint address, PointerDepth depth = Instance);

    public static unsafe class RttiExtensions {
        private const int BUFFER_SIZE = 60;

        /// <summary>
        /// Attempts to retrieve an RTTI class name for a structure located at a given address in a 64-bit application. If no such name is found, returns null.
        /// </summary>
        public static string GetClassName64(this MemorySource source, nuint address, PointerDepth depth = Instance) {
            for (PointerDepth i = depth; i > VTable; i--)
                if (!source.TryRead(address, out address)) return null;

            if (!source.TryRead(address - 0x08, out ulong object_locator)) return null;
            if (!source.TryRead((nuint)object_locator + 0x14, out ulong base_offset)) return null;
            ulong base_address = object_locator - base_offset;
            if (!source.TryRead((nuint)object_locator + 0x0C, out uint type_descriptor_offset)) return null;
            ulong class_name = base_address + type_descriptor_offset + 0x10 + 0x04;
            byte* buffer = stackalloc byte[BUFFER_SIZE];
            buffer[0] = (byte)'?';
            if (!source.TryRead((nuint)class_name, BUFFER_SIZE - 1, buffer + 1)) return null;
            byte* target = stackalloc byte[BUFFER_SIZE];
            uint len = NativeMethods.UnDecorateSymbolName(buffer, target, BUFFER_SIZE, 0x1800);
            return len != 0 ? Encoding.UTF8.GetString(target, (int)len) : null;
        }

        /// <summary>
        /// Attempts to retrieve an RTTI class name for a structure located at a given address in a 32-bit application. If no such name is found, returns null.
        /// </summary>
        public static string GetClassName32(this MemorySource source, nuint address, PointerDepth depth = Instance) {
            for (PointerDepth i = depth; i > VTable; i--)
                if (!source.TryRead(address, out address)) return null;

            if (!source.TryRead(address - 0x04, out uint object_locator)) return null;
            if (!source.TryRead(object_locator + 0x06, out uint type_descriptor)) return null;
            nuint class_name = type_descriptor + 0x0C + 0x03;
            byte* buffer = stackalloc byte[BUFFER_SIZE];
            buffer[0] = (byte)'?';
            if (!source.TryRead(class_name, BUFFER_SIZE - 1, buffer + 1)) return null;
            byte* target = stackalloc byte[BUFFER_SIZE];
            uint len = NativeMethods.UnDecorateSymbolName(buffer, target, BUFFER_SIZE, 0x1000);
            return len != 0 ? Encoding.UTF8.GetString(target, (int)len) : null;
        }
    }
}
