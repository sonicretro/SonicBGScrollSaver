using SonicRetro.SonLVL.API;
using System.Drawing;

namespace MHZ
{
    public class MHZ : SonicBGScrollSaver.Level
    {
		int Camera_X_pos, Camera_Y_pos;
		BitmapBits levelimg, layer1img, layer2img;
		Bitmap bgimg = new Bitmap(1, 1);
		int Width, Height;

		public override void Init(int width, int height)
		{
			Width = width;
			Height = height;
			LevelData.LoadGame("./setup.ini");
			LevelData.LoadLevel("Level", true);
			levelimg = LevelData.DrawBackground(null, true, true, false, false);
			layer1img = new BitmapBits("../layer1.png");
			layer1img.IncrementIndexes(0x30);
			layer2img = new BitmapBits("../layer2.png");
			layer2img.IncrementIndexes(0x30);
			Camera_Y_pos = 0;
			if (height < levelimg.Height)
				Camera_Y_pos += (short)((levelimg.Height / 2) - (height / 2));
			Camera_X_pos = 0;
			UpdateScrolling(0, 0);
		}

		public override System.Drawing.Bitmap GetBG()
		{
			return bgimg;
		}

		public override void UpdateScrolling(short Camera_X_pos_diff, short Camera_Y_pos_diff)
		{
			Camera_X_pos += Camera_X_pos_diff;
			Camera_Y_pos += Camera_Y_pos_diff;
			BitmapBits bmp = new BitmapBits(levelimg.Width, levelimg.Height);
			for (int x = (int)(-Camera_X_pos * 0.5) % layer1img.Width; x < bmp.Width; x += layer1img.Width)
				bmp.DrawBitmapBounded(layer1img, x, 0x1F0);
			for (int x = (int)(-Camera_X_pos * 0.625) % layer2img.Width; x < bmp.Width; x += layer2img.Width)
				bmp.DrawBitmapBounded(layer2img, x, 0x200);
			BitmapBits tmp = new BitmapBits(levelimg);
			tmp.ScrollHorizontal((int)(Camera_X_pos * 0.75));
			bmp.DrawBitmapComposited(tmp, 0, 0);
			if (Width < bmp.Width)
				bmp = bmp.GetSection(0, 0, Width, bmp.Height);
			bmp.ScrollVertical(Camera_Y_pos);
			if (Height < bmp.Height)
				bmp = bmp.GetSection(0, 0, bmp.Width, Height);
			bgimg = bmp.ToBitmap(LevelData.BmpPal);
		}

		public override void PlayMusic()
		{
			SonicBGScrollSaver.Music.PlaySong(IniSerializer.Deserialize<MusicInfo>("setup.ini").Music);
		}
	}

	internal class MusicInfo
	{
		[System.ComponentModel.DefaultValue("MysticCave2P")]
		[IniName("music")]
		public string Music { get; set; }
	}
}
