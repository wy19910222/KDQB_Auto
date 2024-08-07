/*
 * @Author: wangyun
 * @CreateTime: 2023-11-03 14:55:53 057
 * @LastEditor: wangyun
 * @EditTime: 2023-11-03 14:55:53 065
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MiningTycoon {
	public static int ACTIVITY_ORDER = 7;	// 活动排序
	public static int ORDER_RETRY_INTERVAL = 1800;	// 失败点击间隔（秒）
	public const int ORDER_TRY_RADIUS = 5; // 寻找标签半径
	public const int ORDER_TRY_MAX = 20; // 最大标签数
	
	public static bool SMART_COLLECT;	// 智能收取
	public static int TRAMCAR_COUNTDOWN_NUMBER = 3;	// 收取矿车编号
	public static DateTime NEAREST_DT = DateTime.Now;	// 下一次收取时间
	public static DateTime ACTIVITY_END_DT = DateTime.Now;	// 活动结束时间（不包括额外的一天）
	
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
		int orderTryRadius = ORDER_TRY_RADIUS;
		while (true) {
			yield return null;
			if (DateTime.Now < NEAREST_DT) {
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
			Task.CurrentTask = nameof(MiningTycoon);
			
			// 如果是世界界面远景，则没有显示活动按钮，需要先切换到近景
			for (int i = 0; i < 50 && Recognize.CurrentScene == Recognize.Scene.OUTSIDE_FARAWAY; i++) {
				// 鼠标移动到屏幕中央并滚动滚轮
				Operation.SetMousePosTemporarily(960, 540, () => {
					MouseUtils.ScrollWheel(1);
				});
				yield return new EditorWaitForSeconds(0.1F);
			}
			Debug.Log("活动按钮");
			Operation.Click(1880, Recognize.CurrentScene == Recognize.Scene.INSIDE ? 280 : 290);	// 活动按钮
			// Operation.Click(1880, Recognize.CurrentScene == Recognize.Scene.INSIDE ? 280 : 350);	// 活动按钮（被新世界图标挤到下一格了）
			yield return new EditorWaitForSeconds(0.5F);

			Debug.Log("拖动以显示活动标签页");
			List<int> nearbyOrders = new List<int>() { ACTIVITY_ORDER };
			for (int i = -orderTryRadius; i <= orderTryRadius; i++) {
				if (i != 0) {
					int order = ACTIVITY_ORDER + i;
					if (order > 0) {
						nearbyOrders.Add(ACTIVITY_ORDER + i);
					}
				}
			}
			const int TAB_WIDTH = 137;
			const int VISIBLE_ITEMS_COUNT = 4;
			int offsetX = 0;
			for (int orderIndex = 0, orderCount = nearbyOrders.Count; orderIndex < orderCount; orderIndex++) {
				if (Recognize.FullWindowTitle == "超值活动") {
					int order = nearbyOrders[orderIndex];
					int orderOffsetX = Mathf.Max((order - VISIBLE_ITEMS_COUNT) * TAB_WIDTH, 0);
					int deltaOffsetX = orderOffsetX - offsetX;
					if (deltaOffsetX > 0) {
						while (deltaOffsetX > 0) {
							int dragDistance = Mathf.Min(TAB_WIDTH * VISIBLE_ITEMS_COUNT, deltaOffsetX);
							// 往左拖动
							var ie = Operation.NoInertiaDrag(1190, 190, 1190 - dragDistance, 190, 0.5F);
							while (ie.MoveNext()) {
								yield return ie.Current;
							}
							yield return new EditorWaitForSeconds(0.1F);
							deltaOffsetX -= dragDistance;
						}
					} else {
						deltaOffsetX = -deltaOffsetX;
						while (deltaOffsetX > 0) {
							int dragDistance = Mathf.Min(TAB_WIDTH * VISIBLE_ITEMS_COUNT, deltaOffsetX);
							// 往右拖动
							var ie = Operation.NoInertiaDrag(750, 190, 750 + dragDistance, 190, 0.5F);
							while (ie.MoveNext()) {
								yield return ie.Current;
							}
							yield return new EditorWaitForSeconds(0.1F);
							deltaOffsetX -= dragDistance;
						}
					}
					offsetX = orderOffsetX;

					Debug.Log($"活动标签页{order}");
					Operation.Click(750 + (order - 1) * TAB_WIDTH - offsetX, 190);	// 活动标签页
					yield return new EditorWaitForSeconds(0.5F);

					if (Recognize.IsMiningTycoon) {
						Debug.Log($"尝试领取矿车奖励");
						for (int i = 4; i > 0; --i) {
							Operation.Click(660 + 120 * i, 850);	// 点击矿车
							yield return new EditorWaitForSeconds(0.2F);
							Operation.Click(960, 730);	// 领取奖励按钮
							Operation.Click(660, 850);	// 点击窗口外关闭
							yield return new EditorWaitForSeconds(0.2F);
						}

						if (DateTime.Now <= ACTIVITY_END_DT) {
							Debug.Log("挖矿");
							for (int i = 0; i < 20; ++i) {
								Operation.Click(1060, 970); // 挖矿按钮
								yield return new EditorWaitForSeconds(0.2F);
							}
							yield return new EditorWaitForSeconds(1F);
						}

						List<int> truckTypes = Recognize.GetMiningTruckTypes();
						Debug.Log("矿车类型：" + string.Join(",", truckTypes));
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
						NEAREST_DT = DateTime.Now + new TimeSpan(Mathf.Max(truckTypes[targetIndex], 1), 0, 0);
						if (order != ACTIVITY_ORDER) {
							ACTIVITY_ORDER = order;
							Debug.Log($"活动排序改为：{order}");
						}
						orderTryRadius = ORDER_TRY_RADIUS;
						break;
					}

					if (orderIndex == 0) {
						Debug.Log($"标签{order}错误，尝试相邻标签页: {string.Join(",", nearbyOrders.Skip(1))}");
					} else if (orderIndex < orderCount - 1) {
						Debug.Log($"标签{order}错误，继续尝试标签页: " + nearbyOrders[orderIndex + 1]);
					} else {
						Debug.LogError($"标签{string.Join(",", nearbyOrders)}全部错误，稍后重新尝试更大范围");
						if (nearbyOrders[1] <= 1 && order >= ORDER_TRY_MAX) {
							orderTryRadius = ORDER_TRY_RADIUS;
						} else {
							orderTryRadius += ORDER_TRY_RADIUS;
						}
						NEAREST_DT = DateTime.Now + new TimeSpan(0, 0, ORDER_RETRY_INTERVAL);
					}
				} else {
					Debug.LogWarning($"窗口未打开或异常关闭，稍后重试。");
					NEAREST_DT = DateTime.Now + new TimeSpan(0, 0, ORDER_RETRY_INTERVAL);
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
