using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;

namespace ULTRAINTERFACE {
	public static class Options {
		public static CustomScrollView OptionsScroll { get; private set; }

		// It's just easier to have SetupUI grab these
		public static RectTransform OptionsMenu { get; internal set; } 
		public static Transform GameplayOptionsContent { get; internal set; }

		public static void CreateOptionsMenu(string title, Action<OptionsMenu> createAction, string buttonText = "", bool forceCaps = true) {
			UI.RegisterOnSceneLoad((scene) => {
				OptionsMenu optionsMenu = CreateOptionsMenu_Internal(title, buttonText, forceCaps);

				createAction(optionsMenu);
				optionsMenu.Rebuild();
			});
		}

		public static RectTransform CreateOptionsPanel(Transform parent, TextAnchor childAlignment = TextAnchor.MiddleLeft, float spacing = 20) {
			if (!Init()) return null;

			RectTransform panel = GameObject.Instantiate(UI.PanelPrefab, parent).GetComponent<RectTransform>();
			panel.gameObject.name = "Options Panel";

			panel.anchorMin = new Vector2(0, 0.5f);
			panel.anchorMax = new Vector2(1, 0.5f);

			VerticalLayoutGroup layoutGroup = panel.GetComponent<VerticalLayoutGroup>();
			layoutGroup.childAlignment = childAlignment;
			layoutGroup.spacing = spacing;

			ContentSizeFitter sizeFitter = panel.GetComponent<ContentSizeFitter>();
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

			return panel;
		}

		public static void SetupDefaultBackSelectOverride(UIComponent uiComponent) {
			OptionsMenu[] menus = uiComponent.GetComponentsInParent<OptionsMenu>(true);
			if (menus.Length == 0) return;

			uiComponent.SetBackSelectOverride(menus[0].OptionsButton.Button);
		}

		static OptionsMenu CreateOptionsMenu_Internal(string title, string buttonText, bool forceCaps) {
			if (!Init()) return null;
			if (buttonText == "") buttonText = title;

			CustomScrollView scrollView = UI.CreateScrollView(OptionsMenu, 620, 520, 20, TextAnchor.MiddleCenter, CultureInfo.InvariantCulture.TextInfo.ToTitleCase(title.ToLower()) + " Options");
			CustomButton optionsButton = UI.CreateButton(OptionsScroll.Content, title, 14, 160, 50);

			RectTransform scrollViewRect = scrollView.GetComponent<RectTransform>();
			scrollViewRect.anchoredPosition = new Vector2(12.5f, -50);
			scrollViewRect.gameObject.SetActive(false);

			CustomText titleText = UI.CreateText(scrollView.GetComponent<RectTransform>(), $"--{title}--", 24, 620, forceCaps: forceCaps);
			titleText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 320);
			titleText.transform.SetAsFirstSibling();

			VerticalLayoutGroup contentLayout = scrollView.Content.GetComponent<VerticalLayoutGroup>();
			contentLayout.childForceExpandWidth = true;
			contentLayout.childControlWidth = true;

			// Disable this menu when the other buttons are clicked
			for (int i = 0; i < OptionsScroll.Content.transform.childCount; i++) {
				Button button = OptionsScroll.Content.transform.GetChild(i).GetComponent<Button>();
				if (button == null || button == optionsButton.Button) continue;
				
				button.onClick.AddListener(() => { scrollView.gameObject.SetActive(false); });
			}

			// Disable other menus when this button is clicked
			for (int i = 0; i < OptionsMenu.childCount; i++) {
				Transform child = OptionsMenu.GetChild(i);
				if (!child.name.EndsWith(" Options")) continue;

				optionsButton.Button.onClick.AddListener(() => { child.gameObject.SetActive(child == scrollView.transform); });
			}

			GamepadObjectSelector scrollViewGOS = scrollView.gameObject.AddComponent<GamepadObjectSelector>();
			typeof(GamepadObjectSelector).GetField("selectOnEnable", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scrollViewGOS, false);
			typeof(GamepadObjectSelector).GetField("dontMarkTop", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scrollViewGOS, true);

			BackSelectEvent scrollViewBSE = scrollView.gameObject.AddComponent<BackSelectEvent>();

			UnityEvent onBack = new UnityEvent();
			onBack.AddListener(() => { scrollViewGOS.PopTop(); } );
			typeof(BackSelectEvent).GetField("m_OnBack", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scrollViewBSE, onBack);

