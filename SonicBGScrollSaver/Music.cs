using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using SonicRetro.SonLVL.API;

namespace SonicBGScrollSaver
{
	public static class Music
	{
		private static class NativeMethods
		{
			[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static extern bool InitializeDriver();
			[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static extern bool PlaySong(short song);
			[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static extern bool StopSong();
			[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static extern bool FadeOutSong();
			[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static extern bool PauseSong();
			[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static extern bool ResumeSong();
			[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static extern bool SetSongTempo(int pct);
			[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static unsafe extern IntPtr* GetCustomSongs(out uint count);
			[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static extern void SetVolume(double volume);
		}

		static bool initsuccess;
		static Dictionary<string, short> songNums = new Dictionary<string, short>(StringComparer.OrdinalIgnoreCase);

		internal static unsafe void Init()
		{
			if (initsuccess) return;
			if (!File.Exists("songs.ini"))
				return;
			Dictionary<string, Dictionary<string, string>> ini = IniFile.Load("songs.ini");
			if (File.Exists("songs_SKC.ini"))
				ini = IniFile.Combine(ini, IniFile.Load("songs_SKC.ini"));
			short songCount = 0;
			foreach (string song in IniSerializer.Deserialize<SongList>(ini).songs.Keys)
				songNums.Add(song, songCount++);
			string dir = Environment.CurrentDirectory;
			Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, "lib" + (IntPtr.Size == 8 ? "64" : "32"));
			try { NativeMethods.SetVolume(1); }
			catch
			{
				Environment.CurrentDirectory = dir;
				return;
			}
			Environment.CurrentDirectory = dir;
			NativeMethods.InitializeDriver();
			uint custcnt;
			IntPtr* p = NativeMethods.GetCustomSongs(out custcnt);
			for (uint i = 0; i < custcnt; i++)
			{
				string song = Marshal.PtrToStringAnsi(*(p++));
				songNums.Add(song, songCount++);
			}
			initsuccess = true;
		}

		public static void PlaySong(string name)
		{
			if (initsuccess && songNums.ContainsKey(name))
				NativeMethods.PlaySong(songNums[name]);
		}

		public static void StopSong()
		{
			if (initsuccess)
				NativeMethods.StopSong();
		}

		public static void SetVolume(double volume)
		{
			if (initsuccess)
				NativeMethods.SetVolume(volume);
		}
	}

	class SongList
	{
		public int residstart { get; set; }
		[IniCollection(IniCollectionMode.IndexOnly)]
		public Dictionary<string, SongInfo> songs { get; set; }
	}

	class SongInfo
	{
		public string Type { get; set; }
		public string Offset { get; set; }
		public string File { get; set; }
	}
}
