using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;
using DigitalRuby.LightningBolt;
using PathologicalGames;

#region Enums
public enum EControlType
{
    NONE = -1, HUMAN, ROACH, RAM, ROOK, BALL, DRAGON1, DRAGON2, QUEROQUERO, JUMPER,
    MOTHERJUMPER, WASP, MOTHERWASP, SLIME, RAFT, FISH, HERB,
    FROG, FOG, FIREBALL, PROJECTILE,
    FLYING_PUSHABLE, IRON_BALL, BOUNCING_BALL
}
public enum EMoveType
{
    NONE = -1, FRONT, FRONTSIDE, SIDE, BACKSIDE, BACK, ROTATE, STILL
}
public enum EActionType
{
    NONE = -1, MOVE_N, MOVE_NE, MOVE_E, MOVE_SE, MOVE_S, MOVE_SW, MOVE_W, MOVE_NW, ROTATE_CW, ROTATE_CCW, WAIT, SPECIAL, ADVANCE_VCR, BATTLE
}
#endregion

[System.Serializable]
public partial class Controller : MonoBehaviour
{
    #region Variables
    [TabGroup( "" )]
    [PropertyTooltip( "Tab 1" )]
    bool clear = false;
    [TabGroup( "Links" )]
    public Unit Unit, Mother, Nest;
    [TabGroup( "Links" )]
    public tk2dSprite RestingRadiusSprite, RoachGlueSprite;
    [TabGroup( "Main" )]
    public EControlType ControlType;
    [TabGroup( "Main" )]
    public Vector2 OldPos;      // Oldpos Only changes if the hero has move between different tiles
    [TabGroup( "Main" )]
    public Vector2 LastPos;     // LastPos changes even if the move has failed
    [TabGroup( "Main" )]
    public Vector2 JumpTarget, AnimationOrigin;
    [TabGroup( "Level" )]
    public int MoveOrderID, RamStraightMoveCount, PlatformWalkingCounter,
    FlightJumpPhase, ForcedFrontalMovementFactor;
    [TabGroup( "Level" )]
    public float MovementLevel, ArrowWalkingLevel, ArrowInLevel, ArrowOutLevel, MonsterCorneringLevel, ScoutLevel, MonsterPushLevel,
                 PlatformWalkingLevel, SprinterLevel, SneakingLevel, BarricadeFighterLevel, EvasionLevel, EvasionPoints,
                 PerfectionistLevel, ScavengerLevel, ArrowFighterLevel, OverBarricadeScoutLevel, ShowResourceChance,
                 FlightSpeedFactor, FlightSpeedFactorTimer, FlightTime, FlightTargetTime, AttackSpeedLerpTimer,
                 ShowResourceNeighborsChance, SpeedTimeCounter, RealtimeSpeedFactor, MireLevel, RestingLevel,
                 SlayerLevel, FlyingTargetting, SlayerAngle, SlayerMaxHP, DragonDisguiseLevel, DragonBonusDropLevel,
                 DragonBarricadeProtection, SpawnTimer, TotSpawnTimer, PlatformSteps, FrogHeroBlockTimeCounter, FlightSpeed, OriginalZCord = -100;

    [TabGroup( "Bool" )]
    public bool NeedLineOfSightToMove, HasLineOfSight;
    [TabGroup( "Bool" )]
    public bool FirstMoveDone, Initialized, BeingMudPushed = false, BoulderDirChanged, BrokenRaft = false;
    [TabGroup( "Link" )]
    public LightningBoltScript RestLine;
    [TabGroup( "Move" )]
    public bool TickBasedMovement = false;
    [TabGroup( "Move" )]
    public EDirection ForcedFrontalMovementDir = EDirection.NONE;
    [TabGroup( "Graph" )]
    public float ShakeTimer;
    [TabGroup( "Graph" )]
    public Vector3 NotAnimatedPosition;
    [TabGroup( "Graph" )]
    public LineRenderer FishingLine;
    [TabGroup( "Move" )]
    public float RealtimeSpeed = 0;
    [TabGroup( "Move" )]
    public int SpeedListID = 0;
    [TabGroup( "Move" )]
    public int StairStepDir = -1;
    [TabGroup( "Main" )]
    public int WakeUpGroup = -1;
    [TabGroup( "Move" )]
    public int SkipMovement = 0;
    [TabGroup( "Move" )]
    public LightningBoltScript BoltEffect = null;
    [TabGroup( "Move" )]
    public float TimeforNextEffect = 0;
    [TabGroup( "Move" )]
    public int PushStrenght = 1;
    [TabGroup( "Fly" )]
    public int WaspClimbBarricadeNumber = 0;
    [TabGroup( "Links" )]
    public MyPathfinding PathFinding;
    [TabGroup( "Move" )]
    public EActionType LastAction;
    [TabGroup( "Move" )]
    public EActionType LastMoveAction = EActionType.NONE;   // Not includding rotation and wait or special
    [TabGroup( "Move" )]
    public EMoveType LastMoveType;
    [TabGroup( "bool" )]
    public bool IsSuspended, CanPushObjects, CanPushOrthogonally, RealtimeMoveTried,
                CanPushDiagonally, CanBePushed, BlockedByArrow, ArrowRushEnabled, Sleeping, OriginalSleeping, Resting, OriginalResting,
                IsBeingPushedAgainstObstacle, IsBeingPushedByHero, LockMeleeTarget, TileChanged;

    [TabGroup( "Bool" )]
    public bool FrontalRebound = true;
    [TabGroup( "Move" )]
    public ETileType DeniedTerrain = ETileType.NONE;
    [TabGroup( "Move" )]
    public ETileType AllowedTerrain = ETileType.NONE;                                   // Movement terrain exception
    [TabGroup( "Move" )]
    public EActionType ForceMove;
    [TabGroup( "Move" )]
    public List<Vector2> OriginalPositionHistory, PositionHistory;
    [TabGroup( "Move" )]
    public List<Vector2> HuggerStepList = null;
    [TabGroup( "Fly" )]
    public float FlightPhaseTimer = 0, FlightStepPhaseTimer = 0;
    [TabGroup( "Fly" )]
    public EProjectileType ProjectileType = EProjectileType.NONE;
    [TabGroup( "Graph" )]
    public float SmoothMovementFactor, TurnTime;
    [TabGroup( "Fly" )]
    public static int JumperCount = 0, FinishedJumperCount = 0;
    [TabGroup( "Fly" )]
    public bool IsFlyingUnit = false;
    [TabGroup( "Fly" )]
    public Vector2 FlyingTarget = new Vector2( -1, -1 );
    [TabGroup( "Fly" )]
    public Vector2 FlyingOrigin = new Vector2( -1, -1 );
    [TabGroup( "Fly" )]
    public EDirection FlightDir = EDirection.NONE;
    [TabGroup( "Fly" )]
    public Vector2 LastFlownOverTile = new Vector2( -1, -1 );
    [TabGroup( "Fly" )]
    public EDirection FlyingAngle = EDirection.NONE;
    [TabGroup( "Fly" )]
    public List<Vector2> WaspOccupiedTiles = new List<Vector2>();
    [TabGroup( "Move" )]
    public float SpikeFreeStepTime = 0;
    [TabGroup( "Fly" )]
    public static int WaspCount = 0;
    [TabGroup( "Fly" )]
    public static int MotherWaspCount = 0;
    [TabGroup( "Fly" )]
    public int FireMarkedWaspFactor = 0;
    [TabGroup( "Fly" )]
    public bool FireMarkAvailable = true;
    [TabGroup( "Main" )]
    public int BaseRestDistance = 0;
    [TabGroup( "Bool" )]
    public static bool SnowSliding = false;
    [TabGroup( "Bool" )]
    public bool UnitProcessed = false;
    [TabGroup( "Bool" )]
    public static bool SandSliding = false;
    [TabGroup( "Bool" )]
    public static bool MoveTickDone = false;
    [TabGroup( "Move" )]
    public float BoulderPunchPower = 100;
    [TabGroup( "Move" )]
    public float Current = 100;
    [TabGroup( "Bool" )]
    public bool UnitMoved = false;
    [TabGroup( "Main" )]
    public EDirection InitialDirection = EDirection.NONE;
    [TabGroup( "Main" )]
    public EDirection FixedRestingDirection = EDirection.NONE;
    [TabGroup( "Main" )]
    public bool RestingDirectionIsSameAsHero = false;
    [TabGroup( "Dir" )]
    public EDirection RaftMoveDir = EDirection.NONE;
    [TabGroup( "Move" )]
    public int RaftGroupID = -1;
    [TabGroup( "Move" )]
    public int Floor = 0, OldFloor = 0, ConsecutiveStillMove = 0;
    [TabGroup( "Move" )]
    public List<int> TickMoveList, VineList, VineColorList, ForceVineLink;
    [TabGroup( "Dynamic" )]
    public EDynamicObjectSortingType MovementSorting = EDynamicObjectSortingType.SEQUENCE;
    [TabGroup( "Dynamic" )]
    public List<EActionType> DynamicObjectMoveList = null;
    [TabGroup( "Dynamic" )]
    public List<Vector2> DynamicObjectJumpList = null;
    [TabGroup( "Dynamic" )]
    public EDynamicObjectSortingType OrientationSorting = EDynamicObjectSortingType.SEQUENCE;
    [TabGroup( "Dynamic" )]
    public List<EOrientation> DynamicObjectOrientationList = null;
    [TabGroup( "Dynamic" )]
    public List<EDirection> DynamicObjectDirectionList = null;
    [TabGroup( "Dynamic" )]
    public List<int> RandomMoveTurnList = null;
    [TabGroup( "Dynamic" )]
    public bool WaitAfterObstacleCollision = true;
    [TabGroup( "Dynamic" )]
    public List<int> RandomDirTurnList = null;
    [TabGroup( "Dynamic" )]
    public int DynamicMovementCurrentStep = 0;
    [TabGroup( "Dynamic" )]
    public int DynamicOrientationCurrentStep = 0;
    [TabGroup( "Dynamic" )]
    public int DynamicMaxSteps = 0;
    [TabGroup( "Dynamic" )]
    public int DynamicStepCount = 0;
    [TabGroup( "Dynamic" )]
    public List<tk2dSprite> RaftJointList;
    [TabGroup( "Fly" )]
    public float BaseWaspTotSpawnTimer = 30;
    [TabGroup( "Fly" )]
    public float WaspSpawnInflationPerTile = .1f;
    [TabGroup( "Fly" )]
    public float ExtraWaspSpawnTimerPerTile = 2;
    [TabGroup( "Fly" )]
    public float ExtraWaspSpawnInflationPerSec = 1f;
    [TabGroup( "Fly" )]
    public float MaxSpawnCount = 20;
    [TabGroup( "Fly" )]
    public float WaspDamage = 2;
    [TabGroup( "Fly" )]
    public int MotherWaspRadius = 5;
    [TabGroup( "Fly" )]
    public int SpawnCount = 0, ShieldedWaspCount = 0, CocoonWaspCount = 0, EnragedWaspCount = 0, ExtraEnragedSpawns = 0;
    [TabGroup( "Fly" )]
    public float WaspInflationSum = 0;
    [TabGroup( "Fly" )]
    public float TargettingRadiusFactor = 100;
    [TabGroup( "Move" )]
    public int CurFA = 0;
    [TabGroup( "Move" )]
    public float WaitTime = -1;
    [TabGroup( "Move" )]
    public float SwimmingDepht;
    [TabGroup( "Move" )]
    public bool IsBeingPushed = false;
    [TabGroup( "Move" )]
    public float ActionTimeCounter = -1;  // action time counter
    [TabGroup( "Move" )]
    public float WaitTimeCounter = -1;  // wait time counter
    [TabGroup( "Move" )]
    public float WakeupTimeCounter = -1, WakeupTotalTime = -1;
    [TabGroup( "Move" )]
    public bool EggsDestroyed = false;
    [TabGroup( "Move" )]
    public int SpiderAttackBlockPhase = 0;
    [TabGroup( "bool" )]
    public bool FishActionWaiting = false;
    public static bool HeroJumped = false;
    public static bool MoveMade = false;
    public static bool IsRealtime = false;
    public static bool HeroBeingPushedByFog = false;
    public static bool PlayBumpSound = false;
    public static bool IgnoreLeverPush = false;
    public static List<Unit> IgnoreLevelPushList = new List<Unit>();
    [TabGroup( "Move" )]
    public int MergingID = -1;
    [TabGroup( "Move" )]
    public int HeroPushedMonsterBarricadeDestroyCount = 0;
    [TabGroup( "Float" )]
    public float SideFlightSpeed = 0;
    [TabGroup( "Float" )]
    public float SideFlightTime = 0;
    [TabGroup( "Float" )]
    public float FishRedLimit = 70;
    [TabGroup( "Float" )]
    public float FishGreenLimit = 30;
    [TabGroup( "Float" )]
    public float FishCaughtTimeCount = 0;
    [TabGroup( "Float" )]
    public float FishingPowerFactor = 100;
    [TabGroup( "Float" )]
    public float MaxFishSpeed;
    [TabGroup( "Float" )]
    public float MinFishSpeed;
    [TabGroup( "Float" )]
    public float OverFishSecondsNeededPerUnit = -1; // Aproximate value in seconds since distance from hook influences it (50% to 150%)  set to -1 to invalidade bonuses at this quest
    [TabGroup( "Float" )]
    public float OverFishTimeCount = 0;             // for fish bonus purpose. resets when hook is moved away from fish
    [TabGroup( "Float" )]
    public float FAOverFishTimeCount = 0;           // tosal seconds hook has been over fish
    [TabGroup( "Float" )]
    public float FAOverFishTimeCountPerFA = 0;      // total seconds hook has been over fish. Resets after FA has changed
    [TabGroup( "Float" )]
    public float HeroBlockFrogTotalTime = 1;
    [TabGroup( "Float" )]
    public int PoleExtraLevel = 0;
    [TabGroup( "Enum" )]
    public EFishSwimType FishSwimType = EFishSwimType.NONE;
    [TabGroup( "Float" )]
    public float RespawnTimeCount = 0;
    [TabGroup( "Float" )]
    public float BaseTotalRespawnTime = 10;
    [TabGroup( "Int" )]
    public int BaseTotalRespawnAmount = 0;
    [TabGroup( "Int" )]
    public int RespawnCount = 0;
    [TabGroup( "bool" )]
    public bool RandomizePositionOnRespawn = false;
    [TabGroup( "bool" )]
    public bool RandomizePositionOnPoleStep = false;
    [TabGroup( "bool" )]
    public bool FrogBlockedByHero = false;
    [TabGroup( "float" )]
    public float FishingBonusAmount = 1;
    public static int HerbCount = 0;
    public static float BarricadeBumpTimeCount = 0;
    public static bool CheckHeroSwordBlock = true, CheckingLure = false;
    [TabGroup( "Fly" )]
    public List<Vector2> FlyingActionTarget;
    [TabGroup( "Fly" )]
    public List<FishActionStruct> FishAction;
    [TabGroup( "Fly" )]
    public List<float> FlyingActionVal;
    [TabGroup( "Fly" )]
    public List<int> FlyingActionLoopCount;
    [TabGroup( "Fly" )]
    public List<float> FlightActionTotalTime;
    [TabGroup( "Fly" )]
    public float FlyingRotationSpeed = 75;
    [TabGroup( "Fly" )]
    public float FlyingSpeed = 0;
    [TabGroup( "Fly" )]
    public float FlyingMaxSpeed = 2;
    [TabGroup( "Fly" )]
    public float SideFlightZigZagPhase = 0;
    [TabGroup( "Fly" )]
    public float FlyingAcceleration = 0;
    [TabGroup( "Fly" )]
    public float SideFlightZigZagDistance = 0;
    [TabGroup( "Fly" )]
    public float SideFlightZigZagTimeFactor = 1;
    [TabGroup( "Fly" )]
    public float TotalSideFlightTime = .5f;
    [TabGroup( "Fly" )]
    public float TotalSideFlightSpeed = 0;
    [TabGroup( "Fly" )]
    public float FlightActionTime = 0;
    [TabGroup( "Fly" )]
    public float PhaseWonFreePhases = 0;
    [TabGroup( "Fly" )]
    public int FrontalFlyingTargetDist = -1;
    [TabGroup( "Fly" )]
    public int FlightPhaseLoopCount = 0;
    [TabGroup( "Fly" )]
    public int FlightLoopCount = 0;
    [TabGroup( "Fly" )]
    public MyBool FlightResourcePicking = MyBool.FALSE;
    [TabGroup( "Fly" )]
    public MyBool FlightOrbStrike = MyBool.FALSE;
    [TabGroup( "Fly" )]
    public MyBool FlightBarricadeDestroy = MyBool.FALSE;
    [TabGroup( "Fly" )]
    public MyBool FlightWallClash = MyBool.FALSE;
    [TabGroup( "Fly" )]
    public List<float> CastProjectileList = new List<float>();
    [TabGroup( "Main" )]
    public Vector2 WhiteGateGroup = new Vector2( -1, -1 );
    public static bool ResetMergingID = false;
    [TabGroup( "List" )]
    public List<EDirection> RedRoachBabyList;
    [TabGroup( "float" )]
    public float HeroNeighborTimeCount = 0;
    public static Vector2 HeroBumpTarget = new Vector2( -1, -1 );
    public static bool UnitHasBeenKilledWhileMoving = false;
    public static bool HeroMovedFromSlidingTerrainToMud = false;
    public static int SnowEnterFrameCount = 0;
    public static float BumperTimeCount = 100;
    public static EDirection BoulderBumpDir = EDirection.NONE;
    public static List<Vector2> BumpPositionList = new List<Vector2>();
    public List<Unit> SpiderChild = new List<Unit>();
    public static List<int> TrapDropList = new List<int>();
    public static List<int> DropTrapGroupList;
    public static List<Unit> MovedVineList = new List<Unit>();
    public static float SlideTerrainExitWaitTime = 0.03f;
    public static float SlideTimeCount = 0;
    public static Vector3 OldSnowSlidingPosition = Vector3.zero;
    public static Vector3 OldSandSlidingPosition = Vector3.zero;
    public static bool MaskMoveInitiated = false;
    public static float CornPickedAmt = 0;
    public static bool RaftMoving = false;
    public static bool CornAlreadyCharged = false;
    public static bool MudPushing = false;
    public static bool ForceDiagonalMovement = false;
    public static bool MaskMoveFinalized = false;
    public static bool StitchesPunishment = true;
    public static bool MineSwitching = false, BlockStepping = false;
    public static bool LowTerrainPush = false;
    public static bool FromBrokenRaft = false;
    public static Unit PassiveBoomerangAttackTg = null;
    static List<Vector2> BridgeSupportPoints = new List<Vector2>();
    public static bool ForceBoomerangAttack = false;
    public static Vector2 LastResPosCollectedByHeroPos = new Vector2( -1, -1 );
    public static Vector2 CurentMoveTrial;
    public static List<Unit> FogKillList = null;
    public static Vector3 InputVector;
    public static List<Vector3> InputVectorList;
    public static int FrontalTargetManeuverDist = 0;
    public static bool AttemptMining = false;
    public static Vector2 LastVelocity;
    public static int RaftStepCount = -1;
    static public Vector2 AuxBridgePos;
    static public EDirection AuxBridgeDir;
    public EFishSwimType EndingFishSwimType;
    #endregion
    void Start()
    {
        OldPos = new Vector2( -1, -1 );
        LastPos = new Vector2( -1, -1 );
        AnimationOrigin = new Vector2( -1, -1 );
        FirstMoveDone = false;
        ForceMove = EActionType.NONE;
    }

