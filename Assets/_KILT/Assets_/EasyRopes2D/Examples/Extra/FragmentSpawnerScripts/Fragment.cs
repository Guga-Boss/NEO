using UnityEngine;
using System.Collections;

public class Fragment : MonoBehaviour {
	
	protected virtual void Awake () 
	{
		FragmentSpawner.RegisterFragment(this);
	}
	
	// Update is called once per frame
	protected virtual void OnDestroy () 
	{
		FragmentSpawner.UnregisterFragment(this);
	}
}
