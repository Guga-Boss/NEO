using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Kilt.Core;

public enum ButtonTrigger {OnClick, OnMouseOver, OnMouseOut, OnPress, OnRelease, OnDoubleClick, None, OnAwake, OnEnable, OnStart, OnDisable, OnApplicationQuit, OnLevelWasLoaded, OnGainFocus, OnLoseFocus }
public enum PauseActionEnum {Pause, Resume, None, ForceResume}

[System.Flags]
public enum UnityTriggerEnum {OnAwake=1, OnEnable=2, OnStart=4, OnDisable=8, OnApplicationQuit=16, OnLevelWasLoaded=32, OnAppGainFocus=64, OnAppLoseFocus=128, OnDestroy = 256, OnFirstUpdate=512, OnUpdate=1024, OnFirstFixedUpdate=2048, OnFixedUpdate=4096, OnFirstLateUpdate=8192, OnLateUpdate=16384}
[System.Flags]
public enum UITriggerEnum {OnPointerEnter=1, OnPointerExit=2, OnPointerDown=4, OnPointerUp=8, OnPointerClick=16, OnDrag=32, OnDrop=64, OnScroll=128, OnUpdateSelected=256, OnSelect=512, OnDeselect=1024, OnMove=2048, OnInitializePotentialDrag=4096, OnBeginDrag=8192, OnEndDrag=16384, OnSubmit=32768, OnCancel=65536}
[System.Flags]
public enum KiltCamTriggerEnum {OnClick=1, OnMouseOver=2, OnMouseOut=4, OnPress=8, OnRelease=16, OnDoubleClick=32}

public class InteractionBehaviour : MonoBehaviour , UnityEngine.EventSystems.IEventSystemHandler
{
	#region Private Variables

	[SerializeField, PropertyBackingFieldAttribute]
	bool m_useOldEventSystem = false; //Used in DependentField of deprecated Trigger Property
	[SerializeField, PropertyBackingFieldAttribute(typeof(DependentFieldAttribute), false, typeof(ButtonTrigger) , "m_useOldEventSystem", DependenceOptionEnum.NormalDependence, CheckerFalseDrawOptionEnum.ReadOnlyProperty)]
	ButtonTrigger m_trigger = ButtonTrigger.None;
	[SerializeField, PropertyBackingFieldAttribute(typeof(MaskEnumAttribute))]
	UnityTriggerEnum m_unityTrigger = (UnityTriggerEnum)0;
	[SerializeField, PropertyBackingFieldAttribute("UITrigger", typeof(MaskEnumAttribute))]
	UITriggerEnum m_uiTrigger = (UITriggerEnum)0;
	[SerializeField, PropertyBackingFieldAttribute(typeof(MaskEnumAttribute))]
	KiltCamTriggerEnum m_kiltCamTrigger = (KiltCamTriggerEnum)0;
	[SerializeField]
	List<AlarmTrigger> m_alarmTriggers = new List<AlarmTrigger>();
	[SerializeField]
	List<ComboKeys> m_comboKeysTrigger = new List<ComboKeys>();

	[SerializeField]
	KeyCode m_keyToLauch = KeyCode.None;
	[SerializeField]
	PauseActionEnum m_pauseAction = PauseActionEnum.None;
	[SerializeField]
	float m_timeBeetweenSends = 0.8f;
	[SerializeField]
	bool m_forceKinematicBeforePressTouch = true;
	[SerializeField]
	bool m_canCallWhenLoadingScene = false;

	bool _started = false;
	bool _highlighted = false;
	
	bool _oldKinematic = true;

	float _currentTimeBeetweenSends = 0;


	#endregion

	#region Callbacks
	
	public UnityEvent OnSendCallback;
	
	#endregion

	#region Protected Properties

	protected bool Started
	{
		get {return _started;} 
		set
		{
			if(_started == value)
				return;
			_started = value;
		}
	}

	protected bool Highlighted
	{
		get {return _highlighted;} 
		set
		{
			if(_highlighted == value)
				return;
			_highlighted = value;
		}
	}

	protected bool OldKinematic
	{
		get {return _oldKinematic;} 
		set
		{
			if(_oldKinematic == value)
				return;
			_oldKinematic = value;
		}
	}

	#endregion

	#region Public Properties

	public PauseActionEnum PauseAction
	{ 
		get {return m_pauseAction;} 
		set {m_pauseAction = value;}
	}

