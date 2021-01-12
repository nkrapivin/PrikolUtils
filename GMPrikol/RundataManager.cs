using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GMPrikol
{
    public static class RundataManager
    {
        public static void LoadRundata(int projectVersion, string outputPath)
        {
            string AppDir = AppDomain.CurrentDomain.BaseDirectory;
            string fname = "rundata" + projectVersion.ToString();
            string RundataPath = Path.Combine(AppDir, fname);

            if (!File.Exists(RundataPath))
            {
                throw new Exception("Unable to find matching rundata file, expected at " + RundataPath);
            }

            byte[] file = File.ReadAllBytes(RundataPath);
            byte[] rawsize = new byte[4];
            Array.Copy(file, rawsize, rawsize.Length);
            int size = BitConverter.ToInt32(file);

            byte[] zlib = new byte[size];
            Array.Copy(file, rawsize.Length, zlib, 0, size);

            byte[] unpacked = ZlibStream.UncompressBuffer(zlib);
            File.WriteAllBytes(outputPath, unpacked);
        }
    }
}
