using HarmonyLib;

using UnityEngine;
using UnityEngine.UI;

namespace ULTRAINTERFACE {
	// Correctly sets the "targetSlider" property for ULTRAINTERFACE Sliders
	[HarmonyPatch("Start")]
	[HarmonyPatch(typeof(SliderValueToText))]
	static class SliderValueToTextPatch {
		static void Postfix(ref Slider ___targetSlider, SliderValueToText __instance) {
			___targetSlider = __instance.transform.parent.GetComponentInChildren<Slider>();
			if (___targetSlider == null) ___targetSlider = __instance.GetComponentInParent<Slider>();
		}
	}
}