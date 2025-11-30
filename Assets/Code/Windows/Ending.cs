using UnityEngine;
using System.Collections;

public class Ending : MonoBehaviour
{
	public static Ending I;
	public tk2dTextMesh MainText, TitleText;

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
			Input.GetKeyDown( KeyCode.Return ) ) ExitEnding();
	}

	public void ShowEnding()
	{
		Manager.I.Status = EGameStatus.ENDING;
		gameObject.SetActive( true );
		UI.I.gameObject.SetActive( false );
        MainText.text = Language.Get( "DEMO_ENDING" );
        MainText.text = MainText.text.Replace( "\\n", "\n" );
	}

	public void ExitEnding()
	{
		gameObject.SetActive( false );
		UI.I.gameObject.SetActive( true );
		Map.I.ClearTilemap();
        Map.I.ClearTransTilemap( new Vector2( -1, -1 ) );
		Manager.I.LoadLevel( 0 );
		Manager.I.GoToMainMenu( true );
	}
}
