/*
 * @Author: wangyun
 * @CreateTime: 2024-03-30 18:08:32 761
 * @LastEditor: wangyun
 * @EditTime: 2024-03-30 18:08:32 765
 */

using System.Collections;
using UnityEngine;
using UnityEditor;

public class MiniMapStatus {
	public static bool KEEP_SHOWING = false;	// 监测间隔
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartMiniMapStatus", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"小地图状态监管已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopMiniMapStatus", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("小地图状态监管已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			
			bool? isMiniMapShowing = Recognize.IsMiniMapShowing;
			if (isMiniMapShowing != null && isMiniMapShowing != KEEP_SHOWING) {
				if (Recognize.LeftAreaDeltaY >= 0) {
					if (Task.CurrentTask != null) {
						continue;
					}
					Task.CurrentTask = nameof(MiniMapStatus);
					Debug.Log("小地图展开收起按钮");
					Operation.Click(27, 161 + Recognize.LeftAreaDeltaY);	// 修理按钮按钮
				
					Task.CurrentTask = null;
					
					yield return new EditorWaitForSeconds(1F);
				}
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
