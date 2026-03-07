using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;
using static tk2dTileMapData;
using UnityEditor.Tilemaps;
using UnityEngine.SocialPlatforms;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MyTilemap: SerializedMonoBehaviour
{
    [Title("Configurações de Identificação")]
    public string targetTileName = "QuestTile";
    public List<string> FoldersToSearch = new List<string>();

    [Title("Configurações de Grid")]
    public bool RestrainDrawing = true;
    public int width;
    public int height;
    [Title("Referências")]
    public List<Tilemap> Tilemaps;
    public MyTilemapEditor TilemapEditor;

    [Title("Base de Dados (Bake)")]
    public Dictionary<int, TileBase> spriteToTileMap = new Dictionary<int, TileBase>();

    [ReadOnly]
    public Dictionary<Vector2Int, RandomMapData> questDataCache = new Dictionary<Vector2Int, RandomMapData>();

    public static bool IgnoreUpdate = false;

    // ===================================================================================
    // SISTEMA DE PROCESSAMENTO SEGURO (EDITOR)
    // ===================================================================================
#if UNITY_EDITOR
    [InitializeOnLoad]                                                                         // Ensures this class starts automatically in the Editor
    public static class MyTilemapEditorWatcher
    {
        static MyTilemapEditorWatcher()
        {
            Tilemap.tilemapTileChanged -= OnTileChanged;                                       // Safety clear to avoid duplicate events
            Tilemap.tilemapTileChanged += OnTileChanged;                                       // Hooks to the global tilemap painting event
        }

        private static void OnTileChanged( Tilemap tilemap, Tilemap.SyncTile[ ] changes )
        {
            if( Application.isPlaying || MyTilemap.IgnoreUpdate ) return;                      // Skips if game is running or updates are paused

            MyTilemap parentMap = tilemap.GetComponentInParent<MyTilemap>();                   // Finds the MyTilemap logic script
            if( parentMap == null || !parentMap.RestrainDrawing ) return;                      // Skips if this is not our custom map

            if( parentMap.gameObject.name.Contains( "Trans" ) ) return;                        // Skips transition maps

                                                                                               // delayCall replaces the heavy Update loop. It runs exactly ONCE right after you finish a brush stroke.
            EditorApplication.delayCall += () =>
            {
                if( parentMap == null || tilemap == null ) return;                             // Safety check if object was destroyed

                Tilemap.tilemapTileChanged -= OnTileChanged;                                   // Mutes the event to prevent infinite loops

                try
                {
                    foreach( var change in changes )
                    {
                        if( change.tile != null )
                            parentMap.ProcessPosition( tilemap, change.position );             // Processes only the painted tiles
                    }
                }
                finally
                {
                    Tilemap.tilemapTileChanged += OnTileChanged;                               // Restores the event listening
                }
            };
        }
    }

    public void ProcessPosition( Tilemap sourceMap, Vector3Int pos )
    {
        if( sourceMap == null ) return;                                                         // Safety check

                                                                                                // Regra de Borda
        if( pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height )
        {
            sourceMap.SetTile( pos, null );                                                     // Clears the tile if painted out of bounds
            return;                                                                             // Exits
        }

        TileBase tile = sourceMap.GetTile(pos);                                                 // Reads the painted tile from Unity
        if( tile == null ) return;                                                              // Exits if it was an erase action

        int tileID = GetGlobalIDFromSprite((tile as Tile)?.sprite);                             // Calculates global ID from sprite name

        if( tileID != -1 && Map.I != null )
        {
            ELayerType correctLayer = Map.GetTileLayer((ETileType)tileID);                      // Finds the target layer logic
            int layerIndex = (int)correctLayer;                                                 // Converts enum to array index

            if( correctLayer != ELayerType.DECOR && correctLayer != ELayerType.DECOR2 && 
                correctLayer != ELayerType.NONE && layerIndex < Tilemaps.Count )
            {
                if( Tilemaps[ layerIndex ] != sourceMap )
                {
                    sourceMap.SetTile( pos, null );                                             // Removes tile from the wrong layer
                    Tilemaps[ layerIndex ].SetTile( pos, tile );                                // Places tile in the correct layer automatically
                }
            }
        }
    }
#endif

    // ===================================================================================
    // MATEMÁTICA DE QUADRANTES (128x128)
    // ===================================================================================

    public int GetGlobalIDFromSprite( Sprite s )
    {
        if( s == null ) return -1;

        // 1. Extrair ID local do nome do sprite
        string spriteName = s.name;
        int underscore = spriteName.LastIndexOf('_');
        if( underscore == -1 || !int.TryParse( spriteName.Substring( underscore + 1 ), out int localID ) )
            return -1;

        // 2. Determinar Quadrante (p) através da Textura
        int p = 0;

        // Pegamos o nome da textura original (Ex: "Tiles 1")
        string texName = s.texture != null ? s.texture.name : "";

        // Checagem BLINDADA: Evita que o número do tile interfira
        if( texName.Contains( "Tile 2" ) || texName.Contains( "Tiles 2" ) ) p = 1;
        else if( texName.Contains( "Tile 3" ) || texName.Contains( "Tiles 3" ) ) p = 2;
        else if( texName.Contains( "Tile 4" ) || texName.Contains( "Tiles 4" ) ) p = 3;
        else p = 0; // Padrão é Quadrante 0 (Tiles 1)

        // 3. Coordenadas Locais
        int lx = localID % 64;
        int ly = localID / 64;

        // 4. Coordenadas Globais
        int gx = lx + (p % 2) * 64;
        int gy = ly + (p / 2) * 64;

        return ( gy * 128 ) + gx;
    }

    // ===================================================================================
    // BOTÕES E UTILITÁRIOS
    // ===================================================================================

    [Button( "Navigation Map", ButtonSizes.Gigantic ), GUIColor( 0, 1f, 0 )]
    public void LoadNavigatioMap()
    {
        //Load( Map.I.NavigationMap.Tilemap, Map.I.TM );

        MyTilemap myTilemap = Map.I.TM;
        if( myTilemap == null ) return;
        myTilemap.width = 128;
        myTilemap.height = 128;
    }

    [Button( "Update Tilemaps List" ), GUIColor( 0, 1f, 0 )]
    public void UpdateTileMapList()
    {
        Tilemaps = new List<Tilemap>( GetComponentsInChildren<Tilemap>() );
    }
    public void ChangePalette( string paletteName )
    {
        // 1. Procura o Asset da Paleta pelo nome 
        // Assume que suas paletas estão em uma pasta "Palettes" ou similar
        string[] guids = AssetDatabase.FindAssets(paletteName + " t:Prefab");

        if( guids.Length > 0 )
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            GameObject palettePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if( palettePrefab != null )
            {
                // 2. A MÁGICA: Define a paleta ativa no GridPaintingState
                GridPaintingState.palette = palettePrefab;

                // 3. Força a janela Tile Palette a atualizar a visualização
               // InspectorWindow.RepaintAllInspectors();

                Debug.Log( $"<color=green>[Master]</color> Paleta alterada para: {paletteName}" );
            }
        }
        else
        {
            Debug.LogWarning( $"[Master] Paleta '{paletteName}' não encontrada!" );
        }
    }


    [Button( "Update Trans Tilemap", ButtonSizes.Gigantic ), GUIColor( 1, 1f, 0 )]
    public void UpdateTrans()
    {
        ChangePalette( "Trans Tilemap" );
        ClearTilemap( Map.I.TransT );
        Map.I.UpdateTransLayerTilemap();
        Map.I.TransTilemapUpdateList = new List<VI>(); // reset list;
    }

    [Button( "Bake Tiles", ButtonSizes.Gigantic ), GUIColor( 0, 1f, 0 )]
    public void BakeTileReference()
    {
        spriteToTileMap = new Dictionary<int, TileBase>();

        // Filtro de pastas
        string[] paths = (FoldersToSearch != null && FoldersToSearch.Count > 0) ? FoldersToSearch.ToArray() : null;
        string[] guids = AssetDatabase.FindAssets("t:Tile", paths);

        foreach( string guid in guids )
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Tile t = AssetDatabase.LoadAssetAtPath<Tile>(path);

            if( t == null || t.sprite == null ) continue;

            // SEMPRE use a lógica de quadrantes para manter a compatibilidade 128x128
            int id = GetGlobalIDFromSprite(t.sprite);

            if( id != -1 && !spriteToTileMap.ContainsKey( id ) )
            {
                spriteToTileMap.Add( id, t );
            }
        }

        Debug.Log( $"<color=cyan><b>[BAKE]</b></color> Sucesso! {spriteToTileMap.Count} tiles mapeados. Tiles 1_0 agora é ID 0." );
        EditorUtility.SetDirty( this );
    }

    public List<Vector3Int>[] BatchPositions;
    public List<TileBase>[] BatchTiles;

    public void InitBatch()
    {
        if( BatchPositions == null || BatchPositions.Length != Tilemaps.Count )
        {
            BatchPositions = new List<Vector3Int>[ Tilemaps.Count ];
            BatchTiles = new List<TileBase>[ Tilemaps.Count ];

            for( int i = 0; i < Tilemaps.Count; i++ )
            {
                BatchPositions[ i ] = new List<Vector3Int>();                         // Initialize position buffer for each layer ;
                BatchTiles[ i ] = new List<TileBase>();                               // Initialize tile buffer for each layer ;
            }
        }
        else
        {
            for( int i = 0; i < Tilemaps.Count; i++ )
            {
                BatchPositions[ i ].Clear();                                // Clear lists to reuse existing capacity ;
                BatchTiles[ i ].Clear();                                    // Prevent reallocation by clearing data ;
            }
        }
    }
    public void FlushTiles()
    {
        if( BatchPositions == null ) return;                                                 // Early exit if buffers are null ;

        for( int i = 0; i < Tilemaps.Count; i++ )
        {
            if( Tilemaps[ i ] != null && BatchPositions[ i ].Count > 0 )
            {
                                                                                             // Core Optimization: Rebuilds mesh once per layer ;
                Tilemaps[ i ].SetTiles( BatchPositions[ i ].ToArray(), 
                BatchTiles[ i ].ToArray() );

                BatchPositions[ i ].Clear();                                                 // Reset buffers after execution ;
                BatchTiles[ i ].Clear();                                                     // Reset buffers after execution ;
            }
        }
    }

    [HideInInspector] 
    public bool UseArrayData = true;
    [HideInInspector]
    public int[,,] tileIdData;
    [HideInInspector]                                                                        // Logical storage for all IDs [layer, x, y]
    public bool[,,] visibleData;                                                             // Visibility flag for each tile
    internal int GetTile( int x, int y, int l )
    {
        if( l < 0 || l >= Tilemaps.Count ) return -1;                                        // Valida camada

        if( UseArrayData )
        if( Application.isPlaying )
            return tileIdData[ l, x, y ];                                                    // Retorna o ID lógico da camada (pode ser -1 se vazio)

        TileBase tile = Tilemaps[ l ].GetTile( new Vector3Int( x, y, 0 ) );                  // Pega o tile diretamente da Unity ;
       
        return TileToID( tile );                                                             // Usa o seu método TileToID que já faz a conversão de sprite para Global ID
    }
    internal void SetTile( int x, int y, int layer, int id, bool useBatch = false )
    { 
        if( UseArrayData )
        if( Application.isPlaying )
        {
            tileIdData[ layer, x, y ] = id;                                                   // Always store the ID in logic array                                                                                            
            bool isVisible = (id != -1);                                                      // False if empty
            if( layer == ( int ) ELayerType.GAIA && 
                Map.I.RM.InvisibleGaia( ( ETileType ) id ) )
                isVisible = false;                                                            // Force invisible for logic tiles

            visibleData[ layer, x, y ] = isVisible;                                           // Store visibility state

            if( id != -1 )
            if( isVisible == false ) return;                                                  // if invisible, skip rendering (but still save logic state)
        }

        if( layer < 0 || layer >= Tilemaps.Count ) return;
        if( useBatch )
        {
            BatchPositions[ layer ].Add( new Vector3Int( x, y, 0 ) );                         // Guarda a posição
            BatchTiles[ layer ].Add( id == -1 ? null : EnumToTile( id ) );                    // Guarda o Tile (se for -1, salva null para apagar o tile na Unity)
        }
        else
        {
            Tilemap tilemap = Tilemaps[layer];
            if( tilemap == null ) return;
            TileBase tile = EnumToTile(id);
            tilemap.SetTile( new Vector3Int( x, y, 0 ), tile );
        }
    }
    public static void ClearTilemap( MyTilemap mt )
    {
        if( mt.Tilemaps != null )
            foreach( var t in mt.Tilemaps )                                                   // Loops through all tilemaps and clears them in Unity (visual)
                if( t ) t.ClearAllTiles();
        mt.InitData( mt.width, mt.height );                                                   // Clears logical data and resets dimensions
    }

    public static void Load( tk2dTileMap tm, MyTilemap myTilemap )
    {
        IgnoreUpdate = true;
        if( Application.isPlaying ) return;
        myTilemap.width = tm.width;
        myTilemap.height = tm.height;

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
                    {
                        targetTilemap.SetTile( new Vector3Int( x, y, 0 ), tile );
                    }
                }
            }
        }
        Debug.Log( "<color=green>Load Concluído com Sincronia de Quadrantes!</color>" );
        //Map.I.TransT.UpdateTrans();
        IgnoreUpdate = false;
    }

    [Button( "Rebuild Quest Cache" ), GUIColor( 0, 0.8f, 1 )]
    public void RebuildQuestCache()
    {
        questDataCache = new Dictionary<Vector2Int, RandomMapData>();
        foreach( var data in Map.I.RM.RMList )
        {
            Vector2Int key = new Vector2Int( ( int ) data.MapCord.x, ( int ) data.MapCord.y ); // ajuste se sua coord for diferente
            if( !questDataCache.ContainsKey( key ) )
                questDataCache.Add( key, data );
        }
#if UNITY_EDITOR
        SceneView.RepaintAll();
#endif
        Debug.Log( $"<color=cyan>[QuestCache]</color> Rebuild completo: {questDataCache.Count} entradas." );
    }


    public void InitData( int w, int h )
    {
        int layers = Tilemaps.Count;                                       // Gets number of layers from the list
        width = w;                                                         // Updates internal width reference
        height = h;                                                        // Updates internal height reference

        if( UseArrayData == false ) return;
        tileIdData = new int[ layers, w, h ];                              // Allocates memory for tile IDs
        visibleData = new bool[ layers, w, h ];                            // Allocates memory for visibility flags
       
        for( int l = 0; l < layers; l++ )                                  // Manual loop is more reliable for 3D arrays [,,]
        for( int x = 0; x < w; x++ )
        for( int y = 0; y < h; y++ )
            {
             tileIdData[ l, x, y ] = -1;                                   // Sets default state to empty
             visibleData[ l, x, y ] = false;                               // Sets default visibility to hidden
            }
    }

    public TileBase EnumToTile( int type )
    {
        if( spriteToTileMap.TryGetValue( type, out TileBase tile ) )       
            return tile;        
        return null;
    }

    public int TileToID( TileBase tileBase )
    {
        Tile tile = tileBase as Tile;
        if( tile == null || tile.sprite == null ) return -1;
        return GetGlobalIDFromSprite( tile.sprite );
    }

    [Button( "Update Tilemaps List" )]
    public void Upd() => Tilemaps = new List<Tilemap>( GetComponentsInChildren<Tilemap>( true ) );
    internal Vector2 GetSize()
    {
        if( width == 30 )
        if( height == 30 ) { width = 29; height = 29; }    // temp fix for migration size bug
        return new Vector2( width, height );
    }
    public bool GetTileAtPosition( Vector3 position, out int x, out int y )
    {
        Vector3Int cellPosition = Tilemaps[0].WorldToCell(position);
        x = cellPosition.x;
        y = cellPosition.y;
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}