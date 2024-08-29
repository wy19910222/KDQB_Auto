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
	
	public static int ENERGY_FULL = 95;
	
	// private const int ENERGY_EMPTY_X = 21;
	// private const int ENERGY_FULL_X = 116;
	// private const int ENERGY_Y = 127;
	// private static readonly Color32 ENERGY_TARGET_COLOR = new Color32(194, 226, 62, 255);
	// private const int ENERGY_Y = 113;
	// private static readonly Color32 ENERGY_TARGET_COLOR = new Color32(197, 237, 100, 255);
	// private static readonly Color32 ENERGY_TARGET_COLOR = new Color32(253, 248, 83, 255);
	private const int ENERGY_EMPTY_X = 24;	// 包含
	private const int ENERGY_FULL_X = 119;	// 不包含
	private const int ENERGY_Y = 129;
	public static int Energy {
		get {
			return GetCachedValueOrNew(nameof(Energy), () => {
				if (EnergyAreaDeltaX >= 0) {
					const int energyWidth = ENERGY_FULL_X - ENERGY_EMPTY_X;
					const int detectWidth = energyWidth + 1;	// 多取1个像素进行左右对比灰度
					Color32[,] colors = Operation.GetColorsOnScreen(EnergyAreaDeltaX + ENERGY_EMPTY_X, ENERGY_Y, detectWidth, 1);
					// 最少只能判断到x=19，再继续会受到体力图标的影响
					if (WindowCoveredCount >= 0) {
						float grayThreshold = 100 * COVER_COEFFICIENT_DICT[WindowCoveredCount];
						for (int x = colors.GetLength(0) - 1; x > 10; --x) {
							float rightGray = colors[x, 0].GrayScale();
							float currentGray = colors[x - 1, 0].GrayScale();
							if (currentGray > grayThreshold && currentGray / rightGray > 1.2F) {
								return Mathf.RoundToInt((float) x / energyWidth * Global.ENERGY_FULL);
							}
						}
					}
					return EnergyOCR;
				}
				return 0;
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
	
	public static int EnergyOCR {
		get {
			return GetCachedValueOrNew(nameof(EnergyOCR), () => {
				if (EnergyAreaDeltaX >= 0) {
					string str = Operation.GetTextOnScreenNew(60 + EnergyAreaDeltaX, 111, 39, 20, false, 1, color => {
							float threshold = WindowCoveredCount >= 0 ? 240 * COVER_COEFFICIENT_DICT[WindowCoveredCount] : 0;
							return color.r > threshold && color.g > threshold && color.b > threshold;
					});
					if (int.TryParse(str, out int result)) {
						return result;
					}
				}
				return 0;
			});
		}
	}
	
	private static int EnergyAreaDeltaX => GetCachedValueOrNew(
			nameof(EnergyAreaDeltaX),
			() => CurrentScene switch {
				Scene.OUTSIDE_NEARBY => 80,
				Scene.OUTSIDE_FARAWAY => 0,
				_ => -1
			}
	);
}
