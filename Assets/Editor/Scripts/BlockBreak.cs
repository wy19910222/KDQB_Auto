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
	public static int FIGHTING_BLOCK_SECONDS = 30;	// 战斗阻塞异常阈值（秒）
	public static int WINDOW_BLOCK_SECONDS = 30;	// 窗口阻塞异常阈值（秒）
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartBlockBreak", priority = -1)]
	private static void StartBlockBreak() {
		StopBlockBreak();
		Debug.Log($"异常阻塞处理已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopBlockBreak", priority = -1)]
	private static void StopBlockBreak() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("异常阻塞处理已关闭");
		}
	}

	private static IEnumerator Update() {
		DateTime fightingBlockDT = DateTime.Now;
		DateTime windowBlockDT = DateTime.Now;
		float windowCoveredCount = -1;
		while (true) {
			yield return null;
			
			// 非无人值守状态不处理
			if (GlobalStatus.UnattendedDuration < 3 * 1000_000_0) {
				windowBlockDT = DateTime.Now;
				fightingBlockDT = DateTime.Now;
				continue;
			}
			
			if (Recognize.CurrentScene == Recognize.Scene.FIGHTING) {
				// 战斗场景，只判断战斗阻塞
				if ((DateTime.Now - fightingBlockDT).TotalSeconds > FIGHTING_BLOCK_SECONDS) {
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
					fightingBlockDT = DateTime.Now;
				}
			} else {
				// 重置战斗阻塞时间
				fightingBlockDT = DateTime.Now;
				
				// 非战斗场景，只判断窗口阻塞
				float newWindowCoveredCount = Recognize.WindowCoveredCount;
				if (Mathf.Approximately(newWindowCoveredCount, windowCoveredCount)) {
					// 窗口层数变化，重置窗口阻塞时间
					windowBlockDT = DateTime.Now;
					windowCoveredCount = newWindowCoveredCount;
				} else if (newWindowCoveredCount <= 0) {
					// 无窗口，重置窗口阻塞时间
					windowBlockDT = DateTime.Now;
				} else if ((DateTime.Now - windowBlockDT).TotalSeconds > WINDOW_BLOCK_SECONDS) {
					for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
						Debug.Log("关闭窗口");
						Operation.Click(720, 128);	// 左上角返回按钮
						yield return new EditorWaitForSeconds(0.2F);
					}
					windowBlockDT = DateTime.Now;
					windowCoveredCount = 0;
				}
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
