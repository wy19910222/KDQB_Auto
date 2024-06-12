/*
 * @Author: wangyun
 * @CreateTime: 2023-09-28 02:57:33 976
 * @LastEditor: wangyun
 * @EditTime: 2023-09-28 02:57:33 983
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Follow {
	public static bool KEEP_NO_WINDOW = true;	// 是否在非跟车界面跟车
	public static bool SINGLE_GROUP = true;	// 是否单队列跟车
	public static float FOLLOW_DELAY_MIN = 1F;	// 跟车延迟
	public static float FOLLOW_DELAY_MAX = 5F;	// 跟车延迟
	public static float FOLLOW_COOLDOWN = 20F;	// 同一人跟车冷却
	public static bool FEAR_STAR_FIRST = true;	// 惧星没跟完不跟其他
	
	public static bool RESET_DAILY = true;	// 每日重置次数
	public static DateTime LAST_RESET_TIME;	// 上次重置时间
	
	public static bool FEAR_STAR_HELP_ENABLED;	// 帮打惧星开关
	public static string FEAR_STAR_HELP_OWNER = null;	// 帮打惧星跟车的车主
	public static int FEAR_STAR_HELP_SQUAD_NUMBER = 1;	// 帮打惧星编队号码
	public static bool FEAR_STAR_HELP_MUST_FULL_SOLDIER = true;	// 帮打惧星必须满兵
	
	public static readonly Dictionary<Recognize.FollowType, int> TypeCountDict = new Dictionary<Recognize.FollowType, int>(); // 各类型次数
	public static readonly Dictionary<Recognize.FollowType, bool> TypeWillResetDict = new Dictionary<Recognize.FollowType, bool>(); // 各类型每日是否重置次数
	public static readonly Dictionary<Recognize.FollowType, bool> TypeCanOuterDict = new Dictionary<Recognize.FollowType, bool>(); // 各类型是否支持在外面跟车
	public static readonly Dictionary<string, Color32[,]> OwnerNameDict = new Dictionary<string, Color32[,]>(); // 记录下来的车主昵称
	public static readonly Dictionary<string, bool> OwnerEnabledDict = new Dictionary<string, bool>();	// 记录下来的要跟车的车主
	
	private static Color32[,] s_CachedOwnerName;	// 缓存的车主昵称
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartFollow", priority = -1)]
	private static void Enable() {
		Disable();
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
		List<string> switches = new List<string>();
		foreach (var (type, count) in TypeCountDict) {
			if (count > 0) {
				switches.Add($"{Utils.GetEnumInspectorName(type)}{count}次");
			}
		}
		Debug.Log($"自动跟车已开启：{string.Join("、", switches)}");
	}

	[MenuItem("Tools_Task/StopFollow", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动跟车已关闭");
		}
	}

	public static int GetDefaultCount(Recognize.FollowType type) {
		return type switch {
			Recognize.FollowType.UNKNOWN => 50,
			Recognize.FollowType.WAR_HAMMER => 50,
			Recognize.FollowType.REFUGEE_CAMP => 10,
			Recognize.FollowType.FEAR_STAR => 10,
			Recognize.FollowType.STRONGHOLD => 50,
			Recognize.FollowType.ELITE_GUARD => 50,
			Recognize.FollowType.HEART_PANG => 50,
			_ => TypeCountDict[type]
		};
	}

	private static IEnumerator Update() {
		long cooldownTime = 0;
		while (true) {
			yield return null;
			if (RESET_DAILY) {
				DateTime date = DateTime.Now.Date;
				if (LAST_RESET_TIME < date) {
					foreach (var (followType, willReset) in TypeWillResetDict) {
						if (willReset) {
							TypeCountDict[followType] = GetDefaultCount(followType);
						}
					}
					LAST_RESET_TIME = date;
				}
			}
			
			// 队列数量
			if (!Recognize.IsAnyGroupIdle) {
				continue;
			}
			// 如果单队列跟车
			if (SINGLE_GROUP) {
				bool gatherExist = false;
				for (int i = 0; i < Recognize.BusyGroupCount; ++i) {
					if (Recognize.GetGroupState(i) == Recognize.GROUP_STATE_GATHER) {
						gatherExist = true;
						break;
					}
				}
				if (gatherExist) {
					// 如果已经有队列在集结，则不跟车
					continue;
				}
			}
			bool followWindowOpened = false;
			// 是否有加入按钮
			if (!Recognize.IsFollowJoinBtnExist) {
				// 是否在非跟车界面跟车
				if (!KEEP_NO_WINDOW) {
					// 如果不在跟车界面，但要在跟车界面跟车，则不符合跟车条件
					continue;
				}
				if (!Recognize.IsFollowOuterJoinBtnExist) {
					continue;
				}
				// 如果有界面覆盖，则说明正在操作别的
				if (Recognize.IsWindowCovered) {
					continue;
				}
				// 如果所有支持在外面跟车的类别都没次数了
				bool anyoneCanOuter = false;
				foreach (var (followType, canOuter) in TypeCanOuterDict) {
					if (canOuter && TypeCountDict[followType] > 0) {
						anyoneCanOuter = true;
						break;
					}
				}
				if (!anyoneCanOuter) {
					continue;
				}
				
				followWindowOpened = true;
			}

			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(Follow);
			
			if (followWindowOpened) {
				Debug.Log("外面加入按钮");
				Operation.Click(1771, 714);	// 加入按钮
				yield return new EditorWaitForSeconds(0.2F);
				// 是否有加入按钮(切后台可能导致动画阻塞，从而外面有按钮实际集结已结束)
				if (!Recognize.IsFollowJoinBtnExist) {
					goto EndOfFollow;
				}
			}
			// 有时候会误判图标，所以尝试等一会儿
			yield return new EditorWaitForSeconds(0.1F);
			// 是否已加入
			if (Recognize.HasFollowJoined) {
				goto EndOfFollow;
			}
			Debug.Log("未加入");
			Recognize.FollowType type = Recognize.GetFollowType();
			int fearStarCount = TypeCountDict[Recognize.FollowType.FEAR_STAR];
			bool isFearStarNeedHelp = false;
			int count = 0;
			switch (type) {
				case Recognize.FollowType.NONE:
					Debug.Log("未显示Icon，不跟车");
					goto EndOfFollow;
				case Recognize.FollowType.UNKNOWN:
				case Recognize.FollowType.WAR_HAMMER:
				case Recognize.FollowType.REFUGEE_CAMP:
				case Recognize.FollowType.STRONGHOLD:
				case Recognize.FollowType.ELITE_GUARD:
				case Recognize.FollowType.HEART_PANG:
					count = FEAR_STAR_FIRST && fearStarCount > 0 ? 0 : TypeCountDict[type];
					if (count <= 0) {
						Debug.Log($"不跟{type}");
						goto EndOfFollow;
					}
					break;
				case Recognize.FollowType.FEAR_STAR:
					count = fearStarCount;
					isFearStarNeedHelp = IsFearStarNeedHelp();
					if (!isFearStarNeedHelp) {
						if (count <= 0) {
							Debug.Log($"不跟{type}");
							goto EndOfFollow;
						}
						if (!IsFollowOwnerEnabled()) {
							Debug.Log($"不跟该车主的{type}");
							goto EndOfFollow;
						}
					}
					break;
			}
			
			Debug.Log("可以跟车");
			// 如果车主换人了，则直接结束冷却
			Color32[,] ownerName = Operation.GetColorsOnScreen(OWNER_NAME_RECT.x, OWNER_NAME_RECT.y, OWNER_NAME_RECT.width, OWNER_NAME_RECT.height);
			if (s_CachedOwnerName == null || Recognize.ApproximatelyRect(ownerName, s_CachedOwnerName) < 0.9F) {
				// 如果车主换人了，则直接结束冷却
				cooldownTime = 0;
			}
			// 如果还在冷却中，则不加入
			if (DateTime.Now.Ticks < cooldownTime) {
				goto EndOfFollow;
			}
			Debug.Log("决定跟车");
			bool maybeSucceed = false;
			int busyGroupCount = Recognize.BusyGroupCount;
			s_CachedOwnerName = ownerName;	// 记录车主
			float delay = UnityEngine.Random.Range(FOLLOW_DELAY_MIN, FOLLOW_DELAY_MAX);	// 额外随机延迟量
			Debug.Log("加入按钮");
			Operation.Click(968, 307);	// 加入按钮
			yield return new EditorWaitForSeconds(0.3F);
			if (Recognize.IsFollowConfirming) {
				Debug.Log("确定加入按钮");
				Operation.Click(960, 670);	// 确定按钮
			}
			// 等待进入战斗界面，如果超过1秒钟没进去，说明进入失败，结束流程
			for (int i = 0; i < 10 && Recognize.CurrentScene != Recognize.Scene.FIGHTING_MARCH; ++i) {
				if (i == 9) {
					goto EndOfFollow;
				}
				yield return new EditorWaitForSeconds(0.1F);
			}
			if (isFearStarNeedHelp) {
				Debug.Log("选择编队");
				Operation.Click(1145 + 37 * FEAR_STAR_HELP_SQUAD_NUMBER, 870);	// 选择编队
				yield return new EditorWaitForSeconds(0.2F);
				if ((!FEAR_STAR_HELP_MUST_FULL_SOLDIER || Recognize.FightingSoldierCountPercent > 0.99F) && Recognize.FightingHeroEmptyCount == 0) {
					Debug.Log("出征按钮");
					Operation.Click(960, 470);	// 出征按钮
					yield return new EditorWaitForSeconds(0.2F);
					// 如果出现赶不上弹框，则取消出征
					if (Recognize.IsTooLateWindowExist) {
						maybeSucceed = false;
						Debug.Log("取消按钮");
						Operation.Click(900, 657);	// 取消按钮
						yield return new EditorWaitForSeconds(0.5F);
					} else {
						maybeSucceed = true;
					}
				} else {
					Debug.Log("退出按钮");
					Operation.Click(30, 140);	// 退出按钮
					yield return new EditorWaitForSeconds(0.2F);
					Debug.Log("确认退出按钮");
					Operation.Click(1064, 634);	// 确认退出按钮
					yield return new EditorWaitForSeconds(0.5F);
				}
			} else {
				Debug.Log("士兵卡片");
				Operation.Click(1160, 962);	// 士兵卡片
				yield return new EditorWaitForSeconds(Mathf.Max(delay, 0.1F));
				Debug.Log("出征按钮");
				Operation.Click(961, 476);	// 出征按钮
				yield return new EditorWaitForSeconds(0.2F);
				// 如果出现赶不上弹框，则取消出征
				if (Recognize.IsTooLateWindowExist) {
					maybeSucceed = false;
					Debug.Log("取消按钮");
					Operation.Click(900, 657);	// 取消按钮
					// 车主变化前不再上车
					cooldownTime = long.MaxValue;
					yield return new EditorWaitForSeconds(0.5F);
				} else {
					maybeSucceed = true;
					// 跟车冷却
					cooldownTime = DateTime.Now.Ticks + Mathf.RoundToInt(FOLLOW_COOLDOWN * 10000000);
				}
			}
			// 如果还停留在出征界面，则退出
			if (Recognize.CurrentScene == Recognize.Scene.FIGHTING_MARCH) {
				maybeSucceed = false;
				Debug.Log("退出按钮");
				Operation.Click(30, 140);	// 退出按钮
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("确认退出按钮");
				Operation.Click(1064, 634);	// 确认退出按钮
				// 等待退出战斗界面
				for (int i = 0; i < 10 && Recognize.CurrentScene == Recognize.Scene.FIGHTING_MARCH; i++) {
					yield return new EditorWaitForSeconds(0.1F);
				}
			}
			yield return new EditorWaitForSeconds(0.2F);
			if (maybeSucceed && Recognize.BusyGroupCount > busyGroupCount) {
				Debug.Log("跟车成功");
				// 跟车次数减1
				if (count > 0) {
					TypeCountDict[type] = count - 1;
				}
			} else {
				Debug.LogError("跟车失败");
			}
			EndOfFollow:
			if (followWindowOpened || GlobalStatus.IsUnattended) {
				// 如果是从外面进来的，则关闭跟车界面
				Debug.Log("左上角返回按钮");
				for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.3F);
					if (i == 9) {
						Debug.LogError("10次都没关掉");
					}
				}
			}
			Task.CurrentTask = null;
			if (followWindowOpened) {
				// 外面的按钮持续几秒钟才消失
				yield return new EditorWaitForSeconds(2F);
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
	
	private static readonly RectInt OWNER_NAME_RECT = new RectInt(804, 193, 114, 24);	// 集结发起人昵称范围
	public static void RecordFollowOwnerName(string ownerName) {
		Color32[,] colors = Operation.GetColorsOnScreen(OWNER_NAME_RECT.x, OWNER_NAME_RECT.y, OWNER_NAME_RECT.width, OWNER_NAME_RECT.height);
		OwnerNameDict[ownerName ?? ""] = colors;
	}
	public static void RemoveFollowOwnerName(string ownerName) {
		if (ownerName != null) {
			OwnerNameDict.Remove(ownerName);
		}
	}
	public static void LogFollowOwnerNameSimilarity(string ownerName) {
		Color32[,] realColors = Operation.GetColorsOnScreen(OWNER_NAME_RECT.x, OWNER_NAME_RECT.y, OWNER_NAME_RECT.width, OWNER_NAME_RECT.height);
		Color32[,] targetColors = OwnerNameDict[ownerName ?? ""] ?? new Color32[0, 0];
		Debug.Log(Recognize.ApproximatelyRect(realColors, targetColors));
	}
	public static bool IsFollowOwnerEnabled() {
		// 全部没打勾，表示可以跟任何人的车
		bool everyOneEnabled = true;
		foreach (var ownerName in OwnerNameDict.Keys) {
			if (OwnerEnabledDict.TryGetValue(ownerName, out bool enabled) && enabled) {
				everyOneEnabled = false;
			}
		}
		if (everyOneEnabled) {
			return true;
		}
		// 判断车主
		Color32[,] realColors = Operation.GetColorsOnScreen(OWNER_NAME_RECT.x, OWNER_NAME_RECT.y, OWNER_NAME_RECT.width, OWNER_NAME_RECT.height);
		foreach (var ownerName in OwnerNameDict.Keys) {
			// 判断是否允许跟该车主
			if (OwnerEnabledDict.TryGetValue(ownerName, out bool enabled) && enabled) {
				// 判断是否是该车主
				Color32[,] targetColors = OwnerNameDict[ownerName] ?? new Color32[0, 0];
				if (Recognize.ApproximatelyRect(realColors, targetColors) > 0.99F) {
					return true;
				}
			}
		}
		return false;
	}
	public static bool IsFearStarNeedHelp() {
		// 判断是否开启帮打
		if (FEAR_STAR_HELP_ENABLED) {
			// 判断是否是帮打车主
			Color32[,] realColors = Operation.GetColorsOnScreen(OWNER_NAME_RECT.x, OWNER_NAME_RECT.y, OWNER_NAME_RECT.width, OWNER_NAME_RECT.height);
			if (OwnerNameDict.TryGetValue(FEAR_STAR_HELP_OWNER, out Color32[,] targetColors) && targetColors != null) {
				if (Recognize.ApproximatelyRect(realColors, targetColors) > 0.99F) {
					return true;
				}
			}
		}
		return false;
	}
}