			optionsButton.Button.onClick.AddListener(() => { scrollViewGOS.Activate(); });
			optionsButton.Button.onClick.AddListener(() => { scrollViewGOS.SetTop(); });

			optionsButton.Text.SetText(buttonText, forceCaps);

			OptionsMenu optionsMenu = scrollView.gameObject.AddComponent<OptionsMenu>();
			optionsMenu.Init(scrollView, optionsButton, titleText);

			optionsMenu.LateCreate.Add((menu) => {
				Selectable firstSelectable = menu.ScrollView.Content.GetComponentInChildren<Selectable>();
				typeof(GamepadObjectSelector).GetField("mainTarget", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(menu.ScrollView.GetComponent<GamepadObjectSelector>(), firstSelectable ? firstSelectable.gameObject : null);
			});

			optionsMenu.FirstShown.Add((menu) => {
				menu.Rebuild();

				foreach (ScrollRect scrollRect in menu.GetComponentsInChildren<ScrollRect>()) {
					scrollRect.ScrollToTop();
				}
			});

			UpdateOptionsScrollNavigation();

			return optionsMenu;
		}

		public static void UpdateOptionsScrollNavigation() {
			List<Button> buttons = OptionsScroll.Content.GetComponentsInChildren<Button>().ToList();
			Button backButton = OptionsMenu.Find("Back").GetComponent<Button>();

			for (int i = 0; i < buttons.Count; i++) {
				Button button = buttons[i];

				Navigation nav = new Navigation();
 				nav.mode = Navigation.Mode.Explicit;

				if (i > 0) {
					nav.selectOnUp = buttons[i - 1];
				} else {
					nav.selectOnUp = backButton;
				}
				if (i < buttons.Count - 1) {
					nav.selectOnDown = buttons[i + 1];
				} else {
					nav.selectOnDown = backButton;
				}

				button.navigation = nav;
			}

			Navigation backNav = new Navigation();
			backNav.mode = Navigation.Mode.Explicit;

			backNav.selectOnUp = buttons[buttons.Count - 1];
			backNav.selectOnDown = buttons[0];

			backButton.navigation = backNav;
		}

		internal static void Unload() {
			if (OptionsScroll != null) {
				while (OptionsScroll.Content.childCount > 0) {
					Transform buttonTrans = OptionsScroll.Content.GetChild(0);
					buttonTrans.SetParent(OptionsMenu, false);

					Button button = buttonTrans.GetComponent<Button>();
					if (button) button.onClick.RemoveAllListeners();
				}
				GameObject.Destroy(OptionsScroll.gameObject);
			}
		}

		internal static bool Init() {
			UI.Log.LogInfo("Initing Options");
			if (!UI.Init()) return false;
			if (OptionsScroll != null) return true;

			UI.Log.LogInfo($"OptionsScroll: {OptionsScroll}");
			UI.Log.LogInfo($"OptionsMenu: {OptionsMenu}");

			// If "Options Scroll View" exists then another mod has set it up already
			Transform existingMenuTrans = OptionsMenu.Find("Options Scroll View");
			if (!existingMenuTrans) {
				OptionsScroll = UI.CreateScrollView(OptionsMenu, 185, 470, 10, TextAnchor.UpperCenter, "Options Scroll View");
				RectTransform optionsScrollRect = OptionsScroll.GetComponent<RectTransform>();
				optionsScrollRect.anchorMin = new Vector2(0, 0.5f);
				optionsScrollRect.anchorMax = new Vector2(0, 0.5f);
				optionsScrollRect.pivot = new Vector2(0, 0.5f);
				optionsScrollRect.anchoredPosition = new Vector3(17.5f, 0);
				optionsScrollRect.SetAsFirstSibling();

				// Move Buttons to the scroll view
				MoveOptionToOptionScroll("Gameplay");
				MoveOptionToOptionScroll("Controls");
				MoveOptionToOptionScroll("Video");
				MoveOptionToOptionScroll("Audio");
				MoveOptionToOptionScroll("HUD");
				MoveOptionToOptionScroll("Assist");
				MoveOptionToOptionScroll("Colors");
				MoveOptionToOptionScroll("Saves");
			} else {
				OptionsScroll = existingMenuTrans.GetComponent<CustomScrollView>();

				if (OptionsScroll == null) {
					OptionsScroll = existingMenuTrans.gameObject.AddComponent<CustomScrollView>();
				}
			}

			GameplayOptionsContent = OptionsMenu.Find("Gameplay Options").GetChild(1).GetChild(0);

			return true;
		}

