/*
 * @Author: wangyun
 * @CreateTime: 2023-11-07 02:59:53 218
 * @LastEditor: wangyun
 * @EditTime: 2023-11-07 02:59:53 222
 */

using UnityEngine;

public static partial class Recognize {
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

	private static readonly Color32[,] ENERGY_SHORTCUT_ADDING = ScreenshotUtils.GetFromFile("PersistentData/Textures/EnergyShortcutAdding.png");
	public static bool IsEnergyShortcutAdding {
		get {
			Color32[,] realColors = ScreenshotUtils.GetColorsOnScreen(910, 370, 100, 26);
			return ApproximatelyRect(realColors, ENERGY_SHORTCUT_ADDING) > 0.99F;
		}
	}

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
}
