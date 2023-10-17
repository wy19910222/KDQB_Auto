/*
 * @Author: wangyun
 * @CreateTime: 2023-10-18 01:59:11 518
 * @LastEditor: wangyun
 * @EditTime: 2023-10-18 01:59:11 522
 */

using System.Collections.Generic;
using UnityEngine;

public static partial class Recognize {
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
}
