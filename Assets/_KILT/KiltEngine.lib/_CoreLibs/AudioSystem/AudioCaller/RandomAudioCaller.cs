using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomAudioCaller : AudioCaller {
	
	#region Private Variables
	
	[SerializeField]
	RandomClipsStruct m_clipsToRandomize = new RandomClipsStruct();
	
	#endregion
	
	#region Public Properties
	
	public RandomClipsStruct ClipsToRandomize 
	{ 
		get 
		{
			return m_clipsToRandomize;
		}
		set
		{
			m_clipsToRandomize = value;
		}
	}
	
	#endregion
	
	#region Public Properties
	
	protected override void Awake()
	{
		Choose();
		base.Awake();
	}

	#region Events Registration
	
	private void Choose ()
	{
		if(ClipsToRandomize != null)
		{
			Clip = ClipsToRandomize.GetClip();
		}
	}
	
	#endregion
	
	#endregion
}

public enum RandomClipsStructEnum { RandomPlay }  

[System.Serializable]
public class RandomClipsStruct
{
	#region Private Variables
	[SerializeField]
	List<AudioClip> m_clips = new List<AudioClip>();
	[SerializeField]
	RandomClipsStructEnum m_playerOption = RandomClipsStructEnum.RandomPlay;
	[SerializeField]
	float m_probChanceToCall = 100f; // 0 to 100
	
	#endregion
	
	#region Public Properties

	public List<AudioClip> Clips 
	{
		get 
		{
			if(m_clips == null)
				m_clips = new List<AudioClip>();
			return m_clips;
		} 
		set {m_clips = value;}
	}
	public RandomClipsStructEnum PlayerOption {get {return m_playerOption;} set {m_playerOption = value;}}
	public float ProbChanceToCall 
	{
		get 
		{
			m_probChanceToCall = Mathf.Clamp(m_probChanceToCall,0,100f);
			return m_probChanceToCall;
		} 
		set 
		{
			m_probChanceToCall = Mathf.Clamp(value,0,100f);
		}
	}
	
	#endregion
	
	#region Public Properties
	
	public AudioClip GetClip()
	{
		AudioClip v_return = null;
		if(PlayerOption == RandomClipsStructEnum.RandomPlay)
		{
			float v_randomChanceValue = KiltUtils.RandomRange(0.0f,100.0f);
			if(Clips.Count > 0 &&  v_randomChanceValue < ProbChanceToCall)
				v_return = Clips[KiltUtils.RandomRange(0, Clips.Count)];
		}
		return v_return;
	}
	
	#endregion
}
