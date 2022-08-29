using UnityEngine;
using UnityEngine.UI;

namespace ULTRAINTERFACE {
	public class CustomImage : UIComponent {
		public Image Image { get; private set; }

		public Sprite Sprite {
			get { return Image.sprite; }
			private set { Image.sprite = value; }
		}

		public static implicit operator Image(CustomImage image) => image.Image;
		public static implicit operator Sprite(CustomImage image) => image.Sprite;

		internal void Init(Image image) {
			Image = image;
		}

		public void SetSprite(Sprite newSprite, bool setDimensions) {
			Sprite = newSprite;
			if (setDimensions && newSprite != null) SetDimensions(newSprite.bounds.size.x, newSprite.bounds.size.y);
		}

		public void SetTransparency(float transparency) {
			Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, transparency);
		}

		public void SetColor(Color color) {
			Image.color = color;
		}
	}
}