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
		BitmapBits levelimg, tmpimg, layer1img, layer2img;
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
			layer2img = new BitmapBits("layer2.png");
			layer2img.IncrementIndexes(0x30);
			layer2img = layer2img.Scale(scale);
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
				TempArray_LayerDef[0x11] += 4 * scale;
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
				TempArray_LayerDef[a2++] = d3.sw;
				d3.Swap();
				d3.l += d0.l;
				d3.Swap();
				TempArray_LayerDef[a2++] = d3.sw;
				d3.Swap();
				d3.l += d0.l;
				d3.Swap();
				TempArray_LayerDef[a2++] = d3.sw;
				TempArray_LayerDef[a2++] = d3.sw;
				d3.Swap();
				d3.l += d0.l;
				d3.l += d0.l;
				d3.Swap();
				for (int i = 0; i < 4; i++)
				{
					TempArray_LayerDef[a2++] = d3.sw;
					TempArray_LayerDef[a2++] = d3.sw;
					TempArray_LayerDef[a2++] = d3.sw;
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
				tmp = new BitmapBits(layer2img);
				for (int i = 0; i < 0x10; i++)
					Horiz_Scroll_Buf.FastFill(TempArray_LayerDef[i], i * scale, scale);
				tmp.ScrollHorizontal(Horiz_Scroll_Buf);
				for (int x = 0; x < tmpimg.Width; x += tmp.Width)
					tmpimg.DrawBitmapBehind(tmp, x, stY + 0x70 * scale);
				bgimg = tmpimg.ToBitmap(LevelData.BmpPal);
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
