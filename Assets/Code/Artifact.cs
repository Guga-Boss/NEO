using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum EArtifactDataBase
{
    ARTIFACT, Star_1, Star_2, Star_3, ImproveRanged, ImproveMelee, MovementUpgrade, ArrowWalkingUpgrade,
    ImproveDexterity, OrbStriker, ImproveMonsterCornering, ImproveCooperation, ImproveDamageSurplus,
    ImproveMeleeShield, ImproveMissileShield, ImproveMonsterPush, ImproveScout, ImprovePlatformWalking, PlatformSteps_Upgrade, FreePlatformExit_Upgrade,
    ImproveAmbusher, ImproveMemory, ImproveToolbox,
    SprinterUpgrade, FiremasterUpgrade, BerserkUpgrade, RicochetUpgrade, BeehivethrowerUpgrade, PsychicUpgrade, SneakingUpgrade, ImproveWallDestroyer,
    LooterUpgrade, ProspectorUpgrade, BarricadeFighter, EvasionUpgrade, PerfectionistUpgrade, ScavengerUpgrade, AgilityUpgrade,
    ArrowFighterUpgrade, Arrow_In, Arrow_Out, IntelligenceUpgrade, CollectorUpgrade, Agility_FreshAttack_Upgrade, Agility_RiskyAttack_Upgrade, Agility_MortalJumpAttack_Upgrade, Agility_OpenFieldAttack_Upgrade,
    Intelligence_ThreatLevel_Upgrade, Intelligence_ScaryLevel_Upgrade, Intelligence_Intel2_Upgrade, Intelligence_Intel3_Upgrade, Intelligence_Intel4_Upgrade,
    Intelligence_BaseThreatDuration_Upgrade, Intelligence_BaseFreeExitHPLimit_Upgrade,
    FirePowerLevel_Upgrade, FireSpreadLevel_Upgrade, FireWoodNeeded_Upgrade, OutsideFireWoodAllowed_Upgrade, FirepitsNeededForDiscount_Upgrade, FireStarBonus_Upgrade,
    BarricadeDestroyLevel_Upgrade, OutAreaBurningBarricadeDestroyBonus_Upgrade,
    Scout_OverBarricadeScout_Upgrade, Scout_ShowResourceChance_Upgrade, Scout_ShowResourceNeighborsChance_Upgrade, BarricadeForRune_Upgrade,
    HeroNeighborTouchedAdder_Upgrade, ResourcePersistance_Upgrade, RTMeleeAttackSpeed_Upgrade, RTRangedAttackSpeed_Upgrade,
    MireLevel_Upgrade, RestDistance_Upgrade, Slayer_Level_Upgrade, Slayer_FlyingTargetting_Upgrade, Slayer_BackAngle_Upgrade, Slayer_MaxHP_Upgrade,
    Slayer_Disguise_Upgrade, Slayer_BonusDrop_Upgrade, Slayer_DragonBarricadeProtection_Upgrade, MiningLevel_Upgrade, FishingLevel_Upgrade
}

public class Artifact : MonoBehaviour {
    public enum EArtifactLifeTime { NONE, AREA, SESSION, GAME, CUBE }
    public enum EStatus
    {
        NOT_COLLECTED, TEMPORARY_COLLECTED, COLLECTED, RECURRING
    }
    public enum ECategory { NONE = -1, NORMAL, SECONDARY }
    public enum EMultiplier
    {
        x1, x2, x3, x4, x5, x6, x7, x8, x9, x10, x12 = 11, x15 = 14, x20 = 19, x25 = 24, x30 = 29, x40 = 39, x50 = 49, x75 = 74, x100 = 99
    }

    public MyTrigger Trigger;
    public Perk Perk;
    public tk2dSprite Sprite;
	public tk2dTextMesh TargetUnitName;
    public EMultiplier Multiplier;
    public EHeroID TargetHero = EHeroID.ALL_HEROES;
    public EArtifactLifeTime LifeTime;
	public EPerkType PerkType = EPerkType.NONE;
    public ECategory Category;
	public EStatus Collected;
	public Vector2 Pos;
    public  int MapLevelLimit = 0;
    public bool Recurring = false;
    public List<float> RecurringPriceList;
    public int TimesBought = 0;
	public int BelongingLevel, BelongingCube;
	public string ArtifactName, PrefabName, ActionDescription, PerkDescription;
    public float CostValue_1;
    public float CostValue_2;
    
	// Update is called once per frame
	public void Apply( ref Unit _unit )
	{
		Trigger.Unit = _unit;

        int times = ( ( int ) Multiplier + 1 ) * TimesBought;

        for( int i = 0; i < times; i++ )
            if( _unit.Variation == ( int ) TargetHero ||
                TargetHero == EHeroID.ALL_HEROES )
                Trigger.UpdateIt( true );
	}

