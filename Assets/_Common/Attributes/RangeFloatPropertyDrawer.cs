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
[CustomPropertyDrawer(typeof(RangeFloatPropertyAttribute))]
public class RangeFloatPropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var fieldAttribute = (attribute as RangeFloatPropertyAttribute);
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
			float inspectorValue = EditorGUI.Slider(position, propertyName, currentValue, fieldAttribute.Left, fieldAttribute.Right);
			
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

[AttributeUsage(AttributeTargets.Field)]
public class RangeFloatPropertyAttribute : PropertyAttribute
{
	public string TargetName { get; }
	public float Left { get;	}
	public float Right { get; }

	public RangeFloatPropertyAttribute(string targetName, float left, float right)
	{
		TargetName = targetName;
		Left = left;
		Right = right;
	}
}