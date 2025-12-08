using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using Sirenix.OdinInspector;

public enum BuildingType
{
    NONE = -1,
    Stone_Storage, Wood_Pile,
    Wood_Cutter, Work_Area, 
    Tools_Building, Berry_Storage, Berry_Bush,
    Forge, Firepit,
    BlackBerry_Bush, Strawberry_Bush, Warehouse, 
    Food_Warehouse                                          // ends here:  ********* Caution:  ******** This list must be in the same order of Farm.BuildingList or you may have bugs  
}

public enum EBuildingCategory
{
    NONE = -1,
    Producer, Storage, Work_Area, Plant
}

public enum EVarType
{
    NONE = 0,
    Production_Total_Time, Item_Count, Maximum_Item_Stack, Carry_Capacity, Road_Carry_Capacity,
    Crafting_Bonus_Factor = 12, BluePrint_Success_Rate, BluePrint_Cost,
    BluePrint_ReSort_Chance, BluePrint_Effect_Amount, Total_Life_Time, Total_Building_Production_Time, PackMule_Capacity,
    Production_Limit
}

public class Building : MonoBehaviour
{
    #region Variables
    [TabGroup( "Main" )]
    public int ID;
    [TabGroup( "Main" )]
    [ReadOnly()]
    public string UniqueID = "";          // Unique ID for Saving
    [TabGroup( "Link" )]
    public Unit Unit;
    [TabGroup( "Link" )]
    public Building RelatedBuilding;
    [TabGroup( "Main" )]
    public BuildingType Type = BuildingType.NONE;
    [TabGroup( "Main" )]
    public EBuildingCategory Category = EBuildingCategory.NONE;
    [TabGroup( "Main" )]
    public int SelItemID = 0;
    [TabGroup( "Main" )]
    public int ConstructionAllowed = 1;          // -1 for unlimited
    [TabGroup( "Main" )]
    public int BonusConstructionAllowed = 0;     // increase this value for extra buildings. Its already being saved
    [TabGroup( "Main" )]
    public float UnitsConstructed = 0;
    [TabGroup( "Other" )]
    public float CustomProductionTime = -1;       // Custom production time for each instance created instead of using master list. (axes for example)
    [TabGroup( "Other" )]
    public int FertilizingCount = 0;
    [TabGroup( "Other" )]
    public int WateringCount = 0;
    [TabGroup( "Other" )]
    public ItemType ToolType = ItemType.NONE;
    [TabGroup( "Other" )]
    public float FertilizingTimeCount = 0;
    [TabGroup( "Other" )]
    public float WateringTimeCount = 0;
    [TabGroup( "Other" )]
    public ItemType ConstructionLimitItem = ItemType.NONE;
    [TabGroup( "Other" )]
    public float MovingWheelBarrowCost = 10;
    public static bool BuildingHover = false;
    [TabGroup( "Link" )]
    public List<tk2dSprite> SpriteList;
    [TabGroup( "Link" )]
    public List<tk2dSprite> AcessoryList;
    [TabGroup( "Link" )]
    public tk2dSprite SelItemSprite;
    [TabGroup( "Link" )]
    public List<BuildingItem> Itm;
    [TabGroup( "Link" )]
    public List<Recipe> RecipeList;
    public int SelectedRecipeID = -1;
    public static List<Unit> Bl = null;
    public static Unit Bld;

    #endregion
    public void Copy( Building bl )
    {
        RelatedBuilding = bl.RelatedBuilding;
        Type = bl.Type;
        Category = bl.Category;
        ToolType = bl.ToolType;
        ConstructionAllowed = bl.ConstructionAllowed;
        UnitsConstructed = bl.UnitsConstructed;
        ConstructionLimitItem = bl.ConstructionLimitItem;
        MovingWheelBarrowCost = bl.MovingWheelBarrowCost;
        SelItemID = bl.SelItemID;
        FertilizingCount = bl.FertilizingCount;
        CustomProductionTime = bl.CustomProductionTime;
        WateringCount = bl.WateringCount;
        FertilizingTimeCount = bl.FertilizingTimeCount;
        WateringTimeCount = bl.WateringTimeCount;        
        for( int i = 0; i < bl.AcessoryList.Count; i++ )
        {
            AcessoryList[ i ].transform.localPosition = bl.AcessoryList[ i ].transform.localPosition;
            AcessoryList[ i ].spriteId = bl.AcessoryList[ i ].spriteId;
            AcessoryList[ i ].scale = bl.AcessoryList[ i ].scale;
        }

        for( int i = 0; i < bl.Itm.Count; i++ )
        {
            Itm[ i ].Copy( bl.Itm[ i ] );
        }

        for( int i = 0; i < bl.RecipeList.Count; i++ )
        {
            RecipeList[ i ].Copy( bl.RecipeList[ i ] );
        }
    }
    public void UpdateProduction( double addTime = 0 )
    {
        if( Category != EBuildingCategory.Producer ) return;
        for( int i = 0; i < Itm.Count; i++ )
            Itm[ i ].UpdateBuildingItemProduction( this, addTime, i );
    }

    public void UpdateRecipe( double addTime = 0 )
    {
        Recipe.UpdateChosen( this, addTime );
        for( int i = 0; i < RecipeList.Count; i++ )
            RecipeList[ i ].UpdateAll( this, addTime, i );
    }

    public void UpdateTool( double addTime = 0 )
    {
        if( Category != EBuildingCategory.Work_Area ) return;
        for( int i = 0; i < Itm.Count; i++ )
            Itm[ i ].UpdateBuildingToolUsage( this, addTime, i );
    }
    public void UpdatePlants( double addTime = 0 )
    {
        if( Category != EBuildingCategory.Plant ) return;
        for( int i = 0; i < Itm.Count; i++ )
            Itm[ i ].UpdatePlantItem( this, addTime, i );
    }
    
