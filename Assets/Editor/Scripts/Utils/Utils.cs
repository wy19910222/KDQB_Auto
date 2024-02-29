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
			if (attr is InspectorNameAttribute inspectorName) {
				enumName = inspectorName.displayName;
				break;
			}
		}
		return enumName;
	}
}
