using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ItemType
{
    RND_Butcher_Level = -100,
    ALL = -2, NONE = -1,
    Stone, Coal, Iron_Ore, Iron_Bar, Vine, Rope, Wood = 6, Tools, WoodAxe, Cog, Energy, Shell, Coconut, Quest_XP, HoneyComb, Honey,
    Feather, Clover, Butcher_Level, Butcher_Coin_Level, Butcher_Bonus_Quality_Level, Butcher_Slots_Level, Butcher_Power_Level, Blue_Coin,
    WheelBarrow = 27, Farm_Size_Token = 29, Bronze_Trophy, Silver_Trophy, Gold_Trophy, Diamond_Trophy, Adamantium_Trophy, Genius_Trophy,
    Quest_Trial_Count, Starting_Cube, Adventure_Completion, Study_XP,
    Res_Fish_Yellow = 40, Res_Fish_Red, Res_Fish_Blue, Res_Fish_Crab, Res_Fish_Manta_Ray, Res_Fish_Water_Snake, Res_Fish_Brown, Res_Fish_Frog,
    Res_Melee_Attacks = 48, Res_Bow_Arrow = 49,

    Res_Red_Herb = 50, Res_Green_Herb, Res_Blue_Herb, Res_Yellow_Herb, Res_Black_Herb, Res_White_Herb,
    Res_SnowStep = 58, Res_HP = 59,
    Sardine_Rarity_1 = 60, Trout_Rarity_2, Bass_Rarity_3, Cod_Rarity_6, StoneCat_Rarity_5, SwordFish_Rarity_6,
    Res_Stitches = 72, Res_Mining_Points = 73, Res_Monster_Kill = 74, Res_Oxygen = 75,
    Res_FireWood = 76, Res_Mushroom = 77, Res_Mask = 78, Res_BirdCorn = 79, Res_ForcedZoom = 80, Res_Mining_Bag = 81,
    Res_Green_Mine_Crystal = 82, Res_Cramps = 83, Res_Water_Flower = 84, Res_Hero_Shield = 85, Res_Spear = 86,
    Chest_Points, Res_Boomerang, Res_Rudder, Res_Raft_Hammer, Res_Quiver, Res_Butcher_Coin, Res_Butcher_Prize, Res_Bone, Res_Fire_Staff,
    Res_TRIGGER, Res_Throwing_Axe, Res_Bamboo, Res_Bridge_Wood, Res_Hook_CW, Res_Hook_CCW, Res_Mushroom_Potion, Res_Vine_Bale,
    Res_Fishing_Pole, Res_Harpoon, Res_Mining_Sieve, Res_Kick, Res_Exploding_Plant, Res_Torch, Res_Fire_Lits, Res_Double_Attack, Res_WebTrap, Res_Attack_Bonus,
    Berry_Seed = 208, Blackberry_Seed, Strawberry_Seed,

    Baked_Fish_Dish = 140,

    Berry_Fruit = 240, Blackberry_Fruit, Strawberry_Fruit,

    void1 = 112, void2, Plague_Monster_Cloner = 117,
    Plague_Monster_Spawner = 118, Plague_Monster_Slayer = 119, Club = 120, Plague_Monster_Kickable = 121, Plague_Monster_Swap = 122,
    Res_Plague_Monster, Plague_Monster_Blocker, Plague_Monster_Grabber,
    AdventureLevelPurchase = 129, Goals_Completed, Secrets_Found, Egg = 133, Chicken, Wild_Greens, Wood_Board, Meat, Fur, Barbecue, 
    BluePrint_Image_1 = 144, Planting_Token, Water_Token, Hoe, Shovel, Recipe_Image_1 = 149,
    TechPurchase_0_0 = 150, TechPurchase_1_0, TechPurchase_2_0, TechPurchase_3_0, TechPurchase_4_0, TechPurchase_5_0,
    TechPurchase_0_1, TechPurchase_1_1, TechPurchase_2_1, TechPurchase_3_1, TechPurchase_4_1, TechPurchase_5_1,
    TechPurchase_0_2, TechPurchase_1_2, TechPurchase_2_2, TechPurchase_3_2, TechPurchase_4_2, TechPurchase_5_2,
    TechPurchase_0_3, TechPurchase_1_3, TechPurchase_2_3, TechPurchase_3_3, TechPurchase_4_3, TechPurchase_5_3,
    Tl_Blueprint_Icon_1 = 177, Tl_Blueprint_Icon_2
}

public enum EItemCategory
{
    NONE = -1,
    Resource, Tool, Farm, Fruit, AdventureObject
}

