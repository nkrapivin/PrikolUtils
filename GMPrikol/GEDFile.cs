using PrikolLib.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace GMPrikol
{
    public class GEDFile
    {
        public enum GEDFileKind : int
        {
            Unknown,
            DLL,
            GML,
            ActionLib,
            Other
        }

        public int Version;
        public string FileName;
        public string OriginalName;
        public GEDFileKind Kind;
        public string Initialization;
        public string Finalization;
        public List<GEDFunction> Functions;
        public List<GEDConstant> Constants;

        public void Load(ProjectReader reader)
        {
            Version = reader.ReadInt32();
            if (Version != 700)
            {
                throw new Exception("Invalid extension file version, got " + Version);
            }

            FileName = reader.ReadString();
            OriginalName = reader.ReadString();
            Kind = (GEDFileKind)reader.ReadInt32();
            Initialization = reader.ReadString();
            Finalization = reader.ReadString();

            int cnt = reader.ReadInt32();
            Functions = new List<GEDFunction>(cnt);
            for (int i = 0; i < cnt; i++)
            {
                var gedf = new GEDFunction();
                gedf.Load(reader);
                Functions.Add(gedf);
            }

            cnt = reader.ReadInt32();
            Constants = new List<GEDConstant>(cnt);
            for (int i = 0; i < cnt; i++)
            {
                GEDConstant cc = new GEDConstant();
                cc.Load(reader);
                Constants.Add(cc);
            }
        }
    }
}
