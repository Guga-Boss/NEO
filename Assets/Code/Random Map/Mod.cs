using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

#region Enums
public enum EModType
{
    NONE = -1, MOD_1 = 0, MOD_2, MOD_3, MOD_4,
    MOD_5, MOD_6, MOD_7, MOD_8, MOD_9, MOD_10, MOD_11, MOD_12,
    MOD_13, MOD_14, MOD_15, MOD_16, MOD_17, MOD_18, MOD_19,
    MOD_20, MOD_21, MOD_22, MOD_23, MOD_24, MOD_25, MOD_26,
    MOD_27, MOD_28, MOD_29, MOD_30, MOD_31, MOD_32
}
public enum EV
{
    NONE = -1, UnitBaseLevel = 0, Resting, UnitLives, RealtimeSpeed, WaitTime,
    RestingDistance, FireON, ResourceAmount, IsTiny, void_1, TickMoveList,
    TickBasedMovement, HitPointsRatio, IsBoss, BoulderPunchPower, IsWorking, TouchCount,
    BaseWaspTotSpawnTimer = 100,
    WaspSpawnInflationPerTile,
    ExtraWaspSpawnTimerPerTile,
    ExtraWaspSpawnInflationPerSec,
    MaxSpawnCount,
    WaspDamage,
    MotherWaspRadius,
    DynamicObjectMoveList, DynamicObjectOrientationList, DynamicObjectJumpList,
    DynamicObjectDirectionList, RandomMoveTurnList, RandomDirTurnList,
    MovementSorting, OrientationSorting, DynamicMaxSteps,
    ResourceOperation, ResourcePersist,
    FrontalRebound = 300,
    TotHp, MeleeAttack, RaftGroupID, RaftJointList, DynamicRaftMoveList, InitialDirection,
    BaseMiningChance, SortMiningChanceFactor, SortMiningChanceRandomList, MiningPrize,
    MiningBonusType, MiningBonusAmount, Direction, ResourceType,
    WaterObjectType, BaseTotalRespawnTime, BaseTotalRespawnAmount, RandomizePositionOnRespawn,
    FishSwimType, RandomDirection, RandomizePositionOnPoleStep, FishingPowerFactor, FishRedLimit,
    FishGreenLimit, FishingBonusAmount,
    HerbColor, HerbType, HerbBonusAmount, RandomizableHerb,
    FlyingAction,
    FlyingActionTarget,
    FlyingActionVal, WhiteGateGroup, AvailableFireHits, RangedAttack, MiniDomeTimer, BabyList,
    VsRoachAttDecreasePerBaby, VsHeroRoachkAttackIncreasePerBaby,
    FixedRestingDirection, BaseMeleeShield, BaseMissileShield, MeleeAttackTotalTime,
    ResTrigger, OptionalMonster, SpriteColor, BonusItemList, BonusItemAmountList, BonusItemChanceList,
    MinFishSpeed, MaxFishSpeed, InitialForcedFrontalMovementFactor, FlyingActionLoopCount, RestingDirectionIsSameAsHero,
    FlyingActionTotalTime,
    FlyingActionSeedCost, PlayAnimation, InflictedDamageRate, void4, MineBonuses, MineType,
    void2, RandomMineType, RandomMineTypeFactor, MineLeverChance,
    MineLeverDirection, void6, void5, RedRoachBabyList, MineLeverChainTarget,
    ShieldedWaspChance,
    WaitAfterObstacleCollision,
    OverFishSecondsNeededPerUnit,
    OrientatorDirection, FishingPole,
    FishAction, InfectedChance,
    RangedAttackRange,
    RangedAttackTotalTime,
    Vine_Type,
    Variation,
    FlyingSpeed
}

public enum EOrientation
{
    NONE = 0, RIGHT_X1 = -1, RIGHT_X2 = -2, RIGHT_X3 = -3, LEFT_X1 = +1, LEFT_X2 = +2, LEFT_X3 = +3, INVERT = 4
}
public enum MyBool
{
    DONT_CHANGE = 0, FALSE, TRUE
}

public enum EResourceOperation
{
    NONE = 0, ADD, SET
}

public enum ECustomMoveType
{
    NONE = 0, STILL, RANDOM, STANDARD, AROUND_HERO, AROUND_HERO_X2
}
public enum EDynamicObjectSortingType
{
    NONE = 0, SEQUENCE, RANDOM,
    SEQUENCE_BASED_ON_ORIGINAL_DIR, SEQUENCE_CHANGE_ON_OBSTACLE
}
public enum EResTriggerType
{
    NONE = -1,
    SWITCH_MOVE_MODE
}

public enum EOrientatorEffect
{
    NONE = -1, SetDirection, HookPushSteps, ChainConnect,
    Resting_Radius, Unit_Lives, AddRandomBabyNum, SetRandomBabyNum,
    AddBabyToDir, Wake_Up_Group, Move_Speed,
    ResourceAmount, SetLeverDirection, Infected_Radius, Fishing_Pole_Level, UpMineDirection,
    Mini_Dome_Total_Time, Unit_HP,
    BabyAmount, WaspBreedSpeed,
    WakeUpTime, Resource_Waste_Time, AddBonusToPole,
    Item_Type, Set_Pole_Direction,
    Res_Trigger, Melee_Attack_Base_Damage, Mine_Type, Mine_Item_Type, UpMine_Type, Vine_Type,
    Vine_Link, Variation, Move_Speed_Start_Time, RAttack_Speed_Start_Time, MAttack_Speed_Start_Time,
    Spike_Free_Step_Time, Melee_Attack_Total_Time, FlyingSpeed, Vine_ID,
    PoleBonusType, PoleBonusCondition, PoleBonusVal1, PoleBonusVal2, Mine_Chance, Mine_Bonus_Chance,
    MineBonusType, MineBonusCondition, MineBonusVal1, MineBonusVal2, MineBonusVal3, MineBonusVal4,
    Mine_Item_Amount, MineBonusDir, Toggle_Tick_Movement, Res_Persist_Times, FlyingSpeedFactor,
    Move_Speed_Factor, PoleBonusAvailableAngle, Set_Butcher_Baby_Item,
    Set_Butcher_Scope, Set_Butcher_Bonus_Factor,
    RotationSpeed, Set_Butcher_Bonus_Factor_Multiplier, Set_Raft_Joint,
    Resource_Slot,
    QQMissHeroDamage, QQMinDamage, QQMaxDamage
}

public enum EOriPointDir
{
    NONE = -1, NOWHERE, ME, AWAY
}

public enum ESpiderBabyType
{
    ITEM = -2, NONE = -1, SPIDER, COCOON, BOTH,
    SCORPION = 100, SCARAB, 
    THROWING_AXE = 200, HERO_SHIELD, BONE, SPEAR,
    BAMBOO, HOOK_CW, HOOK_CCW, MUSHROOM_POTION, 
    EMPTY_TILE, TORCH, DOUBLE_ATTACK, WEBTRAP, ATTACK_BONUS
}

#endregion

public class Mod : SerializedMonoBehaviour 
{
    #region Buttons
    #if UNITY_EDITOR
    public static int CurrentChosenMod = 0;
    [HorizontalGroup( "Split", 0.5f )]
    [Button( "Update Mods", ButtonSizes.Large ), GUIColor( 0, 1f, 0 )]
    public void Button2()
    {
        GameObject ob = GameObject.Find( "Farm" );
        Farm f = ob.GetComponent<Farm>();
        f.UpdateListsCallBack();
    }

    [HorizontalGroup( "Split", 0.5f )]
    [Button( "Edit Quest", ButtonSizes.Large ), GUIColor( 1f, 0.52f, 0.1f )]
    public void Button9()
    {
        MapSaver ms = GameObject.Find( "Areas Template Tilemap" ).GetComponent<MapSaver>();
        ms.EditQuestDataCallBack();
    }

    [HorizontalGroup( "Split", 0.5f )]
    [Button( "Prev", ButtonSizes.Large ), GUIColor( 1, 1f, 0 )]
    public void PrevButton()
    {
        Mod[] md = transform.parent.transform.GetComponentsInChildren<Mod>();
        if( --CurrentChosenMod < 0 )
            CurrentChosenMod = md.Length - 1;
        Selection.activeGameObject = md[ CurrentChosenMod ].gameObject;
    }

