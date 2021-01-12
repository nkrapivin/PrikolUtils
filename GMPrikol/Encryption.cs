using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GMPrikol
{
    public static class Encryption
    {
        public static byte[] EncryptGM8(MemoryStream stream)
        {
            byte[] raw = stream.ToArray();

            // We have to do passes in reverse!

            // Pass 2: Swap
            for (int i = 0; i < raw.Length; i++)
            {
                int index = Math.Max(0, i - (i % 256));
                byte bb = raw[i];
                raw[i] = raw[index];
                raw[index] = bb;
            }

            // Pass 1: GMKrypt, first byte is not encrypted
            for (int i = 1; i < raw.Length; i++)
            {
                byte cur = raw[i];
                byte prev = raw[i - 1];

                int bint = (cur + prev + i) % 256;
                if (bint < 0) bint += 256;

                raw[i] = (byte)bint;
            }

            return raw;
        }

        public static uint[] InitGM81CRCTable()
        {
            const uint crcThing = 0xEDB88320;
            uint[] table = new uint[256];
            
            for (uint i = 0; i < table.Length; i++)
            {
                uint ii = i;
                for (int j = 0; j < 8; j++)
                {
                    ii = ((ii & 1) == 0) ? (ii >> 1) : ((ii >> 1) ^ crcThing);
                }

                table[i] = ii;
            }

            return table;
        }

        public static void EncryptGM81(MemoryStream stream)
        {
            uint[] crcTable = InitGM81CRCTable();
            stream.Seek(0, SeekOrigin.Begin);

            throw new NotImplementedException("TODO: Implement CRC/XOR thing.");
        }
    }
}
