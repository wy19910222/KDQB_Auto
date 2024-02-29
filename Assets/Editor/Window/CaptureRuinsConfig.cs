/*
 * @Author: wangyun
 * @CreateTime: 2023-10-06 12:16:55 107
 * @LastEditor: wangyun
 * @EditTime: 2023-10-06 12:16:55 112
 */

using UnityEngine;
using UnityEditor;

public class CaptureRuinsConfig : EditorWindow {
	[MenuItem("Tools_Window/Default/CaptureRuins", false, 29)]
	private static void Open() {
		GetWindow<CaptureRuinsConfig>("王者遗迹截图").Show();
	}
	
	private void OnGUI() {
		if (CaptureRuins.IsRunning) {
			if (GUILayout.Button("取消截图")) {
				EditorApplication.ExecuteMenuItem("Tools_Task/StopCaptureRuins");
			}
		} else {
			if (GUILayout.Button("开始截图")) {
				EditorApplication.ExecuteMenuItem("Tools_Task/StartCaptureRuins");
			}
		}
	}
}