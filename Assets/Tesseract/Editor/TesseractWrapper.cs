using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class TesseractWrapper : IDisposable {
	public static string Version => Marshal.PtrToStringAnsi(TessVersion());
	
	private IntPtr m_TessHandle = IntPtr.Zero;
	
	public string m_ErrorMsg;
	public string ErrorMessage => m_ErrorMsg;
	
	public float MinimumConfidence { get; set; } = 60;
	
	public Texture2D HighlightedTexture { get; private set; }

	public bool Init(string lang, string dataPath) {
		if (!m_TessHandle.Equals(IntPtr.Zero)) {
			Dispose();
		}
		try {
			m_TessHandle = TessBaseAPICreate();
			if (m_TessHandle.Equals(IntPtr.Zero)) {
				m_ErrorMsg = "TessAPICreate failed";
				return false;
			}
			if (string.IsNullOrWhiteSpace(dataPath)) {
				m_ErrorMsg = "Invalid DataPath";
				return false;
			}
			int init = TessBaseAPIInit3(m_TessHandle, dataPath, lang);
			if (init != 0) {
				Dispose();
				m_ErrorMsg = "TessAPIInit failed. Output: " + init;
				return false;
			}
		} catch (Exception ex) {
			m_ErrorMsg = ex.GetType() + " -- " + ex.Message;
			return false;
		}
		return true;
	}

	public string RecognizeText(Texture2D texture, bool highlight = false) {
		if (m_TessHandle.Equals(IntPtr.Zero))
			return null;
		int width = texture.width;
		int height = texture.height;
		Color32[] colors = texture.GetPixels32();
		return RecognizeText(colors, width, height, highlight);
	}

	public string RecognizeText(Color32[] colors, int width, int height, bool highlight = false) {
		if (m_TessHandle.Equals(IntPtr.Zero))
			return null;

		(string, int)[] results = Recognize(colors, width, height, highlight);
		StringBuilder sb = new StringBuilder();
		for (int i = 0, length = results.Length; i < length; i++) {
			(string word, int confidence) = results[i];
			Debug.Log(word + " -> " + confidence);
			if (confidence >= MinimumConfidence) {
				sb.Append(word);
				sb.Append(" ");
			}
		}
		return sb.ToString();
	}

	public (string, int)[] Recognize(Texture2D texture, bool highlight = false) {
		if (m_TessHandle.Equals(IntPtr.Zero))
			return null;
		int width = texture.width;
		int height = texture.height;
		Color32[] colors = texture.GetPixels32();
		return Recognize(colors, width, height, highlight);
	}

	public (string, int)[] Recognize(Color32[] colors, int width, int height, bool highlight = false) {
		const int BYTES_PER_PIXEL = 4;
		int byteLength = width * height * BYTES_PER_PIXEL;
		byte[] dataBytes = new byte[byteLength];
		int bytePtr = 0;
		for (int y = height - 1; y >= 0; y--) {
			for (int x = 0; x < width; x++) {
				int colorIdx = y * width + x;
				dataBytes[bytePtr++] = colors[colorIdx].r;
				dataBytes[bytePtr++] = colors[colorIdx].g;
				dataBytes[bytePtr++] = colors[colorIdx].b;
				dataBytes[bytePtr++] = colors[colorIdx].a;
			}
		}

		IntPtr imagePtr = Marshal.AllocHGlobal(byteLength);
		Marshal.Copy(dataBytes, 0, imagePtr, byteLength);

		TessBaseAPISetImage(m_TessHandle, imagePtr, width, height, BYTES_PER_PIXEL, width * BYTES_PER_PIXEL);

		if (TessBaseAPIRecognize(m_TessHandle, IntPtr.Zero) != 0) {
			Marshal.FreeHGlobal(imagePtr);
			return null;
		}

		IntPtr confidencesPointer = TessBaseAPIAllWordConfidences(m_TessHandle);
		List<int> confidence = new List<int>();
		{
			int i = 0;
			while (true) {
				int tempConfidence = Marshal.ReadInt32(confidencesPointer, i * 4);
				if (tempConfidence == -1) break;
				confidence.Add(tempConfidence);
				i++;
			}
		}

		int pointerSize = Marshal.SizeOf(typeof(IntPtr));
		IntPtr intPtr = TessBaseAPIGetWords(m_TessHandle, IntPtr.Zero);
		BoxInfo boxInfo = Marshal.PtrToStructure<BoxInfo>(intPtr);
		Box[] boxes = new Box[boxInfo.n];

		for (int index = 0; index < boxes.Length; index++) {
			if (confidence[index] >= MinimumConfidence) {
				IntPtr boxPtr = Marshal.ReadIntPtr(boxInfo.box, index * pointerSize);
				boxes[index] = Marshal.PtrToStructure<Box>(boxPtr);
			}
		}
		
		if (highlight) {
			Color32[] highlightColors = new Color32[width * height];
			Array.Copy(colors, highlightColors, highlightColors.Length);
			for (int index = 0; index < boxes.Length; index++) {
				if (confidence[index] >= MinimumConfidence) {
					Box box = boxes[index];
					RectInt boundingRect = new RectInt(box.x, height - 1 - (box.y + box.h - 1), box.w, box.h);
					DrawLines(highlightColors, width, boundingRect, Color.green);
				}
			}
			HighlightedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
			HighlightedTexture.SetPixels32(highlightColors);
			HighlightedTexture.Apply();
		}

		IntPtr stringPtr = TessBaseAPIGetUTF8Text(m_TessHandle);
		Marshal.FreeHGlobal(imagePtr);
		if (stringPtr.Equals(IntPtr.Zero))
			return null;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		string recognizedText = Marshal.PtrToStringAnsi(stringPtr);
#else
        string recognizedText = Marshal.PtrToStringAuto(stringPtr);
#endif
		TessBaseAPIClear(m_TessHandle);
		TessDeleteText(stringPtr);

		if (recognizedText == null) {
			return null;
		}

		string[] words = recognizedText.Split(new[] {' ', '\n'}, StringSplitOptions.RemoveEmptyEntries);
		int count = boxes.Length;
		(string, int)[] results = new (string, int)[count];
		for (int i = 0; i < count; i++) {
			results[i] = (words[i], confidence[i]);
		}
		return results;
	}

	public void Dispose() {
		if (!m_TessHandle.Equals(IntPtr.Zero)) {
			TessBaseAPIEnd(m_TessHandle);
			TessBaseAPIDelete(m_TessHandle);
			m_TessHandle = IntPtr.Zero;
		}
	}

	private static void DrawLines(IList<Color32> colors, int width, RectInt boundingRect, Color32 color, int thickness = 3) {
		int xMin = boundingRect.xMin;
		int xMax = boundingRect.xMax - 1;
		int yMin = boundingRect.y;
		int yMax = boundingRect.yMax - 1;
		
		for (int x = xMin; x <= xMax; x++) {
			for (int i = 0; i < thickness; i++) {
				colors[(yMin + i) * width + x] = color;
				colors[(yMax - i) * width + x] = color;
			}
		}
		for (int y = yMin; y <= yMax; y++) {
			for (int i = 0; i < thickness; i++) {
				colors[y * width + xMin + i] = color;
				colors[y * width + xMax - i] = color;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct BoxInfo {
		public int n;
		public int nAlloc;
		public int refCount;
		public IntPtr box;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Box {
		public int x;
		public int y;
		public int w;
		public int h;
		public int refCount;
	}

	#region DllImport

	private const string TesseractDllName = "tesseract";
	private const string LeptonicaDllName = "tesseract";

	[DllImport(TesseractDllName)]
	private static extern IntPtr TessVersion();

	[DllImport(TesseractDllName)]
	private static extern IntPtr TessBaseAPICreate();

	[DllImport(TesseractDllName)]
	private static extern int TessBaseAPIInit3(IntPtr handle, string dataPath, string language);

	[DllImport(TesseractDllName)]
	private static extern void TessBaseAPIDelete(IntPtr handle);

	[DllImport(TesseractDllName)]
	private static extern void TessBaseAPISetImage(IntPtr handle, IntPtr imagedata, int width, int height,
			int bytes_per_pixel, int bytes_per_line);

	[DllImport(TesseractDllName)]
	private static extern void TessBaseAPISetImage2(IntPtr handle, IntPtr pix);

	[DllImport(TesseractDllName)]
	private static extern int TessBaseAPIRecognize(IntPtr handle, IntPtr monitor);

	[DllImport(TesseractDllName)]
	private static extern IntPtr TessBaseAPIGetUTF8Text(IntPtr handle);

	[DllImport(TesseractDllName)]
	private static extern void TessDeleteText(IntPtr text);

	[DllImport(TesseractDllName)]
	private static extern void TessBaseAPIEnd(IntPtr handle);

	[DllImport(TesseractDllName)]
	private static extern void TessBaseAPIClear(IntPtr handle);

	[DllImport(TesseractDllName)]
	private static extern IntPtr TessBaseAPIGetWords(IntPtr handle, IntPtr pixa);

	[DllImport(TesseractDllName)]
	private static extern IntPtr TessBaseAPIAllWordConfidences(IntPtr handle);

	#endregion

}