    public void UpdateText()
    {
        if( Itm == null || Itm.Count <= 0 ) return;
        BuildingItem it = Itm[ SelItemID ];
        float max = Building.GetStat( EVarType.Maximum_Item_Stack, this, SelItemID );
        Unit.LevelTxt.text = "" + it.ItemCount.ToString( "0." );
        if( it.BaseMaxItemStack > 0 ) Unit.LevelTxt.text += "/" + max.ToString( "0." );

        if( it.ItemCount < 1 ) Unit.LevelTxt.text = "Empty";

        float prod = GetStat( EVarType.Total_Building_Production_Time, this, SelItemID );

        if( Category == EBuildingCategory.Plant )
        {
            Unit.HealthBar.gameObject.SetActive( false );
            Unit.LevelTxt.text = Util.ToSTime( prod - it.ProductionTimeCount );
            if( it.WorkIsDone )
                Unit.LevelTxt.text = "";
        }

        if( Category == EBuildingCategory.Work_Area )
        {
            Unit.LevelTxt.text = Util.ToSTime( prod - it.ProductionTimeCount );
        }
        else
        {
            Unit.HealthBar.gameObject.SetActive( false );
        }
    }

    public static bool Place( bool apply, Vector2 tg, BuildingType type, ItemType tool = ItemType.NONE, bool move = false )
    {
        if( type == BuildingType.NONE ) return false;
        Unit ga2 = Map.I.GetUnit( tg, ELayerType.GAIA2 );
        if( Map.I.Farm.CheckFarmLimit( tg ) == false ) return false;                                                 // Check Farm Limit
        Unit ga  = Map.I.GetUnit( tg, ELayerType.GAIA );
        int id = -1;
        for( int i = 0; i < Map.I.Farm.BuildingList.Count; i++ )                                                     // Find the Building in the list
        if( Map.I.Farm.BuildingList[ i ].Type == type ) id = i;
        if( id == -1 ) return false;
        Building tgbl = Map.I.Farm.BuildingList[ id ];
        Bl = null;
        
        if( ga != null )
        {
            if( tgbl.Category != EBuildingCategory.Plant )
            if( ga.TileID == ETileType.MUD )
            {
                if( tool != ItemType.Hoe )
                if( tool != ItemType.Shovel )
                    return false;                                                                                   // Build over mud error
            }

            if( ga.TileID == ETileType.WATER )
            {
                if( tool != ItemType.Shovel )
                    return false;                                                                                   // Build over water error
            } 

            if( tgbl.Category == EBuildingCategory.Producer ||                                                      // no building over gaia for these
                tgbl.Category == EBuildingCategory.Storage )
            return false;
        }

        if( move == false )
        if( Farm.CreatingBuildings == false )
        if( type != BuildingType.Work_Area )
        if( tgbl.ConstructionAllowed != -1 )
        if( tgbl.UnitsConstructed >= tgbl.ConstructionAllowed )                                                    // Error: Too many of the same type            
            return false;

        if( ga2 == null )
        {
            if( apply )                                                                                             // Creates The Object
            {
                GameObject go = Map.I.CreateUnit( tgbl.Unit, tg );
                Unit res = go.GetComponent<Unit>();
                if( res ) Bld = res;
                res.Building.Copy( tgbl );
                Controller.CreateMagicEffect( tg );                                                                 // Magic FX  

                if( type == BuildingType.Work_Area )
                    Map.I.Farm.SetSelectedItem( Map.I.Farm.SelectedItem, Map.I.Farm.CarryingAmount - 1 );           // Destroys wood axe
                
                for( int i = 0; i < res.Building.Itm.Count; i++ )                                                   
                {

                    BuildingItem bi = res.Building.Itm[ i ];
                    int itid = ( int ) bi.ItemType;                                                                 // Place items in the building if its the first time built


                    G.GIT( itid ).WarehouseBuiltCount++;



                    if( G.GIT( itid ).BuildingMaxStackList == null ||
                        G.GIT( itid ).BuildingMaxStackList.Count == 0 )
                        {
                            float amt = G.GIT( itid ).Count;
                            AddItem( true, ( ItemType ) itid, amt );
                            float max = Building.GetStat( EVarType.Maximum_Item_Stack, res.Building, i );
                            if( max > 0 )
                                G.GIT( itid ).BuildingMaxStackList.Add( max );
                        }
                }
            }
            return true;
        }
        else
            if( ga2.TileID == ETileType.ITEM ) return true;

        return false;
    }

    public static bool ValidForestTileForChopping( Vector2 tg )                                        // to avoid tunnels in the farm, require 3 open tiles around tg
    {
        int count = 0;
        for( int d = 0; d < 8; d++ )
        {
            Vector2 pos = tg + Manager.I.U.DirCord[ ( int ) d ];
            if( Map.PtOnMap( Map.I.Tilemap, pos ) )
            {
                Unit un = Map.I.GetUnit( ETileType.FOREST, pos );
                if( un == null ) count++;
            }
        }
        if( count >= 3 ) return true;
        Message.RedMessage( "Error: Target Requires at least\n 3 open tiles around it." );
        return false;
    }
    public static bool PlaceItemScatered( ItemType type, int amt )                                                    // place an item in a random warehouse 
    {
        List<BuildingItem> il = GetBuildingItemList( type );
        if( il.Count > 0 )
        {
            PlaceItem( il[ 0 ].Building.Unit.Pos, amt, type, true );                                                  // WARNING: multiple buildings may cause future problems. For now theres only one bulding per item
            return true;                                                                                              // make a preference warehouse and spread through buildings
        }
        return false;
    }