    [HorizontalGroup( "Split", 0.5f )]
    [Button( "Next", ButtonSizes.Large ), GUIColor( 1, 1f, 0 )]
    public void NextButton()
    {
        Mod[ ] md = transform.parent.transform.GetComponentsInChildren<Mod>();
        if( ++CurrentChosenMod == md.Length )
            CurrentChosenMod = 0;
        Selection.activeGameObject = md[ CurrentChosenMod ].gameObject;
    }
    #endif
    #endregion
    #region Variables
    public EModType ModNumber = EModType.MOD_1;
    public string ModDescription = "";
    [TabGroup( "Layer" )]
    public List<ETileType> RestrictToUnitType = null;
    [TabGroup( "Layer" )]
    public bool Gaia = true;
    [TabGroup( "Layer" )]
    public bool Gaia2 = true;
    [TabGroup( "Layer" )]
    public bool Monster = true;
    [TabGroup( "Layer" )]
    public bool Raft = true;
    [TabGroup( "Layer" )]
    public List< EOrientatorEffect> OrientatorEffects;
    [TabGroup( "Layer" )]
    public List<float> OrientatorTable1 = null;                  // Table for Main Orientator effect
    [TabGroup( "Layer" )]
    public List<float> OrientatorTable2 = null;                  // Table for Second Orientator effect
    [TabGroup( "Layer" )]
    public List<float> OrientatorTable3 = null;                  // Table for Third Orientator effect
    [TabGroup( "Layer" )]
    public List<float> OrientatorTable4 = null;                  // Table for fourth Orientator effect
    [TabGroup( "Layer" )]
    public List<float> OrientatorTable5 = null;                  // Table for fifth Orientator effect
    [TabGroup( "Layer" )]
    public List<float> OrientatorTable6 = null;                  // Table for sixth Orientator effect
    [TabGroup( "Layer" )]
    public List<ItemType> OrientatorItemTable1 = null;            
    [TabGroup( "Layer" )]
    public List<EResTriggerType> ResourceTriggerTypeList = null;
    [TabGroup( "Layer" )]
    public List<EMineType> MineTypeList = null;
    [TabGroup( "Layer" )]
    public List<EPoleBonusEffType> PoleBonusEffList = null;
    [TabGroup( "Layer" )]
    public List<EPoleBonusCnType> PoleBonusCnList = null;
    [TabGroup( "Layer" )]
    public List<EMineBonusEffType> MineBonusEffList = null;
    [TabGroup( "Layer" )]
    public List<EMineBonusCnType> MineBonusCnList = null;
    [TabGroup( "Unit" )]
    public bool OptionalMonster = false;
    [TabGroup( "Unit" )]
    public EDirection Direction = EDirection.NONE;
    [TabGroup( "Unit" )]
    public EDirection FixedRestingDirection = EDirection.NONE;
    [TabGroup( "Unit" )]
    public int RandomDirection = 0;
    [TabGroup("Unit")]
    public float UnitBaseLevel = 0;
    [TabGroup("Unit")]
    public float UnitLives = 0;
    [TabGroup( "Unit" )]
    public float Variation = 0;
    [TabGroup("Unit")]
    public float MaxLives = 0;                                   // Below 0 == unlimited
    [TabGroup("Unit")]
    public float HitPointsRatio = 100;
    [TabGroup("Unit")]
    public float TotHp = -1;
    [TabGroup( "Unit" )]
    public float MeleeAttack = -1;
    [TabGroup( "Unit" )]
    public float MeleeAttackTotalTime = -1;
    [TabGroup( "Move" )]
    public List<float> MeleeAttackTotalTimeList = null;
    [TabGroup( "Move" )]
    public List<float> RangedAttackTotalTimeList = null;
    [TabGroup( "Unit" )]
    public float InflictedDamageRate = -1;
    [TabGroup( "Unit" )]
    public float BaseMeleeShield = -1;
    [TabGroup( "Unit" )]
    public float BaseMissileShield = -1;
    [TabGroup("Unit")]
    public float RangedAttack = -1;
    [TabGroup( "Unit" )]
    public float RangedAttackRange = -1;
    [TabGroup( "Unit" )]
    public float RangedAttackTotalTime = -1;
    [TabGroup( "Rest" )]
    public bool Resting = false;
    [TabGroup( "Unit" )]
    public bool FireOn = false;
    [TabGroup( "Unit" )]
    public int AvailableFireHits = -1;
    [TabGroup("Unit")]
    public bool IsTiny = false;
    [TabGroup("Unit")]
    public bool IsWorking = true;
    [TabGroup("Unit")]
    public int TouchCount = 0;
    [TabGroup( "Unit" )]
    public Vector2 WhiteGateGroup = new Vector2( -1, -1 );
    [TabGroup( "Unit" )]
    public Color SpriteColor = new Color( 0, 0, 0, 0 );
    [TabGroup( "Unit" )]
    public float VineBurnTime = .5f;
    [TabGroup( "Unit" )]
    public bool DefaultImmunityShieldState = false;
    [TabGroup( "Res" )]
    public ItemType ResourceType = ItemType.NONE;
    [TabGroup( "Res" )]
    public EResourceOperation ResourceOperation = EResourceOperation.ADD;
    [TabGroup( "Res" )]
    public float ResourceAmount = 1;
    [TabGroup( "Res" )]
    public int ResourceSlot = 1;
    [TabGroup( "Res" )]
    public int ResourcePersistTotalSteps = 1;
    [TabGroup( "Res" )]
    public int ResourceWasteTime = -1;
    [TabGroup( "Res" )]
    public List<float> MiniDomeTotalTimeList = null;
    [TabGroup( "Res" )]
    public List<ItemType> BonusItemList = null;
    [TabGroup( "Res" )]
    public List<float> BonusItemAmountList = null;
    [TabGroup( "Res" )]
    public List<float> BonusItemChanceList = null;
    [TabGroup( "Move" )]
    public float RealtimeSpeed = 0;
    [TabGroup( "Move" )]
    public List<float> RealtimeSpeedList = null;
    [TabGroup( "Move" )]
    public float WaitTime = 0;
    [TabGroup( "Fly" )]
    public float BaseWaspTotSpawnTimer = 30;
    [TabGroup( "Fly" )]
    public float WaspSpawnInflationPerTile = .1f;
    [TabGroup( "Fly" )]
    public float ExtraWaspSpawnTimerPerTile = 2;
    [TabGroup( "Fly" )]
    public float ExtraWaspSpawnInflationPerSec = 1f;
    [TabGroup( "Fly" )]
    public int MaxSpawnCount = 20;
    [TabGroup( "Fly" )]
    public float WaspDamage = 2;
    [TabGroup( "Fly" )]
    public int MotherWaspRadius = 5;
    [TabGroup( "Fly" )]
    public int SpawnCount = 0;
    [TabGroup( "Fly" )]
    public float ShieldedWaspChance = 0;
    [TabGroup( "Fly" )]
    public int StartShieldedWasps = 0;
    [TabGroup( "Fly" )]
    public int MinShieldedWasps = 0;
    [TabGroup( "Fly" )]
    public int MaxShieldedWasps = 0;
    [TabGroup( "Fly" )]
    public float CocoonWaspChance = 0;
    [TabGroup( "Fly" )]
    public int StartCocoonWasps = 0;
    [TabGroup( "Fly" )]
    public int MinCocoonWasps = 0;
    [TabGroup( "Fly" )]
    public int MaxCocoonWasps = 0;
    [TabGroup( "Fly" )]
    public float EnragedWaspChance = 0;
    [TabGroup( "Fly" )]
    public int StartEnragedWasps = 0;
    [TabGroup( "Fly" )]
    public int MinEnragedWasps = 0;
    [TabGroup( "Fly" )]
    public int MaxEnragedWasps = 0;
    [TabGroup( "Fly" )]
    public float EnragedWaspSpawnPercentAdd = 1;
    [TabGroup( "Fly" )]
    public float EnragedWaspSpawnLimitAdd = 1;
    [TabGroup( "Fly" )]
    public float EnragedWaspExtraSpawns = 1;
    [TabGroup( "Fly" )]
    public float EnragedWaspExtraBreedingSpeed = 5;
    [TabGroup( "Fly" )]
    public int EnragedWaspUpdates = 2;
    [TabGroup( "Fly" )]
    public bool WaspCatchUp = true;
    [TabGroup( "Fly" )]
    public float FlyingSpeed = -1;
    [TabGroup( "Fly" )]
    public float FlyingSpeedFactor = 100;
    [TabGroup( "Fly" )]
    public float FlyingRotationSpeed = -1;
    [TabGroup( "Fly" )]
    public int StartFlyingPhase = -1;
    [TabGroup( "Fly" )]
    public string PlayAnimation = "";
    [TabGroup( "Fly" )]
    public int ProjectileType = 2;
    [TabGroup( "Fly" )]
    public float ProjectileSpeed = 10;
    [TabGroup( "Fly" )]
    public float ProjectileRotationSpeed = 150;
    [TabGroup( "Fly" )]
    public List<float> MonsterDestroyHeroShieldChance = null;         // Chance for a monster to destroy hero shield when attacking depending on the amount of active shields
    [TabGroup( "Fly" )]
    public List<float> ProjectileDestroyHeroShieldChance = null;      // Chance for a bolt to destroy hero shield when attacking depending on the amount of active shields
    [TabGroup( "Fly" )]
    public float QQMinDamage = 2;
    [TabGroup( "Fly" )]
    public float QQMaxDamage = 10;
    [TabGroup( "Fly" )]
    public float QQMissHeroDamage = 0;
    [TabGroup( "Fly" )]
    public float QQDamageCurve = 1.5f;
    [TabGroup( "Boulder" )]
    public bool FrontalRebound = true;
    [TabGroup( "Boulder" )]
    public bool SpikedBoulder = false;
    [TabGroup( "Rest" )]
    public int RestingDistance = -1;
    [TabGroup( "Rest" )]
    public int WakeUpGroup = -1;
    [TabGroup( "Rest" )]
    public int WakeUpTime = -1;
    [TabGroup( "Rest" )]
    public bool RestingDirectionIsSameAsHero = false;
    [TabGroup( "Move" )]
    public MyBool TickBasedMovement = MyBool.DONT_CHANGE;
    [TabGroup( "Move" )]
    public MyBool SteppingBasedMovement = MyBool.DONT_CHANGE;
    [TabGroup( "Move" )]
    public List<int> TickMoveList = null;
    [TabGroup( "Move" )]
    public int InitialForcedFrontalMovementFactor = 0;
    [TabGroup( "Move" )]
    public bool ResetHuggerMoveAfterStepOut = false;
    [TabGroup( "Move" )]
    public List<ECustomMoveType> CustomMoveType;
    [TabGroup( "Move" )]
    public List<float> CustomMoveFactor;
    [TabGroup( "Move" )]
    public float SpikeFreeStepTime = 0;
    [TabGroup( "Unit" )]
    public MyBool IsBoss = MyBool.DONT_CHANGE;
    [TabGroup( "Unit" )]
    public float InfectedChance = 0; 
    [TabGroup( "Unit" )]
    public int InfectedRadius = 1; 
    [TabGroup( "Boulder" )]
    public float BoulderPunchPower = -1;
    [TabGroup( "Unit" )]
    public List<float> AddRandomMonsterStat = new List<float>();
    [HideInInspector]
    public List<Unit> ModdedMonstersList = null;
    [HideInInspector]
    public List<Unit> ModdedGaia2List = null;
    [TabGroup( "Unit" )]
    public Vector2 KeepTileHidden = new Vector2( -1, -1 );
    public static List<Vector2> KeepHiddenTileList = null;
    [TabGroup( "Unit" )]
    public string BoolToogle1VarName = "";
    [TabGroup( "Unit" )]
    public string BoolToogle2VarName = "";
    [TabGroup( "Unit" )]
    public int RandomizeArrowDir = 0;
    [TabGroup( "Dynamic" )]
    public bool DynamicObject = false;
    [TabGroup( "Dynamic" )]
    public EDynamicObjectSortingType MovementSorting = EDynamicObjectSortingType.SEQUENCE;
    [TabGroup( "Dynamic" )]
    public List<EActionType> DynamicObjectMoveList = null;
    [TabGroup( "Dynamic" )]
    public List<Vector2> DynamicObjectJumpList = null;
    [TabGroup( "Dynamic" )]
    public bool WaitAfterObstacleCollision = true;
    [TabGroup( "Dynamic" )]
    public EDynamicObjectSortingType OrientationSorting = EDynamicObjectSortingType.SEQUENCE;
    [TabGroup( "Dynamic" )]
    public bool RandomizeRotation = false;
    [TabGroup( "Dynamic" )]
    public List<EOrientation> DynamicObjectOrientationList = null;
    [TabGroup( "Dynamic" )]
    public List<EDirection> DynamicObjectDirectionList = null;
    [TabGroup( "Dynamic" )]
    public List<int> RandomMoveTurnList = null;
    [TabGroup( "Dynamic" )]
    public List<int> RandomDirTurnList = null;
    [TabGroup( "Dynamic" )]
    public int DynamicMaxSteps = 0;
    [TabGroup( "Dynamic" )]
    public EDirection InitialDirection = EDirection.NONE;
    [TabGroup( "Raft" )]
    public bool BrokenRaft = false;
    [TabGroup( "Raft" )]
    public int RaftGroupID = -1;
    [TabGroup( "Raft" )]
    public bool ShowJointMatrix = false;
    [TabGroup( "Raft" )]
    [ShowIf( "ShowJointMatrix", true )]
    [OdinSerialize]
    [TableMatrix( SquareCells = true )]
    public bool[ , ] RaftJointList = new bool[ 3, 3 ];  
    [TabGroup( "Mn" )]
    public float VsRoachHeroAttBonusPerBaby = 30;
    [TabGroup( "Mn" )]
    public float VsHeroRoachAttackBonusPerBaby = 30;
    [TabGroup( "Mn" )]
    public List<EDirection> RedRoachBabyList = null;
    [TabGroup( "Mn" )]
    public bool ShowBabies = false;
    [TabGroup( "Mn" )]
    [ShowIf( "ShowBabies", true )]
    [OdinSerialize]
    [TableMatrix( SquareCells = true )]
    public bool[ , ] BabyList = new bool[ 3, 3 ];
    [TabGroup( "Mn" )]
    public int NumRandomBabies = 0;
    [TabGroup( "Mn" )]
    public List<EDirection> RandomBabyPosList = null;
    [TabGroup( "Mn" )]
    public List<float> RandomBabyFactorList = null;
    [TabGroup( "Mn" )]
    public List<EAlgaeBabyType> BabyTypeList = null;
    [TabGroup( "Mn" )]
    public List<float> BabyTypeFactorList = null;
    [TabGroup( "Mn" )]
    public EAlgaeBabyType ForceFirstBabyType = EAlgaeBabyType.NONE;
    [TabGroup( "Mn" )]
    public List<ESpiderBabyType> SpiderBabyTypeList = null;
    [TabGroup( "Mn" )]
    public float BabyDisableTotTime = 0;
    [TabGroup( "Mn" )]
    public float BabyDisableChance = 0;
    [TabGroup( "Mn" )]
    public ESpiderBabyType PointAwaySpiderType = ESpiderBabyType.SPIDER;
    [TabGroup( "Mn" )]
    public ESpiderBabyType PointToMeSpiderType = ESpiderBabyType.NONE;
    [TabGroup( "Mn" )]
    public List<ItemType> NeighborModItemList;
    [TabGroup( "Mn" )]
    public List<ESpiderBabyType> NeighborSpellTypeList;
    [TabGroup( "Mn" )]
    public List<ItemType> NeighborSpellItemList;
    [TabGroup( "Mn" )]
    public float UnblockedFrogRestorationFactor = 0;
    [TabGroup( "Mn" )]
    public int MaxFrogTurnDelay = 3;                                             // Max turns frog can be delayed in stepping mode
    [TabGroup( "Mn" )]
    public bool UnblockedFrogSpawnsBaby = false;
    [TabGroup( "Mn" )]
    public List<int> VineTypeList;
    [TabGroup( "Mn" )]
    public bool AddAlgaeToSpike = false;
    [TabGroup( "Mine" )]
    [Space( 10 )]
    public EMineType MineType = EMineType.DIRECTIONAL;
    [TabGroup( "Mine" )]
    public EMineType UpMineType = EMineType.NONE;
    [Space( 10 )]
    [TabGroup( "Mine" )]
    public List<EMineType> RandomMineType;
    [TabGroup( "Mine" )]
    public List<float> RandomMineTypeFactor;
    [TabGroup( "Mine" )]
    public float BaseMiningChance = 50;
    [TabGroup( "Mine" )]
    public float SortMiningChanceFactor = 0;
    [TabGroup( "Mine" )]
    public List<float> SortMiningChanceRandomList = new List<float>();
    [TabGroup( "Mine" )]
    public List<float> SortMiningChanceRandomFactorList = new List<float>();
    [Header( "Mining Prize:" )]
    [TabGroup( "Mine" )]
    public ItemType DefaultMiningPrize = ItemType.NONE;
    [TabGroup( "Mine" )]
    public List<ItemType> MiningPrizeList = null;
    [TabGroup( "Mine" )]
    public List<float> MiningBonusAmountList = null;
    [TabGroup( "Mine" )]
    public List<float> MiningBonusFactor = null;
    [TabGroup( "Mine" )]
    public float MiningBonusChance = 100;
    [Header( "Mine Power-Up:" )]
    [TabGroup( "Mine" )]
    public float SwapperMineChance = 0;
    [TabGroup( "Mine" )]
    public float HoleMineChance = 0;
    [TabGroup( "Mine" )]
    public float RopeConnectedMineChance = 0;
    [TabGroup( "Mine" )]
    public float HammerMineChance = 0;
    [TabGroup( "Mine" )]
    public float ChiselMineChance = 0;
    [TabGroup( "Mine" )]
    public float StickyMineChance = 0;
    [TabGroup( "Mine" )]
    public float DynamiteMineChance = 0;
    [TabGroup( "Mine" )]
    public float CogMineChance = 0;
    [TabGroup( "Mine" )]
    public float ArrowMineChance = 0;
    [TabGroup( "Mine" )]
    public float CannonMineChance = 0;
    [TabGroup( "Mine" )]
    public float MagnetMineChance = 0;
    [TabGroup( "Mine" )]
    public float WheelMineChance = 0;
    [TabGroup( "Mine" )]
    public float JumperMineChance = 0;
    [TabGroup( "Mine" )]
    public float GloveMineChance = 0;
    [TabGroup( "Mine" )]
    public float SpikedMineChance = 0;
    [Header( "Mine Lever:" )]
    [TabGroup( "Mine" )]
    public float MineLeverCogChance = 0;
    [TabGroup( "Mine" )]
    public EDirection MineLeverDirection = EDirection.NONE;
    [TabGroup( "Mine" )]
    public List<EModType> MineLeverChainTargetModList = null;
    [TabGroup( "Mine" )]
    public List<float> MineLeverChainTargetModFactor = null;
    [TabGroup( "Mine" )]
    public List<int> HookPushStepList = new List<int>();
    [TabGroup( "Mine" )]
    public List<float> HookPushStepListFactor = new List<float>();
    [TabGroup( "Mine" )]
    public List<ETileType> CustomChainConnectionAvailability = null;
    [TabGroup( "Mine" )]
    public List<EModType> LimitChainConnectionModList = null;
    [TabGroup( "Mine" )]
    public int MaxChainConnections = 1;
    [TabGroup( "Mine" )]
    public float ClockwiseChainPushChance = 100;
    [Header( "Vault:" )]
    [TabGroup( "Mine" )]
    public EMineBonusCnType MineBonusCnType = EMineBonusCnType.NONE;
    [TabGroup( "Mine" )]
    public EMineBonusEffType MineBonusEffType = EMineBonusEffType.NONE;
    [TabGroup( "Mine" )]
    public float MineBonusVal1 = 0;
    [TabGroup( "Mine" )]
    public float MineBonusVal2 = 0;
    [TabGroup( "Mine" )]
    public float MineBonusVal3 = 0;
    [TabGroup( "Mine" )]
    public float MineBonusVal4 = 0; 
    [TabGroup( "Fish" )]
    public EFishType WaterObjectType = EFishType.NONE;
    [TabGroup( "Fish" )]
    public EFishCatchType FishCatchType = EFishCatchType.RANDOM;
    [TabGroup( "Fish" )]
    public EFishSwimType FishSwimType = EFishSwimType.NONE;
    [TabGroup( "Fish" )]
    public EModType DefaultFishMod = EModType.MOD_1;
    [TabGroup( "Fish" )]
    public float BaseTotalRespawnTime = -1;
    [TabGroup( "Fish" )]
    public int BaseTotalRespawnAmount = -1;
    [TabGroup( "Fish" )]
    public bool RandomizePositionOnRespawn = false;
    [TabGroup( "Fish" )]
    public bool RandomizePositionOnPoleStep = false;
    [TabGroup( "Fish" )]
    public float FishingPowerFactor = 100;
    [TabGroup( "Fish" )]
    public float FishRedLimit = -1;
    [TabGroup( "Fish" )]
    public float FishGreenLimit = -1;
    [TabGroup( "Fish" )]
    public float FishingBonusAmount = -1;
    [TabGroup( "Fish" )]
    public float MinFishSpeed = .5f;
    [TabGroup( "Fish" )]
    public float MaxFishSpeed = 1.2f;
    [TabGroup( "Fish" )]
    public float OverFishSecondsNeededPerUnit = -1;
    [TabGroup( "Fish" )]
    public List<FishActionStruct> FishAction = null;
    [Header( "Water Flower:" )]
    [TabGroup( "Fish" )]
    public float WaterFlowerBaseAttack = 1.0f;            // flower hp decrease rate
    [TabGroup( "Fish" )]
    public float WaterFlowerBonusPerUnit = 20f;           // Rotation speed bonus for each flower
    [TabGroup( "Fish" )]
    public float WaterFlowerRefreshTime = 5f;             // time for flower to be touchable again
    [TabGroup( "Fish" )]
    public float WaterFlowerConsecutiveHitBonus = 25;     // this adds a bonus to every consecutive flower hit
    
