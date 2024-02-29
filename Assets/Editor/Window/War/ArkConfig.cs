/*
 * @Author: wangyun
 * @CreateTime: 2023-10-31 07:02:42 199
 * @LastEditor: wangyun
 * @EditTime: 2023-10-31 07:02:42 204
 */

using System;
using UnityEngine;
using UnityEditor;

public class ArkConfig : PrefsEditorWindow<Ark> {
	[MenuItem("Tools_Window/War/Ark")]
	private static void Open() {
		GetWindow<ArkConfig>("参战方舟").Show();
	}

	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("每天触发时间", GUILayout.Width(EditorGUIUtility.labelWidth));
		EditorGUI.BeginChangeCheck();
		int startHours = EditorGUILayout.IntField(Ark.DAILY_TIME.Hours, GUILayout.MinWidth(20));
		EditorGUILayout.LabelField(":", GUILayout.Width(8));
		int startMinutes = EditorGUILayout.IntField(Ark.DAILY_TIME.Minutes, GUILayout.MinWidth(20));
		if (EditorGUI.EndChangeCheck()) {
			Ark.DAILY_TIME = new TimeSpan(startHours, startMinutes, 0);
		}
		EditorGUILayout.EndHorizontal();
		for (int i = 0, length = Ark.SQUAD_NUMBERS.Length; i < length; ++i) {
			Ark.SQUAD_NUMBERS[i] = EditorGUILayout.IntSlider($"{i + 1}号方舟使用编队", Ark.SQUAD_NUMBERS[i], 1, 8);
		}
		EditorGUILayout.BeginHorizontal();
		for (int i = 0, length = Ark.IsInArks.Length; i < length; ++i) {
			Ark.IsInArks[i] = GUILayout.Toggle(Ark.IsInArks[i], $"{i + 1}号方舟", "Button");
		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5F);
		if (Ark.IsRunning) {
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