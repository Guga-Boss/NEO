using UnityEngine;
using System.Collections;
using DarkTonic.MasterAudio;

public class RoachGlueAnimation : MonoBehaviour 
{
    public Unit Unit;
    public float Factor = 0;
    public bool Shrink = false;
    public static float AnimationTime = .12f;
    public bool Oldstate = false;
	void Start ()
    {
        transform.localScale = new Vector3( 0, 0, 1 );
        Factor = 0;
        Shrink = false;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if( Unit.TileID == ETileType.ROACH )
            UpdateRoach();
        else
            if( Unit.TileID == ETileType.SCARAB )
                UpdateScarab();
	}

    private void UpdateRoach()
    {
        if( Util.IsNeighbor( Unit.Pos, G.Hero.Pos ) ) Shrink = false;
        else Shrink = true;

        if( Shrink == false )
        {
            Factor += Time.deltaTime / AnimationTime;
            if( Unit.Control.SpeedTimeCounter < .01f )
                MasterAudio.PlaySound3DAtVector3( "Roach Glue", transform.position );
        }
        else
            Factor -= Time.deltaTime / AnimationTime;

        Factor = Mathf.Clamp( Factor, 0, 1 );

        float fact = .3f + ( Factor * .7f );
        transform.localScale = new Vector3( fact, fact, 1 );
    }
    private void UpdateScarab()
    {
        Oldstate = Shrink;
        Shrink = false;
        if( Unit.MeleeAttack.SpeedTimeCounter < Unit.MeleeAttack.GetRealtimeSpeedTime() )
            Shrink = true;        

        if( Shrink == false )
        {
            Factor += Time.deltaTime / AnimationTime;
            if( Unit.Control.Resting == false )
            if( Oldstate != Shrink )
                MasterAudio.PlaySound3DAtVector3( "Roach Glue", transform.position );
        }
        else
            Factor -= Time.deltaTime / AnimationTime;

        Factor = Mathf.Clamp( Factor, 0, 1 );

        float fact = .3f + ( Factor * .7f );
        transform.localScale = new Vector3( fact, fact, 1 );
    }
}
