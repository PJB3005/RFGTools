using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ManagedSquish;

namespace RFGTools.Formats.Peg10
{
    public class Peg10
    {
        public uint Signature;
        public ushort Version;
        public ushort Platform;
        public uint DirectoryBlockSize;
        public uint DataBlockSize;
        public ushort NumberOfBitmaps;
        public ushort Flags;
        public ushort TotalEntries;
        public ushort AlignValue;

        public List<Entry> Entries;

        public List<string> Filenames;

        public void Deserialize(string inputPath, string outputPath)
        {
            var reader = new BinaryReader(new FileStream(inputPath, FileMode.Open));

            //Todo: Double check that signature is valid
            Signature = reader.ReadUInt32();
            Version = reader.ReadUInt16(); //Todo: Check if valid
            Platform = reader.ReadUInt16();
            DirectoryBlockSize = reader.ReadUInt32();
            DataBlockSize = reader.ReadUInt32();
            NumberOfBitmaps = reader.ReadUInt16();
            Flags = reader.ReadUInt16();
            TotalEntries = reader.ReadUInt16();
            AlignValue = reader.ReadUInt16();

            Entries = new List<Entry>();
            for (int i = 0; i < NumberOfBitmaps; i++)
            {
                var newEntry = new Entry();
                newEntry.Deserialize(reader);
                Entries.Add(newEntry);
            }

            Filenames = new List<string>();
            for (int i = 0; i < NumberOfBitmaps; i++)
            {
                var name = new StringBuilder();
                do
                {
                    name.Append(reader.ReadChar());
                }
                while (reader.PeekChar() != 0);
                Filenames.Add(name.ToString());
                reader.ReadByte(); //Move past null byte
            }

            var debug = 2;
            //Find and read from gpeg, do graphics magic with that data to make images
            //Todo: Make sure to make output folder if it doesn't exist
            Directory.CreateDirectory(outputPath);

            string gpuFileExtension = "";
            if (Path.GetExtension(inputPath) == ".cpeg_pc")
            {
                gpuFileExtension = ".gpeg_pc";
            }
            else if (Path.GetExtension(inputPath) == ".cvbm_pc")
            {
                gpuFileExtension = ".gvbm_pc";
            }

            string gpuFileInputPath = Path.GetDirectoryName(inputPath) + "\\" + Path.GetFileNameWithoutExtension(inputPath) + gpuFileExtension;
            string inputFileName = Path.GetFileName(inputPath);

            var dataReader = new BinaryReader(new FileStream(gpuFileInputPath, FileMode.Open));
            foreach (var entry in Entries.Select((Value, Index) => new { Index, Value }))
            {
                dataReader.BaseStream.Seek(entry.Value.Data, SeekOrigin.Begin);

                var gpuData = new byte[entry.Value.FrameSize];

                var decompressedGpuData = ManagedSquish.Squish.DecompressImage(gpuData, entry.Value.Width,
                    entry.Value.Height, SquishFlags.Dxt5); //This crashes for some reason. WIP

                Directory.CreateDirectory(outputPath + "\\" + inputFileName + "\\");
                Directory.CreateDirectory(outputPath + "\\" + inputFileName + "_decompressed\\");
                File.WriteAllBytes(outputPath + "\\" + inputFileName + "\\" + Filenames[entry.Index], gpuData);
                File.WriteAllBytes(outputPath + "\\" + inputFileName + "_decompressed\\" + Filenames[entry.Index], decompressedGpuData);
            }
        }
    }
}