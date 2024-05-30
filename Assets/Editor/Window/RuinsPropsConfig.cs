﻿/*
 * @Author: wangyun
 * @CreateTime: 2024-05-28 22:30:05 645
 * @LastEditor: wangyun
 * @EditTime: 2024-05-28 22:30:05 651
 */

using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class RuinsPropsConfig : PrefsEditorWindow<RuinsProps> {
	[MenuItem("Tools_Window/Default/RuinsProps")]
	private static void Open() {
		GetWindow<RuinsPropsConfig>("遗迹道具").Show();
	}

	private ReorderableList m_List;

	protected override void OnEnable() {
		base.OnEnable();
		m_List = new ReorderableList(RuinsProps.RUIN_PROP_PRIORITY, typeof(Recognize.RuinPropType)) {
			drawHeaderCallback = rect => {
				EditorGUI.LabelField(rect, "道具优先级");
			},
			drawElementCallback = (rect, index, active, focused) => {
				RuinsProps.RUIN_PROP_PRIORITY[index] = (Recognize.RuinPropType) EditorGUI.EnumPopup(rect, RuinsProps.RUIN_PROP_PRIORITY[index]);
			}
		};
	}

	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("排序列表");
		// if (GUILayout.Button("+", GUILayout.Width(50F))) {
		// 	RuinsProps.RUIN_ORDERS.Add(RuinsProps.RUIN_ORDERS.Count > 0 ? RuinsProps.RUIN_ORDERS[^1] : 1);
		// }
		EditorGUILayout.EndHorizontal();
		for (int i = 0, length = RuinsProps.RUIN_ORDERS.Count; i < length; i++) {
			int order = RuinsProps.RUIN_ORDERS[i];
			int newOrder = EditorGUILayout.IntSlider("    排序1", order, 1, 8);
			if (newOrder != order) {
				RuinsProps.RUIN_ORDERS[i] = newOrder;
			}
		}
		
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		TimeSpan refreshCountdown = RuinsProps.LAST_REFRESH_TIME + RuinsProps.INTERVAL - DateTime.Now;
		int hours = EditorGUILayout.DelayedIntField("刷新倒计时", (int) refreshCountdown.TotalHours);
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 8F;
		int minutes = EditorGUILayout.DelayedIntField(":", refreshCountdown.Minutes);
		int seconds = EditorGUILayout.DelayedIntField(":", refreshCountdown.Seconds);
		EditorGUIUtility.labelWidth = prevLabelWidth;
		if (EditorGUI.EndChangeCheck()) {
			RuinsProps.LAST_REFRESH_TIME = DateTime.Now + new TimeSpan(hours, minutes, seconds) - RuinsProps.INTERVAL;
		}
		EditorGUILayout.EndHorizontal();

		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);
		
		m_List.DoLayoutList();
		
		GUILayout.Space(5F);
		EditorGUILayout.BeginHorizontal();
		RuinsProps.Test = GUILayout.Toggle(RuinsProps.Test, "测试模式", "Button", GUILayout.Width(60F));
		if (RuinsProps.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
		if (GUILayout.Button("单次执行", GUILayout.Width(60F))) {
			EditorApplication.ExecuteMenuItem("Tools_Task/OnceRuinsProps");
		}
		EditorGUILayout.EndHorizontal();
	}
	
	private void Update() {
		Repaint();
	}
}