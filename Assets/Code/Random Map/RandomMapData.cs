using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using Sirenix.OdinInspector;
using System.Text.RegularExpressions;
//using UnityEngine.Serialization;

public enum EAdventureType
{
    ALL = -2, NONE = -1, A1_FIRST_STEPS = 0, A2_SHOOTING, A3_FINGER_TRAINING, A4_MONSTER_MOVEMENT,
    A5_BARRICADES, A6_FIRE_1, A7_RESTING, A8_TOUCHED_BARRICADES, A9_TIMED_FIGHT,
    A10_ARROWS, A11_HARDER_FIGHT, A12_BOULDERS_1, A13_GATES_1, A14_BOULDERS_2, A15_PLATFORMS_1,
    A16_WASPS, A17_SNOW, A18_ROACHES, A19_QUEROQUERO, A20_BOULDERS_AND_PIT, A21_SPIDERS, A22_BOULDERS_ASYNCH,
    A23_DYNAMIC_ARROWS, TEST
}

public class RandomMapData : MonoBehaviour
{
    public enum ELabType
    {
       NONE = -1, LAB_1 = 0
    }
    [TabGroup( "Main" )]
    [ReadOnly]
    public string UniqueID;                       // For Saving
    [TabGroup( "Main" )]
    [ReadOnly]
    public bool IsUp;
    [TabGroup( "" )]
    [Header( "Write Quest Notes Here!" )]
    [TextArea( 1, 120 )]
    public string Notes = "";
    [TabGroup( "" )]
    [PropertyTooltip( "Tab 1" )]
    bool clear1 = false;
    [TabGroup( "2", "" )]
    [PropertyTooltip("Tab 2")]
    bool clear2 = false;
    [TabGroup( "3", "" )]
    [PropertyTooltip( "Tab 3" )]
    bool clear3 = false;
    [TabGroup( "Main" )]
    public MapQuestHelper QuestHelper;
    [TabGroup( "Main" )]
    public Vector2 MapCord;
    [TabGroup( "Main" )]
    public Vector2 BlockArea = new Vector2( 1, 1 );
    [TabGroup( "Lists" )]
    public GameObject ImagesFolder;
    [TabGroup( "Lists" )]
    public GameObject GoalsFolder;
    [TabGroup( "Lists" )]
    public RandomMapGoal[ ] GoalList;
    [TabGroup( "Main" )]
    public int SDID;
    [TabGroup( "Main" )]
    public GameObject SectorHintFolder;
    [TabGroup( "Main" )]
    public int QuestID = -1;
    [TabGroup( "Main" )]
    public bool EnableAlternateStartingCube = false;
    [TabGroup( "3", "Lab" )]
    public ELabType LabType = ELabType.LAB_1;
    [TabGroup( "Req" )]
    public int InitiallyAvailableCubes = -1;      // Initial available cubes for playing at the start  ( -1 means all available )
    [TabGroup( "Req" )]
    public bool Available = false;
    [TabGroup( "Req" )]
    public List<EAdventureType> RequiredAdventureList;
    [TabGroup( "Req" )]
    public int RequiredBronzeTrophies = 0;
    [TabGroup( "Req" )]
    public int RequiredSilverTrophies = 0;
    [TabGroup( "Req" )]
    public int RequiredGoldTrophies = 0;
    [TabGroup( "Req" )]
    public int RequiredDiamondTrophies = 0;
    [TabGroup( "Req" )]
    public int RequiredGeniusTrophies = 0;
    [TabGroup( "Req" )]
    public string RequiredTrophyArea = "";
    [TabGroup( "Req" )]
    public ItemType RequiredItem = ItemType.NONE;
    [TabGroup( "Req" )]
    public int RequiredItemAmount = 0;
    [TabGroup( "Req" )]
    public int RequiredItemLifeTime = 60;
    [TabGroup( "Req" )]
    public ItemType RequiredMaxCapacityItem = ItemType.NONE;
    [TabGroup( "Req" )]
    public int RequiredMaxCapacityAmount = 0;
    [TabGroup( "Lists" )]
    public GameObject UpgradeListFolder;
    [TabGroup( "Lists" )]
    public AdventureUpgradeInfo[] AdventureUpgradeInfoList = null;
    [TabGroup( "Lists" )]
    public GameObject TechListFolder;
    [TabGroup( "Main" )]
    public int StartingAdventureLevel = 1;
    [TabGroup( "Main" )]
    public int StartingTrainningLevel = 0;
    [TabGroup( "Gate" )]
    public bool RescueToGate = true;
    [TabGroup( "Gate" )]
    public int StartingGotoCheckPointLevel = 0;
    [TabGroup( "Gate" )]
    public float GotoCheckPointResourceCost = 0f;
    [TabGroup( "Gate" )]
    public float AutoOpenGateResourceCost = 0f;
    [TabGroup( "Main" )]
    public bool RescueEnabled = true;
    [TabGroup( "Main" )]
    public int MaxCubes = 0;
    [TabGroup( "Main" )]
    public int QuestSecrets = 0;
    [TabGroup( "Main" )]
    public int MaximumUnits = -1;
    [TabGroup( "Main" )]
    public float CubeDeathDamage = 10;
    [TabGroup( "Main" )]
    public float HeroStartingHP = 100;
    [TabGroup( "Main" )]
    public float QuestRelevance = 100;   // for game completition calculation
    [TabGroup( "Bonus" )]
    public float CubeClearHPBonus = 0;
    [TabGroup( "Options" )]
    public bool ShowPopup = false;
    [TabGroup( "3", "Difficulty" )]
    public bool ShowDificultySlider = false;
    [TabGroup( "Options" )]
    public bool ForceGrid = false;
    [TabGroup( "3", "Difficulty" )]
    public float MinimumDifficulty = 50;
    [TabGroup( "3", "Difficulty" )]
    public float MaximumDifficulty = 200;
    [TabGroup( "3", "Difficulty" )]
    public float StartingDifficultyLimit = 0;
    [TabGroup( "3", "Difficulty" )]
    public float NewbieDifficultyFactor = -30;
    [TabGroup( "3", "Difficulty" )]
    public float CasualDifficultyFactor = -20;
    [TabGroup( "3", "Difficulty" )]
    public float NormalDifficultyFactor = -10;
    [TabGroup( "3", "Difficulty" )]
    public float VeteranDifficultyFactor = 0;
    [TabGroup( "3", "Difficulty" )]
    public float HellDifficultyFactor = 15;
    [TabGroup( "Gate" )]
    public bool ShowGate = false;
    [TabGroup( "Gate" )]
    public int GateSize = 5;
    [TabGroup( "Gate" )]
    public float InitialAreaGateCost = 0;
    [TabGroup( "Gate" )]
    public float ExtraAreaGate = 5;
    [TabGroup( "Gate" )]
    public float MaxAreaGateCost = 0;
    [TabGroup( "Gate" )]
    public float InitialLabGateCost = 10;
    [TabGroup( "Gate" )]
    [Range( 1, 100 )]
    public float GateCreationChance = 75;
    [TabGroup( "Gate" )]
    public bool AllowFreeGateOpening = false;
    [TabGroup( "Main" )]
    public float MonsterDamageRate = 100;
    [TabGroup( "3", "Old" )]
    public float PointsPerTile = 0.5f;
    [TabGroup( "3", "Old" )]
    public float TurnPenalty = 0.5f;
    [TabGroup( "Main" )]
    public float BaseMonsterLevel = 0;                     // Adds to lev 1 already initialized in the monster prefabs Keep at 0 to start lv 1
    [TabGroup( "Main" )]
    public bool SteppingMode = false;
    [TabGroup( "Main" )]
    public bool AutoScorpionSteppingMode = true; 
    [TabGroup( "Main" )]
    public float MonsterLevelIncreaseFactor = 1;
    [TabGroup( "Main" )]
    public float MonsterLevelFactorInflation = 0;
    [TabGroup( "Main" )]
    public float DynamicMonsterLevelAddPerDeath = 0;
    [TabGroup( "Main" )]
    public ItemType DefaulLoadItemType = ItemType.Energy;
    [TabGroup( "Main" )]
    public float DefaultLoadCost = 2;
    [TabGroup( "Gate" )]
    public int MaximumDirtyAreas = 0;                       // Gate Opening
    [TabGroup( "Options" )]
    public bool LockedZoom = false;
    [TabGroup( "Options" )]
    public int DefaultZoom = 0;
    [TabGroup( "Options" )]
    public float CameraFrontFacingDist = 2;
    [TabGroup( "3", "Punishment" )]
    public float ScarabPunishment = 10;
    [TabGroup( "3", "Punishment" )]
    public float AreaExitPenalty = 100;
    [TabGroup( "3", "Punishment" )]
    public float DirtyAreaExitHPPenalty = 0;       // in Percent
    [TabGroup( "3", "Punishment" )]
    public float RestartPenalty = 0;               // In Percent. For Restarting or Death
    [TabGroup( "2", "Resources" )]
    public List<ItemType> GlobalResourcesList = null;
    [TabGroup( "3", "Punishment" )]
    public int OutAreaFlashBackDeathCost = 3;
    [TabGroup( "3", "Punishment" )]
    public int InAreaFlashBackDeathCost = 2;
    [TabGroup( "Options" )]
    public bool SuddenDeathMode = false;
    [TabGroup( "2", "Resources" )]
    public float HpHealPerDeadRoach = 0;
    [TabGroup( "2", "Resources" )]
    public float HpHealPerDeadScarab = 0;
    [TabGroup( "2", "Barricade" )]
    public List<float> BarricadeChanceHeight;
    [TabGroup( "2", "Barricade" )]
    public List<float> OutBarricadeChanceHeight;
    [TabGroup( "2", "Barricade" )]
    public List<float> InExtraBarricadeSize;
    [TabGroup( "2", "Barricade" )]
    public List<float> OutExtraBarricadeSize;
    [TabGroup( "3","Packmule" )]
    public float NailWorkingChance = 100;
    [TabGroup( "2", "Barricade" )]
    public float BurningBarricadeDamagePercent = 25;
    [TabGroup( "2", "Barricade" )]
    public float BarricadeDestroyInflation = 0;              // Per Area Cleared Inflation
    [TabGroup( "3", "Area" )]
    public float PerfectAreaKeyWeight = 0.5f;
    [TabGroup( "2", "Resources" )]
    public float MinimumRandomResourceTimer = 8;
    [TabGroup( "2", "Resources" )]
    public float MaximumRandomResourceTimer = 20;
    [TabGroup( "2", "Resources" )]
    public float OutRandomResourceCreationChance = 100;
    [TabGroup( "2", "Resources" )]
    public float OutMinimumRandomResourceTimer = 3;
    [TabGroup( "2", "Resources" )]
    public float OutMaximumRandomResourceTimer = 8;
    [TabGroup( "2", "Resources" )]
    public float RandomResourceWitherFactor = 1;
    [TabGroup( "2", "Resources" )]
    public float RandomResourceDarkWitherFactor = 2;
    [TabGroup( "2", "Resources" )]
    public float BaseRandomResourceChance = 50;
    [TabGroup( "2", "Resources" )]
    public int SpawnResourceRange = 1;
    [TabGroup( "2", "Resources" )]
    public float RelocatorCreationChance = 0;
    [TabGroup( "2", "Resources" )]
    public float InitialUpgradeChestChance = 50f;
    [TabGroup( "2", "Resources" )]
    public float CloverPickUpgradeChestChance = 20f;
    [TabGroup( "2", "Resources" )]
    public float CubeClearUpgradeChestChance = 30f;
    [TabGroup( "2", "Resources" )]
    public float OpenChestUpgradeChestChance = 50f;
    [TabGroup( "2", "Resources" )]
    public float BaseChestPersistChance = 0;
    [TabGroup( "2", "Resources" )]
    public bool ChestInflationCummulative = false;
    [TabGroup( "2", "Resources" )]
    public List<ItemType> ChestBonusItemList = null;
    [TabGroup( "2", "Resources" )]
    public int CloversPerCube = 5;
    [TabGroup( "2", "Resources" )]
    public float QuestXPPerClover = .4f;

