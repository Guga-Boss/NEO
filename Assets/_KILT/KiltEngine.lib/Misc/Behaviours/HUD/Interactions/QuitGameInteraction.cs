using UnityEngine;
using System.Collections;

public class QuitGameInteraction : InteractionBehaviour {

	#region Helper Functions

	public override void Send()
	{
		base.Send();
		if(!Application.isEditor)
			Application.Quit();
		else
		{
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
			#endif
		}
	}

	#endregion
}
