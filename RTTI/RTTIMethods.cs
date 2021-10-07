using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Windows.Win32;
using Windows.Win32.Foundation;

using TheLeftExit.Memory.Sources;

namespace TheLeftExit.Memory.RTTI {
    public static unsafe class RTTIMethods {

        [MethodImpl(MethodImplOptions.Synchronized)]
        private static unsafe string Undecorate(byte* source, bool is32bit) {
            byte* target = stackalloc byte[60];
            uint len = DbgHelp.UnDecorateSymbolName(new PCSTR(source), new PSTR(target), 60, is32bit ? 0x1000u : 0x1800u);
            return len != 0 ? Encoding.UTF8.GetString(target, (int)len) : null;
        }

        /// <summary>
        /// Retrieves an RTTI-sourced list of class names for a structure.
        /// </summary>
        /// <param name="source">Memory source to read from.</param>
        /// <param name="address">Address of the structure.</param>
        /// <returns>Array of names, or <see cref="null"/> if unsuccessful.</returns>
        public static string[] GetRTTIClassNames64(this MemorySource source, ulong address) {
            if (!source.TryRead(address, out UInt64 struct_addr)) return null;
            if (!source.TryRead(struct_addr - 0x08, out UInt64 object_locator_ptr)) return null;
            if (!source.TryRead(object_locator_ptr + 0x14, out UInt64 base_offset)) return null;
            UInt64 base_address = object_locator_ptr - base_offset;
            if (!source.TryRead(object_locator_ptr + 0x10, out UInt32 class_hierarchy_descriptor_offset)) return null;
            UInt64 class_hierarchy_descriptor_ptr = base_address + class_hierarchy_descriptor_offset;
            if (!source.TryRead(class_hierarchy_descriptor_ptr + 0x08, out Int32 base_class_count)) return null;
            if (base_class_count < 1 || base_class_count > 24) return null;
            if (!source.TryRead(class_hierarchy_descriptor_ptr + 0x0C, out UInt32 base_class_array_offset)) return null;
            UInt64 base_class_array_ptr = base_address + base_class_array_offset;
            string[] result = new string[base_class_count];
            byte* base_class_name_buffer = stackalloc byte[60];
            base_class_name_buffer[0] = (byte)'?';
            Unsafe.Write(base_class_name_buffer, '?');
            for(uint i = 0; i < base_class_count; i++) {
                UInt64 base_class_ptr = base_class_array_ptr + 4 * i;
                if (!source.TryRead(base_class_ptr, out UInt32 base_class_descriptor_offset)) return null;
                UInt64 base_class_descriptor_ptr = base_address + base_class_descriptor_offset;
                if (!source.TryRead(base_class_descriptor_ptr, out UInt32 type_descriptor_offset)) return null;
                UInt64 type_descriptor_ptr = base_address + type_descriptor_offset;
                if (source.TryRead(type_descriptor_ptr + 0x14, 59, base_class_name_buffer + 1))
                    result[i] = Undecorate(base_class_name_buffer, false);
            }
            return result;
        }

        /// <summary>
        /// Retrieves an RTTI-sourced list of class names for a structure in an 32-bit process.
        /// </summary>
        /// <param name="source">Memory source to read from.</param>
        /// <param name="address">Address of the structure.</param>
        /// <returns>Array of names, or <see cref="null"/> if unsuccessful.</returns>
        public static string[] GetRTTIClassNames32(this MemorySource source, ulong address) {
            if (!source.TryRead(address, out UInt32 struct_addr)) return null;
            if (!source.TryRead(struct_addr - 0x04, out UInt32 object_locator_ptr)) return null;
            if (!source.TryRead(object_locator_ptr + 0x10, out UInt32 class_hierarchy_descriptor)) return null;
            if (!source.TryRead(class_hierarchy_descriptor + 0x08, out Int32 base_class_count)) return null;
            if (base_class_count < 1 || base_class_count > 24) return null;
            if (!source.TryRead(class_hierarchy_descriptor + 0x0C, out UInt32 base_class_array_ptr)) return null;
            string[] result = new string[base_class_count];
            byte* base_class_name_buffer = stackalloc byte[60];
            base_class_name_buffer[0] = (byte)'?';
            for (uint i = 0; i < base_class_count; i++) {
                UInt64 base_class_ptr = base_class_array_ptr + 4 * i;
                if (!source.TryRead(base_class_ptr, out UInt32 base_class_descriptor_ptr)) return null;
                if (!source.TryRead(base_class_descriptor_ptr, out UInt32 type_descriptor_ptr)) return null;
                if (source.TryRead(type_descriptor_ptr + 0x0C, 59, base_class_name_buffer + 1))
                    result[i] = Undecorate(base_class_name_buffer, true);
            }
            return result;
        }
    }
}
