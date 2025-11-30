using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;

public class FishingObject : MonoBehaviour
{
    #region Variables
    public tk2dSprite Sprite, Sprite2;
    public tk2dTextMesh Text;
    public Vector2 TilePos = new Vector2( -1, -1 );
    public Vector2 OldTilePos = new Vector2( -1, -1 );
    public Vector3 OldPosition = new Vector3( -1, -1, 0 );
    public Vector3 ForcedMoveDir = Vector3.zero;
    public Rigidbody2D RigidBody;
    public int HookType = -1;
    public int Type_3_MergeCount = 0;
    public int Type_2_MergeCount = 0;
    public int Type_1_MergeCount = 0;
    public float EnterIntTileTimerCount = 0;
    public float ClosestFishDistance = 1000;
    public float OrbHitTimeCounter = 0;
    public float ForcedMoveStuckTime = 0;
    public Unit ClosestFish = null;
    public List<Unit> TargetFishList = null, OldTargetFishList = null;
    public Vector2 ArrowCheckedPos = new Vector2( -1, -1 );
    public Vector3 Delta, Dest;
    public float TileDistanceTravelled;
    #endregion
    public void Init()
    {
        if( gameObject.activeSelf )
            Text.text = "";
        OldTilePos = new Vector2( -1, -1 );
        OldPosition = new Vector3( -1, -1, 0 );
        ForcedMoveDir = Vector3.zero;
        ArrowCheckedPos = new Vector2( -1, -1 );
        OrbHitTimeCounter = 0;
        ForcedMoveStuckTime = 0;
        Delta = Vector3.zero;
        TargetFishList = new List<Unit>();
        OldTargetFishList = new List<Unit>();
        TileDistanceTravelled = 0;
        Sprite2.gameObject.SetActive( false );
    }

    public void FixedUpdate()
    {
        OldPosition = transform.position; 

        if( Dest != Vector3.zero )
            RigidBody.MovePosition( Dest );
        else
        if( Delta != Vector3.zero )
            RigidBody.MovePosition( transform.position + Delta * Time.fixedDeltaTime );     
    }

    public static FishingObject Add( Vector2 pos, int type, Vector2 bnpos )
    {
        int id = -1;
        for( int i = 0; i < Map.I.HookList.Count; i++ )
        if( Map.I.HookList[ i ].gameObject.activeSelf == false )
        {
            if( type == -1 && i == 0 ) { }                                                  // No extra hook takes main hoon ID == 0
            else
            {
                id = i;
                break;
            }
        }
        if( id == -1 ) return null;

        if( pos.x == -1 )
        {
            if( Map.I.PondID == null ) Map.I.UpdatePondID();
            int pond = Map.I.PondID[ ( int ) bnpos.x, ( int ) bnpos.y ];
            List<Vector2> pl = FishingObject.GetRandomWaterObjPosition( pond );
            if( pl != null && pl.Count > 0 )
            {
                int rid = Random.Range( 0, pl.Count );
                pos = pl[ rid ];
            }
            MasterAudio.PlaySound3DAtVector3( "Water Splash", G.Hero.transform.position );
        }

        FishingObject f = Map.I.HookList[ id ];
        if( type == -1 ) 
            type = Random.Range( 1, 4 );
        f.gameObject.SetActive( true );
        f.transform.position = pos;
        f.HookType = type;
        f.Type_1_MergeCount = 0;
        f.Type_2_MergeCount = 0;
        f.Type_3_MergeCount = 0;
        f.ClosestFishDistance = 1000;
        f.ClosestFish = null;
        f.TargetFishList = new List<Unit>();
        f.OldTargetFishList = new List<Unit>();
        f.OldTilePos = pos;
        f.OldPosition = pos;
        f.ForcedMoveDir = Vector3.zero;
        f.ArrowCheckedPos = new Vector2( -1, -1 );
        f.OrbHitTimeCounter = 0;
        f.ForcedMoveStuckTime = 0;
        f.Delta = Vector3.zero;      
        f.TileDistanceTravelled = 0;
        f.Dest = Vector2.zero;
        return f;
    }

