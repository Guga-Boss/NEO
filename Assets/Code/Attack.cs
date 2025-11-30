using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using DarkTonic.MasterAudio;
using DeenGames.Utils;
using DeenGames.Utils.AStarPathFinder;
using System;

#region Enums
public enum EDamageType
{
	NONE = -1, MELEE, RANGED, MAGIC, BEEHIVE, FIRE, BLEEDING
}
public enum ETargettingType
{
    NONE = 0, FRONTFANCING, ROACHBASIC, MISSILE, BEEHIVE, DRAGON,
    JUMPER, DRAGON3, WASP, MOTHERWASP,
    SLIME, FROG, STEPPER, HUGGER, SPELL,
    BOOMERANG, FIREBALL, TOWER, INFECTION
}
#endregion


[Serializable]
public class Attack : MonoBehaviour
{
    #region Variables
    public Body Body;
	public Unit Unit;
	public EDamageType DamageType;
	public ETargettingType TargettingType;
	public EUnitType EnemyTargetType;
    public int ID = -1;
    public int SpeedListID = 0;
	public float 
    TotalDamage, BaseDamage, BonusDamage, DamagePerLevel, DamagePerStar,   
	BackAttBonus, BackSideAttBonus, SideAttBonus, FrontSideAttBonus, FrontalAttBonus, 
	BaseRange, TotalRange, SurplusBonus, RicochetLevel, SpeedTimeCounter, AttackSpeedRate,
    AttackSpeedTarget, InvalidateAttackTimer;
	public float AttackResult;
    public List< Vector2> AttackTg;
    public List<float> AttackTotalTimeList;
    public const float BREAK  = -99999999;
    public static bool BerserkAttack = false;
    public static bool SecondaryAttack = false;
    public static bool SuccessfulBeehiveAttack = false;
    public float DamageSurplus, OriginalDamageSurplus, RTDamageSurplus, OriginalRTDamageSurplus;
    public static bool AttackMade = false;
    public static bool ResetAttackCounter = true;
    public static bool InvalidateAttack = false;
    public Unit ForcedEnemy = null;
    public static bool IsMoveShot = false;
    public static bool ShotOverPit = false;
    public static int ShotPassThroughCount = 0, DuplicatorCount = 0, CurrentHit = 0;
    public int AttacksPerMove = 1;
    public bool ForceAttack = false;
    public static bool AttackedByHeroMelee = false;
    public static Vector2 AttackOrigin;
    public static EDirection AttackOriginalDirection;
    public static bool BoomerangAttack;
    public float TotalAttackTime = -1;
    public static bool SpiderSquash = false;
    public static List<int> AxeAttackList;
    public static List<int> DuplicatorAttackList;
    public static float QQHPDamage;
    #endregion

    public void UpdateIt( bool apply ) 
    {
        if( Map.I.IsPaused() ) return;
        ForcedEnemy = null;                                                                       // Init Variables
        AttackMade = false;
        BoomerangAttack = false;
        ResetAttackCounter = true;
        InvalidateAttack = false;
        ShotOverPit = false;
        ShotPassThroughCount = 0;
        QQHPDamage = -1;
        DuplicatorCount = 0;
        AttackOrigin = Unit.Pos;
        AttackOriginalDirection = Unit.Dir;

        if( CheckTimer() == false ) return;                                                       // Check timer

		if( apply == false ) return;
        if( Unit.Body.MakePeaceful > 0 ) return;

        if( DamageType == EDamageType.RANGED )                                                    // No ranged attack if hero jumped to avoid back and forth bug
        if( Controller.HeroJumped ) return;

        AttackResult = 0;                                                               
        UI.I.AttackDescription = UI.I.AttackType = UI.I.AmbushInfo =                              // Empty strings
        UI.I.CorneringInfo = UI.I.SurplusInfo = UI.I.SprinterInfo = "";
        UI.I.ShieldReduction = UI.I.Shield = UI.I.AmbushBonus = 
        UI.I.ConeringBonus = UI.I.SprinterBonus = 0;
        
        AttackTg = GetAttackTargets();                                                            // Get Attack Targets
        float tothp = 0;
        if ( AttackTg != null )
        for( int i = 0; i < AttackTg.Count; i++ )                                                 // Loop through Targets
        {
            int totHits = 1 + DuplicatorCount;
            for( int hits = 0; hits < totHits; hits++ )                                          // multiple attacks
            {
                CurrentHit = hits;
                float hp = AttackUnit( true, AttackOrigin, AttackTg[ i ], i );                   // Attack Positions

                if( InvalidateAttack ) goto endloop;                                             // break if requested

                tothp += hp;
                if( DuplicatorCount == 0 )
                if( ShotPassThroughCount <= 0 )                                                  // pass through attack
                    {
                        if( hp == BREAK ) goto endloop;
                        if( TargettingType == ETargettingType.MISSILE && tothp > 0 ) 
                            goto endloop;
                        if( TargettingType == ETargettingType.SPELL && tothp > 0 )
                        if( G.Hero.Body.Sp[ ID ].Type != ESpiderBabyType.THROWING_AXE )          // axe hit all targets
                            goto endloop;
                    }
            }
            if( ForcedEnemy ) goto endloop;                                                      // forced enemy,
        }
        endloop:

        AttackResult = tothp;

        if( AttackMade )                                                                          // Attack Made, reset Speed Counter
        {
            if( ResetAttackCounter )
                SpeedTimeCounter = 0;

            UpdateCustomSpeedCounterID();                                                         // Update custom speed ID

            Spell.UpdateAfterAttackItemCharge( this );                                            // charges spells costs
        }

        float tottime = GetRealtimeSpeedTime();                                                   // clamps counter to maximum value
        if( SpeedTimeCounter > tottime )
            SpeedTimeCounter = tottime;

        ForceAttack = false;
    }
    public float GetRealtimeSpeedTime( int lv = -1, bool boost = true )
    {
        if( AttackTotalTimeList != null && AttackTotalTimeList.Count > 0 )                                            // Custom speed list movement type
        {
            return AttackTotalTimeList[ SpeedListID ];
        }
  
        if( Unit.TileID == ETileType.SPIKES ) return TotalAttackTime;                                                 // These ones are measured in seconds
        float spd = GetRealtimeSpeed( lv, boost );
        return 1 / ( spd / Map.I.Tick );
    }
    public float GetRealtimeSpeed( int lv = -1, bool boost = true )
    {
        float speed = 0;
        if( Unit.UnitType == EUnitType.HERO )                                                                          // Hero Att Speed
        {
            if( DamageType == EDamageType.MELEE )
            {
                if( lv == -1 )
                    speed = Map.I.RM.RMD.RTHeroMeleeAttackSpeed + Unit.Body.RealtimeMeleeAttSpeed;
                else
                    speed = Map.I.RM.RMD.RTHeroMeleeAttackSpeed + lv;
            } 
            else
            if( DamageType == EDamageType.RANGED )
            {
                if( lv == -1 )
                    speed = Map.I.RM.RMD.RTHeroRangedAttackSpeed + Unit.Body.RealtimeRangedAttSpeed;
                else
                    speed = Map.I.RM.RMD.RTHeroRangedAttackSpeed + lv;
                speed += Item.GetNum( ItemType.Res_Quiver );
            }

            if( boost )
            if( Map.I.CurrentArea != -1 && Map.I.CurArea.Vigor > 0 )
                speed += Util.Percent( Map.I.RM.RMD.VigorRTAttackSpeedBonus, speed );
        }
        else                                                                                                   // Monster Att Speed
        {
            speed = Unit.Control.GetMonsterRTMovSpeed() * AttacksPerMove;
            if( TotalAttackTime != -1 ) speed = TotalAttackTime;

            if( TargettingType == ETargettingType.INFECTION )                                                  // Attention, using melee total time. You can change it later if you wish different timers
                speed = Unit.MeleeAttack.TotalAttackTime;
  
            if( Unit.Control.IsFlyingUnit )            
            {
                speed = Util.Percent( AttackSpeedRate, Map.I.RM.RMD.RTDragonRangedAttackSpeed );
                if( Unit.TileID == ETileType.QUEROQUERO ) speed = 0;                                            // Immediate attack for Dragon 3
                if( Unit.TileID == ETileType.DRAGON1 ) speed = 10;
            }
        }

        return speed;
    }

    public float GetRealtimeDamageFactor()
    {
        float fact = 0;
        if( Unit.UnitType == EUnitType.HERO )                                           // Hero RT att
        {
            if( DamageType == EDamageType.MELEE )
                fact = Map.I.RM.RMD.RTHeroMeleeAttackDamageFactorr;
            if( DamageType == EDamageType.RANGED )
                fact = Map.I.RM.RMD.RTHeroRangedAttackDamageFactorr;
        }
        else                                                                            // Monster RT att
        {
            if( DamageType == EDamageType.MELEE )
                fact = Map.I.RM.RMD.RTMonsterMeleeAttackDamageFactor;
            if( DamageType == EDamageType.RANGED )
                fact = Map.I.RM.RMD.RTMonsterRangedAttackDamageFactor;
        }
        return fact;
    }

