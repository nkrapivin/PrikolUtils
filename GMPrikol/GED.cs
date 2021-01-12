using PrikolLib.Base;
using System;
using System.Collections.Generic;

namespace GMPrikol
{
    public class GED
    {
        public int Version;
        public bool Editable;
        public string Name;
        public string Folder;
        public string ExtVersion;
        public string Author;
        public string Date;
        public string License;
        public string Description;
        public string HelpFile;
        public bool Hidden;
        public List<string> Uses;
        public List<GEDFile> Files;

        public void Load(ProjectReader reader)
        {
            Version = reader.ReadInt32();
            if (Version != 700)
            {
                throw new Exception("Invalid GED version, got " + Version);
            }

            Editable = reader.ReadBoolean();

            Name = reader.ReadString();
            Folder = reader.ReadString();
            ExtVersion = reader.ReadString();
            Author = reader.ReadString();
            Date = reader.ReadString();
            License = reader.ReadString();
            Description = reader.ReadString();
            HelpFile = reader.ReadString();
            Hidden = reader.ReadBoolean();

            int cnt = reader.ReadInt32();
            Uses = new List<string>(cnt);
            for (int i = 0; i < cnt; i++)
            {
                Uses.Add(reader.ReadString());
            }

            cnt = reader.ReadInt32();
            Files = new List<GEDFile>(cnt);
            for (int i = 0; i < cnt; i++)
            {
                var ff = new GEDFile();
                ff.Load(reader);
                Files.Add(ff);
            }
        }
    }
}
