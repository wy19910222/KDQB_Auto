/*
 * @Author: wangyun
 * @CreateTime: 2023-10-12 00:00:51 751
 * @LastEditor: wangyun
 * @EditTime: 2023-10-12 00:00:51 756
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class DeepSeaConfig : PrefsEditorWindow<DeepSea> {
	[MenuItem("Window/DeepSea")]
	private static void Open() {
		GetWindow<DeepSeaConfig>("深海寻宝").Show();
	}
	
	private void OnGUI() {
		DeepSea.ACTIVITY_ORDER = EditorGUILayout.IntSlider("活动排序（活动排在第几个）", DeepSea.ACTIVITY_ORDER, 1, 20);
		DeepSea.DETECTOR_COUNT = EditorGUILayout.IntSlider("拥有探测器", DeepSea.DETECTOR_COUNT, 1, 3);
		// DeepSea.CLICK_INTERVAL = Mathf.Clamp(EditorGUILayout.IntField("点击间隔时间（秒）", DeepSea.CLICK_INTERVAL), 60, 1200);
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		TimeSpan ts = DeepSea.Countdown;
		int hours = EditorGUILayout.IntField("倒计时", ts.Hours);
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 8F;
		int minutes = EditorGUILayout.IntField(":", ts.Minutes);
		int seconds = EditorGUILayout.IntField(":", ts.Seconds);
		EditorGUIUtility.labelWidth = prevLabelWidth;
		if (EditorGUI.EndChangeCheck()) {
			DeepSea.Countdown = new TimeSpan(hours, minutes, seconds);
		}
		EditorGUILayout.EndHorizontal();
		DeepSea.TRY_COUNT = EditorGUILayout.IntSlider("尝试次数", DeepSea.TRY_COUNT, 1, 4);
		
		GUILayout.Space(5F);
		if (DeepSea.IsRunning) {
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

public class DeepSea {
	public static int ACTIVITY_ORDER = 8;	// 活动排序
	public static int DETECTOR_COUNT = 3;	// 拥有探测器数量
	public static TimeSpan DEFAULT_COUNTDOWN = new TimeSpan(8, 0, 0);	// 倒计时
	public static int TRY_COUNT = 3;	// 倒计时结束时尝试3次
	// public static int CLICK_INTERVAL = 120;	// 点击间隔
	public static DateTime TargetDT;
	public static TimeSpan Countdown {
		get => TargetDT - DateTime.Now;
		set => TargetDT = DateTime.Now + value;
	}
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartDeepSea", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"深海寻宝自动点击已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopDeepSea", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("深海寻宝自动点击已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			if (DateTime.Now < TargetDT) {
				continue;
			}
			// 有窗口打开着
			if (Recognize.IsWindowCovered) {
				continue;
			}

			for (int tryCount = 0; tryCount < TRY_COUNT; ++tryCount) {
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
				int orderOffsetX = (ACTIVITY_ORDER - 4) * 137;
				while (orderOffsetX > 0) {
					const int dragDistance = 136 * 4;
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
				Debug.Log("点击探测器");
				for (int i = 0; i < DETECTOR_COUNT; ++i) {
					Debug.Log($"点击探测器{i + 1}");
					Operation.Click(808 + 153 * i, 870);	// 探测器
					yield return new EditorWaitForSeconds(0.2F);
					Operation.Click(808 + 153 * i, 870);	// 探测器
					yield return new EditorWaitForSeconds(0.2F);
					Operation.Click(808 + 153 * i, 870);	// 探测器
					yield return new EditorWaitForSeconds(0.2F);
					Operation.Click(808 + 153 * i, 870);	// 探测器
					yield return new EditorWaitForSeconds(0.2F);
				}
				while (Recognize.IsWindowCovered) {	// 如果有窗口，多点几次返回按钮
					Debug.Log("关闭窗口");
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.2F);
				}
			}
			TargetDT = DateTime.Now + DEFAULT_COUNTDOWN;
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
