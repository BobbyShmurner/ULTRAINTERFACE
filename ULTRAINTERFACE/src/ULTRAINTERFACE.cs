using BepInEx.Logging;
using BepInEx.Configuration;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using System;
using System.Linq;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;

namespace ULTRAINTERFACE {
	public static class UI {
		public static List<Action<Scene>> OnSceneLoadActions { get; private set; } = new List<Action<Scene>>();
		public static ManualLogSource Log { get; private set; }

		static GameObject ScrollRectPrefab;
		static GameObject ScrollbarPrefab;
		static GameObject ButtonPrefab;
		static GameObject TogglePrefab;
		static GameObject TextPrefab;

		static bool IsUISetup = false;
		static bool HasInitalisedBefore = false;

		public static void RegisterOnSceneLoad(Action<Scene> action, bool executeNow = true) {
			OnSceneLoadActions.Add(action);
			
			if (executeNow) {
				if (!Init()) {
					Log.LogWarning("Failed to intialise UI, cannot execute newly registered OnSceneLoad Actions");
					return;
				}

				action(SceneManager.GetActiveScene());
			}
		}

		public static RectTransform CreateHorizontalLayoutGroup(Transform parent, float width = 500, float height = 20, int spacing = 20, TextAnchor childAlignment = TextAnchor.MiddleCenter) {
			HorizontalLayoutGroup layout = new GameObject("Horizontal Layout Group").AddComponent<HorizontalLayoutGroup>();

			RectTransform layoutRect = layout.GetComponent<RectTransform>();
			layoutRect.sizeDelta = new Vector2(width, height);
			layoutRect.SetParent(parent, false);

			layout.childAlignment = childAlignment;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = false;
			layout.childControlHeight = false;
			layout.spacing = spacing;

			ContentSizeFitter sizeFitter = layout.gameObject.AddComponent<ContentSizeFitter>();
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;

			return layoutRect;
		}

		public static RectTransform CreateVerticalLayoutGroup(Transform parent, float width = 20, float height = 500, int spacing = 20, TextAnchor childAlignment = TextAnchor.MiddleCenter) {
			VerticalLayoutGroup layout = new GameObject("Vertical Layout Group").AddComponent<VerticalLayoutGroup>();

			RectTransform layoutRect = layout.GetComponent<RectTransform>();
			layoutRect.sizeDelta = new Vector2(width, height);
			layoutRect.SetParent(parent, false);

			layout.childAlignment = childAlignment;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = false;
			layout.childControlWidth = false;
			layout.spacing = spacing;

			ContentSizeFitter sizeFitter = layout.gameObject.AddComponent<ContentSizeFitter>();
			sizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

			return layoutRect;
		}

		public static CustomScrollView CreateScrollView(Transform parent, float width = 620, float height = 520, int spacing = 20, TextAnchor childAlignment = TextAnchor.UpperCenter, string name = "Custom Scroll View") {
			if (!Init()) return null;

			RectTransform scrollViewRect = new GameObject(name, new Type[]{typeof(RectTransform)}).GetComponent<RectTransform>();
			scrollViewRect.gameObject.layer = 5;
			scrollViewRect.sizeDelta = new Vector2(width + 35, height);
			scrollViewRect.localPosition = Vector3.zero;
			scrollViewRect.SetParent(parent, false);

			HorizontalLayoutGroup scrollViewLayoutGroup = scrollViewRect.gameObject.AddComponent<HorizontalLayoutGroup>();
			scrollViewLayoutGroup.childControlWidth = false;
			scrollViewLayoutGroup.childControlHeight = false;
			scrollViewLayoutGroup.spacing = spacing;

			ScrollRect scrollRect = GameObject.Instantiate(ScrollRectPrefab, scrollViewRect).GetComponent<ScrollRect>();
			Scrollbar scrollbar = GameObject.Instantiate(ScrollbarPrefab, scrollViewRect).GetComponent<Scrollbar>();

			scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
			scrollRect.verticalScrollbar = scrollbar;
			scrollRect.gameObject.name = "Scroll Rect";

			RectTransform scrollbarRect = scrollbar.GetComponent<RectTransform>();
			scrollbarRect.sizeDelta = new Vector2(30, height);
			scrollbarRect.localPosition = Vector3.zero;
			scrollbarRect.gameObject.name = "Scrollbar";

			RectTransform scrollRectTrans = scrollRect.GetComponent<RectTransform>();
			scrollRectTrans.sizeDelta = new Vector2(width, height);
			scrollRectTrans.localPosition = Vector3.zero;

			RectTransform scrollRectContent = scrollRect.transform.GetChild(0).GetComponent<RectTransform>();
			scrollRectContent.sizeDelta = new Vector2(width, height + 160);
			scrollRectContent.localPosition = Vector3.zero;
			scrollRectContent.gameObject.name = "Content";

			VerticalLayoutGroup scrollRectContentLayout = scrollRectContent.gameObject.AddComponent<VerticalLayoutGroup>();
			scrollRectContentLayout.childAlignment = childAlignment;
			scrollRectContentLayout.childForceExpandHeight = false;
			scrollRectContentLayout.childForceExpandWidth = false;
			scrollRectContentLayout.childControlHeight = false;
			scrollRectContentLayout.childControlWidth = false;
			scrollRectContentLayout.spacing = 10;

			ContentSizeFitter scrollRectContentFitter = scrollRectContent.gameObject.AddComponent<ContentSizeFitter>();
			scrollRectContentFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

			for (; scrollRectContent.childCount > 0;) {
				GameObject.DestroyImmediate(scrollRectContent.GetChild(0).gameObject);
			}

			CustomScrollView scrollView = scrollViewRect.gameObject.AddComponent<CustomScrollView>();
			scrollView.Init(scrollRectContent, scrollRect, scrollbar);

			return scrollView;
		}

