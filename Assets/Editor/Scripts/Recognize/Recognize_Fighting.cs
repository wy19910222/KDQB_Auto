﻿/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using UnityEngine;

public static partial class Recognize {
	public static bool IsFightingPlayback => GetCachedValueOrNew(nameof(IsFightingPlayback), () => 
			Approximately(Operation.GetColorOnScreen(30, 185), new Color32(94, 126, 202, 255)));
	
	private const int SOLDIER_EMPTY_X = 48;
	private const int SOLDIER_FULL_X = 156;
	private const int SOLDIER_Y = 468;
	public static float FightingSoldierCountPercent {
		get {
			const int width = SOLDIER_FULL_X - SOLDIER_EMPTY_X;
			Color32[,] colors = Operation.GetColorsOnScreen(SOLDIER_EMPTY_X, SOLDIER_Y, width + 1, 1);
			for (int x = colors.GetLength(0) - 1; x >= 0; --x) {
				Color32 color = colors[x, 0];
				if (color.g > color.r && color.g > color.b) {
					return (float) x / width;
				}
			}
			return 0;
		}
	}
	
	private static readonly Color32 HERO_EMPTY_COLOR1 = new Color32(97, 166, 248, 255);
	private static readonly Color32 HERO_EMPTY_COLOR2 = new Color32(85, 157, 242, 255);
	public static int FightingHeroEmptyCount {
		get {
			int count = 0;
			Color32[,] realColors = Operation.GetColorsOnScreen(20, 391, 160, 27);
			if (IsHeroEmpty(realColors, 132, 0)) {
				++count;
			}
			if (IsHeroEmpty(realColors, 70, 0) || IsHeroEmpty(realColors, 104, 0)) {
				++count;
			}
			if (IsHeroEmpty(realColors, 7, 0) || IsHeroEmpty(realColors, 35, 0)) {
				++count;
			}
			return count;
		}
	}
	private static bool IsHeroEmpty(Color32[,] realColors, int x, int y) {
		Color32 realColor1 = realColors[x + 5, y + 5];
		Color32 realColor2 = realColors[x + 21, y + 5];
		Color32 realColor3 = realColors[x + 5, y + 21];
		Color32 realColor4 = realColors[x + 21, y + 21];
		return Approximately(realColor1, HERO_EMPTY_COLOR1)
				&& Approximately(realColor2, HERO_EMPTY_COLOR1)
				&& Approximately(realColor3, HERO_EMPTY_COLOR2)
				&& Approximately(realColor4, HERO_EMPTY_COLOR2);
	}
}
