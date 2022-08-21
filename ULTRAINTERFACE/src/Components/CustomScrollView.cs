using UnityEngine;
using UnityEngine.UI;

namespace ULTRAINTERFACE {
	public class CustomScrollView : UIComponent {
		public RectTransform Content { get; private set; }
		public ScrollRect ScrollRect { get; private set; }
		public Scrollbar Scrollbar { get; private set; }

		public static implicit operator RectTransform(CustomScrollView scrollView) => scrollView.Content;

		internal void Init(ScrollRect scrollRect, Scrollbar scrollbar, Transform content) {
			ScrollRect = scrollRect;
			Scrollbar = scrollbar;
			Content = (RectTransform)content;
		}
	}
}