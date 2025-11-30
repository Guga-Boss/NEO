using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public enum PanelStateEnum {Opening, Closing, Opened, Closed }
public enum CloseSpecialActionEnum {Nothing, Deactivate, DeactivateCollider }

//Send Message to Self Calling OnPanelStateChanged(PanelStateEnum p_panelState)

public class ScheduledContainer : MonoBehaviour 
{	
	#region Events

	public event Delegates.EventHandler OnOpening;
	public event Delegates.EventHandler OnOpened;
	public event Delegates.EventHandler OnClosing;
	public event Delegates.EventHandler OnClosed;

	#endregion

	#region Private Variables
	
	[SerializeField]
	List<ShowObjectStruct> m_objectsToEnableOrDisable = new List<ShowObjectStruct>();

	[SerializeField, PropertyBackingFieldAttribute]
	PanelStateEnum _panelState = PanelStateEnum.Closed;
	PanelStateEnum m_panelState = PanelStateEnum.Closed;
	[SerializeField]
	CloseSpecialActionEnum m_closeSpecialAction = CloseSpecialActionEnum.Deactivate;
	[SerializeField]
	RestartEnum m_restartOption = RestartEnum.DontRestartIfRunning;
	[SerializeField]
	bool m_ignoreTimeScale = true;
	[SerializeField]
	bool m_enableScheduler = true;
	
	TimeScheduler _scheduler = null;

	#endregion

	#region UnityEvents Callback

	public UnityEvent OnOpeningCallBack;
	public UnityEvent OnOpenedCallBack;
	public UnityEvent OnClosingCallBack;
	public UnityEvent OnClosedCallBack;
	
	#endregion

	#region Public Properties

	public List<ShowObjectStruct> ObjectsToEnableOrDisable {get {return m_objectsToEnableOrDisable;} set {m_objectsToEnableOrDisable = value;}}
	
	public PanelStateEnum PanelState
	{
		get 
		{
			if(!_awaked)
				m_panelState = _panelState;
			return m_panelState; 
		}
		protected set 
		{
			if(_panelState == value && m_panelState == value)
				return;
			_panelState = value;
			m_panelState = _panelState;
			if(Application.isPlaying)
				ForceSetPanelStateValue(_panelState, false);
		}
	}

	public RestartEnum RestartOption {get{return m_restartOption;} set{m_restartOption = value;}}

	public bool IgnoreTimeScale {get{return m_ignoreTimeScale;} set{m_ignoreTimeScale = value;}}

	public bool EnableScheduler {get{return m_enableScheduler;} set{m_enableScheduler = value;}}

	public CloseSpecialActionEnum CloseSpecialAction 
	{
		get{return m_closeSpecialAction;} 
		set
		{
			if(m_closeSpecialAction == value)
				return;
			m_closeSpecialAction = value;
		}
	}

	public TimeScheduler Scheduler 
	{
		get
		{
			if(_scheduler == null)
				_scheduler = this.GetComponent<TimeScheduler>();
			return _scheduler;
		}
	}
	
	#endregion

	#region Unity Functions
	
	protected virtual void OnEnable()
	{
		if(!_awaked)
		{
			CorrectPanelState(false);
			ForceSetPanelStateValue(PanelState, true, true);
		}
		_awaked = false;
	}
	
	protected virtual void OnDisable()
	{
		if(PanelState != PanelStateEnum.Closed && CloseSpecialAction == CloseSpecialActionEnum.Deactivate && (!gameObject.activeSelf || !enabled))
			ForceSetPanelStateValue(PanelStateEnum.Closed, true, true);
	}

	bool _awaked = false;
	protected virtual void Awake () 
	{
		if(Scheduler != null)
		{
			Scheduler.OnCycleFinishedExecution -= OnCycleFinishedExecution;
			Scheduler.OnCycleFinishedExecution += OnCycleFinishedExecution;
			Scheduler.OnCycleStartedExecution -= OnCycleStartedExecution;
			Scheduler.OnCycleStartedExecution += OnCycleStartedExecution;
		}

		if(!_awaked)
		{
			_awaked = true;
			CorrectPanelState(false);
			ForceSetPanelStateValue(PanelState, true);
		}
	}