    [Header( "Pole Bonus:" )]
    [TabGroup( "Fish" )]
    public EPoleBonusEffType PoleBonusEffType = EPoleBonusEffType.NONE;
    [TabGroup( "Fish" )]
    public EPoleBonusCnType PoleBonusCnType = EPoleBonusCnType.NONE;
    [TabGroup( "Fish" )]
    public float PoleBonusVal1 = 0;
    [TabGroup( "Fish" )]
    public float PoleBonusVal2 = 0;
    [TabGroup( "Fish" )]
    public EFishType PoleBonusFishType = EFishType.NONE;
    [TabGroup( "Fish" )]
    public float FishingPoleExtraTime = 0;
    [TabGroup( "Fish" )]
    public float PoleBonusHPRange = 0;
    [TabGroup( "Herb" )]
    public EHerbColor HerbColor = EHerbColor.NONE;
    [TabGroup( "Herb" )]
    public EHerbType HerbType = EHerbType.NONE;
    [TabGroup( "Herb" )]
    public int HerbBonusAmount = 1;
    [TabGroup( "Herb" )]
    public bool RandomizableHerb = false;
    [TabGroup( "Fly" )]
    public FlyingActionStruct[ ] FlyingAction;
    [TabGroup( "Art" )]
    public  EArtifactDataBase ArtifactType = EArtifactDataBase.ARTIFACT;
    [TabGroup( "Art" )]
    public Artifact.EArtifactLifeTime ArtifactLifetime = Artifact.EArtifactLifeTime.SESSION;
    [TabGroup( "Art" )]
    public float ArtifactCost = 0;
    [TabGroup( "Art" )]
    public Artifact.EMultiplier ArtifactMultiplier = Artifact.EMultiplier.x1;
    [TabGroup( "Art" )]
    public EHeroID TargetHero = EHeroID.ALL_HEROES;
    [Header( "Basic:" )]
    [TabGroup( "Altar" )]
    public List<AltarBonusStruct> InitialAltarBonusList;
    [Header( "Custom Bonuses:" )]
    [TabGroup( "Altar" )]
    public List<AltarBonusStruct> PointToMeAndAwayAltarBonus = null;
    [TabGroup( "Altar" )]
    public List<AltarBonusStruct> AltarPoleBonus = null;
    [TabGroup( "Altar" )]
    public float PoleRotationSpeed = 90;     // deegrees per sec
    [TabGroup( "Altar" )]
    public int PoleStartRotation = 0;
    [TabGroup( "Altar" )]
    public float PoleBumpWaitTime = 100;     // Percentage Relative to full circle
    [TabGroup( "Altar" )]
    public float SpikeDamage = 10;
    [TabGroup( "Altar" )]
    public List<AltarBonusStruct> PoleObjList;
    public static Mod MD;
    public static EOriPointDir PointDir = EOriPointDir.NONE;
    public static EDirection OriDir;

