using UnityEngine;
using System.Collections;
using System.IO;

public class Statistics : MonoBehaviour {

    public enum EStatsType
    {
        NONE = 0, ROACHDEATHCOUNT, SCARABDEATHCOUNT, AREASCLEARED, NORMALSECTORSDISCOVERED, SECTORSCLEARED, TOTALPOINTS,
        BONFIRESLIT, DIRTYBONFIRESLIT, AREASDISCOVERED, MUSHROOMDESTROYED, POISONERDEATHCOUNT, ACCUMULATEDBONUSES,
        MAXBONUSREACHED, MONSTERSDEATHCOUNT, XKILLS, PLUSKILLS, PLATFORMSDOWN, PLATFORMPOINTS, PLATFORMGROUPS, BARRICADEWOOD, 
        MONSTERSDISCOVERED, RESOURCECOLLECTED, FISHINGBONUSREACHED, CONQUEREDGOALS, TOTVAL
    }

    public int RoachDeathCount, ScarabDeathCount, AreasCleared, SectorsDiscovered, NormalSectorsDiscovered, SectorsCleared, NumPerfectAreas, 
               NumPerfectSectors, BonfiresLit, DirtyBonfiresLit, AreasDiscovered, MushroomDestroyed, PoisonerDeathCount, 
               XKillCount, PlusKillCount, MonstersDiscovered, ConqueredGoals;
    public float TotalRunesGained, AccumulatedPoints, AccumulatedBonuses, MaxBonusReached, PlatformsDown,
               PlatformPoints, PlatformGroups, BarricadeWood, ResourceCollected, MonstersDeathCount, FishingBonusReached;

	public void Reset () 
    {
        RoachDeathCount = ScarabDeathCount = AreasCleared = SectorsDiscovered = NormalSectorsDiscovered = SectorsCleared = NumPerfectAreas =
        NumPerfectSectors = BonfiresLit = DirtyBonfiresLit = MonstersDiscovered = 0;
        TotalRunesGained = AccumulatedPoints = AreasDiscovered = MushroomDestroyed =
            PoisonerDeathCount = XKillCount = PlusKillCount = ConqueredGoals = 0;
        AccumulatedBonuses = MaxBonusReached = MonstersDeathCount = 0;
        PlatformsDown = PlatformPoints = PlatformGroups = BarricadeWood = 0;
        ResourceCollected = FishingBonusReached = 0;
	}	

    public static void AddStats( EStatsType tp, int val )
    {
        for( int i = 0; i < 4; i++ )
        {
            Statistics st   = Map.I.LevelStats;
            if( i == 1 ) st = Map.I.GameStats;
            if( i == 2 ) st = Map.I.SectorStats;
            if( i == 3 ) st = Map.I.AreaStats;

            switch( tp )
            {                
                case EStatsType.AREASCLEARED:                    st.AreasCleared            += val; break;
                case EStatsType.SECTORSCLEARED:                  st.SectorsCleared          += val; break;
                case EStatsType.NORMALSECTORSDISCOVERED:         st.NormalSectorsDiscovered += val; break;
                case EStatsType.TOTALPOINTS:                     st.AccumulatedPoints       += val; break;
                case EStatsType.BONFIRESLIT:                     st.BonfiresLit             += val; break;
                case EStatsType.DIRTYBONFIRESLIT:                st.DirtyBonfiresLit        += val; break;
                case EStatsType.XKILLS:                          st.XKillCount              += val; break;
                case EStatsType.PLUSKILLS:                       st.PlusKillCount           += val; break;
                case EStatsType.PLATFORMSDOWN:                   st.PlatformsDown           += val; break;
                case EStatsType.PLATFORMGROUPS:                  st.PlatformGroups          += val; break;
                case EStatsType.MONSTERSDISCOVERED:              st.MonstersDiscovered      += val; break;
                case EStatsType.CONQUEREDGOALS:                  st.ConqueredGoals          += val; break;
            }
        }
        UI.I.UpdGoalText = true;
        if( GS.IsLoading == false )
            Map.I.RM.DungeonDialog.UpdateIt();
    }

