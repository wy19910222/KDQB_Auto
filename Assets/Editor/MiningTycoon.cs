/*
 * @Author: wangyun
 * @CreateTime: 2023-11-03 14:55:53 057
 * @LastEditor: wangyun
 * @EditTime: 2023-11-03 14:55:53 065
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class MiningTycoonConfig : PrefsEditorWindow<MiningTycoon> {
	[MenuItem("Window/MiningTycoon")]
	private static void Open() {
		GetWindow<MiningTycoonConfig>("矿产大亨").Show();
	}
	
	private void OnGUI() {
		MiningTycoon.ACTIVITY_ORDER = EditorGUILayout.IntSlider("活动排序（活动排在第几个）", MiningTycoon.ACTIVITY_ORDER, 1, 20);
		MiningTycoon.TRAMCAR_AWARD_NUMBER = EditorGUILayout.IntSlider("领奖矿车编号", MiningTycoon.TRAMCAR_AWARD_NUMBER, 1, 4);
		MiningTycoon.TRAMCAR_COUNTDOWN_NUMBER = EditorGUILayout.IntSlider("收取矿车编号", MiningTycoon.TRAMCAR_COUNTDOWN_NUMBER, 1, 4);

		GUILayout.Space(5F);
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		TimeSpan ts = MiningTycoon.NEAREST_DT - DateTime.Now;
		int hours = EditorGUILayout.IntField("倒计时", ts.Hours);
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 8F;
		int minutes = EditorGUILayout.IntField(":", ts.Minutes);
		int seconds = EditorGUILayout.IntField(":", ts.Seconds);
		EditorGUIUtility.labelWidth = prevLabelWidth;
		if (EditorGUI.EndChangeCheck()) {
			MiningTycoon.NEAREST_DT = DateTime.Now + new TimeSpan(hours, minutes, seconds);
		}
		EditorGUILayout.EndHorizontal();
		MiningTycoon.CLICK_INTERVAL = EditorGUILayout.IntSlider("点击间隔（秒）", MiningTycoon.CLICK_INTERVAL, 300, 3600);
		
		GUILayout.Space(5F);
		if (MiningTycoon.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
	}
	
	protected override void OnEnable() {
		base.OnEnable();
		EditorApplication.update += Repaint;
	}

	protected override void OnDisable() {
		base.OnDisable();
		EditorApplication.update -= Repaint;
	}
}

public class MiningTycoon {
	public static int ACTIVITY_ORDER = 7;	// 活动排序
	public static int TRAMCAR_COUNTDOWN_NUMBER = 3;	// 收取矿车编号
	public static int TRAMCAR_AWARD_NUMBER = 3;	// 领奖矿车编号
	public static DateTime NEAREST_DT = DateTime.Now;
	public static int CLICK_INTERVAL = 120;	// 点击间隔
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartMiningTycoon", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"矿产大亨自动点击已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopMiningTycoon", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("矿产大亨自动点击已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			if (DateTime.Now < NEAREST_DT) {
				continue;
			}
			
			// 有窗口打开着
			if (Recognize.IsWindowCovered) {
				continue;
			}

			// 如果是世界界面远景，则没有显示活动按钮，需要先切换到近景
			if (Recognize.CurrentScene == Recognize.Scene.OUTSIDE && Recognize.IsOutsideFaraway) {
				while (Recognize.IsOutsideFaraway) {
					Vector2Int oldPos = MouseUtils.GetMousePos();
					MouseUtils.SetMousePos(960, 540);	// 鼠标移动到屏幕中央
					MouseUtils.ScrollWheel(1);
					MouseUtils.SetMousePos(oldPos.x, oldPos.y);
					yield return new EditorWaitForSeconds(0.1F);
				}
			}
			Debug.Log("活动按钮");
			Operation.Click(1880, 290);	// 活动按钮
			yield return new EditorWaitForSeconds(0.5F);
			Debug.Log("拖动以显示活动标签页");
			const int TAB_WIDTH = 137;
			int orderOffsetX = (ACTIVITY_ORDER - 4) * TAB_WIDTH;
			while (orderOffsetX > 0) {
				const int dragDistance = TAB_WIDTH * 4;
				// 往左上拖动
				var ie = Operation.NoInertiaDrag(1190, 200, 1190 - dragDistance, 200, 0.5F);
				while (ie.MoveNext()) {
					yield return ie.Current;
				}
				yield return new EditorWaitForSeconds(0.1F);
				orderOffsetX -= dragDistance;
			}
			Debug.Log("活动标签页");
			Operation.Click(1190 + orderOffsetX, 200);	// 活动标签页
			yield return new EditorWaitForSeconds(0.1F);
			
			Debug.Log($"尝试领取第{TRAMCAR_AWARD_NUMBER}个矿车");
			Operation.Click(660 + 120 * TRAMCAR_AWARD_NUMBER, 850);	// 点击矿车
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(830, 730);	// 开始收取按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(960, 730);	// 领取奖励按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(660, 850);	// 点击窗口外关闭
			yield return new EditorWaitForSeconds(0.2F);

			Debug.Log("挖矿");
			for (int i = 0; i < 20; ++i) {
				Operation.Click(1060, 970);	// 挖矿按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			
			Debug.Log($"开始获取第{TRAMCAR_COUNTDOWN_NUMBER}个矿车");
			Operation.Click(660 + 120 * TRAMCAR_COUNTDOWN_NUMBER, 850);	// 点击矿车
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(830, 730);	// 开始收取按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(960, 730);	// 领取奖励按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(660, 850);	// 点击窗口外关闭
			yield return new EditorWaitForSeconds(0.2F);
			
			for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			
			NEAREST_DT = DateTime.Now + new TimeSpan(0, 0, CLICK_INTERVAL);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
