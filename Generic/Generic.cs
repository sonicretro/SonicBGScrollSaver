using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SonicRetro.SonLVL.API;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Generic
{
	public class Generic : SonicBGScrollSaver.Level
	{
		int[] Horiz_Scroll_Buf;
		int Camera_X_pos, Camera_Y_pos;
		BitmapBits levelimg;
		Bitmap bgimg = new Bitmap(1, 1);
		int Width, Height;
		LevelInfo levelinfo;
		double[] hscrollspeeds;

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			levelinfo = IniSerializer.Deserialize<LevelInfo>("setup.ini");
			hscrollspeeds = new double[levelinfo.HScrollSpeeds.Count];
			double lastval = 0;
			for (int i = 0; i < levelinfo.HScrollSpeeds.Count; i++)
				if (levelinfo.HScrollSpeeds[i].HasValue)
					lastval = hscrollspeeds[i] = levelinfo.HScrollSpeeds[i].Value;
				else
					hscrollspeeds[i] = lastval;
			if (string.IsNullOrEmpty(levelinfo.Image))
			{
				LevelData.LoadGame("./setup.ini");
				LevelData.LoadLevel("Level", true);
				levelimg = LevelData.DrawBackground(null, true, true, false, false);
			}
			else
				using (Bitmap tmp = new Bitmap(levelinfo.Image))
				{
					LevelData.BmpPal = tmp.Palette;
					levelimg = new BitmapBits(tmp);
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
				Camera_Y_pos += Camera_Y_pos_diff;
				BitmapBits bmp = new BitmapBits(levelimg);
				bmp.ScrollVertical(Camera_Y_pos);
				if (Height < bmp.Height)
					bmp = bmp.GetSection(0, 0, bmp.Width, Height);
				for (int i = 0; i < Horiz_Scroll_Buf.Length; i++)
					Horiz_Scroll_Buf[i] = (int)(Camera_X_pos * hscrollspeeds[(i + Camera_Y_pos) % hscrollspeeds.Length]);
				bmp.ScrollHorizontal((int[])Horiz_Scroll_Buf.Clone());
				if (Width < bmp.Width)
					bmp = bmp.GetSection(0, 0, Width, bmp.Height);
				bgimg = bmp.ToBitmap(LevelData.BmpPal);
			}
		}

		public override void PlayMusic()
		{
			if (!string.IsNullOrEmpty(levelinfo.Music))
				SonicBGScrollSaver.Music.PlaySong(levelinfo.Music);
		}
	}

	internal class LevelInfo
	{
		[IniName("music")]
		public string Music { get; set; }
		[IniName("image")]
		public string Image { get; set; }
		[IniName("hscroll")]
		[IniCollection(IniCollectionMode.NoSquareBrackets)]
		public List<double?> HScrollSpeeds { get; set; }
	}
}
