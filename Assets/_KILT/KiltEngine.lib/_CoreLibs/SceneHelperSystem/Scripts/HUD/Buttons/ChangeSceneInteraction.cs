using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kilt.Core;

public class ChangeSceneInteraction : InteractionBehaviourWithParameters<SceneStringParameter> {

	#region Private Variables

	[SerializeField]
	SceneChangerWidget m_owner = null;
	[SerializeField]
	string m_stageToLoadOnClick = "";
	[SerializeField]
	float m_fadeTime = 1f;

	#endregion

	#region Public Properties

	public SceneChangerWidget Owner{ 
		get 
		{
			try
			{
				if(m_owner == null)
					m_owner = transform.parent.gameObject.GetComponent<SceneChangerWidget>();
			}
			catch{m_owner = null;}
			return m_owner;
		} 
		set 
		{
			m_owner = value;
		}
	}

	public virtual string StageToLoadOnClick
	{ 
		get {return m_stageToLoadOnClick;} 
		set {m_stageToLoadOnClick = value;}
	}

	public float FadeTime
	{ 
		get {return m_fadeTime;} 
		set {m_fadeTime = value;}
	}

	#endregion

	#region Unity Functions
	
	protected override void Awake()
	{
		base.Awake();
		PauseAction = PauseActionEnum.Resume;
	}
	
	#endregion

	#region Helper Functions

	public override void Send()
	{
		ChangeScene();
	}

	public bool ChangeScene()
	{
		try
		{
			#if NGUI_DLL
			if(m_owner == null || !m_owner.IsLocked)
			#endif
			{
				return ChangeSceneInternal(StageToLoadOnClick);
			}
		}
		catch{}
		return false;
	}
	
	protected bool ChangeSceneInternal(string p_stageName)
	{
		FillParameters();
		if(Application.CanStreamedLevelBeLoaded(p_stageName))
			return SceneManager.LoadLevelFading(p_stageName, Color.black, m_fadeTime);
		else if(!string.IsNullOrEmpty(p_stageName))
			Debug.LogWarning("Scene " + p_stageName + " must be in build. Go to File->Build Settings and add this scene.");
		else
			Debug.LogWarning("Scene name is empty.");
		return false;
	}

	//Override To Prevent Call FillParameters
	protected override void BeforeSend()
	{
	}

	#endregion
}

