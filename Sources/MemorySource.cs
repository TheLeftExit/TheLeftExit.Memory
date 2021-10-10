using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TheLeftExit.Memory.Sources {
    // Copy-based operations on remote memory.
    public unsafe abstract class MemorySource {
        public abstract bool TryRead(ulong address, int count, void* buffer);

        public bool TryRead<T>(ulong address, out T result) where T : unmanaged {
            fixed (void* ptr = &result)
                return TryRead(address, sizeof(T), ptr);
        }

        public bool TryRead<T>(ulong address, Span<T> buffer) where T : unmanaged {
            fixed (void* ptr = buffer)
                return TryRead(address, buffer.Length * sizeof(T), ptr);
        }

        public T? Read<T>(ulong address) where T : unmanaged {
            return TryRead(address, out T result) ? result : null;
        }

        public T ReadValue<T>(ulong address) where T : unmanaged {
            if (TryRead(address, out T result))
                return result;
            throw new ApplicationException($"Unable to read at {address:X}");
        }
    }

    // Writing operations on remote memory.
    public unsafe abstract class WriteableMemorySource : MemorySource {
        public abstract bool TryWrite(ulong address, int count, void* buffer);

        public bool TryWrite<T>(ulong address, T value) where T : unmanaged {
            return TryWrite(address, sizeof(T), &value);
        }

        public bool TryWrite<T>(ulong address, ref T value) where T : unmanaged {
            fixed (void* ptr = &value)
                return TryWrite(address, sizeof(T), ptr);
        }

        public bool TryWrite<T>(ulong address, Span<T> buffer) where T : unmanaged {
            fixed (void* ptr = buffer)
                return TryWrite(address, buffer.Length * sizeof(T), ptr);
        }
    }

    // Reference-based operations on memory allocated by the program.
    public unsafe abstract class LocalMemorySource : WriteableMemorySource {
        public abstract bool Contains(ulong address, int count);

        public abstract void* GetReference(ulong address);

        public ref T Get<T>(ulong address) where T : unmanaged {
            return ref Unsafe.AsRef<T>(GetReference(address));
        }

        public Span<T> Slice<T>(ulong address, int count) where T : unmanaged {
            return new Span<T>(GetReference(address), count);
        }

        public override bool TryRead(ulong address, int count, void* buffer) {
            if (Contains(address, count)) {
                Unsafe.CopyBlock(buffer, GetReference(address), (uint)count);
                return true;
            }
            return false;
        }

        public override unsafe bool TryWrite(ulong address, int count, void* buffer) {
            if(Contains(address, count)) {
                Unsafe.CopyBlock(GetReference(address), buffer, (uint)count);
                return true;
            }
            return false;
        }
    }
}