    public void Copy( Artifact ar, bool copyPos = true )
    {
        if( copyPos ) Pos = ar.Pos;
        Trigger.Copy( ar.Trigger );
        Perk.Copy( ar.Perk );
        Sprite.spriteId = ar.Sprite.spriteId;
        LifeTime = ar.LifeTime;
        TargetHero = ar.TargetHero;
		PerkType = ar.PerkType;
		Collected = ar.Collected;
		PrefabName = ar.PrefabName;
		ActionDescription = ar.ActionDescription;
		PerkDescription = ar.PerkDescription;
		ArtifactName = ar.ArtifactName;
        Category = ar.Category;
        CostValue_1 = ar.CostValue_1;
        CostValue_2 = ar.CostValue_2;
        Multiplier = ar.Multiplier;
        Recurring = ar.Recurring;
        TimesBought = ar.TimesBought;
        RecurringPriceList = new List<float>();
        RecurringPriceList.AddRange( ar.RecurringPriceList );
    }
    public static void Save( string nm )
    {
        string file = Manager.I.GetProfileFolder();
        if( nm != "" ) file += "Cube Save/Artifact" + nm + ".NEO";                          // Provides filename
        else file += "/Artifact.NEO";
        Statistics st = Map.I.LevelStats;

        using( GS.W = new BinaryWriter( File.Open( file, FileMode.OpenOrCreate ) ) )
        {
            int SaveVersion = 1;
            GS.W.Write( SaveVersion );                                                      // Save Version 

            GS.W.Write( HeroData.I.CollectedLevelArtifacts.Count );
            for( int i = 0; i < HeroData.I.CollectedLevelArtifacts.Count; i++ )
                GS.SVector2( HeroData.I.CollectedLevelArtifacts[ i ] );                    // Save collected artifacts

            GS.W.Write( Quest.I.CurLevel.ArtifactList.Count );
            for( int i = 0; i < Quest.I.CurLevel.ArtifactList.Count; i++ )
                GS.W.Write( Quest.I.CurLevel.ArtifactList[ i ].TimesBought );              // Save number of times collected

            GS.W.Close();
        }
    }
    public static void Load( string nm = "" )
    {
        string file = Manager.I.GetProfileFolder();
        if( nm != "" ) file += "Cube Save/Artifact" + nm + ".NEO";                        // Provides filename
        else file += "/Artifact.NEO";
        Statistics st = Map.I.LevelStats;
        using( GS.R = new BinaryReader( File.Open( file, FileMode.Open ) ) )
        {
            int SaveVersion = GS.R.ReadInt32();                                           // Load Version

            HeroData.I.CollectedLevelArtifacts = new List<Vector2>();
            int sz = GS.R.ReadInt32(); 
            for( int i = 0; i < sz; i++ )
                HeroData.I.CollectedLevelArtifacts.Add( GS.LVector2() );                 // Load collected artifacts
            
            sz = GS.R.ReadInt32();
            for( int i = 0; i < sz; i++ )
                Quest.I.CurLevel.ArtifactList[ i ].TimesBought = GS.R.ReadInt32();       // Load number of times collected

            GS.R.Close();

            List<Vector2>col = new List<Vector2>();
            for( int a = 0; a < Quest.I.CurLevel.ArtifactList.Count; a++ )                                      // Initializes artifacts
            {
                Quest.I.CurLevel.ArtifactList[ a ].gameObject.SetActive( true );                                // NON Collected
                Quest.I.CurLevel.ArtifactList[ a ].Collected = EStatus.NOT_COLLECTED;
                RandomMap.CreateDome( Quest.I.CurLevel.ArtifactList[ a ].Pos );                                 // Create a dome automatically

                for( int c = 0; c < HeroData.I.CollectedLevelArtifacts.Count; c++ )
                if( HeroData.I.CollectedLevelArtifacts.Contains( Quest.I.CurLevel.ArtifactList[ a ].Pos ) )
                {
                    Quest.I.CurLevel.ArtifactList[ a ].gameObject.SetActive( false );                           // Collected
                    Quest.I.CurLevel.ArtifactList[ a ].Collected = EStatus.COLLECTED;
                    Unit dome = Map.I.GetUnit( ETileType.DOME, Quest.I.CurLevel.ArtifactList[ a ].Pos );
                    if( dome ) dome.Activate( false );
                }
            }
        }
    }

    public static void Swap( ref Artifact ar1, ref Artifact ar2 )
    {
        Map.I.RM.TempArtifact.Copy( ar1 );
        ar1.Copy( ar2 );
        ar2.Copy( Map.I.RM.TempArtifact );         
    }

    public static void QuickBuy()
    {
        if( Map.I.RM.RMD.EnableQuickBuy == false ) return;
        if( Quest.I.CurLevel.ArtifactList.Count <= 0 ) return;
        Artifact ar = Quest.I.CurLevel.ArtifactList[ Map.I.CurrentArtifactSeekID ];
        Quest.I.UpdateArtifactStepping( ar.Pos );
        ar.Buy( true, ar.Pos );
        Map.I.FreeCamMode = false;
    }

    public bool Buy( bool apply, Vector2 tg ) 
    {
        Unit un = Map.I.GetUnit( ETileType.DOME, tg );
        if( un == null ) return false;

        bool ok1 = false;
        bool ok2 = false;

        if( ok1 && ok2 )
        {
            if( apply )            
            if( un.TileID == ETileType.DOME )
                un.Activate( false );

            return true;
        }
            
        return false;
    }

    public float CalculatePrice()
    {
        int vari = ( int ) G.Hero.GetPerkVar( PerkType );
        if( PerkType == EPerkType.STAR ) vari = TimesBought;
        if( RecurringPriceList != null && RecurringPriceList.Count > 0 )
        {
            if( vari > RecurringPriceList.Count - 1 )
                vari = RecurringPriceList.Count - 1;
            float price = RecurringPriceList[ vari ];
            return price;
        }
        else return -1;                    
    }
}

