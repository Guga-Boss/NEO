using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;

public enum EHeroDataVal
{
    None = 0, AGL_PABN, AGL_RABN, AGL_MJBN, AGL_OFBN,
    FISHING_HOOK_RADIUS = 100, FISHING_HOOK_ATTACK, FISHING_HOOK_SPEED, FISHING_TIME,
    FISHING_LEVEL,
    MELEESHIELDLEVEL
};
public class HeroData : MonoBehaviour 
{
	public static HeroData I;
    public List<Vector2> CollectedLevelArtifacts, CollectedGameArtifacts, CollectedAreaArtifacts;
    public float[] DamageSurplusBonus, MonsterCorneringBonus, SprinterAttackBonus, 
        ExtraAttacksperBersekerLevel, BersekerAttackPower, PsychicShowHintFactor, PsychicShowLetterFactor, WallDestroyerClearedAreasPerBomb, WallDestroyerBombsPerLevel, SneakingSleepingChance,
        RandomLootChance, ExtraLootAmount, ExtraRandomResourceAmount, ExtraMonsterLootTurns, ProspectorInterestRate, AttackBonusPerFireLit, SneakingWakeUpExtraTurns, BeehiveChance, BeehiveExtraTurns, 
        SecondaryAttackPower, AmbusherBonusPerTile, EvasionStartingPoints, PlatformAttackBonus,
        AgilityRiskyAttackPower, AgilityOpenFieldAttackPower, AgilityMortalJumpAttackBonus, AgilityPointsToRune, AgilityRunePrize, AgilityAttackBonusPerPerk, SlayerForestTiles, SlayerForestDragonAngle;
	public string[] HeroNameList;
    public int[] HoneycombPrizes;

    public float BlueRunes, RedRunes, GreenRunes, AreaPoints, InitialStars, PlatformPoints, BarricadeWood,
                 OriginalBarricadeWood, AgilityPoints, Lives;

	// Use this for initialization
	void Start () {
	I = this;
	}

    public void StartGame()
    {
        PlatformPoints = 0;
        BarricadeWood = 0;
        OriginalBarricadeWood = 0;
        AgilityPoints = 0;
    }
	
    public bool IsAreaCleared( string areaname )
    {
        //for( int i = 0; i < ConqueredAreas.Count; i++ )
        //    if( ConqueredAreas[ i ] == areaname ) return true;
        return false;
    }

	public void AddCollectedArtifact( Vector2 tg, Artifact.EArtifactLifeTime type )
	{
        if( type == Artifact.EArtifactLifeTime.AREA )
        {
            if( CollectedAreaArtifacts.Contains( tg ) ) return;
            CollectedAreaArtifacts.Add( tg );
        }

        if( type == Artifact.EArtifactLifeTime.SESSION )
        {
            if( CollectedLevelArtifacts.Contains( tg ) ) return;
            CollectedLevelArtifacts.Add( tg );
        }

        if( type == Artifact.EArtifactLifeTime.GAME )
        {
            if( CollectedGameArtifacts.Contains( tg ) ) return;
            CollectedGameArtifacts.Add( tg );
        }
	}

	public void RemoveConqueredArtifact( Vector2 tg )
	{
		for( int a = 0; a < CollectedLevelArtifacts.Count; a++ )
			if( CollectedLevelArtifacts[ a ] == tg )
			{
				CollectedLevelArtifacts.Remove( tg );
				return;
			}

		for( int a = 0; a < CollectedAreaArtifacts.Count; a++ )
			if( CollectedAreaArtifacts[ a ] == tg )
			{
				CollectedAreaArtifacts.Remove( tg );
				return;
			}

		for( int a = 0; a < CollectedGameArtifacts.Count; a++ )
			if( CollectedGameArtifacts[ a ] == tg )
			{
				CollectedGameArtifacts.Remove( tg );
				return;
			}
	}
    
    //_____________________________________________________________________________________________________________________ ClearArtifactsThatHaveBeenCollected

    public void ClearAllCollectedArtifacts( Artifact.EArtifactLifeTime type )
    {
        if( type == Artifact.EArtifactLifeTime.AREA )
        {
        for( int c = 0; c < CollectedAreaArtifacts.Count; c++ )
        for( int a = 0; a < Quest.I.LevelList[ Quest.CurrentLevel ].ArtifactList.Count; a++ )
        if ( CollectedAreaArtifacts[ c ] == Quest.I.LevelList[ Quest.CurrentLevel ].ArtifactList[ a ].Pos )
            {
                Quest.I.LevelList[ Quest.CurrentLevel ].ArtifactList[ a ].Collected = Artifact.EStatus.NOT_COLLECTED;
                Quest.I.LevelList[ Quest.CurrentLevel ].ArtifactList[ a ].gameObject.SetActive( true );
            }
            
            CollectedAreaArtifacts.Clear();
        }

        if( type == Artifact.EArtifactLifeTime.SESSION )
        {
            CollectedLevelArtifacts.Clear();
        }

        if( type == Artifact.EArtifactLifeTime.GAME )
        {
            CollectedGameArtifacts.Clear();
        }
    }

    public void DoOperation( ref float variable, EResourceOperation oper, float value )
    {
        switch( oper )
        {
            case EResourceOperation.ADD:
            variable += value;
            break;
            case EResourceOperation.SET:
            variable = value;
            break;      
        }
    }

