using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class GlobalScheduler : MonoBehaviour {

	#region Non-Instance 

	#region Static Variables/Properties
	
	static AOTDictionaryKV<float, FunctionAndParameters> m_functionsToCallOverTime = new AOTDictionaryKV<float,FunctionAndParameters>();
	
	public static AOTDictionaryKV<float, FunctionAndParameters> FunctionsToCallOverTime
	{
		get
		{
			if(m_functionsToCallOverTime == null)
				m_functionsToCallOverTime = new AOTDictionaryKV<float, FunctionAndParameters>();
			return m_functionsToCallOverTime;
		}
	}
	
	#endregion

	#region Static Constructor
	
	static GlobalScheduler()
	{
		Init();
	}
	
	static System.DateTime _lastCheckedTime = System.DateTime.UtcNow;
	static void Init()
	{
		_lastCheckedTime = System.DateTime.UtcNow;
		#if UNITY_EDITOR
		EditorApplication.update -= EditorUpdate;
		EditorApplication.update += EditorUpdate;
		#endif
	}

	#endregion

	#region Static Receivers

	static void EditorUpdate()
	{ 
		ProcessStaticTime();
	}

	#endregion

	#region Helper Non-Intance Functions

	static void RegisterInCallerOverTime(FunctionAndParameters p_struct, float p_time)
	{
		if(p_struct != null)
		{
			if(p_time <= 0)
				p_struct.CallFunction();
			else
				FunctionsToCallOverTime.AddChecking(p_time, p_struct);
		}

	}

	static void ProcessStaticTime()
	{
		bool v_needClean = false;
		System.DateTime v_utfNow = System.DateTime.UtcNow;
		double v_seconds = (v_utfNow - _lastCheckedTime).TotalSeconds;
		_lastCheckedTime = v_utfNow;
		foreach(AOTKeyValuePair<float, FunctionAndParameters> v_pair in FunctionsToCallOverTime)
		{
			if(v_pair != null && v_pair.Value != null)
			{
				v_pair.Key -= (float)v_seconds;
				if(v_pair.Key  <= 0)
				{
					v_pair.Value.CallFunction();
					v_pair.Value = null;
					v_needClean = true;
				}
			}
			else
				v_needClean = true;
		}
		if(v_needClean)
		{
			FunctionsToCallOverTime.RemoveNulls();
			FunctionsToCallOverTime.RemovePairsWithNullValues();
		}
	}

	#endregion

	#endregion

	#region Instance Methods (Singleton Needed)

	#region Singleton
	
	private static GlobalScheduler m_instance = null;
	
	public static GlobalScheduler Instance
	{
		get
		{
			if( m_instance == null )
			{
				m_instance = GameObject.FindObjectOfType(typeof(GlobalScheduler)) as GlobalScheduler;
				if(m_instance == null && Application.isPlaying)
				{
					GlobalScheduler v_object = new GameObject("GlobalScheduler").AddComponent<GlobalScheduler>();
					v_object.gameObject.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
					KiltUtils.Destroy(v_object.gameObject, true);
					m_instance = v_object;
				}
			}
			
			return m_instance;
		}
		protected set
		{
			m_instance = value;
		}
	}
	
	#endregion

	#region Private Variables

	List<TimeScheduler> m_schedulersList = new List<TimeScheduler>();

	#endregion

	#region Public Variables

	protected List<TimeScheduler> SchedulersList 
	{
		get 
		{
			if(m_schedulersList == null )
			{
				m_schedulersList = new List<TimeScheduler>();
			}
			return m_schedulersList;
		} 
	}

	#endregion

	#region Unity Functions

	protected virtual void OnLevelWasLoaded()
	{
		if(m_instance != this && m_instance != null)
			Object.Destroy(this.gameObject);
		else
		{
			if(m_instance == null)
				Instance = this;
			RemoveUselessSchedulers();
		}
	}

	protected virtual void Update()
	{
		if(!Application.isEditor)
			ProcessStaticTime();
	}

	#endregion

	#region Public Functions

	public T GetSchedulerByValue<T>(GameObject p_objectOwner) where T: TimeScheduler
	{
		List<T> v_returnList = GetSchedulersByValue<T>(p_objectOwner);
		T v_returnScheduler = null;
		if(v_returnList != null && v_returnList.Count > 0)
			v_returnScheduler = v_returnList[0];
		return v_returnScheduler;
	}

	public T GetSchedulerByValueOrRegisterNew<T>(GameObject p_objectOwner) where T: TimeScheduler
	{
		T v_returnScheduler =  GetSchedulerByValue<T>(p_objectOwner);
		if(v_returnScheduler == null)
			v_returnScheduler = RegisterScheduler<T>(p_objectOwner);
		return v_returnScheduler;
	}

	public List<T> GetSchedulersByValue<T>(GameObject p_objectOwner) where T: TimeScheduler
	{
		List<T> v_returnList = new List<T>();
		if(p_objectOwner != null)
		{
			foreach(T v_scheduler in SchedulersList)
			{
				if(v_scheduler != null)
				{
					if(v_scheduler.Owner == p_objectOwner)
						v_returnList.Add(v_scheduler);
				}
			}
		}
		return v_returnList;
	}

	public T RegisterScheduler<T>(GameObject p_object) where T: TimeScheduler
	{
		if(p_object != null)
		{
			T v_scheduler = gameObject.AddComponent<T>();
			v_scheduler.Owner = p_object;
			SchedulersList.Add(v_scheduler);
			return v_scheduler;
		}
		return null;
	}
	
	public void RemoveSchedulersByValue<T>(GameObject p_objectOwner) where T: TimeScheduler
	{
		List<TimeScheduler> v_removeList = new List<TimeScheduler>(GetSchedulersByValue<T>(p_objectOwner).ToArray());
		RemoveSchedulers(v_removeList);
	}

	public void RemoveAllSchedulers<T>() where T: TimeScheduler
	{
		List<TimeScheduler> v_removeList = new List<TimeScheduler>(GetComponents<T>());
		RemoveSchedulers(v_removeList);
	}

	public void RemoveUselessSchedulers()
	{
		List<TimeScheduler> v_schedulersToRemove = FindUselessSchedulers();
		RemoveSchedulers(v_schedulersToRemove);
	}

	private void RemoveSchedulers(List<TimeScheduler> p_removeList)
	{
		foreach(TimeScheduler v_scheduler in p_removeList)
		{
			RemoveScheduler(v_scheduler);
		}
	}

	private void RemoveScheduler(TimeScheduler p_scheduler)
	{
		if(p_scheduler != null)
		{
			if(SchedulersList.Contains(p_scheduler))
				SchedulersList.Remove(p_scheduler);
			Object.Destroy(p_scheduler);
		}
	}

	private List<TimeScheduler> FindUselessSchedulers()
	{
		List<TimeScheduler> v_schedulersToRemove = new List<TimeScheduler>();
		foreach(TimeScheduler v_scheduler in SchedulersList)
		{
			if(v_scheduler != null)
			{
				if(v_scheduler.Owner == this.gameObject || v_scheduler.Owner == null)
				{
					v_schedulersToRemove.Add(v_scheduler);
					continue;
				}
			}
		}
		return v_schedulersToRemove;
	}

	#endregion

	#endregion

	#region Public Static Functions

	#region Instance Helper

	public static bool InstanceExists()
	{
		return GetInstance(false) == null? false : true;
	}

	public static GlobalScheduler GetInstance(bool p_canCreateANewOne = false)
	{
		GlobalScheduler v_instance = null;
		if(p_canCreateANewOne)
			v_instance = Instance;
		else
		{
			if(m_instance == null )
				m_instance = GameObject.FindObjectOfType(typeof(GlobalScheduler)) as GlobalScheduler;
			v_instance = m_instance;
		}
		return v_instance;
	}

	#endregion

	#region Functions Caller

	public static void CallFunctionWhenOutOfMainThread(System.Delegate p_functionPointer, float p_time)
	{
		CallFunctionWhenOutOfMainThread(p_functionPointer, null, p_time);
	}

	public static void CallFunctionWhenOutOfMainThread(System.Delegate p_functionPointer, List<object> p_params, float p_time)
	{
		FunctionAndParameters v_newStruct = new FunctionAndParameters();
		v_newStruct.DelegatePointer = p_functionPointer;
		v_newStruct.Params = p_params;
		v_newStruct.Target = null;
		if(p_time <=0)
			p_time = 0.001f;
		RegisterInCallerOverTime(v_newStruct, p_time);
	}

	public static void CallStaticFunction(System.Delegate p_functionPointer, float p_time, bool p_canCreateNewOne = true)
	{
		CallStaticFunction(p_functionPointer, null, p_time, p_canCreateNewOne);
	}

	public static void CallStaticFunction(System.Delegate p_functionPointer, List<object> p_params, float p_time, bool p_canCreateNewOne = true)
	{
		GlobalScheduler v_instance = GlobalScheduler.GetInstance(p_canCreateNewOne);
		if(v_instance != null)
		{
			FunctionCallerScheduler v_scheduler = v_instance.RegisterScheduler<FunctionCallerScheduler>(v_instance.gameObject);
			if(v_scheduler != null && Application.isPlaying)
			{
				v_scheduler.DestroyOptionWhenStop = WhenStopDestroyEnum.DestroyScript;
				v_scheduler.FunctionToCall = p_functionPointer;
				v_scheduler.Parameters = p_params;
				v_scheduler.MaxTime = p_time;
				v_scheduler.Owner = v_instance.gameObject;
				v_scheduler.StartTimer();
			}
		}
		else
		{
			FunctionAndParameters v_newStruct = new FunctionAndParameters();
			v_newStruct.DelegatePointer = p_functionPointer;
			v_newStruct.Params = p_params;
			v_newStruct.Target = null;
			RegisterInCallerOverTime(v_newStruct, p_time);
		}
	}

	public static void CallFunction(GameObject p_owner, System.Delegate p_functionPointer, float p_time, bool p_canCreateNewOne = true)
	{
		CallFunction(p_owner, p_functionPointer, null, p_time, p_canCreateNewOne);
	}

	public static void CallFunction(GameObject p_owner, System.Delegate p_functionPointer, List<object> p_params, float p_time, bool p_canCreateNewOne = true)
	{
		if(p_owner != null)
		{
			GlobalScheduler v_instance = GlobalScheduler.GetInstance(p_canCreateNewOne);
			if(v_instance != null && Application.isPlaying)
			{
				FunctionCallerScheduler v_scheduler = v_instance.RegisterScheduler<FunctionCallerScheduler>(p_owner);
				if(v_scheduler != null)
				{
					v_scheduler.DestroyOptionWhenStop = WhenStopDestroyEnum.DestroyScript;
					v_scheduler.FunctionToCall = p_functionPointer;
					v_scheduler.Parameters = p_params;
					v_scheduler.MaxTime = p_time;
					v_scheduler.Owner = p_owner;
					v_scheduler.StartTimer();
				}
			}
			else
			{
				FunctionAndParameters v_newStruct = new FunctionAndParameters();
				v_newStruct.DelegatePointer = p_functionPointer;
				v_newStruct.Params = p_params;
				v_newStruct.Target = p_owner;
				RegisterInCallerOverTime(v_newStruct, p_time);
			}
		}
	}

	#endregion

	#endregion
}

