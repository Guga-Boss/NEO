using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;

public class Recipe : MonoBehaviour
{
    #region Variables
    public int ID = -1;
    [ReadOnly()]
    public string UniqueID = "";           // Unique ID for Saving
    [Space( 10 )]
    public int Level = 0;
    [Space( 10 )]
    public int RecipesAvailable = -1;
    [Space( 10 )]
    public float TotalTime = 3600;
    public float TimeCount = 0;
    [Space( 10 )]

    public ItemType Required_Item_1 = ItemType.NONE;
    public int Required_Item_1_Amount = 1;
    public ItemType Required_Item_2 = ItemType.NONE;
    public int Required_Item_2_Amount = 1;
    public ItemType Required_Item_3 = ItemType.NONE;
    public int Required_Item_3_Amount = 1;
    public ItemType Required_Item_4 = ItemType.NONE;
    public int Required_Item_4_Amount = 1;
    [Space( 20 )]
    public ItemType Generated_Item_1 = ItemType.NONE;
    public int Generated_Item_1_Amount = 1;
    public ItemType Generated_Item_2 = ItemType.NONE;
    public int Generated_Item_2_Amount = 1;
    public ItemType Generated_Item_3 = ItemType.NONE;
    public int Generated_Item_3_Amount = 1;
    public ItemType Generated_Item_4 = ItemType.NONE;
    public int Generated_Item_4_Amount = 1;
    public static Unit SelectedBuilding = null;
    public RecipeUpgradeInfo[ ] RecipeUpgradeInfoList = null;
    public int RecipeNumericalOrder = -1;
    public bool Activated = false;
    public int StoredCicles = 0;
    public int QueueLenght = 1;
    public static int TotRecipesEnabled = -1;
    public static float ActivateKeypressTimeCount = 0;
#endregion

    public void Copy( Recipe r )
    {
        Level = r.Level;
        RecipesAvailable = r.RecipesAvailable;
        TotalTime = r.TotalTime;
        TimeCount = r.TimeCount;
        RecipeNumericalOrder = r.RecipeNumericalOrder;
        Activated = r.Activated;
        StoredCicles = r.StoredCicles;
        QueueLenght = r.QueueLenght;
    }

    public void UpdateAll( Building bl, double addTime, int id )
    {
        UpdateProduction( addTime, bl, id );                                                                       // Update Production
    }

    public static void UpdateChosen( Building bl, double addTime )
    {
        UpdateInput( bl );                                                                                         // Update Input        
        if( SelectedBuilding == null ) return;                                                                     // No building Selected
        if( SelectedBuilding.Building.RecipeList == null ) return;
        if( SelectedBuilding.Building.RecipeList.Count < 1 ) return;
        if( bl.SelectedRecipeID == -1 ) return;
        if( bl.SelectedRecipeID > SelectedBuilding.Building.RecipeList.Count - 1 ) return;
        Map.I.Farm.RecipePanel.gameObject.SetActive( true );
        Recipe sr = SelectedBuilding.Building.RecipeList[ bl.SelectedRecipeID ];
        Map.I.Farm.RecipePanel.UpdateIt( sr, bl.SelectedRecipeID, bl );   
    }

    public static void UpdateInput( Building bl )
    {
        Vector2 tg = G.Hero.GetFront();
        Unit fbl = Map.I.GetUnit( ETileType.BUILDING, tg );
        if( fbl == null ) tg = new Vector2( Map.I.Mtx, Map.I.Mty );

        if( tg == bl.Unit.Pos )                                                                                    // Any building Frontally selected?
        {
            SelectedBuilding = bl.Unit;
            if( Map.I.Farm.RecipePanel.gameObject.activeSelf == false )
                Map.I.Farm.RecipePanel.gameObject.SetActive( true );
        }
        else
        {
            fbl = Map.I.GetUnit( ETileType.BUILDING, tg );
            if( fbl == null || TotRecipesEnabled < 1 )
            {
                SelectedBuilding = null;
                Map.I.Farm.RecipePanel.gameObject.SetActive( false );
                return;
            }
            return;
        }

        UpdateRecipesAvailable( bl );

       int res = GetNextRecipeThatIsAvailable( bl );
       if( res == -1 ) return;

       float time = .4f;
       if( ActivateKeypressTimeCount < time )
       if( cInput.GetKeyUp( "Battle" ) == true )                                                                     // Chooses next recipe
           {
               GetNextRecipe( +1, bl );
               ActivateKeypressTimeCount = 0;
           }

       if( cInput.GetKey( "Battle" ) == true )                                                                       // KeyHold recive activation
           ActivateKeypressTimeCount += Time.unscaledDeltaTime;
       else
           ActivateKeypressTimeCount = 0;

       if( cInput.GetKey( "Battle" ) == true )
       if( ActivateKeypressTimeCount > time )                                                                        // switches activation state
       if( ActivateKeypressTimeCount < 2 ) 
       {
           ActivateKeypressTimeCount = 2;
           Recipe sr = SelectedBuilding.Building.RecipeList[ bl.SelectedRecipeID ];
           sr.Activated = !sr.Activated;
       }
    }

