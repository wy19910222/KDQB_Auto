/*
 * @Author: wangyun
 * @CreateTime: 2023-10-24 04:22:33 341
 * @LastEditor: wangyun
 * @EditTime: 2023-10-24 04:22:33 346
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AllianceMechaDonate {
	public static int DEFAULT_DONATE_COUNT = 3;	// 捐献数量
	public static int INTERVAL = 300;	// 点击间隔
	public static DateTime NEXT_TIME = DateTime.Now;
	
	public static readonly Dictionary<Recognize.AllianceMechaType, bool> VALID_DICT = new Dictionary<Recognize.AllianceMechaType, bool>();
	public static readonly Dictionary<Recognize.AllianceMechaType, DateTime> FIXED_TIME_DICT = new Dictionary<Recognize.AllianceMechaType, DateTime>();
	public static readonly Dictionary<Recognize.AllianceMechaType, int[]> DONATE_COUNTS_DICT = new Dictionary<Recognize.AllianceMechaType, int[]>();

	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartAllianceMechaDonate", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"联盟机甲捐献尝试已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopAllianceMechaDonate", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("联盟机甲捐献尝试已关闭");
		}
	}

	private static IEnumerator Update() {
		// bool prevIsMarshalTime = false;
		while (true) {
			yield return null;
			
			if (!IsAnyMechaFixed()) {
				if (DateTime.Now < NEXT_TIME) {
					NEXT_TIME = DateTime.Now;
				}
				// Debug.Log("所有机甲都在修理");
				continue;
			}
			if (DateTime.Now < NEXT_TIME) {
				// Debug.Log("等待重试中");
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
			Task.CurrentTask = nameof(AllianceMechaDonate);
			
			Operation.Click(1870, 710);	// 联盟按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(830, 620);	// 联盟活动按钮
			yield return new EditorWaitForSeconds(0.2F);
			int index = Array.IndexOf(Recognize.AllianceActivityTypes, Recognize.AllianceActivityType.MECHA);
			if (index != -1) {
				Operation.Click(960, 300 + 269 * index);	// 联盟机甲
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(1170, 200);	// 排行奖励按钮
				yield return new EditorWaitForSeconds(0.5F);
				if (Recognize.IsAllianceMechaDonateRankShowing) {
					// 有弹出奖励排行窗口，说明已开启捐献
					bool isInRank = Recognize.IsAllianceMechaDonateInRank;
					Operation.Click(720, 128);	// 点击窗口外关闭窗口
					yield return new EditorWaitForSeconds(0.1F);
					Recognize.AllianceMechaType mechaType = Recognize.CurrentMechaType;
					if (!isInRank) {
						int mechaLevel = Recognize.CurrentMechaLevel;
						int donateCount = DEFAULT_DONATE_COUNT;
						if (DONATE_COUNTS_DICT.TryGetValue(mechaType, out int[] counts)) {
							int count = counts[mechaLevel - 1];
							if (count > 0) {
								donateCount = count;
							}
						}
						for (int i = 0; i < donateCount; ++i) {
							Operation.Click(960, 960);	// 捐献按钮
							yield return new EditorWaitForSeconds(0.3F);
							if (Recognize.IsAllianceMechaDonateConfirming) {
								Operation.Click(960, 686);	// 兑换按钮
								yield return new EditorWaitForSeconds(0.2F);
								Operation.Click(1167, 353);	// 关闭按钮
								yield return new EditorWaitForSeconds(0.2F);
							}
						}

						FIXED_TIME_DICT[mechaType] = mechaType switch {
							Recognize.AllianceMechaType.ALPHA => DateTime.Now + new TimeSpan(12 + 1, 0, 0),
							Recognize.AllianceMechaType.GAMMA => DateTime.Now + new TimeSpan(2, 1, 0, 0),
							Recognize.AllianceMechaType.DELTA => DateTime.Now + new TimeSpan(4, 1, 0, 0),
							Recognize.AllianceMechaType.EPSILON => DateTime.Now + new TimeSpan(7, 1, 0, 0),
							_ => DateTime.Now + new TimeSpan(12 + 1, 0, 0)
						};
					} else {
						FIXED_TIME_DICT[mechaType] = DateTime.Now + new TimeSpan(12 + 1, 0, 0);
					}
				}
			}
			for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.1F);
			}
			
			Task.CurrentTask = null;

			NEXT_TIME = DateTime.Now + new TimeSpan(0, 0, INTERVAL);
		}
		// ReSharper disable once IteratorNeverReturns
	}

	public static bool IsAnyMechaFixed() {
		if (FIXED_TIME_DICT.Count <= 0) {
			return true;
		}
		DateTime now = DateTime.Now;
		foreach (var (type, time) in FIXED_TIME_DICT) {
			if (VALID_DICT.TryGetValue(type, out bool valid) && valid && now > time) {
				return true;
			}
		}
		return false;
	}

	[MenuItem("Tools_Task/OnceAllianceMechaDonate", priority = -1)]
	private static void ExecuteOnce() {
		Debug.Log($"单次执行联盟机甲捐献尝试");
		EditorCoroutineManager.StartCoroutine(IEExecuteOnce());
	}
	private static IEnumerator IEExecuteOnce() {
		if (Task.CurrentTask != null) {
			Debug.LogError($"正在执行【{Task.CurrentTask}】, 请稍后！");
			yield break;
		}
		DateTime prevNextTime = NEXT_TIME;
		NEXT_TIME = DateTime.Now;
		do {
			yield return null;
		} while (Task.CurrentTask != null);
		NEXT_TIME = prevNextTime;
	}
}
