/*
 * @Author: wangyun
 * @CreateTime: 2023-10-12 00:00:51 751
 * @LastEditor: wangyun
 * @EditTime: 2023-10-12 00:00:51 756
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class GatherConfig : PrefsEditorWindow<Gather> {
	[MenuItem("Window/Gather")]
	private static void Open() {
		GetWindow<GatherConfig>("集结").Show();
	}
	
	private void OnGUI() {
		Gather.GROUP_COUNT = EditorGUILayout.IntSlider("拥有行军队列", Gather.GROUP_COUNT, 0, 7);
		Gather.RESERVED_ENERGY = EditorGUILayout.IntField("保留体力值", Gather.RESERVED_ENERGY);
		Gather.GATHER_ZC = EditorGUILayout.Toggle("集结战锤", Gather.GATHER_ZC);
		Gather.GATHER_JX = EditorGUILayout.Toggle("集结惧星", Gather.GATHER_JX);
		Gather.GATHER_JW = EditorGUILayout.Toggle("集结精卫", Gather.GATHER_JW);
		Gather.SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", Gather.SQUAD_NUMBER, 1, 8);
		Gather.USE_SMALL_BOTTLE = EditorGUILayout.Toggle("是否使用小体", Gather.USE_SMALL_BOTTLE);
		Gather.USE_BIG_BOTTLE = EditorGUILayout.Toggle("是否使用大体", Gather.USE_BIG_BOTTLE);
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
	public static bool USE_SMALL_BOTTLE = false;	// 是否使用小体
	public static bool USE_BIG_BOTTLE = false;	// 是否使用大体
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartGather", priority = -1)]
	private static void Enable() {
		Disable();
		List<string> switches = new List<string>();
		switches.Add($"拥有行军队列【{GROUP_COUNT}】");
		switches.Add($"保留体力值【{RESERVED_ENERGY}】");
		{
			List<string> targets = new List<string>();
			if (GATHER_ZC) { targets.Add("战锤"); }
			if (GATHER_JX) { targets.Add("惧星"); }
			if (GATHER_JW) { targets.Add("精卫/砰砰"); }
			switches.Add($"目标【{string.Join("、", targets)}】");
		}
		switches.Add($"使用编队【{SQUAD_NUMBER}】");
		if (USE_SMALL_BOTTLE) { switches.Add("【允许使用小体】"); }
		if (USE_BIG_BOTTLE) { switches.Add("【允许使用大体】"); }
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
			if (Recognize.energy < RESERVED_ENERGY + 8) {
				continue;
			}
			// 队列数量
			if (Recognize.BusyGroupCount >= GROUP_COUNT) {
				continue;
			}
			// 尤里卡在任务中
			if (Recognize.GetYLKGroupNumber() >= 0) {
				continue;
			}
			// 有窗口打开着
			if (Recognize.IsWindowCovered) {
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
				Operation.Click(960, 300);	// 面板外部区域任意点
				yield return new EditorWaitForSeconds(0.2F);
				continue;
			}

			// 搜索面板消失，说明搜索到了
			Debug.Log("已搜到，选中目标");
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
			int useBottle = 0;
			{
				Debug.Log("确定使用大小体");
				List<int> list = new List<int>();
				if (USE_SMALL_BOTTLE) {
					list.Add(1);
				}
				if (USE_BIG_BOTTLE) {
					list.Add(2);
				}
				int listCount = list.Count;
				useBottle = listCount > 0 ? list[Random.Range(0, list.Count)] : 0;
				switch (useBottle) {
					case 1:
						Debug.Log("使用小体");
						break;
					case 2:
						Debug.Log("使用大体");
						break;
				}
			}
			Debug.Log($"确定使用: {new []{"小体", "大体"}[useBottle]}");
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
							if (i < 2) {
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
			Debug.Log("选择队列");
			Operation.Click(1145 + 37 * SQUAD_NUMBER, 870);
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(960, 470);	// 出战按钮
			Debug.Log("出发");
			
			// 休息5秒，避免出错时一直受控不能操作
			yield return new EditorWaitForSeconds(5);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
