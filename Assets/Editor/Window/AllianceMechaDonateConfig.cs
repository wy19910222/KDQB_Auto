/*
 * @Author: wangyun
 * @CreateTime: 2023-10-24 04:22:33 341
 * @LastEditor: wangyun
 * @EditTime: 2023-10-24 04:22:33 346
 */

using System;
using UnityEngine;
using UnityEditor;

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
		AllianceMechaDonate.DONATE_COUNT = EditorGUILayout.IntSlider("捐献数量", AllianceMechaDonate.DONATE_COUNT, 1, 10);
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