    public static bool PlaceItem( Vector2 tg, int amt, ItemType force = ItemType.NONE, bool forcePlacing = false )
    {
        Unit bl = Map.I.GetUnit( tg, ELayerType.GAIA2 );
        if( tg.x != -1 )
        if( Map.I.Farm.CheckFarmLimit( tg ) == false ) return false;                                  // Check Farm Limit

        if( bl != null )
        {
            if( bl.TileID != ETileType.BUILDING ) return false;
            BuildingItem it = null;
            if( forcePlacing )                                                                        // Upgrade Item Removal
            {
                int id = -1;
                for( int i = 0; i < bl.Building.Itm.Count; i++ )                                      // Dropping on building that has a different item selected
                if ( bl.Building.Itm[ i ].ItemType == force )
                     id = i;
                if( id == -1 )
                {
                    PlaceItemScatered( force, amt );                                                  // No Building found on target, Find a Building and call function again
                    return false;
                }
                it = bl.Building.Itm[ id ];

                if( it.BaseMaxItemStack > 0 )  // new
                if( amt > 0 )
                if( it.ItemCount >= it.BaseMaxItemStack ) return false;                               // max limit reached
                it.ItemCount += amt;
                return true;
            }

            it = bl.Building.Itm[ bl.Building.SelItemID ];
            if( it.ItemType == ItemType.NONE ) return false;
            float max = Building.GetStat( EVarType.Maximum_Item_Stack, 
                                   bl.Building, bl.Building.SelItemID );

            if( it.ItemType != Map.I.Farm.SelectedItem )                                              // Dropping on building that has a different item selected
            if( Map.I.Farm.CarryingAmount > 0 )
                {
                    int bldid = -1;
                    for( int i = 0; i < bl.Building.Itm.Count; i++ )                                
                    if ( bl.Building.Itm[ i ].ItemType == Map.I.Farm.SelectedItem )
                        bldid = i;

                    if( bldid == -1 ) return false;
                    max = Building.GetStat( EVarType.Maximum_Item_Stack,
                    bl.Building, bldid );

                    if( amt == 1 )  
                       {
                           if( bl.Building.Itm[ bldid ].ItemCount + amt > max )
                               return false;
                           bl.Building.Itm[ bldid ].ItemCount += amt;
                           Map.I.Farm.SetSelectedItem( bl.Building.Itm[ bldid ].ItemType, 
                           Map.I.Farm.CarryingAmount - amt );
                           return true;                               
                       }
                    return false;
                }                    

            if( amt > 0 )
            {
                if( bl.Building.Category == EBuildingCategory.Plant ) return false;
                if( it.BaseMaxItemStack > 0 )
                if( it.ItemCount + amt > max ) return false;     
                if( Map.I.Farm.CarryingAmount < 1 ) return false;
            }
            else
            {
                Item itm = G.GIT( it.ItemType );
                if( bl.Building.Category == EBuildingCategory.Plant )                          // No fruit pickin up if not ripe yet
                if( it.WorkIsDone == false ) return false;                   
 
                float cap = ( int ) Item.GetStat( EVarType.Carry_Capacity, itm );  
                Unit road = Map.I.GetUnit( ETileType.ROAD, G.Hero.Pos );
                if( road ) cap += ( int ) Item.GetStat( EVarType.Road_Carry_Capacity, itm );

                if( amt < 0 && Map.I.Farm.CarryingAmount >= cap )
                {
                    Message.RedMessage( "Maximum Carry Capacity Reached: " + cap );
                    return false;
                }

                if( it.ItemCount < 1 ) return false;
            }
                     
            Map.I.Farm.SetSelectedItem( ( ItemType ) it.ItemType, Map.I.Farm.CarryingAmount - amt );

            it.ItemCount += amt;

            if( it.ItemCount < 1 )                                                                       // Last fruit picked up, Destroy Plant
            if( bl.Building.Category == EBuildingCategory.Plant )
                bl.Kill();
        }
        return true;
    }
    public static void LoadBuildings( ref tk2dTileMap tm )
    {
        string file = Manager.I.GetProfileFolder() + "Building.NEO";                                      // Provides File name
        Farm.CreatingBuildings = true;

        byte[] fileData = File.ReadAllBytes( file );                                                      // Read full file
        byte[] content = Security.CheckLoad( fileData );                                                  // Validate HMAC and get clean content

        using( GS.R = new BinaryReader( new MemoryStream( content ) ) )                                   // Use MemoryStream for TF
        {
            int SaveVersion = Security.LoadHeader();                                                      // Load Header

            TF.ActivateFieldList( "Building" );                                                           // Activates Building Tagged Field List

            int sz = TF.LoadT<int>( "BuildingListSize" );                                                 // Load Building list size ; temporary load

            for( int b = 0; b < sz; b++ )
            {
                Vector2 pos = TF.LoadT<Vector2>( "BuildingPos" + b );                                     // Load Building pos ; temporary load
                BuildingType type = TF.LoadT<BuildingType>( "BuildingType" + b );                         // Load Building type ; temporary load

                Place( true, pos, type );                                                                 // Building objects are created only now
                Building bl = Map.I.Gaia2[ ( int ) pos.x, ( int ) pos.y ].Building;
                bl.Type = type;
                bl.Copy( Map.I.Farm.BuildingList[ ( int ) type ] );

                bl.CustomProductionTime = TF.LoadT<float>( "CustomProductionTime" + b );                  // Load Custom production Time ; temporary load
                bl.ToolType = TF.LoadT<ItemType>( "ToolType" + b );                                       // Load Tool ; temporary load

                ItemType ittype = TF.LoadT<ItemType>( "SelectedItemType" + b );                           // Load Building Selected item type (not ID) ; temporary load
                bl.SelItemID = GetBuildingItemID( ittype, bl );                                           // Selects the id based on item type saved
                if( bl.SelItemID == -1 ) bl.SelItemID = 0;                                                // If item has been removed, select first

                int itmsz = TF.LoadT<int>( "BuildingItemArraySize" + b );                                 // Load Building Item array size ; temporary load

                // Load all item lists ; temporary loads
                List<float> itemCounts = TF.LoadT<List<float>>( "ItemCountList" + b );                    // Load item counts list
                List<float> showAccessories = TF.LoadT<List<float>>( "ShowAccessoriesList" + b );         // Load shown accessories list  
                List<float> additivePlanting = TF.LoadT<List<float>>( "AdditivePlantingList" + b );       // Load additive planting factors list
                List<float> productionTime = TF.LoadT<List<float>>( "ProductionTimeList" + b );           // Load production time counts list
                List<int> toolHits = TF.LoadT<List<int>>( "ToolHitList" + b );                            // Load tool hit counts list
                List<bool> workDone = TF.LoadT<List<bool>>( "WorkDoneList" + b );                         // Load work done status list

                for( int it = 0; it < bl.Itm.Count; it++ )
                {
                    bl.Itm[ it ].ItemCount = itemCounts[ it ];                                            // Apply loaded item count to building item
                    bl.Itm[ it ].ShownAcessories = showAccessories[ it ];                                 // Apply loaded shown accessories to building item
                    bl.Itm[ it ].AdditivePlantingFactor = additivePlanting[ it ];                         // Apply loaded additive planting factor to building item
                    bl.Itm[ it ].ProductionTimeCount = productionTime[ it ];                              // Apply loaded production time count to building item
                    bl.Itm[ it ].ToolHitCount = toolHits[ it ];                                           // Apply loaded tool hit count to building item
                    bl.Itm[ it ].WorkIsDone = workDone[ it ];                                             // Apply loaded work done status to building item
                }

                int rcpsz = TF.LoadT<int>( "RecipeListCount" + b );                                       // Load recipe array size ; temporary load
                List<bool> recipeActivated = TF.LoadT<List<bool>>( "RecipeActivatedList" + b );           // Load recipe activation status list
                List<int> recipeLevel = TF.LoadT<List<int>>( "RecipeLevelList" + b );                     // Load recipe levels list
                List<float> recipeTimeCount = TF.LoadT<List<float>>( "RecipeTimeCountList" + b );         // Load recipe time counts list
                List<int> recipeQueueLength = TF.LoadT<List<int>>( "RecipeQueueLengthList" + b );         // Load recipe queue lengths list

                for( int r = 0; r < bl.RecipeList.Count; r++ )
                {
                    bl.RecipeList[ r ].Activated = recipeActivated[ r ];                                  // Apply loaded activation status to recipe
                    bl.RecipeList[ r ].Level = recipeLevel[ r ];                                          // Apply loaded level to recipe
                    bl.RecipeList[ r ].TimeCount = recipeTimeCount[ r ];                                  // Apply loaded time count to recipe
                    bl.RecipeList[ r ].QueueLenght = recipeQueueLength[ r ];                              // Apply loaded queue length to recipe
                }
            }

            int blsz = TF.LoadT<int>( "GlobalBuildingListSize" );                                         // Load Global Building Size
            List<int> bonusList = TF.LoadT<List<int>>( "GlobalBuildingBonusList" );                       // Load Global Building Bonus List
            for( int i = 0; i < blsz; i++ )
            {
                G.Farm.BuildingList[ i ].BonusConstructionAllowed = bonusList[ i ];                       // Apply Value
            }

            GS.R.Close();                                                                                 // Close stream
        }
        Farm.CreatingBuildings = false;
    }

