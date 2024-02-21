/*
 * @Author: wangyun
 * @CreateTime: 2023-12-30 00:36:09 952
 * @LastEditor: wangyun
 * @EditTime: 2023-12-30 00:36:09 956
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class AllianceHelpConfig : PrefsEditorWindow<AllianceHelp> {
	[MenuItem("Tools_Window/Default/AllianceHelp", false, 23)]
	private static void Open() {
		GetWindow<AllianceHelpConfig>("联盟帮助").Show();
	}
	
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("帮助类型：", GUILayout.Width(EditorGUIUtility.labelWidth - 2F));
		AllianceHelp.OUTER_HELPS = GUILayout.Toggle(AllianceHelp.OUTER_HELPS, "在外面帮助他人", "Button");
		AllianceHelp.INTO_REQUEST = GUILayout.Toggle(AllianceHelp.INTO_REQUEST, "去里面请求帮助", "Button");
		EditorGUILayout.EndHorizontal();
		if (AllianceHelp.INTO_REQUEST) {
			for (int i = AllianceHelp.TARGET_LIST.Count; i < 4; ++i) {
				AllianceHelp.TARGET_LIST.Add(0);
			}
			for (int i = 0; i < 4; ++i) {
				EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginChangeCheck();
				int count = AllianceHelp.TARGET_LIST[i];
				int newCount = Math.Max(EditorGUILayout.IntField($"    目标{i + 1}", Math.Abs(count)), 0);
				if (EditorGUI.EndChangeCheck()) {
					count = count < 0 ? -newCount : newCount;
					AllianceHelp.TARGET_LIST[i] = count;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Toggle(count > 0, GUILayout.Width(16F));
				if (EditorGUI.EndChangeCheck()) {
					count = count == 0 ? 999 : -count;
					AllianceHelp.TARGET_LIST[i] = count;
				}
				EditorGUILayout.EndHorizontal();
			}
			
			AllianceHelp.INTO_HELPS_TIMES = EditorGUILayout.IntSlider("在里面帮助检查次数", AllianceHelp.INTO_HELPS_TIMES, 0, 10);
			AllianceHelp.INTERVAL = EditorGUILayout.IntSlider("尝试请求间隔（秒）", AllianceHelp.INTERVAL, 1800, 7200);
			bool started = AllianceHelp.s_StartTime > (DateTime.Now - new TimeSpan(0, 1, 0)).Date;
			if (started) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("今天已请求", GUILayout.Width(EditorGUIUtility.labelWidth - 2F));
				if (GUILayout.Button("取消已请求")) {
					AllianceHelp.s_StartTime = DateTime.Now - new TimeSpan(24, 0, 0);
				}
				EditorGUILayout.EndHorizontal();
			} else {
				TimeSpan ts = AllianceHelp.s_NextTime - DateTime.Now;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField($"下一次尝试时间：{ts:hh\\:mm\\:ss}");
				if (GUILayout.Button("立即尝试")) {
					AllianceHelp.s_NextTime = DateTime.Now;
				}
				if (GUILayout.Button("设为已请求")) {
					AllianceHelp.s_StartTime = DateTime.Now;
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		GUILayout.Space(5F);
		if (AllianceHelp.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
	}
}

public class AllianceHelp {
	public static bool OUTER_HELPS = false;	// 在外面帮助他人
	public static bool INTO_REQUEST = false;	// 去里面请求帮助
	public static int INTO_HELPS_TIMES = 5;	// 去里面请求帮助
	public static int INTERVAL = 3600;	// 尝试请求间隔
	
	public static readonly List<int> TARGET_LIST = new List<int>();	// 帮助物品随机范围
	
	public static DateTime s_StartTime;	// 起头时间
	public static DateTime s_NextTime = DateTime.Now;
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartAllianceHelp", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"联盟帮助已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopAllianceHelp", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("联盟帮助已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;

			if (Recognize.IsWindowCovered) {
				continue;
			}

			if (OUTER_HELPS) {
				// 在外面帮助
				if (Recognize.CanAllianceHelpOuter) {
					if (Task.CurrentTask == null) {
						Task.CurrentTask = nameof(AllianceHelp);
						Debug.Log("帮助气泡");
						// Operation.Click(1795, 716);	// 帮助气泡
						Operation.Click(1820, 725);	// 帮助气泡，避开跟车
						yield return new EditorWaitForSeconds(0.2F);
						// 有可能会点到上车提示气泡
						for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
							Debug.Log("关闭窗口");
							Operation.Click(720, 128);	// 左上角返回按钮
							yield return new EditorWaitForSeconds(0.2F);
						}
						Task.CurrentTask = null;
					}
				}
			}

			if (INTO_REQUEST) {
				// 去里面请求帮助
				DateTime now = DateTime.Now;
				bool started = s_StartTime > (now - new TimeSpan(0, 1, 0)).Date;
				bool nextTimeNotReach = DateTime.Now < s_NextTime;
				if ((started || nextTimeNotReach) && !Recognize.IsAllianceHelpAwardOuter) {
					continue;
				}
	
				if (Task.CurrentTask != null) {
					continue;
				}
				Task.CurrentTask = nameof(AllianceHelp);
				
				if (!nextTimeNotReach) {
					s_NextTime = DateTime.Now + new TimeSpan(0, 0, INTERVAL);
				}
				
				
				Debug.Log("联盟按钮");
				Operation.Click(1870, 710);	// 联盟按钮
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("联盟帮助按钮");
				Operation.Click(1080, 620);	// 联盟帮助按钮
				yield return new EditorWaitForSeconds(0.5F);
				Debug.Log($"IsAllianceHelpAwardIntuitive: {Recognize.IsAllianceHelpAwardIntuitive}");
				if (Recognize.IsAllianceHelpAwardIntuitive) {
					Debug.Log("领取奖励按钮");
					Operation.Click(1115, 895);	// 领取奖励按钮
					yield return new EditorWaitForSeconds(1F);
					Debug.Log("点空白处关闭恭喜获得界面");
					Operation.Click(1115, 895);	// 点空白处关闭恭喜获得界面
					yield return new EditorWaitForSeconds(0.5F);
				}
				Debug.Log($"CanAllianceHelpRequest: {Recognize.CanAllianceHelpRequest}");
				if (Recognize.CanAllianceHelpRequest) {
					// 确定帮助物品
					int target = RandomTarget();
					if (target != -1) {
						Debug.Log("请求帮助按钮");
						Operation.Click(1115, 895);	// 请求帮助按钮
						yield return new EditorWaitForSeconds(0.5F);
						Debug.Log("选择帮助物品");
						Operation.Click(795 + 109 * target, 410);	// 选择帮助物品
						yield return new EditorWaitForSeconds(0.3F);
						Debug.Log("请求帮助按钮");
						Operation.Click(960, 750);	// 请求帮助按钮
						if (TARGET_LIST[target] < 999) {
							--TARGET_LIST[target];
						}
						yield return new EditorWaitForSeconds(0.5F);
						if (!started) {
							s_StartTime = DateTime.Now;
						}
					}
				}
				for (int i = 0; i < INTO_HELPS_TIMES; ++i) {
					if (Recognize.CanAllianceHelpOthers) {
						Debug.Log("帮助全部按钮");
						Operation.Click(960, 780);	// 帮助全部按钮
					}
					yield return new EditorWaitForSeconds(0.2F);
				}
				
				for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {
					Debug.Log("左上角返回按钮");
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.2F);
				}
	
				Task.CurrentTask = null;
				
				yield return new EditorWaitForSeconds(5F);
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
	
	private static int RandomTarget() {
		List<int> list = new List<int>();
		for (int i = 0, length = TARGET_LIST.Count; i < length; ++i) {
			if (TARGET_LIST[i] > 0) {
				list.Add(i);
			}
		}
		if (list.Count <= 0) {
			return -1;
		}
		return list[Random.Range(0, list.Count)];
	}
}
