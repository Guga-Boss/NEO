using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;
#if UNITY_EDITOR
using UnityEditor;
#endif
public enum FPowType
{
    NONE = -1,
    Nothing, Show_More_Powers, Upgrade_Base_Chance, Lucky_Streak_Step, Downgrade_Chance, Kill_Worn_Chance, Sort_Candidates, Add_Power_Time,
    Extra_Feather_Bonus = 30, Extra_Honey_Bonus, 
    Sustainable_Eggs = 50, // Static_Chicken, Extra_Eggs
    Refund_Tool = 100, Refund_Seed, BP_Refund_Resource_Cost,
    Free_Diagonal_Move = 200, Diagonal_Around_Forest, Diagonal_Around_Water, Diagonal_Over_Mud, Diagonal_Around_Building,
    Bump_Squash_Plague_Behind = 300, Axe_Carnage, Explode_Flocking_on_Push,
    Plant_Over_Monster = 400, Work_Over_Monster,
    Hurry_Plants = 500, Hurry_Production,
    Perma_Glow = 600
}   //  extra power time, skip reset vars chance

public class FPow : MonoBehaviour 
{
    [HideInInspector]
    public string UniqueID = "";
    [Title( "Power Configuration" )]
    [GUIColor( 0.2f, 1f, 0.2f )]                                        
    public FPowType Type = FPowType.NONE;                                 
    [Space( 10 )]                                                       
    [GUIColor( 0.3f, 0.6f, 1f )]                                           
    [PropertyRange( 1, 5 )]    
    public int Level = 1;
    [Space( 10 )] 
    public float Weight = 100;
    [Title( "Power: " )]
    public float Power = 0;
    [Title( "Uses:" )]
    public int TotalUses = 0;
    [Space( 10 )]
    public bool OnlyFarm = true;
    [HideInEditorMode]
    public float UsesCount = 0;
    [Title( "Resort" )]
    public int TotalResort = 1;
    [HideInEditorMode]
    public int ResortCount = 0;
    public const int Max_Level = 5;
    public static string LastName = "";
    public static FPow LastPow = null, LastUnlim;
    public static bool UpdateText = true;
    public static int TrialCount = 0;
    public static int ExtraPowersShown = 1;
    public static int LuckyStreakStep = 25;
    public static int DowngradeChance = 25;
    public static int UpgradeBaseChance = 25;
    public static List<int> SortTargets;
    public static int TotalSortTargets = 3;
    public static bool TentBuilt = false;
    public static int RelocateAsterix = 0;

#if UNITY_EDITOR
    void OnValidate()
    {
        if( Application.isPlaying ) return;
        if( transform.parent == null ) return;
        if( string.IsNullOrEmpty( UniqueID ) || IsDuplicated() )
        {
            UniqueID = Farm.SortUniqueID( 4 );
            EditorUtility.SetDirty( this );
        }
    }
    bool IsDuplicated()
    {
        FPow[] all = transform.parent.GetComponentsInChildren<FPow>();
        for( int i = 0; i < all.Length; i++ )
        {
            if( all[ i ] == this ) continue;
            if( all[ i ].UniqueID == UniqueID ) return true;
        }
        return false;
    }
#endif

    internal static void UpdateIt()
    {
        if( G.Farm.FirePow.Count == 0 )                                                                 // First time Forced Init
        {
            InitPowers();
        }

        UpdateTargetSorting();                                                                          // Updates Target Sorting

        if( UpdateText || Map.I.AdvanceTurn )
        {
            UpdatePanelText();                                                                          // Update text
            UpdateText = false;
        }
        UpdateFirePowers();                                                                             // Update Fire powers constant loop
        UpdateMouseHelp();                                                                              // Update Tent mouse help
    }

