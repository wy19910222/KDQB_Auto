/*
 * @Author: wangyun
 * @CreateTime: 2023-10-31 07:02:42 199
 * @LastEditor: wangyun
 * @EditTime: 2023-10-31 07:02:42 204
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class ArkConfig : PrefsEditorWindow<Ark> {
	[MenuItem("Window/Ark")]
	private static void Open() {
		GetWindow<ArkConfig>("参战方舟").Show();
	}

	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("每天触发时间", GUILayout.Width(EditorGUIUtility.labelWidth));
		EditorGUI.BeginChangeCheck();
		int startHours = EditorGUILayout.IntField(Ark.DAILY_TIME.Hours, GUILayout.MinWidth(20));
		EditorGUILayout.LabelField(":", GUILayout.Width(8));
		int startMinutes = EditorGUILayout.IntField(Ark.DAILY_TIME.Minutes, GUILayout.MinWidth(20));
		if (EditorGUI.EndChangeCheck()) {
			Ark.DAILY_TIME = new TimeSpan(startHours, startMinutes, 0);
		}
		EditorGUILayout.EndHorizontal();
		Ark.GROUP_COUNT = EditorGUILayout.IntSlider("拥有行军队列", Ark.GROUP_COUNT, 0, 7);
		Ark.SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", Ark.SQUAD_NUMBER, 1, 8);

		GUILayout.Space(5F);
		if (Ark.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
	}
}

public class Ark {
	public static TimeSpan DAILY_TIME = new TimeSpan(9, 5, 0);
	public static int GROUP_COUNT = 4;	// 拥有行军队列数
	public static int SQUAD_NUMBER = 1;	// 使用编队号码
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartArk", priority = -1)]
	private static void Enable() {
		Disable();
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopArk", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
		}
	}

	private static IEnumerator Update() {
		bool isInArk1 = false;
		bool isInArk2 = false;
		while (true) {
			yield return null;

			DateTime now = DateTime.Now;
			bool currIsAfterTime = now - now.Date >= DAILY_TIME;
			if (!currIsAfterTime) {
				isInArk1 = false;
				isInArk2 = false;
				continue;
			}
			if (isInArk1 && isInArk2) {
				continue;
			}

			if (Recognize.CurrentScene != Recognize.Scene.OUTSIDE) {
				continue;
			}

			if (Recognize.BusyGroupCount >= GROUP_COUNT) {
				yield return new EditorWaitForSeconds(0.2F);
				if (Recognize.BusyGroupCount >= GROUP_COUNT) {
					continue;
				}
			}
			if (!isInArk1) {
				int deltaY = Recognize.IsOutsideNearby ? 76 : Recognize.IsOutsideFaraway ? 0 : -1;
				if (deltaY != -1) {
					Debug.Log("收藏夹按钮");
					Operation.Click(100, 355 + deltaY);	// 收藏夹按钮
					yield return new EditorWaitForSeconds(0.5F);
					Debug.Log("列表第一个");
					Operation.Click(960, 340);	// 列表第一个
					yield return new EditorWaitForSeconds(0.5F);
					// Debug.Log("列表第二个");
					// Operation.Click(960, 445);	// 列表第二个
					// yield return new EditorWaitForSeconds(0.5F);
					Debug.Log("选中方舟");
					Operation.Click(960, 400);	// 选中方舟
					yield return new EditorWaitForSeconds(0.2F);
					Debug.Log("加入战斗按钮");
					Operation.Click(1060, 825);	// 加入战斗按钮
					yield return new EditorWaitForSeconds(0.5F);
					if (Recognize.CurrentScene == Recognize.Scene.FIGHTING) {
						Debug.Log("选择队列");
						Operation.Click(1145 + 37 * SQUAD_NUMBER, 870);	// 选择队列
						yield return new EditorWaitForSeconds(0.2F);
						Debug.Log("出战按钮");
						Operation.Click(960, 470);	// 出战按钮
						yield return new EditorWaitForSeconds(0.3F);
						if (Recognize.IsFriendlyHinting) {
							Debug.Log("确定按钮");
							Operation.Click(1060, 700);	// 选择队列
						}
						isInArk1 = true;
					}
				}
			}
			
			yield return new EditorWaitForSeconds(0.5F);
			
			if (Recognize.BusyGroupCount >= GROUP_COUNT) {
				yield return new EditorWaitForSeconds(0.2F);
				if (Recognize.BusyGroupCount >= GROUP_COUNT) {
					continue;
				}
			}
			if (!isInArk2) {
				int deltaY = Recognize.IsOutsideNearby ? 76 : Recognize.IsOutsideFaraway ? 0 : -1;
				if (deltaY != -1) {
					Debug.Log("收藏夹按钮");
					Operation.Click(100, 355 + deltaY);	// 收藏夹按钮
					yield return new EditorWaitForSeconds(0.5F);
					Debug.Log("列表第二个");
					Operation.Click(960, 445);	// 列表第二个
					yield return new EditorWaitForSeconds(0.5F);
					Debug.Log("选中方舟");
					Operation.Click(960, 400);	// 选中方舟
					yield return new EditorWaitForSeconds(0.2F);
					Debug.Log("加入战斗按钮");
					Operation.Click(1060, 825);	// 加入战斗按钮
					yield return new EditorWaitForSeconds(0.5F);
					if (Recognize.CurrentScene == Recognize.Scene.FIGHTING) {
						Debug.Log("选择队列");
						Operation.Click(1145 + 37 * SQUAD_NUMBER, 870);	// 选择队列
						yield return new EditorWaitForSeconds(0.2F);
						Debug.Log("出战按钮");
						Operation.Click(960, 470);	// 出战按钮
						yield return new EditorWaitForSeconds(0.3F);
						if (Recognize.IsFriendlyHinting) {
							Debug.Log("确定按钮");
							Operation.Click(1060, 700);	// 选择队列
						}
						isInArk2 = true;
					}
				}
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
