using UnityEngine.UI;

namespace ULTRAINTERFACE {
	public class CustomButton : UIComponent {
		public Text Text { get; private set; }
		public Button Button { get; private set; }

		internal void Init(Button button, Text text) {
			Button = button;
			Text = text;
		}
	}
}