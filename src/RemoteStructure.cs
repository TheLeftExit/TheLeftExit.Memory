using System;
using System.Collections.Generic;

namespace TheLeftExit.Memory {
    public partial class ProcessMemory {
        public Dictionary<(string SourceClass, string TargetClass, bool ByRef), int> Offsets { get; } = new(); // Feel free to serialize.
        public RemoteStructure Root => new(this, BaseAddress, RemoteStructure.RootName);
    }

    public interface IRemoteStructure {
        public MemorySource Source { get; init; }
        public ulong Address { get; init; }
    }

    public struct RemoteStructure {
        public const string RootName = "`ROOT";
        public const string EmptyName = "`EMPTY";

        public readonly ProcessMemory Source;
        public readonly ulong Address;
        public readonly string Name;

        public RemoteStructure(ProcessMemory source, ulong address, string name = EmptyName) {
            if (source == null)
                throw new ArgumentException();
            Source = source;
            Address = address;
            Name = name;
        }

        public T As<T>() where T : IRemoteStructure, new() {
            return new T {
                Source = Source,
                Address = Address
            };
        }

        public T Read<T>(int offset) where T : unmanaged {
            ulong address = offset >= 0 ? Address + (uint)offset : Address - (uint)(-offset);
            return Source.Read<T>(address);
        }

        public void Write<T>(int offset, T value) where T : unmanaged {
            ulong address = offset >= 0 ? Address + (uint)offset : Address - (uint)(-offset);
            Source.Write(address, value);
        }

        public RemoteStructure this[int offset, bool byRef = true, string name = EmptyName] => Branch(offset, byRef, name);
        public RemoteStructure this[string className, bool byRef = true] => Branch(className, byRef);

        private RemoteStructure Branch(int offset, bool byRef, string name = EmptyName) {
            ulong address = offset >= 0 ? Address + (uint)offset : Address - (uint)(-offset);
            if (byRef) address = Source.Read<ulong>(address);
            return new(Source, address, name);
        }

        private RemoteStructure Branch(string className, bool byRef) {
            int offset;
            (string, string, bool) key = (Name, className, byRef);
            if (!Source.Offsets.TryGetValue(key, out offset)) {
                offset = Scan(className, byRef);
                if (Name != EmptyName || className != EmptyName)
                    Source.Offsets.Add(key, offset);
            }
            return Branch(offset, byRef, className);
        }

        private int Scan(string className, bool byRef) {
            ulong address = Address;
            ulong target;
            while (Source.TryRead(address, out target)) {
                string name = Source.Is32Bit ?
                    Source.GetRTTIClassName32(target, byRef ? 1 : 0) :
                    Source.GetRTTIClassName64(target, byRef ? 1 : 0);
                if (name == className) {
                    return checked((int)(address - Address));
                }
                address += Source.Is32Bit ? 4u : 8u;
            }
            throw new ApplicationException($"Failed to branch from {Name} to {className} by {(byRef ? "reference" : "value")}.");
        }
    }
}
