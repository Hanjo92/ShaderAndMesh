using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Utility
{
	public static float ConvertAngleTo360Range(float angle) => angle % 360f;
	public static float ConvertAngleTo180Range(float angle)
	{
		var range360 = ConvertAngleTo360Range(angle);
		if(range360 < 0)
		{
			return (range360 < -180f) ? (360 + range360) : range360;
		}
		return (range360 > 180) ? (range360 - 360) : range360;
	}

	public static bool CheckMouseOnUI()
	{
		var pointerData = new PointerEventData(EventSystem.current);
		pointerData.position = Input.mousePosition;
		var results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerData, results);
		foreach(var result in results)
		{
			if(result.gameObject.layer == LayerMask.NameToLayer("UI"))
				return true;
		}
		return false;
	}
}
