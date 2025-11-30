using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using PathologicalGames;
using Sirenix.OdinInspector;

#region Enums
public enum EFishCatchType
{
    NONE = 0, RANDOM, ABSOLUTE
}
public enum EFishType
{
    NONE = -1, FISH_1, FISH_2, FISH_3, FISH_CRAB, FISH_MANTA, FISH_SNAKE, FISH_BROWN, FISH_FROG,
    FAST_TILE = 100, BONUS_TIME, EMPTY, CLOSED_WATER_TRAP, OPEN_WATER_TRAP,
    BAIT, HOOK_PULL, INT_TILE, BONUS_HOOK_SMALL, BONUS_HOOK_MEDIUM,
    BONUS_HOOK_LARGE, TIME_SKIP, BONUS_FISHING_LEVEL
}
public enum EPoleBonusEffType
{
    NONE = -1,
    SPAWN_SIMILAR = 10,
    SPAWN_DEFAULT,
    GLOW_FISH,
    SET_FISH_HP,
    GIVE_PRIZE,
    DISABLE_CATCH,
    ENABLE_CATCH,
    SPAWN_LAST_GLOWED,
    SPAWN_INVENTORY,
    SPAWN_ALL_GLOWED,
    ADD_TIME = 100,
    EXTRA_FISH_CATCH,
    ADD_POWER,
    ADD_SPEED,
    ADD_RADIUS,
    RETURN_CATCH,
    ADD_FISH_PERCENT,
    MULTIPLY_FISH_PERCENT,
    ENABLE_ALGAE_DESTROY,
    ENABLE_RAFT_DESTROY,
    INVENTORY_FISH_ADD,
    INVENTORY_FISH_MULTIPLY,
    INVENTORY_ALL_ADD,
    INVENTORY_ALL_MULTIPLY,
    ADD_GREEN_ATT_POWER,
    ADD_YELLOW_ATT_POWER,
    ADD_RED_ATT_POWER,
    IMPULSIONATE_RAFT,
    ADD_TO_NEXT_POLE_BONUS,
    MULTIPLY_NEXT_POLE_BONUS,
    HARPOON_ATTACK_BONUS,
    ATTACKED_FISH_PERCENT_DOUBLED,
    HOOK_DESTROY_ARROWS,
    HOOK_PUSH_ARROWS,
    FOG_FISH_ATTACK_BONUS,
    INCREASE_FISH_PERCENT,
    GIVE_X_SMALL_HOOK,
    GIVE_X_MEDIUM_HOOK,
    GIVE_X_LARGE_HOOK,
    SWITCH_MERGING
}
public enum EPoleBonusCnType
{
    NONE = -1, FIRST_ATTACK_VIRGIN,
    FIRST_ATTACK_ANY_HP,
    FIRST_ATTACK_HP_HIGHER_THAN,
    FIRST_ATTACK_HP_SMALLER_THAN,
    FIRST_ATTACK_AT_EXACT_HP,
    HOOK_LEAVE_AT_EXACT_HP,
    HOOK_LEAVE_AT_RANGE_HP,
    FIRST_ATTACK_AT_RANGE_HP,
    MARK_FISH,
    HOOK_STRIKE_ORB,
    ATTACK_X_VIRGIN_FISH,
    THROW_HOOK,
    DESTROY_X_ALGAE,
    SIMULTANEOUS_FISH_ATTACKED,
    SIMULTANEOUS_FISH_CATCH,
    HOOK_LEAVE_AT_HP_HIGHER_THAN,
    HOOK_LEAVE_AT_HP_SMALLER_THAN,
    FINISH_FISHING,
    STEP_THE_POLE,
    PULL_ALGAE_X_METERS,
    MOVE_HOOK_X_METERS,
    HARPOON_ATTACK_X_PERCENT,
    HAVE_X_FISH_IN_INVENTORY,
    HOOK_REMAIN_AROUND_LAND,
    MOVE_HOOK_X_METERS_FROM_POLE,
    CONQUER_X_HOOK_BONUS,
    FISH_FOR_X_SECONDS
}
public enum EFishingPhase
{
    NO_FISHING = -1, INTRO, FISHING, FINISH
}
public enum EFishSwimType
{
    NONE = -1, NORMAL, STILL, FLEE_TO_FARTHEST,
    FLEE_TO_RANDOM
}
public enum EFishActionConditionType
{
    NONE = -1, FishPercent, TotalSecondsOver, HitAndRun,
    WhileFishing,
    WhileNotFishing
}
public enum EFishActionEffectType
{
    NONE = -1, SwimType
}
#endregion

[System.Serializable]
public class Water : MonoBehaviour
{
    #region Variables
    [TabGroup( "Main" )]
    public bool PoleBonusIsGlobal = true;
    [TabGroup( "Main" )]
    public EPoleBonusEffType PoleBonusEffType = EPoleBonusEffType.NONE;
    [TabGroup( "Main" )]
    public EPoleBonusCnType PoleBonusCnType = EPoleBonusCnType.NONE;
    [TabGroup( "Main" )]
    public float PoleBonusVal1 = -1;
    [TabGroup( "Main" )]
    public float PoleBonusVal2 = -1;
    [TabGroup( "Main" )]
    public bool PoleBonusFailed = false;
    [TabGroup( "Main" )]
    public bool PoleBonusActive = true;
    [TabGroup( "Link" )]
    public Unit Unit;
    public static bool PoleBonusGiven;
    [TabGroup( "Main" )]
    public float IgnoreAttackTime = 0;
    [TabGroup( "Fish" )]
    public ItemType DefaultItemPrize = ItemType.Res_Water_Flower;
    [TabGroup( "Fish" )]
    public bool GlowingFish = false;
    [TabGroup( "Fish" )]
    public int MarkedFish = 0;
    #region Statics
    public static string ErrorMsg = "";
    public static string PoleBonusText = "";
    public static string PoleBonusEffText = "";
    public static bool ForceUpdatePoleText = false;

    public static bool TempMarkingEnabled = false;
    public static bool TempCatchEnabled = true;
    public static bool TempImpulsionateEnabled = false;
    public static float TempFishingExtraAttack = 0;
    public static float TempHarpoonAttackBonus = 0;
    public static float TempFogExtraAttack = 0;
    public static float TempGreenFishExtraAttack = 0;
    public static float TempYellowFishExtraAttack = 0;
    public static float TempRedFishExtraAttack = 0;
    public static float TempFishingExtraSpeed = 0;
    public static float TempFishingExtraRadius = 0;
    public static int TempFishingExtraPrize = 0;
    public static int TempFishingReturnCatch = 0;
    public static float TempFishAddPercent = 0;
    public static float TempFishMultiplyPercent = 0;
    public static float TempFishIncreasePercent = 0;
    public static int TempOrbStrikeCount = 0;
    public static int TempNumAttackedFish = 0;
    public static bool TempAlgaeDestroy = false;
    public static int TempCountAlgaeDestroy = 0;
    public static bool TempRaftDestroy = false;
    public static float TempAlgaePullDistance = 0;
    public static int TempGlowNextFishAmount = 0;
    public static float TempHookMoveDistance = 0;
    public static float TempHarpoonAttackPercent = 0;
    public static float TempSetFishPercent = -1;
    public static float TempDoubleNextPerAmt = 0;
    public static bool TempArrowDestroy = false;
    public static bool TempArrowPush = false;

    public static int MarkedFishCount = 0;
    public static int SimultaneousAttackedFish = 0;
    public static int SimultaneousCatchFish = 0;
    public static bool HeroLeftHook = true;
    public static Vector2 HookDropPosition;
    public static bool TempHookAroundLandTile = true;
    public static float TempHookDistanceFromPole = 0;
    public static float TimeToAdd = 0;
    public static bool ExtraHooksGiven = false;
    public static float FishingTime = 0;
    public static List<Unit> TempPoleAttackedFishList = new List<Unit>();
    #endregion
    #endregion
    public void Copy( Water un )
    {
        IgnoreAttackTime = un.IgnoreAttackTime;
        PoleBonusFailed = un.PoleBonusFailed;
        PoleBonusActive = un.PoleBonusActive;
        GlowingFish = un.GlowingFish;
        MarkedFish = un.MarkedFish;
        PoleBonusIsGlobal = un.PoleBonusIsGlobal;
        PoleBonusEffType = un.PoleBonusEffType;
        PoleBonusCnType = un.PoleBonusCnType;
        PoleBonusVal1 = un.PoleBonusVal1;
        PoleBonusVal2 = un.PoleBonusVal2;
    }
    public void Save()
    {
        GS.W.Write( IgnoreAttackTime );
        GS.W.Write( PoleBonusFailed );
        GS.W.Write( GlowingFish );
        GS.W.Write( MarkedFish );
        GS.W.Write( PoleBonusIsGlobal );
        GS.W.Write( ( int ) PoleBonusEffType );
        GS.W.Write( ( int ) PoleBonusCnType );
        GS.W.Write( PoleBonusVal1 );
        GS.W.Write( PoleBonusVal2 );
        GS.W.Write( PoleBonusActive );
    }

