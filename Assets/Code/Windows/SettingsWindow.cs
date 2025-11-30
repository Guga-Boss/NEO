using UnityEngine;
using System.Collections;
using DarkTonic.MasterAudio;

public class SettingsWindow : MonoBehaviour {
	public static SettingsWindow I;
	public tk2dTextMesh[] PrimaryActionButtonText;
	public tk2dTextMesh[] SecondaryActionButtonText;
    public float MaxHeroSpeed, KeyHoldDelay, DampTime, MusicVolume, SoundFxVolume;
	public tk2dUITextInput KeyHoldTextInput, MaxHeroSpeedTextInput, CameraDampTextInput;
	public tk2dUIScrollbar KeyHoldScrollbar, MaxHeroSpeedScrollbar, MusicVolumeScrollBar, SoundFxScrollBar;
    public tk2dUIDropDownMenu LanguageMenu;
    public int OldLanguageIndex;

	//_____________________________________________________________________________________________________________________ Start

	public void Start()
	{
		I = this;
        OldLanguageIndex = -1;
	}

	//_____________________________________________________________________________________________________________________ Update

    public void FixedUpdate()
    {
        for( int i = 0; i < cInput.length; i++ )
        {
            if( PrimaryActionButtonText  [ i ] ) PrimaryActionButtonText  [ i ].text = cInput.GetText( i, 1 );
            if( SecondaryActionButtonText[ i ] ) SecondaryActionButtonText[ i ].text = cInput.GetText( i, 2 );
        }

        KeyHoldDelay = 0 + KeyHoldScrollbar.Value * 0.5f;
        KeyHoldTextInput.Text = "" + KeyHoldDelay.ToString("0.00");
        if( CameraDampTextInput.Text != "" )
            DampTime = System.Convert.ToSingle( CameraDampTextInput.Text );
        DampTime = Mathf.Clamp( DampTime, 400, 1200 );

        MaxHeroSpeed = MaxHeroSpeedScrollbar.Value * 0.16f;
        MaxHeroSpeedTextInput.Text = "" + MaxHeroSpeed.ToString("0.00");

        if( LanguageMenu.Index != OldLanguageIndex )
        {
            Language.SwitchLanguage( LanguageMenu.selectedTextMesh.text );
            OldLanguageIndex = LanguageMenu.Index;
        }

        SoundFxVolume = SoundFxScrollBar.Value;
        MasterAudio.SetBusVolumeByName( "Sound FX", SoundFxVolume );
        MusicVolume = MusicVolumeScrollBar.Value;
        MasterAudio.PlaylistMasterVolume = MusicVolume;
	}
	
	//_____________________________________________________________________________________________________________________ Finalize Settings Screen

	public void FinalizeSettingsScreen()
	{
		gameObject.SetActive( false );
		SaveSettingsWindowData( Manager.I.ProfileNumber );
        UI.I.UpdateAllTranslations();
        MainMenu.I.LogoImage.gameObject.SetActive( true );
	}

	//_____________________________________________________________________________________________________________________ Save Settings
	
	public void SaveSettingsWindowData( int prof )
	{
        string file = Application.persistentDataPath + "/Profiles/Profile " + prof + "/Settings.dat";

        int version = 1;
        ES2.Save( version,            file + "?tag=version" ); 
		ES2.Save( MaxHeroSpeed,       file + "?tag=MaxHeroSpeed" ); 
		ES2.Save( KeyHoldDelay,       file + "?tag=KeyHoldDelay" ); 		
		ES2.Save( LanguageMenu.Index, file + "?tag=LanguageMenu.Index" );
		ES2.Save( DampTime,           file + "?tag=DampTime" );
		ES2.Save( MusicVolume,        file + "?tag=MusicVolume" );
		ES2.Save( SoundFxVolume,      file + "?tag=SoundFxVolume" );

		for( int i = 0; i < Manager.I.InputNames.Length; i++ )
		{
			ES2.Save( cInput.GetText( i, 1 ), file + "?tag=Primary Input: "   + i );
			ES2.Save( cInput.GetText( i, 2 ), file + "?tag=Secondary Input: " + i ); 
		}
	}

	//_____________________________________________________________________________________________________________________ Load Settings

	public void LoadSettingsWindowData( int profile )
	{
        string file = Application.persistentDataPath + "/Profiles/Profile " + profile + "/Settings.dat";

		if( ES2.Exists( file ) )
		{
            if( ES2.Exists( file + "?tag=version" ) )
            {
                int version     = ES2.Load<int>  ( file + "?tag=version" );
            }
			MaxHeroSpeed        = ES2.Load<float>( file + "?tag=MaxHeroSpeed" );
			KeyHoldDelay        = ES2.Load<float>( file + "?tag=KeyHoldDelay" );
			LanguageMenu.Index  = ES2.Load<int>  ( file + "?tag=LanguageMenu.Index" );
			DampTime            = ES2.Load<float>( file + "?tag=DampTime" );
			MusicVolume         = ES2.Load<float>( file + "?tag=MusicVolume" );
			SoundFxVolume       = ES2.Load<float>( file + "?tag=SoundFxVolume" );

			for( int i = 0; i < Manager.I.InputNames.Length-1; i++ )
			    {
			     string prim = ES2.Load<string>( file + "?tag=Primary Input: "   + i );
			     string sec  = ES2.Load<string>( file + "?tag=Secondary Input: " + i );
			     cInput.ChangeKey( Manager.I.InputNames[ i ], prim, sec );	 
                }

			UpdateControls();
            Language.SwitchLanguage( LanguageMenu.selectedTextMesh.text );
		}
	}

	//_____________________________________________________________________________________________________________________ Update Controls

