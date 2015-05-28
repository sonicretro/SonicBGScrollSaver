using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SonicBGScrollSaver
{
	static class Program
	{
		private static readonly bool isWindows = !(Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.Xbox);
		internal static bool IsWindows { get { return isWindows; } }
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			string arg1 = string.Empty;
			string arg2 = null;
			if (args.Length > 0)
			{
				if (args[0].Contains(":"))
				{
					string[] split = args[0].Split(':');
					arg1 = split[0];
					arg2 = split[1];
				}
				else
				{
					arg1 = args[0];
					if (args.Length > 1)
						arg2 = args[1];
				}
			}
			switch (arg1.ToLowerInvariant())
			{
				case "/p":
					if (arg2 == null) goto case "/s";
					IntPtr previewWndHandle = new IntPtr(long.Parse(arg2));
					Application.Run(new MainForm(previewWndHandle));
					break;
				case "/s":
					Application.Run(new MainForm());
					break;
				default:
					Application.Run(new ConfigDialog());
					break;
			}
		}
	}
}
