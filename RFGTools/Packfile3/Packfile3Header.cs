using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RFGFormats
{
    public class Packfile3Header
    {
        //Header block
        public uint Signature;
        public uint Version;
        public char[] ShortName; //65 Bytes
        public char[] PathName; //256 Bytes
        //3 Bytes Padding 
        public uint Flags; //Values of Compressed and Condensed stored here in file.
        public bool Compressed; //Stored in flags in file, only here for convenience
        public bool Condensed; //Stored in flags in file, only here for convenience
        public uint Sector;
        public uint NumberOfFiles;
        public uint FileSize;
        public uint DirectoryBlockSize;
        public uint FilenameBlockSize;
        public uint DataSize;
        public uint CompressedDataSize;
        public uint DirectoryPointer;
        public uint FilenamesPointer;
        public uint DataPointer;
        public uint OpenCount;
        //1668 Bytes Padding

        public void Deserialize(BinaryReader File)
        {
            ShortName = new char[65];
            PathName = new char[256];

            Signature = File.ReadUInt32();
            if (Signature != 1367935694) //Hex: 0xCE0A8951
            {
                throw new Exception("Error! Invalid packfile signature");
            }
            Version = File.ReadUInt32();
            if (Version != 3)
            {
                throw new Exception("Error! Invalid packfile version. Expected version 3. The detected version is " + Version);
            }
            ShortName = File.ReadChars(65);
            PathName = File.ReadChars(256);
            File.ReadBytes(3); //3 Bytes padding
            Flags = File.ReadUInt32();
            if (Flags == 1)
            {
                Compressed = true;
            }
            if (Flags == 2)
            {
                Condensed = true;
            }
            if (Flags == 3)
            {
                Compressed = true;
                Condensed = true;
            }
            Sector = File.ReadUInt32();
            NumberOfFiles = File.ReadUInt32();
            FileSize = File.ReadUInt32();
            DirectoryBlockSize = File.ReadUInt32();
            FilenameBlockSize = File.ReadUInt32();
            DataSize = File.ReadUInt32();
            CompressedDataSize = File.ReadUInt32();
            DirectoryPointer = File.ReadUInt32();
            FilenamesPointer = File.ReadUInt32();
            DataPointer = File.ReadUInt32();
            OpenCount = File.ReadUInt32();
            File.ReadBytes(1668); //Padding to next chunk/block. VPPs are in made up of 2048 byte aligned chunks.
        }

        public void Serialize(BinaryWriter File)
        {
            File.Write(Signature);
            File.Write(Version);
            File.Write(ShortName); //Make sure this is 65 bytes before writing
            File.Write(PathName); //Make sure this is 256 bytes before writing
            File.Write(new byte []{0x0, 0x0, 0x0}); //3 Bytes padding
            File.Write(Flags);
            File.Write(Sector);
            File.Write(NumberOfFiles);
            File.Write(FileSize);
            File.Write(DirectoryBlockSize);
            File.Write(FilenameBlockSize);
            File.Write(DataSize);
            File.Write(CompressedDataSize);
            File.Write(DirectoryPointer);
            File.Write(FilenamesPointer);
            File.Write(DataPointer);
            File.Write(OpenCount);
            for (int i = 0; i < 1668; i++) //Write padding to next chunk/block.
            {
                File.Write((byte)0x0);
            }
        }
    }
}