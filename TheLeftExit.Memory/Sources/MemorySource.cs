using System;

namespace TheLeftExit.Memory.Sources {
    public class MemoryException : ApplicationException { }

    /// <summary>
    /// Abstract class for generically typed read/write operations on a remote memory source.
    /// </summary>
    public unsafe abstract class MemorySource {
        // Core implementation on pointers
        protected abstract bool TryReadCore(nuint address, nuint count, void* buffer);
        protected abstract bool TryWriteCore(nuint address, nuint count, void* buffer);
        public bool TryRead(nuint address, nuint count, void* buffer) => TryReadCore(address, count, buffer);
        public bool TryWrite(nuint address, nuint count, void* buffer) => TryWriteCore(address, count, buffer);

        // Generics
        public bool TryRead<T>(nuint address, out T result) where T : unmanaged {
            fixed (void* ptr = &result)
                return TryRead(address, (nuint)sizeof(T), ptr);
        }
        public T Read<T>(nuint address) where T : unmanaged {
            return TryRead(address, out T result) ? result : throw new MemoryException();
        }
        public bool TryWrite<T>(nuint address, T value) where T : unmanaged {
            return TryWrite(address, (nuint)sizeof(T), &value);
        }
        public bool TryWrite<T>(nuint address, in T value) where T : unmanaged {
            fixed (void* ptr = &value)
                return TryWrite(address, (nuint)sizeof(T), ptr);
        }
        public void Write<T>(nuint address, T value) where T : unmanaged {
            if (!TryWrite(address, value)) throw new MemoryException();
        }
        public void Write<T>(nuint address, in T value) where T : unmanaged {
            if (!TryWrite(address, value)) throw new MemoryException();
        }

        // Spans
        public bool TryWrite<T>(nuint address, Span<T> buffer) where T : unmanaged {
            fixed (void* ptr = buffer)
                return TryWrite(address, (nuint)buffer.Length * (nuint)sizeof(T), ptr);
        }
        public bool TryRead<T>(nuint address, Span<T> buffer) where T : unmanaged {
            fixed (void* ptr = buffer)
                return TryRead(address, (nuint)buffer.Length * (nuint)sizeof(T), ptr);
        }
    }
}