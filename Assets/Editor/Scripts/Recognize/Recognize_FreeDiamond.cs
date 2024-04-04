/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using UnityEngine;

public static partial class Recognize {
	public static bool MallIsNew {
		get {
			Color32 realColor = Operation.GetColorOnScreen(1794, 110);
			return Approximately(realColor, new Color32(225, 64, 12, 255));
		}
	}

	public static bool DailySuppliesIsNew {
		get {
			Color32 realColor = Operation.GetColorOnScreen(786, 367);
			return Approximately(realColor, new Color32(221, 57, 0, 255));
		}
	}
	
	private static readonly Color32[,] FREE_DIAMOND_COOL_DOWN = Operation.GetFromFile("PersistentData/Textures/FreeDiamondCoolDown.png");
	public static bool IsFreeDiamondCoolDown {
		get {
			const int WIDTH = 55, HEIGHT = 30;
			const int OFFSET_MAX = -5;
			Color32[,] realColors = Operation.GetColorsOnScreen(1099, 295 + Mathf.Min(OFFSET_MAX, 0), WIDTH, HEIGHT + Mathf.Abs(OFFSET_MAX));
			int sign = Mathf.RoundToInt(Mathf.Sign(OFFSET_MAX));
			for (int i = 0; i < Mathf.Abs(OFFSET_MAX); i++) {
				Color32[,] _realColors = new Color32[WIDTH, HEIGHT];
				for (int x = 0; x < WIDTH; x++) {
					for (int y = 0; y < HEIGHT; y++) {
						_realColors[x, y] = realColors[x, y - Mathf.Min(OFFSET_MAX, 0) + i * sign];
					}
				}
				if (ApproximatelyRect(_realColors, FREE_DIAMOND_COOL_DOWN) > 0.9F) {
					return true;
				}
			}
			return false;
		}
	}

	private static readonly Color32[,] FREE_DIAMOND_NO_COUNTDOWN = Operation.GetFromFile("PersistentData/Textures/FreeDiamondNoCountdown.png");
	public static bool IsFreeDiamondNoCountdown {
		get {
			const int WIDTH = 58, HEIGHT = 15;
			const int OFFSET_MAX = -5;
			Color32[,] realColors = Operation.GetColorsOnScreen(1096, 261 + Mathf.Min(OFFSET_MAX, 0), WIDTH, HEIGHT + Mathf.Abs(OFFSET_MAX));
			int sign = Mathf.RoundToInt(Mathf.Sign(OFFSET_MAX));
			for (int i = 0; i < Mathf.Abs(OFFSET_MAX); i++) {
				Color32[,] _realColors = new Color32[WIDTH, HEIGHT];
				for (int x = 0; x < WIDTH; x++) {
					for (int y = 0; y < HEIGHT; y++) {
						_realColors[x, y] = realColors[x, y - Mathf.Min(OFFSET_MAX, 0) + i * sign];
					}
				}
				if (ApproximatelyRect(_realColors, FREE_DIAMOND_NO_COUNTDOWN) > 0.9F) {
					return true;
				}
			}
			return false;
		}
	}
}
