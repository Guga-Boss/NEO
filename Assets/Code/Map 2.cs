using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public partial class Map : MonoBehaviour
{
    public void UpdateTransLayerTilemap()
    {
        if( TransTilemapUpdateList.Count <= 0 ) return; // nothing to update;

        tk2dTileMap tm = TransTileMap;
        if( tm == null ) { Debug.Log( "tm == null" ); return; } // sanity check;

        tm.transform.position = new Vector2( -0.25f, -0.25f ); // align tiles;

        // ---------- LOOP 1: Atualiza cada tile ----------
        for( int i = 0; i < TransTilemapUpdateList.Count; i++ )
        {
            VI pos = TransTilemapUpdateList[ i ];
            int gx = pos.x;
            int gy = pos.y;
            var g = Gaia[ gx, gy ];
            if( g == null ) continue; // no Gaia;

            ETileType id = g.TileID;

            switch( id )
            {
                case ETileType.OPENROOMDOOR: 
                case ETileType.NONE: UpdateTransTile( tm, pos, id, -1, 2 ); break;
                case ETileType.FOREST: UpdateTransTile( tm, pos, id, 0, 2 ); break;
                case ETileType.WATER: UpdateTransTile( tm, pos, id, 120, 0 ); break;
                case ETileType.MUD: UpdateTransTile( tm, pos, id, 240, 0 ); break;
                case ETileType.CLOSEDDOOR: UpdateTransTile( tm, pos, id, 360, 2 ); break;
                case ETileType.OPENDOOR: UpdateTransTile( tm, pos, id, 480, 0 ); break;
                case ETileType.ROOMDOOR: UpdateTransTile( tm, pos, id, 600, 2 ); break;
                case ETileType.PIT: UpdateTransTile( tm, pos, id, 720, 0 ); break;
                case ETileType.BLACKGATE: UpdateTransTile( tm, pos, id, 840, 2 ); break;
                case ETileType.SNOW: UpdateTransTile( tm, pos, id, 960, 0 ); break;
                case ETileType.ROAD: UpdateTransTile( tm, pos, id, 1080, 0 ); break;
                case ETileType.SAND: UpdateTransTile( tm, pos, id, 1200, 0 ); break;
                case ETileType.STONEPATH: UpdateTransTile( tm, pos, id, 1320, 0 ); break;
                case ETileType.LAVA: UpdateTransTile( tm, pos, id, 1440, 0 ); break;
            }
        }

        // ---------- LOOP 2: Atualiza sombras básicas ----------
        for( int i = 0; i < TransTilemapUpdateList.Count; i++ )
        {
            UpdateTileShadow( TransTilemapUpdateList[ i ] ); // basic shadow update;
        }

        // ---------- LOOP 3: Atualiza sombras para portas e gates ----------
        for( int i = 0; i < TransTilemapUpdateList.Count; i++ )
        {
            VI pos = TransTilemapUpdateList[ i ];
            int gx = pos.x;
            int gy = pos.y;
            var g = Gaia[ gx, gy ];
            if( g == null ) continue; // no Gaia;

            ETileType id = g.TileID;

            if( id == ETileType.OPENDOOR || id == ETileType.CLOSEDDOOR ||
               id == ETileType.BLACKGATE || id == ETileType.ROOMDOOR ||
               id == ETileType.OPENBLACKGATE || id == ETileType.OPENROOMDOOR )
            {
                UpdateTileShadow( new VI( gx - 1, gy + 1 ) ); // top-left neighbor;
                UpdateTileShadow( new VI( gx, gy + 1 ) );     // top neighbor;
                UpdateTileShadow( new VI( gx - 1, gy ) );     // left neighbor;
                UpdateTileShadow( new VI( gx + 1, gy ) );     // right neighbor;
            }
        }

        tm.Build(); // rebuild tilemap after all updates;
        TransTilemapUpdateList.Clear(); // clear list after processing;
    }
    public void UpdateTransTile( tk2dTileMap tm, VI tg, ETileType tile, int first, int tlayer )
    {
        // guarda ints locais uma vez
        int gx = tg.x;
        int gy = tg.y;

        // valida
        if( !Map.PtOnMap( Map.I.Tilemap, tg ) ) return;

        // np := neighbor present
        bool[] np = { false, false, false, false, false, false, false, false };

        // camada (não usada depois, mas mantive)
        ELayerType layer = GetTileLayer( tile );

        // se pediram NONE, limpar
        if( tile == ETileType.NONE )
        {
            SetTransTile( gx, gy, tlayer, -1 );
        }

        // precisa existir Gaia
        var ga = Gaia[ gx, gy ];
        if( ga != null )
        {
            switch( ga.TileID )
            {
                case ETileType.OPENDOOR: SetTransTile( gx, gy, 2, -1 ); break;
                case ETileType.CLOSEDDOOR: SetTransTile( gx, gy, 0, -1 ); break;
                case ETileType.OPENROOMDOOR: SetTransTile( gx, gy, 2, -1 ); return;
                case ETileType.OPENBLACKGATE: SetTransTile( gx, gy, 2, -1 ); return;
            }
        }
        else return;

        ETileType tl = ga.TileID;

        if( tile != tl ) return;

        // verifica vizinhos (0..7)
        for( int d = 0; d < 8; d++ )
        {
            VI t = tg + Manager.I.U.DirCordI[ d ]; 
            if( PtOnMap( Tilemap, t ) )
            {
                var g2 = Gaia[ t.x, t.y ];
                if( g2 != null )
                {
                    ETileType tileId2 = g2.TileID;
                    if( tileId2 == tl ) np[ d ] = true;
                    if( tl == ETileType.PIT && ( tileId2 == ETileType.TRAP || tileId2 == ETileType.FREETRAP ) ) np[ d ] = true;
                    if( tl == ETileType.TRAP && tileId2 == ETileType.FREETRAP ) np[ d ] = true;
                }
            }
            else np[ d ] = true;
        }

        // offsets
        int o0 = 32, o1 = 32, o2 = 32, o3 = 32;

        if( !np[ ( int ) EDirection.W ] && !np[ ( int ) EDirection.N ] ) o0 = 0;
        else if( np[ ( int ) EDirection.W ] && np[ ( int ) EDirection.NW ] && np[ ( int ) EDirection.N ] ) o0 = 32;
        else if( np[ ( int ) EDirection.W ] && !np[ ( int ) EDirection.NW ] && np[ ( int ) EDirection.N ] ) o0 = 39;
        else if( !np[ ( int ) EDirection.N ] ) o0 = 2;
        else if( !np[ ( int ) EDirection.W ] ) o0 = 24;

        if( !np[ ( int ) EDirection.N ] && !np[ ( int ) EDirection.E ] ) o1 = 5;
        else if( np[ ( int ) EDirection.N ] && np[ ( int ) EDirection.NE ] && np[ ( int ) EDirection.E ] ) o1 = 32;
        else if( np[ ( int ) EDirection.N ] && !np[ ( int ) EDirection.NE ] && np[ ( int ) EDirection.E ] ) o1 = 41;
        else if( !np[ ( int ) EDirection.N ] ) o1 = 2;
        else if( !np[ ( int ) EDirection.E ] ) o1 = 35;

        if( !np[ ( int ) EDirection.W ] && !np[ ( int ) EDirection.S ] ) o2 = 75;
        else if( np[ ( int ) EDirection.W ] && np[ ( int ) EDirection.SW ] && np[ ( int ) EDirection.S ] ) o2 = 32;
        else if( np[ ( int ) EDirection.W ] && !np[ ( int ) EDirection.SW ] && np[ ( int ) EDirection.S ] ) o2 = 69;
        else if( !np[ ( int ) EDirection.S ] ) o2 = 77;
        else if( !np[ ( int ) EDirection.W ] ) o2 = 24;

        if( !np[ ( int ) EDirection.E ] && !np[ ( int ) EDirection.S ] ) o3 = 80;
        else if( np[ ( int ) EDirection.S ] && np[ ( int ) EDirection.SE ] && np[ ( int ) EDirection.E ] ) o3 = 32;
        else if( np[ ( int ) EDirection.E ] && !np[ ( int ) EDirection.SE ] && np[ ( int ) EDirection.S ] ) o3 = 71;
        else if( !np[ ( int ) EDirection.S ] ) o3 = 77;
        else if( !np[ ( int ) EDirection.E ] ) o3 = 35;

        // aplica nos 4 sub-tiles
        tm.SetTile( gx * 2 + 0, gy * 2 + 1, tlayer, first + o0 );
        tm.SetTile( gx * 2 + 1, gy * 2 + 1, tlayer, first + o1 );
        tm.SetTile( gx * 2 + 0, gy * 2 + 0, tlayer, first + o2 );
        tm.SetTile( gx * 2 + 1, gy * 2 + 0, tlayer, first + o3 );
    }

    public void UpdateTileShadow( VI pos )
    {
        int gx = pos.x;
        int gy = pos.y;

        if( gx >= Tilemap.width - 1 || gy <= 0 ) return; // bounds check;
        if( !PtOnMap( Tilemap, pos ) ) return; // outside map;

        tk2dTileMap tm = TransTileMap;

        int tgX = gx + 1;
        int tgY = gy - 1;
        if( !PtOnMap( Tilemap, new VI( tgX, tgY ) ) ) return; // target outside map;

        // verifica vizinhos
        bool hasNW = Gaia[ gx, gy ] != null && CastShadow( pos );
        bool hasN = Gaia[ tgX, gy ] != null && CastShadow( new VI( tgX, gy ) );
        bool hasW = Gaia[ gx, tgY ] != null && CastShadow( new VI( gx, tgY ) );

        // limpa tiles
        int baseX = tgX * 2;
        int baseY = tgY * 2;
        tm.SetTile( baseX + 0, baseY + 1, 1, -1 );
        tm.SetTile( baseX + 1, baseY + 1, 1, -1 );
        tm.SetTile( baseX + 0, baseY + 0, 1, -1 );

        // aplica sombras
        if( hasNW && hasN && hasW )
        {
            tm.SetTile( baseX + 0, baseY + 1, 1, 87 );
            tm.SetTile( baseX + 1, baseY + 1, 1, 95 );
            tm.SetTile( baseX + 0, baseY + 0, 1, 81 );
        }
        else if( hasNW && !hasN && hasW )
        {
            tm.SetTile( baseX + 0, baseY + 1, 1, 81 );
            tm.SetTile( baseX + 0, baseY + 0, 1, 81 );
        }
        else if( hasNW && hasN && !hasW )
        {
            tm.SetTile( baseX + 0, baseY + 1, 1, 93 );
            tm.SetTile( baseX + 1, baseY + 1, 1, 93 );
        }
        else if( hasNW && !hasN && !hasW ) tm.SetTile( baseX + 0, baseY + 1, 1, 96 );
        else if( !hasNW && !hasN && hasW )
        {
            tm.SetTile( baseX + 0, baseY + 1, 1, 6 );
            tm.SetTile( baseX + 0, baseY + 0, 1, 21 );
        }
        else if( !hasNW && hasN && hasW )
        {
            tm.SetTile( baseX + 0, baseY + 1, 1, 90 );
            tm.SetTile( baseX + 1, baseY + 1, 1, 91 );
            tm.SetTile( baseX + 0, baseY + 0, 1, 21 );
        }
        else if( !hasNW && hasN && !hasW )
        {
            tm.SetTile( baseX + 0, baseY + 1, 1, 90 );
            tm.SetTile( baseX + 1, baseY + 1, 1, 91 );
        }
    }

    public bool CastShadow( Vector2 tg )
    {
        if( !PtOnMap( Tilemap, tg ) ) return false; // out of map;

        var g = Gaia[ ( int ) tg.x, ( int ) tg.y ];
        if( g == null ) return false; // no Gaia;

        switch( g.TileID )
        {
            case ETileType.FOREST:
            case ETileType.CLOSEDDOOR:
            case ETileType.BLACKGATE:
            case ETileType.ROOMDOOR: return true;
            default: return false;
        }
    }

    public void SetTileColor( tk2dTileMap tm, Vector2 pos, Color col )
    {
        for( int x = 0; x <= 1; x++ )
            for( int y = 0; y <= 1; y++ )
            {
                Vector2 pt = new Vector2( pos.x + x, pos.y + y );
                {
                    tm.ColorChannel.SetColor( ( int ) ( ( pt.x * 2 ) + 0 ),
                                              ( int ) ( ( pt.y * 2 ) + 0 ), col );

                    tm.ColorChannel.SetColor( ( int ) ( ( pt.x * 2 ) + 1 ),
                                              ( int ) ( ( pt.y * 2 ) + 0 ), col );

                    tm.ColorChannel.SetColor( ( int ) ( ( pt.x * 2 ) + 0 ),
                                              ( int ) ( ( pt.y * 2 ) + 1 ), col );

                    tm.ColorChannel.SetColor( ( int ) ( ( pt.x * 2 ) + 1 ),
                                              ( int ) ( ( pt.y * 2 ) + 1 ), col );
                }
            }
    }

    public static int GetTransLayer( ETileType tile )
    {
        switch( tile )
        {
            case ETileType.WATER:
            case ETileType.LAVA:
            case ETileType.PIT:
            case ETileType.SNOW:
            case ETileType.OPENDOOR:
            case ETileType.SAND:
            case ETileType.MUD:
            case ETileType.ROAD: return 0;
        }
        return 2;
    }


    public void SetTransTile( int x, int y, int layer, int tile )
    {
        TransTileMap.SetTile( ( int ) ( x * 2 ) + 0, ( int ) ( y * 2 ) + 1, layer, tile );
        TransTileMap.SetTile( ( int ) ( x * 2 ) + 1, ( int ) ( y * 2 ) + 1, layer, tile );
        TransTileMap.SetTile( ( int ) ( x * 2 ) + 0, ( int ) ( y * 2 ) + 0, layer, tile );
        TransTileMap.SetTile( ( int ) ( x * 2 ) + 1, ( int ) ( y * 2 ) + 0, layer, tile );
    }

    public static void AddNeighborTransToList( VI to )
    {
        for( int dr = 0; dr < 8; dr++ )
        {
            VI tg = to + Manager.I.U.DirCordI[ ( int ) dr ];
            if( Map.I.TransTilemapUpdateList.Contains( tg ) == false )
                Map.I.TransTilemapUpdateList.Add( tg );
        }
    }
    
    public void UpdateAreaMarkTile( tk2dTileMap tm, Vector2 tg, int area, bool cleared )
    {
        bool[ ] np = { false, false, false, false, false, false, false, false }; // Neighbor present?        

        if( AreaID[ ( int ) tg.x, ( int ) tg.y ] == -1 ) return;

        Vector2 t = new Vector2( 0, 0 );
        for( int d = 0; d < 8; d++ )
        {
            t = tg + Manager.I.U.DirCord[ d ];
            if( PtOnMap( Map.I.Tilemap, t ) )
            {
                if( AreaID[ ( int ) t.x, ( int ) t.y ] != -1 )
                    if( AreaID[ ( int ) t.x, ( int ) t.y ] == area ) np[ d ] = true;
            }
            else np[ d ] = true;
        }

        int[ ] offset = { 32, 32, 32, 32 };

        if( !np[ ( int ) EDirection.W ] && !np[ ( int ) EDirection.N ] ) offset[ 0 ] = 0;
        else
            if( np[ ( int ) EDirection.W ] && np[ ( int ) EDirection.NW ] && np[ ( int ) EDirection.N ] ) offset[ 0 ] = 32;
            else
                if( np[ ( int ) EDirection.W ] && !np[ ( int ) EDirection.NW ] && np[ ( int ) EDirection.N ] ) offset[ 0 ] = 39;
                else
                    if( !np[ ( int ) EDirection.N ] ) offset[ 0 ] = 2;
                    else
                        if( !np[ ( int ) EDirection.W ] ) offset[ 0 ] = 24;

        if( !np[ ( int ) EDirection.N ] && !np[ ( int ) EDirection.E ] ) offset[ 1 ] = 5;
        else
            if( np[ ( int ) EDirection.N ] && np[ ( int ) EDirection.NE ] && np[ ( int ) EDirection.E ] ) offset[ 1 ] = 32;
            else
                if( np[ ( int ) EDirection.N ] && !np[ ( int ) EDirection.NE ] && np[ ( int ) EDirection.E ] ) offset[ 1 ] = 41;
                else
                    if( !np[ ( int ) EDirection.N ] ) offset[ 1 ] = 2;
                    else
                        if( !np[ ( int ) EDirection.E ] ) offset[ 1 ] = 35;

        if( !np[ ( int ) EDirection.W ] && !np[ ( int ) EDirection.S ] ) offset[ 2 ] = 75;
        else
            if( np[ ( int ) EDirection.W ] && np[ ( int ) EDirection.SW ] && np[ ( int ) EDirection.S ] ) offset[ 2 ] = 32;
            else
                if( np[ ( int ) EDirection.W ] && !np[ ( int ) EDirection.SW ] && np[ ( int ) EDirection.S ] ) offset[ 2 ] = 69;
                else
                    if( !np[ ( int ) EDirection.S ] ) offset[ 2 ] = 77;
                    else
                        if( !np[ ( int ) EDirection.W ] ) offset[ 2 ] = 24;

        if( !np[ ( int ) EDirection.E ] && !np[ ( int ) EDirection.S ] ) offset[ 3 ] = 80;
        else
            if( np[ ( int ) EDirection.S ] && np[ ( int ) EDirection.SE ] && np[ ( int ) EDirection.E ] ) offset[ 3 ] = 32;
            else
                if( np[ ( int ) EDirection.E ] && !np[ ( int ) EDirection.SE ] && np[ ( int ) EDirection.S ] ) offset[ 3 ] = 71;
                else
                    if( !np[ ( int ) EDirection.S ] ) offset[ 3 ] = 77;
                    else
                        if( !np[ ( int ) EDirection.E ] ) offset[ 3 ] = 35;

        int first = 1080;
        if( cleared ) first = 960;

        tm.SetTile( ( int ) ( tg.x * 2 ) + 0, ( int ) ( tg.y * 2 ) + 1, 3, first + offset[ 0 ] );
        tm.SetTile( ( int ) ( tg.x * 2 ) + 1, ( int ) ( tg.y * 2 ) + 1, 3, first + offset[ 1 ] );
        tm.SetTile( ( int ) ( tg.x * 2 ) + 0, ( int ) ( tg.y * 2 ) + 0, 3, first + offset[ 2 ] );
        tm.SetTile( ( int ) ( tg.x * 2 ) + 1, ( int ) ( tg.y * 2 ) + 0, 3, first + offset[ 3 ] );
    }

    public void ClearAreaTile( tk2dTileMap tm, Vector2 tg )
    {
        tm.SetTile( ( int ) ( tg.x * 2 ) + 0, ( int ) ( tg.y * 2 ) + 1, 3, -1 );
        tm.SetTile( ( int ) ( tg.x * 2 ) + 1, ( int ) ( tg.y * 2 ) + 1, 3, -1 );
        tm.SetTile( ( int ) ( tg.x * 2 ) + 0, ( int ) ( tg.y * 2 ) + 0, 3, -1 );
        tm.SetTile( ( int ) ( tg.x * 2 ) + 1, ( int ) ( tg.y * 2 ) + 0, 3, -1 );
    }

    public static GameObject CreateObjInstance( string str, string iname, EDirection dir, Vector3 pos )
    {
        GameObject prefab = ( GameObject ) Resources.Load( str );
        GameObject instance = ( GameObject ) GameObject.Instantiate( prefab );
        instance.transform.position = pos;
        instance.name = iname;
        if( dir != EDirection.NONE ) instance.transform.Rotate( 0, 0, -( 45.0f * ( float ) ( int ) dir ) );
        return instance;
    }

    public void ClearTilemap( tk2dTileMap tm )
    {
        for( int y = 0; y < tm.height; y++ )
            for( int x = 0; x < tm.width; x++ )
                for( int l = 0; l < tm.Layers.Length; l++ )
                {
                    tm.ClearTile( x, y, l );
                }
        tm.ForceBuild();
    }

    public EDirection GetArrowDir( Vector2 pos )
    {
        if( Map.PtOnMap( Tilemap, pos ) == false ) return EDirection.NONE;
        EDirection dr = EDirection.NONE;
        Unit arrow = GetUnit( ETileType.ARROW, pos );

        if( arrow )
        {
            dr = arrow.Dir;
            if( arrow.Body.isWorking == false )
                dr = EDirection.NONE;
        }
        return dr;
    }
    
    //__________________________________________________________________________________________________ Checks for Arrow Blockage

    public bool CheckArrowBlockFromTo( Vector2 from, Vector2 to, Unit _unit, bool ignoreMessage = false, int inlev = -1, int outlev= -1 )
    {
        if( !PtOnMap( Tilemap, from ) ) return false;
        if( _unit != null && _unit.Control.BlockedByArrow == false ) return false;
        if( inlev == -1 && outlev == -1 )
        if( from == to ) return false;

        EDirection over = GetArrowDir( from );
        EDirection dest = GetArrowDir( to );
        Vector2 dif = new Vector2( from.x - to.x, from.y - to.y );

        if( BabyData.CheckWaterArrow )                                   // water arrow check (fishing)
        {
            if( BabyData.OverArrowDir != EDirection.NONE )
            {
                over = BabyData.OverArrowDir;
                //dest = EDirection.NONE;
                inlev = 5;
                //dif = dif * -1;
            }
            else return false;
        }

        bool showmsg = false;
        if( _unit != null && _unit.UnitType == EUnitType.HERO )
            showmsg = true;
        if( ignoreMessage ) showmsg = false;
        showmsg = false;

        //if( level == -1 )
        //level = _unit.Control.ArrowWalkingLevel;

        //if( level < 1 )    // No arrow movement
        //{
        //    if( dest != EDirection.NONE )
        //    {
        //        if( showmsg )
        //        ShowMessage( Language.Get( "ERROR_ARROWSTEPFORBIDDEN1" ) );
        //        return true;
        //    }
        //    return false;
        //}

        //if( level >= 5 ) return false;
        if( over == EDirection.NONE && dest == EDirection.NONE ) return false;
        int In = 0;
        int Out = 0;

        if( _unit != null )
        {
            In = ( int ) _unit.Control.ArrowInLevel;
            Out = ( int ) _unit.Control.ArrowOutLevel;
        }
        else                                                                                // Overide, Check for bugs
        {
            In = 3;
            Out = 3;
        }

        if( inlev != -1 ) In = inlev;
        if( outlev != -1 ) Out = outlev;
        if( inlev == -2 ) In = 5;                                                           // Only exiting check

        if( _unit != null && _unit.UnitType == EUnitType.MONSTER )
        {
            if( over != EDirection.NONE )
            {
                if( G.Hero.Control.ArrowInLevel >= 4 ) Out -= 1;                                      // Hero decreases Monster out arrow walkability
                if( G.Hero.Control.ArrowInLevel >= 5 ) Out -= 1;
            }

            if( dest != EDirection.NONE )
            {
                if( G.Hero.Control.ArrowOutLevel >= 4 ) In -= 1;                                       // Hero decreases Monster In arrow walkability
                if( G.Hero.Control.ArrowOutLevel >= 5 ) In -= 1;
            }
        }

        bool front = false;

        if( In < 2 )                                                                                  // Only same direction IN arrow movement
        {
            if( dest == EDirection.N && dif.x == 0 && dif.y == -1 ) front = true;
            if( dest == EDirection.NE && dif.x == -1 && dif.y == -1 ) front = true;
            if( dest == EDirection.E && dif.x == -1 && dif.y == 0 ) front = true;
            if( dest == EDirection.SE && dif.x == -1 && dif.y == 1 ) front = true;
            if( dest == EDirection.S && dif.x == 0 && dif.y == 1 ) front = true;
            if( dest == EDirection.SW && dif.x == 1 && dif.y == 1 ) front = true;
            if( dest == EDirection.W && dif.x == 1 && dif.y == 0 ) front = true;
            if( dest == EDirection.NW && dif.x == 1 && dif.y == -1 ) front = true;
            if( dest == EDirection.NONE ) front = true;
            if( In < 1 ) front = false;
        }
        else front = true;

        if( front == false )
        {
            if( showmsg )
                ShowMessage( Language.Get( "ERROR_ARROWSTEPFORBIDDEN2" ) );
            return true;
        }

        front = false;
        if( Out < 2 )                                                                                // Only same direction OUT arrow movement
        {
            if( over == EDirection.N && dif.x == 0 && dif.y == -1 ) front = true;
            if( over == EDirection.NE && dif.x == -1 && dif.y == -1 ) front = true;
            if( over == EDirection.E && dif.x == -1 && dif.y == 0 ) front = true;
            if( over == EDirection.SE && dif.x == -1 && dif.y == 1 ) front = true;
            if( over == EDirection.S && dif.x == 0 && dif.y == 1 ) front = true;
            if( over == EDirection.SW && dif.x == 1 && dif.y == 1 ) front = true;
            if( over == EDirection.W && dif.x == 1 && dif.y == 0 ) front = true;
            if( over == EDirection.NW && dif.x == 1 && dif.y == -1 ) front = true;
            if( over == EDirection.NONE ) front = true;
            else
                if( Out < 1 ) front = false;
        }
        else front = true;

        if( front == false )
        {
            if( showmsg )
                ShowMessage( Language.Get( "ERROR_ARROWSTEPFORBIDDEN2" ) );
            return true;
        }

        bool ok = false;
        if( over != EDirection.NONE && Out < 3 )                                                                // Leaving arrow
        {
            if( dif.x == 0 && dif.y == -1 )
                if( over == EDirection.S || over == EDirection.SE ||
                    over == EDirection.SW || over == EDirection.E || over == EDirection.W ) ok = true; // to N

            if( dif.x == 0 && dif.y == 1 )
                if( over == EDirection.N || over == EDirection.NE ||
                    over == EDirection.NW || over == EDirection.E || over == EDirection.W ) ok = true; // to S

            if( dif.x == 1 && dif.y == 0 )
                if( over == EDirection.E || over == EDirection.NE ||
                    over == EDirection.SE || over == EDirection.N || over == EDirection.S ) ok = true; // to W

            if( dif.x == -1 && dif.y == 0 )
                if( over == EDirection.W || over == EDirection.NW ||
                    over == EDirection.SW || over == EDirection.N || over == EDirection.S ) ok = true; // to E


            if( dif.x == 1 && dif.y == -1 )
                if( over == EDirection.S || over == EDirection.SE ||
                    over == EDirection.E || over == EDirection.NE || over == EDirection.SW ) ok = true; // to NW

            if( dif.x == -1 && dif.y == +1 )
                if( over == EDirection.N || over == EDirection.NW ||
                    over == EDirection.W || over == EDirection.SW || over == EDirection.NE ) ok = true; // to SE

            if( dif.x == -1 && dif.y == -1 )
                if( over == EDirection.W || over == EDirection.SW ||
                    over == EDirection.S || over == EDirection.NW || over == EDirection.SE ) ok = true; // to NE

            if( dif.x == 1 && dif.y == +1 )
                if( over == EDirection.E || over == EDirection.NE ||
                    over == EDirection.N || over == EDirection.NW || over == EDirection.SE ) ok = true; // to SW
        }

        if( inlev != -2 )
        if( !PtOnMap( Tilemap, to ) ) return false;
        if( ( int ) dest >= ( int ) EDirection.N && ( int ) dest <= ( int ) EDirection.NW && In < 3 )         // Entering arrow
        {
            if( dif.x == 0 && dif.y == -1 )
                if( dest == EDirection.S || dest == EDirection.SE ||
                    dest == EDirection.SW || dest == EDirection.E || dest == EDirection.W ) ok = true; // to N

            if( dif.x == 0 && dif.y == +1 )
                if( dest == EDirection.N || dest == EDirection.NE ||
                    dest == EDirection.NW || dest == EDirection.E || dest == EDirection.W ) ok = true; // to S

            if( dif.x == 1 && dif.y == 0 )
                if( dest == EDirection.E || dest == EDirection.NE ||
                    dest == EDirection.SE || dest == EDirection.N || dest == EDirection.S ) ok = true; // to W

            if( dif.x == -1 && dif.y == 0 )
                if( dest == EDirection.W || dest == EDirection.NW ||
                    dest == EDirection.SW || dest == EDirection.N || dest == EDirection.S ) ok = true; // to E


            if( dif.x == 1 && dif.y == -1 )
                if( dest == EDirection.S || dest == EDirection.SE ||
                    dest == EDirection.E || dest == EDirection.NE || dest == EDirection.SW ) ok = true; // to NW

            if( dif.x == -1 && dif.y == +1 )
                if( dest == EDirection.N || dest == EDirection.NW ||
                    dest == EDirection.W || dest == EDirection.SW || dest == EDirection.NE ) ok = true; // to SE

            if( dif.x == -1 && dif.y == -1 )
                if( dest == EDirection.W || dest == EDirection.SW ||
                    dest == EDirection.S || dest == EDirection.NW || dest == EDirection.SE ) ok = true; // to NE

            if( dif.x == 1 && dif.y == +1 )
                if( dest == EDirection.E || dest == EDirection.NE ||
                    dest == EDirection.N || dest == EDirection.SE || dest == EDirection.NW ) ok = true; // to SW
        }

        if( ok == true )
        {
            if( showmsg )
                ShowMessage( Language.Get( "ERROR_ARROWSTEPFORBIDDEN3" ) );
            return true;
        }

        if( over != EDirection.NONE && Out < 4 )                                                                // Leaving arrow
        {
            if( dif.x == 0 && dif.y == -1 )
                if( over == EDirection.S || over == EDirection.SE || over == EDirection.SW ) ok = true; // to N

            if( dif.x == 0 && dif.y == 1 )
                if( over == EDirection.N || over == EDirection.NE || over == EDirection.NW ) ok = true; // to S

            if( dif.x == 1 && dif.y == 0 )
                if( over == EDirection.E || over == EDirection.NE || over == EDirection.SE ) ok = true; // to W

            if( dif.x == -1 && dif.y == 0 )
                if( over == EDirection.W || over == EDirection.NW || over == EDirection.SW ) ok = true; // to E


            if( dif.x == 1 && dif.y == -1 )
                if( over == EDirection.S || over == EDirection.SE || over == EDirection.E ) ok = true; // to NW

            if( dif.x == -1 && dif.y == +1 )
                if( over == EDirection.N || over == EDirection.NW || over == EDirection.W ) ok = true; // to SE

            if( dif.x == -1 && dif.y == -1 )
                if( over == EDirection.W || over == EDirection.SW || over == EDirection.S ) ok = true; // to NE

            if( dif.x == 1 && dif.y == +1 )
                if( over == EDirection.E || over == EDirection.NE || over == EDirection.N ) ok = true; // to SW
        }

        if( inlev != -2 )
        if( !PtOnMap( Tilemap, to ) ) return false;
        if( ( int ) dest >= ( int ) EDirection.N && ( int ) dest <= ( int ) EDirection.NW && In < 4 )         // Entering arrow
        {
            if( dif.x == 0 && dif.y == -1 )
                if( dest == EDirection.S || dest == EDirection.SE || dest == EDirection.SW ) ok = true; // to N

            if( dif.x == 0 && dif.y == +1 )
                if( dest == EDirection.N || dest == EDirection.NE || dest == EDirection.NW ) ok = true; // to S

            if( dif.x == 1 && dif.y == 0 )
                if( dest == EDirection.E || dest == EDirection.NE || dest == EDirection.SE ) ok = true; // to W

            if( dif.x == -1 && dif.y == 0 )
                if( dest == EDirection.W || dest == EDirection.NW || dest == EDirection.SW ) ok = true; // to E


            if( dif.x == 1 && dif.y == -1 )
                if( dest == EDirection.S || dest == EDirection.SE || dest == EDirection.E ) ok = true; // to NW

            if( dif.x == -1 && dif.y == +1 )
                if( dest == EDirection.N || dest == EDirection.NW || dest == EDirection.W ) ok = true; // to SE

            if( dif.x == -1 && dif.y == -1 )
                if( dest == EDirection.W || dest == EDirection.SW || dest == EDirection.S ) ok = true; // to NE

            if( dif.x == 1 && dif.y == +1 )
                if( dest == EDirection.E || dest == EDirection.NE || dest == EDirection.N ) ok = true; // to SW
        }

        if( ok == true )
        {
            if( showmsg )
                ShowMessage( Language.Get( "ERROR_ARROWSTEPFORBIDDEN4" ) );
            return true;
        }


        if( over != EDirection.NONE && Out < 5 )                                                                // Leaving arrow
        {
            if( dif.x == 0 && dif.y == -1 )
                if( over == EDirection.S ) ok = true; // to N

            if( dif.x == 0 && dif.y == 1 )
                if( over == EDirection.N ) ok = true; // to S

            if( dif.x == 1 && dif.y == 0 )
                if( over == EDirection.E ) ok = true; // to W

            if( dif.x == -1 && dif.y == 0 )
                if( over == EDirection.W ) ok = true; // to E


            if( dif.x == 1 && dif.y == -1 )
                if( over == EDirection.SE ) ok = true; // to NW

            if( dif.x == -1 && dif.y == +1 )
                if( over == EDirection.NW ) ok = true; // to SE

            if( dif.x == -1 && dif.y == -1 )
                if( over == EDirection.SW ) ok = true; // to NE

            if( dif.x == 1 && dif.y == +1 )
                if( over == EDirection.NE ) ok = true; // to SW
        }

        if( inlev != -2 )
        if( !PtOnMap( Tilemap, to ) ) return false;
        if( ( int ) dest >= ( int ) EDirection.N && ( int ) dest <= ( int ) EDirection.NW && In < 5 )         // Entering arrow
        {
            if( dif.x == 0 && dif.y == -1 )
                if( dest == EDirection.S ) ok = true; // to N

            if( dif.x == 0 && dif.y == +1 )
                if( dest == EDirection.N ) ok = true; // to S

            if( dif.x == 1 && dif.y == 0 )
                if( dest == EDirection.E ) ok = true; // to W

            if( dif.x == -1 && dif.y == 0 )
                if( dest == EDirection.W ) ok = true; // to E


            if( dif.x == 1 && dif.y == -1 )
                if( dest == EDirection.SE ) ok = true; // to NW

            if( dif.x == -1 && dif.y == +1 )
                if( dest == EDirection.NW ) ok = true; // to SE

            if( dif.x == -1 && dif.y == -1 )
                if( dest == EDirection.SW ) ok = true; // to NE

            if( dif.x == 1 && dif.y == +1 )
                if( dest == EDirection.NE ) ok = true; // to SW
        }

        if( ok == true )
        {
            if( showmsg )
                ShowMessage( Language.Get( "ERROR_ARROWSTEPFORBIDDEN5" ) );
            return true;
        }

        return false;
    }

    public static void DeleteInvalidAreas( tk2dTileMap tm )
    {
        //Area[] arl = Quest.I.CurLevel.AreaFolder.transform.GetComponentsInChildren<Area>();

        //for( int a = 0; a < arl.Length; a++ )
        //{
        //    ETileType tile = ( ETileType ) tm.GetTile( ( int ) arl[ a ].P1.x, ( int ) arl[ a ].P1.y, ( int ) ELayerType.MODIFIER );
        //    if( tile != ETileType.BOTTOMLEFT_AREALIMITER )
        //        DestroyImmediate( arl[ a ].gameObject );
        // }
    }


    public bool SetAreaID( ref tk2dTileMap tm, ref List<Vector2> tlist, Vector2 pos, ref int id, ETileType tgtile )
    {
        if( Map.PtOnMap( tm, pos ) == false ) return false;

        ETileType tile = ( ETileType ) tm.GetTile( ( int ) pos.x, ( int ) pos.y, ( int ) ELayerType.AREAS );

        if( tile != tgtile ) return false;
        if( AreaID[ ( int ) pos.x, ( int ) pos.y ] > -1 ) return false;

        AreaID[ ( int ) pos.x, ( int ) pos.y ] = id;
        tlist.Add( pos );
        id++;

        SetAreaID( ref tm, ref tlist, new Vector2( pos.x - 1, pos.y ), ref id, tgtile );
        SetAreaID( ref tm, ref tlist, new Vector2( pos.x + 1, pos.y ), ref id, tgtile );
        SetAreaID( ref tm, ref tlist, new Vector2( pos.x, pos.y - 1 ), ref id, tgtile );
        SetAreaID( ref tm, ref tlist, new Vector2( pos.x, pos.y + 1 ), ref id, tgtile );

        return true;
    }

    public void CreateArea( Level level, ref List<Area> areaList, Sector sec, ref List<Vector2> tlist, ETileType drag )
    {
        Vector2 p1 = new Vector2( 999, -999 );
        Vector2 p2 = new Vector2( -999, 999 );

        for( int i = 0; i < tlist.Count; i++ )
        {
            if( p2.x < tlist[ i ].x ) p2.x = tlist[ i ].x;
            if( p2.y > tlist[ i ].y ) p2.y = tlist[ i ].y;
            if( p1.x > tlist[ i ].x ) p1.x = tlist[ i ].x;
            if( p1.y < tlist[ i ].y ) p1.y = tlist[ i ].y;
        }

        Rect r = new Rect( p1.x, p1.y, p2.x - p1.x + 1, p1.y - p2.y + 1 );
        Vector2 pt = Vector2.zero;
        
        if( tlist != null && tlist.Count > 0 ) pt = tlist[ 0 ];

        GameObject obj = CreateObjInstance( "Area", "Area " + p1.x + " " + p1.y, EDirection.NONE, new Vector3( pt.x, pt.y, 0 ) );
        Area ar = obj.GetComponent<Area>();
        ar.AreaRect = r;
        ar.Sector = sec;
        if( tlist != null && tlist.Count > 0 ) ar.TileCount = tlist.Count; 
        ar.P1 = p1;
        ar.P2 = p2;
        ar.gameObject.transform.parent = level.AreaFolder.transform;
        ar.transform.position = new Vector3( r.center.x - 0.5f, r.yMin - ( ( p1.y - p2.y ) / 2 ), -0.25f );
        ar.Spr.scale = new Vector3( 1, 1, 1 );
        ar.Spr.dimensions = new Vector3( 20 * r.width, 20 * r.height, 0 );
        ar.Spr.gameObject.SetActive( false );
        ar.AreaName = Area.GetRandomAreaName();
        areaList.Add( ar );
        ar.SectorID = areaList.Count - 1;
        ar.GlobalID = Quest.I.CurLevel.AreaList.Count;

        ar.TL = new List<Vector2>();
        ar.TL.AddRange( tlist );
        ar.UpdateAreaColor();
        ar.AreaDragID = drag;

        if( Quest.CurrentDungeon != -1 ) Quest.I.Dungeon.AreaList.Add( ar );
    }


    //_____________________________________________________________________________________________________________________ Creates Areas on map

    public void CreateAreas( tk2dTileMap tm, int lv, Level level, Rect area, ref List<Area> areaList, Sector s, SectorDefinition SD )
    {
        if( tm == null ) return;
        if( level == null ) return;

        //Debug.Log( "Creating areas: Level " + lv );

        if( lv <= -1 ) return;

        int minx = 0;
        int miny = 0;
        int maxx = tm.width;
        int maxy = tm.height;

        if( area.x != -1 )
        {
            minx = ( int ) area.xMin;
            maxx = ( int ) area.xMax;
            miny = ( int ) area.yMin;
            maxy = ( int ) area.yMax;
        }

        for( int i = 0; i < 4; i++ )
        {
            List<Vector2> tlist = new List<Vector2>();                                                  
            ETileType tl = ETileType.AREA_DRAG;
            bool conected = false;
            if( SD.IslandsRedAreaMode ) conected = true;
            if( i == 1 ) { tl = ETileType.AREA_DRAG2; if( SD.IslandsGreenAreaMode  ) conected = true; }
            if( i == 2 ) { tl = ETileType.AREA_DRAG3; if( SD.IslandsBlueAreaMode   ) conected = true; }
            if( i == 3 ) { tl = ETileType.AREA_DRAG4; if( SD.IslandsYellowAreaMode ) conected = true; }

            if( conected )
            {
                for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )                          // This creates a CONTIGUOUS area list that needs to be connected
                for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
                if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
                 {
                     ETileType tile = ( ETileType ) tm.GetTile( xx, yy, ( int ) ELayerType.AREAS );
                     if( tile == tl )
                     if( AreaID[ xx, yy ] < 0 )
                         {
                             SetAreaID( ref tm, ref tlist, new Vector2( xx, yy ), ref Map.I.TotalAreas, tile );
                             if( tlist.Count > 0 )
                                 CreateArea( level, ref areaList, s, ref tlist, tile );
                         }
                 }
            }
            else
            {
                for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )                              // This creates a NON CONTIGUOUS area list
                for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
                 if( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
                 {
                     ETileType tile = ( ETileType ) tm.GetTile( xx, yy, ( int ) ELayerType.AREAS );
                     if( tile == ( ETileType ) tl )
                     {
                         AreaID[ xx, yy ] = Map.I.TotalAreas;
                         tlist.Add( new Vector2( xx, yy ) );
                     }
                 }
                CreateArea( level, ref areaList, s, ref tlist, ( ETileType ) tl );                               // Creates the Only one area for that color
                Map.I.TotalAreas++;
            }
        } 

        Vector2 max = new Vector2( 25, 20 );
        bool oldstate = level.gameObject;

        Area[ ] al = level.AreaFolder.transform.GetComponentsInChildren<Area>();

        level.gameObject.SetActive( oldstate );
    }

    public static Vector2 GetNextTileInLine( tk2dTileMap tm, Vector2 orig, Vector2 dir, ETileType tgtile, int range, ELayerType layer )
    {
        for( int i = 0; i < range; i++ )
        {
            Vector2 tg = orig + new Vector2( dir.x * i, dir.y * i );
            if( Map.PtOnMap( tm, tg ) )
            {
                int tile = tm.GetTile( ( int ) tg.x, ( int ) tg.y, ( int ) layer );
                if( ( int ) tgtile == tile ) return tg;
            }
            else break;
        }
        return new Vector2( -1, -1 );
    }

    public static int GetTileLineSize( tk2dTileMap tm, Vector2 orig, Vector2 dir, ETileType tgtile, ref Vector2 last, ELayerType layer = ELayerType.MODIFIER )
    {
        last = orig;
        for( int size = 1; size < Sector.TSX; size++ )
        {
            Vector2 tg = orig + new Vector2( dir.x * size, dir.y * size );
            if( Map.PtOnMap( tm, tg ) == false ) return size;
            int tile = tm.GetTile( ( int ) tg.x, ( int ) tg.y, ( int ) layer );
            if( ( int ) tgtile != tile ) return size; else last = tg;
        }
        return -1;
    }

    public static bool PtOnMap( tk2dTileMap tm, Vector2 pt )
    {
        if( pt.x < 0 || pt.x >= tm.width ||
             pt.y < 0 || pt.y >= tm.height ) return false;
        return true;
    }

    public bool PtOnAreaMap( Vector2 pt )
    {
        if( CurrentArea == -1 ) return false;
        if( pt.x < P1.x || pt.x > P2.x ||
             pt.y > P1.y || pt.y < P2.y ) return false;
        if( AreaID[ ( int ) pt.x, ( int ) pt.y ] < 0 ) return false;
        if( AreaID[ ( int ) pt.x, ( int ) pt.y ] != CurrentArea ) return false;

        return true;
    }

    public bool UpdateMouseTile()
    {
        Vector3 mp = Input.mousePosition;

        MouseCord = Camera.main.WorldToScreenPoint( mp );

        float lim = Util.Percent( 17, Screen.height );

        UI.I.MouseOverUI = false;
        if( mp.y < lim )
        {
            Mtx = Mty = -1;
            UI.I.MouseOverUI = true;
            return false;
        }

        Mtx = Mty = -1;
        Tilemap.GetTileAtPosition( Manager.I.Camera.ScreenToWorldPoint( mp ), out Mtx, out Mty );
        G.MP = new Vector2( Mtx, Mty );

        if( Mtx >= Tilemap.width ) { Mtx = Mty = -1; }
        if( Mtx < 0 ) { Mtx = Mty = -1; }
        if( Mty >= Tilemap.height ) { Mtx = Mty = -1; }
        if( Mty < 0 ) { Mtx = Mty = -1; }

        return true;
    }

    public EDirection RotateDir( EDirection direction, int val )
    {
        int dir = ( int ) direction;

        for( ; ; )
        {
            if( val > 0 ) dir--;
            if( val < 0 ) dir++;
            if( dir < 0 ) dir = 7;
            if( dir > 7 ) dir = 0;
            if( val > 0 ) val--;
            if( val < 0 ) val++;
            if( val == 0 ) return ( EDirection ) dir;
        }
    }
    public void ClearTransTile( int x, int y, int layer )
    {
        TransTileMap.SetTile( ( x * 2 ) + 0, ( y * 2 ) + 1, layer, -1 );
        TransTileMap.SetTile( ( x * 2 ) + 1, ( y * 2 ) + 1, layer, -1 );
        TransTileMap.SetTile( ( x * 2 ) + 0, ( y * 2 ) + 0, layer, -1 );
        TransTileMap.SetTile( ( x * 2 ) + 1, ( y * 2 ) + 0, layer, -1 );
    }

    public static bool FinalizeMap( tk2dTileMap tm )
    {
        bool res = Map.CheckForErrors( tm );
        if( !res ) return false;
        Quest.I = GameObject.Find( "Quest" ).GetComponent<Quest>();
        if( tm.name == "----------------Navigation Map" ) return true;
        if( tm.transform.parent.name == "Dungeon Areas" ) return true;
        for( int i = 0; i < Quest.I.LabList.Length; i++ )
            if( tm.transform.parent.name == "Dungeon " + i ) return true;

        // Map.I.CreateAreas( tm, Quest.CurrentLevel, Quest.I.CurLevel, new Rect( -1, -1, -1, -1 ), ref Quest.I.CurLevel.AreaList, null );
        //  Map.DeleteInvalidAreas( tm );
        return true;
    }

    public static bool CheckForErrors( tk2dTileMap tm )
    {
        return true; /////////////
        Vector2 pt = new Vector2();
        if( tm.name == "Areas Template Tilemap" )
            for( pt.y = 0; pt.y < tm.height; pt.y++ )
                for( pt.x = 0; pt.x < tm.width; pt.x++ )
                {
                    ETileType dec = ( ETileType ) tm.GetTile( ( int ) pt.x, ( int ) pt.y, ( int ) ELayerType.DECOR );
                    if( dec != ETileType.NONE )
                        if( pt.x == 0 || pt.x == 28 || pt.y == 0 || pt.y == 28 )
                        {
                            Debug.LogError( "Error: Decor on border." ); return false;
                        }

                    ETileType raft = ( ETileType ) tm.GetTile( ( int ) pt.x, ( int ) pt.y, ( int ) ELayerType.RAFT );
                    ETileType mn = ( ETileType ) tm.GetTile( ( int ) pt.x, ( int ) pt.y, ( int ) ELayerType.MONSTER );
                    if( raft == ETileType.RAFT && mn == ETileType.HERB )
                    {
                        Debug.LogError( "Error: No Herb over raft allowed." ); return false;
                    }
                }


        string err = "";
        for( int l = 0; l < 5; l++ )
            for( pt.y = 0; pt.y < tm.height; pt.y++ )
                for( pt.x = 0; pt.x < tm.width; pt.x++ )
                {
                    ETileType tile = ( ETileType ) tm.GetTile( ( int ) pt.x, ( int ) pt.y, l );

                    ELayerType right = GetTileLayer( tile );

                    if( right == ELayerType.NONE ) { }
                    else
                        if( ( ELayerType ) l != right )
                        {
                            err += "Tilemap Error: " + tile + " on layer " + l + " at: " + pt.x + " " + pt.y;
                            tm.SetTile( ( int ) pt.x, ( int ) pt.y, ( int ) right, ( int ) tile );
                            tm.SetTile( ( int ) pt.x, ( int ) pt.y, l, -1 );
                            tm.ForceBuild();
                        }
                }

        if( err != "" ) { Debug.Log( "Tilemap Error" + err ); return false; }
        return true;
    }

    public List<Vector2> GetLineCords( Vector2 pt1, Vector2 pt2, float distFactor = 1 )
    {
        List<Vector2> vl = new List<Vector2>();

        int x0 = ( int ) pt1.x;
        int y0 = ( int ) pt1.y;
        int x1 = ( int ) pt2.x;
        int y1 = ( int ) pt2.y;
        int dx, dy, dx2, dy2, x_inc, y_inc, error, index;

        Vector2 elev = new Vector2( 0, 0 );

        dx = x1 - x0;
        dy = y1 - y0;

        dx *= ( int ) distFactor;
        dy *= ( int ) distFactor;

        int l = 1;

        // test which direction the line is going in i.e. slope angle
        if( dx >= 0 ) { x_inc = l; } // end if line is moving right
        else { x_inc = -l; dx = -dx; } // end else moving left

        if( dy >= 0 ) { y_inc = l; }
        else { y_inc = -l; dy = -dy; } // end else moving up

        // compute (dx,dy) * 2
        dx2 = dx << 1;
        dy2 = dy << 1;
        // now based on which delta is greater we can draw the line
        if( dx > dy )
        {
            // initialize error term
            error = dy2 - dx;
            // draw the line
            for( index = 0; index <= dx / l; index++ )
            {
                vl.Add( new Vector2( x0 + elev.x, y0 + elev.y ) );

                // test if error has overflowed
                if( error >= 0 )
                {
                    error -= dx2;
                    // move to next line
                    y0 += y_inc;

                } // end if error overflowed
                // adjust the error term
                error += dy2;
                // move to the next pixel
                x0 += x_inc;

            } // end for
        } // end if |slope| <= 1
        else
        {
            // initialize error term
            error = dx2 - dy;
            // draw the line
            for( index = 0; index <= dy / l; index++ )
            {
                vl.Add( new Vector2( x0 + elev.x, y0 + elev.y ) );

                // test if error overflowed
                if( error >= 0 )
                {
                    error -= dy2;
                    // move to next line
                    x0 += x_inc;
                } // end if error overflowed

                // adjust the error term
                error += dx2;
                // move to the next pixel
                y0 += y_inc;
            } // end for
        } // end else |slope| > 1
        // return success

        return vl;
    }

    public Unit GetUnit( Vector2 pos, ELayerType layer, bool onlyinside = false )
    {
        if( PtOnMap( Tilemap, pos ) )
            if( onlyinside == false || PtOnAreaMap( pos ) )
            {
                if( layer == ELayerType.MONSTER )
                    if( Unit[ ( int ) pos.x, ( int ) pos.y ] )
                        return Unit[ ( int ) pos.x, ( int ) pos.y ];

                if( layer == ELayerType.GAIA )
                    if( Gaia[ ( int ) pos.x, ( int ) pos.y ] )
                        return Gaia[ ( int ) pos.x, ( int ) pos.y ];

                if( layer == ELayerType.GAIA2 )
                    if( Gaia2[ ( int ) pos.x, ( int ) pos.y ] )
                        return Gaia2[ ( int ) pos.x, ( int ) pos.y ];
            }
        return null;
    }

    public bool HasLOS( Vector2 pt1, Vector2 pt2, bool arrow = false, Unit unit = null, bool firstpoint = false, bool lastpoint = false, string options = "", int maxRange = -1)
    {
        List<Vector2> LineCord = GetLineCords( pt1, pt2 );

        bool dragonshot = false;
        if( options == "Dragon Shot" ) dragonshot = true;

        int first = 1;
        if( firstpoint ) first = 0;
        int last = 1;
        if( lastpoint ) last = 0;
        for( int i = first; i < LineCord.Count - last; i++ )
        {
            if( maxRange != -1 )
            if( i > maxRange ) return false;
            Vector2 tg = LineCord[ i ];
            Map.I.LastLOSPos = tg;
            if( Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ] )
                if( Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ].BlockSight ) return false;

            if( Map.I.Gaia2[ ( int ) tg.x, ( int ) tg.y ] )
                if( Map.I.Gaia2[ ( int ) tg.x, ( int ) tg.y ].BlockSight ) return false;

            if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ] )
                if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ].Activated )
                {
                    if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ].BlockSight ) return false;
                    if( dragonshot == false )
                    {
                    }
                    else
                        if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ].TileID == ETileType.BARRICADE )                                                      // Dragon Shot Blocked by Barricade
                        {
                            if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ].Body.TouchCount > 0 )
                            {
                                float chance = G.Hero.Control.DragonBarricadeProtection *
                                ( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ].Variation + 1 );
                                if( Util.Chance( chance ) )
                                    return false;
                            }
                        }
                }

            if( arrow && i >= 1 )
            if( Map.I.CheckArrowBlockFromTo( LineCord[ i - 1 ], tg, unit ) ) return false;
        }

        if( LineCord.Count >= 2 && pt1 != pt2 )
            Map.I.InfectedScarabAttackSource = LineCord[ LineCord.Count - 2 ];                                                                           // this is for precise shield blocking
        return true;
    }

    public int UpdateScoutLOS( Vector2 pt1, Vector2 pt2 )
    {
        return 0;
        if( Manager.I.GameType == EGameType.FARM ||
            Manager.I.GameType == EGameType.NAVIGATION ) return -1;
        List<Vector2> LineCord = GetLineCords( pt1, pt2 );
        int cont = 0;
        bool rev = true;
        for( int i = 0; i < LineCord.Count - 2; i++ )
        {
            Vector2 tg = LineCord[ i ];
            bool block = false;
            if( GetPosArea( tg ) != -1 )
            {
                block = true; rev = false;
                if( RM.RMD.RevealAreaFromAfar ) rev = true;
            }

            Sector s = Sector.GetPosSector( tg );
            if( s )
            {
                if( s.Type == Sector.ESectorType.GATES )
                {
                    block = true; rev = true;
                }
                else
                    if( s.Discovered == false )
                    {
                        block = true; rev = false;
                    }
            }

            RevealFactor[ ( int ) tg.x, ( int ) tg.y ] = 0;

            if( Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ] )
                if( Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ].BlockSight ) block = true;

            if( Map.I.Gaia2[ ( int ) tg.x, ( int ) tg.y ] )
                if( Map.I.Gaia2[ ( int ) tg.x, ( int ) tg.y ].BlockSight ) block = true;

            if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ] )
                if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ].Activated )
                {
                    if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ].TileID == ETileType.BARRICADE )
                    {
                        if( ( int ) G.Hero.Control.OverBarricadeScoutLevel <                                               // Over barricade scout level
                            ( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ].Variation + 1 ) ) block = true;
                    }
                    else
                        if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ].BlockSight ) block = true;
                }

            if( block )
            {
                if( rev ) RevealFactor[ ( int ) tg.x, ( int ) tg.y ] = 1;
                return cont;
            }
            else
            {
                if( RevealFactor[ ( int ) tg.x, ( int ) tg.y ] == 0 ) cont++;
                RevealFactor[ ( int ) tg.x, ( int ) tg.y ] = 1;
            }
        }
        return cont;
    }
   
    //______________________________________________________________________________________________________________________ Update Camera

    public void UpdateCamera()
    {
        if( Hero == null ) return;
        if( Tilemap == null ) return;

        Vector3 target = new Vector3( Hero.transform.position.x, 
                                      Hero.transform.position.y - 0.75f, -10 );
        int zm = ZoomMode;

        int DaytimeLimit = 60;
        if( Map.I.Lights.DayLight < DaytimeLimit ) 
            zm = 0;                                                                       // to avoid ilumination bug

        int fz = ( int ) Item.GetNum( ItemType.Res_ForcedZoom );
        if( fz >= 0 ) zm = fz;

        if( CubeData.I.FixedZoomMode != -1 )                                              // Cubedata Forced zoom
        {
            fz = CubeData.I.FixedZoomMode;
            zm = fz;
        }

        bool freeCamAreaZoom = false;
        if( fz >= 10 ) freeCamAreaZoom = true;
        if( Manager.I.GameType != EGameType.CUBES )
            fz = -1;
        bool focusHero = false;
        float zoom = CData.RoamingCameraZoom;
        float ZoomFXTime = 4;
        float frontDist = 0;
        if( G.HS && G.HS.Type == Sector.ESectorType.LAB ) 
            zm = 2;

        int md = -1;                                      
        int ori = -1;
        Unit cm = Map.I.GetUnit( ETileType.CAMERA, G.Hero.Pos );                          // Camera object
        Unit oldcm = Map.I.GetUnit( ETileType.CAMERA, G.Hero.Control.OldPos );
        if( cm )
        {
            md = ( int ) Mod.GetModInTile( G.Hero.Pos );
            ori = ( int ) Quest.I.Dungeon.Tilemap.GetTile( ( int ) 
            G.Hero.Pos.x, ( int ) G.Hero.Pos.y, ( int ) ELayerType.AREAS );
        }

        if( oldcm && cm == null )                                                 // step out of camera object
            HideVegetation = false;

        if( cm )                                                                  // camera obj zoom by ori
        {
            if( ori == 22 ) { Map.I.ZoomMode = 0; zm = 0; }
            if( ori == 23 ) { Map.I.ZoomMode = 1; zm = 1; }
            if( ori == 24 ) { Map.I.ZoomMode = 2; zm = 2; }
            if( ori == 38 ) { Map.I.ZoomMode = 3; zm = 3; }

            if( ori != -1 )                                                       // disables custom camera
            {
                CamDataID = -1;
                CamAreaStepped = false;
                CamArea = new Rect( 0, 0, 0, 0 );  
                ObjectCameraMod = -1;                
            }
        }

        if( md >= 0 && md <= 27 || ObjectCameraMod != -1 )                        // custom camera by mod# based on CamDataID
        {
            if( md != -1 )
                ObjectCameraMod = md + 2;
            CamDataID = ObjectCameraMod;
            CamAreaStepped = true;
            CamArea = new Rect( 0, 0, ObjectCameraMod, ObjectCameraMod );
            focusHero = true;
            zm = 0;
            frontDist = 0;
            if( CamDataID == 2 ) frontDist -= 1;                                  // Decrease target frontal distance if zoom is too close
            if( CamDataID == 3 ) frontDist -= 1;
            if( CamDataID == 4 ) frontDist -= 1;
            if( CamDataID == 5 ) frontDist -= 1;
        }

        if( CurrentArea == -1 || AreaCleared )
        {
            if( zm == 0 ) CData.RoamingCameraZoom = 1.6f;
            if( zm == 1 ) CData.RoamingCameraZoom = 1.0f;
            if( zm == 2 ) CData.RoamingCameraZoom = .7f;

            if( Map.I.FishingMode != EFishingPhase.NO_FISHING )
                CData.RoamingCameraZoom = 2f;
        }
        zoom = CData.RoamingCameraZoom;

        float waittime = .4f;       
        if( Input.GetKey( KeyCode.LeftControl ) )                                             // Hide Vegetation keypress
        if( fz < 0 )
        {
            HideVegetationTimer += Time.deltaTime;
            if( HideVegetationTimer > waittime )
            if( Map.I.Lights.DayLight > DaytimeLimit )
            {
                UI.I.UIFolder.gameObject.SetActive( false );
                HideVegetation = true;
                UI.I.BackgroundUI.gameObject.SetActive( false );
                HideVegetationTimer = 0;
            }
        }

        DecorLayer1.SetActive( true );
        DecorLayer2.SetActive( true );

        if( Manager.I.GameType == EGameType.NAVIGATION )
            HideVegetation = ForceHideVegetation = false;

        if( Input.GetKeyDown( KeyCode.Tab ) )                                                   // Hide Vegetation forced
            ForceHideVegetation = !ForceHideVegetation;
        
        bool AltarHideVegetation = false;
        //if( G.HS && G.HS.Cleared )            // enable this to optimize after testing altars ***************                                       
        {
            for( int i = 0; i < 8; i++ )
            {
                Vector2 tgg = G.Hero.Pos + Manager.I.U.DirCord[ i ];                            // Hero around random altar hide vegetation
                Unit un = Map.I.GetUnit( ETileType.ALTAR, tgg );
                if( un && un.Altar.RandomAltar )
                { 
                    AltarHideVegetation = true; 
                    Map.I.ZoomMode = 0; 
                }
            }
        }

        if( HideVegetation || ForceHideVegetation || AltarHideVegetation )                      // Hide vegetation layers
        {
            DecorLayer1.SetActive( false );
            DecorLayer2.SetActive( false );
        }

        if(!Input.GetKey( KeyCode.LeftControl ) )
        {            
            HideVegetationTimer = 0; 
        }

        if( HideVegetation == false )
        {
            UI.I.UIFolder.gameObject.SetActive( true );
            UI.I.BackgroundUI.gameObject.SetActive( true );
        }

        if( fz < 0 )
        if( Input.GetKeyDown( KeyCode.LeftControl ) )                                          // Show vegetation layer
        if( HideVegetation )
        {
            HideVegetation = false;
            HideVegetationTimer = 0;
            DisabledForce = true;
        }

        if( DisabledForce == false )
        if( Input.GetKeyUp( KeyCode.LeftControl ) )                               
        if( HideVegetationTimer < waittime )
        if( HideVegetation  == false )
        {
            UI.I.SetZoomLevel();
        }

        if( Input.GetKeyUp( KeyCode.LeftControl ) )
            DisabledForce = false;

        if( cm && md == -1 && ori == -1 )
        {
            HideVegetation = true;
            UI.I.UIFolder.gameObject.SetActive( false );
        }        

        if( Manager.I.GameType != EGameType.FARM )
        if( Manager.I.GameType != EGameType.NAVIGATION )
        {
            Rect ca = new Rect( -1, -1, -1, -1 );

            if( zm == 0 )                                                               // Camera area type of zoom
            if( CamArea.width != 0 ) ca = CamArea;

            if( CamArea.width == 1 )
            if( CamArea.height == 1 )
                Item.SetAmt( ItemType.Res_ForcedZoom, -1 );                             // Stepping into an alone pink tile invalidates forced zoom

            if( freeCamAreaZoom )                                                       // Free Cam area zoom
            {
                focusHero = true;
                if( CamArea.width != 0 )
                    ca = CamArea;  
            }

            if( ca.width != -1 )
            {
                //int clickArea = GetPosArea( new Vector2( Mtx, Mty ) );
                //if( clickArea == -1 && Input.GetMouseButtonDown( 0 ) && Mtx != -1 ) AreaZoomLevel = 1;              

                CamDataID = ( int ) ca.height;
                float div = ca.width / ca.height;

                bool ok = false;

                if( ( ca.width / 2 ) > ca.height )
                {
                    CamDataID = ( int ) ca.width;
                    ok = true;
                    if( AreaZoomLevel == 1 ) CamDataID += 2;
                    //if( CurrentArea == -1 ) bestId += 2;
                    //Debug.Log( "X:" + bestId );
                }

                if( AreaZoomLevel == 1 ) CamDataID += 2;
                //if( CurrentArea == -1 ) bestId += 2;

                float ax = CData.OptimalVerticalDifY[ CamDataID ];

                zoom = CData.OptimalZoomY[ CamDataID ];
                if( ok )
                {
                    zoom = CData.OptimalZoomX[ CamDataID ];
                    ax = CData.OptimalVerticalDifX[ CamDataID ];
                }

                if( freeCamAreaZoom ) FreeCamAreaZoom = zoom;                                                          // Area has chnaged so change cam area zoom
                //else
                //	Debug.Log( "Y:" + bestId );

                //if( !ar.ZoomInIfCleared && ar.Cleared ||
                //    ar.BypassZoomIfDirty && !ar.Cleared ) zoom = CData.RoamingCameraZoom;
                //else
                {
                    target.x = ca.x + ( ca.width / 2 ) - 0.5f;                                                         // Center target in the middle of the area
                    target.y = ca.y - ( ca.height / 2 ) - ax;
                }
            }
            else
            {
                zoom = CData.RoamingCameraZoom;

                if( Map.I.RM.HeroSector )
                if( Map.I.RM.HeroSector.Type == Sector.ESectorType.NORMAL )                                             // Study mode
                if( zm == 3 || HideVegetation )                                                                         // Whole Cube Locked Zoom Mode
                {
                    target = Map.I.RM.HeroSector.Area.center;
                    target += CData.WholeCubeCenterVectorDistance;
                    zoom = CData.WholeCubeZoom;
                  }
                else
                if( zm == 0 || zm == 1 || zm == 2 )                                                                     // Camera face hero front when too close
                    focusHero = true;

                if( FishingMode == EFishingPhase.FISHING ||                                                             // Fishing mode
                    FishingMode == EFishingPhase.INTRO )
                {
                    target = new Vector3( MinHook.x + ( ( MaxHook.x - MinHook.x ) / 2 ),
                                          MinHook.y + ( ( MaxHook.y - MinHook.y ) / 2 ), target.z );
                    focusHero = false;
                }
            }
        }

        if( FreeCamAreaZoom > 0 )
        if( freeCamAreaZoom ) zoom = FreeCamAreaZoom;                                                                   // Free cam area zoom continues even after leaving pink area

        if( focusHero )
        {
            float front = Map.I.RM.RMD.CameraFrontFacingDist + frontDist;
            if( freeCamAreaZoom )                                                    // Every 1 unit bigger than 10 increases frontal distance for hero
            {                                                                        //  10: center on hero,   11: focus frontal tile, etc
                front = fz - 10;                                                     // Edit ItemType.Res_ForcedZoom to alter zoom mode
            }
            Vector3 tgg = G.Hero.GetRelativePosition( EDirection.N, 1 );
            tgg.Normalize();

            if( zm == 0 ) tgg.y += -0.4f;                              // added to position hero in the center of screen
            if( zm == 1 ) tgg.y += -0.64f;
            if( zm == 2 ) tgg.y += -0.9f;

            tgg *= front;
            target = G.Hero.Graphic.transform.position + tgg;
        }

        Vector3 velocity = Vector3.zero;
        Vector3 delta = target -  Manager.I.Camera.ViewportToWorldPoint( new Vector3( 0.5f, 0.5f, -10 ) );
        Vector3 destination = Manager.I.Camera.transform.position + delta;

        if( Input.GetKeyDown( KeyCode.LeftShift ) )                                              // toggle free cam mode
        {
            UI.I.SetFreeCamera( !FreeCamMode );
        }

        if( RM.DungeonDialog.gameObject.activeSelf ) FreeCamMode = false;
        UI.I.FreeCamModeLabel.gameObject.SetActive( FreeCamMode );
        if( fz != -1 ) FreeCamMode = false;

        if( Manager.I.GameType == EGameType.NAVIGATION )                                          // navigation map zoom levels
        {
            if( Map.I.ZoomMode == 0 ) zoom = 1.6f;
            if( Map.I.ZoomMode == 1 ) zoom = 1.0f;
            if( Map.I.ZoomMode == 2 ) zoom = .7f;
        }

        if( FreeCamMode )
        {
            float vel = 20.3f;
            Vector3 tg = Manager.I.Camera.transform.position;
            //zoom = Manager.I.Cam.ZoomFactor;
            if( cInput.GetKey( "Move N" ) ) tg.y += vel * Time.deltaTime;                      // move free camera
            if( cInput.GetKey( "Move S" ) ) tg.y -= vel * Time.deltaTime;
            if( cInput.GetKey( "Move E" ) ) tg.x += vel * Time.deltaTime;
            if( cInput.GetKey( "Move W" ) ) tg.x -= vel * Time.deltaTime;

            if( cInput.GetKey( "Move NE" ) ) zoom += 2.0f * Time.deltaTime;
            if( cInput.GetKey( "Move NW" ) ) zoom -= 2.0f * Time.deltaTime;
            if( cInput.GetKey( "Move SW" ) ) zoom = 5f;
            if( cInput.GetKey( "Move SE" ) ) zoom = 0.45f;

            zoom = Mathf.Clamp( zoom, .6f, 5f );

            Manager.I.Cam.ZoomFactor = zoom;
            UpdateBounds( ref tg );
            Manager.I.Cam.transform.position = tg;
        }
        else
        {
            float vel = 0;
            float damp = SettingsWindow.I.DampTime / 10000;

            if( Application.platform == RuntimePlatform.WindowsPlayer )
                damp /= 1.6f;

            float smooth = 0.125f;
            if( SessionTime < ZoomFXTime )
            {
                if( Input.GetKey( KeyCode.LeftControl ) ) 
                    ZoomKeyPressed = true;
                if( ZoomKeyPressed == false )
                {
                    smooth *= 3.8f;
                    if( CamAreaStepped ) smooth = 0.125f;
                }
            }

            if( CamAreaStepped && fz < 10 )
            if( CamDataID >= 0 && CamDataID <= CData.OptimalZoomSpeed.Length - 1 )
            {
                smooth = CData.OptimalZoomSpeed[ CamDataID ];                                           // optimal in area cam zoom speed
            }

            if( CamArea.x != 0 ) smooth *= 3.8f;
            if( HideVegetation )
            if( Manager.I.GameType == EGameType.CUBES )
            { 
                smooth = 0.125f / 3;
                destination = new Vector3( Map.I.RM.HeroSector.Area.center.x, 
                Map.I.RM.HeroSector.Area.center.y - .5f, Manager.I.Camera.transform.position.z );
                zoom = 0.65f;
            }

            Vector3 tg = Vector3.SmoothDamp( Manager.I.Camera.transform.position,
                         destination, ref velocity, damp );

            UpdateBounds( ref tg );

            Unit sand = GetUnit( ETileType.SAND, G.Hero.Pos );
            Unit snow = GetUnit( ETileType.SNOW, G.Hero.Pos );

            float zmm = zoom;            
            if( SessionTime >= ZoomFXTime )
            if( FishingMode == EFishingPhase.NO_FISHING )
            if( sand == null && snow == null )
            if( CamAreaStepped == false )
            if( HideVegetation == false )
            if( ZoomMode < 3 )
            if( fz < 0 )
            {
                //if( Input.GetKeyDown( KeyCode.T ) ) 
                //    ZoomPercent = Random.Range( 80, 120 );
                //zmm = Util.Percent( ZoomPercent, zoom );
                //if( G.Hero.Control.TurnTime > 2 )
                //    ZoomPercent -= Time.deltaTime * 4;
                //else
                //    ZoomPercent = 100;
                //ZoomPercent = Mathf.Clamp( ZoomPercent, 70, 180 );
            }

            Manager.I.Cam.ZoomFactor = Mathf.SmoothDamp( Manager.I.Cam.ZoomFactor, zmm, ref vel, smooth );
            if( SessionTime < 0.5f ) Manager.I.Cam.ZoomFactor = 0.6f;

            Manager.I.Camera.transform.position = new Vector3( tg.x, tg.y, -10 );
        }

        if( Helper.I.ShowCameraDebugText )                                                        // Debug
        {
            Debug.Log( "CamDataID: " +  CamDataID );
        }

        if( CamAreaStepped == false )
            CamDataID = -1;

        Manager.I.AfterCamera.ZoomFactor = Manager.I.Cam.ZoomFactor;
    }


    public void UpdateCameraAreaStepping( Vector2 to )
    {
        ETileType tl = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile( ( int ) to.x, ( int ) to.y, ( int ) ELayerType.AREAS );
        if( tl == ETileType.CAM_AREA )
        {
            CamAreaStepped = true;
            if( CamArea.x == 0 )
            {
                Vector2 n = new Vector2( 0, 0 );
                int ns = GetTileLineSize( Quest.I.Dungeon.Tilemap, G.Hero.Pos, new Vector2( 0, 1 ), ETileType.CAM_AREA, ref n, ELayerType.AREAS );
                Vector2 s = new Vector2( 0, 0 );
                int ss = GetTileLineSize( Quest.I.Dungeon.Tilemap, G.Hero.Pos, new Vector2( 0, -1 ), ETileType.CAM_AREA, ref s, ELayerType.AREAS );
                Vector2 e = new Vector2( 0, 0 );
                int es = GetTileLineSize( Quest.I.Dungeon.Tilemap, G.Hero.Pos, new Vector2( 1, 0 ), ETileType.CAM_AREA, ref e, ELayerType.AREAS );
                Vector2 w = new Vector2( 0, 0 );
                int ws = GetTileLineSize( Quest.I.Dungeon.Tilemap, G.Hero.Pos, new Vector2( -1, 0 ), ETileType.CAM_AREA, ref w, ELayerType.AREAS );

                Vector2 p1 = new Vector2( w.x, n.y );
                Vector2 p2 = new Vector2( e.x, s.y );
                Vector2 size = ( p2 - p1 );
                if( size.x < 0 ) size.x *= -1;
                if( size.y < 0 ) size.y *= -1;
                size += new Vector2( 1, 1 );
                CamArea = new Rect( p1, size );
            }
        }
        else
        {
            CamArea = new Rect( 0, 0, 0, 0 );                                                    // no camera area detected
            CamAreaStepped = false;
        }
    }

    public void UpdateBounds( ref  Vector3 tg )
    {
        if( HideVegetation ) return;
        int zm = ZoomMode;
        int fz = ( int ) Item.GetNum( ItemType.Res_ForcedZoom );

        if( CubeData.I.FixedZoomMode != -1 )                                              // Cubedata Forced zoom
            fz = CubeData.I.FixedZoomMode;

        if( fz >= 0 ) zm = fz;
        if( zm == 0 )
        {
            tg.x = ( float ) Mathf.Clamp( tg.x, CData.LowerLeftCamLimit.x - 0.5f + Manager.I.Cam.ScreenExtents.width / 2,
                                                CData.UpperRightCamLimit.x + Tilemap.width - 0.5f - Manager.I.Cam.ScreenExtents.width / 2 );
            tg.y = ( float ) Mathf.Clamp( tg.y, CData.LowerLeftCamLimit.y - 0.5f + Manager.I.Cam.ScreenExtents.height / 2,
                                                CData.UpperRightCamLimit.y + Tilemap.height - 0.5f - Manager.I.Cam.ScreenExtents.height / 2 );
        }
        else
        if( zm == 1 )
        {
            tg.x = ( float ) Mathf.Clamp( tg.x, CData.LowerLeftCamLimit2.x - 0.5f + Manager.I.Cam.ScreenExtents.width / 2,
                                                CData.UpperRightCamLimit2.x + Tilemap.width - 0.5f - Manager.I.Cam.ScreenExtents.width / 2 );
            tg.y = ( float ) Mathf.Clamp( tg.y, CData.LowerLeftCamLimit2.y - 0.5f + Manager.I.Cam.ScreenExtents.height / 2,
                                                CData.UpperRightCamLimit2.y + Tilemap.height - 0.5f - Manager.I.Cam.ScreenExtents.height / 2 );
        }
        else
        if( zm == 2 )
        {
            tg.x = ( float ) Mathf.Clamp( tg.x, CData.LowerLeftCamLimit3.x - 0.5f + Manager.I.Cam.ScreenExtents.width / 2,
                                                CData.UpperRightCamLimit3.x + Tilemap.width - 0.5f - Manager.I.Cam.ScreenExtents.width / 2 );
            tg.y = ( float ) Mathf.Clamp( tg.y, CData.LowerLeftCamLimit3.y - 0.5f + Manager.I.Cam.ScreenExtents.height / 2,
                                                CData.UpperRightCamLimit3.y + Tilemap.height - 0.5f - Manager.I.Cam.ScreenExtents.height / 2 );
        }
    }

    public void AreaSeek()
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
            if( Map.I.RM.GameOver ) return;
        if( Map.I.Hero.Body.ToolBoxLevel < 4 )
        {
            Map.I.ShowMessage( Language.Get( "ERROR_TOOLBOX4B" ) );
            return;
        }

        if( LevelStats.NormalSectorsDiscovered <= 0 ) return;

        FreeCamMode = true;
        int count = 0;
        for( int i = CurrentAreaSeekID + 1; i < 999; i++ )
        {
            if( i >= Quest.I.CurLevel.AreaList.Count ) i = 0;
            Area ar = Quest.I.CurLevel.AreaList[ i ];

            if( ar.Discovered )
                if( ar.Cleared == false )
                {
                    CurrentAreaSeekID = i;
                    Vector3 tar = new Vector3( ar.AreaRect.center.x, ar.P2.y - 0.5f + ( ( ar.P1.y - ar.P2.y ) / 2 ), -10 );
                    Manager.I.Camera.transform.position = tar;
                    break;
                }
            if( ++count == 999 ) break;
        }
    }

    public void ArtifactSeek()
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
            if( Map.I.RM.GameOver ) return;
        if( Map.I.Hero.Body.ToolBoxLevel < 4 )
        {
            Map.I.ShowMessage( Language.Get( "ERROR_TOOLBOX4A" ) );
            return;
        }

        FreeCamMode = true;
        int count = 0;
        for( int i = CurrentArtifactSeekID + 1; i < 999; i++ )
        {
            if( i >= Quest.I.CurLevel.ArtifactList.Count ) i = 0;

            Artifact ar = Quest.I.CurLevel.ArtifactList[ i ];
            if( ar.Collected == Artifact.EStatus.NOT_COLLECTED )
                if( Revealed[ ( int ) ar.Pos.x, ( int ) ar.Pos.y ] )
                {
                    bool ok = true;
                    if( Manager.I.GameType == EGameType.CUBES )
                    {
                        //if( !HeroData.I.AddResource( false, ar.CostResource_1, -ar.CostValue_1 ) ||
                        //    !HeroData.I.AddResource( false, ar.CostResource_2, -ar.CostValue_2 ) ) ok = false;
                    }

                    if( ok )
                    {
                        CurrentArtifactSeekID = i;
                        Vector3 tar = new Vector3( ar.Pos.x, ar.Pos.y, -10 );
                        Manager.I.Camera.transform.position = tar;
                        SeekArtifact = ar;
                        Quest.I.UpdateArtifactMouseOverInfo( ar.Pos );
                        break;
                    }
                }
            if( ++count == 999 ) break;
        }
    }


    public void UpdateSeekFunctions()
    {
        return;
        if( Input.GetKeyDown( KeyCode.F11 ) )                                                        // Artifact seek function F11
        {
            ArtifactSeek();
        }

        if( Input.GetKeyDown( KeyCode.Insert ) )                                                     // Artifact buy insert key
        {
            Artifact.QuickBuy();
        }

        if( Input.GetKeyDown( KeyCode.F12 ) )                                                        // Area seek function F12
        {
            AreaSeek();
        }
    }

    public void ClearTransTilemap( Vector2 pt )
    {
        if( pt.x == -1 )
        for( int l = 0; l < 4; l++ )
        for( int y = 0; y < TransTileMap.height; y++ )
        for( int x = 0; x < TransTileMap.width; x++ )
        {
            TransTileMap.SetTile( x, y, l, ( int ) ETileType.NONE );
        }
        if( pt.x != -1 )
        for( int l = 0; l < 4; l++ )
        {
            TransTileMap.SetTile( ( int ) pt.x, ( int ) pt.y, l, ( int ) ETileType.NONE );
        }
        TransTileMap.Build();
    }

    public void ClearTilemap()
    {
        for( int l = 0; l < 5; l++ )
            for( int y = 0; y < Tilemap.height; y++ )
                for( int x = 0; x < Tilemap.width; x++ )
                {
                    Tilemap.SetTile( x, y, l, ( int ) ETileType.NONE );
                }
    }

    public static bool CheckNarrowPassage( Vector2 pt1, Vector2 pt2 )
    {
        Vector2 dif = pt2 - pt1;
        bool ok = false;

        if( dif == new Vector2( 1, 1 ) )
        {
            if( Map.IsWall( pt1 + new Vector2( 0, 1 ) ) == true )
                if( Map.IsWall( pt1 + new Vector2( 1, 0 ) ) == true ) ok = true;
        }
        if( dif == new Vector2( 1, -1 ) )
        {
            if( Map.IsWall( pt1 + new Vector2( 1, 0 ) ) == true )
                if( Map.IsWall( pt1 + new Vector2( 0, -1 ) ) == true ) ok = true;
        }
        if( dif == new Vector2( -1, -1 ) )
        {
            if( Map.IsWall( pt1 + new Vector2( 0, -1 ) ) == true )
                if( Map.IsWall( pt1 + new Vector2( -1, 0 ) ) == true ) ok = true;
        }

        if( dif == new Vector2( -1, 1 ) )
        {
            if( Map.IsWall( pt1 + new Vector2( 0, 1 ) ) == true )
                if( Map.IsWall( pt1 + new Vector2( -1, 0 ) ) == true ) ok = true;
        }
        return ok;
    }

    public static bool IsDiagonal( EActionType ac )
    {
        if( ac == EActionType.MOVE_NW ||
            ac == EActionType.MOVE_NE ||
            ac == EActionType.MOVE_SW ||
            ac == EActionType.MOVE_SE ) return true;
        return false;
    }

    public string GetDirectionName( EActionType ac )
    {
        if( ac == EActionType.MOVE_N ) return "North";
        if( ac == EActionType.MOVE_S ) return "South";
        if( ac == EActionType.MOVE_E ) return "East";
        if( ac == EActionType.MOVE_W ) return "West";
        if( ac == EActionType.MOVE_NW ) return "NorthWest";
        if( ac == EActionType.MOVE_SW ) return "SouthWest";
        if( ac == EActionType.MOVE_NE ) return "NorthEast";
        if( ac == EActionType.MOVE_SE ) return "SouthEast";
        return "";
    }

    public void CalculateAgilityPoints()
    {
        //float points = 0;
        //for( int i = 0; i < MoveOrder.Count; i++ )
        //    if( MoveOrder[ i ].ValidMonster )
        //        if( MoveOrder[ i ].Body.IsDead )
        //        {
        //            points += MoveOrder[ i ].Body.EnemyAttackBonus;
        //        }

        //HeroData.I.AgilityPoints += points;
    }

    public bool IsTileOnlyGrass( Vector2 tg, bool mud = false )
    {
        if( PtOnMap( Tilemap, tg ) == false ) return false;
        Unit ga = Gaia[ ( int ) tg.x, ( int ) tg.y ];
        if( ga ) 
        {
            if( mud == false ) return false;
            if( ga.TileID != ETileType.MUD ) return false;
        }
        if( Gaia2[ ( int ) tg.x, ( int ) tg.y ] != null ) return false;
        if( Unit[ ( int ) tg.x, ( int ) tg.y ] != null ) return false;
        return true;
    }

    public bool IsTileFree( Vector2 from, Vector2 tg )
    {
        if( PtOnMap( Tilemap, tg ) == false ) return false;
        if( Revealed[ ( int ) tg.x, ( int ) tg.y ] == false ) return false;
        if( G.Hero.CanMoveFromTo( false, from, tg, G.Hero ) == false ) return false;
        //if( Gaia[ ( int ) tg.x, ( int ) tg.y ] != null ) return false;
        //if( Gaia2[ ( int ) tg.x, ( int ) tg.y ] != null )
        //if( Gaia2[ ( int ) tg.x, ( int ) tg.y ].Activated )
        //{
        //    if( Gaia2[ ( int ) tg.x, ( int ) tg.y ].TileID == ETileType.SAVEGAME ) gaia2 = true;

        //    return false;
        //}
        //if( Unit[ ( int ) tg.x, ( int ) tg.y ] != null )
        //if( Unit[ ( int ) tg.x, ( int ) tg.y ].Activated )
        //    return false;
        return true;
    }

    public float GetRuneValue( Vector2 tg )
    {
        if( G.Hero.Body.CollectorLevel < 1 ) return 1;

        int water = 0; int fire = 0; int free = 0;

        for( int i = 0; i < 8; i++ )
        {
            Vector2 aux = tg + Manager.I.U.DirCord[ i ];
            if( PtOnMap( Tilemap, aux ) )
                if( Revealed[ ( int ) aux.x, ( int ) aux.y ] == true )
                {
                    if( Map.I.IsTileFree( tg, aux ) ) free++;
                    Unit wa = GetUnit( ETileType.WATER, aux );
                    Unit bar = GetUnit( ETileType.BARRICADE, aux );
                    if( wa != null && bar == null ) water++;
                    Unit Fire = GetUnit( ETileType.FIRE, aux );
                    if( Fire != null && bar == null && Fire.Body.FireIsOn )
                        fire++;
                }
        }

        Area ar = Area.Get( tg );

        int max = 90;
        int central = 0;
        int totTiles = 0;
        float bonus = 0;

        if( G.Hero.Body.CollectorLevel < 2 )                                               // 6 free tiles
        {
            totTiles = 6;
            max = 2;
        }
        else
            if( G.Hero.Body.CollectorLevel < 3 )                                               // 5 free tiles
            {
                totTiles = 5;
                max = 2;
            }
            else
                if( G.Hero.Body.CollectorLevel < 4 )                                               // 4 free tiles
                {
                    totTiles = 4;
                    max = 2;
                }
                else
                    if( G.Hero.Body.CollectorLevel < 5 )                                               // every 3 free tiles
                    {
                        totTiles = 3;
                        max = 3;
                    }
                    else
                        if( G.Hero.Body.CollectorLevel < 6 )                                               // every 3 tiles central worth 1
                        {
                            central = 1;
                            totTiles = 3;
                        }
                        else
                            if( G.Hero.Body.CollectorLevel < 7 )                                               // every 2 free tiles
                            {
                                central = 1;
                                totTiles = 2;
                            }
                            else
                                if( G.Hero.Body.CollectorLevel < 8 )                                               // every 2 free tiles central worth 2
                                {
                                    central = 2;
                                    totTiles = 2;
                                }
                                else
                                    if( G.Hero.Body.CollectorLevel < 9 )                                               // inarea touched bonus 20%
                                    {
                                        central = 2;
                                        totTiles = 2;
                                        if( ar )
                                            bonus += 15 * ( ar.TouchedBarricadeCount -
                                                            ar.TouchedFromOutareaCount );
                                    }

        float num = 1 + ( ( int ) ( free + central ) / totTiles );

        if( num > max ) num = max;

        num += Util.Percent( bonus, num );

        //int num = water + fire;
        //int res = 1 + ( water ) + ( fire );
        //num *= res;

        if( num < 1 ) num = 1;
        return num;
    }

    public int GetNeighborBarricadesTouchedCount( Vector2 barpos )
    {
        if( PtOnMap( Tilemap, barpos ) == false ) return 0;
        if( RM.DungeonDialog.gameObject.activeSelf ) return 0;
        if( Manager.I.GameType == EGameType.FARM ) return 0;

        List<Vector2> poslist = new List<Vector2>();
        List<Vector2> adjin = new List<Vector2>();
        List<Vector2> adjout = new List<Vector2>();
        List<Vector2> naTouch = new List<Vector2>();
        List<Vector2> adder = new List<Vector2>();
        List<Vector2> areatouch = new List<Vector2>();
        List<Vector2> tot = new List<Vector2>();

        CountNeighborAreasTouched( barpos, ref naTouch );
        CountAdjacentTouched( barpos, ref adjin, ref adjout );
        CountAdderTouched( barpos, ref adder );

        int barArea = GetPosArea( barpos );
        Unit br = GetUnit( ETileType.BARRICADE, barpos );

        if( br == null )                                                         // If theres no barricade in the target, return area touch sum
        {
            if( barArea != -1 && Quest.I.CurLevel.AreaList.Count > 0 )
            {
                Area arr = Quest.I.CurLevel.AreaList[ barArea ];
                CountAreaTouched( arr, ref areatouch );
                Util.AddNew( ref tot, areatouch );
                return tot.Count;
            }
            else return 0;
        }

        if( CurrentArea == -1 )                                                  // Hero is OUT AREA
        {
            if( barArea == -1 )                                                  // And, Barricade is Outarea too
            {
                Util.AddNew( ref tot, adjin );
                Util.AddNew( ref tot, adjout );
                Util.AddNew( ref tot, naTouch );
                Util.AddNew( ref tot, adder );
                return tot.Count;
            }
            else                                                                  // Or Barricade is Inarea
            {
                Area arr = Quest.I.CurLevel.AreaList[ barArea ];
                CountAreaTouched( arr, ref areatouch );

                Util.AddNew( ref tot, adjin );
                Util.AddNew( ref tot, adjout );
                Util.AddNew( ref tot, areatouch );
                Util.AddNew( ref tot, adder );
                return tot.Count;
            }
        }
        else                                                                       // Hero is IN AREA
        {
            if( barArea != -1 )                                                    // And, Barricade is Inarea too
            {
                CountAreaTouched( CurArea, ref areatouch );

                Util.AddNew( ref tot, adjin );
                Util.AddNew( ref tot, adjout );
                Util.AddNew( ref tot, areatouch );
                Util.AddNew( ref tot, adder );
                return tot.Count;
            }
            else                                                                  // Or Barricade is Outarea
            {
                Util.AddNew( ref tot, adjin );
                Util.AddNew( ref tot, adjout );
                Util.AddNew( ref tot, naTouch );
                Util.AddNew( ref tot, adder );
                return tot.Count;
            }
        }
    }
    public void CountAdjacentTouched( Vector2 tg, ref List<Vector2> inlist, ref List<Vector2> outlist )
    {
        for( int i = 0; i < 8; i++ )
        {
            Vector2 aux = tg + Manager.I.U.DirCord[ i ];
            if( PtOnMap( Tilemap, aux ) )
            {
                if( Unit[ ( int ) aux.x, ( int ) aux.y ] )
                    if( Unit[ ( int ) aux.x, ( int ) aux.y ].TileID == ETileType.BARRICADE )
                        if( Unit[ ( int ) aux.x, ( int ) aux.y ].Activated )
                            if( Map.I.CheckArrowBlockFromTo( aux, tg, null ) == false )
                                if( Unit[ ( int ) aux.x, ( int ) aux.y ].Body.TouchCount > 0 )
                                {
                                    int area = GetPosArea( aux );

                                    if( area == -1 ) outlist.Add( aux );
                                    else inlist.Add( aux );
                                }
            }
        }
    }

    public void CountNeighborAreasTouched( Vector2 tg, ref List<Vector2> poslist )
    {
        Unit br = GetUnit( ETileType.BARRICADE, tg );
        if( br == null ) return;

        List<int> arlist = new List<int>();
        for( int i = 0; i < 8; i++ )
        {
            Vector2 aux = tg + Manager.I.U.DirCord[ i ];
            if( PtOnMap( Tilemap, aux ) )
            {
                int area = GetPosArea( aux );
                if( area != -1 && arlist.Contains( area ) == false )
                {
                    Area arr = Quest.I.CurLevel.AreaList[ area ];
                    if( arr != null )
                        if( arr.AreAllBonfiresLit() )
                        {
                            CountAreaTouched( arr, ref poslist );
                            arlist.Add( area );
                        }
                }
            }
        }
    }

    public void CountAreaTouched( Area ar, ref List<Vector2> poslist )
    {
        for( int y = ( int ) ar.P2.y; y <= ar.P1.y; y++ )
            for( int x = ( int ) ar.P1.x; x <= ar.P2.x; x++ )
                if( Map.I.AreaID[ x, y ] == ar.GlobalID )
                    if( Unit[ x, y ] )
                        if( Unit[ x, y ].TileID == ETileType.BARRICADE )
                            if( Unit[ x, y ].Activated )
                                if( Unit[ x, y ].Body.TouchCount > 0 )
                                {
                                    poslist.Add( new Vector2( x, y ) );
                                }
    }

    public void CountAdderTouched( Vector2 barpos, ref List<Vector2> poslist )
    {
        return;  // Adder has been removed. Add via perk idea
        for( int h = 0; h < 8; h++ )
        {
            Vector2 heron = G.Hero.Pos + Manager.I.U.DirCord[ h ];
            if( PtOnMap( Tilemap, heron ) )
            {
                if( Unit[ ( int ) heron.x, ( int ) heron.y ] )
                if( Unit[ ( int ) heron.x, ( int ) heron.y ].TileID == ETileType.BARRICADE )
                if( Unit[ ( int ) heron.x, ( int ) heron.y ].Body.TouchCount > 0 )
                if( Unit[ ( int ) heron.x, ( int ) heron.y ].Activated )
                if( Util.IsNeighbor( heron, barpos ) == false )
                    poslist.Add( heron );                             
            }
        }
    }


    public void RestoreOriginalOutAreaBarricades( bool exit )
    {
        for( int y = 0; y < Map.I.Tilemap.height; y++ )
            for( int x = 0; x < Map.I.Tilemap.width; x++ )
                if( AreaID[ x, y ] == -1 )
                    if( Unit[ x, y ] )
                        if( Unit[ x, y ].TileID == ETileType.BARRICADE )
                        {
                            if( exit == false )
                            {
                                Unit[ x, y ].SetVariation( Unit[ x, y ].Body.OriginalVariation );
                                Unit[ x, y ].Body.TouchCount = Unit[ x, y ].Body.OriginalTouchCount;
                            }
                            else
                            {
                                Unit[ x, y ].Body.OriginalVariation = Unit[ x, y ].Variation;
                                Unit[ x, y ].Body.OriginalTouchCount = Unit[ x, y ].Body.TouchCount;
                            }
                        }
    }

    public int GetFUnit( int x, int y )
    {
        if( FUnit[ x, y ] == null ) return 0;
        return FUnit[ x, y ].Count;
    }
    public List<Unit> GetFUnit( Vector2 tg, ETileType tl = ETileType.NONE )
    {
        if( FUnit[ ( int ) tg.x, ( int ) tg.y ] == null ) return null;
        if( tl != null )
        {
            List<Unit> ul = new List<Unit>();
            for( int i = 0; i < FUnit[ ( int ) tg.x, ( int ) tg.y ].Count; i++ )
            if( tl == ETileType.NONE || 
                tl == FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ].TileID )
                 ul.Add( FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ] );
            if( ul != null  && ul.Count < 1 ) ul = null;
            return ul;
        }
        if( FUnit[ ( int ) tg.x, ( int ) tg.y ] != null && 
            FUnit[ ( int ) tg.x, ( int ) tg.y ].Count < 1 ) return null;
        return FUnit[ ( int ) tg.x, ( int ) tg.y ];
    }
    
    public List<Unit> GetFlyingInArea( Vector2 pt, Vector2 rad )
    {
        List<Unit> l = new List<Unit>();
        for( int y = ( int ) ( pt.y - rad.y ); y < pt.y + rad.y; y++ )
        for( int x = ( int ) ( pt.x - rad.x ); x < pt.x + rad.x; x++ )
        if ( PtOnMap( Tilemap, new Vector2( x, y ) ) )
            {
                if( FUnit[ x, y ] != null && FUnit[ x, y ].Count > 0 ) l.AddRange( FUnit[ x, y ] );
            }
        return l;
    }

    public List<Unit> GF( Vector2 tg, ETileType type )
    {
        if ( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ] == null ) return null;
        List<Unit> l = new List<Unit>();
        for( int i = 0; i < Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ].Count; i++ )
        if ( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ].TileID == type )
             l.Add( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ] );
        if( l == null || l.Count < 1 ) return null;
        return l;
    }

    public List<Unit> GProj( Vector2 tg, EProjectileType type )            // Returns a flying projectile list of type
    {
        if( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ] == null ) return null;
        List<Unit> l = new List<Unit>();
        for( int i = 0; i < Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ].Count; i++ )
        if ( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ].TileID ==  ETileType.PROJECTILE )
        if ( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ].Control.ProjectileType == type )
             l.Add( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ] );
        if( l == null || l.Count < 1 ) return null;
        return l;
    }

    public static Unit GFU( ETileType type, Vector2 tg )
    {
        if ( Map.PtOnMap( Map.I.Tilemap, tg ) == false ) return null;
        if ( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ] == null ) return null;
        for( int i = 0; i < Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ].Count; i++ )
        if ( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ].TileID == type )
             return Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ];
        return null;
    }
    public static Unit GTM( Vector2 mtg )                // This returns only mines that can be used with tools:  Get Tool mine
    {
        Unit m = Map.GFU( ETileType.MINE, mtg );
        if( m == null ) return null;
        if( m.Body.MineType == EMineType.VAULT ) return null;
        if( Controller.TypeCanBeMined( m ) == false ) return null;
        return m;
    }
    public static Unit GMine( EMineType type, Vector2 tg )
    {
        if ( Map.PtOnMap( Map.I.Tilemap, tg ) == false ) return null;
        if ( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ] == null ) return null;
        for( int i = 0; i < Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ].Count; i++ )
        if ( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ].TileID == ETileType.MINE )
        if ( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ].Body.MineType == type )
             return Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ];
        return null;
    }
    public static bool CheckLeverCrossingBlock( Vector2 from, Vector2 to )
    {
        Vector2 dif = to - from;
        EDirection dr = Util.GetTargetUnitDir( from, to );
        if( dr == EDirection.NW && CheckLevelMove( from, EDirection.NW, EDirection.W, EDirection.NE ) ) return true;
        if( dr == EDirection.NW && CheckLevelMove( from, EDirection.NW, EDirection.N, EDirection.SW ) ) return true;
        if( dr == EDirection.NE && CheckLevelMove( from, EDirection.NE, EDirection.E, EDirection.NW ) ) return true;
        if( dr == EDirection.NE && CheckLevelMove( from, EDirection.NE, EDirection.N, EDirection.SE ) ) return true;
        if( dr == EDirection.SE && CheckLevelMove( from, EDirection.SE, EDirection.E, EDirection.SW ) ) return true;
        if( dr == EDirection.SE && CheckLevelMove( from, EDirection.SE, EDirection.S, EDirection.NE ) ) return true;
        if( dr == EDirection.SW && CheckLevelMove( from, EDirection.SW, EDirection.S, EDirection.NW ) ) return true;
        if( dr == EDirection.SW && CheckLevelMove( from, EDirection.SW, EDirection.W, EDirection.SE ) ) return true;
        return false;
    }

    public static bool CheckLevelMove( Vector2 from, EDirection dir, EDirection minedr, EDirection lev )
    {
        Vector2 pt = from + Manager.I.U.DirCord[ ( int ) minedr ];
        Unit mine = Map.GFU( ETileType.MINE, pt );
        if( mine )
        if( mine.Body.MineHasLever() )
        if( mine.Body.MineLeverDir == lev )
            return true;
        return false;
    }

    public static bool DoesLeverBlockMe( Vector2 to, Unit un )
    {
        for( int i = 0; i < 8; i++ )
        {
            Vector2 desttg = to + Manager.I.U.DirCord[ i ];
            Unit mine = Map.GFU( ETileType.MINE, desttg );
            if( mine != null )
            if( un == null || mine != un )
            if( mine.Body.MineHasLever() )
            {
                Vector2 levp = mine.Pos + Manager.I.U.DirCord[
                ( int ) mine.Body.MineLeverDir ];
                if( levp == to ) return true;                                                             // lever blocks tile
            }
        }
        return false;
    }
    public RandomMapData GetAdv()
    {
       return RM.RMList[ ( int ) RM.CurrentAdventure ];
    }
    public static bool IsClimbable( Vector2 pt, bool stair = true )
    {
        Unit ga = Map.I.GetUnit( pt, ELayerType.GAIA );
        if( ga && ga.TileID == ETileType.CLOSEDDOOR ) return true;
        if( ga && ga.TileID == ETileType.ROOMDOOR ) return true;
        if( ga && ga.TileID == ETileType.FOREST ) return true;
        Unit mine = Map.GFU( ETileType.MINE, pt );
        if( mine != null )
        {
            if( mine.Body.MineType != EMineType.SHACKLE )  
            if( mine.Body.MineType != EMineType.TUNNEL ) return true;
        }
        Unit ga2 = Map.I.GetUnit( pt, ELayerType.GAIA2 );
        if( stair )
        if( ga2 && ga2.TileID == ETileType.MINE && ga2.Body.MineType == EMineType.LADDER ) return true;
        return false;
    }   
}














