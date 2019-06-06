using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Linq;

namespace RFGFormats
{
    class Packfile3
    {
        //Header block
        public uint Signature;
        public uint Version;
        public char[] ShortName; //65 Bytes
        public char[] PathName; //256 Bytes
        //3 Bytes Padding 
        public uint Flags; //Values of Compressed and Condensed stored here in file.
        public bool Compressed;
        public bool Condensed;
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

        //Directory block
        public List<Packfile3DirectoryEntry> DirectoryEntries;

        //Filenames block
        public List<string> Filenames;

        //Data block


        /*End of packfile data. Below this point is runtime data, only used by this tool.*/
            
        //Reads the header, directory, and filenames blocks from the file.
        public void Deserialize(string PackfilePath, string OutputPath)
        {
            var vppName = Path.GetFileName(PackfilePath);

            ShortName = new char[65];
            PathName = new char[256];

            DirectoryEntries = new List<Packfile3DirectoryEntry>();
            Filenames = new List<string>();
            string PackfileName = Path.GetFileName(PackfilePath);
            System.IO.Directory.CreateDirectory(OutputPath);
            var FileInfo = new FileInfo(PackfilePath);
            Console.WriteLine("Extracting " + PackfileName + "...");
            if (FileInfo.Length <= 2048)
            {
                Console.WriteLine("Cancelled extraction of {0}. Packfile is empty!", PackfileName);
                return;
            }
            Console.WriteLine(PackfileName + "> Reading header data...");

            var File = new BinaryReader(new FileStream(PackfilePath, FileMode.Open));
            
            Signature = File.ReadUInt32();
            if (Signature != 1367935694) //Hex: 0xCE0A8951
            {
                throw (new Exception("Error! Invalid packfile signature"));
            }
            Version = File.ReadUInt32();
            if (Version != 3)
            {
                throw (new Exception("Error! Invalid packfile version. Expected version 3. The detected version is " + Version.ToString()));
            }
            ShortName = File.ReadChars(65);
            PathName = File.ReadChars(256);
            File.ReadBytes(3); //3 Bytes Padding
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

            for (int i = 0; i < NumberOfFiles; i++)
            {
                var Entry = new Packfile3DirectoryEntry();
                Entry.Deserialize(File);
                DirectoryEntries.Add(Entry);
            }
            File.ReadBytes(2048 - ((int)File.BaseStream.Position % 2048)); //Alignment Padding

            for (int i = 0; i < NumberOfFiles; i++)
            {
                var Name = new StringBuilder();
                do
                {
                    Name.Append(File.ReadChar());
                }
                while (File.PeekChar() != 0);
                Filenames.Add(Name.ToString());
                File.ReadByte(); //Move past null byte
            }
            File.ReadBytes(2048 - ((int)File.BaseStream.Position % 2048)); //Alignment Padding
            long Position = File.BaseStream.Position; //For debug usage

            if(Compressed && Condensed)
            {
                //Inflate whole data block
                byte[] CompressedData = new byte[CompressedDataSize];
                byte[] DecompressedData = new byte[DataSize];
                File.Read(CompressedData, 0, (int)CompressedDataSize);

                int DecompressedSizeResult = 0;
                using (MemoryStream Memory = new MemoryStream(CompressedData))
                {
                    using (InflaterInputStream Inflater = new InflaterInputStream(Memory))
                    {
                        DecompressedSizeResult = Inflater.Read(DecompressedData, 0, (int)DataSize);
                    }
                }
                if (DecompressedSizeResult != DataSize)
                {
                    Console.WriteLine("Error while deflating " + PackfileName + "! Decompressed data size " + DecompressedSizeResult.ToString() + " bytes, while it should be " + DataSize + " bytes according to header data");
                    throw new Exception("Error while deflating " + PackfileName + "! Decompressed data size " + DecompressedSizeResult.ToString() + " bytes, while it should be " + DataSize + " bytes according to header data");
                }
                long DecompressedPosition = 0;
                foreach (var Entry in DirectoryEntries.Select((Value, Index) => new { Index, Value }))
                {
                    DecompressedPosition = Entry.Value.DataOffset;
                    Console.Write(PackfileName + "> Extracting " + Filenames[Entry.Index].ToString() + "...");
                    var Writer = new BinaryWriter(System.IO.File.Create(OutputPath + Filenames[Entry.Index]));
                    //Writer.Write(DecompressedData, (int)DecompressedPosition, (int)Entry.Value.DataSize);
                    for(long i = 0; i < Entry.Value.DataSize; i++)
                    {
                        Writer.Write(DecompressedData[DecompressedPosition + i]);
                    }
                    DecompressedPosition += Entry.Value.DataSize;
                    Console.Write(" Done!\n");
                }     
            }
            else
            {
                if(Compressed)
                {
                    //Inflate block by block
                    foreach (var Entry in DirectoryEntries.Select((Value, Index) => new { Index, Value }))
                    {
                        Console.Write(PackfileName + "> Extracting " + Filenames[Entry.Index].ToString() + "...");
                        byte[] CompressedData = new byte[Entry.Value.CompressedDataSize];
                        byte[] DecompressedData = new byte[Entry.Value.DataSize];
                        File.Read(CompressedData, 0, (int)Entry.Value.CompressedDataSize);

                        int DecompressedSizeResult = 0;
                        using (var Memory = new MemoryStream(CompressedData))
                        {
                            using (InflaterInputStream Inflater = new InflaterInputStream(Memory))
                            {
                                DecompressedSizeResult = Inflater.Read(DecompressedData, 0, (int)Entry.Value.DataSize);
                            }
                        }
                        if(DecompressedSizeResult != Entry.Value.DataSize)
                        {
                            Console.WriteLine("Error while deflating " + Filenames[Entry.Index].ToString() + " in " + "! Decompressed data size " + DecompressedSizeResult.ToString() + " bytes, while it should be " + Entry.Value.DataSize + " bytes according to header data");
                            throw new Exception("Error while deflating " +  Filenames[Entry.Index].ToString() + " in " + PackfileName + "! Decompressed data size " + DecompressedSizeResult.ToString() + " bytes, while it should be " + Entry.Value.DataSize + " bytes according to header data");
                        }
                        else
                        {
                            System.IO.File.WriteAllBytes(OutputPath + Filenames[Entry.Index], DecompressedData);
                        }
                        int Remainder = (int)File.BaseStream.Position % 2048;
                        if(Remainder > 0)
                        {
                            File.ReadBytes(2048 - Remainder); //Alignment Padding
                        }
                        Console.Write(" Done!\n");
                    }
                }
                else
                {
                    //Copy data into individual files
                    foreach (var Entry in DirectoryEntries.Select((Value, Index) => new { Index, Value }))
                    {
                        Console.Write(PackfileName + "> Extracting " + Filenames[Entry.Index].ToString() + "...");
                        byte[] FileData = new byte[Entry.Value.DataSize];
                        //File.Read(FileData, (int)Entry.Value.DataOffset, (int)Entry.Value.DataSize);
                        File.Read(FileData, 0, (int)Entry.Value.DataSize);
                        System.IO.File.WriteAllBytes(OutputPath + Filenames[Entry.Index], FileData);
                        
                        if (!Condensed)
                        {
                            //If you remove the parentheses here you'll break unpacking on terr01_l0.vpp_pc
                            int Remainder = (int)(File.BaseStream.Position % 2048);
                            if(Remainder > 0)
                            {
                                File.ReadBytes(2048 - Remainder); //Alignment Padding
                            }
                        }

                        var CorruptionCheck = new BinaryReader(new FileStream(OutputPath + Filenames[Entry.Index], FileMode.Open));
                        uint HeaderValue = CorruptionCheck.ReadUInt32();

                        if (FileInfo.Extension == ".str2_pc")
                        {
                            if (HeaderValue == 1367935694)
                            {
                                Console.Write(" Done!\n");
                            }
                            else
                            {
                                Console.Write(" Corrupted! Extraction error! Magic number: {0}\n", HeaderValue);
                            }
                        }
                        else
                        {
                            Console.Write(" Done!\n");
                        }
                    }
                }
            }
        }

        public void Serialize()
        {

        }
    }
}
