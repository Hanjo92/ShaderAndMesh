using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshContainer : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] renderers;

	private void Start()
	{
		renderers = GetComponentsInChildren<MeshRenderer>();
	}


	public void ChangeColor(Color color)
	{
		if(renderers ==  null || renderers.Length == 0) return;

		foreach(var renderer in renderers)
		{
			var materials = renderer.materials;
			foreach(var material in materials)
			{
				material.SetColor("_MainColor", color);
			}
		}
	}
}
