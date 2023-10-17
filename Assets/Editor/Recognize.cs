/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using System.Collections.Generic;
using UnityEngine;

public static partial class Recognize {
	public enum Scene {
		INSIDE,
		OUTSIDE,
		ARMY_SELECTING,
	}

	public static Scene CurrentScene {
		get {
			// 左上角蓝色返回按钮存在，说明处于出战界面
			if (ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(50, 130), new Color32(94, 126, 202, 255)) >= 0) {
				return Scene.ARMY_SELECTING;
			}
			// 右下角一排按钮里的雷达按钮存在，说明处于世界界面
			if (ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(1850, 540), new Color32(69, 146, 221, 255)) >= 0) {
				return Scene.OUTSIDE;
			}
			return Scene.INSIDE;
		}
	}

	public static bool IsWindowCovered {
		get {
			switch (CurrentScene) {
				case Scene.ARMY_SELECTING:
					// 左上角返回按钮颜色很暗
					return !Approximately(ScreenshotUtils.GetColorOnScreen(50, 130), new Color32(94, 126, 202, 255));
				case Scene.INSIDE:
				case Scene.OUTSIDE:
					// 右下角一排按钮颜色很暗
					return !Approximately(ScreenshotUtils.GetColorOnScreen(1850, 620), new Color32(69, 146, 221, 255));
			}
			return false;
		}
	}

	public static bool IsWindowNoCovered {
		get {
			switch (CurrentScene) {
				case Scene.ARMY_SELECTING:
					// 左上角返回按钮颜色不暗
					return Approximately(ScreenshotUtils.GetColorOnScreen(50, 130), new Color32(94, 126, 202, 255));
				case Scene.INSIDE:
				case Scene.OUTSIDE:
					// 右下角一排按钮颜色不暗
					return Approximately(ScreenshotUtils.GetColorOnScreen(1850, 620), new Color32(69, 146, 221, 255));
			}
			return false;
		}
	}

	public static bool IsOutsideFaraway => ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(170, 164), new Color32(56, 124, 205, 255)) >= 0;

	public static bool IsOutsideNearby => ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(170, 240), new Color32(56, 124, 205, 255)) >= 0;

	public static int BusyGroupCount {
		get {
			int deltaY = IsOutsideNearby ? 76 : IsOutsideFaraway ? 0 : -1;
			if (deltaY >= 0) {
				int groupCount = 0;
				// 返回加速等蓝色按钮
				Color32 targetColor = new Color32(98, 135, 229, 255);
				while (groupCount < 10) {
					Color32 realColor = ScreenshotUtils.GetColorOnScreen(145, 438 + deltaY + groupCount * 50);
					// Debug.LogError($"groupCount: {groupCount}");
					if (ApproximatelyCoveredCount(realColor, targetColor) < 0) {
						break;
					}
					groupCount++;
				}
				return groupCount;
			}
			return int.MaxValue;
		}
	}

	private const int ENERGY_EMPTY = 0;
	private const int ENERGY_FULL = 75;
	private const int ENERGY_EMPTY_X = 21;
	private const int ENERGY_FULL_X = 116;
	private const int ENERGY_Y = 127;
	private static readonly Color32 ENERGY_TARGET_COLOR = new Color32(194, 226, 62, 255);
	public static int energy {
		get {
			int deltaX = IsOutsideNearby ? 80 : IsOutsideFaraway ? 0 : -1;
			if (deltaX >= 0) {
				const int width = ENERGY_FULL_X - ENERGY_EMPTY_X;
				Color32[,] colors = ScreenshotUtils.GetColorsOnScreen(ENERGY_EMPTY_X + deltaX, ENERGY_Y, width + 1, 1);
				for (int x = width; x >= 0; --x) {
					if (Approximately(colors[x, 0], ENERGY_TARGET_COLOR, 0.5F)) {
						return Mathf.RoundToInt((float) x / width * (ENERGY_FULL - ENERGY_EMPTY) + ENERGY_EMPTY);
					}
				}
			}
			return ENERGY_EMPTY;
		}
	}

	// 当前是否处于搜索面板
	public static bool IsSearching => Approximately(ScreenshotUtils.GetColorOnScreen(960, 466), new Color32(119, 131, 184, 255));

	public static bool IsEnergyAdding {
		get {
			// 当前是否处于嗑药面板
			Color32 targetColor1 = new Color32(255, 255, 108, 255);
			Color32 realColor1 = ScreenshotUtils.GetColorOnScreen(830, 590);
			Color32 targetColor2 = new Color32(255, 255, 108, 255);
			Color32 realColor2 = ScreenshotUtils.GetColorOnScreen(960, 590);
			Color32 targetColor3 = new Color32(254, 209, 51, 255);
			Color32 realColor3 = ScreenshotUtils.GetColorOnScreen(960, 702);
			return Approximately(realColor1, targetColor1) ||	// 小体图标
					Approximately(realColor2, targetColor2) ||	// 大体图标
					Approximately(realColor3, targetColor3);	// 使用按钮
		}
	}
	
	public static bool IsBigEnergy(RectInt rect) {
		Color32 targetColor1 = new Color32(255, 255, 108, 255);
		Color32 realColor1 = ScreenshotUtils.GetColorOnScreen(830, 590);
		Color32 targetColor2 = new Color32(255, 255, 108, 255);
		Color32 realColor2 = ScreenshotUtils.GetColorOnScreen(960, 590);
		Color32 targetColor3 = new Color32(254, 209, 51, 255);
		Color32 realColor3 = ScreenshotUtils.GetColorOnScreen(960, 702);
		return Approximately(realColor1, targetColor1) ||	// 小体图标
				Approximately(realColor2, targetColor2) ||	// 大体图标
				Approximately(realColor3, targetColor3);	// 使用按钮
	}
	
	public static readonly Vector2Int[] PROP_ICON_SAMPLE_POINTS = {
		new Vector2Int(20, 20), new Vector2Int(42, 20), new Vector2Int(65, 20),
		new Vector2Int(20, 42), new Vector2Int(42, 42), new Vector2Int(65, 42),
		new Vector2Int(20, 65),	// 右下角有数量显示，不能作为判断依据
		new Vector2Int(12, 73), // 用于判断背景是什么颜色
	};

