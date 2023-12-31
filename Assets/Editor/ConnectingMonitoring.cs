﻿/*
 * @Author: wangyun
 * @CreateTime: 2023-11-05 19:03:52 124
 * @LastEditor: wangyun
 * @EditTime: 2023-11-05 19:03:52 128
 */

using System.Collections;
using UnityEngine;
using UnityEditor;

public class ConnectingMonitoringConfig : PrefsEditorWindow<ConnectingMonitoring> {
	[MenuItem("Window/Default/ConnectingMonitoring", false, -100)]
	private static void Open() {
		GetWindow<ConnectingMonitoringConfig>("网络状态监测").Show();
	}
	
	private void OnGUI() {
		ConnectingMonitoring.INTERVAL = EditorGUILayout.IntSlider("监测间隔", ConnectingMonitoring.INTERVAL, 60, 3600);
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

public class ConnectingMonitoring {
	public static int INTERVAL = 60;	// 监测间隔
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartConnectingMonitoring", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"网络状态监测已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopConnectingMonitoring", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("网络状态监测已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			while (Recognize.IsNetworkDisconnected) {
				Operation.Click(960, 630);	// 确定按钮
				yield return new EditorWaitForSeconds(0.5F);
			}
			while (Recognize.IsMigrateInviting) {
				Operation.Click(1140, 408);	// 关闭按钮
				yield return new EditorWaitForSeconds(0.5F);
			}
			yield return new EditorWaitForSeconds(INTERVAL);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
