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

    [MenuItem( "Tools/Tilemap Master Watcher" )]
    public static void ShowWindow() => GetWindow<TilePaletteWatcher>( "Tile Master" ).Show();

    private void OnEnable()
    {
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

    private void OnSceneGUI( SceneView sceneView )
    {
        Handles.BeginGUI(); // begin GUI drawing in SceneView

        foreach( var tilemap in Map.I.TM.Tilemaps )
        {
            if( tilemap == null )
                continue; // skip null tilemaps

            foreach( var pos in tilemap.cellBounds.allPositionsWithin )
            {
                TileBase tile = tilemap.GetTile(pos);
                if( tile == null )
                    continue; // skip empty cells

                if( !tile.name.Contains( Map.I.TM.targetTileName ) )
                    continue; // only process target tiles

                Vector2Int coordKey = new Vector2Int(pos.x, pos.y); // grid coordinate key

                if( Map.I.TM.questDataCache == null )
                    continue; // no cache available

                if( !Map.I.TM.questDataCache.TryGetValue( coordKey, out RandomMapData data ) )
                    continue; // no quest assigned to this tile

                if( !data.Available )
                    continue; // only show button if quest is available

                Vector3 worldPos = tilemap.GetCellCenterWorld(pos); // convert cell to world position
                Vector2 guiPos = HandleUtility.WorldToGUIPoint(worldPos); // convert world to GUI space

                string label = data.QuestHelper.QuestName; // quest display name
                GameObject targetToSelect = data.gameObject; // object to select on click

                GUIStyle style = new GUIStyle(GUI.skin.button);
                Vector2 size = style.CalcSize(new GUIContent(label));

                float maxWidth = 190f;
                style.fontSize = 10; 
                float width = Mathf.Min(size.x + 20, maxWidth);
                GUI.backgroundColor = Color.green; // available quest color
                Rect rect = new Rect(guiPos.x - width * 0.5f, guiPos.y - 12, width, 25);
                
                if( GUI.Button( rect, label, style ) )
                { 
                    Selection.activeGameObject = targetToSelect; // select quest object
                    EditorGUIUtility.PingObject( targetToSelect ); // highlight in hierarchy/project 
                    MapSaver ms = MapSaver.Get( );
                    ms.FolderName = data.QuestHelper.SubFolder + "/" + data.QuestHelper.Signature;
                    ms.CurrentAdventure = data.QuestID;
                    ms.CurrentAdventureName = data.QuestHelper.QuestName;
                    Helper.I.StartingAdventure = data.QuestID;
                    MapSaver.I.SetStartingCube();
                    MapSaver.I.Load();
                    Debug.Log( $"<color=blue>Selected:</color> {data.QuestHelper.QuestName}" ); // debug feedback 
                }
            }
        }

        GUI.backgroundColor = Color.white; // reset GUI color
        Handles.EndGUI(); // finish GUI drawing

        if( Event.current.type == EventType.MouseMove )
            sceneView.Repaint(); // smooth UI refresh
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

            if( layer != ELayerType.NONE && Map.I.TM.Tilemaps.Count > (int) layer )
            {
                GameObject targetLayer = Map.I.TM.Tilemaps[(int)layer].gameObject;

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
