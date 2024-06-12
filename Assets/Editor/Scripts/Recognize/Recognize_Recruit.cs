/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using UnityEngine;

public static partial class Recognize {
	public static bool CanRecruitOuter {
		get {
			Color32 realColor1 = Operation.GetColorOnScreen(1896, 611);	// 红点红色
			Color32 realColor2 = Operation.GetColorOnScreen(1891, 616);	// 按钮蓝色
			return realColor1.r > realColor1.g + realColor1.b && realColor2.b > realColor2.r + realColor2.g;
		}
	}

	// 中层界面的英雄招募按钮红点
	public static bool CanRecruitMiddle {
		get {
			Color32 realColor1 = Operation.GetColorOnScreen(1204, 933);
			Color32 realColor2 = Operation.GetColorOnScreen(1119, 933);
			Color32 targetColor = new Color32(222, 57, 0, 255);
			return Approximately(realColor1, targetColor) || Approximately(realColor2, targetColor);
		}
	}

	// 内层界面的普通招募红点
	public static bool CanGeneralRecruit {
		get {
			Color32 realColor = Operation.GetColorOnScreen(765, 926);
			Color32 targetColor = new Color32(222, 57, 0, 255);
			return Approximately(realColor, targetColor);
		}
	}

	// 内层界面的技能招募红点
	public static bool CanSkillRecruit {
		get {
			Color32 realColor = Operation.GetColorOnScreen(1193, 926);
			Color32 targetColor = new Color32(222, 57, 0, 255);
			return Approximately(realColor, targetColor);
		}
	}

	// 内层界面的招募按钮红点
	public static bool CanRecruitInner {
		get {
			Color32 realColor = Operation.GetColorOnScreen(938, 780);
			Color32 targetColor = new Color32(222, 57, 0, 255);
			return Approximately(realColor, targetColor);
		}
	}

	// 内层界面的额外招募红点
	public static bool CanRecruitExtra {
		get {
			Color32 realColor = Operation.GetColorOnScreen(787, 711);
			Color32 targetColor = new Color32(222, 57, 0, 255);
			return Approximately(realColor, targetColor);
		}
	}
}
