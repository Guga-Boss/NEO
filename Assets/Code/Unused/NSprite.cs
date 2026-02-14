using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

public class NSprite: MonoBehaviour
{
    [BoxGroup("Configurações")]
    [OnValueChanged("OnColChanged")]
    public ESpriteCol collection;

    [BoxGroup("Configurações"), Required, OnValueChanged("UpdateVisuals")]
    [ValueDropdown("GetSpritesFromCol")]
    public Sprite sprite;

    [BoxGroup("Configurações")]
    [ReadOnly] public string spriteName;

    [BoxGroup("Configurações"), OnValueChanged("UpdateVisuals")]
    public Color baseColor = Color.white;

    public Color color
    {
        get => baseColor;
        set { baseColor = value; UpdateVisuals(); }
    }

    [BoxGroup("Layout")]
    [OnValueChanged("UpdateVisuals")]
    public Vector2 scale = Vector2.one;

    [BoxGroup("Layout")]
    [OnValueChanged("UpdateVisuals")]
    public bool preserveAspect = true;

    public SpriteRenderer Render;

#if UNITY_EDITOR
    // Altere a linha 38 para:
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if( this != null ) // Verifica se o objeto ainda existe
                UpdateVisuals();
        };
    }
#endif

    [BoxGroup("Layout")]
    [OnValueChanged("UpdateVisuals")]
    public int TkSpriteId = -1;

    private void OnColChanged()
    {
        sprite = null;
        spriteName = "";
        UpdateVisuals();
    }

    private IEnumerable<Sprite> GetSpritesFromCol()
    {
        if( IDM.I != null && collection != ESpriteCol.NONE )
        {
            if( IDM.I.CollectionsDic.TryGetValue( collection, out var col ) )
            {
                return col.Sprites;
            }
        }
        return null;
    }

    public void ApplyToRenderer( SpriteRenderer renderer )
    {
        if( renderer == null ) return;
        renderer.sprite = sprite;
        renderer.color = baseColor;
        UpdateVisuals();
    }

    private int _spriteId;
    public int spriteId
    {
        get => _spriteId;
        set
        {
            if( _spriteId == value ) return;
            _spriteId = value;

            if( IDM.I == null || collection == ESpriteCol.NONE ) return;

            Sprite sp = IDM.I.GetSpriteById(collection, _spriteId);
            if( sp != null )
            {
                sprite = sp;
                spriteName = sp.name;
                UpdateVisuals();
            }
        }
    }

    public void UpdateVisuals()
    {
        if( this == null || Render == null ) return; // previne chamadas em objetos destruído
        if( Render == null ) Render = GetComponent<SpriteRenderer>(); // bug aqui
        if( Render == null ) Render = gameObject.AddComponent<SpriteRenderer>();

        if( sprite == null ) return;

        Render.sprite = sprite;
        Render.color = baseColor;
        spriteName = sprite.name;

        Render.drawMode = SpriteDrawMode.Sliced;
        Vector2 nativeSize = sprite.bounds.size;

        if( preserveAspect && nativeSize.y != 0 )
        {
            float ratio = nativeSize.x / nativeSize.y;
            Render.size = new Vector2( nativeSize.x * scale.x, ( nativeSize.x * scale.x ) / ratio );
        }
        else
        {
            Render.size = new Vector2( nativeSize.x * scale.x, nativeSize.y * scale.y );
        }
    }

