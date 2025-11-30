using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;
using PathologicalGames;
using System.Linq;
using DigitalRuby.LightningBolt;


//Todo List before next release

// Check false areas bug
// remove guga version 
// Check for cheats

// food fish
// VCR points
// credit: pbmojART for runestone
// artisticdude for fire noise
// StumpyStrust for wodden perk back
// Muska666 for cashier soundfx

public enum ESpriteCol
{
    NONE = -1, MAP_PLAY, MONSTER_ANIM, ITEM
}

[System.Serializable]
public partial class Map : MonoBehaviour
{
    #region Variables
    public static Map I;
    [TabGroup( "Main" )]
    public Unit[] SharedObjectsList;
    [TabGroup( "Main" )]
    [HideLabel, ReadOnly]
    public int TotalCubeCount = 0;
    [TabGroup( "Main" )]
    public float TransUpdateStatRadius = 10;
    [TabGroup( "Main" )]
    public float TransUpdateStatRadiusPerTurn = 5, TransUpdateStatRadiusCount = 0, ProcessedTransCount;
    [TabGroup( "Main" )]
    public int TotalSecrets;
    [TabGroup( "Link" )]
    public tk2dTileMap Tilemap, TransTileMap;
    [TabGroup( "Link" )]
    public RandomMap RM;
    [TabGroup( "Link" )]
    public NavigationMap NavigationMap;
    [TabGroup( "Link" )]
    public Farm Farm;
    [TabGroup( "Link" )]
    public tk2dSpriteCollectionData[] SpriteCollectionList;
    [TabGroup( "Link" )]
    public tk2dSprite TileSelection, WindRose, HeadShotGhost, HeroTargetSprite, HeroGrabWallSprite;
    [TabGroup( "Link" )]
    public List<AdventureUpgradeInfo> GlobalTechList;
    [TabGroup( "Link" )]
    public Droplets Droplets;
    [TabGroup( "Link" )]
    public TechButton[ ] TB;
    [TabGroup( "Link" )]
    public tk2dTextMesh[,] TileText;
    [TabGroup( "Link" )]
    public List<ItemType> TechEditorItemList;
    [TabGroup( "Link" )]
    public GameObject MonsterUnitsFolder, GaiaUnitsFolder, HeroSpells,
        AreasFolder, GarbageFolder, ForestObj, WaterObj, PoolingFolder, DecorLayer1, DecorLayer2;
    [TabGroup( "Link" )]
    public ParticleSystem SnowParticle, RainParticle, DropsParticle, WindParticle, FireFliesParticle, FogParticle, LavaParticle;
    [TabGroup( "Link" )]
    public ParticleSystemRenderer RainRenderer;
    [TabGroup( "Link" )]
    public Unit Hero, AuxMine;
    [TabGroup( "int" )]
    public int TotalAreas, Mtx, Mty, DebugPage, CurrentArtifactSeekID, CurrentAreaSeekID, ConsecutivePlatformSteps, SessionFrameCount;
    [TabGroup( "enum" )]
    public EHeroID SelectedHero;
    [TabGroup( "float" )]
    public float SessionTime, RecordTime, FirstCubeDiscoveredTime, ShadowStrength, MonstersTotHpSum, MonstersHpSum,
                 ErrorMessageTimer, HeroEnterAreaHP, HeroAttackSpeedBonus, HeroDeathTimer, TurnTime, RaftDragTurnTime, 
                 ZoomPercent = 100, FreeCamAreaZoom = 0, Tick = 10, TiltRotationTimer, KickTimer, TransInitTime; 
    [TabGroup( "Link" )]
    public Unit[,] Unit, Gaia, Gaia2;
    [TabGroup( "Link" )]
    public List<Unit>[ , ] FUnit;
    [TabGroup( "Link" )]
    public bool[,] Revealed, TransInit;
    [TabGroup( "List" )]
    public float[,] RevealFactor;
    [TabGroup( "List" )]
    public int[,] ObjectID, AreaID, WaspDist, PondID, ContinuousPondID, VineID, GateID;
    [TabGroup( "List" )]
    public List<Vector2> ValidWaspTargetList;
    [TabGroup( "List" )]
    public Unit[] HeroList;
    [TabGroup( "List" )]
    public float[,] DistFromTarget;
    [TabGroup( "str" )]
    public Rect Viewport, CamArea;
    readonly float TileSize = 64;
    [TabGroup( "int" )]
    public int LevelTurnCount, ZoomMode, AreaZoomLevel, NumWoundedMonsters, RaftReverseCount, FanCount,
        AreaExitTurnCount, TicTacMonsterCount, SpiderCount, MineCount, FireBallCount, SessionChestsOpenCount, CurrentVineSelection, TurnFrameCount, AutoOpenGateStep = 0;
    [TabGroup( "bool" )]
    public bool AdvanceTurn, ValidateMove, HeroIsDead, AreaCleared,
                FreeCamMode, ShowGrid, PlatformUpdated, RestartOnRepeat,
                SecondInitializationDone, DraggingFromHero, MonstersMoved, ZoomKeyPressed, OxygenActive, FinalizeCube = false, OldSteppingMode = false;
    [TabGroup( "Vec" )]
    public Vector2 MouseCord, LockAiTarget;
    [TabGroup( "enum" )]
    public EDirection HeroEnterDir;
    [TabGroup( "Vec" )]
    public Vector2 AreaOffset, HeroEnterPos, AreaEnterFromTile;
    [TabGroup( "enum" )]
    public EHeroID HeroEnterID;
    [TabGroup( "enum" )]
    public Mod DefaultButcherMod;
    [TabGroup( "Link" )]
    public PhysicsMaterial2D[ ] HeroPhysicsMaterial;
    [TabGroup( "Link" )]
    public Material WaterMaterial;
    [TabGroup( "Link" )]
    public GameObject WaterFolder;
    [TabGroup( "Link" )]
    public Texture2D BaseWaterTexture;       // para usar como _MainTex
    [TabGroup( "Link" )]
    public Texture2D WaterTexture;
    [TabGroup( "Link" )]
    public LightMaskGenerator Lights;
    [TabGroup( "Link" )]
    public CircleCollider2D HeroCircleCollider;
    [TabGroup( "Link" )]
    public BoxCollider2D HeroSwordCollider;
    [TabGroup( "int" )]
    public int CurrentArea = -1;
    [TabGroup( "Vec" )]
    public Vector2 P1, P2;        // Active Area bound points
    [TabGroup( "Link" )]
    public CameraData CData;
    [TabGroup( "int" )]
    public int CamDataID = -1;
    [TabGroup( "bool" )]
    public bool RecordedMovementAvailable, LevelClear, CamAreaStepped;
    [TabGroup( "Link" )]
    public TurnDataCollection TDCol;
    [TabGroup( "List" )]
    public List<TurnDataCollection> SavedTurnData;
    [TabGroup( "string" )]
    public string LastFileSavedName, Deb, HeroText;
    [TabGroup( "Link" )]
    public Area CurArea;
    [TabGroup( "float" )]
    public float TimeKeyPressing, ScrollAnimationTimer, DeathAnimationTimer, OverWaterAttackTimer;
    [TabGroup( "int" )]
    public int TurnsKeyPressing;
    [TabGroup( "Count" )]
    public int NumValidMonsters, NumNonGlueyMonsters, NumMonstersOverArrows, NumRealtimeMonsters, NumSteppingMonsters, NumScorpions, NumBrains;
    public static int TotHeroes = 1;  // Total number of heroes
    public static int TotOri3 = 10;
    public static int TotBarricade = 10;
    public static int TotSplitters = 8;
    public static int TotMod = 4;
    public static int TotKeys = 12;
    public static int TotDecor = 13696;    
    public static bool OrbStruck = false;
    [TabGroup( "float" )]
    public float WaspFastMoveTime = 3, ExtraHeroHP;
    [TabGroup( "bool" )]
    public bool UpdateTilemap, RepeatToLast, Finalizing;
    [TabGroup( "List" )]
    public List<VI> TransTilemapUpdateList;
    [TabGroup( "List" )]
    public List<Vector2> KnightTG, CustomSpeedRaftPos, CustomSpeedFogPos, TunnelCordList;
    [TabGroup( "Link" )]
    public List<Unit> MetaShootDeathList;
    [TabGroup( "List" )]
    public int[] RedPPGateOpenerCount, RedPPGateSwitcherCount, RedPPGateCloserCount, OldRedPPGateOpenerCount, OldRedPPGateSwitcherCount, OldRedPPGateCloserCount,
    GreenPPGateOpenerCount, GreenPPGateSwitcherCount, GreenPPGateCloserCount, OldGreenPPGateOpenerCount, OldGreenPPGateSwitcherCount, OldGreenPPGateCloserCount;
    [TabGroup( "List" )]
    public int[] GreenPPGateOpenerTotalCount, GreenPPGateSwitcherTotalCount, GreenPPGateCloserTotalCount;
    [TabGroup( "List" )]
	public bool[] AvailableHeroesList;
    [TabGroup( "bool" )]
    public bool AreaNeighboring, Create2DMap;
    [TabGroup( "List" )]
    public Vector2[] SwipeTileOrigin, SwipeTileDest, SwipeOrigin, SwipeDest;
    [TabGroup( "Link" )]
    public TouchCameraControl TCamera;
    [TabGroup( "float" )]
    public float SamePosTouchTimeCount, ButtonPressingTimeCount, FPS, AverageFPS, FPSSum, InvalidateInputTimer, RevealedPercent, TunnelPhaseTimer;
    [TabGroup( "Link" )]
    public Unit[] ObjList;
    [TabGroup( "int" )]
    public int NumOptionalRooms, NumWallsDestroyed, NumMonsterSeeingHero, TunnelPhase, FPSSumCount;
    [TabGroup( "Vec" )]
    public Vector2 ClickTileOrigin, PreferedAreaSave, EntrancePushFrom, EntrancePushTo, LevelEntrancePosition, InfectedScarabAttackSource;
    [TabGroup( "Link" )]
    public RandValues RandTable;
    [TabGroup( "List" )]
    public List<Vector2> AreaSaveList;
    [TabGroup( "List" )]
    public Color[] DecorColorList;
    [TabGroup( "List" )]
    public EActionType[] InvertActionList;
    [TabGroup( "Link" )]
    public tk2dSprite MouseRotationIndicator, PlatformExitIndicator, TicTacVisualIndicator, HeroFishingPoleSprite, HeroPickaxeSprite, HeroShieldSprite;
    [TabGroup( "bool" )]
    public bool MouseRotationIndicatorState, CubeDeath, PlatformDeath, 
        ForceUpdateLOSFire, BarricadeDestroyedInTheTurn, LabRevealed, CountRecordTime;
    [TabGroup( "enum" )]
    public EActionType CurrentMoveTrial;
    [TabGroup( "Vec" )]
    public Vector2 BumpTarget;
    [TabGroup( "List" )]
    public List<Vector2> PermaDeathPositionList;
    [TabGroup( "Link" )]
    public Statistics LevelStats, GameStats, SectorStats, AreaStats;
    [TabGroup( "enum" )]
    public EActionType PlatformEntranceMove;
    [TabGroup( "Link" )]
    public Artifact SeekArtifact = null;
    [TabGroup( "List" )]
    public List<Unit> LOSFireList, BoomerangList, KillList, TimeKillList;
    [TabGroup( "List" )]
    public List<int> ElectrifiedFogList, DirtyFogList;
    [TabGroup( "List" )]
    public List<tk2dSprite> HeroShieldSpriteList;
    [TabGroup( "List" )]
    public List<tk2dTiledSprite> HeroBambooSpriteList;
    [TabGroup( "List" )]
    public List<tk2dSprite> HeroSpellSpriteList;
    [TabGroup( "enum" )]
    public EMoveType CurrentMoveTrialType;
    [TabGroup( "Link" )]
    public GameObject AreasTilemapFolder, MovingAreasTilemapFolder;
    [TabGroup( "bool" )]
    public bool MaskMove = false, RaftDirectionChangeAvailable, TransLayerInitialized;
    [TabGroup( "int" )]
    public int RaftTraverseDistance;
    [TabGroup( "enum" )]
    public EDirection LastMaskMoveDir = EDirection.NONE;
    [TabGroup( "float" )]
    public float MouseStoppedTime = 0, CloverResortTimer = 0;
    [TabGroup( "Vec" )]
    public Vector3 OldMousePosition = Vector3.zero;
    [TabGroup( "float" )]
    public float PlatformAttackSpeedBonus = 0;
    [TabGroup( "Vec" )]
    public Vector2 LastLOSPos;
    [TabGroup( "List" )]
    public List<Vector2> RestPos = new List<Vector2>();
    [TabGroup( "List" )]
    public List<int> RestID = new List<int>();
    [TabGroup( "enum" )]
    public EHerbColor MatchHerbType = EHerbColor.NONE;
    [TabGroup( "List" )]
    public List<Unit> HerbMatchList = new List<Unit>();
    [TabGroup( "List" )]
    public int[ , ] MudPoolID;
    [TabGroup( "List" )]
    List<Unit> shuffleList = new List<Unit>();
    [TabGroup( "bool" )]
    public bool HideVegetation = false, ForceHideVegetation = false, DisabledForce = false, ForceUpdateTrans = false;
    [TabGroup( "float" )]
    public float HideVegetationTimer = 0;
    [TabGroup( "int" )]
    public int ObjectCameraMod = -1;

    #endregion
    void Start () 
    {
		I = this;
		ShowGrid = false;
        RandTable.InitRandTable();
        Map.I.Farm.UniqueID = "";
        Map.I.ZoomMode = RM.RMD.DefaultZoom;
 	}

    public void Save()
    {
        GS.W.Write( ( int ) PlatformEntranceMove );
        GS.W.Write( SessionFrameCount );
        GS.W.Write( ConsecutivePlatformSteps );
        GS.W.Write( SessionTime );
        GS.W.Write( RecordTime );
        GS.W.Write( FirstCubeDiscoveredTime );
        GS.W.Write( HeroAttackSpeedBonus );
        GS.W.Write( HeroDeathTimer );
        GS.W.Write( LevelTurnCount );
        GS.W.Write( ZoomMode );
        GS.W.Write( RaftReverseCount );
        GS.W.Write( ( int ) CurrentMoveTrial );
        GS.W.Write( ( int ) CurrentMoveTrialType );
        GS.W.Write( NumValidMonsters );
        GS.W.Write( NumNonGlueyMonsters );
        GS.W.Write( NumMonstersOverArrows );
        GS.W.Write( NumRealtimeMonsters );
        GS.W.Write( NumSteppingMonsters );
        GS.W.Write( CurrentVineSelection );
        GS.W.Write( TurnFrameCount );
        GS.W.Write( FinalizeCube );
        GS.W.Write( OldSteppingMode );
        GS.W.Write( HeroIsDead );

        Water.SaveGlobals();
        Mine.SaveGlobals();
        /*
           #region Variables
    public List<AdventureUpgradeInfo> GlobalTechList;
        NumScorpions
    public EHeroID SelectedHero;
    public float , ShadowStrength, MonstersTotHpSum, MonstersHpSum,
                 ErrorMessageTimer, HeroEnterAreaHP, ;

    public float[,] RevealFactor, RevealChanceFactor;
    public int[,] ObjectID, AreaID, WaspDist, PondID, ContinuousPondID, VineID;
    public List<Vector2> ValidWaspTargetList;
        

    public Rect Viewport, CamArea;
    float TileSize = 64;
    public float aux;
    public int ,  , AreaZoomLevel, NumWoundedMonsters,  , 
        AreaExitTurnCount, TicTacMonsterCount, SpiderCount, MineCount, FireBallCount,
    public bool AdvanceTurn, ValidateMove, , AreaCleared,
                FreeCamMode, ShowGrid, PlatformUpdated, RestartOnRepeat,
                SecondInitializationDone, DraggingFromHero, MonstersMoved, ZoomKeyPressed, OxygenActive;
    public float ZoomPercent = 100;
    public float FreeCamAreaZoom = 0;
    public Vector2 MouseCord, LockAiTarget;
    public EDirection HeroEnterDir;
    public Vector2 AreaOffset, HeroEnterPos, AreaEnterFromTile;
    public EHeroID HeroEnterID;
    public int CurrentArea = -1;
    public Vector2 P1, P2;        // Active Area bound points
    //public List<Unit> MoveOrder;
    public CameraData CData;
    public int CamDataID = -1;
    public bool RecordedMovementAvailable, LevelClear, CamAreaStepped;
    public TurnDataCollection TDCol;
    public List<TurnDataCollection> SavedTurnData;
    public string LastFileSavedName, Deb, HeroText;
    public Area CurArea;
    public float TimeKeyPressing, ScrollAnimationTimer, DeathAnimationTimer, OverWaterAttackTimer;
    public int TurnsKeyPressing;    
    public float WaspFastMoveTime = 3;
    public float ExtraHeroHP;
    public bool UpdateTilemap, RepeatToLast, Finalizing;
    public List<Vector2> TransTilemapUpdateList, KnightTG, CustomSpeedRaftPos, CustomSpeedFogPos;
    public List<Unit> MetaShootDeathList;
    public int[] RedPPGateOpenerCount,   RedPPGateSwitcherCount,   RedPPGateCloserCount;
    public int[] OldRedPPGateOpenerCount,   OldRedPPGateSwitcherCount,   OldRedPPGateCloserCount;
    public int[] GreenPPGateOpenerCount, GreenPPGateSwitcherCount, GreenPPGateCloserCount;
    public int[] OldGreenPPGateOpenerCount, OldGreenPPGateSwitcherCount, OldGreenPPGateCloserCount;
    public int[] GreenPPGateOpenerTotalCount, GreenPPGateSwitcherTotalCount, GreenPPGateCloserTotalCount;
	public bool[] AvailableHeroesList;
    public bool AreaNeighboring, Create2DMap;
    public float TiltRotationTimer;
    public Vector2[] SwipeTileOrigin, SwipeTileDest, SwipeOrigin, SwipeDest;
    public TouchCameraControl TCamera;
    public float SamePosTouchTimeCount, ButtonPressingTimeCount, FPS, InvalidateInputTimer, RevealedPercent;
    public Unit[] ObjList;
    public int NumOptionalRooms, NumWallsDestroyed, NumMonsterSeeingHero;
    public Vector2 ClickTileOrigin, PreferedAreaSave, EntrancePushFrom, EntrancePushTo, LevelEntrancePosition;


    public List<Vector2> AreaSaveList;  
    public bool  MouseRotationIndicatorState, , 
        ForceUpdateLOSFire, BarricadeDestroyedInTheTurn, LabRevealed, CountRecordTime;
    public Vector2 BumpTarget;
    public List<Vector2> PermaDeathPositionList;
    public Statistics LevelStats, GameStats, SectorStats, AreaStats;

    public Artifact SeekArtifact = null;
    public List<Unit> LOSFireList, BoomerangList, KillList;
    public List<int> ElectrifiedFogList, DirtyFogList;
    public List<tk2dSprite> HeroShieldSpriteList;
    public List<tk2dTiledSprite> HeroBambooSpriteList;
    public List<tk2dSprite> HeroSpellSpriteList;
    public int AutoOpenGateStep = 0;
    public  float Tick = 10; // For Realtime speed measurement
    public bool MaskMove = false, RaftDirectionChangeAvailable;
    public int RaftTraverseDistance;
    public EDirection LastMaskMoveDir = EDirection.NONE;
    public float MouseStoppedTime = 0;
    public Vector3 OldMousePosition = Vector3.zero;
    public float PlatformAttackSpeedBonus = 0;
    public Vector2 LastLOSPos;   

    #endregion*/
    }
    
    public void Load()
    {
        PlatformEntranceMove = ( EActionType ) GS.R.ReadInt32();
        SessionFrameCount = GS.R.ReadInt32();
        ConsecutivePlatformSteps = GS.R.ReadInt32();
        SessionTime = GS.R.ReadSingle();
        RecordTime = GS.R.ReadSingle();
        FirstCubeDiscoveredTime = GS.R.ReadSingle();
        HeroAttackSpeedBonus = GS.R.ReadSingle();
        HeroDeathTimer = GS.R.ReadSingle();
        LevelTurnCount = GS.R.ReadInt32();
        ZoomMode = GS.R.ReadInt32();
        RaftReverseCount = GS.R.ReadInt32();
        CurrentMoveTrial = ( EActionType ) GS.R.ReadInt32();
        CurrentMoveTrialType = ( EMoveType ) GS.R.ReadInt32();
        NumValidMonsters = GS.R.ReadInt32();
        NumNonGlueyMonsters = GS.R.ReadInt32();
        NumMonstersOverArrows = GS.R.ReadInt32();
        NumRealtimeMonsters = GS.R.ReadInt32();
        NumSteppingMonsters = GS.R.ReadInt32();
        CurrentVineSelection = GS.R.ReadInt32();
        TurnFrameCount = GS.R.ReadInt32();
        FinalizeCube = GS.R.ReadBoolean();
        OldSteppingMode = GS.R.ReadBoolean();
        HeroIsDead = GS.R.ReadBoolean();
        Water.LoadGlobals();
        Mine.LoadGlobals();
        Controller.UpdatePlatformEntranceIndicator();
        CubeDeath = false;
        PlatformDeath = false;
    }
    
	//______________________________________________________________________________________________________________________ Start map

