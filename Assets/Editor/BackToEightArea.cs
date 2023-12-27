/*
 * @Author: wangyun
 * @CreateTime: 2023-10-31 07:02:42 199
 * @LastEditor: wangyun
 * @EditTime: 2023-10-31 07:02:42 204
 */

using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

using Debug = UnityEngine.Debug;

public class BackToEightAreaConfig : PrefsEditorWindow<BackToEightArea> {
	[MenuItem("Tools_Window/War/BackToEightArea")]
	private static void Open() {
		GetWindow<BackToEightAreaConfig>("返回八国").Show();
	}

	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("每天时段", GUILayout.Width(EditorGUIUtility.labelWidth));
		EditorGUI.BeginChangeCheck();
		int startHours = EditorGUILayout.IntField(BackToEightArea.DAILY_TIME_START.Hours, GUILayout.MinWidth(20));
		EditorGUILayout.LabelField(":", GUILayout.Width(8));
		int startMinutes = EditorGUILayout.IntField(BackToEightArea.DAILY_TIME_START.Minutes, GUILayout.MinWidth(20));
		if (EditorGUI.EndChangeCheck()) {
			BackToEightArea.DAILY_TIME_START = new TimeSpan(startHours, startMinutes, 0);
		}
		EditorGUILayout.LabelField("——", "CenteredLabel", GUILayout.Width(28));
		EditorGUI.BeginChangeCheck();
		int stopHours = EditorGUILayout.IntField(BackToEightArea.DAILY_TIME_STOP.Hours, GUILayout.MinWidth(20));
		EditorGUILayout.LabelField(":", GUILayout.Width(8));
		int stopMinutes = EditorGUILayout.IntField(BackToEightArea.DAILY_TIME_STOP.Minutes, GUILayout.MinWidth(20));
		if (EditorGUI.EndChangeCheck()) {
			BackToEightArea.DAILY_TIME_STOP = new TimeSpan(stopHours, stopMinutes, 0);
		}
		EditorGUILayout.EndHorizontal();
		
		BackToEightArea.INTERVAL = EditorGUILayout.IntSlider("尝试间隔", BackToEightArea.INTERVAL, 60, 120);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField(BackToEightArea.SUCCESS_TIME > DateTime.Now.Date ? "今日已完成" : "今日待完成", GUILayout.Width(EditorGUIUtility.labelWidth));
		if (GUILayout.Button("清除")) {
			BackToEightArea.SUCCESS_TIME = default;
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5F);
		if (BackToEightArea.IsRunning) {
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

public class BackToEightArea {
	public static TimeSpan DAILY_TIME_START = new TimeSpan(4, 5, 0);
	public static TimeSpan DAILY_TIME_STOP = new TimeSpan(4, 15, 0);
	public static int INTERVAL = 60;
	public static DateTime SUCCESS_TIME;
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartBackToEightArea", priority = -1)]
	private static void Enable() {
		Disable();
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopBackToEightArea", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
		}
	}

	private static IEnumerator Update() {
		// bool prevIsMarshalTime = false;
		while (true) {
			yield return null;

			DateTime now = DateTime.Now;
			if (SUCCESS_TIME > now.Date) {
				continue;
			}
			
			TimeSpan timeOfDay = now.TimeOfDay;
			if (timeOfDay < DAILY_TIME_START || timeOfDay > DAILY_TIME_STOP) {
				continue;
			}
			
			Debug.Log("八国活动按钮");
			switch (Recognize.CurrentScene) {
				case Recognize.Scene.INSIDE:
					Operation.Click(1875, 356);	// 八国活动按钮
					break;
				case Recognize.Scene.OUTSIDE:
					if (Recognize.IsOutsideNearby) {
						Operation.Click(1875, 365);	// 八国活动按钮
					} else if (Recognize.IsOutsideFaraway) {
						Operation.Click(1875, 216);	// 八国活动按钮
					} else {
						continue;
					}
					break;
				default:
					continue;
			}
			yield return new EditorWaitForSeconds(0.3F);
			if (Recognize.CanGotoEightArea) {
				Debug.Log("前往按钮");
				Operation.Click(1165, 930);	// 前往按钮
				yield return new EditorWaitForSeconds(0.3F);
				Debug.Log("确定按钮");
				Operation.Click(1060, 700);	// 确定按钮
				yield return new EditorWaitForSeconds(10);
				Debug.Log("左上角返回按钮");
				for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.2F);
				}
				if (Recognize.CurrentScene == Recognize.Scene.INSIDE) {
					Debug.Log("右下角主城与世界切换按钮");
					Operation.Click(1170, 970);	// 右下角主城与世界切换按钮
				}
				SUCCESS_TIME = now;
			} else {
				Debug.Log("左上角返回按钮");
				for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.2F);
				}
				SUCCESS_TIME = now;
			}
			yield return new EditorWaitForSeconds(INTERVAL);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
