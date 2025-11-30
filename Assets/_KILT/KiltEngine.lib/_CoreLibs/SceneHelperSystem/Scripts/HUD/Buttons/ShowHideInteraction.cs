using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShowHideInteraction : InteractionBehaviourWithParameters<SceneStringParameter> {

	#region Private Variables
	
	[SerializeField]
	List<ObjectShowActionStruct> m_targets = new List<ObjectShowActionStruct>();
	
	#endregion
	
	#region Public Properties

	public List<ObjectShowActionStruct> Targets
	{ 
		get {
			if(m_targets == null)
				m_targets = new List<ObjectShowActionStruct>();
			return m_targets;
		} 
		set {m_targets = value;}
	}
	
	#endregion

	#region Helper Functions

	public override void Send()
	{
		foreach(ObjectShowActionStruct v_objectTarget in Targets)
		{
			if(v_objectTarget != null && v_objectTarget.ObjectToApply != null)
			{
				KiltUtils.SetContainerVisibility(v_objectTarget.ObjectToApply, v_objectTarget.Action, v_objectTarget.DelayToCallAction, false);
			}
		}
	}

	#endregion
}