		public static void SetupLayoutElement(GameObject gameObject, float width, float height) {
			LayoutElement layoutElement = gameObject.AddComponent<LayoutElement>();
			layoutElement.minHeight = height;
			layoutElement.minWidth = width;
		}

		public static Button CreateButton(Transform parent, string text, UnityAction onClick, float width = 160, float height = 50, bool forceCaps = true) {
			Button button = CreateButton(parent, text, width, height, forceCaps);
			button.onClick.AddListener(onClick);

			return button;
		}

		public static Button CreateButton(Transform parent, string text, float width = 160, float height = 50, bool forceCaps = true) {
			if (!Init()) return null;
			if (forceCaps) text = text.ToUpper();

			GameObject buttonGO = GameObject.Instantiate(ButtonPrefab, parent);
			buttonGO.name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase($"{text.ToLower()} Button");
			buttonGO.AddComponent<UIComponent>();

			RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
			buttonRect.sizeDelta = new Vector2(width, height);
			buttonRect.anchoredPosition = Vector2.zero;

			Button button = buttonGO.GetComponent<Button>();
			button.onClick.RemoveAllListeners();

			// Disable all the persisten listeners
			for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++) {
				button.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
			}

			Text buttonText = buttonGO.GetComponentInChildren<Text>();
			buttonText.horizontalOverflow = HorizontalWrapMode.Overflow;
			buttonText.verticalOverflow = VerticalWrapMode.Overflow;
			buttonText.gameObject.name = "Text";
			buttonText.text = text;

			SetupLayoutElement(buttonGO, width, height);
			Options.SetupBackSelectOverride(buttonGO);

			return button;
		}

		public static CustomToggle CreateToggle(Transform parent, string label, UnityAction<bool> onValueChanged, float width = 170, float height = 20, float spacing = 75, int labelSize = 14, bool forceCaps = true) {
			CustomToggle toggle = CreateToggle(parent, label, width, height, spacing, labelSize, forceCaps);
			toggle.Toggle.onValueChanged.AddListener(onValueChanged);

			return toggle;
		}

		public static CustomToggle CreateToggle(Transform parent, string label, float width = 170, float height = 20, float spacing = 75, int labelSize = 14, bool forceCaps = true) {
			if (!Init()) return null;
			if (forceCaps) label = label.ToUpper();

			RectTransform containerRect = new GameObject(CultureInfo.InvariantCulture.TextInfo.ToTitleCase($"{label.ToLower()} Toggle")).AddComponent<RectTransform>(); 
			containerRect.sizeDelta = new Vector2(width, height);
			containerRect.pivot = new Vector2(0.5f, 0.5f);
			containerRect.SetParent(parent, false);

			GameObject toggleGO = GameObject.Instantiate(TogglePrefab, containerRect);
			toggleGO.name = "Toggle";

			RectTransform toggleRect = toggleGO.GetComponent<RectTransform>();
			toggleRect.anchoredPosition = new Vector2(spacing, 0);
			toggleRect.anchorMin = new Vector2(0.5f, 0.5f);
			toggleRect.anchorMax = new Vector2(0.5f, 0.5f);
			toggleRect.pivot = new Vector2(0.5f, 0.5f);

			Text labelText = CreateText(containerRect, label, labelSize, 160, 30, TextAnchor.MiddleLeft);

			RectTransform labelRect = labelText.rectTransform;
			labelRect.anchoredPosition = new Vector2(0, 0);
			labelRect.anchorMin = new Vector2(0, 0.5f);
			labelRect.anchorMax = new Vector2(0, 0.5f);
			labelRect.pivot = new Vector2(0, 0.5f);
			labelRect.SetAsFirstSibling();
			
			Toggle toggle = toggleGO.GetComponent<Toggle>();
			toggle.onValueChanged.RemoveAllListeners();

			// Disable all the persisten listeners
			for (int i = 0; i < toggle.onValueChanged.GetPersistentEventCount(); i++) {
				toggle.onValueChanged.SetPersistentListenerState(i, UnityEventCallState.Off);
			}

			SetupLayoutElement(containerRect.gameObject, width, height);
			Options.SetupBackSelectOverride(toggleGO);

			CustomToggle customToggle = containerRect.gameObject.AddComponent<CustomToggle>();
			customToggle.Init(labelText, toggle);

			return customToggle;
		}

