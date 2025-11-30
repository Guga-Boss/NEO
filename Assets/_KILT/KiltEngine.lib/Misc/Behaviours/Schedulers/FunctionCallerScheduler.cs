using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FunctionCallerScheduler : TimeScheduler {

	#region Private Variables

	[SerializeField, HideInInspector]
	System.Delegate m_functionToCall = null;
	/*[SerializeField]
	Delegates.FunctionPointerOne m_functionToCallOne = null;
	[SerializeField]
	Delegates.FunctionPointerTwo m_functionToCallTwo = null;
	[SerializeField]
	Delegates.FunctionPointerThree m_functionToCallThree = null;*/
	//System.Delegate m_functionWithParameterToCall = null;

	/*[SerializeField, HideInInspector]
	object m_parameterOne = default(object);
	[SerializeField, HideInInspector]
	object m_parameterTwo = default(object);
	[SerializeField, HideInInspector]
	object m_parameterThree = default(object);*/
	[SerializeField, HideInInspector]
	List<object> m_parameters = new List<object>();

	[SerializeField]
	bool m_clearFunctionWhenFinish = false;
	#endregion


	#region Public Properties

	public System.Delegate FunctionToCall {get {return m_functionToCall;} set {m_functionToCall = value;}}
	/*public Delegates.FunctionPointerOne FunctionToCallOne {get {return m_functionToCallOne;} set {m_functionToCallOne = value;}}
	public Delegates.FunctionPointerTwo FunctionToCallTwo {get {return m_functionToCallTwo;} set {m_functionToCallTwo = value;}}
	public Delegates.FunctionPointerThree FunctionToCallThree {get {return m_functionToCallThree;} set {m_functionToCallThree = value;}}

	public object ParameterOne {get {return m_parameterOne;} set {m_parameterOne = value;}}
	public object ParameterTwo {get {return m_parameterTwo;} set {m_parameterTwo = value;}}
	public object ParameterThree {get {return m_parameterThree;} set {m_parameterThree = value;}}*/

	//public System.Delegate FunctionWithParameterToCall {get {return m_functionWithParameterToCall;} set {m_functionWithParameterToCall = value;}}
	public List<object> Parameters 
	{
		get 
		{
			if(m_parameters == null)
				m_parameters = new List<object>();
			return m_parameters;
		} 
		set 
		{
			if(m_parameters == value)
				return;
			m_parameters = value;
		}
	}

	public bool ClearFunctionWhenFinish {get {return m_clearFunctionWhenFinish;} set {m_clearFunctionWhenFinish = value;}}

	#endregion

	#region Constructor

	public FunctionCallerScheduler()
	{
		MaxTime = 2f;
		TimerStyle = TimerStyle.OneTimeOnly;
		Loop = false;
	}

	#endregion

	#region Overridden Functions

	protected override void OnPingFinish()
	{
		if(Owner != null)
			CallFunction();
	}

	#endregion

	#region Helper Functions

	public void CallFunction()
	{
		try
		{
			System.Delegate v_tempFunctionPointer = FunctionToCall;
			object[] v_params = Parameters.ToArray();
			if(ClearFunctionWhenFinish)
			{
				FunctionToCall = null;
				Parameters.Clear();
			}
			if(v_tempFunctionPointer != null)
			{
				if(Parameters.Count == 0)
					v_tempFunctionPointer.DynamicInvoke(null);
				else
					v_tempFunctionPointer.DynamicInvoke(v_params);
			}
		}
		catch
		{ 
			FunctionToCall = null;
			Parameters.Clear();
		}
	}

	#endregion
}
