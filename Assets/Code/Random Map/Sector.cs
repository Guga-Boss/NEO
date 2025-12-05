using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;
using System.IO;
using PathologicalGames;
public class Sector : MonoBehaviour
{
    public enum ESectorType
    {
        NONE, NORMAL, LAB, GATES, FARM
    };
    [TabGroup( "Lists" )]
    public List<Unit> MoveOrder, Fly, DynamicObjects;
    [TabGroup( "Lists" )]
    public List<Area> AreaList;
    [TabGroup( "Lists" )]
    public List<Vector2> CheckpointList, CloverPosList;
    [TabGroup( "Lists" )]
    public List<Spell> SpellList;
    [TabGroup( "Lists" )]
    public List<int> CatchModList = new List<int>();
    [TabGroup( "Link" )]
    public List<ItemType> PrizeRes = new List<ItemType>();
    [TabGroup( "Link" )]
    public List<float> PrizeResAmt = new List<float>();
    [TabGroup( "Link" )]
    public List<Vector2> PrizeStepList = new List<Vector2>();
    [TabGroup( "Link" )]
    public tk2dTextMesh HintText;
    public ESectorType Type;
    public static Sector LastNormalSector;
    public Rect Area;
    public bool AreasCreated, Discovered, Cleared, HintSortSuccess, Perfect, SteppingMode = false, GotoPuzzleUnlocked;
    public int X, Y, Number;
    public Vector2 LastHeroPos, RescuePos;
    [TabGroup( "Old" )]
    public string HintTypeText, HintDescription, AllHintText;
    [TabGroup( "Old" )]
    public SectorHint.ESectorHintOperation HintOperation;
    [TabGroup( "Old" )]
    public SectorHint.ESectorHintStrenght HintStrenght;
    [TabGroup( "Old" )]
    public SectorHint SectorHint = null;
    [TabGroup( "Old" )]
    public float[] HintOperatorFactor, HintFactor, HintStrenghtFactor;
    [TabGroup( "Old" )]
    public float ShowHintFactor, TotalBonus;
    public float NeighBorBonus;  // Bonus not counting neighboors
    [Range( 0, 10 )]
    public float RealtimeSpeedFactor = 1;
    public float HeroHP = 100;
    public static bool ObjectsCreated = false; 
    public bool GoalConquered = false;
    public int AliveFlyingMonsters, TotAliveFlyingMonsters, AliveNormalMonsters, TotNormalMonsters, TotFlyingMonsters, SectorHintQuality, MorphCount, NumFreeReentrances, PerfectAreaCount,
        AllBurntAreaCount, MarkedAreaCount, BackAttackCount, MonsterFlankCount, FireIgnitionCount, CloverPickCount, TotalClovers;
    public static int AwakenFlyingMonsters, AwakenNormalMonsters, TotalAwakenMonsters;
    public int MaximumWoundedUnits, MaxRaftID;
    public int LastAreaJumpID = 0;
    public  bool FlipX;
    public  bool FlipY;
    public static int TSX = 30;  // Tile size x
    public static int TSY = 30;  // Tile Size y
    public static int NX = 8;    // Number of sectors x
    public static int NY = 8;    // Number of Sectors y
    public  bool Carved = false;
    public  bool Revealed = false;
    public float TimeSpentOnCube = 0;
    public int CubeTurnCount = 0;
    public static Sector CS = null;
    public List< float> ItemCountList;
    public int CubeFrameCount = 0;
    public List<bool> ActiveHeroShield;
    public bool GridInitialized = false;
    [TabGroup( "Move" )]
    public float TickTimeCounter = 0;
    [TabGroup( "Move" )]
    public int CurrentTickNumber = 1;
    // Fishing Pole bonus variables
    public bool HookMergeEnabled = false;
    public bool CatchEnabled = true;
    public bool ImpulsionateEnabled = false;
    public bool MarkingEnabled = false;
    public int FishingHookExtraLevel = 0;
    public float FishingExtraTime = 0;
    public float FishingExtraAttack = 0;
    public float HarpoonAttackBonus = 0;
    public float FogExtraAttack = 0;
    public float GreenFishExtraAttack = 0;
    public float YellowFishExtraAttack = 0;
    public float RedFishExtraAttack = 0;
    public float FishingExtraSpeed = 0;
    public float FishingExtraRadius = 0;
    public int FishingExtraPrize = 0;
    public int FishingReturnCatch = 0;
    public float FishAddPercent = 0;
    public float FishMultiplyPercent = 0;
    public float FishIncreasePercent = 0;
    public int OrbStrikeCount = 0;
    public int NumAttackedFish = 0;
    public bool AlgaeDestroy = false;
    public int CountAlgaeDestroy = 0;
    public bool RaftDestroy = false;
    public float InventoryFishAddBonus = 0;
    public float InventoryFishMultiplyBonus = 0;
    public float InventoryAllAddBonus = 0;
    public float InventoryAllMultiplyBonus = 0;
    public float AddNextPoleBonusVal = 0;
    public float MultiplyNextPoleBonusVal = 0;
    public int GlowNextFishAmount = 0;
    public float SetFishPercent = -1;
    public float DoubleNextPerAmt = 0;
    public int ExtraSmallHook = 0;
    public int ExtraMediumHook = 0;
    public int ExtraLargeHook = 0;
    public bool ArrowDestroy = false;
    public bool ArrowPush = false;
    public int PoleBonusesConquered = 0;
    public int LastGlowedFishMod = -1;

