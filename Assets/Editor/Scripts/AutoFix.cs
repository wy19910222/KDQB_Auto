﻿/*
 * @Author: wangyun
 * @CreateTime: 2023-11-05 19:03:52 124
 * @LastEditor: wangyun
 * @EditTime: 2023-11-05 19:03:52 128
 */

using System.Collections;
using UnityEngine;
using UnityEditor;

public class AutoFix {
	public static int INTERVAL = 5;	// 监测间隔
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartAutoFix", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"自动修理已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopAutoFix", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动修理已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			int isQuickFixExist = Recognize.IsQuickFixExist;
			if (isQuickFixExist > 0) {
				if (Task.CurrentTask != null) {
					continue;
				}
				Task.CurrentTask = nameof(AutoFix);
				
				Debug.Log("修理按钮");
				switch (isQuickFixExist) {
					case 1:
						Operation.Click(792, 800);	// 修理按钮按钮
						break;
					case 2:
						Operation.Click(850, 800);	// 修理按钮按钮
						break;
					case 3:
						Operation.Click(918, 800);	// 修理按钮按钮
						break;
				}
				yield return new EditorWaitForSeconds(0.2F);
				if (Recognize.CanFixAll) {
					Debug.Log("全体修理按钮");
					Operation.Click(930, 960);	// 全体修理按钮
					yield return new EditorWaitForSeconds(0.2F);
				}
				for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
					Debug.Log("关闭窗口");
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.2F);
				}
				
				Task.CurrentTask = null;
			}
			
			yield return new EditorWaitForSeconds(INTERVAL);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
