/*
 * @Author: wangyun
 * @CreateTime: 2024-06-01 15:12:53 886
 * @LastEditor: wangyun
 * @EditTime: 2024-06-01 15:12:53 892
 */

using UnityEngine;
using UnityEditor;

public class CapsuleToysConfig : PrefsEditorWindow<CapsuleToys> {
	[MenuItem("Tools_Window/Activity/CapsuleToys")]
	private static void Open() {
		GetWindow<CapsuleToysConfig>("自动扭蛋").Show();
	}

	private static readonly string[] s_LuckLevelNames = {"自定义", "非酋", "黑人", "普通", "白人", "欧皇"};

	private void OnGUI() {
		CapsuleToys.KEY_START = (KeyboardUtils.VKCode) EditorGUILayout.EnumPopup("开始快捷键", CapsuleToys.KEY_START);
		CapsuleToys.KEY_STOP = (KeyboardUtils.VKCode) EditorGUILayout.EnumPopup("终止快捷键", CapsuleToys.KEY_STOP);
		EditorGUILayout.LabelField("方案");
		EditorGUILayout.BeginHorizontal();
		for (int i = 0, length = CapsuleToys.GET_OF_STAR_PLANS.Length; i < length; i++) {
			bool selected = i == CapsuleToys.LUCK_LEVEL;
			bool newSelected = GUILayout.Toggle(i == CapsuleToys.LUCK_LEVEL, s_LuckLevelNames[i], "Button");
			if (newSelected && !selected) {
				CapsuleToys.LUCK_LEVEL = i;
			}
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.LabelField("收取星级");
		EditorGUI.BeginDisabledGroup(CapsuleToys.LUCK_LEVEL != 0);
		EditorGUILayout.BeginHorizontal();
		bool[] getOfStar = CapsuleToys.GET_OF_STAR_PLANS[CapsuleToys.LUCK_LEVEL];
		for (int i = 0, length = getOfStar.Length; i < length; i++) {
			getOfStar[i] = GUILayout.Toggle(getOfStar[i], $"{i + 1}星", "Button");
		}
		EditorGUILayout.EndHorizontal();
		EditorGUI.EndDisabledGroup();
		
		GUILayout.Space(5F);
		if (CapsuleToys.IsRunning) {
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