    public bool CheckTimer()
    {
        if( Map.I.HeroDeathTimer > 0 ) return false;
        if( InvalidateAttackTimer > 0 )                                                                   // Invalidate attack timer
        {
            if( Map.Stepping() == false )
            {
                InvalidateAttackTimer -= Time.deltaTime;
            }
            else
            {
                InvalidateAttackTimer -= 1;                                                               // Stepping mode
            }
            return false;
        }

        if( Unit.ProjType( EProjectileType.FIREBALL ) ) return true;                                

        if( TargettingType == ETargettingType.SPELL )                                              
        {
            if( G.Hero.Body.Sp[ ID ].Type == ESpiderBabyType.THROWING_AXE )                          // Throwing Axe Attacks automatically
                return true;

            if( Map.I.AdvanceTurn )
            if( Spell.IsHook( G.Hero.Body.Sp[ ID ].Type ) )                                          // Hook Attack            
                return true; 

            if( Map.I.AdvanceTurn )
            if( G.Hero.Control.LastMoveType != EMoveType.STILL &&
                G.Hero.Control.LastMoveType != EMoveType.ROTATE )
                return true;
            return false;
        }
        if( TargettingType == ETargettingType.BOOMERANG )                                           // Boomerang
        if( Map.I.AdvanceTurn ||
            Controller.PassiveBoomerangAttackTg != null ||
            Controller.ForceBoomerangAttack )
            return true; else return false;

        float speed = GetRealtimeSpeed();
        float tottime = GetRealtimeSpeedTime();
        SpeedTimeCounter += Time.deltaTime;                                                        // increment counter

        if( Unit.Control.WaitTimeCounter > 0 ) return false;                                       // unit is entangled

        UpdateSpike( false, tottime );                                                             // Update Spike

        if( Map.I.IsPaused() ) return false;
        if( Item.GetNum( ItemType.Res_Mask ) > 0 ) return false;

        if( TargettingType != ETargettingType.INFECTION )
        if( Unit.Control.UnitMoved )
        if( Unit.TileID == ETileType.SPIDER ||
            Unit.TileID == ETileType.SCARAB )
            SpeedTimeCounter = 0;                                                               // free walking around after move
        
        if( Unit.Control.UnitMoved )
        if( Unit.TileID == ETileType.SLIME ) return true;

        if( Map.Stepping() )                                                                    // Stepping Based Movement
        {
            SpeedTimeCounter = 0;
            if( Map.I.AdvanceTurn && Controller.BlockStepping == false )
            {
                if( Unit.Control.UnitMoved == false || Unit.UnitType == EUnitType.HERO )
                    return true;
                if( Unit.TileID == ETileType.FROG )
                    return true;
            }
            return false;
        }

        if( Unit.UnitType == EUnitType.MONSTER )
        if( Unit.Control.TickBasedMovement )    
        {
            SpeedTimeCounter = 0;
            if( Unit.Control.UnitMoved == false )
            if( Unit.Control.CheckTickMove() ) return true;
            return false;
        }

        if( SpeedTimeCounter >= tottime || speed == 0 )
        {
            Map.I.MonstersMoved = true;
            //SpeedTimeCounter -= tottime;
            return true;
        }
        else return false;
    }    

	public void Copy( Attack un )
	{
        DamageType            = un.DamageType;
        TargettingType        = un.TargettingType;
        EnemyTargetType       = un.EnemyTargetType;
		BaseRange             = un.BaseRange;
		TotalRange            = un.TotalRange;
		DamagePerLevel        = un.DamagePerLevel;
		DamagePerStar         = un.DamagePerStar;
        TotalDamage           = un.TotalDamage;
        BaseDamage            = un.BaseDamage;
        BonusDamage           = un.BonusDamage;
        RicochetLevel         = un.RicochetLevel;
        SpeedTimeCounter      = un. SpeedTimeCounter;
		//DamageSurplus       = un.DamageSurplus;
        //OriginalDamageSurplus = un.OriginalDamageSurplus;
        AttackSpeedTarget     = un.AttackSpeedTarget;
        AttackSpeedRate       = un.AttackSpeedRate;
        AttacksPerMove        = un.AttacksPerMove;
        ForceAttack           = un.ForceAttack;
        TotalAttackTime       = un.TotalAttackTime;
        InvalidateAttackTimer = un.InvalidateAttackTimer;
        SpeedListID           = un.SpeedListID;
        AttackTotalTimeList = null;
	}

	public void Save ()
	{
        GS.W.Write( SpeedTimeCounter );
        GS.W.Write( BaseRange);
        GS.W.Write(TotalRange);
        GS.W.Write(TotalDamage);
        GS.W.Write(BaseDamage);
        GS.W.Write(BonusDamage);
        GS.W.Write(SpeedListID);
        GS.W.Write(TotalAttackTime);
        GS.W.Write(AttackSpeedRate);
        #region vars
        //DamageType = un.DamageType;
        //TargettingType = un.TargettingType;
        //EnemyTargetType = un.EnemyTargetType;
        //DamagePerLevel = un.DamagePerLevel;
        //DamagePerStar = un.DamagePerStar;
        //RicochetLevel = un.RicochetLevel;
        ////DamageSurplus       = un.DamageSurplus;
        ////OriginalDamageSurplus = un.OriginalDamageSurplus;
        //AttackSpeedTarget = un.AttackSpeedTarget;
        //AttacksPerMove = un.AttacksPerMove;
        //ForceAttack = un.ForceAttack;
        //InvalidateAttackTimer = un.InvalidateAttackTimer;
        #endregion
    }
    public void Load()
    {
        SpeedTimeCounter = GS.R.ReadSingle();
        BaseRange = GS.R.ReadSingle();
        TotalRange = GS.R.ReadSingle();
        TotalDamage = GS.R.ReadSingle();
        BaseDamage = GS.R.ReadSingle();
        BonusDamage = GS.R.ReadSingle();
        SpeedListID = GS.R.ReadInt32();
        TotalAttackTime = GS.R.ReadSingle();
        AttackSpeedRate = GS.R.ReadSingle();
    }

	public float AttackUnit ( bool apply, Vector2 from, Vector2 tg, int attId )
	{
		if ( tg.x  == -1 || tg.y == -1 ) return 0;	                                          // Target out of Map
		if ( Map.PtOnMap( Map.I.Tilemap, tg ) == false ) return 0;
		//if ( Map.I.PtOnAreaMap( tg ) == false ) return 0;

		Unit Enemy = Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ];
        if( ForcedEnemy != null ) 
            Enemy = ForcedEnemy;

        if( Unit.UnitType == EUnitType.MONSTER )                                              // Monsters attacking target hero
        {
            if( Unit.TileID != ETileType.INACTIVE_HERO )
            if( Unit.ProjType( EProjectileType.BOOMERANG ) == false )
            if( Unit.ProjType( EProjectileType.FIREBALL  ) == false )
                Enemy = Map.I.Hero;  

            Unit un = Map.I.GetUnit( from, ELayerType.GAIA2 );                                // small Monsters over fire do not attack
            if( un && un.TileID == ETileType.FIRE && un.Body.FireIsOn )
            if( Spell.IsCompoundMonster( Unit ) == false )
                return 0;
            
            Unit tgvn = Map.GFU( ETileType.VINES, from );                                     // Burning Vine in position?
            if( tgvn && tgvn.Body.FireIsOn )
                return 0;

            if( SpiderSquash == true )                                                        // Hero immune after spider squashing
            {
                if( Map.Stepping() == false )                                                 // In realtime, zero attacker move and att timers
                {
                    Unit.Control.SpeedTimeCounter = 0;
                    SpeedTimeCounter = 0;
                    return 0;
                }
            }

            if( Unit.TileID == ETileType.SPIDER )
            if( Map.Stepping() == true )
                {
                    if( Unit.Control.SpiderAttackBlockPhase == 0 )                           // Spider neighbor tile traverse block (only one turn)
                    if( Util.Neighbor( G.Hero.Pos, Unit.Pos ) )
                    {
                        Vector3 tgg = Util.GetTargetUnitVector( Unit.Pos, G.Hero.Pos );
                        iTween.PunchPosition( Unit.Graphic.gameObject, tgg * 0.6f, 0.5f );
                        MasterAudio.PlaySound3DAtVector3( "Bump 2", G.Hero.Pos );
                        Unit.Control.SpiderAttackBlockPhase = 1;
                        return 0;
                    }
                }
        }

        if( Unit.Body.IsDead ) return 0;

        if( Unit.UnitType == EUnitType.MONSTER )
        if( Body.RopeConnectFather != null )
        for( int i = 0; i < Body.RopeConnectFather.Count; i++ )                               // Shackled monster
            {
                int dist = Util.Manhattan( Body.RopeConnectFather[ i ].Pos, tg ) - 1;
                if( Body.OriginalHookType == EMineType.SHACKLE )
                if( dist > Body.RopeConnectFather[ i ].Body.ShackleDistance ) return 0;
            }
    
		if( Enemy      == null ) return 0;                                                    // No enemy in position

		if( Enemy.Body == null ) return 0;                                                    // Enemy has no body

        if( Unit.UnitType == EUnitType.HERO )
        if( Enemy.TileID == ETileType.INACTIVE_HERO ) return 0;                               // Hero dont attack inactive

        if( Enemy.Body.GetImmunityShieldState() ) return 0;                                   // Immunity shield active
        if( Enemy.TileID == ETileType.SCORPION )                                              // No second attack to stepper per turn
        {
            if( CheckScorpionAttackBlock( from, tg, Enemy ) ) return 0;                       // Compound Scorpion block
            if( AttackedByHeroMelee ) return 0;
        }

        if( Enemy.TileID == ETileType.ORB ) return 0;                                         // No Orb Attacking 
        if( Enemy.TileID == ETileType.FISH ) return 0;                                        // No Fish Attacking 

        if( Enemy.TileID == ETileType.MOTHERWASP )                                            // Mother wasp invulnerable while having children
        if( Enemy.Body.ChildList.Count > 0 ) return 0;

        if( Map.GMine( EMineType.BRIDGE, from ) )                                             // Prevent crossing floor bridge attack
        if( Map.GMine( EMineType.BRIDGE, tg ) )
        if( Unit.Control.Floor != Enemy.Control.Floor ) 
            return 0;

        if( Unit.Control.Floor == 0 )                                                         // no high att from ground
        if( Enemy.Control.Floor == 2 ) return 0; 

        bool cfrom = Map.IsClimbable( from );                                                 // Climbing restriction
        bool cto = Map.IsClimbable( tg );
        if( cfrom == false && cto == true )
        if( DamageType != EDamageType.MELEE ||
            Unit.UnitType != EUnitType.HERO )            
            return 0;  

        if( Enemy.TileID == ETileType.FROG )                                                  // No attack to Swimming Frogs
        if( Enemy.Control.ControlType == EControlType.ROACH ) 
            return 0;        
        
        if( Enemy.Body.InflictedDamageRate <= 0 ) return 0;                                   // Damage Multiplier equals to zero 

        if( Unit.UnitType == EUnitType.HERO )                                                 // Attack from outside fog to inside Fog
        {
            Unit fromFog = Controller.GetFog( Unit.Pos );
            Unit toFog = Controller.GetFog( Enemy.Pos );
            if( fromFog == null && toFog != null ) return 0;
        }
   
        if( TotalDamage <= 0 ) return 0;                                                      // Attack Damage Below zero

