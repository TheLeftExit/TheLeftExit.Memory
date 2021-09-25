using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheLeftExit.Memory.Sources {
    /// <summary>
    /// <see cref="MemorySource"/> over an array of bytes.
    /// </summary>
    public class StoredMemory : MemorySource {
        protected byte[] file;

        public StoredMemory(byte[] source) {
            file = source;
        }

        public override unsafe bool ReadBytes(ulong address, nuint count, Span<byte> buffer) {
            return new Span<byte>(file, (int)address, (int)count).TryCopyTo(buffer);
        }
    }
}
