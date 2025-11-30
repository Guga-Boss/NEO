using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using System.IO;

public class NavigationMap : MonoBehaviour
{
    #region Variables
    public tk2dTileMap Tilemap;
    public Level Level;
    public RandomMapData MapData;
    public GameObject MapQuestPrefab, MapBonusPrefab, QuestPanelPrefab, FirePrefab;
    public GameObject PrefabFolder, MapBonusFolder;
    public GameObject AreasTilemap;
    public List<tk2dSprite> MonsterSprite, NestSprite;
    public List<tk2dTextMesh> DirtyQuestName;
    public List<Vector2> BonusStepList;
    public List<tk2dSlicedSprite> BackgroundSprite;
    public List<NavigationMapBonus> MapBonusList, SpawnedMapBonusList;
    public NavigationMapBonus[ , ] BonusList;
    public Vector2 HeroInititialPosition;
    public bool MapQuestObjectsCreated = false;
    public int BumpedQuest = -1;
    public float BackAlpha = .5f;
    public int NavigationZoomMode = 1;
    #endregion

    public static NavigationMap Get()
    {
        return GameObject.Find( "----------------Navigation Map" ).GetComponent<NavigationMap>();
    }
    public void StartIt () 
    {
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )                                                  // Error: Game in progress
            if( Map.I.RM.GameOver == false )
            {
                Map.I.RM.DungeonDialog.SetMsg( "Error: Finalize Session First!", Color.red );
                return;
            }        
        Manager.I.GameType = EGameType.NAVIGATION;
        Manager.I.Status = EGameStatus.PLAYING;
        Quest.I.CurLevel = Level;        
        UI.I.DebugLabel.text = "";
        PrefabFolder.SetActive( true );
        Map.I.ForceHideVegetation = false;
        Map.I.RM.DungeonDialog.gameObject.SetActive( false );
        Map.I.RM.DungeonDialog.NavigationMapButtonClick = false;
        Map.I.RM.DungeonDialog.GotoNavigationMap = false;
        tk2dTileMap tm = Quest.I.Dungeon.Tilemap;
        TKUtil.CreateBlankMap( ref tm );
        Map.I.RM.RMD.Copy( MapData );
        Map.I.RM.RMD.Init();
        ResourceIndicator.DisableAll();  
        Map.I.StartGame();
        UI.I.GameLevelText.text = "Navigation Map";
        UI.I.GoalPanel.gameObject.SetActive( false );
        CreateMapQuests();
        DungeonDialog.UpdateStartingCube = false;
        Map.I.ZoomMode = NavigationZoomMode;
        Load();
        CreateMapBonuses();

        for( int i = 0; i < Map.I.RM.RMList.Count; i++ )
        {
            BackgroundSprite[ i ].color = new Color( 1, 0, 0, 0 );                 // Init back color to transparent
            MonsterSprite[ i ].color = new Color( 0, 0, 0, 0 );                    // Init monster color to transparent
            NestSprite[ i ].color = new Color( 0, 0, 0, 0 );                       // Init Nest color to transparent
            DirtyQuestName[ i ].color = new Color( 0, 0, 0, 0 );
            UpdateFireFx( i );
        }

        for( int i = 0; i < SpawnedMapBonusList.Count; i++ )
        {
            SpawnedMapBonusList[ i ].BonusIcon.color = new Color( 0, 0, 0, 0 );    // Init Bonus Icon color to transparent
            SpawnedMapBonusList[ i ].TextMesh.color = new Color( 0, 0, 0, 0 );     // Init bonus TextMesh color to transparent
        }

        Map.I.UpdateFogOfWar( true );
        Map.I.Tilemap.Layers[ ( int ) ELayerType.GAIA ].gameObject.SetActive( false ); 


