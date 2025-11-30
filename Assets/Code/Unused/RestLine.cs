using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PathologicalGames;
using DigitalRuby.LightningBolt;

public class RestLine : MonoBehaviour 
{
    public static void Create()
    {
        List<Vector2> psl = Map.I.RestPos;
        List<int> restg = Map.I.RestID;
        for( int k = 1; k < restg.Count; k++ )                          // reorder targets by range                                                                            // Sorts score list        
        for( int j = 0; j < restg.Count - k; j++ )
        if ( ( restg[ j ] > restg[ j + 1 ] ) )
        {
            int temp = restg[ j ];
            restg[ j ] = restg[ j + 1 ];
            restg[ j + 1 ] = temp;
            Vector2 tempv = psl[ j ];
            psl[ j ] = psl[ j + 1 ];
            psl[ j + 1 ] = tempv;
        }

        int last = 0;
        for( int g = 0; g <= 7; g++ )
        {
            List<Vector2> gl = new List<Vector2>();
            for( int i = last; i < psl.Count; i++ )                    // create a list for each group     
            {
                if( g == restg[ i ] )
                {
                    gl.Add( psl[ i ] );
                    last++;
                }
            }

            Util.RandomizeList( ref gl );
            for( int i = 0; i < gl.Count - 1; i++ )                                   // creates lines
            {
                Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Resting Line" );
                LightningBoltScript lb = tr.GetComponent<LightningBoltScript>();

                Collider2D col = tr.GetComponent<Collider2D>();
                lb.type = "Rest Line";
                lb.StartPosition = new Vector3( gl[ i ].x, gl[ i ].y, -5 );
                lb.EndPosition = new Vector3( gl[ i + 1 ].x, gl[ i + 1 ].y, -5 );

                if( Map.I.Unit[ ( int ) gl[ i ].x, ( int ) gl[ i ].y ] )
                Map.I.Unit[ ( int ) gl[ i ].x, ( int ) gl[ i ].y ].Control.RestLine = lb;
                else
                {
                    List<Unit> fl = Map.I.GetFUnit( gl[ i ] );
                    if( fl != null ) fl[ 0 ].Control.RestLine = lb;                    
                }

                lb.Target = new Vector3( gl[ i + 1 ].x, gl[ i + 1 ].y, -5 );
                lb.gameObject.SetActive( true );
                lb.Lifetime = 99999;

                Color color = Color.white;                                      // set colors
                switch( g )
                {
                    case 0: color = Color.white; break;
                    case 1: color = Color.yellow; break;
                    case 2: color = Color.red; break;
                    case 3: color = Color.blue; break;
                    case 4: color = Color.green; break;
                    case 5: color = Color.cyan; break;
                    case 6: color = Color.magenta; break;
                    case 7: color = new Color32( 138, 49, 31, 255 ); break;
                }
                lb.lineRenderer.SetColors( color, color );  
            }
        }
    }

    internal static void RemoveIt( Unit un )
    {
        if( un.Control )
        {
            if( un.Control.RestLine )
            if( PoolManager.Pools[ "Pool" ].IsSpawned( un.Control.RestLine.transform ) )         // Destroy line animation
                PoolManager.Pools[ "Pool" ].Despawn( un.Control.RestLine.transform );
            un.Control.RestLine = null;
        }
    }
}
