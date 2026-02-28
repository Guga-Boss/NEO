using UnityEngine;

namespace PathologicalGames
{
    [AddComponentMenu( "Pooling System/Pre Runtime Pool Item" )]
    public class PreRuntimePoolItem: MonoBehaviour
    {
        public string poolName = ""; // Target pool name
        public string prefabName = ""; // Target prefab name 
        public bool despawnOnStart = true; // Despawn flag
        public bool doNotReparent = false; // Reparent flag

        private void Start()
        {
            SpawnPool pool; // Pool reference

            // Fast lookup without throwing exceptions
            if( !PoolManager.Pools.TryGetValue( this.poolName, out pool ) )
            {
                Debug.LogError( $"PreRuntimePoolItem Error: No pool with the name '{this.poolName}' exists!" ); // Error log
                return; // Abort silently
            }

            // Register this pre-existing item into the modern pool system
            pool.Add( this.transform, this.prefabName, this.despawnOnStart, !this.doNotReparent ); // Add to pool
        }
    }
}