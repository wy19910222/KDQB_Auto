/*
 * @Author: wangyun
 * @CreateTime: 2024-04-17 00:48:28 458
 * @LastEditor: wangyun
 * @EditTime: 2024-04-17 00:48:28 475
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RescueRefugeesConfig : PrefsEditorWindow<RescueRefugees> {
	[MenuItem("Tools_Window/Default/RescueRefugees", false, 2)]
	private static void Open() {
		GetWindow<RescueRefugeesConfig>("拯救难民").Show();
	}
	
	private void OnGUI() {
		RescueRefugees.UNATTENDED_DURATION = EditorGUILayout.Slider("等待无操作（秒）", RescueRefugees.UNATTENDED_DURATION, 0, 20);
		bool useBottle = RescueRefugees.USE_BOTTLE_DICT.Values.ToList().Exists(count => count > 0);
		if (!useBottle) {
			EditorGUILayout.BeginHorizontal();
			float prevFieldWidth = EditorGUIUtility.fieldWidth;
			EditorGUIUtility.fieldWidth = 40;
			RescueRefugees.RESERVED_ENERGY = EditorGUILayout.IntField("保留体力值", RescueRefugees.RESERVED_ENERGY);
			EditorGUIUtility.fieldWidth = prevFieldWidth;
			// RescueRefugees.KEEP_ENERGY_NOT_FULL = GUILayout.Toggle(RescueRefugees.KEEP_ENERGY_NOT_FULL, "保持不满", "Button");
			List<int> reservedEnergy = new List<int>() { 0, 25 };
			int onceCost = Global.DAN_EXIST ? 3 : 5;
			int fearStarCost = Global.DAN_EXIST ? 80 : 100;
			if (onceCost + fearStarCost < Global.ENERGY_FULL) {
				reservedEnergy.Add(fearStarCost);
			}
			reservedEnergy.Add(Global.ENERGY_FULL - 50 - 2);
			reservedEnergy.Add(Global.ENERGY_FULL - onceCost - 2);
			foreach (int energy in reservedEnergy) {
				if (GUILayout.Button(energy.ToString())) {
					RescueRefugees.RESERVED_ENERGY = energy;
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);
		
		
		RescueRefugees.ACTIVITY_ORDER = EditorGUILayout.IntSlider("活动排序（活动排在第几个）", RescueRefugees.ACTIVITY_ORDER, 1, 8);
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		int attackCount = RescueRefugees.ATTACK_COUNT;
		int newAttackCount = Math.Max(EditorGUILayout.IntField("攻击次数", Math.Abs(attackCount)), 0);
		if (EditorGUI.EndChangeCheck()) {
			attackCount = attackCount < 0 ? -newAttackCount : newAttackCount;
			RescueRefugees.ATTACK_COUNT = attackCount;
		}
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.Toggle(attackCount > 0, GUILayout.Width(16F));
		if (EditorGUI.EndChangeCheck()) {
			attackCount = -attackCount;
			RescueRefugees.ATTACK_COUNT = attackCount;
		}
		EditorGUILayout.EndHorizontal();
		
		Rect rect2 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect2 = new Rect(rect2.x, rect2.y + 4.5F, rect2.width, 1);
		EditorGUI.DrawRect(wireRect2, Color.gray);
		
		RescueRefugees.SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", RescueRefugees.SQUAD_NUMBER, 1, 8);
		EditorGUILayout.BeginHorizontal();
		foreach (Recognize.HeroType type in Enum.GetValues(typeof(Recognize.HeroType))) {
			GUILayout.Toggle(type == Global.GetLeader(RescueRefugees.SQUAD_NUMBER), Utils.GetEnumInspectorName(type), "Button");
		}
		EditorGUILayout.EndHorizontal();
		
		Rect rect3 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect3 = new Rect(rect3.x, rect3.y + 4.5F, rect3.width, 1);
		EditorGUI.DrawRect(wireRect3, Color.gray);

		foreach (Recognize.EnergyShortcutAddingType type in Enum.GetValues(typeof(Recognize.EnergyShortcutAddingType))) {
			if (type != Recognize.EnergyShortcutAddingType.NONE) {
				EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginChangeCheck();
				RescueRefugees.USE_BOTTLE_DICT.TryGetValue(type, out int count);
				int newCount = Math.Max(EditorGUILayout.IntField(Utils.GetEnumInspectorName(type), Math.Abs(count)), 0);
				if (EditorGUI.EndChangeCheck()) {
					count = count < 0 ? -newCount : newCount;
					RescueRefugees.USE_BOTTLE_DICT[type] = count;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Toggle(count > 0, GUILayout.Width(16F));
				if (EditorGUI.EndChangeCheck()) {
					count = -count;
					RescueRefugees.USE_BOTTLE_DICT[type] = count;
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		
		GUILayout.Space(5F);
		
		EditorGUILayout.BeginHorizontal();
		RescueRefugees.Test = GUILayout.Toggle(RescueRefugees.Test, "测试模式", "Button", GUILayout.Width(60F));
		if (RescueRefugees.IsRunning) {
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