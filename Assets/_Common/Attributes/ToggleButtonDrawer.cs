using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ToggleButtonAttribute))]
public class ToggleButtonDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var propertyName = (attribute as ToggleButtonAttribute).TargetName;
		var target = property.serializedObject.targetObject;
		var type = target.GetType();

		var targetProperty = type.GetProperty(propertyName);
		if(targetProperty != null)
		{
			if(targetProperty.PropertyType != typeof(bool))
			{
				GUI.Label(position, "TargetMember must be a bool type.");
				return;
			}

			var currentValue = (bool)targetProperty.GetValue(target);
			var buttonText = $"{propertyName} :: {currentValue}";

			if(GUI.Button(position, buttonText))
			{
				targetProperty.SetValue(target, !currentValue);
			}
		}
		else
		{
			GUI.Label(position, $"Is not property type. {propertyName}");
		}
	}
}
#endif

[AttributeUsage(AttributeTargets.Field)]
public class ToggleButtonAttribute : PropertyAttribute
{
	public string TargetName
	{
		get;
	}

	/// <summary>
	/// is bool property toggle button on inspector
	/// </summary>
	/// <param name="targetName"> must be a bool type </param>
	public ToggleButtonAttribute(string targetName)
	{
		TargetName = targetName;
	}
}