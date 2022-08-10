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

		static GameObject OptionsPanelPrefab;
		static GameObject CheckboxOptionsPrefab;

		public static void CreateOptionsMenu(string title, Action<OptionsMenu> createAction, string buttonText = "", bool forceCaps = true) {
			UI.RegisterOnSceneLoad((scene) => {
				OptionsMenu optionsMenu = CreateOptionsMenu_Internal(title, buttonText, forceCaps);

				createAction(optionsMenu);
				optionsMenu.UpdateNavigation();
			});
		}

		public static RectTransform CreateOptionsPanel(Transform parent, TextAnchor childAlignment = TextAnchor.MiddleLeft, float spacing = 20) {
			RectTransform panel = GameObject.Instantiate(OptionsPanelPrefab, parent).GetComponent<RectTransform>();
			panel.gameObject.name = "Options Panel";
			panel.gameObject.layer = 5;

			while (panel.childCount > 0) {
				GameObject.DestroyImmediate(panel.GetChild(0).gameObject);
			}

			VerticalLayoutGroup layoutGroup = panel.gameObject.AddComponent<VerticalLayoutGroup>();
			layoutGroup.padding = new RectOffset(40, 40, 20, 20);
			layoutGroup.childAlignment = childAlignment;
			layoutGroup.childForceExpandHeight = false;
			layoutGroup.childForceExpandWidth = false;
			layoutGroup.childControlWidth = false;
			layoutGroup.spacing = spacing;

			ContentSizeFitter fitter = panel.gameObject.AddComponent<ContentSizeFitter>();
			fitter.verticalFit = ContentSizeFitter.FitMode.MinSize;

			return panel;
		}

		public static void SetupBackSelectOverride(GameObject gameObject) {
			OptionsMenu[] menus = gameObject.GetComponentsInParent<OptionsMenu>(true);
			if (menus.Length == 0) return;

			OptionsMenu menu = menus[0];

			BackSelectOverride backSelectOverride = gameObject.GetComponent<BackSelectOverride>();
			if (backSelectOverride == null) backSelectOverride = gameObject.AddComponent<BackSelectOverride>();

			backSelectOverride.Selectable = menu.OptionsButton.Button;
		}

		static OptionsMenu CreateOptionsMenu_Internal(string title, string buttonText, bool forceCaps) {
			if (!Init()) return null;

			if (forceCaps) {
				title = title.ToUpper();
				buttonText = buttonText.ToUpper();
			}
			if (buttonText == "") buttonText = title;

			CustomScrollView scrollView = UI.CreateScrollView(OptionsMenu, 620, 520, 0, TextAnchor.MiddleCenter, CultureInfo.InvariantCulture.TextInfo.ToTitleCase(title.ToLower()) + " Options");
			CustomButton optionsButton = UI.CreateButton(OptionsScroll.Content, title, 14, 160, 50);
			GameObject.Destroy(scrollView.GetComponent<HorizontalLayoutGroup>());
			scrollView.gameObject.AddComponent<HudOpenEffect>();

			Text titleText = UI.CreateText(scrollView.GetComponent<RectTransform>(), $"--{title}--", 24, 620);
			titleText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -75);
			titleText.transform.SetAsFirstSibling();

			RectTransform scrollViewRect = scrollView.GetComponent<RectTransform>();
			scrollViewRect.anchoredPosition = Vector2.zero;
			scrollViewRect.gameObject.SetActive(false);
			scrollViewRect.sizeDelta = Vector2.zero;
			scrollViewRect.anchorMin = Vector2.zero;
			scrollViewRect.anchorMax = Vector2.one;
			scrollViewRect.pivot = Vector2.one / 2f;

			RectTransform scrollRectRect = scrollView.ScrollRect.GetComponent<RectTransform>();
			scrollRectRect.anchoredPosition = new Vector2(0, -50);

			RectTransform scrollbarRect = scrollView.Scrollbar.GetComponent<RectTransform>();
			scrollbarRect.anchoredPosition = new Vector2(330, -50);

			RectTransform scrollRectContentRect = scrollView.Content.GetComponent<RectTransform>();
			scrollRectContentRect.anchorMin = new Vector2(0.5f, 0.5f);
			scrollRectContentRect.anchorMax = new Vector2(0.5f, 0.5f);
			scrollRectContentRect.pivot = new Vector2(0.5f, 0.5f);
			scrollRectContentRect.anchoredPosition = Vector2.zero;

			// Disable these options when clicked on the other buttons
			for (int i = 0; i < OptionsScroll.Content.transform.childCount; i++) {
				Button button = OptionsScroll.Content.transform.GetChild(i).GetComponent<Button>();
				if (button == null || button == optionsButton.Button) continue;
				
				button.onClick.AddListener(() => { scrollView.gameObject.SetActive(false); });
			}

			// Disable the other options when this button is clicked
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

			optionsButton.Text.text = buttonText;

			OptionsMenu optionsMenu = scrollView.gameObject.AddComponent<OptionsMenu>();
			optionsMenu.Init(scrollView, optionsButton, titleText);

			optionsMenu.LateCreate.Add((menu) => {
				Selectable firstSelectable = menu.ScrollView.Content.GetComponentInChildren<Selectable>();
				typeof(GamepadObjectSelector).GetField("target", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(menu.ScrollView.GetComponent<GamepadObjectSelector>(), firstSelectable ? firstSelectable.gameObject : null);
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
			if (!UI.Init()) return false;
			if (OptionsScroll != null) return true;

			// If "Options Scroll View" exists then another mod has set it up already
			Transform existingMenuTrans = OptionsMenu.Find("Options Scroll View");
			if (!existingMenuTrans) {
				OptionsScroll = UI.CreateScrollView(OptionsMenu, 185, 470, 20, TextAnchor.UpperCenter, "Options Scroll View");
				RectTransform optionsScrollRect = OptionsScroll.GetComponent<RectTransform>();
				optionsScrollRect.anchorMin = new Vector2(0, 0.5f);
				optionsScrollRect.anchorMax = new Vector2(0, 0.5f);
				optionsScrollRect.pivot = new Vector2(0, 0.5f);
				optionsScrollRect.anchoredPosition = new Vector3(20, 0, 3);
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

					OptionsScroll.Init(
						existingMenuTrans.GetChild(0).GetChild(0).GetComponent<RectTransform>(),
						existingMenuTrans.GetChild(0).GetComponent<ScrollRect>(),
						existingMenuTrans.GetChild(1).GetComponent<Scrollbar>()
					);
				}
			}

			GameplayOptionsContent = OptionsMenu.Find("Gameplay Options").GetChild(1).GetChild(0);

			OptionsPanelPrefab = GameplayOptionsContent.GetChild(0).gameObject;
			CheckboxOptionsPrefab = GameplayOptionsContent.GetComponentInChildren<Toggle>().gameObject;

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
		public Text Title { get; private set; }

		public RectTransform Content { get { return ScrollView.Content; } }

		public bool IsInitalised { get; private set; } = false;
		public bool HasBeenShown { get; private set; } = false;

		public RectTransform AddOptionsPanel(TextAnchor anchor = TextAnchor.MiddleLeft) {
			return Options.CreateOptionsPanel(Content, anchor);
		}

		public Text AddHeader(string text, int size = 24, float width = 600, float height = 50, TextAnchor anchor = TextAnchor.MiddleCenter, bool forceCaps = true) {
			return UI.CreateText(Content, text, size, width, height, anchor, forceCaps);
		}

		public CustomButton AddButton(string text, UnityAction onClick, int fontSize = 24, float width = 600, float height = 60, bool forceCaps = true) {
			return UI.CreateButton(Content, text, onClick, fontSize, width, height, forceCaps);
		}

		public CustomButton AddButton(string text, int fontSize = 24, float width = 600, float height = 60, bool forceCaps = true) {
			return UI.CreateButton(Content, text, fontSize, width, height, forceCaps);
		}

		public void Rebuild() {
			foreach (LayoutGroup layout in this.GetComponentsInChildren<LayoutGroup>()) {
				LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)layout.transform);
				LayoutRebuilder.MarkLayoutForRebuild((RectTransform)layout.transform);
			}

			UpdateNavigation();
		}

		internal void Init(CustomScrollView scrollView, CustomButton optionsButton, Text title) {
			if (IsInitalised) {
				UI.Log.LogError($"Options Menu \"{gameObject.name}\" already initalised, returning...");
				return;
			}

			this.ScrollView = scrollView;
			this.OptionsButton = optionsButton;
			this.Title = title;

			this.HasBeenShown = false;

			Camera.main.GetComponent<MonoBehaviour>().InvokeNextFrame(() => {
				foreach (Action<OptionsMenu> action in LateCreate) {
					action(this);
				}
				
				UpdateNavigation();
			});

			IsInitalised = true;
		}

		public void UpdateNavigation() {
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
			if (forceCaps) titleText = titleText.ToUpper();

			Title.text = $"--{titleText}--";
		}

		public void SetButtonText(string buttonText, bool forceCaps = true) {
			if (forceCaps) buttonText = buttonText.ToUpper();

			OptionsButton.Text.text = buttonText;
		}

		void OnEnable() {
			if (!HasBeenShown) {
				HasBeenShown = true;

				this.InvokeNextFrame(() => {
					foreach (Action<OptionsMenu> action in FirstShown) {
						action(this);
					}
				});
			}

			this.InvokeNextFrame(() => {
				foreach (Action<OptionsMenu> action in OnShown) {
					action(this);
				}
			});
		}
	}
}