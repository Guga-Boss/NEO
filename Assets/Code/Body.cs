using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using PathologicalGames;
using Sirenix.OdinInspector;
public enum EUnitHealthStatus
{
	NONE = -1, HEALTHY, DAMAGED, DYING
}

public enum EAutoRotateMineType
{
    NONE = -1, RANDOM_ANY
}

[System.Serializable]
public class Body : MonoBehaviour
{
    #region Variables
    [TabGroup( "" )]
    [PropertyTooltip( "Tab 1" )]
    bool clear = false;
    [TabGroup( "Link" )]
    public Unit Unit;
    [TabGroup( "Link" )]
    public tk2dSprite Shadow;
    [TabGroup( "Link" )]
    public tk2dSprite ItemMiniDome;
    [TabGroup( "Link" )]
    public tk2dSprite Sprite2, Sprite3, Sprite4, Sprite5, Sprite6, Sprite7, Sprite8, InfectedSprite, ImmunityDome;
    [TabGroup( "Link" )]
    public tk2dTiledSprite PoleSprite;
    [TabGroup( "Link" )]
    public tk2dSpriteAnimator Animator;
    [TabGroup( "Link" )]
    public tk2dTextMesh AuxText;
    [TabGroup( "Link" )]
    public ParticleSystem Particle;
    [TabGroup( "Link" )]
    public GameObject[] EffectList, ArrowList, WoodList;
    [TabGroup( "Link" )]
    public List<tk2dTextMesh> TextList;
    [TabGroup( "Link" )]
    public List<Unit> ChildList, ShacklePullList;
    [TabGroup( "Link" )]
    public Unit RopeConnectSon = null;
    [TabGroup( "Link" )]
    public Vector2 RopeConnectSonPos;   // auxiliar to save cube reference
    [TabGroup( "Link" )]
    public List<Unit> RopeConnectFather = null;
    [TabGroup( "Link" )]
    public List<Vector2> RopeConnectFatherPosList = null; // auxiliar to save cube reference
    [TabGroup( "Main" )]
    public EUnitHealthStatus HealthStatus;
    [TabGroup( "Main" )]
    public Vector2 FireSourcePos = new Vector2( -1, -1 );
    [TabGroup( "Link" )]
    public Rigidbody2D RigidBody;
    [TabGroup( "Link" )]
    public TackRope Rope;
    [TabGroup( "Link" )]
    public List<tk2dSprite> PoleSpriteList, PoleBackSpriteList;
    [TabGroup( "Link" )]
    public Tack RopeOrigin, RopeDestination;
    [TabGroup( "Link" )]
    public GameObject RopeRotationHelper;
    [TabGroup( "Bool" )]
    public bool BigMonster, ShowShieldInfo, ShowLevelInfo, FireIsOn, OriginalFireIsOn, IsTiny, IsMarked, OriginalIsMarked,
    PoisonBite, IsEternalToken, HasSeenTheHero, OriginalHasSeenTheHero, IsBoss, SpawnBlocker, LeverMine,  
    ClockwiseChainPush, ShieldedWasp, RaftResource = false, EnragedWasp = false, CocoonWasp = false, Crippled = false, SpikedBoulder = false, TrailCoil = false, TrailRotator = false,
    HookIsStuck = false, CreatedVine = false, GraphicsInitialized = false;
    [TabGroup( "Main" )]
    public float Level = 1, BaseLevel = 1, Stars, TotHp, Hp, BaseTotHP;
    [TabGroup( "Main" )]
    public int  HitsTaken, OriginalHitsTaken;
    [TabGroup( "Level" )]
    public float TotalMeleeShield,   BaseMeleeShield,   BonusMeleeShield,   MeleeShieldPerLevel,   MeleeShieldPerStar,
    TotalMissileShield, BaseMissileShield, BonusMissileShield, MissileShieldPerLevel, MissileShieldPerStar,
    TotalMagicShield,   BaseMagicShield,   BonusMagicShield,   MagicShieldPerLevel,   MagicShieldPerStar,
    Lives, NumMushroom, HpPerMushroom, PressureAttackBonus, InvulnerabilityFactor,  
    MeleeAttackLevel, RangedAttackLevel, DexterityLevel, AgilityLevel, OrbStrikerLevel, CooperationLevel, PsychicLevel,
    DamageSurplusLevel, MeleeShieldLevel, MissileShieldLevel, MagicShieldLevel, DestroyBarricadeLevel, AmbusherLevel,
	HpPerLevel, HpPerStar, DamageTaken, MemoryLevel, ToolBoxLevel, FireMasterLevel, BerserkLevel, BeeHiveThrowerLevel, 
    WallDestroyerLevel, NumFreeAttacks, LooterLevel, ProspectorLevel, FreshAttackLevel, RiskyAttackLevel, MortalJumpLevel, 
    OpenFieldAtttackLevel, ThreatLevel, BaseFreeExitHPLimit,    
    FirePowerLevel, FireSpreadLevel, FireWoodNeeded, OutsideFireWoodAllowed, FreePlatformExit, 
    OutAreaBurningBarricadeDestroyBonus, BarricadeForRune, ScaryLevel, HeroNeighborTouchAdder, FireStarBonus, CollectorLevel,
    ResourcePersistance, EconomyLevel, RealtimeMeleeAttSpeed, RealtimeRangedAttSpeed, MiningLevel, FishingLevel, QQMissHeroDamage = 0, QQMinDamage = 5, QQMaxDamage = 10,
    Fishing_1, Fishing_2, Fishing_3, Fishing_4, TweenTime, RotationSpeed, KillTimer = -1;

    [TabGroup( "Enum" )]
    public EResourceOperation ResourceOperation = EResourceOperation.NONE;

    [TabGroup( "List" )]
    public List<ItemType> BonusItemList = null;
    [TabGroup( "List" )]
    public List<float> BonusItemAmountList = null;
    [TabGroup( "List" )]
    public List<float> BonusItemChanceList = null;
    [TabGroup( "List" )]
    public List<int> BonusItemChestLevelList = null;
    [TabGroup( "Enum" )]
    public EFishType FishType = EFishType.NONE;
    [TabGroup( "Enum" )]
    public EHerbColor HerbColor = EHerbColor.NONE;
    [TabGroup( "Enum" )]
    public EHerbType HerbType = EHerbType.NORMAL;
    [TabGroup( "Enum" )]
    public EHerbType OriginalHerbType = EHerbType.NORMAL;
    [TabGroup( "Bool" )]
    public bool OptionalMonster = false;
    [TabGroup( "Bool" )]
    public bool FishCaught = false;
    [TabGroup( "Bool" )]
    public bool IsFish = true;
    [TabGroup( "Bool" )]
    public int ResourcePersistTotalSteps = 1;
    [TabGroup( "Bool" )]
    public int ResourcePersistStepCount = 0;
    [TabGroup( "Bool" )]
    public bool isWorking = true;
    [TabGroup( "Bool" )]
    public bool IsDead, MudPushable, PerformPreMoveAttack, PerformPostMoveAttack, IsLoot, OrbFogStopEnabled, KillOnFlightEnd = false, SpikeDamageApplied = false;
    [TabGroup( "Main" )]
    public float TurnsToDie, OriginalTurnsToDie;
    [TabGroup( "Main" )]
    public float DeathCountFactor = 1;
    [TabGroup( "Main" )]
    public float HitPointsRatio = 100;
    [TabGroup( "Original" )]
    public int OriginalVariation = -1;
    [TabGroup( "Int" )]
    public int FrontalAttacks, FrontSideAttacks, SideAttacks, BackSideAttacks, BackAttacks,
               OriginalFrontalAttacks, OriginalFrontSideAttacks, OriginalSideAttacks, OriginalBackSideAttacks,
               OriginalBackAttacks, MakePeaceful, MakeImovable, OriginalArrowHitCount, ArrowHitCount,
               OriginalRiskyAttacks, RiskyAttacks, OriginalThreatLevel, BaseThreatDuration, WoodAddedTurnCount, ChiselSlideCount, VineFireSpreadID = -1, ResourceSlot = 1;
    [TabGroup( "Link" )]
    public Spell HookID = null;
    [TabGroup( "float" )]
    public float MortalJumpBonusAttack, OutAreaMortalJumpBonusAttack, OriginalMortalJumpBonusAttack,
                 OriginalOpenFieldAttacks, OpenFieldAttacks, FirstAttackTime, ShieldedWaspChance, MaxShieldedWasps, BumpTimer = 0, VineBurnTime = .5f, OverFireTimeCount = 0, ImmunityShieldTime = 0, PoleBonusAvailableAngle = -1;
    [TabGroup( "float" )]
    public float HPLostInTheTurn, EnemyAttackBonus, BaseMiningChance, ExtraMiningChance, AnimationTimeCounter,
    MiningBonusAmount;
    [TabGroup( "Enum" )]
    public EMineType MineType = EMineType.DIRECTIONAL, UPMineType = EMineType.NONE;
    [TabGroup( "Enum" )]
    public EResTriggerType ResTriggerType = EResTriggerType.NONE;
    [TabGroup( "Int" )]
    public int ProtectionLevel, BaseMonsterLootTurns,
    TouchCount, OriginalTouchCount, AnimationFrame, AnimationSignal, ChestLevel = 1;
    [TabGroup( "Float" )]
    public float StackAmount, AuxStackAmount = -98.9f;
    [TabGroup( "Enum" )]
    public EDirection MineLeverDir, UPMineDir;
    [TabGroup( "Enum" )]
    public EAutoRotateMineType AutoRotatingMineType = EAutoRotateMineType.RANDOM_ANY;
    [TabGroup( "Float" )]
    public float MineGreenCrystalAmount = 0;
    [TabGroup( "Float" )]
    public float MineRedCrystalAmount = 0;
    [TabGroup( "Link" )]
    public Unit MineLeverActivePuller = null;
    [TabGroup( "Link" )]
    public Vector2 MineLeverActivePullerPos; // auxiliar to save cube reference
    [TabGroup( "Link" )]
    public Transform AuxTransform;
    [TabGroup( "int" )]
    public int MinePushSteps = 0;
    [TabGroup( "Enum" )]
    public ItemType MiningPrize;
    [TabGroup( "Int" )]
    public int HerbBonusAmount = 1;
    [TabGroup( "Int" )]
    public int AvailableFireHits = -1;
    [TabGroup( "Int" )]
    public int ExclusiveToAdventure;
    [TabGroup( "List" )]
    public List<Vector2> HeroAttackPositionHistory;
    [TabGroup( "List" )]
    public List<EDirection> HeroAttackDirectionHistory;
    [TabGroup( "List" )]
    public List<bool> WoodAdded;
    [TabGroup( "List" )]
    public List<tk2dSprite> BabySprite, BabyBackSprite;
    [TabGroup( "Link" )]
    public GameObject BabySpriteFolder;
    [TabGroup( "List" )]
    public List<bool> HasBaby;
    [TabGroup( "List" )]
    public List<BabyData> BabyDataList;
    [TabGroup( "List" )]
    public List<Spell> Sp;
    [TabGroup( "List" )]
    public List<int> BabyVariation, MineSideHitCount, CreatedVineList;
    [HideInInspector]
    public int HerbMatchChecked = 0;
    [TabGroup( "Bool" )]
    public bool RandomizableHerb = false;
    [TabGroup( "float" )]
    public float HerbKillTimer = -1;
    [TabGroup( "float" )]
    public float MiniDomeTimerCounter = -1;
    [TabGroup( "float" )]
    public float MiniDomeTotalTime = -1;
    [TabGroup( "float" )]
    public float ResourceWasteTimeCounter = -1;
    [TabGroup( "float" )]
    public float ResourceWasteTotalTime = -1;
    [TabGroup( "float" )]
    public float VsRoachAttDecreasePerBaby = 20;
    [TabGroup( "float" )]
    public float VsHeroRoachAttackIncreasePerBaby = 50;
    [TabGroup( "Link" )]
    public Color SpriteColor = new Color( 0, 0, 0, 0 );
    [TabGroup( "float" )]
    public float FrameDamageTaken;
    [TabGroup( "float" )]
    public float InflictedDamageRate = 100;
    [TabGroup( "Int" )]
    public int ShackleDistance = -1;
    [TabGroup( "Enum" )]
    public EMineType OriginalHookType = EMineType.NONE;
    [TabGroup( "Int" )]
    public int AddedLivesCount = 0;
    [TabGroup( "Link" )]
    public Animator Animation1, Animation2;
    [TabGroup( "Int" )]
    public int InfectedRadius = 1;
    [TabGroup( "Bool" )]
    public bool IsInfected = false;


