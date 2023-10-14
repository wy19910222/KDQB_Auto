/*
 * @Author: wangyun
 * @CreateTime: 2023-10-12 00:00:51 751
 * @LastEditor: wangyun
 * @EditTime: 2023-10-12 00:00:51 756
 */

using System.Collections;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class DeepSeaConfig : PrefsEditorWindow<DeepSea> {
	[MenuItem("Window/DeepSea")]
	private static void Open() {
		GetWindow<DeepSeaConfig>("深海寻宝").Show();
	}
	
	private void OnGUI() {
		DeepSea.ACTIVITY_ORDER = EditorGUILayout.IntSlider("活动排序（活动排在第几个）", DeepSea.ACTIVITY_ORDER, 1, 20);
		DeepSea.DETECTOR_COUNT = EditorGUILayout.IntSlider("拥有探测器", DeepSea.DETECTOR_COUNT, 1, 3);
		DeepSea.CLICK_INTERVAL = Mathf.Clamp(EditorGUILayout.IntField("点击间隔时间（秒）", DeepSea.CLICK_INTERVAL), 60, 1200);
		GUILayout.Space(5F);
		if (DeepSea.IsRunning) {
			if (GUILayout.Button("关闭")) {
				EditorApplication.ExecuteMenuItem("Assets/StopDeepSea");
			}
		} else {
			if (GUILayout.Button("开启")) {
				EditorApplication.ExecuteMenuItem("Assets/StartDeepSea");
			}
		}
	}
}

public class DeepSea {
	public static int ACTIVITY_ORDER = 8;	// 活动排序
	public static int DETECTOR_COUNT = 3;	// 拥有探测器数量
	public static int CLICK_INTERVAL = 300;	// 点击间隔
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Assets/StartDeepSea", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"深海寻宝自动点击已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopDeepSea", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("深海寻宝自动点击已关闭");
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
			if (Recognize.CurrentScene == Recognize.Scene.OUTSIDE && Recognize.IsOutsideFaraway) {
				while (Recognize.IsOutsideFaraway) {
					Vector2Int oldPos = MouseUtils.GetMousePos();
					MouseUtils.SetMousePos(960, 540);	// 鼠标移动到屏幕中央
					MouseUtils.ScrollWheel(1);
					MouseUtils.SetMousePos(oldPos.x, oldPos.y);
					yield return new EditorWaitForSeconds(0.1F);
				}
			}
			Debug.Log("活动按钮");
			Operation.Click(1880, 290);	// 活动按钮
			yield return new EditorWaitForSeconds(0.5F);
			Debug.Log("拖动以显示活动标签页");
			int orderOffsetX = (ACTIVITY_ORDER - 4) * 137;
			while (orderOffsetX > 0) {
				int dragDistance = 138 * 4;
				// 往左上拖动
				var ie = Operation.NoInertiaDrag(1190, 200, 1190 - dragDistance, 200, 0.5F);
				while (ie.MoveNext()) {
					yield return ie.Current;
				}
				yield return new EditorWaitForSeconds(0.1F);
				orderOffsetX -= dragDistance;
			}
			Debug.Log("活动标签页");
			Operation.Click(1190 + orderOffsetX, 200);	// 活动标签页
			yield return new EditorWaitForSeconds(0.1F);
			Debug.Log("点击探测器");
			for (int i = 0; i < DETECTOR_COUNT; ++i) {
				Debug.Log($"点击探测器{i + 1}");
				Operation.Click(808 + 153 * i, 870);	// 探测器
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(808 + 153 * i, 870);	// 探测器
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(808 + 153 * i, 870);	// 探测器
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(808 + 153 * i, 870);	// 探测器
				yield return new EditorWaitForSeconds(0.2F);
			}
			while (Recognize.IsWindowCovered) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(735, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			
			// 60秒后再重新检查
			yield return new EditorWaitForSeconds(CLICK_INTERVAL);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
