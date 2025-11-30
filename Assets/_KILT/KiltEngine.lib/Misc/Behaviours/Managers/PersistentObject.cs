using UnityEngine;
using System.Collections;

public class PersistentObject : MonoBehaviour {

	protected virtual void Awake () {
		DontDestroyOnLoad(gameObject);
	}
}
