using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FPowType
{
    NONE = -1,
    Nothing, void1, Extra_Feather_Bonus, Extra_Honey_Bonus, Hurry_Production, 
    BP_Refund_Resource_Cost, Less_X_Plague_Monster, void3, void4,
    Hurry_Plants, Explode_Flocking_on_Push, 
    Refund_Tool = 100, Refund_Seed,
    Free_Diagonal_Move = 200,
}

public class FPow : MonoBehaviour 
{
    public FPowType Type = FPowType.NONE;
    public float Power = 0;
    public int TotalUses = 0;
    public float UsesCount = 0;
    public int TotalResort = 1;
    public int ResortCount = 0;
    public string UniqueID = "";

    [Space( 30 )]
    public float Level1Chance = 0;
    public float Level2Chance = 0;
    public float Level3Chance = 0;
    public float Level4Chance = 0;
    public float Level5Chance = 0;
    public static string LastName = "";
    public static FPow LastPow = null;
    public static bool UpdateText = true;


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
            List<float> fact = new List<float>();
            for( int p = 0; p < pl.Count; p++ )
            {
                if( lev == 1 ) fact.Add( pl[ p ].Level1Chance );               // atribb weight for each level
                if( lev == 2 ) fact.Add( pl[ p ].Level2Chance );
                if( lev == 3 ) fact.Add( pl[ p ].Level3Chance );
                if( lev == 4 ) fact.Add( pl[ p ].Level4Chance );
                if( lev == 5 ) fact.Add( pl[ p ].Level5Chance );
            }

            int id = Util.Sort( fact );                                        // Sort ID

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
        UI.I.NavigationMapText.gameObject.SetActive( true );
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
                UI.I.NavigationMapText.text += "L" + ( i + 1 ) + ": " + nm + "\n";                      // uses 2 text meshes for multicolor effect
            UI.I.NavigationMapText2.text += "L" + ( i + 1 ) + ": " + nm + "\n";
        }
    }
    public static bool Has( FPowType type, bool increment = true )
    {
        if( Manager.I.GameType != EGameType.FARM ) return false;
        if( G.Farm.FirePow == null ) return false;

        int lev = Mathf.Clamp( ( int ) Item.GetNum( ItemType.Fire_Level ), 0, G.Farm.FirePow.Count );

        for( int i = 0; i < lev; i++ )
        {
            FPow p = G.Farm.FirePow[ i ];
            if( p.Type == type )
            {
                if( increment )
                    p.UsesCount++;
                if( p.TotalUses > 0 )
                {
                    if( p.UsesCount >= p.TotalUses )                                                    // Max uses reached
                        return false;
                }
                LastName = p.GetName();
                LastPow = p;
                UpdateText = true;
                return true;
            }
        }
        return false;
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
            if( p.Type == type )
            {
                LastName = p.GetName();
                LastPow = p;
                if( increment )
                    p.UsesCount++;
                if( p.TotalUses <= 0 ||
                    p.UsesCount < p.TotalUses )                                                            // Max uses reached
                    val += p.Power;
            }
        }
        return val;
    }

    internal static void UpdateBump()
    {
        Item.AddItem( ItemType.Fire_Level, +1 );

        Unit bld = Map.I.GetUnit( ETileType.BUILDING, G.Hero.GetFront() );
        if( bld && bld.Building.Type == BuildingType.Tent )
            FPow.SortPowers();
        UpdateText = true;
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
        string nm = Type.ToString();
        nm = nm.Replace( '_', ' ' );

        float rem = TotalUses - UsesCount;
        if( Type == FPowType.Hurry_Production || Type == FPowType.Hurry_Plants )
            nm += " " + Util.ToSTime( rem );
        else
        {
            if( Power != 0 )
                nm += " " + Power.ToString( "+#;-#;0" );
            if( Type == FPowType.BP_Refund_Resource_Cost || 
                Type == FPowType.Refund_Seed || 
                Type == FPowType.Refund_Tool ) nm += " %";
            if( TotalUses > 0 ) nm += " x" + ( rem );
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

        for( int i = 0; i < G.Farm.FirePow.Count; i++ )
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
                    G.Farm.FirePow.Add( p );
                    p.ResortCount = reslist[ i ];
                    p.UsesCount = useslist[ i ];
                    break;
                }
            }
        }
        UpdateText = true;
    }
}
