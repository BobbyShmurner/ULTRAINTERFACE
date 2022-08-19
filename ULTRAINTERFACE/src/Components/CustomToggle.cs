using UnityEngine.UI;

namespace ULTRAINTERFACE {
	public class CustomToggle : UIComponent {
		public Text Label { get; private set; }
		public Toggle Toggle { get; private set; }
		public Image Checkmark { get; private set; }
		public Image Background { get; private set; }

		internal void Init(Toggle toggle, Text label, Image background, Image checkmark) {
			Toggle = toggle;
			Label = label;
			Background = background;
			Checkmark = checkmark;
		}

		public void SetValue(bool isOn, bool notify = true) {
			if (notify) Toggle.isOn = isOn;
			else Toggle.SetIsOnWithoutNotify(isOn);
		}
	}
}