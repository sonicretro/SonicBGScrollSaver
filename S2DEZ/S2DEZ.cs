using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace S2DEZ
{
	public class S2DEZ : SonicBGScrollSaver.Level
	{
		readonly int[] TempArray_LayerDef = new int[256];
		int[] Horiz_Scroll_Buf;
		int Camera_X_pos, Camera_Y_pos;
		BitmapBits levelimg, tmpimg;
		Bitmap bgimg = new Bitmap(1, 1);
		int Width, Height;

		public byte[] byte_D48A = {
									  8,8,8,8,8,8,8, // 8
									  8,8,8,8,8,8,8,8, // 16
									  8,8,8,8,8,8,8,8, // 24
									  8,8,8,8,8,3,5,8, // 32
									  0x10,0x80,0x81 // 36
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
			tmpimg.Bits.FastFill(0x2D);
			Horiz_Scroll_Buf = new int[levelimg.Height];
			if (height > levelimg.Height)
			{
				Camera_Y_pos = (short)((levelimg.Height / 2) - (height / 2) - 32);
				BitmapBits tmp = new BitmapBits(levelimg.Width, height + Camera_Y_pos);
				tmp.Bits.FastFill(0x2D);
				levelimg.Bits.CopyTo(tmp.Bits, 0);
				levelimg = tmp;
			}
			else
				Camera_Y_pos = 0;
			Camera_X_pos = 0;
			Array.Clear(TempArray_LayerDef, 0, TempArray_LayerDef.Length);
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
				BWL d4 = Camera_X_pos;
				int a2 = 0;
				TempArray_LayerDef[a2++] += 3;
				TempArray_LayerDef[a2++] += 2;
				TempArray_LayerDef[a2++] += 4;
				TempArray_LayerDef[a2++] += 1;
				TempArray_LayerDef[a2++] += 2;
				TempArray_LayerDef[a2++] += 4;
				TempArray_LayerDef[a2++] += 3;
				TempArray_LayerDef[a2++] += 4;
				TempArray_LayerDef[a2++] += 2;
				TempArray_LayerDef[a2++] += 6;
				TempArray_LayerDef[a2++] += 3;
				TempArray_LayerDef[a2++] += 4;
				TempArray_LayerDef[a2++] += 1;
				TempArray_LayerDef[a2++] += 2;
				TempArray_LayerDef[a2++] += 4;
				TempArray_LayerDef[a2++] += 3;
				TempArray_LayerDef[a2++] += 2;
				TempArray_LayerDef[a2++] += 3;
				TempArray_LayerDef[a2++] += 4;
				TempArray_LayerDef[a2++] += 1;
				TempArray_LayerDef[a2++] += 3;
				TempArray_LayerDef[a2++] += 4;
				TempArray_LayerDef[a2++] += 2;
				TempArray_LayerDef[a2] += 1;
				BWL d0 = TempArray_LayerDef[a2++];
				BWL d1 = d0.w;
				d0.w >>= 1;
				TempArray_LayerDef[a2++] = d0.w;
				TempArray_LayerDef[a2++] += 3;
				TempArray_LayerDef[a2++] += 2;
				TempArray_LayerDef[a2++] += 4;
				d1.Swap();
				d0.l = d1.l;
				d1.l >>= 3;
				d0.l -= d1.l;
				d0.Swap();
				TempArray_LayerDef[a2 + 2] = d0.w;
				d0.Swap();
				d0.l -= d1.l;
				d0.Swap();
				TempArray_LayerDef[a2 + 1] = d0.w;
				d0.Swap();
				d0.l -= d1.l;
				d0.Swap();
				TempArray_LayerDef[a2++] = d0.w;
				a2 += 2;
				TempArray_LayerDef[a2++] += 1;
				TempArray_LayerDef[a2++] = d4.sw;
				TempArray_LayerDef[a2++] = d4.sw;
				int a3 = 0;
				a2 = 0;
				d1 = byte_D48A[0];
				d0.l = 0;
				d0.sw = (short)TempArray_LayerDef[a2++];
				for (int i = 0; i < Horiz_Scroll_Buf.Length; i++)
				{
					Horiz_Scroll_Buf[i] = d0.sw;
					if (--d1.sw == 0)
					{
						d1.b = byte_D48A[a3++];
						d0.sw = (short)TempArray_LayerDef[a2++];
					}
				}
				levelimg.ScrollHV(tmpimg, -Camera_Y_pos, 0, Horiz_Scroll_Buf);
				if (Height > levelimg.Height)
					for (int i = -Camera_Y_pos; i >= 0; i -= 224)
						if (i - 224 >= 0)
							Array.Copy(tmpimg.Bits, tmpimg.GetPixelIndex(0, -Camera_Y_pos), tmpimg.Bits, tmpimg.GetPixelIndex(0, i - 224), tmpimg.GetPixelIndex(0, 224));
						else
							Array.Copy(tmpimg.Bits, tmpimg.GetPixelIndex(0, -Camera_Y_pos + (224 - i)), tmpimg.Bits, 0, tmpimg.GetPixelIndex(0, i));
				bgimg = tmpimg.ToBitmap(LevelData.BmpPal);
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
		[System.ComponentModel.DefaultValue("S2DeathEgg")]
		[IniName("music")]
		public string Music { get; set; }
	}
}
