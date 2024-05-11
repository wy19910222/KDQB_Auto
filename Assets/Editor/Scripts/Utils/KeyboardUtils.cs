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
	public enum VKCode {
		Q = 81,
		W = 87,
		E = 69,
		R = 82,
		F1 = 112,
		F2 = 113,
		F3 = 114,
		F4 = 115,
		F5,
		F6,
		F7,
		F8,
		F9,
		F10,
		F11,
		F12,
	}
	
	
	public static Action<int> OnKeyUp { get; set; }
	public static Action<int> OnKeyDown { get; set; }

	private static int m_HookID;
	private static int HookID {
		get {
			if (m_HookID == 0) {
				m_HookID = Prefs.Get<int>("KeyboardUtils.HookID");
			}
			return m_HookID;
		}
		set {
			if (m_HookID != value) {
				m_HookID = value;
				Prefs.Set("KeyboardUtils.HookID", m_HookID);
			}
		}
	}

	public static bool IsRunning => m_HookID != 0;

	public static System.Threading.Thread thread { get; set; }
	
	[MenuItem("Assets/Hook", priority = -1)]
	public static void Hook() {
		Unhook();
		HookID = SetHook(HookCallback);
		// thread = new System.Threading.Thread(() => {
		// 	int hookID = SetHook(HookCallback);
		// 	EditorApplication.delayCall += () => {
		// 		HookID = hookID;
		// 		Debug.Log($"HookID:{HookID}");
		// 	};
		// });
		// UnityEditor.Compilation.CompilationPipeline.compilationStarted += StopThread;
		// thread.Start();
		Debug.Log("开始按键监听");
	}
	
	[MenuItem("Assets/Unhook", priority = -1)]
	public static void Unhook() {
		if (HookID != 0) {
			UnhookWindowsHookEx(HookID);
			Debug.Log("结束按键监听");
			HookID = 0;
			// StopThread();
		}
	}
	
	// private static void StopThread(object obj = null) {
	// 	Debug.Log("StopThread");
	// 	if (thread != null) {
	// 		thread.Abort();
	// 		thread = null;
	// 	}
	// }
	
	private const int WH_KEYBOARD_LL = 13;
	private const int WM_KEYDOWN = 0x0100;
	private const int WM_KEYUP = 0x0101;

	private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
	
	// [DllImport("kernel32.dll")]
	// private static extern uint GetCurrentThreadId();

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern int SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnhookWindowsHookEx(int hookID);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr CallNextHookEx(int hookID, int nCode, IntPtr wParam, IntPtr lParam);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	private static int SetHook(LowLevelKeyboardProc proc, uint dwThreadId = 0) {
		using Process curProcess = Process.GetCurrentProcess();
		using ProcessModule curModule = curProcess.MainModule;
		EditorApplication.delayCall += () => Debug.Log("SetHook " + dwThreadId);
		return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule?.ModuleName), dwThreadId);
	}

	private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam) {
		if (nCode >= 0 && (wParam == (IntPtr) WM_KEYDOWN || wParam == (IntPtr) WM_KEYUP)) {
			int vkCode = Marshal.ReadInt32(lParam);
			EditorApplication.delayCall += () => Debug.Log($"按键：0x{vkCode}");
			if (wParam == (IntPtr) WM_KEYDOWN) {
				OnKeyUp?.Invoke(vkCode);
			} else if (wParam == (IntPtr) WM_KEYUP) {
				OnKeyUp?.Invoke(vkCode);
			}
		}
		return CallNextHookEx(m_HookID, nCode, wParam, lParam);
	}
}