    // Mine Vault bonus variables
    public int MineDestroyedCount = 0;
    public int MineMinedCount = 0;
    public int InitialGlobalVaultPoints = 0;
    public int GlobalVaultPoints = 0;
    public int RemainingCracked = 0;
    public int RoundMineMinedCount = 0;
    public int SquareMineMinedCount = 0;
    public int DefaultMiningChanceItemID = -1;
    public float DefaultMiningChance = -1;
    public int BehindMineBonusItemID = -1;
    public int BehindMineBonusAmount = -1;
    public int BehindMineBonusTimes = -1;
    public float BehindMineBonusChanceAmount = -1;
    public int BehindMineBonusChanceTimes = -1;
    public float BehindMineBonusCompoundChanceAmount = -1;
    public int BehindMineBonusCompoundChanceTimes = -1;
    public float FailedMineBonusChance = 0;
    public float NextMineBonusChanceCount = 0;
    public float NextMineBonusChanceAmount = 0;
    public float FreePushableObjects = 0;
    public int FailedMineMoveRandomly = -1;
    public int HeroJumpOverMinesCost = -1;
    public int HeroJumpOverMinesItemID = -1;
    public int BagExtraBonusItemID = -1;
    public float BagExtraBonusAmount = 0;
    public int BagExtraInventoryItemID = -1;
    public float BagExtraInventoryAmount = 0;
    public int DestroyedRockSpawnArrow = 0;
    public int AroundMineBonusItemID = -1;
    public int AroundMineBonusTimes = 0;
    public float AroundMineBonusAmount = 0;
    public int MuddyRockSpawnBoulder = 0;
    public bool BoomerangCanMine = false;
    public bool FireballCanMine = false;
    public int AlcovePush = 0;
    public float PickaxeInterest = 0;
    public int PickaxeInterestItemID = -1;
    public float PickaxeInterestItemMultiplier = 0;
    public float RandomItemsCreatedBonus = 0;
    public float MinesAroundChiselBonusChance = 0;
    public float DestroyedMinesInSequence = 0;
    public int HeroChangeBoulderDir = 0;
    public int BoulderPushPower = 1;
    public bool BoulderSidePush = false;
    public int LinkedCrackedBnItemID = -1;
    public float LinkedCrackedBnAmount = 1;
    public int HeroExpandMud = 0;
    public int MissSameMineInSequence = 0;
    public float NextVaultExtraPower = 0;
    public int VaultReuse = 0;
    public int DestroyedGiveExtraItemID = -1;
    public float DestroyedGiveExtraItemAmount = 0;
    public int CrackedGiveExtraItemID = -1;
    public float CrackedGiveExtraItemAmount = 0;
    public int DynamiteExplodeCracked = 0;
    public float MaxCostMiningFailure = -1;
    public int MaxCostMiningFailureUses = 0;
    public float MaxCostMiningSuccess = -1;
    public int MaxCostMiningSuccessUses = 0;
    public float DestroyedCrackNeighborChance = 0;
    public float FailedCrackNeighborChance = 0;
    public int GetExtraItemID = -1;
    public float GetExtraItemAmount = 0;
    public float FailedInflationPerSide = 0;
    public int XDestroyedGiveItemID = -1;
    public float XDestroyedGiveItemAmount = 0;
    public Vector2 GloveTarget = new Vector2( -1, -1 );
    public int MaxTicTacMoves = -1;
    public int TicTacMoveCount = 1;
    public int DisplaceHeroTickNumber = -1;
    public int DisplaceHeroType = -1;
    public int AltarCount = 0;
    public int RandomButchersSpawned = 0;  
    void Reset()
    {
        Area = new Rect();
        Type = ESectorType.NONE;
        LastNormalSector = null;
        Discovered = Cleared = AreasCreated = Perfect = GotoPuzzleUnlocked = false; 
        GridInitialized = false;
        Carved = false;
        X = Y = -1;
        MaxRaftID = -1;
        LastHeroPos = new Vector2( -1, -1 );
        HintText.text = "";
        HintTypeText = HintDescription = "";
        AllHintText = "";
        HintStrenght = SectorHint.ESectorHintStrenght.NONE;
        HintOperation = SectorHint.ESectorHintOperation.NONE;
        SectorHint = null;
        SectorHintQuality = 1;
        CloverPickCount = 0;
        TotalClovers = -1;
        MorphCount = 0;
        NumFreeReentrances = 0;
        PerfectAreaCount = 0;
        AllBurntAreaCount = 0;
        AliveNormalMonsters = -1;
        TotNormalMonsters = 0;
        TotFlyingMonsters = 0;
        AwakenFlyingMonsters = 0;
        AwakenNormalMonsters = 0;
        TotalAwakenMonsters = 0;
        TotalBonus = 0;
        TimeSpentOnCube = 0;
        CubeFrameCount = 0;
        NeighBorBonus = 0;
        FireIgnitionCount = 0;
        MaximumWoundedUnits = 0;
        MonsterFlankCount = 0;
        AliveFlyingMonsters = -1;
        LastAreaJumpID = 0;
        TotAliveFlyingMonsters = -1;
        MoveOrder = null;
        Fly = new List<Unit>();
        CubeTurnCount = 0;
        GoalConquered = false;
        SteppingMode = false;
        CloverPosList = new List<Vector2>();
        CatchModList = new List<int>();
        TickTimeCounter = 0;
        CurrentTickNumber = 1;
        AltarCount = 0;
        RandomButchersSpawned = 0;
        HeroHP = Map.I.RM.RMD.HeroStartingHP;
    }
    public void Init()
    {
        Discovered = Cleared = AreasCreated = Perfect = HintSortSuccess = GotoPuzzleUnlocked = false;       
        LastHeroPos = new Vector2( -1, -1 );
        HintText.text = "";
        MaxRaftID = -1;
        HintTypeText = HintDescription = "";
        AllHintText = "";
        HintStrenght = SectorHint.ESectorHintStrenght.NONE;
        HintOperation = SectorHint.ESectorHintOperation.NONE;
        SectorHintQuality = 1;
        MorphCount = 0;
        NumFreeReentrances = 0;
        PerfectAreaCount = 0;
        TotalBonus = 0;
        NeighBorBonus = 0;
        TimeSpentOnCube = 0;
        CubeFrameCount = 0;
        MaximumWoundedUnits = 0;
        AllBurntAreaCount = 0;
        MarkedAreaCount = 0;
        BackAttackCount = 0;
        AreaList = null;
        Carved = false;
        Revealed = false;
        Type = ESectorType.NORMAL;
        MonsterFlankCount = 0;
        FireIgnitionCount = 0;
        GoalConquered = false;
        MoveOrder = null;
        HeroHP = Map.I.RM.RMD.HeroStartingHP;
        ItemCountList = new List<float>();
        for( int i = 0; i < Manager.I.Inventory.ItemList.Count; i++ )                                     // Warning: Dont use G.GIT here: the access is directly to the itemlist via loop and id    
        {
            float num = 0;
            if( Manager.I.Inventory.ItemList[ i ] )
                num = Manager.I.Inventory.ItemList[ i ].StartingBonus;
            ItemCountList.Add( num ); 
        }
        ItemCountList[ ( int ) ItemType.Res_ForcedZoom ] = -1;
        ItemCountList[ ( int ) ItemType.Res_HP ] = -1;
 
        if( Map.I.RM.RMD.LimitedArrowPerCube != -1 )
            ItemCountList[ ( int ) ItemType.Res_Bow_Arrow ] = Map.I.RM.RMD.LimitedArrowPerCube;
        if( Map.I.RM.RMD.LimitedMeleeAttacksPerCube != -1 )
            ItemCountList[ ( int ) ItemType.Res_Melee_Attacks ] = Map.I.RM.RMD.LimitedMeleeAttacksPerCube;

        ActiveHeroShield = new List<bool>();
        CloverPosList = new List<Vector2>();
        for( int i = 0; i < 8; i++ )
        {
            ActiveHeroShield.Add( false );
        }
        for( int i = 0; i < G.Hero.Body.Sp.Count; i++ )         
            G.Hero.Body.Sp[ i ].Reset();

        for( int i = 0; i < SpellList.Count; i++ )
        {
            SpellList[ i ].Reset();
        }
        PrizeRes = new List<ItemType>();
        PrizeResAmt = new List<float>();
        PrizeStepList = new List<Vector2>();
    }

