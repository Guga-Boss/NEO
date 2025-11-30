/* MinMaxRangeDrawer.cs
* by Eddie Cameron – For the public domain
* ———————————————————–
* — EDITOR SCRIPT : Place in a subfolder named ‘Editor’ —
* ———————————————————–
* Renders a MinMaxRange field with a MinMaxRangeAttribute as a slider in the inspector
* Can slide either end of the slider to set ends of range
* Can slide whole slider to move whole range
* Can enter exact range values into the From: and To: inspector fields
*
*/

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer( typeof( MinMaxRangeAttribute ) )]
public class MinMaxRangeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight( SerializedProperty property, GUIContent label )
    {
        return base.GetPropertyHeight( property, label ) + 16;
    }

    // Draw the property inside the given rect
    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        // Now draw the property as a Slider or an IntSlider based on whether it’s a float or integer.
        if( property.type != "MinMaxRange" )
            Debug.LogWarning( "Use only with MinMaxRange type" );
        else
        {
            var range = attribute as MinMaxRangeAttribute;
            var minValue = property.FindPropertyRelative( "rangeStart" );
            var maxValue = property.FindPropertyRelative( "rangeEnd" );
            var newMin = minValue.floatValue;
            var newMax = maxValue.floatValue;

            var xDivision = position.width * 0.33f;
            var yDivision = position.height * 0.5f;
            EditorGUI.LabelField( new Rect( position.x, position.y, xDivision, yDivision ), label );

            EditorGUI.LabelField( new Rect( position.x, position.y + yDivision, position.width, yDivision ), range.minLimit.ToString( "0.##" ) );
            
            float ff = position.x + position.width;
            ff = ff - 28f;
            Rect r1 = new Rect( ff, position.y + yDivision, position.width, yDivision );
            
            EditorGUI.LabelField( r1, range.maxLimit.ToString( "0.##" ) );

            float w = position.width;  w-=48;           

            Rect r = new Rect( position.x + 24f, position.y + yDivision, w, yDivision );


            EditorGUI.MinMaxSlider( r, ref newMin, ref newMax, range.minLimit, range.maxLimit );

           // EditorGUI.LabelField( new Rect( position.x + xDivision, position.y, xDivision, yDivision ), "From: " );

            float fff = xDivision;
            xDivision  = xDivision - 30;

            Rect rrr =new Rect( position.x + xDivision + 30, position.y, fff, yDivision );

            newMin = Mathf.Clamp( EditorGUI.FloatField( rrr, newMin ), range.minLimit, newMax );
            
            //EditorGUI.LabelField( new Rect( position.x + xDivision * 2f, position.y, xDivision, yDivision ), "To: " );

            float f2= xDivision;
            xDivision = xDivision - 24;        
            
            Rect rr = new Rect( position.x + xDivision * 2f + 24, position.y, f2, yDivision );
            
            newMax = Mathf.Clamp( EditorGUI.FloatField( rr, newMax ), newMin, range.maxLimit );

            minValue.floatValue = newMin;
            maxValue.floatValue = newMax;
        }
    }
}