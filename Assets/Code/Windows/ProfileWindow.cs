using UnityEngine;
using System.Collections;

public class ProfileWindow : MonoBehaviour 
{
	public static ProfileWindow I;
	public tk2dTextMesh[] ProfileNames;
	public GameObject NameInputPanel;
	public int ProfileNum;
	public tk2dTextMesh ProfileName;
    public tk2dUITextInput TextInput;

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
			ProfileNames[ (int) ProfileNum ].text = ProfileName.text;
			
			for( int i = 0; i < ProfileNames.Length; i++ )
				if ( i == ProfileNum ) ProfileNames[ i ].color = new Color( 0, 1, 0, 1 );
			else ProfileNames[ i ].color = new Color( 1, 1, 1, 1 );
			MainMenu.I.ProfileButton.text = Language.Get("PROFILE_BUTTON")
                          + ":\n" + ProfileNames[ (int) ProfileNum ].text;
			NameInputPanel.gameObject.SetActive( true );
			
			if( Input.GetKeyDown( KeyCode.Return ) ) FinalizeProfileScreen();
		}
	}

	//_____________________________________________________________________________________________________________________ Init Profile Screen

	public void InitProfileScreen()
	{
		if( SettingsWindow.I.gameObject.activeSelf ) SettingsWindow.I.FinalizeSettingsScreen();
        if( Credits.I.gameObject.activeSelf ) return;
		if( QuestWindow.I.gameObject.activeSelf    ) QuestWindow.I.FinalizeQuestScreen();
		ProfileWindow.I.gameObject.SetActive( true );
        MainMenu.I.LogoImage.gameObject.SetActive( false );
	}

	//_____________________________________________________________________________________________________________________ Save Profile
	
	public void SaveProfileWindowData()
	{
        string file = Application.persistentDataPath + "/Profiles/ProfileNames.dat";
        if( Application.platform == RuntimePlatform.WindowsPlayer )
            file = Application.dataPath + "/Profiles/ProfileNames.dat";

		for( int i = 0; i < ProfileNames.Length; i++ )
			ES2.Save( ProfileNames[ i ].text, file + "?tag=ProfileNames: " + i );
		ES2.Save( Manager.I.ProfileNumber, file + "?tag=ProfileNumber" );
	}


	//_____________________________________________________________________________________________________________________ Load Profile

	public void LoadProfileWindowData()
    {
        string file = Application.persistentDataPath + "/Profiles/ProfileNames.dat";
        if( Application.platform == RuntimePlatform.WindowsPlayer )
            file = Application.dataPath + "/Profiles/ProfileNames.dat";
		if( ES2.Exists( file ) )
		{
			Manager.I.ProfileNumber = ES2.Load<int>( file + "?tag=ProfileNumber");
			if( Manager.I.ProfileNumber != -1 )
			{
				for( int i = 0; i < ProfileNames.Length; i++ )
					ProfileNames[ i ].text = ES2.Load<string>( file + "?tag=ProfileNames: " + i );
				MainMenu.I.ProfileButton.text = Language.Get("PROFILE_BUTTON")
					+ ":\n" + ProfileNames[ (int) Manager.I.ProfileNumber ].text;
				ProfileName.text = ProfileNames[ (int) Manager.I.ProfileNumber ].text;
				ProfileNum = Manager.I.ProfileNumber;
			}
		}
    }

	//_____________________________________________________________________________________________________________________ Finalize  Profile

	public void FinalizeProfileScreen()
	{
		if( ProfileName.text == "" ) return;
		if( ProfileName.text == "Empty" ) return;
		if( ProfileNum == -1 ) return;
		MainMenu.I.ProfileButton.text = Language.Get("PROFILE_BUTTON")
                      + ":\n" + ProfileNames[ (int) ProfileNum ].text;
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
		QuestWindow.I.QuestMenu.Index = 0;
        QuestWindow.I.SaveQuestWindowData();
		ProfileName.text = ""; 
		TextInput.SetFocus( true );
        TextInput.Text = "";
        SettingsWindow.I.RestoreDefaultSettings();
		SettingsWindow.I.SaveSettingsWindowData( ProfileNum );
    }

	//_____________________________________________________________________________________________________________________ Chose profile after button click

	public void ChooseProfile( int number, string pname )
	{
		ProfileNum = number;

		if( ProfileNames[ number ].text == "Empty" )                                                         // New profile click
          { 
			InitNewProfile();
          } 
        else                                                                                                 // Existing profile click
          {
		    ProfileName.text = ProfileNames[ ProfileNum ].text;
    		QuestWindow.I.LoadQuestWindowData( ProfileNum );
			MainMenu.I.ProfileButton.text = Language.Get("PROFILE_BUTTON")
                        + ":\n" + ProfileNames[ (int) ProfileNum ].text;
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
