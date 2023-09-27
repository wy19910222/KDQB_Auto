/*
 * @Author: wangyun
 * @CreateTime: 2023-09-07 20:18:29 730
 * @LastEditor: wangyun
 * @EditTime: 2023-09-07 20:18:29 842
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public static class Follow {
	private static bool INCLUDE_JD = true;	// 是否跟据点
	private static bool INCLUDE_ZC = false;	// 是否跟战锤
	private static bool INCLUDE_JW = true;	// 是否跟精卫
	private static bool INCLUDE_NMY = true;	// 是否跟难民营
	private static bool INCLUDE_AXPP = true;	// 是否跟爱心砰砰
	private static bool INCLUDE_JX = false;	// 是否跟惧星
	private static int GROUP_COUNT = 4;	// 拥有行军队列数
	
	private static readonly List<Vector2Int> s_PosList = new List<Vector2Int>() {
		new Vector2Int(968, 307),	// 加入集结按钮
		new Vector2Int(1458, 962),	// 士兵卡片
		new Vector2Int(961, 476),	// 出战按钮
		new Vector2Int(900, 657),		// 取消按钮（如果出现必须加速才能抵达集结点的确认框）
		new Vector2Int(50, 130),		// 返回按钮（为避免出战失败卡死，额外点击一下返回）
		new Vector2Int(1020, 657),		// 确认按钮（是否确认退出战斗）
	};
	
	private static readonly RectInt s_OwnerAvatarRect = new RectInt(731, 190, 55, 54);	// 集结发起人头像范围
	private const int OWNER_AVATAR_FEATURE_XY_COUNT = 5; // 集结发起人头像特征获取的阵列边长
	private static Color32[] s_CachedOwnerAvatarFeature;	// 缓存的集结发起人头像特征
	
	// private static readonly List<(Vector2Int, Color32)> s_List = new List<(Vector2Int, Color32)>() {
	// 	(new Vector2Int(1, 141), new Color32(94, 126, 202, 255)),	// 出战界面左上角返回按钮
	// 	
	// 	(new Vector2Int(965, 305), new Color32(148, 148, 155, 255)),	// 灰色加号按钮
	// 	(new Vector2Int(965, 280), new Color32(170, 169, 171, 255)),	// 灰色加号按钮
	// 	(new Vector2Int(952, 297), new Color32(197, 194, 192, 255)),	// 灰色加号按钮
	
	// 	(new Vector2Int(790, 325), new Color32(147, 199, 167, 255)),	// 绿色已加入箭头
	// 	(new Vector2Int(790, 325), new Color32(44, 89, 115, 255)),	// 覆盖了“已加入当前发起的集结”提示的绿色已加入箭头
	
	// 	(new Vector2Int(170, 240), new Color32(17, 37, 61, 255)),	// 世界界面左上角小地图按钮（近）（相差76）
	// 	(new Vector2Int(170, 164), new Color32(17, 37, 61, 255)),	// 世界界面左上角小地图按钮（远）（相差76）
	// 	(new Vector2Int(145, 514), new Color32(29, 40, 68, 255)),	// 蓝色返回按钮（每行50）
	// 	(new Vector2Int(157, 603), new Color32(33, 45, 74, 255)),	// 蓝色返回按钮（每行50）
	// 	
	// 	(new Vector2Int(1107, 279), new Color32(118, 132, 169, 255)),	// 图标尚未出现时某处
	// 	
	// 	(new Vector2Int(1104, 236), new Color32(41, 39, 48, 255)),	// 黑暗军团据点图标中间红色处
	// 	(new Vector2Int(1142, 310), new Color32(253, 109, 106, 255)),	// 战锤图标中间红色处
	// 	(new Vector2Int(1111, 206), new Color32(196, 99, 112, 255)),	// 精卫图标上部红色处
	
	// 	(new Vector2Int(1111, 206), new Color32(195, 79, 94, 255)),	// 精卫图标上部红色处
	// 	(new Vector2Int(1111, 206), new Color32(193, 80, 95, 255)),	// 精卫图标上部红色处
	// 	(new Vector2Int(1111, 206), new Color32(193, 78, 93, 255)),	// 精卫图标上部红色处
	// 	(new Vector2Int(1111, 206), new Color32(183, 76, 91, 255)),	// 精卫图标上部红色处
	
	// 	(new Vector2Int(1113, 215), new Color32(153, 86, 93, 255)),	// 精卫图标上部红色处
	// 	(new Vector2Int(1113, 215), new Color32(158, 85, 92, 255)),	// 精卫图标上部红色处
	
	// 	(new Vector2Int(1107, 279), new Color32(48, 65, 101, 255)),	// 难民营图标中间深蓝处
	// 	(new Vector2Int(1049, 321), new Color32(182, 75, 97, 255)),	// 爱心砰砰图标中间红色处
	// 	
	// 	(new Vector2Int(1089, 213), new Color32(40, 129, 217, 255)),	// 惧星图标上部某处
	// 	(new Vector2Int(1089, 213), new Color32(41, 132, 222, 255)),	// 惧星图标上部某处
	// 	(new Vector2Int(1089, 213), new Color32(41, 133, 225, 255)),	// 惧星图标上部某处
	
	// 	(new Vector2Int(880, 700), new Color32(211, 78, 56, 255)),	// 出战界面弹出的赶不上集结提示框的红色取消按钮
	// 	(new Vector2Int(50, 130), new Color32(94, 126, 202, 255)),	// 出战界面左上角蓝色返回按钮
	// };
	
	private static readonly List<Func<bool>> s_ConditionalList = new List<Func<bool>>() {
		() => {
			int deltaY = -1;
			if (Approximately(ScreenshotUtils.GetColorOnScreen(170, 240), new Color32(17, 37, 61, 255), 10)) {
				// 世界界面（近）
				deltaY = 76;
			} else if (Approximately(ScreenshotUtils.GetColorOnScreen(170, 164), new Color32(17, 37, 61, 255), 10)) {
				// 世界界面（远）
				deltaY = 0;
			}
			if (deltaY >= 0) {
				// 获取出征状态的行军队列数量
				int groupCount = 0;
				Color32 targetColor = new Color32(29, 40, 68, 255);
				while (groupCount < 8) {
					Color32 realColor = ScreenshotUtils.GetColorOnScreen(145, 438 + deltaY + groupCount * 50);
					if (!Approximately(realColor, targetColor, 10)) {
						break;
					}
					groupCount++;
				}
				return groupCount < GROUP_COUNT;
			}
			return false;
		},
		() => {
			Color32 targetColor1 = new Color32(148, 148, 155, 255);
			Color32 realColor1 = ScreenshotUtils.GetColorOnScreen(965, 305);
			Color32 targetColor2 = new Color32(170, 169, 171, 255);
			Color32 realColor2 = ScreenshotUtils.GetColorOnScreen(965, 280);
			Color32 targetColor3 = new Color32(197, 194, 192, 255);
			Color32 realColor3 = ScreenshotUtils.GetColorOnScreen(952, 297);
			// 灰色加号按钮存在时才执行
			return Approximately(realColor1, targetColor1, 10) &&
					Approximately(realColor2, targetColor2, 10) &&
					Approximately(realColor3, targetColor3, 10);
		},
		() => {
			Color32 targetColor1 = new Color32(147, 199, 167, 255);
			Color32 targetColor2 = new Color32(44, 89, 115, 255);
			Color32 realColor = ScreenshotUtils.GetColorOnScreen(790, 325);
			//绿色已加入箭头不存在时才执行
			return !Approximately(realColor, targetColor1, 10) &&
					!Approximately(realColor, targetColor2, 10);
		},
		() => {
			Color32 targetColor = new Color32(118, 132, 169, 255);
			Color32 realColor = ScreenshotUtils.GetColorOnScreen(1107, 279);
			// 如果图标尚未出现，则先不执行，看看下一帧怎么样
			return !Approximately(realColor, targetColor, 10);
		},
		() => {
			// 如果不跟黑暗军团据点，则判断是否是黑暗军团据点，不是才执行
			if (!INCLUDE_JD) {
				Color32 targetColor = new Color32(41, 39, 48, 255);
				Color32 realColor = ScreenshotUtils.GetColorOnScreen(1104, 236);
				return !Approximately(realColor, targetColor, 10);
			}
			return true;
		},
		() => {
			// 如果不跟战锤，则判断是否是战锤，不是才执行
			if (!INCLUDE_ZC) {
				Color32 targetColor = new Color32(253, 109, 106, 255);
				Color32 realColor = ScreenshotUtils.GetColorOnScreen(1142, 310);
				return !Approximately(realColor, targetColor, 10);
			}
			return true;
		},
		() => {
			// 如果不跟精卫，则判断是否是精卫，不是才执行
			if (!INCLUDE_JW) {
				Color32 targetColor = new Color32(158, 85, 92, 255);
				Color32 realColor = ScreenshotUtils.GetColorOnScreen(1113, 215);
				return !Approximately(realColor, targetColor, 10);
			}
			return true;
		},
		() => {
			// 如果不跟难民营，则判断是否是难民营，不是才执行
			if (!INCLUDE_NMY) {
				Color32 targetColor = new Color32(48, 65, 101, 255);
				Color32 realColor = ScreenshotUtils.GetColorOnScreen(1107, 279);
				return !Approximately(realColor, targetColor, 10);
			}
			return true;
		},
		() => {
			// 如果不跟爱心砰砰，则判断是否是爱心砰砰，不是才执行
			if (!INCLUDE_AXPP) {
				Color32 targetColor = new Color32(182, 75, 97, 255);
				Color32 realColor = ScreenshotUtils.GetColorOnScreen(1049, 321);
				return !Approximately(realColor, targetColor, 10);
			}
			return true;
		},
		() => {
			// 如果不跟惧星，则判断是否是惧星，不是惧星才执行
			if (!INCLUDE_JX) {
				Color32 targetColor = new Color32(41, 131, 221, 255);
				Color32 realColor = ScreenshotUtils.GetColorOnScreen(1089, 213);
				return !Approximately(realColor, targetColor, 10);
			}
			return true;
		}
	};

	private static readonly Func<bool> s_FailedConditional = () => {
		Color32 targetColor = new Color32(211, 78, 56, 255);
		Color32 realColor = ScreenshotUtils.GetColorOnScreen(880, 700);
		// 如果出现赶不上集结提示框的红色取消按钮，说明加入失败了
		return Approximately(realColor, targetColor, 10);
	};

	private static readonly Func<bool> s_StaySelectingConditional = () => {
		Color32 targetColor = new Color32(94, 126, 202, 255);
		Color32 realColor = ScreenshotUtils.GetColorOnScreen(50, 130);
		// 如果出战界面左上角蓝色返回按钮仍然存在，说明卡在出战界面了
		return Approximately(realColor, targetColor, 10);
	};
	
	private static EditorCoroutine s_CO;

	[MenuItem("Assets/StartFollow", priority = -1)]
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

	[MenuItem("Assets/StopFollow", priority = -1)]
	private static void Disable() {
		if (s_CO != null) {
			EditorCoroutineManager.StopCoroutine(s_CO);
			Debug.Log("自动跟车已关闭");
		}
	}

	private static IEnumerator Update() {
		while (true) {
			yield return null;
			bool willJoin = true;
			foreach (var conditional in s_ConditionalList) {
				if (!conditional()) {
					willJoin = false;
				}
			}
			if (willJoin) {
				Color32[] ownerAvatarFeature = GetAvatarFeature();
				// 如果上次没失败，则s_CachedOwnerAvatarFeature为null
				// 如果上次没失败或者集结发起人头像换了，则可以加入
				if (s_CachedOwnerAvatarFeature == null || !EqualsFeature(ownerAvatarFeature, s_CachedOwnerAvatarFeature)) {
					// 清除失败的记录
					s_CachedOwnerAvatarFeature = null;
					
					bool oldIsLeftDown = MouseUtils.IsLeftDown();
					if (oldIsLeftDown) {
						MouseUtils.LeftUp();
					}
					Vector2Int oldPos = MouseUtils.GetMousePos();
					MouseUtils.SetMousePos(s_PosList[0].x, s_PosList[0].y);	// 加入按钮
					MouseUtils.LeftDown();
					MouseUtils.LeftUp();
					MouseUtils.SetMousePos(oldPos.x, oldPos.y);
					yield return new EditorWaitForSeconds(0.2F);
					MouseUtils.SetMousePos(s_PosList[1].x, s_PosList[1].y);	// 士兵卡片
					MouseUtils.LeftDown();
					MouseUtils.LeftUp();
					MouseUtils.SetMousePos(oldPos.x, oldPos.y);
					yield return new EditorWaitForSeconds(0.1F);
					MouseUtils.SetMousePos(s_PosList[2].x, s_PosList[2].y);	// 出战按钮
					MouseUtils.LeftDown();
					MouseUtils.LeftUp();
					MouseUtils.SetMousePos(oldPos.x, oldPos.y);
					yield return new EditorWaitForSeconds(0.2F);
					if (s_FailedConditional()) {
						MouseUtils.SetMousePos(s_PosList[3].x, s_PosList[3].y);	// 取消按钮
						MouseUtils.LeftDown();
						MouseUtils.LeftUp();
						MouseUtils.SetMousePos(oldPos.x, oldPos.y);
						// 记录加入失败时的集结发起人头像特征
						s_CachedOwnerAvatarFeature = ownerAvatarFeature;
						yield return new EditorWaitForSeconds(0.2F);
					}
					if (s_StaySelectingConditional()) {
						MouseUtils.SetMousePos(s_PosList[4].x, s_PosList[4].y);	// 退出按钮
						MouseUtils.LeftDown();
						MouseUtils.LeftUp();
						MouseUtils.SetMousePos(oldPos.x, oldPos.y);
						yield return new EditorWaitForSeconds(0.2F);
						MouseUtils.SetMousePos(s_PosList[5].x, s_PosList[5].y);	// 确认按钮
						MouseUtils.LeftDown();
						MouseUtils.LeftUp();
						MouseUtils.SetMousePos(oldPos.x, oldPos.y);
					}
					if (oldIsLeftDown) {
						MouseUtils.LeftDown();
					}
				}
			}
		}
		// ReSharper disable once IteratorNeverReturns
	}

	private static bool Approximately(Color32 c1, Color32 c2, int threshold) {
		return Mathf.Abs(c1.r - c2.r) <= threshold &&
				Mathf.Abs(c1.g - c2.g) <= threshold &&
				Mathf.Abs(c1.b - c2.b) <= threshold;
	}

	// [MenuItem("Assets/TestClick", priority = -1)]
	// private static void TestClick() {
	// 	if (s_CO != null) {
	// 		EditorCoroutineManager.StopCoroutine(s_CO);
	// 	}
	// 	s_CO = EditorCoroutineManager.StartCoroutine(IETestClick());
	// }
	//
	// private static IEnumerator IETestClick() {
	// 	yield return new EditorWaitForSeconds(1);
	// 	bool oldIsLeftDown = MouseUtils.IsLeftDown();
	// 	if (oldIsLeftDown) {
	// 		MouseUtils.LeftUp();
	// 	}
	// 	Vector2Int oldPos = MouseUtils.GetMousePos();
	// 	MouseUtils.SetMousePos(1130, 480);	// 设置队列按钮
	// 	MouseUtils.LeftDown();
	// 	MouseUtils.LeftUp();
	// 	MouseUtils.SetMousePos(oldPos.x, oldPos.y);
	// 	yield return new EditorWaitForSeconds(0.1F);
	// 	MouseUtils.SetMousePos(s_PosList[1].x, s_PosList[1].y);	// 士兵卡片
	// 	MouseUtils.LeftDown();
	// 	MouseUtils.LeftUp();
	// 	MouseUtils.SetMousePos(oldPos.x, oldPos.y);
	// 	yield return new EditorWaitForSeconds(0.1F);
	// 	MouseUtils.SetMousePos(s_PosList[2].x, s_PosList[2].y);	// 出战按钮
	// 	MouseUtils.LeftDown();
	// 	MouseUtils.LeftUp();
	// 	MouseUtils.SetMousePos(oldPos.x, oldPos.y);
	// 	if (oldIsLeftDown) {
	// 		MouseUtils.LeftDown();
	// 	}
	// }

	private static Color32[] GetAvatarFeature() {
		int xStepLength = s_OwnerAvatarRect.width / OWNER_AVATAR_FEATURE_XY_COUNT;
		int offsetX = (s_OwnerAvatarRect.width - xStepLength * OWNER_AVATAR_FEATURE_XY_COUNT) >> 1;
		int yStepLength = s_OwnerAvatarRect.height / OWNER_AVATAR_FEATURE_XY_COUNT;
		int offsetY = (s_OwnerAvatarRect.height - yStepLength * OWNER_AVATAR_FEATURE_XY_COUNT) >> 1;
		int startX = s_OwnerAvatarRect.x + offsetX;
		int startY = s_OwnerAvatarRect.y + offsetY;
		Color32[] ownerAvatarFeature = new Color32[OWNER_AVATAR_FEATURE_XY_COUNT * OWNER_AVATAR_FEATURE_XY_COUNT];
		for (int y = 0; y < OWNER_AVATAR_FEATURE_XY_COUNT; ++y) {
			for (int x = 0; x < OWNER_AVATAR_FEATURE_XY_COUNT; ++x) {
				ownerAvatarFeature[y * OWNER_AVATAR_FEATURE_XY_COUNT + x] = ScreenshotUtils.GetColorOnScreen(startX + x, startY + y);
			}
		}
		return ownerAvatarFeature;
	}

	private static bool EqualsFeature(Color32[] feature1, Color32[] feature2) {
		if (feature1.Length != feature2.Length) {
			return false;
		}
		for (int i = 0, length = feature1.Length; i < length; ++i) {
			if (feature1[i].r != feature2[i].r || feature1[i].g != feature2[i].g || feature1[i].b != feature2[i].b) {
				return false;
			}
		}
		return true;
	}
}
