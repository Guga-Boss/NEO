using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class Farm : MonoBehaviour
{
    #region Variables
    [TabGroup( "Lists" )]
    public tk2dSprite[] ItemSprite;
    [TabGroup( "Lists" )]
    public tk2dSprite NextPlagueIndicator, GrabPlagueIndicator, GlowPlagueMonsterIndicator;
    [TabGroup( "Main" )]
    public Vector2 NextPosition, GrabPosition;
    [TabGroup( "Main" )]
    public Vector2 ScrollPosition = new Vector2( 120, 130 );
    [TabGroup( "Main" )]        
    public bool GrabActivated;
    [TabGroup( "Main" )]
    public ItemType NextMonsterType;
    [TabGroup( "Link" )]
    public UILabel BluePrintLabel, BuildingLabel, BuildingDescription; 
    [TabGroup( "Main" )]
    public ItemType SelectedItem;
    [TabGroup( "Main" )]
    public Vector2 GrabTarget;
    [TabGroup( "Main" )]
    public int FeathersPlaced = 5;
    [TabGroup( "Main" )]
    public int CarryingAmount;
    [TabGroup( "Main" )]
    public int ExclusiveToAdventureItem;
    [TabGroup( "Main" )]
    public string UniqueID = "";
    [TabGroup( "Main" )]
    public int ConsecutiveHoneyCombsCollected, ConsecutiveFeatherCollected;
    [TabGroup( "Link" )]
    public Idle IdleEngine;
    [TabGroup( "Link" )]
    public GameObject BlueprintUI, BuildingUI, PlagueIndicatorsFolder;
    [TabGroup( "Link" )]
    public BluePrintPanel BPPanel;
    [TabGroup( "Link" )]
    public RecipePanel RecipePanel;
    [TabGroup( "Main" )]
    public bool Button, MaxHoneycombsReached;
    [TabGroup( "Main" )]
    public List<Blueprint> BluePrintList;
    [TabGroup( "Main" )]
    public List<Building> BuildingList;
    [TabGroup( "Main" )]
    public EItemCategory CurrentToolbar = EItemCategory.Resource;
    [TabGroup( "Link" )]
    public UIButton[] ToolbarButtons;
    [TabGroup( "Link" )]
    public UILabel FreePlantButtonTxt, OldBlueprintButtonText;
    [TabGroup( "Main" )]
    public int FarmSize;
    [TabGroup( "Main" )]
    public Vector2 FarmLimit = new Vector2( 30, 30 );
    [TabGroup( "Main" )]
    public readonly Vector2 MiddleTile = new Vector2( 128, 128 );
    [TabGroup( "Main" )]
    public readonly Vector2 FarmEntrance = new Vector2( 128, 128 );
    [TabGroup( "Main" )]
    public int MudTiles = 0;
    [TabGroup( "Main" )]
    public int WaterTiles = 0;
    [TabGroup( "Link" )]
    public RandomMapData FarmData;
    [TabGroup( "Link" )]
    public UISprite BluePrintItemBackSprite, BluePrintItemBackSpriteActive;
    [TabGroup( "Link" )]
    public List<UI2DSprite> BluePrintCustomSprite;
    [TabGroup( "Lists" )]
    public List<Vector2> ChooserMonsterList, PushMonsterList, KillerMonsterList,
    KickableMonsterList, SwapMonsterList, GrabMonsterList, BlockerMonsterList, HoneyCombList;
    [TabGroup( "Lists" )]
    public List<VI> Tl = null;
    [TabGroup( "Data" )]
    public List<ItemType> AddMonsterList;
    [TabGroup( "Data" )]
    public List<float> WoodAxeChopTime;          // Progressive time needed to chop forest tiles depending on active Axes
    public static bool CreatingBuildings = false;
    public static bool ItemSavingAllowed = true;
    public static List<Vector2> RevealedTileList = new List<Vector2>();
    #endregion
    
    public void StartIt()
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )                                                  // Error: Game in progress
        if( Map.I.RM.GameOver == false )
            {
                Map.I.RM.DungeonDialog.SetMsg( "Error: Finalize Session First!", Color.red );
                return;
            }

        UpdateTileList();
        Manager.I.GameType = EGameType.FARM;
        Manager.I.Status = EGameStatus.PLAYING;
        ItemSavingAllowed = false;
        MaxHoneycombsReached = false;                          
        Map.I.RM.RMD.Copy( FarmData );
        Map.I.RM.RMD.Init();

        Quest.I.UpdateArtifactData( ref Map.I.Hero );
        SetSelectedItem( ItemType.NONE, 0 );
        Map.I.RM.Finalize();
        BluePrintWindow.Init();
        BluePrintWindow.I.gameObject.SetActive( false );
        Blueprint.UpdateSorting();
        Manager.I.Inventory.transform.localPosition = new Vector3( 713, 329.74f, 0 );
        Map.I.RM.DungeonDialog.gameObject.SetActive( false );
        Map.I.RM.DungeonDialog.BluePrintUI.gameObject.SetActive( true );
        LevelIntro.I.gameObject.SetActive( false );
        UI.I.FarmUI.gameObject.SetActive( true );
        Map.I.RM.DungeonDialog.InventoryBack.SetActive( true );
        Map.I.RM.DungeonDialog.DarkPerkBack.SetActive( false );
        UI.I.BackgroundUI.gameObject.SetActive( true );
        UI.I.GoalPanel.gameObject.SetActive( false );
        Map.I.ForceHideVegetation = false;
        gameObject.SetActive( true );
        UI.I.gameObject.SetActive( true );
        ResourceIndicator.DisableAll();        
        CreatingBuildings = false;
        Item.SetAmt( ItemType.Res_Plague_Monster, 0 );
        NextMonsterType = ItemType.NONE;
        GrabActivated = false;
        Building.Bl = null;
        Quest.I.CurLevel = Quest.I.Farm;
        Quest.CurrentLevel = -1;
        Map.I.AreaID = new int[ Map.I.Tilemap.width, Map.I.Tilemap.height ];
        Manager.I.ExitLevel();
        UI.I.SetBigMessage( "", Color.white );
        string pname = ProfileWindow.I.ProfileNames[ 0 ].text;
        UI.I.GameLevelText.text = pname + "'s Farm";
        UI.I.AreasText.text = "";
        UI.I.ArtifactsText.text = "";
        bool file = Load();
        CarryingAmount = 0;
        ConsecutiveHoneyCombsCollected = 0;
        ConsecutiveFeatherCollected = 0;
        if( !file ) Map.I.StartGame();
        AddStoneResources();
        Manager.I.Inventory.gameObject.SetActive( true );
        Map.I.Tilemap.Layers[ ( int ) ELayerType.GAIA ].gameObject.SetActive( false ); 
        IdleEngine.StartIt();
        Blueprint.GetNextBluePrint( true );
        Unit scr = Map.I.GetUnit( ETileType.SCROLL, ScrollPosition );                                        // Create Scroll over position (to avoid scroll destroyed bug)
        if( scr == null ) 
            Map.I.CreateUnit( Map.I.GetUnitPrefab( ETileType.SCROLL ), ScrollPosition );
    }  
    public bool Finalize()
    {
        if( G.Tutorial.CheckPhase( 14 ) == false ) return false;

        Map.I.Lights.SetTempLight();                                               
        Map.I.RM.DungeonDialog.BluePrintUI.gameObject.SetActive( false );
        Building.GatherGroundObjects();
        Save();
        ItemSavingAllowed = true;
        Building.UpdateItemData( true );
        gameObject.SetActive( false );
        SetSelectedItem( ItemType.NONE, 0 );
        UI.I.SetBigMessage( "", Color.yellow );
        MapSaver.I.UpdateIntroMessage();
        return true;
    }

    public void UpdateIt()
    {
        if( Manager.I.GameType != EGameType.FARM ) return;
                
        UpdateInput();
        UpdateItemPlacement();
        Blueprint.CheckPatterns();
        Building.UpdateAllBuildings();
        Building.UpdateItemData();
        UpdateUI();
        Blueprint.UpdateIt();
        G.Tutorial.UpdateIt();
        G.Inventory.UpdateIt();
        UpdateFarmObjects();
    }

    public void UpdateUI()
    {
        if( !G.Tutorial.CheckPhase( 6 ) ) 
        {
            BuildingUI.SetActive( false );
            BlueprintUI.SetActive( true );     // Tutorial stuff
            Building.BuildingHover = true;
            if( G.Tutorial.Phase < 6 )
                BlueprintUI.SetActive( false );
            return;
        }

        if( Item.GetNum( ItemType.HoneyComb ) == Item.GetMax( ItemType.HoneyComb ) )
        {
            MaxHoneycombsReached = true;
            //if( Map.I.AdvanceTurn )                                                                        // Honeycomb full message
            //if( Map.I.SessionTime < 5 ) 
            //    {
            //        Message.CreateMessage( ETileType.NONE, ItemType.HoneyComb,
            //        "Maximum number of Honeycombs Reached!\nCollect now not to Lose some of them.",
            //        G.Hero.Pos, Color.red, true, true, 10, .1f );
            //    }
        } 

        BuildingUI.SetActive( false );
        Building.BuildingHover = false;
        ItemType typ = G.Inventory.GetHoverItem();
        if( typ != ItemType.NONE )
        {
            string itemTxt = G.Inventory.UpdateInfoText( G.GIT( typ ) );
            if( BluePrintWindow.I.gameObject.activeSelf == false )
            if( itemTxt != null )                                                                     // Show Item info
            {
                Building.BuildingHover = true;
                BuildingLabel.text = G.GIT( typ ).GetName();
                BuildingDescription.text = itemTxt;
                BuildingUI.SetActive( true );
                return;
            }
        }
       
        Unit un = Map.I.GetUnit( ETileType.BUILDING, G.Hero.GetFront() );                                        // Gets Building Unit
        if( un == null ) un = Map.I.GetUnit( ETileType.BUILDING, new Vector2( Map.I.Mtx, Map.I.Mty ) );
        if( un == null ) return; 
         
        Building bl = un.Building;
        Building orb = BuildingList[ ( int ) bl.ID ];

        if( bl == null ) return;

        if( bl.RecipeList == null || bl.RecipeList.Count < 1 )
            RecipePanel.gameObject.SetActive( false );

        BuildingLabel.text = bl.GetName();

        string info = bl.Type.ToString().ToUpper() + "_DESCRIPTION";
        string txt = Language.Get( info, "Building" );

        if( bl.Category != EBuildingCategory.Plant )
        {
            txt += "\nBuilding: " + orb.UnitsConstructed;
            if( bl.ConstructionAllowed != -1 )
                txt += " of " + bl.ConstructionAllowed;
        }

        if( bl.Category != EBuildingCategory.Work_Area )
        if( bl.Category != EBuildingCategory.Plant )
            txt += "\nMoving Cost: " + bl.MovingWheelBarrowCost + " WheelBarrows";
        txt = txt.Replace( "\\n", "\n" );

        Item it = null;
        if( bl.Itm != null && bl.Itm.Count > 0 )
        {
            float prd = Building.GetStat( EVarType.Total_Building_Production_Time, bl, bl.SelItemID );
            string tm = Util.ToSTime( ( double ) prd ); 
            float maxc = Building.GetStat( EVarType.Maximum_Item_Stack, bl, bl.SelItemID );
            string max = "/" + maxc.ToString( "0." );
            if( bl.Itm[ bl.SelItemID ].ItemType != ItemType.NONE )
                it = G.GIT( bl.Itm[ bl.SelItemID ].ItemType );
            BuildingItem bi = bl.Itm[ bl.SelItemID ];
            if( bi.BaseMaxItemStack == -1 ) max = "";

            if( bl.Category == EBuildingCategory.Producer ||
                bl.Category == EBuildingCategory.Plant )
            {
                txt += "\n\n" + it.GetName() + ": " + bi.ItemCount.ToString( "0." ) + max;
                if( bi.WorkIsDone )
                {
                    if( prd > 0 && bl.Category == EBuildingCategory.Plant )
                    {
                        float wit = bi.TotalWitherTime - ( bi.ProductionTimeCount - prd );
                        if( bi.TotalWitherTime > 0 )
                            txt += "\nWither Time: " + Util.ToSTime( wit );
                        else
                            txt += "\nWither Time: Eternal";
                    }
                }
                else
                {
                    if( prd > 0 )
                    {
                        txt += "\nProduction Time: " + tm;
                        if( bi.MaxProductionStack != -1 )
                            txt += "\nMax: " + bi.MaxProductionStack;
                    }
                    if( bi.AdditivePlantingFactor > 0 )                                                              // additive planting
                    txt += "\nPlant Multiple seeds to Increase Harvest";
                }
            }
            if( bl.Category == EBuildingCategory.Storage )
            {
                txt += "\n\n" + it.GetName() + ": " + bi.ItemCount.ToString( "0." ) + max;
            }
            if( bl.Category == EBuildingCategory.Work_Area )
            {
                if( prd > 0 )
                    txt += "\nWork Time: " + tm;
            }
            float aux = prd - bi.ProductionTimeCount;
            string answer = Util.ToTime( aux );
            if( bl.Category == EBuildingCategory.Producer )
            if( bi.ItemCount >= maxc ) answer = "Max Capacity Reached!";
            string next = "\nNext in: \n" + answer;
            if( aux <= 0 ) next = "";

            if( bl.Category == EBuildingCategory.Plant )
            {
                if( bi.WorkIsDone ) next = "\nTree is Ripe!\n";
                else
                next = "\nRipe in: \n" + answer;
            }
            if( bl.Category == EBuildingCategory.Work_Area )
                next = "\nDone in: \n" + answer;

            if( it != null )
            {
                float cap = ( int ) Item.GetStat( EVarType.Carry_Capacity, it );
                float rcap = ( int ) Item.GetStat( EVarType.Carry_Capacity, it );
                txt += "\nCarry Cap: " + cap; // +"  (" + ( cap + rcap ) + ")";
            }

            BuildingDescription.text = txt + "\n" + next;
            if( prd == 0 ) next = "";
        }

        Building.BuildingHover = true;
        if( BluePrintWindow.I.gameObject.activeSelf == false )
        {
            BuildingUI.SetActive( true );
            UI.I.SetTurnInfoText( "", 1, Color.white );
        }
    }

    public void Save()
    {
        if( Manager.I.SaveOnEndGame == false ) return;
        if( G.Tutorial.CheckPhase( 14 ) == false ) return;                          // No saving if has not yet reached phase 14 of tutorial

        TKUtil.Save( "Farm.NEO", ref Quest.I.CurLevel.Tilemap );                    // Save Farm tilemap
        Building.SaveBuildings( ref Quest.I.CurLevel.Tilemap );                     // Save Buildings
        Blueprint.SaveAll();                                                        // Save Blueprint   
        G.Tutorial.Save();                                                          // Save tutorial
        IdleEngine.Save();                                                          // Save Idle Engine

        if( G.Tutorial.Phase == 14 )
        {
            Secret.Save();
            Manager.I.Save();
            Map.I.NavigationMap.Save( true );
            Manager.I.Inventory.Save();
            //RandomMapGoal.SaveAll();                                        
        }
    }

    public bool Load()
    {
        bool res = TKUtil.Load( "Farm.NEO", ref Quest.I.CurLevel.Tilemap );         // Load Farm Tilemap
        if( res == false ) return false;
        Map.I.Finalize();                                                           // Finalize map
        Map.I.StartGame();                                                          // Init game
        Building.LoadBuildings( ref Quest.I.CurLevel.Tilemap );                     // Load Buildings
        Blueprint.LoadAll();                                                        // Load Blueprints
        Building.SendItemsToWarehouses();                                           // Send items to warehouses
        InitMonstersAfterLoading();                                                 // Init monsters
        G.Tutorial.Load();                                                          // Load Tutorial
        TKUtil.LoadFogOfWar( "Farm.NEO", ref Quest.I.CurLevel.Tilemap );            // Load Fog of War
        return true;
    }

    public bool UpdateItemPlacement()
    {
        Vector2 tg = Map.I.Hero.GetFront();
        if( Map.PtOnMap( Map.I.Tilemap, tg ) == false ) return false;

        if( G.Tutorial.CheckPhase( 2 ) == false ) return false;                                                        // Tutorial restriction

        int amt = 0;                                                                                                   // Key check
        if( cInput.GetKeyDown( "Wait"    ) ) amt = +1;
        if( cInput.GetKeyDown( "Special" ) ) amt = -1;

        if( UpdateToolUsage( tg ) ) return false;                                                                      // Tool Usage
        if( amt == 0 ) return false;

        bool res = PlaceItem( tg, amt, true, false );                                                                  // Place item
        if( res ) return true;
        res = Building.PlaceItem( tg, amt );                                                                           // Place item in building

        return res;
     }       

    public bool PlaceItem( Vector2 tg, int amt, bool heroplaced, bool bp = false, ItemType force = ItemType.NONE )
    {
        if( tg == Map.I.LevelEntrancePosition ) return false;
        Unit ga2 = Map.I.GetUnit( tg, ELayerType.GAIA2 );
        if( CheckFarmLimit( tg ) == false ) return false;                                                              // Check Farm Limit
        Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, tg );
        if( un != null && Item.IsPlagueMonster( un.Variation ) )
            return false;
        Unit ga = Map.I.GetUnit( tg, ELayerType.GAIA );
        if( ga != null && ga.TileID != ETileType.MUD ) return false;                                                  // Only grass and mud placement

        ItemType it = SelectedItem;
        if( force != ItemType.NONE ) it = force;
        if( Item.IsPlagueMonster( ( int ) it ) == true ) return false;
        
        if( ga2 == null )
        {
            if( amt == -1 ) return false;                                                                              // Empty target, place new
            if( heroplaced && CarryingAmount < 1 ) return false;
            if( it == ItemType.NONE ) return false;
            int carry = CarryingAmount - 1;
            if(!heroplaced ) carry = CarryingAmount;

            SetSelectedItem( SelectedItem, carry );
            Unit prefabUnit = Map.I.GetUnitPrefab( ETileType.ITEM );
            
            GameObject go = Map.I.CreateUnit( prefabUnit, tg );                                                       // create object
            Unit res = go.GetComponent<Unit>();
            res.Variation = ( int ) it;
            res.Spr.spriteId = G.GIT( it ).TKSprite.spriteId;
            res.Body.StackAmount = amt;
            res.Body.ExclusiveToAdventure = ExclusiveToAdventureItem;
            res.UpdateText();
            if( !bp ) Blueprint.LastPlacedPos = tg;
        }
        else                                                                                                           // Adds or Subtracts from stack
        {
            if( ga2.TileID != ETileType.ITEM ) return false;
            if( Item.IsPlagueMonster( ( int ) ga2.Variation ) == true ) return false;
            if( amt == 1 && CarryingAmount < 1 ) return false;

            if( ga2.Variation != ( int ) SelectedItem )
            if( CarryingAmount > 0 ) return false;

            float cap = 1;
            if( it != ItemType.NONE )
            {
                cap = ( int ) Item.GetStat( EVarType.Carry_Capacity, G.GIT( SelectedItem ) );                                                  // capacity check
                      
                Unit road = Map.I.GetUnit( ETileType.ROAD, G.Hero.Pos );
                if( road ) cap += ( int ) Item.GetStat( EVarType.Road_Carry_Capacity,  G.GIT( SelectedItem ) );                                   
            }

            if( amt == -1 && CarryingAmount >= cap && heroplaced )
            {
                Message.RedMessage( "Maximum Carry Capacity Reached: " + cap );                                       // maxximum capacity reached
                return false;
            }

            int carry = CarryingAmount - amt;
            if( heroplaced == false ) carry = CarryingAmount;
            SetSelectedItem( ( ItemType ) ga2.Variation, carry, ga2.Body.ExclusiveToAdventure );
            
            ga2.Body.StackAmount += amt;
            ga2.UpdateText();

            if( amt == -1 && ga2.Body.StackAmount <= 0 ) ga2.Kill();
            if( !bp ) Blueprint.LastPlacedPos = tg;
        }

        if( amt > 0 ) Blueprint.CheckBluePrintPatters = true;                                                        // Triggers Blueprint Check pattern function

        return true;
    }

    public void SetSelectedItem( ItemType it, int carry, int exclusive = -1 )
    {
        if( Item.IsPlagueMonster( ( int ) it ) == true ) return;
        if( carry > CarryingAmount )
            MasterAudio.PlaySound3DAtVector3( "Lift Resource", G.Hero.transform.position );
        if( carry < CarryingAmount )
            MasterAudio.PlaySound3DAtVector3( "Drop Resource", G.Hero.transform.position );

        SelectedItem = it;
        CarryingAmount = carry;
        ExclusiveToAdventureItem = exclusive;
        for( int i = 0; i < ItemSprite.Length; i++ )
        {
            if( it != ItemType.NONE && G.GIT( it ).TKSprite )
            ItemSprite[ i ].spriteId = G.GIT( it ).TKSprite.spriteId;
            ItemSprite[ i ].gameObject.SetActive( false );

            if( CarryingAmount > i )
                ItemSprite[ i ].gameObject.SetActive( true );

            if( it == ItemType.NONE || CarryingAmount <= 0 )
            {
                ItemSprite[ i ].gameObject.SetActive( false );
            }
        }

        Map.I.Hero.LevelTxt.gameObject.SetActive( false );
        if( CarryingAmount >= 9 )
        {
            Map.I.Hero.LevelTxt.text = "x" + CarryingAmount;
            Map.I.Hero.LevelTxt.gameObject.SetActive( true );
        }
    }

    private void UpdateInput()
    {
        if( Manager.I.GugaVersion == false ) return;
        if( Helper.I.DebugHotKey )
        {
            if( Input.GetKeyDown( KeyCode.A ) )
            {
                AddMonster( G.GIT( ItemType.Chicken ) );
                //Building.PlaceItemScatered( ItemType.Barbecue, 1 );              // Charge required items
            }

            if( Input.GetKeyDown( KeyCode.E ) )
            {
                Item it = G.GIT( ItemType.Plague_Monster_Cloner );
                UpdateBlockingMonsters( it );   /// REMOOOOOVE
                it = G.GIT( ( int ) ItemType.Plague_Monster_Spawner );
                UpdateBlockingMonsters( it );   /// REMOOOOOVE 
                it = G.GIT( ( int ) ItemType.Plague_Monster_Slayer );
                UpdateBlockingMonsters( it );   /// REMOOOOOVE 
                it = G.GIT( ( int ) ItemType.Plague_Monster_Kickable );
                UpdateBlockingMonsters( it );   /// REMOOOOOVE          
                it = G.GIT( ( int ) ItemType.Plague_Monster_Swap );
                UpdateBlockingMonsters( it );   /// REMOOOOOVE    
                it = G.GIT( ( int ) ItemType.Plague_Monster_Blocker );
                UpdateBlockingMonsters( it );   /// REMOOOOOVE    
                it = G.GIT( ( int ) ItemType.Plague_Monster_Grabber );
                UpdateBlockingMonsters( it );   /// REMOOOOOVE                                       
                it = G.GIT( ( int ) ItemType.HoneyComb );
                UpdateBlockingMonsters( it );   /// REMOOOOOVE   
                ConsecutiveFeatherCollected = 0;
                it = G.GIT( ( int ) ItemType.Feather );
                UpdateBlockingMonsters( it );   /// REMOOOOOVE  
                ConsecutiveHoneyCombsCollected = 0;
                UpdateFeatherCreation();
            }

            Unit bld = Map.I.GetUnit( ETileType.BUILDING, G.MP );
            if( bld )
            if( Input.GetMouseButtonDown( 0 ) )
            {
                ItemType itt = Building.GetCurrentBuildingItem( G.MP );
                Building.PlaceItem( G.MP, 1, itt, true );
                G.GIT( ItemType.Energy ).Count = 100;
            }
            if( Input.GetMouseButtonDown( 1 ) )
            {
                ItemType itt = Building.GetCurrentBuildingItem( G.MP );
                Building.PlaceItem( G.MP, -1, itt, true );
            }

            if( Input.GetKeyDown( KeyCode.F3 ) )
                for( int i = 0; i < BluePrintList.Count; i++ )
                    BluePrintList[ i ].Sort();           
        }
    }
    public void GetNextBluePrintCallBack()
    {
        Blueprint.GetNextBluePrint();
        Map.I.InvalidateInputTimer = 1;
    }
    public bool UpdateToolUsage( Vector2 tg )
    {
        if( cInput.GetKeyDown( "Battle" ) == false ) return false;

        Unit ga = Map.I.GetUnit( tg, ELayerType.GAIA );
        Unit ga2 = Map.I.GetUnit( tg, ELayerType.GAIA2 );

        if( SelectedItem == ItemType.NONE ) return false;
        if( CarryingAmount <= 0 ) return false;
        if( CheckFarmLimit( tg ) == false ) return false;                                                      // Check Farm Limit

        if( SelectedItem == ItemType.Club )                                                                    // Club kills plague monster
        {
            Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, tg );
            if( un == null ) return false;
            if( Item.IsPlagueMonster( un.Variation, false ) == false ) return false;
            G.Farm.SetSelectedItem( SelectedItem, G.Farm.CarryingAmount - 1 );
            un.Kill();
            Map.I.CreateExplosionFX( tg );                                                                     // Explosion FX
            MasterAudio.PlaySound3DAtVector3( "Monster Falling", un.Pos );
            return true;
        }

        if( ga2 != null )                                                                                      // Stack Tool: Return
        if( ga2.TileID == ETileType.ITEM ) return false;

        ItemType tool = ItemType.NONE;
        Item it = G.GIT( ( int ) SelectedItem );
        float customProductionTime = -1;

        if( SelectedItem == ItemType.WoodAxe )                                                                 //---------- Wood Axe
        if( ga != null && ga.TileID == ETileType.FOREST )
            {
                if( ga == null || ga.TileID != ETileType.FOREST ) return false;
                if( Building.ValidForestTileForChopping( tg ) == false ) return false;                         // Check neighbor tiles for chopping 
                tool = ItemType.WoodAxe;
                customProductionTime = Building.GetForestChopTime();
                if( CheckTokenLimits(tool, ItemType.Farm_Size_Token, G.Farm.FarmSize  ) == false ) 
                    return false;
            }

        if( SelectedItem == ItemType.Hoe )                                                                     //------------ Hoe dig planting spot  
            {
                if( ga2 != null ) return false;
                if( ga != null && ga.TileID != ETileType.MUD ) return false;
                tool = ItemType.Hoe;
                customProductionTime = 3600f;
                if( ga == null || ga.TileID != ETileType.MUD )
                if( CheckTokenLimits( tool, ItemType.Planting_Token, G.Farm.MudTiles ) == false )
                    return false;
            }

        if( SelectedItem == ItemType.Shovel )                                                                  //------------ Shovel dig water spot  
        {
            if( ga2 != null ) return false;
            if( ga != null && ga.TileID != ETileType.WATER && 
                ga.TileID != ETileType.MUD ) return false;
            tool = ItemType.Shovel;
            customProductionTime = 3600f;
            if( ga == null || ga.TileID != ETileType.WATER )
            if( CheckTokenLimits( tool, ItemType.Water_Token, G.Farm.WaterTiles ) == false )
                return false;
        }

        if( it.IsSeed )                                                                                       // Seed Planting
        {
            if( ga == null ) return false;
            if( ga.TileID == ETileType.MUD )
                tool = it.Type;
            else return false;

            int rad = 2;
            for( int y = ( int ) tg.y - rad; y <= tg.y + rad; y++ )                            
            for( int x = ( int ) tg.x - rad; x <= tg.x + rad; x++ )
            if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( x, y ) ) )                                          // Plants min Distance check
            {
                Unit un = Map.I.GetUnit( ETileType.BUILDING, new Vector2( x, y ) );
                if( un && un.Building != null )
                if( un.Building.Category == EBuildingCategory.Plant )
                {
                    Message.RedMessage( "Plants need 2-tile spacing." );
                    return false;
                }
            }

            if( ga2 )
            if( ga2.Building.Type == it.ToolCreationBuilding ) 
            {
                BuildingItem bi = ga2.Building.Itm[ 0 ];
                if( bi.ProductionTimeCount > 60 ) 
                { 
                    Message.RedMessage( "Multiple Planting Only Available in the First Minute" );              // Additive Planting times up
                    return false; 
                }
                if( bi.AdditivePlantingFactor > 0 )                                                            // Additive planting: Use more seeds to increase item count (fruits)
                {
                    bool res = Building.PlaceItem( ga2.Pos, ( int )
                    bi.AdditivePlantingFactor, bi.ItemType, true );                                            // Place item in building
                    if( res ) G.Farm.SetSelectedItem( G.Farm.SelectedItem, G.Farm.CarryingAmount - 1 );        // Remove Seed from hand
                    Controller.CreateMagicEffect( tg );                                                        // Magic FX  s
                    return false;
                }
            }

            if( ga2 != null ) return false;
        }

        BuildingType btype = it.ToolCreationBuilding;

        if( tool == ItemType.NONE || btype == BuildingType.NONE ) return false;                                           // No deal, return

        if( ga2 && ga2.TileID == ETileType.BUILDING )
        {
            if( ga2.Building.Type != btype ) return false;

            float en = G.GIT( tool ).ToolUsageEnergyCost;
            if( Item.AddItem( Inventory.IType.Inventory, ItemType.Energy, -en, false ) )                      // Perform work manually
            {
                int itmid = Building.GetBuildingItemID( ( ItemType ) tool, ga2.Building );
                if( itmid == -1 ) { Debug.LogError( "Bad itmid" ); return false; } 

                ga2.Building.Itm[ itmid ].ToolHitCount++;
                ga2.Building.Itm[ itmid ].ProductionTimeCount +=                       
                G.GIT( itmid ).ToolUsageWorkTime;

                Message.RedMessage( en.ToString( "0.#" ) + " Energy Depleted!" );
            }
            else
                Message.RedMessage( "Not Enough Energy!" );
            return true;
        }
        else                                                                                                    // Create work area
        {
            bool res = Building.Place( true, tg, btype, tool );

            if( res )
            {
                Building.Bld.Building.ToolType = tool;
                Building.Bld.Building.CustomProductionTime = customProductionTime;
                if( it.IsSeed )
                    G.Farm.SetSelectedItem( G.Farm.SelectedItem, G.Farm.CarryingAmount - 1 );                 // Remove Seed from hand
            }
            return true;
        }
    }
    private bool CheckTokenLimits(ItemType tool, ItemType token, int size )                                   // Check to see if the number of tokens is enough for the work area to be installed
    {
        int areas = Building.GetWorkAreaCount( tool );
        int tk = ( int ) Item.GetNum( token );
        int avail = ( int ) ( tk - size ) - areas;
        if( avail < 1 )
        {
            string nm = G.GIT( token ).GetName();
            Message.CreateMessage( ETileType.NONE, token, "Not Enough " + 
            nm + "!", G.Hero.Pos, Color.red );                                                                // not enough
            return false;
        }
        return true;
    }

    public bool CheckFarmLimit( Vector2 tg, bool showmsg = true )
    {
        int sx = ( int ) FarmLimit.x / 2;
        int sy = ( int ) FarmLimit.y / 2;
        Rect r = new Rect( Map.I.LevelEntrancePosition.x - sx,
        Map.I.LevelEntrancePosition.y - sy, FarmLimit.x, FarmLimit.y );

        if( r.Contains( tg ) ) return true;
        if( showmsg )
            Message.RedMessage( "Error, Farm limit size exceeded!" );
        return false;
    }

    public void UpdateCarriedItemRotation()
    {
        if( Manager.I.GameType != EGameType.FARM ) return;
        if( SelectedItem == ItemType.NONE ) return;

        if( G.GIT( SelectedItem ).AutoRotateWithHero == false )                       // non rotating item
            for( int i = 0; i < ItemSprite.Length; i++ )
                ItemSprite[ i ].transform.eulerAngles = new Vector3( 0, 0, 0 );
        else
            for( int i = 0; i < ItemSprite.Length; i++ )
                ItemSprite[ i ].transform.localRotation = G.Hero.transform.localRotation;                    // rotating item
    }

    public bool CheckRoadCarryCapacityBlock( Vector2 from, Vector2 to )
    {
        if( Manager.I.GameType != EGameType.FARM ) return false;
        if( SelectedItem == ItemType.NONE ) return false;
        if( CarryingAmount <= 1 ) return false;

        Unit fu = Map.I.GetUnit( ETileType.ROAD, from );
        Unit tu = Map.I.GetUnit( ETileType.ROAD, to );

        if( fu && tu ) return false;
        if( !fu && !tu ) return false;

        if( fu && !tu )
        {
            float cap = ( int ) Item.GetStat( EVarType.Carry_Capacity, G.GIT( SelectedItem ) );
            //Unit ga = Map.I.GetUnit( ETileType.ROAD, G.Hero.Pos );
            //if( ga ) cap = G.GIT( SelectedItem ).RoadCarryCapacity;

            if( CarryingAmount > cap )
            {
                Message.RedMessage( "Drop Some items First!" );
                return true;
            }
        }

        return false;
    }
    private void UpdateTileList()
    {
        Tl = new List<VI>();
        int x0 = Mathf.Max( Mathf.FloorToInt( MiddleTile.x - FarmLimit.x ), 0 );
        int x1 = Mathf.Min( Mathf.CeilToInt( MiddleTile.x + FarmLimit.x ), Map.I.Tilemap.width - 1 );
        int y0 = Mathf.Max( Mathf.FloorToInt( MiddleTile.y - FarmLimit.y ), 0 );
        int y1 = Mathf.Min( Mathf.CeilToInt( MiddleTile.y + FarmLimit.y ), Map.I.Tilemap.height - 1 );
        for( int y = y0; y <= y1; y++ )
        for( int x = x0; x <= x1; x++ )
             Tl.Add( new VI( x, y ) );                                                                   // Init tile list optimized to work only inside farm area
    }


    public void UpdateListsCallBack()
    {
        //GameObject go = GameObject.Find( "BluePrint List" );        its being done manually now
        //Blueprint[] bp = go.GetComponentsInChildren<Blueprint>();
        //BluePrintList.Clear();
        //BluePrintList.AddRange( bp );

        MapSaver ms = GameObject.Find( "Areas Template Tilemap" ).
        GetComponent<MapSaver>();
        Map.I = GameObject.Find( "----------------Map" ).GetComponent<Map>();
        RandomMap rm = GameObject.Find( "----------------Random Map----------------" ).
        GetComponent<RandomMap>();
        rm.DungeonDialog.UpdatePanels();
        Manager.I = GameObject.Find( "----------------Game Manager---------" ).GetComponent<Manager>();
        Map.I.RM = rm;
        GameObject go = GameObject.Find( "----------------Navigation Map" );
        NavigationMap nm = go.GetComponent<NavigationMap>();
        List<RandomMapData> rmaps = new List<RandomMapData>();
        List<string> siglist = new List<string>();
        nm.MapBonusList = new List<NavigationMapBonus>();
        int[ , ] poscount = new int[ TechButton.SX, TechButton.SY ];
        Map.I.GlobalTechList = new List<AdventureUpgradeInfo>();

        Helper.I = GameObject.Find( "Helper" ).GetComponent<Helper>();
        Helper.I.StartingAdventure = ms.CurrentAdventure;
        G.Init();
        Map.I.TotalCubeCount = 0;

        for( int i = 0; i < nm.Tilemap.GetTilePrefabsListCount(); i++ )
        {
            int tempLayer = 0;
            GameObject prefabObject;
            int x, y;
            nm.Tilemap.GetTilePrefabsListItem( i, out x, out y, out tempLayer, out prefabObject );
            Vector2 pos = new Vector2( x, y );

            if( prefabObject != null )
                if( prefabObject.tag == "Map Quest" )                                                           // Map quest
                {
                    RandomMapData rd = prefabObject.GetComponent<RandomMapData>();

                    if( rm.RMList.Contains( rd ) == false )
                    {
                        rd.QuestHelper = prefabObject.GetComponent<MapQuestHelper>();
                        rd.MapCord = new Vector2( pos.x, pos.y );
                        if( rd.QuestHelper.QuestName == "" )
                            rd.QuestHelper.QuestName = Area.GetRandomAreaName( rm.AreaNamesTextFile );

                        prefabObject.name = rd.QuestHelper.QuestName;
                        //prefabObject.name = "Quest Prefab " + rd.QuestID + ": " + rd.QuestName;                   // Enable this to rename prefabs
                        MapQuestHelper he = rd.QuestHelper;

                        if( he.Signature == "" )
                        {
                            Debug.LogError( "Invalid Signature: " + prefabObject );
                            return;
                        }
                        else siglist.Add( he.Signature );
#if UNITY_EDITOR
                        PrefabUtility.DisconnectPrefabInstance( prefabObject );
#endif
                        rmaps.Add( rd );
                    }
                }

            if( prefabObject != null )
                if( prefabObject.tag == "Map Bonus" )
                {
                    NavigationMapBonus rd = prefabObject.GetComponent<NavigationMapBonus>();
                    rd.TextMesh.text = "x" + rd.BonusAmount;
                    rd.BonusIcon.spriteId = G.GIT( rd.BonusItem ).TKSprite.spriteId;
                    nm.MapBonusList.Add( rd );
                    rd.MapCord = new Vector2( pos.x, pos.y );

#if UNITY_EDITOR
                    PrefabUtility.DisconnectPrefabInstance( prefabObject );
#endif                    
                }
        }

        for( int m = 0; m < rmaps.Count; m++ )                                                                   // WARNING Quests cant be deleted or quest id will be changed
        {
            rm.RMList.Add( rmaps[ m ] );
            int id = rm.RMList.Count - 1;
            rm.RMList[ id ].QuestID = id;                                                                        // Assign Quest id
            Debug.Log( "New Quests Have been added: " + rmaps[ m ].name + " id: " + id );                        // new Quests Added
        }

        string repeat = "";                                                             // Check for repeated Quest signature
        bool rep = Util.CheckRepeated( siglist, ref repeat );
        if( rep )
            Debug.LogError( "Repeated Quest Signature: " + repeat );

        DungeonDialog dd = Map.I.RM.DungeonDialog;
        dd.TotBronze = dd.TotSilver = dd.TotGold = dd.TotDiamond = dd.TotAdamantium = dd.TotGenius = 0;

        for( int rmid = 0; rmid < rm.RMList.Count; rmid++ )                                                                                       // Check for Duplicated Goal ID
        {
            RandomMapData rmd = rm.RMList[ rmid ];
            if( rmd.UniqueID == "" )
                rmd.UniqueID = SortUniqueID( 5 );

            rm.RMList[ rmid ].gameObject.name = rm.RMList[ rmid ].QuestHelper.QuestName;

            if( rmd.GoalsFolder == null )
                rmd.GoalsFolder = Util.getChildGameObject( rmd.gameObject, "Goals" );

            if( rmd.ImagesFolder == null )
                rmd.ImagesFolder = Util.getChildGameObject( rmd.gameObject, "Images" );

            RandomMapGoal[] rmg = rmd.gameObject.GetComponentsInChildren<RandomMapGoal>();
            if( rmg.Length > 0 )
                rmd.GoalList = rmg;
            if( rmd.UpgradeListFolder == null )
                rmd.UpgradeListFolder = Util.getChildGameObject( rmd.gameObject, "Adventure Upgrade Info List" );

            if( rmd.UpgradeListFolder )
                rmd.AdventureUpgradeInfoList = rmd.UpgradeListFolder.gameObject.GetComponentsInChildren<AdventureUpgradeInfo>();         // Updates Adventure upgrade info list        
            else rmd.AdventureUpgradeInfoList = null;            
            
            if( rmd.AdventureUpgradeInfoList != null && rmd.AdventureUpgradeInfoList.Length > 0 )
                rmd.AdventureUpgradeInfoList[ 0 ].transform.parent.name = "Adventure Upgrade Info List";

            if( rmd.AdventureUpgradeInfoList != null )                                                                        // force unlock goal if initial level == 0
            if( rmd.AdventureUpgradeInfoList.Length >= 1 )
            if( rmd.StartingAdventureLevel == 0 )
            {
                rmd.AdventureUpgradeInfoList[ 0 ].UpgradeType = EAdventureUpgradeType.UNLOCK_ADVENTURE;
            }

            if( rmd.AdventureUpgradeInfoList != null )
            for( int u = 0; u < rmd.AdventureUpgradeInfoList.Length; u++ )                                                    // Updates Adventure upgrade obj name
                rmd.AdventureUpgradeInfoList[ u ].UpdateGameObjectText();

            Map.I.RM.DungeonDialog.StudiesGO.gameObject.SetActive( true );
            Map.I.TB = Map.I.RM.DungeonDialog.TechButtonsGO.GetComponentsInChildren<TechButton>();
            Map.I.RM.DungeonDialog.StudiesGO.gameObject.SetActive( false );

            #region Tech Button Adjust
            //for( int tt = 0; tt < Map.I.TB.Length; tt++ )    //use this to adjust tech button stuff
            //{
            //    Map.I.TB[ tt ].UnlockCostSprite.height = Map.I.TB[ tt ].UnlockCostSprite.width = 45;
            //    Map.I.TB[ tt ].UnlockCostSprite.transform.localPosition = new Vector3( 18.3f, 618.1f, 0);
            //    Map.I.TB[ tt ].DescriptionLabel.width = 195;
            //    Map.I.TB[ tt ].DescriptionLabel.height = 193;
            //    Map.I.TB[ tt ].DescriptionLabel.fontSize = 138;
            //    Map.I.TB[ tt ].DescriptionLabel.transform.localPosition = new Vector3(
            //        -0.27f, 10.37f, Map.I.TB[ tt ].DescriptionLabel.transform.position.z );
            //    Map.I.TB[ tt ].UnlockCostLabel.transform.localPosition = new Vector3( 3, -13.6f, 0 );
            //    Map.I.TB[ tt ].UnlockCostLabel.pivot = UIWidget.Pivot.Center;
            //    Map.I.TB[ tt ].UnlockCostLabel.height = 39;
            //    Map.I.TB[ tt ].UnlockCostLabel.fontSize = 44;
            //    Map.I.RM.DungeonDialog.StudiesGO.transform.localPosition = new Vector3( 1015, -5, 0 );
            //}
            #endregion

            if( rmd.TechListFolder == null )
                rmd.TechListFolder = Util.getChildGameObject( rmd.gameObject, "Tech Tree" );
            poscount = new int[ TechButton.SX, TechButton.SY ];
            if( rmd.TechListFolder )
            {
                AdventureUpgradeInfo[] tl = rm.RMList[ rmid ].TechListFolder.
                gameObject.GetComponentsInChildren<AdventureUpgradeInfo>();
                for( int u = 0; u < tl.Length; u++ )                                                                                     // Updates Tech upgrade obj name
                {
                    //tl[ u ].ID = u;
                    string aux = tl[ u ].transform.parent.name.Substring( 5 );
                    var words = aux.Split( ' ' );
                    tl[ u ].X = int.Parse( words[ 0 ] );
                    tl[ u ].Y = int.Parse( words[ 1 ] );
                    tl[ u ].TechID = poscount[ tl[ u ].X, tl[ u ].Y ];            // quest id if theres more than one quest in a button position
                    poscount[ tl[ u ].X, tl[ u ].Y ]++;
                    tl[ u ].QuestID = rmid;                                       // Quest id this tech belongs to
                    tl[ u ].tag = "Tech";
                    tl[ u ].UpdateGameObjectText();
                    if( tl[ u ].TechScope == ETechScope.All_Quests ) 
                        Map.I.GlobalTechList.Add( tl[ u ] );
                }
            }

            //if( rm.RMList[ i ].QuestHelper )
            //{
            //    if( rm.RMList[ i ].QuestHelper.MonsterSprite )
            //    {
            //rm.RMList[ i ].QuestHelper.MonsterSprite.SortingOrder = 0;
            //rm.RMList[ i ].QuestHelper.MonsterSprite.transform.position =

            //    new Vector3(
            //    rm.RMList[ i ].QuestHelper.MonsterSprite.transform.position.x,
            //    rm.RMList[ i ].QuestHelper.MonsterSprite.transform.position.y, -0.9f );

            //    }

            //    if( rm.RMList[ i ].QuestHelper.NestSprite )
            //    {
            //        rm.RMList[ i ].QuestHelper.NestSprite.SortingOrder = 0;
            //        rm.RMList[ i ].QuestHelper.NestSprite.transform.position =

            //            new Vector3(
            //            rm.RMList[ i ].QuestHelper.NestSprite.transform.position.x,
            //            rm.RMList[ i ].QuestHelper.NestSprite.transform.position.y, -0.8f );

            //    }

            //    if( rm.RMList[ i ].QuestHelper.QuestNameMesh )
            //    {
            //        rm.RMList[ i ].QuestHelper.QuestNameMesh.SortingOrder = 0;
            //        rm.RMList[ i ].QuestHelper.QuestNameMesh.transform.position =

            //            new Vector3(
            //            rm.RMList[ i ].QuestHelper.QuestNameMesh.transform.position.x,
            //            rm.RMList[ i ].QuestHelper.QuestNameMesh.transform.position.y, -5.2f );
            //    }
            //}

            for( int g= 0; g < rmg.Length; g++ )
            {
                rmg[ g ].ID = g;
                rmg[ g ].GoalUpgradeInfoList = rmg[ g ].gameObject.GetComponentsInChildren<GoalUpgradeInfo>();            // Updates Goal upgrade info list   
                rmg[ g ].name = rmg[ g ].UpdateGoalDescription();                                                         // Updates goal game obj name                

                if( rm.RMList[ rmid ].Available )
                {
                    if( rmg[ g ].TrophyType == ETrophyType.BRONZE ) dd.TotBronze++;                                       // Total Trophy Count
                    if( rmg[ g ].TrophyType == ETrophyType.SILVER ) dd.TotSilver++;
                    if( rmg[ g ].TrophyType == ETrophyType.GOLD ) dd.TotGold++;
                    if( rmg[ g ].TrophyType == ETrophyType.DIAMOND ) dd.TotDiamond++;
                    if( rmg[ g ].TrophyType == ETrophyType.ADAMANTIUM ) dd.TotAdamantium++;
                    if( rmg[ g ].TrophyType == ETrophyType.GENIUS ) dd.TotGenius++;
                }

                for( int u = 0; u < rmg[ g ].GoalUpgradeInfoList.Length; u++ )                
                {
                    GoalUpgradeInfo gu = rmg[ g ].GoalUpgradeInfoList[ u ];
                    string goal = " " + rm.RMList[ rmid ].QuestID + " " + rmg[ g ] + " " + gu;                          // check for goal errors
                    if( gu.UpgradeType == EGoalUpgradeType.NONE )
                        Debug.LogError( "EGoalUpgradeType.NONE" + goal );
 
                    if( gu.UpgradeCostAmount == 0 )
                        Debug.LogError( "UpgradeItem1CostAmount == 0" + goal );
                    if( gu.UpgradeType != EGoalUpgradeType.UNLOCK_GOAL )
                        if( gu.UpgradeBonusAmount == 0 )
                    if( gu.UpgradeType == EGoalUpgradeType.ITEM )
                        Debug.LogError( "UpgradeItem1BonusAmount == 0" + goal );
                    string head = "";

                    if( gu.UpgradeType == EGoalUpgradeType.ITEM )
                        head = "Extra " + rmg[ g ].BonusItem + " +" + gu.UpgradeBonusAmount + " ";
                    if( gu.UpgradeType == EGoalUpgradeType.ITEM2 )
                        head = "Extra " + rmg[ g ].BonusItem2 + " +" + gu.UpgradeBonusAmount + " ";
                    if( gu.UpgradeType == EGoalUpgradeType.UNLOCK_GOAL )
                        head = "Unlock Goal ";
                    if( gu.UpgradeType == EGoalUpgradeType.RECIPE_BONUS )
                        head = "Recipe Bonus: +" + gu.UpgradeBonusAmount + " ";
                    if( gu.UpgradeType == EGoalUpgradeType.BLUEPRINT_PLANTS )
                        head = "Blueprint Plants: +" + gu.UpgradeBonusAmount + " ";
                    if( gu.UpgradeType == EGoalUpgradeType.BLUEPRINT_USES )
                        head = "Blueprint Uses: +" + gu.UpgradeBonusAmount + " ";

                    //if( gu.UpgradeType == EGoalUpgradeType.ITEM2 )
                    //    head = "Extra " + rmg[ g ].BonusItem2 + " +" + gu.UpgradeItem2BonusAmount + " ";

                    gu.name = "" + head + " (" + gu.UpgradeCostType + " " + gu.UpgradeCostAmount + ")";       // Update goal name
                    gu.transform.parent.name = "Goal Upgrade List";
                }

                if( rmg[ g ].InitialLevel == 0 )
                {
                    rmg[ g ].GoalUpgradeInfoList[ 0 ].UpgradeType = EGoalUpgradeType.UNLOCK_GOAL;
                    //rmg[ g ].GoalUpgradeInfoList[ 0 ].name = gu.UniqueID + "Unlock Goal";
                }
            }

            SectorDefinition[] sd = null;
            if( rmd.IsUp )
            {
                sd = rm.RMList[ rmid ].gameObject.GetComponentsInChildren<SectorDefinition>();                             // Updates Mod List
                rmd.SDList = new SectorDefinition[ sd.Length ];
            }
            else
                sd = rmd.SDList;

            //Debug . Log( rmd.name );

            for( int s = 0; s < sd.Length; s++ )
            {
                sd[ s ].ModList = sd[ s ].gameObject.GetComponentsInChildren<Mod>();
                for( int m =0; m < sd[ s ].ModList.Length; m++ )                                                                           // Updates Mod name
                {
                    if( m == 0 ) sd[ s ].ModList[ m ].transform.parent.name = "Mod List";
                    string desc = " - " + sd[ s ].ModList[ m ].ModDescription;
                    string tname = "Mod " + ( int ) ( sd[ s ].ModList[ m ].ModNumber + 1 ) + desc;

                    if ( sd[ s ].ModList[ m ].RestrictToUnitType != null && sd[ s ].ModList[ m ].RestrictToUnitType.Count > 0 )            // Restrict to unit type
                    for( int u = 0; u < sd[ s ].ModList[ m ].RestrictToUnitType.Count; u++ )
                         tname += " " + sd[ s ].ModList[ m ].RestrictToUnitType[ u ].ToString() + ": ";   
                        
                    if( sd[ s ].ModList[ m ].name != tname )
                        sd[ s ].ModList[ m ].name = tname;
                }

                if( rmd.IsUp )
                {
                    rmd.SDList[ s ] = sd[ s ];
                    rmd.SDList[ s ].ADList = sd[ s ].gameObject.GetComponentsInChildren<AreaDefinition>();
                }

                for( int ai = 0; ai < rmd.SDList[ s ].ADList.Length; ai++ )
                {
                    AreaDefinition ad = rmd.SDList[ s ].ADList[ ai ];
                    if( ai == 0 )
                        ad.transform.parent.name = "Area Definition";

                    ad.name = ad.AreaColor.ToString() + " Area";
                    ad.TileList = ad.gameObject.GetComponentsInChildren<TileDefinition>();

                    for( int tid = 0; tid < ad.TileList.Length; tid++ )
                    {
                        ad.TileList[ tid ].name = "" + ad.TileList[ tid ].Tile.ToString();

                        if( ad.TileList[ tid ].PercentOfArea > 0 )
                            ad.TileList[ tid ].name += " " + ad.TileList[ tid ].PercentOfArea + "%"; else
                        if( ad.TileList[ tid ].Amount > 0 )
                            ad.TileList[ tid ].name += " x" + ad.TileList[ tid ].Amount;
                        if( ad.TileList[ tid ].Mod != EModType.NONE )
                        {
                            for( int i = 0; i < sd[ s ].ModList.Length; i++ )
                                if( sd[ s ].ModList[ i ].ModNumber == ad.TileList[ tid ].Mod )
                                ad.TileList[ tid ].name += "    " + sd[ s ].ModList[ i ].name;
                        }
                    }
                }

                RandomMapGoal.RestoreGoalPrefabReferences( rmd );
            }

            int count = 0;                                                                                  
            for( int j  = 0; j  < rmg.Length;  j++ )
            for( int jj = 0; jj < rmg.Length; jj++ )
            if ( j != jj )
                    {
                        if( rmg[ j  ].name.Substring( 0, 4 ) == 
                            rmg[ jj ].name.Substring( 0, 4 ) ) count++;

                        if( count > 1 )
                        {
                            Debug.LogError( "Duplicated Goal ID: " + rmd.QuestHelper.QuestName              // checks duplicated goal id
                            + " " + rmg[ j ].name + " " + rmg[ jj ].name );
                            return;
                        }
                    }   

            rmd.MaxCubes = 0;                                                                // Auto updates MaxCubes variable
            for( int tr = 1; tr < 100; tr++ )
            {
                string file = Application.dataPath + "/Resources/Map Templates/" +
                rmd.QuestHelper.SubFolder + "/" + rmd.QuestHelper.Signature +
                "/Cube " + tr + ".NEO";
                if( File.Exists( file ) )
                {
                    rmd.MaxCubes++;
                    if( rmd.Available )
                        Map.I.TotalCubeCount++;
                }
                else break;
            }

            //if( rmd.MaxCubes == 0 )
            //    Debug.LogError( "No Cubes in: " + rmd.QuestHelper.QuestName );
        }

        for( int i = 0; i < BluePrintList.Count; i++ ) 
        {
            BluePrintList[ i ].ID = i;
            if( BluePrintList[ i ].UniqueID == "" )
                BluePrintList[ i ].UniqueID = SortUniqueID( 5 );
        }

        
        #region Daily Rewards
        //go = GameObject.Find( "Daily Rewards" );
        //NiobiumStudios.DailyRewards d = go.GetComponent<NiobiumStudios.DailyRewards>();
        //d.RewardBack = d.transform.GetComponentsInChildren<tk2dSlicedSprite>();
        //d.RewardIcon = d.transform.GetComponentsInChildren<tk2dSprite>();
        //d.RewardText = d.transform.GetComponentsInChildren<tk2dTextMesh>();
        #endregion


        go = GameObject.Find( "Building List" );                                       // its being done manually now
        //Building[] bl = go.GetComponentsInChildren<Building>();
        
        //int size = BuildingType.GetNames( typeof( BuildingType ) ).Length;

        //BuildingList.Clear();

        //for( int e = 0; e < size; e++ )
        //{
        //    cont = 0;
        //    for( int b = 0; b < bl.Length; b++ )
        //        if( bl[ b ].Type == ( BuildingType ) e )
        //        {
        //            BuildingList.Add( bl[ b ] );
        //            cont++;
        //        }
             
        //    if( cont > 1 ) Debug.LogError( "Duplicated Building" );
        //}

        for( int b = 0; b < BuildingList.Count; b++ )                                          // recipe upgrade info list
        {
            BuildingList[ b ].ID = b;

            if( BuildingList[ b ].UniqueID == "" )
                BuildingList[ b ].UniqueID = SortUniqueID( 5 );

            for( int r = 0; r < BuildingList[ b ].RecipeList.Count; r++ )
            {
                BuildingList[ b ].RecipeList[ r ].ID = r;
                if( BuildingList[ b ].RecipeList[ r ].UniqueID == "" )
                    BuildingList[ b ].RecipeList[ r ].UniqueID = SortUniqueID( 5 );

                BuildingList[ b ].RecipeList[ r ].RecipeUpgradeInfoList = 
                BuildingList[ b ].RecipeList[ r ].gameObject.transform.GetComponentsInChildren<RecipeUpgradeInfo>();
                Recipe rc = BuildingList[ b ].RecipeList[ r ];

                for( int ui = 0; ui < BuildingList[ b ].RecipeList[ r ].RecipeUpgradeInfoList.Length; ui++ )
                {
                    RecipeUpgradeInfo uin = BuildingList[ b ].RecipeList[ r ].RecipeUpgradeInfoList[ ui ];
                    string info = " " + Map.I.Farm.BuildingList[ b ] + " " + rc + " " + ui;                          // check for errors
                    if( uin.UpgradeType == ERecipeUpgradeType.NONE )
                        Debug.LogError( "ERecipeUpgradeType.NONE" + info );
                    if( uin.UpgradeEffectAmount == 0 )
                        Debug.LogError( "UpgradeItemCostAmount == 0" + info );

                  string str1 = uin.UpdateText( uin.UpgradeType,  uin.UpgradeEffectAmount,  rc, ui, b, 1 );
                  string str2 = uin.UpdateText( uin.UpgradeType2, uin.UpgradeEffectAmount2, rc, ui, b, 2 );
                  string str3 = uin.UpdateText( uin.UpgradeType,  uin.UpgradeEffectAmount,  rc, ui, b, 3 );
                  uin.name = str1 + str2 + str3;
                }
            }
        }

        GameObject inv = GameObject.Find( "Inventory" );
        Inventory inve = inv.GetComponent<Inventory>();
        if( inve == null ) Debug.LogError( "No inventory obj found" );

        for( int i = 0; i < inve.ItemList.Count; i++ )                                        // Assigns ID to inventory items
            if( inve.ItemList[ i ] )
            {
                if( inve.ItemList[ i ] )
                    inve.ItemList[ i ].ID = i;

                if( inve.ItemList[ i ].UniqueID == "" )
                    inve.ItemList[ i ].UniqueID = SortUniqueID( 5 );

                if( inve.ItemList[ i ].SavePerAdventureCount )
                {
                    inve.ItemList[ i ].PerAdventureCount = new List<float>();
                    for( int q = 0; q < rm.RMList.Count; q++ )
                        inve.ItemList[ i ].PerAdventureCount.Add( 0 );
                    inve.ItemList[ i ].PerAdventureTotalCount = new List<float>();
                    for( int q = 0; q < rm.RMList.Count; q++ )
                        inve.ItemList[ i ].PerAdventureTotalCount.Add( 0 );
                }
            }

        for( int i = 0; i < inve.ItemList.Count; i++ )                                                            // check duplicated item id
        if ( inve.ItemList[ i ] )
        {
            for( int ii = i + 1; ii < inve.ItemList.Count; ii++ ) // i+1 evita comparar com si mesmo
             if( inve.ItemList[ ii ] )
            {
                if( inve.ItemList[ i ].UniqueID == inve.ItemList[ ii ].UniqueID )
                {
                    Debug.LogError( "Duplicated Item Unique ID" );
                    return;
                }
            }
        }

        for( int i = 0; i < rm.RMList.Count; i++ )                                                       // check duplicated Quest unique id
        for( int ii = i + 1; ii < rm.RMList.Count; ii++ ) // i+1 evita comparar com si mesmo
        if ( rm.RMList[ i ].UniqueID == rm.RMList[ ii ].UniqueID )
        {
            Debug.LogError( "Duplicated Quest Unique ID" );
            return;
        }       

        for( int i = 0; i < BuildingList.Count; i++ )                                                             // check duplicated building id
        for( int ii = i + 1; ii < BuildingList.Count; ii++ ) // i+1 evita comparar com si mesmo
         if( BuildingList[ i ].UniqueID == BuildingList[ ii ].UniqueID )
         {
             Debug.LogError( "Duplicated Building Unique ID" );
             return;
         }  

        for( int i = 0; i < BluePrintList.Count; i++ )                                                            // checks duplicated blueprint
        for( int ii = i + 1; ii < BluePrintList.Count; ii++ ) // i+1 evita comparar com si mesmo
            {
                if( BluePrintList[ i ].name == BluePrintList[ ii ].name )
                {
                    Debug.LogError( "Duplicated Blueprint Name" );
                    return;
                }

                if( BluePrintList[ i ].UniqueID == BluePrintList[ ii ].UniqueID )
                {
                    Debug.LogError( "Duplicated Blueprint Unique ID" );
                    return;
                }
            }

        for( int b = 0; b < BuildingList.Count; b++ )                                                             // checks duplicated recipe id
        {
            var recipes = BuildingList[ b ].RecipeList;
            for( int r = 0; r < recipes.Count; r++ )
            {
                for( int rr = r + 1; rr < recipes.Count; rr++ ) // r+1 evita comparar com ele mesmo
                {
                    if( recipes[ r ].UniqueID == recipes[ rr ].UniqueID )
                    {
                        Debug.LogError( "Duplicated Recipe Unique ID in Building " + BuildingList[ b ] );
                        return;
                    }
                }
            }
        }


        inv = GameObject.Find( "Packmule Panel" );
        inve = inv.GetComponent<Inventory>();
        if( inve == null ) Debug.LogError( "No Packmule obj found" );

        for( int i = 0; i < inve.ItemList.Count; i++ )                                          // Assigns ID to packmule items
            if( inve.ItemList[ i ] )
                inve.ItemList[ i ].ID = i;

        //EditorApplication.SaveScene();                                                        // Save Scene
        //EditorApplication.SaveAssets();
        Debug.Log( "Data Updated!" );
    }
    public static string SortUniqueID( int size )
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; // 36 caracteres possíveis
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        byte[] randomBytes = new byte[ size ];
        System.Security.Cryptography.RNGCryptoServiceProvider rng =
            new System.Security.Cryptography.RNGCryptoServiceProvider();
        rng.GetBytes( randomBytes ); // preenche com bytes aleatórios
        for( int i = 0; i < size; i++ )
            sb.Append( chars[ randomBytes[ i ] % chars.Length ] ); // converte byte para índice
        return sb.ToString();
    }

    public void AddStoneResources()                                                             // This method fixe the chest instead of stone placed on ground on farm init
    {
        if( G.Tutorial.CanSave() == true ) return;
        for( int y = 0; y < Map.I.Tilemap.height; y++ )
        for( int x = 0; x < Map.I.Tilemap.width; x++ )
            {
                Unit it = Map.I.GetUnit( ETileType.ITEM, new Vector2( x, y ) );
                if( it )
                {
                    it.Spr.spriteId = 0;
                    it.Variation = ( int ) ItemType.Stone;
                    it.Body.BonusItemList = null;
                }
            }
    }
}