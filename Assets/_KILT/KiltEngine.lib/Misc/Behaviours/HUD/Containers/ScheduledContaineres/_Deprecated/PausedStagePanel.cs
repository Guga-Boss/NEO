using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PausedStagePanel : ScheduledContainer
{
	#region Private Variables

	[SerializeField]
	GameObject m_selectLevelButton = null;
	[SerializeField]
	GameObject m_resumeButton = null;
	[SerializeField]
	bool m_registerPauseManagerEvents = false;

	#endregion
	
	#region Public Properties

	public GameObject SelectLevelButton {get {return m_selectLevelButton;} set {m_selectLevelButton = value;}}
	public GameObject ResumeButton {get {return m_resumeButton;} set {m_resumeButton = value;}}
	public bool RegisterPauseManagerEvents {get {return m_registerPauseManagerEvents;} set {m_registerPauseManagerEvents = value;}}

	#endregion
	
	#region Unity Functions
	
	protected override void Awake()
	{
		base.Awake();
		InitEvents();
	}

	protected virtual void OnDestroy()
	{
		RemoveEvents();
	}
	
	#endregion

	#region Helper Functions

	protected void InitEvents()
	{
		RemoveEvents();
		if(RegisterPauseManagerEvents)
		{
			PauseManager.OnPause += HandleOnPause;
			PauseManager.OnResume += HandleOnResume;
		}
		OnClosed += HandleOnClosed;
		OnOpening += HandleOnOpening;
		OnOpened += HandleOnOpened;
	}
	
	protected void RemoveEvents()
	{
		PauseManager.OnPause -= HandleOnPause;
		PauseManager.OnResume -= HandleOnResume;
		OnClosed -= HandleOnClosed;
		OnOpening -= HandleOnOpening;
		OnOpened -= HandleOnOpened;
	}

	#endregion

	#region Event Registration

	void HandleOnPause()
	{
		Show ();
	}

	void HandleOnResume()
	{
		Hide (true);
	}

	void HandleOnOpened()
	{
		PauseManager.Pause();
	}

	void HandleOnOpening ()
	{
		PauseManager.Pause();
	}
	
	void HandleOnClosed ()
	{
		PauseManager.Resume(true);
	}

	#endregion
}