    [TabGroup( "Bonus" )]
    public float CubeClearBonusCurve = 1.5f;
    [TabGroup( "Bonus" )]
    public float ShellStartBonus = 1;
    [TabGroup( "Bonus" )]
    public float ShellEndBonus = 3;
    [TabGroup( "Bonus" )]
    public float CogStartBonus = 1;
    [TabGroup( "Bonus" )]
    public float CogEndBonus = 3;
    [TabGroup( "Bonus" )]
    public float XpStartBonus = 1f;
    [TabGroup( "Bonus" )]
    public float XpEndBonus = 3f;
    [TabGroup( "Bonus" )]
    public float HoneycombStartBonus = 1;
    [TabGroup( "Bonus" )]
    public float HoneycombEndBonus = 3;
    [TabGroup( "Bonus" )]
    public float QuestXPPerChest = 0.5f;
    [TabGroup( "Bonus" )]
    public float AltarEvolutionFactor = 100;  // in % the amount of Altar bonuses gained on cube clear
    [TabGroup( "Rest" )]
    public float BaseRestingDistance = 10;
    [TabGroup( "Rest" )]
    public bool RestingNeedLOSCheck = false;
    [TabGroup( "3", "Old" )]
    public float BaseFrontalProtectionChance = 15;
    [TabGroup( "3", "Old" )]
    public float BaseFrontSideProtectionChance = 8;
    [TabGroup( "3", "Old" )]
    public float FrontalProtectionMonsterBonus = 50;
    [TabGroup( "3", "Old" )]
    public float FrontSideProtectionMonsterBonus = 100;
    [TabGroup( "3", "Old" )]
    public int MinFrontalProtectionSector = 4;
    [TabGroup( "3", "Old" )]
    public int MinFrontSideProtectionSector = 6;
    [TabGroup( "2", "Monster" )]
    public int MinPoisonBiteSector = 4;
    [TabGroup( "2", "Monster" )]
    public float PoisonBiteMonsterChance = 10;
    [TabGroup( "2", "Monster" )]
    public float PoisonBiteRoachChance = 0;
    [TabGroup( "2", "Monster" )]
    public float PoisonBiteMosquitoChance = 20;
    [TabGroup( "2", "Monster" )]
    public float PoisonBiteMonsterBonus = 100;
    [TabGroup( "2", "Monster" )]
    public int MinSpiderMergeAmount = 2;
    [TabGroup( "2", "Slime" )]
    public float SlimeShotPassThroughBonus = 50;
    [TabGroup( "2", "Slime" )]
    public float SlimeRotationSpeed = 90;
    [TabGroup( "2", "Monster" )]
    public int ForcedFrontalMovementDistance = 100;
    [TabGroup( "2", "Old" )]
    public float MarkedAreaChance = 10;
    [TabGroup( "2", "Old" )]
    public int MinimumMarkedAreaSector = 3;
    [TabGroup( "2", "Old" )]
    public float MarkedMonsterChance = 75;
    [TabGroup( "2", "Old" )]
    public float MarkedScarabChance = 20;
    [TabGroup( "Rest" )]
    public int BaseWakeUpTurnAmount = 10;
    [TabGroup( "2", "Monster" )]
    public float PoisonerFriendlyFireAttackRate = 50;
    [TabGroup( "2", "Monster" )]
    public float[ ] MonsterShieldPerAttackAngle = { 100, 75, 50, 25, 0 };
    [TabGroup( "2", "Monster" )]
    public float[ ] HeroShieldPerAttackAngle = { 100, 75, 50, 25, 0 };
    [TabGroup( "Options" )]
    public bool AutoHeal;
    [TabGroup( "2", "Barricade" )]
    public bool ClassicBarricadeDestruction = true;
    [TabGroup( "3", "Lab" )]
    public bool RevealLabFog = true;
    [TabGroup( "Options" )]
    public bool RevealCubeFog = false;
    [TabGroup( "Options" )]
    public bool AutoBuyMode = false;
    [TabGroup( "Options" )]
    public bool AutoBuyDirectPurchase = true;
    [TabGroup( "Options" )]
    public bool QuickTravelEnabled = true;
    [TabGroup( "Options" )]
    public bool LockCubeJumpEnabled = false;
    [TabGroup( "Options" )]
    public bool AutoPickupNeighborArtifacts = false;
    [TabGroup( "Options" )]
    public bool EnableQuickBuy = false;
    [TabGroup( "Options" )]
    public bool ForceJumpToCheckPoint = false;   // for snow and sand terrains
    [TabGroup( "Options" )]
    public EMineType DefaultMineType = EMineType.SQUARE;  // Default mine type for mines with no mod
    [TabGroup( "3", "Punishment" )]
    public float InsideDirtyBleedingHP = 0;
    [TabGroup( "3", "Punishment" )]
    public float InsideCleanBleedingHP = 0;
    [TabGroup( "3", "Punishment" )]
    public float OutsideBleedingHP = 0;
    [TabGroup( "3", "Lab" )]
    public float InsideLabBleedingHP = 0;
    [TabGroup( "2", "Fire" )]
    public float BaseFireMasterLevel = 0;
    [TabGroup( "2", "Fire" )]
    public float BaseFirePower = 20;
    [TabGroup( "2", "Fire" )]
    public float FirePowerInflationPerBonfire = 10;
    [TabGroup( "2", "Fire" )]
    public float FireAttackBonusFactor = 100;
    [TabGroup( "2", "Fire" )]
    public int BaseFireWoodNeeded = 10;
    [TabGroup( "2", "Fire" )]
    public float FirewoodNeededInflation = 0;   // Per area cleared
    [TabGroup( "2", "Fire" )]
    public int BaseWoodRequiredForSale = 20;
    [TabGroup( "2", "Fire" )]
    public int WoodForRunePrize = 2;
    [TabGroup( "Lev" )]
    public int BaseThreatDuration = 1;
    [TabGroup( "3", "Lab" )]
    public Vector2 PreferedLabPosition = new Vector2( -1, -1 );    // 3, 4 to center
    [TabGroup( "3", "Lab" )]
    public bool DirectLabJump = true;
    [TabGroup( "Lists" )]
    public SectorDefinition[ ] SDList;
    [TabGroup( "3", "Lab" )]
    public EModAction[ ] LabModAction;
    [TabGroup( "3", "Lab" )]
    public float[ ] LabModValue;
    [TabGroup( "2", "Platform" )]
    public float BasePlatformWalkingLevel = 0;
    [TabGroup( "2", "Platform" )]
    public float BasePlatformSteps = 0;
    [TabGroup( "2", "Platform" )]
    public float MonsterPlatformExitCost = 25;
    [TabGroup( "2", "Platform" )]
    public float ReversedPlatformExitCost = 50;
    [TabGroup( "2", "Platform" )]
    public float OrientationPlatformExitCost = 75;
    [TabGroup( "2", "Platform" )]
    public float FreePlatformExitCost = 100;
    [TabGroup( "2", "Platform" )]
    public float GlobalPlatformAttBonusPerPoint = .1f;
    [TabGroup( "2", "Platform" )]
    public float PlatformBleedingHP = 0;
    [TabGroup( "2", "Platform" )]
    public float PlatformAttackSpeedBonus = 30;
    [TabGroup( "2", "Platform" )]
    public float PerPlatformStepAttackSpeedBonus = 15;
    [TabGroup( "Options" )]
    public bool RestrictAreaExitToEntranceTile = true;
    [TabGroup( "Options" )]
    public bool RevealAreaFromAfar = true;
    [TabGroup( "Lev" )]
    public float BaseMovementLevel = 0;
    [TabGroup( "Lev" )]
    public float BaseScoutLevel = 0;
    [TabGroup( "Lev" )]
    public float BaseHeroMeleeAttackLevel = 0;
    [TabGroup( "Lev" )]
    public int BaseHeroRangedAttackRange = 0;
    [TabGroup( "Lev" )]
    public float BaseHeroRangedAttackLevel = 0;
    [TabGroup( "Lev" )]
    public float LimitedArrowPerCube = -1;
    [TabGroup( "Lev" )]
    public float LimitedMeleeAttacksPerCube = -1;
    [TabGroup( "Lev" )]
    public float BaseArrowInLevel = 0;
    [TabGroup( "Lev" )]
    public float BaseArrowOutLevel = 0;
    [TabGroup( "Lev" )]
    public float BaseHeroMeleeShieldLevel = 0;
    [TabGroup( "Lev" )]
    public float HeroMeleeShieldBonusPerLevel = 10;
    [TabGroup( "Lev" )]
    public float BaseHeroRangedShieldLevel = 0;
    [TabGroup( "Lev" )]
    public float HeroRangedShieldBonusPerLevel = 10;
    [TabGroup( "Lev" )]
    public float BaseMiningLevel = 0;
    [TabGroup( "2", "Barricade" )]
    public float BaseDestroyBarricadeLevel = 0;
    [TabGroup( "2", "Barricade" )]
    public float BarricadeDestroyHeroWaitTime = 0f;
    [TabGroup( "2", "Barricade" )]
    public float BarricadeDestroyWaitTime = 0.5f;
    [TabGroup( "RT" )]
    public bool RealtimeResourceCounterDecrease = false;
    [TabGroup( "RT" )]
    public float RTHeroRangedAttackSpeed = 40;
    [TabGroup( "RT" )]
    public float RTHeroRangedAttackDamageFactorr = 100;
    [TabGroup( "RT" )]
    public float RTHeroMeleeAttackSpeed = 20;
    [TabGroup( "RT" )]
    public float RTHeroMeleeAttackDamageFactorr = 100;
    [TabGroup( "RT" )]
    public float RTMonsterRangedAttackDamageFactor = 100;
    [TabGroup( "RT" )]
    public float RTMonsterMeleeAttackDamageFactor = 100;
    [TabGroup( "RT" )]
    public float RTBaseHeroMeleeAttackDamage = -1;
    [TabGroup( "RT" )]
    public float RTBaseHeroRangedAttackDamage = -1;
    [TabGroup( "RT" )]
    public float BaseThrowingAxeAttackDamage = 50;
    [TabGroup( "RT" )]
    public float BaseBambooAttackDamage = 50;
    [TabGroup( "RT" )]
    public float BaseSpearAttackDamage = 50;
    [TabGroup( "RT" )]
    public float BaseHookAttackDamage = 50;
    [TabGroup( "RT" )]
    public float BaseKickDamage = 10;
    [TabGroup( "RT" )]
    public float RTDragonRangedAttackSpeed = 20;
    [TabGroup( "RT" )]
    public float HurryUpTimeScaleFactor = 4;
    [TabGroup( "2", "Fly" )]
    public float RTDragonMinAttSpeed = 100;
    [TabGroup( "2", "Fly" )]
    public float RTDragonMaxAttSpeed = 300;
    [TabGroup( "2", "Fly" )]
    public int FindNestRange = 3;
    [TabGroup( "2", "Fly" )]
    public float BaseDragonSlayerAngle = 30;
    [TabGroup( "2", "Fly" )]
    public float BaseDragonSlayerMaxHP = 90;
    [TabGroup( "2", "Fly" )]
    public float BaseJumperFlightTime = 6;
    [TabGroup( "2", "Fly" )]
    public float JumperHeroAttackDistanceDecrease = 35;
    [TabGroup( "2", "Fly" )]
    public int MaxFrontalTargetManeuverDist = 3;
    [TabGroup( "2", "Fly" )]
    public int MinFrontalTargetManeuverDist = -2;
    [TabGroup( "2", "Fly" )]
    public float BaseMonsterTargetRadius = .5f;
    [TabGroup( "2", "Fly" )]
    public float BaseHeadShotRadius = .15f;
    [TabGroup( "2", "Fly" )]
    public float BaseEyeShotRadius = .03f;
    [TabGroup( "2", "Fly" )]
    public float ExtraMonsterTargetRadiusPerLevel = .1f;
    [TabGroup( "2", "Fly" )]
    public float DragonShotImpactForce = .2f;
    [TabGroup( "RT" )]
    public float RTMonsterMovementSpeed = 10;
    [TabGroup( "RT" )]
    public float RTHeroStepPenaltyFactor = 0;
    [TabGroup( "Bonus" )]
    public float VigorRTAttackSpeedBonus = 50;
    [TabGroup( "Bonus" )]
    public float VigorTime = 4;
    [TabGroup( "Bonus" )]
    public float DifficultyBonusFactor = 1;                        // Bonus factor for shell and cogs depending on the goal difficulty
    [TabGroup( "2", "Fly" )]
    public float QueroQueroWaitTime = 2f;
    [TabGroup( "2", "Fly" )]
    public float NeighborWaspAttSpeedBonusPerTile = 10;
    [TabGroup( "2", "Fly" )]
    public float NeighborWaspPerTileInflation = 20;
    [TabGroup( "2", "Fly" )]
    public float AlignedWaspAttSpeedBonusPerTile = 5;
    [TabGroup( "2", "Fly" )]
    public float AlignedWaspPerTileInflation = 20;
    [TabGroup( "2", "Fly" )]
    public int FireMarkedWaspTurns = 3;
    [TabGroup( "2", "Fly" )]
    public float WaspOverWaterSpeedBonus = 20;
    [TabGroup( "2", "Fly" )]
    public float WaspOverWaterBonusTime = 2;
    [TabGroup( "2", "Fly" )]
    public float BaseDragonFireDamage = 5;
    [TabGroup( "2", "Fly" )]
    public float HeroTargetRadius = .4f;
    [TabGroup( "2", "Fly" )]
    public float OnOxygenFlyingUnitsSpeedFactor = 20;
    [TabGroup( "RT" )]
    public float TickMoveTime = 1;
    [TabGroup( "3", "Terrain" )]
    public float MaxHeroRotationSpeed = 50;
    [TabGroup( "3", "Terrain" )]
    public float DefaultSnowSpeed = 200;
    [TabGroup( "3", "Terrain" )]
    public float SnowSpeedLimit = 10;
    [TabGroup( "3", "Terrain" )]
    public float SnowStepFactor = 50;
    [TabGroup( "3", "Fish" )]
    public float BaseFishingLevel = 0;
    [TabGroup( "3", "Fish" )]
    public float FishingBaseHookAttack = 10;
    [TabGroup( "3", "Fish" )]
    public float FishingHookAttackPerLevel = 5;
    [TabGroup( "3", "Fish" )]
    public float BaseFishingTime = 15;
    [TabGroup( "3", "Fish" )]
    public float FishingBaseHookRadius = .25f;
    [TabGroup( "3", "Fish" )]
    public float FishingHookRadiusPerLevel = 1f;
    [TabGroup( "3", "Fish" )]
    public float FishingBaseHookSpeed = 1;
    [TabGroup( "3", "Fish" )]
    public float FishingHookSpeedPerLevel = 2.5f;
    [TabGroup( "3", "Fish" )]
    public float OverFishBonusPerSecond = 2;
    [TabGroup( "3", "Fish" )]
    public float OverFishBonusInflationPerSecond = 2;
    [TabGroup( "3", "Fish" )]
    public float OverFishBaseBonusPerUnit = 50;
    [TabGroup( "3", "Fish" )]
    public float OverFishInflationPerUnit = 50;
    [TabGroup( "3", "Fish" )]
    public float OverFishCumulativeBonusFactor = 50;
    [TabGroup( "3", "Fish" )]
    public float PerfectFishingBonus = 20;
    [TabGroup( "3", "Fish" )]
    public float FishingBonusPerFlower = 1;
    [TabGroup( "3", "Fish" )]
    public int FishCornTargetRadius = 0;
    [TabGroup( "3", "Fish" )]
    public int MaxPoleDistanceForFishing = 1;
    [TabGroup( "3", "Fish" )]
    public float HarpoonBaseSpeed = 6;      // speed multiplier
    [TabGroup( "3", "Fish" )]
    public float HarpoonBonusAttack = 15;
    [TabGroup( "3", "Fish" )]
    public float HarpoonMoveSensitivity = 8f;
    [TabGroup( "3", "Farm" )]
    public float ChooserMonsterGenerationFactor = 0;
    [TabGroup( "3", "Farm" )]
    public float SpawnerPlagueMonsterGenerationFactor = 0;
    [TabGroup( "3", "Farm" )]
    public float KillerPlagueMonsterGenerationFactor = 0;
    [TabGroup( "3", "Farm" )]
    public float KickablePlagueMonsterGenerationFactor = 0;
    [TabGroup( "3", "Farm" )]
    public float SwapPlagueMonsterGenerationFactor = 0;
    [TabGroup( "3", "Farm" )]
    public float BlockerPlagueMonsterGenerationFactor = 0;
    [TabGroup( "3", "Farm" )]
    public float GrabPlagueMonsterGenerationFactor = 0;
    [Space(20)]
    [TabGroup( "3", "Farm" )]
    public int BlockerPlagueMonsterBaseCost = 5;
    public static int PlayCount = 0;



