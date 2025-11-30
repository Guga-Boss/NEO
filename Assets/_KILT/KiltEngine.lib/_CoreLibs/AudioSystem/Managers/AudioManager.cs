using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : PersistentObject {

	#region Singleton
	
	private static AudioManager m_instance = null;
	
	public static AudioManager Instance
	{
		get
		{
			if( m_instance == null )
			{
				m_instance = GameObject.FindObjectOfType(typeof(AudioManager)) as AudioManager;
				if(m_instance == null)
				{
					GameObject v_object = new GameObject("_AudioManager");
					m_instance = v_object.AddComponent<AudioManager>();
					if(!Application.isPlaying)
						KiltUtils.Destroy(v_object, true);
				}
			}
			
			return m_instance;
		}
		protected set
		{
			m_instance = value;
		}
	}
	
	#endregion

	#region Private Variables

	float m_backgroundVolume = 1f;
	float m_effectVolume = 1f;
	bool m_backgroundMusicIsMuted = false;
	bool m_effectSoundIsMuted = false;

	AudioSource m_backgroundMusicSource = null;
	List<AudioSource> m_soundEffectsSource = new List<AudioSource>();

	bool _needCleanAudio = true;

	#endregion

	#region Public Properties

	public AudioSource BackgroundMusicSource
	{ 
		get 
		{ 
			if(m_backgroundMusicSource == null)
			{
				GameObject v_object = GameObject.Find("BackgroundAudioObject"); //finding music player object
				if(v_object == null)
				{
					v_object = new GameObject("BackgroundAudioObject");
					v_object.transform.parent = this.transform;
					m_backgroundMusicSource = v_object.AddComponent<AudioSource>();
					m_backgroundMusicSource.loop = true;
					m_backgroundMusicSource.mute = BackgroundMusicIsMuted;
					m_backgroundMusicSource.volume = BackgroundVolume;
					m_backgroundMusicSource.playOnAwake = false;
				}
				else
				{
					m_backgroundMusicSource = v_object.GetComponent<AudioSource>();
					if(m_backgroundMusicSource == null)
					{
						m_backgroundMusicSource = v_object.AddComponent<AudioSource>();
						m_backgroundMusicSource.loop = true;
						m_backgroundMusicSource.mute = BackgroundMusicIsMuted;
						m_backgroundMusicSource.volume = BackgroundVolume;
						m_backgroundMusicSource.playOnAwake = false;
					}
				}
			}
			return m_backgroundMusicSource;
		}
	}

	public float EffectVolume {
		get {return m_effectVolume;} 
		set
		{
			if(m_effectVolume == value)
				return;
			m_effectVolume = value; 
			ApplyVolume();
			EffectSoundIsMuted = m_effectVolume <= 0? true : false;
		}
	}
	public float BackgroundVolume {
		get {return m_backgroundVolume;} 
		set
		{
			if(m_backgroundVolume == value)
				return;
			m_backgroundVolume = value; 
			ApplyVolume();
			BackgroundMusicIsMuted = m_backgroundVolume <= 0? true : false;
		}
	}
	
	public bool EffectSoundIsMuted {
		get {return m_effectSoundIsMuted;} 
		set
		{
			bool v_value = !value && m_effectVolume > 0? false : true;
			if(m_effectSoundIsMuted == v_value)
				return;
			m_effectSoundIsMuted = v_value;
			ApplyMute();
		}
	}
	public bool BackgroundMusicIsMuted 
	{
		get {return m_backgroundMusicIsMuted;} 
		set
		{
			bool v_value = !value && m_backgroundVolume > 0? false : true;
			if(m_backgroundMusicIsMuted == v_value)
				return;
			m_backgroundMusicIsMuted = v_value; 
			ApplyMute();
		}
	}

	public List<AudioSource> SoundEffectsSource
	{
		get 
		{ 
			if(m_soundEffectsSource == null)
				m_soundEffectsSource = new List<AudioSource>();
			return m_soundEffectsSource;
		}
		protected set
		{
			m_soundEffectsSource = value;
		}
	}

	#endregion

	#region Unity Functions

	protected virtual void OnLevelWasLoaded(int p_levelIndex)
	{
		ClearAudio(true);
	}

	protected override void Awake()
	{
		base.Awake();
		LoadFromPlayerPrefs();
		ClearAudio(true);
		ClearChildrens();

		if(m_instance != this && m_instance != null)
		{
			if(!Application.isPlaying && Application.isEditor)
				KiltUtils.Destroy(m_instance.gameObject);
			else
				Object.Destroy(m_instance.gameObject);
		}
		Instance = this;
	}

	protected virtual void OnEnable()
	{
		ApplyVolume();
		ApplyMute();
	}

	protected virtual void Update()
	{
		ClearAudio();
	}

	#endregion

	#region Helper Functions

	public void LoadFromPlayerPrefs()
	{
		float v_backVol = PlayerPrefs.GetFloat("BackVolume", BackgroundVolume);
		float v_effectVol =  PlayerPrefs.GetFloat("EffectVolume", EffectVolume);
		bool v_backMute = PlayerPrefs.GetInt("BackMute", BackgroundMusicIsMuted? 1 : 0) == 0? false : true;
		bool v_effectMute = PlayerPrefs.GetInt("EffectMute", EffectSoundIsMuted? 1 : 0) == 0? false : true;

		BackgroundVolume = v_backVol;
		EffectVolume = v_effectVol;
		BackgroundMusicIsMuted = v_backMute;
		EffectSoundIsMuted = v_effectMute;
	}

	public void SaveToPlayerPrefs()
	{
		PlayerPrefs.SetFloat("BackVolume", BackgroundVolume);
		PlayerPrefs.SetFloat("EffectVolume", EffectVolume);
		PlayerPrefs.SetInt("BackMute", BackgroundMusicIsMuted? 1 : 0);
		PlayerPrefs.SetInt("EffectMute", EffectSoundIsMuted? 1 : 0);
		PlayerPrefs.Save();
	}

	public void MarkToClean()
	{
		_needCleanAudio = true;
	}

	protected virtual  void PlayBackgroundMusicInternal(AudioClip p_sound)
	{
		if(p_sound != null)
		{
			if(BackgroundMusicSource.clip != p_sound)
			{
				if(BackgroundMusicSource.isPlaying)
					BackgroundMusicSource.Stop();
				BackgroundMusicSource.clip = p_sound;
			}
			BackgroundMusicSource.loop = true;
			BackgroundMusicSource.volume = BackgroundVolume;
			BackgroundMusicSource.mute = BackgroundMusicIsMuted;
			BackgroundMusicSource.playOnAwake = false;
			if(!BackgroundMusicSource.isPlaying)
				BackgroundMusicSource.Play();
		}
		else
			StopBackgroundMusicInternal();
	}
	
	protected virtual void PlaySoundInternal(AudioClip p_sound, bool p_isLoop = false, bool p_oneSoundOfThisTypeOnly = true , GameObject p_caller = null, float p_spartialBlend = 0.0f)
	{
		if(p_sound == null || (!p_isLoop && (EffectVolume <= 0 || EffectSoundIsMuted))) //Prevent Play When Muted to avoid Unnecessary Lag
			return;
		if(p_caller == null)
			p_caller = this.gameObject;
		
		//Check If any sound of this AudioClip is playing
		if(p_oneSoundOfThisTypeOnly)
		{
			foreach(AudioSource v_source in m_soundEffectsSource)
			{
				if(v_source != null && v_source.clip == p_sound && v_source.isPlaying)
					return;
				else if (v_source == null || !v_source.isPlaying)
					_needCleanAudio = true;
			}
		}
		
		//Find Any Source that is not playing
		AudioSource[] v_callerSources = p_caller.GetComponents<AudioSource>();
		AudioSource v_hostSource = null;
		foreach(AudioSource v_callerSource in v_callerSources)
		{
			if(v_callerSource != null && !v_callerSource.isPlaying)
			{
				//MarkToDestroy not Playing when HostSource != null
				if(v_hostSource != null)
				{
					KiltUtils.DestroyImmediate(v_callerSource);
					_needCleanAudio = true;
				}
				//Found One HostSource To PlaySound
				else
					v_hostSource = v_callerSource;
			}
		}
		//Add New AudioSource if not found
		if(v_hostSource == null)
			v_hostSource = p_caller.AddComponent<AudioSource>();

		v_hostSource.spatialBlend = Mathf.Clamp(p_spartialBlend, 0, 1f);
		v_hostSource.clip = p_sound;
		v_hostSource.loop = p_isLoop;
		v_hostSource.volume = EffectVolume;
		v_hostSource.mute = EffectSoundIsMuted;
		v_hostSource.playOnAwake = false;
		v_hostSource.Play();
		m_soundEffectsSource.AddChecking(v_hostSource);
	}
	
	protected virtual void StopBackgroundMusicInternal()
	{
		BackgroundMusicSource.Stop();
	}
	
	protected virtual void StopSoundInternal(AudioClip p_sound , GameObject p_caller = null)
	{
		if(p_caller == null)
			p_caller = this.gameObject;
		foreach(AudioSource v_source in m_soundEffectsSource)
		{
			if(v_source != null)
			{
				if(p_caller == v_source.gameObject && v_source.clip == p_sound)
					v_source.Stop();
			}
			else
				_needCleanAudio = true;
		}
	}

	protected virtual void ClearAudio(bool p_force = false)
	{
		if(p_force || _needCleanAudio)
		{
			_needCleanAudio = false;
			m_soundEffectsSource.RemoveNulls();
			List<AudioSource> v_clonedList = m_soundEffectsSource.CloneList();
			foreach(AudioSource v_source in v_clonedList)
			{
				if(v_source != null && !v_source.isPlaying)
					m_soundEffectsSource.RemoveChecking(v_source);
			}
		}
	}

	protected virtual void ClearChildrens()
	{
		AudioSource v_backgroundMusicSource = BackgroundMusicSource;
		foreach(Transform v_transform in transform)
		{
			if(v_transform != null && v_transform != v_backgroundMusicSource.transform)
				KiltUtils.Destroy(v_transform.gameObject);
		}
	}

	protected virtual void ApplyVolume()
	{
		SaveToPlayerPrefs();
		if(BackgroundMusicSource != null)
		{
			BackgroundMusicSource.volume = BackgroundVolume;
		}
		//Effect Sound
		foreach(AudioSource v_source in m_soundEffectsSource)
		{
			if(v_source != null)
				v_source.volume = EffectVolume;
			else
				_needCleanAudio = true;
		}
	}
	
	protected virtual void ApplyMute()
	{
		SaveToPlayerPrefs();
		if(BackgroundMusicSource != null)
			BackgroundMusicSource.mute = BackgroundMusicIsMuted;
		//EffectMute
		foreach(AudioSource v_source in m_soundEffectsSource)
		{
			if(v_source != null)
				v_source.mute = EffectSoundIsMuted;
			else
				_needCleanAudio = true;
		}
	}

	#endregion

	#region Static Methods

	public static bool InstanceExists()
	{
		return GetInstance(false) == null? false : true;
	}
	
	public static AudioManager GetInstance(bool p_canCreateANewOne = false)
	{
		AudioManager v_instance = null;
		if(p_canCreateANewOne)
			v_instance = Instance;
		else
		{
			if(m_instance == null )
				m_instance = GameObject.FindObjectOfType(typeof(AudioManager)) as AudioManager;
			v_instance = m_instance;
		}
		return v_instance;
	}

	public static void PlayBackgroundMusic(AudioClip p_sound)
	{
		if(Instance != null)
			Instance.PlayBackgroundMusicInternal(p_sound);
	}
	
	public static void PlaySound(AudioClip p_sound, bool p_isLoop = false, bool p_oneSoundOfThisTypeOnly = true , GameObject p_caller = null, float p_spartialBlend = 0.0f)
	{
		if(Instance != null)
			Instance.PlaySoundInternal(p_sound, p_isLoop, p_oneSoundOfThisTypeOnly, p_caller, p_spartialBlend);
	}
	
	public static void StopBackgroundMusic()
	{
		if(Instance != null)
			Instance.StopBackgroundMusicInternal();
	}
	
	public static void StopSound(AudioClip p_sound , GameObject p_caller = null)
	{
		if(Instance != null)
			Instance.StopSoundInternal(p_sound, p_caller);
	}

	#endregion


}