    #endregion
    public void Save()
    {
        GS.W.Write( Lives );
        GS.W.Write( IsDead );
        GS.W.Write( TotHp );
        GS.W.Write( BaseTotHP );
        GS.W.Write( Hp );
        GS.W.Write( AnimationFrame );
        GS.W.Write( AnimationSignal );
        GS.W.Write( AnimationTimeCounter );
        GS.W.Write( ( int ) FishType );
        GS.W.Write( IsFish );
        GS.W.Write( HookIsStuck );
        GS.W.Write( StackAmount );
        GS.W.Write( ArrowHitCount );
        GS.W.Write( HPLostInTheTurn );
        GS.W.Write( isWorking );
        GS.W.Write( MakePeaceful );
        GS.W.Write( MakeImovable );
        GS.W.Write( Level );
        GS.W.Write( BaseLevel );
        GS.W.Write( NumFreeAttacks );
        GS.W.Write( DamageTaken );
        GS.W.Write( ( int ) HealthStatus );
        GS.W.Write( FireIsOn );
        GS.W.Write( ImmunityShieldTime );
        
        if( Unit.UnitType == EUnitType.HERO )
        {
            GS.W.Write( RangedAttackLevel );
            GS.W.Write( MeleeAttackLevel );
            GS.W.Write( FireMasterLevel );
            GS.W.Write( DestroyBarricadeLevel );
            GS.W.Write( MeleeShieldLevel );
            GS.W.Write( MissileShieldLevel );
            GS.W.Write( MiningLevel );
            GS.W.Write( Stars );
            GS.W.Write( FishingLevel );
            for( int i = 0; i < 8; i++ )
                GS.W.Write( G.Hero.Body.Sp[ i ].BambooSize );
        }

        if( Unit.TileID == ETileType.VINES )
        {
            GS.W.Write( VineFireSpreadID );
            GS.W.Write( CreatedVine );
            GS.SVector2( FireSourcePos );
            
            int sz = CreatedVineList.Count;
            GS.W.Write( sz );
            for( int i = 0; i < CreatedVineList.Count; i++ )
                GS.W.Write( CreatedVineList[ i ] );
        }

        if( Unit.UnitType == EUnitType.HERO || 
            Unit.TileID == ETileType.MINE )
        {
            GS.W.Write( ShackleDistance );
            GS.W.Write( RopeConnectFather.Count );
            for( int i = 0; i < RopeConnectFather.Count; i++ )
                GS.SVector2( RopeConnectFather[ i ].Pos );
            Vector2 pos = new Vector2( -1, -1 );
            if( RopeConnectSon )
                pos = RopeConnectSon.Pos;
            GS.SVector2( pos );
        }

        if( Unit.TileID == ETileType.MINE )
        {
            GS.W.Write( ( int ) MineLeverDir );
            GS.W.Write( LeverMine );
            GS.W.Write( BaseMiningChance );
            GS.W.Write( ExtraMiningChance );
            GS.W.Write( MinePushSteps );
           Vector2 pos = new Vector2( -1, -1 );
            if( MineLeverActivePuller ) 
                pos = MineLeverActivePuller.Pos;
            GS.SVector2( pos );
            GS.W.Write( ( int ) MineType );
            GS.W.Write( ( int ) UPMineType );
            GS.W.Write( ( int ) UPMineDir );
            GS.W.Write( ( int ) MiningPrize );
            GS.W.Write( MiningBonusAmount );
            GS.W.Write( ( int ) AutoRotatingMineType );           
            GS.W.Write( ChiselSlideCount );
            GS.W.Write( ClockwiseChainPush );
            GS.W.Write( MineGreenCrystalAmount );
            GS.W.Write( MineRedCrystalAmount );
            GS.W.Write( MineSideHitCount.Count );
            for( int i = 0; i < MineSideHitCount.Count; i++ )
                GS.W.Write( MineSideHitCount[ i ] );
        }

        if( Unit.TileID == ETileType.FIRE )
        {
            int sz = WoodAdded.Count;
            GS.W.Write( sz );
            for( int i = 0; i < WoodAdded.Count; i++ )
                GS.W.Write( WoodAdded[ i ] );
        }

        if( Unit.TileID == ETileType.ITEM )
        {
            GS.W.Write( ChestLevel );
            GS.W.Write( MiniDomeTimerCounter );
            GS.W.Write( MiniDomeTotalTime );
            GS.W.Write( ResourcePersistTotalSteps );
            GS.W.Write( ResourcePersistStepCount );
            GS.SVector3( ItemMiniDome.transform.eulerAngles );
            //int sz = BonusItemList.Count;
            //GS.W.Write( sz );
            //for( int i = 0; i < BonusItemList.Count; i++ )
            //    GS.W.Write( ( int ) BonusItemList[ i ] );
            //sz = BonusItemAmountList.Count;
            //GS.W.Write( sz );
            //for( int i = 0; i < BonusItemAmountList.Count; i++ )
            //    GS.W.Write( ( int ) BonusItemAmountList[ i ] );
            //sz = BonusItemChanceList.Count;
            //GS.W.Write( sz );
            //for( int i = 0; i < BonusItemChanceList.Count; i++ )
            //    GS.W.Write( ( int ) BonusItemChanceList[ i ] );
        }

        if( Unit.TileID == ETileType.ALGAE )
        {
            BabyData.Save( Unit );
        }
        if( Unit.TileID == ETileType.ALTAR )
        {
            GS.W.Write( RotationSpeed );
            GS.W.Write( PoleBonusAvailableAngle );
        }
        if( IsInfected )
          {
              GS.W.Write( InfectedRadius );
              GS.SVector2( InfectedSprite.scale );
          }

        #region vars
        /*
            if( Body.EffectList != null && Body.EffectList.Length > 0 )
                for( int i = 0; i < Body.EffectList.Length; i++ )
                    Body.EffectList[ i ].SetActive( false );
            
            Body.TotalMagicShield = un.Body.TotalMagicShield;
            Body.TotalMissileShield = un.Body.TotalMissileShield;
            Body.TotalMeleeShield = un.Body.TotalMeleeShield;
            Body.BaseMagicShield = un.Body.BaseMagicShield;
            Body.BaseMissileShield = un.Body.BaseMissileShield;
            Body.BaseMeleeShield = un.Body.BaseMeleeShield;
            Body.BonusMagicShield = un.Body.BonusMagicShield;
            Body.BonusMissileShield = un.Body.BonusMissileShield;
            Body.BonusMeleeShield = un.Body.BonusMeleeShield;
            Body.MagicShieldPerLevel = un.Body.MagicShieldPerLevel;
            Body.MissileShieldPerLevel = un.Body.MissileShieldPerLevel;
            Body.MeleeShieldPerLevel = un.Body.MeleeShieldPerLevel;
            Body.MagicShieldPerStar = un.Body.MagicShieldPerStar;
            Body.MissileShieldPerStar = un.Body.MissileShieldPerStar;
            Body.MeleeShieldPerStar = un.Body.MeleeShieldPerStar;

            Body.HpPerLevel = un.Body.HpPerLevel;
            Body.HpPerStar = un.Body.HpPerStar;
            Body.InvulnerabilityFactor = un.Body.InvulnerabilityFactor;
            Body.NumMushroom = un.Body.NumMushroom;
            Body.HpPerMushroom = un.Body.HpPerMushroom;
            Body.PressureAttackBonus = un.Body.PressureAttackBonus;
            Body.DexterityLevel = un.Body.DexterityLevel;
            Body.AgilityLevel = un.Body.AgilityLevel;
            Body.OrbStrikerLevel = un.Body.OrbStrikerLevel;
            Body.PerformPreMoveAttack = un.Body.PerformPreMoveAttack;
            Body.PerformPostMoveAttack = un.Body.PerformPostMoveAttack;
            Body.CooperationLevel = un.Body.CooperationLevel;
            Body.DamageSurplusLevel = un.Body.DamageSurplusLevel;
            Body.MagicShieldLevel = un.Body.MagicShieldLevel;
            Body.AmbusherLevel = un.Body.AmbusherLevel;
            Body.MemoryLevel = un.Body.MemoryLevel;
            Body.ToolBoxLevel = un.Body.ToolBoxLevel;
            Body.BerserkLevel = un.Body.BerserkLevel;
            Body.BeeHiveThrowerLevel = un.Body.BeeHiveThrowerLevel;
            Body.OriginalFireIsOn = un.Body.OriginalFireIsOn;
            Body.AvailableFireHits = un.Body.AvailableFireHits;
            Body.PsychicLevel = un.Body.PsychicLevel;
            Body.WallDestroyerLevel = un.Body.WallDestroyerLevel;
            Body.IsLoot = un.Body.IsLoot;
            Body.IsMarked = un.Body.IsMarked;
            Body.PoisonBite = un.Body.PoisonBite;
            Body.LooterLevel = un.Body.LooterLevel;
            Body.ProspectorLevel = un.Body.ProspectorLevel;
            Body.ProtectionLevel = un.Body.ProtectionLevel;
            Body.BaseMonsterLootTurns = un.Body.BaseMonsterLootTurns;
            Body.AuxStackAmount = un.Body.AuxStackAmount;
            Body.ExclusiveToAdventure = un.Body.ExclusiveToAdventure;
            Body.BigMonster = un.Body.BigMonster;
            Body.EnemyAttackBonus = un.Body.EnemyAttackBonus;
            Body.FrontalAttacks = un.Body.FrontalAttacks;
            Body.FrontSideAttacks = un.Body.FrontSideAttacks;
            Body.SideAttacks = un.Body.SideAttacks;
            Body.BackSideAttacks = un.Body.BackSideAttacks;
            Body.BackAttacks = un.Body.BackAttacks;
            Body.FreshAttackLevel = un.Body.FreshAttackLevel;
            Body.RiskyAttackLevel = un.Body.RiskyAttackLevel;
            Body.MortalJumpLevel = un.Body.MortalJumpLevel;
            Body.OpenFieldAtttackLevel = un.Body.OpenFieldAtttackLevel;
            Body.ThreatLevel = un.Body.ThreatLevel;
            Body.BaseThreatDuration = un.Body.BaseThreatDuration;
            Body.BaseFreeExitHPLimit = un.Body.BaseFreeExitHPLimit;
            Body.FirePowerLevel = un.Body.FirePowerLevel;
            Body.FireSpreadLevel = un.Body.FireSpreadLevel;
            Body.FireWoodNeeded = un.Body.FireWoodNeeded;
            Body.OutsideFireWoodAllowed = un.Body.OutsideFireWoodAllowed;
            Body.FirepitsNeededForDiscount = un.Body.FirepitsNeededForDiscount;
            Body.WoodAddedTurnCount = un.Body.WoodAddedTurnCount;
            Body.HitsTaken = un.Body.HitsTaken;
            Body.TouchCount = un.Body.TouchCount;
            Body.OriginalTouchCount = un.Body.OriginalTouchCount;
            Body.FreePlatformExit = un.Body.FreePlatformExit;
            Body.IsEternalToken = un.Body.IsEternalToken;
            Body.BarricadeForRune = un.Body.BarricadeForRune;
            Body.ScaryLevel = un.Body.ScaryLevel;
            Body.HeroNeighborTouchAdder = un.Body.HeroNeighborTouchAdder;
            Body.OriginalHitsTaken = un.Body.OriginalHitsTaken;
            Body.FireStarBonus = un.Body.FireStarBonus;
            Body.CollectorLevel = un.Body.CollectorLevel;
            Body.HasSeenTheHero = un.Body.HasSeenTheHero;
            Body.ResourcePersistance = un.Body.ResourcePersistance;
            Body.EconomyLevel = un.Body.EconomyLevel;
            Body.RealtimeMeleeAttSpeed = un.Body.RealtimeMeleeAttSpeed;
            Body.RealtimeRangedAttSpeed = un.Body.RealtimeRangedAttSpeed;
            Body.IsTiny = un.Body.IsTiny;
            Body.FirstAttackTime = un.Body.FirstAttackTime;
            Body.HitPointsRatio = un.Body.HitPointsRatio;
            Body.IsBoss = un.Body.IsBoss;
            Body.ResourceOperation = un.Body.ResourceOperation;
            Body.ExtraMiningChance = un.Body.ExtraMiningChance;
            Body.FrameDamageTaken = un.Body.FrameDamageTaken;
            Body.DeathCountFactor = un.Body.DeathCountFactor;
            Body.Animator = un.Body.Animator;
            Body.Fishing_1 = un.Body.Fishing_1;
            Body.Fishing_2 = un.Body.Fishing_2;
            Body.Fishing_3 = un.Body.Fishing_3;
            Body.Fishing_4 = un.Body.Fishing_4;
            //Body.Sprite2                   = un.Body.Sprite2;
            Body.HerbColor = un.Body.HerbColor;
            Body.HerbType = un.Body.HerbType;
            Body.OriginalHerbType = un.Body.OriginalHerbType;
            Body.HerbBonusAmount = un.Body.HerbBonusAmount;
            Body.RandomizableHerb = un.Body.RandomizableHerb;
            Body.HerbKillTimer = un.Body.HerbKillTimer;
            Body.VsRoachAttDecreasePerBaby = un.Body.VsRoachAttDecreasePerBaby;
            Body.VsHeroRoachAttackIncreasePerBaby = un.Body.VsHeroRoachAttackIncreasePerBaby;
            Body.SpawnBlocker = un.Body.SpawnBlocker;
            Body.OptionalMonster = un.Body.OptionalMonster;
            Body.SpriteColor = un.Body.SpriteColor;
            Body.OrbFogStopEnabled = un.Body.OrbFogStopEnabled;
         * Body.HookPushSteps = un.Body.HookPushSteps;
            Body.ShieldedWaspChance = un.Body.ShieldedWaspChance;
            Body.MaxShieldedWasps = un.Body.MaxShieldedWasps;
            Body.ShieldedWasp = un.Body.ShieldedWasp;
            Body.EnragedWasp = un.Body.EnragedWasp;
            Body.CocoonWasp = un.Body.CocoonWasp;
            Body.OriginalHookType = un.Body.OriginalHookType;
            Body.AddedLivesCount = un.Body.AddedLivesCount;
            Body.AuxMiningBonusAmount = un.Body.AuxMiningBonusAmount;
            Body.RaftResource = un.Body.RaftResource;
            Body.ResourceWasteTimeCounter = un.Body.ResourceWasteTimeCounter;
            Body.ResourceWasteTotalTime = un.Body.ResourceWasteTotalTime;
            Body.MudPushable = un.Body.MudPushable;
            Body.BumpTimer = un.Body.BumpTimer;
            Body.Crippled = un.Body.Crippled;
            Body.KillOnFlightEnd = un.Body.KillOnFlightEnd;
            Body.ResTriggerType = un.Body.ResTriggerType;
            Body.SpikeDamageApplied = un.Body.SpikeDamageApplied;
            Body.SpikedBoulder = un.Body.SpikedBoulder;
            Body.HookID = un.Body.HookID;

            Body.OutAreaBurningBarricadeDestroyBonus = un.Body.OutAreaBurningBarricadeDestroyBonus;
            Body.HeroAttackPositionHistory = new List<Vector2>();
            Body.HeroAttackPositionHistory.AddRange( un.Body.HeroAttackPositionHistory );

            Body.HeroAttackDirectionHistory = new List<EDirection>();
            Body.HeroAttackDirectionHistory.AddRange( un.Body.HeroAttackDirectionHistory );
            Body.ChildList = new List<Unit>();
            Body.ChildList.AddRange( un.Body.ChildList );
            Body.HasBaby = new List<bool>();
            Body.HasBaby.AddRange( un.Body.HasBaby );
            Body.BabyVariation = new List<int>();
            Body.BabyVariation.AddRange( un.Body.BabyVariation );
            Body.AltarBonusList = new List<AltarBonusStruct>();

            if( restart )
            {
                Body.OriginalIsMarked = un.Body.OriginalIsMarked;
                Body.TotHp = un.Body.TotHp;
                Body.Hp = un.Body.Hp;
                Body.TurnsToDie = un.Body.TurnsToDie;
                Body.OriginalTurnsToDie = un.Body.OriginalTurnsToDie;
                Body.OriginalThreatLevel = un.Body.OriginalThreatLevel;
                Body.AreaEnterDirection = un.Body.AreaEnterDirection;
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

                Body.OriginalHeroAttackPositionHistory = new List<Vector2>();
                Body.OriginalHeroAttackPositionHistory.AddRange( un.Body.OriginalHeroAttackPositionHistory );

                Body.OriginalHeroAttackDirectionHistory = new List<EDirection>();
                Body.OriginalHeroAttackDirectionHistory.AddRange( un.Body.OriginalHeroAttackDirectionHistory );
            }
        */ 
#endregion
    }   