    [System.Serializable]
    public class FlyingActionStruct
    {
        public EFlyingAction FlyingAction;
        [TabGroup( "Target" )]
        public Vector2 Target;
        [TabGroup( "Main" )]
        public float FloatVal;
        [TabGroup( "Main" )]
        public float MaxVal = 0;
        [TabGroup( "Condition" )]
        public bool IgnoreCheck = false;
        [TabGroup( "Condition" )]
        public bool OnlyInit = false;
        [TabGroup( "Condition" )]
        public int LoopCount;
        [TabGroup( "Condition" )]
        public float TotalTime;
        [TabGroup( "Condition" )]
        public float MinimumTime;
        [TabGroup( "Condition" )]
        public float SeedCostPerSecond;
        [TabGroup( "Condition" )]
        public bool ExecuteWithCorn = false;
        [TabGroup( "Condition" )]
        public bool ExecuteWithoutCorn = true;
        [TabGroup( "Condition" )]
        public bool IsPhaseWonPrize = false;
        [TabGroup( "Condition" )]
        public float ExecutionChance = 100;
        [TabGroup( "Val" )]
        public float FlyingMaxSpeed = -1;
        [TabGroup( "Val" )]
        public float FlyingAcceleration = -1;
        [TabGroup( "Val" )]
        public float FlyingRotationSpeed = -1;
        [TabGroup( "Val" )]
        public float SideFlightZigZagDistance = -1;
        [TabGroup( "Corn" )]
        public int CornPickedJumpPhase = -1;
        [TabGroup( "Corn" )]
        public int CornAddJumpPhase = -1;
        [TabGroup( "Corn" )]
        public int CornZeroedJumpPhase = -1;
        [Tooltip("If A certain task is suceeded, the next phases will be exucuted by force during this time.")]
        [TabGroup( "Val" )]
        public float PhaseWonFreePhasesGained = -1;
        [TabGroup( "Ability" )]
        public MyBool FlightResourcePicking = MyBool.DONT_CHANGE;
        [TabGroup( "Ability" )]
        public MyBool FlightOrbStrike = MyBool.DONT_CHANGE;
        [TabGroup( "Ability" )]
        public MyBool FlightBarricadeDestroy = MyBool.DONT_CHANGE;
        [TabGroup( "Ability" )]
        public MyBool FlightWallClash = MyBool.DONT_CHANGE;
        [TabGroup( "Ability" )]
        public string SoundEffect = "";
    }

