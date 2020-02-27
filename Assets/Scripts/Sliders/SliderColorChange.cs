using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderColorChange : MonoBehaviour
{
    // Change Slider Color with Value Change
    [SerializeField] private Image _fill;

    private Slider _slider;

    private void Awake()
    {
        _slider = this.gameObject.GetComponent<Slider>();
        if (_slider == null)
        {
            Debug.LogError("SliderColorChange; Unable to find Slider script.");
        }
    }

    public void SliderColor(System.Single sliderValue)
    {
        float green = sliderValue >= (_slider.maxValue / 2.0f) ? 1.0f : sliderValue / (_slider.maxValue / 2.0f);
        float red = sliderValue >= (_slider.maxValue / 2.0f) ? (_slider.maxValue - sliderValue) / (_slider.maxValue / 2.0f) : 1.0f;

        _fill.color = new Color(red, green, 0.0f, 1.0f);
    }

}
