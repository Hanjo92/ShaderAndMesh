using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllUI : MonoBehaviour
{
    [SerializeField] private Slider rSlider;
	[SerializeField] private Slider gSlider;
	[SerializeField] private Slider bSlider;
	[SerializeField] private Slider aSlider;

	[SerializeField] private Image colorView;

	[SerializeField] private MeshContainer meshContainer;

	private void Start()
	{
		rSlider?.onValueChanged.AddListener(OnSliderValueChange);
		gSlider?.onValueChanged.AddListener(OnSliderValueChange);
		bSlider?.onValueChanged.AddListener(OnSliderValueChange);
		aSlider?.onValueChanged.AddListener(OnSliderValueChange);
	}

	private void OnSliderValueChange(float v)
	{
		if(colorView == null)
			return;

		var r = rSlider?.value ?? 0;
		var g = gSlider?.value ?? 0;
		var b = bSlider?.value ?? 0;
		var a = aSlider?.value ?? 0;

		colorView.color = new Color(r, g, b, a);

		meshContainer?.ChangeColor(colorView.color);
	}
}
