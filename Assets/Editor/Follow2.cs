/*
 * @Author: wangyun
 * @CreateTime: 2023-09-28 02:57:33 976
 * @LastEditor: wangyun
 * @EditTime: 2023-09-28 02:57:33 983
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public static class Follow2 {
	private static bool INCLUDE_JD = true;	// 是否跟据点
	private static bool INCLUDE_ZC = false;	// 是否跟战锤
	private static bool INCLUDE_JW = true;	// 是否跟精卫
	private static bool INCLUDE_NMY = true;	// 是否跟难民营
	private static bool INCLUDE_AXPP = true;	// 是否跟爱心砰砰
	private static bool INCLUDE_JX = false;	// 是否跟惧星
	private static int GROUP_COUNT = 4;	// 拥有行军队列数
	
	private static Color32[] s_CachedOwnerAvatarFeature;	// 缓存的集结发起人头像特征
	private static EditorCoroutine s_CO;

	[MenuItem("Assets/StartFollow_2.0", priority = -1)]
	private static void Enable() {
		Disable();
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
		List<string> switches = new List<string>();
		if (INCLUDE_ZC) { switches.Add("战锤"); }
		if (INCLUDE_AXPP) { switches.Add("砰砰"); }
		if (INCLUDE_NMY) { switches.Add("难民营"); }
		if (INCLUDE_JW) { switches.Add("精卫"); }
		if (INCLUDE_JD) { switches.Add("据点"); }
		if (INCLUDE_JX) { switches.Add("惧星"); }
		Debug.Log($"自动跟车已开启：{string.Join("、", switches)}");
	}

	[MenuItem("Assets/StopFollow_2.0", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			Debug.Log("自动跟车已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			// 队列数量
			if (Recognize.BusyGroupCount > GROUP_COUNT) {
				continue;
			}
			// 是否有加入按钮
			if (!Recognize.IsFollowJoinBtnExist) {
				continue;
			}
			// 是否已加入
			if (Recognize.HasFollowJoined) {
				continue;
			}
			// 是否已加入
			if (!Recognize.IsFollowIconExist) {
				continue;
			}
			// 如果不跟黑暗军团据点
			if (!INCLUDE_JD && Recognize.IsJDCanFollow) {
				continue;
			}
			// 如果不跟战锤
			if (!INCLUDE_ZC && Recognize.IsZCCanFollow) {
				continue;
			}
			// 如果不跟爱心砰砰
			if (!INCLUDE_AXPP && Recognize.IsAXPPCanFollow) {
				continue;
			}
			// 如果不跟难民营
			if (!INCLUDE_NMY && Recognize.IsNMYCanFollow) {
				continue;
			}
			// 如果不跟精卫
			if (!INCLUDE_JW && Recognize.IsJWCanFollow) {
				continue;
			}
			// 如果不跟惧星
			if (!INCLUDE_JX && Recognize.IsJXCanFollow) {
				continue;
			}
			
			Color32[] ownerAvatarFeature = Recognize.GetFollowOwnerAvatar();
			// 如果上次没失败，则s_CachedOwnerAvatarFeature为null
			// 如果上次失败了且集结发起人头像没换，则不加入
			if (s_CachedOwnerAvatarFeature != null && Recognize.ApproximatelyBigAvatar(ownerAvatarFeature, s_CachedOwnerAvatarFeature)) {
				continue;
			}
			// 清除失败的记录
			s_CachedOwnerAvatarFeature = null;
			Operation.Click(968, 307);	// 加入按钮
			yield return new EditorWaitForSeconds(0.2F);
			Operation.Click(1458, 962);	// 士兵卡片
			yield return new EditorWaitForSeconds(0.1F);
			Operation.Click(961, 476);	// 出征按钮
			yield return new EditorWaitForSeconds(0.2F);
			// 如果出现赶不上弹框，则取消出征
			if (Recognize.IsTooLateWindowExist) {
				Operation.Click(900, 657);	// 取消按钮
				// 记录加入失败时的集结发起人头像特征
				s_CachedOwnerAvatarFeature = ownerAvatarFeature;
				yield return new EditorWaitForSeconds(0.2F);
			}
			// 如果还停留在出征界面，则退出
			if (Recognize.CurrentScene == Recognize.Scene.ARMY_SELECTING) {
				Operation.Click(50, 130);	// 退出按钮
				yield return new EditorWaitForSeconds(0.2F);
				Operation.Click(1020, 657);	// 确认退出按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
