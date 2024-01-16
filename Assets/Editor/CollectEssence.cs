/*
 * @Author: wangyun
 * @CreateTime: 2023-10-24 04:22:33 341
 * @LastEditor: wangyun
 * @EditTime: 2023-10-24 04:22:33 346
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class CollectEssenceConfig : PrefsEditorWindow<CollectEssence> {
	[MenuItem("Tools_Window/NewWorld/CollectEssence", false, 21)]
	private static void Open() {
		GetWindow<CollectEssenceConfig>("收取精华").Show();
	}
	
	private void OnGUI() {
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		TimeSpan ts = CollectEssence.NEXT_TIME - DateTime.Now;
		int hours = EditorGUILayout.IntField("下次尝试时间", ts.Hours);
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 8F;
		int minutes = EditorGUILayout.IntField(":", ts.Minutes);
		int seconds = EditorGUILayout.IntField(":", ts.Seconds);
		EditorGUIUtility.labelWidth = prevLabelWidth;
		if (EditorGUI.EndChangeCheck()) {
			CollectEssence.NEXT_TIME = DateTime.Now + new TimeSpan(hours, minutes, seconds);
		}
		EditorGUILayout.EndHorizontal();
		CollectEssence.INTERVAL = EditorGUILayout.IntSlider("尝试收取间隔（秒）", CollectEssence.INTERVAL, 900, 5400);
		GUILayout.Space(5F);
		EditorGUILayout.BeginHorizontal();
		if (CollectEssence.IsRunning) {
			if (GUILayout.Button("关闭")) {
				IsRunning = false;
			}
		} else {
			if (GUILayout.Button("开启")) {
				IsRunning = true;
			}
		}
		if (GUILayout.Button("单次执行", GUILayout.Width(60F))) {
			EditorApplication.ExecuteMenuItem("Tools_Task/OnceCollectEssence");
		}
		EditorGUILayout.EndHorizontal();
	}
	
	private void Update() {
		Repaint();
	}
}

public class CollectEssence {
	public static int INTERVAL = 3600;	// 点击间隔
	public static DateTime NEXT_TIME = DateTime.Now;
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartCollectEssence", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"收取精华已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopCollectEssence", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("收取精华已关闭");
		}
	}

	private static IEnumerator Update() {
		// bool prevIsMarshalTime = false;
		Vector2Int[] positions = new[] {
			new Vector2Int(965, 435),
			new Vector2Int(895, 355),
			new Vector2Int(1030, 355),
			new Vector2Int(840, 440),
			new Vector2Int(1085, 440),
			new Vector2Int(895, 515),
			new Vector2Int(1030, 515),
		};
		while (true) {
			yield return null;
			if (DateTime.Now < NEXT_TIME) {
				continue;
			}
			
			if (Recognize.CurrentScene == Recognize.Scene.FIGHTING) {
				Debug.Log("处于出战界面，不执行操作");
				continue;
			}
			if (Recognize.IsWindowCovered) {
				Debug.Log("有窗口覆盖，不执行操作");
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
			Task.CurrentTask = nameof(CollectEssence);

			Debug.Log("新世界按钮");
			switch (Recognize.CurrentScene) {
				case Recognize.Scene.OUTSIDE:
					Operation.Click(1875, 285);	// 新世界按钮
					break;
				case Recognize.Scene.INSIDE:
					Operation.Click(45, 245);	// 新世界按钮
					break;
			}
			yield return new EditorWaitForSeconds(0.5F);
			Debug.Log("净化涡轮");
			Operation.Click(775, 930);	// 净化涡轮
			yield return new EditorWaitForSeconds(0.2F);
			Debug.Log("点击精华");
			foreach (Vector2Int position in positions) {
				Operation.Click(position.x, position.y);	// 点击精华
				yield return new EditorWaitForSeconds(0.5F);
				Operation.Click(800, 600);	// 点击空白处
				yield return new EditorWaitForSeconds(0.2F);
			}
			Debug.Log("关闭窗口");
			Operation.Click(720, 128);	// 左上角返回按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(720, 128);	// 左上角返回按钮
			yield return new EditorWaitForSeconds(0.2F);
			for (int i = 0; i < 5 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.1F);
			}

			NEXT_TIME = DateTime.Now + new TimeSpan(0, 0, INTERVAL);
			
			Task.CurrentTask = null;
		}
		// ReSharper disable once IteratorNeverReturns
	}

	[MenuItem("Tools_Task/OnceCollectEssence", priority = -1)]
	private static void ExecuteOnce() {
		Debug.Log($"单次执行联盟机甲捐献尝试");
		EditorCoroutineManager.StartCoroutine(IEExecuteOnce());
	}
	private static IEnumerator IEExecuteOnce() {
		if (Task.CurrentTask != null) {
			Debug.LogError($"正在执行【{Task.CurrentTask}】, 请稍后！");
			yield break;
		}
		DateTime prevNextTime = NEXT_TIME;
		NEXT_TIME = DateTime.Now;
		do {
			yield return null;
		} while (Task.CurrentTask != null);
		NEXT_TIME = prevNextTime;
	}
}
