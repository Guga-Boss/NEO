using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using System.Text.RegularExpressions;
using System.Text;
using Sirenix.OdinInspector;
using System.IO;

public enum EBPIconType
{
    NONE = -1,
    Forest = 0, Water,
    Bld_Stone_Storage,
    Bld_Wood_Storage
}

public class Blueprint : MonoBehaviour
{
    #region Variables
    [TabGroup( "Main" )]
    public int ID = 0;
    [TabGroup( "Main" )]
    [ReadOnly()]
    public string UniqueID = "";          // Unique ID for Saving
    [TabGroup( "Main" )]
    public float FreePlants = 0;
    [TabGroup( "Cost" )]
    public ItemType CostItem = ItemType.Energy;
    [TabGroup( "Cost" )]
    public float InitialCost = 0;
    [TabGroup( "Cost" )]
    public float Cost = 0;
    [TabGroup( "Cost" )]
    public float CostFluctuation = 10;
    [TabGroup( "Cost" )]
    public float MinCostFluctuation = 0;
    [TabGroup( "Cost" )]
    public bool RandomCostCalculation = true;
    [TabGroup( "Cost" )]
    public bool PercentCostCalculation = true;
    [TabGroup( "Cost" )]                                      // Amount of resource cost increase (CostItem) after each use
    public float PenaltyPerUse = 0;
    [Header( "Auto Apply" )]
    [TabGroup( "Cost" )]
    public ItemType AutoApplyCostItem = ItemType.NONE;
    [TabGroup( "Cost" )]
    public float AutoApplyCost = 0;
    [Header( "Buy Blueprint" )]
    [TabGroup( "Cost" )]
    public ItemType BuyBPCostItem = ItemType.NONE;
    [TabGroup( "Cost" )]
    public float BuyBPCost = 0;
    [Header( "Cost Limits" )]
    [TabGroup( "Cost" )]
    public float MaxCost = 100;
    [TabGroup( "Cost" )]
    public float MinCost = 0;
    [TabGroup( "Chance" )]
    public float BaseSuccessRate = 100;
    [TabGroup( "Chance" )]
    public float SuccessRate = 0;
    [TabGroup( "Chance" )]
    public float SuccessFluctuation = 0;
    [TabGroup( "Chance" )]
    public float MinSuccessFluctuation = 0;
    [TabGroup( "Chance" )]
    public bool RandomSuccessCalculation = true;
    [TabGroup( "Chance" )]
    public bool PercentSuccessCalculation = true;
    [TabGroup( "Main" )]
    public List<int> UsesList;                                    // Useslist only saves the random bonus sorted. this way base values can be changed after releasing
    [TabGroup( "Main" )]
    public float MaxUses = 1;
    [TabGroup( "Main" )]
    public float AvailableUses = 0;
    [TabGroup( "Main" )]
    public float BaseSortChanceAfterUse = 100;
    [TabGroup( "Main" )]
    public float SortChanceAfterUse = 0;
    [TabGroup( "Effect" )]
    public BuildingType GeneratedBuilding = BuildingType.NONE;
    [TabGroup( "Effect" )]
    public ItemType GeneratedItem = ItemType.NONE;
    [TabGroup( "Effect" )]
    public int GeneratedItemAmount = 1;
    [TabGroup( "Effect" )]
    public BuildingType AffectedBuilding = BuildingType.NONE;
    [TabGroup( "Effect" )]
    public ItemType AffectedItem = ItemType.NONE;
    [TabGroup( "Effect" )]
    public Blueprint AffectedBluePrint = null;
    [TabGroup( "Effect" )]
    public EVarType AffectedVariable = EVarType.NONE;
    [TabGroup( "Effect" )]
    public float AffectedVariableBaseAmount = 0;
    [TabGroup( "Effect" )]
    public List<float> AffectedVariableBaseAmountList = null;
    [TabGroup( "Effect" )]
    public float AffectedVariableAmount = 0;
    [TabGroup( "Effect" )]
    public float AffectedVariableBonusAmount = 0;                   // Saved in useslist
    [TabGroup( "Effect" )]
    public float BaseAffectedBuildingItemsRequired = 0;
    [TabGroup( "Effect" )]
    public int AffectedBuildingItemsRequired = 0;
    [TabGroup( "Effect" )]
    public float AffectedBuildingItemsRequiredInflation = 0;
    [TabGroup( "Matrix" )]
    [Tooltip( "Matrix Max spread" )]
    public Vector2 Size = new Vector2( 3, 3 );
    [TabGroup( "Matrix" )]
    [InfoBox( "This Defines Max Stack Height. Set 0 to Unlimited. " )]
    public int MaxStack = 0;
    [InfoBox( "This Defines Max tiles. Set 0 to Unlimited. " )]
    [TabGroup( "Matrix" )]
    public int MaxTiles = 0;
    [TabGroup( "Matrix" )]
    public float SortBoothBlueprintsChance = 0;
    [TabGroup( "Matrix" )]
    public ItemType[,] ItemMatrix;
    [TabGroup( "Matrix" )]
    public int[,] ItemAmount;
    [TabGroup( "Matrix" )]
    public List<ItemType> ItemList;
    [TabGroup( "Matrix" )]
    public EBPIconType BPCustomIconType1 = EBPIconType.NONE;
    [TabGroup( "Matrix" )]
    public EBPIconType BPCustomIconType2 =  EBPIconType.NONE;

    public static Vector2 LastPlacedPos;
    public static Blueprint SelectedBluePrint;
    public static int AvailableBluePrints = 0;
    public static bool BluePrintsLoaded = false;
    public static bool CheckBluePrintPatters = false;
    public static float UpgradePower = 0;
    public static Unit AutoApplyBuilding = null;
    #endregion

