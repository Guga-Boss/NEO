using UnityEngine;
using System.Collections;
using PathologicalGames;

public class IsFX : MonoBehaviour {

	// Use this for initialization
    float Timer;
	void OnSpawned () {
        Timer = 0;
	}
	
	// Update is called once per frame
	void Update () {
        Timer += Time.deltaTime;
        if( Timer > 1 ) transform.parent = Map.I.PoolingFolder.transform;
        if( Timer > 1.2f ) PoolManager.Pools[ "Pool" ].Despawn( transform );
	}
}
