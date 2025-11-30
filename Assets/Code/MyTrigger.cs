using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
//using System.Single;

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


public class MyTrigger : MonoBehaviour
{
    #region Variables
    [Space( 10 )]
    [TabGroup( "Condition" )]
    public ETriggerVarID ConditionVarID;
    [Space( 10 )]
    [TabGroup( "Condition" )]
    public ETriggerCondOperator ConditionOperator;
    [Space( 10 )]
    [TabGroup( "Condition" )]
    public float ConditionVal1;
    [TabGroup( "Effect" )]
    public ETriggerVarID EffectVarID;
    [TabGroup( "Effect" )]
    public ETriggerEffOperator EffectOperator;
    [TabGroup( "Effect" )]
    public ETriggerVarID EffectVarID1;
    [TabGroup( "Effect" )]
    public float EffectVal1;
    [TabGroup( "Effect" )]
    public ETriggerEffOperator EffectOperator2;
    [TabGroup( "Effect" )]
    public ETriggerVarID EffectVarID2;
    [TabGroup( "Effect" )]
    public float EffectVal2;
    [TabGroup( "Effect" )]
    public ETriggerEffOperator EffectOperator3;
    [TabGroup( "Effect" )]
    public ETriggerVarID EffectVarID3;
    [TabGroup( "Effect" )]
    public float EffectVal3;
    [TabGroup( "Link" )]
    public Unit Unit;
    [TabGroup( "Link" )]
    public float[ ] VariableList;
    #endregion

