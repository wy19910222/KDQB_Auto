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
	
	public static string RectToString(RectInt rect) {
		return $"{rect.x},{rect.y},{rect.width},{rect.height}";
	}
	
	public static RectInt StringToRect(string str) {
		RectInt rect = new RectInt();
		string[] nums = str?.Split(',');
		switch (nums?.Length) {
			case 2:
				rect.width = int.Parse(nums[0].Trim());
				rect.height = int.Parse(nums[1].Trim());
				break;
			case 4:
				rect.x = int.Parse(nums[0].Trim());
				rect.y = int.Parse(nums[1].Trim());
				rect.width = int.Parse(nums[2].Trim());
				rect.height = int.Parse(nums[3].Trim());
				break;
		}
		return rect;
	}
}
