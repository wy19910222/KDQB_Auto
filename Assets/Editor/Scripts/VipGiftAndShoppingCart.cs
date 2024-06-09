/*
 * @Author: wangyun
 * @CreateTime: 2024-06-10 03:09:16 835
 * @LastEditor: wangyun
 * @EditTime: 2024-06-10 03:09:16 841
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class VipGiftAndShoppingCart {
	public static bool Test { get; set; } // 测试模式
	
	public static int SHOPPING_CART_ORDER = 2;
	public static TimeSpan DAILY_TIME = new TimeSpan(1, 0, 0);	// 每天几点执行
	
	public static DateTime LAST_VIP_GIFT_TIME;
	public static DateTime LAST_SHOPPING_CART_TIME;
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;
	
	private const int TAB_WIDTH = 140;
	private const int VISIBLE_TABS_COUNT = 4;

	[MenuItem("Tools_Task/StartVipGiftAndShoppingCart", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"军级礼物/购物车自动领取已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopVipGiftAndShoppingCart", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("军级礼物/购物车自动领取已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			DateTime now = DateTime.Now;
			DateTime date = now.Date;
			
			bool test = Test;
			bool vipGiftSucceed = LAST_VIP_GIFT_TIME > date;
			bool shoppingCartSucceed = LAST_SHOPPING_CART_TIME > date;
			if ((vipGiftSucceed && shoppingCartSucceed || DateTime.Now.TimeOfDay < DAILY_TIME) && !test) {
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
			Task.CurrentTask = nameof(VipGiftAndShoppingCart);

			// 如果是世界界面远景，则没有军级按钮，需要先切换到近景
			for (int i = 0; i < 50 && Recognize.CurrentScene == Recognize.Scene.OUTSIDE_FARAWAY; i++) {
				// 鼠标移动到屏幕中央并滚动滚轮
				Operation.SetMousePosTemporarily(960, 540, () => {
					MouseUtils.ScrollWheel(1);
				});
				yield return new EditorWaitForSeconds(0.1F);
			}
			Debug.Log("军级按钮");
			Operation.Click(54, 192);	// 军级按钮
			yield return new EditorWaitForSeconds(0.5F);

			if (!vipGiftSucceed) {
				Debug.Log("军级每日奖励");
				Operation.Click(1148, 250);	// 军级每日奖励
				yield return new EditorWaitForSeconds(0.5F);
				Debug.Log("点外面关闭");
				Operation.Click(960, 200);	// 点外面关闭
				yield return new EditorWaitForSeconds(0.2F);
				LAST_VIP_GIFT_TIME = DateTime.Now;
			}

			if (!shoppingCartSucceed) {
				Debug.Log("军级商店按钮");
				Operation.Click(815, 345);	// 军级商店按钮
				yield return new EditorWaitForSeconds(0.3F);
				Debug.Log("拖动以显示购物车标签页");
				{
					// 先拖动到列表最开头，以便计算
					var ie = Operation.NoInertiaDrag(760, 192, 1200, 192, 0.5F);
					while (ie.MoveNext()) {
						yield return ie.Current;
					}
					yield return new EditorWaitForSeconds(1F);
				}
				int offsetX = 0;
				int orderOffsetX = Mathf.Max((SHOPPING_CART_ORDER - VISIBLE_TABS_COUNT) * TAB_WIDTH, 0);
				int deltaOffsetX = orderOffsetX - offsetX;
				while (deltaOffsetX > 0) {
					int dragDistance = Mathf.Min(TAB_WIDTH * VISIBLE_TABS_COUNT, deltaOffsetX);
					// 往左拖动
					var ie = Operation.NoInertiaDrag(1180, 192, 1180 - dragDistance, 192, 0.5F);
					while (ie.MoveNext()) {
						yield return ie.Current;
					}
					yield return new EditorWaitForSeconds(0.1F);
					deltaOffsetX -= dragDistance;
				}
				offsetX = orderOffsetX;
				yield return new EditorWaitForSeconds(0.2F);
				
				Debug.Log("购物车标签页");
				Operation.Click(760 + (SHOPPING_CART_ORDER - 1) * TAB_WIDTH - offsetX, 192);	// 购物车标签页
				yield return new EditorWaitForSeconds(0.3F);

				if (Recognize.IsShoppingCartExchangeBtn) {
					Debug.Log("一键兑换按钮");
					Operation.Click(1143, 962);	// 一键兑换按钮
					yield return new EditorWaitForSeconds(0.3F);
					Debug.Log("确定按钮");
					Operation.Click(1072, 705);	// 确定按钮
					yield return new EditorWaitForSeconds(0.5F);
					Debug.Log("点外面关闭");
					Operation.Click(960, 200);	// 点外面关闭
					yield return new EditorWaitForSeconds(0.2F);
					LAST_SHOPPING_CART_TIME = DateTime.Now;
				}
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
}
