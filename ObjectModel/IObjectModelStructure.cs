using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TheLeftExit.Memory.Queries;
using TheLeftExit.Memory.Sources;
using static TheLeftExit.Memory.Queries.Conditions;

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
                throw new ApplicationException();
            CachedMemory memory = new CachedMemory(structure.BaseAddress, structure.Size);
            if (!structure.Source.TryRead(structure.BaseAddress, memory.Memory.Span))
                throw new ApplicationException();
            return new T {
                BaseAddress = structure.BaseAddress,
                Source = memory
            };
        }

        public static TTo BranchByVal<TFrom, TTo>(this TFrom root, PointerQuery query, PointerQueryOptions options = PointerQueryOptions.ForceCached)
            where TFrom : IObjectModelStructure
            where TTo : IObjectModelStructure, new() {
            PointerQueryResult? result = query.GetResult(root.Source, root.BaseAddress, options);
            if (!result.HasValue)
                throw new ApplicationException();
            return new TTo {
                BaseAddress = result.Value.Target,
                Source = root.Source
            };
        }

        public static TTo BranchByRef<TFrom, TTo>(this TFrom root, PointerQuery query, PointerQueryOptions options = PointerQueryOptions.ForceCached)
            where TFrom : IObjectModelStructure
            where TTo : IObjectModelStructure, new() {
            PointerQueryResult? result = query.GetResult(root.Source, root.BaseAddress, options);
            if (!result.HasValue || root.Source.TryRead(result.Value.Target, out ulong target))
                throw new ApplicationException();
            return new TTo {
                BaseAddress = target,
                Source = root.Source
            };
        }
    }
}
