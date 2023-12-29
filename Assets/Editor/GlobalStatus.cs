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
		EditorGUILayout.IntField("体力值", Recognize.energy);
		EditorGUILayout.FloatField("窗口覆盖", Recognize.WindowCoveredCount);
	}

	private void OnBecameVisible() {
		EditorApplication.update += Repaint;
	}

	private void OnBecameInvisible() {
		EditorApplication.update -= Repaint;
	}
}