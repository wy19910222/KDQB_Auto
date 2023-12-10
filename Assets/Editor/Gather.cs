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

using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class GatherConfig : PrefsEditorWindow<Gather> {
	[MenuItem("Window/Default/Gather", false, 2)]
	private static void Open() {
		GetWindow<GatherConfig>("集结").Show();
	}
	
	private void OnGUI() {
		Gather.GROUP_COUNT = EditorGUILayout.IntSlider("拥有行军队列", Gather.GROUP_COUNT, 0, 7);
		Gather.RESERVED_ENERGY = EditorGUILayout.IntField("保留体力值", Gather.RESERVED_ENERGY);
		
		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);
		
		Gather.GATHER_ZC = EditorGUILayout.Toggle("集结战锤", Gather.GATHER_ZC);
		Gather.GATHER_JX = EditorGUILayout.Toggle("集结惧星", Gather.GATHER_JX);
		Gather.GATHER_JW = EditorGUILayout.Toggle("集结精卫", Gather.GATHER_JW);
		
		Rect rect2 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect2 = new Rect(rect2.x, rect2.y + 4.5F, rect2.width, 1);
		EditorGUI.DrawRect(wireRect2, Color.gray);
		
		Gather.SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", Gather.SQUAD_NUMBER, 1, 8);
		EditorGUILayout.BeginHorizontal();
		foreach (Recognize.HeroType type in Enum.GetValues(typeof(Recognize.HeroType))) {
			bool isSelected = type == Gather.HERO_AVATAR;
			bool newIsSelected = GUILayout.Toggle(isSelected, Utils.GetEnumInspectorName(type), "Button");
			if (newIsSelected && !isSelected) {
				Gather.HERO_AVATAR = type;
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
				Gather.USE_BOTTLE_DICT.TryGetValue(type, out int count);
				int newCount = Math.Max(EditorGUILayout.IntField(Utils.GetEnumInspectorName(type), Math.Abs(count)), 0);
				if (EditorGUI.EndChangeCheck()) {
					count = count < 0 ? -newCount : newCount;
					Gather.USE_BOTTLE_DICT[type] = count;
				}
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.Toggle(count > 0, GUILayout.Width(16F));
				if (EditorGUI.EndChangeCheck()) {
					count = -count;
					Gather.USE_BOTTLE_DICT[type] = count;
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		GUILayout.Space(5F);
		if (Gather.IsRunning) {
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

public class Gather {
	public static int GROUP_COUNT = 4;	// 拥有行军队列数
	public static int RESERVED_ENERGY = 60;	// 保留体力值
	public static bool GATHER_ZC = false;	// 是否集结战锤
	public static bool GATHER_JX = true;	// 是否集结惧星
	public static bool GATHER_JW = false;	// 是否集结精卫
	public static int SQUAD_NUMBER = 3;	// 使用编队号码
	public static Recognize.HeroType HERO_AVATAR = Recognize.HeroType.MRX;	// 集结英雄头像
	public static readonly Dictionary<Recognize.EnergyShortcutAddingType, int> USE_BOTTLE_DICT = new Dictionary<Recognize.EnergyShortcutAddingType, int>();	// 是否自动补充体力
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartGather", priority = -1)]
	private static void Enable() {
		Disable();
		List<string> switches = new List<string>();
		switches.Add($"拥有行军队列【{GROUP_COUNT}】");
		if (!USE_BOTTLE_DICT.Values.ToList().Exists(count => count > 0)) {
			switches.Add($"保留体力值【{RESERVED_ENERGY}】");
		}
		{
			List<string> targets = new List<string>();
			if (GATHER_ZC) { targets.Add("战锤"); }
			if (GATHER_JX) { targets.Add("惧星"); }
			if (GATHER_JW) { targets.Add("精卫/砰砰"); }
			switches.Add($"目标【{string.Join("、", targets)}】");
		}
		switches.Add($"使用编队【{SQUAD_NUMBER}】");
		foreach (var (type, count) in USE_BOTTLE_DICT) {
			if (count > 0) {
				switches.Add($"【{Utils.GetEnumInspectorName(type)}{count}次】");
			}
		}
		Debug.Log($"自动集结已开启，{string.Join("，", switches)}");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopGather", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动集结已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			// 体力值
			if (USE_BOTTLE_DICT.Values.All(count => count <= 0) && Recognize.energy < RESERVED_ENERGY + 8) {
				Debug.Log($"当前体力：{Recognize.energy}");
				continue;
			}
			// 队列数量
			if (Recognize.BusyGroupCount >= GROUP_COUNT) {
				Debug.Log($"忙碌队列：{Recognize.BusyGroupCount}");
				continue;
			}
			// 存在打野英雄头像
			if (Recognize.GetHeroGroupNumber(HERO_AVATAR) >= 0) {
				Debug.Log($"存在打野英雄头像");
				continue;
			}
			// 有窗口打开着
			if (Recognize.IsWindowCovered) {
				Debug.Log($"有窗口打开着");
				continue;
			}
			
			// 开始集结
			while (!Recognize.IsSearching) {
				// Debug.Log("搜索按钮");
				Operation.Click(750, 970);	// 搜索按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			Debug.Log("集结按钮");
			Operation.Click(1024, 512);	// 集结按钮
			yield return new EditorWaitForSeconds(0.1F);
			int target = 0;
			{
				Debug.Log("确定攻击目标");
				List<int> list = new List<int>();
				if (GATHER_ZC) {
					list.Add(0);
				}
				if (GATHER_JX) {
					list.Add(1);
				}
				if (GATHER_JW) {
					list.Add(2);
				}
				target = list[Random.Range(0, list.Count)];
			}
			Debug.Log($"选中目标: {new []{"战锤", "惧星", "精卫"}[target]}");
			Operation.Click(800 + 170 * target, 670);	// 选中惧星
			yield return new EditorWaitForSeconds(0.1F);
			Debug.Log("等级滑块");
			Operation.Click(1062, 880);	// 等级滑块
			yield return new EditorWaitForSeconds(0.1F);
			Debug.Log("搜索按钮");
			Operation.Click(960, 940);	// 搜索按钮
			yield return new EditorWaitForSeconds(0.2F);
			// 搜索面板消失，说明搜索到了
			if (Recognize.IsSearching) {
				Debug.Log("未搜到，关闭搜索面板");
				while (Recognize.IsSearching) {
					// 点击空白处退出搜索面板
					Operation.Click(660, 970);	// 点击空白处
					yield return new EditorWaitForSeconds(0.3F);
				}
				continue;
			}

			// 搜索面板消失，说明搜索到了
			Debug.Log("已搜到，选中目标");
			// 避免没刷出来，先等一会儿
			yield return new EditorWaitForSeconds(0.3F);
			Operation.Click(960, 560);	// 选中目标
			yield return new EditorWaitForSeconds(0.2F);
			if (target == 1) {
				Debug.Log("集结");
				Operation.Click(870, 830);	// 集结按钮
				yield return new EditorWaitForSeconds(0.3F);
			} else {
				Debug.Log("集结");
				Operation.Click(1050, 450);	// 集结按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
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
					Debug.Log($"嗑{index + 1}号位");
					Operation.Click(828 + index * 130, 590);	// 选中图标
					yield return new EditorWaitForSeconds(0.1F);
					Operation.Click(960, 702);	// 使用按钮
					USE_BOTTLE_DICT.TryGetValue(useBottle, out int count);
					USE_BOTTLE_DICT[useBottle] = count - 1;
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
			if (Recognize.CurrentScene == Recognize.Scene.FIGHTING) {
				Operation.Click(1145 + 37 * SQUAD_NUMBER, 870);	// 选择队列
				yield return new EditorWaitForSeconds(0.2F);
				if (Recognize.SoldierCountPercent > 0.99F) {
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
			}
			
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
		return list.Count > 0 ? list[Random.Range(0, list.Count)] : Recognize.EnergyShortcutAddingType.NONE;
	}
}
