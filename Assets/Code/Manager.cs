using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DarkTonic.MasterAudio;
using System;
#if UNITY_EDITOR  
using UnityEditor;
#endif

public enum EGameStatus
{
	NONE = -1, PLAYING, MAINMENU, LEVELINTRO, ENDING, CREDITS
}
public enum EGameType
{
    NONE = -1, NORMAL, CUBES, FARM, NAVIGATION
}

public class Manager : MonoBehaviour
{
    #region Variables
    public static Manager I;
    public Quest ActiveQuest;
    public PlaylistController PlaylistController;
    public string PlayerName, QuestName;
    public Idle InventoryIdle, FarmIdle;
    public EGameStatus Status;
    public EGameType GameType;
	public bool GugaVersion;
    public int StartingLevel;
    public tk2dCamera UICam;
    public Camera Cam, AfterCamera;
    public Camera Camera;
    public GameObject MainMenu, InitialObject;
    public bool DebugMode, ShowTileText, MapInitialized, ForceRestart, ForceRestartFromBeginning;
    public Util U;
    public NiobiumStudios.DailyRewards Reward;
    public Quest Quest;
    public int ProfileNumber;
    public string[] InputNames;
    public string SaveInfo;
    public bool FullVersion;
    public Console Console;
    public Inventory Inventory, PackMule;
    public Idle Idle;
    public Tutorial Tutorial;
    public float PlayTime = 0;
    public float TotalPlayTime = 0;
    public float CubesTotalTime = 0;
    public GameObject bundle;
    public bool PlayTimeLoaded = false;
    public int ConnectionRetries = -1;
    public bool IdleInitialized = false;
    public bool SaveOnEndGame = true;
    public static bool ForceInitEditor = false;
    #endregion
    void Start()
    {
        I = this;
        U = new Util();

        G.Init();
        Inventory.InitDictionary();                                                              // init item dictionary 
        SaveOnEndGame = false;
        UpdateCurrentAdventure( );

        Directory.CreateDirectory( Manager.I.GetProfileFolder() );                               // Create Folder for the first time
        Directory.CreateDirectory( Manager.I.GetProfileFolder() + "Cube Save" ); 

        Manager.I.QuestName = "Main Quest";

        if( Environment.UserName == "alien" )
        if( Directory.Exists( "C:/NEO/Assets/Resources" ) )
            GugaVersion = true;                                                                  // checks for my version

        if( Helper.I.ReleaseVersion || Helper.I.StartAtCubes == false )
            Manager.I.Reward.StartIt();
        else
            Manager.I.Reward.now = System.DateTime.Now;

        //gg Map.I.NavigationMap.AreasTilemap.transform.parent = Map.I.AreasTilemapFolder.transform;
        //Map.I.NavigationMap.Tilemap.gameObject.SetActive( false );
		MapInitialized = false;
        ForceRestart = false;
        ForceRestartFromBeginning = false;
        DontDestroyOnLoad( this );
        InitInput();
        GoToMainMenu( true );
        Quest.Init();
        Camera = Cam.GetComponent<Camera>();
    }
#if UNITY_EDITOR

    [InitializeOnLoadMethod]                                                                   // Editor only initialization for Singleton
    public static void InitSingleton()
    {
        EditorApplication.delayCall += () =>
        {
            if( !Application.isPlaying )
            {
                GameObject go = GameObject.Find("----------------Game Manager---------");
                if( go != null )
                {
                    I = go.GetComponent<Manager>();
                    if( I != null )
                        I.U = new Util();
                }
            }
        };
    }

