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

public class JungleConfig : PrefsEditorWindow<Jungle> {
	[MenuItem("Window/Jungle")]
	private static void Open() {
		GetWindow<JungleConfig>("打野").Show();
	}
	
	private void OnGUI() {
		Jungle.GROUP_COUNT = EditorGUILayout.IntSlider("拥有行军队列", Jungle.GROUP_COUNT, 0, 7);
		Jungle.COOLDOWN = Mathf.Max(EditorGUILayout.FloatField("打野间隔", Jungle.COOLDOWN), 5);
		if (!Jungle.USE_SMALL_BOTTLE && !Jungle.USE_BIG_BOTTLE) {
			Jungle.RESERVED_ENERGY = EditorGUILayout.IntField("保留体力值", Jungle.RESERVED_ENERGY);
		}
		
		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);
		
		Jungle.JUNGLE_LAND = EditorGUILayout.Toggle("攻击陆军残兵", Jungle.JUNGLE_LAND);
		Jungle.JUNGLE_SEA = EditorGUILayout.Toggle("攻击海军残兵", Jungle.JUNGLE_SEA);
		Jungle.JUNGLE_AIR = EditorGUILayout.Toggle("攻击空军残兵", Jungle.JUNGLE_AIR);
		Jungle.JUNGLE_MECHA = EditorGUILayout.Toggle("攻击黑暗机甲", Jungle.JUNGLE_MECHA);
		if (Jungle.JUNGLE_MECHA) {
			Jungle.JUNGLE_STAR = EditorGUILayout.IntSlider("  黑暗机甲星级", Jungle.JUNGLE_STAR, 1, 5);
		}
		
		Rect rect2 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect2 = new Rect(rect2.x, rect2.y + 4.5F, rect2.width, 1);
		EditorGUI.DrawRect(wireRect2, Color.gray);
		
