/*
 * @Author: wangyun
 * @CreateTime: 2023-10-31 07:02:42 199
 * @LastEditor: wangyun
 * @EditTime: 2023-10-31 07:02:42 204
 */

using System;
using UnityEngine;
using UnityEditor;

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