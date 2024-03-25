using System;
using System.Reflection;
using UnityEngine;

public static class Utils {
	public static string GetEnumInspectorName(object enumValue) {
		Type enumType = enumValue.GetType();
		string enumName = Enum.GetName(enumType, enumValue);
		FieldInfo field = enumType.GetField(enumName);
		object[] attrs = field.GetCustomAttributes(false);
		foreach (var attr in attrs) {
			if (attr is UnityEngine.InspectorNameAttribute inspectorName) {
				enumName = inspectorName.displayName;
				break;
			}
		}
		return enumName;
	}
	
	[UnityEditor.MenuItem("Tools_Utils/Recompile", priority = -1)]
	public static void Recompile() {
		UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
	}
	
	public static string RectToString(Rect rect) {
		return $"{rect.x},{rect.y},{rect.width},{rect.height}";
	}
	
	public static Rect StringToRect(string str) {
		Rect rect = new Rect();
		string[] nums = str?.Split(',');
		switch (nums?.Length) {
			case 2:
				rect.width = float.Parse(nums[0].Trim());
				rect.height = float.Parse(nums[1].Trim());
				break;
			case 4:
				rect.x = float.Parse(nums[0].Trim());
				rect.y = float.Parse(nums[1].Trim());
				rect.width = float.Parse(nums[2].Trim());
				rect.height = float.Parse(nums[3].Trim());
				break;
		}
		return rect;
	}
}
