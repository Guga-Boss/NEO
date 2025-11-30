using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SoundTriggerEnum {Custom, OnAwake, OnEnable, OnDisable, OnDestroy, OnGlobalPress, OnGlobalRelease, OnClick, OnPress, OnRelease, OnMouseOver, OnMouseOut, OnFirstUpdate, OnStart}

public class AudioCaller : MonoBehaviour {

	#region Private Variables

	[SerializeField]
	AudioClip m_clip = null;

	//Background Music Properties
	[SerializeField]
	bool m_isBackgroundMusic = false;
	
	//Sound Effect Properties
	[SerializeField, DependentFieldAttribute("m_isBackgroundMusic", DependenceOptionEnum.InverseDependence, CheckerFalseDrawOptionEnum.DontDrawProperty)]
	bool m_loop = false;
	[SerializeField, DependentFieldAttribute("m_isBackgroundMusic", DependenceOptionEnum.InverseDependence, CheckerFalseDrawOptionEnum.DontDrawProperty)]
	bool m_oneSoundOfThisTypeOnly = true;
	[SerializeField, Range(0,1)]
	float m_spartialBlend = 0;


	//Special Options
	[SerializeField]
	bool m_canPlayInFade = true;
	[SerializeField]
	SoundTriggerEnum m_soundPlayerOption = SoundTriggerEnum.OnStart;
	[SerializeField]
	SoundTriggerEnum m_soundStopperOption = SoundTriggerEnum.Custom;

	#endregion

	#region Public Properties

	// Properties Can Be Replaced to Prevent Inherited Members to Make Any Action

	public virtual AudioClip Clip {get{return m_clip;} set {m_clip = value; MarkToClean();}}
	public virtual SoundTriggerEnum SoundStopperOption {get{return m_soundStopperOption;} set {m_soundStopperOption = value;}}
	public virtual float SpartialBlend {get{return m_spartialBlend;} set {m_spartialBlend = value;}}
	public virtual bool IsBackgroundMusic {get{return m_isBackgroundMusic;} set {m_isBackgroundMusic = value;}} 
	public virtual bool Loop {get{return m_loop;} set {m_loop = value;}}
	public virtual bool OneSoundOfThisTypeOnly {get{return m_oneSoundOfThisTypeOnly;} set {m_oneSoundOfThisTypeOnly = value;}}
	public virtual bool CanPlayInFade {get{return m_canPlayInFade;} set {m_canPlayInFade = value;}}
	public virtual SoundTriggerEnum SoundPlayerOption {get{return m_soundPlayerOption;} set {m_soundPlayerOption = value;}}


	#endregion

	#region Unity Functions

	protected virtual void Awake()
	{
		RegisterEvents();
		if(SoundStopperOption == SoundTriggerEnum.OnAwake)
			Stop();
		if(SoundPlayerOption == SoundTriggerEnum.OnAwake)
			Play();
	}

	protected virtual void Start()
	{
		RegisterEvents();
		if(SoundStopperOption == SoundTriggerEnum.OnStart)
			Stop();
		if(SoundPlayerOption == SoundTriggerEnum.OnStart)
			Play();
	}
	
	protected virtual void OnDestroy()
	{
		AudioManager v_instance = AudioManager.GetInstance(false);
		if(Application.isPlaying && v_instance != null)
		{
			if(IsBackgroundMusic)
			{
				if(SoundStopperOption == SoundTriggerEnum.OnDestroy)
					Stop();
				if(SoundPlayerOption == SoundTriggerEnum.OnDestroy)
					Play();
			}
		}
		UnregisterEvents();
		MarkToClean();
	}

	protected virtual void OnEnable()
	{
		if(SoundStopperOption == SoundTriggerEnum.OnEnable)
			Stop();
		if(SoundPlayerOption == SoundTriggerEnum.OnEnable)
			Play();
	}

	protected virtual void OnDisable()
	{
		if(SoundStopperOption == SoundTriggerEnum.OnDisable)
			Stop();
		if(SoundPlayerOption == SoundTriggerEnum.OnDisable)
			Play();
	}

	bool _firstUpdate = true;
	protected virtual void Update()
	{
		if(_firstUpdate)
		{
			_firstUpdate = false;
			if(SoundStopperOption == SoundTriggerEnum.OnFirstUpdate)
				Stop();
			if(SoundPlayerOption == SoundTriggerEnum.OnFirstUpdate)
				Play();
		}
	}

	#endregion

	#region Events Registration
	
	protected virtual void OnClick()
	{
		if(SoundStopperOption == SoundTriggerEnum.OnClick)
			Stop();
		if(SoundPlayerOption == SoundTriggerEnum.OnClick)
			Play();
	}

	protected virtual void OnHover (bool p_hovered)
	{
		if(p_hovered)
		{
			if(SoundStopperOption == SoundTriggerEnum.OnMouseOver)
				Stop();
			if(SoundPlayerOption == SoundTriggerEnum.OnMouseOver)
				Play();
		}
		else
		{
			if(SoundStopperOption == SoundTriggerEnum.OnMouseOut)
				Stop();
			if(SoundPlayerOption == SoundTriggerEnum.OnMouseOut)
				Play();
		}
	}

	protected virtual void OnPress(bool p_pressed)
	{
		if(p_pressed)
		{
			if(SoundStopperOption == SoundTriggerEnum.OnPress)
				Stop();
			if(SoundPlayerOption == SoundTriggerEnum.OnPress)
				Play();
		}
		else
		{
			if(SoundStopperOption == SoundTriggerEnum.OnRelease)
				Stop();
			if(SoundPlayerOption == SoundTriggerEnum.OnRelease)
				Play();
		}
	}
	
	protected virtual void HandleOnGlobalPress (bool p_pressed)
	{
		if(p_pressed)
		{
			if(SoundStopperOption == SoundTriggerEnum.OnGlobalPress)
				Stop();
			if(SoundPlayerOption == SoundTriggerEnum.OnGlobalPress)
				Play();
		}
		else
		{
			if(SoundStopperOption == SoundTriggerEnum.OnGlobalRelease)
				Stop();
			if(SoundPlayerOption == SoundTriggerEnum.OnGlobalRelease)
				Play();
		}
	}

	#endregion

	#region Helper Functions
	
	protected virtual void RegisterEvents()
	{
		UnregisterEvents();
		GlobalPressController.OnGlobalPress += HandleOnGlobalPress;
	}
	
	protected virtual void UnregisterEvents()
	{
		GlobalPressController.OnGlobalPress -= HandleOnGlobalPress;
	}
	
	protected virtual void MarkToClean()
	{
		AudioManager v_instance = AudioManager.GetInstance(false);
		if(Application.isPlaying && v_instance != null)
			v_instance.MarkToClean();
	}
	
	#endregion

	#region Audio Functions

	public void Play(bool p_force = false)
	{
		if(p_force || CanPlayInFade || !CameraFade.InstanceExists())
		{
			if(IsBackgroundMusic)
				AudioManager.PlayBackgroundMusic(Clip);
			else
				AudioManager.PlaySound(Clip, Loop, OneSoundOfThisTypeOnly, this.gameObject);
		}
	}
	
	public void Stop()
	{
		if(IsBackgroundMusic)
			AudioManager.StopBackgroundMusic();
		else
			AudioManager.StopSound(Clip, this.gameObject);
	}
	
	#endregion
}
