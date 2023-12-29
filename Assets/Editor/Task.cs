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
			m_CurrentTask = value;
			if (s_CO != null) {
				EditorCoroutineManager.StopCoroutine(s_CO);
				s_CO = null;
			}
			s_CO = EditorCoroutineUtil.Once(null, 60, () => {
				m_CurrentTask = null;
				s_CO = null;
			});
		}
	}
	
	private static EditorCoroutine s_CO;

	[MenuItem("Tools_Log/LogCurrentTask", priority = -1)]
	private static void LogCurrentTask() {
		Debug.LogError(CurrentTask);
	}
}