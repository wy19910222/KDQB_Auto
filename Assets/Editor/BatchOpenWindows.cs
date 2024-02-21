/*
 * @Author: wangyun
 * @CreateTime: 2023-11-05 19:03:52 124
 * @LastEditor: wangyun
 * @EditTime: 2023-11-05 19:03:52 128
 */

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BatchOpenWindowsConfig : PrefsEditorWindow<BatchOpenWindows> {
	[MenuItem("Tools_Window/BatchOpenWindows", false, 22)]
	private static void Open() {
		GetWindow<BatchOpenWindowsConfig>("批量打开窗口").Show();
	}

	[SerializeField]
	private string m_TempType = string.Empty;
	[SerializeField]
	private List<string> m_AddedTypes = new List<string>();
	[SerializeField]
	private List<string> m_TempWindowNames = new List<string>();
	
	private void OnGUI() {
		if (GUILayout.Button("全部打开")) {
			BatchOpenWindows.OpenWindows();
		}
		foreach (var (_type, _windowNames) in BatchOpenWindows.TYPE_WINDOWS_DICT) {
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("×", GUILayout.Width(30F))) {
				EditorApplication.delayCall += () => {
					BatchOpenWindows.TYPE_WINDOWS_DICT.Remove(_type);
					for (int i = m_AddedTypes.Count - 1; i >= 0; --i) {
						if (m_AddedTypes[i] == _type) {
							m_AddedTypes.RemoveAt(i);
							m_TempWindowNames.RemoveAt(i);
							Repaint();
						}
					}
				};
			}
			EditorGUILayout.LabelField(_type);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("批量打开", GUILayout.Width(120F))) {
				BatchOpenWindows.OpenWindows(_type);
			}
			EditorGUILayout.EndHorizontal();

			for (int i = 0, length = _windowNames.Count; i < length; ++i) {
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(30F + 2F);
				if (GUILayout.Button("×", GUILayout.Width(30F))) {
					_windowNames.RemoveAt(i);
					--length;
				} else {
					EditorGUILayout.LabelField(_windowNames[i]);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("单独打开", GUILayout.Width(80F))) {
						BatchOpenWindows.OpenWindows(_type, _windowNames[i]);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			
			for (int i = 0, length = m_AddedTypes.Count; i < length; ++i) {
				string type = m_AddedTypes[i];
				if (type == _type) {
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(30F + 30F + 4F);
					string newTempWindowName = EditorGUILayout.TextField(m_TempWindowNames[i]);
					if (newTempWindowName != m_TempWindowNames[i]) {
						Undo.RecordObject(this, "TempWindowNameChange");
						m_TempWindowNames[i] = newTempWindowName;
					}
					if (GUILayout.Button("+", GUILayout.Width(30F))) {
						_windowNames.Add(newTempWindowName);
					}
					EditorGUILayout.EndHorizontal();
				}
			}
		}
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(30F + 4F);
		string newTempType = EditorGUILayout.TextField(m_TempType);
		if (newTempType != m_TempType) {
			Undo.RecordObject(this, "TempTypeChange");
			m_TempType = newTempType;
		}
		if (GUILayout.Button("+", GUILayout.Width(30F))) {
			if (!BatchOpenWindows.TYPE_WINDOWS_DICT.ContainsKey(m_TempType)) {
				BatchOpenWindows.TYPE_WINDOWS_DICT.Add(m_TempType, new List<string>());
				m_AddedTypes.Add(newTempType);
				m_TempWindowNames.Add(string.Empty);
			}
		}
		EditorGUILayout.EndHorizontal();
	}

	protected override bool IsRunning {
		get => m_IsRunning;
		set => m_IsRunning = value;
	}
}

public class BatchOpenWindows {
	public static readonly Dictionary<string, List<string>> TYPE_WINDOWS_DICT = new Dictionary<string, List<string>>();
	
	public static void OpenWindows(string type = null, string windowName = null) {
		foreach (var (_type, _windowNames) in TYPE_WINDOWS_DICT) {
			if (type == null || _type == type) {
				foreach (var _windowName in _windowNames) {
					if (windowName == null || _windowName == windowName) {
						EditorApplication.ExecuteMenuItem($"Tools_Window/{_type}/{_windowName}");
					}
				}
			}
		}
	}
}