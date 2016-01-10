// NativeFuncs.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include <cstdint>

extern "C"
{
	__declspec(dllexport) void Dummy() { }

	__declspec(dllexport) void ScrollHV(uint8_t *src, int32_t srcW, int32_t srcH, uint8_t *dst, int32_t dstW, int32_t dstH, int32_t dstY, int32_t srcY, int32_t *srcX, int32_t srcXLen)
	{
		if (dstY < 0 || dstY >= dstH) return;
		for (int i = 0; i < srcXLen; i++)
		{
			srcX[i] %= srcW;
			if (srcX[i] < 0)
				srcX[i] += srcW;
		}
		srcY %= srcH;
		if (srcY < 0)
			srcY += srcH;
		int rowSrc = srcY * srcW;
		int rowDst = dstY * dstW;
		for (int y = 0; y < dstH - dstY; y++)
		{
			int amount = srcX[(srcY + y) % srcXLen];
			memcpy(&dst[rowDst], &src[rowSrc + amount], min(srcW - amount, dstW));
			if (amount != 0 && srcW - amount < dstW)
				memcpy(&dst[rowDst + srcW - amount], &src[rowSrc], min(amount, dstW - (srcW - amount)));
			rowSrc = (rowSrc + srcW) % (srcW * srcH);
			rowDst += dstW;
		}
	}
}
