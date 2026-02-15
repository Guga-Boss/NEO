using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ESpriteCol { NONE = -1, MAP_PLAY, MONSTER_ANIM, ITEM, BUILDING }

public class IDM: SerializedMonoBehaviour
{
    public static IDM I;

    [System.Serializable]
    public class AtlasCollection
    {
        [HorizontalGroup("Group"), HideLabel]
        public ESpriteCol CollectionType;

        [HorizontalGroup("Group"), HideLabel]
        public Texture2D Texture;

        [FoldoutGroup("Sprites de $CollectionType")]
        // Lista simples de Sprites. O Índice da lista = ID do tk2d
        public List<Sprite> Sprites = new List<Sprite>();

        [OdinSerialize, ReadOnly]
        [DictionaryDrawerSettings(KeyLabel = "ID", ValueLabel = "Sprite Asset")]
        public Dictionary<int, Sprite> SpritesDicById = new Dictionary<int, Sprite>();

        public void RebuildInternal()
        {
            SpritesDicById = new Dictionary<int, Sprite>();
            for( int i = 0; i < Sprites.Count; i++ )
            {
                if( Sprites[ i ] != null )
                    SpritesDicById[ i ] = Sprites[ i ];
            }
        }
    }

    [Title("Configurações das Coleções")]
    public List<AtlasCollection> Collections = new List<AtlasCollection>();

    [OdinSerialize, ReadOnly]
    public Dictionary<ESpriteCol, AtlasCollection> CollectionsDic = new Dictionary<ESpriteCol, AtlasCollection>();

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    public static void InitSingleton()
    {
        EditorApplication.delayCall += () =>
        {
            if( Application.isPlaying ) return;
            GameObject go = GameObject.Find("IDM");
            if( go != null ) I = go.GetComponent<IDM>();
        };
    }

    [Button( "1. Scan All Atlas", ButtonSizes.Large ), GUIColor( 0.4f, 0.8f, 1f )]
    private void ScanAllAtlas()
    {
        foreach( var col in Collections )
        {
            if( col.Texture == null ) continue;
            string path = AssetDatabase.GetAssetPath(col.Texture);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            // Lista temporária para podermos ordenar
            List<Sprite> tempSprites = new List<Sprite>();

            foreach( Object o in assets )
            {
                if( o is Sprite sp ) tempSprites.Add( sp );
            }

            // ORDENAÇÃO NATURAL (1, 2, 10, 100...)
            tempSprites.Sort( ( a, b ) => EditorUtility.NaturalCompare( a.name, b.name ) );

            col.Sprites.Clear();
            col.Sprites.AddRange( tempSprites );
        }
        Rebuild();
    }

    [Button( "2. Rebuild Dictionaries" )]
    public void Rebuild()
    {
        CollectionsDic = new Dictionary<ESpriteCol, AtlasCollection>();
        foreach( var col in Collections )
        {
            if( col.CollectionType == ESpriteCol.NONE ) continue;
            col.RebuildInternal();
            CollectionsDic[ col.CollectionType ] = col;
        }
        Debug.Log( "[IDM] Dicionários reconstruídos. Puro Sprite, sem lixo." );
    }
#endif

    public Sprite GetSpriteById( ESpriteCol colType, int id )
    {
        if( CollectionsDic.TryGetValue( colType, out var col ) )
        {
            if( col.SpritesDicById.TryGetValue( id, out var sp ) )
                return sp;
        }
        return null;
    }
    public Sprite GetSpriteFromIDMByName( ESpriteCol colType, string spriteName )
    {
        if( CollectionsDic.TryGetValue( colType, out var col ) )
        {
            // Busca na lista de sprites pelo nome
            return col.Sprites.Find( s => s != null && s.name == spriteName );
        }
        return null;
    }
    public Sprite GetSpriteAnywhere( string spriteName )
    {
        // Primeiro, garante que o dicionário está pronto
        if( CollectionsDic == null || CollectionsDic.Count == 0 ) Rebuild();

        foreach( var col in Collections )
        {
            // Procura em cada coleção cadastrada no IDM
            Sprite sp = col.Sprites.Find(s => s != null && s.name == spriteName);
            if( sp != null ) return sp;
        }
        return null;
    }
}