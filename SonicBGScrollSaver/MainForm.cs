using SonicRetro.SonLVL.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace SonicBGScrollSaver
{
	public partial class MainForm : Form
	{
		bool previewMode;
		Rectangle bounds;
		Size imageSize;
		double previewScale;

		public MainForm()
		{
			InitializeComponent();
			foreach (Screen scr in Screen.AllScreens)
				bounds = Rectangle.Union(bounds, scr.Bounds);
			imageSize = bounds.Size;
			Cursor.Hide();
		}

		private static class NativeMethods
		{
			[DllImport("user32.dll")]
			public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
		}

		public MainForm(IntPtr previewWndHandle)
		{
			InitializeComponent();
			NativeMethods.SetParent(Handle, previewWndHandle);
			ManagedWinapi.Windows.SystemWindow wnd = new ManagedWinapi.Windows.SystemWindow(this);
			wnd.Style |= ManagedWinapi.Windows.WindowStyleFlags.CHILD;
			wnd = new ManagedWinapi.Windows.SystemWindow(previewWndHandle);
			bounds.Size = wnd.Size;
			Size scrnSize = Screen.PrimaryScreen.Bounds.Size;
			if (scrnSize.Width > scrnSize.Height)
			{
				imageSize.Width = (int)(bounds.Width * ((double)scrnSize.Height / bounds.Height));
				imageSize.Height = scrnSize.Height;
				previewScale = (double)bounds.Height / scrnSize.Height;
			}
			else
			{
				imageSize.Width = scrnSize.Width;
				imageSize.Height = (int)(bounds.Height * ((double)scrnSize.Width / bounds.Width));
				previewScale = (double)bounds.Width / scrnSize.Width;
			}
			previewMode = true;
		}

		long frameTime;
		private static readonly System.Timers.Timer SwitchTimer = new System.Timers.Timer();
		Graphics gfx;
		Level level;
		short hscrollspeed = 8, vscrollspeed;
		Settings settings;
		List<KeyValuePair<string, Level>> levels = new List<KeyValuePair<string, Level>>();
		int currentlevel = -1;
		bool playMusic;
		Queue<DateTime> frametimes;

		private void Form1_Load(object sender, EventArgs e)
		{
			gfx = CreateGraphics();
			gfx.SetOptions();
			Environment.CurrentDirectory = Application.StartupPath;
			settings = Settings.Load();
			if (!previewMode)
			{
				Music.Init();
				if (settings.MusicVolume != 100)
					Music.SetVolume(settings.MusicVolume / 100d);
				fpsLabel.Visible = settings.FpsCounter;
			}
			playMusic = settings.PlayMusic;
			frameTime = (int)(1 / (double)Math.Max(Math.Min((int)settings.FramesPerSecond, 60), 1) * 1000);
			frametimes = new Queue<DateTime>(settings.FramesPerSecond * 5);
			hscrollspeed = settings.ScrollSpeed;
			SwitchTimer.Interval = settings.DisplayTime.TotalMilliseconds;
			foreach (string item in settings.Levels ?? System.Linq.Enumerable.Empty<string>())
			{
				if (!File.Exists(Path.Combine(item, "setup.ini")))
					continue;
				LevelInfo info = LevelInfo.Load(Path.Combine(item, "setup.ini"));
				string fullpath = Path.GetFullPath(item);
				try
				{
					Assembly assembly = Assembly.LoadFile(Path.Combine(fullpath, info.FileName));
					levels.Add(new KeyValuePair<string, Level>(fullpath, (Level)Activator.CreateInstance(assembly.GetType(info.Type))));
				}
				catch { continue; }
			}
			if (levels.Count == 0)
			{
				Cursor.Show();
				MessageBox.Show("Hey, you don't have any levels selected! Try the Configure option.", "Sonic Background Scrolling Screensaver", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
				return;
			}
			SwitchTimer.Elapsed += new System.Timers.ElapsedEventHandler(SwitchTimer_Elapsed);
			Bounds = bounds;
			ChangeLevel();
		}

		private void ChangeLevel()
		{
			if (settings.Shuffle)
			{
				Random random = new Random();
				int a;
				do { a = random.Next(levels.Count); }
				while (a == currentlevel);
				currentlevel = a;
			}
			else
				currentlevel = (currentlevel + 1) % levels.Count;
			Environment.CurrentDirectory = levels[currentlevel].Key;
			level = levels[currentlevel].Value;
			level.Init(imageSize.Width, imageSize.Height);
			if (previewMode)
				DrawInvoker = DrawBackgroundPreview;
			else
				DrawInvoker = DrawBackground;
			if (!previewMode && playMusic)
				level.PlayMusic();
			DrawThread = new Thread(DrawStuff);
			DrawThread.Start();
			if (levels.Count > 1)
				SwitchTimer.Start();
		}

		void DrawBackground()
		{
			BackgroundImage = level.GetBG();
			if (settings.FpsCounter)
			{
				DateTime now = DateTime.Now;
				if (frametimes.Count > 0)
					fpsLabel.Text = "FPS: " + (frametimes.Count / (now - frametimes.Peek()).TotalSeconds).ToString("0.###");
				if (frametimes.Count == settings.FramesPerSecond * 5)
					frametimes.Dequeue();
				frametimes.Enqueue(now);
			}
		}

		void DrawBackgroundPreview()
		{
			Bitmap tmp = level.GetBG();
			BackgroundImage = new Bitmap(tmp, (int)(tmp.Width * previewScale), (int)(tmp.Height * previewScale));
		}

		void SwitchTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			level = null;
			DrawThread.Abort();
			SwitchTimer.Stop();
			ChangeLevel();
		}

		Action DrawInvoker;
		Thread DrawThread;
		void DrawStuff()
		{
			Stopwatch sw = new Stopwatch();
			while (level != null)
			{
				sw.Start();
				level.UpdatePalette();
				level.UpdateAnimatedTiles();
				level.UpdateScrolling(hscrollspeed, vscrollspeed);
				vscrollspeed = 0;
				Invoke(DrawInvoker);
				while (sw.ElapsedMilliseconds < frameTime)
					Thread.Sleep(0);
				sw.Reset();
			}
		}

		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (previewMode || level == null) return;
			switch (e.KeyCode)
			{
				case Keys.Left:
					hscrollspeed--;
					break;
				case Keys.Right:
					hscrollspeed++;
					break;
				case Keys.Up:
					vscrollspeed = -4;
					break;
				case Keys.Down:
					vscrollspeed = 4;
					break;
				case Keys.W:
					level.ToggleWater();
					break;
				case Keys.M:
					if (playMusic)
					{
						Music.StopSong();
						playMusic = false;
					}
					else
					{
						level.PlayMusic();
						playMusic = true;
					}
					break;
				case Keys.N:
					if (levels.Count == 1)
						return;
					level = null;
					DrawThread.Abort();
					SwitchTimer.Stop();
					ChangeLevel();
					break;
				default:
					level = null;
					DrawThread.Abort();
					Close();
					break;
			}
		}

		Point? lastmouse;
		private void MainForm_MouseMove(object sender, MouseEventArgs e)
		{
			if (previewMode) return;
			if (lastmouse.HasValue)
				if (Math.Sqrt(Math.Pow(e.X - lastmouse.Value.X, 2) + Math.Pow(e.Y - lastmouse.Value.Y, 2)) > 5)
				{
					level = null;
					DrawThread.Abort();
					Close();
				}
			lastmouse = e.Location;
		}

		private void MainForm_MouseDown(object sender, MouseEventArgs e)
		{
			if (previewMode) return;
			level = null;
			DrawThread.Abort();
			Close();
		}
	}
}