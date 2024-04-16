/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

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
			return ApproximatelyRect(realColors, SOS_PROP_ICON) > 0.99F;
		}
	}
}
