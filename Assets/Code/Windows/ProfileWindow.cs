using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class ProfileWindow : MonoBehaviour 
{
	public static ProfileWindow I;
	public List<string> ProfileNames;
	public GameObject NameInputPanel;
	public int ProfileNum;
	public TextMeshPro ProfileName;
    //public tk2dUITextInput TextInput;

	//_____________________________________________________________________________________________________________________ Start

	public void Start()
	{
        I = this;
		NameInputPanel.gameObject.SetActive( false );
		ProfileNum = -1;
	}
	
	//_____________________________________________________________________________________________________________________ Update
	
	public void Update()
	{
		if( ProfileNum != -1 )
		{
			ProfileNames[ (int) ProfileNum ] = ProfileName.text;			
			MainMenu.I.ProfileButton.text = Language.Get("PROFILE_BUTTON")
            + ":\n" + ProfileNames[ (int) ProfileNum ];
			NameInputPanel.gameObject.SetActive( true );			
			if( Input.GetKeyDown( KeyCode.Return ) )
                FinalizeProfileScreen();
		}
	}

	//_____________________________________________________________________________________________________________________ Init Profile Screen

	public void InitProfileScreen()
	{
		if( SettingsWindow.I.gameObject.activeSelf ) SettingsWindow.I.FinalizeSettingsScreen();
        if( Credits.I.gameObject.activeSelf ) return;
		ProfileWindow.I.gameObject.SetActive( true );
        MainMenu.I.LogoImage.gameObject.SetActive( false );
	}

    //_____________________________________________________________________________________________________________________ Save Profile
    public void SaveProfileWindowData()
    {
        string file = Application.persistentDataPath + "/Profiles/ProfileNames.dat";                         // set file path
        if( Application.platform == RuntimePlatform.WindowsPlayer )
            file = Application.dataPath + "/Profiles/ProfileNames.dat";                                      // adjust path for Windows

        Directory.CreateDirectory( Path.GetDirectoryName( file ) );                                          // ensure folder exists

        using( MemoryStream ms = new MemoryStream() )
        using( BinaryWriter writer = new BinaryWriter( ms ) )                                                // open memory stream
        {
            GS.W = writer;                                                                                   // assign BinaryWriter to GS.W for TF
            int Version = Security.SaveHeader( 1, false );                                                   // save header defining current save version
            TF.SaveT( "ProfileNumber", Manager.I.ProfileNumber );                                            // save current profile number
            TF.SaveT( "ProfileNames", ProfileNames );                                                        // save profile names

            File.WriteAllBytes( file, ms.ToArray() );                                                        // write file directly 
        }
    }

    //_____________________________________________________________________________________________________________________ Load Profile
    public void LoadProfileWindowData()
    {
        string file = Application.persistentDataPath + "/Profiles/ProfileNames.dat";                         // set file path
        if( Application.platform == RuntimePlatform.WindowsPlayer )
            file = Application.dataPath + "/Profiles/ProfileNames.dat";                                      // adjust path for Windows

        if( Manager.I.ProfileNumber == -1 ) return;                                                          // skip if no profile selected
        if( !File.Exists( file ) ) return;                                                                   // skip if file missing

        byte[] fileData = File.ReadAllBytes(file);                                                           // read full file

        using( GS.R = new BinaryReader( new MemoryStream( fileData ) ) )                                     // use memory stream for TF
        {
            int SaveVersion = Security.LoadHeader( false );                                                  // load header
            Manager.I.ProfileNumber = TF.LoadT<int>( "ProfileNumber" );                                      // load profile number
            ProfileNames = TF.LoadT<List<string>>( "ProfileNames" );                                         // load profile names

            //MainMenu.I.ProfileButton.text = Language.Get( "PROFILE_BUTTON" )                                 // update main menu button
            //    + ":\n" + ProfileNames[ (int) Manager.I.ProfileNumber ];
            //ProfileName.text = ProfileNames[ (int) Manager.I.ProfileNumber ];                                // update current profile name
            ProfileNum = Manager.I.ProfileNumber;                                                            // set internal profile index
        }
    }


    //_____________________________________________________________________________________________________________________ Finalize  Profile

    public void FinalizeProfileScreen()
	{
		if( ProfileName.text == "" ) return;
		if( ProfileName.text == "Empty" ) return;
		if( ProfileNum == -1 ) return;
		MainMenu.I.ProfileButton.text = Language.Get("PROFILE_BUTTON")
                      + ":\n" + ProfileNames[ (int) ProfileNum ];
		gameObject.SetActive( false );

		Manager.I.ProfileNumber = ProfileNum;
		Manager.I.PlayerName = ProfileName.text;
        MainMenu.I.LogoImage.gameObject.SetActive( true );
		SaveProfileWindowData();
	}

	//_____________________________________________________________________________________________________________________ New profile clicked

	public void InitNewProfile()
    {
        Manager.I.QuestName = "Main Quest";
		ProfileName.text = ""; 
		//TextInput.SetFocus( true );
  //      TextInput.Text = "";
        SettingsWindow.I.RestoreDefaultSettings();
		SettingsWindow.I.SaveSettingsWindowData( ProfileNum );
    }

	//_____________________________________________________________________________________________________________________ Chose profile after button click

	public void ChooseProfile( int number, string pname )
	{
		ProfileNum = number;

		if( ProfileNames[ number ] == "Empty" )                                                              // New profile click
          { 
			InitNewProfile();
          } 
        else                                                                                                 // Existing profile click
          {
		    ProfileName.text = ProfileNames[ ProfileNum ];
			MainMenu.I.ProfileButton.text = Language.Get("PROFILE_BUTTON")
                        + ":\n" + ProfileNames[ (int) ProfileNum ];
			SettingsWindow.I.LoadSettingsWindowData( ProfileNum );
          }
	}
	
	//_____________________________________________________________________________________________________________________ Profile Buttons Calbacks

	public void ChooseProfileNumber0()
	{
		ChooseProfile( 0, ProfileName.text  );
	}
    //public void ChooseProfileNumber1()
    //{
    //    ChooseProfile( 1, ProfileName.text  );
    //}
    //public void ChooseProfileNumber2()
    //{
    //    ChooseProfile( 2, ProfileName.text  );
    //}
    //public void ChooseProfileNumber3()
    //{
    //    ChooseProfile( 3, ProfileName.text );
    //}
    //public void ChooseProfileNumber4()
    //{
    //    ChooseProfile( 4, ProfileName.text  );
    //}
    //public void ChooseProfileNumber5()
    //{
    //    ChooseProfile( 5, ProfileName.text  );
    //}
}
