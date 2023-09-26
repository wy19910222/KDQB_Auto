/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using System.Collections.Generic;
using UnityEngine;

public static class Recognize {
	public enum Scene {
		INSIDE,
		OUTSIDE,
		ARMY_SELECTING,
	}

	public static Scene CurrentScene {
		get {
			// 左上角蓝色返回按钮存在，说明处于出战界面
			if (Approximately(ScreenshotUtils.GetColorOnScreen(50, 130), new Color32(94, 126, 202, 255), 10)) {
				return Scene.ARMY_SELECTING;
			}
			// 右下角一排按钮里的雷达按钮存在，说明处于世界界面
			Color32 radarBtnColor = ScreenshotUtils.GetColorOnScreen(1850, 540);
			if (Approximately(radarBtnColor, new Color32(69, 146, 221, 255), 10) ||	// 无界面覆盖
					Approximately(radarBtnColor, new Color32(21, 44, 66, 255), 10) ||	// 联盟背包等窗口覆盖
					Approximately(radarBtnColor, new Color32(6, 13, 20, 255), 10)) {	// 曙光活动主界面（或双层窗口）覆盖
				// // 左上角蓝色小地图按钮（远）存在，说明处于世界界面（远）
				// if (Approximately(ScreenshotUtils.GetColorOnScreen(170, 164), new Color32(17, 37, 61, 255), 10)) {
				// 	return Scene.OUTSIDE_FAR;
				// }
				// // 左上角蓝色小地图按钮（近）存在，说明处于世界界面（近）
				// else if (Approximately(ScreenshotUtils.GetColorOnScreen(170, 240), new Color32(17, 37, 61, 255), 10)) {
				// 	return Scene.OUTSIDE_NEAR;
				// }
				return Scene.OUTSIDE;
			}
			return Scene.INSIDE;
		}
	}

	public static bool IsWindowCovered {
		get {
			// 右下角一排按钮颜色很暗
			Color32 btnColor = ScreenshotUtils.GetColorOnScreen(1850, 620);
			return Approximately(btnColor, new Color32(20, 43, 66, 255), 10) ||	// 联盟背包等窗口覆盖
					Approximately(btnColor, new Color32(6, 13, 20, 255), 10);	// 曙光活动主界面（或双层窗口）（或双层窗口）覆盖
		}
	}

	public static bool IsWindowNoCovered {
		get {
			// 右下角一排按钮颜色很暗
			Color32 btnColor = ScreenshotUtils.GetColorOnScreen(1850, 620);
			return Approximately(btnColor, new Color32(69, 146, 221, 255), 10); // 无界面覆盖
		}
	}

	public static bool IsOutsideFaraway {
		get {
			Color32 minimapBtnColor = ScreenshotUtils.GetColorOnScreen(170, 164);
			return Approximately(minimapBtnColor, new Color32(56, 124, 205, 255), 10) ||	// 无界面覆盖
					Approximately(minimapBtnColor, new Color32(17, 37, 61, 255), 10) ||	// 联盟背包等窗口覆盖
					Approximately(minimapBtnColor, new Color32(5, 11, 18, 255), 10);	// 曙光活动主界面（或双层窗口）覆盖
		}
	}

	public static bool IsOutsideNearby {
		get {
			Color32 minimapBtnColor = ScreenshotUtils.GetColorOnScreen(170, 240);
			return Approximately(minimapBtnColor, new Color32(56, 124, 205, 255), 10) ||	// 无界面覆盖
					Approximately(minimapBtnColor, new Color32(17, 37, 61, 255), 10) ||	// 联盟背包等窗口覆盖
					Approximately(minimapBtnColor, new Color32(5, 11, 18, 255), 10);	// 曙光活动主界面（或双层窗口）覆盖
		}
	}

