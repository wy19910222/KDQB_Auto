/*
 * @Author: wangyun
 * @CreateTime: 2023-10-24 04:22:33 341
 * @LastEditor: wangyun
 * @EditTime: 2023-10-24 04:22:33 346
 */

using System;
using UnityEngine;
using UnityEditor;

public class AllianceMechaOpenConfig : PrefsEditorWindow<AllianceMechaOpen> {
	[MenuItem("Tools_Window/Default/AllianceMechaOpen", false, 21)]
	private static void Open() {
		GetWindow<AllianceMechaOpenConfig>("联盟机甲开启").Show();
	}
	
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("每天时间", GUILayout.Width(EditorGUIUtility.labelWidth));
		EditorGUI.BeginChangeCheck();
		int startHours = EditorGUILayout.IntField(AllianceMechaOpen.DAILY_TIME.Hours);
		EditorGUILayout.LabelField(":", GUILayout.Width(8));
		int startMinutes = EditorGUILayout.IntField(AllianceMechaOpen.DAILY_TIME.Minutes);
		if (EditorGUI.EndChangeCheck()) {
			AllianceMechaOpen.DAILY_TIME = new TimeSpan(startHours, startMinutes, 0);
		}
		EditorGUILayout.EndHorizontal();
		
		AllianceMechaOpen.MECHA_INDEX = EditorGUILayout.IntSlider("机甲序号", AllianceMechaOpen.MECHA_INDEX, 0, 3);
		AllianceMechaOpen.MECHA_LEVEL = EditorGUILayout.IntSlider("机甲等级", AllianceMechaOpen.MECHA_LEVEL, 1, 6);
		AllianceMechaOpen.DONATE_COUNT = EditorGUILayout.IntSlider("捐献数量", AllianceMechaOpen.DONATE_COUNT, 1, 10);
		
		GUILayout.Space(5F);
		if (AllianceMechaOpen.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
	}
	
	private void Update() {
		Repaint();
	}
}