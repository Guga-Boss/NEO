using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
public enum FPowType
{
    NONE = -1,
    Nothing, 
    Extra_Feather_Bonus = 10, Extra_Honey_Bonus, 
    Sustainable_Eggs = 50, // Static_Chicken, Extra_Eggs
    Refund_Tool = 100, Refund_Seed, BP_Refund_Resource_Cost,
    Free_Diagonal_Move = 200, Diagonal_Around_Forest, Diagonal_Around_Water, Diagonal_Over_Mud, Diagonal_Around_Building,
    Bump_Squash_Plague_Behind = 300, Axe_Carnage, Explode_Flocking_on_Push,
    Plant_Over_Monster = 400, Work_Over_Monster,
    Hurry_Plants = 500, Hurry_Production,
    Perma_Glow = 600
}

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
    public static string LastName = "";
    public static FPow LastPow = null, LastUnlim;
    public static bool UpdateText = true;

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
        if( UpdateText || Map.I.AdvanceTurn )
        {
            UpdatePanelText();                                                 // Update text
            UpdateText = false;
        }
        UpdateFirePowers();
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

                    if( LastPow.UsesCount > LastPow.TotalUses )
                        LastPow.UsesCount = LastPow.TotalUses;                                          // clamp
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
    }

    public static void UpdateCycle( BuildingItem bi )
    {
        SortPowers();
        bi.ProductionTimeCount = 0;
    }

    public static void SortPowers()
    {
        List<FPow> pl = new List<FPow>( G.Farm.FirePowMaster );                 // create list
        G.Farm.FirePow.Clear();                                                 // reset previous powers
        int count = Mathf.Min( 5, pl.Count );                                   // safety check

        for( int i = 0; i < pl.Count; i++ )
        {
            pl[ i ].ResortCount = pl[ i ].TotalResort;                          //
            pl[ i ].UsesCount = 0;
        }

        for( int lev = 1; lev <= count; lev++ )
        {
            List<int> idlist = new List<int>();
            List<float> fact = new List<float>();
            for( int p = 0; p < pl.Count; p++ )
            {
                if( lev == pl[ p ].Level )
                {
                    fact.Add( pl[ p ].Weight );                                // atrib weight for each level
                    idlist.Add( p );
                }
            }

            int id = Util.Sort( fact );                                        // Sort ID
            id = idlist[ id ];

            if( id < 0 || id >= pl.Count )                                     // Bug protection
                break;

            G.Farm.FirePow.Add( pl[ id ] );                                    // assign power
            if( pl[ id ].TotalResort != -1 )
            {
                if( pl[ id ].ResortCount > 0 )
                    pl[ id ].ResortCount--;

                if( pl[ id ].ResortCount == 0 )
                    pl.RemoveAt( id );                                        // avoid duplicates
            }
        }
        Item.AddItem( ItemType.Fire_Level, -1 );
        if( Item.GetNum( ItemType.Fire_Level ) < 0 )
            Item.SetAmt( ItemType.Fire_Level, 0 );
        UpdateText = true;
    }

    public static void UpdatePanelText()
    {      
        UI.I.NavigationMapText.color = Color.green;
        UI.I.NavigationMapText.gameObject.SetActive( true );                                            // update meshes and objects
        UI.I.NavigationMapText2.gameObject.SetActive( true );
        UI.I.NavigationMapText2.color = Color.red;
        UI.I.NavigationMapText.text = "";
        UI.I.NavigationMapText2.text = "";
        UI.I.MidPanelSprite.spriteName = "Fire Icon";
        UI.I.MidPanelSprite.gameObject.SetActive( true );
        int lev = ( int ) Item.GetNum( ItemType.Fire_Level );
        UI.I.ArtifactInfoLabel.text = "Fire Level: " + lev;
        for( int i = 0; i < G.Farm.FirePow.Count; i++ )
        {
            FPow p = G.Farm.FirePow[ i ];
            string nm = p.GetName();
            if( i < lev )
                UI.I.NavigationMapText.text += "L" + nm + "\n";                      // uses 2 text meshes for multicolor effect
            UI.I.NavigationMapText2.text += "L" + nm + "\n";
        }
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

    internal static bool UpdateBuildingBump()
    {
        bool res = false;
        Unit frbld = Map.I.GetUnit( ETileType.BUILDING, G.Hero.GetFront() );
        Unit bump = Map.I.GetUnit( ETileType.BUILDING, Map.I.BumpTarget );
        if( frbld && frbld.Building.Type == BuildingType.Tent )                                            // Tent Bump
        {
            FPow.SortPowers();
            res = true;            
        }

        if( bump )
            Item.AddItem( ItemType.Fire_Level, +1 );

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
        FPow[] all = G.Farm.FirePowerFolder.GetComponentsInChildren<FPow>();                           // name all FPow gameobj  
        G.Farm.FirePowMaster = new List<FPow>( all );

        for( int i = 0; i < all.Length; i++ )
        {         
            FPow p = all[ i ];
            p.name = p.GetName();          
        }

        for( int j  = 0; j  < all.Length;  j++ )                                                        // Check for duplicated IDs
        for( int jj = 0; jj < all.Length; jj++ )
        if ( j != jj )
        if ( all[ j  ].UniqueID ==  all[ jj ].UniqueID ) 
             Debug.LogError( "Duplicated Fire Power ID: " + all[ j  ].name );
    }
    public string GetName()
    {
        string nm = "" + Level + " - " + Type.ToString();                                                // Level and name
        nm = nm.Replace( '_', ' ' );

        float rem = TotalUses - UsesCount;
        if( Type == FPowType.Hurry_Production || Type == FPowType.Hurry_Plants )                         // These use timer
            nm += " " + Util.ToSTime( rem );
        else
        {
            if( Power != 0 )
                nm += " " + Power.ToString( "+#;-#;0" );                                                 // power
            if( Type == FPowType.BP_Refund_Resource_Cost || 
                Type == FPowType.Refund_Seed || 
                Type == FPowType.Refund_Tool ) nm += " %";
            if( TotalUses > 0 ) nm += " x" + ( rem );                                                    // uses 
        }

        if( UniqueID == "" ) UniqueID = Farm.SortUniqueID( 5 );
        return nm;
    }
    public static void Save()
    {
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
        G.Farm.FirePow.Clear();                                                             // reset previous powers

        int sz = TF.LoadT<int>( "FPowSize" );                                               // Load powers list size

        List<string> idlist = TF.LoadT<List<string>>( "IDList_" );                          // Load Unique ID list   
        List<int> reslist = TF.LoadT<List<int>>( "ResortCount_" );                          // Load item counts list
        List<float> useslist = TF.LoadT<List<float>>( "UsesList_" );                        // Load uses list

        for( int i = 0; i < sz; i++ )
        {
            string id = idlist[ i ];
            for( int j = 0; j < G.Farm.FirePowMaster.Count; j++ )
            {
                FPow p = G.Farm.FirePowMaster[ j ];
                if( p.UniqueID == id )
                {
                    G.Farm.FirePow.Add( p );                                                 // attrib values
                    p.ResortCount = reslist[ i ];
                    p.UsesCount = useslist[ i ];
                    break;
                }
            }
        }
        UpdateText = true;
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
