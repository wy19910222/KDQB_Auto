/*
 * @Author: wangyun
 * @CreateTime: 2023-10-12 00:00:51 751
 * @LastEditor: wangyun
 * @EditTime: 2023-10-12 00:00:51 756
 */

using System;
using System.Collections.Generic;
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
			float prevFieldWidth = EditorGUIUtility.fieldWidth;
			EditorGUIUtility.fieldWidth = 40;
			Gather.RESERVED_ENERGY = EditorGUILayout.IntField("保留体力值", Gather.RESERVED_ENERGY);
			EditorGUIUtility.fieldWidth = prevFieldWidth;
			// Gather.KEEP_ENERGY_NOT_FULL = GUILayout.Toggle(Gather.KEEP_ENERGY_NOT_FULL, "保持不满", "Button");
			List<int> reservedEnergy = new List<int>() { 0, 25 };
			int onceCost = Global.DAN_EXIST ? 8 : 10;
			int fearStarCost = Global.DAN_EXIST ? 80 : 100;
			if (onceCost + fearStarCost < Global.ENERGY_FULL) {
				reservedEnergy.Add(fearStarCost);
			}
			reservedEnergy.Add(Global.ENERGY_FULL - 50 - 2);
			reservedEnergy.Add(Global.ENERGY_FULL - onceCost - 2);
			foreach (int energy in reservedEnergy) {
				if (GUILayout.Button(energy.ToString())) {
					Gather.RESERVED_ENERGY = energy;
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("攻击目标");
		if (GUILayout.Button("-")) {
			Gather.TARGET_LIST.RemoveAt(Gather.TARGET_LIST.Count - 1);
		}
		if (GUILayout.Button("+")) {
			Gather.TARGET_LIST.Add(new GatherTarget());
		}
		EditorGUILayout.EndHorizontal();
		for (int i = 0, length = Gather.TARGET_LIST.Count; i < length; ++i) {
			GatherTarget target = Gather.TARGET_LIST[i];
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			int newCount = Math.Max(EditorGUILayout.IntField($"    目标{i + 1}次数", Math.Abs(target.count)), 0);
			if (EditorGUI.EndChangeCheck()) {
				target.count = target.count < 0 ? -newCount : newCount;
				Gather.TARGET_LIST[i] = target;
			}
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Toggle(target.count > 0, GUILayout.Width(16F));
			if (EditorGUI.EndChangeCheck()) {
				target.count = -target.count;
				Gather.TARGET_LIST[i] = target;
			}
			target.willReset = GUILayout.Toggle(target.willReset, "每日重置", "Button", GUILayout.Width(64F));
			EditorGUILayout.EndHorizontal();

			if (target.count > 0) {
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
				Gather.TARGET_LIST[i] = target;
			}
		}
		
		// Rect rect2 = GUILayoutUtility.GetRect(0, 10);
		// Rect wireRect2 = new Rect(rect2.x, rect2.y + 4.5F, rect2.width, 1);
		// EditorGUI.DrawRect(wireRect2, Color.gray);
		//
		// Gather.MUST_FULL_SOLDIER = GUILayout.Toggle(Gather.MUST_FULL_SOLDIER, "必须满兵", "Button", GUILayout.Width(60F));
		
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
		Gather.Test = GUILayout.Toggle(Gather.Test, "测试模式", "Button", GUILayout.Width(60F));
		if (Gather.IsRunning) {
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
}