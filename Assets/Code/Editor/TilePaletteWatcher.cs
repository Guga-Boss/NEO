using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilePaletteWatcher: EditorWindow
{
    private TileBase lastSelectedTile = null;
    private Vector3Int lastGridPos = new Vector3Int(-999, -999, -999);
    private Type paintingStateType;

    [SerializeField] private MyTilemap myTilemap;

    [MenuItem( "Tools/Tilemap Master Watcher" )]
    public static void ShowWindow() => GetWindow<TilePaletteWatcher>( "Tile Master" ).Show();

    private void OnEnable()
    {
        // 1. Localiza o componente core
        if( myTilemap == null )
            myTilemap = FindFirstObjectByType<MyTilemap>();

        // 2. Reflexão para ler a Tile Palette (GridPaintingState)
        paintingStateType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany( a => a.GetTypes() )
            .FirstOrDefault( t => t.Name == "GridPaintingState" );

        // 3. Inscrição nos eventos de atualização
        EditorApplication.update += EditorUpdate;
        SceneView.duringSceneGui += OnSceneGUI; // Fundamental para os botões aparecerem
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void EditorUpdate()
    {
        if( paintingStateType == null ) return;

        ExtractBrushData( out TileBase selectedTile, out Vector3Int gridPos );

        // Só executa lógica se o tile selecionado mudar
        if( selectedTile != lastSelectedTile || gridPos != lastGridPos )
        {
            lastSelectedTile = selectedTile;
            lastGridPos = gridPos;
            if( selectedTile != null ) ProcessSelectedTile( selectedTile );
        }

        if( focusedWindow == this ) Repaint();
    }

    // ===================================================================================
    // VISUALIZAÇÃO NA SCENE (OS BOTÕES)
    // ===================================================================================
    private void OnSceneGUI( SceneView sceneView )
    {
        // 1. Validação de segurança
        if( myTilemap == null ) myTilemap = FindFirstObjectByType<MyTilemap>();
        if( myTilemap == null || myTilemap.Tilemaps == null ) return;

        Handles.BeginGUI();

        foreach( var tilemap in myTilemap.Tilemaps )
        {
            if( tilemap == null ) continue;

            // Percorre os tiles existentes no mapa
            foreach( var pos in tilemap.cellBounds.allPositionsWithin )
            {
                TileBase tile = tilemap.GetTile(pos);
                if( tile == null ) continue;

                // 2. Verifica se o tile é um alvo (ex: QuestTile)
                if( tile.name.Contains( myTilemap.targetTileName ) )
                {
                    Vector3 worldPos = tilemap.GetCellCenterWorld(pos);
                    Vector2 guiPos = HandleUtility.WorldToGUIPoint(worldPos);

                    Vector2Int coordKey = new Vector2Int(pos.x, pos.y);
                    string label = "Quest";
                    GameObject targetToSelect = null;

                    // Busca os dados completos no novo dicionário
                    if( myTilemap.questDataCache != null && myTilemap.questDataCache.TryGetValue( coordKey, out RandomMapData data ) )
                    {
                        label = data.QuestHelper.QuestName;
                        targetToSelect = data.gameObject; // O Prefab/GameObject que você quer selecionar
                    }

                    Rect rect = new Rect(guiPos.x - 40, guiPos.y - 12, 80, 25);

                    GUI.backgroundColor = targetToSelect != null ? Color.green : Color.red;

                    if( GUI.Button( rect, label ) )
                    {
                        if( targetToSelect != null )
                        {
                            // --- A MÁGICA ACONTECE AQUI ---
                            Selection.activeGameObject = targetToSelect;
                            EditorGUIUtility.PingObject( targetToSelect ); // Faz o objeto "piscar" no Project/Hierarchy

                            Debug.Log( $"<color=green>Selecionado:</color> {targetToSelect.name}" );
                        }
                        else
                        {
                            Debug.LogWarning( $"Quest '{label}' encontrada em {pos}, mas o campo 'gameobj' está nulo!" );
                        }
                    }
                }
            }
        }

        GUI.backgroundColor = Color.white;
        Handles.EndGUI();

        // Faz a SceneView atualizar ao mover o mouse (opcional, mas ajuda a UI a não 'engasgar')
        if( Event.current.type == EventType.MouseMove )
            sceneView.Repaint();
    }

    // ===================================================================================
    // LÓGICA DE SELEÇÃO AUTOMÁTICA
    // ===================================================================================
    private void ProcessSelectedTile( TileBase selected )
    {
        if( selected is Tile tileData && tileData.sprite != null )
        {
            // Reutiliza sua lógica de ID Global
            int globalID = GetGlobalID(tileData.sprite);
            ETileType tileType = (ETileType)globalID;

            // Busca a Layer correta no seu Map.cs
            ELayerType layer = Map.GetTileLayer(tileType);

            if( layer != ELayerType.NONE && myTilemap.Tilemaps.Count > (int) layer )
            {
                GameObject targetLayer = myTilemap.Tilemaps[(int)layer].gameObject;

                // Força a Tile Palette a desenhar na layer correta
                GridPaintingState.scenePaintTarget = targetLayer;

                Debug.Log( $"<color=green><b>[Auto-Layer]</b></color> Brush: {selected.name} -> Layer: {layer}  Tile: {tileType}" );

            }
        }
    }

    private int GetGlobalID( Sprite s )
    {
        if( s == null ) return -1;

        // 1. Pega o localID do nome (ex: Tiles 1_2)
        string name = s.name;
        int underscore = name.LastIndexOf('_');
        if( underscore == -1 || !int.TryParse( name.Substring( underscore + 1 ), out int localID ) )
            return 0;

        // 2. Descobre o quadrante pela textura
        int quadrant = 0; // Q0 padrão
        string texName = s.texture.name;
        if( texName.Contains( "Tile 2" ) ) quadrant = 1; // Q1
        else if( texName.Contains( "Tile 3" ) ) quadrant = 2; // Q2
        else if( texName.Contains( "Tile 4" ) ) quadrant = 3; // Q3

        // 3. Calcula posição dentro do quadrante
        int xInQuad = localID % 64;
        int yInQuad = localID / 64;

        // 4. Calcula offset do quadrante
        int offsetX = (quadrant % 2) * 64;
        int offsetY = (quadrant / 2) * 64;

        // 5. GlobalID na textura inteira (128x128 tiles)
        int globalX = xInQuad + offsetX;
        int globalY = yInQuad + offsetY;

        int globalID = globalY * 128 + globalX;
        return globalID;
    }


    private void ExtractBrushData( out TileBase tile, out Vector3Int pos )
    {
        tile = null; pos = Vector3Int.zero;
        if( paintingStateType == null ) return;

        // Pega posição do mouse no grid da paleta
        var selectionProp = paintingStateType.GetProperty("gridSelection", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var selectionObj = selectionProp?.GetValue(null);
        if( selectionObj != null )
        {
            var posProp = selectionObj.GetType().GetProperty("position", BindingFlags.Instance | BindingFlags.Public);
            if( posProp != null ) pos = ( (BoundsInt) posProp.GetValue( selectionObj ) ).position;
        }

        // Pega o tile que está "colado" no pincel
        var brushProp = paintingStateType.GetProperty("gridBrush", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var brush = brushProp?.GetValue(null);
        if( brush != null )
        {
            var cellsField = typeof(GridBrush).GetField("m_Cells", BindingFlags.Instance | BindingFlags.NonPublic);
            var cellsArray = cellsField?.GetValue(brush) as Array;
            if( cellsArray != null && cellsArray.Length > 0 )
            {
                var firstCell = cellsArray.GetValue(0);
                var tField = firstCell.GetType().GetField("m_Tile", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                tile = tField?.GetValue( firstCell ) as TileBase;
            }
        }
    }
}
