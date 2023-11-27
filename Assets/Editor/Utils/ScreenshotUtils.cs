/*
 * @Author: wangyun
 * @CreateTime: 2023-09-08 01:03:24 799
 * @LastEditor: wangyun
 * @EditTime: 2023-09-08 01:03:24 803
 */

using System.Drawing;
using System.Drawing.Imaging;
using UnityEngine;
using Graphics = System.Drawing.Graphics;

public static class ScreenshotUtils {
	public static void Screenshot(int x, int y, int width, int height, string filePath) {
		Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
		using (Graphics graphics = Graphics.FromImage(bitmap)) {
			graphics.CopyFromScreen(x, y, 0, 0, bitmap.Size);
		}
		bitmap.Save(filePath, ImageFormat.Png);
	}
	
	private static readonly Bitmap s_Bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
	public static Color32 GetColorOnScreen(int x, int y) {
		using Graphics graphics = Graphics.FromImage(s_Bitmap);
		graphics.CopyFromScreen(x, y, 0, 0, s_Bitmap.Size);
		System.Drawing.Color c = s_Bitmap.GetPixel(0, 0);
		return new Color32(c.R, c.G, c.B, c.A);
	}
	
	public static Color32[,] GetColorsOnScreen(int x, int y, int width, int height, int stride = 1) {
		using Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.CopyFromScreen(x, y, 0, 0, bitmap.Size);
		
		byte[] pixelsData = GetPixelsDataFromBitmap(bitmap);
		
		int finalWidth = width / stride;
		int finalHeight = height / stride;
		Color32[,] colors = new Color32[finalWidth, finalHeight];
		int stride1 = stride * Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
		int stride2 = width * stride1;
		for (int _y = 0, offset0 = 0; _y < finalHeight; ++_y, offset0 += stride2) {
			for (int _x = 0, offset = offset0; _x < finalWidth; ++_x, offset += stride1) {
				colors[_x, _y] = new Color32(pixelsData[offset + 2], pixelsData[offset + 1], pixelsData[offset], 255);
			}
		}
		return colors;
	}
	
	public static byte[] GetPixelsDataOnScreen(int x, int y, int width, int height) {
		using Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.CopyFromScreen(x, y, 0, 0, bitmap.Size);
		return GetPixelsDataFromBitmap(bitmap);
	}
	
	public static Color32[,] GetFromFile(string filePath) {
		using Bitmap bitmap = new Bitmap(Application.dataPath + "/" + filePath);
		byte[] pixelData = GetPixelsDataFromBitmap(bitmap);
		int width = bitmap.Width;
		int height = bitmap.Height;
		Color32[,] colors = new Color32[width, height];
		int bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
		for (int _y = 0; _y < height; ++_y) {
			for (int _x = 0; _x < width; ++_x) {
				int offset = (_y * width + _x) * bytesPerPixel;
				byte b = pixelData[offset];
				byte g = pixelData[offset + 1];
				byte r = pixelData[offset + 2];
				colors[_x, _y] = new Color32(r, g, b, 255);
			}
		}
		return colors;
	}
	
	private static byte[] GetPixelsDataFromBitmap(Bitmap bitmap) {
		BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
		byte[] pixelsData = new byte[bitmap.Height * bmpData.Stride];
		System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, pixelsData, 0, pixelsData.Length);
		bitmap.UnlockBits(bmpData);
		return pixelsData;
	}
}
