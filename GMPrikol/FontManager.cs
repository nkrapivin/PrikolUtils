using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace GMPrikol
{
    public static class FontManager
    {
        public const string BMFCTemplate = @"# AngelCode Bitmap Font Generator configuration file
fileVersion=1

# font settings
fontName={FONT_NAME}
fontFile=
charSet={FONT_CHARSET}
fontSize={FONT_SIZE}
aa=1
scaleH=100
useSmoothing=1
isBold={IS_BOLD}
isItalic={IS_ITALIC}
useUnicode=0
disableBoxChars=1
outputInvalidCharGlyph=0
dontIncludeKerningPairs=0
useHinting=1
renderFromOutline=0
useClearType={CLEAR_TYPE}
autoFitNumPages=0
autoFitFontSizeMin=0
autoFitFontSizeMax=0

# character alignment
paddingDown=0
paddingUp=0
paddingRight=0
paddingLeft=0
spacingHoriz=1
spacingVert=1
useFixedHeight=1
forceZero=1
widthPaddingFactor=0.00

# output file
outWidth=512
outHeight=512
outBitDepth=32
fontDescFormat=1
fourChnlPacked=0
textureFormat=png
textureCompression=0
alphaChnl=0
redChnl=4
greenChnl=4
blueChnl=4
invA=0
invR=0
invG=0
invB=0

# outline
outlineThickness=0

# selected chars
chars={CHARACTER_RANGE}

# imported icon images
";

        public class FontResult
        {
            public int Width;
            public int Height;
            public byte[] Texture; // alpha channel only.
            public int[][] Glyphs; // [char][prop]
        }

        public static FontResult FindFont(string fontName, int fontSize, bool isBold, bool isItalic, int charset, int first, int last, int AA)
        {
            // TODO: replace BMFont by AngelCode with a more cross-platform approach?
            try
            {
                string conf = BMFCTemplate
                    .Replace("{FONT_NAME}", fontName)
                    .Replace("{FONT_CHARSET}", charset.ToString())
                    .Replace("{FONT_SIZE}", fontSize.ToString())
                    .Replace("{IS_BOLD}", isBold ? "1" : "0")
                    .Replace("{IS_ITALIC}", isItalic ? "1" : "0")
                    .Replace("{CHARACTER_RANGE}", first.ToString() + "-" + last.ToString())

                    // this is wrong, but kinda works.
                    .Replace("{CLEAR_TYPE}", (AA > 0) ? "1" : "0");

                string AppDir = AppDomain.CurrentDomain.BaseDirectory;
                string confPath = Path.Combine(AppDir, "temp.bmfc");
                string texPath = Path.Combine(AppDir, "temp_0.png");
                string xmlPath = Path.Combine(AppDir, "temp.fnt");
                string bmfgPath = Path.Combine(AppDir, "bmfont64.exe");
                string bmfArgs = "-c temp.bmfc -o temp.fnt";
                Process bmf = new Process();
                bmf.StartInfo.FileName = bmfgPath;
                bmf.StartInfo.WorkingDirectory = AppDir;
                bmf.StartInfo.Arguments = bmfArgs;
                File.WriteAllText(confPath, conf);
                bmf.Start();
                bmf.WaitForExit();
                bmf.Dispose();
                File.Delete(confPath);

                // I do the dummy thing so we can delete the file right away.
                Image dummy = Image.FromFile(texPath);
                Bitmap fntTex = new Bitmap(dummy);
                dummy.Dispose();
                File.Delete(texPath);

                byte[] fontalpha = new byte[fntTex.Width * fntTex.Height];
                int ww = fntTex.Width;
                int hh = fntTex.Height;

                for (int yy = 0; yy < hh; yy++)
                    for (int xx = 0; xx < ww; xx++)
                    {
                        // GetPixel is slow, I know.
                        byte alpha = fntTex.GetPixel(xx, yy).A;
                        fontalpha[xx + (yy * ww)] = alpha;
                    }

                fntTex.Dispose();

                XDocument fntXml = XDocument.Load(xmlPath);
                File.Delete(xmlPath);

                FontResult ret = new FontResult();
                ret.Texture = fontalpha;
                ret.Width = ww;
                ret.Height = hh;
                ret.Glyphs = new int[256][];
                for (int i = 0; i < ret.Glyphs.Length; i++)
                    ret.Glyphs[i] = new int[6];

                foreach (var glyph in fntXml.Root.Element("chars").Elements("char"))
                {
                    int id = int.Parse(glyph.Attribute("id").Value);

                    int x = int.Parse(glyph.Attribute("x").Value);
                    int y = int.Parse(glyph.Attribute("y").Value);
                    int width = int.Parse(glyph.Attribute("width").Value);
                    int height = int.Parse(glyph.Attribute("height").Value);
                    int yoffset = int.Parse(glyph.Attribute("yoffset").Value);
                    int xadvance = int.Parse(glyph.Attribute("xadvance").Value);

                    ret.Glyphs[id][0] = x;
                    ret.Glyphs[id][1] = y;
                    ret.Glyphs[id][2] = width;
                    ret.Glyphs[id][3] = height;
                    ret.Glyphs[id][4] = xadvance;
                    ret.Glyphs[id][5] = yoffset;
                }

                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("-- FONT FAIL --");
                Console.WriteLine(e.ToString());
            }

            return null;
        }
    }
}
