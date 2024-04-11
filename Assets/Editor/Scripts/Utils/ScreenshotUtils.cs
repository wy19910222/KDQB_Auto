/*
 * @Author: wangyun
 * @CreateTime: 2023-09-08 01:03:24 799
 * @LastEditor: wangyun
 * @EditTime: 2023-09-08 01:03:24 803
 */

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using UnityEngine;

using Graphics = System.Drawing.Graphics;

public static class ScreenshotUtils {
	private static readonly Bitmap s_Bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
	public static Color32 GetColorOnScreen(int x, int y) {
		using Graphics graphics = Graphics.FromImage(s_Bitmap);
		graphics.CopyFromScreen(x, y, 0, 0, s_Bitmap.Size);
		System.Drawing.Color c = s_Bitmap.GetPixel(0, 0);
		return new Color32(c.R, c.G, c.B, c.A);
	}
	
	public static Bitmap Screenshot(int x, int y, int width, int height) {
		Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.CopyFromScreen(x, y, 0, 0, bitmap.Size);
		return bitmap;
	}
	
	public static void Screenshot(int x, int y, int width, int height, string filePath) {
		using Bitmap bitmap = Screenshot(x, y, width, height);
		bitmap.Save(filePath, ImageFormat.Png);
	}
	
	public static Color32[,] GetColorsOnScreen(int x, int y, int width, int height, int stride = 1) {
		using Bitmap bitmap = Screenshot(x, y, width, height);
		return Bitmap2Colors(bitmap, stride);
	}
	
	public static Color32[,] GetColorsFromFile(string filePath) {
		string path = Application.dataPath + "/" + filePath;
		if (!File.Exists(path)) {
			Debug.LogError($"Can not find file: {path}");
			return null;
		}
		using Bitmap bitmap = new Bitmap(path);
		return Bitmap2Colors(bitmap);
	}
	
	public static byte[] GetPixelsRGBOnScreen(int x, int y, int width, int height, int stride = 1) {
		using Bitmap bitmap = Screenshot(x, y, width, height);
		return Bitmap2PixelsRGB(bitmap, stride);
	}
	
	public static byte[] GetPixelsRGBFromFile(string filePath) {
		string path = Application.dataPath + "/" + filePath;
		if (!File.Exists(path)) {
			Debug.LogError($"Can not find file: {path}");
			return null;
		}
		using Bitmap bitmap = new Bitmap(path);
		return Bitmap2PixelsRGB(bitmap);
	}
	
	private static Color32[,] Bitmap2Colors(Bitmap bitmap, int stride = 1) {
		byte[] pixelsBGR = GetPixelsBGRFromBitmap(bitmap);
		
		int width = bitmap.Width;
		int height = bitmap.Height;
		int finalWidth = width / stride;
		int finalHeight = height / stride;
		Color32[,] colors = new Color32[finalWidth, finalHeight];
		int stride1 = stride * Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
		int stride2 = width * stride1;
		for (int _y = 0, offset0 = 0; _y < finalHeight; ++_y, offset0 += stride2) {
			for (int _x = 0, offset = offset0; _x < finalWidth; ++_x, offset += stride1) {
				colors[_x, _y] = new Color32(pixelsBGR[offset + 2], pixelsBGR[offset + 1], pixelsBGR[offset], 255);
			}
		}
		return colors;
	}
	
	private static byte[] Bitmap2PixelsRGB(Bitmap bitmap, int stride = 1) {
		byte[] pixelsBGR = GetPixelsBGRFromBitmap(bitmap);
		int bytesPerPixelSrc = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
		int bytesPerPixelDst = 3;
		
		if (stride > 1) {
			int width = bitmap.Width;
			int height = bitmap.Height;
			int finalWidth = width / stride;
			int finalHeight = height / stride;
			byte[] bytes = new byte[finalWidth * finalHeight * bytesPerPixelDst];
			int stride1Src = stride * bytesPerPixelSrc;
			int stride2Src = width * stride1Src;
			int stride1Dst = bytesPerPixelDst;
			int stride2Dst = finalWidth * stride1Src;
			for (int _y = 0, offset0Src = 0, offset0Dst = 0; _y < finalHeight; ++_y, offset0Src += stride2Src, offset0Dst += stride2Dst) {
				for (int _x = 0, offsetSrc = offset0Src, offsetDst = offset0Dst; _x < finalWidth; ++_x, offsetSrc += stride1Src, offsetDst += stride1Dst) {
					bytes[offsetDst] = pixelsBGR[offsetSrc + 2];	// r
					bytes[offsetDst + 1] = pixelsBGR[offsetSrc + 1];	// g
					bytes[offsetDst + 2] = pixelsBGR[offsetSrc];	// b
				}
			}
			return bytes;
		} else {
			int width = bitmap.Width;
			int height = bitmap.Height;
			byte[] bytes = new byte[width * height * bytesPerPixelDst];
			for (int _y = 0; _y < height; ++_y) {
				for (int _x = 0; _x < width; ++_x) {
					int index = _y * width + _x;
					int indexSrc = index * bytesPerPixelSrc;
					int indexDst = index * bytesPerPixelDst;
					bytes[indexDst] = pixelsBGR[indexSrc + 2];	// r
					bytes[indexDst + 1] = pixelsBGR[indexSrc + 1];	// g
					bytes[indexDst + 2] = pixelsBGR[indexSrc];	// b
				}
			}
			return bytes;
		}
	}
	
	private static byte[] GetPixelsBGRFromBitmap(Bitmap bitmap) {
		BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
		byte[] pixelsData = new byte[bitmap.Height * bmpData.Stride];
		System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, pixelsData, 0, pixelsData.Length);
		bitmap.UnlockBits(bmpData);
		return pixelsData;
	}
}
