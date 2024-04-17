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
			return realColor.r > realColor.g + realColor.g + realColor.b + realColor.b;
		}
	}

	public static bool DailySuppliesIsNew {
		get {
			Color32 realColor = Operation.GetColorOnScreen(786, 367);
			return Approximately(realColor, new Color32(222, 57, 0, 255));
		}
	}

	public static bool DiscountPacksIsNew {
		get {
			Color32 realColor = Operation.GetColorOnScreen(988, 168);
			return Approximately(realColor, new Color32(222, 57, 0, 255));
		}
	}
	
	public static int DiscountPacksProgress {
		get {
			Color32 targetColor = new Color32(83, 150, 255, 255);
			Color32[,] colors = Operation.GetColorsOnScreen(800, 404, 350, 1);
			// 最少只能判断到x=19，再继续会受到体力图标的影响
			for (int i = 0; i < 9; ++i) {
				if (!Approximately(colors[31+ Mathf.FloorToInt(i * 39.2F), 0], targetColor)) {
					return i;
				}
			}
			return 9;
		}
	}

	public static bool MoreIsNew {
		get {
			Color32 realColor = Operation.GetColorOnScreen(1204, 168);
			return Approximately(realColor, new Color32(222, 57, 0, 255));
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
