using SonicRetro.SonLVL.API;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace SBZ2
{
	public class SBZ2 : SonicBGScrollSaver.Level
	{
		short Camera_X_pos, Camera_Y_pos;
		BitmapBits levelimg;
		Bitmap bgimg = new Bitmap(1, 1);
		int Width, Height;
		short[] pal_durations = new short[3] { 9, 7, 3 };
		short[] pal_lengths = new short[3] { 8, 8, 3 };
		string[] pal_filenames = new string[3] { "Cycle - SBZ 9.bin", "Cycle - SBZ 6.bin", "Cycle - SBZ 8.bin" };
		byte[] pal_offsets = new byte[3] { 0x38, 0x39, 0x3C };
		Color[][] Pal_SBZCyc = new Color[3][];
		short[] PalCycle_Timer = new short[3], PalCycle_Frame = new short[3];

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			LevelData.BmpPal.Entries[0] = LevelData.Palette[0][2, 0].RGBColor;
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			for (int i = 0; i < 3; i++)
				Pal_SBZCyc[i] = SonLVLColor.Load(pal_filenames[i], EngineVersion.S1).Select(a => a.RGBColor).ToArray();
			Array.Clear(PalCycle_Timer, 0, 3);
			Array.Clear(PalCycle_Frame, 0, 3);
			Camera_X_pos = 0;
			Camera_Y_pos = 0;
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
				Camera_Y_pos += Camera_Y_pos_diff;
				BitmapBits bmp = new BitmapBits(levelimg);
				bmp.ScrollVertical(Camera_Y_pos);
				bmp.ScrollHorizontal(Camera_X_pos / 2);
				if (Width < bmp.Width)
					bmp = bmp.GetSection(0, 0, Width, bmp.Height);
				bgimg = bmp.ToBitmap(LevelData.BmpPal);
			}
		}

		public override void UpdatePalette()
		{
			for (int i = 0; i < 3; i++)
				if (--PalCycle_Timer[i] == -1)
				{
					PalCycle_Timer[i] = pal_durations[i];
					int frame = PalCycle_Frame[i]++ % pal_lengths[i];
					if (i == 2)
						Array.Copy(Pal_SBZCyc[i], frame, LevelData.BmpPal.Entries, pal_offsets[i], 3);
					else
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
