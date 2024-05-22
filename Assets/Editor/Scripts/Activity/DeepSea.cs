/*
 * @Author: wangyun
 * @CreateTime: 2023-10-12 00:00:51 751
 * @LastEditor: wangyun
 * @EditTime: 2023-10-12 00:00:51 756
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DeepSea {
	public static int ACTIVITY_ORDER = 8;	// 活动排序
	public static int ORDER_RADIUS = 5;	// 寻找标签半径
	public static int DETECTOR_COUNT = 3;	// 拥有探测器数量
	public static TimeSpan DEFAULT_COUNTDOWN = new TimeSpan(8, 0, 0);	// 倒计时
	public static int TRY_COUNT = 8;	// 尝试点击次数
	public static List<DateTime> TargetDTs = new List<DateTime>();
	public static List<TimeSpan> Countdowns {
		get => TargetDTs.ConvertAll(targetDT => targetDT - DateTime.Now);
		set => TargetDTs = value?.ConvertAll(countdown => DateTime.Now + countdown);
	}
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartDeepSea", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"深海寻宝自动点击已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopDeepSea", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("深海寻宝自动点击已关闭");
		}
	}

	private static IEnumerator Update() {
		List<int> nearbyOrders = new List<int>();
		while (true) {
			yield return null;
			if (DateTime.Now < TargetDTs[0]) {
				continue;
			}
			// 有窗口打开着
			if (Recognize.IsWindowCovered) {
				continue;
			}
			// 不是有活动入口的场景也不是世界远景
			if (!Recognize.IsOutsideOrInsideScene) {
				continue;
			}

			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(DeepSea);

			int activityOrder = ACTIVITY_ORDER;
			if (nearbyOrders.Count > 0) {
				activityOrder = nearbyOrders[0];
				nearbyOrders.RemoveAt(0);
			}
			// 如果是世界界面远景，则没有显示活动按钮，需要先切换到近景
			for (int i = 0; i < 50 && Recognize.CurrentScene == Recognize.Scene.OUTSIDE_FARAWAY; i++) {
				Vector2Int oldPos = MouseUtils.GetMousePos();
				MouseUtils.SetMousePos(960, 540);	// 鼠标移动到屏幕中央
				MouseUtils.ScrollWheel(1);
				MouseUtils.SetMousePos(oldPos.x, oldPos.y);
				yield return new EditorWaitForSeconds(0.1F);
			}
			Debug.Log("活动按钮");
			Operation.Click(1880, Recognize.CurrentScene == Recognize.Scene.INSIDE ? 280 : 290);	// 活动按钮
			// Operation.Click(1880, Recognize.CurrentScene == Recognize.Scene.INSIDE ? 280 : 350);	// 活动按钮（被新世界图标挤到下一格了）
			yield return new EditorWaitForSeconds(0.5F);
			Debug.Log("拖动以显示活动标签页");
			const int TAB_WIDTH = 137;
			int orderOffsetX = (activityOrder - 4) * TAB_WIDTH;
			while (orderOffsetX > 0) {
				const int dragDistance = TAB_WIDTH * 4;
				// 往左拖动
				var ie = Operation.NoInertiaDrag(1190, 200, 1190 - dragDistance, 200, 0.5F);
				while (ie.MoveNext()) {
					yield return ie.Current;
				}
				yield return new EditorWaitForSeconds(0.1F);
				orderOffsetX -= dragDistance;
			}
			Debug.Log("活动标签页");
			Operation.Click(1190 + orderOffsetX, 200);	// 活动标签页
			yield return new EditorWaitForSeconds(0.5F);

			if (Recognize.IsDeepSea) {
				Debug.Log("点击探测器");
				for (int i = 0; i < DETECTOR_COUNT; ++i) {
					Debug.Log($"点击探测器{i + 1}");
					for (int tryCount = 0; tryCount < TRY_COUNT; ++tryCount) {
						Operation.Click(808 + 153 * i, 870);	// 探测器
						yield return new EditorWaitForSeconds(0.2F);
					}
				}
				// 成功，更新倒计时
				if (nearbyOrders.Count > 0) {
					nearbyOrders.Clear();
				}
				ACTIVITY_ORDER = activityOrder;
				TargetDTs.RemoveAt(0);
				TargetDTs.Add(DateTime.Now + DEFAULT_COUNTDOWN);
			} else {
				// 失败，更新倒计时
				if (GlobalStatus.IsUnattended) {
					// 无人值守状态，尝试相邻标签页
					if (activityOrder == ACTIVITY_ORDER) {
						for (int i = 1; i <= ORDER_RADIUS; i++) {
							int prevOrder = ACTIVITY_ORDER - i;
							if (prevOrder > 0) {
								nearbyOrders.Add(prevOrder);
							}
							nearbyOrders.Add(ACTIVITY_ORDER + i);
						}
						Debug.Log($"标签{activityOrder}错误，稍后尝试相邻标签页: {string.Join(",", nearbyOrders)}");
						TargetDTs[0] = DateTime.Now + new TimeSpan(0, 0, 2);
					} else {
						if (nearbyOrders.Count > 0) {
							Debug.Log($"标签{activityOrder}错误，稍后继续尝试标签页: " + nearbyOrders[0]);
							TargetDTs[0] = DateTime.Now + new TimeSpan(0, 0, 2);
						} else {
							Debug.LogError($"标签{activityOrder}错误，取消操作");
							TargetDTs.RemoveAt(0);
							TargetDTs.Add(DateTime.Now + DEFAULT_COUNTDOWN);
						}
					}
				} else {
					// 非无人值守状态，取消操作
					Debug.LogError($"非无人值守状态，取消操作");
					TargetDTs.RemoveAt(0);
					TargetDTs.Add(DateTime.Now + DEFAULT_COUNTDOWN);
					nearbyOrders.Clear();
				}
			}
				
			for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			
			Task.CurrentTask = null;
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
