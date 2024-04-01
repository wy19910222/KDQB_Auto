/*
 * @Author: wangyun
 * @CreateTime: 2024-04-02 02:31:42 315
 * @LastEditor: wangyun
 * @EditTime: 2024-04-02 02:31:42 320
 */

using System;
using UnityEditor;
using UnityEngine;

public class AutoCastCastleAbilityConfig : PrefsEditorWindow<AutoCastCastleAbility> {
	[MenuItem("Tools_Window/Default/AutoCastCastleAbility")]
	private static void Open() {
		GetWindow<AutoCastCastleAbilityConfig>("城堡技能自动施放").Show();
	}
	
	private static readonly Color COLOR_LINE_ODD = new Color(0, 0, 0, 0.2F);
	
	private void OnGUI() {
		AutoCastCastleAbility.RETRY_DELAY = EditorGUILayout.IntSlider("失败重试延迟", AutoCastCastleAbility.RETRY_DELAY, 60, 3600);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("技能列表");
		if (GUILayout.Button("+", GUILayout.Width(50F))) {
			int count = AutoCastCastleAbility.ABILITIES.Count;
			if (count > 0) {
				CastleAbility ability = AutoCastCastleAbility.ABILITIES[^1];
				ability.name = string.Empty;
				ability.cooldownTime += new TimeSpan(1, 0, 0);
				AutoCastCastleAbility.ABILITIES.Add(ability);
			} else {
				AutoCastCastleAbility.ABILITIES.Add(new CastleAbility {
					name = string.Empty,
					order = 1,
					cooldownHours = 24,
					cooldownTime = DateTime.Now + new TimeSpan(1, 0, 0),
				});
			}
		}
		EditorGUILayout.EndHorizontal();
		for (int i = 0, length = AutoCastCastleAbility.ABILITIES.Count; i < length; i++) {
			Rect rect = EditorGUILayout.BeginVertical();
			GUILayout.Space(5F);
			if ((i & 1) != 1) {
				rect.x += 5;
				EditorGUI.DrawRect(rect, COLOR_LINE_ODD);
			}
			CastleAbility ability = AutoCastCastleAbility.ABILITIES[i];
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.BeginHorizontal();
			ability.name = EditorGUILayout.TextField("    名称", ability.name);
			if (GUILayout.Button("×", GUILayout.Width(30F))) {
				int index = i;
				EditorApplication.delayCall += () => AutoCastCastleAbility.ABILITIES.RemoveAt(index);
			}
			EditorGUILayout.EndHorizontal();
			ability.order = EditorGUILayout.IntField("    排序（从1开始）", ability.order);
			ability.cooldownHours = EditorGUILayout.IntSlider("    冷却小时数", ability.cooldownHours, 11, 24);
			if (EditorGUI.EndChangeCheck()) {
				AutoCastCastleAbility.ABILITIES[i] = ability;
			}
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			TimeSpan ts = ability.Countdown;
			float prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 10F;
			int hours = EditorGUILayout.DelayedIntField("    ", (int) ts.TotalHours);
			EditorGUIUtility.labelWidth = 8F;
			int minutes = EditorGUILayout.DelayedIntField(":", ts.Minutes);
			int seconds = EditorGUILayout.DelayedIntField(":", ts.Seconds);
			EditorGUIUtility.labelWidth = prevLabelWidth;
			if (EditorGUI.EndChangeCheck()) {
				ability.Countdown = new TimeSpan(hours, minutes, seconds);
				AutoCastCastleAbility.ABILITIES.RemoveAt(i);
				bool inserted = false;
				for (int index = 0, count = AutoCastCastleAbility.ABILITIES.Count; index < count; ++index) {
					if (ability.cooldownTime < AutoCastCastleAbility.ABILITIES[index].cooldownTime) {
						AutoCastCastleAbility.ABILITIES.Insert(index, ability);
						inserted = true;
						break;
					}
				}
				if (!inserted) {
					AutoCastCastleAbility.ABILITIES.Add(ability);
				}
			}
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5F);
			EditorGUILayout.EndVertical();
		}
		
		GUILayout.Space(5F);
		if (AutoCastCastleAbility.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
	}
	
	private void Update() {
		Repaint();
	}
}