    public static void UpdateRecipesAvailable( Building bl )
    {
        TotRecipesEnabled = 0;
        int count = 0;
        for( int i = 0; i < bl.RecipeList.Count; i++ )
        {
            if( bl.GetRecipesAvailable( i ) != -1 )
            {
                count++;
                TotRecipesEnabled++;
            }
            bl.RecipeList[ i ].RecipeNumericalOrder = count;
        }        
    }

    public static int GetNextRecipeThatIsAvailable( Building bl )   
    {
        if( bl.SelectedRecipeID == -1 )                                            // This auto selects a recipe when the first recipe is enabled in a list
        {
            for( int i = 0; i < bl.RecipeList.Count; i++ )
            {
                if( bl.GetRecipesAvailable( i ) != -1 )
                {
                    bl.SelectedRecipeID = i;
                    return i;
                }
            }
            return -1;
        }
        return bl.SelectedRecipeID;
    }
    public static void GetNextRecipe( int increment, Building bl )
    {
        if( bl.RecipeList.Count <= 1 )
        {
            bl.SelectedRecipeID = 0;
            return;
        }

        for( int i = 0; i < 100; i++ )
        {
            bl.SelectedRecipeID += increment;

            if( bl.SelectedRecipeID >= bl.RecipeList.Count )
                bl.SelectedRecipeID = 0;
            else
            if( bl.SelectedRecipeID < 0 )
                bl.SelectedRecipeID = bl.RecipeList.Count - 1;

            if( bl.GetRecipesAvailable( bl.SelectedRecipeID ) != -1 ) break;
        }
        if( bl.SelectedRecipeID == -1 ) bl.SelectedRecipeID = 0;
    }

    public void UpdateProduction( double addTime, Building bl, int id )
    {
        if( Activated == false ) return;
        int rec = bl.GetRecipesAvailable( id );                                                                     // Not enough recipes
        if( rec < 1 ) return;

        int cicles = UpdateProductionData( id, bl );                                                                // Get max number of prod cicles
        if( cicles < 1 ) return;
        float tot = GV( ERecipeUpgradeType.TIME );

        if( addTime > 0 )                                                                                           // idle time addition
        {
            TimeCount += ( float ) addTime;
            int tcicles = ( int ) ( TimeCount / tot );
            if( tcicles < cicles ) cicles = tcicles;
            if( cicles >= QueueLenght )                                                                              // Max production units for offline production 
            {
                cicles = QueueLenght;
            }
            StoredCicles = cicles;                                                                                  // Just store number of cicles in the if theres addtime and only produce goods later to avoid concatenated production bug
            return;
        }

        TimeCount += Time.unscaledDeltaTime;                                                                        // Add time

        float lim = cicles * tot;                                                                                   // do not accumulate time over cicle limit
        if( TimeCount > lim ) TimeCount = lim;

        int stored = StoredCicles;
        for( int i = 0; i < stored; i++ )
        {
            ProduceGood( bl, tot, rec, id );                                                                        // Produce goods for stored cicles
            StoredCicles--;
        }

        if( TimeCount >= tot )                                                                                      // Times up! ------------
        {
            ProduceGood( bl, tot, rec, id );
        }
    }

