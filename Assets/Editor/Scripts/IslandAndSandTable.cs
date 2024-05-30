﻿/*
 * @Author: wangyun
 * @CreateTime: 2024-05-30 19:12:41 178
 * @LastEditor: wangyun
 * @EditTime: 2024-05-30 19:12:41 185
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class IslandAndSandTable {
	public static bool Test { get; set; } // 测试模式
	
	public static int SAND_TABLE_ORDER = 4;
	public static int ISLAND_ORDER = 10;
	public static TimeSpan DAILY_TIME = new TimeSpan(1, 0, 0);	// 每天几点执行
	
	public static DateTime LAST_SAND_TABLE_TIME;
	public static DateTime LAST_ISLAND_TIME;
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;
	
	private const int ITEM_HEIGHT = 187;
	private const int OFFSET_Y_MAX = 1061;
	private const int VISIBLE_ITEMS_COUNT = 4;

	[MenuItem("Tools_Task/StartIslandAndSandTable", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"岛屿/沙盘自动挑战已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopIslandAndSandTable", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("岛屿/沙盘自动挑战已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			DateTime now = DateTime.Now;
			DateTime date = now.Date;
			
			bool test = Test;
			bool sandTableSucceed = LAST_SAND_TABLE_TIME > date;
			bool islandSucceed = LAST_ISLAND_TIME > date;
			if ((sandTableSucceed && islandSucceed || DateTime.Now.TimeOfDay < DAILY_TIME) && !test) {
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
			Task.CurrentTask = nameof(IslandAndSandTable);

			// 如果是世界界面远景，则没有每日军情按钮，需要先切换到近景
			for (int i = 0; i < 50 && Recognize.CurrentScene == Recognize.Scene.OUTSIDE_FARAWAY; i++) {
				// 鼠标移动到屏幕中央并滚动滚轮
				Operation.SetMousePosTemporarily(960, 540, () => {
					MouseUtils.ScrollWheel(1);
				});
				yield return new EditorWaitForSeconds(0.1F);
			}
			Debug.Log("每日军情按钮");
			Operation.Click(733, 867);	// 活动按钮
			yield return new EditorWaitForSeconds(0.2F);

			int offsetY = 0;
			if (!sandTableSucceed) {
				Debug.Log("拖动以显示沙盘演习");
				int orderOffsetY = Mathf.Clamp((SAND_TABLE_ORDER - VISIBLE_ITEMS_COUNT) * ITEM_HEIGHT, 0, OFFSET_Y_MAX);
				int deltaOffsetY = orderOffsetY - offsetY;
				while (deltaOffsetY > 0) {
					int dragDistance = Mathf.Min(ITEM_HEIGHT * VISIBLE_ITEMS_COUNT, deltaOffsetY);
					// 往左拖动
					var ie = Operation.NoInertiaDrag(960, 960, 960, 960 - dragDistance, 0.5F);
					while (ie.MoveNext()) {
						yield return ie.Current;
					}
					yield return new EditorWaitForSeconds(0.1F);
					deltaOffsetY -= dragDistance;
				}
				offsetY = orderOffsetY;
				yield return new EditorWaitForSeconds(0.2F);
				
				Debug.Log("前往按钮");
				Operation.Click(1092, 320 + (SAND_TABLE_ORDER - 1) * ITEM_HEIGHT - offsetY);	// 前往按钮
				yield return new EditorWaitForSeconds(0.5F);
				if (Recognize.DailyIntelligenceCurrentType == Recognize.DailyIntelligenceType.SAND_TABLE && !test) {
					LAST_SAND_TABLE_TIME = DateTime.Now;
				}
				Debug.Log("左上角返回按钮");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.2F);
			}

			if (!islandSucceed) {
				Debug.Log("拖动以显示岛屿作战");
				int orderOffsetY = Mathf.Clamp((ISLAND_ORDER - VISIBLE_ITEMS_COUNT) * ITEM_HEIGHT, 0, OFFSET_Y_MAX);
				int deltaOffsetY = orderOffsetY - offsetY;
				while (deltaOffsetY > 0) {
					int dragDistance = Mathf.Min(ITEM_HEIGHT * VISIBLE_ITEMS_COUNT, deltaOffsetY);
					// 往左拖动
					var ie = Operation.NoInertiaDrag(960, 960, 960, 960 - dragDistance, 1F);
					while (ie.MoveNext()) {
						yield return ie.Current;
					}
					yield return new EditorWaitForSeconds(0.1F);
					deltaOffsetY -= dragDistance;
				}
				offsetY = orderOffsetY;
				yield return new EditorWaitForSeconds(0.2F);
				
				Debug.Log("前往按钮");
				Operation.Click(1092, 320 + (ISLAND_ORDER - 1) * ITEM_HEIGHT - offsetY);	// 前往按钮
				yield return new EditorWaitForSeconds(0.5F);
				if (Recognize.DailyIntelligenceCurrentType == Recognize.DailyIntelligenceType.ISLAND && !test) {
					for (int i = 0; i < 10; i++) {
						if (Recognize.IsIslandExitBtn) {
							Debug.Log("岛屿作战已完成");
							LAST_ISLAND_TIME = DateTime.Now;
							break;
						}
						if (Recognize.IsIslandResetBtn) {
							Debug.Log("重置按钮");
							Operation.Click(1073, 631);	// 重置按钮
							yield return new EditorWaitForSeconds(1F);
						}
						if (Recognize.IsIslandMopUpIconBtn) {
							Debug.Log("扫荡图标按钮");
							Operation.Click(742, 946);	// 扫荡图标按钮
							yield return new EditorWaitForSeconds(0.5F);
							if (Recognize.IsIslandMopUpBtn) {
								Debug.Log("扫荡按钮");
								Operation.Click(960, 849);	// 扫荡按钮
								yield return new EditorWaitForSeconds(0.5F);
								if (Recognize.IsIslandConfirmBtn) {
									Debug.Log("确定按钮");
									Operation.Click(960, 867);	// 确定按钮
									yield return new EditorWaitForSeconds(1F);
								}
							}
						}
					}
				}
				Debug.Log("左上角返回按钮");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			
			for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			
			Task.CurrentTask = null;
			
			yield return new EditorWaitForSeconds(30F);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
