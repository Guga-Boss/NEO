using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MyTilemap: SerializedMonoBehaviour
{
    [Tooltip("Nome do tile que vai gerar o botão")]
    public string targetTileName = "QuestTile";
    public List<string> FoldersToSearch;

    [Header("Borda e Grid")]
    public bool RestrainDrawing = false;
    public Vector2Int GridSize = new Vector2Int(29, 29);

    [Header("Tilemaps")]
    public List<Tilemap> Tilemaps;

    public MyTilemapEditor TilemapEditor;

    // Agora o dicionário usa int como chave
    [Header("Base de Dados de Tiles")]
    [DictionaryDrawerSettings(KeyLabel = "Tile ID (int)", ValueLabel = "Tile Asset")]
    public Dictionary<int, TileBase> spriteToTileMap = new Dictionary<int, TileBase>();

    [Button( "Navigation Map", ButtonSizes.Gigantic ), GUIColor( 0, 1f, 0 )]
    public void LoadNavigatioMap()
    {
        Load( Map.I.NavigationMap.Tilemap, Map.I.TM );

        MyTilemap myTilemap = Map.I.TM;
        if( myTilemap == null ) return;

        myTilemap.TilemapEditor.gridSize = new Vector2Int( 128, 128 );
        myTilemap.GridSize = new Vector2Int( 128, 128 );
    }

    [Button( "Update Tilemaps List" ), GUIColor( 0, 1f, 0 )]
    public void UpdateTileMapList()
    {
        Tilemaps = new List<Tilemap>( GetComponentsInChildren<Tilemap>() );
    }

    [Button( "Update Trans Tilemap", ButtonSizes.Gigantic ), GUIColor( 1, 1f, 0 )]
    public void UpdateTrans()
    {
        ClearTilemap( Map.I.TransT );
        Map.I.UpdateTransLayerTilemap();
        Map.I.TransTilemapUpdateList = new List<VI>(); // reset list;
    }

    [ReadOnly] public Dictionary<Vector2Int, RandomMapData> questDataCache = new Dictionary<Vector2Int, RandomMapData>();

    [Button( "Update Quest Names", ButtonSizes.Gigantic ), GUIColor( 0, 1f, 0 )]
    public void UpdateQuestNames()
    {
        RandomMap rm = MapSaver.Get().GetRM();
        questDataCache.Clear();

        foreach( RandomMapData rmd in rm.RMList )
        {
            Vector2Int coord = new Vector2Int((int)rmd.MapCord.x, (int)rmd.MapCord.y);
            if( !questDataCache.ContainsKey( coord ) )
            {
                questDataCache.Add( coord, rmd );
            }
        }
        Debug.Log( $"Cache atualizado: {questDataCache.Count} quests mapeadas." );
    }

    // ===================================================================================
    // LÓGICA CORE (Baseada no TilePaletteWatcher)
    // ===================================================================================
    private int GetGlobalIDFromSprite( Sprite sprite )
    {
        if( sprite == null ) return -1;

        Rect r = sprite.textureRect;

        const int tileW = 64;
        const int textureSize = 4096;
        const int totalMapWidth = 128;

        float scaleFactor = (float)textureSize / sprite.texture.width;

        float realX = r.x * scaleFactor;
        float realY = r.y * scaleFactor;

        float epsilon = 0.1f;
        int colX = Mathf.FloorToInt((realX + epsilon) / tileW);
        int rowBottomUp = Mathf.FloorToInt((realY + epsilon) / tileW);

        int rowTopDown = 63 - rowBottomUp;

        int pngIndex = 0;
        string texName = sprite.texture.name;

        if( texName.Contains( "Tile 2" ) ) pngIndex = 1;
        else if( texName.Contains( "Tile 3" ) ) pngIndex = 2;
        else if( texName.Contains( "Tile 4" ) ) pngIndex = 3;

        int offsetX = (pngIndex % 2) * 64;
        int offsetY = (pngIndex / 2) * 64;

        int globalX = colX + offsetX;
        int globalY = rowTopDown + offsetY;

        int globalID = (globalY * totalMapWidth) + globalX;

        return globalID;
    }

    // ===================================================================================
    // BAKE (Gera o Dicionário)
    // ===================================================================================
    [Button( "Bake Tiles", ButtonSizes.Gigantic ), GUIColor( 0, 1f, 0 )]
    public void BakeTileReference()
    {
        spriteToTileMap = new Dictionary<int, TileBase>();

        string[] guids = AssetDatabase.FindAssets("t:Tile", FoldersToSearch.ToArray<string>());

        bool usequadrant = true;
        if( gameObject.name.Contains( "Trans" ) )
            usequadrant = false;



        foreach( string guid in guids )
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Tile tileAsset = AssetDatabase.LoadAssetAtPath<Tile>(path);

            if( tileAsset == null || tileAsset.sprite == null ) continue;

            int globalID;
            if(  usequadrant== false ) 
            {
                // IDs sequenciais para TransTilemap
                globalID = spriteToTileMap.Count;
            }
            else
            {
                // Lógica normal do Watcher
                globalID = GetGlobalIDFromSprite( tileAsset.sprite );
            }

            if( globalID != -1 && !spriteToTileMap.ContainsKey( globalID ) )
            {
                spriteToTileMap.Add( globalID, tileAsset );
            }
        }

        Debug.Log( $"<color=cyan><b>[BAKE]</b></color> Sucesso! {spriteToTileMap.Count} tiles mapeados usando a lógica do Watcher." );
        EditorUtility.SetDirty( this );
    }

    // ===================================================================================
    // EDITOR UPDATE (Troca Layer Automática)
    // ===================================================================================
