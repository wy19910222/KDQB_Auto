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
	
	private const int EXPIRE_SECONDS = 60;
	private static string s_CurrentTask;
	private static EditorCoroutine s_CO;
	public static string CurrentTask {
		get {
			string message = s_SharedMemory.GetString();
			string[] parts = message?.Split('|');
			if (parts?.Length > 1) {
				string currentTask = parts[0];
				if (currentTask == string.Empty) {
					currentTask = null;
				}
				s_CurrentTask = currentTask;
				if (long.TryParse(parts[1], out long ticks)) {
					if (DateTime.Now.Ticks - ticks > (EXPIRE_SECONDS + 1) * 10000000L) {
						currentTask = null;
					}
				}
				return currentTask;
			} else {
				return s_CurrentTask = null;
			}
		}
		set {
			s_SharedMemory.SetString($"{value}|{DateTime.Now.Ticks}");
			if (s_CO != null) {
				EditorCoroutineManager.StopCoroutine(s_CO);
				s_CO = null;
			}
			if (value != null) {
				s_CO = EditorCoroutineUtil.Once(null, EXPIRE_SECONDS, () => {
					s_SharedMemory.SetString($"{null}|{DateTime.Now.Ticks}");
					s_CO = null;
				});
			}
		}
	}
	
	public static void ResetExpire() {
		// 先判断CurrentTask是为了更新可能变了的s_CurrentTask
		if (CurrentTask != null || s_CurrentTask != null) {
			CurrentTask = s_CurrentTask;
		}
	}
}