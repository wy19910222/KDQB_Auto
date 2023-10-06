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
	public static void Click(int x, int y) {
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
		MouseUtils.SetMousePos(x, y);
	}

	public static IEnumerator Drag(int x1, int y1, int x2, int y2, float duration = 0.2F) {
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
		Vector2Int oldPos = MouseUtils.GetMousePos();
		
		MouseUtils.SetMousePos(x1, y1);
		MouseUtils.LeftDown();
		float duration1 = Mathf.Max(duration - 0.1F, 0.1F);
		long startTime = DateTime.Now.Ticks;
		while (true) {
			yield return null;
			float percent = (DateTime.Now.Ticks - startTime) / (duration1 * 10000000);
			if (percent >= 1) {
				break;
			}
			percent = 1 - Mathf.Pow(1 - percent, 2);
			MouseUtils.SetMousePos(Mathf.RoundToInt(Mathf.Lerp(x1, x2, percent)), Mathf.RoundToInt(Mathf.Lerp(y1, y2, percent)));	// 加入按钮
		}
		MouseUtils.SetMousePos(x2, y2);
		yield return new EditorWaitForSeconds(0.1F);
		MouseUtils.LeftUp();
		
		MouseUtils.SetMousePos(oldPos.x, oldPos.y);
	}

	public static IEnumerator Zoom(int value) {
		Vector2Int oldPos = MouseUtils.GetMousePos();
		MouseUtils.SetMousePos(960, 540);	// 鼠标移动到屏幕中央
		int absValue = Mathf.Abs(value);
		int direction = value / absValue;
		for (int i = 0; i < absValue; ++i) {
			MouseUtils.ScrollWheel(direction);
			yield return new EditorWaitForSeconds(0.02F);
		}
		MouseUtils.SetMousePos(oldPos.x, oldPos.y);
	}
}
