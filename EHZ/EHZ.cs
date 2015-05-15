using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace EHZ
{
	public class EHZ : SonicBGScrollSaver.Level
	{
		int[] Horiz_Scroll_Buf;
		ushort Camera_X_pos;
		BitmapBits levelimg, tmpimg;
		Bitmap bgimg = new Bitmap(1, 1);
		Color[] CyclingPal_EHZ_ARZ_Water;
		short PalCycle_Timer, PalCycle_Frame;
		int Width, Height;
		byte framecounter;

		byte[] SwScrl_RippleData = {
			1,  2,  1,  3,  1,  2,  2,  1,  2,  3,  1,  2,  1,  2,  0,  0,
			2,  0,  3,  2,  2,  3,  2,  2,  1,  3,  0,  0,  1,  0,  1,  3,
			1,  2,  1,  3,  1,  2,  2,  1,  2,  3,  1,  2,  1,  2,  0,  0,
			2,  0,  3,  2,  2,  3,  2,  2,  1,  3,  0,  0,  1,  0,  1,  3,
			1,  2
		};

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			tmpimg = new BitmapBits(Math.Min(levelimg.Width, width), height);
			tmpimg.Bits.FastFill(0x22);
			CyclingPal_EHZ_ARZ_Water = SonLVLColor.Load("EHZ ARZ Water.bin", EngineVersion.S2).Select(a => a.RGBColor).ToArray();
			Horiz_Scroll_Buf = new int[levelimg.Height];
			Camera_X_pos = 0;
			PalCycle_Timer = 0;
			PalCycle_Frame = 0;
			framecounter = 0;
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
				Camera_X_pos = (ushort)(Camera_X_pos + Camera_X_pos_diff);
				BitmapBits bmp = new BitmapBits(levelimg);
				//Array.Clear(Horiz_Scroll_Buf, 0, 22);
				int bgpos = Camera_X_pos >> 6;
				Horiz_Scroll_Buf.FastFill(bgpos, 22, 58);
				framecounter--;
				int a2 = (framecounter >> 3) & 0x1F;
				for (int i = 80; i < 101; i++)
					Horiz_Scroll_Buf[i] = bgpos + SwScrl_RippleData[a2++];
				//Array.Clear(Horiz_Scroll_Buf, 101, 11);
				bgpos = Camera_X_pos >> 4;
				Horiz_Scroll_Buf.FastFill(bgpos, 112, 16);
				bgpos += bgpos >> 1;
				Horiz_Scroll_Buf.FastFill(bgpos, 128, 16);
				BWL bgpos2 = new BWL(0, (short)(-Camera_X_pos >> 3));
				int scrlamt = -Camera_X_pos >> 1;
				scrlamt -= bgpos2.hsw;
				scrlamt <<= 8;
				scrlamt /= 0x30;
				scrlamt = (short)scrlamt;
				scrlamt <<= 8;
				for (int i = 144; i < 159; i++)
				{
					Horiz_Scroll_Buf[i] = -bgpos2.hsw;
					bgpos2.sl += scrlamt;
				}
				for (int i = 159; i < 177; i += 2)
				{
					Horiz_Scroll_Buf[i] = -bgpos2.hsw;
					Horiz_Scroll_Buf[i + 1] = -bgpos2.hsw;
					bgpos2.sl += scrlamt * 2;
				}
				for (int i = 177; i < 225; i += 3)
				{
					Horiz_Scroll_Buf[i] = -bgpos2.hsw;
					Horiz_Scroll_Buf[i + 1] = -bgpos2.hsw;
					Horiz_Scroll_Buf[i + 2] = -bgpos2.hsw;
					bgpos2.sl += scrlamt * 3;
				}
				for (int i = 225; i < 252; i += 4)
				{
					Horiz_Scroll_Buf[i] = -bgpos2.hsw;
					Horiz_Scroll_Buf[i + 1] = -bgpos2.hsw;
					Horiz_Scroll_Buf[i + 2] = -bgpos2.hsw;
					Horiz_Scroll_Buf[i + 3] = -bgpos2.hsw;
					bgpos2.sl += scrlamt * 4;
				}
				Horiz_Scroll_Buf.FastFill(-bgpos2.hsw, 252, 3);
				bmp.ScrollHorizontal((int[])Horiz_Scroll_Buf.Clone());
				if (Width < bmp.Width)
					bmp = bmp.GetSection(0, 0, Width, bmp.Height);
				tmpimg.DrawBitmapBounded(bmp, 0, tmpimg.Height - bmp.Height);
				bgimg = tmpimg.ToBitmap(LevelData.BmpPal);
			}
		}

		public override void UpdatePalette()
		{
			if (--PalCycle_Timer == -1)
			{
				PalCycle_Timer = 7;
				int frame = (PalCycle_Frame++ & 3) * 4;
				Array.Copy(CyclingPal_EHZ_ARZ_Water, frame, LevelData.BmpPal.Entries, 0x13, 2);
				Array.Copy(CyclingPal_EHZ_ARZ_Water, frame + 2, LevelData.BmpPal.Entries, 0x1E, 2);
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
		[System.ComponentModel.DefaultValue("EmeraldHill")]
		[IniName("music")]
		public string Music { get; set; }
	}
}
