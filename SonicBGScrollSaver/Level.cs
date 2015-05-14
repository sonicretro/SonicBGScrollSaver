using System.Drawing;

namespace SonicBGScrollSaver
{
	public abstract class Level
	{
		public abstract void Init(int width, int height);
		public abstract Bitmap GetBG();
		public abstract void UpdateScrolling(short Camera_X_pos_diff, short Camera_Y_pos_diff);
		public virtual void UpdatePalette() { }
		public virtual void UpdateAnimatedTiles() { }
		public virtual void PlayMusic() { }
	}
}
