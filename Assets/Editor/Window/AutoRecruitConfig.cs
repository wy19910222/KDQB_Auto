/*
 * @Author: wangyun
 * @CreateTime: 2024-03-23 03:25:46 708
 * @LastEditor: wangyun
 * @EditTime: 2024-03-23 03:25:46 713
 */

using UnityEngine;
using UnityEditor;

public class AutoRecruitConfig : PrefsEditorWindow<AutoRecruit> {
	[MenuItem("Tools_Window/Default/AutoRecruit", false, 22)]
	private static void Open() {
		GetWindow<AutoRecruitConfig>("自动招募").Show();
	}
	
	private void OnGUI() {
		if (AutoRecruit.IsRunning) {
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