public class Item : MonoBehaviour
{
    #region Variables
    [Space( 20 )]
    [TabGroup( "Main" )]
    public ItemType Type;
    [TabGroup( "Main" )]
    [ReadOnly()]
    public string UniqueID = "";          // Unique ID for Saving
    [Space( 20 )]
    [TabGroup( "Main" )]
    public int ID = -1;
    [TabGroup( "Main" )]
    public EItemCategory Category = EItemCategory.NONE;
    [TabGroup( "Main" )]
    public Inventory.IType InventoryType;
    [TabGroup( "Link" )]
    public UIButton IconButton;
    [TabGroup( "Link" )]
    public UILabel IconLabel;
    [TabGroup( "Link" )]
    public UI2DSprite Selection;
    [TabGroup( "Link" )]
    public UI2DSprite Sprite;
    [TabGroup( "Link" )]
    public tk2dSprite TKSprite;
    [TabGroup( "Warez" )]
    public List<Building> WarehouseList;
    [TabGroup( "Warez" )]
    public int WarehouseBuiltCount = 0;
    [Space( 20 )]
    [TabGroup( "Main" )]
    public float Count;
    [TabGroup( "Main" )]
    public float TempCount;                       // For temporary resources
    [TabGroup( "Main" )]
    public float FarmProduction = 0;             // Produced in the farm
    [TabGroup( "Main" )]
    public float GamePrizes = 0;                 // Items gained while playing 
    [TabGroup( "Main" )]
    public float StartingBonus = 0;
    [TabGroup( "Main" )]
    public bool IsGameplayResource = false;
    [TabGroup( "Main" )]
    public bool IsGlobalGameplayResource = false;
    [TabGroup( "Main" )]
    public float TotalGained = 0;   // Total earned 
    [TabGroup( "Main" )]
    public float MaximumCount = 0;    // Maximum number achieved
    [Space( 20 )]
    [HideInInspector]
    public float AreaEnterCount;
    [TabGroup( "Main" )]
    public bool SavePerAdventureCount = false;
    [TabGroup( "Main" )]
    public List<float> PerAdventureCount;
    [TabGroup( "Main" )]
    public List<float> PerAdventureTotalCount;
    [Space( 20 )]
    [TabGroup( "Main" )]
    public float MaxStack;
   [TabGroup( "Main" )]
    public float ExtraCapacity;
    [Space( 20 )]
    [TabGroup( "Main" )]
    public float DefaultNumber;
    [TabGroup( "Warez" )]
    public float PreWarehouseMaxStack = 0;    // Free space available before building the warehouse. 
    [TabGroup( "Carry" )]
    public float BaseCarryCapacity = 1;
    [TabGroup( "Carry" )]
    public float CarryCapacity = 0;
    [TabGroup( "Carry" )]
    public float BaseRoadCarryCapacity = 1;
    [TabGroup( "Carry" )]
    public float RoadCarryCapacity = 0;
    [TabGroup( "Production" )]
    public bool BaseProductionActivated = false;          // item has originaly production activated?
    [TabGroup( "Production" )]
    public int ProductionPurchased = -2;                  // production purchased overide:   -2 == default value,  0 == false,   1 == true
    [TabGroup( "Production" )]
    public float BaseProductionTotalTime = 0;             // original prod time
    [TabGroup( "Production" )]
    public float ExtraProductionTotalTime = 0;            // Purchased prod time
    [TabGroup( "Production" )]
    public float ProductionCount;                         // time counter
    [TabGroup( "Production" )]
    public bool SaveLifeTime = false;
    [TabGroup( "Production" )]
    public float BaseTotalLifeTime = 0;
    [TabGroup( "Production" )]
    public float TotalLifeTime;
    [TabGroup( "Production" )]
    public float LifeTimeCount;
    [TabGroup( "Production" )]
    public bool CountLifetime = false;
    [TabGroup( "Production" )]
    public int BaseProductionLimit = 0;                     // base prod limit
    [TabGroup( "Production" )]
    public int ExtraProductionLimit = 0;                    // purchased limit
    [TabGroup( "Option" )]
    public bool SaveData = true;
    [TabGroup( "Option" )]
    public bool CanBeEquiped = true;
    [TabGroup( "PackMule" )]
    public bool NeedPackmuleSpace = false;
    [TabGroup( "Option" )]
    public bool AlwaysShowInInventory = false;    // Show even if theres 0 
    [TabGroup( "Option" )]
    public bool NeverShowInInventory = false;     // Never show regardless of the number
    [TabGroup( "Option" )]
    public bool CountAsResourcePickInStats = true;
    [TabGroup( "Tool" )]
    public bool IsTool = false;
    [TabGroup( "Tool" )]
    public float ToolUsageWorkTime = 50;
    [TabGroup( "Tool" )]
    public float ToolUsageEnergyCost = 5;
    [TabGroup( "Option" )]
    public bool IsSeed = false;
    [TabGroup( "Option" )]
    public bool IsFruit = false;
    [TabGroup( "Option" )]
    public bool AutoRotateWithHero = true;
    [TabGroup( "PackMule" )]
    public List<float> BasePackmuleCapacityPerAdventure;
    [TabGroup( "Warez" )]
    public List<float> BuildingMaxStackList = null;
    [TabGroup( "Warez" )]
    public float PreLoadBonus = 0;
    [TabGroup( "Tool" )]
    public BuildingType ToolCreationBuilding = BuildingType.NONE;
    [TabGroup( "PackMule" )]
    public static int CurrentPackMuleSlot = -1;
    [TabGroup( "Other" )]
    public float BuildingMaxStackOverride = -1;                                 // this copies the max stack from the building so it can Overide item max stack
    [HideInInspector]
    public float AuxMaxStack, AuxCount;
    public static bool ResourceAdded = false;
    public static bool IgnoreMessage = false;
    public static bool TempResource;
    public static bool ForceMessage = false;
    public static bool IgnoreBuffer = false;
    public static string PostMessage;
    public int OldCount;


    #endregion

    #if UNITY_EDITOR
    [HorizontalGroup( "Split", 0.1f )]
    [Button( "GO", ButtonSizes.Large ), GUIColor( 0, 1, 1 )]
    public void ButtonGo()
    {
        Helper.I = GameObject.Find( "Helper" ).GetComponent<Helper>();
        UnityEditor.Selection.activeGameObject = Helper.I.gameObject;
    }
    #endif

    public void StartIt()
    {
        Count = DefaultNumber;
    }

    public static float GetNum( ItemType type, Inventory.IType itype = Inventory.IType.Inventory, int adv = -1 )
    {
        return GetNum( itype, type, adv );
    }
    public static float GetNum( Inventory.IType itype, ItemType type, int adv = -1 )
    {
        if( type == ItemType.NONE ) return -1;
        if( itype == Inventory.IType.Inventory )
        {
            if( type == ItemType.Res_Hook_CCW ) type = ItemType.Res_Hook_CW;                            // Both Hooks counts only as one resource
            Item itm = G.GIT( type );
            if( type == ItemType.Res_HP ) 
                return G.Hero.Body.Hp;                                                                  // Hero hp 

            if( itm.SavePerAdventureCount )
            {
                if( adv == -1 )
                if( Map.I.RM.CurrentAdventure < 0 ||
                    Map.I.RM.CurrentAdventure > itm.PerAdventureCount.Count - 1 )
                    {
                        //Debug.LogError("adv != Current Adventure!");
                        return 0;
                    }
                if( adv != -1 )
                    return itm.PerAdventureCount[ ( int ) adv ];
                return itm.PerAdventureCount[ ( int ) Map.I.RM.CurrentAdventure ];
            }
            else
            {
                return itm.Count + itm.TempCount;
            }
        }

        if( itype == Inventory.IType.Packmule )
        {
            float num = 0;
            //if( Manager.I.PackMule.ItemList[ ( int ) type ].Selection.gameObject.activeSelf ) return 0;

            for( int i = 0; i < Manager.I.PackMule.ItemList.Count; i++ )
            if( Manager.I.PackMule.ItemList[ i ].Type == type )
                num += Manager.I.PackMule.ItemList[ i ].Count;

            return num;
        }
        return 0;
    }

