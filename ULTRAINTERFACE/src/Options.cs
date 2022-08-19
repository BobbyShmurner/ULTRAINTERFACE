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
		public static RectTransform OptionsMenu { get; private set; } 

		public static void CreateOptionsMenu(string title, Action<OptionsMenu> createAction, string buttonText = "", bool forceCaps = true, bool updateNavigation = true) {
			UI.RegisterOnSceneLoad((scene) => {
				OptionsMenu optionsMenu = CreateOptionsMenu_Internal(title, buttonText, forceCaps);
				if (optionsMenu == null) return;

				createAction(optionsMenu);
				optionsMenu.Rebuild(updateNavigation);
			});
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
				menu.Rebuild(false); // Dont update navigation here, because it would overwrite any manual navigation that's been setup
				menu.ScrollToTop(true);
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

			try {
				OptionsMenuToManager optionsMenuToManager = GameObject.FindObjectOfType<OptionsMenuToManager>();
				OptionsMenu = optionsMenuToManager.transform.Find("OptionsMenu").GetComponent<RectTransform>();	
			} catch {
				UI.Log.LogError("Failed to find the OptionsMenu, will attempt to setup Options UI on next scene load");
				return false;
			}

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

			return true;
		}

		internal static void MoveOptionToOptionScroll(string optionName) {
			RectTransform option = OptionsMenu.Find(optionName).GetComponent<RectTransform>();
			option.SetParent(OptionsScroll.Content, false);
			option.anchoredPosition = Vector2.zero;
		}
	}
}