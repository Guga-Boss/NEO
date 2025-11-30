using UnityEngine;
using System.Collections;

public class PhysicsStabilizerScheduler : TimeScheduler {

	#region Constructor

	public PhysicsStabilizerScheduler()
	{
		MaxTime = 0.1f;
		CancelOnClick = false;
		ForceFinishOnDisable = true;
		IgnoreTimeScale = false;
		OnStop += HandleOnStop;
		OnFirstStart += HandleOnFirstStart;
	}

	#endregion

	#region Event Receivers
	
	protected virtual void HandleOnFirstStart (CycleEventArgs e)
	{
		Block.AddInGlobalPressCount();
	}

	protected virtual void HandleOnStop (CycleEventArgs e)
	{
		ResetAllRigidBodyValues();
		Block.SubInGlobalPressCount();
	}

	#endregion

	#region Helper Functions

	protected virtual void ResetAllRigidBodyValues()
	{
		Rigidbody2D[] v_allRigidInScene = KiltUtils.FindAllComponentsOfType<Rigidbody2D>(false);
		foreach(Rigidbody2D v_body in v_allRigidInScene)
		{
			KiltUtils.ClearRigidBody2D(v_body);
		}
	}

	#endregion
}
