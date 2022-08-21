using UnityEngine;
using UnityEngine.UI;

namespace ULTRAINTERFACE {
	public class CustomText : UIComponent {
		public Text Text { get; private set; }

		public static implicit operator Text(CustomText text) => text.Text;

		internal void Init(Text text) {
			Text = text;
		}

		public void SetText(string text, bool forceCaps = true) {
			Text.SetText(text, forceCaps);
		}

		public void SetColor(Color color) {
			Text.color = color;
		}

		public void SetFontSize(int fontSize) {
			Text.fontSize = fontSize;
		}
	}
}