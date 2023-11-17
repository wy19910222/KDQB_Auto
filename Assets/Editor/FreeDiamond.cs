/*
 * @Author: wangyun
 * @CreateTime: 2023-11-05 19:12:07 765
 * @LastEditor: wangyun
 * @EditTime: 2023-11-05 19:12:07 769
 */

using System.Collections;
using UnityEngine;
using UnityEditor;

public class FreeDiamondConfig : PrefsEditorWindow<FreeDiamond> {
	[MenuItem("Window/FreeDiamond")]
	private static void Open() {
		GetWindow<FreeDiamondConfig>("周卡免费钻石").Show();
	}
	
	private void OnGUI() {
		FreeDiamond.INTERVAL = EditorGUILayout.Slider("点击间隔", FreeDiamond.INTERVAL, 15.5F, 16.5F);
		GUILayout.Space(5F);
		if (FreeDiamond.IsRunning) {
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

public class FreeDiamond {
	public static float INTERVAL = 16;	// 监测间隔
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartFreeDiamond", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"周卡免费钻石自动点击已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopFreeDiamond", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("周卡免费钻石自动点击已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			// 有窗口打开着
			if (Recognize.IsWindowCovered) {
				continue;
			}

			// 如果是世界界面远景，则没有显示活动按钮，需要先切换到近景
			switch (Recognize.CurrentScene) {
				case Recognize.Scene.ARMY_SELECTING:
				case Recognize.Scene.OUTSIDE when Recognize.IsOutsideFaraway:
					continue;
			}
			
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(1820, 136);	// 商城按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(925, 195);	// 周卡标签
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(1125, 310);	// 领取按钮
			
			while (Recognize.IsWindowCovered) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			
			yield return new EditorWaitForSeconds(INTERVAL);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
