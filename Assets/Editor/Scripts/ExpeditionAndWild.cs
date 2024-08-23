/*
 * @Author: wangyun
 * @CreateTime: 2024-04-09 17:05:53 507
 * @LastEditor: wangyun
 * @EditTime: 2024-04-09 17:05:53 512
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class ExpeditionAndWild {
	public static bool Test { get; set; } // 测试模式
	
	public static int WILD_ORDER = 1;
	public static int Expedition_ORDER = 5;
	public static int INTERVAL_HOURS = 6;
	public static DateTime TargetDT;	// 倒计时
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;
	
	private const int ITEM_HEIGHT = 187;
	private const int OFFSET_Y = -61;
	private const int VISIBLE_ITEMS_COUNT = 4;

	[MenuItem("Tools_Task/StartExpeditionAndWild", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"远征/荒野行动自动收取已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopExpeditionAndWild", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("远征/荒野行动自动收取已关闭");
		}
	}

	private static IEnumerator Update() {
		int tryCount = 0;
		bool wildSucceed = false, expeditionSucceed = false;
		while (true) {
			yield return null;
			bool test = Test;
			if (DateTime.Now < TargetDT && !test) {
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
			Task.CurrentTask = nameof(ExpeditionAndWild);

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
			if (!wildSucceed && Recognize.FullWindowTitle == "每日军情") {
				Debug.Log("拖动以显示荒野行动");
				int orderOffsetY = Mathf.Max((WILD_ORDER - VISIBLE_ITEMS_COUNT) * ITEM_HEIGHT + OFFSET_Y, 0);
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
				Operation.Click(1092, 320 + (WILD_ORDER - 1) * ITEM_HEIGHT - offsetY);	// 前往按钮
				yield return new EditorWaitForSeconds(0.3F);
				if (Recognize.DailyIntelligenceCurrentType == Recognize.DailyIntelligenceType.WILD && !test) {
					Debug.Log("宝箱按钮");
					Operation.Click(751, 286);	// 宝箱按钮
					yield return new EditorWaitForSeconds(1F);
					// if (Recognize.MaybeWindowTitle == "领取奖励") {
						Debug.Log("领取或返回按钮");
						Operation.Click(960, 836);	// 领取或返回按钮
						yield return new EditorWaitForSeconds(0.5F);
						Debug.Log("空白处");
						Operation.Click(960, 836);	// 空白处
						yield return new EditorWaitForSeconds(0.2F);
						wildSucceed = true;
					// }
					// if (Recognize.IsWildBackBtn) {
					// 	Debug.Log("返回按钮");
					// 	Operation.Click(960, 836);	// 返回按钮
					// 	yield return new EditorWaitForSeconds(0.2F);
					// 	wildSucceed = true;
					// } else if (Recognize.IsWildGetBtn) {
					// 	Debug.Log("领取按钮");
					// 	Operation.Click(960, 836);	// 领取按钮
					// 	yield return new EditorWaitForSeconds(0.5F);
					// 	Debug.Log("空白处");
					// 	Operation.Click(960, 836);	// 空白处
					// 	yield return new EditorWaitForSeconds(0.2F);
					// 	wildSucceed = true;
					// }
				}
				Debug.Log("左上角返回按钮");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.2F);
			}

			if (!expeditionSucceed && Recognize.FullWindowTitle == "每日军情") {
				Debug.Log("拖动以显示远征行动");
				int orderOffsetY = Mathf.Max((Expedition_ORDER - VISIBLE_ITEMS_COUNT) * ITEM_HEIGHT + OFFSET_Y, 0);
				int deltaOffsetY = orderOffsetY - offsetY;
				while (deltaOffsetY > 0) {
					int dragDistance = Mathf.Min(ITEM_HEIGHT * VISIBLE_ITEMS_COUNT, deltaOffsetY);
					// 往左拖动
					var ie = Operation.NoInertiaDrag(960, 810, 960, 810 - dragDistance, 0.5F);
					while (ie.MoveNext()) {
						yield return ie.Current;
					}
					yield return new EditorWaitForSeconds(0.1F);
					deltaOffsetY -= dragDistance;
				}
				offsetY = orderOffsetY;
				yield return new EditorWaitForSeconds(0.2F);
				
				Debug.Log("前往按钮");
				Operation.Click(1092, 320 + (Expedition_ORDER - 1) * ITEM_HEIGHT - offsetY);	// 前往按钮
				yield return new EditorWaitForSeconds(0.3F);
				if (Recognize.DailyIntelligenceCurrentType == Recognize.DailyIntelligenceType.EXPEDITION && !test) {
					Debug.Log("运输车");
					Operation.Click(840, 856);	// 运输车
					yield return new EditorWaitForSeconds(1F);
					if (Recognize.MaybeWindowTitle == "获得奖励") {
						Debug.Log("领取按钮");
						Operation.Click(960, 900);	// 领取按钮
						yield return new EditorWaitForSeconds(0.3F);
						expeditionSucceed = true;
					}
					// if (Recognize.IsExpeditionGetBtn) {
					// 	Debug.Log("领取按钮");
					// 	Operation.Click(960, 900);	// 领取按钮
					// 	yield return new EditorWaitForSeconds(0.3F);
					// 	expeditionSucceed = true;
					// }
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

			if (wildSucceed && expeditionSucceed || tryCount > 3) {
				tryCount = 0;
				wildSucceed = false;
				expeditionSucceed = false;
				TargetDT = DateTime.Now + new TimeSpan(INTERVAL_HOURS, 0, 2);
			} else {
				++tryCount;
			}
			
			Task.CurrentTask = null;
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
