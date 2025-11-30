using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using PathologicalGames;
using System.Linq;
using DigitalRuby.LightningBolt;

public partial class Map : MonoBehaviour
{
    public void UpdateWorldMapUnitsData()
    {
        if( Unit == null ) return;
        if( CubeDeath == false )
            Hero.Control.UpdateSmoothMovementAnimation();                          // ***Optimize relocate to update numbers
        Vector2 _p1 = new Vector2( P1.x, P1.y );                                                                                          // Restrain to Area
        Vector2 _p2 = new Vector2( P2.x, P2.y );

        if( Manager.I.GameType == EGameType.CUBES )
        {
            if( RM.HeroSector == null ) return; 
            _p1 = new Vector2( ( int ) RM.HeroSector.Area.xMin, ( int ) RM.HeroSector.Area.yMin );                 // Restrain to Cube area
            _p2 = new Vector2( ( int ) RM.HeroSector.Area.xMax, ( int ) RM.HeroSector.Area.yMax );
        }
        else
        if( Manager.I.GameType == EGameType.FARM )
        {
            _p1 = new Vector2( Map.I.LevelEntrancePosition.x - G.Farm.FarmLimit.x,
                               Map.I.LevelEntrancePosition.y - G.Farm.FarmLimit.y );
            _p2 = new Vector2( Map.I.LevelEntrancePosition.x + G.Farm.FarmLimit.x,
                               Map.I.LevelEntrancePosition.y + G.Farm.FarmLimit.y );
        }

        for( int y = ( int ) _p1.y; y <= _p2.y; y++ )
        for( int x = ( int ) _p1.x; x <= _p2.x; x++ )   
        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( x, y ) ) )
            {
                if ( FUnit[ x, y ] != null )                                          // Flying units smooth animation update
                if ( FUnit[ x, y ].Count > 0 )
                for( int i = 0; i < Map.I.FUnit[ x, y ].Count; i++ )
                if ( Map.I.FUnit[ x, y ][ i ].TileID == ETileType.RAFT   ||
                     Map.I.FUnit[ x, y ][ i ].TileID == ETileType.FOG    ||
                     Map.I.FUnit[ x, y ][ i ].TileID == ETileType.MINE   ||
                     Map.I.FUnit[ x, y ][ i ].TileID == ETileType.HERB   ||
                     Map.I.FUnit[ x, y ][ i ].TileID == ETileType.ALGAE  ||
                     Map.I.FUnit[ x, y ][ i ].TileID == ETileType.VINES  ||
                     Map.I.FUnit[ x, y ][ i ].TileID == ETileType.SPIKES ||
                     Map.I.FUnit[ x, y ][ i ].TileID == ETileType.FISHING_POLE ||
                     Map.I.FUnit[ x, y ][ i ].TileID == ETileType.MUD_SPLASH   ||
                     Map.I.FUnit[ x, y ][ i ].TileID == ETileType.TRAIL        ||
                     Map.I.FUnit[ x, y ][ i ].ProjType( EProjectileType.BOOMERANG ) )
                     Map.I.FUnit[ x, y ][ i ].Control.UpdateSmoothMovementAnimation();

                if( Unit[ x, y ] != null )
                {
                    if( Unit[ x, y ].UnitType == EUnitType.MONSTER )
                        Unit[ x, y ].UpdateDirection();
                    Unit[ x, y ].Control.UpdateSmoothMovementAnimation();
                }

                if( Gaia2[ x, y ] != null )
                if( Gaia2[ x, y ].Control )
                    Gaia2[ x, y ].Control.UpdateSmoothMovementAnimation();
            }
    }

    public void ToggleGrid()
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
        if( Map.I.RM.GameOver ) return;
        UpdateGrid( true );
    }

    public int CountOutAreaPlacedFirewood( int area )
    {
        if( area == -1 ) return -1;
        Area ar = Quest.I.CurLevel.AreaList[ area ];
        int cont = 0;
        for( int y = ( int ) ar.P2.y; y <= ar.P1.y; y++ )
            for( int x = ( int ) ar.P1.x; x <= ar.P2.x; x++ )
                if( AreaID[ x, y ] == area )
                {
                    if( Gaia2[ x, y ] )
                        if( Gaia2[ x, y ].TileID == ETileType.FIRE )
                            if( Gaia2[ x, y ].Body.FireIsOn == false )
                                for( int dr = 0; dr < 8; dr++ )
                                    if( Gaia2[ x, y ].Body.WoodAdded[ dr ] )
                                    {
                                        Vector2 tg = new Vector2( x, y ) + Manager.I.U.DirCord[ dr ];
                                        if( PtOnMap( Tilemap, tg ) )
                                        {
                                            if( GetPosArea( tg ) == -1 ) cont++;
                                        }
                                    }
                }

        return cont;
    }

    public Vector2 GetRelativePosToMove( Vector2 pt, EDirection move, bool left )
    {
        if( move == EDirection.N )
            if( left ) return pt + new Vector2( -1, 0 );
            else return pt + new Vector2( +1, 0 );
        if( move == EDirection.S )
            if( left ) return pt + new Vector2( +1, 0 );
            else return pt + new Vector2( -1, 0 );
        if( move == EDirection.E )
            if( left ) return pt + new Vector2( 0, +1 );
            else return pt + new Vector2( 0, -1 );
        if( move == EDirection.W )
            if( left ) return pt + new Vector2( 0, -1 );
            else return pt + new Vector2( 0, +1 );
        if( move == EDirection.NE )
            if( left ) return pt + new Vector2( 0, +1 );
            else return pt + new Vector2( +1, 0 );
        if( move == EDirection.SW )
            if( left ) return pt + new Vector2( -1, 0 );
            else return pt + new Vector2( 0, -1 );
        if( move == EDirection.NW )
            if( left ) return pt + new Vector2( 0, +1 );
            else return pt + new Vector2( -1, 0 );
        if( move == EDirection.SE )
            if( left ) return pt + new Vector2( 0, -1 );
            else return pt + new Vector2( +1, 0 );
        return new Vector2( -1, -1 );
    }

    public bool SetRoomDoorId( Vector2 pos, int id )
    {
        if( PtOnMap( Tilemap, pos ) == false ) return false;
        if( Gaia[ ( int ) pos.x, ( int ) pos.y ] == null ) return false;
        if( Gaia[ ( int ) pos.x, ( int ) pos.y ].TileID != ETileType.ROOMDOOR ) return false;
        if( GateID[ ( int ) pos.x, ( int ) pos.y ] > 0 ) return false;

        GateID[ ( int ) pos.x, ( int ) pos.y ] = id;
        SetRoomDoorId( new Vector2( pos.x - 1, pos.y ), id );
        SetRoomDoorId( new Vector2( pos.x + 1, pos.y ), id );
        SetRoomDoorId( new Vector2( pos.x, pos.y - 1 ), id );
        SetRoomDoorId( new Vector2( pos.x, pos.y + 1 ), id );

        return true;
    }

    public bool SetDoorId( Vector2 pos, int id )
    {
        if( PtOnMap( Tilemap, pos ) == false ) return false;

        if( Gaia[ ( int ) pos.x, ( int ) pos.y ] == null ) return false;
        if( Gaia[ ( int ) pos.x, ( int ) pos.y ].TileID != ETileType.CLOSEDDOOR )
            if( Gaia[ ( int ) pos.x, ( int ) pos.y ].TileID != ETileType.OPENDOOR )
                return false;

        if( GateID[ ( int ) pos.x, ( int ) pos.y ] > 0 ) return false;

        GateID[ ( int ) pos.x, ( int ) pos.y ] = id;

        SetDoorId( new Vector2( pos.x - 1, pos.y ), id );
        SetDoorId( new Vector2( pos.x + 1, pos.y ), id );
        SetDoorId( new Vector2( pos.x, pos.y - 1 ), id );
        SetDoorId( new Vector2( pos.x, pos.y + 1 ), id );

        return true;
    }


    public bool SetWhiteDoorId( Vector2 pos, int id )
    {
        if( PtOnMap( Tilemap, pos ) == false ) return false;
        if( Gaia[ ( int ) pos.x, ( int ) pos.y ] == null ) return false;
        if( Gaia[ ( int ) pos.x, ( int ) pos.y ].TileID != ETileType.ROOMDOOR )
                return false;

        if( GateID[ ( int ) pos.x, ( int ) pos.y ] > 0 ) return false;

        GateID[ ( int ) pos.x, ( int ) pos.y ] = id;

        SetDoorId( new Vector2( pos.x - 1, pos.y ), id );
        SetDoorId( new Vector2( pos.x + 1, pos.y ), id );
        SetDoorId( new Vector2( pos.x, pos.y - 1 ), id );
        SetDoorId( new Vector2( pos.x, pos.y + 1 ), id );
        return true;
    }

    public bool SetRaftId( Vector2 pos, int id, ref Sector s )
    {
        if( !PtOnMap( Tilemap, pos ) ) return false;
        Unit raft = Controller.GetRaft( pos );
        if( raft == null ) return false;
        if( raft.Control.RaftGroupID > 0 ) return false;
        raft.Control.RaftGroupID = id;

        if( s != null && s.MaxRaftID < id )
            s.MaxRaftID = id;
        SetRaftId( pos + Manager.I.U.DirCord[ 0 ], id, ref s ); // N
        SetRaftId( pos + Manager.I.U.DirCord[ 1 ], id, ref s ); // NE
        SetRaftId( pos + Manager.I.U.DirCord[ 2 ], id, ref s ); // E
        SetRaftId( pos + Manager.I.U.DirCord[ 3 ], id, ref s ); // SE
        SetRaftId( pos + Manager.I.U.DirCord[ 4 ], id, ref s ); // S
        SetRaftId( pos + Manager.I.U.DirCord[ 5 ], id, ref s ); // SW
        SetRaftId( pos + Manager.I.U.DirCord[ 6 ], id, ref s ); // W
        SetRaftId( pos + Manager.I.U.DirCord[ 7 ], id, ref s ); // NW
        return true;
    }
    public bool SetFogId( Vector2 pos, int id )
    {
        if( PtOnMap( Tilemap, pos ) == false ) return false;
        if( Controller.GetFog( pos ) == false ) return false;
        Unit fog = Controller.GetFog( pos );
        if( fog == null ) return false;
        if( fog.Control.RaftGroupID > 0 ) return false;
        fog.Control.RaftGroupID = id;

        SetFogId( new Vector2( pos.x - 1, pos.y ), id );
        SetFogId( new Vector2( pos.x + 1, pos.y ), id );
        SetFogId( new Vector2( pos.x, pos.y - 1 ), id );
        SetFogId( new Vector2( pos.x, pos.y + 1 ), id );
        SetFogId( new Vector2( pos.x + 1, pos.y + 1 ), id );
        SetFogId( new Vector2( pos.x + 1, pos.y - 1 ), id );
        SetFogId( new Vector2( pos.x - 1, pos.y - 1 ), id );
        SetFogId( new Vector2( pos.x - 1, pos.y + 1 ), id );
        return true;
    }


    public bool SetTrapId( Vector2 pos, int id )
    {
        if( PtOnMap( Tilemap, pos ) == false ) return false;

        if( Gaia[ ( int ) pos.x, ( int ) pos.y ] == null ) return false;
        if( Gaia[ ( int ) pos.x, ( int ) pos.y ].TileID != ETileType.TRAP )
        if( Gaia[ ( int ) pos.x, ( int ) pos.y ].TileID != ETileType.FREETRAP )
        if( Gaia[ ( int ) pos.x, ( int ) pos.y ].TileID != ETileType.PIT )
            return false;

        if( GateID[ ( int ) pos.x, ( int ) pos.y ] > 0 ) return false;

        GateID[ ( int ) pos.x, ( int ) pos.y ] = id;

        SetTrapId( new Vector2( pos.x - 1, pos.y ), id );
        SetTrapId( new Vector2( pos.x + 1, pos.y ), id );
        SetTrapId( new Vector2( pos.x, pos.y - 1 ), id );
        SetTrapId( new Vector2( pos.x, pos.y + 1 ), id );

        SetTrapId( new Vector2( pos.x + 1, pos.y + 1 ), id );
        SetTrapId( new Vector2( pos.x + 1, pos.y - 1 ), id );
        SetTrapId( new Vector2( pos.x - 1, pos.y - 1 ), id );
        SetTrapId( new Vector2( pos.x - 1, pos.y + 1 ), id );

        return true;
    }

    public void ShowMessage( string msg, bool overlap = true )
    {
        if( overlap == false && ErrorMessageTimer != -1 ) return;
        ErrorMessageTimer = 4f;
        UI.I.ErrorMessageLabel.text = msg;
        UI.I.ErrorMessageLabel.text = UI.I.ErrorMessageLabel.text.Replace( "\\n", "\n" );
        UI.I.ErrorMessageLabel.color = new Color( 1, 0, 0, 1 );
        MasterAudio.PlaySound3DAtVector3( "Error", transform.position );
    }

    public void UpdateGateAndRaftID( Sector s )
    {
        int id = 0;
        int trapid = 0;
        Vector2 _p1 = new Vector2( 0, 0 );                                                           // Restrain to whole map Area
        Vector2 _p2 = new Vector2( Tilemap.width, Tilemap.height );

        if( s )
        {
            _p1 = new Vector2( ( int ) s.Area.xMin, ( int ) s.Area.yMin );                          // Restrain to Cube area
            _p2 = new Vector2( ( int ) s.Area.xMax, ( int ) s.Area.yMax );
        }

        for( int y = ( int ) _p1.y; y <= _p2.y; y++ )
        for( int x = ( int ) _p1.x; x <= _p2.x; x++ )    
        if ( Map.PtOnMap( Tilemap, new Vector2( x, y ) ) )
            {
                if( Gaia[ x, y ] != null )
                {
                    if( Gaia[ x, y ].TileID == ETileType.CLOSEDDOOR ||                              // Updates Doors ID
                        Gaia[ x, y ].TileID == ETileType.OPENDOOR )
                        if( GateID[ x, y ] <= 0 )
                            SetDoorId( new Vector2( x, y ), ++id );

                    if( Gaia[ x, y ].TileID == ETileType.ROOMDOOR )                                 // Updates Room Door ID
                        if( GateID[ x, y ] <= 0 )
                        SetRoomDoorId( new Vector2( x, y ), ++id );

                    if( Gaia[ x, y ].TileID == ETileType.TRAP ||                                    // Updates Traps ID
                        Gaia[ x, y ].TileID == ETileType.FREETRAP ||
                        Gaia[ x, y ].TileID == ETileType.PIT )
                        if( GateID[ x, y ] <= 0 )
                            SetTrapId( new Vector2( x, y ), ++trapid );
                }

            Unit raft = Controller.GetRaft( new Vector2( x, y ) );
            if( raft != null && raft.Control.RaftGroupID == -1 )                                    // Updates Raft ID
                SetRaftId( new Vector2( x, y ), 100 + ++id, ref s );

            Unit fog = Controller.GetFog( new Vector2( x, y ) );
            if( fog != null && fog.Control.RaftGroupID == -1 )                                      // Updates Fog ID
                SetFogId( new Vector2( x, y ), 100 + ++id );
            }
    }

    public void UpdateRoomDoorID()
    {
        for( int a = 0; a < Quest.I.CurLevel.AreaList.Count; a++ )
        {
            Rect r = Quest.I.CurLevel.AreaList[ a ].AreaRect;		// optimize
            AreaOffset = new Vector2( r.x, r.y - r.height + 1 );
            Vector2 p1 = new Vector2( ( int ) ( r.x ), ( int ) ( r.y ) );
            Vector2 p2 = new Vector2( ( int ) ( r.x + r.width - 1 ), ( int ) ( r.y - r.height + 1 ) );

            for( int y = ( int ) p2.y; y <= p1.y; y++ )
            for( int x = ( int ) p1.x; x <= p2.x; x++ )
                {
                    if( Gaia[ x, y ] )
                    if( Gaia[ x, y ].TileID == ETileType.ROOMDOOR )
                    if( GateID[ x, y ] <= 0 )
                        SetRoomDoorId( new Vector2( x, y ), 1000 + a );
                }
        }
    }

    public void UpdateMessage()
    {
        if( ErrorMessageTimer > 0 )
        {
            UI.I.ErrorMessageLabel.gameObject.SetActive( true );
            ErrorMessageTimer -= Time.deltaTime;

            if( ErrorMessageTimer < 2 )
                UI.I.ErrorMessageLabel.color = new Color( 1, 0, 0, ( ErrorMessageTimer * .5f ) );

            if( ErrorMessageTimer < 0 )
            {
                ErrorMessageTimer = -1;
                UI.I.ErrorMessageLabel.gameObject.SetActive( false );
            }
            return;
        }
    }

    public void UpdateRevealedPercent()
    {
        int revcount = 0;
        for( int y = 0; y < Tilemap.height; y++ )
            for( int x = 0; x < Tilemap.width; x++ )
            {
                if( Revealed[ x, y ] ) revcount++;
            }
        if( revcount == 0 ) revcount = 1;
        RevealedPercent = revcount * 100 / ( Tilemap.height * Tilemap.width );
    }

    public void UpdateGrid( bool force = false )
    {
        if( Manager.I.GameType == EGameType.NAVIGATION ) return;

        if( Manager.I.GameType == EGameType.FARM )
        {
            if( Map.I.Farm.CarryingAmount > 0 )
                ShowGrid = true;
            else
                ShowGrid = false;
            force = true;
        }
        if( RM.HeroSector == null ) return; 
        if( RM.HeroSector.CubeFrameCount < 2 ) return;

        if( Input.GetKeyDown( KeyCode.F9 ) )
        {
            RM.HeroSector.GridInitialized = false;
            Helper.I.ShowGaiaGrid = !Helper.I.ShowGaiaGrid;
        }

        if( Manager.I.GameType == EGameType.CUBES )
        if( RM.RMD.ForceGrid ) force = true;
        if( Input.GetKeyDown( KeyCode.F8 ) || force )
        {
            if( Manager.I.GameType != EGameType.FARM )
                ShowGrid = !ShowGrid;

            if( RM.RMD.ForceGrid )
                ShowGrid = true;
            bool show = ShowGrid;

            if( Manager.I.GameType == EGameType.CUBES )
            if( RM.HeroSector.GetRemainingMonsters() == 0 ) show = false;                                     // No grid for cleared cubes
            if( Manager.I.GameType == EGameType.CUBES )
            if( RM.HeroSector.Type == Sector.ESectorType.LAB ) show = false;
            if( show == false )
            if( FishingMode == EFishingPhase.FISHING ||
                FishingMode == EFishingPhase.INTRO ) show = true;

            if( Controller.HerbCount > 0 ) show = true;
            if( MineCount > 0 ) show = true;
            if( TransLayerInitialized )
            if( RM.HeroSector.GridInitialized == false )                                                                    // Initializes Grid Tilemap
            {
                int tile = ( int ) ETileType.NONE;
                Sector s = RM.HeroSector;
                if( s && s.Type == Sector.ESectorType.NORMAL )
                for( int y = ( int ) s.Area.yMin - 1; y < s.Area.yMax + 1; y++ )
                for( int x = ( int ) s.Area.xMin - 1; x < s.Area.xMax + 1; x++ )
                {
                    if( x == 0 )
                    if( y % 2 == 0 ) tile = ( int ) ETileType.NONE;
                    else tile = ( int ) ETileType.GRID;

                    Tilemap.SetTile( x, y, ( int ) ELayerType.GRID, tile );

                    if( Helper.I.ShowGaiaGrid == false )
                    {
                        ETileType tl = ( ETileType ) Quest.I.CurLevel.Tilemap.GetTile( x, y, ( int ) ELayerType.GAIA ); // old test it
                        if( tl == ETileType.FOREST ||
                            tl == ETileType.CLOSEDDOOR ||
                            tl == ETileType.ROOMDOOR ||
                            tl == ETileType.WATER )
                            Tilemap.SetTile( x, y, ( int ) ELayerType.GRID, -1 );
                    }

                    if( tile == ( int ) ETileType.NONE ) tile = ( int ) ETileType.GRID;
                    else tile = ( int ) ETileType.NONE;
                }
                UpdateTilemap = true;
                RM.HeroSector.GridInitialized = true;
            }

            if( show )
                Tilemap.Layers[ ( int ) ELayerType.GRID ].gameObject.SetActive( true );                       // Updates Grid Tilemap
            else
                Tilemap.Layers[ ( int ) ELayerType.GRID ].gameObject.SetActive( false );
        }
    }

    public GameObject UpdateTileGameObjectCreation( int x, int y, ELayerType layer, ETileType forceTile = ETileType.NONE, bool forceKill = false )
    {
        tk2dTileMap tm = Quest.I.CurLevel.Tilemap;
        ETileType tile = ( ETileType ) tm.GetTile( x, y, ( int ) layer );

        if( tile == ETileType.NONE ) return null;          // new: to optimize
        if( Manager.I.GameType == EGameType.FARM )                                                  // Building in the farm are only created after loading
        if( tile == ETileType.BUILDING )
        if( Farm.CreatingBuildings == false ) return null;

        if( forceTile != ETileType.NONE ) tile = forceTile;

        if( forceKill )
        {
            if( Gaia[ x, y ] && layer == ELayerType.GAIA ) Gaia[ x, y ].Kill();
            if( Gaia2[ x, y ] && layer == ELayerType.GAIA2 ) Gaia2[ x, y ].Kill();
            if( Unit[ x, y ] && layer == ELayerType.MONSTER ) Unit[ x, y ].Kill();
        }

        Unit prefabUnit = GetUnitPrefab( tile, layer );

        if( prefabUnit != null )
        {
            GameObject obj = CreateUnit( prefabUnit, new Vector2( x, y ), layer );
            return obj;
        }
        return null;
    }

    public Unit SpawnFlyingUnit( Vector2 tg, ELayerType layer, ETileType tile, Unit mother, bool init = false )
    {
        tk2dTileMap tm = Quest.I.CurLevel.Tilemap;
        Unit prefab = GetUnitPrefab( tile );
        if( prefab != null )
        {
            Transform tr = PoolManager.Pools[ "Pool" ].Spawn( prefab.PrefabName );
            Unit un = tr.gameObject.GetComponent<Unit>();
            un.Copy( prefab, true, true, true );

            int posx = 0, posy = 0;
            Map.I.Tilemap.GetTileAtPosition( un.transform.position, out posx, out posy );
            un.Pos = tg;
            un.Control.OldPos = tg;
            un.IniPos = tg;
            un.Control.AnimationOrigin = tg;
            un.Control.NotAnimatedPosition = tg;
            un.name = "" + un.PrefabName + " " + tg.x + " " + tg.y;
            un.TileID = GetTileID( tile );
            un.Graphic.transform.localPosition = Vector2.zero;                                    // new
           
            if( mother )
            {
                int babies = mother.Body.ChildList.Count; 
                un.Control.Mother = mother;
                un.Control.RealtimeSpeedFactor = mother.Control.RealtimeSpeedFactor;
                un.Control.FireMarkedWaspFactor = mother.Control.FireMarkedWaspFactor;
                un.Body.Sprite2.gameObject.SetActive( false );
                un.Body.Sprite3.gameObject.SetActive( false );
                un.Body.Sprite4.gameObject.SetActive( false );
                if( un.TileID == ETileType.WASP )
                    un.MeleeAttack.BaseDamage = mother.MeleeAttack.BaseDamage;

                if( init == false )
                {
                    if( mother.Body.MaxShieldedWasps > 0 )
                    if( mother.Control.ShieldedWaspCount < mother.Body.MaxShieldedWasps )
                    if( Util.Chance( mother.Body.ShieldedWaspChance ) || 
                        mother.Control.ShieldedWaspCount < mother.Md.MinShieldedWasps )
                        un.SetSpecialWasp( 1, mother );

                    if( mother.Md.MaxCocoonWasps < 1 ||
                        mother.Control.CocoonWaspCount < mother.Md.MaxCocoonWasps )
                    if( Util.Chance( mother.Md.CocoonWaspChance ) ||
                        mother.Control.CocoonWaspCount < mother.Md.MinCocoonWasps )
                        un.SetSpecialWasp( 2, mother );

                    if( mother.Md.MaxEnragedWasps < 1 ||
                        mother.Control.EnragedWaspCount < mother.Md.MaxEnragedWasps )
                    if( Util.Chance( mother.Md.EnragedWaspChance ) ||
                        mother.Control.EnragedWaspCount < mother.Md.MinEnragedWasps )
                        un.SetSpecialWasp( 3, mother );
                }
                mother.Body.ChildList.Add( un );
            }
            un.transform.position = new Vector3( tg.x, tg.y, -2 );
            un.UpdateLevelingData();
            un.Control.WaspClimbBarricadeNumber = 0;

            if( un.Body.ArrowList != null )
                for( int i = 0; i < un.Body.ArrowList.Length; i++ )
                    un.Body.ArrowList[ i ].gameObject.SetActive( false );
            G.HS.AddFlying( un );

            if( FUnit[ ( int ) tg.x, ( int ) tg.y ] == null )
                FUnit[ ( int ) tg.x, ( int ) tg.y ] = new List<Unit>();
            FUnit[ ( int ) tg.x, ( int ) tg.y ].Add( un );
            if( un.TileID == ETileType.BLOCKER )
                un.Spr.transform.Rotate( 0.0f, 0.0f, Random.Range( 0.0f, 360.0f ) );
            if( un.TileID == ETileType.ALGAE )
                BabyData.CreateAlgae( un );
            return un;
        }
        return null;
    }

    public void CreateMapGameObjects()
    {
        if( Manager.I.GameType == EGameType.CUBES )
        {
            for( int i = 0; i < RM.Fl.Count; i++ )
            if( RM.LabArea.Contains( RM.Fl[ i ] ) == false )
            {
                Quest.I.Dungeon.Tilemap.SetTile( RM.Fl[ i ].x,
                RM.Fl[ i ].y, ( int ) ELayerType.GAIA, ( int ) ETileType.FOREST );
                Map.I.UpdateTileGameObjectCreation( RM.Fl[ i ].x, RM.Fl[ i ].y, ELayerType.GAIA );                          // Creates Forest Objects from Gate Sector
                Map.I.Tilemap.SetTile( RM.Fl[ i ].x, RM.Fl[ i ].y, ( int ) ELayerType.GAIA, -1 ); 
            }
            for( int i = 0; i < RM.Dl.Count; i++ )
            if( RM.LabArea.Contains( RM.Dl[ i ] ) == false )
            {
                Quest.I.Dungeon.Tilemap.SetTile( ( int ) RM.Dl[ i ].x, ( int ) 
                RM.Dl[ i ].y, ( int ) ELayerType.GAIA, ( int ) ETileType.CLOSEDDOOR );
                Map.I.UpdateTileGameObjectCreation( RM.Dl[ i ].x, RM.Dl[ i ].y, ELayerType.GAIA );                          // Creates Door Objects from Gate Sector
                Map.I.Tilemap.SetTile( RM.Dl[ i ].x, RM.Dl[ i ].y, ( int ) ELayerType.GAIA, -1 );
            }
            
            for( int y = ( int ) RM.LabArea.yMin; y < RM.LabArea.yMax; y++ )                                                // creates inside lab objects
            for( int x = ( int ) RM.LabArea.xMin; x < RM.LabArea.xMax; x++ )
            if( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )
            if( Sector.GetPosSectorType( new VI( x, y ) ) != Sector.ESectorType.GATES )
            {
                Map.I.UpdateTileGameObjectCreation( x, y, ELayerType.GAIA );
                Map.I.Tilemap.SetTile( x, y, ( int ) ELayerType.GAIA, -1 );
            }                    
            return;
        }

        if( Manager.I.GameType == EGameType.FARM )
        {
            for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                                           // loop inside farm area         
            {
                int x = G.Farm.Tl[ tid ].x;
                int y = G.Farm.Tl[ tid ].y;
                UpdateTileGameObjectCreation( x, y, ELayerType.GAIA );
                UpdateTileGameObjectCreation( x, y, ELayerType.GAIA2 );
                UpdateTileGameObjectCreation( x, y, ELayerType.MONSTER );
            }
            return;
        }
        if( Manager.I.GameType != EGameType.NAVIGATION ) return;                                                        // Navigation objects

        Tilemap.Layers[ ( int ) ELayerType.MONSTER ].gameObject.SetActive( true );
        if( CurrentArea != -1 ) MonsterUnitsFolder.gameObject.SetActive( false );
        if( CurrentArea != -1 ) GaiaUnitsFolder.gameObject.SetActive( false );
        tk2dTileMap tm = Quest.I.CurLevel.Tilemap;

        for( int y = 0; y < Tilemap.height; y++ )
        for( int x = 0; x < Tilemap.width; x++ )
            {
                GameObject g = UpdateTileGameObjectCreation( x, y, ELayerType.GAIA );
                if( g == null )
                {
                    UpdateTileGameObjectCreation( x, y, ELayerType.GAIA2 );
                    UpdateTileGameObjectCreation( x, y, ELayerType.MONSTER );
                }

                ETileType tile = ( ETileType ) tm.GetTile( x, y, ( int ) ELayerType.RAFT );
                if( tile == ETileType.RAFT )
                    UpdateTileGameObjectCreation( x, y, ELayerType.RAFT );                                               // optimized
            }
    }
    
    public bool CanCreateUnitHere( EUnitType type, Vector2 tg )
    {
        if( type == EUnitType.MONSTER )
        {
            if( GS.IsLoading == false )                                            // This is to enable saving more than one monster in a single tile key: save2monsters
            if( Unit[ ( int ) tg.x, ( int ) tg.y ] != null ) return false;
        }
        else
        if( type == EUnitType.GAIA )
        {
            if( Gaia[ ( int ) tg.x, ( int ) tg.y ] != null ) return false;
        }
        else
        if( type == EUnitType.GAIA2 )
        {
            if( Gaia2[ ( int ) tg.x, ( int ) tg.y ] != null ) return false;
        }
        return true;
    }

    public bool CheckMouseRotation()
    {
        if( Application.platform == RuntimePlatform.Android )
            if( Input.touchCount == 1 )
            {
                if( Input.touches[ 0 ].phase == TouchPhase.Began )
                    if( new Vector2( Mtx, Mty ) == Hero.Pos )
                    {
                        DraggingFromHero = true;
                    }
                    else DraggingFromHero = false;

                if( DraggingFromHero == false ) return false;

                if( Input.touches[ 0 ].phase == TouchPhase.Ended )
                {
                    if( DraggingFromHero )
                        SwipeTileOrigin[ 0 ] = SwipeTileDest[ 0 ] = SwipeOrigin[ 0 ] = SwipeDest[ 0 ] = new Vector2( -1, -1 );
                    DraggingFromHero = false;
                    return true;
                }
            }
            else return false;

        if( new Vector2( Mtx, Mty ) == Hero.Pos )
        {
            if( Input.GetMouseButton( 1 ) )
            {
                Hero.Control.ForceMove = EActionType.WAIT; return true;
            }
            return false;
        }

        Vector3 mp = Camera.main.ScreenToWorldPoint( Input.mousePosition );
        mp += new Vector3( .5f, .5f, 0 );

        Vector3[] tg = new Vector3[ 8 ];

        float angle = 0;
        for( int i = 0; i < 8; i++ )
        {
            tg[ i ] = Hero.transform.position;
            tg[ i ].y += 200;
            tg[ i ] = Util.RotatePointAroundPivot( tg[ i ], Hero.transform.position, new Vector3( 0, 0, angle ) );
            angle -= 45;
        }

        float smalest = 999999; int smalestID = -1;
        for( int i = 0; i < 8; i++ )
        {
            float dist = Vector2.Distance( tg[ i ], mp );

            if( dist < smalest )
            {
                smalest = dist;
                smalestID = i;
            }
        }

        MouseRotationIndicator.transform.eulerAngles = Util.GetRotationAngleVector( ( EDirection ) smalestID );

        if( Hero.Control.PathFinding.Path.Count > 0 ) MouseRotationIndicator.gameObject.SetActive( false );
        else
        {
            MouseRotationIndicator.gameObject.SetActive( MouseRotationIndicatorState );
        }

        if( Input.GetMouseButtonDown( 2 ) ) MouseRotationIndicatorState = !MouseRotationIndicatorState;

        if( Application.platform == RuntimePlatform.Android )
        {
            MouseRotationIndicator.gameObject.SetActive( false );
            MouseRotationIndicatorState = false;
        }

        if( Input.GetMouseButton( 1 ) || ( DraggingFromHero && Input.GetMouseButton( 0 ) ) )
        {
            int steps1 = smalestID;
            int steps2 = smalestID;

            if( smalestID == ( int ) Hero.Dir )
            {
                if( DraggingFromHero || Input.GetMouseButton( 1 ) )
                {
                    Hero.Control.ForceMove = EActionType.WAIT;
                    return true;
                }
                return false;
            }

            for( int i = 0; i < 8; i++ )
            {
                if( steps1 == ( int ) Hero.Dir )
                {
                    Hero.Control.ForceMove = EActionType.ROTATE_CCW;
                    return true;
                }
                if( steps2 == ( int ) Hero.Dir )
                {
                    Hero.Control.ForceMove = EActionType.ROTATE_CW;
                    return true;
                }
                steps1 = ( int ) RotateDir( ( EDirection ) steps1, -1 );
                steps2 = ( int ) RotateDir( ( EDirection ) steps2, +1 );
            }
        }

        if( Input.GetMouseButton( 1 ) )
        {
            Hero.Control.ForceMove = EActionType.WAIT;
            return true;
        }

        if( DraggingFromHero ) return true;

        return false;
    }

    public bool CheckWait()
    {
        if( UI.I.KadeWaitButton.state == UIButtonColor.State.Pressed )
            if( ButtonPressingTimeCount > SettingsWindow.I.KeyHoldDelay )
            {
                return true;
            }

        if( Mtx != -1 )
            if( new Vector2( Mtx, Mty ) == Hero.Pos )
                if( Input.GetMouseButtonDown( 0 ) || ( ButtonPressingTimeCount > SettingsWindow.I.KeyHoldDelay && Input.GetMouseButton( 0 ) ) )
                {
                    WaitButton();
                    return true;
                }

        //if( Input.GetMouseButtonDown( 1 ) || ButtonPressingTimeCount > SettingsWindow.I.KeyHoldDelay && Input.GetMouseButton( 1 ) )
        //{
        //    WaitButton(); return true;
        //}

        return false;
    }

    public void UpdateTouchMovevent()
    {
        if( Helper.I.EnablePathFindingMovement == false ) return;
        if( UI.I.MouseOverUI ) return;
        if( Input.GetMouseButtonDown( 0 ) )
            ClickTileOrigin = new Vector2( Mtx, Mty );
        if( Input.GetMouseButtonUp( 0 ) )
            ClickTileOrigin = new Vector2( -1, -1 );

        if( Input.GetMouseButton( 0 ) || Input.GetMouseButton( 1 ) )
        {
            if( ClickTileOrigin == new Vector2( Mtx, Mty ) )
                SamePosTouchTimeCount += Time.deltaTime;
            else
                ClickTileOrigin = new Vector2( -1, -1 );
            ButtonPressingTimeCount += Time.deltaTime;
        }
        else
        {
            ButtonPressingTimeCount = 0; SamePosTouchTimeCount = 0;
        }

        if( FreeCamMode )
        {
            return;
        }

        if( InvalidateInputTimer > 0 ) return;

        if( UpdateNeighborTileMove() ) return;

        if( CheckMouseRotation() )
        {
            return;
        }

        if( CheckWait() )
        {
            WaitButton();
            return;
        }

        if( Mtx != -1 )
            if( Unit[ Mtx, Mty ] )
                return;

        if( SwipeTileDest[ 0 ] != SwipeTileOrigin[ 0 ] )
            if( SwipeTileOrigin[ 0 ] != new Vector2( -1, -1 ) )
                if( SwipeTileDest[ 0 ] != new Vector2( -1, -1 ) )
                {
                    Vector2 d = new Vector2( Mathf.Abs( SwipeTileOrigin[ 0 ].x - SwipeTileDest[ 0 ].x ), Mathf.Abs( SwipeTileOrigin[ 0 ].y - SwipeTileDest[ 0 ].y ) );

                    bool horiz = false;
                    if( d.x > d.y ) horiz = true;

                    //Debug.Log( "Origin: " + SwipeTileOrigin[ 0 ].x + " " + SwipeTileOrigin[ 0 ].y + "   Dest: " + SwipeTileDest[ 0 ].x + " " + SwipeTileDest[ 0 ].y + " horiz: " + horiz );

                    if( !horiz && new Vector2( Mtx, Mty ).x > Hero.Pos.x )
                    {
                        if( SwipeTileOrigin[ 0 ].y > SwipeTileDest[ 0 ].y )
                        {
                            Hero.Control.ForceMove = EActionType.ROTATE_CW;
                            //Debug.Log( "right of hero - or y > dest y - CW " );
                        }

                        if( SwipeTileOrigin[ 0 ].y < SwipeTileDest[ 0 ].y )
                        {
                            Hero.Control.ForceMove = EActionType.ROTATE_CCW;
                            //Debug.Log( "right of hero - or y < dest y - CCW " );
                        }
                    }
                    else
                        if( !horiz && new Vector2( Mtx, Mty ).x < Hero.Pos.x )
                        {
                            if( SwipeTileOrigin[ 0 ].y < SwipeTileDest[ 0 ].y )
                            {
                                Hero.Control.ForceMove = EActionType.ROTATE_CW;
                                //Debug.Log( "left of hero - or y < dest y - CW " );
                            }

                            if( SwipeTileOrigin[ 0 ].y > SwipeTileDest[ 0 ].y )
                            {
                                Hero.Control.ForceMove = EActionType.ROTATE_CCW;
                                //Debug.Log( "left of hero - or y > dest y - CCW " );
                            }
                        }
                        else
                            if( new Vector2( Mtx, Mty ).y > Hero.Pos.y )
                            {
                                if( SwipeTileOrigin[ 0 ].x > SwipeTileDest[ 0 ].x )
                                {
                                    Hero.Control.ForceMove = EActionType.ROTATE_CCW;
                                    //Debug.Log( "up of hero - or x > dest x - CCW " );
                                }

                                if( SwipeTileOrigin[ 0 ].x < SwipeTileDest[ 0 ].x )
                                {
                                    Hero.Control.ForceMove = EActionType.ROTATE_CW;
                                    //Debug.Log( "up of hero - or x < dest x - CW " );
                                }
                            }
                            else
                                if( new Vector2( Mtx, Mty ).y < Hero.Pos.y )
                                {
                                    if( SwipeTileOrigin[ 0 ].x < SwipeTileDest[ 0 ].x )
                                    {
                                        Hero.Control.ForceMove = EActionType.ROTATE_CCW;
                                        //Debug.Log( "down of hero - or x < dest x - CCW " );
                                    }

                                    if( SwipeTileOrigin[ 0 ].x > SwipeTileDest[ 0 ].x )
                                    {
                                        Hero.Control.ForceMove = EActionType.ROTATE_CW;
                                        //Debug.Log( "down of hero - or x > dest x - CW " );
                                    }
                                }
                                else
                                    if( SwipeTileOrigin[ 0 ] != SwipeTileDest[ 0 ] )
                                    {
                                        //Debug.Log("same line move");
                                        SwipeTileOrigin[ 0 ] = SwipeTileDest[ 0 ] = new Vector2( -1, -1 );
                                        return;
                                    }
                }

        if( Hero.Control.ForceMove != EActionType.NONE )
        {
            SwipeTileOrigin[ 0 ] = SwipeTileDest[ 0 ] = SwipeOrigin[ 0 ] = SwipeDest[ 0 ] = new Vector2( -1, -1 );
            return;
        }

        bool mb = false;
        Vector3 dest = new Vector3( -1, -1 );

        if( Input.GetMouseButton( 0 ) == false )
            SwipeTileOrigin[ 0 ] = SwipeTileDest[ 0 ] = new Vector2( -1, -1 );

        if( CurrentArea == -1 && Input.GetMouseButtonUp( 0 ) ) mb = true;
        if( CurrentArea != -1 && Input.GetMouseButtonUp( 0 ) ) mb = true;
        dest = new Vector3( Mtx, Mty, 0 );


        if( ButtonPressingTimeCount > SettingsWindow.I.KeyHoldDelay )
        {
            mb = true;
            SamePosTouchTimeCount = 0;
        }

        if( Input.GetMouseButton( 0 ) )
            if( Map.I.Mtx != -1 )
                if( Map.I.Gaia2[ Map.I.Mtx, Map.I.Mty ] != null )
                    if( Map.I.Gaia2[ Map.I.Mtx, Map.I.Mty ].TileID == ETileType.CHECKPOINT )
                        if( Map.I.ButtonPressingTimeCount < 10 )
                            mb = false;

        if( Application.platform == RuntimePlatform.Android && Input.touchCount > 1 ) return;

        if( dest.x != -1 )                                                                                                             // Pathfinding move           
            if( mb )
                if( RM.DungeonDialog.gameObject.activeSelf == false )
                {
                    Hero.Control.PathFinding.FindPath( new Vector3( Hero.Pos.x, Hero.Pos.y, 0 ), dest, Hero );
                    SwipeTileOrigin[ 0 ] = SwipeTileDest[ 0 ] = new Vector2( -1, -1 );
                    return;
                }
    }

    public void UpdateSwipe()
    {
        if( InvalidateInputTimer > 0 ) return;
        if( Input.touchCount > 1 ) return;
        // if( Application.platform == RuntimePlatform.WindowsPlayer ||
        //     Application.platform == RuntimePlatform.WindowsEditor )
        {
            UpdateMouseTile();
            Vector2 mp = new Vector2( Mtx, Mty );
            for( int touch = 0; touch < 2; touch++ )
                if( PtOnMap( Tilemap, mp ) )
                {
                    if( Input.GetMouseButtonDown( touch ) )
                    {
                        SwipeTileOrigin[ touch ] = mp;
                        SwipeOrigin[ touch ] = Input.mousePosition;
                    }

                    if( Input.GetMouseButtonUp( touch ) )
                    {
                        SwipeTileDest[ touch ] = mp;
                        SwipeDest[ touch ] = Input.mousePosition;
                    }
                }
        }
    }

    public void UpdateRepeat()
    {
        if( Map.I.CurrentArea != -1 )
            if( HeroIsDead && AdvanceTurn )
                Map.I.TDCol.TurnData[ CurArea.AreaTurnCount - 1 ].HeroDie = true;

        if( cInput.GetKey( "Advance VCR" ) )
        {
            RepeatButton();
            return;
        }

        if( UI.I.RepeatButton.state == UIButtonColor.State.Pressed )
            if( ButtonPressingTimeCount > 1 )
            {
                RepeatButton();
                return;
            }

        if( RepeatToLast )
        {
            RepeatButton();
            return;
        }

        if( HeroIsDead )
        {
            RepeatToLastButton();
            return;
        }

    }

    public void RepeatToLastButton()
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
        if( Map.I.RM.GameOver ) return;
        if( Map.I.PreferedAreaSave.x == -1 )
        {
            Map.I.EnterArea( Map.I.CurrentArea, true );
            return;
        }

        RepeatToLast = true;
        RestartOnRepeat = true;
    }

    public void RepeatButton()
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
        if( Map.I.RM.GameOver ) return;
        if( RestartOnRepeat )
        {
            Map.I.EnterArea( Map.I.CurrentArea, true );
        }

        if( PreferedAreaSave == Hero.Pos )
        {
            RepeatToLast = false;
            return;
        }

        Hero.Control.ForceMove = EActionType.ADVANCE_VCR;
        RestartOnRepeat = false;
    }

    public void WaitButton()
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
        if( Map.I.RM.GameOver ) return;
        Hero.Control.ForceMove = EActionType.WAIT;
        Hero.Control.PathFinding.Path.Clear();
        SwipeTileOrigin[ 0 ] = SwipeTileDest[ 0 ] = SwipeOrigin[ 0 ] = SwipeDest[ 0 ] = new Vector2( -1, -1 );
    }

    public void UpdateTouchCamera()
    {
        float dist = 0;
        if( Input.touchCount > 1 )
            dist = Vector3.Distance( Input.GetTouch( 0 ).position, Input.GetTouch( 1 ).position );

        if( Input.touchCount == 2 )
        {
            if( dist > 250 )
                if( Input.GetTouch( 1 ).phase == TouchPhase.Ended )
                    UI.I.ToggleMenu();
        }

        if( Input.touchCount == 3 )
        {
            if( dist > 250 )
                if( Input.GetTouch( 2 ).phase == TouchPhase.Ended )
                    UI.I.SetFreeCamera( !FreeCamMode );
        }
    }

    public void UpdatePathfinding()
    {
        if( Helper.I.EnablePathFindingMovement == false ) return;

        if( Create2DMap )
        {
            Pathfinder2D.Instance.Create2DMap( false, this, true );
            Create2DMap = false;
        }
    }

    public void ActivateAllDoorKnobs( ETileType tl, int key, Vector2 origin, Unit fromUnit )
    {
        if( tl != ETileType.DOOR_OPENER )
            if( tl != ETileType.DOOR_SWITCHER )
                if( tl != ETileType.DOOR_CLOSER )
                    return;
        
        Vector2 _p1 = P1;                                                                                                  // Restrain to Area
        Vector2 _p2 = P2;

        if( Manager.I.GameType == EGameType.CUBES )
        {
            _p1 = new Vector2( ( int ) RM.HeroSector.Area.xMin, ( int ) RM.HeroSector.Area.yMin );                         // Restrain to Cube area
            _p2 = new Vector2( ( int ) RM.HeroSector.Area.xMax, ( int ) RM.HeroSector.Area.yMax );
        }

        for( int y = ( int ) _p1.y; y <= _p2.y; y++ )
        for( int x = ( int ) _p1.x; x <= _p2.x; x++ )                                                                       // Sweeps the area looking for a door knob
        {
            if( Map.I.Gaia2[ x, y ] != null )
            if( Map.I.Gaia2[ x, y ].TileID == ETileType.DOOR_KNOB )
                {               
                    Unit orb = Map.I.GetUnit( ETileType.ORB, new Vector2( x, y ) );                                          // No over Orb Activation

                    if( orb == null )
                    {
                        bool res = Map.I.Gaia2[ x, y ].ActivateDoorKnob( tl, key );                                          // Activate it
                        
                        if( res )                                                                                            // Lighning FX
                        {
                            CreateLightiningEffect( fromUnit, Map.I.Gaia2[ x, y ], "keys" );
                            Map.I.CreateExplosionFX( new Vector2( x, y ) );                                                  // FX
                            OrbStruck = true;
                        }
                    }
                }
        }              
    }

    public LightningBoltScript CreateLightiningEffect( Unit from, Unit to, string type, float frx = -1, float fry = -1, float tox = -1, float toy = -1 )
    {
        Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Lightning Bolt" );
        LineRenderer lr = tr.GetComponent<LineRenderer>();
        LightningBoltScript lb = tr.GetComponent<LightningBoltScript>();
        LightningBoltScript old = lb;
        lb.Start();
        lb.type = type;
        lb.Lifetime = 2;
        lb.StartPosition = new Vector3( frx, fry, -5 );
        if( frx != -1 ) { from = null; lb.StartUnit = null; }
        if( tox != -1 ) { to = null; lb.EndUnit = null; }
        if( from )
        {
            lb.StartUnit = from;
            lb.StartPosition = new Vector3( from.Pos.x, from.Pos.y, -5 );
        }

        lb.EndPosition = lb.StartPosition;
        lb.Target = new Vector3( tox, toy, -5 );
        if( to )
        {
            lb.EndUnit = to;
            lb.Target = new Vector3( to.Pos.x, to.Pos.y, -5 );
            lb.RaftGroupID = to.Control.RaftGroupID;
        }
        if( type == "Electric Fog" ) lb.Lifetime = 99999;

        tr = PoolManager.Pools[ "Pool" ].Spawn( "Lightning Bolt 2" );
        lr = tr.GetComponent<LineRenderer>();
        lb = tr.GetComponent<LightningBoltScript>();
        lb.Start();
        lb.type = type;
        lb.Lifetime = 2;
        lb.StartPosition = new Vector3( frx, fry, -5 );
        if( from )
        {
            lb.StartUnit = from;
            lb.StartPosition = new Vector3( from.Pos.x, from.Pos.y, -5 );
        }

        lb.EndPosition = lb.StartPosition;
        lb.Target = new Vector3( tox, toy, -5 );
        if( to )
        {
            lb.EndUnit = to;
            lb.Target = new Vector3( to.Pos.x, to.Pos.y, -5 );
            lb.RaftGroupID = to.Control.RaftGroupID;
        }
        if( type == "Electric Fog" ) lb.Lifetime = 99999;
        if( type == "Plant" ) lb.Lifetime = 99999;
        if( type != "Cube Cleared" )
        if( type != "Shield" )
        if( type != "Plant" )
            MasterAudio.PlaySound3DAtVector3( "Activate Key", lb.StartPosition );
        lb.Lighting2 = old;
        old.Lighting2 = lb;
        return lb;
    }
    
    public void UpdatePressurePlates()
    {
        return;
        if( AdvanceTurn == false ) return;
        if( CurrentArea == -1 ) return;
        if( CurArea.AreaTurnCount <= 0 ) return;

        for( int i = 0; i < TotKeys; i++ )                                                       // Reset Variables
        {
            OldRedPPGateOpenerCount[ i ] = RedPPGateOpenerCount[ i ];
            OldRedPPGateSwitcherCount[ i ] = RedPPGateSwitcherCount[ i ];
            OldRedPPGateCloserCount[ i ] = RedPPGateCloserCount[ i ];
            OldGreenPPGateOpenerCount[ i ] = GreenPPGateOpenerCount[ i ];
            OldGreenPPGateSwitcherCount[ i ] = GreenPPGateSwitcherCount[ i ];
            OldGreenPPGateCloserCount[ i ] = GreenPPGateCloserCount[ i ];

            GreenPPGateOpenerCount[ i ] = GreenPPGateOpenerTotalCount[ i ] = 0;
            GreenPPGateSwitcherCount[ i ] = GreenPPGateSwitcherTotalCount[ i ] = 0;
            GreenPPGateCloserCount[ i ] = GreenPPGateCloserTotalCount[ i ] = 0;

            RedPPGateOpenerCount[ i ] = RedPPGateSwitcherCount[ i ] = RedPPGateCloserCount[ i ] = 0;
        }

        for( int y = ( int ) P2.y; y <= P1.y; y++ )
        for( int x = ( int ) P1.x; x <= P2.x; x++ )
            {
                int key = 0;

                if( Gaia[ x, y ] != null )
                    if( Gaia[ x, y ].TileID == ETileType.GREEN_PRESSUREPLATE )                            // Count Green PP total
                    {
                        if( Gaia2[ x, y ] )
                        {
                            if( Gaia2[ x, y ].TileID == ETileType.DOOR_OPENER ) GreenPPGateOpenerTotalCount[ key ] += 1;
                            if( Gaia2[ x, y ].TileID == ETileType.DOOR_SWITCHER ) GreenPPGateSwitcherTotalCount[ key ] += 1;
                            if( Gaia2[ x, y ].TileID == ETileType.DOOR_CLOSER ) GreenPPGateCloserTotalCount[ key ] += 1;
                        }
                    }

                if( Hero.Pos == new Vector2( x, y ) || Unit[ x, y ] != null )
                    if( Gaia[ x, y ] != null )
                    {
                        if( Gaia[ x, y ].TileID == ETileType.GREEN_PRESSUREPLATE )                        // Count Green PP Occupied
                        {
                            if( Gaia2[ x, y ] )
                            {
                                if( Gaia2[ x, y ].TileID == ETileType.DOOR_OPENER ) GreenPPGateOpenerCount[ key ] += 1;
                                if( Gaia2[ x, y ].TileID == ETileType.DOOR_SWITCHER ) GreenPPGateSwitcherCount[ key ] += 1;
                                if( Gaia2[ x, y ].TileID == ETileType.DOOR_CLOSER ) GreenPPGateCloserCount[ key ] += 1;
                            }
                        }
                        else
                            if( Gaia[ x, y ].TileID == ETileType.RED_PRESSUREPLATE )                        // Count Red PP Occupied
                            {
                                if( Gaia2[ x, y ] )
                                {
                                    if( Gaia2[ x, y ].TileID == ETileType.DOOR_OPENER ) RedPPGateOpenerCount[ key ] += 1;
                                    if( Gaia2[ x, y ].TileID == ETileType.DOOR_SWITCHER ) RedPPGateSwitcherCount[ key ] += 1;
                                    if( Gaia2[ x, y ].TileID == ETileType.DOOR_CLOSER ) RedPPGateCloserCount[ key ] += 1;
                                }
                            }
                    }
            }


        for( int i = 0; i < TotKeys; i++ )
        {
            //if( RedPPGateOpenerCount[ i ] != OldRedPPGateOpenerCount[ i ] )
            //    if( RedPPGateOpenerCount[ i ] >= 1 ) ActivateAllDoorKnobs( ETileType.DOOR_OPENER, i );
            //    else
            //        if( RedPPGateOpenerCount[ i ] <= 0 ) ActivateAllDoorKnobs( ETileType.DOOR_OPENER, i );

            //if( RedPPGateSwitcherCount[ i ] != OldRedPPGateSwitcherCount[ i ] )
            //    if( RedPPGateSwitcherCount[ i ] >= 1 ) ActivateAllDoorKnobs( ETileType.DOOR_SWITCHER, i );
            //    else
            //        if( RedPPGateSwitcherCount[ i ] <= 0 ) ActivateAllDoorKnobs( ETileType.DOOR_SWITCHER, i );

            //if( RedPPGateCloserCount[ i ] != OldRedPPGateCloserCount[ i ] )
            //    if( RedPPGateCloserCount[ i ] >= 1 ) ActivateAllDoorKnobs( ETileType.DOOR_CLOSER, i );
            //    else
            //        if( RedPPGateCloserCount[ i ] <= 0 ) ActivateAllDoorKnobs( ETileType.DOOR_CLOSER, i );

            //if( GreenPPGateOpenerCount[ i ] != OldGreenPPGateOpenerCount[ i ] )
            //{
            //    if( OldGreenPPGateOpenerCount[ i ] < GreenPPGateOpenerTotalCount[ i ] &&
            //        GreenPPGateOpenerCount[ i ] == GreenPPGateOpenerTotalCount[ i ] ) ActivateAllDoorKnobs( ETileType.DOOR_OPENER, i );
            //    else
            //        if( OldGreenPPGateOpenerCount[ i ] == GreenPPGateOpenerTotalCount[ i ] &&
            //            GreenPPGateOpenerCount[ i ] < GreenPPGateOpenerTotalCount[ i ] ) ActivateAllDoorKnobs( ETileType.DOOR_OPENER, i );
            //}

            //if( GreenPPGateSwitcherCount[ i ] != OldGreenPPGateSwitcherCount[ i ] )
            //{
            //    if( OldGreenPPGateSwitcherCount[ i ] < GreenPPGateSwitcherTotalCount[ i ] &&
            //        GreenPPGateSwitcherCount[ i ] == GreenPPGateSwitcherTotalCount[ i ] ) ActivateAllDoorKnobs( ETileType.DOOR_SWITCHER, i );
            //    else
            //        if( OldGreenPPGateSwitcherCount[ i ] == GreenPPGateSwitcherTotalCount[ i ] &&
            //            GreenPPGateSwitcherCount[ i ] < GreenPPGateSwitcherTotalCount[ i ] ) ActivateAllDoorKnobs( ETileType.DOOR_SWITCHER, i );
            //}

            //if( GreenPPGateCloserCount[ i ] != OldGreenPPGateCloserCount[ i ] )
            //{
            //    if( OldGreenPPGateCloserCount[ i ] < GreenPPGateCloserTotalCount[ i ] &&
            //        GreenPPGateCloserCount[ i ] == GreenPPGateCloserTotalCount[ i ] ) ActivateAllDoorKnobs( ETileType.DOOR_CLOSER, i );
            //    else
            //        if( OldGreenPPGateCloserCount[ i ] == GreenPPGateCloserTotalCount[ i ] &&
            //            GreenPPGateCloserCount[ i ] < GreenPPGateCloserTotalCount[ i ] ) ActivateAllDoorKnobs( ETileType.DOOR_CLOSER, i );
            //}
        }
    }

    public ETileType GetRotationID( ETileType tile, int variation )
    {
        int[] aux = new int[] { -1, 0, 1, 127, 129, 255, 256, 257 };
        return tile + aux[ variation ];
    }

    public ETileType RotateTile( int tile, int times )
    {
        for( int i = 0; i < Mathf.Abs( times ); i++ )
        {
            if( times > 0 )
                if( tile == 16 ) tile = 17;
                else if( tile == 17 ) tile = 18;
                else if( tile == 18 ) tile = 34;
                else
                    if( tile == 34 ) tile = 50;
                    else if( tile == 50 ) tile = 49;
                    else
                        if( tile == 49 ) tile = 48; else if( tile == 48 ) tile = 32; else if( tile == 32 ) tile = 16;

            if( times < 0 )
                if( tile == 16 ) tile = 32;
                else if( tile == 32 ) tile = 48;
                else if( tile == 48 ) tile = 49;
                else
                    if( tile == 49 ) tile = 50;
                    else if( tile == 50 ) tile = 34;
                    else if( tile == 34 ) tile = 18;
                    else
                        if( tile == 18 ) tile = 17; else if( tile == 17 ) tile = 16;
        }

        return ( ETileType ) tile;
    }
    public void UpdateRoachBackSideStep( Vector2 to )
    {
        List<Unit> rl = Util.GetNeighbors( to, ETileType.ROACH );
        if( rl.Count <= 0 ) return;

        for( int i = 0; i < rl.Count; i++ )
        {
            bool ok = false;
            if( rl[ i ].Pos + rl[ i ].GetRelativePosition( EDirection.S, 1 ) == G.Hero.Pos ) ok = true;
            if( rl[ i ].Pos + rl[ i ].GetRelativePosition( EDirection.SE, 1 ) == G.Hero.Pos ) ok = true;
            if( rl[ i ].Pos + rl[ i ].GetRelativePosition( EDirection.SW, 1 ) == G.Hero.Pos ) ok = true;
            if( ok )
            {
                rl[ i ].MeleeAttack.SpeedTimeCounter = 0;                                             // Resets Roach attack timer on bacside step               
            }
        }
    }
    public void LineEffect( Vector2 from, Vector2 to, float time, float fade, Color col1, Color col2, float width = .2f )
    {
        Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Resting Line" );
        LightningBoltScript lb = tr.GetComponent<LightningBoltScript>();
        Collider2D col = tr.GetComponent<Collider2D>();
        lb.type = "Rest Line";
        lb.StartPosition = new Vector3( from.x, from.y, -8 );
        lb.EndPosition = new Vector3( to.x, to.y, -8 );
        lb.Target = new Vector3( to.x, to.y, -8 );
        lb.gameObject.SetActive( true );
        lb.Lifetime = 99999;
        lb.lineRenderer.SetColors( col1, col2 );
        lb.lineRenderer.SetWidth( width, width );
        SelfDestroy[] joints = GetComponents<SelfDestroy>();
        foreach( SelfDestroy joint in joints )
            Destroy( joint );
        SelfDestroy sd = lb.gameObject.AddComponent<SelfDestroy>();
        sd.StartIt( time + .5f, fade, false );
        sd.Line = lb;
    }
    public static bool LowTerrain( Vector2 tg )
    {
        Unit ga = Map.I.GetUnit( tg, ELayerType.GAIA );
        if( ga )
        if( ga.TileID == ETileType.WATER ||
            ga.TileID == ETileType.PIT   ||
            ga.TileID == ETileType.LAVA ) return true;
        return false;
    }
    public static bool EmptyTile( Vector2 tg )
    {
        if( Map.I.GetUnit( tg, ELayerType.GAIA ) ) return false;
        if( Map.I.GetUnit( tg, ELayerType.GAIA2 ) ) return false;
        if( Map.I.GetUnit( tg, ELayerType.MONSTER ) ) return false;
        Unit fl = Map.GFU( ETileType.MINE, tg );
        if( fl && fl.TileID == ETileType.MINE ) return false;
        return true;
    }
}

