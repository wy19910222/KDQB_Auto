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
	[MenuItem("Tools_Window/Default/FreeDiamond", false, 20)]
	private static void Open() {
		GetWindow<FreeDiamondConfig>("周卡免费钻石").Show();
	}
	
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		FreeDiamond.LEFT_COUNT = EditorGUILayout.IntSlider("剩余次数", FreeDiamond.LEFT_COUNT, 0, 20);
		if (GUILayout.Button("重置", GUILayout.Width(EditorGUIUtility.fieldWidth))) {
			FreeDiamond.LEFT_COUNT = 20;
		}
		EditorGUILayout.EndHorizontal();
		FreeDiamond.TAB_ORDER = EditorGUILayout.IntSlider("标签排序（周卡排第几个）", FreeDiamond.TAB_ORDER, 1, 10);
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
	public static int LEFT_COUNT = 20;	// 剩余次数
	public static int TAB_ORDER = 3;	// 周卡页面排序
	public static float INTERVAL = 16;	// 监测间隔
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartFreeDiamond", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"周卡免费钻石自动点击已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopFreeDiamond", priority = -1)]
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
			if (LEFT_COUNT <= 0) {
				continue;
			}
			
			// 有窗口打开着
			if (Recognize.IsWindowCovered) {
				continue;
			}

			// 只有是世界界面近景或主城界面，才执行
			switch (Recognize.CurrentScene) {
				case Recognize.Scene.FIGHTING:
				case Recognize.Scene.OUTSIDE when Recognize.IsOutsideFaraway:
					continue;
			}

			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(FreeDiamond);
			
			Operation.Click(1820, 136);	// 商城按钮
			yield return new EditorWaitForSeconds(0.2F);
			
			const int TAB_WIDTH = 137;
			int orderOffsetX = (TAB_ORDER - 4) * TAB_WIDTH;
			while (orderOffsetX > 0) {
				const int dragDistance = TAB_WIDTH * 4;
				// 往左拖动
				var ie = Operation.NoInertiaDrag(1190, 200, 1190 - dragDistance, 200, 0.5F);
				while (ie.MoveNext()) {
					yield return ie.Current;
				}
				yield return new EditorWaitForSeconds(0.1F);
				orderOffsetX -= dragDistance;
			}
			Debug.Log("活动标签页");
			Operation.Click(1190 + orderOffsetX, 200);	// 周卡标签
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(1125, 310);	// 领取按钮
			LEFT_COUNT--;
			
			for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.2F);
			}

			Task.CurrentTask = null;
			
			yield return new EditorWaitForSeconds(INTERVAL);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
