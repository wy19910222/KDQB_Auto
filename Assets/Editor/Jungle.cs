/*
 * @Author: wangyun
 * @CreateTime: 2023-09-07 20:18:29 730
 * @LastEditor: wangyun
 * @EditTime: 2023-09-07 20:18:29 842
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public static class Jungle {
	private static int GROUP_COUNT = 4;	// 拥有行军队列数
	private static int JUNGLE_STAR = 4;	// 打的黑暗机甲星级
	private static int SQUAD_NUMBER = 1;	// 使用编队号码
	private static bool USE_RANDOM_BOTTLE = false;	// 随机使用大体或小体
	private static bool USE_SMALL_BOTTLE = true;	// 是否使用小体（未开随机时生效）
	private static bool USE_BIG_BOTTLE = false;	// 是否使用大体（未开随机时生效）
	
	private static EditorCoroutine s_CO;

	[MenuItem("Assets/StartJungle", priority = -1)]
	private static void Enable() {
		Disable();
		List<string> switches = new List<string>();
		switches.Add($"拥有行军队列【{GROUP_COUNT}】");
		switches.Add($"目标【黑暗机甲{JUNGLE_STAR}星】");
		switches.Add($"使用编队【{SQUAD_NUMBER}】");
		if (USE_RANDOM_BOTTLE) {
			switches.Add("【随机使用大体或小体】");
		} else {
			if (USE_SMALL_BOTTLE) { switches.Add("【允许使用小体】"); }
			if (USE_BIG_BOTTLE) { switches.Add("【允许使用大体】"); }
		}
		Debug.Log($"自动打野已开启，{string.Join("，", switches)}");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopJungle", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			Debug.Log("自动打野已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			switch (Recognize.CurrentScene) {
				case Recognize.Scene.ARMY_SELECTING:
					Debug.Log("可能是卡在出战界面了，执行返回");
					Operation.Click(50, 130);	// 左上角返回按钮
					break;
				// case Recognize.Scene.INSIDE:
				// 	Operation.Click(1170, 970);	// 右下角主城与世界切换按钮
				// 	break;
			}
			Debug.Log("等待切换到世界界面且无窗口覆盖");
			// 等待切换到世界界面
			while (Recognize.CurrentScene != Recognize.Scene.OUTSIDE) {
				yield return null;
			}
			Debug.Log("当前忙碌队列数量: " + Recognize.BusyGroupCount);
			while (true) {
				if (Recognize.BusyGroupCount < GROUP_COUNT && Recognize.GetYLKGroupNumber() < 0) {
					yield return new EditorWaitForSeconds(0.2F);
					if (Recognize.BusyGroupCount < GROUP_COUNT && Recognize.GetYLKGroupNumber() < 0) {
						Debug.Log("当前忙碌队列数量: " + Recognize.BusyGroupCount);
						break;
					}
				}
				// 等待有队列空闲出来且没有橙色英雄队伍（无法判断打野队伍，只能判断是否是橙色了）
				yield return null;
			}
			if (Recognize.IsWindowCovered) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(735, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.3F);
				Operation.Click(735, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.3F);
				Operation.Click(735, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.3F);
				Operation.Click(735, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			if (Recognize.CurrentScene != Recognize.Scene.OUTSIDE) {
				Debug.Log("已不在世界界面，重新开始");
				continue;
			}
			// 开始打野
			while (!Recognize.IsSearching) {
				// Debug.Log("搜索按钮");
				Operation.Click(750, 970);	// 搜索按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			// Debug.Log("敌军按钮");
			Operation.Click(770, 510);	// 敌军按钮
			yield return new EditorWaitForSeconds(0.1F);
			// Debug.Log("列表往左拖动");
			var ie = Operation.Drag(1120, 670, 790, 670, 0.2F);	// 列表往左拖动
			while (ie.MoveNext()) {
				yield return ie.Current;
			}
			yield return new EditorWaitForSeconds(0.3F);
			// Debug.Log("选中最后一个（黑暗机甲）");
			Operation.Click(1120, 670);	// 选中最后一个（黑暗机甲）
			yield return new EditorWaitForSeconds(0.1F);
			// Debug.Log("星级滑块");
			Operation.Click(810 + 50 * JUNGLE_STAR, 880);	// 星级滑块
			yield return new EditorWaitForSeconds(0.1F);
			// Debug.Log("搜索按钮");
			Operation.Click(960, 940);	// 搜索按钮
			yield return new EditorWaitForSeconds(0.2F);
			// 搜索面板消失，说明搜索到了
			if (!Recognize.IsSearching) {
				Operation.Click(960, 580);	// 选中目标
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(870, 430);	// 攻击5次按钮
				yield return new EditorWaitForSeconds(0.3F);
				// 打开背包嗑小体
				if (Recognize.IsEnergyAdding) {
					Operation.Click(1170, 384);	// 关闭按钮
					yield return new EditorWaitForSeconds(0.3F);
					Operation.Click(1870, 870);	// 背包按钮
					yield return new EditorWaitForSeconds(0.1F);
					// 判断小体是否在第二格
					Color32 targetColor1 = new Color32(129, 242, 25, 255);
					Color32 realColor1 = ScreenshotUtils.GetColorOnScreen(847, 305);
					Color32 targetColor2 = new Color32(248, 210, 22, 255);
					Color32 realColor2 = ScreenshotUtils.GetColorOnScreen(866, 281);
					if (Recognize.Approximately(realColor1, targetColor1) && Recognize.Approximately(realColor2, targetColor2)) {
						Operation.Click(866, 281);	// 选中小体
						yield return new EditorWaitForSeconds(0.1F);
						Operation.Click(960, 960);	// 使用按钮
						yield return new EditorWaitForSeconds(0.1F);
					}
					Operation.Click(735, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.1F);
					Operation.Click(960, 580);	// 选中目标
					yield return new EditorWaitForSeconds(0.1F);
					Operation.Click(870, 430);	// 攻击5次按钮
					yield return new EditorWaitForSeconds(0.3F);
				}
				// 快捷嗑药
				int useBottle = 0;
				if (USE_RANDOM_BOTTLE) {
					int randomValue = Random.Range(0, 2);
					Debug.Log("RandomValue: " + randomValue);
					useBottle = Random.Range(0, 2) > 0 ? 1 : 2;
				} else if (USE_BIG_BOTTLE) {
					useBottle = 2;
				} else if (USE_SMALL_BOTTLE) {
					useBottle = 1;
				}
				if (useBottle == 0) {
					if (Recognize.IsEnergyAdding) {
						yield return new EditorWaitForSeconds(0.1F);
						Operation.Click(1170, 384);	// 关闭按钮
						Debug.Log("体力不足，等待5分钟后再尝试");
						yield return new EditorWaitForSeconds(300);
						continue;
					}
				} else {
					bool willContinue = false;
					int i = 0;
					while (Recognize.IsEnergyAdding) {
						switch (useBottle) {
							case 1:
								if (i < 3) {
									Debug.Log("嗑小体");
									Operation.Click(830, 590);	// 选中小体
									yield return new EditorWaitForSeconds(0.1F);
									Operation.Click(960, 702);	// 使用按钮
								} else {
									Debug.LogError("连续嗑了3瓶小体还是体力不足！");
									willContinue = true;
								}
								break;
							case 2:
								if (i < 1) {
									Debug.Log("嗑大体");
									Operation.Click(960, 590);	// 选中大体
									yield return new EditorWaitForSeconds(0.1F);
									Operation.Click(960, 702);	// 使用按钮
								} else {
									Debug.LogError("嗑了大体还是体力不足！");
									willContinue = true;
								}
								break;
						}
						yield return new EditorWaitForSeconds(0.1F);
						Operation.Click(1170, 384);	// 关闭按钮
						if (willContinue) {
							break;
						}
						yield return new EditorWaitForSeconds(0.3F);
						Operation.Click(960, 580);	// 选中目标
						yield return new EditorWaitForSeconds(0.1F);
						Operation.Click(870, 430);	// 攻击5次按钮
						yield return new EditorWaitForSeconds(0.3F);
						i++;
					}
					if (willContinue) {
						Debug.Log("等待5分钟后再尝试");
						yield return new EditorWaitForSeconds(300);
						continue;
					}
				}
				Operation.Click(1145 + 37 * SQUAD_NUMBER, 870);	// 选择队列
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(960, 470);	// 出战按钮
				Debug.Log("出发");
			}
			
			// 休息5秒，避免出错时一直受控不能操作
			yield return new EditorWaitForSeconds(5);
		}
		// ReSharper disable once IteratorNeverReturns
	}
	
	// [MenuItem("Assets/Jungle.Test", priority = -1)]
	private static void Test() {
		EditorCoroutineManager.StartCoroutine(IETest());
	}

	private static IEnumerator IETest() {
		Operation.Click(1870, 870);	// 背包按钮
		yield return new EditorWaitForSeconds(1F);
		Operation.Click(860, 290);	// 选中小体
		yield return new EditorWaitForSeconds(1F);
		// Operation.Click(960, 960);	// 使用按钮
		Operation.MouseMove(960, 960);
		yield return new EditorWaitForSeconds(1F);
		Operation.Click(735, 128);	// 左上角返回按钮
	}
}
