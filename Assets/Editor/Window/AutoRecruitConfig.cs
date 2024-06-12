/*
 * @Author: wangyun
 * @CreateTime: 2024-03-23 03:25:46 708
 * @LastEditor: wangyun
 * @EditTime: 2024-03-23 03:25:46 713
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AutoRecruitConfig : PrefsEditorWindow<AutoRecruit> {
	[MenuItem("Tools_Window/Default/AutoRecruit", false, 22)]
	private static void Open() {
		GetWindow<AutoRecruitConfig>("自动招募").Show();
	}
	
	private readonly GUIStyle m_Style = new GUIStyle();
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		{
			EditorGUI.BeginChangeCheck();
			TimeSpan tryCountdown = AutoRecruit.s_SeniorRecruitTime + AutoRecruit.SENIOR_RECRUIT_COOLDOWN - DateTime.Now;
			float prevLFieldWidth = EditorGUIUtility.fieldWidth;
			EditorGUIUtility.fieldWidth = 20F;
			int hours = EditorGUILayout.DelayedIntField("高级招募倒计时", (int) tryCountdown.TotalHours);
			float prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 8F;
			int minutes = EditorGUILayout.DelayedIntField(":", tryCountdown.Minutes);
			int seconds = EditorGUILayout.DelayedIntField(":", tryCountdown.Seconds);
			EditorGUIUtility.labelWidth = prevLabelWidth;
			EditorGUIUtility.fieldWidth = prevLFieldWidth;
			if (EditorGUI.EndChangeCheck()) {
				AutoRecruit.s_SeniorRecruitTime = DateTime.Now + new TimeSpan(hours, minutes, seconds) - AutoRecruit.SENIOR_RECRUIT_COOLDOWN;
			}
		}
		EditorGUILayout.EndHorizontal();

		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}

		ShowGeneralRecruitOrSkillRecruit(AutoRecruit.s_GeneralRecruitTimeList, AutoRecruit.GENERAL_RECRUIT_TIMES_MAX, AutoRecruit.GENERAL_RECRUIT_COOLDOWN);

		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}
		
		ShowGeneralRecruitOrSkillRecruit(AutoRecruit.s_SkillRecruitTimeList, AutoRecruit.SKILL_RECRUIT_TIMES_MAX, AutoRecruit.SKILL_RECRUIT_COOLDOWN);
		
		GUILayout.Space(5F);
		ShowRunningButton();
	}

	private void ShowGeneralRecruitOrSkillRecruit(List<DateTime> timeList, int timesMax, TimeSpan cooldown) {
		DateTime date = DateTime.Now.Date;
		int count = timeList.Count;
		for (int i = count - 1; i >= 0; --i) {
			DateTime dt = timeList[i];
			if (dt < date) {
				timeList.RemoveAt(i);
				count--;
			}
		}
		EditorGUILayout.BeginHorizontal();
		{
			GUIContent content = new GUIContent($"{count} /");
			float width = m_Style.CalcSize(content).x + 3;
			GUILayout.Space(EditorGUIUtility.labelWidth + 2);
			EditorGUILayout.LabelField(content, "RightLabel", GUILayout.Width(width));
			EditorGUIUtility.labelWidth += width;
			GUILayout.Space(-EditorGUIUtility.labelWidth - 2);
			EditorGUILayout.IntField("普通招募次数", timesMax);
			EditorGUIUtility.labelWidth -= width;
			if (GUILayout.Button("-")) {
				timeList.RemoveAt(0);
			}
			if (GUILayout.Button("+")) {
				timeList.Insert(0, date);
			}
		}
		EditorGUILayout.EndHorizontal();
		if (count > 0 && count < timesMax) {
			EditorGUILayout.BeginHorizontal();
			{
				EditorGUI.BeginChangeCheck();
				TimeSpan tryCountdown = timeList[^1] + cooldown - DateTime.Now;
				float prevLFieldWidth = EditorGUIUtility.fieldWidth;
				EditorGUIUtility.fieldWidth = 20F;
				int hours = EditorGUILayout.DelayedIntField("高级招募倒计时", (int) tryCountdown.TotalHours);
				float prevLabelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 8F;
				int minutes = EditorGUILayout.DelayedIntField(":", tryCountdown.Minutes);
				int seconds = EditorGUILayout.DelayedIntField(":", tryCountdown.Seconds);
				EditorGUIUtility.labelWidth = prevLabelWidth;
				EditorGUIUtility.fieldWidth = prevLFieldWidth;
				if (EditorGUI.EndChangeCheck()) {
					timeList[^1] = DateTime.Now + new TimeSpan(hours, minutes, seconds) - cooldown;
				}
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}