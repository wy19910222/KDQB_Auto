/*
 * @Author: wangyun
 * @CreateTime: 2023-11-05 19:03:52 124
 * @LastEditor: wangyun
 * @EditTime: 2023-11-05 19:03:52 128
 */

using UnityEngine;
using UnityEditor;

public class BlockBreakConfig : PrefsEditorWindow<BlockBreak> {
	[MenuItem("Tools_Window/Default/BlockBreak", false, -99)]
	private static void Open() {
		GetWindow<BlockBreakConfig>("异常阻塞处理").Show();
	}
	
	private void OnGUI() {
		EditorGUILayout.HelpBox(EditorGUIUtility.TrTempContent("停留在士兵选择界面超过一定时间，则认为处于异常阻塞状态"));
		BlockBreak.SECONDS = EditorGUILayout.IntSlider("判定异常阈值（秒）", BlockBreak.SECONDS, 20, 60);
		GUILayout.Space(5F);
		if (BlockBreak.IsRunning) {
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