    public static bool SetExclusiveItemAmount( ItemType type, float amount, int adv )
    {
        if( type == ItemType.NONE ) return false;
        Item it = G.GIT( type );

        it.PerAdventureCount[ ( int ) adv ] = amount;
        Manager.I.Inventory.Grid.Reposition();

        //G.GIT( type ).Save();
        return true;
    }

    public static bool AddExclusiveItem( ItemType type, float amount, int adv )
    {
        if( type == ItemType.NONE ) return false;
        
        Item it = G.GIT( type );

        it.PerAdventureCount[ ( int ) adv ] += amount;
        Manager.I.Inventory.Grid.Reposition();

        //G.GIT( type ).Save();

        return true;
    }

    public static bool SetAmt( ItemType type, float amount, Inventory.IType itype = Inventory.IType.Inventory, bool force = true, int adv = -1 )
    {
        if( type == ItemType.NONE ) return false;
        ResourceIndicator.UpdateGrid = true;
        Inventory.UpdateGrid = true;
        if( adv != -1 )                                                                             // Exclusive to a certain adventure
        {
            if( G.GIT( type ).PerAdventureCount == null ||
                G.GIT( type ).PerAdventureCount.Count <= 0 )
                Debug.LogError( "Invalid Item " + type + "PerAdventureCount" );
        }

        if( itype == Inventory.IType.Inventory )
        {
            if( type == ItemType.Res_Hook_CCW ) type = ItemType.Res_Hook_CW;                            // Both Hooks counts only as one resource
            Item it = G.GIT( type );
            bool res = true;
            float tot = it.Count + amount;
            if( !force )
            {
                if( it.GetMaxStack() > 0 )
                if( tot > it.GetMaxStack() ) return false;
                if( tot < 0 ) return false;
            }

            if( it.SavePerAdventureCount )
                it.PerAdventureCount[ ( int ) adv ] = amount;
            else
                it.Count = amount;

            if( Manager.I.GameType == EGameType.CUBES )
                if( it.Count > it.GetMaxStack() )
                    if( it.GetMaxStack() > 0 )
                    {
                        if( Helper.I.ReleaseVersion )
                            Message.RedMessage( "No Space for: " + it.GetName() );
                        res = false;
                    }

            float max = it.GetMaxStack();
            if( it.GetMaxStack() <= 0 ) max = 10000000000;
            it.Count = Mathf.Clamp( it.Count, -100, max );

            //G.GIT( type ).Save();                                       // Save Item

            ResourceAdded = true;
            return res;
        }
        return false;
    }
    private float GetMaxStack()
    {
        float max = MaxStack + ExtraCapacity;
        if( Type == ItemType.Energy )
        {
            max += G.Farm.WaterTiles;                                                                     // Energy x farm water tile bonus
        }

        return max;
    }
    public static bool AddItem( ItemType type, float amount, Inventory.IType itype = Inventory.IType.Inventory, bool force = true, int adv = -1 )
    {
        return AddItem( itype, type, amount, force, adv );
    }
    public static bool AddItem( Inventory.IType itype, ItemType type, float amount, bool force = true, int adv = -1 )
    {
        if( type == ItemType.NONE ) return false;
        if( amount == 0 ) return false;
        ResourceIndicator.UpdateGrid = true;
        Inventory.UpdateGrid = true;
        float bonus = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.ITEM_BONUS, type );       // Tech Bonus amount
        if( amount > 0 )
            amount += Util.Percent( bonus, amount );                                                // apply bonus
        float initialamount = amount;

        if( adv != -1 )                                                                             // Exclusive to a certain adventure
        {
            if( G.GIT( type ).PerAdventureCount == null ||
                G.GIT( type ).PerAdventureCount.Count <= 0 )
                Debug.LogError( "Invalid Item " + type + "PerAdventureCount" );
            return AddExclusiveItem( type, amount, adv );
        }

