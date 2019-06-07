using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
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
        public void Deserialize(string packfilePath, string outputPath)
        {
            string packfileName = Path.GetFileName(packfilePath);
            var packfileInfo = new FileInfo(packfilePath);
            Console.WriteLine("Extracting " + packfileName + "...");
            if (packfileInfo.Length <= 2048)
            {
                Console.WriteLine("Cancelled extraction of {0}. Packfile is empty!", packfileName);
                return;
            }
            Directory.CreateDirectory(outputPath);
            Console.WriteLine(packfileName + "> Reading header data...");

            var packfile = new BinaryReader(new FileStream(packfilePath, FileMode.Open));
            Header = new Packfile3Header();
            Header.Deserialize(packfile);

            DirectoryEntries = new List<Packfile3DirectoryEntry>();
            for (int i = 0; i < Header.NumberOfFiles; i++)
            {
                var entry = new Packfile3DirectoryEntry();
                entry.Deserialize(packfile);
                DirectoryEntries.Add(entry);
            }
            packfile.ReadBytes(2048 - ((int)packfile.BaseStream.Position % 2048)); //Alignment Padding

            Filenames = new List<string>();
            for (int i = 0; i < Header.NumberOfFiles; i++)
            {
                var name = new StringBuilder();
                do
                {
                    name.Append(packfile.ReadChar());
                }
                while (packfile.PeekChar() != 0);
                Filenames.Add(name.ToString());
                packfile.ReadByte(); //Move past null byte
            }
            packfile.ReadBytes(2048 - ((int)packfile.BaseStream.Position % 2048)); //Alignment Padding

            if(Header.Compressed && Header.Condensed)
            {
                DeserializeCompressedAndCondensed(packfilePath, outputPath, packfile);
            }
            else
            {
                if(Header.Compressed)
                {
                    DeserializeCompressed(packfilePath, outputPath, packfile);
                }
                else
                {
                    DeserializeDefault(packfilePath, outputPath, packfile);
                }
            }
        }

        private void DeserializeCompressedAndCondensed(string packfilePath, string outputPath, BinaryReader packfile)
        {
            string packfileName = Path.GetFileName(packfilePath);
            //Inflate whole data block
            byte[] compressedData = new byte[Header.CompressedDataSize];
            byte[] decompressedData = new byte[Header.DataSize];
            packfile.Read(compressedData, 0, (int)Header.CompressedDataSize);

            int decompressedSizeResult = 0;
            using (MemoryStream memory = new MemoryStream(compressedData))
            {
                using (InflaterInputStream inflater = new InflaterInputStream(memory))
                {
                    decompressedSizeResult = inflater.Read(decompressedData, 0, (int)Header.DataSize);
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

        private void DeserializeCompressed(string packfilePath, string outputPath, BinaryReader packfile)
        {
            string packfileName = Path.GetFileName(packfilePath);
            //Inflate block by block
            foreach (var Entry in DirectoryEntries.Select((Value, Index) => new { Index, Value }))
            {
                Console.Write("{0}> Extracting {1}...", packfileName, Filenames[Entry.Index]);
                byte[] compressedData = new byte[Entry.Value.CompressedDataSize];
                byte[] decompressedData = new byte[Entry.Value.DataSize];
                packfile.Read(compressedData, 0, (int)Entry.Value.CompressedDataSize);

                int decompressedSizeResult = 0;
                using (var memory = new MemoryStream(compressedData))
                {
                    using (InflaterInputStream inflater = new InflaterInputStream(memory))
                    {
                        decompressedSizeResult = inflater.Read(decompressedData, 0, (int)Entry.Value.DataSize);
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
                File.WriteAllBytes(outputPath + Filenames[Entry.Index], decompressedData);

                int remainder = (int)(packfile.BaseStream.Position % 2048);
                if (remainder > 0)
                {
                    packfile.ReadBytes(2048 - remainder); //Alignment Padding
                }
                Console.WriteLine(" Done!");
            }
        }

        private void DeserializeDefault(string packfilePath, string outputPath, BinaryReader packfile)
        {
            string packfileName = Path.GetFileName(packfilePath);
            //Copy data into individual files
            foreach (var Entry in DirectoryEntries.Select((Value, Index) => new { Index, Value }))
            {
                Console.Write("{0}> Extracting {1}...", packfileName, Filenames[Entry.Index]);
                byte[] fileData = new byte[Entry.Value.DataSize];
                packfile.Read(fileData, 0, (int)Entry.Value.DataSize);
                File.WriteAllBytes(outputPath + Filenames[Entry.Index], fileData);

                if (!Header.Condensed)
                {
                    //If you remove the parentheses here you'll break unpacking on terr01_l0.vpp_pc
                    int remainder = (int)(packfile.BaseStream.Position % 2048);
                    if (remainder > 0)
                    {
                        packfile.ReadBytes(2048 - remainder); //Alignment Padding
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