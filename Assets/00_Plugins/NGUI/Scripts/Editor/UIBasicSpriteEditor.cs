//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UITextures.
/// </summary>

[CanEditMultipleObjects]
[CustomEditor(typeof(UIBasicSprite), true)]
public class UIBasicSpriteEditor : UIWidgetInspector
{
	/// <summary>
	/// Draw all the custom properties such as sprite type, flip setting, fill direction, etc.
	/// </summary>

	protected override void DrawCustomProperties ()
	{
		GUILayout.Space(6f);

		SerializedProperty sp = NGUIEditorTools.DrawProperty("Type", serializedObject, "mType", GUILayout.MinWidth(20f));

		UISprite.Type type = (UISprite.Type)sp.intValue;

		if (type == UISprite.Type.Simple)
		{
			NGUIEditorTools.DrawProperty("Flip", serializedObject, "mFlip");
		}
		else if (type == UISprite.Type.Tiled)
		{
			NGUIEditorTools.DrawBorderProperty("Trim", serializedObject, "mBorder");
			NGUIEditorTools.DrawProperty("Flip", serializedObject, "mFlip");
		}
		else if (type == UISprite.Type.Sliced)
		{
			NGUIEditorTools.DrawBorderProperty("Border", serializedObject, "mBorder");
			NGUIEditorTools.DrawProperty("Flip", serializedObject, "mFlip");

			EditorGUI.BeginDisabledGroup(sp.hasMultipleDifferentValues);
			{
				sp = serializedObject.FindProperty("centerType");
				bool val = (sp.intValue != (int)UISprite.AdvancedType.Invisible);

				if (val != EditorGUILayout.Toggle("Fill Center", val))
				{
					sp.intValue = val ? (int)UISprite.AdvancedType.Invisible : (int)UISprite.AdvancedType.Sliced;
				}
			}
			EditorGUI.EndDisabledGroup();
		}
		else if (type == UISprite.Type.Filled)
		{
			NGUIEditorTools.DrawProperty("Flip", serializedObject, "mFlip");
			NGUIEditorTools.DrawProperty("Fill Dir", serializedObject, "mFillDirection", GUILayout.MinWidth(20f));
			GUILayout.BeginHorizontal();
			GUILayout.Space(4f);
			NGUIEditorTools.DrawProperty("Fill Amount", serializedObject, "mFillAmount", GUILayout.MinWidth(20f));
			GUILayout.Space(4f);
			GUILayout.EndHorizontal();
			NGUIEditorTools.DrawProperty("Invert Fill", serializedObject, "mInvert", GUILayout.MinWidth(20f));
		}
		else if (type == UISprite.Type.Advanced)
		{
			NGUIEditorTools.DrawBorderProperty("Border", serializedObject, "mBorder");
			NGUIEditorTools.DrawProperty("  Left", serializedObject, "leftType");
			NGUIEditorTools.DrawProperty("  Right", serializedObject, "rightType");
			NGUIEditorTools.DrawProperty("  Top", serializedObject, "topType");
			NGUIEditorTools.DrawProperty("  Bottom", serializedObject, "bottomType");
			NGUIEditorTools.DrawProperty("  Center", serializedObject, "centerType");
			NGUIEditorTools.DrawProperty("Flip", serializedObject, "mFlip");
		}

        //GUI.changed = false;
        //Vector4 draw = EditorGUILayout.Vector4Field("Draw Region", mWidget.drawRegion);

        //if (GUI.changed)
        //{
        //    NGUIEditorTools.RegisterUndo("Draw Region", mWidget);
        //    mWidget.drawRegion = draw;
        //}



        GUILayout.Space( 12 );
        Color oldColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.red;

        GUILayout.BeginVertical( "box" );
        GUILayout.Space( 6 );

        // --- DIAGNÓSTICO ---
        // Isso vai te dizer no console se é UISprite, UITexture ou outro
        string typeName = target.GetType().Name;

        GUIStyle bigButton = new GUIStyle(GUI.skin.button);
        bigButton.fontSize = 15;
        bigButton.fixedHeight = 45;
        bigButton.fontStyle = FontStyle.Bold;

        // Tenta pegar como Sprite ou Texture (ambos são UIBasicSprite)
        UIBasicSprite basicSpr = target as UIBasicSprite;

		if( basicSpr != null )
		{
			if( GUILayout.Button( $"🔥 CONVERT {typeName.ToUpper()} 🔥", bigButton ) )
			{
				// Se for UISprite, chama sua ação atual
				if( target is UISprite spr )
				{
					if( ConvertUISAction != null ) ConvertUISAction.Invoke( spr );
					else Debug.LogError( "ConvertUISAction não configurada!" );
				}
				// Se for UITexture, o processo é levemente diferente (não tem Atlas)
				else if( target is UITexture tex )
				{
					Debug.LogWarning( $"[Aviso] {tex.name} é um UITexture. O ConvertUISprite atual espera um Atlas. Deseja converter texturas também?" );
					// Se quiser converter texturas, precisaremos de um NSprite.ConvertUITexture(tex)
				}
			}

			if( GUILayout.Button( "🔥 Finalize 🔥", bigButton ) )
			{
				if( FinalizeUISAction != null ) FinalizeUISAction.Invoke( target );
				else Debug.LogError( "FinalizeUISAction não configurada!" );
			}
		}

		GUILayout.Space( 6 );
        GUILayout.EndVertical();
        GUI.backgroundColor = oldColor;



        base.DrawCustomProperties();
	}
    // DECLARE AQUI (Linha 15 aproximadamente):
    public static System.Action<object> ConvertUISAction;
    public static System.Action<object> FinalizeUISAction;
}
