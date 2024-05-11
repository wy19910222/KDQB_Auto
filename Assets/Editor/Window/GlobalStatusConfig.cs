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
		GlobalStatus.Enable();
		EditorCoroutineManager.Enable = Prefs.Get($"EditorCoroutineManager.Enable", true);
	}
	protected void OnDisable() {
		GlobalStatus.Disable();
		Prefs.Set($"EditorCoroutineManager.Enable", EditorCoroutineManager.Enable);
	}

	private void OnGUI() {
		EditorCoroutineManager.Enable = GUILayout.Toggle(EditorCoroutineManager.Enable, "辅助开启", "Button");
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Toggle("无人值守", GlobalStatus.IsUnattended);
		EditorGUILayout.LabelField($"{GlobalStatus.UnattendedDuration / 1000_000_0}/{Global.UNATTENDED_THRESHOLD / 1000_000_0}");
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("当前任务", GUILayout.Width(EditorGUIUtility.labelWidth));
		EditorGUILayout.LabelField(Task.CurrentTask ?? "Idle", GUILayout.MinWidth(0));
		if (GUILayout.Button("清除", GUILayout.Width(120F))) {
			Task.CurrentTask = null;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EnumPopup("场景", Recognize.CurrentScene);
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("行军队列", GUILayout.Width(EditorGUIUtility.labelWidth - 1F));
		int busyGroupCount = Recognize.BusyGroupCount;
		if (busyGroupCount == int.MaxValue) {
			busyGroupCount = -1;
		}
		EditorGUILayout.IntField(busyGroupCount, GUILayout.Width(40F - 2F));
		EditorGUILayout.LabelField($"/ {Global.GROUP_COUNT}");
		EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.IntField("体力值", Recognize.Energy, GUILayout.Width(EditorGUIUtility.labelWidth + 40F));
		EditorGUILayout.LabelField($"/ {Global.ENERGY_FULL}");
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.FloatField("窗口覆盖", Recognize.WindowCoveredCount);
		// if (KeyboardUtils.IsRunning) {
		// 	if (GUILayout.Button("UnhookKeyboard")) {
		// 		KeyboardUtils.Unhook();
		// 	}
		// } else {
		// 	if (GUILayout.Button("HookKeyboard")) {
		// 		KeyboardUtils.Hook();
		// 	}
		// }
		// EditorGUILayout.IntField("戴安娜所在队列", Recognize.GetHeroGroupNumber(Recognize.HeroType.DAN));
		// EditorGUILayout.IntField("尤里卡所在队列", Recognize.GetHeroGroupNumber(Recognize.HeroType.YLK));
		// EditorGUILayout.IntField("明日香所在队列", Recognize.GetHeroGroupNumber(Recognize.HeroType.MRX));
	}
	
	private void Update() {
		Repaint();
	}
}