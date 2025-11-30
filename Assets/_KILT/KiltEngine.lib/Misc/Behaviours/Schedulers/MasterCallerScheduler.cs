using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO Turn this Control More Flexible
// * Need Accept PingPong
// * Need Accept Loop
// * Need Check Other Master Schedulers (Circular Reference)
// * Need Correct PongTime To Call Objects In Correct Order
// * Need To Accept Slave Schedulers with PingPong without break logic
// * Need an InspectorEditor
public class MasterCallerScheduler : TimeScheduler 
{
	#region Private Variables

	List<TimeScheduler> m_schedulersList = new List<TimeScheduler>();

	#endregion

	#region Public Properties

	public List<TimeScheduler> SchedulersList 
	{
		get 
		{ 
			if(m_schedulersList == null)
				m_schedulersList = new List<TimeScheduler>();
			return m_schedulersList;
		}
		protected set 
		{
			m_schedulersList = value;
		}
	}

	#endregion

	#region Unity Functions
	
	protected override void OnEnable()
	{
		SchedulersList = GetAllPossibleSlaveSchedulersInObject();
		UpdateMaxTime();
		base.OnEnable();
	}

	#endregion

	#region Helper Functions

	protected List<TimeScheduler> GetAllPossibleSlaveSchedulersInObject()
	{
		List<TimeScheduler> m_unfilteredList = new List<TimeScheduler>(GetComponents<TimeScheduler>());
		if(m_unfilteredList.Contains(this))
			m_unfilteredList.Remove(this);
		return m_unfilteredList;
	}

	public void UpdateMaxTime()
	{
		float v_finalDuration = 0;
		//Math of All Slave Schedulers Duration
		foreach(TimeScheduler v_scheduler in SchedulersList)
		{
			float v_currentSchedulerDuration = 0;
			if(v_scheduler != null)
			{
				//Dont Accept Loops in slave Schedulers
				if(v_scheduler.Loop)
					v_scheduler.Loop = false;
				float v_initialDelay = v_scheduler.InitialDelay;
				float v_initialPingDelay = v_scheduler.IsPing || v_scheduler.TimerStyle == TimerStyle.PingPong? v_scheduler.PingDelayTime : 0;
				float v_initialPongDelay = !v_scheduler.IsPing || v_scheduler.TimerStyle == TimerStyle.PingPong? v_scheduler.PongDelayTime : 0;
				float v_duration = v_scheduler.TimerStyle == TimerStyle.PingPong? v_scheduler.MaxTime*2 : v_scheduler.MaxTime;
				v_currentSchedulerDuration = v_initialDelay + v_initialPingDelay + v_initialPongDelay + v_duration;
			}
			v_finalDuration = Mathf.Max(v_finalDuration, v_currentSchedulerDuration);
		}
		MaxTime = v_finalDuration;
	}

	/*public void SetIsPingValueInSlaves(bool p_isPing)
	{
		foreach(TimeScheduler v_scheduler in SchedulersList)
			v_scheduler.IsPing = p_isPing;
	}*/

	public void CallAllSlaveSchedulers()
	{
		foreach(TimeScheduler v_scheduler in SchedulersList)
		{
			v_scheduler.IgnoreTimeScale = IgnoreTimeScale;
			v_scheduler.ForceFinishOnDisable = ForceFinishOnDisable;
			v_scheduler.StartTimer(IsPing, true, RestartOption);
		}
	}

	#endregion

	#region Inherited Functions

	protected override void OnPingStart()
	{
		//UpdateMaxTime();
		CallAllSlaveSchedulers();
	}

	protected override void OnPongStart()
	{
		//UpdateMaxTime();
		CallAllSlaveSchedulers();
	}

	#endregion
}
