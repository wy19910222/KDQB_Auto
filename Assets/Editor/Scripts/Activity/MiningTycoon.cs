/*
 * @Author: wangyun
 * @CreateTime: 2023-11-03 14:55:53 057
 * @LastEditor: wangyun
 * @EditTime: 2023-11-03 14:55:53 065
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MiningTycoon {
	public static int ACTIVITY_ORDER = 7;	// 活动排序
	public static int ORDER_RADIUS = 5;	// 寻找标签半径
	public static bool SMART_COLLECT;	// 智能收取
	public static int TRAMCAR_COUNTDOWN_NUMBER = 3;	// 收取矿车编号
	public static DateTime NEAREST_DT = DateTime.Now;	// 下一次收取时间
	public static DateTime ACTIVITY_END_DT = DateTime.Now;	// 活动结束时间（不包括额外的一天）
	public static int CLICK_INTERVAL = 1800;	// 失败点击间隔（秒）
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartMiningTycoon", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"矿产大亨自动点击已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopMiningTycoon", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("矿产大亨自动点击已关闭");
		}
	}

	private static IEnumerator Update() {
		List<int> nearbyOrders = new List<int>();
		while (true) {
			yield return null;
			if (DateTime.Now < NEAREST_DT) {
				continue;
			}
			
			// 有窗口打开着
			if (Recognize.IsWindowCovered) {
				continue;
			}

			// 战斗场景
			if (Recognize.CurrentScene is Recognize.Scene.UNKNOWN or Recognize.Scene.FIGHTING) {
				continue;
			}

			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(MiningTycoon);

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
			yield return new EditorWaitForSeconds(0.1F);

			if (Recognize.IsMiningTycoon) {
				Debug.Log($"尝试领取矿车奖励");
				for (int i = 4; i > 0; --i) {
					Operation.Click(660 + 120 * i, 850);	// 点击矿车
					yield return new EditorWaitForSeconds(0.2F);
					Operation.Click(960, 730);	// 领取奖励按钮
					Operation.Click(660, 850);	// 点击窗口外关闭
					yield return new EditorWaitForSeconds(0.2F);
				}

				Debug.Log("挖矿");
				for (int i = 0; i < 20; ++i) {
					Operation.Click(1060, 970);	// 挖矿按钮
					yield return new EditorWaitForSeconds(0.2F);
				}

				List<int> truckTypes = Recognize.GetMiningTruckTypes();
				int targetIndex = TRAMCAR_COUNTDOWN_NUMBER - 1;
				if (DateTime.Now > ACTIVITY_END_DT) {
					// 活动结束，挨个收取
					targetIndex = 0;
				} else if (SMART_COLLECT) {
					// 智能收取，-1 > 1 > 24 > 4/8（前3车凑成1个4+2个8）
					targetIndex = truckTypes.IndexOf(-1);
					if (targetIndex == -1) {
						targetIndex = truckTypes.IndexOf(1);
					}
					if (targetIndex == -1) {
						targetIndex = truckTypes.IndexOf(24);
					}
					if (targetIndex == -1) {
						List<int> type4In3Indexes = new List<int>();
						for (int i = 0, length = 3; i < length; ++i) {
							if (truckTypes[i] == 4) {
								type4In3Indexes.Add(i);
							}
						}

						int type4In3Count = type4In3Indexes.Count;
						targetIndex = type4In3Count switch {
							<= 0 => 2,
							1 => 3,
							_ => type4In3Indexes[type4In3Count - 1]
						};
					}
				}
				Debug.Log($"开始获取第{targetIndex + 1}个矿车");
				Operation.Click(780 + 120 * targetIndex, 850);	// 点击矿车
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(830, 730);	// 开始收取按钮
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(660, 850);	// 点击窗口外关闭
				yield return new EditorWaitForSeconds(0.2F);
				
				// 成功，更新倒计时
				if (nearbyOrders.Count > 0) {
					nearbyOrders.Clear();
				}
				ACTIVITY_ORDER = activityOrder;
				NEAREST_DT = DateTime.Now + new TimeSpan(Mathf.Abs(truckTypes[targetIndex]), 0, 0);
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
						NEAREST_DT = DateTime.Now + new TimeSpan(0, 0, 2);
					} else {
						if (nearbyOrders.Count > 0) {
							Debug.Log($"标签{activityOrder}错误，稍后继续尝试标签页: " + nearbyOrders[0]);
							NEAREST_DT = DateTime.Now + new TimeSpan(0, 0, 2);
						} else {
							Debug.LogError($"标签{activityOrder}错误，取消操作");
							NEAREST_DT = DateTime.Now + new TimeSpan(0, 0, CLICK_INTERVAL);
						}
					}
				} else {
					// 非无人值守状态，取消操作
					Debug.LogError($"非无人值守状态，取消操作");
					NEAREST_DT = DateTime.Now + new TimeSpan(0, 0, CLICK_INTERVAL);
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
