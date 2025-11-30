using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using DarkTonic.MasterAudio;

public class Area : MonoBehaviour
{
    #region Variables
    public tk2dTextMesh InfoText;
	public tk2dSlicedSprite Spr;
    public EAreaColor TileColor = EAreaColor.NONE;
    public int SectorID = -1;
    public int GlobalID = -1;
    public List<Vector2> TL; // Tiles List
	public string UniqueId, AreaName;
	public Rect AreaRect;
    public  bool IsFake = false;
	public Vector2 P1, P2, RandomMapSector, LockAiTarget, LastOutAreaStepTile;
    public bool Cleared, Discovered, Optional, Perfect, OriginalPerfect, Virgin, OriginalVirgin, BigMonsterVirgin, OriginalBigMonsterVirgin, ResourcesCleared, OriginalResourcesCleared;
    public float Score;
    public int DefaulMonsterLevel = 1;
	public int DefaulMonsterStar = 0;
    public int DefaultBossLevel = 1;
	public int DefaultBossStar = 0;
    public int NumTimesEntered, NumTimesExitedWhileDirty, LifetimeSteps, OriginalDirtyLifetimeSteps, 
               DirtyLifetimeSteps, ArrowsDestroyed, OriginalArrowsDestroyed;
    public Sector Sector;
	public Vector2 OptimalCameraPos;
    public float OptimalZoom;
	public bool ZoomInIfCleared;
	public bool BypassZoomIfDirty;

    public int BonfiresLit, DirtyBonfiresLit, BonfireCount, OriginalHeroHitsTaken;
    public int MonsterKillCount;
    public int RoachKillCount;
    public int ScarabKillCount;
    public int PoisonerKillCount;
    public int PoisonerCount;
    public int RoachCount;
    public int ScarabCount;
    public int MonsterCount;
    public bool AllBurntAvailable, MarkedArea;
    public int MushroomsCollectedByHero, MushroomsCollectedByMonsters, BackAttackCount, MonsterFlankCount, 
               OriginalMaximumWoundedUnits, MaximumWoundedUnits, FreeBomb, BombsUsed, XKillCount, PlusKillsCount, PlatformsDown, PlatformGroups,
               OriginalXKillCount, OriginalPlusKillsCount, BarricadeWoodBark, OriginalHeroBarricadeWood;
    public float BlueRunes, RedRunes, GreenRunes, FreePasses, Vigor, SongOfPeace, Life, EvasionPointsUsed, OriginalEvasionPointsUsed, PlatformPoints, MonsterDamage, OriginalMonsterDamage;
    public List<Vector2> BonfiresLitList;
    public ETileType AreaDragID;
    public static bool FreeEntrance;
    public int AreaClearedTurnCount, OriginalAreaClearedTurnCount, AreaTurnCount, OriginalAreaTurnCount, TouchedBarricadeCount, BarricadeDestroyCount, OriginalBarricadeDestroyCount, TouchedFromOutareaCount;
    public static float BarricadeWood;
    public bool WasClearedWhenEntered = false;
    public  bool Revealed = false;
    public static bool LastAreaClearedWasPerfect = false;
    public static int TotalValidAreas;
    public int TileCount;

    #endregion
    // Use this for initialization
	void Start() 
    {
        Perfect = true;
        OriginalPerfect = true;
        Virgin = true;
        OriginalVirgin = true;
        BigMonsterVirgin = true;
        OriginalBigMonsterVirgin = true;
		Cleared = false;
		OptimalCameraPos = new Vector2( 0, 0 );
        LockAiTarget = new Vector2( -1, -1 );  
		OptimalZoom = 1;
		Discovered = false;
        NumTimesEntered = 0;
        NumTimesExitedWhileDirty = 0;
        TileCount = 0;
        LifetimeSteps = 0;
        MonsterKillCount = 0;
        RoachKillCount = 0;
        TouchedFromOutareaCount = 0;
        BarricadeDestroyCount = OriginalBarricadeDestroyCount = 0;
        ScarabKillCount = 0;
        DirtyBonfiresLit = 0;
        BonfiresLit = 0;
        PoisonerCount = 0;
        Revealed = false;
        PlatformsDown = PlatformGroups = 0;
        PlatformPoints = 0;
        Score = 0;
        AllBurntAvailable = true;
	    //BypassZoomIfCleared = BypassZoomIfDirty = false;
        InitArea();
        BonfiresLitList = new List<Vector2>();
    }
    
