using UnityEngine;
using System.Collections;
using PathologicalGames;

public class ShakePlatformAnimation : MonoBehaviour
{
    public tk2dSprite Sprite;
    public float AnimationTime = 0;

    public static void Create( Vector2 position )
    {
        Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Shake Platform Animation" );
        ShakePlatformAnimation an = tr.gameObject.GetComponent<ShakePlatformAnimation>();
        an.transform.localScale = new Vector3( 1, 1, 1 );
        an.transform.position = new Vector3( position.x, position.y, 0 );
        an.AnimationTime = 0;
        an.Sprite.gameObject.transform.localPosition = Vector3.zero;
        float amt = .05f;
        iTween.ShakePosition( an.Sprite.gameObject, new Vector3( amt, amt, 0 ), 2f );
        Color col = Quest.I.CurLevel.Tilemap.ColorChannel.GetColor( ( int ) position.x, ( int ) position.y );
        an.Sprite.color = col;

    }

    // Update is called once per frame
    void Update()
    {
        AnimationTime += Time.deltaTime;
        if( AnimationTime > 2f )
        {
            transform.position = Vector3.zero;
            Sprite.transform.position = Vector3.zero;
            Sprite.transform.localPosition = Vector3.zero;
            AnimationTime = 0;


            PoolManager.Pools[ "Pool" ].Despawn( transform );
        }
    }
}
