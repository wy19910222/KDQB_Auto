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
	
	[MenuItem("Window/Follow")]
	private static void Open() {
		GetWindow<FollowConfig>("跟车").Show();
	}
	
	private void OnGUI() {
		Follow.KEEP_NO_WINDOW = EditorGUILayout.Toggle("在外面跟车", Follow.KEEP_NO_WINDOW);
		Follow.GROUP_COUNT = EditorGUILayout.IntSlider("拥有行军队列", Follow.GROUP_COUNT, 0, 7);
		Follow.ENABLED_WITH_COVERED = EditorGUILayout.Toggle("有窗口覆盖时是否生效", Follow.ENABLED_WITH_COVERED);
		
		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);
		
		Follow.SINGLE_GROUP = EditorGUILayout.Toggle("单队列跟车", Follow.SINGLE_GROUP);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("跟车延迟", GUILayout.Width(EditorGUIUtility.labelWidth));
		Follow.FOLLOW_DELAY_MIN = EditorGUILayout.FloatField(Follow.FOLLOW_DELAY_MIN, GUILayout.Width(60F));
		EditorGUILayout.MinMaxSlider(ref Follow.FOLLOW_DELAY_MIN, ref Follow.FOLLOW_DELAY_MAX, 0, 10);
		Follow.FOLLOW_DELAY_MAX = EditorGUILayout.FloatField(Follow.FOLLOW_DELAY_MAX, GUILayout.Width(60F));
		EditorGUILayout.EndHorizontal();
		Follow.FOLLOW_COOLDOWN = Mathf.Max(EditorGUILayout.FloatField("同一人跟车冷却", Follow.FOLLOW_COOLDOWN), 20F);
		
		Rect rect2 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect2 = new Rect(rect2.x, rect2.y + 4.5F, rect2.width, 1);
		EditorGUI.DrawRect(wireRect2, Color.gray);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.BeginVertical();
		CustomField("跟战锤", ref Follow.INCLUDE_ZC, 50);
		CustomField("跟难民营", ref Follow.INCLUDE_NMY, 10);
		CustomField("跟惧星", ref Follow.INCLUDE_JX, 10);
		EditorGUILayout.EndVertical();
		EditorGUILayout.BeginVertical();
		CustomField("跟黑暗军团据点", ref Follow.INCLUDE_JD, 50);
		CustomField("跟爱心砰砰", ref Follow.INCLUDE_AXPP, 50);
		CustomField("跟黑暗精卫", ref Follow.INCLUDE_JW, 50);
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		if (Follow.INCLUDE_JX > 0) {
			GUILayout.Space(5F);
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
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(16F);
				if (GUILayout.Button("测试")) {
					Debug.LogError(Follow.IsFollowOwnerEnabled());
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		GUILayout.Space(5F);
		if (Follow.IsRunning) {
			if (GUILayout.Button("关闭")) {
				EditorApplication.ExecuteMenuItem("Assets/StopFollow");
			}
		} else {
			if (GUILayout.Button("开启")) {
				EditorApplication.ExecuteMenuItem("Assets/StartFollow");
			}
		}
	}

	private static void CustomField(string label, ref int count, int defaultValue) {
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
	}
}

public class Follow {
	public static bool KEEP_NO_WINDOW = true;	// 是否在非跟车界面跟车
	public static int GROUP_COUNT = 4;	// 拥有行军队列数
	public static bool ENABLED_WITH_COVERED = true;	// 有窗口覆盖时是否生效
	public static bool SINGLE_GROUP = true;	// 是否单队列跟车
	public static float FOLLOW_DELAY_MIN = 1F;	// 跟车延迟
	public static float FOLLOW_DELAY_MAX = 5F;	// 跟车延迟
	public static float FOLLOW_COOLDOWN = 20F;	// 同一人跟车冷却
	
	public static int INCLUDE_JD = 50;	// 是否跟据点
	public static int INCLUDE_ZC = 50;	// 是否跟战锤
	public static int INCLUDE_JW = 0;	// 是否跟精卫
	public static int INCLUDE_NMY = 10;	// 是否跟难民营
	public static int INCLUDE_AXPP = 50;	// 是否跟爱心砰砰
	public static int INCLUDE_JX = 10;	// 是否跟惧星
	public static readonly Dictionary<string, Color32[,]> OwnerNameDict = new Dictionary<string, Color32[,]>(); // 记录下来的车主昵称
	public static readonly Dictionary<string, bool> OwnerEnabledDict = new Dictionary<string, bool>();	// 记录下来的要跟车的车主
	