    public void Save()
    {
        GS.W.Write( Resting );
        GS.W.Write( WakeUpGroup );
        GS.W.Write( SpeedTimeCounter );
        //GS.W.Write( MoveSpeedFactor );
        GS.W.Write( MoveOrderID );
        GS.SVector2( LastPos );
        GS.W.Write( TurnTime );
        GS.W.Write( WaitTimeCounter );
        GS.W.Write( IsSuspended );
        GS.W.Write( RaftGroupID );
        GS.W.Write( PlatformWalkingCounter );
        GS.W.Write( ( int ) LastMoveType );
        GS.W.Write( ( int ) LastAction );
        GS.W.Write( ( int ) LastMoveAction );
        GS.SVector2( FlyingTarget );
        GS.W.Write( FlyingTargetting );
        GS.W.Write( FrontalFlyingTargetDist );
        GS.W.Write( Floor );
        GS.W.Write( OldFloor );
        GS.W.Write( MaxFishSpeed );
        GS.W.Write( MinFishSpeed );
        GS.W.Write( FlyingSpeed );
        GS.W.Write( ( int ) InitialDirection );
        GS.W.Write( FlightJumpPhase );
        GS.W.Write( FlightPhaseTimer );
        GS.W.Write( FlightPhaseLoopCount );
        GS.W.Write( FlightLoopCount );
        GS.SVector2( FlyingOrigin );
        GS.W.Write( FlyingRotationSpeed );
        GS.W.Write( FlyingMaxSpeed );
        GS.W.Write( FlyingAcceleration );
        GS.W.Write( ( int ) FlyingAngle );
        GS.W.Write( FlightSpeedFactor );
        GS.W.Write( FlightSpeedFactorTimer );
        GS.W.Write( FlightTime );
        GS.W.Write( FlightTargetTime );
        GS.W.Write( SideFlightSpeed );
        GS.W.Write( SideFlightZigZagDistance );
        GS.W.Write( SideFlightZigZagTimeFactor );
        GS.W.Write( TotalSideFlightTime );
        GS.W.Write( TotalSideFlightSpeed );
        GS.W.Write( SideFlightTime );
        GS.W.Write( ActionTimeCounter );

        GS.W.Write( FlightStepPhaseTimer );
        GS.W.Write( FlightActionTime );
        GS.W.Write( SideFlightZigZagPhase );
        GS.W.Write( PhaseWonFreePhases );
        GS.W.Write( EggsDestroyed );

        if( Unit.UnitType == EUnitType.HERO )
        {
            GS.W.Write( MovementLevel );
            GS.W.Write( ArrowWalkingLevel );
            GS.W.Write( ArrowInLevel );
            GS.W.Write( ArrowOutLevel );
            GS.W.Write( BarricadeFighterLevel );
            GS.W.Write( PlatformWalkingLevel );
            GS.W.Write( PlatformSteps );
        }

        bool dyn = IsDynamicObject();
        GS.W.Write( dyn );
        if( dyn )
        {
            GS.W.Write( DynamicMovementCurrentStep );
            GS.W.Write( DynamicOrientationCurrentStep );
            GS.W.Write( DynamicMaxSteps );
            GS.W.Write( DynamicStepCount );
            GS.W.Write( WaitAfterObstacleCollision );
            GS.SList( DynamicObjectMoveList );
            GS.SList( DynamicObjectOrientationList );
            GS.SV2List( DynamicObjectJumpList );
            GS.SList( DynamicObjectDirectionList );
            GS.SList( RandomMoveTurnList );
        }

        int sz = FlyingActionVal.Count;
        GS.W.Write( sz );
        for( int i = 0; i < FlyingActionVal.Count; i++ )
            GS.W.Write( FlyingActionVal[ i ] );

        sz = FlightActionTotalTime.Count;
        GS.W.Write( sz );
        for( int i = 0; i < FlightActionTotalTime.Count; i++ )
            GS.W.Write( FlightActionTotalTime[ i ] );

        sz = FlyingActionLoopCount.Count;
        GS.W.Write( sz );
        for( int i = 0; i < FlyingActionLoopCount.Count; i++ )
            GS.W.Write( FlyingActionLoopCount[ i ] );

        if( Unit.TileID == ETileType.VINES )
        {
            sz = VineList.Count;
            GS.W.Write( sz );
            for( int i = 0; i < VineList.Count; i++ )
                GS.W.Write( VineList[ i ] );

            sz = ForceVineLink.Count;
            GS.W.Write( sz );
            for( int i = 0; i < ForceVineLink.Count; i++ )
                GS.W.Write( ForceVineLink[ i ] );
        }

        if( Unit.TileID == ETileType.DRAGON1 )
        {
            GS.W.Write( CastProjectileList.Count );
            for( int i = 0; i < CastProjectileList.Count; i++ )
                GS.W.Write( CastProjectileList[ i ] );
        }

        if( Unit.TileID == ETileType.SPIDER )
        {
            GS.W.Write( SpiderAttackBlockPhase );
        }

        if( Unit.TileID == ETileType.FISH )
        {
            GS.W.Write( FishAction.Count );
            for( int i = 0; i < FishAction.Count; i++ )
                FishAction[ i ].Save();
        }
        if( Unit.TileID == ETileType.RAFT )
            GS.W.Write( ( int ) RaftMoveDir );

        #region vars
        /*  GS.W.Write( IsBeingPushedByHero );
        GS.W.Write( RealtimeSpeedFactor );
        GS.W.Write( RealtimeSpeed );
        GS.W.Write( IsInfected );
        GS.W.Write( InfectedRadius );
        GS.SVector2( JumpTarget );
        GS.W.Write( IsFlyingUnit );
        GS.W.Write( SpawnTimer );
        GS.W.Write( SpawnCount );
        GS.W.Write( ShieldedWaspCount );
        GS.W.Write( CocoonWaspCount );
        GS.W.Write( EnragedWaspCount );
        GS.W.Write( BeingMudPushed );
        GS.W.Write( TotSpawnTimer );
        GS.W.Write( Initialized );
        GS.W.Write( ScorpionProcessed );
        GS.W.Write( RealtimeMoveTried );
        GS.W.Write( Mother );
        GS.SVector2( LastFlownOverTile );
        GS.W.Write( ( int ) FlightDir );
        GS.W.Write( FlightSpeed );
        GS.W.Write( TileChanged );
        GS.W.Write( ( int ) ProjectileType );
        GS.W.Write( SpeedListID );
        GS.W.Write( AttackSpeedLerpTimer );
        GS.W.Write( FireMarkAvailable );
        GS.W.Write( SkipMovement );
        GS.W.Write( WakeupTimeCounter );
        GS.W.Write( HeroPushedMonsterBarricadeDestroyCount );
        GS.W.Write( FrogHeroBlockTimeCounter );*/

        /*    GS.W.Write( IsBeingPushedAgainstObstacle );
            GS.W.Write( PushStrenght );
            GS.W.Write( CanPushDiagonally );
            GS.W.Write( CanPushOrthogonally );
            GS.W.Write( RamStraightMoveCount );
            GS.W.Write( CanPushObjects );
            GS.W.Write( CanBePushed );
            GS.W.Write( BlockedByArrow );
            GS.W.Write( ArrowRushEnabled );
            GS.W.Write( Sleeping );
            GS.W.Write( ( int ) DeniedTerrain );
            GS.W.Write( ( int ) AllowedTerrain );
            GS.W.Write( NeedLineOfSightToMove );
            GS.W.Write( HasLineOfSight );
            GS.W.Write( FirstMoveDone );
            GS.W.Write( LockMeleeTarget );
            GS.W.Write( MonsterPushLevel );
            GS.W.Write( MonsterCorneringLevel );
            GS.W.Write( ScoutLevel );
            GS.W.Write( SprinterLevel );
            GS.W.Write( SneakingLevel );
            GS.W.Write( EvasionLevel );
            GS.W.Write( EvasionPoints );
            GS.W.Write( PerfectionistLevel );
            GS.W.Write( ScavengerLevel );
            GS.W.Write( ArrowFighterLevel );
            GS.W.Write( OverBarricadeScoutLevel );
            GS.W.Write( ShowResourceChance );
            GS.W.Write( ShowResourceNeighborsChance );
            GS.W.Write( SlayerLevel );
            GS.W.Write( SlayerAngle );
            GS.W.Write( SlayerMaxHP );
            GS.W.Write( DragonDisguiseLevel );
            GS.W.Write( DragonBonusDropLevel );
            GS.W.Write( DragonBarricadeProtection );
            GS.W.Write( Nest );
            GS.W.Write( MireLevel );
            GS.W.Write( RestingLevel );
            GS.W.Write( BaseRestDistance );
            GS.W.Write( SmoothMovementFactor );
            GS.W.Write( BaseWaspTotSpawnTimer );
            GS.W.Write( ExtraEnragedSpawns );
            GS.W.Write( WaspSpawnInflationPerTile );
            GS.W.Write( MaxSpawnCount );
            GS.W.Write( ExtraWaspSpawnTimerPerTile );
            GS.W.Write( WaspDamage );
            GS.W.Write( MotherWaspRadius );
            GS.W.Write( SpikeFreeStepTime );
            GS.W.Write( WaspInflationSum );
            GS.W.Write( ExtraWaspSpawnInflationPerSec );
            GS.W.Write( WaitTime );
            GS.W.Write( FrontalRebound );
            GS.W.Write( TickBasedMovement );
            GS.W.Write( BoulderPunchPower );
            GS.W.Write( ( int ) MovementSorting );
            GS.W.Write( ( int ) OrientationSorting );
            GS.W.Write( WaspClimbBarricadeNumber );
            GS.W.Write( ForcedFrontalMovementFactor );
            GS.W.Write( SwimmingDepht );
            GS.W.Write( WakeupTotalTime );
            GS.W.Write( FishActionWaiting );
            GS.W.Write( FAOverFishTimeCount );
            GS.W.Write( FishRedLimit );
            GS.W.Write( FishGreenLimit );
            GS.W.Write( FishingLine );
            GS.W.Write( FishCaughtTimeCount );
            GS.W.Write( RespawnTimeCount );
            GS.W.Write( BaseTotalRespawnTime );
            GS.W.Write( BaseTotalRespawnAmount );
            GS.W.Write( RespawnCount );
            GS.W.Write( RandomizePositionOnRespawn );
            GS.W.Write( RandomizePositionOnPoleStep );
            GS.W.Write( PoleExtraLevel );
            GS.W.Write( ( int ) FishSwimType );
            GS.W.Write( FAOverFishTimeCountPerFA );
            GS.W.Write( FishingPowerFactor );
            GS.W.Write( FishingBonusAmount );
            GS.SVector2( WhiteGateGroup );
            GS.W.Write( TargettingRadiusFactor );
            GS.W.Write( HeroNeighborTimeCount );
            GS.W.Write( ( int ) FixedRestingDirection );
            GS.W.Write( RestingDirectionIsSameAsHero );
            GS.W.Write( OverFishTimeCount );
            GS.W.Write( FishingPoleExtraTime );
            GS.W.Write( OverFishSecondsNeededPerUnit );
            GS.W.Write( ( int ) ForcedFrontalMovementDir );
            GS.W.Write( FireMarkedWaspFactor );
            // GS.W.Write(   FlightResourcePicking );
            //GS.W.Write(    FlightOrbStrike );
            //GS.W.Write(    FlightBarricadeDestroy );
            // GS.W.Write( FlightWallClash);
            GS.W.Write( FrogBlockedByHero );
            GS.W.Write( HeroBlockFrogTotalTime );
            GS.W.Write( MergingID );
            GS.W.Write( CurFA );
            GS.W.Write( BoltEffect );
            GS.W.Write( TimeforNextEffect );
            GS.W.Write( WaspLongFlight );
            GS.W.Write( OriginalZCord );
            GS.W.Write( ConsecutiveStillMove );

            GS.W.Write( OriginalSleeping );
            GS.W.Write( OriginalResting );*/


        //Control.PositionHistory.Clear();
        //Control.WaspOccupiedTiles = new List<Vector2>();

        //Control.TickMoveList = new List<int>();
        //Control.TickMoveList.AddRange( un.Control.TickMoveList );

        //Control.RedRoachBabyList = new List<EDirection>();
        //Control.RedRoachBabyList.AddRange( un.Control.RedRoachBabyList );
        //Control.RandomDirTurnList = new List<int>();
        //Control.RandomDirTurnList.AddRange( un.Control.RandomDirTurnList );
        //Control.FlyingActionTarget = new List<Vector2>();
        //Control.FlyingActionTarget.AddRange( un.Control.FlyingActionTarget );

        #endregion
    }

    public void Load()
    {
        Resting = GS.R.ReadBoolean();
        WakeUpGroup = GS.R.ReadInt32();
        if( Resting )
        if( WakeUpGroup >= 0 )
        {
            Map.I.RestPos.Add( Unit.Pos );                              // rest line creation
            Map.I.RestID.Add( WakeUpGroup );
        }
        SpeedTimeCounter = GS.R.ReadSingle();
        MoveOrderID = GS.R.ReadInt32();
        LastPos = GS.LVector2();
        TurnTime = GS.R.ReadSingle();
        WaitTimeCounter = GS.R.ReadSingle();
        IsSuspended = GS.R.ReadBoolean();
        RaftGroupID = GS.R.ReadInt32();
        PlatformWalkingCounter = GS.R.ReadInt32();
        LastMoveType = ( EMoveType ) GS.R.ReadInt32();
        LastAction = ( EActionType ) GS.R.ReadInt32();
        LastMoveAction = ( EActionType ) GS.R.ReadInt32();
        FlyingTarget = GS.LVector2();
        FlyingTargetting = GS.R.ReadSingle();
        FrontalFlyingTargetDist = GS.R.ReadInt32();
        Floor = GS.R.ReadInt32();
        OldFloor = GS.R.ReadInt32();
        MaxFishSpeed = GS.R.ReadSingle();
        MinFishSpeed = GS.R.ReadSingle();
        FlyingSpeed = GS.R.ReadSingle();
        InitialDirection = ( EDirection ) GS.R.ReadInt32();
        FlightJumpPhase = GS.R.ReadInt32();
        FlightPhaseTimer = GS.R.ReadSingle();

        FlightPhaseLoopCount = GS.R.ReadInt32();
        FlightLoopCount = GS.R.ReadInt32();
        FlyingOrigin = GS.LVector2();
        FlyingRotationSpeed = GS.R.ReadSingle();
        FlyingMaxSpeed = GS.R.ReadSingle();
        FlyingAcceleration = GS.R.ReadSingle();

        FlyingAngle = ( EDirection ) GS.R.ReadSingle();
        FlightSpeedFactor = GS.R.ReadSingle();
        FlightSpeedFactorTimer = GS.R.ReadSingle();
        FlightTime = GS.R.ReadSingle();
        FlightTargetTime = GS.R.ReadSingle();
        SideFlightSpeed = GS.R.ReadSingle();
        SideFlightZigZagDistance = GS.R.ReadSingle();
        SideFlightZigZagTimeFactor = GS.R.ReadSingle();
        TotalSideFlightTime = GS.R.ReadSingle();
        TotalSideFlightSpeed = GS.R.ReadSingle();
        SideFlightTime = GS.R.ReadSingle();
        ActionTimeCounter = GS.R.ReadSingle();

        FlightStepPhaseTimer = GS.R.ReadSingle();
        FlightActionTime = GS.R.ReadSingle();
        SideFlightZigZagPhase = GS.R.ReadSingle();
        PhaseWonFreePhases = GS.R.ReadSingle();
        EggsDestroyed = GS.R.ReadBoolean();

        if( Unit.UnitType == EUnitType.HERO )
        {
            MovementLevel = GS.R.ReadSingle();
            ArrowWalkingLevel = GS.R.ReadSingle();
            ArrowInLevel = GS.R.ReadSingle();
            ArrowOutLevel = GS.R.ReadSingle();
            BarricadeFighterLevel = GS.R.ReadSingle();
            PlatformWalkingLevel = GS.R.ReadSingle();
            PlatformSteps = GS.R.ReadSingle();
        }

        bool dyn = GS.R.ReadBoolean();
        if( dyn )
        {
            DynamicMovementCurrentStep = GS.R.ReadInt32();
            DynamicOrientationCurrentStep = GS.R.ReadInt32();
            DynamicMaxSteps = GS.R.ReadInt32();
            DynamicStepCount = GS.R.ReadInt32();
            WaitAfterObstacleCollision = GS.R.ReadBoolean();
            DynamicObjectMoveList = GS.LList<EActionType>();
            DynamicObjectOrientationList = GS.LList<EOrientation>();
            DynamicObjectJumpList = GS.LV2List();
            DynamicObjectDirectionList = GS.LList<EDirection>();
            RandomMoveTurnList = GS.LList<int>(); 
        }

        int sz = GS.R.ReadInt32();
        FlyingActionVal = new List<float>();
        for( int i = 0; i < sz; i++ )
            FlyingActionVal.Add( GS.R.ReadSingle() );

        sz = GS.R.ReadInt32();
        FlightActionTotalTime = new List<float>();
        for( int i = 0; i < sz; i++ )
            FlightActionTotalTime.Add( GS.R.ReadSingle() );

        sz = GS.R.ReadInt32();
        FlyingActionLoopCount = new List<int>();
        for( int i = 0; i < sz; i++ )
            FlyingActionLoopCount.Add( GS.R.ReadInt32() );

        if( Unit.TileID == ETileType.VINES )
        {
            sz = GS.R.ReadInt32();
            VineList = new List<int>();
            for( int i = 0; i < sz; i++ )
                VineList.Add( GS.R.ReadInt32() );

            sz = GS.R.ReadInt32();
            ForceVineLink = new List<int>();
            for( int i = 0; i < sz; i++ )
                ForceVineLink.Add( GS.R.ReadInt32() );
        }
        if( Unit.TileID == ETileType.DRAGON1 )
        {
            sz = GS.R.ReadInt32();
            CastProjectileList = new List<float>();
            for( int i = 0; i < sz; i++ )
                CastProjectileList.Add( GS.R.ReadSingle() );
        }
        if( Unit.TileID == ETileType.SPIDER )
        {
            SpiderAttackBlockPhase = GS.R.ReadInt32();
        }
        if( Unit.TileID == ETileType.FISH )
        {
            sz = GS.R.ReadInt32();
            for( int i = 0; i < sz; i++ )
                FishAction[ i ].Load();
        }
        if( Unit.TileID == ETileType.RAFT )
            RaftMoveDir = ( EDirection ) GS.R.ReadInt32();
    }

    //______________________________________________________________________________________________________________________ Update Controls

    public void UpdateIt()
    {
        MoveMade = false;                                                     // Reset static variables
        UnitMoved = false;
        UnitHasBeenKilledWhileMoving = false;
        PlayBumpSound = true;
        IsBeingPushed = false;
        Unit.Body.ShacklePullList = new List<Unit>();
       
        if( Unit.UnitType != EUnitType.HERO )                                // Oxygen restriction
        if( Map.I.IsPaused() ) return;

        if( Resting ) return;                                                // Resting

        if( CheckTimer() == false ) return;                                  // Check Timer                                                       

        if( Spell.CheckSpellMoveBlock( Unit ) ) return;                      // Spell Move Blockade

        Vector2 oldPos = Unit.Pos;
        if( Unit.Body.IsDead ) return;
        if( Unit.Body.MakeImovable > 0 ) return;

        Vector2 dest = Vector2.zero;                                                                           // Dynamic Object Movement
        int destRot = 0;
        EDirection destRotTo = EDirection.NONE;
        bool res = UpdateDynamicObjectMovement( true, ref dest, ref destRot, ref destRotTo );

        if( res == false )
            switch( ControlType )
            {
                case EControlType.HUMAN: UpdateHumanMovement(); break;
                case EControlType.ROACH: UpdateRoachMovement(); break;
                case EControlType.RAM: UpdateRamMovement(); break;
                case EControlType.ROOK: UpdateRookMovement(); break;
                case EControlType.BALL: UpdateBoulderMovement(); break;
                case EControlType.DRAGON1: UpdateDragon1Movement(); break;
                case EControlType.DRAGON2: UpdateDragon2Movement(); break;
                case EControlType.QUEROQUERO: UpdateQueroQueroMovement(); break;
                case EControlType.WASP: UpdateWaspMovement(); break;
                case EControlType.SLIME: UpdateSlimeMovement(); break;
                case EControlType.RAFT: UpdateRaftMovement(); break;
                case EControlType.FISH: UpdateFishMovement(); break;
                case EControlType.HERB: UpdateHerbMovement(); break;
                case EControlType.FIREBALL: UpdateFireBallMovement(); break;
                case EControlType.FROG: UpdateFrogMovement(); break;
                case EControlType.FOG: UpdateFogMovement(); break;
                case EControlType.PROJECTILE: UpdateProjectileMovement(); break;
                case EControlType.FLYING_PUSHABLE: UpdateFlyingPushableMovement(); break;
                case EControlType.IRON_BALL: UpdateIronBallMovement(); break;
                case EControlType.BOUNCING_BALL: UpdateBouncingIronBallMovement(); break;
            }

        if( Unit.Pos == oldPos )
        {
            //if( Map.I.AdvanceTurn ) OldPos = Unit.Pos;
        }

        if( IsRealtime && Map.Stepping() == false )                                                // Do not let time accumulate
        {
            float tottime = GetRealtimeSpeedTime();

            if( Unit.TileID != ETileType.BOULDER )                                                 // For "stair" type cube precision
            if( SpeedTimeCounter > tottime )
                SpeedTimeCounter = tottime;

            if( MoveMade )
            {
                SpeedTimeCounter -= tottime;

                if( Unit.Md && Unit.Md.RealtimeSpeedList != null )                                 // Custom speed list movement type: increment ID
                    if( ++SpeedListID == Unit.Md.RealtimeSpeedList.Count )
                        SpeedListID = 0;
            }
        }
        PlayStepSoundFx();
    }

    private void PlayStepSoundFx()
    {
        if( MoveMade && ( Unit.UnitType == EUnitType.MONSTER || IsDynamicObject() ) )              // Monster Step Sound FX
        {
            string snd = "Monster Step";
            if( Unit.TileID == ETileType.SLIME ) snd = "Slime Step";
            else
                if( Unit.TileID == ETileType.ARROW ) snd = "Movement 1";
                else
                    if( Unit.TileID == ETileType.FROG )
                    {
                        MasterAudio.PlaySound3DAtVector3( "Water Splash", transform.position );        // Swimming Frog
                        snd = "Frog";
                    }
            if( Unit.TileID == ETileType.ALGAE ) snd = "Water Splash 2";
            MasterAudio.PlaySound3DAtVector3( snd, transform.position );
        }
    }

    //______________________________________________________________________________________________________________________ Apply Move

    public bool CheckTimer()
    {
        if( IsRealtime == false ) return true;
        if( Unit.Body.isWorking == false ) return false;
        if( Unit.TileID == ETileType.FROG ) return true;                                                  // Frog move is processed in the method

        if( Unit.UnitType == EUnitType.MONSTER || IsDynamicObject() )
        {
            if( Unit.TileID != ETileType.TRAIL )
            if( Map.I.GF( Unit.Pos, ETileType.TRAIL ) != null )
                return false;                                                                             // Monster over Trail

            float tottime = GetRealtimeSpeedTime();                                                       // Realtime Monsters Movement
            
            WaitTimeCounter -= Time.deltaTime;                                                            // Decrease wait timer counter
            if( WaitTimeCounter > 0 )
            if( Unit.ValidMonster )
            {
                if( iTween.Count(Unit.Graphic.gameObject ) == 0 )
                if( Unit.Graphic.transform.localPosition == Vector3.zero )
                    iTween.ShakePosition( Unit.Graphic.gameObject, new Vector3( .15f, .15f, 0 ), 1f );    // entangled unit animation
                return false;
            }

            SpeedTimeCounter += Time.deltaTime;                                                           // Increase move timer counter

            if( Unit.TileID == ETileType.FISH ) return true;                                              // Fish

            bool step = Map.Stepping();
            int mask = ( int ) Item.GetNum( ItemType.Res_Mask );
            if( mask > 0 && Unit.TileID == ETileType.BOULDER )                                            // Boulder Step movement under mask
                step = true;

            if( SpeedTimeCounter >= tottime )
            {
                if( OxygenStop() ) return false;                                                          // Oxygen

                if( mask > 0 )
                    if( step == false )
                        return false;                                                                         // Mask
                RealtimeMoveTried = true;
            }

            if( Unit.TileID == ETileType.QUEROQUERO ) return true;                                        // Continuous move
            if( Unit.ProjType( EProjectileType.FIREBALL ) ) return true;
            if( Unit.ProjType( EProjectileType.MISSILE ) ) return true;
            if( Unit.TileID == ETileType.DRAGON1 ) return true;
            if( Unit.TileID == ETileType.SLIME ) return true;
            if( Unit.TileID == ETileType.WASP ) return true;
            if( Unit.TileID == ETileType.HERB ) return true;
            if( Unit.TileID == ETileType.BOUNCING_BALL ) return true;
            if( Unit.TileID == ETileType.IRON_BALL ) return true;
            if( Unit.TileID == ETileType.MINE ) return true;

            if( step )                                                                                   // Stepping Based Movement
            {
                SpeedTimeCounter = 0;
                if( Map.I.AdvanceTurn && BlockStepping == false )
                {
                    if( Unit.Control.SkipMovement-- > 0 ) return false;
                    RealtimeMoveTried = true;
                    return true;
                }
                return false;
            }

            if( CheckTickMove() )                                                                           // Tick based movement
            {
                return true;
            }

            if( tottime > 5 )                                                                               // time remaining text mesh
            {
                float rem = tottime - SpeedTimeCounter;

                if( Unit.LevelTxt )
                {
                    Unit.LevelTxt.gameObject.SetActive( true );
                    Unit.LevelTxt.text = Util.ToSTime( rem );
                    if( rem < .1f )
                        Unit.LevelTxt.gameObject.SetActive( false );
                }
            }

            if( SpeedTimeCounter >= tottime )
            {
                Map.I.MonstersMoved = true;
                return true;
            }
            else return false;
        }
        return false;
    }

    public bool CheckTickMove()
    {
        if( TickBasedMovement )
        {
            SpeedTimeCounter = 0;
            if( Controller.MoveTickDone )
            if( TickMoveList.Contains( G.HS.CurrentTickNumber ) )
                return true;
            return false;
        }
        return false;
    }
    public void ApplyMove( Vector2 from, Vector2 to, bool pushed = false )
    {
        if( Unit == null ) return;

        if( IsFlyingUnit )
        {
            MoveFlyingUnitTo( ref Unit, from, to );
            return;
        }

        if( Unit.TileID != ETileType.SCORPION )
            if( SkipMovement > 0 )                                                                           // Skip movement
            {
                SkipMovement--;
                return;
            }
        UpdatePreMove( to );
        Map m = Map.I;
        OldPos = from;
        LastPos = from;

        AnimationOrigin = from;
        Unit.transform.position = new Vector3( to.x, to.y, 0 );

        Unit item = Map.I.GetUnit( ETileType.ITEM, to );                                                      // Unit over Shadow: Force rotation
        if( item && item.Dir != EDirection.NONE )
        {
            Unit.RotateTo( item.Dir );
            MasterAudio.PlaySound3DAtVector3( "Sword Swing", G.Hero.Pos, .3f );
        }

        if( from != to ) UnitMoved = true;
        if( from.x != -1 )
        {
            Unit.Graphic.transform.position = new Vector3( from.x, from.y, 0 );                               // Smooth movement animation
        }

        Unit shackle = Map.GFU( ETileType.MINE, to );
        if( shackle && shackle.Body.MineType == EMineType.SHACKLE )
        if( ( Unit.UnitType == EUnitType.MONSTER && Unit.ValidMonster ) ||
              Unit.UnitType == EUnitType.HERO )
              Controller.ConnectShackle( shackle, Unit );                                                     // Connect shackle   

        if( Unit.UnitType == EUnitType.MONSTER )
        {
            if( from.x != -1 ) m.Unit[ ( int ) from.x, ( int ) from.y ] = null;                               // Update Unit[,] array
            m.Unit[ ( int ) to.x, ( int ) to.y ] = Unit;

            Unit.name = Unit.PrefabName + " " + to.x + " " + to.y;
            HeroPushedMonsterBarricadeDestroyCount = 0;
            Spell.UpdateHookDestruction( to );                                                                // hook destruction by monster
            Map.I.CreateExplosionFX( to, "Smoke Cloud", "" );                                                 // Smoke Cloud FX
        }

        if( Unit.UnitType == EUnitType.GAIA2 )                                                                // Update Gaia2[,] array
        {
            if( from.x != -1 ) m.Gaia2[ ( int ) from.x, ( int ) from.y ] = null;
            m.Gaia2[ ( int ) to.x, ( int ) to.y ] = Unit;
            Unit.name = Unit.PrefabName + " " + to.x + " " + to.y;
        }
        Unit.Pos = new Vector2( ( int ) to.x, ( int ) to.y );

        if( UI.I.LockedTile != new Vector2( -1, -1 ) )                                                    // Changes tile Selection
            if( UI.I.LockedTile == from )
            {
                UI.I.LockedTile = to;
            }

        if( Unit.UnitType == EUnitType.HERO )                                                             // Hero specific stuff
        {
            Unit.name = "Hero";
            Map.I.Hero.Pos = new Vector2( ( int ) to.x, ( int ) to.y );
            if( GS.IsLoading ) return;
            UpdateResourceCollecting( Unit.Pos, false );                    //changed recently to prevent wand not casting fireball
            Quest.I.UpdateArtifactStepping( to );
            Map.I.LockAiTarget = to;
            UI.I.UpdBeastText = true;
            Map.I.UpdateBoomerangs();
            Map.I.UpdateFireBall();
            if( pushed == false )
                UpdateSpiderStepping( from, to );
            if( UpdateSnowEntering() == false )
                UpdateSandEntering();
            Map.I.UpdateFrontalTarget();
            UpdateTrapStepping();                                                                         // Hero Trap stepping
            UpdateBrokenRaft();
            if( Map.I.GetMud( to ) )
                Map.I.CreateExplosionFX( to, "Smoke Cloud", "" );                                         // Smoke Cloud FX over mud
        }

        UpdateArrowRush();

        Spell.UpdateWeaponInstalation();                                                                  // Hero shield and axe

        if( RaftMoving == false )
        if( Unit.UnitType == EUnitType.MONSTER )                                                          // Resource Collecting
            UpdateResourceCollecting( Unit.Pos, true );

        UpdateSleep();

        TurnTime = SmoothMovementFactor = 0;
        UnitProcessed = true;
        UpdatePostMove();                                                                                 // Post move
    }

