using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UltiBar : MonoBehaviour {

    public Slider slider;
    public Gradient sliderColor;
    public Image fill;

    public void clearBar () {
        slider.maxValue = 100;
        slider.value = 0;
        fill.color = sliderColor.Evaluate (1f);
    }

    public void SetUltValue (int ultProgress) {
        slider.value = ultProgress;
        fill.color = sliderColor.Evaluate (slider.normalizedValue);

    }
}