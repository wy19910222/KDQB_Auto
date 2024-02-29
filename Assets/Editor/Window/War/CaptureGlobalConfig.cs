/*
 * @Author: wangyun
 * @CreateTime: 2023-10-06 12:16:55 107
 * @LastEditor: wangyun
 * @EditTime: 2023-10-06 12:16:55 112
 */

using UnityEngine;
using UnityEditor;

public class CaptureGlobalConfig : PrefsEditorWindow<CaptureGlobal> {
	[MenuItem("Tools_Window/War/CaptureGlobal")]
	private static void Open() {
		GetWindow<CaptureGlobalConfig>("全区截图").Show();
	}
	
	private void OnGUI() {
		if (CaptureGlobal.IsRunning) {
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
