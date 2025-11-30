using UnityEngine;
using System.Collections;

public class RetryLevelInteraction : ChangeSceneInteraction
{
	public override string StageToLoadOnClick
	{
		get 
		{
			#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_4
			return Application.loadedLevelName;
			#else
			return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
			#endif
		}
		set
		{
		}
	}
}
