using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScheduledContainerAudioCaller : AudioCaller {

	#region Private Variables

	[SerializeField]
	List<ClipPerContainerState> m_clipsPerStates = new List<ClipPerContainerState>();

	#endregion
	
	#region Public Properties

	public List<ClipPerContainerState> ClipsPerStates 
	{ 
		get 
		{
			if(m_clipsPerStates == null)
				m_clipsPerStates = new List<ClipPerContainerState>();
			return m_clipsPerStates;
		}
		set
		{
			m_clipsPerStates = value;
		}
	}

	public override SoundTriggerEnum SoundPlayerOption 
	{
		get{
			base.SoundPlayerOption = SoundTriggerEnum.Custom;
			return base.SoundPlayerOption;
		} 
		set 
		{
			base.SoundPlayerOption = SoundTriggerEnum.Custom;
		}
	}

	public override SoundTriggerEnum SoundStopperOption {
		get{
			base.SoundStopperOption = SoundTriggerEnum.Custom;
			return base.SoundStopperOption;
		} 
		set 
		{
			base.SoundStopperOption = SoundTriggerEnum.Custom;
		}
	}
	
	#endregion

	#region Unity Functions
	
	protected override void Awake()
	{
		base.Awake();
		//Clear Clip to Only Play Correct One
		Clip = null;
		SoundPlayerOption = SoundTriggerEnum.Custom;
		SoundStopperOption = SoundTriggerEnum.Custom;
	}
	
	#endregion
	
	#region Events Registration

	protected virtual void OnPanelStateChanged(PanelStateEnum p_panelState)
	{
		if(enabled && gameObject.activeInHierarchy && gameObject.activeSelf)
		{
			foreach(ClipPerContainerState v_struct in ClipsPerStates)
			{
				if(v_struct != null && v_struct.PanelState == p_panelState)
				{
					Clip = v_struct.Clip;
					Play();
				}
			}
		}
	}

	#endregion
}

[System.Serializable]
public class ClipPerContainerState 
{
	#region Private Variables

	[SerializeField]
	PanelStateEnum m_panelState = PanelStateEnum.Opened;
	[SerializeField]
	AudioClip m_clip = null;

	#endregion

	#region Public Properties

	public PanelStateEnum PanelState {get {return m_panelState;} set {m_panelState = value;}}
	public AudioClip Clip {get {return m_clip;} set {m_clip = value;}}

	#endregion
}
