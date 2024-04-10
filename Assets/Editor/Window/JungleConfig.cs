/*
 * @Author: wangyun
 * @CreateTime: 2023-09-07 20:18:29 730
 * @LastEditor: wangyun
 * @EditTime: 2023-09-07 20:18:29 842
 */

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class JungleConfig : PrefsEditorWindow<Jungle> {
	[MenuItem("Tools_Window/Default/Jungle", false, 1)]
	private static void Open() {
		GetWindow<JungleConfig>("打野").Show();
	}
	
	private void OnGUI() {
		if (m_Debug) {
			if (EditorGUIUtility.currentViewWidth > 400) {
				EditorGUILayout.BeginHorizontal();
			}
			if (GUILayout.Button("打印头像特征")) {
				Recognize.LogGroupHeroAvatar();
			}
			foreach (Recognize.HeroType type in Enum.GetValues(typeof(Recognize.HeroType))) {
				string heroName = Utils.GetEnumInspectorName(type);
				if (GUILayout.Button($"打印{heroName}位置")) {
					Debug.LogError($"{heroName}：{Recognize.GetHeroGroupNumber(type)}");
				}
			}
			if (EditorGUIUtility.currentViewWidth > 400) {
				EditorGUILayout.EndHorizontal();
			}
			Rect rect0 = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect0 = new Rect(rect0.x, rect0.y + 4.5F, rect0.width, 1);
			EditorGUI.DrawRect(wireRect0, Color.gray);
		}
		
		Jungle.UNATTENDED_DURATION = EditorGUILayout.Slider("等待无操作（秒）", Jungle.UNATTENDED_DURATION, 0, 20);
		Jungle.COOLDOWN = Mathf.Max(EditorGUILayout.FloatField("打野间隔", Jungle.COOLDOWN), 5);
		bool useBottle = Jungle.USE_BOTTLE_DICT.Values.ToList().Exists(count => count > 0);
		if (!useBottle) {
			EditorGUILayout.BeginHorizontal();
			Jungle.RESERVED_ENERGY = EditorGUILayout.IntField("保留体力值", Jungle.RESERVED_ENERGY);
			// Jungle.KEEP_ENERGY_NOT_FULL = GUILayout.Toggle(Jungle.KEEP_ENERGY_NOT_FULL, "保持不满", "Button");
			Jungle.DAN_EXIST = GUILayout.Toggle(Jungle.DAN_EXIST, "有戴安娜", "Button");
			EditorGUILayout.EndHorizontal();
		}
		
		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("攻击目标");
		if (GUILayout.Button("-")) {
			Jungle.TARGET_ATTACK_LIST.RemoveAt(Jungle.TARGET_ATTACK_LIST.Count - 1);
		}
		if (GUILayout.Button("+")) {
			Jungle.TARGET_ATTACK_LIST.Add(false);
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Space(5F);
		for (int i = 0, length = Jungle.TARGET_ATTACK_LIST.Count; i < length; ++i) {
			Jungle.TARGET_ATTACK_LIST[i] = GUILayout.Toggle(Jungle.TARGET_ATTACK_LIST[i], $"目标{i + 1}", "Button");
		}
		EditorGUILayout.EndHorizontal();
		Jungle.JUNGLE_STAR = EditorGUILayout.IntSlider("星级（如果是黑暗机甲）", Jungle.JUNGLE_STAR, 1, 5);
		Jungle.REPEAT_5 = EditorGUILayout.Toggle("是否5连（如果可以5连）", Jungle.REPEAT_5);
		
		Rect rect2 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect2 = new Rect(rect2.x, rect2.y + 4.5F, rect2.width, 1);
		EditorGUI.DrawRect(wireRect2, Color.gray);
		
		Jungle.SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", Jungle.SQUAD_NUMBER, 1, 8);
		EditorGUILayout.BeginHorizontal();
		foreach (Recognize.HeroType type in Enum.GetValues(typeof(Recognize.HeroType))) {
			bool isSelected = type == Jungle.HERO_AVATAR;
			bool newIsSelected = GUILayout.Toggle(isSelected, Utils.GetEnumInspectorName(type), "Button");
			if (newIsSelected && !isSelected) {
				Jungle.HERO_AVATAR = type;
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
				Jungle.USE_BOTTLE_DICT.TryGetValue(type, out int count);
				int newCount = Math.Max(EditorGUILayout.IntField(Utils.GetEnumInspectorName(type), Math.Abs(count)), 0);
				if (EditorGUI.EndChangeCheck()) {
					count = count < 0 ? -newCount : newCount;
					Jungle.USE_BOTTLE_DICT[type] = count;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Toggle(count > 0, GUILayout.Width(16F));
				if (EditorGUI.EndChangeCheck()) {
					count = -count;
					Jungle.USE_BOTTLE_DICT[type] = count;
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		
		GUILayout.Space(5F);
		
		EditorGUILayout.BeginHorizontal();
		if (Jungle.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
		Jungle.Test = GUILayout.Toggle(Jungle.Test, "测试", "Button", GUILayout.Width(60F));
		EditorGUILayout.EndHorizontal();
	}
}