/*
 * @Author: wangyun
 * @CreateTime: 2023-10-14 16:57:20 211
 * @LastEditor: wangyun
 * @EditTime: 2023-10-14 16:57:20 214
 */

using System;
using System.IO;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

public class PrefsEditorWindow<T> : EditorWindow {
	protected bool m_Debug;

	protected virtual void OnEnable() {
		DoLoadOptions();
		IsRunning = Prefs.Get<bool>($"{typeof(T).Name}Window.IsRunning");
	}
	protected virtual void OnDisable() {
		DoSaveOptions();
		Prefs.Set($"{typeof(T).Name}Window.IsRunning", IsRunning);
	}
	
	protected virtual void OnDestroy() {
		Debug.LogError("OnDestroy");
		IsRunning = false;
	}

	protected bool m_LoadComplexDataFromBackup = false;
	private void DoLoadOptions() {
		LoadOptions();
		if (m_LoadComplexDataFromBackup) {
			RecoverComplexData();
		}
	}
	protected bool m_AutoBackupComplexData = true;
	private void DoSaveOptions() {
		SaveOptions();
		if (m_AutoBackupComplexData) {
			BackupComplexData();
		}
	}

	protected string StartMenuName => $"Tools_Task/Start{typeof(T).Name}";
	protected string StopMenuName => $"Tools_Task/Stop{typeof(T).Name}";
	protected bool m_IsRunning;
	protected virtual bool IsRunning {
		get => m_IsRunning;
		set {
			m_IsRunning = value;
			EditorApplication.ExecuteMenuItem(value ? StartMenuName : StopMenuName);
		}
	}

	protected virtual void OnMenu(GenericMenu menu) {
		menu.AddItem(new GUIContent("调试模式"), m_Debug, () => m_Debug = !m_Debug);
		menu.AddItem(new GUIContent("重置选项"), false, DoLoadOptions);
		menu.AddItem(new GUIContent("保存选项"), false, DoSaveOptions);
		menu.AddItem(new GUIContent("备份复杂数据"), false, BackupComplexData);
		menu.AddItem(new GUIContent("恢复复杂数据"), false, RecoverComplexData);
		menu.AddItem(new GUIContent("自动备份复杂数据"), m_AutoBackupComplexData, () => m_AutoBackupComplexData = !m_AutoBackupComplexData);
		menu.AddItem(new GUIContent("自动恢复复杂数据"), m_LoadComplexDataFromBackup, () => m_LoadComplexDataFromBackup = !m_LoadComplexDataFromBackup);
	}

	private void ShowButton(Rect rect) {
		if (GUI.Button(rect, new GUIContent("", "Show menu"), EditorStyles.toolbarDropDown)) {
			GenericMenu menu = new GenericMenu();
			OnMenu(menu);
			menu.ShowAsContext();
		}
	}

	private static void BackupComplexData() {
		// 复杂数据以文件形式再备份一份
		Type type = typeof(T);
		FieldInfo[] fis = type.GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fi in fis) {
			if (!fi.FieldType.IsValueType) {
				string key = $"{type.Name}.{fi.Name}";
				string filePath = $"{Application.dataPath}/PersistentData/{key}.txt";
				FileInfo file = new FileInfo(filePath);
				if (file.Exists) {
					file.IsReadOnly = false;
					file.Delete();
				} else {
					DirectoryInfo directory = file.Directory;
					if (directory is {Exists: false}) {
						directory.Create();
					}
				}
				using (FileStream fs = file.OpenWrite()) {
					object value = fi.GetValue(null);
					string valueStr = JsonConvert.SerializeObject(value);
					byte[] bytes = Encoding.UTF8.GetBytes(valueStr);
					fs.Write(bytes, 0, bytes.Length); 
					fs.Flush();
				}
				file.IsReadOnly = true;
			}
		}
	}

	private static void RecoverComplexData() {
		// 复杂数据丢失时从文件恢复
		Type type = typeof(T);
		FieldInfo[] fis = type.GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fi in fis) {
			if (!fi.FieldType.IsValueType) {
				string key = $"{type.Name}.{fi.Name}";
				string filePath = $"{Application.dataPath}/PersistentData/{key}.txt";
				if (File.Exists(filePath)) {
					string valueStr = File.ReadAllText(filePath);
					try {
						object value = JsonConvert.DeserializeObject(valueStr, fi.FieldType);
						fi.SetValue(null, value);
					} catch (Exception e) {
						Debug.LogError($"恢复文件失败：{e}");
					}
				}
			}
		}
	}

	private static void LoadOptions() {
		Type type = typeof(T);
		FieldInfo[] fis = type.GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fi in fis) {
			if (!fi.IsLiteral) {
				string key = $"{type.Name}.{fi.Name}";
				object defaultValue = fi.GetValue(null);
				if (fi.FieldType == typeof(bool)) {
					fi.SetValue(null, Prefs.Get(key, (bool) defaultValue));
				} else if (fi.FieldType == typeof(int)) {
					fi.SetValue(null, Prefs.Get(key, (int) defaultValue));
				} else if (fi.FieldType == typeof(float)) {
					fi.SetValue(null, Prefs.Get(key, (float) defaultValue));
				} else if (fi.FieldType == typeof(string)) {
					fi.SetValue(null, Prefs.Get(key, (string) defaultValue));
				} else {
					string valueStr = Prefs.Get<string>(key);
					if (!string.IsNullOrEmpty(valueStr)) {
						object value = JsonConvert.DeserializeObject(valueStr, fi.FieldType);
						fi.SetValue(null, value);
					}
				}
			}
		}
	}

	private static void SaveOptions() {
		Type type = typeof(T);
		FieldInfo[] fis = type.GetFields(BindingFlags.Static | BindingFlags.Public);
		foreach (FieldInfo fi in fis) {
			if (!fi.IsLiteral) {
				string key = $"{type.Name}.{fi.Name}";
				object value = fi.GetValue(null);
				if (fi.FieldType == typeof(bool)) {
					Prefs.Set(key, (bool) value);
				} else if (fi.FieldType == typeof(int)) {
					Prefs.Set(key, (int) value);
				} else if (fi.FieldType == typeof(float)) {
					Prefs.Set(key, (float) value);
				} else if (fi.FieldType == typeof(string)) {
					Prefs.Set(key, (string) value);
				} else {
					string valueStr = JsonConvert.SerializeObject(value);
					Prefs.Set(key, valueStr);
				}
			}
		}
	}
}