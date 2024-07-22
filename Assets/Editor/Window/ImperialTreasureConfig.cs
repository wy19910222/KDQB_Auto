/*
 * @Author: wangyun
 * @CreateTime: 2024-07-22 23:52:59 745
 * @LastEditor: wangyun
 * @EditTime: 2024-07-22 23:52:59 752
 */

using System;
using UnityEditor;
using UnityEngine;

public class ImperialTreasureConfig : PrefsEditorWindow<ImperialTreasure> {
	[MenuItem("Tools_Window/Default/ImperialTreasure", false, 2)]
	private static void Open() {
		GetWindow<ImperialTreasureConfig>("帝国宝藏").Show();
	}
	
	private void OnGUI() {
		ImperialTreasure.UNATTENDED_DURATION = EditorGUILayout.Slider("等待无操作（秒）", ImperialTreasure.UNATTENDED_DURATION, 0, 20);
		ImperialTreasure.INTERVAL = EditorGUILayout.IntSlider("冷却（小时）", ImperialTreasure.INTERVAL, 10, 12);
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		TimeSpan tryCountdown = ImperialTreasure.NEXT_DT - DateTime.Now;
		float prevLFieldWidth = EditorGUIUtility.fieldWidth;
		EditorGUIUtility.fieldWidth = 20F;
		int hours = EditorGUILayout.DelayedIntField("冷却倒计时", (int) tryCountdown.TotalHours);
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 8F;
		int minutes = EditorGUILayout.DelayedIntField(":", tryCountdown.Minutes);
		int seconds = EditorGUILayout.DelayedIntField(":", tryCountdown.Seconds);
		EditorGUIUtility.labelWidth = prevLabelWidth;
		EditorGUIUtility.fieldWidth = prevLFieldWidth;
		if (EditorGUI.EndChangeCheck()) {
			ImperialTreasure.NEXT_DT = DateTime.Now + new TimeSpan(hours, minutes, seconds);
		}
		EditorGUILayout.EndHorizontal();

		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("攻击目标");
		if (GUILayout.Button("-")) {
			ImperialTreasure.TARGET_LIST.RemoveAt(ImperialTreasure.TARGET_LIST.Count - 1);
		}
		if (GUILayout.Button("+")) {
			ImperialTreasure.TARGET_LIST.Add(new ImperialTreasureTarget());
		}
		EditorGUILayout.EndHorizontal();
		for (int i = 0, length = ImperialTreasure.TARGET_LIST.Count; i < length; ++i) {
			ImperialTreasureTarget target = ImperialTreasure.TARGET_LIST[i];
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			int newCount = Math.Max(EditorGUILayout.IntField($"    目标{i + 1}权重", Math.Abs(target.weight)), 0);
			if (EditorGUI.EndChangeCheck()) {
				target.weight = target.weight < 0 ? -newCount : newCount;
				ImperialTreasure.TARGET_LIST[i] = target;
			}
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Toggle(target.weight > 0, GUILayout.Width(16F));
			if (EditorGUI.EndChangeCheck()) {
				target.weight = -target.weight;
				ImperialTreasure.TARGET_LIST[i] = target;
			}
			EditorGUILayout.EndHorizontal();

			if (target.weight > 0) {
				target.levelOffset = EditorGUILayout.IntSlider("        相对最高级偏移", target.levelOffset, -9, 0);
				EditorGUILayout.BeginHorizontal();
				target.squadNumber = EditorGUILayout.IntSlider("        使用编队号码", target.squadNumber, 1, 8);
				target.mustFullSoldiers = GUILayout.Toggle(target.mustFullSoldiers, "必须满兵", "Button", GUILayout.Width(64F));
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(28F);
				foreach (Recognize.HeroType type in Enum.GetValues(typeof(Recognize.HeroType))) {
					GUILayout.Toggle(type == Global.GetLeader(target.squadNumber), Utils.GetEnumInspectorName(type), "Button");
				}
				EditorGUILayout.EndHorizontal();
			}
			
			if (EditorGUI.EndChangeCheck()) {
				ImperialTreasure.TARGET_LIST[i] = target;
			}
		}

		GUILayout.Space(5F);
		EditorGUILayout.BeginHorizontal();
		ImperialTreasure.Test = GUILayout.Toggle(ImperialTreasure.Test, "测试模式", "Button", GUILayout.Width(60F));
		if (ImperialTreasure.IsRunning) {
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