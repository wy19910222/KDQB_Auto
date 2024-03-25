/*
 * @Author: wangyun
 * @CreateTime: 2023-11-26 06:37:03 858
 * @LastEditor: wangyun
 * @EditTime: 2023-11-26 06:37:03 863
 */

using UnityEngine;
using UnityEditor;

public class ExtensionalScreenConfig : PrefsEditorWindow<ExtensionalScreen> {
	[MenuItem("Tools_Window/Default/ExtensionalScreen", false, -101)]
	private static void Open() {
		GetWindow<ExtensionalScreenConfig>("扩展屏幕").Show();
	}

	private bool m_HideOptions;
	private Texture2D m_CursorTex;
	// private Vector2 m_ScrollPos;
	
	private const float LENS_BORDER = 2;
	
	private float m_HalfWidth = 60;
	private float m_HalfHeight = 40;
	private float m_LensScale = 3;

	protected override void OnEnable() {
		base.OnEnable();
		m_CursorTex = new Texture2D(20, 20);
		int width = m_CursorTex.width;
		int height = m_CursorTex.height;
		Color[] colors = new Color[width * height];
		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				int revertY = height - 1 - y;
				if (revertY < x * 2 && revertY > x * 0.5F) {
					if (y == 0 || x == width - 1) {
						colors[y * width + x] = Color.black;
					} else {
						colors[y * width + x] = Color.white;
					}
				} else if (revertY < x * 2 + 2 && revertY > x * 0.5F - 1) {
					colors[y * width + x] = Color.black;
				}
			}
		}
		m_CursorTex.SetPixels(colors);
		m_CursorTex.Apply();
	}

	protected override void OnMenu(GenericMenu menu) {
		base.OnMenu(menu);
		menu.AddItem(new GUIContent("隐藏选项"), m_HideOptions, () => m_HideOptions = !m_HideOptions);
	}
	
	private void OnGUI() {
		if (!m_HideOptions) {
			Rect rect = EditorGUILayout.BeginHorizontal();
				float labelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 40;
				ExtensionalScreen.STRIDE = EditorGUILayout.IntSlider("DPR", ExtensionalScreen.STRIDE, 1, 4);
				EditorGUIUtility.labelWidth = labelWidth;
				
				Rect rect2 = GUILayoutUtility.GetRect(10F, rect.height, GUILayout.Width(10F));
				Rect wireRect2 = new Rect(rect2.x + 4.5F, rect2.y, 1, rect.height + 6);
				EditorGUI.DrawRect(wireRect2, Color.gray);
				
				EditorGUILayout.LabelField(new GUIContent("截图范围"), GUILayout.Width(60));
				EditorGUIUtility.labelWidth = 14;
				ExtensionalScreen.RANGE_X = EditorGUILayout.IntField("X", ExtensionalScreen.RANGE_X);
				ExtensionalScreen.RANGE_Y = EditorGUILayout.IntField("Y", ExtensionalScreen.RANGE_Y);
				ExtensionalScreen.RANGE_W = EditorGUILayout.IntField("W", ExtensionalScreen.RANGE_W);
				ExtensionalScreen.RANGE_H = EditorGUILayout.IntField("H", ExtensionalScreen.RANGE_H);
				
				Rect rect3 = GUILayoutUtility.GetRect(10F, rect.height, GUILayout.Width(10F));
				Rect wireRect3 = new Rect(rect3.x + 4.5F, rect3.y, 1, rect.height + 6);
				EditorGUI.DrawRect(wireRect3, Color.gray);
				
				EditorGUILayout.LabelField(new GUIContent("放大镜", "开启时按住Ctrl，在光标位置显示放大镜"), GUILayout.Width(50));
				EditorGUIUtility.labelWidth = 14;
				m_HalfWidth = EditorGUILayout.IntField("W", Mathf.RoundToInt(m_HalfWidth * 2)) * 0.5F;
				m_HalfHeight = EditorGUILayout.IntField("H", Mathf.RoundToInt(m_HalfHeight * 2)) * 0.5F;
				m_LensScale = EditorGUILayout.FloatField("S", m_LensScale);
				
				EditorGUIUtility.labelWidth = labelWidth;
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5F);
			EditorGUILayout.BeginHorizontal();
			if (ExtensionalScreen.IsRunning) {
				if (GUILayout.Button("关闭")) {
					IsRunning = false;
				}
			} else {
				if (GUILayout.Button("开启")) {
					IsRunning = true;
				}
			}
			if (GUILayout.Button("设置游戏范围")) {
				Operation.CURRENT_GAME_RECT = new Rect(
						ExtensionalScreen.RANGE_X,
						ExtensionalScreen.RANGE_Y,
						ExtensionalScreen.RANGE_W,
						ExtensionalScreen.RANGE_H
				);
			}
			if (GUILayout.Button("重置游戏范围")) {
				Operation.CURRENT_GAME_RECT = Operation.BASED_GAME_RECT;
			}
			EditorGUILayout.EndHorizontal();
		}
		// m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
		{
			// 绘制范围
			Rect rect = EditorGUILayout.BeginVertical();
			rect.height = ExtensionalScreen.Tex.height * rect.width / ExtensionalScreen.Tex.width;
			// 光标位置
			Vector2Int mousePos = MouseUtils.GetMousePos();
			float x = rect.x + rect.width * (mousePos.x - ExtensionalScreen.RANGE_X) / ExtensionalScreen.RANGE_W;
			float y = rect.y + rect.height * (mousePos.y - ExtensionalScreen.RANGE_Y) / ExtensionalScreen.RANGE_H;
			// 绘制画面
			if (ExtensionalScreen.Tex && ExtensionalScreen.IsRunning) {
				GUI.DrawTexture(rect, ExtensionalScreen.Tex);
				if (Event.current.control) {
					// 绘制放大镜
					Rect rectLens = new Rect(x - m_HalfWidth, y - m_HalfHeight, m_HalfWidth + m_HalfWidth, m_HalfHeight + m_HalfHeight);
					Rect rectLensWithBorder = new Rect(rectLens.x - LENS_BORDER, rectLens.y - LENS_BORDER, rectLens.width + LENS_BORDER + LENS_BORDER, rectLens.height + LENS_BORDER + LENS_BORDER);
					EditorGUI.DrawRect(rectLensWithBorder, Color.gray);
					GUI.BeginClip(rectLens);
					Rect rectInClip = new Rect(m_HalfWidth - (x - rect.x) * m_LensScale, m_HalfHeight - (y - rect.y) * m_LensScale, rect.width * m_LensScale, rect.height * m_LensScale);
					GUI.DrawTexture(rectInClip, ExtensionalScreen.Tex);
					GUI.EndClip();
				}
			} else {
				EditorGUI.DrawRect(rect, Color.gray);
			}
			
			Color prevColor = GUI.contentColor;
			GUI.contentColor = Color.black;
			GUI.DrawTexture(new Rect(x, y, m_CursorTex.width, m_CursorTex.height), m_CursorTex);
			GUI.contentColor = prevColor;
			EditorGUILayout.EndVertical();
			if (ExtensionalScreen.Tex && ExtensionalScreen.IsRunning) {
				
			}
		}
		// EditorGUILayout.EndScrollView();
	}
	
	private void Update() {
		Repaint();
	}
}