	public void UpdateControls()
	{
		CameraDampTextInput.Text = "" + DampTime;
		KeyHoldScrollbar.Value = ( float ) KeyHoldDelay / 0.5f;
		KeyHoldTextInput.Text = "" + KeyHoldDelay.ToString( "0.00" );
		MaxHeroSpeedScrollbar.Value = MaxHeroSpeed / 0.16f;
		MaxHeroSpeedTextInput.Text = "" + MaxHeroSpeed.ToString( "0.00" );
		MusicVolumeScrollBar.Value = MusicVolume;
		SoundFxScrollBar.Value = SoundFxVolume;
	}
	
	//_____________________________________________________________________________________________________________________ Restore default Data

	public void RestoreDefaultSettings()
    {
        cInput.ChangeKey( Manager.I.InputNames[ 0 ], "Alpha8",  Keys.ArrowUp );
        cInput.ChangeKey( Manager.I.InputNames[ 1 ], "Alpha9",  Keys.PageUp );
        cInput.ChangeKey( Manager.I.InputNames[ 2 ], "O",       Keys.ArrowRight );
		cInput.ChangeKey( Manager.I.InputNames[ 3  ], "L",      Keys.PageDown );
        cInput.ChangeKey( Manager.I.InputNames[ 4 ], "K",       Keys.ArrowDown );
		cInput.ChangeKey( Manager.I.InputNames[ 5  ], "J",      Keys.End );
        cInput.ChangeKey( Manager.I.InputNames[ 6 ], "U",       Keys.ArrowLeft );
		cInput.ChangeKey( Manager.I.InputNames[ 7  ], "Alpha7", Keys.Home );
		cInput.ChangeKey( Manager.I.InputNames[ 8  ], "V",      "W"       );
		cInput.ChangeKey( Manager.I.InputNames[ 9  ], "C",      "Q"       );
		cInput.ChangeKey( Manager.I.InputNames[ 10 ], "X",      "Keypad5" );
		cInput.ChangeKey( Manager.I.InputNames[ 11 ], "Z",      "D"       );
        cInput.ChangeKey( Manager.I.InputNames[ 12 ], "N",      "N"       );
        cInput.ChangeKey( Manager.I.InputNames[ 13 ], "B",      "B"       );

        KeyHoldDelay = 0.28f;
		MaxHeroSpeed = 0.06f;
		DampTime = 800;
		MusicVolume = .8f;
		SoundFxVolume = .8f;
		UpdateControls();
        Language.SwitchLanguage( "EN" );
        LanguageMenu.Index = 0;
    }

	//_____________________________________________________________________________________________________________________ Init Settings screen

	public void InitSettingsScreen()
	{
		if( gameObject.activeSelf ) return;
        if( ProfileWindow.I.gameObject.activeSelf ) return;
        if( Credits.I.gameObject.activeSelf ) return;
        if( SettingsWindow.I.gameObject.activeSelf ) return;

		if( QuestWindow.I.gameObject.activeSelf   ||  // Dont open settings if no profile is chosen
		    ProfileWindow.I.gameObject.activeSelf ||
		    Manager.I.ProfileNumber == -1         )
		{
			MainMenu.I.OpenProfileWindow();
			return;
		}

        MainMenu.I.LogoImage.gameObject.SetActive( false );
		gameObject.SetActive( true );
		LoadSettingsWindowData( Manager.I.ProfileNumber );
	}	
	
	//_____________________________________________________________________________________________________________________ Set action

	public void SetAction( int id, int btn )
    {
        cInput.ChangeKey( id, btn );
    }

	//_____________________________________________________________________________________________________________________ Button Callbacks

	public void Move_N_B1_Click()     { SetAction( 0, 1 ); }
	public void Move_N_B2_Click()     { SetAction( 0, 2 ); }
	public void Move_NE_B1_Click()    { SetAction( 1, 1 ); }
	public void Move_NE_B2_Click()    { SetAction( 1, 2 ); }
	public void Move_E_B1_Click()     { SetAction( 2, 1 ); }
	public void Move_E_B2_Click()     { SetAction( 2, 2 ); }
	public void Move_SE_B1_Click()    { SetAction( 3, 1 ); }
	public void Move_SE_B2_Click()    { SetAction( 3, 2 ); }
	public void Move_S_B1_Click()     { SetAction( 4, 1 ); }
	public void Move_S_B2_Click()     { SetAction( 4, 2 ); }
	public void Move_SW_B1_Click()    { SetAction( 5, 1 ); }
	public void Move_SW_B2_Click()    { SetAction( 5, 2 ); }
	public void Move_W_B1_Click()     { SetAction( 6, 1 ); }
	public void Move_W_B2_Click()     { SetAction( 6, 2 ); }
	public void Move_NW_B1_Click()    { SetAction( 7, 1 ); }
	public void Move_NW_B2_Click()    { SetAction( 7, 2 ); }
	public void Rotate_CW_B1_Click()  { SetAction( 8, 1 ); }
	public void Rotate_CW_B2_Click()  { SetAction( 8, 2 ); }
	public void Rotate_CCW_B1_Click() { SetAction( 9, 1 ); }
	public void Rotate_CCW_B2_Click() { SetAction( 9, 2 ); }
	public void Wait_B1_Click()       { SetAction( 10, 1 ); }
	public void Wait_B2_Click()       { SetAction( 10, 2 ); }
	public void Special_B1_Click()    { SetAction( 11, 1 ); }
	public void Special_B2_Click()    { SetAction( 11, 2 ); }
    public void Battle_B1_Click()     { SetAction( 13, 1 ); }
    public void Battle_B2_Click()     { SetAction( 13, 2 ); }
}
