/*
 * @Author: wangyun
 * @CreateTime: 2023-11-07 02:59:53 218
 * @LastEditor: wangyun
 * @EditTime: 2023-11-07 02:59:53 222
 */

using System.Collections.Generic;
using UnityEngine;

public static partial class Recognize {
	public enum EnergyShortcutAddingType {
		[InspectorName("无")]
		NONE,
		[InspectorName("使用大体")]
		BIG_BOTTLE,
		[InspectorName("使用小体")]
		SMALL_BOTTLE,
		[InspectorName("购买体力")]
		DIAMOND_BUY,
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

	private static readonly Color32[,] ENERGY_BOTTLE_BIG = ScreenshotUtils.GetFromFile("PersistentData/Textures/EnergyBottleBig.png");
	private static readonly Color32[,] ENERGY_BOTTLE_SMALL = ScreenshotUtils.GetFromFile("PersistentData/Textures/EnergyBottleSmall.png");
	private static readonly Color32[,] ENERGY_DIAMOND_BUY = ScreenshotUtils.GetFromFile("PersistentData/Textures/EnergyDiamondBuy.png");
	public static EnergyShortcutAddingType GetShortcutType(int shortcutIndex) {
		Color32[,] realColors = ScreenshotUtils.GetColorsOnScreen(801 + shortcutIndex * 130, 556, 54, 54);
		if (ApproximatelyRect(realColors, ENERGY_BOTTLE_BIG) > 0.6F) {
			return EnergyShortcutAddingType.BIG_BOTTLE;
		} else if (ApproximatelyRect(realColors, ENERGY_BOTTLE_SMALL) > 0.6F) {
			return EnergyShortcutAddingType.SMALL_BOTTLE;
		} else if (ApproximatelyRect(realColors, ENERGY_DIAMOND_BUY) > 0.6F) {
			return EnergyShortcutAddingType.DIAMOND_BUY;
		} else {
			// ScreenshotUtils.Screenshot(801 + shortcutIndex * 130, 556, 54, 54, Application.dataPath + $"/PersistentData/Textures/Test{shortcutIndex}.png");
			return EnergyShortcutAddingType.NONE;
		}
	}
	public static List<EnergyShortcutAddingType> GetShortcutTypes() {
		List<EnergyShortcutAddingType> list = new List<EnergyShortcutAddingType>();
		for (int i = 0; i < 3; ++i) {
			list.Add(GetShortcutType(i));
		}
		return list;
	}
}