    public void UpdatePostMove()
    {
        if( Unit.Body.RopeConnectSon != null && Unit.Body.Rope != null )
            Unit.UpdateChainSizes( Unit.Body.RopeConnectSon.transform.position );

        if( Unit.Body.RopeConnectFather != null )                                                                                 // chain update
            for( int f = 0; f < Unit.Body.RopeConnectFather.Count; f++ )
                Unit.Body.RopeConnectFather[ f ].UpdateChainSizes( Unit.Pos );

        if( ControlType == EControlType.ROACH )
        {
            Unit arrow = Map.I.GetUnit( ETileType.ARROW, Unit.Pos );
            if( ForcedFrontalMovementFactor > 0 && arrow )                                                                         // el torero over arrow
                ForcedFrontalMovementDir = arrow.Dir;
        }

        EggsDestroyed = false;

        if( IsDynamicObject() )
            Map.I.CreateExplosionFX( Unit.Pos, "Smoke Cloud", "" );                                                                  // Smoke Cloud FX

        if( ControlType == EControlType.SLIME )
        {
            Unit arrow = Map.I.GetUnit( ETileType.ARROW, Unit.Pos );
            if( ForcedFrontalMovementFactor > 0 && arrow )
                ForcedFrontalMovementDir = arrow.Dir;

            if( Map.I.LastMaskMoveDir != EDirection.NONE )
                if( JumpTarget != new Vector2( -1, -1 ) )
                    JumpTarget += Manager.I.U.DirCord[ ( int ) Map.I.LastMaskMoveDir ];
        }

        if( Unit.UnitType == EUnitType.HERO )                                            // Waypoint jump Lock
        {
            if( Map.I.GetUnit( ETileType.FREETRAP, Unit.Pos ) ||
                Map.I.GetUnit( ETileType.TRAP, Unit.Pos ) )
                Map.I.RM.LockWayPointJump = true;

            Mine.UpdateStickyMine();                                                     // Updates Sticky Mine

            if( Unit.GetFront() != Controller.HeroWallGrabTg )
                Controller.HeroWallGrabTg = new Vector2( -1, -1 );
            if( Map.IsWall( Unit.GetFront() ) )
            if( Map.I.GetUnit( ETileType.LAVA, Unit.Pos ) )                              // Wall grab check for lava raft rotational move
            {
                EDirection mov = Util.GetTargetUnitDir( OldPos, G.Hero.Pos );
                Controller.HeroWallGrabTg = Unit.GetFront();                             // grab wall               
            }
        }

        UpdateHugger();

        OldFloor = Floor;

        if( Controller.RaftMoving == false )
            Floor = GetUnitFloor( Unit.Control.OldPos, Unit.Pos, Unit );                // Updates unit current floor  

        if( Unit.UnitType == EUnitType.HERO )
        if( OldFloor != Floor )                                                         // displays floor message
        {
            string msg = "";
            if( Floor == 0 ) msg = "F1";
            if( Floor == 2 ) msg = "F2";
            if( Floor == 4 ) msg = "F3";
            if( msg != "" )
                Message.CreateMessage( ETileType.NONE, ItemType.NONE, msg,
                    G.Hero.Pos, Color.red, false, false, 2.5f, 0, -1, 70 );
        }

        if( Unit.UnitType == EUnitType.MONSTER )                                         // update trail rotator
            Map.I.UpdateTrailRotator( Unit );

        Brain.UpdateBrainMove();                                                         // Update brain move                                             
    }

    public static int GetUnitFloor( Vector2 from, Vector2 to, Unit un )
    {
        if( Manager.I.GameType != EGameType.CUBES ) return 0;
        if( un.UnitType == EUnitType.MONSTER )
            if( un.ValidMonster == false ) return 0; // new
        Unit tob = Map.GMine( EMineType.BRIDGE, to );
        Unit frb = Map.GMine( EMineType.BRIDGE, from );
        Unit frm = Map.GFU( ETileType.MINE, from );
        Unit tom = Map.GFU( ETileType.MINE, to );
        EDirection mov = Util.GetTargetUnitDir( from, to );

        int floor = -1;
        if( tom == null )
            floor = 0;                                                                               // Calculates unit floor
        else
        {
            if( tom.Body.MineType == EMineType.SHACKLE ) floor = 0;                                  // new to avoid shackle blocking hero bug
            else
                if( tom.Body.MineType == EMineType.LADDER ) floor = 1;
                else
                    if( tom.Body.UPMineType == EMineType.LADDER ) floor = 3;
                    else
                    {
                        if( tob || ( tom && tom.Body.UPMineType == EMineType.BRIDGE ) )                      // from ladder to bridge: which floor?
                        {
                            if( un.Control.StairStepDir != -1 && un.Control.StairStepDir <= 1 ||
                                un.Control.Floor == 2 )                                                      // this line is new, added to avoid hero move from gate to bridge block bug
                            {
                                if( tom && tom.Body.UPMineType == EMineType.BRIDGE &&
                                  ( un.Control.OldFloor == 4 || frb ) )                                      // from empty position to bridge
                                {
                                    floor = 4;
                                    if( frm && frm.Mine.FrontSupport == 1 && mov == frm.Dir )                // front support
                                        floor = 2;
                                    if( frm && frm.Mine.BackSupport == 1 && mov == frm.InvDr() )             // back support
                                        floor = 2;
                                }
                                else
                                {
                                    floor = 2;
                                    if( un.Control.StairStepDir == 0 )                                       // from upstair to upbridge
                                        if( frm && frm.Body.UPMineType == EMineType.LADDER )
                                            if( tom && tom.Body.UPMineType == EMineType.BRIDGE )
                                                floor = 4;
                                }
                            }
                            else
                            {
                                if( frm )
                                {
                                    if( un.Control.StairStepDir != -1 && un.Control.StairStepDir >= 3 )     // unit goes down the stair
                                    {
                                        if( tom && tob == null )
                                            floor = 2;                                                      // down to a mine 
                                        else
                                            floor = 0;                                                      // down to the ground level
                                    }
                                    else
                                        if( frb && tob )
                                            floor = un.Control.Floor;                                           // bridge crossing
                                        else
                                        {
                                            floor = 2;                                                          // unit enters bridge lower floor
                                            if( un.Control.OldFloor == 4 ||
                                                frm.Body.UPMineType == EMineType.BRIDGE )
                                                floor = 4;                                                      // unit enters bridge top floor
                                            if( tob ) floor = 2;                                                // ground bridge is always floor 2
                                        }
                                }
                                else
                                {
                                    if( ( tom && tom.Body.UPMineType == EMineType.BRIDGE ) )                // from empty position to bridge
                                        floor = 4;
                                    else
                                        floor = 0;                                                          // ground to ground
                                }
                            }
                        }
                        else
                        {
                            if( tom.Body.UPMineType != EMineType.NONE )
                                floor = 4;                                                                  // unit over upmine
                            else
                                floor = 2;                                                                  // unit over normal mine
                        }
                    }
        }
        Unit tog = Map.I.GetUnit( to, ELayerType.GAIA );                                       // Gate
        if( tog && Sector.GetPosSectorType( to ) == Sector.ESectorType.NORMAL )
        {
            if( tog.TileID == ETileType.CLOSEDDOOR ) floor = 2;
            if( tog.TileID == ETileType.ROOMDOOR ) floor = 2;
            if( tog.TileID == ETileType.FOREST ) floor = 2;
        }
        return floor;
    }

    public void UpdatePreMove( Vector2 to )
    {
        if( Unit.TileID == ETileType.PLAGUE_MONSTER )                                          // Plague Indicators
        {
            if( Map.I.Farm.NextPosition == Unit.Pos ) Map.I.Farm.NextPosition = to;
            if( Map.I.Farm.GrabPosition == Unit.Pos ) Map.I.Farm.GrabPosition = to;
        }
    }

    public void UpdateHugger()
    {
        string snd = "";
        if( Unit.UnitType == EUnitType.HERO )
        {
            List<Unit> oldl = new List<Unit>();
            int rad = 1;
            for( int y = ( int ) OldPos.y - rad; y <= OldPos.y + rad; y++ )                     // FROM position neighbors
                for( int x = ( int ) OldPos.x - rad; x <= OldPos.x + rad; x++ )
                {
                    Unit old = Map.I.GetUnit( ETileType.HUGGER, new Vector2( x, y ) );
                    if( old ) oldl.Add( old );
                }

            List<Unit> newl = new List<Unit>();
            for( int y = ( int ) Unit.Pos.y - rad; y <= Unit.Pos.y + rad; y++ )                // DEST position nighbors
                for( int x = ( int ) Unit.Pos.x - rad; x <= Unit.Pos.x + rad; x++ )
                {
                    Unit nu = Map.I.GetUnit( ETileType.HUGGER, new Vector2( x, y ) );
                    if( nu ) newl.Add( nu );
                }

            for( int i = 0; i < newl.Count; i++ )
            {
                if( oldl.Contains( newl[ i ] ) == false )
                {
                    if( newl[ i ].Control.HuggerStepList == null ||                          // Enter hugger area from outside
                        newl[ i ].Control.HuggerStepList.Count < 1 )
                    {
                        newl[ i ].Control.HuggerStepList = new List<Vector2>();              // init array
                        newl[ i ].Control.HuggerStepList.Add( OldPos );
                        snd = "Roach Glue";
                    }
                }
                else
                {
                    if( newl[ i ].Control.HuggerStepList != null )
                    {
                        if( newl[ i ].Control.HuggerStepList.Count == 1 )
                        {
                            newl[ i ].Control.HuggerStepList.Add( OldPos );                           // second step around
                            snd = "Roach Glue";
                        }
                        else
                            if( newl[ i ].Control.HuggerStepList.Count > 1 )
                            {
                                if( oldl[ i ] == null )
                                    Debug.LogError( oldl + "  " + oldl[ i ] + "!  " + i + "  " + oldl.Count ); // bug correction
                                if( Map.I.CheckArrowBlockFromTo( oldl[ i ].Pos,
                                G.Hero.Pos, G.Hero ) ) return;                                            // Arrow block att
                                if( G.Hero.Body.CheckHeroShield( oldl[ i ] ) ) return;
                                Map.I.StartCubeDeath();                                                   // Death for 3rd step
                                oldl[ i ].Body.Sprite3.transform.localScale = new Vector3( 1, 1.6f, 1 );
                                newl[ i ].Control.HuggerStepList = null;
                            }
                    }
                }
            }

            for( int i = 0; i < oldl.Count; i++ )
            {
                if( newl.Contains( oldl[ i ] ) == false )
                {
                    if( oldl[ i ].Control.HuggerStepList != null )                                    // Exit hugger area
                        if( oldl[ i ].Control.HuggerStepList.Count < 2 )
                        {
                            if( Map.I.CheckArrowBlockFromTo( oldl[ i ].Pos,
                                G.Hero.Pos, G.Hero ) ) return;
                            if( G.Hero.Body.CheckHeroShield( oldl[ i ] ) ) return;
                            oldl[ i ].Body.Sprite3.transform.localScale = new Vector3( 1, 1.6f, 1 );
                            Map.I.StartCubeDeath();                                                       // Death for not enough steps
                            oldl[ i ].Control.HuggerStepList = null;
                            return;
                        }

                    if( oldl[ i ].Control.HuggerStepList != null )
                        if( oldl[ i ].Control.HuggerStepList[ 0 ] != new Vector2( -1, -1 ) )              // Hero step poison tile
                        {
                            if( G.Hero.Pos == oldl[ i ].Control.HuggerStepList[ 0 ] )
                            {
                                Map.I.StartCubeDeath();
                                oldl[ i ].Body.Sprite3.transform.localScale = new Vector3( 1, 1.6f, 1 );
                                oldl[ i ].Control.HuggerStepList = null;
                                return;
                            }
                        }

                    if( oldl[ i ].Md.ResetHuggerMoveAfterStepOut )
                        oldl[ i ].Control.SpeedTimeCounter = 0;                                           // Hero Step out
                    else
                        oldl[ i ].Control.SpeedTimeCounter = oldl[ i ].Control.GetRealtimeSpeedTime();

                    oldl[ i ].Body.Lives--;                                                               // Life Lost
                    Body.CreateDeathFXAt( oldl[ i ].Pos, "Monster Hit" );
                    G.Hero.RangedAttack.CreateArrowAnimation( null, oldl[ i ].Pos, EBoltType.Rock );
                    oldl[ i ].Control.HuggerStepList = null;

                    if( oldl[ i ].Body.Lives < 1 )                                                        // Unit Dies
                    {
                        oldl[ i ].Kill();
                    }
                }
            }
        }

        if( Unit.TileID == ETileType.HUGGER )                                                        // Hugger moves towards hero vicinity
            if( Util.IsNeighbor( Unit.Pos, G.Hero.Pos ) )
            {
                Unit.Control.HuggerStepList = new List<Vector2>();
                Unit.Control.HuggerStepList.Add( G.Hero.Control.OldPos );
                snd = "Roach Glue";
            }

        if( snd != "" )
            MasterAudio.PlaySound3DAtVector3( snd, G.Hero.Pos );
    }

    public bool IsDynamicObject()
    {
        bool dynamic = false;
        if( DynamicObjectMoveList != null && DynamicObjectMoveList.Count > 0 ||
            DynamicObjectDirectionList != null && DynamicObjectDirectionList.Count > 0 ||
            DynamicObjectOrientationList != null && DynamicObjectOrientationList.Count > 0 ||
            DynamicObjectJumpList != null && DynamicObjectJumpList.Count > 0 ) dynamic = true;
        return dynamic;
    }

    //______________________________________________________________________________________________________________________ Update Resource Collecting

    public void UpdateResourceCollecting( Vector2 tg, bool onlyMonster )
    {
        if( Quest.CurrentLevel != -1 ) return;
        if( Map.PtOnMap( Map.I.Tilemap, tg ) == false ) return;
        if( Unit.TileID == ETileType.BOULDER ) return;                                                               // Boulder do not destroy resource
        if( Unit.TileID == ETileType.RAFT ) return;

        if( Unit.UnitType == EUnitType.HERO )
        {
            if( LastResPosCollectedByHeroPos == tg ) return;
            if( SnowSliding == false && SandSliding == false )
            if( IsBeingPushed == false )
            if( Map.I.FishingMode == EFishingPhase.NO_FISHING )
            {
                if( Map.I.AdvanceTurn == false ) return;
                if( Mine.TunnelTraveling == false )
                if( G.Hero.LastMoveWasStill() ) return;
            }
        }

        if( Map.I.Gaia2[ ( int ) tg.x, ( int ) tg.y ] )
        {
            string snd = "Error 2";
            Area ar = Area.Get( tg );
            bool destroy = true;
            Unit res = Map.I.Gaia2[ ( int ) tg.x, ( int ) tg.y ];

            if( res.Activated == false ) return;
            if( res.TileID == ETileType.ITEM )                                                                           // Item Collecting
            {
                if( res.Body.GetMiniDomeTotTime() > 0 )
                {
                    float val = res.Body.MiniDomeTimerCounter;
                    res.Control.StartMiniDomeRotation();
                    if( val > 0 || val == -1 ) return;
                }
                Map.I.CountRecordTime = true;
                if( res.Variation == ( int ) ItemType.Res_Raft_Hammer ) return;
                bool error = false;
                if( Unit.UnitType == EUnitType.MONSTER )                                                                // Monster item step
                {
                    destroy = false;
                    int amt = ( int ) res.Body.StackAmount;
                    ItemType type = ItemType.NONE;
                    if( res.Variation >= 0 )
                        type = G.GIT( res.Variation ).Type;
                    snd = "";

                    if( type == ItemType.Res_Mushroom )                                                                 // Monster Step on Mushroom
                    {
                        destroy = true;
                        snd = "Save Game";
                        ForcedFrontalMovementFactor = Map.I.RM.RMD.ForcedFrontalMovementDistance;
                        EDirection dr = Util.GetTargetUnitDir( OldPos, Unit.Pos );
                        ForcedFrontalMovementDir = dr;                                                                  // el torero direction is always relative from old position
                        JumpTarget = new Vector2( -1, -1 );
                    }

                    if( type == ItemType.Res_Mask )                                                                     // Specific cases where monsters collect prize
                    {
                        destroy = true;
                        snd = "Save Game";
                        GivePrize( G.GIT( res.Variation ), amt, res, ref destroy );
                    }

                    if( snd != "" ) MasterAudio.PlaySound3DAtVector3( snd, res.Pos );
                    if( destroy ) UpdateResourceDestruction( res );                                                   // Destroy item obj
                    return;
                }

                if( Unit.UnitType == EUnitType.HERO )                                                                 // Hero Item Stepping
                {
                    Item.IgnoreMessage = false;
                    if( res.Body.IsChest() && res.Dir == EDirection.NONE )                                            // Chest Stepping
                    {
                        Chests.ChestStep( Unit, ref snd, ref destroy, res );
                        return;
                    }

                    int amt = ( int ) res.Body.StackAmount;                                                           // Item stepping
                    Item it = null;
                    if( res.Variation >= 0 )
                        it = G.GIT( res.Variation );

                    if( it && it.Type == ItemType.Res_Boomerang )                                                     // No diagonal boomerang throw
                        if( Util.IsDiagonal( G.Hero.Dir ) ) return;

                    bool ok = GivePrize( it, amt, res, ref destroy );

                    if( ok )
                    {
                        if( it.CountAsResourcePickInStats )
                            Statistics.AddStats( Statistics.EStatsType.RESOURCECOLLECTED, 1f );                        // Add resource to stats
                        snd = "Click 2";
                        if( it.Type == ItemType.Res_Mushroom ) snd = "";
                        if( snd != "" )
                            MasterAudio.PlaySound3DAtVector3( snd, res.Pos );                                          // Play sound FX
                    }
                    else
                    {
                        if( res.Dir != EDirection.NONE )
                            snd = "Click 2";
                        error = true;
                    }
                }
                else error = true;
                if( error )
                    MasterAudio.PlaySound3DAtVector3( snd, res.Pos );                                                 // Error Sound FX

                if( destroy )
                    UpdateResourceDestruction( res );                                                                 // Destroy item obj              

                return;
            }
        }
    }

    public static void UpdateResourceDestruction( Unit res )
    {
        if( res.Body.ResourceWasteTotalTime > 0 )
        {
            if( res.Body.ResourceWasteTimeCounter == -1 )
                res.Body.ResourceWasteTimeCounter = res.Body.ResourceWasteTotalTime;
            return;
        }

        if( res.Body.ResourcePersistTotalSteps != -1 )                                           // Destroy resource obj
        {
            res.Body.ResourcePersistStepCount++;
            if( res.Body.ResourcePersistStepCount >= res.Body.ResourcePersistTotalSteps )        // Resource persist
            {
                res.Kill();
            }
            else
            {
                res.Control.StartMiniDomeRotation();                                             // Reactivate minidome 
            }
        }
    }
    public bool GivePrize( Item it, float amt, Unit res, ref bool destroy )
    {
        ItemType type = ItemType.NONE;
        if( it ) type = it.Type; else return false;
        bool add = true;
        float old = Item.GetNum( type );
        if( G.HS.BagExtraBonusItemID != -1 )
            if( type == ( ItemType ) G.HS.BagExtraBonusItemID )                                    // Mining Bag Vault Bonus
                amt += Item.GetNum( ItemType.Res_Mining_Bag ) * G.HS.BagExtraBonusAmount;

        bool ok = false;
        if( res.Body.ResourceWasteTotalTime > 0 )
            if( res.Body.ResourceWasteTimeCounter != -1 ) return false;                            // Resource waste counting

        if( res.Body.ResourceWasteTotalTime > 0 )                                              // temp resource add
            Item.TempResource = true;

        if( type == ItemType.Res_Fire_Staff )
        {
            if( UpdateFireBallCreation( true, G.Hero.Control.OldPos,                           // space for fireball creation?
                G.Hero.Pos ) == false ) { destroy = false; return false; }
            Map.I.FireBallCount++;
        }

        if( type == ItemType.Res_Bamboo )                                                      // Bamboo needs free space: handled in the Spell.cs file
        {
            destroy = false;
            add = false;
        }

        if( add && res.Body.ResourceOperation == EResourceOperation.ADD )                     // Add Item amt
            ok = Item.AddItem( type, amt );

        Item.TempResource = false;

        if( type == ItemType.Res_HP )
        {
            Map.I.Hero.Body.AddHP( amt, true );                                                // Heal HP
        }

        if( res.Body.ResourceOperation == EResourceOperation.SET )                             // Set Item amt
            ok = Item.SetAmt( type, amt );

        if( type == ItemType.Res_Mushroom )                                                    // Mushroom damage
        {
            G.Hero.Body.ReceiveDamage( amt, EDamageType.BLEEDING, G.Hero, null );
            G.Hero.Body.CreateDamageAnimation( Map.I.Hero.Pos, amt,
            G.Hero, EDamageType.NONE, true );
        }

        if( type == ItemType.Res_Mask )                                                       // Activate Mask Movement
            if( old <= 0 && Item.GetNum( type ) > 0 )
                MaskMoveInitiated = true;

        if( type == ItemType.Res_BirdCorn )                                                  // Corn        
            Controller.CornPickedAmt = amt;

        if( type == ItemType.Res_Rudder )                                                    // Rudder        
            Map.I.RaftDirectionChangeAvailable = true;

        Spell.AddSpell( type, res );                                                         // Adds a spell

        if( type == ItemType.Res_Boomerang )                                                 // Boomerang        
            Map.I.CreateBoomerang();

        if( type == ItemType.Res_TRIGGER )                                                   // Trigger        
            UpdateResourceTrigger( it, res );

        if( type == ItemType.Res_Vine_Bale )                                                 // Vine bale de-select current option        
            Map.I.CurrentVineSelection = -1;

        LastResPosCollectedByHeroPos = res.Pos;
        Map.I.RM.LockWayPointJump = true;

        CreateMagicEffect( res.Pos );                                                        // Create Magic effect

        Mine.UpdateAfterResourcePickup( it.Type, amt );                                      // Update Vault Bonuses after picking up resources 

        return ok;
    }
    public static void CreateMagicEffect( Vector2 tg, string file = "Magic Explosion", int times = 1 )
    {
        for( int i = 0; i < times; i++ )
        {
            Transform tr = PoolManager.Pools[ "Pool" ].Spawn( file );                           // FX
            tr.position = new Vector3( tg.x, tg.y, -6 );
            ParticleSystem pr = tr.gameObject.GetComponent<ParticleSystem>();
            pr.Stop();
            pr.Play();
        }
    }

    public void UpdateResourceTrigger( Item it, Unit un )
    {
        Sector s = Map.I.RM.HeroSector;
        switch( un.Body.ResTriggerType )
        {
            case EResTriggerType.SWITCH_MOVE_MODE:
            s.SteppingMode = !s.SteppingMode;
            Map.I.SetMovementMode( s.SteppingMode );
            break;
        }
    }

    public void StartMiniDomeRotation()
    {
        //if( G.Hero.LastMoveWasStill() ) return;   novo: atente pra bugs
        Unit.Body.ItemMiniDome.gameObject.SetActive( true );
        Unit.Body.ItemMiniDome.color = new Color( 1, 1, 1, 1 );
        Unit.Body.MiniDomeTimerCounter = Unit.Body.GetMiniDomeTotTime();                                                 // Start Minidome Timer
        MasterAudio.PlaySound3DAtVector3( "Save Game", Unit.Pos );
        Unit.Body.ItemMiniDome.transform.eulerAngles = new Vector3( 0, 0, 0 );
        StitchesPunishment = false;
    }