    public void Copy( MyTrigger tr )
    {
        ConditionVarID = tr.ConditionVarID;
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

    public bool CheckConditionOperation( bool force = false )
    {
        float vval = VariableList[ ( int ) ConditionVarID ];

        if( G.HS == null ) return false;
        if( force == false )
        if( Map.I.RM.HeroSector.Type != Sector.ESectorType.GATES )                             // Hero needs to leave cube to win goal prizes
        if( Map.I.TurnFrameCount != 2 )                                                        // verifies only once per turn if inside cube                
            return false;

        if( ConditionOperator == ETriggerCondOperator.NONE )
            return true;
        if( ConditionOperator == ETriggerCondOperator.EQUALS && vval == ConditionVal1 )
            return true;
        if( ConditionOperator == ETriggerCondOperator.MAJOR && vval > ConditionVal1 )
            return true;
        if( ConditionOperator == ETriggerCondOperator.MINOR && vval < ConditionVal1 )
            return true;
        if( ConditionOperator == ETriggerCondOperator.MAJOR_OR_EQUAL && vval >= ConditionVal1 )
            return true;
        if( ConditionOperator == ETriggerCondOperator.MINOR_OR_EQUAL && vval <= ConditionVal1 ) 
            return true;
        return false;
    }

    bool DoEffectOperation()
    {
        float val1 = EffectVal1;
        float val2 = EffectVal2;
        float val3 = EffectVal3;

        if( EffectVarID1 != ETriggerVarID.NONE ) val1 = VariableList[ ( int ) EffectVarID1 ];
        if( EffectVarID2 != ETriggerVarID.NONE ) val2 = VariableList[ ( int ) EffectVarID2 ];
        if( EffectVarID3 != ETriggerVarID.NONE ) val3 = VariableList[ ( int ) EffectVarID3 ];

        float res = 0;
        if( EffectOperator2 == ETriggerEffOperator.NONE ) res = val1;
        else
            if( EffectOperator2 == ETriggerEffOperator.PLUS ) res = val1 + val2;
            else
                if( EffectOperator2 == ETriggerEffOperator.MINUS ) res = val1 - val2;
                else
                    if( EffectOperator2 == ETriggerEffOperator.MULTIPLY ) res = val1 * val2;
                    else
                        if( EffectOperator2 == ETriggerEffOperator.DIVIDE ) res = val1 / val2;

        if( EffectOperator3 == ETriggerEffOperator.NONE )
        {
        }
        else
            if( EffectOperator3 == ETriggerEffOperator.PLUS ) res += val3;
            else
                if( EffectOperator3 == ETriggerEffOperator.MINUS ) res -= val3;
                else
                    if( EffectOperator3 == ETriggerEffOperator.MULTIPLY ) res *= val3;
                    else
                        if( EffectOperator3 == ETriggerEffOperator.DIVIDE ) res /= val3;

        //Debug.Log("EffectOperator: " + EffectOperator +       " EffectVarID: " + EffectVarID + "  val1 " + val1 + "  val1 " + val2 );      

        if( EffectOperator == ETriggerEffOperator.EQUALS )
        {
            VariableList[ ( int ) EffectVarID ] = res; return true;
        }
        if( EffectOperator == ETriggerEffOperator.PLUS )
        {
            VariableList[ ( int ) EffectVarID ] += res; return true;
        }
        if( EffectOperator == ETriggerEffOperator.MINUS )
        {
            VariableList[ ( int ) EffectVarID ] -= res; return true;
        }
        if( EffectOperator == ETriggerEffOperator.MULTIPLY )
        {
            VariableList[ ( int ) EffectVarID ] *= res; return true;
        }
        if( EffectOperator == ETriggerEffOperator.DIVIDE )
        {
            VariableList[ ( int ) EffectVarID ] /= res; return true;
        }

        return false;
    }

    public bool UpdateIt( bool force = false )
    {
        bool res = false;
        CreateVarList( Unit );
        if( CheckConditionOperation( force ) )
        {
            res = true;
            DoEffectOperation();
        }

        UpdateVarListValues( ref Unit );
        return res;
    }

    public float GetVarAmount( Unit un )
    {
        CreateVarList( un );
        return VariableList[ ( int ) ConditionVarID ];
    }

    public void CreateVarList( Unit un )
    {
        VariableList = new float[ ( int ) ETriggerVarID.TOTAL_VALS ];
        VariableList[ ( int ) ETriggerVarID.UNIT_TOTALHP ] = un.Body.TotHp;
        VariableList[ ( int ) ETriggerVarID.UNIT_HP ] = un.Body.Hp;
        VariableList[ ( int ) ETriggerVarID.UNIT_STARS ] = un.Body.Stars;
        VariableList[ ( int ) ETriggerVarID.UNIT_LIVES ] = un.Body.Lives;

        VariableList[ ( int ) ETriggerVarID.UNIT_BONUSMELEEATTACK ] = un.MeleeAttack.BonusDamage;

        VariableList[ ( int ) ETriggerVarID.UNIT_BONUSRANGEDATTACK ] = un.RangedAttack.BonusDamage;
        VariableList[ ( int ) ETriggerVarID.UNIT_RANGEDRANGE ] = un.RangedAttack.BaseRange;
        VariableList[ ( int ) ETriggerVarID.UNIT_MELEE_SHIELD ] = un.Body.BonusMeleeShield;
        VariableList[ ( int ) ETriggerVarID.UNIT_RANGED_SHIELD ] = un.Body.BonusMissileShield;
        VariableList[ ( int ) ETriggerVarID.UNIT_MAGIC_SHIELD ] = un.Body.BonusMagicShield;

        VariableList[ ( int ) ETriggerVarID.UNIT_MELEEATTACKLEVEL ] = un.Body.MeleeAttackLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_RANGEDATTACKLEVEL ] = un.Body.RangedAttackLevel;
        //		VariableList[ ( int ) ETriggerVarID.UNIT_MAGICATTACKENABLED ] = System.Convert.ToSingle( un.MagicAttack.enabled );
        VariableList[ ( int ) ETriggerVarID.UNIT_MOVEMENTLEVEL ] = un.Control.MovementLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_ARROWWALKINGLEVEL ] = un.Control.ArrowWalkingLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_DEXTERITYLEVEL ] = un.Body.DexterityLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_ORBSTRIKERLEVEL ] = un.Body.OrbStrikerLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_MONSTERCORNERINGLEVEL ] = un.Control.MonsterCorneringLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_COOPERATIONLEVEL ] = un.Body.CooperationLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_DAMAGESURPLUSLEVEL ] = un.Body.DamageSurplusLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_MELEESHIELDLEVEL ] = un.Body.MeleeShieldLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_MISSILESHIELDLEVEL ] = un.Body.MissileShieldLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_MAGICSHIELDLEVEL ] = un.Body.MagicShieldLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_MONSTERPUSHLEVEL ] = un.Control.MonsterPushLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_SCOUTLEVEL ] = un.Control.ScoutLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_PLATFORMWALKINGLEVEL ] = un.Control.PlatformWalkingLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_PLATFORMSTEPS ] = un.Control.PlatformSteps;
        VariableList[ ( int ) ETriggerVarID.UNIT_WALLDESTROYERLEVEL ] = un.Body.WallDestroyerLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_AMBUSHERLEVEL ] = un.Body.AmbusherLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_MEMORYLEVEL ] = un.Body.MemoryLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_TOOLBOXLEVEL ] = un.Body.ToolBoxLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_SPRINTERLEVEL ] = un.Control.SprinterLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_FIREMASTERLEVEL ] = un.Body.FireMasterLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_BERSERKLEVEL ] = un.Body.BerserkLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_RICOCHETLEVEL ] = un.RangedAttack.RicochetLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_BEEHIVETHROWERLEVEL ] = un.Body.BeeHiveThrowerLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_PSYCHICLEVEL ] = un.Body.PsychicLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_SNEAKINGLEVEL ] = un.Control.SneakingLevel;
        VariableList[ ( int ) ETriggerVarID.ROACHDEATHCOUNT ] = Map.I.LevelStats.RoachDeathCount;
        VariableList[ ( int ) ETriggerVarID.MONSTERSDEATHCOUNT ] = Map.I.LevelStats.MonstersDeathCount;
        VariableList[ ( int ) ETriggerVarID.SCARABDEATHCOUNT ] = Map.I.LevelStats.ScarabDeathCount;
        VariableList[ ( int ) ETriggerVarID.POISONERDEATHCOUNT ] = Map.I.LevelStats.PoisonerDeathCount;
        VariableList[ ( int ) ETriggerVarID.UNIT_RESOURCECOLLECTED ] = Map.I.LevelStats.ResourceCollected;
        VariableList[ ( int ) ETriggerVarID.UNIT_FISHINGBONUSREACHED ] = Map.I.LevelStats.FishingBonusReached;
        VariableList[ ( int ) ETriggerVarID.UNIT_CONQUEREDGOALS ] = Map.I.LevelStats.ConqueredGoals;
        VariableList[ ( int ) ETriggerVarID.MAXBONUSREACHED ] = Map.I.LevelStats.MaxBonusReached;
        VariableList[ ( int ) ETriggerVarID.ACCUMULATEDBONUS ] = Map.I.LevelStats.AccumulatedBonuses;
        VariableList[ ( int ) ETriggerVarID.AREASCLEARED ] = Map.I.LevelStats.AreasCleared;
        VariableList[ ( int ) ETriggerVarID.NORMALSECTORSDISCOVERED ] = Map.I.LevelStats.NormalSectorsDiscovered;
        VariableList[ ( int ) ETriggerVarID.SECTORSCLEARED ] = Map.I.LevelStats.SectorsCleared;
        VariableList[ ( int ) ETriggerVarID.PERFECTAREAS ] = Map.I.LevelStats.NumPerfectAreas;
        VariableList[ ( int ) ETriggerVarID.PERFECTSECTORS ] = Map.I.LevelStats.NumPerfectSectors;
        VariableList[ ( int ) ETriggerVarID.ACCUMULATEDPOINTS ] = Map.I.LevelStats.AccumulatedPoints;
        VariableList[ ( int ) ETriggerVarID.BONFIRESLIT ] = Map.I.LevelStats.BonfiresLit;
        VariableList[ ( int ) ETriggerVarID.DIRTYBONFIRESLIT ] = Map.I.LevelStats.DirtyBonfiresLit;
        VariableList[ ( int ) ETriggerVarID.UNIT_LOOTERLEVEL ] = un.Body.LooterLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_PROSPECTORLEVEL ] = un.Body.ProspectorLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_BARRICADE_FIGHTER_LEVEL ] = un.Control.BarricadeFighterLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_EVASIONLEVEL ] = un.Control.EvasionLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_PERFECTIONISTLEVEL ] = un.Control.PerfectionistLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_SCAVENGERLEVEL ] = un.Control.ScavengerLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_AGILITYLEVEL ] = un.Body.AgilityLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_ARROWFIGHTERLEVEL ] = un.Control.ArrowFighterLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_ARROWINLEVEL ] = un.Control.ArrowInLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_ARROWOUTLEVEL ] = un.Control.ArrowOutLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_FRESHATTACKLEVEL ] = un.Body.FreshAttackLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_RISKYATTACKLEVEL ] = un.Body.RiskyAttackLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_MORTALJUMPLEVEL ] = un.Body.MortalJumpLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_OPENFIELDATTACKLEVEL ] = un.Body.OpenFieldAtttackLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_BASETHREATDURATION ] = un.Body.BaseThreatDuration;
        VariableList[ ( int ) ETriggerVarID.UNIT_BASEFREEEXITHPLIMIT ] = un.Body.BaseFreeExitHPLimit;
        VariableList[ ( int ) ETriggerVarID.UNIT_FIREPOWERLEVEL ] = un.Body.FirePowerLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_FIRESPREADLEVEL ] = un.Body.FireSpreadLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_FIREWOODNEEDED ] = un.Body.FireWoodNeeded;
        VariableList[ ( int ) ETriggerVarID.UNIT_OUTSIDEFIREWOODALLOWED ] = un.Body.OutsideFireWoodAllowed;
        VariableList[ ( int ) ETriggerVarID.UNIT_BARRICADEDESTROYLEVEL ] = un.Body.DestroyBarricadeLevel;

        VariableList[ ( int ) ETriggerVarID.UNIT_FREEPLATFORMEXIT ] = un.Body.FreePlatformExit;
        VariableList[ ( int ) ETriggerVarID.UNIT_OUTAREABURNINGBARRICADEDESTROYBONUS ] = un.Body.OutAreaBurningBarricadeDestroyBonus;
        VariableList[ ( int ) ETriggerVarID.UNIT_OVERBARRICADESCOUT ] = un.Control.OverBarricadeScoutLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_SHOWRESOURCECHANCE ] = un.Control.ShowResourceChance;
        VariableList[ ( int ) ETriggerVarID.UNIT_SHOWRESOURCENEIGHBORSCHANCE ] = un.Control.ShowResourceNeighborsChance;
        VariableList[ ( int ) ETriggerVarID.UNIT_BARRICADEFORRUNE ] = un.Body.BarricadeForRune;
        VariableList[ ( int ) ETriggerVarID.UNIT_SCARYLEVEL ] = un.Body.ScaryLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_HERONEIGHBORTOUCHADDER ] = un.Body.HeroNeighborTouchAdder;
        VariableList[ ( int ) ETriggerVarID.UNIT_FIRESTARBONNUS ] = un.Body.FireStarBonus;
        VariableList[ ( int ) ETriggerVarID.UNIT_COLLECTORLEVEL ] = un.Body.CollectorLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_RESOURCEPERSISTANCE ] = un.Body.ResourcePersistance;

        VariableList[ ( int ) ETriggerVarID.UNIT_RTMELEEATTACKSPEED ] = un.Body.RealtimeMeleeAttSpeed;
        VariableList[ ( int ) ETriggerVarID.UNIT_RTRANGEDATTACKSPEED ] = un.Body.RealtimeRangedAttSpeed;
        VariableList[ ( int ) ETriggerVarID.UNIT_MIRELEVEL ] = un.Control.MireLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_RESTDISTANCE ] = un.Control.RestingLevel;

        VariableList[ ( int ) ETriggerVarID.UNIT_SLAYERLEVEL ] = un.Control.SlayerLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_DRAGONTARGETTING ] = un.Control.FlyingTargetting;
        VariableList[ ( int ) ETriggerVarID.UNIT_SLAYERANGLE ] = un.Control.SlayerAngle;
        VariableList[ ( int ) ETriggerVarID.UNIT_SLAYERMAXHP ] = un.Control.SlayerMaxHP;
        VariableList[ ( int ) ETriggerVarID.UNIT_DRAGONDISGUISE ] = un.Control.DragonDisguiseLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_DRAGONBONUSDROP ] = un.Control.DragonBonusDropLevel;
        VariableList[ ( int ) ETriggerVarID.UNIT_DRAGONBARRICADEPROTECTION ] = un.Control.DragonBarricadeProtection;
        VariableList[ ( int ) ETriggerVarID.UNIT_MININGLEVEL ] = un.Body.MiningLevel;

        VariableList[ ( int ) ETriggerVarID.UNIT_FISHING_LEVEL ] = un.Body.FishingLevel;
    }

    public void UpdateVarListValues( ref Unit un )
    {
        un.Body.TotHp = VariableList[ ( int ) ETriggerVarID.UNIT_TOTALHP ];
        un.Body.Hp = VariableList[ ( int ) ETriggerVarID.UNIT_HP ];
        un.Body.TotHp = VariableList[ ( int ) ETriggerVarID.UNIT_TOTALHP ];
        un.Body.Stars = VariableList[ ( int ) ETriggerVarID.UNIT_STARS ];
        un.Body.Lives = VariableList[ ( int ) ETriggerVarID.UNIT_LIVES ];

        un.MeleeAttack.BonusDamage = VariableList[ ( int ) ETriggerVarID.UNIT_BONUSMELEEATTACK ];

        un.RangedAttack.BonusDamage = VariableList[ ( int ) ETriggerVarID.UNIT_BONUSRANGEDATTACK ];
        un.RangedAttack.BaseRange = VariableList[ ( int ) ETriggerVarID.UNIT_RANGEDRANGE ];

        un.Body.BonusMeleeShield = VariableList[ ( int ) ETriggerVarID.UNIT_MELEE_SHIELD ];
        un.Body.BonusMissileShield = VariableList[ ( int ) ETriggerVarID.UNIT_RANGED_SHIELD ];
        un.Body.BonusMagicShield = VariableList[ ( int ) ETriggerVarID.UNIT_MAGIC_SHIELD ];

        un.Body.MeleeAttackLevel = VariableList[ ( int ) ETriggerVarID.UNIT_MELEEATTACKLEVEL ];
        un.Body.RangedAttackLevel = VariableList[ ( int ) ETriggerVarID.UNIT_RANGEDATTACKLEVEL ];
        //un.MagicAttack.enabled  = System.Convert.ToBoolean( VariableList[ ( int ) ETriggerVarID.UNIT_MAGICATTACKENABLED ] );
        un.Control.MovementLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_MOVEMENTLEVEL ];
        un.Control.ArrowWalkingLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_ARROWWALKINGLEVEL ];
        un.Body.DexterityLevel = VariableList[ ( int ) ETriggerVarID.UNIT_DEXTERITYLEVEL ];
        un.Body.OrbStrikerLevel = VariableList[ ( int ) ETriggerVarID.UNIT_ORBSTRIKERLEVEL ];
        un.Control.MonsterCorneringLevel = VariableList[ ( int ) ETriggerVarID.UNIT_MONSTERCORNERINGLEVEL ];
        un.Body.CooperationLevel = VariableList[ ( int ) ETriggerVarID.UNIT_COOPERATIONLEVEL ];
        un.Body.DamageSurplusLevel = VariableList[ ( int ) ETriggerVarID.UNIT_DAMAGESURPLUSLEVEL ];
        un.Body.MeleeShieldLevel = VariableList[ ( int ) ETriggerVarID.UNIT_MELEESHIELDLEVEL ];
        un.Body.MissileShieldLevel = VariableList[ ( int ) ETriggerVarID.UNIT_MISSILESHIELDLEVEL ];
        un.Body.MagicShieldLevel = VariableList[ ( int ) ETriggerVarID.UNIT_MAGICSHIELDLEVEL ];
        un.Control.MonsterPushLevel = VariableList[ ( int ) ETriggerVarID.UNIT_MONSTERPUSHLEVEL ];
        un.Control.ScoutLevel = VariableList[ ( int ) ETriggerVarID.UNIT_SCOUTLEVEL ];
        un.Control.PlatformWalkingLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_PLATFORMWALKINGLEVEL ];
        un.Control.PlatformSteps = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_PLATFORMSTEPS ];
        un.Body.WallDestroyerLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_WALLDESTROYERLEVEL ];
        un.Body.AmbusherLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_AMBUSHERLEVEL ];
        un.Body.MemoryLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_MEMORYLEVEL ];
        un.Body.ToolBoxLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_TOOLBOXLEVEL ];

        un.Control.SprinterLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_SPRINTERLEVEL ];
        un.Body.FireMasterLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_FIREMASTERLEVEL ];
        un.Body.BerserkLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_BERSERKLEVEL ];
        un.RangedAttack.RicochetLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_RICOCHETLEVEL ];
        un.Body.BeeHiveThrowerLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_BEEHIVETHROWERLEVEL ];
        un.Body.PsychicLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_PSYCHICLEVEL ];
        un.Control.SneakingLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_SNEAKINGLEVEL ];

        //Map.I.LevelStats.RoachDeathCount = ( int ) VariableList[ ( int ) ETriggerVarID.ROACHDEATHCOUNT ];
        //Map.I.LevelStats.ScarabDeathCount = ( int ) VariableList[ ( int ) ETriggerVarID.SCARABDEATHCOUNT ];
        //Map.I.LevelStats.MonstersDeathCount = VariableList[ ( int ) ETriggerVarID.MONSTERSDEATHCOUNT ];            // No need to update these since there are no artifacts that upgrades this
        //Map.I.LevelStats.PoisonerDeathCount = ( int ) VariableList[ ( int ) ETriggerVarID.POISONERDEATHCOUNT ];
        //Map.I.LevelStats.ResourceCollected = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_RESOURCECOLLECTED ];
        //FishingBonus, conquered goals,   included more in the list
        Map.I.LevelStats.MaxBonusReached = ( int ) VariableList[ ( int ) ETriggerVarID.MAXBONUSREACHED ];
        Map.I.LevelStats.AccumulatedBonuses = ( int ) VariableList[ ( int ) ETriggerVarID.ACCUMULATEDBONUS ];
        Map.I.LevelStats.AreasCleared = ( int ) VariableList[ ( int ) ETriggerVarID.AREASCLEARED ];
        Map.I.LevelStats.NormalSectorsDiscovered = ( int ) VariableList[ ( int ) ETriggerVarID.NORMALSECTORSDISCOVERED ];
        Map.I.LevelStats.SectorsCleared = ( int ) VariableList[ ( int ) ETriggerVarID.SECTORSCLEARED ];
        Map.I.LevelStats.NumPerfectAreas = ( int ) VariableList[ ( int ) ETriggerVarID.PERFECTAREAS ];
        Map.I.LevelStats.NumPerfectSectors = ( int ) VariableList[ ( int ) ETriggerVarID.PERFECTSECTORS ];
        Map.I.LevelStats.AccumulatedPoints = ( int ) VariableList[ ( int ) ETriggerVarID.ACCUMULATEDPOINTS ];
        Map.I.LevelStats.BonfiresLit = ( int ) VariableList[ ( int ) ETriggerVarID.BONFIRESLIT ];
        Map.I.LevelStats.DirtyBonfiresLit = ( int ) VariableList[ ( int ) ETriggerVarID.DIRTYBONFIRESLIT ];
        un.Body.LooterLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_LOOTERLEVEL ];
        un.Body.ProspectorLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_PROSPECTORLEVEL ];
        un.Control.BarricadeFighterLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_BARRICADE_FIGHTER_LEVEL ];
        un.Control.EvasionLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_EVASIONLEVEL ];
        un.Control.ScavengerLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_SCAVENGERLEVEL ];
        un.Control.PerfectionistLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_PERFECTIONISTLEVEL ];
        un.Body.AgilityLevel = VariableList[ ( int ) ETriggerVarID.UNIT_AGILITYLEVEL ];
        un.Control.ArrowFighterLevel = VariableList[ ( int ) ETriggerVarID.UNIT_ARROWFIGHTERLEVEL ];
        un.Control.ArrowInLevel = VariableList[ ( int ) ETriggerVarID.UNIT_ARROWINLEVEL ];
        un.Control.ArrowOutLevel = VariableList[ ( int ) ETriggerVarID.UNIT_ARROWOUTLEVEL ];
        un.Body.FreshAttackLevel = VariableList[ ( int ) ETriggerVarID.UNIT_FRESHATTACKLEVEL ];
        un.Body.RiskyAttackLevel = VariableList[ ( int ) ETriggerVarID.UNIT_RISKYATTACKLEVEL ];
        un.Body.MortalJumpLevel = VariableList[ ( int ) ETriggerVarID.UNIT_MORTALJUMPLEVEL ];
        un.Body.OpenFieldAtttackLevel = VariableList[ ( int ) ETriggerVarID.UNIT_OPENFIELDATTACKLEVEL ];
        un.Body.BaseThreatDuration = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_BASETHREATDURATION ];
        un.Body.BaseFreeExitHPLimit = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_BASEFREEEXITHPLIMIT ];
        un.Body.FirePowerLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_FIREPOWERLEVEL ];
        un.Body.FireSpreadLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_FIRESPREADLEVEL ];
        un.Body.FireWoodNeeded = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_FIREWOODNEEDED ];
        un.Body.OutsideFireWoodAllowed = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_OUTSIDEFIREWOODALLOWED ];
        un.Body.DestroyBarricadeLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_BARRICADEDESTROYLEVEL ];
        un.Body.FreePlatformExit = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_FREEPLATFORMEXIT ];
        un.Body.OutAreaBurningBarricadeDestroyBonus = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_OUTAREABURNINGBARRICADEDESTROYBONUS ];
        un.Control.OverBarricadeScoutLevel = VariableList[ ( int ) ETriggerVarID.UNIT_OVERBARRICADESCOUT ];
        un.Control.ShowResourceChance = VariableList[ ( int ) ETriggerVarID.UNIT_SHOWRESOURCECHANCE ];
        un.Control.ShowResourceNeighborsChance = VariableList[ ( int ) ETriggerVarID.UNIT_SHOWRESOURCENEIGHBORSCHANCE ];
        un.Body.BarricadeForRune = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_BARRICADEFORRUNE ];
        un.Body.ScaryLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_SCARYLEVEL ];
        un.Body.HeroNeighborTouchAdder = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_HERONEIGHBORTOUCHADDER ];
        un.Body.FireStarBonus = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_FIRESTARBONNUS ];
        un.Body.CollectorLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_COLLECTORLEVEL ];
        un.Body.ResourcePersistance = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_RESOURCEPERSISTANCE ];

        un.Body.RealtimeMeleeAttSpeed = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_RTMELEEATTACKSPEED ];
        un.Body.RealtimeRangedAttSpeed = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_RTRANGEDATTACKSPEED ];
        un.Control.MireLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_MIRELEVEL ];
        un.Control.RestingLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_RESTDISTANCE ];

        un.Control.SlayerLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_SLAYERLEVEL ];
        un.Control.FlyingTargetting = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_DRAGONTARGETTING ];
        un.Control.SlayerAngle = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_SLAYERANGLE ];
        un.Control.SlayerMaxHP = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_SLAYERMAXHP ];
        un.Control.DragonDisguiseLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_DRAGONDISGUISE ];
        un.Control.DragonBonusDropLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_DRAGONBONUSDROP ];
        un.Control.DragonBarricadeProtection = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_DRAGONBARRICADEPROTECTION ];
        un.Body.MiningLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_MININGLEVEL ];

        un.Body.FishingLevel = ( int ) VariableList[ ( int ) ETriggerVarID.UNIT_FISHING_LEVEL ];
    }
}
