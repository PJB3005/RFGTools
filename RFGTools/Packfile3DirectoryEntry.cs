using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace RFGFormats
{
    class Packfile3DirectoryEntry
    {
        public uint NameOffset;
        public uint Sector;
        public uint DataOffset;
        public uint NameHash;
        public uint DataSize;
        public uint CompressedDataSize;
        public uint PackagePointer;

        public void Deserialize(BinaryReader File)
        {
            NameOffset = File.ReadUInt32();
            Sector = File.ReadUInt32();
            DataOffset = File.ReadUInt32();
            NameHash = File.ReadUInt32();
            DataSize = File.ReadUInt32();
            CompressedDataSize = File.ReadUInt32();
            PackagePointer = File.ReadUInt32();
        }

        public void Serialize(BinaryWriter File)
        {
            File.Write(NameOffset);
            File.Write(Sector);
            File.Write(DataOffset);
            File.Write(NameHash);
            File.Write(DataSize);
            File.Write(CompressedDataSize);
            File.Write(PackagePointer);
        }
    }
}
