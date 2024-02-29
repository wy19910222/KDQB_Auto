/*
 * @Author: wangyun
 * @CreateTime: 2023-11-05 19:03:52 124
 * @LastEditor: wangyun
 * @EditTime: 2023-11-05 19:03:52 128
 */

using UnityEngine;
using UnityEditor;

public class AutoFixConfig : PrefsEditorWindow<AutoFix> {
	[MenuItem("Tools_Window/Default/AutoFix", false, 22)]
	private static void Open() {
		GetWindow<AutoFixConfig>("自动修理").Show();
	}
	
	private void OnGUI() {
		AutoFix.INTERVAL = EditorGUILayout.IntSlider("监测间隔", AutoFix.INTERVAL, 5, 20);
		GUILayout.Space(5F);
		if (AutoFix.IsRunning) {
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