    public void StartGame()
    {
        InitFolders();
        DebugPage = 0;
        SessionFrameCount = 0;
        HeroIsDead = false;
        Finalizing = false;
        Unit = new Unit[ Tilemap.width, Tilemap.height ];
        FUnit = new List<Unit>[ Tilemap.width, Tilemap.height ];
        Gaia = new Unit[ Tilemap.width, Tilemap.height ];
        Gaia2 = new Unit[ Tilemap.width, Tilemap.height ];
        TileText = new tk2dTextMesh[ Tilemap.width, Tilemap.height ];
        DistFromTarget = new float[ Tilemap.width, Tilemap.height ];
        ObjectID = new int[ Tilemap.width, Tilemap.height ];
        GateID = new int[ Tilemap.width, Tilemap.height ];
        Revealed = new bool[ Tilemap.width, Tilemap.height ];
        RevealFactor = new float[ Tilemap.width, Tilemap.height ];
        TransInit = new bool[ Tilemap.width, Tilemap.height ];
        LOSFireList = new List<Unit>();
        BoomerangList = new List<Unit>();
        KillList = new List<Unit>();
        TimeKillList = new List<Unit>();
        AvailableHeroesList = new bool[ 5 ]; // Check for bugs when adding heroes
        AvailableHeroesList[ 0 ] = true;
        Lights.SetTempLight( false );
        LevelEntrancePosition = new Vector2( 120, 120 );
        if( Manager.I.GameType == EGameType.FARM )
            Map.I.LevelEntrancePosition = Farm.FarmEntrance;

        TransUpdateStatRadiusCount = TransUpdateStatRadius;
        LevelStats.Reset();
        CreateMapGameObjects();
        InitHero( EHeroID.KADE, true, true, true, EDirection.N );
        BumpTarget = LockAiTarget = new Vector2( -1, -1 );
        TDCol = new TurnDataCollection();
        ForceUpdateLOSFire = false;
        Physics.gravity = new Vector3( 1, -1f, 9.81f );
        UpdateTilemap = true;
        ConsecutivePlatformSteps = 0;
        ProcessedTransCount = 0;
        TurnFrameCount = 0;
        TransInitTime = 0;
        AutoOpenGateStep = 0;
        HeroDeathTimer = -1;
        CurrentVineSelection = -1;
        MonstersMoved = false;
        LabRevealed = false;
        OldSteppingMode = false;
        PondList = new List<PondInfo>();
        GS.IsLoading = false;
        GS.IsSaving = false;
        CloverResortTimer = 2;
        GS.LastSavedCube = null;
        GS.CubeLoaded = false;
        GS.LastLoadedCube = null;
        GS.RestartAvailable = false;
        GS.ForceLoad = false;
        GS.CustomSaveExists = false;
        ForceUpdateTrans = false;
        GS.LoadFrameCount = 0;
        Controller.BarricadeBumpTimeCount = 0;
        Controller.CornPickedAmt = 0;
        Controller.OldCornAmt = 0;
        Controller.CornIsOver = false;
        Controller.RaftMoving = false;
        Controller.HeroBeingPushedByFog = false;
        Controller.FlyingTargetReached = false;
        Controller.MaskMoveFinalized = false;
        Controller.InputVector = Vector2.zero;
        Controller.InputVectorList = new List<Vector3>();
        Controller.PassiveBoomerangAttackTg = null;
        Controller.ForceBoomerangAttack = false;
        Controller.PunchOrigin = new Vector2( -1, -1 );
        Controller.MovedVineList = new List<Unit>();
        Controller.IgnoreLevelPushList = new List<Unit>();
        Controller.HeroWallGrabTg = new Vector2( -1, -1 );
        Mine.LastMessageWasBonus = false;
        Mine.TunnelTraveling = false;
        Mine.ProceedTrip = false;
        Mine.VaultEffTargetList = new List<Vector2>();
        Mine.OldZoom = -1;
        Mine.UpdateChiselText = false;
        Mine.UpdateAllMinesText = false;
        Mine.SafeMining = true;
        ResetFogOfWar( true );
        Quest.I.InitArtifacts();
        UpdateGateAndRaftID( null );
        InvalidateInputTimer = DeathAnimationTimer = 0;
        CubeDeath = false;
        OverWaterAttackTimer = -1;
        HerbMatchList = null;
        AltarBonusStruct.BumpTimeCount = 0;
        Map.I.PlatformEntranceMove = EActionType.NONE;
        Controller.UpdatePlatformEntranceIndicator();
        G.Hero.Body.RigidBody.velocity = Vector2.zero;
        G.Hero.Body.RigidBody.isKinematic = false;
        G.Hero.CircleCollider.enabled = false;
        G.HS = null;
        RestartOnRepeat = true;
        KickTimer = 0;
        GlobalFishingBonusRecord = 0;
        RaftTraverseDistance = 0;
        DraggingFromHero = false;
        PlatformAttackSpeedBonus = 0;
        Controller.CheckHeroSwordBlock = true;
        Controller.CheckingLure = false;
        Controller.BlockStepping = false;
        Controller.RaftStepCount = -1;
        Controller.MudPushing = false;
        Controller.FromBrokenRaft = false;
        MouseRotationIndicatorState = false;
        MouseRotationIndicator.gameObject.SetActive( false );
        PlatformExitIndicator.gameObject.SetActive( false );
        Manager.I.Inventory.gameObject.SetActive( false );
        TrailTiles = null;
        ObjectCameraMod = -1;
        Hero.Body.RopeConnectSon = null;
        Hero.Body.RopeConnectFather = new List<Unit>();
        RepeatToLast = false;
        PreferedAreaSave = AreaEnterFromTile = new Vector2( -1, -1 );
        PermaDeathPositionList = new List<Vector2>();
        SavedTurnData = new List<TurnDataCollection>();
        CamArea = new Rect( 0, 0, 0, 0 );
        CamAreaStepped = false;
        FreeCamAreaZoom = 0;
        TechButton.TechEditorActive = false;

        for( int l = 0; l < Quest.I.LevelList.Length; l++ )
        if( Quest.I.LevelList[ l ] )
        {
            Quest.I.LevelList[ l ].gameObject.SetActive( false );
        }

        for( int l = 0; l < Quest.I.LabList.Length; l++ )
        if( Quest.I.LabList[ l ] )
        {
            Quest.I.LabList[ l ].gameObject.SetActive( false );
            Quest.I.LabList[ l ].Tilemap.gameObject.SetActive( false );
        }

        Quest.I.Dungeon.gameObject.SetActive( false );
        G.Hero.Graphic.gameObject.SetActive( true );

        Tilemap.Layers[ ( int ) ELayerType.GAIA ].gameObject.SetActive( true ); 
        Tilemap.Layers[ ( int ) ELayerType.GAIA2 ].gameObject.SetActive( false );
        Tilemap.Layers[ ( int ) ELayerType.RAFT ].gameObject.SetActive( false );
        Tilemap.Layers[ ( int ) ELayerType.MONSTER ].gameObject.SetActive( false );

        Quest.I.UpdateArtifactData( ref Hero );
        LastFileSavedName = "";
        InitMapVariablesDefaults();
        Manager.I.ForceRestartFromBeginning = false;
        TransLayerInitialized = false;
        Farm.NextMonsterType = ItemType.NONE;
        Farm.GrabActivated = false;
        Farm.NextPosition = new Vector2( -1, 1 );
        Farm.GrabPosition = new Vector2( -1, 1 );
        TotalAreas = 0;
        OxygenActive = false;
        Controller.ForceDiagonalMovement = false;
        Controller.MineSwitching = false;
        Farm.GrabTarget = new Vector2( -1, -1 );
        Hero.Control.PathFinding.Path.Clear();
        Controller.LastResPosCollectedByHeroPos = new Vector2( -1, -1 );
        Controller.MWaspTgList = new List<Vector2>();
        SwipeTileOrigin = new Vector2[] { new Vector2( -1, -1 ), new Vector2( -1, -1 ), new Vector2( -1, -1 ) };
        SwipeTileDest = new Vector2[] { new Vector2( -1, -1 ), new Vector2( -1, -1 ), new Vector2( -1, -1 ) };
        SwipeOrigin = new Vector2[] { new Vector2( -1, -1 ), new Vector2( -1, -1 ), new Vector2( -1, -1 ) };
        SwipeDest = new Vector2[] { new Vector2( -1, -1 ), new Vector2( -1, -1 ), new Vector2( -1, -1 ) };
        LastMaskMoveDir = EDirection.NONE;
        CurrentArtifactSeekID = CurrentAreaSeekID = 0;
        ElectrifiedFogList = new List<int>();
        DirtyFogList = new List<int>();
        SamePosTouchTimeCount = ButtonPressingTimeCount = 0;
        Application.targetFrameRate = 5000;
        ClickTileOrigin = EntrancePushFrom = EntrancePushTo = new Vector2( -1, -1 );
        TurnTime = 0;
        RaftDragTurnTime = 0;
        TunnelPhase = -1;
        TunnelCordList = new List<Vector2>();
        Controller.SnowEnterFrameCount = 0;
        Controller.BumperTimeCount = 100;
        Controller.HeroMovedFromSlidingTerrainToMud = false;
        Controller.MoveTickDone = false;
        Controller.SnowSliding = false;
        Controller.SandSliding = false;
        Controller.ResetMergingID = false;
        Controller.SlideTimeCount = 0;
        Controller.FrontalTargetManeuverDist = 0;
        Controller.AttemptMining = false;
        Controller.IgnoreLeverPush = false;
        Controller.OnlyRudderPush = false;
        ArcherArrowAnimation.Reset();
        Spell.AddSpellOrigin = new Vector2( -1, -1 );
        Spell.AddSpellHeroDir = EDirection.NONE;
        Spell.WeaponType = ItemType.NONE;
        Spell.SpellSlot = 1;
        SpiderCount = MineCount = FireBallCount = FanCount = 0;
        Attack.AttackedByHeroMelee = false;
        Attack.SpiderSquash = false;
        SessionChestsOpenCount = 0;
        Map.I.ZoomMode = RM.RMD.DefaultZoom;
        Item.TempResource = false;
        AreaZoomLevel = 0;
        Create2DMap = false;
        Item.IgnoreMessage = false;
        Item.ForceMessage = false;
        Item.PostMessage = "";
        SecondInitializationDone = false;
        ResourceIndicator.UpdateGrid = true;
        FishingMode = EFishingPhase.NO_FISHING;
        CurrentFishingPole = null;
        LevelClear = false;
        ZoomKeyPressed = false;
        RM.HeroSector = null;
        FinalizeCube = false;
        UpdateFogOfWar( true );
        UpdateAllAreaColors();
        HeroTargetSprite.gameObject.SetActive( false );
        UI.I.InitGame();
        if( Manager.I.GameType == EGameType.CUBES )
            Pathfinder2D.Instance.Create2DMap( true, this, false );
        Hero.LevelTxt.gameObject.SetActive( false );
        Tilemap.Layers[ ( int ) ELayerType.GRID ].gameObject.SetActive( false );
        Tilemap.Layers[ ( int ) ELayerType.AREAS ].gameObject.SetActive( false );
        FishingLine.transform.parent.gameObject.SetActive( false );
        ShowGrid = false;
        MaskMove = false;
        CountRecordTime = false;
        SessionTime = 0;
        RecordTime = 0;
        RaftDirectionChangeAvailable = true;
        FirstCubeDiscoveredTime = 0;

        for( int h = 0; h < HookList.Count; h++ )
        if ( HookList[ h ].gameObject.activeSelf )
             HookList[ h ].gameObject.SetActive( false );
        UI.I.RestartAreaButton.gameObject.SetActive( true );
        UILabel lb = UI.I.RestartAreaButton.GetComponentInChildren<UILabel>();
        if( Manager.I.GameType == EGameType.CUBES )
            lb.text = "Quest \nInfo...";
        else
        if( Manager.I.GameType == EGameType.NAVIGATION )
            lb.text = "Exit Navigation Map...";
        else
        if( Manager.I.GameType == EGameType.FARM )
            lb.text = "Exit Farm...";

        NumOptionalRooms = 0;
        for( int i = 0; i < Quest.I.CurLevel.AreaList.Count; i++ )
        {
            if( Quest.I.CurLevel.AreaList[ i ].Optional )
            {
                NumOptionalRooms++;
            }
            Quest.I.CurLevel.AreaList[ i ].InitArea();
        }
    }

    public void SecondInitialization()
    {
        if( SecondInitializationDone ) return;
       // if( AdvanceTurn == false ) return;
        UpdateRoomDoorID();
        RM.UpdateGatesCost( new Vector2( -1, -1 ) );
        SecondInitializationDone = true;
    }

	public void InitMapVariablesDefaults()
    {
    AreaCleared = false;
	LevelTurnCount = 0;
    AreaExitTurnCount = 0;
    NumRealtimeMonsters = 0;
    NumSteppingMonsters = 0;
    NumScorpions = 0; NumBrains = 0;
    NumWoundedMonsters = 0;
    TicTacMonsterCount = 0;
    HeroAttackSpeedBonus = 0;
	FreeCamMode = false;
    AdvanceTurn = HeroIsDead = false;
	RecordedMovementAvailable = true;
    NumWallsDestroyed = 0;
    TimeKeyPressing               = TurnsKeyPressing = 0;
    RedPPGateOpenerCount          = new int[ TotKeys ];
    RedPPGateSwitcherCount        = new int[ TotKeys ];
    RedPPGateCloserCount          = new int[ TotKeys ];
    OldRedPPGateOpenerCount       = new int[ TotKeys ];
    OldRedPPGateSwitcherCount     = new int[ TotKeys ];
    OldRedPPGateCloserCount       = new int[ TotKeys ];

    GreenPPGateOpenerCount        = new int[ TotKeys ];
    GreenPPGateSwitcherCount      = new int[ TotKeys ];
    GreenPPGateCloserCount        = new int[ TotKeys ];
    OldGreenPPGateOpenerCount     = new int[ TotKeys ];
    OldGreenPPGateSwitcherCount   = new int[ TotKeys ];
    OldGreenPPGateCloserCount     = new int[ TotKeys ];
    GreenPPGateOpenerTotalCount   = new int[ TotKeys ];
    GreenPPGateSwitcherTotalCount = new int[ TotKeys ];
    GreenPPGateCloserTotalCount   = new int[ TotKeys ];

    for( int i = 0; i < TotKeys; i++ )
        {
        RedPPGateOpenerCount        [ i ] = RedPPGateSwitcherCount        [ i ] = RedPPGateCloserCount        [ i ] = 0;
        OldRedPPGateOpenerCount     [ i ] = OldRedPPGateSwitcherCount     [ i ] = OldRedPPGateCloserCount     [ i ] = 0;
        GreenPPGateOpenerCount      [ i ] = GreenPPGateSwitcherCount      [ i ] = GreenPPGateCloserCount      [ i ] = 0;
        OldGreenPPGateOpenerCount   [ i ] = OldGreenPPGateSwitcherCount   [ i ] = OldGreenPPGateCloserCount   [ i ] = 0;
        GreenPPGateOpenerTotalCount [ i ] = GreenPPGateSwitcherTotalCount [ i ] = GreenPPGateCloserTotalCount [ i ] = 0;
        }
    }

	public void InitFolders()
	{
	if( MonsterUnitsFolder != null ) Destroy( MonsterUnitsFolder );
	if( GaiaUnitsFolder != null )    Destroy( GaiaUnitsFolder );

    if( AreasFolder != null ) Destroy( AreasFolder );
    tk2dTileMap tm = null;

    if( Manager.I.GameType == EGameType.NAVIGATION )                                      // Navigation Map
    {
        tm = Map.I.NavigationMap.Tilemap;
        Map.I.NavigationMap.Tilemap.gameObject.SetActive( false );
    }
    else
    {                                                                                     // cubes map
        tm = Quest.I.CurLevel.Tilemap;
        Quest.I.CurLevel.AreaFolder.gameObject.SetActive( true );
        Quest.I.CurLevel.ArtifactFolder.gameObject.SetActive( true );
    }

    Tilemap.width = tm.width;
    Tilemap.height = tm.height;

    CopyTilemap( false, tm, ref Tilemap, new Vector2( 0, 0 ), new Vector2( 0, 0 ), tm.width, tm.height, false, false );

	//Tilemap.Build();  //new opt
    Tilemap.gameObject.SetActive( true );
	Tilemap.transform.position = new Vector3( 0, 0, 0 );
	AreasFolder = new GameObject("Areas");
	AreasFolder.transform.parent = transform;	
	MonsterUnitsFolder = new GameObject("Monster Units");
	MonsterUnitsFolder.transform.parent = transform;
    GaiaUnitsFolder = new GameObject("Gaia Units");
	GaiaUnitsFolder.transform.parent = transform;
	}

	//______________________________________________________________________________________________________________________ Main game Update Function	

	public void UpdateIt () 
	{
		if( this == null    ) return;
		if( Tilemap == null ) return;
        PlatformUpdated = false;
        if( UpdateOverlayAnimation() ) return;
        if( UI.I.UpdateMessageBox() ) return;
        if( RM.UpdateIt() ) return;
        Farm.UpdateIt();

        if( Manager.I.Console.gameObject.activeSelf ) return;
        if( RM.DungeonDialog.gameObject.activeSelf ) return;

        AdvanceTurn = false;

        UpdateDebug();
        UpdateSwipe();
        UpdateTouchMovevent();
        UpdateTouchCamera();        
        UpdateHeroMovement();
        UpdateRealtimeHeroMovement();
        UpdateAllMonsterWakingUp();
        UpdateFire();
        UpdateFireIgnition();
        UpdateLateHeroStuff();
        UpdateMonstersMovement();
        UpdateFishing();
        UpdateRealtimeMonstersMovement();
        FinalizeMonstersMovement();
        UpdateAfterMoveStuff();
        UpdateNumbersData();
        UpdateHerbs();
        UpdatePressurePlates();
        DetectFightingMap();
        UpdateWorldMapUnitsData();
        Hero.Control.UpdateResourceCollecting( Hero.Pos, false );
        Hero.Body.UpdateBleeding();
        UpdateCamera();
        UpdateMessage();
        UpdateFogOfWar();
        UpdateGrid();
        UpdateHelp();
        Helper.I.DrawDebugText();
        UpdatePathfinding();
        UpdateScrollText();
        UpdateMouseHelp();
        UpdateRepeat();
        Lights.UpdateIt();
        SecondInitialization();
        RM.UpdateStatistics();
        Quest.I.ReviveRecurringArtifacts();
        Quest.I.UpdateArtifactPrices();
        NavigationMap.UpdateIt();
        UpdateSandBox();
        Sector.UpdateSectorCleared();
        Secret.InitializeSecrets();
        UpdateProgressiveTransLayerCalculation();
        FinalizeLoop();
        GS.UpdateCubeSaving();
    }

    public void FixedUpdate()
    {
        if( Unit == null ) return;
        if( Map.I.HeroDeathTimer > 0 ) return;
        if( Helper.I.FrameRateLimit > 0 ) 
            Application.targetFrameRate = Helper.I.FrameRateLimit;
        if( G.Hero.Control.UpdateSnowSliding() == false )                                           // On fixed update for Physics not to be affected by FPS 
            G.Hero.Control.UpdateSandSliding();
    }

	//_____________________________________________________________________________________________________________________ Creates a unit at the pos

	public GameObject CreateUnit( Unit prefab, Vector2 tg, ELayerType layer = ELayerType.NONE )
	{
        if( prefab.SharedObject )
       {
           Gaia[ ( int ) tg.x, ( int ) tg.y ] = SharedObjectsList[ ( int ) prefab.TileID ];      
           return null;
       }

        if( prefab == null )
        {
            Debug.LogError( "prefab == null" ); return null;
        }

        if( tg.x != -1 )
		if(!CanCreateUnitHere( prefab.UnitType, tg ) ) return null;
        
        GameObject instance = null;
        Unit un = null;

        Transform tr = PoolManager.Pools[ "Pool" ].Spawn( prefab.PrefabName );
        instance = tr.gameObject;
 
        un = instance.GetComponent<Unit>();

        if( layer == ELayerType.NONE ) layer = GetTileLayer( un.TileID );

		if( PtOnMap( Tilemap, tg ) )
        {
            if( un && un.Control )
            if( un.Control.IsFlyingUnit )
                {
                    if( FUnit[ ( int ) tg.x, ( int ) tg.y ] == null )                   // Adds to flying list
                        FUnit[ ( int ) tg.x, ( int ) tg.y ] = new List<Unit>();
                    FUnit[ ( int ) tg.x, ( int ) tg.y ].Add( un );                      // new, added to allowmine creation over raft
                }

            if( layer == ELayerType.MONSTER )
            {
                if( un.Control && un.Control.IsFlyingUnit == false || GS.IsLoading == false ) // This is to enable saving more than one monster in a single tile key: save2monsters
                    Unit[ ( int ) tg.x, ( int ) tg.y ] = un;
            }
            else
            if( layer == ELayerType.GAIA    ) Gaia [ ( int ) tg.x, ( int ) tg.y ] = un; else
            if( layer == ELayerType.GAIA2   ) Gaia2[ ( int ) tg.x, ( int ) tg.y ] = un; else
            if( layer == ELayerType.RAFT    ) Unit [ ( int ) tg.x, ( int ) tg.y ] = un;
        }

        Unit pfun = prefab.GetComponent<Unit>();
        if( pfun == null ) Debug.LogError("pfun == null");

        un.Copy( pfun, true, true, true );

        un.TileID = pfun.TileID;
		un.Pos = tg;
        un.IniPos = tg;
        un.Variation = GetVariation( tg, GetTileLayer( un.TileID ) );
		//un.name = "" + un.PrefabName + " "  + tg.x + " " + tg.y;     
        un.TileID = GetTileID( un.TileID );
        un.SetVariation( un.Variation );

        if( un.Control )
        {
            un.Control.InitialDirection = un.Dir;
            un.Control.RestLine = null;
        }

        if( un.Body )
        {
            un.Body.Rope = null;
            un.Body.RopeConnectFather = new List<Unit>();
            un.Body.RopeConnectSon = null;
            if( un.Body.ImmunityDome )
                un.Body.ImmunityDome.gameObject.SetActive( false );
        }

        if( un.TileID == ETileType.SLIME )
        {
            un.Control.JumpTarget = new Vector2( -1, -1 );
            un.Spr.transform.rotation = new Quaternion( 0, 0, 0, 0 );
        }

		un.transform.position = new Vector3( tg.x, tg.y, 0 );

        if( un.Spr && pfun.Spr )
            un.Spr.transform.localPosition = pfun.Spr.transform.localPosition;
        //if( layer == ELayerType.GAIA )
        //    un.transform.position = new Vector3( tg.x, tg.y, -1.6f );
        //if( layer == ELayerType.GAIA2 )
        //    un.transform.position = new Vector3( tg.x, tg.y, -1.8f );
        //if( layer == ELayerType.RAFT )
        //    un.transform.position = new Vector3( tg.x, tg.y, -1.7f );

        //if( Revealed[ ( int ) tg.x, ( int ) tg.y ] == false )                      // fog of war
        //{
        //    if( un.LevelTxt ) un.LevelTxt.color = Color.black;
        //    if( un.Spr ) un.Spr.color = Color.black;
        //}
        //else
        //{
        //    if( un.LevelTxt ) un.LevelTxt.color = Color.white;
        //    if( un.Spr ) un.Spr.color = Color.white;
        //}
        if( un.TileID == ETileType.FISHING_POLE )
        {
            un.Activate( true );
            un.Spr.color = Color.white;
            un.Body.Sprite2.gameObject.SetActive( false );
            un.Body.Sprite3.gameObject.SetActive( false );
            un.Body.Sprite4.gameObject.SetActive( false );
        }
        else
        if( un.TileID == ETileType.MUD_SPLASH )
        {
            un.Spr.color = Color.white;
        }
        else
        if( un.TileID == ETileType.ARROW )
        {
            un.Body.SetWorking( true );
        }
        else
        if( un.TileID == ETileType.CHECKPOINT )
        {
            un.gameObject.SetActive( false );
        }
        else
        if( un.TileID == ETileType.SECRET )
        {
            un.Graphic.gameObject.SetActive( false );
            un.Body.Sprite2.gameObject.SetActive( false );
        }
        else
        if( un.TileID == ETileType.DOOR_KNOB    ||
            un.TileID == ETileType.DOOR_OPENER  ||
            un.TileID == ETileType.DOOR_CLOSER  ||
            un.TileID == ETileType.DOOR_SWITCHER )
        {
            un.gameObject.transform.localScale = new Vector3( 1, 1, 1 );
        }
        else
        if( un.TileID == ETileType.FIRE )
        {
            un.Body.EffectList[ 0 ].gameObject.SetActive( false );
            un.Body.WoodAdded = new List<bool>();

            for( int i = 0; i < un.Body.WoodList.Length; i++ )
            {
                un.Body.WoodAdded.Add( false );
                un.Body.WoodList[ i ].gameObject.SetActive( false );
            }
        }
        else
        if( un.TileID == ETileType.ITEM )
        {
            un.Spr.transform.eulerAngles = Vector3.zero;
            un.Spr.scale = new Vector3( 1.2f, 1.2f, 1 );
            un.Body.Sprite2.gameObject.SetActive( false );
            un.Body.Sprite2.scale = new Vector3( 3f, 3f, 1 );
            un.Body.Sprite3.gameObject.SetActive( false );
            un.Body.Sprite4.transform.eulerAngles = new Vector3( 0, 0, 0 );
            un.Body.ItemMiniDome.transform.eulerAngles = new Vector3( 0, 0, 0 );
            un.Dir = EDirection.NONE;
            if( Manager.I.GameType == EGameType.FARM )
                un.Body.BonusItemList = null;              // no chests in the farm
            un.Body.Sprite5.gameObject.SetActive( false );
        }
        else
        if( un.TileID == ETileType.VINES )
        {
            un.Body.EffectList[ 0 ].gameObject.SetActive( false );
            un.Control.VineList = new List<int>();
            un.Control.VineColorList = new List<int>() { -1, -1, -1, -1, -1, -1, -1, -1 };
            un.Body.CreatedVineList = new List<int>() { -1, -1, -1, -1, -1, -1, -1, -1 };
            un.Control.ForceVineLink = new List<int>() { -1, -1, -1, -1, -1, -1, -1, -1 };
            for( int i = 0; i < 8; i++ )
                un.Body.PoleSpriteList[ i ].color = new Color( 1, 1, 1, 0 );
        }

		if( un.UnitType == EUnitType.MONSTER )
		{
            if( un.Spr && pfun.Spr )
                un.Spr.scale = pfun.Spr.scale;
            un.Graphic.gameObject.SetActive( true );
            un.Graphic.transform.localPosition = Vector3.zero;
            if( un.Body != null )
            {
                un.Body.IsDead = false;
                un.Activated = true;
                un.Body.Hp = un.Body.TotHp;
                if( un.Body.BaseTotHP == 0 )
                    un.Body.BaseTotHP = un.Body.TotHp;
                un.Body.UpdateHealthBar();
            }

            if( un.RightText )
            {
                un.RightText.text = "";
                un.RightText.gameObject.SetActive( false );
            }

            if( un.TileID == ETileType.BARRICADE )
            {
                un.Graphic.transform.localPosition = new Vector3( 0, 0, 0 );
                un.Body.EffectList[ 0 ].gameObject.SetActive( false );
            }
            else
            if( un.TileID == ETileType.ALTAR )
            {
                if( Util.Chance( 50 ) )                                                         // Altar rotation direction is defined by direction E or W
                    un.Dir = EDirection.W;
                else
                    un.Dir = EDirection.E;
            }
            else
            if( un.TileID == ETileType.FAN )
            {
                un.Activated = false;
                un.Body.EffectList[ 0 ].SetActive( false );
            }
            else
            if( un.TileID == ETileType.MINE )
            {
                un.Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );
                un.Body.MineType = RM.RMD.DefaultMineType;
                un.RotateTo( ( EDirection ) Util.GetRandomDir() );
                un.Body.UPMineDir = ( EDirection ) Util.GetRandomDir();
                un.Mine.MineBonusDir = ( EDirection ) Util.GetRandomDir();
                un.Body.Sprite5.gameObject.SetActive( false );
                un.Body.Sprite5.spriteId = 238;
                un.Body.gameObject.transform.localScale = new Vector3( 1, 1, 1 );
                un.Body.EffectList[ 2 ].gameObject.SetActive( false );
                un.Body.EffectList[ 2 ].transform.localPosition = new Vector3( 0, 0, -1.66f );
                un.Body.EffectList[ 4 ].gameObject.SetActive( false );
                un.Body.Sprite4.transform.eulerAngles = new Vector3( 0, 0, 0 );
                un.Spr.transform.position = new Vector3( un.Pos.x, un.Pos.y, -1.7f );
                un.Spr.scale = new Vector3( 1, 1, 1 );
                un.RightText.transform.localPosition = new Vector3( 0, -0.29f, -3f );
                un.Spr.color = new Color( 1, 1, 1, 0 );
                un.Body.Sprite3.color = Color.white;
                un.Body.Sprite8.transform.localScale = new Vector3( 1, 1, 1f );
                un.Body.Sprite8.scale = new Vector3( 1, 1, 1 );
                un.Body.Sprite8.transform.localPosition = new Vector3( 0, 0, -1.85f );
                un.Mine.UpdateText = true;
                un.Body.Sprite2.gameObject.SetActive( false );
                un.Body.Sprite3.gameObject.SetActive( false );
                un.Body.Sprite3.transform.localPosition = new Vector3( 0, 0, un.Body.Sprite3.transform.localPosition.z );
                un.Body.Sprite2.transform.localScale = new Vector3( 1.3f, 1.3f, 1 );                
            }
            else
            if( un.TileID == ETileType.PLAGUE_MONSTER )
            {
                un.Variation = ( int ) Farm.GetRandomMonsterType();
                un.Spr.spriteId = G.GIT( un.Variation ).TKSprite.spriteId;
            } 
            else
            if( un.TileID == ETileType.PROJECTILE )
            {
                Util.SetActiveRecursively( un.Body.EffectList[ 0 ].gameObject, true );
                un.Spr.color = Color.white;
                un.Control.Mother = null;
            } 
            else
            if( un.TileID == ETileType.IRON_BALL )
            { 
                un.Spr.color = Color.white;
                un.Body.Sprite2.gameObject.SetActive( false );
            }
            else
            if( un.TileID == ETileType.BOUNCING_BALL )
            { 
                un.Spr.color = Color.white;
               // un.transform.position = new Vector3( -1, -1, -1 );
            }
            else
            if( un.TileID == ETileType.BLOCKER )
            {
                un.Spr.transform.Rotate( 0.0f, 0.0f, Random.Range( 0.0f, 360.0f ) );
            }
            else
            if( un.TileID == ETileType.ROACH )
            {
                un.Body.Sprite2.gameObject.SetActive( true );
                un.Body.Sprite3.gameObject.SetActive( true );
            }
            if( un.TileID == ETileType.HUGGER )
            {
                un.Control.HuggerStepList = null;
                un.Body.Sprite5.gameObject.SetActive( false );
            }
            else
            if( un.TileID == ETileType.SCARAB )
            {
                un.Body.InfectedSprite.gameObject.SetActive( false );
                un.Body.PerformPreMoveAttack = true;
                un.Body.PerformPostMoveAttack = false;
            }
            else
            if( un.TileID == ETileType.FROG )
            {
                un.Body.Sprite2.gameObject.SetActive( true );
                un.Body.Sprite2.transform.localPosition = new Vector3( 0, -0.2f, 0.24f );
            }
            else
            if( un.TileID == ETileType.SCORPION )
            {
                NumScorpions++;
            }
            else
            if( un.TileID == ETileType.BOULDER )
            {
                un.Body.BabySpriteFolder.gameObject.SetActive( false );
                un.Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );
            }
            else
            if( un.TileID == ETileType.DRAGON1 )
            {
                un.Body.Animator = un.Spr.GetComponent<tk2dSpriteAnimator>();
                un.Body.Animator.Play( "Seagul Fly" );
                un.Spr.transform.Rotate( 0.0f, 0.0f, Random.Range( 0.0f, 360.0f ) );
                un.Control.CastProjectileList = new List<float>();
            }
            else
            if( un.TileID == ETileType.RAFT )
            {
                for( int i = 0; i < 8; i++ )
                {
                    un.Control.RaftJointList[ i ].gameObject.SetActive( false );
                    un.Control.RaftJointList[ i ].transform.localScale = new Vector3( .2f, .2f, 1 );
                }
                un.Control.NotAnimatedPosition = un.Graphic.transform.position;
                un.RotateTo( EDirection.N );
            }
            else
            if( un.TileID == ETileType.ALGAE )
            {
                BabyData.CreateAlgae( un );
            }           
            else
            if( un.TileID == ETileType.FOG )
            {
                un.Body.Animator = un.Spr.GetComponent<tk2dSpriteAnimator>();
                var clip = un.Body.Animator.GetClipByName( "Fog Animation" );
                if( clip != null && clip.frames.Length > 0 )
                {
                    int randomFrame = Random.Range( 0, clip.frames.Length );
                    float clipStartTime = ( float ) randomFrame / clip.fps;
                    un.Body.Animator.Play( clip, clipStartTime, 0f );
                }
            }
            else
            if( un.TileID == ETileType.ORB )
            {
                un.Body.EffectList[ 2 ].gameObject.SetActive( true );
            }
            else
            if( un.TileID == ETileType.SPIKES )
            {                
                //un.Variation = Random.Range( 0, 2 );
                un.Spr.transform.position = new Vector3( un.Pos.x, un.Pos.y, -1.5f );
                un.Spr.transform.localPosition = new Vector3( 0, 0, -0.45f );
            }
            else
            if( un.TileID == ETileType.TOWER )
            {                
                Unit forest = Map.I.GetUnit( ETileType.FOREST, un.Pos );
                if( forest ) un.Control.ControlType = EControlType.NONE;
            }
            else
            if( un.TileID == ETileType.TRAIL )
            {
                un.Spr.color = Color.red;
            }

