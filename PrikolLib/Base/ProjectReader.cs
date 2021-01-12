using Ionic.Zlib;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PrikolLib.Base
{
    public class ProjectReader : IDisposable
    {
        private bool UseUTF8;
        private Stream MyStream;

        public ProjectReader(Stream input, bool use_utf8 = true)
        {
            UseUTF8 = use_utf8;
            MyStream = input;
        }

        public long GetLength() => MyStream.Length;
        public long GetPos() => MyStream.Position;

        public int ReadByte() => MyStream.ReadByte();

        public int ReadInt32()
        {
            byte[] rawInt = new byte[4];
            MyStream.Read(rawInt);
            return BitConverter.ToInt32(rawInt);
        }

        public short ReadInt16()
        {
            byte[] rawShort = new byte[2];
            MyStream.Read(rawShort);
            return BitConverter.ToInt16(rawShort);
        }

        public uint ReadUInt32()
        {
            byte[] rawUInt = new byte[4];
            MyStream.Read(rawUInt);
            return BitConverter.ToUInt32(rawUInt);
        }

        public double ReadDouble()
        {
            byte[] rawDouble = new byte[8];
            MyStream.Read(rawDouble);
            return BitConverter.ToDouble(rawDouble);
        }

        public byte[] ReadBytes(int count)
        {
            byte[] bytes = new byte[count];
            MyStream.Read(bytes);
            return bytes;
        }

        public bool ReadBoolean()
        {
            return ReadInt32() != 0;
        }

        public string ReadString()
        {
            if (UseUTF8) return Encoding.UTF8.GetString(ReadBytes(ReadInt32())); // some project files don't use UTF8...
            else return Encoding.Default.GetString(ReadBytes(ReadInt32())); // sorta fix russian language in project files...
        }

        public Version ReadVersion()
        {
            int major = ReadInt32();
            int minor = ReadInt32();
            int release = ReadInt32();
            int build = ReadInt32();
            return new Version(major, minor, release, build); // I decided to swap release and build, sorry.
        }

        public Guid ReadGuid()
        {
            const int guid_size = 16;
            return new Guid(ReadBytes(guid_size));
        }

        public DateTime ReadDate()
        {
            return new DateTime(1899, 12, 30).AddDays(ReadDouble()); // okay thank you C# for not making my life a nightmare.;
        }

        public byte[] ReadCompressedStream()
        {
            int size = ReadInt32();
            byte[] data = null;
            if (size >= 0)
            {
                byte[] compressed_data = ReadBytes(size);
                data = ZlibStream.UncompressBuffer(compressed_data);
            }
            return data;
        }

        public Image ReadBGRAImage(int width, int height)
        {
            int size = ReadInt32();
            byte[] data = ReadBytes(size);
            var img = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            var imgdata = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, img.PixelFormat);
            var ptr = imgdata.Scan0;
            Marshal.Copy(data, 0, ptr, size);
            img.UnlockBits(imgdata);

            return img;
        }

        public Icon ReadIcon()
        {
            int size = ReadInt32();
            if (size <= 0) return null; // some bad GM8 decompilers don't include the icon, LGM fails, I don't. :)

            try
            {
                byte[] data = ReadBytes(size);
                var stream = new MemoryStream(data);
                var icon = new Icon(stream);
                stream.Dispose();
                return icon;
            }
            catch
            {
                return null;
            }
        }

        public Image ReadZlibImage()
        {
            byte[] bitmap = ReadCompressedStream();
            Image ret = null;
            using (MemoryStream ms = new MemoryStream(bitmap))
            using (Image img = Image.FromStream(ms))
            {
                ret = new Bitmap(img);
            }

            return ret;
        }

        public ProjectReader MakeReaderZlib()
        {
            return new ProjectReader(new MemoryStream(ReadCompressedStream()));
        }

        public Color ReadColor()
        {
            uint val = ReadUInt32();
            if (val > 0xFFFFFF) // Contains alpha.
            {
                uint r, g, b, a;
                r = (val & 0xFF);
                g = (val >> 8) & 0xFF;
                b = (val >> 16) & 0xFF;
                a = (val >> 24) & 0xFF;
                return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
            }
            else
            {
                uint r, g, b;
                r = (val & 0xFF);
                g = (val >> 8) & 0xFF;
                b = (val >> 16) & 0xFF;
                return Color.FromArgb(0xFF, (int)r, (int)g, (int)b);
            }
        }

        public void Dispose()
        {
            MyStream.Dispose();
        }
    }
}
