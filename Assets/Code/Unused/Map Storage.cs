using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using PathologicalGames;

public partial class Map : MonoBehaviour
{    
    //_____________________________________________________________________________________________________________________ Enter Area
    public void EnterArea( int area, bool restart = false )
    {
        //if( area == -1 ) return;
        //if( Quest.I.CurLevel.AreaList[ CurrentArea ].IsFake ) return;

        //CurrentArea = area;                                                                                    // Current Area and Cuar Area Initialization
        //CurArea = Quest.I.CurLevel.AreaList[ CurrentArea ];
        //Rect r = Quest.I.CurLevel.AreaList[ CurrentArea ].AreaRect;

        //string aname = "Level " + Quest.CurrentLevel + " " +                                                   // Area Cleared?
        //       Quest.I.CurLevel.AreaList[ area ].name;
        //AreaCleared = HeroData.I.IsAreaCleared( aname );

        //bool restarthp = true;
        //if( Manager.I.GameType == EGameType.CUBES )
        //if( restart )
        //{
        //    if( AreaCleared ) restarthp = false;
        //    if( HeroData.I.Lives <= 0 && AreaCleared == false )
        //        if( Map.I.RM.RMD.RestartPenalty > 0 )
        //        {
        //            restarthp = false;
        //            if( CurArea.AreaTurnCount > 0 )                                                            // Restart or Death HP Penalty
        //                HeroEnterAreaHP -= Util.Percent( RM.RMD.RestartPenalty, G.Hero.Body.TotHp );
        //        }
        //    if( Manager.I.GameType == EGameType.CUBES &&                                                       // Sudden Death
        //        RM.RMD.SuddenDeathMode )
        //    {
        //        restarthp = false;
        //    }
        //    else
        //    {
        //        Hero.Body.Hp = HeroEnterAreaHP;
        //        restarthp = false;
        //    }

        //    if( CurArea.AreaTurnCount > 0 && AreaCleared == false )                                            // Update Lives
        //    if( --HeroData.I.Lives < 0 ) HeroData.I.Lives = 0;
        //}
        //else restarthp = false;

        //AreaStats.Reset();
        //bool clear = CurArea.Cleared;
        //Area.FreeEntrance = false;
        //AreaExitTurnCount = 0;
        //CurArea.ResetLocalVariables( restart );

        //AreaOffset = new Vector2( r.x, r.y - r.height + 1 );

        //P1 = new Vector2( ( int ) ( r.x ), ( int ) ( r.y ) );                                                    // Calculate P1 and P2 for Area
        //P2 = new Vector2( ( int ) ( r.x + r.width - 1 ), ( int ) ( r.y - r.height + 1 ) );

        //CurArea.Discovered = true;
        //CurArea.NumTimesEntered++;

        //Area.LastAreaClearedWasPerfect = false;       

        //CurArea.AreaTurnCount = 0;
        //RM.HeroSector.FireIgnitionCount = 0;

        //MasterAudio.PlaySound3DAtVector3( "Area Enter", transform.position );                                   // Area Enter Sound FX

        //for( int y = ( int ) P2.y; y <= P1.y; y++ )
        //for( int x = ( int ) P1.x; x <= P2.x; x++ )
        //if( AreaID[ x, y ] == CurrentArea )
        //        {
        //            UI.I.UpdateSaveGameIconState( x, y );
        //        }

        //if( MoveOrder == null ) MoveOrder = new List<Unit>();

        //if( !restart )
        //    CreateMoveOrderList( CurArea );

        //if( !restart )
        //{
        //    AreaEnterFromTile = G.Hero.Control.OldPos;
        //    RestartOnRepeat = true;
        //    AreaSaveList = new List<Vector2>();
        //    MetaShootDeathList = new List<Unit>();
        //    AreaEnterPlatformMove = Hero.Control.LastMoveAction;
        //}
        //else
        //{
        //    RestoreOriginalOutAreaBarricades( false );

        //    for( int i = 0; i < MoveOrder.Count; i++ )
        //         Unit[ ( int ) MoveOrder[ i ].Pos.x, ( int ) MoveOrder[ i ].Pos.y ] = null;

        //    for( int i = 0; i < MoveOrder.Count; i++ )
        //    {
        //        MoveOrder[ i ].Revive( true );
        //    }
        //    for( int i = 0; i < MetaShootDeathList.Count; i++ )
        //    {
        //        MetaShootDeathList[ i ].Revive( true );
        //    }

        //    for( int y = ( int ) P2.y; y <= P1.y; y++ )
        //    for( int x = ( int ) P1.x; x <= P2.x; x++ )
        //    if ( AreaID[ x, y ] == CurrentArea )
        //    if ( Gaia2[ x, y ] )
        //    if ( Gaia2[ x, y ].TileID == ETileType.FIRE )
        //    {
        //        LightFire( x, y, Gaia2[ x, y ].Body.OriginalFireIsOn, false );
        //    }
        //}

        //if( restart )
        //{
        //    Manager.I.PackMule.RestartArea();
        //    CurArea.DirtyLifetimeSteps = CurArea.OriginalDirtyLifetimeSteps;
        //    CurArea.AreaClearedTurnCount = CurArea.OriginalAreaClearedTurnCount;

        //    Hero.Pos = HeroEnterPos;
        //    Hero.Dir = HeroEnterDir;
        //    PlatformEntranceMove = AreaEnterPlatformMove;
        //    ChangeHero( HeroEnterID, true );

        //    if( HeroData.I.IsAreaCleared( aname ) == false )
        //        RestoreAreaGates( CurrentArea );

        //    bool upd = false;
        //    for( int a = 0; a < Quest.I.CurLevel.ArtifactList.Count; a++ )
        //    if ( Quest.I.CurLevel.ArtifactList[ a ].Collected == Artifact.EStatus.TEMPORARY_COLLECTED )
        //        {
        //            Quest.I.CurLevel.ArtifactList[ a ].Collected = Artifact.EStatus.NOT_COLLECTED;
        //            Debug.Log( "1" );
        //            Quest.I.CurLevel.ArtifactList[ a ].gameObject.SetActive( true );
        //            HeroData.I.RemoveConqueredArtifact( Quest.I.CurLevel.ArtifactList[ a ].name );
        //            upd = true;
        //        }

        //    RestoreOriginalAreaObjects( CurrentArea, true );
        //    UI.I.CloseMessageBox();

        //    Map.I.RecordedMovementAvailable = true;

        //    if( upd ) Quest.I.UpdateArtifactData( ref Hero, true );

        //    UI.I.UpdateSavegameIcons();
        //}
        //else
        //{
        //    TDCol.TurnData = new List<TurnData>();
        //    SavedTurnData = new List<TurnDataCollection>();
        //    RepeatToLast = false;
        //    PreferedAreaSave = new Vector2( -1, -1 );
        //}
        
        //ClearAllRoomDoors();
        //UpdateNumbersData();
        //UI.I.MessageTurn = 0;
        //AreaZoomLevel = 0;
        //Map.I.LastFileSavedName = "";
        
        //bool surplus = false;
        //if( restart ) surplus = true;

        //InitHero( SelectedHero, Hero.Pos, restarthp, false, surplus, Hero.Dir );

        //if( !restart )
        //{
        //    HeroEnterAreaHP = Hero.Body.Hp;
        //    AreaEnterPlatformPoints = HeroData.I.PlatformPoints;
        //}
        //else
        //    HeroData.I.PlatformPoints = AreaEnterPlatformPoints;

        //HeroEnterPos = Hero.Pos;
        //HeroEnterDir = Hero.Dir;
        //HeroEnterID = ( EHeroID ) Hero.Variation;

        //if( restart ) Hero.Control.PlatformWalkingCounter += 1;

        //if( Quest.I.CurLevel.
        //     AreaList[ CurrentArea ].Cleared ) ClearRoomDoor( CurrentArea );

        //Quest.I.UpdateArtifactStepping( Hero.Pos );

        ////Hero.Control.UpdateTrapStepping();

        //UI.I.ShowAreaEnterMessage();
        //UpdateAreaColor( area );
        //UpdateFogOfWar();
        //Create2DMap = true;
        //if( AreaCleared == false )
        //    if( Hero.Control.PathFinding.Path.Count > 0 )
        //        Hero.Control.PathFinding.Path.Clear();

        //Area.UpdateAiTarget( HeroEnterPos, false );

        ////UI.I.TurnInfoLabel.text = "" + Hero.PrefabName + "  Entered " + CurArea.AreaName + ".";

        //if( Map.I.Hero.Control.MonsterPushLevel >= 3 )
        //    if( EntrancePushFrom.x != -1 )
        //    {
        //        Hero.Control.ApplyMove( Hero.Pos, EntrancePushFrom );
        //        Hero.Push( true, EntrancePushFrom, EntrancePushTo, Hero, Hero.Control.PushStrenght );                   // Obj push from outside the area    
        //        Hero.Control.ApplyMove( Hero.Pos, EntrancePushTo );
        //    }

        //if( Hero.Body.BerserkLevel >= 1 )                                                                                   // Berseker enter area attacks
        //{
        //    Attack.BerserkAttack = true;
        //    Hero.Body.NumFreeAttacks = HeroData.I.ExtraAttacksperBersekerLevel[ ( int ) Hero.Body.BerserkLevel ];
        //    int att = ( int ) Hero.Body.NumFreeAttacks;
        //    for( int i = 0; i < att; i++ )
        //    {
        //        Hero.UpdateAllAttacks( true );
        //    }
        //}

        //for( int i =0; i < MoveOrder.Count; i++ )
        //{
        //    MoveOrder[ i ].Control.UpdateSleep();
        //    MoveOrder[ i ].UpdateDirection();
        //}

        //Attack.BerserkAttack = false;

        //RM.EnterArea( CurArea );

        //InvalidateInputTimer = .1f;

        //UpdateBarricadeFighterAreaEnter();

        //UpdateHeroFireWoodPlacement();

        //RM.UpdateResourceClearing( CurArea, true );

        //Unit tk = GetUnit( ETileType.OUTAREATOKEN, AreaEnterFromTile );                                                   // Creates Out area Token
        //if( tk == null )
        //{
        //    UpdateTileGameObjectCreation( ( int ) AreaEnterFromTile.x, ( int ) AreaEnterFromTile.y,
        //                                        ELayerType.MONSTER, ETileType.OUTAREATOKEN, true );
        //    Unit[ ( int ) AreaEnterFromTile.x, ( int ) AreaEnterFromTile.y ].Body.IsEternalToken = false;
        //}
    }
    //_____________________________________________________________________________________________________________________ Check area Leave    
    public bool ExitArea( Vector2 from, Vector2 to )
    {
        //if( PtOnAreaMap( to ) == true ) return false;
        //if( CurrentArea == -1 ) return false;

        //if( RM.RMD.RestrictAreaExitToEntranceTile )
        ////if( AreaTurnCount <= 0 )
        //{
        //    if( to != AreaEnterFromTile )                                                                 // Outside token exit restriction
        //        if( Unit[ ( int ) to.x, ( int ) to.y ] == null ||
        //            Unit[ ( int ) to.x, ( int ) to.y ].TileID != ETileType.OUTAREATOKEN )
        //            {
        //                Message.CreateMessage( ETileType.NONE, "Exit\nHere!", AreaEnterFromTile,
        //                                         new Color( 1, 0, 0 ), false, false, 4, 0, -1 );
        //                return true;
        //            }
        //}

        //if(!Unit[ ( int ) AreaEnterFromTile.x, ( int ) AreaEnterFromTile.y ].Body.IsEternalToken )        // Destroy temp outarea token
        //    Unit[ ( int ) AreaEnterFromTile.x, ( int ) AreaEnterFromTile.y ].Kill( true );
            
        //if( Hero.CanMoveFromTo( false, from, to, Hero ) == false ) return true;

        //Hero.CanMoveFromTo( true, from, to, Hero );
        ////if( Hero.Control.UpdateTrapStepping() )
        ////{
        ////    UpdateTransLayerTilemap( true );
        ////    UpdateTileMap();
        ////}

        //AreaEnterFromTile = new Vector2( -1, -1 );
        ////UI.I.TurnInfoLabel.text = "" + Hero.PrefabName + " has Exited " + CurArea.AreaName + " Area.";
        //CopyCubeTilemap();

        //float hpPenalty = 0;
        //Area.FreeEntrance = false;
        //if( AreaCleared == false )                                                                                  // Free Exit Stuff
        //{
        //    if( GetPosArea( to ) == -1 )
        //    {
        //        if( Map.I.CurArea.AreaTurnCount >= 1 &&
        //            Map.I.RM.HeroSector.NumFreeReentrances <= 0 && Map.I.CurArea.FreePasses <= 0 )
        //        {
        //            CurArea.NumTimesExitedWhileDirty++;
        //            CurArea.Perfect = false;
        //            hpPenalty = Util.Percent( RM.RMD.DirtyAreaExitHPPenalty, G.Hero.Body.TotHp );                    
        //        }
        //        else Area.FreeEntrance = true;

        //        if( Map.I.CurArea.FreePasses <= 0 )
        //        Map.I.RM.HeroSector.NumFreeReentrances--;
        //    }
        //}

        //CurArea.PayExitAreaRunes( AreaCleared );

        //RM.UpdateResourceTimers( true );

        //if( AreaCleared )
        //{
        //    Quest.I.CurLevel.AreaList[ CurrentArea ].Cleared = true;
        //    bool res = HeroData.I.AddConqueredArea( Quest.I.CurLevel.AreaList[ CurrentArea ].name );
        //    if( res )
        //    {
        //        Map.I.LevelStats.AreasCleared++;

        //        if( Hero.Body.DestroyBarricadeLevel > 0 )
        //            RM.BarricadeDestroyInflation += RM.RMD.BarricadeDestroyInflation;
        //        if( Hero.Body.FireWoodNeeded > 0 )
        //            RM.FirewoodNeededInflation += RM.RMD.FirewoodNeededInflation;
        //    }
        //}

        //for( int a = 0; a < Quest.I.CurLevel.ArtifactList.Count; a++ )                                                 // Finish Collecting Artifacts
        //if ( Quest.I.CurLevel.ArtifactList[ a ].Collected == Artifact.EStatus.TEMPORARY_COLLECTED )
        //{
        //     Quest.I.CurLevel.ArtifactList[ a ].Collected = Artifact.EStatus.COLLECTED;
        //     Quest.I.CurLevel.ArtifactList[ a ].TimesBought++;
        //}

        //if( CurArea.AreaTurnCount >= 1 )
        //{
        //    RestoreOriginalOutAreaBarricades( true );

        //    RM.UpdateResourceClearing( CurArea, true );

        //    for( int i = 0; i < MoveOrder.Count; i++ )
        //        if( MoveOrder[ i ].Body.IsDead )
        //            PoolManager.Pools[ "Pool" ].Despawn( MoveOrder[ i ].transform );

        //    for( int i = 0; i < MoveOrder.Count; i++ )
        //    {
        //        MoveOrder[ i ].SetNewStartingPosition();
        //    }

        //    if( CurArea.MaximumWoundedUnits > CurArea.OriginalMaximumWoundedUnits )
        //        CurArea.OriginalMaximumWoundedUnits = CurArea.MaximumWoundedUnits;
        //    CurArea.OriginalPerfect = CurArea.Perfect;
        //    CurArea.OriginalVirgin = CurArea.Virgin;
        //    CurArea.OriginalBigMonsterVirgin = CurArea.BigMonsterVirgin;
        //}
        //else
        //{
        //    for( int i = 0; i < MoveOrder.Count; i++ )
        //    {
        //        MoveOrder[ i ].Revive( false );
        //    }

        //    for( int i = 0; i < MetaShootDeathList.Count; i++ )
        //    {
        //        MetaShootDeathList[ i ].Revive( false );
        //    }

        //    Manager.I.PackMule.RestartArea();

        //    for( int y = ( int ) P2.y; y <= P1.y; y++ )
        //    for( int x = ( int ) P1.x; x <= P2.x; x++ )
        //    if ( AreaID[ x, y ] == CurrentArea )
        //    if ( Gaia2[ x, y ] )
        //    if ( Gaia2[ x, y ].TileID == ETileType.FIRE )
        //         LightFire( x, y, Gaia2[ x, y ].Body.OriginalFireIsOn, false );
        //}

        //for( int y = ( int ) P2.y; y <= P1.y; y++ )
        //for( int x = ( int ) P1.x; x <= P2.x; x++ )
        //if( AreaID[ x, y ] == CurrentArea )
        //        {
        //            //if( PermaDeathPositionList.Contains( new Vector2( x, y ) ) == false )
        //            //    UpdateTileGameObjectCreation( x, y, ELayerType.MONSTER );
        //            UI.I.UpdateSaveGameIconState( x, y, true );
        //        }

        //if( AreaCleared )
        //{
        //    ClearAllRoomDoors();
        //    AreaCleared = false;
        //    if( CurArea.WasClearedWhenEntered == false )
        //        RandomMap.bUpdateRandomResourceTimers = true;
        //    RandomMap.RecentAreaClearedID = CurrentArea;
        //    CurArea.WasClearedWhenEntered = true;
        //}

        //RestoreOriginalAreaObjects( CurrentArea );
        //Area.UpdateAiTarget( Hero.Pos, false, true );

        //CurArea.AreaTurnCount = 0;
        //CurrentArea = -1;
        //AreaExitTurnCount = 0;
        //CurArea = null;
        //AreaSaveList = new List<Vector2>();
        //PreferedAreaSave = EntrancePushFrom = EntrancePushTo = new Vector2( -1, -1 );
        //RandomMap.bUpdateDynamicMonsterLeveling = true;

        //bool hp = true;
        //if( Manager.I.GameType == EGameType.CUBES ) hp = false;
        //InitHero( SelectedHero, to, hp, false, false, Hero.Dir );

        //if( hpPenalty > 0 )                                                                                 // Dirty Exit HP Punishment
        //{
        //    G.Hero.Body.ReceiveDamage( hpPenalty, EDamageType.BLEEDING, Map.I.Hero, null );
        //    Message.RedMessage( "Dirty Exit Punishment: " + hpPenalty.ToString( "0.#" ) + "HP" );
        //}       

        //Quest.I.UpdateArtifactStepping( Hero.Pos );
        ////HeroData.I.ClearAllCollectedArtifacts( Artifact.EArtifactLifeTime.AREA );
        //Quest.I.UpdateArtifactData( ref Hero, true );
        //Hero.Body.UpdateBleeding( true );
        //AreaNeighboring = false;
        //InvalidateInputTimer = .1f;
        //MoveOrder = null;
        return true;
    }  

