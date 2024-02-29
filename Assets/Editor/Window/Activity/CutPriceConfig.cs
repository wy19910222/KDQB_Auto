/*
 * @Author: wangyun
 * @CreateTime: 2023-10-31 07:02:36 018
 * @LastEditor: wangyun
 * @EditTime: 2023-10-31 07:02:36 022
 */

using UnityEditor;
using UnityEngine;

public class CutPriceConfig : PrefsEditorWindow<CutPrice> {
	[MenuItem("Tools_Window/Activity/CutPrice")]
	private static void Open() {
		GetWindow<CutPriceConfig>("砍一刀").Show();
	}
	
	private void OnGUI() {
		if (CutPrice.IsRunning) {
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