	public KeyCode KeyToLauch
	{
		get {return m_keyToLauch;} 
		set
		{
			if(m_keyToLauch == value)
				return;
			m_keyToLauch = value;
		}
	}

	public bool UseOldEventSystem 
	{
		get 
		{
			return m_useOldEventSystem;
		} 
		set
		{
			if(m_useOldEventSystem == value)
				return;
			m_useOldEventSystem = value;
			if(!m_useOldEventSystem)
				m_trigger = ButtonTrigger.None;
		}
	}

	public ButtonTrigger Trigger 
	{
		get 
		{
			m_useOldEventSystem = !(m_trigger == ButtonTrigger.None);
			return m_trigger;
		} 
		set
		{
			if(m_trigger == value)
				return;
			m_trigger = value;
			m_useOldEventSystem = !(m_trigger == ButtonTrigger.None);
		}
	}

	public UnityTriggerEnum UnityTrigger 
	{
		get {return m_unityTrigger;} 
		set
		{
			if(m_unityTrigger == value)
				return;
			m_unityTrigger = value;
		}
	}

	public UITriggerEnum UITrigger 
	{
		get {return m_uiTrigger;} 
		set
		{
			if(m_uiTrigger == value)
				return;
			m_uiTrigger = value;
			if(Application.isPlaying)
				ApplyUIEvents();
		}
	}

	public KiltCamTriggerEnum KiltCamTrigger
	{
		get {return m_kiltCamTrigger;} 
		set
		{
			if(m_kiltCamTrigger == value)
				return;
			m_kiltCamTrigger = value;
		}
	}

	public List<AlarmTrigger> AlarmTriggers 
	{
		get 
		{
			if(m_alarmTriggers == null)
				m_alarmTriggers = new List<AlarmTrigger>();
			return m_alarmTriggers;
		} 
		set
		{
			if(m_alarmTriggers == value)
				return;
			m_alarmTriggers = value;
			if(Application.isPlaying)
				StartAlarmTriggers();
		}
	}

	public List<ComboKeys> ComboKeysTriggers 
	{
		get 
		{
			if(m_comboKeysTrigger == null)
				m_comboKeysTrigger = new List<ComboKeys>();
			return m_comboKeysTrigger;
		} 
		set
		{
			if(m_comboKeysTrigger == value)
				return;
			m_comboKeysTrigger = value;
		}
	}

	public float TimeBeetweenSends 
	{
		get {return m_timeBeetweenSends;} 
		set
		{
			if(m_timeBeetweenSends == value)
				return;
			m_timeBeetweenSends = value;
		}
	}

	public bool ForceKinematicBeforePressTouch 
	{
		get {return m_forceKinematicBeforePressTouch;} 
		set
		{
			if(m_forceKinematicBeforePressTouch == value)
				return;
			m_forceKinematicBeforePressTouch = value;
		}
	}

	public bool CanCallWhenLoadingScene
	{
		get {return m_canCallWhenLoadingScene;} 
		set
		{
			if(m_canCallWhenLoadingScene == value)
				return;
			m_canCallWhenLoadingScene = value;
		}
	}

	#endregion

	#region Unity Functions

	protected virtual void Awake()
	{
		ApplyUIEvents();
		if(UnityTrigger.ContainsFlag(UnityTriggerEnum.OnAwake) || Trigger == ButtonTrigger.OnAwake)
			CallSend();
	}

	protected virtual void Start () 
	{
		Started = true;
		if(UnityTrigger.ContainsFlag(UnityTriggerEnum.OnStart) || Trigger == ButtonTrigger.OnStart)
			CallSend();
	}
	
	protected virtual void OnEnable () 
	{
		StartAlarmTriggers();
		if (Started && Highlighted) 
			OnHover(KiltUICamera.IsHighlighted(gameObject)); 
		if(UnityTrigger.ContainsFlag(UnityTriggerEnum.OnEnable) || Trigger == ButtonTrigger.OnEnable)
			CallSend();
	}

	protected virtual void OnDisable () 
	{ 
		if(UnityTrigger.ContainsFlag(UnityTriggerEnum.OnDisable) || Trigger == ButtonTrigger.OnDisable)
			CallSend();
	}

	protected virtual void OnDestroy () 
	{
		//Take Care Calling Events OnDestroy!!
		if(UnityTrigger.ContainsFlag(UnityTriggerEnum.OnDestroy))
			CallSend();
	}

