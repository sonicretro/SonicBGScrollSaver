using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GHZ
{
	public class GHZ : SonicBGScrollSaver.Level
	{
		readonly BWL[] CloudScroll = new BWL[3];
		int[] Horiz_Scroll_Buf;
		int Camera_X_pos;
		BWL Camera_BG2_X_pos, Camera_BG3_X_pos;
		BitmapBits levelimg, tmpimg;
		Bitmap bgimg = new Bitmap(1, 1);
		Color[] Pal_GHZCyc;
		short PalCycle_Timer, PalCycle_Frame;
		int Width, Height;

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			LevelData.BmpPal.Entries[0] = LevelData.Palette[0][2, 0].RGBColor;
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			tmpimg = new BitmapBits(Math.Min(levelimg.Width, width), height);
			tmpimg.Bits.FastFill(0x31);
			Pal_GHZCyc = SonLVLColor.Load("palcycle.bin", EngineVersion.S1).Select(a => a.RGBColor).ToArray();
			Horiz_Scroll_Buf = new int[levelimg.Height];
			Camera_X_pos = 0;
			Array.Clear(CloudScroll, 0, CloudScroll.Length);
			Camera_BG2_X_pos = 0;
			Camera_BG3_X_pos = 0;
			PalCycle_Timer = 0;
			PalCycle_Frame = 0;
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
				BWL d4 = (int)Camera_X_pos_diff << 8;
				d4.sl <<= 5;
				BWL d1 = d4.l;
				d4.sl <<= 1;
				d4.l += d1.l;
				Camera_BG3_X_pos.l += d4.l;
				d4 = (int)Camera_X_pos_diff << 8;
				d4.sl <<= 7;
				Camera_BG2_X_pos.l += d4.l;
				int a1 = 0;
				int a2 = 0;
				CloudScroll[a2++].l += 0x10000;
				CloudScroll[a2++].l += 0xC000;
				CloudScroll[a2++].l += 0x8000;
				BWL d0 = CloudScroll[0].hw;
				d0.w += Camera_BG3_X_pos.hw;
				Horiz_Scroll_Buf.FastFill(d0.sw, 0, 0x20);
				a1 += 0x20;
				d0.w = CloudScroll[1].hw;
				d0.w += Camera_BG3_X_pos.hw;
				Horiz_Scroll_Buf.FastFill(d0.sw, a1, 0x10);
				a1 += 0x10;
				d0.w = CloudScroll[2].hw;
				d0.w += Camera_BG3_X_pos.hw;
				Horiz_Scroll_Buf.FastFill(d0.sw, a1, 0x10);
				a1 += 0x10;
				Horiz_Scroll_Buf.FastFill(Camera_BG3_X_pos.hsw, a1, 0x30);
				a1 += 0x30;
				Horiz_Scroll_Buf.FastFill(Camera_BG2_X_pos.hsw, a1, 0x28);
				a1 += 0x28;
				d0.w = Camera_BG2_X_pos.hw;
				BWL d2 = Camera_X_pos;
				d2.w -= d0.w;
				d2.ExtendL();
				d2.sl <<= 8;
				d2.sl /= 0x68;
				d2.ExtendL();
				d2.sl <<= 8;
				BWL d3 = d0.w;
				while (a1 < levelimg.Height)
				{
					Horiz_Scroll_Buf[a1++] = d3.sw;
					d3.Swap();
					d3.l += d2.l;
					d3.Swap();
				}
				levelimg.ScrollHV(tmpimg, tmpimg.Height - levelimg.Height, 0, Horiz_Scroll_Buf);
				bgimg = tmpimg.ToBitmap(LevelData.BmpPal);
			}
		}

		public override void UpdatePalette()
		{
			if (--PalCycle_Timer == -1)
			{
				PalCycle_Timer = 5;
				int frame = (PalCycle_Frame++ & 3) * 4;
				Array.Copy(Pal_GHZCyc, frame, LevelData.BmpPal.Entries, 0x28, 4);
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
		[System.ComponentModel.DefaultValue("GreenHill")]
		[IniName("music")]
		public string Music { get; set; }
	}
}