            if( un.ValidMonster )
            {
                for( int i = 0; i < un.Body.Sp.Count; i++ )
                    un.Body.Sp[ i ].Reset();
            }

            //if( un.TileID == ETileType.QUEROQUERO )
            //    un.SetLevelText( "" + un.Control.RealtimeSpeed );

            if ( un.Body.BigMonster )
            if ( un.Body.ArrowList != null )
            for( int i = 0; i < un.Body.ArrowList.Length; i++ )
                 un.Body.ArrowList[ i ].gameObject.SetActive( false );

            if( Manager.I.GameType == EGameType.CUBES )
                if( un.TileID == ETileType.DOME )
                {
                    un.Spr.transform.localScale = new Vector3( .1f, .1f, 0 );
                    un.Spr.gameObject.GetComponent<tk2dAnimationAdapter>().color = new Color( 1, 1, 1, .3f );
                    un.PriceTag.gameObject.SetActive( false );
                }

            un.Control.OldPos = tg;
            un.Control.LastPos = tg;
            un.Control.AnimationOrigin = tg;
            int area = GetPosArea( un.Pos );
            if( area != -1 )
			{
                un.Body.Level = Quest.I.CurLevel.AreaList[ area ].DefaulMonsterLevel;
                un.Body.Stars = Quest.I.CurLevel.AreaList[ area ].DefaulMonsterStar;
			}
			un.UpdateLevelingData();
            un.Body.ChildList = new List<Unit>();
            un.Control.Nest = null;

