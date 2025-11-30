using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour 
{
	public static MainMenu I;
	public tk2dTextMesh PlayButton, ProfileButton, QuestButton, SettingsButton, ExitButton, VersionText;
	public ProfileWindow ProfileWindow;
    public QuestWindow QuestWindow;
    public SettingsWindow SettingsWindow;
    public tk2dSprite LogoImage;
    public SpriteRenderer MainBack1;
    public Sprite[] BackgroundSpriteList;
    public int ActiveBackgroundObject;
    public bool ChangeBackground;
    public int CurrentBackgroundID = 0;
    public int BackgroundsAvailable = 1;
    public List<int> BackIdList;

	//_____________________________________________________________________________________________________________________ Start

    public void Start()
    {
        I = this;

        ProfileWindow.Start();                // to avoid object order init bug
        QuestWindow.Start();
        SettingsWindow.Start();

        ProfileWindow.I.LoadProfileWindowData();
        QuestWindow.I.LoadQuestWindowData( Manager.I.ProfileNumber );
        SettingsWindow.I.LoadSettingsWindowData( Manager.I.ProfileNumber );
        ProfileWindow.I.gameObject.SetActive( false );
        QuestWindow.I.gameObject.SetActive( false );
        SettingsWindow.I.gameObject.SetActive( false );
        UI.I.BackgroundUI.gameObject.SetActive( false );
        for( int i = 0; i < 99999; i++ )                     // Sort back ground images
        {
            int rand = Random.Range( 0, 9 );
            if( BackIdList.Contains( rand ) == false )
                BackIdList.Add( rand );
            if( BackIdList.Count >= 9 ) break;
        }
        ActiveBackgroundObject = 1;
        ChangeBackground = false;
        ChooseBackground();
        MainBack1.color = new Color( 1, 1, 1, 0 );
    }

	//_____________________________________________________________________________________________________________________ Update

    public void Update()
    {
        if( Manager.I.Status != EGameStatus.MAINMENU ) return;
        MainMenu.I.QuestButton.text = Language.Get( "QUEST_BUTTON" ) + ":\n" + Manager.I.QuestName;
        if( Input.GetKeyDown( KeyCode.Return ) ) PlayGame();
        if( Helper.I.AutoClickPlayButton ) PlayGame();
        if( Input.GetKeyDown( KeyCode.Escape ) ) 
            Exit();
        PlayButton.text = Language.Get( "PLAY_BUTTON" );
        SettingsButton.text = Language.Get( "SETTINGS_BUTTON" );
        ExitButton.text = Language.Get( "EXIT_BUTTON" );
        ExitButton.text = ExitButton.text.Replace( "\\n", "\n" );
        Cursor.visible = true;
                
        VersionText.text = "NEO Engine - Chapter I - Version: 1.0";
        if( Manager.I.FullVersion ) VersionText.text = "Version: Beta 1.0 (Full Demo)";

        float spd = .95f;
        if( ActiveBackgroundObject == 1 )
        {
            MainBack1.color = new Color( 1, 1, 1, MainBack1.color.a + ( spd  * Time.deltaTime ) );
        }

        if( ActiveBackgroundObject == 2 )
        {
            MainBack1.color = new Color( 1, 1, 1, MainBack1.color.a - ( spd * Time.deltaTime ) );
        }

        MainBack1.color = new Color( MainBack1.color.r, MainBack1.color.g, MainBack1.color.b, Mathf.Clamp( MainBack1.color.a, 0, 1 ) );
        if( ChangeBackground ) ChooseBackground();
    }

	//_____________________________________________________________________________________________________________________ Play game

    public void PlayGame()
    {
		if( QuestWindow.I.gameObject.activeSelf    ||  // Dont start game if no profile is chosen
		    ProfileWindow.I.gameObject.activeSelf  ||
		    SettingsWindow.I.gameObject.activeSelf ||
            Credits.I.gameObject.activeSelf        ||
		    Manager.I.ProfileNumber == -1          )
            {
			if(	Manager.I.ProfileNumber == -1 ) OpenProfileWindow();
            return;
            }

        Manager.I.PlayGame();
    }

    //_____________________________________________________________________________________________________________________ Exit

    public void Exit()
    {
        #if UNITY_EDITOR
        EditorApplication.ExecuteMenuItem( "Edit/Play" );
        #endif
        Application.Quit();
    }

	//_____________________________________________________________________________________________________________________ Open Profile Window

    public void OpenProfileWindow()
    {
		ProfileWindow.I.InitProfileScreen();
    }

    public void IndiegogoButton()
    {
        Application.OpenURL( "https://www.indiegogo.com/projects/wonderquest-survival-rpg/x/10420820#/" );
    }

	//_____________________________________________________________________________________________________________________ Open Quest Window


	public void OpenQuestWindow()
	{
        return;
        QuestWindow.I.InitQuestScreen();
	}
    
    //_____________________________________________________________________________________________________________________ Open Quest Window
    
    public void OpenCreditsWindow()
    {
        Credits.I.ShowCredits();
    }

	//_____________________________________________________________________________________________________________________ Open Settings Window

	public void OpenSettingsWindow()
	{
		SettingsWindow.I.InitSettingsScreen(); 
	}

    public void ChooseBackground()
	{
        //SpriteRenderer spr = MainBack1;                 old scrolling bg

        //if( ActiveBackgroundObject == 1 )
        //{
        //    ActiveBackgroundObject = 2;
        //    spr = MainBack2;
        //}
        //else if( ActiveBackgroundObject == 2 )
        //{
        //    ActiveBackgroundObject = 1;
        //    spr = MainBack1;
        //}
        //if( ++CurrentBackgroundID == 9 )
        //    CurrentBackgroundID = 0;
        //spr.sprite = BackgroundSpriteList[ BackIdList[ CurrentBackgroundID ] ];
        //ChangeBackground = false;
    }
}
