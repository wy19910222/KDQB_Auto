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
	public static int SQUAD_NUMBER = 4;	// 使用编队号码
	public static int RESERVED_ENERGY = 60;	// 保留体力值
	
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
			Debug.Log("自动作战研究已关闭");
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

			Operation.Click(1880, Recognize.CurrentScene == Recognize.Scene.OUTSIDE ? 360 : 420);
			yield return new EditorWaitForSeconds(0.1F);
			Operation.Click(860, 860);	// 作战研究室按钮
			yield return new EditorWaitForSeconds(0.1F);
			Operation.Click(950, 425);	// 训练目标——第8使徒
			yield return new EditorWaitForSeconds(0.1F);
			Operation.Click(960, 950);	// 攻击使徒按钮
			
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
			Operation.Click(1880, 440);	// 活动按钮
			yield return new EditorWaitForSeconds(0.1F);
			Operation.Click(960, 480);	// 第七使徒按钮
			yield return new EditorWaitForSeconds(0.1F);
			Operation.Click(960, 920);	// 攻击使徒按钮
			yield return new EditorWaitForSeconds(0.1F);
			Operation.Click(960, 730);	// 攻击按钮
			yield return new EditorWaitForSeconds(0.3F);
			if (Recognize.IsEnergyAdding) {
				Operation.Click(735, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.3F);
				Operation.Click(735, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.3F);
				Operation.Click(735, 128);	// 左上角返回按钮
			} else {
				Operation.Click(1145 + 37 * SQUAD_NUMBER, 870);	// 选择队列
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(960, 470);	// 出战按钮
				yield return new EditorWaitForSeconds(0.5F);
				Operation.Click(30, 250);	// 跳过按钮
				yield return new EditorWaitForSeconds(5F);
				Operation.Click(960, 910);	// 返回按钮
				yield return new EditorWaitForSeconds(0.3F);
				Operation.Click(735, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.3F);
				Operation.Click(735, 128);	// 左上角返回按钮
			}
			// 休息5秒，避免出错时一直受控不能操作
			yield return new EditorWaitForSeconds(5);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