    //[MenuItem( "Tools/Select Cube Data _F8" )]
    private static void SelectInitialObject()
    {
       // Selection.activeGameObject = I.InitialObject;
       // EditorApplication.delayCall += () =>
       // {
       //     Selection.activeGameObject = I.InitialObject;
       //     EditorGUIUtility.PingObject( I.InitialObject );
       //     Helper.I = GameObject.Find( "Helper" ).GetComponent<Helper>();
       //     SceneView sv = SceneView.lastActiveSceneView;
       //     if( sv != null )
       //     {
       //         sv.LookAt( I.InitialObject.transform.position + new Vector3( 15, 14, 0 ), sv.rotation, 30 );
       //         sv.Repaint();
       //     }
       // };
       //ForceInitEditor = true;
       //// Isso força o Inspector a acordar e rodar o OnInspectorGUI imediatamente
       //UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

[MenuItem( "Tools/Select Hero _F9" )]
private static void SelectHeroObject()
{
    Map.I = GameObject.Find( "----------------Map" ).GetComponent<Map>();
    Selection.activeGameObject = Map.I.Hero.gameObject;
    EditorApplication.delayCall += () =>
    {
        Selection.activeGameObject = Map.I.Hero.gameObject;
        EditorGUIUtility.PingObject( Map.I.Hero.gameObject );
    };
}
    
[MenuItem( "Tools/Update Data _F6" )]
private static void UpdateData()
{
    GameObject ob = GameObject.Find( "Farm" );
    Farm f = ob.GetComponent<Farm>();
    f.UpdateListsCallBack();    
}


[MenuItem( "Tools/Save Profile" )]
private static void SaveProfile()
{
    string sourcePath = "C:\\NEO\\Assets\\Profiles\\profile 0";    // source
    string backupPath = "C:\\NEO\\Assets\\Profiles\\Profile Backup"; // destination

    if( !Directory.Exists( sourcePath ) )
    {
        Debug.LogError( "Profile folder does not exist: " + sourcePath );
        return;
    }

    if( !Directory.Exists( backupPath ) )
        Directory.CreateDirectory( backupPath );

    Util.CopyDirectory( sourcePath, backupPath );
    Debug.Log( "Profile saved to backup: " + backupPath );
}
[MenuItem( "Tools/Load Profile" )]
private static void LoadProfile()
{
    string backupPath = "C:\\NEO\\Assets\\Profiles\\Profile Backup"; // source
    string profilePath = "C:\\NEO\\Assets\\Profiles\\profile 0";    // destination

    if( !Directory.Exists( backupPath ) )
    {
        Debug.LogError( "No backup found: " + backupPath );
        return;
    }

    if( Directory.Exists( profilePath ) )
        Directory.Delete( profilePath, true ); // delete old profile

    Util.CopyDirectory( backupPath, profilePath );
    Debug.Log( "Profile restored from backup: " + backupPath );
}
#endif

//_____________________________________________________________________________________________________________________ Main game loop
    
    public void Update()
    {
        switch( Status )
        {
            case EGameStatus.PLAYING:  UpdatePlayGame(); break;                            // Update Playgame
        }

        if( Input.GetKeyDown( KeyCode.BackQuote ) )                                        // toggle Console
        {
            ToggleConsole();
        }

        CheckException();                                                                  // Check for Exceptions and Quit if so
    }
    IEnumerator InventoryProductionLoop()
    {
        float lastTime = Time.unscaledTime;                                              // Store initial real-world time
        float accumulator = 0f;                                                          // Buffer to store elapsed time slices
        var wait = new WaitForSecondsRealtime( 0.05f );                                  // ZERO GC CACHE: Prevent allocation in loop

        while( true )
        {
            // 1. Calculate real time passed since last check
            float frameTime = Time.unscaledTime - lastTime;                              // Calculate delta since last loop
            lastTime = Time.unscaledTime;                                                // Sync anchor with current time

            // 2. Add to buffer
            accumulator += frameTime;                                                    // Accumulate time to be processed

            var mapI = Map.I;                                                            // Cache Map instance to save CPU

            // 3. Determine current tick rate
            float step = ( mapI.RM.GameOver ||
            mapI.RM.DungeonDialog.gameObject.activeSelf )
                         ? 0.1f : 1.0f;                                                  // Adjust step based on UI or Game State

            // 4. Process production in fixed steps
            // This ensures production never exceeds real-world elapsed time.
            while( accumulator >= step )
            {
                Inventory.UpdateInventoryProduction( step );                             // Process one discrete time step
                accumulator -= step;                                                     // Consume the processed time from buffer
            }

            // 5. CPU Throttle
            // Short wait to prevent high CPU usage while maintaining precision.
            yield return wait;                                                           // ZERO GC: Yield cached object
        }
    }

    //_____________________________________________________________________________________________________________________ Update Play Game

    public void UpdatePlayGame()
    {
        Map.I.UpdateIt();
        UpdatePlayGameInput();
		UI.I.UpdateIt();
        TotalPlayTime += Time.unscaledDeltaTime;
        PlayTime += Time.unscaledDeltaTime;
    }

    //_____________________________________________________________________________________________________________________ Update Play Game Input

    public void UpdatePlayGameInput()
    {
#if UNITY_EDITOR
        if( Input.GetKeyDown( KeyCode.T ) )
        {
            UnityEditor.EditorWindow.focusedWindow.maximized = !UnityEditor.EditorWindow.focusedWindow.maximized;
        }
#endif 
        //if( Input.GetKeyDown( KeyCode.Space ) ) UI.I.ToggleMenu();
        if( Input.GetKeyDown( KeyCode.F2 ) || 
           Util.IsPressed( UI.I.RestartAreaButton ) )
        {
            if( GameType == EGameType.FARM )
            {
                if( Map.I.Farm.Finalize() )
                    LevelIntro.I.ShowLevelIntro( true, Quest.CurrentLevel );
                else return;
            }            
        }
        if( Input.GetKeyDown( KeyCode.Escape ) )
        {
            if( GameType == EGameType.FARM )
            {
                if( Map.I.Farm.Finalize() )
                    LevelIntro.I.GoToTheCubes();
                else return;
            }
            else
            if( GameType == EGameType.NORMAL )
                GoToMainMenu( true );
        }
    }
    private static void CheckException()
    {
        if( Application.platform != RuntimePlatform.WindowsPlayer ) return;
        if( Security.FileConsistencyChecked )
        if( GlobalGameGuard.I.BugFound )
            WindowsMessageBox.ShowErrorAndQuit(
            "Saving has been disabled due to a critical issue.\n\nThe game will now close to prevent data loss.", "Warning" );                 // Critical Error
    }

    //_____________________________________________________________________________________________________________________ Play Game
        
    public void UpdateIdleProductionInitialization()
    {
        if( GetInternetAvailable() !=
           NiobiumStudios.CloudClockBuilder.State.Initialized ) return;
        if( IdleInitialized == true ) return;
        Idle.StartIt();
        Manager.InitSingleton();
        Inventory.UpdateInventoryProduction( Manager.I.Idle.OffSeconds );
        PackMule.UpdateInventoryProduction( Manager.I.Idle.OffSeconds );
        Manager.I.Idle.OffSeconds = 0;
        StartCoroutine( InventoryProductionLoop() );
        IdleInitialized = true;
    }

    public void PlayGame()
    {
        SettingsWindow.I.RestoreDefaultSettings();  //ggg temporario ate arrumar setting screen       // ******************************
        if( Security.CheckPlayerFilesConsistency() == false )                                 // Checks for cheaters
            return;

        SaveOnEndGame = true;
        Status = EGameStatus.PLAYING;                                                         // Set Status:: Playing
        MainMenu.gameObject.SetActive( false );
        UI.I.gameObject.SetActive( true );
        Map.I.gameObject.SetActive( true );
         
        if( PlayTimeLoaded == false ) Load();
        Inventory.Load();
        //PackMule.Load(); 

        if( Helper.I.ReleaseVersion == false )
        {
            if( Helper.I.StartAtFarm ) 
            {
                StartCoroutine( InventoryProductionLoop() );
                Map.I.Farm.StartIt();
            }
            else
            if( Helper.I.StartAtCubes )
            {
                if( Helper.I.PlayMusic )
                {
                    StartCoroutine( InventoryProductionLoop() );
                    PlaylistController.StopPlaylist();                                      // Play Music
                    PlaylistController.StartPlaylist( "Full" );
                    PlaylistController.PlayARandomSong( DarkTonic.MasterAudio.
                    PlaylistController.AudioPlayType.PlayNow, false );
                }
                Map.I.RM.StartCubeSession();
            }
            return;
        }

        LevelIntro.I.ShowLevelIntro( false, -1 );
        PlaylistController.StopPlaylist();
        PlaylistController.StartPlaylist( "Full" );

        if( Security.FirstTimePlaying == false )
            PlaylistController.PlayARandomSong( DarkTonic.MasterAudio.PlaylistController.AudioPlayType.PlayNow, false );
        else
            PlaylistController.TriggerPlaylistClip( "Ireland" );                                                                        // First time Farm tutorial Music
    }

    //_____________________________________________________________________________________________________________________ Go to Main Menu

    public void GoToMainMenu( bool changeaudio )
    {
        MainMenu.gameObject.SetActive( true );
        UI.I.gameObject.SetActive( false );
        Map.I.gameObject.SetActive( false );
        Status = EGameStatus.MAINMENU;
        if( changeaudio && Helper.I.PlayMusic )
        {
            PlaylistController.StopPlaylist();
            PlaylistController.StartPlaylist( "Main Menu" );
        }
    }

    //_____________________________________________________________________________________________________________________ Load Level

    public bool LoadLevel( int _level )
    {
        if( _level >= Quest.LevelList.Length ) return false;

        Quest.CurrentLevel = _level;
        Quest.CurLevel = Quest.LevelList[ Quest.CurrentLevel ];

        Map.I.StartGame();
 
        return true;
    }

    //_____________________________________________________________________________________________________________________ On Quit

    public void OnApplicationQuit()
    {
        if( SaveOnEndGame == false ) return;
        if( Helper.I.SaveDataOnExit == false ) return;
        if( ProfileNumber == -1 ) return;

        RandomMapGoal.ClearPrefabData();                                        // clear goals prefab data

        if( GameType == EGameType.FARM )
        {
            Map.I.Farm.Finalize();
        }

        if( GameType == EGameType.NAVIGATION )
        {
            Map.I.NavigationMap.ExitNavigationMap();
        }

        InventoryIdle.Save();

        Save();

        Inventory.Save();
        //PackMule.Save();

        //Manager.I.Reward.Save();

        Map.I.RM.DungeonDialog.UpdatePlayerProfileBackup( true, true );     // Exit Player Profile Backup

        GS.DeleteCubeSaves();
    }

    public void Save()
    {
        if( SaveOnEndGame == false ) return;
        if( G.Tutorial.CanSave() == false ) return;
        string file = Manager.I.GetProfileFolder() + "Time.NEO";                           // Provides file name

        using( MemoryStream ms = new MemoryStream() )
        using( BinaryWriter writer = new BinaryWriter( ms ) )                              // Open Memory Stream
        {
            GS.W = writer;                                                                 // Assign BinaryWriter to GS.W for TF

            int Version = Security.SaveHeader( 1 );                                        // Save Header Defining Current Save Version

            TF.SaveT( "TotalPlayTime", TotalPlayTime );                                    // Save total play time
            TF.SaveT( "CubesTotalTime", CubesTotalTime );                                  // Save cubes total time
            TF.SaveT( "CurrentAdventure", Map.I.RM.CurrentAdventure );                     // Save current adventure
            TF.SaveT( "WaterTiles", Map.I.Farm.WaterTiles );                               // Save Farm Water Tiles

            FPow.Save();                                                                   // Save Fire Power

            GS.W.Flush();                                                                  // Flush the writer

            Security.FinalizeSave( ms, file );                                             // Finalize save
        } 
    }

    public void Load()
    {
        string file = Manager.I.GetProfileFolder() + "Time.NEO";                           // Provides file name
        PlayTimeLoaded = true;

        if( File.Exists( file ) == false )
        {
            TotalPlayTime = CubesTotalTime = 0;
            if( Helper.I.ReleaseVersion )
                Map.I.RM.CurrentAdventure = -1;
            return;
        }

        byte[] fileData = File.ReadAllBytes( file );                                       // Read full file
        byte[] content = Security.CheckLoad( fileData );                                   // Validate HMAC and get clean content

        using( GS.R = new BinaryReader( new MemoryStream( content ) ) )                    // Use MemoryStream for TF
        {
            int SaveVersion = Security.LoadHeader();                                       // Load Header

            TotalPlayTime = TF.LoadT<float>( "TotalPlayTime" );                            // Load total play time
            CubesTotalTime = TF.LoadT<float>( "CubesTotalTime" );                          // Load cubes total time
            int adv = TF.LoadT<int>( "CurrentAdventure" );                                 // Load current adventure
            Map.I.Farm.WaterTiles = TF.LoadT<int>( "WaterTiles" );                         // Load Farm Water Tiles

            FPow.Load();                                                                   // Load Fire Powers

            if( Helper.I.ReleaseVersion )
            {
                Map.I.RM.CurrentAdventure = adv;
                Map.I.RM.DungeonDialog.ChooseAdventure( Map.I.RM.CurrentAdventure );
            }

            GS.R.Close();                                                               // Close stream
        }
    }

    
    //_____________________________________________________________________________________________________________________ Restart Area

    public void RestartArea()
    {
        if( Manager.I.GameType == EGameType.FARM ) return;
        if( Manager.I.GameType == EGameType.CUBES )
        if( Map.I.RM.DungeonDialog.gameObject.activeSelf )
        if( Map.I.RM.GameOver ) return;

        if( Manager.I.GameType == EGameType.NAVIGATION )
        {
            Map.I.NavigationMap.ExitNavigationMap();
            return;
        }

        if( Map.I.CurrentArea == -1 ) 
        {
           // Map.I.RM.DialogButtonPressed = true;
            return;
        }

        if( Manager.I.GameType == EGameType.NORMAL )
        if( Map.I.CurrentArea == -1 ) return;

        Message.CreateMessage( ETileType.NONE, "Area Restarted...", Map.I.Hero.Pos, new Color( 0, 1, 0, 1 ) );

        Map.I.RepeatToLastButton();
    }

    //_____________________________________________________________________________________________________________________ Update Restart

    public void UpdadeRestart()
    {
        if( Map.I.RM.HeroSector == null ) return; // added because of new bug
        if( Input.GetKeyDown( KeyCode.R ) || ForceRestart || ForceRestartFromBeginning )
        {
            if( GameType == EGameType.CUBES )                                                            // Rescue me function
            if( Map.I.CurrentArea == -1 )
            if( Map.I.RM.HeroSector.Type == Sector.ESectorType.NORMAL )
            {
                if( Input.GetKey( KeyCode.LeftAlt ) == false )
                {
                   Message.RedMessage("Press Left Alt + R\nto be Rescued!");
                   return;
                }
                else
                {
                    if( Map.I.RM.RMD.RescueEnabled )
                    {
                    Map.I.StartCubeDeath();
                    Map.I.PlatformDeath = true;
                    }
                    else
                        Message.RedMessage( "Rescue Hero Disabled in this Quest!" );
                }
            return;
            }

			if( Input.GetKey( KeyCode.LeftAlt ) )                                                             // Level Restart
			{
				if( Map.I.CurrentArea == -1 )
				{
					Map.I.LastFileSavedName = "Level Entrance - L" + Quest.CurrentLevel + ".wq";
					//LoadGame( Map.I.LastFileSavedName );
				}
                goto End;
			}
			else
			{
				if( Map.I.CurrentArea != -1 )
				{
                    if( Manager.I.GameType == EGameType.CUBES && Manager.I.ForceRestartFromBeginning )
                    {
                        RestartArea();
                        Map.I.ExitArea( Map.I.Hero.Pos, Map.I.RM.HeroSector.LastHeroPos );
                        goto End;
                    }
                    
                    RestartArea();
                    goto End;
				}
				else
					if( Map.I.CurrentArea == -1 )
					{
                        //if( Map.I.LastFileSavedName != "" )
                        //    LoadGame( Map.I.LastFileSavedName );
                        goto End;
					}
			}
		}

        End:
        {
            ForceRestart = false;
            ForceRestartFromBeginning = false;
            return;
        }

    }

    //_____________________________________________________________________________________________________________________ Gets savegame folder

    public string GetProfileFolder()
    {
        string folder = Application.persistentDataPath + "/Profiles/Profile " + ProfileNumber + "/Main Quest/";
        if( Application.platform == RuntimePlatform.WindowsPlayer )
            folder = Application.dataPath + "/Profiles/Profile " + ProfileNumber + "/Main Quest/";
        if( Application.platform == RuntimePlatform.WindowsEditor )
            folder = Application.dataPath + "/Profiles/Profile " + ProfileNumber + "/Main Quest/";

        return folder;
    }

    public string GetGameFolder()
    {
        string folder = Application.persistentDataPath;
        if( Application.platform == RuntimePlatform.WindowsPlayer ) folder = Application.dataPath;
        if( Application.platform == RuntimePlatform.WindowsEditor ) folder = Application.dataPath;
        return folder;
    }

	//_____________________________________________________________________________________________________________________ Exit Level

	public void ExitLevel()
	{
        if( Quest.I.CurLevel != null )
            Quest.I.CurLevel.gameObject.SetActive( false );

		HeroData.I.ClearAllCollectedArtifacts( Artifact.EArtifactLifeTime.SESSION );
		Map.I.ChangeHero( EHeroID.KADE, false );
		Map.I.Hero.Control.PathFinding.Path.Clear();
        Map.I.Finalize();
	}

    public GameObject CreateObjInstance( string str, string iname, EDirection dir, Vector3 pos )
    {
        GameObject prefab = ( GameObject ) Resources.Load( str );
        if( prefab == null ) Debug.LogError( "Bad Prefab: " + str );
        GameObject instance = ( GameObject ) GameObject.Instantiate( prefab );
        instance.transform.position = pos;
        instance.name = iname;
        if( dir != EDirection.NONE ) instance.transform.Rotate( 0, 0, -( 45.0f * ( float ) ( int ) dir ) );
        return instance;
    }

    public void ToggleConsole()
    {
        Manager.I.Console.gameObject.SetActive( !Manager.I.Console.gameObject.activeSelf );
    }

    public void InitInput()
    {
        cInput.SetKey( InputNames[ 0 ], "Alpha8", "UpArrow" );
        cInput.SetKey( InputNames[ 1 ], "Alpha9", "Keypad3" );
        cInput.SetKey( InputNames[ 2 ], "O", "RightArrow" );
        cInput.SetKey( InputNames[ 3 ], "L", "Keypad9" );
        cInput.SetKey( InputNames[ 4 ], "K", "DownArrow" );
        cInput.SetKey( InputNames[ 5 ], "J", "Keypad7" );
        cInput.SetKey( InputNames[ 6 ], "U", "LeftArrow" );
        cInput.SetKey( InputNames[ 7 ], "Alpha7", "Keypad1" );
        cInput.SetKey( InputNames[ 8 ], "V", "W" );
        cInput.SetKey( InputNames[ 9 ], "C", "Q" );
        cInput.SetKey( InputNames[ 10 ], "X", "Keypad5" );
        cInput.SetKey( InputNames[ 11 ], "Z", "D" );
		cInput.SetKey( InputNames[ 12 ], "N", "N" );
        cInput.SetKey( InputNames[ 13 ], "B", "B" );
    }

    public string Translate( string txt, string sheet )
    {
        if( txt[ 0 ] != '&' ) return txt;
        txt = txt.Substring( 1 );
        txt = Language.Get( txt, sheet );
        txt = txt.Replace( "\\n", "\n" );
		return txt;
    }

    public NiobiumStudios.CloudClockBuilder.State GetInternetAvailable()
    {
        return NiobiumStudios.CloudClockBuilder.currentState;
    }
    public int UpdateCurrentAdventure()
    {
        return 0;//ggg
        int id = -1;
        #if UNITY_EDITOR
        if( Selection.activeGameObject == null ) return -1;
        string nm = "";
        for( int i = 0; i < Map.I.RM.RMList.Count; i++ )
        {
            if( Map.I.RM.RMList[ i ].name == Selection.activeGameObject.name )
            { nm = Map.I.RM.RMList[ i ].name; id = i; break; }
        }
        if( id == -1 ) return -1;

        MapSaver.I.CurrentAdventure = id;
        MapSaver.I.CurrentAdventureName = nm;
        Helper.I.StartingAdventure = id;
        #endif
        return id;
    }

    internal string GetPlayerName()
    {
        return "nameless"; //gggg
        string pname = ProfileWindow.I.ProfileNames[ 0 ];
        if( !string.IsNullOrEmpty( pname ) )
            pname = char.ToUpper( pname[ 0 ] ) + pname.Substring( 1 );
        return pname;
    }
}
