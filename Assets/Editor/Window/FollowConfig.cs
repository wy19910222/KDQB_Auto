/*
 * @Author: wangyun
 * @CreateTime: 2023-09-28 02:57:33 976
 * @LastEditor: wangyun
 * @EditTime: 2023-09-28 02:57:33 983
 */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FollowConfig : PrefsEditorWindow<Follow> {
	private string m_TempJXOwnerName;
	private Vector2 m_ScrollPos;
	
	[MenuItem("Tools_Window/Default/Follow", false, 0)]
	private static void Open() {
		GetWindow<FollowConfig>("跟车").Show();
	}
	
	private void OnGUI() {
		Follow.SINGLE_GROUP = EditorGUILayout.Toggle("单队列跟车", Follow.SINGLE_GROUP);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("跟车延迟", GUILayout.Width(EditorGUIUtility.labelWidth));
		GUILayoutOption optionWidth = GUILayout.Width(Mathf.Clamp(EditorGUIUtility.currentViewWidth * 0.5F - 110, 26, 50));
		Follow.FOLLOW_DELAY_MIN = EditorGUILayout.FloatField(Follow.FOLLOW_DELAY_MIN, optionWidth);
		EditorGUILayout.MinMaxSlider(ref Follow.FOLLOW_DELAY_MIN, ref Follow.FOLLOW_DELAY_MAX, 0, 10);
		Follow.FOLLOW_DELAY_MAX = EditorGUILayout.FloatField(Follow.FOLLOW_DELAY_MAX, optionWidth);
		EditorGUILayout.EndHorizontal();
		Follow.FOLLOW_COOLDOWN = EditorGUILayout.FloatField("同一人跟车冷却", Follow.FOLLOW_COOLDOWN);
		
		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}

		EditorGUILayout.BeginHorizontal();
		Follow.KEEP_NO_WINDOW = GUILayout.Toggle(Follow.KEEP_NO_WINDOW, "在外面跟车", "Button");
		Follow.FEAR_STAR_FIRST = GUILayout.Toggle(Follow.FEAR_STAR_FIRST, "惧星优先", "Button");
		bool newResetDaily = GUILayout.Toggle(Follow.RESET_DAILY, "每日重置次数", "Button");
		if (newResetDaily != Follow.RESET_DAILY) {
			Follow.RESET_DAILY = newResetDaily;
			if (newResetDaily) {
				Follow.LAST_RESET_TIME = DateTime.Now.Date;
			}
		}
		EditorGUILayout.EndHorizontal();
		
		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}
		
		EditorGUILayout.BeginHorizontal();
		bool fearStarHelpEnabled = Follow.FEAR_STAR_HELP_COUNT > 0;
		bool newFearStarHelpEnabled = GUILayout.Toggle(Follow.FEAR_STAR_HELP_COUNT > 0, "帮打", "Button", GUILayout.Width(40F - 4F));
		if (newFearStarHelpEnabled != fearStarHelpEnabled) {
			Follow.FEAR_STAR_HELP_COUNT = newFearStarHelpEnabled ? Follow.GetDefaultCount(Recognize.FollowType.FEAR_STAR) : 0;
		}
		EditorGUI.BeginDisabledGroup(!newFearStarHelpEnabled);
		bool ownerExist = Follow.OwnerNameDict.TryGetValue(Follow.FEAR_STAR_HELP_OWNER, out Color32[,] targetColors) && targetColors != null;
		Follow.FEAR_STAR_HELP_OWNER = EditorGUILayout.TextField(Follow.FEAR_STAR_HELP_OWNER);
		GUILayout.Space(-4F);
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 36F;
		Follow.FEAR_STAR_HELP_COUNT = EditorGUILayout.IntField("的惧星", Follow.FEAR_STAR_HELP_COUNT, GUILayout.Width(58F));
		GUILayout.Space(-4F);
		EditorGUILayout.LabelField("次", GUILayout.Width(14F));
		if (GUILayout.Button(ownerExist ? "更新车主" : "记录车主", GUILayout.Width(60F))) {
			Follow.RecordFollowOwnerName(Follow.FEAR_STAR_HELP_OWNER);
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUIUtility.labelWidth = 80F;
		Follow.FEAR_STAR_HELP_SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", Follow.FEAR_STAR_HELP_SQUAD_NUMBER, 1, 8);
		EditorGUIUtility.labelWidth = prevLabelWidth;
		Follow.FEAR_STAR_HELP_MUST_FULL_SOLDIER = GUILayout.Toggle(Follow.FEAR_STAR_HELP_MUST_FULL_SOLDIER, "必须满兵", "Button", GUILayout.Width(60F));
		EditorGUILayout.EndHorizontal();
		EditorGUI.EndDisabledGroup();
		
		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}

		CustomField(Recognize.FollowType.UNKNOWN);
		if (EditorGUIUtility.currentViewWidth > 460) {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
			CustomField(Recognize.FollowType.WAR_HAMMER);
			CustomField(Recognize.FollowType.REFUGEE_CAMP);
			CustomField(Recognize.FollowType.FEAR_STAR);
			EditorGUILayout.EndVertical();
			EditorGUILayout.BeginVertical();
			CustomField(Recognize.FollowType.STRONGHOLD);
			CustomField(Recognize.FollowType.ELITE_GUARD);
			CustomField(Recognize.FollowType.HEART_PANG);
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		} else {
			CustomField(Recognize.FollowType.STRONGHOLD);
			CustomField(Recognize.FollowType.ELITE_GUARD);
			CustomField(Recognize.FollowType.HEART_PANG);
			GUILayout.Space(5F);
			CustomField(Recognize.FollowType.WAR_HAMMER);
			CustomField(Recognize.FollowType.REFUGEE_CAMP);
			CustomField(Recognize.FollowType.FEAR_STAR);
		}
		if (Follow.TypeCountDict.TryGetValue(Recognize.FollowType.FEAR_STAR, out int count) && count > 0) {
			GUILayout.Space(5F);
			m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, GUILayout.MaxHeight(240F), GUILayout.ExpandHeight(false));
			foreach (var ownerName in new List<string>(Follow.OwnerNameDict.Keys)) {
				if (!Follow.OwnerEnabledDict.ContainsKey(ownerName)) {
					Follow.OwnerEnabledDict.Add(ownerName, false);
				}
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(16F);
				bool enabled = Follow.OwnerEnabledDict[ownerName];
				bool newEnabled = EditorGUILayout.Toggle($"{ownerName}的惧星", enabled);
				if (newEnabled != enabled) {
					Follow.OwnerEnabledDict[ownerName] = newEnabled;
				}
				if (m_Debug) {
					if (GUILayout.Button("更新", GUILayout.Width(60F))) {
						Follow.RecordFollowOwnerName(ownerName);
					}
					if (GUILayout.Button("判断", GUILayout.Width(60F))) {
						Follow.LogFollowOwnerNameSimilarity(ownerName);
					}
					if (GUILayout.Button("删除", GUILayout.Width(60F))) {
						Follow.RemoveFollowOwnerName(ownerName);
					}
				}
				EditorGUILayout.EndHorizontal();
			}

			if (m_Debug) {
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(16F);
				m_TempJXOwnerName = EditorGUILayout.TextField(m_TempJXOwnerName);
				if (GUILayout.Button("添加", GUILayout.Width(60F))) {
					Follow.RecordFollowOwnerName(m_TempJXOwnerName);
					m_TempJXOwnerName = string.Empty;
				}
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(16F);
				if (GUILayout.Button("测试")) {
					Debug.LogError(Follow.IsFollowOwnerEnabled());
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
		}
		GUILayout.Space(5F);
		if (Follow.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
	}

	private static void CustomField(Recognize.FollowType type) {
		Follow.TypeCountDict.TryGetValue(type, out int count);
		Follow.TypeCanOuterDict.TryGetValue(type, out bool canOuter);
		Follow.TypeWillResetDict.TryGetValue(type, out bool willReset);
		CustomField($"跟{Utils.GetEnumInspectorName(type)}", Follow.GetDefaultCount(type), ref count, ref canOuter, ref willReset);
		Follow.TypeCountDict[type] = count;
		Follow.TypeCanOuterDict[type] = canOuter;
		Follow.TypeWillResetDict[type] = willReset;
	}

	private static void CustomField(string label, int defaultValue, ref int count, ref bool canOuter, ref bool willReset) {
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		int newCount = Math.Max(EditorGUILayout.IntField(label, Math.Abs(count)), 0);
		if (EditorGUI.EndChangeCheck()) {
			count = count < 0 ? -newCount : newCount;
		}
		EditorGUI.BeginChangeCheck();
		bool isOpen = EditorGUILayout.Toggle(count > 0, GUILayout.Width(16F));
		if (EditorGUI.EndChangeCheck()) {
			if (isOpen && count == 0) {
				count = defaultValue;
			} else {
				count = -count;
			}
		}
		if (Follow.KEEP_NO_WINDOW) {
			canOuter = GUILayout.Toggle(canOuter, "外面跟车", "Button", GUILayout.Width(64F));
		}
		if (Follow.RESET_DAILY) {
			willReset = GUILayout.Toggle(willReset, "每日重置", "Button", GUILayout.Width(64F));
		}
		EditorGUILayout.EndHorizontal();
	}
}