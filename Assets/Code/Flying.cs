using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
public enum EFlyingAction
{
    NONE = 0, FlyToPosition, FlyToHero, FollowHero, RoamAroundOrigin, JumpToStep, ApproachHero, SetRotationSpeed, SetFlyingMaxSpeed,
    FlyStraight, RoamAroundLastPos, SetTargettingRadiusFactor, SetSideFlightSpeed, SetSideFlightTime, SetSideFlightZigZagDistance,
    SetSideFlightZigZagTimeFactor, SetFlyingAcceleration, SetFlyingSpeed,
    RandomizeZigZagPhase,
    SetInflictedDamageRate,
    void2,
    FlyMe,
    Void,
    FlyToHeroFrontTarget,
    EnableHeroFrontTarget,
    DisableHeroTarget,
    FollowHeroFrontTarget,
    ____InitSubPhase____,
    CastProjectile,
    ToggleImmunityShield
}

public enum EFlyingVal
{
    NONE = 0, Corn
}
public partial class Controller : MonoBehaviour
{
    public static float OldCornAmt = 0;
    public static bool CornIsOver = false;
    public static bool FlyingTargetReached = false;
    public static bool OnlyRudderPush = false;
   
    public bool CheckFlyingTargetReached( float range = .4f )
    {
        float distance = Vector2.Distance( FlyingTarget, Unit.transform.position );                                  // Dragon Has reached the target
        if( distance <= range )
        {
            FlyingTargetReached = true;
            FlyingTarget = new Vector2( -1, -1 );

            if( Unit.TileID != ETileType.DRAGON1 )
                FlightSpeedFactor = 100;
            FlightTargetTime = 0;            
            return true;
        }
        return false;
    }
    public void UpdateAttackRate()
    {
      /*  if( Unit.RangedAttack.AttackSpeedTarget == 0 )
            Unit.RangedAttack.AttackSpeedTarget = Random.Range( Map.I.RM.RMD.RTDragonMinAttSpeed, Map.I.RM.RMD.RTDragonMaxAttSpeed );

        float tot = Map.I.RM.RMD.RTDragonMaxAttSpeed - Map.I.RM.RMD.RTDragonMinAttSpeed;                                                    // Dragon Color
        //float spd = tot / 2;

        AttackSpeedLerpTimer += Time.deltaTime;
        float timer = AttackSpeedLerpTimer;
        if( timer > 1 ) timer = 1;
        Unit.RangedAttack.AttackSpeedRate = Mathf.Lerp( Unit.RangedAttack.AttackSpeedRate, Unit.RangedAttack.AttackSpeedTarget, timer );

        if( AttackSpeedLerpTimer > 4 )
        {
            Unit.RangedAttack.AttackSpeedTarget = 0;
            AttackSpeedLerpTimer = 0;
        }

        float perc = ( Unit.RangedAttack.AttackSpeedRate - Map.I.RM.RMD.RTDragonMinAttSpeed ) * 100 / ( tot );
        perc = Mathf.Clamp( perc, 0, 100 );
        //Debug.Log("perc " + perc + " rete " + Unit.RangedAttack.AttackSpeedRate + " target  " + Unit.RangedAttack.AttackSpeedTarget );
        //perc /= 100;
        //perc = 1- perc;
        //Unit.Spr.color = new Color( perc, perc, perc, 1 );
        Unit.Spr.transform.localScale = new Vector3( .6f + Util.Percent( perc, 1.3f ), .6f + Util.Percent( perc, 1.3f ), 1 );
        //Unit.RightText.text = "" + perc.ToString( "0." );
        //Unit.RightText.gameObject.SetActive( true );*/
    }
    public void SortFlyingTarget( Vector2 cubecord, Vector2 abscord, float range )
    {                                                                                                                                  // Sort a new Target
        Vector2 pt = new Vector2( Map.I.RM.HeroSector.Area.xMin + cubecord.x, Map.I.RM.HeroSector.Area.yMin + cubecord.y );
        if( abscord.x != -1 ) pt = abscord;
        FlyingTarget = new Vector2( ( float ) Random.Range( pt.x - range, pt.x + range ),
                                    ( float ) Random.Range( pt.y - range, pt.y + range ) );
        //Message.RedMessage( FlyingTarget.ToString() );
        FlightTargetTime = 0;
    }
    public void UpdateDragon1Movement()
    {
        if( Unit.Activated == false ) return;
        if( Unit.Md.FlyingAction == null || Unit.Md.FlyingAction.Length <= 0 )
            Debug.LogError( "Flying action not defined. " + Unit + "  " + Helper.I.GetCubeTile( Map.GM() ) );

        Mod.FlyingActionStruct f = Unit.Md.FlyingAction[ FlightJumpPhase ];
        float tfact = FlightSpeedFactor;
        if( Map.I.IsPaused() )                                                                // Oxygen speed reducing
            tfact = Map.I.RM.RMD.OnOxygenFlyingUnitsSpeedFactor;
        float delta = Util.Percent( tfact, Time.deltaTime );
        Unit.Spr.color = Color.white;
        CheckHeroShieldCollision();                                                            // Checks hero shield collision
        float corn = Item.GetNum( ItemType.Res_BirdCorn );
        
        int ph = -1;
        if( OldCornAmt != corn )
        {
            if( CornPickedAmt > 0 && corn > 0 &&                                                // Corn added                                                    
                f.CornAddJumpPhase != -1 )
                ph = f.CornAddJumpPhase;
             if( OldCornAmt <= 0 && corn > 0 &&                                                 //olnly if new Corn picked (old was 0) 
                f.CornPickedJumpPhase != -1 )
                ph = f.CornPickedJumpPhase;
        }

        if( CornIsOver )                                                                        // Corn Zeroed
            ph = f.CornZeroedJumpPhase;

        if( ph != -1 )                                                                          // Phase Jump
        {           
            FlightPhaseAdvance( ph );
        }

        CornIsOver = false;
        if( f.SeedCostPerSecond != 0 && corn > 0 )
        {
            float old = Item.GetNum( ItemType.Res_BirdCorn );
            float amt = f.SeedCostPerSecond * Time.deltaTime;                                    // Charges Corn Resource per second

            if( CornAlreadyCharged == false )
            {
                Item.AddItem( ItemType.Res_BirdCorn, -amt );
                CornAlreadyCharged = true;
            }
            if( Item.GetNum( ItemType.Res_BirdCorn ) < 0 )
            {
                CornIsOver = true;                                                              // Corn Stock is over
                Item.SetAmt( ItemType.Res_BirdCorn, 0 );
            }
        }
        string snd = "";
        float pitch = 1f;
        switch( f.FlyingAction )
        {
            case EFlyingAction.FlyToPosition:
            if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
            if( FlyingTarget.x == -1 )
            {
                SortFlyingTarget( FlyingActionTarget[ FlightJumpPhase ], new Vector2( -1, -1 ),
                GetVal() );
            }

            if( CheckFlyingTargetReached() )
            {
                FlightPhaseAdvance();
            }

            break;

            case EFlyingAction.FlyToHeroFrontTarget:
            if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
            int tr = FrontalFlyingTargetDist;
            if( tr < 0 ) { FlightPhaseAdvance(); break; }                                              // No target distance has been set

            Vector2 tg = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.N, tr );

            if( FlyingTarget.x == -1 )
            {
                SortFlyingTarget( new Vector2( -1, -1 ), tg, 0 );
            }

            if( CheckFlyingTargetReached() )
            {
                FlightPhaseAdvance();
                PhaseWonFreePhases = f.PhaseWonFreePhasesGained;                                    // Phase Objective Succesful!
                snd = "Save Game";
                pitch = 1.6f;
            }
            break;

            case EFlyingAction.FollowHeroFrontTarget:

            if( CheckCondition() == false )
            {
                FrontalFlyingTargetDist = -1;
                FlightPhaseAdvance(); break; 
            }

            tr = FrontalFlyingTargetDist + FrontalTargetManeuverDist;
            if( tr < 0 ) { FlightPhaseAdvance(); break; }
            tg = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.N, tr );
            FlyingTarget = tg;
            float rad = GetVal();
            rad = Mathf.Clamp( rad, .1f, 20 );
            if( CheckFlyingTargetReached( rad ) )
            {
                FlightPhaseAdvance();
                PhaseWonFreePhases = f.PhaseWonFreePhasesGained;                                           // Phase Objective Succesful!
                snd = "Save Game";
                pitch = 1.6f;
            }
            break;

            case EFlyingAction.RoamAroundLastPos:
            if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
            if( FlyingTarget.x == -1 )
            {
                SortFlyingTarget( new Vector2( -1, -1 ), Unit.Pos,
                GetVal() );
            }

            if( CheckFlyingTargetReached() )
            {
                FlightPhaseAdvance();
            }

            break;
            
            case EFlyingAction.RoamAroundOrigin:
            if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
            if( FlyingTarget.x == -1 )
            {
                SortFlyingTarget( new Vector2( -1, -1 ), Unit.IniPos,
                GetVal() );
            }

            if( CheckFlyingTargetReached() )
            {
                FlightPhaseAdvance();
            }

            break;
            
            case EFlyingAction.JumpToStep:
            if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
            FlightJumpPhase = ( int ) GetVal();
            FlightPhaseAdvance( FlightJumpPhase );
            break;

            case EFlyingAction.ApproachHero:
            if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
            if( FlyingTarget.x == -1 )
            {
                List<Vector2> pl = new List<Vector2>();
                float range = GetVal();
                for( int y = ( int ) Unit.Pos.y - ( int ) range; y <= Unit.Pos.y + ( int ) range; y++ )
                for( int x = ( int ) Unit.Pos.x - ( int ) range; x <= Unit.Pos.x + ( int ) range; x++ )
                   {
                       Vector2 pt = new Vector2( x, y );
                       if( Vector2.Distance( pt, G.Hero.Pos ) < Vector2.Distance( Unit.Pos, G.Hero.Pos ) ) pl.Add( pt );
                   }

                if( pl.Count <= 0 ) Debug.LogError("Bad Approach hero radius");
                     
                int id = Random.Range( 0, pl.Count );
                SortFlyingTarget( new Vector2( -1, -1 ), pl[ id ], .2f );
            }

            if( CheckFlyingTargetReached() )
            {
                FlightPhaseAdvance();
            }

            break;

            case EFlyingAction.FlyToHero:
            if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
            if( FlyingTarget.x == -1 )
            {
                SortFlyingTarget( new Vector2( -1, -1 ), G.Hero.Pos, GetVal() );
            }

            if( CheckFlyingTargetReached() )
            {
                FlightPhaseAdvance();
            }

            break;

            case EFlyingAction.FollowHero:
            if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
            FlyingTarget = G.Hero.Pos;

            Unit.Spr.color = new Color( 1, .5f, 5f, 1f );
            if( Map.I.GetUnit( ETileType.SNOW, G.Hero.Pos ) )
                FlyingTarget = G.Hero.transform.position;

            if( CheckFlyingTargetReached( GetVal() ) )
            {
                FlightPhaseAdvance();
                snd = "Drop Resource";         
            }

            break;
            case EFlyingAction.FlyStraight:
            if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
            FlyingTarget = G.Hero.Pos;

            if( FlightTargetTime > GetVal() )
            {
                FlyingTarget = new Vector2( -1, -1 );
                FlightTargetTime = 0;
                FlightPhaseAdvance();
            }
            break;

