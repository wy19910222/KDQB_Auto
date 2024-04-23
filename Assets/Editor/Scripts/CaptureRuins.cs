/*
 * @Author: wangyun
 * @CreateTime: 2023-10-06 12:16:55 107
 * @LastEditor: wangyun
 * @EditTime: 2023-10-06 12:16:55 112
 */

using System.IO;
using System.Collections;
using UnityEngine;
using UnityEditor;

public class CaptureRuins {
	public static bool CAPTURE_NORTH = true;
	public static bool CAPTURE_SOUTH = true;
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;
	
	[MenuItem("Tools_Task/StartCaptureRuins", priority = -1)]
	private static void Enable() {
		Disable();
		s_CO = EditorCoroutineManager.StartCoroutine(IECapture());
		Debug.Log("开始进行王者遗迹截图");
	}

	[MenuItem("Tools_Task/StopCaptureRuins", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("取消王者遗迹截图");
		}
	}

	private static IEnumerator IECapture() {
		while (Task.CurrentTask != null) {
			yield return null;
		}
		Task.CurrentTask = nameof(CaptureRuins);
		if (CAPTURE_NORTH) {
			// 北边遗迹截图
			var ie = IEDoCapture(new Vector2Int(1064, 492), new Vector2Int(1040, 670), new Vector2Int(1100, 600), "NorthRuins");
			while (ie.MoveNext()) {
				yield return ie.Current;
			}
		}
		if (CAPTURE_SOUTH) {
			// 南边遗迹截图
			var ie = IEDoCapture(new Vector2Int(856, 624), new Vector2Int(1040, 670), new Vector2Int(1100, 460), "SouthRuins");
			while (ie.MoveNext()) {
				yield return ie.Current;
			}
		}
		Task.CurrentTask = null;
		s_CO = null;
	}

	private static IEnumerator IEDoCapture(Vector2Int ruinsIconPos, Vector2Int gotoBtnPos, Vector2Int offset, string filename) {
		if (Recognize.CurrentScene != Recognize.Scene.OUTSIDE_NEARBY && Recognize.CurrentScene != Recognize.Scene.OUTSIDE_FARAWAY) {
			yield break;
		}
		{
			// 先拉远，保证跳转遗迹后会重置视距
			var ie = Operation.Zoom(-20);
			while (ie.MoveNext()) {
				yield return ie.Current;
			}
		}
		yield return new EditorWaitForSeconds(0.2F);
		Operation.Click(155, 160);	// 左上角地图按钮
		yield return new EditorWaitForSeconds(0.2F);
		Operation.Click(ruinsIconPos.x, ruinsIconPos.y);	// 选择北方遗迹
		yield return new EditorWaitForSeconds(0.2F);
		Operation.Click(gotoBtnPos.x, gotoBtnPos.y);	// 前往按钮
		yield return new EditorWaitForSeconds(0.2F);
		{
			// 再拉远，保证6级田能显示但只显示成图标
			var ie = Operation.Zoom(-19);
			while (ie.MoveNext()) {
				yield return ie.Current;
			}
		}
		yield return new EditorWaitForSeconds(0.2F);
		for (int i = 0; i < 2; i++) {
			// 往左上拖动
			var ie = Operation.NoInertiaDrag(960 - offset.x / 2, 540 - offset.y / 2, 960 + offset.x / 2, 540 + offset.y / 2, 0.5F);
			while (ie.MoveNext()) {
				yield return ie.Current;
			}
		}
		yield return new EditorWaitForSeconds(0.5F);
		const int screenWidth = 1920, screenHeight = 1080;
		const int column = 4, row = 5;
		const int width = 1500, height = 540;
		Color32[,][,] colorsBlocks = new Color32[column,row][,];
		for (int i = 0; i < row; ++i) {
			for (int j = 0; j < column; ++j) {
				colorsBlocks[j, i] = ScreenshotUtils.GetColorsOnScreen((screenWidth - width) / 2, (screenHeight - height) / 2 - 20, width, height);
				if (j < column - 1) {
					// 往右拖动
					var ie = Operation.NoInertiaDrag((screenWidth + width) / 2, 540, (screenWidth - width) / 2, 540, 1F);
					while (ie.MoveNext()) {
						yield return ie.Current;
					}
					yield return new EditorWaitForSeconds(0.5F);
				}
			}
			if (i < row - 1) {
				for (int j = 0; j < column; ++j) {
					if (j < column - 1) {
						// 往左拖动
						var ie = Operation.NoInertiaDrag((screenWidth - width) / 2, 540, (screenWidth + width) / 2, 540, 0.5F);
						while (ie.MoveNext()) {
							yield return ie.Current;
						}
					}
				}
				{
					// 往下拖动
					var ie = Operation.NoInertiaDrag(960, (screenHeight + height) / 2, 960, (screenHeight - height) / 2, 1F);
					while (ie.MoveNext()) {
						yield return ie.Current;
					}
					yield return new EditorWaitForSeconds(0.5F);
				}
			}
		}
		Color32[] colors = new Color32[width * height * column * row];
		for (int i = row - 1; i >= 0; --i) {
			for (int j = 0; j < column; ++j) {
				for (int y = height - 1; y >= 0; --y) {
					for (int x = 0; x < width; ++x) {
						int totalX = j * width + x;
						int totalY = (row - 1 - i) * height + (height - 1 - y);
						Color32 c = colorsBlocks[j, i][x, y];
						colors[totalY * column * width + totalX] = c;
					}
				}
			}
		}
		Texture2D tex = new Texture2D(column * width, row * height);
		tex.SetPixels32(colors);
		tex.Apply();
		byte[] bytes = tex.EncodeToPNG();
		File.WriteAllBytes($"Assets/{filename}.png", bytes);
		AssetDatabase.Refresh();
	}
}
