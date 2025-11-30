using UnityEngine;
using System.Collections;

public class MainBackScroller : MonoBehaviour {

    public Vector3 Velocity;
	// Use this for initialization
	public void Start () {

        transform.localPosition = new Vector3( -19.04f, 3.500381f, transform.position.z );
	}
	
	// Update is called once per frame
    //void Update ()
    //{
    //    if( Velocity.x > 0 )
    //        if( transform.localPosition.x > 5 )
    //        {
    //            Velocity *= -1;
    //            MainMenu.I.ChangeBackground = true;
    //        }
    //    if( Velocity.x < 0 )
    //        if( transform.localPosition.x < -19.04 )
    //        {
    //            Velocity *= -1;
    //            MainMenu.I.ChangeBackground = true;
    //        }
    //    transform.localPosition += Velocity * Time.deltaTime;
    //}
}
