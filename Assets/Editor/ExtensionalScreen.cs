/*
 * @Author: wangyun
 * @CreateTime: 2023-11-26 06:37:03 858
 * @LastEditor: wangyun
 * @EditTime: 2023-11-26 06:37:03 863
 */

using System.Collections;
using UnityEngine;
using UnityEditor;
using Debug = UnityEngine.Debug;

public class ExtensionalScreenConfig : PrefsEditorWindow<ExtensionalScreen> {
	[MenuItem("Window/Default/ExtensionalScreen", false, -101)]
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
				EditorGUILayout.BeginVertical();
					ExtensionalScreen.INTERVAL = EditorGUILayout.Slider("截图间隔（秒）", ExtensionalScreen.INTERVAL, 0, 0.5F);
					ExtensionalScreen.STRIDE = EditorGUILayout.IntSlider("流畅度", ExtensionalScreen.STRIDE, 1, 4);
				EditorGUILayout.EndVertical();
				GUILayout.Space(50);
				float labelWidth = EditorGUIUtility.labelWidth;
				EditorGUILayout.BeginVertical();
					EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField(new GUIContent("截图范围"), GUILayout.Width(labelWidth));
						EditorGUIUtility.labelWidth = 14;
						ExtensionalScreen.RANGE_X = EditorGUILayout.IntField("X", ExtensionalScreen.RANGE_X);
						ExtensionalScreen.RANGE_Y = EditorGUILayout.IntField("Y", ExtensionalScreen.RANGE_Y);
						EditorGUIUtility.labelWidth = labelWidth;
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField("", GUILayout.Width(labelWidth));
						EditorGUIUtility.labelWidth = 14;
						ExtensionalScreen.RANGE_W = EditorGUILayout.IntField("W", ExtensionalScreen.RANGE_W);
						ExtensionalScreen.RANGE_H = EditorGUILayout.IntField("H", ExtensionalScreen.RANGE_H);
						EditorGUIUtility.labelWidth = labelWidth;
					EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
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
		if (ExtensionalScreen.Tex) {
			// m_ScrollPos = EditorGUILayout.BeginScrollView(m_ScrollPos);
			Rect rect = EditorGUILayout.BeginVertical();
			rect.height = ExtensionalScreen.Tex.height * rect.width / ExtensionalScreen.Tex.width;
			GUI.DrawTexture(rect, ExtensionalScreen.Tex);
			Vector2Int mousePos = MouseUtils.GetMousePos();
			float x = rect.x + rect.width * (mousePos.x - ExtensionalScreen.RANGE_X) / ExtensionalScreen.RANGE_W;
			float y = rect.y + rect.height * (mousePos.y - ExtensionalScreen.RANGE_Y) / ExtensionalScreen.RANGE_H;
			Color prevColor = GUI.contentColor;
			GUI.contentColor = Color.black;
			GUI.DrawTexture(new Rect(x, y, m_CursorTex.width, m_CursorTex.height), m_CursorTex);
			GUI.contentColor = prevColor;
			EditorGUILayout.EndVertical();
			// EditorGUILayout.EndScrollView();
		}
	}

	private void OnBecameVisible() {
		EditorApplication.update += Repaint;
	}

	private void OnBecameInvisible() {
		EditorApplication.update -= Repaint;
	}
}

public class ExtensionalScreen {
	public static float INTERVAL = 0.1F;	// 截图间隔
	public static int RANGE_X = 0;	// 截图范围
	public static int RANGE_Y = 0;	// 截图范围
	public static int RANGE_W = 1920;	// 截图范围
	public static int RANGE_H = 1080;	// 截图范围
	public static int STRIDE = 2;	// 间隔多少取一个像素
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	public static Texture2D Tex { get; } = new Texture2D(960, 540);

	[MenuItem("Assets/StartExtensionalScreen", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"查看第三屏已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/StopExtensionalScreen", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("查看第三屏已关闭");
		}
	}

	private static IEnumerator Update() {
		Color32[] pixels = new Color32[Tex.width * Tex.height];
		while (true) {
			Color32[,] colors = ScreenshotUtils.GetColorsOnScreen(RANGE_X, RANGE_Y, RANGE_W, RANGE_H, STRIDE);
			int realWidth = RANGE_W / STRIDE;
			int realHeight = RANGE_H / STRIDE;
			if (!Tex || Tex.width != realWidth || Tex.height != realHeight) {
				Tex.Reinitialize(realWidth, realHeight);
				pixels = new Color32[realWidth * realHeight];
			}
			for (int y = 0, offset0 = (realHeight - 1) * realWidth; y < realHeight; ++y, offset0 -= realWidth) {
				for (int x = 0, offset = offset0; x < realWidth; ++x, ++offset) {
					pixels[offset] = colors[x, y];
				}
			}
			Tex.SetPixels32(pixels);
			Tex.Apply();
			yield return new EditorWaitForSeconds(INTERVAL);
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
