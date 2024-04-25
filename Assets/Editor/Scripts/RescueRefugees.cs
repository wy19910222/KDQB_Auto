/*
 * @Author: wangyun
 * @CreateTime: 2024-04-17 00:42:49 023
 * @LastEditor: wangyun
 * @EditTime: 2024-04-17 00:42:49 030
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RescueRefugees {
	public static bool Test { get; set; } // 测试模式
	
	public static int RESERVED_ENERGY = 60;	// 保留体力值
	public static bool KEEP_ENERGY_NOT_FULL = true;	// 保持体力不满状态
	public static bool DAN_EXIST = true;	// 是否有戴安娜
	public static float UNATTENDED_DURATION = 5;	// 等待无操作时长
	
	public static int ACTIVITY_ORDER = 4;	// 活动排序
	public static int ATTACK_COUNT = 0;	// 攻击目标次数
	public static int SQUAD_NUMBER = 1;	// 使用编队号码
	public static Recognize.HeroType HERO_AVATAR = Recognize.HeroType.MRX;	// 集结英雄头像
	public static readonly Dictionary<Recognize.EnergyShortcutAddingType, int> USE_BOTTLE_DICT = new Dictionary<Recognize.EnergyShortcutAddingType, int>();	// 是否自动补充体力
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartRescueRefugees", priority = -1)]
	private static void Enable() {
		Disable();
		List<string> switches = new List<string>();
		if (!USE_BOTTLE_DICT.Values.ToList().Exists(count => count > 0)) {
			switches.Add($"保留体力值【{RESERVED_ENERGY}】");
		}
		if (UNATTENDED_DURATION > 0) {
			switches.Add($"等待无操作【{UNATTENDED_DURATION}】秒");
		}
		switches.Add($"使用编队【{SQUAD_NUMBER}】");
		foreach (var (type, count) in USE_BOTTLE_DICT) {
			if (count > 0) {
				switches.Add($"【{Utils.GetEnumInspectorName(type)}{count}次】");
			}
		}
		Debug.Log($"自动拯救难民已开启，{string.Join("，", switches)}");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopRescueRefugees", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动拯救难民已关闭");
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
			
			if (ATTACK_COUNT <= 0) {
				// Debug.Log("无攻击次数，取消操作");
				continue;
			}
			
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
					if (!KEEP_ENERGY_NOT_FULL || Recognize.energy < Recognize.ENERGY_FULL - 1) {
						int cost = DAN_EXIST ? 3 : 5;
						if (Recognize.energy < RESERVED_ENERGY + cost) {
							// Debug.Log($"当前体力：{Recognize.energy}");
							continue;
						}
					}
				}
				// 队列数量
				if (Recognize.BusyGroupCount >= Recognize.GROUP_COUNT) {
					// Debug.Log($"忙碌队列：{Recognize.BusyGroupCount}");
					continue;
				}
				// 存在拯救难民英雄头像
				if (Recognize.GetHeroGroupNumber(HERO_AVATAR) >= 0) {
					// Debug.Log($"存在打野英雄头像");
					continue;
				}
				// 可能处于世界场景远近景切换的动画过程中，所以等待0.2秒再判断一次
				yield return new EditorWaitForSeconds(0.2F);
				// 队列数量
				if (Recognize.BusyGroupCount >= Recognize.GROUP_COUNT) {
					// Debug.Log($"忙碌队列：{Recognize.BusyGroupCount}");
					continue;
				}
				// 存在拯救难民英雄头像
				if (Recognize.GetHeroGroupNumber(HERO_AVATAR) >= 0) {
					// Debug.Log($"存在打野英雄头像");
					continue;
				}
			}
			
			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(RescueRefugees);
			
			// 开始拯救难民
			// 如果是世界界面远景，则没有显示活动按钮，需要先切换到近景
			for (int i = 0; i < 50 && Recognize.IsOutsideFaraway; i++) {
				Vector2Int oldPos = MouseUtils.GetMousePos();
				MouseUtils.SetMousePos(960, 540);	// 鼠标移动到屏幕中央
				MouseUtils.ScrollWheel(1);
				MouseUtils.SetMousePos(oldPos.x, oldPos.y);
				yield return new EditorWaitForSeconds(0.1F);
			}
			Debug.Log("常规活动入口");
			Operation.Click(1880, 212);	// 常规活动入口
			yield return new EditorWaitForSeconds(0.5F);
			Debug.Log("拖动以显示活动标签页");
			{
				// 先拖动到列表最开头，以便计算
				var ie = Operation.NoInertiaDrag(1190 - 200, 200, 1190, 200);
				while (ie.MoveNext()) {
					yield return ie.Current;
				}
				yield return new EditorWaitForSeconds(0.3F);
			}
			const int TAB_WIDTH = 137;
			int orderOffsetX = (ACTIVITY_ORDER - 4) * TAB_WIDTH;
			while (orderOffsetX > 0) {
				const int dragDistance = TAB_WIDTH * 4;
				// 往左拖动
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

			int canUseSOS = Recognize.CanUseSOS;
			if (canUseSOS != 0) {
				if (canUseSOS == 3) {
					Debug.Log("前往按钮");
					Operation.Click(1120, 956);	// 前往按钮
					yield return new EditorWaitForSeconds(0.3F);
					Debug.Log("第一个难民营");
					Operation.Click(960, 300);	// 选中第一个难民营
					yield return new EditorWaitForSeconds(0.2F);
				} else {
					Debug.Log("使用求救信按钮");
					Operation.Click(1120, canUseSOS == 2 ? 868 : 956);	// 使用求救信按钮
					yield return new EditorWaitForSeconds(0.2F);
					if (Recognize.IsSOSExist) {
						Debug.Log("使用道具按钮");
						Operation.Click(960, 960); // 使用道具按钮
						yield return new EditorWaitForSeconds(0.2F);
					}
				}
				if (!Recognize.IsWindowCovered) {
					Debug.Log("选中目标");
					Operation.Click(960, 560);	// 选中目标
					yield return new EditorWaitForSeconds(0.2F);
					Debug.Log("气泡里的集结按钮");
					Operation.Click(1050, 450);	// 集结按钮
					yield return new EditorWaitForSeconds(0.3F);
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
							Debug.Log("关闭按钮");
							Operation.Click(1170, 384);	// 关闭按钮
							yield return new EditorWaitForSeconds(0.3F);
							Debug.Log("选中目标");
							Operation.Click(960, 560);	// 选中目标
							yield return new EditorWaitForSeconds(0.2F);
							Debug.Log("气泡里的集结按钮");
							Operation.Click(1050, 450);	// 集结按钮
							yield return new EditorWaitForSeconds(0.3F);
							i++;
						}
						if (Recognize.IsEnergyShortcutAdding) {
							Operation.Click(1170, 384);	// 关闭按钮
							Debug.Log("体力不足，等待稍后尝试");
							yield return new EditorWaitForSeconds(300);
						}
					}
				}
			}
			if (Recognize.CurrentScene == Recognize.Scene.FIGHTING) {
				Debug.Log("选择编队");
				Operation.Click(1145 + 37 * SQUAD_NUMBER, 870);	// 选择编队
				yield return new EditorWaitForSeconds(0.2F);
				if (!test && Recognize.FightingSoldierCountPercent > 0.99F && Recognize.FightingHeroEmptyCount <= 0) {
					Operation.Click(960, 470);	// 出战按钮
					ATTACK_COUNT--;
					Debug.Log("出发");
				} else {
					Debug.Log("退出按钮");
					Operation.Click(30, 140);	// 退出按钮
					yield return new EditorWaitForSeconds(0.2F);
					Debug.Log("确认退出按钮");
					Operation.Click(1064, 634);	// 确认退出按钮
					yield return new EditorWaitForSeconds(2);
				}
			} else {
				for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
					Debug.Log("关闭窗口");
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.3F);
				}
			}
			
			EndOfRescueRefugees:
			Task.CurrentTask = null;
			
			// 休息5秒，避免出错时一直受控不能操作
			yield return new EditorWaitForSeconds(5);
		}
		// ReSharper disable once IteratorNeverReturns
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
