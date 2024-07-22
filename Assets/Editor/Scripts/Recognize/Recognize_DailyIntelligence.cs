/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using UnityEngine;

public static partial class Recognize {
	public enum DailyIntelligenceType {
		[InspectorName("未知")]
		UNKNOWN,
		[InspectorName("荒野行动")]
		WILD,
		[InspectorName("远征行动")]
		EXPEDITION,
		[InspectorName("沙盘演习")]
		SAND_TABLE,
		[InspectorName("跨战区演习")]
		TRANSNATIONAL,
		[InspectorName("岛屿作战")]
		ISLAND,
	}
	
	private static readonly Color32[,] DAILY_INTELLIGENCE_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceBtn.png");
	public static bool DailyIntelligenceBtnVisible {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(718, 852, 31, 31);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_BTN) > 0.7F;
		}
	}

	private static readonly Color32[,] DAILY_INTELLIGENCE_TITLE_WILD = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceTitleWild.png");
	private static readonly Color32[,] DAILY_INTELLIGENCE_TITLE_EXPEDITION = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceTitleExpedition.png");
	private static readonly Color32[,] DAILY_INTELLIGENCE_TITLE_SAND_TABLE = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceTitleSandTable.png");
	private static readonly Color32[,] DAILY_INTELLIGENCE_TITLE_TRANSNATIONAL = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceTitleTransnational.png");
	public static DailyIntelligenceType DailyIntelligenceCurrentType {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(896, 112, 131, 30);
			if (ApproximatelyRectIgnoreCovered(realColors, DAILY_INTELLIGENCE_TITLE_WILD) > 0.9F) {
				return DailyIntelligenceType.WILD;
			}
			else if (ApproximatelyRectIgnoreCovered(realColors, DAILY_INTELLIGENCE_TITLE_EXPEDITION) > 0.9F) {
				return DailyIntelligenceType.EXPEDITION;
			}
			else if (ApproximatelyRectIgnoreCovered(realColors, DAILY_INTELLIGENCE_TITLE_SAND_TABLE) > 0.9F) {
				return DailyIntelligenceType.SAND_TABLE;
			}
			else if (ApproximatelyRectIgnoreCovered(realColors, DAILY_INTELLIGENCE_TITLE_TRANSNATIONAL) > 0.9F) {
				return DailyIntelligenceType.TRANSNATIONAL;
			}
			return IsIsland ? DailyIntelligenceType.ISLAND : DailyIntelligenceType.UNKNOWN;
		}
	}
	
	private static readonly Color32[,] DAILY_INTELLIGENCE_ISLAND_RESET_TIMES_LABEL = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceIslandResetTimesLabel.png");
	private static bool IsIsland {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(714, 983, 80, 20);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_ISLAND_RESET_TIMES_LABEL) > 0.7F;
		}
	}
	
	private static readonly Color32[,] DAILY_INTELLIGENCE_WINDOW_GET_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceWindowGetBtn.png");
	private static readonly Color32[,] DAILY_INTELLIGENCE_WINDOW_BACK_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceWindowBackBtn.png");
	public static bool IsWildGetBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(934, 823, 52, 27);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_WINDOW_GET_BTN) > 0.7F;
		}
	}
	public static bool IsWildBackBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(934, 823, 52, 27);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_WINDOW_BACK_BTN) > 0.7F;
		}
	}
	public static bool IsExpeditionGetBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(934, 896, 52, 27);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_WINDOW_GET_BTN) > 0.7F;
		}
	}
	
	private static readonly Color32[,] DAILY_INTELLIGENCE_ISLAND_EXIT_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceIslandExitBtn.png");
	public static bool IsIslandExitBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(930, 616, 60, 30);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_ISLAND_EXIT_BTN) > 0.7F;
		}
	}
	private static readonly Color32[,] DAILY_INTELLIGENCE_ISLAND_RESET_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceIslandResetBtn.png");
	public static bool IsIslandResetBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(1043, 616, 60, 30);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_ISLAND_RESET_BTN) > 0.7F;
		}
	}
	private static readonly Color32[,] DAILY_INTELLIGENCE_ISLAND_MOP_UP_ICON_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceIslandMopUpIconBtn.png");
	public static bool IsIslandMopUpIconBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(714, 916, 56, 60);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_ISLAND_MOP_UP_ICON_BTN) > 0.7F;
		}
	}
	private static readonly Color32[,] DAILY_INTELLIGENCE_ISLAND_MOP_UP_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceIslandMopUpBtn.png");
	public static bool IsIslandMopUpBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(930, 834, 60, 30);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_ISLAND_MOP_UP_BTN) > 0.7F;
		}
	}
	private static readonly Color32[,] DAILY_INTELLIGENCE_ISLAND_CONFIRM_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceIslandConfirmBtn.png");
	public static bool IsIslandConfirmBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(930, 852, 60, 30);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_ISLAND_CONFIRM_BTN) > 0.7F;
		}
	}
	
	public static bool IsSandTableCanChallenge {
		get {
			Color32 realColor = Operation.GetColorOnScreen(920, 940);
			return Approximately(realColor, new Color32(104, 146, 248, 255));
		}
	}
	// public static bool IsSandTableUsingPhalanx {
	// 	get {
	// 		Color32 realColor = Operation.GetColorOnScreen(52, 502);
	// 		return Approximately(realColor, new Color32(105, 232, 254, 255));
	// 	}
	// }
	
	private static readonly Color32[,] DAILY_INTELLIGENCE_EXPEDITION_QUICK_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceExpeditionQuickBtn.png");
	public static bool IsExpeditionQuickBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(1105, 923, 60, 60);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_EXPEDITION_QUICK_BTN) > 0.7F;
		}
	}
	private static readonly Color32[,] DAILY_INTELLIGENCE_EXPEDITION_QUICK_FREE_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceExpeditionQuickFreeBtn.png");
	public static bool IsExpeditionQuickFreeBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(895, 714, 130, 50);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_EXPEDITION_QUICK_FREE_BTN) > 0.95F;
		}
	}
	private static readonly Color32[,] DAILY_INTELLIGENCE_EXPEDITION_QUICK_BY_50_DIAMOND_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceExpeditionQuickBy50DiamondBtn.png");
	public static bool IsExpeditionQuickBy50DiamondBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(895, 714, 130, 50);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_EXPEDITION_QUICK_BY_50_DIAMOND_BTN) > 0.95F;
		}
	}
	private static readonly Color32[,] DAILY_INTELLIGENCE_EXPEDITION_QUICK_CONFIRM_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceExpeditionQuickConfirmBtn.png");
	public static bool IsExpeditionQuickConfirmBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(938, 687, 44, 22);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_EXPEDITION_QUICK_CONFIRM_BTN) > 0.95F;
		}
	}
	
	private static readonly Color32[,] DAILY_INTELLIGENCE_TRANSNATIONAL_TARGET_LIST = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceTransnationalTargetList.png");
	public static bool IsTransnationalTargetList {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(910, 230, 100, 30);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_TRANSNATIONAL_TARGET_LIST) > 0.95F;
		}
	}
	private static readonly Color32[,] DAILY_INTELLIGENCE_TRANSNATIONAL_TIMES_EMPTY = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceTransnationalTimesEmpty.png");
	public static bool IsTransnationalTimesEmpty {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(1020, 297, 22, 16);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_TRANSNATIONAL_TIMES_EMPTY) > 0.95F;
		}
	}
}
