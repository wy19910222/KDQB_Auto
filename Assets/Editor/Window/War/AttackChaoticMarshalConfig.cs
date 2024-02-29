/*
 * @Author: wangyun
 * @CreateTime: 2023-10-31 07:02:42 199
 * @LastEditor: wangyun
 * @EditTime: 2023-10-31 07:02:42 204
 */

using System;
using UnityEngine;
using UnityEditor;

public class AttackChaoticMarshalConfig : PrefsEditorWindow<AttackChaoticMarshal> {
	[MenuItem("Tools_Window/War/AttackChaoticMarshal")]
	private static void Open() {
		GetWindow<AttackChaoticMarshalConfig>("攻击混乱之源").Show();
	}

	private readonly GUIStyle m_Style = new GUIStyle();
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		GUIContent content = new GUIContent($"{AttackChaoticMarshal.AttackTimes} /");
		float width = m_Style.CalcSize(content).x + 3;
		GUILayout.Space(EditorGUIUtility.labelWidth + 2);
		EditorGUILayout.LabelField(content, "RightLabel", GUILayout.Width(width));
		EditorGUIUtility.labelWidth += width;
		GUILayout.Space(-EditorGUIUtility.labelWidth - 2);
		AttackChaoticMarshal.ATTACK_TIMES = EditorGUILayout.IntField("攻击次数", AttackChaoticMarshal.ATTACK_TIMES);
		EditorGUIUtility.labelWidth -= width;
		if (GUILayout.Button("-")) {
			AttackChaoticMarshal.s_AttackTimeList.RemoveAt(AttackChaoticMarshal.s_AttackTimeList.Count - 1);
		}
		if (GUILayout.Button("+")) {
			AttackChaoticMarshal.s_AttackTimeList.Add(DateTime.Now);
		}
		EditorGUILayout.EndHorizontal();
		AttackChaoticMarshal.SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", AttackChaoticMarshal.SQUAD_NUMBER, 1, 8);
		AttackChaoticMarshal.USE_SMALL_BOTTLE = EditorGUILayout.Toggle("是否使用小体", AttackChaoticMarshal.USE_SMALL_BOTTLE);
		AttackChaoticMarshal.USE_BIG_BOTTLE = EditorGUILayout.Toggle("是否使用大体", AttackChaoticMarshal.USE_BIG_BOTTLE);
		GUILayout.Space(5F);
		if (AttackChaoticMarshal.IsRunning) {
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