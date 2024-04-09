/*
 * @Author: wangyun
 * @CreateTime: 2024-04-09 17:05:53 507
 * @LastEditor: wangyun
 * @EditTime: 2024-04-09 17:05:53 512
 */

using System;
using UnityEditor;
using UnityEngine;

public class ExpeditionConfig : PrefsEditorWindow<Expedition> {
	[MenuItem("Tools_Window/Default/Expedition")]
	private static void Open() {
		GetWindow<ExpeditionConfig>("远征/荒野行动").Show();
	}
	
	private void OnGUI() {
		Expedition.WILD_ORDER = EditorGUILayout.IntSlider("荒野排序", Expedition.WILD_ORDER, 0, 2);
		Expedition.Expedition_ORDER = EditorGUILayout.IntSlider("远征排序", Expedition.Expedition_ORDER, 4, 6);
		Expedition.INTERVAL_HOURS = EditorGUILayout.IntSlider("收取间隔（小时）", Expedition.INTERVAL_HOURS, 1, 12);
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		TimeSpan ts = Expedition.TargetDT - DateTime.Now;
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 20F;
		int hours = EditorGUILayout.IntField(" ", ts.Hours);
		EditorGUIUtility.labelWidth = 8F;
		int minutes = EditorGUILayout.IntField(":", ts.Minutes);
		int seconds = EditorGUILayout.IntField(":", ts.Seconds);
		EditorGUIUtility.labelWidth = prevLabelWidth;
		if (EditorGUI.EndChangeCheck()) {
			Expedition.TargetDT = DateTime.Now + new TimeSpan(hours, minutes, seconds);
		}
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space(5F);
		
		EditorGUILayout.BeginHorizontal();
		if (Expedition.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
		Expedition.Test = GUILayout.Toggle(Expedition.Test, "测试", "Button", GUILayout.Width(60F));
		EditorGUILayout.EndHorizontal();
	}
	
	private void Update() {
		Repaint();
	}
}