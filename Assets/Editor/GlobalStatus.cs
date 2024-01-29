/*
 * @Author: wangyun
 * @CreateTime: 2023-12-28 12:59:39 966
 * @LastEditor: wangyun
 * @EditTime: 2023-12-28 12:59:39 971
 */

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class GlobalStatusConfig : EditorWindow {
	[MenuItem("Tools_Window/Default/GlobalStatus", false, -1)]
	private static void Open() {
		GetWindow<GlobalStatusConfig>("全局状态").Show();
	}

	protected void OnEnable() {
		Recognize.GROUP_COUNT = EditorPrefs.GetInt($"Recognize.GROUP_COUNT");
	}
	protected void OnDisable() {
		EditorPrefs.SetInt($"Recognize.GROUP_COUNT", Recognize.GROUP_COUNT);
	}

	private void OnGUI() {
		EditorGUILayout.RectField("游戏范围", Operation.CURRENT_GAME_RECT);
		EditorGUILayout.Toggle("无人值守", GlobalStatus.IsUnattended);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("当前任务", GUILayout.Width(EditorGUIUtility.labelWidth));
		EditorGUILayout.LabelField(Task.CurrentTask ?? "Idle", GUILayout.MinWidth(0));
		if (GUILayout.Button("清除", GUILayout.Width(120F))) {
			Task.CurrentTask = null;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EnumPopup("场景", Recognize.CurrentScene);
		Recognize.GROUP_COUNT = EditorGUILayout.IntSlider("拥有行军队列", Recognize.GROUP_COUNT, 0, 7);
		EditorGUILayout.IntField("忙碌队列", Recognize.BusyGroupCount);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.IntField("体力值", Recognize.energy, GUILayout.Width(EditorGUIUtility.labelWidth + 30F));
		EditorGUILayout.LabelField("/", GUILayout.Width(8F));
		Recognize.ENERGY_FULL = EditorGUILayout.IntField(Recognize.ENERGY_FULL);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.FloatField("窗口覆盖", Recognize.WindowCoveredCount);
		// EditorGUILayout.IntField("戴安娜所在队列", Recognize.GetHeroGroupNumber(Recognize.HeroType.DAN));
		// EditorGUILayout.IntField("尤里卡所在队列", Recognize.GetHeroGroupNumber(Recognize.HeroType.YLK));
		// EditorGUILayout.IntField("明日香所在队列", Recognize.GetHeroGroupNumber(Recognize.HeroType.MRX));
	}
	
	private void Update() {
		Repaint();
	}
}

public static class GlobalStatus {
	public static bool IsUnattended { get; private set; }
	public const long UNATTENDED_THRESHOLD = 30 * 1000 * 1000 * 10; // 30秒

	private static EditorCoroutine s_CO;
	

	[InitializeOnLoadMethod]
	private static void Enable() {
		Disable();
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
		Debug.Log($"全局状态监测已开启");
	}

	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("全局状态监测已关闭");
		}
	}

	private static IEnumerator Update() {
		Vector2Int prevMousePos = MouseUtils.GetMousePos();
		DateTime startDT = DateTime.Now;
		while (true) {
			yield return null;
			Vector2Int nextMousePos = MouseUtils.GetMousePos();
			DateTime now = DateTime.Now;
			if (nextMousePos.x != prevMousePos.x || nextMousePos.y != prevMousePos.y) {
				prevMousePos = nextMousePos;
				startDT = now;
				IsUnattended = false;
			} else if ((now - startDT).Ticks > UNATTENDED_THRESHOLD) {
				IsUnattended = true;
			}
		}
	}
}