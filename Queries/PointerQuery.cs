using System;
using System.Runtime.CompilerServices;

using TheLeftExit.Memory.Sources;
using TheLeftExit.Memory.ObjectModel;

namespace TheLeftExit.Memory.Queries {
    public partial class PointerQuery {
        public PointerQueryOptions Settings { get; set; }
        public object Tag { get; set; }
        public uint? Offset { get; set; }
        public EventHandler<ProcessStepEventArgs> ProcessStep { get; set; }

        public ulong? Run(ReadOnlyMemorySource source, ulong baseAddress) {
            if (Offset.HasValue && !Settings.IgnoreOffset) {
                ulong cachedResult = Settings.ScanForward ? baseAddress + Offset.Value : baseAddress - Offset.Value;
                if (DoStep(source, cachedResult) == ProcessStepResult.Return)
                    return cachedResult;
            }

            Offset = RunCore(source, baseAddress);
            if (!Offset.HasValue)
                return null;
            return Settings.ScanForward ? baseAddress + Offset.Value : baseAddress - Offset.Value;
        }

        private uint? RunCore(ReadOnlyMemorySource source, ulong baseAddress) {
            bool forward = Settings.ScanForward;
            uint range = Settings.Range;
            uint step = Settings.Step;

            ulong address = baseAddress;
            ulong limit = forward ? address + range : address - range;
            uint? result = null;

            args.Source = source;

            bool DoProcessStep() {
                switch (DoStep(source, address)) {
                    case ProcessStepResult.Break:
                        result = null;
                        return true;
                    case ProcessStepResult.Return:
                        result = forward ? (uint)(address - baseAddress) : (uint)(baseAddress - address);
                        return true;
                    default:
                        return false;
                }
            }

            if (forward)
                while (address < limit) {
                    if (DoProcessStep()) return result;
                    address += step;
                }
            else
                while(address > limit) {
                    if (DoProcessStep()) return result;
                    address -= step;
                }

            return null;
        }

        private ProcessStepEventArgs args = new ProcessStepEventArgs();
        private ProcessStepResult DoStep(ReadOnlyMemorySource source, ulong address) {
            args.Source = source;
            args.Address = address;
            args.Result = null;
            ProcessStep?.Invoke(this, args);
            if (!args.Result.HasValue)
                throw new ApplicationException("A pointer query step was not handled.");
            return args.Result.Value;
        }
    }

    public class PointerQueryOptions {
        public bool IgnoreOffset { get; set; }
        public bool ValidateOffset { get; set; }
        public bool ScanForward { get; set; }
        public uint Range { get; set; }
        public uint Step { get; set; }
    }

    public class ProcessStepEventArgs : EventArgs {
        public ReadOnlyMemorySource Source { get; internal set; }
        public ulong Address { get; internal set; }
        public ProcessStepResult? Result { get; set; }
    }

    public enum ProcessStepResult {
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
}
