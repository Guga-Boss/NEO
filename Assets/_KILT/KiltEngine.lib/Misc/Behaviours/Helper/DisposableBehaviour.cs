using UnityEngine;
using System.Collections;

public class DisposableBehaviour : MonoBehaviour 
{
	#region Events

	//Only Called When "Destroy Component Called"... Not Called When Game Object Destroyed
	protected virtual void OnComponentDestroyed()
	{

	}

	#endregion

	#region Helper Functions

	public void Destroy()
	{
		OnComponentDestroyed();
		Object.DestroyImmediate(this);
	}

	#endregion
}
