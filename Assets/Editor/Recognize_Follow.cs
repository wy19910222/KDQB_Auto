﻿/*
 * @Author: wangyun
 * @CreateTime: 2023-10-18 01:59:04 226
 * @LastEditor: wangyun
 * @EditTime: 2023-10-18 01:59:04 232
 */

using System.Collections.Generic;
using UnityEngine;

public static partial class Recognize {
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
	
	public static bool ColorsEquals(IList<Color32> feature1, IList<Color32> feature2) {
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
}