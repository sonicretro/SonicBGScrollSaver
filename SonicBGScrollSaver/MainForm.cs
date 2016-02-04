using SharpDX.Direct2D1;
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
using D2DBitmap = SharpDX.Direct2D1.Bitmap;
using GDIpBitmap = System.Drawing.Bitmap;
using System.Drawing.Imaging;

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
			[DllImport("winmm.dll", ExactSpelling = true)]
			public static extern uint timeBeginPeriod(uint uPeriod);
			[DllImport("winmm.dll", ExactSpelling = true)]
			public static extern uint timeEndPeriod(uint uPeriod);
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
		readonly System.Timers.Timer FrameTimer = new System.Timers.Timer() { AutoReset = true };
		readonly System.Timers.Timer SwitchTimer = new System.Timers.Timer();
		Level level;
		short hscrollspeed = 8, vscrollspeed;
		Settings settings;
		List<KeyValuePair<string, Level>> levels = new List<KeyValuePair<string, Level>>();
		int currentlevel = -1;
		bool playMusic;
		Queue<DateTime> frametimes;
		Factory d2dfactory = new Factory();
		WindowRenderTarget d2drendertarget;
		static SharpDX.Direct2D1.PixelFormat pixelformat = new SharpDX.Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Ignore);
		SharpDX.DirectWrite.Factory dwfactory = new SharpDX.DirectWrite.Factory();
		SharpDX.DirectWrite.TextFormat textformat;
		SolidColorBrush textbrush;

		private void Form1_Load(object sender, EventArgs e)
		{
			HwndRenderTargetProperties rtp = new HwndRenderTargetProperties()
				{
					Hwnd = Handle,
					PixelSize = new SharpDX.Size2(bounds.Width, bounds.Height),
				};
			d2drendertarget = new WindowRenderTarget(d2dfactory, new RenderTargetProperties(pixelformat), rtp);
			textformat = new SharpDX.DirectWrite.TextFormat(dwfactory, Font.FontFamily.Name, (Font.SizeInPoints / 72) * 96);
			textbrush = new SolidColorBrush(d2drendertarget, new SharpDX.Color4(1, 1, 0, 1));
			Environment.CurrentDirectory = Application.StartupPath;
			settings = Settings.Load();
			if (!previewMode)
			{
				Music.Init();
				if (settings.MusicVolume != 100)
					Music.SetVolume(settings.MusicVolume / 100d);
			}
			playMusic = settings.PlayMusic;
			if (Program.IsWindows)
				frameTime = (int)(1 / (double)Math.Max(Math.Min((int)settings.FramesPerSecond, 60), 1) * 1000);
			else
			{
				FrameTimer.Interval = 1 / (double)Math.Max(Math.Min((int)settings.FramesPerSecond, 60), 1) * 1000;
				FrameTimer.Elapsed += FrameTimer_Elapsed;
			}
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
			if (Program.IsWindows)
				NativeMethods.timeBeginPeriod(1);
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
			if (Program.IsWindows)
			{
				DrawThread = new Thread(DrawStuff);
				DrawThread.Start();
			}
			else
				FrameTimer.Start();
			if (levels.Count > 1)
				SwitchTimer.Start();
		}

		BitmapProperties bmpprops = new BitmapProperties(pixelformat);
		BitmapBrushProperties brushprops = new BitmapBrushProperties()
		{
			ExtendModeX = ExtendMode.Wrap,
			ExtendModeY = ExtendMode.Wrap,
			InterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor
		};
		void DrawBackground()
		{
			d2drendertarget.BeginDraw();
			GDIpBitmap bmp = level.GetBG().To32bpp();
			BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (D2DBitmap d2dbmp = new D2DBitmap(d2drendertarget, new SharpDX.Size2(bmp.Width, bmp.Height), bmpprops))
			{
				d2dbmp.CopyFromMemory(bd.Scan0, bd.Stride);
				bmp.UnlockBits(bd);
				using (BitmapBrush bb = new BitmapBrush(d2drendertarget, d2dbmp, brushprops))
					d2drendertarget.FillRectangle(new SharpDX.RectangleF(0, 0, Width, Height), bb);
			}
			if (settings.FpsCounter)
			{
				DateTime now = DateTime.Now;
				if (frametimes.Count > 0)
					d2drendertarget.DrawText("FPS: " + (frametimes.Count / (now - frametimes.Peek()).TotalSeconds).ToString("0.###"), textformat, new SharpDX.RectangleF(10, 10, 1000, 0), textbrush);
				if (frametimes.Count == settings.FramesPerSecond * 5)
					frametimes.Dequeue();
				frametimes.Enqueue(now);
			}
			d2drendertarget.EndDraw();
		}

		void DrawBackgroundPreview()
		{
			System.Drawing.Bitmap tmp = level.GetBG();
			BackgroundImage = new System.Drawing.Bitmap(tmp, (int)(tmp.Width * previewScale), (int)(tmp.Height * previewScale));
		}

		void SwitchTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			StopDrawing();
			SwitchTimer.Stop();
			ChangeLevel();
		}

		private void StopDrawing()
		{
			level = null;
			if (Program.IsWindows)
				DrawThread.Abort();
			else
				FrameTimer.Stop();
		}

		Action DrawInvoker;
		Thread DrawThread;
		void DrawStuff()
		{
			Stopwatch sw = new Stopwatch();
			while (level != null)
			{
				sw.Start();
				DoFrame();
				while (sw.ElapsedMilliseconds < frameTime)
					Thread.Sleep((int)Math.Max(0, frameTime - sw.ElapsedMilliseconds - 1));
				sw.Reset();
			}
		}

		private void DoFrame()
		{
			level.UpdatePalette();
			level.UpdateAnimatedTiles();
			level.UpdateScrolling(hscrollspeed, vscrollspeed);
			vscrollspeed = 0;
			DrawInvoker();
		}

		void FrameTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			DoFrame();
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
					StopDrawing();
					SwitchTimer.Stop();
					ChangeLevel();
					break;
				default:
					StopDrawing();
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
					StopDrawing();
					Close();
				}
			lastmouse = e.Location;
		}

		private void MainForm_MouseDown(object sender, MouseEventArgs e)
		{
			if (previewMode) return;
			StopDrawing();
			Close();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			StopDrawing();
		}
	}
}