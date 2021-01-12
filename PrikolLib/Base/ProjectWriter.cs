using Ionic.Zlib;
using PrikolLib.Misc;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PrikolLib.Base
{
    public class ProjectWriter : IDisposable
    {
        private bool UseUTF8;
        private Stream MyStream;

        public ProjectWriter(Stream output, bool use_utf8 = true)
        {
            UseUTF8 = use_utf8;
            MyStream = output;
        }

        public void Write(byte val) => MyStream.WriteByte(val);

        public void Write(byte[] arr) => MyStream.Write(arr);

        public void Write(int val)
        {
            byte[] rawInt = BitConverter.GetBytes(val);
            MyStream.Write(rawInt);
        }

        public void Write(uint val)
        {
            byte[] rawUInt = BitConverter.GetBytes(val);
            MyStream.Write(rawUInt);
        }

        public void Write(double val)
        {
            byte[] rawDouble = BitConverter.GetBytes(val);
            MyStream.Write(rawDouble);
        }

        public byte[] GetZlib()
        {
            byte[] ret = ZlibStream.CompressBuffer(((MemoryStream)MyStream).ToArray());
            return ret;
        }

        public void WriteZlibChunk(ProjectWriter w_in)
        {
            var data = w_in.GetZlib();
            Write(data.Length);
            Write(data);
        }

        public void Write(Guid value) => Write(value.ToByteArray());
        public void Write(DateTime value) => Write(value.Subtract(new DateTime(1899, 12, 30)).TotalDays);
        public void Write(Color value) => Write(value.R | value.G << 8 | value.B << 16);
        public void Write(Point value)
        {
            Write(value.X);
            Write(value.Y);
        }

        public void Write(Size value)
        {
            Write(value.Width);
            Write(value.Height);
        }

        public void Write(Rectangle value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Width);
            Write(value.Height);
        }

        public void Write(bool value) => Write(value ? 1 : 0); // ah, the dreaded bool longints...

        public void Write(string value)
        {
            byte[] data;
            if (UseUTF8) data = Encoding.UTF8.GetBytes(value);
            else data = Encoding.GetEncoding(866).GetBytes(value);
            Write(data.Length);
            Write(data);
        }

        public void Write(Image img)
        {
            if (img == null) Write(false);
            else
            {
                var stream = new MemoryStream();
                img.Save(stream, ImageFormat.Bmp);
                var data = ZlibStream.CompressBuffer(stream.ToArray());
                Write(true);
                Write(data.Length);
                Write(data);
                stream.Dispose();
            }
        }

        public void Write(PathPoint value)
        {
            Write(value.X);
            Write(value.Y);
            Write(value.Speed);
        }

        public void Write(Image img, bool _is_bgra)
        {
            var bitmap = new Bitmap(img);
            var bmdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            var x = img.Height;
            var size = bmdata.Stride * x;
            byte[] data = new byte[size];
            Marshal.Copy(bmdata.Scan0, data, 0, size);
            Write(size);
            Write(data);
            bitmap.UnlockBits(bmdata);
            bitmap.Dispose();
        }

        public void Write(Icon ico)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ico.Save(stream);
                Write(Convert.ToInt32(stream.Length));
                Write(stream.ToArray());
            }
        }

        public void Write(Version ver)
        {
            Write(ver.Major);
            Write(ver.Minor);
            Write(ver.Build);
            Write(ver.Revision);
        }

        public void Dispose()
        {
            MyStream.Dispose();
        }
    }
}
