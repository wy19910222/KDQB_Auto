/*
 * @Author: wangyun
 * @CreateTime: 2024-07-22 23:52:59 745
 * @LastEditor: wangyun
 * @EditTime: 2024-07-22 23:52:59 752
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public struct ImperialTreasureTarget {
	public int weight;
	public int levelOffset;
	public int squadNumber;
	public bool mustFullSoldiers;
}

public class ImperialTreasure {
	public static string[] TARGET_NAMES = new[] { "陆军", "海军", "空军" };
	
	public static bool Test { get; set; } // 测试模式
	
	public static float UNATTENDED_DURATION = 5;	// 等待无操作时长
	public static int INTERVAL = 12;	// 每12小时1次
	public static DateTime NEXT_DT = DateTime.Now;	// 下一次时间
	public static readonly List<ImperialTreasureTarget> TARGET_LIST = new() {
		new ImperialTreasureTarget {weight = 1, levelOffset = 0, squadNumber = 1},
		new ImperialTreasureTarget {weight = 1, levelOffset = 0, squadNumber = 1},
		new ImperialTreasureTarget {weight = 1, levelOffset = 0, squadNumber = 1},
	};	// 攻击目标随机范围
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartImperialTreasure", priority = -1)]
	private static void Enable() {
		Disable();
		List<string> switches = new List<string>();
		if (UNATTENDED_DURATION > 0) {
			switches.Add($"等待无操作【{UNATTENDED_DURATION}】秒");
		}
		{
			List<string> targets = new List<string>();
			for (int i = 0, length = TARGET_LIST.Count; i < length; ++i) {
				int attackCount = TARGET_LIST[i].weight;
				if (attackCount > 0) {
					targets.Add(TARGET_NAMES[i]);
				}
			}
			switches.Add($"目标【{string.Join("、", targets)}】");
		}
		Debug.Log($"自动打帝国宝藏已开启，{string.Join("，", switches)}");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopImperialTreasure", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动打帝国宝藏已关闭");
		}
	}

	public static int GetDailyCount(int i) {
		return i switch {
			1 => 10,
			_ => 0
		};
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			
			if (GlobalStatus.UnattendedDuration < UNATTENDED_DURATION * 1000_000_0) {
				// Debug.Log("正在做其他操作");
				continue;
			}
			
			if (DateTime.Now < NEXT_DT) {
				// Debug.Log("等待下次攻击时间");
				continue;
			}
			
			// 确定攻击目标
			int targetIndex = RandomTarget();
			if (targetIndex == -1) {
				// Debug.Log("未选择攻击目标，取消操作");
				continue;
			}
			ImperialTreasureTarget target = TARGET_LIST[targetIndex];
			
			if (Recognize.CurrentScene != Recognize.Scene.OUTSIDE_NEARBY && Recognize.CurrentScene != Recognize.Scene.OUTSIDE_FARAWAY) {
				// Debug.Log("不在世界场景");
				continue;
			}
			if (Recognize.IsWindowCovered) {
				// Debug.Log("有窗口打开着，正在做其他操作");
				continue;
			}
			
			bool test = Test;
			if (test) {
				Debug.Log("测试模式，忽略队列数量");
			} else {
				// 队列数量
				if (!Recognize.IsAnyGroupIdle) {
					// Debug.Log($"忙碌队列：{Recognize.BusyGroupCount}");
					continue;
				}
				// 存在打帝国宝藏英雄头像
				if (Recognize.GetHeroGroupNumber(Global.GetLeader(target.squadNumber)) >= 0) {
					// Debug.Log($"存在打野英雄头像");
					continue;
				}
				// 可能处于世界场景远近景切换的动画过程中，所以等待0.2秒再判断一次
				yield return new EditorWaitForSeconds(0.2F);
				// 队列数量
				if (!Recognize.IsAnyGroupIdle) {
					// Debug.Log($"忙碌队列：{Recognize.BusyGroupCount}");
					continue;
				}
				// 存在打帝国宝藏英雄头像
				if (Recognize.GetHeroGroupNumber(Global.GetLeader(target.squadNumber)) >= 0) {
					// Debug.Log($"存在打野英雄头像");
					continue;
				}
			}
			
			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(ImperialTreasure);
			
			// 开始打帝国宝藏
			// 如果是世界界面远景，则没有显示活动按钮，需要先切换到近景
			for (int i = 0; i < 5 && !Recognize.IsSearching; i++) {
				Debug.Log("搜索按钮");
				Operation.Click(750, 970);	// 搜索按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			Debug.Log("帝国宝藏按钮");
			Operation.Click(1150, 512);	// 帝国宝藏按钮
			yield return new EditorWaitForSeconds(0.1F);
			const int TARGET_WIDTH = 163;
			Debug.Log("攻击目标: " + targetIndex);
			{
				// 先拖动到列表最开头，以便计算
				var ie = Operation.NoInertiaDrag(803, 672, 803 + TARGET_WIDTH * (TARGET_LIST.Count - 2), 672);
				while (ie.MoveNext()) {
					yield return ie.Current;
				}
				yield return new EditorWaitForSeconds(0.5F);
			}
			Debug.Log("拖动以显示攻击目标");
			int orderOffsetX = (targetIndex - 2) * TARGET_WIDTH;
			while (orderOffsetX > 0) {
				int dragDistance = Mathf.Min(TARGET_WIDTH * 3, orderOffsetX);
				// 往左拖动
				var ie = Operation.NoInertiaDrag(1129, 672, 1129 - dragDistance, 672);
				while (ie.MoveNext()) {
					yield return ie.Current;
				}
				yield return new EditorWaitForSeconds(0.2F);
				orderOffsetX -= dragDistance;
			}
			Operation.Click(1129 + orderOffsetX, 672);	// 选中目标
			yield return new EditorWaitForSeconds(0.1F);
			Debug.Log("等级滑块");
			Operation.Click(1062, 880);	// 先拉满滑块
			for (int i = 0; i > target.levelOffset; --i) {
				yield return new EditorWaitForSeconds(0.1F);
				Operation.Click(822, 880);	// 再降等级
			}
			yield return new EditorWaitForSeconds(0.1F);
			
			Debug.Log("搜索按钮");
			Operation.Click(960, 940);	// 搜索按钮
			yield return new EditorWaitForSeconds(0.2F);
			// 搜索面板未消失，说明未搜索到
			if (!Recognize.IsSearching) {
				// 搜索面板消失，说明搜索到了
				// 避免没刷出来，先等一会儿
				yield return new EditorWaitForSeconds(0.3F);
				Debug.Log("已搜到，选中目标");
				Operation.Click(960, 560);	// 选中目标
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("气泡里的攻击按钮");
				Operation.Click(960, 800);	// 攻击按钮
				yield return new EditorWaitForSeconds(0.3F);
				if (!Recognize.IsEnergyShortcutAdding && Recognize.CurrentScene != Recognize.Scene.FIGHTING_MARCH) {
					// 不同视角距离按钮位置会不一样，所以尝试两个不同的位置
					Debug.Log("气泡里的攻击按钮");
					Operation.Click(960, 830);	// 攻击按钮
					yield return new EditorWaitForSeconds(0.3F);
				}
				
				if (Recognize.CurrentScene == Recognize.Scene.FIGHTING_MARCH) {
					Debug.Log("选择编队");
					Operation.Click(1145 + 37 * target.squadNumber, 870);	// 选择编队
					yield return new EditorWaitForSeconds(0.2F);
					if (!test && (!target.mustFullSoldiers || Recognize.FightingSoldierCountPercent > 0.99F) && Recognize.FightingHeroEmptyCount <= 0) {
						Debug.Log("出发");
						Operation.Click(960, 470);	// 出战按钮
						TARGET_LIST[targetIndex] = target;
						yield return new EditorWaitForSeconds(0.3F);
					} else {
						Debug.Log("退出按钮");
						Operation.Click(30, 140);	// 退出按钮
						yield return new EditorWaitForSeconds(0.2F);
						Debug.Log("确认退出按钮");
						Operation.Click(1064, 634);	// 确认退出按钮
						yield return new EditorWaitForSeconds(0.3F);
					}
				} else {
					for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
						Debug.Log("关闭窗口");
						Operation.Click(1168, 240);	// 右上角叉叉
						yield return new EditorWaitForSeconds(0.3F);
					}
					NEXT_DT += new TimeSpan(0, INTERVAL, 0, 0);
				}
			} else {
				Debug.Log("未搜到，关闭搜索面板");
				while (Recognize.IsSearching) {
					// 点击空白处退出搜索面板
					Operation.Click(660, 970);	// 点击空白处
					yield return new EditorWaitForSeconds(0.3F);
				}
			}
			
			Task.CurrentTask = null;
			
			// 休息5秒，避免出错时一直受控不能操作
			yield return new EditorWaitForSeconds(5);
		}
		// ReSharper disable once IteratorNeverReturns
	}
	
	private static int RandomTarget() {
		int totalWeight = 0;
		for (int i = 0, length = TARGET_LIST.Count; i < length; ++i) {
			int weight = TARGET_LIST[i].weight;
			if (weight > 0) {
				totalWeight += weight;
			}
		}
		int random = UnityEngine.Random.Range(0, totalWeight);
		for (int i = 0, length = TARGET_LIST.Count; i < length; ++i) {
			int weight = TARGET_LIST[i].weight;
			if (weight > 0) {
				random -= weight;
				if (random < 0) {
					return i;
				}
			}
		}
		return -1;
	}
}
