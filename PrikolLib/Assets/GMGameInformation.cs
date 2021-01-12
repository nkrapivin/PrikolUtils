﻿using PrikolLib.Base;
using System;
using System.Drawing;

namespace PrikolLib.Assets
{
    public class GMGameInformation
    {
        public int Version;
        public Color BackgroundColor;
        public bool SeparateWindow;
        public string Caption;
        public Rectangle Position;
        public bool ShowBorder;
        public bool AllowResize;
        public bool AlwaysOnTop; // Modal
        public bool Freeze; // freeze the game while showing help or not
        public DateTime LastChanged;
        public string Text; // it's an rtf string.

        public void Save(ProjectWriter writer)
        {
            writer.Write(BackgroundColor);
            writer.Write(SeparateWindow);
            writer.Write(Caption);
            writer.Write(Position);
            writer.Write(ShowBorder);
            writer.Write(AllowResize);
            writer.Write(AlwaysOnTop);
            writer.Write(Freeze);
            writer.Write(LastChanged);
            writer.Write(Text);
        }

        public GMGameInformation(ProjectReader reader)
        {
            BackgroundColor = reader.ReadColor();
            SeparateWindow = reader.ReadBoolean();
            Caption = reader.ReadString();
            int _l, _t, _w, _h;
            _l = reader.ReadInt32();
            _t = reader.ReadInt32();
            _w = reader.ReadInt32();
            _h = reader.ReadInt32();
            Position = new Rectangle(_l, _t, _w, _h);
            ShowBorder = reader.ReadBoolean();
            AllowResize = reader.ReadBoolean();
            AlwaysOnTop = reader.ReadBoolean();
            Freeze = reader.ReadBoolean();
            LastChanged = reader.ReadDate();
            Text = reader.ReadString();

            reader.Dispose();
        }
    }
}