    public void Copy( Blueprint bp )
    {
        FreePlants = bp.FreePlants;
        GeneratedBuilding = bp.GeneratedBuilding;
        GeneratedItem = bp.GeneratedItem;
        GeneratedItemAmount = bp.GeneratedItemAmount;
        Size = bp.Size;
        InitialCost = bp.InitialCost;
        Cost = bp.Cost;
        CostItem = bp.CostItem;
        AutoApplyCost = bp.AutoApplyCost;
        AutoApplyCostItem = bp.AutoApplyCostItem;
        SuccessRate = bp.SuccessRate;
        BaseSuccessRate = bp.BaseSuccessRate;
        MaxStack = bp.MaxStack;
        MaxTiles = bp.MaxTiles;
        AffectedBluePrint = bp.AffectedBluePrint;
        AffectedVariable = bp.AffectedVariable;
        AffectedVariableBaseAmount = bp.AffectedVariableBaseAmount;
        AffectedVariableAmount = bp.AffectedVariableAmount;
        AffectedVariableBonusAmount = bp.AffectedVariableBonusAmount;
        AffectedVariableBaseAmountList = new List<float>();
        AffectedVariableBaseAmountList.AddRange( bp.AffectedVariableBaseAmountList );
        AffectedBuilding = bp.AffectedBuilding;
        AffectedItem = bp.AffectedItem;
        AffectedBuildingItemsRequired = bp.AffectedBuildingItemsRequired;
        BaseAffectedBuildingItemsRequired = bp.BaseAffectedBuildingItemsRequired;
        AffectedBuildingItemsRequiredInflation = bp.AffectedBuildingItemsRequiredInflation;
        AvailableUses = bp.AvailableUses;
        CostFluctuation = bp.CostFluctuation;
        MinCostFluctuation = bp.MinCostFluctuation;
        PercentCostCalculation = bp.PercentCostCalculation;
        RandomCostCalculation = bp.RandomCostCalculation;
        SuccessFluctuation = bp.SuccessFluctuation;
        MinSuccessFluctuation = bp.MinSuccessFluctuation;
        PercentSuccessCalculation = bp.PercentSuccessCalculation;
        RandomSuccessCalculation = bp.RandomSuccessCalculation;
        BPCustomIconType1 = bp.BPCustomIconType1;
        BPCustomIconType2 = bp.BPCustomIconType2;
        MaxUses = bp.MaxUses;
        SortChanceAfterUse = bp.SortChanceAfterUse;
        ItemMatrix = new ItemType[ 5, 5 ];
        ItemMatrix = ( ItemType[ , ] ) bp.ItemMatrix.Clone();
        ItemAmount = new int[ 5, 5 ]; 
        ItemAmount = ( int[ , ] ) bp.ItemAmount.Clone();
        ItemList = new List<ItemType>( bp.ItemList );
        UsesList = new List<int>( bp.UsesList );
    }
    public static void UpdateIt()
    {
        if( BluePrintWindow.I.gameObject.activeSelf == false )                                               // Free Plants Check
        {
            int count = 0;
            for( int i = 0; i < G.Farm.BluePrintList.Count; i++ )
            {
                if( G.Farm.BluePrintList[ i ].FreePlants >= 1 )
                {
                    if( G.Farm.BluePrintList[ i ].ItemMatrix == null )                                       // Not yet sorted, so Sort it
                    {
                        G.Farm.BluePrintList[ i ].Sort();
                        G.Farm.BluePrintList[ i ].FreePlants--;
                        Item.AddItem( Inventory.IType.Inventory,
                                      ItemType.BluePrint_Image_1, -1 );
                        //G.Farm.BluePrintList[ i ].Save();
                    }
                    count++;
                }
            }

            if( count == 0 && 
                Item.GetNum( Inventory.IType.Inventory, ItemType.BluePrint_Image_1 ) != 0 )                  // Clear Bp icons
            {
                Item.AddItem( Inventory.IType.Inventory, ItemType.BluePrint_Image_1, 
                -G.GIT( ItemType.BluePrint_Image_1 ).Count );
            }

            G.Farm.FreePlantButtonTxt.transform.parent.gameObject.SetActive( false );                        // Update Choose Plant Button

            if( SelectedBluePrint.FreePlants >= 1 )
            {
                int num = ( int ) SelectedBluePrint.FreePlants;
                if( num > 99 ) num = 99;
                G.Farm.FreePlantButtonTxt.transform.parent.gameObject.SetActive( true );
                G.Farm.FreePlantButtonTxt.text = "+" + num;
            }
        }
        BluePrintWindow.UpdateIt();
        UpdateBluePrintInput();

        Blueprint bp = SelectedBluePrint;
        if( bp )
            bp.CalculateBuildingItemsRequired();
    }
    public void CalculateBuildingItemsRequired()
    {
        AffectedBuildingItemsRequired = ( int ) BaseAffectedBuildingItemsRequired;
        if( UsesList != null && UsesList.Count > 0 )
            AffectedBuildingItemsRequired += ( int ) ( AffectedBuildingItemsRequiredInflation * UsesList.Count );    // Building Item Requirement Calculation
    }
        
    public static void GetNextBluePrint( bool up = true )
    {
        if( BluePrintWindow.I.gameObject.activeSelf ) return;
        if( SelectedBluePrint == null ) 
            SelectedBluePrint = G.Farm.BluePrintList[ 0 ];
        AutoApplyBuilding = null;
        int sel = SelectedBluePrint.ID;
        int lim = G.Farm.BluePrintList.Count + 5;
        for( int i = 0; i < lim; i++ )
        {
            if(  up && ++sel == Map.I.Farm.BluePrintList.Count ) sel = 0;
            if( !up && --sel == -1 ) sel = Map.I.Farm.BluePrintList.Count - 1;

            if( Map.I.Farm.BluePrintList[ sel ].IsAvailable() == false )
            {
                if( i > G.Farm.BluePrintList.Count )
                {
                    string text = "No Building Specific BluePrint Found!";
                    UI.I.SetBigMessage( text, Color.red, .1f, 4.7f, 122.8f, 55 );
                    return;
                }
                continue;
            }
            else
            {
                G.Tutorial.ProgressPhase( 6 );
                SelectedBluePrint = Map.I.Farm.BluePrintList[ sel ]; break;
            }
        }            
    }

