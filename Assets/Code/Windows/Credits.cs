using UnityEngine;
using System.Collections;

public class Credits : MonoBehaviour
{
    public static Credits I;
    public tk2dTextMesh MainText, TitleText;
    public tk2dUIItem[] PageButton;
    public tk2dTextMesh [] PageButtonText;
    public int CurrentPage = 1;

    // Use this for initialization
    void Start()
    {
        I = this;
        gameObject.SetActive( false );
    }

    // Update is called once per frame
    void Update()
    {
        if( Input.GetKeyDown( KeyCode.F2 ) ||
            Input.GetKeyDown( KeyCode.Return ) ) ExitCredits();
        MainText.text = Language.Get( "CREDITS_TEXT_PAGE_" + CurrentPage );
        MainText.text = MainText.text.Replace( "\\n", "\n" );
        for( int i = 0; i < PageButton.Length; i++ )
        {
            PageButtonText[ i ].color = Color.white;
            if( i == CurrentPage - 1 )
                PageButtonText[ i ].color = Color.green;
        }
    }

    public void ShowCredits()
    {
        if( ProfileWindow.I.gameObject.activeSelf ) return;
        if( SettingsWindow.I.gameObject.activeSelf ) 
            SettingsWindow.I.FinalizeSettingsScreen();
        Manager.I.Status = EGameStatus.CREDITS;
        CurrentPage = 1;
        gameObject.SetActive( true );
        UI.I.gameObject.SetActive( false );
        MainMenu.I.LogoImage.gameObject.SetActive( false );
    }

    public void ExitCredits()
    {
        gameObject.SetActive( false );
        UI.I.gameObject.SetActive( true );
        MainMenu.I.LogoImage.gameObject.SetActive( true );
        Manager.I.GoToMainMenu( false );
    }

    public void PageButton1Callback()
    {
        CurrentPage = 1;
    }
    public void PageButton2Callback()
    {
        CurrentPage = 2;
    }
    public void PageButton3Callback()
    {
        CurrentPage = 3;
    }
}