    #endregion
    public static void StartIt()
    {
        KeepHiddenTileList = null;
        for( int i = 0; i < Map.I.RM.SD.ModList.Length; i++ )
        {
            Map.I.RM.SD.ModList[ i ].ModdedMonstersList = new List<Unit>();
            Map.I.RM.SD.ModList[ i ].ModdedGaia2List = new List<Unit>();
        }
    }

    public static void ApplyMods( int xx, int yy )
    {
        if( KeepHiddenTileList == null ) KeepHiddenTileList = new List<Vector2>();
        for( int i = 0; i < Map.I.RM.SD.ModList.Length; i++ )
        {
            Map.I.RM.SD.ModList[ i ].ApplyMod( xx, yy, i );
        }
    }

    public void ApplyMod( int xx, int yy, int id, Unit forcedUnit = null )
    {
        ETileType keym = ( ETileType )
        Quest.I.Dungeon.Tilemap.GetTile( xx, yy, ( int ) ELayerType.GAIA2  );
        if( Map.GetTileID( ( ETileType ) keym ) != ETileType.DOOR_OPENER   )            // key type mod opener
        if( Map.GetTileID( ( ETileType ) keym ) != ETileType.DOOR_SWITCHER )            // key type mod switcher
        if( Map.GetTileID( ( ETileType ) keym ) != ETileType.DOOR_CLOSER   )            // key type mod closer
        if( Map.GetTileID( ( ETileType ) keym ) != ETileType.DOOR_KNOB     )            // key type mod knob
            keym = ETileType.NONE;

        ETileType modtile = ( ETileType )
        Quest.I.Dungeon.Tilemap.GetTile( xx, yy, ( int ) ELayerType.MODIFIER );                                 // gets mod tile id

        EModType mod = GetMod( modtile ); 
        if( forcedUnit == null )
        if( keym == ETileType.NONE )       
        if( mod < EModType.MOD_1 || mod > EModType.MOD_32 ) return;                                             // mod within bounds?       
        if( forcedUnit == null )
        if( mod != ModNumber ) return;

        //if( KeepTileHidden != new Vector2( -1, -1 ) )
        //for( int y = yy - ( int ) KeepTileHidden.y; y <= yy + ( int ) KeepTileHidden.y; y++ )                 // Keep hidden tile pos list add
        //for( int x = xx - ( int ) KeepTileHidden.x; x <= xx + ( int ) KeepTileHidden.x; x++ )
        //{
        //    KeepHiddenTileList.Add( new Vector2( x, y ) );
        //}

        List<Unit> mnl = new List<Unit>();
        Unit monster = Map.I.GetUnit( new Vector2( xx, yy ), ELayerType.MONSTER );                           

        bool checkMonster = Monster;
        if( monster && monster.TileID == ETileType.RAFT )                                                       // Not raft applying
            if( Raft == false ) return;
            else checkMonster = true;
        
        if ( monster ) mnl.Add( monster );                                                                      // Adds monster to the list
        if ( Map.I.FUnit[ xx, yy ] != null )
        for( int i = 0; i < Map.I.FUnit[ xx, yy ].Count;i++ )
        if ( mnl.Contains( Map.I.FUnit[ xx, yy ][ i ] ) == false ) mnl.Add( Map.I.FUnit[ xx, yy ][ i ] );       // Adds Flying to the list
        
        Unit gaia2 = Map.I.GetUnit( new Vector2( xx, yy ), ELayerType.GAIA2 );
        if( gaia2 )
        {
            if( gaia2.TileID == ETileType.FIRE || gaia2.TileID == ETileType.ARROW    ||
                gaia2.TileID == ETileType.ITEM || gaia2.TileID == ETileType.SAVEGAME || 
                gaia2.TileID == ETileType.SECRET )
                mnl.Add( gaia2 );                                                                               // Adds Gaia to the list
        }

        if( forcedUnit != null )                                                                                // apply mods only to the forced unit
        {
            mnl = new List<Unit>();
            mnl.Add( forcedUnit );
        }

        if( mnl == null ) return;

        for( int i = 0; i < mnl.Count; i++ )
        if ( ( mnl[ i ].UnitType == EUnitType.GAIA  && Gaia == true  && keym == ETileType.NONE ) ||             // Layer restriction
             ( mnl[ i ].UnitType == EUnitType.GAIA2 && Gaia2 == true && keym == ETileType.NONE ) ||
             ( mnl[ i ].UnitType == EUnitType.MONSTER && checkMonster == true ) )
            if( Map.I.RM.ModedUnitlList.Contains( mnl[ i ] ) == false )
        {
            Unit mn = mnl[ i ];
            mn.Md = this;
            MD = this;
            mn.ModID = id;
 
            if ( RestrictToUnitType != null && RestrictToUnitType.Count > 0 )                      // Restrict to unit type
            for( int u = 0; u < RestrictToUnitType.Count; u++ )
            if ( RestrictToUnitType.Contains( mn.TileID ) == false )
                 goto end;

            Map.I.RM.ModedUnitlList.Add( mn );
            mn.Init( EV.Resting, Resting );
            mn.Init( EV.FireON, FireOn );
            mn.Init( EV.AvailableFireHits, AvailableFireHits );
            mn.Init( EV.IsTiny, IsTiny );
            if( UnitBaseLevel > 0 ) mn.Init( EV.UnitBaseLevel, UnitBaseLevel );
            mn.Init( EV.UnitLives, UnitLives );
            mn.Init( EV.Variation, Variation );
            mn.Init( EV.RealtimeSpeed, RealtimeSpeed );
            if( WaitTime > 0 ) mn.Init( EV.WaitTime, WaitTime );
            if( HitPointsRatio != 100 ) mn.Init( EV.HitPointsRatio, HitPointsRatio );
            if( TotHp > 0 ) mn.Init( EV.TotHp, TotHp );
            mn.Init( EV.MeleeAttack, MeleeAttack );
            mn.Init( EV.MeleeAttackTotalTime, MeleeAttackTotalTime );
            mn.Init( EV.RangedAttackTotalTime, RangedAttackTotalTime );
            mn.Init( EV.RangedAttack, RangedAttack );
            mn.Init( EV.RangedAttackRange, RangedAttackRange );
            if( BaseMeleeShield != -1 ) mn.Init( EV.BaseMeleeShield, BaseMeleeShield );
            if( BaseMissileShield != -1 ) mn.Init( EV.BaseMissileShield, BaseMissileShield );

            if( RestingDistance >= 0 ) mn.Init( EV.RestingDistance, RestingDistance );

            if( PlayAnimation != "" )
                mn.Init( EV.PlayAnimation, PlayAnimation );
            mn.Init( EV.ResourceType, ResourceType );
            mn.Init( EV.ResTrigger, ItemType.NONE );
            mn.Init( EV.MineType, ( int ) MineType );
            mn.Init( EV.MineType, MineType );
            mn.Init( EV.ResourceAmount, ResourceAmount );
            if( ResourceOperation != EResourceOperation.NONE )
                mn.Init( EV.ResourceOperation, ResourceOperation );

            mn.Init( EV.ResourcePersist, ResourcePersistTotalSteps );
            if( TickMoveList != null && TickMoveList.Count > 0 )
                mn.Init( EV.TickMoveList, TickMoveList );
            mn.Init( EV.Vine_Type, VineTypeList );
            mn.Init( EV.BaseWaspTotSpawnTimer, BaseWaspTotSpawnTimer );
            mn.Init( EV.WaspSpawnInflationPerTile, WaspSpawnInflationPerTile );
            mn.Init( EV.ExtraWaspSpawnTimerPerTile, ExtraWaspSpawnTimerPerTile );
            mn.Init( EV.ExtraWaspSpawnInflationPerSec, ExtraWaspSpawnInflationPerSec );
            mn.Init( EV.MaxSpawnCount, MaxSpawnCount );
            mn.Init( EV.WaspDamage, WaspDamage );
            mn.Init( EV.MotherWaspRadius, MotherWaspRadius );
            mn.Init( EV.ShieldedWaspChance, ShieldedWaspChance );
            mn.Init( EV.FrontalRebound, FrontalRebound );
            mn.Init( EV.TickBasedMovement, TickBasedMovement );
            mn.Init( EV.IsBoss, IsBoss );
            mn.Init( EV.IsWorking, IsWorking );
            mn.Init( EV.TouchCount, TouchCount );
            mn.Init( EV.InfectedChance, InfectedChance );            
            mn.Init( EV.BoulderPunchPower, BoulderPunchPower );

            if( DynamicObjectMoveList != null && DynamicObjectMoveList.Count > 0 )
                mn.Init( EV.DynamicObjectMoveList, DynamicObjectMoveList );
            mn.Init( EV.WaitAfterObstacleCollision, WaitAfterObstacleCollision );
            if( DynamicObjectOrientationList != null && DynamicObjectOrientationList.Count > 0 )
                mn.Init( EV.DynamicObjectOrientationList, DynamicObjectOrientationList );

            if( DynamicObjectDirectionList != null && DynamicObjectDirectionList.Count > 0 )
                mn.Init( EV.DynamicObjectDirectionList, DynamicObjectDirectionList );

            if( DynamicObjectJumpList != null && DynamicObjectJumpList.Count > 0 )
                mn.Init( EV.DynamicObjectJumpList, DynamicObjectJumpList );

            if( RandomMoveTurnList != null && RandomMoveTurnList.Count > 0 )
                mn.Init( EV.RandomMoveTurnList, RandomMoveTurnList );

            if( RandomDirTurnList != null && RandomDirTurnList.Count > 0 )
                mn.Init( EV.RandomDirTurnList, RandomDirTurnList );

            if( RandomDirTurnList != null && RandomDirTurnList.Count > 0 )
                mn.Init( EV.RandomDirTurnList, RandomDirTurnList );

            if( BonusItemList != null && BonusItemList.Count > 0 )
                mn.Init( EV.BonusItemList, BonusItemList );

            if( BonusItemAmountList != null && BonusItemAmountList.Count > 0 )
                mn.Init( EV.BonusItemAmountList, BonusItemAmountList );

            if( BonusItemChanceList != null && BonusItemChanceList.Count > 0 )
                mn.Init( EV.BonusItemChanceList, BonusItemChanceList );

            mn.Init( EV.MovementSorting, MovementSorting );
            mn.Init( EV.OrientationSorting, OrientationSorting );
            mn.Init( EV.DynamicMaxSteps, DynamicMaxSteps );
            if( InitialDirection != EDirection.NONE )
                mn.Init( EV.InitialDirection, InitialDirection );
            if( FixedRestingDirection != EDirection.NONE )
                mn.Init( EV.FixedRestingDirection, FixedRestingDirection );
            if( Direction != EDirection.NONE )
                mn.Init( EV.Direction, Direction );
            if( RestingDirectionIsSameAsHero == true )
                mn.Init( EV.RestingDirectionIsSameAsHero, RestingDirectionIsSameAsHero );
            mn.Init( EV.RandomDirection, RandomDirection );
            mn.Init( EV.OrientatorDirection, RandomDirection );
            if( InitialForcedFrontalMovementFactor > 0 )
                mn.Init( EV.InitialForcedFrontalMovementFactor, InitialForcedFrontalMovementFactor );
            mn.Init( EV.BaseMiningChance, BaseMiningChance );
            mn.Init( EV.SortMiningChanceFactor, SortMiningChanceFactor );
            mn.Init( EV.MiningPrize, MiningPrizeList, this );
            mn.Init( EV.WaterObjectType, WaterObjectType );
            mn.Init( EV.OptionalMonster, OptionalMonster );
            mn.Init( EV.MineLeverChance, MineLeverCogChance );
            mn.Init( EV.MineLeverDirection, MineLeverDirection );
            mn.Init( EV.MineBonuses, RopeConnectedMineChance );
            int any = 0;
            mn.Init( EV.MineLeverChainTarget, any );
            mn.Init( EV.FlyingSpeed, FlyingSpeed );
            if( FishSwimType != EFishSwimType.NONE )
                mn.Init( EV.FishSwimType, FishSwimType );
            mn.Init( EV.FishingPowerFactor, FishingPowerFactor );
            if( FishRedLimit != -1 )
                mn.Init( EV.FishRedLimit, FishRedLimit );
            if( FishGreenLimit != -1 )
                mn.Init( EV.FishGreenLimit, FishGreenLimit );
            if( BaseTotalRespawnTime != -1)
                mn.Init( EV.BaseTotalRespawnTime, BaseTotalRespawnTime );
            if( FishingBonusAmount != -1 )
                mn.Init( EV.FishingBonusAmount, FishingBonusAmount );
            if( MinFishSpeed != -1 )
                mn.Init( EV.MinFishSpeed, MinFishSpeed );
            if( MaxFishSpeed != -1 )
                mn.Init( EV.MaxFishSpeed, MaxFishSpeed );
            if( OverFishSecondsNeededPerUnit != -1 )
                mn.Init( EV.OverFishSecondsNeededPerUnit, OverFishSecondsNeededPerUnit );
            mn.Init( EV.FishingPole, FishingPoleExtraTime );

            if( FishAction.Count != null && FishAction.Count > 0 )
                if( mn.Control )
                {
                    mn.Control.FishAction = new List<FishActionStruct>();
                    for( int j = 0; j < FishAction.Count; j++ )
                    {
                        mn.Init( EV.FishAction, ( int ) FishAction[ j ].EffectType, j, this );
                    }
                }

            if( BaseTotalRespawnAmount != -1 )
                mn.Init( EV.BaseTotalRespawnAmount, BaseTotalRespawnAmount );
            mn.Init( EV.RandomizePositionOnRespawn, RandomizePositionOnRespawn );
            mn.Init( EV.RandomizePositionOnPoleStep, RandomizePositionOnPoleStep );

            if( FlyingAction.Length != null && FlyingAction.Length > 0 )
            if( mn.Control )
                {
                    mn.Control.FlyingActionTarget = new List<Vector2>();
                    mn.Control.FlyingActionVal = new List<float>();
                    mn.Control.FlyingActionLoopCount = new List<int>();
                    for( int j = 0; j < FlyingAction.Length; j++ )
                    {
                        mn.Init( EV.FlyingActionTarget, FlyingAction[ j ].Target, j );
                        mn.Init( EV.FlyingActionVal, FlyingAction[ j ].FloatVal, j, this );
                        mn.Init( EV.FlyingActionLoopCount, FlyingAction[ j ].LoopCount, j, this );
                        mn.Init( EV.FlyingActionTotalTime, FlyingAction[ j ].TotalTime, j, this );
                    }
                }

            if( InflictedDamageRate != -1 )
                mn.Init( EV.InflictedDamageRate, InflictedDamageRate );
            if( RaftGroupID != -1 )
                mn.Init( EV.RaftGroupID, RaftGroupID );
            mn.Init( EV.RaftJointList, RaftJointList, this );
            mn.Init( EV.BabyList, BabyList, this );
            if( VsRoachHeroAttBonusPerBaby != -1 )
             mn.Init( EV.VsRoachAttDecreasePerBaby, VsRoachHeroAttBonusPerBaby );
            if( VsHeroRoachAttackBonusPerBaby != -1 )
                mn.Init( EV.VsHeroRoachkAttackIncreasePerBaby, VsHeroRoachAttackBonusPerBaby );
            if( RedRoachBabyList != null )
                mn.Init( EV.RedRoachBabyList, RedRoachBabyList );
            if( HerbColor != EHerbColor.NONE )
                mn.Init( EV.HerbColor, HerbColor );
            if( HerbType != EHerbType.NONE )
                mn.Init( EV.HerbType, HerbType );
            mn.Init( EV.HerbBonusAmount, HerbBonusAmount );
            mn.Init( EV.RandomizableHerb, RandomizableHerb );
            mn.Init( EV.WhiteGateGroup, WhiteGateGroup );

            if( SpriteColor != new Color( 0, 0, 0, 0 ) )
                mn.Init( EV.SpriteColor, SpriteColor );

            if( mn.UnitType == EUnitType.MONSTER ) ModdedMonstersList.Add( mn );
            if( mn.UnitType == EUnitType.GAIA2 ) ModdedGaia2List.Add( mn );
            end: { }
        }
    }
    public static EModType GetMod( ETileType modtile )
    {
        if( ( int ) modtile >= ( int ) ETileType.MOD1 && ( int ) modtile <= ( int ) ETileType.MOD16 )
            return ( EModType ) ( modtile - ETileType.MOD1 );
        if( ( int ) modtile >= ( int ) ETileType.MOD17 && ( int ) modtile <= ( int ) ETileType.MOD32 )
            return ( EModType ) ( modtile - ETileType.MOD17 + 16 );
        return EModType.NONE;
    }
    public static EModType GetModInTile( Vector2 tg, tk2dTileMap tm = null )
    {
        if( tm == null ) tm = Quest.I.Dungeon.Tilemap;
        ETileType modtile = ( ETileType ) tm.GetTile( ( int ) tg.x, ( int ) tg.y, ( int ) ELayerType.MODIFIER );
        return GetMod( modtile );
    }
    public static ETileType GetTileIDforMod( EModType mod )
    {
        if( mod >= EModType.MOD_1 && mod <= EModType.MOD_16 )
            return ETileType.MOD1 + ( int ) mod;
        if( mod >= EModType.MOD_17 && mod <= EModType.MOD_32 )
            return ETileType.MOD17 + ( int ) mod - 16;
        return ETileType.NONE;
    }
    public static void PostModInitialization( Sector s )
    {
        s.DynamicObjects = new List<Unit>();

        for( int m = 0; m < Map.I.RM.SD.ModList.Length; m++ )
        {
            Mod md = Map.I.RM.SD.ModList[ m ];
            List<Unit> mlist = new List<Unit>();
            mlist.AddRange( md.ModdedMonstersList );

            if( md.AddRandomMonsterStat != null && md.AddRandomMonsterStat.Count > 0 )               // Random Monster stat
                if( md.ModdedMonstersList != null && md.ModdedMonstersList.Count > 0 )
                {
                    for( int i = 0; i < md.AddRandomMonsterStat.Count; i++ )
                        if( mlist.Count > 0 )
                        {
                            int id = Random.Range( 0, mlist.Count );
                            int level = Util.FloatSort( md.AddRandomMonsterStat[ i ] );
                            mlist[ id ].Body.BaseLevel += level;
                            mlist.RemoveAt( id );
                        }
                }

            if( md.DynamicObjectMoveList != null && md.DynamicObjectMoveList.Count > 0 ||
                md.DynamicObjectOrientationList != null && md.DynamicObjectOrientationList.Count > 0 ||
                md.DynamicObjectJumpList != null && md.DynamicObjectJumpList.Count > 0 ||
                md.DynamicObjectDirectionList != null && md.DynamicObjectDirectionList.Count > 0 )
            {
                s.DynamicObjects.AddRange( md.ModdedGaia2List );
            }
        }
    }