    public void Load()
    {
        Lives = GS.R.ReadSingle();
        IsDead = GS.R.ReadBoolean();
        TotHp = GS.R.ReadSingle();
        BaseTotHP = GS.R.ReadSingle();
        Hp = GS.R.ReadSingle();
        AnimationFrame = GS.R.ReadInt32();
        AnimationSignal = GS.R.ReadInt32();
        AnimationTimeCounter = GS.R.ReadSingle();
        FishType = ( EFishType ) GS.R.ReadInt32();
        IsFish = GS.R.ReadBoolean();
        HookIsStuck = GS.R.ReadBoolean();
        StackAmount = GS.R.ReadSingle();
        ArrowHitCount = GS.R.ReadInt32();
        HPLostInTheTurn = GS.R.ReadSingle();
        isWorking = GS.R.ReadBoolean();
        MakePeaceful = GS.R.ReadInt32();
        MakeImovable = GS.R.ReadInt32();
        Level = GS.R.ReadSingle();
        BaseLevel = GS.R.ReadSingle();
        NumFreeAttacks = GS.R.ReadSingle();
        DamageTaken = GS.R.ReadSingle();
        HealthStatus = ( EUnitHealthStatus ) GS.R.ReadSingle();
        FireIsOn = GS.R.ReadBoolean();
        ImmunityShieldTime = GS.R.ReadSingle();

        if( Unit.UnitType == EUnitType.HERO )
        {
            RangedAttackLevel = GS.R.ReadSingle();
            MeleeAttackLevel = GS.R.ReadSingle();
            FireMasterLevel = GS.R.ReadSingle();
            DestroyBarricadeLevel = GS.R.ReadSingle();
            MeleeShieldLevel = GS.R.ReadSingle();
            MissileShieldLevel = GS.R.ReadSingle();
            MiningLevel = GS.R.ReadSingle();
            Stars = GS.R.ReadSingle();
            FishingLevel = GS.R.ReadSingle();
            for( int i = 0; i < 8; i++ )
                G.Hero.Body.Sp[ i ].BambooSize = GS.R.ReadInt32();
        }

        if( Unit.TileID == ETileType.VINES )
        {
            VineFireSpreadID = GS.R.ReadInt32();
            CreatedVine = GS.R.ReadBoolean();
            FireSourcePos = GS.LVector2();
            int sz = GS.R.ReadInt32();
            CreatedVineList = new List<int>();
            for( int i = 0; i < sz; i++ )
                CreatedVineList.Add( GS.R.ReadInt32() );
        }

        if( Unit.UnitType == EUnitType.HERO ||
            Unit.TileID == ETileType.MINE )
        {
            ShackleDistance = GS.R.ReadInt32();
            RopeConnectFatherPosList = new List<Vector2>();
            int sz = GS.R.ReadInt32();
            for( int i = 0; i < sz; i++ )
                RopeConnectFatherPosList.Add( GS.LVector2() );
            RopeConnectSonPos = GS.LVector2();
        }

        if( Unit.TileID == ETileType.MINE )
        {
            MineLeverDir = ( EDirection ) GS.R.ReadInt32();
            LeverMine = GS.R.ReadBoolean();
            BaseMiningChance = GS.R.ReadSingle();
            ExtraMiningChance = GS.R.ReadSingle();           
            MinePushSteps = GS.R.ReadInt32();
            MineLeverActivePullerPos = GS.LVector2();
            MineType = ( EMineType ) GS.R.ReadInt32();
            UPMineType = ( EMineType ) GS.R.ReadInt32();
            UPMineDir = ( EDirection ) GS.R.ReadInt32();
            MiningPrize = ( ItemType ) GS.R.ReadInt32();
            MiningBonusAmount = GS.R.ReadSingle();
            AutoRotatingMineType = ( EAutoRotateMineType ) GS.R.ReadSingle();
            ChiselSlideCount = GS.R.ReadInt32();
            ClockwiseChainPush = GS.R.ReadBoolean();
            MineGreenCrystalAmount = GS.R.ReadSingle();
            MineRedCrystalAmount = GS.R.ReadSingle();
            int sz = GS.R.ReadInt32();
            MineSideHitCount = new List<int>();
            for( int i = 0; i < sz; i++ )
                MineSideHitCount.Add( GS.R.ReadInt32() );
        }

        if( Unit.TileID == ETileType.FIRE )
        {
            int sz = GS.R.ReadInt32();
            WoodAdded = new List<bool>();
            for( int i = 0; i < sz; i++ )
                WoodAdded.Add( GS.R.ReadBoolean() );
        }

        if( Unit.TileID == ETileType.ITEM )
        {
            ChestLevel = GS.R.ReadInt32();
            MiniDomeTimerCounter = GS.R.ReadSingle();
            MiniDomeTotalTime = GS.R.ReadSingle();
            ResourcePersistTotalSteps = GS.R.ReadInt32();
            ResourcePersistStepCount = GS.R.ReadInt32();
            ItemMiniDome.transform.eulerAngles = GS.LVector3();
            //int sz = GS.R.ReadInt32();
            //BonusItemList = new List<ItemType>();
            //for( int i = 0; i < sz; i++ )
            //    BonusItemList.Add( ( ItemType ) GS.R.ReadInt32() );
            //sz = GS.R.ReadInt32();
            //BonusItemAmountList = new List<float>();
            //for( int i = 0; i < sz; i++ )
            //    BonusItemAmountList.Add( GS.R.ReadSingle() );
            //sz = GS.R.ReadInt32();
            //BonusItemChanceList = new List<float>();
            //for( int i = 0; i < sz; i++ )
            //    BonusItemChanceList.Add( GS.R.ReadSingle() );
        }

        if( Unit.TileID == ETileType.ALGAE )
        {
            BabyData.Load( Unit );
        }
        if( Unit.TileID == ETileType.ALTAR )
        {
            RotationSpeed = GS.R.ReadSingle();
            PoleBonusAvailableAngle = GS.R.ReadSingle();
        }
        if( IsInfected )
        {
            InfectedRadius = GS.R.ReadInt32();
            InfectedSprite.scale = GS.LVector2();
        }
    }
    
