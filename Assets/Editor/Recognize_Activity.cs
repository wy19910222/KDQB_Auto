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
}