    public static void Save( string nm = "" )
    {
        Sector s = Map.I.RM.HeroSector;
        string file = Manager.I.GetProfileFolder();
        if( nm != "" ) file += "Cube Save/Sector" + nm + ".NEO";                           // Provides filename
        else file += "/Sector.NEO";
        Statistics st = Map.I.LevelStats;

        using( GS.W = new BinaryWriter( File.Open( file, FileMode.OpenOrCreate ) ) )
        {
            int SaveVersion = 1;
            GS.W.Write( SaveVersion );                                                     // Save Version
            GS.W.Write( s.Cleared );
            GS.W.Write( s.Perfect );
            GS.W.Write( s.AliveFlyingMonsters );
            GS.W.Write( s.AliveNormalMonsters );
            GS.W.Write( s.TimeSpentOnCube );
            GS.W.Write( s.CurrentTickNumber );
            GS.W.Write( s.TickTimeCounter );
            GS.W.Write( s.CatchModList.Count );
            for( int i = 0; i < s.CatchModList.Count; i++ )
                GS.W.Write( s.CatchModList[ i ] );
            GS.W.Write( s.ActiveHeroShield.Count );
            for( int i = 0; i < s.ActiveHeroShield.Count; i++ )
                GS.W.Write( s.ActiveHeroShield[ i ] );
            GS.W.Close();
        }
    }
    public static void Load( string nm = "" )
    {
        Sector s = Map.I.RM.HeroSector;
        string file = Manager.I.GetProfileFolder();
        if( nm != "" ) file += "Cube Save/Sector" + nm + ".NEO";                          // Provides filename
        else file += "/Sector.NEO";
        Statistics st = Map.I.LevelStats;
        using( GS.R = new BinaryReader( File.Open( file, FileMode.Open ) ) )
        {
            int SaveVersion = GS.R.ReadInt32();                                           // Load Version
            s.Cleared = GS.R.ReadBoolean();
            s.Perfect = GS.R.ReadBoolean(); 
            s.AliveFlyingMonsters = GS.R.ReadInt32();
            s.AliveNormalMonsters = GS.R.ReadInt32();
            s.TimeSpentOnCube = GS.R.ReadSingle();
            s.CurrentTickNumber = GS.R.ReadInt32();
            s.TickTimeCounter = GS.R.ReadSingle();
            s.CatchModList = new List<int>();
            int sz = GS.R.ReadInt32();
            for( int i = 0; i < sz; i++ )                                                    
                s.CatchModList.Add( GS.R.ReadInt32() );
            sz = GS.R.ReadInt32();
            s.ActiveHeroShield = new List<bool>();
            for( int i = 0; i < sz; i++ )
                s.ActiveHeroShield.Add( GS.R.ReadBoolean() );
            GS.R.Close();
        }
        UI.I.UpdBeastText = true;
    }

    public static void InitAll()
    {
        for( int y = 0; y < NY; y++ )
        for( int x = 0; x < NX; x++ )            
             Map.I.RM.RMSector[ x, y ].Init();             
    }

    public static Sector GetPosSector( Vector2 pos )
    {
        if( Manager.I.GameType == EGameType.FARM ) return null;
        Rect lab = new Rect( Map.I.RM.LabArea.position, new Vector2( 59, 59 ) );                               // returns the original lab pos to prevent gate over problem
        if( lab.Contains( pos ) )
            return Map.I.RM.RMSector[ ( int ) Map.I.RM.LabCordOrigin.x, ( int ) Map.I.RM.LabCordOrigin.y ];

        for( int y = 0; y < NY; y++ )
        for( int x = 0; x < NX; x++ )
            {
                Rect r = Map.I.RM.RMSector[ x, y ].Area;
                if( r.Contains( pos ) )
                {
                    return Map.I.RM.RMSector[ x, y ];
                }
            }

        return Map.I.RM.GateSector;
    }
    public static ESectorType GetPosSectorType( Vector2 pos )
    {
        if( Manager.I.GameType == EGameType.FARM ) return ESectorType.FARM;
        Rect lab = new Rect( Map.I.RM.LabArea.position, new Vector2( 31, 31 ) );
        if( lab.Contains( pos ) ) return ESectorType.LAB;

        for( int y = 0; y < NY; y++ )
        for( int x = 0; x < NX; x++ )
            {
                Rect r = Map.I.RM.RMSector[ x, y ].Area;
                if( r.Contains( pos ) ) return Map.I.RM.RMSector[ x, y ].Type;
            }

        return ESectorType.GATES;
    }

