using PrikolLib.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace GMPrikol
{
    public class GEDConstant
    {
        public int Version;
        public string Name;
        public string Value;
        public bool Hidden;

        public void Load(ProjectReader reader)
        {
            Version = reader.ReadInt32();
            if (Version != 700)
            {
                throw new Exception("Invalid constant version, got " + Version);
            }

            Name = reader.ReadString();
            Value = reader.ReadString();
            Hidden = reader.ReadBoolean();
        }
    }
}
