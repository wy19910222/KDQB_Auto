/*
 * @Author: wangyun
 * @CreateTime: 2023-11-26 06:37:03 858
 * @LastEditor: wangyun
 * @EditTime: 2023-11-26 06:37:03 863
 */

using System.Collections;
using UnityEngine;
using UnityEditor;

public class ExtensionalScreen {
	public static int RANGE_X = 0;	// 截图范围
	public static int RANGE_Y = 0;	// 截图范围
	public static int RANGE_W = 1920;	// 截图范围
	public static int RANGE_H = 1080;	// 截图范围
	public static int STRIDE = 2;	// 间隔多少取一个像素
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	public static Texture2D Tex { get; } = new Texture2D(960, 540);

	[MenuItem("Tools_Task/StartExtensionalScreen", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"查看第三屏已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopExtensionalScreen", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("查看第三屏已关闭");
		}
	}

	private static IEnumerator Update() {
		Color32[] pixels = new Color32[Tex.width * Tex.height];
		while (true) {
			Color32[,] colors = ScreenshotUtils.GetColorsOnScreen(RANGE_X, RANGE_Y, RANGE_W, RANGE_H, STRIDE);
			int realWidth = RANGE_W / STRIDE;
			int realHeight = RANGE_H / STRIDE;
			if (!Tex || Tex.width != realWidth || Tex.height != realHeight) {
				Tex.Reinitialize(realWidth, realHeight);
				pixels = new Color32[realWidth * realHeight];
			}
			for (int y = 0, offset0 = (realHeight - 1) * realWidth; y < realHeight; ++y, offset0 -= realWidth) {
				for (int x = 0, offset = offset0; x < realWidth; ++x, ++offset) {
					pixels[offset] = colors[x, y];
				}
			}
			Tex.SetPixels32(pixels);
			Tex.Apply();
			yield return null;
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
