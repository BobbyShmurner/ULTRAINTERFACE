using HarmonyLib;

using BepInEx.Logging;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ULTRAINTERFACE {
	public static class UI {
		public static List<Action<Scene>> OnSceneLoadActions { get; private set; } = new List<Action<Scene>>();
		public static ManualLogSource Log { get; private set; }
		public static Canvas Canvas;

		internal static Harmony HarmonyInstance;

		internal static GameObject ScrollViewPrefab;
		internal static GameObject ButtonPrefab;
		internal static GameObject TogglePrefab;
		internal static GameObject SliderPrefab;
		internal static GameObject PanelPrefab;
		internal static GameObject ModalPrefab;
		internal static GameObject TextPrefab;

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

		public static RectTransform CreateHorizontalLayoutGroup(Transform parent, int spacing = 20, TextAnchor childAlignment = TextAnchor.MiddleCenter, RectOffset padding = null) {
			if (padding == null) padding = new RectOffset(0, 0, 0, 0);
			
			HorizontalLayoutGroup layout = new GameObject("Horizontal Layout Group").AddComponent<HorizontalLayoutGroup>();
			layout.gameObject.AddComponent<UIComponent>();

			RectTransform layoutRect = layout.GetComponent<RectTransform>();
			layoutRect.SetParent(parent, false);

			layout.childAlignment = childAlignment;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = false;
			layout.childControlHeight = false;
			layout.padding = padding;
			layout.spacing = spacing;

			ContentSizeFitter sizeFitter = layout.gameObject.AddComponent<ContentSizeFitter>();
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
			sizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

			return layoutRect;
		}

		public static RectTransform CreateVerticalLayoutGroup(Transform parent, int spacing = 20, TextAnchor childAlignment = TextAnchor.MiddleCenter, RectOffset padding = null) {
			if (padding == null) padding = new RectOffset(0, 0, 0, 0);
			
			VerticalLayoutGroup layout = new GameObject("Vertical Layout Group").AddComponent<VerticalLayoutGroup>();
			layout.gameObject.AddComponent<UIComponent>();

			RectTransform layoutRect = layout.GetComponent<RectTransform>();
			layoutRect.SetParent(parent, false);

			layout.childAlignment = childAlignment;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = false;
			layout.childControlWidth = false;
			layout.padding = padding;
			layout.spacing = spacing;

			ContentSizeFitter sizeFitter = layout.gameObject.AddComponent<ContentSizeFitter>();
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
			sizeFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

			return layoutRect;
		}

		public static CustomScrollView CreateScrollView(Transform parent, float width = 620, float height = 520, int spacing = 10, TextAnchor childAlignment = TextAnchor.UpperCenter, string name = "Custom Scroll View") {
			if (!Init()) return null;

			RectTransform scrollViewRect = GameObject.Instantiate(ScrollViewPrefab).GetComponent<RectTransform>();
			scrollViewRect.SetParent(parent, false);
			scrollViewRect.gameObject.name = name;

			VerticalLayoutGroup scrollRectContentLayout = scrollViewRect.GetComponentInChildren<VerticalLayoutGroup>();
			scrollRectContentLayout.childAlignment = childAlignment;
			scrollRectContentLayout.spacing = spacing;

			CustomScrollView customScrollView = scrollViewRect.gameObject.AddComponent<CustomScrollView>();
			customScrollView.Init(scrollViewRect.GetComponentInChildren<ScrollRect>(), scrollViewRect.GetComponentInChildren<Scrollbar>(), scrollViewRect.GetComponentInChildren<VerticalLayoutGroup>().transform);
			customScrollView.SetDimensions(width + 45, height);

			return customScrollView;
		}

		public static CustomButton CreateButton(Transform parent, string text, UnityAction onClick, int fontSize = 14, float width = 160, float height = 50, bool forceCaps = true) {
			CustomButton button = CreateButton(parent, text, fontSize, width, height, forceCaps);
			button.Button.onClick.AddListener(onClick);

			return button;
		}

		public static CustomButton CreateButton(Transform parent, string text, int fontSize = 14, float width = 160, float height = 50, bool forceCaps = true) {
			if (!Init()) return null;

			GameObject buttonGO = GameObject.Instantiate(ButtonPrefab, parent);
			buttonGO.name = (text == "" ? "Custom Button" : $"{text.ToLower()} Button");

			Button button = buttonGO.GetComponent<Button>();

			Text buttonText = buttonGO.GetComponentInChildren<Text>();
			buttonText.fontSize = fontSize;
			buttonText.SetText(text, forceCaps);


			CustomButton customButton = buttonGO.AddComponent<CustomButton>();
			customButton.SetupDefaultBackSelectOverride();
			customButton.SetDimensions(width, height);
			customButton.Init(button, buttonText);

			return customButton;
		}

		public static CustomModal CreateModal(Action<CustomModal> onCreate, bool showOutline = true, bool blockRaycasts = true, float backgroundOpacity = 1, float raycastScreenOpacity = 0.5f, string name = "Custom Modal") {
			CustomModal modal = CreateModal(showOutline, blockRaycasts, backgroundOpacity, raycastScreenOpacity, name);
			onCreate(modal);

			return modal;
		}

		public static CustomModal CreateModal(bool showOutline = true, bool blockRaycasts = true, float backgroundOpacity = 1, float raycastScreenOpacity = 0.5f, string name = "Custom Modal") {
			if (!Init()) return null;

			GameObject modalGO = GameObject.Instantiate(ModalPrefab, UI.Canvas.transform);
			modalGO.name = name;

			RectTransform content = (RectTransform)modalGO.transform.GetChild(1).GetChild(1);
			Image outline = modalGO.transform.GetChild(1).GetChild(0).GetComponent<Image>();
			Image raycastScreen = modalGO.transform.GetChild(0).GetComponent<Image>();
			Image background = modalGO.transform.GetChild(1).GetComponent<Image>();

			CopyRectSize copyRectSize = content.parent.gameObject.AddComponent<CopyRectSize>();
			copyRectSize.TargetRect = content;

			CustomModal customModal = modalGO.AddComponent<CustomModal>();
			customModal.Init(content, raycastScreen, background, outline, modalGO.GetComponent<GamepadObjectSelector>(), modalGO.GetComponent<MenuEsc>());

			customModal.SetRaycastScreenOpacity(raycastScreenOpacity);
			customModal.SetBackgroundOpacity(backgroundOpacity);
			customModal.SetRaycastScreenActive(blockRaycasts);
			customModal.SetOutlineActive(showOutline);

			customModal.OnShown.Add((modal) => {
				modal.SetFirstSelectable();
				modal.GamepadObjectSelector.Activate();
				modal.GamepadObjectSelector.SetTop();
			});

			return customModal;
		}

		public static CustomToggle CreateToggle(Transform parent, string label, UnityAction<bool> onValueChanged) {
			return CreateToggle(parent, label, onValueChanged, 200);
		}

		public static CustomToggle CreateToggle(Transform parent, string label) {
			return CreateToggle(parent, label, 200);
		}

		public static CustomToggle CreateToggle(Transform parent, string label, UnityAction<bool> onValueChanged, float width) {
			return CreateToggle(parent, label, onValueChanged, width, 30);
		}

		public static CustomToggle CreateToggle(Transform parent, string label, float width) {
			return CreateToggle(parent, label, width, 30);
		}

		public static CustomToggle CreateToggle(Transform parent, string label, UnityAction<bool> onValueChanged, float width, float height) {
			return CreateToggle(parent, label, onValueChanged, width, height, 14);
		}

		public static CustomToggle CreateToggle(Transform parent, string label, float width, float height) {
			return CreateToggle(parent, label, width, height, 14);
		}

		public static CustomToggle CreateToggle(Transform parent, string label, UnityAction<bool> onValueChanged, float width, float height, int labelFontSize) {
			return CreateToggle(parent, label, onValueChanged, width, height, labelFontSize, 20);
		}

		public static CustomToggle CreateToggle(Transform parent, string label, float width, float height, int labelFontSize) {
			return CreateToggle(parent, label, width, height, labelFontSize, 20);
		}

		public static CustomToggle CreateToggle(Transform parent, string label, UnityAction<bool> onValueChanged, float width, float height, int labelFontSize, float spacing) {
			return CreateToggle(parent, label, onValueChanged, width, height, labelFontSize, spacing, true);
		}

		public static CustomToggle CreateToggle(Transform parent, string label, float width, float height, int labelFontSize, float spacing) {
			return CreateToggle(parent, label, width, height, labelFontSize, spacing, true);
		}

		public static CustomToggle CreateToggle(Transform parent, string label, UnityAction<bool> onValueChanged, float width, float height, int labelFontSize, float spacing, bool forceCaps) {
			CustomToggle toggle = CreateToggle(parent, label, width, height, labelFontSize, spacing, forceCaps);
			toggle.Toggle.onValueChanged.AddListener(onValueChanged);

			return toggle;
		}

		public static CustomToggle CreateToggle(Transform parent, string label, float width, float height, int labelFontSize, float spacing, bool forceCaps) {
			if (!Init()) return null;

			GameObject toggleInstance = GameObject.Instantiate(TogglePrefab);
			toggleInstance.name = (label == "" ? "Custom Toggle" : $"{label} Toggle");
			toggleInstance.transform.SetParent(parent, false);

			Toggle toggle = toggleInstance.GetComponent<Toggle>();

			HorizontalLayoutGroup toggleLayout = toggleInstance.GetComponent<HorizontalLayoutGroup>();
			toggleLayout.spacing = spacing;

			Text labelText = toggleInstance.GetComponentInChildren<Text>();
			labelText.GetComponent<RectTransform>().sizeDelta = new Vector2(width - spacing - (height - 10), height);
			labelText.SetText(label, forceCaps);
			labelText.fontSize = labelFontSize;

			Image background = toggleInstance.GetComponentInChildren<Image>();
			background.GetComponent<RectTransform>().sizeDelta = new Vector2(height - 10, height - 10);

			Image checkmark = background.transform.GetChild(0).GetComponent<Image>();

			CustomToggle customToggle = toggleInstance.AddComponent<CustomToggle>();
			customToggle.SetupDefaultBackSelectOverride();
			customToggle.Init(toggle, labelText, background, checkmark);

			return customToggle;
		}

		public static CustomText CreateText(Transform parent, string displayText = "New Text", int fontSize = 24, float width = 160, float height = 30, TextAnchor anchor = TextAnchor.MiddleCenter, bool forceCaps = true) {
			if (!Init()) return null;

			GameObject textGO = GameObject.Instantiate(TextPrefab, parent);
			textGO.name = "Custom Text";

			Text text = textGO.GetComponent<Text>();
			text.SetText(displayText, forceCaps);
			text.fontSize = fontSize;
			text.alignment = anchor;

			CustomText customText = textGO.AddComponent<CustomText>();
			customText.SetDimensions(width, height);
			customText.Init(text);

			return customText;
		}

		public static CustomSlider CreateSlider(Transform parent, string label, SliderSettings settings, UnityAction<float> onValueChanged, int labelFontSize = 14, int valueFontSize = 24, bool forceCaps = true) {
			CustomSlider slider = CreateSlider(parent, label, settings, labelFontSize, valueFontSize, forceCaps);
			slider.Slider.onValueChanged.AddListener(onValueChanged);

			return slider;
		}

		public static CustomSlider CreateSlider(Transform parent, string label, SliderSettings settings, int labelFontSize = 14, int valueFontSize = 24, bool forceCaps = true) {
			if (!Init()) return null;
			if (forceCaps) {
				settings.Suffix = settings.Suffix.ToUpper();
				settings.IfMin = settings.IfMin.ToUpper();
				settings.IfMax = settings.IfMax.ToUpper();
			}

			GameObject sliderInstance = GameObject.Instantiate(SliderPrefab, parent);
			sliderInstance.name = (label == "" ? "Custom Slider" : $"{label} Slider");

			Slider slider = sliderInstance.GetComponentInChildren<Slider>();
			slider.wholeNumbers = (settings.DecimalType == DecimalType.NoDecimals);

			OptionsMenuToManager[] omtms = sliderInstance.GetComponentsInParent<OptionsMenuToManager>(true);
			OptionsMenuToManager omtm;

			if (omtms.Length > 0) omtm = omtms[0];
			else omtm = Resources.FindObjectsOfTypeAll<OptionsMenuToManager>()[0];

			Button selectableButton = sliderInstance.GetComponent<Button>();
			selectableButton.onClick.AddListener(() => { omtm.SetSelected(slider); });

			Text labelText = sliderInstance.transform.GetChild(0).GetComponent<Text>();
			labelText.SetText(label, forceCaps);
			labelText.fontSize = labelFontSize;

			Text valueText = sliderInstance.transform.GetChild(2).GetComponent<Text>();
			valueText.fontSize = valueFontSize;

			CustomSlider customSlider = sliderInstance.AddComponent<CustomSlider>();
			customSlider.Init(slider, labelText, valueText, settings);

			return customSlider;
		}

		public static CustomPanel CreatePanel(Transform parent, TextAnchor childAlignment = TextAnchor.MiddleLeft, float spacing = 20, float opacity = 1, RectOffset padding = null, string name = "Custom Panel") {
			if (!Init()) return null;
			if (padding == null) padding = new RectOffset(20, 20, 15, 15);

			RectTransform panel = GameObject.Instantiate(UI.PanelPrefab, parent).GetComponent<RectTransform>();
			panel.gameObject.name = name;

			VerticalLayoutGroup layoutGroup = panel.GetComponent<VerticalLayoutGroup>();
			layoutGroup.childAlignment = childAlignment;
			layoutGroup.spacing = spacing;

			Image image = panel.GetComponent<Image>();
			image.color = new Color(0, 0, 0, opacity);

			CustomPanel customPanel = panel.gameObject.AddComponent<CustomPanel>();
			customPanel.Init(image);

			return customPanel;
		}

		public static CustomImage CreateImage(Transform parent, Sprite sprite, float width = -1, float height = -1, string name = "Custom Image") {
			if (!Init()) return null;

			RectTransform imageRect = new GameObject(name).AddComponent<RectTransform>();
			imageRect.SetParent(parent, false);

			Image image = imageRect.gameObject.AddComponent<Image>();
			
			CustomImage customImage = imageRect.gameObject.AddComponent<CustomImage>();
			customImage.Init(image);
			customImage.SetSprite(sprite, false);

			if (sprite != null) {
				if (width == -1) width = sprite.texture.width;
				if (height == -1) height = sprite.texture.height;

				customImage.SetDimensions(width, height);
			}

			return customImage;
		}

		public static void Unload() {
			SceneManager.sceneLoaded -= OnSceneLoad;
			OnSceneLoadActions.Clear();

			Options.Unload();
			HarmonyInstance.UnpatchSelf();

			foreach (ModObject ui in Resources.FindObjectsOfTypeAll<ModObject>()) {
				Attribute[] attrs = Attribute.GetCustomAttributes(ui.GetType());
				bool shouldDestroyGameObject = !attrs.ToList().Exists((attr) => attr is DontDestroyGameObjectOnUnloadAttribute);

				if (shouldDestroyGameObject) GameObject.Destroy(ui.gameObject);
				else GameObject.Destroy(ui);
			}
		}

		internal static bool Init() {
			if (!HasInitalisedBefore) {
				Log = new ManualLogSource("ULTRAINTERFACE");
				BepInEx.Logging.Logger.Sources.Add(Log);

				SceneManager.sceneLoaded += OnSceneLoad;

				HasInitalisedBefore = true;

				AssetLoader.CurrentAssembly = Assembly.GetExecutingAssembly();
				string resourceName = AssetLoader.CurrentAssembly.GetManifestResourceNames().First((name) => name.EndsWith("resources.ultrainterface"));
				Stream resourceStream = AssetLoader.CurrentAssembly.GetManifestResourceStream(resourceName);
				var bundle = AssetBundle.LoadFromStream(resourceStream);

				AssetLoader.ResourceNamespace = resourceName.Replace(".resources.ultrainterface", "");

				ScrollViewPrefab = bundle.LoadAsset<GameObject>("ScrollViewPrefab");
				ButtonPrefab = bundle.LoadAsset<GameObject>("ButtonPrefab");
				TogglePrefab = bundle.LoadAsset<GameObject>("TogglePrefab");
				SliderPrefab = bundle.LoadAsset<GameObject>("SliderPrefab");
				PanelPrefab = bundle.LoadAsset<GameObject>("PanelPrefab");
				ModalPrefab = bundle.LoadAsset<GameObject>("ModalPrefab");
				TextPrefab = bundle.LoadAsset<GameObject>("TextPrefab");

				HarmonyInstance = new Harmony($"ULTRAINTERFACE-{AssetLoader.CurrentAssembly.GetName().Name}");
				HarmonyInstance.PatchAll(typeof(SliderValueToTextPatch));

				resourceStream.Close();
				bundle.Unload(false);
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

			try {
				Camera.main.gameObject.AddComponent<CoroManager>();
				Canvas = GameObject.FindObjectOfType<OptionsMenuToManager>().GetComponent<Canvas>();
			} catch(Exception e) {
				Log.LogError($"Failed to Initalise UI!\n\n{e}");
				return false;
			}

			Log.LogInfo($"Initalised UI");
			return true;
		}
	}
}