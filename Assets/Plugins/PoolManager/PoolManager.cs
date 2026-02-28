using UnityEngine;
using System.Collections.Generic;

namespace PathologicalGames
{
    public static class PoolManager
    {
        public static readonly SpawnPoolsDict Pools = new SpawnPoolsDict(); // Global dictionary for all pools
    }

    public static class PoolManagerUtils
    {
        internal static void SetActive( GameObject obj, bool state )
        {
            obj.SetActive( state ); // Modern Unity direct call, legacy #if blocks removed
        }
    }

    public class SpawnPoolsDict: Dictionary<string, SpawnPool>
    {
        public override string ToString()
        {
            var keysArray = new string[this.Count]; // Name array
            this.Keys.CopyTo( keysArray, 0 ); // Copy keys
            return string.Format( "[{0}]", string.Join( ", ", keysArray ) ); // Return formatted
        }

        public void DestroyAll()
        {
            foreach( KeyValuePair<string, SpawnPool> pair in this )
            {
                Object.Destroy( pair.Value.gameObject ); // Destroy each pool safely
            }
            this.Clear(); // Clear dictionary
        }

        // REFACTORED: Safe getter that prevents fatal crashes during scene transitions
        public new SpawnPool this[ string key ]
        {
            get
            {
                if( this.TryGetValue( key, out SpawnPool pool ) ) return pool; // Return if found

                Debug.LogWarning( $"PoolManager: Pool '{key}' not found. Returning null instead of crashing." ); // Warn instead of crash
                return null; // Safe fallback
            }
            set
            {
                base[ key ] = value; // Standard setter
            }
        }

        public void Add( SpawnPool pool )
        {
            if( !this.ContainsKey( pool.poolName ) )
            {
                this.Add( pool.poolName, pool ); // Add pool if missing
            }
        }

        public bool Remove( SpawnPool pool )
        {
            if( this.ContainsKey( pool.poolName ) )
            {
                return this.Remove( pool.poolName ); // Remove pool safely
            }
            return false; // Not found
        }
    }
}