/*
 * @Author: wangyun
 * @CreateTime: 2023-12-28 12:59:39 966
 * @LastEditor: wangyun
 * @EditTime: 2023-12-28 12:59:39 971
 */

using UnityEditor;
using UnityEngine;

public class GlobalStatus : EditorWindow {
	[MenuItem("Tools_Window/Default/GlobalStatus", false, -1)]
	private static void Open() {
		GetWindow<GlobalStatus>("全局状态").Show();
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