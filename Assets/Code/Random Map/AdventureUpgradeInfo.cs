using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EAdventureUpgradeType
{
    NONE = -1, UNLOCK_ADVENTURE, UPGRADE_SHOP, UPGRADE_STUDIES, UPGRADE_PACKMULE,
    REDUCE_REQUIRED_ITEM_AMOUNT, INCREASE_REQUIRED_ITEM_TIME, INCREASE_AVAILABLE_CUBES,
    UPGRADE_AUTOOPENGATE_BUTTON, RECEIVE_GIFT, CHEST_BONUS_CHANCE, CHEST_BONUS_AMOUNT,
    CHEST_PERSIST_CHANCE, CHEST_ITEM_CHANCE_INFLATION, CHEST_LEVEL, DECREASE_ENERGY_COST,
    HEALING_HP, INITIAL_HP, LEISURE, UPGRADE_PACKMULE_STACK, PACKMULE_ITEM_CAPACITY,
    INITIAL_ITEM_BONUS, ITEM_BONUS, GOTO_CHECKPOINT, GATE_PRICE, CLOVER_CHANCE,
    INITIAL_UPGRADE_CHEST_CHANCE, CLOVER_UPGRADE_CHEST_CHANCE, CUBE_CLEAR_UPGRADE_CHEST_CHANCE,
    SPAWN_BUTCHER_CHANCE, CHEST_BASE_BONUS_CHANCE, LOAD_COST_DISCOUNT, TRADE,
    UPGRADE_MAX_CAPACITY, INCREASE_CUBE_CLEAR_DEFAULT_BONUS, ITEM_PRODUCTION_LIMIT,
    ITEM_PRODUCTION_ACTIVATED, ITEM_PRODUCTION_TOTAL_TIME
}

    //---------------------------------------------------------------- these have not yet been added to help text
    //NONE = -1, , UPGRADE_SHOP, , UPGRADE_PACKMULE,
    //REDUCE_REQUIRED_ITEM_AMOUNT, INCREASE_REQUIRED_ITEM_TIME, ,
    //, , CHEST_BONUS_CHANCE, CHEST_BONUS_AMOUNT,
    //, CHEST_ITEM_CHANCE_INFLATION, CHEST_LEVEL, DECREASE_ENERGY_COST,
    //HEALING_HP, INITIAL_HP, LEISURE, UPGRADE_PACKMULE_STACK, PACKMULE_ITEM_CAPACITY,
    //INITIAL_ITEM_BONUS, ITEM_BONUS, , GATE_PRICE, 

public enum ELeisureType
{
    NONE = -1, Workout, Swimming, Poetry, Singing, Meditation, Sleep
}
public enum ETechScope
{
    NONE = -1, This_Quest, All_Quests
}

public class AdventureUpgradeInfo : MonoBehaviour
{
    [Header( "Upgrade Type:" )]
    public EAdventureUpgradeType UpgradeType = EAdventureUpgradeType.UNLOCK_ADVENTURE;
    [Header( "Cost:" )]
    public ItemType UpgradeItem1Type = ItemType.NONE;
    public float UpgradeItem1Cost = 1;
    public List< int> UpgradeItem1RecuringCost = null;

    public bool TotalCollected = false;
    [Header( "Item Affected:" )]
    public ItemType ItemAffected = ItemType.NONE;
    public float UpgradeEffectAmount = 1;
    public List<float> UpgradeRecuringEffect = null;
    [Header( "Study XP:" )]
    public float StudyXPBonus = 1;
    [Header( "Requirements:" )]
    public int UpNeed = 0;
    public int DownNeed = 0;
    public int LeftNeed = 0;
    public int RightNeed = 0;
    public int TechNeed = 0;

    [Header( "Show Requirements:" )]
    public int StudiesLevelToShow = 0;
    public bool OnlyShowIfNeighborsArrowsPermit = false;

