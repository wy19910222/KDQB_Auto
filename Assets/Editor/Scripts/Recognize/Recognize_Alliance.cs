﻿/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using UnityEngine;

public static partial class Recognize {
	public enum AllianceActivityType {
		[InspectorName("无")]
		NONE,
		[InspectorName("联盟机甲")]
		MECHA,
		[InspectorName("联盟保卫战")]
		DEFENCE,
	}
	
	private static readonly Color32[,] ALLIANCE_ACTIVITY_MECHA = Operation.GetFromFile("PersistentData/Textures/AllianceActivityMecha.png");
	private static readonly Color32[,] ALLIANCE_ACTIVITY_DEFENCE = Operation.GetFromFile("PersistentData/Textures/AllianceActivityDefence.png");
	public static AllianceActivityType[] AllianceActivityTypes {
		get {
			const int ACTIVITY_COUNT_MAX = 3;
			Color32[][,] realColorsArray = new Color32[3][,];
			for (int i = 0; i < ACTIVITY_COUNT_MAX; ++i) {
				realColorsArray[i] = Operation.GetColorsOnScreen(724, 188 + 269 * i, 290, 34);
			}
			AllianceActivityType[] types = new AllianceActivityType[ACTIVITY_COUNT_MAX];
			for (int i = 0; i < ACTIVITY_COUNT_MAX; ++i) {
				if (ApproximatelyRect(realColorsArray[i], ALLIANCE_ACTIVITY_MECHA) > 0.8F) {
					types[i] = AllianceActivityType.MECHA;
				} else if (ApproximatelyRect(realColorsArray[i], ALLIANCE_ACTIVITY_DEFENCE) > 0.8F) {
					types[i] = AllianceActivityType.DEFENCE;
				}
			}
			return types;
		}
	}
	
