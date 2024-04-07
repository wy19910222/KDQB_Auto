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
		BlockBreak.FIGHTING_BLOCK_SECONDS = EditorGUILayout.IntSlider("判定异常阈值（秒）", BlockBreak.FIGHTING_BLOCK_SECONDS, 20, 60);
		if (BlockBreak.FightingBlockIsRunning) {
			if (GUILayout.Button("关闭")) {
				EditorApplication.ExecuteMenuItem("Tools_Task/StopFightingBlockBreak");
			}
		} else {
			if (GUILayout.Button("开启")) {
				EditorApplication.ExecuteMenuItem("Tools_Task/StartFightingBlockBreak");
			}
		}
		GUILayout.Space(5F);
		EditorGUILayout.HelpBox(EditorGUIUtility.TrTempContent("存在窗口且一直没有层数变化，则认为处于异常阻塞状态"));
		BlockBreak.WINDOW_BLOCK_SECONDS = EditorGUILayout.IntSlider("判定异常阈值（秒）", BlockBreak.WINDOW_BLOCK_SECONDS, 20, 60);
		if (BlockBreak.WindowBlockIsRunning) {
			if (GUILayout.Button("关闭")) {
				EditorApplication.ExecuteMenuItem("Tools_Task/StopWindowBlockBreak");
			}
		} else {
			if (GUILayout.Button("开启")) {
				EditorApplication.ExecuteMenuItem("Tools_Task/StartWindowBlockBreak");
			}
		}
	}
}