    private static void UpdateTargetSorting()
    {
        if( SortTargets == null )
            SortTargets = new List<int>();                                                              // initialize list if needed;

        List<int> removed = new List<int>();                                                            // track removed targets;

        int removeCount = Mathf.Min( RelocateAsterix, SortTargets.Count );                              // clamp remove amount;
        for( int i = 0; i < removeCount; i++ )
        {
            int index = Random.Range( 0, SortTargets.Count );                                           // pick random target;
            removed.Add( SortTargets[ index ] );                                                        // store removed id;
            SortTargets.RemoveAt( index );                                                              // remove from sort list;
        }

        float bn = FPow.Get( FPowType.Sort_Candidates );                                                // Firepower: Narrow Sort Candidates;
        if( TotalSortTargets >= 2 )
            if( bn > 0 )
            {
                TotalSortTargets += ( int ) bn;                                                         // increase total sort targets;
            }

        int missing = TotalSortTargets - SortTargets.Count;                                             // how many targets are missing;
        if( missing <= 0 )
        {
            RelocateAsterix = 0;                                                                        // reset counter;
            return;
        }

        List<int> pool = new List<int>() { 1, 2, 3, 4, 5 };                                             // available targets;
        for( int i = pool.Count - 1; i >= 0; i-- )
        if( SortTargets.Contains( pool[ i ] ) || removed.Contains( pool[ i ] ) )
            pool.RemoveAt( i );                                                                         // avoid duplicates and re-pick;

        for( int i = 0; i < missing && pool.Count > 0; i++ )
        {
            int index = Random.Range( 0, pool.Count );                                                  // random index;
            SortTargets.Add( pool[ index ] );                                                           // add target;
            pool.RemoveAt( index );                                                                     // remove from pool;
        }         
        RelocateAsterix = 0;                                                                            // reset relocation counter;
    }


    private static void UpdateMouseHelp()
    {
        UI.I.BigTextHelpLabel.gameObject.SetActive( false );
        Unit bld = Map.I.GetUnit( ETileType.BUILDING, G.MP );
        if( bld == null ) return;
        if( bld.Building.Type != BuildingType.Tent ) return;
        //if( Cursor.visible == false ) return;

        int page = 1;
        if( Input.GetMouseButton( 0 ) )                                                                     // Choose page
            page = 2;

        string txt = "";
        if( page == 1 )
        {
            txt += "Fire Power Tutorial:  (Hold Left Button for page 2)\n\n";
            txt += Language.Get( "FIRE_HELP_INTRO", "Main" ) + "\n\n";                                      // intro 

            txt += "Current Stats (values reset to default when the timer reaches zero):\n\n";

            txt += "Upgrade Base Chance 'UP':  " + UpgradeBaseChance + "%:  ";
            txt += Language.Get( "FIRE_VAR_UPGRADE_BASE_CHANCE", "Main" ) + "\n\n";  

            txt += "Lucky Streak: " + LuckyStreakStep + "%:  ";
            txt += Language.Get( "FIRE_VAR_LUCKY_STREAK", "Main" ) + "\n\n";

            txt += "Downgrade Chance 'DN': " + DowngradeChance + "%:  ";
            txt += Language.Get( "FIRE_VAR_DOWNGRADE_CHANCE", "Main" ) + "\n\n";

            txt += "Sort Candidates: +" + TotalSortTargets + "\n";
            txt += Language.Get( "FIRE_VAR_TOTAL_SORT_TARGETS", "Main" ) + "\n\n";  

            txt += "Extra Powers Shown: +" + ExtraPowersShown + "\n";
            txt += Language.Get( "FIRE_VAR_EXTRA_POWERS_SHOWN", "Main" ) + "\n\n";  
        }
        else
        if( page == 2 )
        {
            txt += Language.Get( "FIRE_HELP_INTRO_POWERS", "Main" ) + "\n\n"; 
            int lev = ( int ) Item.GetNum( ItemType.Fire_Level );
            int max = lev + ExtraPowersShown;                                                               // get max visible
            for( int i = 0; i < G.Farm.FirePow.Count; i++ )
            if( i < max )
            {
                FPow p = G.Farm.FirePow[ i ];
                txt += p.GetName() + ":\n";                                                                 // tech name
                txt += Language.Get( "L" + "FIRE_" + p.Type.ToString().ToUpper(), "Main" ) + "\n\n";        // power description
            }
        }

        UI.I.BigTextHelpLabel.gameObject.SetActive( true );
        UI.I.BigTextHelpLabel.text = txt;                                                                   // update text mesh                              
    }

