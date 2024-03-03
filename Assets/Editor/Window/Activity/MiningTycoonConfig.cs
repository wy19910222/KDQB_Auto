/*
 * @Author: wangyun
 * @CreateTime: 2023-11-03 14:55:53 057
 * @LastEditor: wangyun
 * @EditTime: 2023-11-03 14:55:53 065
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class MiningTycoonConfig : PrefsEditorWindow<MiningTycoon> {
	[MenuItem("Tools_Window/Activity/MiningTycoon")]
	private static void Open() {
		GetWindow<MiningTycoonConfig>("矿产大亨").Show();
	}
	
	private void OnGUI() {
		MiningTycoon.ACTIVITY_ORDER = EditorGUILayout.IntSlider("活动排序（活动排在第几个）", MiningTycoon.ACTIVITY_ORDER, 1, 20);
		MiningTycoon.ORDER_RADIUS = EditorGUILayout.IntSlider("寻找标签半径", MiningTycoon.ORDER_RADIUS, 1, 6);
		MiningTycoon.TRAMCAR_COUNTDOWN_NUMBER = EditorGUILayout.IntSlider("收取矿车编号", MiningTycoon.TRAMCAR_COUNTDOWN_NUMBER, 1, 4);

		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		TimeSpan ts = MiningTycoon.NEAREST_DT - DateTime.Now;
		int hours = EditorGUILayout.IntField("倒计时", ts.Hours);
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 8F;
		int minutes = EditorGUILayout.IntField(":", ts.Minutes);
		int seconds = EditorGUILayout.IntField(":", ts.Seconds);
		EditorGUIUtility.labelWidth = prevLabelWidth;
		if (EditorGUI.EndChangeCheck()) {
			MiningTycoon.NEAREST_DT = DateTime.Now + new TimeSpan(hours, minutes, seconds);
		}
		EditorGUILayout.EndHorizontal();
		MiningTycoon.CLICK_INTERVAL = EditorGUILayout.IntSlider("点击间隔（小时）", MiningTycoon.CLICK_INTERVAL, 1, 24);
		
		GUILayout.Space(5F);
		if (MiningTycoon.IsRunning) {
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