    public void Load()
    {
        IgnoreAttackTime = GS.R.ReadSingle();
        PoleBonusFailed = GS.R.ReadBoolean();
        GlowingFish = GS.R.ReadBoolean();
        MarkedFish = GS.R.ReadInt32();
        PoleBonusIsGlobal = GS.R.ReadBoolean();
        PoleBonusEffType = ( EPoleBonusEffType ) GS.R.ReadInt32();
        PoleBonusCnType = ( EPoleBonusCnType ) GS.R.ReadInt32();
        PoleBonusVal1 = GS.R.ReadSingle();
        PoleBonusVal2 = GS.R.ReadSingle();
        PoleBonusActive = GS.R.ReadBoolean();
    }
    public static void SaveGlobals()
    {
        GS.W.Write( PoleBonusGiven );
        GS.W.Write( G.HS.CatchEnabled );
        GS.W.Write( G.HS.ImpulsionateEnabled );
        GS.W.Write( G.HS.FishingExtraTime );
        GS.W.Write( G.HS.FishingExtraAttack );
        GS.W.Write( G.HS.HarpoonAttackBonus );
        GS.W.Write( G.HS.FogExtraAttack );
        GS.W.Write( G.HS.GreenFishExtraAttack );
        GS.W.Write( G.HS.YellowFishExtraAttack );
        GS.W.Write( G.HS.RedFishExtraAttack );
        GS.W.Write( G.HS.FishingExtraSpeed );
        GS.W.Write( G.HS.FishingExtraRadius );
        GS.W.Write( G.HS.FishingExtraPrize );
        GS.W.Write( G.HS.FishingReturnCatch );
        GS.W.Write( G.HS.FishAddPercent );
        GS.W.Write( G.HS.FishMultiplyPercent );
        GS.W.Write( G.HS.OrbStrikeCount );
        GS.W.Write( G.HS.NumAttackedFish );
        GS.W.Write( G.HS.AlgaeDestroy );
        GS.W.Write( G.HS.CountAlgaeDestroy );
        GS.W.Write( G.HS.RaftDestroy );
        GS.W.Write( G.HS.InventoryFishAddBonus );
        GS.W.Write( G.HS.InventoryFishMultiplyBonus );
        GS.W.Write( G.HS.InventoryAllAddBonus );
        GS.W.Write( G.HS.InventoryAllMultiplyBonus );
        GS.W.Write( G.HS.AddNextPoleBonusVal );
        GS.W.Write( G.HS.MultiplyNextPoleBonusVal );
        GS.W.Write( G.HS.GlowNextFishAmount );
        GS.W.Write( G.HS.LastGlowedFishMod );
        GS.W.Write( G.HS.SetFishPercent );
        GS.W.Write( G.HS.DoubleNextPerAmt );
        GS.W.Write( G.HS.ArrowDestroy );
        GS.W.Write( G.HS.ArrowPush );
        GS.W.Write( G.HS.FishIncreasePercent );
        GS.W.Write( G.HS.PoleBonusesConquered );
    }
    public static void LoadGlobals()
    {
        PoleBonusGiven = GS.R.ReadBoolean();
        G.HS.CatchEnabled = GS.R.ReadBoolean();
        G.HS.ImpulsionateEnabled = GS.R.ReadBoolean();
        G.HS.FishingExtraTime = GS.R.ReadSingle();
        G.HS.FishingExtraAttack = GS.R.ReadSingle();
        G.HS.HarpoonAttackBonus = GS.R.ReadSingle();
        G.HS.FogExtraAttack = GS.R.ReadSingle();
        G.HS.GreenFishExtraAttack = GS.R.ReadSingle();
        G.HS.YellowFishExtraAttack = GS.R.ReadSingle();
        G.HS.RedFishExtraAttack = GS.R.ReadSingle();
        G.HS.FishingExtraSpeed = GS.R.ReadSingle();
        G.HS.FishingExtraRadius = GS.R.ReadSingle();
        G.HS.FishingExtraPrize = GS.R.ReadInt32();
        G.HS.FishingReturnCatch = GS.R.ReadInt32();
        G.HS.FishAddPercent = GS.R.ReadSingle();
        G.HS.FishMultiplyPercent = GS.R.ReadSingle();
        G.HS.OrbStrikeCount = GS.R.ReadInt32();
        G.HS.NumAttackedFish = GS.R.ReadInt32();
        G.HS.AlgaeDestroy = GS.R.ReadBoolean();
        G.HS.CountAlgaeDestroy = GS.R.ReadInt32();
        G.HS.RaftDestroy = GS.R.ReadBoolean();
        G.HS.InventoryFishAddBonus = GS.R.ReadSingle();
        G.HS.InventoryFishMultiplyBonus = GS.R.ReadSingle();
        G.HS.InventoryAllAddBonus = GS.R.ReadSingle();
        G.HS.InventoryAllMultiplyBonus = GS.R.ReadSingle();
        G.HS.AddNextPoleBonusVal = GS.R.ReadSingle();
        G.HS.MultiplyNextPoleBonusVal = GS.R.ReadSingle();
        G.HS.GlowNextFishAmount = GS.R.ReadInt32();
        G.HS.LastGlowedFishMod = GS.R.ReadInt32();
        G.HS.SetFishPercent = GS.R.ReadSingle();
        G.HS.DoubleNextPerAmt = GS.R.ReadSingle();
        G.HS.ArrowDestroy = GS.R.ReadBoolean();
        G.HS.ArrowPush = GS.R.ReadBoolean();
        G.HS.FishIncreasePercent = GS.R.ReadSingle();
        G.HS.PoleBonusesConquered = GS.R.ReadInt32();
    }

