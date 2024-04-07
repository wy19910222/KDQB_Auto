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
	public static int FIGHTING_BLOCK_SECONDS = 30;	// 判定异常阈值（秒）
	
	private static EditorCoroutine s_FightingBlockCO;
	public static bool FightingBlockIsRunning => s_FightingBlockCO != null;

	[MenuItem("Tools_Task/StartFightingBlockBreak", priority = -1)]
	private static void StartFightingBlockBreak() {
		StopFightingBlockBreak();
		Debug.Log($"战斗异常阻塞处理已开启");
		s_FightingBlockCO = EditorCoroutineManager.StartCoroutine(FightingBlockUpdate());
	}

	[MenuItem("Tools_Task/StopFightingBlockBreak", priority = -1)]
	private static void StopFightingBlockBreak() {
		if (s_FightingBlockCO != null) {
			EditorCoroutineManager.StopCoroutine(s_FightingBlockCO);
			s_FightingBlockCO = null;
			Debug.Log("战斗异常阻塞处理已关闭");
		}
	}

	private static IEnumerator FightingBlockUpdate() {
		DateTime dt = DateTime.Now;
		float windowCoveredCount = -1;
		while (true) {
			yield return null;
			
			// 非无人值守状态不处理
			if (!GlobalStatus.IsUnattended) {
				continue;
			}
			
			if (Recognize.CurrentScene != Recognize.Scene.FIGHTING || Recognize.IsFightingPlayback) {
				dt = DateTime.Now;
			} else {
				float newWindowCoveredCount = Recognize.WindowCoveredCount;
				if (!Mathf.Approximately(newWindowCoveredCount, windowCoveredCount)) {
					windowCoveredCount = newWindowCoveredCount;
					dt = DateTime.Now;
				}
				if ((DateTime.Now - dt).TotalSeconds > FIGHTING_BLOCK_SECONDS) {
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
	
	
	public static int WINDOW_BLOCK_SECONDS = 30;	// 判定异常阈值（秒）
	
	private static EditorCoroutine s_WindowBlockCO;
	public static bool WindowBlockIsRunning => s_WindowBlockCO != null;

	[MenuItem("Tools_Task/StartWindowBlockBreak", priority = -1)]
	private static void StartWindowBlockBreak() {
		StopWindowBlockBreak();
		Debug.Log($"窗口异常阻塞处理已开启");
		s_WindowBlockCO = EditorCoroutineManager.StartCoroutine(WindowBlockUpdate());
	}

	[MenuItem("Tools_Task/StopWindowBlockBreak", priority = -1)]
	private static void StopWindowBlockBreak() {
		if (s_WindowBlockCO != null) {
			EditorCoroutineManager.StopCoroutine(s_WindowBlockCO);
			s_WindowBlockCO = null;
			Debug.Log("窗口异常阻塞处理已关闭");
		}
	}

	private static IEnumerator WindowBlockUpdate() {
		DateTime dt = DateTime.Now;
		float windowCoveredCount = -1;
		while (true) {
			yield return null;
			
			// 非无人值守状态不处理
			if (!GlobalStatus.IsUnattended) {
				continue;
			}
			
			float newWindowCoveredCount = Recognize.WindowCoveredCount;
			if (Mathf.Approximately(newWindowCoveredCount, windowCoveredCount)) {
				dt = DateTime.Now;
				windowCoveredCount = newWindowCoveredCount;
			} else if (windowCoveredCount <= 0) {
				dt = DateTime.Now;
			} else if ((DateTime.Now - dt).TotalSeconds > WINDOW_BLOCK_SECONDS) {
				for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
					Debug.Log("关闭窗口");
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.2F);
				}
				dt = DateTime.Now;
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
