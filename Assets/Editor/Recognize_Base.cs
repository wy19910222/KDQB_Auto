/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using System.Collections.Generic;
using UnityEngine;

public static partial class Recognize {
	public static float ApproximatelyRectAverage(Color32[,] realColors, Color32[,] targetColors, int averageW, int averageH, float thresholdMulti = 1) {
		int realWidth = realColors.GetLength(0);
		int targetWidth = targetColors.GetLength(0);
		if (realWidth != targetWidth) {
			return 0;
		}
		int realHeight = realColors.GetLength(1);
		int targetHeight = targetColors.GetLength(1);
		if (realHeight != targetHeight) {
			return 0;
		}
		Color32[,] realAverageColors = GetAverageColors(realColors, averageW, averageH);
		Color32[,] targetAverageColors = GetAverageColors(targetColors, averageW, averageH);
		return ApproximatelyRect(realAverageColors, targetAverageColors, thresholdMulti);
	}
	private static Color32[,] GetAverageColors(Color32[,] colors, int averageW, int averageH) {
		int width = colors.GetLength(0);
		int height = colors.GetLength(1);
		Color32[,] averageColors = new Color32[width, height];
		int countAverage = (averageW + averageW + 1) * (averageH + averageH + 1);
		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				float r = 0, g = 0, b = 0;
				for (int offsetY = -averageH; offsetY <= averageH; ++offsetY) {
					for (int offsetX = -averageW; offsetX <= averageW; ++offsetX) {
						Color32 c = colors[
								Mathf.Clamp(x + offsetX, 0, width - 1),
								Mathf.Clamp(y + offsetY, 0, height - 1)
						];
						r += c.r;
						g += c.g;
						b += c.b;
					}
				}
				averageColors[x, y] = new Color32(
						(byte) Mathf.RoundToInt(r / countAverage),
						(byte) Mathf.RoundToInt(g / countAverage),
						(byte) Mathf.RoundToInt(b / countAverage),
						255
				);
			}
		}
		return averageColors;
	}
	
	public static float ApproximatelyRect(Color32[,] realColors, Color32[,] targetColors, float thresholdMulti = 1) {
		int realWidth = realColors.GetLength(0);
		int targetWidth = targetColors.GetLength(0);
		if (realWidth != targetWidth) {
			return 0;
		}
		int realHeight = realColors.GetLength(1);
		int targetHeight = targetColors.GetLength(1);
		if (realHeight != targetHeight) {
			return 0;
		}
		int approximatelyCount = 0;
		for (int y = 0; y < realHeight; ++y) {
			for (int x = 0; x < realWidth; ++x) {
				if (Approximately(realColors[x, y], targetColors[x, y], thresholdMulti)) {
					++approximatelyCount;
				}
			}
		}
		return (float) approximatelyCount / (realWidth * realHeight);
	}
	
	public static float ApproximatelyRectIgnoreCovered(Color32[,] realColors, Color32[,] targetColors, float thresholdMulti = 1) {
		int realWidth = realColors.GetLength(0);
		int targetWidth = targetColors.GetLength(0);
		if (realWidth != targetWidth) {
			return 0;
		}
		int realHeight = realColors.GetLength(1);
		int targetHeight = targetColors.GetLength(1);
		if (realHeight != targetHeight) {
			return 0;
		}
		int approximatelyCount = 0;
		for (int y = 0; y < realHeight; ++y) {
			for (int x = 0; x < realWidth; ++x) {
				if (ApproximatelyCoveredCount(realColors[x, y], targetColors[x, y], thresholdMulti) >= 0) {
					++approximatelyCount;
				}
			}
		}
		return (float) approximatelyCount / (realWidth * realHeight);
	}
	
	public static bool Approximately(Color32 realColor, Color32 targetColor, float thresholdMulti = 1) {
		float targetR = targetColor.r;
		float targetG = targetColor.g;
		float targetB = targetColor.b;
		float deltaR = targetR == 0 ? realColor.r - 1 : Mathf.Abs(realColor.r / targetR - 1);
		float deltaG = targetG == 0 ? realColor.g - 1 : Mathf.Abs(realColor.g / targetG - 1);
		float deltaB = targetB == 0 ? realColor.b - 1 : Mathf.Abs(realColor.b / targetB - 1);
		return deltaR <= GetThreshold(targetR) * thresholdMulti &&
				deltaG <= GetThreshold(targetG) * thresholdMulti &&
				deltaB <= GetThreshold(targetB) * thresholdMulti;
	}
	// {0.4F, 0.65F}: 出征界面有个特殊弹窗很浅
	private static readonly Dictionary<float, float> COVER_COEFFICIENT_DICT = new() {
		{0, 1},
		{0.4F, 0.65F},
		{1, 0.298F},	// 76/255
		{2, 0.09F},		// 23/255
		{3, 0.02745F},		// 7/255
	};
	public static float ApproximatelyCoveredCount(Color32 realColor, Color32 targetColor, float thresholdMulti = 1) {
		foreach (var (coverCount, coefficient) in COVER_COEFFICIENT_DICT) {
			float targetR = targetColor.r * coefficient;
			float targetG = targetColor.g * coefficient;
			float targetB = targetColor.b * coefficient;
			// 当目标值为0，则1以内算相近，否则按比例
			float deltaR = targetR == 0 ? realColor.r - 1 : Mathf.Abs(realColor.r / targetR - 1);
			float deltaG = targetG == 0 ? realColor.g - 1 : Mathf.Abs(realColor.g / targetG - 1);
			float deltaB = targetB == 0 ? realColor.b - 1 : Mathf.Abs(realColor.b / targetB - 1);
			// Debug.LogError($"第{coverCount}层：{deltaR}, {deltaG}, {deltaB}");
			if (deltaR <= GetThreshold(targetR) * thresholdMulti &&
					deltaG <= GetThreshold(targetG) * thresholdMulti &&
					deltaB <= GetThreshold(targetB) * thresholdMulti) {
				return coverCount;
			}
		}
		return -1;
	}
	public static float GetThreshold(float value) {
		return value switch {
			// > 100 => ((value - 100) * 0.03333F + 10) / value,
			> 100 => 0.03333F + 6.66667F / value,
			// > 10 => ((value - 10) * 0.07778F + 3) / value,
			> 10 => 0.07778F + 2.22222F / value,
			// > 0 => (value * 0.2F + 1) / value
			> 0 => 0.2F + 1 / value,
			_ => 1
		};
	}
}
