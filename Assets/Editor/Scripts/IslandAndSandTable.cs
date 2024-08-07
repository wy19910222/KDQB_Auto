/*
 * @Author: wangyun
 * @CreateTime: 2024-05-30 19:12:41 178
 * @LastEditor: wangyun
 * @EditTime: 2024-05-30 19:12:41 185
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class IslandAndSandTable {
	public static bool Test { get; set; } // 测试模式
	
	public static int SAND_TABLE_ORDER = 4;
	public static int SAND_TABLE_TAB = 1;	// 1-陆军，2-海军，3-空军
	public static int SAND_SQUAD_NUMBER = 1;	// 沙盘使用编队号码
	public static bool SAND_MUST_FULL_SOLDIER = true;	// 必须满兵
	
	public static int EXPEDITION_ORDER = 5;
	public static bool EXPEDITION_QUICK_BY_50_DIAMOND = true;
	
	public static int TRANSNATIONAL_ORDER = 6;
	public static List<int> TRANSNATIONAL_TARGET_WEIGHTS = new List<int>() { 0, 0, 0, 0, 1 };	// 跨战区目标随机权重
	public static int TRANSNATIONAL_SQUAD_NUMBER = 1;	// 跨战区使用编队号码
	public static bool TRANSNATIONAL_MUST_FULL_SOLDIER = true;	// 必须满兵
	
	public static int ISLAND_ORDER = 10;
	
	public static TimeSpan DAILY_TIME = new TimeSpan(1, 0, 0);	// 每天几点执行
	
	public static DateTime LAST_SAND_TABLE_TIME;
	public static DateTime LAST_EXPEDITION_TIME;
	public static DateTime LAST_ISLAND_TIME;
	public static DateTime LAST_TRANSNATIONAL_TIME;
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;
	
	private const int ITEM_HEIGHT = 187;
	private const int OFFSET_Y = -61;
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
			bool expeditionSucceed = LAST_EXPEDITION_TIME > date;
			bool transnationalSucceed = LAST_TRANSNATIONAL_TIME > date;
			bool islandSucceed = LAST_ISLAND_TIME > date;
			if ((sandTableSucceed && expeditionSucceed && transnationalSucceed && islandSucceed || DateTime.Now.TimeOfDay < DAILY_TIME) && !test) {
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
			Task.ResetExpire();	// 任务持续太久，刷新任务过期事件，避免任务过期
			if (!sandTableSucceed && Recognize.FullWindowTitle == "每日军情") {
				Debug.Log("拖动以显示沙盘演习");
				int orderOffsetY = Mathf.Max((SAND_TABLE_ORDER - VISIBLE_ITEMS_COUNT) * ITEM_HEIGHT + OFFSET_Y, 0);
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
				yield return new EditorWaitForSeconds(1F);
				if (Recognize.DailyIntelligenceCurrentType == Recognize.DailyIntelligenceType.SAND_TABLE && !test) {
					yield return new EditorWaitForSeconds(0.5F);
					Debug.Log("军种标签");
					Operation.Click(660 + SAND_TABLE_TAB * 150, 190);	// 军种标签
					yield return new EditorWaitForSeconds(0.2F);
					Debug.Log("挑战按钮");
					Operation.Click(960, 960);	// 挑战按钮
					yield return new EditorWaitForSeconds(0.3F);
					if (Recognize.CurrentScene == Recognize.Scene.FIGHTING_MARCH) {
						Debug.Log("选择编队" + SAND_SQUAD_NUMBER);
						Operation.Click(1145 + 37 * SAND_SQUAD_NUMBER, 870);	// 选择编队
						yield return new EditorWaitForSeconds(0.2F);
						// if (!Recognize.IsSandTableUsingPhalanx) {
						// 	Debug.Log("军阵按钮");
						// 	Operation.Click(52, 515);	// 军阵按钮
						// 	yield return new EditorWaitForSeconds(0.3F);
						// 	Debug.Log("狂鲨军阵");
						// 	Operation.Click(780, 710);	// 狂鲨军阵
						// 	yield return new EditorWaitForSeconds(0.2F);
						// 	Debug.Log("应用按钮");
						// 	Operation.Click(960, 820);	// 应用按钮
						// 	yield return new EditorWaitForSeconds(0.3F);
						// }
						if (!SAND_MUST_FULL_SOLDIER || Recognize.FightingSoldierCountPercent > 0.99F) {
							Debug.Log("战斗按钮");
							Operation.Click(960, 476);	// 战斗按钮
							yield return new EditorWaitForSeconds(2F);
							for (int i = 0; i < 10 && Recognize.CurrentScene is Recognize.Scene.FIGHTING or Recognize.Scene.FIGHTING_PLAYBACK; i++) {
								Task.ResetExpire();	// 演习任务持续太久，刷新任务过期事件，避免任务过期
								Debug.Log("跳过按钮");
								Operation.Click(30, 250);	// 跳过按钮
								yield return new EditorWaitForSeconds(2F);
								Debug.Log("再次挑战");
								Operation.Click(910, 910);	// 再次挑战
								yield return new EditorWaitForSeconds(2F);
								Debug.LogError(Recognize.CurrentScene);
								// if (Recognize.IsSandTableAgainBtn) {
								// } else {
								// 	Debug.Log("返回按钮");
								// 	Operation.Click(990, 910);	// 返回按钮
								// 	break;
								// }
							}
						} else {
							Debug.Log("退出按钮");
							Operation.Click(30, 140);	// 退出按钮
							yield return new EditorWaitForSeconds(0.2F);
							Debug.Log("确认退出按钮");
							Operation.Click(1064, 634);	// 确认退出按钮
							yield return new EditorWaitForSeconds(2);
						}
					}
					LAST_SAND_TABLE_TIME = DateTime.Now;
				}
				Debug.Log("左上角返回按钮");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			
			Task.ResetExpire();	// 任务持续太久，刷新任务过期事件，避免任务过期
			if (!expeditionSucceed && Recognize.FullWindowTitle == "每日军情") {
				Debug.Log("拖动以显示远征行动");
				int orderOffsetY = Mathf.Max((EXPEDITION_ORDER - VISIBLE_ITEMS_COUNT) * ITEM_HEIGHT + OFFSET_Y, 0);
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
				Operation.Click(1092, 320 + (EXPEDITION_ORDER - 1) * ITEM_HEIGHT - offsetY);	// 前往按钮
				yield return new EditorWaitForSeconds(1F);
				if (Recognize.DailyIntelligenceCurrentType == Recognize.DailyIntelligenceType.EXPEDITION && !test) {
					yield return new EditorWaitForSeconds(0.5F);
					for (int i = 0; i < 5 && Recognize.IsExpeditionQuickBtn; i++) {
						Debug.Log("快速战斗按钮");
						Operation.Click(1135, 953);	// 快速战斗按钮
						yield return new EditorWaitForSeconds(0.3F);
						if (Recognize.IsExpeditionQuickFreeBtn) {
							Debug.Log("快速战斗按钮");
							Operation.Click(960, 740);	// 快速战斗按钮
							yield return new EditorWaitForSeconds(0.3F);
							Debug.Log("跳过动画按钮");
							Operation.Click(1135, 888);	// 跳过动画按钮
							yield return new EditorWaitForSeconds(0.5F);
							bool isSucceeded = false;
							for (int j = 0; j < 10 && Recognize.IsExpeditionGetBtn; j++) {
								Debug.Log("领取按钮");
								Operation.Click(960, 900);	// 领取按钮
								yield return new EditorWaitForSeconds(0.3F);
								isSucceeded = true;
							}
							if (isSucceeded && !EXPEDITION_QUICK_BY_50_DIAMOND) {
								LAST_EXPEDITION_TIME = DateTime.Now;
								break;
							}
						} else if (Recognize.IsExpeditionQuickBy50DiamondBtn && EXPEDITION_QUICK_BY_50_DIAMOND) {
							Debug.Log("快速战斗按钮");
							Operation.Click(960, 740);	// 快速战斗按钮
							yield return new EditorWaitForSeconds(0.5F);
							if (Recognize.IsExpeditionQuickConfirmBtn) {
								Debug.Log("确定按钮");
								Operation.Click(960, 700);	// 确定按钮
								yield return new EditorWaitForSeconds(0.5F);
								Debug.Log("跳过动画按钮");
								Operation.Click(1135, 888);	// 跳过动画按钮
								yield return new EditorWaitForSeconds(0.5F);
								bool isSucceeded = false;
								for (int j = 0; j < 10 && Recognize.IsExpeditionGetBtn; j++) {
									Debug.Log("领取按钮");
									Operation.Click(960, 900);	// 领取按钮
									yield return new EditorWaitForSeconds(0.3F);
									isSucceeded = true;
								}
								if (isSucceeded) {
									LAST_EXPEDITION_TIME = DateTime.Now;
									break;
								}
							}
						} else {
							Debug.Log("关闭按钮");
							Operation.Click(1169, 397);	// 关闭按钮
							yield return new EditorWaitForSeconds(0.3F);
						}
					}
				}
				Debug.Log("左上角返回按钮");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			
			Task.ResetExpire();	// 任务持续太久，刷新任务过期事件，避免任务过期
			if (!transnationalSucceed && Recognize.FullWindowTitle == "每日军情") {
				Debug.Log("拖动以显示跨战区演习");
				int orderOffsetY = Mathf.Max((TRANSNATIONAL_ORDER - VISIBLE_ITEMS_COUNT) * ITEM_HEIGHT + OFFSET_Y, 0);
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
				Operation.Click(1092, 320 + (TRANSNATIONAL_ORDER - 1) * ITEM_HEIGHT - offsetY);	// 前往按钮
				yield return new EditorWaitForSeconds(1F);
				if (Recognize.DailyIntelligenceCurrentType == Recognize.DailyIntelligenceType.TRANSNATIONAL && !test) {
					yield return new EditorWaitForSeconds(0.5F);
					for (int i = 0; i < 10 && !Recognize.IsTransnationalTargetList; i++) {
						Debug.Log("挑战按钮");
						Operation.Click(960, 960);	// 挑战按钮
						yield return new EditorWaitForSeconds(0.2F);
					}
					for (int i = 0; i < 20 && !Recognize.IsTransnationalTimesEmpty; i++) {
						Task.ResetExpire();	// 演习任务持续太久，刷新任务过期事件，避免任务过期
						Debug.Log("选择目标");
						int targetIndex = RandomTransnationalTarget();
						Operation.Click(1110, 420 + 100 * targetIndex);	// 选择目标
						yield return new EditorWaitForSeconds(0.3F);
						if (Recognize.CurrentScene == Recognize.Scene.FIGHTING_MARCH) {
							Debug.Log("选择编队" + TRANSNATIONAL_SQUAD_NUMBER);
							Operation.Click(1145 + 37 * TRANSNATIONAL_SQUAD_NUMBER, 870);	// 选择编队
							yield return new EditorWaitForSeconds(0.2F);
							if (!TRANSNATIONAL_MUST_FULL_SOLDIER || Recognize.FightingSoldierCountPercent > 0.99F) {
								Debug.Log("战斗按钮");
								Operation.Click(960, 476);	// 战斗按钮
								yield return new EditorWaitForSeconds(2F);
								Debug.Log("跳过按钮");
								Operation.Click(30, 250);	// 跳过按钮
								yield return new EditorWaitForSeconds(2F);
								Debug.Log("返回按钮");
								Operation.Click(960, 906);	// 返回按钮
								yield return new EditorWaitForSeconds(0.5F);
							} else {
								Debug.Log("退出按钮");
								Operation.Click(30, 140);	// 退出按钮
								yield return new EditorWaitForSeconds(0.2F);
								Debug.Log("确认退出按钮");
								Operation.Click(1064, 634);	// 确认退出按钮
								yield return new EditorWaitForSeconds(2);
							}
						} else {
							yield return new EditorWaitForSeconds(0.3F);
							Debug.Log("右上角叉叉");
							Operation.Click(1168, 352);	// 右上角叉叉
							yield return new EditorWaitForSeconds(0.3F);
						}
					}
					LAST_TRANSNATIONAL_TIME = DateTime.Now;
				}
				Debug.Log("右上角叉叉");
				Operation.Click(1170, 248);	// 右上角叉叉
				yield return new EditorWaitForSeconds(0.3F);
				Debug.Log("左上角返回按钮");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.2F);
			}

			Task.ResetExpire();	// 任务持续太久，刷新任务过期事件，避免任务过期
			if (!islandSucceed && Recognize.FullWindowTitle == "每日军情") {
				Debug.Log("拖动以显示岛屿作战");
				int orderOffsetY = Mathf.Max((ISLAND_ORDER - VISIBLE_ITEMS_COUNT) * ITEM_HEIGHT + OFFSET_Y, 0);
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
				yield return new EditorWaitForSeconds(1F);
				if (Recognize.DailyIntelligenceCurrentType == Recognize.DailyIntelligenceType.ISLAND && !test) {
					yield return new EditorWaitForSeconds(0.5F);
					for (int i = 0; i < 10; i++) {
						if (Recognize.IsIslandExitBtn) {
							Debug.Log("岛屿作战已完成");
							LAST_ISLAND_TIME = DateTime.Now;
							break;
						}
						if (Recognize.IsIslandResetBtn) {
							Debug.Log("重置按钮");
							Operation.Click(1073, 631);	// 重置按钮
							yield return new EditorWaitForSeconds(1.5F);
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
									yield return new EditorWaitForSeconds(1.5F);
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
			
			yield return new EditorWaitForSeconds(5F);
		}
		// ReSharper disable once IteratorNeverReturns
	}
	
	private static int RandomTransnationalTarget() {
		int totalWeight = 0;
		int length = TRANSNATIONAL_TARGET_WEIGHTS.Count;
		for (int i = 0; i < length; ++i) {
			int weight = TRANSNATIONAL_TARGET_WEIGHTS[i];
			if (weight > 0) {
				totalWeight += weight;
			}
		}
		if (totalWeight > 0) {
			int random = UnityEngine.Random.Range(0, totalWeight);
			for (int i = 0; i < length; ++i) {
				int weight = TRANSNATIONAL_TARGET_WEIGHTS[i];
				if (weight > 0) {
					random -= weight;
					if (random < 0) {
						return i;
					}
				}
			}
		}
		return UnityEngine.Random.Range(0, length);
	}
}
