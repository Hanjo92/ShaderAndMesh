using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllUI : MonoBehaviour
{
	[Header("Color")]
    [SerializeField] private Slider rSlider;
	[SerializeField] private Slider gSlider;
	[SerializeField] private Slider bSlider;
	[SerializeField] private Slider aSlider;

	[SerializeField] private Image colorView;

	[Header("Line")]
	[SerializeField] private Slider depthSlider;
	[SerializeField] private Slider thicknessSlider;

	[SerializeField] private MeshContainer meshContainer;

	private void Start()
	{
		rSlider.onValueChanged.AddListener(OnColorSliderValueChange);
		gSlider.onValueChanged.AddListener(OnColorSliderValueChange);
		bSlider.onValueChanged.AddListener(OnColorSliderValueChange);
		aSlider.onValueChanged.AddListener(OnColorSliderValueChange);

		depthSlider.onValueChanged.AddListener(f => meshContainer.ChangeDepth(f));
		thicknessSlider.onValueChanged.AddListener(f => meshContainer.ChangeThickness(f));

		meshContainer.ChangeColor(new Color(rSlider.value, gSlider.value, bSlider.value, aSlider.value));
		meshContainer.ChangeDepth(depthSlider.value);
		meshContainer.ChangeThickness(thicknessSlider.value);
	}

	private void OnColorSliderValueChange(float v)
	{
		if(colorView == null)
			return;

		var r = rSlider.value;
		var g = gSlider.value;
		var b = bSlider.value;
		var a = aSlider.value;

		colorView.color = new Color(r, g, b, a);

		meshContainer?.ChangeColor(colorView.color);
	}
}
