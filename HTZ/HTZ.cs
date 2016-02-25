using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace HTZ
{
	public class HTZ : SonicBGScrollSaver.Level
	{
		readonly int[] TempArray_LayerDef = new int[256];
		int[] Horiz_Scroll_Buf;
		short Camera_X_pos;
		BitmapBits levelimg, tmpimg, layer1img;
		byte[] ArtUnc_HTZClouds;
		Bitmap bgimg = new Bitmap(1, 1);
		int Width, Height;
		LevelInfo levelinfo;
		int scale;

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			levelinfo = IniSerializer.Deserialize<LevelInfo>("setup.ini");
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			LevelData.BmpPal.Entries[0] = LevelData.Palette[0][2, 0].RGBColor;
			LevelData.BmpPal.Entries[0x30] = LevelData.BmpPal.Entries[0];
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			if (levelinfo.Scale < 1)
				scale = height / levelimg.Height;
			else
				scale = levelinfo.Scale;
			levelimg = levelimg.Scale(scale);
			tmpimg = new BitmapBits(Math.Min(levelimg.Width, width), height);
			layer1img = new BitmapBits("layer1.png");
			layer1img.IncrementIndexes(0x20);
			layer1img = layer1img.Scale(scale);
			byte[] buf = System.IO.File.ReadAllBytes("Background clouds (HTZ).bin");
			ArtUnc_HTZClouds = new byte[buf.Length * 2];
			for (int i = 0; i < buf.Length; i++)
			{
				ArtUnc_HTZClouds[i * 2] = (byte)((buf[i] >> 4) | 0x30);
				ArtUnc_HTZClouds[(i * 2) + 1] = (byte)((buf[i] & 0xF) | 0x30);
			}
			Horiz_Scroll_Buf = new int[levelimg.Height];
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
				int a1 = 0;
				BWL d0 = (short)(Camera_X_pos * scale);
				short d2 = d0.sw;
				d0.sw >>= 3;
				for (; a1 < 0x80 * scale; a1++)
					Horiz_Scroll_Buf[a1] = d0.sw;
				BWL d4 = d0;
				d0.sw = (short)TempArray_LayerDef[0x11];
				TempArray_LayerDef[0x11] += 4;
				d2 -= d0.sw;
				d0.sw = d2;
				short d1 = d0.sw;
				d0.sw >>= 1;
				d1 >>= 4;
				d0.sw -= d1;
				d0.ExtendL();
				d0.l <<= 8;
				d0.sl /= 0x70;
				d0.ExtendL();
				d0.l <<= 8;
				int a2 = 0;
				BWL d3 = d1;
				d3.Swap();
				d3.l += d0.l;
				d3.Swap();
				TempArray_LayerDef[a2++] = (int)Math.Round(d3.sw / (double)scale, MidpointRounding.AwayFromZero);
				d3.Swap();
				d3.l += d0.l;
				d3.Swap();
				TempArray_LayerDef[a2++] = (int)Math.Round(d3.sw / (double)scale, MidpointRounding.AwayFromZero);
				d3.Swap();
				d3.l += d0.l;
				d3.Swap();
				TempArray_LayerDef[a2++] = (int)Math.Round(d3.sw / (double)scale, MidpointRounding.AwayFromZero);
				TempArray_LayerDef[a2++] = (int)Math.Round(d3.sw / (double)scale, MidpointRounding.AwayFromZero);
				d3.Swap();
				d3.l += d0.l;
				d3.l += d0.l;
				d3.Swap();
				for (int i = 0; i < 4; i++)
				{
					TempArray_LayerDef[a2++] = (int)Math.Round(d3.sw / (double)scale, MidpointRounding.AwayFromZero);
					TempArray_LayerDef[a2++] = (int)Math.Round(d3.sw / (double)scale, MidpointRounding.AwayFromZero);
					TempArray_LayerDef[a2++] = (int)Math.Round(d3.sw / (double)scale, MidpointRounding.AwayFromZero);
					d3.Swap();
					d3.l += d0.l;
					d3.l += d0.l;
					d3.l += d0.l;
					d3.Swap();
				}
				d0.l *= 4;
				d4.w = d3.w;
				for (int i = 0; i < 3 * scale; i++)
					Horiz_Scroll_Buf[a1++] = d4.sw;
				d3.Swap();
				d3.l += d0.l;
				d3.Swap();
				d4.w = d3.w;
				for (int i = 0; i < 5 * scale; i++)
					Horiz_Scroll_Buf[a1++] = d4.sw;
				d3.Swap();
				d3.l += d0.l;
				d3.Swap();
				d4.w = d3.w;
				for (int i = 0; i < 7 * scale; i++)
					Horiz_Scroll_Buf[a1++] = d4.sw;
				d3.Swap();
				d3.l += d0.l;
				d3.l += d0.l;
				d3.Swap();
				d4.w = d3.w;
				for (int i = 0; i < 8 * scale; i++)
					Horiz_Scroll_Buf[a1++] = d4.sw;
				d3.Swap();
				d3.l += d0.l;
				d3.l += d0.l;
				d3.Swap();
				d4.w = d3.w;
				for (int i = 0; i < 0xA * scale; i++)
					Horiz_Scroll_Buf[a1++] = d4.sw;
				d3.Swap();
				d3.l += d0.l;
				d3.l += d0.l;
				d3.l += d0.l;
				d3.Swap();
				d4.w = d3.w;
				for (int i = 0; i < 0xF * scale; i++)
					Horiz_Scroll_Buf[a1++] = d4.sw;
				d3.Swap();
				d3.l += d0.l;
				d3.l += d0.l;
				d3.l += d0.l;
				d3.Swap();
				for (int j = 0; j < 5; j++)
				{
					d4.w = d3.w;
					for (int i = 0; i < 0x10 * scale; i++)
						Horiz_Scroll_Buf[a1++] = d4.sw;
					d3.Swap();
					d3.l += d0.l;
					d3.l += d0.l;
					d3.l += d0.l;
					d3.l += d0.l;
					d3.Swap();
				}
				int stY = Math.Max(tmpimg.Height - levelimg.Height, 0);
				levelimg.ScrollHV(tmpimg, stY, 0, Horiz_Scroll_Buf);
				BitmapBits tmp = new BitmapBits(layer1img);
				tmp.ScrollHorizontal((Camera_X_pos  + 0xF) / 10);
				for (int x = 0; x < tmpimg.Width; x += tmp.Width)
					tmpimg.DrawBitmapBehind(tmp, x, stY + 0x50 * scale);
				tmp = UpdateClouds();
				tmp.ScrollHorizontal(-Horiz_Scroll_Buf[0]);
				for (int x = 0; x < tmpimg.Width; x += tmp.Width)
					tmpimg.DrawBitmapBehind(tmp, x, stY + 0x70 * scale);
				bgimg = tmpimg.ToBitmap(LevelData.BmpPal);
			}
		}

		int[] lut =
		{
			0x000, 0x001, 0x002, 0x003, 0x004, 0x005, 0x006, 0x007,
			0x020, 0x021, 0x022, 0x023, 0x024, 0x025, 0x026, 0x027,
			0x040, 0x041, 0x042, 0x043, 0x044, 0x045, 0x046, 0x047,
			0x060, 0x061, 0x062, 0x063, 0x064, 0x065, 0x066, 0x067,
			0x080, 0x081, 0x082, 0x083, 0x084, 0x085, 0x086, 0x087,
			0x0A0, 0x0A1, 0x0A2, 0x0A3, 0x0A4, 0x0A5, 0x0A6, 0x0A7,
			0x0C0, 0x0C1, 0x0C2, 0x0C3, 0x0C4, 0x0C5, 0x0C6, 0x0C7,
			0x0E0, 0x0E1, 0x0E2, 0x0E3, 0x0E4, 0x0E5, 0x0E6, 0x0E7,
			0x100, 0x101, 0x102, 0x103, 0x104, 0x105, 0x106, 0x107,
			0x120, 0x121, 0x122, 0x123, 0x124, 0x125, 0x126, 0x127,
			0x140, 0x141, 0x142, 0x143, 0x144, 0x145, 0x146, 0x147,
			0x160, 0x161, 0x162, 0x163, 0x164, 0x165, 0x166, 0x167,
			0x180, 0x181, 0x182, 0x183, 0x184, 0x185, 0x186, 0x187,
			0x1A0, 0x1A1, 0x1A2, 0x1A3, 0x1A4, 0x1A5, 0x1A6, 0x1A7,
			0x1C0, 0x1C1, 0x1C2, 0x1C3, 0x1C4, 0x1C5, 0x1C6, 0x1C7,
			0x1E0, 0x1E1, 0x1E2, 0x1E3, 0x1E4, 0x1E5, 0x1E6, 0x1E7,
			0x008, 0x009, 0x00A, 0x00B, 0x00C, 0x00D, 0x00E, 0x00F,
			0x028, 0x029, 0x02A, 0x02B, 0x02C, 0x02D, 0x02E, 0x02F,
			0x048, 0x049, 0x04A, 0x04B, 0x04C, 0x04D, 0x04E, 0x04F,
			0x068, 0x069, 0x06A, 0x06B, 0x06C, 0x06D, 0x06E, 0x06F,
			0x088, 0x089, 0x08A, 0x08B, 0x08C, 0x08D, 0x08E, 0x08F,
			0x0A8, 0x0A9, 0x0AA, 0x0AB, 0x0AC, 0x0AD, 0x0AE, 0x0AF,
			0x0C8, 0x0C9, 0x0CA, 0x0CB, 0x0CC, 0x0CD, 0x0CE, 0x0CF,
			0x0E8, 0x0E9, 0x0EA, 0x0EB, 0x0EC, 0x0ED, 0x0EE, 0x0EF,
			0x108, 0x109, 0x10A, 0x10B, 0x10C, 0x10D, 0x10E, 0x10F,
			0x128, 0x129, 0x12A, 0x12B, 0x12C, 0x12D, 0x12E, 0x12F,
			0x148, 0x149, 0x14A, 0x14B, 0x14C, 0x14D, 0x14E, 0x14F,
			0x168, 0x169, 0x16A, 0x16B, 0x16C, 0x16D, 0x16E, 0x16F,
			0x188, 0x189, 0x18A, 0x18B, 0x18C, 0x18D, 0x18E, 0x18F,
			0x1A8, 0x1A9, 0x1AA, 0x1AB, 0x1AC, 0x1AD, 0x1AE, 0x1AF,
			0x1C8, 0x1C9, 0x1CA, 0x1CB, 0x1CC, 0x1CD, 0x1CE, 0x1CF,
			0x1E8, 0x1E9, 0x1EA, 0x1EB, 0x1EC, 0x1ED, 0x1EE, 0x1EF,
			0x010, 0x011, 0x012, 0x013, 0x014, 0x015, 0x016, 0x017,
			0x030, 0x031, 0x032, 0x033, 0x034, 0x035, 0x036, 0x037,
			0x050, 0x051, 0x052, 0x053, 0x054, 0x055, 0x056, 0x057,
			0x070, 0x071, 0x072, 0x073, 0x074, 0x075, 0x076, 0x077,
			0x090, 0x091, 0x092, 0x093, 0x094, 0x095, 0x096, 0x097,
			0x0B0, 0x0B1, 0x0B2, 0x0B3, 0x0B4, 0x0B5, 0x0B6, 0x0B7,
			0x0D0, 0x0D1, 0x0D2, 0x0D3, 0x0D4, 0x0D5, 0x0D6, 0x0D7,
			0x0F0, 0x0F1, 0x0F2, 0x0F3, 0x0F4, 0x0F5, 0x0F6, 0x0F7,
			0x110, 0x111, 0x112, 0x113, 0x114, 0x115, 0x116, 0x117,
			0x130, 0x131, 0x132, 0x133, 0x134, 0x135, 0x136, 0x137,
			0x150, 0x151, 0x152, 0x153, 0x154, 0x155, 0x156, 0x157,
			0x170, 0x171, 0x172, 0x173, 0x174, 0x175, 0x176, 0x177,
			0x190, 0x191, 0x192, 0x193, 0x194, 0x195, 0x196, 0x197,
			0x1B0, 0x1B1, 0x1B2, 0x1B3, 0x1B4, 0x1B5, 0x1B6, 0x1B7,
			0x1D0, 0x1D1, 0x1D2, 0x1D3, 0x1D4, 0x1D5, 0x1D6, 0x1D7,
			0x1F0, 0x1F1, 0x1F2, 0x1F3, 0x1F4, 0x1F5, 0x1F6, 0x1F7,
			0x018, 0x019, 0x01A, 0x01B, 0x01C, 0x01D, 0x01E, 0x01F,
			0x038, 0x039, 0x03A, 0x03B, 0x03C, 0x03D, 0x03E, 0x03F,
			0x058, 0x059, 0x05A, 0x05B, 0x05C, 0x05D, 0x05E, 0x05F,
			0x078, 0x079, 0x07A, 0x07B, 0x07C, 0x07D, 0x07E, 0x07F,
			0x098, 0x099, 0x09A, 0x09B, 0x09C, 0x09D, 0x09E, 0x09F,
			0x0B8, 0x0B9, 0x0BA, 0x0BB, 0x0BC, 0x0BD, 0x0BE, 0x0BF,
			0x0D8, 0x0D9, 0x0DA, 0x0DB, 0x0DC, 0x0DD, 0x0DE, 0x0DF,
			0x0F8, 0x0F9, 0x0FA, 0x0FB, 0x0FC, 0x0FD, 0x0FE, 0x0FF,
			0x118, 0x119, 0x11A, 0x11B, 0x11C, 0x11D, 0x11E, 0x11F,
			0x138, 0x139, 0x13A, 0x13B, 0x13C, 0x13D, 0x13E, 0x13F,
			0x158, 0x159, 0x15A, 0x15B, 0x15C, 0x15D, 0x15E, 0x15F,
			0x178, 0x179, 0x17A, 0x17B, 0x17C, 0x17D, 0x17E, 0x17F,
			0x198, 0x199, 0x19A, 0x19B, 0x19C, 0x19D, 0x19E, 0x19F,
			0x1B8, 0x1B9, 0x1BA, 0x1BB, 0x1BC, 0x1BD, 0x1BE, 0x1BF,
			0x1D8, 0x1D9, 0x1DA, 0x1DB, 0x1DC, 0x1DD, 0x1DE, 0x1DF,
			0x1F8, 0x1F9, 0x1FA, 0x1FB, 0x1FC, 0x1FD, 0x1FE, 0x1FF,
		};

		BitmapBits UpdateClouds()
		{
			BitmapBits result = new BitmapBits(32, 16);
			int a1 = 0;
			short d2 = (short)(Camera_X_pos >> 3);
			int a0 = 0;
			int a2 = 0;
			for (int i = 0; i < 0x10; i++)
			{
				short d0 = (short)TempArray_LayerDef[a1++];
				d0 += d2;
				d0 &= 0x1F;
				int a4 = a0 + d0;
				for (int j = 0; j < 3; j++)
				{
					Array.Copy(ArtUnc_HTZClouds, a4, result.Bits, lut[a2], 8);
					a4 += 8;
					a2 += 0x80;
				}
				Array.Copy(ArtUnc_HTZClouds, a4, result.Bits, lut[a2], 8);
				a2 -= 0x178;
				a0 += 0x40;
			}
			return result.Scale(scale);
		}

		public override void PlayMusic()
		{
			SonicBGScrollSaver.Music.PlaySong(levelinfo.Music);
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
		[System.ComponentModel.DefaultValue("CasinoNight")]
		[IniName("music")]
		public string Music { get; set; }
		[IniIgnore]
		public int Scale { get; set; }
		[System.ComponentModel.DefaultValue("Auto")]
		[IniName("scale")]
		public string ScaleString
		{
			get
			{
				if (Scale < 1)
					return "Auto";
				return Scale.ToString();
			}
			set
			{
				int i;
				if (value.Equals("Auto", StringComparison.OrdinalIgnoreCase))
					Scale = 0;
				else if (int.TryParse(value, out i))
					Scale = i;
				else
					Scale = 1;
			}
		}
	}
}
