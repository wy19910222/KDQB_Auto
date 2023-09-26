/*
 * @Author: wangyun
 * @CreateTime: 2023-09-21 20:30:39 012
 * @LastEditor: wangyun
 * @EditTime: 2023-09-21 20:30:39 016
 */

using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class Test {
	private static bool Approximately(Color32 c1, Color32 c2, int threshold) {
		return Mathf.Abs(c1.r - c2.r) <= threshold &&
				Mathf.Abs(c1.g - c2.g) <= threshold &&
				Mathf.Abs(c1.b - c2.b) <= threshold;
	}
	
	[MenuItem("Assets/LogGroupHeroAvatar", priority = -1)]
	private static void LogGroupHeroAvatar() {
		int deltaY = Recognize.IsOutsideNearby ? 76 : Recognize.IsOutsideFaraway ? 0 : -1;
		if (deltaY >= 0) {
			int groupCount = 0;
			Color32 targetColor1 = new Color32(98, 135, 229, 255);	// 无界面覆盖
			Color32 targetColor2 = new Color32(29, 40, 68, 255);	// 联盟背包等窗口覆盖
			Color32 targetColor3 = new Color32(9, 12, 20, 255);	// 曙光活动主界面（或双层窗口）覆盖
			while (groupCount < 10) {
				Color32 realColor = ScreenshotUtils.GetColorOnScreen(145, 438 + deltaY + groupCount * 50);
				Debug.Log(realColor);
				Debug.Log(Approximately(realColor, targetColor1, 10));
				Debug.Log(Approximately(realColor, targetColor2, 10));
				Debug.Log(Approximately(realColor, targetColor3, 10));
				if (!Approximately(realColor, targetColor1, 10) &&
						!Approximately(realColor, targetColor2, 10) &&
						!Approximately(realColor, targetColor3, 10)) {
					break;
				}
				Debug.Log($"----------------------{groupCount}-----------------------");
				List<Color32> list = new List<Color32>();
				for (int i = 0, length = Recognize.AVATAR_SAMPLE_POINTS.Length; i < length; ++i) {
					Vector2Int point = Recognize.AVATAR_SAMPLE_POINTS[i];
					Vector2Int finalPoint = new Vector2Int(22 + point.x, 418 + deltaY + groupCount * 50 + point.y);
					int r = 0, g = 0, b = 0;
					for (int y = -1; y < 2; ++y) {
						Color32 c = ScreenshotUtils.GetColorOnScreen(finalPoint.x, finalPoint.y + y);
						r += c.r;
						g += c.g;
						b += c.b;
					}
					Color32 color = new Color32((byte) (r / 3), (byte) (g / 3), (byte) (b / 3), 255);
					list.Add(color);
					// Debug.Log($"{finalPoint}: {color}");
				}
				StringBuilder sb = new StringBuilder();
				foreach (var c in list) {
					sb.Append("new Color32(");
					sb.Append(c.r);
					sb.Append(", ");
					sb.Append(c.g);
					sb.Append(", ");
					sb.Append(c.b);
					sb.Append(", 255), ");
				}
				Debug.LogError(sb);
				groupCount++;
			}
		}
	}
	
	[MenuItem("Assets/LogCoverCoefficient", priority = -1)]
	private static void LogCoverCoefficient() {
		Color32[] colors1 = {
			new Color32(69, 146, 221, 255),
			new Color32(21, 44, 66, 255),
			new Color32(6, 13, 20, 255),  
		};
		Color32[] colors2 = {
			new Color32(56, 124, 205, 255),
			new Color32(17, 37, 61, 255),
			new Color32(5, 11, 18, 255),  
		};
		Color32[] colors3 = {
			new Color32(98, 135, 229, 255),
			new Color32(29, 40, 68, 255),
			new Color32(9, 12, 20, 255),  
		};
		void LogCoefficient(Color32[] colors) {
			Color32 baseColor = colors[0];
			for (int i = 1; i < 3; ++i) {
				Color32 coveredColor = colors[i];
				Debug.Log($"{i}层：{coveredColor.r * 1F / baseColor.r}, {coveredColor.g * 1F / baseColor.g}, {coveredColor.b * 1F / baseColor.b}");
			}
		}
		LogCoefficient(colors1);
		LogCoefficient(colors2);
		LogCoefficient(colors3);
	}
	
	[MenuItem("Assets/LogCoverCoefficient1", priority = -1)]
	private static void LogCoverCoefficient1() {
		Color32[][] colorArrayArray1 = {
			// 近
			new[] {
				new Color32(218, 213, 252, 255),
				new Color32(148, 165, 185, 255),
				new Color32(157, 90, 67, 255),
				new Color32(103, 65, 53, 255),
				new Color32(189, 157, 143, 255),
				new Color32(32, 41, 64, 255),
				new Color32(139, 113, 97, 255),
			},
			// 近1
			new[] {
				new Color32(65, 63, 75, 255),
				new Color32(44, 49, 55, 255),
				new Color32(47, 27, 20, 255),
				new Color32(30, 19, 16, 255),
				new Color32(56, 46, 43, 255),
				new Color32(9, 12, 19, 255),
				new Color32(41, 34, 29, 255),
			},
			// 近2
			new[] {
				new Color32(19, 18, 22, 255),
				new Color32(13, 14, 16, 255),
				new Color32(14, 8, 5, 255),
				new Color32(9, 6, 4, 255),
				new Color32(17, 14, 13, 255),
				new Color32(2, 3, 5, 255),
				new Color32(12, 10, 8, 255), 
			},
		};
		Color32[][] colorArrayArray2 = {
			// 近
			new[] {
				new Color32(122, 122, 130, 255),
				new Color32(159, 155, 156, 255),
				new Color32(202, 203, 200, 255),
				new Color32(111, 109, 115, 255),
				new Color32(172, 168, 163, 255),
				new Color32(162, 141, 140, 255),
				new Color32(133, 120, 106, 255), 
			},
			// 近1
			new[] {
				new Color32(36, 36, 38, 255),
				new Color32(47, 46, 46, 255),
				new Color32(60, 61, 60, 255),
				new Color32(33, 32, 34, 255),
				new Color32(51, 50, 49, 255),
				new Color32(48, 42, 42, 255),
				new Color32(40, 36, 31, 255),
			},
			// 近2
			new[]{
				new Color32(8, 7, 4, 255),
				new Color32(9, 8, 6, 255),
				new Color32(8, 7, 4, 255),
				new Color32(7, 7, 4, 255),
				new Color32(17, 17, 16, 255),
				new Color32(16, 16, 15, 255),
				new Color32(11, 11, 10, 255), 
			},
		};
		Color32[][] colorArrayArray3 = {
			// 远
			new[] {
				new Color32(202, 227, 242, 255),
				new Color32(166, 185, 207, 255),
				new Color32(149, 87, 67, 255),
				new Color32(142, 92, 79, 255),
				new Color32(207, 158, 144, 255),
				new Color32(26, 33, 52, 255),
				new Color32(122, 92, 78, 255), 
			},
			// 远1
			new[] {
				new Color32(60, 68, 72, 255),
				new Color32(49, 55, 61, 255),
				new Color32(44, 26, 20, 255),
				new Color32(42, 27, 24, 255),
				new Color32(61, 47, 43, 255),
				new Color32(7, 10, 15, 255),
				new Color32(36, 27, 23, 255), 
			},
			// 远2
			new[] {
				new Color32(17, 20, 21, 255),
				new Color32(14, 16, 18, 255),
				new Color32(13, 7, 5, 255),
				new Color32(12, 8, 7, 255),
				new Color32(18, 14, 13, 255),
				new Color32(2, 3, 4, 255),
				new Color32(11, 8, 7, 255), 
			},
		};
		Color32[][] colorArrayArray4 = {
			// 远
			new[] {
				new Color32(120, 119, 125, 255),
				new Color32(120, 118, 118, 255),
				new Color32(202, 203, 204, 255),
				new Color32(108, 103, 105, 255),
				new Color32(158, 132, 105, 255),
				new Color32(175, 156, 145, 255),
				new Color32(95, 93, 90, 255), 
			},
			// 远1
			new[] {
				new Color32(36, 35, 37, 255),
				new Color32(36, 35, 35, 255),
				new Color32(60, 61, 61, 255),
				new Color32(32, 30, 31, 255),
				new Color32(47, 39, 31, 255),
				new Color32(52, 46, 43, 255),
				new Color32(28, 28, 27, 255), 
			},
			// 远2
			new[]{
				new Color32(3, 3, 3, 255),
				new Color32(6, 6, 6, 255),
				new Color32(2, 1, 1, 255),
				new Color32(2, 2, 2, 255),
				new Color32(17, 17, 17, 255),
				new Color32(17, 17, 17, 255),
				new Color32(8, 8, 8, 255), 
			},
		};
		float coefficient1R = 0;
		float coefficient1G = 0;
		float coefficient1B = 0;
		float coefficient2R = 0;
		float coefficient2G = 0;
		float coefficient2B = 0;
		void LogCoefficient(Color32[][] colorArrayArray) {
			Color32[] baseColorArray = colorArrayArray[0];
			for (int i = 1; i < 3; ++i) {
				Color32[] coveredColorArray = colorArrayArray[i];
				for (int j = 0, length = coveredColorArray.Length; j < length; ++j) {
					Color32 baseColor = baseColorArray[j];
					Color32 coveredColor = coveredColorArray[j];
					float coefficientR = coveredColor.r * 1F / baseColor.r;
					float coefficientG = coveredColor.g * 1F / baseColor.g;
					float coefficientB = coveredColor.b * 1F / baseColor.b;
					// Debug.Log($"{i}层：{coefficientR}, {coefficientG}, {coefficientB}");
					if (i == 1) {
						coefficient1R += coefficientR;
						coefficient1G += coefficientG;
						coefficient1B += coefficientB;
					} else if (i == 2) {
						coefficient2R += coefficientR;
						coefficient2G += coefficientG;
						coefficient2B += coefficientB;
					}
				}
			}
		}
		LogCoefficient(colorArrayArray1);
		LogCoefficient(colorArrayArray2);
		LogCoefficient(colorArrayArray3);
		LogCoefficient(colorArrayArray4);
		coefficient1R /= 7 * 4;
		coefficient1G /= 7 * 4;
		coefficient1B /= 7 * 4;
		coefficient2R /= 7 * 4;
		coefficient2G /= 7 * 4;
		coefficient2B /= 7 * 4;
		Debug.Log($"1层：{coefficient1R}, {coefficient1G}, {coefficient1B}");
		Debug.Log($"2层：{coefficient2R}, {coefficient2G}, {coefficient2B}");
	}
	
	// [MenuItem("Assets/LogYLKGroupNumber", priority = -1)]
	// private static void LogYLKGroupNumber() {
	// 	Debug.LogError(Recognize.GetYLKGroupNumber());
	// }
	
	[MenuItem("Assets/LogWindowCoveredCount", priority = -1)]
	private static void LogWindowCoveredCount() {
		Debug.LogError(Recognize.ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(1850, 540), new Color32(69, 146, 221, 255)));
		Debug.LogError(Recognize.ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(170, 164), new Color32(56, 124, 205, 255)));
		Debug.LogError(Recognize.ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(170, 164 + 76), new Color32(56, 124, 205, 255)));
		Debug.LogError(Recognize.ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(145, 438), new Color32(98, 135, 229, 255)));
		Debug.LogError(Recognize.ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(145, 438 + 76), new Color32(98, 135, 229, 255)));
		Debug.LogError(Recognize.ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(50, 130), new Color32(94, 126, 202, 255)));
	}
}