		Jungle.SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", Jungle.SQUAD_NUMBER, 1, 8);
		Jungle.USE_SMALL_BOTTLE = EditorGUILayout.Toggle("是否使用小体", Jungle.USE_SMALL_BOTTLE);
		Jungle.USE_BIG_BOTTLE = EditorGUILayout.Toggle("是否使用大体", Jungle.USE_BIG_BOTTLE);
		GUILayout.Space(5F);
		if (Jungle.IsRunning) {
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

public class Jungle {
	public static int GROUP_COUNT = 4;	// 拥有行军队列数
	public static int RESERVED_ENERGY = 59;	// 保留体力值
	public static float COOLDOWN = 5;	// 打野间隔
	
	public static bool JUNGLE_LAND = true;	// 是否攻击陆军残兵
	public static bool JUNGLE_SEA = true;	// 是否攻击海军残兵
	public static bool JUNGLE_AIR = false;	// 是否攻击空军残兵
	public static bool JUNGLE_MECHA = false;	// 是否攻击黑暗机甲
	public static int JUNGLE_STAR = 4;	// 打的黑暗机甲星级
	public static int SQUAD_NUMBER = 1;	// 使用编队号码
	public static bool USE_SMALL_BOTTLE = false;	// 是否使用小体
	public static bool USE_BIG_BOTTLE = false;	// 是否使用大体
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartJungle", priority = -1)]
	private static void Enable() {
		Disable();
		List<string> switches = new List<string>();
		switches.Add($"拥有行军队列【{GROUP_COUNT}】");
		switches.Add($"保留体力值【{RESERVED_ENERGY}】");
		{
			List<string> targets = new List<string>();
			if (JUNGLE_LAND) { targets.Add("陆军残兵"); }
			if (JUNGLE_SEA) { targets.Add("海军残兵"); }
			if (JUNGLE_AIR) { targets.Add("空军残兵"); }
			if (JUNGLE_MECHA) { targets.Add($"黑暗机甲{JUNGLE_STAR}星"); }
			switches.Add($"目标【{string.Join("、", targets)}】");
		}
		switches.Add($"使用编队【{SQUAD_NUMBER}】");
		if (USE_SMALL_BOTTLE) { switches.Add("【允许使用小体】"); }
		if (USE_BIG_BOTTLE) { switches.Add("【允许使用大体】"); }
		Debug.Log($"自动打野已开启，{string.Join("，", switches)}");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopJungle", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动打野已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			switch (Recognize.CurrentScene) {
				case Recognize.Scene.ARMY_SELECTING:
					Debug.Log("可能是卡在出战界面了，执行返回");
					Operation.Click(30, 140);	// 左上角返回按钮
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
				bool energyEnough = USE_SMALL_BOTTLE | USE_BIG_BOTTLE | Recognize.energy >= RESERVED_ENERGY + 15;
				if (energyEnough) {
					if (Recognize.BusyGroupCount < GROUP_COUNT && Recognize.GetYLKGroupNumber() < 0) {
						yield return new EditorWaitForSeconds(0.2F);
						if (Recognize.BusyGroupCount < GROUP_COUNT && Recognize.GetYLKGroupNumber() < 0) {
							Debug.Log("当前忙碌队列数量: " + Recognize.BusyGroupCount);
							break;
						}
					}
				}
				// 等待有队列空闲出来且没有橙色英雄队伍（无法判断打野队伍，只能判断是否是橙色了）
				yield return null;
			}
			while (Recognize.IsWindowCovered) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(735, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			if (Recognize.CurrentScene != Recognize.Scene.OUTSIDE) {
				Debug.Log("已不在世界界面，重新开始");
				continue;
			}
			// 开始打野
			while (!Recognize.IsSearching) {
				Debug.Log("搜索按钮");
				Operation.Click(750, 970);	// 搜索按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			Debug.Log("敌军按钮");
			Operation.Click(770, 510);	// 敌军按钮
			yield return new EditorWaitForSeconds(0.1F);
			// 确定攻击目标
			{
				List<int> list = new List<int>();
				if (JUNGLE_LAND) {
					list.Add(0);
				}
				if (JUNGLE_SEA) {
					list.Add(1);
				}
				if (JUNGLE_AIR) {
					list.Add(2);
				}
				if (JUNGLE_MECHA) {
					list.Add(3);
				}
				int target = list[Random.Range(0, list.Count)];
				Debug.Log("攻击目标: " + target);
				switch (target) {
					case 3: {
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
						break;
					}
					default: {
						// Debug.Log("列表往右拖动");
						var ie = Operation.Drag(790, 670, 1120, 670, 0.2F);	// 列表往右拖动
						while (ie.MoveNext()) {
							yield return ie.Current;
						}
						yield return new EditorWaitForSeconds(0.3F);
						// Debug.Log("选中目标");
						Operation.Click(794 + 163 * target, 670);	// 选中目标
						yield return new EditorWaitForSeconds(0.1F);
						// Debug.Log("等级滑块");
						Operation.Click(1062, 880);	// 等级滑块
						yield return new EditorWaitForSeconds(0.1F);
						break;
					}
				}
			}
			// Debug.Log("搜索按钮");
			Operation.Click(960, 940);	// 搜索按钮
			yield return new EditorWaitForSeconds(0.2F);
			// 搜索面板消失，说明搜索到了
			if (!Recognize.IsSearching) {
				Operation.Click(960, 580);	// 选中目标
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(870, 430);	// 攻击5次按钮
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
					// Operation.Click(735, 128);	// 左上角返回按钮
					// yield return new EditorWaitForSeconds(0.1F);
					// Operation.Click(960, 580);	// 选中目标
					// yield return new EditorWaitForSeconds(0.1F);
					// Operation.Click(870, 430);	// 攻击5次按钮
					// yield return new EditorWaitForSeconds(0.3F);
					
					// 快捷嗑药
					Recognize.EnergyShortcutAddingType useBottle = RandomUseBottle();	// 随机使用大小体
					int i = 0;
					int iMax = 0;
					switch (useBottle) {
						case Recognize.EnergyShortcutAddingType.SMALL_BOTTLE:
							Debug.Log("使用小体");
							iMax = 3;
							break;
						case Recognize.EnergyShortcutAddingType.BIG_BOTTLE:
							Debug.Log("使用大体");
							iMax = 1;
							break;
					}
					while (Recognize.IsEnergyShortcutAdding && i < iMax) {
						List<Recognize.EnergyShortcutAddingType> types = Recognize.GetShortcutTypes();
						int index = types.IndexOf(useBottle);
						if (index != -1) {
							Debug.Log($"嗑{index + 1}号位");
							Operation.Click(828 + index * 130, 590);	// 选中图标
							yield return new EditorWaitForSeconds(0.1F);
							Operation.Click(960, 702);	// 使用按钮
							yield return new EditorWaitForSeconds(0.1F);
						} else {
							Debug.LogError("体力药剂数量不足！");
						}
						Operation.Click(1170, 384);	// 关闭按钮
						yield return new EditorWaitForSeconds(0.3F);
						Operation.Click(960, 580);	// 选中目标
						yield return new EditorWaitForSeconds(0.1F);
						Operation.Click(870, 430);	// 攻击5次按钮
						yield return new EditorWaitForSeconds(0.3F);
						i++;
					}
					if (Recognize.IsEnergyShortcutAdding) {
						Operation.Click(1170, 384);	// 关闭按钮
						Debug.Log("体力不足，等待稍后尝试");
						yield return new EditorWaitForSeconds(300);
					}
				}
				if (Recognize.CurrentScene == Recognize.Scene.ARMY_SELECTING) {
					Operation.Click(1145 + 37 * SQUAD_NUMBER, 870);	// 选择队列
					yield return new EditorWaitForSeconds(0.2F);
					Operation.Click(960, 470);	// 出战按钮
					Debug.Log("出发");
				}
			} else {
				while (Recognize.IsSearching) {
					// 点击空白处退出搜索面板
					Operation.Click(660, 970);	// 选中目标
					yield return new EditorWaitForSeconds(0.3F);
				}
			}
			yield return new EditorWaitForSeconds(COOLDOWN);
		}
		// ReSharper disable once IteratorNeverReturns
	}
	
	private static Recognize.EnergyShortcutAddingType RandomUseBottle() {
		List<Recognize.EnergyShortcutAddingType> list = new List<Recognize.EnergyShortcutAddingType>();
		if (USE_SMALL_BOTTLE) { list.Add(Recognize.EnergyShortcutAddingType.SMALL_BOTTLE); }
		if (USE_BIG_BOTTLE) { list.Add(Recognize.EnergyShortcutAddingType.BIG_BOTTLE); }
		return list.Count > 0 ? list[Random.Range(0, list.Count)] : Recognize.EnergyShortcutAddingType.NONE;
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
