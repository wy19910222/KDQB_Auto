/*
 * @Author: wangyun
 * @CreateTime: 2023-12-30 00:36:09 952
 * @LastEditor: wangyun
 * @EditTime: 2023-12-30 00:36:09 956
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class AllianceHelpConfig : PrefsEditorWindow<AllianceHelp> {
	[MenuItem("Tools_Window/Default/AllianceHelp")]
	private static void Open() {
		GetWindow<AllianceHelpConfig>("联盟帮助").Show();
	}
	
	private void OnGUI() {
		AllianceMechaDonate.INTERVAL = EditorGUILayout.IntSlider("尝试帮助间隔（秒）", AllianceMechaDonate.INTERVAL, 1800, 7200);
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
	public static int INTERVAL = 3600;	// 尝试间隔
	
	public static DateTime s_StartTime;	// 起头时间
	
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
		DateTime nextTime = DateTime.Now;
		while (true) {
			yield return null;
			
			DateTime date = DateTime.Now.Date;
			bool started = s_StartTime != null && s_StartTime > date;
			bool nextTimeNotReach = DateTime.Now < nextTime;
			if ((started || nextTimeNotReach) && !Recognize.IsAllianceHelpAwardOuter) {
				continue;
			}
			if (!nextTimeNotReach) {
				nextTime = DateTime.Now + new TimeSpan(INTERVAL, 0, 0);
			}
			
			Debug.Log("联盟按钮");
			Operation.Click(1870, 710);	// 联盟按钮
			yield return new EditorWaitForSeconds(0.2F);
			Debug.Log("联盟帮助按钮");
			Operation.Click(1080, 620);	// 联盟帮助按钮
			yield return new EditorWaitForSeconds(0.2F);
			Debug.Log($"IsAllianceHelpAwardIntuitive: {Recognize.IsAllianceHelpAwardIntuitive}");
			if (Recognize.IsAllianceHelpAwardIntuitive) {
				Debug.Log("领取奖励按钮");
				Operation.Click(1115, 895);	// 领取奖励按钮
				yield return new EditorWaitForSeconds(1F);
				Debug.Log("点空白处关闭恭喜获得界面");
				Operation.Click(1115, 895);	// 点空白处关闭恭喜获得界面
				yield return new EditorWaitForSeconds(0.3F);
			}
			Debug.Log($"CanAllianceHelpRequest: {Recognize.CanAllianceHelpRequest}");
			if (Recognize.CanAllianceHelpRequest) {
				Debug.Log("请求帮助按钮");
				Operation.Click(1115, 895);	// 请求帮助按钮
				yield return new EditorWaitForSeconds(0.5F);
				int index = Random.Range(0, 4);
				Debug.Log("选择帮助物品");
				Operation.Click(795 + 109 * index, 410);	// 选择帮助物品
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("请求帮助按钮");
				Operation.Click(960, 750);	// 请求帮助按钮
				yield return new EditorWaitForSeconds(0.5F);
				if (!started) {
					s_StartTime = DateTime.Now;
				}
			}
			for (int i = 0; i < 20; ++i) {
				if (Recognize.CanAllianceHelpOthers) {
					Debug.Log("帮助全部按钮");
					Operation.Click(960, 780);	// 帮助全部按钮
				}
				yield return new EditorWaitForSeconds(0.2F);
			}
			
			for (int i = 0; i < 10 && Recognize.IsWindowCovered1; i++) {
				Debug.Log("左上角返回按钮");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			
			yield return new EditorWaitForSeconds(5F);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