    public void Copy( RandomMapData rm, bool all = true )
    {
        if( all )
        {
            MapCord = rm.MapCord;
            SDID = rm.SDID;
            QuestID = rm.QuestID;
        }
        UniqueID = rm.UniqueID;
        EnableAlternateStartingCube = rm.EnableAlternateStartingCube;
        Available = rm.Available;
        InitiallyAvailableCubes = rm.InitiallyAvailableCubes;
        ForceGrid = rm.ForceGrid;
        QuestRelevance = rm.QuestRelevance;
        ShowPopup = rm.ShowPopup;
        ForceJumpToCheckPoint = rm.ForceJumpToCheckPoint;
        ShowDificultySlider = rm.ShowDificultySlider;
        RevealAreaFromAfar = rm.RevealAreaFromAfar;
        OverFishCumulativeBonusFactor = rm.OverFishCumulativeBonusFactor;
        TickMoveTime = rm.TickMoveTime;
        OverFishBonusPerSecond = rm.OverFishBonusPerSecond;
        OverFishInflationPerUnit = rm.OverFishInflationPerUnit;
        OverFishBaseBonusPerUnit = rm.OverFishBaseBonusPerUnit;
        OverFishBonusInflationPerSecond = rm.OverFishBonusInflationPerSecond;
        PerfectFishingBonus = rm.PerfectFishingBonus;
        FishCornTargetRadius = rm.FishCornTargetRadius;
        NeighborWaspAttSpeedBonusPerTile = rm.NeighborWaspAttSpeedBonusPerTile;
        AlignedWaspAttSpeedBonusPerTile = rm.AlignedWaspAttSpeedBonusPerTile;
        NeighborWaspPerTileInflation = rm.NeighborWaspPerTileInflation;
        AlignedWaspPerTileInflation = rm.AlignedWaspPerTileInflation;
        FireMarkedWaspTurns = rm.FireMarkedWaspTurns;
        WaspOverWaterBonusTime = rm.WaspOverWaterBonusTime;
        HeroTargetRadius = rm.HeroTargetRadius;
        VigorRTAttackSpeedBonus = rm.VigorRTAttackSpeedBonus;
        VigorTime = rm.VigorTime;
        LabType = rm.LabType;
        ChooserMonsterGenerationFactor = rm.ChooserMonsterGenerationFactor;
        SpawnerPlagueMonsterGenerationFactor = rm.SpawnerPlagueMonsterGenerationFactor;
        KillerPlagueMonsterGenerationFactor = rm.KillerPlagueMonsterGenerationFactor;
        KickablePlagueMonsterGenerationFactor = rm.KickablePlagueMonsterGenerationFactor;
        SwapPlagueMonsterGenerationFactor = rm.SwapPlagueMonsterGenerationFactor;
        BlockerPlagueMonsterGenerationFactor = rm.BlockerPlagueMonsterGenerationFactor;
        GrabPlagueMonsterGenerationFactor = rm.GrabPlagueMonsterGenerationFactor;
        BlockerPlagueMonsterBaseCost = rm.BlockerPlagueMonsterBaseCost;
        MaxHeroRotationSpeed = rm.MaxHeroRotationSpeed;
        MonsterDamageRate = rm.MonsterDamageRate;
        DefaultSnowSpeed = rm.DefaultSnowSpeed;
        SnowSpeedLimit = rm.SnowSpeedLimit;
        SnowStepFactor = rm.SnowStepFactor;
        PointsPerTile  = rm.PointsPerTile;
        PlatformAttackSpeedBonus = rm.PlatformAttackSpeedBonus;
        PerPlatformStepAttackSpeedBonus = rm.PerPlatformStepAttackSpeedBonus;
        InitialAreaGateCost = rm.InitialAreaGateCost;
        ExtraAreaGate = rm.ExtraAreaGate;
        MaxAreaGateCost = rm.MaxAreaGateCost;
        InitialLabGateCost = rm.InitialLabGateCost;
        HpHealPerDeadRoach = rm.HpHealPerDeadRoach;
        HpHealPerDeadScarab = rm.HpHealPerDeadScarab;
        GateSize = rm.GateSize;
        AreaExitPenalty = rm.AreaExitPenalty;
        RealtimeResourceCounterDecrease = rm.RealtimeResourceCounterDecrease;
        DirtyAreaExitHPPenalty = rm.DirtyAreaExitHPPenalty;
        RestartPenalty = rm.RestartPenalty;
        OutAreaFlashBackDeathCost = rm.OutAreaFlashBackDeathCost;
        InAreaFlashBackDeathCost = rm.InAreaFlashBackDeathCost;
        SuddenDeathMode = rm.SuddenDeathMode;
        TurnPenalty = rm.TurnPenalty;
        BaseMonsterLevel = rm.BaseMonsterLevel;
        BaseFireMasterLevel = rm.BaseFireMasterLevel;
        MonsterLevelIncreaseFactor = rm.MonsterLevelIncreaseFactor;
        ClassicBarricadeDestruction = rm.ClassicBarricadeDestruction;
        PreferedLabPosition = rm.PreferedLabPosition;
        DirectLabJump = rm.DirectLabJump;
        CloversPerCube = rm.CloversPerCube;
        QuestXPPerClover = rm.QuestXPPerClover;
        OutRandomResourceCreationChance = rm.OutRandomResourceCreationChance;
        MinimumRandomResourceTimer = rm.MinimumRandomResourceTimer;
        MaximumRandomResourceTimer = rm.MaximumRandomResourceTimer;
        OutMinimumRandomResourceTimer = rm.OutMinimumRandomResourceTimer;
        OutMaximumRandomResourceTimer = rm.OutMaximumRandomResourceTimer;
        RandomResourceWitherFactor = rm.RandomResourceWitherFactor;
        RandomResourceDarkWitherFactor = rm.RandomResourceDarkWitherFactor;
        RequiredMaxCapacityItem = rm.RequiredMaxCapacityItem;
        RequiredMaxCapacityAmount = rm.RequiredMaxCapacityAmount;
        GateCreationChance = rm.GateCreationChance;
        AllowFreeGateOpening = rm.AllowFreeGateOpening;
        CogStartBonus = rm.CogStartBonus;
        CogEndBonus = rm.CogEndBonus;
        CubeClearBonusCurve = rm.CubeClearBonusCurve;
        XpStartBonus = rm.XpStartBonus;
        XpEndBonus = rm.XpEndBonus;
        ShellStartBonus = rm.ShellStartBonus;
        ShellEndBonus = rm.ShellEndBonus;
        HoneycombStartBonus = rm.HoneycombStartBonus;
        HoneycombEndBonus = rm.HoneycombEndBonus;
        ScarabPunishment = rm.ScarabPunishment;
        SpawnResourceRange = rm.SpawnResourceRange;
        RelocatorCreationChance = rm.RelocatorCreationChance;
        MonsterLevelFactorInflation = rm.MonsterLevelFactorInflation;
        DynamicMonsterLevelAddPerDeath = rm.DynamicMonsterLevelAddPerDeath;
        AutoPickupNeighborArtifacts = rm.AutoPickupNeighborArtifacts;
        NailWorkingChance = rm.NailWorkingChance;
        MaximumDirtyAreas = rm.MaximumDirtyAreas;
        MaxCubes = rm.MaxCubes;
        MaximumUnits = rm.MaximumUnits;
        OnOxygenFlyingUnitsSpeedFactor = rm.OnOxygenFlyingUnitsSpeedFactor;
        PerfectAreaKeyWeight = rm.PerfectAreaKeyWeight;
        MarkedAreaChance = rm.MarkedAreaChance;
        MarkedMonsterChance = rm.MarkedMonsterChance;
        BaseWakeUpTurnAmount = rm.BaseWakeUpTurnAmount;
        ForcedFrontalMovementDistance = rm.ForcedFrontalMovementDistance;
        AltarEvolutionFactor = rm.AltarEvolutionFactor;
        BaseRestingDistance = rm.BaseRestingDistance;
        StartingGotoCheckPointLevel = rm.StartingGotoCheckPointLevel;
        HarpoonBaseSpeed = rm.HarpoonBaseSpeed;
        HarpoonBonusAttack = rm.HarpoonBonusAttack;
        GotoCheckPointResourceCost = rm.GotoCheckPointResourceCost;
        AutoOpenGateResourceCost = rm.AutoOpenGateResourceCost;
        RestingNeedLOSCheck = rm.RestingNeedLOSCheck;
        BaseRandomResourceChance = rm.BaseRandomResourceChance;
        BaseFrontalProtectionChance = rm.BaseFrontalProtectionChance;
        BaseFrontSideProtectionChance = rm.BaseFrontSideProtectionChance;
        QuestSecrets = rm.QuestSecrets;
        RevealLabFog = rm.RevealLabFog;
        RevealCubeFog = rm.RevealCubeFog;
        AutoBuyDirectPurchase = rm.AutoBuyDirectPurchase;
        BurningBarricadeDamagePercent = rm.BurningBarricadeDamagePercent;
        BarricadeDestroyInflation = rm.BarricadeDestroyInflation;
        DragonShotImpactForce = rm.DragonShotImpactForce;
        BarricadeDestroyHeroWaitTime = rm.BarricadeDestroyHeroWaitTime;
        BarricadeDestroyWaitTime = rm.BarricadeDestroyWaitTime;
        FrontalProtectionMonsterBonus = rm.FrontalProtectionMonsterBonus;
        FrontSideProtectionMonsterBonus = rm.FrontSideProtectionMonsterBonus;
        MinFrontalProtectionSector = rm.MinFrontalProtectionSector;
        MinFrontSideProtectionSector = rm.MinFrontSideProtectionSector;
        PoisonerFriendlyFireAttackRate = rm.PoisonerFriendlyFireAttackRate;
        MinimumMarkedAreaSector = rm.MinimumMarkedAreaSector;
        MarkedScarabChance = rm.MarkedScarabChance;
        MaxFrontalTargetManeuverDist = rm.MaxFrontalTargetManeuverDist;
        MinFrontalTargetManeuverDist = rm.MinFrontalTargetManeuverDist;
        DefaultZoom = rm.DefaultZoom;
        LockedZoom = rm.LockedZoom;
        MinSpiderMergeAmount = rm.MinSpiderMergeAmount;
        CameraFrontFacingDist = rm.CameraFrontFacingDist;
        MinPoisonBiteSector = rm.MinPoisonBiteSector;
        PoisonBiteMonsterChance = rm.PoisonBiteMonsterChance;
        PoisonBiteRoachChance = rm.PoisonBiteRoachChance;
        PoisonBiteMosquitoChance = rm.PoisonBiteMosquitoChance;
        PoisonBiteMonsterBonus = rm.PoisonBiteMonsterBonus;
        SlimeShotPassThroughBonus = rm.SlimeShotPassThroughBonus;
        SlimeRotationSpeed = rm.SlimeRotationSpeed;
        OutsideBleedingHP = rm.OutsideBleedingHP;
        InsideDirtyBleedingHP = rm.InsideDirtyBleedingHP;
        InsideCleanBleedingHP = rm.InsideCleanBleedingHP;
        InsideLabBleedingHP = rm.InsideLabBleedingHP;
        PlatformBleedingHP = rm.PlatformBleedingHP;
        CubeDeathDamage = rm.CubeDeathDamage;
        HeroStartingHP = rm.HeroStartingHP;
        CubeClearHPBonus = rm.CubeClearHPBonus;
        RescueToGate = rm.RescueToGate;
        RescueEnabled = rm.RescueEnabled;
        DefaultMineType = rm.DefaultMineType;
        FishingBonusPerFlower = rm.FishingBonusPerFlower;
        NewbieDifficultyFactor = rm.NewbieDifficultyFactor;
        CasualDifficultyFactor = rm.CasualDifficultyFactor;
        NormalDifficultyFactor = rm.NormalDifficultyFactor;
        VeteranDifficultyFactor = rm.VeteranDifficultyFactor;
        HellDifficultyFactor = rm.HellDifficultyFactor;
        BaseFirePower = rm.BaseFirePower;
        FirePowerInflationPerBonfire = rm.FirePowerInflationPerBonfire;
        FireAttackBonusFactor = rm.FireAttackBonusFactor;
        BaseFireWoodNeeded = rm.BaseFireWoodNeeded;
        FirewoodNeededInflation = rm.FirewoodNeededInflation;
        BaseWoodRequiredForSale = rm.BaseWoodRequiredForSale;
        WoodForRunePrize = rm.WoodForRunePrize;
        RTDragonRangedAttackSpeed = rm.RTDragonRangedAttackSpeed;
        RTDragonMinAttSpeed = rm.RTDragonMinAttSpeed;
        RTDragonMaxAttSpeed = rm.RTDragonMaxAttSpeed;
        WaspOverWaterSpeedBonus = rm.WaspOverWaterSpeedBonus;
        FindNestRange = rm.FindNestRange;
        BaseDragonSlayerAngle = rm.BaseDragonSlayerAngle;
        BaseDragonSlayerMaxHP = rm.BaseDragonSlayerMaxHP;
        BaseJumperFlightTime = rm.BaseJumperFlightTime;
        JumperHeroAttackDistanceDecrease = rm.JumperHeroAttackDistanceDecrease;
        BaseMonsterTargetRadius = rm.BaseMonsterTargetRadius;
        BaseHeadShotRadius = rm.BaseHeadShotRadius;
        BaseEyeShotRadius = rm.BaseEyeShotRadius;
        MaxPoleDistanceForFishing = rm.MaxPoleDistanceForFishing;
        ExtraMonsterTargetRadiusPerLevel = rm.ExtraMonsterTargetRadiusPerLevel;
        StartingAdventureLevel = rm.StartingAdventureLevel;
        StartingTrainningLevel = rm.StartingTrainningLevel;
        QuickTravelEnabled = rm.QuickTravelEnabled;
        LockCubeJumpEnabled = rm.LockCubeJumpEnabled;
        BaseThreatDuration = rm.BaseThreatDuration;
        BaseHeroRangedAttackRange = rm.BaseHeroRangedAttackRange;
        BaseHeroMeleeAttackLevel = rm.BaseHeroMeleeAttackLevel;
        BaseMovementLevel = rm.BaseMovementLevel;
        BaseScoutLevel = rm.BaseScoutLevel;
        BaseHeroRangedAttackLevel = rm.BaseHeroRangedAttackLevel;
        LimitedArrowPerCube = rm.LimitedArrowPerCube;
        LimitedMeleeAttacksPerCube = rm.LimitedMeleeAttacksPerCube;
        BaseBambooAttackDamage = rm.BaseBambooAttackDamage;
        BaseArrowInLevel = rm.BaseArrowInLevel;
        BaseArrowOutLevel = rm.BaseArrowOutLevel;
        BaseHeroMeleeShieldLevel = rm.BaseHeroMeleeShieldLevel;
        SteppingMode = rm.SteppingMode;
        AutoScorpionSteppingMode = rm.AutoScorpionSteppingMode;
        HeroMeleeShieldBonusPerLevel = rm.HeroMeleeShieldBonusPerLevel;
        HeroRangedShieldBonusPerLevel = rm.HeroRangedShieldBonusPerLevel;
        BaseHeroRangedShieldLevel = rm.BaseHeroRangedShieldLevel;
        BaseThrowingAxeAttackDamage = rm.BaseThrowingAxeAttackDamage;
        BaseBambooAttackDamage = rm.BaseBambooAttackDamage;
        BaseSpearAttackDamage = rm.BaseSpearAttackDamage;
        BaseHookAttackDamage = rm.BaseHookAttackDamage;
        BaseKickDamage = rm.BaseKickDamage;
        BaseDestroyBarricadeLevel = rm.BaseDestroyBarricadeLevel;
        BasePlatformWalkingLevel = rm.BasePlatformWalkingLevel;
        BasePlatformSteps = rm.BasePlatformSteps;
        DefaulLoadItemType = rm.DefaulLoadItemType;
        DefaultLoadCost = rm.DefaultLoadCost;
        ChestInflationCummulative = rm.ChestInflationCummulative;
        QuestXPPerChest = rm.QuestXPPerChest;
        InitialUpgradeChestChance = rm.InitialUpgradeChestChance;
        CloverPickUpgradeChestChance = rm.CloverPickUpgradeChestChance;
        CubeClearUpgradeChestChance = rm.CubeClearUpgradeChestChance;
        OpenChestUpgradeChestChance = rm.OpenChestUpgradeChestChance;
        BaseChestPersistChance = rm.BaseChestPersistChance;
        MinimumDifficulty = rm.MinimumDifficulty;
        MaximumDifficulty = rm.MaximumDifficulty;
        StartingDifficultyLimit = rm.StartingDifficultyLimit;
        QueroQueroWaitTime = rm.QueroQueroWaitTime;
        BaseDragonFireDamage = rm.BaseDragonFireDamage;
        UpgradeListFolder = rm.UpgradeListFolder;
        TechListFolder = rm.TechListFolder;
        EnableQuickBuy = rm.EnableQuickBuy;
        MonsterPlatformExitCost = rm.MonsterPlatformExitCost;
        ReversedPlatformExitCost = rm.ReversedPlatformExitCost;
        OrientationPlatformExitCost = rm.OrientationPlatformExitCost;
        FreePlatformExitCost = rm.FreePlatformExitCost;
        GlobalPlatformAttBonusPerPoint = rm.GlobalPlatformAttBonusPerPoint;
        RestrictAreaExitToEntranceTile = rm.RestrictAreaExitToEntranceTile;
        RTHeroRangedAttackSpeed = rm.RTHeroRangedAttackSpeed;
        RTHeroRangedAttackDamageFactorr = rm.RTHeroRangedAttackDamageFactorr;
        RTHeroMeleeAttackSpeed = rm.RTHeroMeleeAttackSpeed;
        RTHeroMeleeAttackDamageFactorr = rm.RTHeroMeleeAttackDamageFactorr;
        RTMonsterRangedAttackDamageFactor = rm.RTMonsterRangedAttackDamageFactor;
        RTMonsterMeleeAttackDamageFactor = rm.RTMonsterMeleeAttackDamageFactor;
        RTBaseHeroMeleeAttackDamage = rm.RTBaseHeroMeleeAttackDamage;
        RTBaseHeroRangedAttackDamage = rm.RTBaseHeroRangedAttackDamage;
        RTMonsterMovementSpeed = rm.RTMonsterMovementSpeed;
        RTHeroStepPenaltyFactor = rm.RTHeroStepPenaltyFactor;
        RequiredBronzeTrophies = rm.RequiredBronzeTrophies;
        RequiredSilverTrophies = rm.RequiredSilverTrophies;
        RequiredGoldTrophies = rm.RequiredGoldTrophies;
        RequiredGeniusTrophies = rm.RequiredGeniusTrophies;
        RequiredDiamondTrophies = rm.RequiredDiamondTrophies;
        RequiredTrophyArea = rm.RequiredTrophyArea;
        RequiredItem = rm.RequiredItem;
        RequiredItemAmount = rm.RequiredItemAmount;
        RequiredItemLifeTime = rm.RequiredItemLifeTime;
        FishingBaseHookAttack = rm.FishingBaseHookAttack;
        BaseFishingTime = rm.BaseFishingTime;
        FishingBaseHookRadius = rm.FishingBaseHookRadius;
        FishingBaseHookSpeed = rm.FishingBaseHookSpeed;
        BaseFishingLevel = rm.BaseFishingLevel;
        FishingHookAttackPerLevel = rm.FishingHookAttackPerLevel;
        FishingHookRadiusPerLevel = rm.FishingHookRadiusPerLevel;
        FishingHookSpeedPerLevel = rm.FishingHookSpeedPerLevel;
        HarpoonMoveSensitivity = rm.HarpoonMoveSensitivity;
        HurryUpTimeScaleFactor = rm.HurryUpTimeScaleFactor;
        DifficultyBonusFactor = rm.DifficultyBonusFactor;
        BaseMiningLevel = rm.BaseMiningLevel;        
        LabModAction = new EModAction[ rm.LabModAction.Length ];
        LabModAction = ( EModAction[ ] ) rm.LabModAction.Clone();
        LabModValue = new float[ rm.LabModValue.Length ];
        LabModValue = ( float[ ] ) rm.LabModValue.Clone();
        MonsterShieldPerAttackAngle = new float[ rm.MonsterShieldPerAttackAngle.Length ];
        MonsterShieldPerAttackAngle = ( float[ ] ) rm.MonsterShieldPerAttackAngle.Clone();
        HeroShieldPerAttackAngle = new float[ rm.HeroShieldPerAttackAngle.Length ];
        HeroShieldPerAttackAngle = ( float[ ] ) rm.HeroShieldPerAttackAngle.Clone();
        BarricadeChanceHeight = new List<float>();
        BarricadeChanceHeight.AddRange( rm.BarricadeChanceHeight );
        GlobalResourcesList = new List<ItemType>();
        GlobalResourcesList.AddRange( rm.GlobalResourcesList );
        OutBarricadeChanceHeight = new List<float>();
        OutBarricadeChanceHeight.AddRange( rm.OutBarricadeChanceHeight );
        OutExtraBarricadeSize = new List<float>();
        OutExtraBarricadeSize.AddRange( rm.OutExtraBarricadeSize );
        InExtraBarricadeSize = new List<float>();
        InExtraBarricadeSize.AddRange( rm.InExtraBarricadeSize );
        AdventureUpgradeInfoList = new AdventureUpgradeInfo[ rm.AdventureUpgradeInfoList.Length ];
        AdventureUpgradeInfoList = ( AdventureUpgradeInfo[ ] ) rm.AdventureUpgradeInfoList.Clone();
        RequiredAdventureList = new List<EAdventureType>();
        RequiredAdventureList.AddRange( rm.RequiredAdventureList );
        ChestBonusItemList = new List<ItemType>();
        ChestBonusItemList.AddRange( rm.ChestBonusItemList );
        //SDList = rm.gameObject.GetComponentsInChildren<SectorDefinition>();  old before mixed implementation
        SDList = rm.SDList;
    }
    public void Init()
    {
        if( HeroData.I == null ) return;
        if( Map.I.RM.CurrentAdventure == -1 ) return;
        RandomMapData adv = Map.I.RM.RMList[ ( int ) Map.I.RM.CurrentAdventure ];

        if( adv.EnableAlternateStartingCube )                                                              // Alternate Starting Cube
        {
            string str = Map.I.RM.DungeonDialog.StartingCubePopup.value + "   ";
            if( str == "Restart...   ")
            {
                //Map.I.RM.StartingCube = 1;                                                               // Temporarilly disabled for safety (deletting data)
                //Item.SetAmt( ItemType.Starting_Cube, 0, 
                //Inventory.IType.Inventory, true, Map.I.RM.CurrentAdventure );
                //Map.I.RM.DungeonDialog.StartingCubePopup.value = "Start at Cube #1 ";
            }
            else
            {
                str = str.Substring( 15, 3 );
                Map.I.RM.StartingCube = int.Parse( str );
            }
        }

        if( Helper.I.ReleaseVersion == false )
        if( Helper.I.StartFromLastEditedCube )                                                                         // In editor jumped starting cube
        if( PlayCount <= 1 )
        {
            string resultString = Regex.Match( MapSaver.I.CurrentCube, @"\d+" ).Value;
            Map.I.RM.StartingCube = int.Parse( resultString );
            Map.I.RM.DungeonDialog.UpdateAlternateStartingCube();
            Map.I.RM.DungeonDialog.StartingCubePopup.value = "Start at Cube #" + Map.I.RM.StartingCube + " ";
            PlayCount++;
        }

        Map.I.RM.AvailableCubesForPlaying = Map.I.RM.RMD.InitiallyAvailableCubes + ( int )
        AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.INCREASE_AVAILABLE_CUBES );
        if( Map.I.RM.AvailableCubesForPlaying > MaxCubes ) Map.I.RM.AvailableCubesForPlaying = MaxCubes;

