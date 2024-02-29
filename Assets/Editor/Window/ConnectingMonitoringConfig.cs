/*
 * @Author: wangyun
 * @CreateTime: 2023-11-05 19:03:52 124
 * @LastEditor: wangyun
 * @EditTime: 2023-11-05 19:03:52 128
 */

using UnityEngine;
using UnityEditor;

public class ConnectingMonitoringConfig : PrefsEditorWindow<ConnectingMonitoring> {
	[MenuItem("Tools_Window/Default/ConnectingMonitoring", false, -100)]
	private static void Open() {
		GetWindow<ConnectingMonitoringConfig>("网络状态监测").Show();
	}
	
	private void OnGUI() {
		ConnectingMonitoring.INTERVAL = EditorGUILayout.IntSlider("监测间隔", ConnectingMonitoring.INTERVAL, 5, 3600);
		GUILayout.Space(5F);
		if (ConnectingMonitoring.IsRunning) {
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