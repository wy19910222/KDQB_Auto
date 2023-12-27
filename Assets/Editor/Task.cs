/*
 * @Author: wangyun
 * @CreateTime: 2023-12-27 11:44:03 310
 * @LastEditor: wangyun
 * @EditTime: 2023-12-27 11:44:03 327
 */

using UnityEditor;
using UnityEngine;

public static class Task {
	public enum TaskType {
		IDLE,
		FOLLOW,
		JUNGLE,
		GATHER,
		ATTACK_MARSHAL,
		OTHER,
	}
	public static TaskType Type { get; set; }

	[MenuItem("Tools_Log/LogTaskType", priority = -1)]
	private static void LogType() {
		Debug.LogError(Type);
	}
}