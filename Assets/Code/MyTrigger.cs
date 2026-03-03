using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using Unity.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// --- ENUMS SECTION ---

public enum ETriggerEffOperator
{
    NONE = 0, EQUALS, PLUS, MINUS, MULTIPLY, DIVIDE
}

public enum ETriggerCondOperator
{
    NONE = 0, EQUALS, MINOR, MAJOR, MINOR_OR_EQUAL, MAJOR_OR_EQUAL
}

public enum ETriggerVarID
{
    NONE = 0, UNIT_LEVEL, UNIT_STARS, UNIT_TOTALHP, UNIT_HP, UNIT_LIVES,
    UNIT_BONUSMELEEATTACK, UNIT_BONUSRANGEDATTACK, UNIT_RANGEDACCURACY, UNIT_RANGEDRANGE, UNIT_RANGEDPENETRATION,
    UNIT_MELEE_SHIELD, UNIT_RANGED_SHIELD, UNIT_MAGIC_SHIELD,
    UNIT_MELEEATTACKLEVEL, UNIT_RANGEDATTACKLEVEL, UNIT_MAGICATTACKENABLED, UNIT_MOVEMENTLEVEL, UNIT_ARROWWALKINGLEVEL,
    UNIT_DEXTERITYLEVEL, UNIT_VOID, UNIT_ORBSTRIKERLEVEL, UNIT_MONSTERCORNERINGLEVEL, UNIT_COOPERATIONLEVEL, UNIT_DAMAGESURPLUSLEVEL,
    UNIT_MELEESHIELDLEVEL, UNIT_MISSILESHIELDLEVEL, UNIT_MAGICSHIELDLEVEL, UNIT_MONSTERPUSHLEVEL, UNIT_SCOUTLEVEL, UNIT_NUMBEROFHORSES,
    UNIT_PLATFORMWALKINGLEVEL, UNIT_AMBUSHERLEVEL, UNIT_WALLDESTROYERLEVEL, UNIT_MEMORYLEVEL, UNIT_TOOLBOXLEVEL, UNIT_CARCASS,
    UNIT_SPRINTERLEVEL, UNIT_FIREMASTERLEVEL, UNIT_BERSERKLEVEL, UNIT_RICOCHETLEVEL, UNIT_BEEHIVETHROWERLEVEL, UNIT_PSYCHICLEVEL, UNIT_SNEAKINGLEVEL,
    ROACHDEATHCOUNT, SCARABDEATHCOUNT, AREASCLEARED, NORMALSECTORSDISCOVERED, SECTORSCLEARED, PERFECTAREAS, PERFECTSECTORS, ACCUMULATEDPOINTS, BONFIRESLIT,
    DIRTYBONFIRESLIT, UNIT_LOOTERLEVEL, UNIT_PROSPECTORLEVEL, POISONERDEATHCOUNT, MAXBONUSREACHED, ACCUMULATEDBONUS, UNIT_BARRICADE_FIGHTER_LEVEL,
    MONSTERSDEATHCOUNT, UNIT_EVASIONLEVEL, UNIT_PERFECTIONISTLEVEL, UNIT_SCAVENGERLEVEL, UNIT_AGILITYLEVEL, UNIT_ARROWFIGHTERLEVEL, UNIT_ARROWINLEVEL,
    UNIT_ARROWOUTLEVEL, UNIT_FRESHATTACKLEVEL, UNIT_RISKYATTACKLEVEL, UNIT_MORTALJUMPLEVEL, UNIT_OPENFIELDATTACKLEVEL,
    UNIT_INTELLIGENCELEVEL, UNIT_INTELTHREATLEVEL, UNIT_INTEL2LEVEL, UNIT_INTEL3LEVEL, UNIT_INTEL4LEVEL, UNIT_BASETHREATDURATION, UNIT_BASEFREEEXITHPLIMIT,
    UNIT_FIREPOWERLEVEL, UNIT_FIRESPREADLEVEL, UNIT_FIREWOODNEEDED, UNIT_OUTSIDEFIREWOODALLOWED, VOID, UNIT_BARRICADEDESTROYLEVEL,
    UNIT_FREEPLATFORMEXIT, UNIT_OUTAREABURNINGBARRICADEDESTROYBONUS, UNIT_OVERBARRICADESCOUT, UNIT_SHOWRESOURCECHANCE, UNIT_SHOWRESOURCENEIGHBORSCHANCE,
    UNIT_BARRICADEFORRUNE, UNIT_SCARYLEVEL, UNIT_HERONEIGHBORTOUCHADDER, UNIT_FIRESTARBONNUS, UNIT_COLLECTORLEVEL, UNIT_RESOURCEPERSISTANCE,
    UNIT_RTMELEEATTACKSPEED, UNIT_RTRANGEDATTACKSPEED, UNIT_MIRELEVEL, UNIT_RESTDISTANCE, UNIT_SLAYERLEVEL, UNIT_DRAGONTARGETTING, UNIT_SLAYERANGLE,
    UNIT_SLAYERMAXHP, UNIT_DRAGONDISGUISE, UNIT_DRAGONBONUSDROP, UNIT_DRAGONBARRICADEPROTECTION, UNIT_PLATFORMSTEPS, UNIT_RESOURCECOLLECTED, UNIT_MININGLEVEL,
    UNIT_FISHING_LEVEL, UNIT_FISHING_1, UNIT_FISHING_2, UNIT_FISHING_3, UNIT_FISHING_4,
    UNIT_FISHINGBONUSREACHED, UNIT_CONQUEREDGOALS, TOTAL_VALS
}

