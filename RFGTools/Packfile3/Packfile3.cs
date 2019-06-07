using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Linq;

namespace RFGFormats
{
    public class Packfile3
    {
        //Header block
        public Packfile3Header Header;

        //Directory block
        public List<Packfile3DirectoryEntry> DirectoryEntries;

        //Filenames block
        public List<string> Filenames;
            
        //Reads the header, directory, and filenames blocks from the file.
        public void Deserialize(string PackfilePath, string OutputPath)
        {
            string packfileName = Path.GetFileName(PackfilePath);
            var packfileInfo = new FileInfo(PackfilePath);
            Console.WriteLine("Extracting " + packfileName + "...");
            if (packfileInfo.Length <= 2048)
            {
                Console.WriteLine("Cancelled extraction of {0}. Packfile is empty!", packfileName);
                return;
            }
            Directory.CreateDirectory(OutputPath);
            Console.WriteLine(packfileName + "> Reading header data...");

            var File = new BinaryReader(new FileStream(PackfilePath, FileMode.Open));
            Header = new Packfile3Header();
            Header.Deserialize(File);

            DirectoryEntries = new List<Packfile3DirectoryEntry>();
            for (int i = 0; i < Header.NumberOfFiles; i++)
            {
                var Entry = new Packfile3DirectoryEntry();
                Entry.Deserialize(File);
                DirectoryEntries.Add(Entry);
            }
            File.ReadBytes(2048 - ((int)File.BaseStream.Position % 2048)); //Alignment Padding

            Filenames = new List<string>();
            for (int i = 0; i < Header.NumberOfFiles; i++)
            {
                var name = new StringBuilder();
                do
                {
                    name.Append(File.ReadChar());
                }
                while (File.PeekChar() != 0);
                Filenames.Add(name.ToString());
                File.ReadByte(); //Move past null byte
            }
            File.ReadBytes(2048 - ((int)File.BaseStream.Position % 2048)); //Alignment Padding

            if(Header.Compressed && Header.Condensed)
            {
                DeserializeCompressedAndCondensed(PackfilePath, OutputPath, File);
            }
            else
            {
                if(Header.Compressed)
                {
                    DeserializeCompressed(PackfilePath, OutputPath, File);
                }
                else
                {
                    DeserializeDefault(PackfilePath, OutputPath, File);
                }
            }
        }

        private void DeserializeCompressedAndCondensed(string packfilePath, string outputPath, BinaryReader File)
        {
            string packfileName = Path.GetFileName(packfilePath);
            //Inflate whole data block
            byte[] compressedData = new byte[Header.CompressedDataSize];
            byte[] decompressedData = new byte[Header.DataSize];
            File.Read(compressedData, 0, (int)Header.CompressedDataSize);

            int decompressedSizeResult = 0;
            using (MemoryStream Memory = new MemoryStream(compressedData))
            {
                using (InflaterInputStream Inflater = new InflaterInputStream(Memory))
                {
                    decompressedSizeResult = Inflater.Read(decompressedData, 0, (int)Header.DataSize);
                }
            }
            if (decompressedSizeResult != Header.DataSize)
            {
                var errorString = new StringBuilder();
                errorString.AppendFormat(
                    "Error while deflating {0}! Decompressed data size is {1} bytes, while" +
                    " it should be {2} bytes according to header data.", packfileName,
                    decompressedSizeResult, Header.DataSize);
                Console.WriteLine(errorString.ToString());
                throw new Exception(errorString.ToString());
            }

            foreach (var entry in DirectoryEntries.Select((Value, Index) => new { Index, Value }))
            {
                long decompressedPosition = entry.Value.DataOffset;
                Console.Write("{0}> Extracting {1}...", packfileName, Filenames[entry.Index]);
                var writer = new BinaryWriter(System.IO.File.Create(outputPath + Filenames[entry.Index]));

                for (long i = 0; i < entry.Value.DataSize; i++)
                {
                    writer.Write(decompressedData[decompressedPosition + i]);
                }
                Console.WriteLine(" Done!");
            }
        }

        private void DeserializeCompressed(string packfilePath, string outputPath, BinaryReader File)
        {
            string packfileName = Path.GetFileName(packfilePath);
            //Inflate block by block
            foreach (var Entry in DirectoryEntries.Select((Value, Index) => new { Index, Value }))
            {
                Console.Write("{0}> Extracting {1}...", packfileName, Filenames[Entry.Index]);
                byte[] compressedData = new byte[Entry.Value.CompressedDataSize];
                byte[] decompressedData = new byte[Entry.Value.DataSize];
                File.Read(compressedData, 0, (int)Entry.Value.CompressedDataSize);

                int decompressedSizeResult = 0;
                using (var Memory = new MemoryStream(compressedData))
                {
                    using (InflaterInputStream Inflater = new InflaterInputStream(Memory))
                    {
                        decompressedSizeResult = Inflater.Read(decompressedData, 0, (int)Entry.Value.DataSize);
                    }
                }
                if (decompressedSizeResult != Entry.Value.DataSize)
                {
                    var errorString = new StringBuilder();
                    errorString.AppendFormat(
                        "Error while deflating {0} in {1}! Decompressed data size is {2} bytes, while" +
                        " it should be {3} bytes according to header data.", Filenames[Entry.Index],
                        packfileName, decompressedSizeResult, Entry.Value.DataSize);
                    Console.WriteLine(errorString.ToString());
                    throw new Exception(errorString.ToString());
                }
                System.IO.File.WriteAllBytes(outputPath + Filenames[Entry.Index], decompressedData);

                int remainder = (int)(File.BaseStream.Position % 2048);
                if (remainder > 0)
                {
                    File.ReadBytes(2048 - remainder); //Alignment Padding
                }
                Console.WriteLine(" Done!");
            }
        }

        private void DeserializeDefault(string packfilePath, string outputPath, BinaryReader File)
        {
            string packfileName = Path.GetFileName(packfilePath);
            //Copy data into individual files
            foreach (var Entry in DirectoryEntries.Select((Value, Index) => new { Index, Value }))
            {
                Console.Write("{0}> Extracting {1}...", packfileName, Filenames[Entry.Index]);
                byte[] FileData = new byte[Entry.Value.DataSize];
                File.Read(FileData, 0, (int)Entry.Value.DataSize);
                System.IO.File.WriteAllBytes(outputPath + Filenames[Entry.Index], FileData);

                if (!Header.Condensed)
                {
                    //If you remove the parentheses here you'll break unpacking on terr01_l0.vpp_pc
                    int remainder = (int)(File.BaseStream.Position % 2048);
                    if (remainder > 0)
                    {
                        File.ReadBytes(2048 - remainder); //Alignment Padding
                    }
                }
                Console.WriteLine(" Done!");
            }
        }

        public void Serialize()
        {

        }
    }
}