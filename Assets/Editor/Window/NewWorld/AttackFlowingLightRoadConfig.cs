/*
 * @Author: wangyun
 * @CreateTime: 2023-10-31 07:02:42 199
 * @LastEditor: wangyun
 * @EditTime: 2023-10-31 07:02:42 204
 */

using System;
using UnityEditor;
using UnityEngine;

public class AttackFlowingLightRoadConfig : PrefsEditorWindow<AttackFlowingLightRoad> {
	[MenuItem("Tools_Window/NewWorld/AttackFlowingLightRoad", false, 3)]
	private static void Open() {
		GetWindow<AttackFlowingLightRoadConfig>("攻击流光之路").Show();
	}

	private readonly GUIStyle m_Style = new GUIStyle();
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("攻击目标", GUILayout.Width(EditorGUIUtility.labelWidth - 2F));
		string[] titles = {"左下", "左上", "右上", "右下"};
		for (int i = 0; i < 4; ++i) {
			bool targetSelected = AttackFlowingLightRoad.ATTACK_TARGET == i;
			bool newTargetSelected = GUILayout.Toggle(targetSelected, titles[i], "Button");
			if (!targetSelected && newTargetSelected) {
				AttackFlowingLightRoad.ATTACK_TARGET = i;
			}
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		GUIContent content = new GUIContent($"{AttackFlowingLightRoad.AttackTimes} /");
		float width = m_Style.CalcSize(content).x + 3;
		GUILayout.Space(EditorGUIUtility.labelWidth + 2);
		EditorGUILayout.LabelField(content, "RightLabel", GUILayout.Width(width));
		EditorGUIUtility.labelWidth += width;
		GUILayout.Space(-EditorGUIUtility.labelWidth - 2);
		AttackFlowingLightRoad.ATTACK_TIMES = EditorGUILayout.IntField("攻击次数", AttackFlowingLightRoad.ATTACK_TIMES);
		EditorGUIUtility.labelWidth -= width;
		if (GUILayout.Button("-")) {
			AttackFlowingLightRoad.s_AttackTimeList.RemoveAt(AttackFlowingLightRoad.s_AttackTimeList.Count - 1);
		}
		if (GUILayout.Button("+")) {
			AttackFlowingLightRoad.s_AttackTimeList.Add(DateTime.Now);
		}
		EditorGUILayout.EndHorizontal();
		AttackFlowingLightRoad.SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", AttackFlowingLightRoad.SQUAD_NUMBER, 1, 8);
		AttackFlowingLightRoad.USE_SMALL_BOTTLE = EditorGUILayout.Toggle("是否使用小体", AttackFlowingLightRoad.USE_SMALL_BOTTLE);
		AttackFlowingLightRoad.USE_BIG_BOTTLE = EditorGUILayout.Toggle("是否使用大体", AttackFlowingLightRoad.USE_BIG_BOTTLE);
		AttackFlowingLightRoad.ENERGY_WAIT_SECONDS = EditorGUILayout.IntSlider("体力不足重试冷却(秒)", AttackFlowingLightRoad.ENERGY_WAIT_SECONDS, 60, 600);
		
		GUILayout.Space(5F);
		if (AttackFlowingLightRoad.IsRunning) {
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