using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PrikolLib;
using PrikolLib.Base;

namespace GMPrikol
{
    public class GEX
    {
        public int Version;
        public int Seed;
        private byte[][] GMKrypt;
        public GED Package;
        public byte[] RawDAT;

        public void Lookup(int seed)
        {
            byte[][] table = new byte[2][];
            table[0] = new byte[256];
            table[1] = new byte[table[0].Length];

            int a = 6 + (seed % 250);
            int b = seed / 250;
            for (int i = 0; i < table[0].Length; i++)
                table[0][i] = Convert.ToByte(i);

            for (int i = 1; i < 10001; i++)
            {
                int j = 1 + ((i * a + b) % 254);
                byte t = table[0][j];

                table[0][j] = table[0][j + 1];
                table[0][j + 1] = t;
            }

            table[1][0] = 0; // optional.
            for (int i = 1; i < table[1].Length; i++)
                table[1][table[0][i]] = Convert.ToByte(i);

            GMKrypt = table;
        }

        public void Load(ProjectReader reader)
        {
            int magic = reader.ReadInt32();
            if (magic != GMProject.GMMagic)
            {
                throw new Exception("Invalid GEX magic, got " + magic);
            }

            Version = reader.ReadInt32();
            if (Version != 701)
            {
                throw new Exception("Invalid GEX version, got " + Version);
            }

            Seed = reader.ReadInt32();
            Lookup(Seed);

            // Decrypt GED/DAT
            var gedStream = new MemoryStream();
            gedStream.WriteByte((byte)reader.ReadByte()); // first byte is never encrypted.

            while (true)
            {
                int bb = reader.ReadByte();
                if (bb < 0) break;

                // non-additive mode.
                gedStream.WriteByte(GMKrypt[1][(byte)bb]);
            }

            gedStream.Seek(0, SeekOrigin.Begin);
            var gedReader = new ProjectReader(gedStream);

            Package = new GED();
            Package.Load(gedReader);

            // TODO: maybe do for (int i = 0; i < includedFilesCount; i++) ????????????????????
            RawDAT = gedReader.ReadCompressedStream();

            gedStream.Dispose();
            gedReader.Dispose();
        }
    }
}
