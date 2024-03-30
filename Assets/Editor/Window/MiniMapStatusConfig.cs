/*
 * @Author: wangyun
 * @CreateTime: 2024-03-30 18:08:32 761
 * @LastEditor: wangyun
 * @EditTime: 2024-03-30 18:08:32 765
 */

using UnityEngine;
using UnityEditor;

public class MiniMapStatusConfig : PrefsEditorWindow<MiniMapStatus> {
	[MenuItem("Tools_Window/Default/MiniMapStatus", false, -1)]
	private static void Open() {
		GetWindow<MiniMapStatusConfig>("小地图状态").Show();
	}
	
	private void OnGUI() {
		MiniMapStatus.KEEP_SHOWING = GUILayout.Toggle(MiniMapStatus.KEEP_SHOWING, "保持展开状态", "Button");
		GUILayout.Space(5F);
		if (MiniMapStatus.IsRunning) {
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