    public static void UpdateBluePrintInput()
    {
        if( BluePrintWindow.I.gameObject.activeSelf ) return;
        List<Blueprint> bpl = new List<Blueprint>();
        List<int> bpid = new List<int>();
        AvailableBluePrints = 0;
        for( int i = 0; i < G.Farm.BluePrintList.Count; i++ )
        {
            if( Map.I.Farm.BluePrintList[ i ].IsAvailable() )
            {
                bpl.Add( G.Farm.BluePrintList[ i ] );
                AvailableBluePrints++;
            }
            bpid.Add( AvailableBluePrints );
        }

        int sel = SelectedBluePrint.ID;
        //if( Map.I.Farm.BluePrintList[ sel ].MaxUses > 0 )
        //if( Map.I.Farm.BluePrintList[ sel ].UsesList.Count >=
        //    Map.I.Farm.BluePrintList[ sel ].MaxUses )
        //{
        //    sel = 0;
        //    SelectedBluePrint = Map.I.Farm.BluePrintList[ sel ];
        //}

        Vector2 tg = Map.I.Hero.GetFront();                                              // Selected building related BP
        Unit bl = Map.I.GetUnit( ETileType.BUILDING, tg );
        if( bl == null && AvailableBluePrints > 0 )
        if( Map.I.Farm.BluePrintList[ sel ].IsAvailable() == false )
            GetNextBluePrint(); 

        if( Input.GetKeyUp( KeyCode.PageUp   ) ) GetNextBluePrint( true  );
        if( Input.GetKeyUp( KeyCode.PageDown ) ) GetNextBluePrint( false );         

        BluePrintWindow.I.FarmPanel.BluePrintLabel.text =
        SelectedBluePrint.name + " (" + ( bpid[ sel ] ) + "/" + bpl.Count + ")";
        G.GIT( ItemType.BluePrint_Image_1 ).Count = bpl.Count;                            // Set the amount of Blueprint items as the amount of Blueprints Available
    }
    public bool IsAvailable()
    {
        //Debug.Log( this.name + "  " + MaxUses + " " + UsesList.Count );
        if( AvailableUses < 1 ) return false;
        if( MaxUses > 0 )
        if( UsesList.Count >= MaxUses ) return false;
        if( ItemMatrix == null ) return false;

        //Vector2 tg = Map.I.Hero.GetFront();                                              //Excusive to Selected building related BP 
        //Unit bl = Map.I.GetUnit( ETileType.BUILDING, tg );
        //if( bl )
        //{
        //    if( AffectedBuilding == BuildingType.NONE ) return false;
        //    if( GeneratedBuilding != BuildingType.NONE ) return false;
        //    if( bl.Building.Type != AffectedBuilding ) return false;
        //}

        return true;
    }
    public static void UpdateSorting()
    {
        //for( int i = 0; i < G.Farm.BluePrintList.Count; i++ )
        //{
        //    bool res = G.Farm.BluePrintList[ i ].Load();
        //    if( res == false )                  
        //        G.Farm.BluePrintList[ i ].Sort();
        //}
    }
    public static void SaveAll()
    {
        if( !Manager.I.SaveOnEndGame ) return;
        if( !G.Tutorial.CanSave() ) return;

        TF.ActivateFieldList( "Blueprint" );                                                 // Activates Blueprint Tagged Field List

        string file = Manager.I.GetProfileFolder() + "Blueprint.NEO";                        // Provides File name

        using( MemoryStream ms = new MemoryStream() )
        using( BinaryWriter writer = new BinaryWriter( ms ) )                                // Open Memory Stream
        {
            GS.W = writer;                                                                   // Assign BinaryWriter to GS.W for TF

            int Version = Security.SaveHeader( 1 );                                          // Save Header Defining Current Save Version

            List<string> bpIDs = new List<string>();                                         // Salva a lista de IDs únicos primeiro
            for( int i = 0; i < G.Farm.BluePrintList.Count; i++ )
            {
                if( Helper.I.ReleaseVersion == false )
                    if( G.Farm.BluePrintList[ i ].UniqueID.Length != 5 )
                        Debug.LogError( "Empty UID" );                                             // Check empty unique ID

                bpIDs.Add( G.Farm.BluePrintList[ i ].UniqueID );
            }

            // Save bpIDs manually ; avoid StringList
            TF.SaveT( "BP_IDs_Size", bpIDs.Count );                                           // Save count ; Unity 5.6 safe
            for( int i = 0; i < bpIDs.Count; i++ )
                TF.SaveT( "BP_ID_" + i, bpIDs[ i ] );                                         // Save each element individually

            // Para cada blueprint, salva usando o ID único como chave
            for( int i = 0; i < G.Farm.BluePrintList.Count; i++ )
            {
                Blueprint bp = G.Farm.BluePrintList[ i ];
                string bpID = bp.UniqueID;

                TF.SaveT( "BP_" + bpID + "_FreePlants", bp.FreePlants );                     // Save Free Plants
                TF.SaveT( "BP_" + bpID + "_AvailableUses", bp.AvailableUses );               // Save Available uses
                TF.SaveT( "BP_" + bpID + "_Cost", bp.Cost );                                 // Save cost
                TF.SaveT( "BP_" + bpID + "_SuccessRate", bp.SuccessRate );                   // Save Success rate
                TF.SaveT( "BP_" + bpID + "_SortChanceAfterUse", bp.SortChanceAfterUse );     // Save Sort Chance after use
                TF.SaveT( "BP_" + bpID + "_AffectedVariableAmount", bp.AffectedVariableAmount ); // Save Affected variable amount

                TF.SaveT( "BP_" + bpID + "_UsesListSize", bp.UsesList.Count );               // Save uses array size
                for( int u = 0; u < bp.UsesList.Count; u++ )                                  // Save uses list manually ; Unity 5.6 safe
                    TF.SaveT( "BP_" + bpID + "_Use_" + u, bp.UsesList[ u ] );

                bool mat = bp.ItemMatrix != null;
                TF.SaveT( "BP_" + bpID + "_MatrixExists", mat );                             // Save if matrix exists
                if( mat )
                {
                    int mx = bp.ItemMatrix.GetLength( 0 );
                    int my = bp.ItemMatrix.GetLength( 1 );
                    TF.SaveT( "BP_" + bpID + "_MatrixWidth", mx );                           // Save matrix width
                    TF.SaveT( "BP_" + bpID + "_MatrixHeight", my );                          // Save matrix height

                    for( int y = 0; y < my; y++ )
                        for( int x = 0; x < mx; x++ )
                        {
                            TF.SaveT( "BP_" + bpID + "_MatrixItem_" + ( y * mx + x ), ( int ) bp.ItemMatrix[ x, y ] ); // Save item type
                            TF.SaveT( "BP_" + bpID + "_MatrixAmount_" + ( y * mx + x ), bp.ItemAmount[ x, y ] );      // Save item amount
                        }
                }
            }

            List<string> buildingIDs = new List<string>();                                   // Recipes available are used and saved globally here because it needs to be saved after a goal has been conquered
            for( int b = 0; b < G.Farm.BuildingList.Count; b++ )
            {
                if( G.Farm.BuildingList[ b ].UniqueID.Length != 5 )
                    Debug.LogError( "Empty Bl UID" );

                buildingIDs.Add( G.Farm.BuildingList[ b ].UniqueID );                       // UniqueID
            }

            // Save buildingIDs manually ; avoid StringList
            TF.SaveT( "Building_IDs_Size", buildingIDs.Count );                               // Save count ; Unity 5.6 safe
            for( int i = 0; i < buildingIDs.Count; i++ )
                TF.SaveT( "Building_ID_" + i, buildingIDs[ i ] );                             // Save each element individually

            for( int b = 0; b < G.Farm.BuildingList.Count; b++ )
            {
                string buildingID = G.Farm.BuildingList[ b ].UniqueID;
                List<string> recipeIDs = new List<string>();                                   // Salva lista de Recipe IDs para este building
                for( int r = 0; r < G.Farm.BuildingList[ b ].RecipeList.Count; r++ )
                {
                    if( G.Farm.BuildingList[ b ].RecipeList[ r ].UniqueID.Length != 5 )
                        Debug.LogError( "Empty rec UID" );

                    recipeIDs.Add( G.Farm.BuildingList[ b ].RecipeList[ r ].UniqueID );        // Recipe UniqueID
                }

                // Save recipeIDs manually ; avoid StringList
                TF.SaveT( "Building_" + buildingID + "_Recipe_IDs_Size", recipeIDs.Count );    // Save count ; Unity 5.6 safe
                for( int i = 0; i < recipeIDs.Count; i++ )
                    TF.SaveT( "Building_" + buildingID + "_Recipe_ID_" + i, recipeIDs[ i ] ); // Save each element individually

                for( int r = 0; r < G.Farm.BuildingList[ b ].RecipeList.Count; r++ )           // Para cada recipe, salva os dados usando ID único
                {
                    string recipeID = G.Farm.BuildingList[ b ].RecipeList[ r ].UniqueID;
                    TF.SaveT( "Building_" + buildingID + "_Recipe_" + recipeID + "_Available",
                    G.Farm.BuildingList[ b ].RecipeList[ r ].RecipesAvailable );               // Save recipe availability by unique ID
                }
            }

            GS.W.Flush();                                                                      // Flush the writer

            Security.FinalizeSave( ms, file );                                                 // Finalize save
        }                                                                                      // using closes the stream automatically
    }

