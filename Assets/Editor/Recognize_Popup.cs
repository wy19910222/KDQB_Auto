/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using UnityEngine;

public static partial class Recognize {
	public static bool IsWindowCovered {
		get {
			return GetCachedValueOrNew(nameof(IsWindowCovered), () => {
				switch (CurrentScene) {
					case Scene.FIGHTING:
						// 左上角返回按钮颜色很暗
						return !Approximately(Operation.GetColorOnScreen(50, 130), new Color32(94, 126, 202, 255));
					case Scene.INSIDE:
					case Scene.OUTSIDE:
						// 右下角一排按钮颜色很暗
						return !Approximately(Operation.GetColorOnScreen(1850, 620), new Color32(69, 146, 221, 255));
				}
				return false;
			});
		}
	}

	public static float WindowCoveredCount {
		get {
			return GetCachedValueOrNew(nameof(WindowCoveredCount), () => {
				switch (CurrentScene) {
					case Scene.FIGHTING:
						// 左上角返回按钮颜色很暗
						return ApproximatelyCoveredCount(Operation.GetColorOnScreen(50, 130), new Color32(94, 126, 202, 255));
					case Scene.INSIDE:
					case Scene.OUTSIDE:
						// 右下角一排按钮颜色很暗
						return ApproximatelyCoveredCount(Operation.GetColorOnScreen(1850, 620), new Color32(69, 146, 221, 255));
				}
				return -1;
			});
		}
	}
	
	private static readonly Color32[,] NETWORK_DISCONNECTED = Operation.GetFromFile("PersistentData/Textures/NetworkDisconnected.png");
	public static bool IsNetworkDisconnected {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(910, 440, 100, 26);
			return ApproximatelyRect(realColors, NETWORK_DISCONNECTED) > 0.99F;
		}
	}

	private static readonly Color32[,] INVITE_TO_MIGRATE = Operation.GetFromFile("PersistentData/Textures/InviteToMigrate.png");
	public static bool IsMigrateInviting {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(905, 397, 110, 28);
			return ApproximatelyRect(realColors, INVITE_TO_MIGRATE) > 0.99F;
		}
	}

	private static readonly Color32[,] FRIENDLY_HINT = Operation.GetFromFile("PersistentData/Textures/FriendlyHint.png");
	public static bool IsFriendlyHinting {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(905, 395, 110, 28);
			return ApproximatelyRect(realColors, FRIENDLY_HINT) > 0.99F;
		}
	}

	private static readonly Color32[,] FIGHTING_ABORT = Operation.GetFromFile("PersistentData/Textures/FightingAbort.png");
	public static bool IsFightingAborting {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(910, 440, 100, 26);
			return ApproximatelyRect(realColors, FIGHTING_ABORT) > 0.99F;
		}
	}

	private static readonly Color32[,] MORE_GROUP = Operation.GetFromFile("PersistentData/Textures/MoreGroup.png");
	public static bool IsMoreGroupPopup {
		get {
			Color32[,] realColors = Operation.GetColorsOnScreen(910, 298, 100, 26);
			return ApproximatelyRect(realColors, MORE_GROUP) > 0.99F;
		}
	}
}
