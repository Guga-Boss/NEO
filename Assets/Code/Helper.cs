using UnityEngine;
using System;
using System.Collections;
using Sirenix.OdinInspector;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Helper : MonoBehaviour
{
    public static Helper I;
    [TabGroup( "Main" )]
    public bool ReleaseVersion;
    [TabGroup( "Start" )]
    public int StartingAdventure = 0;
    [TabGroup( "Main" )]
    public bool FreePlay = false;
    [TabGroup( "Main" )]
    public bool PlayMusic = false;
    [TabGroup( "Start" )]
    public int StartingCube = -1;
    [TabGroup( "Start" )]
    public bool StartFromLastEditedCube = false;
    [TabGroup( "Main" )]
    public bool SaveDataOnExit = true;
    [TabGroup( "Main" )]
    public bool FastFarm = false;
    [TabGroup( "Debug" )]
    public bool ShowDebugText, ShowDebugHeaderText;
    [TabGroup( "Debug" )]
    public float FloatVal1;
    [TabGroup( "Debug" )]
    public float FloatVal2;
    [TabGroup( "Debug" )]
    public float FloatVal3;
    [TabGroup( "Debug" )]
    public float FloatVal4;
    [TabGroup( "Debug" )]
    public bool IgnoreTutorial;
    [TabGroup( "Debug" )]
    public bool AutoClickPlayButton;
    [TabGroup( "Start" )]
    public bool StartAtCubes;
    [TabGroup( "Start" )]
    public bool StartAtFarm;
    [TabGroup( "Debug" )]
    public bool DebugHotKey;
    [TabGroup( "Debug" )]
    public bool TestTechTree = false;
    [TabGroup( "Debug" )]
    public bool ShowCameraDebugText = false;
    [TabGroup( "Other" )]
    public bool InvunerableHero;
    [TabGroup( "Other" )]
    public bool EnablePathFindingMovement;
    [TabGroup( "Other" )]
    public bool FreeBarricadeDestroy = false;
    [TabGroup( "Other" )]
    public bool FreeGateOpen = false;
    [TabGroup( "Other" )]
    public bool AutoRestartAfterCubeDeath = false;
    [TabGroup( "Other" )]
    public bool ForceRealtime = false;
    [TabGroup( "Other" )]
    public bool ForceStepping = false;
    [TabGroup( "Main" )]
    public bool AutoOpenGateAtStart = false;
    [TabGroup( "Main" )]
    public bool AutoWayPointJump = false;
    [TabGroup( "Main" )]
    public bool PathfindMoveForWayPointJump = true;
    [TabGroup( "Main" )]
    public bool ShowGaiaGrid = false;
    [TabGroup( "Debug" )]
    public MyBool ForceFlipX = MyBool.DONT_CHANGE;
    [TabGroup( "Debug" )]
    public MyBool ForceFlipY = MyBool.DONT_CHANGE;
    [TabGroup( "Other" )]
    public BoxCollider2D Colliders;
    [TabGroup( "Other" )]
    [Range( 0, 10 )]
    public float RealtimeSpeedFactor = 1;
    [TabGroup( "Debug" )]
    public tk2dSprite HelperMark1, HelperMark2, HelperMark3;
    [TabGroup( "Debug" )]
    public string TileMapCord;
    [TabGroup( "Debug" )]
    public Vector2 TileMapCordVector;
    [TabGroup( "Debug" )]
    public int FrameRateLimit = -1;
    [TabGroup( "Other" )]
    public string ModDescriptionList;
    [TabGroup( "Debug" )]
    public Color TestColor;
    public ItemType ResourceType = ItemType.NONE;
    public float ResourceToAddAmount = 10;
    public float ResourceToRemoveAmount = -1;
    [TabGroup( "Other" )]
    public GameObject[ ] ShortcutList;
    [TabGroup( "Debug" )]
    public ETileType DrawTile;

	void Start () 
    {
        I = this;

        if( Application.platform == RuntimePlatform.WindowsPlayer ) ReleaseVersion = true;

        if( ReleaseVersion )
        {
            HelperMark1.gameObject.SetActive( false );
            HelperMark2.gameObject.SetActive( false );
            HelperMark3.gameObject.SetActive( false );
            StartingAdventure = -1;
            ShowDebugText = false;
            ShowDebugHeaderText = false;
            IgnoreTutorial = false;
            AutoClickPlayButton = false;
            PlayMusic = true;
            StartAtCubes = false;
            SaveDataOnExit = true;
            StartAtFarm = false;
            AutoRestartAfterCubeDeath = true;
            FreePlay = false;                                           
            StartFromLastEditedCube = false;
            DebugHotKey = false;
            AutoOpenGateAtStart = false;
            AutoWayPointJump = false;
            PathfindMoveForWayPointJump = true;
            TestTechTree = false;
            InvunerableHero = false;
            EnablePathFindingMovement = false;
            FreeBarricadeDestroy = false;
            FreeGateOpen = false;
            ForceRealtime = false;
            ForceStepping = false;
            StartingCube = -1;
            FrameRateLimit = -1;
            ForceFlipX = MyBool.DONT_CHANGE;
            ForceFlipY = MyBool.DONT_CHANGE;
            ShowCameraDebugText = false;
            FastFarm = false;
        }
        else
        {
            FreeBarricadeDestroy = false;
            FreeGateOpen = true; 
            //AutoOpenGateAtStart = true;
            //AutoWayPointJump = true;
            PathfindMoveForWayPointJump = false;
            AutoRestartAfterCubeDeath = true;
            FreePlay = true;
        }

        if( Application.platform == RuntimePlatform.WindowsEditor )
        {
            FastFarm = true;
            DebugHotKey = true;
        }

        if( AutoOpenGateAtStart == false )                    // to avoid bugs
            AutoWayPointJump = false;
    } 
	
    public void DrawDebugText()
    {
        if( Map.I.RM.HeroSector == null ) return;             // added to prevent new bug
        Map m = Map.I;      
        
        if( Input.GetKeyDown( KeyCode.F1 ) )                                                                                          // show last vault bonus targets                                                            
        {
            for( int i = 0; i < Mine.VaultEffTargetList.Count; i++ )
                Controller.CreateMagicEffect( Mine.VaultEffTargetList[ i ] );                                                         // Magic FX  
        }

        if( Input.GetKey( KeyCode.F1 ) )
        {
            UI.I.DebugLabel.text = Manager.I.Translate( "&SCROLL 70 15", "Navigation Map" );                                         // F1 show master shortcut list. be careful not to change the scroll position
            return;
        }

        if( Input.GetKeyDown( KeyCode.F2 ) )
        {
            ShowDebugHeaderText = !ShowDebugHeaderText;
            if( ShowDebugHeaderText == false )
                UI.I.DebugLabel.text = "";
        }

        if( m.Unit == null ) return;
        Unit un = null;

        UI.I.DebugLabel.text = "";
        if( ShowDebugHeaderText )
        {
            if( Map.I.Deb != "" )
            {
                UI.I.DebugLabel.text = "Debug: " + Map.I.Deb + "\n";
                return;
            }
            UI.I.DebugLabel.text = "Press F2 to Show Debug Text.";
            UI.I.DebugLabel.text += "\nMouse Over Unit for more Info.";
            UI.I.DebugLabel.text += "\n\nFPS: " + m.FPS + " Av: " + ( int ) m.AverageFPS;
            m.FPSSum += ( int ) m.FPS;
            m.FPSSumCount ++;
            m.AverageFPS = m.FPSSum / m.FPSSumCount;

            Vector2 mt = GetCubeTile( Map.GM() );
            if( ( int ) mt.x >= 0 && ( int ) mt.x < Brain.sizeX )
            if( ( int ) mt.y >= 0 && ( int ) mt.y < Brain.sizeY )
            if( Brain.dist != null)
                UI.I.DebugLabel.text += "\nBrain: " + Brain.dist[ ( int ) mt.x, ( int ) mt.y ];

            UI.I.DebugLabel.text += "\nRecord Time: " + Util.ToSTime( m.RecordTime );
            //UI.I.DebugLabel.text += "\nfloor " + G.Hero.Control.BridgeFloor;
            if( Cursor.visible == false ) return;
            if( m.Mtx == -1 || m.Mty == -1 ) return;
            UI.I.DebugLabel.text += "\nMouse Tile " + m.Mtx + " " + m.Mty; // +" World pos: " + Input.mousePosition.x + " " + Input.mousePosition.y + " Screen pos: " + MouseCord.x + " " + MouseCord.y + "\n";

            if( Manager.I.GameType == EGameType.CUBES )
                if( Map.I.RM.HeroSector.Type == Sector.ESectorType.NORMAL )
                    UI.I.DebugLabel.text += "\nCube Tile: " + GetCubeTile( Map.GM() );

            UI.I.DebugLabel.text += "\n\nPlay Time: " + Util.ToSTime( ( double ) Manager.I.PlayTime ) + "\nSession Time: " + Util.ToSTime( ( double ) Map.I.SessionTime );
            UI.I.DebugLabel.text += "\nGame Total Time: " + Util.ToSTime( ( double ) Manager.I.TotalPlayTime ) + "\nCube Total Time: " + Util.ToSTime( ( double ) Manager.I.CubesTotalTime ) + "\n";
            if( Map.I.CamDataID >= 0 ) UI.I.DebugLabel.text += "\nCamera Data ID: " + Map.I.CamDataID;
            //UI.I.DebugLabel.text += "Area ID: " + Map.I.AreaID[ m.Mtx, m.Mty ] + "\n";
        }

        if( m.Mtx == -1 || m.Mty == -1 ) return;
        Unit raft = Controller.GetRaft( new Vector2( m.Mtx, m.Mty ) );
        if( raft ) un = raft;
        else
        if( m.Unit[ m.Mtx, m.Mty ] ) un = m.Unit[ m.Mtx, m.Mty ];
        else
        if( m.Gaia2[ m.Mtx, m.Mty ] ) un = m.Gaia2[ m.Mtx, m.Mty ];

        List<Unit> fl = Map.I.GetFUnit( new Vector2( m.Mtx, m.Mty ) );
        if( fl != null )
            for( int i = 0; i < fl.Count; i++ )
            {
                if( fl[ i ].ValidMonster )
                    if( fl[ i ].TileID != ETileType.WASP ) un = fl[ i ];
            }    

        if( un == null )
        if( m.Gaia[ m.Mtx, m.Mty ] ) un = m.Gaia[ m.Mtx, m.Mty ];

        if( un == null ) return;

        if( un.Control )
        {
            if( Cursor.visible ) 
            if( un.Control.Resting )
                if( un.Control.RestingRadiusSprite )
                    un.Control.RestingRadiusSprite.color = new Color( 1, 0, 0, .3f );
        }

        if( ShowDebugHeaderText == false )
        if(!ShowDebugText ) return;
        if( Manager.I.Status != EGameStatus.PLAYING ) return;

        Vector2 mp = new Vector2( m.Mtx, m.Mty );
     
        Quest.I.UpdateArtifactMouseOverInfo( new Vector2( ( int ) m.Mtx, ( int ) m.Mty ) );

        if( BluePrintWindow.I.gameObject.activeSelf ) return;
        if( Manager.I.GugaVersion ) UI.I.DebugLabel.text = "Debug Mode, Guga Version:\n";
        if( Map.PtOnMap( Map.I.Tilemap, mp ) == false ) return;

        ETileType terr = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile(
                  Map.I.Mtx, Map.I.Mty, ( int ) ELayerType.TERRAIN );
        ETileType dec = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile(
                                      Map.I.Mtx, Map.I.Mty, ( int ) ELayerType.DECOR );
        ETileType dec2 = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile(
                              Map.I.Mtx, Map.I.Mty, ( int ) ELayerType.DECOR2 );

        ETileType mod = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile(
                              Map.I.Mtx, Map.I.Mty, ( int ) ELayerType.MODIFIER );

        ETileType ga = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile(
                       Map.I.Mtx, Map.I.Mty, ( int ) ELayerType.GAIA );

        ETileType ga2 = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile(
               Map.I.Mtx, Map.I.Mty, ( int ) ELayerType.GAIA2 );

        ETileType mn = ( ETileType ) Quest.I.Dungeon.Tilemap.GetTile(
               Map.I.Mtx, Map.I.Mty, ( int ) ELayerType.MONSTER );

        //UI.I.DebugLabel.text += "Terrain: " + terr + " Decor: " + dec + " Decor2: " + dec2 + "  Mod: " + mod + "  Gaia: " + ga + "  Gaia2: " + ga2 + "  Monster: " + mn + "\n";

        //if( Map.I.Revealed[ Map.I.Mtx, Map.I.Mty ] ) UI.I.DebugLabel.text += "Tile Revealed\n"; else UI.I.DebugLabel.text += "Tile NOT Revealed\n";

        if( Cursor.visible == false ) return;
        //UI.I.DebugLabel.text += MyPathfind.I.UpdateDebug();
        //if( Helper.I.ReleaseVersion ) return;

        //UI.I.DebugLabel.text += "\nDebug Mode\n";
        //UI.I.DebugLabel.text += "Game Object: " + un.gameObject.name + "\n";
        UI.I.DebugLabel.text += "\n\nUnit Name: " + un.TileID;

        //if( m.Mtx >= 0 && Map.I.GateID != null )
        //    UI.I.DebugLabel.text += "\nID: " + Map.I.GateID[ m.Mtx, m.Mty ] + " dont forget to remove this";

        UI.I.DebugLabel.text += "\nUnit Type: " + un.UnitType + "\n";
        //UI.I.DebugLabel.text += "Direction: " + un.Dir + "\n";        
        //if( Map.I.PondID != null )
        //    UI.I.DebugLabel.text += "Pond ID: " + Map.I.PondID[ m.Mtx, m.Mty ] + "\n";

        //if( Map.I.ContinuousPondID != null )
        //    UI.I.DebugLabel.text += "Continuous Pond ID: " + Map.I.ContinuousPondID[ m.Mtx, m.Mty ] + "\n";

        //if( Map.I.MudPoolID != null )
        //    UI.I.DebugLabel.text += "Mud Pool ID: " + Map.I.MudPoolID[ m.Mtx, m.Mty ] + "\n";

        //UI.I.DebugLabel.text += "Variation: " + un.Variation + "\n";
        //UI.I.DebugLabel.text += "SaveData: " + un.SaveData + "\n";
        //UI.I.DebugLabel.text += "UseTransTile: " + un.UseTransTile + "\n";
        //UI.I.DebugLabel.text += "Block Movement: " + un.BlockMovement + "\n";
        //if( raft ) UI.I.DebugLabel.text += "Raft Group ID: " + un.Control.RaftGroupID + "\n";

        if( un.Body )
        {
            if( un.ValidMonster )
            {
                if( un.Body.TotHp > 0 )
                    UI.I.DebugLabel.text += "\nHP: " + un.Body.Hp.ToString( "0.0" ) + " of " + un.Body.TotHp.ToString( "0.0" ) + "\n";
                UI.I.DebugLabel.text += "\nLives: " + un.Body.Lives + "\n";
                if( un.Control.IsFlyingUnit )
                {
                    UI.I.DebugLabel.text += "\nFlight Speed Factor: " + un.Control.FlightSpeedFactor + "%\n";
                    UI.I.DebugLabel.text += "\nFlight Speed: " + un.Control.FlyingSpeed + " Tiles per sec.\n";
                }
            }
        }
        if( un.Control )
        {
            if( un.ValidMonster )
            {
                if( un.Control.TickBasedMovement &&
                    un.Control.TickMoveList.Count > 0 )
                {
                    UI.I.DebugLabel.text += "Tic Tac Movement on Beat: ";
                    for( int i = 0; i < un.Control.TickMoveList.Count; i++ )
                         UI.I.DebugLabel.text += " " + un.Control.TickMoveList[ i ];
                }
                else
                    if( un.Control.IsFlyingUnit == false )
                        UI.I.DebugLabel.text += "Movement Speed: " + un.Control.GetMonsterRTMovSpeed().ToString( "0.#" ) + " steps per 10 sec.\n";
            }
        }

        if( un.MeleeAttack )
        {
            UI.I.DebugLabel.text += "\nMelee Damage: " + un.MeleeAttack.TotalDamage.ToString( "0.#" ) + "\n";
            UI.I.DebugLabel.text += "Att Speed: " + un.MeleeAttack.GetRealtimeSpeed().ToString( "0.#" ) + " hits per 10 sec.\n"; 
        }
        if( un.RangedAttack )
        {
            UI.I.DebugLabel.text += "\nRanged Damage: " + un.RangedAttack.TotalDamage.ToString( "0.#" ) + "\n";
            UI.I.DebugLabel.text += "Att Speed: " + un.RangedAttack.GetRealtimeSpeed().ToString( "0.#" ) + " hits per 10 sec.\n"; 
        }
        if( un.TileID == ETileType.MOTHERWASP )
        {
            if( un.Md.ShieldedWaspChance > 0 )
                UI.I.DebugLabel.text += "\nShielded Wasp Chance: " + un.Md.ShieldedWaspChance.ToString( "0.#" ) + "%";
            if( un.Md.CocoonWaspChance > 0 )
                UI.I.DebugLabel.text += "\nCocoon Wasp Chance: " + un.Md.CocoonWaspChance.ToString( "0.#" ) + "%";
            if( un.Md.EnragedWaspChance > 0 )
                UI.I.DebugLabel.text += "\nEnraged Wasp Chance: " + un.Md.EnragedWaspChance.ToString( "0.#" ) + "%";
            if( un.Md.EnragedWaspExtraSpawns != 0 )
                UI.I.DebugLabel.text += "\nEnraged Wasp Extra Spawn: " + un.Md.EnragedWaspExtraSpawns.ToString( "0.#" );
            if( un.Md.EnragedWaspSpawnLimitAdd != 0 )
                UI.I.DebugLabel.text += "\nEnraged Wasp Extra Spawn Limit: " + un.Md.EnragedWaspSpawnLimitAdd.ToString( "0.#" );
            if( un.Md.EnragedWaspSpawnPercentAdd != 0 )
                UI.I.DebugLabel.text += "\nEnraged Wasp Extra Spawn Speed: " + un.Md.EnragedWaspSpawnPercentAdd.ToString( "0.#" ) + "%";
            if( un.Md.EnragedWaspExtraBreedingSpeed != 0 )
                UI.I.DebugLabel.text += "\nEnraged Wasp Extra Temp Spawn Speed:  " + un.Md.EnragedWaspExtraBreedingSpeed.ToString( "0.#" ) + "%";
        }

        if( un.TileID == ETileType.QUEROQUERO )
        {
            UI.I.DebugLabel.text += "\nMin Damage: " + un.Body.QQMinDamage.ToString( "0.#" );
            UI.I.DebugLabel.text += "\nMax Damage: " + un.Body.QQMaxDamage.ToString( "0.#" );
            UI.I.DebugLabel.text += "\nHero Miss Damage: " + un.Body.QQMissHeroDamage.ToString( "0.#" );
            UI.I.DebugLabel.text += "\nDamage Curve: " + un.Md.QQDamageCurve.ToString( "0.#" );
        }

        if( un.TileID == ETileType.RAFT )
            UI.I.DebugLabel.text += "\nRaft ID: " + un.Control.RaftGroupID + "\n";
        if( un.TileID == ETileType.BARRICADE )
            UI.I.DebugLabel.text += "\nSize: " + ( un.Variation + 1 ) + "\n";
        if( un.TileID == ETileType.ALTAR )
        {
            UI.I.DebugLabel.text += "\n" + Manager.I.Translate( "&ALTAR_DESCRIPTION", "Main" );
        }
    }

    public Vector2 GetCubeTile( Vector2 tg )
    {
        if( Manager.I.GameType == EGameType.CUBES )
            return new Vector2( tg.x - Map.I.RM.HeroSector.Area.xMin,
                                tg.y - Map.I.RM.HeroSector.Area.yMin );
        else return new Vector2( -1, -1 );
    }
    #region Buttons
    #if UNITY_EDITOR
    [ButtonGroup( "1", 1 )]
    // [HorizontalGroup( "Split", 0.5f )]
    [Button( "Edit Quest", ButtonSizes.Large ), GUIColor( 1f, 0.52f, 0.1f )]
    public void EditQuestCallBack()
    {
        MapSaver ms = GameObject.Find( "Areas Template Tilemap" ).GetComponent<MapSaver>();
        ms.EditQuestDataCallBack();
    }

    [ButtonGroup( "2", 2 )]
    //[HorizontalGroup( "Split", 0.5f )]
    [Button( "Goto Resource", ButtonSizes.Large ), GUIColor( 1f, 1f, 0 )]
    public void GotoResourceCallBack()
    {
        if( ResourceType == ItemType.NONE )
        {
            Debug.Log( "Choose a resource first." );
            return;
        }
        GameObject inv = GameObject.Find( "Inventory" );
        Inventory inve = inv.GetComponent<Inventory>();
        if( inve == null ) Debug.LogError( "No inventory obj found" );

        Selection.activeGameObject =
        inve.ItemList[ ( int ) ResourceType ].gameObject;
    }

    [ButtonGroup( "2", 2 )]
    //[HorizontalGroup( "Split", 0.5f )]
    [Button( "Add Resource", ButtonSizes.Large ), GUIColor( 0, 1f, 0 )]
    public void AddResourceCallBack()
    {
        if( ResourceType == ItemType.NONE ) 
        {
            Debug.Log( "Choose a resource first." );
            return;
        }
        if( Application.isPlaying == false ) return;
        Item.AddItem( Inventory.IType.Inventory, ResourceType, ResourceToAddAmount );
    }

    [ButtonGroup( "2", 2 )]
    //[HorizontalGroup( "Split", 0.5f )]
    [Button( "Remove Resource", ButtonSizes.Large ), GUIColor( 1f, 0, 0 )]
    public void RemoveResourceCallBack()
    {
        if( ResourceType == ItemType.NONE )
        {
            Debug.Log( "Choose a resource first." );
            return;
        }
        if( Application.isPlaying == false ) return;
        Item.AddItem( Inventory.IType.Inventory, ResourceType, ResourceToRemoveAmount );
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Hero", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoHeroCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 0 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Map", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoMapCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 1 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Navigation Map", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoNavigationCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 2 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Manager", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoManagerCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 3 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Random Map", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoRandomMapCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 4 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Hero Data", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoHeroDataCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 5 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "BluePrints", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoBlueprintsCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 6 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Buildings", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoBuildingsCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 7 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Farm", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoFarmCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 8 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Dialog", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoDialogCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 9 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Tutorial", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoTutorialCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 10 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Quests Panel", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoQuestsCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 11 ];
    }

    [VerticalGroup( "3", 3 )]
    [Button( "Object Pooling", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoPoolingCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 12 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "UI", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoUICallBack()
    {
        Selection.activeGameObject = ShortcutList[ 13 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Quest", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoQuestCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 14 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Master Audio", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoStatisticsCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 15 ];
    }
    [VerticalGroup( "3", 3 )]
    [Button( "Inventory", ButtonSizes.Small ), GUIColor( .5f, .5f, .5f )]
    public void GotoInventoryCallBack()
    {
        Selection.activeGameObject = ShortcutList[ 16 ];
    }

    [TabGroup( "Other" )]
    public int QuestCopySource = -1;
    [TabGroup( "Other" )]
    public int QuestCopyDestination = -1;


    //[ButtonGroup( "1", 1 )]
    [TabGroup( "Other" )]
    // [HorizontalGroup( "Split", 0.5f )]
    [Button( "Copy Quest", ButtonSizes.Large ), GUIColor( 1f, 0.52f, 0.1f )]
    public void CopyQuestCallBack()
    {
        if( QuestCopyDestination < 0 ) return;
        if( QuestCopySource < 0 ) return;
        Map.I.RM.RMList[ QuestCopyDestination ].Copy( Map.I.RM.RMList[ QuestCopySource ], false );
        Debug.Log( "Quest Copied " + " Source: #" + QuestCopySource + " " + Map.I.RM.RMList[ QuestCopySource ].name + 
        " Destination: #" + QuestCopyDestination + " " + Map.I.RM.RMList[ QuestCopyDestination ].name );
        QuestCopyDestination = -1;
        QuestCopySource = -1;
    }

    #endif
    #endregion

    public string GetModDescriptionText()
    {
        RandomMap rm = GameObject.Find( "----------------Random Map----------------" ).
        GetComponent<RandomMap>();
        MapSaver ms = GameObject.Find( "Areas Template Tilemap" ).
        GetComponent<MapSaver>();
        if( ms.CurrentAdventure < 0 || ms.CurrentAdventure >= rm.RMList.Count ) 
            return "Quest Unavailable";
        string ls = "";
        RandomMapData rmd = rm.RMList[ ms.CurrentAdventure ];
        SectorDefinition[ ] sd = rmd.gameObject.GetComponentsInChildren<SectorDefinition>();
        for( int s = 0; s < sd.Length; s++ )
        {
            for( int m = 0; m < sd[ s ].ModList.Length; m++ )
            {
                if( sd.Length > 1 )
                    ls += "(SD:" + s + ") ";
                ls += sd[ s ].ModList[ m ].name + "\n";
            }
        }
        return ls;
    }
    public string GetOriListText()
    {
        string str = "";
        MapSaver ms = MapSaver.Get();
        RandomMap rm = GameObject.Find( "----------------Random Map----------------" ).GetComponent<RandomMap>();
        RandomMapData rmd = rm.RMList[ ms.CurrentAdventure ];
        SectorDefinition[] sd = rmd.gameObject.GetComponentsInChildren<SectorDefinition>();
        str += "\n\n";
        for( int s = 0; s < sd.Length; s++ )
        {
            for( int m = 0; m < sd[ s ].ModList.Length; m++ )
            {
                Mod md = sd[ s ].ModList[ m ];
                str += "\nBEGIN_" + md.ModNumber + "\n";
                str += "" + md.name + "\n";
                str += "\n\n";
                if( md.OrientatorEffects != null )
                {
                    for( int i = 0; i < md.OrientatorEffects.Count; i++ )
                    {
                        if( i == 0 ) str += "Effect:\n\n";
                        str += "  " + ( i + 1 ) + "= " + md.OrientatorEffects[ i ] + ",";
                        if( i == 9 ) str += "\n";
                    }
                str += "\n\n";
                }
                if( md.OrientatorTable1 != null )
                {
                    for( int i = 0; i < md.OrientatorTable1.Count; i++ )
                    {
                        if( i == 0 ) str += "Ori 1: -----" + md.OrientatorEffects[ 0 ] + "-----\n\n";
                        str += "  " + i + "=" + md.OrientatorTable1[ i ] + ",";
                        if( i == 9 ) str += "\n";
                    }
                str += "\n\n";
                }
                if( md.OrientatorTable2 != null )
                {
                    for( int i = 0; i < md.OrientatorTable2.Count; i++ )
                    {
                        if( i == 0 ) str += "Ori2: -----" + md.OrientatorEffects[ 1 ] + "-----\n\n";
                        str += "  " + i + "=" + md.OrientatorTable2[ i ] + ",";
                        if( i == 9 ) str += "\n";
                    }
                str += "\n\n";
                }
                if( md.OrientatorTable3 != null )
                {
                    for( int i = 0; i < md.OrientatorTable3.Count; i++ )
                    {
                        if( i == 0 ) str += "Ori3: -----" + md.OrientatorEffects[ 2 ] + "-----\n\n";
                        str += "  " + i + "=" + md.OrientatorTable3[ i ] + ",";
                        if( i == 9 || i == 19 ) str += "\n";
                    }
                str += "\n\n";
                }
                if( md.OrientatorTable4 != null )
                {
                    for( int i = 0; i < md.OrientatorTable4.Count; i++ )
                    {
                        if( i == 0 ) str += "Ori4: -----" + md.OrientatorEffects[ 3 ] + "-----\n\n";
                        str += "  " + i + "=" + md.OrientatorTable4[ i ] + ",";
                        if( i == 9 || i == 19 ) str += "\n";
                    }
                str += "\n\n";
                }
                if( md.OrientatorItemTable1 != null )
                {
                    for( int i = 0; i < md.OrientatorItemTable1.Count; i++ )
                    {
                        if( i == 0 ) str += "Item List:\n\n";
                        str += " " + i + "  " + md.OrientatorItemTable1[ i ] + ",";
                        if( i == 9 ) str += "\n";
                    }
                str += "\n\n";
                }
                if( md.InitialAltarBonusList != null )
                {
                    for( int i = 0; i < md.InitialAltarBonusList.Count; i++ )
                    {
                        if( i == 0 ) str += "Altar Bonus List:\n\n";
                        str += " " + i + "  " + md.InitialAltarBonusList[ i ].AltarBonusType + ",";
                        if( i == 9 ) str += "\n";
                    }
                    str += "\n\n";
                    str += "Scope: ";
                    foreach( string name in Enum.GetNames( typeof( EAltarBonusScope ) ) )
                    {
                        int value = ( int ) Enum.Parse( typeof( EAltarBonusScope ), name );
                        str += "   " + value + "  " + name + "   ";
                    }
                }
                str += "END_" + md.ModNumber + "\n";
            }
        }
        return str;
    }


    // draw an x mark over a target for debug purposes
    public void DrawMark( Vector2 pt )
    {
        Message.CreateMessage( ETileType.NONE, "x", pt, Color.green, true, true, 15, 0, -1 );
    }
    internal void UpdateDebug()
    {
        if( Input.GetKey( KeyCode.LeftShift ) )                                                              // Update language cache
        if( Input.GetKey( KeyCode.F1 ) )
                Language.I.UpdateLanguage();

        if( Input.GetMouseButtonDown( 1 ) )
        if( Input.GetKey( KeyCode.Insert ) == false ) 
        {
            float time = 10;
            if( Input.GetKey( KeyCode.F1 ) )
                time = 1000000;
            Message.CreateMessage( ETileType.NONE, ItemType.NONE, "" + Map.I.Mtx + "," + 
            Map.I.Mty, new Vector2( Map.I.Mtx - .3f, Map.I.Mty ),                                            // right click to show cord
            Color.green, false, false, time, 0, -1, 70 );
        }
        
        if( G.HS == null || G.HS.Type != Sector.ESectorType.NORMAL ) return;
        if( Manager.I.GugaVersion == false ) return;                                                         // Below this line, Just for me, baby!
        Vector2 tg = new Vector2( Map.I.Mtx, Map.I.Mty );

        if( Input.GetKeyDown( KeyCode.F12 ) )                                                                // F12: play random song 
            Manager.I.PlaylistController.PlayRandomSong();

        if( Input.GetKey( KeyCode.Insert ) )                                                                 // Mini editor
        {
            if( Input.GetMouseButton( 1 ) )
            {
                TKUtil.ClearLayer( tg, ELayerType.GAIA );
                Vector2 mc = GetCubeTile( tg );
                MapSaver.I.Tilemap.SetTile( ( int ) mc.x, ( int ) mc.y, ( int ) ELayerType.GAIA, ( int ) -1 );
                return;
            }

            if( Input.GetKeyDown( KeyCode.Insert ) )
            if( Input.GetMouseButton( 0 ) == false ) 
            {
                 Unit un = Map.I.GetUnit( tg, ELayerType.GAIA );           
                 if( un )
                 {
                     DrawTile = un.TileID;                                                                   // Select gaia tile with insert key
                     Message.CreateMessage( DrawTile, "", tg, Color.white );
                 }
            }

            if( Input.GetMouseButton( 0 ) ) 
            {
                Quest.I.CurLevel.Tilemap.SetTile( Map.I.Mtx, Map.I.Mty, ( int ) ELayerType.GAIA, ( int ) DrawTile );
                Vector2 mc = GetCubeTile( tg );
                MapSaver.I.Tilemap.SetTile( ( int ) mc.x, ( int ) mc.y, ( int ) ELayerType.GAIA, ( int ) DrawTile );
                Map.I.SetTile( Map.I.Mtx, Map.I.Mty, ELayerType.GAIA, DrawTile, true );
                for( int y = ( int ) tg.y - 1; y <= tg.y + 1; y++ )
                for( int x = ( int ) tg.x - 1; x <= tg.x + 1; x++ )
                    Map.I.TransTilemapUpdateList.Add( new VI( x, y ) );
            }

            if( Input.GetMouseButton( 2 ) )
            {
                MapSaver.I.SaveMap( MapSaver.I.LastLoadedFile, ref MapSaver.I.Tilemap );
                Debug.Log( "Saved: " + MapSaver.I.LastLoadedFile );
            }
            return;
        }

        if( Input.GetMouseButtonDown( 2 ) )
            G.Hero.Control.ApplyMove( new Vector2( -1, -1 ), tg );          // mouse click move hero

        if( Input.GetKey( KeyCode.Delete ) )
        {
            if( Input.GetMouseButton( 0 ) )
            {
                Unit un = Map.I.GetUnit( tg, ELayerType.MONSTER );          // localized kill: use mouse button + Delete
                if( un ) Map.Kill( un );
                Unit ga2 = Map.I.GetUnit( tg, ELayerType.GAIA2 );
                if( ga2 ) Map.Kill( ga2 );
                List<Unit> fl = Map.I.GetFUnit( tg );
                if( fl != null )
                for( int i = 0; i < fl.Count; i++ )
                    Map.Kill( fl[ i ] );
                return;
            }

            if( Input.GetMouseButton( 2 ) )
            {
                for( int i = 0; i < G.HS.MoveOrder.Count; i++ )                                                   // Delete: Kill all debug
                if( G.HS.MoveOrder[ i ].ValidMonster )
                    G.HS.MoveOrder[ i ].Kill();
                for( int i = G.HS.Fly.Count - 1; i >= 0; i-- )
                    G.HS.Fly[ i ].Kill();  
            }
        }
    }
}