    public static void CreateObjects()
    {
        if( ObjectsCreated ) return;
        GameObject sectorfolder = GameObject.Find( "Sector Objects" );

        Map.I.RM.RMSector = new Sector[ NX, NY ];
        for( int y = 0; y < NY; y++ )
        for( int x = 0; x < NX; x++ )
            {
                int xx = x + ( x );
                int yy = y + ( y );
                Rect rect = new Rect( ( x * TSX ) + 1, ( y * TSY ) + 1, TSX - 1, TSY - 1 );

                GameObject go = Manager.I.CreateObjInstance( "Sector", "Sector " + x + " " + y, EDirection.NONE, new Vector3( rect.x, rect.y, 0 ) );
                go.transform.parent = sectorfolder.transform;

                Map.I.RM.RMSector[ x, y ] = go.GetComponent<Sector>();
                Map.I.RM.RMSector[ x, y ].Reset();
                Map.I.RM.RMSector[ x, y ].Area = rect;
                Map.I.RM.RMSector[ x, y ].X = x;
                Map.I.RM.RMSector[ x, y ].Y = y;
                Map.I.RM.RMSector[ x, y ].Type = Sector.ESectorType.NORMAL;
                Map.I.RM.RMSector[ x, y ].HintText.transform.position = new Vector3( Map.I.RM.RMSector[ x, y ].Area.center.x, Map.I.RM.RMSector[ x, y ].Area.center.y, -3 );
            }
        ObjectsCreated = true;
    }

    public static void EnterSector( Sector old, Vector2 pos, Sector newsec = null )
    {
        Sector s = Sector.GetPosSector( pos );
        G.HS = s;
        Map.I.InvalidateInputTimer = .3f;

        if( newsec && newsec.Type == ESectorType.GATES )
        if( old.Type == ESectorType.NORMAL )
        {
            GS.RemoveFixedSpells();                                                                           // IMPORTANT: this is temporary, destroy fixed spells on gate step, just to avoid moving spells abroad. implement better solution later; you need to spawn and despawn objects on enter and leave
            for( int i = 0; i < G.Hero.Body.Sp.Count; i++ )                                                   // Copy hero spell data to sector data
                old.SpellList[ i ].Copy( G.Hero.Body.Sp[ i ] );
            old.HeroHP = G.Hero.Body.Hp;
        }

        if( newsec.Type == ESectorType.NORMAL )
        {
            LastNormalSector = old;
            Map.I.ZoomMode = CubeData.I.PreferedZoomMode;
        }

        if( old && old.Type == ESectorType.GATES )                                                            // proceed only on cube change. not after dying
        if( LastNormalSector != newsec )
        {
            Map.I.ElectrifiedFogList = new List<int>();
        }

        ResourceIndicator.UpdateGrid = true;
        if( old && old.Type == ESectorType.NORMAL )                                                           // Since fish are per cube resource, store old cube num
        {
            for( int i = 0; i < Manager.I.Inventory.ItemList.Count; i++ )                                     // Warning: Dont use G.GIT here: the access is directly to the itemlist via loop and id    
            if( Manager.I.Inventory.ItemList[ i ] )
            if( Manager.I.Inventory.ItemList[ i ].IsGameplayResource )
            {
                old.ItemCountList[ i ] = Item.GetNum( Manager.I.Inventory.ItemList[ i ].Type );
            }
        }

        if( newsec && newsec.Type == ESectorType.GATES )
        if( old.Type == ESectorType.NORMAL )
            {
                Sector.FinishSectorCleared( old );                                                            // Finish Sector Cleared
            }
         
        if( s.Type == ESectorType.NORMAL )                                                                    // Since fish are per cube resource, retrieve cube vals
        {
            for( int i = 0; i < Manager.I.Inventory.ItemList.Count; i++ )                                     // Warning: Dont use G.GIT here: the access is directly to the itemlist via loop and id    
            if( Manager.I.Inventory.ItemList[ i ] )
            if( Manager.I.Inventory.ItemList[ i ].IsGameplayResource )
                UpdateItem( i, s.ItemCountList[ i ] );
            if( s.HeroHP > 0 )
                G.Hero.Body.Hp = s.HeroHP;

            for( int i = 0; i < G.Hero.Body.Sp.Count; i++ )                                                  // Copy sector spell data to hero
                G.Hero.Body.Sp[ i ].Copy( s.SpellList[ i ] );
        }
        else
        {
            for( int i = 0; i < Manager.I.Inventory.ItemList.Count; i++ )                                    // Warning: Dont use G.GIT here: the access is directly to the itemlist via loop and id    
            if( Manager.I.Inventory.ItemList[ i ] )
            if( Manager.I.Inventory.ItemList[ i ].IsGameplayResource )
                UpdateItem( i, 0 );
        }

        SectorHint.S = Sector.GetPosSector( pos );
        Map.I.RM.HeroSector = s;        
        SectorHint.UpdateSectorEntranceMessage();
        Quest.I.UpdateArtifactData( ref Map.I.Hero );
        Map.I.FreeCamAreaZoom = 0;
        Map.TrailTiles = null;
        G.Hero.Spr.gameObject.SetActive( true );
        G.Hero.Body.Shadow.gameObject.SetActive( true );
        Secret.FlushSecretList = new List<Vector2>();

        if( s.MoveOrder == null )
        {
            s.CreateMoveOrderList();                                                                     // Creates the Move Order List
        }

        if( Map.I.RM.HeroSector == null || Map.I.RM.HeroSector.Type == Sector.ESectorType.GATES )        // Gates
            UI.I.GameLevelText.text = "Gate";
        else
            if( Map.I.RM.HeroSector.Type == Sector.ESectorType.LAB )                                      // Lab
                UI.I.GameLevelText.text = "Main Base";
            else
            {
                int id = Map.I.RM.HeroSector.Number - 1;
                if( id >= 0 ) UI.I.GameLevelText.text = UI.I.OrdinalNumberList[ id ] + " Cube ";
            }
        if( Map.I.RM.HeroSector.Type != Sector.ESectorType.NORMAL )
            UI.I.AreasText.text = "";
        UI.I.UpdBeastText = true;

        MapSaver.I.UpdateIntroMessage( MapSaver.I.Message, "Message:" );                                   // Displays Intro message
    }
    public static void UpdateItem( int i, float num = -2 )                                                 // Warning: Dont use G.GIT here: the access is directly to the itemlist via loop and id    
    {
        Item it = Manager.I.Inventory.ItemList[ i ];
        if( num != -2 )
        {
            Item.SetAmt( it.Type, num );
            return;
        }

        if( it.IsGlobalGameplayResource == false )
            Item.SetAmt( it.Type, Item.GetBn( it.Type ));
    }
    public static void UpdateOutAreaMovement()
    {
        Sector s = Map.I.RM.HeroSector;
        if( s.Type != ESectorType.NORMAL ) return;

        UpdateDynamicObjectsMovement();                                                                         // Dynamic objects movement update

        for( int type = 1; type <= 3; type++ )                                                                  // Movement order: 1 Raft
        for( int i = s.MoveOrder.Count - 1; i >= 0; i-- )                                                       //                 2 Boulder
             ProcessMovement( i, type );                                                                        //                 3 Other Monsters

        //Map.I.UpdateTransLayerTilemap( true );  // new to optimize
        
        Map.I.DirtyFogList = new List<int>();
        s.AliveNormalMonsters = 0;
        Map.I.TicTacMonsterCount = 0;
        AwakenNormalMonsters = 0;
        for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )
        for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
            {
                if( Map.I.Gaia2[ xx, yy ] )                                                                     // Counts tic tac monster number
                {
                    if( Map.I.Gaia2[ xx, yy ].Control )
                    if( Map.I.Gaia2[ xx, yy ].Control.TickBasedMovement )
                        Map.I.TicTacMonsterCount++;
                }

                if( Map.I.Unit[ xx, yy ] )
                {
                    Unit un = Map.I.Unit[ xx, yy ];
                    if( un.ValidMonster )                                                                      // Counts alive monsters
                    if( un.Body.OptionalMonster == false )
                    {
                        s.AliveNormalMonsters++;
                        if( un.Control.Resting == false )
                            AwakenNormalMonsters++;
                    }
                                  
                    if( un.Control.TickBasedMovement )
                        Map.I.TicTacMonsterCount++;

                    un.UpdateFogDirty();                                                             // is fog dirty?
                    un.Control.BeingMudPushed = false;
                }
            }

