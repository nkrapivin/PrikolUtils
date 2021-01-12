using PrikolLib;
using PrikolLib.Assets;
using PrikolLib.Base;
using System;
using System.Collections.Generic;
using System.IO;

namespace GMPrikol
{
    public static class Asset
    {
        public static void WriteAsset<T>(ProjectWriter writer, List<T> assets, Action<ProjectWriter, T, GMProject> assetDelegate, GMProject proj = null)
        {
            writer.Write(800);
            writer.Write(assets.Count);
            foreach (var self in assets)
            {
                ProjectWriter zlibA = new ProjectWriter(new MemoryStream());

                // null asset
                if (self == null)
                {
                    zlibA.Write(false); // does the asset exist?
                }
                else
                {
                    zlibA.Write(true); // the asset exists
                    // execute the delegate...
                    assetDelegate(zlibA, self, proj);
                }

                writer.WriteZlibChunk(zlibA);
            }
        }

        public static void WriteMaxRoomIDs(ProjectWriter writer, int maxInstId, int maxTileId)
        {
            writer.Write(maxInstId);
            writer.Write(maxTileId);
        }

        public static void WriteConstants(ProjectWriter writer, List<GMConstant> list)
        {
            writer.Write(800);
            writer.Write(list.Count);
            foreach (GMConstant constant in list)
            {
                writer.Write(constant.Name);
                writer.Write(constant.Value);
            }
        }

        public static void WriteExtensions(ProjectWriter writer, List<string> list, string searchDir)
        {
            writer.Write(700);
            writer.Write(list.Count);
            foreach (string name in list)
            {
                // load the extensions here!!!
                string filePath = Path.Combine(searchDir, name + ".gex");
                // {searchDir}\Prikol.gex
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Warning: Extension package {name} does not exist, it will not be written!");
                    continue;
                }

                // if the file does exist, write it!
                Extension.WriteGEX(writer, filePath);
            }
        }

        public static void WriteGameInformation(ProjectWriter main_writer, GMGameInformation self)
        {
            main_writer.Write(self.Version);
            var stream = new MemoryStream();
            var writer = new ProjectWriter(stream);
            writer.Write(self.BackgroundColor);
            writer.Write(self.SeparateWindow);
            writer.Write(self.Caption);
            writer.Write(self.Position);
            writer.Write(self.ShowBorder);
            writer.Write(self.AllowResize);
            writer.Write(self.AlwaysOnTop);
            writer.Write(self.Freeze);
            writer.Write(self.Text);
            main_writer.WriteZlibChunk(writer);
            stream.Dispose();
            writer.Dispose();
        }

        public static void WriteLibCC(ProjectWriter writer, List<string> list)
        {
            writer.Write(500);
            writer.Write(list.Count);
            foreach (string cc in list)
            {
                writer.Write(cc);
            }
        }

        public static void WriteRoomExecOrder(ProjectWriter writer, List<GMRoom> list, GMProject proj)
        {
            writer.Write(700);
            writer.Write(list.Count);
            foreach (GMRoom rm in list)
            {
                writer.Write(proj.Rooms.IndexOf(rm));
            }
        }
    }
}