    private static void UpdateFirePowers()
    {
        Unit bld = Map.I.GetUnit( ETileType.BUILDING, G.Hero.GetFront() );                              // Fire Power: Hurry up frontal Building Production
        if( bld )                                                                                       // check building in front
        if( bld.Building.Type != BuildingType.Tent )                                                    // ignore tent
        {
            BuildingItem bi = bld.Building.Itm[ bld.Building.SelItemID ];                               // selected building item

            float mstack = Building.GetStat( EVarType.Maximum_Item_Stack,
                                             bld.Building, bld.Building.SelItemID );                    // max stack

            float prod = Building.GetStat( EVarType.Total_Building_Production_Time,
                                           bld.Building, bld.Building.SelItemID );                      // total production time

            bool isPlant = ( bld.Building.Category == EBuildingCategory.Plant );                        // plant check

            FPowType powType = isPlant                                                                  // choose correct power
                ? FPowType.Hurry_Plants
                : FPowType.Hurry_Production;

            float pow = Get( powType, false );                                                          // seconds to consume 3600

            bool canApply = true;                                                                       // power gate

            if( isPlant && bi.ProductionTimeCount >= prod ) canApply = false;                           // plant already finished
            if( !isPlant && mstack > 0 && bi.ItemCount >= mstack ) canApply = false;                    // stack full
            if( pow <= 0f ) canApply = false;                                                           // power inactive
            if( bi.BaseTotalProductionTime <= 0 ) canApply = false;                                     // invalid production
            if( LastPow == null ) canApply = false; else
            if( LastPow.UsesCount >= LastPow.TotalUses ) canApply = false;                              // power exhausted

            if( canApply )                                                                              // apply acceleration
            {
                float dt = Time.unscaledDeltaTime;                                                      // real delta time
                float accel = 3600f / pow;                                                              // acceleration factor
                float step = dt * accel;                                                                // accelerated seconds

                float avail = LastPow.TotalUses - LastPow.UsesCount;                                    // remaining accelerated time
                if( avail > 0f )                                                                        // power available
                {
                    step = Mathf.Min( step, avail );                                                    // clamp usage
                    bi.ProductionTimeCount += step;                                                     // accelerate production
                    LastPow.UsesCount += step;                                                          // consume power
                    UpdateText = true;                                                                  // refresh UI
                    LastPow.ClampUse();                                                                 // clamp
                }
            }
        }

        if( FPow.Has( FPowType.Axe_Carnage, false ) )                                                   // Firepower: Plague Monster Destroyed on push
        {
            Unit plague = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, G.Hero.GetFront() );                 // Fire Power: Axe Carnage
            if( plague )
            if( G.Farm.SelectedItem == ItemType.WoodAxe )
            if( G.Farm.CarryingAmount > 0 )
            if( Map.I.TimeKillList.Contains( plague ) == false ) 
            {
                Map.I.CreateExplosionFX( plague.Pos, "Fire Explosion" );                                // FX
                Map.TimeKill( plague, .3f );
                FPow.Has( FPowType.Axe_Carnage );                                                       // use power
            }
        }
            
        float bn = FPow.Get( FPowType.Upgrade_Base_Chance, true );                                      // Firepower: Upgrade Base Chance
        if( bn != 0 )
        {
            UpgradeBaseChance += ( int ) bn;
        }

        bn = FPow.Get( FPowType.Lucky_Streak_Step, true );                                                // Firepower: Lucky streak step
        if( bn != 0 )
        {
            LuckyStreakStep += ( int ) bn;
        }

        bn = FPow.Get( FPowType.Downgrade_Chance, true );                                                // Firepower: Downgrade Chance
        if( bn != 0 )
        {
            DowngradeChance += ( int ) bn;
            if( DowngradeChance < 0 ) 
                DowngradeChance = 0;
        }

