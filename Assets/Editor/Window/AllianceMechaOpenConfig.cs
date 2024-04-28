/*
 * @Author: wangyun
 * @CreateTime: 2024-03-30 18:08:03 162
 * @LastEditor: wangyun
 * @EditTime: 2024-03-30 18:08:03 166
 */

using System;
using UnityEngine;
using UnityEditor;

public class AllianceMechaOpenConfig : PrefsEditorWindow<AllianceMechaOpen> {
	[MenuItem("Tools_Window/Default/AllianceMechaOpen", false, 21)]
	private static void Open() {
		Prefs.Set($"{nameof(AllianceMechaOpen)}Window.IsRunning", false);
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
		
		AllianceMechaOpen.TRY_COUNT = EditorGUILayout.IntSlider("重试次数", AllianceMechaOpen.TRY_COUNT, 1, 3);
		AllianceMechaOpen.TRY_INTERVAL = EditorGUILayout.IntField("重试间隔（秒）", AllianceMechaOpen.TRY_INTERVAL);
		
		AllianceMechaOpen.MECHA_TYPE = (Recognize.AllianceMechaType) EditorGUILayout.EnumPopup("机甲类型", AllianceMechaOpen.MECHA_TYPE);
		AllianceMechaOpen.MECHA_LEVEL = EditorGUILayout.IntSlider("机甲等级", AllianceMechaOpen.MECHA_LEVEL, 1, 6);
		AllianceMechaOpen.DONATE_COUNT = EditorGUILayout.IntSlider("捐献数量", AllianceMechaOpen.DONATE_COUNT, 1, 10);
		
		EditorGUILayout.BeginHorizontal();
		bool started = AllianceMechaOpen.s_OpenTime > (DateTime.Now - AllianceMechaOpen.DAILY_TIME).Date;
		bool newStarted = GUILayout.Toggle(started, "今日已尝试", "Button");
		if (newStarted != started) {
			bool valid = true;
			if (!AllianceMechaOpen.Test && DateTime.Now.TimeOfDay > AllianceMechaOpen.DAILY_TIME) {
				if (!EditorUtility.DisplayDialog("提示", "当前非测试模式，更改状态后会立即执行，是否继续？", "继续", "取消")) {
					valid = false;
				}
			}
			if (valid) {
				AllianceMechaOpen.s_OpenTime = newStarted ? DateTime.Now.Date : default;
			}
		}
		AllianceMechaOpen.Test = GUILayout.Toggle(AllianceMechaOpen.Test, "测试模式", "Button");
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space(5F);
		EditorGUILayout.BeginHorizontal();
		if (AllianceMechaOpen.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
		EditorGUI.BeginDisabledGroup(!AllianceMechaOpen.Test);
		if (GUILayout.Button("单次执行", GUILayout.Width(60F))) {
			EditorApplication.ExecuteMenuItem("Tools_Task/AllianceMechaOpenOnce");
		}
		EditorGUI.EndDisabledGroup();
		EditorGUILayout.EndHorizontal();
	}
	
	private void Update() {
		Repaint();
	}
}