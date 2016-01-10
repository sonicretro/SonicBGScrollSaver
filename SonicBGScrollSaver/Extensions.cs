using SonicRetro.SonLVL.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SonicBGScrollSaver
{
	public static class NativeFuncs
	{
		private static class NativeMethods
		{
			[DllImport("NativeFuncs", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static extern void Dummy();
			[DllImport("NativeFuncs", ExactSpelling = true, CharSet = CharSet.Auto)]
			public static unsafe extern void ScrollHV(byte* src, int srcW, int srcH, byte* dst, int dstW, int dstH, int dstY, int srcY, int* srcX, int srcXLen);
		}

		static bool initsuccess;

		public static void Init()
		{
			if (initsuccess) return;
			string dir = Environment.CurrentDirectory;
			Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, "lib" + (IntPtr.Size == 8 ? "64" : "32"));
			try { NativeMethods.Dummy(); }
			catch
			{
				Environment.CurrentDirectory = dir;
				return;
			}
			Environment.CurrentDirectory = dir;
			initsuccess = true;
		}

		public static unsafe void ScrollHV(BitmapBits source, BitmapBits destination, int dstY, int srcY, params int[] srcX)
		{
			if (!initsuccess)
				source.ScrollHV(destination, dstY, srcY, srcX);
			else
				fixed (byte* src = source.Bits)
				fixed (byte* dst = destination.Bits)
				fixed (int* sx = srcX)
					NativeMethods.ScrollHV(src, source.Width, source.Height, dst, destination.Width, destination.Height, dstY, srcY, sx, srcX.Length);
		}
	}
}