	protected virtual void LateUpdate()
	{
		CorrectPanelState(true);
		ControlSpecialAction();
		CorrectSchedulerEnableOnOpeningOrClosing();
		CheckNeedActivate();
	}
	
	#endregion

	#region Events Receiver
	
	protected virtual void OnCycleStartedExecution(CycleEventArgs p_args)
	{
		if(EnableScheduler)
		{
			if(p_args.IsPing)
			{
				ForceSetPanelStateValue(PanelStateEnum.Opening, false);
			}
			else
			{
				ForceSetPanelStateValue(PanelStateEnum.Closing, false);
			}
		}
	}
	
	protected virtual void OnCycleFinishedExecution(CycleEventArgs p_args)
	{
		if(EnableScheduler)
		{
			if(p_args.IsPing)
			{
				ForceSetPanelStateValue(PanelStateEnum.Opened, false);
			}
			else
			{
				ForceSetPanelStateValue(PanelStateEnum.Closed, false);
			}
		}
	}
	
	#endregion

	#region Helper Functions

	private void CorrectSchedulerEnableOnOpeningOrClosing()
	{
		if(EnableScheduler && Scheduler != null && !Scheduler.enabled && (PanelState == PanelStateEnum.Opening || PanelState == PanelStateEnum.Closing))
		{
			Scheduler.enabled = true;
		}
	}

	private void CorrectPanelState(bool p_callLogic)
	{
		if(_panelState != m_panelState)
		{
			if(p_callLogic)
				ForceSetPanelStateValue(_panelState, true);
			else
				m_panelState = _panelState;
		}
	}

	#region PanelState Functions

	public void ForceSetPanelStateValue(PanelStateEnum p_value)
	{
		ForceSetPanelStateValue(p_value, true);
	}
	
