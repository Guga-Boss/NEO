using UnityEngine;
using System.Collections;

public class DestroyObjectOnStart : MonoBehaviour 
{
	#region Private Variables

	[SerializeField]
	GameObject m_objectToDestroy = null;
	
	#endregion
	
	#region Public Properties
	
	public GameObject ObjectToDestroy {get {return m_objectToDestroy;} set{m_objectToDestroy = value;}}

	#endregion
	
	#region Unity Functions
	
	protected virtual void Start()
	{
		DestroyTarget();
	}
	
	#endregion

	#region Helper Functions
	
	public void DestroyTarget()
	{
		if(ObjectToDestroy == null)
			ObjectToDestroy = this.gameObject;
		if(ObjectToDestroy != null)
		{
			ObjectToDestroy.name = ObjectToDestroy.name + "(Destroying)";
			ObjectToDestroy.hideFlags = HideFlags.HideInHierarchy;
			DestroyImmediate(ObjectToDestroy);
		}
	}
	
	#endregion
}
