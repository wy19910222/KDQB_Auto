/*
 * @Author: wangyun
 * @CreateTime: 2023-09-08 01:03:24 799
 * @LastEditor: wangyun
 * @EditTime: 2023-09-08 01:03:24 803
 */

using System.Drawing;
using System.Drawing.Imaging;
using UnityEditor;
using UnityEngine;

using Graphics = System.Drawing.Graphics;

public static class ScreenshotUtils {
	[MenuItem("Assets/Screenshot", priority = -1)]
	private static void Screenshot() {
		string filename = Application.dataPath + "/screenshot.png";
		Screenshot(222, 207, 1, 1, filename);
	}
	
	public static void Screenshot(int x, int y, int width, int height, string filePath) {
		Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
		using (Graphics graphics = Graphics.FromImage(bitmap)) {
			graphics.CopyFromScreen(x, y, 0, 0, bitmap.Size);
		}
		bitmap.Save(filePath, ImageFormat.Png);
	}
	
	private static readonly Bitmap s_Bitmap = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
	public static Color32 GetColorOnScreen(int x, int y) {
		using (Graphics graphics = Graphics.FromImage(s_Bitmap)) {
			graphics.CopyFromScreen(x, y, 0, 0, s_Bitmap.Size);
		}
		System.Drawing.Color c = s_Bitmap.GetPixel(0, 0);
		return new Color32(c.R, c.G, c.B, c.A);
	}
}
