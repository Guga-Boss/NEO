using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using PathologicalGames;
using Sirenix.OdinInspector;

#region Enums
public enum EMineType
{
    NONE = -1, DIRECTIONAL, ROUND, SQUARE, DOUBLE, UNITARY, INDESTRUCTIBLE,
    HOOK, SHACKLE, SPIKE_BALL, LADDER, BRIDGE, SUSPENDED, WEDGE_LEFT, WEDGE_RIGHT, 
    VAULT, TUNNEL, SQUARE_BASE, RED = 20,
}
public enum EMineBonusEffType
{
    NONE = -1, GIVE_ITEM, ADD_INTEREST, ITEM_PICKUP_ADD_INTEREST, FAILED_MINE_BONUS_CHANCE, NEIGHBOR_MINES_GET_BONUS_CHANCE,
    FREE_PUSH_X_OBJECTS, ITEM_SET_DEFAULT_MINING_CHANCE, MINE_BEHIND_GET_X_RESOURCE,
    MINE_BEHIND_ADD_X_CHANCE, MINE_BEHIND_ADD_COMPOUND_X_CHANCE, FAILED_MOVE_TO_RAND_POS,
    JUMP_OVER_MINES_FOR_RESOURCE, DESTROY_THIS_VAULT, DESTROYED_MINE_SPAWN_ARROW,
    MINES_AROUND_DESTROYED_GIVE_RESOURCE, GIVE_INVENTORY_BONUS_PERCENT, SET_DEFAULT_MINING_CHANCE,
    BOOMERANG_CAN_MINE, FIREBALL_CAN_MINE, MINES_AROUND_CHISEL_BONUS_CHANCE, HERO_CHANGE_BOULDER_DIR, RESET_RESOURCE,
    STRIKE_CLOSEST_ORB = 40, PICKUP_CLOSEST_RESOURCE,
    ENABLE_ALCOVE_PUSH,
    BAG_GIVES_EXTRA_RESOURCE = 50,
    BAG_GIVE_INVENTORY_BONUS = 100,
    MUDDY_ROCK_SPAWN_BOULDER = 150,
    SPREAD_X_ITEMS = 200, SPREAD_X_MINING_PRIZE, SPREAD_X_HAMMER, SPREAD_X_ROPE, SPREAD_X_SWAPPER, SPREAD_X_HOLE, SPREAD_X_CHISEL, SPREAD_X_DYNAMITE,
    SPREAD_X_ARROW_MINE, SPREAD_X_WHEEL, SPREAD_X_MAGNET, SPREAD_X_CANNON, SPREAD_X_COG, SPREAD_X_JUMPER, SPREAD_X_GLOVE,
    SPREAD_MUD_AROUND_MINE = 230, SPREAD_WEDGE_AROUND_MINE,
    RANDOM_ITENS_CREATED_BONUS = 250,
    ADD_BOULDER_PUSH_POWER = 300,
    LINKED_CRACKED_MINE_GET_ITEM = 350, CRACKED_GIVE_EXTRA_ITEM, DYNAMITE_EXPLODE_CRACKED,
    HERO_EXPAND_MUD = 360, FAILED_INFLATION_PER_SIDE,
    VAULT_POWER = 370, NEXT_VAULT_POWER, VAULT_REUSE,
    void1, DESTROYED_GIVE_EXTRA_ITEM, DESTROYED_CRACK_NEIGHBOR_CHANCE, FAILED_CRACK_NEIGHBOR_CHANCE,
    MAX_FAILED_COST = 400, MAX_SUCCESS_COST, GET_EXTRA_ITEM, X_DESTROYED_GIVE_ONE_ITEM,
    MAX_TIC_TAC_MOVES = 450, DISPLACE_HERO_ON_TICK_X
}
public enum EMineBonusCnType
{
    NONE = -1, ACTIVATE_VAULT, MINE_X_MINE, COLLECT_X_RESOURCE, HAVE_X_RESOURCE_INVENTORY, DESTROY_X_ROUND_MINE,
    DESTROY_X_SQUARE_MINE, HAVE_X_MINE_AROUND_HERO, MISS_X_ATTEMPTS, GET_X_VAULT_POINTS, KILL_X_MONSTERS, ENTER_THE_CUBE,
    DESTROY_X_MINES_IN_SEQUENCE, MISS_SAME_MINE_IN_SEQUENCE_X_TIMES, GET_X_GLOBAL_VAULT_POINTS, ACTIVATE_X_VAULTS, CONQUER_X_VAULTS
    //ideas: Move mine x tiles (manhattan from original position,  add to spread last cubes )
}
#endregion

[System.Serializable]
public class Mine : MonoBehaviour
{
    #region Variables
    [TabGroup( "Main" )]
    public bool Activated = false;
    [TabGroup( "Main" )]
    public EMineBonusEffType MineBonusEffType = EMineBonusEffType.NONE;
    [TabGroup( "Main" )]
    public EMineBonusCnType MineBonusCnType = EMineBonusCnType.NONE;
    [TabGroup( "Main" )]
    public float MineBonusVal1 = -1;
    [TabGroup( "Main" )]
    public float MineBonusVal2 = -1;
    [TabGroup( "Main" )]
    public float MineBonusVal3 = -1;
    [TabGroup( "Main" )]
    public float MineBonusVal4 = -1;
    [TabGroup( "Main" )]
    public bool MineBonusFailed = false;
    [TabGroup( "Main" )]
    public bool MineBonusActive = true;
    [TabGroup( "Main" )]
    public bool MineBonusGiven;
    [TabGroup( "Main" )]
    public bool UpdateText = true;
    [TabGroup( "Main" )]
    public int FrontSupport = -1;
    [TabGroup( "Main" )]
    public int BackSupport = -1;
    [TabGroup( "Link" )]
    public Unit Unit;
    [TabGroup( "Mine" )]
    public float MineRotationSpeed = 0;
    [TabGroup( "Mine" )]
    public int MineDestroyedCount = 0, MineMinedCount = 0, RoundMineMinedCount = 0, SquareMineMinedCount = 0,
    MissedAttempts, VaultPoints = -1, InitialVaultPoints = -1, KilledMonsters = 0, HitCount = 0, VaultsActivated = 0, VaultsConquered = 0;
    [TabGroup( "Mine" )]
    public float ResourceCollected = 0, AnimateIconTimer;
    [TabGroup( "Power Up" )]
    public EDirection MineBonusDir = EDirection.NONE;
    [TabGroup( "Power Up" )]
    public bool CogMine, ArrowMine, DynamiteMine, RopeMine, SwapperMine, HoleMine, HammerMine, 
    ChiselMine, SpikedMine, StickyMine, CannonMine, MagnetMine, WheelMine, JumperMine, GloveMine;
    [TabGroup( "Power Up" )]
    public int CogRotationDir = 1;
    [TabGroup( "Power Up" )]
    public int MineBonusUses = 0;
    public static bool LastMessageWasBonus = false, TunnelTraveling = false, ProceedTrip = false;
    public static List<Vector2> VaultEffTargetList;
    public static int TotBonus = 16;
    public static int BonusID = -1;
    public static Unit BonusMine = null;
    public static int OldZoom = -1;
    public static bool UpdateChiselText = false, SafeMining, UpdateAllMinesText = false;
    #endregion
    #region Init
    public void Copy( Mine mn )
    {
        Activated = mn.Activated;
        MineBonusDir = mn.MineBonusDir;
        MineBonusGiven = mn.MineBonusGiven;
        MineBonusFailed = mn.MineBonusFailed;
        MineBonusActive = mn.MineBonusActive;
        MineBonusEffType = mn.MineBonusEffType;
        MineBonusCnType = mn.MineBonusCnType;
        MineBonusVal1 = mn.MineBonusVal1;
        MineBonusVal2 = mn.MineBonusVal2;
        MineBonusVal3 = mn.MineBonusVal3;
        MineBonusVal4 = mn.MineBonusVal4;
        MineDestroyedCount = mn.MineDestroyedCount;
        MineMinedCount = mn.MineMinedCount;
        RoundMineMinedCount = mn.RoundMineMinedCount;
        SquareMineMinedCount = mn.SquareMineMinedCount;
        MissedAttempts = mn.MissedAttempts;
        ResourceCollected = mn.ResourceCollected;
        VaultPoints = mn.VaultPoints;
        InitialVaultPoints = mn.InitialVaultPoints;
        KilledMonsters = mn.KilledMonsters;
        FrontSupport = mn.FrontSupport;
        BackSupport = mn.BackSupport;
        AnimateIconTimer = mn.AnimateIconTimer;
        RopeMine = mn.RopeMine;
        SwapperMine = mn.SwapperMine;
        HoleMine = mn.HoleMine;
        HammerMine = mn.HammerMine;
        ChiselMine = mn.ChiselMine;
        SpikedMine = mn.SpikedMine;
        StickyMine = mn.StickyMine;
        DynamiteMine = mn.DynamiteMine;
        CogMine = mn.CogMine;
        CogRotationDir = mn.CogRotationDir;
        MineBonusUses = mn.MineBonusUses;
        ArrowMine = mn.ArrowMine;
        CannonMine = mn.CannonMine;
        MagnetMine = mn.MagnetMine;
        WheelMine = mn.WheelMine;
        JumperMine = mn.JumperMine;
        MineRotationSpeed = mn.MineRotationSpeed;
        HitCount = mn.HitCount;
        VaultsActivated = mn.VaultsActivated;
        VaultsConquered = mn.VaultsConquered;
        GloveMine = mn.GloveMine;
    }
    public void Save()
    {
        GS.W.Write( Activated );
        GS.W.Write( MineBonusGiven );
        GS.W.Write( MineBonusFailed );
        GS.W.Write( ( int ) MineBonusEffType );
        GS.W.Write( ( int ) MineBonusCnType );
        GS.W.Write( MineBonusVal1 );
        GS.W.Write( MineBonusVal2 );
        GS.W.Write( MineBonusVal3 );
        GS.W.Write( MineBonusVal4 );
        GS.W.Write( MineBonusActive );
        GS.W.Write( MineDestroyedCount );
        GS.W.Write( MineMinedCount );
        GS.W.Write( RoundMineMinedCount );
        GS.W.Write( SquareMineMinedCount );
        GS.W.Write( ResourceCollected );
        GS.W.Write( MissedAttempts );
        GS.W.Write( VaultPoints );
        GS.W.Write( InitialVaultPoints );
        GS.W.Write( KilledMonsters );
        GS.W.Write( FrontSupport );
        GS.W.Write( BackSupport );
        GS.W.Write( RopeMine );
        GS.W.Write( SwapperMine );
        GS.W.Write( HoleMine );
        GS.W.Write( HammerMine );
        GS.W.Write( ChiselMine );
        GS.W.Write( SpikedMine );
        GS.W.Write( StickyMine );
        GS.W.Write( DynamiteMine );
        GS.W.Write( CogMine );
        GS.W.Write( CogRotationDir );
        GS.W.Write( ArrowMine );
        GS.W.Write( CannonMine );
        GS.W.Write( MagnetMine );
        GS.W.Write( WheelMine );
        GS.W.Write( JumperMine );
        GS.W.Write( GloveMine );
        GS.W.Write( MineBonusUses );
        GS.W.Write( HitCount );
        GS.W.Write( VaultsActivated );
        GS.W.Write( VaultsConquered );
    }

