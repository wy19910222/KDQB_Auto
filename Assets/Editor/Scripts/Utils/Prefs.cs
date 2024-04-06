using UnityEditor;
using UnityEngine;

public static class Prefs {
	private static string ProjectName {
		get {
			string dataPath = Application.dataPath;
			dataPath = dataPath[..dataPath.LastIndexOfAny(new []{'/', '\\'})];
			dataPath = dataPath[(dataPath.LastIndexOfAny(new []{'/', '\\'}) + 1)..];
			return dataPath;
		}
	}
	
	public static void Set<T>(string key, T value) {
		switch (value) {
			case bool bValue:
				EditorPrefs.SetBool($"{ProjectName}.{key}", bValue);
				break;
			case int iValue:
				EditorPrefs.SetInt($"{ProjectName}.{key}", iValue);
				break;
			case float fValue:
				EditorPrefs.SetFloat($"{ProjectName}.{key}", fValue);
				break;
			case string sValue:
				EditorPrefs.SetString($"{ProjectName}.{key}", sValue);
				break;
		}
	}
	
	public static T Get<T>(string key, T defaultValue = default) {
		if (typeof(T) == typeof(bool)) {
			return (T) (object) EditorPrefs.GetBool($"{ProjectName}.{key}", (bool) (object) defaultValue);
		} else if (typeof(T) == typeof(int)) {
			return (T) (object) EditorPrefs.GetInt($"{ProjectName}.{key}", (int) (object) defaultValue);
		} else if (typeof(T) == typeof(float)) {
			return (T) (object) EditorPrefs.GetFloat($"{ProjectName}.{key}", (float) (object) defaultValue);
		} else if (typeof(T) == typeof(string)) {
			return (T) (object) EditorPrefs.GetString($"{ProjectName}.{key}", (string) (object) defaultValue);
		}
		return defaultValue;
	}
}
