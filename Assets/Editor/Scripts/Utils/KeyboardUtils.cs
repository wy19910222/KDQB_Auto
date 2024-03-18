/*
 * @Author: wangyun
 * @CreateTime: 2023-09-08 00:55:27 028
 * @LastEditor: wangyun
 * @EditTime: 2023-09-08 00:55:27 032
 */

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEditor;

using Debug = UnityEngine.Debug;

public static class KeyboardUtils {
	public static Action<int> OnKeyUp { get; set; }

	[MenuItem("Assets/Hook", priority = -1)]
	public static void Hook() {
		Unhook();
		Prefs.Set("KeyboardUtils.HookID", SetHook(HookCallback));
		Debug.LogError("开始按键监听");
	}
	
	[MenuItem("Assets/Unhook", priority = -1)]
	public static void Unhook() {
		int hookID = Prefs.Get<int>("KeyboardUtils.HookID");
		if (hookID != 0) {
			UnhookWindowsHookEx(hookID);
			Debug.LogError("结束按键监听");
		}
	}
	
	private const int WH_KEYBOARD_LL = 13;
	private const int WM_KEYDOWN = 0x0100;
	private const int WM_KEYUP = 0x0101;

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(int hhk);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(int hookID, int nCode, IntPtr wParam, IntPtr lParam);

	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

	private static int SetHook(LowLevelKeyboardProc proc) {
		using Process curProcess = Process.GetCurrentProcess();
		using ProcessModule curModule = curProcess.MainModule;
		return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule?.ModuleName), 0);
	}

	private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
		if (nCode >= 0 && (wParam == (IntPtr) WM_KEYDOWN || wParam == (IntPtr) WM_KEYUP)) {
			int vkCode = Marshal.ReadInt32(lParam);
			Debug.LogError($"按键：0x{vkCode}");
			if (wParam == (IntPtr) WM_KEYUP) {
				OnKeyUp?.Invoke(vkCode);
			}
		}
		return CallNextHookEx(Prefs.Get<int>("KeyboardUtils.HookID"), nCode, wParam, lParam);
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);
}
