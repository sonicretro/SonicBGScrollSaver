using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SLZ
{
	public class SLZ : SonicBGScrollSaver.Level
	{
		readonly int[] TempArray_LayerDef = new int[256];
		int[] Horiz_Scroll_Buf;
		short Camera_X_pos, Camera_Y_pos;
		BitmapBits levelimg;
		Bitmap bgimg = new Bitmap(1, 1);
		int Width, Height;

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			LevelData.BmpPal.Entries[0] = LevelData.Palette[0][2, 0].RGBColor;
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			levelimg = levelimg.GetSection(0, 0xC0, levelimg.Width, levelimg.Height - 0xC0);
			Horiz_Scroll_Buf = new int[Math.Min(height, levelimg.Height)];
			Camera_Y_pos = (short)((levelimg.Height / 2) - (height / 2));
			Camera_X_pos = 0;
			UpdateScrolling(0, 0);
		}

		public override Bitmap GetBG()
		{
			return bgimg;
		}

		public override void UpdateScrolling(short Camera_X_pos_diff, short Camera_Y_pos_diff)
		{
			lock (bgimg)
			{
				Camera_X_pos += Camera_X_pos_diff;
				BitmapBits bmp = new BitmapBits(levelimg);
				if (Height < bmp.Height)
					bmp = bmp.GetSection(0, Camera_Y_pos, bmp.Width, Height);
				int a1 = 0;
				BWL d2 = Camera_X_pos;
				BWL d0 = d2.sw >> 3;
				d0.sw -= d2.sw;
				d0.ExtendL();
				d0.sl <<= 4;
				d0.sl /= 0x1C;
				d0.ExtendL();
				d0.sl <<= 12;
				BWL d3 = d2.sw;
				for (int i = 0; i < 0x1C; i++)
				{
					TempArray_LayerDef[a1++] = d3.sw;
					d3.Swap();
					d3.l += d0.l;
					d3.Swap();
				}
				d0.w = d2.w;
				d0.sw >>= 3;
				BWL d1 = d0.w;
				d1.sw >>= 1;
				d0.w += d1.w;
				TempArray_LayerDef.FastFill(d0.sw, a1, 5);
				a1 += 5;
				d0.w = d2.w;
				d0.sw >>= 2;
				TempArray_LayerDef.FastFill(d0.sw, a1, 5);
				a1 += 5;
				d0.w = d2.w;
				d0.sw >>= 1;
				TempArray_LayerDef.FastFill(d0.sw, a1, 0x1E);
				a1 += 0x1E;
				int a2 = 0;
				d0.sw = Math.Max(Camera_Y_pos, (short)0);
				d2.w = d0.w;
				d0.w &= 0x1F0;
				d0.sw >>= 4;
				a2 = d0.sw;
				a1 = 0;
				d2.w &= 0xF;
				d2.w = (ushort)(16 - d2.w);
				while (a1 < bmp.Height)
				{
					Horiz_Scroll_Buf.FastFill(TempArray_LayerDef[a2++], a1, d2.w);
					a1 += d2.w;
					d2.w = (ushort)Math.Min(16, bmp.Height - a1);
				}
				bmp.ScrollHorizontal((int[])Horiz_Scroll_Buf.Clone());
				if (Width < bmp.Width)
					bmp = bmp.GetSection(0, 0, Width, bmp.Height);
				if (Height > bmp.Height)
				{
					BitmapBits tmpbmp = new BitmapBits(bmp.Width, Height);
					bmp.Bits.CopyTo(tmpbmp.Bits, tmpbmp.GetPixelIndex(0, -Camera_Y_pos));
					for (int i = -Camera_Y_pos; i >= 0; i -= 0x1B0)
						if (i - 0x1B0 >= 0)
							Array.Copy(bmp.Bits, 0, tmpbmp.Bits, tmpbmp.GetPixelIndex(0, i - 0x1B0), tmpbmp.GetPixelIndex(0, 0x1B0));
						else
							Array.Copy(bmp.Bits, bmp.GetPixelIndex(0, 0x1B0 - i), tmpbmp.Bits, 0, bmp.GetPixelIndex(0, i));
					for (int i = -Camera_Y_pos + bmp.Height; i < Height; i += 256)
						Array.Copy(bmp.Bits, bmp.GetPixelIndex(0, 0x340), tmpbmp.Bits, tmpbmp.GetPixelIndex(0, i), tmpbmp.GetPixelIndex(0, Math.Min(256, Height - i)));
					bmp = tmpbmp;
				}
				bgimg = bmp.ToBitmap(LevelData.BmpPal);
			}
		}

		public override void PlayMusic()
		{
			SonicBGScrollSaver.Music.PlaySong(IniSerializer.Deserialize<MusicInfo>("setup.ini").Music);
		}
	}

	[StructLayout(LayoutKind.Explicit, Size = 4)]
	struct BWL
	{
		[FieldOffset(0)]
		public byte b;
		[FieldOffset(0)]
		public sbyte sb;
		[FieldOffset(1)]
		public byte b1;
		[FieldOffset(1)]
		public sbyte sb1;
		[FieldOffset(2)]
		public byte b2;
		[FieldOffset(2)]
		public sbyte sb2;
		[FieldOffset(3)]
		public byte b3;
		[FieldOffset(3)]
		public sbyte sb3;
		[FieldOffset(0)]
		public ushort w;
		[FieldOffset(0)]
		public short sw;
		[FieldOffset(2)]
		public ushort hw;
		[FieldOffset(2)]
		public short hsw;
		[FieldOffset(0)]
		public uint l;
		[FieldOffset(0)]
		public int sl;

		public BWL(byte b)
			: this()
		{
			this.b = b;
		}

		public BWL(sbyte sb)
			: this()
		{
			this.sb = sb;
		}

		public BWL(byte b0, byte b1, byte b2, byte b3)
			: this()
		{
			b = b0;
			this.b1 = b1;
			this.b2 = b2;
			this.b3 = b3;
		}

		public BWL(sbyte sb0, sbyte sb1, sbyte sb2, sbyte sb3)
			: this()
		{
			sb = sb0;
			this.sb1 = sb1;
			this.sb2 = sb2;
			this.sb3 = sb3;
		}

		public BWL(ushort w)
			: this()
		{
			this.w = w;
		}

		public BWL(short sw)
			: this()
		{
			this.sw = sw;
		}

		public BWL(ushort w, ushort hw)
			: this()
		{
			this.w = w;
			this.hw = hw;
		}

		public BWL(short sw, short hsw)
			: this()
		{
			this.sw = sw;
			this.hsw = hsw;
		}

		public BWL(uint l)
			: this()
		{
			this.l = l;
		}

		public BWL(int sl)
			: this()
		{
			this.sl = sl;
		}

		public void Swap()
		{
			ushort tmp = w;
			w = hw;
			hw = tmp;
		}

		public void ExtendW()
		{
			sw = sb;
		}

		public void ExtendL()
		{
			sl = sw;
		}

		public static implicit operator BWL(byte b)
		{
			return new BWL(b);
		}

		public static implicit operator BWL(sbyte sb)
		{
			return new BWL(sb);
		}

		public static implicit operator BWL(ushort w)
		{
			return new BWL(w);
		}

		public static implicit operator BWL(short sw)
		{
			return new BWL(sw);
		}

		public static implicit operator BWL(uint l)
		{
			return new BWL(l);
		}

		public static implicit operator BWL(int sl)
		{
			return new BWL(sl);
		}
	}

	internal class MusicInfo
	{
		[System.ComponentModel.DefaultValue("StarLight")]
		[IniName("music")]
		public string Music { get; set; }
	}
}
