/*
 * @Author: wangyun
 * @CreateTime: 2024-04-02 02:31:42 315
 * @LastEditor: wangyun
 * @EditTime: 2024-04-02 02:31:42 320
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public struct CastleAbility {
	public string name;
	public int order;
	public int cooldownHours;
	public DateTime cooldownTime;

	public TimeSpan Countdown {
		get => cooldownTime - DateTime.Now;
		set => cooldownTime = DateTime.Now + value;
	}
}

public class AutoCastCastleAbility {
	public static int RETRY_DELAY = 300;
	public static readonly List<CastleAbility> ABILITIES = new List<CastleAbility>();
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartAutoCastCastleAbility", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"城堡技能自动施法已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopAutoCastCastleAbility", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("城堡技能自动施法已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			if (ABILITIES.Count == 0) {
				continue;
			}
			
			if (DateTime.Now < ABILITIES[0].cooldownTime) {
				continue;
			}
			
			// 有窗口打开着
			if (Recognize.IsWindowCovered) {
				continue;
			}
			// 非世界非主城场景
			if (!Recognize.IsOutsideOrInsideScene) {
				continue;
			}

			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(AutoCastCastleAbility);

			// 开始自动施放城堡技能
			switch (Recognize.CurrentScene) {
				case Recognize.Scene.INSIDE:
					Debug.Log("切换场景");
					Operation.Click(1170, 970);	// 右下角主城与世界切换按钮
					break;
				case Recognize.Scene.OUTSIDE_NEARBY:
				case Recognize.Scene.OUTSIDE_FARAWAY:
					Debug.Log("切换场景以定位");
					Operation.Click(1170, 970);	// 右下角主城与世界切换按钮
					yield return new EditorWaitForSeconds(1F);
					Operation.Click(1170, 970);	// 右下角主城与世界切换按钮
					break;
				default:
					goto EndOfAutoCastCastleAbility;
			}
			yield return new EditorWaitForSeconds(2F);
			Debug.Log("自己城堡");
			Operation.Click(960, 500);	// 自己城堡
			yield return new EditorWaitForSeconds(0.2F);
			int btnPosition = Recognize.AbilityShortcutsBtnPosition;
			if (btnPosition is 3 or 4) {
				Debug.Log("技能快捷栏按钮");
				Operation.Click(btnPosition is 3 ? 852 : 961, 782);
				yield return new EditorWaitForSeconds(0.3F);
				Debug.Log("拖动以显示技能列表");
				CastleAbility ability = ABILITIES[0];
				const int ITEM_HEIGHT = 147;
				int orderOffsetY = (ability.order - 4) * ITEM_HEIGHT;
				while (orderOffsetY > 0) {
					const int dragDistance = ITEM_HEIGHT * 4;
					// 往上拖动
					var ie = Operation.NoInertiaDrag(960, 810, 960, 810 - dragDistance, 0.5F);
					while (ie.MoveNext()) {
						yield return ie.Current;
					}
					yield return new EditorWaitForSeconds(0.1F);
					orderOffsetY -= dragDistance;
				}
				yield return new EditorWaitForSeconds(0.1F);
				Color32 btnColor = Operation.GetColorOnScreen(1080, 810 + orderOffsetY);
				if (Recognize.Approximately(btnColor, new Color32(76, 188, 71, 255))) {
					Debug.Log("使用按钮");
					Operation.Click(1114, 810 + orderOffsetY);	// 使用按钮
					yield return new EditorWaitForSeconds(0.3F);
					Debug.Log("确认按钮");
					Operation.Click(960, 700);	// 确认按钮
					ability.cooldownTime = DateTime.Now + new TimeSpan(ability.cooldownHours, 0, 10);
				} else {
					ability.cooldownTime = DateTime.Now + new TimeSpan(0, 0, RETRY_DELAY);
				}
				ABILITIES.RemoveAt(0);
				yield return null;
				bool inserted = false;
				for (int i = 0, length = ABILITIES.Count; i < length; ++i) {
					if (ability.cooldownTime < ABILITIES[i].cooldownTime) {
						ABILITIES.Insert(i, ability);
						inserted = true;
						break;
					}
					yield return null;
				}
				if (!inserted) {
					ABILITIES.Add(ability);
				}
				yield return new EditorWaitForSeconds(0.2F);
				if (Recognize.IsWindowCovered) {
					Debug.Log("关闭按钮");
					Operation.Click(1171, 190);	// 关闭按钮
				}
			}
			
			EndOfAutoCastCastleAbility:
			Task.CurrentTask = null;
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
