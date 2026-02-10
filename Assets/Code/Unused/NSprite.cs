using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

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

    private void OnValidate() => UpdateVisuals();

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
        if( Render == null ) Render = GetComponent<SpriteRenderer>();
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