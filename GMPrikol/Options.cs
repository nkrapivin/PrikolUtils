using PrikolLib.Assets;
using PrikolLib.Base;
using System;
using System.Drawing;
using System.IO;

namespace GMPrikol
{
    public static class Options
    {
        public static void WriteOptions(ProjectWriter main_writer, GMOptions go)
        {
            main_writer.Write(go.FormatVersion);
            var stream = new MemoryStream();
            var writer = new ProjectWriter(stream);
            writer.Write(go.StartInFullscreen);
            writer.Write(go.InterpolatePixels);
            writer.Write(go.DontDrawBorder);
            writer.Write(go.DisplayCursor);
            writer.Write(go.Scaling);
            writer.Write(go.AllowWindowResize);
            writer.Write(go.AlwaysOnTop);
            writer.Write(go.OutsideRoom);
            writer.Write(go.SetResolution);
            writer.Write((int)go.ColorDepth);
            writer.Write((int)go.ScreenResolution);
            writer.Write((int)go.Frequency);
            writer.Write(go.Borderless);
            uint vsync = go.VSync ? 1U : 0U;
            if (go.SoftwareVertex) vsync |= 0x80000000;
            writer.Write(vsync);
            writer.Write(go.DisableScreensavers);
            writer.Write(go.LetF4Fullscreen);
            writer.Write(go.LetF1GameInfo);
            writer.Write(go.LetESCEndGame);
            writer.Write(go.LetF5F6SaveLoad);
            writer.Write(go.LetF9Screenshot);
            writer.Write(go.TreatCloseAsESC);
            writer.Write((int)go.Priority);
            writer.Write(go.FreezeWhenFocusLost);
            writer.Write((int)go.LoadingBarMode);
            string AppDir = AppDomain.CurrentDomain.BaseDirectory;
            if (go.LoadingBarMode == GMOptions.ProgBars.BAR_CUSTOM)
            {
                writer.Write(go.BackLoadingBar);
                writer.Write(go.FrontLoadingBar);
            }
            else if (go.LoadingBarMode == GMOptions.ProgBars.BAR_DEFAULT)
            {
                string back = Path.Combine(AppDir, "back.png");
                if (File.Exists(back))
                {
                    Image defBack = Image.FromFile(back);
                    writer.Write(defBack);
                    defBack.Dispose();
                }

                string front = Path.Combine(AppDir, "front.png");
                if (File.Exists(front))
                {
                    Image defFront = Image.FromFile(front);
                    writer.Write(defFront);
                    defFront.Dispose();
                }
            }

            if (go.ShowCustomLoadImage && go.LoadingImage != null)
            {
                writer.Write(go.LoadingImage);
            }
            else
            {
                string load = Path.Combine(AppDir, "load.png");
                if (File.Exists(load))
                {
                    Image defLoad = Image.FromFile(load);
                    writer.Write(defLoad);
                    defLoad.Dispose();
                }
                else writer.Write(false);
            }

            writer.Write(go.LoadimgImagePartTransparent);
            writer.Write(go.LoadImageAlpha);
            writer.Write(go.ScaleProgressBar);
            writer.Write(go.DisplayErrors);
            writer.Write(go.WriteToLog);
            writer.Write(go.AbortOnAllErrors);
            writer.Write(go.TreatUninitAsZero);
            main_writer.WriteZlibChunk(writer);
            stream.Dispose();
            writer.Dispose();
        }
    }
}
