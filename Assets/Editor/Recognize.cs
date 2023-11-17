/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using UnityEngine;

public static partial class Recognize {
	public static Rect GAME_RECT = new Rect(0, 101, 1920, 915);
	
	public enum Scene {
		INSIDE,
		OUTSIDE,
		ARMY_SELECTING,
	}

	public static Scene CurrentScene {
		get {
			// 左上角蓝色返回按钮存在，说明处于出战界面
			if (ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(50, 130), new Color32(94, 126, 202, 255)) >= 0) {
				return Scene.ARMY_SELECTING;
			}
			// 右下角一排按钮里的雷达按钮存在，说明处于世界界面
			if (ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(1850, 540), new Color32(69, 146, 221, 255)) >= 0) {
				return Scene.OUTSIDE;
			}
			return Scene.INSIDE;
		}
	}

	private static readonly Color32[,] NETWORK_DISCONNECTED = ScreenshotUtils.GetFromFile("PersistentData/Textures/NetworkDisconnected.png");
	public static bool IsNetworkDisconnected {
		get {
			Color32[,] realColors = ScreenshotUtils.GetColorsOnScreen(910, 440, 100, 26);
			return ApproximatelyRect(realColors, NETWORK_DISCONNECTED) > 0.99F;
		}
	}

	public static bool IsWindowCovered {
		get {
			switch (CurrentScene) {
				case Scene.ARMY_SELECTING:
					// 左上角返回按钮颜色很暗
					return !Approximately(ScreenshotUtils.GetColorOnScreen(50, 130), new Color32(94, 126, 202, 255));
				case Scene.INSIDE:
				case Scene.OUTSIDE:
					// 右下角一排按钮颜色很暗
					return !Approximately(ScreenshotUtils.GetColorOnScreen(1850, 620), new Color32(69, 146, 221, 255));
			}
			return false;
		}
	}

	public static bool IsWindowNoCovered {
		get {
			switch (CurrentScene) {
				case Scene.ARMY_SELECTING:
					// 左上角返回按钮颜色不暗
					return Approximately(ScreenshotUtils.GetColorOnScreen(50, 130), new Color32(94, 126, 202, 255));
				case Scene.INSIDE:
				case Scene.OUTSIDE:
					// 右下角一排按钮颜色不暗
					return Approximately(ScreenshotUtils.GetColorOnScreen(1850, 620), new Color32(69, 146, 221, 255));
			}
			return false;
		}
	}

	public static bool IsOutsideFaraway => ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(170, 164), new Color32(56, 124, 205, 255)) >= 0;

	public static bool IsOutsideNearby => ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(170, 240), new Color32(56, 124, 205, 255)) >= 0;

	private static readonly Color32[,] AREA_BUFF = ScreenshotUtils.GetFromFile("PersistentData/Textures/AreaBuff.png");
	public static bool IsInEightArea {
		get {
			switch (CurrentScene) {
				case Scene.OUTSIDE:
					// 通过判断战区buff是否存在确定是否在八国地图
					Color32[,] realColors = ScreenshotUtils.GetColorsOnScreen(226, 184, 12, 12);
					return ApproximatelyRect(realColors, AREA_BUFF) > 0.99F;
			}
			return false;
		}
	}
	private static readonly Color32[,] GOTO_EIGHT_AREA = ScreenshotUtils.GetFromFile("PersistentData/Textures/GotoEightArea.png");
	public static bool CanGotoEightArea {
		get {
			// 通过在八国活动页面判断右下角是否有“前往”按钮，确定是否可以前往八国地图
			Color32[,] realColors = ScreenshotUtils.GetColorsOnScreen(1147, 976, 36, 18);
			return ApproximatelyRect(realColors, GOTO_EIGHT_AREA) > 0.99F;
		}
	}
	
	// 当前是否处于搜索面板
	public static bool IsSearching => Approximately(ScreenshotUtils.GetColorOnScreen(960, 466), new Color32(119, 131, 184, 255));

	public static bool IsMarshalExist {
		get {
			// 当前是否存在元帅
			Color32 targetColor1 = new Color32(140, 17, 15, 255);
			Color32 realColor1 = ScreenshotUtils.GetColorOnScreen(792, 785);
			Color32 targetColor2 = new Color32(243, 215, 16, 255);
			Color32 realColor2 = ScreenshotUtils.GetColorOnScreen(795, 790);
			Color32 targetColor3 = new Color32(243, 210, 155, 255);
			Color32 realColor3 = ScreenshotUtils.GetColorOnScreen(790, 810);
			Color32 targetColor4 = new Color32(79, 118, 174, 255);
			Color32 realColor4 = ScreenshotUtils.GetColorOnScreen(800, 820);
			Color32 targetColor5 = new Color32(212, 31, 28, 255);
			Color32 realColor5 = ScreenshotUtils.GetColorOnScreen(810, 815);
			return Approximately(realColor1, targetColor1) ||
					Approximately(realColor2, targetColor2) ||
					Approximately(realColor3, targetColor3) ||
					Approximately(realColor4, targetColor4) ||
					Approximately(realColor5, targetColor5);
		}
	}

	private static readonly Color32[,] LEAGUE_MECHA_DONATE_ENABLED = ScreenshotUtils.GetFromFile("PersistentData/Textures/LeagueMechaDonateEnabled.png");
	public static bool IsLeagueMechaDonateEnabled {
		get {
			Color32[,] realColors = ScreenshotUtils.GetColorsOnScreen(1067, 929, 16, 16);
			return ApproximatelyRect(realColors, LEAGUE_MECHA_DONATE_ENABLED) > 0.99F;
		}
	}

	private static readonly Color32[,] LEAGUE_MECHA_DONATE_IN_RANK = ScreenshotUtils.GetFromFile("PersistentData/Textures/LeagueMechaDonateInRank.png");
	public static bool IsLeagueMechaDonateInRank {
		get {
			// 通过判断排行列表中最后一行的底色确定自己是否在排行中
			Color32[,] realColors = ScreenshotUtils.GetColorsOnScreen(780, 850, 10, 10);
			return ApproximatelyRect(realColors, LEAGUE_MECHA_DONATE_IN_RANK) > 0.99F;
		}
	}

	private static readonly Color32[,] LEAGUE_MECHA_DONATE_CONFIRM = ScreenshotUtils.GetFromFile("PersistentData/Textures/LeagueMechaDonateConfirm.png");
	public static bool IsLeagueMechaDonateConfirming {
		get {
			Color32[,] realColors = ScreenshotUtils.GetColorsOnScreen(860, 340, 200, 30);
			return ApproximatelyRect(realColors, LEAGUE_MECHA_DONATE_CONFIRM) > 0.99F;
		}
	}
	
	public static readonly Vector2Int[] PROP_ICON_SAMPLE_POINTS = {
		new Vector2Int(20, 20), new Vector2Int(42, 20), new Vector2Int(65, 20),
		new Vector2Int(20, 42), new Vector2Int(42, 42), new Vector2Int(65, 42),
		new Vector2Int(20, 65),	// 右下角有数量显示，不能作为判断依据
		new Vector2Int(12, 73), // 用于判断背景是什么颜色
	};
}
