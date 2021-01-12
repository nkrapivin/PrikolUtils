using PrikolLib.Assets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace GMPrikol
{
    public static class CollisionMaskGen
    {
        public static void DumpIntArray(int[] arr)
        {
			byte[] barr = new byte[arr.Length * 4];
			long offs = 0;
			foreach (int iii in arr)
            {
				byte[] raw = BitConverter.GetBytes(iii);
				barr[offs] = raw[0];
				barr[offs + 1] = raw[1];
				barr[offs + 2] = raw[2];
				barr[offs + 3] = raw[3];
				offs += 4;
            }

			//File.WriteAllBytes("maskdat.dat", barr);
        }

        public static int[] Generate(Image frame, GMSprite sprite)
        {
			int[] maskArray = new int[frame.Width * frame.Height];
			int left = sprite.BBox.X;
			int right = sprite.BBox.Y;
			int bottom = sprite.BBox.Width;
			int top = sprite.BBox.Height;

			int mLeft = Math.Max(0, Math.Min(frame.Height - 1, top));
			int mBottom = Math.Max(0, Math.Min(frame.Height - 1, bottom));
			int mTop = Math.Max(0, Math.Min(frame.Width - 1, left));
			int mRight = Math.Max(0, Math.Min(frame.Width - 1, right));

			if (mLeft != mBottom && mTop != mRight)
            {
				switch (sprite.MaskMode)
                {
					case GMSprite.SpriteMaskMode.RECTANGLE:
                        {
							for (int y = mLeft; y <= mBottom; y++)
							{
								for (int x = mTop; x <= mRight; x++)
								{
									//maskArray[y * frame.Width + x] |= 1 << 7 - (x & 7);
									maskArray[y * frame.Width + x]++;
								}
							}

							break;
						}

					case GMSprite.SpriteMaskMode.DISK:
                        {
							float fXYHalf = (left + right) / 2;
							float fWHHalf = (bottom + top) / 2;
							float fXYX = fXYHalf - left + 0.5f;
							float fWHH = fWHHalf - top + 0.5f;
							for (int y = mLeft; y <= mBottom; y++)
							{
								for (int x = mTop; x <= mRight; x++)
								{
									if (fXYX > 0f && fWHH > 0f && Math.Pow((x - fXYHalf) / fXYX, 2.0) + Math.Pow((y - fWHHalf) / fWHH, 2.0) <= 1.0)
									{
										//maskArray[y * frame.Width + x] |= 1 << 7 - (x & 7);
										maskArray[y * frame.Width + x]++;
									}
								}
							}

							break;
                        }

					case GMSprite.SpriteMaskMode.DIAMOND:
                        {
							float fXYHalf = (left + right) / 2;
							float fWHHalf = (bottom + top) / 2;
							float fXYX = fXYHalf - left + 0.5f;
							float fWHH = fWHHalf - top + 0.5f;
							for (int y = mLeft; y <= mBottom; y++)
							{
								for (int x = mTop; x <= mRight; x++)
								{
									if (fXYX > 0f && fWHH > 0f && Math.Abs((x - fXYHalf) / fXYX) + Math.Abs((y - fWHHalf) / fWHH) <= 1f)
									{
										//maskArray[y * frame.Width + x] |= 1 << 7 - (x & 7);
										maskArray[y * frame.Width + x]++;
									}
								}
							}

							break;
                        }

					case GMSprite.SpriteMaskMode.PRECISE:
                        {
							for (int y = mLeft; y <= mBottom; y++)
                            {
								for (int x = mTop; x <= mRight; x++)
                                {
									if (((Bitmap)frame).GetPixel(x, y).A > sprite.AlphaTolerance)
                                    {
										//maskArray[y * frame.Width + x] |= 1 << 7 - (x & 7);
										maskArray[y * frame.Width + x]++;
									}
                                }
                            }

							break;
                        }
                }
            }

			//DumpIntArray(maskArray);

			return maskArray;
		}
    }
}
