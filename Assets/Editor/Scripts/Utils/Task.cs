/*
 * @Author: wangyun
 * @CreateTime: 2023-12-27 11:44:03 310
 * @LastEditor: wangyun
 * @EditTime: 2023-12-27 11:44:03 327
 */

using System;
using UnityEditor;
using UnityEngine;

public static class Task {
	[MenuItem("Tools_Log/LogCurrentTask", priority = -1)]
	private static void LogCurrentTask() {
		Debug.LogError(CurrentTask);
	}
	
	private const int SHARED_TOTAL_BYTES = 128;
	private static readonly SharedMemory s_SharedMemory = new SharedMemory("KDQB_Task", SHARED_TOTAL_BYTES);
	
	private static string s_OldTask;
	private static DateTime s_OldTaskDT;
	private static EditorCoroutine s_CO;
	public static string CurrentTask {
		get {
			string currentTask = s_SharedMemory.GetString();
			if (string.IsNullOrEmpty(currentTask)) {
				currentTask = null;
			}
			if (currentTask != s_OldTask) {
				s_OldTaskDT = DateTime.Now;
				s_OldTask = currentTask;
			} else if (currentTask != null && DateTime.Now - s_OldTaskDT > TimeSpan.FromSeconds(60)) {
				currentTask = null;
			}
			return currentTask;
		}
		set {
			s_SharedMemory.SetString(value);
			if (s_CO != null) {
				EditorCoroutineManager.StopCoroutine(s_CO);
				s_CO = null;
			}
			s_CO = EditorCoroutineUtil.Once(null, 60, () => {
				s_SharedMemory.SetString(null);
				s_CO = null;
			});
		}
	}
	
	public static void ResetExpire() {
		s_OldTaskDT = DateTime.Now;
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = EditorCoroutineUtil.Once(null, 60, () => {
				s_SharedMemory.SetString(null);
				s_CO = null;
			});
		}
	}
}