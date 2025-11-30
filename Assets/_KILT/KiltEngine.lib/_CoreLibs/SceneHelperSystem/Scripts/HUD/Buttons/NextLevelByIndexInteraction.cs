using UnityEngine;
using System.Collections;
using Kilt.Core;

public class NextLevelByIndexInteraction : ChangeSceneInteraction
{
	public override string StageToLoadOnClick
	{
		get 
		{
			#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_4
			return SceneManager.GetLevelNameByIndex(Application.loadedLevel + 1);
			#else
			return SceneManager.GetLevelNameByIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
			#endif
		}
		set
		{
		}
	}
}
