﻿/*
 * @Author: wangyun
 * @CreateTime: 2023-11-05 19:03:52 124
 * @LastEditor: wangyun
 * @EditTime: 2023-11-05 19:03:52 128
 */

using UnityEngine;
using UnityEditor;

public class ScreenShotAndApproximately : EditorWindow {
	[MenuItem("Tools_Window/ScreenShotAndApproximately", false, 22)]
	private static void Open() {
		GetWindow<ScreenShotAndApproximately>("截取和对比").Show();
	}

	[SerializeField]
	private RectInt m_Rect = new RectInt();
	[SerializeField]
	private string m_Filename = string.Empty;
	[SerializeField]
	private RectInt m_ApproximatelyRect = new RectInt();
	[SerializeField]
	private string m_ApproximatelyFilename = string.Empty;
	[SerializeField]
	private float m_ApproximatelyThresholdMulti = 1;
	
	[SerializeField]
	private Vector2Int m_LogColorPos = new Vector2Int();
	[SerializeField]
	private bool m_LogMousePosColor;

	private void OnGUI() {
		const float MAX_BTN_WIDTH = 70F;
		const float MIN_SPACE_WIDTH = 70F;
		float singleLineHeight = EditorGUIUtility.singleLineHeight;
		float labelWidth = EditorGUIUtility.labelWidth;
		float spaceWidth = Mathf.Max(labelWidth - MAX_BTN_WIDTH, MIN_SPACE_WIDTH);
		float btnWidth = labelWidth - spaceWidth;
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(spaceWidth);
		if (GUILayout.Button("与对比相同", GUILayout.Width(btnWidth))) {
			m_Rect = m_ApproximatelyRect;
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(-singleLineHeight - 2F);
		EditorGUI.BeginChangeCheck();
		RectInt newRect = EditorGUILayout.RectIntField("截取范围", m_Rect);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(this, "Rect");
			m_Rect = newRect;
		}
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(spaceWidth);
		if (GUILayout.Button("与对比相同", GUILayout.Width(btnWidth))) {
			m_Filename = m_ApproximatelyFilename;
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(-singleLineHeight - 2F);
		string newFilename = EditorGUILayout.TextField("保存文件名", m_Filename);
		if (newFilename != m_Filename) {
			Undo.RecordObject(this, "Filename");
			m_Filename = newFilename;
		}
		if (GUILayout.Button("截取")) {
			if (!string.IsNullOrEmpty(m_Filename)) {
				Operation.Screenshot(m_Rect.x, m_Rect.y, m_Rect.width, m_Rect.height, $"PersistentData/Textures/{m_Filename}.png");
			} else {
				Debug.LogError("Filename is empty!");
			}
		}
		
		Rect rect1 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect1 = new Rect(rect1.x, rect1.y + 4.5F, rect1.width, 1);
		EditorGUI.DrawRect(wireRect1, Color.gray);

		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(spaceWidth);
		if (GUILayout.Button("与截取相同", GUILayout.Width(btnWidth))) {
			m_ApproximatelyRect = m_Rect;
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(-singleLineHeight - 2F);
		EditorGUI.BeginChangeCheck();
		RectInt newApproximatelyRect = EditorGUILayout.RectIntField("对比范围", m_ApproximatelyRect);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(this, "ApproximatelyRect");
			m_ApproximatelyRect = newApproximatelyRect;
		}
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(spaceWidth);
		if (GUILayout.Button("与截取相同", GUILayout.Width(btnWidth))) {
			m_ApproximatelyFilename = m_Filename;
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(-singleLineHeight - 2F);
		string newApproximatelyFilename = EditorGUILayout.TextField("对比文件名", m_ApproximatelyFilename);
		if (newApproximatelyFilename != m_ApproximatelyFilename) {
			Undo.RecordObject(this, "ApproximatelyFilename");
			m_ApproximatelyFilename = newApproximatelyFilename;
		}
		float newApproximatelyThresholdMulti = EditorGUILayout.FloatField("对比阈值缩放", m_ApproximatelyThresholdMulti);
		if (!Mathf.Approximately(newApproximatelyThresholdMulti, m_ApproximatelyThresholdMulti)) {
			Undo.RecordObject(this, "ApproximatelyThresholdMulti");
			m_ApproximatelyThresholdMulti = newApproximatelyThresholdMulti;
		}
		if (GUILayout.Button("对比")) {
			Color32[,] targetColors = Operation.GetFromFile($"PersistentData/Textures/{m_ApproximatelyFilename}.png");
			Color32[,] realColors = Operation.GetColorsOnScreen(m_ApproximatelyRect.x, m_ApproximatelyRect.y, m_ApproximatelyRect.width, m_ApproximatelyRect.height);
			Debug.Log(Recognize.ApproximatelyRect(realColors, targetColors, m_ApproximatelyThresholdMulti));
		}
		
		Rect rect2 = GUILayoutUtility.GetRect(0, 10);
		Rect wireRect2 = new Rect(rect2.x, rect2.y + 4.5F, rect2.width, 1);
		EditorGUI.DrawRect(wireRect2, Color.gray);

		Vector2Int mousePos = Operation.GetMousePos();
		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.Vector2IntField("鼠标坐标", mousePos);
		EditorGUI.EndDisabledGroup();
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(spaceWidth);
		m_LogMousePosColor = GUILayout.Toggle(m_LogMousePosColor, "鼠标位置", "Button", GUILayout.Width(btnWidth));
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(-singleLineHeight - 2F);
		m_LogColorPos = EditorGUILayout.Vector2IntField("像素坐标", m_LogMousePosColor ? mousePos : m_LogColorPos);
		Color32 color = Operation.GetColorOnScreen(m_LogColorPos.x, m_LogColorPos.y);
		string colorStr = color.ToString();
		colorStr = colorStr.Substring(5, colorStr.Length - 5 - 6);
		EditorGUI.BeginDisabledGroup(true);
		EditorGUILayout.ColorField($"颜色值({colorStr})", color);
		EditorGUI.EndDisabledGroup();
	}

	private void Update() {
		Repaint();
	}
}