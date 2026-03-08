using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DarkTonic.MasterAudio;
using TMPro;

public class SettingsWindow: MonoBehaviour
{
    public static SettingsWindow I;
    public TextMeshPro[] PrimaryActionButtonText;
    public TextMeshPro[] SecondaryActionButtonText;
    public float MaxHeroSpeed, KeyHoldDelay, DampTime, MusicVolume, SoundFxVolume;
    //public tk2dUITextInput KeyHoldTextInput, MaxHeroSpeedTextInput, CameraDampTextInput;
    //public tk2dUIScrollbar KeyHoldScrollbar, MaxHeroSpeedScrollbar, MusicVolumeScrollBar, SoundFxScrollBar;
    //public tk2dUIDropDownMenu LanguageMenu;
    public int OldLanguageIndex;

    //_____________________________________________________________________________________________________________________ Start
    public void Start()
    {
        I = this;                                                                                     // assign singleton instance
        OldLanguageIndex = -1;                                                                        // initialize old language
    }

    //_____________________________________________________________________________________________________________________ FixedUpdate
    public void FixedUpdate()
    {
        for( int i = 0; i < cInput.length; i++ )
        {
            if( PrimaryActionButtonText[ i ] )
                PrimaryActionButtonText[ i ].text = cInput.GetText( i, 1 );                          // update primary button text
            if( SecondaryActionButtonText[ i ] )
                SecondaryActionButtonText[ i ].text = cInput.GetText( i, 2 );                        // update secondary button text
        }

        //KeyHoldDelay = KeyHoldScrollbar.Value * 0.5f;                                                // scale key hold delay
        //KeyHoldTextInput.Text = "" + KeyHoldDelay.ToString( "0.00" );                                // update input field

        //if( CameraDampTextInput.Text != "" )
        //    DampTime = System.Convert.ToSingle( CameraDampTextInput.Text );                          // parse camera damp

        //DampTime = Mathf.Clamp( DampTime, 400, 1200 );                                               // clamp damp time

        //MaxHeroSpeed = MaxHeroSpeedScrollbar.Value * 0.16f;                                          // scale hero speed
        //MaxHeroSpeedTextInput.Text = "" + MaxHeroSpeed.ToString( "0.00" );                           // update input field

        //if( LanguageMenu.Index != OldLanguageIndex )
        //{
        //    Language.SwitchLanguage( LanguageMenu.selectedTextMesh.text );                           // change language
        //    OldLanguageIndex = LanguageMenu.Index;                                                   // update old index
        //}

        //SoundFxVolume = SoundFxScrollBar.Value;                                                      // read FX volume
        //MasterAudio.SetBusVolumeByName( "Sound FX", SoundFxVolume );                                 // apply FX volume
        //MusicVolume = MusicVolumeScrollBar.Value;                                                    // read music volume
        //MasterAudio.PlaylistMasterVolume = MusicVolume;                                              // apply music volume
    }

    //_____________________________________________________________________________________________________________________ Finalize Settings Screen
    public void FinalizeSettingsScreen()
    {
        gameObject.SetActive( false );                                                               // hide settings window
        SaveSettingsWindowData( Manager.I.ProfileNumber );                                           // save current settings
        UI.I.UpdateAllTranslations();                                                                // refresh UI texts
        MainMenu.I.LogoImage.gameObject.SetActive( true );                                           // show main menu logo
    }

    //_____________________________________________________________________________________________________________________ Save Settings
    public void SaveSettingsWindowData( int prof )
    {
        string file = Application.persistentDataPath + "/Profiles/Profile " + prof + "/Settings.dat";     // set file path
        if( Application.platform == RuntimePlatform.WindowsPlayer )
            file = Application.dataPath + "/Profiles/Profile " + prof + "/Settings.dat";                  // adjust path for Windows

        using( MemoryStream ms = new MemoryStream() )
        using( BinaryWriter writer = new BinaryWriter( ms ) )
        {
            GS.W = writer;                                                                                // assign BinaryWriter for TF
            int version = Security.SaveHeader( 1, false );                                                // save header version

            TF.SaveT( "MaxHeroSpeed", MaxHeroSpeed );                                                     // save hero speed
            TF.SaveT( "KeyHoldDelay", KeyHoldDelay );                                                     // save key hold delay
            TF.SaveT( "DampTime", DampTime );                                                             // save camera damp
            TF.SaveT( "MusicVolume", MusicVolume );                                                       // save music volume
            TF.SaveT( "SoundFxVolume", SoundFxVolume );                                                   // save FX volume
            //TF.SaveT( "LanguageIndex", LanguageMenu.Index );                                              // save language
            TF.SaveT( "LanguageIndex", 0 );                                              // save language

            for( int i = 0; i < Manager.I.InputNames.Length; i++ )
            {
                TF.SaveT( "PrimaryInput" + i, cInput.GetText( i, 1 ) );                                   // save primary key
                TF.SaveT( "SecondaryInput" + i, cInput.GetText( i, 2 ) );                                 // save secondary key
            }

            File.WriteAllBytes( file, ms.ToArray() );                                                     // write file directly, no crypto
        }
    }

