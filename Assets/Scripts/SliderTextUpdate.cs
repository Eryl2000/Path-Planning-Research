using UnityEngine;
using UnityEngine.UI;

public class SliderTextUpdate : MonoBehaviour
{
    float originalFixedDeltaTime;
    Text textComponent;

    void Start()
    {
        originalFixedDeltaTime = Time.fixedDeltaTime;
        textComponent = GetComponent<Text>();
    }

    public void SetSliderValue(float sliderValue)
    {
        textComponent.text = sliderValue.ToString("F2");
        Time.timeScale = sliderValue;
        Time.fixedDeltaTime = originalFixedDeltaTime * sliderValue;
    }
}