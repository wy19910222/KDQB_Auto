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

public class Follow2Config : PrefsEditorWindow<Follow2> {
	[MenuItem("Window/Follow2")]
	private static void Open() {
		GetWindow<Follow2Config>("跟车2.0").Show();
	}
	
	private void OnGUI() {
		Follow2.KEEP_NO_WINDOW = EditorGUILayout.Toggle("在外面跟车", Follow2.KEEP_NO_WINDOW);
		Follow2.GROUP_COUNT = EditorGUILayout.IntSlider("拥有行军队列", Follow2.GROUP_COUNT, 0, 7);
		GUILayout.Space(5F);
		Follow2.INCLUDE_JD = EditorGUILayout.Toggle("跟据点", Follow2.INCLUDE_JD);
		Follow2.INCLUDE_ZC = EditorGUILayout.Toggle("跟战锤", Follow2.INCLUDE_ZC);
		Follow2.INCLUDE_NMY = EditorGUILayout.Toggle("跟难民营", Follow2.INCLUDE_NMY);
		Follow2.INCLUDE_AXPP = EditorGUILayout.Toggle("跟砰砰", Follow2.INCLUDE_AXPP);
		Follow2.INCLUDE_JX = EditorGUILayout.Toggle("跟惧星", Follow2.INCLUDE_JX);
		Follow2.INCLUDE_JW = EditorGUILayout.Toggle("跟精卫", Follow2.INCLUDE_JW);
		GUILayout.Space(5F);
		if (Follow2.IsRunning) {
			if (GUILayout.Button("关闭")) {
				EditorApplication.ExecuteMenuItem("Assets/StopFollow_2.0");
			}
		} else {
			if (GUILayout.Button("开启")) {
				EditorApplication.ExecuteMenuItem("Assets/StartFollow_2.0");
			}
		}
	}
}

public class Follow2 {
	public static bool KEEP_NO_WINDOW = true;	// 是否在非跟车界面跟车
	public static int GROUP_COUNT = 4;	// 拥有行军队列数
	
	public static bool INCLUDE_JD = true;	// 是否跟据点
	public static bool INCLUDE_ZC = true;	// 是否跟战锤
	public static bool INCLUDE_JW = true;	// 是否跟精卫
	public static bool INCLUDE_NMY = true;	// 是否跟难民营
	public static bool INCLUDE_AXPP = true;	// 是否跟爱心砰砰
	public static bool INCLUDE_JX = false;	// 是否跟惧星
	
	private static Color32[] s_CachedOwnerAvatarFeature;	// 缓存的集结发起人头像特征
	private static EditorCoroutine s_CO;
	public static bool IsRunning => s_CO != null;

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
			s_CO = null;
			Debug.Log("自动跟车已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			// 队列数量
			if (Recognize.BusyGroupCount >= GROUP_COUNT) {
				continue;
			}
			bool followWindowOpened = false;
			// 是否有加入按钮
			if (!Recognize.IsFollowJoinBtnExist) {
				// 是否在非跟车界面跟车
				if (!KEEP_NO_WINDOW) {
					// 如果不在跟车界面，但要在跟车界面跟车，则不符合跟车条件
					continue;
				}
				// 如果有界面覆盖，则说明正在操作别的
				if (Recognize.IsWindowCovered && !Recognize.IsFollowJoinBtnExist) {
					continue;
				}
				if (!Recognize.IsFollowOuterJoinBtnExist) {
					continue;
				}
				Debug.Log("外面加入按钮");
				Operation.Click(1771, 714);	// 加入按钮
				followWindowOpened = true;
				yield return new EditorWaitForSeconds(0.1F);
				// 是否有加入按钮(切后台可能导致动画阻塞，从而外面有按钮实际集结已结束)
				if (!Recognize.IsFollowJoinBtnExist) {
					goto EndOfFollow;
				}
			}
			// 是否已加入
			if (Recognize.HasFollowJoined) {
				goto EndOfFollow;
			}
			Debug.Log("未加入");
			// 是否已显示Icon
			if (!Recognize.IsFollowIconExist) {
				Debug.Log("未显示Icon");
				goto EndOfFollow;
			}
			// 如果不跟黑暗军团据点
			if (!INCLUDE_JD && Recognize.IsJDCanFollow) {
				Debug.Log("不跟黑暗军团据点");
				goto EndOfFollow;
			}
			// 如果不跟战锤
			if (!INCLUDE_ZC && Recognize.IsZCCanFollow) {
				Debug.Log("不跟战锤");
				goto EndOfFollow;
			}
			// 如果不跟爱心砰砰
			if (!INCLUDE_AXPP && Recognize.IsAXPPCanFollow) {
				Debug.Log("不跟爱心砰砰");
				goto EndOfFollow;
			}
			// 如果不跟难民营
			if (!INCLUDE_NMY && Recognize.IsNMYCanFollow) {
				Debug.Log("不跟难民营");
				goto EndOfFollow;
			}
			// 如果不跟精卫
			if (!INCLUDE_JW && Recognize.IsJWCanFollow) {
				Debug.Log("不跟精卫");
				goto EndOfFollow;
			}
			// 如果不跟惧星
			if (!INCLUDE_JX && Recognize.IsJXCanFollow) {
				Debug.Log("不跟惧星");
				goto EndOfFollow;
			}
			Debug.Log("可以跟车");
			
			Color32[] ownerAvatarFeature = Recognize.GetFollowOwnerAvatar();
			// 如果上次没失败，则s_CachedOwnerAvatarFeature为null
			// 如果上次失败了且集结发起人头像没换，则不加入
			if (s_CachedOwnerAvatarFeature != null && Recognize.ApproximatelyBigAvatar(ownerAvatarFeature, s_CachedOwnerAvatarFeature)) {
				goto EndOfFollow;
			}
			Debug.Log("决定跟车");
			// 清除失败的记录
			s_CachedOwnerAvatarFeature = null;
			Debug.Log("加入按钮");
			Operation.Click(968, 307);	// 加入按钮
			yield return new EditorWaitForSeconds(0.2F);
			Debug.Log("士兵卡片");
			Operation.Click(1458, 962);	// 士兵卡片
			yield return new EditorWaitForSeconds(0.1F);
			Debug.Log("出征按钮");
			Operation.Click(961, 476);	// 出征按钮
			yield return new EditorWaitForSeconds(0.2F);
			// 如果出现赶不上弹框，则取消出征
			if (Recognize.IsTooLateWindowExist) {
				Debug.Log("取消按钮");
				Operation.Click(900, 657);	// 取消按钮
				// 记录加入失败时的集结发起人头像特征
				s_CachedOwnerAvatarFeature = ownerAvatarFeature;
				yield return new EditorWaitForSeconds(0.2F);
			}
			// 如果还停留在出征界面，则退出
			if (Recognize.CurrentScene == Recognize.Scene.ARMY_SELECTING) {
				Debug.Log("退出按钮");
				Operation.Click(50, 130);	// 退出按钮
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("确认退出按钮");
				Operation.Click(1020, 657);	// 确认退出按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			EndOfFollow:
			if (followWindowOpened) {
				// 如果是从外面进来的，则关闭跟车界面
				Debug.Log("左上角返回按钮");
				while (Recognize.IsWindowCovered) {
					Operation.Click(735, 128);	// 左上角返回按钮
					yield return new EditorWaitForSeconds(0.2F);
				}
				// 外面的按钮持续几秒钟才消失
				yield return new EditorWaitForSeconds(1F);
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}
}
