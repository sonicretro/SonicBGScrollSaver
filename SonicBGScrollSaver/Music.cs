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
		#region Native Methods
		[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
		static extern bool InitializeDriver();
		[DllImport("SMPSOUT", ExactSpelling = true, EntryPoint="PlaySong", CharSet = CharSet.Auto)]
		static extern bool PlaySongNative(short song);
		[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
		internal static extern bool StopSong();
		[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
		internal static extern bool FadeOutSong();
		[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
		internal static extern bool PauseSong();
		[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
		internal static extern bool ResumeSong();
		[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
		internal static extern bool SetSongTempoSong(int pct);
		[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
		static unsafe extern IntPtr *GetCustomSongs(out uint count);
		[DllImport("SMPSOUT", ExactSpelling = true, CharSet = CharSet.Auto)]
		internal static extern void SetVolume(double volume);
		#endregion

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
			try { InitializeDriver(); }
			catch { return; }
			uint custcnt;
			IntPtr* p = GetCustomSongs(out custcnt);
			for (uint i = 0; i < custcnt; i++)
			{
				string song = Marshal.PtrToStringAnsi(*(p++));
				songNums.Add(song, songCount++);
			}
			initsuccess = true;
		}

		public static void PlaySong(string name)
		{
			if (!initsuccess || !songNums.ContainsKey(name)) return;
			PlaySongNative(songNums[name]);
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
