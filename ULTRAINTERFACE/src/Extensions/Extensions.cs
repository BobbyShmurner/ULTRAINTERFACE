using UnityEngine;
using UnityEngine.UI;

using System.IO;

namespace ULTRAINTERFACE {
	public static class Extensions {
		public static void SetText(this Text text, string newText, bool forceCaps = true) {
			if (forceCaps) newText = newText.ToUpper();
			text.text = newText;
		}

		public static void ScrollToTop(this ScrollRect scrollRect) {
			float movementVal = (scrollRect.content.sizeDelta.y - scrollRect.GetComponent<RectTransform>().sizeDelta.y) * -0.5f;
			if (movementVal > 0) return;

			scrollRect.content.anchoredPosition = new Vector2(scrollRect.content.anchoredPosition.x, movementVal);
		}

		public static void Rebuild(this RectTransform rect, bool updateNavigation = true) {
			LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
		}

		public static byte[] ToByteArray(this Stream stream) {
			using (MemoryStream ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}
	}
}