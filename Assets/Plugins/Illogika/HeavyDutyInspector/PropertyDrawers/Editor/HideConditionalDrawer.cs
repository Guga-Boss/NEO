//----------------------------------------------
//            Heavy-Duty Inspector
//         Copyright © 2014  Illogika
//----------------------------------------------
#if !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace HeavyDutyInspector
{

	[CustomPropertyDrawer(typeof(HideConditionalAttribute))]
	public class HideConditionalDrawer : IllogikaDrawer {
			
		HideConditionalAttribute hideConditionalAttribute { get { return ((HideConditionalAttribute)attribute); } }

		public bool isVisible(SerializedProperty prop)
		{
			switch(hideConditionalAttribute.conditionType)
			{
			case HideConditionalAttribute.ConditionType.IsNotNull:
				return GetReflectedFieldRecursively<System.Object>(prop, hideConditionalAttribute.variableName) != null;
			case HideConditionalAttribute.ConditionType.IntOrEnum:
				return hideConditionalAttribute.enumValues.Contains(GetReflectedFieldRecursively<int>(prop, hideConditionalAttribute.variableName));
			case HideConditionalAttribute.ConditionType.FloatRange:
				if(hideConditionalAttribute.minValue < hideConditionalAttribute.maxValue)
				{
					Debug.LogError("Min value has to be lower than max value");
					return false;
				}
				else
				{
					return GetReflectedFieldRecursively<float>(prop, hideConditionalAttribute.variableName) >= hideConditionalAttribute.minValue && GetReflectedFieldRecursively<float>(prop, hideConditionalAttribute.variableName) <= hideConditionalAttribute.maxValue;
				}
			case HideConditionalAttribute.ConditionType.Bool:
				return GetReflectedFieldRecursively<bool>(prop, hideConditionalAttribute.variableName) == hideConditionalAttribute.boolValue;
			default:
				break;
			}
			return false;
		}

		public override float GetPropertyHeight (SerializedProperty prop, GUIContent label)
		{
			if(isVisible(prop))
	    		return base.GetPropertyHeight(prop, label);

			return -2.0f;
	    }
		
		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, prop);

			if(isVisible(prop))
			{
				EditorGUI.PropertyField(position, prop);
			}

			EditorGUI.EndProperty();
		}
	}

}
#endif
