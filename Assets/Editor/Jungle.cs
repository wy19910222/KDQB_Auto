/*
 * @Author: wangyun
 * @CreateTime: 2023-09-07 20:18:29 730
 * @LastEditor: wangyun
 * @EditTime: 2023-09-07 20:18:29 842
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class JungleConfig : PrefsEditorWindow<Jungle> {
	[MenuItem("Tools_Window/Default/Jungle", false, 1)]
	private static void Open() {
		GetWindow<JungleConfig>("打野").Show();
	}
	
	private void OnGUI() {
		if (m_Debug) {
			if (EditorGUIUtility.currentViewWidth > 400) {
				EditorGUILayout.BeginHorizontal();
			}
			if (GUILayout.Button("打印头像特征")) {
				Recognize.LogGroupHeroAvatar();
			}
			if (GUILayout.Button("打印戴安娜位置")) {
				Debug.LogError($"戴安娜：{Recognize.GetHeroGroupNumber(Recognize.HeroType.DAN)}");
			}
			if (GUILayout.Button("打印尤里卡位置")) {
				Debug.LogError($"尤里卡：{Recognize.GetHeroGroupNumber(Recognize.HeroType.YLK)}");
			}
			if (GUILayout.Button("打印明日香位置")) {
				Debug.LogError($"明日香：{Recognize.GetHeroGroupNumber(Recognize.HeroType.MRX)}");
			}
			if (EditorGUIUtility.currentViewWidth > 400) {
				EditorGUILayout.EndHorizontal();
			}
			Rect rect0 = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect0 = new Rect(rect0.x, rect0.y + 4.5F, rect0.width, 1);
			EditorGUI.DrawRect(wireRect0, Color.gray);
		}
		
		Jungle.COOLDOWN = Mathf.Max(EditorGUILayout.FloatField("打野间隔", Jungle.COOLDOWN), 5);
		bool useBottle = Jungle.USE_BOTTLE_DICT.Values.ToList().Exists(count => count > 0);
		if (!useBottle) {
			Jungle.RESERVED_ENERGY = EditorGUILayout.IntField("保留体力值", Jungle.RESERVED_ENERGY);
		}
		
		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("攻击目标");
		if (GUILayout.Button("-")) {
			Jungle.TARGET_ATTACK_LIST.RemoveAt(Jungle.TARGET_ATTACK_LIST.Count - 1);
		}
		if (GUILayout.Button("+")) {
			Jungle.TARGET_ATTACK_LIST.Add(false);
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Space(5F);
		for (int i = 0, length = Jungle.TARGET_ATTACK_LIST.Count; i < length; ++i) {
			Jungle.TARGET_ATTACK_LIST[i] = GUILayout.Toggle(Jungle.TARGET_ATTACK_LIST[i], $"目标{i + 1}", "Button");
		}
		EditorGUILayout.EndHorizontal();
		Jungle.JUNGLE_STAR = EditorGUILayout.IntSlider("星级（如果是黑暗机甲）", Jungle.JUNGLE_STAR, 1, 5);
		Jungle.REPEAT_5 = EditorGUILayout.Toggle("是否5连（如果可以5连）", Jungle.REPEAT_5);
		
		Rect rect2 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect2 = new Rect(rect2.x, rect2.y + 4.5F, rect2.width, 1);
		EditorGUI.DrawRect(wireRect2, Color.gray);
		
		Jungle.SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", Jungle.SQUAD_NUMBER, 1, 8);
		EditorGUILayout.BeginHorizontal();
		foreach (Recognize.HeroType type in Enum.GetValues(typeof(Recognize.HeroType))) {
			bool isSelected = type == Jungle.HERO_AVATAR;
			bool newIsSelected = GUILayout.Toggle(isSelected, Utils.GetEnumInspectorName(type), "Button");
			if (newIsSelected && !isSelected) {
				Jungle.HERO_AVATAR = type;
			}
		}
		EditorGUILayout.EndHorizontal();
		
		Rect rect3 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect3 = new Rect(rect3.x, rect3.y + 4.5F, rect3.width, 1);
		EditorGUI.DrawRect(wireRect3, Color.gray);
		
		foreach (Recognize.EnergyShortcutAddingType type in Enum.GetValues(typeof(Recognize.EnergyShortcutAddingType))) {
			if (type != Recognize.EnergyShortcutAddingType.NONE) {
				EditorGUILayout.BeginHorizontal();
				EditorGUI.BeginChangeCheck();
				Jungle.USE_BOTTLE_DICT.TryGetValue(type, out int count);
				int newCount = Math.Max(EditorGUILayout.IntField(Utils.GetEnumInspectorName(type), Math.Abs(count)), 0);
				if (EditorGUI.EndChangeCheck()) {
					count = count < 0 ? -newCount : newCount;
					Jungle.USE_BOTTLE_DICT[type] = count;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Toggle(count > 0, GUILayout.Width(16F));
				if (EditorGUI.EndChangeCheck()) {
					count = -count;
					Jungle.USE_BOTTLE_DICT[type] = count;
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		
		GUILayout.Space(5F);
		
		EditorGUILayout.BeginHorizontal();
		if (Jungle.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
		Jungle.Test = GUILayout.Toggle(Jungle.Test, "测试", "Button", GUILayout.Width(60F));
		EditorGUILayout.EndHorizontal();
	}
}

public class Jungle {
	public static bool Test { get; set; } // 测试模式

	public static int RESERVED_ENERGY = 59;	// 保留体力值
	public static float COOLDOWN = 5;	// 打野间隔
	
	public static readonly List<bool> TARGET_ATTACK_LIST = new List<bool>();	// 攻击目标随机范围
	public static int JUNGLE_STAR = 4;	// 打的黑暗机甲星级
	public static bool REPEAT_5 = true;	// 是否五连
	
	public static int SQUAD_NUMBER = 1;	// 使用编队号码
	public static Recognize.HeroType HERO_AVATAR = Recognize.HeroType.MRX;	// 打野英雄头像
	public static readonly Dictionary<Recognize.EnergyShortcutAddingType, int> USE_BOTTLE_DICT = new Dictionary<Recognize.EnergyShortcutAddingType, int>();	// 是否自动补充体力
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartJungle", priority = -1)]
	private static void Enable() {
		Disable();
		List<string> switches = new List<string>();
		switches.Add($"打野间隔【{COOLDOWN}】");
		if (!USE_BOTTLE_DICT.Values.ToList().Exists(count => count > 0)) {
			switches.Add($"保留体力值【{RESERVED_ENERGY}】");
		}
		{
			List<string> targets = new List<string>();
			for (int i = 0, length = TARGET_ATTACK_LIST.Count; i < length; ++i) {
				if (TARGET_ATTACK_LIST[i]) {
					targets.Add($"第{i + 1}个");
				}
			}
			switches.Add($"目标【{string.Join("、", targets)}】");
		}
		switches.Add($"星级（如果是黑暗机甲）【{JUNGLE_STAR}】");
		switches.Add(REPEAT_5 ? "5连" : "单刷");
		switches.Add($"使用编队【{SQUAD_NUMBER}】");
		foreach (var (type, count) in USE_BOTTLE_DICT) {
			if (count > 0) {
				switches.Add($"【{Utils.GetEnumInspectorName(type)}{count}次】");
			}
		}
		Debug.Log($"自动打野已开启，{string.Join("，", switches)}");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopJungle", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动打野已关闭");
		}
	}

	private static IEnumerator Update() {
		int starOffset = 0;
		while (true) {
			yield return null;
			if (Recognize.CurrentScene != Recognize.Scene.OUTSIDE) {
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
				if (USE_BOTTLE_DICT.Values.All(count => count <= 0) && Recognize.energy < RESERVED_ENERGY + (REPEAT_5 ? 15 : 10)) {
					// Debug.Log($"当前体力：{Recognize.energy}");
					continue;
				}
				// 队列数量
				if (Recognize.BusyGroupCount >= Recognize.GROUP_COUNT) {
					// Debug.Log($"忙碌队列：{Recognize.BusyGroupCount}");
					continue;
				}
				// 存在打野英雄头像
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
				// 存在打野英雄头像
				if (Recognize.GetHeroGroupNumber(HERO_AVATAR) >= 0) {
					// Debug.Log($"存在打野英雄头像");
					continue;
				}
			}
			Debug.Log("当前忙碌队列数量: " + Recognize.BusyGroupCount);
			// 确定攻击目标
			int target = RandomTarget();
			if (target == -1) {
				Debug.Log("未选择攻击目标，取消操作");
				continue;
			}
			
			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(Jungle);
			
			// 开始打野
			while (!Recognize.IsSearching) {
				Debug.Log("搜索按钮");
				Operation.Click(750, 970);	// 搜索按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			Debug.Log("敌军按钮");
			Operation.Click(770, 510);	// 敌军按钮
			yield return new EditorWaitForSeconds(0.1F);
			const int TARGET_WIDTH = 163;
			Debug.Log("攻击目标: " + target);
			{
				// 先拖动到列表最开头，以便计算
				var ie = Operation.NoInertiaDrag(803, 672, 803 + TARGET_WIDTH * (TARGET_ATTACK_LIST.Count - 2), 672, 0.2F);
				while (ie.MoveNext()) {
					yield return ie.Current;
				}
				yield return new EditorWaitForSeconds(1F);
			}
			Debug.Log("拖动以显示攻击目标");
			int orderOffsetX = (target - 2) * TARGET_WIDTH;
			while (orderOffsetX > 0) {
				int dragDistance = Mathf.Min(TARGET_WIDTH * 3, orderOffsetX);
				// 往左拖动
				var ie = Operation.NoInertiaDrag(1129, 672, 1129 - dragDistance, 672, 0.2F);
				while (ie.MoveNext()) {
					yield return ie.Current;
				}
				yield return new EditorWaitForSeconds(0.2F);
				orderOffsetX -= dragDistance;
			}
			Operation.Click(1129 + orderOffsetX, 672);	// 选中目标
			yield return new EditorWaitForSeconds(0.1F);
			// 活动对象，没有滑块，不能5连
			bool isNoSlider = Recognize.ApproximatelyCoveredCount(Operation.GetColorOnScreen(960, 880), new Color32(199, 208, 210, 255), 1.1F) >= 0;
			// 黑暗机甲， 没有等级，滑块选择的是星级
			bool isStarSlider = Recognize.ApproximatelyCoveredCount(Operation.GetColorOnScreen(942, 851), new Color32(254, 216, 81, 255), 1.1F) >= 0;
			if (isStarSlider) {
				Debug.Log("星级滑块");
				Operation.Click(844 + 44 * (JUNGLE_STAR + starOffset), 880);	// 星级滑块
			} else {
				Debug.Log("等级滑块");
				Operation.Click(1062, 880);	// 等级滑块
			}
			yield return new EditorWaitForSeconds(0.1F);
			
			Debug.Log("搜索按钮");
			Operation.Click(960, 940);	// 搜索按钮
			yield return new EditorWaitForSeconds(0.2F);
			if (Recognize.IsSearching) {	// 搜索面板未消失，说明未搜索到
				if (isStarSlider) {
					// 下次尝试搜索低星级
					starOffset = Mathf.Max(starOffset - 1, -JUNGLE_STAR + 1);
				}
				Debug.Log("未搜到，关闭搜索面板");
				while (Recognize.IsSearching) {
					// 点击空白处退出搜索面板
					Operation.Click(660, 970);	// 点击空白处
					yield return new EditorWaitForSeconds(0.3F);
				}
				continue;
			}
			
			// 搜索面板消失，说明搜索到了
			if (isStarSlider) {
				// 下次重新尝试搜索期望星级
				starOffset = 0;
			}
			// 避免没刷出来，先等一会儿
			yield return new EditorWaitForSeconds(0.3F);
			Debug.Log("已搜到，选中目标");
			Operation.Click(960, 580);	// 选中目标
			yield return new EditorWaitForSeconds(0.2F);
			if (isNoSlider) {
				Debug.Log("活动对象气泡里的攻击按钮");
				Operation.Click(960, 880);	// 攻击按钮
				yield return new EditorWaitForSeconds(0.3F);
				if (!Recognize.IsEnergyShortcutAdding && Recognize.CurrentScene != Recognize.Scene.FIGHTING) {
					// 不同视角距离按钮位置会不一样，所以尝试两个不同的位置
					Operation.Click(960, 920);	// 攻击按钮
				}
			} else {
				Debug.Log("其他气泡里的攻击按钮");
				Operation.Click(REPEAT_5 ? 870 : 1050, 430);	// 攻击按钮/攻击5次按钮
			}
			yield return new EditorWaitForSeconds(0.3F);
			
			// 出现体力不足面板
			if (Recognize.IsEnergyShortcutAdding) {
				// // 打开背包嗑小体
				// Operation.Click(1170, 384);	// 关闭按钮
				// yield return new EditorWaitForSeconds(0.3F);
				// Operation.Click(1870, 870);	// 背包按钮
				// yield return new EditorWaitForSeconds(0.1F);
				// // 判断小体是否在第二格
				// Color32 targetColor1 = new Color32(129, 242, 25, 255);
				// Color32 realColor1 = ScreenshotUtils.GetColorOnScreen(847, 305);
				// Color32 targetColor2 = new Color32(248, 210, 22, 255);
				// Color32 realColor2 = ScreenshotUtils.GetColorOnScreen(866, 281);
				// if (Recognize.Approximately(realColor1, targetColor1) && Recognize.Approximately(realColor2, targetColor2)) {
				// 	Operation.Click(866, 281);	// 选中小体
				// 	yield return new EditorWaitForSeconds(0.1F);
				// 	Operation.Click(960, 960);	// 使用按钮
				// 	yield return new EditorWaitForSeconds(0.1F);
				// }
				// Operation.Click(720, 128);	// 左上角返回按钮
				// yield return new EditorWaitForSeconds(0.1F);
				// Operation.Click(960, 580);	// 选中目标
				// yield return new EditorWaitForSeconds(0.2F);
				// if (isNoSlider) {
				// 	Operation.Click(960, 895);	// 攻击按钮
				// } else {
				// 	Operation.Click(REPEAT_5 ? 870 : 1050, 430);	// 攻击按钮/攻击5次按钮
				// }
				// yield return new EditorWaitForSeconds(0.3F);
				
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
				while (Recognize.IsEnergyShortcutAdding && i < iMax) {
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
					yield return new EditorWaitForSeconds(0.2F);
					if (isNoSlider) {
						Debug.Log("活动对象气泡里的攻击按钮");
						Operation.Click(960, 880);	// 攻击按钮
						yield return new EditorWaitForSeconds(0.3F);
						if (!Recognize.IsEnergyShortcutAdding && Recognize.CurrentScene != Recognize.Scene.FIGHTING) {
							// 不同视角距离按钮位置会不一样，所以尝试两个不同的位置
							Operation.Click(960, 920);	// 攻击按钮
						}
					} else {
						Debug.Log("其他气泡里的攻击按钮");
						Operation.Click(REPEAT_5 ? 870 : 1050, 430);	// 攻击按钮/攻击5次按钮
					}
					yield return new EditorWaitForSeconds(0.3F);
					i++;
				}
				if (Recognize.IsEnergyShortcutAdding) {
					Operation.Click(1170, 384);	// 关闭按钮
					Debug.Log("体力不足，等待稍后尝试");
					yield return new EditorWaitForSeconds(300);
				}
			}
			if (Recognize.CurrentScene == Recognize.Scene.FIGHTING) {
				Operation.Click(1145 + 37 * SQUAD_NUMBER, 870);	// 选择队列
				yield return new EditorWaitForSeconds(0.2F);
				if (!test && (Recognize.SoldierCountPercent > 0.99F || !isStarSlider)) {
					Operation.Click(960, 470);	// 出战按钮
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
					yield return new EditorWaitForSeconds(0.1F);
				}
			}
			
			Task.CurrentTask = null;
			
			yield return new EditorWaitForSeconds(COOLDOWN);
		}
		// ReSharper disable once IteratorNeverReturns
	}
	
	private static int RandomTarget() {
		List<int> list = new List<int>();
		for (int i = 0, length = TARGET_ATTACK_LIST.Count; i < length; ++i) {
			if (TARGET_ATTACK_LIST[i]) {
				list.Add(i);
			}
		}
		if (list.Count <= 0) {
			return -1;
		}
		return list[Random.Range(0, list.Count)];
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
		return list.Count > 0 ? list[Random.Range(0, list.Count)] : Recognize.EnergyShortcutAddingType.NONE;
	}
}
