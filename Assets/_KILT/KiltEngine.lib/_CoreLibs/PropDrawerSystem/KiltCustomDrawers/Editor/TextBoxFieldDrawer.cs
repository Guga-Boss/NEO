using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(TextBoxFieldAttribute))]
public class TextBoxFieldDrawer : SpecificFieldDrawer {

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		TextBoxFieldAttribute attr = attribute as TextBoxFieldAttribute;
		return attr != null? attr.GuiHeight : EditorGUI.GetPropertyHeight(property, label, true);
	}

	protected override void DrawComponent(Rect position, SerializedProperty property, GUIContent label, System.Type p_type)
	{
		if(property.propertyType == SerializedPropertyType.String)
		{
			Vector2 v_offset = new Vector2(20, 2);
			Rect v_labelAreaPos = new Rect(position.x, position.y, position.width, EditorGUI.GetPropertyHeight(property, label, true));
			Rect v_textAreaPos = new Rect(position.x + v_offset.x, position.y + v_labelAreaPos.height + v_offset.y, position.width - v_offset.x, GetPropertyHeight(property, label) - v_labelAreaPos.height - v_offset.y);

			EditorGUI.LabelField(v_labelAreaPos, label);
			EditorGUI.BeginProperty (v_textAreaPos, label, property);
			
			// add word wrap to style.
			GUIStyle v_style = new GUIStyle (EditorStyles.textField);
			v_style.wordWrap = true;
			
			// show the text area.
			EditorGUI.BeginChangeCheck ();
			string v_input = EditorGUI.TextArea (v_textAreaPos, property.stringValue, v_style);
			if (EditorGUI.EndChangeCheck ()) {
				property.stringValue = v_input;
			}
			EditorGUI.EndProperty ();
		}
		else
			EditorGUI.PropertyField(position, property, label, true);
	}
}
