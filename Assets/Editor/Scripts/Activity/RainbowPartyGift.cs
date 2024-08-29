/*
 * @Author: wangyun
 * @CreateTime: 2024-06-06 19:41:32 167
 * @LastEditor: wangyun
 * @EditTime: 2024-06-06 19:41:32 172
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class RainbowPartyGift {
	public static int ACTIVITY_ORDER = 1;	// 活动排序
	public static int INTERVAL = 120;	// 领取间隔（秒）
	public static int COUNT_OF_ONCE = 9;	// 每次数量
	public static DateTime NEXT_TIME;	// 下一次领取时间
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartRainbowPartyGift", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"彩虹派对自动领奖已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopRainbowPartyGift", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("彩虹派对自动领奖已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			DateTime now = DateTime.Now;
			if (now < NEXT_TIME) {
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
			Task.CurrentTask = nameof(RainbowPartyGift);
			
			// 如果是世界界面远景，则没有显示活动按钮，需要先切换到近景
			for (int i = 0; i < 50 && Recognize.CurrentScene == Recognize.Scene.OUTSIDE_FARAWAY; i++) {
				// 鼠标移动到屏幕中央并滚动滚轮
				Operation.SetMousePosTemporarily(960, 540, () => {
					MouseUtils.ScrollWheel(1);
				});
				yield return new EditorWaitForSeconds(0.1F);
			}

			for (int i = 0; i < COUNT_OF_ONCE; i++) {
				Debug.Log("活动按钮");
				Operation.Click(1880, Global.ACTIVITY_BTN_Y);	// 活动按钮
				yield return new EditorWaitForSeconds(0.5F);
				Debug.Log("拖动以显示活动标签页");
				const int TAB_WIDTH = 137;
				int orderOffsetX = (ACTIVITY_ORDER - 4) * TAB_WIDTH;
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

				Debug.Log("前往派对按钮");
				Operation.Click(960, 960);	// 前往派对按钮
				yield return new EditorWaitForSeconds(0.3F);
			
				Debug.Log("附近礼品按钮");
				Operation.Click(960, 720);	// 附近礼品按钮
				yield return new EditorWaitForSeconds(0.2F);
				if (!Recognize.IsRainbowPartyGiftCanGet) {
					Operation.Click(960, 790);	// 附近礼品按钮
					yield return new EditorWaitForSeconds(0.2F);
				}

				if (Recognize.IsRainbowPartyGiftCanGet) {
					Debug.Log("领取按钮");
					Operation.Click(960, 445);	// 领取按钮
					yield return new EditorWaitForSeconds(0.2F);
				}
			}
			NEXT_TIME = now + new TimeSpan(0, 0, INTERVAL);
			
			Task.CurrentTask = null;
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
