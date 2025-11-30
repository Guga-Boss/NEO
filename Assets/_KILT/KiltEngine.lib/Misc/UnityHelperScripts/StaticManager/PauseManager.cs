using UnityEngine;
using System.Collections;

public class PauseManager 
{
	#region Events

	public static event Delegates.EventHandler OnPause;
	public static event Delegates.EventHandler OnResume;

	#endregion

	#region Static Variables

	static int _pauseCounter = 0;
	static float _oldTimeScale = 1;

	#endregion

	#region Helper Functions

	public static bool IsPaused()
	{
		return Time.timeScale == 0? true : false;
	}
	
	public static void Pause()
	{
		if(!IsPaused())
		{
			_oldTimeScale = Time.timeScale;
			Time.timeScale = 0;
			if(OnPause != null)
				OnPause();
		}
		_pauseCounter = Mathf.Max(0, _pauseCounter+1);
	}

	public static void Resume(bool p_force = false)
	{
		_pauseCounter = p_force ? 0 : Mathf.Max(0, _pauseCounter-1);
		if(IsPaused() && _pauseCounter == 0)
		{
			_oldTimeScale = Mathf.Max(0.1f, _oldTimeScale);
			Time.timeScale = _oldTimeScale;
			if(OnResume != null)
				OnResume();
		}
	}

	#endregion
}
