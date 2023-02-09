using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace ULTRAINTERFACE {
	public class CustomModal : UIComponent {
		public List<Action<CustomModal>> LateFirstShown { get; private set; } = new List<Action<CustomModal>>();
		public List<Action<CustomModal>> OnLateShown { get; private set; } = new List<Action<CustomModal>>();
		public List<Action<CustomModal>> LateCreate { get; private set; } = new List<Action<CustomModal>>();
		public List<Action<CustomModal>> FirstShown { get; private set; } = new List<Action<CustomModal>>();
		public List<Action<CustomModal>> OnShown { get; private set; } = new List<Action<CustomModal>>();
		public List<Action<CustomModal>> OnHide { get; private set; } = new List<Action<CustomModal>>();

		public GamepadObjectSelector GamepadObjectSelector { get; private set; }
		public RectTransform Content { get; private set; }
		public Image RaycastScreen { get; private set; }
		public Image Background { get; private set; }
		public MenuEsc MenuEsc { get; private set; }
		public Image Outline { get; private set; }

		public bool HasBeenShown { get; private set; } = false;

		public static implicit operator RectTransform(CustomModal modal) => modal.Content;

		internal void Init(RectTransform content, Image raycastScreen, Image background, Image outline, GamepadObjectSelector gamepadObjectSelector, MenuEsc menuEsc) {
			GamepadObjectSelector = gamepadObjectSelector;
			RaycastScreen = raycastScreen;
			Background = background;
			Content = content;
			Outline = outline;
			MenuEsc = menuEsc;

			CoroManager.InvokeNextFrame(() => {
				foreach (Action<CustomModal> action in LateCreate) {
					action(this);
				}
			});

			gameObject.SetActive(false);
		}

		public void SetFirstSelectable() {
			Selectable firstSelectable = Content.GetComponentInChildren<Selectable>();
			typeof(GamepadObjectSelector).GetField("mainTarget", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(GamepadObjectSelector, firstSelectable ? firstSelectable.gameObject : null);
		}

		public void Rebuild(bool updateNavigation = true) {
			Content.Rebuild(updateNavigation);
		}

		public void Show() {
			if (!HasBeenShown) {
				HasBeenShown = true;

				foreach (Action<CustomModal> action in FirstShown) {
					action(this);
				}

				CoroManager.InvokeNextFrame(() => {
					foreach (Action<CustomModal> action in LateFirstShown) {
						action(this);
					}
				});
			}

			foreach (Action<CustomModal> action in OnShown) {
				action(this);
			}

			CoroManager.InvokeNextFrame(() => {
				foreach (Action<CustomModal> action in OnLateShown) {
					action(this);
				}
			});

			transform.SetAsLastSibling();
			gameObject.SetActive(true);
		}

		public void Hide() {
			foreach (Action<CustomModal> action in OnHide) {
				action(this);
			}

			gameObject.SetActive(false);
		}

		public void SetRaycastScreenOpacity(float opacity) {
			RaycastScreen.color = new Color(RaycastScreen.color.r, RaycastScreen.color.g, RaycastScreen.color.b, opacity);
		}

		public void SetRaycastScreenActive(bool active) {
			RaycastScreen.gameObject.SetActive(active);
		}

		public void SetRaycastScreenColor(Color color) {
			RaycastScreen.color = color;
		}

		public void SetBackgroundOpacity(float opacity) {
			Background.color = new Color(Background.color.r, Background.color.g, Background.color.b, opacity);
		}

		public void SetBackgroundColor(Color color) {
			Background.color = color;
		}

		public void SetOutlineActive(bool active) {
			Outline.gameObject.SetActive(active);
		}

		public void SetOutlineColor(Color color) {
			Outline.color = color;
		}
	}
}