	private static readonly Color32[,] ALLIANCE_MECHA_DONATE_ENABLED = Operation.GetFromFile("PersistentData/Textures/AllianceMechaDonateEnabled.png");
	public static bool IsAllianceMechaDonateEnabled {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(1067, 929, 16, 16);
			return ApproximatelyRect(realColors, ALLIANCE_MECHA_DONATE_ENABLED) > 0.8F;
		}
	}

	private static readonly Color32[,] ALLIANCE_MECHA_DONATE_RANK_LIST = Operation.GetFromFile("PersistentData/Textures/AllianceMechaDonateRankList.png");
	public static bool IsAllianceMechaDonateRankShowing {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(915, 185, 90, 24);
			return ApproximatelyRect(realColors, ALLIANCE_MECHA_DONATE_RANK_LIST) > 0.8F;
		}
	}

	private static readonly Color32[,] ALLIANCE_MECHA_DONATE_IN_RANK = Operation.GetFromFile("PersistentData/Textures/AllianceMechaDonateInRank.png");
	public static bool IsAllianceMechaDonateInRank {
		get {
			// 通过判断排行列表中最后一行的底色确定自己是否在排行中
			Color32[,] realColors = Operation.GetColorsOnScreen(780, 850, 10, 10);
			return ApproximatelyRect(realColors, ALLIANCE_MECHA_DONATE_IN_RANK) > 0.8F;
		}
	}

	// private static readonly Color32[,] ALLIANCE_MECHA_DONATE_CONFIRM = Operation.GetFromFile("PersistentData/Textures/AllianceMechaDonateConfirm.png");
	private static readonly Color32[,] ALLIANCE_MECHA_DONATE_CONFIRM_BTN = Operation.GetFromFile("PersistentData/Textures/AllianceMechaDonateConfirmBtn.png");
	public static bool IsAllianceMechaDonateConfirming {
		get {
			// Color32[,] realColors = Operation.GetColorsOnScreen(860, 340, 200, 30);
			// return ApproximatelyRect(realColors, ALLIANCE_MECHA_DONATE_CONFIRM) > 0.9F;
			Color32[,] realColors = Operation.GetColorsOnScreen(930, 672, 60, 30);
			return ApproximatelyRect(realColors, ALLIANCE_MECHA_DONATE_CONFIRM_BTN) > 0.8F;
		}
	}

	private static readonly Color32[,] ALLIANCE_HELP_OUTER = Operation.GetFromFile("PersistentData/Textures/AllianceHelpOuter.png");
	public static bool CanAllianceHelpOuter  {
		get {
			return GetCachedValueOrNew(nameof(CanAllianceHelpOuter), () => {
				Color32[,] realColors = Operation.GetColorsOnScreen(1777, 698, 36, 36);
				return ApproximatelyRect(realColors, ALLIANCE_HELP_OUTER, 1.6F) > 0.8F;
			});
		}
	}

	private static readonly Color32[,] ALLIANCE_HELP_AWARD_OUTER = Operation.GetFromFile("PersistentData/Textures/AllianceHelpAwardOuter.png");
	public static bool IsAllianceHelpAwardOuter {
		get {
			return GetCachedValueOrNew(nameof(IsAllianceHelpAwardOuter), () => {
				Color32[,] realColors = Operation.GetColorsOnScreen(1888, 680, 19, 16);
				return ApproximatelyRect(realColors, ALLIANCE_HELP_AWARD_OUTER) > 0.8F;
			});
		}
	}

	private static readonly Color32[,] ALLIANCE_HELP_AWARD_INNER = Operation.GetFromFile("PersistentData/Textures/AllianceHelpAwardInner.png");
	public static bool IsAllianceHelpAwardInner {
		get {
			return GetCachedValueOrNew(nameof(IsAllianceHelpAwardInner), () => {
				Color32[,] realColors = Operation.GetColorsOnScreen(1130, 590, 60, 60);
				return ApproximatelyRect(realColors, ALLIANCE_HELP_AWARD_INNER) > 0.8F;
			});
		}
	}

	private static readonly Color32[,] ALLIANCE_HELP_AWARD_INTUITIVE = Operation.GetFromFile("PersistentData/Textures/AllianceHelpAwardIntuitive.png");
	public static bool IsAllianceHelpAwardIntuitive {
		get {
			return GetCachedValueOrNew(nameof(IsAllianceHelpAwardIntuitive), () => {
				Color32[,] realColors = Operation.GetColorsOnScreen(1073, 885, 82, 23);
				return ApproximatelyRect(realColors, ALLIANCE_HELP_AWARD_INTUITIVE) > 0.8F;
			});
		}
	}

	private static readonly Color32[,] ALLIANCE_HELP_REQUEST = Operation.GetFromFile("PersistentData/Textures/AllianceHelpRequest.png");
	public static bool CanAllianceHelpRequest {
		get {
			return GetCachedValueOrNew(nameof(CanAllianceHelpRequest), () => {
				Color32[,] realColors = Operation.GetColorsOnScreen(1073, 885, 82, 23);
				return ApproximatelyRect(realColors, ALLIANCE_HELP_REQUEST) > 0.8F;
			});
		}
	}
	
	private static readonly Color32[,] ALLIANCE_HELP_CANCEL = Operation.GetFromFile("PersistentData/Textures/AllianceHelpCancel.png");
	public static bool CanAllianceHelpCancel {
		get {
			return GetCachedValueOrNew(nameof(CanAllianceHelpCancel), () => {
				Color32[,] realColors = Operation.GetColorsOnScreen(1073, 885, 82, 23);
				return ApproximatelyRect(realColors, ALLIANCE_HELP_CANCEL) > 0.8F;
			});
		}
	}
	
	private static readonly Color32[,] ALLIANCE_HELP_OTHERS = Operation.GetFromFile("PersistentData/Textures/AllianceHelpOthers.png");
	public static bool CanAllianceHelpOthers {
		get {
			return GetCachedValueOrNew(nameof(CanAllianceHelpOthers), () => {
				Color32[,] realColors = Operation.GetColorsOnScreen(926, 768, 69, 19);
				return ApproximatelyRect(realColors, ALLIANCE_HELP_OTHERS) > 0.8F;
			});
		}
	}
	
	private static readonly Color32[,] ALLIANCE_HELP_COMPLETE = Operation.GetFromFile("PersistentData/Textures/AllianceHelpComplete.png");
	public static bool IsAllianceHelpComplete {
		get {
			return GetCachedValueOrNew(nameof(IsAllianceHelpComplete), () => {
				Color32[,] realColors = Operation.GetColorsOnScreen(1066, 883, 22, 30);
				return ApproximatelyRect(realColors, ALLIANCE_HELP_COMPLETE) > 0.8F;
			});
		}
	}
}
