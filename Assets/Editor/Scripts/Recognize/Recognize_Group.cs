/*
 * @Author: wangyun
 * @CreateTime: 2023-09-27 01:41:06 793
 * @LastEditor: wangyun
 * @EditTime: 2023-09-27 01:41:06 799
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public static partial class Recognize {
	public static int BusyGroupCount {
		get {
			return GetCachedValueOrNew(nameof(BusyGroupCount), () => {
				int deltaY = IsOutsideNearby ? 76 : IsOutsideFaraway ? 0 : -1;
				if (deltaY != -1) {
					deltaY = IsMiniMapShowing switch {
						true => deltaY + 155,
						false => deltaY,
						_ => -1
					};
				}
				if (deltaY != -1) {
					int groupCount = 0;
					// 返回加速等蓝色按钮中间的白色
					Color32 targetBtnColor = new Color32(255, 255, 255, 255);
					// 头像框底色
					Color32 targetAvatarGrayColor = new Color32(184, 205, 220, 255);	// 无英雄
					Color32 targetAvatarBlueColor = new Color32(88, 186, 240, 255);	// 蓝色英雄
					Color32 targetAvatarPurpleColor = new Color32(231, 149, 252, 255);	// 紫色英雄
					Color32 targetAvatarOrangeColor1 = new Color32(251, 200, 84, 255);	// 橙色英雄
					Color32 targetAvatarOrangeColor2 = new Color32(253, 204, 68, 255);	// 橙色英雄
					// 434的位置需要将小地图保持展开状态
					Color32[,] realBtnColors = Operation.GetColorsOnScreen(158, 279 + deltaY + 50, 1, 451);
					Color32[,] realAvatarColors = Operation.GetColorsOnScreen(23, 267 + deltaY + 50, 1, 451);
					while (groupCount < 10) {
						Color32 realBtnColor = realBtnColors[0, groupCount * 50];
						Color32 realAvatarColor = realAvatarColors[0, groupCount * 50];
						// Debug.LogError($"{groupCount}: [158, {279 + deltaY + groupCount * 50 + 50}]: {realBtnColor}: {realAvatarColor}");
						if (groupCount == 4 && deltaY == 76 + 155) {
							if (ApproximatelyCoveredCount(realBtnColor, new Color32(192, 212, 229, 255)) < 0) {
								break;
							}
							if (ApproximatelyCoveredCount(realAvatarColor, targetAvatarGrayColor, 1.5F) < 0
									&& ApproximatelyCoveredCount(realAvatarColor, targetAvatarBlueColor, 1.5F) < 0
									&& ApproximatelyCoveredCount(realAvatarColor, targetAvatarPurpleColor, 1.5F) < 0
									&& ApproximatelyCoveredCount(realAvatarColor, targetAvatarOrangeColor1, 1.5F) < 0
									&& ApproximatelyCoveredCount(realAvatarColor, targetAvatarOrangeColor2, 1.5F) < 0) {
								break;
							}
						} else {
							if (ApproximatelyCoveredCount(realBtnColor, targetBtnColor) < 0) {
								break;
							}
							// Debug.LogError($"{{{23}, {267 + deltaY + 50 + groupCount * 50}}}: {realAvatarColor}");
							if (ApproximatelyCoveredCount(realAvatarColor, targetAvatarGrayColor, 1.5F) < 0
									&& ApproximatelyCoveredCount(realAvatarColor, targetAvatarBlueColor, 1.5F) < 0
									&& ApproximatelyCoveredCount(realAvatarColor, targetAvatarPurpleColor, 1.5F) < 0
									&& ApproximatelyCoveredCount(realAvatarColor, targetAvatarOrangeColor1, 1.5F) < 0
									&& ApproximatelyCoveredCount(realAvatarColor, targetAvatarOrangeColor2, 1.5F) < 0) {
								break;
							}
						}
						groupCount++;
					}
					return groupCount;
				}
				return int.MaxValue;
			});
		}
	}

	public static bool IsAnyGroupIdle {
		get {
			return GetCachedValueOrNew(nameof(IsAnyGroupIdle), () => {
				int deltaY = IsOutsideNearby ? 76 : IsOutsideFaraway ? 0 : -1;
				if (deltaY != -1) {
					deltaY = IsMiniMapShowing switch {
						true => deltaY + 155,
						false => deltaY,
						_ => -1
					};
				}
				if (deltaY != -1) {
					Color32[,] busyCount = Operation.GetColorsOnScreen(145, 231 + deltaY, 10, 13);
					Color32[,] totalCount = Operation.GetColorsOnScreen(160, 231 + deltaY, 10, 13);
					return ApproximatelyRect(busyCount, totalCount, 1, (x, y) => {
						Color32 busyCountColor = busyCount[x, y];
						Color32 totalCountColor = totalCount[x, y];
						return busyCountColor.r == busyCountColor.g && busyCountColor.r == busyCountColor.b ||
								totalCountColor.r == totalCountColor.g && totalCountColor.r == totalCountColor.b;
					}) < 0.9F;
				}
				return false;
			});
		}
	}

	public const string GROUP_STATE_GATHER = "Gather";
	public const string GROUP_STATE_COLLECT = "Collect";
	public static readonly Dictionary<string, Color32[,]> GROUP_STATE_DICT_NEARBY = LoadGroupStateDict("GroupStateDictNearby");
	public static readonly Dictionary<string, Color32[,]> GROUP_STATE_DICT_FARAWAY = LoadGroupStateDict("GroupStateDictFaraway");
	public static string GetGroupState(int groupIndex) {
		return GetCachedValueOrNew("GroupState_" + groupIndex, () => {
			int deltaY = -1;
			Dictionary<string, Color32[,]> stateDict = null;
			if (IsOutsideNearby) {
				deltaY = 76;
				stateDict = GROUP_STATE_DICT_NEARBY;
			} else if (IsOutsideFaraway) {
				deltaY = 0;
				stateDict = GROUP_STATE_DICT_FARAWAY;
			}
			deltaY = IsMiniMapShowing switch {
				true => deltaY + 155,
				false => deltaY,
				_ => -1
			};
			if (deltaY >= 0 && stateDict != null) {
				const int width = 11, height = 9;
				Color32[,] colors = Operation.GetColorsOnScreen(47, 286 + deltaY + groupIndex * 50 + 50, width, height);
				Color32[,] averageColors = new Color32[width, height];
				for (int y = 0; y < height; ++y) {
					for (int x = 0; x < width; ++x) {
						Color32 color1 = colors[x, Mathf.Max(y - 1, 0)];
						Color32 color2 = colors[x, y];
						Color32 color3 = colors[x, Mathf.Min(y + 1, height - 1)];
						float r = (color1.r + color2.r + color3.r) / 3F;
						float g = (color1.g + color2.g + color3.g) / 3F;
						float b = (color1.b + color2.b + color3.b) / 3F;
						averageColors[x, y] = new Color32(
								(byte) Mathf.RoundToInt(r),
								(byte) Mathf.RoundToInt(g),
								(byte) Mathf.RoundToInt(b),
								255
						);
					}
				}
				foreach (var (state, stateColors) in stateDict) {
					if (ApproximatelyRectIgnoreCovered(averageColors, stateColors, 1.2F) > 0.8F) {
						return state;
					}
				}
			}
			return null;
		});
	}
	
	private static void AddGroupStateDictNearby(string state, Color32[,] stateColors) {
		GROUP_STATE_DICT_NEARBY[state] = stateColors;
		SaveGroupStateDict("GroupStateDictNearby", GROUP_STATE_DICT_NEARBY);
	}
	private static void AddGroupStateDictFaraway(string state, Color32[,] stateColors) {
		GROUP_STATE_DICT_FARAWAY[state] = stateColors;
		SaveGroupStateDict("GroupStateDictFaraway", GROUP_STATE_DICT_FARAWAY);
	}
	
	private static Dictionary<string, Color32[,]> LoadGroupStateDict(string type) {
		string key = $"{nameof(Recognize)}.{type}";
		string str = Prefs.Get(key, "{}");
		Dictionary<string, Color32[,]> dict = JsonConvert.DeserializeObject<Dictionary<string, Color32[,]>>(str);
		if (dict is not {Count: > 0}) {
			string filePath = $"{Application.dataPath}/PersistentData/{key}.txt";
			if (File.Exists(filePath)) {
				string valueStr = File.ReadAllText(filePath);
				try {
					dict = JsonConvert.DeserializeObject<Dictionary<string, Color32[,]>>(valueStr);
				} catch (Exception e) {
					Debug.LogError($"队列状态特征丢失：{e}");
				}
			}
		}
		return dict ?? new Dictionary<string, Color32[,]>();
	}
	private static void SaveGroupStateDict(string type, Dictionary<string, Color32[,]> stateDict) {
		string key = $"{nameof(Recognize)}.{type}";
		string filePath = $"{Application.dataPath}/PersistentData/{key}.txt";
		FileInfo file = new FileInfo(filePath);
		if (file.Exists) {
			file.IsReadOnly = false;
			file.Delete();
		} else {
			DirectoryInfo directory = file.Directory;
			if (directory is {Exists: false}) {
				directory.Create();
			}
		}
		using (FileStream fs = file.OpenWrite()) {
			string valueStr = JsonConvert.SerializeObject(stateDict);
			byte[] bytes = Encoding.UTF8.GetBytes(valueStr);
			fs.Write(bytes, 0, bytes.Length); 
			fs.Flush();
		}
		file.IsReadOnly = true;
	}
	
	[MenuItem("Assets/AddGroupStateNearby", priority = -1)]
	private static void AddGroupStateNearby() {
		const int width = 11, height = 9;
		Color32[,] colorsNearby = Operation.GetColorsOnScreen(47, 617, width, height);
		Color32[,] averageColorsNearby = new Color32[width, height];
		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				Color32 color1 = colorsNearby[x, Mathf.Max(y - 1, 0)];
				Color32 color2 = colorsNearby[x, y];
				Color32 color3 = colorsNearby[x, Mathf.Min(y + 1, height - 1)];
				float r = (color1.r + color2.r + color3.r) / 3F;
				float g = (color1.g + color2.g + color3.g) / 3F;
				float b = (color1.b + color2.b + color3.b) / 3F;
				averageColorsNearby[x, y] = new Color32(
						(byte) Mathf.RoundToInt(r),
						(byte) Mathf.RoundToInt(g),
						(byte) Mathf.RoundToInt(b),
						255
				);
			}
		}
		AddGroupStateDictNearby(GROUP_STATE_GATHER, averageColorsNearby);
	}
	[MenuItem("Assets/AddGroupStateFaraway", priority = -1)]
	private static void AddGroupStateFaraway() {
		const int width = 11, height = 9;
		Color32[,] colorsFaraway = Operation.GetColorsOnScreen(47, 541, width, height);
		Color32[,] averageColorsFaraway = new Color32[width, height];
		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				Color32 color1 = colorsFaraway[x, Mathf.Max(y - 1, 0)];
				Color32 color2 = colorsFaraway[x, y];
				Color32 color3 = colorsFaraway[x, Mathf.Min(y + 1, height - 1)];
				float r = (color1.r + color2.r + color3.r) / 3F;
				float g = (color1.g + color2.g + color3.g) / 3F;
				float b = (color1.b + color2.b + color3.b) / 3F;
				averageColorsFaraway[x, y] = new Color32(
						(byte) Mathf.RoundToInt(r),
						(byte) Mathf.RoundToInt(g),
						(byte) Mathf.RoundToInt(b),
						255
				);
			}
		}
		AddGroupStateDictFaraway(GROUP_STATE_GATHER, averageColorsFaraway);
	}
}
