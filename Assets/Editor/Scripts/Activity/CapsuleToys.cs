/*
 * @Author: wangyun
 * @CreateTime: 2024-06-01 15:12:53 886
 * @LastEditor: wangyun
 * @EditTime: 2024-06-01 15:12:53 892
 */

using System.Collections;
using UnityEditor;
using UnityEngine;

public class CapsuleToys {
	public static KeyboardUtils.VKCode KEY_START = KeyboardUtils.VKCode.F1;
	public static KeyboardUtils.VKCode KEY_STOP = KeyboardUtils.VKCode.F2;
	public static  int LUCK_LEVEL;
	public static readonly bool[][] GET_OF_STAR_PLANS = {
		new[] { true,	true,	true,	true,	true,	true,	true },	// 自定义，可修改
		new[] { true,	true,	true,	true,	true,	false,	true },	// 非酋
		new[] { true,	false,	false,	false,	false,	false,	true },	// 黑人
		new[] { true,	false,	false,	false,	false,	true,	true },	// 普通
		new[] { false,	false,	false,	false,	false,	true,	true },	// 白人
		new[] { false,	false,	false,	false,	true,	true,	true },	// 欧皇
	};
	
	public static bool IsRunning;

	[MenuItem("Tools_Task/StartCapsuleToys", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"扭蛋快捷键已开启");
		IsRunning = true;
		KeyboardUtils.OnKeyUp += OnKeyUp;
		KeyboardUtils.Hook();
	}

	[MenuItem("Tools_Task/StopCapsuleToys", priority = -1)]
	private static void Disable() {
		if (IsRunning) {
			KeyboardUtils.Unhook();
			KeyboardUtils.OnKeyUp -= OnKeyUp;
			IsRunning = false;
			Debug.Log("扭蛋快捷键已关闭");
		}
	}

	private static void OnKeyUp(int vkCode) {
		KeyboardUtils.VKCode key = (KeyboardUtils.VKCode) vkCode;
		if (key == KEY_START) {
			StartCapsuleToys();
		} else if (key == KEY_STOP) {
			StopCapsuleToys();
		}
	}

	private static EditorCoroutine s_Co;
	private static void StopCapsuleToys() {
		if (s_Co != null) {
			Debug.Log($"终止自动扭蛋");
			EditorCoroutineManager.StopCoroutine(s_Co);
			if (Task.CurrentTask == nameof(CapsuleToys)) {
				Task.CurrentTask = null;
			}
		}
	}

	private static void StartCapsuleToys() {
		StopCapsuleToys();
		if (Task.CurrentTask == null) {
			Task.CurrentTask = nameof(CapsuleToys);
			Debug.Log($"开始自动扭蛋");
			s_Co = EditorCoroutineManager.StartCoroutine(IECapsuleToys());
		}
	}
	
	private static IEnumerator IECapsuleToys() {
		while (true) {
			Operation.Click(900, 970);	// 开始扭蛋
			yield return new EditorWaitForSeconds(2.5F);
			int star = Recognize.CurrentCapsuleToyStar;
			bool validStar = star is > 0 and <= 7;
			bool willGet = !validStar || GET_OF_STAR_PLANS[LUCK_LEVEL][star - 1];
			if (willGet) {
				Operation.Click(990, 860);	// 领取奖励
				yield return new EditorWaitForSeconds(0.3F);
				Operation.Click(1010, 730);	// 领取奖励
				yield return new EditorWaitForSeconds(0.5F);
				Operation.Click(960, 970);	// 点击继续
				yield return new EditorWaitForSeconds(0.2F);
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
