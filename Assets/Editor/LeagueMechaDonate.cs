/*
 * @Author: wangyun
 * @CreateTime: 2023-10-24 04:22:33 341
 * @LastEditor: wangyun
 * @EditTime: 2023-10-24 04:22:33 346
 */

using System.Collections;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class LeagueMechaDonateConfig : PrefsEditorWindow<LeagueMechaDonate> {
	[MenuItem("Window/LeagueMechaDonate")]
	private static void Open() {
		GetWindow<LeagueMechaDonateConfig>("联盟机甲捐献").Show();
	}
	
	private void OnGUI() {
		LeagueMechaDonate.INTERVAL = EditorGUILayout.IntSlider("尝试捐献间隔（秒）", LeagueMechaDonate.INTERVAL, 120, 1800);
		GUILayout.Space(5F);
		if (LeagueMechaDonate.IsRunning) {
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

public class LeagueMechaDonate {
	public static int INTERVAL = 300;	// 点击间隔
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartLeagueMechaDonate", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"联盟机甲捐献尝试已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopLeagueMechaDonate", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("联盟机甲捐献尝试已关闭");
		}
	}

	private static IEnumerator Update() {
		// bool prevIsMarshalTime = false;
		while (true) {
			yield return null;
			
			if (Recognize.CurrentScene == Recognize.Scene.FIGHTING) {
				Debug.Log("处于出战界面，不执行操作");
				continue;
			}
			if (Recognize.IsWindowCovered) {
				Debug.Log("有窗口覆盖，不执行操作");
				continue;
			}
			
			Operation.Click(1870, 710);	// 联盟按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(830, 620);	// 联盟活动按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(960, 300);	// 联盟机甲
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(1170, 200);	// 排行奖励按钮
			yield return new EditorWaitForSeconds(0.5F);
			bool isInRank = Recognize.IsLeagueMechaDonateInRank;
			Operation.Click(720, 128);	// 点击窗口外关闭窗口
			yield return new EditorWaitForSeconds(0.1F);
			if (!isInRank) {
				Operation.Click(960, 960);	// 捐献按钮
				yield return new EditorWaitForSeconds(0.3F);
				if (Recognize.IsLeagueMechaDonateConfirming) {
					Operation.Click(960, 686);	// 兑换按钮
					yield return new EditorWaitForSeconds(0.2F);
					Operation.Click(1167, 353);	// 关闭按钮
					yield return new EditorWaitForSeconds(0.2F);
				}
			}
			while (Recognize.IsWindowCovered) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.1F);
			}
			yield return new EditorWaitForSeconds(INTERVAL);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
