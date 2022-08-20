using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using System;
using System.Collections.Generic;

namespace ULTRAINTERFACE {
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

		public void ScrollToTop(bool includeChildren = true) {
			if (!includeChildren) {
				ScrollView.ScrollRect.ScrollToTop();
				return;
			}

			foreach (ScrollRect scrollRect in GetComponentsInChildren<ScrollRect>(true)) {
				scrollRect.ScrollToTop();
			}
		}

		public RectTransform AddOptionsPanel(TextAnchor anchor = TextAnchor.MiddleLeft, float spacing = 20, float transparency = 1, RectOffset padding = null, string name = "Options Panel") {
			RectTransform panel = UI.CreatePanel(Content, anchor, spacing, transparency, padding, name);

			ContentSizeFitter sizeFitter = panel.GetComponent<ContentSizeFitter>();
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

			return panel;
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

		public void Rebuild(bool updateNavigation = true) {
			Content.Rebuild(updateNavigation);

			// This is just here until the new nav system is ready
			if (updateNavigation) UpdateNavigation();
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