    public float GetVal( EHeroDataVal type, int lev = -1 )
    {
        if( Manager.I.GameType != EGameType.CUBES ) return 0; // to avoid G.HS null bug
        if( G.HS == null ) return 0;
        int aglPerks = ( int ) G.Hero.Body.AgilityLevel    + ( int ) G.Hero.Body.FreshAttackLevel + ( int ) G.Hero.Body.RiskyAttackLevel +
                       ( int ) G.Hero.Body.MortalJumpLevel + ( int ) G.Hero.Body.OpenFieldAtttackLevel;

        float val = 0;
        switch( type )
        {
            case EHeroDataVal.AGL_PABN:
            {
                if( lev == -1 ) lev = ( int ) Mathf.Ceil( Map.I.Hero.Body.FreshAttackLevel );
                val = lev;
                float per = aglPerks * AgilityAttackBonusPerPerk[ ( int ) Map.I.Hero.Body.AgilityLevel ];
                val += Util.Percent( per, val );
                break;                
            }

            case EHeroDataVal.AGL_RABN:
            {
                if( lev == -1 ) lev = ( int ) Mathf.Ceil( Map.I.Hero.Body.RiskyAttackLevel );
                val = HeroData.I.AgilityRiskyAttackPower[ lev ];
                float per = aglPerks * AgilityAttackBonusPerPerk[ ( int ) Map.I.Hero.Body.AgilityLevel ];
                val += Util.Percent( per, val );
                break;
            }
            
            case EHeroDataVal.AGL_MJBN:
            {
                if( lev == -1 ) lev = ( int ) Mathf.Ceil( Map.I.Hero.Body.MortalJumpLevel );
                val = HeroData.I.AgilityMortalJumpAttackBonus[ lev ];
                float per = aglPerks * AgilityAttackBonusPerPerk[ ( int ) Map.I.Hero.Body.AgilityLevel ];
                val += Util.Percent( per, val );
                break;
            }

            case EHeroDataVal.AGL_OFBN:
            {
                if( lev == -1 ) lev = ( int ) Mathf.Ceil( Map.I.Hero.Body.OpenFieldAtttackLevel );
                val = HeroData.I.AgilityOpenFieldAttackPower[ lev ];
                float per = aglPerks * AgilityAttackBonusPerPerk[ ( int ) Map.I.Hero.Body.AgilityLevel ];
                val += Util.Percent( per, val );
                break;
            }
            
            case EHeroDataVal.FISHING_LEVEL:
            int pole = 0;
            if( Map.I.FishingMode != EFishingPhase.NO_FISHING )
                pole = Map.I.CurrentFishingPole.Control.PoleExtraLevel;                                          // Pole extra level
            else
            {
                Unit pl = Map.I.GetUnit( ETileType.FISHING_POLE, new Vector2( Map.I.Mtx, Map.I.Mty ) );
                if( pl ) pole = pl.Control.PoleExtraLevel;
            }

            val = Map.I.RM.RMD.BaseFishingLevel + G.HS.FishingHookExtraLevel + G.Hero.Body.FishingLevel + pole;
            if( Manager.I.GameType == EGameType.NAVIGATION ) return 0;
            break;

            case EHeroDataVal.FISHING_HOOK_RADIUS:
            if( lev == -1 ) lev = ( int ) GetVal( EHeroDataVal.FISHING_LEVEL );
            float amt = Util.Percent( ( lev - 1 ) * Map.I.RM.RMD.FishingHookRadiusPerLevel, 
                                                    Map.I.RM.RMD.FishingBaseHookRadius );
            val = Map.I.RM.RMD.FishingBaseHookRadius + amt;
            val += Util.Percent( G.HS.FishingExtraRadius + Water.TempFishingExtraRadius, val );
            break;

            case EHeroDataVal.FISHING_HOOK_ATTACK:
            if( lev == -1 ) lev = ( int ) GetVal( EHeroDataVal.FISHING_LEVEL );
            amt = Util.Percent( ( lev - 1 ) * Map.I.RM.RMD.FishingHookAttackPerLevel, 
                                              Map.I.RM.RMD.FishingBaseHookAttack );
            val = Map.I.RM.RMD.FishingBaseHookAttack + amt;
            val += Util.Percent( G.HS.FishingExtraAttack + Water.TempFishingExtraAttack, val );
            break;

            case EHeroDataVal.FISHING_HOOK_SPEED:
            if( lev == -1 ) lev = ( int ) GetVal( EHeroDataVal.FISHING_LEVEL );
            amt = Util.Percent( ( lev - 1 ) * Map.I.RM.RMD.FishingHookSpeedPerLevel, 
                                              Map.I.RM.RMD.FishingBaseHookSpeed );
            val = Map.I.RM.RMD.FishingBaseHookSpeed + amt;
            val += Util.Percent( G.HS.FishingExtraSpeed + Water.TempFishingExtraSpeed, val );
            break;     

            case EHeroDataVal.FISHING_TIME:
            val = Map.I.RM.RMD.BaseFishingTime + G.HS.FishingExtraTime;
            break;

            default:
            {
                Debug.LogError( "Bad EHVAL" );
                break;
            }
        }
        return val;
    }
}

