using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum RegisterUnregisterEventMoment {AwakeDestroy, EnableDisable} 

public class GenericReceiver<ShowStructureType> : MonoBehaviour where ShowStructureType : GenericShowObjectStruct
{
	#region Private Variables

	[SerializeField]
	RegisterUnregisterEventMoment m_eventMoment = RegisterUnregisterEventMoment.AwakeDestroy;
	[SerializeField]
	List<ShowStructureType> m_objectsToEnableOrDisable = new List<ShowStructureType>();
	
	#endregion
	
	#region Public Properties

	public RegisterUnregisterEventMoment EventMoment
	{
		get
		{
			return m_eventMoment;
		}
		set
		{
			if(m_eventMoment == value)
				return;
			m_eventMoment = value;
		}
	}

	public List<ShowStructureType> ObjectsToEnableOrDisable
	{
		get
		{
			if(m_objectsToEnableOrDisable == null)
				m_objectsToEnableOrDisable = new List<ShowStructureType>();
			return m_objectsToEnableOrDisable;
		}
		set
		{
			if(m_objectsToEnableOrDisable == value)
				return;
			m_objectsToEnableOrDisable = value;
		}
	}

	#endregion
	
	#region Unity Functions
	
	protected virtual void Start()
	{
	}
	
	protected virtual void Awake()
	{
		RegisterEvents();
	}

	protected virtual void OnEnable()
	{
		TryRegisterEvents(RegisterUnregisterEventMoment.EnableDisable);
	}

	protected virtual void OnDisable()
	{
		TryUnregisterEvents(RegisterUnregisterEventMoment.EnableDisable);
	}
	
	protected virtual void OnDestroy()
	{
		UnregisterEvents();
	}
	
	#endregion
	
	#region Helper Functions

	protected virtual void TryRegisterEvents(RegisterUnregisterEventMoment p_eventMoment)
	{
		if(p_eventMoment == EventMoment)
			RegisterEvents();
	}

	protected virtual void TryUnregisterEvents(RegisterUnregisterEventMoment p_eventMoment)
	{
		if(p_eventMoment == EventMoment)
			UnregisterEvents();
	}
	
	protected virtual void RegisterEvents()
	{
		UnregisterEvents();
	}
	
	protected virtual void UnregisterEvents()
	{
	}
	
	#endregion
}