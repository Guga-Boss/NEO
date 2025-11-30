using UnityEngine;
using System.Collections;
using PathologicalGames;
using DigitalRuby.LightningBolt;

public class SelfDestroy : MonoBehaviour 
{
	public float TargetTime, TimeCount, FadeFactor;
	public bool OnlyDisable;
    public bool DestroyComponent = true;
	public tk2dSprite Spr;
    public LightningBoltScript Line;

	// Use this for initialization
	public void StartIt ( float time, float fade, bool disable )
    {
		TimeCount = 0;
        TargetTime = time;
        FadeFactor = fade;
        OnlyDisable = disable;
	}
	
	// Update is called once per frame
	void Update ()
	{
		TimeCount += Time.deltaTime;

		if( TimeCount > TargetTime )
		{
			if( OnlyDisable ) gameObject.SetActive( false );
			else
            {
                if( DestroyComponent ) Destroy( this );
                PoolManager.Pools[ "Pool" ].Despawn( transform );
            }
			TimeCount = 0;
		}
		if( Spr )
			Spr.color = new Color( Spr.color.r, Spr.color.g, Spr.color.b, Spr.color.a - FadeFactor * Time.deltaTime );

        if( Line )
        {
            Color start = Line.lineRenderer.startColor;
            start = new Color( start.r, start.g, start.b, start.a - FadeFactor * Time.deltaTime );
            Color end = Line.lineRenderer.endColor;
            end = new Color( end.r, end.g, end.b, end.a - FadeFactor * Time.deltaTime );
            Line.lineRenderer.SetColors( start, end );
        }
		//transform.localScale = new Vector3( transform.localScale.x + .9f * Time.deltaTime, transform.localScale.y + .9f * Time.deltaTime, transform.localScale.z );
	}
}
