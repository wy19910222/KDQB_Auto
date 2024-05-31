/*
 * @Author: wangyun
 * @CreateTime: 2024-05-11 15:09:39 704
 * @LastEditor: wangyun
 * @EditTime: 2024-05-11 15:09:39 708
 */

using System;
using UnityEngine;
using UnityEditor;

public class GlobalConfig : PrefsEditorWindow<Global> {
	[MenuItem("Tools_Window/Default/Global", false, -1)]
	private static void Open() {
		GetWindow<GlobalConfig>("全局配置").Show();
	}
	
	protected override bool IsRunning {
		get => m_IsRunning;
		set => m_IsRunning = value;
	}

	protected override void OnEnable() {
		string gameRectStr = Prefs.Get<string>($"Operation.CURRENT_GAME_RECT");
		if (!string.IsNullOrEmpty(gameRectStr)) {
			Operation.CURRENT_GAME_RECT = Utils.StringToRect(gameRectStr);
		}
		base.OnEnable();
	}
	protected override void OnDisable() {
		Prefs.Set($"Operation.CURRENT_GAME_RECT", Utils.RectToString(Operation.CURRENT_GAME_RECT));
		base.OnDisable();
	}

	private void OnGUI() {
		Operation.CURRENT_GAME_RECT = EditorGUILayout.RectIntField("游戏范围", Operation.CURRENT_GAME_RECT);
		Global.UNATTENDED_THRESHOLD = EditorGUILayout.LongField("无人值守阈值（秒）", Global.UNATTENDED_THRESHOLD / 1000_000_0) * 1000_000_0;
		Global.ENERGY_FULL = EditorGUILayout.IntField("体力上限", Global.ENERGY_FULL);
		
		Global.PERSISTENT_GROUP_COUNT = EditorGUILayout.IntSlider("永久行军队列", Global.PERSISTENT_GROUP_COUNT, 0, 7);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("临时行军队列");
		if (GUILayout.Button("+", GUILayout.Width(46F))) {
			Global.TEMPORARY_GROUP_COUNTDOWN.Add(DateTime.Now + new TimeSpan(1, 0, 0, 0));
		}
		EditorGUILayout.EndHorizontal();
		for (int i = 0, length = Global.TEMPORARY_GROUP_COUNTDOWN.Count; i < length; ++i) {
			DateTime time = Global.TEMPORARY_GROUP_COUNTDOWN[i];
			TimeSpan ts = time - DateTime.Now;
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			float prevLFieldWidth = EditorGUIUtility.fieldWidth;
			EditorGUIUtility.fieldWidth = 20F;
			int days = EditorGUILayout.IntField("        队列倒计时", ts.Days);
			float prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 14F;
			int hours = EditorGUILayout.IntField("天", ts.Hours);
			EditorGUIUtility.labelWidth = 8F;
			int minutes = EditorGUILayout.IntField(":", ts.Minutes);
			int seconds = EditorGUILayout.IntField(":", ts.Seconds);
			EditorGUIUtility.labelWidth = prevLabelWidth;
			EditorGUIUtility.fieldWidth = prevLFieldWidth;
			if (EditorGUI.EndChangeCheck()) {
				Global.TEMPORARY_GROUP_COUNTDOWN[i] = DateTime.Now + new TimeSpan(days, hours, minutes, seconds);
			}
			if (GUILayout.Button("×")) {
				int removeIndex = i;
				EditorApplication.delayCall += () => Global.TEMPORARY_GROUP_COUNTDOWN.RemoveAt(removeIndex);
			}
			EditorGUILayout.EndHorizontal();
		}
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("编队");
		if (GUILayout.Button("-", GUILayout.Width(46F))) {
			if (Global.SQUAD_LIST.Count > 0) {
				Global.SQUAD_LIST.RemoveAt(Global.SQUAD_LIST.Count - 1);
			}
		}
		if (GUILayout.Button("+", GUILayout.Width(46F))) {
			Global.SQUAD_LIST.Add(new Squad());
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(28F);
		EditorGUILayout.BeginVertical();
		for (int i = 0, length = Global.SQUAD_LIST.Count; i < length; ++i) {
			Squad squad = Global.SQUAD_LIST[i];
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"编队{i + 1}");
			bool newValid = GUILayout.Toggle(squad.valid, squad.valid ? "已生效" : "未配置", "Button");
			if (newValid != squad.valid) {
				squad.valid = newValid;
				if (!newValid) {
					squad.leader = Recognize.HeroType.DAN;
				}
			}
			EditorGUILayout.EndHorizontal();

			if (newValid) {
				EditorGUILayout.BeginHorizontal();
				foreach (Recognize.HeroType type in Enum.GetValues(typeof(Recognize.HeroType))) {
					bool isSelected = type == squad.leader;
					bool newIsSelected = GUILayout.Toggle(isSelected, Utils.GetEnumInspectorName(type), "Button");
					if (newIsSelected && !isSelected) {
						squad.leader = type;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			
			if (EditorGUI.EndChangeCheck()) {
				Global.SQUAD_LIST[i] = squad;
			}
		}
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
	}
	
	private void Update() {
		Repaint();
	}
}