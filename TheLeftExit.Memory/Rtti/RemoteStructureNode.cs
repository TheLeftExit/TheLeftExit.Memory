// This is still a mess, I'll deal with it later...

/*
using System;
using System.Collections.Generic;

namespace TheLeftExit.Memory.Rtti {
    internal record RemoteStructureMapNode(string SourceClass, string TargetClass, AddressRelation Relation);

    public sealed class RemoteStructureRoot {
        public MemorySource Source { get; private init; }
        public ulong BaseAddress { get; private init; }

        internal GetClassName GetClassName;
        internal uint ScanIncrement;

        public static RemoteStructureRoot FromGenericSource(MemorySource source, ulong baseAddress, GetClassName getClassNameMethod, uint scanIncrement) {
            return new() {
                Source = source,
                BaseAddress = baseAddress,
                GetClassName = getClassNameMethod,
                ScanIncrement = scanIncrement
            };
        }

        public static RemoteStructureRoot FromProcess(ProcessMemorySource processMemory) {
            bool is64Bit = ProcessHelper.Is64BitProcess(processMemory.Handle);
            return new() {
                Source = processMemory,
                BaseAddress = ProcessHelper.GetBaseAddress(processMemory.Handle),
                GetClassName = is64Bit ? RttiExtensions.GetClassName64 : RttiExtensions.GetClassName32,
                ScanIncrement = is64Bit ? 8u : 4u
            };
        }

        public RemoteStructureNode RootNode => new(this, BaseAddress, RemoteStructureNode.RootName);

        internal Dictionary<RemoteStructureMapNode, int> Map;
    }

    public struct RemoteStructureNode {
        public const string RootName = "`ROOT";
        public const string EmptyName = "`EMPTY";

        public RemoteStructureRoot Root { get; }
        public ulong Address { get; }
        public string Name { get; }

        internal RemoteStructureNode(RemoteStructureRoot root, ulong address, string name = EmptyName) {
            Root = root;
            Address = address;
            Name = name;
        }

        public T As<T>() where T : IRemoteStructure, new() {
            return new T {
                Source = Root.Source,
                Address = Address
            };
        }

        public RemoteStructureNode BranchByOffset(int offset, AddressRelation relation = AddressRelation.Reference, string name = EmptyName) {
            ulong address = offset >= 0 ? Address + (uint)offset : Address - (uint)(-offset);
            if (relation is AddressRelation.Reference) address = Root.Source.Read<ulong>(address);
            return new(Root, address, name);
        }

        public RemoteStructureNode BranchByName(string className, AddressRelation relation = AddressRelation.Reference) {
            int offset;
            RemoteStructureMapNode mapNode = new(Name, className, relation);
            if (!Root.Map.TryGetValue(mapNode, out offset)) {
                offset = Scan(className, OffsetRelation(relation, -1));
                if (Name is not EmptyName && className is not EmptyName)
                    Root.Map.Add(mapNode, offset);
            }
            return BranchByOffset(offset, relation);
        }

        private int Scan(string className, AddressRelation relation) {
            ulong address = Address;
            MemorySource source = Root.Source;
            while (source.TryRead(address, out ulong target)) {
                string name = Root.GetClassName(source, target, relation);
                if (name == className) {
                    return checked((int)checked(address - Address));
                }
                address += Root.ScanIncrement;
            }
            throw new ApplicationException($"Failed to branch from {Name} to {className} by {OffsetRelation(relation, 1)}.");
        }

        private AddressRelation OffsetRelation(AddressRelation structureAddress, int offset) =>
            (AddressRelation)((int)structureAddress + offset);
    }

    public struct RemoteStructure : IRemoteStructure {
        public MemorySource Source { get; init; }
        public ulong Address { get; init; }

        public T Read<T>(int offset) where T : unmanaged {
            ulong address = offset >= 0 ? Address + (uint)offset : Address - (uint)(-offset);
            return Source.Read<T>(address);
        }

        public void Write<T>(int offset, T value) where T : unmanaged {
            ulong address = offset >= 0 ? Address + (uint)offset : Address - (uint)(-offset);
            Source.Write(address, value);
        }
    }

    public interface IRemoteStructure {
        public MemorySource Source { get; init; }
        public ulong Address { get; init; }
    }
}
*/