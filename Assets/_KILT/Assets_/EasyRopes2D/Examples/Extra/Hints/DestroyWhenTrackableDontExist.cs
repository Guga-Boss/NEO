using UnityEngine;
using System.Collections;

public class DestroyWhenTrackableDontExist : MonoBehaviour {

	#region Private Variables

	[SerializeField]
	GameObject m_trackable = null;

	#endregion

	#region Public Properties

	public GameObject Trackable
	{
		get
		{
			return m_trackable;
		}
		set
		{
			if(m_trackable == value)
				return;
			m_trackable = value;
		}
	}

	#endregion

	#region Unity Functions

	protected virtual void Update()
	{
		if(Trackable == null)
			KiltUtils.Destroy(this.gameObject);
	}

	#endregion
}