    public void InitArea()
    {
        NumTimesEntered = 0;
        NumTimesExitedWhileDirty = 0;
        LifetimeSteps = DirtyLifetimeSteps = OriginalDirtyLifetimeSteps = 0;
        OriginalEvasionPointsUsed = 0;
        ArrowsDestroyed = OriginalArrowsDestroyed = 0;
        OriginalXKillCount = OriginalPlusKillsCount = 0;
        OriginalAreaClearedTurnCount = AreaClearedTurnCount = 0;
        AreaTurnCount = OriginalAreaTurnCount = 0;
        Score = 0;
        TileColor = 0;
        WasClearedWhenEntered = false;
        BarricadeDestroyCount = OriginalBarricadeDestroyCount = 0;
        ResourcesCleared = OriginalResourcesCleared = false;
        MonsterDamage = OriginalMonsterDamage = 0;
        OriginalHeroHitsTaken = 0;
        BlueRunes = RedRunes = GreenRunes = 0;
        Revealed = false;
        LastOutAreaStepTile = new Vector2( -1, -1 );
        //RandomMapSector = new Vector2( -1, -1 );
        float rune = 0; float monst = 0;
        //if( Manager.I.GameType == EGameType.CUBES )
        //    CalculateAreaClearedBonus( ref rune, ref monst, true, Map.I.RM.HeroSector );
    }

    public void ResetLocalVariables( bool restart )
    {
        MushroomsCollectedByHero = MushroomsCollectedByMonsters = BackAttackCount = MonsterFlankCount = 0; 
        //BlueRunes = RedRunes = GreenRunes = 0;
        MonsterCount = RoachCount + ScarabCount + PoisonerCount;
        MaximumWoundedUnits = OriginalMaximumWoundedUnits;
        EvasionPointsUsed = OriginalEvasionPointsUsed;
        ArrowsDestroyed = OriginalArrowsDestroyed;
        Perfect = OriginalPerfect;
        Virgin = OriginalVirgin;
        BigMonsterVirgin = OriginalBigMonsterVirgin;
        AreaClearedTurnCount = OriginalAreaClearedTurnCount;
        AreaTurnCount = OriginalAreaTurnCount;
        ResourcesCleared = OriginalResourcesCleared;
        BarricadeWoodBark = OriginalHeroBarricadeWood;
        HeroData.I.BarricadeWood = HeroData.I.OriginalBarricadeWood;
        BarricadeDestroyCount = OriginalBarricadeDestroyCount;
        G.Hero.Body.HitsTaken = OriginalHeroHitsTaken;
        MonsterDamage = OriginalMonsterDamage;
        BombsUsed = 0;
        TileCount = 0;
        XKillCount = OriginalXKillCount;
        PlusKillsCount = OriginalPlusKillsCount;
        EvasionPointsUsed = 0;
        PlatformsDown = PlatformGroups = 0;
        PlatformPoints = 0;       
        MonsterKillCount = 0;
        RoachKillCount = 0;
        BonfiresLit = 0;
        DirtyBonfiresLit = 0;
        ScarabKillCount = 0;
        PoisonerKillCount = 0;
        if( restart ) SongOfPeace = 0;
    }

  public static string GetRandomAreaName( TextAsset ta = null )
    {
        string fileContents = null;
        if( ta == null ) fileContents = Map.I.RM.AreaNamesTextFile.text;
        else fileContents = ta.text;
        // put data from filename into a variable
        var mydata = fileContents.Split( "\n"[ 0 ] );
        // pick a random number between 1 and lenght
        var myrandom = Random.Range( 1, mydata.Length );
        return mydata[ myrandom ];
    }


  public void UpdateScoreMesh( float score )
  {
      float a = .28f;
      if( Map.I.CurrentArea != -1 )
          if( GlobalID == Map.I.CurrentArea )
          if( Cleared == false )
          a = 0f;

      if( Perfect && Cleared )
          InfoText.color = new Color( 0, 1, 0, a );
      else
          InfoText.color = new Color( 1, 1, 1, a );
      InfoText.text = "" + score.ToString( "0." );

      if( Map.I.RM.RMD.PointsPerTile == 0 )
      {
          InfoText.gameObject.SetActive( false );
      }
      else
      {
          if( InfoText.gameObject.activeSelf == false )
              InfoText.gameObject.SetActive( true );
      }
     // InfoText.transform.position = new Vector3( AreaRect.center.x, (P2.y -P1.y)/2, 0 );
  }

