using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace ULTRAINTERFACE {
	[DontDestroyGameObjectOnUnload]
	public class CoroManager : ModObject {
		public static CoroManager Instance { get; private set; } = null;

		List<Action> updateActions = new List<Action>();
		List<Action> lateUpdateActions = new List<Action>();
		List<Action> preRenderActions = new List<Action>();

		public static void InvokeNextLateUpdate(Action action) {
			Instance.lateUpdateActions.Add(action);
		}

		public static void InvokeNextUpdate(Action action) {
			Instance.updateActions.Add(action);
		}

		public static void InvokeBeforeRender(Action action) {
			Instance.preRenderActions.Add(action);
		}

		public static void InvokeNextFrame(Action action) {
			Instance.StartCoroutine(InvokeNextFrameCoro(action));
		}

		public static IEnumerator InvokeNextFrameCoro(Action action) {
			yield return null;
			action();
		}

		void Awake() {
			if (Instance != null) {
				Destroy(this);
				return;
			}

			Instance = this;
		}

		void LateUpdate() {
			foreach (Action action in lateUpdateActions) {
				action();
			}

			lateUpdateActions.Clear();
		}

		void OnPreCull() {
			foreach (Action action in preRenderActions) {
				action();
			}

			preRenderActions.Clear();
		}

		void Update() {
			foreach (Action action in updateActions) {
				action();
			}

			updateActions.Clear();
		}
	}
}