    public static void LoadAll()
    {
        string file = Manager.I.GetProfileFolder() + "Blueprint.NEO";                        // Provides File name

        byte[] fileData = File.ReadAllBytes( file );                                         // Read full file
        byte[] content = Security.CheckLoad( fileData );                                     // Validate HMAC and get clean content

        using( GS.R = new BinaryReader( new MemoryStream( content ) ) )                      // Use MemoryStream for TF
        {
            int SaveVersion = Security.LoadHeader();                                         // Load Header

            TF.ActivateFieldList( "Blueprint" );                                             // Activates Blueprint Tagged Field List

            // Load bpIDs manually ; Unity 5.6 safe
            int bpCount = TF.LoadT<int>( "BP_IDs_Size" );                                     // Load count
            List<string> bpIDs = new List<string>();
            for( int i = 0; i < bpCount; i++ )
                bpIDs.Add( TF.LoadT<string>( "BP_ID_" + i ) );                                // Load each element

            for( int i = 0; i < bpIDs.Count; i++ )
            {
                string bpID = bpIDs[ i ];
                Blueprint bp = G.Farm.BluePrintList.Find( b => b.UniqueID == bpID );
                if( bp == null )
                {
                    bp = new Blueprint { UniqueID = bpID };
                    G.Farm.BluePrintList.Add( bp );
                }

                bp.FreePlants = TF.LoadT<float>( "BP_" + bpID + "_FreePlants" );                           // Load Free Plants (float)
                bp.AvailableUses = TF.LoadT<float>( "BP_" + bpID + "_AvailableUses" );                     // Load Available uses (int)
                bp.Cost = TF.LoadT<float>( "BP_" + bpID + "_Cost" );                                       // Load cost (float)
                bp.SuccessRate = TF.LoadT<float>( "BP_" + bpID + "_SuccessRate" );                         // Load Success rate (float)
                bp.SortChanceAfterUse = TF.LoadT<float>( "BP_" + bpID + "_SortChanceAfterUse" );           // Load Sort Chance after use (float)
                bp.AffectedVariableAmount = TF.LoadT<float>( "BP_" + bpID + "_AffectedVariableAmount" );   // Load Affected variable amount (float)

                int usesSize = TF.LoadT<int>( "BP_" + bpID + "_UsesListSize" );                            // Load uses array size
                bp.UsesList = new List<int>();
                for( int u = 0; u < usesSize; u++ )                                          // Load uses manually ; Unity 5.6 safe
                    bp.UsesList.Add( TF.LoadT<int>( "BP_" + bpID + "_Use_" + u ) );

                bool matrixExists = TF.LoadT<bool>( "BP_" + bpID + "_MatrixExists" );        // Load if matrix exists
                if( matrixExists )
                {
                    int width = TF.LoadT<int>( "BP_" + bpID + "_MatrixWidth" );              // Load matrix width
                    int height = TF.LoadT<int>( "BP_" + bpID + "_MatrixHeight" );            // Load matrix height

                    bp.ItemMatrix = new ItemType[ width, height ];
                    bp.ItemAmount = new int[ width, height ];

                    for( int y = 0; y < height; y++ )
                        for( int x = 0; x < width; x++ )
                        {
                            int index = y * width + x;
                            bp.ItemMatrix[ x, y ] = ( ItemType ) TF.LoadT<int>( "BP_" + bpID + "_MatrixItem_" + index );   // Restore item type
                            bp.ItemAmount[ x, y ] = TF.LoadT<int>( "BP_" + bpID + "_MatrixAmount_" + index );              // Restore item amount
                        }
                }
            }

            // Load buildingIDs manually ; Unity 5.6 safe
            int buildingCount = TF.LoadT<int>( "Building_IDs_Size" );
            List<string> buildingIDs = new List<string>();
            for( int i = 0; i < buildingCount; i++ )
                buildingIDs.Add( TF.LoadT<string>( "Building_ID_" + i ) );

            for( int b = 0; b < buildingIDs.Count; b++ )
            {
                string buildingID = buildingIDs[ b ];
                Building building = null;
                for( int k = 0; k < G.Farm.BuildingList.Count; k++ )                         // Loop manually to find building
                {
                    if( G.Farm.BuildingList[ k ].UniqueID == buildingID )
                    {
                        building = G.Farm.BuildingList[ k ];
                        break;
                    }
                }

                if( building != null )
                {
                    int recipeCount = TF.LoadT<int>( "Building_" + buildingID + "_Recipe_IDs_Size" ); // Load recipe count
                    List<string> recipeIDs = new List<string>();
                    for( int i = 0; i < recipeCount; i++ )
                        recipeIDs.Add( TF.LoadT<string>( "Building_" + buildingID + "_Recipe_ID_" + i ) ); // Load recipe ID

                    for( int r = 0; r < recipeIDs.Count; r++ )
                    {
                        string recipeID = recipeIDs[ r ];
                        Recipe recipe = null;
                        for( int rr = 0; rr < building.RecipeList.Count; rr++ )              // Loop manually to find recipe ; Unity 5.6 safe
                        {
                            if( building.RecipeList[ rr ].UniqueID == recipeID )
                            {
                                recipe = building.RecipeList[ rr ];
                                break;
                            }
                        }

                        if( recipe != null )
                        {
                            int available = TF.LoadT<int>( "Building_" +
                            buildingID + "_Recipe_" + recipeID + "_Available" );
                            recipe.RecipesAvailable = available;                            // Apply recipe availability
                        }
                    }
                }
            }
        }
    }



