﻿/*
 * @Author: wangyun
 * @CreateTime: 2024-04-11 15:48:11 801
 * @LastEditor: wangyun
 * @EditTime: 2024-04-11 15:48:11 805
 */

using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public static class OCRUtils {
	[DllImport("libocr.dll")]
	private static extern string ocr_file(string path, int pathSize);
	
	[DllImport("libocr.dll")]
	private static extern string ocr_image(int image_width, int image_height, byte[] image_data, int image_data_size);

	[MenuItem("Test/OCRTest")]
	public static void Test() {
		// string path = Application.dataPath + "/testOCR.png";
		// // byte[] bytes = File.ReadAllBytes(path);
		// Color32[,] colors = Operation.GetFromFile("Test11111.png");
		// int width = colors.GetLength(0);
		// int height = colors.GetLength(1);
		// byte[] bytes = new byte[width * height * 3];
		// for (int y = 0; y < height; y++) {
		// 	for (int x = 0; x < width; x++) {
		// 		int index = y * width + x;
		// 		bytes[index * 3] = colors[x, y].r;
		// 		bytes[index * 3 + 1] = colors[x, y].g;
		// 		bytes[index * 3 + 2] = colors[x, y].b;
		// 	}
		// }
		// string str = ocr_image(56, 20, bytes, 4096);
		// // string str = ocr_file(path, 128);
		// Debug.LogError(str);
		// int deltaY = global::Recognize.IsOutsideNearby ? 76 : global::Recognize.IsOutsideFaraway ? 0 : -1;
		// deltaY = global::Recognize.IsMiniMapShowing switch {
		// 	true => deltaY + 155,
		// 	false => deltaY,
		// 	_ => -1
		// };
		// if (deltaY >= 0) {
		// 	string str = Operation.GetTextOnScreen(140, 228 + deltaY, 34, 20);
		// 	Debug.LogError("截屏识别：" + str);
		// 	Operation.Screenshot(140, 228 + deltaY, 34, 20, "Test11111.png");
		// }

		// string path = "Test11111.png";
		// Operation.Screenshot(142, 112, 35, 17, Application.dataPath + "/" + path);
		// Debug.LogError("从文件识别：" + Recognize(path));
	}

	public static string Recognize(string filePath) {
		return ocr_file(Application.dataPath + "/" + filePath, 1024);
	}

	public static string Recognize(Color32[,] colors) {
		int width = colors.GetLength(0);
		int height = colors.GetLength(1);
		byte[] bytes = new byte[width * height * 3];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				int index = y * width + x;
				bytes[index * 3] = colors[x, y].r;
				bytes[index * 3 + 1] = colors[x, y].g;
				bytes[index * 3 + 2] = colors[x, y].b;
			}
		}
		return ocr_image(width, height, bytes, 1024);
	}

	public static string Recognize(int width, int height, byte[] bytes) {
		return ocr_image(width, height, bytes, 1024);
	}
}
