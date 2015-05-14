using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace S2HPZ
{
	public class S2HPZ : SonicBGScrollSaver.Level
	{
		readonly int[] TempArray_LayerDef = new int[256];
		int[] Horiz_Scroll_Buf;
		BWL Camera_BG_X_pos, Camera_BG_Y_pos = new BWL(0, 0x40);
		ushort Camera_X_pos;
		BitmapBits levelimg;
		Bitmap bgimg = new Bitmap(1, 1);
		Color[] CyclingPal_HPZWater;
		short PalCycle_Timer, PalCycle_Frame;
		int Width, Height;

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			CyclingPal_HPZWater = SonLVLColor.Load("HPZ Water Cycle.bin", EngineVersion.S2).Select(a => a.RGBColor).ToArray();
			Horiz_Scroll_Buf = new int[Math.Min(height, levelimg.Height)];
			Camera_BG_Y_pos.hsw = -0x40;
			if (height < levelimg.Height)
				Camera_BG_Y_pos.hsw += (short)((levelimg.Height / 2) - (height / 2));
			Camera_BG_X_pos = 0;
			Camera_X_pos = 0;
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
				Camera_X_pos = (ushort)(Camera_X_pos + Camera_X_pos_diff);
				Camera_BG_Y_pos.sw += Camera_Y_pos_diff;
				BitmapBits bmp = new BitmapBits(levelimg);
				bmp.ScrollVertical(Camera_BG_Y_pos.hsw);
				if (Height < bmp.Height)
					bmp = bmp.GetSection(0, 0, bmp.Width, Height);
				Camera_BG_X_pos.sl += Camera_X_pos_diff << 14;
				int a1 = 0;
				BWL d2 = (short)-Camera_X_pos;
				BWL d0 = d2;
				d0.sw >>= 1;
				for (int i = 0; i < 8; i++)
					TempArray_LayerDef[a1++] = d0.sw;
				d0.w = d2.w;
				d0.sw >>= 3;
				d0.sw -= d2.sw;
				d0.ExtendL();
				d0.sl <<= 3;
				d0.sw /= 8;
				d0.ExtendL();
				d0.sl <<= 12;
				BWL d3 = d2.w;
				d3.sw >>= 1;
				int a2 = 0x30;
				d3.Swap();
				d3.l += d0.l;
				d3.Swap();
				TempArray_LayerDef[a1++] = d3.sw;
				TempArray_LayerDef[a1++] = d3.sw;
				TempArray_LayerDef[a1++] = d3.sw;
				TempArray_LayerDef[--a2] = d3.sw;
				TempArray_LayerDef[--a2] = d3.sw;
				TempArray_LayerDef[--a2] = d3.sw;
				d3.Swap();
				d3.l += d0.l;
				d3.Swap();
				TempArray_LayerDef[a1++] = d3.sw;
				TempArray_LayerDef[a1++] = d3.sw;
				TempArray_LayerDef[--a2] = d3.sw;
				TempArray_LayerDef[--a2] = d3.sw;
				d3.Swap();
				d3.l += d0.l;
				d3.Swap();
				TempArray_LayerDef[a1++] = d3.sw;
				TempArray_LayerDef[--a2] = d3.sw;
				d3.Swap();
				d3.l += d0.l;
				d3.Swap();
				TempArray_LayerDef[a1++] = d3.sw;
				TempArray_LayerDef[--a2] = d3.sw;
				d0 = -Camera_BG_X_pos.hsw;
				for (int i = 0; i < 0x1A; i++)
					TempArray_LayerDef[a1++] = d0.sw;
				a1 += 7;
				d0.w = d2.w;
				d0.w >>= 1;
				for (int i = 0; i < 0x18; i++)
					TempArray_LayerDef[a1++] = d0.sw;
				d3 = 0x3F;
				a2 = 0;
				BWL d4 = Camera_BG_Y_pos.hsw;
				d2.w = d4.w;
				d4.w >>= 4;
				d4.w &= d3.w;
				a1 = 0;
				d2.w &= 0xF;
				if (d2.w == 0)
					d2 = 16;
				while (a2 < Horiz_Scroll_Buf.Length)
				{
					d0 = -TempArray_LayerDef[d4.w++] % bmp.Width;
					d4.w &= d3.w;
					for (int i = 0; i < d2.w; i++)
					{
						Horiz_Scroll_Buf[a2++] = d0.sl;
						if (a2 == Horiz_Scroll_Buf.Length)
							break;
					}
					d2 = 16;
				}
				bmp.ScrollHorizontal((int[])Horiz_Scroll_Buf.Clone());
				if (Width < bmp.Width)
					bmp = bmp.GetSection(0, 0, Width, bmp.Height);
				bgimg = bmp.ToBitmap(LevelData.BmpPal);
			}
		}

		public override void UpdatePalette()
		{
			if (--PalCycle_Timer == -1)
			{
				PalCycle_Timer = 4;
				int frame = PalCycle_Frame--;
				if (PalCycle_Frame == -1)
					PalCycle_Frame = 3;
				Array.Copy(CyclingPal_HPZWater, frame, LevelData.BmpPal.Entries, 57, 4);
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
		[System.ComponentModel.DefaultValue("MysticCave2P")]
		[IniName("music")]
		public string Music { get; set; }
	}
}
