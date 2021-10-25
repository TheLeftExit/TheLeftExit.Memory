using System;

namespace TheLeftExit.Memory {
    public unsafe abstract class MemorySource {
        // Core implementation
        protected abstract bool TryReadCore(ulong address, int count, void* buffer);
        protected abstract bool TryWriteCore(ulong address, int count, void* buffer);
        public virtual bool AllowRead { get; } = true;
        public virtual bool AllowWrite { get; } = true;
        public bool TryRead(ulong address, int count, void* buffer) => AllowRead && TryReadCore(address, count, buffer);
        public bool TryWrite(ulong address, int count, void* buffer) => AllowWrite && TryWriteCore(address, count, buffer);

        // Read API
        public bool TryRead<T>(ulong address, out T result) where T : unmanaged {
            fixed (void* ptr = &result)
                return TryRead(address, sizeof(T), ptr);
        }
        public bool TryRead<T>(ulong address, Span<T> buffer) where T : unmanaged {
            fixed (void* ptr = buffer)
                return TryRead(address, buffer.Length * sizeof(T), ptr);
        }
        public T Read<T>(ulong address) where T : unmanaged {
            return TryRead(address, out T result) ? result : throw new ApplicationException($"Unable to read at {address:X}.");
        }

        // Write API
        public bool TryWrite<T>(ulong address, T value) where T : unmanaged {
            return TryWrite(address, sizeof(T), &value);
        }
        public bool TryWrite<T>(ulong address, Span<T> buffer) where T : unmanaged {
            fixed (void* ptr = buffer)
                return TryWrite(address, buffer.Length * sizeof(T), ptr);
        }
        public void Write<T>(ulong address, T value) where T : unmanaged {
            if (!TryWrite(address, value)) throw new ApplicationException($"Unable to write at {address:X}.");
        }
    }
}