    public static float GetOrientatorNum( Vector2 Pos, EOrientatorEffect eff, int cont = -1 )                             // Attention: all calls need to use IniPos to work for units that are being loaded afar from original position
    {
        int idd = Mod.MD.Ori( eff );                                      // Gets the current orientator
        if( idd == -1 ) return -1;                                        // Check if the effect id is 1 or 2
        EDirection dr = GetOrientatorDir( Pos, idd );                     // Gets the mod dir
        OriDir = dr;
        int id = -1;

        id = GetDirID( dr, eff, idd, Pos );                               // Get dir id

        if( idd == 0 && MD.OrientatorTable1 != null &&
            id >= 0 && id <= MD.OrientatorTable1.Count - 1 )              // First Table value
            return MD.OrientatorTable1[ id ];

        if( idd == 2 && MD.OrientatorTable3 != null &&
            id >= 0 && id <= MD.OrientatorTable3.Count - 1 )              // Ori3 Table value
            return MD.OrientatorTable3[ id ];

        if( idd == 3 && MD.OrientatorTable4 != null &&
            id >= 0 && id <= MD.OrientatorTable4.Count - 1 )              // Ori4 Table value
            return MD.OrientatorTable4[ id ];

        bool surrounding = false;
        if( eff == EOrientatorEffect.SetLeverDirection        ||          // These effects are only valid for surrounding ori mods
            eff == EOrientatorEffect.AddBabyToDir             ||
            eff == EOrientatorEffect.AddBonusToPole           ||
            eff == EOrientatorEffect.Toggle_Tick_Movement     ) 
            surrounding = true;

        if( idd == 0 && surrounding == false )
            return id;
        PointDir = EOriPointDir.NONE;                                     // Reset Pointing dir

        if( idd == 1 || idd == 4 || idd == 5 || surrounding )             // Case where mods surround object 
        {
            if( cont == -1 )                                              // in this case, cont == -1, search for the next neighbor ori in a loop
            {
                id = -1;
                int rad = 1;
                for( int y = ( int ) ( Pos.y - rad ); y <= Pos.y + rad; y++ )
                for( int x = ( int ) ( Pos.x - rad ); x <= Pos.x + rad; x++ )
                if ( Pos != new Vector2( x, y ) )
                if ( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )
                {
                    dr = GetOrientatorDir( new Vector2( x, y ), idd );                                         // looks for a surrounding object
                    if( dr != EDirection.NONE )
                    {
                        PointDir = EOriPointDir.NOWHERE;                                                       // Pointing to anywhere      
                        if( dr >= EDirection.N && dr <= EDirection.NW )
                        {
                            EDirection inv = ( EDirection ) Manager.I.U.InvDir[ ( int ) dr ];
                            Vector2 ttg = new Vector2( x, y ) + Manager.I.U.DirCord[ ( int ) inv ];
                            if( ttg == Pos )
                            {
                                PointDir = EOriPointDir.AWAY;                                                  // ORIENTATION pointing away 
                            }
                            else
                            {
                                ttg = new Vector2( x, y ) + Manager.I.U.DirCord[ ( int ) dr ];
                                if( ttg == Pos )
                                    PointDir = EOriPointDir.ME;                                                // ORIENTATION pointing towards me 
                            }
                        }
                        OriDir = dr;
                        id = GetDirID( dr, eff, idd, Pos );
                    }
                }
                if( idd == 1 )
                if( MD.OrientatorTable2 != null &&
                    id >= 0 && id <= MD.OrientatorTable2.Count - 1 )                                // Table value for second effect
                    return MD.OrientatorTable2[ id ];

                if( idd == 4 )
                if( MD.OrientatorTable5 != null &&
                    id >= 0 && id <= MD.OrientatorTable5.Count - 1 )                                // Table value for fourth effect
                    return MD.OrientatorTable5[ id ];

                if( idd == 5 )
                if( MD.OrientatorTable6 != null &&
                    id >= 0 && id <= MD.OrientatorTable6.Count - 1 )                                // Table value for fifth effect
                    return MD.OrientatorTable6[ id ];
                return id;
            }

            id = -1;                                                                               // in this case count is provided for a unique ori position search
            Vector2 tg = Pos + Manager.I.U.DirCord[ cont ];                                        // Dir cord based ori search
            if( Map.PtOnMap( Quest.I.Dungeon.Tilemap, tg ) )
            {
                dr = GetOrientatorDir( tg, idd );
                if( Util.ValidDir( dr ) )
                {
                    PointDir = EOriPointDir.NOWHERE;                                               // Pointing to anywhere                        
                    EDirection inv = ( EDirection ) Manager.I.U.InvDir[ ( int ) dr ];
                    Vector2 ttg = tg + Manager.I.U.DirCord[ ( int ) inv ];
                    if( ttg == Pos )
                    {
                        PointDir = EOriPointDir.AWAY;                                              // ORIENTATION pointing away 
                    }
                    else
                    {
                        ttg = tg + Manager.I.U.DirCord[ ( int ) dr ];
                        if( ttg == Pos )
                            PointDir = EOriPointDir.ME;                                           // ORIENTATION pointing towards me 
                    }
                }
                OriDir = dr;
                id = GetDirID( dr, eff, idd, Pos );
                cont--;
            }

            if( idd == 1 )
            if( MD.OrientatorTable2 != null &&
                id >= 0 && id <= MD.OrientatorTable2.Count - 1 )                                // Table value for second effect
                return MD.OrientatorTable2[ id ];

            if( idd == 4 )
            if( MD.OrientatorTable5 != null &&
                id >= 0 && id <= MD.OrientatorTable5.Count - 1 )                                // Table value for fourth effect
                return MD.OrientatorTable5[ id ];

            if( idd == 5 )
            if( MD.OrientatorTable6 != null &&
                id >= 0 && id <= MD.OrientatorTable6.Count - 1 )                                // Table value for fifth effect
                return MD.OrientatorTable6[ id ];
        }
        return id;
    }
    public static int GetDirID( EDirection dr, EOrientatorEffect eff, int type, Vector2 pos )
    {
        if( type == 2 )                                                                                                                                    // key type mod
        {
            ETileType keym = ( ETileType )
            Quest.I.Dungeon.Tilemap.GetTile( ( int ) pos.x, ( int ) pos.y, ( int ) ELayerType.GAIA2 );
            int key = GetKeyNum( keym );
            if( key != -1 ) return key;
            return -1; // new
        }

        if( type == 3 )                                                                                                                                    // Ori 3 type mod
        {
            ETileType keym = ( ETileType )
            Quest.I.Dungeon.Tilemap.GetTile( ( int ) pos.x, ( int ) pos.y, ( int ) ELayerType.RAFT );
            int key = GetOri3Num( keym );
            if( key != -1 ) return key;
            return -1; // new
        }

        if( eff == EOrientatorEffect.SetDirection       ||                                                           
            eff == EOrientatorEffect.UpMineDirection    ||
            eff == EOrientatorEffect.Set_Pole_Direction ||                                                     // These effects use the direction of the ori and do not check id
            eff == EOrientatorEffect.Vine_Link          ||
            eff == EOrientatorEffect.MineBonusDir       ||
            eff == EOrientatorEffect.Set_Raft_Joint     ) 
        {
            OriDir = dr;
            return ( int ) dr;
        }
        if( type == 4 ) return ( int ) dr;
        if( type == 5 ) return ( int ) dr;

        return GetOriNum( dr );                                                                                // These one just use the number, not o ori arrow direction

    }
    public static int GetOriNum( EDirection dr )
    {
        switch( dr )                                                                                           
        {
            case EDirection.NW: return 0;
            case EDirection.N: return 1;
            case EDirection.NE: return 2;
            case EDirection.W: return 3;
            case EDirection.E: return 4;
            case EDirection.SW: return 5;
            case EDirection.S: return 6;
            case EDirection.SE: return 7;
        }
        if( ( int ) dr >= 8 && ( int ) dr <= 21 )       // ori from bottom row
        {
            return ( int ) dr;
        }
        return -1;
    }
    public static int GetKeyNum( ETileType keym )
    {
        if( keym >= ETileType.DOOR_OPENER && keym < ETileType.DOOR_OPENER + Map.TotKeys )
            return keym - ETileType.DOOR_OPENER;
        if( keym >= ETileType.DOOR_SWITCHER && keym < ETileType.DOOR_SWITCHER + Map.TotKeys )
            return ( keym - ETileType.DOOR_SWITCHER ) + 12;
        if( keym >= ETileType.DOOR_CLOSER && keym < ETileType.DOOR_CLOSER + Map.TotKeys )
            return ( keym - ETileType.DOOR_CLOSER ) + 24;
        if( keym >= ETileType.DOOR_KNOB && keym < ETileType.DOOR_KNOB + Map.TotKeys )
            return ( keym - ETileType.DOOR_KNOB ) + 36;
        return -1;
    }

