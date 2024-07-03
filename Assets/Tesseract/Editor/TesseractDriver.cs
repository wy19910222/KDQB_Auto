using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class TesseractDriver {
	private TesseractWrapper m_Tesseract;

	public string CheckTessVersion() {
		m_Tesseract = new TesseractWrapper();
		try {
			string version = "Tesseract version: " + TesseractWrapper.Version;
			Debug.Log(version);
			return version;
		} catch (Exception e) {
			string errorMessage = e.GetType() + " - " + e.Message;
			Debug.LogError("Tesseract version: " + errorMessage);
			return errorMessage;
		}
	}

	public bool Setup() {
		string tessDataPath = GetTessDataPath();
		if (!string.IsNullOrEmpty(tessDataPath)) {
			m_Tesseract = new TesseractWrapper();
			if (m_Tesseract.Init("chi_sim", tessDataPath)) {
				Debug.Log("Init Successful");
				return true;
			} else {
				Debug.LogError(m_Tesseract.ErrorMessage);
			}
		}
		return false;
	}

	public (string, int)[] Recognize(Texture2D imageToRecognize, bool highlight = false) {
		return m_Tesseract.Recognize(imageToRecognize, highlight);
	}

	public (string, int)[] Recognize(Color32[] colors, int width, int height, bool highlight = false) {
		return m_Tesseract.Recognize(colors, width, height, highlight);
	}

	public string RecognizeText(Texture2D imageToRecognize, bool highlight = false) {
		return m_Tesseract.RecognizeText(imageToRecognize, highlight);
	}

	public string RecognizeText(Color32[] colors, int width, int height, bool highlight = false) {
		return m_Tesseract.RecognizeText(colors, width, height, highlight);
	}

	private static string GetTessDataPath() {
		string[] guids = AssetDatabase.FindAssets("tessdata");
		foreach (string guid in guids) {
			string path = AssetDatabase.GUIDToAssetPath(guid);
			if (Directory.Exists(path)) {
				return path;
			}
		}
		return null;
	}
}
