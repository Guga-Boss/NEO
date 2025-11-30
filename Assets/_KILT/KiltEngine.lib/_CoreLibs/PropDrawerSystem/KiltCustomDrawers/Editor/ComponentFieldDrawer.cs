using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ComponentFieldAttribute))]
public class ComponentFieldDrawer : SpecificFieldDrawer
{
	#region Draw Single Component Functions

	protected override void DrawComponent(Rect position, SerializedProperty property, GUIContent label, System.Type p_type)
	{
		Color v_oldColor = GUI.backgroundColor;
		int v_selectorWidth = _isArrayOfComponents? 40 : 30;
		System.Type v_type = p_type;
		SpecificFieldAttribute v_attr = attribute as SpecificFieldAttribute;
		if(v_attr != null && IsSameOrSubClassOrImplementInterface(v_type, v_attr.AcceptedType))
		{
			Component v_selfAsComponent = GetSelfAsComponent(property, p_type);
			List<Component> v_components = GetComponents(property, v_type, p_type);
			string [] v_componentsString = GetStringArrayFromComponents(v_components);
			int v_currentIndex = GetCurrentComponentIndex(v_components,v_selfAsComponent);
			//Draw
			Rect v_objectRect = new Rect(position.x, position.y , Mathf.Max(0,position.width-v_selectorWidth), position.height);
			Rect v_indexRect = new Rect(position.x + v_objectRect.width, position.y, position.width-v_objectRect.width, position.height);
			Object v_object = EditorGUI.ObjectField(v_objectRect, label, property.objectReferenceValue, v_type, true);
			GUI.backgroundColor = v_currentIndex == -1? Color.red : Color.green;
			int v_newIndex = EditorGUI.Popup(v_indexRect, v_currentIndex, v_componentsString);
			GUI.backgroundColor = v_oldColor;
			//Apply
			if(v_object != property.objectReferenceValue)
				property.objectReferenceValue = v_object;
			else if(v_newIndex != v_currentIndex)
				property.objectReferenceValue = v_newIndex >=0 && v_newIndex < v_components.Count? v_components[v_newIndex] : property.objectReferenceValue;
		}
	}

	#endregion

	#region Helper Functions

	private List<Component> GetComponents(SerializedProperty property, System.Type v_typeFilter, System.Type p_type)
	{
		System.Type v_type = p_type;
		Component[] v_components = new Component[0];
		if(IsSameOrSubClassOrImplementInterface(v_type, typeof(Component)))
		{
			if(property.objectReferenceValue != null)
			{
				Component v_componentRef = property.objectReferenceValue as Component;
				if(v_componentRef != null)
				{
					v_components = v_componentRef.GetComponents(v_typeFilter);
				}
			}
		}
		return new List<Component>(v_components);
	}

	private Component GetSelfAsComponent(SerializedProperty property, System.Type p_type)
	{
		System.Type v_type = p_type;
		Component v_componentRef = null;
		if(IsSameOrSubClassOrImplementInterface(v_type, typeof(Component)))
		{
			if(property.objectReferenceValue != null)
			{
				v_componentRef = property.objectReferenceValue as Component;
			}
		}
		return v_componentRef;
	}

	private int GetCurrentComponentIndex(List<Component> p_components, Component p_current)
	{
		int v_index = -1;
		if(p_components != null)
		{
			int i = 0;
			foreach(Component v_comp in p_components)
			{
				if(v_comp == p_current)
				{
					v_index = i;
					break;
				}
				i ++;
			}
		}
		return v_index;
	}

	private string [] GetStringArrayFromComponents(List<Component> p_components)
	{
		string [] v_componentsString = p_components != null? GetStringList(p_components).ToArray() : new string[0];
		for(int i=0; i<v_componentsString.Length; i++)
		{
			if(v_componentsString[i] != null)
			{
				int v_firstRefIndex = v_componentsString[i].IndexOf("(") + 1;
				int v_lastRefIndex = v_componentsString[i].LastIndexOf(")") - 1;
				v_componentsString[i] =(i+1) + ": " + ( v_firstRefIndex >= 0 && v_firstRefIndex < v_componentsString[i].Length && v_lastRefIndex >= 0 && v_lastRefIndex < v_componentsString[i].Length  && v_firstRefIndex < v_lastRefIndex? v_componentsString[i].Substring(v_firstRefIndex, (v_lastRefIndex+1)-v_firstRefIndex): v_componentsString[i]);
			}
		}
		return v_componentsString;
	}

	#endregion
}
