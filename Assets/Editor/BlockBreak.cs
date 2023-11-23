/*
 * @Author: wangyun
 * @CreateTime: 2023-11-05 19:03:52 124
 * @LastEditor: wangyun
 * @EditTime: 2023-11-05 19:03:52 128
 */

using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

public class BlockBreakConfig : PrefsEditorWindow<BlockBreak> {
	[MenuItem("Window/BlockBreak")]
	private static void Open() {
		GetWindow<BlockBreakConfig>("异常阻塞处理").Show();
	}
	
	private void OnGUI() {
		EditorGUILayout.HelpBox(EditorGUIUtility.TrTempContent("停留在士兵选择界面超过一定时间，则认为处于异常阻塞状态"));
		BlockBreak.SECONDS = EditorGUILayout.IntSlider("判定异常阈值（秒）", BlockBreak.SECONDS, 20, 60);
		GUILayout.Space(5F);
		if (BlockBreak.IsRunning) {
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

public class BlockBreak {
	public static int SECONDS = 30;	// 判定异常阈值（秒）
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartBlockBreak", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"异常阻塞处理已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopBlockBreak", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("异常阻塞处理已关闭");
		}
	}

	private static IEnumerator Update() {
		DateTime dt = DateTime.Now;
		while (true) {
			yield return null;
			if (Recognize.CurrentScene != Recognize.Scene.ARMY_SELECTING || Recognize.IsFightingPlayback) {
				dt = DateTime.Now;
			} else if ((DateTime.Now - dt).TotalSeconds > SECONDS) {
				Debug.Log("退出按钮");
				Operation.Click(30, 140);	// 退出按钮
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("确认退出按钮");
				Operation.Click(1064, 634);	// 确认退出按钮
				yield return new EditorWaitForSeconds(0.2F);
				dt = DateTime.Now;
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
