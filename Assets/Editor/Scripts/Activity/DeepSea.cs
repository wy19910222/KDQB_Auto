/*
 * @Author: wangyun
 * @CreateTime: 2023-10-12 00:00:51 751
 * @LastEditor: wangyun
 * @EditTime: 2023-10-12 00:00:51 756
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class DeepSea {
	public static int ACTIVITY_ORDER = 8;	// 活动排序
	public static int ORDER_RETRY_INTERVAL = 1800;	// 寻找标签重试间隔（秒）
	public const int ORDER_TRY_RADIUS = 5; // 寻找标签半径
	public const int ORDER_TRY_MAX = 20; // 最大标签数
	
	public static int DETECTOR_COUNT = 3;	// 拥有探测器数量
	public static TimeSpan DEFAULT_COUNTDOWN = new TimeSpan(8, 0, 0);	// 倒计时
	public static int TRY_COUNT = 8;	// 尝试点击次数
	public static readonly List<DateTime> TargetDTs = new List<DateTime>();
	
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
		int orderTryRadius = ORDER_TRY_RADIUS;
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

			// 如果是世界界面远景，则没有显示活动按钮，需要先切换到近景
			for (int i = 0; i < 50 && Recognize.CurrentScene == Recognize.Scene.OUTSIDE_FARAWAY; i++) {
				// 鼠标移动到屏幕中央并滚动滚轮
				Operation.SetMousePosTemporarily(960, 540, () => {
					MouseUtils.ScrollWheel(1);
				});
				yield return new EditorWaitForSeconds(0.1F);
			}
			Debug.Log("活动按钮");
			Operation.Click(1880, Global.ACTIVITY_BTN_Y);	// 活动按钮
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
					Operation.Click(750 + (order - 1) * TAB_WIDTH - offsetX, 190); // 活动标签页
					yield return new EditorWaitForSeconds(0.5F);

					if (Recognize.IsDeepSea) {
						Debug.Log("点击探测器");
						for (int i = 0; i < DETECTOR_COUNT; ++i) {
							Debug.Log($"点击探测器{i + 1}");
							for (int tryCount = 0; tryCount < TRY_COUNT; ++tryCount) {
								Operation.Click(808 + 153 * i, 870); // 探测器
								yield return new EditorWaitForSeconds(0.2F);
							}
						}
						// 成功，更新倒计时并放至列表末尾
						TargetDTs.RemoveAt(0);
						TargetDTs.Add(DateTime.Now + DEFAULT_COUNTDOWN);
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
						TargetDTs.RemoveAt(0);
						TargetDTs.Add(DateTime.Now + new TimeSpan(0, 0, ORDER_RETRY_INTERVAL));
					}
				} else {
					Debug.LogWarning($"窗口未打开或异常关闭，稍后重试。");
					TargetDTs[0] = DateTime.Now + new TimeSpan(0, 0, ORDER_RETRY_INTERVAL);
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
