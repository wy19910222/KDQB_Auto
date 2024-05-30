/*
 * @Author: wangyun
 * @CreateTime: 2024-05-30 19:12:41 178
 * @LastEditor: wangyun
 * @EditTime: 2024-05-30 19:12:41 185
 */

using System;
using UnityEditor;
using UnityEngine;

public class IslandAndSandTableConfig : PrefsEditorWindow<IslandAndSandTable> {
	[MenuItem("Tools_Window/Default/IslandAndSandTable")]
	private static void Open() {
		GetWindow<IslandAndSandTableConfig>("岛屿作战/沙盘演习").Show();
	}
	
	private void OnGUI() {
		DateTime now = DateTime.Now;
		DateTime date = now.Date;
		EditorGUILayout.BeginHorizontal();
		IslandAndSandTable.SAND_TABLE_ORDER = EditorGUILayout.IntSlider("沙盘排序", IslandAndSandTable.SAND_TABLE_ORDER, 3, 5);
		bool sandTableSucceed = IslandAndSandTable.LAST_SAND_TABLE_TIME > date;
		bool newSandTableSucceed = GUILayout.Toggle(sandTableSucceed, "已完成", "Button", GUILayout.Width(60F));
		if (newSandTableSucceed != sandTableSucceed) {
			IslandAndSandTable.LAST_SAND_TABLE_TIME = newSandTableSucceed ? now : now - new TimeSpan(24, 0, 0);
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		IslandAndSandTable.ISLAND_ORDER = EditorGUILayout.IntSlider("岛屿排序", IslandAndSandTable.ISLAND_ORDER, 9, 11);
		bool islandSucceed = IslandAndSandTable.LAST_ISLAND_TIME > date;
		bool newIslandSucceed = GUILayout.Toggle(islandSucceed, "已完成", "Button", GUILayout.Width(60F));
		if (newIslandSucceed != islandSucceed) {
			IslandAndSandTable.LAST_ISLAND_TIME = newIslandSucceed ? now : now - new TimeSpan(24, 0, 0);
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		TimeSpan ts = IslandAndSandTable.DAILY_TIME;
		int hours = EditorGUILayout.IntField("执行时间", ts.Hours);
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 8F;
		int minutes = EditorGUILayout.IntField(":", ts.Minutes);
		int seconds = EditorGUILayout.IntField(":", ts.Seconds);
		EditorGUIUtility.labelWidth = prevLabelWidth;
		if (EditorGUI.EndChangeCheck()) {
			IslandAndSandTable.DAILY_TIME = new TimeSpan(hours, minutes, seconds);
		}
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space(5F);
		
		EditorGUILayout.BeginHorizontal();
		IslandAndSandTable.Test = GUILayout.Toggle(IslandAndSandTable.Test, "测试模式", "Button", GUILayout.Width(60F));
		if (IslandAndSandTable.IsRunning) {
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