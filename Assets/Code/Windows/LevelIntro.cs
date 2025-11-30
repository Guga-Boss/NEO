using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class LevelIntro : MonoBehaviour
{
    public static LevelIntro I;
    public int SelectedLevel;
    public List<GameObject> ButtonObjList;
    public List<tk2dSprite> ButtonSpriteList;
    public List<tk2dSprite> RewardIcon;
    public List<tk2dSlicedSprite> RewardBack;
    public List<tk2dTextMesh> RewardText;
    public bool MapLoaded;
    public float ExitLevelTimer;
    public SpriteRenderer Back;
    public List<Sprite> SpriteList;

    // Use this for initialization
    void Start()
    {
        I = this;
        gameObject.SetActive( false );
        ExitLevelTimer = -1;
        Back.color = new Color( 1, 1, 1, 0 );
    }

    // Update is called once per frame
    void Update()
    {
        if( Input.GetKeyDown( KeyCode.F2 ) ||
            Input.GetKeyDown( KeyCode.Return ) ) ExitLevelIntro();

        if( ExitLevelTimer != -1 )
        {
            ExitLevelTimer += Time.deltaTime;
            if( ExitLevelTimer > .3f )
            {
                ExitLevelIntro(); ExitLevelTimer = -1f;
            }
        }
        float spd = 1.5f;
        Back.color = new Color( 1, 1, 1, Back.color.a + ( spd * Time.deltaTime ) );
        Cursor.visible = true;
        Map.I.Farm.BlueprintUI.gameObject.SetActive( false );
        Map.I.Farm.BuildingUI.gameObject.SetActive( false );
        UI.I.BackgroundUI.gameObject.SetActive( false );
        if( Input.GetKeyDown( KeyCode.F ) ) GoToTheFarm();                                     // Farm Shortcut
        if( Input.GetKeyDown( KeyCode.C ) ) GoToTheCubes();                                    // Cubes Shortcut
        if( Input.GetKeyDown( KeyCode.Escape ) )
            Manager.I.GoToMainMenu( true );                                                    // Main Menu Shortcut

        UpdateInternetConnection();        
        Manager.I.Reward.UpdateDailyReward();
        Manager.I.UpdateIdleProductionInitialization();
    }
    public void UpdateInternetConnection()
    {
        NiobiumStudios.CloudClockBuilder.State st = Manager.I.GetInternetAvailable();
        string txt = "";
        Color col = Color.yellow;
        if( st == NiobiumStudios.CloudClockBuilder.State.Initialized )
        {
            txt = "Internet Status: Connected.";
            col = Color.green;
        }
        else
        if( st == NiobiumStudios.CloudClockBuilder.State.Initializing )
        {
            txt = "Internet Status: Connecting... (required to play)";
            col = Color.yellow;
        }
        else
        if( st == NiobiumStudios.CloudClockBuilder.State.FailedToInitialize )
        {
            txt = "Internet Status: Failed to Connect! (required to play)";
            col = Color.red;
        }
        UI.I.LoadingLevelText.text = txt;
        UI.I.LoadingLevelText.color = col;
    }

    public void ShowLevelIntroButton()
    {
        if( Map.I.RM.GameOver == false )
        {
            Map.I.RM.DungeonDialog.SetMsg( "ERROR: Finalize Session First!", Color.red );
            return;
        }

        Map.I.RM.Finalize();
        ShowLevelIntro( true, 1 );
    }

    public void ShowLevelIntro( bool _mapLoaded, int selLevel )
    {
        MapLoaded = _mapLoaded;
        gameObject.SetActive( true );
        UI.I.gameObject.SetActive( false );

        for( int i = 1; i <= Quest.I.LevelList.Length; i++ )
        {
            string file = Manager.I.GetProfileFolder() + Manager.I.QuestName +
                   "/Game Exit Savegame - L" + ( i - 1 ) + ".wq";

            if( ES2.Exists( file ) || i == Quest.CurrentLevel + 1 || Manager.I.GugaVersion )
            {
                //ButtonObjList[ i ].SetActive( true );
                if( _mapLoaded == false ) SelectedLevel = i;
            }
            //else
            //    ButtonObjList[ i ].SetActive( false );
        }

        //if( selLevel == -1 ) selLevel = Manager.I.LoadLastSavedLevel();
        SelectLevel( selLevel + 1 );

        Manager.I.Status = EGameStatus.LEVELINTRO;
        //UI.I.LoadingLevelText.gameObject.SetActive( false );
        ExitLevelTimer = -1;
        Back.color = new Color( 1, 1, 1, 0 );
        int id = UnityEngine.Random.Range( 0, SpriteList.Count );
        Back.sprite = SpriteList[ id ];
        Map.I.RM.DungeonDialog.InventoryGO.SetActive( false );
        Map.I.RM.DungeonDialog.gameObject.SetActive( false );
        GS.DeleteCubeSaves();
    }

    public void GoToTheFarm()
    {
        if( Helper.I.ReleaseVersion )
        if( Application.platform != RuntimePlatform.WindowsEditor )
        {
            if( Manager.I.GetInternetAvailable() !=
                NiobiumStudios.CloudClockBuilder.State.Initialized )                                              // Block go to the farm button since no connection to Internet has been found
                return;
            if( Manager.I.IdleInitialized == false ) return;
        }

        gameObject.SetActive( false );
        UI.I.gameObject.SetActive( true );
        Map.I.Farm.gameObject.SetActive( true );
        Map.I.Farm.StartIt();
    }
    public void GoToTheCubes()
    {
        if( Helper.I.ReleaseVersion )
        if( Application.platform != RuntimePlatform.WindowsEditor )
        {
            if( Manager.I.GetInternetAvailable() !=
                NiobiumStudios.CloudClockBuilder.State.Initialized )
                return;
            if( Manager.I.IdleInitialized == false ) return;
        } 

        if( G.Tutorial.CheckPhase( 14, true ) == false ) return;
        gameObject.SetActive( false );
        UI.I.gameObject.SetActive( true );
        Manager.I.Status = EGameStatus.PLAYING;

        Manager.I.GameType = EGameType.CUBES;
        //Map.I.RM.StartCubeSession( false );
        Map.I.RM.GameOver = true;
        Map.I.RM.DungeonDialog.gameObject.SetActive( true );
        Manager.I.Inventory.gameObject.SetActive( true );
    }


    public void ExitLevelIntro()
    {
        return;
        //UI.I.LoadingLevelText.gameObject.SetActive( true );

        //if( ExitLevelTimer == -1 )
        //{
        //    ExitLevelTimer = 0f; return;
        //}

        //if( MapLoaded == false || SelectedLevel != Quest.CurrentLevel + 1 )
        //{
        //    if( MapLoaded )
        //        Manager.I.SaveExitSavegame();

        //    Quest.CurrentLevel = SelectedLevel - 1;
        //    Manager.I.ExitLevel();
        //    Manager.I.LoadExitSavegame( SelectedLevel - 1 );
        //}
        //gameObject.SetActive( false );
        //UI.I.gameObject.SetActive( true );
        //Manager.I.Status = EGameStatus.PLAYING;
        //Manager.I.SaveExitSavegame();
        //Manager.I.SaveLevelEntrance();
        //ExitLevelTimer = -1;
        //Manager.I.GameType = EGameType.NORMAL;
    }

    public void SelectLevel( int level )
    {
        //SelectedLevel = level;
        //for( int i = 1; i <= Quest.I.LevelList.Length; i++ )
        //    if( ButtonObjList[ i ].activeSelf )
        //    {
        //        if( i == SelectedLevel )
        //            ButtonSpriteList[ i ].color = new Color( 0, 1, 0, 1 );
        //        else
        //            ButtonSpriteList[ i ].color = new Color( 1, 0, 0, 1 );
        //    }

        //TitleText.text = "Story Mode";
        //if( level > 0 )
        //    TitleText.text+= " - Level " + ( level );

        //IntroText.text = Language.Get( "LEVELINTRO_" + ( level ) );
        //IntroText.text = IntroText.text.Replace( "\\n", "\n" );
        //IntroText.text = "Story Mode Temporarily Disabled.";
    }

    public void SelectLevel_1()
    {
        SelectLevel( 1 );
    }
    public void SelectLevel_2()
    {
        SelectLevel( 2 );
    }
    public void SelectLevel_3()
    {
        SelectLevel( 3 );
    }
    public void SelectLevel_4()
    {
        SelectLevel( 4 );
    }
    public void SelectLevel_5()
    {
        SelectLevel( 5 );
    }
    public void SelectLevel_6()
    {
        SelectLevel( 6 );
    }
    public void SelectLevel_7()
    {
        SelectLevel( 7 );
    }
}
