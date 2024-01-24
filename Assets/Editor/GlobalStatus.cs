/*
 * @Author: wangyun
 * @CreateTime: 2023-12-28 12:59:39 966
 * @LastEditor: wangyun
 * @EditTime: 2023-12-28 12:59:39 971
 */

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
	}
	
	private void Update() {
		Repaint();
	}
}