    public void ProduceGood( Building bl, float tot, int rec, int id )
    {
        TimeCount -= tot;
        Building.PlaceItem( bl.Unit.Pos, -GV( ERecipeUpgradeType.Req_1_Amt ), Required_Item_1,  true );              // Charge required items
        Building.PlaceItem( bl.Unit.Pos, -GV( ERecipeUpgradeType.Req_2_Amt ), Required_Item_2,  true );
        Building.PlaceItem( bl.Unit.Pos, -GV( ERecipeUpgradeType.Req_3_Amt ), Required_Item_3,  true );
        Building.PlaceItem( bl.Unit.Pos, -GV( ERecipeUpgradeType.Req_4_Amt ), Required_Item_4,  true );
        Building.PlaceItem( bl.Unit.Pos, +GV( ERecipeUpgradeType.Gen_1_Amt ), Generated_Item_1, true );              // Give Generated items
        Building.PlaceItem( bl.Unit.Pos, +GV( ERecipeUpgradeType.Gen_2_Amt ), Generated_Item_2, true );
        Building.PlaceItem( bl.Unit.Pos, +GV( ERecipeUpgradeType.Gen_3_Amt ), Generated_Item_3, true );
        Building.PlaceItem( bl.Unit.Pos, +GV( ERecipeUpgradeType.Gen_4_Amt ), Generated_Item_4, true );
        bl.SetRecipesAvailable( id, rec - 1 );
        QueueLenght--;
        if( QueueLenght <= 0 )                                                                                       // Queue finished. Stop production
        {
            Activated = false;
            TimeCount = 0;
        }
        MasterAudio.PlaySound3DAtVector3( "Cashier", transform.position );                                           // FX
    }

    public int UpdateProductionData( int id, Building bl )
    {
        int cicles = 100;
        GetTotalCiclesReq( ref cicles, Required_Item_1,  GV( ERecipeUpgradeType.Req_1_Amt ), bl );
        GetTotalCiclesReq( ref cicles, Required_Item_2,  GV( ERecipeUpgradeType.Req_2_Amt ), bl );
        GetTotalCiclesReq( ref cicles, Required_Item_3,  GV( ERecipeUpgradeType.Req_3_Amt ), bl );
        GetTotalCiclesReq( ref cicles, Required_Item_4,  GV( ERecipeUpgradeType.Req_4_Amt ), bl );
        GetTotalCiclesGen( ref cicles, Generated_Item_1, GV( ERecipeUpgradeType.Gen_1_Amt ), bl );
        GetTotalCiclesGen( ref cicles, Generated_Item_2, GV( ERecipeUpgradeType.Gen_2_Amt ), bl );
        GetTotalCiclesGen( ref cicles, Generated_Item_3, GV( ERecipeUpgradeType.Gen_3_Amt ), bl );
        GetTotalCiclesGen( ref cicles, Generated_Item_4, GV( ERecipeUpgradeType.Gen_4_Amt ), bl );

        return cicles;
    }
    public int GV( ERecipeUpgradeType type )
    {
        float sum = 0;
        for( int i = 0; i < RecipeUpgradeInfoList.Length; i++ )
        {
            if( i >= Level ) break;
            if( RecipeUpgradeInfoList[ i ].UpgradeType == type )
            {
                sum += RecipeUpgradeInfoList[ i ].UpgradeEffectAmount;
            }
            if( RecipeUpgradeInfoList[ i ].UpgradeType2 == type )
            {
                sum += RecipeUpgradeInfoList[ i ].UpgradeEffectAmount2;
            }
        }

        switch( type )
        {
            case ERecipeUpgradeType.TIME:
            return ( int ) ( TotalTime + sum );
            case ERecipeUpgradeType.Req_1_Amt:
            return ( int ) ( Required_Item_1_Amount + sum );
            case ERecipeUpgradeType.Req_2_Amt:
            return ( int ) ( Required_Item_2_Amount + sum );
            case ERecipeUpgradeType.Req_3_Amt:
            return ( int ) ( Required_Item_3_Amount + sum );
            case ERecipeUpgradeType.Req_4_Amt:
            return ( int ) ( Required_Item_4_Amount + sum );
            case ERecipeUpgradeType.Gen_1_Amt:
            return ( int ) ( Generated_Item_1_Amount + sum );
            case ERecipeUpgradeType.Gen_2_Amt:
            return ( int ) ( Generated_Item_2_Amount + sum );
            case ERecipeUpgradeType.Gen_3_Amt:
            return ( int ) ( Generated_Item_3_Amount + sum );
            case ERecipeUpgradeType.Gen_4_Amt:
            return ( int ) ( Generated_Item_4_Amount + sum );
            case ERecipeUpgradeType.Queue:
            return ( int ) ( 1 + sum );
        }
        return -1;
    }
    public void GetTotalCiclesReq( ref int cicles, ItemType it, float req, Building bl )
    {
        if( it == ItemType.NONE ) return;
        int id = -1;
        for( int i = 0; i < bl.Itm.Count; i++ )
         if( bl.Itm[ i ].ItemType == it ) id = i;

        float avail = 0;   
        if( id == -1 )
        {
            avail = Item.GetNum( it );
        }
        else
          avail = bl.Itm[ id ].ItemCount;

        int count = 0;
        for( int i = 0; i < 50; i++ )
        {
            if( avail < req ) break;
            avail -= req;
            count++;
        }
        if( count < cicles ) cicles = count;
    }
    public void GetTotalCiclesGen( ref int cicles, ItemType it, float req, Building bl )
    {
        if( it == ItemType.NONE ) return; // nothing to generate

        int id = -1;
        for( int i = 0; i < bl.Itm.Count; i++ )
            if( bl.Itm[ i ].ItemType == it )
            {
                id = i;
                break;
            }

        if( id == -1 )
        {
            cicles = 0; // item not in inventory
            return;
        }

        float mstack = Building.GetStat( EVarType.Maximum_Item_Stack, bl, id ); // max stack
        if( mstack <= 0 ) mstack = float.MaxValue; // treat as infinite
        else mstack -= bl.Itm[ id ].ItemCount;      // remaining space

        if( mstack < 0 ) mstack = 0;               // safety

        int count = ( mstack == float.MaxValue ) ? int.MaxValue : ( int ) ( mstack / req ); // calculate cycles

        if( count < cicles ) cicles = count;       // limit cycles
    }


