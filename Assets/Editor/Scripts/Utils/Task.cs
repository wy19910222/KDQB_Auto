/*
 * @Author: wangyun
 * @CreateTime: 2023-12-27 11:44:03 310
 * @LastEditor: wangyun
 * @EditTime: 2023-12-27 11:44:03 327
 */

using System;
using System.IO.MemoryMappedFiles;
using System.Threading;
using UnityEditor;
using UnityEngine;

public static class Task {
	[MenuItem("Tools_Log/LogCurrentTask", priority = -1)]
	private static void LogCurrentTask() {
		Debug.LogError(CurrentTask);
	}
	
	private static EditorCoroutine s_CO;
	private static string s_OldTask;
	private static DateTime s_OldTaskDT;
	public static string CurrentTask {
		get {
			string currentTask = GetCurrentTask();
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
			SetCurrentTask(value ?? string.Empty);
			if (s_CO != null) {
				EditorCoroutineManager.StopCoroutine(s_CO);
				s_CO = null;
			}
			s_CO = EditorCoroutineUtil.Once(null, 60, () => {
				SetCurrentTask(string.Empty);
				s_CO = null;
			});
		}
	}

	private const int SHARED_TOTAL_BYTES = 1024;
	private static readonly MemoryMappedFile s_MMF = MemoryMappedFile.CreateOrOpen("MemoryMappedFile_KDQB_Task", SHARED_TOTAL_BYTES);
	private static readonly Mutex s_Mutex = new Mutex(false, "Mutex_KDQB_Task");
	private static void SetCurrentTask(string currentTask) {
		if (s_Mutex.WaitOne()) {
			byte[] buffer = new byte[SHARED_TOTAL_BYTES];
			System.Text.Encoding.UTF8.GetBytes(currentTask, 0, currentTask.Length, buffer, 0);
			using (MemoryMappedViewAccessor writer = s_MMF.CreateViewAccessor(0, SHARED_TOTAL_BYTES)) {
				writer.WriteArray(0, buffer, 0, buffer.Length);
			}
			s_Mutex.ReleaseMutex();
		} else {
			Debug.LogError("获取锁失败！");
		}
	}
	private static string GetCurrentTask() {
		if (s_Mutex.WaitOne()) {
			byte[] buffer = new byte[SHARED_TOTAL_BYTES];
			using (MemoryMappedViewAccessor reader = s_MMF.CreateViewAccessor(0, SHARED_TOTAL_BYTES)) {
				reader.ReadArray(0, buffer, 0, buffer.Length);
			}
			s_Mutex.ReleaseMutex();
			return System.Text.Encoding.UTF8.GetString(buffer).Trim('\0');
		} else {
			Debug.LogError("获取锁失败！");
			return null;
		}
	}
}