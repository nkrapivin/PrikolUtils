using PrikolLib;
using PrikolLib.Assets;
using PrikolLib.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GMPrikol
{
    public static class AssetDelegates
    {
        public static void Trigger(ProjectWriter writer, GMTrigger self, GMProject _)
        {
            writer.Write(self.Version);
            writer.Write(self.Name);
            writer.Write(self.Condition);
            writer.Write((int)self.Event);
            writer.Write(self.ConstName);
        }

        public static void Constant(ProjectWriter writer, GMConstant self, GMProject _)
        {
            writer.Write(self.Name);
            writer.Write(self.Value);
        }

        public static void Sound(ProjectWriter writer, GMSound self, GMProject _)
        {
            writer.Write(self.Name);
            writer.Write(self.Version);
            writer.Write((int)self.Kind);
            writer.Write(self.FileType);
            writer.Write(self.FileName);
            if (self.Data != null)
            {
                writer.Write(true);
                writer.Write(self.Data.Length);
                writer.Write(self.Data);
            }
            else
            {
                writer.Write(false);
            }

            writer.Write(self.GetEffectsInt());
            writer.Write(self.Volume);
            writer.Write(self.Panning);
            writer.Write(self.Preload);
        }

        public static void Sprite(ProjectWriter writer, GMSprite self, GMProject _)
        {
            writer.Write(self.Name);
            writer.Write(self.Version);
            writer.Write(self.Origin);
            writer.Write(self.Subimages.Count);
            foreach (var subimage in self.Subimages)
            {
                writer.Write(800);
                writer.Write(subimage.Size);
                if (subimage.Width * subimage.Height != 0)
                {
                    writer.Write(subimage, true);
                }
            }

            writer.Write(self.SeparateMasks);
            if (self.Subimages.Count <= 0) return;

            if (self.SeparateMasks)
            {
                foreach (var subimage in self.Subimages)
                {
                    writer.Write(800); // mask data version
                    writer.Write(subimage.Width);
                    writer.Write(subimage.Height);
                    writer.Write(self.BBox);
                    int[] mask = CollisionMaskGen.Generate(subimage, self);
                    foreach (int iii in mask)
                        writer.Write(iii);
                }
            }
            else
            {
                writer.Write(800); // mask data version
                writer.Write(self.Subimages[0].Width);
                writer.Write(self.Subimages[0].Height);
                writer.Write(self.BBox);
                int[] mask = CollisionMaskGen.Generate(self.Subimages[0], self);
                foreach (int iii in mask)
                    writer.Write(iii);
            }
        }

        public static void Background(ProjectWriter writer, GMBackground self, GMProject _)
        {
            writer.Write(self.Name);
            writer.Write(self.Version);

            writer.Write(800);
            writer.Write(self.Background.Size);
            if (self.Background.Width * self.Background.Height != 0)
            {
                writer.Write(self.Background, true);
            }
            else
            {
                writer.Write(false);
            }
        }

        public static void Path(ProjectWriter writer, GMPath self, GMProject _)
        {
            writer.Write(self.Name);
            writer.Write(self.Version);
            writer.Write(self.Smooth);
            writer.Write(self.Closed);
            writer.Write(self.Precision);
            writer.Write(self.Points.Count);
            foreach (var point in self.Points)
            {
                writer.Write(point);
            }
        }

        public static void Script(ProjectWriter writer, GMScript self, GMProject _)
        {
            writer.Write(self.Name);
            writer.Write(self.Version);

            writer.Write(self.Code);
        }

        public static void Font(ProjectWriter writer, GMFont self, GMProject _)
        {
            writer.Write(self.Name);
            writer.Write(self.Version);

            writer.Write(self.FontName);
            writer.Write(self.Size);
            writer.Write(self.Bold);
            writer.Write(self.Italic);
            writer.Write(self.RangeMinInt);
            writer.Write(self.RangeMax);

            // generate le font texture.
            FontManager.FontResult fnt = FontManager.FindFont(self.FontName, self.Size, self.Bold, self.Italic, 0, self.RangeMin, self.RangeMax, self.AA);

            // glyph table. (there are always 255 of them)
            for (int chr = 0; chr < 256; chr++)
            {
                // write dummy glyph if out of range.
                if (chr < self.RangeMin || chr > self.RangeMax)
                {
                    for (int a = 0; a < 6; a++) writer.Write(0); // x,y,w,h,shift,offset.
                    continue;
                }

                // write actual glyph.
                for (int a = 0; a < 6; a++) writer.Write(fnt.Glyphs[chr][a]);
            }

            // write the generated font texture as an Alpha-only bitmap.
            writer.Write(fnt.Width);
            writer.Write(fnt.Height);
            writer.Write(fnt.Texture.Length);
            writer.Write(fnt.Texture);
        }

        public static void Timeline(ProjectWriter writer, GMTimeline self, GMProject proj)
        {
            writer.Write(self.Name);
            writer.Write(self.Version);

            writer.Write(self.Moments.Count);
            foreach (var moment in self.Moments)
            {
                writer.Write(moment.Point);
                moment.Event.Save(writer, proj);
            }
        }

        public static void Object(ProjectWriter writer, GMObject self, GMProject proj)
        {
            writer.Write(self.Name);
            writer.Write(self.Version);

            if (self.Sprite == null)
                writer.Write(-1);
            else
                writer.Write(proj.Sprites.IndexOf(self.Sprite));

            writer.Write(self.Solid);
            writer.Write(self.Visible);
            writer.Write(self.Depth);
            writer.Write(self.Persistent);

            if (self.Parent == null)
                writer.Write(-100);
            else
                writer.Write(proj.Objects.IndexOf(self.Parent));

            if (self.Mask == null)
                writer.Write(-1);
            else
                writer.Write(proj.Sprites.IndexOf(self.Mask));

            writer.Write(self.Events.Count - 1);
            foreach (var ev in self.Events)
            {
                foreach (var inner in ev)
                {
                    writer.Write(inner.Key);
                    inner.Save(writer, proj);
                }
                writer.Write(-1);
            }
        }

        public static void Room(ProjectWriter writer, GMRoom self, GMProject proj)
        {
            writer.Write(self.Name);
            writer.Write(self.Version);

            writer.Write(self.Caption);
            writer.Write(self.Width);
            writer.Write(self.Height);
            writer.Write(self.Speed);
            writer.Write(self.Persistent);
            writer.Write(self.BackgroundColor);
            int val = self.DrawBackgroundColor ? 1 : 0;
            if (!self.ClearBGWithWindowColor) val |= 0b10;
            writer.Write(val);
            writer.Write(self.CreationCode);

            // -- Backgrounds
            writer.Write(self.Backgrounds.Count);
            foreach (var bg in self.Backgrounds)
            {
                writer.Write(bg.Visible);
                writer.Write(bg.IsForeground);
                if (bg.Background == null)
                    writer.Write(-1);
                else
                    writer.Write(proj.Backgrounds.IndexOf(bg.Background));
                writer.Write(bg.Position);
                writer.Write(bg.TileHorizontal);
                writer.Write(bg.TileVertical);
                writer.Write(bg.SpeedHorizontal);
                writer.Write(bg.SpeedVertical);
                writer.Write(bg.Stretch);
            }

            // -- Views
            writer.Write(self.EnableViews);
            writer.Write(self.Views.Count);
            foreach (var view in self.Views)
            {
                writer.Write(view.Visible);
                writer.Write(view.ViewCoords);
                writer.Write(view.PortCoords);
                writer.Write(view.BorderHor);
                writer.Write(view.BorderVert);
                writer.Write(view.HSpeed);
                writer.Write(view.VSpeed);
                if (view.ViewObject == null)
                    writer.Write(-1);
                else
                    writer.Write(proj.Objects.IndexOf(view.ViewObject));
            }

            // -- Room Instances
            writer.Write(self.Instances.Count);
            foreach (var inst in self.Instances)
            {
                if (inst.ID == -1)
                {
                    Console.WriteLine("Corrupted room instance found!");
                    continue;
                }
                writer.Write(inst.Position);
                writer.Write(proj.Objects.IndexOf(inst.Object));
                writer.Write(inst.ID);
                writer.Write(inst.CreationCode);
            }

            // -- Room Tiles
            writer.Write(self.Tiles.Count);
            foreach (var tile in self.Tiles)
            {
                writer.Write(tile.RoomPosition);
                writer.Write(proj.Backgrounds.IndexOf(tile.Background));
                writer.Write(tile.BGCoords);
                writer.Write(tile.Depth);
                writer.Write(tile.ID);
            }

            // -- in GMK 820 Physics data would go here, but PrikolLib does not support 820 GMK files and never will.
        }

        public static void IncludedFile(ProjectWriter writer, GMIncludedFile self, GMProject _)
        {
            writer.Write(self.Version);
            writer.Write(self.FileName);
            writer.Write(self.FilePath);
            writer.Write(self.Original);
            writer.Write(self.FileSize);
            writer.Write(self.StoreInProject);
            if (self.Original && self.StoreInProject)
            {
                writer.Write(self.Data.Length);
                writer.Write(self.Data);
            }
            writer.Write((int)self.ExportKind);
            writer.Write(self.ExportFolder);
            writer.Write(self.Overwrite);
            writer.Write(self.FreeMemory);
            writer.Write(self.RemoveAtGameEnd);
        }
    }
}