    //_____________________________________________________________________________________________________________________ Load Settings
    public void LoadSettingsWindowData( int prof )
    {
        string file = Application.persistentDataPath + "/Profiles/Profile " + prof + "/Settings.dat";      // set file path
        if( Application.platform == RuntimePlatform.WindowsPlayer )
            file = Application.dataPath + "/Profiles/Profile " + prof + "/Settings.dat";                   // adjust path for Windows

        if( !File.Exists( file ) ) return;                                                                 // skip if no file

        byte[] fileData = File.ReadAllBytes(file);                                                         // read file bytes

        using( GS.R = new BinaryReader( new MemoryStream( fileData ) ) )                                   // read directly, no crypto
        {
            int saveVersion = Security.LoadHeader( false );                                                // read save header

            MaxHeroSpeed = TF.LoadT<float>( "MaxHeroSpeed" );                                              // load hero speed
            KeyHoldDelay = TF.LoadT<float>( "KeyHoldDelay" );                                              // load key hold delay
            DampTime = TF.LoadT<float>( "DampTime" );                                                      // load camera damp
            MusicVolume = TF.LoadT<float>( "MusicVolume" );                                                // load music volume
            SoundFxVolume = TF.LoadT<float>( "SoundFxVolume" );                                            // load FX volume

            int trash = TF.LoadT<int>( "LanguageIndex" );                                                  // load language
            //LanguageMenu.Index = TF.LoadT<int>( "LanguageIndex" );                                         // load language
            //Language.SwitchLanguage( LanguageMenu.selectedTextMesh.text );                                 // apply language

            for( int i = 0; i < Manager.I.InputNames.Length; i++ )
            {
                string prim = TF.LoadT<string>("PrimaryInput" + i);                                        // load primary key
                string sec = TF.LoadT<string>("SecondaryInput" + i);                                       // load secondary key
                //cInput.ChangeKey( Manager.I.InputNames[ i ], prim, sec );                                  // apply key
            }

            //UpdateControls();                                                                              // refresh UI
        }
    }


    //_____________________________________________________________________________________________________________________ Update Controls
    public void UpdateControls()
    {
        //CameraDampTextInput.Text = "" + DampTime;                                                     // update damp input
        //KeyHoldScrollbar.Value = KeyHoldDelay / 0.5f;                                                 // update key hold scrollbar
        //KeyHoldTextInput.Text = "" + KeyHoldDelay.ToString( "0.00" );                                 // update key hold input
        //MaxHeroSpeedScrollbar.Value = MaxHeroSpeed / 0.16f;                                           // update hero speed scrollbar
        //MaxHeroSpeedTextInput.Text = "" + MaxHeroSpeed.ToString( "0.00" );                            // update hero speed input
        //MusicVolumeScrollBar.Value = MusicVolume;                                                     // update music scrollbar
        //SoundFxScrollBar.Value = SoundFxVolume;                                                       // update FX scrollbar
    }

