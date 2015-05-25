using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace SBZ1
{
	public class SBZ1 : SonicBGScrollSaver.Level
	{
		readonly int[] TempArray_LayerDef = new int[256];
		int[] Horiz_Scroll_Buf;
		short Camera_X_pos;
		BWL Camera_BG_X_pos, Camera_BG2_X_pos, Camera_BG3_X_pos;
		BitmapBits levelimg, tmpimg;
		Bitmap bgimg = new Bitmap(1, 1);
		int Width, Height;
		int AnimWait1, AnimTime1, AnimFrame1, AnimWait2, AnimTime2, AnimFrame2;
		BitmapBits[] smokeimgs;
		List<Point> Anim1Locs, Anim2Locs;
		short[] pal_durations = new short[4] { 0xE, 0xB, 0x7, 0x1C };
		short[] pal_lengths = new short[4] { 8, 8, 8, 0x10 };
		string[] pal_filenames = new string[4] { "Cycle - SBZ 3.bin", "Cycle - SBZ 5.bin", "Cycle - SBZ 6.bin", "Cycle - SBZ 7.bin" };
		byte[] pal_offsets = new byte[4] { 0x37, 0x38, 0x39, 0x3F };
		Color[][] Pal_SBZCyc = new Color[4][];
		short[] PalCycle_Timer = new short[4], PalCycle_Frame = new short[4];

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			LevelData.BmpPal.Entries[0] = LevelData.Palette[0][2, 0].RGBColor;
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			tmpimg = new BitmapBits(Math.Min(levelimg.Width, width), height);
			Horiz_Scroll_Buf = new int[levelimg.Height];
			smokeimgs = new BitmapBits[8];
			smokeimgs[0] = new BitmapBits(16, 48);
			byte[] Art_SbzSmoke = Compression.Decompress("SBZ Background Smoke.bin", CompressionType.Uncompressed);
			int t = 0;
			for (int i = 1; i < 8; i++)
			{
				smokeimgs[i] = new BitmapBits(16,48);
				for (int y = 0; y < 48; y += 8)
					for (int x = 0; x < 16; x += 8)
						smokeimgs[i].DrawBitmap(LevelData.TileToBmp8bpp(Art_SbzSmoke, t++, 3), x, y);
			}
			AnimFrame1 = AnimFrame2 = AnimTime1 = AnimTime2 = AnimWait1 = AnimWait2 = 0;
			Anim1Locs = new List<Point>();
			Anim2Locs = new List<Point>();
			for (int cy = 0; cy < LevelData.BGHeight; cy++)
				for (int cx = 0; cx < LevelData.BGWidth; cx++)
					for (int by = 0; by < LevelData.Level.ChunkHeight / 16; by++)
						for (int bx = 0; bx < LevelData.Level.ChunkWidth / 16; bx++)
						{
							if (LevelData.Chunks[LevelData.Layout.BGLayout[cx, cy]].Blocks[bx, by].Block == 0x64)
								Anim1Locs.Add(new Point((cx * LevelData.Level.ChunkWidth) + (bx * 16), (cy * LevelData.Level.ChunkHeight) + (by * 16)));
							else if (LevelData.Chunks[LevelData.Layout.BGLayout[cx, cy]].Blocks[bx, by].Block == 0x67)
								Anim2Locs.Add(new Point((cx * LevelData.Level.ChunkWidth) + (bx * 16), (cy * LevelData.Level.ChunkHeight) + (by * 16)));
						}
			for (int i = 0; i < 4; i++)
				Pal_SBZCyc[i] = SonLVLColor.Load(pal_filenames[i], EngineVersion.S1).Select(a => a.RGBColor).ToArray();
			Array.Clear(PalCycle_Timer, 0, 4);
			Array.Clear(PalCycle_Frame, 0, 4);
			Camera_X_pos = 0;
			Camera_BG_X_pos = 0;
			Camera_BG2_X_pos = 0;
			Camera_BG3_X_pos = 0;
			UpdateAnimatedTiles();
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
				BWL d4 = (int)(Camera_X_pos_diff << 8);
				d4.sl <<= 7;
				Camera_BG_X_pos.l += d4.l;
				d4 = (int)(Camera_X_pos_diff << 8);
				d4.sl <<= 6;
				Camera_BG3_X_pos.l += d4.l;
				d4 = (int)(Camera_X_pos_diff << 8);
				d4.sl <<= 5;
				d4.sl += d4.sl * 2;
				Camera_BG2_X_pos.l += d4.l;
				int a1 = 0;
				BWL d2 = Camera_X_pos;
				d2.sw >>= 2;
				BWL d0 = d2.sw >> 1;
				d0.sw -= d2.sw;
				d0.ExtendL();
				d0.sl <<= 3;
				d0.sl /= 4;
				d0.ExtendL();
				d0.sl <<= 12;
				BWL d3 = (short)(d2.sw >> 1);
				for (int i = 0; i < 4; i++)
				{
					TempArray_LayerDef[a1++] = d3.sw;
					d3.Swap();
					d3.l += d0.l;
					d3.Swap();
				}
				TempArray_LayerDef.FastFill(Camera_BG3_X_pos.hsw, a1, 10);
				a1 += 10;
				TempArray_LayerDef.FastFill(Camera_BG2_X_pos.hsw, a1, 7);
				a1 += 7;
				TempArray_LayerDef.FastFill(Camera_BG_X_pos.hsw, a1, 11);
				a1 += 11;
				int a2 = 0;
				a1 = 0;
				while (a1 < levelimg.Height)
				{
					Horiz_Scroll_Buf.FastFill(TempArray_LayerDef[a2++], a1, 16);
					a1 += 16;
				}
				levelimg.ScrollHV(tmpimg, tmpimg.Height - levelimg.Height, 0, Horiz_Scroll_Buf);
				bgimg = tmpimg.ToBitmap(LevelData.BmpPal);
			}
		}

		public override void UpdateAnimatedTiles()
		{
			if (AnimWait1 == 0)
			{
				if (--AnimTime1 < 0)
				{
					AnimTime1 = 7;
					int fr = AnimFrame1;
					AnimFrame1 = (AnimFrame1 + 1) & 7;
					if (AnimFrame1 == 0)
						AnimWait1 = 180;
					foreach (Point item in Anim1Locs)
						levelimg.DrawBitmap(smokeimgs[fr], item);
					return;
				}
			}
			else
			{
				if (AnimWait1 == 180)
					foreach (Point item in Anim1Locs)
						levelimg.DrawBitmap(smokeimgs[0], item);
				--AnimWait1;
			}
			if (AnimWait2 == 0)
			{
				if (--AnimTime2 < 0)
				{
					AnimTime2 = 7;
					int fr = AnimFrame2;
					AnimFrame2 = (AnimFrame2 + 1) & 7;
					if (AnimFrame2 == 0)
						AnimWait2 = 120;
					foreach (Point item in Anim2Locs)
						levelimg.DrawBitmap(smokeimgs[fr], item);
				}
			}
			else
			{
				if (AnimWait2 == 120)
					foreach (Point item in Anim2Locs)
						levelimg.DrawBitmap(smokeimgs[0], item);
				--AnimWait2;
			}
		}

		public override void UpdatePalette()
		{
			for (int i = 0; i < 4; i++)
				if (--PalCycle_Timer[i] == -1)
				{
					PalCycle_Timer[i] = pal_durations[i];
					int frame = PalCycle_Frame[i]++ % pal_lengths[i];
					LevelData.BmpPal.Entries[pal_offsets[i]] = Pal_SBZCyc[i][frame];
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
		[System.ComponentModel.DefaultValue("ScrapBrain")]
		[IniName("music")]
		public string Music { get; set; }
	}
}