    public EActionType GetUserAction()
    {
        if( Map.I.RepeatToLast ) return ForceMove;

        float dlim = .18f;
        if( Input.GetAxis( "Joy1 Axis 1" ) > dlim )
            if( Input.GetAxis( "Joy1 Axis 2" ) < -dlim ) return EActionType.MOVE_NE;

        if( Input.GetAxis( "Joy1 Axis 1" ) > dlim )
            if( Input.GetAxis( "Joy1 Axis 2" ) > dlim ) return EActionType.MOVE_SE;

        if( Input.GetAxis( "Joy1 Axis 1" ) < -dlim )
            if( Input.GetAxis( "Joy1 Axis 2" ) < -dlim ) return EActionType.MOVE_NW;

        if( Input.GetAxis( "Joy1 Axis 1" ) < -dlim )
            if( Input.GetAxis( "Joy1 Axis 2" ) > dlim ) return EActionType.MOVE_SW;

        dlim = .3f;
        if( Input.GetAxis( "Joy1 Axis 4" ) > dlim )
            if( Input.GetAxis( "Joy1 Axis 5" ) < -dlim ) return EActionType.MOVE_NE;

        if( Input.GetAxis( "Joy1 Axis 4" ) > dlim )
            if( Input.GetAxis( "Joy1 Axis 5" ) > dlim ) return EActionType.MOVE_SE;

        if( Input.GetAxis( "Joy1 Axis 4" ) < -dlim )
            if( Input.GetAxis( "Joy1 Axis 5" ) < -dlim ) return EActionType.MOVE_NW;

        if( Input.GetAxis( "Joy1 Axis 4" ) < -dlim )
            if( Input.GetAxis( "Joy1 Axis 5" ) > dlim ) return EActionType.MOVE_SW;

        float lim = .4f;
        if( Input.GetAxis( "Joy1 Axis 1" ) < -lim ) return EActionType.MOVE_W;
        if( Input.GetAxis( "Joy1 Axis 1" ) > lim ) return EActionType.MOVE_E;

        if( Input.GetAxis( "Joy1 Axis 2" ) < -lim ) return EActionType.MOVE_N;
        if( Input.GetAxis( "Joy1 Axis 2" ) > lim ) return EActionType.MOVE_S;

        if( Input.GetButton( "Rotate CW" ) ) return EActionType.ROTATE_CW;
        if( Input.GetButton( "Rotate CCW" ) ) return EActionType.ROTATE_CCW;

        if( cInput.GetKey( "Rotate CW" ) ) return EActionType.ROTATE_CW;
        if( cInput.GetKey( "Rotate CCW" ) ) return EActionType.ROTATE_CCW;
        if( cInput.GetKey( "Wait" ) ) return EActionType.WAIT;
        if( cInput.GetKey( "Move N" ) ) return EActionType.MOVE_N;
        if( cInput.GetKey( "Move NE" ) ) return EActionType.MOVE_NE;
        if( cInput.GetKey( "Move E" ) ) return EActionType.MOVE_E;
        if( cInput.GetKey( "Move SE" ) ) return EActionType.MOVE_SE;
        if( cInput.GetKey( "Move S" ) ) return EActionType.MOVE_S;
        if( cInput.GetKey( "Move SW" ) ) return EActionType.MOVE_SW;
        if( cInput.GetKey( "Move W" ) ) return EActionType.MOVE_W;
        if( cInput.GetKey( "Move NW" ) ) return EActionType.MOVE_NW;
        if( LastAction != EActionType.NONE )
            if( cInput.GetKey( "Battle" ) )
                if( Manager.I.GameType != EGameType.FARM )
                    return Map.I.InvertActionList[ ( int ) LastAction ];

        float scr = Input.GetAxis( "Mouse ScrollWheel" );
        if( UI.I.MouseOverUI == false )
        {
            if( scr < 0 ) return EActionType.ROTATE_CW;
            if( scr > 0 ) return EActionType.ROTATE_CCW;
        }

        return EActionType.NONE;
    }


    //______________________________________________________________________________________________________________________ Human Movement

    public bool UpdateHumanMovement()
    {
        if( this == null ) return false;

        InputVector = Map.I.GetInputVector( true );                                   // For snow  
        InputVector = Map.I.GetInputVector( false );                                  // For sand

        if( Map.I.FreeCamMode ) return false;
        if( Map.I.InvalidateInputTimer > 0 ) return false;
        if( Map.I.HeroDeathTimer != -1 ) return false;
        if( Map.I.TunnelPhase != -1 ) return false;

        Map.I.ValidateMove = false;
        ForceDiagonalMovement = false;
        Vector3 pos = Unit.Pos;
        Vector2 from = new Vector2( pos.x, pos.y );

        EActionType ac = EActionType.NONE;
        EActionType vcrac = EActionType.NONE;

        int rot = 0;                                                                                            // Battle Key
        if( ForceMove == EActionType.BATTLE || cInput.GetKey( "Battle" ) )
            if( Map.I.Hero.Body.ToolBoxLevel < 3 )
                if( Manager.I.GameType == EGameType.NORMAL )
                {
                    Map.I.ShowMessage( Language.Get( "ERROR_TOOLBOX3" ) );
                    ForceMove = EActionType.NONE;
                    return false;
                }

        ac = GetUserAction();

        if( Map.I.UpdateOxygen( ac ) ) return false;                                                           // oxygen pause check

        if( ac != EActionType.NONE ) Map.I.RestartOnRepeat = true;

        if( ac != EActionType.NONE )
            Map.I.RM.FinishAutoGoto();

        if( PathFinding != null && PathFinding.Path != null && PathFinding.Path.Count > 0 )
        {
            Map.I.TimeKeyPressing = 10;
            if( Map.I.CurrentArea != -1 )                                                                      // Moves step by step while inside dirty area
                if( !Quest.I.CurLevel.AreaList[ Map.I.CurrentArea ].Cleared )
                    if( PathFinding.CurrentStep > 1 )
                    {
                        Map.I.RM.FinishAutoGoto();
                        return false;
                    }

            if( PathFinding.CurrentStep >= PathFinding.Path.Count - 1 )
            {
                Map.I.RM.FinishAutoGoto();
                return false;
            }
            else
            {
                if( PathFinding.CurrentStep == 1 )
                {
                    bool res = Map.I.TrimArrowPath();
                    if( res ) return false;
                }

                if( PathFinding.CurrentStep >= 1 )
                {
                    Vector2 dif = new Vector2( PathFinding.Path[ PathFinding.CurrentStep - 1 ].x, PathFinding.Path[ PathFinding.CurrentStep - 1 ].y ) -
                                  new Vector2( PathFinding.Path[ PathFinding.CurrentStep ].x, PathFinding.Path[ PathFinding.CurrentStep ].y );

                    if( dif == new Vector2( 0, 1 ) ) ac = EActionType.MOVE_S;
                    if( dif == new Vector2( 1, 1 ) ) ac = EActionType.MOVE_SW;
                    if( dif == new Vector2( 1, 0 ) ) ac = EActionType.MOVE_W;
                    if( dif == new Vector2( 1, -1 ) ) ac = EActionType.MOVE_NW;
                    if( dif == new Vector2( 0, -1 ) ) ac = EActionType.MOVE_N;
                    if( dif == new Vector2( -1, -1 ) ) ac = EActionType.MOVE_NE;
                    if( dif == new Vector2( -1, 0 ) ) ac = EActionType.MOVE_E;
                    if( dif == new Vector2( -1, 1 ) ) ac = EActionType.MOVE_SE;
                }
            }
        }

        if( ForceMove != EActionType.NONE )
        {
            ac = ForceMove;
            if( ac == EActionType.BATTLE && LastAction != EActionType.NONE )
                if( Manager.I.GameType != EGameType.FARM )
                    ac = Map.I.InvertActionList[ ( int ) LastAction ];

            ForceMove = EActionType.NONE;
        }

        bool rs = GetActionMoveEffect( ac, ref rot, ref pos, vcrac, true );
        if( rs ) return false;

        if( ac == EActionType.NONE ) return false;
        Map.I.CurrentMoveTrial = ac;
        LastPos = Unit.Pos;

        float spd = SettingsWindow.I.MaxHeroSpeed;
        bool stone = false;
        float delay = SettingsWindow.I.KeyHoldDelay;

        Unit stoned = Map.I.GetUnit( ETileType.STONEPATH, G.Hero.Pos );
        if( stoned == null )
        if( rot == 0 )
            stoned = Map.I.GetUnit( ETileType.STONEPATH, G.Hero.Control.OldPos );

        if( rot != 0 )
        if( Spell.AnyMonsterHookStuck() ) return false;                                                          // Rotation locked: Monsters stuck by hook

        if( Manager.I.GameType == EGameType.CUBES )
        if( G.HS.MaxTicTacMoves > 0 )                                                                           // Max Tic Tac moves vault power move restriction
        if( G.HS.TicTacMoveCount > G.HS.MaxTicTacMoves )
        if( Sector.AwakenNormalMonsters > 0 )
        {
            return false;
        }

        if( Manager.I.GameType == EGameType.CUBES )
        if( stoned || HeroMovedFromSlidingTerrainToMud )                                                        // Mud Hero Movement Delay
        if( CubeData.I.StonepathRotationMaxSpeed > 0 ||
            CubeData.I.StonepathMovementMaxSpeed > 0 )
        {
            stone = true;
            spd = 1 / ( CubeData.I.StonepathMovementMaxSpeed / Map.I.Tick );

            if( rot != 0 )
                spd = 1 / ( CubeData.I.StonepathRotationMaxSpeed / Map.I.Tick );
        }

        if( rot != 0 )
        {
            float lim = 1 / ( Map.I.RM.RMD.MaxHeroRotationSpeed / Map.I.Tick );
            if( spd < lim )
                spd = lim;
        }

        if( Map.I.RM.RMD.BarricadeDestroyHeroWaitTime > 0 )
        if( Map.I.LevelTurnCount > 1 )
            {
                if( BarricadeBumpTimeCount < Map.I.RM.RMD.BarricadeDestroyHeroWaitTime ) return false;
            }

        if( Map.I.AutoOpenGateStep > 3 )
        if( Map.I.RepeatToLast == false )
        if( TurnTime < spd )
            return false;

        Map.I.TimeKeyPressing += Time.deltaTime;
        if( stone )
            delay = spd;

        if( FirstMoveDone && stone == false )
        if( Map.I.RepeatToLast == false )
        if( Map.I.TimeKeyPressing < delay )
            return false;

        if( Map.I.FireBallCount > 0 )                                                         // Any fireball still flying? wait until it finishes
        for( int i = 0; i < G.HS.Fly.Count; i++ )
        if( G.HS.Fly[ i ].ProjType( EProjectileType.FIREBALL ) )
        if( G.HS.Fly[ i ].Control.FlyingTarget !=
            new Vector2( -1, -1 ) ) return false;

        FirstMoveDone = true;
        HeroMovedFromSlidingTerrainToMud = false;
        if( Map.I.CurrentArea == -1 )                                                         // Wait move and special
        {
            if( ac == EActionType.WAIT ||
                ac == EActionType.SPECIAL )
            {
                Map.I.AdvanceTurn = true;
                Attack.AttackedByHeroMelee = false;
                TurnTime = 0;
                ForceMove = EActionType.NONE;
                AnimationOrigin = Unit.transform.position;
                LastAction = ac;
                MasterAudio.PlaySound3DAtVector3( "Foot Step", transform.position );
                Map.I.UpdateBoomerangs();
                UpdateFanMovement( from, pos );                                               // Update Fan Movement
                return true;
            }
        }

        Vector2 to = new Vector2( pos.x, pos.y );
        Map.I.CurrentMoveTrialType = GetMoveType( ac, from, to, true, true );

        int area = Map.I.GetPosArea( to );                                                    // Change area zoom level if hero tries to exit area
        if( Map.I.AreaZoomLevel == 0 && Map.I.CurrentArea != -1 && area == -1 )
        {
            Map.I.AreaZoomLevel = 1;
            return false;
        }

        if( rot != 0 )
        {
            if( Unit.GetFront() != Controller.HeroWallGrabTg )
                Controller.HeroWallGrabTg = new Vector2( -1, -1 );
            if( Unit.CheckBambooBlock( true, from, to, rot ) ) return false;
            if( Map.I.FishingMode != EFishingPhase.NO_FISHING ) return false;
            Unit snow = Map.I.GetUnit( ETileType.SNOW, Unit.Pos );
            if( snow && Item.GetNum( ItemType.Res_SnowStep ) < 1 ) return false;
            if( Item.GetNum( ItemType.Res_Cramps ) > 0 ) return false;
            
            if( Spell.SpellSlot != 2 )
            if( Spell.AddSpellOrigin != new Vector2( -1, -1 ) )
                return false;
            Unit item = Map.I.GetUnit( ETileType.ITEM, Unit.Pos );                             // unit over item shadow: no rotation   
            if( item && item.Dir != EDirection.NONE )
                return false;
            EDirection old = Unit.Dir;
            Unit.Rotate( rot );                                                                // Unit Rotate
            Mine.UpdateMudExpansionVault( old );                                               // mud expansion vault power
            Map.I.UpdateBoomerangs();
            MasterAudio.PlaySound3DAtVector3( "Sword Swing", G.Hero.Pos, .3f );
            SmoothMovementFactor = 1;
            Attack.IsMoveShot = true;
            Map.I.BumpTarget = new Vector2( -1, -1 );
            TurnTime = 0;
            AnimationOrigin = Unit.Pos;
            LastAction = ac;
            Attack.AttackedByHeroMelee = false;
            ConsecutiveStillMove++;
            UpdateTrapStepping();

            if( snow )
            {
                Item.AddItem( ItemType.Res_SnowStep, -1 );
                Unit orb = Map.I.GetUnit( ETileType.ORB, Unit.GetFront() );                   // Hero Strike frontal orb over snow
                if( orb ) orb.StrikeTheOrb( Unit );
            }
            else
            {
                Map.I.AdvanceTurn = true;
            }
        }

        if( SnowSliding ) return false;
        if( SandSliding ) return false;

        if( Map.I.FishingMode != EFishingPhase.NO_FISHING ) return false;                      // Fishing: No hero move

        if( G.Farm.CheckRoadCarryCapacityBlock( from, to ) ) return false;

        bool glued = false;                                                                    // Gluey: Restricts Hero movement
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = Unit.Pos + Manager.I.U.DirCord[ i ];
            if( Map.I.GetUnit( ETileType.GLUEY, tg ) ) { glued = true; break; }
        }

        bool ok = false;
        if( glued )
            for( int i = 0; i < 8; i++ )
            {
                Vector2 tg = to + Manager.I.U.DirCord[ i ];
                if( Map.I.GetUnit( ETileType.GLUEY, tg ) )
                {
                    ok = true; break;
                }
            }

        if( Map.I.CurrentArea != -1 )
            if( glued && ok == false ) return false;

        for( int i = 0; i < 8; i++ )                                                          // No monster neighbor tile move
        {
            Vector2 tg = Unit.Pos + Manager.I.U.DirCord[ i ];
            if( Map.I.GetUnit( ETileType.ROACH, tg ) )
            {
                if( Util.IsNeighbor( to, tg ) )
                    if( Map.I.CheckArrowBlockFromTo( tg, to, null, true, 3, 3 ) == false )
                        if( Util.IsNeighbor( from, tg ) )
                            if( Map.I.CheckArrowBlockFromTo( tg, from, null, true, 3, 3 ) == false )
                            {
                                return false;
                            }
            }
        }

        CurentMoveTrial = to;                                      // This var represents where the hero wants to move

        Map.I.AdvanceTurn = true;                                  // Advance turn bool set

        Attack.AttackedByHeroMelee = false;
        UI.I.ArtifactLevelDifference = 0;

        if( Map.I.CurrentArea != -1 )                              // Records Move to the VCR
            if( vcrac == EActionType.NONE )
                if( ac != EActionType.ADVANCE_VCR )
                {
                    TurnData td = new TurnData();
                    td.Action = ac;
                    td.HeroPos = Unit.Pos;
                    td.HeroDir = Unit.Dir;
                    td.HeroDie = false;
                    td.ActionTime = TurnTime;
                    td.SaveCount = 1;
                    Map.I.TDCol.TurnData.Add( td );
                }


        UI.I.ActiveCompareHeroID = ( int ) EHeroID.NONE;
        UI.I.UpdatePortrait( ( EHeroID ) Map.I.Hero.Variation );

        Map.I.TurnsKeyPressing++;
        Map.I.BumpTarget = new Vector2( -1, -1 );
        AttemptMining = true;

        if( UpdateFanMovement( from, to ) ) return false;                                                         // Update Fan Movement
        if( Controller.UpdateRaftOverPitMovement( from, to ) ) return false;                                      // Raft Over Pit Movement     
        if( Controller.UpdateRaftOverLavaMovement( from, to, rot ) ) return false;                                // Raft Over Lava Movement     

        UpdateMudObjectPush( true, from, to );                                                                    // Mud Object push

        if( Mine.UpdateStickyMineBump( from, to ) ) return false;                                                 // Updates Sticky Mine Bump
        if( Mine.UpdateBoulderDirectionChange( from, to ) ) return false;                                         // Updates hero change boulder direction

        bool mine = Controller.UpdateMining( G.Hero.Pos, to );                                                    // Mining
        if( mine == true ) return false;

        Altar.UpdateAltarBump( to );                                                                               // Altar Bump

        for( int i = 0; i < 8; i++ )                                                                              // Roach Glue Animation
        {
            Vector2 tg = to + Manager.I.U.DirCord[ i ];
            Unit un = Map.I.GetUnit( ETileType.ROACH, tg );
            if( un )
            {
                un.Control.RoachGlueSprite.gameObject.SetActive( true );
                MasterAudio.PlaySound3DAtVector3( "Roach Glue", transform.position );
            }
        }

        if( Mine.UpdateTunnelTravel( from, to ) ) return false;                                                   // Updates Tunnel Travel
        if( GS.ChargeCubeSavingResource() == false ) return false;                                                // Charge resource if cube has been loaded from checkpoint
        Map.I.UpdateFishingPoleStepping( from, to );                                                              // Fishing Pole stepping
        Map.I.UpdateHerbBump( from, to );                                                                         // Herb stepping
        if( Map.I.UpdateKickingBoots( from, to ) ) return false;                                                  // Update Kicking Boots

        UpdateSuspendedBridgeConstruction( from, to );                                                            // Suspended Bridge Creation

        Map.I.Farm.UpdateFeatherPicking( from, to );                                                              // Farm Feather Picking
        if( Map.I.Farm.UpdateRaftHammer( from, to ) ) return false;                                               // Raft Hammer Update
        if( Map.I.Farm.UpdateFlockingMonsterPushing( from, to ) ) return false;                                   // Farm Monster Grouped Push
        if( Map.I.Farm.UpdateMonsterChoosing( from, to ) ) return false;                                          // Farm Monster Choosing
        if( Map.I.Farm.UpdateMonsterGrabbingSwitch( from, to ) ) return false;                                    // Farm Monster Grab switch 
        if( Map.I.Farm.UpdateMonsterGrabbing( from, to ) ) return false;                                          // Farm Monster Grabbing
        if( Map.I.Farm.UpdateMonsterSpawning( from, to ) ) return false;                                          // Farm Monster Moving
        if( Map.I.Farm.UpdateMonsterBlocking( from, to ) ) return false;                                          // Farm Monster Blocking
        if( Map.I.Farm.UpdateMonsterKilling( from, to ) ) return false;                                           // Farm Monster Killing
        if( Map.I.Farm.UpdateMonsterKicking( from, to ) ) return false;                                           // Farm Monster Kicking
        if( Map.I.Farm.UpdateMonsterSwapping( from, to ) ) return false;                                          // Farm Monster Swapping
        Map.I.Farm.UpdateHoneyComb( from, to );                                                                   // Honeycomb step
        if( Spell.UpdateHookJump( from, to ) == true ) return false;                                              // Checks for Hook jump

        if( rot == 0 )
        if( Unit.CanMoveFromTo( false, from, to, Unit ) == false )                                                // Check Movement
            {
                Unit ga2 = Map.I.GetUnit( to, ELayerType.GAIA2 );
                if( ga2 && ga2.TileID == ETileType.ITEM && ga2.Body.ItemMiniDome.gameObject.activeSelf )             // Item has a Mini dome timer active
                {
                    if( ga2.Body.MiniDomeTimerCounter == -1 )
                        ga2.Control.StartMiniDomeRotation();                                                         // start mini dome rotation
                }

                Map.I.BumpTarget = to;
                LastAction = ac;                              //added recently check for bugs
                UpdateFrontalTargetManeuver( from, to );                                                              // Frontal target maneuver
                UpdateBumpDamage( to );                                                                               // Damage cause by bumping objects
                Vector3 pun = Util.GetTargetUnitVector( from, to );                                                   // obstacle bump fx
                iTween.PunchPosition( Unit.Graphic.gameObject, pun * 0.3f, .6f );
                if( PlayBumpSound )
                    if( mine == false )
                        MasterAudio.PlaySound3DAtVector3( "Bump 2", G.Hero.Pos, 0.2f );
                ga2 = Map.I.GetUnit( to, ELayerType.GAIA2 );
                if( ga2 ) iTween.PunchPosition( ga2.Graphic.gameObject, pun * -0.3f, .6f );
                Unit ga22 = Map.I.GetUnit( from, ELayerType.GAIA2 );
                if( ga22 ) iTween.PunchPosition( ga22.Graphic.gameObject, pun * 0.3f, .6f );
                Unit mn = Map.I.GetUnit( to, ELayerType.MONSTER );
                if( mn ) iTween.PunchPosition( mn.Graphic.gameObject, pun * -0.3f, .6f );

                OnlyRudderPush = true;
                Controller.ImpulsionateRaft( from, to );                                                             // Impulsionate Raft with rudder

                if( Map.I.ValidateMove == false || Map.I.NumRealtimeMonsters > 0 )
                {
                    //Map.I.AdvanceTurn = false;
                    return false;
                }

                Building.UpdateMoving( from, to );
                LastMoveType = GetMoveType( ac, from, to, false, false );
                if( Unit.Control.PathFinding.Path.Count > 0 ) Unit.Control.PathFinding.Path.Clear();
                return true;
            }

        Map.I.AutoOpenGateStep++;
        LastAction = ac;
        LastMoveAction = ac;

        if( PathFinding.Path.Count > 0 )
        {
            PathFinding.CurrentStep++;
        }

        Unit.Body.UpdateSecondaryAttack( 5 );

        if( rot == 0 )
        {
            ConsecutiveStillMove = 0;
            Unit.CanMoveFromTo( true, from, to, Unit );                           // Apply Move
        }

        Map.I.UpdateCameraAreaStepping( to );                                     // Camera area stepping

        Map.I.UpdateRoachBackSideStep( to );

        UpdateMeleeTargetLock( from, to, ac );

        Unit mudto = Map.I.GetMud( G.Hero.Pos );
        Unit mudfr = Map.I.GetMud( G.Hero.Control.OldPos );
        float vol = 1;
        string stepsnd = "Foot Step";

        if( Floor == 0 )
            if( mudto && rot == 0 )                                                  // mud step
            {
                stepsnd = "Slime Step";
                vol = 0.15f;
                if( mudfr )
                    Util.PlayParticleFX( "Mud Splat", Unit.Control.OldPos );
                if( mudto )
                    Util.PlayParticleFX( "Mud Splat", Unit.Pos );
            }
        MasterAudio.PlaySound3DAtVector3( stepsnd, Unit.Pos, vol );              // hero step sound

        LastMoveType = GetMoveType( ac, from, to, true, false );

        Map.I.SeekArtifact = null;

        MoveMade = true;

        OnlyRudderPush = false;
        Controller.ImpulsionateRaft( from, to );                                 // Impulsionate Raft

        Controller.ImpulsionateFog( from, to );                                  // Impulsionate Fog

