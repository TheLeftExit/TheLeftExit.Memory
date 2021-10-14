using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLeftExit.Memory.RTTI;
using TheLeftExit.Memory.Sources;

namespace TheLeftExit.Memory.Queries {
    public static class Conditions {
        public static PointerQuery.Condition AOB(params Byte?[] pattern) => (MemorySource source, UInt64 addr) => {
            LocalMemorySource localSource = source as LocalMemorySource;
            Span<byte> buffer = stackalloc byte[pattern.Length];
            if (localSource != null) {
                if (!localSource.Contains(addr, pattern.Length))
                    return PointerQuery.ConditionResult.Break;
                buffer = localSource.Slice<byte>(addr, pattern.Length);
            } else {
                if (!source.TryRead(addr, buffer))
                    return PointerQuery.ConditionResult.Break;
            }
            for (int i = 0; i < buffer.Length; i++)
                if (pattern[i].HasValue && pattern[i] != buffer[i])
                    return PointerQuery.ConditionResult.Continue;
            return PointerQuery.ConditionResult.Return;
        };

        public static PointerQuery.Condition RTTIByRef(string name) => (MemorySource source, UInt64 addr) => {
            if (!source.TryRead(addr, out UInt64 target))
                return PointerQuery.ConditionResult.Break;
            if (source.GetRTTIClassNames64(target)?.Contains(name) ?? false)
                return PointerQuery.ConditionResult.Return;
            return PointerQuery.ConditionResult.Continue;
        };

        public static PointerQuery.Condition RTTIByVal(string name) => (MemorySource source, UInt64 addr) => {
            if (!source.TryRead<Byte>(addr, out _))
                return PointerQuery.ConditionResult.Break;
            if (source.GetRTTIClassNames64(addr)?.Contains(name) ?? false)
                return PointerQuery.ConditionResult.Return;
            return PointerQuery.ConditionResult.Continue;
        };
    }
}
