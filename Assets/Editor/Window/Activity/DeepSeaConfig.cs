/*
 * @Author: wangyun
 * @CreateTime: 2023-10-12 00:00:51 751
 * @LastEditor: wangyun
 * @EditTime: 2023-10-12 00:00:51 756
 */

using System;
using UnityEditor;
using UnityEngine;

public class DeepSeaConfig : PrefsEditorWindow<DeepSea> {
	[MenuItem("Tools_Window/Activity/DeepSea")]
	private static void Open() {
		GetWindow<DeepSeaConfig>("深海寻宝").Show();
	}
	
	private void OnGUI() {
		DeepSea.ACTIVITY_ORDER = EditorGUILayout.IntSlider("活动排序（活动排在第几个）", DeepSea.ACTIVITY_ORDER, 1, 20);
		DeepSea.ORDER_RETRY_INTERVAL = EditorGUILayout.IntSlider("寻找标签重试间隔（秒）", DeepSea.ORDER_RETRY_INTERVAL, 60, 600);

		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}
		
		DeepSea.DETECTOR_COUNT = EditorGUILayout.IntSlider("拥有探测器", DeepSea.DETECTOR_COUNT, 1, 3);
		DeepSea.TRY_COUNT = EditorGUILayout.IntSlider("探测器点击次数", DeepSea.TRY_COUNT, 4, 10);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("倒计时");
		if (GUILayout.Button("+", GUILayout.Width(50F))) {
			int count = DeepSea.TargetDTs.Count;
			DeepSea.TargetDTs.Add(count > 0 ? DeepSea.TargetDTs[count - 1] : DateTime.Now);
		}
		EditorGUILayout.EndHorizontal();
		for (int i = 0, length = DeepSea.TargetDTs.Count; i < length; i++) {
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			TimeSpan ts = DeepSea.TargetDTs[i] - DateTime.Now;
			float prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 20F;
			int hours = EditorGUILayout.IntField(" ", ts.Hours);
			EditorGUIUtility.labelWidth = 8F;
			int minutes = EditorGUILayout.IntField(":", ts.Minutes);
			int seconds = EditorGUILayout.IntField(":", ts.Seconds);
			EditorGUIUtility.labelWidth = prevLabelWidth;
			if (EditorGUI.EndChangeCheck()) {
				DeepSea.TargetDTs[i] = DateTime.Now + new TimeSpan(hours, minutes, seconds);
			}
			if (i == 0) {
				GUILayout.Space(30F + 2F);
			} else {
				if (GUILayout.Button("×", GUILayout.Width(30F))) {
					DeepSea.TargetDTs.RemoveAt(i);
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		
		GUILayout.Space(5F);
		if (DeepSea.IsRunning) {
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