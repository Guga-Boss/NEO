using UnityEngine;
using System.Collections;

public class TileDefinition : MonoBehaviour
{
    public ETileType Tile = ETileType.NONE;
    public int Amount = 0;
    public float Chance = 100;
    public float PercentOfArea = -1;
    public EModType Mod = EModType.NONE;
}
