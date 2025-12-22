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

        using( var w = new BinaryWriter( File.Open( file, FileMode.Create ) ) )           // Always recreate file
        {
            int SaveVersion = 1;                                                          // Save Version
            w.Write( SaveVersion );                                                    // Write save version

            w.Write( st.AreasCleared );                                                // int   total cleared areas
            w.Write( st.MonstersDeathCount );                                          // float  monster deaths
            w.Write( st.RoachDeathCount );                                             // int   roach kills
            w.Write( st.ScarabDeathCount );                                            // int   scarab kills
            w.Write( st.NormalSectorsDiscovered );                                     // int   discovered normal sectors
            w.Write( st.SectorsCleared );                                              // int   cleared sectors
            w.Write( st.DirtyBonfiresLit );                                            // int   corrupted bonfires
            w.Write( st.MonstersDiscovered );                                          // int   monster discoveries
            w.Write( st.BonfiresLit );                                                 // int   lit bonfires
            w.Write( st.ResourceCollected );                                           // float  collected resources
            w.Write( st.ConqueredGoals );                                              // int   conquered goals
            w.Write( st.AccumulatedPoints );                                           // float  accumulated points
            w.Write( st.PlatformsDown );                                               // float  destroyed platforms
            w.Write( st.PlatformGroups );                                              // float  platform groups
            w.Write( st.PlatformPoints );                                              // float  platform points
            w.Write( st.BarricadeWood );                                               // float  barricade wood
            w.Write( st.FishingBonusReached );                                         // float  fishing bonus
            Debug.Log( "save Statistics: " + file + "  " + nm );                          // Debug save path
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
        {
            Debug.LogError( "[STAT LOAD FAIL] File not found: " + file + "  " + nm );
            return;                                                                       // Abort if save does not exist
        }

        Statistics st = Map.I.LevelStats;                                                 // Cached statistics reference


        using( var r = new BinaryReader( File.Open( file, FileMode.Open ) ) )             // Open existing save
        {
            int SaveVersion = r.ReadInt32();                                           // Load Version

            if( SaveVersion >= 1 )                                                        // Version 1 compatibility
            {
                st.AreasCleared = r.ReadInt32();                                       // int   total cleared areas
                st.MonstersDeathCount = r.ReadSingle();                                // float  monster deaths
                st.RoachDeathCount = r.ReadInt32();                                    // int   roach kills
                st.ScarabDeathCount = r.ReadInt32();                                   // int   scarab kills
                st.NormalSectorsDiscovered = r.ReadInt32();                            // int   discovered normal sectors
                st.SectorsCleared = r.ReadInt32();                                     // int   cleared sectors
                st.DirtyBonfiresLit = r.ReadInt32();                                   // int   corrupted bonfires
                st.MonstersDiscovered = r.ReadInt32();                                 // int   monster discoveries
                st.BonfiresLit = r.ReadInt32();                                        // int   lit bonfires
                st.ResourceCollected = r.ReadSingle();                                 // float  collected resources
                st.ConqueredGoals = r.ReadInt32();                                     // int   conquered goals
                st.AccumulatedPoints = r.ReadSingle();                                 // float  accumulated points
                st.PlatformsDown = r.ReadSingle();                                     // float  destroyed platforms
                st.PlatformGroups = r.ReadSingle();                                    // float  platform groups
                st.PlatformPoints = r.ReadSingle();                                    // float  platform points
                st.BarricadeWood = r.ReadSingle();                                     // float  barricade wood
                st.FishingBonusReached = r.ReadSingle();                               // float  fishing bonus
                Debug.Log( "load Statistics: " + file + "  " + nm );                              // Debug load path
            }
        }
    }

}