        bn = FPow.Get( FPowType.Add_Power_Time, false );                                                 // Firepower: Add Power Time
        if( bn != 0 )
        {
            float secondsPerSecond = LastPow.TotalUses / bn;                                             // consume rate
            float partial = Time.unscaledDeltaTime * secondsPerSecond;
            List<BuildingItem> bi = Building.GetBuildingItemList( ItemType.Fire_Token );
            bi[ 0 ].ProductionTimeCount -= partial;                                                      // applies time
            LastPow.Use( partial );                                                                      // consume same time
            LastPow.ClampUse();
            UpdateText = true;
        }
    }
    private void ClampUse()
    {
        if( UsesCount > TotalUses )
            UsesCount = TotalUses;  
    }

    public static void UpdateCycle( BuildingItem bi )
    {
        bi.ProductionTimeCount = 0;
        if( --ExtraPowersShown < 1 ) 
            ExtraPowersShown = 1;
        UpgradeBaseChance = 25;
        LuckyStreakStep = 25;
        DowngradeChance = 25;
        TotalSortTargets = 3;
        Item.AddItem( ItemType.Fire_Level, -1 );                                // Decrement Level
        Item.Clamp( ItemType.Fire_Level, 0, Max_Level );
        UpdateText = true;
    }

    public static void UpdatePanelText()
    {
        UI.I.MidPanelSprite.gameObject.SetActive( true );
        UI.I.NavigationMapText.gameObject.SetActive( true );                                            // update meshes and objects
        UI.I.NavigationMapText.color = Color.green;
        if( TentBuilt == false )
        {
            string pn = Manager.I.GetPlayerName();
            UI.I.NavigationMapText.text = "\n      Welcome to the Farm,\n      " + pn + "!"; 
            return;
        }
        UI.I.NavigationMapText2.gameObject.SetActive( true );
        UI.I.NavigationMapText2.color = Color.red;
        UI.I.NavigationMapText.text = "";
        UI.I.NavigationMapText2.text = "";
        UI.I.MidPanelSprite.spriteName = "Fire Icon";
        int lev = ( int ) Item.GetNum( ItemType.Fire_Level );
        float upchc = UpgradeBaseChance + TrialCount * LuckyStreakStep;
        UI.I.ArtifactInfoLabel.text = "Fire Level: " + lev + " UP: " + 
        upchc + "% - DN: " + DowngradeChance + "%";

        int max = lev;
        float bn = FPow.Get( FPowType.Show_More_Powers, false );                                         // Firepower: Show more Powers
        int used = ( int ) Mathf.Min( bn, Max_Level - lev );                                             // Calculates how many are useful to be spent
        if( bn > 0 && used > 0 )
        {
            ExtraPowersShown += ( int ) used;
            LastPow.Use( used );                                                                         // use only necessary
        }
        max += ExtraPowersShown;

        for( int i = 0; i < G.Farm.FirePow.Count; i++ )
        if( i < max )
        {
            FPow p = G.Farm.FirePow[ i ];
            string nm = p.GetName( i + 1 );
            if( i < lev )
                UI.I.NavigationMapText.text += nm + "\n";                                          // uses 2 text meshes for multicolor effect
            UI.I.NavigationMapText2.text += nm + "\n";
        }
    }
    public void Use( float amt )
    {
        UsesCount += amt;
    }
    public static bool Has( FPowType type, bool increment = true )
    {       
        if( G.Farm.FirePow == null ) return false;
        int lev = Mathf.Clamp( ( int ) Item.GetNum( ItemType.Fire_Level ), 0, G.Farm.FirePow.Count );
        bool val = false;
        for( int i = 0; i < lev; i++ )
        {
            FPow p = G.Farm.FirePow[ i ];
            if( p.OnlyFarm == false ||                                                                       // Farm restriction
                Manager.I.GameType == EGameType.FARM ) 
            if( p.Type == type )
            {
                if( p.TotalUses <= 0 ||
                    p.UsesCount < p.TotalUses )                                                              // Max uses reached
                {
                    LastPow = p;
                    LastName = p.GetName();
                    val = true;
                }

                if( p.TotalUses <= 0 )                                                                       // Found an unlimited Power
                    LastUnlim = p;
                UpdateText = true;
            }
        }
        if( val )
        if( increment )
            LastPow.UsesCount++;                                                                             // increment uses

        return val;
    }

    public static float Get( FPowType type, bool increment = true )
    {
        if( Manager.I.GameType != EGameType.FARM ) return 0;
        if( G.Farm.FirePow == null ) return 0;

        int lev = Mathf.Clamp( ( int ) Item.GetNum( ItemType.Fire_Level ), 0, G.Farm.FirePow.Count );
        float val = 0;
        for( int i = 0; i < lev; i++ )
        {
            FPow p = G.Farm.FirePow[ i ];
            if( p.OnlyFarm == false ||
                Manager.I.GameType == EGameType.FARM )                                                        // Farm restriction
            if( p.Type == type )
            {
                if( p.TotalUses <= 0 ||
                    p.UsesCount < p.TotalUses )                                                               // Max uses reached
                {
                    LastName = p.GetName();
                    LastPow = p;
                    val += p.Power;
                }
                if( p.TotalUses <= 0 )                                                                        // Found an unlimited Power
                    LastUnlim = p;
            }
        }
        if( val != 0 )
        if( increment )
            LastPow.UsesCount++;                                                                              // increment uses
        return val;
    }
    public static FPow SortPower( int lev, bool resetOldSlot = true )
    {
        if( resetOldSlot && G.Farm.FirePow[ lev - 1 ] != null )
            G.Farm.FirePow[ lev - 1 ].ResetData();                              // restore default data for the old power
        
        List<FPow> pl = G.Farm.FirePowMaster;                                   // link list
        List<int> idlist = new List<int>();
        List<float> fact = new List<float>();
        for( int i = 0; i < pl.Count; i++ )
        {
            if( lev == pl[ i ].Level )
            if( pl[ i ].ResortCount >= 1 ||
                G.Farm.FirePow.Contains( pl[ i ] ) == false )
            {
                idlist.Add( i );                                                // attrib ID
                fact.Add( pl[ i ].Weight );                                     // atrib weight for each level
            }
        }

        if( idlist.Count == 0 ) return null;                                    // nothing to sort, exit

        int id = Util.Sort( fact );                                             // Sort ID
        id = idlist[ id ];

        if( id < 0 || id >= pl.Count )                                          // Bug protection
            return null;

        G.Farm.FirePow[ lev - 1 ] = pl[ id ];                                   // assign power

        FPow p = pl[ id ]; 

        if( p.TotalResort != -1 )
        {
            if( p.ResortCount > 0 )
                p.ResortCount--;                                                // Decrement  resort count
        }
        return p;
    }
    private void ResetData()
    {
        ResortCount = TotalResort;                                             // Reset Data
        UsesCount = 0;
    }

    internal static bool UpdateBuildingBump()
    {
        bool res = false;
        Unit frbld = Map.I.GetUnit( ETileType.BUILDING, G.Hero.GetFront() );                           // Tent Frontal Bump
        if( frbld && frbld.Building.Type == BuildingType.Tent )                                        
        if( SortTargets.Count > 0 )
        {
            if( Building.AddItem( true, ItemType.Fire_Token, -1 ) == 0 ) 
                return true;                                                                           // Charge Fire Token

            int id = Random.Range( 0, SortTargets.Count );                                             // Pick from the list
            id = SortTargets[ id ];
            float bn = FPow.Get( FPowType.Kill_Worn_Chance, false );                                   // Firepower: Kill worn first
            string kill = "Old: ";
            if( Util.Chance( bn ) )                                                                    // Kill Worn Chance
            {
                for( int i = G.Farm.FirePow.Count - 1; i >= 0; i-- )
                if ( G.Farm.FirePow[ i ].TotalUses > 0 )
                if ( G.Farm.FirePow[ i ].TotalUses == G.Farm.FirePow[ i ].UsesCount )
                {
                    kill = "Kill Worn: ";
                    id = i + 1;
                    LastPow.Use( 1 );                                                                  // use
                    break;
                }
            }
            if( SortTargets.Contains( id ) )                                                           // Removes ID from Sort Target
                SortTargets.Remove( id );

            string msg = kill + G.Farm.FirePow[ id - 1 ].GetName();
            Message.RedMessage( ItemType.Fire_Level, msg );                                            // sorted msg
            FPow p = FPow.SortPower( id );
            if( p ) 
                Message.GreenMessage( ItemType.Fire_Level, "New: " + p.GetName() );                    // sorted msg
            return true;            
        }

        Unit bump = Map.I.GetUnit( ETileType.BUILDING, Map.I.BumpTarget );                             // Tent Bump
        if( bump && bump.Building.Type == BuildingType.Tent )                                          // bump action triggered
        {
            res = true;
            float upChance = Mathf.Min( UpgradeBaseChance + TrialCount * LuckyStreakStep, 100f );      // cumulative upgrade chance capped at 100%
            bool upgradeSuccess = Util.Chance( upChance );                                             // roll for upgrade
            bool downgradeSuccess = Util.Chance( DowngradeChance );                                    // roll for downgrade

            if( Item.GetNum( ItemType.Fire_Level ) < Max_Level )
            if( Item.GetNum( ItemType.Fire_Token ) >= 1 )
            {
                RelocateAsterix = 1;                                                                    // Relocates a new asterix
                Item.IgnoreMessage = true;
                Building.AddItem( true, ItemType.Fire_Token, -1 );                                      // Charge Fire Token
                if( upgradeSuccess )                                                                    // if upgrade succeeded
                {
                    Item.IgnoreMessage = true;
                    Item.AddItem( ItemType.Fire_Level, +1 );                                            // increment Fire Level
                    Message.GreenMessage( ItemType.Fire_Level, "UP!" );                                 // show upgrade message
                    MasterAudio.PlaySound3DAtVector3( "Cashier", G.Hero.Pos );                          // play upgrade sound
                    TrialCount = 0;                                                                     // reset trial count
                    Controller.CreateMagicEffect( bump.Pos );                                           // Magic FX  
                }
                else if( downgradeSuccess && Item.GetNum( ItemType.Fire_Level ) > 0 )                   // if downgrade succeeded and level > 0
                {
                    Item.AddItem( ItemType.Fire_Level, -1 );                                            // decrement Fire Level
                    Message.RedMessage( ItemType.Fire_Level, "DN!" );                                   // show downgrade message
                    MasterAudio.PlaySound3DAtVector3( "Error", G.Hero.Pos );                            // play downgrade sound
                }
                else                                                                                    // if neither upgrade nor downgrade
                {
                    TrialCount++;                                                                       // increment trial count for next cumulative chance
                    float c = upChance + LuckyStreakStep;
                    Message.WhiteMessage( ItemType.Fire_Level, "Lucky Streak: " +
                    c + "%"  + " (+" + LuckyStreakStep + "%)");                                         // Msg
                }
            }
        }

        if( bump )
        if( FPow.Has( FPowType.Bump_Squash_Plague_Behind, false ) )                                    // Firepower: Bump Building to squash plague behind
        {
            EDirection dr = Util.GetTargetUnitDir( G.Hero.Pos, bump.Pos );
            Vector2 tg = bump.Pos + Manager.I.U.DirCord[ ( int ) dr ];
            Unit plague = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, tg );
            if( plague )                                                                               // monster found
            {
                Map.I.CreateExplosionFX( plague.Pos, "Fire Explosion" );                               // FX
                Map.TimeKill( plague, .3f );
                FPow.Has( FPowType.Bump_Squash_Plague_Behind );                                        // Use power
                res = true;
            }
        }
        UpdateText = true;
        return res;
    }

    public static void UpdateFirePowerNames()
    {
        FPow[] all = G.Farm.FirePowerFolder.GetComponentsInChildren<FPow>();                             // name all FPow gameobj  
        G.Farm.FirePowMaster = new List<FPow>( all );

        for( int i = 0; i < all.Length; i++ )
        {         
            FPow p = all[ i ];
            p.name = p.GetName();          
        }

        for( int j  = 0; j  < all.Length;  j++ )                                                         // Check for duplicated IDs
        for( int jj = 0; jj < all.Length; jj++ )
        if ( j != jj )
        if ( all[ j  ].UniqueID ==  all[ jj ].UniqueID ) 
             Debug.LogError( "Duplicated Fire Power ID: " + all[ j  ].name );
    }
    public string GetName( int lev = -1 )
    {
        string ini = "";
        if( lev > 0 )                                                                                    // Adds symbol to identify next sort target
        {
            ini = "L";
            if( SortTargets.Contains( lev ) )
                ini = "*L";
        }

        string nm = ini + Level + " - " + Type.ToString();                                               // Level and name
        nm = nm.Replace( '_', ' ' );

        float rem = TotalUses - UsesCount;
        if( Type == FPowType.Hurry_Production || Type == FPowType.Hurry_Plants || 
            Type == FPowType.Add_Power_Time )                                                             // These use timer
        {
            if( IsWorn() == false )
                nm += " " + Util.ToSTime( rem );
            else nm += " worn";
        }
        else
        {
            if( Power != 0 )
                nm += " " + Power.ToString( "+#;-#;0" );                                                 // power
            if( Type == FPowType.BP_Refund_Resource_Cost ||                                              // these use percent %
                Type == FPowType.Refund_Seed         ||
                Type == FPowType.Refund_Tool         ||
                Type == FPowType.Upgrade_Base_Chance ||
                Type == FPowType.Lucky_Streak_Step   ||
                Type == FPowType.Downgrade_Chance    ||
                Type == FPowType.Kill_Worn_Chance    ) 
                nm += "%";
            if( TotalUses > 0 )
            {
                if( Type == FPowType.Upgrade_Base_Chance    ||                                           // these are forced to have only one usage
                    Type == FPowType.Lucky_Streak_Step      ||
                    Type == FPowType.Downgrade_Chance       ||
                    Type == FPowType.Show_More_Powers       ||
                    Type == FPowType.Sort_Candidates )
                {
                    if( lev > 0 )
                        nm += " worn";
                    else
                        nm += "";
                }
                else
                if( TotalUses == UsesCount )
                    nm += " worn";
                else
                    nm += " x" + ( rem );                                                    // uses 
            }
        }

        if( UniqueID == "" ) UniqueID = Farm.SortUniqueID( 5 );
        return nm;
    }

    private bool IsWorn()
    {
        if( TotalUses > 0 )
        if( UsesCount >= TotalUses ) return true;
        return false;
    }
    public static void Save()
    {
        TF.SaveT( "TrialCount", TrialCount );                                                // Save Trial Count
        TF.SaveT( "ExtraPowersShown", ExtraPowersShown );                                    // Save Extra Powers Shown
        TF.SaveT( "BaseUpgradeChance", UpgradeBaseChance );                                  // Save Base Upgrade Chance
        TF.SaveT( "LuckyStreakStep", LuckyStreakStep );                                      // Save Lucky Streak Step
        TF.SaveT( "DowngradeChance", DowngradeChance );                                      // Save Downgrade Chance
        TF.SaveT( "SortTargets", SortTargets );                                              // Save Sort Targets

        TF.SaveT( "FPowSize", G.Farm.FirePow.Count );                                        // Save powers list size

        List<string> idlist = new List<string>();
        List<int> reslist = new List<int>();
        List<float> useslist = new List<float>();

        for( int i = 0; i < G.Farm.FirePow.Count; i++ )                                      // fill lists
        {
            idlist.Add( G.Farm.FirePow[ i ].UniqueID );
            reslist.Add( G.Farm.FirePow[ i ].ResortCount );
            useslist.Add( G.Farm.FirePow[ i ].UsesCount );
        }

        TF.SaveT( "IDList_", idlist );                                                      // Save Unique ID list       
        TF.SaveT( "ResortCount_", reslist );                                                // Save Resort counts list 
        TF.SaveT( "UsesList_", useslist );                                                  // Save Uses list  
    }
    public static void Load()
    {
        TrialCount = TF.LoadT<int>( "TrialCount" );                                         // Load TrialCount
        ExtraPowersShown = TF.LoadT<int>( "ExtraPowersShown" );                             // Load Extra Powers Shown
        UpgradeBaseChance = TF.LoadT<int>( "UpgradeBaseChance" );                           // Load Upgrade Base Chance
        LuckyStreakStep = TF.LoadT<int>( "LuckyStreakStep" );                               // Load Lucky Streak Step
        DowngradeChance = TF.LoadT<int>( "DowngradeChance" );                               // Load Downgrade Chance
        //SortTargets = TF.LoadT<List<int>>( "SortTargets" );                                 // Load Sort Targets     

        int sz = TF.LoadT<int>( "FPowSize" );                                               // Load powers list size

        if( sz == 0 )                                                                       // First time
        {
            InitPowers();
        }
        else                                                                                // Subsequent times
        {
            List<string> idlist = TF.LoadT<List<string>>( "IDList_" );                      // Load Unique ID list   
            List<int> reslist = TF.LoadT<List<int>>( "ResortCount_" );                      // Load item counts list
            List<float> useslist = TF.LoadT<List<float>>( "UsesList_" );                    // Load uses list
            for( int i = 0; i < sz; i++ )
            {
                string id = idlist[ i ];
                for( int j = 0; j < G.Farm.FirePowMaster.Count; j++ )
                {
                    FPow p = G.Farm.FirePowMaster[ j ];
                    if( p.UniqueID == id )
                    {              
                        p.ResortCount = reslist[ i ];                                      // attrib values
                        p.UsesCount = useslist[ i ];
                        break;
                    }
                }
            }
        }
        UpdateText = true;
    }
    private static void InitPowers()
    {
        G.Farm.FirePow = new List<FPow>();         // create new list of Fire Powers;  initialize the list
        for( int i = 0; i < Max_Level; i++ )
            G.Farm.FirePow.Add( null );            // add null slots up to Max_Level;  prepare empty slots

        foreach( var p in G.Farm.FirePowMaster )
        {
            p.ResortCount = p.TotalResort;         // reset ResortCount to default;  ensures each power can be resorted
            p.UsesCount = 0;                       // reset UsesCount;  power starts unused
        }

        for( int i = 0; i < Max_Level; i++ )
        {
            SortPower( i + 1, false );             // sort power into slot i; // false = don't reset old slot because it's empty
        }
    }


    internal static bool CheckDiagonalMove()
    {
        bool res = false;
        LastUnlim = null;
        if( FPow.Has( FPowType.Free_Diagonal_Move, false ) ) res = true;                                    // Firepower: Free diagonal move
        int forest = 0, water = 0, bld = 0;
        Vector2 tg = G.Hero.Pos;
        int rad = 1;                          
        for( int y = ( int ) tg.y - rad; y <= tg.y + rad; y++ )                                            
        for( int x = ( int ) tg.x - rad; x <= tg.x + rad; x++ )
            {
                if( Map.I.GetUnit( ETileType.FOREST, new Vector2( x, y ) ) ) forest++;                      // Count tiles
                if( Map.I.GetUnit( ETileType.WATER, new Vector2( x, y ) ) ) water++;
                Unit bl = Map.I.GetUnit( ETileType.BUILDING, new Vector2( x, y ) );
                if( bl && bl.Building.Category != EBuildingCategory.Plant && 
                    bl.Building.Category != EBuildingCategory.Work_Area ) bld++;
            }
        if( forest > 0 && FPow.Has( FPowType.Diagonal_Around_Forest,   false ) ) res = true;                // Firepower: Free diagonal move around Forest
        if( water > 0  && FPow.Has( FPowType.Diagonal_Around_Water,    false ) ) res = true;                // Firepower: Free diagonal move around Water
        if( bld > 0    && FPow.Has( FPowType.Diagonal_Around_Building, false ) ) res = true;                // Firepower: Free diagonal move around Building

        if( res )
        {
            if( LastUnlim ) LastPow = LastUnlim;                                                            // Prioritize Unlimited power
        }
        return res;
    }
}
