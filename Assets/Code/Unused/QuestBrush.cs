using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

// ESSE ATRIBUTO É O QUE FAZ APARECER NA LISTA DA TILE PALETTE
[CustomGridBrush( false, true, false, "Quest Brush" )]
[CreateAssetMenu( fileName = "New Quest Brush", menuName = "Tilemap/Brushes/Quest Brush" )]
public class QuestBrush: GridBrush
{
    [Header("Variáveis Customizadas")]
    public ETileType TileID = ETileType.NONE;
    public RandomMapData RM;
}