    [Header( "Leisure:" )]
    public ELeisureType LeisureType = ELeisureType.NONE;
    [Header( "Scope:" )]
    public ETechScope TechScope = ETechScope.This_Quest;
    [Header( "Info:" )]
    public int X;
    public int Y, TechID, QuestID;
    public static List<ItemType> ChestBonusItem, InitialPrizeBonusList;

    public static int GetTechLevel( int x = -1, int y = -1, int tech = -1 )
    {
        RandomMap rm = null;
        if( Application.isPlaying == false )
            rm = GameObject.Find( "----------------Random Map----------------" ).GetComponent<RandomMap>();
        else rm = Map.I.RM;

        if( x == -1 )                                                                                              // Main Adventure Level
        {
            int purchased = ( int ) Item.GetNum( Inventory.IType.Inventory,
            ItemType.AdventureLevelPurchase, rm.CurrentAdventure );
            return purchased;
        }
        else                                                                                                       // Tech Level              
        {
            if( x < 0 || x >= TechButton.SX ||
                y < 0 || y >= TechButton.SY ) return -1;

            if( tech == -1 ) tech = rm.CurrentAdventure;
            ItemType it = ItemType.TechPurchase_0_0 + x + ( y * TechButton.SX );
            int purchased = ( int ) Item.GetNum( Inventory.IType.Inventory, it, tech );
            return purchased;
        }
    }

    public static void AddTechLevel( int x, int y, int val, AdventureUpgradeInfo au = null )
    {       
        if( x == -1 )                                                                                              // Main Adventure Level
        {       
            Item.AddItem( Inventory.IType.Inventory, ItemType.AdventureLevelPurchase,
            val, true, Map.I.RM.CurrentAdventure );
        }
        else                                                                                                       // Tech Level              
        {
            float max = 0;
            if( TechButton.Button[ x, y ] != null )
            if( TechButton.Button[ x, y ].UpgradeList != null )
            {

                max = TechButton.Button[ x, y ].UpgradeList.Length;
                if( au && au.IsRecurring() ) 
                    max = au.UpgradeItem1RecuringCost.Count;
            }    

            ItemType it = ItemType.TechPurchase_0_0 + x + ( y * TechButton.SX );
            Item.AddItem( Inventory.IType.Inventory, it,
            val, true, Map.I.RM.CurrentAdventure );

            if( Item.GetNum( it ) > max ) Item.SetAmt( it, max, 
                Inventory.IType.Inventory, true, Map.I.RM.CurrentAdventure );
            if( Item.GetNum( it ) < 0 ) Item.SetAmt( it, 0, 
                Inventory.IType.Inventory, true, Map.I.RM.CurrentAdventure );
        }
    }

