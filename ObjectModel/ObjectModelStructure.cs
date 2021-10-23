using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TheLeftExit.Memory.Queries;
using TheLeftExit.Memory.Sources;

namespace TheLeftExit.Memory.ObjectModel {
    public interface IObjectModelStructure {
        public ulong BaseAddress { get; init; }
        public ReadOnlyMemorySource Source { get; init; }
    }

    public static class ObjectModelStructure {
        public static TTo BranchByVal<TFrom, TTo>(this TFrom root, uint offset)
        where TFrom : IObjectModelStructure
        where TTo : IObjectModelStructure, new() {
            return new TTo {
                BaseAddress = root.BaseAddress + offset,
                Source = root.Source
            };
        }

        public static TTo BranchByRef<TFrom, TTo>(this TFrom root, uint offset)
        where TFrom : IObjectModelStructure
        where TTo : IObjectModelStructure, new() {
            if(!root.Source.TryRead(root.BaseAddress + offset, out ulong target))
                throw new ApplicationException($"Failed to branch from {nameof(TFrom)} to {nameof(TTo)} by reference.");
            return new TTo {
                BaseAddress = target,
                Source = root.Source
            };
        }

        public static TValue ReadAt<TStruct, TValue>(this TStruct root, uint offset)
        where TStruct : IObjectModelStructure
        where TValue : unmanaged {
            return root.Source.Read<TValue>(root.BaseAddress + offset) ??
                throw new ApplicationException($"Failed to read at {nameof(TStruct)}+{offset:X}");
        }

        public static void WriteAt<TStruct, TValue>(this TStruct root, uint offset, TValue value)
        where TStruct : IObjectModelStructure
        where TValue : unmanaged {
            if (root.Source is MemorySource source)
                if (source.TryWrite(root.BaseAddress + offset, value))
                    return;
            throw new ApplicationException($"Failed to write at {nameof(TStruct)}+{offset:X}");
        }
    }
}
