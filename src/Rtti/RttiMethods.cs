using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Windows.Win32;
using Windows.Win32.Foundation;

namespace TheLeftExit.Memory.Rtti {
    public enum AddressRelation {
        VTable = 0,
        Instance = 1,
        Reference = 2
    }

    public delegate string GetClassName(MemorySource source, ulong address, AddressRelation relation);

    public static unsafe class RttiExtensions {
        private const int BUFFER_SIZE = 60;

        // Pointer depth: 0 for vftable address, 1 for instance address, 2 for pointer to an instance.
        public static string GetClassName64(this MemorySource source, ulong address, AddressRelation relation = AddressRelation.Instance) {
            int pointerDepth = (int)relation;
            for (sbyte i = 0; i < pointerDepth; i++)
                if (!source.TryRead(address, out address)) return null;

            if (!source.TryRead(address - 0x08, out ulong object_locator)) return null;
            if (!source.TryRead(object_locator + 0x14, out ulong base_offset)) return null;
            ulong base_address = object_locator - base_offset;
            if (!source.TryRead(object_locator + 0x0C, out uint type_descriptor_offset)) return null;
            ulong class_name = base_address + type_descriptor_offset + 0x10 + 0x04;
            byte* buffer = stackalloc byte[BUFFER_SIZE];
            buffer[0] = (byte)'?';
            if (!source.TryRead(class_name, BUFFER_SIZE - 1, buffer + 1)) return null;
            byte* target = stackalloc byte[BUFFER_SIZE];
            uint len = DbgHelp.UnDecorateSymbolName(new PCSTR(buffer), new PSTR(target), BUFFER_SIZE, 0x1800);
            return len != 0 ? Encoding.UTF8.GetString(target, (int)len) : null;
        }

        public static string GetClassName32(this MemorySource source, ulong address, AddressRelation relation = AddressRelation.Instance) {
            int pointerDepth = (int)relation;
            for (sbyte i = 0; i < pointerDepth; i++)
                if (!source.TryRead(address, out address)) return null;

            if (!source.TryRead(address - 0x04, out uint object_locator)) return null;
            if (!source.TryRead(object_locator + 0x06, out uint type_descriptor)) return null;
            ulong class_name = type_descriptor + 0x0C + 0x03;
            byte* buffer = stackalloc byte[BUFFER_SIZE];
            buffer[0] = (byte)'?';
            if (!source.TryRead(class_name, BUFFER_SIZE - 1, buffer + 1)) return null;
            byte* target = stackalloc byte[BUFFER_SIZE];
            uint len = DbgHelp.UnDecorateSymbolName(new PCSTR(buffer), new PSTR(target), BUFFER_SIZE, 0x1000);
            return len != 0 ? Encoding.UTF8.GetString(target, (int)len) : null;
        }
    }
}
