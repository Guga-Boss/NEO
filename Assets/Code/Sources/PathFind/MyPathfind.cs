using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DeenGames.Utils;
using DeenGames.Utils.AStarPathFinder;

public class MyPathfind : MonoBehaviour
{
    public static MyPathfind I;
    public int Width;
    public int Height;
    public byte[,] Grid;
    public Vector2[] Path;
    public const int BLOCKED_TILE = 0;
    public const int PASSABLE_TILE = 1;

    public void Start()
    {
        I = this;
    }


    public void CreateMap( int w, int h )
    {
        Width = Util.RoundToNearestPowerOfTwo( w );
        Height = Util.RoundToNearestPowerOfTwo( h );
        Grid = new byte[ Width, Height ];
    }

    public void SeedBeehiveMap( Area area )
    {
        for( int y =  ( int ) area.P2.y-1; y <= area.P1.y+1; y++ )
        for( int x =  ( int ) area.P1.x-1; x <= area.P2.x+1; x++ )
        if ( Map.PtOnMap( Quest.I.CurLevel.Tilemap, new Vector2( x, y ) ) )
            {             
                bool ok = false;
                Vector2 pos = new Vector2( x, y );
                Grid[ x, y ] = MyPathfind.BLOCKED_TILE;  
                ok = Map.I.Hero.CheckTerrainMove( new Vector2( -1, -1 ), pos, true, true, true, false, false );
            
                Unit un = Map.I.GetUnit( pos, ELayerType.MONSTER );
                if( un )
                {
                    if( un.TileID == ETileType.BARRICADE ) ok = true;
                    if( un && un.ValidMonster ) ok = false;
                }

                if( Map.I.AreaID[ x, y ] != area.GlobalID ) ok = false;
                if( ok ) Grid[ x, y ] = MyPathfind.PASSABLE_TILE;
            }    
    }

    public void SeedForestMap( Area area )
    {
        for( int y =  ( int ) area.P2.y-1; y <= area.P1.y+1; y++ )
        for( int x =  ( int ) area.P1.x-1; x <= area.P2.x+1; x++ )
        if ( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )
            {
                Grid[ x, y ] = MyPathfind.BLOCKED_TILE;
                ETileType gaia = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( x, y, ( int ) ELayerType.GAIA );
                if( gaia != ETileType.FOREST ) Grid[ x, y ] = MyPathfind.PASSABLE_TILE;

                if( x == ( int ) area.P1.x - 1 || x == area.P2.x + 1 ||                                        // 1 unti border to block passage out of area
                    y == ( int ) area.P2.y - 1 || y == area.P1.y + 1 ) Grid[ x, y ] = MyPathfind.BLOCKED_TILE;

                if( Map.I.AreaID[ x, y ] == -1 ) Grid[ x, y ] = MyPathfind.BLOCKED_TILE;

                if( Map.I.AreaID[ x, y ] != area.GlobalID )
                {
                    Grid[ x, y ] = MyPathfind.BLOCKED_TILE;
                }
            }
    }


    public void SeedClearedMap( Area area )
    {
        for( int y =  ( int ) area.P2.y - 1; y <= area.P1.y + 1; y++ )
            for( int x =  ( int ) area.P1.x - 1; x <= area.P2.x + 1; x++ )
                if( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )
                {
                    Grid[ x, y ] = MyPathfind.PASSABLE_TILE;
                    //ETileType gaia = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( x, y, ( int ) ELayerType.GAIA );
                    //if( gaia != ETileType.FOREST ) Grid[ x, y ] = MyPathfind.PASSABLE_TILE;

            //        if( x == ( int ) area.P1.x - 1 || x == area.P2.x + 1 ||                                        // 1 unti border to block passage out of area
            //            y == ( int ) area.P2.y - 1 || y == area.P1.y + 1 ) Grid[ x, y ] = MyPathfind.BLOCKED_TILE;

                   // ETileType mod = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( x, y, ( int ) ELayerType.MODIFIER );

                    if( Map.I.AreaID[ x, y ] == -1 ) Grid[ x, y ] = MyPathfind.BLOCKED_TILE;

                    if( Map.I.AreaID[ x, y ] != area.GlobalID )
                    {
                        Grid[ x, y ] = MyPathfind.BLOCKED_TILE;
                    }
                }
    }


    public void SeedJumpMap()
    {
        for( int y = 0; y < Map.I.Tilemap.height; y++ )
        for( int x = 0; x < Map.I.Tilemap.width; x++ )
            {
                bool ok = false;
                Vector2 pos = new Vector2( x, y );
                Grid[ x, y ] = MyPathfind.BLOCKED_TILE;
                //ok = Map.I.Hero.CheckTerrainMove( new Vector2( -1, -1 ), pos, true, true, true, false, false );
                if( Map.I.Gaia2[ ( int ) pos.x, ( int ) pos.y ] &&
                    Map.I.Gaia2[ ( int ) pos.x, ( int ) pos.y ].TileID == ETileType.CHECKPOINT ) { ok = true; }
                else
                    if( Map.I.IsTileOnlyGrass( pos ) ) ok = true;

                if( Map.I.Gaia[ ( int ) pos.x, ( int ) pos.y ] )
                if( Map.I.Gaia[ ( int ) pos.x, ( int ) pos.y ].TileID == ETileType.WATER )
                { ok = false; }      

                //if( Map.I.AreaID[ x, y ] != -1 ) ok = false;
                if( ok ) Grid[ x, y ] = MyPathfind.PASSABLE_TILE;
            }              
    }

    
    public string UpdateDebug()
    {
        string st = "";
       // return st;
        //if( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( Map.I.Mtx, Map.I.Mty ) ) ) return st;
        if( Grid == null ) return "";
        if( Grid[ Map.I.Mtx, Map.I.Mty ] == MyPathfind.PASSABLE_TILE )
            st = "\nMypathFind Debug: Passable Tile\n";
        else
            st = "\nMypathFind Debug: Blocked Tile\n";

        return st;
    }

    // Update is called once per frame
    public void GetPath( Vector2 from, Vector2 to )
    {
        PathFinder pf = new PathFinder( Grid );
        pf.Diagonals = true;

        List<PathFinderNode> path = pf.FindPath( new Point( ( int ) from.x, ( int ) from.y ),
                                                 new Point( ( int ) to.x,   ( int ) to.y   ) );

        //Path = null;
        if( path != null )
        {
            Path = new Vector2[ path.Count ];
            for( int i = 0; i < path.Count; i++ )
                Path[ i ] = new Vector2( path[ i ].X, path[ i ].Y );

            //Debug.Log( "Found path " + path.Count );
        }

        //if( path != null )
        //foreach( PathFinderNode node in path )
        //{
        //    Debug.Log( node.PX + "x" + node.PY );
        //}
        //else
        //    Debug.Log( path );
    }
}
