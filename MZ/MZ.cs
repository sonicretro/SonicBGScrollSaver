using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MZ
{
	public class MZ : SonicBGScrollSaver.Level
	{
		readonly int[] TempArray_LayerDef = new int[256];
		int[] Horiz_Scroll_Buf;
		short Camera_X_pos, Camera_Y_pos;
		BWL Camera_BG_X_pos, Camera_BG2_X_pos, Camera_BG3_X_pos;
		BitmapBits[] levelimg;
		BitmapBits tmpimg;
		Bitmap bgimg = new Bitmap(1, 1);
		int Width, Height;
		int AnimTime, AnimFrame;

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			LevelData.BmpPal.Entries[0] = LevelData.Palette[0][2, 0].RGBColor;
			int tilecount = LevelData.Tiles.Count;
			for (int i = 0; i < 6; i++)
				LevelData.Tiles.Add(new byte[0x20]);
			byte[] Art_MzTorch = Compression.Decompress("MZ Background Torch.bin", CompressionType.Uncompressed);
			levelimg = new BitmapBits[4];
			for (int i = 0; i < 4; i++)
			{
				for (int t = 0; t < 6; t++)
					Array.Copy(Art_MzTorch, ((i * 6) + t) * 0x20, LevelData.Tiles[tilecount + t], 0, 0x20);
				LevelData.UpdateTileArray();
				LevelData.RedrawBlock(0x69, false);
				LevelData.RedrawBlock(0x6A, true);
				levelimg[i] = LevelData.DrawBackground(null, true, true, false, false);
			}
			tmpimg = new BitmapBits(Math.Min(levelimg[0].Width, width), height);
			Horiz_Scroll_Buf = new int[levelimg[0].Height];
			Camera_X_pos = 0;
			Camera_BG_X_pos = 0;
			Camera_BG2_X_pos = 0;
			Camera_BG3_X_pos = 0;
			AnimTime = 1;
			AnimFrame = -1;
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
				Camera_Y_pos = (short)Math.Max(0, Camera_Y_pos + Camera_Y_pos_diff);
				if (Camera_Y_pos + Height > levelimg[AnimFrame].Height)
					Camera_Y_pos = (short)Math.Max(0, levelimg[AnimFrame].Height - Height);
				BWL d4 = (int)(Camera_X_pos_diff << 8);
				d4.sl <<= 6;
				Camera_BG3_X_pos.l += d4.l;
				d4.sl += d4.sl * 2;
				Camera_BG_X_pos.l += d4.l;
				d4 = (int)(Camera_X_pos_diff << 8);
				d4.sl <<= 7;
				Camera_BG2_X_pos.l += d4.l;
				int a1 = 0;
				BWL d2 = Camera_X_pos;
				BWL d0 = d2.sw >> 2;
				d0.sw -= d2.sw;
				d0.ExtendL();
				d0.sl <<= 3;
				d0.sl /= 5;
				d0.ExtendL();
				d0.sl <<= 12;
				BWL d3 = (short)(d2.sw >> 1);
				for (int i = 0; i < 5; i++)
				{
					TempArray_LayerDef[a1++] = d3.sw;
					d3.Swap();
					d3.l += d0.l;
					d3.Swap();
				}
				TempArray_LayerDef.FastFill(Camera_BG3_X_pos.hw, a1, 2);
				a1 += 2;
				TempArray_LayerDef.FastFill(Camera_BG2_X_pos.hw, a1, 9);
				a1 += 9;
				TempArray_LayerDef.FastFill(Camera_BG_X_pos.hw, a1, TempArray_LayerDef.Length - a1);
				int a2 = 0;
				d2.w = 16;
				a1 = 0;
				while (a1 < levelimg[AnimFrame].Height)
				{
					Horiz_Scroll_Buf.FastFill(TempArray_LayerDef[a2++], a1, d2.w);
					a1 += d2.w;
					d2.w = (ushort)Math.Min(16, levelimg[AnimFrame].Height - a1);
				}
				levelimg[AnimFrame].ScrollHV(tmpimg, 0, Camera_Y_pos, Horiz_Scroll_Buf);
				bgimg = tmpimg.ToBitmap(LevelData.BmpPal);
			}
		}

		public override void UpdateAnimatedTiles()
		{
			if (--AnimTime == 0)
			{
				AnimTime = 7;
				AnimFrame = (AnimFrame + 1) & 3;
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
		[System.ComponentModel.DefaultValue("Marble")]
		[IniName("music")]
		public string Music { get; set; }
	}
}
