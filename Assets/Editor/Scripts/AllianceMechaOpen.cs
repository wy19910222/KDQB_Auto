﻿/*
 * @Author: wangyun
 * @CreateTime: 2023-10-24 04:22:33 341
 * @LastEditor: wangyun
 * @EditTime: 2023-10-24 04:22:33 346
 */

using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

public class AllianceMechaOpen {
	public static TimeSpan DAILY_TIME = new TimeSpan(10, 0, 0);	// 开启时间
	public static int MECHA_INDEX = 0;	// 机甲序号
	public static int MECHA_LEVEL = 1;	// 机甲等级
	public static int DONATE_COUNT = 3;	// 捐献数量
	
	public static DateTime s_OpenTime;	// 上次开启时间
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartAllianceMechaOpen", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"联盟机甲开启已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopAllianceMechaOpen", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("联盟机甲开启已关闭");
		}
	}

	private static IEnumerator Update() {
		// bool prevIsMarshalTime = false;
		while (true) {
			yield return null;
			
			bool started = s_OpenTime > (DateTime.Now - DAILY_TIME).Date;
			if (started) {
				continue;
			}
			
			if (Recognize.CurrentScene == Recognize.Scene.FIGHTING) {
				Debug.Log("处于出战界面，不执行操作");
				continue;
			}
			if (Recognize.IsWindowCovered) {
				Debug.Log("有窗口覆盖，不执行操作");
				continue;
			}
			
			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(AllianceMechaOpen);
			
			// bool succeed = false;
			Operation.Click(1870, 710);	// 联盟按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(830, 620);	// 联盟活动按钮
			yield return new EditorWaitForSeconds(0.2F);
			int index = Array.IndexOf(Recognize.AllianceActivityTypes, Recognize.AllianceActivityType.MECHA);
			if (index != -1) {
				// 开启
				Operation.Click(960, 300 + 269 * index);	// 联盟机甲
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(937 + Mathf.RoundToInt(16.5F * MECHA_INDEX), 413);	// 机甲序号
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(772 + 63 * MECHA_INDEX, 483);	// 机甲序号
				yield return new EditorWaitForSeconds(0.2F);
				if (Recognize.IsAllianceMechaOpenEnabled) {
					Operation.Click(960, 960);	// 开启按钮
					yield return new EditorWaitForSeconds(0.3F);
					Operation.Click(960, 700);	// 确定按钮
					yield return new EditorWaitForSeconds(0.3F);
					// succeed = true;
				
					// 捐献
					for (int i = 0; i < DONATE_COUNT; ++i) {
						Operation.Click(960, 960);	// 捐献按钮
						yield return new EditorWaitForSeconds(0.3F);
						if (Recognize.IsAllianceMechaDonateConfirming) {
							Operation.Click(960, 686);	// 兑换按钮
							yield return new EditorWaitForSeconds(0.2F);
							Operation.Click(1167, 353);	// 关闭按钮
							yield return new EditorWaitForSeconds(0.2F);
						}
					}
				}
			}
			for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.1F);
			}

			// if (succeed) {
				// 无论成功失败都只尝试一次
				s_OpenTime = DateTime.Now;
			// }
			
			Task.CurrentTask = null;
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
