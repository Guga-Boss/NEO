using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[CustomPropertyDrawer(typeof(SerializableTypeAttribute))]
public class SerializableTypeDrawer : DependentFieldDrawer {
	
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		SerializableType v_type = GetSerializableType(property);
		int v_fixedSize = 16;
		if(v_type.FoldOut)
			v_fixedSize = v_type != null && v_type.CastedType != null? v_fixedSize*(v_type.CastedType.GetGenericArguments().Length + 1): v_fixedSize;
		return GetDependentFieldValue(property)? v_fixedSize: -2;
	}

	protected override void DrawComponentAfterDependenceCheck(Rect position, SerializedProperty property, GUIContent label, System.Type p_type)
	{
		SerializableTypeAttribute v_attr = (SerializableTypeAttribute)attribute;
		SerializableType v_serType = GetSerializableType(property);
		SerializableType v_newType = InspectorUtils.SerializableTypePopup(position, label.text, v_serType, v_attr.FilterType, v_attr.AcceptGenericDefinitions);
		if(v_newType == null)
			v_newType = new SerializableType();
		if(v_serType.CastedType != v_newType.CastedType || v_serType.StringType != v_newType.StringType)
		{
			SetSerializableType(property, v_newType);
			try
			{
				EditorUtility.SetDirty(property.serializedObject.targetObject);
			}
			catch{}
		}
	}

	protected virtual SerializableType GetSerializableType(SerializedProperty p_property)
	{
		SerializableType v_return = null;
		if(p_property.serializedObject != null && p_property.serializedObject.targetObject != null)
		{
			try
			{
				FieldInfo v_myField = p_property.serializedObject.targetObject.GetType().GetField(p_property.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				if(v_myField != null)
				{
					v_return = (SerializableType)v_myField.GetValue(p_property.serializedObject.targetObject);
				}
			}
			catch{}
		}
		if(v_return == null)
			v_return = new SerializableType();
		return v_return;
	}

	protected virtual void SetSerializableType(SerializedProperty p_property, SerializableType p_value)
	{
		if(p_property.serializedObject != null && p_property.serializedObject.targetObject != null)
		{
			FieldInfo v_myField = p_property.serializedObject.targetObject.GetType().GetField(p_property.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
			if(v_myField != null)
			{
				v_myField.SetValue(p_property.serializedObject.targetObject, p_value);
			}
		}
	}
}
