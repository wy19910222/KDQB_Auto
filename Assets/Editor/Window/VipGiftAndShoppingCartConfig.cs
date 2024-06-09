/*
 * @Author: wangyun
 * @CreateTime: 2024-06-10 03:09:16 835
 * @LastEditor: wangyun
 * @EditTime: 2024-06-10 03:09:16 841
 */

using System;
using UnityEditor;
using UnityEngine;

public class VipGiftAndShoppingCartConfig : PrefsEditorWindow<VipGiftAndShoppingCart> {
	[MenuItem("Tools_Window/Default/VipGiftAndShoppingCart")]
	private static void Open() {
		GetWindow<VipGiftAndShoppingCartConfig>("军级礼物/购物车").Show();
	}

	private void OnGUI() {
		DateTime now = DateTime.Now;
		DateTime date = now.Date;
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("军级每日奖励");
		bool vipGiftSucceed = VipGiftAndShoppingCart.LAST_VIP_GIFT_TIME > date;
		bool newVipGiftSucceed = GUILayout.Toggle(vipGiftSucceed, "已完成", "Button", GUILayout.Width(60F));
		if (newVipGiftSucceed != vipGiftSucceed) {
			VipGiftAndShoppingCart.LAST_VIP_GIFT_TIME = newVipGiftSucceed ? now : now - new TimeSpan(24, 0, 0);
		}
		EditorGUILayout.EndHorizontal();

		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}
		
		EditorGUILayout.BeginHorizontal();
		VipGiftAndShoppingCart.SHOPPING_CART_ORDER = EditorGUILayout.IntSlider("购物车排序", VipGiftAndShoppingCart.SHOPPING_CART_ORDER, 1, 4);
		bool shoppingCartSucceed = VipGiftAndShoppingCart.LAST_SHOPPING_CART_TIME > date;
		bool newShoppingCartSucceed = GUILayout.Toggle(shoppingCartSucceed, "已完成", "Button", GUILayout.Width(60F));
		if (newShoppingCartSucceed != shoppingCartSucceed) {
			VipGiftAndShoppingCart.LAST_SHOPPING_CART_TIME = newShoppingCartSucceed ? now : now - new TimeSpan(24, 0, 0);
		}
		EditorGUILayout.EndHorizontal();

		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}
		
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		TimeSpan ts = VipGiftAndShoppingCart.DAILY_TIME;
		int hours = EditorGUILayout.IntField("执行时间", ts.Hours);
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 8F;
		int minutes = EditorGUILayout.IntField(":", ts.Minutes);
		int seconds = EditorGUILayout.IntField(":", ts.Seconds);
		EditorGUIUtility.labelWidth = prevLabelWidth;
		if (EditorGUI.EndChangeCheck()) {
			VipGiftAndShoppingCart.DAILY_TIME = new TimeSpan(hours, minutes, seconds);
		}
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space(5F);
		
		EditorGUILayout.BeginHorizontal();
		VipGiftAndShoppingCart.Test = GUILayout.Toggle(VipGiftAndShoppingCart.Test, "测试模式", "Button", GUILayout.Width(60F));
		if (VipGiftAndShoppingCart.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
		EditorGUILayout.EndHorizontal();
	}
	
	private void Update() {
		Repaint();
	}
}