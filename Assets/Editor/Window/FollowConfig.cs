﻿/*
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
		
		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);
		
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
		
		Rect rect2 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect2 = new Rect(rect2.x, rect2.y + 4.5F, rect2.width, 1);
		EditorGUI.DrawRect(wireRect2, Color.gray);

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
			m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos, GUILayout.MaxHeight(240F));
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
		Follow.TypeCountDict.TryGetValue(type, out int countHammer);
		Follow.TypeCountDict[type] = CustomField($"跟{Utils.GetEnumInspectorName(type)}", countHammer, Follow.GetDefaultCount(type));
	}

	private static int CustomField(string label, int count, int defaultValue) {
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
		EditorGUILayout.EndHorizontal();
		return count;
	}
}