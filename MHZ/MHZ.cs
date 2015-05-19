using SonicRetro.SonLVL.API;
using System;
using System.Drawing;

namespace MHZ
{
    public class MHZ : SonicBGScrollSaver.Level
    {
		int Camera_X_pos, Camera_Y_pos;
		BitmapBits levelimg, layer1img, layer2img;
		Bitmap bgimg = new Bitmap(1, 1);
		int Width, Height;
		LevelInfo levelinfo;
		System.Timers.Timer paltimer;
		int curpal;
		int fadeframe = -1;

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			levelinfo = IniSerializer.Deserialize<LevelInfo>("setup.ini");
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			LevelData.BmpPal.Entries[0] = LevelData.Palette[0][2, 0].RGBColor;
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			layer1img = new BitmapBits("../layer1.png");
			layer1img.IncrementIndexes(0x30);
			layer2img = new BitmapBits("../layer2.png");
			layer2img.IncrementIndexes(0x30);
			Camera_Y_pos = 0;
			if (height < levelimg.Height)
				Camera_Y_pos += (short)((levelimg.Height / 2) - (height / 2));
			Camera_X_pos = 0;
			curpal = 0;
			fadeframe = -1;
			UpdateScrolling(0, 0);
			if (LevelData.Palette.Count > 1)
			{
				if (paltimer != null)
				{
					paltimer.Stop();
					paltimer.Dispose();
				}
				paltimer = new System.Timers.Timer((levelinfo.PaletteTime ?? TimeSpan.FromMinutes(1)).TotalMilliseconds);
				paltimer.Elapsed += paltimer_Elapsed;
				paltimer.Start();
			}
		}

		void paltimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			fadeframe = 0;
		}

		public override System.Drawing.Bitmap GetBG()
		{
			return bgimg;
		}

		public override void UpdateScrolling(short Camera_X_pos_diff, short Camera_Y_pos_diff)
		{
			if (fadeframe >= 0)
			{
				if (fadeframe == levelinfo.FadeLength)
				{
					curpal = (curpal + 1) % LevelData.Palette.Count;
					int i = 0;
					for (int y = 0; y < 4; y++)
						for (int x = 0; x < 16; x++)
							LevelData.BmpPal.Entries[i++] = LevelData.Palette[curpal][y, x].RGBColor;
					LevelData.BmpPal.Entries[0] = LevelData.Palette[curpal][2, 0].RGBColor;
					fadeframe = -1;
					paltimer.Start();
				}
				else
				{
					int i = 0;
					int blendpal = (curpal + 1) % LevelData.Palette.Count;
					double A = fadeframe++ / (double)levelinfo.FadeLength;
					for (int y = 0; y < 4; y++)
						for (int x = 0; x < 16; x++)
						{
							Color oldcolor = LevelData.Palette[curpal][y, x].RGBColor;
							Color newcolor = LevelData.Palette[blendpal][y, x].RGBColor;
							LevelData.BmpPal.Entries[i++] = Color.FromArgb((int)(((1 - A) * oldcolor.R) + (A * newcolor.R)), (int)(((1 - A) * oldcolor.G) + (A * newcolor.G)), (int)(((1 - A) * oldcolor.B) + (A * newcolor.B)));
						}
					LevelData.BmpPal.Entries[0] = LevelData.BmpPal.Entries[0x20];
				}
			}
			Camera_X_pos += Camera_X_pos_diff;
			Camera_Y_pos += Camera_Y_pos_diff;
			BitmapBits bmp = new BitmapBits(levelimg);
			bmp.ScrollHorizontal((int)(Camera_X_pos * 0.75));
			BitmapBits tmp = new BitmapBits(layer1img);
			tmp.ScrollHorizontal((int)(Camera_X_pos * 0.5));
			for (int x = 0; x < bmp.Width; x += tmp.Width)
				bmp.DrawBitmapBehind(tmp, x, 0x1F0);
			tmp = new BitmapBits(layer2img);
			tmp.ScrollHorizontal((int)(Camera_X_pos * 0.625));
			for (int x = 0; x < bmp.Width; x += tmp.Width)
				bmp.DrawBitmapBehind(tmp, x, 0x200);
			if (Width < bmp.Width)
				bmp = bmp.GetSection(0, 0, Width, bmp.Height);
			bmp.ScrollVertical(Camera_Y_pos);
			if (Height < bmp.Height)
				bmp = bmp.GetSection(0, 0, bmp.Width, Height);
			bgimg = bmp.ToBitmap(LevelData.BmpPal);
		}

		public override void PlayMusic()
		{
			SonicBGScrollSaver.Music.PlaySong(levelinfo.Music);
		}
	}

	internal class LevelInfo
	{
		[System.ComponentModel.DefaultValue("MushroomHill1")]
		[IniName("music")]
		public string Music { get; set; }
		[System.ComponentModel.DefaultValue(30)]
		[IniName("fadelen")]
		public int FadeLength { get; set; }
		[System.ComponentModel.TypeConverter(typeof(SonicBGScrollSaver.CustomTimeSpanConverter))]
		[IniName("paltime")]
		public TimeSpan? PaletteTime { get; set; }
	}
}
