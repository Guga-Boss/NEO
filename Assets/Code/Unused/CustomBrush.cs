using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomGridBrush( false, true, false, "Custom Brush" )]
[CreateAssetMenu( fileName = "New Custom Brush", menuName = "Tilemap/Brushes/Custom Brush" )]
public class CustomBrush: GridBrush
{
    [Header("Dados do Último Tile Selecionado")]
    public ETileType TileID = ETileType.NONE;
    public Vector2Int BrushPos;

    // Método que o Bake vai chamar
    public void SetTileData( ETileType id, int x, int y )
    {
        this.TileID = id;
        this.BrushPos = new Vector2Int( x, y );
    }
}
