using UnityEngine;
using System.Collections;

public class BlinkEffect : MonoBehaviour
{
	public tk2dSprite Spr;
	public Unit Unit;
	public float Factor = 1;
    public float Speed = 0.1f;
	public float Base = 0.7f;
	public float Signal = 1;

	// Update is called once per frame
	void FixedUpdate()
	{
		if( Map.I == null ) return;
		if( Spr == null ) Debug.LogError( "Sprite not Set" );

		if( Map.I.Revealed[ ( int ) Unit.Pos.x, ( int ) Unit.Pos.y ] == false )
		{
			Spr.color = Color.black;
			return;
		}

		Spr.color = new Color( Factor, Factor, Factor, 1 );

		Factor += ( Speed * Time.deltaTime ) * Signal;
		if( Factor > 1    ) Signal = -1; else
        if( Factor < Base ) Signal = 1;
	}
}
