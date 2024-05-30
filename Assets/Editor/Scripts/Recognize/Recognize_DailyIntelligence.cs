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
	
	private static readonly Color32[,] DAILY_INTELLIGENCE_GET_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceGetBtn.png");
	private static readonly Color32[,] DAILY_INTELLIGENCE_BACK_BTN = Operation.GetFromFile("PersistentData/Textures/DailyIntelligenceBackBtn.png");
	public static bool IsWildGetBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(934, 823, 52, 27);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_GET_BTN) > 0.7F;
		}
	}
	public static bool IsWildBackBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(934, 823, 52, 27);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_BACK_BTN) > 0.7F;
		}
	}
	public static bool IsExpeditionGetBtn {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(934, 896, 52, 27);
			return ApproximatelyRect(realColors, DAILY_INTELLIGENCE_GET_BTN) > 0.7F;
		}
	}
}
