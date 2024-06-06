/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using System.Collections.Generic;
using UnityEngine;

public static partial class Recognize {

	private static readonly Color32[,] MINING_BTN = Operation.GetFromFile("PersistentData/Textures/MiningBtn.png");
	public static bool IsMiningTycoon {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(1035, 955, 55, 28);
			return ApproximatelyRectIgnoreCovered(realColors, MINING_BTN) > 0.9F;
		}
	}

	private static readonly Color32[,] DEEP_SEA_BTN = Operation.GetFromFile("PersistentData/Textures/DeepSeaBtn.png");
	public static bool IsDeepSea {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(801, 240, 80, 22);
			return ApproximatelyRectIgnoreCovered(realColors, DEEP_SEA_BTN) > 0.9F;
			}
	}

	private static readonly Color32[,] CUT_PRICE = Operation.GetFromFile("PersistentData/Textures/CutPrice.png");
	public static bool CanCutPrice {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(930, 870, 60, 22);
			return ApproximatelyRect(realColors, CUT_PRICE) > 0.99F;
		}
	}

	private static readonly Color32[,] Refugee_USE_SOS_BTN = Operation.GetFromFile("PersistentData/Textures/RefugeeUseSOSBtn.png");
	private static readonly Color32[,] Refugee_GOTO_BTN = Operation.GetFromFile("PersistentData/Textures/RefugeeGotoBtn.png");
	public static int CanUseSOS {
		get {
			Color32[,] realColors1 = Operation.GetColorsOnScreen(1099, 938, 41, 36);
			if (ApproximatelyRect(realColors1, Refugee_GOTO_BTN) > 0.9F) {
				return 3;
			}
			if (ApproximatelyRect(realColors1, Refugee_USE_SOS_BTN) > 0.7F) {
				return 1;
			}
			Color32[,] realColors2 = Operation.GetColorsOnScreen(1099, 850, 41, 36);
			if (ApproximatelyRect(realColors2, Refugee_USE_SOS_BTN) > 0.7F) {
				return 2;
			}
			return 0;
		}
	}

	private static readonly Color32[,] SOS_PROP_ICON = Operation.GetFromFile("PersistentData/Textures/SOSPropIcon.png");
	public static bool IsSOSExist {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(738, 260, 60, 50);
			return ApproximatelyRect(realColors, SOS_PROP_ICON) > 0.9F;
		}
	}

	private static readonly Color32[,] MINING_TRUCK_0 = Operation.GetFromFile("PersistentData/Textures/MiningTruck0.png");
	private static readonly Color32[,] MINING_TRUCK_1 = Operation.GetFromFile("PersistentData/Textures/MiningTruck1.png");
	private static readonly Color32[,] MINING_TRUCK_4 = Operation.GetFromFile("PersistentData/Textures/MiningTruck4.png");
	private static readonly Color32[,] MINING_TRUCK_8 = Operation.GetFromFile("PersistentData/Textures/MiningTruck8.png");
	private static readonly Color32[,] MINING_TRUCK_24 = Operation.GetFromFile("PersistentData/Textures/MiningTruck24.png");
	public static int GetMiningTruckType(int shortcutIndex) {
		Color32[,] realColors = Operation.GetColorsOnScreen(775 + shortcutIndex * 118, 796, 40, 52);
		if (ApproximatelyRect(realColors, MINING_TRUCK_1) > 0.6F) {
			return 1;
		} else if (ApproximatelyRect(realColors, MINING_TRUCK_4) > 0.6F) {
			return 4;
		} else if (ApproximatelyRect(realColors, MINING_TRUCK_8) > 0.6F) {
			return 8;
		} else if (ApproximatelyRect(realColors, MINING_TRUCK_24) > 0.6F) {
			return 24;
		} else {
			Color32[,] realColors1 = Operation.GetColorsOnScreen(766 + shortcutIndex * 118, 820, 40, 40);
			if (ApproximatelyRect(realColors1, MINING_TRUCK_0, 1.5F) > 0.6F) {
				return 0;
			} else {
				return -1;
			}
		}
	}
	public static List<int> GetMiningTruckTypes() {
		List<int> list = new List<int>();
		for (int i = 0; i < 4; ++i) {
			list.Add(GetMiningTruckType(i));
		}
		return list;
	}
	
	// 从1开始
	public static int CurrentCapsuleToyStar {
		get {
			int level = 7;
			while (level >= 0) {
				Color32 realColor = Operation.GetColorOnScreen(Mathf.RoundToInt(879.1F + level * 20.167F), 823);
				float value = (float) (realColor.r + realColor.g) / realColor.b;
				if (value > 4F) {
					return level;
				}
				--level;
			}
			return -1;
		}
	}
}