#if UNITY_EDITOR
    private void OnEnable() => EditorApplication.update += EditorUpdate;
    private void OnDisable() => EditorApplication.update -= EditorUpdate;

    private void EditorUpdate()
    {
        return;
        if( !RestrainDrawing || Tilemaps == null ) return;
        if( this == null || gameObject == null ) return;

        for( int i = Tilemaps.Count - 1; i >= 0; i-- )
        {
            var tilemap = Tilemaps[i];
            if( tilemap == null ) continue;

            try
            {
                BoundsInt bounds = tilemap.cellBounds;
                foreach( var pos in bounds.allPositionsWithin )
                {
                    if( !tilemap.HasTile( pos ) ) continue;

                    if( pos.x < 0 || pos.x >= GridSize.x || pos.y < 0 || pos.y >= GridSize.y )
                    {
                        tilemap.SetTile( pos, null );
                        continue;
                    }

                    TileBase tile = tilemap.GetTile(pos);
                    if( tile == null ) continue;

                    int tileID = GetGlobalIDFromSprite((tile as Tile)?.sprite);

                    if( tileID != -1 )
                    {
                        ELayerType correctLayer = Map.GetTileLayer((ETileType)tileID);
                        if( correctLayer != ELayerType.DECOR && correctLayer != ELayerType.DECOR2 )
                        if( correctLayer != ELayerType.NONE )
                        {
                            int layerIndex = (int)correctLayer;
                            if( layerIndex < Tilemaps.Count && Tilemaps[ layerIndex ] != tilemap )
                            {
                                tilemap.SetTile( pos, null );
                                Tilemaps[ layerIndex ].SetTile( pos, tile );
                            }
                        }
                    }
                }
            }
            catch( MissingReferenceException ) { return; }
        }
    }
#endif

    public TileBase EnumToTile( int type )
    {
        if( spriteToTileMap.TryGetValue( type, out TileBase tile ) )
        {
            //Debug.Log( tile + "  " + spriteToTileMap.Count + "  " + name );
            return tile;
        }
        return null;
    }

    public int TileToID( TileBase tileBase )
    {
        Tile tile = tileBase as Tile;
        if( tile == null || tile.sprite == null ) return -1;
        return GetGlobalIDFromSprite( tile.sprite );
    }

    // ===================================================================================
    // LOAD (Tk2d -> Unity) - CORRIGIDO
    // ===================================================================================
    public static void Load( tk2dTileMap tm, MyTilemap myTilemap )
    {
        myTilemap.TilemapEditor.gridSize = new Vector2Int( tm.width, tm.height );
        myTilemap.GridSize = new Vector2Int( tm.width, tm.height );

        ClearTilemap( myTilemap );

        const int totalMapWidth = 128;

        for( int l = 0; l < tm.data.NumLayers; l++ )
        {
            if( l >= myTilemap.Tilemaps.Count ) break;
            Tilemap targetTilemap = myTilemap.Tilemaps[l];
            if( targetTilemap == null ) continue;

            for( int y = 0; y < tm.height; y++ )
            {
                for( int x = 0; x < tm.width; x++ )
                {
                    int tk2dRawID = tm.GetTile(x, y, l);
                    if( tk2dRawID < 0 ) continue;

                    int col = tk2dRawID % 128;
                    int row = tk2dRawID / 128;

                    int qX = col / 64;
                    int qY = row / 64;
                    int pngIndex = qX + (qY * 2);

                    int localCol = col % 64;
                    int localRow = row % 64;

                    int offsetX = (pngIndex % 2) * 64;
                    int offsetY = (pngIndex / 2) * 64;

                    int finalX = localCol + offsetX;
                    int finalY = localRow + offsetY;

                    int globalID = (finalY * totalMapWidth) + finalX;

                    TileBase tile = myTilemap.EnumToTile(globalID);

                    if( tile != null )
                        targetTilemap.SetTile( new Vector3Int( x, y, 0 ), tile );
                }
            }
        }
        Debug.Log( "<color=green>Load Concluído com Sincronia de Quadrantes!</color>" );
        myTilemap.UpdateTrans();
    }

    private static void ClearTilemap( MyTilemap myTilemap )
    {
        foreach( var tmap in myTilemap.Tilemaps )
            if( tmap != null ) tmap.ClearAllTiles();
    }

    internal void SetTile( int x, int y, int layer, int tk2dID )
    {
        if( layer < 0 || layer >= Tilemaps.Count ) return;
        Tilemap tilemap = Tilemaps[layer];
        if( tilemap == null ) return;

        TileBase tile = EnumToTile(tk2dID);
        tilemap.SetTile( new Vector3Int( x, y, 0 ), tile );

//#if UNITY_EDITOR
//        // ISSO AQUI força a Unity a redesenhar o que o script acabou de fazer
//        if( !Application.isPlaying )
//        {
//            tilemap.RefreshTile( new Vector3Int( x, y, 0 ) );
//            EditorUtility.SetDirty( tilemap );
//        }
//#endif
    }
}
