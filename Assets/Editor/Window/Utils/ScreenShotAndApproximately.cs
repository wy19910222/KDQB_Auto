/*
 * @Author: wangyun
 * @CreateTime: 2023-11-05 19:03:52 124
 * @LastEditor: wangyun
 * @EditTime: 2023-11-05 19:03:52 128
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ScreenShotAndApproximately : EditorWindow {
	[MenuItem("Tools_Window/ScreenShotAndApproximately", false, 22)]
	private static void Open() {
		GetWindow<ScreenShotAndApproximately>("截取和对比").Show();
	}

	[SerializeField]
	private RectInt rect = new RectInt();
	[SerializeField]
	private string filename = string.Empty;

	private void OnGUI() {
		EditorGUI.BeginChangeCheck();
		RectInt newRect = EditorGUILayout.RectIntField(rect);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(this, "Rect");
			rect = newRect;
		}
		string newFilename = EditorGUILayout.TextField(filename);
		if (newFilename != filename) {
			Undo.RecordObject(this, "Filename");
			filename = newFilename;
		}
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("截取")) {
			if (!string.IsNullOrEmpty(filename)) {
				Operation.Screenshot(rect.x, rect.y, rect.width, rect.height, $"{Application.dataPath}/PersistentData/Textures/{filename}.png");
			} else {
				Debug.LogError("Filename is empty!");
			}
		}
		if (GUILayout.Button("对比")) {
			Color32[,] targetColors = Operation.GetFromFile($"PersistentData/Textures/{filename}.png");
			Color32[,] realColors = Operation.GetColorsOnScreen(rect.x, rect.y, rect.width, rect.height);
			Debug.Log(Recognize.ApproximatelyRect(realColors, targetColors));
		}
		EditorGUILayout.EndHorizontal();
	}
}