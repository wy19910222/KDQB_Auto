/*
 * @Author: wangyun
 * @CreateTime: 2023-10-31 07:02:42 199
 * @LastEditor: wangyun
 * @EditTime: 2023-10-31 07:02:42 204
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class AttackMarshalConfig : PrefsEditorWindow<AttackMarshal> {
	[MenuItem("Window/AttackMarshal")]
	private static void Open() {
		GetWindow<AttackMarshalConfig>("攻击元帅").Show();
	}

	private readonly GUIStyle m_Style = new GUIStyle();
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		GUIContent content = new GUIContent($"{AttackMarshal.AttackTimes} /");
		float width = m_Style.CalcSize(content).x + 3;
		GUILayout.Space(EditorGUIUtility.labelWidth + 2);
		EditorGUILayout.LabelField(content, "RightLabel", GUILayout.Width(width));
		EditorGUIUtility.labelWidth += width;
		GUILayout.Space(-EditorGUIUtility.labelWidth - 2);
		AttackMarshal.ATTACK_TIMES = EditorGUILayout.IntField("攻击次数", AttackMarshal.ATTACK_TIMES);
		EditorGUIUtility.labelWidth -= width;
		EditorGUILayout.EndHorizontal();
		AttackMarshal.GROUP_COUNT = EditorGUILayout.IntSlider("拥有行军队列", AttackMarshal.GROUP_COUNT, 0, 7);
		AttackMarshal.SQUAD_NUMBER = EditorGUILayout.IntSlider("使用编队号码", AttackMarshal.SQUAD_NUMBER, 1, 8);
		GUILayout.Space(5F);
		if (AttackMarshal.IsRunning) {
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

public class AttackMarshal {
	public static int GROUP_COUNT = 4;	// 拥有行军队列数
	public static int SQUAD_NUMBER = 1;	// 使用编队号码
	public static int ATTACK_TIMES = 5;	// 攻击总次数
	
	public static readonly List<DateTime> s_AttackTimeList = new List<DateTime>();	// 攻击次数
	public static int AttackTimes {	// 攻击次数
		get {
			int times = 0;
			DateTime date = DateTime.Now.Date;
			for (int i = s_AttackTimeList.Count - 1; i >= 0; --i) {
				DateTime dt = s_AttackTimeList[i];
				if (dt > date) {
					++times;
				} else {
					break;
				}
			}
			return times;
		}
	}
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartAttackMarshal", priority = -1)]
	private static void Enable() {
		Disable();
		List<string> switches = new List<string> {
			$"拥有行军队列【{GROUP_COUNT}】",
			$"使用编队【{SQUAD_NUMBER}】"
		};
		Debug.Log($"自动打元帅已开启，{string.Join("，", switches)}");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopAttackMarshal", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动打元帅已关闭");
		}
	}

	private static IEnumerator Update() {
		// bool prevIsMarshalTime = false;
		while (true) {
			yield return null;
			
			int hour = DateTime.Now.Hour;
			bool isMarshalTime = hour is 4 or 12 or 20;
			if (!isMarshalTime) {
				continue;
			}
			Debug.Log($"当前时间：{hour}点");
			
			if (Recognize.BusyGroupCount >= GROUP_COUNT) {
				continue;
			}
			Debug.Log("当前忙碌队列数量: " + Recognize.BusyGroupCount);

			if (Recognize.IsWindowCovered) {	// 如果有窗口覆盖，说明用户正在操作
				continue;
			}
			Debug.Log($"无窗口覆盖");

			int attackTimes = AttackTimes;
			if (attackTimes >= ATTACK_TIMES) {
				continue;
			}
			Debug.Log($"剩余攻击次数：{ATTACK_TIMES - attackTimes}");
			
			if (!Recognize.IsMarshalExist) {
				Debug.Log($"未检测到元帅按钮，尝试切换场景");
				Operation.Click(1170, 970);	// 右下角主城与世界切换按钮
				yield return new EditorWaitForSeconds(1F);
				if (Recognize.CurrentScene == Recognize.Scene.INSIDE) {
					Operation.Click(1170, 970);	// 右下角主城与世界切换按钮
					yield return new EditorWaitForSeconds(1F);
				}
				if (!Recognize.IsMarshalExist) {
					Debug.Log($"切换场景后还是没有元帅");
					yield return new EditorWaitForSeconds(300F);	// 5分钟后再重新尝试
					continue;
				}
			}
			
			Debug.Log("当前忙碌队列数量: " + Recognize.BusyGroupCount);
			if (Recognize.BusyGroupCount >= GROUP_COUNT) {
				continue;
			}
			
			Debug.Log("元帅按钮");
			Operation.Click(800, 800);	// 元帅按钮
			yield return new EditorWaitForSeconds(0.3F);
			Debug.Log("快速搜索按钮");
			Operation.Click(960, 720);	// 快速搜索按钮
			yield return new EditorWaitForSeconds(0.3F);
			Debug.Log("选中目标");
			Operation.Click(960, 520);	// 选中目标
			yield return new EditorWaitForSeconds(0.2F);
			Debug.Log("攻击按钮");
			Operation.Click(1050, 840);	// 攻击按钮
			yield return new EditorWaitForSeconds(0.2F);
			if (Recognize.CurrentScene == Recognize.Scene.ARMY_SELECTING) {
				Debug.Log($"选择队列{SQUAD_NUMBER}");
				Operation.Click(1145 + 37 * SQUAD_NUMBER, 870);	// 选择队列
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("出战按钮");
				Operation.Click(960, 470);	// 出战按钮
				Debug.Log("出发");
				s_AttackTimeList.Add(DateTime.Now);
			}
			yield return new EditorWaitForSeconds(0.5F);
			// 如果还停留在出征界面(比如点出战按钮前一瞬间元帅没了)，则退出
			if (Recognize.CurrentScene == Recognize.Scene.ARMY_SELECTING) {
				Debug.Log("退出按钮");
				Operation.Click(30, 140);	// 退出按钮
				yield return new EditorWaitForSeconds(0.3F);
				Debug.Log("确认退出按钮");
				Operation.Click(1064, 634);	// 确认退出按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			
			yield return new EditorWaitForSeconds(5);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
