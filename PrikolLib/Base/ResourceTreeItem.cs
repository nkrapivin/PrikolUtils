﻿using PrikolLib.Assets;
using System;
using System.Collections.Generic;

namespace PrikolLib.Base
{
    public class ResourceTreeItem
    {
        // WARNING this implementation of a resource tree is pretty horrible.
        // but it's enough to make it work in WinForms :p

        public ResourceStatus Status;
        public Type ResType;
        public int Index;
        public string Name;
        public List<ResourceTreeItem> Resources;

        public enum ResourceStatus
        {
            UNKNOWN,
            PRIMARY,
            GROUP,
            SECONDARY
        }

        public static Type[] RESOURCE_KIND =
        {
            null, typeof(GMObject), typeof(GMSprite), typeof(GMSound),
            typeof(GMRoom),null,typeof(GMBackground),typeof(GMScript),typeof(GMPath),typeof(GMFont),typeof(GMGameInformation),
            typeof(GMOptions),typeof(GMTimeline), typeof(string) /* extension pkg names */, typeof(GMShader) // gmk 820 only.
        };

        public void Save(ProjectWriter writer)
        {
            writer.Write((int)Status);
            writer.Write(Array.IndexOf(RESOURCE_KIND, ResType));
            writer.Write(Index);
            writer.Write(Name);
            writer.Write(Resources.Count);
            for (int i = 0; i < Resources.Count; i++)
            {
                Resources[i].Save(writer);
            }
        }

        public ResourceTreeItem(ProjectReader reader)
        {
            Status = (ResourceStatus)reader.ReadInt32();
            ResType = RESOURCE_KIND[reader.ReadInt32()];
            Index = reader.ReadInt32();
            Name = reader.ReadString();
            int content = reader.ReadInt32();
            Resources = new List<ResourceTreeItem>(content);
            for (int i = 0; i < content; i++)
            {
                Resources.Add(new ResourceTreeItem(reader));
            }
        }
    }
}
