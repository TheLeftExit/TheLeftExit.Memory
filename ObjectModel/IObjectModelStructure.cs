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
        public MemorySource Source { get; init; }
        public int Size { get; }
    }

    public static class ObjectModelExtensions {
        public static T Cache<T>(this T structure)
        where T : IObjectModelStructure, new() {
            if (structure.Size <= 0)
                throw new ApplicationException($"{typeof(T).Name} does not support caching.");
            CachedMemory memory = new CachedMemory(structure.BaseAddress, structure.Size);
            if (!structure.Source.TryRead(structure.BaseAddress, memory.Memory.Span))
                throw new ApplicationException($"Failed to cache {typeof(T).Name} - memory could not be read.");
            return new T {
                BaseAddress = structure.BaseAddress,
                Source = memory
            };
        }

        public static TTo BranchByVal<TFrom, TTo>(this TFrom root, int offset)
        where TFrom : IObjectModelStructure
        where TTo : IObjectModelStructure, new() {
            return new TTo {
                BaseAddress = ApplyOffset(root.BaseAddress, offset),
                Source = root.Source
            };
        }

        public static TTo BranchByRef<TFrom, TTo>(this TFrom root, int offset)
        where TFrom : IObjectModelStructure
        where TTo : IObjectModelStructure, new() {
            if(!root.Source.TryRead(ApplyOffset(root.BaseAddress, offset), out ulong target))
                throw new ApplicationException($"Failed to branch from {typeof(TFrom).Name} to {typeof(TTo).Name} by reference.");
            return new TTo {
                BaseAddress = target,
                Source = root.Source
            };
        }

        public static TTo BranchByVal<TFrom, TTo>(this TFrom root, PointerQuery query, PointerQuery.Options options = PointerQuery.Options.ForceCached)
        where TFrom : IObjectModelStructure
        where TTo : IObjectModelStructure, new() {
            ulong? result = query.GetResult(root.Source, root.BaseAddress, options);
            if (!result.HasValue)
                throw new ApplicationException($"Failed to branch from {typeof(TFrom).Name} to {typeof(TTo).Name} by value.");
            return new TTo {
                BaseAddress = result.Value,
                Source = root.Source
            };
        }

        public static TTo BranchByRef<TFrom, TTo>(this TFrom root, PointerQuery query, PointerQuery.Options options = PointerQuery.Options.ForceCached)
        where TFrom : IObjectModelStructure
        where TTo : IObjectModelStructure, new() {
            ulong? result = query.GetResult(root.Source, root.BaseAddress, options);
            if (!result.HasValue || !root.Source.TryRead(result.Value, out ulong target))
                throw new ApplicationException($"Failed to branch from {typeof(TFrom).Name} to {typeof(TTo).Name} by reference.");
            return new TTo {
                BaseAddress = target,
                Source = root.Source
            };
        }

        // A version of "ulong + long" that doesn't confuse the compiler.
        public static UInt64 ApplyOffset(UInt64 baseAddress, Int64 delta) =>
            delta >= 0 ? baseAddress + (ulong)(delta) : baseAddress - (ulong)(-delta);
    }
}
