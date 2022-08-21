using UnityEngine;
using UnityEngine.UI;

namespace ULTRAINTERFACE {
	public class CustomPanel : UIComponent {
		public Image Image { get; private set; }

		public static implicit operator RectTransform(CustomPanel panel) => panel.RectTransform;

		internal void Init(Image image) {
			Image = image;
		}

		public void SetTransparency(float transparency) {
			Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, transparency);
		}

		public void SetColor(Color color) {
			Image.color = color;
		}
	}
}