using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TheLeftExit.Memory.Sources {
    public unsafe class CachedMemory : LocalMemorySource {
        public ulong BaseAddress { get; }
        public int Size { get; }
        public Memory<byte> Memory { get; }

        protected byte* pointer;

        public CachedMemory(ulong baseAddress, int size) {
            BaseAddress = baseAddress;
            Size = size;
            Memory = new byte[size];
        }

        public Handle Pin() => new Handle(this);

        public struct Handle : IDisposable {
            private readonly MemoryHandle memoryHandle;
            private readonly CachedMemory memorySource;
            public byte* Pointer => memorySource.pointer;
            public Handle(CachedMemory source) {
                memorySource = source;
                memoryHandle = memorySource.Memory.Pin();
                memorySource.pointer = (byte*)memoryHandle.Pointer;
            }
            public void Dispose() {
                memorySource.pointer = null;
            }
        }

        public override Span<T> Slice<T>(ulong address, int count) {
            int offset = (int)(address - BaseAddress);
            if (pointer != null)
                return new Span<T>(pointer + offset, count);
            using (MemoryHandle handle = Memory.Pin())
                return new Span<T>(Unsafe.Add<byte>(handle.Pointer, offset), count);
        }

        public override bool Contains(ulong address, int count) =>
            BaseAddress <= address && BaseAddress + (uint)Size >= address + (uint)count;
    }
}
