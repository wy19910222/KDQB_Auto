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

public class AttackDiscipleConfig : EditorWindow {
	[MenuItem("Window/AttackDisciple")]
	private static void Open() {
		GetWindow<AttackDiscipleConfig>("作战实验室").Show();
	}
	
	private void OnGUI() {
		AttackDisciple2.GROUP_COUNT = EditorGUILayout.IntSlider("拥有行军队列", AttackDisciple2.GROUP_COUNT, 0, 7);
		AttackDisciple2.RESERVED_ENERGY = EditorGUILayout.IntField("保留体力值", AttackDisciple2.RESERVED_ENERGY);
		AttackDisciple2.TARGET = EditorGUILayout.Popup("训练目标", AttackDisciple2.TARGET, new []{"第7使徒", "第8使徒", "腐坏机甲", "第10使徒"});
		AttackDisciple2.SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", AttackDisciple2.SQUAD_NUMBER, 1, 8);
		GUILayout.Space(5F);
		if (AttackDisciple2.IsRunning) {
			if (GUILayout.Button("关闭")) {
				EditorApplication.ExecuteMenuItem("Assets/StopAttackDisciple_2.0");
			}
		} else {
			if (GUILayout.Button("开启")) {
				EditorApplication.ExecuteMenuItem("Assets/StartAttackDisciple_2.0");
			}
		}
	}
}

public static class AttackDisciple2 {
	public static int GROUP_COUNT = 4;	// 拥有行军队列数
	public static int RESERVED_ENERGY = 60;	// 保留体力值
	public static int SQUAD_NUMBER = 4;	// 使用编队号码
	public static int TARGET;	// 攻打目标
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartAttackDisciple_2.0", priority = -1)]
	private static void Enable() {
		Disable();
		List<string> switches = new List<string> {
			$"拥有行军队列【{GROUP_COUNT}】",
			$"使用编队【{SQUAD_NUMBER}】"
		};
		Debug.Log($"自动作战研究已开启，{string.Join("，", switches)}");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopAttackDisciple_2.0", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动作战研究已关闭");
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
			Debug.Log("当前忙碌队列数量: " + Recognize.BusyGroupCount);
			while (true) {
				if (Recognize.energy >= RESERVED_ENERGY + 10 && Recognize.BusyGroupCount < GROUP_COUNT) {
					yield return new EditorWaitForSeconds(0.2F);
					if (Recognize.energy >= RESERVED_ENERGY + 10 && Recognize.BusyGroupCount < GROUP_COUNT) {
						Debug.Log("当前忙碌队列数量: " + Recognize.BusyGroupCount);
						break;
					}
				}
				yield return null;
			}
			if (Recognize.IsWindowCovered) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("左上角返回按钮");
				do {
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.2F);
				} while (Recognize.IsWindowCovered);
			}

			Debug.Log("准备打使徒");
			Operation.Click(1880, Recognize.CurrentScene == Recognize.Scene.OUTSIDE ? 360 : 420);
			yield return new EditorWaitForSeconds(0.1F);
			Operation.Click(860, 860);	// 作战研究室按钮
			yield return new EditorWaitForSeconds(0.1F);
			Operation.Click(Mathf.Min(800 + TARGET * 154, 1190), 425);	// 训练目标
			yield return new EditorWaitForSeconds(0.1F);
			Debug.Log("攻击使徒");
			Operation.Click(960, 950);	// 攻击使徒按钮
			yield return new EditorWaitForSeconds(0.3F);
			if (Recognize.IsEnergyShortcutAdding) {
				Debug.Log("体力不足");
				while (Recognize.IsWindowCovered) {
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.2F);
				}
			} else {
				Debug.Log("选择出征");
				Operation.Click(1145 + 37 * SQUAD_NUMBER, 870);	// 选择队列
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(960, 470);	// 出战按钮
				yield return new EditorWaitForSeconds(0.5F);
				Operation.Click(30, 250);	// 跳过按钮
				yield return new EditorWaitForSeconds(5F);
				Operation.Click(960, 910);	// 返回按钮
				yield return new EditorWaitForSeconds(0.3F);
				while (Recognize.IsWindowCovered) {
					Operation.Click(720, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.2F);
				}
			}
			// 休息5秒，避免出错时一直受控不能操作
			yield return new EditorWaitForSeconds(5);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
