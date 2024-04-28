/*
 * @Author: wangyun
 * @CreateTime: 2024-04-05 01:43:34 064
 * @LastEditor: wangyun
 * @EditTime: 2024-04-05 01:43:34 068
 */

using System.Collections;
using UnityEngine;
using UnityEditor;

public class DailySupplies {
	public static int FAILED_COOLDOWN_MINUTE = 1;	// 失败后冷却时间
	
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartDailySupplies", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"每日补给自动领取已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopDailySupplies", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("每日补给自动领取已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;

			// 只有是世界界面近景或主城界面，才执行
			switch (Recognize.CurrentScene) {
				case Recognize.Scene.UNKNOWN:
				case Recognize.Scene.FIGHTING:
				case Recognize.Scene.OUTSIDE_FARAWAY:
					continue;
			}
			
			// 有窗口打开着
			if (Recognize.IsWindowCovered) {
				continue;
			}
			
			// 商城是否有红点
			if (!Recognize.MallIsNew) {
				continue;
			}

			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(DailySupplies);
			
			Debug.Log("商城按钮");
			Operation.Click(1820, 136);	// 商城按钮
			yield return new EditorWaitForSeconds(0.2F);

			bool succeed = false;
			if (Recognize.DailySuppliesIsNew) {
				succeed = true;
				Debug.Log("补给图标");
				Operation.Click(761, 392);	// 补给图标
				yield return new EditorWaitForSeconds(0.2F);
			}

			if (Recognize.DiscountPacksIsNew) {
				succeed = true;
				Debug.Log("特惠礼包标签页");
				Operation.Click(925, 195);	// 特惠礼包标签页
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("免费奖励");
				Operation.Click(1168, 296);	// 免费奖励
				yield return new EditorWaitForSeconds(0.5F);
				Operation.Click(990, 300);	// 点空白处关闭恭喜获得
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("进度奖励");
				for (int i = 0, length = Recognize.DiscountPacksProgress; i < length; ++i) {
					Operation.Click(838 + 39 * i, 350 + (i + 1) % 2 * 100);	// 免费奖励
					yield return new EditorWaitForSeconds(0.5F);
					Operation.Click(990, 300);	// 点空白处关闭恭喜获得
					yield return new EditorWaitForSeconds(0.2F);
				}
			}

			if (Recognize.MoreIsNew) {
				succeed = true;
				Debug.Log("更多红点");
				Operation.Click(1198, 168);	// 更多红点
				yield return new EditorWaitForSeconds(0.2F);
			}

			for (int i = 0; i < 10 && (i == 0 || Recognize.IsWindowCovered); i++) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.3F);
			}

			Task.CurrentTask = null;

			yield return new EditorWaitForSeconds(2F);
			if (!succeed) {
				yield return new EditorWaitForSeconds(FAILED_COOLDOWN_MINUTE * 60);
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
