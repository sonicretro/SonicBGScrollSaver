using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ARZ
{
	public class ARZ : SonicBGScrollSaver.Level
	{
		readonly int[] TempArray_LayerDef = new int[256];
		int[] Horiz_Scroll_Buf;
		short Camera_X_pos;
		int Camera_Y_pos;
		BWL Camera_BG_X_pos;
		BitmapBits levelimg, tmpimg;
		Bitmap bgimg = new Bitmap(1, 1);
		int Width, Height;

		static readonly byte[] byte_D5CE =
		{
			0xB0, 0x70, 0x30, 0x60, 0x15, 0xC, 0xE, 6,
			0xC, 0x1F, 0x30, 0xC0, 0xF0, 0xF0, 0xF0, 0xF0
		};

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			LevelData.BmpPal.Entries[0] = LevelData.Palette[0][2, 0].RGBColor;
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			tmpimg = new BitmapBits(Math.Min(levelimg.Width, width), height);
			tmpimg.Bits.FastFill(0x3F);
			Horiz_Scroll_Buf = new int[levelimg.Height];
			Camera_X_pos = 0;
			if (levelimg.Height < Height)
				Camera_Y_pos = levelimg.Height - Height;
			else
				Camera_Y_pos = 0;
			Camera_BG_X_pos = 0;
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
				if (levelimg.Height > Height)
					Camera_Y_pos = Math.Min(Math.Max(0, Camera_Y_pos + Camera_Y_pos_diff), levelimg.Height - Height);
				BWL d4 = (int)(Camera_X_pos_diff << 8);
				d4.sl = d4.sw * 0x119;
				Camera_BG_X_pos.l += d4.l;
				int a3 = 3;
				BWL d0 = (int)Camera_X_pos;
				d0.sl <<= 4;
				d0.sl /= 10;
				d0.ExtendL();
				d0.sl <<= 12;
				BWL d1 = d0;
				TempArray_LayerDef[a3++] = d1.hsw;
				d1.l += d1.l;
				d1.l += d0.l;
				for (int i = 0; i < 6; i++)
				{
					TempArray_LayerDef[a3++] = d1.hsw;
					d1.l += d0.l;
				}
				TempArray_LayerDef[a3++] = d1.hsw;
				d0.w = Camera_BG_X_pos.hw;
				TempArray_LayerDef.FastFill(d0.sw, 0, 3);
				TempArray_LayerDef.FastFill(d0.sw, 11, 5);
				a3 = 0;
				int a2 = 0;
				int a1 = 0;
				d1.sw = byte_D5CE[a3++];
				d0.sl = TempArray_LayerDef[a2++];
				while (a1 < Horiz_Scroll_Buf.Length)
				{
					Horiz_Scroll_Buf[a1++] = d0.sl;
					if (--d1.sw == 0)
					{
						d0.sl = TempArray_LayerDef[a2++];
						d1.sw = byte_D5CE[a3++];
					}
				}
				levelimg.ScrollHV(tmpimg, Math.Max(0, -Camera_Y_pos), Math.Max(0, Camera_Y_pos), Horiz_Scroll_Buf);
				bgimg = tmpimg.ToBitmap(LevelData.BmpPal);
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
		[System.ComponentModel.DefaultValue("AquaticRuin")]
		[IniName("music")]
		public string Music { get; set; }
	}
}