	public static int BusyGroupCount {
		get {
			int deltaY = IsOutsideNearby ? 76 : IsOutsideFaraway ? 0 : -1;
			if (deltaY >= 0) {
				int groupCount = 0;
				Color32 targetColor1 = new Color32(98, 135, 229, 255);	// 无界面覆盖
				Color32 targetColor2 = new Color32(29, 40, 68, 255);	// 联盟背包等窗口覆盖
				Color32 targetColor3 = new Color32(9, 12, 20, 255);	// 曙光活动主界面（或双层窗口）覆盖
				while (groupCount < 10) {
					Color32 realColor = ScreenshotUtils.GetColorOnScreen(145, 438 + deltaY + groupCount * 50);
					if (!Approximately(realColor, targetColor1, 10) &&
							!Approximately(realColor, targetColor2, 10) &&
							!Approximately(realColor, targetColor3, 10)) {
						break;
					}
					groupCount++;
				}
				return groupCount;
			}
			return int.MaxValue;
		}
	}

	public static bool IsSearching {
		get {
			// 当前是否处于搜索面板
			Color32 targetColor = new Color32(119, 131, 184, 255);
			Color32 realColor = ScreenshotUtils.GetColorOnScreen(960, 466);
			return Approximately(realColor, targetColor, 10);
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
			return Approximately(realColor1, targetColor1, 10) ||	// 小体图标
					Approximately(realColor2, targetColor2, 10) ||	// 大体图标
					Approximately(realColor3, targetColor3, 10);	// 使用按钮
		}
	}
	
	public static bool IsOrangeHeroInGroup() {
		int deltaY = IsOutsideNearby ? 76 : IsOutsideFaraway ? 0 : -1;
		if (deltaY >= 0) {
			int groupCount = 0;
			Color32 targetColor1 = new Color32(98, 135, 229, 255);	// 无界面覆盖
			Color32 targetColor2 = new Color32(29, 40, 68, 255);	// 联盟背包等窗口覆盖
			Color32 targetColor3 = new Color32(9, 12, 20, 255);	// 曙光活动主界面（或双层窗口）覆盖
			while (groupCount < 10) {
				Color32 realColor = ScreenshotUtils.GetColorOnScreen(145, 438 + deltaY + groupCount * 50);
				if (!Approximately(realColor, targetColor1, 10) &&
						!Approximately(realColor, targetColor2, 10) &&
						!Approximately(realColor, targetColor3, 10)) {
					break;
				}
				Color32 frameColor = ScreenshotUtils.GetColorOnScreen(51, 419 + deltaY + groupCount * 50);
				if (frameColor.r > frameColor.g && frameColor.g > frameColor.b) {
					return true;
				}
				groupCount++;
			}
		}
		return false;
	}

	public static int GetYLKGroupNumber() {
		return GetHeroGroupNumber(AVATAR_YLK_FARAWAY, AVATAR_YLK_NEARBY);
	}
	public static readonly Vector2Int[] AVATAR_SAMPLE_POINTS = {
		new Vector2Int(7, 7), new Vector2Int(15, 7), new Vector2Int(23, 7),
		new Vector2Int(7, 15), new Vector2Int(15, 15), new Vector2Int(23, 15),
									new Vector2Int(15, 23),
	};
	public static readonly Color32[] AVATAR_YLK_FARAWAY = {
		new Color32(118, 120, 129, 255), new Color32(147, 142, 142, 255), new Color32(186, 187, 188, 255),
		new Color32(108, 103, 107, 255), new Color32(165, 138, 111, 255), new Color32(179, 166, 155, 255),
												new Color32(89, 87, 85, 255),  
	};
	public static readonly Color32[] AVATAR_YLK_NEARBY = {
		new Color32(122, 121, 129, 255), new Color32(151, 148, 149, 255), new Color32(210, 212, 210, 255),
		new Color32(113, 112, 118, 255), new Color32(167, 163, 160, 255), new Color32(159, 135, 134, 255),
												new Color32(123, 113, 104, 255),
	};
	public static int GetHeroGroupNumber(IReadOnlyList<Color32> heroAvatarFaraway, IReadOnlyList<Color32> heroAvatarNearby) {
		int deltaY = -1;
		IReadOnlyList<Color32> heroAvatar = null;
		if (IsOutsideNearby) {
			deltaY = 76;
			heroAvatar = heroAvatarNearby;
		} else if (IsOutsideFaraway) {
			deltaY = 0;
			heroAvatar = heroAvatarFaraway;
		}
		if (deltaY >= 0 && heroAvatar != null) {
			int groupCount = 0;
			Color32 targetColor1 = new Color32(98, 135, 229, 255);	// 无界面覆盖
			Color32 targetColor2 = new Color32(29, 40, 68, 255);	// 联盟背包等窗口覆盖
			Color32 targetColor3 = new Color32(9, 12, 20, 255);	// 曙光活动主界面（或双层窗口）覆盖
			while (groupCount < 10) {
				Color32 realColor = ScreenshotUtils.GetColorOnScreen(145, 438 + deltaY + groupCount * 50);
				if (!Approximately(realColor, targetColor1, 10) &&
						!Approximately(realColor, targetColor2, 10) &&
						!Approximately(realColor, targetColor3, 10)) {
					break;
				}
				int approximatelyCount = 0;
				int pointCount = AVATAR_SAMPLE_POINTS.Length;
				for (int i = 0; i < pointCount; ++i) {
					Vector2Int point = AVATAR_SAMPLE_POINTS[i];
					Vector2Int finalPoint = new Vector2Int(22 + point.x, 418 + deltaY + groupCount * 50 + point.y);
					int r = 0, g = 0, b = 0;
					for (int y = -1; y < 2; ++y) {
						Color32 c = ScreenshotUtils.GetColorOnScreen(finalPoint.x, finalPoint.y + y);
						r += c.r;
						g += c.g;
						b += c.b;
					}
					Color32 color = new Color32((byte) (r / 3), (byte) (g / 3), (byte) (b / 3), 255);
					if (Approximately(color, heroAvatar[i], 20)) {
						++approximatelyCount;
					}
				}
				if (approximatelyCount > pointCount * 0.75F) {
					return groupCount;
				}
				groupCount++;
			}
		}
		return -1;
	}

