using System;
using System.Reflection;

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
}
