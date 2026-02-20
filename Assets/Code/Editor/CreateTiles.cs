using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using UnityEditor.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

public class CreateTiles
{
    // ===================================================================================
    // 1. REBUILD (Cria a paleta e os arquivos .asset a partir da Textura Selecionada)
    // ===================================================================================
    [MenuItem( "Tools/Rebuild Tiles (From Selected Texture)" )]
    static void RebuildTiles()
    {
        Texture2D texture = Selection.activeObject as Texture2D;
        if( texture == null )
        {
            Debug.LogError( "Selecione a Textura (ex: Tiles 1) no Project antes de rodar o Rebuild!" );
            return;
        }

        string texturePath = AssetDatabase.GetAssetPath(texture);
        string texName = texture.name;
        string baseFolder = $"Assets/Images/Map Play Tilemap/{texName}";

        // Limpeza instantânea da pasta antiga (se existir)
        if( Directory.Exists( baseFolder ) ) Directory.Delete( baseFolder, true );
        Directory.CreateDirectory( baseFolder );
        AssetDatabase.Refresh();

        // Preparação da Textura
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        if( !importer.isReadable || importer.textureCompression != TextureImporterCompression.Uncompressed )
        {
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        Color32[] allPixels = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath).GetPixels32();
        int texWidth = texture.width;

        Sprite[] allSprites = AssetDatabase.LoadAllAssetsAtPath(texturePath)
            .OfType<Sprite>()
            .OrderBy(s => GetLocalID(s.name))
            .ToArray();

        // Processamento Paralelo usando o seu IsTransparentFast já existente
        Rect[] rects = allSprites.Select(s => s.textureRect).ToArray();
        bool[] isTransparent = new bool[allSprites.Length];
        Parallel.For( 0, allSprites.Length, i => {
            isTransparent[ i ] = IsTransparentFast( rects[ i ], allPixels, texWidth );
        } );

        // Criação da Paleta Oficial
        GameObject paletteGO = GridPaletteUtility.CreateNewPalette(
            baseFolder, texName, GridLayout.CellLayout.Rectangle,
            GridPalette.CellSizing.Automatic, new Vector3(1, 1, 0), GridLayout.CellSwizzle.XYZ
        );

        Tilemap tilemap = paletteGO.GetComponentInChildren<Tilemap>();
        var renderer = tilemap.GetComponent<TilemapRenderer>();
        if( renderer != null ) renderer.sortingLayerName = "Default";

        // Escrita em Lote dos Tiles
        AssetDatabase.StartAssetEditing();
        int total = 0;

        try
        {
            for( int i = 0; i < allSprites.Length; i++ )
            {
                if( isTransparent[ i ] ) continue;

                int localID = GetLocalID(allSprites[i].name);
                int lx = localID % 64;
                int ly = localID / 64;

                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = allSprites[ i ];

                AssetDatabase.CreateAsset( tile, $"{baseFolder}/{allSprites[ i ].name}.asset" );
                // Posiciona mantendo a ordem visual da grade
                tilemap.SetTile( new Vector3Int( lx, -ly, 0 ), tile );

                total++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        EditorUtility.SetDirty( paletteGO );
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        GridPaintingState.palette = paletteGO;
        Debug.Log( $"<color=#00FF00><b>[REBUILD OK]</b></color> {total} tiles criados na paleta {texName}." );
    }

    // ===================================================================================
    // 2. CLEANER (O código que você enviou, inalterado)
    // ===================================================================================
    [MenuItem( "Tools/Clean Transparent Tiles (Active Palette)" )]
    static void CleanActivePalette()
    {
        // 1. Pega a paleta ativa na Janela "Tile Palette"
        GameObject paletteGO = GridPaintingState.palette;
        if( paletteGO == null )
        {
            Debug.LogError( "Nenhuma paleta ativa na Tile Palette Window!" );
            return;
        }

        Tilemap tilemap = paletteGO.GetComponentInChildren<Tilemap>();
        if( tilemap == null ) return;

        // Regra Master: Garantir Sorting Layer correta
        var renderer = tilemap.GetComponent<TilemapRenderer>();
        if( renderer != null ) renderer.sortingLayerName = "Default";

        // 2. Coleta de dados em massa (Sem loops lentos da Unity API)
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        // Localiza a textura através do primeiro tile válido
        Texture2D texture = null;
        foreach( var t in allTiles )
        {
            if( t is Tile tile && tile.sprite != null )
            {
                texture = tile.sprite.texture;
                break;
            }
        }

        if( texture == null ) { Debug.LogWarning( "Nenhum Tile com Sprite encontrado na paleta." ); return; }

        // 3. Preparação da Textura
        string texPath = AssetDatabase.GetAssetPath(texture);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(texPath);
        if( !importer.isReadable || importer.textureCompression != TextureImporterCompression.Uncompressed )
        {
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        Color32[] pixels = texture.GetPixels32();
        int texWidth = texture.width;

        // 4. Processamento Paralelo (Cálculo puro na CPU)
        // Criamos um array de metadados para as threads não tocarem na API da Unity
        var tileData = new (Rect rect, string path, Vector3Int pos)[allTiles.Length];
        for( int i = 0; i < allTiles.Length; i++ )
        {
            if( allTiles[ i ] is Tile t && t.sprite != null )
            {
                // Calculamos a posição 3D baseada no índice do array GetTilesBlock
                int x = i % bounds.size.x;
                int y = i / bounds.size.x;
                tileData[ i ] = (t.sprite.textureRect, AssetDatabase.GetAssetPath( t ), new Vector3Int( bounds.xMin + x, bounds.yMin + y, 0 ));
            }
        }

        bool[] toDelete = new bool[allTiles.Length];
        Parallel.For( 0, allTiles.Length, i =>
        {
            if( tileData[ i ].path != null )
            {
                toDelete[ i ] = IsTransparentFast( tileData[ i ].rect, pixels, texWidth );
            }
        } );

        // 5. Aplicação das mudanças (Escrita em Disco)
        int removedCount = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            for( int i = 0; i < allTiles.Length; i++ )
            {
                if( toDelete[ i ] )
                {
                    // Remove do Tilemap (Evita o "Rosa")
                    tilemap.SetTile( tileData[ i ].pos, null );

                    // Deleta o asset físico
                    AssetDatabase.DeleteAsset( tileData[ i ].path );
                    removedCount++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        // 6. Refresh Final
        EditorUtility.SetDirty( paletteGO );
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log( $"<color=#00FF00><b>[CLEANER]</b></color> Faxina Turbo concluída! {removedCount} tiles deletados." );
    }

    // ===================================================================================
    // FUNÇÕES AUXILIARES
    // ===================================================================================

    // Checagem de transparência otimizada com early exit (Inalterada)
    static bool IsTransparentFast( Rect r, Color32[ ] pixels, int texWidth )
    {
        int yMin = (int)r.y;
        int yMax = yMin + (int)r.height;
        int xMin = (int)r.x;
        int xMax = xMin + (int)r.width;

        for( int y = yMin; y < yMax; y++ )
        {
            int row = y * texWidth;
            for( int x = xMin; x < xMax; x++ )
            {
                if( pixels[ row + x ].a > 10 ) return false; // Encontrou pixel opaco, sai imediatamente
            }
        }
        return true;
    }

    // Identifica o ID original do Tile para manter a ordem da grade (Usado pelo Rebuild)
    static int GetLocalID( string name )
    {
        int idx = name.LastIndexOf('_');
        return ( idx != -1 && int.TryParse( name.Substring( idx + 1 ), out int id ) ) ? id : 0;
    }
}