    public void UpdatePoleText( bool force = false )
    {
        if( Map.I.CurrentFishingPole && Map.I.CurrentFishingPole == Unit ) 
            force = true;
        if( force == false )
        if( Map.I.TurnFrameCount != 3 ) return;                                         // Show dice image for randomizable pole       

        string txt = "";
        if( Unit.Control.PoleExtraLevel > 0 )
        {
            txt = "lv +" + Unit.Control.PoleExtraLevel;
        }

        switch( PoleBonusCnType )
        {
            case EPoleBonusCnType.THROW_HOOK:
            txt = "Throw the Hook";
            break;
            case EPoleBonusCnType.STEP_THE_POLE:
            txt = "Step the Pole";
            break;
            case EPoleBonusCnType.FINISH_FISHING:
            txt = "Finish Fishing";
            break;
            case EPoleBonusCnType.FIRST_ATTACK_ANY_HP:
            txt = "Target a Fish";
            break;
            case EPoleBonusCnType.FIRST_ATTACK_HP_HIGHER_THAN:
            txt = "Target a Fish Higher than " + PoleBonusVal2.ToString( "0.#" ) + "%";
            break;
            case EPoleBonusCnType.FIRST_ATTACK_HP_SMALLER_THAN:
            txt = "Target a Fish Smaller than " + PoleBonusVal2.ToString( "0.#" ) + "%";
            break;
            case EPoleBonusCnType.FIRST_ATTACK_VIRGIN:
            txt = "Target an Untouched Fish ";
            break;
            case EPoleBonusCnType.ATTACK_X_VIRGIN_FISH:
            int v = TempNumAttackedFish;
            if( PoleBonusIsGlobal )
                v = G.HS.NumAttackedFish;
            v = ( int ) PoleBonusVal2 - v;
            txt = "Attack " + v + " Fish For the First Time";
            break;        
            case EPoleBonusCnType.FIRST_ATTACK_AT_EXACT_HP:
            txt = "Target a Fish At " + PoleBonusVal2.ToString( "0.#" ) + "%";
            break;
            case EPoleBonusCnType.FIRST_ATTACK_AT_RANGE_HP:
            txt = "Target a Fish At " + PoleBonusVal2.ToString( "0.#" ) + "% to " +
            ( PoleBonusVal2 + Unit.Md.PoleBonusHPRange ).ToString( "0.#" ) + "%";
            break;
            case EPoleBonusCnType.HOOK_LEAVE_AT_EXACT_HP:
            txt = "Move Hook away from a Fish At " + PoleBonusVal2.ToString( "0.#" ) + "%";
            break;
            case EPoleBonusCnType.HOOK_LEAVE_AT_RANGE_HP:
            txt = "Move Hook away from a Fish At " + PoleBonusVal2.ToString( "0.#" ) + "% to " +
            ( PoleBonusVal2 + Unit.Md.PoleBonusHPRange ).ToString( "0.#" ) + "%";
            break;
            case EPoleBonusCnType.HOOK_LEAVE_AT_HP_HIGHER_THAN:
            txt = "Move Hook away from a Fish Higher than " + PoleBonusVal2.ToString( "0.#" ) + "%";
            break;
            case EPoleBonusCnType.HOOK_LEAVE_AT_HP_SMALLER_THAN:
            txt = "Move Hook away from a Fish Smaller than " + PoleBonusVal2.ToString( "0.#" ) + "%";
            break;
            case EPoleBonusCnType.MARK_FISH:
            txt = "Mark " + ( int ) PoleBonusVal2 + " Fish with +";
            break;
            case EPoleBonusCnType.HOOK_STRIKE_ORB:
            v = TempOrbStrikeCount;
            if( PoleBonusIsGlobal )
                v = G.HS.OrbStrikeCount;
            v = ( int ) PoleBonusVal2 - v;
            txt = "Hook Strike the Orb ";
            if( PoleBonusVal2 > 0 ) txt += "" + v + " more Time" + Util.GetPlural( v );
            break;
            case EPoleBonusCnType.DESTROY_X_ALGAE:
            v = TempCountAlgaeDestroy;
            if( PoleBonusIsGlobal )
                v = G.HS.CountAlgaeDestroy;
            v = ( int ) PoleBonusVal2 - v;
            txt = "Hook Destroy " + v.ToString( "+0;-#" ) + " Algae";
            break;
            case EPoleBonusCnType.SIMULTANEOUS_FISH_ATTACKED:
            txt = "Attack " + PoleBonusVal2 + " Fish Simultaneously (" + SimultaneousAttackedFish + ")";
            break;
            case EPoleBonusCnType.SIMULTANEOUS_FISH_CATCH:
            txt = "Catch " + PoleBonusVal2 + " Fish Simultaneously";
            break;
            case EPoleBonusCnType.PULL_ALGAE_X_METERS:
            float fv =  PoleBonusVal2 - TempAlgaePullDistance;
            if( fv < 0 ) fv = 0;
            txt = "Pull Algae for " + fv.ToString( "0.#" ) + " Meters";
            PoleBonusText = txt;
            break;
            case EPoleBonusCnType.MOVE_HOOK_X_METERS:
            fv = PoleBonusVal2 - TempHookMoveDistance;
            if( fv < 0 ) fv = 0;
            txt = "Move Hook Away from Fish for " + fv.ToString( "0.#" ) + " Meters";
            PoleBonusText = txt;
            break;
            case EPoleBonusCnType.HARPOON_ATTACK_X_PERCENT:
            fv = PoleBonusVal2 - TempHarpoonAttackPercent;
            if( fv < 0 ) fv = 0;
            txt = "Harpoon Attack Fish for " + fv.ToString( "0.#" ) + "%";
            PoleBonusText = txt;
            break;
            case EPoleBonusCnType.HAVE_X_FISH_IN_INVENTORY:
            txt = "Have " + PoleBonusVal2 + " Fish in the Inventory.";
            break;
            case EPoleBonusCnType.HOOK_REMAIN_AROUND_LAND:
            txt = "Keep Hook Around at least One Land Tile";
            break;
            case EPoleBonusCnType.MOVE_HOOK_X_METERS_FROM_POLE:
            txt = "Move Hook " + PoleBonusVal2 + " Meters Away from the Pole";
            PoleBonusText = "Pole Distance: " + TempHookDistanceFromPole.ToString( "0.0" ) + " m";
            break;
            case EPoleBonusCnType.CONQUER_X_HOOK_BONUS:
            fv = PoleBonusVal2 - G.HS.PoleBonusesConquered;
            if( fv < 0 ) fv = 0;
            txt = "Conquer +" + fv + " Pole Bonuses";
            break;
            case EPoleBonusCnType.FISH_FOR_X_SECONDS:
            fv = PoleBonusVal2 - FishingTime;
            if( fv < 0 ) fv = 0;
            txt = "Fish for " + fv.ToString( "0." ) + " Seconds.";
            break;
        }

        int showFishMod = -1;
        bool showItem = false;
        switch( PoleBonusEffType )
        {
            case EPoleBonusEffType.SPAWN_SIMILAR:
            txt += "\nto Spawn " + ( int ) PoleBonusVal1 + " Similar.";
            break;
            case EPoleBonusEffType.SPAWN_DEFAULT:
            txt += "\nto Spawn " + PoleBonusVal1.ToString( "+0;-#" ) + " Fish of this type:";
            showFishMod = ( int ) Unit.Md.DefaultFishMod;
            break;
            case EPoleBonusEffType.SPAWN_LAST_GLOWED:
            txt += "\nto Spawn Last Glowed Fish type x" + PoleBonusVal1;
            showFishMod = G.HS.LastGlowedFishMod;
            break;
            case EPoleBonusEffType.SPAWN_ALL_GLOWED:
            if( PoleBonusIsGlobal )
                txt += "\nto Spawn All Glowing Fish x" + PoleBonusVal1 + " in All Poles.";
            else
                txt += "\nto Spawn All Glowing Fish x" + PoleBonusVal1 + " in this Pole.";
            break;
            case EPoleBonusEffType.SPAWN_INVENTORY:
            txt += "\nto Spawn Inventory Fish x" + PoleBonusVal1;
            break;
            case EPoleBonusEffType.ADD_TIME:
            if( PoleBonusIsGlobal )
                txt += "\nAdds " + ( int ) PoleBonusVal1 + "s of Fishing Time for All Poles.";
            else
                txt += "\nAdds " + ( int ) PoleBonusVal1 + "s of Fishing Time for This Pole.";
            break;

            case EPoleBonusEffType.GIVE_PRIZE:
            txt += "\nto Get " + PoleBonusVal1.ToString( "+0;-#" ) + " Resource of this type:";
            showItem = true;
            break;

            case EPoleBonusEffType.EXTRA_FISH_CATCH:
            if( PoleBonusIsGlobal )
                txt += "\nto Get +" + ( int ) PoleBonusVal1 + " Extra Catch for All Poles.";
            else
                txt += "\nto Get +" + ( int ) PoleBonusVal1 + " Extra Catch for This Pole.";
            break;
            case EPoleBonusEffType.RETURN_CATCH:
            if( PoleBonusIsGlobal )
                txt += "\nto Return +" + ( int ) PoleBonusVal1 + " Catch Back to Lake in All Poles.";
            else
                txt += "\nto Return +" + ( int ) PoleBonusVal1 + " Catch Back to Lake in This Pole.";
            break;
            case EPoleBonusEffType.ADD_POWER:
            if( PoleBonusIsGlobal )
                txt += "\nAdds " + ( int ) PoleBonusVal1 + "% of Power for All Poles.";
            else
                txt += "\nAdds " + ( int ) PoleBonusVal1 + "% of Power for This Pole.";
            break;
            case EPoleBonusEffType.HARPOON_ATTACK_BONUS:
            if( PoleBonusIsGlobal )
                txt += "\nHarpoon Attack Bonus " + PoleBonusVal1.ToString( "+0;-#" ) + "% for All Poles.";
            else
                txt += "\nHarpoon Attack Bonus " + PoleBonusVal1.ToString( "+0;-#" ) + "% for this Pole.";
            break;
            case EPoleBonusEffType.ADD_GREEN_ATT_POWER:
            if( PoleBonusIsGlobal )
                txt += "\nGreen Fish Attack " + PoleBonusVal1.ToString( "+0;-#" ) + "% for All Poles.";
            else
                txt += "\nGreen Fish Attack " + PoleBonusVal1.ToString( "+0;-#" ) + "% for this Pole.";
            break;
            case EPoleBonusEffType.ADD_YELLOW_ATT_POWER:
            if( PoleBonusIsGlobal )
                txt += "\nYellow Fish Attack " + PoleBonusVal1.ToString( "+0;-#" ) + "% for All Poles.";
            else
                txt += "\nYellow Fish Attack " + PoleBonusVal1.ToString( "+0;-#" ) + "% for this Pole.";
            break;
            case EPoleBonusEffType.ADD_RED_ATT_POWER:
            if( PoleBonusIsGlobal )
                txt += "\nRed Fish Attack " + PoleBonusVal1.ToString( "+0;-#" ) + "% for All Poles.";
            else
                txt += "\nRed Fish Attack " + PoleBonusVal1.ToString( "+0;-#" ) + "% for this Pole.";
            break;
            case EPoleBonusEffType.ADD_SPEED:
            if( PoleBonusIsGlobal )
                txt += "\nAdds " + ( int ) PoleBonusVal1 + "% of Speed for All Poles.";
            else
                txt += "\nAdds " + ( int ) PoleBonusVal1 + "% of Speed for This Pole.";
            break;
            case EPoleBonusEffType.ADD_RADIUS:
            if( PoleBonusIsGlobal )
                txt += "\nAdds " + ( int ) PoleBonusVal1 + "% of Hook Radius for All Poles.";
            else
                txt += "\nAdds " + ( int ) PoleBonusVal1 + "% of Hook Radius for This Pole.";
            break;
            case EPoleBonusEffType.SET_FISH_HP:
            txt += "\nChanges All Attacked Fish Percent to " + ( int ) PoleBonusVal1 + "%";
            break;
            case EPoleBonusEffType.GLOW_FISH:
            if( PoleBonusIsGlobal )
                txt += "\nAdds Glow to next Attacked Fish in All Poles (x" + ( int ) PoleBonusVal1 + ")";
            else
                txt += "\nAdds Glow to next Attacked Fish in This Pole (x" + ( int ) PoleBonusVal1 + ")";
            break;
            case EPoleBonusEffType.ADD_FISH_PERCENT:
            if( PoleBonusIsGlobal )
                txt += "\nAdds " + ( int ) PoleBonusVal1 + "% to All Fish Percent for All Poles.";
            else
                txt += "\nAdds " + ( int ) PoleBonusVal1 + "% to All Fish Percent for This Pole.";
            break;
            case EPoleBonusEffType.MULTIPLY_FISH_PERCENT:
            if( PoleBonusIsGlobal )
                txt += "\nMultiply All Fish Percent by " + ( int ) PoleBonusVal1 + " in All Poles.";
            else
                txt += "\nMultiply All Fish Percent by " + ( int ) PoleBonusVal1 + " in this Pole.";
            break;
            case EPoleBonusEffType.INCREASE_FISH_PERCENT:
            if( PoleBonusIsGlobal )
                txt += "\nAll Fish Percent +" + ( int ) PoleBonusVal1 + "% in All Poles.";
            else
                txt += "\nAll Fish Percent +" + ( int ) PoleBonusVal1 + "% in this Pole.";
            break;
            case EPoleBonusEffType.ENABLE_ALGAE_DESTROY:
            if( PoleBonusIsGlobal )
                txt += "\nHook can Destroy Algae in All Poles.";
            else
                txt += "\nHook can Destroy Algae in This Pole.";
            break;
            case EPoleBonusEffType.HOOK_DESTROY_ARROWS:
            if( PoleBonusIsGlobal )
                txt += "\nHook can Destroy Arrows in All Poles.";
            else
                txt += "\nHook can Destroy Arrows in This Pole.";
            break;
            case EPoleBonusEffType.HOOK_PUSH_ARROWS:
            if( PoleBonusIsGlobal )
                txt += "\nHook can Push Arrows in All Poles.";
            else
                txt += "\nHook can Push Arrows in This Pole.";
            break;
            case EPoleBonusEffType.ENABLE_RAFT_DESTROY:
            if( PoleBonusIsGlobal )
                txt += "\nHook can Destroy Rafts in All Poles.";
            else
                txt += "\nHook can Destroy Rafts in This Pole.";
            break;
            case EPoleBonusEffType.INVENTORY_FISH_ADD:
            if( PoleBonusIsGlobal )
                txt += "\nInventory Fish " + PoleBonusVal1.ToString( "+0;-#" ) + " for All Poles";
            else
                txt += "\nInventory Fish " + PoleBonusVal1.ToString( "+0;-#" ) + " for this Pole";
            break;
            case EPoleBonusEffType.INVENTORY_FISH_MULTIPLY:
            if( PoleBonusIsGlobal )
                txt += "\nInventory Fish X" + PoleBonusVal1 + " for All Poles";
            else
                txt += "\nInventory Fish X" + PoleBonusVal1 + " for this Pole";
            break;
            case EPoleBonusEffType.INVENTORY_ALL_ADD:
            if( PoleBonusIsGlobal )
                txt += "\nAll Inventory Items " + PoleBonusVal1.ToString( "+0;-#" ) + " for All Poles";
            else
                txt += "\nAll Inventory Items " + PoleBonusVal1.ToString( "+0;-#" ) + " for this Pole";
            break;
            case EPoleBonusEffType.INVENTORY_ALL_MULTIPLY:
            if( PoleBonusIsGlobal )
                txt += "\nAll Inventory Items X" + PoleBonusVal1 + " for All Poles";
            else
                txt += "\nAll Inventory Items X" + PoleBonusVal1 + " for this Pole";
            break;
            case EPoleBonusEffType.DISABLE_CATCH:
            if( PoleBonusIsGlobal )
                txt += "\nDisables Fish Catching in All Poles.";
            else
                txt += "\nDisables Fish Catching in This Pole.";
            break;
            case EPoleBonusEffType.ENABLE_CATCH:
            if( PoleBonusIsGlobal )
                txt += "\nEnables Fish Catching in All Poles.";
            else
                txt += "\nEnables Fish Catching in This Pole.";
            break;
            case EPoleBonusEffType.IMPULSIONATE_RAFT:
            if( PoleBonusIsGlobal )
                txt += "\nHook can Impulsionate Rafts in All Poles.";
            else
                txt += "\nHook can Impulsionate Rafts in This Pole.";
            break;
            case EPoleBonusEffType.ADD_TO_NEXT_POLE_BONUS:
            txt += "\nAdd " + PoleBonusVal1.ToString( "+0;-#" ) + " to Next Pole Bonus Power.";
            break;
            case EPoleBonusEffType.MULTIPLY_NEXT_POLE_BONUS:
            txt += "\nMultiply Next Pole Bonus Power by " + PoleBonusVal1.ToString( "0.#" );
            break;
            case EPoleBonusEffType.ATTACKED_FISH_PERCENT_DOUBLED:
            if( PoleBonusIsGlobal )
            {
                txt += "\nDoubles next " + PoleBonusVal1 + " Attacked Fish % in All Poles.";
                PoleBonusEffText = "\nDoubles next fish % +" + G.HS.DoubleNextPerAmt;
            }
            else
            {
                txt += "\nDoubles next " + PoleBonusVal1 + " Attacked Fish % in this Pole.";
                PoleBonusEffText = "Doubles next fish % +" + TempDoubleNextPerAmt;
            }
            break;
            case EPoleBonusEffType.FOG_FISH_ATTACK_BONUS:
            if( PoleBonusIsGlobal )
            {
                txt += "\nFish under Fog: +" + PoleBonusVal1 + "% Attack in All Poles.";       
            }
            else
            {
                txt += "\nFish under Fog: +" + PoleBonusVal1 + "% Attack in this Pole.";
            }
            break;
            case EPoleBonusEffType.GIVE_X_SMALL_HOOK:
            if( PoleBonusIsGlobal )
            {
                txt += "\nGives Extra Small Hook x" + PoleBonusVal1 + " in All Poles.";       
            }
            else
            {
                txt += "\nGives Extra Small Hook x" + PoleBonusVal1 + " in this Pole.";
            }
            break;
            case EPoleBonusEffType.GIVE_X_MEDIUM_HOOK:
            if( PoleBonusIsGlobal )
            {
                txt += "\nGives Extra Medium Hook x" + PoleBonusVal1 + " in All Poles.";       
            }
            else
            {
                txt += "\nGives Extra Medium Hook x" + PoleBonusVal1 + " in this Pole.";
            }
            break;
            case EPoleBonusEffType.GIVE_X_LARGE_HOOK:
            if( PoleBonusIsGlobal )
            {
                txt += "\nGives Extra Large Hook x" + PoleBonusVal1 + " in All Poles.";       
            }
            else
            {
                txt += "\nGives Extra Large Hook x" + PoleBonusVal1 + " in this Pole.";
            }
            break;
            case EPoleBonusEffType.SWITCH_MERGING:
            if( PoleBonusActive == false )
            {
                txt += "\n-Hook Merging" + Util.GetEnabled( G.HS.HookMergeEnabled ) + "-";  
            }
            else
            if( G.HS.HookMergeEnabled )
            {
                txt += "\nDisables Hook Merging.";       
            }
            else
            {
                txt += "\nEnables Hook Merging";
            }
            break;
        }

        Unit.LevelTxt.gameObject.SetActive( false );

        if( txt != "" )
        {
            Unit.LevelTxt.gameObject.SetActive( true );                                                          // Shows bonus text
            Unit.LevelTxt.text = txt;
        }

        Unit.Body.Sprite4.gameObject.SetActive( false );                                                        // Global glow effect
        if( PoleBonusIsGlobal && PoleBonusEffType != EPoleBonusEffType.NONE )
            Unit.Body.Sprite4.gameObject.SetActive( true );
        float a = 1, a2 = .65f;
        if( Unit.Activated == false )                                                                           // inactive pole sprite color
        {
            a = .25f; a2 = .25f;
            Unit.Body.Sprite2.gameObject.SetActive( false );
        }
        if( Map.I.CurrentFishingPole == Unit )                                                                  // disables current fishing pole sprite while fishing
        if( Map.I.FishingMode != EFishingPhase.NO_FISHING ) 
          { a = 0; a2 = 0; }

        Unit.Spr.color = new Color( 1, 1, 1, a );
        Unit.Body.Sprite4.color = new Color( 1, 1, 1, a2 );
        if( Map.I.CurrentFishingPole != Unit )
        {
            PoleBonusText = "";
            PoleBonusEffText = "";
        }

        Unit.LevelTxt.color = Color.white;                                                                       // Bonus text color
        if( PoleBonusActive == false )
            Unit.LevelTxt.color = new Color( 0, 1, 0, 1 );
        if( PoleBonusFailed )
            Unit.LevelTxt.color = new Color( 1, 0, 0, 1 );
                                                     
        if( showFishMod != -1 )
        {
            Unit.Body.Sprite3.gameObject.SetActive( true );                                                      // Show default fish sprite
            Water w = Unit.Water;
            int id = -1;
            for( int i = 0; i < Map.I.RM.SD.ModList.Length; i++ )                                                // spawn default used The MOD_1 , not the id, so the id needs to be found in the list
            {
                if( ( int ) Map.I.RM.SD.ModList[ i ].ModNumber == showFishMod ) 
                    id = i;              
            }
            if( PoleBonusEffType == EPoleBonusEffType.SPAWN_LAST_GLOWED ) 
                id = showFishMod;                                                                                // these ones use just the real mod id
            EFishType tp = Map.I.RM.SD.ModList[ id ].WaterObjectType;                                            // Finds sprite id         
            int res = ( int ) Map.I.GetFishResID( tp );
            Unit.Body.Sprite3.spriteId = G.GIT( res ).TKSprite.spriteId;
        }
        if( showItem )
        {
            Unit.Body.Sprite3.gameObject.SetActive( true );                                                      // Show default prize sprite
            Water w = Unit.Water;
            Unit.Body.Sprite3.spriteId = Manager.I.Inventory.
            ItemList[ ( int ) w.DefaultItemPrize ].TKSprite.spriteId;
        }
    }
    public static bool CheckCn( EPoleBonusCnType PoleBonusCnType, Unit un )
    {
        if( PoleBonusGiven ) return false;
        if( Map.I.CurrentFishingPole == null ) return false;
        Water w = Map.I.CurrentFishingPole.Water;
        if( w.PoleBonusCnType != PoleBonusCnType ) return false;
        bool res = false;
        bool upd = false;
        bool fail = false;
        switch( PoleBonusCnType )
        {
            case EPoleBonusCnType.THROW_HOOK:
            res = true;
            break;
            case EPoleBonusCnType.STEP_THE_POLE:
            res = true;
            break;
            case EPoleBonusCnType.FINISH_FISHING:
            res = true;
            break;
            case EPoleBonusCnType.FIRST_ATTACK_VIRGIN:
            if( un.Body.Hp == 0 ) res = true;
            else fail = true;
            break;
            case EPoleBonusCnType.ATTACK_X_VIRGIN_FISH:
            if( un.Body.Hp == 0 ) 
            {
                TempNumAttackedFish++;
                G.HS.NumAttackedFish++;
                upd = true;
            }
            if( w.PoleBonusIsGlobal )
            {
                if( G.HS.NumAttackedFish == ( int ) w.PoleBonusVal2 )
                    res = true;
            }
            else
                if( TempNumAttackedFish == ( int ) w.PoleBonusVal2 )
                    res = true;            
            break;
            case EPoleBonusCnType.FIRST_ATTACK_ANY_HP:
            res = true;
            break;
            case EPoleBonusCnType.FIRST_ATTACK_HP_HIGHER_THAN:
            if( un.Body.Hp >= w.PoleBonusVal2 )
                res = true;
            else fail = true;
            break;
            case EPoleBonusCnType.FIRST_ATTACK_HP_SMALLER_THAN:
            if( un.Body.Hp <= w.PoleBonusVal2 )
                res = true;
            else fail = true;
            break;
            case EPoleBonusCnType.FIRST_ATTACK_AT_EXACT_HP:
            if( ( int ) un.Body.Hp == ( int ) w.PoleBonusVal2 ) 
                res = true;
            else fail = true;
            break;
            case EPoleBonusCnType.HOOK_LEAVE_AT_EXACT_HP:
            if( ( int ) un.Body.Hp == ( int ) w.PoleBonusVal2 ) 
                res = true;
            else fail = true;
            break;
            case EPoleBonusCnType.HOOK_LEAVE_AT_HP_HIGHER_THAN:
            if( ( int ) un.Body.Hp > ( int ) w.PoleBonusVal2 )
                res = true;
            else fail = true;
            break;
            case EPoleBonusCnType.HOOK_LEAVE_AT_HP_SMALLER_THAN:
            if( ( int ) un.Body.Hp < ( int ) w.PoleBonusVal2 )
                res = true;
            else fail = true;
            break;
            case EPoleBonusCnType.HOOK_LEAVE_AT_RANGE_HP:
            if( un.Body.Hp >= w.PoleBonusVal2 &&
                un.Body.Hp <= ( w.PoleBonusVal2 + w.Unit.Md.PoleBonusHPRange ) ) 
                res = true;
            else fail = true;
            break;
            case EPoleBonusCnType.FIRST_ATTACK_AT_RANGE_HP:
            if( un.Body.Hp >= w.PoleBonusVal2 &&
                un.Body.Hp <= ( w.PoleBonusVal2 + w.Unit.Md.PoleBonusHPRange ) ) 
                res = true;
            else fail = true;
            break;
            case EPoleBonusCnType.MARK_FISH:
            TempMarkingEnabled = true;
            if( MarkedFishCount >= w.PoleBonusVal2 ) 
                res = true;
            break;
            case EPoleBonusCnType.HOOK_STRIKE_ORB:
            G.HS.OrbStrikeCount++;
            TempOrbStrikeCount++;
            upd = true;
            if( w.PoleBonusVal2 > 0 )
            {
                if( w.PoleBonusIsGlobal )
                {
                    if( G.HS.OrbStrikeCount == ( int ) w.PoleBonusVal2 )
                        res = true;
                }
                else
                if( TempOrbStrikeCount == ( int ) w.PoleBonusVal2 ) 
                    res = true;
            }
            else
                res = true;
            break;
            case EPoleBonusCnType.SIMULTANEOUS_FISH_ATTACKED:
            upd = true;
            if( SimultaneousAttackedFish >= ( int ) w.PoleBonusVal2 )
                res = true;
            break;
            case EPoleBonusCnType.SIMULTANEOUS_FISH_CATCH:
            upd = true;
            if( SimultaneousCatchFish >= ( int ) w.PoleBonusVal2 )
                res = true;
            break;  
            case EPoleBonusCnType.DESTROY_X_ALGAE:
            G.HS.CountAlgaeDestroy++;
            TempCountAlgaeDestroy++;
            upd = true;
            if( w.PoleBonusIsGlobal )
            {
                if( G.HS.CountAlgaeDestroy == ( int ) w.PoleBonusVal2 )
                    res = true;
            }
            else
            if( TempCountAlgaeDestroy == ( int ) w.PoleBonusVal2 )
                res = true;   
            break;
            case EPoleBonusCnType.PULL_ALGAE_X_METERS:
            upd = true;
            if( TempAlgaePullDistance >= ( int ) w.PoleBonusVal2 )
                res = true;   
            break;
            case EPoleBonusCnType.MOVE_HOOK_X_METERS:
            upd = true;
            if( TempHookMoveDistance >= ( int ) w.PoleBonusVal2 )
                res = true;   
            break;
            case EPoleBonusCnType.HARPOON_ATTACK_X_PERCENT:
            upd = true;
            if( TempHarpoonAttackPercent >= ( int ) w.PoleBonusVal2 )
                res = true;   
            break;
            case EPoleBonusCnType.HAVE_X_FISH_IN_INVENTORY:
            if( Water.CountFishInInventory() >= ( int ) w.PoleBonusVal2 )
                res = true;
            break;
            case EPoleBonusCnType.HOOK_REMAIN_AROUND_LAND:
            if( Water.TempHookAroundLandTile == true )
                res = true;
            else
                fail = true;
            break;
            case EPoleBonusCnType.MOVE_HOOK_X_METERS_FROM_POLE:
            upd = true;
            if( TempHookDistanceFromPole >= ( int ) w.PoleBonusVal2 )
                res = true;
            break;
            case EPoleBonusCnType.CONQUER_X_HOOK_BONUS:
            upd = true;
            if( G.HS.PoleBonusesConquered >= ( int ) w.PoleBonusVal2 )
                res = true;
            break;
            case EPoleBonusCnType.FISH_FOR_X_SECONDS:
            upd = true;
            if( FishingTime >= ( int ) w.PoleBonusVal2 )
                res = true;
            break;
        }
        if( res )
            PoleBonusGiven = true;                                                          // Condition Met: Mark bonus given bool as true

        if( res || fail || PoleBonusGiven )
            w.PoleBonusActive = false;                                                      // Is this bonus available?

        if( res ) ApplyPoleBonusEffect( un );                                               // Apply Bonus effect

        if( fail )
        {
            SetBonusFailed();                                                               // Bonus failed
        }

        w.UpdatePoleText( upd );                                                            // Updates Pole text

        return res;
    }
    public static void ApplyPoleBonusEffect( Unit un )
    {
        Water w = Map.I.CurrentFishingPole.Water;
        Sector hs = Map.I.RM.HeroSector;
        string msg = "";
        Vector2 hkp = Map.I.MainHook.transform.position;
        if( Map.I.GetUnit( ETileType.WATER, hkp ) == null )
            hkp = HookDropPosition;

        switch( w.PoleBonusEffType )
        {
            case EPoleBonusEffType.SPAWN_SIMILAR:
            SpawnFishByMod( un, ( int ) w.PoleBonusVal1, un.ModID, hkp );
            break;

            case EPoleBonusEffType.SPAWN_LAST_GLOWED:
            if( G.HS.LastGlowedFishMod == -1 ) 
            {
                ErrorMsg = "No Glowing Fish Found!";
                SetBonusFailed();
                break;
            }
            SpawnFishByMod( un, ( int ) w.PoleBonusVal1, ( int ) G.HS.LastGlowedFishMod, hkp );
            break;
            case EPoleBonusEffType.SPAWN_INVENTORY:
            int count = 0;
            for( int i = 0; i < Map.I.RM.HeroSector.CatchModList.Count; i++ )
            {
                int mod = Map.I.RM.HeroSector.CatchModList[ i ];
                EFishType fish = Map.I.RM.SD.ModList[ mod ].WaterObjectType;
                ItemType it = Map.I.GetFishResID( fish );
                float amt = Item.GetNum( it );
                amt *= w.PoleBonusVal1;
                SpawnFishByMod( un, ( int ) amt, mod, hkp );
                count += ( int ) amt;
            }
            if( count <= 0 )
            {
                ErrorMsg = "No Inventory Fish Found!";
                SetBonusFailed();
            }
            break;
            case EPoleBonusEffType.SPAWN_ALL_GLOWED:
            count = 0;
            for( int u = hs.Fly.Count - 1; u >= 0; u-- )
            if( hs.Fly[ u ].TileID == ETileType.FISH )
            if( hs.Fly[ u ].Water.GlowingFish )
            {
                Unit fl = hs.Fly[ u ];
                SpawnFishByMod( un, ( int ) w.PoleBonusVal1, fl.ModID, hkp );
                count++;
            }
            if( count == 0 )
            {
                ErrorMsg = "No Glowing Fish Found!";
                SetBonusFailed();
            }
            break;
            case EPoleBonusEffType.SPAWN_DEFAULT:
            int id = -1;
            for( int i = 0; i < Map.I.RM.SD.ModList.Length; i++ )
            {
                if( Map.I.RM.SD.ModList[ i ].ModNumber == w.Unit.Md.DefaultFishMod )
                    id = i;
            }
            SpawnFishByMod( un, ( int ) w.PoleBonusVal1, id, hkp );
            break;
            case EPoleBonusEffType.GIVE_PRIZE:
            Item.AddItem( w.DefaultItemPrize, w.PoleBonusVal1 );
            break;
            case EPoleBonusEffType.GLOW_FISH:
            if( w.PoleBonusIsGlobal )
                G.HS.GlowNextFishAmount += ( int ) w.PoleBonusVal1;
            else
                TempGlowNextFishAmount += ( int ) w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.EXTRA_FISH_CATCH:
            if( w.PoleBonusIsGlobal )
                G.HS.FishingExtraPrize += ( int ) w.PoleBonusVal1;
            else
                TempFishingExtraPrize += ( int ) w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.RETURN_CATCH:
            if( w.PoleBonusIsGlobal )
                G.HS.FishingReturnCatch += ( int ) w.PoleBonusVal1;
            else
                TempFishingReturnCatch += ( int ) w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.ADD_POWER:
            if( w.PoleBonusIsGlobal )
                G.HS.FishingExtraAttack += w.PoleBonusVal1;
            else
                TempFishingExtraAttack += w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.HARPOON_ATTACK_BONUS:
            if( w.PoleBonusIsGlobal )
                G.HS.HarpoonAttackBonus += w.PoleBonusVal1;
            else
                TempHarpoonAttackBonus += w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.ADD_GREEN_ATT_POWER:
            if( w.PoleBonusIsGlobal )
                G.HS.GreenFishExtraAttack += w.PoleBonusVal1;
            else
                TempGreenFishExtraAttack += w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.ADD_YELLOW_ATT_POWER:
            if( w.PoleBonusIsGlobal )
                G.HS.YellowFishExtraAttack += w.PoleBonusVal1;
            else
                TempYellowFishExtraAttack += w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.ADD_RED_ATT_POWER:
            if( w.PoleBonusIsGlobal )
                G.HS.RedFishExtraAttack += w.PoleBonusVal1;
            else
                TempRedFishExtraAttack += w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.ADD_SPEED:
            if( w.PoleBonusIsGlobal )
                G.HS.FishingExtraSpeed += w.PoleBonusVal1;
            else
                TempFishingExtraSpeed += w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.ADD_RADIUS:
            if( w.PoleBonusIsGlobal )
                G.HS.FishingExtraRadius += w.PoleBonusVal1;
            else
                TempFishingExtraRadius += w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.ADD_TIME:
            if( w.PoleBonusIsGlobal )
            {
                TimeToAdd = w.PoleBonusVal1;
                G.HS.FishingExtraTime += w.PoleBonusVal1;
            }
            else
                TimeToAdd = w.PoleBonusVal1;
            msg = w.PoleBonusVal1.ToString( "+0;-#" ) + "s";
            break;
            case EPoleBonusEffType.SET_FISH_HP:
            if( w.PoleBonusIsGlobal )
            {
                G.HS.SetFishPercent = w.PoleBonusVal1;
            }
            else
                TempSetFishPercent = w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.ADD_FISH_PERCENT:
            if( w.PoleBonusIsGlobal )
            {
                G.HS.FishAddPercent += w.PoleBonusVal1;
            }
            else
                TempFishAddPercent += w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.MULTIPLY_FISH_PERCENT:
            if( w.PoleBonusIsGlobal )
            {
                G.HS.FishMultiplyPercent += w.PoleBonusVal1;
            }
            else
                TempFishMultiplyPercent += w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.INCREASE_FISH_PERCENT:
            if( w.PoleBonusIsGlobal )
            {
                G.HS.FishIncreasePercent += w.PoleBonusVal1;
            }
            else
                TempFishIncreasePercent += w.PoleBonusVal1;
            break;

            case EPoleBonusEffType.ENABLE_ALGAE_DESTROY:
            TempAlgaeDestroy = true;
            if( w.PoleBonusIsGlobal )
                G.HS.AlgaeDestroy = true;
            break;
            case EPoleBonusEffType.HOOK_DESTROY_ARROWS:
            TempArrowDestroy = true;
            if( w.PoleBonusIsGlobal )
                G.HS.ArrowDestroy = true;
            break;
            case EPoleBonusEffType.HOOK_PUSH_ARROWS:
            TempArrowPush = true;
            if( w.PoleBonusIsGlobal )
                G.HS.ArrowPush = true;
            break;
            case EPoleBonusEffType.ENABLE_RAFT_DESTROY:
            TempRaftDestroy = true;
            if( w.PoleBonusIsGlobal )
                G.HS.RaftDestroy = true;
            break;
            case EPoleBonusEffType.INVENTORY_FISH_ADD:
            float val = w.PoleBonusVal1;
            if( w.PoleBonusIsGlobal )
            {
                G.HS.InventoryFishAddBonus += w.PoleBonusVal1;
                val = G.HS.InventoryFishAddBonus;
            }
            GiveInventoryBonus( val, true, false );
            break;
            case EPoleBonusEffType.INVENTORY_FISH_MULTIPLY:     
            val = w.PoleBonusVal1;
            if( w.PoleBonusIsGlobal )
            {
                G.HS.InventoryFishMultiplyBonus += w.PoleBonusVal1;
                val = G.HS.InventoryFishMultiplyBonus;
            }
            GiveInventoryBonus( val, false, false );
            break;
            case EPoleBonusEffType.INVENTORY_ALL_ADD:
            val = w.PoleBonusVal1;
            if( w.PoleBonusIsGlobal )
            {
                G.HS.InventoryAllAddBonus += w.PoleBonusVal1;
                val = G.HS.InventoryAllAddBonus;
            }
            GiveInventoryBonus( val, true, true );
            break;
            case EPoleBonusEffType.INVENTORY_ALL_MULTIPLY:
            val = w.PoleBonusVal1;
            if( w.PoleBonusIsGlobal )
            {
                G.HS.InventoryAllMultiplyBonus += w.PoleBonusVal1;
                val = G.HS.InventoryAllMultiplyBonus;
            }
            GiveInventoryBonus( val, false, true );
            break;
            case EPoleBonusEffType.DISABLE_CATCH:
            TempCatchEnabled = false;
            if( w.PoleBonusIsGlobal )
                G.HS.CatchEnabled = false;
            break;
            case EPoleBonusEffType.ENABLE_CATCH:
            TempCatchEnabled = true;
            if( w.PoleBonusIsGlobal )
                G.HS.CatchEnabled = true;
            break;
            case EPoleBonusEffType.IMPULSIONATE_RAFT:
            TempImpulsionateEnabled = true;
            if( w.PoleBonusIsGlobal )
                G.HS.ImpulsionateEnabled = true;
            break;
            case EPoleBonusEffType.ADD_TO_NEXT_POLE_BONUS:
            G.HS.AddNextPoleBonusVal = w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.MULTIPLY_NEXT_POLE_BONUS:
            G.HS.MultiplyNextPoleBonusVal = w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.ATTACKED_FISH_PERCENT_DOUBLED:
            if( w.PoleBonusIsGlobal )
            {
                G.HS.DoubleNextPerAmt = w.PoleBonusVal1;
            }
            else
                TempDoubleNextPerAmt = w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.FOG_FISH_ATTACK_BONUS:
            if( w.PoleBonusIsGlobal )
                G.HS.FogExtraAttack += w.PoleBonusVal1;
            else
                TempFogExtraAttack += w.PoleBonusVal1;
            break;
            case EPoleBonusEffType.GIVE_X_SMALL_HOOK:
            if( w.PoleBonusIsGlobal )
            {
                G.HS.ExtraSmallHook += ( int ) w.PoleBonusVal1;
                CreateExtraHook( G.HS.ExtraSmallHook, 3, HookDropPosition );
                ExtraHooksGiven = true;
            }
            else
                CreateExtraHook( ( int ) w.PoleBonusVal1, 3, HookDropPosition );
            break;
            case EPoleBonusEffType.GIVE_X_MEDIUM_HOOK:
            if( w.PoleBonusIsGlobal )
            {
                G.HS.ExtraMediumHook += ( int ) w.PoleBonusVal1;
                CreateExtraHook( G.HS.ExtraMediumHook, 2, HookDropPosition );
                ExtraHooksGiven = true;
            }
            else
                CreateExtraHook( ( int ) w.PoleBonusVal1, 2, HookDropPosition );
            break;
            case EPoleBonusEffType.GIVE_X_LARGE_HOOK:
            if( w.PoleBonusIsGlobal )
            {
                G.HS.ExtraLargeHook += ( int ) w.PoleBonusVal1;
                CreateExtraHook( G.HS.ExtraLargeHook, 1, HookDropPosition );
                ExtraHooksGiven = true;
            }
            else
                CreateExtraHook( ( int ) w.PoleBonusVal1, 1, HookDropPosition );
            break;
            case EPoleBonusEffType.SWITCH_MERGING:
            G.HS.HookMergeEnabled = !G.HS.HookMergeEnabled;
            break;   
        }
        w.Unit.LevelTxt.color = Color.green;                                                    // Turn text green
        MasterAudio.PlaySound3DAtVector3( "Click 2", G.Hero.Pos );                              // Sound FX
        G.HS.PoleBonusesConquered++;

        if( msg != "" )
        {
            Vector3 tg = new Vector3( Map.I.MainHook.transform.position.x,
            Map.I.MainHook.transform.position.y, -5 );
            Message.CreateMessage( ETileType.NONE, msg, tg, Color.white );                      // message
        }
    }

