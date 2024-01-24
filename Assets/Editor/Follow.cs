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

using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class FollowConfig : PrefsEditorWindow<Follow> {
	private string m_TempJXOwnerName;
	private Vector2 m_ScrollPos;
	
	[MenuItem("Tools_Window/Default/Follow", false, 0)]
	private static void Open() {
		GetWindow<FollowConfig>("跟车").Show();
	}
	
	private void OnGUI() {
		Follow.SINGLE_GROUP = EditorGUILayout.Toggle("单队列跟车", Follow.SINGLE_GROUP);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("跟车延迟", GUILayout.Width(EditorGUIUtility.labelWidth));
		GUILayoutOption optionWidth = GUILayout.Width(Mathf.Clamp(EditorGUIUtility.currentViewWidth * 0.5F - 110, 26, 50));
		Follow.FOLLOW_DELAY_MIN = EditorGUILayout.FloatField(Follow.FOLLOW_DELAY_MIN, optionWidth);
		EditorGUILayout.MinMaxSlider(ref Follow.FOLLOW_DELAY_MIN, ref Follow.FOLLOW_DELAY_MAX, 0, 10);
		Follow.FOLLOW_DELAY_MAX = EditorGUILayout.FloatField(Follow.FOLLOW_DELAY_MAX, optionWidth);
		EditorGUILayout.EndHorizontal();
		Follow.FOLLOW_COOLDOWN = EditorGUILayout.FloatField("同一人跟车冷却", Follow.FOLLOW_COOLDOWN);
		
		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);
		
		EditorGUILayout.BeginHorizontal();
		Follow.KEEP_NO_WINDOW = GUILayout.Toggle(Follow.KEEP_NO_WINDOW, "在外面跟车", "Button");
		Follow.FEAR_STAR_FIRST = GUILayout.Toggle(Follow.FEAR_STAR_FIRST, "惧星优先", "Button");
		bool newResetDaily = GUILayout.Toggle(Follow.RESET_DAILY, "每日重置次数", "Button");
		if (newResetDaily != Follow.RESET_DAILY) {
			Follow.RESET_DAILY = newResetDaily;
			if (newResetDaily) {
				Follow.LAST_RESET_TIME = DateTime.Now.Date;
			}
		}
		EditorGUILayout.EndHorizontal();
		
		Rect rect2 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect2 = new Rect(rect2.x, rect2.y + 4.5F, rect2.width, 1);
		EditorGUI.DrawRect(wireRect2, Color.gray);

		if (EditorGUIUtility.currentViewWidth > 460) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			CustomField(Recognize.FollowType.WAR_HAMMER);
			CustomField(Recognize.FollowType.REFUGEE_CAMP);
			CustomField(Recognize.FollowType.FEAR_STAR);
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical();
			CustomField(Recognize.FollowType.STRONGHOLD);
			CustomField(Recognize.FollowType.ELITE_GUARD);
			CustomField(Recognize.FollowType.HEART_PANG);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		} else {
			CustomField(Recognize.FollowType.STRONGHOLD);
			CustomField(Recognize.FollowType.ELITE_GUARD);
			CustomField(Recognize.FollowType.HEART_PANG);
			GUILayout.Space(5F);
			CustomField(Recognize.FollowType.WAR_HAMMER);
			CustomField(Recognize.FollowType.REFUGEE_CAMP);
			CustomField(Recognize.FollowType.FEAR_STAR);
		}
		if (Follow.TypeCountDict.TryGetValue(Recognize.FollowType.FEAR_STAR, out int count) && count > 0) {
			GUILayout.Space(5F);
			m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, GUILayout.MaxHeight(240F));
			foreach (var ownerName in new List<string>(Follow.OwnerNameDict.Keys)) {
				if (!Follow.OwnerEnabledDict.ContainsKey(ownerName)) {
					Follow.OwnerEnabledDict.Add(ownerName, false);
				}
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(16F);
				bool enabled = Follow.OwnerEnabledDict[ownerName];
				bool newEnabled = EditorGUILayout.Toggle($"{ownerName}的惧星", enabled);
				if (newEnabled != enabled) {
					Follow.OwnerEnabledDict[ownerName] = newEnabled;
				}
				if (m_Debug) {
					if (GUILayout.Button("更新", GUILayout.Width(60F))) {
						Follow.RecordFollowOwnerName(ownerName);
					}
					if (GUILayout.Button("判断", GUILayout.Width(60F))) {
						Follow.LogFollowOwnerNameSimilarity(ownerName);
					}
					if (GUILayout.Button("删除", GUILayout.Width(60F))) {
						Follow.RemoveFollowOwnerName(ownerName);
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			if (m_Debug) {
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(16F);
				m_TempJXOwnerName = EditorGUILayout.TextField(m_TempJXOwnerName);
				if (GUILayout.Button("添加", GUILayout.Width(60F))) {
					Follow.RecordFollowOwnerName(m_TempJXOwnerName);
					m_TempJXOwnerName = string.Empty;
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(16F);
				if (GUILayout.Button("测试")) {
					Debug.LogError(Follow.IsFollowOwnerEnabled());
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
		}
		GUILayout.Space(5F);
		if (Follow.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
	}

	private static void CustomField(Recognize.FollowType type) {
		Follow.TypeCountDict.TryGetValue(type, out int countHammer);
		Follow.TypeCountDict[type] = CustomField($"跟{Utils.GetEnumInspectorName(type)}", countHammer, Follow.GetDefaultCount(type));
	}

	private static int CustomField(string label, int count, int defaultValue) {
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		int newCount = Math.Max(EditorGUILayout.IntField(label, Math.Abs(count)), 0);
		if (EditorGUI.EndChangeCheck()) {
			count = count < 0 ? -newCount : newCount;
		}
		EditorGUI.BeginChangeCheck();
		bool isOpen = EditorGUILayout.Toggle(count > 0, GUILayout.Width(16F));
		if (EditorGUI.EndChangeCheck()) {
			if (isOpen && count == 0) {
				count = defaultValue;
			} else {
				count = -count;
			}
		}
		EditorGUILayout.EndHorizontal();
		return count;
	}
}

public class Follow {
	public static bool KEEP_NO_WINDOW = true;	// 是否在非跟车界面跟车
	public static bool SINGLE_GROUP = true;	// 是否单队列跟车
	public static float FOLLOW_DELAY_MIN = 1F;	// 跟车延迟
	public static float FOLLOW_DELAY_MAX = 5F;	// 跟车延迟
	public static float FOLLOW_COOLDOWN = 20F;	// 同一人跟车冷却
	public static bool FEAR_STAR_FIRST = true;	// 惧星没跟完不跟其他
	
	public static bool RESET_DAILY = true;	// 每日重置次数
	public static DateTime LAST_RESET_TIME;	// 上次重置时间
	
	public static readonly Dictionary<Recognize.FollowType, int> TypeCountDict = new Dictionary<Recognize.FollowType, int>(); // 各类型次数
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
					TypeCountDict[Recognize.FollowType.STRONGHOLD] = GetDefaultCount(Recognize.FollowType.STRONGHOLD);
					TypeCountDict[Recognize.FollowType.WAR_HAMMER] = GetDefaultCount(Recognize.FollowType.WAR_HAMMER);
					TypeCountDict[Recognize.FollowType.REFUGEE_CAMP] = GetDefaultCount(Recognize.FollowType.REFUGEE_CAMP);
					TypeCountDict[Recognize.FollowType.FEAR_STAR] = GetDefaultCount(Recognize.FollowType.FEAR_STAR);
					LAST_RESET_TIME = date;
				}
			}
			
			// 队列数量
			int busyGroupCount = Recognize.BusyGroupCount;
			if (busyGroupCount >= Recognize.GROUP_COUNT) {
				continue;
			}
			// 如果单队列跟车
			if (SINGLE_GROUP) {
				bool gatherExist = false;
				for (int i = 0; i < busyGroupCount; ++i) {
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
				followWindowOpened = true;
			}

			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(Follow);
			
			if (followWindowOpened) {
				Debug.Log("外面加入按钮");
				Operation.Click(1771, 714);	// 加入按钮
				yield return new EditorWaitForSeconds(0.1F);
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
			int count = 0;
			switch (type) {
				case Recognize.FollowType.UNKNOWN:
					Debug.Log("未知类型，不跟车");
					goto EndOfFollow;
				case Recognize.FollowType.NONE:
					Debug.Log("未显示Icon，不跟车");
					goto EndOfFollow;
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
					if (count <= 0) {
						Debug.Log($"不跟{type}");
						goto EndOfFollow;
					} else if (!IsFollowOwnerEnabled()) {
						Debug.Log($"不跟该车主的{type}");
						goto EndOfFollow;
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
			// 清除失败的记录
			s_CachedOwnerName = ownerName;
			float delay = Random.Range(FOLLOW_DELAY_MIN, FOLLOW_DELAY_MAX);
			Debug.Log("加入按钮");
			Operation.Click(968, 307);	// 加入按钮
			yield return new EditorWaitForSeconds(0.2F);
			yield return new EditorWaitForSeconds(delay * 0.5F);
			// 等待进入战斗界面，如果超过1秒钟没进去，说明进入失败，结束流程
			for (int i = 0; i < 10 && Recognize.CurrentScene != Recognize.Scene.FIGHTING; ++i) {
				if (i == 9) {
					goto EndOfFollow;
				}
				yield return new EditorWaitForSeconds(0.1F);
			}
			Debug.Log("士兵卡片");
			Operation.Click(1160, 962);	// 士兵卡片
			yield return new EditorWaitForSeconds(0.1F);
			yield return new EditorWaitForSeconds(delay * 0.5F);
			Debug.Log("出征按钮");
			Operation.Click(961, 476);	// 出征按钮
			yield return new EditorWaitForSeconds(0.2F);
			// 如果出现赶不上弹框，则取消出征
			if (Recognize.IsTooLateWindowExist) {
				Debug.Log("取消按钮");
				Operation.Click(900, 657);	// 取消按钮
				// 车主变化前不再上车
				cooldownTime = long.MaxValue;
				yield return new EditorWaitForSeconds(0.5F);
			} else {
				// 跟车次数减1
				TypeCountDict[type] = count - 1;
				// 跟车冷却
				cooldownTime = DateTime.Now.Ticks + Mathf.RoundToInt(FOLLOW_COOLDOWN * 10000000);
			}
			// 如果还停留在出征界面，则退出
			if (Recognize.CurrentScene == Recognize.Scene.FIGHTING) {
				Debug.Log("退出按钮");
				Operation.Click(30, 140);	// 退出按钮
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("确认退出按钮");
				Operation.Click(1064, 634);	// 确认退出按钮
				// 等待退出战斗界面
				for (int i = 0; i < 10 && Recognize.CurrentScene == Recognize.Scene.FIGHTING; i++) {
					yield return new EditorWaitForSeconds(0.1F);
				}
			}
			EndOfFollow:
			if (followWindowOpened) {
				// 如果是从外面进来的，则关闭跟车界面
				Debug.Log("左上角返回按钮");
				for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.2F);
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
}