        if( Unit.UnitType == EUnitType.HERO   ||                                              // No normal att to realtime monsters
            Unit.ProjType( EProjectileType.FIREBALL ) ||                                               
            Unit.ProjType( EProjectileType.BOOMERANG ) )
        {
            //if( Enemy.Control.RealtimeSpeedFactor > 0 )
            //if( Enemy.TileID != ETileType.QUEROQUERO )
            //if( Enemy.TileID != ETileType.SCORPION )
            //if( IsRealtime == false ) return 0;

            //if( Enemy.Control.RealtimeSpeedFactor <= 0 )                                      // No realtime aat to normal monsters
            //    if( IsRealtime == true ) return 0;

            if( Enemy.TileID == ETileType.SPIDER ||                                            // No Attacking against these
                Enemy.TileID == ETileType.HUGGER )
            {
                if( TargettingType != ETargettingType.SPELL ||                                  
                    G.Hero.Body.Sp[ ID ].IsAttackingSpell() )
                    return 0;                                
            }           

            int num = BabyData.GetNumActiveAlgaeinTile( tg );                                 // Algae in tile
            if( num > 0 ) return 0;
        }

        if( Unit.UnitType == EUnitType.MONSTER )                                              // No monster attack on tiles with eggs
        {
            Unit frog = Map.I.GetUnit( ETileType.FROG, G.Hero.GetFront() );                   // new: frontfacing frog geive immunity against stepping monsters
            if( frog && Map.Stepping() ) return 0;

            int num = BabyData.GetNumActiveAlgaeinTile( tg, EAlgaeBabyType.EGG );             // Algae in tile
            if( num > 0 ) return 0;

            if( Body.HookIsStuck ) return 0;                                                  // Hook is stuck in the monster, so he does not attack
        }

        if( Unit.UnitType == EUnitType.HERO )                                                 // Do not do puncture attack traversing hook to destination if only one live remaining (to avoid hook lost)
        if( TargettingType == ETargettingType.SPELL )
        {
            if( G.Hero.Body.Sp[ ID ].Type == ESpiderBabyType.HOOK_CW ||
                G.Hero.Body.Sp[ ID ].Type == ESpiderBabyType.HOOK_CCW )
            if( attId == 1 )
            if( Enemy.Body.Lives <= 1 ) return 0;

            if( Enemy.Control.BeingMudPushed ) return 0;                                      // Enemy is beig mud pushed
        }

		if( Enemy.UnitType != EnemyTargetType ) return 0;                                     // Target is not enemy

        if( Map.I.Hero.Control.MonsterPushLevel >= 4 )                                        // Pushed monster attack restrained
        {
            if( Unit.UnitType == EUnitType.MONSTER )
                if( Unit.Control.IsBeingPushedByHero ) return 0;

            if( BerserkAttack == false )
            if( Unit.UnitType == EUnitType.HERO )
            if( Enemy.Control.IsBeingPushedByHero ) return 0;
        }

        if( Enemy.Control.IsFlyingUnit )                                                      // Cannot attack dragons from distance if they're resting
        if( Enemy.Control.Resting ) return 0;

        if( Map.I.CurrentArea != -1 )
        if( Map.I.CurArea.SongOfPeace > 0 )                                                   // Song of Peace
        if( Unit.UnitType == EUnitType.MONSTER )  
        {
            return 0;
        }     

        EDirection atdr = Enemy.Body.GetPosRelativeDir( Enemy.Dir, Unit.Pos );

        if( Enemy.Body.ProtectionLevel == 1 )                                                 // Error: Frontal Protection
        if( atdr == EDirection.N )
        {
            Map.I.ShowMessage( Language.Get( "ERROR_FRONTALPROTECTION" ) );
            return BREAK;
        }

        if( Enemy.Body.ProtectionLevel == 2 )                                                 // Error: Frontside Protection
        if( atdr == EDirection.NW || atdr == EDirection.NE || atdr == EDirection.N )
        {
            Map.I.ShowMessage( Language.Get( "ERROR_FRONTSIDEPROTECTION" ) );
            return BREAK;
        }

        if( Unit.ProjType( EProjectileType.BOOMERANG ) )
        if( Enemy.ValidMonster == false ) return 0;

        if( Unit.UnitType == EUnitType.HERO )                                                 // no melee skill
		{
        if( Enemy.ValidMonster == false ) return 0;

		if( DamageType == EDamageType.MELEE )
			{
				if( Unit.Body.MeleeAttackLevel < 1 )                                          // Error: No Melee
				{
					return 0;
				}

				if( Unit.Control.LastMoveType == EMoveType.FRONT &&                           // Error: No frontal melee attack
                    Unit.Body.MeleeAttackLevel < 2 )
				{
					return 0;
				}

				if( Unit.Control.LastMoveType == EMoveType.FRONTSIDE &&                       // Error: No frontside melee attack
					Unit.Body.MeleeAttackLevel < 2 )
				{
					return 0;
				}

				if( Unit.Control.LastMoveType == EMoveType.SIDE &&                           // Error: No side melee attack
					Unit.Body.MeleeAttackLevel < 2 )
				{
					return 0;
				}

				if( Unit.Control.LastMoveType == EMoveType.BACKSIDE &&                       // Error: No backside melee attack
					Unit.Body.MeleeAttackLevel < 3 )
				{
					return 0;
				}

				if( Unit.Control.LastMoveType == EMoveType.BACK &&                          // Error: No Back melee attack
					Unit.Body.MeleeAttackLevel < 3 )
				{
					return 0;
				}
			}

			if( DamageType == EDamageType.RANGED )
			{
				if( Unit.Body.RangedAttackLevel < 1 )                                          // Error: No RANGED
				{
					return 0;
				}

				if( Unit.Control.LastMoveType == EMoveType.FRONT &&                           // Error: No frontal RANGED attack
					Unit.Body.RangedAttackLevel < 2 )
				{
					return 0;
				}

				if( Unit.Control.LastMoveType == EMoveType.FRONTSIDE &&                       // Error: No frontside RANGED attack
					Unit.Body.RangedAttackLevel < 2 )
				{
					return 0;
				}

				if( Unit.Control.LastMoveType == EMoveType.SIDE &&                           // Error: No side RANGED attack
					Unit.Body.RangedAttackLevel < 2 )
				{
					return 0;
				}

				if( Unit.Control.LastMoveType == EMoveType.BACKSIDE &&                       // Error: No backside RANGED attack
					Unit.Body.RangedAttackLevel < 3 )
				{
					return 0;
				}

				if( Unit.Control.LastMoveType == EMoveType.BACK &&                          // Error: No Back RANGED attack
					Unit.Body.RangedAttackLevel < 3 )
				{
					return 0;
				}
			}
		}

        if( DamageType == EDamageType.RANGED )                                             // Arrow block missile
        {
            List<Vector2> at = new List<Vector2>();
            at.Add( Unit.Pos );
            for( int i = 0; i < AttackTg.Count; i++ ) at.Add( AttackTg[ i ] );

            bool ok = false;
            for( int i = 1; i < at.Count; i++ )
            {
                if(!Map.PtOnMap( Map.I.Tilemap, at[ i ] ) ) break;
                if( Unit.UnitType == EUnitType.HERO || Unit.TileID == ETileType.MOSQUITO )
                if( Map.I.Unit[ ( int ) at[ i ].x, ( int ) at[ i ].y ] )
                if( Map.I.Unit[ ( int ) at[ i ].x, ( int ) at[ i ].y ].ValidMonster )
                    {
                        //Enemy = Map.I.Unit[ ( int ) at[ i ].x, ( int ) at[ i ].y ]; // removed because it was causing slime passthroug shot bug, test it
                        ok = true; break;
                    }

                if( Unit.UnitType == EUnitType.MONSTER )
                    if( Enemy.Pos == at[ i ] )
                    {
                        ok = true; break;
                    }
            }
            if( ForcedEnemy == null )
            if( ok == false ) return 0;
        }
        if( Unit.ProjType( EProjectileType.BOOMERANG ) == false )
        if( Unit.CheckLeverBlock( false, Unit.Pos, Unit.GetFront(), false ) ) return 0;                          // Lever Block
        if( DamageType == EDamageType.MELEE )
        if( Map.I.CheckArrowBlockFromTo( from, tg, Unit ) )
        {
            if( Unit.UnitType == EUnitType.HERO )
                Map.I.ShowMessage( Language.Get( "ERROR_ARROWBLOCKATTACK" ) );
            return 0;
        }
        
		if( Enemy.UnitType == EUnitType.HERO )                                                                      // Transfer Damage to hero if inactive is the target
        {
			if( Enemy.TileID == ETileType.INACTIVE_HERO ) Enemy = Map.I.Hero;
        }
        else
        if( Controller.GetBlocker( tg ) != null ) return 0;                                                         // Blocker

		float att = Enemy.Body.ReceiveDamage( 0, DamageType, Unit, this );                                          // Enemy Receives Damage

        if( InvalidateAttack ) return 0;

        if( att > 0 )
        if( DamageType == EDamageType.MELEE )
            MasterAudio.PlaySound3DAtVector3( "Melee Attack", transform.position );

		if( DamageType == EDamageType.RANGED )                                                                      // Arrow animation
		{
            EBoltType tp = EBoltType.Arrow;
            if( TargettingType == ETargettingType.SPELL )
                tp = G.Hero.Body.Sp[ ID ].BoltType;

            if( att > 0 )
                CreateArrowAnimation( Enemy, Vector2.zero, tp );
		}

        string atype = "Melee";
        if( DamageType == EDamageType.RANGED ) atype = "Ranged";

        string intro = "" + Unit.PrefabName + " has Performed a " + UI.I.AttackType + " " + atype + " Attack against a " + Enemy.PrefabName + ".\n";
        if( Unit.UnitType == EUnitType.MONSTER )
            intro = "A " + Unit.PrefabName + " has Performed a " + UI.I.AttackType + " " + atype + " Attack against " + Enemy.PrefabName + ".\n";

        float perc = Util.Percent( UI.I.ShieldReduction, UI.I.Shield );

        float attc = ( TotalDamage + UI.I.SurplusBonus ) - perc;

        //if( Unit.UnitType == EUnitType.HERO )
        //    Debug.Log( "attc " + attc + " surp " + UI.I.SurplusBonus + " dt " + DamageType + " Ambush " + UI.I.AmbushBonus + " corn " + UI.I.ConeringBonus );

        float tta = attc + Util.Percent( UI.I.AmbushBonus + UI.I.ConeringBonus + UI.I.SprinterBonus, attc );

        //if( UI.I.MessageTurn != Map.I.AreaTurnCount ) UI.I.TurnInfoLabel.text = "";

