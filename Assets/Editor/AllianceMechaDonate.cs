/*
 * @Author: wangyun
 * @CreateTime: 2023-10-24 04:22:33 341
 * @LastEditor: wangyun
 * @EditTime: 2023-10-24 04:22:33 346
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class AllianceMechaDonateConfig : PrefsEditorWindow<AllianceMechaDonate> {
	[MenuItem("Tools_Window/Default/AllianceMechaDonate", false, 21)]
	private static void Open() {
		GetWindow<AllianceMechaDonateConfig>("联盟机甲捐献").Show();
	}
	
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		TimeSpan ts = AllianceMechaDonate.NEXT_TIME - DateTime.Now;
		int hours = EditorGUILayout.IntField("下次尝试时间", ts.Hours);
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 8F;
		int minutes = EditorGUILayout.IntField(":", ts.Minutes);
		int seconds = EditorGUILayout.IntField(":", ts.Seconds);
		EditorGUIUtility.labelWidth = prevLabelWidth;
		if (EditorGUI.EndChangeCheck()) {
			AllianceMechaDonate.NEXT_TIME = DateTime.Now + new TimeSpan(hours, minutes, seconds);
		}
		EditorGUILayout.EndHorizontal();
		AllianceMechaDonate.DONATE_COUNT = EditorGUILayout.IntSlider("捐献数量", AllianceMechaDonate.DONATE_COUNT, 1, 5);
		AllianceMechaDonate.INTERVAL = EditorGUILayout.IntSlider("尝试捐献间隔（秒）", AllianceMechaDonate.INTERVAL, 120, 1800);
		AllianceMechaDonate.COOL_DOWN = EditorGUILayout.IntSlider("捐献成功后冷却（小时）", AllianceMechaDonate.COOL_DOWN, 6, 12);
		GUILayout.Space(5F);
		EditorGUILayout.BeginHorizontal();
		if (AllianceMechaDonate.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
		if (GUILayout.Button("单次执行", GUILayout.Width(60F))) {
			EditorApplication.ExecuteMenuItem("Tools_Task/OnceAllianceMechaDonate");
		}
		EditorGUILayout.EndHorizontal();
	}
	
	private void Update() {
		Repaint();
	}
}

public class AllianceMechaDonate {
	public static int DONATE_COUNT = 3;	// 捐献数量
	public static int INTERVAL = 300;	// 点击间隔
	public static int COOL_DOWN = 6;	// 捐献冷却
	public static DateTime NEXT_TIME = DateTime.Now;
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartAllianceMechaDonate", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"联盟机甲捐献尝试已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopAllianceMechaDonate", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("联盟机甲捐献尝试已关闭");
		}
	}

	private static IEnumerator Update() {
		// bool prevIsMarshalTime = false;
		while (true) {
			yield return null;
			if (DateTime.Now < NEXT_TIME) {
				continue;
			}
			
			if (Recognize.CurrentScene == Recognize.Scene.FIGHTING) {
				Debug.Log("处于出战界面，不执行操作");
				continue;
			}
			if (Recognize.IsWindowCovered) {
				Debug.Log("有窗口覆盖，不执行操作");
				continue;
			}

			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(AllianceMechaDonate);
			
			bool succeed = false;
			Operation.Click(1870, 710);	// 联盟按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(830, 620);	// 联盟活动按钮
			yield return new EditorWaitForSeconds(0.2F);
			int index = Array.IndexOf(Recognize.AllianceActivityTypes, Recognize.AllianceActivityType.MECHA);
			if (index != -1) {
				Operation.Click(960, 300 + 269 * index);	// 联盟机甲
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(1170, 200);	// 排行奖励按钮
				yield return new EditorWaitForSeconds(0.5F);
				if (Recognize.IsAllianceMechaDonateRankShowing) {
					// 有弹出奖励排行窗口，说明已开启捐献
					bool isInRank = Recognize.IsAllianceMechaDonateInRank;
					Operation.Click(720, 128);	// 点击窗口外关闭窗口
					yield return new EditorWaitForSeconds(0.1F);
					if (!isInRank) {
						for (int i = 0; i < DONATE_COUNT; ++i) {
							Operation.Click(960, 960);	// 捐献按钮
							yield return new EditorWaitForSeconds(0.3F);
							if (Recognize.IsAllianceMechaDonateConfirming) {
								Operation.Click(960, 686);	// 兑换按钮
								yield return new EditorWaitForSeconds(0.2F);
								Operation.Click(1167, 353);	// 关闭按钮
								yield return new EditorWaitForSeconds(0.2F);
							}
						}
						succeed = true;
					}
				}
			}
			for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.1F);
			}
			
			Task.CurrentTask = null;

			if (succeed) {
				NEXT_TIME = DateTime.Now + new TimeSpan(COOL_DOWN, 0, 0);
			} else {
				NEXT_TIME = DateTime.Now + new TimeSpan(0, 0, INTERVAL);
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}

	[MenuItem("Tools_Task/OnceAllianceMechaDonate", priority = -1)]
	private static void ExecuteOnce() {
		Debug.Log($"单次执行联盟机甲捐献尝试");
		EditorCoroutineManager.StartCoroutine(IEExecuteOnce());
	}
	private static IEnumerator IEExecuteOnce() {
		if (Task.CurrentTask != null) {
			Debug.LogError($"正在执行【{Task.CurrentTask}】, 请稍后！");
			yield break;
		}
		DateTime prevNextTime = NEXT_TIME;
		NEXT_TIME = DateTime.Now;
		do {
			yield return null;
		} while (Task.CurrentTask != null);
		NEXT_TIME = prevNextTime;
	}
}
