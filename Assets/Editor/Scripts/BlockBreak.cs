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

public class BlockBreak {
	public static int SECONDS = 30;	// 判定异常阈值（秒）
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartBlockBreak", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"异常阻塞处理已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopBlockBreak", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("异常阻塞处理已关闭");
		}
	}

	private static IEnumerator Update() {
		DateTime dt = DateTime.Now;
		float windowCoveredCount = -1;
		while (true) {
			yield return null;
			if (Recognize.CurrentScene != Recognize.Scene.FIGHTING || Recognize.IsFightingPlayback) {
				dt = DateTime.Now;
			} else {
				float newWindowCoveredCount = Recognize.WindowCoveredCount;
				if (!Mathf.Approximately(newWindowCoveredCount, windowCoveredCount)) {
					windowCoveredCount = newWindowCoveredCount;
					dt = DateTime.Now;
				}
				if ((DateTime.Now - dt).TotalSeconds > SECONDS) {
					for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {
						Debug.Log("点外部关闭弹窗");
						Operation.Click(30, 140);	// 点外部关闭弹窗
						yield return new EditorWaitForSeconds(0.2F);
					}
					Debug.Log("退出按钮");
					Operation.Click(30, 140);	// 退出按钮
					yield return new EditorWaitForSeconds(0.3F);
					if (Recognize.IsFightingAborting) {
						Debug.Log("确认退出按钮");
						Operation.Click(1064, 634);	// 确认退出按钮
						yield return new EditorWaitForSeconds(0.2F);
					}
					dt = DateTime.Now;
				}
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
