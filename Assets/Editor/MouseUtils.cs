/*
 * @Author: wangyun
 * @CreateTime: 2023-09-08 00:51:05 046
 * @LastEditor: wangyun
 * @EditTime: 2023-09-08 00:51:05 050
 */

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

using Debug = UnityEngine.Debug;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public static class MouseUtils {
	[Flags]
	private enum MouseEventFlag : uint {
		Move = 0x0001,
		LeftDown = 0x0002,
		LeftUp = 0x0004,
		RightDown = 0x0008,
		RightUp = 0x0010,
		MiddleDown = 0x0020,
		MiddleUp = 0x0040,
		XDown = 0x0080,
		XUp = 0x0100,
		Wheel = 0x0800,
		VirtualDesk = 0x4000,
		Absolute = 0x8000
	}
	
	private const int VK_MOUSE_LEFT_BUTTON = 0x01;	// 鼠标左键的虚拟键码
	private const int VK_MOUSE_RIGHT_BUTTON = 0x02;	// 鼠标右键的虚拟键码
	private const int VK_CANCEL = 0x03;	// Control-break处理的虚拟键码
	private const int VK_MOUSE_MIDDLE_BUTTON = 0x04;	// 鼠标中键的虚拟键码
	private const int VK_MOUSE_EXT_BUTTON1 = 0x05;	// 第一个扩展鼠标按钮的虚拟键码
	private const int VK_MOUSE_EXT_BUTTON2 = 0x06;	// 第二个扩展鼠标按钮的虚拟键码
	
	[DllImport("user32.dll")]
	private static extern bool GetCursorPos(out Vector2Int lpPoint);
	[DllImport("user32.dll")]
	private static extern bool SetCursorPos(int X, int Y);
	[DllImport("user32.dll")]
	public static extern short GetAsyncKeyState(int vKey);
	[DllImport("user32.dll")]
	private static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo);
	[DllImport("user32.dll")]
	private static extern void mouse_event(MouseEventFlag flags, int dx, int dy, int dwData, UIntPtr extraInfo);

	private static EditorCoroutine s_CO;
	[MenuItem("Assets/EnableMousePositionLog", priority = -1)]
	private static void Enable() {
		Disable();
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Assets/DisableMousePositionLog", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return new WaitForSeconds(1);
			if (GetCursorPos(out Vector2Int pos)) {
				Debug.LogError(pos);
				Debug.LogError(ScreenshotUtils.GetColorOnScreen(pos.x, pos.y));
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
	
	public static Vector2Int GetMousePos() {
		return GetCursorPos(out Vector2Int pos) ? pos : default;
	}
	
	public static void SetMousePos(int x, int y) {
		SetCursorPos(x, y);
	}
	
	public static bool IsLeftDown() {
		short state = GetAsyncKeyState(VK_MOUSE_LEFT_BUTTON);
		return (state & 0x8000) != 0;	// 最高位为1表示按下
	}
	
	public static void LeftDown() {
		mouse_event(MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
	}
	
	public static void LeftUp() {
		mouse_event(MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
	}
	
	/// <summary>
	/// 正数向上滚，负数向下滚
	/// </summary>
	/// <param name="delta"></param>
	public static void ScrollWheel(int delta) {
		mouse_event(MouseEventFlag.Wheel, 0, 0, delta, UIntPtr.Zero);
	}
}
