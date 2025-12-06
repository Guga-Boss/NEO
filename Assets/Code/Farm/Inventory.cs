using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class Inventory : MonoBehaviour {

    public enum IType
    {
        NONE = 0, Inventory, Packmule
    }

    public IType InventoryType;
    public List<Item> ItemList;
    public Dictionary<ItemType, int> Dic = new Dictionary<ItemType, int>();
    public List<Item> EmptySlotList;
    public static Item HoverIcon;
    public UIGrid Grid;
    
	// Use this for initialization
	void Start () 
    {
        HoverIcon = null;
	}

    public void InitDictionary()
    {
        for( int i = 0; i < ItemList.Count; i++ )                                                     // Initializes GIT Item Dictionary
        if ( ItemList[ i ] )
             Dic[ ItemList[ i ].Type ] = i;                                                           // Map ItemType to Dictionarylist index
    }
	
	// Update is called once per frame
    public void UpdateIt()
    {
        if( InventoryType == IType.Packmule )                                                          // Updates The Packmule
        {
            UpdatePackMule();
            return;
        }

        for( int i = 0; i < G.Farm.ToolbarButtons.Length; i++ )                                        // Toolbar Switching
        {
            if( G.Farm.ToolbarButtons[ i ].state == UIButtonColor.State.Pressed )
            {
                G.Farm.CurrentToolbar = ( EItemCategory ) i;
                Map.I.InvalidateInputTimer = .5f;
            }
        }

        //HoverIcon = null;
        for( int i = 0; i < ItemList.Count; i++ )
        if ( ItemList[ i ] )
        {
            Item it = ItemList[ i ];

            if( it.WarehouseList != null && it.WarehouseList.Count >= 1 )                              // Updates Max Stack   
            {
                float waremax = 0;
                if( it.BuildingMaxStackList != null )
                for( int m = 0; m < it.BuildingMaxStackList.Count; m++ )
                {
                    waremax += it.BuildingMaxStackList[ m ];
                }               

                it.MaxStack = waremax;                                                                // uses max stack list as max  

                if( it.WarehouseBuiltCount <= 0 )
                    it.MaxStack = it.PreWarehouseMaxStack;                                            // first: use pre warehouse max defined in item obj  

                if( it.BuildingMaxStackOverride > 0 )
                    it.MaxStack = it.BuildingMaxStackOverride;                                        // for last, uses the building overide item Max cap as the final decision. this one can be used to revert  limited cap to infinite again
            }

            it.gameObject.SetActive( false );

            if( it.NeverShowInInventory == false )
            if( it.GetCount() > 0 || 
              ( it.AlwaysShowInInventory && InventoryType == IType.Inventory ) )                  
            {
                bool ex = false;

                if( it.Category == EItemCategory.AdventureObject )
                {
                    if( G.Farm.CurrentToolbar == EItemCategory.AdventureObject ) ex = true;
                }  
                else  
                if( it.Category == EItemCategory.Resource )
                {
                    if( G.Farm.CurrentToolbar == EItemCategory.Resource ) ex = true;
                }
                else
                if( it.Category == EItemCategory.Tool )
                {
                    if( G.Farm.CurrentToolbar == EItemCategory.Tool ) ex = true;
                }
                else
                if( it.Category == EItemCategory.Farm )
                {
                    if( G.Farm.CurrentToolbar == EItemCategory.Farm ) ex = true;
                }
                else
                if( it.Category == EItemCategory.Fruit )
                {
                    if( G.Farm.CurrentToolbar == EItemCategory.Fruit ) ex = true;
                }

                //if( it.InventoryType == IType.Inventory && it.NeedPackmuleSpace )
                //{
                //    float cap = Item.GetStat( EVarType.PackMule_Capacity, it );                          // Packmule Capacity
                //    if( it.Category == EItemCategory.AdventureObject )
                //    {
                //        if( cap <= 0 ) ex = false;
                //        G.Packmule.ItemList[ ( int ) it.Type ].MaxStack = cap;
                //    }
                //}

                if( ex )
                {
                    it.gameObject.SetActive( true );
                    it.UpdateIconLabel();
                }
            }

            if( it.PreLoadBonus > 0 )                                                                                     // Adds preload bonus from daily rewards
            {
                if( Manager.I.GameType == EGameType.CUBES )
                    Item.AddItem( it.Type, it.PreLoadBonus );
                else
                if( Manager.I.GameType == EGameType.FARM )
                    Building.AddItem( true, it.Type, it.PreLoadBonus );
                it.PreLoadBonus = 0;
            }
        }

        ItemType itt = GetHoverItem();
        if( itt != ItemType.NONE )
        {
            string tx = UpdateInfoText( G.GIT( itt ) );
            if( tx != null )                                                                                               // Show item info
            {
                string txt = ItemList[ ( int ) itt ].GetName() + ":\n\n" + tx;
                Map.I.RM.DungeonDialog.SetMsg( txt, Color.white, 0 );
            }
        }

        RecipePanel.IsUpgradeButtonHovered = false;                                                                       // it only works here due to script processing order
        if( Map.I.Farm.RecipePanel.UpgradeRecipeButton.state == UIButtonColor.State.Hover )
            RecipePanel.IsUpgradeButtonHovered = true; 

        Grid.enabled = true;
        Grid.Reposition();
    }
    public void UpdatePackMule()
    {
        return;
        if( Map.I.RM.CurrentAdventure == -1 ) return;
        //HoverIcon = null;
        int size = ( int ) AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.UPGRADE_PACKMULE );        
        RandomMapData rm = Map.I.RM.RMList[ Map.I.RM.CurrentAdventure ];

        for( int i = 0; i < EmptySlotList.Count; i++ )
        {
            EmptySlotList[ i ].gameObject.SetActive( true );
            EmptySlotList[ i ].IconLabel.gameObject.SetActive( false );
            EmptySlotList[ i ].Sprite.gameObject.SetActive( false );

            if( rm.RequiredItem != ItemType.NONE )                                                              // Empty slot Stuff
            if( i == 0 )
            if( rm.GetRequiredItemAmount() > 0 )
            {
                EmptySlotList[ i ].Sprite.gameObject.SetActive( true );
                EmptySlotList[ i ].Sprite.sprite2D = 
                G.GIT( rm.RequiredItem ).Sprite.sprite2D;
                EmptySlotList[ i ].Sprite.color = Color.red;

                EmptySlotList[ i ].IconLabel.gameObject.SetActive( true );
                EmptySlotList[ i ].IconLabel.text = "x" + rm.GetRequiredItemAmount();
                EmptySlotList[ i ].IconLabel.color = Color.red;         
            }

            if( i >= size )
                EmptySlotList[ i ].gameObject.SetActive( false );
        }

        for( int i = 0; i < ItemList.Count; i++ )
        {
            Item it = ItemList[ i ];
            it.gameObject.SetActive( false );
            bool ex = false;

            it.Sprite.gameObject.SetActive( true );
            if( i >= size )
                it.Sprite.gameObject.transform.parent.gameObject.SetActive( false );

            if( it.Type != ItemType.NONE && it.Count > 0 )
            {
                ex = true;
                it.Sprite.sprite2D = G.GIT( it.Type ).Sprite.sprite2D;
                EmptySlotList[ i ].gameObject.SetActive( false );
            }

            if( ex )
            {
                it.gameObject.SetActive( true );
                it.UpdateIconLabel();
            }

            it.MaxStack = ( int ) AdventureUpgradeInfo.GetStat(                                                     // Base PAckMule Max Stack
            EAdventureUpgradeType.UPGRADE_PACKMULE_STACK );

            if( rm.RequiredItem != ItemType.NONE )                                                                    // Required Item Stuff
                if( i == 0 )
                    if( rm.GetRequiredItemAmount() > 0 )
                    {
                        it.MaxStack = rm.GetRequiredItemAmount();
                        int amount = ( int ) ItemList[ i ].Count;
                        if( amount < it.MaxStack )
                        {
                            it.IconLabel.text = "" + amount + " / " + it.MaxStack;
                            it.IconLabel.color = Color.red;
                        }
                        else
                            it.IconLabel.color = Color.green;

                        if( it.CountLifetime )
                        {
                            float tottime = Item.GetStat( EVarType.Total_Life_Time, it );
                            it.IconLabel.text = Util.ToSTime( tottime - it.LifeTimeCount );
                        }
                    }

            if( it.IconButton.state == UIButtonColor.State.Hover )
            {
                string msg = UpdateInfoText( it );                                                  // Updates mouse over item text
                string nm = G.GIT( it.Type ).GetName() +":\n\n";
                if( msg != "" )
                    Map.I.RM.DungeonDialog.SetMsg( nm + msg, Color.white, 0 );
            }
        }
    }
    
    public void UpdateInventoryProduction( float addSec = 0 )
    {
        if( InventoryType == IType.Inventory )                                // Updates Inventory Obj Production
        for( int i = 0; i < ItemList.Count; i++ )
        if ( ItemList[ i ] )
        {
            ItemList[ i ].UpdateProduction( addSec );            
        }

        if( InventoryType == IType.Packmule )                                 // Updates Packmule Obj Lifetime
        for( int i = 0; i < ItemList.Count; i++ )
            ItemList[ i ].UpdateLifeTime( addSec );
    }

    public ItemType GetHoverItem()
    {
        for( int i = 0; i < ItemList.Count; i++ )
            if( ItemList[ i ] )
            {
                if( ItemList[ i ].IconButton.state == UIButtonColor.State.Hover )
                {
                    return ( ItemType ) i;
                }
            }
        return ItemType.NONE;
    }
    public void UpdateDebug( ItemType it )
    {
        if( Helper.I.DebugHotKey )
        {
            //if( Input.GetMouseButtonDown( 0 ) )
            //    Item.AddItem( IType.Inventory, it, +1 );

            //if( Input.GetMouseButtonDown( 1 ) )
            //    Item.AddItem( IType.Inventory, it, -1 );
        }
    }

    public string UpdateInfoText( Item itm )
    {
        if( itm.Type != ItemType.NONE )
        {
            UpdateDebug( itm.Type );
            string txt = Language.Get( itm.Type.ToString().ToUpper() + "_DESCRIPTION", "Inventory" );
            txt = txt.Replace( "\\n", "\n" );
            HoverIcon = itm;

            Item other = G.GIT( itm.Type );                                                   // Lifetime takes data from the inventory even if mouse over packmule
            float totl = Item.GetStat( EVarType.Total_Life_Time, other );

            string lifet = Util.ToTime( totl - other.LifeTimeCount );
            string life = "\n\nLifeTime: " + lifet;
            if( itm.TotalLifeTime == 0 ) life = "";
            
            float tott = Item.GetStat( EVarType.Production_Total_Time, other );
            float tm = tott - itm.ProductionCount;
            string answer = Util.ToTime( tm );
            string next = "\n\nNext in: " + answer;
            if( Item.IsPlagueMonster( ( int ) itm.Type, false ) )
            {
                next = "\n\nPosition Change in: " + answer;
                if( tm <= 0 ) next = "\n\nPOSITION CHANGED!!";
            }
            if( tott == 0 ) next = "";
            float plim = Item.GetStat( EVarType.Production_Limit, other );
            if( plim > 0 ) 
                next += "\nMax Production Limit: " + plim;
            if( other.ProductionEnabled() == false ) next = "";

            //float craftbonus = Item.GetStat( EVarType.Crafting_Bonus_Factor, itm );
            //float bn = 0; float perc = 0; bool suc = false;
            //Util.GetFactorInfo( craftbonus, ref bn, ref perc, ref suc );
            //string craft = "\nCraft Bonus: " + bn + " + 1 (" + perc + "%)";
            //if( bn == 0 ) craft = "";

            string extra = "";
            if( itm.Type == ItemType.Farm_Size_Token )
            {
                int spc = ( int ) ( itm.Count ) - G.Farm.FarmSize; 
                extra = "\n\nFarm Size: " + G.Farm.FarmSize;
                extra += "\nAvailable Space: " + ( spc  );  
            }

            if( itm.Type == ItemType.Planting_Token )
            {
                int spc = ( int ) ( itm.Count ) - G.Farm.MudTiles;
                extra = "\n\nCurrent: " + G.Farm.MudTiles;
                extra += "\nAvailable  " + ( spc );
            }

            if( itm.Type == ItemType.Water_Token )
            {
                int spc = ( int ) ( itm.Count ) - G.Farm.WaterTiles;
                extra = "\n\nCurrent: " + G.Farm.WaterTiles;
                extra += "\nAvailable  " + ( spc );
            }

            if( itm.Type == ItemType.Chest_Points )
            {               
                float chc = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.CHEST_PERSIST_CHANCE );                       // Chest persist
                extra = "\nSession Open Chests: " + Map.I.SessionChestsOpenCount + "\nPersist Chance: " + chc + "%";
            }

            if( Manager.I.GameType == EGameType.CUBES )
                extra += "\nLifetime Collected: " + Item.GetTotalGained( itm.Type ).ToString( "0." );


            if( itm.Type == ItemType.Secrets_Found )
            {
                int col = ( int ) ItemList[ ( int ) ItemType.Secrets_Found ].Count;
                extra = "\nGame Secrets: " + col + "/" + Map.I.TotalSecrets;
                col = ( int ) Item.GetNum( ItemType.Secrets_Found, IType.Inventory, Map.I.RM.CurrentAdventure );
                extra += "\nQuest Secrets: " + col + "/" + Map.I.RM.RMList[ Map.I.RM.CurrentAdventure ].QuestSecrets;
            }

            if( itm.Type == ItemType.Coconut )
            {
                float hp = AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.HEALING_HP, itm.Type );
                extra = "\nHP Amount: +" + hp;
                int cap = ( int ) AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.PACKMULE_ITEM_CAPACITY, itm.Type );
                extra += "\nMax Equip: " + cap;
            }
            return txt + life + next + extra;
        }
        return null;
    }
    public void Save( string fl = "" )
    {
        if( Manager.I.SaveOnEndGame == false ) return;
        if( G.Tutorial.CanSave() == false ) return;

        TF.ActivateFieldList( "Inventory" );                                                // Activates Inventory Tagged Field List

        string file = Manager.I.GetProfileFolder();
        if( fl != "" ) file += "Cube Save/Inventory" + fl + ".NEO";
        else file += "/Inventory.NEO";

        using( MemoryStream ms = new MemoryStream() )
        using( BinaryWriter writer = new BinaryWriter( ms ) )                               // Open Memory Stream
        {
            GS.W = writer; 
            // troca para a versão 2 (novo formato chunked por item)
            int Version = Security.SaveHeader( 1 );                                         // Save Header Defining Current Save Version

            writer.Write( ItemList.Count );                                                 // Save List size

            for( int i = 0; i < ItemList.Count; i++ )
            {
                bool save = false;
                if( ItemList[ i ] )
                {
                    save = true;
                    if( GS.IsSaving )
                    if( ItemList[ i ].IsGameplayResource == false )                         // if Middle cube playing: Just save if it is gameplay resource
                        save = false;
                }

                writer.Write( save );                                                       // Save existence boolean

                if( save )
                {
                    // Cria um MemoryStream temporário para o item (bloco independente) isso possiblita adicionar variaveis novas a serem salvas NO FINAL de item.save()
                    using( var msItem = new MemoryStream() )
                    using( var wItem = new BinaryWriter( msItem ) )
                    {
                        // configura GS.W para gravar dentro do bloco do item
                        var prevW = GS.W;
                        GS.W = wItem;

                        ItemList[ i ].Save();                                               // Save Item (escreve no msItem via GS.W)

                        wItem.Flush();

                        // restaura GS.W para o writer principal
                        GS.W = prevW;

                        // grava tamanho do bloco e os bytes no stream principal
                        var itemBytes = msItem.ToArray();
                        writer.Write( ( int ) itemBytes.Length );                          // item block size
                        writer.Write( itemBytes );                                         // item data
                    }
                }
            }

            writer.Flush();                                                                 // Flush the writer
            Security.FinalizeSave( ms, file );                                              // Finalize save
        }                                                                                   // using closes the stream automatically
    }

    public void Load( string fl = "" )
    {
        string file = Manager.I.GetProfileFolder();
        if( fl != "" ) file += "Cube Save/Inventory" + fl + ".NEO";
        else file += "/Inventory.NEO";

        if( File.Exists( file ) == false ) return;

        TF.ActivateFieldList( "Inventory" );                                                      // Activates Inventory Tagged Field List

        byte[] fileData = File.ReadAllBytes( file );                                              // Read full file
        byte[] content = Security.CheckLoad( fileData );                                          // Validate HMAC and get clean content

        using( var ms = new MemoryStream( content ) )
        using( var mainReader = new BinaryReader( ms ) )
        {
            GS.R = mainReader;                                                                    // aponta GS.R para o reader principal só para compatibilidade (alguns flows podem usar diretamente)

            int SaveVersion = Security.LoadHeader();                                              // Load Header

            // --- novo formato (chunked por item) ---
            int count = GS.R.ReadInt32();                                                         // quantidade de items

            if( count > ItemList.Count )
                count = ItemList.Count;

            for( int i = 0; i < count; i++ )
            {
                bool load = GS.R.ReadBoolean();                                                   // Load existence boolean

                if( !load )
                    continue;

                if( ItemList[ i ] == null )
                {
                    // existe no save, mas não há objeto atual; pular o bloco
                    int skipSize = GS.R.ReadInt32();
                    GS.R.BaseStream.Seek( skipSize, SeekOrigin.Current );
                    continue;
                }

                // Ler tamanho do bloco e criar reader temporário para o item
                int itemSize = GS.R.ReadInt32();
                if( itemSize <= 0 )
                    continue;

                byte[] itemData = GS.R.ReadBytes( itemSize );

                using( var msItem = new MemoryStream( itemData ) )
                using( var rItem = new BinaryReader( msItem ) )
                {
                    // troca GS.R para o reader do item, chama Load e depois restaura
                    var prevR = GS.R;
                    GS.R = rItem;

                    ItemList[ i ].Load();                                                         // Load no contexto do bloco (usa GS.R interno)

                    // restaura GS.R para o reader principal
                    GS.R = prevR;
                }
            }        
            GS.R.Close();                                                                         // Close stream
        }
    }


    public void InitAreaEnter()
    {
        if ( Quest.CurrentLevel != -1 ) return;
        for( int i = 0; i < ItemList.Count; i++ )
        if ( ItemList[ i ] != null )
        {
            ItemList[ i ].AreaEnterCount = ItemList[ i ].Count;
        }
    }
    public void RestartArea()
    {
        if ( Quest.CurrentLevel != -1 ) return;
        for( int i = 0; i < ItemList.Count; i++ )
        if ( ItemList[ i ] != null )
        {
            ItemList[ i ].Count = ItemList[ i ].AreaEnterCount;
        }
    }

    public static void InitReference()
    {
        GameObject ob = GameObject.Find( "Inventory Items" );
        Item[] il = ob.GetComponentsInChildren<Item>();
        Inventory iv = GameObject.Find( "Inventory" ).GetComponent<Inventory>();
 
        for( int i = 0; i < il.Length; i++ )
        {
            iv.ItemList[ ( int ) il[ i ].Type ] = il[ i ];
        }

        ob = GameObject.Find( "Packmule Items" );
        il = ob.GetComponentsInChildren<Item>();
        Inventory pc = GameObject.Find( "Packmule" ).GetComponent<Inventory>();
        for( int i = 0; i < il.Length; i++ )
        {
            pc.ItemList[ ( int ) il[ i ].Type ] = il[ i ];
        }
    }

    public void ClearPackMule()
    {
        //if( InventoryType != IType.Packmule ) return;

        //if( Map.I.RM.GameOver )                                                                // attempts to save packmule items
        //for( int i = 0; i < ItemList.Count; i++ )
        //for( int t = 0; t < 100; t++ )
        //     Item.MoveItem( Inventory.IType.Inventory, ItemList[ i ].Type, 1, i );

        //for( int i = 0; i < ItemList.Count; i++ )
        //{
        //    Item it = ItemList[ i ];
        //    it.Count = 0;
        //    it.Type = ItemType.NONE;
        //    it.CountLifetime = false;
        //    it.LifeTimeCount = 0;
        //}
        //Debug.Log("Packmule Emptied!");
    }
}
