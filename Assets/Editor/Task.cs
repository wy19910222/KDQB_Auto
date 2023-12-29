/*
 * @Author: wangyun
 * @CreateTime: 2023-12-27 11:44:03 310
 * @LastEditor: wangyun
 * @EditTime: 2023-12-27 11:44:03 327
 */

using UnityEditor;
using UnityEngine;

public static class Task {
	public static string m_CurrentTask;
	public static string CurrentTask {
		get => m_CurrentTask;
		set {
			Debug.LogWarning(value);
			m_CurrentTask = value;
		}
	}

	[MenuItem("Tools_Log/LogCurrentTask", priority = -1)]
	private static void LogCurrentTask() {
		Debug.LogError(CurrentTask);
	}
}