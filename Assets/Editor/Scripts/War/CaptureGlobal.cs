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

public class CaptureGlobal {
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;
	
	[MenuItem("Tools_Task/StartCaptureGlobal", priority = -1)]
	private static void Enable() {
		Disable();
		s_CO = EditorCoroutineManager.StartCoroutine(IECapture());
		Debug.Log("开始进行全区截图");
	}

	[MenuItem("Tools_Task/StopCaptureGlobal", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("取消全区截图");
		}
	}

	private static IEnumerator IECapture() {
		var ie = IEDoCapture("Global");
		while (ie.MoveNext()) {
			yield return ie.Current;
		}
		s_CO = null;
	}

	private static IEnumerator IEDoCapture(string filename) {
		yield return new EditorWaitForSeconds(2);
		const int screenWidth = 1920, screenHeight = 1080;
		const int column = 4, row = 11;
		const int width = 1232, height = 722;
		Color32[,][,] colorsBlocks = new Color32[column + 2,row + 2][,];
		for (int i = 0; i < row; ++i) {
			for (int j = 0; j < column; ++j) {
				if (i == 0) {
					if (j == 0) {
						colorsBlocks[j + 1 - 1, i + 1 - 1] = ScreenshotUtils.GetColorsOnScreen((screenWidth - width) / 2 - width, (screenHeight - height) / 2 - 15 - height, width, height);
					} else if (j == column - 1) {
						colorsBlocks[j + 1 + 1, i + 1 - 1] = ScreenshotUtils.GetColorsOnScreen((screenWidth - width) / 2 + width, (screenHeight - height) / 2 - 15 - height, width, height);
					}
					colorsBlocks[j + 1, i + 1 - 1] = ScreenshotUtils.GetColorsOnScreen((screenWidth - width) / 2, (screenHeight - height) / 2 - 15 - height, width, height);
				} else if (i == row - 1) {
					if (j == 0) {
						colorsBlocks[j + 1 - 1, i + 1 + 1] = ScreenshotUtils.GetColorsOnScreen((screenWidth - width) / 2 - width, (screenHeight - height) / 2 - 15 + height, width, height);
					} else if (j == column - 1) {
						colorsBlocks[j + 1 + 1, i + 1 + 1] = ScreenshotUtils.GetColorsOnScreen((screenWidth - width) / 2 + width, (screenHeight - height) / 2 - 15 + height, width, height);
					}
					colorsBlocks[j + 1, i + 1 + 1] = ScreenshotUtils.GetColorsOnScreen((screenWidth - width) / 2, (screenHeight - height) / 2 - 15 + height, width, height);
				}
				if (j == 0) {
					colorsBlocks[j + 1 - 1, i + 1] = ScreenshotUtils.GetColorsOnScreen((screenWidth - width) / 2 - width, (screenHeight - height) / 2 - 15, width, height);
				} else if (j == column - 1) {
					colorsBlocks[j + 1 + 1, i + 1] = ScreenshotUtils.GetColorsOnScreen((screenWidth - width) / 2 + width, (screenHeight - height) / 2 - 15, width, height);
				}
				colorsBlocks[j + 1, i + 1] = ScreenshotUtils.GetColorsOnScreen((screenWidth - width) / 2, (screenHeight - height) / 2 - 15, width, height);
				if (j < column - 1) {
					// 往右拖动
					var ie = Operation.NoInertiaDrag((screenWidth + width) / 2, 540, (screenWidth - width) / 2, 540, 1F);
					while (ie.MoveNext()) {
						yield return ie.Current;
					}
					yield return new EditorWaitForSeconds(1);
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

		// 上：659（height - ((screenHeight - height) / 2 - 15 - 101)）
		// 下：592（height - ((screenHeight - height) / 2 + 15 - 64)）
		// 左：888（width - (screenWidth - width) / 2）
		// 右：888（width - (screenWidth - width) / 2）
		int totalRow = row + 2;
		int totalColumn = column + 2;
		Debug.LogError($"row:{row}, column:{column}, totalRow:{totalRow}, totalColumn:{totalColumn}");
		Color32[,] colors = new Color32[width * totalColumn, height * totalRow];
		for (int i = 0; i < totalRow; ++i) {
			for (int j = 0; j < totalColumn; ++j) {
				for (int y = 0; y < height; ++y) {
					for (int x = 0; x < width; ++x) {
						colors[j * width + x, i * height + y] = colorsBlocks[j, i][x, y];
					}
				}
			}
		}
		// for (int i = totalRow - 1; i >= 0; --i) {
		// 	for (int j = 0; j < totalColumn; ++j) {
		// 		for (int y = height - 1; y >= 0; --y) {
		// 			for (int x = 0; x < width; ++x) {
		// 				int totalX = j * width + x;
		// 				int totalY = (totalRow - 1 - i) * height + (height - 1 - y);
		// 				colors[totalX, totalY] = colorsBlocks[j, i][x, y];
		// 			}
		// 		}
		// 	}
		// }
		int top = height - ((screenHeight - height) / 2 - 15 - 101);
		int bottom = height - ((screenHeight - height) / 2 + 15 - 64);
		int side = width - (screenWidth - width) / 2;
		int clippedWidth = width * totalColumn - side - side;
		int clippedHeight = height * totalRow - top - bottom;
		Color32[] clippedColors = new Color32[clippedWidth * clippedHeight];
		for (int y = clippedHeight - 1; y >= 0; --y) {
			for (int x = 0; x < clippedWidth; ++x) {
				clippedColors[y * clippedWidth + x] = colors[side + x, top + clippedHeight - 1 - y];
			}
		}
		// Color32[] colors = new Color32[width * totalColumn * height * totalRow];
		// for (int i = totalRow - 1; i >= 0; --i) {
		// 	for (int j = 0; j < totalColumn; ++j) {
		// 		for (int y = height - 1; y >= 0; --y) {
		// 			for (int x = 0; x < width; ++x) {
		// 				int totalX = j * width + x;
		// 				int totalY = (totalRow - 1 - i) * height + (height - 1 - y);
		// 				colors[totalY * totalColumn * width + totalX] = colorsBlocks[j, i][x, y];
		// 			}
		// 		}
		// 	}
		// }
		Texture2D tex = new Texture2D(clippedWidth, clippedHeight);
		tex.SetPixels32(clippedColors);
		tex.Apply();
		byte[] bytes = tex.EncodeToPNG();
		File.WriteAllBytes($"Assets/{filename}.png", bytes);
		AssetDatabase.Refresh();
	}
}
