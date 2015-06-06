using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CPZ
{
	public class CPZ : SonicBGScrollSaver.Level
	{
		int[] Horiz_Scroll_Buf;
		short Camera_X_pos;
		int Camera_Y_pos;
		BWL Camera_BG_X_pos, Camera_BG2_X_pos;
		BitmapBits levelimg, tmpimg;
		Bitmap bgimg = new Bitmap(1, 1);
		int Width, Height;
		int AnimTime, AnimFrame;
		BitmapBits[] CPZAnimBackImgs, CPZAnimBackImgsFlip;
		List<Point> Anim1Locs, Anim2Locs;
		short[] pal_lengths = new short[3] { 9, 0x15, 0xF };
		string[] pal_filenames = new string[3] { "CPZ Cycle 1.bin", "CPZ Cycle 2.bin", "CPZ Cycle 3.bin" };
		byte[] pal_offsets = new byte[3] { 0x3C, 0x3F, 0x2F };
		int[] pal_numentries = new int[3] { 3, 1, 1 };
		Color[][] CyclingPal_CPZ = new Color[3][];
		short PalCycle_Timer;
		short[] PalCycle_Frame = new short[3];
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
			LevelData.BmpPal.Entries[0] = LevelData.Palette[0][2, 0].RGBColor;
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			tmpimg = new BitmapBits(Math.Min(levelimg.Width, width), height);
			Horiz_Scroll_Buf = new int[levelimg.Height];
			CPZAnimBackImgs = new BitmapBits[8];
			CPZAnimBackImgsFlip = new BitmapBits[8];
			byte[] ArtUnc_CPZAnimBack = Compression.Decompress("Animated background section (CPZ and DEZ).bin", CompressionType.Uncompressed);
			int t = 0;
			for (int i = 0; i < 8; i++)
			{
				CPZAnimBackImgs[i] = new BitmapBits(16, 16);
				for (int x = 0; x < 16; x += 8)
				{
					for (int y = 0; y < 16; y += 8)
						CPZAnimBackImgs[i].DrawBitmap(LevelData.TileToBmp8bpp(ArtUnc_CPZAnimBack, t, 2), x, y);
					++t;
				}
				CPZAnimBackImgsFlip[i] = new BitmapBits(CPZAnimBackImgs[i]);
				CPZAnimBackImgsFlip[i].Flip(true, false);
			}
			AnimFrame = AnimTime = 0;
			Anim1Locs = new List<Point>();
			Anim2Locs = new List<Point>();
			for (int cy = 0; cy < LevelData.BGHeight; cy++)
				for (int cx = 0; cx < LevelData.BGWidth; cx++)
					for (int by = 0; by < LevelData.Level.ChunkHeight / 16; by++)
						for (int bx = 0; bx < LevelData.Level.ChunkWidth / 16; bx++)
						{
							if (LevelData.Chunks[LevelData.Layout.BGLayout[cx, cy]].Blocks[bx, by].Block == 0x2FF)
								if (!LevelData.Chunks[LevelData.Layout.BGLayout[cx, cy]].Blocks[bx, by].XFlip)
									Anim1Locs.Add(new Point((cx * LevelData.Level.ChunkWidth) + (bx * 16), (cy * LevelData.Level.ChunkHeight) + (by * 16)));
								else
									Anim2Locs.Add(new Point((cx * LevelData.Level.ChunkWidth) + (bx * 16), (cy * LevelData.Level.ChunkHeight) + (by * 16)));
						}
			for (int i = 0; i < 3; i++)
				CyclingPal_CPZ[i] = SonLVLColor.Load(pal_filenames[i], EngineVersion.S2).Select(a => a.RGBColor).ToArray();
			PalCycle_Timer = 0;
			Array.Clear(PalCycle_Frame, 0, 3);
			framecounter = 0;
			Camera_X_pos = 0;
			if (levelimg.Height < Height)
				Camera_Y_pos = levelimg.Height - Height;
			else
				Camera_Y_pos = 0;
			Camera_BG_X_pos = 0;
			Camera_BG2_X_pos = 0;
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
				if (levelimg.Height > Height)
					Camera_Y_pos = Math.Min(Math.Max(0, Camera_Y_pos + Camera_Y_pos_diff), levelimg.Height - Height);
				BWL d4 = (int)(Camera_X_pos_diff << 8);
				d4.sl <<= 5;
				Camera_BG_X_pos.l += d4.l;
				d4 = (int)(Camera_X_pos_diff << 8);
				d4.sl <<= 7;
				Camera_BG2_X_pos.l += d4.l;
				framecounter--;
				d4 = 0;
				int a1 = 0;
				while (a1 < levelimg.Height)
				{
					switch (d4.sl.CompareTo(0x12))
					{
						case -1:
							Horiz_Scroll_Buf.FastFill(Camera_BG_X_pos.hsw, a1, 16);
							a1 += 16;
							break;
						case 0:
							short d3 = Camera_BG_X_pos.hsw;
							int a2 = (framecounter >> 3) & 0x1F;
							for (int i = 0; i < 16; i++)
								Horiz_Scroll_Buf[a1++] = SwScrl_RippleData[a2++] + d3;
							break;
						case 1:
							Horiz_Scroll_Buf.FastFill(Camera_BG2_X_pos.hsw, a1, 16);
							a1 += 16;
							break;
					}
					++d4.sl;
				}
				levelimg.ScrollHV(tmpimg, Math.Max(0, -Camera_Y_pos), Math.Max(0, Camera_Y_pos), Horiz_Scroll_Buf);
				bgimg = tmpimg.ToBitmap(LevelData.BmpPal);
			}
		}

		public override void UpdateAnimatedTiles()
		{
			if (--AnimTime < 0)
			{
				AnimTime = 4;
				int fr = AnimFrame;
				AnimFrame = (AnimFrame + 1) & 7;
				foreach (Point item in Anim1Locs)
					levelimg.DrawBitmap(CPZAnimBackImgs[fr], item);
				foreach (Point item in Anim2Locs)
					levelimg.DrawBitmap(CPZAnimBackImgsFlip[fr], item);
				return;
			}
		}

		public override void UpdatePalette()
		{
			if (--PalCycle_Timer == -1)
			{
				PalCycle_Timer = 7;
				for (int i = 0; i < 3; i++)
				{
					int frame = PalCycle_Frame[i]++ % pal_lengths[i];
					LevelData.BmpPal.Entries[pal_offsets[i]] = CyclingPal_CPZ[i][frame];
				}
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
		[System.ComponentModel.DefaultValue("ChemicalPlant")]
		[IniName("music")]
		public string Music { get; set; }
	}
}
