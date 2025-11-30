using UnityEngine;
using System.Collections;

public class MuteInteraction : InteractionBehaviour 
{
	#region Private Variables

	bool m_isMuted = false;
	#if NGUI_DLL
	[SerializeField]
	UISprite m_spriteToChange = null;
	#endif
	[SerializeField]
	string m_muteSpriteName = "";
	[SerializeField]
	string m_normalSpriteName = "";
	[SerializeField]
	bool m_isBackgroundMusic = true;

	#endregion

	#region Public Variables

	public bool IsMuted {
		get {return m_isMuted;} 
		set 
		{
			if(m_isMuted == value)
				return;
			m_isMuted = value;
			ChangeSprite();
		}
	}
	#if NGUI_DLL
	public UISprite SpriteToChange 
	{
		get 
		{
			if(m_spriteToChange == null)
				m_spriteToChange = gameObject.GetComponent<UISprite>();
			return m_spriteToChange; 
		} 
		set {m_spriteToChange = value;}
	}
	#endif
	public string MuteSpriteName {get {return m_muteSpriteName;} set {m_muteSpriteName = value;}}
	public string NormalSpriteName {get {return m_normalSpriteName;} set {m_normalSpriteName = value;}}
	public bool IsBackgroundMusic {get {return m_isBackgroundMusic;} set {m_isBackgroundMusic = value;}}

	#endregion

	#region Unity Functions

	protected override void Awake()
	{
		base.Awake();
		CorrectValues();
	}

	protected override void Update()
	{
		CorrectValues();
		base.Update();
	}

	#endregion

	#region Helper Functions

	public override void Send()
	{
		CorrectValues();
		IsMuted = !IsMuted;
		ApplyInAudioManager();
	}

	protected virtual void CorrectValues()
	{
		if(AudioManager.Instance != null && AudioManager.Instance.BackgroundMusicSource != null)
		{
			if(IsBackgroundMusic)
				IsMuted = AudioManager.Instance.BackgroundMusicIsMuted;
			else
				IsMuted = AudioManager.Instance.EffectSoundIsMuted;
		}
	}

	protected virtual void ChangeSprite()
	{
		#if NGUI_DLL
		if(m_isMuted)
		{
			if(SpriteToChange != null)
				SpriteToChange.spriteName = MuteSpriteName;
		}
		else
		{
			if(SpriteToChange != null)
				SpriteToChange.spriteName = NormalSpriteName;
		}
		#endif
	}

	protected virtual void ApplyInAudioManager()
	{
		if(AudioManager.Instance != null)
		{
			if(IsBackgroundMusic)
				AudioManager.Instance.BackgroundMusicIsMuted = IsMuted;
			else
				AudioManager.Instance.EffectSoundIsMuted = IsMuted;
		}
	}

	#endregion
}
