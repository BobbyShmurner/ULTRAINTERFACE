using UnityEngine;

namespace ULTRAINTERFACE {
	[DontDestroyGameObjectOnUnload] // GameObjects shouldnt be destroyed just because they have this component
	public class CopyRectSize : UIComponent {
		public RectTransform TargetRect;

		void LateUpdate() {
			SetDimensions(TargetRect.sizeDelta.x, TargetRect.sizeDelta.y);
		}
	}
}