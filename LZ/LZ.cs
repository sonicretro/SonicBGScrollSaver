﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace LZ
{
	public class LZ : SonicBGScrollSaver.Level
	{
		int[] Horiz_Scroll_Buf;
		BWL Camera_BG_X_pos;
		short Camera_BG_Y_pos;
		BWL LZ_Water_Ripple;
		BitmapBits levelimg, tmpimg;
		Bitmap bgimg = new Bitmap(1, 1);
		int Width, Height;
		LevelInfo levelinfo;
		WaterMode waterMode;
		int waterheight;
		List<Sprite> surfacesprites;
		sbyte surfacetimer;
		byte surfaceframe;
		bool shiftsurface;

		sbyte[] Drown_WobbleData = {
			0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2,
			2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
			3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 2,
			2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0,
			0, -1, -1, -1, -1, -1, -2, -2, -2, -2, -2, -3, -3, -3, -3, -3,
			-3, -3, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4,
			-4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -3,
			-3, -3, -3, -3, -3, -3, -2, -2, -2, -2, -2, -1, -1, -1, -1, -1,
			0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2,
			2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3,
			3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 2,
			2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0,
			0, -1, -1, -1, -1, -1, -2, -2, -2, -2, -2, -3, -3, -3, -3, -3,
			-3, -3, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4,
			-4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -4, -3,
			-3, -3, -3, -3, -3, -3, -2, -2, -2, -2, -2, -1, -1, -1, -1, -1
			};

		bool oscDir;
		short oscRate;
		BWL oscVal;
		const short oscFreq = 2, oscAmp = 0x10;

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			levelinfo = IniSerializer.Deserialize<LevelInfo>("setup.ini");
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			LevelData.BmpPal.Entries[0] = LevelData.Palette[0][2, 0].RGBColor;
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			tmpimg = new BitmapBits(Math.Min(width, levelimg.Width), height);
			for (int l = 0; l < 4; l++)
				for (int i = 0; i < 16; i++)
					LevelData.BmpPal.Entries[(l * 16) + i + 64] = LevelData.Palette[1][l, i].RGBColor;
			LevelData.BmpPal.Entries[64] = LevelData.Palette[1][2, 0].RGBColor;
			surfacesprites = new List<Sprite>();
			byte[] art = Compression.Decompress("../LZ Water Surface.bin", CompressionType.Nemesis);
			foreach (MappingsFrame frame in MappingsFrame.LoadASM("../Water Surface.asm", LevelData.Game.EngineVersion))
				surfacesprites.Add(new Sprite(LevelData.MapFrameToBmp(art, frame, 2)));
			Horiz_Scroll_Buf = new int[height];
			waterheight = height / 2;
			waterMode = levelinfo.WaterMode;
			oscVal = 0x80;
			oscRate = 0;
			Camera_BG_X_pos = 0;
			Camera_BG_Y_pos = 0;
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
				Camera_BG_X_pos.sl += Camera_X_pos_diff << 15;
				Camera_BG_Y_pos += Camera_Y_pos_diff;
				if (!oscDir)
				{
					oscRate += oscFreq;
					oscVal.sw += oscRate;
					if (oscVal.b1 > oscAmp)
						oscDir = true;
				}
				else
				{
					oscRate -= oscFreq;
					oscVal.sw += oscRate;
					if (oscVal.b1 <= oscAmp)
						oscDir = false;
				}
				byte d2 = LZ_Water_Ripple.b1;
				LZ_Water_Ripple.w += 0x80;
				if (waterMode != WaterMode.None)
				{
					int screenwater = 0;
					if (waterMode == WaterMode.Partial)
						screenwater = waterheight - Camera_BG_Y_pos + (oscVal.b1 >> 1);
					if (screenwater > 0)
						Horiz_Scroll_Buf.FastFill(Camera_BG_X_pos.hsw, 0, Math.Min(screenwater, tmpimg.Height));
					d2 = (byte)(d2 + screenwater);
					for (int i = Math.Max(screenwater, 0); i < tmpimg.Height; i++)
						Horiz_Scroll_Buf[i] = Camera_BG_X_pos.hsw + Drown_WobbleData[d2++];
					int[] tmpbuf = new int[Horiz_Scroll_Buf.Length];
					int y = Camera_BG_Y_pos % levelimg.Height;
					if (y < 0)
						y += levelimg.Height;
					for (int i = 0; i < Horiz_Scroll_Buf.Length; i++)
						tmpbuf[(i + y) % Horiz_Scroll_Buf.Length] = Horiz_Scroll_Buf[i];
					levelimg.ScrollHV(tmpimg, 0, Camera_BG_Y_pos, tmpbuf);
					if (waterMode == WaterMode.Partial)
					{
						int surfx = -(Camera_BG_X_pos.hsw % 0x20) + 0x60;
						if (shiftsurface)
							surfx += 0x20;
						shiftsurface = !shiftsurface;
						if (--surfacetimer < 0)
						{
							surfacetimer = 7;
							surfaceframe = (byte)((surfaceframe + 1) % 3);
						}
						for (int i = 0; i < tmpimg.Width; i += 0xC0)
							tmpimg.DrawSprite(surfacesprites[surfaceframe], surfx + i, screenwater);
					}
					if (screenwater < Height)
						tmpimg.ApplyWaterPalette(Math.Max(screenwater, 0));
				}
				else
					levelimg.ScrollHV(tmpimg, 0, Camera_BG_Y_pos, Camera_BG_X_pos.hsw);
				bgimg = tmpimg.ToBitmap(LevelData.BmpPal);
			}
		}

		public override void PlayMusic()
		{
			SonicBGScrollSaver.Music.PlaySong(levelinfo.Music);
		}

		public override void ToggleWater()
		{
			switch (waterMode)
			{
				case WaterMode.None:
					waterMode = WaterMode.Partial;
					break;
				case WaterMode.Partial:
					waterMode = WaterMode.Full;
					break;
				case WaterMode.Full:
					waterMode = WaterMode.None;
					break;
			}
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

	internal class LevelInfo
	{
		[System.ComponentModel.DefaultValue("Labyrinth")]
		[IniName("music")]
		public string Music { get; set; }
		[System.ComponentModel.DefaultValue(WaterMode.Partial)]
		[IniName("water")]
		public WaterMode WaterMode { get; set; }
	}

	public enum WaterMode
	{
		None,
		Partial,
		Full
	}
}
