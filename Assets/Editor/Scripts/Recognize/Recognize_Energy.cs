﻿/*
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
	public static int ENERGY_FULL = 95;
	
	private const int ENERGY_EMPTY_X = 21;
	private const int ENERGY_FULL_X = 116;
	// private const int ENERGY_Y = 127;
	// private static readonly Color32 ENERGY_TARGET_COLOR = new Color32(194, 226, 62, 255);
	private const int ENERGY_Y = 113;
	private static readonly Color32 ENERGY_TARGET_COLOR = new Color32(197, 237, 100, 255);
	public static int energy {
		get {
			return GetCachedValueOrNew(nameof(energy), () => {
				int deltaX = IsOutsideNearby ? 80 : IsOutsideFaraway ? 0 : -1;
				if (deltaX >= 0) {
					const int width = ENERGY_FULL_X - ENERGY_EMPTY_X;
					Color32[,] colors = Operation.GetColorsOnScreen(ENERGY_EMPTY_X + deltaX, ENERGY_Y, width + 1, 1);
					// 最少只能判断到x=19，再继续会受到体力图标的影响
					for (int x = colors.GetLength(0) - 1; x >= 0; --x) {
						if (ApproximatelyCoveredCount(colors[x, 0], ENERGY_TARGET_COLOR, 0.4F) >= 0) {
							return Mathf.RoundToInt((float) x / width * (ENERGY_FULL - ENERGY_EMPTY) + ENERGY_EMPTY);
						}
					}
					// 10以下误差会比较大，最少只能判断到x=11
					if (WindowCoveredCount >= 0) {
						float threshold = 420 * COVER_COEFFICIENT_DICT[WindowCoveredCount];
						for (int x = 20; x >= 11; --x) {
							Color32 c = colors[x, 0];
							if (c.r + c.g + c.b > threshold) {
								return Mathf.RoundToInt((float) (x - 3) / width * (ENERGY_FULL - ENERGY_EMPTY) + ENERGY_EMPTY);
							}
						}
					}
				}
				return ENERGY_EMPTY;
			});
		}
	}

	private static readonly Color32[,] ENERGY_SHORTCUT_ADDING = Operation.GetFromFile("PersistentData/Textures/EnergyShortcutAdding.png");
	public static bool IsEnergyShortcutAdding {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(910, 370, 100, 26);
			return ApproximatelyRect(realColors, ENERGY_SHORTCUT_ADDING) > 0.99F;
		}
	}

	private static readonly Color32[,] ENERGY_BOTTLE_BIG = Operation.GetFromFile("PersistentData/Textures/EnergyBottleBig.png");
	private static readonly Color32[,] ENERGY_BOTTLE_SMALL = Operation.GetFromFile("PersistentData/Textures/EnergyBottleSmall.png");
	private static readonly Color32[,] ENERGY_DIAMOND_BUY = Operation.GetFromFile("PersistentData/Textures/EnergyDiamondBuy.png");
	public static EnergyShortcutAddingType GetShortcutType(int shortcutIndex) {
		Color32[,] realColors = Operation.GetColorsOnScreen(801 + shortcutIndex * 130, 556, 54, 54);
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
