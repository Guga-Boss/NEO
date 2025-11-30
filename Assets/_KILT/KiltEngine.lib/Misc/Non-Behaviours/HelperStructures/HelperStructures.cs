using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8
using Kilt.RT.Reflection;
#else
using System.Reflection;
#endif

#region Type Structures

[System.Serializable]
public class SerializableType : FoldOutStruct
{
	#region Private Variables

	[SerializeField, PropertyBackingFieldAttribute]
	string m_stringType = "";

	[HideInInspector,SerializeField]
	System.Type m_castedType = null;

	#endregion

	#region Public Properties
	
	public string StringType
	{
		get
		{
			return m_stringType;
		}
		set
		{
			if(m_stringType == value)
				return;
			m_stringType = value;
			//CheckIfNeedReapplyType(true);
		}
	}

	public System.Type CastedType
	{
		get
		{
			CheckIfNeedReapplyType(false);
			return m_castedType;
		}
		set
		{
			if(m_castedType == value)
				return;
			m_castedType = value;
			m_stringType = GetStringTypeFromType(value);
		}
	}
	
	#endregion

	#region Construtor

	public SerializableType() 
	{
	}

	public SerializableType(System.Type p_type) 
	{
		m_castedType = p_type;
		m_stringType =  GetStringTypeFromType(p_type);
	}

	#endregion

	#region Helper Functions

	protected string GetStringTypeFromType(System.Type p_type)
	{
        //METRO CHANGED ASSEMBLY see: https://msdn.microsoft.com/en-us/library/windows/apps/br230302(v=VS.85).aspx#reflection
        #if UNITY_WINRT && !UNITY_EDITOR && !UNITY_WP8
		return p_type != null? p_type.FullName + ", " + p_type.Assembly().FullName : "";
        #else
        return p_type != null? p_type.FullName + ", " + p_type.Assembly.FullName : "";
		#endif
	}

	public void CheckIfNeedReapplyType(bool p_force = false)
	{
		if((m_castedType == null && !string.IsNullOrEmpty(m_stringType)) || p_force)
		{
			m_castedType = System.Type.GetType(m_stringType);
		}
	}

	#endregion

	#region Operator Overloads

	public static implicit operator System.Type(SerializableType p_type) 
	{
		return p_type != null? p_type.CastedType : null;
	}

	public static implicit operator SerializableType(System.Type p_type) 
	{
		return new SerializableType(p_type);
	}

	// Override the Object.Equals(object o) method:
	public override bool Equals(object p_object) 
	{
		try 
		{
			if(p_object is System.Type)
				return ((System.Type)this == (System.Type)p_object);
			else
				return (this == (SerializableType) p_object);
		}
		catch 
		{
			return false;
		}
	}

	public override int GetHashCode() 
	{
		return CastedType != null? CastedType.GetHashCode() : 0;
	}

	public override string ToString() 
	{
		return GetStringTypeFromType(CastedType);
	}

	#endregion
}

#endregion

#region Editor Helper Structures

[System.Serializable]
public abstract class FoldOutStruct
{
	#region Inspector Properties
	
	//Editor Util
	[HideInInspector,SerializeField]
	bool _foldOut = false;

	public bool FoldOut
	{
		get
		{
			return _foldOut;
		}
		set
		{
			if(_foldOut == value)
				return;
			_foldOut = value;
		}
	}
	
	#endregion
}

#endregion

#region ScheduledContainer/Receivers Helper Structs

[System.Serializable]
public abstract class GenericShowObjectStruct
{
	#region Private Variables
	
	[SerializeField]
	GameObject m_objectToApply = null;
	
	#endregion
	
	#region Public Properties
	
	public GameObject ObjectToApply 
	{
		get 
		{
			return m_objectToApply;
		} 
		set 
		{ 
			if(m_objectToApply == value)
				return;
			m_objectToApply = value;
		} 
	}
	
	#endregion
}

[System.Serializable]
public abstract class GenericShowObjectStructWithAction<ActionType> : GenericShowObjectStruct where ActionType: GenericStateAction
{
	#region Private Variables
	
	[SerializeField]
	List<ActionType> m_stateActionsList = new List<ActionType>();
	
	#endregion
	
	#region Public Properties
	
	public List<ActionType> StateActionsList 
	{
		get 
		{
			if(m_stateActionsList == null)
				m_stateActionsList = new List<ActionType>();
			return m_stateActionsList;
		} 
		set 
		{
			m_stateActionsList = value;
		} 
	}
	
	#endregion
}

public enum ShowObjectActionEnum {Show, Hide, ShowFinish, HideFinish }

[System.Serializable]
public class GenericStateAction
{
	#region Private Variables
	
	[SerializeField]
	ShowObjectActionEnum m_action =  ShowObjectActionEnum.Show;
	[SerializeField]
	float m_delayToCallAction = 0;
	
	#endregion
	
	#region Public Properties
	
	public ShowObjectActionEnum Action {get {return m_action;} set {m_action = value;} }
	public float DelayToCallAction {get {return m_delayToCallAction;} set {m_delayToCallAction = value;} }
	
	#endregion
}

[System.Serializable]
public class GenericStateActionWithConditions<ConditionType> : GenericStateAction
{
	#region Private Variables
	
	[SerializeField]
	ConditionType m_stateToMakeAction = default(ConditionType);
	
	#endregion
	
	#region Public Properties
	
	public ConditionType StateToMakeAction {get {return m_stateToMakeAction;} set {m_stateToMakeAction = value;} }
	
	#endregion
}

#endregion

#region ShowHideButton Helper Structs

[System.Serializable]
public class ObjectShowActionStruct
{
	#region Private Variables
	
	[SerializeField]
	GameObject m_objectToApply = null;
	[SerializeField]
	float m_delayToCallAction = 0;
	[SerializeField]
	ShowObjectActionEnum m_action = ShowObjectActionEnum.Show;
	
	#endregion
	
	#region Public Properties
	
	public GameObject ObjectToApply {get {return m_objectToApply;} set { m_objectToApply = value;} }
	public float DelayToCallAction {get {return m_delayToCallAction;} set {m_delayToCallAction = value;} }
	public ShowObjectActionEnum Action {get {return m_action;} set {m_action = value;} }
	
	#endregion
}

#endregion