    public void Load()
    {
        Activated = GS.R.ReadBoolean();
        MineBonusGiven = GS.R.ReadBoolean();
        MineBonusFailed = GS.R.ReadBoolean();
        MineBonusEffType = ( EMineBonusEffType ) GS.R.ReadInt32();
        MineBonusCnType = ( EMineBonusCnType ) GS.R.ReadInt32();
        MineBonusVal1 = GS.R.ReadSingle();
        MineBonusVal2 = GS.R.ReadSingle();
        MineBonusVal3 = GS.R.ReadSingle();
        MineBonusVal4 = GS.R.ReadSingle();
        MineBonusActive = GS.R.ReadBoolean();
        MineDestroyedCount = GS.R.ReadInt32();
        MineMinedCount = GS.R.ReadInt32();
        RoundMineMinedCount = GS.R.ReadInt32();
        SquareMineMinedCount = GS.R.ReadInt32();
        ResourceCollected = GS.R.ReadSingle();
        MissedAttempts = GS.R.ReadInt32();
        VaultPoints = GS.R.ReadInt32();
        InitialVaultPoints = GS.R.ReadInt32();
        KilledMonsters = GS.R.ReadInt32();
        FrontSupport = GS.R.ReadInt32();
        BackSupport = GS.R.ReadInt32();
        RopeMine = GS.R.ReadBoolean();
        SwapperMine = GS.R.ReadBoolean();
        HoleMine = GS.R.ReadBoolean();
        HammerMine = GS.R.ReadBoolean();
        ChiselMine = GS.R.ReadBoolean();
        SpikedMine = GS.R.ReadBoolean();
        StickyMine = GS.R.ReadBoolean();
        DynamiteMine = GS.R.ReadBoolean();
        CogMine = GS.R.ReadBoolean();
        CogRotationDir = GS.R.ReadInt32();
        ArrowMine = GS.R.ReadBoolean();
        CannonMine = GS.R.ReadBoolean();
        MagnetMine= GS.R.ReadBoolean();
        WheelMine = GS.R.ReadBoolean();
        JumperMine = GS.R.ReadBoolean();
        GloveMine = GS.R.ReadBoolean();
        MineBonusUses = GS.R.ReadInt32();
        HitCount = GS.R.ReadInt32();
        VaultsActivated = GS.R.ReadInt32();
        VaultsConquered = GS.R.ReadInt32();
    }
    public static void ResetVars( Sector s = null )
    {
        if( s )
        {
            s.MineDestroyedCount = 0;
            s.RemainingCracked = 0;
            s.InitialGlobalVaultPoints = 0;
            s.GlobalVaultPoints = 0;
            s.MineMinedCount = 0;
            s.RoundMineMinedCount = 0;
            s.SquareMineMinedCount = 0;
            s.FailedMineBonusChance = 0;
            s.NextMineBonusChanceCount = 0;
            s.NextMineBonusChanceAmount = 0;
            s.FreePushableObjects = 0;
            s.DefaultMiningChanceItemID = -1;
            s.DefaultMiningChance = -1;
            s.BehindMineBonusItemID = -1;
            s.BehindMineBonusAmount = 0;
            s.BehindMineBonusTimes = 0;
            s.BehindMineBonusChanceAmount = 0;
            s.BehindMineBonusChanceTimes = 0;
            s.BehindMineBonusCompoundChanceAmount = 0;
            s.BehindMineBonusCompoundChanceTimes = 0;
            s.FailedMineMoveRandomly = 0;
            s.HeroJumpOverMinesCost = 0;
            s.HeroJumpOverMinesItemID = -1;
            s.BagExtraBonusItemID = -1;
            s.BagExtraBonusAmount = 0;
            s.BagExtraInventoryItemID = -1;
            s.BagExtraInventoryAmount = 0;
            s.DestroyedRockSpawnArrow = 0;
            s.AroundMineBonusItemID = -1;
            s.AroundMineBonusTimes = 0;
            s.AroundMineBonusAmount = 0;
            s.MuddyRockSpawnBoulder = 0;
            s.BoomerangCanMine = false;
            s.FireballCanMine = false;
            s.AlcovePush = 0;
            s.PickaxeInterest = 0;
            s.PickaxeInterestItemID = -1;
            s.PickaxeInterestItemMultiplier = 0;
            s.RandomItemsCreatedBonus = 0;
            s.MinesAroundChiselBonusChance = 0;
            s.DestroyedMinesInSequence = 0;
            s.MissSameMineInSequence = 0;
            s.HeroChangeBoulderDir = 0;
            s.BoulderPushPower = 1;
            s.BoulderSidePush = false;
            s.LinkedCrackedBnItemID = -1;
            s.LinkedCrackedBnAmount = 0;
            s.HeroExpandMud = 0;
            s.NextVaultExtraPower = 0;
            s.VaultReuse = 0;
            s.DestroyedGiveExtraItemID = -1;
            s.DestroyedGiveExtraItemAmount = 0;
            s.CrackedGiveExtraItemID = -1;
            s.CrackedGiveExtraItemAmount = 0;
            s.DynamiteExplodeCracked = 0;
            s.MaxCostMiningFailure = -1;
            s.MaxCostMiningFailureUses = 0;
            s.MaxCostMiningSuccess = -1;
            s.MaxCostMiningSuccessUses = 0;
            s.DestroyedCrackNeighborChance = 0;
            s.FailedCrackNeighborChance = 0;
            s.GetExtraItemID = -1;
            s.GetExtraItemAmount = 0;
            s.FailedInflationPerSide = 0;
            s.XDestroyedGiveItemID = -1;
            s.XDestroyedGiveItemAmount = 0;
            s.GloveTarget = new Vector2( -1, -1 );
            s.MaxTicTacMoves = -1;
            s.TicTacMoveCount = 1;
            s.DisplaceHeroTickNumber = -1;
            s.DisplaceHeroType = -1;  
        }
    }
    public static void SaveGlobals()
    {
        GS.SVector2( G.HS.GloveTarget );
        GS.W.Write( G.HS.MineDestroyedCount );
        GS.W.Write( G.HS.RemainingCracked );
        GS.W.Write( G.HS.MineMinedCount );
        GS.W.Write( G.HS.InitialGlobalVaultPoints );
        GS.W.Write( G.HS.GlobalVaultPoints );
        GS.W.Write( G.HS.RoundMineMinedCount );
        GS.W.Write( G.HS.SquareMineMinedCount );
        GS.W.Write( G.HS.FailedMineBonusChance );
        GS.W.Write( G.HS.NextMineBonusChanceCount );
        GS.W.Write( G.HS.NextMineBonusChanceAmount );
        GS.W.Write( G.HS.FreePushableObjects );
        GS.W.Write( G.HS.DefaultMiningChanceItemID );
        GS.W.Write( G.HS.DefaultMiningChance );
        GS.W.Write( G.HS.BehindMineBonusItemID );
        GS.W.Write( G.HS.BehindMineBonusAmount );
        GS.W.Write( G.HS.BehindMineBonusTimes );
        GS.W.Write( G.HS.BehindMineBonusChanceAmount );
        GS.W.Write( G.HS.BehindMineBonusChanceTimes );
        GS.W.Write( G.HS.BehindMineBonusCompoundChanceAmount );
        GS.W.Write( G.HS.BehindMineBonusCompoundChanceTimes );
        GS.W.Write( G.HS.FailedMineMoveRandomly );
        GS.W.Write( G.HS.HeroJumpOverMinesItemID );
        GS.W.Write( G.HS.HeroJumpOverMinesCost );
        GS.W.Write( G.HS.BagExtraBonusItemID );
        GS.W.Write( G.HS.BagExtraBonusAmount );
        GS.W.Write( G.HS.BagExtraInventoryItemID );
        GS.W.Write( G.HS.BagExtraInventoryAmount );
        GS.W.Write( G.HS.DestroyedRockSpawnArrow );
        GS.W.Write( G.HS.AroundMineBonusItemID );
        GS.W.Write( G.HS.AroundMineBonusTimes );
        GS.W.Write( G.HS.AroundMineBonusAmount );
        GS.W.Write( G.HS.MuddyRockSpawnBoulder );
        GS.W.Write( G.HS.BoomerangCanMine );
        GS.W.Write( G.HS.FireballCanMine );
        GS.W.Write( G.HS.AlcovePush );
        GS.W.Write( G.HS.PickaxeInterest );
        GS.W.Write( G.HS.PickaxeInterestItemID );
        GS.W.Write( G.HS.PickaxeInterestItemMultiplier );
        GS.W.Write( G.HS.RandomItemsCreatedBonus );
        GS.W.Write( G.HS.MinesAroundChiselBonusChance );
        GS.W.Write( G.HS.DestroyedMinesInSequence );
        GS.W.Write( G.HS.MissSameMineInSequence );
        GS.W.Write( G.HS.HeroChangeBoulderDir );
        GS.W.Write( G.HS.BoulderPushPower );
        GS.W.Write( G.HS.LinkedCrackedBnItemID );
        GS.W.Write( G.HS.LinkedCrackedBnAmount );
        GS.W.Write( G.HS.HeroExpandMud );
        GS.W.Write( G.HS.NextVaultExtraPower );
        GS.W.Write( G.HS.VaultReuse );
        GS.W.Write( G.HS.DestroyedGiveExtraItemID );
        GS.W.Write( G.HS.DestroyedGiveExtraItemAmount );
        GS.W.Write( G.HS.CrackedGiveExtraItemID );
        GS.W.Write( G.HS.CrackedGiveExtraItemAmount );
        GS.W.Write( G.HS.DynamiteExplodeCracked );
        GS.W.Write( G.HS.MaxCostMiningFailure );
        GS.W.Write( G.HS.MaxCostMiningFailureUses );
        GS.W.Write( G.HS.MaxCostMiningSuccess );
        GS.W.Write( G.HS.MaxCostMiningSuccessUses );
        GS.W.Write( G.HS.DestroyedCrackNeighborChance );
        GS.W.Write( G.HS.FailedCrackNeighborChance );
        GS.W.Write( G.HS.GetExtraItemID );
        GS.W.Write( G.HS.GetExtraItemAmount );
        GS.W.Write( G.HS.FailedInflationPerSide );
        GS.W.Write( G.HS.XDestroyedGiveItemID );
        GS.W.Write( G.HS.XDestroyedGiveItemAmount );
        GS.W.Write( G.HS.MaxTicTacMoves );
        GS.W.Write( G.HS.TicTacMoveCount );
        GS.W.Write( G.HS.DisplaceHeroTickNumber );
        GS.W.Write( G.HS.DisplaceHeroType );
    }
    public static void LoadGlobals()
    {
        G.HS.GloveTarget = GS.LVector2();
        G.HS.MineDestroyedCount = GS.R.ReadInt32();
        G.HS.RemainingCracked = GS.R.ReadInt32();
        G.HS.MineMinedCount = GS.R.ReadInt32();
        G.HS.InitialGlobalVaultPoints = GS.R.ReadInt32();
        G.HS.GlobalVaultPoints = GS.R.ReadInt32();
        G.HS.RoundMineMinedCount = GS.R.ReadInt32();
        G.HS.SquareMineMinedCount = GS.R.ReadInt32();
        G.HS.FailedMineBonusChance = GS.R.ReadSingle();
        G.HS.NextMineBonusChanceCount = GS.R.ReadSingle();
        G.HS.NextMineBonusChanceAmount = GS.R.ReadSingle();
        G.HS.FreePushableObjects = GS.R.ReadSingle();
        G.HS.DefaultMiningChanceItemID = GS.R.ReadInt32();
        G.HS.DefaultMiningChance = GS.R.ReadSingle();
        G.HS.BehindMineBonusItemID = GS.R.ReadInt32();
        G.HS.BehindMineBonusAmount = GS.R.ReadInt32();
        G.HS.BehindMineBonusTimes = GS.R.ReadInt32();
        G.HS.BehindMineBonusChanceAmount = GS.R.ReadSingle();
        G.HS.BehindMineBonusChanceTimes = GS.R.ReadInt32();
        G.HS.BehindMineBonusCompoundChanceAmount = GS.R.ReadSingle();
        G.HS.BehindMineBonusCompoundChanceTimes = GS.R.ReadInt32();
        G.HS.FailedMineMoveRandomly = GS.R.ReadInt32();
        G.HS.HeroJumpOverMinesItemID = GS.R.ReadInt32();
        G.HS.HeroJumpOverMinesCost = GS.R.ReadInt32();
        G.HS.BagExtraBonusItemID = GS.R.ReadInt32();
        G.HS.BagExtraBonusAmount = GS.R.ReadSingle();
        G.HS.BagExtraInventoryItemID = GS.R.ReadInt32();
        G.HS.BagExtraInventoryAmount = GS.R.ReadSingle();
        G.HS.DestroyedRockSpawnArrow = GS.R.ReadInt32();
        G.HS.AroundMineBonusItemID = GS.R.ReadInt32();
        G.HS.AroundMineBonusTimes = GS.R.ReadInt32();
        G.HS.AroundMineBonusAmount = GS.R.ReadSingle();
        G.HS.MuddyRockSpawnBoulder = GS.R.ReadInt32();
        G.HS.BoomerangCanMine = GS.R.ReadBoolean();
        G.HS.FireballCanMine = GS.R.ReadBoolean();
        G.HS.AlcovePush = GS.R.ReadInt32();
        G.HS.PickaxeInterest = GS.R.ReadSingle();
        G.HS.PickaxeInterestItemID = GS.R.ReadInt32();
        G.HS.PickaxeInterestItemMultiplier = GS.R.ReadSingle();
        G.HS.RandomItemsCreatedBonus = GS.R.ReadSingle();
        G.HS.MinesAroundChiselBonusChance = GS.R.ReadSingle();
        G.HS.DestroyedMinesInSequence = GS.R.ReadInt32();
        G.HS.MissSameMineInSequence = GS.R.ReadInt32();
        G.HS.HeroChangeBoulderDir = GS.R.ReadInt32();
        G.HS.BoulderPushPower = GS.R.ReadInt32();
        G.HS.LinkedCrackedBnItemID = GS.R.ReadInt32();
        G.HS.LinkedCrackedBnAmount = GS.R.ReadSingle();
        G.HS.HeroExpandMud = GS.R.ReadInt32();
        G.HS.NextVaultExtraPower = GS.R.ReadSingle();
        G.HS.VaultReuse = GS.R.ReadInt32();
        G.HS.DestroyedGiveExtraItemID = GS.R.ReadInt32();
        G.HS.DestroyedGiveExtraItemAmount = GS.R.ReadSingle();
        G.HS.CrackedGiveExtraItemID = GS.R.ReadInt32();
        G.HS.CrackedGiveExtraItemAmount = GS.R.ReadSingle();
        G.HS.DynamiteExplodeCracked = GS.R.ReadInt32();
        G.HS.MaxCostMiningFailure = GS.R.ReadSingle();
        G.HS.MaxCostMiningFailureUses = GS.R.ReadInt32();
        G.HS.MaxCostMiningSuccess  = GS.R.ReadSingle();
        G.HS.MaxCostMiningSuccessUses = GS.R.ReadInt32();
        G.HS.DestroyedCrackNeighborChance = GS.R.ReadSingle();
        G.HS.FailedCrackNeighborChance = GS.R.ReadSingle();
        G.HS.GetExtraItemID = GS.R.ReadInt32();
        G.HS.GetExtraItemAmount = GS.R.ReadSingle();
        G.HS.FailedInflationPerSide = GS.R.ReadSingle();
        G.HS.XDestroyedGiveItemID = GS.R.ReadInt32();
        G.HS.XDestroyedGiveItemAmount = GS.R.ReadSingle();
        G.HS.MaxTicTacMoves = GS.R.ReadInt32();
        G.HS.TicTacMoveCount = GS.R.ReadInt32();
        G.HS.DisplaceHeroTickNumber = GS.R.ReadInt32();
        G.HS.DisplaceHeroType = GS.R.ReadInt32();
    }
#endregion
    public void UpdateMineBonusText()
    {
        if( Unit.Body.MineType != EMineType.VAULT ) return;

        if( Util.IsNeighbor( G.Hero.Pos, Unit.Pos ) )                                          // Check for vault activation
            ActivateVault();

        CheckConditionals();                                                                   // Check Vault Conditions

        if( Map.I.TurnFrameCount == 1 )
        if( MineBonusCnType == EMineBonusCnType.GET_X_VAULT_POINTS )                           // vault points needs constant check
        if( MineBonusGiven == false )
            UpdateText = true;
        
        if( Map.I.TurnFrameCount == 3 )
        if( MineBonusCnType == EMineBonusCnType.GET_X_GLOBAL_VAULT_POINTS )                    // vault points needs constant check
        if( MineBonusGiven == false )
            UpdateText = true;

        if( OldZoom != -1 )
        if( Map.I.ZoomMode != OldZoom )
            UpdateText = true;

        bool showItem = false;
        string txt = "";
        if( UpdateText )
        switch( MineBonusCnType )
        {          
            case EMineBonusCnType.ACTIVATE_VAULT:
            txt = "Activate Vault";
            break;
            case EMineBonusCnType.ENTER_THE_CUBE:
            txt = "Enter the Cube";
            break;
            case EMineBonusCnType.MINE_X_MINE:
            int v = MineMinedCount;
            v = ( int ) MineBonusVal2 - v; 
            if( v < 0 ) v = 0;
            txt = "Destroy " + ( int ) v + " Mine" + Util.GetPlural( v );
            break;
            case EMineBonusCnType.KILL_X_MONSTERS:
            v = KilledMonsters;
            v = ( int ) MineBonusVal2 - v;
            if( v < 0 ) v = 0;
            txt = "Kill " + ( int ) v + " Monster" + Util.GetPlural( v );
            break;
            case EMineBonusCnType.MISS_X_ATTEMPTS:
            v = MissedAttempts;
            v = ( int ) MineBonusVal2 - v;
            if( v < 0 ) v = 0;
            txt = "Miss " + ( int ) v + " Minning Attempt" + Util.GetPlural( v );
            break;
            case EMineBonusCnType.DESTROY_X_MINES_IN_SEQUENCE:
            v = ( int ) MineBonusVal2;
            txt = "Destroy  " + ( int ) v + " Mines In Sequence";
            break;
            case EMineBonusCnType.MISS_SAME_MINE_IN_SEQUENCE_X_TIMES:
            v = ( int ) MineBonusVal2;
            txt = "Miss the Same Mine In Sequence: x" + ( int ) v;
            break;
            case EMineBonusCnType.COLLECT_X_RESOURCE:
            v = ( int ) ResourceCollected;
            v = ( int ) MineBonusVal2 - v; 
            if( v < 0 ) v = 0;
            string nm = G.GIT( Unit.Body.MiningPrize ).GetName();
            txt = "Collect " + ( int ) v + " " + nm;
            showItem = true;
            break;
            case EMineBonusCnType.HAVE_X_RESOURCE_INVENTORY:
            //v = ( int ) Item.GetNum( DefaultItemPrize );
            v = ( int ) MineBonusVal2; 
            if( v < 0 ) v = 0;
            nm = G.GIT( Unit.Body.MiningPrize ).GetName();
            txt = "Have " + ( int ) v + " " + nm + "(" + Item.GetNum( Unit.Body.MiningPrize ) + ")";
            break;
            case EMineBonusCnType.DESTROY_X_ROUND_MINE:
            v = RoundMineMinedCount;
            v = ( int ) MineBonusVal2 - v; 
            if( v < 0 ) v = 0;
            txt = "Destroy " + ( int ) v + " Round Mine" + Util.GetPlural( v );
            break;
            case EMineBonusCnType.DESTROY_X_SQUARE_MINE:
            v = SquareMineMinedCount;
            v = ( int ) MineBonusVal2 - v; 
            if( v < 0 ) v = 0;
            txt = "Destroy " + ( int ) v + " Square Mine" + Util.GetPlural( v );
            break;
            case EMineBonusCnType.ACTIVATE_X_VAULTS:
            v = VaultsActivated;
            v = ( int ) MineBonusVal2 - v; 
            if( v < 0 ) v = 0;
            txt = "Activate  " + ( int ) v + " Vault" + Util.GetPlural( v );
            break;
            case EMineBonusCnType.CONQUER_X_VAULTS:
            v = VaultsConquered;
            v = ( int ) MineBonusVal2 - v; 
            if( v < 0 ) v = 0;
            txt = "Conquer  " + ( int ) v + " Vault" + Util.GetPlural( v );
            break;
            case EMineBonusCnType.HAVE_X_MINE_AROUND_HERO:
            v = ( int ) MineBonusVal2;
            txt = "Have " + ( int ) v + " Mine" + Util.GetPlural( v ) + " Around Hero";
            break;
            case EMineBonusCnType.GET_X_VAULT_POINTS:
            if( InitialVaultPoints == -1 )
            {
                InitialVaultPoints = GetVaultPoints();
                G.HS.InitialGlobalVaultPoints += InitialVaultPoints;
            }
            VaultPoints = GetVaultPoints();
            if( Map.I.TurnFrameCount == 1 )
                G.HS.GlobalVaultPoints += VaultPoints;

            int dif = InitialVaultPoints - VaultPoints;
            if( Unit.Control.RestingRadiusSprite )
            Unit.Control.RestingRadiusSprite.gameObject.SetActive( true );                                            // Enables Radius sprite 
            int rad = 1 + ( int ) ( ( Map.I.RM.RMD.BaseRestingDistance + Unit.Control.BaseRestDistance ) * 2 );
            if( Map.I.TurnFrameCount == 1 )
            Unit.Control.RestingRadiusSprite.gameObject.transform.localScale = new Vector3( rad, rad, 1 );
            v = ( int ) MineBonusVal2 - dif;
            if( v < 0 ) v = 0;
            txt = "Get " + ( int ) v + " Vault Points";
            break;
            case EMineBonusCnType.GET_X_GLOBAL_VAULT_POINTS:
             dif = G.HS.InitialGlobalVaultPoints - G.HS.GlobalVaultPoints;
             v = ( int ) MineBonusVal2 - dif;
            if( v < 0 ) v = 0;
            txt = "Get " + ( int ) v + " Vault Points Across the Cube";
            break;
        }
        if( UpdateText )
        switch( MineBonusEffType )
        {
            case EMineBonusEffType.GIVE_ITEM:
            string nm = G.GIT( Unit.Body.MiningPrize ).GetName();
            txt += "\nto Get " + MineBonusVal1.ToString( "+0;-#" ) + " " + nm;
            showItem = true;
            break;
            case EMineBonusEffType.RESET_RESOURCE:
            nm = G.GIT( Unit.Body.MiningPrize ).GetName();
            txt += "\nto Dump all " + nm + " in the Inventory";
            showItem = true;
            break;
            case EMineBonusEffType.ADD_INTEREST:
            txt += "\nPickaxe Interest " + MineBonusVal1.ToString( "+0;-#" ) + "%";
            showItem = true;
            break;
            case EMineBonusEffType.ITEM_PICKUP_ADD_INTEREST:
            txt += "\nItem Increases Pickaxe Interest by " + MineBonusVal1.ToString( "+0;-#" ) + "%";
            showItem = true;
            break;
            case EMineBonusEffType.FAILED_MINE_BONUS_CHANCE:
            txt += "\nFailed Mine Extra Chance: " + MineBonusVal1.ToString( "+0;-#" ) + "%";
            break;
            case EMineBonusEffType.NEIGHBOR_MINES_GET_BONUS_CHANCE:
            txt += "\nNext " + MineBonusVal1 + " Neighbor Mines get " + MineBonusVal3 + "% Bonus Chance";
            break;
            case EMineBonusEffType.MINES_AROUND_CHISEL_BONUS_CHANCE:
            txt += "\nNear Chisel Extra Chance: " + MineBonusVal1.ToString( "+0;-#" ) + "%";
            break;
            case EMineBonusEffType.FREE_PUSH_X_OBJECTS:
            txt += "\nNext " + MineBonusVal1 + " Objects can be Pushed";
            break;
            case EMineBonusEffType.ITEM_SET_DEFAULT_MINING_CHANCE:
            nm = G.GIT( Unit.Body.MiningPrize ).GetName();
            float amt = Item.GetNum( Unit.Body.MiningPrize );
            txt += "\nDefault Mining Chance Defined by " + nm + " Amount: " + amt + "%";
            showItem = true;
            break;
            case EMineBonusEffType.MINE_BEHIND_GET_X_RESOURCE:
            nm = G.GIT( Unit.Body.MiningPrize ).GetName();
            txt += "\nMines Behind Target Give " + MineBonusVal1.ToString( "+0;-#" ) + " Bonus " + nm + " Each (x" + MineBonusVal3 + ")";
            showItem = true;
            break;
            case EMineBonusEffType.MINE_BEHIND_ADD_X_CHANCE:
            txt += "\nMines Behind Target Get " + MineBonusVal1.ToString( "+0;-#" ) + "% Bonus Chance (x" + MineBonusVal3 + ")";
            break;
            case EMineBonusEffType.MINE_BEHIND_ADD_COMPOUND_X_CHANCE:
            txt += "\nMines Behind Target Get Increased" + MineBonusVal1.ToString( "+0;-#" ) + "% Bonus Chance (x" + MineBonusVal3 + ")";
            break;
            case EMineBonusEffType.FAILED_MOVE_TO_RAND_POS:
            txt += "\nFailed Mine Move to a random Position: x" + MineBonusVal1;
            break;
            case EMineBonusEffType.JUMP_OVER_MINES_FOR_RESOURCE:
            txt += "\nJump over Mines, Cost: x" + MineBonusVal1 + " per Rock";
            showItem = true;
            break;
            case EMineBonusEffType.BAG_GIVES_EXTRA_RESOURCE:
            nm = G.GIT( Unit.Body.MiningPrize ).GetName();
            txt += "\nEach Collected Bag gives " + MineBonusVal1.ToString( "+0;-#" ) + " Extra " + nm;
            showItem = true;
            break;
            case EMineBonusEffType.BAG_GIVE_INVENTORY_BONUS:
            nm = G.GIT( Unit.Body.MiningPrize ).GetName();
            txt += "\nWhen you Pick a Bag, Inventory " + nm + " Increases by " + MineBonusVal1.ToString( "+0;-#" ) + "%";
            showItem = true;
            break;
            case EMineBonusEffType.DESTROY_THIS_VAULT:
            txt += "\nDestroy this Vault";
            break;
            case EMineBonusEffType.DESTROYED_MINE_SPAWN_ARROW:
            txt += "\nRocks Spawn Arrows x" + MineBonusVal1;
            break;
            case EMineBonusEffType.MUDDY_ROCK_SPAWN_BOULDER:
            txt += "\nMuddy Rocks Spawn Boulder x" + MineBonusVal1;
            break;
            case EMineBonusEffType.HERO_EXPAND_MUD:
            txt += "\nHero can Expand Mud by Rotating: x" + ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.VAULT_POWER:
            txt += "\nAll Vaults Effect Power: " + MineBonusVal1.ToString( "+0;-#" ) + "%";
            break;
            case EMineBonusEffType.NEXT_VAULT_POWER:
            txt += "\nNext Vault Effect Power: " + MineBonusVal1.ToString( "+0;-#" ) + "%";
            break;
            case EMineBonusEffType.FAILED_INFLATION_PER_SIDE:
            txt += "\nFailed Mine Inflation Per Side: " + MineBonusVal1.ToString( "+0;-#" );
            break;
            case EMineBonusEffType.VAULT_REUSE:
            txt += "\nHero can Reuse a Vault: x" + ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.MAX_TIC_TAC_MOVES:
            txt += "\nMax TIC TAC Moves: " + ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.DISPLACE_HERO_ON_TICK_X:
            txt += "\nDisplace Hero on Tick #" + ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.MAX_FAILED_COST:
            txt += "\nMax Cost for Mining Failure: " + ( int ) MineBonusVal1 + " (x" + MineBonusVal3 + ")";
            break;
            case EMineBonusEffType.MAX_SUCCESS_COST:
            txt += "\nMax Cost for Mining Success: " + ( int ) MineBonusVal1 + " (x" + MineBonusVal3 + ")";
            break;
            case EMineBonusEffType.void1:
            txt += "\n********************************************************************************************" + ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.MINES_AROUND_DESTROYED_GIVE_RESOURCE:
            nm = G.GIT( Unit.Body.MiningPrize ).GetName();
            txt += "\nMines Around Destroyed give" + MineBonusVal1.ToString( "+0;-#" ) + " Bonus " + nm + " (x" + MineBonusVal3 + ")";
            showItem = true;
            break;
            case EMineBonusEffType.GIVE_INVENTORY_BONUS_PERCENT:
            nm = G.GIT( Unit.Body.MiningPrize ).GetName();
            txt += "\nInventory " + nm + " Increases by " + MineBonusVal1.ToString( "+0;-#" ) + "%";
            showItem = true;
            break;
            case EMineBonusEffType.SET_DEFAULT_MINING_CHANCE:
            txt += "\nDefault Mining Chance: " + MineBonusVal1 + "%";
            break;
            case EMineBonusEffType.STRIKE_CLOSEST_ORB:
            txt += "\nStrike Nearest Orb";
            break;
            case EMineBonusEffType.HERO_CHANGE_BOULDER_DIR:
            txt += "\nHero Can Change Boulder Dir: " + MineBonusVal1.ToString( "+0;-#" );
            break;
            case EMineBonusEffType.ADD_BOULDER_PUSH_POWER:
            if( MineBonusVal1 >= 49 )
                txt += "\nBoulder Push Power: MAX!";
            else
                txt += "\nBoulder Push Power: " + MineBonusVal1.ToString( "+0;-#" );
            break;
            case EMineBonusEffType.BOOMERANG_CAN_MINE:
            txt += "\nBoomerang Can Destroy Rocks";
            break;
            case EMineBonusEffType.FIREBALL_CAN_MINE:
            txt += "\nFireball Can Destroy Rocks";
            break;
            case EMineBonusEffType.ENABLE_ALCOVE_PUSH:
            txt += "\nEnable Alcove Push x" + MineBonusVal1;
            break;
            case EMineBonusEffType.RANDOM_ITENS_CREATED_BONUS:
            txt += "\nRandom Items Created Amount: " + MineBonusVal1.ToString( "+0;-#" ) + "%";
            break;
            case EMineBonusEffType.LINKED_CRACKED_MINE_GET_ITEM:
            txt += "\nOn Destroy, Linked Cracked Mines Get: " + MineBonusVal1.ToString( "+0;-#" );
            showItem = true;
            break;
            case EMineBonusEffType.DESTROYED_GIVE_EXTRA_ITEM:
            txt += "\nDestroyed Mines Give Extra: " + MineBonusVal1.ToString( "+0.#" ) + "%";
            showItem = true;
            break;
            case EMineBonusEffType.X_DESTROYED_GIVE_ONE_ITEM:
            txt += "\nEvery " + MineBonusVal1 + " Destroyed Mines Gives one";
            showItem = true;
            break;
            case EMineBonusEffType.DESTROYED_CRACK_NEIGHBOR_CHANCE:
            txt += "\nDestroyed Mine Crack Neighbor Chance: " + MineBonusVal1.ToString( "+0.#" ) + "%";
            break;
            case EMineBonusEffType.FAILED_CRACK_NEIGHBOR_CHANCE:
            txt += "\nFailed Mine Crack Neighbor Chance: " + MineBonusVal1.ToString( "+0.#" ) + "%";
            break;
            case EMineBonusEffType.CRACKED_GIVE_EXTRA_ITEM:
            txt += "\nCracked Mines Give Extra: " + MineBonusVal1.ToString( "+0.#" ) + "%";
            showItem = true;
            break;
            case EMineBonusEffType.GET_EXTRA_ITEM:
            txt += "\nGet Extra Item: " + MineBonusVal1.ToString( "+0.#" ) + "%";
            showItem = true;
            break;
            case EMineBonusEffType.DYNAMITE_EXPLODE_CRACKED:
            txt += "\nDynamite Explode Cracked: x" + ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.SPREAD_X_ITEMS:
            nm = G.GIT( Unit.Body.MiningPrize ).GetName();
            txt += "\nSpread " + ( int ) MineBonusVal1 + " "  + nm;
            if( MineBonusVal3 > 1 ) txt += " (+" + MineBonusVal3 + ")";
            showItem = true;
            break;
            case EMineBonusEffType.SPREAD_X_MINING_PRIZE:
            nm = G.GIT( Unit.Body.MiningPrize ).GetName();
            txt += "\nSpread " + ( int ) MineBonusVal1 + " "  + nm + " Over Mines";
            if( MineBonusVal3 > 1 ) txt += " (+" + MineBonusVal3 + ")";
            showItem = true;
            break;
            case EMineBonusEffType.SPREAD_X_HAMMER:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Hammers";
            break;
            case EMineBonusEffType.SPREAD_X_ROPE:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Mine Rope";
            break;
            case EMineBonusEffType.SPREAD_X_HOLE:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Mine Hole";
            break;       
            case EMineBonusEffType.SPREAD_X_SWAPPER:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Mine Swapper";
            break;
            case EMineBonusEffType.SPREAD_X_CHISEL:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Mine Chisel";
            break;
            case EMineBonusEffType.SPREAD_X_DYNAMITE:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Dynamite";
            break;
            case EMineBonusEffType.SPREAD_MUD_AROUND_MINE:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Mud Around Rocks";
            break;
            case EMineBonusEffType.SPREAD_X_ARROW_MINE:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Mine Arrow";
            break;
            case EMineBonusEffType.SPREAD_X_WHEEL:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Mine Wheel";
            break;
            case EMineBonusEffType.SPREAD_X_MAGNET:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Mine Magnet";
            break;
            case EMineBonusEffType.SPREAD_X_CANNON:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Mine Cannon";
            break;
            case EMineBonusEffType.SPREAD_X_COG:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Mine Cog";
            break;
            case EMineBonusEffType.SPREAD_X_JUMPER:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Jumper";
            break;
            case EMineBonusEffType.SPREAD_X_GLOVE:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Glove";
            break;
            case EMineBonusEffType.SPREAD_WEDGE_AROUND_MINE:
            txt += "\nSpread " + ( int ) MineBonusVal1 + " Wedge";
            break;
        }

        if( UpdateText ) 
        {
            float sc = 2;
            if( Map.I.ZoomMode == 1 ) sc = 2.8f;
            if( Map.I.ZoomMode == 2 ) sc = 3.3f;
            if( Map.I.ZoomMode == 3 ) sc = 3.5f;

            Unit.RightText.transform.localScale = new Vector3( sc, sc, 1 );
            Unit.Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );
            Unit.Spr.spriteId = 92;
            Unit.Spr.scale = new Vector3( 1.4f, 1.4f );
            Unit.Spr.color = Color.white;
            Unit.RightText.gameObject.SetActive( false );
            Unit.Spr.transform.position = new Vector3( Unit.Pos.x, Unit.Pos.y, -2.1f ); // new
            if( txt != "" )
            {
                Unit.RightText.gameObject.SetActive( true );                                                          // Shows bonus text
                Unit.RightText.text = txt;
            }
            Unit.RightText.color = Color.white;                                                                       // Bonus text color

            if( MineBonusActive == false )
                Unit.RightText.color = new Color( 0, 1, 0, 1 );
            if( MineBonusFailed )
                Unit.RightText.color = new Color( 1, 0, 0, 1 );
            if( Activated == false )
                Unit.RightText.color = new Color32( 128, 125, 255, 255 );

            if( showItem && Unit.Body.MiningPrize != ItemType.NONE )
            {
                Unit.Body.Sprite4.transform.eulerAngles = new Vector3( 0, 0, 0 );
                Unit.Body.Sprite4.gameObject.SetActive( true );                                                      // Show default prize sprite
                Unit.Body.Sprite4.spriteId = Manager.I.Inventory.
                ItemList[ ( int ) Unit.Body.MiningPrize ].TKSprite.spriteId;
            }
            else
                Unit.Body.Sprite4.gameObject.SetActive( false );

            Unit.RightText.transform.localPosition = new Vector3( 0, 0.1f, -3f );
            UpdateText = false;
        }
    }
    private void CheckConditionals()
    {
        if( Map.Stepping() == false )
        if( Map.I.TurnFrameCount != 2 ) return;
        CheckCn( EMineBonusCnType.KILL_X_MONSTERS );                                                      // Check Condition: Kill X Monsters
        CheckCn( EMineBonusCnType.ENTER_THE_CUBE );                                                       // Check Condition: Enter the cube
        CheckCn( EMineBonusCnType.ACTIVATE_VAULT );                                                       // Check Condition: Activate Vault
        CheckCn( EMineBonusCnType.MINE_X_MINE );                                                          // Check Condition: Destroy X mine
        CheckCn( EMineBonusCnType.DESTROY_X_ROUND_MINE );                                                 // Check Condition: Destroy X Round mine
        CheckCn( EMineBonusCnType.DESTROY_X_SQUARE_MINE );                                                // Check Condition: Destroy X Square mine
        CheckCn( EMineBonusCnType.COLLECT_X_RESOURCE );                                                   // Check Condition: Collect X resource
        CheckCn( EMineBonusCnType.HAVE_X_RESOURCE_INVENTORY );                                            // Check Condition: Have X resource inventory
        CheckCn( EMineBonusCnType.HAVE_X_MINE_AROUND_HERO );                                              // Check Condition: Have X mine around hero
        CheckCn( EMineBonusCnType.MISS_X_ATTEMPTS );                                                      // Check Condition: Miss X mine attempts
        CheckCn( EMineBonusCnType.GET_X_VAULT_POINTS );                                                   // Check Condition: Get X Vault points   
        CheckCn( EMineBonusCnType.DESTROY_X_MINES_IN_SEQUENCE );                                          // Check Condition: Destroy X mines in sequence 
        CheckCn( EMineBonusCnType.MISS_SAME_MINE_IN_SEQUENCE_X_TIMES );                                   // Check Condition: Fail same mine in sequence
        CheckCn( EMineBonusCnType.ACTIVATE_X_VAULTS );                                                    // Check Condition: activate x vaults
        CheckCn( EMineBonusCnType.CONQUER_X_VAULTS );                                                     // Check Condition: conquer x vaults

        if( Map.I.TurnFrameCount > 1 )                                                                    // Important vault points are calculated in the frame 0 and global in the frame 2 so only check after frame 1
            CheckCn( EMineBonusCnType.GET_X_GLOBAL_VAULT_POINTS );                                        // Check Condition: Get X Global Vault points    
    }
    public void ActivateVault()
    {
        if( Activated == false )                                                                          // Activates mine by stepping around
        {
            MasterAudio.PlaySound3DAtVector3( "Save Game 2", Unit.Pos );                                  // Sound FX
            Controller.CreateMagicEffect( Unit.Pos );                                                     // Magic FX  
            Activated = true;                                                                             //
            UpdateText = true;
            if( G.HS.NextVaultExtraPower != 0 )
            {
                MineBonusVal1 += Util.Percent( G.HS.NextVaultExtraPower, MineBonusVal1 );
                Message.GreenMessage( "Vault Extra Power: +" + G.HS.NextVaultExtraPower + "%" );          // next vault extra power bonus
                G.HS.NextVaultExtraPower = 0;
            }
            Mine.UpdateVaultCounter( EMineBonusCnType.ACTIVATE_X_VAULTS, Unit );                          // Increment value
        }
    }
    public int GetVaultPoints()
    {
        int points = 0;
        int radius = ( int ) ( ( Map.I.RM.RMD.BaseRestingDistance + Unit.Control.BaseRestDistance ));
        for( int y = ( int ) Unit.Pos.y - radius; y <= Unit.Pos.y + radius; y++ )
        for( int x = ( int ) Unit.Pos.x - radius; x <= Unit.Pos.x + radius; x++ )
        {
            if( G.Hero.Pos == new Vector2( x, y ) ) ActivateVault();                                             // activates vault by radius step
            Unit tgmine = Map.GFU( ETileType.MINE, new Vector2( x, y ) );
            if( tgmine != null && Controller.TypeCanBeMined( tgmine ) )
            {
                int pt = Util.Manhattan( Unit.Pos, tgmine.Pos );
                points += ( radius + 1 ) - pt;                                                                   // vault points based on vault distance incrementally
            }
        }
        return points;
    }
    public bool CheckCn( EMineBonusCnType type )
    {
        if( type != EMineBonusCnType.ENTER_THE_CUBE )       // this condition dont need to be activated
        if( Activated == false ) return false;
        if( MineBonusCnType != type ) return false;
        if( MineBonusGiven ) return false;
        bool res = false;
        bool fail = false;
        switch( type )
        {
            case EMineBonusCnType.ACTIVATE_VAULT:
            if( Activated )
                res = true;
            break;
            case EMineBonusCnType.ENTER_THE_CUBE:
            Activated = true;
            res = true;
            break;
            case EMineBonusCnType.MINE_X_MINE:
            if( MineMinedCount >= MineBonusVal2 )
                res = true;
            break;
            case EMineBonusCnType.MISS_X_ATTEMPTS:
            if( MissedAttempts >= MineBonusVal2 )
                res = true;
            break;
            case EMineBonusCnType.DESTROY_X_MINES_IN_SEQUENCE:
            if( G.HS.DestroyedMinesInSequence >= MineBonusVal2 )
                res = true;
            break;
            case EMineBonusCnType.MISS_SAME_MINE_IN_SEQUENCE_X_TIMES:
            if( G.HS.MissSameMineInSequence >= MineBonusVal2 )
                res = true;
            break;
            case EMineBonusCnType.KILL_X_MONSTERS:
            if( KilledMonsters >= MineBonusVal2 )
                res = true;
            break;
            case EMineBonusCnType.COLLECT_X_RESOURCE:
            if( ResourceCollected >= MineBonusVal2 )
                res = true;
            break;
            case EMineBonusCnType.HAVE_X_RESOURCE_INVENTORY:
            if( Item.GetNum( Unit.Body.MiningPrize ) >= MineBonusVal2 )
                res = true;
            break;     
            case EMineBonusCnType.DESTROY_X_ROUND_MINE:
            if( RoundMineMinedCount >= MineBonusVal2 )
                res = true;
            break;
            case EMineBonusCnType.DESTROY_X_SQUARE_MINE:
            if( SquareMineMinedCount >= MineBonusVal2 )
                res = true;
            break;
            case EMineBonusCnType.HAVE_X_MINE_AROUND_HERO:
            int count = 0;
            for( int dr = 0; dr < 8; dr++ )
            {
                Vector2 tg = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) dr ];
                Unit mine = Map.GFU( ETileType.MINE, tg );
                if( mine && Controller.CanBeMined( tg ) ) count++;
            }
            if( count >= MineBonusVal2 )
                res = true;
            break;
            case EMineBonusCnType.GET_X_VAULT_POINTS:         
            int dif = InitialVaultPoints - VaultPoints;
            if( dif >= MineBonusVal2 )
                res = true;
            break;
            case EMineBonusCnType.GET_X_GLOBAL_VAULT_POINTS:
            dif = G.HS.InitialGlobalVaultPoints - G.HS.GlobalVaultPoints;
            if( dif >= MineBonusVal2 )
                res = true;
            break;
            case EMineBonusCnType.ACTIVATE_X_VAULTS:
            if( VaultsActivated >= MineBonusVal2 )
                res = true;
            break;
            case EMineBonusCnType.CONQUER_X_VAULTS:
            if( VaultsConquered >= MineBonusVal2 )
                res = true;
            break;
        }
        if( res )
        {
            MineBonusGiven = true;                                                            // Condition Met: Mark bonus given bool as true
            string txt = this.Unit.RightText.text;

            if( Util.Manhattan( G.Hero.Pos, Unit.Pos ) >= 6 )
                Message.CreateMessage( ETileType.NONE, Unit.Body.MiningPrize, txt,
                G.Hero.Pos + new Vector2( +2, +2 ), Color.green, false, false, 10, 0, -1, 50 );
        }

        if( res || fail || MineBonusGiven )
            MineBonusActive = false;                                                          // Is this bonus available?

        if( res ) ApplyMineBonusEffect();                                                     // Apply Bonus effect

        if( fail )
        {
            //SetBonusFailed();                                                               // Bonus failed
        }
     
        return res;
    }
    public void ApplyMineBonusEffect()
    {
        VaultEffTargetList = new List<Vector2>();
        Sector hs = Map.I.RM.HeroSector;
        bool conquer = true;
        if( MineBonusCnType == EMineBonusCnType.ACTIVATE_VAULT ||
            MineBonusCnType == EMineBonusCnType.ENTER_THE_CUBE ) conquer = false;             // These are not conquered
        if( conquer )
            Mine.UpdateVaultCounter( EMineBonusCnType.CONQUER_X_VAULTS, Unit );               // Increment value

        switch( MineBonusEffType )
        {
            case EMineBonusEffType.GIVE_ITEM:
            Item.AddItem( Unit.Body.MiningPrize, MineBonusVal1 );
            break;
            case EMineBonusEffType.RESET_RESOURCE:
            Item.SetAmt( Unit.Body.MiningPrize, 0 );
            break;
            case EMineBonusEffType.ADD_INTEREST:
            G.HS.PickaxeInterest += MineBonusVal1;
            break;
            case EMineBonusEffType.ITEM_PICKUP_ADD_INTEREST:
            G.HS.PickaxeInterestItemID = ( int ) Unit.Body.MiningPrize;
            G.HS.PickaxeInterestItemMultiplier += ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.FAILED_MINE_BONUS_CHANCE:
            G.HS.FailedMineBonusChance += MineBonusVal1;
            break;
            case EMineBonusEffType.MINES_AROUND_CHISEL_BONUS_CHANCE:
            G.HS.MinesAroundChiselBonusChance += MineBonusVal1;
            UpdateChiselText = true;
            break;
            case EMineBonusEffType.HERO_CHANGE_BOULDER_DIR:
            G.HS.HeroChangeBoulderDir += ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.ADD_BOULDER_PUSH_POWER:
            G.HS.BoulderPushPower += ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.NEIGHBOR_MINES_GET_BONUS_CHANCE:
            G.HS.NextMineBonusChanceCount += MineBonusVal1;
            G.HS.NextMineBonusChanceAmount = MineBonusVal3;
            break;
            case EMineBonusEffType.FREE_PUSH_X_OBJECTS:
            G.HS.FreePushableObjects += MineBonusVal1;
            break;
            case EMineBonusEffType.ITEM_SET_DEFAULT_MINING_CHANCE:
            G.HS.DefaultMiningChanceItemID = ( int ) Unit.Body.MiningPrize;
            break;
            case EMineBonusEffType.MINE_BEHIND_GET_X_RESOURCE:
            G.HS.BehindMineBonusItemID = ( int ) Unit.Body.MiningPrize;
            G.HS.BehindMineBonusTimes += ( int ) MineBonusVal3;
            G.HS.BehindMineBonusAmount = ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.MINE_BEHIND_ADD_X_CHANCE:
            G.HS.BehindMineBonusChanceAmount += ( int ) MineBonusVal1;
            G.HS.BehindMineBonusChanceTimes += ( int ) MineBonusVal3;
            break;
            case EMineBonusEffType.MINE_BEHIND_ADD_COMPOUND_X_CHANCE:
            G.HS.BehindMineBonusCompoundChanceAmount += ( int ) MineBonusVal1;
            G.HS.BehindMineBonusCompoundChanceTimes += ( int ) MineBonusVal3;
            break;
            case EMineBonusEffType.FAILED_MOVE_TO_RAND_POS:
            G.HS.FailedMineMoveRandomly += ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.JUMP_OVER_MINES_FOR_RESOURCE:
            G.HS.HeroJumpOverMinesCost = ( int ) MineBonusVal1;
            G.HS.HeroJumpOverMinesItemID = ( int ) Unit.Body.MiningPrize;
            break;
            case EMineBonusEffType.BAG_GIVES_EXTRA_RESOURCE:
            G.HS.BagExtraBonusItemID = ( int ) Unit.Body.MiningPrize;
            G.HS.BagExtraBonusAmount += ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.BAG_GIVE_INVENTORY_BONUS:
            G.HS.BagExtraInventoryItemID = ( int ) Unit.Body.MiningPrize;
            G.HS.BagExtraInventoryAmount += ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.DESTROY_THIS_VAULT:
            Map.Kill( Unit );
            Map.I.CreateExplosionFX( Unit.Pos );                                               // Explosion FX
            break;
            case EMineBonusEffType.DESTROYED_MINE_SPAWN_ARROW:
            G.HS.DestroyedRockSpawnArrow += ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.MUDDY_ROCK_SPAWN_BOULDER:
            G.HS.MuddyRockSpawnBoulder += ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.HERO_EXPAND_MUD:
            G.HS.HeroExpandMud += ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.VAULT_POWER:
            for( int yy = ( int ) G.HS.Area.yMin - 1; yy < G.HS.Area.yMax + 1; yy++ )
            for( int xx = ( int ) G.HS.Area.xMin - 1; xx < G.HS.Area.xMax + 1; xx++ )
            {
                Unit mine = Map.GFU( ETileType.MINE, new Vector2( xx, yy ) );
                if( mine )
                if( mine.Body.MineType == EMineType.VAULT )
                if( mine.Mine.MineBonusGiven == false )
                {
                    mine.Mine.MineBonusVal1 += Util.Percent( MineBonusVal1, mine.Mine.MineBonusVal1 );
                    mine.Mine.AnimateIconTimer = .0001f;
                }
            }
            break;
            case EMineBonusEffType.NEXT_VAULT_POWER:
            G.HS.NextVaultExtraPower += ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.FAILED_INFLATION_PER_SIDE:
            G.HS.FailedInflationPerSide += ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.VAULT_REUSE:
            G.HS.VaultReuse += ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.MAX_TIC_TAC_MOVES:
            G.HS.MaxTicTacMoves = ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.DISPLACE_HERO_ON_TICK_X:
            G.HS.DisplaceHeroTickNumber = ( int ) MineBonusVal1;
            G.HS.DisplaceHeroType = ( int ) MineBonusVal3;
            break;
            case EMineBonusEffType.MAX_FAILED_COST:
            G.HS.MaxCostMiningFailure = ( int ) MineBonusVal1;
            G.HS.MaxCostMiningFailureUses += ( int ) MineBonusVal3;
            break;
            case EMineBonusEffType.MAX_SUCCESS_COST:
            G.HS.MaxCostMiningSuccess = ( int ) MineBonusVal1;
            G.HS.MaxCostMiningSuccessUses += ( int ) MineBonusVal3;
            break;
            case EMineBonusEffType.MINES_AROUND_DESTROYED_GIVE_RESOURCE:
            G.HS.AroundMineBonusItemID = ( int ) Unit.Body.MiningPrize;
            G.HS.AroundMineBonusTimes += ( int ) MineBonusVal3;
            G.HS.AroundMineBonusAmount = ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.LINKED_CRACKED_MINE_GET_ITEM:
            G.HS.LinkedCrackedBnItemID = ( int ) Unit.Body.MiningPrize;
            G.HS.LinkedCrackedBnAmount += MineBonusVal1;
            break;
            case EMineBonusEffType.DESTROYED_GIVE_EXTRA_ITEM:
            G.HS.DestroyedGiveExtraItemID = ( int ) Unit.Body.MiningPrize;
            G.HS.DestroyedGiveExtraItemAmount += MineBonusVal1;
            break;
            case EMineBonusEffType.X_DESTROYED_GIVE_ONE_ITEM:
            G.HS.XDestroyedGiveItemID = ( int ) Unit.Body.MiningPrize;
            G.HS.XDestroyedGiveItemAmount = MineBonusVal1;
            break;
            case EMineBonusEffType.DESTROYED_CRACK_NEIGHBOR_CHANCE:
            G.HS.DestroyedCrackNeighborChance += MineBonusVal1;
            break;
            case EMineBonusEffType.FAILED_CRACK_NEIGHBOR_CHANCE:
            G.HS.FailedCrackNeighborChance += MineBonusVal1;
            break;
            case EMineBonusEffType.CRACKED_GIVE_EXTRA_ITEM:
            G.HS.CrackedGiveExtraItemID = ( int ) Unit.Body.MiningPrize;
            G.HS.CrackedGiveExtraItemAmount += MineBonusVal1;
            break;
            case EMineBonusEffType.GET_EXTRA_ITEM:
            G.HS.GetExtraItemID = ( int ) Unit.Body.MiningPrize;
            G.HS.GetExtraItemAmount += MineBonusVal1;
            break;
            case EMineBonusEffType.DYNAMITE_EXPLODE_CRACKED:
            G.HS.DynamiteExplodeCracked = ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.GIVE_INVENTORY_BONUS_PERCENT:
            float num = Item.GetNum( ( ItemType ) ( int ) Unit.Body.MiningPrize );
            float old = num;
            num = Util.Percent( MineBonusVal1, num );
            Item.AddItem( Unit.Body.MiningPrize, num );
            float cur = Item.GetNum( ( ItemType ) ( int ) Unit.Body.MiningPrize );
            UI.I.SetTurnInfoText( "Inventory:" + old.ToString( "0.#" ) + " + ( " + MineBonusVal1 + "% = " +                             
            num.ToString( "0.#" ) + " ) = " + cur.ToString( "0.#" ) , 15, Color.green );                // msg
            break;
            case EMineBonusEffType.SET_DEFAULT_MINING_CHANCE:
            G.HS.DefaultMiningChance = MineBonusVal1;
            break;
            case EMineBonusEffType.BOOMERANG_CAN_MINE:
            G.HS.BoomerangCanMine = !G.HS.BoomerangCanMine;
            break;
            case EMineBonusEffType.FIREBALL_CAN_MINE:
            G.HS.FireballCanMine = !G.HS.FireballCanMine;
            break;
            case EMineBonusEffType.STRIKE_CLOSEST_ORB:
            Unit closest = null;
            float closestdist = 999;
            for( int yy = ( int ) G.HS.Area.yMin - 1; yy < G.HS.Area.yMax + 1; yy++ )
            for( int xx = ( int ) G.HS.Area.xMin - 1; xx < G.HS.Area.xMax + 1; xx++ )
            if( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
            {
                Unit orb = Map.I.GetUnit( ETileType.ORB, new Vector2( xx, yy ) );
                if( orb )
                {
                    float dist = Vector2.Distance( Unit.Pos, orb.Pos );
                    if( dist < closestdist )
                    {
                        closestdist = dist;
                        closest = orb;
                    }
                }
            }
            if( closest )
            {
                closest.StrikeTheOrb( G.Hero );
                Map.I.CreateLightiningEffect( Unit, closest, "keys" );
            }
            break;
            case EMineBonusEffType.ENABLE_ALCOVE_PUSH:
            G.HS.AlcovePush += ( int ) MineBonusVal1;
            break;
            case EMineBonusEffType.RANDOM_ITENS_CREATED_BONUS:
            G.HS.RandomItemsCreatedBonus += MineBonusVal1;
            break;
            case EMineBonusEffType.SPREAD_X_ITEMS:
            CreateRandomBonuses( "Item", ( int ) MineBonusVal1, MineBonusVal3 );
            break;
            case EMineBonusEffType.SPREAD_X_MINING_PRIZE:
            CreateRandomBonuses( "Prize", ( int ) MineBonusVal1, MineBonusVal3 );
            break;
            case EMineBonusEffType.SPREAD_X_HAMMER:
            CreateRandomBonuses( "Hammer", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_X_ROPE:
            CreateRandomBonuses( "Rope", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_X_HOLE:
            CreateRandomBonuses( "Hole", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_X_SWAPPER:
            CreateRandomBonuses( "Swapper", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_X_CHISEL:
            CreateRandomBonuses( "Chisel", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_X_DYNAMITE:
            CreateRandomBonuses( "Dynamite", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_MUD_AROUND_MINE:
            CreateRandomBonuses( "Mud Around", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_X_ARROW_MINE:
            CreateRandomBonuses( "Arrow Mine", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_X_WHEEL:
            CreateRandomBonuses( "Wheel", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_X_MAGNET:
            CreateRandomBonuses( "Magnet", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_X_CANNON:
            CreateRandomBonuses( "Cannon", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_X_COG:
            CreateRandomBonuses( "Cog", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_X_JUMPER:
            CreateRandomBonuses( "Jumper", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_X_GLOVE:
            CreateRandomBonuses( "Glove", ( int ) MineBonusVal1 );
            break;
            case EMineBonusEffType.SPREAD_WEDGE_AROUND_MINE:
            CreateRandomBonuses( "Wedge", ( int ) MineBonusVal1 );
            break;
        }
        UpdateText = true;
        UpdateMineBonusText();
        Unit.RightText.color = Color.green;                                                    // Turn text green
        MasterAudio.PlaySound3DAtVector3( "Click 2", G.Hero.Pos );                             // Sound FX
        MasterAudio.PlaySound3DAtVector3( "Save Game 2", Unit.Pos );                           // Sound FX
        Controller.CreateMagicEffect( Unit.Pos );                                              // Magic FX  
    }
    public bool CreateRandomBonuses( string type, float amt, float stackamt = 1 )
    {
        List<Vector2> pl = new List<Vector2>();
        List<Unit> ml = new List<Unit>();
        List<Vector2> mudl = new List<Vector2>();
        bool res = false;
        for( int y = ( int ) G.HS.Area.yMin - 1; y < G.HS.Area.yMax + 1; y++ )                             // make lists of good positions for bonuses
        for( int x = ( int ) G.HS.Area.xMin - 1; x < G.HS.Area.xMax + 1; x++ )
        if ( Sector.IsPtInCube( new Vector2( x, y ) ) )
        {
            Unit mine = Map.GFU( ETileType.MINE, new Vector2( x, y ) );
            if( mine )
            if( Controller.TypeCanBeMined( mine ) )
            if( Mine.GetMineBonusCount( new Vector2( x, y ) ) == 0 )
                ml.Add( mine );                                                                            // adds to lists
            if( Map.EmptyTile( new Vector2( x, y ) ) )
                pl.Add( new Vector2( x, y ) );

            int count = 0;
            for( int dr = 0; dr < 8; dr++ )
            {
                Vector2 tg = new Vector2( x, y ) + Manager.I.U.DirCord[ ( int ) dr ];
                Unit tgmine = Map.GFU( ETileType.MINE, tg );
                Unit ga = Map.I.GetUnit( new Vector2( x, y ), ELayerType.GAIA );
                if( Sector.IsPtInCube( tg ) )
                if( tgmine )
                if( tgmine.Body.MineType != EMineType.VAULT )
                if( ga == null || 
                  ( ga.TileID == ETileType.NONE ) )
                if( tgmine && tgmine ) count++;    
            }
            if( count > 0 && mudl.Contains( new Vector2( x, y ) ) == false ) 
                mudl.Add( new Vector2( x, y ) );
        }

        int old = ( int ) amt;
        if( G.HS.RandomItemsCreatedBonus != 0 )
            amt += Util.Percent( G.HS.RandomItemsCreatedBonus, amt );                         // Random items bonus amount
        amt = Util.FloatSort( amt );
        UI.I.SetTurnInfoText( type + " Created: " + old + "+" +
        ( +amt - old ), 15, Color.green );               

        Vector2 pos = Vector2.zero;
        for( int i = 0; i < amt; i++ )                                                        // loop through all amount of bonuses
        {
            int ok = 0;
            int idm = Random.Range( 0, ml.Count );                                            // sort target for mines
            int idi = Random.Range( 0, pl.Count );                                            // sort target for Items
            int idmud = Random.Range( 0, mudl.Count );                                        // sort target for mud

            if( ml.Count > 0 )
                pos = ml[ idm ].Pos;

            if( ml.Count > 0 )
            {
                if( type == "Prize" )                                                         // over mine bonuses
                {
                    ok = 1;
                    ml[ idm ].Body.MiningPrize = Unit.Body.MiningPrize;
                    ml[ idm ].Body.MiningBonusAmount = stackamt;
                }
                if( type == "Hammer" )
                {
                    ok = 1;
                    ml[ idm ].Mine.HammerMine = true;
                }
                if( type == "Hole" )
                {
                    ok = 1;
                    ml[ idm ].Mine.HoleMine = true;
                }
                if( type == "Rope" )
                {
                    ok = 1;
                    ml[ idm ].Mine.RopeMine = true;
                }
                if( type == "Chisel" )
                {
                    ok = 1;
                    ml[ idm ].Mine.ChiselMine = true;
                    UpdateChiselText = true;
                }
                if( type == "Swapper" )
                {
                    ok = 1;
                    ml[ idm ].Mine.SwapperMine = true;
                }
                if( type == "Dynamite" )
                {
                    ok = 1;
                    ml[ idm ].Mine.DynamiteMine = true;
                }
                if( type == "Magnet" )
                {
                    ok = 1;
                    ml[ idm ].Mine.MagnetMine = true;
                    ml[ idm ].Mine.MineBonusDir = ( EDirection ) Util.GetRandomDir();
                    ml[ idm ].Mine.AnimateIconTimer = 1f;
                    Mine.FindRandomWheel( ml[ idm ].Pos );
                }
                if( type == "Arrow Mine" )
                {
                    ok = 1;
                    ml[ idm ].Mine.ArrowMine = true;
                    ml[ idm ].Mine.MineBonusDir = ( EDirection ) Util.GetRandomDir();
                    ml[ idm ].Mine.AnimateIconTimer = 1f;
                }
                if( type == "Wheel" )
                {
                    ok = 1;
                    ml[ idm ].Mine.WheelMine = true;
                    ml[ idm ].Mine.MineBonusDir = EDirection.NONE;
                    ml[ idm ].Mine.AnimateIconTimer = 1f;
                }
                if( type == "Cannon" )
                {
                    ok = 1;
                    ml[ idm ].Mine.CannonMine = true;
                }
                if( type == "Cog" )
                {
                    ok = 1;
                    ml[ idm ].Mine.CogMine = true;
                }
                if( type == "Jumper" )
                {
                    ok = 1;
                    ml[ idm ].Mine.JumperMine = true;
                }
                if( type == "Glove" )
                {
                    ok = 1;
                    ml[ idm ].Mine.GloveMine = true;
                }
                ml[ idm ].Mine.UpdateText = true;
            }

            if( pl.Count > 0 )
            {
                if( type == "Item" )                                                                   // spread item over map
                {
                    pos = pl[ idi ];
                    ok = 2;
                    Unit prefab = Map.I.GetUnitPrefab( ETileType.ITEM );
                    GameObject go = Map.I.CreateUnit( prefab, pl[ idi ] );
                    Unit un = go.GetComponent<Unit>();
                    un.Variation = ( int ) Unit.Body.MiningPrize;
                    un.Body.BonusItemList = null;
                    int id = G.GIT( un.Variation ).TKSprite.spriteId;
                    un.Spr.spriteId = id;
                    un.Body.StackAmount = stackamt;
                }

                if( type == "Wedge" )                                                                   // spread Wedge over map
                {
                    pos = pl[ idi ];
                    ok = 2;
                    Unit un = Map.I.SpawnFlyingUnit( pl[ idi ], ELayerType.MONSTER, ETileType.MINE, null );
                    un.Body.MineType = EMineType.WEDGE_LEFT;
                    if( Util.Chance( 50 ) ) 
                        un.Body.MineType = EMineType.WEDGE_RIGHT;
                    un.RotateTo( ( EDirection ) Util.GetRandomDir() );
                    un.Mine.AnimateIconTimer = 1;
                    un.Body.MinePushSteps = 1;
                }
            }
            if( mudl.Count > 0 )
            {
                if( type == "Mud Around" )                                                               // spread mud over map
                {
                    pos = mudl[ idmud ];
                    ok = 3;
                    Map.I.SetTile( ( int ) pos.x, ( int ) pos.y, 
                    ELayerType.GAIA, ETileType.MUD, true, true );
                    Map.AddNeighborTransToList( VI.VTOVI( pos ) );
                }
            }
            if( ok > 0 )
            {
                res = true;
                if( ok == 1 )
                    ml.RemoveAt( idm );                                                                 // removes position from list
                else
                if( ok == 2 )
                    pl.RemoveAt( idi );                                                                 // removes position from list
                else
                if( ok == 3 )
                    mudl.RemoveAt( idmud );

                Controller.CreateMagicEffect( pos );                                                    // Magic FX  
                VaultEffTargetList.Add( pos );
            }
        }
        return res;
    }
    public static void UpdateVaultCounter( EMineBonusCnType cn, Unit mined = null, ItemType itype = ItemType.NONE, float amt = -1 )
    {
        if( G.HS == null || G.HS.Type != Sector.ESectorType.NORMAL ) return;

        if( mined )
        if( cn == EMineBonusCnType.MINE_X_MINE )
        {
            if( cn == EMineBonusCnType.MINE_X_MINE )
                G.HS.MineMinedCount++;
            if( cn == EMineBonusCnType.DESTROY_X_ROUND_MINE )          
            if( mined.Body.MineType == EMineType.ROUND ) 
                G.HS.RoundMineMinedCount++;
            if( cn == EMineBonusCnType.DESTROY_X_SQUARE_MINE )
            if( mined.Body.MineType == EMineType.SQUARE ) 
                G.HS.SquareMineMinedCount++;
        }
                
        for( int y = ( int ) G.HS.Area.yMin - 1; y < G.HS.Area.yMax + 1; y++ )
        for( int x = ( int ) G.HS.Area.xMin - 1; x < G.HS.Area.xMax + 1; x++ )
        {
            Unit vault = Map.GMine( EMineType.VAULT, new Vector2( x, y ) );
            if( vault )
            if( vault.Mine.Activated )
            {
                if( mined )
                {
                    if( cn == EMineBonusCnType.MINE_X_MINE )
                        vault.Mine.MineMinedCount++;
                    if( cn == EMineBonusCnType.DESTROY_X_ROUND_MINE )                                             // counts different types of mines
                    if( mined.Body.MineType == EMineType.ROUND )
                        vault.Mine.RoundMineMinedCount++;
                    if( cn == EMineBonusCnType.DESTROY_X_SQUARE_MINE )
                    if( mined.Body.MineType == EMineType.SQUARE ) 
                        vault.Mine.SquareMineMinedCount++;
                }
                if( cn == EMineBonusCnType.COLLECT_X_RESOURCE )
                if( itype != ItemType.NONE )
                if( itype == vault.Mine.Unit.Body.MiningPrize )                                                   // counts resouces picked
                    vault.Mine.ResourceCollected += amt;                
                if( cn == EMineBonusCnType.MISS_X_ATTEMPTS )
                    vault.Mine.MissedAttempts++;                                                                  // failed attempt increment                
                if( cn == EMineBonusCnType.CONQUER_X_VAULTS )
                if( mined != vault )
                    vault.Mine.VaultsConquered++;                                                                 // Conquered Vaults increment                
                if( cn == EMineBonusCnType.ACTIVATE_X_VAULTS )
                if( mined != vault )
                    vault.Mine.VaultsActivated++;                                                                 // Conquered Vaults increment                
                if( cn == EMineBonusCnType.KILL_X_MONSTERS )
                    vault.Mine.KilledMonsters++;                                                                  // killed monsters increment                
                vault.Mine.AnimateIconTimer = .01f;
            }
        }
    }    
    public static void UpdateAfterHeroStep()
    {
        if( Manager.I.GameType != EGameType.CUBES ) return;
        if( G.HS.NextMineBonusChanceCount > 0 )
        {
            for( int dr = 0; dr < 8; dr++ )
            {
                Vector2 tg = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) dr ];
                Unit mine = Map.GFU( ETileType.MINE, tg );
                if( mine && Controller.CanBeMined( tg ) )
                {
                    mine.Body.ExtraMiningChance += G.HS.NextMineBonusChanceAmount;
                    MasterAudio.PlaySound3DAtVector3( "Save Game 2", mine.Pos );                                  // Sound FX
                    Controller.CreateMagicEffect( mine.Pos );                                                     // Magic FX 
                    G.HS.NextMineBonusChanceCount--;
                }
            }
        }

        string txt = "";
        if( G.HS.NextMineBonusChanceCount > 0 )
            txt += "Next " + G.HS.NextMineBonusChanceCount + " Neighbor Mines will get a bonus chance of " + G.HS.NextMineBonusChanceAmount + "%\n";

        if( G.HS.FreePushableObjects > 0 )
            txt += "Next " + G.HS.FreePushableObjects + " Mines Can be Pushed!\n";

        if( G.HS.DefaultMiningChanceItemID >= 0 )
        {
            string nm = G.GIT( G.HS.DefaultMiningChanceItemID ).GetName();
            float amt = Item.GetNum( ( ItemType ) G.HS.DefaultMiningChanceItemID );
            txt += "Default Mining Chance defined by " + nm + " Amount: " + amt + "%\n";
        }
        if( G.HS.DefaultMiningChance >= 0 )
        {
            txt += "Default Mining Chance: " + G.HS.DefaultMiningChance + "%\n";
        }
        if( G.HS.BehindMineBonusTimes > 0 )
        {
            string nm = G.GIT( G.HS.BehindMineBonusItemID ).GetName();
            float amt =  G.HS.BehindMineBonusAmount;
            txt += "Aligned Mines Behind Destroyed Rock Will give " + amt.ToString( "+0;-#" ) + " bonus " + nm + " Each (x" + G.HS.BehindMineBonusTimes + ")\n";
        }
        if( G.HS.AroundMineBonusTimes > 0 )
        {
            string nm = G.GIT( G.HS.AroundMineBonusItemID ).GetName();
            float amt =  G.HS.AroundMineBonusAmount;
            txt += "Mines Around Destroyed Rock Will give " + amt.ToString( "+0;-#" ) + " bonus " + nm + " Each (x" + G.HS.AroundMineBonusTimes + ")\n";
        }
        //if( G.HS.MinesAroundChiselBonusChance > 0 )
        //{
        //    float amt = G.HS.MinesAroundChiselBonusChance;
        //    txt += "Mines Around Chisel Bonus Chance: " + amt.ToString( "+0;-#" ) + "%\n";
        //}
        if( G.HS.BoomerangCanMine )
        {
            txt += "Boomerang Can Destroy Rocks.\n";
        }
        if( G.HS.FireballCanMine )
        {
            txt += "Fireball Can Destroy Rocks.\n";
        }
        if( G.HS.BehindMineBonusChanceTimes > 0 )
        {
            float amt = G.HS.BehindMineBonusChanceAmount;
            txt += "Mines Behind Destroyed Rock Will get " + amt.ToString( "+0;-#" ) + " bonus Chance (x" + G.HS.BehindMineBonusChanceTimes + ")\n";
        }
        if( G.HS.BehindMineBonusChanceTimes > 0 )
        {
            float amt = G.HS.BehindMineBonusCompoundChanceAmount;
            txt += "Mines Behind Destroyed Rock Will get COMPOUND " + amt.ToString( "+0;-#" ) + " bonus Chance (x" + G.HS.BehindMineBonusChanceTimes + ")\n";
        }
        if( G.HS.FailedMineMoveRandomly > 0 )
        {
            float amt = G.HS.FailedMineMoveRandomly;
            txt += "Failed Mine will Move Randomly x" + amt.ToString( "+0;-#" ) + " - Warning: Risk of Death!\n";
        }
        if( G.HS.HeroJumpOverMinesCost > 0 )
        {
            string nm = G.GIT( G.HS.HeroJumpOverMinesItemID ).GetName();
            float amt = G.HS.HeroJumpOverMinesCost;
            txt += "Hero Can Jump Over Mines By Frontfacing them for a cost of : " + amt + " " + nm + " Per Mine\n";
        }
        if( G.HS.FailedMineBonusChance > 0 )
        {
            float amt = G.HS.FailedMineBonusChance;
            txt += "Failed Mine Adds Extra Chance " + amt.ToString( "+0;-#" ) + "%\n";
        }
        if( G.HS.BagExtraBonusItemID > 0 )
        {
            string nm = G.GIT( G.HS.BagExtraBonusItemID ).GetName();
            float amt = G.HS.BagExtraBonusAmount;
            txt += "Each Collected Bag gives " + amt.ToString( "+0;-#" ) + " Extra" + nm + "\n";
        }
        if( G.HS.BagExtraInventoryItemID > 0 )
        {
            string nm = G.GIT( G.HS.BagExtraInventoryItemID ).GetName();
            float amt = G.HS.BagExtraInventoryAmount ;
            txt += "When you Pick a Bag, Inventory " + nm + " Increases by " + amt.ToString( "+0;-#" ) + "%\n";
        }
        if( G.HS.DestroyedRockSpawnArrow > 0 )
        {
            float amt = G.HS.DestroyedRockSpawnArrow;
            txt += "Destroyed Rocks Spawn Arrow x" + amt + "\n";
        }
        if( G.HS.MuddyRockSpawnBoulder > 0 )
        {
            float amt = G.HS.MuddyRockSpawnBoulder;
            txt += "Rocks Destroyed over Mud Spawn Boulder x" + amt + "\n";
        }
        if( G.HS.AlcovePush > 0 )
        {
            float amt = G.HS.AlcovePush;
            txt += "Rocks can be Pushed to Alcoves if there's only one free spot around it: x" + amt + "\n";
        }
        if( G.HS.RandomItemsCreatedBonus > 0 )
        {
            float amt = G.HS.RandomItemsCreatedBonus;
            txt += "Amount of Random Items Created: " + amt.ToString( "+0;-#" ) + "%" + "\n";
        }
        if( G.HS.PickaxeInterest > 0 )
        {
            float amt = G.HS.PickaxeInterest;
            txt += "Pickaxe Interest: " + amt + "% (Collect Pickaxes to Multiply Inventory by this Value)" + "\n";
        }
        if( G.HS.PickaxeInterestItemID > 0 )
        {
            string nm = G.GIT( G.HS.PickaxeInterestItemID ).GetName();
            float amt = G.HS.PickaxeInterestItemMultiplier;
            txt += "When you Pick a" + nm + " Pickaxe Interest Increases by " + amt.ToString( "+0;-#" ) + "%" + "\n";
        }
        if( G.HS.HeroChangeBoulderDir > 0 )
        {
            float amt = G.HS.HeroChangeBoulderDir;
            txt += "Hero can Change Boulder Direction by Bumping against it: x" + amt + "\n";
        }
        if( G.HS.BoulderPushPower > 1 )
        {
            float amt = G.HS.BoulderPushPower;
            if( amt >= 50 )
                txt += "Boulder Stack Push Power: MAX!\n";
            else
                txt += "Boulder Stack Push Power: +" + ( int ) amt + "\n";
        }
        if( G.HS.LinkedCrackedBnItemID >= 0 )
        {
            string nm = G.GIT( G.HS.LinkedCrackedBnItemID ).GetName();
            float amt = G.HS.LinkedCrackedBnAmount;
            txt += "When a Rock is Destroyed, all Cracked (Failed) mines that are linked to it get bonus: " + nm + " " + amt.ToString( "+0;-#" ) + "\n";
        }
        if( G.HS.HeroExpandMud != 0 )
        {
            float amt = G.HS.HeroExpandMud;
            txt += "Hero can Expand mud pools By Rotating Over mud: x" + ( int ) amt + "\n";
        }
        if( G.HS.NextVaultExtraPower != 0 )
        {
            float amt = G.HS.NextVaultExtraPower;
            txt += "Next Activated Vault Power: " + amt.ToString( "+0;-#" ) + "%\n";
        }
        if( G.HS.FailedInflationPerSide != 0 )
        {
            float amt = G.HS.FailedInflationPerSide;
            txt += "Failed Mine inflation per Side : " + amt.ToString( "+0;-#" ) + "\n";
        }
        if( G.HS.VaultReuse != 0 )
        {
            float amt = G.HS.VaultReuse;
            txt += "Hero Can Reuse a Vault by Bumping against it: x" + ( int ) amt + "\n";
        }
        if( G.HS.MaxTicTacMoves > 0 )
        {
            float amt = G.HS.TicTacMoveCount;
            txt += "Max TIC TAC Moves: " + ( int ) amt + " of " + G.HS.MaxTicTacMoves + "\n";
        }
        if( G.HS.DisplaceHeroTickNumber >= 0 )
        {
            float amt = G.HS.DisplaceHeroTickNumber;
            txt += "Hero is Randomly Displaced on tick number: " + ( int ) amt + "\n";
        }
        if( G.HS.DestroyedGiveExtraItemID >= 0 )
        {
            string nm = G.GIT( G.HS.DestroyedGiveExtraItemID ).GetName();
            float amt = G.HS.DestroyedGiveExtraItemAmount;
            float tot = G.HS.MineDestroyedCount * amt;
            txt += "Every Destroyed Rock so far Gives a Bonus of: " + amt.ToString( "+0.#" ) + "% When Picking " + nm + " (" + tot.ToString( "+0.#" ) + "%)\n";
        }
        if( G.HS.CrackedGiveExtraItemID >= 0 )
        {
            string nm = G.GIT( G.HS.CrackedGiveExtraItemID ).GetName();
            float amt = G.HS.CrackedGiveExtraItemAmount;
            float tot = G.HS.RemainingCracked * amt;
            txt += "Every Remaining Cracked Rock Gives a Bonus of: " + amt.ToString( "+0.#" ) + "% When Picking " + nm + " (" + tot.ToString( "+0.#" ) + "%)\n";
        }
        if( G.HS.GetExtraItemID >= 0 )
        {
            string nm = G.GIT( G.HS.GetExtraItemID ).GetName();
            float amt = G.HS.GetExtraItemAmount;
            txt += "Get Extra Bonus of: " + amt.ToString( "+0.#" ) + "% When Picking " + nm + "\n";
        }
        if( G.HS.MaxCostMiningFailureUses >= 1 )
        {
            float amt = G.HS.MaxCostMiningFailure;
            txt += "Max Cost When Hero Fails Mining a Rock is " + ( int ) amt + " (x" + G.HS.MaxCostMiningFailureUses + ")\n";
        }     
        if( G.HS.XDestroyedGiveItemID >= 0 )
        {
            string nm = G.GIT( G.HS.XDestroyedGiveItemID ).GetName();
            float amt = G.HS.XDestroyedGiveItemAmount;
            txt += "Every " + amt + " Destroyed Rocks Give you One " + nm + "\n";
        }
        if( G.HS.MaxCostMiningSuccessUses >= 1 )
        {
            float amt = G.HS.MaxCostMiningSuccess;
            txt += "Max Cost When Hero Succeeds Mining a Rock is " + ( int ) amt + " (x" + G.HS.MaxCostMiningSuccessUses + ")\n";
        }
        if( G.HS.DynamiteExplodeCracked >= 1 )
        {
            txt += "Dynamite will Expand Explosion to Nearby Cracked Mines: x" + ( int ) G.HS.DynamiteExplodeCracked + "\n";
        }
        if( G.HS.DestroyedCrackNeighborChance > 0 )
        {
            txt += "When a Mine is Destroyed, Neighbor Mines Have " + G.HS.DestroyedCrackNeighborChance + "% of Chance to Crack\n";
        }
        if( G.HS.FailedCrackNeighborChance > 0 )
        {
            txt += "When Mining Fails, Neighbor Mines Have " + G.HS.FailedCrackNeighborChance + "% of Chance to Crack\n";
        }

        if( txt != "" )
        {
            UI.I.SetBigMessage( txt, Color.green, 6f, 4.7f, 83, 40, 2 );                                                                             // Create green Big message text
            LastMessageWasBonus = true;
        }
        else
        {
            if( LastMessageWasBonus ) UI.I.SetBigMessage( "", Color.yellow, .1f );                                                                   // Create yellow Big message text
            LastMessageWasBonus = false;
        }
    }
    public static void UpdateMiningSuccessfulBonuses( Unit mine, ref float price )
    {
        EDirection dr = Util.GetTargetUnitDir( G.Hero.Pos, mine.Pos );
        List<Unit> behl = new List<Unit>();
        for( int i = 1; i < Sector.TSX; i++ )
        {
            Vector2 desttg = mine.Pos + ( Manager.I.U.DirCord[ ( int ) dr ] * i );
            Unit tgm = Map.GFU( ETileType.MINE, desttg );
            if( tgm && Controller.CanBeMined( desttg ) ) behl.Add( tgm );                                                               // make a list of destroyable mines behind
            else break;
        }

        List<Unit> arol = new List<Unit>();
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = mine.Pos + Manager.I.U.DirCord[ i ];
            Unit mn = Map.GFU( ETileType.MINE, tg );
            if( mn && Controller.CanBeMined( tg ) ) arol.Add( mn );                                                                     // make a list of mines around pos
        }

        if( G.HS.AroundMineBonusItemID >= 0 )                                                                                           // around mine bonus item
        if( G.HS.AroundMineBonusTimes > 0 )
            {
                for( int i = 0; i < arol.Count; i++ )
                {
                    Controller.CreateMagicEffect( arol[ i ].Pos );                                                                      // Magic FX  
                    Message.CreateMessage( ETileType.NONE, ( ItemType ) G.HS.AroundMineBonusItemID, "" +
                    G.HS.AroundMineBonusAmount.ToString( "+0;-#" ), arol[ i ].Pos, Color.green, false, false, 5, 0, -1 );               // localized bonus msg
                }
                Item.IgnoreMessage = true;
                Item.AddItem( ( ItemType ) G.HS.AroundMineBonusItemID, G.HS.AroundMineBonusAmount * arol.Count );
                MasterAudio.PlaySound3DAtVector3( "Save Game 2", mine.Pos );                                                            // Sound FX
                if( --G.HS.AroundMineBonusTimes == 0 )
                {
                    G.HS.AroundMineBonusTimes = -1;                                                                                     // last use, reset vars
                    G.HS.AroundMineBonusItemID = -1;
                    G.HS.AroundMineBonusAmount = -1;
                }
            }

        if( G.HS.BehindMineBonusItemID >= 0 )                                                                                           // behind mine bonus item
        if( G.HS.BehindMineBonusTimes > 0 )
        {
            for( int i = 0; i < behl.Count; i++ )
            {               
                Controller.CreateMagicEffect( behl[ i ].Pos );                                                                          // Magic FX  
                Message.CreateMessage( ETileType.NONE, ( ItemType ) G.HS.BehindMineBonusItemID, "" + 
                G.HS.BehindMineBonusAmount.ToString( "+0;-#" ), behl[ i ].Pos, Color.green, false, false, 5, 0, -1 );                   // localized bonus msg
            }
            Item.IgnoreMessage = true;
            Item.AddItem( ( ItemType ) G.HS.BehindMineBonusItemID, G.HS.BehindMineBonusAmount * behl.Count );
            MasterAudio.PlaySound3DAtVector3( "Save Game 2", mine.Pos );                                                              // Sound FX
            if(--G.HS.BehindMineBonusTimes == 0 )                                                             
            {
                G.HS.BehindMineBonusTimes = -1;                                                                                       // last use, reset vars
                G.HS.BehindMineBonusItemID = -1;
                G.HS.BehindMineBonusAmount = -1;
            }
        }

        if( G.HS.BehindMineBonusChanceAmount >= 0 )                                                                                   // behind mine bonus chance
        if( G.HS.BehindMineBonusChanceTimes > 0 )
            {
                for( int i = 0; i < behl.Count; i++ )
                {
                    behl[ i ].Body.ExtraMiningChance += G.HS.BehindMineBonusChanceAmount;
                    Controller.CreateMagicEffect( behl[ i ].Pos );                                                                    // Magic FX  
                }
                MasterAudio.PlaySound3DAtVector3( "Save Game 2", mine.Pos );                                                          // Sound FX
                if( --G.HS.BehindMineBonusChanceTimes == 0 )
                {
                    G.HS.BehindMineBonusChanceAmount = -1;                                                                            // last use, reset vars
                    G.HS.BehindMineBonusChanceTimes = -1;
                }
            }

        if( G.HS.BehindMineBonusCompoundChanceAmount >= 0 )                                                                           // Behind mine compound chance bonus
        if( G.HS.BehindMineBonusCompoundChanceTimes > 0 )
            {
                for( int i = 0; i < behl.Count; i++ )
                {
                    behl[ i ].Body.ExtraMiningChance += G.HS.BehindMineBonusCompoundChanceAmount * ( i + 1 );
                    Controller.CreateMagicEffect( behl[ i ].Pos );                                                                    // Magic FX  
                }
                MasterAudio.PlaySound3DAtVector3( "Save Game 2", mine.Pos );                                                          // Sound FX
                if( --G.HS.BehindMineBonusCompoundChanceTimes == 0 )
                {
                    G.HS.BehindMineBonusCompoundChanceAmount = -1;                                                                    // last use, reset vars
                    G.HS.BehindMineBonusCompoundChanceTimes = -1;
                }
            }
        if( G.HS.DestroyedRockSpawnArrow > 0 )                                                                                        // Spawn arrow over destroyed rock
        {
            Unit prefab = Map.I.GetUnitPrefab( ETileType.ARROW );
            GameObject go = Map.I.CreateUnit( prefab, mine.Pos );
            Unit un = go.GetComponent<Unit>();
            un.RotateTo( ( EDirection ) Util.GetRandomDir() );                                                                        // sets random dir
            G.HS.DestroyedRockSpawnArrow--;
        }
        if( G.HS.MuddyRockSpawnBoulder > 0 )                                                                                          // Spawn Boulder over destroyed muddy rock
        {
            Unit mud = Map.I.GetMud( mine.Pos );
            if( mud )
            {
                Unit prefab = Map.I.GetUnitPrefab( ETileType.BOULDER );
                GameObject go = Map.I.CreateUnit( prefab, mine.Pos );
                Unit un = go.GetComponent<Unit>();
                un.RotateTo( ( EDirection ) Util.GetRandomDir() );                                                                     // sets random dir
                un.Control.SpeedTimeCounter = 0;
                G.HS.MoveOrder.Insert( 0, un );
                G.HS.MuddyRockSpawnBoulder--;
            }
        }
        G.HS.DestroyedMinesInSequence++;
        G.HS.MissSameMineInSequence = 0;
        if( mine.Mine.HitCount >= 1 )
            G.HS.RemainingCracked--;

        if( G.HS.DestroyedCrackNeighborChance >= 1 )                                                                                  // On Mine Destroyed crack neighbor chance Vault power
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = mine.Pos + Manager.I.U.DirCord[ i ];
            Unit mn = Map.GFU( ETileType.MINE, tg );
            if( mn && Controller.TypeCanBeMined( mn ) )
            {
                if( Util.Chance( G.HS.DestroyedCrackNeighborChance ) )
                {
                    if( mn.Mine.HitCount == 0 )
                        G.HS.RemainingCracked++;
                    mn.Mine.HitCount++;
                    Controller.CreateMagicEffect( mn.Pos );                                                                            // Magic FX  
                    MasterAudio.PlaySound3DAtVector3( "Save Game 2", mine.Pos );                                                       // Sound FX
                }
            }
        }

        UpdateCrackedMineBonus( mine );                                                                                                // Cracked mine item bonus

        if( G.HS.MaxCostMiningSuccessUses >= 1 )                                                                                       //Max Success price Vault power
        if( price > G.HS.MaxCostMiningSuccess )
        {
            price = G.HS.MaxCostMiningSuccess;
            G.HS.MaxCostMiningSuccessUses--;
            Message.GreenMessage( "Max Successful Cost: " + G.HS.MaxCostMiningSuccess );
        }  
    }
    public static bool UpdateMiningFailedBonuses( Unit mine, ref float price )
    {
        bool res = false;
        if( G.HS.FailedMineMoveRandomly > 0 )                                                                  // move mine randomly after failed
        {
            List<Vector2> pl = new List<Vector2>();
            for( int i = 0; i < 8; i++ )
            {
                Vector2 tg = mine.Pos + ( Manager.I.U.DirCord[ i ] );
                if( mine.CanFlyFromTo( false, mine.Pos, tg ) )
                if( tg != G.Hero.Pos )
                    pl.Add( tg );                                                                             // make a list of possible targets
            }

            if( pl.Count >= 1 )
            {
                int id = Random.Range( 0, pl.Count );
                mine.CanFlyFromTo( true, mine.Pos, pl[ id ] );                                               // move mine to sorted position
            }
            else
            {
                Controller.MoveFlyingUnitTo( ref mine, mine.Pos, G.Hero.Pos );
                Map.I.SetHeroDeathTimer( .2f );                                                              // or kill the hero squashed
                G.Hero.Spr.transform.localPosition = new Vector3( 
                G.Hero.Spr.transform.localPosition.x, G.Hero.Spr.transform.localPosition.y, -1.75f );        
                res = true;
            }
            G.HS.FailedMineMoveRandomly--;
            MasterAudio.PlaySound3DAtVector3( "Boulder Rolling", mine.Pos );                                // sound FX
        }

        if( G.HS.FailedCrackNeighborChance >= 1 )                                                           // On Mine Failed crack neighbor chance Vault power
            for( int i = 0; i < 8; i++ )
            {
                Vector2 tg = mine.Pos + Manager.I.U.DirCord[ i ];
                Unit mn = Map.GFU( ETileType.MINE, tg );
                if( mn && Controller.TypeCanBeMined( mn ) )
                {
                    if( Util.Chance( G.HS.FailedCrackNeighborChance ) )
                    {
                        if( mn.Mine.HitCount == 0 )
                            G.HS.RemainingCracked++;
                        mn.Mine.HitCount++;
                        Controller.CreateMagicEffect( mn.Pos );                                             // Magic FX  
                        MasterAudio.PlaySound3DAtVector3( "Save Game 2", mine.Pos );                        // Sound FX
                    }
                }
            }
        
        G.HS.MissSameMineInSequence++;
        if( mine.Mine.HitCount == 0 )
            G.HS.RemainingCracked++;
        mine.Mine.HitCount++;                                                                               // hit count increment
        G.HS.DestroyedMinesInSequence = 0;

        if( G.HS.MaxCostMiningFailureUses >= 1 )                                                            // Max Failed price Vault power
        if( price > G.HS.MaxCostMiningFailure )
        {
            price = G.HS.MaxCostMiningFailure;
            G.HS.MaxCostMiningFailureUses--;
            Message.GreenMessage( "Max Failure Cost: " + G.HS.MaxCostMiningFailure );
        }
        return res;
    }
    public static void UpdateAfterResourcePickup( ItemType it, float amt )
    {
        if( G.HS.PickaxeInterestItemID >= 0 )                                                              // Interest Given by item pickup
        if( G.HS.PickaxeInterestItemID == ( int ) it )
        {
            G.HS.PickaxeInterest += amt * G.HS.PickaxeInterestItemMultiplier;
            return;
        }
                                                             
        if( G.HS.BagExtraInventoryItemID >= 0 )                                                            // Bag gives inventory bonus in percent
        if( it == ItemType.Res_Mining_Bag )
        {
            float bag = Item.GetNum( ItemType.Res_Mining_Bag );
            if( bag != 0 )
            {
                string nm = G.GIT( G.HS.BagExtraInventoryItemID ).GetName();
                float num = Item.GetNum( ( ItemType ) G.HS.BagExtraInventoryItemID );
                num = Util.Percent( G.HS.BagExtraInventoryAmount, num );
                Item.AddItem( ( ItemType ) G.HS.BagExtraInventoryItemID, num );
            }
        }
        float sum = 0;
        string msg = "";
        string inttxt = "";
        if( G.HS.GetExtraItemID == ( int ) it )                                                                         // Get extra item vault power
        {
           sum += GiveExtraItem( amt, ref msg, "Bonus: ", ( ItemType ) G.HS.GetExtraItemID,
            G.HS.GetExtraItemAmount, 1 );
        }
        if( G.HS.DestroyedGiveExtraItemID == ( int ) it )                                                               // Destroyed give extra item vault power
         {
             sum += GiveExtraItem( amt, ref msg, "Destroyed: ", ( ItemType ) G.HS.DestroyedGiveExtraItemID,
             G.HS.DestroyedGiveExtraItemAmount, G.HS.MineDestroyedCount );
         } 
         if( G.HS.CrackedGiveExtraItemID == ( int ) it )                                                               // Cracked give extra item vault power
         {
             sum += GiveExtraItem( amt, ref msg, "Cracked: ", ( ItemType ) G.HS.CrackedGiveExtraItemID,
             G.HS.CrackedGiveExtraItemAmount, G.HS.RemainingCracked );
         }

         if( it == ItemType.Res_Mining_Points )
         if( G.HS.PickaxeInterest != 0 )                                                                               // Pickaxe interest
         {
             float stock = Item.GetNum( ItemType.Res_Mining_Points );
             float inter = stock * ( G.HS.PickaxeInterest / 100 );
             Item.IgnoreMessage = true;
             Item.AddItem( ItemType.Res_Mining_Points, inter );                                                       // gives Interest mining prize 
             inttxt = "Interest: +" + inter.ToString( "0.#" ) + " (" + G.HS.PickaxeInterest + "%)";
             Message.CreateMessage( ETileType.NONE, ItemType.Res_Mining_Points,
             inttxt, G.Hero.Pos + new Vector2( 2, 0 ), Color.green );                                                 // msg
         }

         if( msg != "" )
         {
             float tot = sum + amt;
             Message.CreateMessage( ETileType.NONE, it,
             "Extra: " + sum.ToString( "+0.#" ), G.Hero.Pos, Color.green );                                               
             UI.I.SetTurnInfoText( "Extra:\n" + msg + "Total: " + 
             tot.ToString( "+0.#" ) + "\n" + inttxt, 15, Color.green );                                               // msg
         }
    }
    public static float GiveExtraItem( float given, ref string msg, string title, ItemType it, float itamt, int count )
    {
        float perc = count * itamt;
        float extra = given * ( perc / 100 );
        Item.IgnoreMessage = true;
        Item.AddItem( it, extra );                                                                                         // gives extra item
        msg += title + "" + extra.ToString( "+0.#" ) + " (" + perc + "%)\n";
        return extra;
    }
    public static void UpdateCrackedMineBonus( Unit mine )
    {
        if( G.HS.LinkedCrackedBnItemID < 0 ) return;
        if( G.HS.LinkedCrackedBnAmount == 0 ) return;
        int step = 0;
        List<Unit> ul = new List<Unit>();
        GetLinkedCrackedMineList( mine.Pos, ref ul, ref step );                                                  // Recursive function to Find all connected failed mines
        
        for( int i = 0; i < ul.Count; i++ )
        if ( ul[ i ] != mine )
        {
            Unit un = ul[ i ];
            un.Body.MiningPrize = ( ItemType ) G.HS.LinkedCrackedBnItemID;                                       // adds the prize
            un.Body.MiningBonusAmount += G.HS.LinkedCrackedBnAmount;
            Controller.CreateMagicEffect( un.Pos );                                                              // FX
            un.Mine.AnimateIconTimer = .001f;
        }
        if( ul.Count > 0 )
            MasterAudio.PlaySound3DAtVector3( "Save Game 2", mine.Pos );                                         // Sound FX
    }
    private static void GetLinkedCrackedMineList( Vector2 pos, ref List<Unit> ul, ref int step )
    {
        Unit mine = Map.GFU( ETileType.MINE, pos );
        if( mine == null ) return;
        if( step > 0 )
        if( mine.Body.Sprite5.gameObject.activeSelf == false ) return;                                          // is is cracked     
        if( ul.Contains( mine ) ) return;
        if( step > 0 )
            ul.Add( mine );                                                                                     // add cracked mine to list
        step++;
        for( int i = 0; i < 8; i++ )
        {
            Vector2 ttg = mine.Pos + Manager.I.U.DirCord[ i ];
            GetLinkedCrackedMineList( ttg, ref ul, ref step );                                                  // Recursive function to Find all connected failed mines
        }
    }
    public static bool UpdateTunnelTravel( Vector2 from, Vector2 to )
    {
        Unit tun = Map.GMine( EMineType.TUNNEL, to );
        if( tun == null ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return true;
        if( Map.DoesLeverBlockMe( to, G.Hero ) ) return false;
        EDirection dr = Util.GetTargetUnitDir( from, to );
        Vector2 middle = new Vector2( -1, -1 );
        for( int phase = 1; phase <= 2 ; phase++ )                                                                   // loop for two phases back and forth
        for( int i = 0; i < Sector.TSX; i++ )                                                                        // main axis loop
        {
            float sig = 1;
            if( phase == 2 ) sig = -1;                                                                               // signal to use in the main loop
            Vector2 tg = tun.Pos + ( Manager.I.U.DirCord[ ( int ) dr ] * i * sig );
            if( Sector.GetPosSectorType( tg ) == Sector.ESectorType.GATES ) break;
            if( Map.LowTerrain( tg ) ) break;
            for( int j = 0; j < Sector.TSX; j++ )                                                                    // second lateral loop
            {
                Vector2 tg2 = tg + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * j );
                if( j == 0 ) middle = tg;
                if( Sector.GetPosSectorType( tg2 ) == Sector.ESectorType.GATES ) break;
                if( Map.LowTerrain( tg2 ) ) break;
                if( j > 0 )
                {
                    if( G.Hero.Dir == dr ) break;                                                                    // this is to avoid 2 loops in the same direction
                    if( G.Hero.Dir == Util.GetInvDir( dr ) ) break;
                }
                Unit tun2 = Map.GMine( EMineType.TUNNEL, tg2 );
                if( phase == 2 && j == 0 ) 
                    return true;   // new 
                if( tun2 )                                                                                           // tunnel found!
                if( tun != tun2 ) 
                {
                    Vector2 outtg = tun2.Pos + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] );
                    Vector2 outtg1 = outtg;
                    bool ok = false;
                    int tofl = Controller.GetUnitFloor( tun2.Pos, outtg, G.Hero );
                    if( G.Hero.Control.Floor != tofl )
                    if( Map.GFU( ETileType.MINE, outtg ) )
                        if( Controller.UpdateMining( tun2.Pos, outtg ) == false && !ProceedTrip )                   // Attempts to mine before leaving tunnel
                            return true;                          

                    TunnelTraveling = true;
                    if( G.Hero.CanMoveFromTo( true, tun2.Pos, outtg, G.Hero ) ) ok = true;                           // move hero to first exit if possible
                    if( ok == false )
                    {
                        outtg = tun2.Pos + ( Manager.I.U.DirCord[ ( int ) Util.GetInvDir( G.Hero.Dir ) ] );
                        if( G.Hero.CanMoveFromTo( true, tun2.Pos, outtg, G.Hero ) ) ok = true;                       // or try the second inverse exit

                        ProceedTrip = false;
                        tofl = Controller.GetUnitFloor( tun2.Pos, outtg, G.Hero );
                        if( G.Hero.Control.Floor != tofl )
                        if( Controller.UpdateMining( tun2.Pos, outtg ) == false && !ProceedTrip )                   // Attempts to mine before leaving tunnel
                            return true;                     
                    }
                    if( ok )                                    
                    {
                        Map.I.LineEffect( tun.Pos, middle, 2.5f, .5f, Color.blue, Color.blue );                      // Travel Line FX
                        Map.I.LineEffect( middle, tun2.Pos, 2.5f, .4f, Color.blue, Color.blue );
                        G.Hero.Graphic.transform.position = from;
                        Unit fire = Map.I.GetUnit( ETileType.FIRE, outtg );                                          // hero fire step
                        if( fire && fire.Body.FireIsOn )
                        {
                            MasterAudio.PlaySound3DAtVector3( "Fire Ignite", outtg );
                            Map.I.SetHeroDeathTimer( .8f );
                        }
                        Map.I.TunnelPhase = 0;                                                                       // fill animation data
                        Map.I.TunnelPhaseTimer = 0;
                        Map.I.TunnelCordList = new List<Vector2>();
                        Map.I.TunnelCordList.Add( from );
                        Map.I.TunnelCordList.Add( tun.Pos );
                        Map.I.TunnelCordList.Add( tun2.Pos );
                        Map.I.TunnelCordList.Add( outtg );
                        MasterAudio.PlaySound3DAtVector3( "Tunnel Travel", outtg );                                  // sound FX
                        G.Hero.Control.UpdateTrapStepping(
                        ( int ) from.x, ( int ) from.y, ( int ) to.x, ( int ) to.y );
                    }
                    else
                    {                                                                                                // No exit found!
                        Message.CreateMessage( ETileType.NONE, ItemType.NONE, "Front Exit\nBlocked!",
                        outtg1 + new Vector2( -0.5f, 0 ), Color.red, false, false, 3, 0, -1 );                       // localized msg
                        Message.CreateMessage( ETileType.NONE, ItemType.NONE, "Back Exit\nBlocked!",
                        outtg + new Vector2( -0.5f, 0 ), Color.red, false, false, 3, 0, -1 );                        // localized msg
                        Controller.CreateMagicEffect( outtg );                                                       // Magic FX  
                        Controller.CreateMagicEffect( outtg1 );                                                      // Magic FX  
                        MasterAudio.PlaySound3DAtVector3( "Error 2", outtg );                                        // sound FX
                    }
                    TunnelTraveling = false;
                    return true; 
                }
            }
        }
        return true;
    }
    public static void UpdateTunnelAnimation()
    {
        Vector3 from = Map.I.TunnelCordList[ 0 ];
        Vector3 to = Map.I.TunnelCordList[ 1 ];
        Map.I.TunnelPhaseTimer += Time.deltaTime;
        if( Map.I.TunnelPhase == 0 )                                                                               // first phase move to first hole
        {
            G.Hero.Spr.color = new Color( 1, 1, 1, 1 - ( Map.I.TunnelPhaseTimer * 4 ) );
            G.Hero.Graphic.transform.position = Vector3.Lerp( from, to, Map.I.TurnTime * 12 );
            if( G.Hero.Graphic.transform.position == to )
              { Map.I.TunnelPhase++; Map.I.TunnelPhaseTimer = 0; }
        }
        else
        if( Map.I.TunnelPhase == 1 )                                                                               // fade out hero sprite
        {
            G.Hero.Spr.color = new Color( 1, 1, 1, 1 - ( Map.I.TunnelPhaseTimer * 4 ) );
            if( G.Hero.Spr.color.a <= 1f ) 
              { Map.I.TunnelPhase++; Map.I.TunnelPhaseTimer = 0; }
        }
        else
        if( Map.I.TunnelPhase == 2 )                                                                              // fade in hero sprite
        {
            G.Hero.Graphic.transform.position = Map.I.TunnelCordList[ 2 ];
            G.Hero.Spr.color = new Color( 1, 1, 1, Map.I.TunnelPhaseTimer * 8 );
            if( G.Hero.Spr.color.a >= 1 )
              { Map.I.TunnelPhase++; Map.I.TunnelPhaseTimer = 0; }
        }
        else
        if( Map.I.TunnelPhase == 3 )                                                                              // last phase move hero out of tunnel
        {
            from = Map.I.TunnelCordList[ 2 ];
            to = Map.I.TunnelCordList[ 3 ];
            G.Hero.Graphic.transform.position = Vector3.Lerp( from, to, Map.I.TunnelPhaseTimer * 12 );
            if( G.Hero.Graphic.transform.position == to )
                Map.I.TunnelPhase = -1;
        }
    }
    public static void UpdateSupportStatus( Unit un )
    {
        if( un.Mine.FrontSupport == -1 )
        {
            un.Mine.FrontSupport = 0;
            Vector2 tg = un.Pos + Manager.I.U.DirCord[ ( int ) un.Dir ];
            Unit tom = Map.GFU( ETileType.MINE, tg );
            if( tom )
            if( tom.Body.MineType != EMineType.BRIDGE )
            if( tom.Body.MineType != EMineType.LADDER )
            if( tom.Body.UPMineType == EMineType.NONE )
                un.Mine.FrontSupport = 1;
        }
        if( un.Mine.BackSupport == -1 )
        {
            un.Mine.BackSupport = 0;
            Vector2 tg = un.Pos + Manager.I.U.DirCord[ ( int ) Util.GetInvDir( un.Dir ) ];
            Unit tom = Map.GFU( ETileType.MINE, tg );
            if( tom )
            if( tom.Body.MineType != EMineType.BRIDGE )
            if( tom.Body.MineType != EMineType.LADDER )
            if( tom.Body.UPMineType == EMineType.NONE )
                un.Mine.BackSupport = 1;
        }
        //Debug.Log( "front " + un.Mine.FrontSupport + " back " + un.Mine.BackSupport );
    }
    public static void UpdateStickyMine()
    {
        List<Unit> ml = new List<Unit>();
        List<Vector2> bl = new List<Vector2>();
        int range = 2;
        for( int y = ( int ) G.Hero.Pos.y - range; y <= G.Hero.Pos.y + range; y++ )
        for( int x = ( int ) G.Hero.Pos.x - range; x <= G.Hero.Pos.x + range; x++ )
        {
            Unit mine = Map.GFU( ETileType.MINE, new Vector2( x, y ) );                    
            if( mine != null && mine.Mine.StickyMine )
            {
                Vector2 bestpos = mine.Control.GetBestStandardMove( G.Hero.Pos );
                if( mine.CanFlyFromTo( false, mine.Pos, bestpos ) )
                if( Util.IsNeighbor( G.Hero.Pos, mine.Pos ) == false )
                if( Util.IsNeighbor( G.Hero.Pos, bestpos ) )
                {
                        bl.Add( bestpos );                                                           // make a list of all best targets
                        ml.Add( mine );                                                              // make a list of all close mines
                }
            }
        }
        List<int> idl = new List<int>();
        bool block = false;
        for( int i = bl.Count - 1; i >= 0; i-- )
        {
            int count = 0;
            for( int j = bl.Count - 1; j >= 0; j-- )
                if( bl[ j ] == bl[ i ] )
                {
                    count++;                                                                          // counts same position best targets
                }
            if( count >= 2 )
            {
                block = true;
                idl.Add( i );
                Message.StaticMsg( "Block!", bl[ i ],  Color.red, .8f, 75 );
                MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos, .5f );                       // sound FX
            }
        }

        for( int i = bl.Count - 1; i >= 0; i-- )
        {
            if( block )
                iTween.ShakePosition( ml[ i ].Graphic.gameObject,                                     // Shake FX
                new Vector3( .09f, .09f, 0 ), .2f );
            if( idl.Contains( i ) )
            {
                bl.RemoveAt( i );                                                                     // remove duplicated same target positions
                ml.RemoveAt( i );
            }
        }

        for( int i = 0; i < ml.Count; i++ )
        {
            Vector2 bestpos = ml[ i ].Control.GetBestStandardMove( G.Hero.Pos );
            ml[ i ].CanFlyFromTo( true, ml[ i ].Pos, bestpos );                                       // move mines
        }

        if( ml.Count > 0 )
            MasterAudio.PlaySound3DAtVector3( "Slime 1", G.Hero.Pos );                                // sound FX
    }
    public static bool UpdateStickyMineBump( Vector2 from, Vector2 to )
    {
        Unit mine = Map.GFU( ETileType.MINE, to );
        if( mine == null ) return false;
        if( mine.Mine.StickyMine == false ) return false;

        if( GetMineBonusCount( to ) > 0 ) return false;                                               // mine already has a bonus

        int count = StickyMineCanSuck( to );                                                          // can it suck the bonus

        if( count == 1 )
        {
            SetMineBonus( ref mine, BonusID, true );                                                  // sets the bonus
            mine.Body.MiningPrize = BonusMine.Body.MiningPrize;
            mine.Body.MiningBonusAmount = BonusMine.Body.MiningBonusAmount;

            for( int i = 0; i <= TotBonus;i++ )
            if( i != 7 )
                SetMineBonus( ref BonusMine, i, false );                                              // clears source mine bonuses

            BonusMine.Body.MiningPrize = ItemType.NONE;
            BonusMine.Body.MiningBonusAmount = 0;
            Controller.CreateMagicEffect( mine.Pos );                                                 // Create Magic effect
            MasterAudio.PlaySound3DAtVector3( "Click 2", mine.Pos );                                  // Sound FX

            Vector3 aux = BonusMine.Body.Sprite3.transform.position;
            mine.Body.Sprite3.transform.position = aux;
            mine.Body.Sprite3.transform.position = aux;                                                                               // Slide animation FX
            mine.Body.Sprite4.transform.position = aux;                                                                               // Slide animation FX
            mine.Body.EffectList[ 3 ].transform.position = aux;
            mine.Mine.AnimateIconTimer = 1f;
        }
        return true;
    }
    public static int StickyMineCanSuck( Vector2 to )
    {
        BonusMine = null;
        BonusID = -1;
        int count = 0;
        for( int dr = 0; dr < 8; dr++ )
        {
            Vector2 tg = to + Manager.I.U.DirCord[ ( int ) dr ];
            count += GetMineBonusCount( tg );
        }
        return count;
    }
    public static int GetMineBonusCount( Vector2 tg, bool stick = false, bool item = true )
    {
        Unit mine = Map.GFU( ETileType.MINE, tg );
        int count = 0;
        if( mine == null ) return 0;
        if( mine.Body.MineType == EMineType.VAULT ) return 0;
        if( item && mine.Body.MiningPrize != ItemType.NONE ) { count++; BonusMine = mine; BonusID = 1; }
        if( mine.Mine.HammerMine ) { count++; BonusMine = mine; BonusID = 2; }
        if( mine.Mine.RopeMine ) { count++; BonusMine = mine; BonusID = 3; }
        if( mine.Mine.HoleMine ) { count++; BonusMine = mine; BonusID = 4; }
        if( mine.Mine.SwapperMine ) { count++; BonusMine = mine; BonusID = 5; }
        if( mine.Mine.ChiselMine ) { count++; BonusMine = mine; BonusID = 6; }
        if( mine.Mine.StickyMine && stick ) { count++; BonusMine = mine; BonusID = 7; }
        if( mine.Mine.DynamiteMine ) { count++; BonusMine = mine; BonusID = 8; }
        if( mine.Mine.SpikedMine ) { count++; BonusMine = mine; BonusID = 9; }
        if( mine.Mine.CogMine ) { count++; BonusMine = mine; BonusID = 10; }
        if( mine.Mine.ArrowMine ) { count++; BonusMine = mine; BonusID = 11; }
        if( mine.Mine.CannonMine ) { count++; BonusMine = mine; BonusID = 12; }
        if( mine.Mine.MagnetMine ) { count++; BonusMine = mine; BonusID = 13; }
        if( mine.Mine.WheelMine ) { count++; BonusMine = mine; BonusID = 14; }
        if( mine.Mine.JumperMine ) { count++; BonusMine = mine; BonusID = 15; }
        if( mine.Mine.GloveMine ) { count++; BonusMine = mine; BonusID = 16; }
        return count;
    }
    public static void SetMineBonus( ref Unit tgmine, int id, bool val )
    {
        if( id == 2 ) { tgmine.Mine.HammerMine = val; }
        if( id == 3 ) { tgmine.Mine.RopeMine = val; }
        if( id == 4 ) { tgmine.Mine.HoleMine = val; }
        if( id == 5 ) { tgmine.Mine.SwapperMine = val; }
        if( id == 6 ) { tgmine.Mine.ChiselMine = val; }
        if( id == 7 ) { tgmine.Mine.StickyMine = val; }
        if( id == 8 ) { tgmine.Mine.DynamiteMine = val; }
        if( id == 9 ) { tgmine.Mine.SpikedMine = val; }
        if( id == 10 ) { tgmine.Mine.CogMine = val; }
        if( id == 11 ) { tgmine.Mine.ArrowMine = val; }
        if( id == 12 ) { tgmine.Mine.CannonMine = val; }
        if( id == 13 ) { tgmine.Mine.MagnetMine = val; }
        if( id == 14 ) { tgmine.Mine.WheelMine = val; }
        if( id == 15 ) { tgmine.Mine.JumperMine = val; }
        if( id == 16 ) { tgmine.Mine.GloveMine = val; }
    }
    public static void CopyMineBonus( Unit from, Unit to )
    {
        to.Mine.MineBonusUses = from.Mine.MineBonusUses;
        to.Mine.ChiselMine = from.Mine.ChiselMine;
        to.Mine.RopeMine = from.Mine.RopeMine;
        to.Mine.HoleMine = from.Mine.HoleMine;
        to.Mine.HammerMine = from.Mine.HammerMine;
        to.Mine.SwapperMine = from.Mine.SwapperMine;
        to.Mine.StickyMine = from.Mine.StickyMine;
        to.Mine.DynamiteMine = from.Mine.DynamiteMine;
        to.Mine.SpikedMine = from.Mine.SpikedMine;
        to.Body.MiningPrize = from.Body.MiningPrize;
        to.Mine.CogMine = from.Mine.CogMine;
        to.Mine.ArrowMine = from.Mine.ArrowMine;
        to.Mine.CannonMine = from.Mine.CannonMine;
        to.Mine.MagnetMine = from.Mine.MagnetMine;
        to.Mine.WheelMine = from.Mine.WheelMine;
        to.Mine.JumperMine = from.Mine.JumperMine;
        to.Mine.GloveMine = from.Mine.GloveMine;
        to.Body.MiningBonusAmount = from.Body.MiningBonusAmount;
        to.Mine.MineBonusDir = from.Mine.MineBonusDir;
        to.Body.Sprite3.color = from.Body.Sprite3.color;
        to.Body.Sprite3.transform.eulerAngles = from.Body.Sprite3.transform.eulerAngles;
    }
    public static void FindRandomWheel( Vector2 tg )
    {
        List<Unit> ul = new List<Unit>();
        List<int> dr = new List<int>();
        for( int i = 0; i < 8; i++ )
        {
            Unit mm = Map.GFU( ETileType.MINE, tg + Manager.I.U.DirCord[ i ] );
            if( mm  )
            if( Mine.GetMineBonusCount( mm.Pos ) == 0 )
            if( mm.Mine.WheelMine == false )
              { ul.Add( mm ); dr.Add( i ); }
        }
        if( ul.Count > 0 )
        {
            int id = Random.Range( 0, ul.Count );
            ul[ id ].Mine.MineBonusDir = ( EDirection ) Util.GetInvDir( ( EDirection ) dr[ id ] );
            ul[ id ].Mine.WheelMine = true;
            ul[ id ].Mine.AnimateIconTimer = .1f;
            ul[ id ].Mine.MineBonusUses = 1;                                                      // using this to prevent Wheel direction change
        }
    }
    public static bool UpdateBoulderDirectionChange( Vector2 from, Vector2 to )
    {
        if( Manager.I.GameType != EGameType.CUBES ) return false;
        if( G.HS.HeroChangeBoulderDir < 1 ) return false;
        Unit bld = Map.I.GetUnit( ETileType.BOULDER, to );
        if( bld )
        {
            if( bld.Control.BoulderDirChanged ) return false;                                       // To avoid multiple dir changes
            EDirection mov = Util.GetTargetUnitDir( from, to );
            if( mov == EDirection.NONE ) return false;
            if( bld.Dir == mov ) goto error;  
            if( bld.Control.SetBoulderDir( mov ) == false ) goto error;                              // Check to see if boulder can be rotated
            bld.Control.BoulderDirChanged = true;
            bld.Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );
            G.HS.HeroChangeBoulderDir--;                                                             // Power decrement
            Controller.CreateMagicEffect( to );                                                      // Magic FX
            MasterAudio.PlaySound3DAtVector3( "Boulder Rolling", to );                               // Sound FX
            return true;
            error:
            MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );                               // Error sound FX
        }
        return false;
    }
    public static void UpdateMudExpansionVault( EDirection OldHeroDir )
    {
        if( G.HS == null ) return;
        if( OldHeroDir == G.Hero.Dir ) return;
        Unit overm = Map.I.GetMud( G.Hero.Pos );                                       // Check if all conditions met
        if( overm == null ) return;
        Vector2 frtg = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) OldHeroDir ];
        Vector2 totg = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ];
        Unit mudfr = Map.I.GetMud( frtg );
        if( mudfr == null ) return;
        Unit mudto = Map.I.GetMud( totg );
        if( mudto ) return;
        if( G.HS.HeroExpandMud < 1 ) return;                                                           // not enough power
        Map.I.SetTile( ( int ) totg.x, ( int ) totg.y, ELayerType.GAIA, ETileType.MUD, true, true );   // set mud tile
        Map.AddNeighborTransToList( VI.VTOVI( totg ) );
        Util.PlayParticleFX( "Mud Splat", totg );                                                      // splat FX
        MasterAudio.PlaySound3DAtVector3( "Slime Step", totg, .7f );                                   // Sound FX
        G.HS.HeroExpandMud--;
    }
}
