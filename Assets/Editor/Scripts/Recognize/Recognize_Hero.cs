/*
 * @Author: wangyun
 * @CreateTime: 2023-10-18 01:59:11 518
 * @LastEditor: wangyun
 * @EditTime: 2023-10-18 01:59:11 522
 */

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

public static partial class Recognize {
	public enum HeroType {
		[InspectorName("戴安娜")]
		DAN,
		[InspectorName("尤里卡")]
		YLK,
		[InspectorName("明日香")]
		MRX,
		[InspectorName("阿宝")]
		AB,
		[InspectorName("普蕾")]
		PL,
		[InspectorName("公爵")]
		GJ,
	}

	public static int GetHeroGroupNumber(HeroType type) {
		int ret = type switch {
			HeroType.DAN => GetHeroGroupNumber(AVATAR_DAN_FARAWAY, AVATAR_DAN_NEARBY),
			HeroType.YLK => GetHeroGroupNumber(AVATAR_YLK_FARAWAY, AVATAR_YLK_NEARBY),
			HeroType.MRX => GetHeroGroupNumber(AVATAR_MRX_FARAWAY, AVATAR_MRX_NEARBY),
			HeroType.AB => GetHeroGroupNumber(AVATAR_AB_FARAWAY, AVATAR_AB_NEARBY),
			HeroType.PL => GetHeroGroupNumber(AVATAR_PL_FARAWAY, AVATAR_PL_NEARBY),
			HeroType.GJ => GetHeroGroupNumber(AVATAR_GJ_FARAWAY, AVATAR_GJ_NEARBY),
			_ => -1
		};
		if (ret == -1) {
			// 如果结果是-1，也可能是UI正在变动，重新判断一次
			ret = type switch {
				HeroType.DAN => GetHeroGroupNumber(AVATAR_DAN_FARAWAY, AVATAR_DAN_NEARBY),
				HeroType.YLK => GetHeroGroupNumber(AVATAR_YLK_FARAWAY, AVATAR_YLK_NEARBY),
				HeroType.MRX => GetHeroGroupNumber(AVATAR_MRX_FARAWAY, AVATAR_MRX_NEARBY),
				HeroType.AB => GetHeroGroupNumber(AVATAR_AB_FARAWAY, AVATAR_AB_NEARBY),
				HeroType.PL => GetHeroGroupNumber(AVATAR_PL_FARAWAY, AVATAR_PL_NEARBY),
				HeroType.GJ => GetHeroGroupNumber(AVATAR_GJ_FARAWAY, AVATAR_GJ_NEARBY),
				_ => -1
			};
		}
		return ret;
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
	public static readonly Color32[] AVATAR_DAN_FARAWAY = {
		new Color32(240, 190, 95, 255), new Color32(73, 74, 124, 255), new Color32(57, 65, 107, 255),
		new Color32(182, 127, 74, 255), new Color32(233, 168, 148, 255), new Color32(228, 160, 145, 255),
												new Color32(232, 164, 133, 255),
	};
	public static readonly Color32[] AVATAR_DAN_NEARBY = {
		new Color32(251, 193, 92, 255), new Color32(86, 80, 134, 255), new Color32(65, 57, 109, 255),
		new Color32(172, 119, 76, 255), new Color32(231, 177, 156, 255), new Color32(222, 152, 134, 255),
												new Color32(231, 134, 122, 255),
	};
	public static readonly Color32[] AVATAR_MRX_FARAWAY = {
		new Color32(211, 132, 72, 255), new Color32(221, 141, 64, 255), new Color32(198, 116, 60, 255),
		new Color32(147, 66, 70, 255), new Color32(186, 127, 90, 255), new Color32(231, 204, 203, 255),
												new Color32(158, 105, 93, 255),
	};
	public static readonly Color32[] AVATAR_MRX_NEARBY = {
		new Color32(211, 132, 72, 255), new Color32(221, 141, 64, 255), new Color32(198, 116, 60, 255),
		new Color32(147, 66, 70, 255), new Color32(186, 127, 90, 255), new Color32(231, 204, 203, 255),
												new Color32(158, 105, 93, 255),
	};
	public static readonly Color32[] AVATAR_AB_FARAWAY = {
		new Color32(250, 192, 93, 255), new Color32(227, 226, 219, 255), new Color32(215, 210, 206, 255),
		new Color32(223, 222, 215, 255), new Color32(229, 225, 217, 255), new Color32(146, 134, 117, 255),
												new Color32(187, 173, 163, 255),
	};
	public static readonly Color32[] AVATAR_AB_NEARBY = {
		new Color32(251, 196, 93, 255), new Color32(223, 219, 213, 255), new Color32(212, 208, 205, 255),
		new Color32(225, 222, 215, 255), new Color32(229, 226, 219, 255), new Color32(128, 116, 102, 255),
												new Color32(194, 185, 175, 255),
	};
	public static readonly Color32[] AVATAR_PL_FARAWAY = {
		new Color32(250, 192, 93, 255), new Color32(236, 176, 142, 255), new Color32(170, 126, 107, 255),
		new Color32(230, 184, 126, 255), new Color32(90, 101, 49, 255), new Color32(159, 107, 87, 255),
												new Color32(35, 31, 31, 255),
	};
	public static readonly Color32[] AVATAR_PL_NEARBY = {
		new Color32(251, 196, 93, 255), new Color32(232, 169, 135, 255), new Color32(159, 113, 95, 255),
		new Color32(226, 190, 150, 255), new Color32(152, 158, 67, 255), new Color32(170, 120, 103, 255),
												new Color32(35, 30, 29, 255),
	};
	public static readonly Color32[] AVATAR_GJ_FARAWAY = {
		new Color32(250, 192, 93, 255), new Color32(208, 163, 104, 255), new Color32(196, 165, 100, 255),
		new Color32(241, 170, 74, 255), new Color32(190, 131, 109, 255), new Color32(168, 125, 137, 255),
												new Color32(104, 78, 85, 255),
	};
	public static readonly Color32[] AVATAR_GJ_NEARBY = {
		new Color32(251, 196, 93, 255), new Color32(196, 158, 87, 255), new Color32(180, 153, 97, 255),
		new Color32(241, 171, 80, 255), new Color32(160, 114, 94, 255), new Color32(170, 133, 150, 255),
												new Color32(123, 95, 114, 255),
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
		deltaY = IsMiniMapShowing switch {
			true => deltaY + 155,
			false => deltaY,
			_ => -1
		};
		if (deltaY >= 0 && heroAvatar != null) {
			int groupCount = 0;
			// 返回加速等蓝色按钮中间的白色
			Color32[,] realColors = Operation.GetColorsOnScreen(0, 245 + deltaY + 50, 160, 500);
			while (groupCount < BusyGroupCount) {
				int approximatelyCount = 0;
				int pointCount = AVATAR_SAMPLE_POINTS.Length;
				for (int i = 0; i < pointCount; ++i) {
					Vector2Int point = AVATAR_SAMPLE_POINTS[i];
					Vector2Int finalPoint = new Vector2Int(22 + point.x, 18 + groupCount * 50 + point.y);
					int r = 0, g = 0, b = 0;
					for (int y = -1; y < 2; ++y) {
						Color32 c = realColors[finalPoint.x, finalPoint.y + y];
						r += c.r;
						g += c.g;
						b += c.b;
					}
					Color32 color = new Color32((byte) (r / 3), (byte) (g / 3), (byte) (b / 3), 255);
					// Color32 color = realColors[finalPoint.x, finalPoint.y];
					if (groupCount == 4 && deltaY == 76 + 155) {
						Color32 coverColor = new Color32(129, 169, 203, 255);
						float[] weight = new[] {0.1F, 0.1F, 0.1F, 0.15F, 0.15F, 0.15F, 0.2F};
						color.r = (byte) ((color.r - coverColor.r * weight[i]) / (1 - weight[i]));
						color.g = (byte) ((color.g - coverColor.g * weight[i]) / (1 - weight[i]));
						color.b = (byte) ((color.b - coverColor.b * weight[i]) / (1 - weight[i]));
					}
					if (ApproximatelyCoveredCount(color, heroAvatar[i], 1.5F) >= 0) {
						++approximatelyCount;
					}
				}
				// Debug.LogError($"------------{groupCount}: {approximatelyCount}--------------");
				if (approximatelyCount > pointCount * 0.4F) {
					return groupCount;
				}
				groupCount++;
			}
			return -1;
		}
		return int.MaxValue;
	}
	
	[MenuItem("Assets/LogGroupHeroAvatar", priority = -1)]
	public static void LogGroupHeroAvatar() {
		int deltaY = IsOutsideNearby ? 76 : IsOutsideFaraway ? 0 : -1;
		deltaY = IsMiniMapShowing switch {
			true => deltaY + 155,
			false => deltaY,
			_ => -1
		};
		if (deltaY >= 0) {
			int groupCount = 0;
			Color32[,] realColors = Operation.GetColorsOnScreen(0, 245 + deltaY + 50, 160, 500);
			while (groupCount < BusyGroupCount) {
				Debug.LogError($"----------------------{groupCount}----------------------");
				List<Color32> list = new List<Color32>();
				for (int i = 0, length = AVATAR_SAMPLE_POINTS.Length; i < length; ++i) {
					Vector2Int point = AVATAR_SAMPLE_POINTS[i];
					Vector2Int finalPoint = new Vector2Int(22 + point.x, 18 + groupCount * 50 + point.y);
					int r = 0, g = 0, b = 0;
					for (int y = -1; y < 2; ++y) {
						Color32 c = realColors[finalPoint.x, finalPoint.y + y];
						r += c.r;
						g += c.g;
						b += c.b;
					}
					Color32 color = new Color32((byte) (r / 3), (byte) (g / 3), (byte) (b / 3), 255);
					list.Add(color);
					// Debug.Log($"{finalPoint}: {color}");
				}
				StringBuilder sb = new StringBuilder();
				foreach (var c in list) {
					sb.Append("new Color32(");
					sb.Append(c.r);
					sb.Append(", ");
					sb.Append(c.g);
					sb.Append(", ");
					sb.Append(c.b);
					sb.Append(", 255), ");
				}
				Debug.LogError(sb);
				groupCount++;
			}
		}
	}
}
