using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityEditor {
	public class EditorWaitForFrames {
		public int Frames { get; }
		
		public EditorWaitForFrames(int frames) {
			Frames = frames;
		}
	}
	
	public class EditorWaitForSeconds {
		public float Seconds { get; }
		
		public EditorWaitForSeconds(float seconds) {
			Seconds = seconds;
		}
	}
	
	public class EditorWaitForLateUpdate {}
	
	public class EditorCoroutine {
		public ScriptableObject Owner { get; }
		public IEnumerator Enumerator { get; }
		
		public object Current { get; private set; }
		public bool IsDone { get; private set; }
	
		public EditorCoroutine(IEnumerator enumerator, ScriptableObject owner) {
			Enumerator = enumerator;
			Owner = owner;
		}
	
		public bool MoveNext() {
			try {
				IsDone = !Enumerator.MoveNext();
				Current = Enumerator.Current;
				return !IsDone;
			} catch (Exception e) {
				Debug.LogError(e);
				IsDone = true;
				Current = null;
				return false;
			}
		}
	}
	
	[InitializeOnLoad]
	public class EditorCoroutineManager {
		private static readonly Dictionary<Type, List<EditorCoroutine>> s_CoroutinesDict = new Dictionary<Type, List<EditorCoroutine>>();
		private static List<EditorCoroutine> s_ListTemp = new List<EditorCoroutine>();
	
		private static readonly ConditionalWeakTable<EditorCoroutine, Func<bool>> s_MoveNextConditionDict = new ConditionalWeakTable<EditorCoroutine, Func<bool>>();

		public static bool Enable { get; set; } = true;
		public static int FrameCount { get; private set; }
		public static double TimeSinceStartup => EditorApplication.timeSinceStartup;
	
		static EditorCoroutineManager() {
			s_CoroutinesDict.Add(typeof(object), new List<EditorCoroutine>());
			s_CoroutinesDict.Add(typeof(EditorWaitForFrames), new List<EditorCoroutine>());
			s_CoroutinesDict.Add(typeof(EditorWaitForSeconds), new List<EditorCoroutine>());
			s_CoroutinesDict.Add(typeof(AsyncOperation), new List<EditorCoroutine>());
			s_CoroutinesDict.Add(typeof(EditorCoroutine), new List<EditorCoroutine>());
			s_CoroutinesDict.Add(typeof(EditorWaitForLateUpdate), new List<EditorCoroutine>());
			EditorApplication.update += () => {
				if (Enable) {
					FrameCount++;
					InvokeCoroutine(typeof(object));
					InvokeCoroutine(typeof(EditorWaitForFrames));
					InvokeCoroutine(typeof(EditorWaitForSeconds));
					InvokeCoroutine(typeof(AsyncOperation));
					InvokeCoroutine(typeof(EditorCoroutine));
					InvokeCoroutine(typeof(EditorWaitForLateUpdate));
				}
			};
		}
		
		private static void InvokeCoroutine(Type type) {
			List<EditorCoroutine> list = s_CoroutinesDict[type];
			s_CoroutinesDict[type] = s_ListTemp;
			foreach (var coroutine in list) {
				if (coroutine.Owner) {
					if (IsCoroutineWillNext(coroutine)) {
						if (coroutine.MoveNext()) {
							UpdateCoroutine(coroutine);
							AddCoroutine(coroutine);
						}
						// else {
							// 协程结束
						// }
					} else {
						AddCoroutine(coroutine);
					}
				}
			}
			list.Clear();
			s_ListTemp = list;
		}
	
		private static bool IsCoroutineWillNext(EditorCoroutine coroutine) {
			if (s_MoveNextConditionDict.TryGetValue(coroutine, out Func<bool> condition)) {
				if (condition()) {
					s_MoveNextConditionDict.Remove(coroutine);
					return true;
				}
				return false;
			}
			return true;
		}
	
		private static void UpdateCoroutine(EditorCoroutine coroutine) {
			var current = coroutine.Current;
			switch (current) {
				case EditorWaitForFrames waitForFrames:
					int doneFrame = FrameCount + waitForFrames.Frames;
					s_MoveNextConditionDict.Add(coroutine, () => FrameCount > doneFrame);
					break;
				case EditorWaitForSeconds waitForSeconds:
					double doneTime = TimeSinceStartup + waitForSeconds.Seconds;
					s_MoveNextConditionDict.Add(coroutine, () => TimeSinceStartup > doneTime);
					break;
				case AsyncOperation waitForOperation:
					s_MoveNextConditionDict.Add(coroutine, () => waitForOperation.isDone);
					break;
				case EditorCoroutine waitForCoroutine:
					s_MoveNextConditionDict.Add(coroutine, () => waitForCoroutine.IsDone);
					break;
			}
		}
	
		private static void AddCoroutine(EditorCoroutine coroutine) {
			var current = coroutine.Current;
			Type type = current?.GetType();
			if (type != null && s_CoroutinesDict.ContainsKey(type)) {
				s_CoroutinesDict[type].Add(coroutine);
				return;
			}
			s_CoroutinesDict[typeof(object)].Add(coroutine);
		}
	
		private static bool IsCoroutineRunning(EditorCoroutine coroutine) {
			foreach (var list in s_CoroutinesDict.Values) {
				foreach (var editorCoroutine in list) {
					if (editorCoroutine == coroutine) {
						return true;
					}
				}
			}
			return false;
		}
	
		private static EditorCoroutine GetCoroutine(IEnumerator enumerator) {
			foreach (var list in s_CoroutinesDict.Values) {
				foreach (var editorCoroutine in list) {
					if (editorCoroutine.Enumerator == enumerator) {
						return editorCoroutine;
					}
				}
			}
			return null;
		}
	
		private static void RemoveCoroutineBy(Func<Type, EditorCoroutine, bool> predicate) {
			foreach (var pair in s_CoroutinesDict) {
				Type type = pair.Key;
				List<EditorCoroutine> list = pair.Value;
				for (int i = list.Count - 1; i >= 0; --i) {
					if (predicate(type, list[i])) {
						list.RemoveAt(i);
					}
				}
			}
		}
	
		/// <summary>
		/// 开启一个Editor协程
		/// </summary>
		/// <param name="coroutine">已经存在但已停止的协程</param>
		/// <returns>传入的协程</returns>
		/// <exception cref="Exception">协程不存在或正在运行</exception>
		public static EditorCoroutine StartCoroutine(EditorCoroutine coroutine) {
			if (coroutine == null) {
				throw new Exception("Coroutine is null!");
			}
			if (IsRunning(coroutine)) {
				throw new Exception("Coroutine is already running!");
			}
		
			if (coroutine.MoveNext()) {
				UpdateCoroutine(coroutine);
				AddCoroutine(coroutine);
			}
			return coroutine;
		}
		/// <summary>
		/// 开启一个Editor协程
		/// </summary>
		/// <param name="enumerator">需要开启协程的迭代器对象</param>
		/// <param name="owner">协程的拥有者，可以通过它来批量结束协程</param>
		/// <returns>协程对象</returns>
		/// <exception cref="Exception">迭代器对象不存在或有对应的协程正在运行</exception>
		public static EditorCoroutine StartCoroutine(IEnumerator enumerator, ScriptableObject owner = null) {
			if (enumerator == null) {
				throw new Exception("Enumerator is null!");
			}
			if (IsRunning(enumerator)) {
				throw new Exception("Coroutine is already running!");
			}
		
			EditorCoroutine coroutine = new EditorCoroutine(enumerator, owner == null ? ScriptableObject.CreateInstance<ScriptableObject>() : owner);
			if (coroutine.MoveNext()) {
				UpdateCoroutine(coroutine);
				AddCoroutine(coroutine);
			}
			return coroutine;
		}
		
		/// <summary>
		/// 结束某个协程
		/// </summary>
		/// <param name="coroutine">需要结束的协程，可以传空或者传入已经结束的协程</param>
		public static void StopCoroutine(EditorCoroutine coroutine) {
			if (coroutine != null) {
				RemoveCoroutineBy((_, _coroutine) => _coroutine == coroutine);
			}
		}
		/// <summary>
		/// 结束某个协程
		/// </summary>
		/// <param name="enumerator">需要结束的迭代器对象，可以传空或者传入已经结束的协程</param>
		public static void StopCoroutine(IEnumerator enumerator) {
			if (enumerator != null) {
				RemoveCoroutineBy((_, _coroutine) => _coroutine.Enumerator == enumerator);
			}
		}
		
		/// <summary>
		/// 批量结束协程
		/// </summary>
		/// <param name="owner">协程的拥有者，在创建协程时绑定对应关系</param>
		public static void StopAllCoroutines(ScriptableObject owner) {
			RemoveCoroutineBy((_, _coroutine) => _coroutine.Owner == owner);
		}
	
		/// <summary>
		/// 判断协程是否正在运行
		/// </summary>
		/// <param name="coroutine">需要判断状态的协程</param>
		/// <returns>协程是否正在运行</returns>
		public static bool IsRunning(EditorCoroutine coroutine) {
			return IsCoroutineRunning(coroutine);
		}
		/// <summary>
		/// 判断迭代器对象对应的协程是否正在运行
		/// </summary>
		/// <param name="enumerator">需要判断状态的迭代器对象</param>
		/// <returns>代器对象对应的协程是否正在运行</returns>
		public static bool IsRunning(IEnumerator enumerator) {
			return GetCoroutine(enumerator) != null;
		}
		
		/// <summary>
		/// 无论协程当前等待的是什么，都跳至下一个yield，并保持运行或保持停止
		/// </summary>
		/// <param name="coroutine">需要跳至下一个yield的协程</param>
		public static void MoveNext(EditorCoroutine coroutine) {
			bool isRunning = IsRunning(coroutine);
			if (isRunning) {
				StopCoroutine(coroutine);
				StartCoroutine(coroutine);
			} else {
				StartCoroutine(coroutine);
				StopCoroutine(coroutine);
			}
		}
		/// <summary>
		/// 无论协程当前等待的是什么，都跳至下一个yield，并保持运行或保持停止
		/// </summary>
		/// <param name="enumerator">需要跳至下一个yield的迭代器对象</param>
		public static void MoveNext(IEnumerator enumerator) {
			bool isRunning = IsRunning(enumerator);
			if (isRunning) {
				StopCoroutine(enumerator);
				StartCoroutine(enumerator);
			} else {
				StartCoroutine(enumerator);
				StopCoroutine(enumerator);
			}
		}
		
	
		/// <summary>
		/// 直接将协程执行到最终状态
		/// </summary>
		/// <param name="coroutine">需要执行到最终状态的协程</param>
		/// <param name="maxSteps">由于协程可能是个死循环，所以用一个最大步数来限制执行步数</param>
		public static void Flush(EditorCoroutine coroutine, int maxSteps = int.MaxValue) {
			if (coroutine != null) {
				StopCoroutine(coroutine);
				bool hasNext = true;
				int steps = 0;
				while (hasNext && steps < maxSteps) {
					hasNext = !coroutine.MoveNext();
					++steps;
				}
				if (steps >= maxSteps) {
					Debug.LogWarning("Flush " + steps + " steps!");
				}
			}
		}
		/// <summary>
		/// 直接将迭代器对象执行到最终状态
		/// </summary>
		/// <param name="enumerator">需要执行到最终状态的迭代器对象</param>
		/// <param name="maxSteps">由于迭代器对象可能是个死循环，所以用一个最大步数来限制执行步数</param>
		public static void Flush(IEnumerator enumerator, int maxSteps = int.MaxValue) {
			if (enumerator != null) {
				StopCoroutine(enumerator);
				bool hasNext = true;
				int steps = 0;
				while (hasNext && steps < maxSteps) {
					hasNext = !enumerator.MoveNext();
					++steps;
				}
				if (steps >= maxSteps) {
					Debug.LogWarning("Flush " + steps + " steps!");
				}
			}
		}
	}
	
	public static class EditorCoroutineUtil {
		public static EditorCoroutine StartCoroutine(this ScriptableObject owner, IEnumerator enumerator) {
			return EditorCoroutineManager.StartCoroutine(enumerator, owner);
		}
		public static EditorCoroutine StartCoroutine(this ScriptableObject owner, EditorCoroutine coroutine) {
			return EditorCoroutineManager.StartCoroutine(coroutine);
		}
		
		public static void StopCoroutine(this ScriptableObject owner, IEnumerator enumerator) {
			EditorCoroutineManager.StopCoroutine(enumerator);
		}
		public static void StopCoroutine(this ScriptableObject owner, EditorCoroutine coroutine) {
			EditorCoroutineManager.StopCoroutine(coroutine);
		}
	
		public static void StopAllCoroutines(this ScriptableObject owner) {
			EditorCoroutineManager.StopAllCoroutines(owner);
		}
	
		public static void IsRunning(this ScriptableObject owner, IEnumerator enumerator) {
			EditorCoroutineManager.IsRunning(enumerator);
		}
		public static void IsRunning(this ScriptableObject owner, EditorCoroutine coroutine) {
			EditorCoroutineManager.IsRunning(coroutine);
		}
	
		public static void MoveNext(this ScriptableObject owner, IEnumerator enumerator) {
			EditorCoroutineManager.MoveNext(enumerator);
		}
		public static void MoveNext(this ScriptableObject owner, EditorCoroutine coroutine) {
			EditorCoroutineManager.MoveNext(coroutine);
		}
	
		public static void Flush(this ScriptableObject owner, IEnumerator enumerator, int maxSteps = 999) {
			EditorCoroutineManager.Flush(enumerator, maxSteps);
		}
		public static void Flush(this ScriptableObject owner, EditorCoroutine coroutine, int maxSteps = 999) {
			EditorCoroutineManager.Flush(coroutine, maxSteps);
		}
	
		public static EditorCoroutine Late(this ScriptableObject owner, Action callback) {
			return StartCoroutine(owner, IELate(callback));
		}
		private static IEnumerator IELate(Action callback) {
			if (callback != null) {
				yield return new EditorWaitForLateUpdate();
				callback.Invoke();
			}
		}
		
		public static EditorCoroutine Once(this ScriptableObject owner, float delay, Action callback) {
			return StartCoroutine(owner, IEOnce(delay, callback));
		}
		private static IEnumerator IEOnce(float delay, Action callback) {
			if (callback != null) {
				yield return new EditorWaitForSeconds(delay);
				callback.Invoke();
			}
		}
		
		public static EditorCoroutine FrameOnce(this ScriptableObject owner, int delay, Action callback) {
			return StartCoroutine(owner, IEFrameOnce(delay, callback));
		}
		private static IEnumerator IEFrameOnce(int delay, Action callback) {
			if (callback != null) {
				yield return new EditorWaitForFrames(delay);
				callback.Invoke();
			}
		}
	
		public static EditorCoroutine Wait(this ScriptableObject owner, Func<bool> loopUntil, Action callback) {
			return StartCoroutine(owner, IEWait(loopUntil, callback));
		}
		private static IEnumerator IEWait(Func<bool> loopUntil, Action callback) {
			if (loopUntil != null) {
				while (!loopUntil()) {
					yield return null;
				}
				callback?.Invoke();
			}
		}
	
		public static EditorCoroutine Wait(this ScriptableObject owner, AsyncOperation coroutine, Action callback) {
			return StartCoroutine(owner, IEWait(coroutine, callback));
		}
		private static IEnumerator IEWait(AsyncOperation coroutine, Action callback) {
			if (coroutine != null) {
				while (!coroutine.isDone) {
					yield return null;
				}
				callback?.Invoke();
			}
		}
	
		public static EditorCoroutine Wait(this ScriptableObject owner, EditorCoroutine coroutine, Action callback) {
			return StartCoroutine(owner, IEWait(coroutine, callback));
		}
		private static IEnumerator IEWait(EditorCoroutine coroutine, Action callback) {
			if (coroutine != null) {
				while (!coroutine.IsDone) {
					yield return null;
				}
				callback?.Invoke();
			}
		}
	
		public static EditorCoroutine Loop(this ScriptableObject owner, float interval, Func<bool> loopUntil) {
			return StartCoroutine(owner, IELoop(interval, loopUntil));
		}
		private static IEnumerator IELoop(float interval, Func<bool> loopUntil) {
			if (loopUntil != null) {
				EditorWaitForSeconds instruction = new EditorWaitForSeconds(interval);
				while (!loopUntil()) {
					yield return instruction;
				}
			}
		}
	
		public static EditorCoroutine FrameLoop(this ScriptableObject owner, int interval, Func<bool> loopUntil) {
			return StartCoroutine(owner, IEFrameLoop(interval, loopUntil));
		}
		private static IEnumerator IEFrameLoop(int interval, Func<bool> loopUntil) {
			if (loopUntil != null) {
				EditorWaitForFrames instruction = new EditorWaitForFrames(interval);
				while (!loopUntil()) {
					yield return instruction;
				}
			}
		}
	}
}
