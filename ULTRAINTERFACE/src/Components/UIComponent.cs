using UnityEngine;
using UnityEngine.UI;

namespace ULTRAINTERFACE {
	[RequireComponent(typeof(RectTransform))]
	public class UIComponent : ModObject {
		public bool dimensionsHaveBeenSet { get; private set; } = false;

		RectTransform rectTransform;
		public RectTransform RectTransform { 
			get {
				if (rectTransform == null) rectTransform = (RectTransform)transform;
				return rectTransform;
			}
		}

		HudOpenEffect hudOpenEffect;
		public HudOpenEffect HudOpenEffect { 
			get {
				if (hudOpenEffect == null) hudOpenEffect = gameObject.AddComponent<HudOpenEffect>();
				return hudOpenEffect;
			}
			private set {
				hudOpenEffect = value;
			}
		}

		LayoutElement layoutElement;
		public LayoutElement LayoutElement { 
			get {
				if (layoutElement == null) layoutElement = gameObject.AddComponent<LayoutElement>();
				return layoutElement;
			}
			private set {
				layoutElement = value;
			}
		}

		BackSelectOverride backSelectOverride;
		public BackSelectOverride BackSelectOverride { 
			get {
				if (backSelectOverride == null) {
					if (GetComponent<Selectable>() == null) {
						UI.Log.LogError($"Cannot add a BackSelectOverride component to UIComponent \"{gameObject.name}\" because it doesn't have any Selectable components attached to it");
						return null;
					} else backSelectOverride = gameObject.AddComponent<BackSelectOverride>();
				}
				return backSelectOverride;
			}
			private set {
				backSelectOverride = value;
			}
		}

		ContentSizeFitter contentSizeFitter;
		public ContentSizeFitter ContentSizeFitter { 
			get {
				if (contentSizeFitter == null) contentSizeFitter = gameObject.AddComponent<ContentSizeFitter>();
				return contentSizeFitter;
			}
			private set {
				contentSizeFitter = value;
			}
		}

		void Awake() {
			hudOpenEffect = GetComponent<HudOpenEffect>();
			layoutElement = GetComponent<LayoutElement>();
			backSelectOverride = GetComponent<BackSelectOverride>();
			contentSizeFitter = GetComponent<ContentSizeFitter>();
		}

		void Start() {
			if (layoutElement == null || dimensionsHaveBeenSet) return; // Only set the dimensions on start if they havent already been set manually, and only if there is actually a layout component
			SetDimensions(RectTransform.sizeDelta.x, RectTransform.sizeDelta.y);
		}

		public virtual void SetHudOpenEffectActive(bool active) {
			HudOpenEffect.enabled = active;
		}

		public virtual void SetBackSelectOverride(Selectable selectable) {
			if (BackSelectOverride == null) return;
			BackSelectOverride.Selectable = selectable;
		}

		public virtual void SetupDefaultBackSelectOverride() {
			Options.SetupDefaultBackSelectOverride(this);
		}

		public virtual void SetDimensions(float width, float height) {
			string warningMessage = "You are trying to change the dimensions of UIComponent \"{gameObject.name}\", which has a Layout Element component attached. You should not be changing the dimensions of a gameObject with a Layout Element component";
			bool hasDisplayedWarning = false;

			RectTransform.sizeDelta = new Vector2(width, height);
			dimensionsHaveBeenSet = true;

			if (contentSizeFitter == null || ContentSizeFitter.horizontalFit == ContentSizeFitter.FitMode.Unconstrained) {
				LayoutElement.minWidth = width;
				LayoutElement.flexibleWidth = width;
				LayoutElement.preferredWidth = width;
			} else {
				LayoutElement.minWidth = -1;
				LayoutElement.flexibleWidth = -1;
				LayoutElement.preferredWidth = -1;

				// I'm awear that checking if its false is redundant, but I want to because of symmetry
				if (!hasDisplayedWarning) {
					UI.Log.LogWarning(warningMessage);
					hasDisplayedWarning = true;
				}
			}

			if (contentSizeFitter == null || ContentSizeFitter.verticalFit == ContentSizeFitter.FitMode.Unconstrained) {
				LayoutElement.minHeight = height;
				LayoutElement.flexibleHeight = height;
				LayoutElement.preferredHeight = height;
			} else {
				LayoutElement.minHeight = -1;
				LayoutElement.flexibleHeight = -1;
				LayoutElement.preferredHeight = -1;

				if (!hasDisplayedWarning) {
					UI.Log.LogWarning(warningMessage);
					hasDisplayedWarning = true; // Same thing with setting this to true
				}
			}
		}
	}
}