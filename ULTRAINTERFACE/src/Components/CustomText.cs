using UnityEngine.UI;

namespace ULTRAINTERFACE {
	public class CustomText : UIComponent {
		public Text Text { get; private set; }

		internal void Init(Text text) {
			Text = text;
		}

		public void SetText(string text, bool forceCaps = true) {
			Text.SetText(text, forceCaps);
		}
	}
}