// --- CLASS SECTION ---

public class MyTrigger: MonoBehaviour
{
    #region Variables
    [TabGroup( "Condition" )] public ETriggerVarID ConditionVarID;
    [TabGroup( "Condition" )] public ETriggerCondOperator ConditionOperator;
    [TabGroup( "Condition" )] public float ConditionVal1;

    [TabGroup( "Effect" )] public ETriggerVarID EffectVarID;
    [TabGroup( "Effect" )] public ETriggerEffOperator EffectOperator;
    [TabGroup( "Effect" )] public ETriggerVarID EffectVarID1;
    [TabGroup( "Effect" )] public float EffectVal1;
    [TabGroup( "Effect" )] public ETriggerEffOperator EffectOperator2;
    [TabGroup( "Effect" )] public ETriggerVarID EffectVarID2;
    [TabGroup( "Effect" )] public float EffectVal2;
    [TabGroup( "Effect" )] public ETriggerEffOperator EffectOperator3;
    [TabGroup( "Effect" )] public ETriggerVarID EffectVarID3;
    [TabGroup( "Effect" )] public float EffectVal3;

    [TabGroup( "Link" )] public Unit Unit;
    [TabGroup( "Link" )] public float[] VariableList;                    // Local buffer pre-allocated;
    #endregion

    public void Copy( MyTrigger tr )
    {
        ConditionVarID = tr.ConditionVarID;                              // Copy logic;
        ConditionOperator = tr.ConditionOperator;
        ConditionVal1 = tr.ConditionVal1;
        EffectVarID = tr.EffectVarID;
        EffectOperator = tr.EffectOperator;
        EffectVarID1 = tr.EffectVarID1;
        EffectVal1 = tr.EffectVal1;
        EffectOperator2 = tr.EffectOperator2;
        EffectVarID2 = tr.EffectVarID2;
        EffectVal2 = tr.EffectVal2;
        EffectOperator3 = tr.EffectOperator3;
        EffectVarID3 = tr.EffectVarID3;
        EffectVal3 = tr.EffectVal3;
    }

    public bool UpdateIt( bool force = false )
    {
        if( Unit == null ) return false;                                  // Safety check;

        EnsureBufferExists();                                            // Pre-allocate VariableList once;
        SyncData( Unit, true );                                          // Pull data from unit;

        bool success = false;
        if( CheckConditionOperation( force ) )
        {
            success = true;
            DoEffectOperation();                                         // Run calculations;
            SyncData( Unit, false );                                     // Push data back to unit;
        }
        return success;
    }
    public float GetVarAmount( Unit un )
    {
        if( un == null ) return 0;                                       // Safety check;
        EnsureBufferExists();                                            // Pre - allocate buffer;
        SyncData( un, true );                                            // Pull data from unit;
        return VariableList[ (int) ConditionVarID ];                     // Return requested value;
    }

    private void EnsureBufferExists()
    {
        int size = (int)ETriggerVarID.TOTAL_VALS;                        // Get total enum count;
        if( VariableList == null || VariableList.Length != size )
            VariableList = new float[ size ];                              // Initial allocation only;
    }