	protected virtual void FirstUpdate()
	{
		if(UnityTrigger.ContainsFlag(UnityTriggerEnum.OnFirstUpdate))
			CallSend();
	}
	
	protected virtual void FirstFixedUpdate()
	{
		if(UnityTrigger.ContainsFlag(UnityTriggerEnum.OnFirstFixedUpdate))
			CallSend();
	}
	
	protected virtual void FirstLateUpdate()
	{
		if(UnityTrigger.ContainsFlag(UnityTriggerEnum.OnFirstLateUpdate))
			CallSend();
	}

	bool _firstUpdate = true;
	protected virtual void Update()
	{
		if(_firstUpdate)
		{
			_firstUpdate = false;
			FirstUpdate();
		}
		if(UnityTrigger.ContainsFlag(UnityTriggerEnum.OnUpdate))
			CallSend();
		UpdateTimeBetweenSends();
		if(KeyToLauch != KeyCode.None && Input.GetKeyDown(KeyToLauch))
			CallSend(false);
		ApplyAlarmTriggersTime();
		CheckComboKeys();
	}

	bool _firstFixedUpdate = true;
	protected virtual void FixedUpdate()
	{
		if(_firstFixedUpdate)
		{
			_firstFixedUpdate = false;
			FirstFixedUpdate();
		}
		if(UnityTrigger.ContainsFlag(UnityTriggerEnum.OnFixedUpdate))
			CallSend();
	}

	bool _firstLateUpdate = true;
	protected virtual void LateUpdate()
	{
		if(_firstLateUpdate)
		{
			_firstLateUpdate = false;
			FirstLateUpdate();
		}
		if(UnityTrigger.ContainsFlag(UnityTriggerEnum.OnLateUpdate))
			CallSend();
	}

	protected virtual void OnApplicationQuit() 
	{
		if(UnityTrigger.ContainsFlag(UnityTriggerEnum.OnApplicationQuit) || Trigger == ButtonTrigger.OnApplicationQuit) 
			CallSend();
	}

	protected virtual void OnLevelWasLoaded(int p_index) 
	{
		if(UnityTrigger.ContainsFlag(UnityTriggerEnum.OnLevelWasLoaded) || Trigger == ButtonTrigger.OnLevelWasLoaded)
			CallSend();
	}

	protected virtual void OnApplicationPause(bool p_paused) 
	{
		if((UnityTrigger.ContainsFlag(UnityTriggerEnum.OnAppGainFocus) || Trigger == ButtonTrigger.OnGainFocus) && !p_paused)
			CallSend();
		else if((UnityTrigger.ContainsFlag(UnityTriggerEnum.OnAppLoseFocus) || Trigger == ButtonTrigger.OnLoseFocus) && p_paused)
			CallSend();
	}

	#endregion

	#region Event Receivers

	protected virtual void OnHover (bool isOver)
	{
		if (enabled && !CameraFade.InstanceExists())
		{
			if (((isOver && (KiltCamTrigger.ContainsFlag(KiltCamTriggerEnum.OnMouseOver) || Trigger == ButtonTrigger.OnMouseOver)) ||
			     (!isOver && (KiltCamTrigger.ContainsFlag(KiltCamTriggerEnum.OnMouseOut) || Trigger == ButtonTrigger.OnMouseOut)))) 
				CheckSend();
			Highlighted = isOver;
		}
	}
	
	protected virtual void OnPress (bool isPressed)
	{
		if (enabled && !CameraFade.InstanceExists())
		{
			UpdateOldPos();
			UpdateKinematicOnPress(isPressed);
			if (((isPressed && (KiltCamTrigger.ContainsFlag(KiltCamTriggerEnum.OnPress) || Trigger == ButtonTrigger.OnPress)) ||
			     (!isPressed && (KiltCamTrigger.ContainsFlag(KiltCamTriggerEnum.OnRelease) || Trigger == ButtonTrigger.OnRelease)))) 
				CheckSend();

		}
	}
	
	protected virtual void OnClick () 
	{ 
		if (enabled && !CameraFade.InstanceExists())
		{
			if(KiltCamTrigger.ContainsFlag(KiltCamTriggerEnum.OnClick) || Trigger == ButtonTrigger.OnClick) 
				CheckSend(); 
		}
	}
	
	protected virtual void OnDoubleClick () 
	{ 
		if (enabled && !CameraFade.InstanceExists())
		{
			if (KiltCamTrigger.ContainsFlag(KiltCamTriggerEnum.OnDoubleClick) || Trigger == ButtonTrigger.OnDoubleClick) 
				CheckSend();
		}
	}

