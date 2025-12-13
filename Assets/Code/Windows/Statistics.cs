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
        string file = Manager.I.GetProfileFolder();                                       // Base profile folder

        if( nm != "" )
            file += "Cube Save/Statistics" + nm + ".NEO";                                 // Provides filename
        else
            file += "Statistics.NEO";                                                     // Default filename

        string dir = Path.GetDirectoryName( file );                                       // Extract directory path
        if( !Directory.Exists( dir ) )
            Directory.CreateDirectory( dir );                                             // Ensure directory exists

        Statistics st = Map.I.LevelStats;                                                 // Cached statistics reference

        Debug.Log( "save Statistics: " + file + "  " + nm );                              // Debug save path

        using( GS.W = new BinaryWriter( File.Open( file, FileMode.Create ) ) )            // Always recreate file
        {
            int SaveVersion = 1;                                                          // Save Version
            GS.W.Write( SaveVersion );                                                    // Write save version

            GS.W.Write( st.AreasCleared );                                                // int   total cleared areas
            GS.W.Write( st.MonstersDeathCount );                                          // float  monster deaths
            GS.W.Write( st.RoachDeathCount );                                             // int   roach kills
            GS.W.Write( st.ScarabDeathCount );                                            // int   scarab kills
            GS.W.Write( st.NormalSectorsDiscovered );                                     // int   discovered normal sectors
            GS.W.Write( st.SectorsCleared );                                              // int   cleared sectors
            GS.W.Write( st.DirtyBonfiresLit );                                            // int   corrupted bonfires
            GS.W.Write( st.MonstersDiscovered );                                          // int   monster discoveries
            GS.W.Write( st.BonfiresLit );                                                 // int   lit bonfires
            GS.W.Write( st.ResourceCollected );                                           // float  collected resources
            GS.W.Write( st.ConqueredGoals );                                              // int   conquered goals
            GS.W.Write( st.AccumulatedPoints );                                           // float  accumulated points
            GS.W.Write( st.PlatformsDown );                                               // float  destroyed platforms
            GS.W.Write( st.PlatformGroups );                                              // float  platform groups
            GS.W.Write( st.PlatformPoints );                                              // float  platform points
            GS.W.Write( st.BarricadeWood );                                               // float  barricade wood
            GS.W.Write( st.FishingBonusReached );                                         // float  fishing bonus
        }
    }
    public static void Load( string nm = "" )
    {
        string file = Manager.I.GetProfileFolder();                                       // Base profile folder

        if( nm != "" )
            file += "Cube Save/Statistics" + nm + ".NEO";                                 // Provides filename
        else
            file += "Statistics.NEO";                                                     // Default filename

        if( !File.Exists( file ) )
            return;                                                                       // Abort if save does not exist

        Statistics st = Map.I.LevelStats;                                                 // Cached statistics reference

        Debug.Log( "load Statistics: " + file + "  " + nm );                              // Debug load path

        using( GS.R = new BinaryReader( File.Open( file, FileMode.Open ) ) )              // Open existing save
        {
            int SaveVersion = GS.R.ReadInt32();                                           // Load Version

            if( SaveVersion >= 1 )                                                        // Version 1 compatibility
            {
                st.AreasCleared = GS.R.ReadInt32();                                       // int   total cleared areas
                st.MonstersDeathCount = GS.R.ReadSingle();                                // float  monster deaths
                st.RoachDeathCount = GS.R.ReadInt32();                                    // int   roach kills
                st.ScarabDeathCount = GS.R.ReadInt32();                                   // int   scarab kills
                st.NormalSectorsDiscovered = GS.R.ReadInt32();                            // int   discovered normal sectors
                st.SectorsCleared = GS.R.ReadInt32();                                     // int   cleared sectors
                st.DirtyBonfiresLit = GS.R.ReadInt32();                                   // int   corrupted bonfires
                st.MonstersDiscovered = GS.R.ReadInt32();                                 // int   monster discoveries
                st.BonfiresLit = GS.R.ReadInt32();                                        // int   lit bonfires
                st.ResourceCollected = GS.R.ReadSingle();                                 // float  collected resources
                st.ConqueredGoals = GS.R.ReadInt32();                                     // int   conquered goals
                st.AccumulatedPoints = GS.R.ReadSingle();                                 // float  accumulated points
                st.PlatformsDown = GS.R.ReadSingle();                                     // float  destroyed platforms
                st.PlatformGroups = GS.R.ReadSingle();                                    // float  platform groups
                st.PlatformPoints = GS.R.ReadSingle();                                    // float  platform points
                st.BarricadeWood = GS.R.ReadSingle();                                     // float  barricade wood
                st.FishingBonusReached = GS.R.ReadSingle();                               // float  fishing bonus
            }
        }
    }

}
