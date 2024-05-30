/*
 * @Author: wangyun
 * @CreateTime: 2024-04-09 17:05:53 507
 * @LastEditor: wangyun
 * @EditTime: 2024-04-09 17:05:53 512
 */

using System;
using UnityEditor;
using UnityEngine;

public class ExpeditionAndWildConfig : PrefsEditorWindow<ExpeditionAndWild> {
	[MenuItem("Tools_Window/Default/ExpeditionAndWild")]
	private static void Open() {
		GetWindow<ExpeditionAndWildConfig>("远征/荒野行动").Show();
	}
	
	private void OnGUI() {
		ExpeditionAndWild.WILD_ORDER = EditorGUILayout.IntSlider("荒野排序", ExpeditionAndWild.WILD_ORDER, 1, 3);
		ExpeditionAndWild.Expedition_ORDER = EditorGUILayout.IntSlider("远征排序", ExpeditionAndWild.Expedition_ORDER, 4, 6);
		ExpeditionAndWild.INTERVAL_HOURS = EditorGUILayout.IntSlider("收取间隔（小时）", ExpeditionAndWild.INTERVAL_HOURS, 1, 12);
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		TimeSpan ts = ExpeditionAndWild.TargetDT - DateTime.Now;
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 20F;
		int hours = EditorGUILayout.IntField(" ", ts.Hours);
		EditorGUIUtility.labelWidth = 8F;
		int minutes = EditorGUILayout.IntField(":", ts.Minutes);
		int seconds = EditorGUILayout.IntField(":", ts.Seconds);
		EditorGUIUtility.labelWidth = prevLabelWidth;
		if (EditorGUI.EndChangeCheck()) {
			ExpeditionAndWild.TargetDT = DateTime.Now + new TimeSpan(hours, minutes, seconds);
		}
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space(5F);
		
		EditorGUILayout.BeginHorizontal();
		ExpeditionAndWild.Test = GUILayout.Toggle(ExpeditionAndWild.Test, "测试模式", "Button", GUILayout.Width(60F));
		if (ExpeditionAndWild.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
		EditorGUILayout.EndHorizontal();
	}
	
	private void Update() {
		Repaint();
	}
}