    public bool UpgradeRecipe( bool apply, Building bl )
    {
        RecipeUpgradeInfo up = RecipeUpgradeInfoList[ Level ];
        float avail = Building.GetItemAmount( up.UpgradeItemCostType );
        float rec = bl.GetRecipesAvailable( bl.SelectedRecipeID );

        if( up.UpgradeItemCostType == ItemType.Recipe_Image_1 )                                     // Cost item is available recipes (book icon)
        {
            if( rec < up.UpgradeItemCostAmount ) return false;
        }
        else
        if( avail < up.UpgradeItemCostAmount )
        {
            return false;                                                                           // not enough resources
        }

        if( apply )
        {
            Level++;                                                                                  // Level up!!!

            if( up.UpgradeItemCostType == ItemType.Recipe_Image_1 )                                   // Charges avail recipes                
            {
                int num = ( int ) ( rec - up.UpgradeItemCostAmount );
                bl.SetRecipesAvailable( bl.SelectedRecipeID, num );
            }
            else
            Building.AddItem( true, up.UpgradeItemCostType, - up.UpgradeItemCostAmount );             // Charges Resource

            Message.GreenMessage( "Recipe Upgraded!" );
            Manager.I.Tutorial.RecipeUpgraded = true;
            MasterAudio.PlaySound3DAtVector3( "Cashier", transform.position );                        // FX
        }
        return true;
    }
    public static int GetRecipesAvailable( Recipe recipe )
    {
        for( int b = 0; b < Map.I.Farm.BuildingList.Count; b++ )
        for( int r = 0; r < Map.I.Farm.BuildingList[ b ].RecipeList.Count; r++ )
        if ( Map.I.Farm.BuildingList[ b ].RecipeList[ r ] == recipe )
             return Map.I.Farm.BuildingList[ b ].RecipeList[ r ].RecipesAvailable;
        return -1;
    }
}
