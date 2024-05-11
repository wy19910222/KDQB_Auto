/*
 * @Author: wangyun
 * @CreateTime: 2024-05-11 15:09:39 704
 * @LastEditor: wangyun
 * @EditTime: 2024-05-11 15:09:39 708
 */

using System.Collections.Generic;

public struct Squad {
	public bool valid;	// 是否生效
	public Recognize.HeroType leader; // 领队
}

public class Global {
	public static long UNATTENDED_THRESHOLD = 30 * 1000_000_0; // 30秒
	public static int GROUP_COUNT = 4;	// 拥有行军队列数
	public static int ENERGY_FULL = 95;	// 体力上限
	public static readonly List<Squad> SQUAD_LIST = new List<Squad>();	// 领队英雄

	public static Recognize.HeroType GetLeader(int squadNumber) {
		if (squadNumber > 0 && squadNumber <= SQUAD_LIST.Count) {
			return SQUAD_LIST[squadNumber - 1].leader;
		}
		return Recognize.HeroType.DAN;
	}
}