    public static List<Vector2> GetRandomWaterObjPosition( int pond )
    {
        if( Map.I.PondID == null ) Map.I.UpdatePondID();
        Sector s = Map.I.RM.HeroSector;
        List<Vector2> pl = new List<Vector2>();
        for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )
        for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
        if ( Map.I.PondID[ xx, yy ] == pond )
        if ( Map.I.GetUnit( ETileType.WATER, new Vector2( xx, yy ) ) )
            {
                pl.Add( new Vector2( xx, yy ) );
            }
        return pl;
    }

    public void UpdateHookMerging( int h )
    {
        if( G.HS.HookMergeEnabled == false ) return;
        for( int j = 0; j < Map.I.HookList.Count; j++ )
        if ( Map.I.HookList[ j ].gameObject.activeSelf )
        if ( h != j )
        if ( HookType == Map.I.HookList[ j ].HookType )
            {
                float dist = Vector2.Distance( transform.position, Map.I.HookList[ j ].transform.position );
                float max = .2f;
                if( dist < max )
                {
                    FishingObject kill = this;
                    FishingObject stay = Map.I.HookList[ j ];
                    if( Map.I.HookList[ j ] == Map.I.MainHook )
                    {
                        kill = Map.I.HookList[ j ];
                        stay = this;
                    }

                    if( stay.HookType == 1 ) stay.Type_1_MergeCount++;
                    if( stay.HookType == 2 ) stay.Type_2_MergeCount++;
                    if( stay.HookType == 3 ) stay.Type_3_MergeCount++;

                    if( stay.HookType > 1 ) stay.HookType--;

                    stay.Type_1_MergeCount += kill.Type_1_MergeCount;
                    stay.Type_2_MergeCount += kill.Type_2_MergeCount;
                    stay.Type_3_MergeCount += kill.Type_3_MergeCount;

                    if( stay == Map.I.MainHook || kill == Map.I.MainHook )
                    {
                        int tot = stay.Type_1_MergeCount * 1;
                        tot += stay.Type_2_MergeCount * 2;
                        tot += stay.Type_3_MergeCount * 3;
                        G.HS.FishingHookExtraLevel += tot;
                        stay.Type_1_MergeCount = stay.Type_2_MergeCount = stay.Type_3_MergeCount = 0;
                        Message.CreateMessage(ETileType.FISHING_POLE, "Extra Hook Level: +" + tot, stay.TilePos, Color.green );
                    }
                    Controller.CreateMagicEffect( stay.transform.position );                                                                   // Create Magic effect
                    Water.CreateExtraHook( 1, kill.HookType, kill.TilePos );
                    kill.Deactivate( false );
                    MasterAudio.PlaySound3DAtVector3( "Raft Merge", TilePos );
                    UI.I.UpdateInfoPanel( true );
                }
            }
    }

    public void Deactivate( bool fx = true )
    {
        if( fx ) Map.I.CreateExplosionFX( transform.position );
        gameObject.SetActive( false );
        if( Map.I.MainHook.gameObject.activeSelf == false )                                               // set new main hook
        for( int sz = 1; sz <= 3; sz++ )
        for( int j = 0; j < Map.I.HookList.Count; j++ )
        if ( Map.I.HookList[ j ].gameObject.activeSelf )
        if ( Map.I.HookList[ j ].HookType == sz )
        {
            Map.I.MainHook = Map.I.HookList[ j ];
            return;
        }
    }

    public void UpdateForcedMovement( ref Vector3 add, int x, int y, Vector3 vec, Vector3 vecn )
    {
        Sprite2.gameObject.SetActive( false );
        if( ForcedMoveDir == Vector3.zero ) return;
        Sprite2.gameObject.SetActive( true );
        EDirection dr = Util.GetTargetUnitDir( Vector2.zero, ForcedMoveDir );
        Sprite2.transform.eulerAngles = Util.GetRotationAngleVector( dr );
        add = ForcedMoveDir.normalized;                                                                   // Next position calculation   
        EDirection back = Util.GetRelativeDirection( 4, 
        Util.GetTargetUnitDir( Vector2.zero, ForcedMoveDir ) );
        bool slowdown = true;                                                                       // Can harpoon be slowed down?
        if( slowdown || dr != back )
            add += vecn * Time.deltaTime * Map.I.RM.RMD.HarpoonMoveSensitivity;       
        
        Vector3 to = new Vector3( x, y, 0 ) + ForcedMoveDir;
       
        bool stop = UpdatePassingOrbRebound( to );                                                  // Passing orb rebound   

        if( TileDistanceTravelled > .75f )
        if( Map.I.GetUnit( ETileType.WATER, to ) == null )
        {
            if( Map.IsWall( to ) )
                stop = true;
        }
        //else
        //if( TilePos.x != -1 )
        //if( Map.I.ContinuousWater( TilePos, to ) == false ) removed since this was causing the diagonal harpoon to stop suddenly
        //    stop = true;
        List<Unit> ol = Map.I.FUnit[ ( int ) to.x, ( int ) to.y ];                          
        if ( ol != null )
        for( int i = 0; i < ol.Count; i++ )
        if ( ol[ i ].TileID == ETileType.FISH )            
        if ( ol[ i ].Body.FishType == EFishType.CLOSED_WATER_TRAP ) 
            stop = true;

        if( ForcedMoveDir != Vector3.zero )
        if( OldPosition == transform.position )
        {
            ForcedMoveStuckTime += Time.deltaTime; 
            if( ForcedMoveStuckTime > .05f )                                                             // this is to avoid the harpoon getting stuck bug
                stop = true;
        }
        else
            ForcedMoveStuckTime = 0;

        Unit arrow = Map.I.GetUnit( ETileType.ARROW, TilePos );
        if( Map.I.CheckArrowBlockFromTo( TilePos, to, null, true ) ) 
            stop = true;
        else
        if( arrow && ArrowCheckedPos.x == -1 )
        {
            ForcedMoveDir = Manager.I.U.DirCord[ ( int ) arrow.Dir ];                                   // Arrow direction change
            if( dr != arrow.Dir )
                transform.position = TilePos;
            ArrowCheckedPos = arrow.Pos;
            MasterAudio.PlaySound3DAtVector3( "Click 2", arrow.Pos );
        }
        else
        if( stop )
        {
            transform.position = TilePos;
            ForcedMoveDir = Vector3.zero;
        }
        add *= Map.I.RM.RMD.HarpoonBaseSpeed; 
    }
    public bool UpdatePassingOrbRebound( Vector3 to )
    {
        float fact = 1f;
        Vector3 ivec = Vector3.zero;
        if( cInput.GetKey( "Move NE" ) ) ivec += new Vector3( fact, fact,   0 ); else            
        if( cInput.GetKey( "Move SE" ) ) ivec += new Vector3( +fact, -fact, 0 ); else
        if( cInput.GetKey( "Move SW" ) ) ivec += new Vector3( -fact, -fact, 0 ); else
        if( cInput.GetKey( "Move NW" ) ) ivec += new Vector3( -fact, +fact, 0 );
        if( ivec == Vector3.zero )
           {
            if( cInput.GetKey( "Move N" ) ) ivec += new Vector3( 0, fact, 0  ); else
            if( cInput.GetKey( "Move S" ) ) ivec += new Vector3( 0, -fact, 0 ); else
            if( cInput.GetKey( "Move E" ) ) ivec += new Vector3( +fact, 0    ); else
            if( cInput.GetKey( "Move W" ) ) ivec += new Vector3( -fact, 0, 0 );
           }

        Vector2 tg = TilePos + new Vector2( -ivec.x, -ivec.y );
        Unit orb = Map.I.GetUnit( ETileType.ORB, tg );                                            // Side orb exists?
        if( orb == null )
        {
            orb = Map.I.GetUnit( ETileType.ORB, to );                                             // if not, destination tile is an orb?
            if( orb )
            {
                if( TileDistanceTravelled < .5f ) return false;
                if( ivec == Vector3.zero ) return true;                                           // no user direction to move, so stop hook   
            }
        }
        if( orb )
        {
            if( TileDistanceTravelled < .5f ) return false;
            EDirection dr = Util.GetTargetUnitDir( -ivec, Vector2.zero );

            if( OrbHitTimeCounter <= 0 )
            {
                OrbHitTimeCounter = .5f;
                CastHarpoon( dr );
                Util.PlayParticleFX( "Water Splash", transform.position ); 
                orb.StrikeTheOrb( G.Hero );                                                       // Strike the orb
                Water.CheckCn( EPoleBonusCnType.HOOK_STRIKE_ORB, orb );
            }
            return false;
        }
        return false;
    }
    void OnCollisionEnter2D( Collision2D col )
    {
        int posx = 0, posy = 0;
        Map.I.Tilemap.GetTileAtPosition( col.collider.transform.position, out posx, out posy );

        Unit ga = Map.I.GetUnit( new Vector2( posx, posy ), ELayerType.GAIA );
        if( ga && Map.IsWall( new Vector2( posx, posy ), true ) ) 
            ForcedMoveDir = Vector3.zero; 

        Unit un = Map.I.GetUnit( new Vector2( posx, posy ), ELayerType.MONSTER );
        if( un )
        {
            if( un.TileID == ETileType.ORB )                                                                     // Orb bump FX
            {       
                if( OldTilePos.x != 1 )               // Hook strike water orb
                {
                    if( OrbHitTimeCounter <= 0 )
                    {
                        Unit raft = Controller.GetRaft( new Vector2( posx, posy ) );
                        if( G.HS.RaftDestroy || Water.TempRaftDestroy )                                         // Hook bonus Raft destroy: this need to be done here to avoid the harpoon bump bug instead of raft destruction
                        {
                            if( Water.DestroyRaft( raft ) )
                                return;
                        }

                        Util.PlayParticleFX( "Water Splash", transform.position ); 
                        un.StrikeTheOrb( G.Hero, true );
                        OrbHitTimeCounter = .5f;
                        Vector2 vec = Map.I.GetInputVector( false ) * -1;
                        EDirection dir = Util.GetTargetUnitDir( Vector2.zero, vec );
                        CastHarpoon( dir );
                        Water.CheckCn( EPoleBonusCnType.HOOK_STRIKE_ORB, un );
                    }
                }
            }
        }
    }

    public static void UpdateHarpoonShot()
    {
        float avail = Item.GetNum( ItemType.Res_Harpoon );
        if( avail < 1 ) return;
        if( cInput.GetKey( "Rotate CCW" ) == false )  
        if( cInput.GetKey( "Rotate CW" ) == false ) return;

        Vector2 vec = Map.I.GetInputVector( true );
        if( vec == Vector2.zero ) return;
        bool ok = false;
        EDirection dir = Util.GetTargetUnitDir( Vector2.zero, vec );
        for( int h = 0; h < Map.I.HookList.Count; h++ )
        if ( Map.I.HookList[ h ].gameObject.activeSelf )
        {
            FishingObject hk = Map.I.HookList[ h ];
            EDirection hdir = EDirection.NONE;
            if( hk.ForcedMoveDir != Vector3.zero )
               hdir = Util.GetTargetUnitDir( Vector2.zero, hk.ForcedMoveDir );

            if( hdir == EDirection.NONE ||
                Util.GetInvDir( hdir ) != dir )
            {
                ok = true;
                hk.CastHarpoon( dir );
            }
        }
        if( ok )
        {
            Item.AddItem( ItemType.Res_Harpoon, -1 );                                                  // Charge harpoon item
            MasterAudio.PlaySound3DAtVector3( "Jet 1", Map.I.MainHook.transform.position );       // Sound FX   
        }
    }
    public void CastHarpoon( EDirection dir )
    {
        if( dir == EDirection.NONE ) return;
        ForcedMoveDir = Manager.I.U.DirCord[ ( int ) dir ];
        Sprite2.transform.eulerAngles = Util.GetRotationAngleVector( dir );                        // Sprite direction change
    }
}
