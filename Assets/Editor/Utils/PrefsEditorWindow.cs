/*
 * @Author: wangyun
 * @CreateTime: 2023-10-14 16:57:20 211
 * @LastEditor: wangyun
 * @EditTime: 2023-10-14 16:57:20 214
 */

using System;
using System.Reflection;
using UnityEditor;

public class PrefsEditorWindow<T> : EditorWindow {
	protected virtual void OnEnable() {
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
			}
		}
	}

	protected virtual void OnDisable() {
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
			}
		}
	}
}