    public static void SaveBuildings( ref tk2dTileMap tm )
    {
        if( Manager.I.SaveOnEndGame == false ) return;
        string file = Manager.I.GetProfileFolder() + "Building.NEO";                        // Provides File name

        using( MemoryStream ms = new MemoryStream() )
        using( BinaryWriter writer = new BinaryWriter( ms ) )                               // Open Memory Stream
        {
            GS.W = writer;                                                                  // Assign BinaryWriter to GS.W for TF

            int Version = Security.SaveHeader( 1 );                                         // Save Header Defining Current Save Version

            TF.ActivateFieldList( "Building" );                                             // Activates Building Tagged Field List

            List<Building> bl = new List<Building>();

            for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                // Warning: saving only farm area         
            {
                int x = G.Farm.Tl[ tid ].x;
                int y = G.Farm.Tl[ tid ].y;
                if( Map.I.Gaia2[ x, y ] )
                if( Map.I.Gaia2[ x, y ].TileID == ETileType.BUILDING )                      // Fills Building Array
                    bl.Add( Map.I.Gaia2[ x, y ].Building );
            }

            TF.Save( "BuildingListSize", bl.Count );                                        // Save Building list size

            for( int b = 0; b < bl.Count; b++ )
            {
                TF.SaveT( "BuildingPos" + b, bl[ b ].Unit.Pos );                            // Save Building pos
                TF.SaveT( "BuildingType" + b, bl[ b ].Type );                               // Save Building type
                TF.SaveT( "CustomProductionTime" + b, bl[ b ].CustomProductionTime );       // Save Custom production Time
                TF.SaveT( "ToolType" + b, bl[ b ].ToolType );                               // Save Tool

                ItemType tp = bl[ b ].Itm[ bl[ b ].SelItemID ].ItemType;
                TF.SaveT( "SelectedItemType" + b, tp );                                     // Save Building Selected item type (not ID)
                TF.SaveT( "BuildingItemArraySize" + b, bl[ b ].Itm.Count );                 // Save Building Item array size

                List<float> itemCounts = new List<float>();                                 // Declare lists
                List<float> showAccessories = new List<float>();
                List<float> additivePlanting = new List<float>();
                List<float> productionTime = new List<float>();
                List<int> toolHits = new List<int>();
                List<bool> workDone = new List<bool>();

                for( int it = 0; it < bl[ b ].Itm.Count; it++ )
                {
                    var item = bl[ b ].Itm[ it ];                                           // Add lists
                    itemCounts.Add( item.ItemCount );
                    showAccessories.Add( item.ShownAcessories );
                    additivePlanting.Add( item.AdditivePlantingFactor );
                    productionTime.Add( item.ProductionTimeCount );
                    toolHits.Add( item.ToolHitCount );
                    workDone.Add( item.WorkIsDone );
                }

                TF.SaveT( "ItemCountList" + b, itemCounts );                                // Save Item counts list
                TF.SaveT( "ShowAccessoriesList" + b, showAccessories );                     // Save Shown accessories list
                TF.SaveT( "AdditivePlantingList" + b, additivePlanting );                   // Save Additive planting factors list
                TF.SaveT( "ProductionTimeList" + b, productionTime );                       // Save Production time counts list
                TF.SaveT( "ToolHitList" + b, toolHits );                                    // Save Tool hit counts list
                TF.SaveT( "WorkDoneList" + b, workDone );                                   // Save Work done status list
                TF.SaveT( "RecipeListCount" + b, bl[ b ].RecipeList.Count );                // Save Recipe array size

                List<bool> recipeActivated = new List<bool>();
                List<int> recipeLevel = new List<int>();
                List<float> recipeTimeCount = new List<float>();
                List<int> recipeQueueLength = new List<int>();

                for( int r = 0; r < bl[ b ].RecipeList.Count; r++ )
                {
                    var recipe = bl[ b ].RecipeList[ r ];                                   // Add Lists
                    recipeActivated.Add( recipe.Activated );
                    recipeLevel.Add( recipe.Level );
                    recipeTimeCount.Add( recipe.TimeCount );
                    recipeQueueLength.Add( recipe.QueueLenght );
                }

                TF.SaveT( "RecipeActivatedList" + b, recipeActivated );                     // Save Recipe activation status list
                TF.SaveT( "RecipeLevelList" + b, recipeLevel );                             // Save Recipe levels list
                TF.SaveT( "RecipeTimeCountList" + b, recipeTimeCount );                     // Save Recipe time counts list
                TF.SaveT( "RecipeQueueLengthList" + b, recipeQueueLength );                 // Save Recipe queue lengths list
            }

            TF.Save( "GlobalBuildingListSize", G.Farm.BuildingList.Count );                 // Save Global Building list size

            List<int> bonusList = new List<int>();
            for( int i = 0; i < G.Farm.BuildingList.Count; i++ )
            {
                bonusList.Add( G.Farm.BuildingList[ i ].BonusConstructionAllowed );
            }

            TF.Save( "GlobalBuildingBonusList", bonusList );                                // Save Global Building bonus list
            
            GS.W.Flush();                                                                   // Flush the writer

            Security.FinalizeSave( ms, file );                                              // Finalize save
        }                                                                                   // using closes the stream automatically
    }
    public static void UpdateItemData( bool saveInventory = false )
    {
        List<int> withware = new List<int>();
        
        for( int i = 0; i < G.Inventory.ItemList.Count; i++ )                                                                 // Loop Through all Items in Inventory and reset Counters
            if( G.Inventory.ItemList[ i ] )                                                                                   // Warning: Dont use G.GIT here: the access is directly to the itemlist via loop and id    
        {
            if( G.Inventory.ItemList[ i ].HasWarehouse() )
            {
                G.Inventory.ItemList[ i ].AuxCount = G.Inventory.ItemList[ i ].Count;                                         // save data for later case item with warez do not have warez built yet. 
                G.Inventory.ItemList[ i ].AuxMaxStack = G.Inventory.ItemList[ i ].MaxStack;                                   // othewise values would be set to zero
                G.Inventory.ItemList[ i ].MaxStack = 0;
                G.Inventory.ItemList[ i ].Count = 0;
                G.Inventory.ItemList[ i ].BuildingMaxStackList = new List<float>();
                G.Inventory.ItemList[ i ].WarehouseBuiltCount = 0;
                withware.Add( i );
            }
        }

        for( int i = 0; i < G.Farm.BuildingList.Count; i++ )                                                                  // Loop Through all Buildings int the List and reset Counters
            {
                G.Farm.BuildingList[ i ].UnitsConstructed = 0;
            }

        G.Farm.FarmSize = 0;
        G.Farm.MudTiles = 0;                                                                                                  // zero counters
        G.Farm.WaterTiles = 0;

        for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                                                      // Loop Through all Buildings in the MAP and Update Values
        {
            int x = G.Farm.Tl[ tid ].x;
            int y = G.Farm.Tl[ tid ].y;

            if( G.Farm.CheckFarmLimit( new Vector2( x, y ), false ) )                                                         // Calculates Farm Size
            {
                bool isFarm = true;
                if( Map.I.Gaia[ x, y ] )
                {
                    if( Map.I.Gaia[ x, y ].TileID == ETileType.MUD ) 
                        G.Farm.MudTiles++;                                                                                    // mud tiles

                    if( Map.I.Gaia[ x, y ].TileID == ETileType.WATER ) 
                        G.Farm.WaterTiles++;                                                                                  // water tiles

                    if( Map.I.Gaia[ x, y ].TileID == ETileType.FOREST ||
                        Map.I.Gaia[ x, y ].TileID == ETileType.WATER ) isFarm = false;
                }
                if( isFarm ) G.Farm.FarmSize++;
            }

            if( Map.I.Gaia2[ x, y ] )
            {
                if( Map.I.Gaia2[ x, y ].TileID == ETileType.BUILDING )
                {
                    Building bld = Map.I.Gaia2[ x, y ].Building;

                    G.Farm.BuildingList[ ( int ) bld.ID ].UnitsConstructed++;                                                 // Updates Units Constructed

                    for( int i = 0; i < bld.Itm.Count; i++ )
                        if( bld.Category != EBuildingCategory.Plant )
                        {
                            BuildingItem blit = bld.Itm[ i ];
                            int itid = ( int ) blit.ItemType;                                                                   // Counts Items

                            Item item = G.GIT( itid );

                            if( item.HasWarehouse() )
                            {
                                float max = Building.GetStat( EVarType.Maximum_Item_Stack,
                                Map.I.Gaia2[ x, y ].Building, i );

                                //if( max >= 0 ) // old
                                {
                                    item.MaxStack += max;
                                    item.BuildingMaxStackList.Add( max );                                           // Updates BuildingMaxStackList
                                }

                                item.BuildingMaxStackOverride = max;                                                // new: to make it possible to revert saved item maxstack to infinite (-1) again  // file:///C:/Users/alien/Desktop/Docs/infinity.m4a just change basemaxstack on buildingitem to -1 and it can be reverted

                                item.Count += blit.ItemCount;                                                       // updates item number so it appears on the inventory

                                withware.Remove( item.ID );

                                item.WarehouseBuiltCount++;                                                         // fills the WarehouseBuilt List 
                            }
                        }
                }

                if( Map.I.Gaia2[ x, y ].TileID == ETileType.ITEM )                                                         // Count Ground Items                    
                {
                    int it = ( int ) Map.I.Gaia2[ x, y ].Variation;
                    if( G.GIT( it ).HasWarehouse() )
                    if( G.GIT( it ).HasMaxStack() )
                        G.GIT( it ).Count += Map.I.Gaia2[ x, y ].Body.StackAmount;
                }
            }
        }

        for( int i = 0; i < withware.Count; i++ )                                                                            // The ones in this list are Items that have warehouse but it has not yet been built yet
        {                                                                                                                    // so, restore aux numbers
            int id = withware[ i ];
            G.Inventory.ItemList[ id ].Count = G.Inventory.ItemList[ id ].AuxCount;
            G.Inventory.ItemList[ id ].MaxStack = G.Inventory.ItemList[ id ].AuxMaxStack;  
        }

        if( saveInventory )
        {
            G.Inventory.Save();                                                                                              // Saves Inventory if needed
        }
    }

   
    public static void GatherGroundObjects()
    {
        if( G.Farm.CarryingAmount > 0 )                                                                                       // Carried Items
        if( G.Farm.SelectedItem != ItemType.NONE )
        {
            int res = Building.AddItem( true, G.Farm.SelectedItem, G.Farm.CarryingAmount );
            Item.AddItem( G.Farm.SelectedItem, res );
        }

        List<Vector2> comb = new List<Vector2>();
        for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                                                          
        {
            int x = G.Farm.Tl[ tid ].x;
            int y = G.Farm.Tl[ tid ].y;                                                                                       // Loop Through all Ground intems in the MAP and send to inventory
            var ga2 = Map.I.Gaia2[ x, y ];


            if( ga2 )
            if( ga2.TileID == ETileType.ITEM )
                {
                    if( ga2.Variation == ( int ) ItemType.WoodAxe ||                                                          // axes have limited production cap, so it must be destroyed
                        ga2.Variation == ( int ) ItemType.Egg ||                                                              // eggs: prevent cheating
                        ga2.Variation == ( int ) ItemType.Feather )                                                           // remove feathers
                        ga2.Kill();
                    else
                    if( ga2.Variation == ( int ) ItemType.HoneyComb )
                        comb.Add( ga2.Pos );
                    else
                    if( Item.IsPlagueMonster( ga2.Variation ) == false )
                    {
                        ItemType it = ( ItemType ) ga2.Variation;
                        int res = Building.AddItem( true, it, ga2.Body.StackAmount );
                        G.GIT( it ).Count += res;
                        ga2.Kill();
                    }
                }
        }
                                              
        Util.RandomizeList( ref comb );                                                                                     // Remove combs if at full level
        if( G.Farm.MaxHoneycombsReached )
        for( int i = 0; i < 4; i++ )
        if( comb.Count > 0 )
        {
            int id = comb.Count - 1;
            Vector2 pt = comb[ id ];
            Map.I.Gaia2[ ( int ) pt.x, ( int ) pt.y ].Kill();
            comb.RemoveAt( id );
            Item.AddItem( ItemType.HoneyComb, -2 );
        }
    }
    public static void SendItemsToWarehouses()
    {
        float[] bldItemCount = new float[ G.Inventory.ItemList.Count ];

        for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                                             // First: Counts items in Buildings           
        {
            int x = G.Farm.Tl[ tid ].x;
            int y = G.Farm.Tl[ tid ].y;
            {
                if( Map.I.Gaia2[ x, y ] )
                {
                    Unit bl = Map.I.Gaia2[ x, y ];
                    if( bl.TileID == ETileType.BUILDING )
                    for( int i = 0; i < bl.Building.Itm.Count; i++ )
                    {
                        ItemType id = bl.Building.Itm[ i ].ItemType;
                        if( id != ItemType.NONE )
                        if( bl.Building.Category != EBuildingCategory.Plant )
                            bldItemCount[ ( int ) id ] += bl.Building.Itm[ i ].ItemCount;
                    }
                }
            }
        }

        for( int i = 0; i < G.Inventory.ItemList.Count; i++ )                                                      // Checks Difference Between Buildings and Inventory Amounts
        if( G.Inventory.ItemList[ i ] )                                                                            // Warning: Dont use G.GIT here: the access is directly to the itemlist via loop and id    
        {
            bldItemCount[ i ] = G.Inventory.ItemList[ i ].Count - bldItemCount[ i ];
        }
        
        for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                                           // Last: Distribute items in Buildings         
        {
            int x = G.Farm.Tl[ tid ].x;
            int y = G.Farm.Tl[ tid ].y;
            {
                if( Map.I.Gaia2[ x, y ] )
                {
                    Unit bl = Map.I.Gaia2[ x, y ];
                    if( bl.TileID == ETileType.BUILDING )
                    if( bl.Building.Category != EBuildingCategory.Plant )
                    {
                        for( int i = 0; i < bl.Building.Itm.Count; i++ )
                        {
                            BuildingItem it = bl.Building.Itm[ i ];
                            ItemType id = it.ItemType;
                            float max = Building.GetStat( EVarType.Maximum_Item_Stack, bl.Building, i );           // calculates BuildingMaxStackList
                            float lack = max - it.ItemCount;
                            if( it.BaseMaxItemStack <= 0 )
                                lack = G.Inventory.ItemList[ ( int ) id ].Count;

                            if( id != ItemType.NONE )
                            {
                                if( lack > bldItemCount[ ( int ) id ] )
                                    lack = bldItemCount[ ( int ) id ];
                                it.ItemCount += lack;
                                bldItemCount[ ( int ) id ] -= lack;
                                G.GIT( id ).Count -= lack;
                            }
                        }
                    }
                }
            }
        }
        //for( int i = 0; i < G.Inventory.ItemList.Count; i++ )                                                // Displays MessagesFor Dumped Items
        //{
        //    if( count[ i ] > 0 )
        //    {
        //        Message.CreateMessage( ETileType.NONE, "No Storage for " +
        //        G.Inventory.ItemList[ i ].name + " \nDumping: " + count[ i ] + " Units!",
        //        Map.I.Hero.Pos + new Vector2( Random.Range( -2, 2 ), Random.Range( -2, 2 ) ), 
        //        new Color( 0, 1, 0, 1 ), true, true, 10 );
        //    }
        //}
    }
    