        s.AliveFlyingMonsters = 0;                                                                   // Flying Units
        AwakenFlyingMonsters = 0;
        if( s.TotAliveFlyingMonsters == -1 )
            s.TotAliveFlyingMonsters = G.HS.Fly.Count;

        for( int i = G.HS.Fly.Count - 1; i >= 0; i-- )
        {
            if( G.HS.Fly[ i ].Control.Mother == null ) 
            if( G.HS.Fly[ i ].Body.IsDead == false )
            if( G.HS.Fly[ i ].ValidMonster )
            if( G.HS.Fly[ i ].Body.OptionalMonster == false )                                   // Counts Alive Flying monster number
            {
                s.AliveFlyingMonsters++;
                if( G.HS.Fly[ i ].Control.Resting == false )
                    AwakenFlyingMonsters++;
            }

            if( G.HS.Fly[ i ].Control.TickBasedMovement )
                Map.I.TicTacMonsterCount++;

            G.HS.Fly[ i ].UpdateText();

            G.HS.Fly[ i ].UpdateFogDirty();
        }

        TotalAwakenMonsters = AwakenNormalMonsters + AwakenFlyingMonsters;                            // Total Awaken monsters account

        if( Map.I.PlatformDeath || Map.I.CubeDeath )                                                  // Updates Cube death for hero
            G.Hero.Body.UpdateCubeDeath();

        if( s.Type == ESectorType.NORMAL )
        {
            Map.I.RM.CheckGate( false, new Vector2( -1, -1 ) );                                       // Checks for cube gate openning
        }

