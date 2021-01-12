﻿using PrikolLib.Base;
using System.IO;

namespace PrikolLib.Assets
{
    public class GMConstant
    {
        public string Name;
        public string Value;

        public void Save(ProjectWriter writer)
        {
            writer.Write(Name);
            writer.Write(Value);
        }

        public GMConstant(ProjectReader reader)
        {
            Name = reader.ReadString();
            Value = reader.ReadString();
        }
    }
}
