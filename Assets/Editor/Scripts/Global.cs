/*
 * @Author: wangyun
 * @CreateTime: 2024-05-11 15:09:39 704
 * @LastEditor: wangyun
 * @EditTime: 2024-05-11 15:09:39 708
 */

using System;
using System.Collections.Generic;

public struct Squad {
	public bool valid;	// 是否生效
	public Recognize.HeroType leader; // 领队
}

public class Global {
	public static long UNATTENDED_THRESHOLD = 30 * 1000_000_0; // 30秒
	public static int ENERGY_FULL = 95;	// 体力上限
	public static bool DAN_EXIST = true;	// 是否有戴安娜
	public static int PERSISTENT_GROUP_COUNT = 4;	// 拥有永久行军队列数
	public static readonly List<DateTime> TEMPORARY_GROUP_COUNTDOWN = new List<DateTime>();	// 拥有临时行军队列数
	public static readonly List<Squad> SQUAD_LIST = new List<Squad>();	// 各编队领队英雄
	
	public static int GROUP_COUNT {
		get {
			int count = PERSISTENT_GROUP_COUNT;
			DateTime now = DateTime.Now;
			foreach (DateTime time in TEMPORARY_GROUP_COUNTDOWN) {
				if (time > now) {
					++count;
				}
			}
			return count;
		}
	}

	public static Recognize.HeroType GetLeader(int squadNumber) {
		if (squadNumber > 0 && squadNumber <= SQUAD_LIST.Count) {
			return SQUAD_LIST[squadNumber - 1].leader;
		}
		return Recognize.HeroType.DAN;
	}
}