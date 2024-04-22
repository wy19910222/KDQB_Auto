/*
 * @Author: wangyun
 * @CreateTime: 2024-03-30 18:08:03 162
 * @LastEditor: wangyun
 * @EditTime: 2024-03-30 18:08:03 166
 */

using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

public class AllianceMechaOpen {
	public static bool Test { get; set; } // 测试模式
	
	public static TimeSpan DAILY_TIME = new TimeSpan(10, 0, 0);	// 开启时间
	public static Recognize.AllianceMechaType MECHA_TYPE = 0;	// 机甲序号
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
			if (MECHA_TYPE == Recognize.AllianceMechaType.UNKNOWN) {
				Debug.Log("未选择机甲类型");
				continue;
			}
			
			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(AllianceMechaOpen);
			
			bool test = Test;
			// bool succeed = false;
			Debug.Log("联盟按钮");
			Operation.Click(1870, 710);	// 联盟按钮
			yield return new EditorWaitForSeconds(0.2F);
			Debug.Log("联盟活动按钮");
			Operation.Click(830, 620);	// 联盟活动按钮
			yield return new EditorWaitForSeconds(0.2F);
			int index = Array.IndexOf(Recognize.AllianceActivityTypes, Recognize.AllianceActivityType.MECHA);
			if (index != -1) {
				// 开启
				Debug.Log("联盟机甲");
				Operation.Click(960, 300 + 269 * index);	// 联盟机甲
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("机甲序号");
				Operation.Click(Mathf.RoundToInt(920.3F + 16.5F * (int) MECHA_TYPE), 413);	// 机甲序号
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("机甲等级");
				Operation.Click(772 + 63 * MECHA_LEVEL, 483);	// 机甲等级
				yield return new EditorWaitForSeconds(0.2F);
				if (Recognize.AllianceMechaStatus == Recognize.AllianceMechaState.CAN_OPEN) {
					Debug.Log("开启按钮");
					Operation.Click(960, 960);	// 开启按钮
					yield return new EditorWaitForSeconds(0.3F);
					if (!test) {
						Debug.Log("确定按钮");
						Operation.Click(960, 700);	// 确定按钮
						yield return new EditorWaitForSeconds(0.3F);
						// succeed = true;
				
						// 捐献
						for (int i = 0; i < DONATE_COUNT; ++i) {
							Debug.Log("捐献按钮");
							Operation.Click(960, 960);	// 捐献按钮
							yield return new EditorWaitForSeconds(0.3F);
							if (Recognize.IsAllianceMechaDonateConfirming) {
								Debug.Log("兑换按钮");
								Operation.Click(960, 686);	// 兑换按钮
								yield return new EditorWaitForSeconds(0.2F);
								Debug.Log("关闭按钮");
								Operation.Click(1167, 353);	// 关闭按钮
								yield return new EditorWaitForSeconds(0.2F);
							}
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

	[MenuItem("Tools_Task/TestAllianceMechaOpen", priority = -1)]
	private static void ExecuteTest() {
		Debug.Log($"测试");
		EditorCoroutineManager.StartCoroutine(IEExecuteTest());
	}
	private static IEnumerator IEExecuteTest() {
		if (Task.CurrentTask != null) {
			Debug.LogError($"正在执行【{Task.CurrentTask}】, 请稍后！");
			yield break;
		}
		DateTime prevNextTime = s_OpenTime;
		s_OpenTime = default;
		do {
			yield return null;
		} while (Task.CurrentTask != null);
		s_OpenTime = prevNextTime;
	}
}
