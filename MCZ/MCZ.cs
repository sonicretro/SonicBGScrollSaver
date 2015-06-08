using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MCZ
{
	public class MCZ : SonicBGScrollSaver.Level
	{
		readonly int[] TempArray_LayerDef = new int[256];
		int[] Horiz_Scroll_Buf;
		short Camera_X_pos, Camera_Y_pos;
		BitmapBits levelimg, tmpimg;
		Bitmap bgimg = new Bitmap(1, 1);
		int Width, Height;

		static readonly byte[] byte_CE6C =
		{
			0x25, 0x17, 0x12, 7, 7, 2, 2, 0x30, 0xD, 0x13, 0x20, 0x40,
			0x20, 0x13, 0xD, 0x30, 2, 2, 7, 7, 0x20, 0x12, 0x17, 0x25
		};

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			LevelData.BmpPal.Entries[0] = LevelData.Palette[0][2, 0].RGBColor;
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			Horiz_Scroll_Buf = new int[levelimg.Height];
			tmpimg = new BitmapBits(Math.Min(levelimg.Width, width), Math.Min(levelimg.Height, height));
			Camera_Y_pos = 0;
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
				Camera_Y_pos += Camera_Y_pos_diff;
				int a3 = 15;
				BWL d0 = (int)Camera_X_pos;
				d0.sl <<= 4;
				d0.sl /= 10;
				d0.ExtendL();
				d0.sl <<= 12;
				BWL d1 = d0;
				TempArray_LayerDef[a3++] = d1.hsw;
				TempArray_LayerDef[7] = d1.hsw;
				d1.l += d0.l;
				TempArray_LayerDef[a3++] = d1.hsw;
				TempArray_LayerDef[6] = d1.hsw;
				d1.l += d0.l;
				TempArray_LayerDef[a3++] = d1.hsw;
				TempArray_LayerDef[5] = d1.hsw;
				d1.l += d0.l;
				TempArray_LayerDef[a3++] = d1.hsw;
				TempArray_LayerDef[4] = d1.hsw;
				d1.l += d0.l;
				TempArray_LayerDef[a3++] = d1.hsw;
				TempArray_LayerDef[3] = d1.hsw;
				TempArray_LayerDef[8] = d1.hsw;
				TempArray_LayerDef[14] = d1.hsw;
				d1.l += d0.l;
				TempArray_LayerDef[a3++] = d1.hsw;
				d1.l += d0.l;
				TempArray_LayerDef[a3++] = d1.hsw;
				TempArray_LayerDef[2] = d1.hsw;
				TempArray_LayerDef[9] = d1.hsw;
				TempArray_LayerDef[13] = d1.hsw;
				d1.l += d0.l;
				TempArray_LayerDef[a3++] = d1.hsw;
				TempArray_LayerDef[1] = d1.hsw;
				TempArray_LayerDef[10] = d1.hsw;
				TempArray_LayerDef[12] = d1.hsw;
				d1.l += d0.l;
				TempArray_LayerDef[a3++] = d1.hsw;
				TempArray_LayerDef[0] = d1.hsw;
				TempArray_LayerDef[11] = d1.hsw;
				a3 = 0;
				int a2 = 0;
				int a1 = 0;
				d1.sw = byte_CE6C[a3++];
				d0.sl = TempArray_LayerDef[a2++];
				while (a1 < Horiz_Scroll_Buf.Length)
				{
					Horiz_Scroll_Buf[a1++] = d0.sl;
					if (a1 != Horiz_Scroll_Buf.Length && --d1.sw == 0)
					{
						d0.sl = TempArray_LayerDef[a2++];
						d1.sw = byte_CE6C[a3++];
					}
				}
				levelimg.ScrollHV(tmpimg, 0, Camera_Y_pos, Horiz_Scroll_Buf);
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
		[System.ComponentModel.DefaultValue("MysticCave")]
		[IniName("music")]
		public string Music { get; set; }
	}
}