        Map.I.UpdateHeroDisplacement();                                                               // Hero Displacement vault power
    }

    public static void ProcessMovement( int i, int type )
    {
        Unit un = G.HS.MoveOrder[ i ];
        if( un.TileID == ETileType.INACTIVE_HERO ) return;                           // Restrict by movement order
        if( type == 1 &&   un.TileID != ETileType.RAFT ) return;
        if( type == 2 &&   un.TileID != ETileType.BOULDER ) return;
        if( type == 3 && ( un.TileID == ETileType.RAFT    ||
                           un.TileID == ETileType.BOULDER ) ) return;

        un.Control.IsBeingPushedAgainstObstacle = false;
        un.UpdateAllAttacks( false );
        un.Control.UpdateIt();                                                       // Updates monster movement

        if( Controller.UnitHasBeenKilledWhileMoving ) return;
        un.UpdateAllAttacks( true );                                                 // Updated Monster Attack

        bool fire = un.Control.RealtimeMoveTried;                                    // Position History Update
        if( un.TileID == ETileType.BOULDER ) fire = true;                            // Burn Boulder immediatelly
        if( un.Control.RealtimeMoveTried || Map.I.MaskMove )
            un.Control.UpdatePositionHistory();
        un.Control.RealtimeMoveTried = false;

        if( fire || Map.I.MaskMove )
            un.CheckFireDamage();                                                    // Check fire damage                
    }

    public static void UpdateDynamicObjectsMovement()
    {
        Sector s = Map.I.RM.HeroSector;
        if( s.Type != ESectorType.NORMAL ) return;
        if( s.DynamicObjects == null ) return;
        if( s.DynamicObjects.Count < 1 ) return;

        for( int i = 0; i < s.DynamicObjects.Count; i++ )
        if( s.DynamicObjects[ i ].Control )
        {
            s.DynamicObjects[ i ].Control.UpdateIt();
        }
    }
    public void CreateMoveOrderList()
    {
        if( Type != ESectorType.NORMAL ) return;

        MoveOrder = new List<Unit>();
        for( int pass = 1; pass <= 4; pass++ )
        for( int y = ( int ) Area.yMin - 1; y < Area.yMax + 1; y++ )
        for( int x = ( int ) Area.xMin - 1; x < Area.xMax + 1; x++ )
        {
            if( Map.I.Unit[ x, y ] != null )
            {
                if( pass == 1 && Map.I.Unit[ x, y ].TileID == ETileType.BOULDER )
                {
                    MoveOrder.Add( Map.I.Unit[ x, y ] );
                    if( Map.I.Unit[ x, y ].Control )
                        Map.I.Unit[ x, y ].Control.MoveOrderID = MoveOrder.Count - 1;
                }
                else
                if( pass == 2 && Map.I.Unit[ x, y ].ValidMonster &&
                    Map.I.Unit[ x, y ].TileID != ETileType.FROG )
                {
                    MoveOrder.Add( Map.I.Unit[ x, y ] );
                    if( Map.I.Unit[ x, y ].Control )
                        Map.I.Unit[ x, y ].Control.MoveOrderID = MoveOrder.Count - 1;
                }
                else
                if( pass == 3 && Map.I.Unit[ x, y ].TileID == ETileType.FROG )
                {
                    MoveOrder.Add( Map.I.Unit[ x, y ] );
                    if( Map.I.Unit[ x, y ].Control )
                        Map.I.Unit[ x, y ].Control.MoveOrderID = MoveOrder.Count - 1;
                }
                else
                if( pass == 4 && ( Map.I.Unit[ x, y ].TileID == ETileType.BARRICADE ||
                                   Map.I.Unit[ x, y ].TileID == ETileType.ALTAR ||
                                   Map.I.Unit[ x, y ].TileID == ETileType.TOWER ||
                                   Map.I.Unit[ x, y ].TileID == ETileType.ORB ||
                                   Map.I.Unit[ x, y ].TileID == ETileType.FAN ) )
                {
                    MoveOrder.Add( Map.I.Unit[ x, y ] );
                }
            }
        }
        //Util.DebugList<Unit>( MoveOrder );
    }

    public static void FinishSectorCleared( Sector hs )
    {
        hs.GiveCummulativeGlobalPrizes();                                                             // gives incremental bonuses like clover and xp

        if( hs.Cleared )
            hs.GiveGlobalPrizes();                                                                    // Finally gives Global prizes gained

        if( Map.I.FinalizeCube == true )                                                              // Stepping the gate code
        {
            for( int i = 0; i < Map.I.RM.RMD.GoalList.Length; i++ )
            {
                RandomMapGoal go = Map.I.RM.RMD.GoalList[ i ];
                go.Trig.UpdateIt();
                go.CheckConquest();                                                                   // Finally, Conquer cube goal
            }

            GiveCubeClearedDefaultPrizes();                                                           // Gives Default prizes like shell and cog, etc for clearing cube

            int sec = Map.I.LevelStats.SectorsCleared;                                                // Alternate Starting Cube
            if( sec > Map.I.RM.RMD.MaxCubes ) sec = Map.I.RM.RMD.MaxCubes;
            if( Map.I.RM.AvailableCubesForPlaying > -1 )
                if( sec > Map.I.RM.AvailableCubesForPlaying )
                    sec = Map.I.RM.AvailableCubesForPlaying;

            if( Helper.I.StartFromLastEditedCube == false )
            {
                if( sec > Item.GetNum( ItemType.Starting_Cube ) )                                     // Select next cube
                    Item.SetAmt( ItemType.Starting_Cube, sec,
                    Inventory.IType.Inventory, true, Map.I.RM.CurrentAdventure );
                Map.I.RM.DungeonDialog.StartingCubePopup.value = "Start at Cube #" + ( sec + 1 ) + " ";
            }

            Map.I.LevelStats.SectorsCleared++;                                                        // Sectors Cleared increment

            for( int i = 0; i < 8; i++ )                                                              // Destroys the Bamboo
                G.Hero.Body.Sp[ i ].BambooSize = 0;
            Item.SetAmt( ItemType.Res_Stitches, 0 );                                                  // to allow gate openning
            Item.SetAmt( ItemType.Res_BirdCorn, 0 );
            G.Hero.RemoveChains( true );                                                              // Remove Hero Chains

            Secret.FlushSecrets( hs );                                                                // Save secrets

            RandomMapGoal.SaveAll();                                                                  // Save all Goals information
                                                                                                      // ATTENTION:  Saving inventory and goal before clearing the cube  can cause a bug because you can reload the cube and keep the prizes: to avoid this thats why it was moved from goals.conquer to here
            Manager.I.Inventory.Save();                                                               // Save Inventory
        }
        else
        {
            if( hs.PrizeRes.Count > 0 )
            {
                Message.RedMessage( "You earned global prizes!\nClear " +
                "the cube and return\n to the gate to claim them." );                                 // prizes remaining message            
            }
        }

        for( int i = 0; i < G.HS.CheckpointList.Count; i++ )
        {
            Unit but = Map.I.GetUnit( ETileType.ALTAR, G.HS.CheckpointList[ i ] );                    // Kill random altars on cube leave
            if( but && but.Altar.RandomAltar )
                but.Kill();
        }

        Map.I.HeroTargetSprite.gameObject.SetActive( false );                                         // Disable corn frontal target                         
         
        GS.DeleteCubeSaves( hs );                                                                     // Delete cube saves
        Secret.DestroyAllSecrets( hs );                                                                   // Destroy all secrets
        Map.I.FinalizeCube = false;
    }

    private static void GiveCubeClearedDefaultPrizes()
    {
        float shell = Util.GetCurveVal( G.HS.Number, Map.I.RM.RMD.MaxCubes, 
        Map.I.RM.RMD.ShellStartBonus, Map.I.RM.RMD.ShellEndBonus, Map.I.RM.RMD.CubeClearBonusCurve );
        float bn = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.INCREASE_CUBE_CLEAR_DEFAULT_BONUS );       // tech bonus

        shell += Util.Percent( bn, shell ); 
        shell = Util.FloatSort( shell );
        if( shell > 0 )                                                                                           // Shell bonus
        {
            Item.AddItem( Inventory.IType.Inventory, ItemType.Shell, shell, true );
        }

        float cog = Util.GetCurveVal( G.HS.Number, Map.I.RM.RMD.MaxCubes,
        Map.I.RM.RMD.CogStartBonus, Map.I.RM.RMD.CogEndBonus, Map.I.RM.RMD.CubeClearBonusCurve );

        cog += Util.Percent( bn, cog );
        cog = Util.FloatSort( cog );
        if( cog > 0 )                                                                                             // Cog bonus
        {
            Item.AddItem( Inventory.IType.Inventory, ItemType.Cog, cog, true );
        }

        float xp = Util.GetCurveVal( G.HS.Number, Map.I.RM.RMD.MaxCubes,
        Map.I.RM.RMD.XpStartBonus, Map.I.RM.RMD.XpEndBonus, Map.I.RM.RMD.CubeClearBonusCurve );

        xp += Util.Percent( bn, xp );
        xp = Util.FloatSort( xp );
        if( xp > 0 )                                                                                              // xp bonus
        {
            Item.AddItem( Inventory.IType.Inventory, ItemType.Quest_XP, xp, true );
        }

        float hc = Util.GetCurveVal( G.HS.Number, Map.I.RM.RMD.MaxCubes,
        Map.I.RM.RMD.HoneycombStartBonus, Map.I.RM.RMD.HoneycombEndBonus, Map.I.RM.RMD.CubeClearBonusCurve );

        hc += Util.Percent( bn, hc );
        hc = Util.FloatSort( hc );
        if( hc > 0 )                                                                                              // hc bonus
        {
            Item.AddItem( Inventory.IType.Inventory, ItemType.HoneyComb, hc, true );
        }
        
        GlobalAltar.UpdateEvolution( "cube clear");                                                               // Random altar evolution
    }
    public static void UpdateSectorCleared()
    {
     //   if( Map.I.AdvanceTurn == false && Map.I.FreeCamMode == false ) return;
        Sector hs = Map.I.RM.HeroSector;
        if( hs != null && hs.Type == Sector.ESectorType.NORMAL )
        {
            if( hs.Cleared ) return;
            hs.Discovered = true;

            float num = Map.I.RM.HeroSector.GetRemainingMonsters();

            if( num == 0 )                                                                                   // Clear sector now
            {
                string msg = "Cube Purged!";
                if( Map.I.RM.RMD.CubeClearHPBonus > 0 && hs.Cleared == false )
                {
                    float amt = Map.I.RM.RMD.CubeClearHPBonus;
                    msg += "\nHealing +" + amt.ToString( "0." ) + " HP.";                                     // HP Restore
                    Map.I.Hero.Body.AddHP( amt );
                }

                if( hs.Cleared == false )
                {
                    Map.I.FinalizeCube = true;
                    Message.CreateMessage( ETileType.NONE, ItemType.Res_HP, msg, 
                    Map.I.Hero.Pos, new Color( 0, 1, 0, 1 ), true, true, 7, 3 );
                    KillOptionalMonsters();
                }

                hs.Cleared = true;
                UI.I.SetBigMessage( "This Cube is Safe Now...\n\nGo to the Gate to Claim Prizes.",
                Color.yellow, 8, 0, 480, 80 );                                                                // Displays message
                MasterAudio.PlaySound3DAtVector3( "Area Cleared", G.Hero.transform.position );
                Map.I.ZoomMode = 2;
                Map.I.ClearRoomDoorAtPos( new Vector2( -1, -1 ), true );                                      // Clear the room door

                float upchc = AdventureUpgradeInfo.GetStat(
                EAdventureUpgradeType.CUBE_CLEAR_UPGRADE_CHEST_CHANCE );                                      // upgrade chest
                Chests.UpgradeChests( upchc );

                GlobalAltar.I.UpdateButcherSpawning();                                                        // Update Butcher Spawning

                if( hs.TimeSpentOnCube > 10 )
                {
                    //iTween.ShakePosition( Manager.I.Camera.gameObject, new Vector3( .4f, .4f, 0 ), 1.1f );  // Camera shake effect
                    Map.I.RM.CreateCubeLightningEffect( hs, "Cube Cleared" );
                }
            }
        }
    }

    public static void KillOptionalMonsters()
    {
        Sector s = Map.I.RM.HeroSector;
        for( int i = s.MoveOrder.Count - 1; i >= 0; i-- )  
        if ( s.MoveOrder[ i ].ValidMonster )
             s.MoveOrder[ i ].Kill();
        for( int i = G.HS.Fly.Count - 1; i >= 0; i-- )
        if ( G.HS.Fly[ i ].ValidMonster )
             G.HS.Fly[ i ].Kill();
    }

    public static Vector2 GetMirrorPos( Vector2 pos, Sector s )
    {
        Vector2 res = new Vector2( pos.x, pos.y );
        Vector2 mid = new Vector2( s.Area.xMin + 14, s.Area.yMin + 14 );
        Vector2 rel = mid - res;

        if( s.FlipX )
        {
            if( res.x > mid.x ) res.x = ( int ) ( mid.x + rel.x );
            if( res.x < mid.x ) res.x = ( int ) ( mid.x + rel.x );
        }

        if( s.FlipY )
        {
            if( res.y > mid.y ) res.y = ( int ) ( mid.y + rel.y );
            if( res.y < mid.y ) res.y = ( int ) ( mid.y + rel.y );
        }
        return res;
    }

    public static void DeleteUndesiredAreas( Sector s )
    {
        for( int i = 0; i < s.AreaList.Count; i++ )
        {
            Area ar = s.AreaList[ i ]; 
            if( ( Map.I.RM.SD.CreateRedArea    == false && ar.AreaDragID == ETileType.AREA_DRAG  ) ||
                ( Map.I.RM.SD.CreateGreenArea  == false && ar.AreaDragID == ETileType.AREA_DRAG2 ) ||
                ( Map.I.RM.SD.CreateBlueArea   == false && ar.AreaDragID == ETileType.AREA_DRAG3 ) ||
                ( Map.I.RM.SD.CreateYellowArea == false && ar.AreaDragID == ETileType.AREA_DRAG4 ) )
            {
                for( int y = ( int ) ar.P2.y; y <= ar.P1.y; y++ )
                    for( int x = ( int ) ar.P1.x; x <= ar.P2.x; x++ )
                    {
                        if( Map.I.AreaID[ x, y ] == ar.GlobalID )
                        {
                            Map.I.AreaID[ x, y ] = -1;
                            Map.I.ClearAreaTile( Map.I.TransTileMap, new Vector2( x, y ) );
                        }
                    }
                ar.InfoText.gameObject.SetActive( false );
                ar.IsFake = true;
                //Quest.I.Dungeon.AreaList.Remove( ar );
                //s.AreaList.RemoveAt( i );
            }
        }
    }

    public static bool IsPtInCube( Vector2 tg )
    {
        if( Map.I.RM.HeroSector.Area.Contains( tg ) ) return true;
        return false;
    }

    public static void UpdateSectorChanging()
    {        
        Sector.ESectorType oldtype = Map.I.RM.HeroSectorType;
        Sector old = Map.I.RM.HeroSector;
        Map.I.RM.HeroSectorType = Sector.GetPosSectorType( Map.I.Hero.Pos );
        Map.I.RM.HeroSector = Sector.GetPosSector( Map.I.Hero.Pos );

        if( Map.I.RM.HeroSector.Type == ESectorType.GATES )
        {
            Map.I.RM.LastGateTileStepped = G.Hero.Pos;
            Map.I.RM.LockWayPointJump = false;
        }

        if( oldtype != Map.I.RM.HeroSectorType )
            {
                Sector.EnterSector( old, Map.I.Hero.Pos, Map.I.RM.HeroSector );
            }
    }

    internal int GetRemainingMonsters()
    {
        if( AliveNormalMonsters == -1 ) return -1;  // return -1 if not yet calculated
        if( AliveFlyingMonsters == -1 ) return -1;
        return AliveNormalMonsters + AliveFlyingMonsters;
    }
    public void InitClovers()
    {
        CloverPosList = new List<Vector2>();                                                     // Init clover positions
        if( Manager.I.GameType != EGameType.CUBES ) return;

        if( G.HS.TotalClovers == -1 )                    
        {
            int tot = Map.I.RM.RMD.CloversPerCube;
            float perc = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.CLOVER_CHANCE );    // tech
            float num = tot + Util.Percent( perc, tot );
            tot = Util.FloatSort( num );
            G.HS.TotalClovers = tot;                                                             // Initializes total clovers just once
        }

        int total = G.HS.TotalClovers;
        for( ; ; )
        {
            int x = Random.Range( 0, TSX - 1 );
            int y = Random.Range( 0, TSY - 1 );
            if( CloverPosList.Contains( new Vector2( x, y ) ) == false )
            {
                Vector2 pos = new Vector2( x + Area.xMin, y + Area.yMin );
                CloverPosList.Add( pos );                                                        // add clover to position
                if( --total <= 0 ) break;
            }
        }
    }
    public static void UpdateCloverPicking()
    {
        if( Manager.I.GameType != EGameType.CUBES ) return;
        if( G.Hero.Control.PathFinding.Path != null ) 
        if( G.Hero.Control.PathFinding.Path.Count > 0 ) return;

        if( G.HS.TotalClovers == -1 )                                                                            // Init clover positions if not yet done
            G.HS.InitClovers();

        Sector s = Map.I.RM.HeroSector;
        if( s.CloverPosList.Contains( G.Hero.Pos ) )
        {
            if( G.HS.CloverPickCount >= G.HS.TotalClovers )                                                      // Max clovers picked
                return;
           
            Item.AddItem( ItemType.Clover, 1 );                                                                  // gives clover
            Item.AddItem( ItemType.Quest_XP, Map.I.RM.RMD.QuestXPPerClover );                                    // gives quest xp

            if( Util.Chance( 20 ) )           // todo: Add clover prize bonus tech to  improve these chances
            {
                Item.AddItem( ItemType.Chest_Points, 1 );                                                        // gives chest points
            }

            if( Util.Chance( 15 ) )
            {
                GlobalAltar.SortBonus( 1, false );                                                               // evolve altar
            }

            s.CloverPosList.Remove( G.Hero.Pos );
            float upchc = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.CLOVER_UPGRADE_CHEST_CHANCE );     // upgrade chest
            Chests.UpgradeChests( upchc, "Clover" );
            G.HS.CloverPickCount++;
        }  
    }
    public void AddFlying( Unit un )
    {
        if( Fly.Contains( un ) == false )
            Fly.Add( un );
        else Debug.LogError( "exists " + un );
    }

    internal void AddGameplayPrize( Item it, float amount )
    {
        int index = PrizeRes.IndexOf( it.Type );
        if( index >= 0 )
        {
            PrizeResAmt[ index ] += amount;                           // Add to existing amount
        }
        else
        {
            PrizeRes.Add( it.Type );                                  // create new item in the list
            PrizeResAmt.Add( amount );
        }
        if( PrizeStepList.Contains( G.Hero.Pos ) == false )
            PrizeStepList.Add( G.Hero.Pos );                          // Add to step list
    }
    private void GiveGlobalPrizes()
    {
        for( int i = 0; i < PrizeRes.Count; i++ )
        {
            Item.ForceMessage = true;
            Item.AddItem( PrizeRes[ i ], PrizeResAmt[ i ] );          // gives prizes
        }
        PrizeRes = new List<ItemType>();                              // resets lists
        PrizeResAmt = new List<float>();
        //PrizeStepList = new List<Vector2>();
    }
    public void GiveCummulativeGlobalPrizes()
    {
        for( int i = PrizeRes.Count - 1; i >= 0; i-- )
        {
            if( PrizeRes[ i ] == ItemType.Quest_XP ||                  // these ones must be given on game over or gate step since they should be incremental
                PrizeRes[ i ] == ItemType.Clover )
            {
                Item.ForceMessage = true;
                Item.AddItem( PrizeRes[ i ], PrizeResAmt[ i ] );       // adds items
                PrizeRes.RemoveAt( i );
                PrizeResAmt.RemoveAt( i );                             // removes lists
            }
        }
    }
}
