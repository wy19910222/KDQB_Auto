/*
 * @Author: wangyun
 * @CreateTime: 2023-10-14 16:57:20 211
 * @LastEditor: wangyun
 * @EditTime: 2023-10-14 16:57:20 214
 */

using System;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

public class PrefsEditorWindow<T> : EditorWindow {
	protected bool m_Debug;
	
	protected virtual void OnEnable() {
		LoadOptions();
	}

	protected virtual void OnDisable() {
		SaveOptions();
	}

	protected virtual void OnMenu(GenericMenu menu) {
		menu.AddItem(new GUIContent("调试模式"), m_Debug, () => m_Debug = !m_Debug);
		menu.AddItem(new GUIContent("重置选项"), false, LoadOptions);
		menu.AddItem(new GUIContent("保存选项"), false, SaveOptions);
	}

	private void ShowButton(Rect rect) {
		if (GUI.Button(rect, new GUIContent("", "Show menu"), EditorStyles.toolbarDropDown)) {
			GenericMenu menu = new GenericMenu();
			OnMenu(menu);
			menu.ShowAsContext();
		}
	}

	private static void LoadOptions() {
		Type type = typeof(T);
		FieldInfo[] fis = type.GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fi in fis) {
			string key = $"{type.Name}.{fi.Name}";
			object defaultValue = fi.GetValue(null);
			if (fi.FieldType == typeof(bool)) {
				fi.SetValue(null, EditorPrefs.GetBool(key, (bool) defaultValue));
			} else if (fi.FieldType == typeof(int)) {
				fi.SetValue(null, EditorPrefs.GetInt(key, (int) defaultValue));
			} else if (fi.FieldType == typeof(float)) {
				fi.SetValue(null, EditorPrefs.GetFloat(key, (float) defaultValue));
			} else if (fi.FieldType == typeof(string)) {
				fi.SetValue(null, EditorPrefs.GetString(key, (string) defaultValue));
			} else {
				string defaultValueStr = JsonConvert.SerializeObject(defaultValue);
				string valueStr = EditorPrefs.GetString(key, defaultValueStr);
				object value = JsonConvert.DeserializeObject(valueStr, fi.FieldType);
				fi.SetValue(null, value);
			}
		}
	}

	private static void SaveOptions() {
		Type type = typeof(T);
		FieldInfo[] fis = type.GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fi in fis) {
			string key = $"{type.Name}.{fi.Name}";
			object value = fi.GetValue(null);
			if (fi.FieldType == typeof(bool)) {
				EditorPrefs.SetBool(key, (bool) value);
			} else if (fi.FieldType == typeof(int)) {
				EditorPrefs.SetInt(key, (int) value);
			} else if (fi.FieldType == typeof(float)) {
				EditorPrefs.SetFloat(key, (float) value);
			} else if (fi.FieldType == typeof(string)) {
				EditorPrefs.SetString(key, (string) value);
			} else {
				string valueStr = JsonConvert.SerializeObject(value);
				EditorPrefs.SetString(key, valueStr);
			}
		}
	}
}