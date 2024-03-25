/*
 * @Author: wangyun
 * @CreateTime: 2023-10-06 12:16:55 107
 * @LastEditor: wangyun
 * @EditTime: 2023-10-06 12:16:55 112
 */

using UnityEngine;
using UnityEditor;

public class CaptureRuinsConfig : PrefsEditorWindow<CaptureRuins> {
	[MenuItem("Tools_Window/Default/CaptureRuins", false, 29)]
	private static void Open() {
		GetWindow<CaptureRuinsConfig>("王者遗迹截图").Show();
	}
	
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		CaptureRuins.CAPTURE_NORTH = GUILayout.Toggle(CaptureRuins.CAPTURE_NORTH, "北边遗迹", "Button");
		CaptureRuins.CAPTURE_SOUTH = GUILayout.Toggle(CaptureRuins.CAPTURE_SOUTH, "南边遗迹", "Button");
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space(5F);
		
		if (CaptureRuins.IsRunning) {
			if (GUILayout.Button("取消截图")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开始截图")) {
				IsRunning = true;
			}
		}
	}
}