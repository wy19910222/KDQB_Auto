/*
 * @Author: wangyun
 * @CreateTime: 2023-11-05 19:12:07 765
 * @LastEditor: wangyun
 * @EditTime: 2023-11-05 19:12:07 769
 */

using UnityEngine;
using UnityEditor;

public class FreeDiamondConfig : PrefsEditorWindow<FreeDiamond> {
	[MenuItem("Tools_Window/Default/FreeDiamond", false, 20)]
	private static void Open() {
		GetWindow<FreeDiamondConfig>("周卡免费钻石").Show();
	}
	
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.IntField("剩余次数", FreeDiamond.LEFT_COUNT);
		if (GUILayout.Button("重置", GUILayout.Width(EditorGUIUtility.fieldWidth))) {
			FreeDiamond.LEFT_COUNT = 999;
		}
		EditorGUILayout.EndHorizontal();
		FreeDiamond.TAB_ORDER = EditorGUILayout.IntSlider("标签排序（周卡排第几个）", FreeDiamond.TAB_ORDER, 1, 10);
		GUILayout.Space(5F);
		if (FreeDiamond.IsRunning) {
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