    public void Sort( bool onlyMatrix = false )
    {
        if( onlyMatrix == false )
        if( FreePlants <= 0 ) return;

        ItemMatrix = new ItemType[ 5, 5 ];
        ItemAmount = new int[ 5, 5 ]; 
        List<Vector2> tgl = new List<Vector2>(); 

        for( int y = 0; y < 5; y++ )                                                                    // Init Matrix
        for( int x = 0; x < 5; x++ )
            {
                ItemMatrix[ x, y ] = ItemType.NONE;
            }

        for( int y = 0; y < Size.y; y++ )                                                               // Loop through all pos to add targets
        for( int x = 0; x < Size.x; x++ )
        {
            ItemMatrix[ x, y ] = ItemType.NONE;

            int ax = 0, ay = 0;
            if( Size.x == 1 ) ax = 2;
            if( Size.x == 2 ) ax = 1;
            if( Size.x == 3 ) ax = 1;
            if( Size.y == 1 ) ay = 2;
            if( Size.y == 2 ) ay = 1;
            if( Size.y == 3 ) ay = 1;

            tgl.Add( new Vector2( x + ax, y + ay ) );     
        }

        if( ItemList == null ) return;

        CalculateBuildingItemsRequired();

        int tile = 0, trial = 0;
        for( int i = 0; i < ItemList.Count; i++ )                                                               // Loop through all Items recipe list
        {            
            int id = Random.Range( 0, tgl.Count );

            ItemType it = ItemMatrix[ ( int ) tgl[ id ].x, ( int ) tgl[ id ].y ];

            if( it != ItemType.NONE && it != ItemList[ i ] )                                                    // Overlapping different Item: Restart loop
            {
                i--;
            }
            else
            {
                if( it == ItemType.NONE )                                                                       // Place new Item over empty
                {
                    if( tile < MaxTiles || MaxTiles <= 0 )
                    {
                        ItemMatrix[ ( int ) tgl[ id ].x, ( int ) tgl[ id ].y ] = ItemList[ i ];
                        ItemAmount[ ( int ) tgl[ id ].x, ( int ) tgl[ id ].y ]++;

                        if( IsBuilding( ItemList[ i ], BPCustomIconType1 ) ||  // new                           // Building Item Amount Required   
                            IsBuilding( ItemList[ i ], BPCustomIconType2 ) )
                            {
                                ItemAmount[ ( int ) tgl[ id ].x, ( int ) tgl[ id ].y ] = 
                                AffectedBuildingItemsRequired;
                            }
                        tile++;
                    }
                    else i--;   
                }
                else
                    if( it == ItemList[ i ] )                                                                   // same item
                    {
                        int posX = ( int ) tgl[ id ].x;
                        int posY = ( int ) tgl[ id ].y;

                        if( ItemAmount[ posX, posY ] < MaxStack || MaxStack <= 0 )                              // respect cell max
                        {
                            ItemAmount[ posX, posY ]++;
                        }
                        else i--;                                                                               // cannot add more here
                    }

                if( ++trial > 100000 )                                                                          // Error Message
                {
                    if( ItemList.Count >  MaxStack * MaxTiles  )
                        Debug.LogError( "Blueprint Error: MaxStack  " + name );
                    Debug.LogError( "Blueprint Error " + name );
                    break;
                }
            }
        }

        if( onlyMatrix == false ) SortEverything( false );                                                     // Sort Price
    }


    public void SortEverything( bool istemp )
    {
        SortPrice( Cost, istemp, false );
        SortSuccess( SuccessRate, istemp, false );
    }
    
    public float SortPrice( float orCost, bool istemp, bool report )
    {
        if( !report && Cost <= 0 )
        {
            Cost = InitialCost;
            return Cost;
        }

        float cs = orCost;                                                                                      // Cost
        float _cost = 0;
        if( istemp ) cs = SelectedBluePrint.Cost;

        float _minCostFluctuation = CostFluctuation;
        if( MinCostFluctuation != 0 ) _minCostFluctuation = MinCostFluctuation;

        float min = orCost - Util.Percent( _minCostFluctuation, cs );
        float max = orCost + Util.Percent( CostFluctuation, cs );

        if( PercentCostCalculation == false )                                                                   // Absolute cost calculation
        {
            min = orCost - _minCostFluctuation;
            max = orCost + CostFluctuation;
        }

        if( RandomCostCalculation )
            _cost = Random.Range( min, max );
        else
            _cost = orCost + CostFluctuation;
                                                                 
        if( _cost > MaxCost ) _cost = MaxCost;                                         // Cost Bounds
        if( _cost < MinCost ) _cost = MinCost;
        
        if( report == false ) Cost = _cost;

        return _cost;
    }

    public float SortSuccess( float orSucc, bool istemp, bool report )
    {
        float ss = orSucc;                                                                                      // Success
        float _succ = 0;
        if( istemp ) ss = Blueprint.GetStat( EVarType.BluePrint_Success_Rate, SelectedBluePrint );

        float _minSuccessFluctuation = SuccessFluctuation;
        if( MinSuccessFluctuation != 0 ) _minSuccessFluctuation = MinSuccessFluctuation;

        float mins = orSucc - Util.Percent( _minSuccessFluctuation, ss );
        float maxs = orSucc + Util.Percent( SuccessFluctuation, ss );

        if( PercentSuccessCalculation == false )                                                               // Absolute Success calculation
        {
            mins = orSucc - _minSuccessFluctuation;
            maxs = orSucc + SuccessFluctuation;
        }

        if( RandomSuccessCalculation )
            _succ = Random.Range( mins, maxs );
        else
            _succ = orSucc + SuccessFluctuation;

        if( report == false ) SuccessRate = _succ;

        return _succ;
    }


    public static void CheckPatterns()
    {
        if( !G.Tutorial.CheckPatterns() ) return;                                                           // Tutorial stuff

        if( CheckBluePrintPatters == false ) return;

        for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                                    // Loops through all Tiles on the map and checks for patterns        
        {
            int x = G.Farm.Tl[ tid ].x;
            int y = G.Farm.Tl[ tid ].y;
            {
                CheckPattern( new Vector2( x, y ) );
            }
        }

        CheckBluePrintPatters = false;
    }