        if( ac != EActionType.ADVANCE_VCR )
        if( ac != EActionType.WAIT )
        if( ac != EActionType.BATTLE )
        if( ac != EActionType.SPECIAL )
            Attack.IsMoveShot = true;
        return true;
    }

    public void UpdateBumpDamage( Vector2 to )
    {
        float stitches = Item.GetNum( ItemType.Res_Stitches );
        if( StitchesPunishment )
            if( stitches > 0 )                                                                                    // Stitches hp punishment
            {
                Unit.Body.ReceiveDamage( stitches, EDamageType.BLEEDING, Map.I.Hero, null );
                Unit.Body.CreateDamageAnimation( Unit.Pos, stitches, Unit, EDamageType.NONE, true );
            }

        Unit mn = Map.I.GetUnit( ETileType.BOULDER, to );                                                      // Hero bump against spiked boulder
        if( mn && mn.Body.SpikedBoulder )
        {
            Map.I.StartCubeDeath();
        }
    }

    public void UpdateFrontalTargetManeuver( Vector2 from, Vector2 to )
    {
        EDirection dr = Util.GetTargetUnitDir( from, to );
        if( dr == G.Hero.Dir )
            FrontalTargetManeuverDist++;
        else
            if( Util.GetInvDir( dr ) == G.Hero.Dir )
                FrontalTargetManeuverDist--;
        FrontalTargetManeuverDist = Mathf.Clamp( FrontalTargetManeuverDist,
        Map.I.RM.RMD.MinFrontalTargetManeuverDist, Map.I.RM.RMD.MaxFrontalTargetManeuverDist );

    }

    public bool GetActionMoveEffect( EActionType ac, ref int rot, ref Vector3 pos, EActionType vcrac, bool hero = false )
    {
        float uz = -1;
        if( ac == EActionType.ROTATE_CW ) rot = 1;
        else
        if( ac == EActionType.ROTATE_CCW ) rot = -1;
        else
        if( ac == EActionType.WAIT ) { }
        else
        if( ac == EActionType.MOVE_N ) pos = new Vector3( pos.x + 0, pos.y + 1, uz );
        else
        if( ac == EActionType.MOVE_S ) pos = new Vector3( pos.x + 0, pos.y - 1, uz );
        else
        if( ac == EActionType.MOVE_E ) pos = new Vector3( pos.x + 1, pos.y + 0, uz );
        else
        if( ac == EActionType.MOVE_W ) pos = new Vector3( pos.x - 1, pos.y + 0, uz );
        else
        if( ac == EActionType.MOVE_NE ) pos = new Vector3( pos.x + 1, pos.y + 1, uz );
        else
        if( ac == EActionType.MOVE_SE ) pos = new Vector3( pos.x + 1, pos.y - 1, uz );
        else
        if( ac == EActionType.MOVE_SW ) pos = new Vector3( pos.x - 1, pos.y - 1, uz );
        else
        if( ac == EActionType.MOVE_NW ) pos = new Vector3( pos.x - 1, pos.y + 1, uz );
        else
            if( vcrac == EActionType.NONE && hero )
            {
                Map.I.TimeKeyPressing = 0;
                Map.I.TurnsKeyPressing = 0;
                FirstMoveDone = false;
                return true;
            }
        return false;
    }

    //______________________________________________________________________________________________________________________ Ram Movement

    public void UpdateRamMovement()
    {
        Vector2 pos = new Vector2( ( int ) Unit.Pos.x, ( int ) Unit.Pos.y );
        Vector2 tg = pos + Manager.I.U.DirCord[ ( int ) Unit.Dir ];

        EDirection dr = EDirection.NONE;

        if( pos.x == Map.I.Hero.Pos.x || pos.y == Map.I.Hero.Pos.y )
            RamStraightMoveCount = 0;

        if( RamStraightMoveCount == 0 )
        {
            float dx = Mathf.Abs( Map.I.Hero.Pos.x - pos.x );
            float dy = Mathf.Abs( Map.I.Hero.Pos.y - pos.y );

            Unit.UpdateDirection();
            if( pos.x > Map.I.Hero.Pos.x && pos.y > Map.I.Hero.Pos.y )
                if( dx > dy ) dr = EDirection.W;
                else
                    if( dx < dy ) dr = EDirection.S; else dr = EDirection.SW;

            if( pos.x > Map.I.Hero.Pos.x && pos.y < Map.I.Hero.Pos.y )
                if( dx > dy ) dr = EDirection.W;
                else
                    if( dx < dy ) dr = EDirection.N; else dr = EDirection.NW;

            if( pos.x < Map.I.Hero.Pos.x && pos.y < Map.I.Hero.Pos.y )
                if( dx > dy ) dr = EDirection.E;
                else
                    if( dx < dy ) dr = EDirection.N; else dr = EDirection.NE;

            if( pos.x < Map.I.Hero.Pos.x && pos.y > Map.I.Hero.Pos.y )
                if( dx > dy ) dr = EDirection.E;
                else
                    if( dx < dy ) dr = EDirection.S; else dr = EDirection.SE;

            Unit.RotateTo( dr );
        }

        if( ++RamStraightMoveCount >= 5 ) RamStraightMoveCount = 0;

        if( Unit.CanMoveFromTo( false, pos, tg, Unit ) == true )
        {
            Unit.CanMoveFromTo( true, pos, tg, Unit );
        }
        else RamStraightMoveCount = 0;
    }

    //______________________________________________________________________________________________________________________ Rook Movement
    public void UpdateRookMovement()
    {
        Vector2 pos = new Vector2( ( int ) Unit.Pos.x, ( int ) Unit.Pos.y );

        Vector2 bestpos = new Vector2( -1, -1 );
        float bestscore = Map.I.DistFromTarget[ ( int ) pos.x, ( int ) pos.y ];

        int[ , ] dr = { { 6, 2, 0, 4 },             // Horizontal, vertical,  ar = 0
			          { 0, 4, 6, 2 } };           // Vertical, Horizontal,  ar = 1

        int ar = 1;
        if( Map.I.Hero.Pos.y == pos.y ) ar = 0;
        float dx = Mathf.Abs( Map.I.Hero.Pos.x - pos.x );
        float dy = Mathf.Abs( Map.I.Hero.Pos.y - pos.y );
        if( dx > dy ) ar = 0;
        if( dx == 1 && dy == 1 ) return;                      // Monster adjacent to hero doesnt move

        for( int i = 0; i < 4; i++ )
        {
            Vector2 tg = pos + Manager.I.U.DirCord[ dr[ ar, i ] ];
            if( Unit.CanMoveFromTo( false, pos, tg, Unit ) == true )
            {
                if( bestscore > Map.I.DistFromTarget[ ( int ) tg.x, ( int ) tg.y ] )
                {
                    bestpos = tg;
                    bestscore = Map.I.DistFromTarget[ ( int ) tg.x, ( int ) tg.y ];
                }
            }
        }

        Unit.UpdateDirection();
        if( bestpos.x == -1 ) return;

        Unit.CanMoveFromTo( true, pos, bestpos, Unit );
        Unit.UpdateDirection();
    }

    //______________________________________________________________________________________________________________________ Boulder Movement

    public void UpdateBoulderMovement()
    {
        Vector2 pos = new Vector2( ( int ) Unit.Pos.x, ( int ) Unit.Pos.y );
        Vector2 tg = pos + Manager.I.U.DirCord[ ( int ) Unit.Dir ];
        BoulderDirChanged = false;
        CheckBoulderAndPitBlockade();
        tg = pos + Manager.I.U.DirCord[ ( int ) Unit.Dir ];

        bool res = Unit.CanMoveFromTo( false, pos, tg, Unit );

        if( res == true )                                                                                   // move ball if possible
        {
            Unit.CanMoveFromTo( true, pos, tg, Unit );
            MasterAudio.PlaySound3DAtVector3( "Boulder Rolling", transform.position );
            MoveMade = true;
            CheckBoulderAndPitBlockade();
        }

        CkeckBoulderFall();

        tg = Unit.Pos + Manager.I.U.DirCord[ ( int ) Unit.Dir ];
        Unit orb = Map.I.GetUnit( ETileType.ORB, tg );
        if( orb )                                                                                            // Boulder hit the orb and rebound
        {
            orb.StrikeTheOrb( Unit );
            Unit.SetVariation( Manager.I.U.InvDir[ ( int ) Unit.Dir ] );
            Vector3 cord = Util.GetTargetUnitVector( Unit.Control.OldPos, Unit.Pos );
            Vector3 amt = new Vector3( cord.x, cord.y, 0 ) * .8f;
            iTween.PunchPosition( orb.Graphic.gameObject, amt, 1 );                                          // FX
            MasterAudio.PlaySound3DAtVector3( "Orb Strike", orb.Pos );
        }
        EDirection dr = Map.I.GetArrowDir( Unit.Pos );                                                       // Arrow: update ball direction

        if( res )
            SetBoulderDir( dr );                                                                             // Check if its possible
    }
    public bool SetBoulderDir( EDirection dr )
    {
        if( dr == EDirection.NONE ) return false;
        for( int i = -2; i <= 2; i++ )                                                                      // Boulder cannot rotate backwards
        {
            EDirection rel = Unit.GetRelativeDirection( i );
            if( dr == rel )
            {
                Unit.SetVariation( ( int ) dr );                                                            // Rotate Boulder
                return true;
            }
        }
        return false;
    }
    public bool UpdateDynamicObjectMovement( bool apply, ref Vector2 dest, ref int destRot, ref EDirection destRotTo )
    {
        if( IsDynamicObject() == false ) return false;
        if( DynamicMaxSteps > 0 )
            if( DynamicStepCount >= DynamicMaxSteps ) return false;

        if( apply )
        {
            if( Unit.TileID == ETileType.RAFT ) return false;
            if( Unit.TileID == ETileType.FOG ) return false;
        }

        bool incrementMovementCounters = true;
        bool incrementRotationCounters = true;

        Vector2 pos = new Vector2( ( int ) Unit.Pos.x, ( int ) Unit.Pos.y );
        Vector3 tg3 = Vector3.zero;
        EActionType ac = EActionType.NONE;
        int maxmov = 0;
        int maxor = 0;
        bool obstacleBump = false;
        int movestep = DynamicMovementCurrentStep;
        int orstep = DynamicOrientationCurrentStep;

        bool orturn = true;
        if( RandomDirTurnList != null && RandomDirTurnList.Count > 0 )
            if( RandomDirTurnList.Contains( movestep ) == false ) orturn = false;

        bool moveturn = true;
        if( RandomMoveTurnList != null && RandomMoveTurnList.Count > 0 )
            if( RandomMoveTurnList.Contains( orstep ) == false ) moveturn = false;

        if( DynamicObjectMoveList != null )                                                                // Movement by user action
            if( DynamicObjectMoveList.Count > 0 )
            {
                maxmov = DynamicObjectMoveList.Count;
                if( MovementSorting == EDynamicObjectSortingType.RANDOM )
                    movestep = Random.Range( 0, maxmov );
                ac = DynamicObjectMoveList[ movestep ];
                if( MovementSorting != EDynamicObjectSortingType.SEQUENCE_BASED_ON_ORIGINAL_DIR )
                    ac = ( EActionType ) Util.FlipDir( ( EDirection ) ac,
                         Map.I.RM.HeroSector.FlipX, Map.I.RM.HeroSector.FlipY );

                if( MovementSorting == EDynamicObjectSortingType.SEQUENCE_CHANGE_ON_OBSTACLE )            // NONE cancels movement if obstacle based movement
                    if( ac == EActionType.NONE ) return false;
            }

        int rot = 0;
        bool rs = GetActionMoveEffect( ac, ref rot, ref tg3, EActionType.NONE );
        Vector2 tg = tg3;
        tg += pos;

        EDirection basedir = Unit.Dir;
        if( MovementSorting == EDynamicObjectSortingType.SEQUENCE_BASED_ON_ORIGINAL_DIR )                   // Movement based on original dir
        {
            basedir = Util.FlipDir( InitialDirection,
            Map.I.RM.HeroSector.FlipX, Map.I.RM.HeroSector.FlipY );
            if( ac >= EActionType.MOVE_N && ac <= EActionType.MOVE_NW )
                tg = pos + Util.GetRelativePosition( basedir, Util.GetActionDirection( ac ) );
        }

        Vector2 jump = Vector2.zero;                                                                        // Unit Jumps
        if( DynamicObjectJumpList != null )
            if( DynamicObjectJumpList.Count > 0 )
            {
                maxmov = DynamicObjectJumpList.Count;
                if( MovementSorting == EDynamicObjectSortingType.RANDOM )
                    movestep = Random.Range( 0, maxmov );
                jump = DynamicObjectJumpList[ movestep ];
                jump = Util.FlipVector( jump, Map.I.RM.HeroSector.FlipX, Map.I.RM.HeroSector.FlipY );
            }

        if( jump != Vector2.zero ) tg = pos + jump;

        if( rot != 0 )
        {
            if( apply )
                Unit.Rotate( rot );                                                                             // Unit Rotate
            destRot = rot;
        }
        else
        {
            if( moveturn )
            {
                if( apply )
                {
                    bool res = false;
                    if( pos != tg )
                    {
                        res = Unit.CanMoveFromTo( true, pos, tg, Unit );
                    }

                    if( res == false )
                        if( MovementSorting == EDynamicObjectSortingType.SEQUENCE_CHANGE_ON_OBSTACLE )
                        {
                            incrementMovementCounters = incrementRotationCounters = false;
                            SetMovementDynamicCounters( +1 );
                            obstacleBump = true;
                            Map.I.CreateExplosionFX( tg );                                                   // Bump Explosion FX
                        }
                    if( res || WaitAfterObstacleCollision ) MoveMade = true;
                }
                dest = tg;
            }
        }

        if( DynamicObjectOrientationList != null && DynamicObjectOrientationList.Count > 0 )               // rotation by orientation
        {
            maxor = DynamicObjectOrientationList.Count;
            if( OrientationSorting == EDynamicObjectSortingType.RANDOM )
                orstep = Random.Range( 0, maxor );
            rot = ( int ) DynamicObjectOrientationList[ orstep ];
            rot = ( int ) Util.FlipDir( ( EDirection ) rot,
                          Map.I.RM.HeroSector.FlipX, Map.I.RM.HeroSector.FlipY );
            if( orturn )
                if( rot != 0 )
                {
                    if( apply )
                        Unit.Rotate( rot );
                    destRot = rot;
                }
        }

        if( DynamicObjectDirectionList != null && DynamicObjectDirectionList.Count > 0 )                   // rotation by Direction
        {
            maxor = DynamicObjectDirectionList.Count;
            bool randrot = false;
            if( OrientationSorting == EDynamicObjectSortingType.RANDOM )
                randrot = true;

            if( OrientationSorting == EDynamicObjectSortingType.SEQUENCE_CHANGE_ON_OBSTACLE )
            {
                orturn = true;
                if( Unit.Md.RandomizeRotation )
                    randrot = true;
                if( obstacleBump == false ) orturn = false;
            }

            if( randrot )
                orstep = Random.Range( 0, maxor );

            rot = ( int ) DynamicObjectDirectionList[ orstep ];

            if( orturn )
                if( rot >= 0 )
                {
                    if( apply )
                        Unit.RotateTo( ( EDirection ) rot );
                    destRotTo = ( EDirection ) rot;
                }
        }

        if( apply )
            if( MovementSorting == EDynamicObjectSortingType.SEQUENCE_CHANGE_ON_OBSTACLE )
            {
                incrementMovementCounters = incrementRotationCounters = false;
            }

        if( incrementMovementCounters )
        {
            SetMovementDynamicCounters( +1 );
            DynamicStepCount++;
        }

        if( incrementRotationCounters )
        {
            SetRotationDynamicCounters( +1 );
            //DynamicStepCount++;
        }
        return true;
    }

    public void SetMovementDynamicCounters( int val )
    {
        DynamicMovementCurrentStep += val;
        if( DynamicObjectMoveList != null && DynamicObjectMoveList.Count > 0 )
        {
            if( DynamicMovementCurrentStep == DynamicObjectMoveList.Count )                                       // DynamicMovementCurrentStep increment and limiting
                DynamicMovementCurrentStep = 0;
            if( DynamicMovementCurrentStep < 0 )
                DynamicMovementCurrentStep = DynamicObjectMoveList.Count - 1;
        }

        if( DynamicObjectJumpList != null && DynamicObjectJumpList.Count > 0 )
        {
            if( DynamicMovementCurrentStep == DynamicObjectJumpList.Count )                                      // DynamicObjectJumpList increment and limiting
                DynamicMovementCurrentStep = 0;
            if( DynamicMovementCurrentStep < 0 )
                DynamicMovementCurrentStep = DynamicObjectJumpList.Count - 1;
        }
    }

    public void SetRotationDynamicCounters( int val )
    {
        DynamicOrientationCurrentStep += val;

        if( DynamicObjectOrientationList != null && DynamicObjectOrientationList.Count > 0 )
        {
            if( DynamicOrientationCurrentStep == DynamicObjectOrientationList.Count )                           // DynamicObjectOrientationList increment and limiting
                DynamicOrientationCurrentStep = 0;
            if( DynamicOrientationCurrentStep < 0 )
                DynamicOrientationCurrentStep = DynamicObjectOrientationList.Count - 1;
        }

        if( DynamicObjectDirectionList != null && DynamicObjectDirectionList.Count > 0 )
        {
            if( DynamicOrientationCurrentStep == DynamicObjectDirectionList.Count )                             // DynamicObjectDirectionList increment and limiting
                DynamicOrientationCurrentStep = 0;
            if( DynamicOrientationCurrentStep < 0 )
                DynamicOrientationCurrentStep = DynamicObjectDirectionList.Count - 1;
        }
    }

    public void CkeckBoulderFall()
    {
        CheckPitSquare( Unit.Pos + new Vector2( 0, 1 ) );                                                                 // Check for Pit Holes for boulder to fall
        CheckPitSquare( Unit.Pos + new Vector2( 0, 0 ) );
        CheckPitSquare( Unit.Pos + new Vector2( -1, 0 ) );
        CheckPitSquare( Unit.Pos + new Vector2( -1, 1 ) );
    }

    //______________________________________________________________________________________________________________________ Roach Movement

    // Roach Ideas:
    // Cool: Roach Glue recedes after few seconds. this will avoid hero getting stuck. should expand again after 1 step
    // Cool: Roach move timer reset after roach is attacked for the first time in a tile. this will give time for the hero to maneuver around her
    // Roach int HP idea
    public void UpdateRoachMovement()
    {
        if( Sleeping )
        {
            if( UpdateSleep() == false ) return;
        }

        if( Item.GetNum( ItemType.Res_Mask ) > 0 || MaskMoveFinalized ) return;                                        // Mask

        Vector2 pos = new Vector2( ( int ) Unit.Pos.x, ( int ) Unit.Pos.y );
        Vector2 bestpos = new Vector2( -1, -1 );
        Vector2 bestbarricade = new Vector2( -1, -1 );
        float bestscore = Map.I.DistFromTarget[ ( int ) pos.x, ( int ) pos.y ];
        float dx = Mathf.Abs( Map.I.Hero.Pos.x - pos.x );
        float dy = Mathf.Abs( Map.I.Hero.Pos.y - pos.y );

        Unit.UpdateDirection();

        if( ForcedFrontalMovementFactor <= 0 )
            if( dx == 1 && dy == 1 ) return;                                                                           // Monster adjacent to hero doesnt move

        bestpos = GetBestStandardMove( G.Hero.Pos );

        Unit.UpdateDirection();

        if( bestpos.x == -1 )                                                                                          // Classic barricade destruction
        {
            if( Manager.I.GameType == EGameType.CUBES )
            if( Map.I.RM.RMD.ClassicBarricadeDestruction )
            if( Map.PtOnMap( Map.I.Tilemap, bestbarricade ) )
                Unit.CheckBarricadeBlock( true, pos, bestbarricade );
            return;
        }

        Vector2 torero = UpdateElTorero( true );                                                                       // el torero for mushroom step

        if( torero.x != -1 ) bestpos = torero;
        if( torero.x == -2 ) return;

        bool ok = Unit.CanMoveFromTo( false, pos, bestpos, Unit );                                                      // Move unit
        if( ok )
        {
            Unit.CanMoveFromTo( true, pos, bestpos, Unit );
            if( Unit.TileID == ETileType.FROG )
                Unit.MeleeAttack.SpeedTimeCounter = 999;                                                               // Frog swimming Attacks
            MoveMade = true;
            MergingID = -1;
        }

        Unit.UpdateDirection();                                                                                        // Update Direction

        if( Unit.TileID == ETileType.ROACH )                                                                           // Roach exclusive rotation system
        {
            EDirection dr = Util.GetRotationToTarget( pos, bestpos );
            if( bestpos != new Vector2( 0, 0 ) )
            {
                Unit.RotateTo( dr );
            }
            else
            {
                Unit.RotateTo( Util.GetRotationToTarget( Unit.Pos, G.Hero.Pos ) );
            }

            if( Unit.Body.SpawnBlocker )                                                                                // Spawn Roach blocker after attack
                if( pos != Unit.Pos )
                    if( Controller.GetBlocker( pos ) == null )
                    {
                        Map.I.SpawnFlyingUnit( pos, ELayerType.MONSTER, ETileType.BLOCKER, Unit );
                        Unit.Body.SpawnBlocker = false;
                    }
        }
    }

    // FROG Ideas:
    // Frog move delay buffer for stepping 1 turn frontfacing = 1 turn delay
    public void UpdateFrogMovement()
    {
        if( Sleeping )
        {
            if( UpdateSleep() == false ) return;
        }
        if( Item.GetNum( ItemType.Res_Mask ) > 0 ) return;                                                     // Mask

        if( SpeedTimeCounter <= 0 )
            SpeedTimeCounter = 0;

        Unit water = Map.I.GetUnit( ETileType.WATER, Unit.Pos );
        EDirection dr = Util.GetRotationToTarget( Unit.Pos, G.Hero.Pos );                                      // Frog Neighboring water jumps into it
        Vector2 pt = Unit.Pos + Manager.I.U.DirCord[ ( int ) dr ];
        Unit waterto = Map.I.GetUnit( ETileType.WATER, pt );

        if( water || waterto )
        {
            float tottime = GetRealtimeSpeedTime();                                                            // too early for frog movement
            if( SpeedTimeCounter >= tottime )
            {
                UpdateRoachMovement();
            }
            SpeedTimeCounter += Time.deltaTime;                                                                // Frog Move Speed Increment
            return;
        }

        bool step = Map.Stepping();
        if( Unit.TileID == ETileType.FROG )                                                                    // Frog
            if( ControlType == EControlType.FROG )
            { }
            else
            {
                bool res = UpdateHeroFrogBlock( false );                                                       // Hero Blocks Frog path over water
                if( res ) return;
            }

        List<Vector2> tgl = Map.I.KnightTG;
        Unit.UpdateDirection();
        float score = 0;
        float bestscore = +99999;
        Vector2 bestpos = new Vector2( -1, -1 );
        Unit.MeleeAttack.SpeedTimeCounter = 0;
        bool swordblock = false;

        bool bl = UpdateHeroFrogBlock( true );                                                                 // Hero Blocks Frog path
        if( bl ) return;

        for( int i = 0; i < 8; i++ )                                                                           // Calculated frog targets
        {
            Vector2 tg = Unit.Pos + tgl[ i ];
            if( tg == G.Hero.GetFront() )                                                                      // hero sword frontal block
            if( Map.I.IsHeroMeleeAvailable() )
            {
                swordblock = true;
            }
        }

        if( Unit.Pos == G.Hero.GetFront() || swordblock )                                                      // frontfacing frog restarts move timer
        {
            if( step == false )
            {
                float tm = Util.Percent( 100 +                                                                 // RT time increment
                Map.I.HeroAttackSpeedBonus, Time.deltaTime );
                SpeedTimeCounter -= tm;
                if( SpeedTimeCounter <= 0 )
                    SpeedTimeCounter = 0;
            }
            else
            {
                if( Map.I.AdvanceTurn )                                                                        // STEPPING delay increment
                {
                    if( ++SpeedTimeCounter > Unit.Md.MaxFrogTurnDelay )
                        SpeedTimeCounter = Unit.Md.MaxFrogTurnDelay;
                    return;
                }
            }
        }
        else
        {
            if( step == false )
            {
                SpeedTimeCounter += Time.deltaTime;                                                             // Frog Move Speed Increment
            }
            else
            {
                if( Map.I.AdvanceTurn )                                                                         // STEPPING delay check
                {
                    SpeedTimeCounter--;
                    if( SpeedTimeCounter >= 0 )
                    {
                        Unit.MeleeAttack.InvalidateAttackTimer = 1;
                        return;
                    }
                    if( SpeedTimeCounter < 0 )
                        SpeedTimeCounter = 0;
                }
            }
        }

        if( step == false )
        {
            float tottime = GetRealtimeSpeedTime();                                                              // too early for frog movement
            if( SpeedTimeCounter < tottime ) return;
        }
        else
        {
            SpeedTimeCounter = ( int ) SpeedTimeCounter;
            if( SpeedTimeCounter > 0 ) return;
            if( Map.I.AdvanceTurn == false ) return;
        }     

        for( int i = 0; i < 8; i++ )                                                                                 // Calculated frog targets
        {
            Vector2 tg = Unit.Pos + tgl[ i ];

            bool closer = false;
            if( Vector2.Distance( tg, G.Hero.Pos ) <
                Vector2.Distance( Unit.Pos, G.Hero.Pos ) )
                closer = true;

            if( Util.IsNeighbor( Unit.Pos, G.Hero.Pos ) ) 
                closer = true;
            
            if( closer )
            if( Unit.CanMoveFromTo( false, Unit.Pos, tg, Unit ) == true )
            //if( tg != G.Hero.GetFront() )
            {
                score = Util.Manhattan( G.Hero.Pos, tg );
                score += Vector2.Distance( G.Hero.GetFront(), tg ) / 10000;

                if( score < bestscore )
                {
                    bestpos = tg;
                    bestscore = score;
                }
            }
        }

        if( Map.I.IsPaused() ) return;                                                // Oxygen 
        if( Item.GetNum( ItemType.Res_Mask ) > 0 ) return;                            // Mask 

        if( swordblock == false )
        if( bestpos.x != -1 )                                                         // best target position found
        {
            bool mov = Unit.CanMoveFromTo( false, Unit.Pos, bestpos, Unit );
            if( mov )
            {
                Unit.CanMoveFromTo( true, Unit.Pos, bestpos, Unit );
                Unit.MeleeAttack.SpeedTimeCounter = 999;
                MoveMade = true;
                Unit.Body.Sprite2.transform.localPosition = new Vector3( 0, 0.42f, 0.24f );

                if( Unit.Pos == G.Hero.GetFront() )
                if( step )
                if( Map.I.AdvanceTurn )                                                                        // STEPPING delay increment
                {
                    if( ++SpeedTimeCounter > Unit.Md.MaxFrogTurnDelay )
                        SpeedTimeCounter = Unit.Md.MaxFrogTurnDelay;
                    Message.GreenMessage( "Block!" );
                }
            }
        }
        Unit.UpdateDirection();
    }
    public bool UpdateHeroFrogBlock( bool attack )
    {
        FrogBlockedByHero = false;
        bool step = Map.Stepping();
        List<Vector2> tgl = Map.I.KnightTG;
        for( int i = 0; i < 8; i++ )                                                                            // Hero block frog
        {
            Vector2 tg = Unit.Pos + tgl[ i ];
            if( G.Hero.Pos == tg )
            {
                float tm = Util.Percent( 100 +
                Map.I.HeroAttackSpeedBonus, Time.deltaTime );
                bool hit = false;

                if( step == false )                                                                              // REALTIME block
                {
                    SpeedTimeCounter -= tm;
                    if( FrogHeroBlockTimeCounter >= HeroBlockFrogTotalTime )
                        hit = true;
                }
                else
                {
                    if( Map.I.AdvanceTurn )                                                                      // STEPPING block
                    {
                        hit = true;
                        if( --SpeedTimeCounter <= 0 )
                            SpeedTimeCounter = 0;
                    }
                }

                FrogBlockedByHero = true;
                if( attack == false )                                                                           // Over water no att
                {
                    if( SpeedTimeCounter <= 0 )
                        SpeedTimeCounter = 0;
                    return true;
                }

                FrogHeroBlockTimeCounter += tm;                                                             // Hero Block Frog Time Increment
                if( SpeedTimeCounter <= 0 )
                    SpeedTimeCounter = 0;

                if( hit )                                                                                   // Hero block attack vs frog
                {
                    FrogHeroBlockTimeCounter -= HeroBlockFrogTotalTime;
                    Unit.Body.ReceiveDamage( 10, EDamageType.BLEEDING, G.Hero, G.Hero.RangedAttack );
                    iTween.ShakePosition( Unit.Graphic.gameObject, new Vector3( .09f, .09f, 0 ), .2f );
                    Attack.AttackOrigin = G.Hero.Pos;
                    G.Hero.RangedAttack.CreateArrowAnimation( null, Unit.Pos, EBoltType.Rock );
                    MasterAudio.PlaySound3DAtVector3( "Frog", transform.position );
                    Unit.Body.Sprite2.transform.localPosition = new Vector3( 0, 0.42f, 0.24f );
                }
                return true;
            }
        }

        if( Unit.Md.UnblockedFrogRestorationFactor != 0 )                                               // Unblocked Frog Restoration
        {
            FrogHeroBlockTimeCounter -= Unit.Md.UnblockedFrogRestorationFactor * Time.deltaTime;
            if( FrogHeroBlockTimeCounter >= HeroBlockFrogTotalTime )
                FrogHeroBlockTimeCounter = HeroBlockFrogTotalTime;
            if( FrogHeroBlockTimeCounter <= 0 )
            {
                FrogHeroBlockTimeCounter = HeroBlockFrogTotalTime;
                if( Unit.Md.UnblockedFrogSpawnsBaby )                                                   // New baby spawned
                    if( Unit.Body.Lives < Unit.Md.MaxLives ||
                        Unit.Md.MaxLives <= 0 )
                    {
                        Unit.Body.AddedLivesCount++;
                        if( Unit.Body.AddedLivesCount == 1 ) return false;
                        Unit.Body.Lives++;
                    }
            }
        }
        return false;
    }

    public void UpdateSlimeMovement()
    {
        if( Sleeping )
        {
            if( UpdateSleep() == false ) return;
        }
        if( Item.GetNum( ItemType.Res_Mask ) > 0 ) return;                                               // Mask
        FlightJumpPhase = 0;
        bool incone = false;
        Vector3 directionToTarget = Unit.transform.position - G.Hero.transform.position;                 // Calculates if hero is inside slime cone view
        float angle = 180 - Vector3.Angle( Unit.Spr.transform.up, directionToTarget );
        float maxangle = 45;
        if( angle < maxangle ) { incone = true; }
        Unit.Spr.color = Color.white;

        if( JumpTarget != new Vector2( -1, -1 ) )
        {
            FlightPhaseTimer += Time.deltaTime;
            Quaternion qn = Util.GetRotationToPoint( Unit.Spr.transform.position, JumpTarget );              // Quick rotate to face target
            Unit.Spr.transform.rotation = Quaternion.RotateTowards(
            Unit.Spr.transform.rotation, qn, Time.deltaTime * 100000 );
        }

        Unit.Dir = Util.GetAngleDirection( Unit.Spr.transform.rotation.eulerAngles.z );
        float tottime = GetRealtimeSpeedTime();

        if( Util.Neighbor( Unit.Pos, G.Hero.Pos ) )                                                        // Hero neighbor attracts slime towards him
        {
            if( Unit.Pos + Unit.GetRelativePosition( EDirection.N, 1 ) == G.Hero.Pos ||
                Unit.Pos + Unit.GetRelativePosition( EDirection.NE, 1 ) == G.Hero.Pos ||
                Unit.Pos + Unit.GetRelativePosition( EDirection.NW, 1 ) == G.Hero.Pos ||
                Unit.Pos + Unit.GetRelativePosition( EDirection.E, 1 ) == G.Hero.Pos ||
                Unit.Pos + Unit.GetRelativePosition( EDirection.W, 1 ) == G.Hero.Pos )
            {
                JumpTarget = G.Hero.Pos;
                if( ForcedFrontalMovementFactor <= 0 )
                    ForcedFrontalMovementFactor = Map.I.RM.RMD.ForcedFrontalMovementDistance;
                EDirection dr = Util.GetTargetUnitDir( Unit.Pos, G.Hero.Pos );
                ForcedFrontalMovementDir = dr;
            }
        }

        Vector2 torero = UpdateElTorero( false );                                                           // el torero
        if( torero.x == -2 ) return;

        if( incone )                                                                                        // Hero in front of slime
        {
            if( JumpTarget == new Vector2( -1, -1 ) )
            {
                UpdateSlimeTarget();
                Unit.Dir = Util.GetTargetUnitDir( Unit.Pos, JumpTarget );
            }
        }
        else                                                                                               // Hero behind slime 
        {
            float spd = Map.I.RM.RMD.SlimeRotationSpeed;
            Quaternion qn = Util.GetRotationToPoint( Unit.Spr.transform.position,
            G.Hero.transform.position );
            Unit.Spr.transform.rotation = Quaternion.RotateTowards(
                Unit.Spr.transform.rotation, qn, Time.deltaTime * spd );
        }

        if( JumpTarget != new Vector2( -1, -1 ) )
            if( FlightPhaseTimer > tottime )                                                                     // Slime moves
            {
                bool res = Unit.CanMoveFromTo( false, Unit.Pos, JumpTarget, Unit );

                if( res )                                                                                        // Move ok
                {
                    Unit.CanMoveFromTo( true, Unit.Pos, JumpTarget, Unit );
                    FlightPhaseTimer = 0;
                    MoveMade = true;
                    Unit arrow = Map.I.GetUnit( ETileType.ARROW, Unit.Pos );
                    if( ForcedFrontalMovementFactor > 0 && arrow )
                        Unit.RotateTo( arrow.Dir );
                }
                else                                                                                             // Blocked
                {
                    if( JumpTarget == G.Hero.Pos )                                                               // Force attack is hero is in front of it
                    {
                        Unit.MeleeAttack.ForceAttack = true;
                        FlightPhaseTimer = 0;
                    }
                    ForcedFrontalMovementFactor = 0;
                }
                JumpTarget = new Vector2( -1, -1 );
                FlightJumpPhase = 1;
                UnitMoved = true;
                return;
            }
    }

    public Vector2 UpdateElTorero( bool mushroom )
    {
        if( JumpTarget == new Vector2( -1, -1 ) || mushroom )                                                           // el torero maneuver                                        
        {
            if( ForcedFrontalMovementFactor > 0 )
            {
                bool move = Unit.CanMoveFromTo( false, Unit.Pos, Unit.GetFront(), Unit );
                Vector2 tg = Unit.GetFront();

                if( move == false && Util.IsDiagonal( Unit.Dir ) )                                                     // Slide monster if diagonal forced move
                {
                    EDirection dr1 = Util.GetRelativeDirection( -1, Unit.Dir );
                    Vector2 tg1 = Unit.Pos + Manager.I.U.DirCord[ ( int ) dr1 ];
                    bool move1 = Unit.CanMoveFromTo( false, Unit.Pos, tg1, Unit );

                    EDirection dr2 = Util.GetRelativeDirection( +1, Unit.Dir );
                    Vector2 tg2 = Unit.Pos + Manager.I.U.DirCord[ ( int ) dr2 ];
                    bool move2 = Unit.CanMoveFromTo( false, Unit.Pos, tg2, Unit );

                    if( move1 && move2 == false ) { move = true; tg = tg1; }
                    if( move2 && move1 == false ) { move = true; tg = tg2; }
                }

                if( move == true )
                {
                    MoveMade = true;
                    JumpTarget = tg;
                    if( ForcedFrontalMovementFactor > 0 )                                                               // el torero timer decrement
                        if( ForcedFrontalMovementFactor < 30 )
                        {
                            if( --ForcedFrontalMovementFactor <= 0 )
                                StopForcedMovement();
                        }
                    return JumpTarget;
                }
                else
                {
                    if( Unit.TileID != ETileType.SLIME )
                        MoveMade = true;
                    StopForcedMovement();
                    return new Vector2( -2, -2 );
                }
            }

            if( ForcedFrontalMovementFactor <= 0 )
                ForcedFrontalMovementDir = EDirection.NONE;
        }
        return new Vector2( -1, -1 );
    }
    public void UpdateSlimeTarget()
    {
        Vector2 pos = new Vector2( ( int ) Unit.Pos.x, ( int ) Unit.Pos.y );
        Vector2 bestpos = new Vector2( -1, -1 );
        Vector2 bestbarricade = new Vector2( -1, -1 );
        float bestscore = Map.I.DistFromTarget[ ( int ) pos.x, ( int ) pos.y ];
        float dx = Mathf.Abs( Map.I.Hero.Pos.x - pos.x );
        float dy = Mathf.Abs( Map.I.Hero.Pos.y - pos.y );
        bestpos = GetBestStandardMove( G.Hero.Pos );

        List<Vector2> tgl = new List<Vector2>();
        float hdist = Util.Manhattan( Unit.Pos, G.Hero.Pos );
        //float hdist = Vector2.Distance( Unit.Pos, G.Hero.Pos );
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = Unit.Pos + Manager.I.U.DirCord[ i ];                                   // Checks possible targets
            float dist = Util.Manhattan( tg, G.Hero.Pos );
            //float dist = Vector2.Distance( tg, G.Hero.Pos );
            if( dist < hdist || ( Util.Neighbor( pos, G.Hero.Pos ) && dist <= hdist ) )
                if( Unit.CanMoveFromTo( false, pos, tg, Unit ) == true )
                    tgl.Add( tg );
        }
        if( tgl.Count >= 1 )
        {
            int id = Random.Range( 0, tgl.Count );                                              // Sorts a target
            bestpos = tgl[ id ];
            JumpTarget = bestpos;
            FlightPhaseTimer = 0;
            return;
        }
    }

    //______________________________________________________________________________________________________________________ Update Monster move order
    public void UpdateMoveOrderId()
    {
        if( Map.I.RM.HeroSector )
            if( Map.I.RM.HeroSector.MoveOrder != null )
                if( MoveOrderID != -1 )
                {
                    if( Map.I.RM.HeroSector.MoveOrder.Contains( Unit ) )
                        Map.I.RM.HeroSector.MoveOrder.Remove( Unit );
                }
    }

    //______________________________________________________________________________________________________________________ Update Obj Suspended

    public Vector3 UpdateObjSuspended()
    {
        if( Unit == null ) return Vector3.zero;
        if( Unit.Pos.x == -1 ) return Vector3.zero;
        if( Map.PtOnMap( Map.I.Tilemap, Unit.Pos ) == false )
            return Vector3.zero;
        Vector3 pos = new Vector3( 0, 0, 0 );
        IsSuspended = false;
        if( Map.IsClimbable( Unit.Pos ) )
        {
            if( Floor == 0 && Map.GMine( EMineType.BRIDGE, Unit.Pos ) )                           // Under Bridge z position calculation
                pos -= new Vector3( 0f, 0f, -0.1f );
            else
            {
                pos -= new Vector3( 0.2f, -0.2f, 0 );
                IsSuspended = true;
            }
        }
        return pos;
    }

    //______________________________________________________________________________________________________________________ Get movement type
    public EMoveType GetMoveType( EActionType ua, Vector2 from, Vector2 to, bool moveok, bool trial )
    {
        if( ua == EActionType.ROTATE_CW || ua == EActionType.ROTATE_CCW ) return EMoveType.ROTATE;

        if( from == to || moveok == false ) return EMoveType.STILL;

        if( trial == false )
            if( OldPos == Map.I.Hero.Pos ) return EMoveType.STILL;

        if( ua == EActionType.MOVE_N )
        {
            if( Map.I.Hero.Dir == EDirection.N ) return EMoveType.FRONT;
            else
                if( Map.I.Hero.Dir == EDirection.NW || Map.I.Hero.Dir == EDirection.NE ) return EMoveType.FRONTSIDE;
                else
                    if( Map.I.Hero.Dir == EDirection.E || Map.I.Hero.Dir == EDirection.W ) return EMoveType.SIDE;
                    else
                        if( Map.I.Hero.Dir == EDirection.SE || Map.I.Hero.Dir == EDirection.SW ) return EMoveType.BACKSIDE;
                        else
                            if( Map.I.Hero.Dir == EDirection.S ) return EMoveType.BACK;
        }
        else
            if( ua == EActionType.MOVE_NE )
            {
                if( Map.I.Hero.Dir == EDirection.NE ) return EMoveType.FRONT;
                else
                    if( Map.I.Hero.Dir == EDirection.N || Map.I.Hero.Dir == EDirection.E ) return EMoveType.FRONTSIDE;
                    else
                        if( Map.I.Hero.Dir == EDirection.SE || Map.I.Hero.Dir == EDirection.NW ) return EMoveType.SIDE;
                        else
                            if( Map.I.Hero.Dir == EDirection.S || Map.I.Hero.Dir == EDirection.W ) return EMoveType.BACKSIDE;
                            else
                                if( Map.I.Hero.Dir == EDirection.SW ) return EMoveType.BACK;
            }
            else
                if( ua == EActionType.MOVE_E )
                {
                    if( Map.I.Hero.Dir == EDirection.E ) return EMoveType.FRONT;
                    else
                        if( Map.I.Hero.Dir == EDirection.NE || Map.I.Hero.Dir == EDirection.SE ) return EMoveType.FRONTSIDE;
                        else
                            if( Map.I.Hero.Dir == EDirection.S || Map.I.Hero.Dir == EDirection.N ) return EMoveType.SIDE;
                            else
                                if( Map.I.Hero.Dir == EDirection.SW || Map.I.Hero.Dir == EDirection.NW ) return EMoveType.BACKSIDE;
                                else
                                    if( Map.I.Hero.Dir == EDirection.W ) return EMoveType.BACK;
                }
                else
                    if( ua == EActionType.MOVE_SE )
                    {
                        if( Map.I.Hero.Dir == EDirection.SE ) return EMoveType.FRONT;
                        else
                            if( Map.I.Hero.Dir == EDirection.E || Map.I.Hero.Dir == EDirection.S ) return EMoveType.FRONTSIDE;
                            else
                                if( Map.I.Hero.Dir == EDirection.SW || Map.I.Hero.Dir == EDirection.NE ) return EMoveType.SIDE;
                                else
                                    if( Map.I.Hero.Dir == EDirection.W || Map.I.Hero.Dir == EDirection.N ) return EMoveType.BACKSIDE;
                                    else
                                        if( Map.I.Hero.Dir == EDirection.NW ) return EMoveType.BACK;
                    }
                    else
                        if( ua == EActionType.MOVE_S )
                        {
                            if( Map.I.Hero.Dir == EDirection.S ) return EMoveType.FRONT;
                            else
                                if( Map.I.Hero.Dir == EDirection.SE || Map.I.Hero.Dir == EDirection.SW ) return EMoveType.FRONTSIDE;
                                else
                                    if( Map.I.Hero.Dir == EDirection.W || Map.I.Hero.Dir == EDirection.E ) return EMoveType.SIDE;
                                    else
                                        if( Map.I.Hero.Dir == EDirection.NW || Map.I.Hero.Dir == EDirection.NE ) return EMoveType.BACKSIDE;
                                        else
                                            if( Map.I.Hero.Dir == EDirection.N ) return EMoveType.BACK;
                        }
                        else
                            if( ua == EActionType.MOVE_SW )
                            {
                                if( Map.I.Hero.Dir == EDirection.SW ) return EMoveType.FRONT;
                                else
                                    if( Map.I.Hero.Dir == EDirection.S || Map.I.Hero.Dir == EDirection.W ) return EMoveType.FRONTSIDE;
                                    else
                                        if( Map.I.Hero.Dir == EDirection.NW || Map.I.Hero.Dir == EDirection.SE ) return EMoveType.SIDE;
                                        else
                                            if( Map.I.Hero.Dir == EDirection.N || Map.I.Hero.Dir == EDirection.E ) return EMoveType.BACKSIDE;
                                            else
                                                if( Map.I.Hero.Dir == EDirection.NE ) return EMoveType.BACK;
                            }
                            else
                                if( ua == EActionType.MOVE_W )
                                {
                                    if( Map.I.Hero.Dir == EDirection.W ) return EMoveType.FRONT;
                                    else
                                        if( Map.I.Hero.Dir == EDirection.SW || Map.I.Hero.Dir == EDirection.NW ) return EMoveType.FRONTSIDE;
                                        else
                                            if( Map.I.Hero.Dir == EDirection.N || Map.I.Hero.Dir == EDirection.S ) return EMoveType.SIDE;
                                            else
                                                if( Map.I.Hero.Dir == EDirection.NE || Map.I.Hero.Dir == EDirection.SE ) return EMoveType.BACKSIDE;
                                                else
                                                    if( Map.I.Hero.Dir == EDirection.E ) return EMoveType.BACK;
                                }
                                else
                                    if( ua == EActionType.MOVE_NW )
                                    {
                                        if( Map.I.Hero.Dir == EDirection.NW ) return EMoveType.FRONT;
                                        else
                                            if( Map.I.Hero.Dir == EDirection.W || Map.I.Hero.Dir == EDirection.N ) return EMoveType.FRONTSIDE;
                                            else
                                                if( Map.I.Hero.Dir == EDirection.NE || Map.I.Hero.Dir == EDirection.SW ) return EMoveType.SIDE;
                                                else
                                                    if( Map.I.Hero.Dir == EDirection.E || Map.I.Hero.Dir == EDirection.S ) return EMoveType.BACKSIDE;
                                                    else
                                                        if( Map.I.Hero.Dir == EDirection.SE ) return EMoveType.BACK;
                                    }

        return EMoveType.NONE;
    }

    public void UpdateArrowRush()
    {
        if( ArrowRushEnabled == false ) return;
        EDirection dr = Map.I.GetArrowDir( Unit.Pos );
        if( dr == EDirection.NONE ) return;
        Vector2 tg = Unit.Pos + Manager.I.U.DirCord[ ( int ) dr ];

        if( Unit.CanMoveFromTo( false, Unit.Pos, tg, Unit ) == false ) return;
        Unit.UpdateOrbHit();

        ApplyMove( Unit.Pos, tg );
    }
    public void ResetShakeTimer()
    {
        ShakeTimer = 2.5f + ( .7f * ( MoveOrderID - 1 ) );
    }
    public void UpdateMeleeTargetLock( Vector2 from, Vector2 to, EActionType ac )
    {
        if( LockMeleeTarget == false ) return;
        if( ac == EActionType.ROTATE_CCW || ac == EActionType.ROTATE_CW ||
            ac == EActionType.SPECIAL || ac == EActionType.WAIT ) return;

        Vector2 tg = from + Manager.I.U.DirCord[ ( int ) Unit.Dir ];

        if( Map.I.PtOnAreaMap( tg ) == false ) return;
        if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ] == null ) return;
        if( Map.I.Unit[ ( int ) tg.x, ( int ) tg.y ].ValidMonster == false ) return;

        EDirection dr = EDirection.NONE;

        for( int i = 0; i < 8; i++ )
        {
            Vector2 aux = to + Manager.I.U.DirCord[ i ];
            if( aux == tg ) { dr = ( EDirection ) i; break; }
        }

        if( dr == EDirection.NONE ) return;

        Unit.RotateTo( dr );
    }

    public void UpdatePositionHistory()
    {
        if( Map.I.CurrentArea != -1 )
            if( Map.I.CurArea.AreaTurnCount == 0 )
            {
                PositionHistory = new List<Vector2>();
            }

        PositionHistory.Add( Unit.Pos );
    }
    public bool UpdateSleep( bool forcewake = false )
    {
        if( Unit.UnitType != EUnitType.MONSTER ) return false;
        float r = 0;

        if( Map.I.Hero.Control.SneakingLevel >= 1 )
            if( forcewake == false )
                if( Map.I.HasLOS( Unit.Pos, Map.I.Hero.Pos ) == false ) return false;

        EZone zone = G.Hero.GetPositionZone( Unit.Pos, ref r );

        if( Map.I.Hero.Control.SneakingLevel < 1 ) Sleeping = false;

        if( zone == EZone.Back )
            if( Map.I.Hero.Control.SneakingLevel < 1 ) Sleeping = false;

        if( zone == EZone.BackLeft || zone == EZone.BackRight )
            if( Map.I.Hero.Control.SneakingLevel < 2 ) Sleeping = false;

        if( zone == EZone.BackSideLeft || zone == EZone.BackSideRight ||
            zone == EZone.Left || zone == EZone.Right )
            if( Map.I.Hero.Control.SneakingLevel < 3 ) Sleeping = false;

        if( zone == EZone.FrontSideLeft || zone == EZone.FrontSideRight )
            if( Map.I.Hero.Control.SneakingLevel < 4 ) Sleeping = false;

        if( zone == EZone.FrontLeft || zone == EZone.FrontRight )
            if( Map.I.Hero.Control.SneakingLevel < 5 ) Sleeping = false;

        if( zone == EZone.Front ) Sleeping = false;

        if( forcewake ) Sleeping = false;

        if( Sleeping == false )
        {
            if( Unit.LevelTxt )
                Unit.LevelTxt.text = Unit.LevelTxt.text.Replace( 'Z', ' ' );
            if( Unit.TileID == ETileType.SCARAB )
                Unit.LevelTxt.gameObject.SetActive( false );
        }
        return !Sleeping;
    }

    public bool UpdateResting()
    {
        if( IsDynamicObject() == false )
            if( Unit.UnitType != EUnitType.MONSTER ) return false;
        if( Resting == false ) return false;

        int dist = 0;
        bool ok = InsideRestingRange( ref dist );

        if( ok )
        {
            WakeUp();
            return false;
        }
        return Resting;
    }
    public bool InsideRestingRange( ref int dist )
    {
        int basedist = ( int ) Map.I.RM.RMD.BaseRestingDistance;
        int rad = ( int ) ( basedist - G.Hero.Control.RestingLevel );
        if( rad < 0 ) rad = 0;
        rad += BaseRestDistance;

        dist = Util.Manhattan( G.Hero.Pos, Unit.Pos ) - rad;

        if( Unit.TileID == ETileType.QUEROQUERO )                                                                                            // Dragon 3 unlimited range if hero in front
        {
            float rr = 0;
            EZone zone = Unit.GetPositionZone( G.Hero.Pos, ref rr );
            if( zone == EZone.Front )
                if( Map.I.IsInTheSameLine( Unit.Pos, G.Hero.Pos, true ) )
                    if( Map.I.HasLOS( Unit.Pos, G.Hero.Pos, true, Unit, false, true, "Dragon Shot" ) )
                        rad = Sector.TSX;
            Unit.Spr.transform.rotation = Util.GetRotationToPoint( Unit.Spr.transform.position, G.Hero.Pos );
            EDirection dr = Util.GetRotationToTarget( Unit.Pos, G.Hero.Pos );
            Unit.RotateTo( dr );
            FlightJumpPhase = -1;                                                                                                           // Hero immune while waking up dragon 3
            FlightPhaseTimer = 0;
        }

        Rect r = new Rect( Unit.Pos.x - rad, Unit.Pos.y - rad, 1 + ( rad * 2 ), 1 + ( rad * 2 ) );
        bool los = true;

        if( Map.I.RM.RMD.RestingNeedLOSCheck )                                                                                             // LOS Check if Needed
            if( Map.I.HasLOS( G.Hero.Pos, Unit.Pos ) == false ) los = false;

        if( los && r.Contains( G.Hero.Pos ) )
            return true;
        return false;
    }

    public void WakeUp()
    {
        if( Resting == false ) return;

        Unit fromFog = Controller.GetFog( G.Hero.Pos );                                                // Monster inside Fog Stay Sleeping
        Unit toFog = Controller.GetFog( Unit.Pos );
        if( fromFog == null && toFog != null ) return;

        bool group = Unit.WakeMeUp();
        WakeupTimeCounter = 0;

        MasterAudio.PlaySound3DAtVector3( "Click 2", Unit.transform.position );

        if( group == false ) return;

        if( WakeUpGroup >= 0 )
            for( int y = ( int ) Map.I.RM.HeroSector.Area.yMin; y < Map.I.RM.HeroSector.Area.yMax; y++ )   // Wakes up all monsters in the wake up group                                                  // Create sector objects 
                for( int x = ( int ) Map.I.RM.HeroSector.Area.xMin; x < Map.I.RM.HeroSector.Area.xMax; x++ )
                    if( Map.PtOnMap( Quest.I.Dungeon.Tilemap, new Vector2( x, y ) ) )
                    {
                        Unit un = Map.I.GetUnit( new Vector2( x, y ), ELayerType.MONSTER );
                        if( un && un.Control.WakeUpGroup == WakeUpGroup )
                        {
                            un.WakeMeUp();
                        }
                    }

        if( WakeUpGroup >= 0 )                                                                         // flying units group
            for( int i = 0; i < G.HS.Fly.Count; i++ )
            {
                if( G.HS.Fly[ i ].Body.IsDead == false )
                    if( G.HS.Fly[ i ].Control.WakeUpGroup == WakeUpGroup )
                    {
                        G.HS.Fly[ i ].WakeMeUp();
                    }
            }

        if( WakeUpGroup >= 0 )                                                                         // Dynamic objects group
            for( int i = 0; i < Map.I.RM.HeroSector.DynamicObjects.Count; i++ )
            {
                if( Map.I.RM.HeroSector.DynamicObjects[ i ].Control.WakeUpGroup == WakeUpGroup )
                {
                    Map.I.RM.HeroSector.DynamicObjects[ i ].WakeMeUp();
                }
            }
    }
    public float GetRealtimeSpeedTime( int lv = -1, bool boost = true )
    {
        if( Unit.Md && Unit.Md.RealtimeSpeedList != null && Unit.Md.RealtimeSpeedList.Count > 0 )           // Custom speed list movement type
        {
            return Unit.Md.RealtimeSpeedList[ SpeedListID ];
        }

        float spd = GetMonsterRTMovSpeed( lv );
        float res = 1 / ( spd / Map.I.Tick );
        res *= Helper.I.RealtimeSpeedFactor * Map.I.RM.HeroSector.RealtimeSpeedFactor;
        return res;
    }
    public float GetMonsterRTMovSpeed( int lv = -1 )
    {
        float sp = 1;
        if( lv == -1 ) sp = Map.I.RM.RMD.RTMonsterMovementSpeed - G.Hero.Control.MireLevel;
        else sp = Map.I.RM.RMD.RTMonsterMovementSpeed - lv;
        if( RealtimeSpeed > 0 ) sp = RealtimeSpeed;
        //if( sp < 1 ) sp = 1;
        return sp;
    }

    public float CalculateRestingTiles()
    {
        Vector2 dif = G.Hero.Pos - Unit.Pos;
        if( dif.x < 0 ) dif.x *= -1;
        if( dif.y < 0 ) dif.y *= -1;

        int basedist = ( int ) Map.I.RM.RMD.BaseRestingDistance;
        int rad = ( int ) ( basedist - G.Hero.Control.RestingLevel );
        rad += BaseRestDistance;
        if( rad < 0 ) rad = 0;

        float numx = dif.x - rad;
        float numy = dif.y - rad;

        if( dif.x > dif.y ) return numx;
        else return numy;
    }

    public Vector2 GetBestNeighborTarget( Vector2 tg )
    {
        int range = 1;
        Vector2 pos = new Vector2( ( int ) Unit.Pos.x, ( int ) Unit.Pos.y );
        Vector2 bestpos = new Vector2( 0, 0 );
        float bestscore = Util.Manhattan( tg, Unit.Pos );
        float dist = Vector3.Distance( new Vector3( tg.x, tg.y, 0 ), new Vector3( Unit.Pos.x, Unit.Pos.y, 0 ) ) / 100;
        bestscore += dist;
        for( int y = ( int ) Unit.Pos.y - range; y <= Unit.Pos.y + range; y++ )
            for( int x = ( int ) Unit.Pos.x - range; x <= Unit.Pos.x + range; x++ )
            {
                Vector2 target = new Vector2( x, y );
                if( Map.PtOnMap( Map.I.Tilemap, target ) )
                    if( Sector.IsPtInCube( target ) )
                    {
                        float score = Util.Manhattan( tg, target );
                        dist = Vector3.Distance( new Vector3( tg.x,
                        tg.y, 0 ), new Vector3( target.x, target.y, 0 ) ) / 100;
                        score += dist;
                        Vector2 deltatg = tg - target;
                        deltatg = new Vector2( Mathf.Abs( deltatg.x ), Mathf.Abs( deltatg.y ) );
                        Vector2 deltamn = tg - Unit.Pos;
                        deltamn = new Vector2( Mathf.Abs( deltamn.x ), Mathf.Abs( deltamn.y ) );
                        bool skip = false;
                        if( deltatg.x > deltamn.x ||                                                      // Prevent the monster moving away from hero bug
                            deltatg.y > deltamn.y ) skip = true;

                        if( skip == false )
                            if( target != tg )
                                if( Unit.CanMoveFromTo( false, pos, target, Unit ) == true )
                                {
                                    bool checklure = false;
                                    if( Map.I.IsInTheSameLine( tg, target, false ) )
                                        if( Util.Manhattan( G.Hero.Control.OldPos, Unit.Pos ) > 1 )
                                            if( Util.IsInTheSameDiagonal( G.Hero.Control.OldPos, Unit.Pos ) == false )
                                                if( Util.IsInTheSameDiagonal( G.Hero.Pos, Unit.Pos ) == false )
                                                    checklure = true;

                                    if( checklure )                                                          // Lure monsters to hide behind obstacles
                                    {
                                        EDirection d2 = Util.GetRotationToTarget( target, tg );
                                        Vector2 tg2 = target + Manager.I.U.DirCord[ ( int ) d2 ];
                                        CheckHeroSwordBlock = false;
                                        CheckingLure = true;
                                        if( Unit.CanMoveFromTo( false, target, tg2, Unit ) == false )
                                            if( Unit.CanMoveFromTo( false, pos, tg2, Unit ) == false )
                                                score -= 2;
                                        CheckHeroSwordBlock = true;
                                        CheckingLure = false;
                                    }

                                    if( score < bestscore )
                                    {
                                        bestpos = target;
                                        bestscore = score;
                                    }
                                }
                    }
            }
        return bestpos;
    }

    public Vector2 GetBestStandardMove( Vector2 tg, bool diagonalLock = true )
    {
        if( Brain.Active() )
        if( Brain.IsMonsterTrapped( Unit.Pos ) == false )
            return Brain.GetBestMove( Unit.Pos );                                        // Brain move

        int amt = 1;
        Vector2 best = Vector2.zero;

        #region BacksideCode
        //if( Util.Manhattan( tg, Unit.Pos ) <= 2 )                                                    // Backside neighbor move
        //{
        //    Vector2 tg = tg + G.Hero.GetRelativePosition( EDirection.S, 1 );
        //    if( Util.IsNeighbor( Unit.Pos, tg ) && 
        //        Unit.CanMoveFromTo( false, Unit.Pos, tg, Unit ) == true ) 
        //        best = tg;
        //    else
        //    {
        //        tg = tg + G.Hero.GetRelativePosition( EDirection.SE, 1 );
        //        if( Util.IsNeighbor( Unit.Pos, tg ) && 
        //            Unit.CanMoveFromTo( false, Unit.Pos, tg, Unit ) == true )
        //            best = tg;
        //        else
        //        {
        //            tg = tg + G.Hero.GetRelativePosition( EDirection.SW, 1 );
        //            if( Util.IsNeighbor( Unit.Pos, tg ) && 
        //                Unit.CanMoveFromTo( false, Unit.Pos, tg, Unit ) == true )
        //                best = tg;
        //        }
        //    }
        //    if( best != Vector2.zero ) best -= Unit.Pos;
        //}
        #endregion

        if( best == Vector2.zero )
            if( tg.x == Unit.Pos.x )                                                     // Same X cord
            {
                if( tg.y > Unit.Pos.y ) best = new Vector2( 0, +amt );
                else
                    if( tg.y < Unit.Pos.y ) best = new Vector2( 0, -amt );
            }
            else
                if( tg.y == Unit.Pos.y )                                                 // Same Y cord
                {
                    if( tg.x > Unit.Pos.x ) best = new Vector2( +amt, 0 );
                    else
                        if( tg.x < Unit.Pos.x ) best = new Vector2( -amt, 0 );
                }
                else
                {
                    if( Map.I.IsInTheSameLine( tg, Unit.Pos, true ) && diagonalLock )
                    {
                        Vector2 aux = new Vector2( 0, 0 );
                        Vector2 tg1 = new Vector2( 0, 0 );
                        Vector2 tg2 = new Vector2( 0, 0 );

                        if( tg.x > Unit.Pos.x )
                            if( tg.y > Unit.Pos.y )
                                aux = new Vector2( +amt, +amt );

                        if( tg.x < Unit.Pos.x )
                            if( tg.y > Unit.Pos.y )
                                aux = new Vector2( -amt, +amt );

                        if( tg.x < Unit.Pos.x )
                            if( tg.y < Unit.Pos.y )
                                aux = new Vector2( -amt, -amt );

                        if( tg.x > Unit.Pos.x )
                            if( tg.y < Unit.Pos.y )
                                aux = new Vector2( +amt, -amt );

                        tg1 = new Vector2( 0, aux.y );
                        tg2 = new Vector2( aux.x, 0 );

                        if( Unit.Control.IsFlyingUnit == false || Unit.TileID == ETileType.MINE ) //new: second part added recently for sticky mine
                        {
                            if( Unit.CanMoveFromTo( false, Unit.Pos, Unit.Pos + tg1, Unit ) == false ||
                                Unit.CanMoveFromTo( false, Unit.Pos, Unit.Pos + tg2, Unit ) == false )
                                best = GetBestNeighborTarget( tg ) - Unit.Pos;
                            else
                            {
                                best = aux;
                            }
                        }
                    }
                    else
                        best = GetBestNeighborTarget( tg ) - Unit.Pos;
                }
        return Unit.Pos + best;
    }

    #region Spider

    // IMPORTANT: DeathCountFactor needs to be revised for monster death counter. Changed since the Spider system changed from level based to the new system

    public bool UpdateSpiderStepping( Vector2 from, Vector2 to )
    {
        Unit spd = Map.I.GetUnit( ETileType.SPIDER, to );
        if( spd == null ) return true;

        EDirection md = Util.GetTargetUnitDir( from, to );
        List<Vector2> ul = new List<Vector2>();
        List<ESpiderBabyType> st = new List<ESpiderBabyType>();
        List<int> bvar = new List<int>();

        for( int i = 0; i < 8; i++ )
            if( spd.Body.Sp[ i ].Type != ESpiderBabyType.NONE )
            {
                if( Spell.IsSpawnAble( spd.Body.Sp[ i ].Type ) )                                // First line  
                    ul.Add( spd.Pos + spd.GetRelativePosition( ( EDirection ) i, 1 ) );
                st.Add( spd.Body.Sp[ i ].Type );
                bvar.Add( spd.Body.BabyVariation[ i ] );
                if( Spell.IsSpawnAble( spd.Body.Sp[ i + 8 ].Type ) )                            // Second Line 
                    ul.Add( spd.Pos + spd.GetRelativePosition( ( EDirection ) i, 2 ) );
                st.Add( spd.Body.Sp[ i + 8 ].Type );
                bvar.Add( spd.Body.BabyVariation[ i + 8 ] );
                if( Spell.IsSpawnAble( spd.Body.Sp[ i + 16 ].Type ) )                            // Third Line
                    ul.Add( spd.Pos + spd.GetRelativePosition( ( EDirection ) i, 3 ) );
                st.Add( spd.Body.Sp[ i + 16 ].Type );
                bvar.Add( spd.Body.BabyVariation[ i + 16 ] );
            }

        if( CheckSpiderBlock( from, to ) )                                       // Spider blocked?
        {
            return false;
        }

        float deathfact = 1;
        if( spd.Body.Level >= 2 )
            deathfact = spd.Body.DeathCountFactor / ul.Count;

        List<float> rr = new List<float>();
        for( int i = 0; i < ul.Count; i++ )
            rr.Add( Vector2.Distance( G.Hero.Pos, ul[ i ] ) );

        for( int k = 1; k < ul.Count; k++ )                                     // reorder unit creation order by hero distance        
            for( int j = 0; j < ul.Count - k; j++ )
                if( ( rr[ j ] < rr[ j + 1 ] ) )
                {
                    float temp = rr[ j ];
                    rr[ j ] = rr[ j + 1 ];
                    rr[ j + 1 ] = temp;
                    Vector2 tempun = ul[ j ];
                    ul[ j ] = ul[ j + 1 ];
                    ul[ j + 1 ] = tempun;
                    ESpiderBabyType tempst = st[ j ];
                    st[ j ] = st[ j + 1 ];
                    st[ j + 1 ] = tempst;
                    int tempbvar = bvar[ j ];
                    bvar[ j ] = bvar[ j + 1 ];
                    bvar[ j + 1 ] = tempbvar;
                }

        for( int i = 0; i < ul.Count; i++ )
        {
            CheckHeroSwordBlock = false;
            if( spd.CanMoveFromTo( false, spd.Pos, ul[ i ], spd ) == true )
            {
                ETileType tile = ETileType.NONE;
                Unit prefab = null;
                if( st[ i ] == ESpiderBabyType.SPIDER ) { tile = ETileType.SPIDER; prefab = spd; }
                if( st[ i ] == ESpiderBabyType.SCORPION ) tile = ETileType.SCORPION;
                if( st[ i ] == ESpiderBabyType.SCARAB ) tile = ETileType.SCARAB;
                if( st[ i ] == ESpiderBabyType.ITEM ) tile = ETileType.ITEM;
                if( st[ i ] == ESpiderBabyType.EMPTY_TILE ) tile = ETileType.NONE;
                GameObject go = null;
                if( tile != ETileType.NONE )
                {
                    if( prefab == null )
                        prefab = Map.I.GetUnitPrefab( tile );
                    go = Map.I.CreateUnit( prefab, ul[ i ] );                                           // Spawns the baby
                }

                if( go )
                {
                    Unit un = go.GetComponent<Unit>();
                    un.Control.Resting = false;
                    if( un.ValidMonster )
                        if( Map.I.RM.HeroSector.MoveOrder.Contains( un ) == false )                       // adds to move list
                        {
                            Map.I.RM.HeroSector.MoveOrder.Add( un );
                            un.Control.MoveOrderID = Map.I.RM.HeroSector.MoveOrder.Count - 1;
                        }

                    if( st[ i ] == ESpiderBabyType.ITEM )                                             // Create item
                    {
                        un.Variation = bvar[ i ];
                        if( un.Variation >= 0 )
                            un.Spr.spriteId = G.GIT( un.Variation ).TKSprite.spriteId;
                        un.Body.StackAmount = 1;
                        un.Body.BonusItemList = null;
                    }
                    un.Control.RealtimeSpeed = spd.Control.RealtimeSpeed;
                    un.Body.DeathCountFactor = deathfact;
                    un.Control.SpiderAttackBlockPhase = 0;
                    un.Control.SpeedTimeCounter = 0;
                    un.Control.MergingID = Map.I.LevelTurnCount;
                    un.Control.SmoothMovementFactor = 0;
                    un.Control.LastPos = spd.Pos;
                    un.Control.OldPos = spd.Pos;
                    un.Control.TurnTime = 0;
                    un.Control.UnitMoved = true;
                    if( Map.Stepping() )
                        un.Control.SkipMovement = 1;
                    un.Control.AnimationOrigin = spd.Pos;
                }
            }
            CheckHeroSwordBlock = true;
        }

        if( spd.Body.Level > 1 )                                                                         // A Spider L2 or up has spreaded, so do not cout kill, but only spreads death count factor
            spd.Body.DeathCountFactor = 0;

        spd.Control.MergingID = -1;
        spd.Control.WakeUp();
        spd.Kill();                                                                                      // Kill stepped Spider
        Attack.SpiderSquash = true;
        spd.Body.CreateBloodSpilling( to );
        spd.Body.CreateBloodSplat();
        MasterAudio.PlaySound3DAtVector3( "Slime", transform.position );

        int cont = 0;
        Sector s = Map.I.RM.HeroSector;
        for( int i = 0; i < s.MoveOrder.Count; i++ )
            if( s.MoveOrder[ i ].TileID == ETileType.SPIDER )                                               // This is for avoiding the spider not moving bug
            {
                bool single = true;
                for( int ss = 0; ss < 8; ss++ )
                    if( s.MoveOrder[ i ].Body.Sp[ ss ].Type != ESpiderBabyType.NONE )
                        single = false;
                if( single ) cont++;
            }

        if( cont <= 2 )                                                                                 // Resets merge id if theres no mergeable spiders available
            for( int i = 0; i < s.MoveOrder.Count; i++ )
                if( s.MoveOrder[ i ].TileID == ETileType.SPIDER )
                    s.MoveOrder[ i ].Control.MergingID = -1;
        return true;
    }

    public static List<int> blockiID = new List<int>();
    public Vector3 FishCatchPosition;
    public static Vector2 PunchOrigin;

    public bool CheckSpiderBlock( Vector2 from, Vector2 to )
    {
        if( Unit.UnitType != EUnitType.HERO ) return false;  // new, check for bugs
        Unit spd = Map.I.GetUnit( ETileType.SPIDER, to );
        if( spd == null ) return false;
        EDirection md = Util.GetTargetUnitDir( from, to );
        List<Vector2> ul = new List<Vector2>();
        List<Vector2> ulor = new List<Vector2>();
        List<ESpiderBabyType> btype = new List<ESpiderBabyType>();
        List<int> st = new List<int>();
        blockiID = new List<int>();
        bool res = false;
        bool single = true;

        for( int i = 0; i < 8; i++ )
        {
            if( spd.Body.Sp[ i + 16 ].Type != ESpiderBabyType.NONE )                                 // third Spider layer
            {
                ul.Add( spd.Pos + spd.GetRelativePosition( ( EDirection ) i, 3 ) );
                ulor.Add( spd.Pos + spd.GetRelativePosition( ( EDirection ) i, 1 ) );
                st.Add( i + 16 ); single = false;
                btype.Add( spd.Body.Sp[ i + 16 ].Type );
            }
            if( spd.Body.Sp[ i + 8 ].Type != ESpiderBabyType.NONE )                                 // second Spider layer
            {
                ul.Add( spd.Pos + spd.GetRelativePosition( ( EDirection ) i, 2 ) );
                ulor.Add( spd.Pos + spd.GetRelativePosition( ( EDirection ) i, 1 ) );
                st.Add( i + 8 ); single = false;
                btype.Add( spd.Body.Sp[ i + 8 ].Type );
            }
            if( spd.Body.Sp[ i ].Type != ESpiderBabyType.NONE )                                     // first Spider layer
            {
                ul.Add( spd.Pos + spd.GetRelativePosition( ( EDirection ) i, 1 ) );
                ulor.Add( spd.Pos + spd.GetRelativePosition( ( EDirection ) i, 1 ) );
                st.Add( i ); single = false;
                btype.Add( spd.Body.Sp[ i ].Type );
            }
        }

        if( single )
            if( spd.Control.SpiderAttackBlockPhase == 1 ) return true;                                       // Green Single (non Mother) Slime - Ready to Attack Spider cannot be stepped on Stepping Mode

        for( int i = 0; i < ul.Count; i++ )
        {
            CheckHeroSwordBlock = false;
            Unit water = Map.I.GetUnit( ETileType.WATER, ul[ i ] );
            if( water == null ) water = Map.I.GetUnit( ETileType.PIT, ul[ i ] );

            if( ul[ i ] != G.Hero.Pos || Spell.IsSpawnAble( spd.Body.Sp[ i ].Type ) == false )
                if( btype[ i ] != ESpiderBabyType.EMPTY_TILE )
                    if( spd.CanMoveFromTo( false, spd.Pos, ul[ i ], spd ) == false )
                        if( water == null )
                        {
                            blockiID.Add( st[ i ] );
                        }
            if( water != null ) res = true;
            CheckHeroSwordBlock = true;
            Unit mn = Map.I.GetUnit( ul[ i ], ELayerType.MONSTER );

            if( mn )
            {
                if( mn.TileID == ETileType.BARRICADE ) blockiID.Add( st[ i ] );
            }

            if( btype[ i ] == ESpiderBabyType.COCOON || btype[ i ] == ESpiderBabyType.BOTH )                           // new: now instead of spawning, cocoon block movement until destroyed (check scarab for same rule)
            {
                List<ETileType> ex = new List<ETileType>();
                ex.Add( ETileType.WATER ); ex.Add( ETileType.PIT );
                bool ok = Util.CheckBlock( spd.Pos, ulor[ i ], G.Hero, true, false, true, false, false, true, ex );
                if( ok )
                {
                    if( spd.Control.EggsDestroyed ) return true;
                    spd.Control.EggsDestroyed = true;
                    PlayBumpSound = false;
                    spd.Body.Sp[ st[ i ] ].Reset();
                    spd.Control.SpeedTimeCounter = 0;                                                                  // destroying cocoon resets movement counter
                    spd.MeleeAttack.SpeedTimeCounter = 0;
                    MasterAudio.PlaySound3DAtVector3( "Spider Merge", spd.Pos );
                }
                res = true;
            }
        }
        if( blockiID != null && blockiID.Count >= 1 ) return true;                                                      // Block hero
        return res;
    }

    public static void UpdateSpiderMerge( bool step )
    {
        return;                                        // review all over again since spell system has been added. maybe no merging could be better 
        //  maybe only merge if there is a specia item attached to spider
        List<Unit> unl = new List<Unit>();
        List<float> bestscore = new List<float>();

        Sector hs = Map.I.RM.HeroSector;
        for( int y = ( int ) hs.Area.yMin - 1; y < hs.Area.yMax + 1; y++ )                         // Make a list of all single mergeable spiders
            for( int x = ( int ) hs.Area.xMin - 1; x < hs.Area.xMax + 1; x++ )
            {
                Unit un = Map.I.GetUnit( ETileType.SPIDER, new Vector2( x, y ) );
                if( un )
                {
                    bool ok = true;
                    for( int s = 0; s < 8; s++ )
                        if( un.Body.Sp[ s ].Type != ESpiderBabyType.NONE )
                            ok = false;
                    if( un.Control.Resting == true ) ok = false;
                    if( un.Control.TickBasedMovement )
                        if( MoveTickDone == false || un.Control.CheckTickMove() == false ) ok = false;
                    if( un.Control.TickBasedMovement == false )
                        if( un.Control.SpeedTimeCounter < un.Control.GetRealtimeSpeedTime() ) ok = false;
                    if( step )
                        if( un.Control.MergingID == -1 ) ok = false;
                    if( !step )
                        if( un.Control.MergingID != -1 ) ok = false;
                    if( ok )
                    {
                        unl.Add( un );                                                                  // Adds the spider to the list
                        bestscore.Add( 0 );
                        if( un.Control.MergingID != -1 )
                            ResetMergingID = true;
                    }
                }
            }

        for( int phase = 0; phase < unl.Count; phase++ )                                   // loops through all mergeable spiders based by ascending score
        {
            for( int u = unl.Count - 1; u >= 0; u-- )
            {
                List<Unit> add = new List<Unit>() { null, null, null, null, null, null, null, null };
                for( int dr = 0; dr < 8; dr++ )
                {
                    int dir = dr + ( int ) unl[ u ].Dir;
                    if( dir < 0 ) dir += 8;
                    if( dir > 7 ) dir -= 8;
                    bool skip = false;

                    add[ dr ] = CheckSpiderMergeBlock( unl[ u ].Pos,                       // can spider merge or is blocked by arrows for example
                    unl[ u ].Pos + Manager.I.U.DirCord[ dir ], step, ref skip );
                    if( skip )
                    {
                        add = null;
                        break;
                    }
                    if( add[ dr ] != null )                                                           // Calculates score
                    {
                        float r = 0;
                        EZone zone = G.Hero.GetPositionZone( unl[ u ].Pos, ref r );
                        if( zone == EZone.Front ) bestscore[ u ] += 1000;                             // 1) - by being in the hero frontal zone
                        bestscore[ u ] += 100;                                                        // 2) - by number of neighbor spiders
                        if( Util.IsEven( dir ) == true ) bestscore[ u ] += 10;                        // 3) - by number of ortho neighbor spiders
                        bestscore[ u ] -= Vector2.Distance( G.Hero.Pos, add[ dr ].Pos ) / 10000;      // 4) - by hero distance
                    }
                }
                unl[ u ].Control.SpiderChild = add;
            }

            Util.SortUnitListByScore( ref bestscore, ref unl );                             // Sorts spider list order by score order

            Unit sp = unl[ 0 ];
            int count = 0;
            for( int dd = 0; dd < 8; dd++ )                                                 //counts babies
                if( sp.Control.SpiderChild != null )
                    if( sp.Control.SpiderChild[ dd ] != null )
                        count++;

            if( count >= 2 )                                                                // merges 3 or more spiders
            {
                for( int dd = 0; dd < 8; dd++ )                                             // Adds the baby
                    if( sp.Control.SpiderChild[ dd ] != null )
                        sp.Body.Sp[ dd ].Type = ESpiderBabyType.SPIDER;

                for( int k = 0; k < sp.Control.SpiderChild.Count; k++ )
                    if( sp.Control.SpiderChild[ k ] )
                    {
                        sp.Body.DeathCountFactor +=
                        sp.Control.SpiderChild[ k ].Body.DeathCountFactor;
                        sp.Control.SkipMovement = 1;
                        sp.Control.MergingID = -1;
                        sp.MeleeAttack.SpeedTimeCounter = 0;
                        sp.Control.SpiderChild[ k ].Kill();                                 // kills the remaining spider
                    }
                MasterAudio.PlaySound3DAtVector3( "Spider Merge", G.Hero.Pos, 2, null, .2f );
            }
        }
    }
    public static Unit CheckSpiderMergeBlock( Vector2 to, Vector2 from, bool step, ref bool skip )
    {
        Unit un = Map.I.GetUnit( ETileType.SPIDER, from );
        if( un == null ) return null;
        bool single = true;
        for( int s = 0; s < 8; s++ )
            if( un.Body.Sp[ s ].Type == ESpiderBabyType.SPIDER )
                single = false;
        if( single == false )
            return null;

        if( !step )                                                                 // moving spiders only merge when all of them are move ready
        {
            if( un.Control.TickBasedMovement == false )
                if( un.Control.Resting == false )
                    if( un.Control.SpeedTimeCounter < un.Control.GetRealtimeSpeedTime() )
                    {
                        skip = true;
                        return null;
                    }
        }

        if( un && un.CheckTerrainMove( from, to, true, true, false, true, true ) == false ) un = null;
        if( un && un.Control.MergingID != -1 )
            ResetMergingID = true;
        return un;
    }
    #endregion
    public static void UpdateTickTimer()
    {
        if( Map.I.TicTacMonsterCount < 1 ) return;
        if( Map.I.IsPaused() ) return;
        G.HS.TickTimeCounter += Time.deltaTime;                                                             // Delta Time Updates
        MoveTickDone = false;
        float step = Map.I.RM.RMD.TickMoveTime;

        if( CubeData.I.TickMoveList != null )
        if( CubeData.I.TickMoveList.Count > 0 )
        {
            step = CubeData.I.TickMoveTime;                                                                 // Cube Custom Tick system
        }

        EDirection vd = EDirection.NONE;
        float alpha = Map.I.TicTacVisualIndicator.color.a - Time.deltaTime;
        if( alpha < 0 ) alpha = 0;
        Map.I.TicTacVisualIndicator.color = new Color( 1, 1, 1, alpha );
        Map.I.TicTacVisualIndicator.gameObject.SetActive( true );

        if( G.HS.TickTimeCounter >= step )
        {
            G.HS.TickTimeCounter -= step;
            G.HS.CurrentTickNumber++;
            MoveTickDone = true;
            if( G.HS.CurrentTickNumber > 4 ) G.HS.CurrentTickNumber = 1;
            if( G.HS.CurrentTickNumber == 1 )
            {
                MasterAudio.PlaySound3DAtVector3( "Wasp Timer Go", G.Hero.transform.position, .4f );       // Plays main tick SFX
                G.HS.TicTacMoveCount = 1;
            }
            else
            {
                MasterAudio.PlaySound3DAtVector3( "Wasp Timer Tick", G.Hero.transform.position, .5f );     // Plays tick SFX
            }

            if( G.HS.CurrentTickNumber == 1 ) vd = EDirection.N;
            if( G.HS.CurrentTickNumber == 2 ) vd = EDirection.E;
            if( G.HS.CurrentTickNumber == 3 ) vd = EDirection.S;
            if( G.HS.CurrentTickNumber == 4 ) vd = EDirection.W;

            Map.I.TicTacVisualIndicator.color = new Color( 1, 1, 1, .5f );
            Map.I.TicTacVisualIndicator.transform.eulerAngles = Util.GetRotationAngleVector( vd );
        }
    }

    public bool OxygenStop()
    {
        if( Map.I.IsPaused() )
            if( Unit.TileID != ETileType.DRAGON1 )
                return true;
        return false;
    }

    public bool UpdateFireBallCreation( bool apply, Vector2 from, Vector2 to )
    {
        Unit un = Map.I.GetUnit( ETileType.ITEM, to );
        if( un == null ) return false;
        if( un.Variation != ( int ) ItemType.Res_Fire_Staff ) return false;
        EDirection dr = Util.GetTargetUnitDir( from, to );
        Vector2 tg = to + Manager.I.U.DirCord[ ( int ) dr ];
        Item it = G.GIT( un.Variation );
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;
        if( Map.I.CheckArrowBlockFromTo( to, tg, G.Hero ) == true ) return false;
        if( Map.IsWall( tg ) ) return false;
        if( Map.CheckLeverCrossingBlock( to, tg ) ) return false;
        if( Map.DoesLeverBlockMe( tg, un ) ) return false;

        if( apply )                                                                                  // Creates the Fireball
        {
            Unit aux = Map.I.SpawnFlyingUnit( tg, ELayerType.MONSTER, ETileType.PROJECTILE, null );
            aux.Control.ProjectileType = EProjectileType.FIREBALL;
            aux.Control.ControlType = EControlType.FIREBALL;
            aux.Spr.spriteId = 212;
            aux.RangedAttack.TargettingType = ETargettingType.FIREBALL;
            Util.SetActiveRecursively( aux.Body.EffectList[ 0 ].gameObject, true );
            aux.Body.EffectList[ 1 ].gameObject.SetActive( true );
            aux.Body.EffectList[ 1 ].transform.localPosition = new Vector3( 0, 0, -2.8f );
            MasterAudio.PlaySound3DAtVector3( "Fire Ignite", Map.I.Hero.Pos );
        }
        return true;
    }

    public void StopForcedMovement()
    {
        ForcedFrontalMovementFactor = 0;
        ForcedFrontalMovementDir = EDirection.NONE;
        Spell.RemoveAllSpellsOfType( ESpiderBabyType.MUSHROOM_POTION, Unit );
        MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );
        Vector3 pun = Util.GetTargetUnitVector( Unit.Pos, Unit.GetFront() );
        iTween.PunchPosition( Unit.Graphic.gameObject, pun * -1.2f, 1.0f );
        Unit.UpdateColor();
    }
    #region Projectile
    public void UpdateProjectileMovement()
    {
        if( Unit.Body.IsDead ) return;
        Vector3 add = Vector3.zero;
        if( Unit.Variation == 0 )                                                   // Shoot to FIXED Direction, fly straight
        {
            add = Unit.Spr.transform.up * Time.deltaTime * FlyingSpeed;
            Unit.transform.position += add;
        }
        else
            if( Unit.Variation == 1 )                                                   // Shoot towards Hero Direction, fly straight
            {
                add = Unit.Spr.transform.up * Time.deltaTime * FlyingSpeed;
                Unit.transform.position += add;
            }
            else
                if( Unit.Variation == 2 )                                                  // Shoot towards Hero Direction, fly towards hero
                {
                    Quaternion qn = Util.GetRotationToPoint( Unit.Spr.transform.position, G.Hero.Spr.transform.position );
                    Unit.Spr.transform.rotation = Quaternion.RotateTowards(
                    Unit.Spr.transform.rotation, qn, FlyingRotationSpeed * Time.deltaTime );
                    add = Unit.Spr.transform.up * Time.deltaTime * FlyingSpeed;
                    Unit.transform.position += add;
                }
                else
                    if( Unit.Variation == 3 )                                                  // Zig zag TBD
                    {
                        Quaternion qn = Util.GetRotationToPoint( Unit.Spr.transform.position, G.Hero.Spr.transform.position );
                        Unit.Spr.transform.rotation = Quaternion.RotateTowards(
                        Unit.Spr.transform.rotation, qn, FlyingRotationSpeed * Time.deltaTime );
                        add = Unit.Spr.transform.up * Time.deltaTime * FlyingSpeed;

                        float fact = 2;
                        float res = -1 + Mathf.PingPong( ( SideFlightZigZagPhase + Time.time ) * fact, 2 );
                        float sidespd = res * 10;
                        add += -Unit.Spr.transform.right * sidespd * Time.deltaTime;

                        Unit.transform.position += add;
                    }

        Attack att = Unit.Control.Mother.MeleeAttack;
        if( Unit.Control.Mother.TileID == ETileType.DRAGON1 )
            att = Unit.Control.Mother.RangedAttack;
        if( CheckHeroHit( .3f, att, false ) )                                                                               // Check Hero hit!                                                                                      
            goto kill;

        UpdateFlyingTile();                                                                                                 // Update Flying tile

        KillProj = false;
        int posx = 0, posy = 0;
        Map.I.Tilemap.GetTileAtPosition( Unit.transform.position, out posx, out posy );

        Unit ga = Map.I.GetUnit( new Vector2( posx, posy ), ELayerType.GAIA );                                               // Gaia block
        if( ga && ga.BlockMissile && Mother.Pos != Unit.Pos )
            if( Vector2.Distance( Unit.Control.Mother.Pos, Unit.transform.position ) >= .4f )                                     // mother distance
                goto kill;

        if( Sector.GetPosSectorType( Unit.Pos ) == Sector.ESectorType.GATES ) goto kill;                                    // out of map
        if( Sector.GetPosSector( Unit.Pos ) != Sector.GetPosSector( G.Hero.Pos ) ) goto kill;

        Unit fire = Map.I.GetUnit( ETileType.FIRE, Unit.Pos );                                                              // Ignites projectile 
        if( fire && fire.Body.FireIsOn )
        {
            Util.SetActiveRecursively( Unit.Body.EffectList[ 0 ].gameObject, true );
            Unit.Body.FireIsOn = true;
            MasterAudio.PlaySound3DAtVector3( "Fire Ignite", Map.I.Hero.Pos );                                               // fx
        }

        int range = 1;                                                                                                       //Check projectile collision
        for( int y = posy - range; y <= posy + range; y++ )
        for( int x = posx - range; x <= posx + range; x++ )
        {
            CheckProjectileColision( new Vector2( x, y ) );
            if( KillProj ) goto kill;
        }

        if( KillProj == false ) return;

        kill:                                                                                                                 // Destroy projectile
        {
            if( Map.I.KillList.Contains( Unit ) == false )
                if( Map.I.ForceHideVegetation == false )
                {
                    Map.I.CreateExplosionFX( Unit.transform.position );
                }
            Map.TimeKill( Unit, 3.5f );
            Unit.Spr.color = new Color( 0, 0, 0, 0 );
            return;
        }
    }
    public void UpdateFlyingPushableMovement()                                                                                          // use this control enum for flying objects that can be mup pushed like fishing pole
    {
        if( FlyingTarget == new Vector2( -1, -1 ) ) return;
        float moveSpeed = 5f;
        Vector3 tg = FlyingTarget;
        Vector3 add = new Vector3( 0, 0, -0.27f );
        if( Unit.Body.MineType == EMineType.HOOK ) tg += add;
        Unit.transform.position = Vector3.MoveTowards( Unit.transform.position, tg, Time.deltaTime * moveSpeed );
        UpdateFlyingTile();
        if( Unit.Body.MineType == EMineType.HOOK )
        if( new Vector2( Unit.Graphic.transform.position.x, Unit.Graphic.transform.position.y ) == FlyingTarget )
        {
            for( int j = 0; j < Unit.Body.RopeConnectFather.Count; j++ )                                                               // smooth the chain after arrival
                Unit.Body.RopeConnectFather[ j ].Body.Rope.AutoCalculateAmountOfNodes = true;
            FlyingTarget = new Vector2( -1, -1 );
        }
        else
        for( int j = 0; j < Unit.Body.RopeConnectFather.Count; j++ )
        {
            Unit.Body.RopeConnectFather[ j ].Body.RopeDestination.transform.position = Unit.Graphic.transform.position + add;          // Adjust chain size while flying
            float dst = Vector2.Distance( Unit.Graphic.transform.position, Unit.Body.RopeConnectFather[ j ].Pos );
            Unit.Body.RopeConnectFather[ j ].Body.Rope.AmountOfNodes = ( int ) ( dst * 3.0f );
        }
    }

    public static bool KillProj = false;
    public bool CheckProjectileColision( Vector2 tg )
    {
        if( tg == G.Hero.GetFront() )                                                                                  // Sword Destroys projectile
            if( Map.I.IsHeroMeleeAvailable() )
                if( CheckProjDist( Unit.transform.position, G.Hero.GetFront() ) )
                {
                    KillProj = true;
                    return true;
                }

        if( Util.Manhattan( G.Hero.Pos, Unit.Pos ) <= 2 )
            for( int i = 0; i < Map.I.RM.HeroSector.ActiveHeroShield.Count; i++ )
                if( Map.I.RM.HeroSector.ActiveHeroShield[ i ] )                                                               // Hero shield collision
                {
                    if( CheckProjDist( Unit.transform.position, Map.I.HeroShieldSpriteList[ i ].transform.position ) )
                        Body.UseShield( i, Unit );
                }

        if( KillProj ) return true;
        Unit mn = Map.I.GetUnit( tg, ELayerType.MONSTER );
        if( mn && mn.ValidMonster == false )                                                                           // Boulder hit  
            if( mn.TileID == ETileType.BOULDER )
                mn.Control.CheckProjDist( Unit );

        if( KillProj ) return true;
        if( Unit.Body.FireIsOn )
        {
            Unit orb = Map.I.GetUnit( ETileType.ORB, tg );                                                            // Burning Projectile strikes the orb
            if( orb && Unit.Body.FireIsOn )
                if( orb.Control.CheckProjDist( Unit ) )
                {
                    orb.StrikeTheOrb( Unit );
                    return true;
                }

            if( mn && mn.ValidMonster )                                                                                // Burning projectile hits monter
                if( mn.Control.CheckProjDist( Unit ) )
                {
                    mn.Body.ReceiveDamage( 10, EDamageType.BLEEDING, Map.I.Hero, Unit.Control.Mother.MeleeAttack );
                    return true;
                }
        }

        List<Unit> fl = Map.I.GetFUnit( tg );                                                                          // Flying Units
        if( fl != null )
            for( int i = 0; i < fl.Count; i++ )
            {
                if( fl[ i ].TileID == ETileType.MINE ||
                    fl[ i ].ValidMonster ||
                    fl[ i ].ProjType( EProjectileType.BOOMERANG ) ||
                    fl[ i ].ProjType( EProjectileType.FIREBALL ) )
                {
                    fl[ i ].Control.CheckProjDist( Unit );                                                                 // Distance check
                    if( KillProj )
                    {
                        if( Unit.Control.Mother == fl[ i ] )                                                               // Projectile does not destroy mother caster
                            if( fl[ i ].ValidMonster )
                                KillProj = false;

                        if( Unit.Control.Mother.TileID != ETileType.TOWER )                                                // To avoid monsters shooting themselves
                            KillProj = false;

                        if( fl[ i ].ValidMonster )
                            if( Unit.Body.FireIsOn )
                                if( KillProj )
                                {
                                    fl[ i ].Body.ReceiveDamage( 10, EDamageType.BLEEDING,                                           // Burning projectile hits Flying monster
                                    Map.I.Hero, Unit.Control.Mother.MeleeAttack );
                                }
                        return true;
                    }
                }
            }
        return false;
    }

    public bool CheckProjDist( Unit proj, float min = 0.5f )
    {
        float curdist = Vector2.Distance( Unit.Spr.transform.position, proj.transform.position );
        if( curdist <= min )
        {
            KillProj = true;
            return true;
        }
        return false;
    }
    public static bool CheckProjDist( Vector2 p1, Vector2 p2, float min = 0.5f )
    {
        float curdist = Vector2.Distance( p1, p2 );
        if( curdist <= min )
        {
            KillProj = true;
            return true;
        }
        return false;
    }
    public void UpdateTowerRotation( bool force = false )
    {
        if( Unit.Variation == 1 || Unit.Variation == 2 )
            if( Unit.MeleeAttack.SpeedTimeCounter > .4f || force )
            {
                Quaternion qu = Util.GetRotationToPoint( Unit.Pos, G.Hero.Pos );
                Unit.Spr.transform.rotation = Quaternion.RotateTowards(
                Unit.Spr.transform.rotation, qu, 100 );
            }
    }
    public void UpdateIronBallMovement()
    {
        if( FlyingTarget.x == -1 )
        {
            FlyingTarget = Unit.GetFront();
            if( Unit.Variation == 0 ) Unit.Body.Sprite2.transform.localEulerAngles = new Vector3( 0, 0, -90 );                        // Init side indicator
            if( Unit.Variation == 1 ) Unit.Body.Sprite2.transform.localEulerAngles = new Vector3( 0, 0, 90 );
            Unit.Body.Sprite2.gameObject.SetActive( true );
        }

        Vector3 old = Unit.transform.position;
        Unit.transform.position = Vector3.MoveTowards( Unit.transform.position, FlyingTarget, Time.deltaTime * FlyingSpeed );        // Translate Ball
        UpdateFlyingTile();

        if( TileChanged )
            Map.I.CreateExplosionFX( old, "Smoke Cloud", "" );                                                                       // FX

        bool res = CheckFlyingTargetReached( .00001f );                                                                              // Check Target Reached

        if( res )
        {
            int sig = 2;
            if( Unit.Variation == 1 ) sig = -2;                                                                                      // Clockwise or not?

            EDirection inv = Util.GetInvDir( Unit.Dir );
            EDirection dr1 = Util.GetRelativeDirection( -sig, inv );
            Vector2 tg1 = Unit.Pos + Manager.I.U.DirCord[ ( int ) dr1 ];
            EDirection dr2 = Util.GetRelativeDirection( -sig, inv );
            Vector2 tg2 = Unit.Pos + Manager.I.U.DirCord[ ( int ) dr2 ];

            bool side = Map.IsWall( tg1 );
            if( Map.IsWall( Unit.GetFront() ) && side == true )                                                                     // rotate anti original side
            {
                Unit.RotateTo( Util.RotateDir( ( int ) Unit.Dir, -sig ) );
                FlyingTarget = Unit.GetFront();
            }

            if( side == false )
            {
                Unit.RotateTo( Util.RotateDir( ( int ) Unit.Dir, sig ) );                                                           // rotate pro original side
                FlyingTarget = Unit.GetFront();
            }

            if( Map.IsWall( Unit.GetFront() ) )
                if( Map.IsWall( tg1 ) && Map.IsWall( tg2 ) )
                {
                    Unit.RotateTo( inv );                                                                                           // Dead End: Invert direction
                    FlyingTarget = Unit.GetFront();
                }
        }
        Util.SmoothRotate( Unit.Spr.transform, Unit.Dir, 18 );
        CheckHeroHit( .4f, Unit.RangedAttack, false );                                                                              // Check hero hit
    }

    public void UpdateBouncingIronBallMovement()
    {
        if( G.HS.CubeFrameCount == 1 || GS.LoadFrameCount == 1 ) 
        {
            Vector3 a = Manager.I.U.DirCord[ ( int ) Unit.Dir ] * -.5f;                                                     // init position at the tile border for synch precision
            Unit.transform.position = new Vector3(
            Unit.Pos.x + a.x, Unit.Pos.y + a.y, Unit.transform.position.z );
            Unit.Spr.spriteId = 433;
            if( Unit.Variation == 1 )
                Unit.Spr.spriteId = 435;
        }

        Vector3 add = Manager.I.U.DirCord[ ( int ) Unit.Dir ];
        float moveSpeed = Util.Percent( FlightSpeedFactor, FlyingSpeed );                                                   // speed factor
        add *= Time.deltaTime * moveSpeed;
        Vector2 old = Unit.transform.position;
        Unit.transform.Translate( add );                                                                                    // Translate ball

        if( Unit.Variation == 0 )
            Unit.Spr.transform.Rotate( 0f, 0f, -720 * Time.deltaTime );                                                     // Rotation animation

        bool res = CheckHeroHit( .4f, Unit.RangedAttack, false, "Drill" );                                                  // Check hero hit

        if( res )
        if( Unit.Variation == 1 )
        {
            Vector2 vec = Manager.I.U.DirCord[ ( int ) Unit.Dir ];
            bool m = G.Hero.CanMoveFromTo( true, G.Hero.Pos, G.Hero.Pos + vec, G.Hero );                                    // Round ball pushes hero
            if( m == false ) Map.I.SetHeroDeathTimer( .001f );
        }

        UpdateFlyingTile();                                                                                                 // Update Flying tile

        bool bounce = false;
        if( TileChanged )
        {
            int posx = 0, posy = 0;
            Map.I.Tilemap.GetTileAtPosition( Unit.transform.position, out posx, out posy );
            Unit ga = Map.I.GetUnit( new Vector2( posx, posy ), ELayerType.GAIA );                                          // Gaia block
            if( ga && ga.BlockMissile )
                bounce = true;
            if( Sector.GetPosSectorType( Unit.Pos ) == Sector.ESectorType.GATES ) bounce = true;                            // out of map
            if( Sector.GetPosSector( Unit.Pos ) != Sector.GetPosSector( G.Hero.Pos ) ) bounce = true;
            if( Map.GFU( ETileType.MINE, new Vector2( posx, posy ) ) ) bounce = true;
            Unit mn = Map.I.GetUnit( new Vector2( posx, posy ), ELayerType.MONSTER );                                       // Monster block
            if( mn && mn.TileID == ETileType.ORB ) bounce = true;
        }

        if( bounce )                                                                                                        // Bounce ball
        {
            Unit.RotateTo( Util.GetInvDir( Unit.Dir ) );
            Unit.transform.position = Util.ReflectPosition( old, Unit.transform.position );                                 // Calculate math for perfect bounce with no discompass
            UpdateFlyingTile();
            Map.I.CreateExplosionFX( old );                                                                                   // FX
        }
    }
    private bool CheckHeroHit( float tgdist, Attack att, bool death, string snd = "" )
    {
        if( G.HS.CubeTurnCount < 2 ) return false;                                                                             // to avoid initial quicktravel kill
        if( G.Hero.Body.InvulnerabilityFactor > 0 ) return false;
        float dist = Vector2.Distance( G.Hero.Spr.transform.position, Unit.transform.position );
        if( dist <= tgdist )                                                                                                   // Hero hit!
        {
            if( Unit.TileID == ETileType.BOUNCING_BALL )                                                                       // Round Pushing ball
            if( Unit.Variation == 1 )
                return true;
            G.Hero.Body.ReceiveDamage( att.BaseDamage, EDamageType.BLEEDING, Unit, att );                                      // Deduct damage        
            G.Hero.Body.InvulnerabilityFactor = .5f;
            if( death ) Map.I.StartCubeDeath();
            Body.CreateDeathFXAt( G.Hero.Spr.transform.position );                                                             // Fx
            if( snd != "" )
                MasterAudio.PlaySound3DAtVector3( snd, G.Hero.Pos );                                                           // Sound FX
            return true;
        }
        return false;
    }
    #endregion

    private bool UpdateFanMovement( Vector2 from, Vector2 to )
    {
        if( Map.I.FanCount < 1 ) return false;
        int count = 0;
        int alig = 0;
        Vector2 desl = Vector2.zero;
        for( int y = ( int ) G.HS.Area.yMin - 1; y < G.HS.Area.yMax + 1; y++ )
            for( int x = ( int ) G.HS.Area.xMin - 1; x < G.HS.Area.xMax + 1; x++ )
            {
                Unit fan = Map.I.GetUnit( ETileType.FAN, new Vector2( x, y ) );
                if( fan && fan.Activated )                                                                                             // a fan has been found
                {
                    desl += Manager.I.U.DirCord[ ( int ) fan.Dir ];
                    if( fan.Variation == 0 )
                    {
                        alig++;                                                                                                        // number of hero aligned fans
                        fan.Activated = false;
                        fan.Body.EffectList[ 0 ].SetActive( false );
                    }
                    count++;
                    if( count == 1 )
                        MasterAudio.StopAllOfSound( "Fan Loop" );
                    Map.I.CreateExplosionFX( new Vector2( x, y ), "Smoke Cloud", "" );                                                 // Smoke Cloud FX
                }
            }
        if( alig < 1 ) return false;
        if( desl != Vector2.zero )
        {
            Vector2 tg = to + desl;
            bool res = false;
            Vector2 frpush = tg - ( to - from );
            if( from == to )                                                      
                frpush = to;

            UpdateMudObjectPush( true, frpush, tg );                                                                                // Mud Object push

            if( Sector.GetPosSector( tg ) == G.HS )                                                                                 // Avoid cube jumping
                res = G.Hero.CanMoveFromTo( true, frpush, tg, G.Hero );

            if( res )
            {
                if( from != to )
                    G.Hero.Control.AnimationOrigin = from + desl;                                                                  // positional animation
            }
            else
            {
                if( Map.LowTerrainBlock( tg ) )
                {
                    G.Hero.Control.ApplyMove( from, tg );                                                  // water death
                    Map.I.SetHeroDeathTimer( .1f, false );
                }
                else
                {
                    CreateMagicEffect( tg );                                                               // Create Magic effect
                    MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );                             // play error sound FX
                }
            }
            UpdateFanActivation();
            Color c1 = Color.red; Color c2 = Color.blue;
            //if( from == to ) c1 = Color.blue;
            Map.I.LineEffect( frpush, tg, .8f, .6f, c1, c1 );                                             // Travel Line FX
            Map.I.LineEffect( from, frpush, .8f, .6f, c2, c2 );
            return true;
        }
        return false;
    }

    public void UpdateFanActivation()
    {
        if( Map.I.FanCount < 1 ) return;
        for( int i = 0; i < 8; i++ )
            for( int ds = 1; ds < Sector.TSX; ds++ )
            {
                Vector2 tg = Unit.Pos + Manager.I.U.DirCord[ i ] * ds;
                if( Map.IsWall( tg ) ) break;
                Unit fan = Map.I.GetUnit( ETileType.FAN, tg );
                if( fan )
                if( fan.Dir == ( EDirection ) Util.GetInvDir( ( EDirection ) i ) )
                    {
                        fan.Activated = true;                                                                             // activate fan
                        fan.Body.EffectList[ 0 ].SetActive( true );
                        fan.Variation = 0;                                                                                // set variation to 0 since its aligned to hero
                        MasterAudio.PlaySound3DAtVector3( "Fan Loop", tg );                                               // Error Sound FX
                        Map.I.CreateExplosionFX( tg, "Smoke Cloud", "" );                                                 // Smoke Cloud FX
                    }
            }
    }
}