            case EFlyingAction.SetRotationSpeed:
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                FlyingRotationSpeed = GetVal();
                FlightPhaseAdvance();
            }
            break;
            case EFlyingAction.SetTargettingRadiusFactor:
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                TargettingRadiusFactor = GetVal();
                FlightPhaseAdvance();
            }
            break;
            case EFlyingAction.SetFlyingSpeed:
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                FlyingSpeed = GetVal();
                FlightPhaseAdvance();
            }
            break;
            case EFlyingAction.SetFlyingMaxSpeed:
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                FlyingMaxSpeed = GetVal();
                FlightPhaseAdvance();
            }
            break;
            case EFlyingAction.SetFlyingAcceleration:
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                FlyingAcceleration = GetVal();
                FlightPhaseAdvance();
            }
            break;
            case EFlyingAction.SetSideFlightSpeed:
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                TotalSideFlightSpeed = GetVal();
                FlightPhaseAdvance();
            }
            break;
            case EFlyingAction.SetSideFlightTime:
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                TotalSideFlightTime = GetVal();
                FlightPhaseAdvance();
            }
            break;
            case EFlyingAction.SetSideFlightZigZagDistance:
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                SideFlightZigZagDistance = GetVal();
                FlightPhaseAdvance();
            }
            break;
            case EFlyingAction.SetSideFlightZigZagTimeFactor:
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                SideFlightZigZagTimeFactor = GetVal();
                FlightPhaseAdvance();
            }
            break;
            case EFlyingAction.RandomizeZigZagPhase:
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                SideFlightZigZagPhase = Random.Range( 0, 100 );
                FlightPhaseAdvance();
            }
            break;
            case EFlyingAction.SetInflictedDamageRate:
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                Unit.Body.InflictedDamageRate = GetVal();
                FlightPhaseAdvance();
            }
            break;

            case EFlyingAction.EnableHeroFrontTarget:
            if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
            FrontalFlyingTargetDist = ( int ) GetVal();
            FlightPhaseAdvance();
            break;

            case EFlyingAction.DisableHeroTarget:
            if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
            FrontalFlyingTargetDist = -1;
            FlightPhaseAdvance();
            break;

            case EFlyingAction.FlyMe:
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                Unit.Spr.color = new Color( .55f, .55f, 1f, 1f );
                FlyingTarget = Unit.Pos + G.Hero.GetRelativePosition( EDirection.N, 30 );
            }
            break;
            case EFlyingAction.____InitSubPhase____:
            FlightPhaseAdvance();
            break;

            case EFlyingAction.CastProjectile:                                
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                CastProjectileList.Add( f.FloatVal );                                                                           // if you want to cast more projectiles, add more triggers 
                FlightPhaseAdvance();
            }
            break;
            case EFlyingAction.ToggleImmunityShield:                                
            {
                if( CheckCondition() == false ) { FlightPhaseAdvance(); break; }
                Unit.Body.ImmunityShieldTime = f.FloatVal;
                FlightPhaseAdvance();
            }
            break;
        }

        Unit.Body.UpdateImmunityShield();

        for( int i = CastProjectileList.Count - 1; i >= 0; i-- )                                                                // Casts Projectiles from the list
        {
            CastProjectileList[ i ] -= delta;
            if( CastProjectileList[ i ] <= 0 )
            {
                Unit b = Map.I.SpawnFlyingUnit( Unit.transform.position, ELayerType.MONSTER, ETileType.PROJECTILE, null );
                b.Control.ProjectileType = EProjectileType.MISSILE;
                b.Control.ControlType = EControlType.PROJECTILE;
                b.Spr.spriteId = 210;
                b.Control.Mother = Unit;
                b.Variation = Unit.Md.ProjectileType;
                b.Spr.transform.rotation = Unit.Spr.transform.rotation;
                Quaternion qn = Util.GetRotationToPoint( b.Spr.transform.position, G.Hero.Pos );
                b.Spr.transform.rotation = Quaternion.RotateTowards(
                b.Spr.transform.rotation, qn, 9999 );
                b.Spr.transform.localPosition = new Vector3( 0, 0, b.Spr.transform.localPosition.z );
                b.Control.FlyingSpeed = Unit.Md.ProjectileSpeed;
                b.Control.FlyingRotationSpeed = Unit.Md.ProjectileRotationSpeed;
                b.Spr.color = Color.white;
                CastProjectileList.RemoveAt( i );
                Map.I.CreateExplosionFX( b.transform.position );                                                                // Explosion FX
            }
        }

        FlightActionTime += delta;
        float tottime = FlightActionTotalTime[ FlightJumpPhase ];
        if( tottime != 0 )                                                                            // Flight Phase Max Time check
        {
            if( FlightActionTime >= tottime )
                FlightPhaseAdvance();
        }

        //Map.I.Deb = "Phase " + FlightJumpPhase + ": " + f.FlyingAction +
        //"\nFlyingTarget: " + FlyingTarget +
        //" Loop: " + FlightPhaseLoopCount + "\nTime: " + FlightActionTime + " tot: " + tottime +
        //"\nSpeed: " + FlyingSpeed + " Rot Speed: " + FlyingRotationSpeed +
        //"\nPhaseWonFreePhases: " + PhaseWonFreePhases + 
        //"\nImmunity Shield: " + Unit.Body.ImmunityShieldTime;
        //Debug.Log( Unit.Body.ImmunityShieldTime );

        CheckFlyingTargetReached();

        FlightTime += delta;                                                                     // Time Increment
        FlightTargetTime += delta;

        if( FlyingAcceleration == 0 )                                                            // Flying Acceleration
            FlyingSpeed = FlyingMaxSpeed;
        else
            FlyingSpeed += FlyingAcceleration * delta;

        if( FlyingSpeed > FlyingMaxSpeed )                                                       // Max Speed Clamp
            FlyingSpeed = FlyingMaxSpeed;

        Vector3 add = Unit.Spr.transform.up * delta * FlyingSpeed;                               // Adds to movement vector frontal deslocation

        float fact = SideFlightZigZagTimeFactor;
        float res = -1 + Mathf.PingPong( ( SideFlightZigZagPhase + FlightTime ) * fact, 2 );
        float sidespd = res * SideFlightZigZagDistance;
        if( SideFlightZigZagDistance != 0 )
            add += -Unit.Spr.transform.right * sidespd * delta;                                  // Adds zigzag to movement vector

        if( Unit.Body.FrameDamageTaken != 0 )
        {
            if( SideFlightTime <= 0 )
            {
                SideFlightTime = TotalSideFlightTime;
                SideFlightSpeed = TotalSideFlightSpeed;
                if( Util.Chance( 50 ) ) SideFlightSpeed *= -1;
            }
            add += G.Hero.Spr.transform.up * Map.I.RM.RMD.DragonShotImpactForce;                              // Adds shot impact to movement vector
            Unit.Body.FrameDamageTaken = 0;
        }

        SideFlightTime -= delta;

        if( SideFlightTime <= 0 )                                                                             // Side Flight Time is Over
        {
            SideFlightTime = 0;
            SideFlightSpeed = 0;
        }

        if( SideFlightSpeed != 0 )
        {
            add += -Unit.Spr.transform.right * SideFlightSpeed * delta;                                       // Apply Side flight Calculation
        }
 
        Unit.transform.position += add;                                                                       // Apply final movement Calculation
        FlyingAngle = Util.GetAngleDirection( Unit.Spr.transform.eulerAngles.z );
 
        float rotationSpeed = 75f;                                                                            // Rotation Calculation
        if( FlyingRotationSpeed != 0 )
            rotationSpeed = FlyingRotationSpeed;
        //if( FlightTargetTime > 3 )                                                                          // This is for not getting stuck in an eternal roundabout around target
        //    rotationSpeed += FlightTargetTime * 2;     

        if( FrontalFlyingTargetDist > 0 )
        {
            Map.I.HeroTargetSprite.gameObject.SetActive( true );                                              // Frontal target enabled
            float ds = FrontalFlyingTargetDist + FrontalTargetManeuverDist;           
            if( Util.IsDiagonal( G.Hero.Dir ) ) ds = ds * 1.414213f;
            Map.I.HeroTargetSprite.transform.localPosition = new Vector3( 0, ds, -5 );
        }
        else
        { 
            Map.I.HeroTargetSprite.gameObject.SetActive( false );
            FrontalTargetManeuverDist = 0;
        }

        if( f.FlyingAction != EFlyingAction.FlyStraight )                                                     // Rotates towards target
        if( FlyingTarget != new Vector2( -1, -1 ) )
        {
            Quaternion qn = Util.GetRotationToPoint( Unit.Spr.transform.position, FlyingTarget );
            Unit.Spr.transform.rotation = Quaternion.RotateTowards( 
            Unit.Spr.transform.rotation, qn, delta * rotationSpeed );
        }
        if( f.FlightOrbStrike != MyBool.DONT_CHANGE )                                                         // Orb Strike Condition Update
            FlightOrbStrike = f.FlightOrbStrike;
        if( f.FlightBarricadeDestroy != MyBool.DONT_CHANGE )                                                  // Barricade Destroy Condition Update                           
            FlightBarricadeDestroy = f.FlightBarricadeDestroy;
        if( f.FlightResourcePicking != MyBool.DONT_CHANGE )                                                   // Resource Collecting Condition Update                           
            FlightResourcePicking = f.FlightResourcePicking;
        if( f.FlightWallClash != MyBool.DONT_CHANGE )                                                         // Wall Clash Condition Update                    
            FlightWallClash = f.FlightWallClash;
        if( f.SoundEffect != "" )                                                                             // Sound FX Update                    
            snd = f.SoundEffect; 

        UpdateFlyingTile();

        if( TileChanged )
        {
            Unit fire = Map.I.GetUnit( ETileType.FIRE, Unit.Pos );                           // Fire Damage
            if( fire && fire.Body.FireIsOn )
            {
                Unit.Body.TakeFireDamage();
            }

            Unit orb = Map.I.GetUnit( ETileType.ORB, Unit.Pos );                            // Orb Strike 
            if( FlightOrbStrike == MyBool.TRUE ) 
            if( orb ) orb.StrikeTheOrb( Unit );

            Unit bar = Map.I.GetUnit( ETileType.BARRICADE, Unit.Pos );                      // Barricade Destroy   
            if( FlightBarricadeDestroy == MyBool.TRUE )  
            if( bar ) bar.DestroyBarricade( Unit.Pos );

            if( FlightResourcePicking == MyBool.TRUE )
                G.Hero.Control.UpdateResourceCollecting( Unit.Pos, true );                 // Resource Collecting   

            if( FlightWallClash == MyBool.TRUE )                                           // Wall Clash 
            if( Map.IsWall( Unit.Pos, false ) )                                       
            {
                Unit.Body.CreateBloodSplat();
                MasterAudio.PlaySound3DAtVector3( "Death 1", transform.position );
                Unit.Kill();
            }
        }
        OldCornAmt = Item.GetNum( ItemType.Res_BirdCorn );                                // Corn Stuff
        CornPickedAmt = 0;

        if( snd != "" )
            MasterAudio.PlaySound3DAtVector3( snd, Unit.Pos, 1, pitch );                  // Play Sound FX
    }

    private float GetVal()
    {
        float res = FlyingActionVal[ FlightJumpPhase ];
        Mod.FlyingActionStruct f = Unit.Md.FlyingAction[ FlightJumpPhase ];
        if( f.MaxVal != 0 )
            res = Random.Range( res, f.MaxVal );
        return res;
    }
    public void CheckHeroShieldCollision()                                                                                             // Hero shield collision for Flying units
    {
        if( Util.Manhattan( G.Hero.Pos, Unit.Pos ) > 2 ) return;
        if( Unit.MeleeAttack  && Unit.MeleeAttack.InvalidateAttackTimer > 0 ) return;
        if( Unit.RangedAttack && Unit.RangedAttack.InvalidateAttackTimer > 0 ) return; 
        for( int i = 0; i < Map.I.RM.HeroSector.ActiveHeroShield.Count; i++ )
        if( Map.I.RM.HeroSector.ActiveHeroShield[ i ] )                                                                                // Hero shield collision
        {
            if( CheckProjDist( Unit.transform.position, Map.I.HeroShieldSpriteList[ i ].transform.position ) )
            {
                if( Unit.MeleeAttack )
                    Unit.MeleeAttack.InvalidateAttackTimer = 2;                                                                        // invalidade melee att
                if( Unit.RangedAttack )
                    Unit.RangedAttack.InvalidateAttackTimer = 2;                                                                       // invalidade ranged att
                Body.UseShield( i, Unit );
                Map.I.CreateExplosionFX( Unit.transform.position, "Fire Explosion", "" );                                              // Explosion FX
            }
        }
    }

    public bool CheckCondition()
    {
        Mod.FlyingActionStruct f = Unit.Md.FlyingAction[ FlightJumpPhase ];
        if( f.IgnoreCheck ) return true;

        if( f.OnlyInit )
        if( FlightLoopCount > 0 ) return false;

        if( f.IsPhaseWonPrize )                                                                    // Phase Won Prize ignore check if phase was won
        {
            if( PhaseWonFreePhases > 0 ) return true;
            return false;
        }

        float seed = Item.GetNum( ItemType.Res_BirdCorn );
        if( f.ExecutionChance < 100 )                                                              // Execution Chance
        if( Util.Chance( f.ExecutionChance ) == false )
        {
            return false;
        }
        if( !f.ExecuteWithCorn && !f.ExecuteWithoutCorn ) return false;
        if( f.ExecuteWithCorn && f.ExecuteWithoutCorn ) { }                                        // Always execute
        else
        {
            if( f.ExecuteWithCorn )                                                                // Corn Phase
            if( seed <= 0 ) return false;

            if( f.ExecuteWithoutCorn )                                                             // No Corn Phase
            if( seed > 0 ) return false;
        }

        return true;
    }
    public void FlightPhaseAdvance( int phase = -1, bool force = false )
    {
        Mod.FlyingActionStruct f = Unit.Md.FlyingAction[ FlightJumpPhase ];
        if( force == false )
        {
            if( FlightPhaseLoopCount > 0 )                                                   // Looping Flight Phase
            {
                FlightPhaseLoopCount--;
                return;
            }

            if( f.MinimumTime != -1 )                                                        // Minimum time to advance not enough
            if( FlightTime < f.MinimumTime ) return;            
        }
        FlyingTargetReached = false;
        PhaseWonFreePhases--;                                                                // Every time a Phase is "Won" the next phases are free (no condition check) acording to this number
        if( phase != -1 )
        {
            FlyingTarget = new Vector2( -1, -1 );
            FlightJumpPhase = phase;
        }
        else
        if( ++FlightJumpPhase == Unit.Md.FlyingAction.Length )                               // Advance Flight Phase
        {
            FlightJumpPhase = 0;
            FlightLoopCount++;
        }

        FlightPhaseLoopCount = FlyingActionLoopCount[ FlightJumpPhase ];                     // Set Loop Amount
        FlightActionTime = 0;

        if( f.FlyingMaxSpeed != -1      ) FlyingMaxSpeed = f.FlyingMaxSpeed;
        if( f.FlyingAcceleration != -1  ) FlyingAcceleration = f.FlyingAcceleration;
        if( f.FlyingRotationSpeed != -1 ) FlyingRotationSpeed = f.FlyingRotationSpeed;
        if( f.SideFlightZigZagDistance != -1 ) SideFlightZigZagDistance = f.SideFlightZigZagDistance;
    }
    public void UpdateDragon2Movement()
    {
        if( Unit.Activated == false ) return;

        if( Sleeping )
        {
            if( UpdateSleep() == false ) return;
        }

        Vector2 bestpos = new Vector2( -1, -1 );
        List<Vector2> tgl = new List<Vector2>();

        #region Grave
        //Vector2 pos = new Vector2( ( int ) Unit.Pos.x, ( int ) Unit.Pos.y );
        //Vector2 bestbarricade = new Vector2( -1, -1 );

        //float bestscore = 99999;
        //int[,] dr = { { 6, 2, 0, 4, 1, 5, 3, 7 },                                                    // Horizontal, vertical, diagonal - ar = 0
        //              { 0, 4, 6, 2, 1, 5, 3, 7 } };                                                  // Vertical, Horizontal, diagonal - ar = 1

        //int ar = 0;
        //if( Map.I.Hero.Pos.y == pos.y ) ar = 1;
        //float dx = Mathf.Abs( Map.I.Hero.Pos.x - pos.x );
        //float dy = Mathf.Abs( Map.I.Hero.Pos.y - pos.y );
        //if( dx > dy ) ar = 1;

        //Unit.UpdateDirection();

        //CheckPusher();

        // Unit ga2 = Map.I.GetUnit( ETileType.WEB, G.Hero.Pos );                                       // Hero over Rotator 

        // if( !ga2 && Map.I.Hero.Pos == Map.I.CurArea.LockAiTarget )

        //    if( dx == 1 && dy == 1 ) return;                                                            // Monster adjacent to hero doesnt move

        
        //for( int i = 0; i < 8; i++ )
        //{
        //    Vector2 tg = pos + ( Manager.I.U.DirCord[ dr[ ar, i ] ] * 3 );
        //    if( Map.PtOnMap( Map.I.Tilemap, tg ) )
        //    {
        //        float score = Map.I.DistFromTarget[ ( int ) tg.x, ( int ) tg.y ];
        //        if( score < bestscore )
        //        {
        //            //if( Unit.CanFlyFromTo( false, pos, tg, Unit ) == true )
        //            {
        //                bestpos = tg;
        //                bestscore = Map.I.DistFromTarget[ ( int ) tg.x, ( int ) tg.y ];
        //                tgl.Add( tg );
        //            }
        //        }
        //    }
        //}
        
        //if( Map.I.PtOnAreaMap( bestpos ) )
        //    if( Map.I.IsInTheSameLine( G.Hero.Pos, Unit.Pos ) )                                            // No move if 2 or more equal scores
        //        if( Map.I.IsInTheSameLine( G.Hero.Pos, bestpos ) == false )
        //            if( equal >= 1 )
        //            {
        //                float dist1 = Vector2.Distance( G.Hero.GetFront(), equalid1 );
        //                float dist2 = Vector2.Distance( G.Hero.GetFront(), equalid2 );
        //                if( Map.I.CurrentMoveTrialType == EMoveType.ROTATE ) return;
        //                if( Map.I.CurrentMoveTrialType == EMoveType.STILL ) return;
        //                if( G.Hero.Control.OldPos != G.Hero.Pos ) return;

        //                if( dist1 == dist2 ) return;
        //                if( dist2 < dist1 ) bestpos = equalid2;
        //                else bestpos = equalid1;
        //            }

        //Unit.UpdateDirection();

        //bool res = Unit.CanMoveFromTo( true, pos, bestpos, Unit );
        //if( res )
        //   MoveMade = true;
        //CheckPusher();
        //Unit.UpdateDirection();
        //UpdateFlyingTarget();
        //Debug.Log(bestpos);

        //if( Unit.CanFlyFromTo( false, pos, bestpos, Unit ) == false )
        //    for( int i = 0; i < 8; i++ )
        //    {
        //        tgl.Remove( bestpos );
        //        if( tgl != null && tgl.Count > 0 )
        //        {
        //            int id = Random.Range( 0, tgl.Count );
        //            bestpos = tgl[ id ];
        //        }
        //        else
        //        {
        //            bestpos = Unit.Pos; break;

        //        }
        //    }

        #endregion

        //FlightSpeedFactorTimer -= Time.deltaTime;

        //if( FlightSpeedFactorTimer <= 0 )
        //{
        //    if( Map.I.IsInTheSameLine( G.Hero.Pos, Unit.Pos, false ) )
        //        if( Map.I.HasLOS( Unit.Pos, G.Hero.Pos ) )
        //        {
        //            bestpos = G.Hero.Pos;
        //            FlightSpeedFactor = 300;
        //            FlightSpeedFactorTimer = 5;
        //        }

        //}

        Unit.RightText.text = "P" + FlightJumpPhase + " " + FlightPhaseTimer.ToString( "0." );
        Unit.RightText.gameObject.SetActive( true );

        if( FlightJumpPhase == 0 )                                                                                          // Initial Phase
        {
            FlightPhaseTimer = 0;
            FlightJumpPhase = 1;
            if( FlyingTarget.x == -1 )
                FlyingTarget = Unit.Pos;
        }   
        else
        if( FlightJumpPhase == 1 )                                                                                         // Countdown Phase
        {
           // Unit.RightText.text = "" + JumperPhaseTimer.ToString( "0." );
            Unit.RightText.gameObject.SetActive( true );
            Unit.Spr.transform.rotation = Util.GetRotationToPoint( Unit.Spr.transform.position, G.Hero.Pos );
            
            if( FlightPhaseTimer > 3 )
            {
                FlightPhaseTimer = 0;
                FlightJumpPhase = 2;
            }
        }
        else
        if( FlightJumpPhase == 2 )                                                                                         // Target Chosing Phase
        {
            Vector2 target = G.Hero.Pos;
            if( Map.I.HasLOS( Unit.Pos, Map.I.Hero.Pos ) == true )
            {
                target = G.Hero.Pos;
            }
            else
            {
                target = GetBestPos( 3, ref tgl );
            }

            List<Vector2> LineCord = Map.I.GetLineCords( Unit.Pos, target, 2 );

            FlyingTarget = LineCord[ 0 ];
            for( int i = 0; i < LineCord.Count; i++ )
            {
                if( Map.IsWall( LineCord[ i ] ) ) break;
                FlyingTarget = LineCord[ i ];
            }

            FlightPhaseTimer = 0;
            FlightJumpPhase = 3;
        }
        else
        if( FlightJumpPhase == 3 )                                                                                     // Flight Countdown Phase
        {
            Unit.RightText.gameObject.SetActive( true );
            Unit.Spr.transform.rotation = Util.GetRotationToPoint( Unit.Spr.transform.position, FlyingTarget );
            if( FlightPhaseTimer > 3 )
            {
                FlightPhaseTimer = 0;
                FlightJumpPhase = 4;
            }
        }
        else
        if( FlightJumpPhase == 4 )                                                                                     // Flight Phase
        {
            FlightPhaseTimer = 0;
            UpdateFlight();
            //Unit.RightText.gameObject.SetActive( false );
            bool res = CheckFlyingTargetReached();
            if( res )
            {
                FlightPhaseTimer = 0;
                FlightJumpPhase = 0;
            }
        }
    }
    
    public void UpdateQueroQueroMovement()
    {
        if( FlightJumpPhase == 0 || FlightJumpPhase == -1 )                                                                            // Initial Phase: Countdown to flight and target Choosing (-1) if was resting 
        {
            if( Map.I.AdvanceTurn || FlyingTarget.x == -1 || FlightJumpPhase == -1 )
            {
                UpdateQueroQueroFlyingTarget( false );
            }

            if( FlyingTarget.x == -1 )                                                                                                 // Updates Sprite Orientation             
                Unit.Spr.transform.rotation = Util.GetRotationToPoint( Unit.Spr.transform.position, G.Hero.Pos );             
            else
                Unit.Spr.transform.rotation = Util.GetRotationToPoint( Unit.transform.position, FlyingTarget );

            float waittime = Map.I.RM.RMD.QueroQueroWaitTime;
            if( WaitTime > 0 ) waittime = WaitTime;

            if( FlightPhaseTimer > waittime )
                FlightJumpPhase = 0;

            if( FlightPhaseTimer > waittime )                                                                                          // Time to fly
            if( FlyingTarget.x != -1 )
            {
                UpdateQueroQueroFlyingTarget( true );
                FlightPhaseTimer = 0;
                FlightJumpPhase = 1;
                FlyingOrigin = Unit.Pos;
            }
        }
        else
        if( FlightJumpPhase == 1 )                                                                                                     // Flight Phase
        {
            FlightPhaseTimer = 0;
            UpdateFlight();

            if( TileChanged )                                                                                                        
            {
                UpdateResourceCollecting( Unit.Pos, true );                                                                           // Monster resource destroy        

                Unit gate = Map.I.GetUnit( ETileType.CLOSEDDOOR, Unit.Pos );                                                          // Flight interrupted in the middle: Kill monster
                if( gate || Map.I.CheckArrowBlockFromTo( OldPos, Unit.Pos, Unit ) )
                    Unit.Kill();

                Unit orb = Map.I.GetUnit( ETileType.ORB, Unit.Pos );                                                                  // Orb Strike
                if( orb )
                {
                    orb.StrikeTheOrb( Unit );
                    FlightPhaseTimer = 100;
                }

                Unit res = Map.I.GetUnit( ETileType.ITEM, Unit.Pos );                                                                // Item destroy
                if( res ) 
                {
                    MasterAudio.PlaySound3DAtVector3( "Error 2", Unit.Pos );
                    Controller.CreateMagicEffect( Unit.Pos, "Mine Debris" );                                                         // FX
                    Map.I.CreateExplosionFX( Unit.Pos );                                                                             // Explosion FX
                    res.Kill();
                }
            }
             
            if( Unit.transform.position.x == FlyingTarget.x &&                                                                       // Target Reached
                Unit.transform.position.y == FlyingTarget.y )
            {
                FlightPhaseTimer = 0;
                FlightJumpPhase = 0;
                FlyingTarget = new Vector2( -1, -1 );
                Unit arrow = Map.I.GetUnit( ETileType.ARROW, Unit.Pos );
                Unit barricade = Map.I.GetUnit( ETileType.BARRICADE, Unit.Pos );

                if( arrow )                                                                                                          // Arrow 
                {
                    FlightPhaseTimer = 100;
                }

                if( barricade )
                {
                    if( barricade.Variation > 0 )
                        barricade.DestroyBarricade( Unit.Pos );                                                                     // destroy barricade                                   
                    FlightPhaseTimer = 100;
                }
            }           
                        
            if( TileChanged )
            {
                Unit barricade = Map.I.GetUnit( ETileType.BARRICADE, Unit.Pos );
                if( barricade && barricade.Variation == 0 )
                {
                    barricade.DestroyBarricade( Unit.Pos );                                                                         // destroy Sized 1 barricade and keep flying Straight     
                }

                Unit fire = Map.I.GetUnit( ETileType.FIRE, Unit.Pos );
                if( fire && fire.Body.FireIsOn )
                {
                    Unit.Body.ReceiveDamage( 1000, EDamageType.FIRE, G.Hero, G.Hero.RangedAttack );                                  // fire hits qq     
                }
            }
        }

        Unit.SetLevelText( "" + Unit.Control.RealtimeSpeed );
    }

    public void UpdateQueroQueroFlyingTarget( bool beforeflight )
    {
        Unit overbarricade = Map.I.GetUnit( ETileType.BARRICADE, Unit.Pos );                          // check if on barricade
        if( overbarricade && beforeflight ) return;                                                   // no flight if before start

        FlyingTarget = new Vector2( -1, -1 );                                                         // reset target
        float bestDist = 1000f;                                                                       // start with large distance

        for( int dr = 0; dr < 8; dr++ )                                                               // test all 8 directions
        {
            Vector2 neigh = Unit.Pos + Manager.I.U.DirCord[ dr ];
            if( !Map.PtOnMap( Map.I.Tilemap, neigh ) ) continue;                                      // skip if outside map
            if( Map.IsWall( neigh ) ) continue;                                                       // skip if blocked by wall

            Unit overarrow = Map.I.GetUnit( ETileType.ARROW, Unit.Pos );
            if( overarrow != null && ( int ) overarrow.Dir != dr ) continue;                          // arrow mismatch
            if( overbarricade != null && ( int ) Util.GetInvDir( FlightDir ) != dr ) continue;        // must fly opposite if on barricade

            bool flightOK = true;
            Vector2 finalTile = new Vector2( -1, -1 );

            for( int r = 1; r < Sector.TSY && flightOK; r++ )                                         // scan forward
            {
                Vector2 tg = Unit.Pos + Manager.I.U.DirCord[ dr ] * r;
                Vector2 old = Unit.Pos + Manager.I.U.DirCord[ dr ] * ( r - 1 );
                if( !Map.PtOnMap( Map.I.Tilemap, tg ) ) break;                                        // stop at edge

                if( Map.I.CheckArrowBlockFromTo( old, tg, Unit ) ) break;                             // blocked by arrow
                if( Map.I.GetPosArea( old ) != Map.I.GetPosArea( tg ) ) break;                        // area change not allowed

                Unit arrow = Map.I.GetUnit( ETileType.ARROW, tg );
                Unit forest = Map.I.GetUnit( ETileType.FOREST, tg );
                Unit gate = Map.I.GetUnit( ETileType.CLOSEDDOOR, tg );
                Unit barricade = Map.I.GetUnit( ETileType.BARRICADE, tg );

                if( arrow && arrow.Dir == ( EDirection ) dr ) arrow = null;                           // same-dir arrow ignored
                if( gate ) { flightOK = false; break; }                                               // gate blocks flight

                if( forest || arrow || ( barricade && barricade.Variation > 0 ) )
                {
                    finalTile = tg;                                                                   // found a valid target
                    break;
                }
            }

            if( flightOK && finalTile.x >= 0 )
            {
                float dist = Vector2.Distance( G.Hero.Pos, finalTile );                               // closer to hero is better
                if( LastFlownOverTile == finalTile )
                    dist += Vector2.Distance( LastFlownOverTile, Unit.Pos ) / 1000f;                  // keep flight straight

                if( dist < bestDist )
                {
                    bestDist = dist;
                    FlyingTarget = finalTile;
                    FlightDir = ( EDirection ) dr;                                                    // store best direction
                }
            }
        }
    }


    public void UpdateFlight()
    {
        float moveSpeed = 4.6f;
        float rotationSpeed= 637f;
        if( Unit.TileID == ETileType.QUEROQUERO )
        {
            rotationSpeed = 99999;
            moveSpeed = FlyingSpeed;
        }

        moveSpeed = Util.Percent( FlightSpeedFactor, moveSpeed );
        rotationSpeed = Util.Percent( FlightSpeedFactor, rotationSpeed );
        Quaternion qn = Util.GetRotationToPoint( Unit.transform.position, FlyingTarget );
        Unit.Spr.transform.rotation = Quaternion.RotateTowards( Unit.Spr.transform.rotation, qn, Time.deltaTime * rotationSpeed );   
        Vector3 old = Unit.transform.position;
        Unit.transform.position = Vector3.MoveTowards( Unit.transform.position, FlyingTarget, Time.deltaTime * moveSpeed );
        UpdateFlyingTile();
        FlyingAngle = Util.GetAngleDirection( Unit.Spr.transform.eulerAngles.z );
    }

    public Vector2 GetBestPos( int range, ref List<Vector2> tgl )
   {
       Vector2 pos = new Vector2( ( int ) Unit.Pos.x, ( int ) Unit.Pos.y );
       Vector2 bestpos = new Vector2( -1, -1 );
       float bestscore = 99999;       

       for( int y = ( int ) Unit.Pos.y - range; y <= Unit.Pos.y + range; y++ )
       for( int x = ( int ) Unit.Pos.x - range; x <= Unit.Pos.x + range; x++ )
           {
               Vector2 tg = new Vector2( x, y );
               if( Map.PtOnMap( Map.I.Tilemap, tg ) )
                   if( Sector.IsPtInCube( tg ) )
                   {
                       Unit ga = Map.I.GetUnit( ETileType.WATER, tg );
                       float rand = Random.Range( 0.00001f, 0.0001f );
                       float score = Map.I.DistFromTarget[ ( int ) tg.x, ( int ) tg.y ] + rand;
                       if( tg != G.Hero.Pos && ga == null )
                           if( Map.I.HasLOS( pos, tg, true, Unit, true, true ) )
                           {
                               tgl.Add( tg );
                               if( score < bestscore )
                               {
                                   bestpos = tg;
                                   bestscore = score;
                               }
                           }
                   }
           }
       return bestpos;
   }

    #region Jumper
    public void UpdateMotherJumperMovement()
    {
        JumperCount++;
        CalculateJumperJump();

        Unit.RangedAttack.AttackSpeedRate = 100;

        SpawnTimer += Time.deltaTime;

        if( SpawnCount == MaxSpawnCount ) 
            SpawnTimer = 0;

        if( MaxSpawnCount == 0 ) CalculateMaxSpawns();

        if( SpawnTimer > 5 )
        if( SpawnCount < MaxSpawnCount )
            {
                Map.I.SpawnFlyingUnit( Unit.Pos, ELayerType.MONSTER, ETileType.JUMPER, Unit );
                SpawnTimer = 0;
                SpawnCount++;
            }
    }
    public void UpdateJumperMovement()
    {
        JumperCount++;
        Unit.RangedAttack.AttackSpeedRate = 100;
        CalculateJumperJump();
    }
    public void UpdateJumperJumpTarget()
    {
        List<Vector2> tgl = new List<Vector2>();
        Vector2 bestpos = GetBestPos( 3, ref tgl );
 
        if( bestpos == Unit.Pos )
        //if( Util.Chance( 50 ) )                                                                                  // Random Target
        {
            int id = Random.Range( 0, tgl.Count );
            if( tgl.Count > 0 )
                bestpos = tgl[ id ];
            else bestpos = Unit.Pos;
        }

        //float rad = .3f;                                                                                         // random inside tile pos for small Jumpers
        //if( Unit.TileID == ETileType.MOTHERJUMPER ) rad = 0; 
        float rad = 0;
        Vector2 randv = new Vector2( Random.Range( -rad, +rad ), Random.Range( -rad, +rad ) );
        JumpTarget = bestpos + randv;
    }
    public void CalculateJumperJump()
    {        
        if( Unit.Activated == false ) return;
        if( Sleeping )
        {
            if( UpdateSleep() == false ) return;
        }

        //float rotationSpeed = 420f;
        //Unit.Spr.transform.rotation = Quaternion.RotateTowards( Unit.Spr.transform.rotation, qn, Time.deltaTime * rotationSpeed );

        if( FlightJumpPhase == 1 )
        {
            UpdateJumperJumpTarget();
            Quaternion qn = Util.GetRotationToPoint( Unit.Spr.transform.position, JumpTarget );                                                            // Rotates Jumper Towards Target
            Unit.Spr.transform.rotation = qn;
            FlightPhaseTimer = 0;
            FlightJumpPhase = 2;
        }        

        float jumpspeed = 5;
        if( FlightJumpPhase == 2 )
        if( FlightPhaseTimer > Map.I.RM.RMD.BaseJumperFlightTime )
        Unit.transform.position = Vector3.MoveTowards( Unit.transform.position, JumpTarget, Time.deltaTime * jumpspeed );

        if( FlightJumpPhase == 2 )
        if( Unit.transform.position.x == JumpTarget.x )                                                                                                    // Jumper has landed
        if( Unit.transform.position.y == JumpTarget.y )
            {
                FinishedJumperCount++;
                FlightPhaseTimer = 0;
            }

        UpdateFlyingTile();
    }
    public void CalculateMaxSpawns()
    {
        MaxSpawnCount = 0;                                                          
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = Unit.Pos + Manager.I.U.DirCord[ i ];
            if( Map.I.IsTileOnlyGrass( tg ) )
            {
                MaxSpawnCount++;
            }
        }
    }

    #endregion

    #region Wasps
    public static List<Vector2> MWaspTgList;
    public bool WaspLongFlight = false;
 
    public static void UpdateAllMotherWasps()
    {
        if( MotherWaspCount <= 0 ) return;
        if( Map.I.IsPaused() ) return;
        bool fast = false;

        if( Map.I.RM.HeroSector.TimeSpentOnCube < Map.I.WaspFastMoveTime )                                // Accelerated move
        {
            MasterAudio.PlaySound3DAtVector3( "Bee Attack", G.Hero.transform.position, .8f );
            fast = true;
        }

        List<Unit> firel = Util.GetNeighbors( G.Hero.Pos, ETileType.FIRE );
        for( int f = 0; f < firel.Count; f++ )
        if ( firel[ f ].Body.FireIsOn )
            {
                List<Unit> wl = Util.GetFlyingNeighbors( G.Hero.Pos, ETileType.WASP );                   // Fire Mark Hero Neighbor Wasps
                for( int w = 0; w < wl.Count; w++ )
                {
                    if( wl[ w ].Control.FireMarkAvailable )
                    {
                        MasterAudio.PlaySound3DAtVector3( "Save Game", G.Hero.Pos, 1, 1.6f );
                        wl[ w ].Control.FireMarkedWaspFactor += Map.I.RM.RMD.FireMarkedWaspTurns;
                        wl[ w ].Control.FireMarkAvailable = false;
                    }
                }
                break;
            }

        for( int i = G.HS.Fly.Count - 1; i >= 0; i-- )                                       // Flying Move and Attack Update
        if ( G.HS.Fly[ i ].TileID == ETileType.MOTHERWASP )
        {
            Unit fu = G.HS.Fly[ i ];

            bool go = false;
            if( Controller.MoveTickDone )
            if( fu.Control.TickMoveList.Contains( G.HS.CurrentTickNumber ) )
                {
                    go = true;
                }

            if( fu.Control.JumpTarget != new Vector2( -1, -1 ) )
            {
                Vector2 or = fu.Control.OldPos;
                if( or.x == -1 ) or = fu.IniPos;
                float speed = Vector2.Distance( or, fu.Control.JumpTarget ) * 5;
                if( UpdateMotherLocalFlightAnimation( fu ) == false)
                fu.transform.position = Vector3.MoveTowards( fu.transform.position,
                fu.Control.JumpTarget, Time.deltaTime * speed );
                fu.Control.UpdateFlyingTile();
            }
            
            if( fast || go )
            {                
                fu.Control.UpdateMotherWaspMovement();
                fu.UpdateAllAttacks( false );
            }

            if( Controller.MoveTickDone )                                        // Enraged Wasp Update
                fu.Control.UpdateEnragedWasp();

            if( Map.I.CubeDeath == false )                                       // Rotates Mother Wasp Towards Hero
                if( fu.Pos != G.Hero.Pos )
                {
                    fu.UpdateDirection();
                    Quaternion qn = Util.GetRotationToPoint(
                    fu.Spr.transform.position, G.Hero.Pos );
                    fu.Spr.transform.rotation = qn;
                }

            fu.Control.UpdateMotherWaspSpawning();                                // Updates Spawning

            fu.Body.UpdateFrameAnimation( 202, 5, true, 0.03f );                  // Updates Animation
        }
        MWaspTgList = new List<Vector2>();
    }
    public static bool UpdateMotherLocalFlightAnimation( Unit fu )
    {
        if( fu.transform.position.x == fu.Control.JumpTarget.x &&
            fu.transform.position.y == fu.Control.JumpTarget.y )
            fu.Control.WaspLongFlight = false;

        if( ( int ) fu.Control.JumpTarget.x !=  ( int ) fu.transform.position.x ||
            ( int ) fu.Control.JumpTarget.y !=  ( int ) fu.transform.position.y ) return false;
        if( fu.Control.WaspLongFlight ) return false;

        if( fu.transform.position.x == fu.Control.JumpTarget.x &&
            fu.transform.position.y == fu.Control.JumpTarget.y )
        {
            float rad = .25f;
            Vector2 randv = new Vector2( Random.Range( -rad, +rad ), Random.Range( -rad, +rad ) );
            fu.Control.JumpTarget = fu.Pos + randv;
        }
        else
        {
            if( fu.Control.JumpTarget != new Vector2( -1, -1 ) )
            {
                float flightspeed = .2f;
                fu.transform.position = Vector3.MoveTowards( fu.transform.position, 
                fu.Control.JumpTarget, Time.deltaTime * flightspeed );
            }
        }
        return true;
    }

    public void UpdateCustomMove()
    {
        ECustomMoveType move = ECustomMoveType.NONE;
        if( Unit.Md.CustomMoveType == null ) return;
        if( Unit.Md.CustomMoveType.Count < 1 ) return;
        if( Unit.Md.CustomMoveFactor == null ) return;
        if( Unit.Md.CustomMoveFactor.Count != Unit.Md.CustomMoveType.Count ) return;
        if( Unit.Control.Resting ) return;
        if( Map.I.RM.HeroSector.TimeSpentOnCube < Map.I.WaspFastMoveTime ) return;

        int id = Util.Sort( Unit.Md.CustomMoveFactor );
        move = Unit.Md.CustomMoveType[ id ];
        Vector2 best = new Vector2( -1, -1 );
        switch( move )
        {
            case ECustomMoveType.STANDARD:
            best = GetBestStandardMove( G.Hero.Pos, false );
            if( Unit.CanMoveFromTo( false, Unit.Pos, best, Unit ) )
            if( Map.I.RM.HeroSector.TimeSpentOnCube >= Map.I.WaspFastMoveTime )
            {
                JumpTarget = best;
                WaspLongFlight = true;
                MWaspTgList.Add( best );
            }
            break;
            case ECustomMoveType.RANDOM:
            case ECustomMoveType.AROUND_HERO:
            case ECustomMoveType.AROUND_HERO_X2:
            Vector2 tg = Unit.Pos;
            int rad = 1;
            if( move == ECustomMoveType.AROUND_HERO ) 
                tg = G.Hero.Pos;
            if( move == ECustomMoveType.AROUND_HERO_X2 )
            {
                tg = G.Hero.Pos;
                rad = 2;
            }
            for( int i = 0; i < 99; i++ )
            {
                best = tg + new Vector2( Random.Range( -rad, rad + 1 ), 
                                         Random.Range( -rad, rad + 1 ) );
                if( best != G.Hero.Pos )
                if( Unit.CanMoveFromTo( false, Unit.Pos, best, Unit ) )
                {
                    JumpTarget = best;
                    WaspLongFlight = true;
                    MWaspTgList.Add( best );
                    break;
                }
            }
            break;
        }
    }

    public void UpdateMotherWaspMovement()
    {
        UpdateCustomMove();
        UpdateWaspJumpTarget();
        UpdateFlyingTile();
        Unit.MeleeAttack.AttackSpeedRate = 100;
        if( Unit.Body.ChildList.Count <= 0 )                                                            // Fire Damage
            Unit.Body.TakeFireDamage( -1, -1, false );
    }

    public void UpdateEnragedWasp()
    {
        int num = Unit.Md.EnragedWaspUpdates;
        if( num == 1 )
        if( G.HS.CurrentTickNumber != 1 ) return;
        if( num == 2 )
        if( G.HS.CurrentTickNumber != 1 )
        if( G.HS.CurrentTickNumber != 3 ) return;
        if( num == 3 )
        if( G.HS.CurrentTickNumber == 3 ) return;

        if( Unit.Control.Resting == false )
        if( Map.I.RM.HeroSector.TimeSpentOnCube >= Map.I.WaspFastMoveTime )
            {
                float amt = Unit.Control.EnragedWaspCount;
                Unit.Control.MaxSpawnCount += amt * Unit.Md.EnragedWaspSpawnLimitAdd;                       // Enraged Wasp increments
                Unit.Control.BaseWaspTotSpawnTimer += amt * Unit.Md.EnragedWaspSpawnPercentAdd;
                Unit.Control.ExtraEnragedSpawns = ( int ) ( amt * Unit.Md.EnragedWaspExtraSpawns );
            }
    }

    public void UpdateMotherWaspSpawning()
    {
        if( Map.I.KillList.Contains( Unit ) ) return;

        SpawnTimer += Time.deltaTime;

        if( SpawnCount == ( int ) MaxSpawnCount )
            SpawnTimer = 0;

        float herospeed = G.Hero.RangedAttack.GetRealtimeSpeed( -1, false );                                        // Get Hero att speed for comparison
        float herotottime = 1 / ( herospeed / Map.I.Tick );
        float spawnperc = BaseWaspTotSpawnTimer;
        spawnperc += WaspOccupiedTiles.Count * ExtraWaspSpawnTimerPerTile;
        spawnperc += Unit.Md.EnragedWaspExtraBreedingSpeed * Unit.Control.EnragedWaspCount;
        Unit.RightText.gameObject.SetActive( true );                                                                // Updates Text mesh
        Unit.RightText.text = "(" + Unit.Body.ChildList.Count + "/" + ( int ) MaxSpawnCount + ")";
        if( spawnperc > 0 )
            Unit.RightText.text += "\n" + spawnperc.ToString( "0." ) + "%";

        Unit.RightText.color = Color.white;
        if( Unit.Control.EnragedWaspCount > 0 )
            Unit.RightText.color = Color.red;
        Unit.Body.Sprite2.gameObject.SetActive( true );
        if( Unit.Body.ChildList.Count <= 0 )
        {
            Unit.RightText.gameObject.SetActive( false );     
            Unit.Body.Sprite2.gameObject.SetActive( false );
        }
        int totspawn = 1;
        float spawntottime = 0;
        if( spawnperc > 0 ) spawntottime = ( 100 / spawnperc ) * herotottime;

        if( Unit.Control.ExtraEnragedSpawns > 0 )                             // Enraged Extra Spawns
        {
            totspawn = Unit.Control.ExtraEnragedSpawns;
            if( SpawnTimer > spawntottime )
                totspawn += 1;
        }

        if( Initialized == false )
        {
            totspawn = ( int ) MaxSpawnCount;                                                                              // initializing, spawn all at once
            SpawnTimer = 100;
        }

        if( Initialized )                                                                                          // No more babies if all killed
            if( SpawnCount <= 0 )
                SpawnTimer = 0;

        float fat = Map.I.SessionTime - Unit.Body.FirstAttackTime;                                                 // fat: first attack time
        if( Unit.Control.Resting ) fat = 0;

        if( fat > 0 )
        {
            spawnperc += fat * ExtraWaspSpawnInflationPerSec;
            WaspInflationSum += Time.deltaTime * ( WaspOccupiedTiles.Count * WaspSpawnInflationPerTile );
            spawnperc += WaspInflationSum;
        }

        //Map.I.Deb = "HS " + herospeed.ToString( "0.00" ) + " SS " + spawntottime.ToString( "0.00" ) +
        //" TL " + WaspOccupiedTiles.Count + " spawnperc " + spawnperc.ToString( "0.0" ) + "%" + " herotottime " +
        //herotottime.ToString( "0.0" ) + " fat " + fat.ToString( "0.0" ) + " WaspInflationSum " + WaspInflationSum;
        
        int outside = 0;
        for( int u = 0; u < Unit.Body.ChildList.Count; u++ )                                                      // Count outside fire wasps number
        {
            Unit fire = Map.I.GetUnit( ETileType.FIRE, Unit.Body.ChildList[ u ].Pos );
            if( fire == null || !fire.Body.FireIsOn ) outside++;
        }

        if( spawntottime > 0 || Initialized == false )
        if ( SpawnTimer > spawntottime || Unit.Control.ExtraEnragedSpawns > 0 )
        for( int i = 0; i < totspawn; i++ )
        if( SpawnCount < ( int ) MaxSpawnCount )                                                                          // Spawns babies
        if ( Unit.Body.ChildList.Count == 0 || outside >= 1 || Initialized == false )
        {
            Unit un = Map.I.SpawnFlyingUnit( Unit.Pos, ELayerType.MONSTER, ETileType.WASP, Unit, !Initialized );
            if( totspawn == 1 )
            {
                SpawnTimer -= spawntottime;
            }

            SpawnCount++;


            #region old
            //int range = 1;                                                                                  // Immediatelly move spawn to random neighbor tile
            //List<Vector2> tgl = new List<Vector2>();
            //for( int y = ( int ) Unit.Pos.y - range; y <= Unit.Pos.y + range; y++ )
            //for( int x = ( int ) Unit.Pos.x - range; x <= Unit.Pos.x + range; x++ )
            //    {
            //        bool ok = true;
            //        if( Map.IsWall( new Vector2( x, y ) ) ) ok = false;
            //        if( un.Pos == new Vector2( x, y ) ) ok = false;
            //        if( G.Hero.Pos == new Vector2( x, y ) ) ok = false;
            //        if( Sector.GetPosSectorType( new Vector2( x, y ) ) != 
            //            Sector.ESectorType.NORMAL ) ok = false;
            //        if( Map.I.CheckArrowBlockFromTo( Unit.Pos, new Vector2( x, y ), un ) ) ok = false;
            //        if( ok ) tgl.Add( new Vector2( x, y ) );
            //    }
            #endregion

            if( Unit.Body.ChildList.Count >= 1 )                                                      // Creates child over random wasp position
            {
                List<Unit> unl = new List<Unit>();
                for( int u = 0; u < Unit.Body.ChildList.Count - 1; u++ )                                // Dont breed Unit over fire
                {
                    Unit fire = Map.I.GetUnit( ETileType.FIRE, Unit.Body.ChildList[ u ].Pos );
                    if( fire == null || !fire.Body.FireIsOn ) unl.Add( Unit.Body.ChildList[ u ] );
                }

                if( unl.Count > 0 )
                {
                    int id = Random.Range( 0, unl.Count );
                    Unit tgu = unl[ id ];
                    un.Control.JumpTarget = tgu.Control.JumpTarget;
                    un.Control.FlightJumpPhase = tgu.Control.FlightJumpPhase;
                    un.transform.rotation = tgu.transform.rotation;
                    //un.Control.FlightPhaseTimer = tgu.Control.FlightPhaseTimer;
                    un.transform.position = tgu.transform.position;
                    un.Control.UpdateFlyingTile();
                    if( totspawn == 1 ) Message.RedMessage( "+", un.Pos );
                }
            }
        }

        if( SpawnTimer > spawntottime )
            SpawnTimer = spawntottime;

        if( Initialized == false )
        {
            CreateSpecialWasp( Unit.Md.StartShieldedWasps, 1 ); // Minimum special wasp creation
            CreateSpecialWasp( Unit.Md.StartCocoonWasps, 2 );  
            CreateSpecialWasp( Unit.Md.StartEnragedWasps, 3 ); 
        }

        Initialized = true;
        Unit.Control.ExtraEnragedSpawns = 0;
    }

    public void CreateSpecialWasp( int num, int type )
    {
        for( int j = 0; j < num; j++ )
        {
            for( int trial = 0; trial < 999; trial++ )
            {
                int id = Random.Range( 0, Unit.Body.ChildList.Count );
                if( Unit.Body.ChildList == null ) return;
                if( id < Unit.Body.ChildList.Count )
                if( ( type == 1 && Unit.Body.ChildList[ id ].Body.ShieldedWasp == false ) ||
                    ( type == 2 && Unit.Body.ChildList[ id ].Body.CocoonWasp   == false ) ||
                    ( type == 3 && Unit.Body.ChildList[ id ].Body.EnragedWasp  == false ) )
                {
                    Unit.Body.ChildList[ id ].SetSpecialWasp( type, Unit );
                    break;
                }
            }
        }
    }

    public void CalculateWaspDistance()
    {
        Map.I.WaspDist = new int[ Map.I.Tilemap.width, Map.I.Tilemap.height ];
        int id = 0;
        Map.I.ValidWaspTargetList = new List<Vector2>();
        SetWaspDistance( Unit.Pos, Unit.Control.JumpTarget, ++id );
    }

    public bool SetWaspDistance( Vector2 from, Vector2 to, int id )
    {
        if( id > MotherWaspRadius + 5 ) return false;  
        if( Map.PtOnMap( Map.I.Tilemap, to ) == false ) return false;
        Unit ga = Map.I.GetUnit( to, ELayerType.GAIA );
        if( Map.IsWall( to ) ) return false;
        if( from != to )
        if( Map.I.CheckArrowBlockFromTo( from, to, Unit ) == true ) return false;

        Unit mn = Map.I.GetUnit( to, ELayerType.MONSTER );
        if( mn && mn.TileID != ETileType.BARRICADE ) return false;

        if( id < Map.I.WaspDist[ ( int ) to.x, ( int ) to.y ] || 
                 Map.I.WaspDist[ ( int ) to.x, ( int ) to.y ] == 0 )
        {
                 Map.I.WaspDist[ ( int ) to.x, ( int ) to.y ] = id;
                 if( id <= MotherWaspRadius + 1 )
                     Map.I.ValidWaspTargetList.Add( to );
        }
        else return false;
        
        id++;
        SetWaspDistance( to, new Vector2( to.x,     to.y + 1 ), id );
        SetWaspDistance( to, new Vector2( to.x + 1, to.y + 1 ), id );
        SetWaspDistance( to, new Vector2( to.x + 1, to.y     ), id );
        SetWaspDistance( to, new Vector2( to.x + 1, to.y - 1 ), id );
        SetWaspDistance( to, new Vector2( to.x,     to.y - 1 ), id );
        SetWaspDistance( to, new Vector2( to.x - 1, to.y - 1 ), id );
        SetWaspDistance( to, new Vector2( to.x - 1, to.y     ), id );
        SetWaspDistance( to, new Vector2( to.x - 1, to.y + 1 ), id );  
        return true;
    }

    public void UpdateWaspMovement()
    {
        WaspCount++;
        Unit.MeleeAttack.AttackSpeedRate = 100;
        CalculateWaspJump();

        if( Util.IsNeighbor( Unit.Pos, G.Hero.Pos ) )
            MasterAudio.PlaySound3DAtVector3( "Bee Attack", G.Hero.transform.position );                   // neighbor wasp FX
        Unit.Body.UpdateFrameAnimation( 202, 5, true, 0.03f );                                             // Updates Animation
    }
    public void UpdateWaspJumpTarget()
    {
        if( G.HS.CurrentTickNumber != 1 ) return;                                                          // Wasps only move in the first tick while mothers can move in any tick
        if( Map.I.WaspDist == null )
        {
            CalculateWaspDistance();
            //return;
        }

        if( Map.I.ValidWaspTargetList.Contains( G.Hero.Pos ) )
            Map.I.ValidWaspTargetList.Remove( G.Hero.Pos );    

        int range = 1;
        List<Vector2> busy = new List<Vector2>();
        for( int i = 0; i < Unit.Body.ChildList.Count; i++ )
        {
            bool hasfire = false;
            Unit un = Unit.Body.ChildList[ i ];
            List<Vector2> tgl = new List<Vector2>();
            List<Vector2> forcedtgl = new List<Vector2>();
            List<float> hlist = new List<float>();
            List<float> forcedhlist = new List<float>();
            for( int y = ( int ) un.Pos.y - range; y <= un.Pos.y + range; y++ )
            for( int x = ( int ) un.Pos.x - range; x <= un.Pos.x + range; x++ )
                {
                    bool ok = true;
                    if( Util.Manhattan( JumpTarget, new Vector2( x, y ) ) > MotherWaspRadius ) ok = false;
                    if( Map.IsWall( new Vector2( x, y ) ) ) ok = false;
                    int dif = Map.I.WaspDist[ x, y ] - Map.I.WaspDist[ ( int ) un.Pos.x, ( int ) un.Pos.y ];
                    if( Map.I.WaspDist[ ( int ) un.Pos.x, ( int ) un.Pos.y ] != 0 )
                    //if( Mathf.Abs( dif ) > 1 ) ok = false;                                                       // removed because this was causing the arrow bug on wasps cube 3 middle arrow
                    if( Map.I.WaspDist[ x, y ] <= 1 ) ok = false;
                    if( G.Hero.Pos == new Vector2( x, y ) ) ok = false;
                    if( Sector.GetPosSectorType( new Vector2( x, y ) ) !=                                          // remain inside cube
                        Sector.ESectorType.NORMAL ) ok = false;
                    if( Map.I.CheckArrowBlockFromTo( un.Pos, new Vector2( x, y ), un ) ) ok = false;               // arrow block                    
                    Unit ga = Map.I.GetUnit( new Vector2( x, y ), ELayerType.GAIA );
                    if( ga && ga.TileID == ETileType.PIT ) ok = false;
                    Unit mn = Map.I.GetUnit( new Vector2( x, y ), ELayerType.MONSTER );
                    if( mn && mn.TileID != ETileType.BARRICADE ) ok = false;

                    if( ok )
                    {
                        tgl.Add( new Vector2( x, y ) );

                        float height = 0;
                        Unit overbar = Map.I.GetUnit( ETileType.BARRICADE, un.Pos );                         // over barricade
                        Unit bar = Map.I.GetUnit( ETileType.BARRICADE, new Vector2( x, y ) );                // barricade
                        if( bar && bar.Body.TouchCount > 0 )
                        if( overbar && overbar.Body.TouchCount > 0 )
                        {
                            if( bar.Variation == overbar.Variation + 1 )                                     // Wasp "Climb" barricade
                            {
                                height = 10;
                                forcedtgl.Add( new Vector2( x, y ) );
                                forcedhlist.Add( height );
                                if( forcedhlist.Count == 1 )
                                {
                                    WaspClimbBarricadeNumber++;
                                    Message.GreenMessage( "+", new Vector2( x, y ) );
                                }
                            }
                        }

                        Unit fire = Map.I.GetUnit( ETileType.FIRE, new Vector2( x, y ) );                // move to fire
                        height = 10;

                        if( un.Pos == new Vector2( x, y ) ) height = 10;                                 // remain in the same place

                        if( Util.Neighbor( G.Hero.Pos, new Vector2( x, y ) ) )                           // move close to hero (Body odor ability)
                        {            
                            height *= ( 1 + ( WaspClimbBarricadeNumber + 1 ) );
                        }
                        else
                        if( fire && fire.Body.FireIsOn )
                        {
                            height *= ( 1 + WaspClimbBarricadeNumber );
                        }
                        else
                        if( busy.Contains( new Vector2( x, y ) ) == true )  height /= 100f;              // try to avoid wasp busy tiles
                        
                        if( bar && bar.Body.TouchCount > 0 )                                             // Never descend Touched size
                        if( overbar && overbar.Body.TouchCount > 0 )
                        if( bar.Variation < overbar.Variation )
                        {
                            height = 0;
                        }

                        if( un.Control.FireMarkedWaspFactor > 0 )
                        {
                            if( fire && fire.Body.FireIsOn )
                            {
                                height = 10000000000000000000;                                         // Fire Marked Wasp move to Fire
                                forcedtgl.Add( new Vector2( x, y ) );
                                forcedhlist.Add( height );
                                hasfire = true;
                            }
                            else
                            if( Util.Neighbor( G.Hero.Pos, new Vector2( x, y ) ) )                      // Fire marked move to hero neighbor tile
                            {
                                height = 100;
                                forcedtgl.Add( new Vector2( x, y ) );
                                forcedhlist.Add( height );
                            }
                        }

                        if( Util.Neighbor( un.Pos, G.Hero.Pos ) && height > 1000 )                      // neigh wasp remain in place to attack hero if theres no fire around
                        {
                            height = 0;
                        }



                        hlist.Add( height );
                    }
                }

            if ( hasfire )                                                                               // if theres a fire neighboring a marked wasp do not move to hero neighbor tile
            for( int ft = 0; ft < forcedhlist.Count; ft++ )
            if( forcedhlist[ ft ] == 100 )
                forcedhlist[ ft ] = 0;

            if( forcedtgl.Count > 0 )                                                                    // forced moves
            {
                tgl = new List<Vector2>();
                tgl.AddRange( forcedtgl );
                hlist = new List<float>();
                hlist.AddRange( forcedhlist );
            }

            if( tgl.Count == 0 )                                                                          // No possible move, stay in place
            {
                tgl.Add( un.Pos );
                hlist.Add( 100 );
            }
            int id = Util.Sort( hlist );
            un.Control.JumpTarget = tgl[ id ];
            busy.Add( un.Control.JumpTarget );

            if( Unit.Md.WaspCatchUp )
            if( Util.Manhattan( JumpTarget, un.Pos ) > MotherWaspRadius )
            //if( Map.I.WaspDist[ ( int ) un.Pos.x, ( int ) un.Pos.y ] > MotherWaspRadius )             // Help forgotten wasps to catch up moving mothers      
            {
                List<Vector2> targetList = new List<Vector2>();
                targetList.AddRange( Map.I.ValidWaspTargetList );

                //for( int j = targetList.Count - 1; j >= 0; j-- )                                        // Only fly to wasp occupied tiles to avoid hero step death
                //if ( Map.I.GF( targetList[ j ], ETileType.WASP ) == null )
                //     targetList.RemoveAt( j );

                if( targetList.Count > 0 )                                                              // Sorts new fly jump target
                {
                    int idd = Random.Range( 0, targetList.Count );
                    un.Control.JumpTarget = targetList[ idd ];
                }
                else
                {
                    un.Control.JumpTarget = un.Control.Mother.Pos;
                } 
            }
            Unit overfire = Map.I.GetUnit( ETileType.FIRE, un.Pos );                                     // unit over fire remain in position
            if( overfire && overfire.Body.FireIsOn )
                un.Control.JumpTarget = un.Pos;
            un.Control.WaspLongFlight = true;
            un.Control.FlightJumpPhase = 1;
            un.Control.FlightPhaseTimer = 0;
            if( un.Control.FireMarkedWaspFactor > 0 )                                                     // Fire mark factor decrement
            if( --un.Control.FireMarkedWaspFactor == 0 )
                un.Control.FireMarkedWaspFactor = -1;          

            un.Control.FlightSpeed = 3 * Vector2.Distance( un.transform.position, un.Control.JumpTarget );
        }
        Map.I.WaspDist = null;
        Map.I.ValidWaspTargetList = null;
    }

    public void CalculateWaspJump()
    {
        if( FlightJumpPhase == 0 )
        {
            Quaternion qn = Util.GetRotationToPoint( Unit.Spr.transform.position, G.Hero.Pos );                                                            // Rotates Wasp Towards hero
            Unit.Spr.transform.rotation = qn;

            if( Unit.transform.position.x == JumpTarget.x &&
                Unit.transform.position.y == JumpTarget.y )
            {
                float rad = .3f;
                Vector2 randv = new Vector2( Random.Range( -rad, +rad ), Random.Range( -rad, +rad ) );
                Unit.Control.JumpTarget = Unit.Pos + randv;
            }
            else
            {
                if( JumpTarget != new Vector2( 0, 0 ) )
                {
                    float flightspeed = 1;
                    Unit.transform.position = Vector3.MoveTowards( Unit.transform.position, JumpTarget, Time.deltaTime * flightspeed );
                }
            }
        }
        else
        if( FlightJumpPhase == 1 )                                                                                                                         // Wasp Fliimg Phase
        {
            float spd = FlightSpeed;
            Unit.transform.position = Vector3.MoveTowards( Unit.transform.position, JumpTarget, Time.deltaTime * spd );
            Quaternion qn = Util.GetRotationToPoint( Unit.Spr.transform.position, G.Hero.Pos );                                                            // Rotates Wasp Towards hero
            Unit.Spr.transform.rotation = qn;
            UpdateFlyingTile();

            if( Unit.transform.position.x == JumpTarget.x )                                                                                                   
            if( Unit.transform.position.y == JumpTarget.y )
                {
                    FlightPhaseTimer = 0;
                    FlightJumpPhase = 0;
                    BeingMudPushed = false;
                    Unit.Control.WaspLongFlight = false;
                }
        }

        if( FlightPhaseTimer > 1 )                                                            // Burn Wasps over fire
        {
            Unit fire = Map.I.GetFire( Unit.Pos, true );
            if( fire )
            {
                Unit.Body.TakeFireDamage( -1, -1, false );
            }
        }
    }
    public void UpdateFlyingTile()
    {
        int posx = 0, posy = 0;
        Map.I.Tilemap.GetTileAtPosition( Unit.transform.position, out posx, out posy );
        TileChanged = false;
        if( Unit.Pos != new Vector2( posx, posy ) )
        {
            TileChanged = true;
            OldPos = Unit.Pos;
            AnimationOrigin = Unit.Pos; 
            UpdatePassiveAttack( posx, posy );
        }
        Unit.Pos = new Vector2( ( int ) posx, ( int ) posy );
        if( Unit.Pos != FlyingTarget )
            LastFlownOverTile = Unit.Pos;

        if( Map.PtOnMap( Map.I.Tilemap, new Vector2( posx, posy ) ) )
        if( TileChanged )
        {
            if( Map.I.FUnit[ posx, posy ] == null )
                Map.I.FUnit[ posx, posy ] = new List<Unit>();
            Map.I.FUnit[ posx, posy ].Add( Unit );
            Map.I.FUnit[ ( int ) OldPos.x, ( int ) OldPos.y ].Remove( Unit );
            FireMarkAvailable = true;
        }

        if ( Unit.Body.RopeConnectFather != null )
        for( int f = 0; f < Unit.Body.RopeConnectFather.Count; f++ )
             Unit.Body.RopeConnectFather[ f ].UpdateChainSizes( Unit.transform.position );                      // Update chain if connected
    }

    public void UpdatePassiveAttack( int posx, int posy )
    {
        if( Unit.ValidMonster )
        {
            List<Unit> bm = Map.I.GProj( new Vector2( posx, posy ), EProjectileType.BOOMERANG );  // Passive Boomerang Attack
            if( bm != null )
                for( int i = 0; i < bm.Count; i++ )
                {
                    Unit boom = bm[ i ];
                    PassiveBoomerangAttackTg = Unit;
                    boom.RangedAttack.UpdateIt( true );
                    PassiveBoomerangAttackTg = null;
                    Map.I.UpdatePostBoomerangAttack( ref boom );
                }
        }
    }
    #endregion

    #region Raft 

    public void NextDynamicStep( bool blocked )
    {
        Vector2 dest = Vector2.zero;                                                                           // Dynamic Object Movement
        int destRot = 0;
        EDirection destRotTo = EDirection.NONE;

        if( blocked == false )
            if( RaftMoveDir != EDirection.NONE)
            if( MovementSorting == EDynamicObjectSortingType.SEQUENCE_CHANGE_ON_OBSTACLE )
                return;
        bool res = UpdateDynamicObjectMovement( false, ref dest, ref destRot, ref destRotTo );
        EDirection dr = Util.GetTargetUnitDir( Unit.Pos, dest );
        RaftMoveDir = dr;
    }
    public bool UpdateRaftMovement( int type = 1, bool justTest = false )                                     // type water == 1  pit == 2  lava == 3 
    {
        float tottime = GetRealtimeSpeedTime();
        bool pit = false;
        if( type == 2 ) pit = true;

        if( IsDynamicObject() )                                                                               // Initializes Dynamic Raft Move List
        if( RaftMoveDir == EDirection.NONE || 
            FlightPhaseTimer > tottime )
        {
            NextDynamicStep( false );
        }

        if( type == 1 )
        if( Map.I.GetUnit( ETileType.PIT,  G.Hero.Pos ) != null ||                                            // stops raft if water moving raft moves hero over pit or lava
            Map.I.GetUnit( ETileType.LAVA, G.Hero.Pos ) != null )
            SetRaftDirection( true, RaftGroupID, EDirection.NONE );

        if( RaftMoveDir == EDirection.NONE ) return false;
        RaftMoving = true;
        List<EDirection> waterArrowDir = new List<EDirection>();
        List<Unit> waterArrow = new List<Unit>();

        if( FlightPhaseTimer > tottime || type > 1 )
        {
            bool moveok = true;
            List<Unit> rl = new List<Unit>();
            List<Unit> ga2l = new List<Unit>();
            List<Unit> mnl = new List<Unit>();
            List<Unit> fll = new List<Unit>();
            IgnoreLevelPushList = new List<Unit>();
            List<Vector2> tgl = Util.GetDirectionalSectorLoopCords( RaftMoveDir );

            for( int i = 0; i < tgl.Count; i++ )
                {
                    int x = ( int ) tgl[ i ].x;
                    int y = ( int ) tgl[ i ].y;
                    Unit tgRaft = GetRaft( new Vector2( x, y ) );
                    if( tgRaft )
                    {
                        if( RaftGroupID == tgRaft.Control.RaftGroupID )
                        {
                            rl.Add( tgRaft );                                                                          // Checks Raft Collision
                            Vector2 tg = tgRaft.Pos + Manager.I.U.DirCord[ ( int ) RaftMoveDir ];
                            Unit water = Map.I.GetUnit( ETileType.WATER, tg );
                            Unit ispit = Map.I.GetUnit( ETileType.PIT, tg );
                            Unit lava = Map.I.GetUnit( ETileType.LAVA, tg );

                            Unit raft = GetRaft( tg );
                            if( ( water == null && ispit == null && lava == null ) ||
                              ( raft && raft.Control.RaftGroupID != RaftGroupID ) )
                            {
                                moveok = false;
                            }

                            Unit mine = Map.GFU( ETileType.MINE, new Vector2( x, y ) );                                // Check if any lever is blocking raft movement
                            if( mine )
                            if( mine.CheckLeverBlock( false, mine.Pos, tg, false ) )
                                moveok = false;

                            Unit orb = Map.I.GetUnit( ETileType.ORB, tg );                                            // Check if orb is blocking raft movement
                            if( orb && raft == null ) 
                                moveok = false;

                            Unit fire = Map.I.GetUnit( ETileType.FIRE, new Vector2( x, y ) );                                        
                            if( fire )                                                                                // Fire Log Block  
                            {
                                bool block = fire.CheckFirepitLogBlock( fire.Pos, tg );
                                if( block ) moveok = false;
                            }

                            Unit arrow = Map.I.GetUnit( tg, ELayerType.GAIA2 );                                       // Arrow Over Water for raft direction change
                            if( arrow && arrow.TileID == ETileType.ARROW )
                            if( raft == null && water != null )
                                {
                                    if( arrow.Body.isWorking )
                                    {
                                        if( !waterArrowDir.Contains( arrow.Dir ) )
                                        {
                                            waterArrowDir.Add( arrow.Dir );
                                        }
                                        waterArrow.Add( arrow );
                                    }
                                }

                            if( G.Hero.Pos == new Vector2( x, y ) )
                            if( G.Hero.CheckBambooBlock( false, G.Hero.Pos, tg ) )                                     // Bamboo Block
                                moveok = false;
                            Unit ga2 = Map.I.GetUnit( new Vector2( x, y ), ELayerType.GAIA2 );                         // Adds Gaia2 obj
                            if( ga2 )
                            {
                                ga2.Body.RaftResource = true;
                                ga2l.Add( ga2 );
                            }
                            Unit mn = Map.I.GetUnit( new Vector2( x, y ), ELayerType.MONSTER );                        // Adds monsters
                            if( mn ) mnl.Add( mn );                                                       

                            List<Unit> fl = Map.I.GetFUnit( new Vector2( x, y ) );
                            if ( fl != null )
                            for( int f = 0; f < fl.Count; f++ )
                            if ( fl[ f ].TileID == ETileType.SPIKES ||
                                 fl[ f ].TileID == ETileType.MINE   ||
                                 fl[ f ].TileID == ETileType.HERB   ||
                                 fl[ f ].TileID == ETileType.VINES  ||
                                 fl[ f ].TileID == ETileType.ORB    ||
                                 fl[ f ].TileID == ETileType.MUD_SPLASH ||
                                 fl[ f ].TileID == ETileType.FISHING_POLE )
                            {
                                 fll.Add( fl[ f ] );
                                 if( fl[ f ].TileID == ETileType.MINE )
                                     IgnoreLevelPushList.Add( fl[ f ] );                                            // this is a list of lever mines to not be pulled since they are raft moving
                            }
                        }
                    }
                }

            if( moveok == false )                                                                                   // Move Blocked
            {
                if( IsDynamicObject() )                                                                             // Dynamic Raft
                {
                        
                    if( MovementSorting == EDynamicObjectSortingType.SEQUENCE_CHANGE_ON_OBSTACLE )
                    {
                        NextDynamicStep( true );
                        //if( DynamicObjectMoveList != null && DynamicObjectMoveList.Count > 0 )                                  // Dynamic Raft
                        //{
                        //    if( ++DynamicStepCount >= DynamicObjectMoveList.Count )
                        //        DynamicStepCount = 0;
                        //    EDirection dr = Util.FlipDir( ( EDirection ) DynamicObjectMoveList[
                        //    DynamicStepCount ], Map.I.RM.HeroSector.FlipX, Map.I.RM.HeroSector.FlipY );
                        //    RaftMoveDir = dr;
                        //}
                    }
                    else
                    {
                        FlightPhaseTimer = 0;
                        SetMovementDynamicCounters( -1 );                                                                   // Blocked raft remains in place until deobstructed
                    }
                }
                else                                                                                                        // Normal Raft
                {
                    EDirection inv = Util.GetInvDir( RaftMoveDir );
                    waterArrow = null;
                    waterArrowDir = null;
                    Map.I.RaftDirectionChangeAvailable = true;
                    if( pit == false )
                        SetRaftDirection( false, RaftGroupID, inv );                                                       // Inverts Raft movement direction on water
                    else
                        SetRaftDirection( false, RaftGroupID, EDirection.NONE );                                           // Nullifies move direction over pit   
                    if( Map.I.RaftTraverseDistance == 0 )
                        SetRaftDirection( false, RaftGroupID, EDirection.NONE );                                           // prevents infinite back and forth raft bumping
                    Map.I.RaftTraverseDistance = 0;
                }
                RaftMoving = false;
                return false;
            }
            if( justTest ) return true;
            if( waterArrow!= null )                                                                                         // kill Water Arrows
            if( waterArrow.Count >= 0 )
            for( int i = 0; i < waterArrow.Count; i++ )
            waterArrow[ i ].Kill();   

            List<Unit> pm = new List<Unit>();
            for( int i = 0; i < rl.Count; i++ )
            {
                Vector2 mov =  Manager.I.U.DirCord[ ( int ) RaftMoveDir ];
                Unit r = rl[ i ];
                Vector2 tg = r.Pos + mov;                                                                          // Moves Gameobject

                r.Graphic.transform.position = new Vector3( r.Pos.x, r.Pos.y, r.Graphic.transform.position.z );
                r.transform.position = new Vector3( tg.x, tg.y, r.transform.position.z );
                r.Control.NotAnimatedPosition = r.Graphic.transform.position;

                Vector2 prtg = tg + mov;
                if( GetRaft( prtg ) == null )
                if( Map.I.GetUnit( ETileType.WATER, tg ) != null ) 
                {
                    int particleCount = 2;                                                                         // quantas partículas você quer criar
                    float randomOffset = .3f;                                                                      // máximo de deslocamento aleatório em x e y
                    for( int j = 0; j < particleCount; j++ )
                    {
                        Vector2 splashPos = tg + ( prtg - tg ) * .3f;
                        Vector2 randomShift = new Vector2(
                        UnityEngine.Random.Range( -randomOffset, randomOffset ),
                        UnityEngine.Random.Range( -randomOffset, randomOffset ) );
                        Util.PlayParticleFX( "Water Splash", splashPos + randomShift );                             // water splash particle
                    }
                }

                if( r.Control.FlightPhaseTimer > tottime )                                                          // Timer Stuff
                    r.Control.FlightPhaseTimer = tottime;
                r.Control.FlightPhaseTimer -= tottime;
                r.Control.SmoothMovementFactor = 0;
                r.Control.TurnTime = 0;
                Controller.MoveMade = true;
                                                 
                bool rock = IsRock( tg );                                                                         // Any rock to sink the raft
 
                Vector2 htg = tg;
                if( rock )                                                                                        // Raft sinks
                {
                    Map.Kill( r );
                    if( r.Pos == G.Hero.Pos )
                    {
                        Vector2 tar = G.Hero.Pos - Manager.I.U.DirCord[ ( int ) RaftMoveDir ];
                        if( G.Hero.CanMoveFromTo( false, G.Hero.Pos, tar, r ) == false || 
                            Map.I.GetUnit( tar, ELayerType.GAIA2 ) != null )
                            Map.I.StartCubeDeath();
                        else 
                        { 
                            htg = r.Pos;
                        }
                    }
                    FallingAnimation.Create( 75, 1, new Vector3( tg.x, tg.y, -0.33f ), r.transform.eulerAngles, .1f );
                    MasterAudio.PlaySound3DAtVector3( "Wood Break", r.transform.position );
                    Controller.CreateMagicEffect( tg, "Mine Debris" );                                              // Debris FX
                    Map.I.CreateExplosionFX( tg );
                }
 
                if( pm.Contains( G.Hero ) == false )                                                                // Moves Hero
                if( G.Hero.Pos == r.Pos )
                    {
                       bool res = G.Hero.CheckShacklePulling( true, G.Hero.Pos, htg );                              // Shackle Pulling
                       if( res ) Map.I.StartCubeDeath();
                        if( G.Hero.CheckLeverBlock( true, G.Hero.Pos, htg, true ) )
                            Map.I.StartCubeDeath();
                        G.Hero.Control.AnimationOrigin = G.Hero.Pos; 
                        G.Hero.Graphic.transform.position = new Vector3( 
                        G.Hero.Pos.x, G.Hero.Pos.y, G.Hero.Graphic.transform.position.z );
                        G.Hero.transform.position = new Vector3( htg.x, htg.y, G.Hero.transform.position.z );
                        G.Hero.CheckBambooBlock( true, G.Hero.Pos, htg );                                          // Bamboo
                        G.Hero.Control.ApplyMove( G.Hero.Pos, htg, true );
                        G.Hero.Control.OldPos = htg;
                        G.Hero.Control.LastPos = htg;
                        G.Hero.Control.SmoothMovementFactor = 0;
                        G.Hero.Control.TurnTime = 0;
                        G.Hero.Control.UnitMoved = false;  
                        UpdatePushedHero( false );
                        pm.Add( G.Hero );
                    }
      
                Unit ga2 = Map.I.GetUnit( tg, ELayerType.GAIA2 );                                                  // Destroy uncollected water resource
                if( ga2 && ga2.TileID == ETileType.ITEM )
                if( ga2.Body.RaftResource == false )
                if( ga2.Pos != G.Hero.Pos )
                    ga2.Kill();

                int posx = 0, posy = 0;                                                                            // Tile Calculation
                Map.I.Tilemap.GetTileAtPosition( tg, out posx, out posy );
                r.Control.TileChanged = false;
                if( r.Pos != new Vector2( posx, posy ) )
                {
                    r.Control.TileChanged = true;
                    r.Control.OldPos = r.Pos;
                    r.Control.LastPos = r.Pos;
                    r.Control.AnimationOrigin = r.Pos;

                    if( Map.I.HookList != null )                                                                  
                    for( int h = 0; h < Map.I.HookList.Count; h++ )
                    if ( Map.I.HookList[ h ].gameObject.activeSelf )
                    if ( Map.I.HookList[ h ].TilePos == tg )
                    {
                        if( G.HS.RaftDestroy || Water.TempRaftDestroy )                                            // Hook bonus Raft destroy
                            Water.DestroyRaft( r );
                        else
                            Map.I.HookList[ h ].Deactivate();                                                       // Raft over fishing hooks deactivation
                    }
                }
                r.Pos = new Vector2( posx, posy );

                if( r.Control.TileChanged )                                                                         // Tile Changed
                {
                    if( Map.I.FUnit[ posx, posy ] == null )
                        Map.I.FUnit[ posx, posy ] = new List<Unit>();
                    Map.I.FUnit[ posx, posy ].Add( r );
                    Map.I.FUnit[ ( int ) r.Control.OldPos.x, ( int ) r.Control.OldPos.y ].Remove( r );
                    MasterAudio.PlaySound3DAtVector3( "Raft Move", r.Pos );                                         // Sound FX

                    Vector2 tg2 = tg +  Manager.I.U.DirCord[ ( int ) RaftMoveDir ];
                    Unit water2 = Map.I.GetUnit( ETileType.WATER, tg2 );
                    Unit pit2 = Map.I.GetUnit( ETileType.PIT, tg2 );
                    Unit lava2 = Map.I.GetUnit( ETileType.LAVA, tg2 );

                    if( water2 == null && pit2 == null && lava2 == null )
                    if( pit == false )
                    {
                        Map.I.CreateExplosionFX( tg, "Smoke Cloud", "" );                                         // Smoke Cloud FX
                        MasterAudio.PlaySound3DAtVector3( "Raft Bump", tg );                                      // Raft Bump Sound FX
                    }
                }
            }

            for( int i = 0; i < ga2l.Count; i++ )
            {
                Map.I.Gaia2[ ( int ) ga2l[ i ].Pos.x, ( int ) ga2l[ i ].Pos.y ] = null;
            }

            for( int i = 0; i < ga2l.Count; i++ )                                                                        // Moves Gaia 2 Objects
            {
                Unit ga = ga2l[ i ];
                Vector2 old = ga.Pos;
                Vector2 tg = ga.Pos + Manager.I.U.DirCord[ ( int ) RaftMoveDir ];
                ga.transform.position = new Vector3( tg.x, tg.y, ga.transform.position.z );
                ga.Graphic.transform.position = new Vector3( old.x, old.y, ga.Graphic.transform.position.z );
                ga.Control.ApplyMove( new Vector2( -1, -1 ), tg );
                ga.Control.OldPos = old;
                ga.Control.LastPos = old;
                ga.Control.AnimationOrigin = old;
                ga.Control.SmoothMovementFactor = 0;
                ga.Control.TurnTime = 0;
                ga.Control.UnitMoved = false;                                                                             // unit was pushed, can cause bugs if true like resetting scarab timer attack (att wont work)           
                if( IsRock( tg ) ) Map.Kill( ga );
            }

            G.Hero.Control.IsBeingPushed = true;
            G.Hero.Control.UpdateResourceCollecting( G.Hero.Pos, false );
            G.Hero.Control.IsBeingPushed = false;
            Map.I.RaftTraverseDistance++;

            List<Unit> blocklist = new List<Unit>();
            for( int i = mnl.Count - 1; i >= 0; i-- )
            if ( mnl[ i ].ValidMonster == true && pit == true )                                                           // Check for monster block while dragging raft over pit
            {
                EDirection inv = Util.GetInvDir( RaftMoveDir );
                Vector2 tg = mnl[ i ].Pos + Manager.I.U.DirCord[ ( int ) inv ];
                bool res = Util.CheckBlock( mnl[ i ].Pos, tg, mnl[ i ], true, false, true, true, true, true, null );                  
                if( res && GetRaft( tg ) != null )  
                    blocklist.Add( mnl[ i ] );                                                                            // Adds to a block list and moves normally like other objects over raft
            }

            for( int i = 0; i < mnl.Count; i++ )
            if ( mnl[ i ].ValidMonster == false || pit == false || blocklist.Contains( mnl[ i ] ) )
            {
                Map.I.Unit[ ( int ) mnl[ i ].Pos.x, ( int ) mnl[ i ].Pos.y ] = null;
            }

            for( int i = 0; i < mnl.Count; i++ )                                                                          // Moves Monsters Objects
            if ( mnl[ i ].ValidMonster == false || pit == false || blocklist.Contains( mnl[ i ] ) )
            {
                Unit mn = mnl[ i ];
                Vector2 old = mn.Pos;
                Vector2 tg = mn.Pos + Manager.I.U.DirCord[ ( int ) RaftMoveDir ];
                mn.transform.position = new Vector3( tg.x, tg.y, mn.transform.position.z );
                mn.Graphic.transform.position = new Vector3( old.x, old.y, mn.Graphic.transform.position.z );
                mn.Control.ApplyMove( new Vector2( -1, -1 ), tg );
                mn.Control.OldPos = old;
                mn.Control.LastPos = old;
                mn.Control.AnimationOrigin = old;
                //mn.Control.SmoothMovementFactor = 0;                                                                    // removed to avoid the monster and raft moving toguether bug
                mn.Control.TurnTime = 0;
                mn.Control.UnitMoved = false;                                                                             // unit was pushed, can cause bugs if true like resetting scarab timer attack (att wont work)

                if( mn.TileID == ETileType.SLIME )
                if( mn.Control.JumpTarget != new Vector2( -1, -1 ) )
                    mn.Control.JumpTarget += Manager.I.U.DirCord[ ( int ) RaftMoveDir ];                                  // this solves the slime over raft bug by moving jump target too

                if( mn.TileID == ETileType.MINE )                                                                         // Mine chain
                {
                    if( mn.Body.RopeConnectSon != null )
                        mn.UpdateChainSizes( mn.Body.RopeConnectSon.transform.position );
                    for( int f = 0; f < mn.Body.RopeConnectFather.Count; f++ )
                        mn.Body.RopeConnectFather[ f ].UpdateChainSizes( tg );
                }
                iTween.Stop( mn.Graphic.gameObject );
                if( IsRock( tg ) ) Map.Kill( mn );
            }
            else
            {
                Map.I.RaftDragTurnTime = 0;
            }

            for( int i = 0; i < fll.Count; i++ )                                                                          // Moves Flying Objects
            {
                Unit fl = fll[ i ];
                Vector2 old = fl.Pos;
                Vector2 tg = fl.Pos + Manager.I.U.DirCord[ ( int ) RaftMoveDir ];
                MoveFlyingUnitTo( ref fl, old, tg );
                fl.transform.position = new Vector3( tg.x, tg.y, fl.transform.position.z );
                fl.Graphic.transform.position = new Vector3( old.x, old.y, fl.Graphic.transform.position.z );
                //fl.Control.FlyingTarget -= Manager.I.U.DirCord[ ( int ) RaftMoveDir ];                             // new removed to avoid over raft pole move bug
                fl.Control.OldPos = old;
                fl.Control.LastPos = old;
                fl.Control.AnimationOrigin = old;
                fl.Control.SmoothMovementFactor = 0;
                fl.Control.TurnTime = 0;
                fl.Control.UnitMoved = false;                                                                             // unit was pushed, can cause bugs if true like resetting scarab timer attack (att wont work)
                iTween.Stop( fl.Graphic.gameObject );
                if( fl.TileID == ETileType.VINES )
                    MovedVineList.Add( fl );                                                                              // Adds the vine to a list to check if it should be ignited             
                if( IsRock( tg ) ) Map.Kill( fl );
            }

            //for( int i = 0; i < fll.Count; i++ )                                                                        // Checks herb matches
            //{
            //    Unit fl = fll[ i ];
            //    bool res = Map.I.CheckHerbMatchAtPos( fl.Pos );
            //    if( res ) fl.Control.SmoothMovementFactor = 1;
            //}

            Map.I.UpdateFireIgnition( true );                                                                             // Updates Fire ignition for land firepits

            UpdateRaftMerging( rl );                                                                                      // Update raft merging

            if( waterArrowDir != null )                                                                                   // Changes Raft direction for Over Water Arrows
            if( waterArrowDir.Count >= 1 && waterArrowDir.Count <= 1 )
                {
                    EDirection dr = waterArrowDir[ 0 ];
                    for( int i = 0; i < rl.Count; i++ )
                        rl[ i ].Control.RaftMoveDir = dr;
                }

            for( int i = 0; i < mnl.Count; i++ )                                                                          // Kill Monsters left behind (falling)
            if ( mnl[ i ].ValidMonster == true && pit )
                {
                    Unit un = mnl[ i ];
                    if( GetRaft( mnl[ i ].Pos ) == null )
                    {
                        Map.Kill( un );
                        int spr = un.Spr.spriteId;
                        if( un.TileID == ETileType.SCARAB ) spr = 63;
                        FallingAnimation.CreateAnim( spr, 1, new Vector3(
                        un.Pos.x, un.Pos.y, -0.18f ),
                        un.Spr.transform.eulerAngles );
                        un.MeleeAttack.InvalidateAttackTimer = 1;
                        MasterAudio.PlaySound3DAtVector3( "Monster Falling", un.Pos );                // Monster Falling Sound Effect
                    }
                    else
                    {
                        mnl[ i ].Control.SpeedTimeCounter = 0;                                        // theres a raft behind him in this case
                    }
                }

            if( pit == true )                                                                         // Raft moving over pit
            {
                RaftStepCount--;
                if( RaftStepCount == 0 )
                {
                    RaftStepCount = -1;
                    SetRaftDirection( false, RaftGroupID, EDirection.NONE );                           // Stops after one step
                }
            }        
        }

        if( GetRaft( G.Hero.Pos ) == null )                                                            // Drown hero
        if( Map.I.GetUnit( ETileType.WATER, G.Hero.Pos ) )
            Map.I.StartCubeDeath(); 

        RaftMoving = false;
        IgnoreLevelPushList = new List<Unit>();
        return true;
    }

    private void UpdateRaftMerging( List<Unit> rl )
    {
        bool snd = false;
        List<GameObject> jointl = new List<GameObject>();
        List<Unit> processed = new List<Unit>();
        EDirection inverse = Util.GetInvDir( RaftMoveDir );
        for( int l = 0; l < rl.Count; l++ )                                                          // Raft Merging
        {
            int x = ( int ) rl[ l ].Pos.x;
            int y = ( int ) rl[ l ].Pos.y;
            Unit raft = GetRaft( new Vector2( x, y ) );
            Unit merge = null;
            int otherid = -1;
            if( raft )
            {
                if ( raft.Control.RaftJointList != null )                                             // 1) check joints of current raft
                for( int i = 0; i < raft.Control.RaftJointList.Count; i++ )                           // loop joints
                if ( raft.Control.RaftJointList[ i ].gameObject.activeSelf )                          // joint is active
                {
                    Vector2 tg = new Vector2( x, y ) + Manager.I.U.DirCord[ i ];
                    Unit tgRaft = GetRaft( tg );
                    if( tgRaft && raft.Control.RaftGroupID != tgRaft.Control.RaftGroupID )            // different groups
                    {
                        merge = raft;
                        otherid = tgRaft.Control.RaftGroupID;
                        jointl.Add( raft.Control.RaftJointList[ i ].gameObject );
                    }
                }

                if( merge == null )                                                                   // 2) check joints of neighbor raft
                {
                    for( int i = 0; i < 8; i++ )                                                      // 8 directions
                    {
                        Vector2 tg = new Vector2( x, y ) + Manager.I.U.DirCord[ i ];
                        Unit tgRaft = GetRaft( tg );
                        if( tgRaft && raft.Control.RaftGroupID != tgRaft.Control.RaftGroupID )       // found neighbor raft in other group
                        {
                            EDirection inv = Util.GetInvDir( ( EDirection ) i );
                            if( tgRaft.Control.RaftJointList != null &&
                                tgRaft.Control.RaftJointList[ ( int ) inv ].gameObject.activeSelf )  // neighbor joint is active
                            {
                                merge = tgRaft;
                                otherid = tgRaft.Control.RaftGroupID;
                                jointl.Add( tgRaft.Control.RaftJointList[ ( int ) inv ].gameObject );
                            }
                        }
                    }
                }
                if( merge != null )                                                                  // merge groups if connected
                {
                    List<Unit> ral = rl.CloneList();
                    ral.AddRange( GetRaftList( otherid ) );
                    {
                        for( int j = 0; j < ral.Count; j++ )                                        // update all raft units
                        if ( processed.Contains( ral[ j ] ) == false )
                        {
                            ral[ j ].Control.RaftGroupID = raft.Control.RaftGroupID;
                            ral[ j ].Control.RaftMoveDir = inverse;
                            ral[ j ].Control.FlightPhaseTimer = FlightPhaseTimer;
                            ral[ j ].Control.DynamicObjectMoveList = DynamicObjectMoveList;
                            processed.Add( ral[ j ] );
                            snd = true;
                        }
                    }
                }
            }
        }
        if( snd ) MasterAudio.PlaySound3DAtVector3( "Raft Merge", G.Hero.Pos );                         // fx

        for( int i = 0; i < jointl.Count; i++ )
            iTween.ShakeScale( jointl[ i ], new Vector3( .2f, .2f, 0 ), .7f );                          // shake joint FX
    }

    public static bool IsRock( Vector2 tg )
    {
        List<Unit> algae = Map.I.GetFUnit( tg, ETileType.ALGAE );                                       // Any rock to sink the raft
        if( algae != null )
        for( int j = 0; j < algae.Count; j++ )
            {
                for( int k = 0; k < algae[ j ].Body.BabyDataList.Count; k++ )
                if ( algae[ j ].Body.BabyDataList[ k ].BabyType == EAlgaeBabyType.ROCK )
                     return true;
            }
        return false;
    }

    public static bool UpdateRaftOverPitMovement( Vector2 from, Vector2 to )
    {
        if( Manager.I.GameType == EGameType.NAVIGATION ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;

        if( from == to ) return false;
        Unit mine = Map.GFU( ETileType.MINE, to );                                                      // Needs No mine in the target
        if( mine != null ) return false;
        Unit toRaft = GetRaft( to );
        Unit fromRaft = GetRaft( from );
        Unit pitfr = Map.I.GetUnit( ETileType.PIT, from );
        Unit pitto = Map.I.GetUnit( ETileType.PIT, to );
        EDirection step = Util.GetTargetUnitDir( from, to );
        if( pitfr && G.Hero.Control.Floor == 0 )                                                        // Raft over Pit moves step by step
        {
            if( pitto && toRaft == null )
            {
                fromRaft.Control.SetRaftDirection( true, fromRaft.Control.RaftGroupID, step );
                RaftStepCount = 1;                                                                     // number of steps
                List<Unit> rl = GetRaftList( fromRaft.Control.RaftGroupID );
                for( int i = 0; i < rl.Count; i++ )
                {
                    rl[ i ].Control.UpdateRaftMovement( 2 );                                           // Moves the rafts over pit
                }
                return true;
            }
        }
        return false;
    }

    public static Vector2 HeroWallGrabTg;
    public static bool UpdateRaftOverLavaMovement( Vector2 from, Vector2 to, int rot )
    {
        if( Manager.I.GameType == EGameType.NAVIGATION ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;
        if( from == to && rot == 0 ) return false;
        if( G.Hero.Control.Floor > 0 ) return false;                                                  // suspended hero            
        Unit mine = Map.GFU( ETileType.MINE, to );                                                    // Needs No mine in the target
        if( mine != null ) return false;
        Unit toRaft = GetRaft( to );
        Unit fromRaft = GetRaft( from );
        Unit lavafr = Map.I.GetUnit( ETileType.LAVA, from );
        Unit lavato = Map.I.GetUnit( ETileType.LAVA, to );
        EDirection step = Util.GetTargetUnitDir( from, to );
        bool move = false;
        bool ok0 = false, ok1 = false, ok2 = false;
        List<EDirection> drl = new List<EDirection>();
        Unit raft = toRaft;
        List<Vector2> tgl = new List<Vector2>();

        if( HeroWallGrabTg.x != -1 )
        if( rot != 0 )
        {
            EDirection hdr = Util.RotateDir( ( int ) G.Hero.Dir, -rot );
            bool wall = Map.IsWall( G.Hero.Pos + Manager.I.U.DirCord[ ( int ) hdr ] );                        // Wall grab move type
            if( wall )
            { 
                EDirection dr = EDirection.NONE;
                if( hdr == EDirection.N )  if( rot == -1 ) dr = EDirection.E; else dr = EDirection.W; else
                if( hdr == EDirection.S )  if( rot == -1 ) dr = EDirection.W; else dr = EDirection.E; else
                if( hdr == EDirection.E )  if( rot == -1 ) dr = EDirection.S; else dr = EDirection.N; else
                if( hdr == EDirection.W )  if( rot == -1 ) dr = EDirection.N; else dr = EDirection.S; else
                if( hdr == EDirection.NE ) if( rot == -1 ) dr = EDirection.E; else dr = EDirection.N; else
                if( hdr == EDirection.SE ) if( rot == -1 ) dr = EDirection.S; else dr = EDirection.E; else
                if( hdr == EDirection.NW ) if( rot == -1 ) dr = EDirection.N; else dr = EDirection.W; else
                if( hdr == EDirection.SW ) if( rot == -1 ) dr = EDirection.W; else dr = EDirection.S;
                if( dr == EDirection.NONE ) return false;
                step = dr;
            }
        }

        Map.I.GetMimicTargets( ref tgl, step );
        int type = 0; 

        if( lavafr == null && lavato ) type = 1;                                                       // type 1: step from land
        if( lavafr         && lavato ) type = 2;                                                       // type 2: step from raft

        if( toRaft && toRaft.Control.BrokenRaft ||
            fromRaft && fromRaft.Control.BrokenRaft )                                                 // broken raft doesnt move
        {
            raft.Control.SetRaftDirection( true, raft.Control.RaftGroupID, EDirection.NONE );         // resets move for all rafts
            return false;
        }
        if( type == 1 || type == 2 )
        {
            move = true;
            bool res = G.Hero.CanMoveFromTo( true, from, to, G.Hero );
            if( res == false ) return true;
        }
        else
        if( lavafr && Map.IsWall( to ) )                                                               // type 3: move by forest bump
        {
            move = true;
            step = Util.GetInvDir( step );
            raft = fromRaft;
            type = 3;
        }

        if( move )                                                                                     // Raft over Lava moves step by step
        {
            for( int phase = 0; phase < tgl.Count; phase++ )
            {
                if( type == 1 || type == 2 )
                {
                    step = Util.GetVectorDir( tgl[ phase ] );
                    drl.Add( step );
                }
                if( type == 3 )
                {
                    step = Util.GetVectorDir( tgl[ phase ] );
                    step = Util.GetInvDir( step );
                    drl.Add( step );
                }

                raft.Control.SetRaftDirection( true, raft.Control.RaftGroupID, step );
                RaftStepCount = 1;                                                                    // number of steps           

                bool ok = raft.Control.UpdateRaftMovement( 3, true );                                 // Only test moving the rafts over lava                                                      
                if( phase == 0 ) ok0 = ok;
                if( phase == 1 ) ok1 = ok;
                if( phase == 2 ) ok2 = ok;
            }
            RaftStepCount = 1;                                                                        // number of steps 
            if( ok0 == false )
            if( ok1 && ok2 )
            {
                raft.Control.SetRaftDirection( true, raft.Control.RaftGroupID, EDirection.NONE );     // resets move for all rafts
                return true;
            }
            if( ok0 )
            {
                raft.Control.SetRaftDirection( true, raft.Control.RaftGroupID, drl[ 0 ] );
                raft.Control.UpdateRaftMovement( 3 );                                                // Moves the rafts over lava        
            }
            else
            if( ok1 )
            {
                raft.Control.SetRaftDirection( true, raft.Control.RaftGroupID, drl[ 1 ] );
                raft.Control.UpdateRaftMovement( 3 );                                                // Moves the rafts over lava        
            }
            else
            if( ok2 )
            {
                raft.Control.SetRaftDirection( true, raft.Control.RaftGroupID, drl[ 2 ] );
                raft.Control.UpdateRaftMovement( 3 );                                                // Moves the rafts over lava        
            }

            if( type == 3 )
                Map.I.CreateExplosionFX( to, "Fire Explosion", "Place Log" );                         // Bump Explosion FX 

            raft.Control.SetRaftDirection( true, raft.Control.RaftGroupID, EDirection.NONE );         // resets move for all rafts
            G.Hero.Control.AnimationOrigin = from;                                                    // to fix animation
            return true;
        }
        return false;
    }
    
    public static void ImpulsionateRaft( Vector2 from, Vector2 to, bool resetTimer = true )
    {
        if( Manager.I.GameType == EGameType.NAVIGATION ) return;
        if( FromBrokenRaft ) { FromBrokenRaft = false; return; }

        if( from == to ) return;
        Unit toRaft = GetRaft( to );
        Unit fromRaft = GetRaft( from );
        bool stopfrom = false;
        bool moveto = false;
        RaftStepCount = -1;
        Unit pitto = Map.I.GetUnit( ETileType.PIT, to );
        if( pitto ) return;

        EDirection step = Util.GetTargetUnitDir( from, to );

        if( toRaft == false && fromRaft == true ) 
            stopfrom = true;                  // Landing

        if( toRaft && fromRaft == false ) moveto = true;                            // Boarding
        
        if( toRaft == true && fromRaft == true )                                    // Change Raft Group ID
        if( toRaft.Control.RaftGroupID != fromRaft.Control.RaftGroupID )
        if( G.Hero.CanMoveFromTo( false, from, to, G.Hero ) == true )
        {
            stopfrom = true;
            moveto = true;
        }

        if( toRaft && toRaft.Control.IsDynamicObject() ) moveto = false;           // Dynamic to Raft: Do nothing
        if( fromRaft && fromRaft.Control.IsDynamicObject() ) stopfrom = false;     // Dynamic from Raft: Do nothing

        int rudder = ( int ) Item.GetNum( ItemType.Res_Rudder );                   // Rudder
        if( Map.I.GetUnit( ETileType.WATER, to ) != null )
        if( toRaft == false && fromRaft == true )
        {    
            if( Map.I.RaftDirectionChangeAvailable )
            if( Map.I.RaftTraverseDistance >= 1 )
            if( moveto == false )
            if( fromRaft.Control.IsDynamicObject() == false )
            if( rudder >= 1 )
            {
                List<Unit> rl = GetRaftList( fromRaft.Control.RaftGroupID );
                EDirection raft = rl[ 0 ].Control.RaftMoveDir;
                bool ok = false;
                if( step != raft )
                for( int i = -1; i <= 1; i++ )
                    {
                        EDirection rel = Util.GetRelativeDirection( i, raft );                                   // Rudder only works like the boulder 
                        if( step == rel )
                        {
                            ok = true;
                            break;
                        }
                    }

                if( ok )                                                                                         // Rudder direction change
                {
                    Item.AddItem( ItemType.Res_Rudder, -1 );
                    resetTimer = false;
                    fromRaft.Control.SetRaftDirection( resetTimer, fromRaft.Control.RaftGroupID, step );
                    Vector2 tg2 = G.Hero.Pos, last = Vector2.zero;
                    for( int i = 0; i < Sector.TSX; i++ )
                    {
                        if( Map.I.GetUnit( ETileType.WATER, tg2 ) == false )
                        {
                            tg2 = last; break;  
                        }                                                                                       // finds line effect ent target
                        last = tg2;
                        tg2 += Manager.I.U.DirCord[ ( int ) step ];
                    }
                    Map.I.LineEffect( G.Hero.Pos, tg2, 1.1f, .8f, Color.red, Color.red );                       // Line effect
                    Map.I.RaftDirectionChangeAvailable = false;
                    MasterAudio.PlaySound3DAtVector3( "Save Game", G.Hero.Pos );
                    return;
                }
            return;  // new: removed to prevent raft not stopping when hero leave it bug
            }
        }
        if( OnlyRudderPush ) { OnlyRudderPush = false; return; }

        if( stopfrom )                                                                                           // Stops From Raft
        {
            fromRaft.Control.SetRaftDirection( resetTimer, fromRaft.Control.RaftGroupID, EDirection.NONE );
            Map.I.RaftDirectionChangeAvailable = true;
            Map.I.RaftTraverseDistance = 0;
        }

        if( moveto )                                                                                             // Activates Destination Raft
        {
            toRaft.Control.SetRaftDirection( resetTimer, toRaft.Control.RaftGroupID, step );
            Map.I.RaftTraverseDistance = 0;
        }
    }

    public void SetRaftDirection( bool resetTimer, int id, EDirection dr )
    {
        List<Unit> rl = GetRaftList( id );
        for( int i = 0; i < rl.Count; i++ )
        {
            if( id == rl[ i ].Control.RaftGroupID )
            {
                rl[ i ].Control.RaftMoveDir = dr;
                if( resetTimer )
                    rl[ i ].Control.FlightPhaseTimer = 0;
            }
        }
    }
    public static Unit GetRaft( Vector2 tg )                                                                 // Returns raft in position
    {
        if( Map.PtOnMap( Map.I.Tilemap, tg ) == false ) return null;
        if( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ] != null )
        for( int i = 0; i < Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ].Count; i++ )
            {
                if( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ].TileID == ETileType.RAFT )
                    return Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ];
            }
        return null;
    }
    
    public static List<Unit> GetRaftList( int raftID )                                      // find all rafts in the cube with the same id
    {
        Sector s = Map.I.RM.HeroSector;
        List<Unit> rl = new List<Unit>();
        for( int y = ( int ) s.Area.yMin; y < s.Area.yMax; y++ )
        for( int x = ( int ) s.Area.xMin; x < s.Area.xMax; x++ )
        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( x, y ) ) )
        {
            Unit tgRaft = GetRaft( new Vector2( x, y ) );
            if( tgRaft )
            if( raftID == tgRaft.Control.RaftGroupID )
                rl.Add( tgRaft );
        }
        return rl;
    }
    #endregion


    #region Fog
    public static void ImpulsionateFog( Vector2 from, Vector2 to )
    {
        if( Manager.I.GameType == EGameType.NAVIGATION ) return;
        if( from == to ) return;
        Unit toFog = GetFog( to );
        Unit fromFog = GetFog( from );
        bool moveto = false;
        if( toFog && fromFog == false ) moveto = true;                            // Boarding

        if( toFog == true && fromFog == true )                                    // Change Fog Group ID
        if( toFog.Control.RaftGroupID != fromFog.Control.RaftGroupID )
            {
                moveto = true;
            }

        if( toFog && toFog.Control.IsDynamicObject() ) moveto = false;           // Dynamic to Fog: Do nothing
        if( moveto )                                                                // Activates Destination Fog
        {
            if( Map.I.ElectrifiedFogList.Contains( 
                toFog.Control.RaftGroupID ) == false ) return;
            int id = toFog.Control.RaftGroupID;
            List<Unit> rl = GetFogList( id );
            bool hasorb = false;
            for( int i = 0; i < rl.Count; i++ )
            if( id == rl[ i ].Control.RaftGroupID ) 
                {
                    Unit orb = Map.I.GetUnit( ETileType.ORB, rl[ i ].Pos );      // Electric fog is touching orb or moving?
                    if( orb ) hasorb = true;
                }

            if( hasorb == true )
            for( int i = 0; i < rl.Count; i++ )
            {
                if( id == rl[ i ].Control.RaftGroupID )
                {
                    rl[ i ].Control.RaftMoveDir = Util.GetTargetUnitDir( from, to );
                    rl[ i ].Control.FlightPhaseTimer = 0;
                }
            }
        }
    }

    public void UpdateFogMovement()
    {
        float tottime = GetRealtimeSpeedTime();

        if( IsDynamicObject() )                                                                               // Initializes Dynamic Raft Move List
            if( RaftMoveDir == EDirection.NONE ||
                FlightPhaseTimer > tottime )
            {
                NextDynamicStep( false );
            }

        if( RaftMoveDir == EDirection.NONE ) return;
        RaftMoving = true;
        bool stopGroup = false;
        if( FlightPhaseTimer > tottime )
        {
            EDirection mdir = RaftMoveDir;
            int rid = RaftGroupID;
            bool moveok = true;
            Sector s = Map.I.RM.HeroSector;
            List<Unit> fl = new List<Unit>();
            for( int y = ( int ) s.Area.yMin; y < s.Area.yMax; y++ )
            for( int x = ( int ) s.Area.xMin; x < s.Area.xMax; x++ )
            if( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )
                    {
                        Unit tgFog = GetFog( new Vector2( x, y ) );
                        if( tgFog )
                        {
                            if( RaftGroupID == tgFog.Control.RaftGroupID )
                            {
                                fl.Add( tgFog );                                                                          // Checks Fog Collision
                                Vector2 tg = tgFog.Pos + Manager.I.U.DirCord[ ( int ) mdir ];
                                Unit destfog = GetFog( tg );
                                if( destfog && destfog.Control.RaftGroupID != RaftGroupID )
                                {
                                    moveok = false;
                                }

                                if( Sector.GetPosSectorType( tg ) == Sector.ESectorType.GATES )                            // kill fog in the border
                                {
                                    FogKillList.Add( tgFog );
                                }
                            }
                        }
                    }

            if( moveok == false )                                                                                   // Move Blocked
            {
                if( IsDynamicObject() )                                                                             // Dynamic Raft
                {

                    if( MovementSorting == EDynamicObjectSortingType.SEQUENCE_CHANGE_ON_OBSTACLE )
                    {
                        NextDynamicStep( true );
                        //if( DynamicObjectMoveList != null && DynamicObjectMoveList.Count > 0 )                                  // Dynamic Raft
                        //{
                        //    if( ++DynamicStepCount >= DynamicObjectMoveList.Count )
                        //        DynamicStepCount = 0;
                        //    EDirection dr = Util.FlipDir( ( EDirection ) DynamicObjectMoveList[
                        //    DynamicStepCount ], Map.I.RM.HeroSector.FlipX, Map.I.RM.HeroSector.FlipY );
                        //    mdir = dr;
                        //}
                    }
                    else
                    {
                        FlightPhaseTimer = 0;
                        SetMovementDynamicCounters( -1 );                                                                   // Blocked raft remains in place until deobstructed
                    }
                }
                else                                                                                                // Normal Raft
                {
                    for( int i = fl.Count - 1; i >= 0; i-- )
                    {
                        fl[ i ].Control.RaftMoveDir = EDirection.NONE;
                        if( Map.I.ElectrifiedFogList.Contains(
                            fl[ i ].Control.RaftGroupID ) == true )
                            if( fl[ i ].Control.BoltEffect )                                                        // Bolt effect destroy after fog is white
                            {
                                Map.I.ElectrifiedFogList.Remove(
                                fl[ i ].Control.RaftGroupID );
                                fl[ i ].Control.BoltEffect.Kill();
                                fl[ i ].Control.BoltEffect = null;
                            }
                    }
                }
                RaftMoving = false;
                return;
            }
            bool movehero = true;
            Unit hfog = GetFog( G.Hero.Pos );
            if( hfog )
            {
                Vector2 fg = G.Hero.Pos - Manager.I.U.DirCord[ ( int ) mdir ];                                      // hero is in the last fog tile
                if( GetFog( fg ) != null )
                    movehero = false;
            }
            
            List<Unit> pm = new List<Unit>();
            for( int i = fl.Count - 1; i >= 0; i-- )
            {
                Unit r = fl[ i ];
                Vector2 tg = r.Pos + Manager.I.U.DirCord[ ( int ) mdir ];                                           // Moves Gameobject 

                if( G.Hero.Pos == tg )                                                                              // Hero killed by moving fog
                if( hfog == null )
                    Map.I.StartCubeDeath();

                r.Graphic.transform.position = new Vector3( r.Pos.x, r.Pos.y, r.Graphic.transform.position.z );
                r.transform.position = new Vector3( tg.x, tg.y, r.transform.position.z );
                r.Control.NotAnimatedPosition = r.Graphic.transform.position;

                if( r.Control.FlightPhaseTimer > tottime )                                                          // Timer Stuff
                    r.Control.FlightPhaseTimer = tottime;
                r.Control.FlightPhaseTimer -= tottime;
                r.Control.SmoothMovementFactor = 0;
                r.Control.TurnTime = 0;
                Controller.MoveMade = true;

                if( movehero )
                if( pm.Contains( G.Hero ) == false )                                                                // Moves Hero
                if( G.Hero.Pos == r.Pos )
                    {
                        HeroBeingPushedByFog = true;
                        if( Sector.GetPosSectorType( tg ) == Sector.ESectorType.GATES ||
                            G.Hero.CanMoveFromTo( false, G.Hero.Pos, tg, r ) == false )
                            Map.I.StartCubeDeath();
                        else
                        {
                            G.Hero.Control.AnimationOrigin = G.Hero.Pos;
                            G.Hero.Graphic.transform.position = new Vector3( 
                            G.Hero.Pos.x, G.Hero.Pos.y, G.Hero.Graphic.transform.position.z );
                            G.Hero.transform.position = new Vector3( tg.x, tg.y, G.Hero.transform.position.z );
                            G.Hero.Control.ApplyMove( G.Hero.Pos, tg, true );
                            G.Hero.Control.OldPos = tg;
                            G.Hero.Control.LastPos = tg;
                            G.Hero.Control.SmoothMovementFactor = 0;
                            G.Hero.Control.TurnTime = 0;
                            G.Hero.Control.UnitMoved = false;
                            UpdatePushedHero( false );
                            pm.Add( G.Hero );
                        }
                        HeroBeingPushedByFog = false;
                    }

                int posx = 0, posy = 0;                                                                            // Tile Calculation
                Map.I.Tilemap.GetTileAtPosition( tg, out posx, out posy );
                r.Control.TileChanged = false;
                if( r.Pos != new Vector2( posx, posy ) )
                {
                    r.Control.TileChanged = true;
                    r.Control.OldPos = r.Pos;
                    r.Control.LastPos = r.Pos;
                    r.Control.AnimationOrigin = r.Pos;
                    Unit orb = Map.I.GetUnit( ETileType.ORB, tg );                                             // orb touched - stop fog 
                    if( orb && orb.Body.OrbFogStopEnabled )
                    {
                        orb.Body.OrbFogStopEnabled = false;
                        stopGroup = true;
                    }
                }
                r.Pos = new Vector2( posx, posy );

                if( r.Control.TileChanged )                                                                        // Tile Changed
                {
                    if( Map.I.FUnit[ posx, posy ] == null )
                        Map.I.FUnit[ posx, posy ] = new List<Unit>();
                    Map.I.FUnit[ posx, posy ].Add( r );
                    Map.I.FUnit[ ( int ) r.Control.OldPos.x, ( int ) r.Control.OldPos.y ].Remove( r );
                    MasterAudio.PlaySound3DAtVector3( "Fog Move", r.Pos );                                         // Sound FX
                }
            }
            Map.I.UpdateFireIgnition( true );                                                                // Updates Fire ignition for land firepits
            if( stopGroup )
            {
                List<Unit> rl = GetFogList( rid );
                for( int j = 0; j < rl.Count; j++ )
                {
                    rl[ j ].Control.RaftMoveDir = EDirection.NONE;
                }
            }
        }
        RaftMoving = false;       
    }
    
    public static Unit GetFog( Vector2 tg )                                                                 // Returns fog in position
    {
        if( Map.PtOnMap( Map.I.Tilemap, tg ) == false ) return null;
        if( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ] != null )
            for( int i = 0; i < Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ].Count; i++ )
            {
                if( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ].TileID == ETileType.FOG )
                    return Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ];
            }
        return null;
    }

    public static List<Unit> GetFogList( int fogID )                                                        // find all fog in the cube with the same id
    {
        Sector s = Map.I.RM.HeroSector;
        List<Unit> rl = new List<Unit>();
        for( int y = ( int ) s.Area.yMin; y < s.Area.yMax; y++ )
            for( int x = ( int ) s.Area.xMin; x < s.Area.xMax; x++ )
                if( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )
                {
                    Unit tgFog = GetFog( new Vector2( x, y ) );
                    if( tgFog )
                        if( fogID == tgFog.Control.RaftGroupID )
                            rl.Add( tgFog );
                }
        return rl;
    }  
    
    #endregion    

    #region Fish
    public void UpdateFishMovement()
    {
        if( Map.I.TurnFrameCount == 1 )
        {
            if( Unit.Body.StackAmount > 0 )
            {
                Water.SpawnFishByMod( null, ( int ) Unit.Body.StackAmount - 1,
                Unit.ModID, Unit.Pos );                                                                    // resource amount spawns the number of extra fish in the tile
                Unit.Body.StackAmount = 0;
            }
        }

        if( Time.timeScale > Map.I.RM.RMD.HurryUpTimeScaleFactor ) return;
        if( G.Hero.Control.PathFinding.Path != null &&
            G.Hero.Control.PathFinding.Path.Count > 0 ) return;

        Map.I.UpdateFishBehaviour( Unit );

        Vector3[ ] plist = new[ ] { Unit.Spr.transform.position, G.Hero.Spr.transform.position };
        if( Unit.Activated == false ) return;
        if( SwimmingDepht == 0 ) 
            Unit.Spr.color = new Color( 1, 1, 1, 1 );
        else
        {
            float alpha = 1 - SwimmingDepht;
            Unit.Spr.color = new Color( 1, 1, 1, Mathf.Lerp( Unit.Spr.color.a, alpha, Time.deltaTime * .01f ) );
            Unit.Spr.color = new Color( 1, 1, 1, alpha );
        }

        if( FishSwimType != EFishSwimType.NORMAL ) 
        if( FishSwimType != EFishSwimType.FLEE_TO_FARTHEST )
        if( FishSwimType != EFishSwimType.FLEE_TO_RANDOM )
            return;
       // Unit.Spr.color = new Color( 1, 1, 1, Helper.I.FloatVal );
        CheckFishTargetReached();
        SortFishTarget();
        //UpdateAttackRate();

        Unit.RightText.gameObject.SetActive( false );
        Unit.RightText.color = Color.white;
        //if( Unit.Body.FishCaught == false )

        Unit.RightText.gameObject.SetActive( true );
        string add = "";
        if( Unit.Control.OverFishSecondsNeededPerUnit > 0 )
        if( Unit.Water.MarkedFish == 1 ) add = "-";                                     // Marked fish sign
        else
            if( Unit.Water.MarkedFish == 2 ) add = "+";
        if( Unit.Md && Unit.Md.FishCatchType == EFishCatchType.ABSOLUTE )
        {
            Unit.RightText.color = new Color32( 255, 0, 255, 255 );
            if( Unit.Body.Hp >= 100 )
            {
                Unit.RightText.color = Color.green;
                Unit.RightText.text = "Catch!";
            }
            else
                Unit.RightText.text = "" + ( 100 - Unit.Body.Hp ).ToString( "0." ) + add;
        }
        else
            if( Unit.Body.Hp > 0 )
                Unit.RightText.text = "" + ( int ) Unit.Body.Hp + "%" + add;
        if( Unit.Md && Unit.Md.FishCatchType == EFishCatchType.RANDOM )
        {
            Unit.RightText.color = Color.yellow;
            if( Unit.Body.Hp > FishRedLimit ) Unit.RightText.color = Color.red;                             // Red Limit     
            if( Unit.Body.Hp < FishGreenLimit ) Unit.RightText.color = Color.green;                           // Green Limit   
        }

        FlightTime += Time.deltaTime;
        FlightTargetTime += Time.deltaTime;

        float rotationSpeed = 700;
        if( FlightTargetTime > 5 )
            rotationSpeed += FlightTargetTime * 2;

        Quaternion qn = Util.GetRotationToPoint( Unit.Spr.transform.position, FlyingTarget );
        Unit.Spr.transform.rotation = Quaternion.RotateTowards( 
        Unit.Spr.transform.rotation, qn, Time.deltaTime * rotationSpeed );

        //if( Unit.Body.IsFish == false )
        //    Unit.Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );

        if( Unit.Body.FishCaught )                                                                        // Fish being caught animation
        {
            float fact = 1 - ( Map.I.FishingTimerCount / Map.I.FishFinishingTotalTime );

            if( FishCaughtTimeCount != -1 )
            {
                FishCaughtTimeCount += Time.deltaTime;
                fact = ( FishCaughtTimeCount / Map.I.FishFinishingTotalTime );
            }

            fact = Mathf.Clamp( fact, 0, 1 );
            Vector3 lp = Vector3.Lerp( Unit.Control.FishCatchPosition, G.Hero.Pos, fact );
            lp = new Vector3( lp.x, lp.y, Unit.transform.position.z );
            Unit.transform.position = lp;
        }
        else
        if( FlyingTarget.x != -1 )
        {
            float spd = FlyingSpeed;
            if( FishSwimType == EFishSwimType.FLEE_TO_FARTHEST ||                                                    // fleeing speed defined by Effectval
                FishSwimType == EFishSwimType.FLEE_TO_RANDOM ) 
                spd = FishAction[ CurFA ].EffectVal;
            if( OverFishTimeCount > 0 )
            if( FlightTargetTime < .5f ) spd = .3f;
            Unit.transform.position += Unit.Spr.transform.up * Time.deltaTime * spd;                                 // Updates fish position
        }

        if( Unit.Body.FishCaught && Util.Chance( 5 ) )                                                               // Fish Caught animation
        {
            float amt = .1f;
            Unit.transform.position += new Vector3( Random.Range( -amt, amt ), Random.Range( -amt, amt ), 0 );
        }

        UpdateFlyingTile();

        if( TileChanged )
        if( Unit.Body.FishCaught  ||                                                                                 // Caught fish and fast manta water splash fx
            FishSwimType == EFishSwimType.FLEE_TO_FARTHEST )
            Util.PlayParticleFX( "Water Splash", transform.position ); 
    }

    public void SortFishTarget()
    {
        if( FlyingTarget.x != -1 ) return;

        Vector2 np = Unit.Pos;
        int maxrange = 5;

        bool line = true;
        if( Util.Chance( 80 ) )
        {
            line = false;
            maxrange = 2;                                                                                 // diving mode
        }
        List<Vector2> tlist = new List<Vector2>();
        bool removeoriginal = false;
        if( Map.I.FishingMode == EFishingPhase.NO_FISHING )
        if( Map.I.IsInTheSameLine( G.Hero.Pos, Unit.Pos ) )
        {
            maxrange = Sector.TSX;
            line = true;
        }

        if( FishSwimType == EFishSwimType.FLEE_TO_FARTHEST || 
            FishSwimType == EFishSwimType.FLEE_TO_RANDOM )
        {
            int dist = ( int ) FishAction[ CurFA ].EffectVal2;
            if( dist <= 0 ) dist -= Sector.TSX;
            maxrange = dist;                                                                            // Flee max distance defined by effval2
        }

        for( int dr = 0; dr < 8; dr++ )
        {
            List<Vector2> tl = Util.GetTileLine( np, ( EDirection ) dr, ETileType.WATER, maxrange );
            int maxID = maxrange;
            for( int j = 0; j < tl.Count - 1; j++ )
                if( Map.I.CanSwimFromTo( tl[ j ], tl[ j + 1 ] ) == false )                               // remove no continuous water tiles
                {
                    int cut = ( tl.Count - ( j + 1 ) );
                    if( cut < 0 ) cut = 0;
                    tl.RemoveRange( j + 1, cut );
                }

            if( tl.Count > 1 ) 
                removeoriginal = true;                                                                   // Remove original unit pos from list since theres more possibilities;

            if( tl != null )
            {
                if( tl.Count > 0 ) tlist.AddRange( tl );                                                 // Add direction to the master list
            }
        }

        if( removeoriginal )
            tlist.RemoveAll( item => item == Unit.Pos );                                                 // remove original position

        if( tlist.Count <= 0 ) tlist.Add( Unit.Pos );
        int id = Random.Range( 0, tlist.Count );

        Vector2 tg = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] *
        Map.I.RM.RMD.FishCornTargetRadius;
        if( Map.I.FishingMode == EFishingPhase.NO_FISHING )
        if( Item.GetNum( ItemType.Res_BirdCorn ) > 0 )                                                   // Fish Lure
        if( Map.I.GetUnit( ETileType.WATER, tg ) )
        {
            List<int> good = new List<int>();
            for( int i = 0; i < tlist.Count; i++ )
            for( int y = ( int ) tg.y - 1; y <= tg.y + 1; y++ )
            for( int x = ( int ) tg.x - 1; x <= tg.x + 1; x++ )
            if ( Map.I.IsInTheSameLine( new Vector2( x, y ), Unit.Pos ) )
            if ( new Vector2( x, y ) == tlist[ i ] )
                 good.Add( i );

            if( good != null && good.Count > 0 )
            {
                int sort = Random.Range( 0, good.Count );
                id = good[ sort ];
            }
        }

        if( FishSwimType == EFishSwimType.FLEE_TO_FARTHEST )                                                         // Chooses farthest tile for fleeing fish
        {
            List<float> distlist = new List<float>();
            for( int i = 0; i < tlist.Count; i++ )
            {
                float dist = Vector2.Distance( Map.I.MainHook.TilePos, tlist[ i ] );
                distlist.Add( dist );
            }
            id = Util.GetBiggestValID( distlist );
        }

        if( FishSwimType == EFishSwimType.FLEE_TO_RANDOM )                                                         // Chooses random tile for fleeing fish
        {
            List<float> factl = new List<float>();
            for( int i = 0; i < tlist.Count; i++ )
            {
                float dist = Util.Manhattan( Map.I.MainHook.TilePos, tlist[ i ] );
                factl.Add( dist * 10 );
            }
            //id = Util.GetBiggestValID( factl );
            id = Util.Sort( factl );
        }

        Unit water = Map.I.GetUnit( ETileType.WATER, tlist[ id ] );                                                // this is to avoid fish stuck on land bug after beig spawned
        if( water == null )
        {
            for( int dr = 0; dr < 8; dr++ )
            {
                Vector2 pt = tlist[ id ] + ( Manager.I.U.DirCord[ dr ] );
                if( Map.PtOnMap( Map.I.Tilemap, pt ) )
                {
                    int pid = Map.I.ContinuousPondID[ ( int ) pt.x, ( int ) pt.y ];                // fish over non water terrain crashes here - make an error check
                    water = Map.I.GetUnit( ETileType.WATER, pt );
                    if( water != null )
                        if( pid == Map.I.CurrentContinuousPondId )
                        {
                            FlyingTarget = pt;
                            return;
                        }
                }
            }
            return;
        }

        FlightTargetTime = 0;                                                                                      // target chosen
        float rad = .2f;
        Vector2 rand = new Vector2( Random.Range( -rad, +rad ), Random.Range( -rad, +rad ) );
        FlyingTarget = tlist[ id ] + rand;
        SwimmingDepht = 0;
        //if( line == false )
        SwimmingDepht = Random.Range( .3f, .6f );        
    }

    public bool CheckFishTargetReached()
    {
        float distance = Vector2.Distance( FlyingTarget, Unit.transform.position );                                // Fish Has reached the target
        if( distance < .2f )
        {
            FlyingTarget = new Vector2( -1, -1 );
            FlyingSpeed = Random.Range( MinFishSpeed, MaxFishSpeed );
            return true;
        }
        return false;
    }
    #endregion

    #region FireBall
    public void UpdateFireBallMovement()
    {
        if( Unit.Activated == false ) return;
        float distance = Vector2.Distance( FlyingTarget, Unit.transform.position );                                // FB Has reached the target
        Unit.Spr.color = Color.white;
        if( FlyingTarget.x != -1 )
        if( distance < .1f )
            {
                FlyingTarget = new Vector2( -1, -1 );
                Unit.Spr.transform.localPosition = new Vector3( 0, 0, Unit.Spr.transform.localPosition.z );  

                Unit water = Map.I.GetUnit( ETileType.WATER, Unit.Pos );
                if( water )                                                                                       // fireball stop over water
                {
                    Unit.Body.KillOnFlightEnd = true;           
                    MasterAudio.PlaySound3DAtVector3( "Water on Fire", transform.position );
                }

                if( Unit.Body.KillOnFlightEnd )
                {
                    Item.AddItem( ItemType.Res_Fire_Staff, -1 );                                                  // Fire staff decrement
                    Controller.CreateMagicEffect( Unit.Pos );                                                     // Magic FX  
                    Map.I.CreateExplosionFX( Unit.Pos );                                                          // Explosion FX  /
                    if( water == null ) 
                        MasterAudio.PlaySound3DAtVector3( "Explosion 1", Unit.Pos );                              // Explosion Sound FX
                    Unit.Kill();
                }
                return;
            }
        FlightTime += Time.deltaTime;
        FlightTargetTime += Time.deltaTime;
        float moveSpeed = 30;
        Vector3 tg = new Vector3( FlyingTarget.x, FlyingTarget.y, Unit.transform.position.z );
        if( FlyingTarget.x != -1 )
            Unit.transform.position = Vector3.MoveTowards( Unit.transform.position, tg, Time.deltaTime * moveSpeed );
        UpdateFlyingTile();
    }
    #endregion    

    public void UpdateHerbMovement()
    {
        if( Unit.Activated == false ) return;

        Unit.Spr.color = Color.white;
        float distance = Vector2.Distance( FlyingTarget, Unit.Spr.transform.position );                                 // Herb Has reached the target

        if( FlyingTarget.x != -1 )
        if( distance < .1f )
        {
            FlyingTarget = new Vector2( -1, -1 );
            Unit.Spr.transform.localPosition = new Vector3( 0, 0, Unit.Spr.transform.localPosition.z );
            if( Map.I.HerbMatchList != null )
            for( int i = 0; i < Map.I.HerbMatchList.Count; i++ )
            {
                iTween.ShakePosition( Map.I.HerbMatchList[ i ].Spr.gameObject, new Vector3( .2f, .2f, 0 ), .5f );
                Map.I.HerbMatchList[ i ].Body.HerbKillTimer = 0;
                if( Map.I.HerbMatchList.Count > 1 )
                    MasterAudio.PlaySound3DAtVector3( "Item Collect", G.Hero.Pos );                                    // Sound Fx
                else
                    MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );
            }
            return;
        }
        FlightTime += Time.deltaTime;
        FlightTargetTime += Time.deltaTime;

        float moveSpeed = 12f;
        Vector3 tg = new Vector3( FlyingTarget.x, FlyingTarget.y, Unit.Spr.transform.position.z );
        if( FlyingTarget.x != -1 )
            Unit.Spr.transform.position = Vector3.MoveTowards( Unit.Spr.transform.position, tg, Time.deltaTime * moveSpeed );
        Unit.Body.Sprite2.transform.position = Unit.Spr.transform.position;
        //UpdateFlyingTile();
    }

    public static Unit GetBlocker( Vector2 tg )                                                                 
    {
        if( Map.PtOnMap( Map.I.Tilemap, tg ) == false ) return null;
        if( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ] != null )
        for( int i = 0; i < Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ].Count; i++ )
            {
                if( Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ].TileID == ETileType.BLOCKER )
                    return Map.I.FUnit[ ( int ) tg.x, ( int ) tg.y ][ i ];
            }
        return null;
    }

    public static void MoveFlyingUnitTo( ref Unit un, Vector2 from, Vector2 to )
    {
        Unit shackle = Map.GFU( ETileType.MINE, to );
        if( shackle && shackle.Body.MineType == EMineType.SHACKLE )                                         // connect shackle
            Controller.ConnectShackle( shackle, un );      
        
        if( un.TileID == ETileType.MINE || un.ProjType( EProjectileType.BOOMERANG ) ) 
            Controller.UpdateLeverMine( un, true, from, to );
        un.Pos = to;                                                                                  
        un.transform.position = to;        
        un.Control.FlyingTarget = to;

        if( from.x != -1 )
            un.Graphic.transform.position = new Vector3( from.x, from.y, un.Graphic.transform.position.z );
        else
            un.Graphic.transform.position = un.transform.position;

        un.Control.OldPos = from;
        un.Control.LastPos = from;
        un.Control.AnimationOrigin = from;
        un.Control.SmoothMovementFactor = 0;
        un.Control.TurnTime = 0;
        un.Control.UnitMoved = false;
        if( un.TileID == ETileType.MINE )
            un.Mine.AnimateIconTimer = 1;
        if( Map.I.FUnit[ ( int ) to.x, ( int ) to.y ] == null )
            Map.I.FUnit[ ( int ) to.x, ( int ) to.y ] = new List<Unit>();
        Map.I.FUnit[ ( int ) to.x, ( int ) to.y ].Add( un );
        if( from.x != -1 )
            Map.I.FUnit[ ( int ) from.x, ( int ) from.y ].Remove( un );
        un.Control.UpdatePostMove();

        if( un.TileID == ETileType.SPIKES )
        if( un.Control.IsDynamicObject() )
            un.MeleeAttack.FlipSpike();
        Spell.UpdateHookDestruction( to );                                                            // hook destruction
        if( un.TileID == ETileType.MINE )
            Map.I.CreateExplosionFX( to, "Smoke Cloud", "" );                                         // Smoke Cloud FX      
    }
}
