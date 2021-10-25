using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TheLeftExit.Memory {
    public unsafe class LocalMemory : MemorySource {
        public readonly ulong BaseAddress;
        public readonly int Size;
        public readonly Memory<byte> Memory;

        protected byte* pointer;

        public LocalMemory(ulong baseAddress, int size) {
            BaseAddress = baseAddress;
            Size = size;
            Memory = new byte[size];
        }

        public Handle Pin() => new Handle(this);

        public struct Handle : IDisposable {
            private readonly MemoryHandle memoryHandle;
            private readonly LocalMemory memorySource;
            public byte* Pointer => memorySource.pointer;
            public Handle(LocalMemory source) {
                memorySource = source;
                memoryHandle = memorySource.Memory.Pin();
                memorySource.pointer = (byte*)memoryHandle.Pointer;
            }
            public void Dispose() {
                memorySource.pointer = null;
                memoryHandle.Dispose();
            }
        }

        public Span<T> Slice<T>(ulong address, int count) {
            int offset = (int)(address - BaseAddress);
            if (pointer != null)
                return new Span<T>(pointer + offset, count);
            return MemoryMarshal.CreateSpan(ref Unsafe.As<byte, T>(ref Memory.Span[offset]), count);
        }

        // MemorySource compatibility. Do not use directly.
        public bool Contains(ulong address, int count) =>
            BaseAddress <= address && BaseAddress + (uint)Size >= address + (uint)count;
        protected override bool TryReadCore(ulong address, int count, void* buffer) =>
            Contains(address, count) && Slice<byte>(address, count).TryCopyTo(new Span<byte>(buffer, count));
        protected override unsafe bool TryWriteCore(ulong address, int count, void* buffer) =>
            Contains(address, count) && new Span<byte>(buffer, count).TryCopyTo(Slice<byte>(address, count));
    }
}