	#endregion

	#region ComboKeys Triggers Helper Functions

	protected virtual void CheckComboKeys()
	{
		foreach(ComboKeys v_comboKeys in ComboKeysTriggers)
		{
			if(v_comboKeys.CheckTrigger())
			{
				CallSend(false);
			}
		}
	}

	#endregion

	#region Alarm Triggers Helper Functions

	protected virtual void StartAlarmTriggers()
	{
		foreach(AlarmTrigger v_alarmTriggers in AlarmTriggers)
		{
			v_alarmTriggers.StartAlarm();
		}
	}

	protected virtual void ApplyAlarmTriggersTime()
	{
		foreach(AlarmTrigger v_alarmTriggers in AlarmTriggers)
		{
			if(v_alarmTriggers.ApplyTime())
			{
				CallSend();
			}
		}
	}

	#endregion

	#region UI Helper Functions
	
	protected virtual void ApplyUIEvents()
	{
		ApplyButtonUIEvents();
		ApplyTriggerUIEvents();
	}

	protected virtual void ApplyButtonUIEvents()
	{
		Button v_button = GetComponent<Button>();
		if(v_button != null)
		{
			RemoveListener(v_button.onClick, FullSend);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnPointerClick))
				AddListener(v_button.onClick, FullSend);
		}
	}

	protected virtual void ApplyTriggerUIEvents()
	{
		EventTrigger v_eventTrigger = GetComponent<EventTrigger>();
		if(v_eventTrigger != null)
		{
			//Remove Listeners Before Add
			foreach(EventTrigger.Entry v_entry in v_eventTrigger.triggers)
			{
				if(v_entry != null && v_entry.callback != null)
					v_entry.callback.RemoveListener(CallFullSendWithEventData);
			}

			if(UITrigger.ContainsFlag(UITriggerEnum.OnPointerEnter))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.PointerEnter);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnPointerExit))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.PointerExit);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnPointerDown))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.PointerDown);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnPointerUp))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.PointerUp);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnPointerClick))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.PointerClick);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnDrag))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.Drag);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnDrop))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.Drop);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnScroll))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.Scroll);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnUpdateSelected))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.UpdateSelected);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnSelect))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.Select);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnDeselect))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.Deselect);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnMove))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.Move);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnInitializePotentialDrag))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.InitializePotentialDrag);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnBeginDrag))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.BeginDrag);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnEndDrag))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.EndDrag);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnSubmit))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.Submit);
			if(UITrigger.ContainsFlag(UITriggerEnum.OnCancel))
				AddReplacingEntry(v_eventTrigger, EventTriggerType.Cancel);
		}
	}

	private void AddReplacingEntry(EventTrigger p_eventTrigger, EventTriggerType p_type)
	{
		if(p_eventTrigger != null)
		{
			EventTrigger.Entry v_entryToUpdate = null;
			foreach(EventTrigger.Entry v_entry in p_eventTrigger.triggers)
			{
				if(v_entry.eventID == p_type)
				{
					v_entryToUpdate = v_entry;
					break;
				}
			}
			if(v_entryToUpdate == null)
			{
				v_entryToUpdate = CreateEventEntry(p_type);
				p_eventTrigger.triggers.Add(v_entryToUpdate);
			}
			else
				TryAddEventListener(v_entryToUpdate);
		}
	}

	private EventTrigger.Entry CreateEventEntry(EventTriggerType p_type)
	{
		EventTrigger.Entry v_entry = new EventTrigger.Entry();
		v_entry.eventID = p_type;
		TryAddEventListener(v_entry);
		return v_entry;
	}

	protected virtual void TryAddEventListener(EventTrigger.Entry p_entry)
	{
		if(p_entry != null)
		{
			EventTrigger.TriggerEvent v_trigger = p_entry.callback != null? p_entry.callback : new EventTrigger.TriggerEvent();
			v_trigger.RemoveListener(CallFullSendWithEventData);
			v_trigger.AddListener(CallFullSendWithEventData);
		}
	}

	private void CallFullSendWithEventData(BaseEventData p_data)
	{
		FullSend();
	}

	protected virtual void AddListener(UnityEvent p_event, UnityAction p_action)
	{
		if(p_event != null && p_action != null)
		{
			p_event.RemoveListener(p_action);
			p_event.AddListener(p_action);
		}
	}

	protected virtual void RemoveListener(UnityEvent p_event, UnityAction p_action)
	{
		if(p_event != null && p_action != null)
			p_event.RemoveListener(p_action);
	}

	#endregion

	#region Helper Functions

	protected virtual void UpdateTimeBetweenSends()
	{
		if(_currentTimeBeetweenSends > 0)
		{
			_currentTimeBeetweenSends = Mathf.Max(0f, _currentTimeBeetweenSends - GetDeltaTime()) ;
		}
	}

	protected virtual float GetDeltaTime()
	{
		return Time.unscaledDeltaTime; //IgnoreTimeScale? Time.unscaledDeltaTime : Time.deltaTime;
	}

	public bool IsAnyColliderActive()
	{
		List<Collider2D> v_colliders = new List<Collider2D>(GetComponents<Collider2D>());
		v_colliders.MergeList(new List<Collider2D>(GetComponentsInChildren<Collider2D>()));
		foreach(Collider2D v_collider in v_colliders)
		{
			if(v_collider != null)
			{
				if(v_collider.gameObject.activeSelf && v_collider.gameObject.activeInHierarchy && v_collider.enabled)
					return true;
			}
		}
		return false;
	}

	protected void UpdateKinematicOnPress(bool p_isPressed)
	{
		if(ForceKinematicBeforePressTouch)
		{
			if(p_isPressed)
			{
				UpdateOldKinematic();
				SetKinematic(true);
			}
			else
				SetKinematic(OldKinematic);
		}
	}

	private void SetKinematic(bool p_value)
	{
		Rigidbody2D v_body = GetComponent<Rigidbody2D>();
		if(v_body == null)
			v_body = GetComponentInChildren<Rigidbody2D>();
		if(v_body != null)
			v_body.isKinematic = p_value;
	}

	protected virtual void UpdateOldKinematic()
	{
		Rigidbody2D v_body = GetComponent<Rigidbody2D>();
		if(v_body != null)
			OldKinematic = v_body.isKinematic;
	}

	Vector2 _oldPosition = new Vector2(-1,-1);
	protected void UpdateOldPos()
	{
		if(_oldPosition.x < 0 || _oldPosition.y < 0)
		{
			Camera v_camera = CameraManager.GetCameraThatDrawLayer(this.gameObject.layer);
			if(v_camera != null)
				_oldPosition = v_camera.WorldToScreenPoint(transform.position);
		}
	}

	protected void CheckSend()
	{
		if(CanCallWhenLoadingScene || !SceneManager.IsLoading())
		{
			Camera v_camera = CameraManager.GetCameraThatDrawLayer(this.gameObject.layer);
			if(v_camera != null)
			{
				Vector2 v_newPosition = v_camera.WorldToScreenPoint(transform.position);
				float v_xDistance = v_newPosition.x - _oldPosition.x;
				float v_yDistance = v_newPosition.y - _oldPosition.y;
				int v_maxScreenWidthHeight = Mathf.Max(Screen.width,Screen.height);
				if(Mathf.Abs(v_xDistance) < v_maxScreenWidthHeight * 0.015f && Mathf.Abs(v_yDistance) < v_maxScreenWidthHeight * 0.015f)
					CallSend();
				_oldPosition = new Vector2(-1,-1);
			}
			else
				CallSend();
		}
	}

	protected void CallSend(bool p_ignoreFocus = true)
	{
		if(p_ignoreFocus || FocusContainer.IsUnderFocus(this.gameObject))
		{
			if(CanCallWhenLoadingScene || !SceneManager.IsLoading() && _currentTimeBeetweenSends <= 0)
			{
				_currentTimeBeetweenSends = TimeBeetweenSends;
				CallPauseAction();
				BeforeSend();
				if(OnSendCallback != null)
					OnSendCallback.Invoke();
				Send();
			}
		}
	}

	private void CallPauseAction()
	{
		if(PauseAction == PauseActionEnum.Pause)
			PauseManager.Pause();
		else if(PauseAction == PauseActionEnum.Resume)
			PauseManager.Resume(false);
		else if(PauseAction == PauseActionEnum.ForceResume)
			PauseManager.Resume(true);
	}

	public void FullSend()
	{
		FullSend(false);
	}

	public void FullSend(bool p_checkIfActive)
	{
		if(!p_checkIfActive || (gameObject.activeSelf && gameObject.activeInHierarchy))
		{
			CallSend(true);
		}
	}

	public virtual void Send ()
	{
	}

	protected virtual void BeforeSend()
	{
	}

	#endregion

	#region Helper Classes
	
	[System.Serializable]
	public class AlarmTrigger
	{
		#region Private Variables

		[SerializeField]
		string m_alarmName = "Alarm";
		[SerializeField]
		float m_minTime = 1f;
		[SerializeField]
		float m_maxTime = 2f;
		[SerializeField]
		bool m_ignoreTimeScale = true;
		[SerializeField]
		bool m_loop = true;
		[SerializeField]
		bool m_isCycleAlarm = false; //Will Reduce 1 of the time every update cycle

		float _currentTime = -1;

		#endregion

		#region Public Properties

		public string AlarmName
		{
			get
			{
				return m_alarmName;
			}
			set
			{
				if(m_alarmName == value)
					return;
				m_alarmName = value;
			}
		}

		public float MinTime
		{
			get
			{
				return m_minTime;
			}
			set
			{
				if(m_minTime == value)
					return;
				m_minTime = value;
			}
		}

		public float MaxTime
		{
			get
			{
				return m_maxTime;
			}
			set
			{
				if(m_maxTime == value)
					return;
				m_maxTime = value;
			}
		}

		public bool IgnoreTimeScale
		{
			get
			{
				return m_ignoreTimeScale;
			}
			set
			{
				if(m_ignoreTimeScale == value)
					return;
				m_ignoreTimeScale = value;
			}
		}

		public bool Loop
		{
			get
			{
				return m_loop;
			}
			set
			{
				if(m_loop == value)
					return;
				m_loop = value;
			}
		}

		public bool IsCycleAlarm
		{
			get
			{
				return m_isCycleAlarm;
			}
			set
			{
				if(m_isCycleAlarm == value)
					return;
				m_isCycleAlarm = value;
			}
		}

		public float CurrentTime
		{
			get
			{
				return _currentTime;
			}
			set
			{
				if(_currentTime == value)
					return;
				_currentTime = value;
			}
		}

		#endregion

		#region Constructor

		public AlarmTrigger()
		{
			m_alarmName = "Alarm";
			m_minTime = 1f;
			m_maxTime = 2f;
			m_ignoreTimeScale = true;
			m_loop = true;
		}

		#endregion

		#region Helper Functions

		public bool ApplyTime()
		{
			if(IsActive())
			{
				float v_delta = IgnoreTimeScale? (IsCycleAlarm? 1 : Time.unscaledDeltaTime) : (IsCycleAlarm? (Time.timeScale > 0? 1 : 0) : Time.deltaTime);
				CurrentTime -= v_delta;
				if(IsCycleAlarm)
					CurrentTime = Mathf.Floor(CurrentTime);
				if(CurrentTime <= 0)
				{
					if(Loop)
						StartAlarm();
					return true;
				}
			}
			return false;
		}

		public void StartAlarm()
		{
			CurrentTime = KiltUtils.RandomRange(MinTime, MaxTime);
		}

		public void StopAlarm()
		{
			CurrentTime = -1;
		}

		public bool IsActive()
		{
			return CurrentTime > 0;
		}

		#endregion
	}

	[System.Serializable]
	public class ComboKeys
	{
		#region Private Variables

		[SerializeField]
		KeyCode m_mainKey = KeyCode.None;
		[SerializeField]
		List<KeyCode> m_complementarKeys = new List<KeyCode>();
		
		#endregion
		
		#region Public Properties
		
		public KeyCode MainKey
		{
			get
			{
				return m_mainKey;
			}
			set
			{
				if(m_mainKey == value)
					return;
				m_mainKey = value;
			}
		}
		
		public List<KeyCode> ComplementarKey
		{
			get
			{
				if(m_complementarKeys == null)
					m_complementarKeys = new List<KeyCode>();
				return m_complementarKeys;
			}
			set
			{
				if(m_complementarKeys == value)
					return;
				m_complementarKeys = value;
			}
		}
		
		#endregion
		
		#region Helper Functions
		
		public bool CheckTrigger()
		{
			bool v_canSend = false;
			v_canSend = MainKey != KeyCode.None && Input.GetKeyDown(MainKey);
			if(v_canSend)
			{
				foreach(KeyCode v_key in ComplementarKey)
				{
					if(v_key != KeyCode.None)
					{
						if(!Input.GetKey(v_key))
						{
							v_canSend = false;
							break;
						}
					}
				}
			}
			return v_canSend;
		}
		
		#endregion
	}

	#endregion
}
