using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(SpecificFieldAttribute))]
public abstract class SpecificFieldDrawer : UnityEditor.PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		bool v_oldGuiEnabled = GUI.enabled;
		SpecificFieldAttribute v_attr = attribute as SpecificFieldAttribute;
		if(v_attr != null && v_attr.ReadOnly)
			GUI.enabled = false;
		CheckIfNeedRecalc(property.isArray);
		if(_isComponent)
			DrawComponent(position, property, label);
		else if(_isArrayOfComponents)
			DrawComponent(position, property, label, GetAcceptedFilterTypeFromContainerType(fieldInfo.FieldType));
		else
			EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = v_oldGuiEnabled;
	}
	
	#region Draw Single Component Functions

	protected void DrawComponent(Rect position, SerializedProperty property, GUIContent label)
	{
		SpecificFieldAttribute v_attr = attribute as SpecificFieldAttribute;
		System.Type v_attrType = v_attr != null? v_attr.AcceptedType : typeof(object);
		DrawComponent(position, property, label, fieldInfo != null && fieldInfo.FieldType != null? fieldInfo.FieldType : v_attrType);
	}
	
	protected virtual void DrawComponent(Rect position, SerializedProperty property, GUIContent label, System.Type p_type)
	{
		EditorGUI.PropertyField(position, property, label, true);
	}
	
	#endregion

	#region Helper Functions
	
	protected bool _needRecalc = true;
	protected bool _isComponent = false;
	protected bool _isArrayOfComponents = false;
	protected virtual void CheckIfNeedRecalc(bool p_isArray)
	{
		SpecificFieldAttribute v_attr = attribute as SpecificFieldAttribute;
		if(v_attr != null)
		{
			System.Type v_acceptedType = v_attr.AcceptedType;
			System.Type v_type = this.fieldInfo !=null? this.fieldInfo.FieldType: v_attr.AcceptedType;
			System.Type v_convertedType = GetAcceptedFilterTypeFromContainerType(v_type);
			if(_needRecalc && v_type != null)
			{
				_needRecalc = false;
				_isComponent = IsSameOrSubClassOrImplementInterface(v_type, v_acceptedType);
				_isArrayOfComponents = (v_type.IsArray || IsSameOrSubClassOrImplementInterface(v_type, typeof(IList))) &&  IsSameOrSubClassOrImplementInterface(v_convertedType, v_acceptedType);
			}
		}
	}
	
	protected virtual System.Type GetAcceptedFilterTypeFromContainerType(System.Type p_type)
	{
		System.Type v_converted = p_type;
		if(p_type != null)
		{
			if(p_type.IsGenericType)
			{
				System.Type[] typeParameters = p_type.GetGenericArguments();
				if(typeParameters.Length == 1)
					v_converted = typeParameters[0];
				else
					v_converted = p_type; //Could not convert this to one single parameter type
			}
		}
		return v_converted;
	}
	
	#endregion
	
	#region Tools
	
	protected static List<string> GetStringList<T>(List<T> p_list)
	{
		List<string> v_stringList = new List<string>();
		if(p_list != null)
		{
			for(int i=0; i<p_list.Count; i++)
			{
				object v_object = p_list[i];
				string v_toString = "NULL";
				try
				{
					v_toString = v_object.ToString();
				}
				catch{
					v_toString = "NULL";
				}
				v_stringList.Add(v_toString);
			}
		}
		return v_stringList;
	}
	
	protected static bool IsSameOrSubclass(System.Type p_potentialDescendant , System.Type p_potentialBase)
	{
		if(p_potentialBase != null && p_potentialDescendant != null)
		{
			return p_potentialDescendant.IsSubclassOf(p_potentialBase)
				|| p_potentialDescendant == p_potentialBase;
		}
		return false;
	}
	
	protected static bool IsSameOrSubClassOrImplementInterface(System.Type p_potentialDescendant , System.Type p_potentialBase)
	{
		if(p_potentialBase != null && p_potentialDescendant != null)
		{
			bool v_sucess = p_potentialBase.IsAssignableFrom(p_potentialDescendant) || (new List<System.Type>(p_potentialDescendant.GetInterfaces())).Contains(p_potentialBase);
			if(!v_sucess)
				v_sucess = IsSameOrSubclass(p_potentialDescendant , p_potentialBase);
			return v_sucess;
		}
		return false;
	}
	
	#endregion
}
