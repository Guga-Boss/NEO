using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor customizado para UI2DSprite com suporte à migração NSprite.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor( typeof( UI2DSprite ), true )]
public class UI2DSpriteEditor: UIBasicSpriteEditor
{
    UI2DSprite mSprite;

    protected override void OnEnable()
    {
        base.OnEnable();
        mSprite = target as UI2DSprite;
    }

    /// <summary>
    /// Sobrescrevemos o OnInspectorGUI para garantir que nossos botões apareçam
    /// mesmo se o NGUI travar o desenho interno por causa de estilos antigos.
    /// </summary>
    public override void OnInspectorGUI()
    {
        // 1. Tenta desenhar o padrão do NGUI (Widget, Anchor, etc.)
        base.OnInspectorGUI();

        // 2. Desenha os nossos botões de migração
        DrawCustomProperties();
    }

    protected override void DrawCustomProperties()
    {
        // Se o base.DrawCustomProperties() der erro de estilo, o try/catch evita travar tudo
        try { base.DrawCustomProperties(); } catch { }

        GUILayout.Space( 12 );

        // --- BLOCO DE MIGRAÇÃO ---
        Color oldColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color( 0f, 1f, 0.5f ); // Verde neon para destacar

        GUILayout.BeginVertical( "box" );
        GUILayout.Space( 6 );

        GUIStyle bigButton = new GUIStyle(GUI.skin.button)
        {
            fontSize = 15,
            fixedHeight = 45,
            fontStyle = FontStyle.Bold
        };

        if( mSprite != null )
        {
            // Título para diferenciar do UISprite (Atlas)
            EditorGUILayout.LabelField( "MIGRAÇÃO UI2D (NATIVE SPRITE)", EditorStyles.centeredGreyMiniLabel );
            GUILayout.Space( 4 );

            if( GUILayout.Button( "🔥 CONVERT UI2D TO NSPRITE 🔥", bigButton ) )
            {
                if( ConvertUISAction != null )
                {
                    // Passamos o mSprite diretamente
                    ConvertUISAction.Invoke( mSprite );
                }
                else
                {
                    Debug.LogError( "[NSprite] ConvertUISAction não está inicializada! Verifique o MigrationLinker." );
                }
            }

            if( GUILayout.Button( "🔥 Finalize Migration 🔥", bigButton ) )
            {
                if( FinalizeUISAction != null )
                {
                    FinalizeUISAction.Invoke( mSprite );
                }
            }
        }

        GUILayout.Space( 6 );
        GUILayout.EndVertical();
        GUI.backgroundColor = oldColor;
    }

 
    protected override bool ShouldDrawProperties()
    {
        // Mantemos a lógica original do NGUI para desenhar o campo de Sprite 2D
        GUI.changed = false;
        SerializedProperty sp = NGUIEditorTools.DrawProperty("2D Sprite", serializedObject, "mSprite");

        if( GUI.changed )
        {
            Sprite sprite = sp.objectReferenceValue as Sprite;
            if( sprite != null )
            {
                SerializedProperty border = serializedObject.FindProperty("mBorder");
                border.vector4Value = sprite.border;
            }
        }

        NGUISettings.sprite2D = sp.objectReferenceValue as Sprite;
        NGUIEditorTools.DrawProperty( "Material", serializedObject, "mMat" );

        if( mSprite.material == null || serializedObject.isEditingMultipleObjects )
        {
            NGUIEditorTools.DrawProperty( "Shader", serializedObject, "mShader" );
        }

        NGUIEditorTools.DrawProperty( "Pixel Size", serializedObject, "mPixelSize" );

        SerializedProperty fa = serializedObject.FindProperty("mFixedAspect");
        bool before = fa.boolValue;
        NGUIEditorTools.DrawProperty( "Fixed Aspect", fa );

        if( fa.boolValue != before )
            ( target as UIWidget ).drawRegion = new Vector4( 0f, 0f, 1f, 1f );

        return ( sp.objectReferenceValue != null );
    }
}