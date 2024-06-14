/*
 * @Author: wangyun
 * @CreateTime: 2024-05-28 22:30:05 645
 * @LastEditor: wangyun
 * @EditTime: 2024-05-28 22:30:05 651
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RuinsProps {
	public static bool Test { get; set; } // 测试模式
	
	public static readonly List<int> RUIN_ORDERS = new() {1, 8};
	public static DateTime LAST_REFRESH_TIME;
	public static DateTime LAST_TIME;
	public static int GOT_COUNT;
	public static TimeSpan INTERVAL = new(49, 0, 0);
	public static readonly List<Recognize.RuinPropType> RUIN_PROP_PRIORITY = new() {
		Recognize.RuinPropType.CLASS_PROP,
		Recognize.RuinPropType.BIG_ENERGY,
		Recognize.RuinPropType.GREEN_MATERIAL,
		Recognize.RuinPropType.STRENGTHEN_PART,
		Recognize.RuinPropType.SKILL_TICKET,
		Recognize.RuinPropType.PURPLE_HERO_CHIP
	};
	public static int RETRY_DELAY = 300;
	public static DateTime NEXT_TRY_TIME;
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartRuinsProps", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"自动领遗迹道具已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopRuinsProps", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动领遗迹道具已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			
			DateTime now = DateTime.Now;
			DateTime nextRefreshTime = LAST_REFRESH_TIME + INTERVAL;
			if (now > nextRefreshTime) {
				// 如果刷新了，则记录新的最后一次刷新时间
				LAST_REFRESH_TIME = nextRefreshTime;
			}
			DateTime date = now.Date;
			if (LAST_TIME > date && LAST_TIME > LAST_REFRESH_TIME) {
				// 今天领过了 && 最后一次刷新后领过了
				continue;
			}
			if (now < NEXT_TRY_TIME) {
				// 重试冷却
				continue;
			}
			
			if (Recognize.IsWindowCovered) {
				// 有窗口打开着
				continue;
			}
			if (!Recognize.IsOutsideOrInsideScene) {
				// 非世界非主城场景
				continue;
			}

			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(RuinsProps);

			// 尝试领取遗迹道具
			Debug.Log("联盟按钮");
			Operation.Click(1870, 710);	// 联盟按钮
			yield return new EditorWaitForSeconds(0.3F);
			bool test = Test;
			// if (test || Recognize.AllianceTerritoryIsNew) {
			{
				Debug.Log("联盟领地按钮");
				Operation.Click(835, 525);	// 联盟领地按钮
				yield return new EditorWaitForSeconds(0.5F);
				// if (test || Recognize.AllianceRuinIsNew) {
				{
					Debug.Log("遗迹标签");
					Operation.Click(1124, 196);	// 遗迹标签
					yield return new EditorWaitForSeconds(0.2F);
					// if (test || Recognize.AllianceRuinLv2IsNew) {
					{
						Debug.Log("2级遗迹标签");
						Operation.Click(957, 243);	// 2级遗迹标签
						yield return new EditorWaitForSeconds(0.2F);

						const int ITEM_HEIGHT = 116;
						const int OFFSET_Y_MAX = 206;
						const int VISIBLE_ITEMS_COUNT = 6;
						int offsetY = 0;
						foreach (int order in RUIN_ORDERS) {
							int orderOffsetY = Mathf.Clamp((order - VISIBLE_ITEMS_COUNT) * ITEM_HEIGHT, 0, OFFSET_Y_MAX);
							int deltaOffsetY = orderOffsetY - offsetY;
							while (deltaOffsetY > 0) {
								int dragDistance = Mathf.Min(ITEM_HEIGHT * VISIBLE_ITEMS_COUNT, deltaOffsetY);
								// 往上拖动
								var ie = Operation.NoInertiaDrag(960, 900, 960, 900 - dragDistance, 0.5F);
								while (ie.MoveNext()) {
									yield return ie.Current;
								}
								yield return new EditorWaitForSeconds(0.1F);
								deltaOffsetY -= dragDistance;
							}
							offsetY = orderOffsetY;
							
							Debug.Log("遗迹道具按钮");
							Operation.Click(1147, 355 + (order - 1) * ITEM_HEIGHT - offsetY);	// 遗迹道具按钮
							yield return new EditorWaitForSeconds(0.3F);
							
							yield return new EditorWaitForSeconds(2F);	// 多等一会儿，避免道具列表还没刷新
							Recognize.RuinPropType[] propTypes = new Recognize.RuinPropType[5];
							for (int i = 0; i < 4; ++i) {
								int y = Mathf.RoundToInt(472.2F + i * 102.2F);
								propTypes[i] = Recognize.GetRuinPropType(772, y, 44, 44);
							}
							{
								// 往上拖动
								var ie = Operation.NoInertiaDrag(960, 700, 960, 700 - 68, 0.5F);
								while (ie.MoveNext()) {
									yield return ie.Current;
								}
								yield return new EditorWaitForSeconds(0.1F);
							}
							propTypes[4] = Recognize.GetRuinPropType(772, 813, 44, 44);
							// 找职业道具
							int targetIndex = -1;
							foreach (Recognize.RuinPropType propType in RUIN_PROP_PRIORITY) {
								// 按优先级找对应类型的道具
								targetIndex = Array.IndexOf(propTypes, propType);
								if (targetIndex != -1) {
									break;
								}
							}
							if (targetIndex != -1) {
								string[] propNames = Array.ConvertAll(propTypes, t => Utils.GetEnumInspectorName(t));
								Debug.Log($"排序{order}道具有[{string.Join(",", propNames)}]，选择{propNames[targetIndex]}");
								Vector2Int btnPos;
								if (targetIndex == 0) {
									// 往下拖动
									var ie = Operation.NoInertiaDrag(960, 600, 960, 600 + 68, 0.5F);
									while (ie.MoveNext()) {
										yield return ie.Current;
									}
									yield return new EditorWaitForSeconds(0.1F);
									btnPos = new Vector2Int(1085, 495);
								} else {
									btnPos = new Vector2Int(1085, 426 + targetIndex * 102);
								}
								Color32 btnColor = Operation.GetColorOnScreen(btnPos.x, btnPos.y);
								if (btnColor.g > btnColor.r + btnColor.b) {
									Debug.Log("领取按钮");
									Operation.Click(btnPos.x, btnPos.y);	// 领取按钮
									yield return new EditorWaitForSeconds(0.3F);
									if (!test) {
										Debug.Log("确定按钮");
										Operation.Click(960, 700);	// 确定按钮
										GOT_COUNT++;
										yield return new EditorWaitForSeconds(0.3F);
										Debug.Log("点外面关闭");
										Operation.Click(960, 200);	// 点外面关闭
										yield return new EditorWaitForSeconds(0.1F);
									} else {
										Debug.Log("关闭按钮");
										Operation.Click(1155, 408);	// 关闭按钮
										yield return new EditorWaitForSeconds(0.3F);
									}
									Operation.Click(960, 200);	// 点外面关闭
									yield return new EditorWaitForSeconds(0.3F);
								}
							}
							Debug.Log("关闭按钮");
							Operation.Click(1170, 204);	// 关闭按钮
							yield return new EditorWaitForSeconds(0.3F);
						}
					}
				}
			}
			for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			
			Task.CurrentTask = null;
			
			if (GOT_COUNT >= RUIN_ORDERS.Count) {
				// 如果成功，更新领取时间，重置领取数量
				LAST_TIME = DateTime.Now;
				GOT_COUNT = 0;
			} else {
				// 如果失败，则等待一段时间后重试
				NEXT_TRY_TIME = DateTime.Now + new TimeSpan(0, 0, RETRY_DELAY);
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}

	[MenuItem("Tools_Task/OnceRuinsProps", priority = -1)]
	private static void ExecuteOnce() {
		Debug.Log($"单次执行领遗迹道具尝试");
		EditorCoroutineManager.StartCoroutine(IEExecuteOnce());
	}
	private static IEnumerator IEExecuteOnce() {
		if (Task.CurrentTask != null) {
			Debug.LogError($"正在执行【{Task.CurrentTask}】, 请稍后！");
			yield break;
		}
		DateTime prevLastTime = LAST_TIME;
		DateTime prevNextTryTime = NEXT_TRY_TIME;
		LAST_TIME = DateTime.Now.Date - new TimeSpan(0, 1, 0);
		NEXT_TRY_TIME = DateTime.Now;
		for (int i = 0; i < 10 && Task.CurrentTask != nameof(RuinsProps); i++) {
			yield return null;
		}
		while (Task.CurrentTask != null) {
			yield return null;
		}
		LAST_TIME = prevLastTime;
		NEXT_TRY_TIME = prevNextTryTime;
	}
}
