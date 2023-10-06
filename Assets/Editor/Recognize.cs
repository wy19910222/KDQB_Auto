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

	public static bool IsWindowCovered {
		get {
			switch (CurrentScene) {
				case Scene.ARMY_SELECTING:
					// 左上角返回按钮颜色很暗
					return ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(50, 130), new Color32(94, 126, 202, 255)) > 0;
				case Scene.INSIDE:
				case Scene.OUTSIDE:
					// 右下角一排按钮颜色很暗
					return ApproximatelyCoveredCount(ScreenshotUtils.GetColorOnScreen(1850, 620), new Color32(69, 146, 221, 255)) > 0;
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

	public static int BusyGroupCount {
		get {
			int deltaY = IsOutsideNearby ? 76 : IsOutsideFaraway ? 0 : -1;
			if (deltaY >= 0) {
				int groupCount = 0;
				// 返回加速等蓝色按钮
				Color32 targetColor = new Color32(98, 135, 229, 255);
				while (groupCount < 10) {
					Color32 realColor = ScreenshotUtils.GetColorOnScreen(145, 438 + deltaY + groupCount * 50);
					// Debug.LogError($"groupCount: {groupCount}");
					if (ApproximatelyCoveredCount(realColor, targetColor) < 0) {
						break;
					}
					groupCount++;
				}
				return groupCount;
			}
			return int.MaxValue;
		}
	}

	private const int ENERGY_EMPTY = 0;
	private const int ENERGY_FULL = 75;
	private const int ENERGY_EMPTY_X = 21;
	private const int ENERGY_FULL_X = 116;
	private const int ENERGY_Y = 127;
	private static readonly Color32 ENERGY_TARGET_COLOR = new Color32(194, 226, 62, 255);
	public static int energy {
		get {
			int deltaX = IsOutsideNearby ? 80 : IsOutsideFaraway ? 0 : -1;
			if (deltaX >= 0) {
				const int width = ENERGY_FULL_X - ENERGY_EMPTY_X;
				Color32[,] colors = ScreenshotUtils.GetColorsOnScreen(ENERGY_EMPTY_X + deltaX, ENERGY_Y, width + 1, 1);
				for (int x = width; x >= 0; --x) {
					if (Approximately(colors[x, 0], ENERGY_TARGET_COLOR, 0.5F)) {
						return Mathf.RoundToInt((float) x / width * (ENERGY_FULL - ENERGY_EMPTY) + ENERGY_EMPTY);
					}
				}
			}
			return ENERGY_EMPTY;
		}
	}

	// 当前是否处于搜索面板
	public static bool IsSearching => Approximately(ScreenshotUtils.GetColorOnScreen(960, 466), new Color32(119, 131, 184, 255));

	public static bool IsEnergyAdding {
		get {
			// 当前是否处于嗑药面板
			Color32 targetColor1 = new Color32(255, 255, 108, 255);
			Color32 realColor1 = ScreenshotUtils.GetColorOnScreen(830, 590);
			Color32 targetColor2 = new Color32(255, 255, 108, 255);
			Color32 realColor2 = ScreenshotUtils.GetColorOnScreen(960, 590);
			Color32 targetColor3 = new Color32(254, 209, 51, 255);
			Color32 realColor3 = ScreenshotUtils.GetColorOnScreen(960, 702);
			return Approximately(realColor1, targetColor1) ||	// 小体图标
					Approximately(realColor2, targetColor2) ||	// 大体图标
					Approximately(realColor3, targetColor3);	// 使用按钮
		}
	}
	
	public static bool IsBigEnergy(RectInt rect) {
		Color32 targetColor1 = new Color32(255, 255, 108, 255);
		Color32 realColor1 = ScreenshotUtils.GetColorOnScreen(830, 590);
		Color32 targetColor2 = new Color32(255, 255, 108, 255);
		Color32 realColor2 = ScreenshotUtils.GetColorOnScreen(960, 590);
		Color32 targetColor3 = new Color32(254, 209, 51, 255);
		Color32 realColor3 = ScreenshotUtils.GetColorOnScreen(960, 702);
		return Approximately(realColor1, targetColor1) ||	// 小体图标
				Approximately(realColor2, targetColor2) ||	// 大体图标
				Approximately(realColor3, targetColor3);	// 使用按钮
	}
	
	public static bool IsOrangeHeroInGroup() {
		int deltaY = IsOutsideNearby ? 76 : IsOutsideFaraway ? 0 : -1;
		if (deltaY >= 0) {
			int groupCount = 0;
			// 返回加速等蓝色按钮
			Color32 targetColor = new Color32(98, 135, 229, 255);
			while (groupCount < 10) {
				Color32 realColor = ScreenshotUtils.GetColorOnScreen(145, 438 + deltaY + groupCount * 50);
				if (ApproximatelyCoveredCount(realColor, targetColor) < 0) {
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
			// 返回加速等蓝色按钮
			Color32 targetColor = new Color32(98, 135, 229, 255);
			while (groupCount < 10) {
				Color32 realColor = ScreenshotUtils.GetColorOnScreen(145, 438 + deltaY + groupCount * 50);
				if (ApproximatelyCoveredCount(realColor, targetColor) < 0) {
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
					// Color32 color = ScreenshotUtils.GetColorOnScreen(finalPoint.x, finalPoint.y);
					if (ApproximatelyCoveredCount(color, heroAvatar[i]) >= 0) {
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

	public static bool IsFollowOuterJoinBtnExist {
		get {
			if (CurrentScene == Scene.OUTSIDE) {
				// 三个条件都满足才是绿色加入按钮
				Color32 targetColor1 = new Color32(106, 212, 98, 255);
				Color32 realColor1 = ScreenshotUtils.GetColorOnScreen(1771, 700);
				Color32 targetColor2 = new Color32(94, 203, 91, 255);
				Color32 realColor2 = ScreenshotUtils.GetColorOnScreen(1746, 710);
				Color32 targetColor3 = new Color32(94, 203, 91, 255);
				Color32 realColor3 = ScreenshotUtils.GetColorOnScreen(1796, 710);
				return ApproximatelyCoveredCount(realColor1, targetColor1) >= 0 &&
						ApproximatelyCoveredCount(realColor2, targetColor2) >= 0 &&
						ApproximatelyCoveredCount(realColor3, targetColor3) >= 0;
			}
			return false;
		}
	}

	public static bool IsFollowJoinBtnExist {
		get {
			// 三个条件都满足才是灰色加号按钮
			Color32 targetColor1 = new Color32(148, 148, 155, 255);
			Color32 realColor1 = ScreenshotUtils.GetColorOnScreen(965, 305);
			Color32 targetColor2 = new Color32(170, 169, 171, 255);
			Color32 realColor2 = ScreenshotUtils.GetColorOnScreen(965, 280);
			Color32 targetColor3 = new Color32(197, 194, 192, 255);
			Color32 realColor3 = ScreenshotUtils.GetColorOnScreen(952, 297);
			return Approximately(realColor1, targetColor1) &&
					Approximately(realColor2, targetColor2) &&
					Approximately(realColor3, targetColor3);
		}
	}

	public static bool HasFollowJoined {
		get {
			//绿色已加入箭头存在，说明已加入
			Color32 targetColor1 = new Color32(147, 199, 167, 255);
			Color32 targetColor2 = new Color32(44, 89, 115, 255);
			Color32 realColor = ScreenshotUtils.GetColorOnScreen(790, 325);
			return Approximately(realColor, targetColor1) ||
					Approximately(realColor, targetColor2);
		}
	}

	public static bool IsFollowIconExist {
		get {
			// 判断车界面图标是否已出现
			Color32 targetColor = new Color32(118, 132, 169, 255);
			Color32 realColor = ScreenshotUtils.GetColorOnScreen(1107, 279);
			return !Approximately(realColor, targetColor);
		}
	}

	public static bool IsJDCanFollow {
		get {
			// 判断跟车界面图标是否是黑暗军团据点
			Color32 targetColor = new Color32(41, 39, 48, 255);
			Color32 realColor = ScreenshotUtils.GetColorOnScreen(1104, 236);
			return Approximately(realColor, targetColor);
		}
	}

	public static bool IsZCCanFollow {
		get {
			// 判断跟车界面图标是否是战锤
			Color32 targetColor = new Color32(253, 109, 106, 255);
			Color32 realColor = ScreenshotUtils.GetColorOnScreen(1142, 310);
			return Approximately(realColor, targetColor);
		}
	}

	public static bool IsNMYCanFollow {
		get {
			// 判断跟车界面图标是否是难民营
			Color32 targetColor = new Color32(74, 133, 159, 255);
			Color32 realColor = ScreenshotUtils.GetColorOnScreen(1116, 273);
			return Approximately(realColor, targetColor);
		}
	}

	public static bool IsAXPPCanFollow {
		get {
			// 判断跟车界面图标是否是爱心砰砰
			Color32 targetColor = new Color32(182, 75, 97, 255);
			Color32 realColor = ScreenshotUtils.GetColorOnScreen(1049, 321);
			return Approximately(realColor, targetColor);
		}
	}

	public static bool IsJWCanFollow {
		get {
			// 判断跟车界面图标是否是精卫
			Color32 targetColor = new Color32(158, 85, 92, 255);
			Color32 realColor = ScreenshotUtils.GetColorOnScreen(1113, 215);
			return Approximately(realColor, targetColor);
		}
	}

	public static bool IsJXCanFollow {
		get {
			// 判断跟车界面图标是否是惧星
			Color32 targetColor = new Color32(41, 131, 221, 255);
			Color32 realColor = ScreenshotUtils.GetColorOnScreen(1089, 213);
			return Approximately(realColor, targetColor);
		}
	}

	public static bool IsTooLateWindowExist {
		get {
			if (CurrentScene == Scene.ARMY_SELECTING) {
				// 判断出征界面是否弹出赶不上弹框
				// 暂时判断的是指定位置是否存在红色取消按钮
				Color32 targetColor = new Color32(211, 78, 56, 255);
				Color32 realColor = ScreenshotUtils.GetColorOnScreen(880, 700);
				return Approximately(realColor, targetColor);
			}
			return false;
		}
	}

	private static readonly RectInt s_OwnerAvatarRect = new RectInt(731, 190, 55, 54);	// 集结发起人头像范围
	private const int OWNER_AVATAR_FEATURE_XY_COUNT = 5; // 集结发起人头像特征获取的阵列边长
	public static Color32[] GetFollowOwnerAvatar() {
		int xStepLength = s_OwnerAvatarRect.width / OWNER_AVATAR_FEATURE_XY_COUNT;
		int offsetX = (s_OwnerAvatarRect.width - xStepLength * OWNER_AVATAR_FEATURE_XY_COUNT) >> 1;
		int yStepLength = s_OwnerAvatarRect.height / OWNER_AVATAR_FEATURE_XY_COUNT;
		int offsetY = (s_OwnerAvatarRect.height - yStepLength * OWNER_AVATAR_FEATURE_XY_COUNT) >> 1;
		int startX = s_OwnerAvatarRect.x + offsetX;
		int startY = s_OwnerAvatarRect.y + offsetY;
		Color32[] ownerAvatarFeature = new Color32[OWNER_AVATAR_FEATURE_XY_COUNT * OWNER_AVATAR_FEATURE_XY_COUNT];
		for (int y = 0; y < OWNER_AVATAR_FEATURE_XY_COUNT; ++y) {
			for (int x = 0; x < OWNER_AVATAR_FEATURE_XY_COUNT; ++x) {
				ownerAvatarFeature[y * OWNER_AVATAR_FEATURE_XY_COUNT + x] = ScreenshotUtils.GetColorOnScreen(startX + x, startY + y);
			}
		}
		return ownerAvatarFeature;
	}
	public static bool ApproximatelyBigAvatar(IList<Color32> feature1, IList<Color32> feature2) {
		if (feature1.Count != feature2.Count) {
			return false;
		}
		for (int i = 0, length = feature1.Count; i < length; ++i) {
			if (feature1[i].r != feature2[i].r || feature1[i].g != feature2[i].g || feature1[i].b != feature2[i].b) {
				return false;
			}
		}
		return true;
	}
	
	public static readonly Vector2Int[] PROP_ICON_SAMPLE_POINTS = {
		new Vector2Int(20, 20), new Vector2Int(42, 20), new Vector2Int(65, 20),
		new Vector2Int(20, 42), new Vector2Int(42, 42), new Vector2Int(65, 42),
		new Vector2Int(20, 65),	// 右下角有数量显示，不能作为判断依据
		new Vector2Int(12, 73), // 用于判断背景是什么颜色
	};

#region Base
	public static bool Approximately(Color32 realColor, Color32 targetColor, float thresholdMulti = 1) {
		float targetR = targetColor.r;
		float targetG = targetColor.g;
		float targetB = targetColor.b;
		float deltaR = Mathf.Abs(realColor.r / targetR - 1);
		float deltaG = Mathf.Abs(realColor.g / targetG - 1);
		float deltaB = Mathf.Abs(realColor.b / targetB - 1);
		// Debug.LogError($"{deltaR}, {deltaG}, {deltaB}");
		return deltaR < GetThreshold(targetR) * thresholdMulti &&
				deltaG < GetThreshold(targetG) * thresholdMulti &&
				deltaB < GetThreshold(targetB) * thresholdMulti;
	}
	// {0.4F, 0.65F}: 出征界面有个特殊弹窗很浅
	private static readonly Dictionary<float, float> COVER_COEFFICIENT_DICT = new() {
		{0, 1},
		{0.4F, 0.65F},
		{1, 0.299F},
		{2, 0.084F},
	};
	public static float ApproximatelyCoveredCount(Color32 realColor, Color32 targetColor, float thresholdMulti = 1) {
		foreach (var (coverCount, coefficient) in COVER_COEFFICIENT_DICT) {
			float targetR = targetColor.r * coefficient;
			float targetG = targetColor.g * coefficient;
			float targetB = targetColor.b * coefficient;
			// 当目标值为0，则1以内算相近，否则按比例
			float deltaR = targetR == 0 ? realColor.r <= 1 ? 0 : 1 : Mathf.Abs(realColor.r / targetR - 1);
			float deltaG = targetG == 0 ? realColor.g <= 1 ? 0 : 1 : Mathf.Abs(realColor.g / targetG - 1);
			float deltaB = targetB == 0 ? realColor.b <= 1 ? 0 : 1 : Mathf.Abs(realColor.b / targetB - 1);
			// Debug.LogError($"第{coverCount}层：{deltaR}, {deltaG}, {deltaB}");
			if (deltaR < GetThreshold(targetR) * thresholdMulti &&
					deltaG < GetThreshold(targetG) * thresholdMulti &&
					deltaB < GetThreshold(targetB) * thresholdMulti) {
				return coverCount;
			}
		}
		return -1;
	}
	public static float GetThreshold(float value) {
		return value switch {
			// > 100 => ((value - 100) * 0.03333F + 10) / value,
			> 100 => 0.03333F + 6.66667F / value,
			// > 10 => ((value - 10) * 0.07778F + 3) / value,
			> 10 => 0.07778F + 2.22222F / value,
			// > 0 => (value * 0.2F + 1) / value
			> 0 => 0.2F + 1 / value,
			_ => 1
		};
	}
#endregion
}
