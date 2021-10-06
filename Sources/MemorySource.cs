using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TheLeftExit.Memory.Sources
{
    /// <summary>
    /// Base class for system resources that support memory reading.
    /// </summary>
    public abstract unsafe class MemorySource {
        public abstract bool TryRead(ulong address, int count, void* buffer);

        public bool TryRead<T>(ulong address, out T result) where T : unmanaged {
            result = default;
            return TryRead(address, sizeof(T), Unsafe.AsPointer(ref result));
        }

        public T? Read<T>(ulong address) where T : unmanaged =>
            TryRead(address, out T result) ? result : null;

        public T ReadValue<T>(ulong address) where T : unmanaged {
            if (TryRead(address, out T result))
                return result;
            throw new ApplicationException($"Unable to read {sizeof(T)} bytes at {address:X}");
        }

        public bool TryRead(ulong address, int count, Span<byte> buffer) {
            fixed (void* ptr = buffer)
                return TryRead(address, count, ptr);
        }

        public bool TryRead(ulong address, int count, Memory<byte> buffer) {
            using (MemoryHandle handle = buffer.Pin())
                return TryRead(address, count, handle.Pointer);
        }
    }

}
