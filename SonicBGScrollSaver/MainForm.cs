using SonicRetro.SonLVL.API;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SonicBGScrollSaver
{
	public partial class MainForm : Form
	{
		bool previewMode;

		public MainForm()
		{
			InitializeComponent();
			foreach (Screen scr in Screen.AllScreens)
				Bounds = Rectangle.Union(Bounds, scr.Bounds);
			Cursor.Hide();
		}

		[DllImport("user32.dll")]
		static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

		public MainForm(IntPtr previewWndHandle)
		{
			InitializeComponent();
			SetParent(Handle, previewWndHandle);
			ManagedWinapi.Windows.SystemWindow wnd = new ManagedWinapi.Windows.SystemWindow(this);
			wnd.Style |= ManagedWinapi.Windows.WindowStyleFlags.CHILD;
			wnd = new ManagedWinapi.Windows.SystemWindow(previewWndHandle);
			Location = new Point();
			Size = wnd.Size;
			previewMode = true;
		}

		private static readonly System.Timers.Timer FrameTimer = new System.Timers.Timer() { AutoReset = true };
		private static readonly System.Timers.Timer SwitchTimer = new System.Timers.Timer() { AutoReset = true };
		Graphics gfx;
		Level level;
		short hscrollspeed = 8, vscrollspeed;
		Settings settings;
		List<KeyValuePair<string, Level>> levels = new List<KeyValuePair<string, Level>>();
		int currentlevel = -1;
		bool playMusic;

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
			}
			playMusic = settings.PlayMusic;
			FrameTimer.Interval = 1 / (double)Math.Max(Math.Min((int)settings.FramesPerSecond, 60), 1) * 1000;
			hscrollspeed = settings.ScrollSpeed;
			SwitchTimer.Interval = settings.DisplayTime.TotalMilliseconds;
			foreach (string item in settings.Levels)
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
				MessageBox.Show("Hey, you don't have any levels selected! Try the Configure option.", "Sonic Background Scrolling Screensaver", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Close();
				return;
			}
			FrameTimer.Elapsed += new System.Timers.ElapsedEventHandler(FrameTimer_Elapsed);
			SwitchTimer.Elapsed += new System.Timers.ElapsedEventHandler(SwitchTimer_Elapsed);
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
			level.Init(Width, Height);
			BackColor = LevelData.Palette[0][2, 0].RGBColor;
			DrawInvoker = () => BackgroundImage = level.GetBG();
			if (!previewMode && playMusic)
				level.PlayMusic();
			FrameTimer.Start();
			if (levels.Count > 1)
				SwitchTimer.Start();
		}

		void SwitchTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			level = null;
			FrameTimer.Stop();
			SwitchTimer.Stop();
			ChangeLevel();
		}

		Action DrawInvoker;
		void FrameTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (level == null) return;
			level.UpdatePalette();
			level.UpdateAnimatedTiles();
			level.UpdateScrolling(hscrollspeed, vscrollspeed);
			vscrollspeed = 0;
			Invoke(DrawInvoker);
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
				default:
					level = null;
					FrameTimer.Stop();
					Close();
					break;
			}
		}

		Point? lastmouse;
		private void MainForm_MouseMove(object sender, MouseEventArgs e)
		{
			if (lastmouse.HasValue)
				if (Math.Sqrt(Math.Pow(e.X - lastmouse.Value.X, 2) + Math.Pow(e.Y - lastmouse.Value.Y, 2)) > 5)
				{
					level = null;
					FrameTimer.Stop();
					Close();
				}
			lastmouse = e.Location;
		}
	}
}