    public float ReceiveDamage( float damage, EDamageType type, Unit attacker, Attack attack )
    {
        if( InvulnerabilityFactor > 0 ) return 0;                                                         // Invulnerable    
        if( GetImmunityShieldState() ) return 0;                                                          // Immunity shield active

        float bonus = 0;

        if( attack != null )
        {
            damage = attack.TotalDamage;
            if( attack.DamageType == EDamageType.BEEHIVE )                                                // beehive attack power
                damage = Map.I.Hero.MeleeAttack.TotalDamage / 2;

            if( attack.TargettingType == ETargettingType.SPELL )                                          // Spell Attack Damage
                damage = Spell.GetSpellDamage( attack );

            if( Unit.TileID == ETileType.QUEROQUERO )                                                     // Quero quero hp damage based on precision calculation
            if( TotHp > 1 )
                damage = Attack.QQHPDamage;
        }

        if( attacker.Body.PoisonBite )                                                                    // Poison Bite
            if( Map.I.Hero.Body.IsHealty() == false )
                if( Unit.UnitType == EUnitType.HERO )
                    damage = 999999;

        if( CheckEvasion( type, attacker ) ) return 0;                                                    // Check Evasion

        float shield = CalculateDefenseDeduction( type, attacker );                                       // Defense calculation

        if( CheckHeroShield( attacker ) ) return 0;                                                       // Hero Shield

        damage -= shield;

        bonus += CheckCornering( attacker, type );

        bonus += CheckAmbush( type, attacker );

        bonus += CheckDragon( attacker, attack, ref damage );

        bonus += CheckRoach( attacker, attack, ref damage );

        bonus += CalculateSprinterAttackBonus( attacker.Control.SprinterLevel, type );

        bonus += CalculateFireAttackBonus( attacker );

        bonus += CalculatePlatformAttackBonus( attacker );

        bonus += CalculateAgilityAttackBonus( type, attacker );

        bonus += CalculateSlimeBonus();

        damage += Util.Percent( bonus, damage );                                                          // Apply bonus att

        if( Map.Stepping() == false && attack )
        {
            damage = Util.Percent( attack.GetRealtimeDamageFactor(), damage );                            // Realtime Damage Reduction
        }

        damage += CheckDamageSurplus( attack );                                                           // Add Damage Surplus

        if( damage < 0 ) damage = 0;

        if( Unit.TileID == ETileType.MOSQUITO )                                                           // Poisoner restriction
        {
            if( type == EDamageType.RANGED ) return 0;
        }

        if( attacker.TileID == ETileType.MOSQUITO )                                                       // Poisoner Frendly fire attack Rate 
            if( Unit.UnitType == EUnitType.MONSTER )
            {
                damage = Util.Percent( Map.I.RM.RMD.PoisonerFriendlyFireAttackRate, damage );
            }

        int lev = Map.I.Hero.GetLevel( EPerkType.BERSERKLEVEL );                                          // Berserk Attack Power
        if( lev >= 1 && Attack.BerserkAttack )
            damage = Util.Percent( HeroData.I.BersekerAttackPower[ lev ], damage );

        if( Unit.UnitType == EUnitType.HERO )                                                             // Invalidates Perfect Area
            if( damage > 0 && Map.I.CurrentArea != -1 )
                Map.I.CurArea.Perfect = false;

        if( Manager.I.GameType == EGameType.CUBES )                                                       // Dungeon Damage Reduction
        {
            if( Unit.UnitType == EUnitType.HERO )
                if( type != EDamageType.BLEEDING )
                    damage = Util.Percent( Map.I.RM.RMD.MonsterDamageRate, damage );

            if( Unit.UnitType == EUnitType.HERO )
                if( Helper.I.InvunerableHero ) damage = 0;
        }

        if( Manager.I.GameType == EGameType.CUBES )
            if( Map.I.RM.RMD.ScarabPunishment >= 0 && attacker.TileID == ETileType.SCARAB )               // Cubes Scarab Bite: exit area punishment       
                damage = Map.I.RM.RMD.ScarabPunishment;

        if( Attack.InvalidateAttack ) return 0;

        damage = Util.Percent( InflictedDamageRate, damage );                                             // Monster specifica damage multiplier

        bool spell = Spell.SpellUsage( attack, this.Unit );                                               // Spell or attack
        if( spell ) { Attack.InvalidateAttack = true; return 0; }
        
        Hp -= damage;                                                                                     // Apply Damage
        
        DamageTaken += damage;

        FrameDamageTaken += damage;

        HPLostInTheTurn += damage;

        Attack.AttackMade = true;
        Map.I.CountRecordTime = true;
        UI.I.ForceUpdateUI = true;

        Unit.Control.WakeUp();                                                                           // wakes up unit

        if( damage > 0 )
        {
            if( Attack.CurrentHit >= 1 )
                Attack.DuplicatorAttackList.Add( attack.ID );                                            // Attack duplicator will be used

            HitsTaken++;
            ThreatLevel = 0;                                                                             // Invalidates Threat

            if( BigMonster )                                                                             // Sticky Arrow Animation
            if( type == EDamageType.RANGED )
            {
                float num = 10 - ( Util.GetPercent( Hp, TotHp ) / 10 );
                num = Mathf.Clamp( num, 0, ArrowList.Length - 1 );
                for( int i = 0; i < num; i++ )
                    ArrowList[ i ].gameObject.SetActive( true );        
            }            
            Map.I.TimeKeyPressing = 0;
        }

        CreateDamageAnimation( Unit.Pos, damage, attacker, type );

        UpdateUnitHealthStatus();

        if( Unit.Body.BigMonster )                                                                        // Fill Surplus Buffers
        {
            if( Hp < 0 )
            {
                if( attack )
                if( Unit.Control.RealtimeSpeedFactor > 0 )
                    attack.RTDamageSurplus = Hp * -1;
                else
                    attack.DamageSurplus = Hp * -1;
            }
        }

        Hp = Mathf.Clamp( Hp, 0, TotHp );                                                                 // Clamp hp boundaries

        UpdatePunchEffect( attacker, type );

        if( type != EDamageType.FIRE )                                                                    // Invalidates Allburt
        {
            int ar = Map.I.GetPosArea( Unit.Pos );
            if( ar != -1 )
                Quest.I.CurLevel.AreaList[ ar ].AllBurntAvailable = false;
        }

        if( Unit.UnitType == EUnitType.HERO )
        {
            MasterAudio.PlaySound3DAtVector3( "Hero Damage", transform.position );
            if( Unit.Control.PathFinding.Path.Count > 0 ) Unit.Control.PathFinding.Path.Clear();
        }
        else
        if( Unit.UnitType == EUnitType.MONSTER )
            {
                CheckArrowHit( type );

                MasterAudio.PlaySound3DAtVector3( "Monster Hit", transform.position );

                if( attack != null && attack.DamageType == EDamageType.BEEHIVE )
                    MasterAudio.PlaySound3DAtVector3( "Bee Attack", transform.position );
                Map.I.Hero.Body.NumFreeAttacks = 0;

                if( Unit.TileID == ETileType.SLIME )                                                           // Slime shot pass through
                    Attack.ShotPassThroughCount++;

                if( Unit.TileID == ETileType.SCORPION )                                                         // Stepper baat blocked by Hero melee
                if( attack.DamageType == EDamageType.MELEE )
                {
                    Attack.AttackedByHeroMelee = true;
                    Vector3 pun = Util.GetTargetUnitVector( attacker.Pos, attacker.GetFront() );               // Melee att animation
                    iTween.PunchPosition( attacker.Graphic.gameObject, pun * 0.8f, 1f );
                }

                Unit ga = Map.I.GetUnit( ETileType.WATER, Unit.Pos );
                if( ga )
                if( Controller.GetRaft( Unit.Pos ) == null )
                {
                    Map.I.OverWaterAttackTimer = 0;                                                           // Over Water Attack Speed Bonus
                }

                if( Map.I.RM.RMD.LimitedArrowPerCube != -1 )                                                  // Limited arrows # decrement
                if( type == EDamageType.RANGED )
                if( attack.TargettingType == ETargettingType.MISSILE ) 
                    Item.AddItem( Inventory.IType.Inventory, ItemType.Res_Bow_Arrow, -1 );

                if( Map.I.RM.RMD.LimitedMeleeAttacksPerCube != -1 )                                           // Limited Melee # decrement
                if( type == EDamageType.MELEE )
                    Item.AddItem( Inventory.IType.Inventory, ItemType.Res_Melee_Attacks, -1 );
            }


        if( Hp <= 0 )                                                                                         // HP Bellow 0
        {
            Lives--;                                                                                          // Life Decrement

            if( attack && attack.TargettingType == ETargettingType.SPELL )                                    // Hook stuck check
            {
                Spell sp = G.Hero.Body.Sp[ attack.ID ];
                if( Spell.IsHook( sp.Type ) )
                if( Lives >= 1 )
                {
                    Spell.StickHook( sp, Unit, false );                                                       // moster still alive, hook gets stuck                 
                }
                else
                {
                    Spell.ChargeItem( attack, ItemType.Res_Hook_CW );
                    CreateBloodSpilling( Unit.Pos );
                }
            }

            if( Lives >= 1 )                                                                                  // Only Life Lost
            {
                Hp = TotHp;
                if( attack ) attack.AttackResult = 1;
            }
            else                                                                                              // Unit Dies
            {
                if( Unit.UnitType == EUnitType.HERO )
                {
                    Map.I.HeroIsDead = true;

                    if( Manager.I.GameType == EGameType.CUBES )
                    {
                        bool spr = false;
                        if( attacker.TileID == ETileType.IRON_BALL ) spr = true;
                        if( attacker.TileID == ETileType.BOUNCING_BALL ) spr = true;
                        if( attacker.TileID == ETileType.DRAGON1 ) spr = true;
                        if( attacker.TileID == ETileType.ALTAR ) spr = true;
                        Map.I.StartCubeDeath( true, spr );                                                             // start cube death
                    }

                    MasterAudio.PlaySound3DAtVector3( "Hero Death", transform.position );                              // Hero Death FX
                }
                else
                if( Unit.UnitType == EUnitType.MONSTER )
                    {
                        bool res = UpdateWaspSpecial( attacker );                                                     // Specific wasp and boomerang stuff                        
                        if( res ) return damage;

                        if( Unit.ValidMonster )
                        if( Unit.TileID == ETileType.ROACH )
                            MasterAudio.PlaySound3DAtVector3( "Roach Death", transform.position );
                        else
                        {
                            int id = 3; //Random.Range( 1, 4 );
                            MasterAudio.PlaySound3DAtVector3( "Death " + id, transform.position );   
                        }

                        if( attacker.ProjType( EProjectileType.FIREBALL ) )                              // Destroys fireball after attack
                            attacker.Body.KillOnFlightEnd = true;

                        int olda = Map.I.GetPosArea( Map.I.Hero.Pos );
                        int newa = Map.I.GetPosArea( Unit.Pos );
                        if( Unit.ValidMonster ) CreateBloodSplat();                                     // Blood Splat
                        if( Unit.TileID == ETileType.MOTHERWASP ) 
                            Map.I.KillList.Add( Unit );
                        else
                            Unit.Kill();
                    }
            }
        }

        if( Manager.I.GameType == EGameType.CUBES )
        if( Map.I.RM.RMD.ScarabPunishment >= 0 &&
            attacker.TileID == ETileType.SCARAB || 
            attacker.TileID == ETileType.SPIDER || 
            attacker.TileID == ETileType.SCORPION )                                                               // Dungeon Scarab Bite: exit area punishment    
            {
                Vector3 pun = Util.GetTargetUnitVector( attacker.Pos, Unit.transform.position );
                float amt = 0.8f;

                if( attacker.Body.IsInfected && attack.TargettingType == ETargettingType.INFECTION )
                {
                    amt = Vector3.Distance( Unit.transform.position, attacker.transform.position );
                    MasterAudio.PlaySound3DAtVector3( "Roach Glue", attacker.transform.position );
                    Map.I.CreateLightiningEffect( null, null, "keys", attacker.Pos.x,                            // Lighting FX
                    attacker.Pos.y, Unit.Pos.x, Unit.Pos.y );
                    Map.I.CreateExplosionFX( Unit.Pos );                                                         // Explosion FX
                    Map.I.SetHeroDeathTimer( .5f );
                    UpdateHealthBar();
                    Attack.ResetAttackCounter = false;
                    return damage;
                }
                else
                    iTween.PunchPosition( attacker.Graphic.gameObject, pun * amt, 1f );
                Map.I.StartCubeDeath();
            }

        UpdateHealthBar();
        return damage;
    }
    public bool UpdateWaspSpecial( Unit attacker )
    {
        List<Unit> fl = Map.I.GetFUnit( Unit.Pos );
        List<Unit> left = Map.I.GetFUnit( Unit.Pos + Util.GetRelativePosition( G.Hero.Dir, EDirection.W ) );
        List<Unit> right = Map.I.GetFUnit( Unit.Pos + Util.GetRelativePosition( G.Hero.Dir, EDirection.E ) );
        bool ok = false;
        if ( left != null )
        for( int i = 0; i < left.Count; i++ )
        if ( left[ i ].TileID == ETileType.WASP ) ok = true;
        if ( ok == false ) left = null;
        ok = false;
        if ( right != null )
        for( int i = 0; i < right.Count; i++ )
        if ( right[ i ].TileID == ETileType.WASP ) ok = true;
        if ( ok == false ) right = null;

        if( fl != null && Unit.TileID == ETileType.WASP )                                              // Boomerang and Cocoon Wasp area kill
        {
            bool cocoon = false;
            bool boom = false;
            for( int i = 0; i < fl.Count; i++ )
            {
                if( fl[ i ].TileID == ETileType.WASP &&
                    fl[ i ].Body.CocoonWasp ) cocoon = true;
                if( fl[ i ].ProjType( EProjectileType.BOOMERANG ) ) boom = true;
            }
            if( cocoon || boom )
            {
                for( int i = fl.Count - 1; i >= 0; i-- )
                if ( fl[ i ].TileID == ETileType.WASP )
                if ( fl[ i ] != Unit ) Map.I.KillList.Add( fl[ i ] );
            }
            if( cocoon )
            {
                for( int r = 1; r < 3; r++ )
                {
                    Vector2 aux = Unit.Pos + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * r;         // Coocon back rows wasp death
                    List<Unit> auxl = Map.I.GetFUnit( aux );
                    int count = 0;
                    if ( auxl != null )
                    for( int i = 0; i < auxl.Count; i++ )
                    if ( auxl[ i ].TileID == ETileType.WASP )
                    {
                        if( count++ == 0 ) CreateBloodSplat( aux );
                        Map.I.KillList.Add( auxl[ i ] );
                    }
                }

                if( left != null || right != null )                                                 // If theres a wasp to the left or gight of a coocooned wasp, relocates cocoon randomly
                {
                    Vector2 back = Unit.Pos + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ];
                    List<Unit> waspl = new List<Unit>();
                    for( int i = 0; i < 8; i++ )
                    {
                        Vector2 aux = Unit.Pos + Manager.I.U.DirCord[ i ];
                        List<Unit> tgl = Map.I.GetFUnit( aux );
                        if ( tgl != null )
                        for( int j = 0; j < tgl.Count; j++ )
                        if ( tgl[ j ].TileID == ETileType.WASP )
                        if ( tgl[ j ].Body.CocoonWasp == false )
                        if ( aux != back )
                        {
                            waspl.Add( tgl[ j ] );
                            break;
                        }
                    }
                    if( waspl != null &&  waspl.Count > 0 )                                          // Adds cocoon to a random neighbor wasp
                    {
                        int id = Random.Range( 0, waspl.Count );
                        waspl[ id ].SetSpecialWasp( 2, waspl[ id ].Control.Mother );
                    }
                }
            }
            if( cocoon || boom )
                MasterAudio.PlaySound3DAtVector3( "Explosion 1", Unit.Pos );               // Sound FX
        }
        if( attacker.ProjType( EProjectileType.BOOMERANG ) )
            Attack.BoomerangAttack = true;

