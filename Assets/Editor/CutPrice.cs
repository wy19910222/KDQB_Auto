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
	[MenuItem("Window/CutPrice")]
	private static void Open() {
		GetWindow<CutPriceConfig>("砍一刀").Show();
	}
	
	private void OnGUI() {
		CutPrice.INTERVAL = EditorGUILayout.Slider("点击间隔", CutPrice.INTERVAL, 0.1F, 1F);
		GUILayout.Space(5F);
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
	public static float INTERVAL = 0.1F;	// 点击间隔
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartCutPrice", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"自动砍一刀已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopCutPrice", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动砍一刀已关闭");
		}
	}

	private static IEnumerator Update() {
		// bool prevIsMarshalTime = false;
		while (true) {
			yield return null;
			Operation.Click(960, 880);	// 砍一刀按钮
			yield return new EditorWaitForSeconds(0.1F);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
