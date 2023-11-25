/*
 * @Author: wangyun
 * @CreateTime: 2023-10-18 01:59:04 226
 * @LastEditor: wangyun
 * @EditTime: 2023-10-18 01:59:04 232
 */

using UnityEngine;

public static partial class Recognize {
	public enum FollowType {
		[InspectorName("未知")]
		UNKNOWN = -1,
		[InspectorName("无")]
		NONE,
		[InspectorName("战锤")]
		WAR_HAMMER,
		[InspectorName("难民营")]
		REFUGEE_CAMP,
		[InspectorName("惧星")]
		FEAR_STAR,
		[InspectorName("据点")]
		STRONGHOLD,
		[InspectorName("精卫")]
		ELITE_GUARD,
		[InspectorName("砰砰")]
		HEART_PANG
	}
	
	public static bool IsFollowOuterJoinBtnExist {
		get {
			if (CurrentScene == Scene.OUTSIDE) {
				// 三个条件都满足才是绿色加入按钮
				Color32[,] realColors = Operation.GetColorsOnScreen(1740, 700, 60, 11);
				Color32 targetColor1 = new Color32(106, 212, 98, 255);
				Color32 realColor1 = realColors[31, 0];
				Color32 targetColor2 = new Color32(94, 203, 91, 255);
				Color32 realColor2 = realColors[6, 10];
				Color32 targetColor3 = new Color32(94, 203, 91, 255);
				Color32 realColor3 = realColors[56, 10];
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
			Color32[,] realColors = Operation.GetColorsOnScreen(950, 280, 20, 30);
			Color32 targetColor1 = new Color32(148, 148, 155, 255);
			Color32 realColor1 = realColors[15, 25];
			Color32 targetColor2 = new Color32(170, 169, 171, 255);
			Color32 realColor2 = realColors[15, 0];
			Color32 targetColor3 = new Color32(197, 194, 192, 255);
			Color32 realColor3 = realColors[2, 17];
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
			Color32 realColor = Operation.GetColorOnScreen(790, 325);
			return Approximately(realColor, targetColor1) ||
					Approximately(realColor, targetColor2);
		}
	}

	public static FollowType GetFollowType() {
		Color32[,] colors = Operation.GetColorsOnScreen(988, 182, 213, 167);
		// 判断跟车界面图标是否已出现
		if (Approximately(colors[119, 97], new Color32(118, 132, 169, 255))) {
			return FollowType.NONE;
		}
		// 判断跟车界面图标是否是战锤
		else if (Approximately(colors[154, 128], new Color32(253, 109, 106, 255))) {
			return FollowType.WAR_HAMMER;
		}
		// 判断跟车界面图标是否是难民营
		else if (Approximately(colors[128, 91], new Color32(74, 133, 159, 255))) {
			return FollowType.REFUGEE_CAMP;
		}
		// 判断跟车界面图标是否是惧星
		else if (Approximately(colors[101, 31], new Color32(41, 131, 221, 255))) {
			return FollowType.FEAR_STAR;
		}
		// 判断跟车界面图标是否是黑暗军团据点
		else if (Approximately(colors[116, 54], new Color32(41, 39, 48, 255))) {
			return FollowType.STRONGHOLD;
		}
		// 判断跟车界面图标是否是精卫
		else if (Approximately(colors[125, 33], new Color32(158, 85, 92, 255))) {
			return FollowType.ELITE_GUARD;
		}
		// 判断跟车界面图标是否是爱心砰砰
		else if (Approximately(colors[123, 60], new Color32(225, 95, 97, 255))) {
			return FollowType.HEART_PANG;
		}
		return FollowType.UNKNOWN;
	}

	public static bool IsTooLateWindowExist {
		get {
			if (CurrentScene == Scene.FIGHTING) {
				// 判断出征界面是否弹出赶不上弹框
				// 暂时判断的是指定位置是否存在红色取消按钮
				Color32 targetColor = new Color32(211, 78, 56, 255);
				Color32 realColor = Operation.GetColorOnScreen(880, 700);
				return Approximately(realColor, targetColor);
			}
			return false;
		}
	}

	#region 废弃
	// private static readonly RectInt FOLLOW_OWNER_AVATAR_RECT = new RectInt(731, 190, 55, 54);	// 集结发起人头像范围
	// private const int FOLLOW_OWNER_AVATAR_FEATURE_XY_COUNT = 5; // 集结发起人头像特征获取的阵列边长
	// public static Color32[] GetFollowOwnerAvatar() {
	// 	int xStepLength = FOLLOW_OWNER_AVATAR_RECT.width / FOLLOW_OWNER_AVATAR_FEATURE_XY_COUNT;
	// 	int offsetX = (FOLLOW_OWNER_AVATAR_RECT.width - xStepLength * FOLLOW_OWNER_AVATAR_FEATURE_XY_COUNT) >> 1;
	// 	int yStepLength = FOLLOW_OWNER_AVATAR_RECT.height / FOLLOW_OWNER_AVATAR_FEATURE_XY_COUNT;
	// 	int offsetY = (FOLLOW_OWNER_AVATAR_RECT.height - yStepLength * FOLLOW_OWNER_AVATAR_FEATURE_XY_COUNT) >> 1;
	// 	int startX = FOLLOW_OWNER_AVATAR_RECT.x + offsetX;
	// 	int startY = FOLLOW_OWNER_AVATAR_RECT.y + offsetY;
	// 	Color32[] ownerAvatarFeature = new Color32[FOLLOW_OWNER_AVATAR_FEATURE_XY_COUNT * FOLLOW_OWNER_AVATAR_FEATURE_XY_COUNT];
	// 	for (int y = 0; y < FOLLOW_OWNER_AVATAR_FEATURE_XY_COUNT; ++y) {
	// 		for (int x = 0; x < FOLLOW_OWNER_AVATAR_FEATURE_XY_COUNT; ++x) {
	// 			ownerAvatarFeature[y * FOLLOW_OWNER_AVATAR_FEATURE_XY_COUNT + x] = ScreenshotUtils.GetColorOnScreen(startX + x, startY + y);
	// 		}
	// 	}
	// 	return ownerAvatarFeature;
	// }
	//
	// public static bool IsFollowIconExist {
	// 	get {
	// 		// 判断车界面图标是否已出现
	// 		Color32 targetColor = new Color32(118, 132, 169, 255);
	// 		Color32 realColor = ScreenshotUtils.GetColorOnScreen(1107, 279);
	// 		return !Approximately(realColor, targetColor);
	// 	}
	// }
	//
	// public static bool IsJDCanFollow {
	// 	get {
	// 		// 判断跟车界面图标是否是黑暗军团据点
	// 		Color32 targetColor = new Color32(41, 39, 48, 255);
	// 		Color32 realColor = ScreenshotUtils.GetColorOnScreen(1104, 236);
	// 		return Approximately(realColor, targetColor);
	// 	}
	// }
	//
	// public static bool IsZCCanFollow {
	// 	get {
	// 		// 判断跟车界面图标是否是战锤
	// 		Color32 targetColor = new Color32(253, 109, 106, 255);
	// 		Color32 realColor = ScreenshotUtils.GetColorOnScreen(1142, 310);
	// 		return Approximately(realColor, targetColor);
	// 	}
	// }
	//
	// public static bool IsNMYCanFollow {
	// 	get {
	// 		// 判断跟车界面图标是否是难民营
	// 		Color32 targetColor = new Color32(74, 133, 159, 255);
	// 		Color32 realColor = ScreenshotUtils.GetColorOnScreen(1116, 273);
	// 		return Approximately(realColor, targetColor);
	// 	}
	// }
	//
	// public static bool IsAXPPCanFollow {
	// 	get {
	// 		// 判断跟车界面图标是否是爱心砰砰
	// 		// Color32 targetColor = new Color32(182, 75, 97, 255);
	// 		// Color32 realColor = ScreenshotUtils.GetColorOnScreen(1049, 321);
	// 		Color32 targetColor = new Color32(225, 95, 97, 255);
	// 		Color32 realColor = ScreenshotUtils.GetColorOnScreen(1111, 242);
	// 		return Approximately(realColor, targetColor);
	// 	}
	// }
	//
	// public static bool IsJWCanFollow {
	// 	get {
	// 		// 判断跟车界面图标是否是精卫
	// 		Color32 targetColor = new Color32(158, 85, 92, 255);
	// 		Color32 realColor = ScreenshotUtils.GetColorOnScreen(1113, 215);
	// 		return Approximately(realColor, targetColor);
	// 	}
	// }
	//
	// public static bool IsJXCanFollow {
	// 	get {
	// 		// 判断跟车界面图标是否是惧星
	// 		Color32 targetColor = new Color32(41, 131, 221, 255);
	// 		Color32 realColor = ScreenshotUtils.GetColorOnScreen(1089, 213);
	// 		return Approximately(realColor, targetColor);
	// 	}
	// }
	#endregion
}