		public static Text CreateText(Transform parent, string displayText = "New Text", int fontSize = 24, float width = 600, float height = 30, TextAnchor anchor = TextAnchor.MiddleCenter, bool forceCaps = true) {
			if (!Init()) return null;
			if (forceCaps) displayText = displayText.ToUpper();

			GameObject textGO = GameObject.Instantiate(TextPrefab, parent);
			textGO.AddComponent<UIComponent>();
			textGO.name = "Text";

			RectTransform textRect = textGO.GetComponent<RectTransform>();
			textRect.sizeDelta = new Vector2(width, height);
			textRect.anchoredPosition = Vector2.zero;

			Text text = textGO.GetComponent<Text>();
			text.fontSize = fontSize;
			text.text = displayText;
			text.alignment = anchor;

			text.horizontalOverflow = HorizontalWrapMode.Overflow;
			text.verticalOverflow = VerticalWrapMode.Overflow;

			SetupLayoutElement(textGO, width, height);

			return text;
		}

		public static void Unload() {
			SceneManager.sceneLoaded -= OnSceneLoad;
			OnSceneLoadActions.Clear();

			Options.Unload();

			foreach (UIComponent ui in Resources.FindObjectsOfTypeAll<UIComponent>()) {
				GameObject.Destroy(ui.gameObject);
			}
		}

		internal static bool Init() {
			if (!HasInitalisedBefore) {
				Log = new ManualLogSource("ULTRAINTERFACE");
				BepInEx.Logging.Logger.Sources.Add(Log);

				SceneManager.sceneLoaded += OnSceneLoad;

				HasInitalisedBefore = true;
			}

			IsUISetup = SetupUI();
			return IsUISetup;
		}

		static void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode) { 
			IsUISetup = false;
			IsUISetup = SetupUI();

			if (OnSceneLoadActions.Count <= 0) return;
			if (!IsUISetup) {
				Log.LogWarning("UI failed to initalised, not calling OnSceneLoad Actions");
				return;
			}

			Log.LogInfo("Calling OnSceneLoad Actions");

			foreach (Action<Scene> action in OnSceneLoadActions) {
				action(scene);
			}

			Log.LogInfo("Finished Calling OnSceneLoad Actions");
		}

		static bool SetupUI() {
			if (IsUISetup) return true;

			OptionsMenuToManager optionsMenuToManager = GameObject.FindObjectOfType<OptionsMenuToManager>();
			if (optionsMenuToManager == null) {
				Log.LogError("Failed to find the OptionsMenu, will attempt to setup UI on next scene load");
				return false;
			}

			Options.OptionsMenu = optionsMenuToManager.transform.Find("OptionsMenu").GetComponent<RectTransform>();
			
			ScrollRectPrefab = Options.OptionsMenu.Find("Gameplay Options").Find("Scroll Rect (1)").gameObject;
			ScrollbarPrefab = Options.OptionsMenu.Find("Gameplay Options").Find("Scrollbar (1)").gameObject;

			TextPrefab = Options.OptionsMenu.Find("Gameplay Options").Find("Text").gameObject;

			Transform possibleButtonPrefab = Options.OptionsMenu.Find("Gameplay");
			
			if (possibleButtonPrefab != null) ButtonPrefab = possibleButtonPrefab.gameObject;
			else ButtonPrefab = Options.OptionsMenu.Find("Options Scroll View").GetChild(0).GetChild(0).Find("Gameplay").gameObject;

			Options.GameplayOptionsContent = Options.OptionsMenu.Find("Gameplay Options").GetChild(1).GetChild(0);

			TogglePrefab = Options.GameplayOptionsContent.GetComponentInChildren<Toggle>().gameObject;

			Log.LogInfo($"Initalised UI");
			return true;
		}
	}

	public class CustomScrollView : UIComponent {
		public RectTransform Content { get; private set; }
		public ScrollRect ScrollRect { get; private set; }
		public Scrollbar Scrollbar { get; private set; }

		internal void Init(RectTransform content, ScrollRect scrollRect, Scrollbar scrollbar) {
			if (Content != null) {
				UI.Log.LogError($"Scroll View \"{gameObject.name}\" already initalised, returning...");
				return;
			}

			this.Content = content;
			this.ScrollRect = scrollRect;
			this.Scrollbar = scrollbar;
		}
	}

	public class CustomToggle : UIComponent {
		public Text Label { get; private set; }
		public Toggle Toggle { get; private set; }

		internal void Init(Text label, Toggle toggle) {
			if (Label != null) {
				UI.Log.LogError($"Toggle \"{gameObject.name}\" already initalised, returning...");
				return;
			}

			this.Label = label;
			this.Toggle = toggle;
		}
	}

	// This is purely used to tag UI created with this library
	public class UIComponent : MonoBehaviour {}
}