    public static void UpdateAllBuildings()
    {
        double time = G.Farm.IdleEngine.OffSeconds;
        Item.SetAmt( ItemType.Recipe_Image_1, 0 );
        Recipe.SelectedBuilding = null;
        Map.I.Farm.RecipePanel.gameObject.SetActive( false );

        for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                                           // Last: Distribute items in Buildings         
        {
            int x = G.Farm.Tl[ tid ].x;
            int y = G.Farm.Tl[ tid ].y;
            {
                if( Map.I.Gaia2[ x, y ] )
                if( Map.I.Gaia2[ x, y ].TileID == ETileType.BUILDING )
                {
                    if( Map.I.Gaia2[ x, y ] ) Map.I.Gaia2[ x, y ].Building.UpdateIt( time );
                }
            }
        }

        if( time > 0 )
            Message.GreenMessage( "Farm Idle Time: " + Util.ToSTime( time ) );
        G.Farm.IdleEngine.OffSeconds = 0;
    }

    public void UpdateIt( double time )
    {
        UpdateProduction( time );
        UpdateRecipe( time );
        UpdateTool( time );
        UpdatePlants( time );
        UpdateAnimation();
        UpdateText();
    }

    public void UpdateAnimation()
    {
        if( Map.I.BumpTarget.x != -1 )
        if( Unit.Pos == Map.I.BumpTarget )
        {
            if( ++SelItemID >= Itm.Count ) SelItemID = 0;
            Message.CreateMessage( ETileType.NONE, Itm[ SelItemID ].ItemType,
            Item.GetName( Itm[ SelItemID ].ItemType ), G.Hero.Pos, Color.white );
            Map.I.BumpTarget = new Vector2( -1, -1 );
        }

        if( SelItemSprite )                                                                    // Selected item sprite update
        {
            SelItemSprite.gameObject.SetActive( false );
            if( Itm.Count > 1 )
            {
                SelItemSprite.gameObject.SetActive( true );
                int id = ( int ) Itm[ SelItemID ].ItemType;
                SelItemSprite.spriteId = ( int ) G.GIT( id ).TKSprite.spriteId;
            }
        }
    }