		internal static void MoveOptionToOptionScroll(string optionName) {
			RectTransform option = OptionsMenu.Find(optionName).GetComponent<RectTransform>();
			option.SetParent(OptionsScroll.Content, false);
			option.anchoredPosition = Vector2.zero;
		}
	}

	public class OptionsMenu : UIComponent {
		public List<Action<OptionsMenu>> LateCreate { get; private set; } = new List<Action<OptionsMenu>>();
		public List<Action<OptionsMenu>> FirstShown { get; private set; } = new List<Action<OptionsMenu>>();
		public List<Action<OptionsMenu>> OnShown { get; private set; } = new List<Action<OptionsMenu>>();

		public CustomScrollView ScrollView { get; private set; }
		public CustomButton OptionsButton { get; private set; }
		public CustomText Title { get; private set; }

		public RectTransform Content { get { return ScrollView.Content; } }

		public bool IsInitalised { get; private set; } = false;
		public bool HasBeenShown { get; private set; } = false;

		float previousHeight;

		public RectTransform AddOptionsPanel(TextAnchor anchor = TextAnchor.MiddleLeft) {
			return Options.CreateOptionsPanel(Content, anchor);
		}

		public CustomText AddHeader(string text, int size = 24, float width = 600, float height = 50, TextAnchor anchor = TextAnchor.MiddleCenter, bool forceCaps = true) {
			return UI.CreateText(Content, text, size, width, height, anchor, forceCaps);
		}

		public CustomButton AddButton(string text, UnityAction onClick, int fontSize = 24, float width = 600, float height = 60, bool forceCaps = true) {
			return UI.CreateButton(Content, text, onClick, fontSize, width, height, forceCaps);
		}

		public CustomButton AddButton(string text, int fontSize = 24, float width = 600, float height = 60, bool forceCaps = true) {
			return UI.CreateButton(Content, text, fontSize, width, height, forceCaps);
		}

		public void Rebuild() {
			LayoutRebuilder.ForceRebuildLayoutImmediate(Content);

			Content.anchoredPosition = new Vector2(Content.anchoredPosition.x, Content.anchoredPosition.y - (Content.sizeDelta.y - previousHeight) / 2);

			CoroManager.InvokeBeforeRender(() => {
				Content.anchoredPosition = new Vector2(Content.anchoredPosition.x, Content.anchoredPosition.y - (Content.sizeDelta.y - previousHeight) / 2);
			});

			UpdateNavigation();
		}

		void LateUpdate() {
			previousHeight = Content.sizeDelta.y;
		}

		internal void Init(CustomScrollView scrollView, CustomButton optionsButton, CustomText title) {
			if (IsInitalised) {
				UI.Log.LogError($"Options Menu \"{gameObject.name}\" already initalised, returning...");
				return;
			}

			this.ScrollView = scrollView;
			this.OptionsButton = optionsButton;
			this.Title = title;

			this.HasBeenShown = false;

			CoroManager.InvokeNextFrame(() => {
				foreach (Action<OptionsMenu> action in LateCreate) {
					action(this);
				}
				
				Rebuild();
			});

			IsInitalised = true;
		}

		void UpdateNavigation() {
			Selectable[] selectables = ScrollView.Content.GetComponentsInChildren<Selectable>(false);

			for (int i = 0; i < selectables.Length; i++) {
				Selectable selectable = selectables[i];

				Navigation nav = new Navigation();
 				nav.mode = Navigation.Mode.Explicit;

				if (i > 0) {
					nav.selectOnUp = selectables[i - 1];
				} else {
					nav.selectOnUp = selectables[selectables.Length - 1];
				}
				if (i < selectables.Length - 1) {
					nav.selectOnDown = selectables[i + 1];
				} else {
					nav.selectOnDown = selectables[0];
				}

				selectable.navigation = nav;
			}
		}

		public void SetTitle(string titleText, bool forceCaps = true) {
			Title.SetText($"--{titleText}--", forceCaps);
		}

		public void SetButtonText(string buttonText, bool forceCaps = true) {
			OptionsButton.Text.SetText(buttonText, forceCaps);
		}

		void OnEnable() {
			if (!HasBeenShown) {
				HasBeenShown = true;

				CoroManager.InvokeNextFrame(() => {
					foreach (Action<OptionsMenu> action in FirstShown) {
						action(this);
					}
				});
			}

			CoroManager.InvokeNextFrame(() => {
				foreach (Action<OptionsMenu> action in OnShown) {
					action(this);
				}
			});
		}
	}
}