﻿/*
 * @Author: wangyun
 * @CreateTime: 2023-10-24 04:22:33 341
 * @LastEditor: wangyun
 * @EditTime: 2023-10-24 04:22:33 346
 */

using System.Collections;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class LeagueMechaDonateConfig : PrefsEditorWindow<LeagueMechaDonate> {
	[MenuItem("Window/LeagueMechaDonate")]
	private static void Open() {
		GetWindow<LeagueMechaDonateConfig>("联盟机甲捐献").Show();
	}
	
	private void OnGUI() {
		LeagueMechaDonate.INTERVAL = EditorGUILayout.Slider("尝试捐献间隔", LeagueMechaDonate.INTERVAL, 120F, 1200F);
		GUILayout.Space(5F);
		if (LeagueMechaDonate.IsRunning) {
			if (GUILayout.Button("关闭")) {
				EditorApplication.ExecuteMenuItem("Assets/StopLeagueMechaDonate");
			}
		} else {
			if (GUILayout.Button("开启")) {
				EditorApplication.ExecuteMenuItem("Assets/StartLeagueMechaDonate");
			}
		}
	}
}

public class LeagueMechaDonate {
	public static float INTERVAL = 0.1F;	// 点击间隔
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartLeagueMechaDonate", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"联盟机甲捐献尝试已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopLeagueMechaDonate", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("联盟机甲捐献尝试已关闭");
		}
	}

	private static IEnumerator Update() {
		// bool prevIsMarshalTime = false;
		while (true) {
			yield return null;
			
			if (Recognize.CurrentScene == Recognize.Scene.ARMY_SELECTING) {
				Debug.Log("处于出战界面，不执行操作");
				continue;
			}
			if (Recognize.IsWindowCovered) {
				Debug.Log("有窗口覆盖，不执行操作");
				continue;
			}
			
			Operation.Click(1870, 710);	// 联盟按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(830, 620);	// 联盟活动按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(960, 300);	// 联盟机甲
			yield return new EditorWaitForSeconds(0.2F);
			if (Recognize.IsLeagueMechaDonateEnabled) {
				Operation.Click(960, 960);	// 免费捐献按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			while (Recognize.IsWindowCovered) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(735, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.1F);
			}
			yield return new EditorWaitForSeconds(INTERVAL);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}