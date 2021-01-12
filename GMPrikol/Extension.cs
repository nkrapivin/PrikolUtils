using PrikolLib.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GMPrikol
{
    public static class Extension
    {
        public static void WriteGEX(ProjectWriter writer, string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (ProjectReader pr = new ProjectReader(fs))
            {
                GEX gex = new GEX();
                gex.Load(pr);

                writer.Write(gex.Package.Version);
                writer.Write(gex.Package.Name);
                writer.Write(gex.Package.Folder);
                writer.Write(gex.Package.Files.Count);
                foreach (var file in gex.Package.Files)
                {
                    writer.Write(file.Version);
                    writer.Write(file.FileName);
                    writer.Write((int)file.Kind);
                    writer.Write(file.Initialization);
                    writer.Write(file.Finalization);
                    writer.Write(file.Functions.Count);
                    int id = 0;
                    foreach (var func in file.Functions)
                    {
                        writer.Write(func.Version);
                        writer.Write(func.Name);
                        writer.Write(func.ExternalName);
                        writer.Write((int)func.Convention);
                        writer.Write(id); // ?????
                        writer.Write(func.Argc);
                        for (int i = 0; i < 17; i++)
                        {
                            writer.Write((int)func.ArgTypes[i]);
                        }
                        writer.Write((int)func.Return);
                        id++;
                    }
                }
            }
        }
    }
}
