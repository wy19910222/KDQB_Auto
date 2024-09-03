/*
 * @Author: wangyun
 * @CreateTime: 2023-10-12 00:00:51 751
 * @LastEditor: wangyun
 * @EditTime: 2023-10-12 00:00:51 756
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public struct GatherTarget {
	public int count;
	public bool willReset;
	public int levelOffset;
	public int squadNumber;
	public bool mustFullSoldiers;
}

public class Gather {
	public static bool Test { get; set; } // 测试模式
	
	public static int RESERVED_ENERGY = 60;	// 保留体力值
	public static bool KEEP_ENERGY_NOT_FULL = true;	// 保持体力不满状态
	public static float UNATTENDED_DURATION = 5;	// 等待无操作时长
	
	public static readonly List<GatherTarget> TARGET_LIST = new() {
		new GatherTarget {count = 0, willReset = false, levelOffset = 0, squadNumber = 1},
		new GatherTarget {count = 0, willReset = true, levelOffset = 0, squadNumber = 1},
		new GatherTarget {count = 0, willReset = false, levelOffset = 0, squadNumber = 1},
	};	// 攻击目标随机范围
	
	public static readonly Dictionary<Recognize.EnergyShortcutAddingType, int> USE_BOTTLE_DICT = new ();	// 是否自动补充体力
	
	public static DateTime LAST_RESET_TIME;	// 上次重置时间
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartGather", priority = -1)]
	private static void Enable() {
		Disable();
		List<string> switches = new List<string>();
		if (!USE_BOTTLE_DICT.Values.ToList().Exists(count => count > 0)) {
			switches.Add($"保留体力值【{RESERVED_ENERGY}】");
		}
		if (UNATTENDED_DURATION > 0) {
			switches.Add($"等待无操作【{UNATTENDED_DURATION}】秒");
		}
		{
			List<string> targets = new List<string>();
			for (int i = 0, length = TARGET_LIST.Count; i < length; ++i) {
				int attackCount = TARGET_LIST[i].count;
				if (attackCount > 0) {
					targets.Add($"第{i + 1}个{attackCount}次");
				}
			}
			switches.Add($"目标【{string.Join("、", targets)}】");
		}
		foreach (var (type, count) in USE_BOTTLE_DICT) {
			if (count > 0) {
				switches.Add($"【{Utils.GetEnumInspectorName(type)}{count}次】");
			}
		}
		Debug.Log($"自动集结已开启，{string.Join("，", switches)}");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopGather", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动集结已关闭");
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
			
			// 每天10次惧星
			DateTime date = DateTime.Now.Date;
			if (LAST_RESET_TIME < date) {
				for (int i = 0, length = TARGET_LIST.Count; i < length; ++i) {
					GatherTarget _target = TARGET_LIST[i];
					if (_target.willReset) {
						_target.count = GetDailyCount(i);
						TARGET_LIST[i] = _target;
					}
				}
				LAST_RESET_TIME = date;
			}
			
			if (GlobalStatus.UnattendedDuration < UNATTENDED_DURATION * 1000_000_0) {
				// Debug.Log("正在做其他操作");
				continue;
			}
			
			// 确定攻击目标
			int targetIndex = RandomTarget();
			if (targetIndex == -1) {
				// Debug.Log("未选择攻击目标，取消操作");
				continue;
			}
			GatherTarget target = TARGET_LIST[targetIndex];
			
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
				Debug.Log("测试模式，忽略体力与队列数量");
			} else {
				// 体力值
				if (USE_BOTTLE_DICT.Values.All(count => count <= 0)) {
					if (!KEEP_ENERGY_NOT_FULL || Recognize.Energy < Global.ENERGY_FULL - 1) {
						int cost = Global.DAN_EXIST ? 8 : 10;
						if (Recognize.Energy < RESERVED_ENERGY + cost) {
							// Debug.Log($"当前体力：{Recognize.Energy}");
							continue;
						}
					}
				}
				// 队列数量
				if (!Recognize.IsAnyGroupIdle) {
					// Debug.Log($"忙碌队列：{Recognize.BusyGroupCount}");
					continue;
				}
				// 存在目标队列英雄头像
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
				// 存在目标队列英雄头像
				if (Recognize.GetHeroGroupNumber(Global.GetLeader(target.squadNumber)) >= 0) {
					// Debug.Log($"存在打野英雄头像");
					continue;
				}
			}
			
			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(Gather);
			
			// 开始集结
			for (int i = 0; i < 5 && !Recognize.IsSearching; i++) {
				Debug.Log("搜索按钮");
				Operation.Click(750, 970);	// 搜索按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			yield return new EditorWaitForSeconds(0.2F);
			Debug.Log("集结按钮");
			Operation.Click(1024, 512);	// 集结按钮
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
			bool isGatherFearStar = Recognize.IsGatherFearStar;
			// if (isGatherFearStar) {
			// 	Debug.Log("惧星等级滑块");
			// 	Operation.Click(844 + 44 * FEAR_STAR_LEVEL, 880);	// 惧星等级滑块
			// } else {
			// 	Debug.Log("其他等级滑块");
			// 	Operation.Click(1062, 880);	// 其他等级滑块
			// 	for (int i = 0; i > TARGET_LEVEL_OFFSET; --i) {
			// 		yield return new EditorWaitForSeconds(0.1F);
			// 		Operation.Click(822, 880);	// 其他等级滑块
			// 	}
			// }
			Debug.Log("等级滑块");
			if (target.levelOffset > 0) {
				Operation.Click(UnityEngine.Random.Range(858, 1062), 878);	// 随机滑块
			} else {
				Operation.Click(1062, 878);	// 先拉满滑块
				for (int i = 0; i > target.levelOffset; --i) {
					yield return new EditorWaitForSeconds(0.1F);
					Operation.Click(822, 878);	// 再降等级
				}
			}
			yield return new EditorWaitForSeconds(0.1F);
			
			Debug.Log("搜索按钮");
			Operation.Click(960, 940);	// 搜索按钮
			yield return new EditorWaitForSeconds(0.3F);
			// 搜索面板未消失，说明未搜索到
			if (Recognize.IsSearching) {
				Debug.Log("未搜到，关闭搜索面板");
				while (Recognize.IsSearching) {
					// 点击空白处退出搜索面板
					Operation.Click(660, 970);	// 点击空白处
					yield return new EditorWaitForSeconds(0.3F);
				}
				goto EndOfGather;
			}

			// 搜索面板消失，说明搜索到了
			// 避免没刷出来，先等一会儿
			yield return new EditorWaitForSeconds(0.3F);
			Debug.Log("已搜到，选中目标");
			Operation.Click(960, 560);	// 选中目标
			yield return new EditorWaitForSeconds(0.2F);
			if (isGatherFearStar) {
				Debug.Log("惧星气泡里的集结按钮");
				Operation.Click(870, 830);	// 集结按钮
				yield return new EditorWaitForSeconds(0.3F);
				if (!Recognize.IsEnergyShortcutAdding && Recognize.CurrentScene != Recognize.Scene.FIGHTING_MARCH) {
					// 不同视角距离按钮位置会不一样，所以尝试两个不同的位置
					Debug.Log("惧星气泡里的集结按钮");
					Operation.Click(870, 850);	// 集结按钮
					yield return new EditorWaitForSeconds(0.3F);
				}
			} else {
				Debug.Log("其他气泡里的集结按钮");
				Operation.Click(1050, 450);	// 集结按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			
			// 出现体力不足面板
			if (Recognize.IsEnergyShortcutAdding) {
				// 快捷嗑药
				Recognize.EnergyShortcutAddingType useBottle = RandomUseBottle();	// 随机使用大小体
				Debug.Log(Utils.GetEnumInspectorName(useBottle));
				int i = 0;
				int iMax = useBottle switch {
					Recognize.EnergyShortcutAddingType.SMALL_BOTTLE => 3,
					Recognize.EnergyShortcutAddingType.BIG_BOTTLE => 1,
					Recognize.EnergyShortcutAddingType.DIAMOND_BUY => 1,
					_ => 0
				};
				while (Recognize.IsEnergyShortcutAdding && i < iMax && USE_BOTTLE_DICT[useBottle] > 0) {
					List<Recognize.EnergyShortcutAddingType> types = Recognize.GetShortcutTypes();
					int index = types.IndexOf(useBottle);
					if (index != -1) {
						Debug.Log($"选择{index + 1}号位");
						Operation.Click(828 + index * 130, 590);	// 选中图标
						yield return new EditorWaitForSeconds(0.1F);
						if (!test) {
							Operation.Click(960, 702);	// 使用按钮
						}
						USE_BOTTLE_DICT[useBottle]--;
						yield return new EditorWaitForSeconds(0.1F);
					} else {
						Debug.LogError("体力药剂数量不足！");
					}
					Operation.Click(1170, 384);	// 关闭按钮
					yield return new EditorWaitForSeconds(0.3F);
					Operation.Click(960, 580);	// 选中目标
					yield return new EditorWaitForSeconds(0.1F);
					Operation.Click(870, 430);	// 集结按钮
					yield return new EditorWaitForSeconds(0.3F);
					i++;
				}
				if (Recognize.IsEnergyShortcutAdding) {
					Operation.Click(1170, 384);	// 关闭按钮
					Debug.Log("体力不足，等待稍后尝试");
					yield return new EditorWaitForSeconds(300);
				}
			}
			if (Recognize.CurrentScene == Recognize.Scene.FIGHTING_MARCH) {
				Debug.Log("选择编队");
				Operation.Click(1145 + 37 * target.squadNumber, 870);	// 选择编队
				yield return new EditorWaitForSeconds(0.2F);
				if (!test && (!target.mustFullSoldiers || Recognize.FightingSoldierCountPercent > 0.99F) && Recognize.FightingHeroEmptyCount <= 0) {
					Debug.Log("出发");
					Operation.Click(960, 470);	// 出战按钮
					--target.count;
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
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.3F);
				}
			}
			
			EndOfGather:
			Task.CurrentTask = null;
			
			// 休息5秒，避免出错时一直受控不能操作
			yield return new EditorWaitForSeconds(5);
		}
		// ReSharper disable once IteratorNeverReturns
	}
	
	private static int RandomTarget() {
		List<int> list = new List<int>();
		for (int i = 0, length = TARGET_LIST.Count; i < length; ++i) {
			if (TARGET_LIST[i].count > 0) {
				list.Add(i);
			}
		}
		if (list.Count <= 0) {
			return -1;
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}
	
	private static Recognize.EnergyShortcutAddingType RandomUseBottle() {
		List<Recognize.EnergyShortcutAddingType> list = new List<Recognize.EnergyShortcutAddingType>();
		if (USE_BOTTLE_DICT.TryGetValue(Recognize.EnergyShortcutAddingType.BIG_BOTTLE, out int bigCount) && bigCount > 0) {
			list.Add(Recognize.EnergyShortcutAddingType.BIG_BOTTLE);
		}
		if (USE_BOTTLE_DICT.TryGetValue(Recognize.EnergyShortcutAddingType.SMALL_BOTTLE, out int smallCount) && smallCount > 0) {
			list.Add(Recognize.EnergyShortcutAddingType.SMALL_BOTTLE);
		}
		if (USE_BOTTLE_DICT.TryGetValue(Recognize.EnergyShortcutAddingType.DIAMOND_BUY, out int buyCount) && buyCount > 0) {
			list.Add(Recognize.EnergyShortcutAddingType.DIAMOND_BUY);
		}
		return list.Count > 0 ? list[UnityEngine.Random.Range(0, list.Count)] : Recognize.EnergyShortcutAddingType.NONE;
	}
}
