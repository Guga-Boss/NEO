using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using PathologicalGames;
using Sirenix.OdinInspector;
using DigitalRuby.LightningBolt;

#region Enums
public enum EUnitType
{
	NONE = 0, HERO, MONSTER, GAIA, GAIA2
}
public enum EDirection
{
    NONE = -1, N, NE, E, SE, S, SW, W, NW, CENTER
}
public enum EZone
{
    NONE = -1, 
    Back,     
    BackLeft, BackRight, 
    BackSideLeft, BackSideRight,        
    Right, Left,     
    FrontSideLeft, FrontSideRight,
    FrontLeft, FrontRight, 
    Front 
}
public enum EStatus
{
	NONE = -1, ACTIVE, DEAD
}
public enum EHeroID
{
    NONE = -3, ALL_HEROES, CURRENT_HERO, KADE, ADAM, HERO_2, HERO_3, HERO_4
}
public enum EProjectileType
{
    NONE = -1, BOOMERANG, FIREBALL, MISSILE
}
public enum ETileType
{
    NONE = -1, GRASS = 0, SAND = 1, FOREST = 2, WATER = 3, TRAP = 4, SNOW = 5, MUD = 6, ORB = 7,
    ARTIFACT, CLOSEDDOOR, OPENDOOR, ROOMDOOR, PIT, SCROLL, LEVEL_ENTRANCE = 14, STONEPATH,
    ARROW = 129, BOULDER = 132, ORIENTATION = 135, FREETRAP = 141,
    RED_PRESSUREPLATE = 142, GREEN_PRESSUREPLATE = 143, AREA_DRAG = 263, LAVA = 269,
    BLACKGATE = 270, GREENGATE = 271, THORNS = 398, SAVEGAME = 1290, CHECKPOINT, CAMERA = 1293,
    ORI3 = 1280, MUD_SPLASH = 399, SPLITTER,
    MAP_QUEST = 526, MAP_BONUS, INACTIVE_HERO = 219,
    FIRE = 908, RAFT, OPENBLACKGATE, OPENROOMDOOR, DOOR_CLOSER = 1024, 
    CAM_AREA = 1036, AREA_DRAG2 = 1037, AREA_DRAG3 = 1038, AREA_DRAG4 = 1039,
    ROACH = 640, SCARAB, MOSQUITO, GLUEY, SLIME, HUGGER, BARRICADE,
    DOOR_OPENER = 768, GRID = 781, DOME, FOG, DOOR_SWITCHER = 896, DOOR_KNOB = 1152,
    ITEM = 1165, BUILDING, ROAD,
    DRAGON1 = 1418, DRAGON2, QUEROQUERO, MOTHERJUMPER, JUMPER, FROG,
    HERB = 1802, BLOCK, BRAIN = 1546, SECRET, SCORPION, MOTHERWASP, WASP, SPIDER,
    MINE = 1674, FAN, FISH = 1676, 
    FISHING_POLE, BLOCKER, ALGAE, PLAGUE_MONSTER = 1804, ALTAR, TRAIL, VINES = 1807,
    IRON_BALL = 1929, BOUNCING_BALL, PROJECTILE, SPIKES, TOWER, BOOL_TOOGLE_1, BOOL_TOOGLE_2,  
    MOD1 = 2304, MOD2, MOD3, MOD4, MOD5, MOD6, MOD7, MOD8, MOD9, MOD10, MOD11, MOD12, MOD13, MOD14, MOD15, MOD16,
    MOD17 = 2432, MOD18, MOD19, MOD20, MOD21, MOD22, MOD23, MOD24, MOD25, MOD26, MOD27, MOD28, MOD29, MOD30, MOD31, MOD32,
    DECOR_TALL = 2560,
    DECOR_GAIA = 2559
}
public enum ELayerType
{
    NONE = -1, TERRAIN, GAIA, GAIA2, MONSTER, MODIFIER, GRID,
    DECOR, AREAS, DECOR2, RAFT
}
#endregion

[SelectionBase]
[System.Serializable]
public partial class Unit : MonoBehaviour
{
    #region Variables
    [TabGroup( "Other" )]
    [Button( "Update Shared Object", ButtonSizes.Large ), GUIColor( 1, 0.2f, 0 )]
    public void Button2()
    {
        Map.I = GameObject.Find( "----------------Map" ).GetComponent<Map>();
        int id = ( int ) TileID;
        // Garante que o array comporta o índice
        Map.I.SharedObjectsList = RedimensionarArraySeNecessario( Map.I.SharedObjectsList, id );
        Map.I.SharedObjectsList[ id ] = this;
        Debug.Log( "Tile: " + TileID + "  Size: " + Map.I.SharedObjectsList.Length + " Link:  " + Map.I.SharedObjectsList[ id ] );
    }

    [TabGroup( "Main" )]
    public bool Activated = true;
    [TabGroup( "Link" )]
    public string PrefabName;
    [TabGroup( "Link" )]
    public Attack MeleeAttack, RangedAttack, MagicAttack, BeeHiveAttack, InfectionAttack;
    [TabGroup( "Link" )]
    public List<Attack> Attacks;
    [TabGroup( "Link" )]
    public Body Body;
    [TabGroup( "Link" )]
    public Water Water;
    [TabGroup( "Link" )]
    public Mine Mine;
    [TabGroup( "Link" )]
    public Altar Altar;
    [TabGroup( "Link" )]
    public EnergyBar HealthBar;
    [TabGroup( "Link" )]
    public Controller Control;
    [TabGroup( "Link" )]
    public Building Building;
    [TabGroup( "Link" )]
    public tk2dSprite Spr;
    [TabGroup( "Link" )]
    public GameObject Graphic;
    [TabGroup( "Link" )]
    public tk2dTextMesh LevelTxt, RightText;
    [TabGroup( "Link" )]
    public PriceTag PriceTag;
    [TabGroup( "Link" )]
    public BoxCollider2D Collider;
    [TabGroup( "Link" )]
    public CircleCollider2D CircleCollider;
    [TabGroup( "Main" )]
    public EUnitType UnitType;
    [TabGroup( "Main" )]
    public ETileType TileID;
    [TabGroup( "Main" )]
    public bool CanBeRotated;
    [TabGroup( "Main" )]
    public EDirection Dir;
    [TabGroup( "Main" )]
    public Vector2 Pos, IniPos;
    [TabGroup( "Main" )]
    public bool BlockMovement, CanBeWalkedIfSuspended, BlockMissile, BlockSight, ValidMonster, Slippery, UseTransTile, SaveData, SharedObject;
    [TabGroup( "Main" )]
    public int Variation;
    [TabGroup( "Main" )]
    public Mod Md;                   // Link to the MOD applied
    [TabGroup( "Main" )]
    public int ModID = -1;                // Link to the MOD applied
    [TabGroup( "Link" )]  
	public List<MyTrigger> TriggerList;
    [TabGroup( "Other" )]
    public bool SpawnedCollider = false;

    #endregion
    public void Copy( Unit un, bool copySprite, bool copyhp, bool restart )
	{
        IniPos             = un.IniPos;
        Variation          = un.Variation;
        Activated          = un.Activated;
        BlockMovement      = un.BlockMovement;
        BlockMissile       = un.BlockMissile;
        BlockSight         = un.BlockSight;
        ValidMonster       = un.ValidMonster;
        Slippery           = un.Slippery; 
        UseTransTile       = un.UseTransTile;
        SaveData           = un.SaveData;
        CanBeRotated       = un.CanBeRotated;
        PrefabName         = un.PrefabName;
        SpawnedCollider    = un.SpawnedCollider;
        Md                 = un.Md;
        ModID              = un.ModID;

        if( Water ) Water.Copy( un.Water );
        if( Mine ) Mine.Copy( un.Mine );
        if( Altar ) Altar.Copy( un.Altar ); 

        if( Body )
        {
            if( Body.EffectList != null && Body.EffectList.Length > 0 )
            for( int i = 0; i < Body.EffectList.Length; i++ )
                Body.EffectList[ i ].SetActive( false );

                Body.Stars = un.Body.Stars;
            Body.TotHp = un.Body.TotHp;
            Body.BaseTotHP = un.Body.BaseTotHP;
            if( copyhp ) Body.Hp = un.Body.Hp;
            Body.Lives = un.Body.Lives;
            Body.TotalMagicShield      = un.Body.TotalMagicShield;
            Body.TotalMissileShield    = un.Body.TotalMissileShield;
            Body.TotalMeleeShield      = un.Body.TotalMeleeShield;
            Body.BaseMagicShield       = un.Body.BaseMagicShield;
            Body.BaseMissileShield     = un.Body.BaseMissileShield;
            Body.BaseMeleeShield       = un.Body.BaseMeleeShield;
            Body.BonusMagicShield      = un.Body.BonusMagicShield;
            Body.BonusMissileShield    = un.Body.BonusMissileShield;
            Body.BonusMeleeShield      = un.Body.BonusMeleeShield;
            Body.MagicShieldPerLevel   = un.Body.MagicShieldPerLevel;
            Body.MissileShieldPerLevel = un.Body.MissileShieldPerLevel;
            Body.MeleeShieldPerLevel   = un.Body.MeleeShieldPerLevel;
            Body.MagicShieldPerStar    = un.Body.MagicShieldPerStar;
            Body.MissileShieldPerStar  = un.Body.MissileShieldPerStar;
            Body.MeleeShieldPerStar    = un.Body.MeleeShieldPerStar;
            Body.GraphicsInitialized   = un.Body.GraphicsInitialized;
            Body.isWorking             = un.Body.isWorking;
            Body.HealthStatus          = un.Body.HealthStatus;
            Body.Level                 = un.Body.Level;
            Body.BaseLevel             = un.Body.BaseLevel;
            Body.HpPerLevel            = un.Body.HpPerLevel;
            Body.HpPerStar             = un.Body.HpPerStar;
            Body.DamageTaken           = un.Body.DamageTaken;
            Body.InvulnerabilityFactor = un.Body.InvulnerabilityFactor;
			Body.DestroyBarricadeLevel = un.Body.DestroyBarricadeLevel;
            Body.IsDead                = un.Body.IsDead;
			Body.NumMushroom           = un.Body.NumMushroom;
			Body.HpPerMushroom         = un.Body.HpPerMushroom;
			Body.PressureAttackBonus   = un.Body.PressureAttackBonus;
			Body.MeleeAttackLevel      = un.Body.MeleeAttackLevel;
			Body.RangedAttackLevel     = un.Body.RangedAttackLevel;
			Body.DexterityLevel        = un.Body.DexterityLevel;
			Body.AgilityLevel          = un.Body.AgilityLevel;
			Body.OrbStrikerLevel       = un.Body.OrbStrikerLevel;
			Body.PerformPreMoveAttack  = un.Body.PerformPreMoveAttack;
			Body.PerformPostMoveAttack = un.Body.PerformPostMoveAttack;
			Body.CooperationLevel      = un.Body.CooperationLevel;
			Body.DamageSurplusLevel    = un.Body.DamageSurplusLevel;
			Body.MeleeShieldLevel      = un.Body.MeleeShieldLevel;
			Body.MissileShieldLevel    = un.Body.MissileShieldLevel;
			Body.MagicShieldLevel      = un.Body.MagicShieldLevel;
            Body.AmbusherLevel         = un.Body.AmbusherLevel;
            Body.MemoryLevel           = un.Body.MemoryLevel;
            Body.ToolBoxLevel          = un.Body.ToolBoxLevel;
            Body.FireMasterLevel       = un.Body.FireMasterLevel;
            Body.BerserkLevel          = un.Body.BerserkLevel;
            Body.BeeHiveThrowerLevel   = un.Body.BeeHiveThrowerLevel;
            Body.NumFreeAttacks        = un.Body.NumFreeAttacks;
            Body.FireIsOn              = un.Body.FireIsOn;
            Body.OriginalFireIsOn      = un.Body.OriginalFireIsOn;
            Body.AvailableFireHits     = un.Body.AvailableFireHits;
            Body.PsychicLevel          = un.Body.PsychicLevel;
            Body.WallDestroyerLevel    = un.Body.WallDestroyerLevel;
            Body.IsLoot                = un.Body.IsLoot;
            Body.IsMarked              = un.Body.IsMarked;
            Body.PoisonBite            = un.Body.PoisonBite;
            Body.LooterLevel           = un.Body.LooterLevel;
            Body.ProspectorLevel       = un.Body.ProspectorLevel;
            Body.ProtectionLevel       = un.Body.ProtectionLevel;
            Body.BaseMonsterLootTurns  = un.Body.BaseMonsterLootTurns;
            Body.StackAmount           = un.Body.StackAmount;
            Body.AuxStackAmount        = un.Body.AuxStackAmount;
            Body.ExclusiveToAdventure  = un.Body.ExclusiveToAdventure;
            Body.HPLostInTheTurn       = un.Body.HPLostInTheTurn;
            Body.BigMonster            = un.Body.BigMonster;
            Body.EnemyAttackBonus      = un.Body.EnemyAttackBonus;
            Body.MakePeaceful          = un.Body.MakePeaceful;
            Body.MakeImovable          = un.Body.MakeImovable;
            Body.FrontalAttacks        = un.Body.FrontalAttacks;
            Body.FrontSideAttacks      = un.Body.FrontSideAttacks;
            Body.SideAttacks           = un.Body.SideAttacks;
            Body.BackSideAttacks       = un.Body.BackSideAttacks;
            Body.BackAttacks           = un.Body.BackAttacks;
            Body.ArrowHitCount         = un.Body.ArrowHitCount;
            Body.FreshAttackLevel      = un.Body.FreshAttackLevel;
            Body.RiskyAttackLevel      = un.Body.RiskyAttackLevel;
            Body.MortalJumpLevel       = un.Body.MortalJumpLevel;
            Body.OpenFieldAtttackLevel = un.Body.OpenFieldAtttackLevel;
            Body.ThreatLevel           = un.Body.ThreatLevel;
            Body.BaseThreatDuration       = un.Body.BaseThreatDuration;
            Body.BaseFreeExitHPLimit   = un.Body.BaseFreeExitHPLimit;
            Body.FirePowerLevel        = un.Body.FirePowerLevel;
            Body.FireSpreadLevel       = un.Body.FireSpreadLevel;
            Body.FireWoodNeeded        = un.Body.FireWoodNeeded;
            Body.VineFireSpreadID      = un.Body.VineFireSpreadID;
            Body.OutsideFireWoodAllowed    = un.Body.OutsideFireWoodAllowed;
            Body.WoodAddedTurnCount        = un.Body.WoodAddedTurnCount;
            Body.TouchCount                = un.Body.TouchCount;
            Body.OriginalTouchCount        = un.Body.OriginalTouchCount;
            Body.FreePlatformExit          = un.Body.FreePlatformExit;
            Body.IsEternalToken            = un.Body.IsEternalToken;
            Body.BarricadeForRune          = un.Body.BarricadeForRune;
            Body.ScaryLevel                = un.Body.ScaryLevel;
            Body.HeroNeighborTouchAdder    = un.Body.HeroNeighborTouchAdder;
            Body.HitsTaken                 = un.Body.HitsTaken;
            Body.OriginalHitsTaken         = un.Body.OriginalHitsTaken;
            Body.FireStarBonus             = un.Body.FireStarBonus;
            Body.CollectorLevel            = un.Body.CollectorLevel;
            Body.HasSeenTheHero            = un.Body.HasSeenTheHero;
            Body.ResourcePersistance       = un.Body.ResourcePersistance;
            Body.EconomyLevel              = un.Body.EconomyLevel;
            Body.RealtimeMeleeAttSpeed     = un.Body.RealtimeMeleeAttSpeed;
            Body.RealtimeRangedAttSpeed    = un.Body.RealtimeRangedAttSpeed;
            Body.IsTiny                    = un.Body.IsTiny;
            Body.FirstAttackTime           = un.Body.FirstAttackTime;
            Body.HitPointsRatio            = un.Body.HitPointsRatio;
            Body.IsBoss                    = un.Body.IsBoss;
            Body.ResourceOperation         = un.Body.ResourceOperation;
            Body.ResourcePersistTotalSteps      = un.Body.ResourcePersistTotalSteps;
            Body.ResourcePersistStepCount  = un.Body.ResourcePersistStepCount;
            Body.MiningLevel               = un.Body.MiningLevel;
            Body.BaseMiningChance          = un.Body.BaseMiningChance;
            Body.ExtraMiningChance         = un.Body.ExtraMiningChance;
            Body.MiningPrize               = un.Body.MiningPrize;
            Body.FrameDamageTaken          = un.Body.FrameDamageTaken;
            Body.MiningBonusAmount         = un.Body.MiningBonusAmount;
            Body.MineType                  = un.Body.MineType;
            Body.UPMineType                = un.Body.UPMineType;
            Body.DeathCountFactor          = un.Body.DeathCountFactor;
            Body.FishType                  = un.Body.FishType;
            Body.Animator                  = un.Body.Animator;
            Body.IsFish                    = un.Body.IsFish;
            Body.FishingLevel              = un.Body.FishingLevel;
            Body.Fishing_1                 = un.Body.Fishing_1;
            Body.Fishing_2                 = un.Body.Fishing_2;
            Body.Fishing_3                 = un.Body.Fishing_3;
            Body.Fishing_4                 = un.Body.Fishing_4;
            //Body.Sprite2                   = un.Body.Sprite2;
            Body.HerbColor                 = un.Body.HerbColor;
            Body.HerbType                  = un.Body.HerbType;
            Body.OriginalHerbType          = un.Body.OriginalHerbType;
            Body.HerbBonusAmount           = un.Body.HerbBonusAmount;
            Body.RandomizableHerb          = un.Body.RandomizableHerb;
            Body.HerbKillTimer             = un.Body.HerbKillTimer;
            Body.MiniDomeTimerCounter      = un.Body.MiniDomeTimerCounter;
            Body.ShackleDistance           = un.Body.ShackleDistance;
            Body.VsRoachAttDecreasePerBaby = un.Body.VsRoachAttDecreasePerBaby;
            Body.VsHeroRoachAttackIncreasePerBaby = un.Body.VsHeroRoachAttackIncreasePerBaby;
            Body.SpawnBlocker              = un.Body.SpawnBlocker;
            Body.OptionalMonster           = un.Body.OptionalMonster;
            Body.SpriteColor               = un.Body.SpriteColor;
            Body.OrbFogStopEnabled         = un.Body.OrbFogStopEnabled;
            Body.AutoRotatingMineType      = un.Body.AutoRotatingMineType;
            Body.LeverMine                 = un.Body.LeverMine;
            Body.AnimationFrame            = un.Body.AnimationFrame;
            Body.AnimationSignal           = un.Body.AnimationSignal;
            Body.AnimationTimeCounter      = un.Body.AnimationTimeCounter;
            Body.MineLeverDir              = un.Body.MineLeverDir;
            Body.ChiselSlideCount          = un.Body.ChiselSlideCount;
            Body.MineGreenCrystalAmount    = un.Body.MineGreenCrystalAmount;
            Body.MineRedCrystalAmount      = un.Body.MineRedCrystalAmount;
            Body.ClockwiseChainPush        = un.Body.ClockwiseChainPush;
            Body.MinePushSteps             = un.Body.MinePushSteps;
            Body.ShieldedWaspChance        = un.Body.ShieldedWaspChance;
            Body.MaxShieldedWasps          = un.Body.MaxShieldedWasps;
            Body.ShieldedWasp              = un.Body.ShieldedWasp;
            Body.EnragedWasp               = un.Body.EnragedWasp;
            Body.CocoonWasp                = un.Body.CocoonWasp;
            Body.MineLeverActivePuller     = un.Body.MineLeverActivePuller;
            Body.MiniDomeTotalTime         = un.Body.MiniDomeTotalTime;
            Body.OriginalHookType          = un.Body.OriginalHookType;
            Body.AddedLivesCount           = un.Body.AddedLivesCount;
            Body.RaftResource              = un.Body.RaftResource;
            Body.ResourceWasteTimeCounter  = un.Body.ResourceWasteTimeCounter;
            Body.ResourceWasteTotalTime    = un.Body.ResourceWasteTotalTime;
            Body.ResourceSlot              = un.Body.ResourceSlot;
            Body.MudPushable               = un.Body.MudPushable;
            Body.BumpTimer                 = un.Body.BumpTimer;
            Body.Crippled                  = un.Body.Crippled;
            Body.KillOnFlightEnd           = un.Body.KillOnFlightEnd;
            Body.ResTriggerType            = un.Body.ResTriggerType;
            Body.UPMineDir                 = un.Body.UPMineDir;
            Body.SpikeDamageApplied        = un.Body.SpikeDamageApplied;
            Body.SpikedBoulder             = un.Body.SpikedBoulder;
            Body.HookIsStuck               = un.Body.HookIsStuck;
            Body.HookID                    = un.Body.HookID;
            Body.CreatedVine               = un.Body.CreatedVine;
            Body.FireSourcePos             = un.Body.FireSourcePos;
            Body.VineBurnTime              = un.Body.VineBurnTime;
            Body.OverFireTimeCount         = un.Body.OverFireTimeCount;
            Body.ChestLevel                = un.Body.ChestLevel;
            Body.IsInfected                = un.Body.IsInfected;
            Body.InfectedRadius            = un.Body.InfectedRadius;
            Body.ImmunityShieldTime        = un.Body.ImmunityShieldTime;
            Body.TweenTime                 = un.Body.TweenTime;
            Body.PoleBonusAvailableAngle   = un.Body.PoleBonusAvailableAngle;
            Body.RotationSpeed             = un.Body.RotationSpeed;
            Body.KillTimer                 = un.Body.KillTimer;
            Body.TrailCoil                 = un.Body.TrailCoil;
            Body.TrailRotator              = un.Body.TrailRotator;
            Body.QQMissHeroDamage          = un.Body.QQMissHeroDamage;
            Body.QQMinDamage               = un.Body.QQMinDamage;
            Body.QQMaxDamage               = un.Body.QQMaxDamage;

            Body.OutAreaBurningBarricadeDestroyBonus = un.Body.OutAreaBurningBarricadeDestroyBonus;
            Body.HeroAttackPositionHistory = new List<Vector2>();
            Body.HeroAttackPositionHistory.AddRange( un.Body.HeroAttackPositionHistory );

            Body.HeroAttackDirectionHistory = new List<EDirection>();
            Body.HeroAttackDirectionHistory.AddRange( un.Body.HeroAttackDirectionHistory );

            if( TileID == ETileType.ITEM )
            {
                Body.BonusItemList = new List<ItemType>();
                Body.BonusItemList.AddRange( un.Body.BonusItemList );
                Body.BonusItemAmountList = new List<float>();
                Body.BonusItemAmountList.AddRange( un.Body.BonusItemAmountList );
                Body.BonusItemChanceList = new List<float>();
                Body.BonusItemChanceList.AddRange( un.Body.BonusItemChanceList );
                Body.BonusItemChestLevelList = new List<int>();
                Body.BonusItemChestLevelList.AddRange( un.Body.BonusItemChestLevelList );
            }
            Body.ChildList = new List<Unit>();
            Body.ChildList.AddRange( un.Body.ChildList );
            Body.MineSideHitCount = new List<int>();
            Body.MineSideHitCount.AddRange( un.Body.MineSideHitCount );
            Body.HasBaby = new List<bool>();
            Body.HasBaby.AddRange( un.Body.HasBaby );
            Body.BabyVariation = new List<int>();
            Body.BabyVariation.AddRange( un.Body.BabyVariation );
            Body.CreatedVineList = new List<int>();
            Body.CreatedVineList.AddRange( un.Body.CreatedVineList );

            if( restart )
            {
                Body.OriginalIsMarked = un.Body.OriginalIsMarked;
                Body.TotHp = un.Body.TotHp;
                Body.Hp = un.Body.Hp;
                Body.TurnsToDie = un.Body.TurnsToDie;
                Body.OriginalTurnsToDie = un.Body.OriginalTurnsToDie;
                Body.OriginalThreatLevel = un.Body.OriginalThreatLevel;
                Body.OriginalArrowHitCount = un.Body.OriginalArrowHitCount;
                Body.InflictedDamageRate = un.Body.InflictedDamageRate;
                Body.OriginalFrontalAttacks = un.Body.OriginalFrontalAttacks;
                Body.OriginalFrontSideAttacks = un.Body.OriginalFrontSideAttacks;
                Body.OriginalSideAttacks = un.Body.OriginalSideAttacks;
                Body.OriginalBackSideAttacks = un.Body.OriginalBackSideAttacks;
                Body.OriginalBackAttacks = un.Body.OriginalBackAttacks;
                Body.OriginalRiskyAttacks = un.Body.OriginalRiskyAttacks;
                Body.OriginalOpenFieldAttacks = un.Body.OpenFieldAttacks;
                Body.OriginalMortalJumpBonusAttack = un.Body.MortalJumpBonusAttack;
                Body.OriginalVariation = un.Body.OriginalVariation;
                Body.OriginalHasSeenTheHero = un.Body.OriginalHasSeenTheHero;
            }
        }


        if( restart )
            Dir = un.Dir;
        Activated          = un.Activated;

        if( Control )
        {
            Control.IsBeingPushedAgainstObstacle  = un.Control.IsBeingPushedAgainstObstacle;
            Control.MoveOrderID                   = un.Control.MoveOrderID;
            Control.PushStrenght                  = un.Control.PushStrenght;
            Control.CanPushDiagonally             = un.Control.CanPushDiagonally;
            Control.CanPushOrthogonally           = un.Control.CanPushOrthogonally;
            Control.IsSuspended                   = un.Control.IsSuspended;
            Control.RamStraightMoveCount          = un.Control.RamStraightMoveCount;
            Control.CanPushObjects                = un.Control.CanPushObjects;
			Control.MonsterPushLevel              = un.Control.MonsterPushLevel;
            Control.CanBePushed                   = un.Control.CanBePushed;
            Control.BlockedByArrow                = un.Control.BlockedByArrow;
            Control.ArrowRushEnabled              = un.Control.ArrowRushEnabled;
            Control.Sleeping                      = un.Control.Sleeping;
            Control.Resting                       = un.Control.Resting;
            Control.DeniedTerrain                 = un.Control.DeniedTerrain;
            Control.AllowedTerrain                = un.Control.AllowedTerrain;
            Control.LastMoveType                  = un.Control.LastMoveType;
            Control.NeedLineOfSightToMove         = un.Control.NeedLineOfSightToMove;
            Control.HasLineOfSight                = un.Control.HasLineOfSight;
            Control.FirstMoveDone                 = un.Control.FirstMoveDone;
            Control.LastAction                    = un.Control.LastAction;
            Control.LastMoveAction                = un.Control.LastMoveAction;
            Control.LockMeleeTarget               = un.Control.LockMeleeTarget;
			Control.MonsterCorneringLevel         = un.Control.MonsterCorneringLevel;
			Control.MovementLevel                 = un.Control.MovementLevel;
			Control.ArrowWalkingLevel             = un.Control.ArrowWalkingLevel;
            Control.ArrowInLevel                  = un.Control.ArrowInLevel;
            Control.ArrowOutLevel                 = un.Control.ArrowOutLevel;
			Control.ScoutLevel                    = un.Control.ScoutLevel;
            Control.PlatformWalkingLevel          = un.Control.PlatformWalkingLevel;
            Control.PlatformWalkingCounter        = un.Control.PlatformWalkingCounter;
            Control.SprinterLevel                 = un.Control.SprinterLevel;
            Control.SneakingLevel                 = un.Control.SneakingLevel;
            Control.IsBeingPushedByHero           = un.Control.IsBeingPushedByHero;
            Control.BarricadeFighterLevel         = un.Control.BarricadeFighterLevel;
            Control.BoulderDirChanged             = un.Control.BoulderDirChanged;
            Control.EvasionLevel                  = un.Control.EvasionLevel;
            Control.EvasionPoints                 = un.Control.EvasionPoints;
            Control.FrontalFlyingTargetDist       = un.Control.FrontalFlyingTargetDist;
            Control.PerfectionistLevel            = un.Control.PerfectionistLevel;
            Control.FlightPhaseLoopCount          = un.Control.FlightPhaseLoopCount;
            Control.ScavengerLevel                = un.Control.ScavengerLevel;
            Control.ArrowFighterLevel             = un.Control.ArrowFighterLevel;
            Control.OverBarricadeScoutLevel       = un.Control.OverBarricadeScoutLevel;
            Control.ShowResourceChance            = un.Control.ShowResourceChance;
            Control.ShowResourceNeighborsChance   = un.Control.ShowResourceNeighborsChance;
            Control.RealtimeSpeedFactor           = un.Control.RealtimeSpeedFactor;
            Control.FlyingTarget                  = un.Control.FlyingTarget;
            Control.IsFlyingUnit                  = un.Control.IsFlyingUnit;
            Control.FlightSpeedFactor             = un.Control.FlightSpeedFactor;
            Control.FlightSpeedFactorTimer        = un.Control.FlightSpeedFactorTimer;
            Control.FlightTime                    = un.Control.FlightTime;
            Control.FlightTargetTime              = un.Control.FlightTargetTime;
            Control.AttackSpeedLerpTimer          = un.Control.AttackSpeedLerpTimer;
            Control.SlayerLevel                   = un.Control.SlayerLevel;
            Control.FlyingTargetting              = un.Control.FlyingTargetting;
            Control.FlyingOrigin                  = un.Control.FlyingOrigin;
            Control.SlayerAngle                   = un.Control.SlayerAngle;
            Control.SlayerMaxHP                   = un.Control.SlayerMaxHP;
            Control.DragonDisguiseLevel           = un.Control.DragonDisguiseLevel;
            Control.DragonBonusDropLevel          = un.Control.DragonBonusDropLevel;
            Control.DragonBarricadeProtection     = un.Control.DragonBarricadeProtection;
            Control.Nest                          = un.Control.Nest;
            Control.SpawnTimer                    = un.Control.SpawnTimer;
            Control.SpawnCount                    = un.Control.SpawnCount; 
            Control.ShieldedWaspCount             = un.Control.ShieldedWaspCount;
            Control.CocoonWaspCount               = un.Control.CocoonWaspCount;
            Control.EnragedWaspCount              = un.Control.EnragedWaspCount;
            Control.BeingMudPushed                = un.Control.BeingMudPushed;
            Control.TotSpawnTimer                 = un.Control.TotSpawnTimer;
            Control.Initialized                   = un.Control.Initialized;
            Control.JumpTarget                    = un.Control.JumpTarget;            
            Control.SpeedTimeCounter              = un.Control.SpeedTimeCounter;
            Control.UnitProcessed             = un.Control.UnitProcessed;
            Control.RealtimeMoveTried             = un.Control.RealtimeMoveTried;
            Control.MireLevel                     = un.Control.MireLevel;
            Control.RestingLevel                  = un.Control.RestingLevel;
            Control.BaseRestDistance              = un.Control.BaseRestDistance;
            Control.Mother                        = un.Control.Mother;
            Control.FlightJumpPhase               = un.Control.FlightJumpPhase;
            Control.FlightPhaseTimer              = un.Control.FlightPhaseTimer;
            Control.SmoothMovementFactor          = un.Control.SmoothMovementFactor;
            Control.TurnTime                      = un.Control.TurnTime;
            Control.RealtimeSpeed                 = un.Control.RealtimeSpeed;
            Control.PlatformSteps                 = un.Control.PlatformSteps;
            Control.LastFlownOverTile             = un.Control.LastFlownOverTile;
            Control.FlightDir                     = un.Control.FlightDir;
            Control.FlightSpeed                   = un.Control.FlightSpeed;
            Control.TileChanged                   = un.Control.TileChanged;
            Control.BaseWaspTotSpawnTimer         = un.Control.BaseWaspTotSpawnTimer;
            Control.FireMarkAvailable             = un.Control.FireMarkAvailable;
            Control.ExtraEnragedSpawns            = un.Control.ExtraEnragedSpawns;
            Control.WaspSpawnInflationPerTile     = un.Control.WaspSpawnInflationPerTile;
            Control.MaxSpawnCount                 = un.Control.MaxSpawnCount;
            Control.ExtraWaspSpawnTimerPerTile    = un.Control.ExtraWaspSpawnTimerPerTile;
            Control.WaspDamage                    = un.Control.WaspDamage;
            Control.MotherWaspRadius              = un.Control.MotherWaspRadius;
            Control.SpikeFreeStepTime             = un.Control.SpikeFreeStepTime;
            Control.WaspInflationSum              = un.Control.WaspInflationSum;
            Control.FlightStepPhaseTimer          = un.Control.FlightStepPhaseTimer;
            Control.ExtraWaspSpawnInflationPerSec = un.Control.ExtraWaspSpawnInflationPerSec;
            Control.WaitTime                      = un.Control.WaitTime;
            Control.FrontalRebound                = un.Control.FrontalRebound;
            Control.WakeUpGroup                   = un.Control.WakeUpGroup;
            Control.SkipMovement                  = un.Control.SkipMovement;
            Control.TickBasedMovement             = un.Control.TickBasedMovement;
            Control.BoulderPunchPower             = un.Control.BoulderPunchPower;
            Control.DynamicMovementCurrentStep    = un.Control.DynamicMovementCurrentStep;
            Control.DynamicOrientationCurrentStep = un.Control.DynamicOrientationCurrentStep;
            Control.DynamicMaxSteps               = un.Control.DynamicMaxSteps;
            Control.DynamicStepCount              = un.Control.DynamicStepCount;
            Control.SideFlightSpeed               = un.Control.SideFlightSpeed;
            Control.SideFlightZigZagDistance      = un.Control.SideFlightZigZagDistance;
            Control.SideFlightZigZagTimeFactor    = un.Control.SideFlightZigZagTimeFactor;
            Control.TotalSideFlightTime           = un.Control.TotalSideFlightTime;
            Control.TotalSideFlightSpeed          = un.Control.TotalSideFlightSpeed;
            Control.SideFlightTime                = un.Control.SideFlightTime;
            Control.MovementSorting               = un.Control.MovementSorting;
            Control.OrientationSorting            = un.Control.OrientationSorting;
            Control.InitialDirection              = un.Control.InitialDirection;
            Control.WaspClimbBarricadeNumber      = un.Control.WaspClimbBarricadeNumber;
            Control.ForcedFrontalMovementFactor   = un.Control.ForcedFrontalMovementFactor;
            Control.RaftMoveDir                   = un.Control.RaftMoveDir;
            Control.RaftGroupID                   = un.Control.RaftGroupID;
            Control.BrokenRaft                    = un.Control.BrokenRaft;
            Control.SwimmingDepht                 = un.Control.SwimmingDepht;
            Control.ActionTimeCounter             = un.Control.ActionTimeCounter;
            Control.WaitTimeCounter               = un.Control.WaitTimeCounter;
            Control.WakeupTimeCounter             = un.Control.WakeupTimeCounter;
            Control.WakeupTotalTime               = un.Control.WakeupTotalTime;
            Control.FishActionWaiting             = un.Control.FishActionWaiting;
            Control.FAOverFishTimeCount           = un.Control.FAOverFishTimeCount;
            Control.FishRedLimit                  = un.Control.FishRedLimit;
            Control.FishGreenLimit                = un.Control.FishGreenLimit;
            Control.FishingLine                   = un.Control.FishingLine;
            Control.FishCaughtTimeCount           = un.Control.FishCaughtTimeCount;
            Control.RespawnTimeCount              = un.Control.RespawnTimeCount;
            Control.BaseTotalRespawnTime          = un.Control.BaseTotalRespawnTime;
            Control.BaseTotalRespawnAmount        = un.Control.BaseTotalRespawnAmount;
            Control.RespawnCount                  = un.Control.RespawnCount;
            Control.RandomizePositionOnRespawn    = un.Control.RandomizePositionOnRespawn;
            Control.RandomizePositionOnPoleStep   = un.Control.RandomizePositionOnPoleStep;
            Control.PoleExtraLevel                = un.Control.PoleExtraLevel;
            Control.FishSwimType                  = un.Control.FishSwimType;
            Control.FAOverFishTimeCountPerFA      = un.Control.FAOverFishTimeCountPerFA;
            Control.FishingPowerFactor            = un.Control.FishingPowerFactor;
            Control.FishingBonusAmount            = un.Control.FishingBonusAmount;
            Control.FlyingRotationSpeed           = un.Control.FlyingRotationSpeed;
            Control.FlyingMaxSpeed                = un.Control.FlyingMaxSpeed;
            Control.FlyingAcceleration            = un.Control.FlyingAcceleration;
            Control.FlyingSpeed                   = un.Control.FlyingSpeed;
            Control.WhiteGateGroup                = un.Control.WhiteGateGroup;
            Control.TargettingRadiusFactor        = un.Control.TargettingRadiusFactor;
            Control.HeroNeighborTimeCount         = un.Control.HeroNeighborTimeCount;
            Control.FixedRestingDirection         = un.Control.FixedRestingDirection;
            Control.RestingDirectionIsSameAsHero  = un.Control.RestingDirectionIsSameAsHero;
            Control.FrogHeroBlockTimeCounter      = un.Control.FrogHeroBlockTimeCounter;
            Control.OverFishTimeCount             = un.Control.OverFishTimeCount;
            Control.MaxFishSpeed                  = un.Control.MaxFishSpeed;
            Control.MinFishSpeed                  = un.Control.MinFishSpeed;
            Control.OverFishSecondsNeededPerUnit  = un.Control.OverFishSecondsNeededPerUnit;
            Control.LastPos                       = un.Control.LastPos;
            Control.SideFlightZigZagPhase         = un.Control.SideFlightZigZagPhase;
            Control.ForcedFrontalMovementDir      = un.Control.ForcedFrontalMovementDir;
            Control.FlightActionTime              = un.Control.FlightActionTime;
            Control.FireMarkedWaspFactor          = un.Control.FireMarkedWaspFactor;
            Control.PhaseWonFreePhases            = un.Control.PhaseWonFreePhases;
            Control.FlightResourcePicking         = un.Control.FlightResourcePicking;
            Control.FlightOrbStrike               = un.Control.FlightOrbStrike;
            Control.FlightBarricadeDestroy        = un.Control.FlightBarricadeDestroy;
            Control.FlightWallClash               = un.Control.FlightWallClash;
            Control.FrogBlockedByHero             = un.Control.FrogBlockedByHero;
            Control.HeroBlockFrogTotalTime        = un.Control.HeroBlockFrogTotalTime;
            Control.MergingID                     = un.Control.MergingID;
            Control.WaitAfterObstacleCollision    = un.Control.WaitAfterObstacleCollision;
            Control.CurFA                         = un.Control.CurFA;
            Control.BoltEffect                    = un.Control.BoltEffect;
            Control.TimeforNextEffect             = un.Control.TimeforNextEffect;
            Control.FlyingAngle                   = un.Control.FlyingAngle;
            Control.WaspLongFlight                = un.Control.WaspLongFlight;
            Control.Floor                         = un.Control.Floor;
            Control.OldFloor                      = un.Control.OldFloor;
            Control.OriginalZCord                 = un.Control.OriginalZCord;
            Control.ConsecutiveStillMove          = un.Control.ConsecutiveStillMove;
            Control.ProjectileType                = un.Control.ProjectileType;
            Control.SpeedListID                   = un.Control.SpeedListID;
            Control.EggsDestroyed                 = un.Control.EggsDestroyed;
            Control.SpiderAttackBlockPhase        = un.Control.SpiderAttackBlockPhase;
            Control.HeroPushedMonsterBarricadeDestroyCount = un.Control.HeroPushedMonsterBarricadeDestroyCount;
            Control.StairStepDir                  = un.Control.StairStepDir;

            if( restart )
            {
                Control.OriginalSleeping          = un.Control.OriginalSleeping;
                Control.OriginalResting           = un.Control.OriginalResting;
            }

            Control.PositionHistory.Clear();
            Control.WaspOccupiedTiles = new List<Vector2>();

            Control.TickMoveList = new List<int>();
            Control.TickMoveList.AddRange( un.Control.TickMoveList );

            Control.DynamicObjectMoveList = new List<EActionType>();
            Control.DynamicObjectMoveList.AddRange( un.Control.DynamicObjectMoveList );

            Control.DynamicObjectOrientationList = new List<EOrientation>();
            Control.DynamicObjectOrientationList.AddRange( un.Control.DynamicObjectOrientationList );

            Control.DynamicObjectJumpList = new List<Vector2>();
            Control.DynamicObjectJumpList.AddRange( un.Control.DynamicObjectJumpList );

            Control.DynamicObjectDirectionList = new List<EDirection>();
            Control.DynamicObjectDirectionList.AddRange( un.Control.DynamicObjectDirectionList );

            Control.RandomMoveTurnList = new List<int>();
            Control.RandomMoveTurnList.AddRange( un.Control.RandomMoveTurnList );

            Control.ForceVineLink = new List<int>();
            Control.ForceVineLink.AddRange( un.Control.ForceVineLink );

            Control.RedRoachBabyList = new List<EDirection>();
            Control.RedRoachBabyList.AddRange( un.Control.RedRoachBabyList );   
            Control.RandomDirTurnList = new List<int>();
            Control.RandomDirTurnList.AddRange( un.Control.RandomDirTurnList );
            Control.FlyingActionTarget = new List<Vector2>();
            Control.FlyingActionTarget.AddRange( un.Control.FlyingActionTarget );
            Control.FlyingActionVal = new List<float>();
            Control.FlyingActionVal.AddRange( un.Control.FlyingActionVal );
            Control.FlyingActionLoopCount = new List<int>();
            Control.FlyingActionLoopCount.AddRange( un.Control.FlyingActionLoopCount );
            Control.FishAction = new List<FishActionStruct>();
            Control.FishAction.AddRange( un.Control.FishAction );
            Control.FlightActionTotalTime = new List<float>();
            Control.FlightActionTotalTime.AddRange( un.Control.FlightActionTotalTime );            
        }

        if( Building )
        {
            Building.Copy( un.Building );
        }

        if( copySprite )
        if( Spr && un.Spr ) Spr.spriteId = un.Spr.spriteId;

		if( MeleeAttack     ) { MeleeAttack.enabled  = un.MeleeAttack.enabled;  MeleeAttack.Copy  ( un.MeleeAttack   ); }
		if( RangedAttack    ) { RangedAttack.enabled = un.RangedAttack.enabled; RangedAttack.Copy ( un.RangedAttack  ); }
		if( MagicAttack     ) { MagicAttack.enabled  = un.MagicAttack.enabled;  MagicAttack.Copy  ( un.MagicAttack   ); }
        if( BeeHiveAttack   ) { BeeHiveAttack.enabled = un.BeeHiveAttack.enabled; BeeHiveAttack.Copy( un.BeeHiveAttack ); }
        if( InfectionAttack ) { InfectionAttack.enabled = un.InfectionAttack.enabled; InfectionAttack.Copy( un.InfectionAttack ); }	 
	}
    
