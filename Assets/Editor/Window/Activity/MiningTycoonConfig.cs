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
		MiningTycoon.ORDER_TRY_RADIUS = EditorGUILayout.IntSlider("寻找标签半径", MiningTycoon.ORDER_TRY_RADIUS, 1, 6);
		MiningTycoon.ORDER_RETRY_INTERVAL = EditorGUILayout.IntSlider("寻找标签重试间隔（秒）", MiningTycoon.ORDER_RETRY_INTERVAL, 60, 600);

		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("收取矿车编号", GUILayout.Width(EditorGUIUtility.labelWidth - 2F));
		MiningTycoon.SMART_COLLECT = GUILayout.Toggle(MiningTycoon.SMART_COLLECT, "智能收取", "Button", GUILayout.Width(60F));
		if (!MiningTycoon.SMART_COLLECT) {
			MiningTycoon.TRAMCAR_COUNTDOWN_NUMBER = EditorGUILayout.IntSlider(MiningTycoon.TRAMCAR_COUNTDOWN_NUMBER, 1, 4);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		{
			EditorGUI.BeginChangeCheck();
			TimeSpan ts = MiningTycoon.NEAREST_DT - DateTime.Now;
			int hours = EditorGUILayout.IntField("收取倒计时", ts.Hours);
			float prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 8F;
			int minutes = EditorGUILayout.IntField(":", ts.Minutes);
			int seconds = EditorGUILayout.IntField(":", ts.Seconds);
			EditorGUIUtility.labelWidth = prevLabelWidth;
			if (EditorGUI.EndChangeCheck()) {
				MiningTycoon.NEAREST_DT = DateTime.Now + new TimeSpan(hours, minutes, seconds);
			}
		}
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUI.BeginChangeCheck();
			float prevLFieldWidth = EditorGUIUtility.fieldWidth;
			EditorGUIUtility.fieldWidth = 20F;
			TimeSpan ts = MiningTycoon.ACTIVITY_END_DT - DateTime.Now;
			int days = EditorGUILayout.IntField("活动结束倒计时", ts.Days);
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
				MiningTycoon.ACTIVITY_END_DT = DateTime.Now + new TimeSpan(days, hours, minutes, seconds);
			}
		}
		EditorGUILayout.EndHorizontal();
		
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