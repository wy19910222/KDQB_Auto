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
			int deltaY = IsOutsideNearby ? 76 : IsOutsideFaraway ? 0 : -1;
			if (deltaY >= 0) {
				int groupCount = 0;
				// 返回加速等蓝色按钮中间的白色
				Color32 targetColor = new Color32(255, 255, 255, 255);
				// 434的位置需要将小地图保持展开状态
				Color32[,] realColors = Operation.GetColorsOnScreen(158, 434 + deltaY, 1, 451);
				while (groupCount < 10) {
					Color32 realColor = realColors[0, groupCount * 50];
					// Debug.LogError($"{groupCount}: [158, {434 + deltaY + groupCount * 50}]: {realColor}");
					if (ApproximatelyCoveredCount(realColor, targetColor) < 0) {
						break;
					}
					groupCount++;
				}
				return groupCount;
			}
			return int.MaxValue;
		}
	}
	
	public const string GROUP_STATE_GATHER = "Gather";
	public const string GROUP_STATE_COLLECT = "Collect";
	public static readonly Dictionary<string, Color32[,]> GROUP_STATE_DICT_NEARBY = LoadGroupStateDict("GroupStateDictNearby");
	public static readonly Dictionary<string, Color32[,]> GROUP_STATE_DICT_FARAWAY = LoadGroupStateDict("GroupStateDictFaraway");
	public static string GetGroupState(int groupIndex) {
		int deltaY = -1;
		Dictionary<string, Color32[,]> stateDict = null;
		if (IsOutsideNearby) {
			deltaY = 76;
			stateDict = GROUP_STATE_DICT_NEARBY;
		} else if (IsOutsideFaraway) {
			deltaY = 0;
			stateDict = GROUP_STATE_DICT_FARAWAY;
		}
		if (deltaY >= 0 && stateDict != null) {
			const int width = 11, height = 9;
			Color32[,] colors = Operation.GetColorsOnScreen(47, 441 + deltaY + groupIndex * 50, width, height);
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
	}
	
	public static void AddGroupStateDictNearby(string state, Color32[,] stateColors) {
		GROUP_STATE_DICT_NEARBY[state] = stateColors;
		SaveGroupStateDict("GroupStateDictNearby", GROUP_STATE_DICT_NEARBY);
	}
	public static void AddGroupStateDictFaraway(string state, Color32[,] stateColors) {
		GROUP_STATE_DICT_FARAWAY[state] = stateColors;
		SaveGroupStateDict("GroupStateDictFaraway", GROUP_STATE_DICT_FARAWAY);
	}
	
	private static Dictionary<string, Color32[,]> LoadGroupStateDict(string type) {
		string key = $"{nameof(Recognize)}.{type}";
		string str = EditorPrefs.GetString(key, "{}");
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