	public static bool JoinBtnExist {
		get {
			// 三个条件都满足才是灰色加号按钮
			Color32 targetColor1 = new Color32(148, 148, 155, 255);
			Color32 realColor1 = ScreenshotUtils.GetColorOnScreen(965, 305);
			Color32 targetColor2 = new Color32(170, 169, 171, 255);
			Color32 realColor2 = ScreenshotUtils.GetColorOnScreen(965, 280);
			Color32 targetColor3 = new Color32(197, 194, 192, 255);
			Color32 realColor3 = ScreenshotUtils.GetColorOnScreen(952, 297);
			return Approximately(realColor1, targetColor1, 10) &&
					Approximately(realColor2, targetColor2, 10) &&
					Approximately(realColor3, targetColor3, 10);
		}
	}

	public static bool HasJoined {
		get {
			//绿色已加入箭头存在，说明已加入
			Color32 targetColor1 = new Color32(147, 199, 167, 255);
			Color32 targetColor2 = new Color32(44, 89, 115, 255);
			Color32 realColor = ScreenshotUtils.GetColorOnScreen(790, 325);
			return Approximately(realColor, targetColor1, 10) ||
					Approximately(realColor, targetColor2, 10);
		}
	}

	private static bool Approximately(Color32 c1, Color32 c2, int threshold) {
		return Mathf.Abs(c1.r - c2.r) <= threshold &&
				Mathf.Abs(c1.g - c2.g) <= threshold &&
				Mathf.Abs(c1.b - c2.b) <= threshold;
	}
	
	// {0.4F, 0.65F}: 出征界面有个特殊弹窗很浅
	private static readonly Dictionary<float, float> COVER_COEFFICIENT_DICT = new() {
		{0, 1},
		{0.4F, 0.65F},
		{1, 0.299F},
		{2, 0.084F},
	};
	public static float ApproximatelyColoredCount(Color32 realColor, Color32 targetColor, float threshold = 0.1F) {
		foreach (var (coverCount, coefficient) in COVER_COEFFICIENT_DICT) {
			float targetR = targetColor.r * coefficient;
			float targetG = targetColor.g * coefficient;
			float targetB = targetColor.b * coefficient;
			float deltaR = Mathf.Abs(realColor.r / targetR - 1);
			float deltaG = Mathf.Abs(realColor.g / targetG - 1);
			float deltaB = Mathf.Abs(realColor.b / targetB - 1);
			// Debug.LogError($"第{pair.Key}层：{deltaR}, {deltaG}, {deltaB}");
			if (deltaR < threshold && deltaG < threshold && deltaB < threshold) {
				return coverCount;
			}
		}
		return -1;
	}
}