    public bool CheckConditionOperation( bool force = false )
    {
        if( G.HS == null ) return false;                                 // Global safety;

        var mapI = Map.I;                                                // Cache Map instance;
        if( !force )
        {
            if( mapI.RM.HeroSector.Type != Sector.ESectorType.GATES ||
                mapI.TurnFrameCount != 2 ) return false;                 // Gate condition;
        }

        float vval = VariableList[ ( int ) ConditionVarID ];             // Get current var value;

        switch( ConditionOperator )                                     // Optimized branch logic;
        {
        case ETriggerCondOperator.NONE: return true;
        case ETriggerCondOperator.EQUALS: return vval == ConditionVal1;
        case ETriggerCondOperator.MAJOR: return vval > ConditionVal1;
        case ETriggerCondOperator.MINOR: return vval < ConditionVal1;
        case ETriggerCondOperator.MAJOR_OR_EQUAL: return vval >= ConditionVal1;
        case ETriggerCondOperator.MINOR_OR_EQUAL: return vval <= ConditionVal1;
        default: return false;
        }
    }

    bool DoEffectOperation()
    {
        float v1 = (EffectVarID1 != ETriggerVarID.NONE) ? VariableList[(int)EffectVarID1] : EffectVal1;
        float v2 = (EffectVarID2 != ETriggerVarID.NONE) ? VariableList[(int)EffectVarID2] : EffectVal2;
        float v3 = (EffectVarID3 != ETriggerVarID.NONE) ? VariableList[(int)EffectVarID3] : EffectVal3;

        float res = v1;                                                  // Base start;

        if( EffectOperator2 != ETriggerEffOperator.NONE )
        {
            switch( EffectOperator2 )                                     // First calculation;
            {
            case ETriggerEffOperator.PLUS: res = v1 + v2; break;
            case ETriggerEffOperator.MINUS: res = v1 - v2; break;
            case ETriggerEffOperator.MULTIPLY: res = v1 * v2; break;
            case ETriggerEffOperator.DIVIDE: res = ( v2 != 0 ) ? v1 / v2 : v1; break;
            }
        }

        if( EffectOperator3 != ETriggerEffOperator.NONE )
        {
            switch( EffectOperator3 )                                     // Second calculation;
            {
            case ETriggerEffOperator.PLUS: res += v3; break;
            case ETriggerEffOperator.MINUS: res -= v3; break;
            case ETriggerEffOperator.MULTIPLY: res *= v3; break;
            case ETriggerEffOperator.DIVIDE: if( v3 != 0 ) res /= v3; break;
            }
        }

        int targetID = (int)EffectVarID;                                 // Final target index;
        switch( EffectOperator )
        {
        case ETriggerEffOperator.EQUALS: VariableList[ targetID ] = res; return true;
        case ETriggerEffOperator.PLUS: VariableList[ targetID ] += res; return true;
        case ETriggerEffOperator.MINUS: VariableList[ targetID ] -= res; return true;
        case ETriggerEffOperator.MULTIPLY: VariableList[ targetID ] *= res; return true;
        case ETriggerEffOperator.DIVIDE: if( res != 0 ) VariableList[ targetID ] /= res; return true;
        default: return false;
        }
    }