    //_____________________________________________________________________________________________________________________ Restore Default Settings
    public void RestoreDefaultSettings()
    {
        //cInput.ChangeKey( Manager.I.InputNames[ 0 ], "Alpha8", Keys.ArrowUp );                       // default N
        //cInput.ChangeKey( Manager.I.InputNames[ 1 ], "Alpha9", Keys.PageUp );                        // default NE
        //cInput.ChangeKey( Manager.I.InputNames[ 2 ], "O", Keys.ArrowRight );                         // default E
        //cInput.ChangeKey( Manager.I.InputNames[ 3 ], "L", Keys.PageDown );                           // default SE
        //cInput.ChangeKey( Manager.I.InputNames[ 4 ], "K", Keys.ArrowDown );                          // default S
        //cInput.ChangeKey( Manager.I.InputNames[ 5 ], "J", Keys.End );                                // default SW
        //cInput.ChangeKey( Manager.I.InputNames[ 6 ], "U", Keys.ArrowLeft );                          // default W
        //cInput.ChangeKey( Manager.I.InputNames[ 7 ], "Alpha7", Keys.Home );                          // default NW
        //cInput.ChangeKey( Manager.I.InputNames[ 8 ], "V", "W" );                                     // default rotate CW
        //cInput.ChangeKey( Manager.I.InputNames[ 9 ], "C", "Q" );                                     // default rotate CCW
        //cInput.ChangeKey( Manager.I.InputNames[ 10 ], "X", "Keypad5" );                              // default wait
        //cInput.ChangeKey( Manager.I.InputNames[ 11 ], "Z", "D" );                                    // default special
        //cInput.ChangeKey( Manager.I.InputNames[ 12 ], "N", "N" );                                    // default battle B1
        //cInput.ChangeKey( Manager.I.InputNames[ 13 ], "B", "B" );                                    // default battle B2

        KeyHoldDelay = 0.28f;                                                                        // reset key hold
        MaxHeroSpeed = 0.06f;                                                                        // reset hero speed
        DampTime = 800;                                                                              // reset damp
        MusicVolume = 0.8f;                                                                          // reset music volume
        SoundFxVolume = 0.8f;                                                                        // reset FX volume

        UpdateControls();                                                                            // refresh UI
        Language.SwitchLanguage( "EN" );                                                             // reset language
        //LanguageMenu.Index = 0;                                                                      // reset language menu
        Debug.Log( "Settings Reset." );
    }

    //_____________________________________________________________________________________________________________________ Init Settings Screen
    public void InitSettingsScreen()
    {
        if( gameObject.activeSelf ) return;                                                          // already open
        if( ProfileWindow.I.gameObject.activeSelf ) return;                                          // profile open
        if( Credits.I.gameObject.activeSelf ) return;                                                // credits open
        if( SettingsWindow.I.gameObject.activeSelf ) return;                                         // settings open

        if( Manager.I.ProfileNumber == -1 )                                                          // require profile
        {
            MainMenu.I.OpenProfileWindow();                                                          // open profile window
            return;
        }

        MainMenu.I.LogoImage.gameObject.SetActive( false );                                          // hide logo
        gameObject.SetActive( true );                                                                // show settings
        LoadSettingsWindowData( Manager.I.ProfileNumber );                                           // load saved settings
    }

    //_____________________________________________________________________________________________________________________ Set Action
    public void SetAction( int id, int btn )
    {
        //cInput.ChangeKey( id, btn );                                                                // assign key to input
    }

    //_____________________________________________________________________________________________________________________ Button Callbacks
    public void Move_N_B1_Click() { SetAction( 0, 1 ); }
    public void Move_N_B2_Click() { SetAction( 0, 2 ); }
    public void Move_NE_B1_Click() { SetAction( 1, 1 ); }
    public void Move_NE_B2_Click() { SetAction( 1, 2 ); }
    public void Move_E_B1_Click() { SetAction( 2, 1 ); }
    public void Move_E_B2_Click() { SetAction( 2, 2 ); }
    public void Move_SE_B1_Click() { SetAction( 3, 1 ); }
    public void Move_SE_B2_Click() { SetAction( 3, 2 ); }
    public void Move_S_B1_Click() { SetAction( 4, 1 ); }
    public void Move_S_B2_Click() { SetAction( 4, 2 ); }
    public void Move_SW_B1_Click() { SetAction( 5, 1 ); }
    public void Move_SW_B2_Click() { SetAction( 5, 2 ); }
    public void Move_W_B1_Click() { SetAction( 6, 1 ); }
    public void Move_W_B2_Click() { SetAction( 6, 2 ); }
    public void Move_NW_B1_Click() { SetAction( 7, 1 ); }
    public void Move_NW_B2_Click() { SetAction( 7, 2 ); }
    public void Rotate_CW_B1_Click() { SetAction( 8, 1 ); }
    public void Rotate_CW_B2_Click() { SetAction( 8, 2 ); }
    public void Rotate_CCW_B1_Click() { SetAction( 9, 1 ); }
    public void Rotate_CCW_B2_Click() { SetAction( 9, 2 ); }
    public void Wait_B1_Click() { SetAction( 10, 1 ); }
    public void Wait_B2_Click() { SetAction( 10, 2 ); }
    public void Special_B1_Click() { SetAction( 11, 1 ); }
    public void Special_B2_Click() { SetAction( 11, 2 ); }
    public void Battle_B1_Click() { SetAction( 13, 1 ); }
    public void Battle_B2_Click() { SetAction( 13, 2 ); }
}