        //if( Attack.IsRealtime == false )
        //{
        //string breakl = "\n";
        //if( UI.I.TurnInfoLabel.text == "" ) breakl = "";  
        //    UI.I.TurnInfoLabel.text += breakl + intro +
        //    "Brute Dmg: " + TotalDamage + UI.I.SurplusInfo + " - Shield: " + UI.I.ShieldReduction + "% of " + UI.I.Shield + " = " + perc + " - Dmg Deduction: " +
        //                                 attc.ToString( "0.0" ) + UI.I.AmbushInfo + UI.I.CorneringInfo + UI.I.SprinterInfo + " - Total: " + tta.ToString( "0.0" );
        //}
		return att;
	}

    public bool CheckScorpionAttackBlock( Vector2 from, Vector2 to, Unit enemy )
    {
        if( Unit.UnitType != EUnitType.HERO )
        if( enemy.UnitType != Unit.UnitType )            
            return false;

        if( TargettingType == ETargettingType.SPELL )                                  // Just a Spell, dont breed
        {
            ESpiderBabyType type = G.Hero.Body.Sp[ ID ].Type;
            if( type != ESpiderBabyType.THROWING_AXE )
            if( type != ESpiderBabyType.SPEAR )
            if( type != ESpiderBabyType.HOOK_CW )
            if( type != ESpiderBabyType.HOOK_CCW )
                return false;
        }

        Unit scp = Map.I.GetUnit( ETileType.SCORPION, to );
        if( scp == null ) return false;
        if( scp.Control.UnitProcessed ) return false;
        EDirection md = Util.GetTargetUnitDir( from, to );
        List<Vector2> ul = new List<Vector2>();
        List<Vector2> ul2 = new List<Vector2>();
        List<Vector2> tgl = new List<Vector2>();
        List<int> st = new List<int>();
        Controller.blockiID = new List<int>();
        List<int> bvar = new List<int>();
        List<ESpiderBabyType> bt = new List<ESpiderBabyType>();

        bool res = false;
        for( int i = 0; i < scp.Body.Sp.Count; i++ )
        {
            if( scp.Body.Sp[ i ].Type != ESpiderBabyType.NONE )
            {
                if( Spell.IsSpawnAble( scp.Body.Sp[ i ].Type ) )                        // Do not Spawn these
                {
                    int l = 1; int dr = i;
                    if( i >= 16 ) { l = 3; dr -= 16; } else
                    if( i >= 8 ) { l = 2; dr -= 8; }
                    ul.Add( scp.Pos + Manager.I.U.DirCord[ dr ] * l );
                    ul2.Add( scp.Pos + Manager.I.U.DirCord[ dr ] * ( l + 1 ) );
                }
                st.Add( i );
                bt.Add( scp.Body.Sp[ i ].Type );

                int vari = -1;
                if( scp.Body.BabyVariation != null )
                    if( scp.Body.BabyVariation.Count > 0 )
                        vari = scp.Body.BabyVariation[ i ];
                bvar.Add( vari );
            }
        }

        bool jump = false;
        for( int i = 0; i < ul.Count; i++ )                                             // Tests possible spawn positions
        {
            bool tg1 = scp.CanMoveFromTo( false, scp.Pos, ul[ i ], scp );               
            bool tg2 = scp.CanMoveFromTo( false, scp.Pos, ul2[ i ], scp );

            if( tg1 == false && tg2 == false ) Controller.blockiID.Add( st[ i ] );
            else
                if( tg1 == true && jump == false ) tgl.Add( ul[ i ] );                  // spawns in the primary target
                else 
                {
                    if( tg2 )
                    {
                        tgl.Add( ul2[ i ] );                                             // spawns in the secondary target
                        jump = true; 
                    }
                    else Controller.blockiID.Add( st[ i ] );                             // secondary is blocked
                }
        }
          
        if( Controller.blockiID != null && Controller.blockiID.Count >= 1 )             // Blocked attack
        {
            scp.Control.UnitProcessed = true;
            InvalidateAttack = true;
            return true;
        }

        for( int i = 0; i < tgl.Count; i++ )
        {
            ETileType tile = ETileType.NONE;
            Unit prefab = scp;                                                                      // Scorpion baby
            if( bt[ i ] == ESpiderBabyType.SCORPION ) tile = ETileType.SCORPION;
            if( bt[ i ] == ESpiderBabyType.SPIDER ) tile = ETileType.SPIDER;
            if( bt[ i ] == ESpiderBabyType.ITEM ) tile = ETileType.ITEM;
            prefab = Map.I.GetUnitPrefab( tile );

            GameObject go = Map.I.CreateUnit( prefab, tgl[ i ] );                                  // Spawns new Scorpions
            if( go )
            {
                Unit un = go.GetComponent<Unit>();
                for( int j = 0; j < un.Body.Sp.Count; j++ )
                    un.Body.Sp[ j ].Type = ESpiderBabyType.NONE;
                
                if( Map.I.RM.HeroSector.MoveOrder.Contains( un ) == false )
                if( un.ValidMonster )
                    Map.I.RM.HeroSector.MoveOrder.Add( un );

                if( bt[ i ] == ESpiderBabyType.SPIDER ||                                         // normal monster spawned: no babies
                    bt[ i ] == ESpiderBabyType.SCORPION )
                {
                    un.Control.SkipMovement = 1;
                    un.Control.UnitMoved = true;
                    un.Control.MoveOrderID = Map.I.RM.HeroSector.MoveOrder.Count - 1;
                    un.Control.Resting = false;
                }

                if( bt[ i ] == ESpiderBabyType.ITEM )
                {
                    un.Variation = bvar[ i ];
                    if( un.Variation >= 0 )
                        un.Spr.spriteId = G.GIT( un.Variation ).TKSprite.spriteId;
                    un.Body.StackAmount = 1;
                    un.Body.BonusItemList = null;
                }
                un.Control.SmoothMovementFactor = 0;
                un.Control.LastPos = scp.Pos;
                un.Control.OldPos = scp.Pos;
                un.Control.TurnTime = 0;
                un.Control.UnitMoved = true;
                un.Control.AnimationOrigin = scp.Pos;
            }
        }
        scp.Control.UnitProcessed = true;
        return res;
    }

    public ArcherArrowAnimation CreateArrowAnimation( Unit enemy, Vector2 target, EBoltType type )
    {
        if( Unit.ProjType( EProjectileType.BOOMERANG ) ) return null;
        if( Unit.ProjType( EProjectileType.FIREBALL ) ) return null;
        Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Archer Arrow" ); 
        if( Unit.UnitType == EUnitType.HERO )
        {
            if( type == EBoltType.Arrow )
                MasterAudio.PlaySound3DAtVector3( "Archer Shot", transform.position );
        }
        else
        {
            MasterAudio.PlaySound3DAtVector3( "Mosquito Attack", transform.position );
            Unit.Body.UpdatePunchEffect( Map.I.Hero, EDamageType.RANGED, true );
            if( Unit.TileID == ETileType.DRAGON1 )
            {
                type = EBoltType.Shit;
                target = G.Hero.Pos;
            }
        }

        tr.position = AttackOrigin;
        tr.eulerAngles = Util.GetRotationAngleVector( AttackOriginalDirection );
        ArcherArrowAnimation ar = tr.gameObject.GetComponent<ArcherArrowAnimation>();
        ar.Create( AttackOrigin, enemy, tr, target, type );
        return ar;
    }
	
	public List<Vector2> GetAttackTargets()
	{
        List<Vector2> tg = new List<Vector2>();
        //if( Map.I.CurrentArea == -1 ) return null;
        if( Sector.GetPosSectorType( Unit.Pos ) == Sector.ESectorType.GATES ) return null;

        if( Unit.UnitType == EUnitType.MONSTER )
        if( Unit.ProjType( EProjectileType.FIREBALL ) == false )
        if( Map.Stepping() )                                                                       // Stepping based attack not advancing turn
        if( Map.I.AdvanceTurn == false ) return null;

        switch( TargettingType )
        {
            case ETargettingType.FRONTFANCING:
            {                
                AddTg( ref tg, Unit.Pos + Manager.I.U.DirCord[ ( int ) Unit.Dir ] );

                if( Unit.UnitType == EUnitType.HERO )
                {
                    if( Map.I.GetUnit( ETileType.ARROW, Unit.Pos ) ||                              // Hero over arrow or facing: Block
                        Map.I.GetUnit( ETileType.ARROW, Unit.GetFront() ) )
                    {
                        tg = null;
                        break;
                    }

                    if( Map.I.RM.RMD.LimitedMeleeAttacksPerCube != -1 )                            // Not enough Melee Attacks
                    if( Item.GetNum( ItemType.Res_Melee_Attacks ) < 1 ) return null;
                }
            }
            break;
            case ETargettingType.HUGGER:
            {
                if( TotalAttackTime == 0 ) return null;                                                // No attack
                if( TotalRange == 0 ) return null; 
                tg = new List<Vector2>();
                bool res = Map.I.HasLOS( Unit.Pos, G.Hero.Pos, true, null, true, true, "", ( int ) TotalRange );
                int dist = Util.Manhattan( Unit.Pos, G.Hero.Pos );
                if( dist > 1 )
                    CreateArrowAnimation( null, Map.I.LastLOSPos, EBoltType.Hugger );
                SpeedTimeCounter = 0;
                if( dist > 1 && dist <= TotalRange )
                if( res )
                {
                    tg.Add( G.Hero.Pos );
                    return tg;
                }
            }
            break;
            case ETargettingType.INFECTION:
            {
                if( Unit.Body.IsInfected )
                if( Util.Manhattan( Unit.Pos, G.Hero.Pos ) <= Unit.Body.InfectedRadius )
                    {
                        bool res = Map.I.HasLOS( Unit.Pos, G.Hero.Pos, true );
                        //if( Util.IsNeighbor( Unit.Pos, G.Hero.Pos ) == false )
                        {
                            Unit mudf = Map.I.GetMud( G.Hero.Pos );
                            if( mudf ) res = false;
                            mudf = Map.I.GetMud( Unit.Pos );                                // mud x infected
                            if( mudf ) res = false;
                        }
                        if( res )
                        {
                            tg.Add( G.Hero.Pos );
                            return tg;
                        }
                    }
                return null;
            }
            break;
            case ETargettingType.ROACHBASIC:
            {
                Unit.UpdateDirection();
                Vector2 tar = Unit.Pos + Manager.I.U.DirCord[ ( int ) Unit.Dir ];
                tg = new List<Vector2>();

                if( Unit.TileID == ETileType.HUGGER )
                    return null;
                
                if( Unit.TileID == ETileType.ROACH ||                                                    // these can side attack
                    Unit.TileID == ETileType.SCARAB )
                if( Unit.Control.ForcedFrontalMovementDir == EDirection.NONE )
                if( Util.IsNeighbor( Unit.Pos, G.Hero.Pos ) ) tar = G.Hero.Pos;
                tg.Add( tar );

                if( Map.I.GetPosArea( tar ) == Map.I.GetPosArea( Unit.Pos ) )
                {
                    if( Map.I.Hero.Pos == tar )
                    {
                        //EDirection dr = Util.GetRotationToTarget( Unit.Pos, tg[ 0 ] );
                        //Unit.RotateTo( dr );
                        return tg;
                    }

                    if( Map.I.Unit[ ( int ) tar.x, ( int ) tar.y ] != null )
                    if( Map.I.Unit[ ( int ) tar.x, ( int ) tar.y ].TileID == ETileType.INACTIVE_HERO )
                        {
                            return tg;
                        }
                }
                return null;
            } 
		break;
		case ETargettingType.MISSILE:
        case ETargettingType.SPELL:
        {
            if( TargettingType == ETargettingType.SPELL )
            {
                if( ID >= 8 )                                                                                                           // Fixed Spell slot origin
                    AttackOrigin = Unit.Pos + G.Hero.Body.Sp[ ID ].Delta;
                else
                    AttackOrigin = Unit.Pos + Util.GetRelativePosition( Unit.Dir, ( EDirection ) ID );                                  // moving spell slot origin

                if( G.Hero.Body.Sp[ ID ].Type == ESpiderBabyType.TORCH ) return null;
                if( G.Hero.Body.Sp[ ID ].Type == ESpiderBabyType.DOUBLE_ATTACK ) return null;
                if( G.Hero.Body.Sp[ ID ].Type == ESpiderBabyType.WEBTRAP ) return null;
                if( G.Hero.Body.Sp[ ID ].Type == ESpiderBabyType.ATTACK_BONUS ) return null;
            }

            if( TargettingType == ETargettingType.SPELL )
            if( G.Hero.Body.Sp[ ID ].Type == ESpiderBabyType.THROWING_AXE )
            {
                if( Map.I.AdvanceTurn == false ) return null;
                if( Map.I.CurrentMoveTrial == EActionType.BATTLE ) return null;
                if( Map.I.CurrentMoveTrial == EActionType.WAIT ) return null;

                for( int i = 0; i < 8; i++ )
                {
                    Vector2 tgg = AttackOrigin + Map.I.KnightTG[ i ];
                    if( Map.IsWall( AttackOrigin ) == false )
                    {
                        tg.Add( tgg );                                                                                                 // Throwing axe knight tg
                        for( int s = 0; s < G.Hero.Body.Sp.Count; s++ )                                                                // Counts duplicators in the way  
                            Spell.CheckDuplicator( tgg, s );
                    }
                }
                ShotPassThroughCount = DuplicatorCount;
                return tg;
            }

            if( TargettingType == ETargettingType.SPELL )                                                                               // Hook attack
            if( Spell.IsHook( G.Hero.Body.Sp[ ID ].Type ) )
            {
                int dr = 0;
                if( G.Hero.Body.Sp[ ID ].Type == ESpiderBabyType.HOOK_CW )                                                              // Hook rotation attack
                if( G.Hero.Control.LastAction == EActionType.ROTATE_CW ) dr = -1;
                if( G.Hero.Body.Sp[ ID ].Type == ESpiderBabyType.HOOK_CCW )
                if( G.Hero.Control.LastAction == EActionType.ROTATE_CCW ) dr = +1;
                if( dr == 0 )
                {
                    int id2 = ( int ) Util.RotateDir( ( int ) G.Hero.Dir, ID );
                    Vector2 pt = G.Hero.Pos + Manager.I.U.DirCord[ id2 ];
                    Spell.UpdateHookDestruction( pt );                                                                                  // Wrong direction, Destroy hook
                    return null;
                }
                AddTg( ref tg, Unit.Pos + Util.GetRelativePosition( Unit.Dir, ( EDirection ) ID ) );
                dr = ( int ) Util.RotateDir( ( int ) Unit.Dir, dr );
                AttackOrigin = Unit.Pos + Util.GetRelativePosition( ( EDirection ) dr, ( EDirection ) ID );                             // Attack is OK
                return tg;
            }

            int _range = ( int ) TotalRange;
            if( Unit.UnitType == EUnitType.HERO )
                _range += Map.I.RM.RMD.BaseHeroRangedAttackRange;
            if( TargettingType == ETargettingType.SPELL )
                _range = Sector.TSX;

            int range = 0;
            int hitmonsterdist = -1;
            bool canshoot = GetEffectiveShootingRange( ref tg, ref range, ref hitmonsterdist, false );                    // Gets effective shooting range
            if( canshoot == false ) return null;

            EDirection dir = AttackOriginalDirection;
            float bestdist = 999;
            if( Unit.UnitType == EUnitType.HERO )
            if( Map.I.CurrentArea == -1 )                                                                                 // Flying Units as Target
                {
                    List<Unit> ul = new List<Unit>();
                    List<int> rr = new List<int>();
                    int temprange = range;
                    if( hitmonsterdist != -1 ) temprange = hitmonsterdist;

                    for( int r = 1; r <= temprange; r++ )                                                                // Make a list of all flying monsters in the target radius
                    {
                        Vector2 rel = AttackOrigin + ( Manager.I.U.DirCord[ ( int ) dir ] * r );
                        int rad = 1;
                        for( int y = ( int ) rel.y - rad; y <= rel.y + rad; y++ )
                        for( int x = ( int ) rel.x - rad; x <= rel.x + rad; x++ )
                        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( x, y ) ) )
                        {
                            if( Map.I.FUnit[ x, y ] != null )                                       
                            for( int i = 0; i < Map.I.FUnit[ x, y ].Count; i++ )
                            if( ul.Contains( Map.I.FUnit[ x, y ][ i ] ) == false )
                            {
                                Unit fu = Map.I.FUnit[ x, y ][ i ];
                                if( fu.TileID == ETileType.WASP ||                           // Just add these ones if in the same shooting line as the hero
                                    fu.TileID == ETileType.MOTHERWASP )
                                if( tg.Contains( fu.Pos ) == false ) fu = null;
                                if( fu )
                                if( fu.ProjType( EProjectileType.BOOMERANG ) ||
                                    fu.TileID == ETileType.RAFT ||
                                    fu.TileID == ETileType.FOG )
                                    fu = null;

                                if( fu )
                                {
                                    ul.Add( Map.I.FUnit[ x, y ][ i ] );
                                    rr.Add( Util.Manhattan( new Vector2( x, y ), AttackOrigin ) );
                                }
                            }
                        }
                        #region old
                        //if( Util.IsDiagonal( dir ) )                                                                       // Get Diagonal tiles
                        //{
                        //    center = AttackOrigin + G.Hero.GetRelativePosition( EDirection.N, r );
                        //    left = center + Util.GetRelativePosition( dir, EDirection.SW );
                        //    right = center + Util.GetRelativePosition( dir, EDirection.SE );
                        //}
                        //else
                        //{
                        //    center = AttackOrigin + G.Hero.GetRelativePosition( EDirection.N, r );                          // Get Ortho tiles
                        //    left = center + Util.GetRelativePosition( dir, EDirection.W );
                        //    right = center + Util.GetRelativePosition( dir, EDirection.E );
                        //}

                        //if( Map.I.FUnit[ ( int ) center.x, ( int ) center.y ] != null )                                        // Optimize: add all at once
                        //{
                        //    ul.AddRange( Map.I.FUnit[ ( int ) center.x, ( int ) center.y ] );
                        //    for( int i = 0; i < Map.I.FUnit[ ( int ) center.x, ( int ) center.y ].Count; i++ ) rr.Add( r );
                        //}
                        //if( Map.I.FUnit[ ( int ) left.x, ( int ) left.y ] != null )                                           // Optimize: add all at once
                        //{
                        //    ul.AddRange( Map.I.FUnit[ ( int ) left.x, ( int ) left.y ] );
                        //    for( int i = 0; i < Map.I.FUnit[ ( int ) left.x, ( int ) left.y ].Count; i++ ) rr.Add( r );
                        //}
                        //if( Map.I.FUnit[ ( int ) right.x, ( int ) right.y ] != null )                                         // Optimize: add all at once
                        //{
                        //    ul.AddRange( Map.I.FUnit[ ( int ) right.x, ( int ) right.y ] );
                        //    for( int i = 0; i < Map.I.FUnit[ ( int ) right.x, ( int ) right.y ].Count; i++ ) rr.Add( r );
                        //}*/
                        #endregion
                    }

                    for( int k = 1; k < ul.Count; k++ )        // reorder targets by range                                                                            // Sorts score list        
                    for( int j = 0; j < ul.Count - k; j++ )
                    if ( ( rr[ j ] > rr[ j + 1 ] ) )
                    {
                        int temp = rr[ j ];
                        rr[ j ] = rr[ j + 1 ];
                        rr[ j + 1 ] = temp;
                        Unit tempun = ul[ j ];
                        ul[ j ] = ul[ j + 1 ];
                        ul[ j + 1 ] = tempun;
                    }

                    for( int i = 0; i < ul.Count; i++ )
                    if ( ul[ i ].Body.IsDead == false )
                    if ( ul[ i ].Control.Resting == false )
                        {
                            Unit fl = ul[ i ];
                            int r = rr[ i ];
                            Vector2 l1 = AttackOrigin + ( Manager.I.U.DirCord[ ( int ) dir ] * .5f );
                            Vector2 l2 = l1 + ( Manager.I.U.DirCord[ ( int ) dir ] * range );
                            float linedist = Util.GetDistanceToLine( l1, l2, fl.transform.position );                                 // Calculates Distance to shot line
                            bool ok = false;
                            float minrange = Vector2.Distance( AttackOrigin, fl.transform.position );
                            float herodist = minrange;
                        
                            if( fl.TileID == ETileType.DRAGON1 )
                            {
                                float maxlinedist = Map.I.RM.RMD.BaseMonsterTargetRadius + (
                                G.Hero.Control.FlyingTargetting * Map.I.RM.RMD.ExtraMonsterTargetRadiusPerLevel );
                                maxlinedist = Util.Percent( fl.Control.TargettingRadiusFactor, maxlinedist );

                                if( linedist <= maxlinedist ) ok = true;

                                float maxrange = Util.Manhattan( fl.Pos, AttackOrigin );

                                if( maxrange > _range ) ok = false;                                                               // Min and Max ranges
                                if( minrange < .5f ) ok = false;

                                if( AttackOrigin == fl.Pos ) ok = false;
                                if( Map.IsWall( fl.Pos ) ) ok = false;
                                if( !ok && Map.I.HasLOS( AttackOrigin, fl.Pos, true,
                                    G.Hero, true, true ) == false ) ok = false;
                            }
                            else
                            if( fl.TileID == ETileType.QUEROQUERO )
                            {
                                float maxlinedist = Map.I.RM.RMD.BaseMonsterTargetRadius + (
                                G.Hero.Control.FlyingTargetting * Map.I.RM.RMD.ExtraMonsterTargetRadiusPerLevel );

                                if( linedist <= maxlinedist ) ok = true;
                                //Vector2 rel = AttackOrigin + ( Manager.I.U.DirCord[ ( int ) dir ] * range ); 
                                float maxrange = Util.Manhattan( fl.Pos, AttackOrigin );
  
                                if( maxrange > _range ) ok = false;                                                               // Min and Max ranges
                                if( minrange < .5f ) ok = false;

                                if( AttackOrigin == fl.Pos ) ok = false;
                                if( Map.IsWall( fl.Pos ) ) ok = false;
                                if( !ok && Map.I.HasLOS( AttackOrigin, fl.Pos, true,
                                    G.Hero, true, true ) == false ) ok = false;
                            }
                            else
                            if( fl.TileID == ETileType.WASP )                                                          // Wasps always hit if inside the taget tile (no distance check)
                            {
                                if( fl.Control.Mother )
                                if( fl.Control.Mother.Control.Resting )                                                // Awakes mother only with frontal att
                                if( ForcedEnemy == null )
                                    {
                                        return null;
                                    }

                                Vector2 tar = AttackOrigin + ( Manager.I.U.DirCord[ ( int ) dir ] * r );
                                if( tg.Contains( fl.Pos ) )
                                if( tar == fl.Pos ) ok = true;

                                List<Unit> wl = Map.I.GF( fl.Pos, ETileType.WASP );
                                for( int w = 0; w < wl.Count; w++ )
                                if ( wl[ w ].Control.Mother )
                                if ( wl[ w ].Control.Mother.Control.Resting )                                          // resting wasp in the tile voids attack
                                    return null;

                                int shield = 0, fire = 0, enr = 0; Unit normal = null;                                 
                                List<Unit> cl = Map.I.GF( tar, ETileType.WASP );                        
                                if ( cl != null )
                                for( int c = 0; c < cl.Count; c++ )
                                    {
                                        if( cl[ c ].Body.EnragedWasp ) enr++;
                                        if( cl[ c ].Body.ShieldedWasp ||
                                            cl[ c ].Body.CocoonWasp ) shield++;
                                        if( cl[ c ].Control.FireMarkedWaspFactor > 0 )
                                            fire++;
                                        else 
                                            normal = cl[ c ];
                                    }

                                if( shield <= 0 )
                                if( fl.Body.ShieldedWasp == false )
                                if( tg.Contains( fl.Pos ) )                                                   // Force no shielded wasp as tg
                                if( Map.I.IsInTheSameLine( AttackOrigin, fl.Pos ) ) /// new
                                {
                                    ForcedEnemy = fl;
                                    return tg;
                                }

                                if( shield >= 1 && r > 1 ) return null;                                       // Shielded Wasp no far shooting                 

                                if( shield >= 1 && r == 1 )
                                if( tg.Contains( fl.Pos ) )
                                {
                                    if( fl.Body.ShieldedWasp )                                                // force shielded as tg
                                    {
                                        ForcedEnemy = fl;
                                        return tg;
                                    }
                                }
                                if( normal )
                                if( fire > 0 || enr > 0 )                                                    // force att to non firemarked if crowded
                                {
                                    ForcedEnemy = normal;
                                    return tg;
                                }
                            }
                            else
                            if( fl.TileID == ETileType.MOTHERWASP )                                                               // Mother wasp invulnerable while having children
                            {            
                                Vector2 tar = AttackOrigin + ( Manager.I.U.DirCord[ ( int ) dir ] * r );
                                if( tg.Contains( fl.Pos ) )
                                if( tar == fl.Pos ) ok = true;
                                if( fl.Body.ChildList.Count > 0 )
                                if( Map.I.IsInTheSameLine( AttackOrigin, fl.Pos ) )
                                    ok = false;    
                            }

                            #region Test Precision

                            //int posx = 0, posy = 0;
                            //Map.I.Tilemap.GetTileAtPosition( fl.transform.position, out posx, out posy );                        // REVERT TO UNIT POS
                            //fl.Pos = new Vector2( posx, posy );

                            //float amt = .01f;
                            //Vector3 amtv = new Vector3( 0, 0, 0 );
                            //if( Input.GetKey( KeyCode.E ) ) amtv.y += amt;
                            //if( Input.GetKey( KeyCode.D ) ) amtv.y -= amt;
                            //if( Input.GetKey( KeyCode.S ) ) amtv.x -= amt;
                            //if( Input.GetKey( KeyCode.F ) ) amtv.x += amt;
                            //fl.transform.position = fl.transform.position + amtv;

                        //    Unit arrow = Map.I.GetUnit( ETileType.ARROW, fl.Pos );
                        //    if( arrow )
                        //        for( int i = 0; i <= range; i++ )
                        //        {
                        //            Vector2 tgg = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.N, i + 1 );
                        //            if( tgg == fl.Pos ) if( tg[ i ] == new Vector2( -1, -1 ) ) ok = false;
                        //        }

                        //    if( fl.TileID == ETileType.DRAGON3 )                           
                        //    {
                        //        if( dist > Map.I.RM.RMD.BaseHeadShotRadius ) ok = false;

                        //        if( fl.Control.FlightDir == G.Hero.Dir ||                                                        // No Same Angle Shot
                        //            fl.Control.FlightDir == Util.GetInvDir( G.Hero.Dir ) ) ok = false;

                        //        if( Attack.IsRealtime ) ok = false;
                        //        if( Attack.IsMoveShot == false ) ok = false;                                                     // Dragon 3 Only hit by move Shot

                        //        if( ok )                                                                                           // Headshot!
                        //        {
                        //            Message.GreenMessage( "Headshot! " + ( dist * 100 ).ToString( "0.0" ) + " cm!" );     

                        //            Map.I.Deb = "Headshot! " + ( dist * 100 ).ToString( "0.0" ) + " cm!";
                        //        }
                        //        else
                        //        {
                        //            float needed = ( dist - Map.I.RM.RMD.BaseHeadShotRadius ) * 100;
                        //            if( needed > 0 && dist < .5f )                                                                // Bad Shot Attampted
                        //            {
                        //                    Message.RedMessage( "Missed for " + needed.ToString( "0.0" ) + " cm!" );

                        //                Map.I.Deb = "Missed for " + needed.ToString( "0.0" ) + " cm!";

                        //                    G.Hero.Body.ReceiveDamage( 10, EDamageType.MISSILE, Unit, this );
                        //            }
                        //            else
                        //                Map.I.Deb = "";
                        //        }
                        //    }

                        //    if( ok )
                        //    {
                        //        ForcedEnemy = fl;
                        //        return tg;
                        //    }
                        //}

                            #endregion

                            if( fl.TileID == ETileType.QUEROQUERO )                                                                   // Dragon 3 Only hit by move Shot                                                                              
                            {
                                if( Attack.IsMoveShot == false ) return tg; 
                                if( linedist > Map.I.RM.RMD.BaseHeadShotRadius ) ok = false;

                                if( fl.Control.FlightDir == AttackOriginalDirection ||                                                // No Same Angle Shot
                                    fl.Control.FlightDir == Util.GetInvDir( AttackOriginalDirection ) ) ok = false;

                                bool ghost = false;
                                if( ok )                                                                                              // Headshot!
                                {
                                    float mind = fl.Body.QQMinDamage;
                                    float maxd = fl.Body.QQMaxDamage;
                                    float accuracy = ( 1f - ( linedist / Map.I.RM.RMD.BaseHeadShotRadius ) ) * 100f;
                                    accuracy = Mathf.Clamp( accuracy, 0f, 100f );  
                                    QQHPDamage = Util.GetCurveVal( accuracy, 100, mind, maxd, fl.Md.QQDamageCurve );
                                    bool eyeshot = linedist <= Map.I.RM.RMD.BaseEyeShotRadius;
                                    if( eyeshot )
                                    {
                                        Message.GreenMessage( "Eye Shot! " + ( linedist * 100 ).ToString( "0.0" ) + " cm!" );         // eye shot
                                        QQHPDamage += Util.Percent( 20, QQHPDamage );
                                    }
                                    else
                                        Message.GreenMessage( "Headshot! " + ( linedist * 100 ).ToString( "0.0" ) + " cm!" );
                                    ghost = true;
                                }
                                else
                                {                                   
                                    float needed = ( linedist - Map.I.RM.RMD.BaseHeadShotRadius ) * 100;
                                    if( Attack.IsMoveShot )
                                    if( needed > 0 && linedist < .5f )                                                               // Bad Shot Attampted
                                    {
                                        Message.RedMessage( "Missed for " + needed.ToString( "0.0" ) + " cm!" );
                                        Vector2 stg = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.N, range );
                                        CreateArrowAnimation( null, stg, EBoltType.Arrow );

                                        if( fl.Body.QQMissHeroDamage > 0 )                                                           // hero miss self damage
                                        {
                                            G.Hero.Body.ReceiveDamage( fl.Body.QQMissHeroDamage, EDamageType.BLEEDING, Unit, null );
                                            ArcherArrowAnimation ar = G.Hero.RangedAttack.CreateArrowAnimation( 
                                            null, G.Hero.Pos, EBoltType.Rock );                                                      // FX
                                            ar.transform.position = fl.Pos;
                                            Body.CreateDamageAnimation( Unit.Pos, fl.Body.QQMissHeroDamage, G.Hero );
                                            Body.CreateDeathFXAt( G.Hero.Pos, "Monster Hit" );                                       // Fx
                                        }
                                        ghost = true;
                                    }
                                }

                                if( ghost )                                                                                          // Place Ghost
                                {
                                    Map.I.HeadShotGhost.gameObject.SetActive( true );
                                    Map.I.HeadShotGhost.transform.position = fl.Spr.transform.position;
                                    Map.I.HeadShotGhost.transform.rotation = fl.Spr.transform.rotation;
                                    Map.I.HeadShotGhost.color = new Color( Map.I.HeadShotGhost.color.r, 
                                    Map.I.HeadShotGhost.color.g, Map.I.HeadShotGhost.color.b, .5f );
                                }
                            }

                            if( ok )
                            {
                                if( herodist < bestdist )
                                {
                                    ForcedEnemy = fl;
                                    bestdist = herodist;
                                }
                            }

                            if( i + 1 < rr.Count )                 // no need to proceed cheching if enemy already found and range++
                            if( rr[ i + 1 ] > rr[ i ] ) break;
                        }
                    return tg;
                }
		} 
		break;

        case ETargettingType.BOOMERANG:
        {          
            tg.Add( Unit.Pos );
            if( Controller.PassiveBoomerangAttackTg != null ) 
                ForcedEnemy = Controller.PassiveBoomerangAttackTg;
            List<Unit> fl = Map.I.GetFUnit( Unit.Pos );                         // Boomerang kill all wasps in the tile
            if( fl != null )
            for( int i = 0; i < fl.Count; i++ )
            {
                if( fl[ i ].TileID == ETileType.WASP )
                    ForcedEnemy = fl[ i ];
                if( fl[ i ].TileID == ETileType.MOTHERWASP )
                    if( fl[ i ].Body.ChildList.Count <= 0 )
                        ForcedEnemy = fl[ i ];
            }
            return tg;
        }

        case ETargettingType.FIREBALL:
        {
            tg.Add( Unit.Control.FlyingTarget );                                // idea: make fireball destroy all wasps in the tile like boomerang
            return tg;
        }

        case ETargettingType.FROG:
        {
            Unit.UpdateDirection();
            Unit water = Map.I.GetUnit( ETileType.WATER, Unit.Pos );
            if( water )                                                              // Swimming Frog Attack Disabled
                return null;

            Unit waterfr = Map.I.GetUnit( ETileType.WATER, Unit.Control.OldPos );
            if( waterfr )                                                            // leaving water Frog Attack Disabled
                return null;

            if( Map.Stepping() )                                                     // STEPPING mode
            {
                if( Unit.Control.SpeedTimeCounter >= 1 )
                    return null;
            }

            tg = new List<Vector2>();                                             
            if( Util.IsNeighbor( Unit.Pos, G.Hero.Pos ) )
            {
                tg.Add( G.Hero.Pos );
                if( G.Hero.GetFront() == Unit.Pos )
                {
                    Message.GreenMessage( "Block!" );
                    return null;
                }
                return tg;
            }
            
            return null;
        }
        break;

        case ETargettingType.STEPPER:
        {
            Unit.UpdateDirection();
            if( Map.I.AdvanceTurn == false ) return null;
            tg = new List<Vector2>();
            if( Util.IsNeighbor( Unit.Pos, G.Hero.Pos ) )
            {
                tg.Add( G.Hero.Pos );
                if( Unit.Control.ForcedFrontalMovementFactor > 0 )          // Mushroom target: Only Frontal Tile
                if( Unit.GetFront() != G.Hero.Pos ) return null;
                return tg;
            }
            return null;
        }
        break;
        case ETargettingType.DRAGON:
        {
            tg = new List<Vector2>( 1 );
            tg.Add( G.Hero.Pos );
            #region Slayer
            //if( Unit.Body.HasFullHealth() )
            //    if( Unit.Control.FlightTime < 4 ) return null;

            //Vector3 directionToTarget = Unit.Spr.transform.position - G.Hero.transform.position;           // slayer angle
            //float angle = Vector3.Angle( Unit.Spr.transform.up, directionToTarget );
            //float maxangle = Map.I.RM.RMD.BaseDragonSlayerAngle + G.Hero.Control.SlayerAngle;
            //float maxdragonhp = Map.I.RM.RMD.BaseDragonSlayerMaxHP - G.Hero.Control.SlayerMaxHP;              // slayer max hp
            //float perc = Unit.Body.GetHealthPercent();

            //Unit.RightText.text = "" + angle.ToString( "0." ) + " " + perc.ToString( "0." );
            //Unit.RightText.gameObject.SetActive( true );

            //if( angle <= maxangle / 2 )
            //    if( perc > maxdragonhp ) return null;

            //if( Map.I.HasLOS( Unit.Pos, G.Hero.Pos, true, Unit, true, true, "Dragon Shot" ) == false ) return null; 
            #endregion
            float dist = Vector2.Distance( G.Hero.transform.position, Unit.transform.position );
            if( dist > Map.I.RM.RMD.HeroTargetRadius ) return null;
            return tg;
        }
        break;

        case ETargettingType.JUMPER:
        {
            if( Unit.Control.FlightJumpPhase == 0 )
                Unit.Control.FlightJumpPhase = 1;
            else return null;

            //if( Unit.Control.JumperPhaseTimer != 0 ) return null;
            if( Util.IsInBox( G.Hero.Pos, Unit.Pos, 3 ) == false ) return null;

            tg = new List<Vector2>();
            tg.Add( G.Hero.Pos );

            Vector3 directionToTarget = Unit.Spr.transform.position - G.Hero.transform.position;                           // slayer angle
            float angle = Vector3.Angle( Unit.Spr.transform.up, directionToTarget );
            float maxangle = Map.I.RM.RMD.BaseDragonSlayerAngle + G.Hero.Control.SlayerAngle;
            float maxdragonhp = Map.I.RM.RMD.BaseDragonSlayerMaxHP - G.Hero.Control.SlayerMaxHP;                           // slayer max hp
            float perc = Unit.Body.GetHealthPercent();

            Unit.RightText.text = "" + angle.ToString( "0." ) + " " + perc.ToString( "0." );
            Unit.RightText.gameObject.SetActive( true );

            if( angle <= maxangle / 2 )
                if( perc > maxdragonhp ) return null;

            if( Map.I.HasLOS( Unit.Pos, G.Hero.Pos, true, Unit, true, true, "Dragon Shot" ) == false ) return null;
            return tg;
        }
        break;

        case ETargettingType.WASP:
        case ETargettingType.MOTHERWASP:
        {
            if( Unit.TileID == ETileType.WASP )
            if( Unit.Control.Mother.Control.Resting )
                return null;
            if( Unit.TileID == ETileType.MOTHERWASP )
            if( Unit.Control.Resting ) return null;
            if( Unit.Control.Mother && Unit.Pos == Unit.Control.Mother.Pos ) return null;

            tg = new List<Vector2>();
            tg.Add( G.Hero.Pos );
            if( Controller.MoveTickDone == false ) return null;
            if( Unit.TileID == ETileType.MOTHERWASP )
            if( Unit.Control.TickMoveList.Contains( G.HS.CurrentTickNumber ) == false ) return null;
            if( Unit.TileID == ETileType.WASP )
            if( G.HS.CurrentTickNumber != 1 ) return null;
            if( Util.Neighbor( G.Hero.Pos, Unit.Pos ) == false ) return null;
            if( Map.I.CheckArrowBlockFromTo( Unit.Pos, G.Hero.Pos, Unit ) ) return null;
            List<Unit> firel = Util.GetNeighbors( Unit.Pos, ETileType.FIRE );
            if( Unit.Control.FireMarkedWaspFactor > 0 )                                         // firemarked neighbor to fire do no attack
            for( int f = 0; f < firel.Count; f++ )
                if( firel[ f ].Body.FireIsOn ) return null;
            if( CheckBarricadeBlockForRangedAttack( Unit.Pos ) ) return null;
            Vector3 pun = Util.GetTargetUnitVector( Unit.Pos, G.Hero.Control.OldPos );
            iTween.PunchPosition( Unit.Graphic.gameObject, -pun * .7f, 0.5f );
            Unit.Body.CreateBloodSpilling( G.Hero.Pos );
            G.Hero.Body.ReceiveDamage( Unit.MeleeAttack.BaseDamage, EDamageType.MELEE, Unit, Unit.MeleeAttack );
            Map.I.StartCubeDeath();
            return null;
        }
        break;

        case ETargettingType.DRAGON3:
        {
            tg = new List<Vector2>();
            tg.Add( G.Hero.Pos );
            if( Map.I.CubeDeath ) return null;
            if( Map.I.IsInTheSameLine( Map.I.Hero.Pos, Unit.Pos ) == false ) return null;  
            float r = 0;
            EZone zone = Unit.GetPositionZone( G.Hero.Pos, ref r );
            if( zone != EZone.Front ) return null;
            if( Map.I.HasLOS( Unit.Pos, G.Hero.Pos, true, Unit, false, true, "Dragon Shot" ) ==  false ) return null;
            Map.I.StartCubeDeath();
            return tg;
        }
        break;

        case ETargettingType.SLIME:
        {
            tg = new List<Vector2>();
            tg.Add( G.Hero.Pos );

            if( ForceAttack == false )
            {
                if( Unit.Control.JumpTarget != new Vector2( -1, -1 ) ) return null;
                if( Unit.Control.FlightJumpPhase == 0 ) return null;
                if( Util.IsNeighbor( Unit.Pos, G.Hero.Pos ) == false ) return null;
            }
            ForceAttack = false;
            if( Unit.Pos + Unit.GetRelativePosition( EDirection.N,  1 ) == G.Hero.Pos ||
                Unit.Pos + Unit.GetRelativePosition( EDirection.NE, 1 ) == G.Hero.Pos ||
                Unit.Pos + Unit.GetRelativePosition( EDirection.NW, 1 ) == G.Hero.Pos )
                return tg;
            return null;
        }
        break;

        case ETargettingType.TOWER:
        {
            Unit b = Map.I.SpawnFlyingUnit( Unit.Pos, ELayerType.MONSTER, ETileType.PROJECTILE, null );
            Unit.Control.UpdateTowerRotation( true );
            b.Control.ProjectileType = EProjectileType.MISSILE;
            b.Control.ControlType = EControlType.PROJECTILE;
            b.Body.EffectList[ 1 ].gameObject.SetActive( true );
            b.Body.EffectList[ 1 ].transform.localPosition = new Vector3( 0, -0.35f, -2.8f );
            b.Spr.spriteId = 210;
            //b.Spr.spriteId = 193;
            b.Control.Mother = Unit;
            b.Variation = Unit.Variation;
            b.Spr.transform.rotation = Unit.Spr.transform.rotation;
            b.Spr.transform.localPosition = new Vector3( 0, 0, b.Spr.transform.localPosition.z );
            b.RangedAttack.TargettingType = ETargettingType.TOWER;
            b.Control.FlyingSpeed = Unit.Control.FlyingSpeed;
            b.Control.FlyingRotationSpeed = Unit.Control.FlyingRotationSpeed;
            b.Spr.color = Color.white;
            AttackMade = true;
            MasterAudio.PlaySound3DAtTransform( "Explosion 2", G.Hero.transform );
            if( iTween.Count( Unit.Spr.gameObject ) <= 0 )                                                               // Punch FX                                       
                iTween.PunchPosition( Unit.Spr.gameObject, iTween.Hash( "y", -0.5f, "delay", 0.1f, "time", .3f ) );
            return null;
        }
        break;
        }
	return tg;
	}

    public void AddTg( ref List<Vector2> tgl, Vector2 tg )
    {
        if( tgl == null )
            tgl = new List<Vector2>();
        Unit mn = Map.I.GetUnit( tg, ELayerType.MONSTER );                           // Land units
        if( mn && mn.ValidMonster )
        {
            tgl.Add( tg );
            return;
        }

        List<Unit> fl = Map.I.GetFUnit( tg );                                        // Flying units
        if( Map.I.AdvanceTurn )
        if( fl != null )
        for( int i = 0; i < fl.Count; i++ )
        {
            if( fl[ i ].ValidMonster )
            {
                tgl.Add( tg );
                ForcedEnemy = fl[ i ];
                return;
            }
        }
    }

    public int GetEffectiveShootingRange( bool penetrateMonsters )
    {
        int range = 0;
        List<Vector2> tg = new List<Vector2>();
        int hitmonsterdist = -1;
        bool ok = GetEffectiveShootingRange( ref tg, ref range, ref hitmonsterdist, penetrateMonsters );
        if( ok == false ) return 0;
        return range;
    }
    public bool GetEffectiveShootingRange( ref List<Vector2> tg, ref int range, ref int hitmonsterdist, bool penetrateMonsters )
    {
        int _range = ( int ) TotalRange;
        if( Unit.UnitType == EUnitType.HERO )
            _range += Map.I.RM.RMD.BaseHeroRangedAttackRange;
        tg = new List<Vector2>();
        Vector2 from = Unit.Pos;
        Vector2 to = Unit.GetFront();
        int start = 1;
        int dir = ( int ) Unit.Dir;
        if( TargettingType == ETargettingType.SPELL )
        {
            _range = Sector.TSX;
            int dr = ( int ) Unit.Dir + ID;
            if( dr >= 8 ) dr -= 8;
            from = AttackOrigin;
            dir = ( int ) Util.GetTargetUnitDir( G.Hero.Control.OldPos, G.Hero.Pos );
            to = from + Manager.I.U.DirCord[ dir ];
            if( Map.IsWall( AttackOrigin ) == true ) return false;                                                          // this is new: now only attack origin must be clear for spell to be cast if you want to change it, look for old code
            if( Unit.Body.Sp[ ID ].Type == ESpiderBabyType.MUSHROOM_POTION )                                                // includes the first tile which is the tile the weapon or spell moves to
               start = 0;
        }
        else
        {
            if( Map.I.RM.RMD.LimitedArrowPerCube != -1 )
            if( Item.GetNum( ItemType.Res_Bow_Arrow ) < 1 ) return false;                                                     // Limited Arrows
        }

        AttackOrigin = from;
        AttackOriginalDirection = ( EDirection ) dir;

        if( Map.I.CheckArrowBlockFromTo( from, to, Unit ) ) return false;
        if( Unit.CheckLeverBlock( false, from, to, false ) ) return false;

        for( int i = 0; i < _range; i++ ) tg.Add( new Vector2( -1, -1 ) );
        for( int i = 0; i < _range; i++ )                                                                                      // Tests Targets
        {
            tg[ i ] = from + Manager.I.U.DirCord[ dir ] * ( i + start );
            bool ok = CanShoot( ref tg, i, ref hitmonsterdist, penetrateMonsters );
            if( ok == false ) break;
            range++;
        }
        return true;
    }

    public bool CanShoot( ref List<Vector2> tg, int i, ref int hitmonsterdist, bool penetrateMonsters )
    {
        if( Map.PtOnMap( Map.I.Tilemap, tg[ i ] ) == false ) return false;
        Unit un = Map.I.Unit[ ( int ) tg[ i ].x, ( int ) tg[ i ].y ];
        Unit ga = Map.I.Gaia[ ( int ) tg[ i ].x, ( int ) tg[ i ].y ];
        Unit ga2 = Map.I.Gaia2[ ( int ) tg[ i ].x, ( int ) tg[ i ].y ];
        if( Map.GFU( ETileType.MINE, tg[ i ] ) != null ) return false;
        
        if( ga != null )
        {
            if( ga.BlockMissile )
            {
                Unit mos = Map.I.GetUnit( ETileType.MOSQUITO, tg[ i ] );                               // mosquito as target over forest
                if( mos ) return false;
                else
                {
                    tg[ i ] = new Vector2( -1, -1 );
                    return false;
                }
            }

            if( ga.TileID == ETileType.PIT )                                                           // No shot over Pit 
                ShotOverPit = true;
        }   

        if( Unit.UnitType == EUnitType.HERO )
        {
            if( i == 0 && Map.I.GetUnit( ETileType.ARROW, G.Hero.Pos ) )                               // Hero over arrow: Block
            {
                tg[ i ] = new Vector2( -1, -1 );
                return false;
            }

            //if( ga2 != null ) IDEA: if a shot has been done passing through arrow spawn blocker     // Arrow block Hero Shot
            //    if( ga2.TileID == ETileType.ARROW )
            //    {
            //        tg[ i ] = new Vector2( -1, -1 );
            //        return false;
            //    }

            if( TargettingType == ETargettingType.SPELL )                                             // No spell cast over Hero
            {
                if( tg[ i ] == G.Hero.Pos ) return false;
                EDirection mov = Util.GetTargetUnitDir( G.Hero.Control.OldPos, G.Hero.Pos );
                Vector2 aux = G.Hero.Pos + ( Manager.I.U.DirCord[ ( int ) mov ] ) * 2;
                if( G.Hero.Body.Sp[ ID ].Type == ESpiderBabyType.SPEAR )                              // Spear cannot be thrown from front tile as well
                if( i == 0 && tg[ i ] == aux )
                    return false;   
            }
        }

        if( ga2 != null )
            if( ga2.BlockMissile ) return false;
        if( un && un.Body.IsDead )                                                                      // Avoid a second attack to a melee killed monster
        {
            tg[ i ] = new Vector2( -1, -1 );
            return false;
        }

        if( un != null )
        {
            if( un.TileID == ETileType.SCORPION )                                                        // No ranged attack to stepper after melee attack
            if( AttackedByHeroMelee ) return false;

            if( un.TileID == ETileType.SPIDER ||                                                         // no Shot over spider and hugger
                un.TileID == ETileType.HUGGER )
            {
                return false;
            }

            if( i == 1 )
            if( Map.I.IsHeroMeleeAvailable() )                                                            // Hero Sword Monster Block
            if( Map.Stepping() )
                {
                    tg[ i ] = new Vector2( -1, -1 );
                    return false;
                }

            if( ShotOverPit )
            if( un.Body.BigMonster )                                                                       // no big monster shot over pit
                {
                    tg[ i ] = new Vector2( -1, -1 );
                    return false;
                }

            if( un.ValidMonster ) hitmonsterdist = i;

            if( un.TileID != ETileType.SLIME && penetrateMonsters == false )                               // Monster block 
            if( un.BlockMissile )
                {
                    tg[ i ] = new Vector2( -1, -1 );
                    return false;
                }
        }
        if( Sector.GetPosSectorType( tg[ i ] ) == Sector.ESectorType.GATES ) return false;
        if( i >= 1 && Map.I.CheckArrowBlockFromTo( tg[ i - 1 ], tg[ i ], Unit, true ) )
        {
            tg[ i ] = new Vector2( -1, -1 );
            return false;
        }

        if ( TargettingType == ETargettingType.SPELL )
        for( int s = 0; s < G.Hero.Body.Sp.Count; s++ )                                                    // Counts duplicators in the way                             
             Spell.CheckDuplicator( tg[ i ], s );

        return true;
    }

    public bool CheckBarricadeBlockForRangedAttack( Vector2 tg )
    {
        if( Map.PtOnMap( Map.I.Tilemap, tg ) == false ) return true;
        Unit br = Map.I.GetUnit( ETileType.BARRICADE, tg );
        if( br == null ) return false;

        if( br.Body.TouchCount > 0 )
        {
            if( ( int ) G.Hero.Control.DragonBarricadeProtection >= br.Variation + 1 ) return true;
        }
        return false;
    }

    public void UpdateSpike( bool force, float tottime )
    {
        if( Unit.TileID != ETileType.SPIKES ) return;

        if( Unit.Control.IsDynamicObject() == false ) 
        if( SpeedTimeCounter >= tottime || force )                                                  // Important: Spike time is measured by seconds (not tottime)
        {
            FlipSpike();
        }
        if( SpeedTimeCounter >= Unit.Control.SpikeFreeStepTime )                                   // Spike damages hero
        if( Unit.Variation == 1 || Unit.Variation == 3 )
        if( G.Hero.Pos == Unit.Pos )
        if( Unit.Body.SpikeDamageApplied == false )
        if( G.Hero.Body.InvulnerabilityFactor <= 0 )
        {
            G.Hero.Body.ReceiveDamage( BaseDamage, EDamageType.BLEEDING, Unit, this );             // HP deduction
            if( G.Hero.Body.IsDead )
                Unit.Spr.transform.position = new Vector3(
                Unit.Spr.transform.position.x, Unit.Spr.transform.position.y, -2f );
            Unit.Body.SpikeDamageApplied = true;
            string msg = " -" + BaseDamage.ToString( "0.#" );                                       // Hit msg
            Message.CreateMessage( ETileType.NONE, ItemType.Res_HP,
            msg, G.Hero.Pos + new Vector2( 3, 1 ), Color.red );
            Body.CreateDeathFXAt( G.Hero.Pos, "Monster Hit" );                                     // Fx
        }       
    }
    public void FlipSpike( bool init = false )
    {
        if( Unit.Variation == 3 ) return;
        if( Unit.Variation == 0 )
        {
            Unit.Variation = 1;                                                       // Spike activated
        }
        else
        if( Unit.Variation == 1 )
        {
            Unit.Variation = 0;                                                      // Spike deactivated
        }

        if( init ) return;
        UpdateCustomSpeedCounterID();
        SpeedTimeCounter = 0;
        Unit.Body.SpikeDamageApplied = false;       
    }

    public void UpdateCustomSpeedCounterID()
    {
            if( AttackTotalTimeList != null && AttackTotalTimeList.Count > 0 )                                      // Custom speed list movement type: increment ID
            if( ++SpeedListID == AttackTotalTimeList.Count )
                SpeedListID = 0;
    }
}
