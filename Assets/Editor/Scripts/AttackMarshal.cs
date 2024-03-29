﻿/*
 * @Author: wangyun
 * @CreateTime: 2023-10-31 07:02:42 199
 * @LastEditor: wangyun
 * @EditTime: 2023-10-31 07:02:42 204
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackMarshal {
	public static int SQUAD_NUMBER = 1;	// 使用编队号码
	public static int ATTACK_TIMES = 5;	// 攻击总次数
	public static bool USE_SMALL_BOTTLE = false;	// 是否使用小体
	public static bool USE_BIG_BOTTLE = false;	// 是否使用大体
	public static int MARSHAL_WAIT_SECONDS = 300;	// 未发现元帅等待重新尝试时间
	public static int ENERGY_WAIT_SECONDS = 300;	// 体力不足等待重新尝试时间
	
	public static readonly List<DateTime> s_AttackTimeList = new List<DateTime>();	// 攻击次数
	public static int AttackTimes {	// 攻击次数
		get {
			int times = 0;
			DateTime date = DateTime.Now.Date;
			for (int i = s_AttackTimeList.Count - 1; i >= 0; --i) {
				DateTime dt = s_AttackTimeList[i];
				if (dt > date) {
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

	[MenuItem("Tools_Task/StartAttackMarshal", priority = -1)]
	private static void Enable() {
		Disable();
		List<string> switches = new List<string> {
			$"使用编队【{SQUAD_NUMBER}】"
		};
		if (USE_SMALL_BOTTLE) { switches.Add("【允许使用小体】"); }
		if (USE_BIG_BOTTLE) { switches.Add("【允许使用大体】"); }
		Debug.Log($"自动打元帅已开启，{string.Join("，", switches)}");
		s_CO = EditorCoroutineManager.StartCoroutine(Update());
	}

	[MenuItem("Tools_Task/StopAttackMarshal", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			s_CO = null;
			Debug.Log("自动打元帅已关闭");
		}
	}

	private static IEnumerator Update() {
		// bool prevIsMarshalTime = false;
		while (true) {
			yield return null;
			if (Task.CurrentTask != null && Task.CurrentTask != nameof(AttackMarshal)) {
				continue;
			}
			
			int hour = DateTime.Now.Hour;
			if (hour is not (4 or 12 or 20)) {
				if (Task.CurrentTask == nameof(AttackMarshal)) {
					Task.CurrentTask = null;
				}
				continue;
			}

			int attackTimes = AttackTimes;
			if (attackTimes >= ATTACK_TIMES) {
				if (Task.CurrentTask == nameof(AttackMarshal)) {
					Task.CurrentTask = null;
				}
				continue;
			}
			Debug.Log($"剩余攻击次数：{ATTACK_TIMES - attackTimes}");
			
			Task.CurrentTask = nameof(AttackMarshal);

			if (Recognize.BusyGroupCount >= Recognize.GROUP_COUNT ||
					Recognize.GetHeroGroupNumber(Recognize.HeroType.MRX) >= 0 ||
					Recognize.GetHeroGroupNumber(Recognize.HeroType.YLK) >= 0) {
				continue;
			}
			Debug.Log("当前忙碌队列数量: " + Recognize.BusyGroupCount);

			if (Recognize.IsWindowCovered) {	// 如果有窗口覆盖，说明用户正在操作
				continue;
			}
			Debug.Log($"无窗口覆盖");
			
			if (!Recognize.IsMarshalExist) {
				Debug.Log($"未检测到元帅按钮，尝试切换场景");
				Operation.Click(1170, 970);	// 右下角主城与世界切换按钮
				yield return new EditorWaitForSeconds(2F);
				if (Recognize.CurrentScene == Recognize.Scene.INSIDE) {
					Operation.Click(1170, 970);	// 右下角主城与世界切换按钮
					yield return new EditorWaitForSeconds(2F);
				}
				yield return new EditorWaitForSeconds(1F);
				if (!Recognize.IsMarshalExist) {
					Debug.Log($"切换场景后还是没有元帅");
					yield return new EditorWaitForSeconds(MARSHAL_WAIT_SECONDS);	// 5分钟后再重新尝试
					continue;
				}
				Debug.Log("当前忙碌队列数量: " + Recognize.BusyGroupCount);
				if (Recognize.BusyGroupCount >= Recognize.GROUP_COUNT ||
						Recognize.GetHeroGroupNumber(Recognize.HeroType.MRX) >= 0 ||
						Recognize.GetHeroGroupNumber(Recognize.HeroType.YLK) >= 0) {
					continue;
				}
			}
			
			Debug.Log("元帅按钮");
			Operation.Click(800, 800);	// 元帅按钮
			yield return new EditorWaitForSeconds(0.3F);
			Debug.Log("快速搜索按钮");
			Operation.Click(960, 720);	// 快速搜索按钮
			yield return new EditorWaitForSeconds(0.3F);
			Debug.Log("选中目标");
			Operation.Click(960, 520);	// 选中目标
			yield return new EditorWaitForSeconds(0.2F);
			Debug.Log("攻击按钮");
			// Operation.Click(1050, 840);	// 攻击按钮
			Operation.Click(1050, 670);	// 攻击按钮
			yield return new EditorWaitForSeconds(0.3F);
			// 出现体力不足面板
			if (Recognize.IsEnergyShortcutAdding) {
				// 快捷嗑药
				Recognize.EnergyShortcutAddingType useBottle = RandomUseBottle();	// 随机使用大小体
				int i = 0;
				int iMax = 0;
				switch (useBottle) {
					case Recognize.EnergyShortcutAddingType.SMALL_BOTTLE:
						Debug.Log("使用小体");
						iMax = 3;
						break;
					case Recognize.EnergyShortcutAddingType.BIG_BOTTLE:
						Debug.Log("使用大体");
						iMax = 1;
						break;
				}
				while (Recognize.IsEnergyShortcutAdding && i < iMax) {
					List<Recognize.EnergyShortcutAddingType> types = Recognize.GetShortcutTypes();
					int index = types.IndexOf(useBottle);
					if (index != -1) {
						Debug.Log($"嗑{index + 1}号位");
						Operation.Click(828 + index * 130, 590);	// 选中图标
						yield return new EditorWaitForSeconds(0.1F);
						Operation.Click(960, 702);	// 使用按钮
						yield return new EditorWaitForSeconds(0.1F);
					} else {
						Debug.LogError("体力药剂数量不足！");
					}
					Operation.Click(1170, 384);	// 关闭按钮
					yield return new EditorWaitForSeconds(0.3F);
					Operation.Click(960, 580);	// 选中目标
					yield return new EditorWaitForSeconds(0.1F);
					Operation.Click(870, 430);	// 攻击5次按钮
					yield return new EditorWaitForSeconds(0.3F);
					i++;
				}
				if (Recognize.IsEnergyShortcutAdding) {
					Operation.Click(1170, 384);	// 关闭按钮
					Debug.Log("体力不足，等待稍后尝试");
					yield return new EditorWaitForSeconds(ENERGY_WAIT_SECONDS);
				}
			}
			if (Recognize.CurrentScene == Recognize.Scene.FIGHTING) {
				Debug.Log($"选择队列{SQUAD_NUMBER}");
				Operation.Click(1145 + 37 * SQUAD_NUMBER, 870);	// 选择队列
				yield return new EditorWaitForSeconds(0.2F);
				Debug.Log("出战按钮");
				Operation.Click(960, 470);	// 出战按钮
				Debug.Log("出发");
				s_AttackTimeList.Add(DateTime.Now);
				while (s_AttackTimeList.Count > ATTACK_TIMES) {
					s_AttackTimeList.RemoveAt(0);
				}
			}
			yield return new EditorWaitForSeconds(0.5F);
			// 如果还停留在出征界面(比如点出战按钮前一瞬间元帅没了)，则退出
			if (Recognize.CurrentScene == Recognize.Scene.FIGHTING) {
				Debug.Log("退出按钮");
				Operation.Click(30, 140);	// 退出按钮
				yield return new EditorWaitForSeconds(0.3F);
				Debug.Log("确认退出按钮");
				Operation.Click(1064, 634);	// 确认退出按钮
				yield return new EditorWaitForSeconds(0.2F);
			}
			
			yield return new EditorWaitForSeconds(5);
		}
		// ReSharper disable once IteratorNeverReturns
	}
	
	private static Recognize.EnergyShortcutAddingType RandomUseBottle() {
		List<Recognize.EnergyShortcutAddingType> list = new List<Recognize.EnergyShortcutAddingType>();
		if (USE_SMALL_BOTTLE) { list.Add(Recognize.EnergyShortcutAddingType.SMALL_BOTTLE); }
		if (USE_BIG_BOTTLE) { list.Add(Recognize.EnergyShortcutAddingType.BIG_BOTTLE); }
		return list.Count > 0 ? list[UnityEngine.Random.Range(0, list.Count)] : Recognize.EnergyShortcutAddingType.NONE;
	}
}
