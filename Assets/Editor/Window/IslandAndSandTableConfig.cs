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

	private readonly string[] SAND_TABLE_TAB_NAMES = {"陆军", "海军", "空军"};
	
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
		EditorGUILayout.LabelField("挑战军种", GUILayout.Width(EditorGUIUtility.labelWidth - 2F));
		for (int i = 0, length = SAND_TABLE_TAB_NAMES.Length; i < length; i++) {
			bool isSelected = i + 1 == IslandAndSandTable.SAND_TABLE_TAB;
			bool newIsSelected = GUILayout.Toggle(isSelected, SAND_TABLE_TAB_NAMES[i], "Button");
			if (newIsSelected && !isSelected) {
				IslandAndSandTable.SAND_TABLE_TAB = i + 1;
			}
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		IslandAndSandTable.SAND_SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", IslandAndSandTable.SAND_SQUAD_NUMBER, 1, 8);
		IslandAndSandTable.SAND_MUST_FULL_SOLDIER = GUILayout.Toggle(IslandAndSandTable.SAND_MUST_FULL_SOLDIER, "必须满兵", "Button", GUILayout.Width(60F));
		EditorGUILayout.EndHorizontal();

		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}
		
		EditorGUILayout.BeginHorizontal();
		IslandAndSandTable.EXPEDITION_ORDER = EditorGUILayout.IntSlider("远征排序", IslandAndSandTable.EXPEDITION_ORDER, 4, 6);
		bool expeditionSucceed = IslandAndSandTable.LAST_EXPEDITION_TIME > date;
		bool newExpeditionSucceed = GUILayout.Toggle(expeditionSucceed, "已完成", "Button", GUILayout.Width(60F));
		if (newExpeditionSucceed != expeditionSucceed) {
			IslandAndSandTable.LAST_EXPEDITION_TIME = newExpeditionSucceed ? now : now - new TimeSpan(24, 0, 0);
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("花费钻石", GUILayout.Width(EditorGUIUtility.labelWidth - 2F));
		IslandAndSandTable.EXPEDITION_QUICK_BY_50_DIAMOND = GUILayout.Toggle(IslandAndSandTable.EXPEDITION_QUICK_BY_50_DIAMOND, "花费50钻", "Button");
		EditorGUILayout.EndHorizontal();

		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}
		
		EditorGUILayout.BeginHorizontal();
		IslandAndSandTable.TRANSNATIONAL_ORDER = EditorGUILayout.IntSlider("跨战区排序", IslandAndSandTable.TRANSNATIONAL_ORDER, 5, 7);
		bool transnationalSucceed = IslandAndSandTable.LAST_TRANSNATIONAL_TIME > date;
		bool newTransnationalSucceed = GUILayout.Toggle(transnationalSucceed, "已完成", "Button", GUILayout.Width(60F));
		if (newTransnationalSucceed != transnationalSucceed) {
			IslandAndSandTable.LAST_TRANSNATIONAL_TIME = newTransnationalSucceed ? now : now - new TimeSpan(24, 0, 0);
		}
		EditorGUILayout.EndHorizontal();
		for (int i = 0, length = IslandAndSandTable.TRANSNATIONAL_TARGET_WEIGHTS.Count; i < length; ++i) {
			int weight = IslandAndSandTable.TRANSNATIONAL_TARGET_WEIGHTS[i];
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			int newWeight = Math.Max(EditorGUILayout.IntField($"    目标{i + 1}权重", Math.Abs(weight)), 0);
			if (EditorGUI.EndChangeCheck()) {
				weight = weight < 0 ? -newWeight : newWeight;
				IslandAndSandTable.TRANSNATIONAL_TARGET_WEIGHTS[i] = weight;
			}
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Toggle(weight > 0, GUILayout.Width(16F));
			if (EditorGUI.EndChangeCheck()) {
				weight = -weight;
				IslandAndSandTable.TRANSNATIONAL_TARGET_WEIGHTS[i] = weight;
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.BeginHorizontal();
		IslandAndSandTable.TRANSNATIONAL_SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", IslandAndSandTable.TRANSNATIONAL_SQUAD_NUMBER, 1, 8);
		IslandAndSandTable.TRANSNATIONAL_MUST_FULL_SOLDIER = GUILayout.Toggle(IslandAndSandTable.TRANSNATIONAL_MUST_FULL_SOLDIER, "必须满兵", "Button", GUILayout.Width(60F));
		EditorGUILayout.EndHorizontal();

		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}
		
		EditorGUILayout.BeginHorizontal();
		IslandAndSandTable.ISLAND_ORDER = EditorGUILayout.IntSlider("岛屿排序", IslandAndSandTable.ISLAND_ORDER, 9, 11);
		bool islandSucceed = IslandAndSandTable.LAST_ISLAND_TIME > date;
		bool newIslandSucceed = GUILayout.Toggle(islandSucceed, "已完成", "Button", GUILayout.Width(60F));
		if (newIslandSucceed != islandSucceed) {
			IslandAndSandTable.LAST_ISLAND_TIME = newIslandSucceed ? now : now - new TimeSpan(24, 0, 0);
		}
		EditorGUILayout.EndHorizontal();

		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}
		
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