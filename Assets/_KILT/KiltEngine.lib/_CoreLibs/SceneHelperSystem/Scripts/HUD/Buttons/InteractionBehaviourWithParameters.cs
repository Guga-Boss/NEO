using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kilt.Core;

public class InteractionBehaviourWithParameters<SceneParameterType> : InteractionBehaviour where SceneParameterType : GenericSceneParameter 
{
	#region Private Variables

	[SerializeField]
	bool m_clearAllOldParametersBeforeChangeScene = false; // This Option Will Clear All Others Parameters when before add this currentParamenters
	[SerializeField]
	List<SceneParameterType> m_parameters = new List<SceneParameterType>();

	#endregion

	#region Public Properties

	public bool ClearAllOldParametersBeforeChangeScene
	{ 
		get {return m_clearAllOldParametersBeforeChangeScene;} 
		set {m_clearAllOldParametersBeforeChangeScene = value;}
	}
	
	public List<SceneParameterType> Parameters
	{ 
		get 
		{
			if(m_parameters == null)
				m_parameters = new List<SceneParameterType>();
			return m_parameters;
		} 
		set 
		{
			if(m_parameters == value)
				return;
			m_parameters = value;
		}
	}
	
	#endregion

	#region Helper Functions

	protected override void BeforeSend()
	{
		FillParameters();
	}
	
	protected void FillParameters()
	{
		if(ClearAllOldParametersBeforeChangeScene)
			SceneManager.ParametersToPassToNextScene.ClearParameters();
		foreach(SceneParameterType v_parameter in Parameters)
		{
			SceneManager.AddParameter(v_parameter);
		}
	}

	#endregion
}

#region Helper Classes

[System.Serializable]
public class SceneStringParameter : SceneParameter<string>
{
}

public enum WhenAddParameterEnum {AddInNextScene, AddInCurrentScene, AlwaysAdd} 

[System.Serializable]
public class SceneParameter<ParameterType> : GenericSceneParameter
{
	#region Private Variables
	
	[SerializeField]
	ParameterType m_parameterValue;
	
	#endregion
	
	#region Public Properties
	
	
	public ParameterType ParameterValue
	{
		get
		{
			if(IsDifferentFromObjectParameterValue(m_parameterValue))
				ReplaceObjectParameterValue(m_parameterValue, false);
			return m_parameterValue;
		}
		set
		{
			if(EqualityComparer<ParameterType>.Default.Equals(m_parameterValue, value))
				return;
			m_parameterValue = value;
			ReplaceObjectParameterValue(value, false);
		}
	}
	
	public override object ObjectParameterValue
	{
		get
		{
			if(IsDifferentFromObjectParameterValue(m_parameterValue))
				ReplaceObjectParameterValue(m_parameterValue, false);
			return base.ObjectParameterValue;
		}
		set
		{
			ParameterType v_value = default(ParameterType);
			if(value is ParameterType)
			{
				v_value = (ParameterType)value;
				base.ObjectParameterValue = v_value;
				if(EqualityComparer<ParameterType>.Default.Equals(m_parameterValue, v_value))
					return;
				m_parameterValue = v_value;
			}
		}
	}
	
	#endregion
}

[System.Serializable]
public class GenericSceneParameter
{
	#region Private Variables
	
	[SerializeField]
	string m_parameterName = "";
	[SerializeField, HideInInspector]
	object m_objectParameterValue;
	[SerializeField]
	WhenAddParameterEnum m_whenAdd = WhenAddParameterEnum.AddInNextScene;
	[SerializeField]
	bool m_replaceOldParameterWithSameName = true;
	
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
	
	public virtual object ObjectParameterValue
	{
		get
		{
			return m_objectParameterValue;
		}
		set
		{
			ReplaceObjectParameterValue(value);
		}
	}
	
	public WhenAddParameterEnum WhenAdd
	{
		get
		{
			return m_whenAdd;
		}
		set
		{
			if(m_whenAdd == value)
				return;
			m_whenAdd = value;
		}
	}
	
	public bool ReplaceOldParameterWithSameName
	{
		get
		{
			return m_replaceOldParameterWithSameName;
		}
		set
		{
			if(m_replaceOldParameterWithSameName == value)
				return;
			m_replaceOldParameterWithSameName = value;
		}
	}
	
	#endregion
	
	#region Helper Functions
	
	protected bool IsDifferentFromObjectParameterValue(object p_valueToCheck)
	{
		if(p_valueToCheck != m_objectParameterValue)
			return true;
		return false;
	}
	
	protected void ReplaceObjectParameterValue(object p_valueToCheck, bool p_checkDifference = true)
	{
		if(!p_checkDifference || IsDifferentFromObjectParameterValue(p_valueToCheck))
			m_objectParameterValue = p_valueToCheck;
	}
	
	#endregion
}

#endregion
