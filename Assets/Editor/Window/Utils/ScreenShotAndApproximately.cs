/*
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

	private const float PREVIEW_HEIGHT = 200F;

	[SerializeField]
	private RectInt m_CaptureRect = new RectInt();
	[SerializeField]
	private string m_CaptureFilename = string.Empty;
	[SerializeField]
	private bool m_IsCapturePreview;
	[SerializeField]
	private Texture2D m_CapturePreviewTex;
	private Color32[] m_CapturePreviewColors;

	[SerializeField]
	private RectInt m_ApproximatelyRect = new RectInt();
	[SerializeField]
	private string m_ApproximatelyFilename = string.Empty;
	[SerializeField]
	private float m_ApproximatelyThresholdMulti = 1;
	[SerializeField]
	private bool m_IsApproximatelyPreview;
	[SerializeField]
	private Texture2D m_ApproximatelyPreviewTex;
	private Color32[] m_ApproximatelyPreviewColors;

	[SerializeField]
	private Vector2Int m_LogColorPos = new Vector2Int();
	[SerializeField]
	private bool m_LogMousePosColor;
	[SerializeField]
	private bool m_IsCrosshairShow;
	[SerializeField]
	private Texture2D m_MousePreviewTex;
	private Color32[] m_MousePreviewColors;

	private void Update() {
		Repaint();
	}

	private void OnGUI() {
		const float MAX_BTN_WIDTH = 70F;
		const float MIN_SPACE_WIDTH = 70F;
		float singleLineHeight = EditorGUIUtility.singleLineHeight;
		float labelWidth = EditorGUIUtility.labelWidth;
		float spaceWidth = Mathf.Max(labelWidth - MAX_BTN_WIDTH, MIN_SPACE_WIDTH);
		float btnWidth = labelWidth - spaceWidth;

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2F - 5F - 2F));
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(spaceWidth);
		if (GUILayout.Button("与对比相同", GUILayout.Width(btnWidth))) {
			m_CaptureRect = m_ApproximatelyRect;
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(-singleLineHeight - 2F);
		EditorGUI.BeginChangeCheck();
		RectInt newRect = EditorGUILayout.RectIntField("截取范围", m_CaptureRect);
		if (EditorGUI.EndChangeCheck()) {
			Undo.RecordObject(this, "Rect");
			m_CaptureRect = newRect;
		}
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(spaceWidth);
		if (GUILayout.Button("与对比相同", GUILayout.Width(btnWidth))) {
			m_CaptureFilename = m_ApproximatelyFilename;
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(-singleLineHeight - 2F);
		string newFilename = EditorGUILayout.TextField("保存文件名", m_CaptureFilename);
		if (newFilename != m_CaptureFilename) {
			Undo.RecordObject(this, "Filename");
			m_CaptureFilename = newFilename;
		}
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("截取")) {
			if (!string.IsNullOrEmpty(m_CaptureFilename)) {
				Operation.Screenshot(m_CaptureRect.x, m_CaptureRect.y, m_CaptureRect.width, m_CaptureRect.height, $"PersistentData/Textures/{m_CaptureFilename}.png");
			} else {
				Debug.LogError("Filename is empty!");
			}
		}
		bool newIsCapturePreview = GUILayout.Toggle(m_IsCapturePreview, "预览", "Button");
		if (newIsCapturePreview != m_IsCapturePreview) {
			Undo.RecordObject(this, "IsCapturePreview");
			m_IsCapturePreview = newIsCapturePreview;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		GUILayout.Space(10F);

		EditorGUILayout.BeginVertical(GUILayout.Width(position.width / 2F - 5F - 2F));
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(spaceWidth);
		if (GUILayout.Button("与截取相同", GUILayout.Width(btnWidth))) {
			m_ApproximatelyRect = m_CaptureRect;
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
			m_ApproximatelyFilename = m_CaptureFilename;
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.Space(-singleLineHeight - 2F);
		EditorGUILayout.BeginHorizontal();
		string newApproximatelyFilename = EditorGUILayout.TextField("对比文件名", m_ApproximatelyFilename);
		if (newApproximatelyFilename != m_ApproximatelyFilename) {
			Undo.RecordObject(this, "ApproximatelyFilename");
			m_ApproximatelyFilename = newApproximatelyFilename;
		}
		float prevLabelWidth = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = 50F;
		float newApproximatelyThresholdMulti = EditorGUILayout.FloatField("阈值缩放", m_ApproximatelyThresholdMulti, GUILayout.Width(100F));
		if (!Mathf.Approximately(newApproximatelyThresholdMulti, m_ApproximatelyThresholdMulti)) {
			Undo.RecordObject(this, "ApproximatelyThresholdMulti");
			m_ApproximatelyThresholdMulti = newApproximatelyThresholdMulti;
		}
		EditorGUIUtility.labelWidth = prevLabelWidth;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("对比")) {
			Color32[,] targetColors = Operation.GetFromFile($"PersistentData/Textures/{m_ApproximatelyFilename}.png");
			Color32[,] realColors = Operation.GetColorsOnScreen(m_ApproximatelyRect.x, m_ApproximatelyRect.y, m_ApproximatelyRect.width, m_ApproximatelyRect.height);
			Debug.Log(Recognize.ApproximatelyRect(realColors, targetColors, m_ApproximatelyThresholdMulti));
		}
		bool newIsApproximatelyPreview = GUILayout.Toggle(m_IsApproximatelyPreview, "预览", "Button");
		if (newIsApproximatelyPreview != m_IsApproximatelyPreview) {
			Undo.RecordObject(this, "IsApproximatelyPreview");
			m_IsApproximatelyPreview = newIsApproximatelyPreview;
		}
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();

		if (m_IsCapturePreview || m_IsApproximatelyPreview) {
			Rect rect = GUILayoutUtility.GetRect(position.width, PREVIEW_HEIGHT);
			if (m_IsCapturePreview) {
				ApplyPreviewTexture(m_CaptureRect, ref m_CapturePreviewTex, ref m_CapturePreviewColors);
				Rect capturePreviewRect = rect;
				capturePreviewRect.width = position.width / 2 - 5;
				GUI.DrawTexture(capturePreviewRect, m_CapturePreviewTex, ScaleMode.ScaleToFit);
			}
			if (m_IsApproximatelyPreview) {
				ApplyPreviewTexture(m_ApproximatelyRect, ref m_ApproximatelyPreviewTex, ref m_ApproximatelyPreviewColors);
				Rect approximatelyPreviewRect = rect;
				approximatelyPreviewRect.xMin += position.width / 2 + 5;
				GUI.DrawTexture(approximatelyPreviewRect, m_ApproximatelyPreviewTex, ScaleMode.ScaleToFit);
			}
		}

		{
			Rect rect = GUILayoutUtility.GetRect(0, 10);
			Rect wireRect = new Rect(rect.x, rect.y + 4.5F, rect.width, 1);
			EditorGUI.DrawRect(wireRect, Color.gray);
		}

		EditorGUILayout.BeginHorizontal();
		Rect tempRect = EditorGUILayout.BeginVertical();
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
		EditorGUI.BeginDisabledGroup(true);
		Color32 color = Operation.GetColorOnScreen(m_LogColorPos.x, m_LogColorPos.y);
		string colorStr = color.ToString();
		colorStr = colorStr.Substring(5, colorStr.Length - 5 - 6);
		EditorGUILayout.ColorField($"颜色值({colorStr})", color);
		Color32 grayColor = color.ToGray();
		EditorGUILayout.ColorField($"灰度值({grayColor.r})", grayColor);
		EditorGUI.EndDisabledGroup();
		bool newIsCrosshairShow = GUILayout.Toggle(m_IsCrosshairShow, "十字架", "Button");
		if (newIsCrosshairShow != m_IsCrosshairShow) {
			Undo.RecordObject(this, "IsCrosshairShow");
			m_IsCrosshairShow = newIsCrosshairShow;
		}
		EditorGUILayout.EndVertical();
		
		GUILayout.Space(position.width / 2 + 5);
		
		if (Event.current.type == EventType.Repaint) {
			int dpi = 10;
			Rect rect = tempRect;
			rect.x += rect.width + 12;
			rect.width = position.width - rect.x;
			int width = Mathf.FloorToInt((rect.width / dpi - 1) / 2) * 2 + 1;
			int height = Mathf.FloorToInt((rect.height / dpi - 1) / 2) * 2 + 1;
			RectInt captureRect = new RectInt(mousePos.x - width / 2, mousePos.y - height / 2, width, height);
			ApplyPreviewTexture(captureRect, ref m_MousePreviewTex, ref m_CapturePreviewColors);
			Vector2 center = rect.center;
			rect.size = new Vector2(dpi * width, dpi * height);
			rect.center = center;
			GUI.DrawTexture(rect, m_MousePreviewTex, ScaleMode.ScaleToFit);
			if (newIsCrosshairShow) {
				EditorGUI.DrawRect(new Rect(center.x - dpi / 2F, rect.y, 1, rect.height), Color.cyan * 0.5F);
				EditorGUI.DrawRect(new Rect(center.x + dpi / 2F, rect.y, -1, rect.height), Color.cyan * 0.5F);
				EditorGUI.DrawRect(new Rect(rect.x, center.y - dpi / 2F, rect.width, 1), Color.cyan * 0.5F);
				EditorGUI.DrawRect(new Rect(rect.x, center.y + dpi / 2F, rect.width, -1), Color.cyan * 0.5F);
				EditorGUI.DrawRect(new Rect(center.x - dpi / 2F, rect.y, -1, rect.height), Color.white * 0.5F);
				EditorGUI.DrawRect(new Rect(center.x + dpi / 2F, rect.y, 1, rect.height), Color.white * 0.5F);
				EditorGUI.DrawRect(new Rect(rect.x, center.y - dpi / 2F, rect.width, -1), Color.white * 0.5F);
				EditorGUI.DrawRect(new Rect(rect.x, center.y + dpi / 2F, rect.width, 1), Color.white * 0.5F);
			}
		}
		EditorGUILayout.EndHorizontal();
	}
	
	private static void ApplyPreviewTexture(RectInt rect, ref Texture2D tex, ref Color32[] colors) {
		Color32[,] _colors = Operation.GetColorsOnScreen(rect.x, rect.y, rect.width, rect.height);
		int width = _colors.GetLength(0);
		int height = _colors.GetLength(1);
		if (!tex) {
			tex = new Texture2D(width, height, TextureFormat.RGB24, false) {
				filterMode = FilterMode.Point
			};
		}
		if (colors == null) {
			colors = new Color32[width * height];
		}
		if (tex.width != width || tex.height != height) {
			tex.Reinitialize(width, height);
		}
		if (colors.Length != width * height) {
			colors = new Color32[width * height];
		}
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				colors[(height - 1 - y) * width + x] = _colors[x, y];
			}
		}
		tex.SetPixels32(colors);
		tex.Apply();
	}
}