    public static void AddStats( EStatsType tp, float val )
    {
        for( int i = 0; i < 4; i++ )
        {
            Statistics st   = Map.I.LevelStats;
            if( i == 1 ) st = Map.I.GameStats;
            if( i == 2 ) st = Map.I.SectorStats;
            if( i == 3 ) st = Map.I.AreaStats;

            switch( tp )
            {
                case EStatsType.PLATFORMPOINTS:                  st.PlatformPoints          += val; break;
                case EStatsType.BARRICADEWOOD:                   st.BarricadeWood           += val; break;                  
                case EStatsType.RESOURCECOLLECTED:               st.ResourceCollected       += val; break;
                case EStatsType.FISHINGBONUSREACHED:             st.FishingBonusReached     += val; break;
                case EStatsType.MONSTERSDEATHCOUNT:              st.MonstersDeathCount      += val; break;
                case EStatsType.ROACHDEATHCOUNT:                 st.RoachDeathCount         += ( int ) val; break;
                case EStatsType.SCARABDEATHCOUNT:                st.ScarabDeathCount        += ( int ) val; break;
                case EStatsType.POISONERDEATHCOUNT:              st.PoisonerDeathCount      += ( int ) val; break;
            }
        }
        UI.I.UpdGoalText = true;
        if( GS.IsLoading == false )
            Map.I.RM.DungeonDialog.UpdateIt();    
    }

    public static void Save( string nm = "" )
    {
        string file = Manager.I.GetProfileFolder();
        if( nm != "" ) file += "Cube Save/Statistics" + nm + ".NEO";                       // Provides filename
        else file += "/Statistics.NEO";
        Sector s = Map.I.RM.HeroSector;
        Statistics st = Map.I.LevelStats;

        using( GS.W = new BinaryWriter( File.Open( file, FileMode.OpenOrCreate ) ) ) 
        {
            int SaveVersion = 1;
            GS.W.Write( SaveVersion );                                                     // Save Version

            GS.W.Write( st.AreasCleared );
            GS.W.Write( st.MonstersDeathCount );
            GS.W.Write( st.RoachDeathCount );
            GS.W.Write( st.ScarabDeathCount );
            GS.W.Write( st.NormalSectorsDiscovered );
            GS.W.Write( st.SectorsCleared );
            GS.W.Write( st.DirtyBonfiresLit );
            GS.W.Write( st.MonstersDiscovered );
            GS.W.Write( st.BonfiresLit );
            GS.W.Write( st.ResourceCollected );
            GS.W.Write( st.ConqueredGoals );
            GS.W.Write( st.AccumulatedPoints );
            GS.W.Write( st.PlatformsDown );
            GS.W.Write( st.PlatformGroups );
            GS.W.Write( st.PlatformPoints );
            GS.W.Write( st.BarricadeWood );
            GS.W.Write( st.FishingBonusReached );
            GS.W.Close();
        }
    }
    public static void Load( string nm = "" )
    {
        string file = Manager.I.GetProfileFolder();
        if( nm != "" ) file += "Cube Save/Statistics" + nm + ".NEO";                      // Provides filename
        else file += "/Statistics.NEO";
        Statistics st = Map.I.LevelStats;
        using( GS.R = new BinaryReader( File.Open( file, FileMode.Open ) ) )
        {
            int SaveVersion = GS.R.ReadInt32();                                           // Load Version
            st.AreasCleared = GS.R.ReadInt32();
            st.MonstersDeathCount = GS.R.ReadSingle();
            st.RoachDeathCount = GS.R.ReadInt32();
            st.ScarabDeathCount = GS.R.ReadInt32();
            st.NormalSectorsDiscovered = GS.R.ReadInt32();
            st.SectorsCleared = GS.R.ReadInt32();
            st.DirtyBonfiresLit = GS.R.ReadInt32();
            st.MonstersDiscovered = GS.R.ReadInt32();
            st.BonfiresLit = GS.R.ReadInt32();
            st.ResourceCollected = GS.R.ReadSingle();
            st.ConqueredGoals = GS.R.ReadInt32();
            st.AccumulatedPoints = GS.R.ReadSingle();
            st.PlatformsDown = GS.R.ReadSingle();
            st.PlatformGroups = GS.R.ReadSingle();
            st.PlatformPoints = GS.R.ReadSingle();
            st.BarricadeWood = GS.R.ReadSingle();
            st.FishingBonusReached = GS.R.ReadSingle();
            GS.R.Close();
        }
    }
}