	private static Color32[,] s_CachedOwnerName;	// 缓存的车主昵称
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartFollow", priority = -1)]
	private static void Enable() {
		Disable();
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
		List<string> switches = new List<string>();
		if (INCLUDE_ZC > 0) { switches.Add($"战锤{INCLUDE_ZC}次"); }
		if (INCLUDE_NMY > 0) { switches.Add($"难民营{INCLUDE_NMY}次"); }
		if (INCLUDE_JX > 0) { switches.Add($"惧星{INCLUDE_JX}次"); }
		if (INCLUDE_JD > 0) { switches.Add($"据点{INCLUDE_JD}次"); }
		if (INCLUDE_JW > 0) { switches.Add($"精卫{INCLUDE_JW}次"); }
		if (INCLUDE_AXPP > 0) { switches.Add($"砰砰{INCLUDE_AXPP}次"); }
		Debug.Log($"自动跟车已开启：{string.Join("、", switches)}");
	}

	[MenuItem("Assets/StopFollow", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动跟车已关闭");
		}
	}

	private static IEnumerator Update() {
		long cooldownTime = 0;
		while (true) {
			yield return null;
			// 队列数量
			int busyGroupCount = Recognize.BusyGroupCount;
			if (busyGroupCount >= GROUP_COUNT) {
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
				if (ENABLED_WITH_COVERED) {
					while (Recognize.IsWindowCovered) {	// 如果有窗口，多点几次返回按钮
						Debug.Log("关闭窗口");
						Operation.Click(735, 128);	// 左上角返回按钮
						yield return new EditorWaitForSeconds(0.1F);
					}
				} else {
					if (Recognize.IsWindowCovered) {
						continue;
					}
				}
				Debug.Log("外面加入按钮");
				Operation.Click(1771, 714);	// 加入按钮
				followWindowOpened = true;
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
			// 是否已显示Icon
			if (!Recognize.IsFollowIconExist) {
				Debug.Log("未显示Icon");
				goto EndOfFollow;
			}
			bool willFollow = false;
			if (Recognize.IsJDCanFollow) {
				// 如果是黑暗军团据点
				if (INCLUDE_JD > 0) {
					willFollow = true;
					--INCLUDE_JD;
				} else {
					Debug.Log("不跟黑暗军团据点");
				}
			} else if (Recognize.IsZCCanFollow) {
				// 如果是战锤
				if (INCLUDE_ZC > 0) {
					willFollow = true;
					--INCLUDE_ZC;
				} else {
					Debug.Log("不跟战锤");
				}
			} else if (Recognize.IsAXPPCanFollow) {
				// 如果是爱心砰砰
				if (INCLUDE_AXPP > 0) {
					willFollow = true;
					--INCLUDE_AXPP;
				} else {
					Debug.Log("不跟爱心砰砰");
				}
			} else if (Recognize.IsNMYCanFollow) {
				// 如果是难民营
				if (INCLUDE_NMY > 0) {
					willFollow = true;
					--INCLUDE_NMY;
				} else {
					Debug.Log("不跟难民营");
				}
			} else if (Recognize.IsJWCanFollow) {
				// 如果是精卫
				if (INCLUDE_JW > 0) {
					willFollow = true;
					--INCLUDE_JW;
				} else {
					Debug.Log("不跟精卫");
				}
			} else if (Recognize.IsJXCanFollow) {
				// 如果是惧星
				if (INCLUDE_JX > 0) {
					// 如果不跟该车主
					if (IsFollowOwnerEnabled()) {
						willFollow = true;
						--INCLUDE_JX;
						string filePath = $"{Application.dataPath}/{DateTime.Now:MM-dd_HH.mm.ss.fff}.png";
						Debug.LogError(filePath);
						ScreenshotUtils.Screenshot(OWNER_NAME_RECT.x, OWNER_NAME_RECT.y, OWNER_NAME_RECT.width, OWNER_NAME_RECT.height, filePath);
					} else {
						Debug.Log("不跟该车主");
					}
				} else {
					Debug.Log("不跟惧星");
				}
			}
			if (!willFollow) {
				goto EndOfFollow;
			}
			bool isFollowIconExist = Recognize.IsFollowIconExist;
			bool isJDCanFollow = Recognize.IsJDCanFollow;
			bool isZCCanFollow = Recognize.IsZCCanFollow;
			bool isAXPPCanFollow = Recognize.IsAXPPCanFollow;
			bool isNMYCanFollow = Recognize.IsNMYCanFollow;
			bool isJWCanFollow = Recognize.IsJWCanFollow;
			bool isJXCanFollow = Recognize.IsJXCanFollow;
			Debug.LogWarning($"isFollowIconExist:{isFollowIconExist}\n" +
					$"isJDCanFollow:{isJDCanFollow}\n" +
					$"isZCCanFollow:{isZCCanFollow}\n" +
					$"isAXPPCanFollow:{isAXPPCanFollow}\n" +
					$"isNMYCanFollow:{isNMYCanFollow}\n" +
					$"isJWCanFollow:{isJWCanFollow}\n" +
					$"isJXCanFollow:{isJXCanFollow}");
			if (isFollowIconExist &&
					!isJDCanFollow &&
					!isZCCanFollow &&
					!isAXPPCanFollow &&
					!isNMYCanFollow &&
					!isJWCanFollow &&
					!isJXCanFollow) {
				string filePath = $"{Application.dataPath}/{DateTime.Now:MM-dd_HH.mm.ss.fff}.png";
				Debug.LogError(filePath);
				ScreenshotUtils.Screenshot(988, 184, 214, 165, filePath);
			}
			
			Debug.Log("可以跟车");
			// 如果车主换人了，则直接结束冷却
			Color32[,] ownerName = ScreenshotUtils.GetColorsOnScreen(OWNER_NAME_RECT.x, OWNER_NAME_RECT.y, OWNER_NAME_RECT.width, OWNER_NAME_RECT.height);
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
			Debug.Log("士兵卡片");
			Operation.Click(1458, 962);	// 士兵卡片
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
				// 跟车冷却
				cooldownTime = DateTime.Now.Ticks + Mathf.RoundToInt(FOLLOW_COOLDOWN * 10000000);
			}
			// 如果还停留在出征界面，则退出
			if (Recognize.CurrentScene == Recognize.Scene.ARMY_SELECTING) {
				Debug.Log("退出按钮");
				Operation.Click(30, 140);	// 退出按钮
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("确认退出按钮");
				Operation.Click(1064, 634);	// 确认退出按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			EndOfFollow:
			if (followWindowOpened) {
				// 如果是从外面进来的，则关闭跟车界面
				Debug.Log("左上角返回按钮");
				while (Recognize.IsWindowCovered) {
					Operation.Click(735, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.2F);
				}
				// 外面的按钮持续几秒钟才消失
				yield return new EditorWaitForSeconds(1F);
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
	
	private static readonly RectInt OWNER_NAME_RECT = new RectInt(804, 193, 114, 24);	// 集结发起人昵称范围
	public static void RecordFollowOwnerName(string ownerName) {
		Color32[,] colors = ScreenshotUtils.GetColorsOnScreen(OWNER_NAME_RECT.x, OWNER_NAME_RECT.y, OWNER_NAME_RECT.width, OWNER_NAME_RECT.height);
		OwnerNameDict[ownerName ?? ""] = colors;
	}
	public static void RemoveFollowOwnerName(string ownerName) {
		if (ownerName != null) {
			OwnerNameDict.Remove(ownerName);
		}
	}
	public static void LogFollowOwnerNameSimilarity(string ownerName) {
		Color32[,] realColors = ScreenshotUtils.GetColorsOnScreen(OWNER_NAME_RECT.x, OWNER_NAME_RECT.y, OWNER_NAME_RECT.width, OWNER_NAME_RECT.height);
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
		Color32[,] realColors = ScreenshotUtils.GetColorsOnScreen(OWNER_NAME_RECT.x, OWNER_NAME_RECT.y, OWNER_NAME_RECT.width, OWNER_NAME_RECT.height);
		foreach (var ownerName in OwnerNameDict.Keys) {
			// 判断是否允许跟该车主
			if (OwnerEnabledDict.TryGetValue(ownerName, out bool enabled) && enabled) {
				// 判断是否是该车主
				Color32[,] targetColors = OwnerNameDict[ownerName] ?? new Color32[0, 0];
				if (Recognize.ApproximatelyRect(realColors, targetColors) > 0.99F) {
					Debug.LogError($"车主为{ownerName}，可以跟车");
					return true;
				}
			}
		}
		return false;
	}
}
