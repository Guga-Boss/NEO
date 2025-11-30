using UnityEngine;
using System.Collections;
using PathologicalGames;

public class FallingAnimation : MonoBehaviour
{    
    public tk2dSprite Sprite;
    public float Size = 1;
    public float Speed = .5f;
    public float SpeedIncrease = .5f;
    public float MaxRotation = 45;
    public float RotationDelta = 10;
    public float RotationSpeed = 1;
    
    public void OnSpawn()
    {
        Size = 1;
        Speed = .3f;
        SpeedIncrease = .3f;
        RotationDelta = .5f;
        RotationSpeed = 1;
        MaxRotation = 37;
    }
    public static void Create( ETileType tile, Vector3 position, Vector3 rot )
    {
        CreateAnim( ( int ) tile, 0, position, rot );
    }

    public static void Create( int tile, int col, Vector3 position, Vector3 rot, float spd = .3f )
    {
        CreateAnim( tile, col, position, rot, spd );
    }

    public static void CreateAnim( int tile, int col, Vector3 position, Vector3 rot, float spd = .3f, float initialSize = .6f )
    {
        Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Falling Animation" );
        FallingAnimation an = tr.gameObject.GetComponent<FallingAnimation>();
        an.OnSpawn();
        an.Speed = spd;
        an.Sprite.spriteId = ( int ) tile;
        an.Sprite.SetSprite( Map.I.SpriteCollectionList[ col ], tile );
        an.transform.localScale = new Vector3( 1, 1, 1 );
        an.transform.position = new Vector3( position.x, position.y, position.z );
        an.transform.eulerAngles = rot;
        an.Size = initialSize;
        if( ( ETileType ) tile == ETileType.BOULDER ) an.Size = 1.1f;
        an.RotationSpeed = 1 + Random.Range( -an.RotationDelta, an.RotationDelta );
        if( Util.Chance( 50 ) ) an.RotationSpeed *= -1;
    }

    void Update()
    {
        Speed += SpeedIncrease * Time.deltaTime;
        Size -= Speed * Time.deltaTime;
        transform.localScale = new Vector3( Size, Size, 1 );
        transform.Rotate( 0, 0, MaxRotation * Time.deltaTime * RotationSpeed );
        if( Size < 0 )
            PoolManager.Pools[ "Pool" ].Despawn( transform );
    }
}