        //OpenTrophyGates();
	}	
	public void UpdateIt () 
    {
        if( Manager.I.GameType != EGameType.NAVIGATION ) return;

        UpdateBonusCollecting();

        for( int id = 0; id < Map.I.RM.RMList.Count; id++ )                                                // Updates all quest panels
        {
            UpdateQuestPanel( id );
        }

        if( BumpedQuest != -1 )
        {
            if( BumpedQuest != -1 )
                SelectQuest( BumpedQuest );                                                                 // Select Quest
            else
                SelectQuest( -1 );  
            BumpedQuest = -1;
        }

        if( Input.GetKeyDown( KeyCode.M ) || 
            UI.I.RestartAreaButton.state == UIButtonColor.State.Pressed )                                   // Exit Shortcut
            ExitNavigationMap();

        UpdateDebug();
    }

    public void UpdateDebug()
    {
        if( Helper.I.DebugHotKey == false ) return;
        if( Input.GetMouseButton( 1 ) )
            G.Hero.Control.ApplyMove( new Vector2( -1, -1 ), new Vector2( Map.I.Mtx, Map.I.Mty ) );
    }

    public void UpdateQuestPanel( int id )
    {
        RandomMapData rm = Map.I.RM.RMList[ id ];
        DungeonDialog dd = Map.I.RM.DungeonDialog;
        QuestPanel qp = rm.QuestHelper.QuestPanel;
        qp.gameObject.SetActive( false );
        BackgroundSprite[ id ].gameObject.SetActive( true );

        Map.I.Revealed[ ( int ) rm.MapCord.x,                                                       // new 
                            ( int ) rm.MapCord.y ] = true;

        float alpha = 0;
        if( Map.I.Revealed[ ( int ) rm.MapCord.x,                                                       // Updates Back alpha color
                            ( int ) rm.MapCord.y ] )
        {
            alpha = BackgroundSprite[ id ].color.a + ( Time.deltaTime * .2f );
            if( alpha > BackAlpha ) alpha = BackAlpha;

            float alpham = MonsterSprite[ id ].color.a + ( Time.deltaTime * .4f );                      // Updates Monster alpha color
            if( alpham > 1 ) alpham = 1;
            MonsterSprite[ id ].color = new Color( 1, 1, 1, alpham );
            NestSprite[ id ].color = new Color( 1, 1, 1, alpham );
            DirtyQuestName[ id ].color = new Color( 1, 1, 1, alpham );
        }

        float percn = Item.GetNum( Inventory.IType.Inventory,                                             // Completion Percentage
              ItemType.Adventure_Completion, rm.QuestID );
        string perc = " (" + percn.ToString( "0.#" ) + "%)";
        if( percn <= 0 ) perc = "";
        qp.QuestName.color = Color.white;

        int trophy = dd.GetTrophyCount( id );
        if( trophy >= 1 )                                                                                // ALREADY CONQUERED
        {
            MonsterSprite[ id ].gameObject.SetActive( false );
            NestSprite[ id ].gameObject.SetActive( false );
            DirtyQuestName[ id ].gameObject.SetActive( false );

            List<int> trl = Map.I.RM.RMList[ id ].GetTrophiesAvailable( id );
            qp.gameObject.SetActive( true );
            qp.QuestName.text = rm.QuestHelper.QuestName + perc;
            if( percn >= 100 ) qp.QuestName.color = Color.green;
            for( int t = 0; t < qp.TrophyIcons.Count; t++ )
            {
                int gained = dd.GetTrophyCount( rm.QuestID, ( ETrophyType ) ( t + 1 ) );
                qp.TrophyIcons[ t ].gameObject.SetActive( false );
                qp.TrophyBackgrounds[ t ].gameObject.SetActive( false );

                if( trl[ t ] > 0 )                                                                       // trophy type available?
                {
                    qp.TrophyBackgrounds[ t ].gameObject.SetActive( true );

                    if( gained > 0 )                                                                     // trophy type gained?
                    {
                        qp.TrophyIcons[ t ].gameObject.SetActive( true );
                        qp.TrophyBackgrounds[ t ].gameObject.SetActive( false );
                    }
                }
                else
                    qp.TrophyIcons[ t ].gameObject.transform.parent.gameObject.SetActive( false );
            }
            qp.UpperGrid.Reposition();  // Optimize: running every frame
            qp.BottomGrid.Reposition();
        }
        else
        {                                                                                                // NOT CONQUERED
            UpdateMonsterSprite( rm, id );           

            NestSprite[ id ].transform.localScale = new Vector3( 1.5f, 1.5f, 1 );                       // Nest Sprite Scale
            MonsterSprite[ id ].gameObject.SetActive( true );
            NestSprite[ id ].gameObject.SetActive( true );
            DirtyQuestName[ id ].gameObject.SetActive( true );
            if( Map.I.Revealed[ ( int ) rm.MapCord.x, ( int ) rm.MapCord.y ] == false )
            {
                MonsterSprite[ id ].gameObject.SetActive( false );
                NestSprite[ id ].gameObject.SetActive( false );
                DirtyQuestName[ id ].gameObject.SetActive( false );
            }

            DirtyQuestName[ id ].text = rm.QuestHelper.QuestName + perc;
            DirtyQuestName[ id ].color = Color.white;

            int lv = rm.StartingAdventureLevel + ( int ) Item.GetNum( 
                Inventory.IType.Inventory, ItemType.AdventureLevelPurchase, id );

            if( lv < 1 )                                                                                // tag as unlocked
            {
                DirtyQuestName[ id ].text = rm.QuestHelper.QuestName + "\n(Locked)";
                DirtyQuestName[ id ].color = Color.red;
            }
        }
        Vector2 block = rm.BlockArea;                                                                   // Background Size scale
        Vector2 dim = new Vector2( 20, 20 );
        if( block.x > 1 ) dim.x += 40 * ( block.x - 1 );
        if( block.y > 1 ) dim.y += 40 * ( block.y - 1 );
        BackgroundSprite[ id ].dimensions = dim;

        if( trophy < 1 )
            BackgroundSprite[ id ].color = new Color( 1, 0, 0, alpha );                                  // Dirty Quest: red
        else
            BackgroundSprite[ id ].color = new Color( 0, 1, 0, alpha );                                  // Clear Quest: Green

        if( id == Map.I.RM.CurrentAdventure )
            BackgroundSprite[ id ].color = new Color( 1, .92f, .016f, alpha );                           // Selected quest: Yellow
    }

    public void UpdateMonsterSprite( RandomMapData rm, int id )
    {
        float rotationSpeed = 575f;
        tk2dSprite sp = MonsterSprite[ id ];
        Quaternion qn = Util.GetRotationToPoint( rm.MapCord, G.Hero.Pos );                           // Rotates monsters towards hero
        sp.spriteId = ( int ) rm.QuestHelper.NavigationMapIcon;                                      // Navigation map icon
        bool rot = true;
        if( sp.spriteId == ( int ) ETileType.BOULDER ) rot = false;
        if( sp.spriteId == ( int ) ETileType.RAFT ) rot = false;
        if( sp.spriteId == ( int ) ETileType.TRAP ) rot = false;
        if( sp.spriteId == ( int ) ETileType.FREETRAP ) rot = false;
        if( sp.spriteId == ( int ) ETileType.BARRICADE ) rot = false;
        if( sp.spriteId == ( int ) ETileType.ARROW ) rot = false;
        if( sp.spriteId == ( int ) ETileType.PLAGUE_MONSTER ) rot = false;
        if( rot )
        sp.transform.rotation = Quaternion.RotateTowards(
        sp.transform.rotation, qn, Time.deltaTime * rotationSpeed );

        if( Map.I.RM.CurrentAdventure == id )
        {
            sp.transform.localScale = new Vector3(                                 // Selected Monster animation
            2 + Mathf.PingPong( Time.time * 1, .3f ),
            2 + Mathf.PingPong( Time.time * 1, .3f ), 1 );
        }
        else
        {
            sp.transform.localScale = new Vector3( 1.8f, 1.8f, 1 );                      // Monster Sprite Scale
            if( rot == false )
                sp.transform.localScale = new Vector3( 1.1f, 1.1f, 1 );
        }
    }

    public void UpdateBonusCollecting()
    {
        int id = -1;
        for( int i = 0; i < SpawnedMapBonusList.Count; i++ )
        if ( SpawnedMapBonusList != null && SpawnedMapBonusList[ i ].gameObject.activeSelf )
        {
            if( SpawnedMapBonusList[ i ].MapCord == G.Hero.Pos ) { id = i; }

            if( Map.I.Revealed[ ( int ) SpawnedMapBonusList[ i ].MapCord.x,
                                ( int ) SpawnedMapBonusList[ i ].MapCord.y ] )                             // Updates bonus alpha color
            {
                float alpha = SpawnedMapBonusList[ i ].BonusIcon.color.a + ( Time.deltaTime * .4f );
                if( alpha > 1 ) alpha = 1;
                SpawnedMapBonusList[ i ].BonusIcon.color = new Color( 1, 1, 1, alpha );
                SpawnedMapBonusList[ i ].TextMesh.color = new Color( 1, 1, 1, alpha );
            }
        }

        if( id == -1 ) return;
        float amt = SpawnedMapBonusList[ id ].BonusAmount;
        Item.AddItem( Inventory.IType.Inventory, ( ItemType ) SpawnedMapBonusList[ id ].BonusItem, amt );
        Item it = G.GIT( SpawnedMapBonusList[ id ].BonusItem );
        Message.GreenMessage( it.GetName() + " +" + amt );
        BonusStepList.Add( SpawnedMapBonusList[ id ].MapCord );
        SpawnedMapBonusList[ id ].gameObject.SetActive( false );
        Save();
    }
    public void CreateMapQuests()
    {
        if( MapQuestObjectsCreated ) return;                           // Initializes all quests
        MonsterSprite = new List<tk2dSprite>();
        NestSprite = new List<tk2dSprite>();       
        BackgroundSprite = new List<tk2dSlicedSprite>();
        DirtyQuestName = new List<tk2dTextMesh>();

        for( int i = 0; i < Map.I.RM.RMList.Count; i++ )
        {
            CreateMapQuest( Map.I.RM.RMList[ i ] );
        }      
    }

    public void CreateMapBonuses()
    {
        if( MapQuestObjectsCreated ) return;
        BonusList = new NavigationMapBonus[ Tilemap.width, Tilemap.height ];
        for( int i = 0; i < MapBonusList.Count; i++ )
        {
            CreateMapBonus( MapBonusList[ i ] );
        }

        MapQuestObjectsCreated = true;
    }

    public void CreateMapBonus( NavigationMapBonus bn )
    {
        GameObject instance = ( GameObject ) GameObject.Instantiate( MapBonusPrefab );
        instance.transform.position = new Vector3( bn.MapCord.x, bn.MapCord.y, -1 );
        instance.name = bn.name;
        instance.gameObject.SetActive( true );
        instance.transform.parent = MapBonusFolder.transform;
        NavigationMapBonus newmb = instance.GetComponent<NavigationMapBonus>();
        SpawnedMapBonusList.Add( newmb );
        newmb.Copy( bn );
        if( BonusStepList.Contains( bn.MapCord ) )
            instance.gameObject.SetActive( false );
        BonusList[ ( int ) bn.MapCord.x, ( int ) bn.MapCord.y ] = newmb;
    }
    public void CreateMapQuest( RandomMapData rm )
    {
        GameObject instance = ( GameObject ) GameObject.Instantiate( MapQuestPrefab );
        instance.transform.position = new Vector3( rm.MapCord.x, rm.MapCord.y, 0 );
        instance.name = rm.name;
        instance.gameObject.SetActive( true );
        instance.transform.parent = PrefabFolder.transform;
        RandomMapData newrm = instance.GetComponent<RandomMapData>();
        MonsterSprite.Add( newrm.QuestHelper.MonsterSprite );
        NestSprite.Add( newrm.QuestHelper.NestSprite );
        BackgroundSprite.Add( newrm.QuestHelper.BackgroundSprite );
        DirtyQuestName.Add( newrm.QuestHelper.QuestNameMesh );
        
        GameObject panel = ( GameObject ) GameObject.Instantiate( QuestPanelPrefab );
        panel.transform.position = new Vector3( rm.MapCord.x, rm.MapCord.y, 0 );
        panel.name = "Quest Panel";
        panel.gameObject.SetActive( true );
        panel.transform.parent = instance.transform;
        rm.QuestHelper.QuestPanel = panel.GetComponent<QuestPanel>();   
        GameObject fire = ( GameObject ) GameObject.Instantiate( FirePrefab );
        fire.transform.position = new Vector3( rm.MapCord.x, rm.MapCord.y, 0 );
        fire.name = rm.name + " Fire";
        fire.transform.parent = instance.transform;
        fire.gameObject.SetActive( false );
        rm.QuestHelper.QuestPanel.FireFX = fire;
    }

    public void UpdateFireFx( int id )
    {
        RandomMapData rm = Map.I.RM.RMList[ id ];
        Util.SetActiveRecursively( rm.QuestHelper.QuestPanel.FireFX, false );
        int trophy = Map.I.RM.DungeonDialog.GetTrophyCount( id );
        if( trophy >= 1 )
            Util.SetActiveRecursively( rm.QuestHelper.QuestPanel.FireFX, true );
    }
    public void SelectQuest( int i )
    {
        if( Map.I.RM.RMList[ i ].Available == false )
        {
            Message.RedMessage( "Quest not Yet Available!" );
            return;
        }
        int old = Map.I.RM.CurrentAdventure;
        Map.I.RM.CurrentAdventure = i;
        Helper.I.StartingAdventure = i;
        bool doublebump = false;
        if( i != old )
        {
            RandomMapGoal.ClearPrefabData( old );                                                                    // reset prefab data to keep prefab data clear
            G.Packmule.ClearPackMule();
            TechButton.UpdateMatrix = true;
            TechButton.CreateMatrix();
            MasterAudio.PlaySound3DAtVector3( "Choose Quest", G.Hero.transform.position );                           // Sound FX
            DungeonDialog.UpdateStartingCube = true;
        }

        if( old != -1 && i == old ) doublebump = true;
        int trophy = Map.I.RM.DungeonDialog.GetTrophyCount( i );                                                     // Double bump to exit navigation
        if( trophy >= 1 ) doublebump = false;
        if( Map.I.RM.RMList[ i ].QuestHelper.Signature == "A1_QUEST_1" ) doublebump = false;                         // Disables double bump in the beginning to make it easier

        Map.I.RM.DungeonDialog.ChooseAdventure( Map.I.RM.CurrentAdventure );
        
        if( doublebump ) ExitNavigationMap();
    }
    public void ExitNavigationMap()
    {
        if( G.Hero.Pos == new Vector2( 0, 0 ) ) return;                                                     // to avoid the hero stuck bug
        //if( Map.I.RM.CurrentAdventure == -1 ) return;
        Map.I.Lights.SetTempLight();
        Manager.I.GameType = EGameType.CUBES;
        Map.I.RM.DungeonDialog.gameObject.SetActive( true );
        Map.I.RM.DungeonDialog.ChooseAdventure( Map.I.RM.CurrentAdventure );
        PrefabFolder.SetActive( false );
        UI.I.UpdateInfoPanel( true );
        UI.I.PerksListFolder.SetActive( true );
        Save(); 
        UpdateUI();
    }
    public bool CheckNavigationMap( Vector2 to )
    {
        if( Manager.I.GameType != EGameType.NAVIGATION ) return false;

        for( int i = 0; i < Map.I.RM.RMList.Count; i++ )                                                    // optimize
        {
            RandomMapData rm = Map.I.RM.RMList[ i ];
            Vector2 aux = rm.BlockArea - new Vector2( 1, 1 );
            Vector2 sz = rm.BlockArea - new Vector2( 1, 1 );
            sz = new Vector2( rm.BlockArea.x * 2, rm.BlockArea.y * 2 ) - new Vector2( 1, 1 );
            Rect r = new Rect( rm.MapCord.x - aux.x, rm.MapCord.y - aux.y, sz.x, sz.y );

            if( to == rm.MapCord || r.Contains( to ) )
            {
                BumpedQuest = i;                                                                            // No trophy
                int trophy = Map.I.RM.DungeonDialog.GetTrophyCount( i );                                    // Any trophy?

                if( r.Contains( G.Hero.Pos ) )
                    MasterAudio.PlaySound3DAtVector3( "Fire Ignite", G.Hero.Pos );

                if( trophy < 1 || to == rm.MapCord ) return true;
            }
        }
        return false;
    }
    public void Save( bool first = false )
    {
        if( Manager.I.SaveOnEndGame == false ) return;
        string file = Manager.I.GetProfileFolder() + "/Navigation.NEO";

        using( MemoryStream ms = new MemoryStream() )
        using( BinaryWriter writer = new BinaryWriter( ms ) )                               // Open Memory Stream
        {
            GS.W = writer;                                                                  // Assign BinaryWriter to GS.W for TF

            int Version = Security.SaveHeader( 1 );                                         // Save Header Defining Current Save Version

            Vector2 pos = G.Hero.Pos;
            if( first ) pos = new Vector2( -1, -1 );
            TF.SaveT( "HeroPos", pos );                                                     // Save hero pos
            TF.SaveT( "HeroDir", ( int ) G.Hero.Dir );                                      // Save hero direction
            TF.SaveT( "BonusSteps", BonusStepList );                                        // Save bonus step list

            GS.W.Flush();                                                                   // Flush the writer

            Security.FinalizeSave( ms, file );                                              // Finalize save
        }                                                                                   // using closes the stream automatically
    }

    public void Load()
    {
        string file = Manager.I.GetProfileFolder() + "/Navigation.NEO";

        if( File.Exists( file ) == false ) return;                                         // Return if no file is found

        byte[] fileData = File.ReadAllBytes( file );                                       // Read full file
        byte[] content = Security.CheckLoad( fileData );                                   // Validate HMAC and get clean content

        using( GS.R = new BinaryReader( new MemoryStream( content ) ) )                    // Use MemoryStream for TF
        {
            int SaveVersion = Security.LoadHeader();                                       // Load Header

            Vector2 pos = TF.LoadT<Vector2>( "HeroPos" );                                  // Load hero pos
            if( pos.x == -1 ) G.Hero.Pos = HeroInititialPosition;
            else G.Hero.Pos = pos;

            G.Hero.RotateTo( ( EDirection ) TF.LoadT<int>( "HeroDir" ) );                  // Load hero direction
            BonusStepList = TF.LoadT<List<Vector2>>( "BonusSteps" );                       // Load bonus step list

            GS.R.Close();                                                                  // Close stream
        }

        Map.I.ResetFogOfWar();                                                             // Reset fog of war
    }

    public void OpenTrophyGates()
    {
        List<int> gateid = new List<int>();
        for( int y = 0; y < Tilemap.height; y++ )
        for( int x = 0; x < Tilemap.width; x++ )
        if ( BonusList[ x, y ] != null )
        {
            for( int i = 0; i < Map.I.RM.RMList.Count; i++ )                                                    // optimize
            {
                Vector2 to = new Vector2( x, y );
                RandomMapData rm = Map.I.RM.RMList[ i ];
                Vector2 aux = rm.BlockArea - new Vector2( 1, 1 );
                Vector2 sz = rm.BlockArea - new Vector2( 1, 1 );
                sz = new Vector2( rm.BlockArea.x * 2, rm.BlockArea.y * 2 ) - new Vector2( 1, 1 );
                Rect r = new Rect( rm.MapCord.x - aux.x, rm.MapCord.y - aux.y, sz.x, sz.y );

                if( r.Contains( to ) )
                {
                    int trophy = Map.I.RM.DungeonDialog.GetTrophyCount( i, ETrophyType.BRONZE );                                    // Any trophy?
                    if( BonusList[ x, y ].BonusItem == ItemType.Silver_Trophy )
                        trophy = Map.I.RM.DungeonDialog.GetTrophyCount( i, ETrophyType.SILVER );
                    if( BonusList[ x, y ].BonusItem == ItemType.Gold_Trophy )
                        trophy = Map.I.RM.DungeonDialog.GetTrophyCount( i, ETrophyType.GOLD );
                    if( BonusList[ x, y ].BonusItem == ItemType.Diamond_Trophy )
                        trophy = Map.I.RM.DungeonDialog.GetTrophyCount( i, ETrophyType.DIAMOND );
                    if( BonusList[ x, y ].BonusItem == ItemType.Adamantium_Trophy )
                        trophy = Map.I.RM.DungeonDialog.GetTrophyCount( i, ETrophyType.ADAMANTIUM );
                    if( BonusList[ x, y ].BonusItem == ItemType.Genius_Trophy )
                        trophy = Map.I.RM.DungeonDialog.GetTrophyCount( i, ETrophyType.GENIUS );

                    if( trophy >= BonusList[ x, y ].BonusAmount )
                    if( gateid.Contains( Map.I.GateID[ x, y ] ) == false )
                    {
                        gateid.Add( Map.I.GateID[ x, y ] );
                        BonusList[ x, y ].gameObject.SetActive( false );
                    }
                }
            }          
        }

        for( int y = 0; y < Tilemap.height; y++ )
        for( int x = 0; x < Tilemap.width; x++ )
        {
            if( Map.I.Gaia[ x, y ] )
            if( Map.I.Gaia[ x, y ].TileID == ETileType.ROOMDOOR )
            if( gateid.Contains( Map.I.GateID[ x, y ] ) )
            {
                int group = Map.I.GateID[ x, y ];
                Map.I.SetTile( x, y, ELayerType.GAIA, ETileType.OPENROOMDOOR, true );
                Map.I.GateID[ x, y ] = group;
                //Map.I.TransTilemapUpdateList.Add( new VI( x, y ) );
            }
        }
    }
    public void UpdateUI()
    {
        UI.I.NavigationMapUI.SetActive( false );
        UI.I.PerksListFolder.SetActive( true );
 
        if( Manager.I.GameType == EGameType.NAVIGATION )
        {
            UI.I.PerkInfoIconBack.gameObject.SetActive( false );
            UI.I.NavigationMapUI.SetActive( true );
            UI.I.PerkInfoTitleText.text = "Trophy Progress:";
            UI.I.ArtifactInfoLabel.text = "          Navigation Map";
            DungeonDialog dd = Map.I.RM.DungeonDialog;
            UI.I.PerkInfoDescriptionText.text = "";
            UI.I.PerksListFolder.gameObject.SetActive( false );
            dd.UpdateTrophiesPanel();

            // using Plethora ID for the calculation. Careful if you change id
            int id = 2;
            UI.I.NaviBronzeLabel.text = "" + dd.Bronze + "/" + Map.I.RM.RMList[ id ].RequiredBronzeTrophies;
            UI.I.NaviSilverLabel.text = "" + dd.Silver + "/" + Map.I.RM.RMList[ id ].RequiredSilverTrophies;
            UI.I.NaviGoldLabel.text = "" + dd.Gold + "/" + Map.I.RM.RMList[ id ].RequiredGoldTrophies;
            int tot = Map.I.RM.RMList[ id ].RequiredBronzeTrophies + Map.I.RM.RMList[ id ].RequiredSilverTrophies +
                Map.I.RM.RMList[ id ].RequiredGoldTrophies;
            int num = dd.Bronze + dd.Silver + dd.Gold;
            float per = Util.GetPercent( num, tot );
            UI.I.QuestCompletitionBar.value = per;
            UI.I.QuestCompletitionLabel.text = "Trophy Goal to Beat the Demo: " + per.ToString("0.") + "%";
        }
    }
}
