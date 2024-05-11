/*
 * @Author: wangyun
 * @CreateTime: 2024-05-11 15:09:39 704
 * @LastEditor: wangyun
 * @EditTime: 2024-05-11 15:09:39 708
 */

using UnityEditor;

public class GlobalConfig : PrefsEditorWindow<Global> {
	[MenuItem("Tools_Window/Default/Global", false, -1)]
	private static void Open() {
		GetWindow<GlobalConfig>("全局配置").Show();
	}
	
	protected override bool IsRunning {
		get => m_IsRunning;
		set => m_IsRunning = value;
	}

	protected override void OnEnable() {
		string gameRectStr = Prefs.Get<string>($"Operation.CURRENT_GAME_RECT");
		if (!string.IsNullOrEmpty(gameRectStr)) {
			Operation.CURRENT_GAME_RECT = Utils.StringToRect(gameRectStr);
		}
		base.OnEnable();
	}
	protected override void OnDisable() {
		Prefs.Set($"Operation.CURRENT_GAME_RECT", Utils.RectToString(Operation.CURRENT_GAME_RECT));
		base.OnDisable();
	}

	private void OnGUI() {
		Operation.CURRENT_GAME_RECT = EditorGUILayout.RectIntField("游戏范围", Operation.CURRENT_GAME_RECT);
		Global.UNATTENDED_THRESHOLD = EditorGUILayout.LongField("无人值守阈值（秒）", Global.UNATTENDED_THRESHOLD / 1000_000_0) * 1000_000_0;
		Global.GROUP_COUNT = EditorGUILayout.IntSlider("拥有行军队列", Global.GROUP_COUNT, 0, 7);
		Global.ENERGY_FULL = EditorGUILayout.IntField("体力上限", Global.ENERGY_FULL);
	}
	
	private void Update() {
		Repaint();
	}
}