/*
 * @Author: wangyun
 * @CreateTime: 2023-10-31 07:02:42 199
 * @LastEditor: wangyun
 * @EditTime: 2023-10-31 07:02:42 204
 */

using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

public class Ark {
	public static TimeSpan DAILY_TIME = new TimeSpan(9, 5, 0);
	public static readonly int[] SQUAD_NUMBERS = new int[4];	// 各个方舟使用编队号码
	
	public static readonly bool[] IsInArks = new bool[4];	// 当天是否进过方舟
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartArk", priority = -1)]
	private static void Enable() {
		Disable();
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopArk", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;

			DateTime now = DateTime.Now;
			bool currIsAfterTime = now - now.Date >= DAILY_TIME;
			if (!currIsAfterTime) {
				for (int i = 0, length = IsInArks.Length; i < length; ++i) {
					IsInArks[i] = false;
				}
				continue;
			}
			
			if (Array.TrueForAll(IsInArks, isInArk => isInArk)) {
				continue;
			}

			if (Recognize.CurrentScene != Recognize.Scene.OUTSIDE_NEARBY && Recognize.CurrentScene != Recognize.Scene.OUTSIDE_FARAWAY) {
				continue;
			}

			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(Ark);

			for (int i = IsInArks.Length - 1; i >= 0; --i) {
				if (!Recognize.IsAnyGroupIdle) {
					yield return new EditorWaitForSeconds(0.2F);
					if (!Recognize.IsAnyGroupIdle) {
						break;
					}
				}
				if (!IsInArks[i]) {
					int deltaY = Recognize.IsOutsideNearby ? 76 : Recognize.IsOutsideFaraway ? 0 : -1;
					deltaY = Recognize.IsMiniMapShowing switch {
						true => deltaY + 155,
						false => deltaY,
						_ => -1
					};
					if (deltaY != -1) {
						Debug.Log("收藏夹按钮");
						Operation.Click(100, 200 + deltaY);	// 收藏夹按钮
						yield return new EditorWaitForSeconds(0.5F);
						Debug.Log($"列表第{i + 1}个");
						Operation.Click(960, 340 + 105 * i);	// 列表第i个
						yield return new EditorWaitForSeconds(0.5F);
						Debug.Log("选中方舟");
						Operation.Click(960, 400);	// 选中方舟
						yield return new EditorWaitForSeconds(0.2F);
						Debug.Log("加入战斗按钮");
						Operation.Click(1060, 825);	// 加入战斗按钮
						yield return new EditorWaitForSeconds(0.5F);
						if (Recognize.CurrentScene == Recognize.Scene.FIGHTING_MARCH) {
							Debug.Log("选择队列");
							Operation.Click(1145 + 37 * SQUAD_NUMBERS[i], 870);	// 选择队列
							yield return new EditorWaitForSeconds(0.2F);
							Debug.Log("出战按钮");
							Operation.Click(960, 470);	// 出战按钮
							yield return new EditorWaitForSeconds(0.3F);
							if (Recognize.IsFriendlyHinting) {
								Debug.Log("确定按钮");
								Operation.Click(1060, 700);	// 选择队列
							}
							IsInArks[i] = true;
						}
						yield return new EditorWaitForSeconds(0.5F);
					}
				}
			}
			
			Task.CurrentTask = null;
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
