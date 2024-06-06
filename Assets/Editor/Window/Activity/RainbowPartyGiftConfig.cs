/*
 * @Author: wangyun
 * @CreateTime: 2024-06-06 19:41:32 167
 * @LastEditor: wangyun
 * @EditTime: 2024-06-06 19:41:32 172
 */

using System;
using UnityEditor;
using UnityEngine;

public class RainbowPartyGiftConfig : PrefsEditorWindow<RainbowPartyGift> {
	[MenuItem("Tools_Window/Activity/RainbowPartyGift")]
	private static void Open() {
		GetWindow<RainbowPartyGiftConfig>("彩虹派对礼品").Show();
	}
	
	private void OnGUI() {
		RainbowPartyGift.ACTIVITY_ORDER = EditorGUILayout.IntSlider("活动排序（活动排在第几个）", RainbowPartyGift.ACTIVITY_ORDER, 1, 20);
		RainbowPartyGift.INTERVAL = EditorGUILayout.IntSlider("领取间隔（秒）", RainbowPartyGift.INTERVAL, 60, 300);
		RainbowPartyGift.COUNT_OF_ONCE = EditorGUILayout.IntSlider("每次数量", RainbowPartyGift.COUNT_OF_ONCE, 1, 9);
		
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUI.BeginChangeCheck();
			TimeSpan ts = RainbowPartyGift.NEXT_TIME - DateTime.Now;
			int hours = EditorGUILayout.IntField("下一次领取倒计时", ts.Hours);
			float prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 8F;
			int minutes = EditorGUILayout.IntField(":", ts.Minutes);
			int seconds = EditorGUILayout.IntField(":", ts.Seconds);
			EditorGUIUtility.labelWidth = prevLabelWidth;
			if (EditorGUI.EndChangeCheck()) {
				RainbowPartyGift.NEXT_TIME = DateTime.Now + new TimeSpan(hours, minutes, seconds);
			}
		}
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space(5F);
		if (RainbowPartyGift.IsRunning) {
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