    public static float GetUseSum( EVarType var, BuildingType bl, ItemType it )
    {
        float sum  = 0;
        for( int b = 0; b < G.Farm.BluePrintList.Count; b++ )
        if ( G.Farm.BluePrintList[ b ].AffectedVariable == var )
        if ( G.Farm.BluePrintList[ b ].AffectedBuilding == bl )
        if ( it != ItemType.NONE )
        if ( G.Farm.BluePrintList[ b ].AffectedItem == it )
        for( int u = 0; u < G.Farm.BluePrintList[ b ].UsesList.Count; u++ )
        {
            sum += G.Farm.BluePrintList[ b ].UsesList[ u ];
            sum += G.Farm.BluePrintList[ b ].GetBaseAmount( u );
        }
        return sum;
    }

    public static float GetStat( EVarType var, Building bl, int selitem )
    {
        Building listb = G.Farm.BuildingList[ ( int ) bl.Type ];                             // listb uses building from the master list data
        BuildingItem bi = listb.Itm[ selitem ];

        switch( var )
        {
            case EVarType.Maximum_Item_Stack:                                                 // max stack
            if( bi.BaseMaxItemStack <= 0 ) return -1;
            return bi.BaseMaxItemStack + GetUseSum( var, bl.Type, bi.ItemType );

            case EVarType.Total_Building_Production_Time:

            if( bl.CustomProductionTime != -1 ) return bl.CustomProductionTime;               // Custom porduction time for each of the building stances (like wood cutter) each one has a different value

            // building production time
            return bi.BaseTotalProductionTime + GetUseSum( var, bl.Type, bi.ItemType );
        }
        Debug.LogError( "Bad GetStat() Var" );
        return -1;
    }
    public static float GetForestChopTime()                                                // Wood axe special case: Total time acording to Farm.WoodAxeChopTime
    {
        float val = -1;
        int axes = GetWorkAreaCount( ItemType.WoodAxe );

        if( axes >= 0 )
        {
            if( axes < G.Farm.WoodAxeChopTime.Count )                                      // table value
                val = G.Farm.WoodAxeChopTime[ axes ] * 3600f;
            else
            {
                float last = G.Farm.WoodAxeChopTime[ G.Farm.WoodAxeChopTime.Count - 1 ];   // over table = last + 1
                int extra = axes - ( G.Farm.WoodAxeChopTime.Count - 1 );
                val = ( last + extra ) * 3600f;
            }
        }
        return val;
    }