    public static float GetStat( EAdventureUpgradeType type, ItemType itt = ItemType.NONE, bool countNone = true )
    {
        float val = 0;
        int level = GetTechLevel();
        if( type == EAdventureUpgradeType.UPGRADE_PACKMULE_STACK ) val = 1;
        if( type == EAdventureUpgradeType.UPGRADE_SHOP ) val = Map.I.RM.RMD.StartingTrainningLevel;
        if( type == EAdventureUpgradeType.GOTO_CHECKPOINT ) val = Map.I.RM.RMD.StartingGotoCheckPointLevel;
        if( type == EAdventureUpgradeType.INITIAL_UPGRADE_CHEST_CHANCE ) val = Map.I.RM.RMD.InitialUpgradeChestChance;
        if( type == EAdventureUpgradeType.CLOVER_UPGRADE_CHEST_CHANCE ) val = Map.I.RM.RMD.CloverPickUpgradeChestChance;
        if( type == EAdventureUpgradeType.CUBE_CLEAR_UPGRADE_CHEST_CHANCE ) val = Map.I.RM.RMD.CubeClearUpgradeChestChance;
        if( type == EAdventureUpgradeType.CHEST_PERSIST_CHANCE ) 
        { val = Map.I.RM.RMD.BaseChestPersistChance + ( Map.I.SessionChestsOpenCount * 2 ) ; }

        int init = 1 - Map.I.RM.RMD.StartingAdventureLevel;
        if ( Map.I.RM.RMD.AdventureUpgradeInfoList.Length > 0 )                                             // Sums the value of Adventure level
        for( int i = init; i < Map.I.RM.RMD.AdventureUpgradeInfoList.Length; i++ )
        {
            AdventureUpgradeInfo up = Map.I.RM.RMD.AdventureUpgradeInfoList[ i ];
            if( i < level )
            if( up.UpgradeType == type )
            if( ( countNone && itt == ItemType.NONE ) || 
                up.ItemAffected == ItemType.ALL ||
                up.ItemAffected == itt )
            {
                val += up.UpgradeEffectAmount;
            }
        }

        if( TechButton.UpdateMatrix || TechButton.Button == null )
            TechButton.CreateMatrix();

        for( int y = 0; y < TechButton.SY; y++ )                                                            // Sums the value of Tech Tree
        for( int x = 0; x < TechButton.SX; x++ )
        if ( TechButton.Button != null )
        if ( TechButton.Button[ x, y ] != null )
        if ( TechButton.Button[ x, y ].UpgradeList != null )
            {
                level = GetTechLevel( x, y );
                TechButton tb = TechButton.Button[ x, y ];
                for( int i = 0; i < tb.UpgradeList.Length; i++ )
                {
                    if( i < level )
                    if( tb.UpgradeList[ i ].UpgradeType == type )
                    if( tb.UpgradeList[ i ].TechScope == ETechScope.This_Quest )
                        if( ( countNone && itt == ItemType.NONE ) ||
                        tb.UpgradeList[ i ].ItemAffected == ItemType.ALL || 
                        tb.UpgradeList[ i ].ItemAffected == itt )
                    {
                        val += tb.UpgradeList[ i ].GetEffectAmount( x, y, level );
                        UpdateInfo( tb.UpgradeList[ i ], type );
                    }
                }
            }

        for( int i = 0; i < Map.I.GlobalTechList.Count; i++ )                                                 // Global techs
        {
            AdventureUpgradeInfo ad = Map.I.GlobalTechList[ i ];
            level = GetTechLevel( ad.X, ad.Y, ad.QuestID );
            if( ad.TechID < level )
            if( ad.UpgradeType == type )
            if( ( countNone && itt == ItemType.NONE ) || 
                ad.ItemAffected == ItemType.ALL ||
                ad.ItemAffected == itt )
            {
                val += ad.UpgradeEffectAmount;
                UpdateInfo( ad, type );
            }
        }

        if( type == EAdventureUpgradeType.CHEST_PERSIST_CHANCE )                                             // define  limits for these
            if( val > 40 ) val = 40;

        return val;
    }
    private float GetEffectAmount( int techX, int techY, int level )
    {
        if( IsRecurring() )
        {
            float val = 0;
            for( int i = 0; i < UpgradeItem1RecuringCost.Count; i++ )                                  // UpgradeItem1RecuringCost is the master list that defines the amount of possible upgrades 
            {
                if( i < level )
                {
                    if( i < UpgradeRecuringEffect.Count )
                        val += UpgradeRecuringEffect[ i ];
                    else
                        val += UpgradeRecuringEffect[ UpgradeRecuringEffect.Count - 1 ];               // if  list is partial, use last value
                }
                else break;
            }
            return val;
        }
        return UpgradeEffectAmount;
    }

    public static void UpdateInfo(  AdventureUpgradeInfo upg, EAdventureUpgradeType type )
    {
        if( type == EAdventureUpgradeType.CHEST_BONUS_AMOUNT ||                                         // add item to the chest list
            type == EAdventureUpgradeType.CHEST_BONUS_CHANCE )
            if( ChestBonusItem != null && ChestBonusItem.Contains( upg.ItemAffected ) == false )
                ChestBonusItem.Add( upg.ItemAffected );
        if( type == EAdventureUpgradeType.INITIAL_ITEM_BONUS )
        {
            if( InitialPrizeBonusList != null && InitialPrizeBonusList.Contains( 
                upg.ItemAffected ) == false && upg.ItemAffected != ItemType.NONE )
                InitialPrizeBonusList.Add( upg.ItemAffected );
        }
    }