            if( un.TileID == ETileType.FISH ) CreateFish( un );
            if( un.TileID == ETileType.HERB ) CreateHerb( un );
		}

        if( un.TileID == ETileType.SAVEGAME )
        {
            un.Body.Sprite2.gameObject.SetActive( false );
            un.RightText.gameObject.SetActive( false );
            un.Body.EffectList[ 0 ].gameObject.SetActive( false );
            un.Body.EffectList[ 1 ].gameObject.SetActive( false );
            un.Body.EffectList[ 2 ].gameObject.SetActive( false );
            un.Variation = ( int ) RM.RMD.DefaulLoadItemType;
            un.Body.StackAmount = RM.RMD.DefaultLoadCost;
            un.Body.Sprite2.spriteId = G.GIT( un.Variation ).TKSprite.spriteId;
            un.RightText.text = "x" + GS.GetLoadCost( un ).ToString( "0.#" );
        }

        if( un.Body )
        if( un.Body.ItemMiniDome )
        {
            un.Body.ItemMiniDome.gameObject.SetActive( false );
            un.Body.ItemMiniDome.transform.rotation = Quaternion.EulerAngles( 0, 0, 0 );
            un.Body.ItemMiniDome.color = Color.white;
        }

        if( un.Control )
        if( un.Control.RestingRadiusSprite )
            un.Control.RestingRadiusSprite.gameObject.SetActive( false );

		if( !un.UseTransTile )
		{
			Tilemap.SetTile( ( int ) tg.x, ( int ) tg.y, ( int ) layer, ( int ) un.TileID + un.Variation ); // optimize only if necessary
		}
		else
		{
            //TransTilemapUpdateList.Add( tg ); // new opt
        }
		return instance;
	}


	//_____________________________________________________________________________________________________________________ Calculates Movement score for each tile	
	
	public void CalculateAIData()
	{
        Area.UpdateAiTarget( Hero.Pos, true );
        return;
        Vector2 p1 = new Vector2( P1.x, P1.y );
        Vector2 p2 = new Vector2( P2.x, P2.y );

        Vector2 target = LockAiTarget;

        if( CurrentArea == -1 )                                  // Outarea Flying Units
        {
            p1 = RM.HeroSector.Area.min;
            p2 = RM.HeroSector.Area.max;
        }

        Vector2 p = new Vector2( 0, 0 );
        for( p.y = p1.y; p.y <= p2.y; p.y++ )
        for( p.x = p1.x; p.x <= p2.x; p.x++ )
        {
            //Debug.Log(p);
            //float dist = Vector3.Distance( new Vector3( target.x, target.y, 0 ), new Vector3( p.x, p.y, 0 ) );
            DistFromTarget[ ( int ) p.x, ( int ) p.y ] = Mathf.Max( Util.Mod( target.x - p.x ), Util.Mod( target.y - p.y ) );
            int xx = ( int ) Util.Mod( target.x - p.x );
            int yy = ( int ) Util.Mod( target.y - p.y );

            //if( yy <= 28 )
            //    DistFromTarget[ ( int ) p.x, ( int ) p.y ] = RandTable.RandVal[ xx, yy ];
            //else
            //    DistFromTarget[ ( int ) p.x, ( int ) p.y ] = Vector3.Distance( new Vector3( target.x, target.y, 0 ), new Vector3( p.x, p.y, 0 ) );

            DistFromTarget[ ( int ) p.x, ( int ) p.y ] = Util.Manhattan( target, p );            
            DistFromTarget[ ( int ) p.x, ( int ) p.y ] += Vector3.Distance( target, p ) / 1000;
            if( Map.I.IsInTheSameLine( target, p, false ) ) DistFromTarget[ ( int ) p.x, ( int ) p.y ] -= 2;   
        }

        //Unit ga2 = GetUnit( ETileType.WEB, G.Hero.Pos );                                                              // Hero over Rotator: Adjust move score to his back
        //if( ga2 )
        //{
        //    Vector2 s = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.S, 1 );
        //    if( !GetUnit( ETileType.BARRICADE, s ) ) DistFromTarget[ ( int )  s.x, ( int )  s.y ] = -1000000;
        //    Vector2 se = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.SE, 1 );
        //    if( !GetUnit( ETileType.BARRICADE, se ) ) DistFromTarget[ ( int ) se.x, ( int ) se.y ] = -100000;
        //    Vector2 sw = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.SW, 1 );
        //    if( !GetUnit( ETileType.BARRICADE, sw ) ) DistFromTarget[ ( int ) sw.x, ( int ) sw.y ] = -100000;
        //    Vector2 w = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.W, 1 );
        //    if( !GetUnit( ETileType.BARRICADE, w ) ) DistFromTarget[ ( int ) w.x, ( int ) w.y ] = -100000;
        //    Vector2 e = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.E, 1 );
        //    if( !GetUnit( ETileType.BARRICADE, e ) ) DistFromTarget[ ( int ) e.x, ( int ) e.y ] = -100000;
        //    Vector2 ne = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.NE, 1 );
        //    if( !GetUnit( ETileType.BARRICADE, ne ) ) DistFromTarget[ ( int ) ne.x, ( int ) ne.y ] = -10000;
        //    Vector2 nw = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.NW, 1 );
        //    if( !GetUnit( ETileType.BARRICADE, nw ) ) DistFromTarget[ ( int ) nw.x, ( int ) nw.y ] = -10000;
        //}
    }

	//_____________________________________________________________________________________________________________________ Update Monster Movement

	public void UpdateMonstersMovement ()
	{
        if( Manager.I.GameType == EGameType.FARM ) return;
        if( Manager.I.GameType == EGameType.NAVIGATION ) return;
        if( AdvanceTurn == false ) return;   

        if( CurrentArea == -1 )                                                                                     // Calculates AI data for Flying Units And Outarea Monsters
        {
            if( G.HS.Fly == null ) return;
            if( G.HS.Fly.Count > 0 || 
                RM.HeroSector.Type == Sector.ESectorType.NORMAL && 
                RM.HeroSector.MoveOrder.Count >= 1 ) 
                CalculateAIData();
            return;
        }

        CalculateAIData();

        Controller.IsRealtime = false;
    }

    public void FinalizeMonstersMovement()
    {
        if( HeroDeathTimer != -1 )
            HeroDeathTimer -= Time.deltaTime;                                                                                 // Hero death timer increment

        if( HeroDeathTimer > -1 && HeroDeathTimer < 0 )                                                          
            StartCubeDeath();                                                                                                 // Deth timer is over, die

        if( AdvanceTurn == true )
        if( --G.Hero.Body.InvulnerabilityFactor < 0 )                                                                         // invulnerability factor
            G.Hero.Body.InvulnerabilityFactor = 0;

        Attack.SpiderSquash = false;
        if( AdvanceTurn == false )
        if( MonstersMoved == false ) return;
        if( CurrentArea == -1 ) return;

        //if( MoveOrder != null )
        //    for( int i = 0; i < MoveOrder.Count; i++ )                                                                     // Update Unit Position History
        //    {
        //        MoveOrder[ i ].Control.UpdatePositionHistory();
        //        MoveOrder[ i ].Body.ResetTurnData();
        //        MoveOrder[ i ].Body.CalculateMonsterBonusAttack( false, 3 );
        //    }

        //UpdateMarkedMonsters();

        //UpdateAutoWakeUp();
    }

    public void UpdateRealtimeMonstersMovement()
    {
        if( Manager.I.GameType == EGameType.FARM ) return;
        if( RM.HeroSector == null ) return;
        if( RM.HeroSector.Type != Sector.ESectorType.NORMAL ) return;
        if( Map.I.Gaia2 == null ) return;

        if( CurrentArea == -1 )                                                                                    // Flying Units Movement Update
        {
            Controller.JumperCount = 0;
            Controller.HerbCount = 0;
            Controller.FinishedJumperCount = 0;
            Controller.WaspCount = 0;
            Controller.MotherWaspCount = 0;
            Controller.UpdateTickTimer();

            for( int i = RM.HeroSector.DynamicObjects.Count - 1; i >= 0; i-- )
            {
                RM.HeroSector.DynamicObjects[ i ].UpdateRightText();
            }

            for( int i = G.HS.Fly.Count - 1; i >= 0; i-- )
            {
                G.HS.Fly[ i ].Control.SpawnCount = 0;
                G.HS.Fly[ i ].Control.ShieldedWaspCount = 0;                                           // optmize
                G.HS.Fly[ i ].Control.CocoonWaspCount = 0;
                G.HS.Fly[ i ].Control.EnragedWaspCount = 0;
                G.HS.Fly[ i ].Control.BeingMudPushed = false;
                G.HS.Fly[ i ].Control.FlightPhaseTimer += Time.deltaTime;
                G.HS.Fly[ i ].Control.FlightStepPhaseTimer += Time.deltaTime;
                G.HS.Fly[ i ].UpdateColor();
                G.HS.Fly[ i ].UpdateRightText();
                G.HS.Fly[ i ].UpdateDirection();

                if( G.HS.Fly[i].TileID == ETileType.MOTHERWASP )
                {
                    G.HS.Fly[ i ].Control.WaspOccupiedTiles = new List<Vector2>();
                    Controller.MotherWaspCount++;
                }                  
            }

            for( int i = G.HS.Fly.Count - 1; i >= 0; i-- )
            {
                if( G.HS.Fly[ i ].TileID == ETileType.JUMPER ||                                          // Counts Spawn
                    G.HS.Fly[ i ].TileID == ETileType.WASP )
                if( G.HS.Fly[ i ].Control.Mother )
                {
                    G.HS.Fly[ i ].Control.Mother.Control.SpawnCount++;
                    if( G.HS.Fly[ i ].Body.ShieldedWasp )
                        G.HS.Fly[ i ].Control.Mother.Control.ShieldedWaspCount++;
                    if( G.HS.Fly[ i ].Body.Sprite3.gameObject.activeSelf )
                        G.HS.Fly[ i ].Control.Mother.Control.CocoonWaspCount++;
                    if( G.HS.Fly[ i ].Body.EnragedWasp )
                        G.HS.Fly[ i ].Control.Mother.Control.EnragedWaspCount++;
                }

                if( G.HS.Fly[ i ].TileID == ETileType.HERB ) 
                    Controller.HerbCount++;  

                if( G.HS.Fly[ i ].TileID == ETileType.WASP )                                             // Counts tiles occupied by wasps
                {
                    Unit un = G.HS.Fly[ i ];
                    if( un.Control.Mother.Control.WaspOccupiedTiles.Contains( un.Pos ) == false ) 
                    if( un.Control.Mother.Pos != un.Pos  )
                        un.Control.Mother.Control.WaspOccupiedTiles.Add( un.Pos );
                }

                if( AdvanceTurn )
                if( G.HS.Fly[ i ].TileID == ETileType.MINE )
                {
                    G.HS.Fly[ i ].Body.EffectList[ 4 ].SetActive( false );
                    //if( Util.IsNeighbor( G.HS.Fly[ i ].Pos, G.Hero.Pos ) == false )
                    //    G.HS.Fly[ i ].Body.EffectList[ 2 ].SetActive( false );
                }
            }

            Controller.UpdateAllMotherWasps();                                                                 // Updates all Mother Wasp Stuff
            Controller.FogKillList = new List<Unit>();
            for( int i = G.HS.Fly.Count - 1; i >= 0; i-- )                                                // Flying Move and Attack Update
            if( G.HS.Fly[ i ].ValidMonster                      ||                      // new
                G.HS.Fly[ i ].TileID == ETileType.FOG           ||        // optimize fog: only if electrified            
                G.HS.Fly[ i ].TileID == ETileType.FISH          ||        // Make a new list of only the ones that need to be moved, remove unused fog
                G.HS.Fly[ i ].TileID == ETileType.MINE          ||
                G.HS.Fly[ i ].TileID == ETileType.PROJECTILE    ||
                G.HS.Fly[ i ].TileID == ETileType.RAFT          ||                    
                G.HS.Fly[ i ].TileID == ETileType.SPIKES        ||       
                G.HS.Fly[ i ].TileID == ETileType.FISHING_POLE  ||
                G.HS.Fly[ i ].TileID == ETileType.BOUNCING_BALL ||
                G.HS.Fly[ i ].TileID == ETileType.IRON_BALL     ||
                G.HS.Fly[ i ].TileID == ETileType.TRAIL )
            {
                G.HS.Fly[ i ].Control.UpdateIt();
                if( Controller.UnitHasBeenKilledWhileMoving == false )
                {
                    G.HS.Fly[ i ].UpdateAllAttacks( false );
                    if( G.HS.Fly[ i ].TileID == ETileType.MINE )
                        G.HS.Fly[ i ].CheckFireDamage();
                }
            }
            for( int i = Controller.FogKillList.Count - 1; i >= 0; i-- )
                Controller.FogKillList[ i ].Kill();

            Sector.UpdateOutAreaMovement();
            return;
        }
    }
    public void UpdateRealtimeHeroMovement()
    {
        if( Manager.I.GameType == EGameType.FARM ) return;
        if( RM.HeroSector == null ) return;
        if( RM.HeroSector.Type != Sector.ESectorType.NORMAL ) return;

        Time.timeScale = 1;

        if( CurrentArea != -1 ) 
        if( CurArea.AreaTurnCount <= 0 ) return;

        if( Input.GetKey( KeyCode.Z ) ) 
            Time.timeScale = RM.RMD.HurryUpTimeScaleFactor;                                              // Hurry up time

        if( G.Hero.Control.PathFinding.Path != null && 
            G.Hero.Control.PathFinding.Path.Count > 0 )                                                  // Pathfind Hurry up
        {
            Time.timeScale = 30;
            if( Application.platform == RuntimePlatform.WindowsPlayer )
                Time.timeScale = 8;
        }

        if( CurrentArea != -1 )
        {
            if( CurArea.Vigor > 0 )
                CurArea.Vigor -= Time.deltaTime;                                                         // Vigor Timer Decrease
            if( CurArea.Vigor < 0 )
            {
                CurArea.Vigor = 0;
                Message.RedMessage( "Vigor effect Finished!" );
                MasterAudio.PlaySound3DAtVector3( "Error", transform.position );
            }
        }

        Controller.IsRealtime = true;
        if( OverWaterAttackTimer != -1 )                                                                        // Over Water Attack Timer
        if( GetMud( G.Hero.Pos ) == null )
            OverWaterAttackTimer += Time.deltaTime;        
        if( OverWaterAttackTimer > Map.I.RM.RMD.WaspOverWaterBonusTime ) 
            OverWaterAttackTimer = -1;

        Unit fr = GetUnit( Hero.GetFront(), ELayerType.MONSTER );

        //if( AdvanceTurn )
        //if( fr && fr.TileID == ETileType.SCARAB )
        //    Hero.MeleeAttack.SpeedTimeCounter = Hero.MeleeAttack.GetRealtimeSpeedTime();                           // Auto attack frontal scarabs Removed cause it cause bugs. some cube may be broken

        HeroAttackSpeedBonus = Hero.GetPlatformSpeedAttackBonus(); 

        if( OverWaterAttackTimer >= 0 && 
            OverWaterAttackTimer <= Map.I.RM.RMD.WaspOverWaterBonusTime )                                      // Over Water Att speed bonus
            HeroAttackSpeedBonus += RM.RMD.WaspOverWaterSpeedBonus;

        int countneigh = 0;
        float totneigh = 0;
        if( RM.RMD.NeighborWaspAttSpeedBonusPerTile > 0 )
        {
            int frange = 1;
            for( int y = ( int ) Hero.Pos.y - frange; y <= Hero.Pos.y + frange; y++ )                          // Neighbor wasp bonus att speed calculation
            for( int x = ( int ) Hero.Pos.x - frange; x <= Hero.Pos.x + frange; x++ )
            if ( PtOnMap( Tilemap, new Vector2( x, y ) ) )
                {
                    if ( GetFUnit( x, y ) > 0 )
                    for( int i = 0; i < FUnit[ x, y ].Count; i++ )
                    if( FUnit[ x, y ][ i ].TileID == ETileType.WASP )
                    if( FUnit[ x, y ][ i ].Control.Mother.Control.Resting == false )                        
                     {
                         countneigh++; break;
                     }
                }
            if( countneigh > 0 )
                totneigh = Util.CompoundInterest( RM.RMD.NeighborWaspAttSpeedBonusPerTile,
                countneigh, RM.RMD.NeighborWaspPerTileInflation, true );              
        }

        int countalign = 0;  
        float totalign = 0;
        if( RM.RMD.AlignedWaspAttSpeedBonusPerTile > 0 )                                                      // Front Aligned wasp bonus att speed calculation
        {
            int range = G.Hero.RangedAttack.GetEffectiveShootingRange( true );

            for( int i = 0; i < range; i++ )
            {
                Vector2 tg = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * ( i + 1 );
                if ( GetFUnit( ( int ) tg.x, ( int ) tg.y ) > 0 )
                for( int u = 0; u < FUnit[ ( int ) tg.x, ( int ) tg.y ].Count; u++ )
                if ( FUnit[ ( int ) tg.x, ( int ) tg.y ][ u ].TileID == ETileType.WASP )
                if ( FUnit[ ( int ) tg.x, ( int ) tg.y ][ u ].Control.Mother.Control.Resting == false )    
                {
                    countalign++; break;
                }
            }
            if( countalign > 0 )
                totalign = Util.CompoundInterest( RM.RMD.AlignedWaspAttSpeedBonusPerTile, 
                countalign - 1, RM.RMD.AlignedWaspPerTileInflation, true );            
        }

        HeroAttackSpeedBonus += totneigh + totalign;

        if( HeroAttackSpeedBonus > 0 )                                                                                                            // Applies Attack Speed Bonus
        {
            float extra = Util.Percent( HeroAttackSpeedBonus, Time.deltaTime );
            G.Hero.RangedAttack.SpeedTimeCounter += extra;
            G.Hero.MeleeAttack.SpeedTimeCounter += extra;
        }

        if( Map.Stepping() == false )
            Hero.UpdateAllAttacks( true );

        #region old
        // }

        //UpdateInactiveHeroes();
        //UI.I.UpdatePortrait( ( EHeroID ) Map.I.Hero.Variation );

        //if( CurrentArea == -1 )
        //    AreaExitTurnCount++;

        //if( CurrentArea != -1 )
        //{
        //    for( int i = 0; i < MoveOrder.Count; i++ )
        //        MoveOrder[ i ].Control.ResetShakeTimer();
        //    CurArea.AreaTurnCount++;
        //    CurArea.LifetimeSteps++;
        //    if( CurArea.Cleared == false )
        //        CurArea.DirtyLifetimeSteps++;
        //    else
        //        CurArea.AreaClearedTurnCount++;
        //}

        //LevelTurnCount++;

        //for( int i = 0; i < AttackList.Count; i++ )
        //{
        //    AttackList[ i ].FinishKilling( false );
        //}

        //Hero.Control.UpdatePositionHistory();

        //UpdateThreatMonsterSpecial();
        #endregion
    }

    public void UpdateHeroMovement()
	{
        Controller.HeroJumped = false;
        NumMonsterSeeingHero = 0;
        PlatformDeath = false;
        BarricadeDestroyedInTheTurn = false;
        Controller.IsRealtime = false;
        MonstersMoved = false;
        Attack.IsMoveShot = false;
        Controller.MaskMoveInitiated = false;
        Controller.MaskMoveFinalized = false;
        Controller.StitchesPunishment = true;
        Controller.BlockStepping = false;
        MaskMove = false;
        Controller.CornAlreadyCharged = false;
		Hero.Control.UpdateIt();                                                                // Updates Hero Movement

		if( AdvanceTurn == false ) return;                                                      // From hero on, advance only if Move has been made

        TurnTime = 0;
        TurnFrameCount = 0;
        GS.RestartAvailable = false;
        UI.I.UpdBeastText = true;
        UpdateVineExpansion();
        G.Hero.Control.UpdateFanActivation();                                                  // Update Fan Activation
        Sector s = Map.I.RM.HeroSector;
        if( Stepping() == true )
        if( Manager.I.GameType == EGameType.CUBES )
        if( s.Type == Sector.ESectorType.NORMAL )
        for( int i = 0; i < s.MoveOrder.Count; i++ )                                           // Optimize
        {
            s.MoveOrder[ i ].Control.UnitProcessed = false;
            if( s.MoveOrder[ i ].TileID == ETileType.SPIDER )
            if( Util.Neighbor( G.Hero.Pos, s.MoveOrder[ i ].Pos ) == false )                    // neighbor spider att block var reset
            {
                s.MoveOrder[ i ].Control.SpiderAttackBlockPhase = 0;
            }
        }

        Secret.CheckSecretPickup();                                                            // Check for secret pickup
        Spell.UpdateHookedMonsters();                                                          // Updates hooked monsters movement
        Sector.UpdateCloverPicking();
        UI.I.UpdateTurnInfoText();
        Mine.UpdateAfterHeroStep();                                                            // Mine stuff after hero step
        UpdateHeroChanging();
        Attack.SuccessfulBeehiveAttack = false;
		Hero.UpdateOrbHit();
        Farm.UpdatePlagueMonsterMovingAway();                                                  // Farm Monster Moving away disables indicators

        if( Manager.I.GameType == EGameType.CUBES ) 
        if( G.HS.MaxTicTacMoves > 0 )                                                          // Max Tic Tac moves vault power
            G.HS.TicTacMoveCount++;

        UpdateMask();                                                                          // Update Mask Stepping

        UpdateTrail();                                                                         // Update trails

        if( CurrentArea == -1 && Quest.CurrentLevel != -1 )
        {
            Hero.Body.Hp = Hero.Body.TotHp;
            return;
        }
        else
        {
            Create2DMap = true;
        }

        if( CurrentArea != -1 )
        if( Hero.Body.BerserkLevel < 5 || CurArea.AreaTurnCount > 0 ) 
            Hero.Body.NumFreeAttacks = 0;                                                 // Berseker < L5 only works in the initial turn

        int att = 1 + ( int ) Hero.Body.NumFreeAttacks;

        if( Manager.I.GameType == EGameType.CUBES )
        if( Map.Stepping() == true )
        for( int i = 0; i < att; i++ )                                                     // Update hero attacks
        {
            Hero.UpdateAllAttacks( true );
        }

		//UpdateInactiveHeroes();
        UI.I.UpdatePortrait( ( EHeroID ) Map.I.Hero.Variation );

        if( CurrentArea == -1 )
            AreaExitTurnCount++;

        if( CurrentArea != -1 )
        {
            //for( int i = 0; i < MoveOrder.Count; i++ )
            //    MoveOrder[ i ].Control.ResetShakeTimer();
            CurArea.AreaTurnCount++;
            CurArea.LifetimeSteps++;
            if( CurArea.Cleared == false ) 
                CurArea.DirtyLifetimeSteps++;
            else
                CurArea.AreaClearedTurnCount++;
        }

		LevelTurnCount++;
        if( Manager.I.GameType == EGameType.CUBES )
        if( RM.HeroSector.Type == Sector.ESectorType.NORMAL )
            RM.HeroSector.CubeTurnCount++;

        Hero.Control.UpdatePositionHistory();
	}
    public bool UpdateOxygen( EActionType ac )
    {
        if( FishingMode != EFishingPhase.NO_FISHING ) return false;
        if( RM.HeroSector == null ) return false;
        if( RM.HeroSector.Type != Sector.ESectorType.NORMAL ) return false;
        if( OxygenActive )
        {  
            if( ac != EActionType.NONE )                                                    // Stop breathing oxygen
            {
                MasterAudio.StopAllOfSound( "Breathing 2" );                
                OxygenActive = false;
            }
            return false;
        }

        float oxygen = Item.GetNum( ItemType.Res_Oxygen );
        if( Helper.I.ReleaseVersion )
        if( oxygen < 1 ) return false;
        if( Map.Stepping() ) return false;
        if( Map.I.FanCount == 0 )
        if( Input.GetKeyDown( KeyCode.Space ) )                                                     // Start Breathing oxygen
        if( BoomerangList.Count <= 0 )
        {
            Item.AddItem( ItemType.Res_Oxygen, -1 );
            OxygenActive = true;
            MasterAudio.PlaySound3DAtVector3( "Breathing 2", G.Hero.Pos );
            return true;
        }
        return false;
    }

    List<Vector2> fbtl = new List<Vector2>();

    public void UpdateFireBall()
    {
        if( FireBallCount <= 0 ) return;
        if( G.Hero.Control.UnitMoved == false ) return;
        if( G.Hero.Pos == G.Hero.Control.OldPos ) return;
        EDirection mdir = Util.GetTargetUnitDir( G.Hero.Control.OldPos, G.Hero.Pos );       // Which diretion to move
        if( G.Hero.Control.LastAction > EActionType.MOVE_NW )
            mdir = EDirection.NONE;
        if( mdir == EDirection.NONE ) return;        

        fbtl = new List<Vector2>(); 
        EDirection d1 = EDirection.NONE;
        EDirection d2 = EDirection.NONE;
        List<Unit> ul = GetMimicMoveUnitList( mdir, ref d1, ref d2, true );
        bool fx = false;
        for( int u = 0; u < ul.Count; u++ )                                                      // loop all units
        {
            Unit un = ul[ u ];
            EDirection dir = mdir;
            Vector2 to = un.Pos;
            Vector2 from = un.Pos;
            for( int r = 1; r < Map.I.Tilemap.width; r++ )                                       // loops targets
            {
                to = un.Pos + ( Manager.I.U.DirCord[ ( int ) dir ] * r );
                from = un.Pos + ( Manager.I.U.DirCord[ ( int ) dir ] * ( r - 1 ) );

                List<Vector2> vl = new List<Vector2>();
                GetMimicTargets( ref vl, dir );

                bool p = CanFireBallFlyTo( un, from, from + vl[ 0 ] );                            // Main path

                if( G.HS.FireballCanMine )
                if( Controller.CanBeMined( from + vl[ 0 ] ) )
                {
                    Unit mine = Map.GFU( ETileType.MINE, to );                                   // Vault Bonus: Fireball can mine
                    if( mine )
                    {
                        from = to;
                        un.Control.FlyingTarget = to;
                        break;
                    }
                }   

                if( p == false )
                {
                    if( r == 1 && Util.IsDiagonal( dir ) )                          
                    {
                        bool p1 = CanFireBallFlyTo( un, from, from + vl[ 1 ] );                // diagonal slide
                        bool p2 = CanFireBallFlyTo( un, from, from + vl[ 2 ] );
                        if( p1 == false && p2 == false ) break;
                        if( p1 == true  && p2 == true  ) break;
                        if( p1 == true  && p2 == false ) dir = d1;
                        if( p1 == false && p2 == true  ) dir = d2;
                    }
                    else
                        if( r > 1 || Util.IsDiagonal( dir ) == false ) break;
                }

                Unit tun = Map.I.GetUnit( to, ELayerType.MONSTER );                           // monster found
                if( tun && tun.ValidMonster )
                {
                    from = to;
                    un.Control.FlyingTarget = to;   
                    un.RangedAttack.UpdateIt( true );                                         // Update Attack
                    break;
                }
            }
            un.Control.FlyingTarget = from;                                                   // Set flying target
            fbtl.Add( from );
            if( from != un.Pos ) 
                fx = true;
        }
        if( fx )
            MasterAudio.PlaySound3DAtVector3( "Fire Ignite", Map.I.Hero.Pos );               // Sound FX
    }

    private List<Unit> GetMimicMoveUnitList( EDirection mdir, ref EDirection d1, ref EDirection d2, bool fireball )
    {
        int inv = ( int ) Util.GetInvDir( mdir );
        List<Unit> ul = new List<Unit>();
        Vector2 corner = new Vector2( -1, -1 );
        List<Vector2> n = new List<Vector2>();                                          // list of border tiles to start sweeping
        List<Vector2> s = new List<Vector2>();
        List<Vector2> w = new List<Vector2>();
        List<Vector2> e = new List<Vector2>();
        List<Vector2> ne = new List<Vector2>();
        List<Vector2> se = new List<Vector2>();
        List<Vector2> sw = new List<Vector2>();
        List<Vector2> nw = new List<Vector2>();

        Sector hs = Map.I.RM.HeroSector;
        for( int x = 0; x < Sector.TSX - 1; x++ )                                       // ortho addition                  
            n.Add( new Vector2( hs.Area.xMin + x, hs.Area.yMax - 1 ) );
        for( int x = 0; x < Sector.TSX - 1; x++ )
            s.Add( new Vector2( hs.Area.xMin + x, hs.Area.yMin ) );
        for( int y = 0; y < Sector.TSY - 1; y++ )
            w.Add( new Vector2( hs.Area.xMin, hs.Area.yMin + y ) );
        for( int y = 0; y < Sector.TSY - 1; y++ )
            e.Add( new Vector2( hs.Area.xMax - 1, hs.Area.yMin + y ) );

        if( mdir == EDirection.NE )                                                      // diagonal addition 
        {
            ne.AddRange( n );
            for( int i = 0; i < e.Count; i++ )
            if ( ne.Contains( e[ i ] ) == false ) ne.Add( e[ i ] );
        }
        if( mdir == EDirection.SE )
        {
            se.AddRange( s );
            for( int i = 0; i < e.Count; i++ )
            if ( se.Contains( e[ i ] ) == false ) se.Add( e[ i ] );
        }
        if( mdir == EDirection.SW )
        {
            sw.AddRange( s );
            for( int i = 0; i < w.Count; i++ )
            if ( sw.Contains( w[ i ] ) == false ) sw.Add( w[ i ] );
        }
        if( mdir == EDirection.NW )
        {
            nw.AddRange( n );
            for( int i = 0; i < w.Count; i++ )
            if ( nw.Contains( w[ i ] ) == false ) nw.Add( w[ i ] );
        }

        List<Vector2> ll = null;
        d1 = EDirection.NONE;
        d2 = EDirection.NONE;
        switch( mdir )
        {
            case EDirection.N: ll = n; break;                                          // choose buffer according to move dir
            case EDirection.S: ll = s; break;
            case EDirection.E: ll = e; break;
            case EDirection.W: ll = w; break;
            case EDirection.NE: ll = ne; d1 = EDirection.N; d2 = EDirection.E;
            corner = new Vector2( hs.Area.xMax - 1, hs.Area.yMax - 1 ); break;
            case EDirection.SE: ll = se; d1 = EDirection.S; d2 = EDirection.E;
            corner = new Vector2( hs.Area.xMax - 1, hs.Area.yMin ); break;
            case EDirection.SW: ll = sw; d1 = EDirection.S; d2 = EDirection.W;
            corner = new Vector2( hs.Area.xMin, hs.Area.yMin ); break;
            case EDirection.NW: ll = nw; d1 = EDirection.N; d2 = EDirection.W;
            corner = new Vector2( hs.Area.xMin, hs.Area.yMax - 1 ); break;
        }

        for( int t = 0; t < ll.Count; t++ )
        {
            for( int p = 0; p < Sector.TSX; p++ )
            {
                Vector2 pt = ll[ t ] + ( Manager.I.U.DirCord[ inv ] * p );                  // adds units
                if( Sector.IsPtInCube( pt ) == false ) break;

                if( fireball )                                                              // Adds fireballs
                {
                    List<Unit> aux = Map.I.GProj( pt, EProjectileType.FIREBALL );
                    if ( aux != null )
                    for( int i = 0; i < aux.Count; i++ )
                    if ( aux[ i ].Control.Resting == false )
                    {
                        ul.Add( aux[ i ] );
                    }
                }
                else                                                                        // Adds units for mask move
                {
                    Unit un = Map.I.GetUnit( pt, ELayerType.MONSTER );
                    if( un && un.ValidMonster && un.Control.Resting == false )
                        ul.Add( un );
                }
            }
        }

        List<float> uldist = new List<float>();                                            // calculates dintance
        for( int t = 0; t < ul.Count; t++ )
        {
            float dist = Vector2.Distance( ul[ t ].Pos, corner );
            uldist.Add( dist );
        }

        if( Util.IsDiagonal( mdir ) )
        for( int k = 1; k < ul.Count; k++ )                                            // reorder targets by corner distance                                                                         // Sorts score list       
        for( int j = 0; j < ul.Count - k; j++ )
        if ( ( uldist[ j ] > uldist[ j + 1 ] ) )
        {
            float temp = uldist[ j ];
            uldist[ j ] = uldist[ j + 1 ];
            uldist[ j + 1 ] = temp;
            Unit tempu = ul[ j ];
            ul[ j ] = ul[ j + 1 ];
            ul[ j + 1 ] = tempu;
        }
        return ul;
    }

    public bool CanFireBallFlyTo( Unit un, Vector2 from, Vector2 to )
    {
        if( Sector.IsPtInCube( to ) == false ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;
        if( Map.IsWall( to ) ) return false;
        if( to == G.Hero.Pos ) return false;
        if( fbtl.Contains( to ) ) return false;
        if( IsHeroMeleeAvailable( ) )
        if( to == G.Hero.GetFront() ) return false;
        if( CheckLeverCrossingBlock( from, to ) ) return false;
        if( DoesLeverBlockMe( to, un ) ) return false;
        Unit mn = GetUnit( to, ELayerType.MONSTER );
        if( mn && mn.ValidMonster == false ) return false;
        Unit mine = Map.GFU( ETileType.MINE, to );
        if( mine )
        if( G.HS.FireballCanMine == false )
        if( mine.Body.MineType != EMineType.SHACKLE )
        if( mine.Body.MineType != EMineType.BRIDGE )
        if( mine.Body.MineType != EMineType.TUNNEL )
            return false;        
        return true;
    }
    public EDirection GetTrailSlideTarget( EDirection trailDir, EDirection heroDir )
    {
        EDirection slideDir = trailDir;                                                   // default value
        if( trailDir == EDirection.N || trailDir == EDirection.S )                        // cardinal N/S trail
        {
            if( heroDir == EDirection.N ) slideDir = EDirection.N;
            else if( heroDir == EDirection.S ) slideDir = EDirection.S;
            else if( heroDir == EDirection.E ) slideDir = EDirection.E;
            else if( heroDir == EDirection.W ) slideDir = EDirection.W;
            else if( heroDir == EDirection.NE ) slideDir = EDirection.N;
            else if( heroDir == EDirection.SW ) slideDir = EDirection.S;
            else if( heroDir == EDirection.SE ) slideDir = EDirection.S;
            else if( heroDir == EDirection.NW ) slideDir = EDirection.N;
        }
        else if( trailDir == EDirection.E || trailDir == EDirection.W )                   // cardinal E/W trail
        {
            if( heroDir == EDirection.N ) slideDir = EDirection.N;
            else if( heroDir == EDirection.S ) slideDir = EDirection.S;
            else if( heroDir == EDirection.E ) slideDir = EDirection.E;
            else if( heroDir == EDirection.W ) slideDir = EDirection.W;
            else if( heroDir == EDirection.NE ) slideDir = EDirection.E;
            else if( heroDir == EDirection.SW ) slideDir = EDirection.W;
            else if( heroDir == EDirection.SE ) slideDir = EDirection.E;
            else if( heroDir == EDirection.NW ) slideDir = EDirection.W;
        }
        else if( trailDir == EDirection.NW || trailDir == EDirection.SE )                 // diagonal NW/SE trail
        {
            if( heroDir == EDirection.N ) slideDir = EDirection.NW;
            else if( heroDir == EDirection.S ) slideDir = EDirection.SE;
            else if( heroDir == EDirection.E ) slideDir = EDirection.SE;
            else if( heroDir == EDirection.W ) slideDir = EDirection.NW;
            else if( heroDir == EDirection.NE ) slideDir = EDirection.NE;
            else if( heroDir == EDirection.SW ) slideDir = EDirection.SW;
            else if( heroDir == EDirection.NW ) slideDir = EDirection.NW;
            else if( heroDir == EDirection.SE ) slideDir = EDirection.SE;
        }
        else if( trailDir == EDirection.NE || trailDir == EDirection.SW )                  // diagonal NE/SW trail
        {
            if( heroDir == EDirection.N ) slideDir = EDirection.NE;
            else if( heroDir == EDirection.S ) slideDir = EDirection.SW;
            else if( heroDir == EDirection.E ) slideDir = EDirection.NE;
            else if( heroDir == EDirection.W ) slideDir = EDirection.SW;
            else if( heroDir == EDirection.NW ) slideDir = EDirection.NW;
            else if( heroDir == EDirection.SW ) slideDir = EDirection.SW;
            else if( heroDir == EDirection.NE ) slideDir = EDirection.NE;
            else if( heroDir == EDirection.SE ) slideDir = EDirection.SE;
        }
        else
        {
            slideDir = trailDir;                                                          // fallback for other cases
        }
        return slideDir;                                                                  // return the calculated slide direction
    }

    public static List<Unit> TrailTiles = null;
    public void UpdateTrail()
    {
        if(!G.HS || G.HS.Type != Sector.ESectorType.NORMAL ) return;                                              // only in normal cubes
        if( G.Hero.Control.LastAction < EActionType.MOVE_N ||
            G.Hero.Control.LastAction > EActionType.MOVE_NW ) return;                                             // last move valid?
        if( Map.I.BumpTarget.x != -1 ) return;

        if( TrailTiles == null )
            TrailTiles = Util.GetCubeFUnit( ETileType.TRAIL, true );                                              // get all trail tiles Only once for speed
        
        if( TrailTiles.Count == 0 ) return;                                                                       // exit if none

        EDirection heroDir = Util.GetTargetUnitDir( G.Hero.Control.OldPos, G.Hero.Pos );                          // hero movement direction

        HashSet<Unit> processedMonsters = new HashSet<Unit>();                                                    // track processed monsters

        foreach( Unit trailTile in TrailTiles )
        {
            Unit monster = GetUnit( trailTile.Pos, ELayerType.MONSTER );                                          // monster on this trail
            if( monster == null ) 
            {
                monster = Map.GFU( ETileType.MINE, trailTile.Pos );                                               // or mine?
                if( monster && monster.Control.BeingMudPushed ) 
                    monster = null;
            }

            if( !monster || processedMonsters.Contains( monster ) )                                               // skip invalid/processed
                continue;

            EDirection moveDir = GetTrailSlideTarget( trailTile.Dir, heroDir );                                   // get slide direction

            float coil = 1;
            if( trailTile.Body.TrailCoil ) coil = 2;
            Vector2 targetPos = monster.Pos + Manager.I.U.DirCord[ ( int ) moveDir ] * coil;                      // target position
            Vector2 norm = monster.Pos + Manager.I.U.DirCord[ ( int ) moveDir ]; 

            bool res = false;
            if( Map.I.CheckArrowBlockFromTo( monster.Pos, norm, monster ) == false )  
            if( coil== 1 || Map.I.CheckArrowBlockFromTo( norm, targetPos, monster ) == false ) 
            res = monster.CanMoveFromTo( true, monster.Pos, targetPos, null );                                    // attempt move
            
            if( res == false && coil > 1 )                                                                        // Coil jump failed, try normal move
            {
                if( Map.I.CheckArrowBlockFromTo( monster.Pos, norm, monster ) == false ) 
                res = monster.CanMoveFromTo( true, monster.Pos, norm, null );                                     // attempt move
                coil = 1;
            }

            if( res && coil > 1 )                                                                                 // Coil jump ok
            {
                iTween.PunchScale( trailTile.Graphic, new Vector2( .5f, .5f ), .5f );
                Map.I.CreateExplosionFX( targetPos, "Fire Explosion", "Place Log" );                              // FX
            }
            processedMonsters.Add( monster );                                                                     // mark as processed
        }
    }
    public bool UpdateTrailCocoon( Vector2 from, Vector2 to )
    {
            Unit trail = Map.GFU( ETileType.TRAIL, to );
            if( trail )
            if( trail.Variation > 0 )
            {
                EDirection mov = Util.GetTargetUnitDir( from, to );
                Vector2 tg = trail.Pos + Manager.I.U.DirCord[ ( int ) mov ];
                bool res = trail.CanMoveFromTo( true, to, tg, trail );
                if( res )
                {
                    trail.Variation--;
                    Map.I.CreateExplosionFX( tg, "Fire Explosion", "Place Log" );                              // FX
                    trail.UpdateAnimation();
                }
                else
                    return true;
            }
            return false;
        
    }
    public void UpdateTrailRotator( Unit un )
    {
        if( un.ValidMonster == false ) return;
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = un.Pos + Manager.I.U.DirCord[ i ];
            Unit trail = Map.GFU( ETileType.TRAIL, tg );
            if( trail )
            if( trail.Dir == EDirection.NONE )                                                                     // rotates trail
            {
                EDirection dr = Util.GetVectorDir( un.Pos - tg );
                trail.RotateTo( dr );
                Map.I.LineEffect( un.Pos, tg, 3.5f, .5f, Color.blue, Color.blue );                                 // line fx
                Map.I.CreateExplosionFX( tg, "Fire Explosion", "" );                                               // FX 
            }
        }
    }
    public void UpdateMask()
    {
        float mask = Item.GetNum( ItemType.Res_Mask );                                                      // Mask Check
        if( mask < 1 ) return;
        if( Controller.MaskMoveInitiated ) return;
        if( BumpTarget != new Vector2( -1, -1 ) ) return;
        if( mask < 100 )
            Item.AddItem( ItemType.Res_Mask, -1 );                                                          // Mask Decrement
        MaskMove = true;
        Controller.MaskMoveFinalized = true;
        bool r = false;
        Sector s = Map.I.RM.HeroSector;
        EDirection dir = Util.GetTargetUnitDir( G.Hero.Control.OldPos, G.Hero.Pos );
        EDirection d1 = EDirection.NONE;
        EDirection d2 = EDirection.NONE;
        List<Unit> ul = s.MoveOrder;

        if( G.Hero.Control.LastAction >= EActionType.MOVE_N )
        if( G.Hero.Control.LastAction <= EActionType.MOVE_NW )
            ul = GetMimicMoveUnitList( dir, ref d1, ref d2, false );

        if( IsStillMovement( G.Hero.Control.LastAction ) == true )
            dir = EDirection.NONE;

        for( int i = 0; i < ul.Count; i++ )
        {
            Unit un = ul[ i ];
            if( un.ValidMonster )
            if( un.Control.Resting == false )
                {
                    if ( un.Control.ForcedFrontalMovementFactor > 0 )                                      // el torero vector add
                        {
                            Vector2 forced = Manager.I.U.DirCord[ ( int )
                            un.Control.ForcedFrontalMovementDir ];
                            Vector2 move = Vector2.zero;
                            if( dir != EDirection.NONE )
                                move = Manager.I.U.DirCord[ ( int ) dir ];
                            Vector2 final = forced + move;
                            if( final.x == 2  ) final = new Vector2( 1, final.y  );
                            if( final.x == -2 ) final = new Vector2( -1, final.y );
                            if( final.y == 2  ) final = new Vector2( final.x,  1 );
                            if( final.y == -2 ) final = new Vector2( final.x, -1 );
                            dir = Util.GetTargetUnitDir( Vector2.zero, final );
                        }

                    Map.I.LastMaskMoveDir = dir;
                    if( dir == EDirection.NONE ) return;
                    List<Vector2> tgl = new List<Vector2>();
                    GetMimicTargets( ref tgl, dir );
                   
                    if( tgl.Count >= 3 )
                    {
                        r = un.CanMoveFromTo( true, un.Pos, un.Pos + tgl[ 0 ], null );                     // Try Diagonal movements
                        if( r == false )
                            r = un.CanMoveFromTo( true, un.Pos, un.Pos + tgl[ 1 ], null );
                        if( r == false )
                            r = un.CanMoveFromTo( true, un.Pos, un.Pos + tgl[ 2 ], null );
                        if( r == false )
                        {
                            un.Control.StopForcedMovement();
                        }
                    }
                    else
                    {
                        r = un.CanMoveFromTo( true, un.Pos, un.Pos + tgl[ 0 ], null );                     // Ortho move
                        if( r == false )
                        {
                            un.Control.StopForcedMovement();            
                        }
                    }
                    un.Control.SpeedTimeCounter = 0;                                                       // Reset Movement Counter
                    un.Control.FlightPhaseTimer = 0;
                    un.RotateTo( dir );
                }
        }
        Controller.MaskMoveInitiated = false;
    }
    public bool IsStillMovement( EActionType ac )
    {
        if( ac == EActionType.WAIT       ||
            ac == EActionType.ROTATE_CCW ||
            ac == EActionType.ROTATE_CW  ||
            ac == EActionType.SPECIAL ) return true;
        return false;
    }

    public void GetMimicTargets( ref List<Vector2> tgl, EDirection dir )
    {
        switch( dir )
        {
            case EDirection.N:                                                                 // Orthogonal Mask Move 
            tgl.Add( new Vector2( 0, 1 ) );
            break;
            case EDirection.S:
            tgl.Add( new Vector2( 0, -1 ) );
            break;
            case EDirection.E:
            tgl.Add( new Vector2( 1, 0 ) );
            break;
            case EDirection.W:
            tgl.Add( new Vector2( -1, 0 ) );
            break;
            case EDirection.NE:                                                                // NE Mask Move
            tgl.Add( new Vector2( 1, 1 ) );
            tgl.Add( new Vector2( 0, 1 ) );
            tgl.Add( new Vector2( 1, 0 ) );
            break;
            case EDirection.SE:                                                                // SE Mask Move
            tgl.Add( new Vector2( 1, -1 ) );
            tgl.Add( new Vector2( 0, -1 ) );
            tgl.Add( new Vector2( 1, 0 ) );
            break;
            case EDirection.SW:                                                                // SW Mask Move
            tgl.Add( new Vector2( -1, -1 ) );
            tgl.Add( new Vector2( 0, -1 ) );
            tgl.Add( new Vector2( -1, 0 ) );
            break;
            case EDirection.NW:                                                                // MW Mask Move
            tgl.Add( new Vector2( -1, 1 ) );
            tgl.Add( new Vector2( 0, 1 ) );
            tgl.Add( new Vector2( -1, 0 ) );
            break;
        }
    }

    public void UpdateLateHeroStuff()
    {
        if( AdvanceTurn == false ) return;               

        //if( G.Hero.Body.IntelThreatLevel >= 5 )                                                          // Monster Trap Stepping
        //if( MoveOrder != null )
        //for( int i = 0; i < MoveOrder.Count; i++ )
        //if( MoveOrder[ i ].ValidMonster )
        //if( MoveOrder[ i ].Body.ThreatLevel > 0 )
        //{
        //    MoveOrder[ i ].Control.UpdateTrapStepping();
        //}

        if( PlatformDeath || CubeDeath )                                                       // Cube Death
            Hero.Body.UpdateCubeDeath();

        if( Map.I.LevelTurnCount == 2 && Manager.I.GameType == EGameType.CUBES )
            RM.UpdateGatesCost( new Vector2( -1, -1 ) ); // to prevent free gate bug   
    }    
    public void UpdateAfterMoveStuff()
    {
        Hero.Body.WoodAddedTurnCount = 0;
        UpdateHeroSprite();
        if( AdvanceTurn == false ) return;
        Hero.Body.UpdateSecondaryAttack( 4 );
    }
    public void UpdateHeroSprite()
    {
        int melee = 0;
        if( IsHeroMeleeAvailable() ) melee = 1;
        int ranged = ( int ) Item.GetNum( ItemType.Res_Bow_Arrow );
        if( Map.I.RM.RMD.LimitedArrowPerCube == -1 ) ranged = 1;
        if( G.Hero.Body.RangedAttackLevel < 1 ) ranged = 0;
        HeroFishingPoleSprite.gameObject.SetActive( false );
        HeroPickaxeSprite.gameObject.SetActive( false );
        HeroShieldSprite.gameObject.SetActive( false );
        G.Hero.Body.Shadow.gameObject.SetActive( true );

        float sc = 1;
        if( TurnTime <= .5f || Controller.SnowSliding || Controller.SandSliding )
        {
            Util.SmoothRotate( G.Hero.Spr.transform, G.Hero.Dir, 30 );                               // Animate hero rotation
            Util.SmoothRotate( G.Hero.Body.Shadow.transform, G.Hero.Dir, 30 );
        }

        if( Manager.I.GameType == EGameType.FARM )
            G.Farm.UpdateCarriedItemRotation();                                                      // Farm carried item rotation

        if( AdvanceTurn )
        {
            float z = -1.9f;
            if( Hero.Control.Floor >= 2 && Map.GMine( EMineType.BRIDGE, Hero.Pos ) ) z= -2f;         // hero over bridge z pos                              
            if( Hero.Control.Floor == 1 && Map.GMine( EMineType.LADDER, Hero.Pos ) ) z = -2f;        // hero over ladder z pos               
            if( Hero.Control.Floor == 4 ) z = -2f;  
            Hero.Spr.transform.localPosition = new Vector3( 0, 0, z );               // hero below bridge z pos
        }

        int sprite = 0;
        if( melee > 0 ) sprite = 2; else
        if( ranged > 0 ) sprite = 1;
        if( ranged <= 0 && melee <= 0 )
            sprite = 0;
        float tot = G.Hero.RangedAttack.GetRealtimeSpeedTime();

        if( sprite == 0 || RM.HeroSector.Type != Sector.ESectorType.NORMAL )       // no weapon 
        {
            int bs = 32;
            if( Hero.Control.LastMoveType != EMoveType.ROTATE )
            if( Util.IsEven( LevelTurnCount ) ) bs = 34;
            G.Hero.Spr.spriteId = 256 + bs;
            G.Hero.Body.Shadow.spriteId = 256 + bs + 1;
        }
        else
        if( sprite == 1 )   // Bow and arrow  
        {
            G.Hero.Spr.spriteId = 256;
            G.Hero.Body.Shadow.spriteId = 256 + 1;
            if( Map.Stepping() == false )
            if( G.Hero.RangedAttack.SpeedTimeCounter < Util.Percent( 75, tot ) )   // Bow shot
            {
                G.Hero.Spr.spriteId = 258;
                G.Hero.Body.Shadow.spriteId = 258 + 1;
            }
        }
        else
        if( sprite == 2 )    // Melee
        {
            G.Hero.Spr.spriteId = 256 + 64;
            G.Hero.Body.Shadow.spriteId = 256 + 64 + 1;
        }

        if( KickTimer > 0 )                                                                                       // Kick Sprite
        {
            G.Hero.Spr.spriteId = 293;
            EDirection mov = Util.GetTargetUnitDir( G.Hero.Control.OldPos, G.Hero.Pos );
            G.Hero.Spr.transform.eulerAngles = Util.GetRotationAngleVector( mov );
            G.Hero.Body.Shadow.gameObject.SetActive( false );
            sc = 1.4f;
        }
        KickTimer -= Time.deltaTime;


        List<Unit> ml = Util.GetFlyingNeighbors( G.Hero.Pos, ETileType.MINE );
        if( ml.Count > 0 )                                                                                         // Hero pickaxe
        if( Item.GetNum( ItemType.Res_Mining_Points ) >= 1 )
        if( GFU( ETileType.MINE, G.Hero.Pos ) == null )
        {
            G.Hero.Spr.spriteId = 258 + 64;
            G.Hero.Body.Shadow.spriteId = 258 + 64 + 1;
            HeroPickaxeSprite.gameObject.SetActive( true );
        }

        if( FishingMode != EFishingPhase.NO_FISHING )                                                             // Hero fishing
        {
            G.Hero.Spr.spriteId = 258 + 64;
            G.Hero.Body.Shadow.spriteId = 258 + 64 + 1;
            HeroFishingPoleSprite.gameObject.SetActive( true );
        }

        if( G.Hero.Body.InvulnerabilityFactor > 0 )                                                               // Invulnerability shield
        {
            G.Hero.Body.InvulnerabilityFactor -= Time.deltaTime;
            HeroShieldSprite.gameObject.SetActive( true );
            HeroShieldSprite.transform.Rotate( new Vector3( 0, 0, 30 * Time.deltaTime ) );
            if( G.Hero.Body.InvulnerabilityFactor <= 0 )
                MasterAudio.StopAllOfSound( "Electric Orb" );
        }
        
        if( Map.I.CubeDeath )                                                                                      // Dead Sprite
        {
            G.Hero.Spr.spriteId = 294;
            G.Hero.Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );
            G.Hero.Body.Shadow.gameObject.SetActive( false );
            sc = 1.2f;
            G.Hero.Graphic.transform.position = G.Hero.transform.position;
        }

        if( AdvanceTurn )                                                                                         // to avoid puch rotation bugs
        {
            G.Hero.RotateTo( G.Hero.Dir );
        }

        Hero.Spr.scale = new Vector2( sc, sc );                                                                   // sprite Scale

        //Unit pl = GetUnit( ETileType.TRAP, Hero.Pos );                                                            // Hero att speed bonus text info
        //if( pl == null )
        //{
        //    if( Map.I.HeroAttackSpeedBonus > 0 )
        //    {
        //        G.Hero.LevelTxt.gameObject.SetActive( true );
        //        if( Map.I.HeroAttackSpeedBonus > 0 )
        //        {

        //            G.Hero.LevelTxt.text = "" + Map.I.HeroAttackSpeedBonus.ToString( "0." ) + "%";
        //        }
        //    }
        //    else
        //    {
        //        if( GetUnit( ETileType.SNOW, Hero.Pos ) == null )
        //            G.Hero.LevelTxt.text = "";
        //    }
        //}
        Spell.UpdateSprites( G.Hero );                                                             // Update Spell Sprites
    }
    public bool SetHeroDeathTimer( float time, bool sprite = true )
    {
        if( G.Hero.Body.InvulnerabilityFactor <= 0 )
        {
            HeroDeathTimer = time;
            G.Hero.Body.RigidBody.velocity = Vector2.zero;
            Controller.InputVectorList = new List<Vector3>();
            Controller.InputVector = Vector2.zero;
            InvalidateInputTimer = .4f;
            if( sprite )
            {
                G.Hero.Spr.gameObject.SetActive( false );                                                             // disables sprite for better animation
                G.Hero.Body.Shadow.gameObject.SetActive( false );
            }
            return true;
        }
        return false;
    }
    public void StartCubeDeath( bool bloodfx = true, bool showBloodOnSprPosition = false, string fx = "Hero Death" )
    {
        if( Manager.I.GameType == EGameType.NAVIGATION ) return;
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf ) return;                                 // to avoid navigation map dying bug
        G.Hero.Body.RigidBody.velocity = Vector2.zero;
        Controller.InputVectorList = new List<Vector3>();
        Controller.InputVector = Vector2.zero;
        Map.I.InvalidateInputTimer = .4f;
        Map.I.CubeDeath = true;
        G.Hero.Spr.spriteId = 294;
        G.Hero.Spr.transform.eulerAngles = Vector2.zero;

        if( fx != "" )
            MasterAudio.PlaySound3DAtVector3( fx, G.Hero.transform.position );
        UI.I.EnableOverlay( new Color( 1, 0, 0, .4f ), 1.5f );
        Vector2 pt = G.Hero.Spr.transform.position;
        if( showBloodOnSprPosition == false )
            pt = G.Hero.transform.position;
        if( bloodfx )
        {
            G.Hero.Body.CreateBloodSpilling( pt );                          // Blood FX
            G.Hero.Body.CreateBloodSpilling( pt );                          // Blood FX
            G.Hero.Body.CreateBloodSpilling( pt );                          // Blood FX
            Body.CreateBloodSplat( pt );
        }
        for( int i = 0; i < G.Hero.Body.RopeConnectFather.Count; i++ )                             // Destroys connected shackle ball
        {
            Unit un = G.Hero.Body.RopeConnectFather[ i ];
            if( un.TileID == ETileType.MINE )
            if( un.Body.RopeConnectSon.Body.OriginalHookType == EMineType.SHACKLE )
                un.Kill();
        }
    }

    public void UpdateMouseHelp()
    {
        if( Gaia2 == null ) return;
        MouseStoppedTime += Time.deltaTime;

        if( Input.mousePosition != OldMousePosition )
            MouseStoppedTime = 0;

        OldMousePosition = Input.mousePosition;       
        if( MouseStoppedTime > 2 )
            Cursor.visible = false;
        else
            Cursor.visible = true;

        if( Cursor.visible )
        {
            Unit un = GetUnit( ETileType.ITEM, new Vector2( Mtx, Mty ) );
            if( un == null )
                un = GetUnit(ETileType.PLAGUE_MONSTER, new Vector2( Mtx, Mty ) );
            if( un )
            if( un.Variation >= 0 )
            {
                Item itm = G.GIT( un.Variation );

                string add = "";
                if( itm.Type == ItemType.Res_TRIGGER )
                    add = "_" + ( ( int ) un.Body.ResTriggerType + 1 );                                           // Several triggers types

                string txt = itm.GetName() + ":\n";
                txt += Language.Get( itm.Type.ToString().ToUpper() + "_DESCRIPTION"+ add, "Inventory" );
                txt = txt.Replace( "\\n", "\n" );
                if( Manager.I.GameType == EGameType.FARM )
                if( Farm.BuildingUI.activeSelf ) txt = "";

                if( un.Body.IsChest() )                                                                           // Chest Stepping
                {
                    string snd = "";
                    txt = "Chest Contents:\n";
                    Chests.GetChestText( un, ref txt, ref snd, un, false );
                    un.LevelTxt.text = txt;  
                    un.LevelTxt.gameObject.SetActive( true );
                    txt = "";
                }
                UI.I.SetTurnInfoText( txt, 1, Color.yellow );
            }

            un = GetUnit( ETileType.SAVEGAME, new Vector2( Mtx, Mty ) );
            if( un  )
            {
                un.Body.Sprite2.gameObject.SetActive( true );
                un.RightText.gameObject.SetActive( true );
            }
        }
    }

    //_____________________________________________________________________________________________________________________ Update ScrollText

    public void UpdateScrollText()
    {
        if( Gaia2 == null ) return;
        if( UI.I.ScrollText.gameObject.activeSelf ) 
            UI.I.ScrollText.gameObject.SetActive( false );

        if( Quest.I.UpdateArtifactMouseOverInfo( new Vector2( ( int ) Mtx, ( int ) Mty ) ) ) return;

        if( PtOnMap( Tilemap, Hero.Pos ) )
		if( Gaia2[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ] && 
		    Gaia2[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ].TileID == ETileType.SCROLL )
        {
            ScrollAnimationTimer += Time.deltaTime * 400;

            UI.I.ScrollText.gameObject.SetActive( true );

            string sheet = "Scrolls";
            string key = "SCROLL L" + ( Quest.CurrentLevel + 1 ).ToString( "0." ) +
                               " " + ( int ) Hero.Pos.x + " " + ( int ) Hero.Pos.y;

            if( Manager.I.GameType == EGameType.NAVIGATION )
            {
                key = "SCROLL " + ( int ) Hero.Pos.x + " " + ( int ) Hero.Pos.y;                                                     // Navigation Map
                sheet = "Navigation Map";
            }

            if( Manager.I.GameType == EGameType.CUBES )
            {
                if( RM.HeroSector.Type == Sector.ESectorType.NORMAL )                                                               // Cube
                {
                    key = GetAdv().QuestHelper.Signature + "_CUBE_" + RM.HeroSector.Number;
                    sheet = "Cube Scrolls";
                }

                EModType mod = Mod.GetModInTile( G.Hero.Pos );                                                                      // Scroll With Modifier 
                if( mod != EModType.NONE )
                {
                    key += "_" + mod.ToString();
                }
            }

            if( Manager.I.GameType == EGameType.FARM )                                                                              // Farm
            {
                key = "TUTORIAL_" + G.Tutorial.Phase;
                sheet = "Tutorial";
            }

            string text = Language.Get( key, sheet );

            if( Manager.I.GameType == EGameType.CUBES )
                if( RM.HeroSector.Type == Sector.ESectorType.LAB )
                {
                    text = text.Insert( 0, "WQ Cubes Random Tips:\n\n" );
                }

            text = text.Replace( "\\n", "\n" );

            //UI.I.ScrollText.color = new Color( 1, 1, 1, 1 );

            int size = ( int ) ScrollAnimationTimer;

            char[] chars = text.ToCharArray();

            for( int i = 0; i < text.Length; i++ )
                if( i > size ) chars[ i ] = ' ';

            UI.I.ScrollText.text = new string( chars );
            Manager.I.Tutorial.ScrollChecked = true;

            if( AdvanceTurn )
            {
                MapSaver.I.UpdateIntroMessage( text, "Scroll Text:" );                                                            // Displays panel message
                MasterAudio.PlaySound3DAtVector3( "Open Scroll", transform.position );
            }
        }
        else
            ScrollAnimationTimer = 0;
    }
    
	//_____________________________________________________________________________________________________________________ Updates Numbers

    public void UpdateNumbersData()
	{
        if( Unit == null ) return;
        SessionTime += Time.unscaledDeltaTime;
        if( CountRecordTime && Map.I.IsPaused() == false )                                                                 // record time increment
            RecordTime += Time.deltaTime;

        Controller.BarricadeBumpTimeCount += Time.deltaTime;
        if( SessionFrameCount == 0 ) SessionTime = 0;
        SessionFrameCount++;
        if( Manager.I.GameType == EGameType.CUBES )
        Manager.I.CubesTotalTime += Time.unscaledDeltaTime;
        if( LevelStats.NormalSectorsDiscovered <= 0 )
            Map.I.FirstCubeDiscoveredTime += Time.deltaTime;

        FPS = ( int ) ( 1f / Time.deltaTime );
        InvalidateInputTimer -= Time.deltaTime;
        int oldvalid = NumValidMonsters;
        NumValidMonsters = 0;
        NumNonGlueyMonsters = 0;
        NumRealtimeMonsters = 0;
        NumSteppingMonsters = 0;
        NumBrains = 0;
        int snow = 0, lava = 0;
        Map.I.HeadShotGhost.color = new Color( Map.I.HeadShotGhost.color.r,                                                        // Head Shot Ghost sprite alpha fade
        Map.I.HeadShotGhost.color.g, Map.I.HeadShotGhost.color.b, 
        Map.I.HeadShotGhost.color.a - ( Time.deltaTime * .12f ) );
        ResourceIndicator.MonsterGatesCount = 0;
        ResourceIndicator.FirepitPointsCount = 0;
        if( Map.I.CurrentArea != -1 )
        {
            if( CurArea.AreaTurnCount == 0 ) MonstersTotHpSum = 0;
            CurArea.BonfiresLit = 0;
            CurArea.TouchedBarricadeCount = 0;
            CurArea.BonfireCount = 0;
        }

        MonstersTotHpSum = 0;
        MonstersHpSum = 0;
        NumMonstersOverArrows = 0;
        NumWoundedMonsters = 0;
        Area.TotalValidAreas = 0;
        SpiderCount = MineCount = FireBallCount = FanCount = 0;

        if( Manager.I.GameType == EGameType.CUBES )
        if( G.HS.Type == Sector.ESectorType.NORMAL )
        if( G.HS.CubeFrameCount == 5 )
        {
            for( int i = 0; i < CustomSpeedRaftPos.Count; i++ )                                             // Initializes custom raft speed
            {
                Vector2 tg = CustomSpeedRaftPos[ i ];
                Unit raft = Controller.GetRaft( tg );
                List<Unit> ral = Controller.GetRaftList( raft.Control.RaftGroupID );
                for( int j = 0; j < ral.Count; j++ )
                    ral[ j ].Control.RealtimeSpeed = raft.Control.RealtimeSpeed;
            }
            CustomSpeedRaftPos = new List<Vector2>();

            for( int i = 0; i < CustomSpeedFogPos.Count; i++ )                                             // Initializes custom fog speed
            {
                Vector2 tg = CustomSpeedFogPos[ i ];
                Unit fog = Controller.GetFog( tg );
                List<Unit> ral = Controller.GetFogList( fog.Control.RaftGroupID );
                for( int j = 0; j < ral.Count; j++ )
                    ral[ j ].Control.RealtimeSpeed = fog.Control.RealtimeSpeed;
            }
            CustomSpeedFogPos = new List<Vector2>();
        }
 
        Sector s = Map.I.RM.HeroSector;
        if( s && s.MoveOrder != null )
        for( int i = 0; i < s.MoveOrder.Count; i++ )                                                   // move other stuf to here fo optimization
        {
            if( s.MoveOrder[ i ].TileID == ETileType.SPIDER )
                SpiderCount++;
        }

        for( int a = 0; a < Quest.I.CurLevel.AreaList.Count; a++ )
        {
            Area ar = Quest.I.CurLevel.AreaList[ a ];
            if( ar.IsFake == false ) Area.TotalValidAreas++;
            ar.BonfiresLit = 0;
            ar.BonfireCount = 0;
        }

        if ( s && s.Type == Sector.ESectorType.NORMAL )
        for( int y = ( int ) s.Area.yMin - 1; y < s.Area.yMax + 1; y++ )
        for( int x = ( int ) s.Area.xMin - 1; x < s.Area.xMax + 1; x++ )
            {
                if( Gaia[ x, y ] )
                {
                    if( Gaia[ x, y ].TileID == ETileType.SNOW ) snow++;
                    if( Gaia[ x, y ].TileID == ETileType.LAVA )
                    {
                        if( Random.value < ( CubeData.I.LavaParticleForce * .1 ) * Time.deltaTime )
                        {
                            if( Map.I.LavaParticle.gameObject.activeSelf == false )
                                Map.I.LavaParticle.gameObject.SetActive( true );
                            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
                            float randX = Random.Range( -0.5f, 0.5f );
                            float randY = Random.Range( -0.5f, 0.5f );
                            Vector3 worldPos = new Vector3( x + randX, y + randY, 0f );
                            emitParams.position = worldPos;
                            emitParams.velocity = new Vector3( Random.Range( -0.9f, 0.9f ), Random.Range( -0.5f, 2.5f ), 0f );                   // lava particle emmision
                            Map.I.LavaParticle.Emit( emitParams, 1 );
                        }
                    }
                }

                if( Gaia2[ x, y ] != null )
                {
                    if( Gaia2[ x, y ].TileID == ETileType.ITEM )
                    {
                        if( Gaia2[ x, y ].Variation == ( int ) ItemType.Res_Monster_Kill )
                            ResourceIndicator.MonsterGatesCount++;
                        if( Gaia2[ x, y ].Variation == ( int ) ItemType.Res_Fire_Lits )
                            ResourceIndicator.FirepitPointsCount++;
                    }

                    if( Gaia2[ x, y ].TileID == ETileType.FIRE )
                    {
                        int a = GetPosArea( new Vector2( x, y ) );

                        if( Gaia2[ x, y ].Body.FireIsOn )
                        {
                            if( a != -1 )
                                Quest.I.CurLevel.AreaList[ a ].BonfiresLit++;
                        }
                        if( a != -1 ) Quest.I.CurLevel.AreaList[ a ].BonfireCount++;

                        for( int d = 0; d < Gaia2[ x, y ].Body.WoodList.Length; d++ )                                                               // show fire logs
                        {
                            Gaia2[ x, y ].Body.WoodList[ d ].gameObject.SetActive( false );
                            if( Gaia2[ x, y ].Body.FireIsOn == false )
                                if( Gaia2[ x, y ].Body.WoodAdded[ d ] )
                                    Gaia2[ x, y ].Body.WoodList[ d ].gameObject.SetActive( true );

                            if( Gaia2[ x, y ].Body.AvailableFireHits != -1 )
                            if( Gaia2[ x, y ].Body.FireIsOn == true )
                            {
                                Gaia2[ x, y ].Body.WoodList[ d ].gameObject.SetActive( false );
                                if( Gaia2[ x, y ].Body.AvailableFireHits > d )
                                    Gaia2[ x, y ].Body.WoodList[ d ].gameObject.SetActive( true );
                            }
                        }
                    }
                }

                if( Unit[ x, y ] != null )
                {
                    Unit[ x, y ].UpdateColor();
                    Unit[ x, y ].UpdateAnimation();
                }

                if ( FUnit[ x, y ] != null )
                if ( FUnit[ x, y ].Count > 0 )
                for( int i = 0; i < Map.I.FUnit[ x, y ].Count; i++ )
                {
                    if(  Map.I.FUnit[ x, y ][ i ].TileID == ETileType.RAFT              ||
                         Map.I.FUnit[ x, y ][ i ].TileID == ETileType.ALGAE             ||
                         Map.I.FUnit[ x, y ][ i ].ProjType( EProjectileType.BOOMERANG ) ||
                         Map.I.FUnit[ x, y ][ i ].ProjType( EProjectileType.FIREBALL )  ||
                         Map.I.FUnit[ x, y ][ i ].TileID == ETileType.VINES             ||
                         Map.I.FUnit[ x, y ][ i ].TileID == ETileType.SPIKES            ||
                         Map.I.FUnit[ x, y ][ i ].TileID == ETileType.FISHING_POLE      )
                         Map.I.FUnit[ x, y ][ i ].UpdateAnimation();


                    //if( Map.I.FUnit[ x, y ][ i ].TileID == ETileType.MINE )
                    //{
                    //    Map.I.FUnit[ x, y ][ i ].Control.UpdateLadderDirection();
                    //    Map.I.FUnit[ x, y ][ i ].Control.UpdateBridgeDestruction();
                    //}

                    if( Map.I.FUnit[ x, y ][ i ].Control.WakeupTimeCounter > 0 )
                    {
                        Map.I.FUnit[ x, y ][ i ].Control.WakeupTimeCounter -= Time.deltaTime;
                        if( Map.I.FUnit[ x, y ][ i ].Control.WakeupTimeCounter <= 0 )
                            Map.I.FUnit[ x, y ][ i ].WakeMeUp();
                    }
                    if( Map.I.FUnit[ x, y ][ i ].TileID == ETileType.ALGAE )
                        BabyData.UpdateAlgae( Map.I.FUnit[ x, y ][ i ] );
                }

                if( Map.I.CurrentArea != -1 )
                if( Gaia2[ x, y ] != null )
                {
                    if( Gaia2[ x, y ].TileID == ETileType.FIRE )
                    {
                        if( Gaia2[ x, y ].Body.FireIsOn ) CurArea.BonfiresLit++;
                        CurArea.BonfireCount++;
                    }
                }

                if( Gaia2[ x, y ] != null )
                {
                    Gaia2[ x, y ].UpdateRightText();
                    Gaia2[ x, y ].UpdateText();
                    Gaia2[ x, y ].UpdateColor();                    // optimize to update only cube objs
                    Gaia2[ x, y ].UpdateAnimation();
                }

                if( Unit[ x, y ] != null )
                {
                    if( Unit[ x, y ].TileID == ETileType.BRAIN )
                        NumBrains++;

                    if( Unit[ x, y ].TileID == ETileType.ALTAR )
                        Unit[ x, y ].Altar.UpdateAltar();

                    if( Unit[ x, y ].Control.WakeupTimeCounter > 0 )
                    {
                        Unit[ x, y ].Control.WakeupTimeCounter -= Time.deltaTime;
                        if( Unit[ x, y ].Control.WakeupTimeCounter <= 0 )
                            Unit[ x, y ].WakeMeUp();
                    }

                    if( Map.I.CurrentArea != -1 )
                    if( Unit[ x, y ].TileID == ETileType.BARRICADE )
                        if( Unit[ x, y ].Body.TouchCount > 0 )
                            CurArea.TouchedBarricadeCount++;

                    Unit[ x, y ].UpdateRightText();

                    if( Unit[ x, y ].ValidMonster )
                    {
                        if( Unit[ x, y ].Body.Hp != Unit[ x, y ].Body.TotHp ) NumWoundedMonsters++;

                        if( Gaia2[ x, y ] != null )
                            if( Gaia2[ x, y ].TileID == ETileType.ARROW ) NumMonstersOverArrows++;
                        Unit[ x, y ].Control.IsBeingPushedByHero = false;

                        if( Unit[ x, y ].Body.IsTiny )                                                                                    // Tiny monster sprite FX
                        {
                            Unit forest = GetUnit( ETileType.FOREST, new Vector2( x, y ) );
                            Unit[ x, y ].Spr.transform.localScale = new Vector3( .75f, .75f, 1f );
                            if( forest )
                                Unit[ x, y ].Spr.color = new Color( 1, 1, 1, .3f );
                            else
                                Unit[ x, y ].Spr.color = new Color( 1, 1, 1, 1 );
                        }
                        else
                            Unit[ x, y ].Spr.transform.localScale = new Vector3( 1, 1, 1 );

                        if( GetPosArea( new Vector2( x, y ) ) == CurrentArea )
                        {
                            if( Unit[ x, y ].Body.IsDead == false )
                            {
                                NumValidMonsters++;

                                if( Unit[ x, y ].Control.RealtimeSpeedFactor > 0 )
                                    NumRealtimeMonsters++;
                                else
                                    NumSteppingMonsters++;
                            }

                            if( Map.I.CurrentArea != -1 )                                               // Mosters tot hp
                            {
                                MonstersHpSum += Unit[ x, y ].Body.Hp;
                                MonstersTotHpSum += Unit[ x, y ].Body.TotHp;
                            }
                        }

                        if( Unit[ x, y ].TileID != ETileType.GLUEY ) NumNonGlueyMonsters++;
                    }
                }
            }
        Mine.UpdateAllMinesText = false;
        if( NumScorpions > 0 )                                                                        // This is a hack to fix the bug that keeps disabling hero bamboo sprite
            Map.I.UpdateHeroSprite();

        UpdateWeatherFX( snow );                                                                      // weather effects
    }
    
    private int oldZoom = -1; 
    private float snowTimer = 0f;
    private float snowStart = 0f;  // valor inicial da neve
    private float snowTarget = 0f; // valor alvo atual
    public static string[] rainMusic = {
            "forest_1", "water flowing", "Rain Strong",
            "forest stream", "River birds", "monkey",
            "bamboo creaking", "Early Rain"
             };
    private void UpdateWeatherFX( int snow )
    {
        if( Manager.I.GameType != EGameType.CUBES ) return;
        // Ajusta alpha dependendo do zoom
        if( oldZoom != ZoomMode )
        {
            if( RainRenderer == null )
                RainRenderer = RainParticle.GetComponent<ParticleSystemRenderer>();

            float val = 0.5f;
            if( Map.I.ZoomMode == 3 ) val = 0f;
            else if( Map.I.ZoomMode == 2 ) val = 0.3f;
            else if( Map.I.ZoomMode == 1 ) val = 0.4f;

            Color col = new Color( 1f, 1f, 1f, val );
            RainRenderer.material.SetColor( "_TintColor", col );
            oldZoom = ZoomMode;
        }

        CubeData.UpdateParticles = false;
        ParticleSystem rainPS = Map.I.RainParticle;
        ParticleSystem snowPS = Map.I.SnowParticle;
        ParticleSystem windPS = Map.I.WindParticle;
        AudioClip music = Manager.I.PlaylistController.CurrentPlaylistClip;

        // Ativa partículas principais
        if( G.HS.CubeFrameCount == 12 )
        {
            snowPS.gameObject.SetActive( true );
            rainPS.gameObject.SetActive( true );
            windPS.gameObject.SetActive( true );
            FireFliesParticle.gameObject.SetActive( true );
            FogParticle.gameObject.SetActive( true );
        }

        // ================= CHUVA =================
        float rainSpeed = 100f;
        var rainMain = rainPS.main;
        var rainLimit = rainPS.limitVelocityOverLifetime;
        var rainEm = rainPS.emission;
        var rainRate = rainEm.rateOverTime;

        float timeLeft = 0;
        AudioSource audioSrc = Manager.I.PlaylistController.ActiveAudioSource;
        if( audioSrc != null && music != null )
        {
            float musicTime = audioSrc.time;   // tempo atual em segundos
            float musicLength = music.length;  // duração total
            timeLeft = musicLength - musicTime;  // tempo restante
        }

        if( music != null && System.Array.Exists( rainMusic, name => name == music.name ) && timeLeft > 10 )
        {
            rainLimit.enabled = true;
            float targetRate = ( music.name == "Rain Strong" ) ? 1000f : 300f;

            rainRate.constant = Mathf.MoveTowards( rainRate.constant, targetRate, rainSpeed * Time.deltaTime );
            rainEm.rateOverTime = rainRate;

            if( music.name == "Rain Strong" )
            {
                rainLimit.limit = new ParticleSystem.MinMaxCurve( 55f, 88f );
                rainMain.startLifetime = new ParticleSystem.MinMaxCurve( 2f, 6f );
            }
            else
            {
                rainLimit.limit = new ParticleSystem.MinMaxCurve( 25f, 38f );
                rainMain.startLifetime = new ParticleSystem.MinMaxCurve( 2f, 12f );
            }
        }
        else
        {
            rainRate.constant = Mathf.MoveTowards( rainRate.constant, 0f, rainSpeed * Time.deltaTime );
            rainEm.rateOverTime = rainRate;
        }

        // ================= NEVE =================
        var snowEm = snowPS.emission;
        var rate = snowEm.rateOverTime;

        float newTarget = ( snow > 20 ) ? CubeData.I.SnowStrenght : 0f;

        // Se o target mudou, reinicia o timer
        if( Mathf.Abs( newTarget - snowTarget ) > 0.01f )
        {
            snowStart = rate.constant;  // valor atual como início
            snowTarget = newTarget;
            snowTimer = 0f;
        }

        // duração da transição em segundos
        float snowDuration = 10f;
        snowTimer += Time.deltaTime;
        float t = Mathf.Clamp01( snowTimer / snowDuration );

        // interpola suavemente
        rate.constant = Mathf.Lerp( snowStart, snowTarget, t );
        snowEm.rateOverTime = rate;

        // ================= VENTO =================
        float windSpeed = 15f;
        var windEm = windPS.emission;
        var windRate = windEm.rateOverTime;
        var windMain = windPS.main;
        var windLimit = windPS.limitVelocityOverLifetime;

        if( music != null && music.name == "Scary Ambient Wind" )
        {
            windLimit.enabled = true;
            float windTarget = 100f;

            windRate.constant = Mathf.MoveTowards( windRate.constant, windTarget, windSpeed * Time.deltaTime );
            windEm.rateOverTime = windRate;

            windLimit.limit = new ParticleSystem.MinMaxCurve( 20f, 40f );
            windMain.startLifetime = new ParticleSystem.MinMaxCurve( 2f, 5f );
        }
        else
        {
            windRate.constant = Mathf.MoveTowards( windRate.constant, 0f, windSpeed * Time.deltaTime );
            windEm.rateOverTime = windRate;
        }

        // ================= LUZES/OUTRAS PARTICULAS =================
        var firefliesEm = FireFliesParticle.emission;
        ParticleSystem.MinMaxCurve firefliesRate = firefliesEm.rateOverTime;
        float firefliesSpeed = 200;

        if( music != null &&
           ( music.name == "frogcave" || music.name == "Spooky Island" || music.name == "Night" || CubeData.I.ForceFireflies ) )
        {
            float firefliesTarget = CubeData.I.FirefliesAmount;
            firefliesRate.constant = Mathf.MoveTowards( firefliesRate.constant, firefliesTarget, firefliesSpeed * Time.deltaTime );
        }
        else
        {
            float firefliesTarget = 0f;
            firefliesRate.constant = Mathf.MoveTowards( firefliesRate.constant, firefliesTarget, firefliesSpeed * Time.deltaTime );
        }
        firefliesEm.rateOverTime = firefliesRate;

        // ================= FOG =================
        var fogEm = Map.I.FogParticle.emission;
        var fogRate = fogEm.rateOverTime;
        float fogSpeed = 200f;
        float fogTarget = 0f;
        if( CubeData.I.FogIntensity > 0 )
            fogTarget = Util.GetPercent( CubeData.I.FogIntensity, 1, 3000 );
        fogRate.constant = Mathf.MoveTowards( fogRate.constant, fogTarget, fogSpeed * Time.deltaTime );
        fogEm.rateOverTime = fogRate;
        var fcol = Map.I.FogParticle.colorOverLifetime;
        var g = fcol.color.gradient;
        var colorKeys = g.colorKeys;
        var alphaKeys = g.alphaKeys;
        alphaKeys[ 1 ].alpha = CubeData.I.FogAlpha / 300f;
        colorKeys[ 1 ].color = CubeData.I.FogColor;
        g.SetKeys( colorKeys, alphaKeys );
        fcol.color = new ParticleSystem.MinMaxGradient( g );
    }

    public bool IsPaused()
    {
        if( OxygenActive ) return true;                               // Oxygen Pause
        if( Spell.AddSpellOrigin.x != -1 ) return true;               // Spell Pause
        if( GS.CubeLoaded ) return true;                              // Cube has just been loaded
        if( Mine.TunnelTraveling ) return true;                       // Tunnel Traveling
        if( GetUnit( ETileType.SCROLL, G.Hero.Pos ) ) return true;    // Hero over Scroll
        return false;
    }

	//_____________________________________________________________________________________________________________________ Clears all Monsters in all areas
	
	public void ClearAllMonstersInAllAreas( bool onlyIfCleared )
	{
        for( int a = 0; a < Quest.I.CurLevel.AreaList.Count; a++ )
			if( onlyIfCleared == false || Quest.I.CurLevel.AreaList[ a ].Cleared )   
	         ClearAllMonstersInTheArea( a, true, false, true );  
    }

	//_____________________________________________________________________________________________________________________ Update All Area Colors

	public void UpdateAllAreaColors()
	{
		for( int a = 0; a < Quest.I.CurLevel.AreaList.Count; a++ )
			UpdateAreaColor( a );
	}

	//_____________________________________________________________________________________________________________________ Clears all Monsters in the area

	public void ClearAllMonstersInTheArea( int area, bool onlyValid, bool clearInactives, bool barricades )
    {
		Rect r = Quest.I.CurLevel.AreaList[ area ].AreaRect;
		Vector2 p1 = new Vector2( ( int ) ( r.x ), ( int ) ( r.y ) );
		Vector2 p2 = new Vector2( ( int ) ( r.x + r.width - 1 ), ( int ) ( r.y - r.height + 1 ) );

        for( int y = ( int ) p2.y; y <= p1.y; y++ )
        for( int x = ( int ) p1.x; x <= p2.x; x++ )
        if  ( AreaID[ x, y ] == area )
		{
			if( Unit[ x, y ] != null )
			{
				if( Unit[ x, y ].UnitType == EUnitType.MONSTER )
				{
                    if(!barricades && Unit[ x, y ].TileID == ETileType.BARRICADE ) {} else
					if( Unit[ x, y ].ValidMonster || onlyValid == false ) Unit[ x, y ].Kill();
				}
				else
				if( clearInactives && Unit[ x, y ].UnitType == EUnitType.HERO ) Unit[ x, y ].Kill();
			}
		}	
    }
   

    public void ClearRoomDoorAtPos( Vector2 cubecord, bool allcube = false )
	{
        Vector2 pos = new Vector2( Map.I.RM.HeroSector.Area.xMin + cubecord.x, 
                                   Map.I.RM.HeroSector.Area.yMin + cubecord.y );
        if( allcube == false )
        {
            if( PtOnMap( Tilemap, pos ) == false ) return;
            if( Gaia[ ( int ) pos.x, ( int ) pos.y ] == null ) return;
            if( Gaia[ ( int ) pos.x, ( int ) pos.y ].TileID != ETileType.ROOMDOOR ) return;
        }

        int id = GateID[ ( int ) pos.x, ( int ) pos.y ];
        Sector hs = RM.HeroSector;
        for( int y = ( int ) hs.Area.yMin - 1; y < hs.Area.yMax + 1; y++ )
        for( int x = ( int ) hs.Area.xMin - 1; x < hs.Area.xMax + 1; x++ )
            {
             if( Gaia[ x, y ] != null )
             if( Gaia[ x, y ].TileID == ETileType.ROOMDOOR )
             if( allcube || GateID[ x, y ] == id )
             {
                 int group = GateID[ x, y ];
                 SetTile( x, y, ELayerType.GAIA, ETileType.OPENROOMDOOR, true );                              // clears room door
                 GateID[ x, y ] = group;
                 AddTrans( new VI( x, y ) );
                 Map.I.CreateExplosionFX( new Vector2( x, y ), "Fire Explosion", "" );                        // Smoke Cloud FX
             }

             Unit tgvn = Map.GFU( ETileType.VINES, new Vector2( x, y ) );                                     // Burning Vine in position? Destroys them
             if( tgvn && tgvn.Body.FireIsOn )
                 tgvn.Kill();
            }
	}


	//____________________________________________________________________________________________________________ Set Tile

    public void SetTile( int x, int y, ELayerType layer, ETileType tile, bool usetrans, bool updateTilemap = false )
    {
        if( layer == ELayerType.GAIA )
        {
            if( Gaia[ x, y ] ) Gaia[ x, y ].Kill();
            if( updateTilemap ) Tilemap.SetTile( x, y, ( int ) layer, ( int ) tile );
        }
        else
            if( layer == ELayerType.GAIA2 )
            {
                if( Gaia2[ x, y ] ) Gaia2[ x, y ].Kill();
                if( updateTilemap ) Tilemap.SetTile( x, y, ( int ) layer, ( int ) tile );

            }
            else return;

        if( tile != ETileType.NONE )
            UpdateTileGameObjectCreation( x, y, layer, tile );

        if( usetrans )
        {
			AddTrans( new VI( x, y ) );
        }
        else
        {
            UpdateTilemap = true;
        }
    }

	//_________________________________________________________________________________________________ Update Debug

	void UpdateDebug()
	{
        if( RM.DungeonDialog.gameObject.activeSelf ) return;
		UpdateMouseTile ();
        Helper.I.UpdateDebug();
        return;
        TransTileMap.Layers[ 3 ].gameObject.transform.localPosition = new Vector3( 0, 0, 0.9f );

        if( UI.I.LoadingLevelText.gameObject.activeSelf ) UI.I.LoadingLevelText.gameObject.SetActive( false );

        if( DebugPage > 0 )
        {
            Sector s = RM.HeroSector;
            if( s && s.Type == Sector.ESectorType.NORMAL )
                for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )
                    for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
                    {
                        if( TileText[ xx, yy ] == null )
                        {
                            GameObject go = Manager.I.CreateObjInstance( "Tile Text", "Tile Text " + xx + " " + yy, EDirection.NONE, new Vector3( xx, yy, 0 ) );
                            TileText[ xx, yy ] = go.GetComponent<tk2dTextMesh>();
                        }
                    }
        }
        else return;

        Sector sc = RM.HeroSector;
        if ( sc.Type == Sector.ESectorType.NORMAL )
        for( int yy = ( int ) sc.Area.yMin - 1; yy < sc.Area.yMax + 1; yy++ )
        for( int xx = ( int ) sc.Area.xMin - 1; xx < sc.Area.xMax + 1; xx++ )
		switch ( DebugPage )
		{
		case 1: 
		{
            TileText[ xx, yy ].text = "D = " + DistFromTarget[ xx, yy ].ToString( "0.00000" );              
		}
		break;
        case 2:
        {
            TileText[ xx, yy ].text = " " + ObjectID[ xx, yy ];                
        }
        break;
        case 3:
        {
            if( WaspDist != null )
                TileText[ xx, yy ].text = " " + WaspDist[ xx, yy ];
        }
        break;
        }

		if( Input.GetKey( KeyCode.LeftControl ) )
		if( Input.GetMouseButton( 1 ) )                                                                                            // Collect mouse over artifact
		{
			if( Mtx != -1 && PtOnMap( Tilemap, new Vector2( Mtx, Mty ) ) )
			{
				if( Quest.I.UpdateArtifactStepping( new Vector2( Mtx, Mty ) ) ) return;
			}
		}
	}

	//_________________________________________________________________________________________________ Finalize Map and destroy all objects

	public void Finalize(  bool over = false )
    {
        Finalizing = true;
        if( G.HS )
            G.HS.GiveCummulativeGlobalPrizes();                                      // Adds cummulative prizes like clover to inventory     
        PoolManager.Pools[ "Pool" ].DespawnAll();
        Map.I.StopAllLoopedSounds();
        Finalizing = false;
        Gaia = null;
        Gaia2 = null;
        Unit = null;
        TileText = null;
        if( over )
            for( int i = 0; i < Manager.I.Inventory.ItemList.Count; i++ )  // Warning: Dont use G.GIT here: the access is directly to the itemlist via loop and id    
                if( Manager.I.Inventory.ItemList[ i ] )
                {
                    Manager.I.Inventory.ItemList[ i ].StartingBonus = 0;
                    Manager.I.Inventory.ItemList[ i ].IsGlobalGameplayResource = false;
                }
        UI.I.GoalPanel.gameObject.SetActive( false );
        for( int i = 0; i < Map.I.HeroShieldSpriteList.Count; i++ )
            Map.I.HeroShieldSpriteList[ i ].gameObject.SetActive( false );
        for( int i = 0; i < Map.I.HeroBambooSpriteList.Count; i++ )
            Map.I.HeroBambooSpriteList[ i ].gameObject.SetActive( false );
        MapSaver.I.UpdateIntroMessage();
        GS.DeleteCubeSaves();
        ParticleSystem[] systems = {
        RainParticle,
        FireFliesParticle,
        WindParticle,
        SnowParticle };
        foreach( var ps in systems )
        {
            var em = ps.emission;
            em.rateOverTime = 0; // para emissão
            ps.Clear();          // limpa partículas vivas
        }

        if( Lights.lights.Count > 1 )
            Lights.lights.RemoveRange( 1, Map.I.Lights.lights.Count - 1 );           // Remove Lights   
        GS.RemoveFixedSpells();
    }
			
	//_____________________________________________________________________________________________________________________ Inits the Hero data

	public void InitHero( EHeroID hero, bool restartHP, bool init, bool resetSurplus, EDirection dir = EDirection.N )
	{
        //WindRose.gameObject.SetActive( true );
        //WindRose.gameObject.transform.position = LevelEntrancePosition;
        Vector2 pos = LevelEntrancePosition; 
        if( pos.x == -1 ) Debug.LogError( "Hero Start Pos not Set on level: " +
                                           Quest.CurrentLevel );        
        if( init ) ChangeHero( hero, false );
        Hero.Spr.transform.localPosition = new Vector3( 0, 0, -1.9f );
		Hero.Control.ApplyMove( new Vector2( -1, -1 ), pos );
        Hero.Control.OldPos = new Vector2( pos.x, pos.y );
        Hero.Control.LastPos = new Vector2( pos.x, pos.y );
        Hero.Control.AnimationOrigin = new Vector2( pos.x, pos.y );
        Hero.Spr.gameObject.SetActive( true );
        Hero.Spr.gameObject.SetActive( true );
        Hero.Body.Shadow.gameObject.SetActive( true );
		HeroIsDead = false;
        if( restartHP ) Hero.Body.Hp = Hero.Body.TotHp; 
		Hero.RotateTo( dir );
		Hero.Body.UpdateHealthBar();
		Hero.Body.UpdateUnitHealthStatus();
		Hero.Body.Lives = 1;
		Hero.Body.NumMushroom = 0;
        HeroSwordCollider.enabled = false;
        if( resetSurplus )
        {
            if( Hero.MeleeAttack  ) Hero.MeleeAttack.DamageSurplus  = Hero.MeleeAttack.OriginalDamageSurplus;
            if( Hero.RangedAttack ) Hero.RangedAttack.DamageSurplus = Hero.RangedAttack.OriginalDamageSurplus;
            if( Hero.MeleeAttack  ) Hero.MeleeAttack.RTDamageSurplus = Hero.MeleeAttack.OriginalRTDamageSurplus;
            if( Hero.RangedAttack ) Hero.RangedAttack.RTDamageSurplus = Hero.RangedAttack.OriginalRTDamageSurplus;
        }
        else
        {
            if( Hero.MeleeAttack  ) Hero.MeleeAttack.OriginalDamageSurplus  = Hero.MeleeAttack.DamageSurplus;
            if( Hero.RangedAttack ) Hero.RangedAttack.OriginalDamageSurplus = Hero.RangedAttack.DamageSurplus;
            if( Hero.MeleeAttack  ) Hero.MeleeAttack.OriginalRTDamageSurplus = Hero.MeleeAttack.RTDamageSurplus;
            if( Hero.RangedAttack ) Hero.RangedAttack.OriginalRTDamageSurplus = Hero.RangedAttack.RTDamageSurplus;
        }
        UI.I.UpdatePortrait( hero );
        for( int i = 0; i < G.Hero.Body.Sp.Count; i++ )
            G.Hero.Body.Sp[ i ].Reset();
	}	

    //_____________________________________________________________________________________________________________________ Returns variation num for TileID

    public int GetVariation( Vector2 tg, ELayerType layer )
    {
        int tile = Quest.I.CurLevel.Tilemap.GetTile( ( int ) tg.x, ( int ) tg.y, ( int ) layer ); 

        if( tile >= ( int ) ETileType.SPLITTER &&  tile < ( int ) ETileType.SPLITTER + Map.TotSplitters )
        {
            return ( int ) tile - ( int ) ETileType.SPLITTER;
        }

        if( tile >= ( int ) ETileType.DECOR_TALL && tile < ( int ) ETileType.DECOR_TALL + TotDecor )
        {
            return ( int ) tile - ( int ) ETileType.DECOR_TALL;
        }

        if( tile >= ( int ) ETileType.BARRICADE && tile < ( int ) ETileType.BARRICADE + Map.TotBarricade )
        {
            return ( int ) tile - ( int ) ETileType.BARRICADE;
        }

        if( tile >= ( int ) ETileType.DOOR_OPENER && tile < ( int ) ETileType.DOOR_OPENER + TotKeys )
        {
            return ( int ) tile - ( int ) ETileType.DOOR_OPENER;
        }

        if( tile >= ( int ) ETileType.DOOR_SWITCHER && tile < ( int ) ETileType.DOOR_SWITCHER + TotKeys )
        {
            return ( int ) tile - ( int ) ETileType.DOOR_SWITCHER;
        }

        if( tile >= ( int ) ETileType.DOOR_CLOSER && tile < ( int ) ETileType.DOOR_CLOSER + TotKeys )
        {
            return ( int ) tile - ( int ) ETileType.DOOR_CLOSER;
        }

        if( tile >= ( int ) ETileType.DOOR_KNOB && tile < ( int ) ETileType.DOOR_KNOB + TotKeys )
        {
            return ( int ) tile - ( int ) ETileType.DOOR_KNOB;
        }

        if( tile >= ( int ) ETileType.ORI3 && tile < ( int ) ETileType.ORI3 + Map.TotOri3 )
        {
            return ( int ) tile - ( int ) ETileType.ORI3;
        }
        if( tile >= 1408 && tile < 1408 + Map.TotOri3 )    // ori3 from bottom row
        {
            return ( int ) tile - ( int ) 1408;
        }
        if( tile >= ( int ) ETileType.INACTIVE_HERO && tile < ( int ) ETileType.INACTIVE_HERO + Map.TotHeroes )
        {
            return ( int ) tile - ( int ) ETileType.INACTIVE_HERO;
        }

        int[] aux = new int[] { -1, 0, 1, 127, 129, 255, 256, 257 };
        int[] varlist = new int[] { 7, 0, 1, 6, 2, 5, 4, 3 };

        for( int i = 0; i < aux.Length; i++ )
            if( ( int ) tile == ( int ) ETileType.ARROW + aux[ i ] )
            {
                return varlist[ i ];
            }

        for( int i = 0; i < aux.Length; i++ )
            if( ( int ) tile == ( int ) ETileType.BOULDER + aux[ i ] )
            {
                return varlist[ i ];
            }

        for( int i = 0; i < aux.Length; i++ )
            if( ( int ) tile == ( int ) ETileType.ORIENTATION + aux[ i ] )
            {
                return varlist[ i ];
            }

        if( tile >= 512 && tile <= 525 )    // ori from bottom row
        {
            return ( int ) tile - ( int ) 512 + 8;
        }
        if( tile == ( int ) ETileType.ITEM ) return Gaia2[ ( int ) tg.x, ( int ) tg.y ].Variation;
        return 0;
    }
    
    //_____________________________________________________________________________________________________________________ Returns TileID for each variation

    public static ETileType GetTileID( ETileType tile )
    {
        if( ( int ) tile >= ( int ) ETileType.SPLITTER && ( int ) tile < ( int ) ETileType.SPLITTER + Map.TotSplitters )
        {
            return ETileType.SPLITTER;
        }

        if( tile >= 0 && ( int ) tile < Map.I.Tilemap.data.tileInfo.Length )
        {
            var tileInfo = Map.I.Tilemap.data.tileInfo[ ( int ) tile ];
            if( tileInfo.Layer == ELayerType.GAIA )
                return ETileType.DECOR_GAIA;
        }

        if( ( int ) tile >= ( int ) ETileType.DECOR_TALL && ( int ) tile < ( int ) ETileType.DECOR_TALL + TotDecor )
        {
            return ETileType.DECOR_TALL;
        }

        if( ( int ) tile >= ( int ) ETileType.BARRICADE && ( int ) tile < ( int ) ETileType.BARRICADE + Map.TotBarricade )
        {
            return ETileType.BARRICADE;
        }

        if( ( int ) tile >= ( int ) ETileType.DOOR_OPENER && ( int ) tile < ( int ) ETileType.DOOR_OPENER + TotKeys )
        {
            return ETileType.DOOR_OPENER;
        }

        if( ( int ) tile >= ( int ) ETileType.DOOR_SWITCHER && ( int ) tile < ( int ) ETileType.DOOR_SWITCHER + TotKeys )
        {
            return ETileType.DOOR_SWITCHER;
        }

        if( ( int ) tile >= ( int ) ETileType.DOOR_CLOSER && ( int ) tile < ( int ) ETileType.DOOR_CLOSER + TotKeys )
        {
            return ETileType.DOOR_CLOSER;
        }

        if( ( int ) tile >= ( int ) ETileType.DOOR_KNOB && ( int ) tile < ( int ) ETileType.DOOR_KNOB + TotKeys )
        {
            return ETileType.DOOR_KNOB;
        }

        if( ( int ) tile >= ( int ) ETileType.ORI3 && ( int ) tile < ( int ) ETileType.ORI3 + Map.TotOri3 )
        {
            return ETileType.ORI3;
        }
        if( ( int ) tile >= 1408 && ( int ) tile < 1408 + Map.TotOri3 )       // bottom row
        {
            return ETileType.ORI3;
        }

        if( ( int ) tile >= ( int ) ETileType.INACTIVE_HERO && ( int ) tile < ( int ) ETileType.INACTIVE_HERO + Map.TotHeroes )
        {
            return ETileType.INACTIVE_HERO;
        }

        int[] aux = new int[] { -1, 0, 1, 127, 129, 255, 256, 257 };
        
        for( int i = 0; i < aux.Length; i++ )
            if( ( int ) tile == ( int ) ETileType.ARROW + aux[ i ] )
            {
                return ETileType.ARROW;
            }

        for( int i = 0; i < aux.Length; i++ )
            if( ( int ) tile == ( int ) ETileType.BOULDER + aux[ i ] )
            {
                return ETileType.BOULDER;
            }

        for( int i = 0; i < aux.Length; i++ )
            if( ( int ) tile == ( int ) ETileType.ORIENTATION + aux[ i ] )
            {
                return ETileType.ORIENTATION;
            }
        if( ( int ) tile >= 512 && ( int ) tile <= 525 )       // ori from bottom row
        {
            return ETileType.ORIENTATION;
        }
        return tile;
    }

    public Unit GetUnitPrefab( ETileType tile, ELayerType layer = ELayerType.NONE )
    {
        if( layer !=  ELayerType.GAIA ) // to optimize and avoid gettileid
            tile = GetTileID( tile );
        if( ( int ) tile > SharedObjectsList.Length - 1 ) return null;
        if( ( int ) tile < 0 ) return null;
        return SharedObjectsList[ ( int ) tile ];
    }
    
    public void ChangeHero( EHeroID id, bool playsound )
    {
        SelectedHero = id;
        float oldHp = Hero.Body.Hp;
        EMoveType oldemt = Hero.Control.LastMoveType;
        Hero.Copy( HeroList[ ( int ) id ], true, false, false );
        Hero.Body.Hp = oldHp;
        Hero.UnitType = EUnitType.HERO;
		Quest.I.UpdateArtifactData( ref Hero );
		if( playsound )
			MasterAudio.PlaySound3DAtVector3( "Hero Change", transform.position );
        Hero.Control.LastMoveType = oldemt;
	}

	//_____________________________________________________________________________________________________________________ Update Hero Changing 

    public void UpdateHeroChanging()
    {
        //if( Gaia[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ] )                                               // step on splitter
        //if( Gaia[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ].TileID == ETileType.SPLITTER )
        //    {
        //        if( Gaia2[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ] &&
        //            Gaia2[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ].TileID == ETileType.HEROCHANGER )      // splitter with changer, create an inactive hero
        //            {
        //                int id = Gaia2[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ].Variation;
        //                GameObject obj = CreateUnit( HeroList[ Hero.Variation ], Hero.Pos );
        //                Unit un = obj.GetComponent<Unit>();
        //                un.SetVariation( Hero.Variation );
        //                un.RotateTo( Hero.Dir );
        //                Quest.I.UpdateArtifactData( ref un );
        //                ChangeHero( ( EHeroID ) id, true );
        //                Gaia2[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ].Kill();
        //                return;
        //            }
        //        else
        //        for( int y = ( int ) Map.I.P2.y; y <= Map.I.P1.y; y++ )                                             // empty splitter, look for corresponding inactive
        //        for( int x = ( int ) Map.I.P1.x; x <= Map.I.P2.x; x++ )
        //        if ( AreaID[ x, y ] == CurrentArea )
        //        if ( Unit[ x, y ] )
        //        if ( Unit[ x, y ].TileID == ETileType.INACTIVE_HERO )
        //        if ( new Vector2( x, y ) != Hero.Pos )
        //        if ( Gaia[ x, y ] )                                              
        //        if ( Gaia[ x, y ].TileID == ETileType.SPLITTER )
        //        if ( Gaia[ x, y ].Variation == Gaia[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ].Variation )
        //        if ( Hero.Control.LastMoveType != EMoveType.STILL )
        //        if ( Hero.Control.LastMoveType != EMoveType.ROTATE )
        //            {
        //                if( Hero.Body.CooperationLevel < 2 )
        //                {
        //                    Map.I.ShowMessage( Language.Get( "ERROR_COOPERATION2" ) );
        //                    return;					
        //                }

        //                if( Hero.Body.CooperationLevel < 3 )
        //                {
        //                    if( IsInTheSameLine( new Vector2( x, y ), Hero.Pos ) == false ||
        //                        HasLOS( new Vector2( x, y ), Hero.Pos ) == false )
        //                    {
        //                        Map.I.ShowMessage( Language.Get( "ERROR_COOPERATION3" ) );
        //                        return;
        //                    }
        //                }

        //                if( Hero.Body.CooperationLevel < 4 )
        //                {
        //                    if( HasLOS( new Vector2( x, y ), Hero.Pos ) == false )
        //                    {
        //                        Map.I.ShowMessage( Language.Get( "ERROR_COOPERATION4" ) );
        //                        return;
        //                    }
        //                }

        //                int id = Unit[ x, y ].Variation;
        //                EDirection dr = Unit[ x, y ].Dir;
        //                Unit[ x, y ].Kill();
        //                GameObject obj = CreateUnit( HeroList[ Hero.Variation ], Hero.Pos );
        //                Unit un = obj.GetComponent<Unit>();
        //                un.RotateTo( Hero.Dir );
        //                un.SetVariation( Hero.Variation );
        //                Quest.I.UpdateArtifactData( ref un );
        //                Hero.Control.ApplyMove( Hero.Pos, new Vector2( x, y ) );
        //                Hero.RotateTo( dr );
        //                ChangeHero( ( EHeroID ) id, true );
        //                return;
        //            }
        //    }

        //if( !Gaia2[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ] ) return;                                       // no splitter, just change hero
        //if( Gaia2[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ].TileID != ETileType.HEROCHANGER ) return;
        //ChangeHero( (EHeroID) Gaia2[ ( int ) Hero.Pos.x, ( int ) Hero.Pos.y ].Variation, true );
    }

	public bool UpdateHelp()
	{
        return false;
		if( Input.GetKey( KeyCode.F1 ) == false ) return false;
		UI.I.ScrollText.gameObject.SetActive( true );
		UI.I.ScrollText.text = Language.Get( "HELPTEXT" );
		UI.I.ScrollText.text = UI.I.ScrollText.text.Replace( "\\n", "\n" );
		UI.I.ScrollText.color = new Color( 1, 1, 1, 1 );
		return true;
	}

    public void ResetFogOfWar( bool init = false )
	{
		//Tilemap.Build(); // new opt
        return;
        //if( Manager.I.GameType == EGameType.FARM ) return;
		for( int y = 0; y < Tilemap.height; y++ )
		for( int x = 0; x < Tilemap.width; x++ )
			{
                if( init ) Revealed[ x, y ] = false;
				if( Revealed[ x, y ] )
                    RevealTile( true, new Vector2( x, y ) );
				else
                    RevealTile( false, new Vector2( x, y ) );
			}
	}
    
	public void RevealTile( bool reveal, Vector2 pos )
	{
        return;
        if( Manager.I.GameType != EGameType.FARM )
        if( Manager.I.GameType != EGameType.NAVIGATION )
        if( Revealed[ ( int ) pos.x, ( int ) pos.y ] == reveal ) return;
        Color col = Quest.I.CurLevel.Tilemap.ColorChannel.GetColor( ( int ) pos.x, ( int ) pos.y );
		if( reveal == false ) col = Color.black;

		Revealed[ ( int ) pos.x, ( int ) pos.y ] = reveal;

		if( PtOnMap( Tilemap, new Vector2( ( int ) pos.x,     ( int ) pos.y     ) ) ) Tilemap.ColorChannel.SetColor( ( int ) pos.x,     ( int ) pos.y,     col );
		if( PtOnMap( Tilemap, new Vector2( ( int ) pos.x + 1, ( int ) pos.y     ) ) ) Tilemap.ColorChannel.SetColor( ( int ) pos.x + 1, ( int ) pos.y,     col );
		if( PtOnMap( Tilemap, new Vector2( ( int ) pos.x,     ( int ) pos.y + 1 ) ) ) Tilemap.ColorChannel.SetColor( ( int ) pos.x,     ( int ) pos.y + 1, col );
		if( PtOnMap( Tilemap, new Vector2( ( int ) pos.x + 1, ( int ) pos.y + 1 ) ) ) Tilemap.ColorChannel.SetColor( ( int ) pos.x + 1, ( int ) pos.y + 1, col );

		UpdateTilemap = true;
		//TransTilemapUpdateList.Add( pos );
		if( Gaia [ ( int ) pos.x, ( int ) pos.y ] && Gaia [ ( int ) pos.x, ( int ) pos.y ].Spr ) Gaia [ ( int ) pos.x, ( int ) pos.y ].Spr.color = col;
		if( Gaia2[ ( int ) pos.x, ( int ) pos.y ] && Gaia2[ ( int ) pos.x, ( int ) pos.y ].Spr ) Gaia2[ ( int ) pos.x, ( int ) pos.y ].Spr.color = col;
		if( Unit [ ( int ) pos.x, ( int ) pos.y ] && Unit [ ( int ) pos.x, ( int ) pos.y ].Spr )
		{
            Unit[ ( int ) pos.x, ( int ) pos.y ].Spr.color = col;

            if( Unit[ ( int ) pos.x, ( int ) pos.y ].TileID == ETileType.DOME )
            {
                if( Manager.I.GameType == EGameType.CUBES )
                    Unit[ ( int ) pos.x, ( int ) pos.y ].Spr.gameObject.GetComponent<tk2dAnimationAdapter>().color = new Color( col.r , col.g, col.b, .5f );
                else
                    Unit[ ( int ) pos.x, ( int ) pos.y ].Spr.gameObject.GetComponent<tk2dAnimationAdapter>().color = col;
            }
        }

		if( Gaia [ ( int ) pos.x, ( int ) pos.y ] && Gaia [ ( int ) pos.x, ( int ) pos.y ].LevelTxt ) Gaia [ ( int ) pos.x, ( int ) pos.y ].LevelTxt.color = col;
		if( Gaia2[ ( int ) pos.x, ( int ) pos.y ] && Gaia2[ ( int ) pos.x, ( int ) pos.y ].LevelTxt ) Gaia2[ ( int ) pos.x, ( int ) pos.y ].LevelTxt.color = col;
		if( Unit [ ( int ) pos.x, ( int ) pos.y ] && Unit [ ( int ) pos.x, ( int ) pos.y ].LevelTxt ) Unit [ ( int ) pos.x, ( int ) pos.y ].LevelTxt.color = col;

        if( Gaia[ ( int ) pos.x, ( int ) pos.y ] && Gaia[ ( int ) pos.x, ( int ) pos.y ].PriceTag )
        {
            Gaia[ ( int ) pos.x, ( int ) pos.y ].PriceTag.Price_1Resource.gameObject.GetComponent<tk2dAnimationAdapter>().color = new Color( 1, 1, 1, .8f );
            Gaia[ ( int ) pos.x, ( int ) pos.y ].PriceTag.Price_1Text.color = new Color( 1, 1, 1, .8f );
        }

        if( Gaia2[ ( int ) pos.x, ( int ) pos.y ] && Gaia2[ ( int ) pos.x, ( int ) pos.y ].PriceTag )
        {
            Gaia2[ ( int ) pos.x, ( int ) pos.y ].PriceTag.Price_1Resource.color = new Color( 1, 1, 1, .8f );
            Gaia2[ ( int ) pos.x, ( int ) pos.y ].PriceTag.Price_1Text.color = new Color( 1, 1, 1, .8f );
        }
        
        ETileType tile = ( ETileType ) Quest.I.CurLevel.Tilemap.GetTile( ( int ) pos.x,
                                                        ( int ) pos.y, ( int ) ELayerType.GAIA2 );

        if( tile == ETileType.CHECKPOINT ) UI.I.UpdateSaveGameIconState( ( int ) pos.x, ( int ) pos.y );

        if( Gaia2[ ( int ) pos.x, ( int ) pos.y ] )
        if( Gaia2[ ( int ) pos.x, ( int ) pos.y ].TileID == ETileType.ARROW )
            Gaia2[ ( int ) pos.x, ( int ) pos.y ].Body.SetWorking( Gaia2[ ( int ) pos.x, ( int ) pos.y ].Body.isWorking );

		if( tile == ETileType.ARTIFACT )
		{
			for( int a = 0; a < Quest.I.CurLevel.ArtifactList.Count; a++ )
			{
				if( Quest.I.CurLevel.ArtifactList[ a ].Pos == pos )
				{
					if( Quest.I.CurLevel.ArtifactList[ a ].Collected == Artifact.EStatus.NOT_COLLECTED && reveal == true )
						Quest.I.CurLevel.ArtifactList[ a ].gameObject.SetActive( true );
					else
						Quest.I.CurLevel.ArtifactList[ a ].gameObject.SetActive( false );
				}
			}
        }

        if( Map.I.FUnit[ ( int ) pos.x, ( int ) pos.y ] != null )                                            // Raft
            for( int i = 0; i < Map.I.FUnit[ ( int ) pos.x, ( int ) pos.y ].Count; i++ )
            {
                if( Map.I.FUnit[ ( int ) pos.x, ( int ) pos.y ][ i ].TileID == ETileType.RAFT )
                {
                    Map.I.FUnit[ ( int ) pos.x, ( int ) pos.y ][ i ].Spr.color = col;
                    break;
                }
                if( Map.I.FUnit[ ( int ) pos.x, ( int ) pos.y ][ i ].TileID == ETileType.BLOCKER )
                {
                    Map.I.FUnit[ ( int ) pos.x, ( int ) pos.y ][ i ].Spr.color = col;
                    break;
                }
            }
	}
    
	public void UpdateFogOfWar( bool force = false )
    {
        return;
        if( CurrentArea != -1 ) return;
        if( force == false )
        if( AdvanceTurn == false ) return;
		int range = ( int ) Hero.Control.ScoutLevel;
        if( Manager.I.GameType == EGameType.FARM ) range = 10;
        
        if( RM.RMD.RevealLabFog && LabRevealed == false )
        if( Manager.I.GameType == EGameType.CUBES )                                                           // Reveal whole lab
        if( RM.HeroSectorType == Sector.ESectorType.LAB )
            {
                for( int y = ( int ) RM.LabArea.y - 1; y < RM.LabArea.yMax; y++ )
                for( int x = ( int ) RM.LabArea.x - 1; x < RM.LabArea.xMax; x++ )
                if ( PtOnMap( Tilemap, new Vector2( x, y ) ) )
                if ( Revealed[ x, y ] == false )
                {
                        RevealTile( true, new Vector2( x, y ) );
                }
                LabRevealed = true;
            }

        if( RM.RMD.RevealCubeFog && Manager.I.GameType == EGameType.CUBES )                                                                                     // Reveal Whole Cube
        if( RM.HeroSector && RM.HeroSector.Revealed == false )
            if( RM.HeroSector.Type == Sector.ESectorType.NORMAL )
            {
                for( int y = ( int ) RM.HeroSector.Area.y - 1; y < RM.HeroSector.Area.yMax + 1; y++ )
                for( int x = ( int ) RM.HeroSector.Area.x - 1; x < RM.HeroSector.Area.xMax + 1; x++ )
                if ( PtOnMap( Tilemap, new Vector2( x, y ) ) )
                if ( Revealed[ x, y ] == false )
                if ( Sector.GetPosSectorType( new Vector2( x, y ) ) == Sector.ESectorType.GATES ||
                     Sector.GetPosSectorType( new Vector2( x, y ) ) == Sector.ESectorType.NORMAL )
                //if ( Mod.KeepHiddenTileList.Contains( new Vector2( x, y ) ) == false )
                     {
                        Hero.Control.ForceMove = EActionType.WAIT;
                        RevealTile( true, new Vector2( x, y ) );
                        int area = GetPosArea( new Vector2( x, y ) );
                        if( area != -1 ) UpdateAreaColor( area );
                     }

                RM.HeroSector.Revealed = true;
                return;
            }

        List<int> neigh = new List<int>();                                                      // Reveals area by stepping on neighborhood: Check neighborhood
        if( Manager.I.GameType == EGameType.CUBES ||
            Manager.I.GameType == EGameType.NORMAL ) 
        for( int d = 0; d < 8; d++ )
        {
            Vector2 tg = Hero.Pos + Manager.I.U.DirCord[ ( int ) d ];
            if( PtOnMap( Tilemap, tg ) )
            {
                int area = GetPosArea( tg );
                if( area != -1 && Quest.I.CurLevel.AreaList[ area ].Revealed == false )
                    if( neigh.Contains( area ) == false ) neigh.Add( area );
            }
        }

        for( int i = 0; i < neigh.Count; i++ )                                                           // Reveal tiles
        {
            Area ar = Quest.I.CurLevel.AreaList[ neigh[ i ] ];
            ar.Reveal();
        }

        int cont = 0;
        for( int y = ( int ) Hero.Pos.y - range; y <= Hero.Pos.y + range; y++ )                          // Check LOS Tiles
        for( int x = ( int ) Hero.Pos.x - range; x <= Hero.Pos.x + range; x++ )
        if ( PtOnMap( Tilemap, new Vector2( x, y ) ) )
        if ( Revealed[ x, y ] == false )
        {
            if( Manager.I.GameType == EGameType.FARM || 
                Manager.I.GameType == EGameType.NAVIGATION )                                            // Reveal whole square in the farm
                RevealTile( true, new Vector2( x, y ) );
            else
            cont += UpdateScoutLOS( Hero.Pos, new Vector2( x, y ) );
        }        
        int frange = 1;
        for( int y = ( int ) Hero.Pos.y - frange; y <= Hero.Pos.y + frange; y++ )                       // Forced Reveal around hero
        for( int x = ( int ) Hero.Pos.x - frange; x <= Hero.Pos.x + frange; x++ )
        if ( PtOnMap( Tilemap, new Vector2( x, y ) ) )
        if ( Revealed[ x, y ] == false )
        {
            cont++;  
            RevealFactor[ x, y ] = 1;
        }               

        if( cont > 0 )                                                                                    // Reveal LOS tiles
        {
            bool okper = false;
            for( int y = ( int ) Hero.Pos.y - range; y <= Hero.Pos.y + range; y++ )
            for( int x = ( int ) Hero.Pos.x - range; x <= Hero.Pos.x + range; x++ )
            if ( PtOnMap( Tilemap, new Vector2( x, y ) ) )
            if ( Revealed[ x, y ] == false )
            if ( RevealFactor[ x, y ] == 1 )
                {
                    if( RM.RMD.RevealAreaFromAfar )
                    {
                        Area ar = Area.Get( new Vector2( x, y ) );
                        if( ar )
                        {
                            ar.Reveal();
                        }
                    }

                    RevealTile( true, new Vector2( x, y ) );
                    okper = true;
                }
            if( okper ) UpdateRevealedPercent();
        }   
	}         

    public bool UpdateNeighborTileMove()
    {
        if( Input.GetMouseButtonUp( 0 ) == false ) return false;

        Vector2 tg = new Vector2( Mtx, Mty );
        Vector2 dif = tg - Hero.Pos;
        
        if( dif == new Vector2(  0,  1 ) ) Hero.Control.ForceMove = EActionType.MOVE_N;  else
        if( dif == new Vector2(  1,  0 ) ) Hero.Control.ForceMove = EActionType.MOVE_E;  else
        if( dif == new Vector2(  0, -1 ) ) Hero.Control.ForceMove = EActionType.MOVE_S;  else
        if( dif == new Vector2( -1,  0 ) ) Hero.Control.ForceMove = EActionType.MOVE_W;  
        if( Hero.Control.MovementLevel < 4 ) return false;
        if( dif == new Vector2(  1,  1 ) ) Hero.Control.ForceMove = EActionType.MOVE_NE; else
        if( dif == new Vector2(  1, -1 ) ) Hero.Control.ForceMove = EActionType.MOVE_SE; else
        if( dif == new Vector2( -1, -1 ) ) Hero.Control.ForceMove = EActionType.MOVE_SW; else
        if( dif == new Vector2( -1,  1 ) ) Hero.Control.ForceMove = EActionType.MOVE_NW; 

        if( Hero.Control.ForceMove != EActionType.NONE ) return true;
        return false;
    }
   
    public bool UpdateOverlayAnimation()
    {    
        if( UI.I.OverlaySprite.gameObject.activeSelf )                                                                        // whole screen overlay
        {
            UI.I.OverlaySprite.gameObject.SetActive( false );
            UI.I.OverlaySprite.color = new Color( UI.I.OverlaySprite.color.r, UI.I.OverlaySprite.color.g,
            UI.I.OverlaySprite.color.b, UI.I.OverlaySprite.color.a - ( Time.deltaTime * UI.I.OverlayFadeFactor ) );
        }
        if( UI.I.OverlaySprite.color.a > 0 )
            UI.I.OverlaySprite.gameObject.SetActive( true );

        if( CubeDeath == false ) return false; 
        DeathAnimationTimer += Time.deltaTime;
        if( DeathAnimationTimer > 2f )
            return false;
        return true;
    }

    public void UpdateHeroText()
    {
        if( G.HS ==  null) return;
        //if( UpdateHeroText ) return;                                                                         // optimize:  make a UpdateHeroText bool
        G.Hero.LevelTxt.gameObject.SetActive( false );
        float platbn = G.Hero.GetPlatformSpeedAttackBonus();
        if( Manager.I.GameType != EGameType.CUBES ) return;

        G.Hero.LevelTxt.text = "";
        int totsteps = 1 + ( int ) G.Hero.Control.PlatformSteps;
        if( G.Hero.Control.PlatformWalkingCounter > 0 )
            G.Hero.LevelTxt.text = "+" + ( totsteps - G.Hero.Control.PlatformWalkingCounter + 1 );             // platform steps
        if( G.HS.MaxTicTacMoves > 0 )
        if( Sector.AwakenNormalMonsters > 0 )                                    
        {
            int stp = G.HS.MaxTicTacMoves - G.HS.TicTacMoveCount + 1;
            string stps = " +" + stp;
            if( stp <= 0 ) stps = "Stop!";
            G.Hero.LevelTxt.text += stps;                                                                     // Max Tic Tac moves vault power
        }
        if( HeroAttackSpeedBonus > 0 )
        {
            G.Hero.LevelTxt.text += " " + HeroAttackSpeedBonus.ToString( "0." ) + "%";                        // hero attack speed
        }

        if( G.Hero.LevelTxt.text != "" )                                                                      // activate object
            G.Hero.LevelTxt.gameObject.SetActive( true );
        
        if( TurnFrameCount == 5 )
        {
            HeroGrabWallSprite.gameObject.SetActive( false );
            if( Controller.HeroWallGrabTg.x != -1 )
                HeroGrabWallSprite.gameObject.SetActive( true );                                              // Hero grab wall sprite for lava x raft moving
        }
    }

    public void FinalizeLoop()
    {
        UpdateHeroText();                                                                                            // Updates hero text mesh

        TurnTime += Time.deltaTime;

        RaftDragTurnTime += Time.deltaTime;

        if( Map.I.TurnFrameCount == 1 )
            Controller.MovedVineList.Clear();  

        TurnFrameCount++;                                                                                            // Turn frame count increment

        Farm.GlowPlagueMonsterIndicator.gameObject.SetActive( false );                                               // Farm Glow Effect for plague neighbor
        int neigh = Farm.GetAnyNeighborCount();
        if( neigh > 0 )
        {
            Farm.GlowPlagueMonsterIndicator.gameObject.SetActive( true );
            Farm.GlowPlagueMonsterIndicator.transform.Rotate( new Vector3( 0, 0, 30 * Time.deltaTime ) );
        }

        if( G.Hero.Pos != Controller.LastResPosCollectedByHeroPos )
            Controller.LastResPosCollectedByHeroPos = new Vector2( -1, -1 );

        for( int i = TimeKillList.Count - 1; i >= 0; i-- )                                                           // Handles timers to units added to kill timer list
        {
            TimeKillList[ i ].Body.KillTimer -= Time.deltaTime;
            if( TimeKillList[ i ].Body.KillTimer <= 0 )
            {
                Body.CreateDeathFXAt( TimeKillList[ i ].Pos );                                                       // Blood FX // TODO: Make a var for sound and fx
                KillList.Add( TimeKillList[ i ] );
                TimeKillList.RemoveAt( i );
            }
        }

        for( int i = KillList.Count - 1; i >= 0; i-- )                                                               // Kill units added to killlist
        {
            KillList[ i ].Kill();
        }
        KillList = new List<Unit>();

        if( G.HS ) G.HS.CubeFrameCount++;                                                                            // Increment cube frame counter       
    }
    
    private void UpdateProgressiveTransLayerCalculation()
    {
        if( AdvanceTurn ) return;

        if( UpdateTilemap )                                                                                          // updates tilemap if requested
            UpdateTileMap();

        int tot = Tilemap.width * Tilemap.height;    
        if( Manager.I.GameType == EGameType.FARM )                                                                  // farm only checks farm area
            tot = Farm.Tl.Count;

        if( ForceUpdateTrans == false )
        if( ProcessedTransCount >= tot )
        {
            TransLayerInitialized = true;
            return;                                                                                                 // all tiles updated
        }

        ForceUpdateTrans = false;
        int frange = ( int ) TransUpdateStatRadiusCount;
        if( TransLayerInitialized == false )  // new
        for( int y = ( int ) Hero.Pos.y + frange; y >= ( int ) Hero.Pos.y - frange; y-- )
        for( int x = ( int ) Hero.Pos.x + frange; x >= ( int ) Hero.Pos.x - frange; x-- )                           // loop around hero in an increasing radius to update trans tiles
        if ( PtOnMap( Tilemap, new Vector2( x, y ) ) )
        {
            if( TransInit[ x, y ] == false )
            {
                ClearTransTile( x, y, 0 );                                                                          // clear back trans tiles
                ClearTransTile( x, y, 1 );
                ClearTransTile( x, y, 2 );
                AddTrans( new VI( x, y ), false );                                                                   // add to list
            }
        }

        TransUpdateStatRadiusCount += TransUpdateStatRadiusPerTurn;                                                  // increment radius

        if( TransTilemapUpdateList.Count > 0 )
            UpdateTransLayerTilemap();                                                                               // updates trans tilemap if list is populated

    }
    public void AddTrans( VI v, bool force = true  )
    {
        TransTilemapUpdateList.Add( v );                                                                              // always use this function to add to trans
        if( force )
            ForceUpdateTrans = true;
        ProcessedTransCount++;
        TransInit[ v.x, v.y ] = true;
    }
    public void UpdateTileMap()
    {
        //if( TransLayerInitialized == false ) return;
        if( UpdateTilemap == false ) return;
        Tilemap.Build();
        UpdateTilemap = false;
    }
    public static Vector2 GM()
    {
        return new Vector2( Map.I.Mtx, Map.I.Mty );
    }
    public void CreateBoomerang()
    {        
        Unit b = Map.I.SpawnFlyingUnit( G.Hero.Pos, ELayerType.MONSTER, ETileType.PROJECTILE, null );
        b.Dir = G.Hero.Dir;
        b.Control.ProjectileType = EProjectileType.BOOMERANG;
        b.Control.ControlType = EControlType.NONE;
        b.Spr.spriteId = 209;
        b.RangedAttack.TargettingType = ETargettingType.BOOMERANG;
        BoomerangList.Add( b );
        b.Variation = 0;
        MasterAudio.PlaySound3DAtTransform( "Axe Throw", G.Hero.transform );
    }

    public void UpdateBoomerangs( bool pushed = false )
    {
        for( int i = BoomerangList.Count - 1; i >= 0; i-- )
        {
            Unit b = BoomerangList[ i ];
            bool kill = false;
            Vector2 tg = b.Pos + Manager.I.U.DirCord[ ( int ) b.Dir ];
            if( pushed ) tg = b.Pos;

            bool neigh = false;
            if( Util.Neighbor( G.Hero.Pos, b.Pos ) ) 
                neigh = true;

            if( b.Variation == 0 )
                b.Variation = 1;
            else
            if( b.Variation == 1 )
            {
                if( ( b.Dir == EDirection.N &&                                             // Hero Pulls back Boomerang
                  G.Hero.Pos.y < G.Hero.Control.LastPos.y ) ||
                  ( b.Dir == EDirection.S &&
                  G.Hero.Pos.y > G.Hero.Control.LastPos.y ) ||
                  ( b.Dir == EDirection.E &&
                  G.Hero.Pos.x < G.Hero.Control.LastPos.x ) ||
                  ( b.Dir == EDirection.W &&
                  G.Hero.Pos.x > G.Hero.Control.LastPos.x ) )
                {
                    b.Variation = 2;
                    MasterAudio.PlaySound3DAtVector3( "Save Game", G.Hero.Pos );
                }
            }
            else
            if( b.Variation >= 2 )           
            {
                b.Variation++;  
                if( b.Variation >= 3 )                                                                     // Boomerang stops
                    tg = b.Pos;
                if( b.Variation >= 5 )
                {
                    tg = b.Pos + Manager.I.U.DirCord[ ( int ) Util.GetInvDir( b.Dir ) ];                   // Boomerang returns
                    if( pushed ) tg = b.Pos;
                }
            }

            if( b.Dir == EDirection.N || b.Dir == EDirection.S ) tg.x = G.Hero.Pos.x;                     // Keep Boomerang Aligned
            if( b.Dir == EDirection.E || b.Dir == EDirection.W ) tg.y = G.Hero.Pos.y;

            List <ETileType> ex = new List<ETileType>();
            ex.Add( ETileType.ORB ); ex.Add( ETileType.WATER ); ex.Add( ETileType.PIT );
            bool res = Util.CheckBlock( b.Pos, tg, G.Hero, false, true, true, false, false, false, ex );
            if( !res ) res = b.CheckLeverBlock( true, b.Pos, tg, true );
            Unit mn = GetUnit( tg, ELayerType.MONSTER );
            if( mn && mn.ValidMonster == false && mn.TileID != ETileType.ORB ) res = true;
            if( Sector.IsPtInCube( tg ) == false ) res = true;

            if(!res )
            {                        
                MoveBoomerang( ref b, b.Pos, tg, ref kill );                                              // Moves the Boomerang
            }
            else
            {                
                kill = true;                                                                              // Destroys the boomerang
                b.Variation = 0;
            }

            if( b.Variation >= 5 )
            {
                if( neigh ) 
                {
                    MoveBoomerang( ref b, b.Pos, G.Hero.Pos, ref kill );                                 // To avoid boomerang passing throug hero
                }
                if( b.Pos == G.Hero.Pos )
                {
                    b.Variation = 1;
                    if( G.Hero.Dir == Util.GetInvDir( b.Dir ) )                                          // No opposite side cast 
                        kill = true;
                    b.Dir = G.Hero.Dir;
                    tg = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) b.Dir ];
                    MoveBoomerang( ref b, b.Pos, tg, ref kill );
                    if( Util.IsDiagonal( b.Dir ) ) kill = true;                                          // No Diagonal cast
                }
            }

            if( i == 0 ) 
                MasterAudio.PlaySound3DAtTransform( "Axe Throw", G.Hero.transform );                    // Sound FX
            if( kill ) 
            {
                BoomerangList.Remove( b );                                                              // Object destroy
                Map.I.CreateExplosionFX( b.Pos );                                                       // Explosion FX
                b.Kill();
                MasterAudio.PlaySound3DAtVector3( "Explosion 1", G.Hero.Pos );
                Item.AddItem( ItemType.Res_Boomerang, -1 );                                             // Remove item
            }
        }
    }
    public void MoveBoomerang( ref Unit b, Vector2 from, Vector2 tg, ref bool killit )
    {
        Controller.MoveFlyingUnitTo( ref b, b.Pos, tg );
        Unit orb = Map.I.GetUnit( ETileType.ORB, tg );                                   // Orb Strike 
        if( orb )
        {
            orb.StrikeTheOrb( b, true );
            tg = b.Pos + Manager.I.U.DirCord[ ( int ) b.Dir ];
            if( b.Variation >= 5 )
            {
                MoveBoomerang( ref b, b.Pos, tg, ref killit );
                b.Variation = 1;
            }
        }
        Controller.ForceBoomerangAttack = true;
        b.RangedAttack.UpdateIt( true );                                                 // Update Attack
        Controller.ForceBoomerangAttack = false;
        UpdatePostBoomerangAttack( ref b );
        Unit mine = Map.GFU( ETileType.MINE, tg );
        if( mine )
        {
            if( G.HS.BoomerangCanMine )                                                  // Boomerang destroy mine vault bonus
            if( Controller.CanBeMined( tg ) )
                Kill( mine );
            if( mine.Body.MineType != EMineType.BRIDGE )
            if( mine.Body.MineType != EMineType.SHACKLE )
            if( mine.Body.MineType != EMineType.TUNNEL )
                killit = true;                                                           // destroy boomerang in these cases
        }
    }

    public void UpdatePostBoomerangAttack( ref Unit b )
    {
        if( Attack.BoomerangAttack )
        {
            Item.AddItem( ItemType.Res_Boomerang, -1 );
            int num = ( int ) Item.GetNum( ItemType.Res_Boomerang );
            if( num < 1 )
            {
                KillList.Add( b );
                BoomerangList.Remove( b );
            }
        }
        Attack.BoomerangAttack = false;
    }
    public void UpdateAllMonsterWakingUp()
    {
        Sector s = Map.I.RM.HeroSector;
        if( s == null ) return;
        if( s.Type != Sector.ESectorType.NORMAL ) return;

        if( s.MoveOrder != null )
            for( int i = 0; i < s.MoveOrder.Count; i++ )
                s.MoveOrder[ i ].Control.UpdateResting();

        for( int i = G.HS.Fly.Count - 1; i >= 0; i-- )                                                // Flying Move and Attack Update
            G.HS.Fly[ i ].Control.UpdateResting();

        for( int i = 0; i < s.DynamicObjects.Count; i++ )
            if( s.DynamicObjects[ i ].Control )
                s.DynamicObjects[ i ].Control.UpdateResting();
    }
    public void SetMovementMode( bool type )
    {
        if( Manager.I.GameType != EGameType.CUBES ) return;    
            Sector s = Map.I.RM.HeroSector;
        if( s.Type != Sector.ESectorType.NORMAL ) return;
        s.SteppingMode = type;
        string txt = "Stepping Mode!";
        if( s.SteppingMode == false )
            txt = "Realtime Mode!";
        UI.I.SetBigMessage( txt, Color.green, 6f, 4.7f, 122.8f, 85, 2 );
   
        for( int i = 0; i < s.MoveOrder.Count; i++ )
        {
            s.MoveOrder[ i ].Control.SpeedTimeCounter = 0;
            if( s.MoveOrder[ i ].MeleeAttack )
                s.MoveOrder[ i ].MeleeAttack.InvalidateAttackTimer = 0;
        }
    }

    public static bool Stepping()
    {
        if( Manager.I.GameType != EGameType.CUBES ) return false;
        bool res = false;
        if( Map.I.RM.HeroSector.SteppingMode ) res = true;
        if( Map.I.NumScorpions > 0 ) res = true;
        if( CubeData.I.SteppingMode ) res = true;

        if( Map.I.OldSteppingMode == true && res == false )
        {
            UI.I.SetBigMessage( "Realtime Mode!", Color.green, 6f, 4.7f, 122.8f, 85, 2 );
        }
        else
        if( Map.I.OldSteppingMode == false && res == true )
        {
            UI.I.SetBigMessage( "Stepping Mode!", Color.green, 6f, 4.7f, 122.8f, 85, 2 );
        }

        Map.I.OldSteppingMode = res;
        return res;
    }
    public static void Kill( Unit un, bool fx = false, string sound = "" )
    {
        if( Map.I.KillList.Contains( un ) == false )
        {
            Map.I.KillList.Add( un );
            if( fx ) Body.CreateDeathFXAt( un.Pos );                                              // Blood FX
            if( sound != "" )
                MasterAudio.PlaySound3DAtVector3( sound, un.Pos );                                // Sound FX    
        }
    }
    public static void TimeKill( Unit un, float time )
    {
        if( Map.I.TimeKillList.Contains( un ) == false )
        {
            un.Body.KillTimer = time;
            un.Body.IsDead = true;
            Map.I.TimeKillList.Add( un );
        }
    }
    public bool UpdateKickingBoots( Vector2 from, Vector2 to )
    {
        if( from == to ) return false;                                       // no movement
        EDirection mov = Util.GetTargetUnitDir( from, to );                  // get movement direction
        Vector2 untg = to + Manager.I.U.DirCord[ ( int ) mov ];              // default target in front of move
        List <Vector2>tgl = new List<Vector2>();
        if( mov == EDirection.NE || mov == EDirection.NW || 
            mov == EDirection.SE || mov == EDirection.SW )                   // diagonal move
            for( int c = 0; c < 4; c++ )
            {                                                                // 0=N,1=E,2=S,3=W
                EDirection card = ( EDirection ) ( c * 2 );                  // cardinal direction
                EDirection d1 = ( EDirection ) ( ( int ) card + 1 );         // first diagonal
                EDirection d2 = ( EDirection ) ( ( ( int ) card + 7 ) % 8 ); // second diagonal
                if( mov == d1 || mov == d2 )
                {                                                            // matches one of the diagonals
                    Vector2 cp = from + Manager.I.U.DirCord[ ( int ) card ]; // tile in cardinal direction
                    Unit u0 = Map.I.GetUnit( cp, ELayerType.MONSTER );       // get monster
                    if( u0 && u0.ValidMonster )
                    {
                        mov = card; untg = cp; break;                        // force cardinal kick
                    }
                }
            }

        Unit un = Map.I.GetUnit( untg, ELayerType.MONSTER );                 // try monster
        if( un && un.ValidMonster == false ) return false;                   // invalid monster
        if( un == null ) un = Map.GFU( ETileType.MINE, untg );               // try mine
        if( un == null ) return false;                                       // no target found
        if( Item.GetNum( ItemType.Res_Kick ) < 1 ) return false;             // no kick resource
        if( !G.Hero.CanMoveFromTo( false, from, to, G.Hero ) ) return false; // hero cannot move

        Vector2 finalTarget = new Vector2( -1, -1 );                         // default no valid target
        float dmg = RM.RMD.BaseKickDamage;                                   // base kick damage
        for( int i = 1; i < Sector.TSX; i++ )
        {                                                                       // iterate forward tiles
            Vector2 tgg = un.Pos + ( Manager.I.U.DirCord[ ( int ) mov ] * i );  // next tile in kick direction
            if( un.CanMoveFromTo( false, un.Pos, tgg, G.Hero ) )
            {                                                                   // target can move there
                Unit bar = Map.I.GetUnit( ETileType.BARRICADE, tgg );           // check for barricade
                if( bar )
                {
                    if( i > 1 ) bar.DestroyBarricade( tgg );                    // destroy barricade if beyond first tile
                    break;
                }
                finalTarget = tgg;                                              // update final target
                tgl.Add( tgg );
            }
            else
            {
                if( !Map.IsWall( tgg ) ) dmg = 0;                               // stop if blocked and maybe no damage
                break;
            }
        }

        if( finalTarget.x != -1 )
        {                                                                        // valid target found
            un.CanMoveFromTo( true, un.Pos, finalTarget, G.Hero );               // move target
            un.Control.SpeedTimeCounter = 0;                                     // reset target speed counter
            Item.AddItem( ItemType.Res_Kick, -1 );                               // consume kick resource
            Map.I.CreateExplosionFX( un.Pos, "Fire Explosion", "Hero Jump" );    // explosion fx at start
            Map.I.CreateExplosionFX( untg );                                     // explosion fx at target tile
            MasterAudio.PlaySound3DAtVector3( "Hero Jump", G.Hero.Pos );         // play kick sound
            KickTimer = 0.45f;                                                   // set kick cooldown
            if( dmg > 0 && un.ValidMonster )
                un.Body.ReceiveDamage( dmg, EDamageType.BLEEDING, G.Hero, null );  // apply damage
            for( int i = 0; i < tgl.Count; i++ )
                Map.I.CreateExplosionFX( tgl[ i ], "Smoke Cloud", "" );            // Smoke Cloud FX on sliding path
        }
        return false;                                                              // default return
    }    
    public void UpdateHeroDisplacement()
    {
        if( G.HS.DisplaceHeroType < 0 ) return;
        if( Controller.MoveTickDone == false ) return;
        if( G.HS.DisplaceHeroTickNumber != G.HS.CurrentTickNumber ) return;
        List<Vector2> tgl = new List<Vector2>();
        List<Vector2> frtgl = new List<Vector2>();
        for( int yy = ( int ) G.HS.Area.yMin - 1; yy < G.HS.Area.yMax + 1; yy++ )                                         // fill list of targets
        for( int xx = ( int ) G.HS.Area.xMin - 1; xx < G.HS.Area.xMax + 1; xx++ )
        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
        {
            Unit un = Map.I.GetUnit( new Vector2( xx, yy ), ELayerType.MONSTER );
            if( un && un.ValidMonster && un.Control.Resting == false )
            {
                bool los = Map.I.HasLOS( un.Pos, G.Hero.Pos, false );                                                      // needs LOS
                if( los )
                for( int d = 0; d < 8; d++ )
                {
                    Vector2 tg = new Vector2( xx, yy ) + Manager.I.U.DirCord[ d ];
                    if( tgl.Contains( tg ) == false && 
                        G.Hero.CanMoveFromTo( false, new Vector2( xx, yy ), tg, G.Hero ) )                                 // ok to move to position
                    {
                        tgl.Add( tg );
                        frtgl.Add( new Vector2( xx, yy ) );                                                                 // add to list
                    }
                }
            }
        }
        if( tgl.Count < 1 ) return;
        int id = Random.Range( 0, tgl.Count );
        G.Hero.CanMoveFromTo( true, frtgl[ id ], tgl[ id ], G.Hero );                                                        // Apply move
                
        List<int> drl = new List<int>();
        for( int d = 0; d < 8; d++ )
        {
            Vector2 tg = tgl[ id ] + Manager.I.U.DirCord[ d ];                                                               // sorts a direction not facing any monster
            Unit mn = GetUnit( tg, ELayerType.MONSTER );
            if( mn == null || mn.ValidMonster == false )
                drl.Add( d );
        }
        int sort = Util.GetRandomDir();
        if( drl.Count > 0 ) sort = Random.Range( 0, drl.Count );
        G.Hero.RotateTo( ( EDirection ) drl[ sort ] );                                                                      // random dir       
        G.Hero.Spr.transform.eulerAngles = Util.GetRotationAngleVector( G.Hero.Dir );
        G.Hero.Body.Shadow.transform.eulerAngles = Util.GetRotationAngleVector( G.Hero.Dir );
        MasterAudio.PlaySound3DAtVector3( "Hero Jump", G.Hero.Pos );                                                       // sound FX
    }

    public void UpdateSandBox()
    {
        if( Helper.I.ReleaseVersion ) return;
        //if( Input.GetKeyDown( KeyCode.Z ) ) // todo remove
        //    GlobalAltar.I.CreateRandomButcher();

        //if( Input.GetKeyDown( KeyCode.Z ) ) // todo remove
        //{
        //    for( int i = 0; i < G.HS.CloverPosList.Count; i++ )
        //        Controller.CreateMagicEffect( G.HS.CloverPosList[ i ] );
        //}
    }
}

