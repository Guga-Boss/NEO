using UnityEngine;
using System.Collections;

public class AutoRotateZ : MonoBehaviour {

    public float RotationSpeed = 30;
	
	// Update is called once per frame
	void Update () 
    {
        transform.Rotate( new Vector3( 0, 0, RotationSpeed * Time.deltaTime ) );
	}
}