#if UNITY_EDITOR
    private static ESpriteCol GetCollectionFromTk2d( string tkColName )
    {
        string name = tkColName.ToUpper();
        if( name.Contains( "MONSTER" ) ) return ESpriteCol.MONSTER_ANIM;
        if( name.Contains( "ITEM" ) ) return ESpriteCol.ITEM;
        if( name.Contains( "BUILDING" ) ) return ESpriteCol.BUILDING;
        if( name.Contains( "MAP" ) || name.Contains( "PLAY" ) ) return ESpriteCol.MAP_PLAY;
        return ESpriteCol.NONE;
    }

    [InitializeOnLoadMethod]
    private static void InitMigrationActions()
    {
        Debug.Log( "🔌 [NSprite] Inicializando Ações de Migração (tk2d + NGUI)..." );

        // --- TK2D ACTIONS ---
        tk2dSpriteEditor.ConvertAction = ( obj ) =>
        {
            var sprite = obj as tk2dSprite;
            if( sprite != null ) NSprite.Convert( sprite );
        };

        tk2dSpriteEditor.FinalizeAction = ( obj ) =>
        {
            var sprite = obj as tk2dSprite;
            if( sprite != null ) NSprite.Finalize( sprite );
        };

        // --- NGUI ACTIONS (O que estava faltando!) ---
        // Usamos UIBasicSpriteEditor para cobrir UISprite e UI2DSprite de uma vez
        UIBasicSpriteEditor.ConvertUISAction = ( obj ) =>
        {
            if( obj != null ) NSprite.ConvertUISprite( obj );
        };

        UIBasicSpriteEditor.FinalizeUISAction = ( obj ) =>
        {
            if( obj != null ) NSprite.FinalizeUISprite( obj );
        };
    }

    public static void FinalizeUISprite( object obj )
    {
        var widget = obj as UIWidget;
        if( widget == null ) return;
        var root = widget.gameObject;

        // 1. Cache dos dados ORIGINAIS
        Vector3 originalTransformScale = root.transform.localScale;
        int originalW = widget.width;
        int originalH = widget.height;
        Color originalColor = widget.color;
        int originalDepth = widget.depth;

        // --- CACHE DO FLIP (NOVO) ---
        bool flipX = false;
        bool flipY = false;
        // UISprite e UI2DSprite herdam de UIBasicSprite, que contém a propriedade 'flip'
        if( obj is UIBasicSprite basicSpr )
        {
            var f = basicSpr.flip;
            flipX = ( f == UIBasicSprite.Flip.Horizontally || f == UIBasicSprite.Flip.Both );
            flipY = ( f == UIBasicSprite.Flip.Vertically || f == UIBasicSprite.Flip.Both );
        }
        // ----------------------------

        // Pega o Sprite do Preview temporário
        var child = root.transform.Find("Unity_Migration_Preview_UI");
        Sprite finalSprite = null;
        if( child != null )
        {
            var srChild = child.GetComponent<SpriteRenderer>();
            if( srChild != null ) finalSprite = srChild.sprite;
            Undo.DestroyObjectImmediate( child.gameObject );
        }

        UnityEditor.EditorApplication.delayCall += () =>
        {
            if( root == null ) return;

            // 2. Limpeza
            Undo.DestroyObjectImmediate( widget );
            var box = root.GetComponent<BoxCollider>();
            if( box != null ) Undo.DestroyObjectImmediate( box );

            // 3. Adiciona Novos Componentes
            var sr = root.AddComponent<SpriteRenderer>();
            var ns = root.AddComponent<NSprite>();

            // 4. RESTAURAÇÃO FIEL
            root.transform.localScale = originalTransformScale;

            // Configura NSprite
            ns.scale = new Vector2( originalW, originalH );
            ns.sprite = finalSprite;
            ns.baseColor = originalColor;

            // Se o seu NSprite tiver variáveis de flip, descomente abaixo:
            // ns.flipX = flipX;
            // ns.flipY = flipY;

            // Configura Renderer
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.sprite = finalSprite;
            sr.color = originalColor;
            sr.sortingOrder = originalDepth;
            sr.sortingLayerName = "Default";

            // --- APLICA O FLIP NO RENDERER ---
            sr.flipX = flipX;
            sr.flipY = flipY;
            // ---------------------------------

            ns.UpdateVisuals();

            // Atribuição Final do Size (Mantida no final para garantir)
            sr.size = new Vector2( ns.scale.x, ns.scale.y );

            UnityEditor.EditorUtility.SetDirty( root );
            Debug.Log( $"[Finalize] {root.name} Finalizado! Size: {originalW}x{originalH} | Flip: {flipX}/{flipY}" );
        };
    }

    public static void ConvertUISprite( object obj )
    {
        if( obj == null ) return;
        UIWidget widget = obj as UIWidget;
        if( widget == null ) return;

        Sprite sp = null;
        string sprName = "";
        ESpriteCol targetEnum = ESpriteCol.NONE;

        // 1. Identifica o Sprite
        if( obj is UISprite spr )
        {
            if( spr.atlas != null )
            {
                string colName = spr.atlas.name.Replace("Atlas", "").Trim();
                targetEnum = GetCollectionFromTk2d( colName ); // Ajuste conforme seu helper
                sprName = spr.spriteName;
                IDM.InitSingleton();
                sp = IDM.I.GetSpriteFromIDMByName( targetEnum, sprName );
            }
        }
        else if( obj is UI2DSprite spr2d )
        {
            sp = spr2d.sprite2D;
            sprName = ( sp != null ) ? sp.name : "Unknown";
            targetEnum = ESpriteCol.ITEM;
        }

        if( sp != null )
        {
            string childName = "Unity_Migration_Preview_UI";
            Transform childTransform = widget.transform.Find(childName);
            GameObject childGO = childTransform == null ? new GameObject(childName) : childTransform.gameObject;

            childGO.transform.SetParent( widget.transform );
            childGO.transform.localPosition = new Vector3( 0, 0, -0.1f ); // Levemente à frente
            childGO.transform.localRotation = Quaternion.identity;      // Zera rotação

            // --- CORREÇÃO DE TAMANHO (AQUI ESTAVA O ERRO) ---
            // Pegamos o tamanho que o Sprite tem "naturalmente" na Unity
            float spriteNativeW = sp.bounds.size.x;
            float spriteNativeH = sp.bounds.size.y;

            // Proteção contra divisão por zero
            if( spriteNativeW == 0 ) spriteNativeW = 1f;
            if( spriteNativeH == 0 ) spriteNativeH = 1f;

            // A escala deve esticar o sprite para bater com o tamanho do NGUI
            // Ex: Se NGUI é 100px e o Sprite nativo é 1.0 unidade -> Escala vira 100
            Vector3 finalScale = new Vector3(
            widget.width / spriteNativeW,
            widget.height / spriteNativeH,
            1f
        );

            childGO.transform.localScale = finalScale;
            // ------------------------------------------------

            // Garante componentes
            SpriteRenderer sr = childGO.GetComponent<SpriteRenderer>();
            if( sr == null ) sr = childGO.AddComponent<SpriteRenderer>();

            NSprite nsComp = childGO.GetComponent<NSprite>();
            if( nsComp == null ) nsComp = childGO.AddComponent<NSprite>();

            // Configura Renderer
            sr.sprite = sp;
            sr.sortingLayerName = "Default";
            sr.gameObject.layer = 5;
            sr.sortingOrder = widget.depth;
            sr.color = widget.color;

            // Configura NSprite
            nsComp.collection = targetEnum;
            nsComp.sprite = sp;
            nsComp.baseColor = widget.color;
            nsComp.spriteName = sprName;
            // Salva o tamanho original em pixels, caso seu script use isso depois
            nsComp.scale = new Vector2( widget.width, widget.height );

            // IMPORTANTE: Se o seu UpdateVisuals() resetar a escala, o bug volta.
            // Se isso acontecer, comente essa linha abaixo temporariamente.
            nsComp.UpdateVisuals();

            // Agora sim: esconde o original (que está intacto por trás)
            widget.enabled = true;
            childGO.SetActive( true );

            Debug.Log( $"[Migração] Preview ajustado! NGUI: {widget.width}x{widget.height} | Escala: {finalScale}" );
        }
    }
    public static void Convert( tk2dSprite tkSprite )
    {
        if( tkSprite == null || tkSprite.Collection == null ) return;

        string colNameTk = tkSprite.Collection.spriteCollectionName.Replace("Sprite Collection", "").Trim();
        ESpriteCol targetEnum = GetCollectionFromTk2d(colNameTk);
        int targetId = tkSprite.spriteId;

        IDM.InitSingleton();
        if( IDM.I == null ) return;

        Sprite sp = IDM.I.GetSpriteById(targetEnum, targetId);

        if( sp != null )
        {
            Vector2 copiedScale = (Vector2)tkSprite.scale;
            Color instanceColor = tkSprite.color;

            string childName = "Unity_Migration_Preview";
            Transform childTransform = tkSprite.transform.Find(childName);
            GameObject childGO;

            if( childTransform == null )
            {
                childGO = new GameObject( childName );
                childGO.transform.SetParent( tkSprite.transform );
                childGO.transform.localRotation = Quaternion.identity;
                childGO.transform.localScale = Vector3.one;
                childGO.transform.localPosition = new Vector3( 0, 0, -0.01f );
            }
            else { childGO = childTransform.gameObject; }

            NSprite nsComp = childGO.GetComponent<NSprite>() ?? childGO.AddComponent<NSprite>();

            nsComp.collection = targetEnum;
            nsComp._spriteId = targetId;
            nsComp.sprite = sp;
            nsComp.baseColor = instanceColor;
            nsComp.spriteName = sp.name;
            nsComp.scale = copiedScale;

            SpriteRenderer sr = childGO.GetComponent<SpriteRenderer>() ?? childGO.AddComponent<SpriteRenderer>();
            sr.sortingOrder = tkSprite.SortingOrder;
            sr.sortingLayerName = "Default";
            sr.color = instanceColor;

            nsComp.UpdateVisuals();
            tkSprite.enabled = true;
        }
    }

    [Button( "FINAL CONVERT (INSTANCE ONLY)" ), GUIColor( 0, 0.8f, 1f )]
    public static void Finalize( tk2dSprite tkSprite )
    {
        if( tkSprite == null || tkSprite.Collection == null ) return;

        GameObject mainGO = tkSprite.gameObject;
        IDM.InitSingleton();
        if( IDM.I == null ) return;

        string colNameTk = tkSprite.Collection.spriteCollectionName.Replace("Sprite Collection", "").Trim();
        ESpriteCol targetEnum = GetCollectionFromTk2d(colNameTk);
        int targetId = tkSprite.spriteId;

        Sprite sp = IDM.I.GetSpriteById(targetEnum, targetId);
        if( sp == null ) return;

        Color colorToUse = tkSprite.color;
        Transform previewTransform = mainGO.transform.Find("Unity_Migration_Preview");
        if( previewTransform != null )
        {
            NSprite previewNS = previewTransform.GetComponent<NSprite>();
            if( previewNS != null ) colorToUse = previewNS.baseColor;
            Undo.DestroyObjectImmediate( previewTransform.gameObject );
        }

        Vector2 sc = (Vector2)tkSprite.scale;
        int so = tkSprite.SortingOrder;

        EditorApplication.delayCall += () =>
        {
            if( mainGO == null ) return;

            // --- CORREÇÃO DO ERRO DE UNPACK ---
            if( PrefabUtility.IsPartOfPrefabInstance( mainGO ) )
            {
                // Busca a raiz mais próxima da instância do prefab para poder dar o Unpack
                GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(mainGO);
                if( root != null )
                {
                    PrefabUtility.UnpackPrefabInstance( root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction );
                }
            }

            // Agora a remoção será limpa, sem deixar o texto (Removed)
            var tk = mainGO.GetComponent<tk2dSprite>();
            var mr = mainGO.GetComponent<MeshRenderer>();
            var mf = mainGO.GetComponent<MeshFilter>();

            if( tk != null ) Undo.DestroyObjectImmediate( tk );
            if( mr != null ) Undo.DestroyObjectImmediate( mr );
            if( mf != null ) Undo.DestroyObjectImmediate( mf );

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript( mainGO );

            // --- RESTANTE MANTIDO IGUAL ---
            mainGO.transform.localScale = Vector3.one;

            SpriteRenderer newSR = mainGO.AddComponent<SpriteRenderer>();
            NSprite newNS = mainGO.AddComponent<NSprite>();

            newSR.sortingOrder = so;
            newSR.sortingLayerName = "Default"; // Conforme sua regra de ouro
            newSR.sprite = sp;
            newSR.color = colorToUse;

            newNS.Render = newSR;
            newNS.collection = targetEnum;
            newNS._spriteId = targetId;
            newNS.sprite = sp;
            newNS.spriteName = sp.name;
            newNS.baseColor = colorToUse;
            newNS.scale = sc;

            newNS.UpdateVisuals();

            EditorUtility.SetDirty( mainGO );
            Debug.Log( $"[NSprite] Deep Clean OK em: {mainGO.name}" );
        };
    }
#endif
}