        if( itype == Inventory.IType.Inventory )
        {
            if( type == ItemType.Res_Hook_CCW ) type = ItemType.Res_Hook_CW;                        // Both Hooks counts only as one resource
            Item it = G.GIT( type );
            bool res = true;
            float tot = it.Count + amount;

            PlayCustomSoundFX( type );                                                              // Custom Sound FX for some items

            if( G.HS && G.HS.Type == Sector.ESectorType.NORMAL )                                    // temporarily adds prize to list to be given on cube exit
            if( it.IsGameplayResource == false ) 
            if( IsAdditive( it ) == false )
            if( IgnoreBuffer == false )
            if( Map.I.Finalizing == false )
            if( Map.I.RM.GameOver == false )
            {                
                G.HS.AddGameplayPrize( it, amount );
                if( IgnoreMessage == false )
                    DisplayMessage( amount, initialamount, it, res, 0 );
                IgnoreMessage = false;
                return false;
            }
            IgnoreBuffer = false;

            if( !force )
            {
                if( it.GetMaxStack() > 0 )
                    if( tot > it.GetMaxStack() ) return false;
                if( tot < 0 ) return false;
            }

            if( amount < 0 )
            {
                if( it.Type == ItemType.Goals_Completed ) return true;                               // Do not charge these resources (always additive)
            }

            if( it.SavePerAdventureCount )
            {
                it.PerAdventureCount[ ( int ) Map.I.RM.CurrentAdventure ] += amount;                 // Per Adventure add
                if( amount > 0 )
                    it.PerAdventureTotalCount[ ( int ) Map.I.RM.CurrentAdventure ] += amount;        // Per Adventure total add
            }
            else
            {
                if( TempResource && amount > 0 )
                {
                    it.TempCount += amount;    
                }
                else
                {
                    if( amount < 0 )
                    {
                        if( it.TempCount > 0 )
                        {
                            it.TempCount += amount;

                            RemoveTempResource( amount  );

                            if( it.TempCount < 0 )
                            {
                                amount = it.TempCount;
                                it.TempCount = 0;
                            }
                            else amount = 0; 
                        }
                    }
                    it.Count += amount;                                                           // Item addition
                }
            }

            float max = it.GetMaxStack();
            if( it.GetMaxStack() <= 0 ) max = 10000000000;
            float rest = 0;
            if( it.Count > max )
            {
                rest = it.Count - max;
                it.Count = Mathf.Clamp( it.Count, 0, max );                                              // Max cap clamp
            }
  
            float given = amount - rest;
            if( amount > 0 )
            {
                if( type < ItemType.TechPurchase_0_0 )                                                   // Tech purchase uses Totalgained to store timed tech data
                if( type > ItemType.TechPurchase_5_3 ) 
                it.TotalGained += amount - rest;                                                         // Total Gained throgout the game

                if( Manager.I.GameType == EGameType.FARM )  
                    it.FarmProduction += amount;                                                         // Farm production
                else
                    it.GamePrizes += amount;                                                             // Game prizes
            }        

            if( it.Count > it.MaximumCount )                                                             // Maximum count achieved
                it.MaximumCount = it.Count;

            Mine.UpdateVaultCounter( EMineBonusCnType.COLLECT_X_RESOURCE, null, type, amount );          // updates mine vault counter for this picked resource

            if( type == ItemType.Res_Mining_Bag ||
                type == ItemType.Res_Mining_Sieve ) 
                Mine.UpdateAllMinesText = true;                                                          // updades indicators

            ResourceAdded = true;
            
            if( ShowMessage( it.Type, amount, given ) )
            {
                DisplayMessage( amount, initialamount, it, res, rest );
            }
            PostMessage = "";
            return res;
        }

