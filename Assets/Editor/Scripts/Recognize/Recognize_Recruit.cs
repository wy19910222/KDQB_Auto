/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using UnityEngine;

public static partial class Recognize {
	private static readonly Color32[,] MAIN_BTN_RED_POINT = Operation.GetFromFile("PersistentData/Textures/MainBtnRedPoint.png");
	public static bool CanRecruitOuter {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(1890, 609, 9, 9);
			return ApproximatelyRect(realColors, MAIN_BTN_RED_POINT, 2F) > 0.8F;
		}
	}

	public static bool CanRecruitMiddle {
		get {
			Color32 realColor1 = Operation.GetColorOnScreen(1204, 933);
			Color32 realColor2 = Operation.GetColorOnScreen(1119, 933);
			Color32 targetColor = new Color32(222, 57, 0, 255);
			return Approximately(realColor1, targetColor) || Approximately(realColor2, targetColor);
		}
	}

	public static bool CanGeneralRecruit {
		get {
			Color32 realColor = Operation.GetColorOnScreen(765, 926);
			Color32 targetColor = new Color32(222, 57, 0, 255);
			return Approximately(realColor, targetColor);
		}
	}

	public static bool CanSkillRecruit {
		get {
			Color32 realColor = Operation.GetColorOnScreen(1193, 926);
			Color32 targetColor = new Color32(222, 57, 0, 255);
			return Approximately(realColor, targetColor);
		}
	}

	public static bool CanRecruitInner {
		get {
			Color32 realColor = Operation.GetColorOnScreen(938, 780);
			Color32 targetColor = new Color32(222, 57, 0, 255);
			return Approximately(realColor, targetColor);
		}
	}
}
