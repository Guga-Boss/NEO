using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using Sirenix.OdinInspector;

public enum EAreaColor
{
    NONE, ANY, RED, GREEN, BLUE, YELLOW
}

public class AreaDefinition : MonoBehaviour 
{
    public enum EAreaDefinitionVarType
    {
        NONE, PERCENT, MINIMUM, ABSOLUTE
    }

    [TabGroup( "Main" )]
    public EAreaColor AreaColor = EAreaColor.NONE;
    [TabGroup( "Link" )]
    public TileDefinition[ ] TileList = null;
    [TabGroup( "Link" )]
    public static AreaDefinition Current;
    [TabGroup( "Link" )]
    public static RandomMap RM;

    public static void Initialize( Sector s )
    {
        for( int i = 0; i < s.AreaList.Count; i++ )
            PopulateArea( s.AreaList[ i ] );

        Sector.DeleteUndesiredAreas( s );
    }

    public static void PopulateArea( Area area )
    {
        if( area.TileCount <= 0 ) return;
        Map.I.LevelStats.AreasDiscovered++;

        float fact = RM.RMD.MonsterLevelIncreaseFactor;                                                                                                 // Calculates Monster Level
        if( RM.RMD.MonsterLevelFactorInflation != 0 )
            fact = Util.CompoundInterest( fact, Map.I.LevelStats.NormalSectorsDiscovered - 1, RM.RMD.MonsterLevelFactorInflation );

        area.DefaulMonsterLevel = 0;
        if( fact > 0 )
            area.DefaulMonsterLevel = ( int ) RM.RMD.BaseMonsterLevel - 1 + ( int ) ( ( Map.I.LevelStats.NormalSectorsDiscovered ) * fact );

        int val = 1 + ( int ) ( Map.I.LevelStats.NormalSectorsDiscovered / 5 );

        if( SectorHint.GetHintBonus( "FORTEMONSTAR", 0 ) > 0 ) area.DefaulMonsterLevel += val;
        if( SectorHint.GetHintBonus( "WEEKAMONSTAR", 0 ) > 0 ) area.DefaulMonsterLevel -= val;

        if( area.DefaulMonsterLevel < 1 ) area.DefaulMonsterLevel = 1;

        List<int> common = new List<int>();
        int areadef = -1;
        for( int i = 0; i < RM.SD.ADList.Length; i++ )                                                                                                  // Is there a forced area definition for this area color?
        {
            if( RM.SD.ADList[ i ].AreaColor == EAreaColor.ANY ) common.Add( i );
            if( area.AreaDragID == ETileType.AREA_DRAG  && RM.SD.ADList[ i ].AreaColor == EAreaColor.RED ) areadef = i;
            if( area.AreaDragID == ETileType.AREA_DRAG2 && RM.SD.ADList[ i ].AreaColor == EAreaColor.GREEN ) areadef = i;
            if( area.AreaDragID == ETileType.AREA_DRAG3 && RM.SD.ADList[ i ].AreaColor == EAreaColor.BLUE ) areadef = i;
            if( area.AreaDragID == ETileType.AREA_DRAG4 && RM.SD.ADList[ i ].AreaColor == EAreaColor.YELLOW ) areadef = i;
        }

        if( areadef == -1 )
        {
            if( common.Count <= 0 ) Debug.LogError( "Area color not defined!" );
            areadef = common[ Random.Range( 0, common.Count ) ];                                                                       // if not, sort a common area
        }

        if( areadef == -1 ) Debug.LogError( "Bad Area Definition" );

        bool del = false;
        for( RM.AreaPopulateTrialCount = 0; ; RM.AreaPopulateTrialCount++ )                                                                   // Populate areas Loop
        {
            Populate( area, RM.SD.ADList[ areadef ], del );
            break;

            //if( CheckMap( area ) )                                                                                                    // repeated trials for stuck monsters
            //{
            //    break;
            //}
            //else del = true;

            //if( AreaPopulateTrialCount > 999 )
            //{                
            //    break;
            //}
        }

        if( RM.AreaPopulateTrialCount > 0 )
            Debug.Log( "Stuck Monsters - Area rebuilt " + RM.AreaPopulateTrialCount + " Times." );
    }
    
    public static void Populate( Area area, AreaDefinition pad, bool deleteOld )
    {
        tk2dTileMap tm = Quest.I.Dungeon.Tilemap;
        if( deleteOld )
        {
            for( int i = 0; i < RM.CreatedTilesPos.Count; i++ )
                Quest.I.Dungeon.Tilemap.SetTile( ( int ) RM.CreatedTilesPos[ i ].x,
                                                 ( int ) RM.CreatedTilesPos[ i ].y, ( int ) RM.CreatedTilesLayer[ i ], -1 );
        }

        AreaDefinition.Current = pad;
        RM.CreatedTilesPos = new List<Vector2>();
        RM.CreatedTilesLayer = new List<ELayerType>();

        for( int i = 0; i < pad.TileList.Length; i++ )
        {
            TileDefinition td = pad.TileList[ i ];
            int amount = td.Amount;
            if( td.PercentOfArea > 0 )                                                            // Is not absolute, so calculate percentage of area
            {
                amount = Mathf.RoundToInt( Util.Percent( td.PercentOfArea, area.TL.Count ) );
            }

            CreateRandomUnits( td, amount, td.Chance, area );
        }
    }

