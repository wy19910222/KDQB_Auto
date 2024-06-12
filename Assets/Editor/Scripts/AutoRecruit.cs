/*
 * @Author: wangyun
 * @CreateTime: 2024-03-23 03:25:46 708
 * @LastEditor: wangyun
 * @EditTime: 2024-03-23 03:25:46 713
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AutoRecruit {
	public static TimeSpan SENIOR_RECRUIT_COOLDOWN = new TimeSpan(1, 0, 0, 0);
	public static TimeSpan GENERAL_RECRUIT_COOLDOWN = new TimeSpan(0, 5, 0);
	public static TimeSpan SKILL_RECRUIT_COOLDOWN = new TimeSpan(0, 5, 0);
	public static int GENERAL_RECRUIT_TIMES_MAX = 5;
	public static int SKILL_RECRUIT_TIMES_MAX = 5;
	
	public static DateTime s_SeniorRecruitTime;	// 最后一次高级招募时间
	public static bool IsSeniorRecruited => s_SeniorRecruitTime + SENIOR_RECRUIT_COOLDOWN > DateTime.Now;	// 是否已高级招募
	
	public static readonly List<DateTime> s_GeneralRecruitTimeList = new List<DateTime>();	// 最后几次普通招募时间
	public static int GeneralRecruitTimes {	// 普通招募次数
		get {
			int times = 0;
			DateTime date = DateTime.Now.Date;
			for (int i = s_GeneralRecruitTimeList.Count - 1; i >= 0; --i) {
				DateTime dt = s_GeneralRecruitTimeList[i];
				if (dt >= date) {
					++times;
				} else {
					break;
				}
			}
			return times;
		}
	}
	public static readonly List<DateTime> s_SkillRecruitTimeList = new List<DateTime>();	// 最后几次技能招募时间
	public static int SkillRecruitTimes {	// 技能招募次数
		get {
			int times = 0;
			DateTime date = DateTime.Now.Date;
			for (int i = s_SkillRecruitTimeList.Count - 1; i >= 0; --i) {
				DateTime dt = s_SkillRecruitTimeList[i];
				if (dt >= date) {
					++times;
				} else {
					break;
				}
			}
			return times;
		}
	}
	
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
			
			if (IsSeniorRecruited) {
				DateTime now = DateTime.Now;
				if ((GeneralRecruitTimes >= GENERAL_RECRUIT_TIMES_MAX || s_GeneralRecruitTimeList[^1] + GENERAL_RECRUIT_COOLDOWN > now)
						&& (SkillRecruitTimes >= SKILL_RECRUIT_TIMES_MAX || s_SkillRecruitTimeList[^1] + SKILL_RECRUIT_COOLDOWN > now)) {
					continue;
				}
			}

			if (Recognize.IsWindowCovered) {
				continue;
			}

			// 不是有英雄按钮的场景
			if (!Recognize.IsOutsideOrInsideScene) {
				continue;
			}

			// if (!Recognize.CanRecruitOuter) {
			// 	continue;
			// }
			
			if (Task.CurrentTask != null) {
				continue;
			}
			Task.CurrentTask = nameof(AutoRecruit);
			
			Debug.Log("外部英雄按钮");
			Operation.Click(1870, 636);	// 外部英雄按钮
			yield return new EditorWaitForSeconds(0.2F);

			// if (Recognize.CanRecruitMiddle) {
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
						DateTime now = DateTime.Now;
						switch (i) {
							case 0:
								s_SeniorRecruitTime = now;
								break;
							case 1: {
								DateTime date = now.Date;
								for (int _i = s_GeneralRecruitTimeList.Count - 1; _i >= 0; --_i) {
									DateTime dt = s_GeneralRecruitTimeList[_i];
									if (dt < date) {
										s_GeneralRecruitTimeList.RemoveAt(_i);
									}
								}
								s_GeneralRecruitTimeList.Add(now);
								break;
							}
							case 2: {
								DateTime date = now.Date;
								for (int _i = s_SkillRecruitTimeList.Count - 1; _i >= 0; --_i) {
									DateTime dt = s_SkillRecruitTimeList[_i];
									if (dt < date) {
										s_SkillRecruitTimeList.RemoveAt(_i);
									}
								}
								s_SkillRecruitTimeList.Add(now);
								break;
							}
						}
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
			// }
			
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
