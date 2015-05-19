using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace S2CNZ
{
	public class S2CNZ : SonicBGScrollSaver.Level
	{
		readonly int[] TempArray_LayerDef = new int[256];
		int[] Horiz_Scroll_Buf;
		short Camera_X_pos;
		BitmapBits levelimg, tmpimg;
		Bitmap bgimg = new Bitmap(1, 1);
		Color[] CyclingPal_CNZ1, CyclingPal_CNZ3, CyclingPal_CNZ4;
		short PalCycle_Timer, PalCycle_Frame, PalCycle_Frame_CNZ;
		int Width, Height;
		byte framecounter;

		static readonly byte[] byte_D156 = { 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0, 0xF0 };

		static readonly byte[] SwScrl_RippleData = {
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
			LevelData.BmpPal.Entries[0] = LevelData.Palette[0][2, 0].RGBColor;
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			tmpimg = new BitmapBits(Math.Min(levelimg.Width, width), height);
			CyclingPal_CNZ1 = SonLVLColor.Load("CNZ Cycle 1.bin", EngineVersion.S2).Select(a => a.RGBColor).ToArray();
			CyclingPal_CNZ3 = SonLVLColor.Load("CNZ Cycle 3.bin", EngineVersion.S2).Select(a => a.RGBColor).ToArray();
			CyclingPal_CNZ4 = SonLVLColor.Load("CNZ Cycle 4.bin", EngineVersion.S2).Select(a => a.RGBColor).ToArray();
			Horiz_Scroll_Buf = new int[levelimg.Height];
			Camera_X_pos = 0;
			PalCycle_Timer = 0;
			PalCycle_Frame = 0;
			PalCycle_Frame_CNZ = 0;
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
				Camera_X_pos += Camera_X_pos_diff;
				BitmapBits bmp = new BitmapBits(levelimg);
				short d2 = Camera_X_pos;
				int a1 = 0;
				BWL d0 = d2;
				d0.sw >>= 3;
				d0.sw -= d2;
				d0.ExtendL();
				d0.l <<= 13;
				BWL d3 = d2;
				for (int i = 0; i < 7; i++)
				{
					TempArray_LayerDef[a1++] = d3.sw;
					d3.Swap();
					d3.l += d0.l;
					d3.Swap();
				}
				d0.sw = d2;
				d0.sw >>= 3;
				TempArray_LayerDef[a1 + 2] = d0.sw;
				d0.sw >>= 1;
				TempArray_LayerDef[a1++] = d0.sw;
				TempArray_LayerDef[a1++] = d0.sw;
				int a3 = 0;
				int a2 = 0;
				a1 = 0;
				short d1 = byte_D156[a3++];
				d0.sw = (short)TempArray_LayerDef[a2++];
				while (a1 < Horiz_Scroll_Buf.Length)
				{
					Horiz_Scroll_Buf[a1++] = d0.sw;
					--d1;
					while (d1 == 0)
					{
						d0.sw = (short)TempArray_LayerDef[a2++];
						d1 = byte_D156[a3++];
						if (d1 == 0)
						{
							d3.sw = d0.sw;
							d0.b = framecounter++;
							d0.w >>= 3;
							d0.sw = (short)-d0.sw;
							d0.sw &= 0x1F;
							int a4 = d0.sw;
							for (int i = 0; i < 0x10; i++)
								Horiz_Scroll_Buf[a1++] = SwScrl_RippleData[a4++] + d3.sw;
						}
					}
				}
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
				int frame = ++PalCycle_Frame % 3;
				LevelData.BmpPal.Entries[0x25] = CyclingPal_CNZ1[frame];
				LevelData.BmpPal.Entries[0x26] = CyclingPal_CNZ1[frame + 3];
				LevelData.BmpPal.Entries[0x27] = CyclingPal_CNZ1[frame + 6];
				LevelData.BmpPal.Entries[0x2B] = CyclingPal_CNZ1[frame + 9];
				LevelData.BmpPal.Entries[0x2C] = CyclingPal_CNZ1[frame + 12];
				LevelData.BmpPal.Entries[0x2D] = CyclingPal_CNZ1[frame + 15];
				LevelData.BmpPal.Entries[0x32] = CyclingPal_CNZ3[frame];
				LevelData.BmpPal.Entries[0x33] = CyclingPal_CNZ3[frame + 3];
				LevelData.BmpPal.Entries[0x34] = CyclingPal_CNZ3[frame + 6];
				frame = ++PalCycle_Frame_CNZ % 18;
				LevelData.BmpPal.Entries[0x3B] = CyclingPal_CNZ4[frame + 2];
				LevelData.BmpPal.Entries[0x3A] = CyclingPal_CNZ4[frame + 1];
				LevelData.BmpPal.Entries[0x39] = CyclingPal_CNZ4[frame];
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
		[System.ComponentModel.DefaultValue("CasinoNight")]
		[IniName("music")]
		public string Music { get; set; }
	}
}