        if( itype == Inventory.IType.Packmule )
        {
            if( Map.I.RM.CurrentAdventure < 0 ) return false;
            if( Manager.I.GameType != EGameType.CUBES ) return false;

            RandomMapData rm = Map.I.RM.RMList[ Map.I.RM.CurrentAdventure ];
            if( rm.RequiredItem != type )
            if( G.GIT( type ).CanBeEquiped == false ) return false;
            int num = ( int ) GetNum( Inventory.IType.Inventory, type );
            if( num - amount < 0 ) return false;
            
            int free = -1;
            int first = 0;
            int size = ( int ) AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.UPGRADE_PACKMULE );
            int cap = ( int ) AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.PACKMULE_ITEM_CAPACITY, type );
            int packnum = ( int ) GetNum( Inventory.IType.Packmule, type );
            if( type != rm.RequiredItem )
            if( packnum + amount > cap ) return false;

            if( rm.RequiredItem != ItemType.NONE )                                                                  // Required Item
            if( rm.RequiredItemAmount > 0 )
                {
                    if( size >= 1 )
                    {
                        if( type == rm.RequiredItem ) free = 0;
                        else
                            if( size >= 2 ) first = 1;
                    }  
                    if( type != rm.RequiredItem && size <= 1 ) return false;                                       // nor ordinary item over red slot
                }

            if( free == -1 )
            for( int i = first; i < size; i++ )                                                                   // Finds the first free slot
            {
                if( Manager.I.PackMule.ItemList[ i ].Type == ItemType.NONE ||
                  ( Manager.I.PackMule.ItemList[ i ].Type == type &&
                    Manager.I.PackMule.ItemList[ i ].Count <
                    Manager.I.PackMule.ItemList[ i ].GetMaxStack() ) )
                {
                    free = i;
                    break;
                }
            }

            if( amount < 0 ) free = CurrentPackMuleSlot;
            if( free == -1 ) return false;
            Item it = Manager.I.PackMule.ItemList[ free ];
            if( it.CountLifetime ) return false;                                                                    // Item being used, so skip

            //if( Map.I.RM.GameOver == false )                                                  
            //{
            //    if( it.Selection.gameObject.activeSelf ) return false;
            //}

            float tot = it.Count + amount;
            if( !force )
            {
                if( it.GetMaxStack() > 0 )
                    if( tot > it.GetMaxStack() ) return false;
                if( tot < 0 ) return false;
            }

            it.Count += amount;
            it.Type = type;
            if( it.Count <= 0 ) it.Type = ItemType.NONE;      
            it.LifeTimeCount = 0;
            float max = it.MaxStack;
            if( it.GetMaxStack() <= 0 ) max = 10000000000;
            it.Count = Mathf.Clamp( it.Count, 0, max );
            ResourceAdded = true;
        }
        return true;
    }
    private static bool IsAdditive( Item it )
    {
        if( it.Type == ItemType.Clover ) return true;                             // these one always add for game progress and evolution
        if( it.Type == ItemType.Quest_XP ) return true;
        if( it.Type == ItemType.Chest_Points ) return true;
        if( it.Type == ItemType.Butcher_Level ) return true;
        if( it.Type == ItemType.Butcher_Coin_Level ) return true;
        if( it.Type == ItemType.Butcher_Bonus_Quality_Level ) return true;
        if( it.Type == ItemType.Butcher_Slots_Level ) return true;
        if( it.Type == ItemType.Butcher_Power_Level ) return true;
        if( it.Type == ItemType.Blue_Coin ) return true;
        return false;
    }

    private static void DisplayMessage( float amount, float initialamount, Item it, bool res, float rest )
    {
        Color col = Color.green;
        float delay = UnityEngine.Random.Range( 0, 1 );
        Vector2 pos = G.Hero.Pos + new Vector2(
        UnityEngine.Random.Range( -2f, 2f ), UnityEngine.Random.Range( -2f, 2f ) );
        string full = "";
        if( rest > 0 )
        {
            col = Color.red;                                                                     // full
            full = " Full";
        }
        if( amount < 0 ) col = Color.red;
        if( res && Map.I.RM.DungeonDialog.gameObject.activeSelf == false )                       // Display message
            Message.CreateMessage( ETileType.NONE, it.Type,
            initialamount.ToString( "+0.#;-0.#;0" ) + full + PostMessage, pos,
            col, Util.Chance( 50 ), Util.Chance( 50 ), 4, delay, 0, 65 );
    }

    public static float GetTotalGained( ItemType type, int adv = -1 )
    {
        if( adv == -1 ) adv = Map.I.RM.CurrentAdventure;
        if(  G.GIT( type ).SavePerAdventureCount )
            return G.GIT( type ).PerAdventureTotalCount[ adv ];
        else
            return G.GIT( type ).TotalGained;
    }
    private static void PlayCustomSoundFX( ItemType type )
    {
        string snd = "";
        if( type == ItemType.Chicken )
            snd = "Chicken";
        if( snd != "" )
            MasterAudio.PlaySound3DAtVector3( snd, G.Hero.Pos );                                    // FX to warn player
    }

    public static void RemoveTempResource( float amt )
    {
        amt *= -1;
        Sector s = Map.I.RM.HeroSector;
        List<Unit> itl = new List<Unit>();
        for( int y = ( int ) s.Area.yMin; y < s.Area.yMax; y++ )                                       // Check For Items based gate openning
        for( int x = ( int ) s.Area.xMin; x < s.Area.xMax; x++ )
        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( x, y ) ) )
        {
            Unit it = Map.I.GetUnit( ETileType.ITEM, new Vector2( x, y ) );
            if( it && it.Body.ResourceWasteTotalTime > 0 )
            if( it.Body.ResourceWasteTimeCounter > 0 )
            {
                itl.Add( it );
            }
        }

        for( int k = 1; k < itl.Count; k++ )                     // reorder res by highest counter                                                                            // Sorts score list        
        for( int j = 0; j < itl.Count - k; j++ )
        if ( ( itl[ j ].Body.ResourceWasteTimeCounter > 
               itl[ j + 1 ].Body.ResourceWasteTimeCounter ) )
        {
            Unit tempun = itl[ j ];
            itl[ j ] = itl[ j + 1 ];
            itl[ j + 1 ] = tempun;
        }

        for( ; amt > 0; )
        for( int i = 0; i < itl.Count; i++ )
            {
                for( ; itl[ i ].Body.StackAmount > 0; )
                {
                    itl[ i ].Body.StackAmount--;
                    amt--;
                    if( amt <= 0 ) return;
                }
            }
    }

    public static bool ShowMessage( ItemType it, float amt, float given )
    {
        if( ForceMessage == true )
        {
            ForceMessage = false;
            IgnoreMessage = false;
            return true;
        }
        if( IgnoreMessage == true )
          { IgnoreMessage = false; return false; }
        if( given <= 0 ) return false;
        if( it == ItemType.Res_Mining_Points ) return true;    
        if( it == ItemType.Bronze_Trophy ) return false;
        if( it == ItemType.Silver_Trophy ) return false;
        if( it == ItemType.Gold_Trophy ) return false;
        if( it == ItemType.Diamond_Trophy ) return false;
        if( it == ItemType.Adamantium_Trophy ) return false;
        if( it == ItemType.Genius_Trophy ) return false;
        if( it == ItemType.BluePrint_Image_1 ) return false;
        if( it == ItemType.Res_Bow_Arrow && amt <= 0 ) return false;
        if( it == ItemType.Res_SnowStep && amt <= 0 ) return false;
        if( it == ItemType.Res_Melee_Attacks && amt <= 0 ) return false;
        if( it == ItemType.Res_Monster_Kill ) return false;
        if( it == ItemType.Res_Oxygen ) return false;
        if( it == ItemType.Res_Mushroom ) return false;
        if( it == ItemType.Res_HP ) return false;
        if( it == ItemType.Res_Mask ) return false;
        if( it == ItemType.Res_Exploding_Plant ) return false;
        if( it == ItemType.Quest_XP ) return false;
        if( it == ItemType.Energy ) return false;
        if( it == ItemType.Shell ) return false;
        if( it == ItemType.Cog ) return false;
       return true;
    }

    public static bool MoveItem( Inventory.IType to, ItemType itemType, int p, int packID = -1 )
    {
        if( Map.I.RM.GameOver == false ) return false;

        if( to == Inventory.IType.Packmule )                                                               // Move item to packmule
        {
            if( AddItem( Inventory.IType.Packmule, itemType, 1, false ) )
            {
                AddItem( Inventory.IType.Inventory, itemType, -1 );
            }
            return true;
        }
        else
            if( to == Inventory.IType.Inventory )                                                          // Move Item to Inventory
            {
                Item it = G.Packmule.ItemList[ packID ];
                if( it.CountLifetime ) return false;
                if( it.Count < 1 ) return false;
                CurrentPackMuleSlot = packID;
                if( AddItem( Inventory.IType.Inventory, itemType, 1, false ) )
                {
                    AddItem( Inventory.IType.Packmule, itemType, -1, false );
                }
                CurrentPackMuleSlot = -1;
                return true;
            }
        return false;
    }

    public void OnClickButton()
    {
        int amt = 1;
        if( Input.GetMouseButton( 1 ) ) amt = 100;
        CurrentPackMuleSlot = ID;

        if( Map.I.RM.CurrentAdventure >= 0 )
        {
            RandomMapData rm = Map.I.RM.RMList[ Map.I.RM.CurrentAdventure ];                       // Required Item: Moves max
            if( Type == rm.RequiredItem )
                amt = 100;
        }

        if( Map.I.RM.GameOver )
            if( InventoryType == Inventory.IType.Inventory )                                       // Move item from Inventory to Packmule
            {
                for( int i = 0; i < amt; i++ )
                    MoveItem( Inventory.IType.Packmule, Type, 1 );
            }

        if( InventoryType == Inventory.IType.Packmule )                                            // Toggles Packmule item selection
        {
            if( Map.I.RM.GameOver )                                                                // Move item from packmule to Inventory
                for( int i = 0; i < amt; i++ )
                    MoveItem( Inventory.IType.Inventory, Type, 1, ID );

            if( Map.I.RM.GameOver == false )                                                      // Click to use Item 
                UseItem();
        }
        Map.I.InvalidateInputTimer = .5f;
        CurrentPackMuleSlot = -1;
    }

    public void UpdateIconLabel()
    {
        float cont = Count;
        if( SavePerAdventureCount )
        {
            if( Map.I.RM.DungeonDialog.gameObject.activeSelf && 
                Util.VID( PerAdventureCount, Map.I.RM.CurrentAdventure ) )
                cont = PerAdventureCount[ Map.I.RM.CurrentAdventure ];
            else
                cont = Util.SumList( PerAdventureCount );
        }

        IconLabel.color = Color.white;
        float max = GetMaxStack();
        if( max > 0 )
        {
            IconLabel.text = "" + cont.ToString( "0." ) + " / " + max.ToString( "0." );
            if( cont >= max )
                IconLabel.color = Color.red;
        }
        else
            IconLabel.text = "" + cont.ToString( "0." );
    }
    public void Save()
    {
        if( G.Tutorial.CanSave() == false ) return;
        if( SaveData == false ) return;

        if( Farm.ItemSavingAllowed == false ) return;                                                          // Do not save while on farm to avoid power outage bug. only on farm exiting

        if( InventoryType == Inventory.IType.Packmule ) return;

        TF.SaveT( "Count_" + UniqueID, Count );                                                               // Save Item Count

        TF.SaveT( "ExtraCapacity_" + UniqueID, ExtraCapacity );                                               // Save Extra Capacity

        TF.SaveT( "LifeTimeCount_" + UniqueID, LifeTimeCount );                                               // Save Lifetime Count

        TF.SaveT( "ProductionPurchased_" + UniqueID, ProductionPurchased );                                   // Save Production Purchased 
       
        TF.SaveT( "ProductionCount_" + UniqueID, ProductionCount );                                           // Save Production Count

        TF.SaveT( "ExtraProductionTotalTime_" + UniqueID, ExtraProductionTotalTime );                         // Save Extra Production Total Time

        TF.SaveT( "ExtraProductionLimit_" + UniqueID, ExtraProductionLimit );                                 // Save Extra Production Limit

        TF.SaveT( "TotalGained_" + UniqueID, TotalGained );                                                   // Save Total Gained

        TF.SaveT( "MaximumCount_" + UniqueID, MaximumCount );                                                 // Save Maximum count

        TF.SaveT( "FarmProduction_" + UniqueID, FarmProduction );                                             // Save Items Produced in the farm

        TF.SaveT( "GamePrizes_" + UniqueID, GamePrizes );                                                     // Save Items gained while playing

        TF.SaveT( "WarehouseBuiltCount_" + UniqueID, WarehouseBuiltCount );                                   // Save Warehouse Built Count
        
        int count = 0;
        if( Util.HasDada( WarehouseList ) )
            count = WarehouseList.Count;
        TF.SaveT( "WarehouseListCount_" + UniqueID, count );                                                  // Save WarehouseList count

        if( Util.HasDada( WarehouseList ) )
        {
            int c = 0;
            if( BuildingMaxStackList != null ) c = BuildingMaxStackList.Count;
            TF.SaveT( "BuildingMaxStackListCount_" + UniqueID, c );                                           // Max Stack List Size
            if( c > 0 )
                TF.SaveT( "BuildingMaxStackList_" + UniqueID, BuildingMaxStackList );                         // Save Max Stack List
        }

        if( SavePerAdventureCount )                                                                           // Per Adventure item count
        {
            List<string> questIDs = new List<string>();
            List<float> questCounts = new List<float>();
            List<float> questTotals = new List<float>();                                                      // init lists

            for( int i = 0; i < Map.I.RM.RMList.Count; i++ )
            {
                var quest = Map.I.RM.RMList[ i ];
                if( !string.IsNullOrEmpty( quest.UniqueID ) )
                {
                    float num = GetNum( InventoryType, Type, i );
                    float tot = GetTotalGained( Type, i );
                    questIDs.Add( quest.UniqueID );                                                           // fill arrays
                    questCounts.Add( num );
                    questTotals.Add( tot );
                }
            }

            TF.SaveT( "QuestCount_" + UniqueID, questIDs.Count );                                            // Save count    
            for( int i = 0; i < questIDs.Count; i++ )
            {
                TF.SaveT( "QuestID_" + UniqueID + "_" + i, questIDs[ i ] );                                  // Save each ID individually
                TF.SaveT( "QuestCountValue_" + UniqueID + "_" + i, questCounts[ i ] );                       // Save count
                TF.SaveT( "QuestTotalValue_" + UniqueID + "_" + i, questTotals[ i ] );                       // Save total
            }
        }
    }

    public bool Load()
    {
        gameObject.SetActive( false );
        if( SaveData == false ) return false;

        if( InventoryType == Inventory.IType.Packmule ) return false;

        Count = TF.LoadT<float>( "Count_" + UniqueID );                                                      // Load Count

        ExtraCapacity = TF.LoadT<float>( "ExtraCapacity_" + UniqueID );                                      // Load Extra Capacity

        LifeTimeCount = TF.LoadT<float>( "LifeTimeCount_" + UniqueID );                                      // Load Lifetime

        ProductionPurchased = TF.LoadT<int>( "ProductionPurchased_" + UniqueID );                            // Load Production Purchased

        ProductionCount = TF.LoadT<float>( "ProductionCount_" + UniqueID );                                  // Load Production Count   

        ExtraProductionTotalTime = TF.LoadT<float>( "ExtraProductionTotalTime_" + UniqueID );                // Load Extra Production Total Time   

        ExtraProductionLimit = TF.LoadT<int>( "ExtraProductionLimit_" + UniqueID );                          // Load Extra Production Limit  

        TotalGained = TF.LoadT<float>( "TotalGained_" + UniqueID );                                          // Load Total Gained

        MaximumCount = TF.LoadT<float>( "MaximumCount_" + UniqueID );                                        // Load Maximum Count

        FarmProduction = TF.LoadT<float>( "FarmProduction_" + UniqueID );                                    // Load Items Produced in the farm

        GamePrizes = TF.LoadT<float>( "GamePrizes_" + UniqueID );                                            // Load Items gained while playing

        WarehouseBuiltCount = TF.LoadT<int>( "WarehouseBuiltCount_" + UniqueID );                            // Load Warehouse Built Count

        int oldCount = TF.LoadT<int>( "WarehouseListCount_" + UniqueID );                                    // Load old warez size

        if( Util.HasDada( WarehouseList ) )
        {
            BuildingMaxStackList = new List<float>();                                                        // Max Stack List
            int sz = TF.LoadT<int>( "BuildingMaxStackListCount_" + UniqueID );                               // Load Adventure List size
            if( sz > 0 )
                BuildingMaxStackList = TF.LoadT<List<float>>( "BuildingMaxStackList_" + UniqueID );          // Load Building Max Stack List
        }
        else if( oldCount > 0 )                                                                              // Compat with old saves  - this allows you to remove buildings from warehouse list
        {
            int sz = TF.LoadT<int>( "BuildingMaxStackListCount_" + UniqueID );                               // Load Adventure List size (temp) there was a link before but not anymore
            var tmpList = new List<float>();
            if( sz > 0 )
                tmpList = TF.LoadT<List<float>>( "BuildingMaxStackList_" + UniqueID );                       // Load but do not assign
        }

        if( SavePerAdventureCount )                                                                           // Per adventure count - nao pode ser mudado
        {
            int questCount = TF.LoadT<int>( "QuestCount_" + UniqueID );                                       // Load count ; Unity 5.6 safe
            List<string> questIDs = new List<string>();
            List<float> questCounts = new List<float>();
            List<float> questTotals = new List<float>();

            for( int i = 0; i < questCount; i++ )
            {
                questIDs.Add( TF.LoadT<string>( "QuestID_" + UniqueID + "_" + i ) );                         // Load each quest ID
                questCounts.Add( TF.LoadT<float>( "QuestCountValue_" + UniqueID + "_" + i ) );               // Load count
                questTotals.Add( TF.LoadT<float>( "QuestTotalValue_" + UniqueID + "_" + i ) );               // Load total
            }

            if( questIDs == null || questCounts == null ) return false;

            if( PerAdventureCount == null )
                PerAdventureCount = new List<float>();
            if( PerAdventureTotalCount == null )
                PerAdventureTotalCount = new List<float>();                                                    // Init Lists

            int currentQuestCount = Map.I.RM.RMList.Count;
            while( PerAdventureCount.Count < currentQuestCount )
                PerAdventureCount.Add( 0 );
            while( PerAdventureTotalCount.Count < currentQuestCount )                                          // sets value 0
                PerAdventureTotalCount.Add( 0 );

            for( int i = 0; i < Map.I.RM.RMList.Count; i++ )
            {
                var currentQuest = Map.I.RM.RMList[ i ];
                if( !string.IsNullOrEmpty( currentQuest.UniqueID ) )
                {
                    int savedIndex = questIDs.IndexOf( currentQuest.UniqueID );                                // Procura esta quest nos dados salvos
                    if( savedIndex >= 0 && savedIndex < questCounts.Count )
                    {
                        PerAdventureCount[ i ] = questCounts[ savedIndex ];
                        PerAdventureTotalCount[ i ] = questTotals[ savedIndex ];
                    }
                    else
                    {
                        PerAdventureCount[ i ] = 0;                                                            // Quest nova - inicializa com zero
                        PerAdventureTotalCount[ i ] = 0;
                    }
                }
            }
        }
        return true;
    }

    public void UpdateProduction( float addTime = 0 )
    {
        if( ProductionEnabled() == false ) return;

        float tottime = GetStat( EVarType.Production_Total_Time, this );

        if( tottime <= 0 ) return;

        ProductionCount += Time.unscaledDeltaTime + addTime;

        float max = GetMaxStack();                                                          // get stack limit
        if( max < 0 ) max = 100000000;

        float plim = GetStat( EVarType.Production_Limit, this );
        if( plim > 0 )                                                                      // Max Production limited                                    
            max = plim;  
                                                   
        if( Count >= max )                                                                  // Stop production ones since limit was reached
        {
            Count = max;
            ProductionCount = 0;            
        }

        for( ; ; )
        if( ProductionCount >= tottime )
            {
                if( Item.IsPlagueMonster( ( int ) Type, false ))
                if( Map.I.Farm.UpdateBlockingMonsters( this ) )                             // Blocking Monster Creation
                    return;

                if( Type == ItemType.Feather )                                              // Feather Creation
                {
                    Map.I.Farm.UpdateFeatherCreation();
                    return;
                }

                IgnoreMessage = true;
                AddItem( Type, 1,Inventory.IType.Inventory, true );                         // Adds the item

                if( Count >= max )                                                          // limits max
                {
                    Count = max;
                    ProductionCount = 0;
                    break;
                }
                ProductionCount -= tottime;                                                 // decrement timer
            }
            else break;
    }
    public bool ProductionEnabled()
    {
        if( ProductionPurchased == -2 )
            if( BaseProductionActivated == false ) return false;
        if( ProductionPurchased == 0 ) return false;
        return true;
    }

    public void UpdateLifeTime( float addTime = 0 )
    {
        float tottime = GetStat( EVarType.Total_Life_Time, this );
        if( tottime <= 0 ) return;
        if( InventoryType != Inventory.IType.Packmule ) return;
        if( Map.I.RM.GameOver == false )
        if( CountLifetime == false ) return;
        if( Count <= 0 ) return;
        if( Type == ItemType.NONE ) return;

        LifeTimeCount += Time.unscaledDeltaTime + addTime;

        if( LifeTimeCount >= tottime )
        {
            Count = 0;
            LifeTimeCount = 0;
            CountLifetime = false;
        }
    }

    public void UseItem()
    {
        if( Map.I.RM.GameOver ) return;
        switch( Type )
        {
            case ItemType.Coconut:
            float hp = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.HEALING_HP );
            if( Count >= 1 )
                if( Map.I.Hero.Body.Hp + hp <= Map.I.Hero.Body.TotHp )
                {
                    if( Item.AddItem( InventoryType, ItemType.Coconut, -1 ) )
                    {
                        Map.I.RM.DungeonDialog.SetMsg( "Coconut Consumed!\n HP +" + hp, Color.green );
                        Map.I.Hero.Body.AddHP( hp );
                    }
                }
            break;
        }
    }
    
    public string GetName()
    {
        string nm = name;
        nm = nm.Replace( "Res_", "" );
        nm = nm.Replace( '_', ' ' );
        return nm;
    }
    public void ApplyUpgrade( Blueprint bp )
    {
        if( bp.AffectedVariable == EVarType.NONE ) return;

        float power = Blueprint.GetStat( EVarType.BluePrint_Effect_Amount, bp );
        Blueprint.UpgradePower = UnityEngine.Random.Range( 0, bp.AffectedVariableBonusAmount );
        power += Blueprint.UpgradePower; 

        string perc = "";
        switch( bp.AffectedVariable )
        {
            case EVarType.Crafting_Bonus_Factor:
            perc = "%";
            break;

            case EVarType.Production_Total_Time:
            ExtraProductionTotalTime += power;
            break;

        }

        string nm = Util.GetName( bp.AffectedVariable.ToString() );
        Message.GreenMessage( "Item Upgraded!\n" + nm + "\nAmount: " + power.ToString( "+#;-#;0" ) + perc );
    }

    public static float GetStat( EVarType var, Item it )
    {
        switch( var )
        {
            case EVarType.Production_Total_Time:
            return it.BaseProductionTotalTime + it.ExtraProductionTotalTime;

            case EVarType.Production_Limit:
            return it.BaseProductionLimit + it.ExtraProductionLimit;

            case EVarType.Total_Life_Time:
            it.TotalLifeTime = Blueprint.GetUseSum( var );
            return Map.I.RM.RMD.RequiredItemLifeTime + it.BaseTotalLifeTime + it.TotalLifeTime +
            ( int ) AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.INCREASE_REQUIRED_ITEM_TIME );

            case EVarType.Carry_Capacity:

            it.CarryCapacity = Blueprint.GetUseSum( var );
            return it.BaseCarryCapacity + it.CarryCapacity;

            case EVarType.Road_Carry_Capacity:
            it.RoadCarryCapacity = Blueprint.GetUseSum( var );
            return it.BaseRoadCarryCapacity + it.RoadCarryCapacity;

            case EVarType.PackMule_Capacity:
            float cap = Blueprint.GetUseSum( var );

            float basecap = 0;
            if( Map.I.RM.CurrentAdventure != -1 )
                if( it.BasePackmuleCapacityPerAdventure != null )
                    if( it.BasePackmuleCapacityPerAdventure.Count > 0 )
                        if( ( int ) Map.I.RM.CurrentAdventure < it.BasePackmuleCapacityPerAdventure.Count )
                            basecap = it.BasePackmuleCapacityPerAdventure[ ( int ) Map.I.RM.CurrentAdventure ];
            return basecap + cap;
        }
        return -1;
    }

    public float GetCount()
    {
        return Item.GetNum( InventoryType, Type );
    }

    public static string GetName( ItemType type )
    {
        if( Application.isPlaying == false )                               // to avoid in editor bug. Dictionary not built
        {
            return type.ToString();
        }

        Item it = G.GIT( type );
        return it.GetName();
    }

    public static bool IsPlagueMonster( int p, bool others = true )
    {
        if( p == ( int ) ItemType.Plague_Monster_Cloner ) return true;
        if( p == ( int ) ItemType.Plague_Monster_Spawner ) return true;
        if( p == ( int ) ItemType.Plague_Monster_Slayer ) return true;
        if( p == ( int ) ItemType.Plague_Monster_Kickable ) return true;
        if( p == ( int ) ItemType.Plague_Monster_Swap ) return true;
        if( p == ( int ) ItemType.Plague_Monster_Blocker ) return true;
        if( p == ( int ) ItemType.Plague_Monster_Grabber ) return true;
        if( others )
        {
            if( p == ( int ) ItemType.HoneyComb ) return true;
            if( p == ( int ) ItemType.Feather ) return true;
            if( p == ( int ) ItemType.Chicken ) return true;
        }
        return false;
    }

    public static int GetMax( ItemType it )
    {
      return ( int ) G.GIT( it ).MaxStack;
    }

    public static float GetBn( ItemType it )
    {
        return ( float ) G.GIT( it ).StartingBonus;
    }
    public bool HasWarehouse()
    {
        if( WarehouseList != null )
        if( WarehouseList.Count >= 1 )
            return true;
        return false;
    }
    public bool HasMaxStack()
    {
        if( BuildingMaxStackList != null )
        if( BuildingMaxStackList.Count > 0 )
            return true;
        return false;
    }
    public static ItemType GetSortedItem( ItemType type )
    {
        switch( type )
        {
            case ItemType.RND_Butcher_Level:
            type = ( ItemType ) ItemType.Butcher_Level + UnityEngine.Random.Range( 0, 5 );
            break;
        }
        return type;
    }
}