    public static string GetUpgradeMessage( AdventureUpgradeInfo au, bool title = false, bool extra = false )
    {
        string post = " ";                                                                                     // if you want the Current value to be displayed, use post = " " or post = "" if not
        string msg = "";

        if( title ) msg = "Upgrade Effect: ";
        if( Application.platform == RuntimePlatform.WindowsEditor )
        if( au.TechScope == ETechScope.All_Quests ) msg += "(G) ";

        float amt = au.UpgradeEffectAmount;
        if( au.IsRecurring() )
        {
            ItemType it = ItemType.TechPurchase_0_0 + au.X + ( au.Y * TechButton.SX );
            int purchased = ( int ) Item.GetNum( Inventory.IType.Inventory, it, Map.I.RM.CurrentAdventure );

            if( purchased >= au.UpgradeRecuringEffect.Count )
                purchased = au.UpgradeRecuringEffect.Count - 1;
            amt = au.UpgradeRecuringEffect[ purchased ];
        }

        switch( au.UpgradeType )
        {
            case EAdventureUpgradeType.UNLOCK_ADVENTURE:
            msg += "Quest Unlocked.";
            post = "lock";
            break;
            case EAdventureUpgradeType.UPGRADE_SHOP:
            msg += "Training Unlocked.";
            post = "lock";
            break;
            case EAdventureUpgradeType.UPGRADE_AUTOOPENGATE_BUTTON:
            msg += "Auto Open Gate Button Unlocked.";
            post = "lock";
            break;
            case EAdventureUpgradeType.GOTO_CHECKPOINT:
            msg += "Find Puzzle Button Unlocked. (\\Key)";
            post = "lock";
            break;
            case EAdventureUpgradeType.GATE_PRICE:
            string rs = "";
            if( au.ItemAffected == ItemType.NONE )
            {
                msg += au.UpgradeType + " ERROR: bad Item Affected!";
                break;
            }
            if( au.ItemAffected != ItemType.ALL ) rs = Item.GetName( au.ItemAffected );
            msg += rs + " Gate Price: " + amt.ToString( "+0;-#" );
            post = " ";
            break;
            case EAdventureUpgradeType.DECREASE_ENERGY_COST:
            msg += "Energy Cost for Automove Reduced by: " + amt.ToString( "+0;-#" ) + "%";
            post = " ";
            break;
            case EAdventureUpgradeType.UPGRADE_PACKMULE:
            if( AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.UPGRADE_PACKMULE ) >= 1 )
                msg += "Packmule Slot: +" + amt.ToString( "0.#" );
            else msg += "Packmule Unlocked.";
            post = " ";
            break;
            case EAdventureUpgradeType.HEALING_HP:
            if( au.ItemAffected == ItemType.NONE )
            {
                msg += au.UpgradeType + " ERROR: bad Item Affected!";
                break;
            }
            msg +=  G.GIT( au.ItemAffected ).GetName() + 
            "Healing HP +" + amt.ToString( "0.#" );
            post = " ";
            break;
            case EAdventureUpgradeType.REDUCE_REQUIRED_ITEM_AMOUNT:
            msg += "Required Item Amount: -" + amt.ToString( "0.#" );
            post = " ";
            break;
            case EAdventureUpgradeType.INCREASE_REQUIRED_ITEM_TIME:
            msg += "Required Item Lifetime: -" + Util.ToSTime( amt );
            post = " ";
            break;
            
            case EAdventureUpgradeType.ITEM_PRODUCTION_LIMIT:
            msg += Item.GetName( au.ItemAffected ) + " Production Limit: " + amt.ToString( "+0;-#" );
            post = " ";
            break;

            case EAdventureUpgradeType.ITEM_PRODUCTION_ACTIVATED:
            msg += Item.GetName( au.ItemAffected ) + " Production Activated: " + Util.IntToBool( ( int ) amt ).ToString();
            post = " ";
            break;

            case EAdventureUpgradeType.ITEM_PRODUCTION_TOTAL_TIME:
            msg += Item.GetName( au.ItemAffected ) + " Production Time: " + Util.ToSTime( amt );
            post = " ";
            break;

            case EAdventureUpgradeType.UPGRADE_PACKMULE_STACK:
            msg += "Packmule Max Stack: +" + amt.ToString( "0.#" );
            break;
            case EAdventureUpgradeType.INCREASE_AVAILABLE_CUBES:
            msg += "Available Cubes: +" + amt.ToString( "0.#" );
            post = " ";
            break;
            case EAdventureUpgradeType.UPGRADE_STUDIES:
            if( TechButton.StudiesLevel >= 1 )
                msg += "Add More Studies";
            else
                msg += "Unlock Studies";
            post = "level";
            break;
            case EAdventureUpgradeType.PACKMULE_ITEM_CAPACITY:
            if( au.ItemAffected == ItemType.NONE )
            {
                msg += au.UpgradeType + " ERROR: bad Item Affected!";
                break;
            }
            msg += G.GIT( au.ItemAffected ).GetName() + 
            "Equip Cap: +" + amt.ToString( "0.#" );
            break;

            case EAdventureUpgradeType.RECEIVE_GIFT:
            msg += "Receive Gift: " + Item.GetName( au.UpgradeItem1Type ) + " " + au.UpgradeItem1Cost.ToString( "+0;-#" );
            break;               
       
            case EAdventureUpgradeType.TRADE:
            msg += "Buy: " + " " + ( amt ).ToString( "+0;-#" ) + " " + Item.GetName( au.ItemAffected );
            break;

            case EAdventureUpgradeType.UPGRADE_MAX_CAPACITY:
            msg += "Increase Max Cap: " + Item.GetName( au.ItemAffected ) + " " + ( amt ).ToString( "+0;-#" );
            break; 

            case EAdventureUpgradeType.CHEST_BONUS_CHANCE:
            if( au.ItemAffected == ItemType.NONE )
            {
                msg += au.UpgradeType + " ERROR: bad Item Affected!";
                break;
            }
            msg += "Extra Chest Bonus Chance: " + Item.GetName( au.ItemAffected ) + " " + amt.ToString( "+0;-#" ) + "%";
            post = "%";
            break;
            case EAdventureUpgradeType.CHEST_BASE_BONUS_CHANCE:

            msg += "Base Chest Bonus Chance: " + amt.ToString( "+0;-#" ) + "%";
            post = "%";
            break;

            case EAdventureUpgradeType.CHEST_BONUS_AMOUNT:
            if( au.ItemAffected == ItemType.NONE )
            {
                msg += au.UpgradeType + " ERROR: bad Item Affected!";
                break;
            }
            msg += "Chest Bonus Amount: " + Item.GetName( au.ItemAffected ) + " " + amt.ToString( "+0;-#" );
            post = " ";
            break;

            case EAdventureUpgradeType.CHEST_PERSIST_CHANCE:
            msg += "Chest Persist Chance: " + amt.ToString( "+0;-#" ) + "%";
            post = "%";
            break;
            case EAdventureUpgradeType.INITIAL_UPGRADE_CHEST_CHANCE:
            msg += "Chest Upgrade Chance: " + amt.ToString( "+0;-#" ) + "%";
            post = "%";
            break;
            case EAdventureUpgradeType.CLOVER_UPGRADE_CHEST_CHANCE:
            msg += "Clover Pick Chest Upgrade Chance: " + amt.ToString( "+0;-#" ) + "%";
            post = "%";
            break;
            case EAdventureUpgradeType.CUBE_CLEAR_UPGRADE_CHEST_CHANCE:
            msg += "Cube Clear Chest Upgrade Chance: " + amt.ToString( "+0;-#" ) + "%";
            post = "%";
            break;
            case EAdventureUpgradeType.SPAWN_BUTCHER_CHANCE:
            msg += "Spawn Butcher Chance: " + amt.ToString( "+0;-#" ) + "%";
            post = "%";
            break;
            case EAdventureUpgradeType.LOAD_COST_DISCOUNT:
            msg += "Load Cost Discount: " + amt.ToString( "+0;-#" ) + "%";
            post = "%";
            break;
            
            case EAdventureUpgradeType.INCREASE_CUBE_CLEAR_DEFAULT_BONUS:
            msg += "Default Cube clear Bonuses: " + amt.ToString( "+0;-#" ) + "%";
            post = "%";
            break;  

            case EAdventureUpgradeType.CHEST_ITEM_CHANCE_INFLATION:
            msg += " Chest Extra Prize Chance Per Unit: " + amt.ToString( "+0;-#" ) + "%";
            post = "%\nEvery subsequential Chest Opened\nincreases finding chances for all items.";
            break;

            case EAdventureUpgradeType.CHEST_LEVEL:
            msg += " Chest Level: Lv " + amt.ToString( "0." );
            break;

            case EAdventureUpgradeType.INITIAL_HP:
            msg += "Hero Initial HP: " + amt.ToString( "+0;-#" );
            post = " ";
            break;

            case EAdventureUpgradeType.CLOVER_CHANCE:
            msg += "Find Clover Chance: " + amt.ToString( "+0;-#" ) + "%";
            post = " ";
            break;

            case EAdventureUpgradeType.INITIAL_ITEM_BONUS:
            if( au.ItemAffected == ItemType.NONE )
            {
                msg += au.UpgradeType + " ERROR: bad Item Affected!";
                break;
            }
            msg += "" + Item.GetName( au.ItemAffected ) + "Starting Bonus: " + amt.ToString( "+0;-#" );
            post = " ";
            break;

            case EAdventureUpgradeType.ITEM_BONUS:
            if( au.ItemAffected == ItemType.NONE )
            {
                msg += au.UpgradeType + " ERROR: bad Item Affected!";
                break;
            }
            msg += "" + Item.GetName( au.ItemAffected ) + "Prize Bonus: " + amt.ToString( "+0;-#" ) + "%";
            post = "%";
            break;

            case EAdventureUpgradeType.LEISURE:
            if( au.LeisureType == ELeisureType.NONE )
            {
                int sz = System.Enum.GetValues( typeof( ELeisureType ) ).Length;
                au.LeisureType = ( ELeisureType ) Random.Range( 0, sz - 1 );
            }
            msg += au.LeisureType.ToString() + "... (no Effect)";

            break;
        }

        if( extra && post != "" )                                                                                   // extra information
        {
            float val = AdventureUpgradeInfo.GetStat( au.UpgradeType, au.ItemAffected );
            if( post == "lock" )
            {
                if( val <= 0 )
                    msg += "\nStatus: Locked";
                else 
                    msg += "\nStatus: UNLOCKED!";
            }
            if( post == "level" )
            {
                if( val < 1 )
                    msg += "\nLocked";
                else
                    msg += "\nCurrent Level: " + val;
            }
            else
                msg += "\nCurrent Amount: " + val + post;
        }
        return msg;
    }

    public string GetUpgradeText()
    {
        string nm = "";
        nm = GetUpgradeMessage( this );
        nm = nm.Replace( "Upgrade Effect: ", "" );
        nm = nm.Replace( ".", "" );
        return nm;
    }

    public void UpdateGameObjectText()
    {
        string add = "";
        add = " (" + UpgradeItem1Type + " " + UpgradeItem1Cost + ")";
        if( IsRecurring() ) add = " (Recurring)";
        name = GetUpgradeText() + add;
    }

    internal bool IsRecurring()
    {
        if( Util.HasDada( UpgradeItem1RecuringCost ) )
            return true;
        return false;
    }
}