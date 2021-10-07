using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheLeftExit.Memory.RTTI;
using TheLeftExit.Memory.Sources;

namespace TheLeftExit.Memory.Queries {
    public static class Conditions {
        public static PointerQueryCondition AOB(params Byte?[] pattern) => (MemorySource source, UInt64 addr) => {
            Span<Byte> buffer = stackalloc Byte[pattern.Length];
            if (!source.TryRead(addr, buffer))
                return PointerQueryConditionResult.Break;
            for (int i = 0; i < buffer.Length; i++)
                if (pattern[i].HasValue && pattern[i] != buffer[i])
                    return PointerQueryConditionResult.Continue;
            return PointerQueryConditionResult.Return;
        };

        public static PointerQueryCondition RTTIByRef(string name) => (MemorySource source, UInt64 addr) => {
            if (!source.TryRead(addr, out UInt64 target))
                return PointerQueryConditionResult.Break;
            if (source.GetRTTIClassNames64(target)?.Contains(name) ?? false)
                return PointerQueryConditionResult.Return;
            return PointerQueryConditionResult.Continue;
        };

        public static PointerQueryCondition RTTIByVal(string name) => (MemorySource source, UInt64 addr) => {
            if (!source.TryRead<Byte>(addr, out _))
                return PointerQueryConditionResult.Break;
            if (source.GetRTTIClassNames64(addr)?.Contains(name) ?? false)
                return PointerQueryConditionResult.Return;
            return PointerQueryConditionResult.Continue;
        };

        public static PointerQueryCondition Forgive(this PointerQueryCondition condition) => (MemorySource source, UInt64 addr) => {
            PointerQueryConditionResult res = condition(source, addr);
            return res == PointerQueryConditionResult.Break ? PointerQueryConditionResult.Continue : res;
        };
    }
}
