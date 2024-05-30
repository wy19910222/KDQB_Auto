/*
 * @Author: wangyun
 * @CreateTime: 2023-09-28 02:45:23 562
 * @LastEditor: wangyun
 * @EditTime: 2023-09-28 02:45:23 565
 */

using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class Operation {
	public static RectInt BASED_GAME_RECT { get; } = new RectInt(0, 101, 1920, 915);
	public static RectInt CURRENT_GAME_RECT { get; set; } = new RectInt(0, 101, 1920, 915);
	
	private static int frameCount;
	private static Color32[,] colors;

	public static Vector2Int GetMousePos() {
		Vector2Int pos = MouseUtils.GetMousePos();
		pos.x = Mathf.RoundToInt(((float) pos.x - CURRENT_GAME_RECT.x) / CURRENT_GAME_RECT.width * BASED_GAME_RECT.width + BASED_GAME_RECT.x);
		pos.y = Mathf.RoundToInt(((float) pos.y - CURRENT_GAME_RECT.y) / CURRENT_GAME_RECT.height * BASED_GAME_RECT.height + BASED_GAME_RECT.y);
		return pos;
	}

	public static void Click(int x, int y) {
		x = Mathf.RoundToInt(((float) x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y = Mathf.RoundToInt(((float) y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		if (MouseUtils.IsLeftDown()) {
			// 确保鼠标不是在按下状态
			MouseUtils.LeftUp();
		}
		Vector2Int oldPos = MouseUtils.GetMousePos();
		MouseUtils.SetMousePos(x, y);
		MouseUtils.LeftDown();
		MouseUtils.LeftUp();
		MouseUtils.SetMousePos(oldPos.x, oldPos.y);
	}
	
	public static void SetMousePos(int x, int y) {
		x = Mathf.RoundToInt(((float) x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y = Mathf.RoundToInt(((float) y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		MouseUtils.SetMousePos(x, y);
	}

	public static IEnumerator Drag(int x1, int y1, int x2, int y2, float duration = 0.4F) {
		x1 = Mathf.RoundToInt(((float) x1 - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y1 = Mathf.RoundToInt(((float) y1 - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		x2 = Mathf.RoundToInt(((float) x2 - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y2 = Mathf.RoundToInt(((float) y2 - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		
		Vector2Int oldPos = MouseUtils.GetMousePos();
		
		MouseUtils.SetMousePos(x1, y1);
		MouseUtils.LeftDown();
		long startTime = DateTime.Now.Ticks;
		while (true) {
			yield return null;
			float percent = (DateTime.Now.Ticks - startTime) / (duration * 10000000);
			if (percent >= 1) {
				break;
			}
			MouseUtils.SetMousePos(Mathf.RoundToInt(Mathf.Lerp(x1, x2, percent)), Mathf.RoundToInt(Mathf.Lerp(y1, y2, percent)));	// 加入按钮
		}
		MouseUtils.SetMousePos(x2, y2);
		MouseUtils.LeftUp();
		
		MouseUtils.SetMousePos(oldPos.x, oldPos.y);
	}

	public static IEnumerator NoInertiaDrag(int x1, int y1, int x2, int y2, float duration = 0.2F) {
		x1 = Mathf.RoundToInt(((float) x1 - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y1 = Mathf.RoundToInt(((float) y1 - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		x2 = Mathf.RoundToInt(((float) x2 - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y2 = Mathf.RoundToInt(((float) y2 - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		
		Vector2Int oldPos = MouseUtils.GetMousePos();
		
		MouseUtils.SetMousePos(x1, y1);
		MouseUtils.LeftDown();
		long startTime = DateTime.Now.Ticks;
		while (true) {
			yield return null;
			float percent = (DateTime.Now.Ticks - startTime) / (duration * 10000000);
			if (percent >= 1) {
				break;
			}
			percent = 1 - Mathf.Pow(1 - percent, 2);
			MouseUtils.SetMousePos(Mathf.RoundToInt(Mathf.Lerp(x1, x2, percent)), Mathf.RoundToInt(Mathf.Lerp(y1, y2, percent)));	// 加入按钮
		}
		for (int i = 0; i < 5; ++i) {
			MouseUtils.SetMousePos(x2, y2);
			yield return null;
		}
		MouseUtils.LeftUp();
		
		MouseUtils.SetMousePos(oldPos.x, oldPos.y);
	}

	public static IEnumerator Zoom(int value) {
		int centerX = Mathf.RoundToInt(CURRENT_GAME_RECT.x + (float) CURRENT_GAME_RECT.width / 2);
		int centerY = Mathf.RoundToInt(CURRENT_GAME_RECT.y + (float) CURRENT_GAME_RECT.height / 2);
		
		Vector2Int oldPos = MouseUtils.GetMousePos();
		MouseUtils.SetMousePos(centerX, centerY);	// 鼠标移动到屏幕中央
		int absValue = Mathf.Abs(value);
		int direction = value / absValue;
		for (int i = 0; i < absValue; ++i) {
			MouseUtils.ScrollWheel(direction);
			yield return new EditorWaitForSeconds(0.02F);
		}
		MouseUtils.SetMousePos(oldPos.x, oldPos.y);
	}

	public static Color32 GetColorOnScreen(int x, int y) {
		if (frameCount != EditorCoroutineManager.FrameCount || colors == null) {
			frameCount = EditorCoroutineManager.FrameCount;
			colors = ScreenshotUtils.GetColorsOnScreen(CURRENT_GAME_RECT.x, CURRENT_GAME_RECT.y, CURRENT_GAME_RECT.width, CURRENT_GAME_RECT.height);
		}
		if (BASED_GAME_RECT.Contains(new Vector2Int(x, y))) {
			x = Mathf.RoundToInt(((float) x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width);
			y = Mathf.RoundToInt(((float) y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height);
			return colors[x, y];
		} else {
			x = Mathf.RoundToInt(((float) x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width) + CURRENT_GAME_RECT.x;
			y = Mathf.RoundToInt(((float) y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height) + CURRENT_GAME_RECT.y;
			return ScreenshotUtils.GetColorOnScreen(x, y);
		}
	}

	public static Color32[,] GetColorsOnScreen(int x, int y, int width, int height, int stride = 1) {
		if (frameCount != EditorCoroutineManager.FrameCount || colors == null) {
			frameCount = EditorCoroutineManager.FrameCount;
			colors = ScreenshotUtils.GetColorsOnScreen(CURRENT_GAME_RECT.x, CURRENT_GAME_RECT.y, CURRENT_GAME_RECT.width, CURRENT_GAME_RECT.height);
		}
		if (stride == 1 && BASED_GAME_RECT.Contains(new Vector2Int(x, y)) && BASED_GAME_RECT.Contains(new Vector2Int(x + width, y + height))) {
			x = Mathf.RoundToInt(((float) x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width);
			y = Mathf.RoundToInt(((float) y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height);
			width = Mathf.RoundToInt((float) width / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width);
			height = Mathf.RoundToInt((float) height / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height);
			Color32[,] ret = new Color32[width, height];
			for (int _x = 0; _x < width; ++_x) {
				for (int _y = 0; _y < height; ++_y) {
					ret[_x, _y] = colors[x + _x, y + _y];
				}
			}
			return ret;
		} else {
			x = Mathf.RoundToInt(((float) x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
			y = Mathf.RoundToInt(((float) y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
			width = Mathf.RoundToInt((float) width / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width);
			height = Mathf.RoundToInt((float) height / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height);
			return ScreenshotUtils.GetColorsOnScreen(x, y, width, height, stride);
		}
	}

	public static string GetTextOnScreen(int x, int y, int width, int height) {
		x = Mathf.RoundToInt(((float) x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y = Mathf.RoundToInt(((float) y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		width = Mathf.RoundToInt((float) width / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width);
		height = Mathf.RoundToInt((float) height / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height);
		byte[] bytes = ScreenshotUtils.GetPixelsRGBOnScreen(x, y, width, height);
		return OCRUtils.Recognize(width, height, bytes);
	}
	
	public static Color32[,] GetFromFile(string filePath) {
		return ScreenshotUtils.GetColorsFromFile(filePath);
	}
	
	public static void Screenshot(int x, int y, int width, int height, string filePath) {
		x = Mathf.RoundToInt(((float) x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y = Mathf.RoundToInt(((float) y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		width = Mathf.RoundToInt((float) width / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width);
		height = Mathf.RoundToInt((float) height / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height);
		filePath = Application.dataPath + "/" + filePath.Replace("\\", "/");
		int slashIndex = filePath.LastIndexOf("/");
		if (slashIndex != -1) {
			string directoryPath = filePath.Substring(0, slashIndex);
			if (!Directory.Exists(directoryPath)) {
				Directory.CreateDirectory(directoryPath);
			}
		}
		ScreenshotUtils.Screenshot(x, y, width, height, filePath);
	}
	
	public static System.Drawing.Bitmap Screenshot(int x, int y, int width, int height) {
		x = Mathf.RoundToInt(((float) x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y = Mathf.RoundToInt(((float) y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		width = Mathf.RoundToInt((float) width / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width);
		height = Mathf.RoundToInt((float) height / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height);
		return ScreenshotUtils.Screenshot(x, y, width, height);
	}
}
