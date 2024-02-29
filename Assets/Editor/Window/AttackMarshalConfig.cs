/*
 * @Author: wangyun
 * @CreateTime: 2023-10-31 07:02:42 199
 * @LastEditor: wangyun
 * @EditTime: 2023-10-31 07:02:42 204
 */

using System;
using UnityEditor;
using UnityEngine;

public class AttackMarshalConfig : PrefsEditorWindow<AttackMarshal> {
	[MenuItem("Tools_Window/Default/AttackMarshal", false, 3)]
	private static void Open() {
		GetWindow<AttackMarshalConfig>("攻击元帅").Show();
	}

	private readonly GUIStyle m_Style = new GUIStyle();
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		GUIContent content = new GUIContent($"{AttackMarshal.AttackTimes} /");
		float width = m_Style.CalcSize(content).x + 3;
		GUILayout.Space(EditorGUIUtility.labelWidth + 2);
		EditorGUILayout.LabelField(content, "RightLabel", GUILayout.Width(width));
		EditorGUIUtility.labelWidth += width;
		GUILayout.Space(-EditorGUIUtility.labelWidth - 2);
		AttackMarshal.ATTACK_TIMES = EditorGUILayout.IntField("攻击次数", AttackMarshal.ATTACK_TIMES);
		EditorGUIUtility.labelWidth -= width;
		if (GUILayout.Button("-")) {
			AttackMarshal.s_AttackTimeList.RemoveAt(AttackMarshal.s_AttackTimeList.Count - 1);
		}
		if (GUILayout.Button("+")) {
			AttackMarshal.s_AttackTimeList.Add(DateTime.Now);
		}
		EditorGUILayout.EndHorizontal();
		AttackMarshal.SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", AttackMarshal.SQUAD_NUMBER, 1, 8);
		AttackMarshal.USE_SMALL_BOTTLE = EditorGUILayout.Toggle("是否使用小体", AttackMarshal.USE_SMALL_BOTTLE);
		AttackMarshal.USE_BIG_BOTTLE = EditorGUILayout.Toggle("是否使用大体", AttackMarshal.USE_BIG_BOTTLE);
		
		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);
		
		AttackMarshal.MARSHAL_WAIT_SECONDS = EditorGUILayout.IntSlider("发现元帅重试冷却(秒)", AttackMarshal.MARSHAL_WAIT_SECONDS, 60, 600);
		AttackMarshal.ENERGY_WAIT_SECONDS = EditorGUILayout.IntSlider("体力不足重试冷却(秒)", AttackMarshal.ENERGY_WAIT_SECONDS, 60, 600);
		GUILayout.Space(5F);
		if (AttackMarshal.IsRunning) {
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