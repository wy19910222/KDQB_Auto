/*
 * @Author: wangyun
 * @CreateTime: 2024-04-05 01:43:34 064
 * @LastEditor: wangyun
 * @EditTime: 2024-04-05 01:43:34 068
 */

using UnityEngine;
using UnityEditor;

public class DailySuppliesConfig : PrefsEditorWindow<DailySupplies> {
	[MenuItem("Tools_Window/Default/DailySupplies", false, 20)]
	private static void Open() {
		GetWindow<DailySuppliesConfig>("每日补给").Show();
	}
	
	private void OnGUI() {
		DailySupplies.FAILED_COOLDOWN_MINUTE = Mathf.Max(EditorGUILayout.IntField("失败后冷却时间（分钟）", DailySupplies.FAILED_COOLDOWN_MINUTE), 0);
		GUILayout.Space(5F);
		if (DailySupplies.IsRunning) {
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