    public static int GetOri3Num( ETileType keym )
    {
        if( keym >= ETileType.ORI3 && keym < ETileType.ORI3 + Map.TotOri3 )
            return keym - ETileType.ORI3;
        if( ( int ) keym >= 1408 && ( int ) keym < 1408 + Map.TotOri3 )
            return keym - ETileType.ORI3 - 118;
        return -1;
    }

    public static EDirection GetOrientatorDir( Vector2 Pos, int idd = -1 )
    {
            ETileType or = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile(
            ( int ) Pos.x, ( int ) Pos.y, ( int ) Map.GetTileLayer( ETileType.ORIENTATION ) );              // Orientator based direction
            if( idd != 1 && idd != 0 ) or = ETileType.NONE;

            if( or == ETileType.AREA_DRAG  || or == ETileType.AREA_DRAG2 ||                                 // Not a valid orientator in the layer (new)
                or == ETileType.AREA_DRAG3 || or == ETileType.AREA_DRAG4 || or == ETileType.CAM_AREA )
                return EDirection.NONE;

            if( idd == 1 || idd == 4 || idd == 5 )
            {
                if( Quest.I.Dungeon.Tilemap.GetTile( ( int ) Pos.x, ( int ) 
                    Pos.y, ( int ) ELayerType.MONSTER ) != -1 ) return EDirection.NONE;                     // Surrounding ori only over empty tile                
                if( idd != 4 )
                {
                    int ga2 = Quest.I.Dungeon.Tilemap.GetTile( ( int ) Pos.x, ( int )
                    Pos.y, ( int ) ELayerType.GAIA2 );
                    int key = GetKeyNum( ( ETileType ) ga2 );                                               // key is an exception for gaia2
                    if( ga2 != -1 && key == -1 )                        
                        return EDirection.NONE;
                }
            }

            EDirection dr = EDirection.NONE;
            if( or != ETileType.NONE )
            {
                dr = ( EDirection ) Map.I.GetVariation( Pos, Map.GetTileLayer( ETileType.ORIENTATION ) );
                if( dr != EDirection.NONE )
                    dr = Util.FlipDir( ( EDirection ) dr, Sector.CS.FlipX, Sector.CS.FlipY );
            }

            if( idd == 4 )
            if( dr == EDirection.NONE )                                                                    // Ori 4 type mod
            {
                ETileType keym = ( ETileType )
                Quest.I.Dungeon.Tilemap.GetTile( ( int ) Pos.x, ( int ) Pos.y, ( int ) ELayerType.GAIA2 );
                dr = ( EDirection ) GetKeyNum( keym );
                if( dr >= 0 )
                if( Map.I.RM.KeyListToDelete.Contains( Pos ) == false )
                    Map.I.RM.KeyListToDelete.Add( Pos ); 
            }

            if( idd == 5 )
            if( dr == EDirection.NONE )                                                                    // Ori 5 type mod
            {
                ETileType keym = ( ETileType )
                Quest.I.Dungeon.Tilemap.GetTile( ( int ) Pos.x, ( int ) Pos.y, ( int ) ELayerType.RAFT );
                dr = ( EDirection ) GetOri3Num( keym );
                if( dr >= 0 )
                if( Map.I.RM.KeyListToDelete.Contains( Pos ) == false )
                    Map.I.RM.KeyListToDelete.Add( Pos ); 
            }
            return dr;
    }

