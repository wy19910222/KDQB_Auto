/*
 * @Author: wangyun
 * @CreateTime: 2023-10-31 07:02:36 018
 * @LastEditor: wangyun
 * @EditTime: 2023-10-31 07:02:36 022
 */

using System.Collections;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class CutPriceConfig : PrefsEditorWindow<CutPrice> {
	[MenuItem("Tools_Window/Activity/CutPrice")]
	private static void Open() {
		GetWindow<CutPriceConfig>("砍一刀").Show();
	}
	
	private void OnGUI() {
		if (CutPrice.IsRunning) {
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

public class CutPrice {
	private static Color32[,] s_CachedSharerName;	// 缓存分享者的昵称
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartCutPrice", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"自动砍一刀已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopCutPrice", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动砍一刀已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			if (Recognize.CanCutPrice) {
				Color32[,] sharerName = Operation.GetColorsOnScreen(899, 737, 90, 20);
				if (s_CachedSharerName == null || Recognize.ApproximatelyRect(sharerName, s_CachedSharerName) < 0.9F) {
					Operation.Click(960, 880);	// 砍一刀按钮
					s_CachedSharerName = sharerName;
					yield return new EditorWaitForSeconds(0.5F);
					Operation.Click(960, 880);	// 关闭恭喜获得界面
				}
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