    public void DetectFightingMap()
    {
        int area = -1;
        if( CurrentArea == -1 )
            for( int i = 0; i < Quest.I.CurLevel.AreaList.Count; i++ )
            {
                Rect r = Quest.I.CurLevel.AreaList[ i ].AreaRect;
                if( Hero.Pos.x >= r.x && Hero.Pos.x < r.x + r.width )
                    if( Hero.Pos.y <= r.y && Hero.Pos.y > r.y - r.height )
                        if( i == AreaID[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ] )
                        {
                            area = i;
                        }
            }

        if( CurrentArea == -1 && area != -1 )
        {
            if( Quest.I.CurLevel.AreaList[ area ].TL.Contains( Hero.Pos ) )
            {
                EnterArea( area );
                return;
            }
        }
    }

    public void UpdateBarricadeFighterAreaEnter()
    {
        if( CurArea.DirtyLifetimeSteps >= 1 ) return;

        if( Hero.Control.BarricadeFighterLevel >= 2 )                                                                      // Frontfacing +1
        {
            Unit un = GetUnit( ETileType.BARRICADE, Hero.GetFront() );
            if( un )
            if( GetPosArea( un.Pos ) != -1 )
            {
                un.SetVariation( un.Variation + 1 );
                Message.CreateMessage( ETileType.NONE, "+1", new Vector2( un.Pos.x, un.Pos.y + .2f ),
                                                             Color.green, true, true, 5, .2f, .15f );
            }
        }

        if( Hero.Control.BarricadeFighterLevel >= 3 )                                                                      // Hero neighboor tiles +1
        {
            List<Vector2> tgl  = new List<Vector2>();
            Rect rec = new Rect( new Vector2( Hero.Pos.x - 1, Hero.Pos.y - 1 ), new Vector2( 3, 3 ) );
            List<Unit> ul = Util.GetUnitsInTheArea( rec, ETileType.BARRICADE );

            for( int i = 0; i < ul.Count; i++ )
            if( GetPosArea( ul[ i ].Pos ) != -1 )
            {
                ul[ i ].SetVariation( ul[ i ].Variation + 1 );
                Message.CreateMessage( ETileType.NONE, "+1", ul[ i ].Pos, Color.green, true, true, 5, .2f, .12f );
            }
        }

        if( Hero.Control.BarricadeFighterLevel >= 4 )                                                                     // Hero frontal tiles +1
        {            
            for( int i = 0; i < 30; i++ )
            {
                Vector2 tg = Hero.Pos + ( Manager.I.U.DirCord[ ( int ) Hero.Dir ] ) * i;
                if( Map.I.PtOnAreaMap( tg ) )
                {
                    if( IsWall( tg ) ) break;
                    Unit un = GetUnit( ETileType.BARRICADE, tg );
                    if( un )
                    if( GetPosArea( un.Pos ) != -1 )
                    {
                        un.SetVariation( un.Variation + 1 );
                        Message.CreateMessage( ETileType.NONE, "+1", new Vector2( un.Pos.x, un.Pos.y + .4f ),
                                                                     Color.green, true, true, 5, .2f, .15f );
                    }
                }
            }
        }
    }    
    public static bool IsWall( Vector2 tg, bool orb = true, bool mine = true )
    {
        if( Map.PtOnMap( Map.I.Tilemap, tg ) == false ) return false;
        if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ] )
        {
            if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ].TileID == ETileType.BOULDER ) return true;
            if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ].TileID == ETileType.ORB && orb ) return true;
        }
        if( mine && Map.GFU( ETileType.MINE, tg ) ) return true;
        if( Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ] )
        {
            if( Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ].TileID == ETileType.PIT ) return false;
            if( Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ].TileID == ETileType.WATER ) return false;
            if( Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ].TileID == ETileType.LAVA ) return false;
            if( Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ].BlockMovement ) return true;
        }
        return false;
    }

    public void UpdateAutoWakeUp()
    {
        if( Map.I.CurrentArea == -1 ) return;

        int lev = ( int ) Mathf.Ceil( Map.I.Hero.Control.SneakingLevel ); 
        int tt = ( ( int ) HeroData.I.SneakingWakeUpExtraTurns[ lev ] + Map.I.RM.RMD.BaseWakeUpTurnAmount );
        int left = ( tt - Map.I.CurArea.DirtyLifetimeSteps );
        if( left <= 0 )
        {
            WakeUpAllMonsters();
        }
    }
   
    public void CopyCubeTilemap()
    {
        if( Quest.CurrentLevel != -1 ) return;
        if( CurArea.AreaTurnCount < 1 ) return;

        for( int y = ( int ) P2.y; y <= P1.y; y++ )
        for( int x = ( int ) P1.x; x <= P2.x; x++ )
            {
                ETileType g2 = ( ETileType ) Quest.I.CurLevel.Tilemap.GetTile( x, y, ( int ) ELayerType.GAIA2 );

                if( Gaia[ x, y ] )
                    Quest.I.CurLevel.Tilemap.SetTile( x, y, ( int ) ELayerType.GAIA, ( int ) Gaia[ x, y ].TileID );
                else
                    Quest.I.CurLevel.Tilemap.SetTile( x, y, ( int ) ELayerType.GAIA, ( int ) ETileType.NONE );

                if( Gaia2[ x, y ] )
                    Quest.I.CurLevel.Tilemap.SetTile( x, y, ( int ) ELayerType.GAIA2, ( int ) Gaia2[ x, y ].TileID );
                else
                    if( g2 != ETileType.ARTIFACT )
                    Quest.I.CurLevel.Tilemap.SetTile( x, y, ( int ) ELayerType.GAIA2, ( int ) ETileType.NONE );
            
                if( Unit[ x, y ] )
                {
                    Quest.I.CurLevel.Tilemap.SetTile( x, y, ( int ) ELayerType.MONSTER, ( int ) Unit[ x, y ].TileID );
                    if( Unit[ x, y ].TileID == ETileType.BARRICADE )
                        Quest.I.CurLevel.Tilemap.SetTile( x, y, ( int ) ELayerType.MONSTER, ( int ) Unit[ x, y ].TileID + Unit[ x, y ].Variation );
                }
                else
                    Quest.I.CurLevel.Tilemap.SetTile( x, y, ( int ) ELayerType.MONSTER, ( int ) ETileType.NONE );
            }  
    }

    public void WakeUpAllMonsters()
    {
        //if( Map.I.CurrentArea != -1 )
        //    for( int i = 0; i < Map.I.MoveOrder.Count; i++ )
        //    {
        //        Map.I.MoveOrder[ i ].Control.UpdateSleep( true );
        //    }
    }

    public void DestroyWall()
    {
        Vector2 tg = Hero.Pos + Manager.I.U.DirCord[ ( int ) Hero.Dir ];

        if( Map.I.PtOnAreaMap( tg ) &&
             Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ] != null &&
                  Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ].TileID == ETileType.FOREST )
        {
            int lev = ( int ) Mathf.Ceil( Map.I.Hero.Body.WallDestroyerLevel ); 

            int wd = Map.I.NumWallsDestroyed;
            int free = 0;
            if( Map.I.CurrentArea != -1 )
            {
                free = Map.I.CurArea.FreeBomb;
                wd += Map.I.CurArea.BombsUsed - free;
            }
            int avail = ( int ) ( Map.I.LevelStats.AreasCleared / HeroData.I.WallDestroyerClearedAreasPerBomb[ lev ] ) - wd;
            avail += ( int ) HeroData.I.WallDestroyerBombsPerLevel[ lev ];

            if( avail < 1 )
            {
                Map.I.ShowMessage( Language.Get( "ERROR_WALLBREAKER1" ) );
                return;
            }

            TKUtil.SetTile( tg, ETileType.OPENBLACKGATE );

            CurArea.BombsUsed++;
            MasterAudio.PlaySound3DAtVector3( "Explosion", transform.position );
        }
        else
            Map.I.ShowMessage( Language.Get( "ERROR_WALLBREAKER2" ) );
    }

    public bool TrimArrowPath()
    {
        int arrowcont = 0;
        for( int i = 0; i < Hero.Control.PathFinding.Path.Count; i++ )
        {
            Vector2 pt = Hero.Control.PathFinding.Path[ i ];
            if( Gaia2[ ( int ) pt.x, ( int ) pt.y ] )
                if( Gaia2[ ( int ) pt.x, ( int ) pt.y ].TileID == ETileType.ARROW ) arrowcont++;
        }

        if( arrowcont > 0 )
            Hero.Control.PathFinding.Path.RemoveAt( Hero.Control.PathFinding.Path.Count - 1 );

        if( Gaia2[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ] )
            if( Gaia2[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ].TileID == ETileType.ARROW )
                if( Hero.Control.PathFinding.Path.Count > 2 )
                {
                    Hero.Control.PathFinding.Path.Clear();
                    return true;
                }

        if( Hero.Control.PathFinding.Path.Count > 2 )
            for( int i = 0; i < Hero.Control.PathFinding.Path.Count; i++ )
            {
                Vector2 pt = Hero.Control.PathFinding.Path[ i ];
                if( Gaia2[ ( int ) pt.x, ( int ) pt.y ] )
                    if( Gaia2[ ( int ) pt.x, ( int ) pt.y ].TileID == ETileType.ARROW )
                    {
                        if( i > 0 )
                        {
                            Hero.Control.PathFinding.Path.Clear();
                            return true;
                        }
                    }
            }
        return false;
    }
    public static bool LowTerrainBlock( Vector2 to )
    {
        if( Controller.GetRaft( to ) == null ) 
        {
            if( Map.I.GetUnit( ETileType.WATER, to ) ) return true;
            if( Map.I.GetUnit( ETileType.PIT, to ) ) return true;
        }
        return false;
    }
    public Unit GetMud( Vector2 tg )
    {
        if( PtOnMap( Tilemap, tg ) == false ) return null;
        Unit mud = Gaia[ ( int ) tg.x, ( int ) tg.y ];
        if( mud && mud.TileID == ETileType.MUD )
            return Gaia[ ( int ) tg.x, ( int ) tg.y ];
        mud = Map.GFU( ETileType.MUD_SPLASH, tg );
        if( mud ) return mud;
        return null;
    }
    public Unit GetUnit( ETileType tile, Vector2 tg, bool checkworking = true )
    {
        if( PtOnMap( Tilemap, tg ) == false ) return null;
        if( Unit == null ) return null;
        if( Gaia == null ) return null;
        if( Gaia2 == null ) return null;
        Unit un = null;
        if( Unit[ ( int ) tg.x, ( int ) tg.y ] )
            if( Unit[ ( int ) tg.x, ( int ) tg.y ].TileID == tile )
                un = Unit[ ( int ) tg.x, ( int ) tg.y ];

        if( Gaia[ ( int ) tg.x, ( int ) tg.y ] )
            if( Gaia[ ( int ) tg.x, ( int ) tg.y ].TileID == tile )
                un = Gaia[ ( int ) tg.x, ( int ) tg.y ];

        if( Gaia2[ ( int ) tg.x, ( int ) tg.y ] )
            if( Gaia2[ ( int ) tg.x, ( int ) tg.y ].TileID == tile )
                un = Gaia2[ ( int ) tg.x, ( int ) tg.y ];

        if( checkworking )
            if( un && un.Body && un.Body.isWorking == false ) un = null;
        return un;
    }
    public Unit GetFire( Vector2 tg, bool checklit = true )
    {
        if( PtOnMap( Tilemap, tg ) == false ) return null;
        Unit fire = GetUnit( ETileType.FIRE, tg );
        if( fire && fire.Body.FireIsOn || checklit ) return fire;
        fire = GetUnit( ETileType.BARRICADE, tg );
        if( fire && fire.Body.FireIsOn || checklit ) return fire;
        return null;
    }

    public void CopyTilemap( bool justadd, tk2dTileMap from, ref tk2dTileMap to, Vector2 toOrigin, Vector2 fromOrigin, int sx, int sy,
    bool flipx, bool flipy, bool tile = true, bool gaia = true, bool gaia2 = true, bool monster = true, bool mod = true, bool dec = true, bool areas = true, bool dec2 = true, bool raft = true )
    {
        for( int y = 0; y < sx; y++ )
        for( int x = 0; x < sy; x++ )
            {
                Vector2 tg = toOrigin + new Vector2( x, y );

                if( flipx ) tg.x = ( toOrigin.x + sx - 1 ) - x;
                if( flipy ) tg.y = ( toOrigin.y + sy - 1 ) - y;

                for( int l = 0; l < 10; l++ )
                if( tile && l == 0 || gaia && l == 1 || gaia2 && l == 2 || monster && l == 3 || 
                    mod && l == 4 || dec && l == 6 || areas && l == 7 || dec2 && l == 8 || raft && l == 9 )
                    {
                        if( justadd == false )
                            to.SetTile( ( int ) tg.x, ( int ) tg.y, l, ( int ) ETileType.NONE );
                        int tl = from.GetTile( ( int ) fromOrigin.x + x, ( int ) fromOrigin.y + y, l );

                        //if( tl != ( int ) ETileType.ARTIFACT )
                        {
                            if( justadd == false || tl != -1 )
                                to.SetTile( ( int ) tg.x, ( int ) tg.y, l, tl );
                        }

                        FlipTile( to, ( ETileType ) tl, tg, l, flipx, flipy );
                    }
            }
    }
    
    public void FlipTile( tk2dTileMap tm, ETileType tl, Vector2 tg, int l, bool flipx, bool flipy )
    {
        if( flipx == false )
        if( flipy == false ) return;

        if( tl <= 0 ) return;
        int originaltype = ( int ) Map.GetTileID( ( ETileType ) tl );
        if( originaltype != ( int ) ETileType.ARROW )
        if( originaltype != ( int ) ETileType.BOULDER )
        if( originaltype != ( int ) ETileType.ORIENTATION ) return;
        int vari = ( int ) tl - originaltype;

        if( flipx && !flipy )
        {
            if( vari == -1 ) vari =  1; else
            if( vari == 1  ) vari = -1; else
            if( vari == 15 ) vari = 17; else
            if( vari == 17 ) vari = 15; else
            if( vari == 31 ) vari = 33; else
            if( vari == 33 ) vari = 31; 
        }
        else
        if(!flipx &&  flipy )
        {
            if( vari == -1 ) vari =  31; else
            if( vari == 31 ) vari = -1;  else
            if( vari == 0  ) vari =  32; else
            if( vari == 32 ) vari =  0;  else
            if( vari == 1  ) vari =  33; else
            if( vari == 33 ) vari =  1; 
        }
        else
        if( flipx && flipy )
        {
            if( vari == -1 ) vari =  33; else
            if( vari == 33 ) vari = -1;  else
            if( vari == 1  ) vari =  31; else
            if( vari == 31 ) vari =  1;  else
            if( vari == 0  ) vari =  32; else
            if( vari == 32 ) vari =  0;  else
            if( vari == 15 ) vari =  17; else
            if( vari == 17 ) vari =  15; 
        }

        int tile = originaltype + vari;
        tm.SetTile( ( int ) tg.x, ( int ) tg.y, l, ( int ) tile );
    }

    public int GetPosArea( Vector2 pos )
    {
        if( AreaID == null ) return -1;
        if( PtOnMap( Tilemap, pos ) == false ) return -1;
        if( Manager.I.GameType == EGameType.NAVIGATION ) return -1;
        if( Manager.I.GameType == EGameType.FARM ) return -1;
        return AreaID[ ( int ) pos.x, ( int ) pos.y ];
    }

    public Area GetArea( int x, int y )
    {
        return GetArea( new Vector2( x, y ) );
    }

    public Area GetArea( Vector2 pos )
    {
        if( AreaID == null ) return null;
        if( PtOnMap( Tilemap, pos ) == false ) return null;
        int area = AreaID[ ( int ) pos.x, ( int ) pos.y ];
        if( area == -1 ) return null;
        return Quest.I.CurLevel.AreaList[ area ];
    }
    public static ELayerType GetTileLayer( ETileType tile )
    {
        if( tile < 0 ) return ELayerType.NONE;

        var tileInfo = Map.I.Tilemap.data.tileInfo[ ( int ) tile ];
        if( tileInfo.Layer != ELayerType.NONE ) return tileInfo.Layer;

        tile = Map.GetTileID( tile );

        switch( tile )
        {
            case ETileType.GRASS:

            return ELayerType.TERRAIN; 

            case ETileType.MUD:
            case ETileType.WATER:
            case ETileType.FOREST:
            case ETileType.SAND:
            case ETileType.CLOSEDDOOR:
            case ETileType.OPENDOOR:
            case ETileType.ROOMDOOR:
            case ETileType.OPENROOMDOOR:
            case ETileType.PIT:
            case ETileType.TRAP:
            case ETileType.FREETRAP:
            case ETileType.SNOW:
            case ETileType.GREEN_PRESSUREPLATE:
            case ETileType.RED_PRESSUREPLATE:
            case ETileType.SPLITTER:
            case ETileType.BLACKGATE:
            case ETileType.OPENBLACKGATE:
            case ETileType.ROAD:
            case ETileType.STONEPATH:
            case ETileType.LAVA:

            return ELayerType.GAIA;

            case ETileType.RAFT:
            case ETileType.FOG:
            case ETileType.ORI3:
            return ELayerType.RAFT;
            case ETileType.DOOR_KNOB:
            case ETileType.DOOR_OPENER:
            case ETileType.DOOR_SWITCHER:
            case ETileType.DOOR_CLOSER:
            case ETileType.ARROW:
            case ETileType.ARTIFACT:
            case ETileType.SCROLL:
            case ETileType.CHECKPOINT:
            case ETileType.CAMERA:
            case ETileType.FIRE:
            case ETileType.ITEM:
            case ETileType.BUILDING:
            case ETileType.MAP_QUEST:
            case ETileType.MAP_BONUS:
            case ETileType.BLOCK:
            case ETileType.SAVEGAME:
            case ETileType.SECRET:
            case ETileType.MUD_SPLASH:  // mud splash is a gaia2 but the prefab is monster since it becomes a flyer. this allowy you to place monsters over it
            case ETileType.THORNS:
 
            return ELayerType.GAIA2;

            case ETileType.ROACH:
            case ETileType.SCARAB:
            case ETileType.MOSQUITO:
            case ETileType.GLUEY:
            case ETileType.SLIME:
            case ETileType.DOME:
            case ETileType.BOULDER:
            case ETileType.BARRICADE:
            case ETileType.INACTIVE_HERO:
            case ETileType.DRAGON1:
            case ETileType.DRAGON2:
            case ETileType.QUEROQUERO:
            case ETileType.MOTHERJUMPER:
            case ETileType.JUMPER:
            case ETileType.MOTHERWASP:
            case ETileType.WASP:
            case ETileType.SPIDER:
            case ETileType.ORB:
            case ETileType.MINE:
            case ETileType.FISH:
            case ETileType.HERB:
            case ETileType.BLOCKER:
            case ETileType.FROG:
            case ETileType.ALGAE:
            case ETileType.SCORPION:
            case ETileType.HUGGER:
            case ETileType.PLAGUE_MONSTER:
            case ETileType.PROJECTILE:
            case ETileType.ALTAR:
            case ETileType.VINES:
            case ETileType.SPIKES:
            case ETileType.TOWER:
            case ETileType.FISHING_POLE:
            case ETileType.IRON_BALL:
            case ETileType.BOUNCING_BALL:
            case ETileType.FAN:
            case ETileType.BRAIN:
            case ETileType.TRAIL:

            return ELayerType.MONSTER;

            case ETileType.LEVEL_ENTRANCE:
            case ETileType.MOD1:
            case ETileType.MOD2:
            case ETileType.MOD3:
            case ETileType.MOD4:
            case ETileType.MOD5:
            case ETileType.MOD6:
            case ETileType.MOD7:
            case ETileType.MOD8:
            case ETileType.MOD9:
            case ETileType.MOD10:
            case ETileType.MOD11:
            case ETileType.MOD12:
            case ETileType.MOD13:
            case ETileType.MOD14:
            case ETileType.MOD15:
            case ETileType.MOD16:
            case ETileType.MOD17:
            case ETileType.MOD18:
            case ETileType.MOD19:
            case ETileType.MOD20:
            case ETileType.MOD21:
            case ETileType.MOD22:
            case ETileType.MOD23:
            case ETileType.MOD24:
            case ETileType.MOD25:
            case ETileType.MOD26:
            case ETileType.MOD27:
            case ETileType.MOD28:
            case ETileType.MOD29:
            case ETileType.MOD30:
            case ETileType.MOD31:
            case ETileType.MOD32:

            return ELayerType.MODIFIER;

            case ETileType.AREA_DRAG:
            case ETileType.AREA_DRAG2:
            case ETileType.AREA_DRAG3:
            case ETileType.AREA_DRAG4:
            case ETileType.CAM_AREA:
            case ETileType.ORIENTATION:

            return ELayerType.AREAS;
            case ETileType.BOOL_TOOGLE_1:
            return ELayerType.DECOR;
            case ETileType.BOOL_TOOGLE_2:
            return ELayerType.DECOR2;
            case ETileType.GRID:
            return ELayerType.GRID;

            case ETileType.DECOR_TALL:
            return ELayerType.DECOR;
        }
        return ELayerType.TERRAIN;
    }

    //_____________________________________________________________________________________________________________________ is in the same line
    public bool IsInTheSameLine( Vector2 pos1, Vector2 pos2, bool diagonal = true )
    {
        if( pos1.x == pos2.x ) return true;
        if( pos1.y == pos2.y ) return true;
        if( diagonal == false ) return false;
        float distX = Mathf.Abs( pos1.x - pos2.x );
        float distY = Mathf.Abs( pos1.y - pos2.y );

        if( distX == distY ) return true;
        return false;
    }

    public void ClearArea( int area )
    {
        if( area < 0 ) return;
        if( Quest.I.CurLevel.AreaList.Count <= 0 ) return;
        UpdateAreaColor( area );
        ClearAllMonstersInTheArea( area, true, false, false );
        Create2DMap = true;

        Area ar = Quest.I.CurLevel.AreaList[ area ];
        ar.UpdateAreaColor( true );

    }

    //_____________________________________________________________________________________________________________________ Update Area Color

    public void UpdateAreaColor( int area )
    {
        if( Quest.I.CurLevel.AreaList == null ) return;
        if( Quest.I.CurLevel.AreaList.Count <= 0 ) return;

        Debug.Log(area);

        Area ar = Quest.I.CurLevel.AreaList[ area ];

        ar.Discovered = true;
        int numRevealed = 0;
        for( int y = ( int ) ar.P2.y; y <= ar.P1.y; y++ )
        for( int x = ( int ) ar.P1.x; x <= ar.P2.x; x++ )
        if ( Revealed[ x, y ] == false ) ar.Discovered = false; else numRevealed++;

        ar.UpdateAreaColor();
    }

    public bool IsHeroMeleeAvailable()
    {
        if( Manager.I.GameType != EGameType.CUBES ) return false;
        int melee = ( int ) Item.GetNum( ItemType.Res_Melee_Attacks );
        if( Map.I.RM.RMD.LimitedMeleeAttacksPerCube == -1 ) melee = 1;
        if( G.Hero.Body.MeleeAttackLevel < 1 ) melee = 0;
        float tottime = G.Hero.MeleeAttack.GetRealtimeSpeedTime();
        if( Map.Stepping() == false )
        if( G.Hero.MeleeAttack.SpeedTimeCounter < tottime ) melee = 0;
        if( melee > 0 )
            return true;
        return false;
    }
    public void UpdateMarkedMonsters()
    {
        //if( MoveOrder == null ) return;
        //int numMarked = 0; int nonMarked = 0;                                                           // Check Marked Monsters
        //for( int i = 0; i < MoveOrder.Count; i++ )
        //    if( MoveOrder[ i ].ValidMonster )
        //        if( MoveOrder[ i ].Body.IsMarked )
        //            numMarked++;
        //        else
        //            if( MoveOrder[ i ].Body.IsDead == false ) nonMarked++;
        //            else numMarked++;

        //if( numMarked > 0 )
        //    if( nonMarked <= 0 )
        //        for( int i = 0; i < MoveOrder.Count; i++ )
        //        {
        //            MoveOrder[ i ].Body.IsMarked = false;
        //            MoveOrder[ i ].Body.EffectList[ 0 ].SetActive( false );
        //        }
    }
    
   public bool IsBarricade( Vector2 tg )
   {
       if( !PtOnMap( Tilemap, tg ) ) return false;
       if( Unit[ ( int ) tg.x, ( int ) tg.y ] )
       if( Unit[ ( int ) tg.x, ( int ) tg.y ].TileID == ETileType.BARRICADE ) return true;
       return false;
   }
   public void StopAllLoopedSounds()
   {
       MasterAudio.StopAllOfSound( "Snow Slide" );
       MasterAudio.StopAllOfSound( "Sand Slide" );
       MasterAudio.StopAllOfSound( "Breathing 2" );
       MasterAudio.StopAllOfSound( "Fan Loop" );
   }
}