    public int Ori( EOrientatorEffect eff )
    {
        if( OrientatorEffects == null ) return -1;
        if( OrientatorEffects.Count < 1 ) return -1;
        if( OrientatorEffects.Contains( eff ) )
        {
            if( OrientatorEffects[ 0 ] == eff ) return 0;
            if( OrientatorEffects.Count >= 2 )  
            if( OrientatorEffects[ 1 ] == eff ) return 1;
            if( OrientatorEffects.Count >= 3 )
            if( OrientatorEffects[ 2 ] == eff ) return 2;
            if( OrientatorEffects.Count >= 3 )
            if( OrientatorEffects[ 3 ] == eff ) return 3;
            if( OrientatorEffects.Count >= 4 )
            if( OrientatorEffects[ 4 ] == eff ) return 4;
            if( OrientatorEffects.Count >= 5 )
            if( OrientatorEffects[ 5 ] == eff ) return 5;
        }
        return -1;
    }

    public static int GetLoneMod( Vector2 pt )    // only returns valid if theres a mod in the pos and no other object
    {
        EModType mod = GetModInTile( pt );
        if( mod < EModType.MOD_1 || mod > EModType.MOD_32 ) return -1;
        if( Quest.I.Dungeon.Tilemap.GetTile( ( int ) pt.x, ( int )
        pt.y, ( int ) ELayerType.MONSTER ) != -1 ) return -1;
        if( Quest.I.Dungeon.Tilemap.GetTile( ( int ) pt.x, ( int )
        pt.y, ( int ) ELayerType.GAIA2 ) != -1 ) return -1;
        return ( int ) mod;
    }

    public static int GetLoneOri3( Vector2 pt )    // only returns valid if theres a ori3 in the pos and no other object
    {
        ETileType modtile = ( ETileType )
        Quest.I.Dungeon.Tilemap.GetTile( ( int ) pt.x, ( int ) pt.y, ( int ) ELayerType.RAFT );
        int mod = GetOri3Num( modtile );
        if( mod < 0 || mod > 10 ) return -1;
        if( Quest.I.Dungeon.Tilemap.GetTile( ( int ) pt.x, ( int )
        pt.y, ( int ) ELayerType.MONSTER ) != -1 ) return -1;
        if( Quest.I.Dungeon.Tilemap.GetTile( ( int ) pt.x, ( int )
        pt.y, ( int ) ELayerType.GAIA2 ) != -1 ) return -1;
        return ( int ) mod;
    }
    public static int GetOriNumber( Vector2 tg )
    {
        int ori = Map.I.GetVariation( tg, ELayerType.AREAS );
        return Mod.GetOriNum( ( EDirection ) ori );
    }
}
