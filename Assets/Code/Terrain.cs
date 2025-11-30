using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;
using DigitalRuby.LightningBolt;
using PathologicalGames;
public partial class Controller : MonoBehaviour
{
    public bool UpdateMudObjectPush( bool apply, Vector2 from, Vector2 to, bool forcedpush = false, bool bambooPushed = false )
    {
        if( Manager.I.GameType != EGameType.CUBES )
        if( Manager.I.GameType != EGameType.FARM ) return false;
        if( from == to ) return false;   
        EDirection mov = Util.GetTargetUnitDir( from, to );
        List<Vector2> tgl = new List<Vector2>();
        Map.I.GetMimicTargets( ref tgl, mov );                                                             // target for slide push
        List<Unit> ul = Map.I.GetFUnit( to );
        bool firstSlideOk = false;
        Vector2 firstSlideTg = new Vector2( -1, -1 );
        bool checkmud = true;
        LowTerrainPush = false;
        MudPushing = false;
        if( ul != null )
        for( int i = ul.Count - 1; i >= 0; i-- )                                                           // flying objects
        {
            if( ul[ i ].TileID == ETileType.WASP )
            if( ul[ i ].Control.FireMarkedWaspFactor > 0 )
                    checkmud = false;
            if( ul[ i ].Body.MudPushable == false )
                ul.RemoveAt( i );
        }
        if( ul == null ) ul = new List<Unit>();
        Unit mn = Map.I.GetUnit( to, ELayerType.MONSTER );                                                 // add monster
        if( mn != null && mn.Body && mn.Body.MudPushable ) ul.Add( mn );
        Unit obj = Map.I.GetUnit( to, ELayerType.GAIA2 );
        if( obj != null && obj.Body && obj.Body.MudPushable ) ul.Add( obj );                               // add gaia2
        
        if( Manager.I.GameType == EGameType.FARM )
        if( obj != null && obj.TileID == ETileType.ITEM )                                                  // Allows items to be pushed in the farm
        { ul.Add( obj ); obj.Body.MudPushable = true; }

        if( G.HS && G.HS.FreePushableObjects > 0 )                                                         // forced push - no mud required
            forcedpush = true;
        bool push = false;
        for( int pass = 0; pass < 3; pass++ )
        {
            bool arrowpush = false;
            EDirection dr = Util.GetTargetUnitDir( from, to );
            Unit arrow = Map.I.GetUnit( ETileType.ARROW, to );                                                // Over arrow slide push
            if( arrow )
            if( arrow.Dir == G.Hero.Dir )
            {
                tgl = new List<Vector2>();
                Map.I.GetMimicTargets( ref tgl, arrow.Dir );                                                  // target for slide push when arrow pushing
                dr = arrow.Dir;
                arrowpush = true;
            }

            Vector2 tg = to + Manager.I.U.DirCord[ ( int ) dr ];
            if( pass > 0 )                                                                                    // Test Slide Push after failed straight push on pass 1
            {
                if( IsSlidable( tg ) == false ) return false;                                                 // no slide against grass
                if( arrowpush == false )
                if( Util.IsDiagonal( dr ) == false ) return false;
                if( tgl.Count <= 1 ) return false;
                tg = to + tgl[ pass ];                                                                        // test 2 possible slide push targets
            }

            Unit mudfr = Map.I.GetMud( to );
            Unit mudto = Map.I.GetMud( tg );
            Unit gaia2 = Map.I.GetUnit( tg, ELayerType.GAIA2 );
            Unit monster = Map.I.GetUnit( tg, ELayerType.MONSTER );
            if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) ) return false;                                // idea: pushed object ignore arrow
            bool mdiag = G.Hero.CheckDiagonalMove( true, from, to );
            if( mdiag ) return false;
            if( ul == null ) return false;
            for( int id = ul.Count - 1; id >= 0; id-- )                                                        // list of objects loop
            {
                obj = ul[ id ];
                bool ga2 = true;

                if( obj.Body == null ) return false;
                if( obj.Control == null ) return false;
                if( obj.Body.MudPushable == false ) return false;
                if( obj.TileID == ETileType.BOULDER && G.HS && G.HS.BoulderSidePush == false )                // boulder and mushroom infected sidepush only
                    return false;
                if( obj.Control.ForcedFrontalMovementDir != EDirection.NONE ||
                    obj.TileID == ETileType.BOULDER )
                if( dr != obj.GetRelativeDirection( ( int ) EDirection.E ) &&
                    dr != obj.GetRelativeDirection( ( int ) EDirection.W ) )
                    return false;

                if( obj.UnitType == EUnitType.MONSTER )
                {
                    if( gaia2 && gaia2.TileID == ETileType.ITEM &&
                        gaia2.Variation == ( int ) ItemType.Res_Mushroom )
                        ga2 = false;                                                                           // monster over mushroom push
                }

                if( obj.TileID == ETileType.FIRE && obj.Body.FireIsOn ) return false;                          // Fire is on, do not move

                bool monsterl = true;
                List<ETileType> ex = new List<ETileType>();
                ex.Add( ETileType.ARROW ); ex.Add( ETileType.WATER );
                ex.Add( ETileType.PIT ); ex.Add( ETileType.LAVA );

                if( obj && obj.Body.MineType == EMineType.HOOK )                                               // hook exception
                {
                    bool res = obj.Control.UpdateHookMinePush( apply, to, tg );                                // Hook push
                    if( res == false ) return false;
                    if( monster && monster.Body.RopeConnectSon != obj )
                        monsterl = false;
                    if( obj.Body.MinePushSteps >= 0 ||
                        IsConectableMine( monster ) )
                        forcedpush = true;
                }

                if( gaia2 && gaia2.TileID == ETileType.FIRE )                                                  // monster over fire push
                if( obj.TileID != ETileType.FIRE )
                    ga2 = false;

                if( obj.TileID == ETileType.MINE ||
                    obj.UnitType == EUnitType.MONSTER )
                    ga2 = false;

                bool block = Util.CheckBlock( to, tg, G.Hero, true, true, false, monsterl, ga2, true, ex );
                Unit tom = Map.GFU( ETileType.MINE, to );
                if( Map.GFU( ETileType.MINE, tg ) ) block = true;
                if( obj && obj.Body.MineType == EMineType.LADDER )                                             // Limit Ladder kick
                {
                    //if( BridgeFloor != 0 )
                    block = true;
                }
                if( obj.TileID == ETileType.MINE &&
                    obj.CheckLeverBlock( false, obj.Pos, tg, false ) ) block = true;                           // mine lever block

                if( obj && obj.IsPushAble() == false ) block = true;                                           // No Tunnel pushing
                if( Map.GFU( ETileType.FISHING_POLE, to ) )                                                    // no Fishing pole overlapping
                if( Map.GFU( ETileType.FISHING_POLE, tg ) ) block = true;
                if( Map.LowTerrainBlock( tg ) ) 
                  { LowTerrainPush = true; block = true; }

                if( forcedpush == false )
                if( obj.Control.IsFlyingUnit == false || checkmud )                                            // land monster specific
                {
                    if( mudto == null ) block = true;
                    if( mudfr == null ) block = true;
                }

                if( block )
                {
                    if( mn == null ) mn = tom;
                    if( mn )
                    if( ( mn.ValidMonster && mn.Control.HeroPushedMonsterBarricadeDestroyCount <= 0 ) ||
                        ( mn.TileID == ETileType.MINE && mn.IsPushAble() ) )                                    // monster push over barricade destroy it       
                    if( monster && monster.TileID == ETileType.BARRICADE )
                    {
                        Map.I.CountRecordTime = true;
                        monster.DestroyBarricade( tg );
                        monster.Body.BumpTimer = Map.I.RM.RMD.BarricadeDestroyWaitTime;
                        Controller.BarricadeBumpTimeCount = 0;
                        mn.Control.SpeedTimeCounter = 0;
                        if( mn.MeleeAttack )
                            mn.MeleeAttack.SpeedTimeCounter = 0;
                        mn.Control.HeroPushedMonsterBarricadeDestroyCount++;
                    }
                    block = true;
                }

                if( obj.TileID == ETileType.FIRE )
                {
                    bool res = obj.CheckFirepitLogBlock( from, to );                                         // Firepit log block
                    if( res ) block = true;
                }

               if( block == false )
               {
                   if( pass == 1 )                                                                           // first slide pass: 
                   {
                       firstSlideOk = true;                                                                  // if doable, do not move yet
                       firstSlideTg = tg;                                                                    // save tg and
                       block = true;                                                                         // do not move yet
                   }
                   else 
                   if( pass == 2 )
                   {
                       if( firstSlideOk ) return false;                                                       // both slide targets are doable, so return
                   }
               }

               if( pass == 2 )                                                                                // last slide pass:
               {
                   if( block == true )                                                                        // if not doable,
                   if( firstSlideOk )                                                                        
                   {
                       tg = firstSlideTg;                                                                     // retrieve first target
                       block = false;                                                                         // and enables the pushing
                   }                   
                   if( firstSlideOk )
                   {
                       if( IsSlidable( to + tgl[ 2 ] ) == false ) 
                           return false;                                                                      // no slide against grass on first try
                   }
                   else
                   {
                       if( IsSlidable( to + tgl[ 1 ] ) == false ) 
                           return false;                                                                      // no slide against grass on second try
                   }
               }

            if( block == false ) push = true; // new

            if( block == false )
            if( apply )
            for( int i = 0; i < ul.Count; i++ )                                                                // list of objects loop
            {
                Map.I.CountRecordTime = true;
                obj = ul[ i ];
                if( obj.Control.IsFlyingUnit == false )
                {
                    obj.Control.BeingMudPushed = true;
                    obj.Control.ApplyMove( to, tg );                                                           // Push land object
                    obj.Control.SpeedTimeCounter = 0;
                    push = true;
                    Util.PlayParticleFX( "Mud Splat", tg );                                                    // splat FX

                    if( obj.TileID == ETileType.FIRE )
                    {
                        for( int dd = 0; dd < 8; dd++ )                                                        // destroy fire log if there is a fire lit around
                        if( obj.Body.WoodAdded[ dd ] == true )
                        {
                            Vector2 logto = obj.Pos + Manager.I.U.DirCord[ ( int ) dd ];
                            Unit fire = Map.I.GetUnit( ETileType.FIRE, logto );
                            if( fire && fire.Body.FireIsOn )
                            {
                                Map.I.CreateExplosionFX( logto );                                              // log Explosion FX
                                MasterAudio.PlaySound3DAtVector3( "Fire Ignite", G.Hero.Pos );
                                obj.Body.WoodAdded[ dd ] = false;
                            }
                        }
                    }

                    if( obj.TileID == ETileType.HUGGER )
                        obj.Control.HuggerStepList[ 0 ] = from;                                                // Pushed Hugger Poison pos update

                    if( obj.ValidMonster )
                    {
                        obj.Control.WakeUp();                                                                  // wakes up monster
                        if( obj.TileID == ETileType.SCORPION )                                                 // Pushed Scorpion do not move or attack
                        {
                            obj.Control.UnitMoved = true;
                            obj.Control.SkipMovement = 1;
                            if( bambooPushed == true )
                            {
                                obj.Control.UnitMoved = false;
                                obj.Control.SkipMovement = 0;
                            }
                        }
                    }
                }
                else
                {
                    obj.Control.JumpTarget = tg;                                                                 // Push flying object
                    obj.Control.FlyingTarget = tg;
                    obj.Control.BeingMudPushed = true;
                    obj.Control.FlightJumpPhase = 1;
                    if( obj.Control.FireMarkedWaspFactor < Map.I.RM.RMD.FireMarkedWaspTurns )
                        obj.Control.FireMarkedWaspFactor = Map.I.RM.RMD.FireMarkedWaspTurns;
                    Vector2 fact = ( from - to ) * .45f;
                    //if( obj.TileID == ETileType.FISHING_POLE ) fact = Vector2.zero;                            // I dont know if the fact is used only for wasps, these ones do not use fact
                    obj.transform.position = tg + fact;
                    obj.Control.FlightPhaseTimer = 0;
                    Unit item = Map.I.GetUnit( ETileType.ITEM, tg );
                    if( item && item.Variation == ( int ) ItemType.Res_Mushroom )                                // squashes mushroom in the way
                        Map.Kill( item );

                    Util.PlayParticleFX( "Mud Splat", tg );                                                      // splat FX

                    if( obj.TileID == ETileType.MINE )
                    {
                        Controller.IgnoreLeverPush = true;
                        Controller.MoveFlyingUnitTo( ref obj, to, tg );
                        Controller.IgnoreLeverPush = false;
                        obj.Mine.AnimateIconTimer = 1;
                        if( mudfr == null || mudto == null )
                        {
                            if( obj.Body.MineType == EMineType.HOOK )
                                if( --obj.Body.MinePushSteps < 0 )                                             // hook steps decrement                                                       
                                    MasterAudio.PlaySound3DAtVector3( "Mechanical Sound", G.Hero.Pos );
                            if( G.HS ) G.HS.FreePushableObjects--;                                             // free push mine bonus
                        }
                    }

                    if( obj.Control.FireMarkedWaspFactor > 0 )
                    {
                        List<Unit> wl = Util.GetFlyingNeighbors( to, ETileType.WASP );                          // Fire Mark Hero Neighbor Wasps
                        for( int w = 0; w < wl.Count; w++ )
                        {
                            if( wl[ w ].Control.FireMarkAvailable )
                            {
                                MasterAudio.PlaySound3DAtVector3( "Save Game", G.Hero.Pos, 1, 1.6f );
                                wl[ w ].Control.FireMarkedWaspFactor += Map.I.RM.RMD.FireMarkedWaspTurns;
                                wl[ w ].Control.FireMarkAvailable = false;
                            }
                        }
                    }
                }
            }
        }
            if( push ) break;
        }

        if( push )
        {
            MudPushing = true;
            MasterAudio.PlaySound3DAtVector3( "Mud Push", G.Hero.Pos );                                      // Push sound FX    
        }
        return true;
    }

    public bool IsSlidable( Vector2 tg )
    {
        if( Map.IsWall( tg ) ) return true;
        Unit mn = Map.I.GetUnit( tg, ELayerType.MONSTER );                                                 // add monster
        if( mn && mn.ValidMonster ) return true;
        if( mn && mn.TileID == ETileType.FAN ) return true;
        if( Map.GFU( ETileType.MINE, tg ) ) return true;
        return false;
    }

        //______________________________________________________________________________________________________________________ Update Smooth animation
    public void UpdateSmoothMovementAnimation()
    {
        TurnTime += Time.deltaTime;
        float smoothAnimTime = 0.14f;
        SmoothMovementFactor = TurnTime / smoothAnimTime;
        SmoothMovementFactor = Mathf.Clamp( SmoothMovementFactor, 0f, 1f );
        if( Unit == false ) return;
        //if( Unit.Control.IsFlyingUnit )                   // new: test if it works
        //if( Unit.TileID != ETileType.RAFT )
        //if( Unit.TileID != ETileType.FOG )
        //if( Unit.TileID != ETileType.HERB )
        //if( Unit.TileID != ETileType.ALGAE )
        //if( Unit.TileID != ETileType.SPIKES )
        //if( Unit.TileID != ETileType.VINES )
        //if( Unit.TileID != ETileType.FISHING_POLE )
        //if( Unit.TileID != ETileType.MUD_SPLASH )
        //if( Unit.TileID != ETileType.MINE )
        //if( Unit.ProjType( EProjectileType.BOOMERANG ) == false )
        //    return;
        float fact = SmoothMovementFactor;

        if( Map.I.HeroIsDead ) fact = 1;                                                            // move monsters immediatelly to see whos the killer

        if( Unit.UnitType == EUnitType.HERO )
         {
             if( Map.I.TunnelPhase != -1 ) { Mine.UpdateTunnelAnimation(); return; }                // hero travels through tunnel
             Unit snow = Map.I.GetUnit( ETileType.SNOW, Unit.Pos );
             Unit sand = Map.I.GetUnit( ETileType.SAND, Unit.Pos );
             if( snow || sand )
             {
                 Unit.Graphic.transform.localPosition = new Vector2( 0, 0 );
                 return;
             }

             if( fact >= 1 )
                 Unit.transform.position = Unit.Pos;
             else
             {
                 G.Hero.transform.position = new Vector3( G.Hero.Pos.x, G.Hero.Pos.y, G.Hero.transform.position.z );
                 G.Hero.Spr.transform.localPosition = new Vector3( 0, 0, G.Hero.Spr.transform.localPosition.z );
                 G.Hero.Graphic.transform.localPosition = new Vector3( 0, 0, G.Hero.Graphic.transform.localPosition.z );
             }
         }
        #region animation
        //else           // this is an animation animation
        //if( Unit.UnitType == EUnitType.MONSTER )
        //if( Unit.ValidMonster )
        //{
        //    if( Unit.Control.JumpTarget.x <= 0 ||
        //      ( Unit.transform.position.x == Unit.Control.JumpTarget.x &&
        //        Unit.transform.position.y == Unit.Control.JumpTarget.y ) )
        //    {
        //        float rad = .03f;
        //        Vector2 randv = new Vector2( Random.Range( -rad, +rad ), Random.Range( -rad, +rad ) );
        //        Unit.Control.JumpTarget = new Vector2( Unit.transform.position.x, Unit.transform.position.y ) + randv;
        //        Unit.Control.FlightSpeed = Helper.I.FloatVal3 + Random.Range( 0f, 1);
        //    }
        //    else
        //    {
        //        if( Unit.Control.JumpTarget != new Vector2( -1, -1 ) )
        //        {
        //            Unit.transform.position = Vector3.MoveTowards( Unit.transform.position,
        //            Unit.Control.JumpTarget, Time.deltaTime * Unit.Control.FlightSpeed );
        //        }
        //    }
        //}
        #endregion

        Vector3 old = AnimationOrigin;
        Vector3 op = new Vector3( old.x, old.y, Unit.transform.position.z );
        if( op.x == -1 ) op = Unit.transform.position;
        Vector3 from = new Vector3( op.x, op.y, Unit.transform.position.z );
        Vector3 to = Unit.transform.position;
        if( Unit.UnitType == EUnitType.HERO || Unit.ValidMonster )
        {
            to += UpdateObjSuspended();
            from += UpdateObjSuspended();
        }

        Unit.Control.NotAnimatedPosition = Vector3.Lerp( from, to, fact );
        if( fact >= 1 )                                                                              // Over Raft: Unit Floats together
        {
            if( Unit.TileID == ETileType.RAFT ) return;
            if( Unit.TileID == ETileType.ALGAE ) return;
            Unit raft = Controller.GetRaft( Unit.Pos );

            if( raft != null )
            if( Map.I.GetUnit( ETileType.PIT, Unit.Pos ) == null || 
                Unit.ValidMonster == false || Map.I.RaftDragTurnTime >= .21f )
            {
                Unit.Graphic.transform.position = raft.Graphic.transform.position;
                return;
            }
        }
  
        if( Map.I.GetUnit( ETileType.WATER, Unit.Pos ) )                                            // Floating units
        if( Map.GMine( EMineType.BRIDGE, Unit.Pos ) == null )
        if( Controller.GetRaft( Unit.Pos ) == null )
        if( Unit.ProjType( EProjectileType.BOOMERANG ) == false ) 
        if( Unit.TileID != ETileType.FROG )            
            return;

        Unit.Graphic.transform.position = Vector3.Lerp( from, to, fact );                           // Calculates Graphic position 
    }
    public void UpdateBrokenRaft()
    {
        if( RaftMoving ) return;
        Unit fr = Controller.GetRaft( OldPos );
        Unit to = Controller.GetRaft( Unit.Pos );
        if( fr )
        if( fr.Control.BrokenRaft )
        {
            FallingAnimation.Create( 75, 1, new Vector3( 
            OldPos.x, OldPos.y, -0.33f ), fr.transform.eulerAngles, .1f );
            MasterAudio.PlaySound3DAtVector3( "Wood Break", fr.transform.position );                     // FX
            Map.I.CreateExplosionFX( OldPos );
            List<Unit> rl = GetRaftList( fr.Control.RaftGroupID );
            EDirection mov = fr.Control.RaftMoveDir;
            FromBrokenRaft = true;                                                                       // to avoid raft impulsionated
            int id = fr.Control.RaftGroupID;
            fr.Kill();                                                                                   // kill obj
            Unit ga = Map.I.GetUnit( OldPos, ELayerType.GAIA2 );
            if( ga ) ga.Kill();
            List<int> idl = new List<int>();
            List<Unit> rrl = new List<Unit>();
            int top = -1;
            for( int yy = ( int ) G.HS.Area.yMin - 1; yy < G.HS.Area.yMax + 1; yy++ )                    // find highest id
            for( int xx = ( int ) G.HS.Area.xMin - 1; xx < G.HS.Area.xMax + 1; xx++ )
            if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
            {
                Unit raft = Controller.GetRaft( new Vector2( xx, yy ) );
                if( raft )
                {
                    if( raft.Control.RaftGroupID > top )
                        top = raft.Control.RaftGroupID + 1;
                    if( idl.Contains( raft.Control.RaftGroupID ) == false )                             // make a list of rafts for optimization
                        idl.Add( raft.Control.RaftGroupID );
                    rrl.Add( raft );
                }
            }

            for( int i = 0; i < rl.Count; i++ )                                                          // zero raft under hero
            {
                rl[ i ].Control.RaftMoveDir = EDirection.NONE;
                rl[ i ].Control.RaftGroupID = -1;
            }
            for( int i = 0; i < rrl.Count; i++ )
            if ( rrl[ i ].Control.RaftGroupID == -1 )                                                   // Updates Raft ID for raft under hero
                 Map.I.SetRaftId( rrl[ i ].Pos, ++top, ref G.HS );
            
            List<int> idl2 = new List<int>();
            for( int i = 0; i < rrl.Count; i++ )
            if( idl2.Contains( rrl[ i ].Control.RaftGroupID ) == false )                               // counts to check if a new raft has been created 
                 idl2.Add( rrl[ i ].Control.RaftGroupID );

            if( to )
            {
                if( idl2.Count > idl.Count )                                                           // anew group has been created
                if( Map.I.GetUnit( ETileType.WATER, Unit.Pos ) )
                    mov = Util.GetTargetUnitDir( OldPos, Unit.Pos );
                if( mov != EDirection.NONE ) // new
                SetRaftDirection( false, to.Control.RaftGroupID, mov );                                // sets move direction for raft group under hero
            }
        }
    }

    #region Trap
    public void UpdateConsecutiveTrapSteps( bool rot )
    {
        Unit fr = Map.I.GetUnit( ETileType.TRAP, OldPos );
        Unit to = Map.I.GetUnit( ETileType.TRAP, Unit.Pos );
        if( Unit.UnitType == EUnitType.HERO )                                                          // Consecutive hero platform steps
        {
            if( to && Unit.Pos != OldPos )
            {
                if( rot == false )
                    Map.I.ConsecutivePlatformSteps++;
            }
            else
                if( to == null )
                    Map.I.ConsecutivePlatformSteps = 0;
        }
    }
    public bool UpdateTrapStepping( int fx = -1, int fy = -1, int tx = -1, int ty = -1 )
    {
        DropTrapGroupList = new List<int>();
        G.Hero.Control.UpdateFreeTrapStepping( fx, fy, tx, ty );
        bool rot = false;
        if( G.Hero.Control.LastAction == EActionType.ROTATE_CCW ||
            G.Hero.Control.LastAction == EActionType.ROTATE_CW ||
            G.Hero.Control.LastAction == EActionType.WAIT ) rot = true;

        Unit fr = Map.I.GetUnit( ETileType.TRAP, LastPos );
        if( fx != -1 )
            fr = Map.I.GetUnit( ETileType.TRAP, new Vector2( fx, fy ) );
        Unit to = Map.I.GetUnit( ETileType.TRAP, Unit.Pos );
        if( tx != -1 )
            to = Map.I.GetUnit( ETileType.TRAP, new Vector2( tx, ty ) );
        if( rot ) fr = Map.I.GetUnit( Unit.Pos, ELayerType.GAIA );
        if( to ) BlockStepping = true;

        bool monst = false;
        if( Unit.UnitType == EUnitType.MONSTER ) monst = true;

        UpdateConsecutiveTrapSteps( rot );

        int level = ( int ) Mathf.Ceil( Map.I.Hero.Control.PlatformWalkingLevel );
        int totsteps = 1 + ( int ) G.Hero.Control.PlatformSteps;
        if( totsteps > 20 ) totsteps = 9999999;

        if( fr == null && to != null )                                                                                           // Restricted exit stuff
        {
            Map.I.PlatformEntranceMove = Map.I.Hero.Control.LastMoveAction;
            Unit snow = Map.I.GetUnit( ETileType.SNOW, LastPos );                                                               // from snow to Platform
            Unit sand = Map.I.GetUnit( ETileType.SAND, LastPos );                                                               // from sand to Platform
            if( snow || sand )
            {
                EDirection dir = Util.GetTargetUnitDir( LastPos, Unit.Pos );
                Map.I.PlatformEntranceMove = ( EActionType ) dir;
            }

            UpdatePlatformEntranceIndicator();                                                                                   // Updates Indicator
        }
        else
        {
            if( to == null )                                                                                                     // Trap Exit
            {
                Map.I.PlatformEntranceMove = EActionType.NONE;
                UpdatePlatformEntranceIndicator();
            }
        }

        if( Map.I.PlatformUpdated == false )                                                                            // Updates Platform counter
            if( to )
            {
                if( to.TileID == ETileType.TRAP )
                {
                    Map.I.PlatformUpdated = true;
                    {
                        if( rot == false || ConsecutiveStillMove > 1 )                                                  // One free rotation allowed
                            PlatformWalkingCounter++;
                    }
                }
            }
            else
            {
                Map.I.PlatformUpdated = true;
                PlatformWalkingCounter = 0;
            }

        if( monst == false )
        if( PlatformWalkingCounter > totsteps )
        if( Manager.I.GameType == EGameType.CUBES )                                                                     // Cube death
        {
            FallingDeath( ETileType.TRAP );
            Map.I.PlatformDeath = true;
            Map.I.Tilemap.SetTile( ( int ) Unit.Pos.x, ( int ) Unit.Pos.y,
            ( int ) ELayerType.GAIA, ( int ) ETileType.PIT );
            Map.I.Tilemap.Build();
        }

        //if( monst == false )
        //if( Quest.CurrentLevel != -1 )                                                                                  // No more steps available, Platform death
        //if( PlatformWalkingCounter > totsteps )
        //{
        //    Map.I.HeroIsDead = true;
        //    Message.CreateMessage( ETileType.HITPOINTS, "Platform Death!",
        //    Map.I.Hero.Pos, new Color( 1, 0, 0, 1 ), true, true, 7 );
        //    Map.I.Hero.Body.ReceiveDamage( 9999, EDamageType.MELEE, Unit, null );
        //    FallingAnimation.CreateAnim( ( int ) ETileType.INACTIVE_HERO, 0, new 
        //    Vector3( fr.Pos.x, fr.Pos.y, -5 ), fr.transform.eulerAngles );
        //    G.Hero.Spr.gameObject.SetActive( false );
        //}

        //float ppFactor = 1;
        //if( Map.I.AreaCleared ) ppFactor = 0.5f;
        //if( level < 6 ) ppFactor = 0;
        //if( Map.I.CurrentArea != -1 && Map.I.CurArea.NumTimesEntered > 1 ) ppFactor = 0;

        if( OldPos.x != -1 && fr && fr.TileID == ETileType.TRAP )                                                                                                            // Update Trap tile id
        {
            int id = Map.I.GateID[ ( int ) fr.Pos.x, ( int ) fr.Pos.y ];
            if( fr.Pos != Unit.Pos || PlatformWalkingCounter > totsteps )
            {
                DropTrap( fr, id, ETileType.TRAP );                                                          // Drops the Free Trap   
                UpdateVineTrapDropping( fr.Pos, id );                                                        // Updates Vine Trap Dropping
            }
            #region old
            //if( level >= 3 )                                                                               // Give Platform Points for Group
            //{
            //    Map.I.CurArea.PlatformGroups++;

            //    float pp = Util.Factorial( 1, platcont ) * ppFactor;
            //    HeroData.I.PlatformPoints += pp;
            //    if( Map.I.CurrentArea != -1 )
            //        Map.I.CurArea.PlatformPoints += pp;

            //    Message.CreateMessage( ETileType.TRAP, "All platforms Down!\nPlatforms: +" + platcont +
            //                                           "\nPlatform Points: +" + pp,
            //    Map.I.Hero.Pos, new Color( 0, 1, 0, 1 ), true, true, 7 );
            //}
            #endregion

            UpdateAllTrapKeyActivation();                                                                    // Key activation
        }
        return false;
    }
    public bool UpdateFreeTrapStepping( int fx = -1, int fy = -1, int tx = -1, int ty = -1 )
    {
        if( Map.I.CurrentMoveTrial == EActionType.ROTATE_CCW ||
            Map.I.CurrentMoveTrial == EActionType.ROTATE_CW ||
            Map.I.CurrentMoveTrial == EActionType.WAIT ||
            Map.I.CurrentMoveTrial == EActionType.SPECIAL ||
            Map.I.CurrentMoveTrial == EActionType.BATTLE )
            return false;
        Unit fr = Map.I.GetUnit( ETileType.FREETRAP, LastPos );
        if( fx != -1 )
            fr = Map.I.GetUnit( ETileType.FREETRAP, new Vector2( fx, fy ) );
        Unit to = Map.I.GetUnit( ETileType.FREETRAP, Unit.Pos );
        //if( tx != -1 )
        //    to = Map.I.GetUnit( ETileType.FREETRAP, new Vector2( tx, ty ) );
        if( fr && fr.Pos.x != -1 && fr && fr.TileID == ETileType.FREETRAP )
        {
            int id = Map.I.GateID[ ( int ) fr.Pos.x, ( int ) fr.Pos.y ];
            //if( to && fr.Pos != to.Pos )
            {
                DropTrap( fr, id, ETileType.FREETRAP );                                                        // Drops the Free Trap
                UpdateVineTrapDropping( fr.Pos, id );                                                          // Updates Vine Trap Droping         
            }
            UpdateAllTrapKeyActivation();                                                                      // Key activation
        }
        return false;
    }
    public void DropTrap( Unit tg, int id, ETileType type, bool fx = true )
    {
        Map.I.Tilemap.SetTile( ( int ) tg.Pos.x, ( int ) tg.Pos.y,                                                           // set tile
        ( int ) ELayerType.GAIA, ( int ) ETileType.NONE );

        Map.I.UpdateTilemap = true;
        Map.I.CountRecordTime = true;
        if( fx )
            MasterAudio.PlaySound3DAtVector3( "Platform Fall", transform.position );                                         // Sound FX

        #region old
        /* if( type == ETileType.TRAP )
        if( exiting == false )                                                                                                // Give TRAP per platform pp bonus
        if( level >= 4 )
            {
                float points = 1;
                if( level >= 6 ) points += 1;
                points *= ppFactor;
                HeroData.I.PlatformPoints += points;
                Map.I.CurArea.PlatformPoints += points;
                Map.I.CurArea.PlatformsDown++;
            }
         */
        #endregion

        if( tg.TileID == type )
        {
            Map.I.SetTile( ( int ) tg.Pos.x, ( int ) tg.Pos.y, ELayerType.GAIA, ETileType.PIT, true );                       // Trap to Pit                
        }

        Map.I.GateID[ ( int ) tg.Pos.x, ( int ) tg.Pos.y ] = id;

        if( DropTrapGroupList.Contains( id ) == false )                                                                      // For key activation test
            DropTrapGroupList.Add( id );

        FallingAnimation.Create( type, new Vector3( tg.Pos.x, tg.Pos.y, -0.18f ), tg.transform.eulerAngles );

        Unit un = Map.I.GetUnit( tg.Pos, ELayerType.MONSTER );                                                               // monster falls
        if( un )
        {
            Map.Kill( un );
            int spr = un.Spr.spriteId;
            if( un.TileID == ETileType.SCARAB ) spr = 63;
            FallingAnimation.CreateAnim( spr, 1,
            new Vector3( tg.Pos.x, tg.Pos.y, -0.18f ), tg.transform.eulerAngles );
            MasterAudio.PlaySound3DAtVector3( "Monster Falling", un.Pos );
        }

        Unit ga2 = Map.I.GetUnit( tg.Pos, ELayerType.GAIA2 );                                                                // Gaia 2 falls NEW!!! test raft x arrow cubes
        if( ga2 )
        if( ga2.TileID != ETileType.DOOR_OPENER )
        if( ga2.TileID != ETileType.DOOR_CLOSER )
        if( ga2.TileID != ETileType.DOOR_SWITCHER )
        {           
            ga2.Kill();
        }

        un = Map.GFU( ETileType.MINE, tg.Pos );                                                                              // mine falls
        if( un )
        {
            FallingAnimation.CreateAnim( un.Spr.spriteId, 1,
            new Vector3( tg.Pos.x, tg.Pos.y, -4 ), tg.transform.eulerAngles );
            un.Kill();
        }
        un = Map.GFU( ETileType.SPIKES, tg.Pos );                                                                            // Drop spikes
        if( un ) Map.I.KillList.Add( un );
        un = Map.GFU( ETileType.MUD_SPLASH, tg.Pos );                                                                        // Kill Mud Splash
        if( un ) Map.I.KillList.Add( un );
    }
    public static void UpdatePlatformEntranceIndicator()
    {
        bool state = false;
        if( Map.I.PlatformEntranceMove != EActionType.NONE )
        {
            Map.I.PlatformExitIndicator.transform.eulerAngles =                                                             // Exit indicator setup
                 Util.GetRotationAngleVector( Util.GetActionDirection( Map.I.PlatformEntranceMove ) );
            state = true;
        }
        Map.I.PlatformExitIndicator.gameObject.SetActive( state );
    }
    public static void UpdateAllTrapKeyActivation()
    {
        for( int i = 0; i < DropTrapGroupList.Count; i++ )
            UpdateTrapKeyActivation( DropTrapGroupList[ i ] );
    }
    public static bool UpdateTrapKeyActivation( int id )
    {
        Sector hs = Map.I.RM.HeroSector;
        int platcont = 0;
        bool down = true;

        for( int y = ( int ) hs.Area.yMin - 1; y < hs.Area.yMax + 1; y++ )                                                        // Check if all traps are down
        for( int x = ( int ) hs.Area.xMin - 1; x < hs.Area.xMax + 1; x++ )
        {
            if( Map.I.GateID[ x, y ] == id )
            {
                if( Map.I.Gaia[ x, y ] && Map.I.Gaia[ x, y ].TileID == ETileType.TRAP ) down = false;
                if( Map.I.Gaia[ x, y ] && Map.I.Gaia[ x, y ].TileID == ETileType.FREETRAP ) down = false;
                platcont++;
            }
        }

        if( down == false ) return false;
        for( int y = ( int ) hs.Area.yMin - 1; y < hs.Area.yMax + 1; y++ )
        for( int x = ( int ) hs.Area.xMin - 1; x < hs.Area.xMax + 1; x++ )
        {
            if( Map.I.Gaia[ x, y ] && Map.I.Gaia[ x, y ].TileID == ETileType.PIT )
            if( Map.I.GateID[ x, y ] == id )
            if( Map.I.Gaia2[ x, y ] )
            if( Map.I.Gaia2[ x, y ].TileID == ETileType.DOOR_OPENER ||
                Map.I.Gaia2[ x, y ].TileID == ETileType.DOOR_SWITCHER ||
                Map.I.Gaia2[ x, y ].TileID == ETileType.DOOR_CLOSER )
            {
                Map.I.ActivateAllDoorKnobs( Map.I.Gaia2[ x, y ].TileID,
                Map.I.Gaia2[ x, y ].Variation, new Vector2( x, y ), Map.I.Gaia2[ x, y ] );
                Map.I.SetTile( x, y, ELayerType.GAIA2, ETileType.NONE, false, true );
                Map.I.UpdateTilemap = true;
            }
        }
        return true;
    }
    public void UpdateVineTrapDropping( Vector2 pos, int id )
    {
        Unit vn = Map.GFU( ETileType.VINES, pos );
        if( vn == null || vn.Activated == false ) return;
        Unit vntg = Map.GFU( ETileType.VINES, G.Hero.Pos );

        Map.I.VineID = new int[ Map.I.Tilemap.width, Map.I.Tilemap.height ];              // Reset data
        List<Unit> vine = new List<Unit>();
        TrapDropList = new List<int>();

        if( vntg != null )                                                               // this lists vine color that should be force dropped since it doesnt exists in the tile hero has moved from
            for( int a = 0; a < vn.Control.VineList.Count; a++ )
                for( int b = 0; b < vntg.Control.VineList.Count; b++ )
                    if( vntg.Control.VineList.Contains( vn.Control.VineList[ a ] ) == false )
                        TrapDropList.Add( vn.Control.VineList[ a ] );

        if( vntg == null || GetTrap( G.Hero.Pos ) == false )                             // landing out of trap step
            TrapDropList.AddRange( vn.Control.VineList );

        SetVineId( pos, pos, 1, ref vine, vn );                                          // Recursivelly find linked vine traps    

        for( int i = 0; i < vine.Count; i++ )
        {
            bool ok = false;
            for( int j = 0; j < vine[ i ].Control.VineList.Count; j++ )                  // force drop crossing multi colored vines
                if( TrapDropList.Contains( vine[ i ].Control.VineList[ j ] ) )
                    ok = true;

            if( ok )
            {
                Unit tp = GetTrap( vine[ i ].Pos );
                if( tp )
                {
                    vine[ i ].Activated = false;                                         // Deactivates vines
                }
                else
                {
                    for( int c = 0; c < TrapDropList.Count; c++ )                        // In this case only removes outside trap vines while leaving crossing ones
                    {
                        if( vine[ i ].Control.VineList.Contains( TrapDropList[ c ] ) )
                            vine[ i ].Control.VineList.Remove( TrapDropList[ c ] );
                    }
                    if( vine[ i ].Control.VineList.Count <= 0 )
                        vine[ i ].Activated = false;
                }

                if( tp && tp.Pos != pos )
                    DropTrap( tp, Map.I.GateID[ ( int ) tp.Pos.x,
                    ( int ) tp.Pos.y ], tp.TileID, false );                              // Drop traps linked by Vines
            }
        }
        Map.I.VineID = null;                                                             // free memory
    }
    private static Unit GetTrap( Vector2 tg )
    {
        Unit tr = Map.I.GetUnit( tg, ELayerType.GAIA );
        if( tr && tr.TileID == ETileType.TRAP ) return tr;
        if( tr && tr.TileID == ETileType.FREETRAP ) return tr;
        return null;
    }
    public bool SetVineId( Vector2 from, Vector2 pos, int id, ref List<Unit> vine, Unit or )
    {
        if( Map.PtOnMap( Map.I.Tilemap, pos ) == false ) return false;
        Unit vn = Map.GFU( ETileType.VINES, pos );
        if( vn == null ) return false;
        if( vn.Activated == false ) return false;
        if( Map.I.VineID[ ( int ) pos.x, ( int ) pos.y ] > 0 ) return false;

        int count2 = 0;
        for( int i = 0; i < TrapDropList.Count; i++ )
            if( vn.Control.VineList.Contains( TrapDropList[ i ] ) ) count2++;
        if( count2 == 0 ) return false;

        Unit vnfr = Map.GFU( ETileType.VINES, from );
        EDirection mov = Util.GetTargetUnitDir( from, pos );
        if( mov != EDirection.NONE )
            if( vnfr && vnfr.Body.PoleSpriteList[ ( int ) mov ].gameObject.activeSelf == false ) return false;

        if( or != vn )
        {
            int count = 0;
            for( int i = 0; i < vn.Control.VineList.Count; i++ )
                if( vn && or.Control.VineList.Contains( vn.Control.VineList[ i ] ) == true )
                    count++;
            if( count == 0 ) return false;
        }
        Map.I.VineID[ ( int ) pos.x, ( int ) pos.y ] = id;
        vine.Add( vn );
        SetVineId( pos, new Vector2( pos.x, pos.y + 1 ), id, ref vine, or );
        SetVineId( pos, new Vector2( pos.x + 1, pos.y + 1 ), id, ref vine, or );
        SetVineId( pos, new Vector2( pos.x + 1, pos.y ), id, ref vine, or );
        SetVineId( pos, new Vector2( pos.x + 1, pos.y - 1 ), id, ref vine, or );
        SetVineId( pos, new Vector2( pos.x, pos.y - 1 ), id, ref vine, or );
        SetVineId( pos, new Vector2( pos.x - 1, pos.y - 1 ), id, ref vine, or );
        SetVineId( pos, new Vector2( pos.x - 1, pos.y ), id, ref vine, or );
        SetVineId( pos, new Vector2( pos.x - 1, pos.y + 1 ), id, ref vine, or );
        return true;
    }
    public bool CheckSymetricPits()
    {
        if( Unit.Body.IsDead == false )                                                                                    // Boulder x Pit direction changing
        {
            Unit over = Map.I.GetUnit( ETileType.PIT, Unit.Pos );
            bool diagonal = Util.IsDiagonal( Unit.Dir );
            if( over )
            {
                Unit front = Map.I.GetUnit( ETileType.PIT, Unit.GetFront() );
                Unit back = Map.I.GetUnit( ETileType.PIT, Unit.Pos + Unit.GetRelativePosition( EDirection.S, 1 ) );
                Unit back2 = Map.I.GetUnit( ETileType.PIT, Unit.Pos + Unit.GetRelativePosition( EDirection.S, 2 ) );
                Unit l1 = Map.I.GetUnit( ETileType.PIT, Unit.Pos + Unit.GetRelativePosition( EDirection.NW, 1 ) );
                Unit r1 = Map.I.GetUnit( ETileType.PIT, Unit.Pos + Unit.GetRelativePosition( EDirection.NE, 1 ) );
                Unit l2 = Map.I.GetUnit( ETileType.PIT, Unit.Pos + Unit.GetRelativePosition( EDirection.W, 1 ) );
                Unit r2 = Map.I.GetUnit( ETileType.PIT, Unit.Pos + Unit.GetRelativePosition( EDirection.E, 1 ) );

                if( !front )
                    if( !back || !back2 || l2 || r2 ) return true;                                               // 3 pits in the back needed for leaving

                if( front ) return false;
                if( l1 && r1 && l2 && r2 ) return true;                                                                     // Symetric pattern: Block Boulder
                if( !l1 && !r1 && l2 && r2 ) return true;
                if( l1 && r1 && !l2 && !r2 ) return true;
            }
        }
        return false;
    }

    public void CheckBoulderAndPitBlockade()
    {
        if( Unit.Body.IsDead == false )                                                                                    // Boulder x Pit direction changing
        {
            bool diagonal = Util.IsDiagonal( Unit.Dir );
            int rot = 0;

            Unit over = Map.I.GetUnit( ETileType.PIT, Unit.Pos );
            if( over )
            {
                Unit front = Map.I.GetUnit( ETileType.PIT, Unit.GetFront() );
                Unit back = Map.I.GetUnit( ETileType.PIT, Unit.Pos + Unit.GetRelativePosition( EDirection.S, 1 ) );
                Unit l1 = Map.I.GetUnit( ETileType.PIT, Unit.Pos + Unit.GetRelativePosition( EDirection.NW, 1 ) );
                Unit r1 = Map.I.GetUnit( ETileType.PIT, Unit.Pos + Unit.GetRelativePosition( EDirection.NE, 1 ) );
                Unit l2 = Map.I.GetUnit( ETileType.PIT, Unit.Pos + Unit.GetRelativePosition( EDirection.W, 1 ) );
                Unit r2 = Map.I.GetUnit( ETileType.PIT, Unit.Pos + Unit.GetRelativePosition( EDirection.E, 1 ) );

                if( diagonal == false )
                {
                    if( front ) return;
                }
                else
                {
                    if( front )
                        if( !l1 && !r1 ) return;
                }

                if( l1 && r1 )
                    if( !l2 && !r2 ) return;

                if( diagonal == true )
                {
                    if( l1 && !r1 ) rot = +1;
                    if( !l1 && r1 ) rot = -1;

                    if( l1 && r1 )
                    {
                        if( l2 && !r2 ) rot = +1;
                        if( !l2 && r2 ) rot = -1;
                    }
                }
                else
                    if( diagonal == false )
                    {
                        if( back )
                        {
                            if( l2 && !r2 ) rot = +2;
                            if( !l2 && r2 ) rot = -2;

                            if( l2 && r2 )
                            {
                                if( l1 && !r1 ) rot = +2;
                                if( !l1 && r1 ) rot = -2;
                            }

                            if( l1 && !r1 && !l2 && !r2 ) rot = +1;                            // Only one exit 45 deegrees
                            if( !l1 && r1 && !l2 && !r2 ) rot = -1;
                        }
                    }

                //if( l1 && !r1 && !l2 && !r2 ) rot = +1;                            // Only one exit 45 deegrees
                //if( !l1 && r1 && !l2 && !r2 ) rot = -1;
                //if( !l1 && !r1 && !l2 &&  r2 ) rot = -2;                           // Only one exit 90 deegrees
                //if( !l1 && !r1 &&  l2 && !r2 ) rot = +2;

                EDirection dr = Map.I.RotateDir( Unit.Dir, rot );                  // Apply Rotation
                if( rot != 0 )
                    Unit.SetVariation( ( int ) dr );
            }
        }
        return;
    }

    public void CheckPitSquare( Vector2 tg )
    {
        Unit pit1 = Map.I.GetUnit( ETileType.PIT, tg );
        Unit pit2 = Map.I.GetUnit( ETileType.PIT, tg + new Vector2( 1, 0 ) );
        Unit pit3 = Map.I.GetUnit( ETileType.PIT, tg + new Vector2( 0, -1 ) );
        Unit pit4 = Map.I.GetUnit( ETileType.PIT, tg + new Vector2( 1, -1 ) );

        if( pit1 && pit2 && pit3 && pit4 )
        {
            Unit.Kill();
            FallingAnimation.Create( ( ETileType ) Unit.Spr.spriteId, Unit.Pos, Unit.transform.eulerAngles );
        }
    }
    #endregion
    #region Snow
    public bool UpdateSnowEntering()
    {
        if( Manager.I.GameType == EGameType.NAVIGATION ) return false;
        Unit un = Map.I.GetUnit( ETileType.SNOW, Unit.Pos );
        if( un == null ) return false;

        float amt = -0.49f;
        Vector2 fact = Vector2.zero;
        switch( G.Hero.Control.LastAction )
        {
            case EActionType.MOVE_N: fact = new Vector2( 0, 1 ); break;
            case EActionType.MOVE_NE: fact = new Vector2( 1, 1 ); break;
            case EActionType.MOVE_E: fact = new Vector2( 1, 0 ); break;
            case EActionType.MOVE_SE: fact = new Vector2( 1, -1 ); break;
            case EActionType.MOVE_S: fact = new Vector2( 0, -1 ); break;
            case EActionType.MOVE_SW: fact = new Vector2( -1, -1 ); break;
            case EActionType.MOVE_W: fact = new Vector2( -1, 0 ); break;
            case EActionType.MOVE_NW: fact = new Vector2( -1, 1 ); break;
        }

        fact.Normalize();
        fact *= amt;
        Vector3 tg = new Vector3( Unit.transform.position.x + fact.x,
        Unit.transform.position.y + fact.y, Unit.transform.position.z );
        Unit.transform.position = tg;
        Unit.Graphic.transform.position = tg;
        SnowEnterFrameCount = 1;
        G.Hero.CircleCollider.enabled = false;
        SlideTimeCount = 0;
        bool res = UpdateBoulderPunch();
        if( !res )
            SnowSlideTowards( OldPos, Unit.Pos, null );
        UpdateTrapStepping();
        Map.I.UpdateFireIgnition( true );
        SnowSliding = true;
        Map.I.AdvanceTurn = false;
        Controller.InputVectorList = new List<Vector3>();
        return true;
    }

    public void SnowSlideTowards( Vector2 from, Vector2 to, Unit bumper )
    {
        float current = Unit.Body.RigidBody.velocity.magnitude;
        current *= 50;
        Unit.Body.RigidBody.velocity = Vector2.zero;
        Vector2 spd = Util.GetTargetUnitVector( from, to );
        spd.Normalize();
        float def = Map.I.RM.RMD.DefaultSnowSpeed;
        if( bumper ) def *= bumper.Control.BoulderPunchPower / 100;
        if( current < def )                                                          // adds speed if bum is tronger
            spd *= def;
        else
            spd *= current;
        Unit.Body.RigidBody.AddForce( spd );
    }

    public void UpdateSnowExiting()
    {
        if( Manager.I.GameType == EGameType.NAVIGATION ) return;
        HeroMovedFromSlidingTerrainToMud = false;
        if( Map.I.GetUnit( ETileType.SNOW, OldPos ) )
            if( Map.I.GetMud( Unit.Pos ) )
            {
                HeroMovedFromSlidingTerrainToMud = true;
            }

        if( SnowSliding )
        {
            Map.I.InvalidateInputTimer = SlideTerrainExitWaitTime;
            MasterAudio.StopAllOfSound( "Snow Slide" );
            Unit.transform.position = new Vector3( ( int ) Unit.Pos.x,
                                                   ( int ) Unit.Pos.y, Unit.transform.position.z );
            Unit.Graphic.transform.localPosition = new Vector2( 0, 0 );
            TurnTime = 5;
            Map.I.AdvanceTurn = true;
            ForceMove = EActionType.WAIT;
            G.Hero.Control.UpdateResourceCollecting( G.Hero.Pos, false );
            ETileType tile = ETileType.NONE;

            bool die = false;
            Unit ga = Map.I.GetUnit( Unit.Pos, ELayerType.GAIA );                                                 // Death for water falling
            if( ga && Controller.GetRaft( Unit.Pos ) == null )
            if( ga.TileID == ETileType.PIT || ga.TileID == ETileType.WATER )
            {
                die = true;
                tile = ga.TileID;
            }

            Unit mn = Map.I.GetUnit( Unit.Pos, ELayerType.MONSTER );
            if( mn ) die = true; 

            if( die )
            {
                FallingDeath( tile );
            }
            Controller.ImpulsionateRaft( OldPos, Unit.Pos );                                          // Impulsionates raft after Snow leaving
        }

        Map.I.HeroSwordCollider.enabled = false;
        Unit.Body.RigidBody.velocity = Vector2.zero;
        G.Hero.CircleCollider.enabled = false;
        SnowSliding = false;
        Controller.InputVectorList = new List<Vector3>();
    }
    public void FallingDeath( ETileType tile )
    {
        if( G.HS.CubeTurnCount < 2 ) return;
        Map.I.StartCubeDeath( false, false, "Hero Death 2" );
        if( tile == ETileType.WATER )
            MasterAudio.PlaySound3DAtVector3( "Water Splash", transform.position );                  // Sound FX  
        FallingAnimation.CreateAnim( G.Hero.Spr.spriteId, 1, new
        Vector3( G.Hero.Pos.x, G.Hero.Pos.y, -5 ), G.Hero.transform.eulerAngles, .4f, 1.1f );
        G.Hero.Spr.gameObject.SetActive( false );
    }
    public bool UpdateSnowSliding()
    {
        if( Manager.I.GameType == EGameType.NAVIGATION ) return false;
        if( Map.I.RM.GameOver ) return false;
        Unit snow = Map.I.GetUnit( ETileType.SNOW, Unit.Pos );
        Unit sand = Map.I.GetUnit( ETileType.SAND, Unit.Pos );
        if( snow == null && sand == null )
        {
            if( Map.I.CheckArrowBlockFromTo( OldPos, Unit.Pos, Unit ) )
            {
                Unit.Body.RigidBody.velocity = Vector2.zero;
                Unit.transform.position = new Vector3( ( int ) OldPos.x,
                ( int ) OldPos.y, Unit.transform.position.z );
                return false;
            }
            UpdateSnowExiting();
            return false;
        }
        if( snow == null ) return false;
        Unit.Body.RigidBody.drag = 0;

        LastVelocity = Unit.Body.RigidBody.velocity;

        SlideTimeCount += Time.deltaTime;                                                                     // Reactivates hero collider which was deactivated to avoid collision right after entering snowto avoid 
        if( SlideTimeCount > 0.1f )
        {
            G.Hero.CircleCollider.enabled = true;
        }
        Map.I.HeroSwordCollider.enabled = false;
        if( Map.I.IsHeroMeleeAvailable() )
            Map.I.HeroSwordCollider.enabled = true;                                                           // Enables hero sword collider

        if( Manager.I.GameType == EGameType.CUBES )
            MasterAudio.PlaySound3DFollowTransform( "Snow Slide", transform );
        Vector2 add = new Vector2( 0, 0 );                                                                    // Add Step rigidbody force
        if( Item.GetNum( ItemType.Res_SnowStep ) >= 1 )
        {
            if( InputVectorList.Count > 0 )
            {
                add = InputVectorList[ 0 ];
                InputVectorList.RemoveAt( 0 );
            }
            UpdateFrictionRotation( 1, add );
            Vector2 to = Unit.Pos + add;
            bool narrow = Map.CheckNarrowPassage( Unit.Pos, to );

            if( Map.I.GetUnit( ETileType.ARROW, Unit.Pos ) )
                if( Map.I.CheckArrowBlockFromTo( Unit.Pos, Unit.Pos + add, Unit ) )                                // Arrow block force dir
                    add = Vector2.zero;

            if( add != new Vector2( 0, 0 ) )
            if( Controller.PunchOrigin == new Vector2( -1, -1 ) )
                {
                    add.Normalize();
                    add *= Map.I.RM.RMD.SnowStepFactor;
                    Unit.Body.RigidBody.AddForce( add );
                    Item.AddItem( ItemType.Res_SnowStep, -1 );

                    if( narrow )                                                                                   // narrow passage traverse
                    if( Unit.CanMoveFromTo( false, Unit.Pos, to, Unit ) )
                    if( Map.I.InvalidateInputTimer <= 0 )
                    {
                        Unit.transform.position = new Vector3( to.x, to.y, Unit.transform.position.z );
                        Map.I.InvalidateInputTimer = SlideTerrainExitWaitTime;
                    }
                }
        }
        else
        {
            Controller.InputVectorList = new List<Vector3>();                                                            // Empty move buffer
        }

        if( SnowEnterFrameCount == 0 )
        if( Map.I.GetUnit( ETileType.SAND, OldPos ) )
        if( Map.I.GetUnit( ETileType.SNOW, Unit.Pos ) )
        {
            Unit.Body.RigidBody.velocity =
            Unit.Body.RigidBody.velocity.normalized * ( Map.I.RM.RMD.DefaultSnowSpeed / 50 );                            //From Sand to Snow minimum Speed Limit
        }

        SnowEnterFrameCount++;
        if( Unit.Body.RigidBody.velocity.magnitude > Map.I.RM.RMD.SnowSpeedLimit )
            Unit.Body.RigidBody.velocity = Unit.Body.RigidBody.velocity.normalized * Map.I.RM.RMD.SnowSpeedLimit;        // Snow Speed Limit

        Unit.LevelTxt.gameObject.SetActive( false );                                                                     // Updates Step text mesh
        if( snow && Item.GetNum( ItemType.Res_SnowStep ) >= 1 )
        {
            Unit.LevelTxt.gameObject.SetActive( true );
            Unit.LevelTxt.text = "+" + +( Item.GetNum( ItemType.Res_SnowStep ) );
        }

        SnowSliding = true;
        Map.I.ConsecutivePlatformSteps = 0;
        Map.I.PlatformExitIndicator.gameObject.SetActive( false );
        int posx = 0, posy = 0;
        Map.I.Tilemap.GetTileAtPosition( Unit.transform.position, out posx, out posy );

        TileChanged = false;

        UpdateBoulderPunch();                                                              // Boulder Punch

        if( Unit.Pos == new Vector2( posx, posy ) )                                        // Stores old position for retrieval in case of arrow block
            OldSnowSlidingPosition = Unit.transform.position;

        if( Unit.Pos != new Vector2( posx, posy ) )
        {
            snow = Map.I.GetUnit( ETileType.SNOW, Unit.Pos );
            Unit arrow = Map.I.GetUnit( ETileType.ARROW, new Vector2( posx, posy ) );
            Unit ga2 = Map.I.GetUnit( new Vector2( posx, posy ), ELayerType.GAIA2 );

            Vector2 dir = Unit.Body.RigidBody.velocity;
            float angle = Mathf.Atan2( dir.y, dir.x ) * Mathf.Rad2Deg;
            EDirection dr = Util.GetAngleDirection( angle );
            Vector2 tt = new Vector2( posx, posy );

            if( arrow )
            {
                if( OldPos != new Vector2( posx, posy ) )
                if( Map.I.CheckArrowBlockFromTo( Unit.Pos, tt, Unit ) )
                {
                    ArrowSlideStop( true );
                    return true;
                }
            }

            TileChanged = true;
            OldPos = Unit.Pos;
            LastPos = Unit.Pos;
            AnimationOrigin = Unit.Pos;
        }
        Unit.Pos = new Vector2( ( int ) posx, ( int ) posy );
        Area.UpdateAiTarget( Unit.Pos, true );

        if( TileChanged )
        {
            UpdatePushedHero();
            CheckHeroSlideSuicide();
            Map.I.UpdateKickingBoots( OldPos, Unit.Pos );                                                 // Update Kicking Boots

            snow = Map.I.GetUnit( ETileType.SNOW, Unit.Pos );
            sand = Map.I.GetUnit( ETileType.SAND, Unit.Pos );
            if( PunchOrigin != Unit.Pos )
                PunchOrigin = new Vector2( -1, -1 );
            if( snow == null && sand == null )
            {
                if( Map.I.CheckArrowBlockFromTo( OldPos, Unit.Pos, Unit ) )
                {
                    Unit.Body.RigidBody.velocity = Vector2.zero;
                    return false;
                }

                TurnTime = 0;
                Map.I.TimeKeyPressing = 0;
                UpdateSnowExiting();
                UpdateTrapStepping();
            }
            else
                SnowSliding = true;

            Map.I.UpdateFogOfWar( true );
            bool res = UpdateSpiderStepping( OldPos, Unit.Pos );
            if( res == false ) Map.I.StartCubeDeath();
        }

        if( Unit.Body.RigidBody.velocity == Vector2.zero )                                                // no sound FX if stopped
            MasterAudio.StopAllOfSound( "Snow Slide" );
        return true;
    }

    public bool UpdateBoulderPunch()
    {
        EDirection punchdir = EDirection.NONE;
        Unit puncher = null;
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = Unit.Pos + Manager.I.U.DirCord[ i ];
            Unit boul = Map.I.GetUnit( ETileType.BOULDER, tg );
            if( boul && boul.GetFront() == Unit.Pos )
            {
                puncher = boul;
                punchdir = boul.Dir;
                break;
            }
        }

        if( punchdir != EDirection.NONE )
        {
            if( PunchOrigin == new Vector2( -1, -1 ) )
            {
                MasterAudio.PlaySound3DAtVector3( "Bump", transform.position );
                Unit.transform.position = new Vector3( Unit.Pos.x, Unit.Pos.y, Unit.transform.position.z );
                Unit.Body.RigidBody.velocity = Vector2.zero;
                Controller.InputVectorList = new List<Vector3>();
                Controller.PunchOrigin = puncher.Pos;
                Vector2 dir = Util.GetRelativePosition( EDirection.N, puncher.Dir );
                SnowSlideTowards( puncher.Pos, puncher.Pos + dir, puncher );
                iTween.PunchPosition( puncher.Graphic.gameObject, dir * .4f, .5f );
                return true;
            }
        }
        return false;
    }
    public void UpdatePushedHero( bool updateFireIgnition = true )
    {
        Map.I.CalculateAIData();
        Unit orb = Map.I.GetUnit( ETileType.ORB, G.Hero.GetFront() );
        if( orb ) orb.StrikeTheOrb( G.Hero );
        if( updateFireIgnition )
            Map.I.UpdateFireIgnition( true );
        Map.I.UpdateAllMonsterWakingUp();
        for( int i = 0; i < 8; i++ )
        {
            Vector2 desttg = G.Hero.Pos + Manager.I.U.DirCord[ i ];                                    // Lever mine rotation
            Unit mine = Map.GFU( ETileType.MINE, desttg );
            if( mine != null )
                if( mine.Body.MineHasLever() )
                {
                    UpdateLeverMine( G.Hero, true, G.Hero.Control.OldPos, G.Hero.Pos );
                }
        }
        Map.I.UpdateBoomerangs( true );
    }

    public void CheckHeroSlideSuicide()
    {
        Unit mn = Map.I.GetUnit( Unit.Pos, ELayerType.MONSTER );
        if( mn && mn.ValidMonster )
        {
            Map.I.StartCubeDeath();
            G.Hero.Spr.gameObject.SetActive( false );
        }
    }

    void OnCollisionEnter2D( Collision2D col )
    {
        if( SnowSliding == false && SandSliding == false ) return;
        int posx = 0, posy = 0;
        Map.I.Tilemap.GetTileAtPosition( col.transform.position, out posx, out posy );
        List<Unit> unl = Map.I.GF( new Vector2( posx, posy ), ETileType.ALGAE );

        if( unl != null )
        for( int i = 0; i < unl.Count; i++ )
            {
                if( unl[ i ].TileID == ETileType.ALGAE )                                                          // Algae collision
                {
                    BabyData data = col.transform.gameObject.GetComponent<BabyData>();
                    if( data ) data.ProcessLandCollision( posx, posy );
                }  
            }

        if( SandSliding )
        {
            Vector2 fr = new Vector2( posx, posy ) - new Vector2( InputVector.x, InputVector.y );
            UpdateMudObjectPush( true, fr, new Vector2( posx, posy ) );                                          // Mud Object push x sand
        }
        if( SnowSliding )
        {
            EDirection car = Util.GetCardinalDirection( LastVelocity );
            if( car != EDirection.NONE )
            {
                Vector2 ddd = Manager.I.U.DirCord[ ( int ) car ];
                Vector2 fr = new Vector2( posx, posy ) - ddd;
                bool res = UpdateMudObjectPush( true, fr, new Vector2( posx, posy ) );                           // Mud Object push x snow
                if( res ) G.Hero.Body.RigidBody.velocity = LastVelocity;
            }
        }
        Unit un = Map.I.GetUnit( new Vector2( posx, posy ), ELayerType.MONSTER );
        if( un )
        {
            if( un.TileID == ETileType.ORB )                                                                     // Orb bump FX
            {
                if( iTween.Count( un.Body.EffectList[ 2 ].gameObject ) == 0 )
                {
                    iTween.ShakePosition( un.Body.EffectList[ 2 ].gameObject, new Vector3( .3f, .3f, 0 ), 0.6f );
                    un.Body.TweenTime = 0.65f;
                }

                MasterAudio.PlaySound3DAtVector3( "Orb Strike", transform.position );
                BumperTimeCount = 0;
                BoulderBumpDir = EDirection.NONE;
            }
            if( un.TileID == ETileType.ITEM )                                                                     // Item dome bump FX
            {
                if( un.Body.GetMiniDomeTotTime() > 0 )
                {
                    un.Control.StartMiniDomeRotation();                                                          // Reactivate minidome 
                }
            }

            if( un.TileID == ETileType.SCORPION )
            {
                Map.I.AdvanceTurn = true;
                ForceMove = EActionType.WAIT;
            }

            if( un.TileID == ETileType.MINE )
                Controller.UpdateMining( G.Hero.Pos, new Vector2( posx, posy ) );                                     // Sliding Mining 
            if( un.TileID == ETileType.ALTAR )
                Altar.UpdateAltarBump( new Vector2( posx, posy ) );
        }
        else
            un = Map.I.GetUnit( new Vector2( posx, posy ), ELayerType.GAIA );

        if( un && un.TileID == ETileType.BARRICADE )                                                                  // Barricade Destruction
            if( BarricadeBumpTimeCount >= Map.I.RM.RMD.BarricadeDestroyHeroWaitTime )
            {
                un.SetBarricadeSize( true, true, un.Variation - 1, true, G.Hero );
                un.UpdateBarricadeAnimation( new Vector2( posx, posy ) );
                un.UpdateBurningBarricadeDamage( new Vector2( posx, posy ), true );
                BumperTimeCount = 0;
                BoulderBumpDir = EDirection.NONE;
            }

        Unit ga2 = Map.I.GetUnit( new Vector2( posx, posy ), ELayerType.GAIA2 );                                      // Fire step death
        if( ga2 && ga2.TileID == ETileType.FIRE )
        {
            if( ga2.Body.FireIsOn )
            {
                if( Unit.name == "Hero" ) Map.I.StartCubeDeath();
                MasterAudio.PlaySound3DAtVector3( "Fire Ignite", Map.I.Hero.Pos );                                   // fx
            }
        }
    }

    #endregion
    #region Sand
    public bool UpdateSandEntering()
    {
        if( Manager.I.GameType == EGameType.NAVIGATION ) return false;
        Unit un = Map.I.GetUnit( ETileType.SAND, Unit.Pos );
        if( un == null ) return false;
        float amt = -0.49f;
        Vector2 fact = Vector2.zero;
        switch( G.Hero.Control.LastAction )
        {
            case EActionType.MOVE_N: fact = new Vector2( 0, 1 ); break;
            case EActionType.MOVE_NE: fact = new Vector2( 1, 1 ); break;
            case EActionType.MOVE_E: fact = new Vector2( 1, 0 ); break;
            case EActionType.MOVE_SE: fact = new Vector2( 1, -1 ); break;
            case EActionType.MOVE_S: fact = new Vector2( 0, -1 ); break;
            case EActionType.MOVE_SW: fact = new Vector2( -1, -1 ); break;
            case EActionType.MOVE_W: fact = new Vector2( -1, 0 ); break;
            case EActionType.MOVE_NW: fact = new Vector2( -1, 1 ); break;
        }

        fact.Normalize();
        fact *= amt;
        Vector3 tg = new Vector3( Unit.transform.position.x + fact.x,
        Unit.transform.position.y + fact.y, Unit.transform.position.z );
        Unit.transform.position = tg;
        Unit.Graphic.transform.position = tg;
        G.Hero.CircleCollider.enabled = false;
        SandSlideTowards( OldPos, Unit.Pos, null );
        UpdateTrapStepping();
        Map.I.UpdateFireIgnition( true );
        SandSliding = true;
        SlideTimeCount = 0;
        Map.I.HeroSwordCollider.enabled = true;
        return true;
    }

    public void SandSlideTowards( Vector2 from, Vector2 to, Unit bumper )
    {
        float current = Unit.Body.RigidBody.velocity.magnitude;
        current *= 50;
        Unit.Body.RigidBody.velocity = Vector2.zero;
        Vector2 spd = Util.GetTargetUnitVector( from, to );
        spd.Normalize();
        float def = 200;// Map.I.RM.RMD.DefaultSandSpeed;
        if( bumper )
        {
            BumperTimeCount = 0;
            BoulderBumpDir = Util.GetTargetUnitDir( from, to );
            def *= bumper.Control.BoulderPunchPower / 100;
            def *= 10;
            if( BumpPositionList.Contains( to ) == false )
                BumpPositionList.Add( to );
            else
                def = 0;
        }
        else
        {
            Unit.transform.position += new Vector3( spd.x, spd.y, 0 ) * .05f;
            return;
        }

        if( current < def )                                                          // adds speed if bum is tronger
            spd *= def;
        else
            spd *= current;

        if( def != 0 )
        {
            Unit.Body.RigidBody.AddForce( spd );
        }
    }
    public void UpdateSandExiting()
    {
        if( Manager.I.GameType == EGameType.NAVIGATION ) return;
        HeroMovedFromSlidingTerrainToMud = false;
        if( Map.I.GetUnit( ETileType.SAND, OldPos ) )
        if( Map.I.GetMud( Unit.Pos ) )
        {
            HeroMovedFromSlidingTerrainToMud = true;
        }

        Unit tosnow = Map.I.GetUnit( ETileType.SNOW, Unit.Pos );

        if( SandSliding )
        {
            Map.I.InvalidateInputTimer = .1f;
            MasterAudio.StopAllOfSound( "Sand Slide" );
            if( tosnow == null )
            {
                Unit.transform.position = new Vector3( ( int ) Unit.Pos.x,
                                                       ( int ) Unit.Pos.y, Unit.transform.position.z );
                Unit.Graphic.transform.localPosition = new Vector3( 0, 0 );
                TurnTime = 5;
                Map.I.AdvanceTurn = true;
                G.Hero.Control.UpdateResourceCollecting( G.Hero.Pos, false );
            }
            bool die = false;
            ETileType tile = ETileType.NONE;
            Unit ga = Map.I.GetUnit( Unit.Pos, ELayerType.GAIA );                                                 // Death for water falling
            if( ga && Controller.GetRaft( Unit.Pos ) == null )
            if( ga.TileID == ETileType.PIT || ga.TileID == ETileType.WATER )
            {
                tile = ga.TileID;
                die = true;
            }

            Unit mn = Map.I.GetUnit( Unit.Pos, ELayerType.MONSTER );
            if( mn ) die = true;

            if( die )
            {
                FallingDeath( tile );
            }
            Controller.ImpulsionateRaft( OldPos, Unit.Pos );                                          // Impulsionates raft after Sand leaving
        }

        if( tosnow == null )
            Unit.Body.RigidBody.velocity = Vector2.zero;

        Map.I.HeroSwordCollider.enabled = false;
        G.Hero.CircleCollider.enabled = false;
        SandSliding = false;
        Controller.InputVectorList = new List<Vector3>();
    }

    public bool UpdateSandSliding()
    {
        if( Map.I.RM.GameOver ) return false;
        if( Map.I.FreeCamMode ) { Unit.Body.RigidBody.velocity = Vector3.zero; return false; }
        if( Manager.I.GameType == EGameType.NAVIGATION ) return false;
        Unit Sand = Map.I.GetUnit( ETileType.SAND, Unit.Pos );
        if( Sand == null )
        {
            if( Map.I.CheckArrowBlockFromTo( OldPos, Unit.Pos, Unit ) )
            {
                Unit.Body.RigidBody.velocity = Vector2.zero;
                Unit.transform.position = new Vector3( ( int ) OldPos.x,
                ( int ) OldPos.y, Unit.transform.position.z );
                return false;
            }
            UpdateSandExiting();
            return false;
        }
        if( Manager.I.GameType == EGameType.CUBES )
            MasterAudio.PlaySound3DFollowTransform( "Sand Slide", transform );
        MasterAudio.StopAllOfSound( "Snow Slide" );
        Unit.Body.RigidBody.drag = 80;

        SlideTimeCount += Time.deltaTime;                                        // Reactivates hero collider which was deactivated to avoid collision right after entering sand avoid . on sabd this allows diagonal perfection slide good for puzzles
        if( SlideTimeCount > 0.1f )
        {
            G.Hero.CircleCollider.enabled = true;
        }

        float bumptime = 0.7f;
        if( BumperTimeCount < bumptime ) Unit.Body.RigidBody.drag = 0.15f;

        BumperTimeCount += Time.fixedDeltaTime;

        Vector2 add = InputVector;

        UpdateFrictionRotation( 0, add );

        Vector2 to = Unit.Pos + add;
        bool narrow = Map.CheckNarrowPassage( Unit.Pos, to );

        if( BumperTimeCount < bumptime )
        {
            EDirection dr = Util.GetTargetUnitDir( new Vector2( 0, 0 ), add );
            if( dr != EDirection.NONE )
                if( BoulderBumpDir == Util.GetInvDir( dr ) )
                    add = Vector2.zero;
        }

        Unit arrow = Map.I.GetUnit( ETileType.ARROW, Unit.Pos );
        if( add != Vector2.zero )
            if( arrow )
            {
                if( Map.I.CheckArrowBlockFromTo( Unit.Pos, Unit.Pos + add,
                    null, true, -2, ( int ) ArrowOutLevel ) )                                                 // Over Arrow block force dir
                    add = Vector2.zero;
            }

        if( add != Vector2.zero )
        {
            Unit.Body.RigidBody.drag = 0;
            MasterAudio.PlaySound3DFollowTransform( "Sand Step", transform );
            add.Normalize();
            add *= 100;

            Unit.Body.RigidBody.AddForce( add );
            if( narrow )                                                                                   // narrow passage traverse
            if( Unit.CanMoveFromTo( false, Unit.Pos, to, Unit ) )
            if( Map.I.InvalidateInputTimer <= 0 )
            {
                Unit.transform.position = new Vector3( to.x, to.y, Unit.transform.position.z );
                Map.I.InvalidateInputTimer = .4f;
                return true;
            }
        }
        else
        {
            MasterAudio.StopAllOfSound( "Sand Slide" );
        }

        float lim = 3;
        SnowEnterFrameCount = 0;
        if( Unit.Body.RigidBody.velocity.magnitude > lim )
            Unit.Body.RigidBody.velocity = Unit.Body.RigidBody.velocity.normalized * lim;                                // Sand Speed Limit

        Map.I.HeroSwordCollider.enabled = false;
        if( Map.I.IsHeroMeleeAvailable() )
            Map.I.HeroSwordCollider.enabled = true;                                                                      // Enables hero sword collider

        SandSliding = true;
        Map.I.ConsecutivePlatformSteps = 0;
        Map.I.PlatformExitIndicator.gameObject.SetActive( false );
        int posx = 0, posy = 0;
        Map.I.Tilemap.GetTileAtPosition( Unit.transform.position, out posx, out posy );
        TileChanged = false;

        EDirection punchdir = EDirection.NONE;
        Unit puncher = null;
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = Unit.Pos + Manager.I.U.DirCord[ i ];
            Unit boul = Map.I.GetUnit( ETileType.BOULDER, tg );
            if( boul && boul.GetFront() == Unit.Pos )
            {
                puncher = boul;
                punchdir = boul.Dir;
                break;
            }
        }

        if( punchdir != EDirection.NONE )
        {
            if( iTween.Count( puncher.Graphic.gameObject ) == 0 )
            {
                MasterAudio.PlaySound3DAtVector3( "Bump", transform.position );
                Unit.transform.position = new Vector3( Unit.Pos.x, Unit.Pos.y, Unit.transform.position.z );
                Vector2 dir = Util.GetRelativePosition( EDirection.N, puncher.Dir );
                iTween.PunchPosition( puncher.Graphic.gameObject, Unit.Body.RigidBody.velocity * 0.1f, .5f );
                SandSlideTowards( puncher.Pos, puncher.Pos + dir, puncher );
            }
        }

        if( Unit.Pos == new Vector2( posx, posy ) )                                       // Stores old position for retrieval in case of arrow block
            if( add != Vector2.zero )
                OldSandSlidingPosition = Unit.transform.position;

        if( Unit.Pos != new Vector2( posx, posy ) )
        {
            Sand = Map.I.GetUnit( ETileType.SAND, Unit.Pos );
            arrow = Map.I.GetUnit( ETileType.ARROW, new Vector2( posx, posy ) );
            Unit ga2 = Map.I.GetUnit( new Vector2( posx, posy ), ELayerType.GAIA2 );

            if( arrow )
            {
                if( OldPos != new Vector2( posx, posy ) )
                    if( Map.I.CheckArrowBlockFromTo( Unit.Pos, new Vector2( posx, posy ), Unit ) )
                    {
                        ArrowSlideStop( false );
                        return true;
                    }
            }

            TileChanged = true;
            OldPos = Unit.Pos;
            LastPos = Unit.Pos;
            AnimationOrigin = Unit.Pos;
        }
        Unit.Pos = new Vector2( ( int ) posx, ( int ) posy );
        Area.UpdateAiTarget( Unit.Pos, true );

        if( TileChanged )
        {
            UpdatePushedHero();
            CheckHeroSlideSuicide();
            Map.I.UpdateKickingBoots( OldPos, Unit.Pos );                                                 // Update Kicking Boots

            Map.I.AdvanceTurn = true;
            Sand = Map.I.GetUnit( ETileType.SAND, Unit.Pos );
            if( Sand == null )
            {
                if( Map.I.CheckArrowBlockFromTo( OldPos, Unit.Pos, Unit ) )
                {
                    Unit.Body.RigidBody.velocity = Vector2.zero;
                    return false;
                }
                TurnTime = 0;
                Map.I.TimeKeyPressing = 0;
                UpdateSandExiting();
                UpdateTrapStepping();
            }
            else
                SandSliding = true;

            for( int i = BumpPositionList.Count - 1; i >= 0; i-- )
                if( BumpPositionList[ i ] != Unit.Pos )
                    BumpPositionList.RemoveAt( i );

            Map.I.UpdateFogOfWar( true );
            bool res = UpdateSpiderStepping( OldPos, Unit.Pos );
            if( res == false ) Map.I.StartCubeDeath();
        }
        return true;
    }

    public void UpdateFrictionRotation( int id, Vector2 add )
    {
        return;
        Unit.Body.RigidBody.freezeRotation = true;
        float cramps = Item.GetNum( ItemType.Res_Cramps );
        if( cramps <= 0 ) return;

        Unit.Spr.transform.localEulerAngles = new Vector3( 0, 0, 0 );

        if( add == Vector2.zero )
        {
            float angle = Unit.transform.eulerAngles.z;
            EDirection dr1 = Util.GetAngleDirection( angle );
            G.Hero.RotateTo( dr1 );
            G.Hero.Dir = dr1;
            G.Hero.transform.eulerAngles = Util.GetRotationAngleVector( dr1 );
            G.Hero.Body.Shadow.transform.rotation = G.Hero.transform.rotation;
        }

        Unit.Body.RigidBody.freezeRotation = false;
        Map.I.HeroCircleCollider.sharedMaterial = Map.I.HeroPhysicsMaterial[ id ];
    }
    public void ArrowSlideStop( bool snow )
    {
        Unit.Body.RigidBody.velocity = Vector2.zero;
        if( snow )
            Unit.transform.position = OldSnowSlidingPosition;
        else
            Unit.transform.position = OldSandSlidingPosition;
        int tgx = 0; int tgy = 0;
        Map.I.Tilemap.GetTileAtPosition( Unit.transform.position, out tgx, out tgy );
        Unit.Pos = new Vector2( tgx, tgy );
        OldPos = Unit.Pos;
        LastPos = Unit.Pos;
    }

    #endregion
}