        if( Helper.I.FreePlay )
            Map.I.RM.AvailableCubesForPlaying = MaxCubes;

        HeroData.I.AreaPoints = 0;
        Quest.CurrentDungeon = ( int ) LabType;                   
        DungeonDialog dd = Map.I.RM. DungeonDialog.GetComponent<DungeonDialog>();
          
       RevealLabFog = !Map.I.RM.DungeonDialog.BlindModeToggle.value;
       AutoBuyMode = Map.I.RM.DungeonDialog.AutoBuyModeToggle.value;

       Map.I.ExtraHeroHP = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.INITIAL_HP );

       for( int i = 0; i < Manager.I.Inventory.ItemList.Count; i++ )                                                 // Warning: Dont use G.GIT here: the access is directly to the itemlist via loop and id    
       if ( Manager.I.Inventory.ItemList[ i ] )
       if ( Manager.I.Inventory.ItemList[ i ].IsGameplayResource )
       {
           Manager.I.Inventory.ItemList[ i ].Count = 0;
           Manager.I.Inventory.ItemList[ i ].TempCount = 0;
       }

       AdventureUpgradeInfo.InitialPrizeBonusList = new List<ItemType>();
       AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.INITIAL_ITEM_BONUS );

       for( int i = 0; i < AdventureUpgradeInfo.InitialPrizeBonusList.Count; i++ )                                   // initial bonus tech item add
       {
           float amt = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.INITIAL_ITEM_BONUS, 
           AdventureUpgradeInfo.InitialPrizeBonusList[ i ] );
          G.GIT( AdventureUpgradeInfo.InitialPrizeBonusList[ i ] ).StartingBonus = amt;
           Item.SetAmt( AdventureUpgradeInfo.InitialPrizeBonusList[ i ], amt );
       }

       for( int i = 0; i < Map.I.RM.RMD.GlobalResourcesList.Count; i++ )                                             // These are global resources
       {
           G.GIT( Map.I.RM.RMD.GlobalResourcesList[ i ] ).IsGlobalGameplayResource = true;
       }

       float fact = 0;
       string type = Map.I.RM.DungeonDialog.PlayerType.value;                                                       // Player type Difficulty factor
       if( type.Substring( 0, 1 ) == "1" ) fact = adv.NewbieDifficultyFactor;
       if( type.Substring( 0, 1 ) == "2" ) fact = adv.CasualDifficultyFactor;
       if( type.Substring( 0, 1 ) == "3" ) fact = adv.NormalDifficultyFactor;
       if( type.Substring( 0, 1 ) == "4" ) fact = adv.VeteranDifficultyFactor;
       if( type.Substring( 0, 1 ) == "5" ) fact = adv.HellDifficultyFactor;

       if( ShowPopup )
           Map.I.RM.DungeonDialog.PlayerType.gameObject.SetActive( true );
       else
           Map.I.RM.DungeonDialog.PlayerType.gameObject.SetActive( false );

       if( ShowDificultySlider )
           Map.I.RM.DungeonDialog.DificultySlider.gameObject.SetActive( true );
       else
           Map.I.RM.DungeonDialog.DificultySlider.gameObject.SetActive( false );

       float finalDif = dd.Dificulty + fact;
       MonsterDamageRate = Util.Percent( adv.MonsterDamageRate, finalDif );                                                 // Apply dificulty      

       //Debug.Log( "Game Started at Difficulty: " + finalDif + "% " + "Damage Rate: " + MonsterDamageRate );

       if( Helper.I.ReleaseVersion == false )
       {
           GateCreationChance = 100;
           PreferedLabPosition = new Vector2( 3, 4 ); 
       }        
    }
    
   public void ProcessScript( string txt )
    {
        List< string > str = new List<string>();
        str = Util.GetWords( txt );

        RandomMapData myObject = new RandomMapData();
        Type myType = typeof( RandomMapData );
        FieldInfo myFieldInfo = myType.GetField( str[ 0 ] );

        if( myFieldInfo != null )
        {
            float val = float.Parse( str[ 1 ] );
            typeof( RandomMapData ).GetField( str[ 0 ] ).SetValue( Map.I.RM.RMD, val );
        }
        else
            Debug.Log( "Command not Valid: " + str[ 0 ] );
    }

   public List<int> GetTrophiesAvailable( int adv )
   {
       var tl = new List<int> { 0, 0, 0, 0, 0, 0 }; 
       for( int i = 0; i < GoalList.Length; i++ )
       {
           RandomMapGoal go = Map.I.RM.RMList[ adv ].GoalList[ i ];
           if( go.TrophyType == ETrophyType.BRONZE     ) tl[ 0 ]++;
           if( go.TrophyType == ETrophyType.SILVER     ) tl[ 1 ]++;
           if( go.TrophyType == ETrophyType.GOLD       ) tl[ 2 ]++;
           if( go.TrophyType == ETrophyType.DIAMOND    ) tl[ 3 ]++;
           if( go.TrophyType == ETrophyType.ADAMANTIUM ) tl[ 4 ]++;
           if( go.TrophyType == ETrophyType.GENIUS     ) tl[ 5 ]++;
       }
       return tl;
   }
   public int GetRequiredItemAmount()
   {
       int amountneeded = Map.I.RM.RMD.RequiredItemAmount - ( int )
       AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.REDUCE_REQUIRED_ITEM_AMOUNT );
       return amountneeded;
   }
}