  public float CalculateAreaClearedBonus( ref float rune, ref float monst, bool total, Sector sec, bool neighbor = false )
  {
      int tot = NumTimesExitedWhileDirty;
      
      float penalty = 100 - Map.I.RM.RMD.AreaExitPenalty;
      penalty = Mathf.Clamp( penalty, 0, 100 );

      float points = TL.Count * Map.I.RM.RMD.PointsPerTile;                                                             // Init with points per tile
      float Bonus = 0;

      sec.TotalBonus = Bonus;                                                                                          // Attrib Bonus       

      points += Util.Percent( Bonus, points );                                                                         // Apply Bonus
      
      for( int i = 0; i < tot; i++ ) points = Util.Percent( penalty, points );                                         // Calculate steps penalty

      float turnpenalty = 100 - Map.I.RM.RMD.TurnPenalty;                                                              

      turnpenalty = Mathf.Clamp( turnpenalty, 0, 100 );          

      for( int i = 0; i < DirtyLifetimeSteps; i++ ) points = Util.Percent( turnpenalty, points );                     // Steps penalty
     
      rune += points;
      Score = points;
      return rune + monst;
  }
    
  public void UpdateAreaExitStatistics()
  {
      int monster = RoachKillCount + ScarabKillCount + PoisonerKillCount;
      Statistics.AddStats( Statistics.EStatsType.MONSTERSDEATHCOUNT, monster );
      Statistics.AddStats( Statistics.EStatsType.ROACHDEATHCOUNT, RoachKillCount );
      Statistics.AddStats( Statistics.EStatsType.SCARABDEATHCOUNT, ScarabKillCount );
      Statistics.AddStats( Statistics.EStatsType.POISONERDEATHCOUNT, PoisonerKillCount );
      Statistics.AddStats( Statistics.EStatsType.DIRTYBONFIRESLIT, DirtyBonfiresLit );
      Statistics.AddStats( Statistics.EStatsType.XKILLS, XKillCount );
      Statistics.AddStats( Statistics.EStatsType.PLUSKILLS, PlusKillsCount );
      Statistics.AddStats( Statistics.EStatsType.PLATFORMSDOWN, PlatformsDown );
      Statistics.AddStats( Statistics.EStatsType.PLATFORMPOINTS, PlatformPoints );
      Statistics.AddStats( Statistics.EStatsType.PLATFORMGROUPS, PlatformGroups );      
      Statistics.AddStats( Statistics.EStatsType.BARRICADEWOOD, BarricadeWood );

      //HeroData.I.BarricadeWood = 0;
      PlatformsDown = PlatformGroups = 0;
      PlatformPoints = 0;
      MonsterKillCount = 0;
      RoachKillCount = 0;
      ScarabKillCount = 0;
      PoisonerKillCount = 0;
      DirtyBonfiresLit = 0;
      BonfiresLit = 0;
      XKillCount = 0;
      PlusKillsCount = 0;
  }

    public void ClearResources( bool force = false, bool heroover = true )
  {
      int olda = Map.I.GetPosArea( Map.I.Hero.Control.OldPos );                                       // Neighboor area
      int newa = Map.I.GetPosArea( Map.I.Hero.Pos );
      if( force )
      {
          if( newa != -1 && olda != -1 ) return;
          if( AreaTurnCount < 1 ) return;
      }

      ResourcesCleared = true;
  }
    public void FlushResources()
    {
        if( AreaTurnCount < 1 ) 
        {
            BlueRunes = 0;
            RedRunes = 0;
            GreenRunes = 0;
            FreePasses = 0;
            Vigor = 0;
            SongOfPeace = 0;
            Life = 0;
            return;
        }
        Sector.BackAttackCount += BackAttackCount;
        Sector.MonsterFlankCount += MonsterFlankCount;
        Map.I.LevelStats.MushroomDestroyed += MushroomsCollectedByMonsters;
        MushroomsCollectedByHero = 0;
        MushroomsCollectedByMonsters = 0;
        BackAttackCount = 0;
        MonsterFlankCount = 0;

        Map.I.NumWallsDestroyed += Map.I.CurArea.BombsUsed;
        Map.I.CurArea.BombsUsed = 0;
        OriginalEvasionPointsUsed = EvasionPointsUsed;
        OriginalDirtyLifetimeSteps = DirtyLifetimeSteps;
        OriginalAreaClearedTurnCount = AreaClearedTurnCount;
        OriginalArrowsDestroyed = ArrowsDestroyed;
        OriginalHeroBarricadeWood = BarricadeWoodBark;
        OriginalBarricadeDestroyCount = BarricadeDestroyCount;
        OriginalHeroHitsTaken = G.Hero.Body.HitsTaken;
        OriginalMonsterDamage = MonsterDamage;
        OriginalResourcesCleared = ResourcesCleared;
        OriginalAreaTurnCount = AreaTurnCount;

        OriginalXKillCount = XKillCount;
        OriginalPlusKillsCount = PlusKillsCount;
        
        BarricadeWood = HeroData.I.BarricadeWood - HeroData.I.OriginalBarricadeWood;
        HeroData.I.OriginalBarricadeWood = HeroData.I.BarricadeWood;
        BlueRunes = 0;
        RedRunes = 0;
        GreenRunes = 0;
        FreePasses = 0;
        Vigor = 0;
        SongOfPeace = 0;
        Life = 0;
    }
    
