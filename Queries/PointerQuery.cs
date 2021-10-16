using System;
using System.Runtime.CompilerServices;

using TheLeftExit.Memory.Sources;

namespace TheLeftExit.Memory.Queries {
    public class PointerQuery {
        private Condition condition;
        private uint range;
        private byte step;
        private bool forward;

        public int? Offset { get; set; }

        public PointerQuery(Condition queryCondition, uint maxOffset, byte scanStep, bool scanForward = true) {
            if (queryCondition == null || scanStep == 0)
                throw new ArgumentException();
            condition = queryCondition;
            step = scanStep;
            range = maxOffset;
            forward = scanForward;
        }

        public PointerQuery(SimpleCondition queryCondition, uint maxOffset, byte scanStep, bool scanForward = true) :
            this(ConvertCondition(queryCondition), maxOffset, scanStep, scanForward) { }

        public ulong? GetResult(MemorySource source, UInt64 baseAddress, Options options = Options.ValidateCached) {
            if (options == Options.ValidateCached && Offset.HasValue) {
                ConditionResult result = condition(source, applyOffset(baseAddress, Offset.Value));
                if (result != ConditionResult.Return)
                    Offset = null;
            }
            if (options == Options.ForceNew || !Offset.HasValue) {
                Offset = Run(source, baseAddress);
            }
            if (Offset.HasValue) {
                uint offsetAbs = (uint)Math.Abs(Offset.Value);
                ulong newTarget = forward ? baseAddress + offsetAbs : baseAddress - offsetAbs;
                return newTarget;
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int? Run(MemorySource source, UInt64 baseAddress) {
            for (uint offset = 0; offset <= range; offset += step) {
                ulong targetAddress = forward ? baseAddress + offset : baseAddress - offset;
                ConditionResult result = condition(source, targetAddress);
                if (result == ConditionResult.Return) {
                    return forward ? (int)offset : -(int)offset;
                } else if (result == ConditionResult.Break)
                    break;
            }
            return null;
        }

        // A version of "ulong + long" that doesn't confuse the compiler.
        private UInt64 applyOffset(UInt64 baseAddress, Int64 delta) {
            UInt32 deltaAbs = (UInt32)Math.Abs(delta);
            bool scanForward = delta > 0;
            return scanForward ? baseAddress + deltaAbs : baseAddress - deltaAbs;
        }

        public enum Options {
            /// <summary>
            /// Use the cached offset without validating it, unless it's empty.
            /// </summary>
            ForceCached,
            /// <summary>
            /// Invalidate the cached offset, forcing a new scan.
            /// </summary>
            ForceNew,
            /// <summary>
            /// Check the cached offset before using it; if it doesn't match, invalidate it, forcing a new scan.
            /// </summary>
            ValidateCached,
        }

        public enum ConditionResult {
            /// <summary>
            /// Treat the result as not matching the condition, continue scanning.
            /// </summary>
            Continue,
            /// <summary>
            /// Treat the result as matching the condition, return its address and offset.
            /// </summary>
            Return,
            /// <summary>
            /// Interrupt the scan, return null.
            /// </summary>
            Break
        }

        public delegate ConditionResult Condition(MemorySource memorySource, ulong address);
        public delegate bool SimpleCondition(MemorySource memorySource, ulong address);
        private static Condition ConvertCondition(SimpleCondition condition) =>
            (ms, ba) => condition(ms, ba) ? ConditionResult.Return : ConditionResult.Continue;
    }
}
