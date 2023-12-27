/*
 * @Author: wangyun
 * @CreateTime: 2023-09-28 02:45:23 562
 * @LastEditor: wangyun
 * @EditTime: 2023-09-28 02:45:23 565
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public static class Operation {
	public static Rect BASED_GAME_RECT { get; } = new Rect(0, 101, 1920, 915);
	public static Rect CURRENT_GAME_RECT { get; set; } = new Rect(0, 101, 1920, 915);

	public static void Click(int x, int y) {
		x = Mathf.RoundToInt((x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y = Mathf.RoundToInt((y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
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
	
	public static void MouseMove(int x, int y) {
		x = Mathf.RoundToInt((x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y = Mathf.RoundToInt((y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		MouseUtils.SetMousePos(x, y);
	}

	public static IEnumerator Drag(int x1, int y1, int x2, int y2, float duration = 0.2F) {
		x1 = Mathf.RoundToInt((x1 - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y1 = Mathf.RoundToInt((y1 - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		x2 = Mathf.RoundToInt((x2 - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y2 = Mathf.RoundToInt((y2 - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		
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
		x1 = Mathf.RoundToInt((x1 - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y1 = Mathf.RoundToInt((y1 - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		x2 = Mathf.RoundToInt((x2 - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y2 = Mathf.RoundToInt((y2 - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		
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
		int centerX = Mathf.RoundToInt(CURRENT_GAME_RECT.x + CURRENT_GAME_RECT.width / 2);
		int centerY = Mathf.RoundToInt(CURRENT_GAME_RECT.y + CURRENT_GAME_RECT.height / 2);
		
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
		x = Mathf.RoundToInt((x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y = Mathf.RoundToInt((y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		return ScreenshotUtils.GetColorOnScreen(x, y);
	}

	public static Color32[,] GetColorsOnScreen(int x, int y, int width, int height, int stride = 1) {
		x = Mathf.RoundToInt((x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y = Mathf.RoundToInt((y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		width = Mathf.RoundToInt(width / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width);
		height = Mathf.RoundToInt(height / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height);
		return ScreenshotUtils.GetColorsOnScreen(x, y, width, height, stride);
	}

	public static byte[] GetPixelsDataOnScreen(int x, int y, int width, int height) {
		x = Mathf.RoundToInt((x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y = Mathf.RoundToInt((y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		width = Mathf.RoundToInt(width / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width);
		height = Mathf.RoundToInt(height / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height);
		return ScreenshotUtils.GetPixelsDataOnScreen(x, y, width, height);
	}
	
	public static Color32[,] GetFromFile(string filePath) {
		return ScreenshotUtils.GetFromFile(filePath);
	}
	
	public static void Screenshot(int x, int y, int width, int height, string filePath) {
		x = Mathf.RoundToInt((x - BASED_GAME_RECT.x) / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width + CURRENT_GAME_RECT.x);
		y = Mathf.RoundToInt((y - BASED_GAME_RECT.y) / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height + CURRENT_GAME_RECT.y);
		width = Mathf.RoundToInt(width / BASED_GAME_RECT.width * CURRENT_GAME_RECT.width);
		height = Mathf.RoundToInt(height / BASED_GAME_RECT.height * CURRENT_GAME_RECT.height);
		ScreenshotUtils.Screenshot(x, y, width, height, filePath);
	}
}