     public void SaveHeader()
    {
        GS.W.Write( ( int ) TileID );
        GS.W.Write( ( int ) UnitType );
        GS.W.Write( ModID );
        GS.SVector2( Pos );
        GS.SVector2( IniPos );
        GS.W.Write( PrefabName );

        if( UnitType == EUnitType.HERO || UnitType == EUnitType.MONSTER )
        {
            GS.W.Write( Body.Sp.Count );
            for( int i = 0; i < Body.Sp.Count; i++ )                                // Save Spells
                Body.Sp[ i ].Save( UnitType );
        }
    }
     public static void LoadHeader( bool isflying = false )
     {
         ETileType tile = ( ETileType ) GS.R.ReadInt32();
         EUnitType utype = ( EUnitType ) GS.R.ReadInt32();
         int _modID = GS.R.ReadInt32();      
         Vector2 pos = GS.LVector2();
         Vector2 inipos = GS.LVector2();
         string pref = GS.R.ReadString();
         Unit un = null;
         if( utype == EUnitType.MONSTER )
         {
             Unit prefabUnit = Map.I.GetUnitPrefab( tile );
             GameObject obj = Map.I.CreateUnit( prefabUnit, pos, ELayerType.MONSTER );
             un = obj.GetComponent<Unit>();
             un.IniPos = inipos;

             if( _modID >= 0 )
             {
                 un.ModID = _modID;
                 un.Md = Map.I.RM.SD.ModList[ _modID ];
             }

             if( un.ModID != -1 )
                 Map.I.RM.SD.ModList[ un.ModID ].ApplyMod( ( int ) un.IniPos.x,                 // Applies the mod
                 ( int ) un.IniPos.y, un.ModID, un );

             if( isflying )                                                                     // Flying Units
             {
                 G.HS.AddFlying( un );
                 if( Map.I.Unit[ ( int ) pos.x, ( int ) pos.y ] )                               // This is to enable saving more than one monster in a single tile key: save2monsters
                 if( tile == Map.I.Unit[ ( int ) pos.x, ( int ) pos.y ].TileID )
                     Map.I.Unit[ ( int ) pos.x, ( int ) pos.y ] = null;
             }
             else                                                                               // Land Units
             {
                 Map.I.RM.HeroSector.MoveOrder.Add( un );
             }

             int sz = GS.R.ReadInt32();
             for( int i = 0; i < sz; i++ )                                                      // Monster Spells
             {
                 un.Body.Sp[ i ].Load( utype );
             }
         }
         else
         if( utype == EUnitType.GAIA2 )                                                         // Gaia 2
         {
             Unit prefabUnit = Map.I.GetUnitPrefab( tile );
             GameObject obj = Map.I.CreateUnit( prefabUnit, pos, ELayerType.GAIA2 );
             un = obj.GetComponent<Unit>();

             if( _modID >= 0 )
             {
                 un.ModID = _modID;
                 un.Md = Map.I.RM.SD.ModList[ _modID ];
             }

             if( un.ModID != -1 )
                 Map.I.RM.SD.ModList[ un.ModID ].ApplyMod( ( int ) un.IniPos.x,                  // Applies the mod
                 ( int ) un.IniPos.y, un.ModID, un );

             GS.ga2l.Add( un );
         }
         else                                                                                    // Hero
         {
             un = G.Hero;
             int sz = GS.R.ReadInt32();
             for( int i = 0; i < sz; i++ )                                                       // Load Spells
             {
                 if( i >= 8 )
                 {
                     Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Spell" );                // Load FIXED Spells and spawn new objs
                     Spell fsp = tr.GetComponent<Spell>();
                     fsp = tr.GetComponent<Spell>();
                     tr.parent = Map.I.HeroSpells.transform;
                     G.Hero.Body.Sp.Add( fsp );
                     fsp.Attack.Unit = G.Hero;
                     G.Hero.Attacks.Add( fsp.Attack );
                     fsp.Attack.ID = G.Hero.Attacks.Count - 1;
                 }

                 G.Hero.Body.Sp[ i ].Load( utype );
             }
         }

         if( un )                                                                                // Attrib unit values to vars
         {
             un.TileID = tile;
             un.UnitType = utype;
             un.Pos = pos;
             un.PrefabName = pref;
         }
     }
   public void Save()
    {
        GS.STrans( transform );
        //if( Spr )
        //GS.STrans( Spr.transform );
        GS.STrans( Graphic.transform );

        GS.W.Write( Variation );
        GS.W.Write( ( int ) Dir );
        GS.W.Write( Activated );

        if( Body ) Body.Save();                                // Save Body

        if( Control ) Control.Save();                          // Save Control
        
        if( MeleeAttack ) MeleeAttack.Save();                  // Save Melle Attack

        if( RangedAttack ) RangedAttack.Save();                // Save Ranged Attack

        if( InfectionAttack && Body.IsInfected ) 
            InfectionAttack.Save();                            // Save Infected Attack

        if( Water ) Water.Save();                              // Save Water Stuff

        if( Mine ) Mine.Save();                                // Save Mine Stuff

        if( Altar ) Altar.Save();                              // Save Altar Stuff

        PostSaveStuff();                                       // Post Save
    }

   private void PostSaveStuff()
   {
       if( UnitType == EUnitType.HERO )
       {
           G.Hero.Graphic.transform.localPosition = Vector3.zero;
       }
   }

	public void Load()
	{        
        GS.LTrans( transform );
        //if( Spr )
        //GS.LTrans( Spr.transform );   new
        GS.LTrans( Graphic.transform );

        Variation = GS.R.ReadInt32();
        Dir = ( EDirection ) GS.R.ReadInt32();
        Activated = GS.R.ReadBoolean();

        if( Body ) Body.Load();                               // Load Body

        if( Control ) Control.Load();                         // Load Control

        if( MeleeAttack ) MeleeAttack.Load();                 // Load Melle Attack

        if( RangedAttack ) RangedAttack.Load();               // Load Ranged Attack

        if( InfectionAttack && Body.IsInfected ) 
            InfectionAttack.Load();                           // Load Infected Attack

        if( Water ) Water.Load();                             // Load Water stuff

        if( Mine ) Mine.Load();                               // Load Mine stuff

        if( Altar ) Altar.Load();                             // Load Altar stuff

        InitUnitsAfterLoading();                              // Init Units After Loading
	}

    private void InitUnitsAfterLoading()
    {
        if( UnitType == EUnitType.HERO )
        {
            Control.AnimationOrigin = new Vector3( Pos.x, Pos.y, 0 );
            Spr.transform.localPosition = new Vector3( 0, 0, 
            Spr.transform.localPosition.z );
            RotateTo( Dir );
        }

        if( TileID == ETileType.ITEM )                                          // chest or global resource already taken
        {
            if( G.HS.PrizeStepList.Contains( Pos ) ) 
                Map.Kill( this );
        }

        if( TileID == ETileType.BARRICADE )
            SetVariation( Variation );

        if( TileID == ETileType.FISH )
            Map.I.InitFishGraphics( this );

        if( TileID == ETileType.DRAGON1 )
        {
            Body.Animator = Spr.GetComponent<tk2dSpriteAnimator>();
            if( Md.PlayAnimation != "" )
                Body.Animator.Play( Md.PlayAnimation );            
        }

        if( TileID == ETileType.SAVEGAME )
        if( GS.SaveStepList.Contains( Pos ) )
        {
            Body.EffectList[ 2 ].gameObject.SetActive( true );
        }

        if( TileID == ETileType.ARROW   ||
            TileID == ETileType.MINE    ||
            TileID == ETileType.BOULDER ||
            TileID == ETileType.FAN )
        {
            RotateTo( Dir );
        }

        if( TileID == ETileType.FIRE || TileID == ETileType.VINES )
            Util.SetActiveRecursively( Body.EffectList[ 0 ].gameObject, Body.FireIsOn );   
    }
    public void OnDespawned()
    {
        //if( Map.I.Finalizing )
        //{
        //    if( Body != null && Body.WoodAdded != null ) Body.WoodAdded.Clear();
        //    if( Body != null && Body.OriginalWoodAdded != null ) Body.OriginalWoodAdded.Clear();
        //}

        //if( TileID != ETileType.BUILDING )                                                                                   // Buildings are not on shared obj list
        //    this.Copy( Map.I.SharedObjectsList[ ( int ) TileID ], false, false, false );
    }

    public static bool ApplyMove;
    public bool CheckTerrainMove( Vector2 from, Vector2 to, bool gaia, bool gaia2, bool monster, bool arrow, bool hero )
    {
        if( Map.PtOnMap( Map.I.Tilemap, to ) == false ) return false;
        Unit ga = Map.I.GetUnit( to, ELayerType.GAIA );
        Unit mn = Map.I.GetUnit( to, ELayerType.MONSTER );
        Unit ga2 = Map.I.GetUnit( to, ELayerType.GAIA2 );

        if( UnitType == EUnitType.GAIA2 )
        {
            if( ga2 ) return false;                                                                                         // No gaia2 over gaia2 move
        }                                                                            
        if( gaia )
        {
            if( TileID == ETileType.ALGAE )
            {
                Unit water = Map.I.GetUnit( ETileType.WATER, to );                                                          // algae
                if( water == null ) return false;
            }
        }

        if( TileID == ETileType.MINE )
        {
            if( Map.GFU( ETileType.MINE, to ) ) return false;                                                               // no mine over mine move since they are flying units
        }

        if( Control.IsFlyingUnit )                                                                                          // Flying units move anywhere
        if( TileID != ETileType.MINE )   
        if( Control.Resting == false ) return true;
        
        if( UnitType == EUnitType.MONSTER || UnitType == EUnitType.GAIA2 )
        {
            if( Sector.GetPosSectorType( to ) == Sector.ESectorType.GATES ) return false;                                   // No Monster outside cube allowed

            if ( Controller.CheckingLure == false )
            for( int i = 0; i < 8; i++ )
            if ( G.Hero.Body.Sp[ i ].BambooSize > 0 )
            for( int sz = 1; sz <= G.Hero.Body.Sp[ i ].BambooSize; sz++ )                                                  // Bamboo Block
            {
                EDirection dr = Util.GetRelativeDirection( i, G.Hero.Dir );
                Vector2 tg = G.Hero.Pos + ( Manager.I.U.DirCord[ ( int ) dr ] * sz );
                if( to == tg ) return false;
            }

            if( ApplyMove )
            if( Map.I.UpdateTrailCocoon( from, to ) ) return false;                                                          // trail Cocoon check
        }

        if( TileID == ETileType.BOULDER )                                                                                  // Boulder Symetric Pits 
        {
            bool block = Control.CheckSymetricPits();
            if( block ) return false;
        }
    
        if( ValidMonster )
        {
            List<Unit> bm = Map.I.GProj( to, EProjectileType.BOOMERANG );                                                 // Boomerang Blocks Monsters
            if( bm != null && bm.Count > 0 )
                return false;
            if( Map.I.FireBallCount > 0 )                                                                                 // Any fireball flying to that position?
            {
                for( int i = 0; i < G.HS.Fly.Count; i++ )
                if ( G.HS.Fly[ i ].ProjType( EProjectileType.FIREBALL ) )
                if ( G.HS.Fly[ i ].Control.FlyingTarget == to ) return false;

                List<Unit> fb = Map.I.GProj( to, EProjectileType.FIREBALL );                                              // Fireball Blocks Monsters
                if( fb != null && fb.Count > 0 &&
                    fb[ 0 ].Control.FlyingTarget == new Vector2( -1, -1 ) ) return false;
            }
        }

        if( gaia )
        if( ga )
            {
            if( ga.TileID == ETileType.WATER ||
                ga.TileID == ETileType.PIT   ||
                ga.TileID == ETileType.LAVA  )
            {                                                                                                           
                bool raft = Controller.GetRaft( to );                                                                   // Raft
                if( Map.GMine( EMineType.BRIDGE, to ) == null || Control.Floor == 0 )
                if( TileID != ETileType.FROG || ga.TileID == ETileType.PIT )
                if( TileID != ETileType.ALGAE )
                if( raft == false )
                    return false;  
            }

            if( ga.BlockMovement && Controller.GetRaft( to ) == false )
                {     
                    if( ga.TileID != Control.AllowedTerrain )
                    {
                        if( TileID == ETileType.BOULDER &&                                                              // Boulder over Pit
                            ga &&
                            ga.TileID == ETileType.PIT ) { }
                        else
                        {
                            if( TileID == ETileType.BOULDER )                                                           // Boulder do not climb forest
                                return false;
                        }
                    }
                }
                else
                {
                    if( ga.TileID == Control.DeniedTerrain )
                    {
                        if( ga.CanBeWalkedIfSuspended && Control.IsSuspended ) { }
                        else
                        return false;
                    }
                }

            int tile = Map.I.Tilemap.GetTile( ( int ) to.x, ( int ) to.y, ( int ) ELayerType.GAIA );
            if( tile >= 0 && ( int ) tile < Map.I.Tilemap.data.tileInfo.Length )                                     // Decor Gaia Block 
            {
                var tileInfo = Map.I.Tilemap.data.tileInfo[ tile ];
                if( tileInfo.Layer == ELayerType.GAIA )
                if( tileInfo.Blocked )
                    return false;
            }
        }
       
        if( gaia2 )
        if( ga2 )
        {
            Map.I.ValidateMove = true;                                                                                // Hero cant destroy fire so block him
            if( ga2.TileID == ETileType.FIRE )
            if( UnitType == EUnitType.HERO ) 
            if( Mine.TunnelTraveling == false ) return false;

            if( ga2.BlockMovement )
            {
                if( Manager.I.GameType != EGameType.FARM &&
                    ga2.TileID == ETileType.ITEM ) 
                {                                                                            
                    if( UnitType == EUnitType.HERO )
                    {
                        if( ga2.Body.ItemMiniDome.gameObject.activeSelf )                                              // Item has a Mini dome timer active
                        {
                            return false;
                        }
                        if( Controller.RaftMoving == false )
                        if( ga2.Variation == ( int ) ItemType.Res_Raft_Hammer ) return false;                          // Raft hammer block hero
                    }
                }
                else
                if( ga2.TileID != Control.AllowedTerrain ) return false;                                               // Check Gaia2 block
            }
            else
            {
                if( ga2.TileID == Control.DeniedTerrain ) return false;
            }            
        }

        if( monster )
        {
            if( UnitType == EUnitType.MONSTER )
            if( Map.I.IsHeroMeleeAvailable() )                                                                      // Hero Sword Monster Block
            if( Controller.CheckHeroSwordBlock )
            if( TileID != ETileType.SLIME )
            if( TileID != ETileType.FROG )
            if( TileID != ETileType.ROACH )
            if( TileID != ETileType.MINE )
            if( G.Hero.GetFront() == to )
                return false;    

            if( UnitType == EUnitType.HERO )                                                                        // Hero to Jumper Destination (Optimize)
            {
                List<Unit> fl = Map.I.FUnit[ ( int ) to.x, ( int ) to.y ];

                if( fl != null )
                for( int i = 0; i < fl.Count; i++ )
                if ( fl[ i ].Activated )
                if ( fl[ i ].TileID == ETileType.JUMPER           ||
                     fl[ i ].TileID == ETileType.MOTHERJUMPER     ||
                     fl[ i ].TileID == ETileType.WASP             ||
                     fl[ i ].TileID == ETileType.MOTHERWASP       ||
                     fl[ i ].ProjType( EProjectileType.FIREBALL ) ||
                   ( fl[ i ].TileID == ETileType.HERB )
                  &&!Map.I.IsHerbBonus( fl[ i ].Body.HerbType ) )
                {
                    if( fl[ i ].Control.BeingMudPushed == false ) return false;
                    if( fl[ i ].Control.BeingMudPushed )
                    if( new Vector2( ( int ) fl[ i ].Control.JumpTarget.x,
                                     ( int ) fl[ i ].Control.JumpTarget.y ) == to ) return false;
                }
            }

            if( mn && mn.TileID == ETileType.FAN ) return false;
            if( mn )
            if( mn.Activated )
            {
                if( mn.TileID == ETileType.MINE )
                {
                    if( mn.Body.MineType == EMineType.SHACKLE )                     // shackle free step
                    if( ValidMonster || UnitType == EUnitType.HERO ) return true;
                    if( mn.Body.MineType == EMineType.HOOK ) return true;           // Hook mine
                    if( TileID == ETileType.MINE && Body.MineType == EMineType.HOOK ) 
                    if( Body.RopeConnectFather != null && Body.RopeConnectFather.Count > 0 )                        
                        return true;
                }

                //int olda = Map.I.GetPosArea( from );
                //int newa = Map.I.GetPosArea( to );
                //if( olda != -1 && newa != -1 && olda != newa ) return false;
                Map.I.ValidateMove = true;

                if( Control.MonsterPushLevel < 1 )
                {
                    if( UnitType == EUnitType.HERO &&
                        mn.TileID == ETileType.SPIDER ) { }                      // Allow spider step by hero
                    else
                    {
                        if( mn.BlockMovement )                                   // hero climb  mine
                        if( Map.IsClimbable( from ) ||                              
                            Map.IsClimbable( to ) ) { }
                        else
                        {
                            return false;                                                                          // Monster block   
                        }                            
                    }

                    if( Control.CanPushObjects == false )
                    if( Map.IsClimbable( from ) ||
                        Map.IsClimbable( to ) ) { }
                        else 
                        return false;

                    if( !mn.Control.CanBePushed )
                        return false;
                }
            }
        }

        if( arrow )
        if( Map.I.CheckArrowBlockFromTo( from, to, this ) == true ) return false;                                  // Check Arrow block
  
        if( hero )
        if( to == Map.I.Hero.Pos ) return false;                                                                   // Check hero block

        if( UnitType == EUnitType.HERO )
        if( Map.I.NavigationMap.CheckNavigationMap( to ) ) return false;                                           // Navigation Quest Area Block 

        if( UnitType == EUnitType.HERO )                                                                           // Fog
        {
            if( Map.I.GetUnit( ETileType.THORNS, to ) )                                                            // Thorns block only hero
                return false;
            Unit fromFog = Controller.GetFog( from );
            Unit toFog = Controller.GetFog( to );
            if( fromFog && toFog == null )
            if( Controller.HeroBeingPushedByFog == false )
            {
                if( fromFog.Control.RaftMoveDir != EDirection.NONE ) return false;                                 // Fog moving - no exit

                if( Map.I.DirtyFogList.Contains( fromFog.Control.RaftGroupID ) ) return false;                     // Fog Has monsters (dirty)
            }

            if( fromFog == null && toFog )
            {
                if( toFog.Control.RaftMoveDir != EDirection.NONE ) return false;                                  // Fog moving - no entering
            }
            
            //int num = BabyData.GetNumActiveAlgaeinTile( to );
            //Unit snow = Map.I.GetUnit(ETileType.SNOW, to );
            //Unit sand = Map.I.GetUnit( ETileType.SAND, to );                                                      // no sand or snow with algae entering
            //if( sand || snow )
            //if( num > 0 ) return false;
        }
        return true;
    }
    
public bool CheckLeverBlock( bool apply, Vector2 from, Vector2 to, bool move )
{
    if( Map.CheckLeverCrossingBlock( from, to ) ) return true;                                                // Check Lever diagonal crossing
    if( UnitType == EUnitType.MONSTER || move == false )                                                      // Monster blocked by lever
    {
        if( ( TileID == ETileType.MINE && Body.MineType == EMineType.SPIKE_BALL ) || 
            ( ProjType( EProjectileType.BOOMERANG ) ) )
        {
            return Controller.UpdateLeverMine( this, apply, from, to );                                       // iron ball and boomerang pull the lever
        }

        if( Map.DoesLeverBlockMe( to, this ) ) return true;                                                   // Any Lever in my way?

        EDirection dr = Util.GetTargetUnitDir( from, to );
        if( TileID == ETileType.MINE )
        if( Body.MineType == EMineType.INDESTRUCTIBLE )
        if( Body.MineHasLever() )                             // new
        if( dr != EDirection.NONE )
        {
            Vector2 levfrom = Pos + Manager.I.U.DirCord[ ( int ) Body.MineLeverDir ];
            Vector2 levto = levfrom + Manager.I.U.DirCord[ ( int ) dr ];
            if( Map.CheckLeverCrossingBlock( levfrom, levto ) ) return true;
            List<ETileType> ex = new List<ETileType>();
            ex.Add( ETileType.ARROW );
            if( levto != Pos )
            if( Util.CheckBlock( levfrom, levto, this, true, false, false, true, false, true, ex ) )           // Lever needs free terrain to be pushed to
                return true;
            Unit tgm = Map.GFU( ETileType.MINE, levto );
            if( tgm && tgm != this ) return true;
        }
    }

    if( move )
    if( UnitType == EUnitType.HERO )
    if( Controller.UpdateLeverMine( this, apply, from, to ) ) return true;                                    // Rotates the lever

    return false;
}   

public bool CanFlyFromTo( bool bApply, Vector2 from, Vector2 to )
{
    Map m = Map.I;
    if( Sector.GetPosSectorType( to ) == Sector.ESectorType.GATES ) return false;
    if( Map.PtOnMap( m.Tilemap, to ) == false ) return false;                                                   // Out of map dest
    
    bool cmine = true;    
    Unit mine = Map.GFU( ETileType.MINE, to );
    if( mine && mine.Body.MineType == EMineType.SHACKLE ||                                                       // allow shackle step
        Body.MineType == EMineType.HOOK )     
        cmine = false;

    if( Map.IsWall( to, true, cmine ) ) return false;
    if( CheckLeverBlock( bApply, from, to, false ) ) return false;

    if( TileID == ETileType.MOTHERWASP )
    {
        List<Unit> fl = Map.I.GF( to, ETileType.MOTHERWASP );                                                   // No mother wasp overlapping
        if( fl != null && fl.Count > 0 ) return false;
        if( Controller.MWaspTgList.Contains( to ) )
            return false;
    }

    Unit tomn = Map.GFU( ETileType.MINE, to );
    if( cmine == true && tomn ) 
    {
        Unit frmn = Map.GFU( ETileType.MINE, from );
        if( frmn != null && frmn.Body.MineType == EMineType.HOOK ) { } else                                     // hook mine connection step allowed
        if( tomn != null && tomn.Body.MineType == EMineType.HOOK ) { } else
        return false; 
    }

    Unit totr = Map.GFU( ETileType.TRAIL, to );
    if( TileID == ETileType.TRAIL && totr ) return false;                                                       // no trail overlap   

    if( TileID == ETileType.MINE )                                                                              // These are not flying for real
    {
        if( CheckBarricadeBlock( bApply, from, to ) ) return true;                                              // Check Barricade
        Unit mn = Map.I.GetUnit( to, ELayerType.MONSTER );
        if( mn ) return false;                                                                                  // no mine over monsters

        Unit ga = Map.I.GetUnit( to, ELayerType.GAIA );
        if( ga )
        {
            if( Controller.GetRaft( to ) == null ) 
            if( Map.LowTerrain( to ) ) return false;
        }
        if( Controller.CurentMoveTrial == to ) return false;                                                     // To avoid pushed mine to move under hero, suspending him
    }
    if( bApply )
    {
        Unit un = this;
        Controller.MoveFlyingUnitTo( ref un, from, to );                                                        // Move Flying object
    }
    return true;
}

//_________________________________________________________________________________________________________________ Movement Check Function