    public static int CreateRandomUnits( TileDefinition td, float amount, float chance, Area area )
    {
        int cont = 0;
        ELayerType layer = Map.GetTileLayer( td.Tile );
        List<float> ph = new List<float>();
        List<Vector2> pl = new List<Vector2>();                                      // Make a list of all possible targets
        for( int y = ( int ) area.P2.y; y <= area.P1.y; y++ )
        for( int x = ( int ) area.P1.x; x <= area.P2.x; x++ )
        if ( Map.I.AreaID[ x, y ] == area.GlobalID )
        {
            if( RandomMap.CanCreateUnit( new Vector2( x, y ), layer, td.Tile ) )
            {
                pl.Add( new Vector2( x, y ) );
                float height = 100;
                #region border
                //if( borderHeight != 100 )  // This is if you want object to be created on make likeability to a neighbor object
                //{
                //    int cnt = 0;
                //    for( int d = 0; d < 8; d++ )                                                                          // If on border, sets border height
                //    {
                //        Vector2 aux = new Vector2( x, y ) + Manager.I.U.DirCord[ ( int ) d ];
                //        if( Map.I.PtOnAreaMap( aux ) ) cnt++;
                //    }
                //    cnt -= 4;

                //    height = 100 + ( borderHeight * cnt );
                //    if( height < 0 ) height = 1;
                //}
                #endregion
                ph.Add( height );
            }
        }

        if( amount > ph.Count )
        {
            Debug.LogError( "Error: No space to create: " +                          // Error: no space
                td.Tile.ToString() + " Created: " + cont );
            return ph.Count;
        }

        List<Vector2> count = new List<Vector2>();
        for( int i = 0; i < amount; i++ )
        {
            int id = Util.Sort( ph );

            if( id == -1 )
            {
                Debug.Log( "Bad ID: " + id + " " + td.Tile.ToString() );
                return ph.Count;
            }

            ETileType ttype = td.Tile;
            if( td.Tile == ETileType.ARROW || td.Tile == ETileType.BOULDER ||                 // Random rotation
                td.Tile == ETileType.ORIENTATION )
            {
                ttype = Map.I.GetRotationID( td.Tile, Random.Range( 0, 8 ) );
            }

            if( td.Tile == ETileType.BARRICADE )                                           // Barricade size
            {
                int rnd = RM.SortBarricadeSize( true, -1 );
                ttype = ( ETileType ) td.Tile + ( int ) ( rnd );
            }

            bool ok = false;                                                             // Sort Chance
            if( Util.Chance( chance ) ) ok = true;

            if( ok )
            {
                Vector2 pos = pl[ id ];
                Quest.I.Dungeon.Tilemap.SetTile( ( int ) pl[ id ].x, ( int )            // Sets the tile on tilemap
                pl[ id ].y, ( int ) layer, ( int ) ttype );

                if( td.Mod != EModType.NONE )
                {
                    ETileType md = Mod.GetTileIDforMod( td.Mod );  
                    Quest.I.Dungeon.Tilemap.SetTile( ( int ) pl[ id ].x, ( int )        // Sets the mod number
                    pl[ id ].y, ( int ) ELayerType.MODIFIER, ( int ) md );
                }

                RM.CreatedTilesPos.Add( pl[ id ] );                                     // Add to list of created tiles 
                RM.CreatedTilesLayer.Add( layer );

                count.Add( pl[ id ] );
                pl.Remove( pl[ id ] );
                ph.Remove( ph[ id ] );
            }
        }
        return ph.Count;
    }

    #region Script
    public float GetPER( string input )
    {
        Match match = Regex.Match( input, @"PER ([0-9\-]+)", RegexOptions.IgnoreCase );
        float res = 0;
        if( match.Success ) float.TryParse( match.Groups[ 1 ].Value, out res );
        else return 0;
        match = Regex.Match( input, @"TO ([0-9\-]+)", RegexOptions.IgnoreCase );
        float rnd = 0;
        if( match.Success ) float.TryParse( match.Groups[ 1 ].Value, out rnd );
        if( rnd != 0 ) res = Mathf.Round( Random.Range( res, rnd ) );
        return res;
    }
    public float GetABS( string input )
    {
        Match match = Regex.Match( input, @"ABS ([0-9\-]+)", RegexOptions.IgnoreCase );
        float res = 0;
        if( match.Success ) float.TryParse( match.Groups[ 1 ].Value, out res );
        else return 0;
        match = Regex.Match( input, @"TO ([0-9\-]+)", RegexOptions.IgnoreCase );
        float rnd = 0;
        if( match.Success ) float.TryParse( match.Groups[ 1 ].Value, out rnd );
        if( rnd != 0 ) res = Mathf.Round( Random.Range( res, rnd ) );

        return res;
    }
    public float GetMIN( string input )
    {
        Match match = Regex.Match( input, @"MIN ([0-9\-]+)", RegexOptions.IgnoreCase );
        float res = 0;
        if( match.Success ) float.TryParse( match.Groups[ 1 ].Value, out res );
        return res;
    }
    public float GetMAX( string input )
    {
        Match match = Regex.Match( input, @"MAX ([0-9\-]+)", RegexOptions.IgnoreCase );
        float res = 9999;
        if( match.Success ) float.TryParse( match.Groups[ 1 ].Value, out res );
        return res;
    }
    public float GetBH( string input )
    {
        Match match = Regex.Match( input, @"BH ([0-9\-]+)", RegexOptions.IgnoreCase );
        float res = 100;
        if( match.Success ) float.TryParse( match.Groups[ 1 ].Value, out res );
        return res;
    }

    public float GetCHC( string input )
    {
        Match match = Regex.Match( input, @"CHC ([0-9\-]+)", RegexOptions.IgnoreCase );
        float res = 100;
        if( match.Success ) float.TryParse( match.Groups[ 1 ].Value, out res );
        return res;
    }

    public float GetMOD( string input )
    {
        Match match = Regex.Match( input, @"MOD ([0-9\-]+)", RegexOptions.IgnoreCase );
        float res = 0;
        if( match.Success ) float.TryParse( match.Groups[ 1 ].Value, out res );
        return res;
    }

    #endregion
}