#region Base
	public static bool Approximately(Color32 realColor, Color32 targetColor, float thresholdMulti = 1) {
		float targetR = targetColor.r;
		float targetG = targetColor.g;
		float targetB = targetColor.b;
		float deltaR = Mathf.Abs(realColor.r / targetR - 1);
		float deltaG = Mathf.Abs(realColor.g / targetG - 1);
		float deltaB = Mathf.Abs(realColor.b / targetB - 1);
		// Debug.LogError($"{deltaR}, {deltaG}, {deltaB}");
		return deltaR < GetThreshold(targetR) * thresholdMulti &&
				deltaG < GetThreshold(targetG) * thresholdMulti &&
				deltaB < GetThreshold(targetB) * thresholdMulti;
	}
	// {0.4F, 0.65F}: 出征界面有个特殊弹窗很浅
	private static readonly Dictionary<float, float> COVER_COEFFICIENT_DICT = new() {
		{0, 1},
		{0.4F, 0.65F},
		{1, 0.299F},
		{2, 0.084F},
	};
	public static float ApproximatelyCoveredCount(Color32 realColor, Color32 targetColor, float thresholdMulti = 1) {
		foreach (var (coverCount, coefficient) in COVER_COEFFICIENT_DICT) {
			float targetR = targetColor.r * coefficient;
			float targetG = targetColor.g * coefficient;
			float targetB = targetColor.b * coefficient;
			// 当目标值为0，则1以内算相近，否则按比例
			float deltaR = targetR == 0 ? realColor.r <= 1 ? 0 : 1 : Mathf.Abs(realColor.r / targetR - 1);
			float deltaG = targetG == 0 ? realColor.g <= 1 ? 0 : 1 : Mathf.Abs(realColor.g / targetG - 1);
			float deltaB = targetB == 0 ? realColor.b <= 1 ? 0 : 1 : Mathf.Abs(realColor.b / targetB - 1);
			// Debug.LogError($"第{coverCount}层：{deltaR}, {deltaG}, {deltaB}");
			if (deltaR < GetThreshold(targetR) * thresholdMulti &&
					deltaG < GetThreshold(targetG) * thresholdMulti &&
					deltaB < GetThreshold(targetB) * thresholdMulti) {
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
#endregion
}
