/*
 * @Author: wangyun
 * @CreateTime: 2023-10-24 04:22:33 341
 * @LastEditor: wangyun
 * @EditTime: 2023-10-24 04:22:33 346
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AllianceMechaDonateConfig : PrefsEditorWindow<AllianceMechaDonate> {
	[MenuItem("Tools_Window/Default/AllianceMechaDonate", false, 21)]
	private static void Open() {
		GetWindow<AllianceMechaDonateConfig>("联盟机甲捐献").Show();
	}
	
	private readonly Dictionary<Recognize.AllianceMechaType, bool> m_DonateCountEditing = new Dictionary<Recognize.AllianceMechaType, bool>();

	private void OnGUI() {
		for (Recognize.AllianceMechaType type = Recognize.AllianceMechaType.ALPHA; type <= Recognize.AllianceMechaType.EPSILON; type++) {
			string mechaName = Utils.GetEnumInspectorName(type);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(mechaName);
			AllianceMechaDonate.VALID_DICT.TryGetValue(type, out bool valid);
			AllianceMechaDonate.VALID_DICT[type] = GUILayout.Toggle(valid, "启用", "Button");
			EditorGUILayout.EndHorizontal();
			if (valid) {
				DateTime fixedTime = DateTime.Now + new TimeSpan(1, 0, 0, 0);
				if (AllianceMechaDonate.FIXED_TIME_DICT.TryGetValue(type, out DateTime _fixedTime)) {
					fixedTime = _fixedTime;
				}
				EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginChangeCheck();
				float prevLFieldWidth = EditorGUIUtility.fieldWidth;
				EditorGUIUtility.fieldWidth = 20F;
				TimeSpan ts = fixedTime - DateTime.Now;
				int days = EditorGUILayout.IntField("    捐献倒计时", ts.Days);
				EditorGUIUtility.fieldWidth = 30F;
				float prevLabelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 14F;
				int hours = EditorGUILayout.IntField("天", ts.Hours);
				EditorGUIUtility.labelWidth = 8F;
				int minutes = EditorGUILayout.IntField(":", ts.Minutes);
				int seconds = EditorGUILayout.IntField(":", ts.Seconds);
				EditorGUIUtility.labelWidth = prevLabelWidth;
				EditorGUIUtility.fieldWidth = prevLFieldWidth;
				if (EditorGUI.EndChangeCheck()) {
					AllianceMechaDonate.FIXED_TIME_DICT[type] = DateTime.Now + new TimeSpan(days, hours, minutes, seconds);
				}
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("    捐献数量");
				m_DonateCountEditing.TryGetValue(type, out bool editing);
				m_DonateCountEditing[type] = editing = GUILayout.Toggle(editing, "编辑", "Button");
				EditorGUILayout.EndHorizontal();
				if (!AllianceMechaDonate.DONATE_COUNTS_DICT.TryGetValue(type, out int[] counts)) {
					AllianceMechaDonate.DONATE_COUNTS_DICT[type] = counts = new int[6];
				}
				if (editing) {
					for (int i = 0; i <= 5; ++i) {
						EditorGUILayout.BeginHorizontal();
						counts[i] = EditorGUILayout.IntField($"        {i + 1}级", counts[i]);
						EditorGUILayout.LabelField($"片");
						EditorGUILayout.EndHorizontal();
					}
				} else {
					for (int i = 0; i <= 5; ++i) {
						int count = counts[i];
						if (count > 0) {
							EditorGUILayout.LabelField($"        {i + 1}级{count}片");
						}
					}
				}
			}
		}

		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);
		
		{
			AllianceMechaDonate.INTERVAL = EditorGUILayout.IntSlider("捐献失败尝试间隔（秒）", AllianceMechaDonate.INTERVAL, 120, 1800);
			if (AllianceMechaDonate.IsAnyMechaFixed()) {
				EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginChangeCheck();
				TimeSpan ts = AllianceMechaDonate.NEXT_TIME - DateTime.Now;
				int hours = EditorGUILayout.IntField("  下次尝试时间", ts.Hours);
				float prevLabelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 8F;
				int minutes = EditorGUILayout.IntField(":", ts.Minutes);
				int seconds = EditorGUILayout.IntField(":", ts.Seconds);
				EditorGUIUtility.labelWidth = prevLabelWidth;
				if (EditorGUI.EndChangeCheck()) {
					AllianceMechaDonate.NEXT_TIME = DateTime.Now + new TimeSpan(hours, minutes, seconds);
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		AllianceMechaDonate.DEFAULT_DONATE_COUNT = EditorGUILayout.IntSlider("默认捐献数量", AllianceMechaDonate.DEFAULT_DONATE_COUNT, 1, 10);
		GUILayout.Space(5F);
		EditorGUILayout.BeginHorizontal();
		if (AllianceMechaDonate.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
		if (GUILayout.Button("单次执行", GUILayout.Width(60F))) {
			EditorApplication.ExecuteMenuItem("Tools_Task/OnceAllianceMechaDonate");
		}
		EditorGUILayout.EndHorizontal();
	}
	
	private void Update() {
		Repaint();
	}
}