	public void ForceSetPanelStateValue(PanelStateEnum p_value, bool p_updateEffect, bool p_underDisableOrEnable = false)
	{
		if(Scheduler == null || !EnableScheduler)
		{
			if(p_value == PanelStateEnum.Opening)
				p_value = PanelStateEnum.Opened;
			if(p_value == PanelStateEnum.Closing)
				p_value = PanelStateEnum.Closed;
		}
		bool p_changed = CheckIfStateWillChange(p_value);
		m_panelState = p_value;
		_panelState = m_panelState;

		if(p_changed)
		{
			if(CloseSpecialAction != CloseSpecialActionEnum.Deactivate || m_panelState != PanelStateEnum.Closed)
			{
		   		if(!p_underDisableOrEnable)
					SetActiveAndEnable(true,true);
				if(Application.isPlaying)
					gameObject.SendMessage("OnPanelStateChanged", m_panelState, SendMessageOptions.DontRequireReceiver);
			}
		}
		if(Application.isPlaying && (CloseSpecialAction != CloseSpecialActionEnum.Deactivate || m_panelState != PanelStateEnum.Closed))
			gameObject.SendMessage("CallEventsInternal", m_panelState, SendMessageOptions.DontRequireReceiver);
		else
			CallEventsInternal(m_panelState);

		if(p_updateEffect)
			UpdateEffect();

		//Used to Prevent Bugs while activating
		if(p_underDisableOrEnable)
		{
			if(Application.isPlaying && (CloseSpecialAction != CloseSpecialActionEnum.Deactivate || m_panelState != PanelStateEnum.Closed))
			{
				gameObject.SendMessage("CallControlSpecialAction", SendMessageOptions.DontRequireReceiver);
				gameObject.SendMessage("UpdateObjectsToShowOrHideByState", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				CallControlSpecialAction();
				UpdateObjectsToShowOrHideByState();
			}
		}
		else
		{
			CallControlSpecialAction();
			UpdateObjectsToShowOrHideByState();
			if(!gameObject.activeSelf)
			{
				if(Application.isPlaying)
					GlobalScheduler.CallFunction(this.gameObject, new Delegates.FunctionPointer(CheckNeedActivate), 0.01f, true);
			}
		}
		if(!Application.isPlaying)
		{
			ControlSpecialAction();
			CheckNeedActivate();
		}
	}

	protected virtual void CallEventsInternal(PanelStateEnum p_panelState)
	{
		if(p_panelState == PanelStateEnum.Opened)
		{
			if(OnOpened != null)
				OnOpened();
			if(OnOpenedCallBack != null)
				OnOpenedCallBack.Invoke();
		}
		else if (p_panelState == PanelStateEnum.Closed)
		{
			if(OnClosed != null)
				OnClosed();
			if(OnClosedCallBack != null)
				OnClosedCallBack.Invoke();
		}
		else if(p_panelState == PanelStateEnum.Opening)
		{
			if(OnOpening != null)
				OnOpening();
			if(OnOpeningCallBack != null)
				OnOpeningCallBack.Invoke();
		}
		else if(p_panelState == PanelStateEnum.Closing)
		{
			if(OnClosing != null)
				OnClosing();
			if(OnClosingCallBack != null)
				OnClosingCallBack.Invoke();
		}
	}

	#region Internal PanelState Functions

	protected void SetActiveAndEnable(bool p_active = true, bool p_enabled = true)
	{
		if(gameObject.activeSelf != p_active)
			gameObject.SetActive(p_active);
		if(enabled != p_enabled)
			enabled = p_enabled;
	}

	protected bool CheckIfStateWillChange(PanelStateEnum p_value)
	{

		bool v_return = m_panelState != p_value? true : false;
		/*if(RestartOption == RestartEnum.DontRestartIfRunning)
		{
			if(m_panelState == PanelStateEnum.Closing || m_panelState == PanelStateEnum.Opening)
				v_return = false;
		}*/
		if( (m_panelState == PanelStateEnum.Closed && p_value == PanelStateEnum.Closing)
		   || (m_panelState == PanelStateEnum.Opened && p_value == PanelStateEnum.Opening))
		{
			v_return = false;
		}

		return v_return;
	}

	protected void UpdateObjectsToShowOrHideByState()
	{
		foreach(ShowObjectStruct v_struct in ObjectsToEnableOrDisable)
		{
			if(v_struct != null && v_struct.ObjectToApply != null)
			{
				if(v_struct.StateActionsList != null)
				{
					foreach (StateAction v_stateAction in v_struct.StateActionsList)
					{
						if(v_stateAction != null && v_stateAction.StateToMakeAction == PanelState)
						{
							KiltUtils.SetContainerVisibility(v_struct.ObjectToApply, v_stateAction.Action, v_stateAction.DelayToCallAction);
							break;
						}
					}
				}
			}
		}
	}

	protected void CheckNeedActivate()
	{
		if(_needActivate != null && _needActivate.Key)
		{
			_needActivate.Key = false;
			if(gameObject.activeSelf != _needActivate.Value)
				gameObject.SetActive(_needActivate.Value);
		}
	}

	bool _canCallControlSpecialAction = false;
	protected void CallControlSpecialAction()
	{
		CallControlSpecialAction(true);
	}

	protected void CallControlSpecialAction(bool p_forceEnable)
	{
		_canCallControlSpecialAction = true;
		if((!gameObject.activeSelf || !enabled) && p_forceEnable)
		{
			ControlSpecialAction();
		}
	}

	protected void ControlSpecialAction()
	{
		ControlSpecialAction(false);
	}

	AOTKeyValuePair<bool, bool> _needActivate = new AOTKeyValuePair<bool, bool>();
	protected void ControlSpecialAction(bool p_forceExecution)
	{
		if(_canCallControlSpecialAction || p_forceExecution)
		{
			try
			{
				_canCallControlSpecialAction = false;
				if(!enabled)
					enabled = true;
				if (m_panelState == PanelStateEnum.Closed)
				{
					if(CloseSpecialAction == CloseSpecialActionEnum.Deactivate)
					{
						if(gameObject.activeSelf)
						{
							_needActivate.Key = true;
							_needActivate.Value = false;
							//gameObject.SetActive(false);
						}
					}
					else
					{
						if(!gameObject.activeSelf)
						{
							_needActivate.Key = true;
							_needActivate.Value = true;
							//gameObject.SetActive(true);
						}
						if(CloseSpecialAction == CloseSpecialActionEnum.DeactivateCollider)
							SetColliderVisibility(false);
						else
							SetColliderVisibility(true);
					}
				}
				else
				{
					if(!gameObject.activeSelf)
					{
						_needActivate.Key = true;
						_needActivate.Value = true;
						//gameObject.SetActive(true);
					}
					if(m_panelState == PanelStateEnum.Opened)
						SetColliderVisibility(true);
				}
			}
			catch 
			{ 
				CallControlSpecialAction(false);
			}
		}
	}

	protected void SetColliderVisibility(bool p_visibility)
	{
		Collider2D[] v_collidersInSelf = GetComponents<Collider2D>();
		Collider2D[] v_colliders = GetComponentsInChildren<Collider2D>();

		foreach(Collider2D v_collider in v_collidersInSelf)
		{
			if(v_collider != null)
			{
				v_collider.enabled = p_visibility;
			}
		}

		foreach(Collider2D v_collider in v_colliders)
		{
			if(v_collider != null)
			{
				v_collider.enabled = p_visibility;
			}
		}
	}

	protected void UpdateEffect()
	{
		//Active Schedulers
		if(Scheduler != null && EnableScheduler)
		{
			Scheduler.TimerStyle = TimerStyle.OneTimeOnly;
			Scheduler.Loop = false;
			
			bool v_visibility = true;
			bool v_forceFinish = true;
			if(PanelState == PanelStateEnum.Opened)
			{
				v_visibility = true;
				v_forceFinish = true;
			}
			else if(PanelState == PanelStateEnum.Opening)
			{
				v_visibility = true;
				v_forceFinish = false;
			}
			else if(PanelState == PanelStateEnum.Closed)
			{
				v_visibility = false;
				v_forceFinish = true;
			}
			else if(PanelState == PanelStateEnum.Closing)
			{
				v_visibility = false;
				v_forceFinish = false;
			}

			Scheduler.IgnoreTimeScale = IgnoreTimeScale;
			if(v_forceFinish)
			{
				Scheduler.ForceFinish(v_visibility, true);
			}
			else
			{
				Scheduler.StartTimer(v_visibility, true, RestartOption);
			}
		}
	}

	#endregion

	#endregion

	#region Visibility Functions
	
	public void Show()
	{
		Show(false);
	}
	
	public void Show(bool p_forceFinish)
	{
		_awaked = true;
		if(!Application.isPlaying)
			p_forceFinish = true;
		if(PanelState != PanelStateEnum.Opened)
		{
			if(p_forceFinish)
				ForceSetPanelStateValue(PanelStateEnum.Opened, true);
			else if(RestartOption != RestartEnum.DontRestartIfRunning || !Application.isPlaying || 
			        (PanelState != PanelStateEnum.Opening && PanelState != PanelStateEnum.Closing))
				ForceSetPanelStateValue(PanelStateEnum.Opening, true);
		}
	}

	public void Hide()
	{
		Hide(false);
	}

	public void Hide(bool p_forceFinish)
	{
		_awaked = false;
		if(!Application.isPlaying)
			p_forceFinish = true;
		if(PanelState != PanelStateEnum.Closed)
		{
			if(p_forceFinish)
				ForceSetPanelStateValue(PanelStateEnum.Closed, true);
			else if(RestartOption != RestartEnum.DontRestartIfRunning || !Application.isPlaying 
			        || (PanelState != PanelStateEnum.Opening && PanelState != PanelStateEnum.Closing))
				ForceSetPanelStateValue(PanelStateEnum.Closing, true);
		}
	}

	#endregion

	#endregion
}

#region Helper Classes

[System.Serializable]
public class ShowObjectStruct : GenericShowObjectStructWithAction<StateAction>
{
}

[System.Serializable]
public class StateAction : GenericStateActionWithConditions<PanelStateEnum>
{
}

#endregion

