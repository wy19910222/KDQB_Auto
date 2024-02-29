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
	[SerializeField]
	private Rect m_GameRect = Operation.CURRENT_GAME_RECT;

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
		Operation.CURRENT_GAME_RECT = m_GameRect;
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
				m_GameRect.x = ExtensionalScreen.RANGE_X;
				m_GameRect.y = ExtensionalScreen.RANGE_Y;
				m_GameRect.width = ExtensionalScreen.RANGE_W;
				m_GameRect.height = ExtensionalScreen.RANGE_H;
				Operation.CURRENT_GAME_RECT = m_GameRect;
			}
			if (GUILayout.Button("重置游戏范围")) {
				m_GameRect = Operation.BASED_GAME_RECT;
				Operation.CURRENT_GAME_RECT = m_GameRect;
			}
			EditorGUILayout.EndHorizontal();
		}
		// m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
		{
			Rect rect = EditorGUILayout.BeginVertical();
			if (ExtensionalScreen.Tex && ExtensionalScreen.IsRunning) {
				rect.height = ExtensionalScreen.Tex.height * rect.width / ExtensionalScreen.Tex.width;
				GUI.DrawTexture(rect, ExtensionalScreen.Tex);
			} else {
				rect.height = ExtensionalScreen.Tex.height * rect.width / ExtensionalScreen.Tex.width;
				EditorGUI.DrawRect(rect, Color.gray);
			}
			Vector2Int mousePos = MouseUtils.GetMousePos();
			float x = rect.x + rect.width * (mousePos.x - ExtensionalScreen.RANGE_X) / ExtensionalScreen.RANGE_W;
			float y = rect.y + rect.height * (mousePos.y - ExtensionalScreen.RANGE_Y) / ExtensionalScreen.RANGE_H;
			Color prevColor = GUI.contentColor;
			GUI.contentColor = Color.black;
			GUI.DrawTexture(new Rect(x, y, m_CursorTex.width, m_CursorTex.height), m_CursorTex);
			GUI.contentColor = prevColor;
			EditorGUILayout.EndVertical();
		}
		// EditorGUILayout.EndScrollView();
	}
	
	private void Update() {
		Repaint();
	}
}