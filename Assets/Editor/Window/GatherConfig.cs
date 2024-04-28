/*
 * @Author: wangyun
 * @CreateTime: 2023-10-12 00:00:51 751
 * @LastEditor: wangyun
 * @EditTime: 2023-10-12 00:00:51 756
 */

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GatherConfig : PrefsEditorWindow<Gather> {
	[MenuItem("Tools_Window/Default/Gather", false, 2)]
	private static void Open() {
		GetWindow<GatherConfig>("集结").Show();
	}
	
	private void OnGUI() {
		Gather.UNATTENDED_DURATION = EditorGUILayout.Slider("等待无操作（秒）", Gather.UNATTENDED_DURATION, 0, 20);
		bool useBottle = Gather.USE_BOTTLE_DICT.Values.ToList().Exists(count => count > 0);
		if (!useBottle) {
			EditorGUILayout.BeginHorizontal();
			Gather.RESERVED_ENERGY = EditorGUILayout.IntField("保留体力值", Gather.RESERVED_ENERGY);
			// Gather.KEEP_ENERGY_NOT_FULL = GUILayout.Toggle(Gather.KEEP_ENERGY_NOT_FULL, "保持不满", "Button");
			Gather.DAN_EXIST = GUILayout.Toggle(Gather.DAN_EXIST, "有戴安娜", "Button");
			EditorGUILayout.EndHorizontal();
		}

		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("攻击目标");
		if (GUILayout.Button("-")) {
			Gather.TARGET_ATTACK_COUNT_LIST.RemoveAt(Gather.TARGET_ATTACK_COUNT_LIST.Count - 1);
		}
		if (GUILayout.Button("+")) {
			Gather.TARGET_ATTACK_COUNT_LIST.Add(0);
		}
		EditorGUILayout.EndHorizontal();
		for (int i = Gather.TYPE_WILL_RESET_LIST.Count; i < Gather.TARGET_ATTACK_COUNT_LIST.Count; i++) {
			Gather.TYPE_WILL_RESET_LIST.Add(false);
		}
		for (int i = 0, length = Gather.TARGET_ATTACK_COUNT_LIST.Count; i < length; ++i) {
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			int count = Gather.TARGET_ATTACK_COUNT_LIST[i];
			int newCount = Math.Max(EditorGUILayout.IntField($"    目标{i + 1}", Math.Abs(count)), 0);
			if (EditorGUI.EndChangeCheck()) {
				count = count < 0 ? -newCount : newCount;
				Gather.TARGET_ATTACK_COUNT_LIST[i] = count;
			}
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Toggle(count > 0, GUILayout.Width(16F));
			if (EditorGUI.EndChangeCheck()) {
				count = -count;
				Gather.TARGET_ATTACK_COUNT_LIST[i] = count;
			}
			Gather.TYPE_WILL_RESET_LIST[i] = GUILayout.Toggle(Gather.TYPE_WILL_RESET_LIST[i], "每日重置", "Button", GUILayout.Width(64F));
			EditorGUILayout.EndHorizontal();
		}
		Gather.TARGET_LEVEL_OFFSET = EditorGUILayout.IntSlider("等级偏移（如果非惧星）", Gather.TARGET_LEVEL_OFFSET, -9, 0);
		Gather.FEAR_STAR_LEVEL = EditorGUILayout.IntSlider("等级（如果是惧星）", Gather.FEAR_STAR_LEVEL, 1, 5);
		
		Rect rect2 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect2 = new Rect(rect2.x, rect2.y + 4.5F, rect2.width, 1);
		EditorGUI.DrawRect(wireRect2, Color.gray);
		
		EditorGUILayout.BeginHorizontal();
		Gather.SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", Gather.SQUAD_NUMBER, 1, 8);
		Gather.MUST_FULL_SOLDIER = GUILayout.Toggle(Gather.MUST_FULL_SOLDIER, "必须满兵", "Button", GUILayout.Width(60F));
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		foreach (Recognize.HeroType type in Enum.GetValues(typeof(Recognize.HeroType))) {
			bool isSelected = type == Gather.HERO_AVATAR;
			bool newIsSelected = GUILayout.Toggle(isSelected, Utils.GetEnumInspectorName(type), "Button");
			if (newIsSelected && !isSelected) {
				Gather.HERO_AVATAR = type;
			}
		}
		EditorGUILayout.EndHorizontal();
		
		Rect rect3 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect3 = new Rect(rect3.x, rect3.y + 4.5F, rect3.width, 1);
		EditorGUI.DrawRect(wireRect3, Color.gray);

		foreach (Recognize.EnergyShortcutAddingType type in Enum.GetValues(typeof(Recognize.EnergyShortcutAddingType))) {
			if (type != Recognize.EnergyShortcutAddingType.NONE) {
				EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginChangeCheck();
				Gather.USE_BOTTLE_DICT.TryGetValue(type, out int count);
				int newCount = Math.Max(EditorGUILayout.IntField(Utils.GetEnumInspectorName(type), Math.Abs(count)), 0);
				if (EditorGUI.EndChangeCheck()) {
					count = count < 0 ? -newCount : newCount;
					Gather.USE_BOTTLE_DICT[type] = count;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Toggle(count > 0, GUILayout.Width(16F));
				if (EditorGUI.EndChangeCheck()) {
					count = -count;
					Gather.USE_BOTTLE_DICT[type] = count;
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		
		GUILayout.Space(5F);
		
		EditorGUILayout.BeginHorizontal();
		if (Gather.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
		Gather.Test = GUILayout.Toggle(Gather.Test, "测试", "Button", GUILayout.Width(60F));
		EditorGUILayout.EndHorizontal();
	}
}