    public bool CanMoveFromTo( bool bApply, Vector2 from, Vector2 to, Unit pusher )
    {
        ApplyMove = bApply;
        Map m = Map.I;
        if( Map.PtOnMap( m.Tilemap, to ) == false ) return false;                                                   // Out of map dest

        if( Control.IsFlyingUnit )
            return CanFlyFromTo( bApply, from, to );

        CheckFireJump( bApply, ref from, ref to );                                                                  // Fire Jump

        bool terrain = CheckTerrainMove( from, to, true, true, false, true, false );
        
        if( CheckBarricadeBlock( bApply, from, to ) ) return true;                                                  // Check Barricade

        if( CheckBambooBlock( bApply, from, to ) ) return true;                                                     // Check Bamboo

        if( CheckDiagonalMove( bApply, from, to ) ) return true;                                                    // Check Diagonal Move
        
        if( CheckPurchase( bApply, from, to ) ) return true;                                                        // Check Purchase

        if( CheckClimbing( bApply, from, to ) ) return false;                                                       // Check Climbing

        if( CheckPerkRestrictions( bApply, from, to ) == false ) return false;                                      // Checks for movement level allowancePerk Restrictions

        if( Control.CheckSpiderBlock( from, to ) ) return false;                                                    // Checks Spider block

        if( CheckLeverBlock( bApply, from, to, true ) ) return false;                                               // Mine lever block

        if( terrain == false )  return false;

        if( from == to ) return true;

        if( Control.MonsterPushLevel >= 1 || Control.CanPushObjects || Control.IsDynamicObject() )                  // Unit Can push others
        {
            bool res = Push( bApply, from, to, pusher );
            if( res ) return false;

            if( TileID == ETileType.ARROW && Pos == G.Hero.Pos )
            if( Util.Manhattan( from, to ) <= 1 )
            {
                Vector2 dif = to - from;
                res = Push( bApply, from - dif, from, pusher, true );                                               // Over arrow push                      
            }
            if( res ) return false;
        } 
        else                                                                                                        // Unit Can NOT push
        {
            bool terrain2 = CheckTerrainMove( from, to, true, true, true, true, true );
            if( terrain2 == false ) return false;
        }

        if( CheckShacklePulling( bApply, from, to ) ) return false;                                                 // Shackle Pulling

        if( UnitType == EUnitType.MONSTER )
        {
            if( Map.I.CurrentArea != -1 )
            if( m.PtOnAreaMap( to ) == false ) return false;

            if( Control.NeedLineOfSightToMove )                                                                     // Line of sight check - optimize
            {
                Control.HasLineOfSight = Map.I.HasLOS( Map.I.Hero.Pos, from );
                if( Control.HasLineOfSight == false ) return false;
            }

            bool terrain3 = CheckTerrainMove( from, to, true, true, false, true, false );
            if( terrain3 == false ) return false;
        }
        else
        if( UnitType == EUnitType.HERO )
        {
            bool terrain4 = CheckTerrainMove( from, to, true, true, true, true, false );
            if( terrain4 == false ) return false;
        }

        if( bApply ) Control.ApplyMove( from, to );                                                                 // Applies the move

        return true;
    }
    public bool CheckClimbing( bool bApply, Vector2 from, Vector2 to, bool movechecking = true )
    {
        if( Controller.RaftMoving ) return false;
        if( Manager.I.GameType == EGameType.FARM ||
            Manager.I.GameType == EGameType.NAVIGATION )
        {
            Unit tog = Map.I.GetUnit( to, ELayerType.GAIA );                                // No climbing on farm or navigation
            if( Controller.GetRaft( to ) == null )
                if( tog && tog.BlockMovement ) return true;
            return false;
        }
        if( Sector.GetPosSectorType( to ) == Sector.ESectorType.GATES )                     // No border gate climbing
        if( Map.I.GetUnit( ETileType.CLOSEDDOOR , to ) )       
            return true;

        EDirection mov = Util.GetTargetUnitDir( from, to );
        bool cfrom = Map.IsClimbable( from );                                               // Climbing
        bool cto = Map.IsClimbable( to );
        if( TileID == ETileType.BOULDER ) return false;                                     // Allow boulder to push mine
        bool up = false;
        bool block = Controller.CheckMineBlock( from, to, this, movechecking, ref up );     // Mine block main method

        int floor = Controller.GetUnitFloor( from, to, this );                              // Gets the target floor
        
        Unit frm = Map.GFU( ETileType.MINE, from );
        Unit tom = Map.GFU( ETileType.MINE, to );
        Unit tob = Map.GMine( EMineType.BRIDGE, to );
        Unit frb = Map.GMine( EMineType.BRIDGE, from );          
        if( Control.Floor == 2 )
        if( frm && frm.Body.UPMineType == EMineType.BRIDGE )
        if( tob )
            {
                if( tom && tom.Mine.FrontSupport == 1 && tom.InvDr() == mov ) {} else     // bridge suppor different floors  
                if( tom && tom.Mine.BackSupport == 1 && tom.Dir == mov )      {} else
                return true;                                                              // this prevents hero under upbridge jumping to ground bridge
            }

        if( Control.Floor == 4 )
        if( Controller.BridgeBlock( from, to ) ) return true;

        if( tob == null )
        if( Control.Floor == 0 && floor == 2 ) return true;                                 // no floor jumping
        if( Control.Floor == 2 && floor == 0 ) return true;
        if( Control.Floor == 0 && floor == 4 ) return true;                                 // no top floor jumping to ground
        if( Control.Floor == 4 && floor == 0 ) return true;
        
        if( tom == null || tom.Body.UPMineType != EMineType.BRIDGE )                        // allows bridge climbing
        if( frb == null )
        if( Control.Floor == 2 && floor == 4 ) return true;                                 // no top to bottom floor jumping

        if( tob == null && ( tom == null ||
            tom.Body.UPMineType != EMineType.BRIDGE ) )                                     // allows bridge descending
        if( Control.Floor == 4 && floor == 2 ) return true;                                

        if( Control.StairStepDir == 0 && floor == 0 ) return true;                          // Dont jump from the top of the stair to the ground
        if( Control.Floor != 1 && Control.Floor != 3 )
        if( Control.StairStepDir == 0 && floor == 2 && up ) return true;                    // Dont jump from the top of the upstair to the ground
        
        if( block ) return true;
        return false;
    }
    public bool CheckBambooBlock( bool bApply, Vector2 from, Vector2 to, int rot = 0 )
    {
        if( UnitType != EUnitType.HERO ) return false;
        Sector s = Map.I.RM.HeroSector;
        if( s == null || s.Type != Sector.ESectorType.NORMAL ) return false;
        List<Vector2> tgl = new List<Vector2>();
        List<Vector2> froml = new List<Vector2>();
        Controller.IgnoreLevelPushList = new List<Unit>();
        List<Unit> orbl = new List<Unit>();
        List<Unit> pmnl = new List<Unit>();
        List<int> mnlid = new List<int>();
        List<Unit> amnl = new List<Unit>();
        EDirection inv = EDirection.NONE;
        List<int> breaksz = new List<int>();
        bool firefx = false;
        bool attfx = false;
        EDirection _dr = Dir;
        if( rot != 0 ) _dr = Util.RotateDir( ( int ) Dir, rot );
        bool low = false;
        bool front = false;
        for( int i = 0; i < 8; i++ )
        {
            breaksz.Add( -1 );
            if( G.Hero.Body.Sp[ i ].BambooSize > 0 )
            for( int sz = 1; sz <= G.Hero.Body.Sp[ i ].BambooSize; sz++ )                                                // Make lists
                {
                    EDirection dr = Util.GetRelativeDirection( i, _dr );
                    Vector2 tg = to + ( Manager.I.U.DirCord[ ( int ) dr ] * sz );
                    tgl.Add( tg );
                    froml.Add( from + ( Manager.I.U.DirCord[ ( int ) dr ] * sz ) );
                    int id = tgl.Count - 1;

                    if( rot != 0 )
                    {
                        if( rot < 0 )
                        {
                            if( dr == EDirection.N || dr == EDirection.NW ) froml[ id ] = tgl[ id ] + new Vector2( +1, 0 );  // Adjusts the "from" bamboo position for pushing and inverting the lever
                            if( dr == EDirection.S || dr == EDirection.SE ) froml[ id ] = tgl[ id ] + new Vector2( -1, 0 );
                            if( dr == EDirection.E || dr == EDirection.NE ) froml[ id ] = tgl[ id ] + new Vector2( 0, -1 );
                            if( dr == EDirection.W || dr == EDirection.SW ) froml[ id ] = tgl[ id ] + new Vector2( 0, +1 );
                        }
                        if( rot > 0 )
                        {
                            if( dr == EDirection.N || dr == EDirection.NE ) froml[ id ] = tgl[ id ] + new Vector2( -1, 0 );
                            if( dr == EDirection.S || dr == EDirection.SW ) froml[ id ] = tgl[ id ] + new Vector2( +1, 0 );
                            if( dr == EDirection.E || dr == EDirection.SE ) froml[ id ] = tgl[ id ] + new Vector2( 0, +1 );
                            if( dr == EDirection.W || dr == EDirection.NW ) froml[ id ] = tgl[ id ] + new Vector2( 0, -1 );
                        }   
                    }               

                    Unit fire = Map.I.GetUnit( ETileType.FIRE, tg );                                                  // fire list
                    if( fire != null && fire.Body.FireIsOn && breaksz[ i ] == -1 )
                    {
                        breaksz[ i ] = sz - 1;
                        firefx = true;
                        Map.I.CreateExplosionFX( fire.Pos, "Fire Explosion", "" );                                    // Smoke Cloud FX
                    }
                    Unit orb = Map.I.GetUnit( ETileType.ORB, tg );
                    if( orb != null )
                    {
                        orbl.Add( orb );                                                                              // orb list
                        EDirection mov = Util.GetTargetUnitDir( from, to );
                        if( mov == dr )
                        if( sz == G.Hero.Body.Sp[ i ].BambooSize )                            
                            inv = ( EDirection ) i;
                    }
                    Unit mn = Map.I.GetUnit( tg, ELayerType.MONSTER );                                                // monster list
                    if( mn == null ) mn = Map.GFU( ETileType.MINE, tg );
                
                    if( mn )
                    if( mn.ValidMonster || mn.TileID == ETileType.MINE )
                    {
                        bool force = true;
                        if( mn.TileID == ETileType.MINE ) force = false;
                        bool res = G.Hero.Control.UpdateMudObjectPush( false, froml[ id ], tgl[ id ], force, true );
                        if( Controller.LowTerrainPush ) low = true;

                        if( res )                                                                                     // Push ok
                        { 
                            pmnl.Add( mn ); 
                            mnlid.Add( id );
                            Controller.IgnoreLevelPushList.Add( mn );
                        }
                        else                                                                                          // no push
                        if( low == false ) 
                        {
                            if( Util.IsPosInFront( from, to, tgl[ id ] ) )                                          
                                front = true;                                                                         // monster frontally attacked by bambo: block

                            amnl.Add( mn );
                            mnlid.Add( id );
                            if( mn.ValidMonster )
                            {
                                attfx = true;
                                if( breaksz[ i ] == -1 )
                                    breaksz[ i ] = sz - 1;
                            }      
                        }                    
                    }
                }
        }

      if( tgl.Count > 0 )
          iTween.PunchRotation( G.Hero.Spr.gameObject, new Vector3( 0, 0, Util.RandSig( 20 ) ), .25f );          // punch fx

        if( tgl.Count <= 0 ) return false;

        if( bApply && front == false )
        for( int i = 0; i < amnl.Count; i++ )
        {                                                                                                        // Attack Monsters
            if( amnl[ i ].ValidMonster )
            {
                amnl[ i ].Body.ReceiveDamage( Map.I.RM.RMD.BaseBambooAttackDamage,
                EDamageType.MELEE, Map.I.Hero, G.Hero.MeleeAttack );
                MasterAudio.PlaySound3DAtVector3( "Ninja", G.GetHpos( -8 ) );
            }
        }

        List<Vector2> bll = new List<Vector2>();
        for( int i = 0; i < tgl.Count; i++ )
        {
            if( BlocksBamboo( froml[ i ], tgl[ i ], pmnl ) )
            {
                bll.Add( tgl[ i ] );                                                                              // Bamboo blocked?
                Map.I.CreateExplosionFX( tgl[ i ] );                                                              // Smoke Cloud FX
            }
        }

        if( bll.Count > 0 ) return true;                                                                          // failed
        if( low ) return true;
        if( front ) return true;

        if( bApply )
        for( int i = 0; i < 8; i++ )                                                                              // Burns the Bamboo
        {
            if( breaksz[ i ] != -1 )
            {
                int size = G.Hero.Body.Sp[ i ].BambooSize - breaksz[ i ];
                G.Hero.Body.Sp[ i ].BambooSize -= size;                
                Item.AddItem( ItemType.Res_Bamboo, -size );
                iTween.PunchRotation( G.Hero.Spr.gameObject, new Vector3( 0, 0, 30 ), .5f );                       // fx
                if( firefx )
                {                    
                    MasterAudio.PlaySound3DAtVector3( "Fire Ignite", to );
                }
                if( attfx || firefx )
                    MasterAudio.PlaySound3DAtVector3( "Wood Break", Map.I.Hero.Pos );
            }
        }

        for( int i = 0; i < tgl.Count; i++ )                                                                        // Bamboo x lever
        {
            Controller.UpdateLeverMine( G.Hero, true, froml[ i ], tgl[ i ] );
        }

        if( bApply )
        if( inv != EDirection.NONE )
        {
            G.Hero.Body.Sp[ ( int ) inv ].BambooSize--;                                                             // invert bamboo
            G.Hero.Body.Sp[ ( int ) Util.GetInvDir( ( EDirection ) inv ) ].BambooSize++;
        }

        if( bApply )
        for( int i = 0; i < orbl.Count; i++ )
        {
            orbl[ i ].StrikeTheOrb( G.Hero );                                                                       // Bamboo strike the orb
        }

        for( int i = 0; i < pmnl.Count; i++ )
        {
            int id = mnlid[ i ];
            G.Hero.Control.UpdateMudObjectPush( true, froml[ id ], tgl[ id ], true,  true );                        // Push monsters
        }

        return false;
    }

    public static bool BlocksBamboo( Vector2 from, Vector2 to, List<Unit> mnlist )
    {
        if( Map.I.Unit[ ( int ) to.x, ( int ) to.y ] )
        {
            if( Map.I.Unit[ ( int ) to.x, ( int ) to.y ].TileID == ETileType.BOULDER ) return true;
            //if( Map.I.Unit[ ( int ) to.x, ( int ) to.y ].TileID == ETileType.ORB ) return true;
        }

        Unit un = Map.GFU( ETileType.MINE, to );
        if( mnlist != null )
        if( un && mnlist.Contains( un ) == false ) return true;

        if( Map.I.Gaia[ ( int ) to.x, ( int ) to.y ] )
        {
            if( Map.I.Gaia[ ( int ) to.x, ( int ) to.y ].TileID != ETileType.PIT )  
            if( Map.I.Gaia[ ( int ) to.x, ( int ) to.y ].TileID != ETileType.WATER )  
            if( Map.I.Gaia[ ( int ) to.x, ( int ) to.y ].BlockMovement ) return true;
        }
        if( Map.CheckLeverCrossingBlock( from, to ) ) return true;
        if( Controller.UpdateLeverMine( G.Hero, false, from, to ) ) return true;
        return false;
    }

    public bool CheckShacklePulling( bool bApply, Vector2 from, Vector2 to, bool beingPushed = false )       // ainda nao consegui no gpt implementar pull em serie, TBD
    {    
        if( Body.RopeConnectSon )
        {
            Unit un = Body.RopeConnectSon;
            int dist = Util.Manhattan( un.Pos, to ) - 1;
            if( un.Body.OriginalHookType == EMineType.SHACKLE )
            if( dist > Body.ShackleDistance )
                {
                    if( un.Body.ShacklePullList.Contains( this ) ) return false;
                    un.Body.ShacklePullList.Add( this );
                    Vector2 bestpos = un.Control.GetBestStandardMove( to, false );
                    bool res = un.CanMoveFromTo( bApply, un.Pos, bestpos, G.Hero );

                    un.Control.RealtimeMoveTried = true;
                    int destdist = Util.Manhattan( to, bestpos ) - 1;
                    if( res && destdist <= un.Body.ShackleDistance )                                   // move pulled object                                                                                 
                    {
                        MasterAudio.PlaySound3DAtVector3( "Chain Rattling", G.Hero.Pos );
                    }
                    else
                    {
                        if( res == false && beingPushed )                                             // Chain squashed                                              
                        {
                            if( un.UnitType == EUnitType.HERO )
                                Map.I.StartCubeDeath();
                            if( un.UnitType == EUnitType.MONSTER )
                                un.Kill();
                        }
                        return true;
                    }
                }
            return false;
        }

        for( int i = 0; i < Body.RopeConnectFather.Count; i++ )
        if ( Body.ShacklePullList.Contains( Body.RopeConnectFather[ i ] ) == false )  
        {
            Unit un = Body.RopeConnectFather[ i ];
            un.Body.ShacklePullList.Add( this );
            int dist = Util.Manhattan( un.Pos, to ) - 1;
            if( UnitType == EUnitType.MONSTER )
            if( to == Pos ) dist -= 1;

            if( Body.OriginalHookType == EMineType.SHACKLE )
            if( dist > un.Body.ShackleDistance )
            {
                Vector2 bestpos = un.Control.GetBestStandardMove( to, false );

                bool res = un.CanMoveFromTo( bApply, un.Pos, bestpos, G.Hero );
                if( res == false )
                {
                    if( UnitType == EUnitType.HERO )
                    {
                        MasterAudio.PlaySound3DAtVector3( "Hero Damage", G.Hero.Pos );            // Sound FX
                        Body.CreateDeathFXAt( Pos );                                              // Blood FX
                        Vector3 pun = G.Hero.Pos - un.Pos;
                        iTween.PunchPosition( un.Graphic.gameObject, pun * 0.2f, .5f );           // punch FX
                    }
                    return true;
                }
                un.Control.RealtimeMoveTried = true;
                int destdist = Util.Manhattan( to, bestpos ) - 1;
                if( res && destdist <= un.Body.ShackleDistance )                                                                                                                   
                {
                    un.CanMoveFromTo( true, un.Pos, bestpos, G.Hero );                            // move pulled object
                    MasterAudio.PlaySound3DAtVector3( "Chain Rattling", G.Hero.Pos );
                }
                else
                {
                    if( res == false && beingPushed )                                             // Chain squashed
                    {
                        if( UnitType == EUnitType.HERO )
                            Map.I.StartCubeDeath();
                        if( UnitType == EUnitType.MONSTER )
                            Kill();
                    }

                    if( UnitType == EUnitType.HERO )
                    {
                        MasterAudio.PlaySound3DAtVector3( "Hero Damage", G.Hero.Pos );            // Sound FX
                        Body.CreateDeathFXAt( Pos );                                              // Blood FX
                        Vector3 pun = G.Hero.Pos - un.Pos;
                        iTween.PunchPosition( un.Graphic.gameObject, pun * 0.2f, .5f );           // punch FX
                    }
                    return true;
                }
            }
        }
        return false;
    }
        
    public bool CheckDiagonalMove( bool bApply, Vector2 from, Vector2 to )
    {
        if( UnitType != EUnitType.HERO ) return false;
        bool diag = false;
        if( Map.I.RM.SD && CubeData.I.StonepathDiagonalMovement == false )
        if( Map.I.GetUnit( ETileType.STONEPATH, G.Hero.Pos ) ) 
            diag = true;
        if( Controller.ForceDiagonalMovement ) return false;                              // grabbing monster allows free diagonal move                                   
        
        if( Control.MovementLevel < 4 || diag )                                           // Diagonal move moved to here to avoid diagonal purchase bug
        {
            Vector2 dif = to - from;
            bool ok = true;
            if( dif == new Vector2(  1,  1 ) ) ok = false;
            if( dif == new Vector2(  1, -1 ) ) ok = false;
            if( dif == new Vector2( -1, -1 ) ) ok = false;
            if( dif == new Vector2( -1,  1 ) ) ok = false;

            if( ok == false )
            {
                //Map.I.ShowMessage( Language.Get( "ERROR_DIAGONALMOVEFORBIDDEN" ) );
                return true;
            }
        }
        return false;
    }

    public bool CheckPurchase( bool bApply, Vector2 from, Vector2 to )
    {
        if( UnitType != EUnitType.HERO ) return false;
        Unit un = Map.I.GetUnit( ETileType.CLOSEDDOOR, to );
        Unit suspended = Map.I.GetUnit( ETileType.CLOSEDDOOR, G.Hero.Pos );
        if( suspended ) return false;

        if( un == null ) un = Map.I.GetUnit( ETileType.DOME, to );

        if( Manager.I.GameType == EGameType.CUBES )
            if( un != null && un.TileID == ETileType.CLOSEDDOOR )
                if( Sector.GetPosSectorType( un.Pos ) == Sector.ESectorType.GATES )
                {
                    if( Map.I.RM.CheckGate( true, to ) == false ) return false;
                }
                else
                {
                    Sector s = Map.I.RM.HeroSector;
                    if( un && un.TileID == ETileType.CLOSEDDOOR )
                    {
                        List<Unit> itl = new List<Unit>();
                        List<ItemType> ittpl = new List<ItemType>();
                        List<float> itamt = new List<float>();
                        for( int y = ( int ) s.Area.yMin; y < s.Area.yMax; y++ )                                         // Check For Items based gate openning
                            for( int x = ( int ) s.Area.xMin; x < s.Area.xMax; x++ )
                                if( Map.PtOnMap( Map.I.Tilemap, new Vector2( x, y ) ) )
                                    if( Map.I.GateID[ ( int ) un.Pos.x, ( int ) un.Pos.y ] == Map.I.GateID[ x, y ] )
                                    {
                                        Unit it = Map.I.GetUnit( ETileType.ITEM, new Vector2( x, y ) );
                                        if( it )
                                        {
                                            itl.Add( it );
                                            if( ittpl.Contains( ( ItemType ) it.Variation ) == false )                               // add new type of item
                                            {
                                                ittpl.Add( ( ItemType ) it.Variation );
                                                itamt.Add( it.Body.StackAmount );
                                            }
                                            else
                                            {
                                                int id = ittpl.IndexOf( ( ItemType ) it.Variation );                                 // item exists, just add amount
                                                itamt[ id ] += it.Body.StackAmount;
                                            }
                                        }
                                    }
                        bool purchase = true;
                        for( int i = 0; i < ittpl.Count; i++ )                                                            // Are all items ok? (available)
                        {
                            float avail = Item.GetNum( ittpl[ i ] );
                            if( avail < itamt[ i ] ) purchase = false;
                        }
                        if( ittpl.Count < 1 ) purchase = false;

                        if( purchase )                                                                                   // Open the gate
                        {
                            for( int i = 0; i < itl.Count; i++ )
                            {
                                itl[ i ].Kill();                                                                          // Destroy items over gate
                                Controller.CreateMagicEffect( itl[ i ].Pos );                                             // Create Magic effect
                            }
                            List<int> di = new List<int>();
                            un.SetDoorState( un.Pos, ETileType.DOOR_OPENER, ref di, false );                              // Set door state
                            for( int i = 0; i < ittpl.Count; i++ )
                            {
                                Item.AddItem( ( ItemType ) ittpl[ i ], -itamt[ i ] );                                     // Charge items
                            }
                            MasterAudio.PlaySound3DAtVector3( "Open Gate", G.Hero.Pos );                                  // Sound FX                    
                        }
                        else                                                                                              // not enough items
                        {
                            for( int i = 0; i < ittpl.Count; i++ )
                            {
                                float avail = Item.GetNum( ( ItemType ) ittpl[ i ] );
                                float need = itamt[ i ] - avail;
                                if( need > 0 )
                                    Message.CreateMessage( ETileType.NONE, ittpl[ i ],                                      // needed message
                                    " +" + need.ToString( "0." ) + " Needed!", to, Color.red,
                                    Util.Chc(), Util.Chc(), 4, i * .3f );
                            }

                            for( int i = 0; i < itl.Count; i++ )
                            {
                                Vector3 pun = Util.GetTargetUnitVector( G.Hero.Pos, itl[ i ].Pos );
                                iTween.PunchPosition( itl[ i ].Graphic.gameObject, pun * 0.25f, .5f );                  // punch effect                      
                            }
                            if( ittpl.Count > 1 )
                                MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );                             // play error sound FX
                        }
                        Controller.StitchesPunishment = false;
                        Controller.PlayBumpSound = false;
                    }
                    return false;
                }

