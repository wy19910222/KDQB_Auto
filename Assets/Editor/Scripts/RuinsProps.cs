/*
 * @Author: wangyun
 * @CreateTime: 2024-05-28 22:30:05 645
 * @LastEditor: wangyun
 * @EditTime: 2024-05-28 22:30:05 651
 */

using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class RuinsProps {
	public static readonly List<int> RUIN_ORDERS = new List<int>() {1, 8};
	public static DateTime LAST_REFRESH_TIME = default;
	public static DateTime LAST_TIME = default;
	public static TimeSpan INTERVAL = new TimeSpan(49, 0, 0);
	
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
			if (Recognize.AllianceTerritoryIsNew) {
				Debug.Log("联盟领地按钮");
				Operation.Click(835, 525);	// 联盟领地按钮
				yield return new EditorWaitForSeconds(0.5F);
				if (Recognize.AllianceRuinIsNew) {
					Debug.Log("遗迹标签");
					Operation.Click(1124, 196);	// 遗迹标签
					yield return new EditorWaitForSeconds(0.2F);
					if (Recognize.AllianceRuinLv2IsNew) {
						Debug.Log("2级遗迹标签");
						Operation.Click(957, 243);	// 2级遗迹标签
						yield return new EditorWaitForSeconds(0.2F);

						int succeeded = 0;
						const int ITEM_HEIGHT = 116;
						const int OFFSET_Y_MAX = 206;
						int offsetY = 0;
						foreach (int order in RUIN_ORDERS) {
							int orderOffsetY = Mathf.Clamp((order - 6) * ITEM_HEIGHT, 0, OFFSET_Y_MAX);
							int deltaOffsetY = orderOffsetY - offsetY;
							offsetY = orderOffsetY;
							while (deltaOffsetY > 0) {
								int dragDistance = Mathf.Min(ITEM_HEIGHT * 6, deltaOffsetY);
								// 往上拖动
								var ie = Operation.NoInertiaDrag(960, 900, 960, 900 - dragDistance, 0.5F);
								while (ie.MoveNext()) {
									yield return ie.Current;
								}
								yield return new EditorWaitForSeconds(0.1F);
								deltaOffsetY -= dragDistance;
							}
							Debug.Log("遗迹道具按钮");
							Operation.Click(1147, 355 + (order - 1) * ITEM_HEIGHT - offsetY);	// 遗迹道具按钮
							yield return new EditorWaitForSeconds(0.3F);

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
							int targetIndex = Array.IndexOf(propTypes, Recognize.RuinPropType.CLASS_PROP);
							if (targetIndex == -1) {
								// 找大体
								targetIndex = Array.IndexOf(propTypes, Recognize.RuinPropType.BIG_ENERGY);
							}
							if (targetIndex == -1) {
								// 找绿色材料
								targetIndex = Array.IndexOf(propTypes, Recognize.RuinPropType.GREEN_MATERIAL);
							}
							if (targetIndex == -1) {
								// 找强化部件
								targetIndex = Array.IndexOf(propTypes, Recognize.RuinPropType.STRENGTHEN_PART);
							}
							if (targetIndex == -1) {
								// 找技能抽卡券
								targetIndex = Array.IndexOf(propTypes, Recognize.RuinPropType.SKILL_TICKET);
							}
							if (targetIndex == -1) {
								// 找紫色英雄碎片
								targetIndex = Array.IndexOf(propTypes, Recognize.RuinPropType.PURPLE_HERO_CHIP);
							}
							if (targetIndex != -1) {
								Vector2Int btnPos = new Vector2Int(1085, targetIndex == 0 ? 495 : 426 + targetIndex * 102);
								Color32 btnColor = Operation.GetColorOnScreen(btnPos.x, btnPos.y);
								if (btnColor.g > btnColor.r + btnColor.b) {
									Debug.Log("领取按钮");
									Operation.Click(btnPos.x, btnPos.y);	// 领取按钮
									yield return new EditorWaitForSeconds(0.3F);
									Debug.Log("确定按钮");
									Operation.Click(960, 700);	// 确定按钮
									succeeded++;
									yield return new EditorWaitForSeconds(0.3F);
									Debug.Log("点外面关闭");
									Operation.Click(960, 200);	// 点外面关闭
									yield return new EditorWaitForSeconds(0.1F);
									Operation.Click(960, 200);	// 点外面关闭
									yield return new EditorWaitForSeconds(0.3F);
								}
							}
							Debug.Log("关闭按钮");
							Operation.Click(1170, 204);	// 关闭按钮
							yield return new EditorWaitForSeconds(0.3F);
						}
						if (succeeded >= RUIN_ORDERS.Count) {
							LAST_TIME = DateTime.Now;
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
		LAST_TIME = default;
		do {
			yield return null;
		} while (Task.CurrentTask != null);
		LAST_TIME = prevLastTime;
	}
}
