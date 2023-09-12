using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.VisualScripting;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FloatPropertyAttribute))]
public class FloatPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var fieldAttribute = (attribute as FloatPropertyAttribute);
		var propertyName = fieldAttribute.TargetName;
		var target = property.serializedObject.targetObject;
		var type = target.GetType();

		var targetProperty = type.GetProperty(propertyName);
		if(targetProperty != null)
		{
			if(targetProperty.PropertyType != typeof(float))
			{
				GUI.Label(position, "TargetMember must be a float type.");
				return;
			}

			var currentValue = (float)targetProperty.GetValue(target);
			var rangeAttribute = fieldInfo.GetAttribute<RangeAttribute>();
			float inspectorValue;
			if(rangeAttribute != null)
			{
				inspectorValue = EditorGUI.Slider(position, propertyName, currentValue, rangeAttribute.min, rangeAttribute.max);
			}
			else
			{
				inspectorValue = EditorGUI.FloatField(position, propertyName, currentValue, "WhiteBoldLabel");
			}

			if(inspectorValue != currentValue)
				targetProperty.SetValue(target, inspectorValue);
		}
		else
		{
			GUI.Label(position, $"Is not property type. {propertyName}");
		}
	}
}
#endif

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class FloatPropertyAttribute : PropertyAttribute
{
	public string TargetName
	{
		get;
	}

	/// <summary>
	/// is bool property toggle button on inspector
	/// </summary>
	/// <param name="targetName"> must be a float type </param>
	public FloatPropertyAttribute(string targetName)
	{
		TargetName = targetName;
	}
}