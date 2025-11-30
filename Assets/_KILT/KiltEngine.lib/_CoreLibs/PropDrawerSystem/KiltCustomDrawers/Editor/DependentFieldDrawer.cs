using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(DependentFieldAttribute))]
public class DependentFieldDrawer : SpecificFieldDrawer {

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		DependentFieldAttribute attr = attribute as DependentFieldAttribute;
		if(attr != null && attr.CheckerFalseDrawOption != CheckerFalseDrawOptionEnum.DontDrawProperty)
			return EditorGUI.GetPropertyHeight(property, label, true);
		else
			return GetDependentFieldValue(property)? EditorGUI.GetPropertyHeight(property, label, true): -2;
	}
	
	protected override void DrawComponent(Rect position, SerializedProperty property, GUIContent label, System.Type p_type)
	{
		DependentFieldAttribute attr = attribute as DependentFieldAttribute;
		if(attr != null)
		{
			bool v_dependentFieldValue = GetDependentFieldValue(property);
			if(v_dependentFieldValue || attr.CheckerFalseDrawOption != CheckerFalseDrawOptionEnum.DontDrawProperty)
			{
				bool v_oldGUIEnabled = GUI.enabled;
				if(!v_dependentFieldValue && attr.CheckerFalseDrawOption == CheckerFalseDrawOptionEnum.ReadOnlyProperty)
					GUI.enabled = false;
				DrawComponentAfterDependenceCheck(position, property, label, p_type);
				GUI.enabled = v_oldGUIEnabled;
			}
		}
	}

	protected virtual void DrawComponentAfterDependenceCheck(Rect position, SerializedProperty property, GUIContent label, System.Type p_type)
	{
		EditorGUI.PropertyField(position, property, label, true);
	}

	protected virtual bool GetDependentFieldValue(SerializedProperty property)
	{
		DependentFieldAttribute attr = attribute as DependentFieldAttribute;
		SerializedProperty v_dependentField =  property.serializedObject != null && attr != null? property.serializedObject.FindProperty(GetPrePath(property) + attr.DependentBoolFieldName) : null;
		bool v_dependentFieldValue = v_dependentField != null && v_dependentField.propertyType == SerializedPropertyType.Boolean? v_dependentField.boolValue : true;
		if(attr != null && attr.DependenceOption == DependenceOptionEnum.InverseDependence)
			v_dependentFieldValue = !v_dependentFieldValue;
		return v_dependentFieldValue;
	}

	public virtual string GetPrePath(SerializedProperty property)
	{
		string v_prePath = "";
		if(property != null && !string.IsNullOrEmpty(property.propertyPath))
		{
			if(property.propertyPath.Contains("."))
			{
				string v_pathToCheck = property.propertyPath;
				string v_stringToRemove = ".Array.data";
				if(property.propertyPath.Contains(v_stringToRemove))
				{
					int v_pathLastIndex = property.propertyPath.LastIndexOf(v_stringToRemove);
					int v_pathSize = v_pathLastIndex;
					if(v_pathSize > 0 && v_pathSize <= property.propertyPath.Length)
						v_pathToCheck = property.propertyPath.Substring(0, v_pathSize);
					else
						v_pathToCheck = "";
				}
				int v_lastIndex = v_pathToCheck.LastIndexOf(".");
				int v_size = v_lastIndex + 1;
				if(v_size > 0 && v_size <= v_pathToCheck.Length)
					v_prePath = v_pathToCheck.Substring(0, v_size);
			}
		}
		return v_prePath;
	}
}
