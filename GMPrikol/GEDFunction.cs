using PrikolLib.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace GMPrikol
{
    public class GEDFunction
    {
        public enum GEDFunctionConv : int
        {
            Unknown,
            GML = 2,
            Stdcall = 11,
            Cdecl = 12
        }

        public enum GEDFunctionArgType : int
        {
            UnknownReal, // ????????????????
            String,
            Real
        }

        public int Version;
        public string Name;
        public string ExternalName;
        public GEDFunctionConv Convention;
        public string Help;
        public bool Hidden;
        public int Argc;
        public List<GEDFunctionArgType> ArgTypes;
        public GEDFunctionArgType Return;

        public void Load(ProjectReader reader)
        {
            Version = reader.ReadInt32();
            if (Version != 700)
            {
                throw new Exception("Invalid function version, got " + Version);
            }

            Name = reader.ReadString();
            ExternalName = reader.ReadString();
            Convention = (GEDFunctionConv)reader.ReadInt32();
            Help = reader.ReadString();
            Hidden = reader.ReadBoolean();
            Argc = reader.ReadInt32();
            ArgTypes = new List<GEDFunctionArgType>(17);
            for (int i = 0; i < 17; i++)
            {
                GEDFunctionArgType tt = (GEDFunctionArgType)reader.ReadInt32();
                ArgTypes.Add(tt);
            }
            Return = (GEDFunctionArgType)reader.ReadInt32();
        }
    }
}
