/*
 * @Author: wangyun
 * @CreateTime: 2023-09-23 01:57:34 542
 * @LastEditor: wangyun
 * @EditTime: 2023-09-23 01:57:34 547
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public static class AttackDisciple {
	private static int ATTACK_COUNT = 2;	// 攻击次数
	private static int GROUP_COUNT = 4;	// 拥有行军队列数
	private static int SQUAD_NUMBER = 4;	// 使用编队号码
	private static bool USE_SMALL_BOTTLE = false;	// 是否使用小体
	private static bool USE_BIG_BOTTLE = false;	// 是否使用大体
	
	private static EditorCoroutine s_CO;

	[MenuItem("Assets/StartAttackDisciple", priority = -1)]
	private static void Enable() {
		Disable();
		List<string> switches = new List<string> {
			$"拥有行军队列【{GROUP_COUNT}】",
			$"使用编队【{SQUAD_NUMBER}】"
		};
		if (USE_SMALL_BOTTLE) { switches.Add("【允许使用小体】"); }
		if (USE_BIG_BOTTLE) { switches.Add("【允许使用大体】"); }
		Debug.Log($"自动攻击第七使徒已开启，{string.Join("，", switches)}");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopAttackDisciple", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			Debug.Log("自动攻击第七使徒已关闭");
		}
	}

	private static IEnumerator Update() {
		int attackCount = 0;
		while (true) {
			switch (Recognize.CurrentScene) {
				case Recognize.Scene.ARMY_SELECTING:
					Debug.Log("可能是卡在出战界面了，执行返回");
					Click(30, 140);	// 左上角返回按钮
					break;
				// case Recognize.Scene.INSIDE:
				// 	Click(1170, 970);	// 右下角主城与世界切换按钮
				// 	break;
			}
			Debug.Log("当前忙碌队列数量: " + Recognize.BusyGroupCount);
			while (true) {
				if (Recognize.BusyGroupCount < GROUP_COUNT) {
					yield return new EditorWaitForSeconds(0.2F);
					if (Recognize.BusyGroupCount < GROUP_COUNT) {
						Debug.Log("当前忙碌队列数量: " + Recognize.BusyGroupCount);
						break;
					}
				}
				// 等待有队列空闲出来
				yield return null;
			}
			if (Recognize.IsWindowCovered) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("左上角返回按钮");
				do {
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.2F);
				} while (Recognize.IsWindowCovered);
			}
			if (Recognize.CurrentScene != Recognize.Scene.OUTSIDE) {
				Debug.Log("已不在世界界面，重新开始");
				continue;
			}
			
			// 开始打第七使徒
			if (Recognize.IsOutsideFaraway) {	// 如果是世界界面远景，则没有显示活动按钮，需要先切换到近景
				Vector2Int oldPos = MouseUtils.GetMousePos();
				MouseUtils.SetMousePos(960, 540);	// 鼠标移动到屏幕中央
				for (int i = 0; i < 40; ++i) {
					MouseUtils.ScrollWheel(1);
					yield return new EditorWaitForSeconds(0.01F);
				}
				MouseUtils.SetMousePos(oldPos.x, oldPos.y);
				yield return new EditorWaitForSeconds(0.1F);
			}
			Click(1880, 440);	// 活动按钮
			yield return new EditorWaitForSeconds(0.1F);
			Click(960, 480);	// 第七使徒按钮
			yield return new EditorWaitForSeconds(0.1F);
			Click(960, 920);	// 攻击使徒按钮
			yield return new EditorWaitForSeconds(0.1F);
			Click(960, 730);	// 攻击按钮
			
			// MouseUtils.SetMousePos(960, 540);	// 鼠标移动到屏幕中央
			// for (int i = 0; i < 40; ++i) {
			// 	MouseUtils.ScrollWheel(1);
			// 	yield return new EditorWaitForSeconds(0.01F);
			// }
			// yield return new EditorWaitForSeconds(0.3F);
			// while (Recognize.IsOutsideNearby) {
			// 	MouseUtils.ScrollWheel(-1);
			// 	yield return new EditorWaitForSeconds(0.1F);
			// }
			// yield return new EditorWaitForSeconds(0.3F);
			// Click(92, 255);	// 点击地图位置
			// yield return new EditorWaitForSeconds(0.1F);
			// Click(1150, 650);	// 选中使徒
			// yield return new EditorWaitForSeconds(0.1F);
			// Click(960, 730);	// 攻击按钮
			
			yield return new EditorWaitForSeconds(0.3F);
			if (Recognize.IsEnergyShortcutAdding) {
				if (USE_SMALL_BOTTLE) {
					Debug.Log("嗑小体");
					Click(830, 590);	// 选中小体
				} else if (USE_BIG_BOTTLE) {
					Debug.Log("嗑大体");
					Click(830, 590);	// 选中大体
				} else {
					yield break;
				}
				yield return new EditorWaitForSeconds(0.1F);
				Click(960, 702);	// 使用按钮
				yield return new EditorWaitForSeconds(0.1F);
				Click(1170, 384);	// 关闭按钮
				yield return new EditorWaitForSeconds(0.1F);
				Click(1150, 650);	// 选中使徒
				yield return new EditorWaitForSeconds(0.1F);
				Click(960, 730);	// 攻击按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			Click(1145 + 37 * SQUAD_NUMBER, 870);	// 选择队列
			yield return new EditorWaitForSeconds(0.2F);
			Click(960, 470);	// 出战按钮
			attackCount++;
			Debug.Log("出发");
			yield return new EditorWaitForSeconds(0.1F);
			while (Recognize.IsWindowCovered) {
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			if (attackCount >= ATTACK_COUNT) {
				Debug.Log($"已攻击{attackCount}次，自动攻击已结束");
				yield break;
			}
			
			// 休息5秒，避免出错时一直受控不能操作
			yield return new EditorWaitForSeconds(5);
		}
		// ReSharper disable once IteratorNeverReturns
	}

	private static void Click(int x, int y) {
		Vector2Int oldPos = MouseUtils.GetMousePos();
		MouseUtils.SetMousePos(x, y);
		MouseUtils.LeftDown();
		MouseUtils.LeftUp();
		MouseUtils.SetMousePos(oldPos.x, oldPos.y);
	}
	
	// [MenuItem("Assets/AttackDisciple.Test", priority = -1)]
	private static void Test() {
		EditorCoroutineManager.StartCoroutine(IETest());
	}

	private static IEnumerator IETest() {
		if (Recognize.IsOutsideFaraway) {
			Vector2Int oldPos = MouseUtils.GetMousePos();
			MouseUtils.SetMousePos(960, 540);	// 鼠标移动到屏幕中央
			for (int i = 0; i < 40; ++i) {
				MouseUtils.ScrollWheel(1);
				yield return new EditorWaitForSeconds(0.01F);
			}
			MouseUtils.SetMousePos(oldPos.x, oldPos.y);
		}
		Click(1880, 440);	// 活动按钮
		yield return new EditorWaitForSeconds(0.1F);
		Click(960, 480);	// 第七使徒按钮
		yield return new EditorWaitForSeconds(0.1F);
		Click(960, 920);	// 攻击使徒按钮
		yield return new EditorWaitForSeconds(0.1F);
		Click(960, 730);	// 攻击按钮
		// yield return new EditorWaitForSeconds(0.3F);
		// Click(1145 + 37 * SQUAD_NUMBER, 870);	// 选择队列
		// yield return new EditorWaitForSeconds(0.2F);
		// Click(30, 140);	// 左上角返回按钮
		// yield return new EditorWaitForSeconds(0.1F);
		// Click(720, 128);	// 左上角返回按钮
		// yield return new EditorWaitForSeconds(0.1F);
		// Click(720, 128);	// 左上角返回按钮
	}
}
