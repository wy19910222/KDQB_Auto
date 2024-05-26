/*
 * @Author: wangyun
 * @CreateTime: 2024-03-23 03:25:46 708
 * @LastEditor: wangyun
 * @EditTime: 2024-03-23 03:25:46 713
 */

using System.Collections;
using UnityEngine;
using UnityEditor;

public class AutoRecruit {
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

	[MenuItem("Tools_Task/StartAutoRecruit", priority = -1)]
	private static void Enable() {
		Disable();
		Debug.Log($"自动招募已开启");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopAutoRecruit", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动招募已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;

			if (Recognize.IsWindowCovered) {
				continue;
			}

			if (!Recognize.CanRecruitOuter) {
				continue;
			}
			
			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(AutoRecruit);
			
			Debug.Log("外部英雄按钮");
			Operation.Click(1870, 636);	// 外部英雄按钮
			yield return new EditorWaitForSeconds(0.2F);

			if (Recognize.CanRecruitMiddle) {
				for (int i = 0; i < 3; ++i) {
					switch (i) {
						case 0:
							Debug.Log("英雄招募按钮");
							Operation.Click(1090, 960);	// 英雄招募按钮（英雄列表界面）
							yield return new EditorWaitForSeconds(0.2F);
							break;
						case 1:
							if (Recognize.CanGeneralRecruit) {
								Debug.Log("普通招募标签");
								Operation.Click(745, 955);	// 普通招募标签
								yield return new EditorWaitForSeconds(0.5F);
								break;
							} else {
								continue;
							}
						case 2:
							if (Recognize.CanSkillRecruit) {
								Debug.Log("技能招募标签");
								Operation.Click(1175, 955);	// 技能招募标签
								yield return new EditorWaitForSeconds(0.5F);
								break;
							} else {
								continue;
							}
					}

					if (Recognize.CanRecruitInner) {
						Debug.Log("招募1次按钮");
						Operation.Click(840, 805);	// 招募1次按钮
						yield return new EditorWaitForSeconds(1F);
						Operation.Click(1060, 640);	// 空白处
						yield return new EditorWaitForSeconds(0.2F);
						Operation.Click(1060, 640);	// 空白处
						yield return new EditorWaitForSeconds(0.2F);
					}
					if (Recognize.CanRecruitExtra) {
						Debug.Log("额外招募按钮");
						Operation.Click(772, 730);	// 额外招募按钮
						yield return new EditorWaitForSeconds(1F);
						Operation.Click(1060, 640);	// 空白处
						yield return new EditorWaitForSeconds(0.2F);
						Operation.Click(1060, 640);	// 空白处
						yield return new EditorWaitForSeconds(0.2F);
					}
				}
			}
			
			for (int i = 0; i < 10 && Recognize.IsWindowCovered; i++) {	// 如果有窗口，多点几次返回按钮
				Debug.Log("关闭窗口");
				Operation.Click(720, 128);	// 左上角返回按钮
				yield return new EditorWaitForSeconds(0.3F);
			}
			
			Task.CurrentTask = null;
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
