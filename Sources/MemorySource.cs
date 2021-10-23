﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TheLeftExit.Memory.Sources {
    // Copy-based operations on remote memory.
    public unsafe abstract class ReadOnlyMemorySource {
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
    public unsafe abstract class MemorySource : ReadOnlyMemorySource {
        public abstract bool TryWrite(ulong address, int count, void* buffer);

        public bool TryWrite<T>(ulong address, T value) where T : unmanaged {
            return TryWrite(address, sizeof(T), &value);
        }

        public bool TryWrite<T>(ulong address, in T value) where T : unmanaged {
            fixed (void* ptr = &value)
                return TryWrite(address, sizeof(T), ptr);
        }

        public bool TryWrite<T>(ulong address, Span<T> buffer) where T : unmanaged {
            fixed (void* ptr = buffer)
                return TryWrite(address, buffer.Length * sizeof(T), ptr);
        }
    }

    // Reference-based operations on memory allocated by the program.
    public unsafe abstract class LocalMemorySource : MemorySource {
        public abstract Span<T> Slice<T>(ulong address, int count) where T : unmanaged;

        public ref T ReadRef<T>(ulong address) where T : unmanaged {
            return ref Slice<T>(address, 1).GetPinnableReference();
        }

        // WriteableMemorySource compatibility.
        public abstract bool Contains(ulong address, int count);
        public override bool TryRead(ulong address, int count, void* buffer) =>
            Contains(address, count) && Slice<byte>(address, count).TryCopyTo(new Span<byte>(buffer, count));
        public override unsafe bool TryWrite(ulong address, int count, void* buffer) =>
            Contains(address, count) && new Span<byte>(buffer, count).TryCopyTo(Slice<byte>(address, count));
    }
}
