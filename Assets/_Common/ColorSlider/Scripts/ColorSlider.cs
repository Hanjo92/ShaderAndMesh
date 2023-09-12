using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class ColorSlider : MonoBehaviour
{
    private Slider slider;
    [SerializeField] private Image colorPalette;
	private Texture2D paletteTexture;
    private Color[] colors;

    private void Start()
    {
        slider = GetComponent<Slider>();
        if(colorPalette == null)
        {
            Debug.LogError("colorPalette object is null");
            return;
        }
        if(colorPalette.sprite == null)
        {
			Debug.LogError("colorPalette image is null");
			return;
		}
        paletteTexture = colorPalette.sprite.texture;
        colors = paletteTexture.GetPixels();

		slider.onValueChanged.AddListener(OnValueChange);
	}

    public Action<Color> OnChangedColor;
	private void OnValueChange(float value)
	{
        var pixelindex = (int)(value * paletteTexture.width);
        OnChangedColor?.Invoke(colors[pixelindex]);
	}
}
