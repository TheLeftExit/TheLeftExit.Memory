using System;
using System.Runtime.CompilerServices;

using TheLeftExit.Memory.Sources;

namespace TheLeftExit.Memory.Queries {
    public class PointerQuery {
        private PointerQueryCondition condition;
        private UInt32 range;
        private SByte step;

        public Int64? Offset { get; set; }

        public PointerQuery(PointerQueryCondition queryCondition, UInt32 maxOffset, SByte scanStep, Int64? offset = null) {
            if (queryCondition == null || scanStep == 0)
                throw new ArgumentException();
            condition = queryCondition;
            range = maxOffset;
            step = scanStep;
            Offset = offset;
        }

        public PointerQueryResult? GetResult(MemorySource source, UInt64 baseAddress, PointerQueryOptions options = PointerQueryOptions.ValidateCached) {
            if (options == PointerQueryOptions.ValidateCached && Offset.HasValue) {
                PointerQueryConditionResult result = condition(source, applyOffset(baseAddress, Offset.Value));
                if (result != PointerQueryConditionResult.Return)
                    Offset = null;
            }
            if (options == PointerQueryOptions.ForceNew || !Offset.HasValue) {
                Offset = run(source, baseAddress);
            }
            if (Offset.HasValue) {
                UInt32 offsetAbs = (uint)Math.Abs(Offset.Value);
                bool scanForward = Offset.Value > 0;
                UInt64 newTarget = scanForward ? baseAddress + offsetAbs : baseAddress - offsetAbs;
                return new PointerQueryResult() {
                    Target = newTarget,
                    Offset = Offset.Value
                };
            } else
                return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private Int64? run(MemorySource source, UInt64 baseAddress) {
            Byte stepAbs = (Byte)Math.Abs(step);
            bool scanForward = step > 0;
            for (UInt32 offset = 0; offset <= range; offset += stepAbs) {
                UInt64 targetAddress = scanForward ? baseAddress + offset : baseAddress - offset;
                PointerQueryConditionResult result = condition(source, targetAddress);
                if (result == PointerQueryConditionResult.Return)
                    return scanForward ? offset : -offset;
                else if (result == PointerQueryConditionResult.Break)
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
    }

    public delegate PointerQueryConditionResult PointerQueryCondition(MemorySource memorySource, UInt64 address);

    public enum PointerQueryConditionResult {
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

    public enum PointerQueryOptions {
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

    public struct PointerQueryResult {
        public UInt64 Target { get; init; }
        public Int64 Offset { get; init; }
    }
}