    public static void SpawnFishByMod( Unit un, int amt, int mod, Vector2 tg )
    {
        if( mod == -1 ) return;
        if( amt < 1 ) return;
        Unit pf = Map.I.GetUnitPrefab( ETileType.FISH );
        pf.Body.FishType = Map.I.RM.SD.ModList[ mod ].WaterObjectType;
        for( int i = 0; i < amt; i++ )
        {
            if( tg.x == -1 ) tg = Map.I.MainHook.transform.position;
            if( un != null ) tg = un.transform.position;
            Map.I.RM.ModedUnitlList = new List<Unit>();
            Unit fish = SpawnFish( pf, tg );
            Map.I.RM.SD.ModList[ mod ].ApplyMod( ( int ) fish.Pos.x, ( int ) fish.Pos.y, mod, fish );
            fish.Body.StackAmount = 0;
        }
    }

    public static void GiveInventoryBonus( float amt, bool add, bool all )
    {
        for( int i = 0; i < Manager.I.Inventory.ItemList.Count; i++ )                           // Warning: Dont use G.GIT here: the access is directly to the itemlist via loop and id    
        {
            Item it = Manager.I.Inventory.ItemList[ i ]; 
            if( it )
            if( it.IsGameplayResource )
            if( it.Count > 0 )
            if( all == true || IsResourceFish( it.Type ) == true )
            {
                if( add )
                    Item.AddItem( it.Type, amt );                                               // add bonus
                else
                    Item.SetAmt( it.Type, ( it.Count * amt ) );                                // multiply bonus
            }
        }
    }
    public static bool IsResourceFish( ItemType t )
    {
        if( t == ItemType.Res_Fish_Yellow    || t == ItemType.Res_Fish_Red         ||
            t == ItemType.Res_Fish_Blue      || t == ItemType.Res_Fish_Crab        ||
            t == ItemType.Res_Fish_Manta_Ray || t == ItemType.Res_Fish_Water_Snake ||
            t == ItemType.Res_Fish_Brown     || t == ItemType.Res_Fish_Frog        )
            return true;
        return false;
    }
    public static Unit SpawnFish( Unit un, Vector3 tg )
    {
        Unit prefabUnit = Map.I.GetUnitPrefab( ETileType.FISH );                                // Spawns fish
        Unit fu = Map.I.SpawnFlyingUnit( tg, ELayerType.MONSTER, ETileType.FISH, null );
        fu.Copy( un, false, false, false );
        fu.Md = un.Md;
        fu.transform.position = new Vector3( tg.x, tg.y, un.transform.position.z );
        fu.transform.rotation = un.transform.rotation;
        fu.Body.Hp = 0;
        fu.Body.FishCaught = false;
        fu.Control.FlyingTarget = new Vector2( -1, -1 );
        fu.Water.IgnoreAttackTime = 2;
        fu.Water.GlowingFish = false;
        Map.I.CreateFish( fu );
        fu.Body.FishType = un.Body.FishType;
        Map.I.InitFishGraphics( fu );
        return fu;
    }
    public static void ResetVars( Sector s = null )
    {
        ErrorMsg = "";
        PoleBonusText = "";
        PoleBonusEffText = "";
        PoleBonusGiven = false;
        TempPoleAttackedFishList = new List<Unit>();
        TempMarkingEnabled = false;
        FishingTime = 0;

        TempImpulsionateEnabled = false;
        TempFishingExtraAttack = 0;
        TempHarpoonAttackBonus = 0;
        TempFogExtraAttack = 0;
        TempGreenFishExtraAttack = 0;
        TempYellowFishExtraAttack = 0;
        TempRedFishExtraAttack = 0;
        TempFishingExtraSpeed = 0;
        TempFishingExtraRadius = 0;
        TempFishingExtraPrize = 0;
        TempFishingReturnCatch = 0;
        TempFishAddPercent = 0;
        TempFishMultiplyPercent = 0;
        TempFishIncreasePercent = 0;
        TempOrbStrikeCount = 0;
        TempNumAttackedFish = 0;
        TempAlgaeDestroy = false;
        TempCountAlgaeDestroy = 0;
        TempRaftDestroy = false;
        TempAlgaePullDistance = 0;
        TempHookMoveDistance = 0;
        TempHarpoonAttackPercent = 0;
        TempSetFishPercent = -1;
        TempDoubleNextPerAmt = 0;
        TempArrowDestroy = false;
        TempArrowPush = false;
        TempHookAroundLandTile = true;
        TempHookDistanceFromPole = 0;
        TimeToAdd = 0;

        if( s )
        {
            ForceUpdatePoleText = false;
            s.HookMergeEnabled = false;
            s.FishingHookExtraLevel = 0;
            s.FishingExtraTime = 0;
            s.FishingExtraAttack = 0;
            s.HarpoonAttackBonus = 0;
            s.FogExtraAttack = 0;
            s.GreenFishExtraAttack = 0;
            s.YellowFishExtraAttack = 0;
            s.RedFishExtraAttack = 0;
            s.FishingExtraSpeed = 0;
            s.FishingExtraRadius = 0;
            s.FishingExtraPrize = 0;
            s.FishingReturnCatch = 0;
            s.FishAddPercent = 0;
            s.FishMultiplyPercent = 0;
            s.FishIncreasePercent = 0;
            s.MarkingEnabled = false;
            s.CatchEnabled = true;
            s.ImpulsionateEnabled = false;
            s.OrbStrikeCount = 0;
            s.NumAttackedFish = 0;
            s.AlgaeDestroy = false;
            s.CountAlgaeDestroy = 0;
            s.RaftDestroy = false;
            s.InventoryFishAddBonus = 0;
            s.InventoryFishMultiplyBonus = 0;
            s.InventoryAllAddBonus = 0;
            s.InventoryAllMultiplyBonus = 0;
            s.AddNextPoleBonusVal = 0;
            s.MultiplyNextPoleBonusVal = 0;
            s.GlowNextFishAmount = 0;
            s.LastGlowedFishMod = -1;
            s.SetFishPercent = -1;
            s.DoubleNextPerAmt = 0;
            s.ArrowDestroy = false;
            s.ArrowPush = false;
            s.PoleBonusesConquered = 0;
            s.ExtraSmallHook = 0;
            s.ExtraMediumHook = 0;
            s.ExtraLargeHook = 0;
        }
        HeroLeftHook = true;
    }
    public static bool DestroyRaft( Unit raft )
    {
        if( raft == null ) return false;
        FallingAnimation.Create( 75, 1, new Vector3( raft.Pos.x, raft.Pos.y, -0.33f ),
        raft.transform.eulerAngles, .1f );
        MasterAudio.PlaySound3DAtVector3( "Wood Break", raft.transform.position );              // FX
        if( G.Hero.Pos == raft.Pos ) Map.I.StartCubeDeath();
        Map.I.CreateExplosionFX( raft.Pos );
        Unit orb = Map.I.GetUnit( ETileType.ORB, raft.Pos );
        if( orb ) orb.Kill();
        Map.Kill( raft );
        return true;
    }
    public static void UpdateOneTimePerHookBonusCheck( FishingObject hk )
    {
        Water w = Map.I.CurrentFishingPole.Water;
        int land = 0;
        if( w.PoleBonusCnType == EPoleBonusCnType.HOOK_REMAIN_AROUND_LAND )
        {
            for( int dr = 0; dr < 8; dr++ )                                                                   // see if hook is around land tiles
            {
                Vector2 tg = hk.TilePos + Manager.I.U.DirCord[ ( int ) dr ];
                if( Map.I.GetUnit( ETileType.WATER, tg ) == null ) land++;
                if( Controller.GetRaft( tg ) != null ) land++;
            }
            if( land <= 0 )
            {
                TempHookAroundLandTile = false;
                Water.CheckCn( EPoleBonusCnType.HOOK_REMAIN_AROUND_LAND, null );                              // Keep hook around land tile hook bonus check
            }
        }
        TempHookDistanceFromPole = Vector3.Distance( w.Unit.Pos, hk.transform.position );
        Water.CheckCn( EPoleBonusCnType.MOVE_HOOK_X_METERS_FROM_POLE, null );                                 // move hook x meters from pole bonus check
    }
    public static void UpdateOneTimePerFrameBonusCheck()
    {
        if( TimeToAdd != 0 )
        if( Map.I.FishingMode == EFishingPhase.FISHING )
            {
                Map.I.FishingTimerCount += TimeToAdd;                                                         // adds time just here to avoid increasing time while peparing bait
                TimeToAdd = 0;
            }
        
        if( ExtraHooksGiven == false )
        {
            if( G.HS.ExtraLargeHook > 0 )
                CreateExtraHook( G.HS.ExtraLargeHook, 1, HookDropPosition );                                 // gives global hooks
            if( G.HS.ExtraMediumHook > 0 )
                CreateExtraHook( G.HS.ExtraMediumHook, 2, HookDropPosition );
            if( G.HS.ExtraSmallHook > 0 )
                CreateExtraHook( G.HS.ExtraSmallHook, 3, HookDropPosition );
            ExtraHooksGiven = true;
        }
        Water.CheckCn( EPoleBonusCnType.FISH_FOR_X_SECONDS, null );                                          // Condition: Fish for X seconds
    }

