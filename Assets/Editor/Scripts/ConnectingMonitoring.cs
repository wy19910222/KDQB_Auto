/*
 * @Author: wangyun
 * @CreateTime: 2023-11-05 19:03:52 124
 * @LastEditor: wangyun
 * @EditTime: 2023-11-05 19:03:52 128
 */

using System.Collections;
using UnityEngine;
using UnityEditor;

public class ConnectingMonitoring {
	public static int INTERVAL = 10;	// 监测间隔
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartConnectingMonitoring", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"网络状态监测已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopConnectingMonitoring", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("网络状态监测已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			while (Recognize.IsNetworkDisconnected) {
				Operation.Click(960, 630);	// 确定按钮
				yield return new EditorWaitForSeconds(0.5F);
			}
			while (Recognize.IsMigrateInviting) {
				Operation.Click(1140, 408);	// 关闭按钮
				yield return new EditorWaitForSeconds(0.5F);
			}
			while (Recognize.IsMoreGroupPopup) {
				Operation.Click(1168, 308);	// 关闭按钮
				yield return new EditorWaitForSeconds(0.5F);
			}
			yield return new EditorWaitForSeconds(INTERVAL);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
