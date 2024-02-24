/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using UnityEngine;

public static partial class Recognize {
	public enum Scene {
		INSIDE,
		OUTSIDE,
		FIGHTING,
	}

	public static Scene CurrentScene {
		get {
			return GetCachedValueOrNew(nameof(CurrentScene), () => {
				// 左上角蓝色返回按钮存在，说明处于出战界面
				if (ApproximatelyCoveredCount(Operation.GetColorOnScreen(50, 130), new Color32(94, 126, 202, 255)) >= 0) {
					return Scene.FIGHTING;
				}
				// 右下角一排按钮里的雷达按钮存在，说明处于世界界面
				// 处于搜索界面，是没有整排按钮的，但还是处于世界界面
				else if (ApproximatelyCoveredCount(Operation.GetColorOnScreen(1850, 540), new Color32(69, 146, 221, 255)) >= 0 ||
						ApproximatelyCoveredCount(Operation.GetColorOnScreen(1850, 617), new Color32(69, 146, 221, 255)) < 0) {
					return Scene.OUTSIDE;
				}
				return Scene.INSIDE;
			});
		}
	}

	public static bool IsOutsideFaraway => GetCachedValueOrNew(nameof(IsOutsideFaraway), () => 
			ApproximatelyCoveredCount(Operation.GetColorOnScreen(170, 164), new Color32(56, 124, 205, 255)) >= 0);

	public static bool IsOutsideNearby => GetCachedValueOrNew(nameof(IsOutsideNearby), () => 
			ApproximatelyCoveredCount(Operation.GetColorOnScreen(170, 240), new Color32(56, 124, 205, 255)) >= 0);
	
	public static bool? IsMiniMapShowing {
		get {
			return GetCachedValueOrNew<bool?>(nameof(IsMiniMapShowing), () => {
				int deltaY = IsOutsideNearby ? 76 : IsOutsideFaraway ? 0 : -1;
				if (deltaY != -1) {
					Color32[,] realColors = Operation.GetColorsOnScreen(20, 150 + deltaY, 12, 20);
					Color32 targetColor = new Color32(191, 191, 191, 255);
					if (ApproximatelyCoveredCount(realColors[2, 9], targetColor) >= 0 &&
							ApproximatelyCoveredCount(realColors[11, 9], targetColor) >= 0) {
						return true;
					}
					if (ApproximatelyCoveredCount(realColors[10, 7], targetColor) >= 0 &&
							ApproximatelyCoveredCount(realColors[10, 16], targetColor) >= 0) {
						return false;
					}
				}
				return null;
			});
		}
	}

	private static readonly Color32[,] AREA_BUFF = Operation.GetFromFile("PersistentData/Textures/AreaBuff.png");
	public static bool IsInEightArea {
		get {
			switch (CurrentScene) {
				case Scene.OUTSIDE:
					// 通过判断战区buff是否存在确定是否在八国地图
					Color32[,] realColors = Operation.GetColorsOnScreen(226, 184, 12, 12);
					return ApproximatelyRect(realColors, AREA_BUFF) > 0.99F;
			}
			return false;
		}
	}
	private static readonly Color32[,] GOTO_EIGHT_AREA = Operation.GetFromFile("PersistentData/Textures/GotoEightArea.png");
	public static bool CanGotoEightArea {
		get {
			// 通过在八国活动页面判断右下角是否有“前往”按钮，确定是否可以前往八国地图
			Color32[,] realColors = Operation.GetColorsOnScreen(1147, 976, 36, 18);
			return ApproximatelyRect(realColors, GOTO_EIGHT_AREA) > 0.99F;
		}
	}

	// 当前是否处于搜索面板
	public static bool IsSearching => Approximately(Operation.GetColorOnScreen(960, 466), new Color32(119, 131, 184, 255));

	public static bool IsMarshalExist {
		get {
			// 当前是否存在元帅
			Color32[,] realColors = Operation.GetColorsOnScreen(790, 780, 21, 41);
			Color32 targetColor1 = new Color32(140, 17, 15, 255);
			Color32 realColor1 = realColors[2, 5];
			Color32 targetColor2 = new Color32(243, 215, 16, 255);
			Color32 realColor2 = realColors[5, 10];
			Color32 targetColor3 = new Color32(243, 210, 155, 255);
			Color32 realColor3 = realColors[0, 30];
			Color32 targetColor4 = new Color32(79, 118, 174, 255);
			Color32 realColor4 = realColors[10, 40];
			Color32 targetColor5 = new Color32(212, 31, 28, 255);
			Color32 realColor5 = realColors[20, 35];
			return Approximately(realColor1, targetColor1) ||
					Approximately(realColor2, targetColor2) ||
					Approximately(realColor3, targetColor3) ||
					Approximately(realColor4, targetColor4) ||
					Approximately(realColor5, targetColor5);
		}
	}

	private static readonly Color32[,] QUICK_FIX = Operation.GetFromFile("PersistentData/Textures/QuickFix.png");
	public static int IsQuickFixExist {
		get {
			return GetCachedValueOrNew(nameof(IsQuickFixExist), () => {
				if (ApproximatelyRect(Operation.GetColorsOnScreen(774, 786, 37, 37), QUICK_FIX) > 0.7F) {
					return 1;
				} else if (ApproximatelyRect(Operation.GetColorsOnScreen(832, 786, 37, 37), QUICK_FIX) > 0.7F) {
					return 2;
				} else if (ApproximatelyRect(Operation.GetColorsOnScreen(900, 786, 37, 37), QUICK_FIX) > 0.7F) {
					return 3;
				} else {
					return 0;
				}
			});
		}
	}

	private static readonly Color32[,] FIX_ALL = Operation.GetFromFile("PersistentData/Textures/FixAll.png");
	public static bool CanFixAll =>
			ApproximatelyRect(Operation.GetColorsOnScreen(803, 948, 96, 26), FIX_ALL, 1.5F) > 0.7F ||
			ApproximatelyRect(Operation.GetColorsOnScreen(913, 948, 96, 26), FIX_ALL, 1.5F) > 0.7F;

	private static readonly Color32[,] GATHER_FEAR_STAR = Operation.GetFromFile("PersistentData/Textures/GatherFearStar.png");
	public static bool IsGatherFearStar {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(901, 810, 51, 22);
			return ApproximatelyRectIgnoreCovered(realColors, GATHER_FEAR_STAR, 1.05F) > 0.9F;
		}
	}

	private static readonly Color32[,] CUT_PRICE = Operation.GetFromFile("PersistentData/Textures/CutPrice.png");
	public static bool CanCutPrice {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(930, 870, 60, 22);
			return ApproximatelyRect(realColors, CUT_PRICE) > 0.99F;
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
	
	// public static readonly Vector2Int[] PROP_ICON_SAMPLE_POINTS = {
	// 	new Vector2Int(20, 20), new Vector2Int(42, 20), new Vector2Int(65, 20),
	// 	new Vector2Int(20, 42), new Vector2Int(42, 42), new Vector2Int(65, 42),
	// 	new Vector2Int(20, 65),	// 右下角有数量显示，不能作为判断依据
	// 	new Vector2Int(12, 73), // 用于判断背景是什么颜色
	// };
}
