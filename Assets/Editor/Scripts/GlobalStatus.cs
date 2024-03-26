/*
 * @Author: wangyun
 * @CreateTime: 2023-12-28 12:59:39 966
 * @LastEditor: wangyun
 * @EditTime: 2023-12-28 12:59:39 971
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public static class GlobalStatus {
	public static bool IsUnattended => UnattendedDuration > UNATTENDED_THRESHOLD;
	public static long UnattendedDuration { get; private set; }
	public const long UNATTENDED_THRESHOLD = 30 * 1000_000_0; // 30秒

	private static EditorCoroutine s_CO;
	
	public static void Enable() {
		Disable();
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
		Debug.Log($"全局状态监测已开启");
	}

	public static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("全局状态监测已关闭");
		}
	}

	private static IEnumerator Update() {
		Vector2Int prevMousePos = MouseUtils.GetMousePos();
		DateTime startDT = DateTime.Now;
		while (true) {
			yield return null;
			Vector2Int nextMousePos = MouseUtils.GetMousePos();
			DateTime now = DateTime.Now;
			if (nextMousePos.x != prevMousePos.x || nextMousePos.y != prevMousePos.y) {
				prevMousePos = nextMousePos;
				startDT = now;
				UnattendedDuration = 0;
			} else {
				UnattendedDuration = (now - startDT).Ticks;
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
}