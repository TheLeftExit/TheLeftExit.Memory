using TheLeftExit.Memory.Sources;

using static TheLeftExit.Memory.Rtti.PointerDepth;

namespace TheLeftExit.Memory.Rtti {
    public readonly struct RttiWalker {
        public MemorySource Source { get; init; }
        public nuint Address { get; init; }
        public bool Is32Bit { get; init; }
        public nuint SeekRange { get; init; }
        public bool ThrowOnError { get; init; }

        private GetClassName _getClassName => Is32Bit ? RttiExtensions.GetClassName32 : RttiExtensions.GetClassName64;
        private nuint _step => Is32Bit ? 4u : 8u;

        public RttiWalker Offset(nuint offset) => this with { Address = Address + offset };
        public RttiWalker OffsetBackward(nuint offset) => this with { Address = Address - offset };
        public RttiWalker Dereference() => this with {
            Address = Source.TryRead(Address, out nuint newAddress) ? newAddress : !ThrowOnError ? 0u : throw new MemoryException()
        };

        public RttiWalker Seek(string className, PointerDepth depth, out nuint offset) {
            nuint currentOffset = 0;
            nuint currentAddress = Address;
            PointerDepth loweredDepth = depth - 1;
            while (currentOffset <= SeekRange && Source.TryRead(currentAddress, out nuint loweredAddress)) {
                string foundClassName = _getClassName(Source, loweredAddress, loweredDepth);
                if (className == foundClassName) {
                    offset = currentOffset;
                    RttiWalker result = Offset(currentOffset);
                    for (PointerDepth i = depth; i > Instance; i--) {
                        result = result.Dereference();
                    }
                    return result;
                }
                checked {
                    currentAddress += _step;
                    currentOffset += _step;
                }
            }
            offset = 0;
            return !ThrowOnError ? this with { Address = 0 } : throw new MemoryException();
        }

        public RttiWalker SeekBackward(string className, PointerDepth depth, out nuint offset) {
            nuint currentOffset = 0;
            nuint currentAddress = Address;
            PointerDepth loweredDepth = depth - 1;
            while (currentOffset <= SeekRange && Source.TryRead(currentAddress, out nuint loweredAddress)) {
                string foundClassName = _getClassName(Source, loweredAddress, loweredDepth);
                if (className == foundClassName) {
                    offset = currentOffset;
                    RttiWalker result = Offset(currentOffset);
                    for (PointerDepth i = depth; i > Instance; i--) {
                        result = result.Dereference();
                    }
                    return result;
                }
                checked {
                    currentAddress -= _step;
                    currentOffset += _step;
                }
            }
            offset = 0;
            return !ThrowOnError ? this with { Address = 0 } : throw new MemoryException();
        }
    }
}