    public void SyncData( Unit un, bool pull )
    {
        var b = un.Body;                                                 // Local component cache;
        var c = un.Control;                                              // Local component cache;
        var m = un.MeleeAttack;                                          // Local component cache;
        var r = un.RangedAttack;                                         // Local component cache;
        var s = Map.I.LevelStats;                                        // Local component cache;

        if( pull )
        {
            // PULL DATA FROM UNIT TO BUFFER;
            VariableList[ (int) ETriggerVarID.UNIT_TOTALHP ] = b.TotHp;
            VariableList[ (int) ETriggerVarID.UNIT_HP ] = b.Hp;
            VariableList[ (int) ETriggerVarID.UNIT_STARS ] = b.Stars;
            VariableList[ (int) ETriggerVarID.UNIT_LIVES ] = b.Lives;
            VariableList[ (int) ETriggerVarID.UNIT_BONUSMELEEATTACK ] = m.BonusDamage;
            VariableList[ (int) ETriggerVarID.UNIT_BONUSRANGEDATTACK ] = r.BonusDamage;
            VariableList[ (int) ETriggerVarID.UNIT_RANGEDRANGE ] = r.BaseRange;
            VariableList[ (int) ETriggerVarID.UNIT_MELEE_SHIELD ] = b.BonusMeleeShield;
            VariableList[ (int) ETriggerVarID.UNIT_RANGED_SHIELD ] = b.BonusMissileShield;
            VariableList[ (int) ETriggerVarID.UNIT_MAGIC_SHIELD ] = b.BonusMagicShield;
            VariableList[ (int) ETriggerVarID.UNIT_MELEEATTACKLEVEL ] = b.MeleeAttackLevel;
            VariableList[ (int) ETriggerVarID.UNIT_RANGEDATTACKLEVEL ] = b.RangedAttackLevel;
            VariableList[ (int) ETriggerVarID.UNIT_MOVEMENTLEVEL ] = c.MovementLevel;
            VariableList[ (int) ETriggerVarID.UNIT_ARROWWALKINGLEVEL ] = c.ArrowWalkingLevel;
            VariableList[ (int) ETriggerVarID.UNIT_DEXTERITYLEVEL ] = b.DexterityLevel;
            VariableList[ (int) ETriggerVarID.UNIT_ORBSTRIKERLEVEL ] = b.OrbStrikerLevel;
            VariableList[ (int) ETriggerVarID.UNIT_MONSTERCORNERINGLEVEL ] = c.MonsterCorneringLevel;
            VariableList[ (int) ETriggerVarID.UNIT_COOPERATIONLEVEL ] = b.CooperationLevel;
            VariableList[ (int) ETriggerVarID.UNIT_DAMAGESURPLUSLEVEL ] = b.DamageSurplusLevel;
            VariableList[ (int) ETriggerVarID.UNIT_MELEESHIELDLEVEL ] = b.MeleeShieldLevel;
            VariableList[ (int) ETriggerVarID.UNIT_MISSILESHIELDLEVEL ] = b.MissileShieldLevel;
            VariableList[ (int) ETriggerVarID.UNIT_MAGICSHIELDLEVEL ] = b.MagicShieldLevel;
            VariableList[ (int) ETriggerVarID.UNIT_MONSTERPUSHLEVEL ] = c.MonsterPushLevel;
            VariableList[ (int) ETriggerVarID.UNIT_SCOUTLEVEL ] = c.ScoutLevel;
            VariableList[ (int) ETriggerVarID.UNIT_PLATFORMWALKINGLEVEL ] = c.PlatformWalkingLevel;
            VariableList[ (int) ETriggerVarID.UNIT_PLATFORMSTEPS ] = c.PlatformSteps;
            VariableList[ (int) ETriggerVarID.UNIT_WALLDESTROYERLEVEL ] = b.WallDestroyerLevel;
            VariableList[ (int) ETriggerVarID.UNIT_AMBUSHERLEVEL ] = b.AmbusherLevel;
            VariableList[ (int) ETriggerVarID.UNIT_MEMORYLEVEL ] = b.MemoryLevel;
            VariableList[ (int) ETriggerVarID.UNIT_TOOLBOXLEVEL ] = b.ToolBoxLevel;
            VariableList[ (int) ETriggerVarID.UNIT_SPRINTERLEVEL ] = c.SprinterLevel;
            VariableList[ (int) ETriggerVarID.UNIT_FIREMASTERLEVEL ] = b.FireMasterLevel;
            VariableList[ (int) ETriggerVarID.UNIT_BERSERKLEVEL ] = b.BerserkLevel;
            VariableList[ (int) ETriggerVarID.UNIT_RICOCHETLEVEL ] = r.RicochetLevel;
            VariableList[ (int) ETriggerVarID.UNIT_BEEHIVETHROWERLEVEL ] = b.BeeHiveThrowerLevel;
            VariableList[ (int) ETriggerVarID.UNIT_PSYCHICLEVEL ] = b.PsychicLevel;
            VariableList[ (int) ETriggerVarID.UNIT_SNEAKINGLEVEL ] = c.SneakingLevel;
            VariableList[ (int) ETriggerVarID.MAXBONUSREACHED ] = s.MaxBonusReached;
            VariableList[ (int) ETriggerVarID.ACCUMULATEDBONUS ] = s.AccumulatedBonuses;
            VariableList[ (int) ETriggerVarID.AREASCLEARED ] = s.AreasCleared;
            VariableList[ (int) ETriggerVarID.NORMALSECTORSDISCOVERED ] = s.NormalSectorsDiscovered;
            VariableList[ (int) ETriggerVarID.SECTORSCLEARED ] = s.SectorsCleared;
            VariableList[ (int) ETriggerVarID.PERFECTAREAS ] = s.NumPerfectAreas;
            VariableList[ (int) ETriggerVarID.PERFECTSECTORS ] = s.NumPerfectSectors;
            VariableList[ (int) ETriggerVarID.ACCUMULATEDPOINTS ] = s.AccumulatedPoints;
            VariableList[ (int) ETriggerVarID.BONFIRESLIT ] = s.BonfiresLit;
            VariableList[ (int) ETriggerVarID.DIRTYBONFIRESLIT ] = s.DirtyBonfiresLit;
            VariableList[ (int) ETriggerVarID.UNIT_LOOTERLEVEL ] = b.LooterLevel;
            VariableList[ (int) ETriggerVarID.UNIT_PROSPECTORLEVEL ] = b.ProspectorLevel;
            VariableList[ (int) ETriggerVarID.UNIT_BARRICADE_FIGHTER_LEVEL ] = c.BarricadeFighterLevel;
            VariableList[ (int) ETriggerVarID.UNIT_EVASIONLEVEL ] = c.EvasionLevel;
            VariableList[ (int) ETriggerVarID.UNIT_PERFECTIONISTLEVEL ] = c.PerfectionistLevel;
            VariableList[ (int) ETriggerVarID.UNIT_SCAVENGERLEVEL ] = c.ScavengerLevel;
            VariableList[ (int) ETriggerVarID.UNIT_AGILITYLEVEL ] = b.AgilityLevel;
            VariableList[ (int) ETriggerVarID.UNIT_ARROWFIGHTERLEVEL ] = c.ArrowFighterLevel;
            VariableList[ (int) ETriggerVarID.UNIT_ARROWINLEVEL ] = c.ArrowInLevel;
            VariableList[ (int) ETriggerVarID.UNIT_ARROWOUTLEVEL ] = c.ArrowOutLevel;
            VariableList[ (int) ETriggerVarID.UNIT_FRESHATTACKLEVEL ] = b.FreshAttackLevel;
            VariableList[ (int) ETriggerVarID.UNIT_RISKYATTACKLEVEL ] = b.RiskyAttackLevel;
            VariableList[ (int) ETriggerVarID.UNIT_MORTALJUMPLEVEL ] = b.MortalJumpLevel;
            VariableList[ (int) ETriggerVarID.UNIT_OPENFIELDATTACKLEVEL ] = b.OpenFieldAtttackLevel;
            VariableList[ (int) ETriggerVarID.UNIT_BASETHREATDURATION ] = b.BaseThreatDuration;
            VariableList[ (int) ETriggerVarID.UNIT_BASEFREEEXITHPLIMIT ] = b.BaseFreeExitHPLimit;
            VariableList[ (int) ETriggerVarID.UNIT_FIREPOWERLEVEL ] = b.FirePowerLevel;
            VariableList[ (int) ETriggerVarID.UNIT_FIRESPREADLEVEL ] = b.FireSpreadLevel;
            VariableList[ (int) ETriggerVarID.UNIT_FIREWOODNEEDED ] = b.FireWoodNeeded;
            VariableList[ (int) ETriggerVarID.UNIT_OUTSIDEFIREWOODALLOWED ] = b.OutsideFireWoodAllowed;
            VariableList[ (int) ETriggerVarID.UNIT_BARRICADEDESTROYLEVEL ] = b.DestroyBarricadeLevel;
            VariableList[ (int) ETriggerVarID.UNIT_FREEPLATFORMEXIT ] = b.FreePlatformExit;
            VariableList[ (int) ETriggerVarID.UNIT_OUTAREABURNINGBARRICADEDESTROYBONUS ] = b.OutAreaBurningBarricadeDestroyBonus;
            VariableList[ (int) ETriggerVarID.UNIT_OVERBARRICADESCOUT ] = c.OverBarricadeScoutLevel;
            VariableList[ (int) ETriggerVarID.UNIT_SHOWRESOURCECHANCE ] = c.ShowResourceChance;
            VariableList[ (int) ETriggerVarID.UNIT_SHOWRESOURCENEIGHBORSCHANCE ] = c.ShowResourceNeighborsChance;
            VariableList[ (int) ETriggerVarID.UNIT_BARRICADEFORRUNE ] = b.BarricadeForRune;
            VariableList[ (int) ETriggerVarID.UNIT_SCARYLEVEL ] = b.ScaryLevel;
            VariableList[ (int) ETriggerVarID.UNIT_HERONEIGHBORTOUCHADDER ] = b.HeroNeighborTouchAdder;
            VariableList[ (int) ETriggerVarID.UNIT_FIRESTARBONNUS ] = b.FireStarBonus;
            VariableList[ (int) ETriggerVarID.UNIT_COLLECTORLEVEL ] = b.CollectorLevel;
            VariableList[ (int) ETriggerVarID.UNIT_RESOURCEPERSISTANCE ] = b.ResourcePersistance;
            VariableList[ (int) ETriggerVarID.UNIT_RTMELEEATTACKSPEED ] = b.RealtimeMeleeAttSpeed;
            VariableList[ (int) ETriggerVarID.UNIT_RTRANGEDATTACKSPEED ] = b.RealtimeRangedAttSpeed;
            VariableList[ (int) ETriggerVarID.UNIT_MIRELEVEL ] = c.MireLevel;
            VariableList[ (int) ETriggerVarID.UNIT_RESTDISTANCE ] = c.RestingLevel;
            VariableList[ (int) ETriggerVarID.UNIT_SLAYERLEVEL ] = c.SlayerLevel;
            VariableList[ (int) ETriggerVarID.UNIT_DRAGONTARGETTING ] = c.FlyingTargetting;
            VariableList[ (int) ETriggerVarID.UNIT_SLAYERANGLE ] = c.SlayerAngle;
            VariableList[ (int) ETriggerVarID.UNIT_SLAYERMAXHP ] = c.SlayerMaxHP;
            VariableList[ (int) ETriggerVarID.UNIT_DRAGONDISGUISE ] = c.DragonDisguiseLevel;
            VariableList[ (int) ETriggerVarID.UNIT_DRAGONBONUSDROP ] = c.DragonBonusDropLevel;
            VariableList[ (int) ETriggerVarID.UNIT_DRAGONBARRICADEPROTECTION ] = c.DragonBarricadeProtection;
            VariableList[ (int) ETriggerVarID.UNIT_MININGLEVEL ] = b.MiningLevel;
            VariableList[ (int) ETriggerVarID.UNIT_FISHING_LEVEL ] = b.FishingLevel;
        }
        else
        {
            // PUSH CHANGES FROM BUFFER TO UNIT COMPONENTS;
            b.TotHp = VariableList[ (int) ETriggerVarID.UNIT_TOTALHP ];
            b.Hp = VariableList[ (int) ETriggerVarID.UNIT_HP ];
            b.Stars = VariableList[ (int) ETriggerVarID.UNIT_STARS ];
            b.Lives = VariableList[ (int) ETriggerVarID.UNIT_LIVES ];
            m.BonusDamage = VariableList[ (int) ETriggerVarID.UNIT_BONUSMELEEATTACK ];
            r.BonusDamage = VariableList[ (int) ETriggerVarID.UNIT_BONUSRANGEDATTACK ];
            r.BaseRange = VariableList[ (int) ETriggerVarID.UNIT_RANGEDRANGE ];
            b.BonusMeleeShield = VariableList[ (int) ETriggerVarID.UNIT_MELEE_SHIELD ];
            b.BonusMissileShield = VariableList[ (int) ETriggerVarID.UNIT_RANGED_SHIELD ];
            b.BonusMagicShield = VariableList[ (int) ETriggerVarID.UNIT_MAGIC_SHIELD ];
            b.MeleeAttackLevel = VariableList[ (int) ETriggerVarID.UNIT_MELEEATTACKLEVEL ];
            b.RangedAttackLevel = VariableList[ (int) ETriggerVarID.UNIT_RANGEDATTACKLEVEL ];
            c.MovementLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_MOVEMENTLEVEL ];
            c.ArrowWalkingLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_ARROWWALKINGLEVEL ];
            b.DexterityLevel = VariableList[ (int) ETriggerVarID.UNIT_DEXTERITYLEVEL ];
            b.OrbStrikerLevel = VariableList[ (int) ETriggerVarID.UNIT_ORBSTRIKERLEVEL ];
            c.MonsterCorneringLevel = VariableList[ (int) ETriggerVarID.UNIT_MONSTERCORNERINGLEVEL ];
            b.CooperationLevel = VariableList[ (int) ETriggerVarID.UNIT_COOPERATIONLEVEL ];
            b.DamageSurplusLevel = VariableList[ (int) ETriggerVarID.UNIT_DAMAGESURPLUSLEVEL ];
            b.MeleeShieldLevel = VariableList[ (int) ETriggerVarID.UNIT_MELEESHIELDLEVEL ];
            b.MissileShieldLevel = VariableList[ (int) ETriggerVarID.UNIT_MISSILESHIELDLEVEL ];
            b.MagicShieldLevel = VariableList[ (int) ETriggerVarID.UNIT_MAGICSHIELDLEVEL ];
            c.MonsterPushLevel = VariableList[ (int) ETriggerVarID.UNIT_MONSTERPUSHLEVEL ];
            c.ScoutLevel = VariableList[ (int) ETriggerVarID.UNIT_SCOUTLEVEL ];
            c.PlatformWalkingLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_PLATFORMWALKINGLEVEL ];
            c.PlatformSteps = (int) VariableList[ (int) ETriggerVarID.UNIT_PLATFORMSTEPS ];
            b.WallDestroyerLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_WALLDESTROYERLEVEL ];
            b.AmbusherLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_AMBUSHERLEVEL ];
            b.MemoryLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_MEMORYLEVEL ];
            b.ToolBoxLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_TOOLBOXLEVEL ];
            c.SprinterLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_SPRINTERLEVEL ];
            b.FireMasterLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_FIREMASTERLEVEL ];
            b.BerserkLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_BERSERKLEVEL ];
            r.RicochetLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_RICOCHETLEVEL ];
            b.BeeHiveThrowerLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_BEEHIVETHROWERLEVEL ];
            b.PsychicLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_PSYCHICLEVEL ];
            c.SneakingLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_SNEAKINGLEVEL ];
            s.MaxBonusReached = (int) VariableList[ (int) ETriggerVarID.MAXBONUSREACHED ];
            s.AccumulatedBonuses = (int) VariableList[ (int) ETriggerVarID.ACCUMULATEDBONUS ];
            s.AreasCleared = (int) VariableList[ (int) ETriggerVarID.AREASCLEARED ];
            s.NormalSectorsDiscovered = (int) VariableList[ (int) ETriggerVarID.NORMALSECTORSDISCOVERED ];
            s.SectorsCleared = (int) VariableList[ (int) ETriggerVarID.SECTORSCLEARED ];
            s.NumPerfectAreas = (int) VariableList[ (int) ETriggerVarID.PERFECTAREAS ];
            s.NumPerfectSectors = (int) VariableList[ (int) ETriggerVarID.PERFECTSECTORS ];
            s.AccumulatedPoints = (int) VariableList[ (int) ETriggerVarID.ACCUMULATEDPOINTS ];
            s.BonfiresLit = (int) VariableList[ (int) ETriggerVarID.BONFIRESLIT ];
            s.DirtyBonfiresLit = (int) VariableList[ (int) ETriggerVarID.DIRTYBONFIRESLIT ];
            b.LooterLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_LOOTERLEVEL ];
            b.ProspectorLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_PROSPECTORLEVEL ];
            c.BarricadeFighterLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_BARRICADE_FIGHTER_LEVEL ];
            c.EvasionLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_EVASIONLEVEL ];
            c.ScavengerLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_SCAVENGERLEVEL ];
            c.PerfectionistLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_PERFECTIONISTLEVEL ];
            b.AgilityLevel = VariableList[ (int) ETriggerVarID.UNIT_AGILITYLEVEL ];
            c.ArrowFighterLevel = VariableList[ (int) ETriggerVarID.UNIT_ARROWFIGHTERLEVEL ];
            c.ArrowInLevel = VariableList[ (int) ETriggerVarID.UNIT_ARROWINLEVEL ];
            c.ArrowOutLevel = VariableList[ (int) ETriggerVarID.UNIT_ARROWOUTLEVEL ];
            b.FreshAttackLevel = VariableList[ (int) ETriggerVarID.UNIT_FRESHATTACKLEVEL ];
            b.RiskyAttackLevel = VariableList[ (int) ETriggerVarID.UNIT_RISKYATTACKLEVEL ];
            b.MortalJumpLevel = VariableList[ (int) ETriggerVarID.UNIT_MORTALJUMPLEVEL ];
            b.OpenFieldAtttackLevel = VariableList[ (int) ETriggerVarID.UNIT_OPENFIELDATTACKLEVEL ];
            b.BaseThreatDuration = (int) VariableList[ (int) ETriggerVarID.UNIT_BASETHREATDURATION ];
            b.BaseFreeExitHPLimit = (int) VariableList[ (int) ETriggerVarID.UNIT_BASEFREEEXITHPLIMIT ];
            b.FirePowerLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_FIREPOWERLEVEL ];
            b.FireSpreadLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_FIRESPREADLEVEL ];
            b.FireWoodNeeded = (int) VariableList[ (int) ETriggerVarID.UNIT_FIREWOODNEEDED ];
            b.OutsideFireWoodAllowed = (int) VariableList[ (int) ETriggerVarID.UNIT_OUTSIDEFIREWOODALLOWED ];
            b.DestroyBarricadeLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_BARRICADEDESTROYLEVEL ];
            b.FreePlatformExit = (int) VariableList[ (int) ETriggerVarID.UNIT_FREEPLATFORMEXIT ];
            b.OutAreaBurningBarricadeDestroyBonus = (int) VariableList[ (int) ETriggerVarID.UNIT_OUTAREABURNINGBARRICADEDESTROYBONUS ];
            c.OverBarricadeScoutLevel = VariableList[ (int) ETriggerVarID.UNIT_OVERBARRICADESCOUT ];
            c.ShowResourceChance = VariableList[ (int) ETriggerVarID.UNIT_SHOWRESOURCECHANCE ];
            c.ShowResourceNeighborsChance = VariableList[ (int) ETriggerVarID.UNIT_SHOWRESOURCENEIGHBORSCHANCE ];
            b.BarricadeForRune = (int) VariableList[ (int) ETriggerVarID.UNIT_BARRICADEFORRUNE ];
            b.ScaryLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_SCARYLEVEL ];
            b.HeroNeighborTouchAdder = (int) VariableList[ (int) ETriggerVarID.UNIT_HERONEIGHBORTOUCHADDER ];
            b.FireStarBonus = (int) VariableList[ (int) ETriggerVarID.UNIT_FIRESTARBONNUS ];
            b.CollectorLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_COLLECTORLEVEL ];
            b.ResourcePersistance = (int) VariableList[ (int) ETriggerVarID.UNIT_RESOURCEPERSISTANCE ];
            b.RealtimeMeleeAttSpeed = (int) VariableList[ (int) ETriggerVarID.UNIT_RTMELEEATTACKSPEED ];
            b.RealtimeRangedAttSpeed = (int) VariableList[ (int) ETriggerVarID.UNIT_RTRANGEDATTACKSPEED ];
            c.MireLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_MIRELEVEL ];
            c.RestingLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_RESTDISTANCE ];
            c.SlayerLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_SLAYERLEVEL ];
            c.FlyingTargetting = (int) VariableList[ (int) ETriggerVarID.UNIT_DRAGONTARGETTING ];
            c.SlayerAngle = (int) VariableList[ (int) ETriggerVarID.UNIT_SLAYERANGLE ];
            c.SlayerMaxHP = (int) VariableList[ (int) ETriggerVarID.UNIT_SLAYERMAXHP ];
            c.DragonDisguiseLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_DRAGONDISGUISE ];
            c.DragonBonusDropLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_DRAGONBONUSDROP ];
            c.DragonBarricadeProtection = (int) VariableList[ (int) ETriggerVarID.UNIT_DRAGONBARRICADEPROTECTION ];
            b.MiningLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_MININGLEVEL ];
            b.FishingLevel = (int) VariableList[ (int) ETriggerVarID.UNIT_FISHING_LEVEL ];
        }
    }
}