    public static void CheckPattern( Vector2 tg )
    {
        if( LastPlacedPos == new Vector2( -1, -1 ) ) return;
       ItemType[,] _item = new ItemType[ 5, 5 ];
       EBPIconType[ , ] _custom = new EBPIconType[ 5, 5 ];                                                                     // init vars
       int[,] _amount = new int[ 5, 5 ];
       Blueprint bp = SelectedBluePrint;
       List<Vector2> bll = new List<Vector2>();
       Blueprint.UpgradePower = 0;

       for( int y = 0; y < 5; y++ )                                                                                            // loop
       for( int x = 0; x < 5; x++ )
           {
               _item[ x, y ] = ItemType.NONE;                                                                                  // clears matrices
               _amount[ x, y ] = 0;
               _custom[ x, y ] = EBPIconType.NONE;

               Vector2 pos = tg + new Vector2( x, y );                                                                         // Sweeps the tilemap and fills up item matrix

               if( Map.PtOnMap( Map.I.Tilemap, pos ) )
               {
                   Unit ga2 = Map.I.Gaia2[ ( int ) pos.x, ( int ) pos.y ];

                   if( ga2 )                                                                                             
                   {
                       if( ga2.TileID == ETileType.ITEM )
                       {
                           _item[ x, y ] = ( ItemType ) ga2.Variation;                                                         // item on the ground
                           _amount[ x, y ] = ( int ) ga2.Body.StackAmount;
                       }
                       else
                       {

                           if( ga2.TileID == ETileType.BUILDING )
                           {
                               AssignCustomTile( ref _item[ x, y ], ref _custom[ x, y ], bp, ga2, ga2.TileID );                // Building Custom type assignment
                               int itid = -1;                                                                                  // Checks which building item is being affected
                               for( int i = 0; i < ga2.Building.Itm.Count; i++ )
                                   if( bp.AffectedItem == ga2.Building.Itm[ i ].ItemType ) itid = i;

                               if( itid != -1 )
                                   _amount[ x, y ] = ( int ) ga2.Building.Itm[ itid ].ItemCount;                               // Amount of items in the building

                               if( bp.AffectedBuilding == ga2.Building.Type ) bll.Add( pos );
                           }
                       }
                   }
                   else if( Map.I.Gaia[ ( int ) pos.x, ( int ) pos.y ] )
                   {
                       AssignCustomTile( ref _item[ x, y ], ref _custom[ x, y ], bp, null, 
                       Map.I.Gaia[ ( int ) pos.x, ( int ) pos.y ].TileID );                                                     // Custom type assignment for gaia objects
                       _amount[ x, y ] = 1;
                   }
               }
           }

       List<Vector2> tgl = new List<Vector2>();
       int targetStackSize = 0;

       for( int y = 0; y < 5; y++ )                                                                                                // Loop through every position
       for( int x = 0; x < 5; x++ )
           {
               if( bp.ItemMatrix[ x, y ] != ItemType.NONE )                                                                        // Compare 2 matrices
               {
                   bool match = false;

                   if( _item[ x, y ] == bp.ItemMatrix[ x, y ] && _amount[ x, y ] >= bp.ItemAmount[ x, y ] )                        // normal item 
                       match = true;
                    
                   else if( _custom[ x, y ] != EBPIconType.NONE )
                   {
                       if( ( _item[ x, y ] == ItemType.Tl_Blueprint_Icon_1 && _custom[ x, y ] == bp.BPCustomIconType1 ) ||               // custom type 1
                           ( _item[ x, y ] == ItemType.Tl_Blueprint_Icon_2 && _custom[ x, y ] == bp.BPCustomIconType2 ) )                // custom type 2
                       {
                           if( _amount[ x, y ] >= bp.ItemAmount[ x, y ] ) match = true;
                       }
                   }

                   if( match )
                   {
                       for( int i = 0; i < bp.ItemAmount[ x, y ]; i++ ) tgl.Add( new Vector2( x, y ) );                            // Success: add to Item list
                       if( tg + new Vector2( x, y ) == LastPlacedPos )
                           targetStackSize = bp.ItemAmount[ x, y ];
                   }
                   else
                   {
                       tgl = null;                                                                                                 // Failure: Cancel list
                       goto skip;
                   }
               }
           }

           skip:

           if( tgl != null && tgl.Count > 0 )
           {
               Unit un = Map.I.GetUnit( ETileType.ITEM, LastPlacedPos );
               if( un )                                                                                                            // Building over Item Stack
               {
                   if( un.Body.StackAmount > targetStackSize )
                       return;
               }
               bool success = false;
               bool ok = CanUseBP( bp, ref success );
               if( ok == false ) return;

               if( bp.AffectedBuilding != BuildingType.NONE )                                                                     // Building Upgrade
               {
                   for( int i = 0; i < bll.Count; i++ )
                   {
                       Building.ApplyUpgrade( bll[ i ], bp );
                   }
               }
               else
                   if( bp.AffectedItem != ItemType.NONE )                                                                          // Item Upgrade
                   {
                       G.GIT( bp.AffectedItem ).ApplyUpgrade( bp );
                   }
                   else
                       if( bp.AffectedBluePrint != null )                                                                          // BluePrint Upgrade
                       {
                           Blueprint.ApplyUpgrade( bp );
                       }
              

               for( int i = 0; i < tgl.Count; i++ )                                                                               // Removes matched items
               {
                   Vector2 pos = tg + new Vector2( tgl[ i ].x, tgl[ i ].y );
                   Map.I.Farm.PlaceItem( pos, -1, false, true );
               }

               if( bp.AffectedBuilding != BuildingType.NONE )                                                                     // Removes Building Resource Stack
               {
                   for( int i = 0; i < bll.Count; i++ )
                   {
                       Building.PlaceItem( bll[ i ], -bp.AffectedBuildingItemsRequired, bp.AffectedItem, true );
                   }
               }

               ApplyBluePrintEffect( bp, success );                                                                                // apply blueprint effect
           }        
    }

    private static void AssignCustomTile( ref ItemType item, ref EBPIconType custom, Blueprint bp, Unit unit, ETileType tile )
    {
        switch( tile )
        {
            case ETileType.FOREST:
            custom = EBPIconType.Forest;
            if( bp.BPCustomIconType1 == EBPIconType.Forest ) item = ItemType.Tl_Blueprint_Icon_1;
            else if( bp.BPCustomIconType2 == EBPIconType.Forest ) item = ItemType.Tl_Blueprint_Icon_2;
            break;

            case ETileType.WATER:
            custom = EBPIconType.Water;
            if( bp.BPCustomIconType1 == EBPIconType.Water ) item = ItemType.Tl_Blueprint_Icon_1;
            else if( bp.BPCustomIconType2 == EBPIconType.Water ) item = ItemType.Tl_Blueprint_Icon_2;
            break;

            case ETileType.BUILDING:
            if( unit.Building.Type == BuildingType.Stone_Storage )
            {
                custom = EBPIconType.Bld_Stone_Storage;
                if( bp.BPCustomIconType1 == EBPIconType.Bld_Stone_Storage ) item = ItemType.Tl_Blueprint_Icon_1;
                else if( bp.BPCustomIconType2 == EBPIconType.Bld_Stone_Storage ) item = ItemType.Tl_Blueprint_Icon_2;
            }
            if( unit.Building.Type == BuildingType.Wood_Pile )
            {
                custom = EBPIconType.Bld_Wood_Storage;
                if( bp.BPCustomIconType1 == EBPIconType.Bld_Wood_Storage ) item = ItemType.Tl_Blueprint_Icon_1;
                else if( bp.BPCustomIconType2 == EBPIconType.Bld_Wood_Storage ) item = ItemType.Tl_Blueprint_Icon_2;
            }
            break;

            default:
            item = ItemType.NONE;
            custom = EBPIconType.NONE;
            break;
        }
    }

