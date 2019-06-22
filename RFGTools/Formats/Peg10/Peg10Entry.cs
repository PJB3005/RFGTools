using System.IO;

namespace RFGTools.Formats.Peg10
{
    public class Entry
    {
        public uint Data; //Data size?
        public ushort Width;
        public ushort Height;
        public PixelFormat BitmapFormat;
        public ushort SourceWidth;
        public ushort AnimTilesWidth;
        public ushort AnimTilesHeight;
        public ushort NumFrames;
        public TextureFlags Flags;
        public uint Filename; //name offset?
        public ushort SourceHeight;
        public byte Fps;
        public byte MipLevels;
        public uint FrameSize;
        public uint Next;
        public uint Previous;
        public uint[] Cache; //Unsure of the purpose of this

        public void Deserialize(BinaryReader reader)
        {
            Data = reader.ReadUInt32();
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            BitmapFormat = (PixelFormat)reader.ReadUInt16(); //Todo: Check if valid format
            SourceWidth = reader.ReadUInt16();
            AnimTilesWidth = reader.ReadUInt16();
            AnimTilesHeight = reader.ReadUInt16();
            NumFrames = reader.ReadUInt16();
            Flags = (TextureFlags)reader.ReadUInt16();
            Filename = reader.ReadUInt32();
            SourceHeight = reader.ReadUInt16();
            Fps = reader.ReadByte();
            MipLevels = reader.ReadByte();
            FrameSize = reader.ReadUInt32();
            Next = reader.ReadUInt32();
            Previous = reader.ReadUInt32();
            Cache = new uint[2];
            Cache[0] = reader.ReadUInt32();
            Cache[1] = reader.ReadUInt32();
        }
    }
}