    public static int GetWorkAreaCount( ItemType tool )
    {
        int axes = 0;
        for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                              
        {
            Unit bltest = Map.I.GetUnit( ETileType.BUILDING, G.Farm.Tl[ tid ] );
            if( bltest )
            {
                if( bltest.Building.Type == BuildingType.Work_Area )
                    if( bltest.Building.ToolType == tool ) axes++;                             // tool found
            }
        }
        return axes;
    }

    public static bool ApplyUpgrade( Vector2 tg, Blueprint bp )
    {
        if( bp.AffectedVariable == EVarType.NONE ) return false;
        if( Map.PtOnMap( Map.I.Tilemap, tg ) == false ) return false;

        Unit bl = Map.I.Gaia2[ ( int ) tg.x, ( int ) tg.y ];

        float power = Blueprint.GetStat( EVarType.BluePrint_Effect_Amount, bp, 1 );
        Blueprint.UpgradePower = Random.Range( 0, bp.AffectedVariableBonusAmount );
        power += Blueprint.UpgradePower; 

        if( bl && bl.TileID == ETileType.BUILDING )
            {
                BuildingItem it = null;
                for( int i = 0; i < bl.Building.Itm.Count; i++ )
                    if( bp.AffectedItem == bl.Building.Itm[ i ].ItemType )
                        it = bl.Building.Itm[ i ];


                string pow = power.ToString( "+#;-#;0" ); 
                if( it )
                switch( bp.AffectedVariable )
                {
                    case EVarType.Total_Building_Production_Time:
                    it.TotalProductionTime += power;
                    if( it.TotalProductionTime < 0 ) 
                        it.TotalProductionTime = 1;
                    pow = Util.ToSTime( power );
                    break;
                    case EVarType.Item_Count:

                    it.ItemCount += power;

                    break;

                    case EVarType.Maximum_Item_Stack:

                    it.MaxItemStack += power;

                    break;

                    case EVarType.Carry_Capacity:

                    G.GIT( it.ItemType ).CarryCapacity += power;

                    break;

                    case EVarType.Road_Carry_Capacity:

                    G.GIT( it.ItemType ).RoadCarryCapacity += power;

                    break;
                }

                string nm = Util.GetName( bp.AffectedVariable.ToString() );

                Message.GreenMessage( "Building Upgraded!\n" + nm + "\nAmount: " + pow );
                MasterAudio.PlaySound3DAtVector3( "Build Complete", G.Hero.transform.position );
                return true;
            }
        return false;
    }
    public string GetName()
    {
        return Type.ToString().Replace( '_', ' ' );
    }