    public static readonly HashSet<EBPIconType> BuildingTypes = new HashSet<EBPIconType>()
    {
    EBPIconType.Bld_Stone_Storage,
    EBPIconType.Bld_Wood_Storage
    };
    public static bool IsBuilding( ItemType itt, EBPIconType custom )
    {
        return ( itt == ItemType.Tl_Blueprint_Icon_1 || itt == ItemType.Tl_Blueprint_Icon_2 )
               && BuildingTypes.Contains( custom );
    }


    public static bool CanUseBP( Blueprint bp, ref bool success )
    {
        if( bp.MaxUses > 0 && bp.UsesList.Count >= bp.MaxUses )                                                            // Used too many times!
        {
            Map.I.Farm.PlaceItem( Map.I.Hero.GetFront(), -1, true );
            Message.RedMessage( "Maximum Uses Reached!" );
            return false;
        }

        if( Building.GetItemAmount( bp.CostItem ) < bp.Cost )                                                          // Out Of Res!
        {
            Map.I.Farm.PlaceItem( Map.I.Hero.GetFront(), -1, true );
            Message.RedMessage( "Not Enough " + Item.GetName( bp.CostItem ) + "!" );
            return false;
        }

        if( bp.GeneratedBuilding != BuildingType.NONE )
        if( Building.Place( false, LastPlacedPos, bp.GeneratedBuilding ) == false )                                    // Building Limit Reached!
            {
                Unit mud = Map.I.GetUnit( ETileType.MUD, LastPlacedPos );
                if( mud )
                {
                    Message.RedMessage( "Can't Build over Mud!" );                                                     // Build over mud error
                    return false;
                } 
                Map.I.Farm.PlaceItem( Map.I.Hero.GetFront(), -1, true );
                Message.RedMessage( "Building Limit Reached!" );
                return false;
            }

        float chc = GetStat( EVarType.BluePrint_Success_Rate, bp );                                                         // BluePrint Successful?
        if( Util.Chance( chc ) )
        {
            success = true;
            return true;
        }
        success = false;
        return true;
    }
    public static void ApplyBluePrintEffect( Blueprint bp, bool success )
    {      
        if( success )
        if( bp.GeneratedBuilding != BuildingType.NONE )                                                                          // Generate New Building 
        {
             Building.Place( true, LastPlacedPos, bp.GeneratedBuilding );
             Blueprint.UpgradePower = 1;
        }
        else                                                                                                                     // Craft New Item
            if ( bp.GeneratedItem != ItemType.NONE )
            {
                CraftItem( bp );
            }

        Building.AddItem( true, bp.CostItem, -bp.Cost );                                                                         // Charges Resource

        bp.Cost += bp.PenaltyPerUse;                                                                                             // Penalty per use

        float suc = GetStat( EVarType.BluePrint_Success_Rate, bp );
        string succ = "";                                                                                                        // Blueprint Failed Message.
        if( success == false )
        {
            succ = "BluePrint Failed!" + "  (" + suc.ToString( "0.#" ) + "%)\n";
            Message.RedMessage( succ );
        }

        if( success )
        {
            bp.UsesList.Add( ( int ) Blueprint.UpgradePower );                                                                    // Add Uses
            bp.AvailableUses--;
            MasterAudio.PlaySound3DAtVector3( "Build Complete", G.Hero.transform.position );
        }

        float resc = GetStat( EVarType.BluePrint_ReSort_Chance, bp );
        if( Util.Chance( resc ) ) bp.Sort( true );                                                                                // Re Sort BP

        //bp.Save();

        GetNextBluePrint();                                                                                                       // Get Next BP
    }
    public static void ApplyUpgrade( Blueprint bp )
    {
        if( bp.AffectedVariable == EVarType.NONE ) return;
        string perc = "";

        float power = Blueprint.GetStat( EVarType.BluePrint_Effect_Amount, bp );
        Blueprint.UpgradePower = Random.Range( 0, bp.AffectedVariableBonusAmount );
        power += Blueprint.UpgradePower;
        string pow = BluePrintPanel.GetEffectPowerText( bp );
        Blueprint tgb = bp.AffectedBluePrint;
        
        switch( bp.AffectedVariable )
        {
            case EVarType.BluePrint_Success_Rate:
            tgb.SuccessRate += power;
            float suc = GetStat( EVarType.BluePrint_Success_Rate, bp );
            float rest = suc - 100;
            if( rest > 0 ) tgb.SuccessRate -= rest;
            perc = "%";
            break;

            case EVarType.BluePrint_Cost:
            tgb.Cost += power;
            if( tgb.Cost < 0 ) tgb.Cost = 0;
            break;

            case EVarType.BluePrint_ReSort_Chance:
            tgb.SortChanceAfterUse += power;
            perc = "%";
            break;

            case EVarType.BluePrint_Effect_Amount:
            tgb.AffectedVariableAmount += power;
            break;
        }

        string nm = Util.GetName( bp.AffectedVariable.ToString() );
        Message.GreenMessage( "BluePrint Upgraded!\n" + nm + "\nAmount: " + pow );
    }
    public static void CraftItem( Blueprint bp )
    {
        Item it = G.GIT( bp.GeneratedItem );
        float chc = GetStat( EVarType.BluePrint_Success_Rate, bp );
        string craft = "";
        string itname = "\n" + it.GetName();
        craft += "\nCrafting Chance: " + chc + "%";
        float bonusamount = 0;
        int amount = 0;
        float bn = 0; float perc = 0; bool suc = false;
        float craftbonus = GetStat( EVarType.Crafting_Bonus_Factor, bp );
        bonusamount = Util.GetFactorInfo( craftbonus, ref bn, ref perc, ref suc );
        if( bonusamount > 0 )
        {
            craft += "\nBonus: " + bn;
            if( suc ) craft += " + 1  (" + perc + "%)" + "\nBonus Sort OK!";
            else craft += " + 0  (" + perc + "%)" + "\nBonus Sort Failed!";
        }
        amount = ( int ) ( bp.GeneratedItemAmount + bonusamount );
        itname += " x" + amount;
        Map.I.Farm.PlaceItem( LastPlacedPos, amount, false, false, bp.GeneratedItem );
        Message.GreenMessage( "Crafting Successful!" + itname + craft );
        Blueprint.UpgradePower = amount;
    }

    public void AddFreePlantCallBack()
    {
        FreePlants++;
    }
    public void ReSortCallBack()
    {
        Sort( true );
    }

