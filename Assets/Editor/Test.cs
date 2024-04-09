/*
 * @Author: wangyun
 * @CreateTime: 2023-09-21 20:30:39 012
 * @LastEditor: wangyun
 * @EditTime: 2023-09-21 20:30:39 016
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class Test {
	private static bool Approximately(Color32 c1, Color32 c2, int threshold) {
		return Mathf.Abs(c1.r - c2.r) <= threshold &&
				Mathf.Abs(c1.g - c2.g) <= threshold &&
				Mathf.Abs(c1.b - c2.b) <= threshold;
	}
	
	// [MenuItem("Assets/LogCoverCoefficient", priority = -1)]
	// private static void LogCoverCoefficient() {
	// 	Color32[] colors1 = {
	// 		new Color32(69, 146, 221, 255),
	// 		new Color32(21, 44, 66, 255),
	// 		new Color32(6, 13, 20, 255),  
	// 	};
	// 	Color32[] colors2 = {
	// 		new Color32(56, 124, 205, 255),
	// 		new Color32(17, 37, 61, 255),
	// 		new Color32(5, 11, 18, 255),  
	// 	};
	// 	Color32[] colors3 = {
	// 		new Color32(98, 135, 229, 255),
	// 		new Color32(29, 40, 68, 255),
	// 		new Color32(9, 12, 20, 255),  
	// 	};
	// 	void LogCoefficient(Color32[] colors) {
	// 		Color32 baseColor = colors[0];
	// 		for (int i = 1; i < 3; ++i) {
	// 			Color32 coveredColor = colors[i];
	// 			Debug.Log($"{i}层：{coveredColor.r * 1F / baseColor.r}, {coveredColor.g * 1F / baseColor.g}, {coveredColor.b * 1F / baseColor.b}");
	// 		}
	// 	}
	// 	LogCoefficient(colors1);
	// 	LogCoefficient(colors2);
	// 	LogCoefficient(colors3);
	// }
	//
	// [MenuItem("Assets/LogCoverCoefficient1", priority = -1)]
	// private static void LogCoverCoefficient1() {
	// 	Color32[][] colorArrayArray1 = {
	// 		// 近
	// 		new[] {
	// 			new Color32(218, 213, 252, 255),
	// 			new Color32(148, 165, 185, 255),
	// 			new Color32(157, 90, 67, 255),
	// 			new Color32(103, 65, 53, 255),
	// 			new Color32(189, 157, 143, 255),
	// 			new Color32(32, 41, 64, 255),
	// 			new Color32(139, 113, 97, 255),
	// 		},
	// 		// 近1
	// 		new[] {
	// 			new Color32(65, 63, 75, 255),
	// 			new Color32(44, 49, 55, 255),
	// 			new Color32(47, 27, 20, 255),
	// 			new Color32(30, 19, 16, 255),
	// 			new Color32(56, 46, 43, 255),
	// 			new Color32(9, 12, 19, 255),
	// 			new Color32(41, 34, 29, 255),
	// 		},
	// 		// 近2
	// 		new[] {
	// 			new Color32(19, 18, 22, 255),
	// 			new Color32(13, 14, 16, 255),
	// 			new Color32(14, 8, 5, 255),
	// 			new Color32(9, 6, 4, 255),
	// 			new Color32(17, 14, 13, 255),
	// 			new Color32(2, 3, 5, 255),
	// 			new Color32(12, 10, 8, 255), 
	// 		},
	// 	};
	// 	Color32[][] colorArrayArray2 = {
	// 		// 近
	// 		new[] {
	// 			new Color32(122, 122, 130, 255),
	// 			new Color32(159, 155, 156, 255),
	// 			new Color32(202, 203, 200, 255),
	// 			new Color32(111, 109, 115, 255),
	// 			new Color32(172, 168, 163, 255),
	// 			new Color32(162, 141, 140, 255),
	// 			new Color32(133, 120, 106, 255), 
	// 		},
	// 		// 近1
	// 		new[] {
	// 			new Color32(36, 36, 38, 255),
	// 			new Color32(47, 46, 46, 255),
	// 			new Color32(60, 61, 60, 255),
	// 			new Color32(33, 32, 34, 255),
	// 			new Color32(51, 50, 49, 255),
	// 			new Color32(48, 42, 42, 255),
	// 			new Color32(40, 36, 31, 255),
	// 		},
	// 		// 近2
	// 		new[]{
	// 			new Color32(8, 7, 4, 255),
	// 			new Color32(9, 8, 6, 255),
	// 			new Color32(8, 7, 4, 255),
	// 			new Color32(7, 7, 4, 255),
	// 			new Color32(17, 17, 16, 255),
	// 			new Color32(16, 16, 15, 255),
	// 			new Color32(11, 11, 10, 255), 
	// 		},
	// 	};
	// 	Color32[][] colorArrayArray3 = {
	// 		// 远
	// 		new[] {
	// 			new Color32(202, 227, 242, 255),
	// 			new Color32(166, 185, 207, 255),
	// 			new Color32(149, 87, 67, 255),
	// 			new Color32(142, 92, 79, 255),
	// 			new Color32(207, 158, 144, 255),
	// 			new Color32(26, 33, 52, 255),
	// 			new Color32(122, 92, 78, 255), 
	// 		},
	// 		// 远1
	// 		new[] {
	// 			new Color32(60, 68, 72, 255),
	// 			new Color32(49, 55, 61, 255),
	// 			new Color32(44, 26, 20, 255),
	// 			new Color32(42, 27, 24, 255),
	// 			new Color32(61, 47, 43, 255),
	// 			new Color32(7, 10, 15, 255),
	// 			new Color32(36, 27, 23, 255), 
	// 		},
	// 		// 远2
	// 		new[] {
	// 			new Color32(17, 20, 21, 255),
	// 			new Color32(14, 16, 18, 255),
	// 			new Color32(13, 7, 5, 255),
	// 			new Color32(12, 8, 7, 255),
	// 			new Color32(18, 14, 13, 255),
	// 			new Color32(2, 3, 4, 255),
	// 			new Color32(11, 8, 7, 255), 
	// 		},
	// 	};
	// 	Color32[][] colorArrayArray4 = {
	// 		// 远
	// 		new[] {
	// 			new Color32(120, 119, 125, 255),
	// 			new Color32(120, 118, 118, 255),
	// 			new Color32(202, 203, 204, 255),
	// 			new Color32(108, 103, 105, 255),
	// 			new Color32(158, 132, 105, 255),
	// 			new Color32(175, 156, 145, 255),
	// 			new Color32(95, 93, 90, 255), 
	// 		},
	// 		// 远1
	// 		new[] {
	// 			new Color32(36, 35, 37, 255),
	// 			new Color32(36, 35, 35, 255),
	// 			new Color32(60, 61, 61, 255),
	// 			new Color32(32, 30, 31, 255),
	// 			new Color32(47, 39, 31, 255),
	// 			new Color32(52, 46, 43, 255),
	// 			new Color32(28, 28, 27, 255), 
	// 		},
	// 		// 远2
	// 		new[]{
	// 			new Color32(3, 3, 3, 255),
	// 			new Color32(6, 6, 6, 255),
	// 			new Color32(2, 1, 1, 255),
	// 			new Color32(2, 2, 2, 255),
	// 			new Color32(17, 17, 17, 255),
	// 			new Color32(17, 17, 17, 255),
	// 			new Color32(8, 8, 8, 255), 
	// 		},
	// 	};
	// 	float coefficient1R = 0;
	// 	float coefficient1G = 0;
	// 	float coefficient1B = 0;
	// 	float coefficient2R = 0;
	// 	float coefficient2G = 0;
	// 	float coefficient2B = 0;
	// 	void LogCoefficient(Color32[][] colorArrayArray) {
	// 		Color32[] baseColorArray = colorArrayArray[0];
	// 		for (int i = 1; i < 3; ++i) {
	// 			Color32[] coveredColorArray = colorArrayArray[i];
	// 			for (int j = 0, length = coveredColorArray.Length; j < length; ++j) {
	// 				Color32 baseColor = baseColorArray[j];
	// 				Color32 coveredColor = coveredColorArray[j];
	// 				float coefficientR = coveredColor.r * 1F / baseColor.r;
	// 				float coefficientG = coveredColor.g * 1F / baseColor.g;
	// 				float coefficientB = coveredColor.b * 1F / baseColor.b;
	// 				// Debug.Log($"{i}层：{coefficientR}, {coefficientG}, {coefficientB}");
	// 				if (i == 1) {
	// 					coefficient1R += coefficientR;
	// 					coefficient1G += coefficientG;
	// 					coefficient1B += coefficientB;
	// 				} else if (i == 2) {
	// 					coefficient2R += coefficientR;
	// 					coefficient2G += coefficientG;
	// 					coefficient2B += coefficientB;
	// 				}
	// 			}
	// 		}
	// 	}
	// 	LogCoefficient(colorArrayArray1);
	// 	LogCoefficient(colorArrayArray2);
	// 	LogCoefficient(colorArrayArray3);
	// 	LogCoefficient(colorArrayArray4);
	// 	coefficient1R /= 7 * 4;
	// 	coefficient1G /= 7 * 4;
	// 	coefficient1B /= 7 * 4;
	// 	coefficient2R /= 7 * 4;
	// 	coefficient2G /= 7 * 4;
	// 	coefficient2B /= 7 * 4;
	// 	Debug.Log($"1层：{coefficient1R}, {coefficient1G}, {coefficient1B}");
	// 	Debug.Log($"2层：{coefficient2R}, {coefficient2G}, {coefficient2B}");
	// }
	
	// [MenuItem("Assets/LogMRXGroupNumber", priority = -1)]
	// private static void LogMRXGroupNumber() {
	// 	Debug.LogError(Recognize.GetMRXGroupNumber());
	// }
	//
	// [MenuItem("Assets/LogWindowCoveredCount", priority = -1)]
	// private static void LogWindowCoveredCount() {
	// 	Debug.LogError(Recognize.ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(1850, 540), new Color32(69, 146, 221, 255)));
	// 	Debug.LogError(Recognize.ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(170, 164), new Color32(56, 124, 205, 255)));
	// 	Debug.LogError(Recognize.ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(170, 164 + 76), new Color32(56, 124, 205, 255)));
	// 	Debug.LogError(Recognize.ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(145, 438), new Color32(98, 135, 229, 255)));
	// 	Debug.LogError(Recognize.ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(145, 438 + 76), new Color32(98, 135, 229, 255)));
	// 	Debug.LogError(Recognize.ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(50, 130), new Color32(94, 126, 202, 255)));
	// }
	//
	[MenuItem("Assets/TestGetThreshold", priority = -1)]
	private static void TestGetThreshold() {
		// Debug.LogError($"122: {Recognize.GetThreshold(29.302F)}");
		// Debug.LogError($"121: {Recognize.GetThreshold(40.365F)}");
		// Debug.LogError($"129: {Recognize.GetThreshold(68.471F)}");
		// Debug.LogError($"195: {Recognize.GetThreshold(195) * 195}");
		// Debug.LogError($"232: {Recognize.GetThreshold(232) * 232}");
		// Debug.LogError($"64: {Recognize.GetThreshold(64) * 64}");
	}
	
	[MenuItem("Assets/LogBusyGroupCount", priority = -1)]
	private static void LogBusyGroupCount() {
		Stopwatch sw = Stopwatch.StartNew();
		Debug.LogError(Recognize.BusyGroupCount);
		Debug.LogError(Recognize.BusyGroupCount);
		Debug.LogError(sw.Elapsed.Milliseconds);
	}
	
	[MenuItem("Assets/LogFollowType", priority = -1)]
	private static void LogFollowType() {
		Debug.LogError(Recognize.GetFollowType());
	}
	
	[MenuItem("Assets/LogAllianceActivityTypes", priority = -1)]
	private static void LogAllianceActivityTypes() {
		Debug.LogError(string.Join(", ", Recognize.AllianceActivityTypes));
	}
	
	[MenuItem("Assets/ScreenshotFollowTypeIcon", priority = -1)]
	private static void ScreenshotFollowTypeIcon() {
		EditorCoroutineManager.StartCoroutine(IEScreenshotFollowTypeIcon());
	}
	private static IEnumerator IEScreenshotFollowTypeIcon() {
		Stopwatch sw = Stopwatch.StartNew();
		for (int i = 0; i < 100; i++) {
			ScreenshotUtils.Screenshot(988, 182, 213, 167, Application.dataPath + $"/Follow/Test{DateTime.Now:yyyy-MM-dd_HH.mm.ss.fff}.png");
			yield return new EditorWaitForSeconds(0.05F);
		}
		Debug.LogError(sw.Elapsed.Milliseconds);
	}
	
	// [MenuItem("Assets/LogScreenshotCost", priority = -1)]
	// private static void LogScreenshotCost() {
	// 	Stopwatch sw = Stopwatch.StartNew();
	// 	for (int i = 0; i < 3; i++) {
	// 		ScreenshotUtils.GetColorOnScreen(Random.Range(0, 1920), Random.Range(0, 1080));
	// 		ScreenshotUtils.GetColorOnScreen(Random.Range(0, 1920), Random.Range(0, 1080));
	// 		ScreenshotUtils.GetColorOnScreen(Random.Range(0, 1920), Random.Range(0, 1080));
	// 	}
	// 	Debug.LogError(sw.Elapsed.Milliseconds);
	// 	sw.Restart();
	// 	for (int i = 0; i < 3; i++) {
	// 		ScreenshotUtils.GetColorsOnScreen(0, 0, 192, 108);
	// 	}
	// 	Debug.LogError(sw.Elapsed.Milliseconds);
	// 	sw.Restart();
	// 	for (int i = 0; i < 1; i++) {
	// 		ScreenshotUtils.GetColorsOnScreen(0, 0, 1920, 1080);
	// 	}
	// 	Debug.LogError(sw.Elapsed.Milliseconds);
	// }
	//
	[MenuItem("Assets/LogIsQuickFixExist", priority = -1)]
	private static void LogIsQuickFixExist() {
		Debug.LogError(Recognize.IsQuickFixExist);
	}
	
	// [MenuItem("Assets/LogFollowIcon", priority = -1)]
	// private static void LogFollowIcon() {
	// 	Color32 targetColor = new Color32(77, 134, 159, 255);
	// 	const int realX = 1116;
	// 	const int realY = 273;
	// 	for (int y = -1; y < 2; y++) {
	// 		for (int x = -1; x < 2; x++) {
	// 			Color32 realColor = ScreenshotUtils.GetColorOnScreen(realX + x, realY + y);
	// 			Debug.LogError($"{x}, {y}: {Recognize.Approximately(realColor, targetColor)}");
	// 		}
	// 	}
	// }
	
	[MenuItem("Assets/LogIsFollowOuterJoinBtnExist", priority = -1)]
	private static void LogIsFollowOuterJoinBtnExist() {
	}
	[MenuItem("Assets/LogEnergy", priority = -1)]
	private static void LogEnergy() {
		Stopwatch sw = Stopwatch.StartNew();
		Debug.LogError(Recognize.energy);
		Debug.LogError(sw.Elapsed.TotalMilliseconds);
		Debug.LogError(Recognize.CurrentScene);
	}
	[MenuItem("Assets/TestGroupState", priority = -1)]
	private static void TestGroupState() {
		for (int i = 0; i < 4; i++) {
			Debug.LogError(Recognize.GetGroupState(i));;
		}
	}

	[MenuItem("Assets/IsFollowOwnerEnabled", priority = -1)]
	public static void IsFollowOwnerEnabled() {
		Debug.LogError(Follow.IsFollowOwnerEnabled());
	}

	private static EditorCoroutine s_CO;
	[MenuItem("Assets/TestMultiDesktop_Start", priority = -1)]
	private static void TestMultiDesktopStart() {
		TestMultiDesktopStop();
		s_CO = EditorCoroutineManager.StartCoroutine(DoTestMultiDesktopStart());
	}
	[MenuItem("Assets/TestMultiDesktop_Stop", priority = -1)]
	private static void TestMultiDesktopStop() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
		}
	}
	private static IEnumerator DoTestMultiDesktopStart() {
		while (true) {
			ScreenshotUtils.Screenshot(0, 0, 1920, 1080, Application.dataPath + $"/PersistentData/Textures/Test{DateTime.Now:yyyy-MM-dd_HH.mm.ss.fff}.png");
			AssetDatabase.Refresh();
			Debug.LogError("-----------------------------------");
			yield return new EditorWaitForSeconds(5);
		}
	}

	private static EditorCoroutine s_CO1;
	[MenuItem("Assets/Test", priority = -1)]
	private static void TestFunc() {
		Debug.LogError(string.Join(",", Recognize.GetShortcutTypes()));
	}
	
	
	private static EditorCoroutine s_CO_FIC;
	[MenuItem("Test/StartFollowIconCheck", priority = -1)]
	private static void StartFollowIconCheck() {
		StopFollowIconCheck();
		s_CO_FIC = EditorCoroutineManager.StartCoroutine(IEFollowIconCheck());
	}
	[MenuItem("Test/StopFollowIconCheck", priority = -1)]
	private static void StopFollowIconCheck() {
		if (s_CO_FIC != null) {
			EditorCoroutineManager.StopCoroutine(s_CO_FIC);
			EditorCoroutineManager.Flush(s_CO_FIC);
		}
	}
	private static IEnumerator IEFollowIconCheck() {
		const int X = 988, Y = 182;
		const int WIDTH = 213, HEIGHT = 167;
		Color32[,] prevColors = Operation.GetColorsOnScreen(X, Y, WIDTH, HEIGHT);
		int width = prevColors.GetLength(0);
		int height = prevColors.GetLength(1);
		bool[,] diff = new bool[width, height];
		Stopwatch sw = Stopwatch.StartNew();
		for (int i = 0; i < 50; i++) {
			yield return null;
			Color32[,] realColors = Operation.GetColorsOnScreen(X, Y, WIDTH, HEIGHT);
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					if (!diff[x, y]) {
						Color32 realColor = realColors[x, y];
						Color32 prevColor = prevColors[x, y];
						if (realColor.r != prevColor.r || realColor.g != prevColor.g || realColor.b != prevColor.b) {
							diff[x, y] = true;
						}
					}
				}
			}
			if (sw.Elapsed.TotalSeconds > 10) {
				break;
			}
		}
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				if (!diff[x, y]) {
					Color32 prevColor = prevColors[x, y];
					Debug.LogError($"[{x}, {y}]: [{prevColor.r}, {prevColor.g}, {prevColor.b}]");
				}
			}
		}
	}
}