    public static void UpdateMoving( Vector2 from, Vector2 to )
    {
        if( Manager.I.GameType != EGameType.FARM ) return;
        Unit un = Map.I.GetUnit( ETileType.BUILDING, to );
        if( un == null ) return;
        if( un.Building.Category == EBuildingCategory.Work_Area ) return;
        if( un.Building.Category == EBuildingCategory.Plant ) return;

        Building listb = G.Farm.BuildingList[ ( int ) un.Building.Type ];

        if( listb.MovingWheelBarrowCost < 0 ) return;                                                            // No move allowed for negatives
           
        Vector2 tg = to;
        Unit tgit = null;

        if( listb.MovingWheelBarrowCost > 0 )                                                                    // Check Moving Requirements: # of WheelBarrows at target
            {
                for( int i = 0; i < 8; i++ )
                {
                    Vector2 htg = to + Manager.I.U.DirCord[ i ];
                    tgit = Map.I.GetUnit( ETileType.ITEM, htg );

                    if( tgit && tgit.TileID == ETileType.ITEM &&
                        tgit.Variation == ( int ) ItemType.WheelBarrow &&
                        tgit.Body.StackAmount >= listb.MovingWheelBarrowCost )
                    {
                        tg = htg;
                        break;
                    }
                    else if( i >= 7 ) return;
                }
            }

        if( tgit ) tgit.Kill();                                                                                  // Destroy Wheelbarrow Stack
                                                     

        bool res = Place( false, tg, un.Building.Type, un.Building.ToolType, true );

        if( res )
        {
            un.Control.ApplyMove( un.Pos, tg );                                                                 // Move Building
            Manager.I.Tutorial.BuildingMoved = true;
            Map.I.CreateExplosionFX( tg );                                                                      // Explosion FX
            MasterAudio.PlaySound3DAtVector3( "Mechanical Sound", G.Hero.Pos );
        }
    }

       public int GetRecipesAvailable( int id )
       {
           return Map.I.Farm.BuildingList[ ( int ) Type ].RecipeList[ id ].RecipesAvailable;
       }
       public void SetRecipesAvailable( int id, int val )
       {
           Map.I.Farm.BuildingList[ ( int ) Type ].RecipeList[ id ].RecipesAvailable = val;
       }

       public static float GetItemAmount( ItemType type )
       {
           if( G.GIT( type ).HasWarehouse() == false )                                             // Item has no warehouse
               return Item.GetNum( type );

           UpdateBuildingList();
           List<BuildingItem> il = GetBuildingItemList( type );
           float count = 0;
           for( int i = 0; i < il.Count; i++ )
               count += il[ i ].ItemCount;
           return count;
       }

       public static void UpdateBuildingList()
       {       
           if ( Map.I.Gaia == null ) return;
           if( Bl != null && Bl.Count > 0 ) return;
           Bl = new List<Unit>();

           for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                                           // Last: Distribute items in Buildings         
           {
               int x = G.Farm.Tl[ tid ].x;
               int y = G.Farm.Tl[ tid ].y;
               {
                   if( Map.I.Gaia2[ x, y ] )
                   if( Map.I.Gaia2[ x, y ].TileID == ETileType.BUILDING )
                   if( Map.I.Gaia2[ x, y ].Building.Category != EBuildingCategory.Plant )
                   {
                       Bl.Add( Map.I.Gaia2[ x, y ] );
                   }
               }
           }
       }

       public static List<BuildingItem> GetBuildingItemList( ItemType type )
       {
           List<BuildingItem> il = new List<BuildingItem>();
           for( int b = 0; b < Bl.Count; b++ )
           for( int it = 0; it < Bl[ b ].Building.Itm.Count; it++ )
           if ( Bl[ b ].Building.Itm[ it ].ItemType == type )
           {
                il.Add( Bl[ b ].Building.Itm[ it ] );
                Bl[ b ].Building.Itm[ it ].Building = Bl[ b ].Building;
                Bl[ b ].Building.Itm[ it ].ID = it;
           }
           return il;
       }

       public static int GetBuildingItemID( ItemType type, Building bl )
       {
           for( int it = 0; it < bl.Itm.Count; it++ )
           if ( bl.Itm[ it ].ItemType == type )
               {
                   return it;
               }
           return -1;
       }

       public static int AddItem( bool apply, ItemType type, float amt )
       {
           UpdateBuildingList();

           float avail = GetItemAmount( type );

           if( G.GIT( type ).HasWarehouse() == false ||
               G.GIT( type ).WarehouseBuiltCount == 0 )                                     // Item has no warehouse or has not been built yet
           {
               avail = Item.GetNum( type );
               if( amt < 0 && avail < Util.Mod( amt ) ) return 0;
               Item.AddItem( Inventory.IType.Inventory, type, amt );
               return 0;
           }

           if( amt < 0 && avail < Util.Mod( amt ) ) return 0;                               // not enough item in buildings

           if( apply == false )                                                             // Just checking if there´s items available           
               return 0;          

           int given = 0;
           int un = 1;
           if( amt < 0 ) un = -1;
           List<BuildingItem> il = GetBuildingItemList( type );
           for( int done = 0; done < Util.Mod( amt ); done++ )                              // distrubutes item among warehouses
           {
               for( int i = 0; i < il.Count; i++ )
               {
                   float mstack = Building.GetStat( EVarType.Maximum_Item_Stack, 
                   il[ i ].Building, il[ i ].ID );
                   float res = il[ i ].ItemCount + un;
                   if( mstack == -1 || ( res >= 0 && res <= mstack ) )
                   {
                       il[ i ].ItemCount += un;
                       if( ++given == Util.Mod( amt ) ) goto outloop;
                   }
               }
           }
           outloop:
           for( int i = 0; i < il.Count; i++ )
           {
               float mstack = Building.GetStat( EVarType.Maximum_Item_Stack, 
                   il[ i ].Building, il[ i ].ID );
               if( mstack != -1 )
               {
                  if( il[ i ].ItemCount > mstack )
                  {
                      given -= ( int ) il[ i ].ItemCount - ( int ) mstack;
                      il[ i ].ItemCount = mstack;
                  }
               }
           }
           return given;
       }

    public static ItemType GetCurrentBuildingItem( Vector2 tg )
       {
           Unit bl = Map.I.GetUnit( tg, ELayerType.GAIA2 );
           if( bl == null ) return ItemType.NONE;
           int id = bl.Building.SelItemID;
           if( Util.VID( bl.Building.Itm, id ) )
               return bl.Building.Itm[ id ].ItemType;
           return ItemType.NONE;
       }
}
