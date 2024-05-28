/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using System.Collections.Generic;
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
	
	public enum AllianceMechaType {
		[InspectorName("未知")]
		UNKNOWN = 0,
		[InspectorName("阿尔法")]
		ALPHA = 1,
		[InspectorName("伽马")]
		GAMMA = 2,
		[InspectorName("德尔塔")]
		DELTA = 3,
		[InspectorName("伊普西龙")]
		EPSILON = 4,
	}
	
	public enum AllianceMechaState {
		[InspectorName("无")]
		NONE = 0,
		[InspectorName("可开启")]
		CAN_OPEN = 1,
		[InspectorName("可捐献")]
		CAN_DONATE = 2,
		[InspectorName("可召唤")]
		CAN_SUMMON = 3,
		[InspectorName("可挑战")]
		CAN_CHALLENGE = 4,
	}
	
	public enum RuinPropType {
		[InspectorName("其他")]
		OTHER = 0,
		[InspectorName("职业道具")]
		CLASS_PROP = 1,
		[InspectorName("大体力瓶")]
		BIG_ENERGY = 2,
		[InspectorName("绿色材料")]
		GREEN_MATERIAL = 3,
		[InspectorName("强化部件")]
		STRENGTHEN_PART = 4,
		[InspectorName("技能抽卡券")]
		SKILL_TICKET = 5,
		[InspectorName("紫色英雄碎片")]
		PURPLE_HERO_CHIP = 6,
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
	
	// 从0开始
	public static AllianceMechaType CurrentMechaType {
		get {
			Color32 targetColor = new Color32(255,255, 255, 255);
			for (AllianceMechaType type = AllianceMechaType.ALPHA; type <= AllianceMechaType.EPSILON; type++) {
				Color32 realColor = Operation.GetColorOnScreen(Mathf.RoundToInt(920.3F + 16.5F * (int) type), 413);
				if (Approximately(realColor, targetColor)) {
					return type;
				}
			}
			return AllianceMechaType.UNKNOWN;
		}
	}
	
	// 从1开始
	public static int CurrentMechaLevel {
		get {
			int level = 6;
			while (level >= 0) {
				Color32 realColor = Operation.GetColorOnScreen(698 + 63 * level, 471);
				float value = (float) (realColor.r + realColor.g) / realColor.b;
				if (value > 2.5F) {
					return level;
				}
				--level;
			}
			return -1;
		}
	}
	
	private static readonly Color32[,] ALLIANCE_MECHA_BTN_NONE = Operation.GetFromFile("PersistentData/Textures/AllianceMechaBtnNone.png");
	private static readonly Color32[,] ALLIANCE_MECHA_BTN_OPEN = Operation.GetFromFile("PersistentData/Textures/AllianceMechaBtnOpen.png");
	private static readonly Color32[,] ALLIANCE_MECHA_BTN_DONATE = Operation.GetFromFile("PersistentData/Textures/AllianceMechaBtnDonate.png");
	private static readonly Color32[,] ALLIANCE_MECHA_BTN_FREE_DONATE = Operation.GetFromFile("PersistentData/Textures/AllianceMechaBtnFreeDonate.png");
	private static readonly Color32[,] ALLIANCE_MECHA_BTN_SUMMON = Operation.GetFromFile("PersistentData/Textures/AllianceMechaBtnSummon.png");
	public static AllianceMechaState AllianceMechaStatus {
		get {
			return GetCachedValueOrNew(nameof(AllianceMechaStatus), () => {
				Color32[,] realColors = Operation.GetColorsOnScreen(907, 939, 106, 51);
				if (ApproximatelyRect(realColors, ALLIANCE_MECHA_BTN_NONE) > 0.9F) {
					return AllianceMechaState.NONE;
				} else if (ApproximatelyRect(realColors, ALLIANCE_MECHA_BTN_OPEN) > 0.9F) {
					return AllianceMechaState.CAN_OPEN;
				} else if (ApproximatelyRect(realColors, ALLIANCE_MECHA_BTN_DONATE) > 0.9F || ApproximatelyRect(realColors, ALLIANCE_MECHA_BTN_FREE_DONATE) > 0.9F) {
					return AllianceMechaState.CAN_DONATE;
				} else if (ApproximatelyRect(realColors, ALLIANCE_MECHA_BTN_SUMMON) > 0.9F) {
					return AllianceMechaState.CAN_SUMMON;
				} else {
					return AllianceMechaState.CAN_CHALLENGE;
				}
			});
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
				return ApproximatelyRect(realColors, ALLIANCE_HELP_AWARD_OUTER, 2F) > 0.7F;
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
	
	public static bool AllianceTerritoryIsNew {
		get {
			Color32 realColor = Operation.GetColorOnScreen(939, 497);
			return realColor.r > realColor.g + realColor.g + realColor.b + realColor.b;
		}
	}
	
	public static bool AllianceRuinIsNew {
		get {
			Color32 realColor = Operation.GetColorOnScreen(1197, 177);
			return realColor.r > realColor.g + realColor.g + realColor.b + realColor.b;
		}
	}
	
	public static bool AllianceRuinLv2IsNew {
		get {
			Color32 realColor = Operation.GetColorOnScreen(1035, 228);
			return realColor.r > realColor.g + realColor.g + realColor.b + realColor.b;
		}
	}
	
	private static readonly Color32[,] RUIN_PROP_CLASS_PROP = Operation.GetFromFile("PersistentData/Textures/RuinPropClassProp.png");
	private static readonly Color32[,] RUIN_PROP_BIG_ENERGY = Operation.GetFromFile("PersistentData/Textures/RuinPropBigEnergy.png");
	private static readonly Color32[,] RUIN_PROP_GREEN_MATERIAL = Operation.GetFromFile("PersistentData/Textures/RuinPropGreenMaterial.png");
	private static readonly Color32[,] RUIN_PROP_STRENGTHEN_PART = Operation.GetFromFile("PersistentData/Textures/RuinPropStrengthenPart.png");
	private static readonly Color32[,] RUIN_PROP_SKILL_TICKET = Operation.GetFromFile("PersistentData/Textures/RuinPropSkillTicket.png");
	private static readonly Color32[,] RUIN_PROP_PURPLE_HERO_CHIP = Operation.GetFromFile("PersistentData/Textures/RuinPropPurpleHeroChip.png");
	public static RuinPropType GetRuinPropType(int x, int y, int width, int height) {
		Color32[,] realColors = Operation.GetColorsOnScreen(x, y, width, height);
		if (ApproximatelyRect(realColors, RUIN_PROP_CLASS_PROP) > 0.5F) {
			return RuinPropType.CLASS_PROP;
		} else if (ApproximatelyRect(realColors, RUIN_PROP_BIG_ENERGY) > 0.5F) {
			return RuinPropType.BIG_ENERGY;
		} else if (ApproximatelyRect(realColors, RUIN_PROP_GREEN_MATERIAL) > 0.5F) {
			return RuinPropType.GREEN_MATERIAL;
		} else if (ApproximatelyRect(realColors, RUIN_PROP_STRENGTHEN_PART) > 0.5F) {
			return RuinPropType.STRENGTHEN_PART;
		} else if (ApproximatelyRect(realColors, RUIN_PROP_SKILL_TICKET) > 0.5F) {
			return RuinPropType.SKILL_TICKET;
		} else if (ApproximatelyRect(realColors, RUIN_PROP_PURPLE_HERO_CHIP) > 0.5F) {
			return RuinPropType.PURPLE_HERO_CHIP;
		} else {
			return RuinPropType.OTHER;
		}
	}
}
