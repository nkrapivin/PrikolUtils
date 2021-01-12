using PrikolLib.Base;
using System;
using System.IO;

namespace PrikolLib.Assets
{
    public class GMFont
    {
        public int Version;
        public string Name;
        public DateTime LastChanged;
        public string FontName;
        public int Size;
        public bool Bold;
        public bool Italic;
        public int RangeMin;
        public int RangeMax;
        public int GDICharset;
        public int AA;

        public int RangeMinInt => (RangeMin | (GDICharset >> 16) | (AA >> 24));

        public void Save(ProjectWriter writer)
        {
            writer.Write(Name);
            writer.Write(LastChanged);
            writer.Write(Version);
            writer.Write(FontName);
            writer.Write(Size);
            writer.Write(Bold);
            writer.Write(Italic);
            writer.Write(RangeMinInt);
            writer.Write(RangeMax);
        }

        public GMFont(ProjectReader reader)
        {
            Name = reader.ReadString();
            LastChanged = reader.ReadDate();
            Version = reader.ReadInt32();
            if (Version != 800)
            {
                throw new InvalidDataException("This library only supports GM8 files.");
            }

            FontName = reader.ReadString();
            Size = reader.ReadInt32();
            Bold = reader.ReadBoolean();
            Italic = reader.ReadBoolean();
            RangeMin = reader.ReadInt16();
            GDICharset = reader.ReadByte();
            AA = reader.ReadByte();
            RangeMax = reader.ReadInt32();
        }
    }
}