    public static void UpdatePoleBonusOnFishAttacked( Unit un )
    {
        Water w = Map.I.CurrentFishingPole.Water;
        Water.SimultaneousAttackedFish++;
        Water.CheckCn( EPoleBonusCnType.FIRST_ATTACK_ANY_HP, un );                              // Condition: first Attack Any HP
        Water.CheckCn( EPoleBonusCnType.FIRST_ATTACK_HP_HIGHER_THAN, un );                      // Condition: first Attack HP higher than
        Water.CheckCn( EPoleBonusCnType.FIRST_ATTACK_HP_SMALLER_THAN, un );                     // Condition: first Attack Smaller than
        Water.CheckCn( EPoleBonusCnType.FIRST_ATTACK_AT_EXACT_HP, un );                         // Condition: first Attack Exact HP
        Water.CheckCn( EPoleBonusCnType.FIRST_ATTACK_AT_RANGE_HP, un );                         // Condition: first Attack HP in Range
        Water.CheckCn( EPoleBonusCnType.MARK_FISH, un );                                        // Condition: mark fish

        if( w.PoleBonusCnType == EPoleBonusCnType.MOVE_HOOK_X_METERS )                          // hook move x meters failed for touching a fish            
            SetBonusFailed();

        if( Water.TempGlowNextFishAmount >= 1 )
         if( un.Water.GlowingFish == false )
            {
                un.Water.GlowingFish = true;
                un.Body.Sprite3.gameObject.SetActive( true );                                   // non global glowing fish activation
                Water.TempGlowNextFishAmount--;
                G.HS.LastGlowedFishMod = un.ModID;
                MasterAudio.PlaySound3DAtVector3( "Click 2", G.Hero.Pos );                      // Sound FX
                ForceUpdatePoleText = true;
            }

        if( G.HS.GlowNextFishAmount >= 1 )
        if( un.Water.GlowingFish == false )
            {
                un.Water.GlowingFish = true;
                un.Body.Sprite3.gameObject.SetActive( true );                                  // global glowing fish activation
                G.HS.GlowNextFishAmount--;
                G.HS.LastGlowedFishMod = un.ModID;
                MasterAudio.PlaySound3DAtVector3( "Click 2", G.Hero.Pos );                     // Sound FX
                ForceUpdatePoleText = true;
            }

        if( TempSetFishPercent != -1 ) un.Body.Hp = TempSetFishPercent;                        // set fish percent              
        if( G.HS.SetFishPercent != -1 ) un.Body.Hp = G.HS.SetFishPercent;

        if( TempPoleAttackedFishList.Contains( un ) == false )                                 // Fish attackedin the pole fishing session
        {
            TempPoleAttackedFishList.Add( un );
            if( G.HS.DoubleNextPerAmt > 0 )
            {
                un.Body.Hp *= 2;                                                               // Double fish HP pole bonus Global
                G.HS.DoubleNextPerAmt--;
                ForceUpdatePoleText = true;
                Message.GreenMessage( "Doubled!",  un.Pos );
            }
            else
            if( TempDoubleNextPerAmt > 0 )
            {
                un.Body.Hp *= 2;                                                               // Double fish HP pole bonus
                TempDoubleNextPerAmt--;
                ForceUpdatePoleText = true;
                Message.GreenMessage( "Doubled!", un.Pos );
            }
        }
    }
    public static void SetBonusFailed()
    {
        Water w = Map.I.CurrentFishingPole.Water;
        if( w.PoleBonusFailed ) return;
        if( PoleBonusGiven ) return;
        w.PoleBonusFailed = true;
        PoleBonusGiven = true;
        w.PoleBonusActive = false;
        w.Unit.LevelTxt.color = Color.red;                                              // turn text red if failed
        if( ErrorMsg != "" )
            Message.RedMessage( ErrorMsg );                                             // Error Message
        ErrorMsg = "";
        MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );                      // Error Sound FX
    }

    public static void UpdateFishingMessages()
    {
        if( Map.I.TurnFrameCount != 3 ) return;
        string msg = "";
        if( G.HS.AddNextPoleBonusVal != 0 )
            msg = "Step a pole to Add " + G.HS.AddNextPoleBonusVal.ToString( "+0;-#" ) + " to it's Bonus Effect Power.\n";           // Add to next pole msg
        if( G.HS.MultiplyNextPoleBonusVal != 0 )
            msg += "Step a Pole to Multiply its Bonus Effect Power by " + G.HS.MultiplyNextPoleBonusVal + "\n";                      // Multiply to next pole msg            
        if( G.HS.GlowNextFishAmount > 0 )
            msg += "Attack next fish to make it Glow. (x" + G.HS.GlowNextFishAmount + ")\n";                                         // Glow fish msg
        if( G.HS.DoubleNextPerAmt > 0 )
            msg += "Attack next " + G.HS.DoubleNextPerAmt + " Fish to Double its %\n";                                               // Double fish % msg        
        if( msg != "" )
            UI.I.SetBigMessage( msg, Color.yellow, 60, 4.7f, 122.8f, 85, .001f );                                                     // Displays msg
    }
    public static int CountFishInInventory()
    {
        int count = 0;
        for( int i = 0; i < Manager.I.Inventory.ItemList.Count; i++ )                                                                 // Warning: Dont use G.GIT here: the access is directly to the itemlist via loop and id    
        {
            Item it = Manager.I.Inventory.ItemList[ i ];
            if( it && it.IsGameplayResource )
            if( it.Count > 0 )
            if( IsResourceFish( it.Type ) == true )
                count += ( int ) it.Count;
        }
        return count;
    }
    public static bool CreateExtraHook( int num, int type, Vector2 pos )
    {
        for( int h = 0; h < num; h++ )
        {
            FishingObject fo = FishingObject.Add( new Vector2( -1, -1 ), type, pos );
            if( fo == null )
            {
                Message.RedMessage( "Max Number of \nHooks Reached!" );
                return false;
            }
        }
        return true;
    }
}
