/*
 * @Author: wangyun
 * @CreateTime: 2023-12-30 00:36:09 952
 * @LastEditor: wangyun
 * @EditTime: 2023-12-30 00:36:09 956
 */

using System;
using UnityEditor;
using UnityEngine;

public class AllianceHelpConfig : PrefsEditorWindow<AllianceHelp> {
	[MenuItem("Tools_Window/Default/AllianceHelp", false, 23)]
	private static void Open() {
		GetWindow<AllianceHelpConfig>("联盟帮助").Show();
	}
	
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("帮助类型：", GUILayout.Width(EditorGUIUtility.labelWidth - 2F));
		AllianceHelp.OUTER_HELPS = GUILayout.Toggle(AllianceHelp.OUTER_HELPS, "在外面帮助他人", "Button");
		AllianceHelp.INTO_REQUEST = GUILayout.Toggle(AllianceHelp.INTO_REQUEST, "去里面请求帮助", "Button");
		EditorGUILayout.EndHorizontal();
		if (AllianceHelp.INTO_REQUEST) {
			for (int i = AllianceHelp.TARGET_LIST.Count; i < 4; ++i) {
				AllianceHelp.TARGET_LIST.Add(0);
			}
			for (int i = 0; i < 4; ++i) {
				EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginChangeCheck();
				int count = AllianceHelp.TARGET_LIST[i];
				int newCount = Math.Max(EditorGUILayout.IntField($"    目标{i + 1}", Math.Abs(count)), 0);
				if (EditorGUI.EndChangeCheck()) {
					count = count < 0 ? -newCount : newCount;
					AllianceHelp.TARGET_LIST[i] = count;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Toggle(count > 0, GUILayout.Width(16F));
				if (EditorGUI.EndChangeCheck()) {
					count = count == 0 ? 999 : -count;
					AllianceHelp.TARGET_LIST[i] = count;
				}
				EditorGUILayout.EndHorizontal();
			}
			
			EditorGUILayout.BeginHorizontal();
			AllianceHelp.REQUEST_COIN = EditorGUILayout.Toggle("请求一次金币帮助", AllianceHelp.REQUEST_COIN);
			if (AllianceHelp.s_RequestCoinTime >= DateTime.Now.Date) {
				if (GUILayout.Button("取消已请求")) {
					AllianceHelp.s_RequestCoinTime = default;
				}
			} else {
				if (GUILayout.Button("设为已请求")) {
					AllianceHelp.s_RequestCoinTime = DateTime.Now;
				}
			}
			EditorGUILayout.EndHorizontal();
			AllianceHelp.INTO_HELPS_TIMES = EditorGUILayout.IntSlider("在里面帮助检查次数", AllianceHelp.INTO_HELPS_TIMES, 0, 10);
			AllianceHelp.INTERVAL = EditorGUILayout.IntSlider("尝试请求间隔（秒）", AllianceHelp.INTERVAL, 1800, 7200);
			bool started = AllianceHelp.s_StartTime > (DateTime.Now - new TimeSpan(0, 1, 0)).Date;
			if (started) {
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("今天已请求", GUILayout.Width(EditorGUIUtility.labelWidth - 2F));
				if (GUILayout.Button("取消已请求")) {
					AllianceHelp.s_StartTime = default;
				}
				EditorGUILayout.EndHorizontal();
			} else {
				TimeSpan ts = AllianceHelp.s_NextTime - DateTime.Now;
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField($"下一次尝试时间：{ts:hh\\:mm\\:ss}");
				if (GUILayout.Button("立即尝试")) {
					AllianceHelp.s_NextTime = DateTime.Now;
				}
				if (GUILayout.Button("设为已请求")) {
					AllianceHelp.s_StartTime = DateTime.Now;
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		GUILayout.Space(5F);
		if (AllianceHelp.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
	}
}