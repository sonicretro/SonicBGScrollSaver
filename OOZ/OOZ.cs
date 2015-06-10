using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace OOZ
{
	public class OOZ : SonicBGScrollSaver.Level
	{
		int[] Horiz_Scroll_Buf;
		ushort Camera_X_pos;
		BWL Camera_BG_X_pos;
		BitmapBits levelimg, tmpimg;
		Bitmap bgimg = new Bitmap(1, 1);
		Color[] CyclingPal_Oil;
		short PalCycle_Timer, PalCycle_Frame;
		int Width, Height;
		LevelInfo levelinfo;
		int scale;
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
			levelinfo = IniSerializer.Deserialize<LevelInfo>("setup.ini");
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			LevelData.BmpPal.Entries[0] = LevelData.Palette[0][2, 0].RGBColor;
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			if (levelinfo.Scale < 1)
				scale = height / levelimg.Height;
			else
				scale = levelinfo.Scale;
			levelimg = levelimg.Scale(scale);
			tmpimg = new BitmapBits(Math.Min(levelimg.Width, width), height);
			CyclingPal_Oil = SonLVLColor.Load("../OOZ Oil.bin", EngineVersion.S2).Select(a => a.RGBColor).ToArray();
			Horiz_Scroll_Buf = new int[levelimg.Height];
			Camera_X_pos = 0;
			Camera_BG_X_pos = 0;
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
				Camera_BG_X_pos.sl += Camera_X_pos_diff << 13;
				int bgscroll = Camera_BG_X_pos.hsw * scale;
				int fastcloud = (Camera_BG_X_pos.hsw * scale) >> 2;
				int medcloud = (Camera_BG_X_pos.hsw * scale) >> 3;
				int slowcloud = (Camera_BG_X_pos.hsw * scale) >> 4;
				//Array.Clear(Horiz_Scroll_Buf, 0, 152 * scale);
				int a1 = 152 * scale;
				Horiz_Scroll_Buf.FastFill(medcloud, a1, 8 * scale);
				a1 += 8 * scale;
				Horiz_Scroll_Buf.FastFill(slowcloud, a1, 8 * scale);
				a1 += 8 * scale;
				Horiz_Scroll_Buf.FastFill(fastcloud, a1, 8 * scale);
				a1 += 8 * scale;
				Horiz_Scroll_Buf.FastFill(slowcloud, a1, 8 * scale);
				a1 += 8 * scale;
				Horiz_Scroll_Buf.FastFill(medcloud, a1, 8 * scale);
				a1 += 8 * scale;
				framecounter--;
				int a2 = (framecounter >> 3) & 0x1F;
				for (int i = 0; i < 0x21; i++)
				{
					Horiz_Scroll_Buf.FastFill(SwScrl_RippleData[a2++] * scale, a1, scale);
					a1 += scale;
				}
				Horiz_Scroll_Buf.FastFill(slowcloud, a1, 7 * scale);
				a1 += 7 * scale;
				Horiz_Scroll_Buf.FastFill(fastcloud, a1, 8 * scale);
				a1 += 8 * scale;
				Horiz_Scroll_Buf.FastFill(slowcloud, a1, 8 * scale);
				a1 += 8 * scale;
				Horiz_Scroll_Buf.FastFill(medcloud, a1, 8 * scale);
				a1 += 8 * scale;
				Horiz_Scroll_Buf.FastFill(bgscroll, a1, 256 * scale);
				levelimg.ScrollHV(tmpimg, Math.Max(tmpimg.Height - levelimg.Height, 0), 0, Horiz_Scroll_Buf);
				bgimg = tmpimg.ToBitmap(LevelData.BmpPal);
			}
		}

		public override void UpdatePalette()
		{
			if (--PalCycle_Timer == -1)
			{
				PalCycle_Timer = 7;
				int frame = PalCycle_Frame++ & 3;
				Array.Copy(CyclingPal_Oil, frame, LevelData.BmpPal.Entries, 0x2A, 4);
			}
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
		[System.ComponentModel.DefaultValue("OilOcean")]
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
