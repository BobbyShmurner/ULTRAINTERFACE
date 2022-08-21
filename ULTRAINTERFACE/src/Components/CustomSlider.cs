using UnityEngine;
using UnityEngine.UI;

namespace ULTRAINTERFACE {
	public class CustomSlider : UIComponent {
		public Text Label { get; private set; }
		public Text Value { get; private set; }
		public Slider Slider { get; private set; }
		public SliderSettings SliderSettings { get; private set; }
		public SliderValueToText SliderValueToText { get; private set; }

		public static implicit operator Slider(CustomSlider slider) => slider.Slider;

		internal void Init(Slider slider, Text label, Text value, SliderSettings settings) {
			Slider = slider;
			Label = label;
			Value = value;
			SliderValueToText = Value.GetComponent<SliderValueToText>();

			UpdateSliderSettings(settings);
		}

		public void SetValue(float value, bool notify = true) {
			if (notify) Slider.value = value;
			else Slider.SetValueWithoutNotify(value);
		}

		public void UpdateSliderSettings(SliderSettings settings) {
			this.SliderSettings = settings;

			this.SliderValueToText.ifMin = settings.IfMin;
			this.SliderValueToText.ifMax = settings.IfMax;
			this.SliderValueToText.suffix = settings.Suffix;
			this.SliderValueToText.minColor = settings.MinColor;
			this.SliderValueToText.maxColor = settings.MaxColor;
			this.SliderValueToText.decimalType = settings.DecimalType;

			this.Slider.minValue = settings.Min;
			this.Slider.maxValue = settings.Max;
			this.Slider.wholeNumbers = settings.DecimalType == DecimalType.NoDecimals;
		}
	}

	public struct SliderSettings {
		public float Min { get; internal set; }
		public float Max { get; internal set; }
		public string Suffix { get; internal set; }
		public string IfMin { get; internal set; }
		public string IfMax { get; internal set; }
		public DecimalType DecimalType { get; internal set; }
		public Color MinColor { get; internal set; }
		public Color MaxColor { get; internal set; }

		public SliderSettings(float min, float max, DecimalType decimalType = DecimalType.NoLimit, string suffix = "", string ifMin = "", string ifMax = "") {
			DecimalType = decimalType;
			Suffix = suffix;
			IfMin = ifMin;
			IfMax = ifMax;
			Min = min;
			Max = max;

			MinColor = new Color(0, 0, 0, 0);
			MaxColor = new Color(0, 0, 0, 0);
		}

		public SliderSettings(float min, float max, Color minColor, Color maxColor, DecimalType decimalType = DecimalType.NoLimit, string suffix = "", string ifMin = "", string ifMax = "") {
			DecimalType = decimalType;
			Suffix = suffix;
			IfMin = ifMin;
			IfMax = ifMax;
			Min = min;
			Max = max;

			MinColor = minColor;
			MaxColor = maxColor;
		}
	}
}