        if( attacker.ProjType( EProjectileType.BOOMERANG ) )
        {
            if( fl != null )
            for( int i = 0; i < fl.Count; i++ )
            if ( fl[ i ].TileID == ETileType.MOTHERWASP )                          // To avoid bugs, just add these to killlist
            {
                Map.I.KillList.Add( fl[ i ] );
                return true;
            }
        }
        return false;
    }
    
    public bool CheckHeroShield( Unit attacker )
    {
        if( attacker.UnitType != EUnitType.MONSTER ) return false;
        if( attacker.ProjType( EProjectileType.BOOMERANG ) ) return false;
        if( attacker.ProjType( EProjectileType.FIREBALL ) ) return false;
        if( attacker.ProjType( EProjectileType.MISSILE ) ) return false;
        if( attacker.Control.IsFlyingUnit ) return false;                                                 // flying is calculated elsewhere            
        Vector2 uPos = attacker.Pos;
        if( attacker.Body.IsInfected ) 
            uPos = Map.I.InfectedScarabAttackSource;
        EDirection atdr = Util.GetTargetUnitDir( Unit.Pos, uPos );

        int shield = GetShieldID( atdr, G.Hero.Dir );                                                     // Get Shield ID

        if( shield >= 0 && shield < 8 )                                                                   // Shield impact
        if( G.HS.ActiveHeroShield[ shield ] )
        {
            int drr = ( int ) Util.RotateDir( ( int ) Unit.Dir, shield );
            Vector2 shieldpos = Unit.Pos + Manager.I.U.DirCord[ drr ];                                    // Shield tile

            if( attacker.MeleeAttack )
                attacker.MeleeAttack.SpeedTimeCounter = 0;                                                // invalidade melee att
            if( attacker.RangedAttack )
                attacker.RangedAttack.SpeedTimeCounter = 0;                                               // invalidade ranged att               
            if( attacker.InfectionAttack )
                attacker.InfectionAttack.SpeedTimeCounter = 0;                                           // invalidade Infection att         
            attacker.Control.SpeedTimeCounter = 0;

            UseShield( shield, attacker );                                                                // Use Shield

            if( attacker.Body.IsInfected )
            {
                Map.I.CreateLightiningEffect( null, null, "Shield", attacker.Pos.x,                       // Infected Lighting FX
                attacker.Pos.y, shieldpos.x, shieldpos.y );
            }

            Attack.InvalidateAttack = true;
            Map.I.CreateExplosionFX( shieldpos, "Fire Explosion", "" );                                   // Explosion FX
            return true;
        }
        return false;
    }
    public static void UseShield( int shield, Unit attacker )
    {
        int count = 0;
        for( int i = 0; i < Map.I.RM.HeroSector.ActiveHeroShield.Count; i++ )
        if( Map.I.RM.HeroSector.ActiveHeroShield[ i ] ) count++;

        float amt = Item.GetNum( ItemType.Res_Hero_Shield );
        if( amt <= count )
        {
            Map.I.RM.HeroSector.ActiveHeroShield[ shield ] = false;                                   // remove shield if low on numbers

            if( amt == 1 )                                                                            // Stops eletric fx loop sound if no shield                           
                MasterAudio.StopAllSoundsOfTransform( G.Hero.transform );
        }
        Item.AddItem( ItemType.Res_Hero_Shield, -1 );
        MasterAudio.PlaySound3DAtVector3( "Eletric Shock", G.Hero.Pos );                              // Sound fx

        if( attacker.Control.IsFlyingUnit == false )
        {
            iTween.ShakeScale( attacker.Spr.gameObject, new Vector3( .4f, .4f, 0 ), 1 );              // shock FX
        }

        List<float> ls = null;                                                                        
        if( attacker.TileID == ETileType.PROJECTILE )
        {
            attacker = attacker.Control.Mother;
            ls = attacker.Md.ProjectileDestroyHeroShieldChance;                                      // Use this var for bolts or other projectiles
        }
        else
        {
            ls = attacker.Md.MonsterDestroyHeroShieldChance;                                         // Monster Destroy hero shield after use      
        }
        if( ls != null && ls.Count > 0 )
        {
            float chc = 0;
            if( count < ls.Count ) 
                chc = ls[ count ];                                                                   // Chance depends on the amount of shields hero has around him

            if( Util.Chance( chc ) )
            {
                G.HS.ActiveHeroShield[ shield ] = false;                                             // deactivates the shield
            }
        }     
    }

    public int GetShieldID( EDirection atdr, EDirection herodir )
    {
        int shield = -1;
        for( int tm = 0; tm <= 4; tm++ )                                                // Check shield relative position
        {
            EDirection left = ( EDirection ) Util.RotateDir( ( int ) atdr, -tm );
            if( herodir == left )
            {
                shield = tm;
                if( tm == 4 ) shield = 4;
                if( tm == 0 ) shield = 0;
                break;
            }
            EDirection right = ( EDirection ) Util.RotateDir( ( int ) atdr, tm );
            if( herodir == right )
            {
                shield = 8 - tm;
                break;
            }
        }
        return shield;
    }

    public void UpdateUnitHealthStatus()
	{
		//if( this.Bar == null ) return;
		float perc = 100 * Hp / TotHp;
		Color col = Color.green;
		if( perc > 66.6666f ) { HealthStatus = EUnitHealthStatus.HEALTHY; col = Color.green;  } else
		if( perc > 33.3333f ) { HealthStatus = EUnitHealthStatus.DAMAGED; col = Color.yellow; } else
		{ HealthStatus = EUnitHealthStatus.DYING;   col = Color.red;    }
		//Bar.Spr.color = col;
	}

	public void UpdateHealthBar()
	{
        if( Unit == null ) return;
		if( Unit.HealthBar == null ) return;
		Unit.HealthBar.gameObject.SetActive( true );
        if( Hp == TotHp )
        {
            Unit.HealthBar.gameObject.SetActive( false );
            Unit.HealthBar.valueCurrent = 0;
        }
		Unit.HealthBar.valueMax     = (int) TotHp;
		Unit.HealthBar.valueCurrent = (int) Hp;
		Unit.HealthBar.valueMin = 0;
	}

    public void CreateDamageAnimation( Vector2 pos, float dmg, Unit attacker, EDamageType type = EDamageType.NONE, bool force = false )
    {
        if( force == false )
        if( type == EDamageType.BLEEDING ) return;
        Color col = new Color( 0f, 1f, 0f, 1f );
        bool moveright = false;
        //string msg = "hit: " + dmg.ToString( "0.#" );
        string msg = "" + dmg.ToString( "0.#" );
        if( type == EDamageType.FIRE )
            msg = "Burn: " + dmg.ToString( "0.#" );
        if( Unit.TileID == ETileType.WASP       ||
            attacker.TileID == ETileType.SPIDER ||
            attacker.TileID == ETileType.SCARAB ) msg = "";

        if( Unit.UnitType == EUnitType.HERO )
        {
            moveright = true;
            col = new Color( 1f, 0f, 0f, 1f );
        }
        Vector3 orig = Unit.transform.position + new Vector3( Random.Range( -1f, 1f ), Random.Range( -1f, 1f ) );
        //if( attacker.TileID == ETileType.DRAGON ) msg = "";

        if( msg != "" ) 
            Message.CreateMessage( ETileType.NONE, ItemType.Res_HP, msg, orig, col, moveright, true, 4, 0, 0, 50 );
        if( force == false )
        if( Unit.ValidMonster == false ) return;
        CreateBloodSpilling( pos );

        dmg = Mathf.Clamp( dmg, 0, 99999 );
        if( dmg >= 9999 ) msg = "Skin Crawl!";
    }
    public void CreateBloodSpilling( Vector2 pos )
    {
        CreateBloodSpilling( pos, Unit );
    }
    public static void CreateBloodSpilling( Vector2 pos, Unit un )
    {
        Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Blood Spilling" );
        tr.position = new Vector3( pos.x, pos.y, -6 );
        tr.eulerAngles = new Vector3( 0, 0, Random.Range( 0, 360 ) );
        ParticleSystem pr = tr.gameObject.GetComponent<ParticleSystem>();
        pr.Stop();
        pr.Play();

        tr = PoolManager.Pools[ "Pool" ].Spawn( "Explosion 1" );
        tr.position = new Vector3( pos.x, pos.y, -6 );
        tr.eulerAngles = new Vector3( 0, 0, Random.Range( 0, 360 ) );
        pr = tr.gameObject.GetComponent<ParticleSystem>();
        pr.Stop();
        pr.Play();
    }

    public void CreateBloodSplat()
    {
        Vector3 pos = new Vector3( transform.position.x, transform.position.y, -1.85f );
        CreateBloodSplat( pos );
    }

	public static void CreateBloodSplat( Vector2 pos )
    {
		Vector3 pt = new Vector3( pos.x, pos.y, -1.85f );
        Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Blood" );
        tr.position = pt;
        Animation ani = tr.GetComponent<Animation>();
        ani.Play();
	}
    public static void CreateDeathFXAt( Vector2 pos, string sound = "" )
    {
        CreateBloodSplat( pos );
        CreateBloodSpilling( pos, null );
        if( sound != "" )
            MasterAudio.PlaySound3DAtVector3( sound, G.Hero.Pos );
    }

    public float CalculateSprinterAttackBonus( float lev, EDamageType type )
    {
        if( type == EDamageType.FIRE ) return 0;
        if( type == EDamageType.BLEEDING ) return 0;

        int turn = 0;      
        if( Map.I.CurrentArea != -1 ) turn = ( int ) Map.I.CurArea.AreaTurnCount;

        float bn = HeroData.I.SprinterAttackBonus[ ( int ) lev ] - ( ( turn ) * 10 );

        if( bn < 0 ) bn = 0;
        UI.I.SprinterBonus = bn;

        if( bn > 0 )
            UI.I.SprinterInfo = " + Sprinter (" + bn + "%)";

        return bn;
    }    
    public bool CheckEvasion( EDamageType type, Unit attacker )
    {
        bool ok = false;
        float cost = 1;

        if( Unit.UnitType != EUnitType.HERO ) return false;

        bool move = false;
        float discount = 0;                                                                            
        if( Map.I.Hero.Control.LastMoveType != EMoveType.STILL )                                         // Hero has moved?
        if( Map.I.Hero.Control.LastMoveType != EMoveType.ROTATE )
           {
               move = true;
               if( Unit.Control.EvasionLevel >= 5 )                                                      // 1 Point Discount
                   discount = 1;
           }

        if( Map.I.Hero.Control.EvasionPoints < 1 ) return false;
        
        bool monsterAttacked = false;
        if( attacker.Body.HPLostInTheTurn > 0 ) 
            monsterAttacked = true;

        Unit fire = Map.I.GetUnit(  ETileType.FIRE, attacker.Pos );
        if( fire && fire.Body.FireIsOn ) monsterAttacked = true;

        string evtype = "running";

        Vector2 s = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.S, 1 );                          // From South

        if( Unit.Control.EvasionLevel >= 1 )  
        if( attacker.Pos == s )
        if( monsterAttacked == false )
        if( move )
        {
            goto Evade;
        }
        
        Vector2 se = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.SE, 1 );                        // From South East
        Vector2 sw = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.SW, 1 );                        // From South West

        if( Unit.Control.EvasionLevel >= 2 )
        if( move )
        if( monsterAttacked == false )
        if( attacker.Pos == se  ||
            attacker.Pos == sw ) goto Evade;

        bool narrow = Map.CheckNarrowPassage( Unit.Pos, attacker.Pos );                                  // narrow passage

        if( Unit.Control.EvasionLevel >= 3 )  
        if( narrow && Map.I.Hero.Control.EvasionPoints >= 4 - discount )  
        {
            evtype = "narrow passage";
            cost = 4 - discount;
            goto Evade;
        }
        
        Vector2 s2 = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.S, 2 );                         // Back tall tiles
        Vector2 s3 = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.S, 3 );        

        if( Unit.Control.EvasionLevel >= 4 )
        if( Map.I.Hero.Control.EvasionPoints >= 2 - discount ) 
        if( Map.IsWall( s ) && Map.IsWall( s2 ) )
        {
            evtype = "2 tall tiles";
            cost = 4 - discount;
            if( Map.IsWall( s3 ) )
            {
                evtype = "3 tall tiles";
                cost = 2 - discount;
            }

            if( Unit.Control.EvasionPoints >= cost )
            goto Evade;
        }
        

        Vector2 e = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.E, 1 );                         // From East
        Vector2 w = G.Hero.Pos + G.Hero.GetRelativePosition( EDirection.W, 1 );                         // From West

        if( Unit.Control.EvasionLevel >= 7 )
            if( move )
                if( monsterAttacked == false )
                    if( attacker.Pos == e ||
                        attacker.Pos == w ) goto Evade;

        return false;

        Evade:
        {
            Map.I.CurArea.EvasionPointsUsed += cost;
            //Message.CreateMessage( ETileType.HITPOINTS, evtype + " Evasion!    (-" + cost + "P)",
            //Unit.Pos, new Color( 0, 1, 0, 1 ), true, true, 7 );
            return true;
        }
    }

    public float CheckAmbush(  EDamageType type, Unit attacker )
    {
        if( type == EDamageType.FIRE ) return 0;
        if( type == EDamageType.BLEEDING ) return 0;

        if( attacker.UnitType != EUnitType.HERO ) return 0;
        if( Map.I.Hero.Body.AmbusherLevel < 1 ) return 0;

        float num = 0;
        for( int d = 0; d < 8; d++ )
        {
            Vector2 aux = Unit.Pos + ( Manager.I.U.DirCord[ ( int ) d ] * 1 );
            if( Map.PtOnMap( Map.I.Tilemap, aux ) )
            {
                if( Map.I.Gaia[ ( int ) aux.x, ( int ) aux.y ] &&
                    Map.I.Gaia[ ( int ) aux.x, ( int ) aux.y ].TileID == ETileType.FOREST ) num++;
                else
                    if( Map.I.Hero.Body.AmbusherLevel >= 3 )
                        if( Map.I.Unit[ ( int ) aux.x, ( int ) aux.y ] != null )
                            if( Map.I.Unit[ ( int ) aux.x, ( int ) aux.y ].ValidMonster ) num++;
            }

            aux = Map.I.Hero.Pos + ( Manager.I.U.DirCord[ ( int ) d ] * 1 );
            if( Map.PtOnMap( Map.I.Tilemap, aux ) )
            {
                if( Map.I.Hero.Body.AmbusherLevel >= 5 )
                if( Map.I.Gaia[ ( int ) aux.x, ( int ) aux.y ] )
                if( Map.I.Gaia[ ( int ) aux.x, ( int ) aux.y ].TileID == ETileType.WATER ) num++;
            }
        }

        float bonus = HeroData.I.AmbusherBonusPerTile[ ( int ) Map.I.Hero.Body.AmbusherLevel ];

        UI.I.AmbushBonus = bonus * num;
        UI.I.AmbushInfo = " - (Ambush Bonus: " + num + " Tiles x " + bonus + "% = " + bonus * num + "%) ";

        return bonus * num;
    }

	public float CalculateDefenseDeduction( EDamageType type, Unit attacker )
	{
		float shield = 0;
        if( Unit.Control.IsFlyingUnit ) return 0;

        if( attacker.TileID == ETileType.MOSQUITO )                                                                                 // Poisoner att monster
            if( Unit.UnitType == EUnitType.MONSTER ) return 0;

        if( type == EDamageType.FIRE ) return 0;
        if( type == EDamageType.BLEEDING ) return 0;
        float[ ] angle = new float[ 5 ];

        if( attacker.UnitType == EUnitType.MONSTER )
        {
            angle = Map.I.RM.RMD.HeroShieldPerAttackAngle;
            if( type == EDamageType.MELEE )
            {
                float per = ( int ) MeleeShieldLevel * Map.I.RM.RMD.HeroMeleeShieldBonusPerLevel;
                shield = Util.Percent( per, TotalMeleeShield );                                                                    // Melee Shield deduction 
                if( MeleeShieldLevel < 1 ) shield = 0;
            }
            if( type == EDamageType.RANGED )
            {
                float per = ( int ) MissileShieldLevel * Map.I.RM.RMD.HeroRangedShieldBonusPerLevel;
                shield = Util.Percent( per, TotalMissileShield );                                                                  // Missile Shield deduction 
                if( MissileShieldLevel < 1 ) shield = 0;
            }
        }

        if( attacker.UnitType == EUnitType.HERO )
        {
            if( BigMonster == false ) return 0;
            angle = Map.I.RM.RMD.MonsterShieldPerAttackAngle;
            if( type == EDamageType.MELEE )
            {
               // float per = ( int ) MeleeShieldLevel * Map.I.RM.RMD.HeroMeleeShieldBonusPerLevel;
                shield = Util.Percent( 100, TotalMeleeShield );                                                                        // Melee Shield deduction 
                if( MeleeShieldLevel < 1 ) shield = 0;
            }
            if( type == EDamageType.RANGED )
            {
                // float per = ( int ) MissileShieldLevel * Map.I.RM.RMD.HeroMeleeShieldBonusPerLevel;
                shield = Util.Percent( 100, TotalMissileShield );                                                                     // Melee Shield deduction 
                if( MissileShieldLevel < 1 ) shield = 0;
            }
        }

        UI.I.Shield = shield;
		EDirection dr = GetPosRelativeDir( Unit.Dir, Attack.AttackOrigin );
        
        //if( attacker.UnitType == EUnitType.HERO )                                                                                   // No agility enough for shield piercing yet
        //if( attacker.Body.AgilityLevel < 1 ) dr = EDirection.N;

        if( dr == EDirection.S )                                                                                                    // Relative position shield deduction
        {
            shield = Util.Percent( angle[ 4 ], shield ); 
            UI.I.AttackType = "Back"; UI.I.ShieldReduction = 0;
            BackAttacks++;
        }
        if( dr == EDirection.SW || dr == EDirection.SE )
        {
            shield = Util.Percent( angle[ 3 ], shield ); 
            UI.I.AttackType = "Backside"; UI.I.ShieldReduction = 25;
            BackSideAttacks++;
        }
        if( dr == EDirection.W || dr == EDirection.E )
        {
            shield = Util.Percent( angle[ 2 ], shield ); 
            UI.I.AttackType = "Side"; UI.I.ShieldReduction = 50;
            SideAttacks++;
        }
        if( dr == EDirection.NW || dr == EDirection.NE )
        {
            shield = Util.Percent( angle[ 1 ], shield ); 
            UI.I.AttackType = "Frontside"; UI.I.ShieldReduction = 75;
            FrontSideAttacks++;
        }
        if( dr == EDirection.N )
        {
            shield = Util.Percent( angle[ 0 ], shield ); 
            UI.I.AttackType = "Frontal"; UI.I.ShieldReduction = 100;
            FrontalAttacks++;
        }

		return shield;
	}
    
    float CheckDamageSurplus( Attack attack )
    {
        if( BigMonster == false ) return 0;
        if( attack == null ) return 0;
        return 0;

        if( Unit.Control.RealtimeSpeedFactor <= 0 )
        {
            UI.I.SurplusBonus = Util.Percent( HeroData.I.DamageSurplusBonus[ ( int ) G.Hero.Body.DamageSurplusLevel ], attack.DamageSurplus );
            if( UI.I.SurplusBonus != 0 )
                UI.I.SurplusInfo = " + Surplus: " + UI.I.SurplusBonus + " ";
            attack.DamageSurplus = 0;
            return UI.I.SurplusBonus;
        }
        else
        {
            UI.I.SurplusBonus = Util.Percent( HeroData.I.DamageSurplusBonus[ ( int ) G.Hero.Body.DamageSurplusLevel ], attack.RTDamageSurplus );
            attack.RTDamageSurplus = 0;
            return UI.I.SurplusBonus;
        }        
    }

    public float CheckCornering( Unit attacker, EDamageType type )
    {
        float bonus = 0;
        if( type == EDamageType.FIRE ) return 0;
        if( type == EDamageType.BLEEDING ) return 0;
        
		if( attacker.Control.MonsterCorneringLevel >= 1 &&                                                // Cornering bonus
				Unit.Control.IsBeingPushedAgainstObstacle )
		{
            UI.I.ConeringBonus = HeroData.I.MonsterCorneringBonus[ ( int )
                                  attacker.Control.MonsterCorneringLevel ];
            bonus += UI.I.ConeringBonus;
            UI.I.CorneringInfo = " + Cornering: " + UI.I.ConeringBonus + "% ";
		}

        return bonus;
    }

    public float CheckRoach( Unit attacker, Attack attack, ref float damage )
    {
        float bonus = 0;
        if( attacker.UnitType == EUnitType.HERO )                                                       // Hero Attacks Roach
        {
            if( Unit.TileID != ETileType.ROACH ) return 0;

            int dir = ( int ) EDirection.NONE;                                                           // Roach Babies attacked
            for( int dr = 0; dr < 8; dr++ )
            for( int rg = 1; rg < 8; rg++ )  
            {
                Vector2 aux = Unit.Pos + ( Manager.I.U.DirCord[ dr ] * rg );
                if( Sector.GetPosSectorType( aux ) == Sector.ESectorType.GATES ) break;

                if( aux == G.Hero.Pos )
                {
                    dir = dr;
                    goto outloop;
                }
            }
            outloop: { }

            dir -= ( int ) Unit.Dir;
            if( ( int ) dir < 0 ) dir += 8;
            if( ( EDirection ) dir == EDirection.NONE ) G.Deb( "bad Roach babies pos" );

            bool killbaby = true;
            if( dir == ( int ) EDirection.N )                                                           // Frontal baby can only be killed after the others
            {
                int bc = 0;
                for( int i = 0; i < 8; i++ )
                if( BabySprite[ i ].gameObject.activeSelf ) bc++;
                if( bc >= 2 ) killbaby = false;
            }

            if( BabySprite[ ( int ) dir ].gameObject.activeSelf )
                Message.CreateMessage( ETileType.NONE, "Roach Att: -" + VsHeroRoachAttackIncreasePerBaby + "%\nHero Att: + "
                + VsRoachAttDecreasePerBaby + "%", Unit.Pos + new Vector2( 0, -1 ), Color.green );

            if( killbaby )
            {
                BabySprite[ ( int ) dir ].gameObject.SetActive( false );                                             // Deactivate sprite
                HasBaby[ ( int ) dir ] = false; 
            }

            int babies = 0;
            for( int i = 0; i < 8; i++ )
            if( BabySprite[ i ].gameObject.activeSelf ) babies++;                                                    // Count babies

            float per = 100 - ( babies * VsRoachAttDecreasePerBaby );
            damage = Util.Percent( per, damage );
            if( damage < 0 ) damage = 0;
            SpawnBlocker = true;                                                                                     // Roach Spawn Blocker   

            if ( Unit.Control.RedRoachBabyList != null )                                                             // Spawn Red RoachBaby  blocker after attack
            if ( Unit.Control.RedRoachBabyList.Count >= 1 )
            for( int i = 0; i < 8; i++ )
            if ( BabySprite[ i ].gameObject.activeSelf )
            if ( Unit.Control.RedRoachBabyList.Contains( ( EDirection ) i ) )
                {
                    int dr = ( int ) Unit.Dir + i;
                    if( dr >= 8 ) dr -= 8;
                    Vector2 ps = Unit.Pos + Manager.I.U.DirCord[ dr ];
                    if( Controller.GetBlocker( ps ) == null )
                        Map.I.SpawnFlyingUnit( ps, ELayerType.MONSTER, ETileType.BLOCKER, Unit );
                }
        }
        else
        if( attacker.UnitType == EUnitType.MONSTER )
        if( attacker.TileID == ETileType.ROACH )
            {
                int babies = 0;
                for( int i = 0; i < 8; i++ )
                if( attacker.Body.BabySprite[ i ].gameObject.activeSelf ) babies++;
                bonus = babies * VsHeroRoachAttackIncreasePerBaby;
            }
        return bonus;
    }
    public float CheckDragon( Unit attacker, Attack attack, ref float damage )
    {
        if( attack == null ) return 0;
        if( Unit.TileID == ETileType.QUEROQUERO ) return 0;
        float bonus = 0;
        if( attacker.UnitType == EUnitType.HERO && Unit.Control.IsFlyingUnit )                                   // Hero Attacks Dragon
        {
            float perc = Map.I.GetAngleDamageInPercent( attacker.Spr.transform.rotation.eulerAngles, 
                         Unit.Spr.transform.rotation.eulerAngles );
            bonus = Util.Percent( perc, 50 );

            if( Unit.Control.IsFlyingUnit )
            if( HeroAttackDirectionHistory.Contains( Map.I.Hero.Dir ) == false )                                 // Adds New Hero Attack Direction to data and add to bonus
                HeroAttackDirectionHistory.Add( Map.I.Hero.Dir );
            if( HeroAttackDirectionHistory.Count > 1 )
            {
                float bn = G.Hero.Body.FreshAttackLevel;
                bonus += bn * ( HeroAttackDirectionHistory.Count - 1 );
            }

            if( Unit.TileID == ETileType.JUMPER || Unit.TileID == ETileType.MOTHERJUMPER )
            {
                float dist = Vector2.Distance( attacker.Pos, Unit.Pos );                                        // Hero Attack power decrease with distance
                perc = 100 - ( ( dist - 1 ) * Map.I.RM.RMD.JumperHeroAttackDistanceDecrease );
                damage = Util.Percent( perc, damage );
                if( damage <= 0 )
                {
                    Attack.InvalidateAttack = true;
                    damage = 0;
                }
            }
        }
        else
        if( attacker.TileID == ETileType.DRAGON1 )  
        if( attacker.Control.IsFlyingUnit && Unit.UnitType == EUnitType.HERO )                                  // Dragon Attacks Hero
        {
            float perc = Map.I.GetAngleDamageInPercent( Unit.Spr.transform.rotation.eulerAngles,
            attacker.Spr.transform.rotation.eulerAngles );
            bonus = Util.Percent( perc, 50 );
        }
        return bonus;
    }

    public void CheckArrowHit( EDamageType type )
    {
        if( Map.I.CurrentArea == -1 ) return;
        Unit ar = Map.I.GetUnit( ETileType.ARROW, Unit.Pos );
        int totHit = 1 + Map.I.NumMonstersOverArrows + Map.I.CurArea.ArrowsDestroyed;
        bool destroyed = false;
        if( ar != null )                                                                                        // Monster Over Arrow
        {
            ar.Body.ArrowHitCount++;
            if( ar.Body.ArrowHitCount >= totHit - 1 )
            {
                Map.I.CurArea.ArrowsDestroyed++;
                ar.Kill();
                destroyed = true;
            }
           MakePeaceful = 1;
        }

        if( type == EDamageType.RANGED )                                                                     // Ranged attack passing over arrows
        if( BigMonster )
        {
            for( int i = 0; i < Map.I.Hero.RangedAttack.AttackTg.Count; i++ )
            {
                ar = Map.I.GetUnit( ETileType.ARROW, Map.I.Hero.RangedAttack.AttackTg[ i ] );

                if( ar != null && destroyed == false ) 
                {
                    ar.Body.ArrowHitCount++;
                    if( ar.Body.ArrowHitCount >= totHit - 1 )
                    {
                        Map.I.CurArea.ArrowsDestroyed++;
                        ar.Kill();
                        destroyed = true;
                    }
                    MakePeaceful = 1;
                    MakeImovable = 1;
                }
            }
        }

        ar = Map.I.GetUnit( ETileType.ARROW, Map.I.Hero.Pos );

        if( type == EDamageType.MELEE )
        if( ar != null && destroyed == false )                                                                 // Hero over Arrow
        {
            ar.Body.ArrowHitCount++;
            if( ar.Body.ArrowHitCount >= totHit )
            {
                Map.I.CurArea.ArrowsDestroyed++;
                ar.Kill();
                destroyed = true;
            }
            MakePeaceful = 1;

            for( int i = 0; i < 8; i++ )                                                                       // If arrow under hero is destroyed, protect hero from other monsters
            {
                Vector2 aux = Map.I.Hero.Pos + Manager.I.U.DirCord[ i ];
                Unit en = Map.I.GetUnit( aux, ELayerType.MONSTER );
                if( en && en.ValidMonster )
                if( Map.I.CheckArrowBlockFromTo( Map.I.Hero.Pos, aux, en ) ) 
                    en.Body.MakePeaceful = 1;
            }
        }

        if( destroyed )
            Message.GreenMessage( "Arrow Destroyed!\nArrow Hit +1 " );
    }

    public float CalculateSlimeBonus()
    {
        if( Unit.UnitType != EUnitType.MONSTER ) return 0;
        return Attack.ShotPassThroughCount * Map.I.RM.RMD.SlimeShotPassThroughBonus;
    }

    public float CalculateAgilityAttackBonus( EDamageType type, Unit attacker )
    {
        if( type == EDamageType.FIRE ) return 0;
        if( Map.I.CurrentArea == -1 ) return 0;
        if( attacker.UnitType != EUnitType.HERO ) return 0;
        return EnemyAttackBonus;
    }

    public float CalculateFireAttackBonus( Unit attacker )
    {
        return 0;
        if( Map.I.CurrentArea == -1 ) return 0;
        if( attacker.UnitType != EUnitType.HERO ) return 0;
        int lev = ( int ) Mathf.Ceil( Map.I.Hero.Body.FireMasterLevel );

        int cont = Map.I.CurArea.BonfiresLit;
        if( Manager.I.GameType == EGameType.CUBES ) 
            cont = Map.I.RM.HeroSector.FireIgnitionCount;

        float bonus = HeroData.I.AttackBonusPerFireLit[ lev ] * cont;
        bonus = Util.Percent( Map.I.RM.RMD.FireAttackBonusFactor, bonus );
        return bonus;
    }

    public float CalculatePlatformAttackBonus( Unit attacker )
    {
        if( Map.I.CurrentArea == -1 ) return 0;
        if( attacker.UnitType != EUnitType.HERO ) return 0;
        Unit plat = Map.I.GetUnit( Map.I.Hero.Pos, ELayerType.GAIA );

        float over = 0;
        if( plat != null )
        if( plat.TileID == ETileType.TRAP )
            {
                int lev = ( int ) Mathf.Ceil( Map.I.Hero.Control.PlatformWalkingLevel );
                over = HeroData.I.PlatformAttackBonus[ lev ];
            }

        float global = Map.I.LevelStats.PlatformPoints * Map.I.RM.RMD.GlobalPlatformAttBonusPerPoint;

        return global + over; 
    }

    public void UpdatePunchEffect( Unit attacker,  EDamageType type, bool inverse = false )
    {
        if( type == EDamageType.BLEEDING ) return;
        if( Unit.Control.IsFlyingUnit ) return;
        float amt = 0.3f;
        if( inverse ) amt *= -1;
        Vector3 tg = Util.GetTargetUnitVector( Unit.Pos, attacker.Pos );
        iTween.PunchPosition( Unit.Graphic.gameObject, tg * amt, 0.3f );
    }
    
    public void UpdateCubeDeath()
    {
        if( Map.I.CubeDeath == false ) return;
        if( Map.I.DeathAnimationTimer <= 0 )
        {
            //G.Hero.Graphic.gameObject.SetActive( false );
            //G.Hero.Body.CreateBloodSplat();
            return;
        }
        string msg = "You died!";

        if( Map.I.RM.RMD.RescueToGate && Map.I.RM.LastGateTileStepped.x != -1 )
            Map.I.Hero.Control.ApplyMove( Map.I.Hero.Pos, Map.I.RM.LastGateTileStepped );
        else
            Map.I.Hero.Control.ApplyMove( Map.I.Hero.Pos, Map.I.LevelEntrancePosition );
        Map.I.RM.HeroSector.LastHeroPos = new Vector2( -1, -1 );

        if( Input.GetKey( KeyCode.R ) )
        {
            msg = "You were Rescued!";
        }

        if( Map.I.CurrentArea == -1 && Map.I.PlatformDeath )
        {
            msg = "Platform Death!";
            Unit.LevelTxt.text = "";
        }

        Map.I.StopAllLoopedSounds();
        Message.CreateMessage( ETileType.NONE, msg, Unit.Pos, new Color( 1, 0, 0, 1 ), true, true, 7 );
        Map.I.CubeDeath = false;
        Map.I.DeathAnimationTimer = 0;
        G.Hero.Body.RigidBody.velocity = Vector2.zero;
        G.Hero.Graphic.gameObject.SetActive( true );
    }

    public EDirection GetPosRelativeDir( EDirection dir, Vector2 tg )
    {
        Vector2 aux = Unit.Pos;
        for( int d = 0; d < 8; d++ )
            for( int i = 1; i < 100; i++ )
            {
                EDirection dr = Unit.GetRelativeDirection( d );
                aux = Unit.Pos + ( Manager.I.U.DirCord[ ( int ) dr ] * i );
                if( Map.PtOnMap( Map.I.Tilemap, aux ) == false ) break;
                if( tg == aux ) return ( EDirection ) d;
            }
        return EDirection.NONE;
    }
    
    public float AddHP( float _hp, bool showmsg = false )
    {
        float rest = 0;
        Hp += _hp;
        if( Hp > TotHp )
        {
            rest = TotHp - Hp;
            Hp = TotHp;
        }
        UpdateHealthBar();
        Color col = Color.green;
        if( _hp < 0 ) col = Color.red;
        if( showmsg )
        Message.CreateMessage( ETileType.NONE, ItemType.Res_HP, "" +
        _hp.ToString("+0;-#"), Unit.Pos, col, true, true, 4, 0, 0, 50 );
        return _hp - rest;
    }

    public void UpdateSecondaryAttack( int lev )
    {
        if( Map.I.CurrentArea == -1 ) return;
        if( Map.I.CurArea.AreaTurnCount <= 0 ) return;
        Attack.SecondaryAttack = true;

        if( MeleeAttackLevel >= lev )                                              
        {
            Unit.MeleeAttack.AttackResult = 0;
            Unit.MeleeAttack.UpdateIt( Unit.MeleeAttack.enabled );
        }

        if( RangedAttackLevel >= lev )                                                
        {
            if( MeleeAttackLevel <= 0 ||
                Unit.MeleeAttack.AttackResult <= 0 )
            {
                Unit.RangedAttack.UpdateIt( Unit.RangedAttack.enabled );
            }
        }
        Attack.SecondaryAttack = false;
    }
    
    public bool TakeFireDamage( int x = -1, int y = -1, bool fromfire = true )
    {
        if( IsDead ) return false;
        if( Map.I.IsPaused() ) return false;

        if( x == -1 )
        {
            x = ( int ) Unit.Pos.x;  // x and y fire source
            y = ( int ) Unit.Pos.y;
        }

        Unit un = null;
        Unit fr = Map.I.GetUnit( ETileType.FIRE, new Vector2( x, y ) );                          // Fire in position?
        if( fr && fr.Body.FireIsOn ) un = fr;

        Unit br = Map.I.GetUnit( ETileType.BARRICADE, new Vector2( x, y ) );                     // Burning Barricade in position?
        if( un == null )
        if( br && br.Body.FireIsOn ) un = br;

        Unit tgvn = Map.GFU( ETileType.VINES, new Vector2( x, y ) );                             // Burning Vine in position?
        if( un == null )
        if( tgvn && tgvn.Body.FireIsOn )
            un = tgvn;

        if( un == null || un.Body.FireIsOn == false ) return false;

        if( Spell.IsCompoundMonster( Unit ) ) return false;                                      // Compound monster cannot be burned

        bool firetimeok = false;                                                                 // Not immediate damage    (needs more than one turn over fire)      
        if( Unit.Control.PositionHistory.Count >= 2 &&
            Unit.Control.PositionHistory[ Unit.Control.PositionHistory.Count - 2 ] == new Vector2( x, y ) &&
            Unit.Pos == un.Pos )
            firetimeok = true;

        if( Map.I.Hero.Body.FireMasterLevel >= 8 ) firetimeok = true;
        if( Unit.Control.IsFlyingUnit ) firetimeok = true;
        if( Item.IsPlagueMonster( Unit.Variation ) ) firetimeok = true;
        if( Unit.TileID == ETileType.BOULDER ) firetimeok = true;

        if( fromfire == false || firetimeok )
        {
            MasterAudio.PlaySound3DAtVector3( "Fire Ignite", Map.I.Hero.Pos );
            if( Unit.TileID == ETileType.SCARAB || 
                Unit.TileID == ETileType.SCORPION )
            {
                Unit.Control.SpeedTimeCounter = 0;
                Lives--;                                                                                           // Only Life Lost
                if( Lives >= 1 )
                {
                    Hp = TotHp;
                }
                else                                                                                               // Unit Dies
                {
                    Unit.Kill();
                }
                return true;
            }

            if( Unit.TileID == ETileType.SPIDER )
            {
                bool res = Unit.Control.UpdateSpiderStepping( Unit.GetFront(), Unit.Pos );                         // Updates spider Stepping
                if( res ) Unit.Kill();
                return true;
            }

            if( Unit.TileID == ETileType.BOULDER  )                                                               // Burn Boulder 
            {
                Map.Kill( Unit );
                Map.I.CreateExplosionFX( Unit.Pos );                                                              // Explosion FX
                Controller.CreateMagicEffect( Unit.Pos, "Mine Debris", 5 );                                       // Mine Debris FX
                MasterAudio.PlaySound3DAtVector3( "Mine Destroy", Unit.Pos );                                     // Sound FX
                return true;
            }

            if( Unit.TileID == ETileType.MINE )
            {
                Controller.CreateMagicEffect( Unit.Pos, "Mine Debris", 5 );                                       // Mine Debris FX
                Controller.DestroyMine( ref Unit );                                                               // Destroy mine
                return true;
            }

            if( Unit.TileID == ETileType.WASP )                                                                   // Wasp burn
            {
                Unit.Kill();
                return true;
            }

            int lev = ( int ) Mathf.Ceil( Map.I.Hero.Body.FireMasterLevel );
            float per = Map.I.RM.RMD.BaseFirePower + un.Body.FirePowerLevel;

            float dmg = Util.Percent( per, Map.I.Hero.MeleeAttack.TotalDamage );

            if( fromfire == false )
            {
                dmg = Util.Percent( Map.I.RM.RMD.BurningBarricadeDamagePercent, dmg );                            // Burning Barricade damage
            }

            if( Unit.TileID == ETileType.DRAGON1 )
            {
                dmg = Map.I.RM.RMD.BaseDragonFireDamage;
                Hp -= dmg;                                                                   // There is a hidden bug that randomizes flying unit position making it change the current tile several times triggering multiple fire damage (this is to solve it)
                UpdateHealthBar();
                CreateDamageAnimation( Unit.Pos, dmg, G.Hero, EDamageType.FIRE );
                if( Hp <= 0 ) Unit.Kill();
                goto end;
            }

            ReceiveDamage( dmg, EDamageType.FIRE, Map.I.Hero, null );                        // Take Fire Damage

            end:
            if( un.Body.AvailableFireHits != -1 )                                            // Limited Fire hits for the Firepit
            {
                if( --un.Body.AvailableFireHits == 0 )
                    un.Kill();
            }
            return true;
        }   
        return false;
    }

    public void UpdateBleeding( bool force = false )
    {
        if( Manager.I.GameType != EGameType.CUBES ) return;
        if( IsDead ) return;
        if( Helper.I.InvunerableHero ) return;
        if(!force )
        if( Map.I.AdvanceTurn == false ) return;
        float dmg = Map.I.RM.RMD.OutsideBleedingHP;
        if( Map.I.CurrentArea != -1 )
        if( Map.I.AreaCleared == false ) dmg = Map.I.RM.RMD.InsideDirtyBleedingHP;
        else dmg = Map.I.RM.RMD.InsideCleanBleedingHP;
        if( Map.I.RM.HeroSector.Type == Sector.ESectorType.LAB ) dmg = Map.I.RM.RMD.InsideLabBleedingHP;
        Unit un = Map.I.GetUnit( G.Hero.Pos, ELayerType.GAIA );
        if( un )
        if( un.TileID == ETileType.TRAP )
            dmg = Map.I.RM.RMD.PlatformBleedingHP;
        if( dmg <= 0 ) return;        
        if( Map.I.Hero.Body.Hp == Map.I.Hero.Body.TotHp )
            Message.RedMessage( "Bleeding Started!" +
                "\nOutside Area: " + Map.I.RM.RMD.OutsideBleedingHP.ToString("0.0") + " HP" +
                "\nInside Dirty Area: " + Map.I.RM.RMD.InsideDirtyBleedingHP.ToString( "0.0" ) + " HP" +
                "\nInside Clear Area: " + Map.I.RM.RMD.InsideCleanBleedingHP.ToString( "0.0" ) + " HP" +
                "\nOver Platforms: " + Map.I.RM.RMD.PlatformBleedingHP.ToString( "0.0" ) + " HP" );

        ReceiveDamage( dmg, EDamageType.BLEEDING, Map.I.Hero, null );
    }
    
    public bool IsHealty()
    {
        float hp = Map.I.HeroEnterAreaHP - G.Hero.Body.Hp;
        if( hp > 0 ) return false;
        return true;
    }

    public void ResetTurnData()
    {
        MakePeaceful--;
        if( MakePeaceful < 0 )
            MakePeaceful = 0;

        MakeImovable--;
        if( MakeImovable < 0 )
            MakeImovable = 0;

        if( ThreatLevel < 0 )
        if( ThreatLevel > -80 )
            ThreatLevel *= -1;

        if( ThreatLevel == 1 )
        {
            ThreatLevel = -100;
            //if( G.Hero.Body.IntelThreatLevel >= 11 )        // Unlimted Threats
            ThreatLevel = 0;
        }
        else
        if( ThreatLevel > 0 )
            ThreatLevel--;
    }

    public bool HasFullHealth()
    {
        if( Hp == TotHp ) return true;
        return false;
    }

    public float GetHealthPercent()
    {
        return 100 * Hp / TotHp;
    }

    public bool CalculateMonsterLOS()
    {
        if( Unit.ValidMonster == false ) return false;
        if( Map.I.HasLOS( G.Hero.Pos, Unit.Pos ) ) 
        {
            Map.I.NumMonsterSeeingHero++;
            return true;
        }
        else return false;
    }
    public void SetWorking( bool working )
    {
        if( Unit.TileID != ETileType.ARROW ) return;
        isWorking = working;

        if( working )
        {
            Unit.Spr.color = Color.white;
        }
        else
        {            
            Unit.Spr.color = new Color( .4f, .4f, .4f, .8f );
        }
    }
    public void UpdateFrameAnimation( int startframe, int framecount, bool pingpong, float tottime )
    {
        int maxframe = startframe + framecount - 1;
        AnimationTimeCounter += Time.deltaTime;

        if( AnimationTimeCounter > tottime )
        {
            AnimationTimeCounter = 0;
            AnimationFrame += AnimationSignal;

            if( pingpong )
            {
                if( AnimationFrame > maxframe )
                {
                    AnimationSignal = -1;
                    AnimationFrame = maxframe - 1;
                }
                else
                if( AnimationFrame < startframe )
                {
                    AnimationSignal = +1;
                    AnimationFrame = startframe + 1;
                }                
            }
            else
            {
                if( AnimationFrame > maxframe ) AnimationFrame = startframe;
            }

            if( Map.I.RM.HeroSector.TimeSpentOnCube < 1 )  
                AnimationFrame = Random.Range( 202, 207 );
            Unit.Spr.spriteId = AnimationFrame;
        }
    }

    public float GetMiniDomeTotTime()
    {
        float time = 0;
        if( MiniDomeTotalTime > 0 ) return MiniDomeTotalTime;
        if( Unit.Md && Unit.Md.MiniDomeTotalTimeList != null && Unit.Md.MiniDomeTotalTimeList.Count > 0 )
        {
            if( Unit.Body.ResourcePersistStepCount <= Unit.Md.MiniDomeTotalTimeList.Count - 1 )
                time = Unit.Md.MiniDomeTotalTimeList[ Unit.Body.ResourcePersistStepCount ];
            else
                time = Unit.Md.MiniDomeTotalTimeList[ Unit.Md.MiniDomeTotalTimeList.Count - 1 ];
        }
        return time;
    }

    public bool MineHasLever()
    {
        if( MineType != EMineType.INDESTRUCTIBLE ) return false;
        if( MineType != EMineType.SPIKE_BALL )
        if( MineType != EMineType.SHACKLE )
        if( LeverMine || RopeConnectSon != null )
           return true;
        return false;
    }

    public bool isCreatedVine()
    {
        if( Controller.GetRaft( Unit.Pos ) == null )
        if( CreatedVine ) return true;
        return false;
    }
    public bool IsChest()
    {
        if( Unit.Dir == EDirection.NONE )
        if( BonusItemList != null && BonusItemList.Count > 0 ) return true;
        return false;
    }
    public void UpdateImmunityShield()
    {
        if( ImmunityDome )                                                         // immunity shield code
        {
            if( ImmunityShieldTime > 0 )
            {
                ImmunityShieldTime -= Time.deltaTime;
                if( ImmunityShieldTime < 0 )
                    ImmunityShieldTime = 0;
            }
            ImmunityDome.gameObject.SetActive( GetImmunityShieldState() );         // activate sprite
        }
    }
    public bool GetImmunityShieldState()
    {
        if( Unit.Md == null ) return false;
        if( ImmunityShieldTime > 0 )
            return !Unit.Md.DefaultImmunityShieldState;                            // return shield state according with the default state
        return Unit.Md.DefaultImmunityShieldState;
    }
}