    public void GenerateCostReportCallBack()
    {
        float cost = InitialCost;
        float succ = BaseSuccessRate;
        Debug.ClearDeveloperConsole();

        for( int i = 0; i < 100; i++ )
        {
            if( CostFluctuation != 0 )
            {
                float old = cost;
                cost = SortPrice( cost, false, true );
                if( cost > old ) cost = old;

                Debug.Log( "Cost Trial #" + i + "# - " + cost.ToString( "0.#" ) );
            }

            if( SuccessFluctuation != 0 )
            {
                float old = succ;
                succ = SortSuccess ( succ, false, true );
                if( succ < old ) succ = old;

                Debug.Log( "Success Trial #" + i + "# - " + succ.ToString( "0.#" ) );
            }
        }
    }

    public static float GetStat( EVarType var, Blueprint bp, int extralev = 0 )
    {
        switch( var )
        {
            case EVarType.BluePrint_Success_Rate:
            return bp.BaseSuccessRate + bp.SuccessRate;
            case EVarType.BluePrint_ReSort_Chance:
            return bp.BaseSortChanceAfterUse + bp.SortChanceAfterUse;
            case EVarType.BluePrint_Effect_Amount:
            return bp.GetBaseAmount( bp.UsesList.Count - 1 + extralev );
        }
        return -1;
    }

    public float GetBaseAmount( int usesID )
    {
        if( AffectedVariableBaseAmountList == null ||
            AffectedVariableBaseAmountList.Count < 1 )
            return AffectedVariableBaseAmount;
        if( usesID > AffectedVariableBaseAmountList.Count - 1 )
            return AffectedVariableBaseAmountList[ AffectedVariableBaseAmountList.Count - 1 ];
        return AffectedVariableBaseAmountList[ usesID ];
    }

    public static float GetUseSum( EVarType var )
    {
        float sum  = 0;
        for( int b = 0; b < G.Farm.BluePrintList.Count; b++ )
        if ( G.Farm.BluePrintList[ b ].AffectedVariable == var )
        for( int u = 0; u < G.Farm.BluePrintList[ b ].UsesList.Count; u++ )
        {
            sum += G.Farm.BluePrintList[ b ].UsesList[ u ];
            sum += G.Farm.BluePrintList[ b ].GetBaseAmount( u );
        }
        return sum;
    }

    public bool AutoApply( bool justCheck )
    {
        if( CostItem == AutoApplyCostItem )                                                                 // sums amount if both costs are the same
        {
            if( Building.GetItemAmount( AutoApplyCostItem ) < ( AutoApplyCost + Cost ) )
                return false;       
        }
        else
        {
            if( Building.GetItemAmount( AutoApplyCostItem ) < AutoApplyCost ) return false;
            if( Building.GetItemAmount( CostItem ) < ( Cost ) ) return false;
        }

        float bestdist = 100000;
        if( AutoApplyBuilding == null )
        for( int y = 0; y < Map.I.Tilemap.height; y++ )                                                     // Chooses the closest to hero building for auto upgrade
        for( int x = 0; x < Map.I.Tilemap.width; x++ )
        if ( Map.I.Gaia2[ x, y ] )
            {
                Unit bl = Map.I.Gaia2[ x, y ];
                if( bl.TileID == ETileType.BUILDING )
                {
                    float distance = Vector2.Distance( G.Hero.Pos, new Vector2( x, y ) );
                    if( distance < bestdist )
                    if( bl.Building.Type == AffectedBuilding )
                    {
                        AutoApplyBuilding = bl;
                        bestdist = distance;
                    }
                }
            }

        bool success = false;
        bool ok = CanUseBP( this, ref success );

        List<ItemType> il = new List<ItemType>();                                                                             // Make a list of all items to charge
        List<float> amtl = new List<float>();
        il.Add( AffectedItem );
        amtl.Add( AffectedBuildingItemsRequired );        

        for( int y = 0; y < 5; y++ )
        for( int x = 0; x < 5; x++ )
        if ( ItemMatrix[ x, y ] != ItemType.NONE )
        if ( IsBuilding( ItemMatrix[ x, y ], BPCustomIconType1 ) == false &&    // new
             IsBuilding( ItemMatrix[ x, y ], BPCustomIconType2 ) == false )
        {
            if( il.Contains( ItemMatrix[ x, y ] ) == false )                                                                // add new
            {
                il.Add( ItemMatrix[ x, y ] );
                amtl.Add( ItemAmount[ x, y ] );
            }
            else
            {
                for( int i = 0; i < il.Count; i++ )                                                                         // already in the list
                if ( il[ i ] == ItemMatrix[ x, y ] )
                {
                    float num = ItemAmount[ x, y ];
                    amtl[ i ] += num;
                }
            }
        }

        bool enough = true;
        for( int i = 0; i < il.Count; i++ )                                                                                 // Checks if items are enough 
        {
            if( Building.GetItemAmount( il[ i ] ) < amtl[ i ] ) enough = false;
        }

        if( enough == false )                                                                                               // not enough
        {
            return false;
        }

        if( justCheck ) return true;

        for( int i = 0; i < il.Count; i++ )                                                                                 // Charge items
        {
            Building.AddItem( true, il[ i ], -( int ) amtl[ i ] ); 
        }

        Building.AddItem( true, AutoApplyCostItem, -AutoApplyCost );                                                       // Charges auto apply Resource

        if( ok )
        {
            if( AffectedBuilding != BuildingType.NONE )
            {
                Building.ApplyUpgrade( AutoApplyBuilding.Pos, this );                                                      // Apply Upgrade
                ApplyBluePrintEffect( this, success );
            }
            MasterAudio.PlaySound3DAtVector3( "Cashier", transform.position );                                             // FX
            Message.GreenMessage( "Blueprint Automated!" );
            G.Tutorial.BluePrintAutomated = true;
        }

        return true;
    }

    public bool BuyPlants( bool justCheck )
    {
        if( BuyBPCostItem == ItemType.NONE ) return false;
        if( CostItem == BuyBPCostItem )                                                                                // sums amount if both costs are the same
        {
            if( Building.GetItemAmount( BuyBPCostItem ) < ( BuyBPCost + Cost ) )
                return false;       
        }
        else
        {
            if( Building.GetItemAmount( BuyBPCostItem ) < BuyBPCost ) return false;
            if( Building.GetItemAmount( CostItem ) < ( Cost ) ) return false;
        }

        if( justCheck ) return true;
        Building.AddItem( true, BuyBPCostItem, -BuyBPCost );                                                           // Charges Resource
        FreePlants++;
        MasterAudio.PlaySound3DAtVector3( "Cashier", transform.position );                                             // FX
        Message.GreenMessage( "Blueprint Bought!" );
        return true;
    }
}