    public bool IsHeroArea()
    {
        if( Map.I.Hero.Pos.x >= P1.x )
            if( Map.I.Hero.Pos.x <= P2.x )
                if( Map.I.Hero.Pos.y >= P2.y )
                    if( Map.I.Hero.Pos.y <= P1.y ) return true;
        return false;
    }

    public void UpdateAreaColor( bool _cleared = false )
    {
        for( int i =0; i < TL.Count; i++ )
        {
            if( Cleared || _cleared )
            {
                Map.I.UpdateAreaMarkTile( Map.I.TransTileMap, TL[ i ], GlobalID, true );
            }
            else
            {
                Map.I.UpdateAreaMarkTile( Map.I.TransTileMap, TL[ i ], GlobalID, false );
            }
        }
        Map.I.TransTileMap.Build();  
    }


    public static void UpdateAiTarget( Vector2 heropos, bool advanceCounter, bool areaexit = false )
    {
        Area ar = Map.I.CurArea;
        if( Map.I.CurrentArea == -1 )                     // Flying Units
        {
            Map.I.LockAiTarget = heropos;
            return;
        }
    }

    public void Clear()
    {
        //for( int i = 0; i < Map.I.MoveOrder.Count; i++ )
        //{
        //    if( Map.I.MoveOrder[ i ].ValidMonster )
        //    {
        //        Map.I.MoveOrder[ i ].Kill();
        //    }
        //}
    }

    public bool IsPTinAreaRect( Vector2 pt )
    {
        if( pt.x >= AreaRect.xMin && pt.x <= AreaRect.xMax )
        if( pt.y >= AreaRect.yMin && pt.y <= AreaRect.yMax ) 
            return true;
        return false;
    }

    public bool AreAllBonfiresLit()
    {
      if( BonfireCount < 1 ) return false;
      if( BonfireCount == BonfiresLit ) return true;
      return false;
    }

    public static Area Get( int area )
    {
        if( Manager.I.GameType == EGameType.FARM ) return null;
        return Quest.I.CurLevel.AreaList[ area ];
    }
    public static Area Get( Vector2 tg )
    {
        if( Map.PtOnMap( Map.I.Tilemap, tg ) == false ) return null;
        if( Manager.I.GameType == EGameType.FARM ) return null;
        int area = Map.I.GetPosArea( tg );
        if( area >= 0 && area < Quest.I.CurLevel.AreaList.Count )
            return Quest.I.CurLevel.AreaList[ area ];
        return null;
    }

    public Vector2 GetCenter()
    {
        return new Vector2( AreaRect.xMin + (( AreaRect.xMax - AreaRect.xMin ) / 2 ), 
                            AreaRect.yMin - (( AreaRect.yMax - AreaRect.yMin ) / 2 ));
    }
    public void Reveal()
    {
        if( Revealed ) return;
        //for( int y = ( int ) P2.y; y <= P1.y; y++ )
        //for( int x = ( int ) P1.x; x <= P2.x; x++ )
        //if( Map.I.AreaID[ x, y ] == GlobalID )
        //    {
        //        Map.I.RevealTile( true, new Vector2( x, y ) );
        //    }

        UpdateAreaColor( Cleared );
        Revealed = true;

        if( LastOutAreaStepTile.x == -1 ) 
            LastOutAreaStepTile = G.Hero.Pos;
    }
}
