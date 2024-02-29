/*
 * @Author: wangyun
 * @CreateTime: 2023-10-24 04:22:33 341
 * @LastEditor: wangyun
 * @EditTime: 2023-10-24 04:22:33 346
 */

using System;
using UnityEditor;
using UnityEngine;

public class CollectEssenceConfig : PrefsEditorWindow<CollectEssence> {
	[MenuItem("Tools_Window/NewWorld/CollectEssence", false, 21)]
	private static void Open() {
		GetWindow<CollectEssenceConfig>("收取精华").Show();
	}
	
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		TimeSpan ts = CollectEssence.NEXT_TIME - DateTime.Now;
		int hours = EditorGUILayout.IntField("下次尝试时间", ts.Hours);
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 8F;
		int minutes = EditorGUILayout.IntField(":", ts.Minutes);
		int seconds = EditorGUILayout.IntField(":", ts.Seconds);
		EditorGUIUtility.labelWidth = prevLabelWidth;
		if (EditorGUI.EndChangeCheck()) {
			CollectEssence.NEXT_TIME = DateTime.Now + new TimeSpan(hours, minutes, seconds);
		}
		EditorGUILayout.EndHorizontal();
		CollectEssence.INTERVAL = EditorGUILayout.IntSlider("尝试收取间隔（秒）", CollectEssence.INTERVAL, 900, 5400);
		GUILayout.Space(5F);
		EditorGUILayout.BeginHorizontal();
		if (CollectEssence.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
		if (GUILayout.Button("单次执行", GUILayout.Width(60F))) {
			EditorApplication.ExecuteMenuItem("Tools_Task/OnceCollectEssence");
		}
		EditorGUILayout.EndHorizontal();
	}
	
	private void Update() {
		Repaint();
	}
}