        if( un )
        {
            if( Map.I.RM.RMD.AutoBuyMode && Map.I.RM.RMD.AutoBuyDirectPurchase == false )
                if( Sector.GetPosSectorType( to ) == Sector.ESectorType.LAB )
                {
                    Message.CreateMessage( ETileType.NONE, "Autobuy Direct Purchase Unavailable!",
                                                      Pos, new Color( 1, 0, 0, 1 ), true, true, 7 );
                    return false;
                }

            if( un.TileID == ETileType.DOME )
            {
                Controller.StitchesPunishment = false;
                int olda = Map.I.GetPosArea( from );                                                                // NO out area dome destuction to inner dome
                int newa = Map.I.GetPosArea( to );
                if( olda == -1 && newa != -1 ) return false;

                Artifact ar = Quest.I.GetArtifactInPos( to );
                if( ar != null )
                {
                    float price = ar.CalculatePrice();
                    un.UpdateDomePrice( price );
                    int lim = -1;
                    if( ar.MapLevelLimit > 0 ) lim = ar.MapLevelLimit;
                    if( Map.I.Hero.CheckLevelLimits( ar.PerkType, lim ) )
                    {
                        string msg = "Level Limit Reached!";
                        if( ar.MapLevelLimit > 0 ) msg = "Level Limit for\nthis map Reached!";
                        Message.RedMessage( msg );
                        return false;
                    }
                }
            }

            if( bApply == false ) return true;

            if( bApply )
            {
                if( un.TileID == ETileType.CLOSEDDOOR )
                {
                    Map.I.Gaia[ ( int ) to.x, ( int ) to.y ].OpenDoor();
                    //Map.I.UpdateTransLayerTilemap( false );
                    Map.I.RM.UpdateGatesCost( to );
                }
                if( un.TileID == ETileType.DOME )
                    un.Kill();
            }
            return true;
        }
        return false;
    }

    public bool CheckBarricadeBlock( bool bApply, Vector2 from, Vector2 to )
    {
        Unit bar = Map.I.GetUnit( ETileType.BARRICADE, to ); 
		if( bar == null ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, this ) ) return false;         
        if( bar.Activated == false ) return false;
        if( UnitType == EUnitType.MONSTER )
        if( Body.MakePeaceful > 0 ) return false;

        bool transf = false;
		if( bar )
		{
			if( bApply )
			{
                int bs = Map.I.Unit[ ( int ) to.x, ( int ) to.y ].Variation;
                int amt = -1 ;

                if( Control.BarricadeFighterLevel >= 5 )                                                         // Transfer
                if( Map.I.GetPosArea( G.Hero.GetFront() ) != -1 )
                {
                    Vector2 s = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.S, 1 );                      // From South
                    Unit un = Map.I.GetUnit( ETileType.BARRICADE, s );
                    int cost = 2;

                    if( Control.BarricadeFighterLevel >= 6 ) cost = 1;

                    if( G.Hero.GetFront() == to )
                    if( un )
                    {
                        if( un.Variation > 0 + cost - 1 )
                        {
                            bool r = un.SetBarricadeSize( false, true, un.Variation - cost );                    // Source barricade update
                            if( r ) { amt = +1; transf = true; }
                        }
                        else amt = 0;                                                                            // Only one sized Barricade, do nothing
                    }
                    else amt = 0;

                if( amt == 0 )
                    if( Control.BarricadeFighterLevel >= 7 )                                                     // Transfer
                    {
                        cost = 3;
                        if( Control.BarricadeFighterLevel >= 8 ) cost = 2;
                        if( Control.BarricadeFighterLevel >= 9 ) cost = 1;

                        if( G.Hero.GetFront() == to )
                        {
                            Vector2 se = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.SE, 1 );            // From SE
                            un = Map.I.GetUnit( ETileType.BARRICADE, se );
                            if( un )
                            {
                                if( un.Variation > 0 + cost - 1 )
                                {
                                    bool r = un.SetBarricadeSize( false, true, un.Variation - cost );            // Source barricade update
                                    if( r ) { amt = +1; transf = true; }
                                }
                                else amt = 0;
                            }
                            else amt = 0;

                            Vector2 sw = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.SW, 1 );            // From SW
                            un = Map.I.GetUnit( ETileType.BARRICADE, sw );
                            if( amt == 0 && un )
                            {
                                if( un.Variation > 0 + cost - 1 )
                                {
                                    bool r = un.SetBarricadeSize( false, true, un.Variation - cost );            // Source barricade update
                                    if( r ) { amt = +1; transf = true; }
                                    else amt = 0;
                                }
                                else amt = 0;
                            }
                        }
                    }
                }

                int touchcount = Map.I.Unit[ ( int ) to.x, ( int ) to.y ].Body.TouchCount;
                int dest = bs + amt;
                if( UnitType == EUnitType.HERO )
                    Map.I.ValidateMove = true;
                bool res = Map.I.Unit[ ( int ) to.x, ( int ) to.y ].SetBarricadeSize( false, true, dest, true, this );      // Destination barricade update       
                UpdateBarricadeAnimation( to );
                UpdateBurningBarricadeDamage( to, true );
                return res;
			}
            if( Manager.I.GameType == EGameType.CUBES )  // Dungeon
            if( Map.I.RM.RMD.ClassicBarricadeDestruction )
                return true;
            return false;
		}
		return false;
    }

    public void UpdateBurningBarricadeDamage( Vector2 to, bool barricade )
    {
        return;
        int unarea = Map.I.GetPosArea( Pos );
        if( unarea != -1 )
            if( UnitType == EUnitType.MONSTER )
                if( Map.I.Unit[ ( int ) to.x, ( int ) to.y ].Body.FireIsOn || barricade &&                     // Burning barricade dmg
                    Map.I.Gaia2[ ( int ) to.x, ( int ) to.y ] &&
                    Map.I.Gaia2[ ( int ) to.x, ( int ) to.y ].Body.FireIsOn )
                    Body.TakeFireDamage( ( int ) to.x, ( int ) to.y, false ); 
    }

    public void UpdateBarricadeAnimation( Vector2 to )
    {
        if( Map.I.Unit[ ( int ) to.x, ( int ) to.y ] != null )                                          // Shake Animation
        {
            iTween.ShakePosition( Map.I.Unit[ ( int ) to.x, ( int ) to.y ].Graphic.gameObject,
            new Vector3( .09f, .09f, 0 ), .2f );
            iTween.ShakePosition( Graphic.gameObject, new Vector3( .09f, .09f, 0 ), .2f );

            if( Map.I.Unit[ ( int ) to.x, ( int ) to.y ].Variation < 0 )
            {
                Map.I.Unit[ ( int ) to.x, ( int ) to.y ].Kill();
            }
        }
        //MasterAudio.PlaySound3DAtVector3( "Destroy Barricade", transform.position, .6f );           // Destruction Noise
    }

    public bool DestroyBarricade( Vector2 to )
    {
        UpdateBurningBarricadeDamage( to, true );
        bool res = SetBarricadeSize( false, true, Variation - 1, false, this );                       // set barricade size
        if( Variation < 0 )
        {
            Kill();                                                                                   // Destroy barricade obj           
        }
        if( res )
        {
            UpdateBarricadeAnimation( to );
            Map.I.CreateExplosionFX( to );                                                            // Smoke Cloud FX
            Controller.CreateMagicEffect( to, "Mine Debris" );                                        // Debris FX
        }
        return false;
    }

    public bool SetBarricadeSize( bool original, bool checkperk, int size, bool touch = false, Unit un = null, bool fx = true )
    {
        if( un != null && un.UnitType == EUnitType.MONSTER ) checkperk = false;
        if( un && un.TileID == ETileType.ARROW ) return false;

        if( checkperk )
        {
            int max = ( int ) G.Hero.Body.DestroyBarricadeLevel;
            if( max < 0 ) max = 0;   
    
            int extra = Map.I.GetNeighborBarricadesTouchedCount( Pos );
            max += extra;

            int bararea = Map.I.GetPosArea( Pos );

            string msg = "";
            if( Variation >= max )                                                                                    // Too Big
            {
                int req = ( Variation + 1 ) - max;
                msg = "Barricade too big! (+" + req + ")";
            }

            int amt = ( Variation + 1 ) - Map.I.RM.HeroSector.FireIgnitionCount;
            if( Body.FireIsOn )
            if( amt > 0 )
            {
                msg += "\nLight " + amt + " more firepits to destroy \nthis Burning Barricade!";                       // Burning Barricade
            }

            if( Helper.I.FreeBarricadeDestroy ) msg = "";
            if( Body.TouchCount > 0 ) msg = "";                                                                          // Already touched, so force destruction

            if( msg != "" )
            {
                Message.RedMessage( msg );
                return false;
            }
        }

        if( Map.I.CurrentArea == -1 ) original = true;

        if( Map.I.RM.RMD.BarricadeDestroyWaitTime > 0 )                                                        // Barricade timer not yet ready
            {
                if( Body.BumpTimer > 0 ) return false;
            }

        bool res = SetVariation( size, original );
        if( !res ) return false;

        Map.I.BarricadeDestroyedInTheTurn = true;
        if( Variation < 0 ) Map.I.ForceUpdateLOSFire = true;

        int area = Map.I.GetPosArea( Map.I.Hero.Pos );

        if( area != -1 )  
        {
            if( Map.I.CurrentArea != -1 )                                                                   // No barricade update for Cleared allowed
            if( Map.I.AreaCleared ) return false;
            else
            if( Quest.I.CurLevel.AreaList[ area ].Cleared ) return false;
        }

        if( area == -1 )                                                                                    // Updates tilemap too if from out area
            Quest.I.Dungeon.Tilemap.SetTile( ( int ) Pos.x, ( int ) Pos.y,
            ( int ) ELayerType.MONSTER, ( int ) ETileType.BARRICADE + size );

        int barArea = Map.I.GetPosArea( Pos );
        if( touch )
        {                                               
            bool unok = false;                                                                             // Only counts as touched by monster if hero has level AND needs to be threatened
            if( un.UnitType == EUnitType.HERO ) unok = true;
            if( un.UnitType == EUnitType.MONSTER && un.Body.ThreatLevel > 0 ) unok = true;
            Body.BumpTimer = Map.I.RM.RMD.BarricadeDestroyWaitTime;                                        // Barricade refresh time

            if( unok )
            {
                Body.TouchCount++;                                                                         // Add touch

                if( un.UnitType == EUnitType.HERO )
                {
                    Controller.BarricadeBumpTimeCount = 0;
                    Map.I.CountRecordTime = true;
                    Controller.HeroBumpTarget = Pos;
                    Map.I.TimeKeyPressing = 0;
                }
            }
        }

        if( fx )
        {
            Map.I.CreateExplosionFX( Pos, "Fire Explosion" );                                         // Smoke Cloud FX
           // Controller.CreateMagicEffect( Pos, "Mine Debris" );                                     // Debris FX
        }

        if( Map.I.GetPosArea( Pos ) == -1 )                                                           // Kill Outarea Barricade
        if( Variation < 0 )
        {
            Kill();
        }

        if( un )
        {
            if( un.TileID == ETileType.ROACH || un.TileID == ETileType.SCARAB )                      // Reset Roach and infected scarab attack timer after barricade bump
                un.MeleeAttack.SpeedTimeCounter = 0;
            if( un.InfectionAttack && un.Body.IsInfected )                                           // Infection too
                un.InfectionAttack.SpeedTimeCounter = 0;
            un.Control.HeroPushedMonsterBarricadeDestroyCount = 0;
        }
        return true;
    }

	public bool CheckPerkRestrictions( bool bApply, Vector2 from, Vector2 to )
	{
        if( UnitType != EUnitType.HERO ) return true;

		Vector2 dif = to - from;
		bool ok = true;

        //if( Body.ToolBoxLevel < 1 )
        //{
        //    if( Map.I.Gaia2[ ( int ) to.x, ( int ) to.y ] )                                                           // No Scroll Reading level yet
        //        if( Map.I.Gaia2[ ( int ) to.x, ( int ) to.y ].TileID == ETileType.SCROLL )
        //        {
        //            if( UnitType == EUnitType.HERO )
        //                if( Quest.CurrentLevel != -1 )
        //                {
        //                    Map.I.ShowMessage( Language.Get( "ERROR_TOOLBOX1" ) );
        //                    return false;
        //                }
        //        }
        //}

        if( UnitType == EUnitType.HERO )
        if( Control.PlatformWalkingLevel < 1 )                                                                        // Not enough platform ability yet
        {
            if( Map.I.Gaia[ ( int ) to.x, ( int ) to.y ] )
            if( Map.I.Gaia[ ( int ) to.x, ( int ) to.y ].TileID == ETileType.TRAP )
                {
                    if( UnitType == EUnitType.HERO )
                        Map.I.ShowMessage( Language.Get( "ERROR_PLATFORMSTEPPING1" ) );
                    return false;
                }
        }

        if( Control.PlatformWalkingLevel >= 1 )
        {
            Unit frU = Map.I.GetUnit( from, ELayerType.GAIA );                                                         // Restricted Platform exit Blockage
            Unit toU = Map.I.GetUnit( to, ELayerType.GAIA );

            if( UnitType == EUnitType.HERO )
            if( frU != null )
            if( frU.TileID == ETileType.TRAP )
            if( toU == null || ( toU.TileID != ETileType.TRAP ) )
                {
                    float cost = 0;
                    string exit = "";

                    if( Map.I.CurrentArea != -1 )
                    if( Map.I.CurrentMoveTrial == Map.I.PlatformEntranceMove ||                                        // Inarea Exit OK
                      ( Map.I.CurArea.AreaTurnCount <= 0 && Map.I.GetPosArea( to ) == -1 ) )           
                    {
                        goto exitOK;
                    }

                    if( Map.I.CurrentArea == -1 )
                    if( Map.I.CurrentMoveTrial == Map.I.PlatformEntranceMove )                                        // Outarea Exit OK                             
                    {
                        goto exitOK;
                    }

                    int cont = 0;                                                                                      // Monster Neighbor Exit
                    for( int d = 0; d < 8; d++ )
                    {
                        Vector2 nb = to + Manager.I.U.DirCord[ ( int ) d ];
                        Unit mn = Map.I.GetUnit( nb, ELayerType.MONSTER );
                        if( mn && mn.ValidMonster ) cont++;
                    }
                
                    cost = Map.I.RM.RMD.MonsterPlatformExitCost;
                    if( Control.PlatformWalkingLevel >= 9 )                                                            
                    if( HeroData.I.PlatformPoints >= cost )
                    if( cont > 0 )
                    {
                        exit = "Monster Neighbor Exit!";
                        goto exitOK;
                    }

                    cost = Map.I.RM.RMD.ReversedPlatformExitCost;
                    EActionType inv = Map.I.InvertActionList[ ( int ) Map.I.PlatformEntranceMove ];
                    if( Control.PlatformWalkingLevel >= 8 )                                                            // Reversed Exit
                    if( HeroData.I.PlatformPoints >= cost )
                    if( Map.I.CurrentMoveTrial == inv )
                    {
                        exit = "Reversed Exit!";
                        goto exitOK;
                    }

                    cost = Map.I.RM.RMD.OrientationPlatformExitCost;
                    if( Control.PlatformWalkingLevel >= 7 )                                                            // Orientation Exit
                    if( HeroData.I.PlatformPoints >= cost )
                    {
                        bool isOriginDiagonal = Map.IsDiagonal( Map.I.PlatformEntranceMove );
                        bool isExitDiagonal   = Map.IsDiagonal( Map.I.CurrentMoveTrial );

                        if( ( int ) Map.I.CurrentMoveTrial >= ( int ) EActionType.MOVE_N )
                        if( ( int ) Map.I.CurrentMoveTrial <= ( int ) EActionType.MOVE_NW )
                        if( isOriginDiagonal && isExitDiagonal ||
                            !isOriginDiagonal && !isExitDiagonal )
                            {
                                exit = "Orientation Exit!";
                                goto exitOK;
                            }
                    }

                    cost = Map.I.RM.RMD.FreePlatformExitCost;
                    if( Control.PlatformWalkingLevel >= 5 )                                                            // Free Exit
                    if( HeroData.I.PlatformPoints >= cost )
                        {
                            exit = "Free Exit!";
                            goto exitOK;                                     
                        }

                    Map.I.ValidateMove = false;
                    Message.RedMessage( "You need to exit the Platform towards\n" +                                    // Exit Failed
                    Map.I.GetDirectionName( Map.I.PlatformEntranceMove ) + " Direction!" );
                    return false;                        

                    exitOK:
                    {
                        if( bApply )
                        {
                            HeroData.I.PlatformPoints -= cost;
                            if( cost > 0 )
                                Message.GreenMessage( exit + "\n-" + cost + " pp" );
                        }
                    }
                }
        }       	

        if( Control.MovementLevel < 2 )                                                                    // Flanking tiles
        {
            if( Map.IsWall( to + new Vector2( 0, 1 ) ) == true )
                if( Map.IsWall( to + new Vector2( 0, -1 ) ) == true ) ok = false;

            if( Map.IsWall( to + new Vector2( 1, 0 ) ) == true )
                if( Map.IsWall( to + new Vector2( -1, 0 ) ) == true ) ok = false;          

            if( ok == false )
            {
                Map.I.ShowMessage( Language.Get( "ERROR_NOFLANKED" ) );
                return false;
            }
        }

        if( Control.MovementLevel < 3 )
        {
            int cont = 0;
            for( int d = 0; d < 8; d++ )
            {
                Vector2 tg = to + Manager.I.U.DirCord[ ( int ) d ];
                if( Map.PtOnMap( Map.I.Tilemap, tg ) )
                    if( Map.IsWall( tg ) == true ) cont++;
            }

            if( cont >= 6 ) ok = false;

            if( ok == false )
            {
                Map.I.ShowMessage( Language.Get( "ERROR_NOTUNNEL" ) );
                return false;
            }
        }

		if( Control.MovementLevel < 5 )
		{
            bool narrow = Map.CheckNarrowPassage( from, to );
            
			if( narrow )
			{
				Map.I.ShowMessage( Language.Get( "ERROR_NARROWPASSAGEFORBIDDEN" ) );
				return false;
			}
		}


        if( Control.MovementLevel < 6 )
        {
            int cont = 0;
            for( int d = 0; d < 8; d++ )
            {
                Vector2 tg = to + Manager.I.U.DirCord[ ( int ) d ];
                if( Map.PtOnMap( Map.I.Tilemap, tg ) )                 
                    if( Map.IsWall( tg ) == true ) cont++;                 
            }

            if( cont >= 7 ) ok = false;

            if( ok == false )
            {
                Map.I.ShowMessage( Language.Get( "ERROR_NODEADEND" ) );
                return false;
            }
        }

		return true;
	}

    public bool Push( bool bApply, Vector2 from, Vector2 to, Unit pusher, bool overarrowpush = false )
    {
        bool dyn = Control.IsDynamicObject();                                         // is dynamic?
        if( TileID != ETileType.BOULDER )  
        if( dyn == false ) return false;

        int stack = G.HS.BoulderPushPower;                                            // DefaultPush Stack Power
        if( dyn )
        {
            if( TileID == ETileType.ALGAE ) return true;
            stack = 30;
            if( Util.Manhattan( from, to ) > 1 ) return true;                         // Jumping objects do not push
        }

        EDirection dir = Util.GetTargetUnitDir( from, to );                           // Movement direction
        Vector2 pt = to;
        List<Unit> ul = new List<Unit>();
        List<Vector2> tgl = new List<Vector2>();

        if( stack >= 50  )                                                            // Boulder Power vault: Above 50 is considered Max power (infinite)
        {
            stack = Sector.TSX * Sector.TSY;
        }
       
        stack++;                                                                      // adds one stack for hero pushing at the end of the line
        bool heroadded = false;
        for( int i = 0; i < stack; i++ )                                              // make a list of all aligned mines to be pushed
        {
            Unit un = Map.GFU( ETileType.MINE, pt );                                  // mine in place?
            if( G.Hero.Pos == pt )
            if( un == null )
            {
                un = G.Hero;                                                          // Add hero
                heroadded = true;
            }
            if( un == null )
                un = Map.I.GetUnit( pt, ELayerType.MONSTER );                         // Add Monster
            if( un && un.Control.CanBePushed == false )
            {
                if( i == 0 ) return true;                                             // first in line cant be pushed, return
                un = null;
            }

            if( un )
            {
                bool add = true;
                if( un.Body.MineType == EMineType.VAULT ||                            // Vaults and tunnel cant be pushed
                    un.Body.MineType == EMineType.TUNNEL )
                    return true;

                if( i == stack - 1 )
                if( un.UnitType != EUnitType.HERO )                                   // Dont include last object since its not the hero
                if( un.ValidMonster == false )
                    add = false;

                if( ul.Contains( un ) ) add = false;                                  // to avoid multiple push

                if( add )
                {
                    ul.Add( un );                                                     // add mine to list
                }
                else break;
            }

            Unit arrow = Map.I.GetUnit( ETileType.ARROW, pt );                        // Arrow push direction change
            if( pusher.TileID == ETileType.BOULDER )
            if( arrow ) dir = arrow.Dir;

            pt = pt + ( Manager.I.U.DirCord[ ( int ) dir ] );                         // updates target position

            if( un == null ) break;                                                   // End of line: jump out
            tgl.Add( pt );                                                            //add target to list     
            if( heroadded ) break;
        }

        Controller.CurentMoveTrial = new Vector2( -1, -1 );                           // new: to avoid Boulder push mine and hero bug
        bool ret = false;
        for( int i = ul.Count - 1; i >= 0; i-- )
        {
            Unit un = ul[ i ];
            Vector2 old = un.Pos;
            bool keeppos = false;
            if( pusher.TileID == ETileType.ARROW )                                                     // arrow do not push if it doesnt block object
            if( overarrowpush || pusher.Pos != un.Pos )
            {
                if( overarrowpush )
                {
                    Vector2 dif2 = to - from;
                    if( Map.I.CheckArrowBlockFromTo( pusher.Pos, pusher.Pos - dif2, un,                // check block
                        true, 5, ( int ) un.Control.ArrowOutLevel ) == false )
                        return false;
                }
                else
                {
                    if( Map.I.CheckArrowBlockFromTo( un.Pos, pusher.Pos, un ) == false )
                        return false;
                }
            }

            if( keeppos == false )
            if( i == ul.Count - 1 )
            if( un.UnitType == EUnitType.HERO || un.ValidMonster )
            if( un.Control.Floor == 0 )                                                      // detects Hero or monster in the front of the line
            if( i >= 1 && un.Pos == tgl[ i - 1 ] ||
                un.Pos == to )
            {
                bool res1 = un.CanMoveFromTo( true, un.Pos, tgl[ i ], pusher );              // and try to push him 
                if( res1 == false )                                                          // if not possible
                {
                    if( un.UnitType == EUnitType.HERO )
                    {
                        Map.I.SetHeroDeathTimer( .1f );                                      // kill hero
                        un.Spr.gameObject.SetActive( false );
                        ret = false;
                        if( ul.Count == 1 ) return false; 
                    }
                    else
                    {
                        Map.Kill( un, true, "Monster Falling" );                             // or kill the monster
                        ret = true;
                    }
                }
            }

            bool res = un.CanMoveFromTo( true, un.Pos, tgl[ i ], pusher );                   // Moves the Object
            
            if( pusher.TileID == ETileType.BOULDER )
            if( pusher.Body.SpikedBoulder )
                Map.I.SetHeroDeathTimer( .1f );                                              // Spiked Boulder Kills hero

            if( res )
            if( G.Hero.Pos == old )
            if( G.Hero.Control.Floor > 0 )                                                   // move suspended Hero
                G.Hero.Control.ApplyMove( G.Hero.Pos, tgl[ i ], pusher );

            if( !res ) { ret = true; }
        }

        if( dyn ) return false;

        return ret;
}

	public void CheckSquashing( Vector2 from, Vector2 to )
	{
		// Check if target is being pushed against an obstacle
		Vector2 dif = to - from;
		Vector2 obs   = to + dif;
		if( Map.PtOnMap( Map.I.Tilemap, to ) == false ) return;

		Unit un = Map.I.Unit[ ( int ) to.x, ( int ) to.y ];
		if( un == null ) return;

		bool ok = un.CheckTerrainMove( to, obs, true, true, true, true, true );
		//if( ok == false && Map.I.PtOnAreaMap( obs ) == false ) return;

		if( !ok )
		{
			 un.Control.IsBeingPushedAgainstObstacle = true;
		}
	}

    public bool SetVariation( int _variation, bool original = false )
    {
        if( TileID == ETileType.BARRICADE     || 
            TileID == ETileType.INACTIVE_HERO || TileID == ETileType.CHECKPOINT )
        {
            int vr = ( int ) TileID + _variation;

            if( TileID == ETileType.BARRICADE )
            {
                if( vr >= ( int ) ETileType.BARRICADE + Map.TotBarricade )
                {
                    //vr = ( int ) ETileType.BARRICADE - 1;                             // Cant grow barricade beyond limit used with old transfer perk
                    //return false;
                }
                if( vr < ( int ) ETileType.BARRICADE  )
                {
                    //vr = ( int ) ETileType.BARRICADE;
                    //return false;
                }
                else
                    Activate( true );
                if( Variation > 9 ) 
                    vr = ( int ) TileID + Map.TotBarricade - 1;
            }

            if( Spr ) Spr.spriteId = vr;
        }

        Variation = _variation;
        if( original ) Body.OriginalVariation = _variation;

        if( TileID == ETileType.ARROW || TileID == ETileType.BOULDER || TileID == ETileType.ORIENTATION ) 
        {
            Dir = (EDirection) _variation;
            int[] aux = new int[] {  0, 1, 129, 257, 256, 255, 127, -1};
            if( Spr ) Spr.spriteId = ( int ) ( TileID + aux[ _variation ] );
        }

        if( TileID == ETileType.DOOR_OPENER || TileID == ETileType.DECOR_TALL ||
            TileID == ETileType.DOOR_SWITCHER || TileID == ETileType.DOOR_CLOSER || TileID == ETileType.DOOR_KNOB ) 
        {
            if( Spr ) Spr.spriteId = ( int ) ( TileID + Variation );
        }
        return true;
    }

    public bool IsDiagonalMove( Vector2 from, Vector2 to, bool checkJumpy = false )
    {
        Vector2 dif = to - from;
        if( dif == new Vector2(  1,  1 ) ) return true;
        if( dif == new Vector2( -1,  1 ) ) return true;
        if( dif == new Vector2(  1, -1 ) ) return true;
        if( dif == new Vector2( -1, -1 ) ) return true;
        if( dif == new Vector2(  0,  1 ) ) return false;
        if( dif == new Vector2(  1,  0 ) ) return false;
        if( dif == new Vector2(  0, -1 ) ) return false;
        if( dif == new Vector2( -1,  0 ) ) return false;
        if( checkJumpy ) Debug.LogError( "Jumpy move!" );
        return false;
    }

    void CheckFireJump( bool bapply, ref Vector2 from, ref Vector2 to )
    {
        if( UnitType != EUnitType.HERO ) return;
        if( Map.I.CurrentArea == -1 ) return;
        if( Map.I.Hero.Body.FireMasterLevel < 5 ) return;
        if( Map.I.CheckArrowBlockFromTo( from, to, this ) == true ) return;

        Vector2 dif = from - to;

        int count = 0;
        for( int i = 0; i < 99; i++ )
        {
            Vector2 pt = to - ( dif * i );

            if( Map.PtOnMap( Map.I.Tilemap, pt ) )
            {
                Unit fire = Map.I.GetUnit( ETileType.FIRE, pt );
                if( fire == null )
                {
                    fire = Map.I.GetUnit( ETileType.BARRICADE, pt );
                    if( fire != null )
                    {
                        if( fire.Body.FireIsOn == false ) fire = null;
                        if( Map.I.Hero.Body.FireMasterLevel < 9 ) fire = null;                // Burning Barricade Jump
                    }
                }

                if( fire != null )
                {
                    count++;
                }
                else
                {
                    if( count >= 1 )
                    {
                        if( Map.I.GetPosArea( pt ) == -1 ) return;

                        Unit un = Map.I.GetUnit( pt, ELayerType.MONSTER, false );
                        if( un )
                        {
                            if( un ) return;
                        }

                        if( bapply )
                            Map.I.PlaceWood( ( int ) pt.x, ( int ) pt.y );

                        to = pt;
                        if( bapply ) Controller.HeroJumped = true;
                        from = pt + dif;
                        return;
                    }
                    else
                        return;
                }
            }
        }
    }
    
    public void UpdateDirection( bool force = false )
    {
        if( UnitType != EUnitType.MONSTER ) return;
        if( TileID == ETileType.BOULDER ) return;
        if( TileID == ETileType.RAFT ) return;
        if( TileID == ETileType.BRAIN ) return;
        if( force == false )
        if( Control.Resting )
        {
            if( Control.FixedRestingDirection != EDirection.NONE )                                                             // Fixed Resting
            {
                RotateTo( Control.FixedRestingDirection );
                return;
            }

            if( Control.RestingDirectionIsSameAsHero )                                                                         // Resting Direction is same as hero´s dir
            {
                RotateTo( G.Hero.Dir );
                return;
            }
        }
        if( Brain.Active() )                                                                                                  // Brain move faces move direction
        if( ValidMonster )
        if( Control.OldPos.x > 0 )
        if( Control.OldPos != Pos )
        if( Control.Resting == false )
        {
            Dir = Util.GetTargetUnitDir( Control.OldPos, Pos );
            Spr.transform.eulerAngles = Util.GetRotationAngleVector( Dir );
            return;
        }

        bool face = false;
        if( TileID == ETileType.DRAGON1 )                                                                                      // Face hero
        if( Control.Resting ) face = true;

        if( TileID == ETileType.HUGGER )
        if( Map.I.CubeDeath )
        if( Map.I.DeathAnimationTimer < 2f ) 
            face = true;

        if( face )
            {
                Quaternion qn = Util.GetRotationToPoint( Spr.transform.position, G.Hero.transform.position );
                Spr.transform.rotation = Quaternion.RotateTowards( Spr.transform.rotation, qn, Time.deltaTime * 10000 );
                return;
            }

        if( Control.IsFlyingUnit ) return;

        Unit it = Map.I.GetUnit( ETileType.ITEM, Pos );                                                           // Unit over shadow. force dir
        if( it && it.TileID == ETileType.ITEM && it.Dir != EDirection.NONE )
        {
            RotateTo( it.Dir );
            return;
        }

        if( TileID == ETileType.ROACH )
        if( Control.Resting == false ) return;

        if( Control.ForcedFrontalMovementFactor > 0 )                                                         // El torero
        {
            RotateTo( Control.ForcedFrontalMovementDir );
            return;
        }

        if( TileID == ETileType.SLIME )
        {
            if( Control.Resting )
            if( Map.I.HasLOS( G.Hero.Pos, Pos ) || Spr.transform.rotation.z == 0 )                                          // LOS rotation for Resting Slime
            {
                float spd = Map.I.RM.RMD.SlimeRotationSpeed;
                if( Spr.transform.rotation.z == 0 ) spd = 360000000;
                Quaternion qn = Util.GetRotationToPoint( Spr.transform.position, G.Hero.transform.position );
                Spr.transform.rotation = Quaternion.RotateTowards( Spr.transform.rotation, qn, Time.deltaTime * spd );
            }      
            return;
        }

        if( CanBeRotated == false ) return;
        if( ValidMonster == false ) return;
        if( Control.Sleeping ) return;

        Vector2 from = Pos;
        Vector2 tg = Map.I.Hero.Pos;

        int ar = Map.I.GetPosArea( Pos );
        if( ar!= -1 && Quest.I.CurLevel.AreaList[ ar ].LockAiTarget.x != -1 )
        {
            tg = Quest.I.CurLevel.AreaList[ ar ].LockAiTarget;
        }

        if( TileID == ETileType.SCARAB )
        {
            float mask = Item.GetNum( ItemType.Res_Mask );                                                      // Masked monster face mask move dir
            if( mask > 0 )
                return;
        }

        EDirection dr = Util.GetRotationToTarget( from, tg );
        RotateTo( dr );
        face = false;
        if( TileID == ETileType.SCARAB ||
            TileID == ETileType.HUGGER )                                                                   // These Units face hero
            face = true;

        if( face )
        {
            Quaternion qn = Util.GetRotationToPoint( 
            Spr.transform.position, G.Hero.transform.position );
            Spr.transform.rotation = Quaternion.RotateTowards(
            Spr.transform.rotation, qn, Time.deltaTime * 10000 );           
        }
        if( TileID == ETileType.SPIDER )        // spider face hero and babies fixed rotation             
        { 
            Body.BabySpriteFolder.transform.eulerAngles = Util.GetRotationAngleVector( Dir );
        }
    }

    public Vector2 GetRelativePosition( EDirection dr, int times )
    {
        int shift = ( int ) ( ( ( int ) Dir ) + dr );
        if( shift >= 8 ) shift -= 8;
        return Manager.I.U.DirCord[ shift ] * times;
    }

    public Vector2 GetRelativePosition( EDirection dr, float times )
    {
        int shift = ( int ) ( ( ( int ) Dir ) + dr );
        if( shift >= 8 ) shift -= 8;
        return Manager.I.U.DirCord[ shift ] * times;
    }

    public EDirection GetRelativeDirection( int times )
    {
        int shift = ( int ) ( Dir + times );

        if( shift < 0 ) shift += 8;
        if( shift < 0 ) shift += 8;
        if( shift >= 8 ) shift -= 8;
        if( shift >= 8 ) shift -= 8;

        return ( EDirection ) ( shift );
    }
    
	public void RotateTo( EDirection _dir, bool updateSprite = true )
	{
        if( _dir != EDirection.NONE )
        if( _dir < 0 ) _dir += 8;
        if( TileID == ETileType.ARROW || TileID == ETileType.BOULDER )
        {
            SetVariation( ( int ) _dir );
            Dir = _dir; // new
            return;
        }

        if( CanBeRotated == false ) return;
        if( TileID == ETileType.TRAIL )
        {
            Spr.spriteId = 57;
            if( _dir == EDirection.NONE )
                Spr.spriteId = 59;
            Dir = _dir;
        }

		if( _dir == EDirection.NONE ) return;
		if( Spr == null ) return; 
		Dir = _dir;

        if( UnitType == EUnitType.HERO ) updateSprite = false;
        if( TileID == ETileType.MINE ) updateSprite = false;
        if( TileID == ETileType.IRON_BALL ) updateSprite = false;
        if( TileID == ETileType.BOUNCING_BALL ) updateSprite = false;

        if( TileID == ETileType.ITEM ) return;


        if( updateSprite )
            Spr.transform.eulerAngles = Util.GetRotationAngleVector( Dir );
	}

	public void UpdateAllAttacks( bool postMove )
	{
        if( Manager.I.GameType == EGameType.FARM ) return;
        if( Manager.I.GameType == EGameType.NAVIGATION ) return;
        if( Body.IsDead ) return;
        if( Control.Sleeping ) return;
        if( Control.Resting ) return;

        if( TileID == ETileType.SCARAB )                                         // Scarab Poison Bite
        if( Body.PoisonBite && Map.I.Hero.Body.IsHealty() == false )
            postMove = false;

		if( postMove && Body.PerformPostMoveAttack == false ) return;
		if( !postMove && Body.PerformPreMoveAttack == false ) return;
        
		if( MeleeAttack ) MeleeAttack.UpdateIt( MeleeAttack.enabled );                            // Melee Attack

        ///if( MeleeAttack == null )// ||
        // MeleeAttack.AttackResult <= 0 )                                                        // Already hit by melee so skip ranged
        if( RangedAttack ) RangedAttack.UpdateIt( RangedAttack.enabled );

        if( InfectionAttack ) InfectionAttack.UpdateIt( InfectionAttack.enabled );

        Attack.AxeAttackList = new List<int>();
        Attack.DuplicatorAttackList = new List<int>();

        if( Attacks != null )                                                                     // Secondary Attacks
        for( int i = Attacks.Count - 1; i >= 0; i-- )
        if ( Attacks[ i ].gameObject.activeSelf )
        {
            Attacks[ i ].UpdateIt( Attacks[ i ].enabled );
        }

        if( UnitType == EUnitType.HERO )
            Spell.ChargeOneTimeSpellUsage();                                                      // Charges Axe only here to allow multiple targets attacked
    }
	
	public bool Rotate( int val )
	{
        if( TileID == ETileType.ARROW )
        {
            EDirection dr = Map.I.RotateDir( Dir, val );
            SetVariation( ( int ) dr );
            return true;
        }

        if( CanBeRotated == false ) return false;
        //Spr.transform.Rotate( 0, 0, -( 45.0f * ( float ) -( int ) Dir ) );
        Dir = Util.RotateDir( ( int ) Dir, val );
        //Spr.transform.Rotate( 0, 0, -( 45.0f * ( float ) ( int ) Dir ) );

        if( UnitType == EUnitType.HERO )
            Body.Shadow.transform.rotation = Spr.transform.rotation;
		return true;
	}
        
	public void Kill( bool UpdateTilemap = false )
	{
        if( SharedObject )
        {
            if( UnitType == EUnitType.GAIA ) Map.I.Gaia[ ( int ) Pos.x, ( int ) Pos.y ] = null;
            if( UnitType == EUnitType.GAIA2 ) Map.I.Gaia2[ ( int ) Pos.x, ( int ) Pos.y ] = null;            
            return;
        }

        if( GS.IsLoading )
        if( TileID == ETileType.ALTAR && Altar.RandomAltar )                                                // dont destroy random altar on load function
            return;

        Activated = false;
        if( Body )
            Body.IsDead = true;

		if( UpdateTilemap )
			if( UseTransTile )
			{
                Map.I.AddTrans( VI.VTOVI( Pos ) );
			}
			else
			{
				Map.I.Tilemap.SetTile( ( int ) Pos.x, ( int ) Pos.y, ( int ) Map.GetTileLayer( TileID ), ( int ) ETileType.NONE );
				Map.I.UpdateTilemap = true;
            }

		if( UI.I.LockedTile == Pos ) UI.I.SetSelection( new Vector2( -1, -1 ) );

		if( Control ) Control.UpdateMoveOrderId();
        if( UnitType == EUnitType.GAIA    ) Map.I.Gaia [ ( int ) Pos.x, ( int ) Pos.y ] = null;
        if( UnitType == EUnitType.GAIA2   ) Map.I.Gaia2[ ( int ) Pos.x, ( int ) Pos.y ] = null;
        if( UnitType == EUnitType.MONSTER && Control.IsFlyingUnit == false )               
            Map.I.Unit [ ( int ) Pos.x, ( int ) Pos.y ] = null;

        Controller.UnitHasBeenKilledWhileMoving = true;
        Map.I.CountRecordTime = true;
        RestLine.RemoveIt( this );                                                       // Destroy line animation
        FinishKilling();

        if( SpawnedCollider )
        {
            if( Collider != null )                                                       // despawn the collider obj
                PoolManager.Pools[ "Pool" ].Despawn( Collider.transform );
            if( CircleCollider != null )                                                 // despawn the circle collider obj
                PoolManager.Pools[ "Pool" ].Despawn( CircleCollider.transform ); 
        }

        if( PoolManager.Pools[ "Pool" ].IsSpawned( transform ) )
            PoolManager.Pools[ "Pool" ].Despawn( transform );
	}
    private void FinishKilling( bool updateSleep = true )
    {
        if( UnitType != EUnitType.MONSTER ) return;  
        RemoveChains();                                                                                                           // Remove Chains from attached objects     

        if( Body && Body.IsDead )
        {
            if( updateSleep )
            if( Body.Hp < Body.TotHp )                                                                                            // Wakes up if damaged
                Control.UpdateSleep( true );    
                
            if( ValidMonster )
            if( GS.IsLoading == false )                                                                              
            {
                UI.I.UpdBeastText = true;
                UpdateWhiteGateClearing();               

                if( Control.IsFlyingUnit == false )                                                                               // Add Outarea Normal Kill to Statistics 
                    {
                        Statistics.AddStats( Statistics.EStatsType.MONSTERSDEATHCOUNT, Body.DeathCountFactor );
                        Item.AddItem( ItemType.Res_Monster_Kill, Body.DeathCountFactor );
                        if( TileID == ETileType.SCARAB ) 
                            Statistics.AddStats( Statistics.EStatsType.SCARABDEATHCOUNT, Body.DeathCountFactor );
                        if( TileID == ETileType.ROACH )
                            Statistics.AddStats( Statistics.EStatsType.ROACHDEATHCOUNT, Body.DeathCountFactor );
                    }
                Mine.UpdateVaultCounter( EMineBonusCnType.KILL_X_MONSTERS );                                                       // vault kiled monsters number increment
            }

            if( Control.IsFlyingUnit )                                                                                             // Add Outarea Flying Kill to Statistics 
            {
                if( Control.Mother == null )
                {
                    if( ValidMonster )
                    if( GS.IsLoading == false )
                    {
                        Statistics.AddStats( Statistics.EStatsType.MONSTERSDEATHCOUNT, Body.DeathCountFactor );
                        Item.AddItem( ItemType.Res_Monster_Kill, Body.DeathCountFactor );
                    }
                    if( TileID == ETileType.FOG )
                    if( Control.BoltEffect )
                        Control.BoltEffect.KillFog( this );
                }

                if( Control.Mother != null )
                {
                    Control.Mother.Body.ChildList.Remove( this );                                                                       // Remove From Mothers Child List
                }

                Map.I.FUnit[ ( int ) Pos.x, ( int ) Pos.y ].Remove( this );
                G.HS.Fly.Remove( this );
            }

            IsFX[ ] fx = transform.gameObject.GetComponentsInChildren<IsFX>();
            for( int i = 0; i < fx.Length; i++ ) fx[ i ].gameObject.transform.parent = Map.I.PoolingFolder.transform;
        }
        if( TileID == ETileType.BRAIN ) 
            Map.I.NumBrains--;
        if( TileID == ETileType.PLAGUE_MONSTER )
        {
            Map.I.Farm.ActivateIndicator( false, ref Map.I.Farm.GrabPlagueIndicator, ref Map.I.Farm.GrabPosition, this, true );
            Map.I.Farm.ActivateIndicator( false, ref Map.I.Farm.NextPlagueIndicator, ref Map.I.Farm.NextPosition, this, true );
        }

        if( Map.I.Droplets.followTargets.Contains( this.Graphic.transform ) )                                                                    // remove from droplest list rafts
            Map.I.Droplets.followTargets.Remove( this.Graphic.transform );
        if( Map.I.Droplets.followTargets.Contains( this.transform ) )                                                                            // remove from droplest list fish
            Map.I.Droplets.followTargets.Remove( this.transform );
    }

	public void UpdateOrbHit()
	{
        Vector2 tg = Pos + Manager.I.U.DirCord[ ( int ) Dir ];
        if( Map.PtOnMap( Map.I.Tilemap, tg ) )
		if(  Map.I.CurrentArea == -1 || Map.I.PtOnAreaMap( tg ) )
		{
            Unit orb = Map.I.GetUnit( ETileType.ORB, tg );
            if( orb ) 
			   {
                orb.StrikeTheOrb( this );
			   }			   
		}
	}

	public bool IsKey( ETileType tl )
	{
		if( tl == ETileType.DOOR_OPENER   ) return true;
		if( tl == ETileType.DOOR_SWITCHER ) return true;
		if( tl == ETileType.DOOR_CLOSER   ) return true;
		return false;
	}
   
	public void StrikeTheOrb( Unit striker, bool forceFX = false )
	{
		//if( TileID != ETileType.ORB ) 
        Map.OrbStruck = false;

        if( striker.UnitType == EUnitType.HERO )
        {
            if( Map.I.CheckArrowBlockFromTo( striker.Pos, Pos, striker ) )                                                   // Arrow block towards orb
                return;
            Map.I.RM.LockWayPointJump = true;
            Map.I.CountRecordTime = true;
        }

		Vector2[] tgl = { new Vector2( 0, 0 ), 
                          new Vector2( 0, 1 ), new Vector2( 0, -1 ), new Vector2( -1, 0 ), new Vector2( 1, 0 ), 
                          new Vector2( 1, 1 ), new Vector2( 1, -1 ), new Vector2( -1, 1 ), new Vector2( -1, -1 )};

		for( int i = 0; i < tgl.Length; i++ )
		{
			Vector2 tg = Pos + tgl[ i ];
            if( Map.PtOnMap( Map.I.Tilemap, tg ) )
			{
                Unit ga = Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ];
                Unit ga2 = Map.I.Gaia2[ ( int ) tg.x, ( int ) tg.y ];
                if( ga2 != null )                                                              
                {
                    ETileType key = ETileType.NONE;
                    if( tg == Pos )
                    if( ga2.TileID == ETileType.DOOR_KNOB ) key = ETileType.DOOR_SWITCHER;
                    if( ga2.TileID == ETileType.DOOR_OPENER ) key = ETileType.DOOR_OPENER;
                    if( ga2.TileID == ETileType.DOOR_SWITCHER ) key = ETileType.DOOR_SWITCHER;
                    if( ga2.TileID == ETileType.DOOR_CLOSER ) key = ETileType.DOOR_CLOSER;
                    if( ga == null || ga.TileID != ETileType.CLOSEDDOOR )                                                        // Keys over closed door are not activated: make puzzles for this
                    if( key != ETileType.NONE )
                        {
                            Map.I.ActivateAllDoorKnobs( key, ga2.Variation, tg, ga2 );                                           // Activate door knobs
                            Map.I.CreateExplosionFX( tg );                                                                       // FX
                        }
                }
			}
		}

        if( Map.OrbStruck || forceFX )
        {
            if( iTween.Count( Body.EffectList[ 2 ].gameObject ) == 0 )
            {
                iTween.PunchPosition( Body.EffectList[ 2 ].gameObject, iTween.Hash(
                "amount", new Vector3( .3f, .3f, 0 ),                   // quanto ele vai "sacudir" em cada eixo
                "time", 0.5f,                                           // duração da animação
                "isLocal", true                                         // ESSENCIAL para afetar apenas o localPosition
             ) );
                Body.TweenTime = .55f;
            }
            MasterAudio.PlaySound3DAtVector3( "Orb Strike", transform.position );
        }
	}
        
    public void OpenDoor()
    {
        List<int> processedDoorId = new List<int>();
        Map.I.Gaia[ ( int ) Pos.x, ( int ) Pos.y ].SetDoorState( Pos, ETileType.DOOR_OPENER , ref processedDoorId, true );
    }
    
    public bool ActivateDoorKnob( ETileType tl, int key )
	{
		if( TileID != ETileType.DOOR_KNOB ) return false;
        if( Variation != key ) return false;

        if( Map.I.Gaia[ ( int ) Pos.x, ( int ) Pos.y ] )
        {
            List<int> processedDoorId = new List<int>();
            Map.I.Gaia[ ( int ) Pos.x, ( int ) Pos.y ].SetDoorState( Pos, tl, ref processedDoorId, false );               // Changes Door State
        }
                                                                                         
        for( int d = 0; d < 8; d++ )
        {
            Vector2 tg = Pos + Manager.I.U.DirCord[ ( int ) d ];                                                          // Arrow
            Unit ar = Map.I.GetUnit( ETileType.ARROW, tg, false );
            if( ar )
            {
                Map.I.PondID = null;
                if( tl == ETileType.DOOR_OPENER )                                                                        // Opens Arrow      
                    ar.Body.SetWorking( false );
                else
                if( tl == ETileType.DOOR_CLOSER )                                                                        // Activates Arrow       
                    ar.Body.SetWorking( true );
                else
                if( tl == ETileType.DOOR_SWITCHER )                                                                      // Switches Arrow
                    ar.Body.SetWorking( !ar.Body.isWorking );
               Map.I. InvalidateFishTargets( ar.Pos );
            }
            Unit fan = Map.I.GetUnit( ETileType.FAN, tg );
            if( fan )
            {
                if( tl == ETileType.DOOR_OPENER )                                                                        // Activates fan      
                    fan.Activate( true );
                else
                if( tl == ETileType.DOOR_CLOSER )                                                                        // Deactivates fan       
                    fan.Activate( false );
                else
                if( tl == ETileType.DOOR_SWITCHER )                                                                      // Switches fan state
                    fan.Activate( !fan.Activated );
            }
        }       
        
        Map.I.Create2DMap = true;
        return true;
	}

	public void SetDoorState( Vector2 tg, ETileType activator, ref List<int> processedDoorId, bool wholemap )
	{
        if( Map.I.CurrentArea != -1 )
        if( Map.I.PtOnAreaMap( tg ) == false ) return;

        if( Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ] == null ) return;

        int doorId = Map.I.GateID[ ( int ) tg.x, ( int ) tg.y ];

		for( int i = 0; i < processedDoorId.Count; i++ )
		if ( processedDoorId[ i ] == doorId ) return;

        ETileType tile = ( ETileType ) Map.I.Gaia[ ( int ) tg.x, ( int ) tg.y ].TileID;
		if( tile != ETileType.CLOSEDDOOR )
		if( tile != ETileType.OPENDOOR   ) return;

		ETileType tgtl = ETileType.NONE;
		if( activator == ETileType.DOOR_CLOSER   ) tgtl = ETileType.CLOSEDDOOR;		
		if( activator == ETileType.DOOR_OPENER   ) tgtl = ETileType.OPENDOOR;
		if( activator == ETileType.DOOR_SWITCHER )
		   {
			if( tile == ETileType.OPENDOOR   ) tgtl = ETileType.CLOSEDDOOR;
			if( tile == ETileType.CLOSEDDOOR ) tgtl = ETileType.OPENDOOR; 	
	       }
        bool ok = false;
        
        Vector2 _p1 = Map.I.P1;   // Check only inside area
        Vector2 _p2 = Map.I.P2;

        if( Map.I.CurrentArea == -1 ) 
        if( Manager.I.GameType == EGameType.CUBES )                                                                    // Check inside the Cube
        {
            _p1 = new Vector2( ( int ) Map.I.RM.HeroSector.Area.xMin, ( int ) Map.I.RM.HeroSector.Area.yMin );
            _p2 = new Vector2( ( int ) Map.I.RM.HeroSector.Area.xMax, ( int ) Map.I.RM.HeroSector.Area.yMax );
        }

        if( wholemap )                                                                                                 // Check in the whole map (Gate Cube)
        {
            _p1 = new Vector2( 0, 0 );
            _p2 = new Vector2( Map.I.Tilemap.width, Map.I.Tilemap.height );
        }
        bool sound = false;
        for( int y = ( int ) _p1.y; y < _p2.y; y++ )
        for( int x = ( int ) _p1.x; x < _p2.x; x++ )
        {
             ok = OpenDoorTiles( x, y, doorId, ref processedDoorId, tgtl );
             if( ok ) sound = true;
        }
        
        if( sound )
            MasterAudio.PlaySound3DAtVector3( "Open Gate", transform.position );                                      // Open door sound fx
	}

    public bool OpenDoorTiles( int x, int y, int doorId, ref List<int> processedDoorId, ETileType tgtl )
    {
        bool ok = false;
        Unit ga = Map.I.GetUnit( new Vector2( x, y ), ELayerType.GAIA );

        if( ga )
        if( ga.TileID == ETileType.OPENDOOR ||
            ga.TileID == ETileType.CLOSEDDOOR )
        if( doorId == Map.I.GateID[ x, y ] )
        {
            int oldDoorId = Map.I.GateID[ x, y ];
            Map.I.Gaia[ x, y ].Copy( Map.I.SharedObjectsList[ ( int ) tgtl ], false, false, false );
            Map.I.Gaia[ x, y ].TileID = tgtl;
            Map.I.AddTrans( new VI( x, y ), true );  
            Map.I.UpdateTilemap = true;
            Map.I.GateID[ x, ( int ) y ] = oldDoorId;
            ok = true;
            processedDoorId.Add( doorId );
            if( ga.Collider )
            {
                if( tgtl == ETileType.CLOSEDDOOR )
                    ga.Collider.enabled = true;                                                                       // updates collider
                else
                if( tgtl == ETileType.OPENDOOR )
                    ga.Collider.enabled = false;
            }

            if( G.Hero.Pos == new Vector2( x, y ) )                                                                   // Updates hero floor
            {
                G.Hero.Control.Floor = Controller.GetUnitFloor( G.Hero.Control.OldPos, G.Hero.Pos, G.Hero );
            }
            Unit mn = Map.I.GetUnit( new Vector2( x, y ), ELayerType.MONSTER );                                       // Updates monster floor
            if( mn && mn.ValidMonster )
                mn.Control.Floor = Controller.GetUnitFloor( mn.Control.OldPos, mn.Pos, mn );
            
            if( Sector.GetPosSectorType( new Vector2( x, y ) ) != Sector.ESectorType.GATES )
                Map.I.CreateExplosionFX( new Vector2( x, y ), "Fire Explosion", "" );                                 // Explosion FX
        }
        return ok;
    }
    
    public float GetMonsterOriginalTotHP( ETileType tile )
    {
        Unit prefabUnit = Map.I.GetUnitPrefab( tile );
        if( prefabUnit != null )
        {
            return prefabUnit.Body.TotHp;
        }
        else Debug.LogError( "Bad Prefab" );
        return -1;
    }

	public void UpdateLevelingData()
	{
        if( UnitType == EUnitType.HERO )
            Body.Level = 1 + ( ( int ) ( Body.Stars + 0 ) / 5 );

        float stars = Body.Stars;
        while( stars >= 5 ) stars -= 5;

        if( Body )
        {
            float hpperc = ( Body.HpPerLevel * ( Body.Level - 1 ) ) + Body.HpPerStar * ( stars );
            Body.TotHp += Util.Percent( hpperc, Body.TotHp );
           // Body.Hp = Body.TotHp - Body.DamageTaken;

            Body.TotalMeleeShield   = Body.BaseMeleeShield   + ( Body.MeleeShieldPerLevel   * ( Body.Level - 1 ) ) + Body.MeleeShieldPerStar   * ( stars );
            Body.TotalMissileShield = Body.BaseMissileShield + ( Body.MissileShieldPerLevel * ( Body.Level - 1 ) ) + Body.MissileShieldPerStar * ( stars );
            Body.TotalMagicShield   = Body.BaseMagicShield   + ( Body.MagicShieldPerLevel   * ( Body.Level - 1 ) ) + Body.MagicShieldPerStar   * ( stars );

			if( HeroData.I == null ) HeroData.I = GameObject.Find("Hero Data").GetComponent<HeroData>();
        }

        if( MeleeAttack )
        {
            float bs = MeleeAttack.BaseDamage;
            if( UnitType == EUnitType.HERO )
            if( Map.I.RM.RMD.RTBaseHeroMeleeAttackDamage != -1 ) bs = Map.I.RM.RMD.RTBaseHeroMeleeAttackDamage;
            MeleeAttack.TotalDamage = bs + ( MeleeAttack.DamagePerLevel * ( Body.Level - 1 ) ) + 
                                                MeleeAttack.DamagePerStar * ( stars ) + MeleeAttack.BonusDamage;
        }

        if( RangedAttack )
        {
            float bs = RangedAttack.BaseDamage;
            if( UnitType == EUnitType.HERO )
            if( Map.I.RM.RMD.RTBaseHeroRangedAttackDamage != -1 ) bs = Map.I.RM.RMD.RTBaseHeroRangedAttackDamage;
            RangedAttack.TotalDamage = bs + ( RangedAttack.DamagePerLevel * ( Body.Level - 1 ) ) +
                                                 RangedAttack.DamagePerStar * ( stars ) + RangedAttack.BonusDamage;
			RangedAttack.TotalRange = RangedAttack.BaseRange + Body.DexterityLevel;
        }

        if( InfectionAttack )
        {
            float bs = InfectionAttack.BaseDamage;
            InfectionAttack.TotalDamage = bs + MeleeAttack.BonusDamage;
        }

        //if( Attacks != null )
        //    for( int i = 0; i < Attacks.Count; i++ )
        //    {
        //        float bs = Attacks[ i ].BaseDamage;
        //        if( UnitType == EUnitType.HERO )
        //            if( Map.I.RM.RMD.RTBaseHeroThrowingAxeAttackDamage != -1 ) bs = Map.I.RM.RMD.RTBaseHeroThrowingAxeAttackDamage;
        //        Attacks[ i ].TotalDamage = bs + ( Attacks[ i ].DamagePerLevel * ( Body.Level - 1 ) ) +
        //                                          Attacks[ i ].DamagePerStar * ( stars ) + Attacks[ i ].BonusDamage;
        //        Attacks[ i ].TotalRange = Sector.TSX;
        //    }

        //if( BeeHiveAttack )
        //{
        //    BeeHiveAttack.TotalDamage = BeeHiveAttack.BaseDamage + ( BeeHiveAttack.DamagePerLevel * ( Body.Level - 1 ) ) +
        //                                         BeeHiveAttack.DamagePerStar * ( stars ) + BeeHiveAttack.BonusDamage;
        //    BeeHiveAttack.TotalRange = BeeHiveAttack.BaseRange + Body.DexterityLevel;
        //}

        if( UnitType == EUnitType.HERO )                                      // Hint Bonus Movement
        {
            if( SectorHint.GetHintBonus( "MOTUSAGILE", 0 ) > 0 )
            {
                Control.MovementLevel += 2;
            }

            UpdateLevelAdjustment();
            UpdateInflation();
            if( Map.I.CurrentArea != -1 )
            if( Map.I.CurArea.FreeBomb > 0 )                                   // Theres a free bomb (Monsters stuck) so give a temp level for it to be used
            if( Body.WallDestroyerLevel < 1 ) 
                Body.WallDestroyerLevel = 1;
        }

        if( UnitType == EUnitType.MONSTER )
        if( Body )
        {
            if( LevelTxt )
            {
                LevelTxt.text = "" + ( ( int ) Body.Level ).ToString( "0." );
                if( Body.Level == 1 ) LevelTxt.text = "";
            }
            Body.Hp = Body.TotHp;
            return;
        }
	}

    public void UpdateLevelAdjustment()
    {
        if( Control.ArrowInLevel >= 1 || Control.ArrowOutLevel >= 1 )
            Control.ArrowWalkingLevel = 1;

        if( Body.FreshAttackLevel >= 1 || Body.RiskyAttackLevel >= 1 || 
            Body.MortalJumpLevel >= 1  || Body.OpenFieldAtttackLevel >= 1 )
            Body.AgilityLevel += 1;

        if( Body.FirePowerLevel >= 1 || Body.FireSpreadLevel >= 1        ||
            Body.FireWoodNeeded >= 1 || Body.OutsideFireWoodAllowed >= 1 ||
            Body.FireStarBonus >= 1 )
            Body.FireMasterLevel += 1;

        if( Body.DestroyBarricadeLevel >= 1 || Body.OutAreaBurningBarricadeDestroyBonus >= 1 || Body.BarricadeForRune >= 1 )
            Control.BarricadeFighterLevel += 1;

        if( Body.CollectorLevel >= 1 || Body.ResourcePersistance >= 1 || Body.LooterLevel >= 1 )
            Body.EconomyLevel += 1;

        if( Control.SlayerAngle >= 1 || Control.SlayerMaxHP >= 1 || Control.DragonDisguiseLevel >= 1 ||
            Control.DragonBonusDropLevel >= 1 || Control.DragonBarricadeProtection >= 1 ) 
            Control.SlayerLevel = 1;

        if( Control.PlatformWalkingLevel < 1 )
        if( Control.PlatformSteps >= 1 || Body.FreePlatformExit >= 1 )
            Control.PlatformWalkingLevel += 1;
    }

    public void UpdateInflation()
    {
        int oldfire = ( int ) Body.FireWoodNeeded;
        Body.FireWoodNeeded -= ( int ) Map.I.RM.FirewoodNeededInflation;
        if( Body.FireWoodNeeded < 0 ) Body.FireWoodNeeded = 0;

        if( oldfire > ( int ) Body.FireWoodNeeded ) 
            Message.RedMessage("Fire wood\nNeeded Increased.");

        int oldbar = ( int ) Body.DestroyBarricadeLevel;
        Body.DestroyBarricadeLevel -= ( int ) Map.I.RM.BarricadeDestroyInflation;
        if( Body.DestroyBarricadeLevel < 0 ) Body.DestroyBarricadeLevel = 0;

        if( oldbar < ( int ) Body.DestroyBarricadeLevel )
            Message.RedMessage( "Barricade Destruction\nLevel Decreased." );
    }
    public float GetPerkLevelLimit( EPerkType pk )
    {
        return UI.I.PerkList[ ( int ) pk ].Sprite.Length - 1;
    }  

    public float GetPerkVar( EPerkType pk = EPerkType.NONE )
    {
        if( pk == EPerkType.MOVEMENTLEVEL ) return Control.MovementLevel;
        if( pk == EPerkType.MELEESHIELD ) return Body.MeleeShieldLevel;
        if( pk == EPerkType.ARROWWALKING ) return Control.ArrowWalkingLevel;
        if( pk == EPerkType.MONSTERCORNERING ) return Control.MonsterCorneringLevel;
        if( pk == EPerkType.PLATFORMWALKING ) return Control.PlatformWalkingLevel;
        if( pk == EPerkType.MONSTERPUSH ) return Control.MonsterPushLevel;
        if( pk == EPerkType.SNEAKINGLEVEL ) return Control.SneakingLevel;
        if( pk == EPerkType.SPRINTERLEVEL ) return Control.SprinterLevel;
        if( pk == EPerkType.BARRICADEFIGHTERLEVEL ) return Control.BarricadeFighterLevel;
        if( pk == EPerkType.EVASIONLEVEL ) return Control.EvasionLevel;
        if( pk == EPerkType.PERFECTIONISTLEVEL ) return Control.PerfectionistLevel;
        if( pk == EPerkType.SCAVENGERLEVEL ) return Control.ScavengerLevel;
        if( pk == EPerkType.ARROWFIGHTERLEVEL ) return Control.ArrowFighterLevel;
        if( pk == EPerkType.ARROWINLEVEL ) return Control.ArrowInLevel;
        if( pk == EPerkType.ARROWOUTLEVEL ) return Control.ArrowOutLevel;
        if( pk == EPerkType.OVERBARRICADESCOUT ) return Control.OverBarricadeScoutLevel;
        if( pk == EPerkType.SHOWRESOURCECHANCE ) return Control.ShowResourceChance;
        if( pk == EPerkType.SHOWRESOURCENEIGHBORSCHANCE ) return Control.ShowResourceNeighborsChance;
        if( pk == EPerkType.MELEEATTACK ) return Body.MeleeAttackLevel;
        if( pk == EPerkType.RANGEDATTACK ) return Body.RangedAttackLevel;
        if( pk == EPerkType.ORBSTRIKER ) return Body.OrbStrikerLevel;
        if( pk == EPerkType.COOPERATION ) return Body.CooperationLevel;
        if( pk == EPerkType.DAMAGESURPLUS ) return Body.DamageSurplusLevel;
        if( pk == EPerkType.TOOLBOX ) return Body.ToolBoxLevel;
        if( pk == EPerkType.MEMORY ) return Body.MemoryLevel;
        if( pk == EPerkType.AMBUSHER ) return Body.AmbusherLevel;
        if( pk == EPerkType.DEXTERITY ) return Body.DexterityLevel;
        if( pk == EPerkType.AGILITYLEVEL ) return Body.AgilityLevel;
        if( pk == EPerkType.BERSERKLEVEL ) return Body.BerserkLevel;
        if( pk == EPerkType.FIREMASTERLEVEL ) return Body.FireMasterLevel;
        if( pk == EPerkType.BEEHIVETHROWERLEVEL ) return Body.BeeHiveThrowerLevel;
        if( pk == EPerkType.PSYCHICLEVEL ) return Body.PsychicLevel;
        if( pk == EPerkType.LOOTER ) return Body.LooterLevel;
        if( pk == EPerkType.PROSPECTOR ) return Body.ProspectorLevel;
        if( pk == EPerkType.RISKYATTACKLEVEL ) return Body.RiskyAttackLevel;
        if( pk == EPerkType.FRESHATTACKLEVEL ) return Body.FreshAttackLevel;
        if( pk == EPerkType.MORTALJUMPLEVEL ) return Body.MortalJumpLevel;
        if( pk == EPerkType.OPENFIELDATTACKLEVEL ) return Body.OpenFieldAtttackLevel;
        if( pk == EPerkType.BASETHREATDURATION ) return Body.BaseThreatDuration;
        if( pk == EPerkType.BASEFREEEXITHPLIMIT ) return Body.BaseFreeExitHPLimit;
        if( pk == EPerkType.FIREPOWERLEVEL ) return Body.FirePowerLevel;
        if( pk == EPerkType.FIRESPREADLEVEL ) return Body.FireSpreadLevel;
        if( pk == EPerkType.FIREWOODNEEDED ) return Body.FireWoodNeeded;
        if( pk == EPerkType.OUTSIDEFIREWOODALLOWED ) return Body.OutsideFireWoodAllowed;
        if( pk == EPerkType.DESTROYBARRICADE ) return Body.DestroyBarricadeLevel;
        if( pk == EPerkType.FREEPLATFORMEXIT ) return Body.FreePlatformExit;
        if( pk == EPerkType.OUTAREABURNINGBARRICADEDESTROYBONUS ) return Body.OutAreaBurningBarricadeDestroyBonus;
        if( pk == EPerkType.RICOCHETLEVEL ) return RangedAttack.RicochetLevel;
        if( pk == EPerkType.BARRICADEFORRUNE ) return Body.BarricadeForRune;
        if( pk == EPerkType.SCARYLEVEL ) return Body.ScaryLevel;
        if( pk == EPerkType.HERONEIGHBORTOUCHADDER ) return Body.HeroNeighborTouchAdder;
        if( pk == EPerkType.FIRESTARBONUS ) return Body.FireStarBonus;
        if( pk == EPerkType.COLLECTOR ) return Body.CollectorLevel;
        if( pk == EPerkType.RESOURCEPERSISTANCE ) return Body.ResourcePersistance;
        if( pk == EPerkType.RTMELEEATTACKSPEED ) return Body.RealtimeMeleeAttSpeed;
        if( pk == EPerkType.RTRANGEDATTACKSPEED ) return Body.RealtimeRangedAttSpeed;
        if( pk == EPerkType.MIRE ) return Control.MireLevel;
        if( pk == EPerkType.MINING ) return Body.MiningLevel;
        if( pk == EPerkType.RESTDISTANCE ) return Control.RestingLevel;
        if( pk == EPerkType.SLAYERLEVEL ) return Control.SlayerLevel;
        if( pk == EPerkType.FLYINGTARGETTING ) return Control.FlyingTargetting;
        if( pk == EPerkType.SLAYERANGLE ) return Control.SlayerAngle;
        if( pk == EPerkType.SLAYERMAXHP ) return Control.SlayerMaxHP;
        if( pk == EPerkType.DISGUISE ) return Control.DragonDisguiseLevel;
        if( pk == EPerkType.DRAGONBONUSDROP ) return Control.DragonBonusDropLevel;
        if( pk == EPerkType.DRAGONBARRICADEPROTECTION ) return Control.DragonBarricadeProtection;
        if( pk == EPerkType.PLATFORM_STEPS ) return Control.PlatformSteps;
        if( pk == EPerkType.FISHING ) return Body.FishingLevel;
        if( pk == EPerkType.FISHING_1 ) return Body.Fishing_1;
        if( pk == EPerkType.FISHING_2 ) return Body.Fishing_2;
        if( pk == EPerkType.FISHING_3 ) return Body.Fishing_3;
        if( pk == EPerkType.FISHING_4 ) return Body.Fishing_4;

        return -1;
    }

    public bool CheckLevelLimits( EPerkType pk, int lim = 0 )
    {
        if( lim <= 0 )
            lim = ( int ) GetPerkLevelLimit( pk );
        float vari = GetPerkVar( pk );
        if( vari >= lim )
        {
            return true;
        }
        return false;
    }
    
    public void UpdateWhiteGateClearing()
    {       
        int groupcont = 0;
        if( Control.WhiteGateGroup.x == -1 ) return;

        for( int i = 0; i < Map.I.RM.HeroSector.MoveOrder.Count; i++ )
        {
            if( Map.I.RM.HeroSector.MoveOrder[ i ].ValidMonster )
            if( Map.I.RM.HeroSector.MoveOrder[ i ].Control.WhiteGateGroup == Control.WhiteGateGroup ) groupcont++;
        }

        for( int i = 0; i < G.HS.Fly.Count; i++ )
        {
            if( G.HS.Fly[ i ].ValidMonster )
            if( G.HS.Fly[ i ].Control.WhiteGateGroup == Control.WhiteGateGroup ) groupcont++;
        }

        if( groupcont > 1 ) return;
        Map.I.ClearRoomDoorAtPos( Control.WhiteGateGroup );
    }
    public static float AngleInRad( Vector2 vec1, Vector2 vec2 )
    {
        return Mathf.Atan2( vec2.y - vec1.y, vec2.x - vec1.x );
    }

    //This returns the angle in degrees
    public static float AngleInDeg( Vector2 vec1, Vector2 vec2 )
    {
        return AngleInRad( vec1, vec2 ) * 180 / Mathf.PI;
    }

    public EZone GetPositionZone( Vector2 tg, ref float ang, EDirection customDir = EDirection.NONE )
    {
        if( tg == Pos ) return EZone.NONE;
        Vector2 vec2 = Pos;
        Vector2 diference = Pos - tg;
        float sign = ( vec2.y < tg.y ) ? -1.0f : 1.0f;
        float angle = Vector2.Angle( Vector2.right, diference ) * sign;
        angle -= Spr.transform.eulerAngles.z;
        angle = AngleInDeg( vec2, tg ) - Spr.transform.eulerAngles.z - 90;

        if( customDir != EDirection.NONE )
        {
            switch( customDir )
            {
                case EDirection.N: angle = 0; break;
                case EDirection.NE: angle = -45; break;
                case EDirection.E: angle = -90; break;
                case EDirection.SE: angle = -135; break;
                case EDirection.S: angle = 180; break;
                case EDirection.SW: angle = 135; break;
                case EDirection.W: angle = 90; break;
                case EDirection.NW: angle = 45; break;
            }
        }
        angle = Mathf.RoundToInt( angle );
        if( angle < 0 ) angle += 360;
        if( angle < 0 ) angle += 360;

        if( angle >= 360 ) angle -= 360;
        if( angle >= 360 ) angle -= 360;

        ang = angle;

        if( angle == 0 ) return EZone.Front;
        if( angle > 0 && angle < 45 ) return EZone.FrontLeft;
        if( angle >= 45 && angle < 90 ) return EZone.FrontSideLeft;
        if( angle == 90 ) return EZone.Left;
        if( angle > 90 && angle < 135 ) return EZone.BackSideLeft;
        if( angle >= 135 && angle < 180 ) return EZone.BackLeft;
        if( angle == 180 ) return EZone.Back;

        if( angle < 360 && angle > 315 ) return EZone.FrontRight;
        if( angle <= 315 && angle > 270 ) return EZone.FrontSideRight;
        if( angle == 270 ) return EZone.Right;
        if( angle < 270 && angle > 225 ) return EZone.BackSideRight;
        if( angle <= 225 && angle > 180 ) return EZone.BackRight;
        Debug.LogError("Bad zone");
        return EZone.NONE;
    }

    public void UseAbility()
    {
        switch( UI.I.SelectedPerk )
        {
            case EPerkType.WALLDESTROYER:
            Map.I.DestroyWall();
            break;

            case EPerkType.SNEAKINGLEVEL:
            Map.I.WakeUpAllMonsters();
            break;
        }
    }

    public int GetLevel( EPerkType type )
    {
        float var = -1;
        switch( type  )
        {
            case EPerkType.BERSERKLEVEL:
            var = Body.BerserkLevel; 
            break;

            case EPerkType.MELEEATTACK:
            var = Body.MeleeAttackLevel;
            break;
            case EPerkType.RANGEDATTACK:
            var = Body.RangedAttackLevel;
            break;
        }

        if( var == -1 ) Debug.LogError( "GetLevel Bug " + type );
        return Mathf.CeilToInt( var );
    }

    public void Activate( bool _activated )
    {
        Activated = _activated;
        if( TileID == ETileType.FAN )
        {
            Variation = 0;
            if( _activated == true )
                Variation = 1;                                                       // be careful, only use Activate() for fan to activate it from orb key. Variation = 1 means activated by orb
            Body.EffectList[ 0 ].SetActive( Activated );
            return;
        }
        Spr.gameObject.SetActive( _activated );
        if( LevelTxt )
            LevelTxt.gameObject.SetActive( _activated );
        
        if( TileID == ETileType.HERB )
        {
            if( _activated == false ) Body.Sprite2.gameObject.SetActive( false );
            else Map.I.InitHerbGraphics( this );
        }
        if( _activated == false )
            if( Body.FireIsOn )
            {
                Util.SetActiveRecursively( Body.EffectList[ 0 ].gameObject, false );  // turn off fire effect
            }
        if( Control )
            Control.RespawnTimeCount = 0;
    }
    public void UpdateText()
    {
        if( LevelTxt == null ) return;
        if( TileID == ETileType.WASP ) return;
        if( TileID == ETileType.FISHING_POLE ) return;

        bool mouse = false;
        if( Cursor.visible && Pos == new Vector2( Map.I.Mtx, Map.I.Mty ) ) mouse = true;
        LevelTxt.color = Color.white;
       
        if( TileID == ETileType.QUEROQUERO )
            SetLevelText( "" + Control.RealtimeSpeed );

        if( TileID == ETileType.ITEM )
        {
            if( LevelTxt )
            {
                if( Body.StackAmount != Body.AuxStackAmount )
                if( Body.ResourceOperation == EResourceOperation.SET )
                    LevelTxt.text = "=" + Body.StackAmount.ToString( "0.#" );
                else
                    LevelTxt.text = "x" + Body.StackAmount.ToString( "0.#" );

                if( Dir != EDirection.NONE )                                                        // item shadow
                {
                    if( Md.ResourceType == ItemType.NONE )
                    {
                        LevelTxt.text = "";
                        Variation = ( int ) Md.ResourceType;
                    }
                }

                string add = "";
                LevelTxt.gameObject.SetActive( true );
                if( Body.StackAmount == 1 ) 
                    LevelTxt.gameObject.SetActive( false );
                Body.ItemMiniDome.gameObject.SetActive( false );

                float duration = Body.GetMiniDomeTotTime();

                if( duration > 0 )
                if( G.Hero.Pos != Pos )
                {
                    LevelTxt.gameObject.SetActive( true );                                                     // Item Mini Dome timer
                    Body.ItemMiniDome.gameObject.SetActive( true );              
                    
                    if( mouse )
                    if( Body.MiniDomeTimerCounter > 0 )
                        add += "  (" + Util.ToSTime( Body.MiniDomeTimerCounter ) + ")";                        // Time text update      
                    else
                        add += "  (" + Util.ToSTime( Body.GetMiniDomeTotTime() ) + ")";

                    float pers = Body.ResourcePersistTotalSteps - Body.ResourcePersistStepCount - 1;           // persistance indicator 
                    if( mouse && pers >= 1 )
                        add += "\nUses: +" + pers; 

                    if( Body.MiniDomeTimerCounter == -1 ) 
                    {
                        Body.ItemMiniDome.color = new Color( 1, 1, 1, 1 );
                    }
                    else
                    {
                        if( Body.MiniDomeTimerCounter == -2 )                                                  // Time is up
                        {
                            add = "";
                            float a = Body.ItemMiniDome.color.a - ( Time.deltaTime * 3 );
                            Body.ItemMiniDome.color = new Color( 1, 1, 1, a );
                            if( Body.ItemMiniDome.color.a <= 0 )
                                Body.ItemMiniDome.gameObject.SetActive( false );
                            Body.ItemMiniDome.transform.eulerAngles = new Vector3( 0, 0, 0 );
                        }
                        else
                        {
                            if( Map.I.IsPaused() == false )
                            {
                                Body.MiniDomeTimerCounter -= Time.deltaTime;                                   // Time Decrement                            
                                float startRotation = Body.ItemMiniDome.transform.eulerAngles.z;
                                float endRotation = startRotation + 360.0f;
                                float zRotation = Mathf.Lerp( startRotation,                                   // Mini Dome Rotation Animation
                                endRotation, Time.deltaTime / duration ) % 360.0f;
                                Body.ItemMiniDome.transform.eulerAngles = new Vector3( 0, 0, zRotation );
                            }

                            if( Body.MiniDomeTimerCounter <= 0 )                                               // Minidome times is up
                            {
                                Body.MiniDomeTimerCounter = -2;
                                MasterAudio.PlaySound3DAtVector3( "Save Game", Pos, 1, .9f );
                            }
                        }
                    }
                }

                Body.Sprite4.gameObject.SetActive( false );
                if( Body.ResourceWasteTotalTime > 0 )
                {
                    if( Body.ResourceWasteTimeCounter == -2 )
                    {
                        Item it = G.GIT( Variation );
                        it.TempCount -= Body.StackAmount;
                        ResourceIndicator.UpdateGrid = true;
                        if( it.TempCount < 0 ) it.TempCount = 0;
                        Map.I.KillList.Add( this ); 
                    }

                    Body.Sprite4.gameObject.SetActive( true );
                    if( mouse )
                        add += "  (" + Util.ToSTime( Body.ResourceWasteTotalTime ) + ")";              // Time text update      

                    if( Body.ResourceWasteTimeCounter > 0 )
                    {
                        Body.ResourceWasteTimeCounter -= Time.deltaTime;                                   // Time Decrement                            
                        float startRotation = Body.Sprite4.transform.eulerAngles.z;
                        float endRotation = startRotation + 360.0f;
                        float zRotation = Mathf.Lerp( startRotation,                                       // Cage Rotation Animation
                        endRotation, Time.deltaTime / Body.ResourceWasteTotalTime ) % 360.0f;
                        Body.Sprite4.transform.eulerAngles = new Vector3( 0, 0, zRotation );  
                    }

                    if( Body.ResourceWasteTimeCounter != -1 )
                    if( Body.ResourceWasteTimeCounter <= 0 )                                               // Cage times is up
                    {
                        Body.ResourceWasteTimeCounter = -2;
                        MasterAudio.PlaySound3DAtVector3( "Save Game", Pos, 1, .9f );
                    }
                }

                if( add != "" ) LevelTxt.text += add;

                if( Manager.I.GameType == EGameType.CUBES )
                if( G.HS.BagExtraBonusItemID != -1 )
                if( Variation == ( int ) G.HS.BagExtraBonusItemID )                                        // Extra bonus due to vault
                {
                    LevelTxt.text = Body.StackAmount.ToString( "+#;-#;0" );
                    float bag = Item.GetNum( ItemType.Res_Mining_Bag );
                    if( bag != 0 )
                    {
                        LevelTxt.text = Body.StackAmount.ToString( "0." );
                        LevelTxt.text += "+" + ( bag * G.HS.BagExtraBonusAmount );
                    }
                    LevelTxt.color = Color.green;
                    LevelTxt.gameObject.SetActive( true );
                }

                if( Variation == ( int ) ItemType.Res_Mask )                                           // Unlimited Mask
                if( Body.StackAmount >= 100 )
                    LevelTxt.text = "";
                if( Variation == ( int ) ItemType.Res_Mushroom )                                       // Mushroom
                    LevelTxt.text = "";
                if( Item.IsPlagueMonster(  Variation ) )                                               // Plague monster
                    LevelTxt.text = "";
                if( Manager.I.GameType == EGameType.CUBES ) 
                if( Body.IsChest() )                                                                   // Chest
                    LevelTxt.text = "";

                float amt = Item.GetNum( ( ItemType ) Variation );
                if( Map.I.GetUnit( ETileType.CLOSEDDOOR, Pos ) != null )
                {
                    LevelTxt.color = Color.green;
                    if( amt < Body.StackAmount )
                        LevelTxt.color = Color.red;
                }
                if( Manager.I.GameType != EGameType.FARM )
                if( Dir == EDirection.NONE )
                {
                    //if( G.Hero.Control.TurnTime > 5f )
                    //if( Item.IsPlagueMonster( Variation ) == false )
                    //    LevelTxt.text += "\n(Stock: " + amt.ToString( "0.#" ) + ")";
                    if( Body.IsChest() )                                                               // Chest Contents Text info
                    {
                        if( G.Hero.GetFront() == Pos )
                        {
                            string snd = ""; string txt = "Chest Contents:\n";
                            Chests.GetChestText( this, ref txt, ref snd, this, false );
                            UI.I.SetBigMessage( txt, Color.green, 2f, -1, -1, 60, .1f, 1 );
                        }
                        if( Body.ChestLevel == 2 )
                            Spr.spriteId = 90;
                        if( Body.ChestLevel >= 3 )
                        {
                            Body.Sprite2.gameObject.SetActive( true );
                            Spr.spriteId = 91;
                            float rot = Body.Sprite2.transform.eulerAngles.z;
                            rot += Time.deltaTime * 36;
                            if( rot > 360 ) rot -= 360;
                            Body.Sprite2.transform.eulerAngles = new Vector3( 0, 0, rot );
                            if( Body.ChestLevel >= 4 )
                            {
                                Spr.scale = new Vector3( 1.7f, 1.7f, 1 );
                                Body.Sprite2.scale = new Vector3( 5f, 5f, 1 );
                            }
                        }
                    }
                }

                if( Variation == ( int ) ItemType.Res_ForcedZoom )                                     // Forced Zoom
                    LevelTxt.text = "Zoom Lock!";
            }
            return;
        }
    }

    public Vector2 GetFront()
    {
        Vector2 f = Pos + Manager.I.U.DirCord[ ( int ) Dir ];
        return f;
    }

    public void UpdateDomePrice( float forceprice = -1 )
    {
        Artifact ar = Quest.I.GetArtifactInPos( Pos );

        if( ar )
        {
            //PriceTag.CostResource_1 = ar.CostResource_1;
            PriceTag.CostValue_1 = ar.CostValue_1;
            if( forceprice != -1 )
                PriceTag.CostValue_1 = forceprice;
            //PriceTag.CostResource_2 = ar.CostResource_2;
            PriceTag.CostValue_2 = ar.CostValue_2;
            if( PriceTag.CostValue_1 > 0 )
                if( !PriceTag.gameObject.activeSelf )
                    PriceTag.gameObject.SetActive( true );

            PriceTag.Price_1Text.text = "x" + PriceTag.CostValue_1;
        }
    }
    public void UpdateRightText()
    {
        if( TileID == ETileType.FOG ) return;
        if( TileID == ETileType.FISH ) return; 
        if( TileID == ETileType.MINE ) return;
        if( TileID == ETileType.SAVEGAME ) return;
        if( TileID == ETileType.SECRET ) return;
        string txt = "";
        Color col = Color.white;
        if( RightText ) RightText.color = Color.white;

        if( Control && Control.Resting )                                                   // move to another function
            {
                //    int man = ( int ) Control.CalculateRestingTiles();     
                if( Control.RestingRadiusSprite )
                {
                    Control.RestingRadiusSprite.gameObject.SetActive( true );
                    int rad = 1 + ( int ) ( ( Map.I.RM.RMD.BaseRestingDistance + Control.BaseRestDistance ) * 2 );
                    if( Map.I.TurnFrameCount == 1 )
                        Control.RestingRadiusSprite.gameObject.transform.localScale = new Vector3( rad, rad, 1 );
                }

                if( RightText != null )
                {
                    if( Control.WakeUpGroup > 7 )
                        txt = "G" + Control.WakeUpGroup + "\n";                                        // Resting Group           
                    if( Control.WakeupTotalTime > 0 )
                    {
                        if( Control.WakeupTimeCounter > 0 )
                            txt += Util.ToSTime( Control.WakeupTimeCounter );
                        else
                            txt += Util.ToSTime( Control.WakeupTotalTime );
                    }
                }
        }

        if( RightText != null )
        {
            if( Body.EnemyAttackBonus > 0 )
                txt = Body.EnemyAttackBonus.ToString( "0." ) + "%";
          
            if( TileID == ETileType.ALTAR ) return;            
            if( TileID == ETileType.QUEROQUERO )
            {
                if( Body.TotHp > 1 )
                    txt = "HP:" + Body.Hp.ToString( "0." );
                else
                txt = "x" + Body.Lives;
                //if( Control.FlightJumpPhase == 0 )                                                 // Initial Phase: Countdown to flight and target Choosing (-1) if was resting 
                //{
                //    float waittime = Map.I.RM.RMD.QueroQueroWaitTime;
                //    if( Control.WaitTime > 0 ) waittime = Control.WaitTime;
                //    float rem = waittime - Control.FlightPhaseTimer;
                //    if( rem >= 0 ) txt = "" + rem.ToString( "0.0" ) + "s";
                //}
            }

            if( TileID == ETileType.PLAGUE_MONSTER )
            {
                if( Variation == ( int ) ItemType.Plague_Monster_Blocker )                    // Frontface Blocker Price shown
                if( G.Hero.GetFront() == Pos )
                    {
                        int price = Map.I.Farm.GetUnblockPrice( this );
                        txt = "x" + price;
                        col = Color.green;
                        if( Item.GetNum( ItemType.Res_Plague_Monster ) < price )
                            col = Color.red;
                    }
            }

            if( TileID == ETileType.FROG )
            if( Map.Stepping() )
            {
                if( Control.SpeedTimeCounter >= 1 )
                    txt = "" + ( int ) Control.SpeedTimeCounter;
            }

            if( TileID == ETileType.HERB )
            if( Body.HerbType == EHerbType.SHUFFLE_BONUS )
            {
                txt = "x" + Body.HerbBonusAmount;
                if( Activated == false ) txt = "";
                if( Body.HerbBonusAmount == 1 ) txt = "";
            }

            if( TileID == ETileType.SCARAB )
            if( Body.Lives > 10 )
                {
                    txt = "x" + Body.Lives;
                }   

            RightText.color = col;
            RightText.text = txt;

            if( txt == "" )
                RightText.gameObject.SetActive( false );
            else
                RightText.gameObject.SetActive( true );
        }
    }

    public bool MakeMimicMove( EActionType ac )
    {
        switch( ac )
        {
            case EActionType.MOVE_N: if( CanMoveFromTo( true, Pos, Pos + new Vector2( 0, 1  ), G.Hero ) ) return true; break;
            case EActionType.MOVE_S: if( CanMoveFromTo( true, Pos, Pos + new Vector2( 0, -1 ), G.Hero ) ) return true; break;
            case EActionType.MOVE_E: if( CanMoveFromTo( true, Pos, Pos + new Vector2( 1, 0  ), G.Hero ) ) return true; break;
            case EActionType.MOVE_W: if( CanMoveFromTo( true, Pos, Pos + new Vector2( -1, 0 ), G.Hero ) ) return true; break;

            case EActionType.MOVE_NE:
            if( CanMoveFromTo( true, Pos, Pos + new Vector2( 1, 1 ), G.Hero ) ) return true; 
            if( CanMoveFromTo( true, Pos, Pos + new Vector2( 1, 0 ), G.Hero ) ) return true;
            if( CanMoveFromTo( true, Pos, Pos + new Vector2( 0, 1 ), G.Hero ) ) return true;
            break;

            case EActionType.MOVE_SE:
            if( CanMoveFromTo( true, Pos, Pos + new Vector2( 1, -1 ), G.Hero ) ) return true;
            if( CanMoveFromTo( true, Pos, Pos + new Vector2( 1, 0 ), G.Hero ) ) return true;
            if( CanMoveFromTo( true, Pos, Pos + new Vector2( 0, -1 ), G.Hero ) ) return true;
            break;

            case EActionType.MOVE_SW:
            if( CanMoveFromTo( true, Pos, Pos + new Vector2( -1, -1 ), G.Hero ) ) return true;
            if( CanMoveFromTo( true, Pos, Pos + new Vector2( -1, 0 ), G.Hero ) ) return true;
            if( CanMoveFromTo( true, Pos, Pos + new Vector2( 0, -1 ), G.Hero ) ) return true;
            break;

            case EActionType.MOVE_NW:
            if( CanMoveFromTo( true, Pos, Pos + new Vector2( -1, 1 ), G.Hero ) ) return true;
            if( CanMoveFromTo( true, Pos, Pos + new Vector2( -1, 0 ), G.Hero ) ) return true;
            if( CanMoveFromTo( true, Pos, Pos + new Vector2( 0, 1 ), G.Hero ) ) return true; 
            break;
        }

        return false;
    }
    
    public bool MakeSwingMove()
    {
        EActionType ac = EActionType.NONE;

        if( G.Hero.Control.LastAction == EActionType.ROTATE_CW )
        {
            if( G.Hero.Dir == EDirection.N  ) ac = EActionType.MOVE_E; else
            if( G.Hero.Dir == EDirection.NE ) ac = EActionType.MOVE_E; else
            if( G.Hero.Dir == EDirection.E  ) ac = EActionType.MOVE_S; else
            if( G.Hero.Dir == EDirection.SE ) ac = EActionType.MOVE_S; else
            if( G.Hero.Dir == EDirection.S  ) ac = EActionType.MOVE_W; else
            if( G.Hero.Dir == EDirection.SW ) ac = EActionType.MOVE_W; else
            if( G.Hero.Dir == EDirection.W  ) ac = EActionType.MOVE_N; else
            if( G.Hero.Dir == EDirection.NW ) ac = EActionType.MOVE_N; 
        }

        if( G.Hero.Control.LastAction == EActionType.ROTATE_CCW )
        {
            if( G.Hero.Dir == EDirection.N  ) ac = EActionType.MOVE_W; else
            if( G.Hero.Dir == EDirection.NE ) ac = EActionType.MOVE_N; else
            if( G.Hero.Dir == EDirection.E  ) ac = EActionType.MOVE_N; else
            if( G.Hero.Dir == EDirection.SE ) ac = EActionType.MOVE_E; else
            if( G.Hero.Dir == EDirection.S  ) ac = EActionType.MOVE_E; else
            if( G.Hero.Dir == EDirection.SW ) ac = EActionType.MOVE_S; else
            if( G.Hero.Dir == EDirection.W  ) ac = EActionType.MOVE_S; else
            if( G.Hero.Dir == EDirection.NW ) ac = EActionType.MOVE_W;
        }

        if( ac != EActionType.NONE )
        {
            Vector2 fr = G.Hero.Pos;
            EDirection dr = Dir;
            if( G.Hero.Control.LastAction == EActionType.ROTATE_CCW ) dr = Util.RotateDir( ( int ) G.Hero.Dir, +1 );
            if( G.Hero.Control.LastAction == EActionType.ROTATE_CW  ) dr = Util.RotateDir( ( int ) G.Hero.Dir, -1 );
            fr = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) dr ];
            Vector2 to = G.Hero.GetFront();

            bool clearfront = false;
            if( G.Hero.CanMoveFromTo( false, fr, to, G.Hero ) ) clearfront = true;
            if( clearfront )
            if( !Map.I.IsBarricade( fr ) )
            if( !Map.I.IsBarricade( to ) )
            switch( ac )
            {
                case EActionType.MOVE_N: if( CanMoveFromTo( true, Pos, Pos + new Vector2(  0,  1 ), G.Hero ) ) return true; break;
                case EActionType.MOVE_S: if( CanMoveFromTo( true, Pos, Pos + new Vector2(  0, -1 ), G.Hero ) ) return true; break;
                case EActionType.MOVE_E: if( CanMoveFromTo( true, Pos, Pos + new Vector2(  1,  0 ), G.Hero ) ) return true; break;
                case EActionType.MOVE_W: if( CanMoveFromTo( true, Pos, Pos + new Vector2( -1,  0 ), G.Hero ) ) return true; break;
            }
        }
         
        return false;
    }
    public void UpdateDynamicLeveling()
    {
        if( ValidMonster )
        {
            if( TileID == ETileType.SPIDER )
            {
                Body.Level = Body.BaseLevel;
            }
            else
            if( Body.BigMonster )
            {
                int area = Map.I.GetPosArea( Pos );
                Body.Level = Map.I.RM.RMD.BaseMonsterLevel + Body.BaseLevel;
                int mn = Map.I.LevelStats.RoachDeathCount;                              // Add other monsters, make bigmonster death count
                Body.Level += Map.I.RM.RMD.DynamicMonsterLevelAddPerDeath * mn;
                Body.TotHp = Util.Percent( Body.HitPointsRatio, Body.BaseTotHP );
                UpdateLevelingData();
            }  
        }
    }

    public void UpdateColor()
    {
        if( TileID == ETileType.WASP )                                                                                  // Fire Marked wasp
        {
            Spr.color = Color.white;
            if( Control.FireMarkedWaspFactor > 0 )
            {
                Spr.color = new Color( .3f, 0, 0, 1 );
                if( Control.FireMarkAvailable )
                    Spr.color = new Color( .75f, 0, 0, 1 );
            }

            if( Control.Mother.Control.Resting )
                Spr.color = Color.gray;

            return;
        }
        else
        if( TileID == ETileType.MOTHERWASP )                                                                                  // Fire Marked wasp
        {
            Spr.color = Color.white;      
            if( Control.Resting )
                Spr.color = Color.gray;
            Control.RestingRadiusSprite.color = new Color( 1, 1, 1, .12f );
            return;
        }
        else
        if( TileID == ETileType.BARRICADE )                                                                             // Changes Touched Barricade Color
        {
            float tr = 1;
            if( Body.TouchCount > 0 )
            {
                Spr.color = new Color( .65f, .65f, .65f, tr );                             // Touched Color  
            }
            else Spr.color = new Color( 1, 1, 1, tr );                                     // Normal Barricade

            if( Map.I.RM.RMD.BarricadeDestroyHeroWaitTime > 0 )
            {
                if( Controller.HeroBumpTarget == Pos )
                if( Controller.BarricadeBumpTimeCount <
                    Map.I.RM.RMD.BarricadeDestroyHeroWaitTime )
                    Spr.color = new Color( .9f, .5f, .5f, tr );
            }

            if( Map.I.RM.RMD.BarricadeDestroyWaitTime > 0 )                                // Independent per barricade wait time 
            {
                if( Body.BumpTimer > 0 )
                    Spr.color = new Color( .9f, .5f, .5f, tr );
            }

            Body.BumpTimer -= Time.deltaTime;
            if( Body.BumpTimer < 0 ) Body.BumpTimer = 0;
        }
        else
            if( ValidMonster || TileID == ETileType.ALTAR )                                 // Monster Resting Color
            {
                Spr.color = Color.white;     
                if( Body.SpriteColor != new Color( 0, 0, 0, 0 ) )
                    Spr.color = Body.SpriteColor;     
                if( Control.Resting )
                {
                    Color rs = Color.yellow;
                    Spr.color = rs;
                    Control.RestingRadiusSprite.color = new Color32( 0, 0, 255, 60 );

                    int dist = 0;
                    Control.InsideRestingRange( ref dist );

                    if( dist == 1 )
                        Control.RestingRadiusSprite.color = new Color32( 82, 84, 173, 109 );

                    if( RightText ) RightText.color = rs;
                    if( LevelTxt ) LevelTxt.color = rs;

                    if( TileID == ETileType.ROACH )
                    {
                        for( int i = 0; i < 8; i++ )
                        if ( Body.BabySprite[ i ].gameObject.activeSelf )
                        {
                             Body.BabySprite[ i ].color = new Color( .3f, .8f, .8f, 1f );
                             if( Control.RedRoachBabyList != null )                                      // Red Roach Baby color
                             if( Control.RedRoachBabyList.Contains( ( EDirection ) i ) )
                                 Body.BabySprite[ i ].color = new Color32( 200, 75, 75, 255 );
                        }
                    }
                }
                else
                {
                    Spr.color = new Color( 1f, 1f, 1f, 1f );

                    if( TileID == ETileType.ROACH )
                    {
                        for( int i = 0; i < 8; i++ )
                        if ( Body.BabySprite[ i ].gameObject.activeSelf )
                        {
                             Body.BabySprite[ i ].color = new Color( 1f, 1f, 1f, 1f );
                             if( Control.RedRoachBabyList != null )                                       // Red Roach Baby color
                             if( Control.RedRoachBabyList.Contains( ( EDirection ) i ) )
                                 Body.BabySprite[ i ].color = new Color32( 255, 75, 75, 255 );
                        }
                    }
                    if( Control.ForcedFrontalMovementFactor > 0 )                                         // Enraged red color fo slime or mushroom affected monster
                        Spr.color = new Color( 1, .2f, .2f );

                    if( Item.GetNum( ItemType.Res_Mask ) > 0 )                                            // Mask affected monster Color
                        Spr.color = new Color( .35f, .35f, .35f, 1f );
                }
            }
            else
                if( TileID == ETileType.BOULDER )                                                        // Boulder Resting Color
                {
                    if( Control.Resting )
                    {
                        Control.RestingRadiusSprite.color = new Color( 1, 1, 1, .12f );
                        Spr.color = new Color( 1f, .8f, .8f, 1f );
                    }
                    else
                        Spr.color = new Color( 1f, 1f, 1f, 1f );
                    Body.Shadow.color = new Color( 1, 1, 1, 1 );
                }
        else
        if( TileID == ETileType.RAFT )                                                 // Boulder Resting Color
        {
            if( Control.Resting )
                Spr.color = new Color( 1f, .8f, .8f, 1f );
            else
                Spr.color = new Color( 1f, 1f, 1f, 1f );
        }
        else
        if( TileID == ETileType.FOG )
        {
            float alpha = .45f;
            Unit hf = Controller.GetFog( G.Hero.Pos );                                 // Hero on fog group Different alpha
            if( hf && hf.Control.RaftGroupID == Control.RaftGroupID ) 
                alpha = .65f;

            Spr.color = new Color( 1, 1, 1, alpha );                                    
            if( Map.I.DirtyFogList.Contains( Control.RaftGroupID ) )                   // Dirty fog red color
                Spr.color = new Color( 1f, .6f, .6f, alpha );

            if( Map.I.ElectrifiedFogList.Contains( Control.RaftGroupID ) )             // Electric fog blue color
                Spr.color = new Color( .6f, .6f, 1, alpha );

            if( Control.RaftMoveDir != EDirection.NONE )                               // Moving fog blue color
                Spr.color = new Color( .45f, .45f, 1, alpha );
        }
        else        
        if( TileID == ETileType.TRAIL )
        {
            if( G.HS.CubeFrameCount == 1 )
                Spr.color = Color.red;
        }
        else
        if( Control && Control.IsDynamicObject() )                                     // Dynamic Object Color
        {
            if(!Control.Resting )
            if( Spr )
                Spr.color = new Color( 1f, .8f, .8f, 1f );
        }
    }

    public void UpdateAnimation()
    {
        if( Body ) Body. UpdateImmunityShield();                                        // immunity shield

       switch ( TileID )  
       {
           //case ETileType.BARRICADE:                                                    // Try to resolve the invisible barricade bug (Finally!!!)
           //if( G.HS.CubeFrameCount == 3 )
           //    Graphic.transform.localPosition = new Vector3( 0, 0, 0 );
           //break;

           case ETileType.SCARAB:
           Spr.spriteId = 31 + ( int ) Body.Lives;
           if( Body.Lives <= 5 )
               Body.AuxTransform.localPosition = new Vector3( 0, -0.41f, 0 );
           else Body.AuxTransform.localPosition = new Vector3( 0, 0.15f, 0 );
           Spr.transform.localScale = new Vector3( 1, 1, 1 );
           if( Body.Lives > 10 ) Spr.spriteId = 41;
           if( Body.IsInfected )
           {
               Spr.transform.localScale = new Vector3( 1.3f, 1.3f, 1 );                                                                    // Infected scarab
               Spr.color = Color.green;
               Body.InfectedSprite.transform.localScale = new Vector3( 1, 1, 1 );
               int rad = 1 + ( int ) ( ( Body.InfectedRadius ) * 2 );
               bool inf = false;
               float tot = Util.Round( InfectionAttack.GetRealtimeSpeedTime() );           
               if( Util.Round( InfectionAttack.SpeedTimeCounter ) >= tot )                                                                 // use round to avoid C# float imprecision bug this bug only occurs in the release version
                   inf = true;                                                                                                              
               Unit mudf = Map.I.GetMud( G.Hero.Pos );
               if( mudf ) inf = false;
               mudf = Map.I.GetMud( Pos );                                                                                  // mud x infected
               if( mudf ) inf = false;

               if( inf  )  
               {
                   if( Body.InfectedSprite.scale.x <= 1.5f )
                       MasterAudio.PlaySound3DAtVector3( "Roach Glue", transform.position );
                   Vector3 tg = new Vector3( rad, rad, 1 );
                   Body.InfectedSprite.scale = Vector3.MoveTowards( Body.InfectedSprite.scale, tg, Time.deltaTime * 50f );                  // expand infection sprite
               }
               else
               {
                   if( Body.InfectedSprite.scale.x >= rad )
                       MasterAudio.PlaySound3DAtVector3( "Roach Glue", transform.position );
                   Vector3 tg = new Vector3( 1.5f, 1.5f, 1 );
                   Body.InfectedSprite.scale = Vector3.MoveTowards( Body.InfectedSprite.scale, tg, Time.deltaTime * 50f );                  // shrink infection sprite 
               }
           }
           break;

           case ETileType.HUGGER:
           Spr.spriteId = 144 + ( int ) Body.Lives;
           if( Body.Lives > 3 ) Spr.spriteId = 148;
           if( Map.I.CubeDeath ) return;

           float right = -1;
           float left = -1;
           if( Control.HuggerStepList != null )
           {
               if( Control.HuggerStepList.Count >= 1 ) left = 1;
               if( Control.HuggerStepList.Count >= 2 ) right = 1;
           }

           //if( Body.Sprite3.transform.localScale.y > 1.2f ) left = -1;
           //if( Body.Sprite4.transform.localScale.y > 1.2f ) right = -1;

           int dist = Util.Manhattan( Pos, G.Hero.Pos );
           if( dist > 1 )
           {
               Control.HuggerStepList = null;
           }
           if( dist <= 1 ) RangedAttack.SpeedTimeCounter = 0;
           float val = Body.Sprite3.transform.localScale.y;                                     // claw animation
           val += Time.deltaTime * 6 * left;
           val = Mathf.Clamp( val, 0, 1 );
           Body.Sprite3.transform.localScale = new Vector3( 1, val, 1 );
           val = Body.Sprite4.transform.localScale.y;
           val += Time.deltaTime * 6 * right;
           val = Mathf.Clamp( val, 0, 1 );
           Body.Sprite4.transform.localScale = new Vector3( 1, val, 1 );
           Body.Sprite5.gameObject.SetActive( false );
           if( Control.HuggerStepList != null )
           if( Control.HuggerStepList.Count >= 1 )
           if( Control.HuggerStepList[ 0 ] != new Vector2( -1, -1 ) )                            // Cross
           {
               Body.Sprite5.gameObject.SetActive( true );
               Body.Sprite5.transform.position = new Vector3( Control.HuggerStepList[ 0 ].x,
               Control.HuggerStepList[ 0 ].y, Body.Sprite5.transform.position.z );
           }

           break;

           case ETileType.FISHING_POLE:
           Water.UpdatePoleText( Water.ForceUpdatePoleText );
           break;

           case ETileType.DOOR_OPENER:
           case ETileType.DOOR_CLOSER:
           case ETileType.DOOR_SWITCHER:
           case ETileType.DOOR_KNOB:
           {
               Unit un = Map.I.GetUnit( ETileType.ORB, Pos );                                        // attach key to orb during tween effect
               if( un && G.Hero.Control.TurnTime < 1.5f )
               {
                   Spr.transform.position = new Vector3( un.Graphic.transform.position.x,
                   un.Graphic.transform.position.y, Spr.transform.position.z );
               }
               float amplitude = 0.04f; val = 1;
               val += Mathf.Sin( Time.fixedTime * Mathf.PI * 2 ) * amplitude;                        // scale animation
               Spr.transform.localScale = new Vector3( val, val, 1 );
               
               //if( Map.I.TurnFrameCount == 1 )
               //if( TileID == ETileType.DOOR_KNOB )                                                   // scales keys in a better way over doors
               {
                   Spr.scale = new Vector3( 1, 1, 1 );
                   Spr.transform.localPosition = new Vector3( 0, 0, -0.8f );
                   if( Map.I.GetUnit( ETileType.CLOSEDDOOR, Pos ) )
                   {
                       Spr.scale = new Vector3( .75f, .75f, 1 );
                       Spr.transform.localPosition = new Vector3( -.1f, 0.16f, -0.8f );
                   }
                   if( Map.I.GetUnit( ETileType.OPENDOOR, Pos ) )
                   {
                       Spr.scale = new Vector3( .75f, .75f, 1 );
                       Spr.transform.localPosition = new Vector3( 0.07f, -.09f, -0.8f );
                   }
                   Unit orb = Map.I.GetUnit( ETileType.ORB, Pos );
                   if( orb )
                   {
                       Spr.scale = new Vector3( .75f, .75f, 1 );
                       Spr.transform.localPosition = new Vector3( 0.035f, -0.035f, -6.1f );
                       if( iTween.Count( orb.Graphic.gameObject ) > 0 )
                       {
                           gameObject.SetActive( false );
                       }
                       else gameObject.SetActive( true );
                   }
               }
           }
           break;
           case ETileType.FISH:
           if( Map.I.IsNormalFish( this ) == false )
           {
               float amplitude = 0.04f; val = .6f;
               val += Mathf.Sin( Time.fixedTime * Mathf.PI * 2 ) * amplitude;                        // scale animation
               Body.Sprite3.color = new Color( 1, 1, 1, val );
               Body.Sprite3.gameObject.SetActive( true );
           }
           break;
           case ETileType.FROG:
           Spr.spriteId = 95 + ( int ) Body.Lives;
           if( Body.Lives > 7 ) Spr.spriteId = 102;

           float tottime = Control.GetRealtimeSpeedTime();
           float per = Util.GetPercent( Control.SpeedTimeCounter, tottime );
           float frac = 1;
           float v = 1 - ( Util.Percent( per, 1 ) * frac );
           if( Map.Stepping() )
               v = 1 - ( Util.GetPercent( Control.SpeedTimeCounter, Md.MaxFrogTurnDelay ) / 100 );                      // stepping frog color calculation

           Spr.color = new Color( 1, v, v, 1 );        

           if( Control.SmoothMovementFactor >= 1 )
           {
               Vector3 tg = new Vector3( 0, -0.2f, 0.24f );
               Body.Sprite2.transform.localPosition = Vector3.MoveTowards(
               Body.Sprite2.transform.localPosition, tg, Time.deltaTime * .7f );

               tottime = Control.GetRealtimeSpeedTime();
               per = Util.GetPercent( Control.FrogHeroBlockTimeCounter, Control.HeroBlockFrogTotalTime );

               float zRotation = Util.Percent( per, 360f );                                                             // indicator
               Body.Sprite3.transform.eulerAngles = new Vector3(
               Body.Sprite3.transform.eulerAngles.x,
               Body.Sprite3.transform.eulerAngles.y, 360- zRotation );
               if( Map.Stepping() )
                   Body.Sprite3.gameObject.SetActive( false );
               else
                   Body.Sprite3.gameObject.SetActive( true );
           }
           break;

           case ETileType.ROACH:
           if( Util.IsNeighbor( Pos, G.Hero.Pos ) ) 
               Control.HeroNeighborTimeCount += Time.deltaTime;
           else Control.HeroNeighborTimeCount = 0;
           if( Control.HeroNeighborTimeCount > Control.GetRealtimeSpeedTime() )
               UpdateDirection( true );

           float atttime = MeleeAttack.GetRealtimeSpeedTime();                   // Roach Claw      
           Body.Sprite2.gameObject.transform.localScale = new Vector3( 1f, 1f, 1 );
           Body.Sprite3.gameObject.transform.localScale = new Vector3( 1f, 1f, 1 );

           if( Control.Resting == false )
           if( MeleeAttack.SpeedTimeCounter < atttime )
           {
               Body.Sprite2.gameObject.transform.localScale = new Vector3( .5f, .5f, 1 );
               Body.Sprite3.gameObject.transform.localScale = new Vector3( .5f, .5f, 1 );
           }

           for( int i = 0; i < 8; i++ )                                          // Roach babies
           {
               Body.BabySprite[ i ].gameObject.SetActive( false );
               if( Body.HasBaby[ i ] )
                   Body.BabySprite[ i ].gameObject.SetActive( true );
           }
           break;

           case ETileType.MINE:   /////////////////////////////////////////////////////////////// Mine
           Map.I.MineCount++;
           bool sp3 = false;
           float sp3scale = 0f;
           Color sp3col = Color.white;
           Mine.MineRotationSpeed -= Time.deltaTime * 30f;
           if( Mine.MineRotationSpeed < 0 ) Mine.MineRotationSpeed = 0;
           bool eff4 = false;
           bool neigh = Util.IsNeighbor( G.Hero.Pos, Pos );
              
           Mine.UpdateMineBonusText();                                                         // update mine bonus text

           if( Mine.AnimateIconTimer > 0 || GS.LoadFrameCount == 1 )
           {
               float f = 1;
               if( GS.LoadFrameCount == 1 ) f = 10000;                                         // Fast rotate after loading
               Body.Sprite3.transform.localPosition = Vector3.MoveTowards(                     // mine bonus icon slide FX
               Body.Sprite3.transform.localPosition, new Vector3( 0, 0,
               Body.Sprite3.transform.localPosition.z ), Time.deltaTime * 12f * f);
               Body.Sprite4.transform.localPosition = Vector3.MoveTowards(                     // mine bonus icon slide FX
               Body.Sprite4.transform.localPosition, new Vector3( 0, -0.39f,
               Body.Sprite4.transform.localPosition.z ), Time.deltaTime * 12f * f );
               Body.EffectList[ 3 ].transform.localPosition = Vector3.MoveTowards(             // mine bonus icon slide FX
               Body.EffectList[ 3 ].transform.localPosition, new Vector3( 0, 0,
               Body.EffectList[ 3 ].transform.localPosition.z ), Time.deltaTime * 12f * f );
               Util.SmoothRotate( Spr.transform, Dir, 15 * f );
               Util.SmoothRotate( Body.Sprite8.transform, Body.UPMineDir, 15 * f );
               Util.SmoothRotate( Body.Sprite3.transform, Mine.MineBonusDir, 15 * f );
               Mine.AnimateIconTimer -= Time.deltaTime;
               Mine.UpdateText = true;
           }

           if( G.HS.CubeFrameCount == 1 )
           {
               Vector3 angle = Util.GetRotationAngleVector( Dir );                                  // init sprites rotation
               if( Body.MineType != EMineType.VAULT )
               Spr.transform.eulerAngles = angle;
               angle = Util.GetRotationAngleVector( Body.UPMineDir );
               Body.Sprite8.transform.eulerAngles = angle;
               angle = Util.GetRotationAngleVector( Mine.MineBonusDir );
               Body.Sprite3.transform.eulerAngles = angle;
           }

           if( Map.I.TurnFrameCount == 2 )
           if( G.HS.GloveTarget == Pos )
               {
                   Controller.CreateMagicEffect( Pos );                                                                                 // Create Magic effect for glove target
               }
          
           if( Body.MineType == EMineType.INDESTRUCTIBLE )
           {
               float rot = Spr.transform.eulerAngles.z;
               if( Body.ClockwiseChainPush )
                   rot -= Time.deltaTime * 50;
               else
                   rot += Time.deltaTime * 50;
               if( rot > 360 ) rot -= 360;
               if( rot < 0 ) rot += 360;
               if( Body.RopeConnectSon == null ) rot = 0;
               Spr.transform.eulerAngles = new Vector3( 0, 0, rot );               
           }

           if( Mine.SpikedMine )                                                               // Spike Mine rotation
           {
               float z = Body.Sprite3.transform.eulerAngles.z + ( Time.deltaTime * 100 );
               if( z > 360 ) z -= 360;
               Body.Sprite3.transform.eulerAngles = new Vector3( 0, 0, z );
           }

           if( Mine.StickyMine )                                                                         // Sticky Mine
           {              
               int count = Mine.StickyMineCanSuck( Pos );
               if( Mine.GetMineBonusCount( Pos ) > 0 ) count = 0;                                        // mine already has a bonus
               float z = Body.EffectList[ 3 ].transform.eulerAngles.z + ( Time.deltaTime * 20 );
               if( z > 360 ) z -= 360;
               if( count != 1 ) z = 0;
               Body.EffectList[ 3 ].transform.eulerAngles = new Vector3( 0, 0, z );
           }

           if( Mine.WheelMine )                                                                // Wheel Mine
           {            
               float rot = Body.Sprite2.transform.eulerAngles.z;
               if( Mine.CogRotationDir > 0 )
                   rot += Time.deltaTime * Mine.MineRotationSpeed;
               else
                   rot -= Time.deltaTime * Mine.MineRotationSpeed;
               if( rot > 360 ) rot -= 360;
               if( rot < 0 ) rot += 360;
               Body.Sprite2.transform.eulerAngles = new Vector3( 0, 0, rot );
           }

           if( Mine.CogMine )                                                             // Dynamite Mine
           {
               Body.Sprite3.spriteId = 302;
               sp3scale = .65f;
               float rot = Body.Sprite3.transform.eulerAngles.z;
               if( Mine.CogRotationDir > 0 )
                   rot += Time.deltaTime * 50;
               else
                   rot -= Time.deltaTime * 50;
               if( rot > 360 ) rot -= 360;
               if( rot < 0 ) rot += 360;
               Body.Sprite3.transform.eulerAngles = new Vector3( 0, 0, rot );
               sp3 = true;
               sp3col = new Color32( 248, 112, 0, 255 );
               Vector2 mid = Vector2.zero;
               List<Vector2> pl = Control.GetCogMineTgList( Pos, ref mid );                               // adjusts cog position
               if( neigh && Util.IsDiagonal( G.Hero.Dir ) == false ) 
               {
                   eff4 = true;
                   sp3scale = .9f;                   
                   Vector3 hammerangle = Util.GetRotationAngleVector( G.Hero.Dir );
                   Body.EffectList[ 4 ].gameObject.transform.localEulerAngles = G.Hero.Spr.transform.eulerAngles;
                   Body.Sprite3.transform.position = new Vector3( mid.x, mid.y, Body.Sprite3.transform.localPosition.z );
               }
               else
                   Body.Sprite3.transform.localPosition = new Vector3( 0, 0, Body.Sprite3.transform.localPosition.z );
               if( neigh && Util.IsDiagonal( G.Hero.Dir ) == true )
               {
                   sp3col = new Color32( 183, 255, 109, 255 );
                   sp3scale = .75f;
               }
           }

           if( Body.MineType == EMineType.VAULT ) return;

           if( Body.MineType == EMineType.SPIKE_BALL )
           {
               UpdateSpikedBallChainSizes();
           }

           if( G.HS.CubeFrameCount == 1 ) 
               Mine.UpdateText = true;
           if( Map.I.TurnFrameCount == 1 )
           if( Util.Manhattan( Pos, G.Hero.Pos ) <= 2 ) 
               Mine.UpdateText = true;
           if( Util.IsNeighbor( G.Hero.Pos, Pos ) )
           if( Body.MineType == EMineType.WEDGE_LEFT || 
               Body.MineType == EMineType.WEDGE_RIGHT )
           {
               Mine.AnimateIconTimer = 1;
               Mine.UpdateText = true;
           }
           if( Mine.UpdateChiselText )
           if( Map.I.TurnFrameCount == 3 )
           {
               int chisel = Controller.GetNeighborChisels( Pos );
               if( chisel > 0 )
                   Mine.UpdateText = true;
           }

           if( Mine.UpdateAllMinesText == false )
           if( Mine.UpdateText == false ) return;
           Mine.UpdateText = false;

           Spr.spriteId = 224;
           Spr.color = Color.white;
           Spr.transform.localPosition = new Vector3( 0, 0, -1.80f + GetZFactor( .01f ) );
           Body.Sprite2.gameObject.SetActive( false );
           Body.Sprite8.gameObject.SetActive( false );
           LevelTxt.gameObject.SetActive( false );
           Body.EffectList[ 0 ].gameObject.SetActive( false );
           Body.EffectList[ 1 ].gameObject.SetActive( false );
           Body.EffectList[ 3 ].gameObject.SetActive( false );
                                     
           Body.Sprite5.gameObject.SetActive( false );                                  // Mine Crack effect
           if( Mine.HitCount > 0 )
           {
               Body.Sprite5.gameObject.SetActive( true );
               int frm = 237 + Mine.HitCount;
               if( frm >= 242 ) frm = 242;
               Body.Sprite5.spriteId = frm;
           }

           if( Mine.SwapperMine )  
           {
               sp3 = true;
               sp3scale = .6f;
               if( neigh ) sp3scale = .8f;
               Body.Sprite3.spriteId = 300;
               EDirection dr = Util.GetRotationToTarget( G.Hero.Pos, Pos );
               Body.Sprite3.transform.eulerAngles = Util.GetRotationAngleVector( dr );
           }
           if( Mine.HoleMine )
           {
               sp3 = false;
               Body.Sprite2.spriteId = 236;
               Body.Sprite2.gameObject.SetActive( true );
           }
           if( Mine.RopeMine )                                                                 // Rope Connected Mine
           {
               sp3 = true;
               sp3scale = .35f;
               if( neigh ) sp3scale = .55f;
               Body.Sprite3.spriteId = 231;
           }
           if( Mine.HammerMine )                                                               // Hammer Mine
           {
               sp3 = true;
               sp3scale = .7f;
               if( neigh ) sp3scale = 1.0f;
               Body.Sprite3.spriteId = 268;
           }
           if( Mine.ArrowMine )                                                                // Arrow Mine
           {
               sp3 = true;
               sp3scale = 1f;
               if( neigh ) sp3scale = 1.3f;
               Body.Sprite3.spriteId = 303;
               if( neigh )
               {
                   Mine.MineBonusDir = G.Hero.Dir;
                   Mine.AnimateIconTimer = 1f;
               }
           }
           if( Mine.GloveMine )                                                                // Glove Mine
           {
               sp3 = true;
               sp3scale = 1f;
               if( neigh ) sp3scale = 1.2f;
               Body.Sprite3.spriteId = 305;
               if( neigh )
               {
                   Mine.MineBonusDir = G.Hero.Dir;
                   Mine.AnimateIconTimer = 1f;
               }
           }
           if( Mine.DynamiteMine )                                                            // Dynamite Mine
           {
               sp3 = true;
               sp3scale = .65f;
               Body.Sprite3.spriteId = 264;
           }
           if( Mine.ChiselMine )                                                               // Chisel Mine
           {
               sp3 = true;
               sp3scale = .7f;
               if( neigh ) sp3scale = 1.0f;
               Body.Sprite3.spriteId = 267;
           }
           if( Mine.CannonMine )                                                               // Cannon Mine
           {
               sp3 = true;
               Body.Sprite3.spriteId = 301;
               sp3scale = .6f;
               if( neigh )
               {
                   sp3scale = .9f; 
                   Mine.AnimateIconTimer = 1f;
                   Controller.UpdateCannonMine( this, Pos, G.Hero.Pos, true );                 // Update Cannon Mine Rotation Dir
               }  
           }
           if( Mine.MagnetMine )                                                               // Magnet Mine
           {
               sp3 = true;
               Body.Sprite3.spriteId = 299; 
               sp3scale = .5f;               
               if( neigh ) sp3scale = .75f;
           }
           if( Mine.WheelMine )                                                                // Wheel Mine
           {
               Body.Sprite2.transform.localScale = new Vector3( 1.35f, 1.35f, 1 );
               Body.Sprite2.spriteId = 298;
               Body.Sprite2.gameObject.SetActive( true );
               if( Mine.MineBonusDir != EDirection.NONE )
                   Body.EffectList[ 0 ].gameObject.SetActive( true );
           }
           if( Mine.JumperMine )                                                               // Jumper Mine
           {
               sp3 = true;
               Body.Sprite3.spriteId = 304;
               sp3scale = .5f;
               sp3col = Color.white;
               if( neigh )
               {
                   sp3scale = .75f;
                   Mine.MineBonusDir = G.Hero.Dir;
                   Mine.AnimateIconTimer = 1f;
                   Unit front = Map.GFU( ETileType.MINE, G.Hero.GetFront() );
                   if( front && G.Hero.GetFront() != Pos )
                       sp3col = Color.green;
                   else sp3col = Color.yellow;
               }
           }

           if( Mine.SpikedMine )                                                               // Chisel Mine
           {
               sp3 = true;
               sp3scale = 1.2f;
               Body.Sprite3.spriteId = 263;
               Body.Sprite3.gameObject.transform.localScale = new Vector3( 1.2f, 1.2f, 1 );
           }
           else
           if( Mine.CogMine == false )
           if( Mine.ArrowMine == false )
           if( Mine.CannonMine == false )
           if( Mine.SwapperMine == false )
           if( Mine.MagnetMine == false )
           if( Mine.JumperMine == false )
           if( Mine.GloveMine == false )
               Body.Sprite3.transform.eulerAngles = new Vector3( 0, 0, 0 );

           if( Mine.StickyMine )                                                                         // Sticky Mine
           {
               Body.EffectList[ 3 ].gameObject.SetActive( true );
               if( Map.I.SessionFrameCount == 100 )
               Body.EffectList[ 3 ].transform.localPosition = new Vector3( 
               Body.EffectList[ 3 ].transform.localPosition.x,
               Body.EffectList[ 3 ].transform.localPosition.y, -2.23f + GetZFactor( .01f ) );               
           }

           if( Mine.RopeMine    || Mine.HammerMine ||
               Mine.ChiselMine  || Mine.HoleMine   || Mine.WheelMine ||
               Mine.SwapperMine || Mine.ChiselMine || Mine.ArrowMine )                                      
           {
               if( neigh || Mine.WheelMine )
               {                                                                                                                // Show attached rope sprite fx
                   EDirection dr = Util.GetTargetUnitDir( G.Hero.Pos, Pos );
                   if( Mine.HoleMine || Mine.ArrowMine ) dr = G.Hero.Dir;
                   if( Mine.WheelMine ) dr = Mine.MineBonusDir;
                   if( dr != EDirection.NONE )
                   {
                       Vector2 tg = Pos + Manager.I.U.DirCord[ ( int ) dr ];
                       Unit mine = Map.GFU( ETileType.MINE, tg );
                       
                       if( mine != null )
                       {
                           Vector3 angle = Util.GetRotationAngleVector( dr );
                           Body.EffectList[ 0 ].gameObject.transform.localEulerAngles = angle;
                           bool rope = true;
                           if( Mine.ArrowMine && Mine.GetMineBonusCount( tg ) == 0 ) rope = false;
                           Body.EffectList[ 0 ].gameObject.SetActive( rope );                           
                           if( Mine.ArrowMine )
                           {
                               if(!rope )
                                   sp3col = new Color32( 173, 255, 109, 255 );
                               else
                                   sp3col = new Color( 1, 0.8f, 0, 1 );
                           }
                           if( Mine.SwapperMine ) sp3col = Color.yellow;
                           //if( Mine.RopeMine && neigh ) sp3scale = 2.2f;
                           Vector2 pt = Manager.I.U.DirCord[ ( int ) dr ] * .5f;
                           Body.EffectList[ 0 ].gameObject.transform.localPosition = new Vector3( pt.x, pt.y, -1.85f );
                           if( Mine.SwapperMine || Mine.HammerMine || Mine.ChiselMine )                                                    // hammer target sprite ativation
                           {
                               Vector3 hammerangle = Util.GetRotationAngleVector( G.Hero.Dir );
                               mine.Body.EffectList[ 4 ].gameObject.transform.localEulerAngles = hammerangle;
                               mine.Body.EffectList[ 4 ].gameObject.SetActive( true );
                           }
                       }

                       if( mine == null )
                       {
                           if( Mine.ArrowMine )
                               sp3col = new Color32( 255, 63, 63, 255 );
                           if( Mine.SwapperMine )
                           {
                               if( Controller.UpdateSwapperMine( G.Hero.Pos, Pos, true ) == false ) 
                                   sp3col = Color.green;                                                                                   // Update Swapper Mine
                           }
                       }
                   }
               }
           }

           if( Body.MineHasLever() )                                                // Lever Mine
           {
               if( Body.LeverMine )
                   Body.Sprite2.gameObject.SetActive( true );
               Body.Sprite2.spriteId = 237;
               Vector3 angle = Util.GetRotationAngleVector( Body.MineLeverDir );
               Body.Sprite2.transform.eulerAngles = angle;
               Body.Sprite2.color = Color.white;
               RightText.color = Color.white; 
               Body.EffectList[ 1 ].gameObject.transform.localEulerAngles = angle;
               Body.EffectList[ 1 ].gameObject.SetActive( true );
               Body.Rope.NodeDistanceOffSet = -0.2f;

               //if( neigh )
               {
                   //Body.MineLeverActivePuller.Body.EffectList[ 2 ].gameObject.SetActive( true );
                   float rot = 1;
                   if( Body.MineLeverActivePuller.Body.ClockwiseChainPush ) rot = -1;
                   Body.MineLeverActivePuller.Body.EffectList[ 2 ].transform.Rotate( new Vector3( 0, 0, 30 * Time.deltaTime * rot ) );
               }
           }

           Spr.spriteId = GetMineSprite( Body.MineType );

           if( Body.UPMineType != EMineType.NONE )                                                      // up mine
           {
               Body.Sprite8.color = Color.white;
               Body.Sprite8.gameObject.SetActive( true );
               if( Body.UPMineType != EMineType.BRIDGE )
               if( Body.UPMineType != EMineType.LADDER )
               if( Body.MineType != EMineType.SQUARE_BASE )
                   {
                       Spr.color = new Color32( 255, 238, 182, 255 );
                       Body.Sprite8.color = new Color32( 255, 238, 182, 255 ); 
                   }

               float sc = .9f;
               Body.Sprite8.transform.localScale = new Vector3( sc, sc, 1f );

               if( Body.UPMineType == EMineType.LADDER )
               {
                   float ds = .45f;
                   Body.Sprite8.scale = new Vector3( 1, 1.35f, 1 );
                   if( Util.IsDiagonal( Body.UPMineDir ) )
                   {
                       Body.Sprite8.scale = new Vector3( 1, 1.6f, 1 );
                       ds = 0.48f; 
                   }
                   Vector2 pt = Pos + ( Manager.I.U.DirCord[ ( int ) Body.UPMineDir ] * ds );
                   Body.Sprite8.transform.position = new Vector3( pt.x, pt.y, -1.99f );
               }

               if( Body.UPMineType == EMineType.BRIDGE )
               {
                   Vector2 tg = Pos + Manager.I.U.DirCord[ ( int ) Body.UPMineDir ];
                   Unit front = Map.GFU( ETileType.MINE, tg );
                   tg = Pos + Manager.I.U.DirCord[ ( int ) Util.GetInvDir( Body.UPMineDir ) ];
                   Unit back = Map.GFU( ETileType.MINE, tg );
                   float sz = 2.08f;
                   Vector2 fact = Vector2.zero;
                   fact = Manager.I.U.DirCord[ ( int ) Body.UPMineDir ];
                   fact.Normalize();                   

                   if( ( back  && ( back.Body.UPMineType  != EMineType.NONE || back.Body.MineType == EMineType.BRIDGE  ) ) &&
                       ( front && ( front.Body.UPMineType != EMineType.NONE || front.Body.MineType == EMineType.BRIDGE ) ) ) fact = Vector2.zero;                                     // adapt bridge sprite

                   if( Util.IsDiagonal( Body.UPMineDir ) )
                   {
                       fact *= -0.56f;
                       sz = 2.7f;
                       if( front == null || ( front.Body.MineType != EMineType.BRIDGE && front.Body.UPMineType == EMineType.NONE ) ) { sz = 1.78f; }
                       if( back == null  || ( back.Body.MineType  != EMineType.BRIDGE && back.Body.UPMineType  == EMineType.NONE ) ) { sz = 1.78f; fact *= -1; }
                   }
                   else
                   {
                       fact *= -0.35f;
                       if( front == null || ( front.Body.MineType != EMineType.BRIDGE && front.Body.UPMineType == EMineType.NONE ) ) { sz = 1.4f; }
                       if( back == null || ( back.Body.MineType != EMineType.BRIDGE && back.Body.UPMineType == EMineType.NONE ) ) { sz = 1.4f; fact *= -1; }
                   }
                   Body.Sprite8.transform.localScale = new Vector3( 1, 1, 1f );
                   Body.Sprite8.scale = new Vector3( 1, sz, 1 );
                   float z = -1.88f;
                   if( G.Hero.Control.Floor <= 2 )
                       z = -1.95f;
                   Body.Sprite8.transform.localPosition = new Vector3( fact.x, fact.y, z + GetZFactor( .1f ) );
                   Body.Sprite8.color = new Color32( 195, 165, 0, 255 ); 
               }
           }
           Body.Sprite8.spriteId = GetMineSprite( Body.UPMineType );

           Body.Sprite4.gameObject.SetActive( false );                                                         // Mine bonus item sprite update
           if( Body.MiningPrize != ItemType.NONE )
           if( Body.MiningBonusAmount > 0 )
           {
               Body.Sprite4.gameObject.SetActive( true );
               int id = ( int ) Body.MiningPrize;
               Body.Sprite4.spriteId = G.GIT( id ).TKSprite.spriteId;
               Body.Sprite4.transform.eulerAngles = new Vector3( 0, 0, 0 );
            }
           float sca = 1.2f;
           Unit mud = Map.I.GetMud( Pos );
           if( mud || Mine.HoleMine || Mine.WheelMine ) sca = .9f;

           if( Body.UPMineType != EMineType.NONE ) sca = 1.5f;
           
           if( Body.MineType == EMineType.SQUARE_BASE )
           {
               Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );
               Spr.spriteId = 93;
               Spr.scale = new Vector3( 1.26f, 1.26f, 1f );
               float zz = GetZFactor();
               Spr.transform.localPosition = new Vector3( 0, 0, -1.80f + zz );
           }
           if( Body.MineType == EMineType.TUNNEL)
           {
               Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );
               Spr.spriteId = 274;
               Spr.scale = new Vector3( sca, sca, 1f );
               Spr.transform.localPosition = new Vector3( 0, 0, -0.12f );              
           }   
           if( Body.MineType == EMineType.ROUND )
           {
               Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );
               Spr.scale = new Vector3( sca, sca, 1f );
           }   
           if( Body.MineType == EMineType.SQUARE )
           {
               if( Body.UPMineType != EMineType.NONE ) sca = 1.3f;
               Spr.scale = new Vector3( sca, sca, 1f );
               Spr.transform.localPosition = new Vector3( 0, 0, -1.80f + GetZFactor( .01f ) - 0.05f );
           }
           if( Body.MineType == EMineType.SHACKLE )
           {
               Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );
           }
           if( Body.MineType == EMineType.SPIKE_BALL )
           {
               Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );
               Unit fire = Map.I.GetUnit( ETileType.FIRE, Pos );                                  // Fire in position?
               if( fire && fire.Body.FireIsOn )               
               {
                   Body.OverFireTimeCount += Time.deltaTime;
                   if( Body.OverFireTimeCount > .6f ) 
                   {
                       Map.Kill( this );
                       MasterAudio.PlaySound3DAtVector3( "Fire Ignite", Map.I.Hero.Pos );        // Destroy ball over fire
                       Controller.CreateMagicEffect( Pos, "Mine Debris", 5 );  
                   }
               }
               if( fire == null ) Body.OverFireTimeCount = 0;
           }
           if( Body.MineType == EMineType.SUSPENDED )
           {
               Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );
           }

           if( Body.MineType == EMineType.LADDER )
           {
               float ds = .3f;
               Spr.scale = new Vector3( 1, 1.3f, 1 );
               if( Util.IsDiagonal( Dir ) ) 
               {
                   Spr.scale = new Vector3( 1, 1.65f, 1 );
                   ds = .35f; 
               }
               Vector2 pt = Pos + ( Manager.I.U.DirCord[ ( int ) Dir ] * ds );
               Spr.transform.position = new Vector3( pt.x, pt.y, -1.85f );
           }

           if( Body.MineType == EMineType.BRIDGE )
           {
               Mine.UpdateSupportStatus( this );                                                                          // updates Support status
               Vector2 tg = Pos + Manager.I.U.DirCord[ ( int ) Dir ];
               Unit front = Map.GFU( ETileType.MINE, tg );
               tg = Pos + Manager.I.U.DirCord[ ( int ) Util.GetInvDir( Dir ) ];
               Unit back = Map.GFU( ETileType.MINE, tg );
               float sz = 2.2f;
               Vector2 fact = Vector2.zero, fc2 = Vector2.zero;
               fact = Manager.I.U.DirCord[ ( int ) Dir ];
               fact.Normalize();
               fact *= -.2f;
               fc2 = fact;
               if( back == null && front == null )
               {
                   Map.Kill( this );                                                                                     // kill bridge with no connection on both sides
                   Map.I.CreateExplosionFX( Pos );   
               }

               if( back != null && front != null ) fact = Vector2.zero;                                                  // adapt bridge sprite
               if( Util.IsDiagonal( Dir ) )
               {
                   sz = 2.6f;
                   if( front == null || Mine.FrontSupport == 1 ) { sz = 2.2f; }
                   if( back == null  || Mine.BackSupport == 1  ) { fact *= -1; sz = 2.2f; }
                   if( Mine.FrontSupport == 1 )
                   {
                       fact = fc2; sz = 2.6f;
                   }
                   if( Mine.BackSupport == 1 )
                   {
                       fact = fc2; fact *= -1; sz = 2.6f;
                   }
                   if( Mine.BackSupport == 1 && Mine.FrontSupport == 1 )
                   {
                       sz = 2.3f; fact = Vector2.zero;
                   }
               }
               else
               {
                   if( front == null ) { sz = 1.4f; }
                   if( back == null  ) { fact *= -1; sz = 1.4f; }
                   if( Mine.FrontSupport == 1 )
                   {
                       fact = fc2; sz = 1.8f; 
                   }
                   if( Mine.BackSupport == 1 ) 
                   {
                       fact = fc2; fact *= -1; sz = 1.8f;
                   }
                   if( Mine.BackSupport == 1 && Mine.FrontSupport == 1 )
                   {
                       sz = 1.56f; fact = Vector2.zero;
                   }
               }
               Spr.scale = new Vector3( 1, sz, 1 );

               float z = -1.86f;
               if( G.Hero.Control.Floor <= 1 )
                   z = -1.95f;         
               Spr.transform.localPosition = new Vector3( fact.x, fact.y, z );
           }           
           
           if( Body.MineType == EMineType.HOOK )
           {
               Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );
               Spr.spriteId = 245;
               if( Body.RopeConnectFather == null || Body.RopeConnectFather.Count < 1 )
                   Body.gameObject.transform.localScale = new Vector3( .6f, .6f, 1f );
               else
                   Body.gameObject.transform.localScale = new Vector3( 1, 1, 1 );
               if( Body.MinePushSteps >= 1 )
               {
                   Spr.spriteId = 245 + Body.MinePushSteps;
                   if( Spr.spriteId > 255 ) Spr.spriteId = 255;
               }
           }
           if( Body.MineType == EMineType.WEDGE_LEFT )
           {
               Spr.spriteId = 272;
           }
           if( Body.MineType == EMineType.WEDGE_RIGHT )
           {
               Spr.spriteId = 273;
           }

           //if( Body.MineType == EMineType.RED )
           //{
           //    RotateTo( EDirection.N );
           //}

           //Body.Sprite6.gameObject.SetActive( false );
           //if( Body.MineGreenCrystalAmount > 0 )
           //{
           //    Body.Sprite6.spriteId = 276 + ( int ) Body.MineGreenCrystalAmount - 1;
           //    Body.Sprite6.gameObject.SetActive( true );
           //}

           //Body.Sprite7.gameObject.SetActive( false );
           //if( Body.MineRedCrystalAmount > 0 )
           //{
           //    Body.Sprite7.spriteId = 308 + ( int ) Body.MineRedCrystalAmount - 1;
           //    Body.Sprite7.gameObject.SetActive( true );
           //}

               RightText.gameObject.SetActive( true );
               if( Controller.CanBeMined( Pos ) == false )
                   RightText.gameObject.SetActive( false );

               float def = 50;
               float chance = Controller.GetMineChance( this, ref def );
               string txt = "" + chance.ToString( "0." ) + "%";
               float fc = chance / 100;
               if( Input.GetKey( KeyCode.F1 ) == false && chance == def )
               {
                   RightText.gameObject.SetActive( false );
                   txt = "";
               }
               else
                   RightText.color = new Color( 1 - fc, fc, 0 );
               RightText.text = txt;
               Body.AuxText.gameObject.SetActive( false );

               float bag = 0;
               if( G.HS.BagExtraBonusItemID != -1 )
               if( Body.MiningPrize == ( ItemType ) G.HS.BagExtraBonusItemID )                                        // Extra bonus due to vault
                   bag = Item.GetNum( ItemType.Res_Mining_Bag );
               float ramt = Body.MiningBonusAmount + bag;
               if( ramt > 1 )
               {
                   Body.AuxText.gameObject.SetActive( true );
                   Body.AuxText.text = "" + ramt.ToString( "+#;-#;0" );
                   if( bag != 0 )
                   {
                       Body.AuxText.text = "" + Body.MiningBonusAmount.ToString( "0.#" );
                       Body.AuxText.text += "+" + bag;
                   }
               }

           if( Body.MineType == EMineType.WEDGE_LEFT ||
               Body.MineType == EMineType.WEDGE_RIGHT )
           {
               RightText.gameObject.SetActive( false );
               List<Unit> neighl = Controller.GetNeighborSimilarMine( Pos, this );
               if( Body.MinePushSteps >= 1 || neighl.Count > 0 )
               {
                   RightText.gameObject.SetActive( true );
                   RightText.color = Color.green;
                   if( Body.MinePushSteps >= 1 )
                       RightText.text = "+" + Body.MinePushSteps;
                   if( neighl.Count > 0 )
                   {
                       if( Body.MinePushSteps < 1 )
                           RightText.text = "Push\nAway";
                       else
                           RightText.text += " Push\nAway";
                   }
                   EDirection dr = Util.GetRotationToTarget( G.Hero.Pos, Pos );
                   if( Util.Neighbor( G.Hero.Pos, Pos ) )
                       RotateTo( dr );
               }
               if( Body.MinePushSteps < 1 )
                   Spr.color = Color.gray;
           }

           //if( Body.DarkMineFactor > 0 )
           //if( Pos == G.Hero.GetFront() )
           //{
           //    float cost = Controller.GetMiningPrice( Pos, this );
           //    LevelTxt.text += "(" + cost +")";
           //    LevelTxt.gameObject.SetActive( true );
           //}

           float moveSpeed = 10f;
           if( Control.FlyingTarget.x != -1 )
               Graphic.transform.position = Vector3.MoveTowards( Graphic.transform.position,              // fly animation
               Control.FlyingTarget, Time.deltaTime * moveSpeed );
           else
           if( Map.I.GetUnit( ETileType.CLOSEDDOOR, Pos ) )
           if( Body.MineType != EMineType.BRIDGE )
               Spr.transform.localPosition -= new Vector3( .2f, -.2f, 0 );                                // suspended mine
           
           Body.Sprite3.gameObject.SetActive( sp3 );
           if( sp3scale > 0 )
               Body.Sprite3.scale = new Vector3( sp3scale, sp3scale, 1f );

           if( eff4 && eff4 != Body.EffectList[ 4 ].gameObject.activeSelf )
               Body.EffectList[ 4 ].gameObject.SetActive( eff4 );
           Body.Sprite3.color = sp3col;
           break;
           case ETileType.SAVEGAME:

           if( Map.I.TurnFrameCount == 10 )
           {
               RightText.color = Color.green;                                                              // Save Price tag color
               float num = Item.GetNum( ( ItemType ) Variation );
               if( num < Body.StackAmount )
                   RightText.color = Color.red;
               Body.Sprite2.gameObject.SetActive( false );
               RightText.gameObject.SetActive( false );
           }
           if( Pos == G.Hero.Pos )
           {
               Body.Sprite2.gameObject.SetActive( true );
               RightText.gameObject.SetActive( true );
           }
           break;
           case ETileType.ITEM:
           if( Variation == ( int ) ItemType.Res_Hook_CW  ||
               Variation == ( int ) ItemType.Res_Hook_CCW )
           {
               EDirection dr = Util.GetRotationToTarget( G.Hero.Pos, Pos );
               if( Map.I.AdvanceTurn )
               {
                   if( Util.Neighbor( Pos, G.Hero.Pos ) )
                       Spr.transform.eulerAngles = Util.GetRotationAngleVector( dr );
                   else
                       Spr.transform.eulerAngles = Vector3.zero;
               }               
           }
           else
           if( Variation == ( int ) ItemType.Res_Mushroom )                          
           {
               if( Body.StackAmount >= 100 )
                   Spr.spriteId = 63;
               Spr.transform.localScale = new Vector3( 1, 1, 1 );
           }
           else
           {
               Spr.transform.localScale = new Vector3( .5f, .5f, 1 );
           }
           Spr.gameObject.SetActive( true );

           if( Dir != EDirection.NONE )                                                        // item shadow
           {
               Body.Sprite3.gameObject.SetActive( true );
               Body.Sprite3.transform.eulerAngles = Util.GetRotationAngleVector( Dir );
               if( Md.ResourceType == ItemType.NONE ) Spr.gameObject.SetActive( false );
           }

           if( Map.I.GetUnit( ETileType.WATER, Pos ) )
               UpdateBuoyAnimation();
           break;

           case ETileType.RAFT:
           if( Manager.I.GameType != EGameType.NAVIGATION )
           {
               UpdateBuoyAnimation();
               var emission = Body.Particle.emission;
               var rate = emission.rateOverTime.constant; 
               if( Control.RaftMoveDir == EDirection.NONE )
                   rate -= Time.deltaTime * 8f; 
               else
                  rate = 25f; 
               if( rate < 0f ) rate = 0f;
               emission.rateOverTime = new ParticleSystem.MinMaxCurve( rate );
               if( Map.I.GetUnit( ETileType.PIT, Pos ) ) 
                   rate = 0;
               Body.Particle.gameObject.SetActive( rate > 0f );                          // turn on bubble effect
           }
           break;
           case ETileType.SPIDER:
           Spell.UpdateSprites( this );
           Body.Sprite2.gameObject.SetActive( false );
           if( Control.SpiderAttackBlockPhase == 1 )
               Body.Sprite2.gameObject.SetActive( true );
           break;
           case ETileType.SCORPION:
           Spell.UpdateSprites( this );
           //for( int i = 0; i < 8; i++ )
           //if( Body.HasBaby[ i ] )
           //if( Body.Sp[ i ].Type != ESpiderBabyType.ITEM )
           //{
           //    Quaternion qn = Util.GetRotationToPoint( Body.BabySprite[ i ].transform.position, G.Hero.transform.position );
           //    Body.BabySprite[ i ].transform.rotation = Quaternion.RotateTowards( Body.BabySprite[ i ].transform.rotation, qn, Time.deltaTime * 100 );
           //    qn = Util.GetRotationToPoint( Body.BabySprite[ i + 8 ].transform.position, G.Hero.transform.position );
           //    Body.BabySprite[ i + 8 ].transform.rotation = Quaternion.RotateTowards( Body.BabySprite[ i + 8 ].transform.rotation, qn, Time.deltaTime * 100 );
           //    qn = Util.GetRotationToPoint( Body.BabySprite[ i + 16 ].transform.position, G.Hero.transform.position );
           //    Body.BabySprite[ i + 16 ].transform.rotation = Quaternion.RotateTowards( Body.BabySprite[ i + 16 ].transform.rotation, qn, Time.deltaTime * 100 );
           //}
           break;

           case ETileType.FOG:
           if( Map.I.ElectrifiedFogList.Contains( Control.RaftGroupID ) == false )
           {
               Unit orb = Map.I.GetUnit( ETileType.ORB, Pos );
               if( orb )
               {
                   List<Unit> rl = Controller.GetFogList( Control.RaftGroupID );
                   int idd = Random.Range( 0, rl.Count );
                   LightningBoltScript fx = Map.I.CreateLightiningEffect( orb, rl[ idd ], "Electric Fog" );
                   fx.SortDestination( Control.RaftGroupID );
                   rl[ idd ].Control.BoltEffect = fx;
                   Map.I.ElectrifiedFogList.Add( Control.RaftGroupID );
               }
           }

           Control.TimeforNextEffect -= Time.deltaTime;
           if( Control.BoltEffect )
           if( Control.TimeforNextEffect <= 0 ||
               Control.BoltEffect.EndUnit == null )
           {
               Control.TimeforNextEffect = .2f;
               Control.BoltEffect.SortDestination( Control.RaftGroupID );
           }
           break;
           case ETileType.PLAGUE_MONSTER:
           Map.I.Farm.UpdatePlagueMonster( this );
           if( Body.GraphicsInitialized == false ) 
               InitPlagueMonster();
           break;

           case ETileType.TOWER:
           Control.UpdateTowerRotation();
           break;

           case ETileType.PROJECTILE:
           if( Control.ProjectileType == EProjectileType.BOOMERANG )           
           {
               float z = Spr.transform.eulerAngles.z + ( Time.deltaTime * 665 );
               if( z > 360 ) z -= 360;
               Spr.transform.eulerAngles = new Vector3( 0, 0, z );
           }
           else
           if( Control.ProjectileType == EProjectileType.FIREBALL )
               {
                   Map.I.FireBallCount++;
                   Unit mine = Map.GFU( ETileType.MINE, Pos );
                   if( mine )
                   if( mine.Body.MineType != EMineType.TUNNEL )
                   if( mine.Body.MineType != EMineType.SHACKLE )
                   if( mine.Body.MineType != EMineType.BRIDGE )
                   {
                       Map.I.CreateExplosionFX( mine.Pos );                                                       // Explosion FX                    
                       MasterAudio.PlaySound3DAtVector3( "Explosion 1", G.Hero.Pos );
                       Map.Kill( this );
                       if( G.HS.FireballCanMine )
                           Map.Kill( mine );
                   }
               }
           break;

           case ETileType.VINES:
           UpdateVineLinks();                                                                                                 // Updates vine connections around
           Map.I.UpdateVineFire( this );                                                                                      // Updates vine fire ignition

           if( Map.I.TurnFrameCount == 1 )
           {
               if( Controller.MovedVineList.Contains( this ) )
                   Map.I.CheckVineIgnition( this );                                                                           // Checks for vine ignition after being moved

               for( int i = 0; i < 8; i++ )
               {
                   Vector2 tg = Pos + Manager.I.U.DirCord[ i ];
                   Unit tgun = Map.GFU( ETileType.VINES, tg );                                                                // Finds recent moved vine around this vine to ignite it
                   if( tgun != null )
                       if( Controller.MovedVineList.Contains( tgun ) )
                           Map.I.CheckVineIgnition( this );
               }
           }                    
           break;

           case ETileType.SPIKES:
           Spr.color = Color.white;           
           int old = Spr.spriteId;      
           if( Variation == 1 && MeleeAttack.SpeedTimeCounter >= Control.SpikeFreeStepTime )               
               Spr.spriteId = 335;
           else
               Spr.spriteId = 334;
           if( Variation == 3 ) 
               Spr.spriteId = 335;                                                       // always active    

           if( old != Spr.spriteId )
           if( Variation == 1 )
           {
               if( Map.I.ForceHideVegetation == false )
                   Map.I.CreateExplosionFX( Pos, "Smoke Cloud", "" );                                               // Smoke Cloud FX
                MasterAudio.PlaySound3DAtVector3( "Trap 1", transform.position, 0.6f );                             // Spike Sound Fx
                if( Md && Md.RandomizeArrowDir > 0 )
                {
                    Unit arrow = Map.I.GetUnit( Pos, ELayerType.GAIA2 );
                    if( arrow ) arrow.RotateTo( ( EDirection ) Util.GetRandomDir() );
                }
           }

           if( Control.IsDynamicObject() )
           {
               Graphic.transform.localPosition = Vector3.MoveTowards( Graphic.transform.localPosition, Vector2.zero, Time.deltaTime * 10 );
           }
           break;

           case ETileType.FAN:
           {
               int index = Util.Animate( 0.05f, 2 );
               if( Activated ) Spr.spriteId = 215 + index;
               else Spr.spriteId = 214;
               Map.I.FanCount++;
               break;
           }

           case ETileType.ORB:
           Unit fog = Controller.GetFog( Pos );
           if( fog == null )
           {
               Body.OrbFogStopEnabled = true;
               Body.EffectList[ 0 ].gameObject.SetActive( false );
               Body.EffectList[ 1 ].gameObject.SetActive( false );
               MasterAudio.StopAllSoundsOfTransform( transform );
           }
           else
           {
               List<Unit> rl = Controller.GetFogList( fog.Control.RaftGroupID );
               for( int i = 0; i < rl.Count; i++ )
                   if( rl[ i ].Control.BoltEffect && rl[ i ] != fog )
                   {
                       fog.Control.BoltEffect = rl[ i ].Control.BoltEffect;
                       fog.Control.BoltEffect.StartUnit = this;
                       fog.Control.BoltEffect.Lighting2.StartUnit = this;
                       rl[ i ].Control.BoltEffect = null;
                       break;
                   }

               Body.OrbFogStopEnabled = false;
               Body.EffectList[ 0 ].gameObject.SetActive( true );
               Body.EffectList[ 1 ].gameObject.SetActive( true );
               MasterAudio.PlaySound3DAtTransform( "Electric Orb", transform );
           }
           Body.TweenTime -= Time.deltaTime;
           if( Body.TweenTime <= 0 )
               Body.EffectList[ 2 ].transform.localPosition = Vector3.zero;
           break;

           case ETileType.TRAIL:
           Body.Sprite2.gameObject.SetActive( Body.TrailCoil );
           Graphic.transform.localScale = new Vector3( 1, 1, 1 );
           Body.Sprite3.gameObject.SetActive( false );
           if( Variation > 0 ) Body.Sprite3.gameObject.SetActive( true );
           int tid = 59 + Variation;
           if( tid > 62 ) tid = 62;
           Body.Sprite3.spriteId = tid;
           break;
       }       
    }

    public float GetZFactor( float fact = 1 )
    {
        Vector2 ptt = Helper.I.GetCubeTile( Map.GM() );
        ptt = new Vector2( Pos.x, 28 - Pos.y );
        float zz = -( ptt.y / 1000 ) - ( ptt.x / 10000 );
        return zz * fact;
    }
    public void UpdateVineLinks()
    {
        for( int i = 0; i < 8; i++ )
        {
            Vector2 vnp = Pos + Manager.I.U.DirCord[ i ];
            int inv = Manager.I.U.InvDir[ i ];
            Unit vn = Map.GFU( ETileType.VINES, vnp );
            bool act = false;
            if( vn ) act = true;
            float alpha = Body.PoleSpriteList[ i ].color.a;

            if( Activated ) alpha += Time.deltaTime;
            else alpha -= Time.deltaTime;
            alpha = Mathf.Clamp( alpha, 0, 1 );

            int sel = -1;
            if( act )
            {
                bool same = true;                                                                                             // Same Vine ID?
                if( Control.MergingID != -1 )
                if( vn.Control.MergingID != -1 )
                if( Control.MergingID != vn.Control.MergingID )
                if( Body.CreatedVineList[ i ] == -1 )
                {
                    same = false;                                                                                             // This handles vines with same color but different IDS for creating non connected same color neighbor vines
                }
 
                if( ( Control.VineList.Contains( 0 ) && vn.Control.VineList.Contains( 0 ) && same ) ) sel = 0; else      // set vine color
                if( ( Control.VineList.Contains( 1 ) && vn.Control.VineList.Contains( 1 ) && same ) ) sel = 1; else
                if( ( Control.VineList.Contains( 2 ) && vn.Control.VineList.Contains( 2 ) && same ) ) sel = 2; else
                if( ( Control.VineList.Contains( 3 ) && vn.Control.VineList.Contains( 3 ) && same ) ) sel = 3; else
                if( ( Control.VineList.Contains( 4 ) && vn.Control.VineList.Contains( 4 ) && same ) ) sel = 4; else
                if( ( Control.VineList.Contains( 5 ) && vn.Control.VineList.Contains( 5 ) && same ) ) sel = 5; else
                if( ( Control.VineList.Contains( 6 ) && vn.Control.VineList.Contains( 6 ) && same ) ) sel = 6; 
                else act = false;
            }
            if( vn )
               {
                   if( Body.CreatedVineList[ i ] == -1 && vn && vn.Body.CreatedVineList[ inv ] == -1 )                       // Only proceed if this was not hero created
                   if( Control.MergingID == -1 || vn == null || vn.Control.MergingID == -1 ||                                // IMPORTANT: Place An ID of size 10 or higher to ignore link check and force a diagonal link
                       Control.MergingID != vn.Control.MergingID || Control.MergingID < 10 )
                   if( act && Util.IsEven( i ) == false )                                                                    // only checks diagonal links
                   {
                       if( CheckDuplicateVineLink( i, EDirection.SE, EDirection.S, EDirection.E, sel ) ) act = false;        // Checks for a duplicate link avoiding extra angular connections
                       if( CheckDuplicateVineLink( i, EDirection.NE, EDirection.N, EDirection.E, sel ) ) act = false;
                       if( CheckDuplicateVineLink( i, EDirection.NW, EDirection.N, EDirection.W, sel ) ) act = false;
                       if( CheckDuplicateVineLink( i, EDirection.SW, EDirection.S, EDirection.W, sel ) ) act = false;
                   }
               }

            if( sel == -1 )
                sel = Body.CreatedVineList[ i ];                                                                          // Adds a connection from hero created vine

            if( act )
            if( Controller.GetRaft( Pos ) == null )                                                                       // Raft exception: it needs to connect forcedly
            if( vn == null ||
            Controller.GetRaft( vn.Pos ) == null )
            {
                if( Body.CreatedVine )                                                                                    // This part of the code handles removal of unwanted connections to vines created by hero
                if( vn && vn.Body.CreatedVine == false )
                if( vn && vn.Body.CreatedVineList[ inv ] == -1 ) act = false;                                             // dest vine 

                if( Body.CreatedVineList[ i ] == -1 )                                                                     // source vine
                if( vn && vn.Body.CreatedVine ) act = false;
            }

            if( Control.ForceVineLink[ i ] != -1 ) 
              { act = true; sel = Control.ForceVineLink[ i ]; }                                                           // Force creation of link by ori arrows

            Control.VineColorList[ i ] = sel;
            Body.PoleSpriteList[ i ].color = Map.I.GetVineColor( sel, alpha );
            Body.PoleSpriteList[ i ].gameObject.SetActive( act );
        }
    }

    private bool CheckDuplicateVineLink( int dr, EDirection mydr, EDirection dr1, EDirection dr2, int vcol )
    {
        if( Body.isCreatedVine() ) return false;
        if( dr != ( int ) mydr ) return false;

        int inv1 = ( int ) Util.GetInvDir( dr1 );
        int inv2 = ( int ) Util.GetInvDir( dr2 );      

        Unit vn = Map.GFU( ETileType.VINES, Pos + Manager.I.U.DirCord[ ( int ) mydr ] );
        Unit vn1 = Map.GFU( ETileType.VINES, Pos + Manager.I.U.DirCord[ ( int ) dr1 ] );                                                 // checks ortho connection 1
 
        if( vn1 && vn )
        if( ( vn.Control.VineList.Contains ( vcol ) && vn.Body.isCreatedVine()  == false ) )
        if( ( vn1.Control.VineList.Contains( vcol ) && vn1.Body.isCreatedVine() == false ) ) return true;

        Unit vn2 = Map.GFU( ETileType.VINES, Pos + Manager.I.U.DirCord[ ( int ) dr2 ] );                                                 // check ortho connection 2

        if( vn2 && vn )
        if( ( vn.Control.VineList.Contains( vcol  ) && vn.Body.isCreatedVine() == false ) )
        if( ( vn2.Control.VineList.Contains( vcol ) && vn2.Body.isCreatedVine() == false) ) return true;
        return false;
    }

    public int GetMineSprite( EMineType type )
    {
        switch( type )
        {
            case EMineType.ROUND: return 226;
            case EMineType.SQUARE: return 227;
            case EMineType.DOUBLE: return 228;
            case EMineType.UNITARY: return 225;
            case EMineType.DIRECTIONAL: return 224;
            case EMineType.SHACKLE: return 194;
            case EMineType.SPIKE_BALL: return 193;
            case EMineType.SUSPENDED: return 264;
            case EMineType.LADDER: return 266;
            case EMineType.BRIDGE: return 265;
            case EMineType.INDESTRUCTIBLE: return 229;
            case EMineType.RED: return 244;     
        }
        return -1;
    }

    private void UpdateBuoyAnimation()
    {
        float amplitude = 0.04f;
        float frequency = 1.2f;
        Vector3 tempPos = Control.NotAnimatedPosition;
        tempPos.y += Mathf.Sin( Time.fixedTime * Mathf.PI * frequency ) * amplitude;
        Graphic.transform.position = tempPos;
    }
    public void SetLevelText( string txt )
    {
        if( Map.I.Revealed[ ( int ) Pos.x, ( int ) Pos.y ] == false )
        {
            if( LevelTxt.gameObject.activeSelf )
                LevelTxt.gameObject.SetActive( false );
        }
        else
        {
            if( LevelTxt.gameObject.activeSelf == false )
                LevelTxt.gameObject.SetActive( true );
        }
        LevelTxt.text = txt;
    }

    public float GetPlatformSpeedAttackBonus()
    {
        Unit trap = Map.I.GetUnit( ETileType.TRAP, G.Hero.Pos );
        Unit ftrap = Map.I.GetUnit( ETileType.FREETRAP, G.Hero.Pos );
        Unit mud = Map.I.GetMud( G.Hero.Pos );
        if( Map.I.RM.HeroSector == null ||
            Map.I.RM.HeroSector.Type == Sector.ESectorType.GATES )
            return Map.I.PlatformAttackSpeedBonus = 0;

        if( ftrap != null || mud != null )
            return Map.I.PlatformAttackSpeedBonus;

        if( trap == null && ftrap == null && mud == null )
            return Map.I.PlatformAttackSpeedBonus = 0;            

        float ps = ( ( Map.I.ConsecutivePlatformSteps - 1 ) *
        Map.I.RM.RMD.PerPlatformStepAttackSpeedBonus );
        if( Map.I.ConsecutivePlatformSteps <= 1 ) ps = 0;
        Map.I.PlatformAttackSpeedBonus = Map.I.RM.RMD.PlatformAttackSpeedBonus + ps;
        return Map.I.PlatformAttackSpeedBonus;
    }
    public void UpdateFogDirty()
    {
        if( ValidMonster == false ) return;
        Unit fog = Controller.GetFog( Pos );
        if( fog )
        {
            if( Map.I.DirtyFogList.Contains( fog.Control.RaftGroupID ) == false )
                Map.I.DirtyFogList.Add( fog.Control.RaftGroupID );
        }
    }
    static Unit[] RedimensionarArraySeNecessario( Unit[] array, int indiceNecessario )
    {
        if( indiceNecessario < array.Length )
            return array; // Já cabe

        int novoTamanho = indiceNecessario + 1;
        Unit[] novoArray = new Unit[ novoTamanho ];

        // Copia os objetos já existentes
        for( int i = 0; i < array.Length; i++ )
        {
            novoArray[ i ] = array[ i ];
        }
        return novoArray;
    }
    public EDirection InvDr()
    {
        return Util.GetInvDir( Dir );
    }

    internal bool IsPushAble()
    {
        if( TileID == ETileType.MINE )
        {
            if( Body.MineType == EMineType.BRIDGE ) return false;
            if( Body.MineType == EMineType.TUNNEL ) return false;
        }
        return true;
    }
    public bool CheckFirepitLogBlock( Vector2 from, Vector2 to )
    {
        if( Body.FireIsOn ) return false;
        EDirection dr = Util.GetTargetUnitDir( from, to );
        bool block = false;
        for( int dd = 0; dd < 8; dd++ )
        if( Body.WoodAdded[ ( int ) dd ] == true )                                                      // Fire log block
            {
                List<ETileType> ex2 = new List<ETileType>();
                ex2.Add( ETileType.ARROW ); ex2.Add( ETileType.WATER ); 
                ex2.Add( ETileType.PIT ); ex2.Add( ETileType.LAVA );
                Vector2 logfr = Pos + Manager.I.U.DirCord[ dd ];
                Vector2 logto = logfr + Manager.I.U.DirCord[ ( int ) dr ];
                bool logres = Util.CheckBlock( logfr, logto, this,
                true, true, false, false, false, false, ex2 );
                if( logres == true )
                {
                    block = true;
                    Message.RedMessage( "Log Block!" );
                }
            }
        return block;
    }
}
 