#region Helper Classes

[System.Serializable]
public class FunctionAndParameters
{
	#region Private Variables

	[SerializeField]
	object m_target = null;
	[SerializeField]
	System.Type m_functionType = null;
	[SerializeField]
	string m_stringFunctionName = "";
	[SerializeField]
	System.Delegate m_delegatePointer = null;
	[SerializeField]
	List<object> m_params = new List<object>();

	#endregion

	#region Public Properties

	public object Target
	{
		get
		{
			return m_target;
		}
		set
		{
			if(m_target == value)
				return;
			m_target = value;
		}
	}

	public System.Type FunctionType
	{
		get
		{
			return m_functionType;
		}
		set
		{
			if(m_functionType == value)
				return;
			m_functionType = value;
		}
	}

	public string StringFunctionName
	{
		get
		{
			return m_stringFunctionName;
		}
		set
		{
			if(m_stringFunctionName == value)
				return;
			m_stringFunctionName = value;
		}
	}

	public System.Delegate DelegatePointer
	{
		get
		{
			return m_delegatePointer;
		}
		set
		{
			if(m_delegatePointer == value)
				return;
			m_delegatePointer = value;
		}
	}

	public List<object> Params
	{
		get
		{
			if(m_params == null)
				m_params = new List<object>();
			return m_params;
		}
		set
		{
			if(m_params == value)
				return;
			m_params = value;
		}
	}

	#endregion

	#region Helper Methods

	public bool CallFunction()
	{
		if(m_delegatePointer != null)
		{
			return CallDelegateFunction();
		}
		if(string.IsNullOrEmpty(m_stringFunctionName))
		{
			if(m_target != null)
				return KiltUtils.CallFunction(m_target, FunctionType, m_stringFunctionName, Params.ToArray());
			else
				return KiltUtils.CallStaticFunction(FunctionType, m_stringFunctionName, Params.ToArray());
		}
		return false;
	}

	protected bool CallDelegateFunction()
	{
		try
		{
			System.Delegate v_tempFunctionPointer = DelegatePointer;
			object[] v_params = Params.ToArray();
			if(v_tempFunctionPointer != null)
			{
				if(Params.Count == 0)
					v_tempFunctionPointer.DynamicInvoke(null);
				else
					v_tempFunctionPointer.DynamicInvoke(v_params);
				return true;
			}
		}
		catch{}
		return false;
	}

	#endregion
}

#endregion
