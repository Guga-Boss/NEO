using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Kilt.Core;

public class ParameterNameReceiver : GenericReceiver<ShowObjectByParameterNameStruct> 
{
	#region Private Variables

	[SerializeField]
	List<ParameterCallbackStruct> m_parametersCallback = new List<ParameterCallbackStruct>();
	
	#endregion

	#region Public Properties
	
	public List<ParameterCallbackStruct> ParametersCallback
	{
		get
		{
			if(m_parametersCallback == null)
				m_parametersCallback = new List<ParameterCallbackStruct>();
			return m_parametersCallback;
		}
		set
		{
			if(m_parametersCallback == value)
				return;
			m_parametersCallback = value;
		}
	}
	
	#endregion

	#region Events
	
	protected virtual void HandleOnParameterAdded(string p_name, object p_value)
	{
		CallActions(p_name, p_value);
	}
	
	#endregion
	
	#region Helper Functions
	
	protected virtual void CallActions(string p_name, object p_value)
	{
		foreach(ShowObjectByParameterNameStruct v_struct in ObjectsToEnableOrDisable)
		{
			if(v_struct != null && v_struct.ObjectToApply != null)
			{
				if(v_struct.StateActionsList != null)
				{
					foreach (ParameterNameStateAction v_stateAction in v_struct.StateActionsList)
					{
						if(v_stateAction != null)
						{
							if(v_stateAction.StateToMakeAction.Equals(p_name))
							{
								KiltUtils.SetContainerVisibility(v_struct.ObjectToApply, v_stateAction.Action, v_stateAction.DelayToCallAction, true);
								break;
							}
						}
					}
				}
			}
		}

		foreach(ParameterCallbackStruct v_struct in ParametersCallback)
		{
			if(v_struct != null && string.Equals(v_struct.ParameterName, p_name))
			{
				if(v_struct.OnParameterReceivedCallback != null)
					v_struct.OnParameterReceivedCallback.Invoke();
			}
		}
	}
	
	protected override void RegisterEvents()
	{
		UnregisterEvents();
		SceneManager.OnParameterAdded += HandleOnParameterAdded;
	}
	
	protected override void UnregisterEvents()
	{
		SceneManager.OnParameterAdded -= HandleOnParameterAdded;
	}
	
	#endregion
}
	
	#region Helper Classes
	
	[System.Serializable]
	public class ShowObjectByParameterNameStruct : GenericShowObjectStructWithAction<ParameterNameStateAction>
	{
	}
	
	[System.Serializable]
	public class ParameterNameStateAction : GenericStateActionWithConditions<string>
	{
	}

	[System.Serializable]
	public class ParameterCallbackStruct
	{
		#region Private Variables

		[SerializeField]
		string m_parameterName = "";

		#endregion

		#region Public Properties
		
		public string ParameterName
		{
			get
			{
				return m_parameterName;
			}
			set
			{
				if(m_parameterName == value)
					return;
				m_parameterName = value;
			}
		}
		
		#endregion

		#region Callback
		
		public UnityEvent OnParameterReceivedCallback;
		
		#endregion
	}
	
	#endregion

